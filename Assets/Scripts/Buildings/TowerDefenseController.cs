using System.Collections;
using UnityEngine;
using Gorkha.Units;
using Gorkha.Graphics;
using Gorkha.Audio;
using Gorkha.UI;

namespace Gorkha.Buildings
{
    public enum DefenseTowerType
    {
        ArcherWatchtower,
        KamanMortarCannon
    }

    /// <summary>
    /// Master Defense Tower Controller (Clash of Clans & Age of Empires style).
    /// Features:
    /// - Auto-target acquisition within defensive radius
    /// - High-ground mountain ridge range & damage bonuses (+20%)
    /// - Interactive translucent range circle decal when selected
    /// - Physical ballistic projectile simulation with parabolic flight arcs
    /// - Watchtower sniper arrows & Kaman explosive mortar splash damage
    /// </summary>
    public class TowerDefenseController : MonoBehaviour
    {
        [Header("Tower Configuration")]
        [SerializeField] private DefenseTowerType _towerType = DefenseTowerType.ArcherWatchtower;
        [SerializeField] private float _baseRange = 12f;
        [SerializeField] private float _minRange = 0f;
        [SerializeField] private float _attackCooldown = 1.4f;
        [SerializeField] private int _baseDamage = 30;

        private BuildingInstance _building;
        private float _attackTimer = 0f;
        private UnitController _currentTarget;
        private GameObject _rangeIndicator;
        private Transform _rotatingSwivel;
        private bool _isHighGround = false;

        public DefenseTowerType TowerType => _towerType;
        public float EffectiveRange => _baseRange * (_isHighGround ? 1.20f : 1.0f);
        public int EffectiveDamage => Mathf.RoundToInt(_baseDamage * (_isHighGround ? 1.20f : 1.0f));

        private void Awake()
        {
            _building = GetComponent<BuildingInstance>();
            CheckHighGroundBonus();
        }

        private void Start()
        {
            if (_building == null) _building = GetComponent<BuildingInstance>();
            ConfigureTowerParameters();
            SetupStationedTurret();
        }

        private void ConfigureTowerParameters()
        {
            if (_building != null && _building.Data != null)
            {
                if (_building.Data.buildingType == BuildingType.Kaman)
                {
                    _towerType = DefenseTowerType.KamanMortarCannon;
                    _baseRange = 18f;
                    _minRange = 3.5f;
                    _attackCooldown = 3.8f;
                    _baseDamage = 75;
                }
                else
                {
                    _towerType = DefenseTowerType.ArcherWatchtower;
                    _baseRange = 12f;
                    _minRange = 0f;
                    _attackCooldown = 1.4f;
                    _baseDamage = 30;
                }
            }
        }

        private void CheckHighGroundBonus()
        {
            // Mountain terraces or elevated positions grant high ground tactical superiority
            _isHighGround = transform.position.y > 0.15f || transform.position.x < 10f || transform.position.z > 38f;
        }

        private void SetupStationedTurret()
        {
            if (_towerType == DefenseTowerType.ArcherWatchtower)
            {
                // Station 3D Gorkha Archer on the top observation platform (height 3.2m)
                GameObject archer = new GameObject("StationedArcher_Garrison");
                archer.transform.SetParent(transform, false);
                archer.transform.localPosition = new Vector3(0, 3.25f, 0);
                Nepali3DMeshBuilder.BuildUnit(archer.transform, hasKhukuri: false);
                _rotatingSwivel = archer.transform;
            }
            else
            {
                // Find cannon carriage/barrel
                Transform barrel = transform.Find("KamanCannon_3D/CannonBarrel");
                if (barrel != null) _rotatingSwivel = barrel;
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;
            if (_building != null && _building.State != BuildingState.Active) return;

            UpdateRangeIndicator();

            _attackTimer -= Time.deltaTime;

            // Target Validation
            if (_currentTarget == null || !_currentTarget.IsAlive || Vector3.Distance(transform.position, _currentTarget.transform.position) > EffectiveRange)
            {
                AcquireTarget();
            }

            if (_currentTarget != null)
            {
                RotateTowardsTarget(_currentTarget.transform.position);

                if (_attackTimer <= 0f)
                {
                    FireAtTarget(_currentTarget);
                    _attackTimer = _attackCooldown;
                }
            }
        }

        private void AcquireTarget()
        {
            _currentTarget = UnitController.FindNearestHostileTarget(transform.position, EffectiveRange);

            // Blind-spot check for Kaman mortar
            if (_currentTarget != null && _minRange > 0f)
            {
                float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
                if (dist < _minRange)
                {
                    _currentTarget = null;
                }
            }
        }

        private void RotateTowardsTarget(Vector3 targetPos)
        {
            if (_rotatingSwivel == null) return;

            Vector3 dir = targetPos - _rotatingSwivel.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                _rotatingSwivel.rotation = Quaternion.Slerp(_rotatingSwivel.rotation, targetRot, 8f * Time.deltaTime);
            }
        }

        private void FireAtTarget(UnitController target)
        {
            if (target == null) return;

            Vector3 startPos = _rotatingSwivel != null ? _rotatingSwivel.position + Vector3.up * 0.4f : transform.position + Vector3.up * 3.5f;
            Vector3 targetPos = target.transform.position + Vector3.up * 0.5f;

            if (_towerType == DefenseTowerType.ArcherWatchtower)
            {
                GorkhaAudio.PlayBowTwang();
                BallisticProjectile.LaunchArrow(startPos, targetPos, target, EffectiveDamage);
            }
            else
            {
                GorkhaAudio.PlayCannon();
                GorkhaVFX.SpawnPlacementDust(startPos, Vector2.one * 0.8f);
                StartCoroutine(LaunchBallisticCannonball(startPos, targetPos));
            }
        }

        private IEnumerator LaunchBallisticArrow(Vector3 start, Vector3 target, UnitController targetUnit)
        {
            GameObject arrow = Nepali3DMeshBuilder.BuildArrow();
            arrow.transform.position = start;

            float duration = 0.55f;
            float elapsed = 0f;
            float arcHeight = 2.2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                Vector3 currentPos = Vector3.Lerp(start, target, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

                Vector3 nextPos = Vector3.Lerp(start, target, Mathf.Min(1f, t + 0.05f));
                nextPos.y += Mathf.Sin(Mathf.Min(1f, t + 0.05f) * Mathf.PI) * arcHeight;

                Vector3 dir = nextPos - currentPos;
                if (dir.sqrMagnitude > 0.001f)
                {
                    arrow.transform.rotation = Quaternion.LookRotation(dir.normalized);
                }

                arrow.transform.position = currentPos;
                yield return null;
            }

            // Arrow Impact
            if (targetUnit != null && targetUnit.IsAlive)
            {
                targetUnit.TakeDamage(EffectiveDamage);
            }
            GorkhaVFX.SpawnPlacementDust(target, Vector2.one * 0.3f);

            Destroy(arrow);
        }

        private IEnumerator LaunchBallisticCannonball(Vector3 start, Vector3 target)
        {
            GameObject ball = Nepali3DMeshBuilder.BuildCannonball();
            ball.transform.position = start;

            float duration = 1.1f;
            float elapsed = 0f;
            float arcHeight = 5.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                Vector3 currentPos = Vector3.Lerp(start, target, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

                ball.transform.position = currentPos;
                ball.transform.Rotate(Vector3.right, 360f * Time.deltaTime, Space.Self);
                yield return null;
            }

            // High Explosive Splash Impact
            GorkhaAudio.PlayBuildingBounce();
            GorkhaVFX.SpawnPlacementDust(target, Vector2.one * 2.8f);

            // Deal splash damage to all hostiles within splash radius
            float blastMult = Progression.TechTreeManager.Instance != null ? Progression.TechTreeManager.Instance.CannonBlastMultiplier : 1.0f;
            float splashRadius = 3.5f * blastMult;
            foreach (var u in UnitController.AllUnits)
            {
                if (u != null && u.IsHostile && u.IsAlive)
                {
                    float dist = Vector3.Distance(u.transform.position, target);
                    if (dist <= splashRadius)
                    {
                        int splashDamage = Mathf.RoundToInt(EffectiveDamage * (1f - (dist / (splashRadius * 1.5f))));
                        u.TakeDamage(Mathf.Max(15, splashDamage));
                    }
                }
            }

            Destroy(ball);
        }

        private void UpdateRangeIndicator()
        {
            bool shouldShow = _building != null && _building.IsSelected;

            if (shouldShow && _rangeIndicator == null)
            {
                _rangeIndicator = new GameObject("RangeIndicator_Decal");
                _rangeIndicator.transform.SetParent(transform, false);
                _rangeIndicator.transform.localPosition = new Vector3(0, 0.04f, 0);

                float diameter = EffectiveRange * 2f;
                var disc = Nepali3DMeshBuilder.CreateSubMesh("RangeDisc", Nepali3DMeshBuilder.CreateCylinderMesh(diameter * 0.5f, 0.01f), GorkhaMaterials.SelectionGlow, _rangeIndicator.transform);
                var ren = disc.GetComponent<MeshRenderer>();
                if (ren != null && ren.sharedMaterial != null)
                {
                    Color c = _towerType == DefenseTowerType.ArcherWatchtower ? new Color(0.2f, 0.8f, 1.0f, 0.22f) : new Color(1.0f, 0.4f, 0.1f, 0.22f);
                    if (ren.material.HasProperty("_BaseColor")) ren.material.SetColor("_BaseColor", c);
                    if (ren.material.HasProperty("_Color")) ren.material.SetColor("_Color", c);
                }
            }
            else if (!shouldShow && _rangeIndicator != null)
            {
                Destroy(_rangeIndicator);
                _rangeIndicator = null;
            }
        }
    }
}
