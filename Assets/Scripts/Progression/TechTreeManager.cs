using System;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Economy;
using Gorkha.Audio;

namespace Gorkha.Progression
{
    public enum TechType
    {
        ForgedKhukuri,        // +20% Khukuri melee damage
        SalwoodBows,          // +15% Archer range & arrow speed
        HimalayanTerracing,   // +25% Farm production rate
        PavedSlateRoads,      // +20% Unit movement speed
        InterlockingTimber,   // +25% Building structural HP
        BronzeKamanRifling    // +25% Cannon explosive blast radius
    }

    [Serializable]
    public class TechItem
    {
        public TechType type;
        public string title;
        public string nepaliTitle;
        public string branch;
        public string description;
        public string iconEmoji;
        public int requiredAge = 1;
        public double foodCost;
        public double woodCost;
        public double stoneCost;
        public double goldCost;
        public float researchDuration = 15f; // seconds

        [NonSerialized] public bool isResearched = false;
        [NonSerialized] public bool isResearching = false;
        [NonSerialized] public float researchProgress = 0f; // 0 to 1
    }

    /// <summary>
    /// Age of Empires Academy / Vidya Kendra Tech Tree System (AGE-03).
    /// Manages technological research across military metallurgy, agrarian engineering,
    /// Himalayan architecture, and logistics, applying permanent multipliers across kingdom units & buildings.
    /// </summary>
    public class TechTreeManager : MonoBehaviour
    {
        public static TechTreeManager Instance { get; private set; }

        public event Action<TechType> OnTechResearched;
        public event Action<TechType> OnResearchStarted;

        private readonly Dictionary<TechType, TechItem> _techRegistry = new();
        private TechItem _currentResearch = null;
        private float _researchTimer = 0f;

        // Buff Multipliers
        public float MeleeDamageMultiplier => IsResearched(TechType.ForgedKhukuri) ? 1.20f : 1.0f;
        public float ArcherRangeMultiplier => IsResearched(TechType.SalwoodBows) ? 1.15f : 1.0f;
        public float FarmYieldMultiplier => IsResearched(TechType.HimalayanTerracing) ? 1.25f : 1.0f;
        public float MoveSpeedMultiplier => IsResearched(TechType.PavedSlateRoads) ? 1.20f : 1.0f;
        public float BuildingHpMultiplier => IsResearched(TechType.InterlockingTimber) ? 1.25f : 1.0f;
        public float CannonBlastMultiplier => IsResearched(TechType.BronzeKamanRifling) ? 1.25f : 1.0f;

        public TechItem CurrentResearch => _currentResearch;
        public float CurrentResearchRemaining => _currentResearch != null ? Mathf.Max(0f, _currentResearch.researchDuration - _researchTimer) : 0f;
        public float CurrentResearchProgress => _currentResearch != null && _currentResearch.researchDuration > 0f ? Mathf.Clamp01(_researchTimer / _currentResearch.researchDuration) : 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeRegistry();
        }

        private void InitializeRegistry()
        {
            RegisterTech(new TechItem
            {
                type = TechType.ForgedKhukuri,
                title = "Forged Khukuri Blades",
                nepaliTitle = "खुकुरी धार सुधार",
                branch = "⚔️ Military Metallurgy",
                description = "+20% Melee Damage for all Khukuri Warriors and Spearmen.",
                iconEmoji = "🗡️",
                requiredAge = 2,
                foodCost = 50,
                woodCost = 60,
                stoneCost = 40,
                goldCost = 25,
                researchDuration = 12f
            });

            RegisterTech(new TechItem
            {
                type = TechType.SalwoodBows,
                title = "Salwood Recurve Bows",
                nepaliTitle = "सालको धनुष",
                branch = "⚔️ Military Metallurgy",
                description = "+15% Range & Arrow Velocity for all Mountain Archers.",
                iconEmoji = "🏹",
                requiredAge = 2,
                foodCost = 40,
                woodCost = 80,
                stoneCost = 20,
                goldCost = 30,
                researchDuration = 14f
            });

            RegisterTech(new TechItem
            {
                type = TechType.HimalayanTerracing,
                title = "Himalayan Terracing",
                nepaliTitle = "गरा कान्ला खेती",
                branch = "🌾 Agrarian Engineering",
                description = "+25% Food production rate across all Terraced Farms.",
                iconEmoji = "🌾",
                requiredAge = 1,
                foodCost = 30,
                woodCost = 50,
                stoneCost = 30,
                goldCost = 0,
                researchDuration = 10f
            });

            RegisterTech(new TechItem
            {
                type = TechType.PavedSlateRoads,
                title = "Paved Slate Roads",
                nepaliTitle = "ढुङ्गे बाटो",
                branch = "🪵 Himalayan Logistics",
                description = "+20% Movement Speed for all civilian villagers and military units.",
                iconEmoji = "🪨",
                requiredAge = 2,
                foodCost = 25,
                woodCost = 40,
                stoneCost = 75,
                goldCost = 20,
                researchDuration = 12f
            });

            RegisterTech(new TechItem
            {
                type = TechType.InterlockingTimber,
                title = "Interlocking Timber Joists",
                nepaliTitle = "काठको थाम र चुकुल",
                branch = "🏰 Durbar Architecture",
                description = "+25% Maximum Health & Structural Resilience for all kingdom buildings.",
                iconEmoji = "🏯",
                requiredAge = 2,
                foodCost = 30,
                woodCost = 90,
                stoneCost = 60,
                goldCost = 15,
                researchDuration = 15f
            });

            RegisterTech(new TechItem
            {
                type = TechType.BronzeKamanRifling,
                title = "Bronze Cannon Rifling",
                nepaliTitle = "काँसको तोप सुधार",
                branch = "💣 Siege Ordnance",
                description = "+25% Explosive splash blast radius for heavy Kaman cannons.",
                iconEmoji = "💣",
                requiredAge = 3,
                foodCost = 50,
                woodCost = 70,
                stoneCost = 90,
                goldCost = 60,
                researchDuration = 18f
            });
        }

        private void RegisterTech(TechItem item)
        {
            _techRegistry[item.type] = item;
        }

        public IEnumerable<TechItem> GetAllTechs()
        {
            return _techRegistry.Values;
        }

        public TechItem GetTech(TechType type)
        {
            return _techRegistry.TryGetValue(type, out var item) ? item : null;
        }

        public bool IsResearched(TechType type)
        {
            return _techRegistry.TryGetValue(type, out var item) && item.isResearched;
        }

        public bool CanResearch(TechType type, out string reason)
        {
            if (!_techRegistry.TryGetValue(type, out var item))
            {
                reason = "Unknown technology.";
                return false;
            }

            if (item.isResearched)
            {
                reason = "Technology already researched.";
                return false;
            }

            if (_currentResearch != null)
            {
                reason = $"Academy is currently researching '{_currentResearch.title}'.";
                return false;
            }

            int currentAge = AgeManager.Instance != null ? AgeManager.Instance.CurrentAgeNumber : 1;
            if (currentAge < item.requiredAge)
            {
                reason = $"🔒 Locked! Requires Era {item.requiredAge} (Upgrade Gorkha Durbar).";
                return false;
            }

            var rm = ResourceManager.Instance;
            if (rm != null)
            {
                if (item.foodCost > 0 && rm.GetAmount(ResourceType.Food) < item.foodCost)
                {
                    reason = $"Insufficient Food! (Need {item.foodCost})";
                    return false;
                }
                if (item.woodCost > 0 && rm.GetAmount(ResourceType.Wood) < item.woodCost)
                {
                    reason = $"Insufficient Wood! (Need {item.woodCost})";
                    return false;
                }
                if (item.stoneCost > 0 && rm.GetAmount(ResourceType.Stone) < item.stoneCost)
                {
                    reason = $"Insufficient Stone! (Need {item.stoneCost})";
                    return false;
                }
                if (item.goldCost > 0 && rm.GetAmount(ResourceType.Gold) < item.goldCost)
                {
                    reason = $"Insufficient Gold! (Need {item.goldCost})";
                    return false;
                }
            }

            reason = null;
            return true;
        }

        public bool StartResearch(TechType type)
        {
            if (!CanResearch(type, out string reason))
            {
                Debug.LogWarning($"[TechTreeManager] Cannot research {type}: {reason}");
                return false;
            }

            var item = _techRegistry[type];
            var rm = ResourceManager.Instance;
            if (rm != null)
            {
                if (item.foodCost > 0) rm.Spend(ResourceType.Food, item.foodCost);
                if (item.woodCost > 0) rm.Spend(ResourceType.Wood, item.woodCost);
                if (item.stoneCost > 0) rm.Spend(ResourceType.Stone, item.stoneCost);
                if (item.goldCost > 0) rm.Spend(ResourceType.Gold, item.goldCost);
            }

            _currentResearch = item;
            _currentResearch.isResearching = true;
            _currentResearch.researchProgress = 0f;
            _researchTimer = 0f;

            GorkhaAudio.PlayBuildingPlaced();

            OnResearchStarted?.Invoke(type);
            Debug.Log($"[TechTreeManager] Started research on '{item.title}' ({item.researchDuration}s)");
            return true;
        }

        private void Update()
        {
            if (_currentResearch == null) return;

            _researchTimer += Time.deltaTime;
            _currentResearch.researchProgress = Mathf.Clamp01(_researchTimer / _currentResearch.researchDuration);

            if (_researchTimer >= _currentResearch.researchDuration)
            {
                CompleteCurrentResearch();
            }
        }

        private void CompleteCurrentResearch()
        {
            if (_currentResearch == null) return;

            var completed = _currentResearch;
            completed.isResearched = true;
            completed.isResearching = false;
            completed.researchProgress = 1f;
            _currentResearch = null;
            _researchTimer = 0f;

            GorkhaAudio.PlayEraFanfare();

            OnTechResearched?.Invoke(completed.type);
            Debug.Log($"[TechTreeManager] 🎓 Research complete: '{completed.title}'! All bonuses active.");
        }
    }
}
