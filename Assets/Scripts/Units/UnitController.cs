using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Core;
using Gorkha.Graphics;
using Gorkha.Economy;
using Gorkha.Buildings;
using Gorkha.Audio;
using Gorkha.UI;

namespace Gorkha.Units
{
    public enum UnitType
    {
        Villager,
        Spearman,
        KhukuriWarrior,
        Archer
    }

    public enum VillagerWorkState
    {
        IdlePatrol,
        MovingToNode,
        Harvesting,
        DeliveringToDropOff,
        WorkingFarm
    }

    /// <summary>
    /// Controls ambient civilian villagers and military units (Age of Empires & Clash of Clans style).
    /// Features:
    /// - Villager Work Loop: Chops trees, mines slate boulders, carries resource bundles to Town Center / Camps.
    /// - Military Patrol: Marches and forms defensive positions.
    /// - Responsive RTS pathing, directional facing, and work animations.
    /// </summary>
    [ExecuteAlways]
    public class UnitController : MonoBehaviour
    {
        public static readonly List<UnitController> AllUnits = new();

        [Header("Unit Configuration")]
        [SerializeField] private UnitType _unitType = UnitType.Villager;
        [SerializeField] private float _moveSpeed = 3.2f;
        [SerializeField] private float _rotationSpeed = 9.0f;
        [SerializeField] private float _idleDuration = 3.0f;

        [Header("Villager Gathering (AoE)")]
        [SerializeField] private VillagerWorkState _workState = VillagerWorkState.IdlePatrol;
        [SerializeField] private ResourceNode _targetNode;
        [SerializeField] private BuildingInstance _targetFarm;
        [SerializeField] private double _carriedAmount = 0.0;
        [SerializeField] private double _carryCapacity = 15.0;
        [SerializeField] private ResourceType _carriedType = ResourceType.Wood;

        [Header("Combat & Health")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth = 100;
        [SerializeField] private bool _isHostile = false;
        [SerializeField] private int _bountyGold = 25;

        [Header("RTS Selection & Orders")]
        [SerializeField] private bool _isSelected = false;
        [SerializeField] private bool _hasManualOrder = false;
        private GameObject _selectionRing;
        private Vector3 _manualDestination;
        private UnitController _manualTargetUnit;

        private Vector3 _targetPosition;
        private Coroutine _behaviorRoutine;
        private UnitProceduralAnimator _animator;
        private WorldCombatHealthBar _healthBar;

        public UnitType Type => _unitType;
        public VillagerWorkState WorkState => _workState;
        public bool IsVillager => _unitType == UnitType.Villager;
        public bool IsIdle => _workState == VillagerWorkState.IdlePatrol;
        public int Health => _currentHealth;
        public int MaxHealth => _maxHealth;
        public bool IsHostile => _isHostile;
        public bool IsAlive => _currentHealth > 0;
        public bool IsSelected => _isSelected;
        public bool HasManualOrder => _hasManualOrder;
        public UnitProceduralAnimator Animator => _animator;

        private void OnEnable()
        {
            if (!AllUnits.Contains(this)) AllUnits.Add(this);
            EnsureVisuals();
        }

        private void OnDisable()
        {
            AllUnits.Remove(this);
        }

        private void Awake()
        {
            EnsureVisuals();
        }

        private void Start()
        {
            EnsureVisuals();

            if (Application.isPlaying)
            {
                if (_behaviorRoutine != null) StopCoroutine(_behaviorRoutine);
                _behaviorRoutine = StartCoroutine(MainBehaviorRoutine());
            }
        }

        public void SetUnitType(UnitType type)
        {
            _unitType = type;
            EnsureVisuals(forceRebuild: true);
        }

        public void SetHostile(bool hostile)
        {
            _isHostile = hostile;
            _maxHealth = hostile ? 120 : 100;
            _currentHealth = _maxHealth;
            _moveSpeed = hostile ? 2.8f : 3.2f;
            EnsureVisuals(forceRebuild: true);
        }

        private void EnsureVisuals(bool forceRebuild = false)
        {
            string expectedName = _isHostile ? $"HostileBandit_{_unitType}" : $"Gorkha_{_unitType}";
            Transform currentVisual = transform.Find(expectedName);

            // Automatically upgrade legacy procedural block units if high-fidelity Synty model is available
            if (!forceRebuild && currentVisual != null)
            {
                bool hasSyntyAssets = FantasyKingdomAssetRegistry.LoadCharacterPrefab() != null;
                bool isUsingSynty = currentVisual.Find("VisualModel/Synty_Character") != null || currentVisual.GetComponentInChildren<SkinnedMeshRenderer>() != null;
                if (hasSyntyAssets && !isUsingSynty)
                {
                    forceRebuild = true;
                }
            }

            if (forceRebuild || currentVisual == null || transform.childCount == 0)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    var child = transform.GetChild(i);
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }

                currentVisual = Nepali3DMeshBuilder.BuildUnit(transform, _unitType, _isHostile)?.transform;
            }

            // Guarantee authentic Mixamo textured materials are bound to the character renderers
            if (currentVisual != null)
            {
                var charTransform = currentVisual.Find("VisualModel/Synty_Character");
                if (charTransform != null)
                {
                    Nepali3DMeshBuilder.ApplyMixamoMaterials(charTransform.gameObject, _unitType);
                }
            }

            _animator = GetComponentInChildren<UnitProceduralAnimator>();
            if (_animator != null)
            {
                _animator.SetUnitClass(_unitType, _isHostile);
            }

            // Setup World-Space Health Bar
            if (_healthBar == null)
            {
                _healthBar = GetComponent<WorldCombatHealthBar>();
                if (_healthBar == null)
                {
                    _healthBar = gameObject.AddComponent<WorldCombatHealthBar>();
                }
            }
            _healthBar.Initialize(_maxHealth, new Vector3(0, 1.85f, 0), isBuilding: false);

            // Setup Selection Disc Visual
            if (_selectionRing == null)
            {
                Transform existingRing = transform.Find("SelectionRing");
                if (existingRing != null)
                {
                    _selectionRing = existingRing.gameObject;
                }
                else
                {
                    _selectionRing = Nepali3DMeshBuilder.CreateSubMesh("SelectionRing", Nepali3DMeshBuilder.CreateCylinderMesh(0.48f, 0.02f, 16), GorkhaMaterials.SelectionGlow, transform, new Vector3(0, 0.02f, 0));
                    var ringCol = _selectionRing.GetComponent<Collider>();
                    if (ringCol != null)
                    {
                        if (Application.isPlaying) Destroy(ringCol);
                        else DestroyImmediate(ringCol);
                    }
                }
            }
            _selectionRing.SetActive(_isSelected);

            UpdateSackVisual();
        }

        public void SetSelected(bool selected)
        {
            bool wasSelected = _isSelected;
            _isSelected = selected;
            if (_selectionRing != null)
            {
                _selectionRing.SetActive(selected);
            }
            if (selected && !wasSelected)
            {
                GorkhaVoiceBarks.PlaySelectBark(this);
            }
        }

        public void UpdateSackVisual()
        {
            Transform sack = null;
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "DokoPayload" || t.name == "ResourceSack")
                {
                    sack = t;
                    break;
                }
            }

            if (sack != null)
            {
                bool hasCargo = _carriedAmount > 0.5;
                sack.gameObject.SetActive(hasCargo);

                if (hasCargo)
                {
                    Transform wood = sack.Find("Payload_Wood");
                    Transform stone = sack.Find("Payload_Stone");
                    Transform food = sack.Find("Payload_Food");

                    if (wood != null) wood.gameObject.SetActive(_carriedType == ResourceType.Wood);
                    if (stone != null) stone.gameObject.SetActive(_carriedType == ResourceType.Stone);
                    if (food != null) food.gameObject.SetActive(_carriedType == ResourceType.Food);
                }
            }

            UpdateToolVisual();
        }

        public void UpdateToolVisual()
        {
            if (!IsVillager) return;

            Transform toolWood = null;
            Transform toolStone = null;
            Transform toolFood = null;

            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Tool_Wood") toolWood = t;
                else if (t.name == "Tool_Stone") toolStone = t;
                else if (t.name == "Tool_Food") toolFood = t;
            }

            bool isStone = _carriedType == ResourceType.Stone;
            bool isFood = _carriedType == ResourceType.Food;
            bool isWood = !isStone && !isFood;

            if (toolWood != null) toolWood.gameObject.SetActive(isWood);
            if (toolStone != null) toolStone.gameObject.SetActive(isStone);
            if (toolFood != null) toolFood.gameObject.SetActive(isFood);
        }

        /// <summary>
        /// Authentic Rock-Paper-Scissors Combat Triangle (CBT-03):
        /// - Spearmen counter Khukuri Warriors (+35% damage, "CRIT! (Spear Wall)")
        /// - Khukuri Warriors flank and shred Archers (+50% damage, "CRIT! (Flank Slash)")
        /// - Archers counter Spearmen (+40% damage, "CRIT! (Ranged Advantage)")
        /// - Disadvantaged pairings deal 0.75x damage ("Deflected!", "Glancing!", "Outmaneuvered!")
        /// </summary>
        public static int CalculateCounterDamage(UnitType attacker, UnitType defender, int baseDamage, out string feedback, out Color feedbackColor)
        {
            float mult = 1.0f;
            feedback = null;
            feedbackColor = Color.white;

            if (attacker == UnitType.Spearman && defender == UnitType.KhukuriWarrior)
            {
                mult = 1.35f;
                feedback = "CRIT! (Spear Wall)";
                feedbackColor = new Color(0.3f, 0.9f, 1f); // Cyan
            }
            else if (attacker == UnitType.KhukuriWarrior && defender == UnitType.Archer)
            {
                mult = 1.50f;
                feedback = "CRIT! (Flank Slash)";
                feedbackColor = new Color(1f, 0.45f, 0.15f); // Orange-Red
            }
            else if (attacker == UnitType.Archer && defender == UnitType.Spearman)
            {
                mult = 1.40f;
                feedback = "CRIT! (Ranged Advantage)";
                feedbackColor = new Color(0.35f, 1f, 0.35f); // Lime
            }
            // Disadvantaged
            else if (attacker == UnitType.KhukuriWarrior && defender == UnitType.Spearman)
            {
                mult = 0.75f;
                feedback = "Deflected!";
                feedbackColor = new Color(0.85f, 0.85f, 0.85f);
            }
            else if (attacker == UnitType.Archer && defender == UnitType.KhukuriWarrior)
            {
                mult = 0.75f;
                feedback = "Glancing!";
                feedbackColor = new Color(0.85f, 0.85f, 0.85f);
            }
            else if (attacker == UnitType.Spearman && defender == UnitType.Archer)
            {
                mult = 0.80f;
                feedback = "Outmaneuvered!";
                feedbackColor = new Color(0.85f, 0.85f, 0.85f);
            }

            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * mult));
        }

        public void TakeDamageFrom(UnitController attacker, int baseDamage)
        {
            if (!IsAlive) return;

            if (attacker != null)
            {
                int finalDamage = CalculateCounterDamage(attacker.Type, this.Type, baseDamage, out string feedback, out Color feedbackColor);
                TakeDamage(finalDamage, feedback, feedbackColor);
            }
            else
            {
                TakeDamage(baseDamage);
            }
        }

        public void TakeDamage(int damage)
        {
            TakeDamage(damage, null, null);
        }

        public void TakeDamage(int damage, string feedback, Color? feedbackColor)
        {
            if (!IsAlive) return;

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            _healthBar?.UpdateHealth(_currentHealth, _maxHealth);
            _animator?.PlayHitFlinch();

            GorkhaAudio.PlayHarvest();
            GorkhaVFX.SpawnPlacementDust(transform.position, Vector2.one * 0.45f);

            if (!string.IsNullOrEmpty(feedback))
            {
                GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 2.3f, feedback, feedbackColor ?? Color.yellow);
                GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 1.7f, $"-{damage}", Color.red);
            }
            else
            {
                GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 1.8f, $"-{damage}", Color.red);
            }

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private bool _isDying = false;

        private void Die()
        {
            if (_isDying) return;
            _isDying = true;
            _currentHealth = 0;
            AllUnits.Remove(this);

            if (_isHostile)
            {
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.AddResource(ResourceType.Gold, _bountyGold);
                }
                GorkhaAudio.PlayUpgrade();
                GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 2.2f, $"+{_bountyGold} Gold Bounty!", Color.yellow);
            }

            if (_selectionRing != null) Destroy(_selectionRing);
            if (_healthBar != null) Destroy(_healthBar.gameObject);
            foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;

            if (_behaviorRoutine != null)
            {
                StopCoroutine(_behaviorRoutine);
                _behaviorRoutine = null;
            }

            GorkhaVFX.SpawnPlacementDust(transform.position, Vector2.one * 0.9f);

            if (_animator != null)
            {
                _animator.PlayDeathCollapse(() => {
                    Destroy(gameObject);
                });
            }
            else
            {
                Destroy(gameObject, 0.5f);
            }
        }

        public static UnitController FindNearestHostileTarget(Vector3 origin, float maxRange)
        {
            UnitController best = null;
            float bestDistSq = maxRange * maxRange;

            for (int i = 0; i < AllUnits.Count; i++)
            {
                var u = AllUnits[i];
                if (u != null && u.IsHostile && u.IsAlive)
                {
                    float dSq = (u.transform.position - origin).sqrMagnitude;
                    if (dSq <= bestDistSq)
                    {
                        bestDistSq = dSq;
                        best = u;
                    }
                }
            }

            return best;
        }

        public static UnitController FindNearestDefender(Vector3 origin, float maxRange)
        {
            UnitController best = null;
            float bestDistSq = maxRange * maxRange;

            for (int i = 0; i < AllUnits.Count; i++)
            {
                var u = AllUnits[i];
                if (u != null && !u.IsHostile && u.IsAlive)
                {
                    float dSq = (u.transform.position - origin).sqrMagnitude;
                    if (dSq <= bestDistSq)
                    {
                        bestDistSq = dSq;
                        best = u;
                    }
                }
            }

            return best;
        }

        public static UnitController FindNearestIdleVillager(Vector3 worldPos)
        {
            UnitController best = null;
            float bestDistSq = float.MaxValue;

            foreach (var u in AllUnits)
            {
                if (u != null && u.IsVillager && u.IsIdle && !u.IsHostile)
                {
                    float dSq = (u.transform.position - worldPos).sqrMagnitude;
                    if (dSq < bestDistSq)
                    {
                        bestDistSq = dSq;
                        best = u;
                    }
                }
            }

            return best;
        }

        public static ResourceNode FindNearestNodeOfType(Vector3 origin, ResourceType yieldType, float maxRange)
        {
            ResourceNode best = null;
            float bestDistSq = maxRange * maxRange;

            for (int i = 0; i < ResourceNode.AllNodes.Count; i++)
            {
                var n = ResourceNode.AllNodes[i];
                if (n != null && !n.IsDepleted && n.YieldType == yieldType)
                {
                    float dSq = (n.transform.position - origin).sqrMagnitude;
                    if (dSq < bestDistSq)
                    {
                        bestDistSq = dSq;
                        best = n;
                    }
                }
            }

            return best;
        }

        public void AssignHarvestTask(ResourceNode node)
        {
            if (node == null || node.IsDepleted || _isHostile) return;

            _hasManualOrder = false;
            _manualTargetUnit = null;
            _targetFarm = null;
            _targetNode = node;
            _workState = VillagerWorkState.MovingToNode;
            _carriedType = node.YieldType;
            UpdateSackVisual();
            GorkhaVoiceBarks.PlayHarvestOrderBark(this, node.YieldType);
        }

        public void AssignFarmTask(BuildingInstance farm)
        {
            if (farm == null || farm.State != BuildingState.Active || _isHostile) return;

            _hasManualOrder = false;
            _manualTargetUnit = null;
            _targetNode = null;
            _targetFarm = farm;
            _workState = VillagerWorkState.WorkingFarm;
            _carriedType = ResourceType.Food;
            UpdateSackVisual();
            GorkhaVoiceBarks.PlayHarvestOrderBark(this, ResourceType.Food);
        }

        public void OrderMove(Vector3 destination)
        {
            _hasManualOrder = true;
            _manualDestination = destination;
            _manualTargetUnit = null;
            _targetNode = null;
            _targetFarm = null;
            _workState = VillagerWorkState.IdlePatrol;
            UpdateSackVisual();
            GorkhaVoiceBarks.PlayMoveOrderBark(this, destination);
        }

        public void OrderAttack(UnitController target)
        {
            if (target == null || !target.IsAlive) return;
            _hasManualOrder = true;
            _manualTargetUnit = target;
            _manualDestination = target.transform.position;
            _targetNode = null;
            _workState = VillagerWorkState.IdlePatrol;
            UpdateSackVisual();
            GorkhaVoiceBarks.PlayAttackOrderBark(this);
        }

        public void ClearManualOrder()
        {
            _hasManualOrder = false;
            _manualTargetUnit = null;
        }

        public void MoveDirect(Vector3 worldPos)
        {
            OrderMove(worldPos);
        }

        private IEnumerator MainBehaviorRoutine()
        {
            while (Application.isPlaying && IsAlive)
            {
                if (_hasManualOrder)
                {
                    yield return HandleManualOrderBehavior();
                }
                else if (_isHostile)
                {
                    yield return HandleHostileRaidBehavior();
                }
                else if (_unitType == UnitType.Villager && (_targetNode != null || _targetFarm != null))
                {
                    yield return HandleVillagerWorkLoop();
                }
                else if (_unitType != UnitType.Villager)
                {
                    yield return HandleMilitaryDefenseBehavior();
                }
                else
                {
                    yield return HandleAmbientPatrol();
                }
            }
        }

        private IEnumerator HandleManualOrderBehavior()
        {
            // 1. Attack Order against specific hostile unit
            if (_manualTargetUnit != null)
            {
                if (!_manualTargetUnit.IsAlive)
                {
                    _manualTargetUnit = null;
                    _hasManualOrder = false;
                    yield break;
                }

                float attackRange = _unitType == UnitType.Archer ? 7.5f : (_unitType == UnitType.Spearman ? 1.9f : 1.35f);
                float dist = Vector3.Distance(transform.position, _manualTargetUnit.transform.position);

                if (dist > attackRange)
                {
                    MoveTowardPoint(_manualTargetUnit.transform.position, attackRange * 0.95f);
                    yield return null;
                }
                else
                {
                    Vector3 bPos = _manualTargetUnit.transform.position;
                    transform.LookAt(new Vector3(bPos.x, transform.position.y, bPos.z));

                    if (_unitType == UnitType.Archer)
                    {
                        if (_animator != null)
                        {
                            _animator.PlayBowRelease(() => {
                                if (_manualTargetUnit != null && _manualTargetUnit.IsAlive)
                                {
                                    BallisticProjectile.LaunchArcherArrow(transform.position + Vector3.up * 1.1f, _manualTargetUnit, damage: 22, attacker: this);
                                }
                            });
                            yield return new WaitForSeconds(1.15f);
                        }
                        else
                        {
                            BallisticProjectile.LaunchArcherArrow(transform.position + Vector3.up * 1.1f, _manualTargetUnit, damage: 22, attacker: this);
                            yield return new WaitForSeconds(1.15f);
                        }
                    }
                    else if (_unitType == UnitType.Spearman)
                    {
                        if (_animator != null)
                        {
                            _animator.PlaySpearLunge(() => {
                                if (_manualTargetUnit != null && _manualTargetUnit.IsAlive)
                                {
                                    _manualTargetUnit.TakeDamageFrom(this, 25);
                                }
                            });
                            yield return new WaitForSeconds(0.85f);
                        }
                        else
                        {
                            _manualTargetUnit.TakeDamageFrom(this, 25);
                            yield return new WaitForSeconds(0.85f);
                        }
                    }
                    else
                    {
                        int dmg = _unitType == UnitType.KhukuriWarrior ? 20 : 10;
                        if (_animator != null)
                        {
                            _animator.PlayKhukuriSlash(() => {
                                if (_manualTargetUnit != null && _manualTargetUnit.IsAlive)
                                {
                                    _manualTargetUnit.TakeDamageFrom(this, dmg);
                                }
                            });
                            yield return new WaitForSeconds(0.70f);
                        }
                        else
                        {
                            _manualTargetUnit.TakeDamageFrom(this, dmg);
                            yield return new WaitForSeconds(0.70f);
                        }
                    }
                }
            }
            // 2. Move Order to ground slot
            else
            {
                float dist = Vector3.Distance(transform.position, _manualDestination);
                if (dist > 0.35f)
                {
                    MoveTowardPoint(_manualDestination, 0.30f);
                    yield return null;
                }
                else
                {
                    _hasManualOrder = false;
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        private IEnumerator HandleMilitaryDefenseBehavior()
        {
            // 1. Scan for nearest hostile bandit within alert range (15m)
            UnitController bandit = FindNearestHostileTarget(transform.position, 15.0f);

            if (bandit != null && bandit.IsAlive)
            {
                GorkhaVoiceBarks.PlayAttackOrderBark(this);

                switch (_unitType)
                {
                    case UnitType.Archer:
                    {
                        float fireRange = 7.5f;
                        while (bandit != null && bandit.IsAlive && Vector3.Distance(transform.position, bandit.transform.position) > fireRange && IsAlive)
                        {
                            MoveTowardPoint(bandit.transform.position, fireRange * 0.95f);
                            yield return null;
                        }

                        if (bandit != null && bandit.IsAlive && IsAlive)
                        {
                            Vector3 bPos = bandit.transform.position;
                            transform.LookAt(new Vector3(bPos.x, transform.position.y, bPos.z));

                            if (_animator != null)
                            {
                                _animator.PlayBowRelease(() => {
                                    if (bandit != null && bandit.IsAlive)
                                    {
                                        BallisticProjectile.LaunchArcherArrow(transform.position + Vector3.up * 1.1f, bandit, damage: 22, attacker: this);
                                    }
                                });
                                yield return new WaitForSeconds(1.15f);
                            }
                            else
                            {
                                BallisticProjectile.LaunchArcherArrow(transform.position + Vector3.up * 1.1f, bandit, damage: 22, attacker: this);
                                yield return new WaitForSeconds(1.15f);
                            }
                        }
                        break;
                    }

                    case UnitType.Spearman:
                    {
                        float spearRange = 1.9f;
                        while (bandit != null && bandit.IsAlive && Vector3.Distance(transform.position, bandit.transform.position) > spearRange && IsAlive)
                        {
                            MoveTowardPoint(bandit.transform.position, spearRange * 0.95f);
                            yield return null;
                        }

                        if (bandit != null && bandit.IsAlive && IsAlive)
                        {
                            Vector3 bPos = bandit.transform.position;
                            transform.LookAt(new Vector3(bPos.x, transform.position.y, bPos.z));

                            if (_animator != null)
                            {
                                _animator.PlaySpearLunge(() => {
                                    if (bandit != null && bandit.IsAlive)
                                    {
                                        bandit.TakeDamageFrom(this, 25);
                                    }
                                });
                                yield return new WaitForSeconds(0.85f);
                            }
                            else
                            {
                                bandit.TakeDamageFrom(this, 25);
                                yield return new WaitForSeconds(0.85f);
                            }
                        }
                        break;
                    }

                    case UnitType.KhukuriWarrior:
                    default:
                    {
                        float meleeRange = 1.35f;
                        while (bandit != null && bandit.IsAlive && Vector3.Distance(transform.position, bandit.transform.position) > meleeRange && IsAlive)
                        {
                            MoveTowardPoint(bandit.transform.position, meleeRange * 0.95f);
                            yield return null;
                        }

                        if (bandit != null && bandit.IsAlive && IsAlive)
                        {
                            Vector3 bPos = bandit.transform.position;
                            transform.LookAt(new Vector3(bPos.x, transform.position.y, bPos.z));

                            if (_animator != null)
                            {
                                _animator.PlayKhukuriSlash(() => {
                                    if (bandit != null && bandit.IsAlive)
                                    {
                                        bandit.TakeDamageFrom(this, 20);
                                    }
                                });
                                yield return new WaitForSeconds(0.70f);
                            }
                            else
                            {
                                bandit.TakeDamageFrom(this, 20);
                                yield return new WaitForSeconds(0.70f);
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                // No bandits in range: patrol near village center or garrison
                yield return HandleAmbientPatrol();
            }
        }

        private IEnumerator HandleHostileRaidBehavior()
        {
            // 1. Check if a defending military unit is nearby (within 5.5m)
            UnitController defender = FindNearestDefender(transform.position, 5.5f);
            if (defender != null && defender.IsAlive)
            {
                float attackRange = 1.35f;
                while (defender != null && defender.IsAlive && Vector3.Distance(transform.position, defender.transform.position) > attackRange && IsAlive)
                {
                    MoveTowardPoint(defender.transform.position, attackRange * 0.9f);
                    yield return null;
                }

                if (defender != null && defender.IsAlive && IsAlive)
                {
                    Vector3 dPos = defender.transform.position;
                    transform.LookAt(new Vector3(dPos.x, transform.position.y, dPos.z));

                    if (_animator != null)
                    {
                        _animator.PlayKhukuriSlash(() => {
                            if (defender != null && defender.IsAlive)
                            {
                                defender.TakeDamageFrom(this, 15);
                            }
                        });
                        yield return new WaitForSeconds(0.85f);
                    }
                    else
                    {
                        defender.TakeDamageFrom(this, 15);
                        yield return new WaitForSeconds(0.85f);
                    }
                }
                yield break;
            }

            // 2. Otherwise attack nearest active kingdom building
            BuildingInstance targetBuilding = null;
            float bestDistSq = float.MaxValue;

            foreach (var b in BuildingInstance.AllBuildings)
            {
                if (b == null || b.State != BuildingState.Active) continue;
                float dSq = (b.transform.position - transform.position).sqrMagnitude;
                if (dSq < bestDistSq)
                {
                    bestDistSq = dSq;
                    targetBuilding = b;
                }
            }

            if (targetBuilding != null)
            {
                Vector3 targetPos = targetBuilding.transform.position;
                float attackRange = 2.4f;

                while (targetBuilding != null && targetBuilding.State == BuildingState.Active && Vector3.Distance(transform.position, targetPos) > attackRange && IsAlive)
                {
                    MoveTowardPoint(targetPos, attackRange * 0.9f);
                    yield return null;
                }

                // In attack range: strike building with Khukuri slash
                if (targetBuilding != null && targetBuilding.State == BuildingState.Active && IsAlive)
                {
                    transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));

                    if (_animator != null)
                    {
                        _animator.PlayKhukuriSlash(() => {
                            if (targetBuilding != null && targetBuilding.State == BuildingState.Active)
                            {
                                targetBuilding.TakeDamage(14);
                            }
                        });
                        yield return new WaitForSeconds(1.1f);
                    }
                    else
                    {
                        targetBuilding.TakeDamage(14);
                        yield return new WaitForSeconds(1.1f);
                    }
                }
            }
            else
            {
                // March towards kingdom center (24, 0, 24)
                MoveTowardPoint(new Vector3(24f, 0, 24f), 0.5f);
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator HandleVillagerWorkLoop()
        {
            // Case A: Working Farm
            if (_targetFarm != null)
            {
                yield return HandleFarmWorkLoop();
                yield break;
            }

            // Case B: Harvesting Resource Node (Tree or Boulder)
            // 1. Walk to Resource Node
            _workState = VillagerWorkState.MovingToNode;
            while (_targetNode != null && !_targetNode.IsDepleted && Vector3.Distance(transform.position, _targetNode.transform.position) > 1.6f)
            {
                MoveTowardPoint(_targetNode.transform.position, 1.4f);
                yield return null;
            }

            if (_targetNode == null || _targetNode.IsDepleted)
            {
                _targetNode = FindNearestNodeOfType(transform.position, _carriedType, 14.0f);
                if (_targetNode == null)
                {
                    _workState = VillagerWorkState.IdlePatrol;
                    yield break;
                }
            }

            // 2. Harvest at Node (AoE Work Rhythm with Articulated Chopping / Mining)
            _workState = VillagerWorkState.Harvesting;
            float harvestTimer = 0f;
            while (_targetNode != null && !_targetNode.IsDepleted && _carriedAmount < _carryCapacity)
            {
                harvestTimer += Time.deltaTime;

                if (harvestTimer >= 1.25f)
                {
                    harvestTimer = 0f;

                    if (_carriedType == ResourceType.Wood)
                    {
                        GorkhaAudio.PlayWoodChop();
                    }
                    else if (_carriedType == ResourceType.Stone)
                    {
                        GorkhaAudio.PlayStonePick();
                    }

                    if (_animator != null)
                    {
                        _animator.PlayHarvestChop(() => {
                            if (_targetNode != null && !_targetNode.IsDepleted)
                            {
                                double gathered = _targetNode.Harvest(5.0);
                                _carriedAmount += gathered;
                                _carriedType = _targetNode.YieldType;
                                UpdateSackVisual();
                            }
                        });
                    }
                    else
                    {
                        double gathered = _targetNode.Harvest(5.0);
                        _carriedAmount += gathered;
                        _carriedType = _targetNode.YieldType;
                        UpdateSackVisual();
                    }
                }

                yield return null;
            }

            // 3. Deliver Resources to nearest Drop-off
            _workState = VillagerWorkState.DeliveringToDropOff;
            Vector3 dropOffPos = FindBestDropOffPoint(_carriedType);

            while (Vector3.Distance(transform.position, dropOffPos) > 2.0f)
            {
                MoveTowardPoint(dropOffPos, 1.8f);
                yield return null;
            }

            // Deposit resources
            if (_carriedAmount > 0)
            {
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.AddResource(_carriedType, _carriedAmount);
                }

                GorkhaAudio.PlayDeposit();
                GorkhaVoiceBarks.PlayDepositSuccessBark(this, (int)_carriedAmount, _carriedType.ToString());
                GorkhaPlayableHUD.SpawnFloatingText(dropOffPos + Vector3.up * 2.8f, $"+{(int)_carriedAmount} {_carriedType}", Color.yellow);
                _carriedAmount = 0.0;
                UpdateSackVisual();
            }

            // 4. Auto-Chain: If node is depleted or missing, find nearest adjacent node!
            if (_targetNode == null || _targetNode.IsDepleted)
            {
                _targetNode = FindNearestNodeOfType(transform.position, _carriedType, 14.0f);
                if (_targetNode == null)
                {
                    _workState = VillagerWorkState.IdlePatrol;
                }
            }
        }

        private IEnumerator HandleFarmWorkLoop()
        {
            if (_targetFarm == null || _targetFarm.State != BuildingState.Active)
            {
                _targetFarm = null;
                _workState = VillagerWorkState.IdlePatrol;
                yield break;
            }

            Vector3 farmCenter = _targetFarm.transform.position;

            // 1. Walk to Terraced Farm
            _workState = VillagerWorkState.MovingToNode;
            while (_targetFarm != null && _targetFarm.State == BuildingState.Active && Vector3.Distance(transform.position, farmCenter) > 2.0f)
            {
                MoveTowardPoint(farmCenter, 1.8f);
                yield return null;
            }

            if (_targetFarm == null || _targetFarm.State != BuildingState.Active)
            {
                _targetFarm = null;
                _workState = VillagerWorkState.IdlePatrol;
                yield break;
            }

            // 2. Cultivate & Harvest wheat crops
            _workState = VillagerWorkState.Harvesting;
            float harvestTimer = 0f;
            while (_targetFarm != null && _targetFarm.State == BuildingState.Active && _carriedAmount < _carryCapacity)
            {
                harvestTimer += Time.deltaTime;

                if (harvestTimer >= 1.25f)
                {
                    harvestTimer = 0f;
                    GorkhaAudio.PlayWheatRustle();

                    if (_animator != null)
                    {
                        _animator.PlayFarmCultivate(() => {
                            _carriedAmount += 5.0;
                            _carriedType = ResourceType.Food;
                            UpdateSackVisual();
                        });
                    }
                    else
                    {
                        _carriedAmount += 5.0;
                        _carriedType = ResourceType.Food;
                        UpdateSackVisual();
                    }
                }

                yield return null;
            }

            // 3. Deliver Food to Granary or Town Center
            _workState = VillagerWorkState.DeliveringToDropOff;
            Vector3 dropOffPos = FindBestDropOffPoint(ResourceType.Food);

            while (Vector3.Distance(transform.position, dropOffPos) > 2.0f)
            {
                MoveTowardPoint(dropOffPos, 1.8f);
                yield return null;
            }

            // Deposit food
            if (_carriedAmount > 0)
            {
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.AddResource(ResourceType.Food, _carriedAmount);
                }

                GorkhaAudio.PlayDeposit();
                GorkhaVoiceBarks.PlayDepositSuccessBark(this, (int)_carriedAmount, "Food");
                GorkhaPlayableHUD.SpawnFloatingText(dropOffPos + Vector3.up * 2.8f, $"+{(int)_carriedAmount} Food", new Color(1f, 0.9f, 0.3f));
                _carriedAmount = 0.0;
                UpdateSackVisual();
            }
        }

        private Vector3 FindBestDropOffPoint(ResourceType type)
        {
            Vector3 bestPos = new Vector3(24f, 0, 24f); // default kingdom center
            float bestDistSq = float.MaxValue;

            foreach (var b in BuildingInstance.AllBuildings)
            {
                if (b == null || b.State != BuildingState.Active || b.Data == null) continue;

                bool isMatch = b.Data.buildingType == BuildingType.TownCenter ||
                               (type == ResourceType.Wood && (b.Data.buildingType == BuildingType.WoodCamp || b.Data.buildingType == BuildingType.TimberYard)) ||
                               (type == ResourceType.Stone && (b.Data.buildingType == BuildingType.StoneQuarry || b.Data.buildingType == BuildingType.SlateDepot)) ||
                               (type == ResourceType.Food && (b.Data.buildingType == BuildingType.Farm || b.Data.buildingType == BuildingType.Granary));

                if (isMatch)
                {
                    float dSq = (b.transform.position - transform.position).sqrMagnitude;
                    if (dSq < bestDistSq)
                    {
                        bestDistSq = dSq;
                        bestPos = b.transform.position;
                    }
                }
            }

            return bestPos;
        }

        private IEnumerator HandleAmbientPatrol()
        {
            if (IsometricGrid.Instance != null)
            {
                int rx = Random.Range(6, 18);
                int ry = Random.Range(6, 18);
                _targetPosition = IsometricGrid.Instance.GridToWorldPosition(rx, ry);

                while (Vector3.Distance(transform.position, _targetPosition) > 0.35f && _workState == VillagerWorkState.IdlePatrol)
                {
                    MoveTowardPoint(_targetPosition, 0.35f);
                    yield return null;
                }

                yield return new WaitForSeconds(Random.Range(_idleDuration * 0.5f, _idleDuration * 1.5f));
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
            }
        }

        private void MoveTowardPoint(Vector3 dest, float stopDist)
        {
            Vector3 dir = (dest - transform.position);
            dir.y = 0;

            if (dir.sqrMagnitude > stopDist * stopDist)
            {
                Quaternion rot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, _rotationSpeed * Time.deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, dest, _moveSpeed * Time.deltaTime);
            }
        }
    }
}
