using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Buildings;
using Gorkha.Economy;
using Gorkha.Units;
using Gorkha.Audio;
using Gorkha.Graphics;
using Gorkha.CameraSystem;
using Gorkha.Core;
using Gorkha.UI;

namespace Gorkha.Combat
{
    public enum RaidState
    {
        Idle,
        ChoosingOutpost,
        BattleActive,
        VictorySummary
    }

    public struct RaidOutpostConfig
    {
        public int Tier;
        public string OutpostName;
        public string Description;
        public int FoodReward;
        public int GoldReward;
        public int GemReward;
        public int DefenderCount;
    }

    /// <summary>
    /// Clash of Clans Style Asynchronous Village Raid Battle Controller (CBT-05).
    /// Features:
    /// - 3 Outpost Difficulty Tiers with distinct fortifications and loot pools.
    /// - Procedural hostile outpost generation with enemy Town Center, watchtowers, and storages.
    /// - CoC 3-Star Victory Evaluation:
    ///     * Star 1: 50% Destruction of Outpost
    ///     * Star 2: Destruction of Enemy Town Center Keep
    ///     * Star 3: 100% Total Obliteration
    /// - Real-time loot plundering, destruction percentage gauge, and battle countdown timer.
    /// - Post-battle victory fanfare modal with animated stars and resource deposit.
    /// </summary>
    public class GorkhaRaidBattleController : MonoBehaviour
    {
        private static GorkhaRaidBattleController _instance;
        public static GorkhaRaidBattleController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<GorkhaRaidBattleController>();
                    if (_instance == null && Application.isPlaying)
                    {
                        GameObject go = new GameObject("GorkhaRaidBattleController_Manager");
                        _instance = go.AddComponent<GorkhaRaidBattleController>();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        public RaidState State { get; private set; } = RaidState.Idle;
        public bool IsRaidActive => State == RaidState.BattleActive;

        // Active Raid Metrics
        private RaidOutpostConfig _currentOutpost;
        private readonly List<BuildingInstance> _hostileBuildings = new();
        private readonly List<UnitController> _hostileDefenders = new();
        private BuildingInstance _enemyTownCenter;
        private int _totalBuildingCount = 0;
        private int _destroyedBuildingCount = 0;

        private double _plunderedFood = 0;
        private double _plunderedGold = 0;
        private int _plunderedGems = 0;

        private float _battleTimer = 180f; // 3-minute battle clock
        private bool _star1Earned = false; // 50% damage
        private bool _star2Earned = false; // Town Center destroyed
        private bool _star3Earned = false; // 100% destruction

        // UI Textures & Styles
        private Texture2D _raidOverlayBg;
        private Texture2D _progressBg;
        private Texture2D _progressFill;
        private GUIStyle _raidHeaderStyle;
        private GUIStyle _raidSubStyle;
        private GUIStyle _raidCardStyle;
        private GUIStyle _starEarnedStyle;
        private GUIStyle _starEmptyStyle;

        private readonly RaidOutpostConfig[] _availableOutposts = new[]
        {
            new RaidOutpostConfig
            {
                Tier = 1,
                OutpostName = "Rival Chaubisi Kot",
                Description = "A frontier hill encampment. Light fortifications and undefended granaries.",
                FoodReward = 600,
                GoldReward = 450,
                GemReward = 5,
                DefenderCount = 4
            },
            new RaidOutpostConfig
            {
                Tier = 2,
                OutpostName = "Bandit Mountain Stronghold",
                Description = "A fortified Kot perched on mountain terraces with watchtowers and mortar cannons.",
                FoodReward = 1600,
                GoldReward = 1200,
                GemReward = 12,
                DefenderCount = 8
            },
            new RaidOutpostConfig
            {
                Tier = 3,
                OutpostName = "Valley Warlord's Citadel",
                Description = "A formidable mountain citadel housing a royal treasury vault and elite garrison.",
                FoodReward = 3800,
                GoldReward = 3000,
                GemReward = 25,
                DefenderCount = 14
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

        private void OnEnable()
        {
            BuildingInstance.OnBuildingDestroyed += HandleBuildingDestroyed;
        }

        private void OnDisable()
        {
            BuildingInstance.OnBuildingDestroyed -= HandleBuildingDestroyed;
        }

        private void Update()
        {
            if (State == RaidState.BattleActive)
            {
                _battleTimer -= Time.deltaTime;
                if (_battleTimer <= 0f)
                {
                    EndBattle(timeExpired: true);
                }
            }
        }

        public void OpenOutpostSelectionMenu()
        {
            State = RaidState.ChoosingOutpost;
            GorkhaAudio.PlayClick();
        }

        public void CloseOutpostSelectionMenu()
        {
            if (State == RaidState.ChoosingOutpost)
            {
                State = RaidState.Idle;
                GorkhaAudio.PlayClick();
            }
        }

        public void LaunchRaid(int outpostIndex)
        {
            if (outpostIndex < 0 || outpostIndex >= _availableOutposts.Length) return;
            _currentOutpost = _availableOutposts[outpostIndex];

            // Cleanup any previous raid debris
            CleanupRaidEntities();

            State = RaidState.BattleActive;
            _battleTimer = 180f;
            _plunderedFood = 0;
            _plunderedGold = 0;
            _plunderedGems = 0;
            _star1Earned = false;
            _star2Earned = false;
            _star3Earned = false;
            _destroyedBuildingCount = 0;

            // Break player's peace shield upon launching offensive invasion
            if (ResourceManager.Instance != null && ResourceManager.Instance.HasActiveShield)
            {
                ResourceManager.Instance.BreakShield();
                GorkhaPlayableHUD.SpawnFloatingText(new Vector3(24f, 0, 24f) + Vector3.up * 4.5f, "🛡️ Shield Broken by War Expedition!", Color.red);
            }

            BuildHostileOutpost(_currentOutpost);

            GorkhaAudio.PlayHorn(1.0f);
            GorkhaAudio.PlayCannon();
            GorkhaPlayableHUD.SpawnFloatingText(new Vector3(40f, 0, 40f) + Vector3.up * 4f, $"⚔️ RAID LAUNCHED: {_currentOutpost.OutpostName}!", Color.red);

            // Focus camera smoothly on the hostile outpost
            if (IsometricCameraController.Instance != null && _enemyTownCenter != null)
            {
                IsometricCameraController.Instance.FocusOn(_enemyTownCenter.transform.position);
            }
        }

        private void BuildHostileOutpost(RaidOutpostConfig config)
        {
            _hostileBuildings.Clear();
            _hostileDefenders.Clear();

            // Outpost center coordinates placed on the north-east mountain territory (Col: 36, Row: 36)
            int centerCol = 36;
            int centerRow = 36;

            // 1. Enemy Town Center Keep
            _enemyTownCenter = SpawnHostileBuilding(BuildingType.TownCenter, "Enemy Chieftain Keep", centerCol, centerRow, 2, 2, 600 * config.Tier);
            _hostileBuildings.Add(_enemyTownCenter);

            // 2. Hostile Defenses (Watchtowers & Cannons)
            int towerCount = config.Tier;
            (int x, int y)[] towerOffsets = { (-4, -4), (4, -4), (-4, 4), (4, 4) };
            for (int i = 0; i < towerCount && i < towerOffsets.Length; i++)
            {
                var t = SpawnHostileBuilding(BuildingType.Watchtower, "Hostile Watchtower", centerCol + towerOffsets[i].x, centerRow + towerOffsets[i].y, 2, 2, 300 * config.Tier);
                _hostileBuildings.Add(t);
            }

            if (config.Tier >= 2)
            {
                var c = SpawnHostileBuilding(BuildingType.Kaman, "Hostile Kaman Cannon", centerCol, centerRow + 5, 2, 2, 350 * config.Tier);
                _hostileBuildings.Add(c);
            }

            // 3. Lootable Resource Storages (Granaries & Vaults)
            var g = SpawnHostileBuilding(BuildingType.Granary, "Lootable Granary", centerCol - 5, centerRow, 2, 2, 280 * config.Tier);
            _hostileBuildings.Add(g);

            var v = SpawnHostileBuilding(BuildingType.Vault, "Lootable Treasury", centerCol + 5, centerRow, 2, 2, 280 * config.Tier);
            _hostileBuildings.Add(v);

            if (config.Tier >= 3)
            {
                var b = SpawnHostileBuilding(BuildingType.Barracks, "Hostile Kot Barracks", centerCol - 3, centerRow - 6, 2, 2, 400);
                _hostileBuildings.Add(b);
            }

            _totalBuildingCount = _hostileBuildings.Count;

            // 4. Hostile Outpost Garrison Patrols
            Vector3 centerPos = _enemyTownCenter.transform.position;
            for (int i = 0; i < config.DefenderCount; i++)
            {
                float angle = i * (Mathf.PI * 2f / config.DefenderCount);
                Vector3 spawnPos = centerPos + new Vector3(Mathf.Cos(angle) * 6f, 0, Mathf.Sin(angle) * 6f);

                GameObject banditObj = new GameObject($"HostileGarrison_{i}");
                banditObj.transform.position = spawnPos;
                var unit = banditObj.AddComponent<UnitController>();

                UnitType uType = (i % 3 == 0) ? UnitType.Archer : (i % 3 == 1 ? UnitType.Spearman : UnitType.KhukuriWarrior);
                unit.SetUnitType(uType);
                unit.SetHostile(true);

                _hostileDefenders.Add(unit);
                GorkhaVFX.SpawnPlacementDust(spawnPos, Vector2.one * 1.0f);
            }
        }

        private BuildingInstance SpawnHostileBuilding(BuildingType type, string displayName, int col, int row, int width, int height, int health)
        {
            Vector3 worldPos = IsometricGrid.Instance != null
                ? IsometricGrid.Instance.GridToWorldPosition(col, row, width, height)
                : new Vector3(col * 2f, 0, row * 2f);

            GameObject obj = new GameObject(displayName);
            obj.transform.position = worldPos;

            BuildingData data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingType = type;
            data.displayName = displayName;
            data.tileWidth = width;
            data.tileHeight = height;
            data.hasProduction = false;
            data.constructionDurationSeconds = 1.0f;

            BuildingInstance instance = obj.AddComponent<BuildingInstance>();
            instance.Initialize(data, col, row, startAsConstructing: false);
            instance.SetHostile(true);

            // Re-tint mesh with hostile dark slate / crimson tones
            foreach (var ren in obj.GetComponentsInChildren<Renderer>())
            {
                if (ren.material != null && ren.material.HasProperty("_Color"))
                {
                    ren.material.color = Color.Lerp(ren.material.color, new Color(0.75f, 0.25f, 0.25f), 0.35f);
                }
            }

            return instance;
        }

        private void HandleBuildingDestroyed(BuildingInstance building)
        {
            if (State != RaidState.BattleActive || building == null || !building.IsHostile) return;
            if (!_hostileBuildings.Contains(building)) return;

            _destroyedBuildingCount++;

            // Plunder loot fraction
            float share = 1.0f / Mathf.Max(1, _totalBuildingCount);
            double foodLoot = _currentOutpost.FoodReward * share;
            double goldLoot = _currentOutpost.GoldReward * share;

            _plunderedFood += foodLoot;
            _plunderedGold += goldLoot;

            GorkhaAudio.PlayHarvest();
            GorkhaPlayableHUD.SpawnFloatingText(building.transform.position + Vector3.up * 3.5f, $"+{(int)foodLoot}🌾 +{(int)goldLoot}🪙 Plundered!", Color.yellow);

            // Evaluate Stars
            float pct = GetDestructionPercentage();

            // Star 1: 50% Destruction
            if (pct >= 50f && !_star1Earned)
            {
                _star1Earned = true;
                AwardStarFanfare(1, "⭐ 1st Star: 50% Outpost Destruction!");
            }

            // Star 2: Town Center Destroyed
            if (building == _enemyTownCenter && !_star2Earned)
            {
                _star2Earned = true;
                AwardStarFanfare(2, "⭐⭐ 2nd Star: Enemy Chieftain Keep Destroyed!");
            }

            // Star 3: 100% Destruction
            if (pct >= 99.5f && !_star3Earned)
            {
                _star3Earned = true;
                _plunderedGems = _currentOutpost.GemReward;
                AwardStarFanfare(3, "⭐⭐⭐ 3rd Star: TOTAL OUTPOST OBLITERATION!");
                EndBattle(timeExpired: false);
            }
        }

        private void AwardStarFanfare(int starNum, string bannerText)
        {
            GorkhaAudio.PlayUpgrade();
            GorkhaPlayableHUD.SpawnFloatingText(_enemyTownCenter != null ? _enemyTownCenter.transform.position + Vector3.up * 4.5f : Vector3.zero, bannerText, Color.yellow);
        }

        public float GetDestructionPercentage()
        {
            if (_totalBuildingCount <= 0) return 0f;
            return Mathf.Clamp01((float)_destroyedBuildingCount / _totalBuildingCount) * 100f;
        }

        public int GetEarnedStarsCount()
        {
            int s = 0;
            if (_star1Earned) s++;
            if (_star2Earned) s++;
            if (_star3Earned) s++;
            return s;
        }

        public void EndBattle(bool timeExpired = false)
        {
            if (State != RaidState.BattleActive) return;

            State = RaidState.VictorySummary;
            GorkhaAudio.PlayUpgrade();

            if (GetEarnedStarsCount() > 0)
            {
                GorkhaAudio.PlayHorn(1.0f);
            }
            else
            {
                GorkhaAudio.PlayError();
            }
        }

        public void FinalizeRaidAndReturnToVillage()
        {
            // Deposit all plundered loot into player's Kingdom storages
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(ResourceType.Food, _plunderedFood);
                ResourceManager.Instance.AddResource(ResourceType.Gold, _plunderedGold);
                if (_plunderedGems > 0)
                {
                    ResourceManager.Instance.AddResource(ResourceType.Gems, _plunderedGems);
                }
            }

            CleanupRaidEntities();
            State = RaidState.Idle;

            // Return camera view to village Town Center
            if (IsometricCameraController.Instance != null)
            {
                IsometricCameraController.Instance.ResetView();
            }

            GorkhaAudio.PlayClick();
            GorkhaPlayableHUD.SpawnFloatingText(new Vector3(24f, 0, 24f) + Vector3.up * 3f, "Expedition Returned to Gorkha Kingdom!", Color.green);
        }

        private void CleanupRaidEntities()
        {
            foreach (var b in _hostileBuildings)
            {
                if (b != null) Destroy(b.gameObject);
            }
            _hostileBuildings.Clear();

            foreach (var u in _hostileDefenders)
            {
                if (u != null) Destroy(u.gameObject);
            }
            _hostileDefenders.Clear();

            _enemyTownCenter = null;
        }

        #region GUI Rendering

        private void InitGUIStyles()
        {
            if (_raidOverlayBg == null)
            {
                _raidOverlayBg = MakeSolidTex(16, 16, new Color(0.08f, 0.08f, 0.10f, 0.88f));
                _progressBg = MakeSolidTex(16, 16, new Color(0.18f, 0.18f, 0.20f, 1.0f));
                _progressFill = MakeSolidTex(16, 16, new Color(1.0f, 0.78f, 0.15f, 1.0f));

                _raidHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                _raidHeaderStyle.normal.textColor = new Color(1.0f, 0.92f, 0.65f);

                _raidSubStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleCenter
                };
                _raidSubStyle.normal.textColor = Color.white;

                _raidCardStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(16, 16, 14, 14)
                };
                _raidCardStyle.normal.background = _raidOverlayBg;

                _starEarnedStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 28,
                    alignment = TextAnchor.MiddleCenter
                };
                _starEarnedStyle.normal.textColor = new Color(1.0f, 0.85f, 0.10f);

                _starEmptyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 28,
                    alignment = TextAnchor.MiddleCenter
                };
                _starEmptyStyle.normal.textColor = new Color(0.35f, 0.35f, 0.40f);
            }
        }

        private Texture2D MakeSolidTex(int w, int h, Color col)
        {
            Texture2D tex = new Texture2D(w, h);
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            InitGUIStyles();

            if (State == RaidState.ChoosingOutpost)
            {
                DrawOutpostSelectionDialog();
            }
            else if (State == RaidState.BattleActive)
            {
                DrawActiveRaidHUD();
            }
            else if (State == RaidState.VictorySummary)
            {
                DrawPostBattleVictoryModal();
            }
        }

        private void DrawOutpostSelectionDialog()
        {
            float w = 680f;
            float h = 420f;
            Rect modalRect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);

            GUILayout.BeginArea(modalRect);
            GUILayout.BeginVertical(_raidCardStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("⚔️ <b>Select Hostile Outpost to Raid (CoC Expedition)</b>", _raidHeaderStyle);
            if (GUILayout.Button("✖", GUILayout.Width(30), GUILayout.Height(28)))
            {
                CloseOutpostSelectionMenu();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label("<color=#CCCCCC>Command your Gorkha Army to assault rival fortifications. Plunder storages for massive Gold and Food!</color>", _raidSubStyle);
            GUILayout.Space(12);

            for (int i = 0; i < _availableOutposts.Length; i++)
            {
                var outpost = _availableOutposts[i];
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();

                string tierBadge = outpost.Tier == 1 ? "<color=#A0E0A0>[Tier 1: Easy]</color>" : (outpost.Tier == 2 ? "<color=#FFD700>[Tier 2: Medium]</color>" : "<color=#FF6666>[Tier 3: Hard]</color>");
                GUILayout.Label($"🏰 <b>{outpost.OutpostName}</b> {tierBadge}\n<size=11><color=#BBBBBB>{outpost.Description}</color></size>", _raidSubStyle, GUILayout.ExpandWidth(true));

                GUILayout.BeginVertical(GUILayout.Width(170));
                GUILayout.Label($"💰 <b>+{outpost.GoldReward}</b> 🪙  <b>+{outpost.FoodReward}</b> 🌾\n💎 <b>+{outpost.GemReward} Gems</b>", _raidSubStyle);
                if (GUILayout.Button("⚔️ Launch Raid!", GUILayout.Height(30)))
                {
                    LaunchRaid(i);
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(6);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawActiveRaidHUD()
        {
            float bannerW = 640f;
            float bannerH = 76f;
            Rect hudRect = new Rect((Screen.width - bannerW) * 0.5f, 10f, bannerW, bannerH);

            GUILayout.BeginArea(hudRect);
            GUILayout.BeginHorizontal(_raidCardStyle);

            // Left: Stars & Destruction Percentage
            GUILayout.BeginVertical(GUILayout.Width(190));
            GUILayout.BeginHorizontal();
            GUILayout.Label(_star1Earned ? "★" : "☆", _star1Earned ? _starEarnedStyle : _starEmptyStyle, GUILayout.Width(30));
            GUILayout.Label(_star2Earned ? "★" : "☆", _star2Earned ? _starEarnedStyle : _starEmptyStyle, GUILayout.Width(30));
            GUILayout.Label(_star3Earned ? "★" : "☆", _star3Earned ? _starEarnedStyle : _starEmptyStyle, GUILayout.Width(30));
            GUILayout.Label($"<b>{(int)GetDestructionPercentage()}%</b>", _raidHeaderStyle);
            GUILayout.EndHorizontal();

            // Destruction Gauge
            Rect gaugeRect = GUILayoutUtility.GetRect(180, 8);
            GUI.DrawTexture(gaugeRect, _progressBg);
            float fillW = gaugeRect.width * (GetDestructionPercentage() / 100f);
            if (fillW > 0.1f)
            {
                GUI.DrawTexture(new Rect(gaugeRect.x, gaugeRect.y, fillW, gaugeRect.height), _progressFill);
            }
            GUILayout.EndVertical();

            GUILayout.Space(12);

            // Middle: Loot Plundered
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label($"<b>{_currentOutpost.OutpostName}</b>", _raidHeaderStyle);
            GUILayout.Label($"Loot: <color=#FFD700>🪙 {(int)_plunderedGold}</color>  •  <color=#99FF99>🌾 {(int)_plunderedFood}</color>", _raidSubStyle);
            GUILayout.EndVertical();

            GUILayout.Space(12);

            // Right: Countdown & Retreat Button
            int secs = Mathf.Max(0, Mathf.CeilToInt(_battleTimer));
            int mins = secs / 60;
            int remSecs = secs % 60;

            GUILayout.BeginVertical(GUILayout.Width(110));
            GUILayout.Label($"⏳ <b>{mins}:{remSecs:D2}</b>", _raidHeaderStyle);
            if (GUILayout.Button("🏳️ End Battle", GUILayout.Height(24)))
            {
                EndBattle(timeExpired: false);
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawPostBattleVictoryModal()
        {
            float modalW = 520f;
            float modalH = 340f;
            Rect modalRect = new Rect((Screen.width - modalW) * 0.5f, (Screen.height - modalH) * 0.5f, modalW, modalH);

            GUILayout.BeginArea(modalRect);
            GUILayout.BeginVertical(_raidCardStyle);

            int stars = GetEarnedStarsCount();
            string victoryTitle = stars switch
            {
                3 => "🏆 ⭐⭐⭐ PERFECT 3-STAR VICTORY! ⭐⭐⭐",
                2 => "⚔️ ⭐⭐ 2-STAR VICTORY: Keep Fallen! ⭐⭐",
                1 => "🛡️ ⭐ 1-STAR VICTORY: Outpost Pillaged! ⭐",
                _ => "❌ DEFEAT: Outpost Survived"
            };

            GUILayout.Label(victoryTitle, _raidHeaderStyle);
            GUILayout.Space(8);

            // 3 Large Stars Row
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(stars >= 1 ? "★" : "☆", stars >= 1 ? _starEarnedStyle : _starEmptyStyle, GUILayout.Width(45));
            GUILayout.Label(stars >= 2 ? "★" : "☆", stars >= 2 ? _starEarnedStyle : _starEmptyStyle, GUILayout.Width(45));
            GUILayout.Label(stars >= 3 ? "★" : "☆", stars >= 3 ? _starEarnedStyle : _starEmptyStyle, GUILayout.Width(45));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label($"Total Outpost Destruction: <b>{(int)GetDestructionPercentage()}%</b>", _raidSubStyle);
            GUILayout.Space(10);

            // Plundered Resources Tally Box
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>Plundered Spoils Deposited into Kingdom:</b>", _raidSubStyle);
            GUILayout.Label($"🌾 Food Plundered: <b>+{(int)_plunderedFood}</b>", _raidSubStyle);
            GUILayout.Label($"🪙 Gold Plundered: <b>+{(int)_plunderedGold}</b>", _raidSubStyle);
            if (_plunderedGems > 0)
            {
                GUILayout.Label($"💎 Bonus Gems: <b>+{_plunderedGems}</b>", _raidSubStyle);
            }
            GUILayout.EndVertical();

            GUILayout.Space(14);
            if (GUILayout.Button("🏰 <b>Return to Village & Collect Spoils</b>", GUILayout.Height(38)))
            {
                FinalizeRaidAndReturnToVillage();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}
