using System;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Economy;
using Gorkha.Audio;
using Gorkha.UI;

namespace Gorkha.Progression
{
    public enum HistoricalAge
    {
        Age1_BaiseChaubisi = 1,
        Age2_FortifiedKot = 2,
        Age3_Unification = 3,
        Age4_HimalayanEmpire = 4
    }

    public struct AgeMetadata
    {
        public HistoricalAge Age;
        public int Level;
        public string EraCode;
        public string EraTitle;
        public string ShortName;
        public string HistoricYears;
        public string Subtitle;
        public string Description;
        public Color ThemeColor;
        public string[] KeyUnlocks;
        public Dictionary<ResourceType, double> UpgradeCost;
        public float UpgradeDurationSeconds;

        public string name => ShortName;
        public string historicalSpan => HistoricYears;
        public string description => Description;
        public Color themeColor => ThemeColor;
        public string unlockHighlights => KeyUnlocks != null ? string.Join(" • ", KeyUnlocks) : "";
    }

    /// <summary>
    /// Master Age & Era Progression Manager for Gorkha: Rise of the Kingdom (AGE-01 & AGE-02).
    /// Tracks the settlement's historical epoch based on Gorkha Durbar (Town Center) development:
    /// - Era I: Baise-Chaubisi Principality (1559–1740)
    /// - Era II: Fortified Kot Era (1740–1768)
    /// - Era III: Unification Era (1768–1814)
    /// - Era IV: Himalayan Empire Era (1814+)
    /// </summary>
    public class AgeManager : MonoBehaviour
    {
        private static AgeManager _instance;
        public static AgeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<AgeManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        GameObject go = new GameObject("AgeManager_Manager");
                        _instance = go.AddComponent<AgeManager>();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        public static event Action<HistoricalAge> OnAgeAdvanced;

        [Header("Current Historical Epoch")]
        [SerializeField] private HistoricalAge _currentAge = HistoricalAge.Age1_BaiseChaubisi;
        [SerializeField] private int _currentAgeLevel = 1;

        // Age Advancement Banner State
        private bool _showCelebrationBanner = false;
        private float _celebrationTimer = 0f;
        private string _celebrationTitle = "";
        private string _celebrationSubtitle = "";

        public HistoricalAge CurrentAge => _currentAge;
        public int CurrentAgeNumber => _currentAgeLevel;
        public int CurrentAgeIndex => _currentAgeLevel - 1;
        public AgeMetadata CurrentAgeData => GetCurrentAgeData();

        public AgeMetadata GetAge(int level)
        {
            int lvl = Mathf.Clamp(level, 1, 4);
            HistoricalAge age = (HistoricalAge)lvl;
            return Ages.TryGetValue(age, out var meta) ? meta : Ages[HistoricalAge.Age1_BaiseChaubisi];
        }

        public static readonly Dictionary<HistoricalAge, AgeMetadata> Ages = new()
        {
            {
                HistoricalAge.Age1_BaiseChaubisi,
                new AgeMetadata
                {
                    Age = HistoricalAge.Age1_BaiseChaubisi,
                    Level = 1,
                    EraCode = "Era I",
                    EraTitle = "Era I: Baise-Chaubisi Principality",
                    ShortName = "Era I (Principality)",
                    HistoricYears = "1559–1740",
                    Subtitle = "Founding of Gorkha — Timber, Thatch & Spear Militia",
                    Description = "King Dravya Shah establishes the Kingdom of Gorkha atop the Himalayan ridge. A rustic agrarian settlement defended by hardy hill spearmen.",
                    ThemeColor = new Color(0.85f, 0.75f, 0.45f),
                    KeyUnlocks = new[]
                    {
                        "Terraced Farm (Max 1)",
                        "Woodcutter Camp (Max 1)",
                        "Kot / Barracks (Max 1)",
                        "Newari Houses (Max 2)",
                        "Fortification Walls (Max 25)",
                        "Gorkha Spearmen & Peasants"
                    },
                    UpgradeCost = new Dictionary<ResourceType, double>
                    {
                        { ResourceType.Wood, 150 },
                        { ResourceType.Food, 100 }
                    },
                    UpgradeDurationSeconds = 8.0f
                }
            },
            {
                HistoricalAge.Age2_FortifiedKot,
                new AgeMetadata
                {
                    Age = HistoricalAge.Age2_FortifiedKot,
                    Level = 2,
                    EraCode = "Era II",
                    EraTitle = "Era II: Fortified Kot Era",
                    ShortName = "Era II (Fortified Kot)",
                    HistoricYears = "1740–1768",
                    Subtitle = "Kalu Pande Military Reforms — Stone Bastions, Khukuri Warriors & Granaries",
                    Description = "Grand Commander Kalu Pande reorganizes the Gorkhali military. Terracotta brick kilns fire Newari pagodas, deep slate quarries open, and deadly Khukuri shock troops muster.",
                    ThemeColor = new Color(0.95f, 0.55f, 0.25f),
                    KeyUnlocks = new[]
                    {
                        "Khukuri Shock Warriors (Barracks)",
                        "Archer Watchtower (Max 1)",
                        "Himalayan Stone Quarry (Max 1)",
                        "Granary / Bhakari (+1000 Food storage)",
                        "Timber Yard (+1000 Wood storage)",
                        "Concealed Spike Traps",
                        "Additional Farm (+1, Max 2)",
                        "Fortification Walls (+25, Max 50)"
                    },
                    UpgradeCost = new Dictionary<ResourceType, double>
                    {
                        { ResourceType.Wood, 300 },
                        { ResourceType.Stone, 250 },
                        { ResourceType.Gold, 150 }
                    },
                    UpgradeDurationSeconds = 15.0f
                }
            },
            {
                HistoricalAge.Age3_Unification,
                new AgeMetadata
                {
                    Age = HistoricalAge.Age3_Unification,
                    Level = 3,
                    EraCode = "Era III",
                    EraTitle = "Era III: Unification Era",
                    ShortName = "Era III (Unification)",
                    HistoricYears = "1768–1814",
                    Subtitle = "Prithvi Narayan Shah & Valley Conquest — Bronze Cannons & Mountain Archers",
                    Description = "The Great Unification. Prithvi Narayan Shah takes Nuwakot and the Kathmandu Valley. Bronze Kaman cannons roar from stone battlements, and disciplined mountain archers volley arrows.",
                    ThemeColor = new Color(1.0f, 0.84f, 0.0f),
                    KeyUnlocks = new[]
                    {
                        "Mountain Archers (Barracks)",
                        "Kaman (Heavy Mountain Cannon)",
                        "Second Military Barracks (Max 2)",
                        "Treasury Vault / Dhukuti (+1000 Gold cap)",
                        "Slate Depot (+1000 Stone cap)",
                        "Second Watchtower (Max 2)",
                        "Fortification Walls (+50, Max 100)",
                        "Newari Houses (Max 7)"
                    },
                    UpgradeCost = new Dictionary<ResourceType, double>
                    {
                        { ResourceType.Wood, 600 },
                        { ResourceType.Stone, 500 },
                        { ResourceType.Gold, 400 }
                    },
                    UpgradeDurationSeconds = 25.0f
                }
            },
            {
                HistoricalAge.Age4_HimalayanEmpire,
                new AgeMetadata
                {
                    Age = HistoricalAge.Age4_HimalayanEmpire,
                    Level = 4,
                    EraCode = "Era IV",
                    EraTitle = "Era IV: Himalayan Empire Era",
                    ShortName = "Era IV (Empire)",
                    HistoricYears = "1814+",
                    Subtitle = "Imperial Mountain Citadel — Golden Spires, Musket Regiments & Wonder Foundation",
                    Description = "The pinnacle of Gorkha sovereignty spanning the Himalayas. Grand imperial mountain citadels, gilded golden Gajur temples, and the foundation of Himalayan wonders.",
                    ThemeColor = new Color(0.45f, 0.85f, 1.0f),
                    KeyUnlocks = new[]
                    {
                        "Imperial Citadel Palace (4-Tier Durbar)",
                        "Wonder of the World Foundation",
                        "Maximum Fortification Walls (Max 200)",
                        "Maximum Watchtowers (Max 4)",
                        "Maximum Population Houses (Max 10)"
                    },
                    UpgradeCost = new Dictionary<ResourceType, double>(),
                    UpgradeDurationSeconds = 0f
                }
            }
        };

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Update()
        {
            if (_showCelebrationBanner)
            {
                _celebrationTimer -= Time.deltaTime;
                if (_celebrationTimer <= 0f)
                {
                    _showCelebrationBanner = false;
                }
            }
        }

        public AgeMetadata GetCurrentAgeData()
        {
            return Ages.TryGetValue(_currentAge, out var meta) ? meta : Ages[HistoricalAge.Age1_BaiseChaubisi];
        }

        public bool TryGetNextAgeData(out AgeMetadata nextAge)
        {
            int nextLevel = _currentAgeLevel + 1;
            if (Enum.IsDefined(typeof(HistoricalAge), nextLevel))
            {
                nextAge = Ages[(HistoricalAge)nextLevel];
                return true;
            }
            nextAge = default;
            return false;
        }

        /// <summary>
        /// Called automatically when the Town Center (Gorkha Durbar) completes an upgrade.
        /// </summary>
        public void OnTownCenterUpgraded(int newLevel)
        {
            int targetAgeLevel = Mathf.Clamp(newLevel, 1, 4);
            if (targetAgeLevel != _currentAgeLevel)
            {
                AdvanceToAge((HistoricalAge)targetAgeLevel);
            }
        }

        public void AdvanceToAge(HistoricalAge newAge)
        {
            _currentAge = newAge;
            _currentAgeLevel = (int)newAge;

            var meta = GetCurrentAgeData();

            // Trigger visual & audio celebrations
            _celebrationTitle = $"🌟 ERA ADVANCEMENT 🌟";
            _celebrationSubtitle = $"{meta.EraTitle} ({meta.HistoricYears})";
            _showCelebrationBanner = true;
            _celebrationTimer = 7.0f;

            GorkhaAudio.PlayUpgrade();
            GorkhaAudio.PlayHorn();

            // Broadcast event to refresh buildings, units, and UI
            OnAgeAdvanced?.Invoke(_currentAge);

            Debug.Log($"<color=#FFD700><b>[AgeManager]</b> Kingdom successfully advanced to {meta.EraTitle}!</color>");
        }

        public void DrawCelebrationOverlay(float screenWidth, float screenHeight)
        {
            if (!_showCelebrationBanner) return;

            float w = Mathf.Min(screenWidth - 40f, 680f);
            float h = 130f;
            float x = (screenWidth - w) * 0.5f;
            float y = 90f;

            Rect bannerRect = new Rect(x, y, w, h);
            GUI.Box(bannerRect, GUIContent.none);

            GUILayout.BeginArea(bannerRect);
            GUILayout.BeginVertical();
            GUILayout.Space(12);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1.0f, 0.85f, 0.20f);

            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            subStyle.normal.textColor = Color.white;

            GUIStyle descStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
            descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            GUILayout.Label(_celebrationTitle, titleStyle);
            GUILayout.Label(_celebrationSubtitle, subStyle);
            GUILayout.Label(GetCurrentAgeData().Subtitle, descStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
