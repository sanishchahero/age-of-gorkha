using System;
using UnityEngine;
using Gorkha.Core;
using Gorkha.Economy;
using Gorkha.Graphics;
using Gorkha.Audio;
using Gorkha.UI;

namespace Gorkha.Buildings
{
    public enum BuildingState
    {
        Constructing,
        Active,
        Upgrading,
        Destroyed
    }

    /// <summary>
    /// Represents an active building placed in the 3D kingdom.
    /// Manages its grid location, level progression, construction countdown,
    /// floating harvest bubble, and interactive selection.
    /// Operates in BOTH Edit Mode and Play Mode via [ExecuteAlways].
    /// </summary>
    [ExecuteAlways]
    public class BuildingInstance : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<BuildingInstance> AllBuildings = new();
        public static event Action<BuildingInstance> OnBuildingSelected;

        [Header("Building Data")]
        [SerializeField] private BuildingData _data;
        [SerializeField] private int _level = 1;
        [SerializeField] private BuildingState _state = BuildingState.Active;

        [Header("Health & Durability")]
        [SerializeField] private int _maxHealth = 500;
        [SerializeField] private int _currentHealth = 500;

        [Header("Grid Position")]
        [SerializeField] private int _gridCol;
        [SerializeField] private int _gridRow;

        [Header("Visual References")]
        [SerializeField] private GameObject _scaffoldingVisual;
        [SerializeField] private GameObject _completedVisual;
        [SerializeField] private Transform _bubbleAnchor;
        [SerializeField] private GameObject _resourceBubble;
        [SerializeField] private GameObject _selectionRing;

        private float _constructionTimer = 0f;
        private float _totalConstructionTime = 10f;
        private float _bubbleBobTime = 0f;
        private float _bubbleScale = 1.0f;
        private float _bubbleTargetScale = 1.0f;

        // Damped Harmonic Oscillator for Juiciness
        private float _bounceScale = 1.0f;
        private float _bounceVelocity = 0.0f;
        private bool _isSelected = false;

        [Header("Production Buffer (CoC)")]
        [SerializeField] private double _uncollectedYield = 20.0;
        private WorldCombatHealthBar _combatHealthBar;

        public BuildingData Data => _data;
        public int Level => _level;
        public BuildingState State => _state;
        public int Health => _currentHealth;
        public int MaxHealth => _maxHealth;
        public bool IsAlive => _currentHealth > 0;
        public float RemainingConstructionTime => Mathf.Max(0f, _constructionTimer);
        public float TotalConstructionTime => _totalConstructionTime;
        public float ConstructionProgress => _totalConstructionTime > 0f ? 1f - (_constructionTimer / _totalConstructionTime) : 1f;
        public int GridCol => _gridCol;
        public int GridRow => _gridRow;
        public string BuildingId { get; private set; }
        public bool IsSelected => _isSelected;
        public double UncollectedYield => _uncollectedYield;
        public double MaxUncollectedBuffer => (_data != null && _data.hasProduction) ? Math.Max(30.0, _data.baseProductionRate * _level * 60.0) : 0;

        private void Awake()
        {
            EnsureVisuals();
            UpdateVisuals();
        }

        private void OnEnable()
        {
            if (!AllBuildings.Contains(this)) AllBuildings.Add(this);
            EnsureVisuals();
            UpdateVisuals();
            ResourceManager.Instance?.RecalculateStorageCapacities();
        }

        private void OnDisable()
        {
            AllBuildings.Remove(this);
            ResourceManager.Instance?.RecalculateStorageCapacities();
        }

        private void Start()
        {
            EnsureVisuals();
            UpdateVisuals();

            if (_state == BuildingState.Constructing && BuilderManager.Instance != null)
            {
                BuilderManager.Instance.AssignBuilder(this);
            }
        }

        public void Initialize(BuildingData data, int col, int row, bool startAsConstructing = true)
        {
            _data = data;
            _gridCol = col;
            _gridRow = row;
            BuildingId = $"{data.buildingType}_{Guid.NewGuid():N}";

            if (startAsConstructing && Application.isPlaying)
            {
                _level = 0;
                _state = BuildingState.Constructing;
                _totalConstructionTime = Mathf.Max(3f, data.constructionDurationSeconds);
                _constructionTimer = _totalConstructionTime;
                BuilderManager.Instance?.AssignBuilder(this);
            }
            else
            {
                _level = 1;
                _state = BuildingState.Active;
            }

            EnsureVisuals();
            UpdateVisuals();
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            // 1. Spring Bounce Hop (Damped Harmonic Motion)
            if (_bounceScale > 1.001f || Mathf.Abs(_bounceVelocity) > 0.001f)
            {
                float displacement = _bounceScale - 1.0f;
                float force = -displacement * 280.0f;
                _bounceVelocity += force * Time.deltaTime;
                _bounceVelocity *= Mathf.Exp(-16.0f * Time.deltaTime);
                _bounceScale += _bounceVelocity * Time.deltaTime;

                if (Mathf.Abs(_bounceScale - 1.0f) < 0.001f && Mathf.Abs(_bounceVelocity) < 0.01f)
                {
                    _bounceScale = 1.0f;
                    _bounceVelocity = 0.0f;
                }
                transform.localScale = Vector3.one * _bounceScale;
            }

            // 2. Handle construction countdown
            if (_state == BuildingState.Constructing || _state == BuildingState.Upgrading)
            {
                _constructionTimer -= Time.deltaTime;
                if (_constructionTimer <= 0)
                {
                    CompleteConstruction();
                }
            }

            // 3. Animate floating resource bubble & accumulate buffer
            if (_state == BuildingState.Active && _data != null && _data.hasProduction)
            {
                _uncollectedYield = Math.Min(MaxUncollectedBuffer, _uncollectedYield + (_data.baseProductionRate * _level * Time.deltaTime));
                bool hasYield = _uncollectedYield >= 4.0;

                if (_resourceBubble != null)
                {
                    if (_resourceBubble.activeSelf != hasYield) _resourceBubble.SetActive(hasYield);

                    if (hasYield)
                    {
                        _bubbleBobTime += Time.deltaTime * 2.8f;
                        float bobOffset = Mathf.Sin(_bubbleBobTime) * 0.18f;
                        if (_bubbleAnchor != null)
                        {
                            _resourceBubble.transform.localPosition = new Vector3(0, (_data.buildingType == BuildingType.TownCenter ? 5.2f : 3.2f) + bobOffset, 0);
                            _resourceBubble.transform.Rotate(Vector3.up, 35f * Time.deltaTime, Space.Self);
                        }

                        // Smooth scale recovery after harvesting
                        if (Mathf.Abs(_bubbleScale - _bubbleTargetScale) > 0.01f)
                        {
                            _bubbleScale = Mathf.MoveTowards(_bubbleScale, _bubbleTargetScale, Time.deltaTime * 2.0f);
                            _resourceBubble.transform.localScale = Vector3.one * _bubbleScale;
                        }
                    }
                }
            }

            // 4. Pulsing Selection Ring
            if (_isSelected && _selectionRing != null)
            {
                float pulse = 0.55f + 0.20f * Mathf.Sin(Time.time * 6.0f);
                var ren = _selectionRing.GetComponent<MeshRenderer>();
                if (ren != null && ren.sharedMaterial != null)
                {
                    Color c = new Color(0.25f, 0.98f, 0.45f, pulse);
                    if (ren.material.HasProperty("_BaseColor")) ren.material.SetColor("_BaseColor", c);
                    if (ren.material.HasProperty("_Color")) ren.material.SetColor("_Color", c);
                }
            }
        }

        public void TriggerBounce(float intensity = 1.10f)
        {
            _bounceScale = intensity;
            _bounceVelocity = 0.0f;
            GorkhaAudio.PlayBuildingBounce();
        }

        public void CompleteConstruction()
        {
            _level++;
            _state = BuildingState.Active;
            BuilderManager.Instance?.ReleaseBuilder(this);
            UpdateVisuals();

            GorkhaAudio.PlayUpgrade();
            GorkhaVFX.SpawnUpgradeConfetti(transform.position);
            float w = _data != null ? _data.tileWidth * 2f : 4f;
            float h = _data != null ? _data.tileHeight * 2f : 4f;
            GorkhaVFX.SpawnPlacementDust(transform.position, new Vector2(w, h));
            TriggerBounce(1.15f);

            if (_data != null && _data.hasProduction && ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddProductionRate(_data.productionType, _data.baseProductionRate);
            }

            ResourceManager.Instance?.RecalculateStorageCapacities();
        }

        public bool StartUpgrade()
        {
            if (_state != BuildingState.Active || _data == null) return false;

            if (BuilderManager.Instance != null && !BuilderManager.Instance.CanAssignBuilder())
            {
                GorkhaAudio.PlayError();
                GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 3f, "All Builders Busy!", Color.red);
                return false;
            }

            var cost = _data.GetCostDictionary(_level);
            if (ResourceManager.Instance != null && !ResourceManager.Instance.SpendResources(cost)) return false;

            GorkhaAudio.PlayBuild();
            BuilderManager.Instance?.AssignBuilder(this);

            _state = BuildingState.Upgrading;
            _totalConstructionTime = Mathf.Max(4f, _data.constructionDurationSeconds * Mathf.Pow(1.25f, _level));
            _constructionTimer = _totalConstructionTime;
            UpdateVisuals();
            return true;
        }

        public double CollectResource()
        {
            if (_state != BuildingState.Active || _data == null || !_data.hasProduction || _uncollectedYield <= 0.5) return 0;

            if (ResourceManager.Instance != null && ResourceManager.Instance.IsAtCapacity(_data.productionType))
            {
                GorkhaAudio.PlayError();
                GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 3.5f, "Storages Full!", Color.red);
                return 0;
            }

            double freeSpace = ResourceManager.Instance != null ? ResourceManager.Instance.FreeStorage(_data.productionType) : _uncollectedYield;
            double harvestAmount = Math.Min(_uncollectedYield, freeSpace);
            _uncollectedYield -= harvestAmount;

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(_data.productionType, harvestAmount);
            }

            GorkhaAudio.PlayHarvest();
            Color sparkColor = GetResourceColor(_data.productionType);
            GorkhaVFX.SpawnHarvestSparkles(transform.position + Vector3.up * 3.5f, sparkColor);
            TriggerBounce(1.08f);

            // Pop bubble and let it spring back over time
            if (_resourceBubble != null)
            {
                _bubbleScale = 0.1f;
                _bubbleTargetScale = 1.0f;
                _resourceBubble.transform.localScale = Vector3.one * 0.1f;
            }

            return harvestAmount;
        }

        public void RelocateTo(int newCol, int newRow, Vector3 newWorldPos)
        {
            _gridCol = newCol;
            _gridRow = newRow;
            transform.position = newWorldPos;
            TriggerBounce(1.15f);

            if (TryGetComponent<WallInstance>(out var wall))
            {
                wall.RegisterAndRefresh();
            }
        }

        public void TakeDamage(int damage)
        {
            if (_state == BuildingState.Destroyed) return;

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            _combatHealthBar?.UpdateHealth(_currentHealth, _maxHealth);
            TriggerBounce(1.06f);
            GorkhaAudio.PlayHarvest();
            GorkhaVFX.SpawnPlacementDust(transform.position, Vector2.one * 0.7f);
            GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 2.8f, $"-{damage}", Color.red);

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                _state = BuildingState.Destroyed;
                _combatHealthBar?.SetVisible(false);
                GorkhaAudio.PlayStructureBreach();
                GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 3.2f, "💥 Structure Breached!", Color.red);
                if (_completedVisual != null) _completedVisual.SetActive(false);
                if (IsometricGrid.Instance != null && _data != null)
                {
                    IsometricGrid.Instance.FreeArea(_gridCol, _gridRow, _data.tileWidth, _data.tileHeight);
                }
            }
        }

        private Color GetResourceColor(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Gold: return new Color(1.0f, 0.88f, 0.1f);
                case ResourceType.Food: return new Color(0.95f, 0.82f, 0.30f);
                case ResourceType.Wood: return new Color(0.85f, 0.65f, 0.40f);
                case ResourceType.Stone: return new Color(0.75f, 0.78f, 0.82f);
                default: return Color.white;
            }
        }

        public void EnsureVisuals()
        {
            if (_data != null)
            {
                // Check if we need to upgrade an existing procedural visual to a Fantasy Kingdom model
                if (FantasyKingdomAssetRegistry.HasPrefab(_data.buildingType))
                {
                    if (_completedVisual == null || _completedVisual.name != $"FK_Visual_{_data.buildingType}")
                    {
                        if (_completedVisual != null)
                        {
                            if (Application.isPlaying) Destroy(_completedVisual);
                            else DestroyImmediate(_completedVisual);
                            _completedVisual = null;
                        }
                        _completedVisual = FantasyKingdomAssetRegistry.InstantiateBuildingVisual(_data.buildingType, transform);
                    }
                }

                // Procedural fallback or specialized Nepali defensive structures
                if (_completedVisual == null)
                {
                    switch (_data.buildingType)
                    {
                        case BuildingType.TownCenter:
                            _completedVisual = Nepali3DMeshBuilder.BuildTownCenter(transform);
                            break;
                        case BuildingType.Farm:
                            _completedVisual = Nepali3DMeshBuilder.BuildFarm(transform);
                            break;
                        case BuildingType.Barracks:
                            _completedVisual = Nepali3DMeshBuilder.BuildBarracks(transform);
                            break;
                        case BuildingType.WoodCamp:
                            _completedVisual = Nepali3DMeshBuilder.BuildWoodCamp(transform);
                            break;
                        case BuildingType.StoneQuarry:
                            _completedVisual = Nepali3DMeshBuilder.BuildQuarry(transform);
                            break;
                        case BuildingType.House:
                            _completedVisual = Nepali3DMeshBuilder.BuildHouse(transform);
                            break;
                        case BuildingType.Watchtower:
                            _completedVisual = Nepali3DMeshBuilder.BuildWatchtower(transform);
                            break;
                        case BuildingType.Kaman:
                            _completedVisual = Nepali3DMeshBuilder.BuildKamanCannon(transform, _level);
                            break;
                        case BuildingType.Wall:
                            var wall = GetComponent<WallInstance>();
                            if (wall == null) wall = gameObject.AddComponent<WallInstance>();
                            wall.RegisterAndRefresh();
                            _completedVisual = transform.Find("WallGeometry_3D")?.gameObject;
                            break;
                        case BuildingType.Trap:
                            _completedVisual = Nepali3DMeshBuilder.BuildSpikeTrap(transform);
                            break;
                        case BuildingType.Granary:
                            _completedVisual = Nepali3DMeshBuilder.BuildGranary(transform);
                            break;
                        case BuildingType.TimberYard:
                            _completedVisual = Nepali3DMeshBuilder.BuildTimberYard(transform);
                            break;
                        case BuildingType.SlateDepot:
                            _completedVisual = Nepali3DMeshBuilder.BuildSlateDepot(transform);
                            break;
                        case BuildingType.Vault:
                            _completedVisual = Nepali3DMeshBuilder.BuildVault(transform);
                            break;
                        default:
                            _completedVisual = Nepali3DMeshBuilder.BuildTownCenter(transform);
                            break;
                    }
                }

                // Health & durability setup
                if (FantasyKingdomAssetRegistry.TryGetCalibration(_data.buildingType, out var cal))
                {
                    _maxHealth = (int)cal.CustomHealth;
                }
                else
                {
                    _maxHealth = _data.buildingType switch
                    {
                        BuildingType.TownCenter => 1500,
                        BuildingType.Barracks => 600,
                        BuildingType.Watchtower => 500,
                        BuildingType.Kaman => 450,
                        BuildingType.Wall => 600,
                        BuildingType.Farm => 350,
                        BuildingType.Granary => 500,
                        BuildingType.TimberYard => 500,
                        BuildingType.SlateDepot => 600,
                        BuildingType.Vault => 800,
                        BuildingType.WoodCamp => 400,
                        BuildingType.StoneQuarry => 450,
                        BuildingType.House => 300,
                        BuildingType.Trap => 200,
                        _ => 500
                    };
                }

                if (_data.buildingType == BuildingType.Watchtower || _data.buildingType == BuildingType.Kaman)
                {
                    if (!TryGetComponent<TowerDefenseController>(out _))
                    {
                        gameObject.AddComponent<TowerDefenseController>();
                    }
                }
                else if (_data.buildingType == BuildingType.Trap)
                {
                    if (!TryGetComponent<TrapInstance>(out _))
                    {
                        gameObject.AddComponent<TrapInstance>();
                    }
                }

                _currentHealth = _maxHealth;
            }

            // Ensure BoxCollider on this root GameObject for direct raycasts and selection
            if (!TryGetComponent<BoxCollider>(out var myCol))
            {
                myCol = gameObject.AddComponent<BoxCollider>();
                if (_data != null && _data.buildingType == BuildingType.Wall)
                {
                    myCol.center = new Vector3(0, 1.1f, 0);
                    myCol.size = new Vector3(1.8f, 2.2f, 1.8f);
                }
                else if (_data != null && _data.buildingType == BuildingType.Trap)
                {
                    myCol.center = new Vector3(0, 0.25f, 0);
                    myCol.size = new Vector3(1.8f, 0.5f, 1.8f);
                }
                else
                {
                    float w = _data != null ? _data.tileWidth * 1.8f : 3.6f;
                    float h = _data != null ? _data.tileHeight * 1.8f : 3.6f;
                    float colHeight = (_data != null && (_data.buildingType == BuildingType.TownCenter || _data.buildingType == BuildingType.Watchtower)) ? 5.5f : 3.8f;
                    myCol.center = new Vector3(0, colHeight * 0.5f, 0);
                    myCol.size = new Vector3(w, colHeight, h);
                }
            }

            if (_scaffoldingVisual == null && (_data == null || (_data.buildingType != BuildingType.Wall && _data.buildingType != BuildingType.Trap)))
            {
                float w = _data != null ? _data.tileWidth * 1.8f : 3.4f;
                float h = _data != null ? _data.tileHeight * 1.8f : 3.4f;
                _scaffoldingVisual = Nepali3DMeshBuilder.BuildScaffolding(transform, new Vector3(w, 2.5f, h));
            }

            if (_resourceBubble == null && _data != null && _data.hasProduction)
            {
                _resourceBubble = new GameObject("ResourceBubble_3D");
                _resourceBubble.transform.SetParent(transform, false);
                float hOffset = _data.buildingType == BuildingType.TownCenter ? 5.2f : 3.2f;
                _resourceBubble.transform.localPosition = new Vector3(0, hOffset, 0);
                Nepali3DMeshBuilder.BuildHarvestMedallion(_resourceBubble.transform, _data.productionType);

                var sphereCol = _resourceBubble.AddComponent<SphereCollider>();
                sphereCol.radius = 0.65f;

                _bubbleAnchor = _resourceBubble.transform;
            }

            // 3D Clash of Clans Style Ground Selection Baseplate
            if (_selectionRing == null && _data != null)
            {
                _selectionRing = new GameObject("SelectionRing_3D");
                _selectionRing.transform.SetParent(transform, false);
                _selectionRing.transform.localPosition = new Vector3(0, 0.03f, 0);

                float rw = _data.tileWidth * 2.0f;
                float rh = _data.tileHeight * 2.0f;

                // Center glowing baseplate
                var basePlate = Nepali3DMeshBuilder.CreateSubMesh("BasePlate", Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(rw * 0.94f, 0.02f, rh * 0.94f)), GorkhaMaterials.SelectionGlow, _selectionRing.transform);

                // Corner brackets
                float hx = rw * 0.46f;
                float hz = rh * 0.46f;
                Vector3[] corners = {
                    new Vector3(-hx, 0.03f, -hz), new Vector3(hx, 0.03f, -hz),
                    new Vector3(hx, 0.03f, hz), new Vector3(-hx, 0.03f, hz)
                };
                foreach (var cPos in corners)
                {
                    Nepali3DMeshBuilder.CreateSubMesh("CornerBracket", Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(0.35f, 0.04f, 0.35f)), GorkhaMaterials.GoldenGajur, _selectionRing.transform, cPos);
                }

                _selectionRing.SetActive(_isSelected);
            }

            // Initialize World Combat Health Bar
            if (_combatHealthBar == null)
            {
                _combatHealthBar = GetComponent<WorldCombatHealthBar>();
                if (_combatHealthBar == null)
                {
                    _combatHealthBar = gameObject.AddComponent<WorldCombatHealthBar>();
                }
            }
            float barY = (_data != null && _data.buildingType == BuildingType.Watchtower) ? 4.8f : 3.2f;
            _combatHealthBar.Initialize(_maxHealth, new Vector3(0, barY, 0), isBuilding: true);
        }

        private void UpdateVisuals()
        {
            bool isUnderConstruction = _state == BuildingState.Constructing || _state == BuildingState.Upgrading;

            if (_scaffoldingVisual != null) _scaffoldingVisual.SetActive(isUnderConstruction);
            if (_completedVisual != null) _completedVisual.SetActive(!isUnderConstruction);
            if (_resourceBubble != null) _resourceBubble.SetActive(_state == BuildingState.Active && _data != null && _data.hasProduction);
            if (_selectionRing != null) _selectionRing.SetActive(_isSelected);
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            if (_selectionRing != null) _selectionRing.SetActive(selected);
            if (selected)
            {
                TriggerBounce(1.10f);
            }
        }

        public void Select()
        {
            SetSelected(true);
            OnBuildingSelected?.Invoke(this);
        }

        public void Deselect()
        {
            SetSelected(false);
        }
    }
}
