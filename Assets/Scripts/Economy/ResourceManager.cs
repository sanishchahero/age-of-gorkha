using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gorkha.Economy
{
    [Serializable]
    public class ResourceBalance
    {
        public ResourceType type;
        public double amount;
        public double productionRate; // Units generated per second
        public double capacity = 1000.0;
    }

    /// <summary>
    /// Central singleton managing all kingdom resources.
    /// Handles continuous production accumulation, affordability checks,
    /// transactions, and UI event broadcasts.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        public event Action OnResourcesChanged;

        [Header("Starting Balances")]
        [SerializeField] private double _initialFood = 250;
        [SerializeField] private double _initialWood = 250;
        [SerializeField] private double _initialStone = 150;
        [SerializeField] private double _initialIron = 50;
        [SerializeField] private double _initialGold = 100;
        [SerializeField] private double _initialGems = 10;

        [Header("Kingdom Peace Shield (DEF-05)")]
        [SerializeField] private float _shieldTimer = 720f; // 12 minutes starting treaty shield

        [Header("Population Caps (AoE)")]
        [SerializeField] private int _basePopulationCap = 10;
        private int _maxPopulation = 10;

        public int CurrentPopulation
        {
            get
            {
                int count = 0;
                foreach (var u in Gorkha.Units.UnitController.AllUnits)
                {
                    if (u != null && !u.IsHostile && u.IsAlive) count++;
                }
                return count;
            }
        }

        public int MaxPopulation => _maxPopulation;
        public bool CanSupportUnit() => CurrentPopulation < _maxPopulation;

        private readonly Dictionary<ResourceType, ResourceBalance> _balances = new();

        public float ShieldRemainingSeconds => Mathf.Max(0f, _shieldTimer);
        public bool HasActiveShield => _shieldTimer > 0f;

        public void ActivateShield(float durationSeconds)
        {
            _shieldTimer = durationSeconds;
            OnResourcesChanged?.Invoke();
        }

        public void BreakShield()
        {
            _shieldTimer = 0f;
            OnResourcesChanged?.Invoke();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeBalance(ResourceType.Food, _initialFood, 2.0);
            InitializeBalance(ResourceType.Wood, _initialWood, 1.5);
            InitializeBalance(ResourceType.Stone, _initialStone, 0.8);
            InitializeBalance(ResourceType.Iron, _initialIron, 0.0);
            InitializeBalance(ResourceType.Gold, _initialGold, 0.5);
            InitializeBalance(ResourceType.Gems, _initialGems, 0.0);
        }

        private void Start()
        {
            RecalculateStorageCapacities();
        }

        /// <summary>
        /// Recalculates total kingdom storage capacity based on active Town Center,
        /// Granaries, Timber Yards, Slate Depots, and Vaults (CoC ECO-01).
        /// Also calculates maximum population capacity supported by Houses (AoE).
        /// </summary>
        public void RecalculateStorageCapacities()
        {
            double baseFoodCap = 500.0;
            double baseWoodCap = 500.0;
            double baseStoneCap = 500.0;
            double baseGoldCap = 250.0;
            double baseIronCap = 200.0;
            double baseGemsCap = 9999.0;
            int popCap = _basePopulationCap;

            foreach (var b in Gorkha.Buildings.BuildingInstance.AllBuildings)
            {
                if (b == null || b.State == Gorkha.Buildings.BuildingState.Destroyed || b.Data == null) continue;
                int lvl = Mathf.Max(1, b.Level);

                // Town Center grants base capacity across all resources
                if (b.Data.buildingType == Gorkha.Buildings.BuildingType.TownCenter)
                {
                    baseFoodCap += 500.0 * lvl;
                    baseWoodCap += 500.0 * lvl;
                    baseStoneCap += 500.0 * lvl;
                    baseGoldCap += 250.0 * lvl;
                    popCap += 5 * lvl;
                }
                // Dedicated Storages
                else if (b.Data.buildingType == Gorkha.Buildings.BuildingType.Granary || (b.Data.isStorage && b.Data.storageType == ResourceType.Food))
                {
                    baseFoodCap += b.Data.baseStorageCapacity * lvl;
                }
                else if (b.Data.buildingType == Gorkha.Buildings.BuildingType.TimberYard || (b.Data.isStorage && b.Data.storageType == ResourceType.Wood))
                {
                    baseWoodCap += b.Data.baseStorageCapacity * lvl;
                }
                else if (b.Data.buildingType == Gorkha.Buildings.BuildingType.SlateDepot || (b.Data.isStorage && b.Data.storageType == ResourceType.Stone))
                {
                    baseStoneCap += b.Data.baseStorageCapacity * lvl;
                }
                else if (b.Data.buildingType == Gorkha.Buildings.BuildingType.Vault || (b.Data.isStorage && b.Data.storageType == ResourceType.Gold))
                {
                    baseGoldCap += b.Data.baseStorageCapacity * lvl;
                }
                // Houses provide population capacity
                else if (b.Data.buildingType == Gorkha.Buildings.BuildingType.House)
                {
                    popCap += (b.Data.populationCapacity > 0 ? b.Data.populationCapacity : 5) * lvl;
                }
            }

            SetCapacityInternal(ResourceType.Food, baseFoodCap);
            SetCapacityInternal(ResourceType.Wood, baseWoodCap);
            SetCapacityInternal(ResourceType.Stone, baseStoneCap);
            SetCapacityInternal(ResourceType.Iron, baseIronCap);
            SetCapacityInternal(ResourceType.Gold, baseGoldCap);
            SetCapacityInternal(ResourceType.Gems, baseGemsCap);

            _maxPopulation = popCap;
            OnResourcesChanged?.Invoke();
        }

        private void SetCapacityInternal(ResourceType type, double capacity)
        {
            if (_balances.TryGetValue(type, out var bal))
            {
                bal.capacity = capacity;
                bal.amount = Math.Min(bal.capacity, bal.amount);
            }
        }

        private void InitializeBalance(ResourceType type, double amount, double rate)
        {
            _balances[type] = new ResourceBalance
            {
                type = type,
                amount = amount,
                productionRate = rate,
                capacity = 2500
            };
        }

        private void Update()
        {
            // Continuous per-frame accumulation based on Time.deltaTime
            float dt = Time.deltaTime;
            bool changed = false;

            if (_shieldTimer > 0f)
            {
                _shieldTimer = Mathf.Max(0f, _shieldTimer - dt);
            }

            foreach (var kvp in _balances)
            {
                var bal = kvp.Value;
                if (bal.productionRate > 0)
                {
                    bal.amount = Math.Min(bal.capacity, bal.amount + bal.productionRate * dt);
                    changed = true;
                }
            }

            if (changed)
            {
                OnResourcesChanged?.Invoke();
            }
        }

        public double Food => GetAmount(ResourceType.Food);
        public double Wood => GetAmount(ResourceType.Wood);
        public double Stone => GetAmount(ResourceType.Stone);
        public double Iron => GetAmount(ResourceType.Iron);
        public double Gold => GetAmount(ResourceType.Gold);
        public double Gems => GetAmount(ResourceType.Gems);

        public double FoodCapacity => GetCapacity(ResourceType.Food);
        public double WoodCapacity => GetCapacity(ResourceType.Wood);
        public double StoneCapacity => GetCapacity(ResourceType.Stone);
        public double GoldCapacity => GetCapacity(ResourceType.Gold);
        public double GemsCapacity => GetCapacity(ResourceType.Gems);

        public double GetAmount(ResourceType type)
        {
            return _balances.TryGetValue(type, out var bal) ? bal.amount : 0;
        }

        public double GetCapacity(ResourceType type)
        {
            return _balances.TryGetValue(type, out var bal) ? bal.capacity : 2500;
        }

        public void SetCapacity(ResourceType type, double capacity)
        {
            if (_balances.TryGetValue(type, out var bal))
            {
                bal.capacity = capacity;
                bal.amount = Math.Min(bal.capacity, bal.amount);
                OnResourcesChanged?.Invoke();
            }
        }

        public void AddCapacity(ResourceType type, double additional)
        {
            if (_balances.TryGetValue(type, out var bal))
            {
                bal.capacity += additional;
                OnResourcesChanged?.Invoke();
            }
        }

        public bool IsAtCapacity(ResourceType type)
        {
            if (_balances.TryGetValue(type, out var bal))
            {
                return bal.amount >= bal.capacity - 0.01;
            }
            return false;
        }

        public double FreeStorage(ResourceType type)
        {
            if (_balances.TryGetValue(type, out var bal))
            {
                return Math.Max(0.0, bal.capacity - bal.amount);
            }
            return 0;
        }

        public double GetRate(ResourceType type)
        {
            return _balances.TryGetValue(type, out var bal) ? bal.productionRate : 0;
        }

        public void AddProductionRate(ResourceType type, double additionalRate)
        {
            if (_balances.TryGetValue(type, out var bal))
            {
                bal.productionRate += additionalRate;
                OnResourcesChanged?.Invoke();
            }
        }

        public bool CanAfford(Dictionary<ResourceType, double> cost)
        {
            if (cost == null) return true;
            foreach (var kvp in cost)
            {
                if (GetAmount(kvp.Key) < kvp.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public bool SpendResources(Dictionary<ResourceType, double> cost)
        {
            if (!CanAfford(cost)) return false;

            foreach (var kvp in cost)
            {
                if (_balances.TryGetValue(kvp.Key, out var bal))
                {
                    bal.amount -= kvp.Value;
                }
            }

            OnResourcesChanged?.Invoke();
            return true;
        }

        public bool Spend(ResourceType type, double amount)
        {
            if (amount <= 0) return true;
            if (_balances.TryGetValue(type, out var bal))
            {
                if (bal.amount >= amount)
                {
                    bal.amount -= amount;
                    OnResourcesChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public void AddResource(ResourceType type, double amount)
        {
            if (_balances.TryGetValue(type, out var bal))
            {
                bal.amount = Math.Min(bal.capacity, bal.amount + amount);
                OnResourcesChanged?.Invoke();
            }
        }
    }
}
