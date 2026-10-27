using System;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Core;
using Gorkha.Economy;
using Gorkha.Buildings;
using Gorkha.CameraSystem;
using Gorkha.Graphics;
using Gorkha.Audio;
using Gorkha.Units;
using Gorkha.Progression;
using Gorkha.Combat;

namespace Gorkha.UI
{
    public class FloatingTextItem
    {
        public Vector3 WorldPos;
        public string Text;
        public Color TextColor;
        public float Timer;
        public float Duration;
    }

    /// <summary>
    /// Master Studio Playable HUD and Interaction Controller for Gorkha: Rise of the Kingdom.
    /// Professional Clash of Clans / Age of Empires style interface:
    /// 1. Top Kingdom Resource Bar (Live counters, capacity fill gauges & rates)
    /// 2. Tactile 3D Building Selection Ring & Bounce Hop
    /// 3. Bottom-Center Floating Building Details & Upgrade Modal (with animated countdowns & Gem rush)
    /// 4. Barracks Military Recruitment Dock (Spearmen, Khukuri warriors, Archers)
    /// 5. Bottom Construction Dock with styled cards, cost badges, and hover highlights
    /// 6. Floating animated harvest/level-up numbers with drop shadows
    /// </summary>
    public class GorkhaPlayableHUD : MonoBehaviour
    {
        public static GorkhaPlayableHUD Instance { get; private set; }

        private static float _userScaleMultiplier = 1.25f; // Default 1.25x for clear, comfortable readability
        public static float UserScaleMultiplier
        {
            get => _userScaleMultiplier;
            set
            {
                _userScaleMultiplier = Mathf.Clamp(value, 0.75f, 2.5f);
                PlayerPrefs.SetFloat("Gorkha_UIScale", _userScaleMultiplier);
            }
        }

        /// <summary>
        /// Calculates the DPI and resolution-responsive GUI scale factor.
        /// Ensures consistent physical proportions on standard 1080p, 1440p, 4K, and Mac Retina displays.
        /// </summary>
        public static float GetEffectiveScale()
        {
            float autoScale = Mathf.Clamp(Screen.height / 800f, 1.0f, 2.5f);
            return autoScale * _userScaleMultiplier;
        }

        public static void CycleUserScale()
        {
            if (_userScaleMultiplier < 1.15f) UserScaleMultiplier = 1.25f;
            else if (_userScaleMultiplier < 1.4f) UserScaleMultiplier = 1.50f;
            else if (_userScaleMultiplier < 1.65f) UserScaleMultiplier = 1.75f;
            else if (_userScaleMultiplier < 1.9f) UserScaleMultiplier = 2.0f;
            else UserScaleMultiplier = 1.0f;
        }

        private BuildingInstance _selectedBuilding;
        private UnitController _selectedUnit;
        private readonly List<FloatingTextItem> _floatingTexts = new();
        private float _lastClickTime = 0f;
        private int _selectedBuildCategory = 0; // 0 = Resources & Storage, 1 = Military & Defenses

        // Custom GUI Textures
        private Texture2D _cardBgTex;
        private Texture2D _btnNormalTex;
        private Texture2D _btnHoverTex;
        private Texture2D _btnGoldNormalTex;
        private Texture2D _btnGoldHoverTex;
        private Texture2D _btnGreenNormalTex;
        private Texture2D _btnGreenHoverTex;
        private Texture2D _btnDangerTex;
        private Texture2D _progressBarBgTex;
        private Texture2D _progressBarFillTex;

        // Custom GUI Styles
        private GUIStyle _headerStyle;
        private GUIStyle _subHeaderStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _goldButtonStyle;
        private GUIStyle _greenButtonStyle;
        private GUIStyle _dangerButtonStyle;
        private GUIStyle _floaterStyle;
        private GUIStyle _dockCardStyle;
        private bool _stylesInitialized = false;

        private Rect _inspectorRect;
        private Rect _topBannerRect;
        private Rect _bottomDockRect;
        private Rect _unitInspectorRect;
        private bool _showTechModal = false;
        private Rect _techModalRect;
        private bool _showGuthiModal = false;
        private int _guthiTab = 0;
        private Rect _guthiModalRect;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _userScaleMultiplier = PlayerPrefs.GetFloat("Gorkha_UIScale", 1.25f);
            BuildingInstance.OnBuildingSelected += HandleBuildingSelected;
        }

        private void OnDestroy()
        {
            BuildingInstance.OnBuildingSelected -= HandleBuildingSelected;
        }

        private void HandleBuildingSelected(BuildingInstance building)
        {
            if (_selectedBuilding != null && _selectedBuilding != building)
            {
                _selectedBuilding.Deselect();
            }
            _selectedBuilding = building;
            _selectedUnit = null;
            RTSSelectionManager.Instance?.ClearSelection();
        }

        public void DeselectBuilding()
        {
            if (_selectedBuilding != null)
            {
                _selectedBuilding.Deselect();
                _selectedBuilding = null;
            }
        }

        public bool IsPointerOverHUD(Vector2 screenPos)
        {
            float scale = GetEffectiveScale();
            Vector2 guiMousePos = new Vector2(screenPos.x / scale, (Screen.height - screenPos.y) / scale);
            if (_topBannerRect.Contains(guiMousePos) ||
                _bottomDockRect.Contains(guiMousePos) ||
                (_selectedBuilding != null && _inspectorRect.Contains(guiMousePos)) ||
                (RTSSelectionManager.Instance != null && RTSSelectionManager.Instance.SelectedUnits.Count > 0 && _unitInspectorRect.Contains(guiMousePos)) ||
                (_showTechModal && _techModalRect.Contains(guiMousePos)) ||
                (_showGuthiModal && _guthiModalRect.Contains(guiMousePos)) ||
                (GorkhaMiniMap.Instance != null && GorkhaMiniMap.Instance.MapRect.Contains(guiMousePos)))
            {
                return true;
            }
            return false;
        }

        public static void SpawnFloatingText(Vector3 worldPos, string text, Color color)
        {
            if (Instance != null)
            {
                Instance._floatingTexts.Add(new FloatingTextItem
                {
                    WorldPos = worldPos,
                    Text = text,
                    TextColor = color,
                    Timer = 0f,
                    Duration = 1.35f
                });
            }
        }

        private void Update()
        {
            Handle3DMouseInteractions();
            UpdateFloatingTexts();
            HandleHeroAbilityHotkeys();
        }

        private void HandleHeroAbilityHotkeys()
        {
            if (RTSSelectionManager.Instance == null) return;
            var selected = RTSSelectionManager.Instance.SelectedUnits;
            if (selected.Count == 1 && selected[0] != null && selected[0].TryGetComponent<Units.HeroController>(out var hero))
            {
                if (GorkhaInput.GetKeyDown(KeyCode.Q))
                {
                    hero.TriggerAbility1();
                }
                else if (GorkhaInput.GetKeyDown(KeyCode.E) || GorkhaInput.GetKeyDown(KeyCode.W) || GorkhaInput.GetKeyDown(KeyCode.R))
                {
                    hero.TriggerAbility2();
                }
            }
        }

        private void Handle3DMouseInteractions()
        {
            // If currently placing a blueprint, placement controller handles clicks
            if (BuildingPlacementController.Instance != null && BuildingPlacementController.Instance.IsPlacing)
            {
                return;
            }

            // Check for Left Click
            if (GorkhaInput.GetMouseButtonDown(0))
            {
                Vector2 mousePos = GorkhaInput.MousePosition;
                Vector2 guiMousePos = new Vector2(mousePos.x, Screen.height - mousePos.y);

                // If clicked inside UI areas, ignore 3D raycast
                if (IsPointerOverHUD(mousePos))
                {
                    return;
                }

                if (Camera.main == null) return;
                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit, 200f))
                {
                    // 1. Check if clicked a harvest bubble directly
                    if (hit.collider.name.Contains("Bubble") || hit.collider.transform.parent != null && hit.collider.transform.parent.name.Contains("Bubble"))
                    {
                        var b = hit.collider.GetComponentInParent<BuildingInstance>();
                        if (b != null && b.State == BuildingState.Active && b.Data != null && b.Data.hasProduction)
                        {
                            double amt = b.CollectResource();
                            SpawnFloatingText(b.transform.position + Vector3.up * 3.5f, $"+{(int)amt} {b.Data.productionType}", new Color(1f, 0.9f, 0.2f));
                            return;
                        }
                    }

                    // 2. Check if clicked a harvestable Resource Node (AoE Villager work loop)
                    var resNode = hit.collider.GetComponentInParent<ResourceNode>();
                    if (resNode != null && !resNode.IsDepleted)
                    {
                        var villager = UnitController.FindNearestIdleVillager(resNode.transform.position);
                        if (villager != null)
                        {
                            villager.AssignHarvestTask(resNode);
                            string actionVerb = resNode.NodeType == ResourceNodeType.Tree ? "🪓 Chopping Timber" : "⛏️ Mining Slate";
                            SpawnFloatingText(resNode.transform.position + Vector3.up * 2f, $"{actionVerb}!", Color.yellow);
                            GorkhaAudio.PlayClick();
                        }
                        else
                        {
                            SpawnFloatingText(resNode.transform.position + Vector3.up * 2f, "All Villagers Busy!", Color.red);
                            GorkhaAudio.PlayError();
                        }
                        return;
                    }

                    // 3. Check if clicked a building
                    var building = hit.collider.GetComponentInParent<BuildingInstance>();
                    if (building != null)
                    {
                        // Double click check to focus camera smoothly
                        if (Time.time - _lastClickTime < 0.35f && _selectedBuilding == building)
                        {
                            if (IsometricCameraController.Instance != null)
                            {
                                IsometricCameraController.Instance.FocusOn(building.transform.position);
                            }
                        }
                        _lastClickTime = Time.time;

                        if (_selectedBuilding != null && _selectedBuilding != building)
                        {
                            _selectedBuilding.Deselect();
                        }

                        _selectedBuilding = building;
                        _selectedBuilding.Select();
                        _selectedUnit = null;
                        RTSSelectionManager.Instance?.ClearSelection();
                        GorkhaAudio.PlayClick();
                        return;
                    }

                    // 4. Check if clicked a unit
                    var unit = hit.collider.GetComponentInParent<UnitController>();
                    if (unit != null)
                    {
                        DeselectBuilding();
                        _selectedUnit = unit;
                        if (RTSSelectionManager.Instance != null)
                        {
                            RTSSelectionManager.Instance.SelectUnit(unit);
                        }
                        else
                        {
                            GorkhaVoiceBarks.PlaySelectBark(unit);
                        }
                        return;
                    }

                    // 5. Clicked on ground
                    DeselectBuilding();
                }
            }

            // Check for Right Click to command unit
            if (GorkhaInput.GetMouseButtonDown(1) && Camera.main != null)
            {
                if (RTSSelectionManager.Instance != null && RTSSelectionManager.Instance.SelectedUnits.Count > 0)
                {
                    return; // Handled by RTSSelectionManager
                }

                if (_selectedUnit != null)
                {
                    Ray ray = Camera.main.ScreenPointToRay(GorkhaInput.MousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit, 200f))
                    {
                        var resNode = hit.collider.GetComponentInParent<ResourceNode>();
                        if (resNode != null && !resNode.IsDepleted && _selectedUnit.IsVillager)
                        {
                            _selectedUnit.AssignHarvestTask(resNode);
                            GorkhaVoiceBarks.PlayHarvestOrderBark(_selectedUnit);
                            return;
                        }

                        var farm = hit.collider.GetComponentInParent<BuildingInstance>();
                        if (farm != null && farm.Data != null && farm.Data.buildingType == BuildingType.Farm && farm.State == BuildingState.Active && _selectedUnit.IsVillager)
                        {
                            _selectedUnit.AssignFarmTask(farm);
                            GorkhaVoiceBarks.PlayHarvestOrderBark(_selectedUnit);
                            SpawnFloatingText(farm.transform.position + Vector3.up * 2f, "🌾 Tending Terraced Farm!", Color.yellow);
                            return;
                        }
                    }

                    Plane ground = new Plane(Vector3.up, Vector3.zero);
                    if (ground.Raycast(ray, out float enter))
                    {
                        Vector3 targetPos = ray.GetPoint(enter);
                        _selectedUnit.MoveDirect(targetPos);
                        GorkhaVoiceBarks.PlayMoveOrderBark(_selectedUnit, targetPos);
                    }
                }
            }
        }

        private void UpdateFloatingTexts()
        {
            float dt = Time.deltaTime;
            for (int i = _floatingTexts.Count - 1; i >= 0; i--)
            {
                var f = _floatingTexts[i];
                f.Timer += dt;
                f.WorldPos += Vector3.up * (1.2f * dt);
                if (f.Timer >= f.Duration)
                {
                    _floatingTexts.RemoveAt(i);
                }
            }
        }

        private static Texture2D MakeBorderedTexture(int width, int height, Color bg, Color border, int borderWidth = 2)
        {
            Color[] pix = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth;
                    pix[y * width + x] = isBorder ? border : bg;
                }
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private static Texture2D MakeSolidTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            // Generate high-resolution procedural textures
            _cardBgTex = MakeBorderedTexture(32, 32, new Color(0.09f, 0.12f, 0.17f, 0.94f), new Color(0.78f, 0.62f, 0.25f, 0.85f), 2);
            _btnNormalTex = MakeBorderedTexture(32, 32, new Color(0.16f, 0.22f, 0.31f, 0.95f), new Color(0.35f, 0.45f, 0.58f, 0.85f), 2);
            _btnHoverTex = MakeBorderedTexture(32, 32, new Color(0.24f, 0.32f, 0.44f, 0.98f), new Color(0.95f, 0.78f, 0.28f, 1.0f), 2);
            _btnGoldNormalTex = MakeBorderedTexture(32, 32, new Color(0.72f, 0.52f, 0.12f, 0.98f), new Color(1.0f, 0.88f, 0.35f, 1.0f), 2);
            _btnGoldHoverTex = MakeBorderedTexture(32, 32, new Color(0.85f, 0.65f, 0.18f, 1.0f), new Color(1.0f, 0.95f, 0.60f, 1.0f), 2);
            _btnGreenNormalTex = MakeBorderedTexture(32, 32, new Color(0.14f, 0.52f, 0.22f, 0.98f), new Color(0.30f, 0.85f, 0.42f, 1.0f), 2);
            _btnGreenHoverTex = MakeBorderedTexture(32, 32, new Color(0.18f, 0.65f, 0.28f, 1.0f), new Color(0.50f, 0.95f, 0.60f, 1.0f), 2);
            _btnDangerTex = MakeBorderedTexture(32, 32, new Color(0.68f, 0.18f, 0.18f, 0.98f), new Color(0.95f, 0.35f, 0.35f, 1.0f), 2);
            _progressBarBgTex = MakeBorderedTexture(16, 16, new Color(0.06f, 0.08f, 0.12f, 0.95f), new Color(0.25f, 0.30f, 0.38f, 0.8f), 1);
            _progressBarFillTex = MakeSolidTexture(16, 16, new Color(0.20f, 0.85f, 0.35f, 1.0f));

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _headerStyle.normal.textColor = new Color(1.0f, 0.92f, 0.65f);

            _subHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft
            };
            _subHeaderStyle.normal.textColor = new Color(0.92f, 0.92f, 0.92f);

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            _bodyStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            _cardStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(14, 14, 12, 12),
                border = new RectOffset(4, 4, 4, 4)
            };
            _cardStyle.normal.background = _cardBgTex;

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(3, 3, 3, 3)
            };
            _buttonStyle.normal.background = _btnNormalTex;
            _buttonStyle.hover.background = _btnHoverTex;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.hover.textColor = new Color(1.0f, 0.92f, 0.5f);

            _goldButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 38,
                border = new RectOffset(3, 3, 3, 3)
            };
            _goldButtonStyle.normal.background = _btnGoldNormalTex;
            _goldButtonStyle.hover.background = _btnGoldHoverTex;
            _goldButtonStyle.normal.textColor = new Color(0.12f, 0.08f, 0.02f);
            _goldButtonStyle.hover.textColor = Color.black;

            _greenButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 38,
                border = new RectOffset(3, 3, 3, 3)
            };
            _greenButtonStyle.normal.background = _btnGreenNormalTex;
            _greenButtonStyle.hover.background = _btnGreenHoverTex;
            _greenButtonStyle.normal.textColor = Color.white;
            _greenButtonStyle.hover.textColor = new Color(0.9f, 1.0f, 0.9f);

            _dangerButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 34,
                border = new RectOffset(3, 3, 3, 3)
            };
            _dangerButtonStyle.normal.background = _btnDangerTex;
            _dangerButtonStyle.normal.textColor = new Color(1.0f, 0.85f, 0.85f);

            _floaterStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _dockCardStyle = new GUIStyle(_buttonStyle)
            {
                fontSize = 12,
                padding = new RectOffset(6, 6, 4, 4),
                fixedHeight = 68
            };

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            float scale = GetEffectiveScale();
            Matrix4x4 origMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1.0f));

            float vWidth = Screen.width / scale;
            float vHeight = Screen.height / scale;

            DrawTopResourceBanner(vWidth, vHeight);
            DrawBottomConstructionDock(vWidth, vHeight);

            if (_selectedBuilding != null)
            {
                DrawBuildingInspectorModal(vWidth, vHeight);
            }
            else if (RTSSelectionManager.Instance != null && RTSSelectionManager.Instance.SelectedUnits.Count > 0)
            {
                DrawTacticalUnitInspector(vWidth, vHeight);
            }

            DrawFloatingTexts(scale);

            // Historical Era Advancement Fanfare Celebration (AGE-01 / AGE-02)
            AgeManager.Instance?.DrawCelebrationOverlay(vWidth, vHeight);

            // Vidya Kendra Tech Tree Research Modal (AGE-03)
            if (_showTechModal)
            {
                DrawTechResearchModal(vWidth, vHeight);
            }

            // Wonder Victory Countdown & Banner (AGE-04)
            DrawWonderVictoryBanner(vWidth, vHeight);

            // Guthi Clan & Campaign Modal (SOC-01 / SOC-03)
            if (_showGuthiModal)
            {
                DrawGuthiModal(vWidth, vHeight);
            }

            GUI.matrix = origMatrix;
        }

        #region Top Resource Banner

        private void DrawTopResourceBanner(float vWidth, float vHeight)
        {
            float bannerWidth = Mathf.Min(vWidth - 24f, 1320f);
            float xPos = (vWidth - bannerWidth) * 0.5f;
            _topBannerRect = new Rect(xPos, 8, bannerWidth, 60);

            GUILayout.BeginArea(_topBannerRect);
            GUILayout.BeginHorizontal(_cardStyle);

            // Historical Era Pill (AGE-01 / AGE-02)
            if (AgeManager.Instance != null)
            {
                var curAge = AgeManager.Instance.CurrentAgeData;
                GUILayout.BeginVertical(GUILayout.Width(118));
                GUI.color = curAge.themeColor;
                GUILayout.Label($"<b>{curAge.name}</b>", _subHeaderStyle);
                GUI.color = new Color(0.85f, 0.85f, 0.85f, 0.85f);
                GUILayout.Label($"<size=9>{curAge.historicalSpan}</size>", _bodyStyle);
                GUI.color = Color.white;
                GUILayout.EndVertical();
            }

            if (ResourceManager.Instance != null)
            {
                DrawResourcePill("🌾 Food", ResourceManager.Instance.Food, ResourceManager.Instance.GetCapacity(ResourceType.Food), ResourceManager.Instance.GetRate(ResourceType.Food), new Color(0.95f, 0.85f, 0.35f));
                DrawResourcePill("🪵 Wood", ResourceManager.Instance.Wood, ResourceManager.Instance.GetCapacity(ResourceType.Wood), ResourceManager.Instance.GetRate(ResourceType.Wood), new Color(0.85f, 0.65f, 0.45f));
                DrawResourcePill("🪨 Stone", ResourceManager.Instance.Stone, ResourceManager.Instance.GetCapacity(ResourceType.Stone), ResourceManager.Instance.GetRate(ResourceType.Stone), new Color(0.75f, 0.78f, 0.82f));
                DrawResourcePill("🪙 Gold", ResourceManager.Instance.Gold, ResourceManager.Instance.GetCapacity(ResourceType.Gold), ResourceManager.Instance.GetRate(ResourceType.Gold), new Color(1.0f, 0.84f, 0.0f));
                DrawResourcePill("💎 Gems", ResourceManager.Instance.Gems, 9999, 0, new Color(0.35f, 0.85f, 1.0f));

                // Population Cap (AoE ECO-02)
                int curPop = ResourceManager.Instance.CurrentPopulation;
                int maxPop = ResourceManager.Instance.MaxPopulation;
                Color popCol = curPop >= maxPop ? new Color(1.0f, 0.45f, 0.45f) : new Color(0.65f, 0.90f, 1.0f);
                GUILayout.BeginVertical(GUILayout.Width(76));
                GUI.color = popCol;
                GUILayout.Label($"👥 <b>{curPop}/{maxPop}</b>", _subHeaderStyle);
                GUI.color = Color.white;
                float popPct = maxPop > 0 ? Mathf.Clamp01((float)curPop / maxPop) : 0f;
                Rect popGaugeRect = GUILayoutUtility.GetRect(70, 4, GUILayout.ExpandWidth(true));
                GUI.DrawTexture(popGaugeRect, _progressBarBgTex);
                if (popPct > 0.01f)
                {
                    GUI.DrawTexture(new Rect(popGaugeRect.x, popGaugeRect.y, popGaugeRect.width * popPct, popGaugeRect.height), _progressBarFillTex);
                }
                GUILayout.EndVertical();
            }

            // Dakshya Karmi / Builder Tracker (CoC style)
            if (BuilderManager.Instance != null)
            {
                int total = BuilderManager.Instance.TotalBuilders;
                int free = BuilderManager.Instance.AvailableBuildersCount;
                Color bCol = free > 0 ? new Color(0.45f, 1.0f, 0.45f) : new Color(1.0f, 0.55f, 0.45f);
                GUILayout.BeginVertical(GUILayout.Width(96));
                GUI.color = bCol;
                GUILayout.Label($"🔨 <b>{free}/{total} Free</b>", _subHeaderStyle);
                GUI.color = Color.white;
                if (total < BuilderManager.Instance.MaxBuilders)
                {
                    if (GUILayout.Button("+ Hire (250💎)", _buttonStyle, GUILayout.Height(20)))
                    {
                        if (BuilderManager.Instance.HireAdditionalBuilder(250))
                        {
                            SpawnFloatingText(new Vector3(24f, 2f, 24f), "Master Craftsman Hired!", Color.green);
                        }
                        else
                        {
                            GorkhaAudio.PlayError();
                            SpawnFloatingText(new Vector3(24f, 2f, 24f), "Need 250 Gems!", Color.red);
                        }
                    }
                }
                GUILayout.EndVertical();
            }

            // Kingdom Peace Shield (DEF-05)
            if (ResourceManager.Instance != null)
            {
                if (ResourceManager.Instance.HasActiveShield)
                {
                    int secs = Mathf.RoundToInt(ResourceManager.Instance.ShieldRemainingSeconds);
                    int mins = secs / 60;
                    int remSecs = secs % 60;
                    GUI.color = new Color(0.95f, 0.85f, 0.35f);
                    GUILayout.Label($"🛡️ <b>Shield: {mins}:{remSecs:D2}</b>", _subHeaderStyle, GUILayout.Width(110));
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = new Color(1.0f, 0.4f, 0.4f);
                    GUILayout.Label("⚔️ <b>Unshielded</b>", _subHeaderStyle, GUILayout.Width(95));
                    GUI.color = Color.white;
                }
            }

            // Guthi Clan & Campaign (SOC-01 / SOC-03)
            if (GUILayout.Button("🛡️ Guthi", _goldButtonStyle, GUILayout.Width(72), GUILayout.Height(32)))
            {
                _showGuthiModal = !_showGuthiModal;
                GorkhaAudio.PlayClick();
            }

            // Time of Day Toggle (Phase 6)
            if (Graphics.HimalayanDayNightCycle.Instance != null)
            {
                var tod = Graphics.HimalayanDayNightCycle.Instance.CurrentTime;
                string todIcon = tod switch
                {
                    Graphics.TimeOfDay.Dawn => "🌅",
                    Graphics.TimeOfDay.Noon => "☀️",
                    Graphics.TimeOfDay.Sunset => "🌄",
                    _ => "🌙"
                };
                if (GUILayout.Button(todIcon, _buttonStyle, GUILayout.Width(34), GUILayout.Height(32)))
                {
                    var nextTod = (Graphics.TimeOfDay)(((int)tod + 1) % 4);
                    Graphics.HimalayanDayNightCycle.Instance.SetTimeOfDay(nextTod);
                    GorkhaAudio.PlayClick();
                }
            }

            // Quick Center Kingdom Button
            if (GUILayout.Button("🎯 Center", _buttonStyle, GUILayout.Width(76), GUILayout.Height(32)))
            {
                GorkhaAudio.PlayClick();
                if (IsometricCameraController.Instance != null)
                {
                    IsometricCameraController.Instance.ResetView();
                }
            }

            // UI Scale Cycle Toggle (100% / 125% / 150% / 175% / 200%)
            string scaleLabel = $"🔍 {(int)(_userScaleMultiplier * 100)}%";
            if (GUILayout.Button(scaleLabel, _buttonStyle, GUILayout.Width(66), GUILayout.Height(32)))
            {
                CycleUserScale();
                GorkhaAudio.PlayClick();
            }

            // Mute SFX Toggle
            if (GorkhaAudio.Instance != null)
            {
                string soundLabel = GorkhaAudio.Instance.IsMuted ? "🔇" : "🔊";
                if (GUILayout.Button(soundLabel, _buttonStyle, GUILayout.Width(42), GUILayout.Height(32)))
                {
                    GorkhaAudio.Instance.IsMuted = !GorkhaAudio.Instance.IsMuted;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawResourcePill(string label, double amount, double capacity, double rate, Color color)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUI.color = color;
            string rateStr = rate > 0 ? $" <size=10>(+{rate:F1}/s)</size>" : "";
            GUILayout.Label($"{label}: <b>{(int)amount}</b>{rateStr}", _subHeaderStyle);
            GUI.color = Color.white;

            // Mini Capacity Gauge
            float pct = Mathf.Clamp01((float)(amount / Math.Max(1.0, capacity)));
            Rect gaugeRect = GUILayoutUtility.GetRect(100, 4, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(gaugeRect, _progressBarBgTex);
            if (pct > 0.01f)
            {
                GUI.DrawTexture(new Rect(gaugeRect.x, gaugeRect.y, gaugeRect.width * pct, gaugeRect.height), _progressBarFillTex);
            }
            GUILayout.EndVertical();
        }

        #endregion

        #region Bottom Construction Dock

        private void DrawBottomConstructionDock(float vWidth, float vHeight)
        {
            bool isPlacing = BuildingPlacementController.Instance != null && BuildingPlacementController.Instance.IsPlacing;

            float dockWidth = Mathf.Min(vWidth - 24f, 1100f);
            float dockHeight = isPlacing ? 60f : 86f;
            float xPos = (vWidth - dockWidth) * 0.5f;
            _bottomDockRect = new Rect(xPos, vHeight - dockHeight - 8f, dockWidth, dockHeight);

            GUILayout.BeginArea(_bottomDockRect);
            GUILayout.BeginHorizontal(_cardStyle);

            if (isPlacing)
            {
                var bp = BuildingPlacementController.Instance.CurrentBlueprint;
                string bpName = bp != null ? bp.displayName : "Structure";
                bool isRelocating = BuildingPlacementController.Instance.IsRelocating;

                string statusText = isRelocating
                    ? $"📦 Relocating <b>{bpName}</b> — Click ground to place!"
                    : $"🔨 Placing <b>{bpName}</b> — Click ground to construct!";
                string cancelText = isRelocating ? "❌ Cancel Move (Esc)" : "❌ Cancel Placement (Esc)";

                GUILayout.Label(statusText, _headerStyle);

                if (GUILayout.Button(cancelText, _dangerButtonStyle, GUILayout.Width(220)))
                {
                    GorkhaAudio.PlayClick();
                    BuildingPlacementController.Instance.CancelPlacement();
                }
            }
            else
            {
                // Category Tabs on Left
                GUILayout.BeginVertical(GUILayout.Width(116));
                GUIStyle activeCategoryStyle = _goldButtonStyle;
                GUIStyle inactiveCategoryStyle = _buttonStyle;

                if (GUILayout.Button("🌾 Economy", _selectedBuildCategory == 0 ? activeCategoryStyle : inactiveCategoryStyle, GUILayout.Height(32)))
                {
                    _selectedBuildCategory = 0;
                    GorkhaAudio.PlayClick();
                }
                if (GUILayout.Button("⚔️ Military", _selectedBuildCategory == 1 ? activeCategoryStyle : inactiveCategoryStyle, GUILayout.Height(32)))
                {
                    _selectedBuildCategory = 1;
                    GorkhaAudio.PlayClick();
                }
                GUILayout.EndVertical();

                GUILayout.Space(6);

                if (_selectedBuildCategory == 0)
                {
                    // Economy & Storage Tab (CoC ECO-01 + AoE ECO-02 + AGE-02 Gating)
                    DrawGatedDockCard(BuildingType.Farm, "🌾", "Farm", "30W • +2.5/s", () =>
                        StartBlueprintPlacement(BuildingType.Farm, "Terraced Farm", 2, 2, ResourceType.Food, 2.5, woodCost: 30));

                    DrawGatedDockCard(BuildingType.WoodCamp, "🪵", "Wood Camp", "30F • +2.0/s", () =>
                        StartBlueprintPlacement(BuildingType.WoodCamp, "Woodcutter Camp", 2, 2, ResourceType.Wood, 2.0, foodCost: 30));

                    DrawGatedDockCard(BuildingType.StoneQuarry, "🪨", "Quarry", "50W • +1.0/s", () =>
                        StartBlueprintPlacement(BuildingType.StoneQuarry, "Stone Quarry", 2, 2, ResourceType.Stone, 1.0, woodCost: 50));

                    DrawGatedDockCard(BuildingType.House, "🏠", "House", "40W • +5 Pop", () =>
                        StartBlueprintPlacement(BuildingType.House, "Newari House", 2, 2, ResourceType.Food, 0.0, woodCost: 40));

                    DrawGatedDockCard(BuildingType.Granary, "🌾", "Granary", "80W, 40S • +1k F", () =>
                        StartBlueprintPlacement(BuildingType.Granary, "Granary (Bhakari)", 2, 2, ResourceType.Food, 0.0, woodCost: 80, stoneCost: 40, isStorage: true, storageType: ResourceType.Food, storageCapacity: 1000));

                    DrawGatedDockCard(BuildingType.TimberYard, "🪵", "Timber Yard", "80W, 40S • +1k W", () =>
                        StartBlueprintPlacement(BuildingType.TimberYard, "Timber Yard", 2, 2, ResourceType.Wood, 0.0, woodCost: 80, stoneCost: 40, isStorage: true, storageType: ResourceType.Wood, storageCapacity: 1000));

                    DrawGatedDockCard(BuildingType.SlateDepot, "🪨", "Slate Depot", "80W, 40S • +1k S", () =>
                        StartBlueprintPlacement(BuildingType.SlateDepot, "Slate Depot", 2, 2, ResourceType.Stone, 0.0, woodCost: 80, stoneCost: 40, isStorage: true, storageType: ResourceType.Stone, storageCapacity: 1000));

                    DrawGatedDockCard(BuildingType.Vault, "🪙", "Vault", "100W, 60S • +1k G", () =>
                        StartBlueprintPlacement(BuildingType.Vault, "Treasury Vault (Dhukuti)", 2, 2, ResourceType.Gold, 0.0, woodCost: 100, stoneCost: 60, isStorage: true, storageType: ResourceType.Gold, storageCapacity: 1000));

                    DrawGatedDockCard(BuildingType.Academy, "🏛️", "Academy", "120W, 60S", () =>
                        StartBlueprintPlacement(BuildingType.Academy, "Vidya Kendra (Academy)", 2, 2, ResourceType.Food, 0.0, woodCost: 120, stoneCost: 60));
                }
                else
                {
                    // Military & Fortifications Tab (DEF & AGE-02 Gating)
                    DrawGatedDockCard(BuildingType.Barracks, "⚔️", "Barracks", "100W, 50S", () =>
                        StartBlueprintPlacement(BuildingType.Barracks, "Kot (Barracks)", 2, 2, ResourceType.Food, 0.0, woodCost: 100, stoneCost: 50));

                    DrawGatedDockCard(BuildingType.Watchtower, "🏹", "Tower", "60W, 40S", () =>
                        StartBlueprintPlacement(BuildingType.Watchtower, "Watchtower", 2, 2, ResourceType.Food, 0.0, woodCost: 60, stoneCost: 40));

                    DrawGatedDockCard(BuildingType.Wall, "🧱", "Wall", "10W • 1x1", () =>
                        StartBlueprintPlacement(BuildingType.Wall, "Fortification Wall", 1, 1, ResourceType.Food, 0.0, woodCost: 10));

                    DrawGatedDockCard(BuildingType.Kaman, "💣", "Kaman", "75W, 50S", () =>
                        StartBlueprintPlacement(BuildingType.Kaman, "Kaman Cannon", 2, 2, ResourceType.Food, 0.0, woodCost: 75, stoneCost: 50));

                    DrawGatedDockCard(BuildingType.Trap, "🕳️", "Trap", "20W, 10S", () =>
                        StartBlueprintPlacement(BuildingType.Trap, "Spike Trap", 1, 1, ResourceType.Food, 0.0, woodCost: 20, stoneCost: 10));

                    DrawGatedDockCard(BuildingType.Wonder, "🏆", "Wonder", "500W, 500S, 300G", () =>
                        StartBlueprintPlacement(BuildingType.Wonder, "Himalayan Wonder", 3, 3, ResourceType.Food, 0.0, woodCost: 500, stoneCost: 500));

                    if (GUILayout.Button("⚔️ <b>Raid!</b>\n<size=10><color=#FF8888>Outposts</color></size>", _dangerButtonStyle, GUILayout.Width(76)))
                    {
                        if (GorkhaRaidBattleController.Instance != null)
                        {
                            GorkhaRaidBattleController.Instance.OpenOutpostSelectionMenu();
                        }
                        else
                        {
                            SpawnHostileRaidWave();
                        }
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawGatedDockCard(BuildingType bType, string icon, string name, string costDesc, Action onClick)
        {
            bool canConstruct = BuildingGatingManager.CanConstruct(bType, out string lockReason);
            int curCount = BuildingGatingManager.GetCurrentCount(bType);
            int maxCount = BuildingGatingManager.GetMaxAllowed(bType);
            string countText = maxCount >= 999 ? "" : $" ({curCount}/{maxCount})";

            if (!canConstruct)
            {
                GUI.color = new Color(0.65f, 0.65f, 0.65f, 0.65f);
                if (GUILayout.Button($"🔒 <b>{name}</b>{countText}\n<size=10><color=#FF8888>{lockReason}</color></size>", _dockCardStyle))
                {
                    GorkhaAudio.PlayError();
                    SpawnFloatingText(new Vector3(24f, 2f, 24f), lockReason, Color.red);
                }
                GUI.color = Color.white;
            }
            else
            {
                if (GUILayout.Button($"{icon} <b>{name}</b>{countText}\n<size=11><color=#D4B070>{costDesc}</color></size>", _dockCardStyle))
                {
                    onClick?.Invoke();
                }
            }
        }

        private void SpawnHostileRaidWave()
        {
            GorkhaAudio.PlayClick();
            GorkhaAudio.PlayCannon();

            if (ResourceManager.Instance != null && ResourceManager.Instance.HasActiveShield)
            {
                ResourceManager.Instance.BreakShield();
                SpawnFloatingText(new Vector3(24f, 0, 24f) + Vector3.up * 5.2f, "🛡️ Peace Treaty Shield Broken!", Color.red);
            }

            SpawnFloatingText(new Vector3(24f, 0, 24f) + Vector3.up * 3.8f, "⚠️ INCOMING BANDIT RAID WAVE! ⚠️", Color.red);

            // Spawn hostile bandit raiders on perimeter
            Vector3[] spawnPoints = {
                new Vector3(4f, 0, 8f),
                new Vector3(6f, 0, 4f),
                new Vector3(42f, 0, 10f),
                new Vector3(44f, 0, 40f)
            };

            foreach (var pos in spawnPoints)
            {
                GameObject banditObj = new GameObject("HostileBandit_Raider");
                banditObj.transform.position = pos;
                var unit = banditObj.AddComponent<UnitController>();
                unit.SetUnitType(UnitType.KhukuriWarrior);
                unit.SetHostile(true);
                GorkhaVFX.SpawnPlacementDust(pos, Vector2.one * 1.2f);
            }
        }

        private void StartBlueprintPlacement(BuildingType type, string name, int w, int h, ResourceType prodType, double rate, double woodCost = 0, double foodCost = 0, double stoneCost = 0, bool isStorage = false, ResourceType storageType = ResourceType.Food, double storageCapacity = 1000.0)
        {
            GorkhaAudio.PlayClick();

            BuildingData data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingType = type;
            data.displayName = name;
            data.tileWidth = w;
            data.tileHeight = h;
            data.hasProduction = rate > 0;
            data.productionType = prodType;
            data.baseProductionRate = rate;
            data.constructionDurationSeconds = 6.0f;
            data.isStorage = isStorage;
            data.storageType = storageType;
            data.baseStorageCapacity = storageCapacity;
            data.prefab = FantasyKingdomAssetRegistry.GetPrefab(type);

            if (woodCost > 0) data.baseCost.Add(new ResourceCost { type = ResourceType.Wood, amount = woodCost });
            if (foodCost > 0) data.baseCost.Add(new ResourceCost { type = ResourceType.Food, amount = foodCost });
            if (stoneCost > 0) data.baseCost.Add(new ResourceCost { type = ResourceType.Stone, amount = stoneCost });

            BuildingPlacementController.Instance.StartPlacement(data);
        }

        #endregion

        #region Building Inspector & Upgrade Modal (Clash of Clans Centered Card)

        private void DrawBuildingInspectorModal(float vWidth, float vHeight)
        {
            var b = _selectedBuilding;
            if (b == null) return;

            float modalWidth = 480f;
            float modalHeight = 320f;
            if (b.Data != null)
            {
                if (b.Data.buildingType == BuildingType.Barracks) modalHeight = 460f;
                else if (b.Data.buildingType == BuildingType.TownCenter) modalHeight = 550f;
                else if (b.Data.buildingType == BuildingType.Academy) modalHeight = 360f;
                else if (b.Data.buildingType == BuildingType.Wonder) modalHeight = 380f;
                else if (b.Data.isStorage) modalHeight = 360f;
            }
            float xPos = (vWidth - modalWidth) * 0.5f;
            _inspectorRect = new Rect(xPos, vHeight - modalHeight - 96f, modalWidth, modalHeight);

            GUILayout.BeginArea(_inspectorRect);
            GUILayout.BeginVertical(_cardStyle);

            // Title & Level Header
            GUILayout.BeginHorizontal();
            string displayName = b.Data != null ? b.Data.displayName : "Kingdom Building";
            GUILayout.Label($"🏰 <b>{displayName}</b>", _headerStyle);
            GUILayout.Label($"<color=#FFD700><b>[ Lv. {b.Level} ]</b></color>", _subHeaderStyle, GUILayout.Width(85));

            if (GUILayout.Button("✖", _buttonStyle, GUILayout.Width(30), GUILayout.Height(28)))
            {
                GorkhaAudio.PlayClick();
                b.Deselect();
                _selectedBuilding = null;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // State and Production stats
            if (b.State == BuildingState.Constructing || b.State == BuildingState.Upgrading)
            {
                string stateTitle = b.State == BuildingState.Constructing ? "Under Construction" : "Upgrading";
                GUILayout.Label($"🔨 <b>{stateTitle}</b> ({b.RemainingConstructionTime:F1}s remaining)", _subHeaderStyle);

                // Animated Progress Bar
                Rect progRect = GUILayoutUtility.GetRect(modalWidth - 28, 16);
                GUI.DrawTexture(progRect, _progressBarBgTex);
                float fillW = (progRect.width - 4) * Mathf.Clamp01(b.ConstructionProgress);
                if (fillW > 0.01f)
                {
                    GUI.DrawTexture(new Rect(progRect.x + 2, progRect.y + 2, fillW, progRect.height - 4), _progressBarFillTex);
                }

                GUILayout.Space(6);

                // Gem rush CTA
                if (GUILayout.Button("⚡ Finish Instantly (3 Gems)", _goldButtonStyle))
                {
                    if (ResourceManager.Instance != null && ResourceManager.Instance.Gems >= 3)
                    {
                        var gemCost = new Dictionary<ResourceType, double> { { ResourceType.Gems, 3 } };
                        if (ResourceManager.Instance.SpendResources(gemCost))
                        {
                            b.CompleteConstruction();
                            SpawnFloatingText(b.transform.position + Vector3.up * 3f, "Completed!", Color.yellow);
                        }
                    }
                    else
                    {
                        GorkhaAudio.PlayError();
                        SpawnFloatingText(b.transform.position + Vector3.up * 3f, "Not Enough Gems!", Color.red);
                    }
                }
            }
            else
            {
                GUILayout.Label($"🟢 Status: <b>Operational</b>  •  ❤️ <b>{b.Health}/{b.MaxHealth} HP</b>", _subHeaderStyle);
                if (b.Data != null && (b.Data.buildingType == BuildingType.Watchtower || b.Data.buildingType == BuildingType.Kaman))
                {
                    string rangeDesc = b.Data.buildingType == BuildingType.Kaman ? "3.5m - 18.0m (Mortar)" : "12.0m (Archer)";
                    GUILayout.Label($"🎯 Defense Range: <b>{rangeDesc}</b>", _subHeaderStyle);
                }
                if (b.Data != null && b.Data.hasProduction)
                {
                    double currentRate = b.Data.baseProductionRate * b.Level;
                    GUILayout.Label($"📈 Yield: <b>+{currentRate:F1} {b.Data.productionType} / sec</b>", _subHeaderStyle);

                    if (GUILayout.Button($"🌾 Collect Yield Now", _goldButtonStyle))
                    {
                        double amt = b.CollectResource();
                        SpawnFloatingText(b.transform.position + Vector3.up * 3.5f, $"+{(int)amt} {b.Data.productionType}", Color.yellow);
                    }
                }

                if (b.Data != null && b.Data.isStorage)
                {
                    double curAmt = ResourceManager.Instance != null ? ResourceManager.Instance.GetAmount(b.Data.storageType) : 0;
                    double maxCap = ResourceManager.Instance != null ? ResourceManager.Instance.GetCapacity(b.Data.storageType) : 1000;
                    GUILayout.Label($"📦 Kingdom {b.Data.storageType} Stored: <b>{(int)curAmt} / {(int)maxCap}</b>", _subHeaderStyle);

                    float fillPct = Mathf.Clamp01((float)(curAmt / Math.Max(1.0, maxCap)));
                    Rect storRect = GUILayoutUtility.GetRect(modalWidth - 28, 10);
                    GUI.DrawTexture(storRect, _progressBarBgTex);
                    if (fillPct > 0.01f)
                    {
                        GUI.DrawTexture(new Rect(storRect.x + 2, storRect.y + 2, (storRect.width - 4) * fillPct, storRect.height - 4), _progressBarFillTex);
                    }
                    GUILayout.Label($"<size=11><color=#A0E0A0>Adds +{(int)(b.Data.baseStorageCapacity * b.Level)} to kingdom maximum storage</color></size>", _subHeaderStyle);
                }

                // Upgrade Building Button
                if (b.Data != null)
                {
                    var nextCost = b.Data.GetCostDictionary(b.Level);
                    string costStr = FormatCostString(nextCost);
                    bool canAfford = ResourceManager.Instance != null && ResourceManager.Instance.CanAfford(nextCost);

                    GUILayout.Space(6);
                    GUILayout.Label($"⬆️ <b>Upgrade to Level {b.Level + 1}:</b>", _subHeaderStyle);
                    GUILayout.Label($"Cost: <color=#D4B070>{costStr}</color>", _subHeaderStyle);

                    GUI.enabled = canAfford;
                    if (GUILayout.Button(canAfford ? $"⬆️ Upgrade Building (Level {b.Level + 1})" : "❌ Insufficient Resources", _greenButtonStyle))
                    {
                        if (b.StartUpgrade())
                        {
                            SpawnFloatingText(b.transform.position + Vector3.up * 3f, "Upgrade Started!", Color.green);
                        }
                    }
                    GUI.enabled = true;

                    // Relocate / Move Building (CoC Drag & Drop Relocation)
                    GUILayout.Space(6);
                    if (GUILayout.Button("📦 Move Structure (Relocate)", _buttonStyle))
                    {
                        GorkhaAudio.PlayClick();
                        var target = b;
                        b.Deselect();
                        _selectedBuilding = null;
                        BuildingPlacementController.Instance.StartRelocation(target);
                        SpawnFloatingText(target.transform.position + Vector3.up * 3f, "Move Mode: Click new location!", Color.cyan);
                        GUILayout.EndVertical();
                        GUILayout.EndArea();
                        return;
                    }
                }
            }

            // If Barracks: Troop Recruitment Section (AGE-02 Gated)
            if (b.Data != null && b.Data.buildingType == BuildingType.Barracks && b.State == BuildingState.Active)
            {
                GUILayout.Space(8);
                GUILayout.Label("⚔️ <b>Gorkha Army Recruitment:</b>", _headerStyle);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("🗡️ Spearman\n<size=11>25 Food</size>", _buttonStyle, GUILayout.Height(46)))
                {
                    TrainUnit(UnitType.Spearman, foodCost: 25);
                }

                bool khukuriUnlocked = BuildingGatingManager.IsUnitUnlocked(UnitType.KhukuriWarrior, out string khukuriReason);
                if (khukuriUnlocked)
                {
                    if (GUILayout.Button("⚔️ Khukuri\n<size=11>35F, 15W</size>", _buttonStyle, GUILayout.Height(46)))
                    {
                        TrainUnit(UnitType.KhukuriWarrior, foodCost: 35, woodCost: 15);
                    }
                }
                else
                {
                    GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                    if (GUILayout.Button($"🔒 Khukuri\n<size=10><color=#FF8888>{khukuriReason}</color></size>", _buttonStyle, GUILayout.Height(46)))
                    {
                        GorkhaAudio.PlayError();
                        SpawnFloatingText(b.transform.position + Vector3.up * 3f, khukuriReason, Color.red);
                    }
                    GUI.color = Color.white;
                }

                bool archerUnlocked = BuildingGatingManager.IsUnitUnlocked(UnitType.Archer, out string archerReason);
                if (archerUnlocked)
                {
                    if (GUILayout.Button("🏹 Archer\n<size=11>30F, 20W</size>", _buttonStyle, GUILayout.Height(46)))
                    {
                        TrainUnit(UnitType.Archer, foodCost: 30, woodCost: 20);
                    }
                }
                else
                {
                    GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                    if (GUILayout.Button($"🔒 Archer\n<size=10><color=#FF8888>{archerReason}</color></size>", _buttonStyle, GUILayout.Height(46)))
                    {
                        GorkhaAudio.PlayError();
                        SpawnFloatingText(b.transform.position + Vector3.up * 3f, archerReason, Color.red);
                    }
                    GUI.color = Color.white;
                }
                GUILayout.EndHorizontal();
            }

            // If Town Center: Civilian Recruitment & Historical Era Status (AoE ECO-02 + AGE-01 / AGE-02)
            if (b.Data != null && b.Data.buildingType == BuildingType.TownCenter && b.State == BuildingState.Active)
            {
                GUILayout.Space(6);
                if (AgeManager.Instance != null)
                {
                    var curAge = AgeManager.Instance.CurrentAgeData;
                    GUILayout.Label($"🏛️ <b>Current Historical Era:</b> <color=#{ColorUtility.ToHtmlStringRGB(curAge.themeColor)}>{curAge.name}</color> ({curAge.historicalSpan})", _subHeaderStyle);
                    GUILayout.Label($"<size=10><color=#CCCCCC>{curAge.description}</color></size>", _bodyStyle);
                    if (AgeManager.Instance.CurrentAgeNumber < 4)
                    {
                        var nextAge = AgeManager.Instance.GetAge(AgeManager.Instance.CurrentAgeNumber + 1);
                        GUILayout.Label($"<size=10><color=#FFD700>⬆️ Upgrading TC unlocks: {nextAge.unlockHighlights}</color></size>", _bodyStyle);
                    }
                }

                GUILayout.Space(4);
                GUILayout.Label("👨‍🌾 <b>Civilian Recruitment:</b>", _headerStyle);

                bool popFull = ResourceManager.Instance != null && !ResourceManager.Instance.CanSupportUnit();
                bool hasFood = ResourceManager.Instance != null && ResourceManager.Instance.Food >= 50;

                GUILayout.BeginHorizontal();
                GUI.enabled = !popFull && hasFood;
                if (GUILayout.Button("🌾 Train Villager (Kisan)\n<size=11>50 Food • +1 Laborer</size>", _buttonStyle, GUILayout.Height(46)))
                {
                    TrainVillager(50);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                if (popFull)
                {
                    GUILayout.Label("<color=#FF6666>⚠️ Population limit reached! Build more Houses.</color>", _subHeaderStyle);
                }

                // Hero Generals (CBT-06)
                GUILayout.Space(6);
                GUILayout.Label("👑 <b>Commanders & Hero Generals (CBT-06):</b>", _headerStyle);
                GUILayout.BeginHorizontal();

                // General Kalu Pande
                bool kaluUnlocked = Progression.BuildingGatingManager.IsUnitUnlocked(UnitType.HeroKaluPande, out string kaluReason);
                bool kaluAlive = Progression.HeroManager.Instance != null && Progression.HeroManager.Instance.IsHeroAlive(Units.HeroGeneralType.KaluPande);
                if (kaluAlive)
                {
                    GUI.color = new Color(0.4f, 0.8f, 1f, 0.8f);
                    GUILayout.Button("🛡️ Kalu Pande\n<size=10>In Battle</size>", _buttonStyle, GUILayout.Height(44));
                    GUI.color = Color.white;
                }
                else if (kaluUnlocked)
                {
                    if (GUILayout.Button("🛡️ General Kalu Pande\n<size=10>150F, 100G</size>", _buttonStyle, GUILayout.Height(44)))
                    {
                        if (Progression.HeroManager.Instance != null && Progression.HeroManager.Instance.CanSummonHero(Units.HeroGeneralType.KaluPande, out string r))
                        {
                            Progression.HeroManager.Instance.SummonHero(Units.HeroGeneralType.KaluPande, b.transform.position + new Vector3(2.5f, 0, 0));
                        }
                        else
                        {
                            GorkhaAudio.PlayError();
                            SpawnFloatingText(b.transform.position + Vector3.up * 3f, "Insufficient Resources!", Color.red);
                        }
                    }
                }
                else
                {
                    GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                    if (GUILayout.Button($"🔒 Kalu Pande\n<size=10>{kaluReason}</size>", _buttonStyle, GUILayout.Height(44)))
                    {
                        GorkhaAudio.PlayError();
                        SpawnFloatingText(b.transform.position + Vector3.up * 3f, kaluReason, Color.red);
                    }
                    GUI.color = Color.white;
                }

                // King Prithvi Narayan Shah
                bool prithviUnlocked = Progression.BuildingGatingManager.IsUnitUnlocked(UnitType.HeroPrithvi, out string pReason);
                bool prithviAlive = Progression.HeroManager.Instance != null && Progression.HeroManager.Instance.IsHeroAlive(Units.HeroGeneralType.PrithviNarayanShah);
                if (prithviAlive)
                {
                    GUI.color = new Color(1f, 0.85f, 0.2f, 0.8f);
                    GUILayout.Button("👑 King Prithvi\n<size=10>In Battle</size>", _buttonStyle, GUILayout.Height(44));
                    GUI.color = Color.white;
                }
                else if (prithviUnlocked)
                {
                    if (GUILayout.Button("👑 King Prithvi N. Shah\n<size=10>250F, 200G</size>", _buttonStyle, GUILayout.Height(44)))
                    {
                        if (Progression.HeroManager.Instance != null && Progression.HeroManager.Instance.CanSummonHero(Units.HeroGeneralType.PrithviNarayanShah, out string r))
                        {
                            Progression.HeroManager.Instance.SummonHero(Units.HeroGeneralType.PrithviNarayanShah, b.transform.position + new Vector3(-2.5f, 0, 0));
                        }
                        else
                        {
                            GorkhaAudio.PlayError();
                            SpawnFloatingText(b.transform.position + Vector3.up * 3f, "Insufficient Resources!", Color.red);
                        }
                    }
                }
                else
                {
                    GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                    if (GUILayout.Button($"🔒 King Prithvi\n<size=10>{pReason}</size>", _buttonStyle, GUILayout.Height(44)))
                    {
                        GorkhaAudio.PlayError();
                        SpawnFloatingText(b.transform.position + Vector3.up * 3f, pReason, Color.red);
                    }
                    GUI.color = Color.white;
                }
                GUILayout.EndHorizontal();
            }

            // If Academy: Vidya Kendra Tech Tree (AGE-03)
            if (b.Data != null && b.Data.buildingType == BuildingType.Academy && b.State == BuildingState.Active)
            {
                GUILayout.Space(8);
                GUILayout.Label("📚 <b>Academy of Warfare (Vidya Kendra):</b>", _headerStyle);
                GUILayout.Label("<size=11><color=#CCCCCC>Research ancient metallurgy, architecture, agriculture & logistics.</color></size>", _bodyStyle);
                GUILayout.Space(4);
                if (GUILayout.Button("🔬 <b>Open Tech Tree Research Window</b>", _goldButtonStyle, GUILayout.Height(44)))
                {
                    _showTechModal = true;
                    GorkhaAudio.PlayClick();
                }
            }

            // If Wonder: Himalayan Wonder Defense Status (AGE-04)
            if (b.Data != null && b.Data.buildingType == BuildingType.Wonder && b.State == BuildingState.Active)
            {
                GUILayout.Space(8);
                GUILayout.Label("🏆 <b>Himalayan Monumental Wonder:</b>", _headerStyle);
                if (Progression.WonderVictoryController.Instance != null && Progression.WonderVictoryController.Instance.IsCountdownActive)
                {
                    float rem = Progression.WonderVictoryController.Instance.CountdownRemaining;
                    int mins = Mathf.FloorToInt(rem / 60f);
                    int secs = Mathf.FloorToInt(rem % 60f);
                    GUILayout.Label($"<color=#FFD700><b>🛡️ DEFENDING WONDER: {mins:D2}:{secs:D2}</b></color>", _headerStyle);
                }
                else
                {
                    GUILayout.Label("<color=#69f0ae>Monument of the Ages standing undefeated!</color>", _bodyStyle);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void TrainUnit(UnitType type, double foodCost = 0, double woodCost = 0)
        {
            GorkhaAudio.PlayClick();

            if (ResourceManager.Instance != null && !ResourceManager.Instance.CanSupportUnit())
            {
                GorkhaAudio.PlayError();
                SpawnFloatingText(_selectedBuilding != null ? _selectedBuilding.transform.position + Vector3.up * 2.5f : Vector3.zero, "Population Cap Reached! Build Houses.", Color.red);
                return;
            }

            var cost = new Dictionary<ResourceType, double>();
            if (foodCost > 0) cost[ResourceType.Food] = foodCost;
            if (woodCost > 0) cost[ResourceType.Wood] = woodCost;

            if (ResourceManager.Instance != null && !ResourceManager.Instance.SpendResources(cost))
            {
                GorkhaAudio.PlayError();
                SpawnFloatingText(_selectedBuilding != null ? _selectedBuilding.transform.position + Vector3.up * 2.5f : Vector3.zero, "Need More Food/Wood!", Color.red);
                return;
            }

            // Spawn unit next to barracks
            Vector3 spawnPos = _selectedBuilding != null
                ? _selectedBuilding.transform.position + new Vector3(1.5f, 0, -1.5f)
                : new Vector3(24f, 0, 24f);

            GameObject unitObj = new GameObject($"Unit_{type}");
            unitObj.transform.position = spawnPos;
            var controller = unitObj.AddComponent<UnitController>();
            controller.SetUnitType(type);

            GorkhaAudio.PlayRecruit();
            SpawnFloatingText(spawnPos + Vector3.up * 2f, $"{type} Enlisted!", Color.green);
        }

        private void TrainVillager(double foodCost = 50)
        {
            GorkhaAudio.PlayClick();

            if (ResourceManager.Instance != null && !ResourceManager.Instance.CanSupportUnit())
            {
                GorkhaAudio.PlayError();
                SpawnFloatingText(_selectedBuilding != null ? _selectedBuilding.transform.position + Vector3.up * 2.5f : Vector3.zero, "Population Cap Reached! Build Houses.", Color.red);
                return;
            }

            var cost = new Dictionary<ResourceType, double> { { ResourceType.Food, foodCost } };
            if (ResourceManager.Instance != null && !ResourceManager.Instance.SpendResources(cost))
            {
                GorkhaAudio.PlayError();
                SpawnFloatingText(_selectedBuilding != null ? _selectedBuilding.transform.position + Vector3.up * 2.5f : Vector3.zero, "Need 50 Food to Train Villager!", Color.red);
                return;
            }

            Vector3 spawnPos = _selectedBuilding != null
                ? _selectedBuilding.transform.position + new Vector3(2.5f, 0, -2.5f)
                : new Vector3(24f, 0, 24f);

            GameObject unitObj = new GameObject("Unit_Villager");
            unitObj.transform.position = spawnPos;
            var controller = unitObj.AddComponent<UnitController>();
            controller.SetUnitType(UnitType.Villager);

            GorkhaAudio.PlayRecruit();
            SpawnFloatingText(spawnPos + Vector3.up * 2f, "🌾 Villager (Kisan) Recruited!", Color.green);
        }

        private string FormatCostString(Dictionary<ResourceType, double> cost)
        {
            if (cost == null || cost.Count == 0) return "Free";
            List<string> parts = new();
            foreach (var kvp in cost)
            {
                parts.Add($"{kvp.Value:F0} {kvp.Key}");
            }
            return string.Join(", ", parts);
        }

        #endregion

        #region Floating Texts

        private void DrawFloatingTexts(float scale)
        {
            if (Camera.main == null) return;

            foreach (var f in _floatingTexts)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(f.WorldPos);
                if (screenPos.z > 0) // In front of camera
                {
                    float alpha = Mathf.Clamp01(1f - (f.Timer / f.Duration));
                    Color c = f.TextColor;
                    c.a = alpha;

                    float vx = screenPos.x / scale;
                    float vy = (Screen.height - screenPos.y) / scale;

                    // Text shadow for crisp legibility
                    _floaterStyle.normal.textColor = new Color(0, 0, 0, alpha * 0.75f);
                    GUI.Label(new Rect(vx - 119, vy - 19, 240, 32), f.Text, _floaterStyle);

                    _floaterStyle.normal.textColor = c;
                    GUI.Label(new Rect(vx - 120, vy - 20, 240, 32), f.Text, _floaterStyle);
                }
            }
        }

        #endregion

        #region Tactical Unit & Army Formation Inspector

        private void DrawTacticalUnitInspector(float vWidth, float vHeight)
        {
            var rts = RTSSelectionManager.Instance;
            if (rts == null || rts.SelectedUnits.Count == 0) return;

            var selected = rts.SelectedUnits;
            bool isSingleHero = selected.Count == 1 && selected[0] != null && selected[0].IsHero;
            float panelWidth = selected.Count == 1 ? 440f : 490f;
            float panelHeight = selected.Count == 1 ? (isSingleHero ? 235f : 175f) : 215f;
            float xPos = (vWidth - panelWidth) * 0.5f;
            float yPos = vHeight - panelHeight - 96f;

            _unitInspectorRect = new Rect(xPos, yPos, panelWidth, panelHeight);
            GUILayout.BeginArea(_unitInspectorRect);
            GUILayout.BeginVertical(_cardStyle);

            if (selected.Count == 1)
            {
                var u = selected[0];
                if (u != null)
                {
                    GUILayout.BeginHorizontal();
                    string unitName = u.Type switch
                    {
                        UnitType.HeroPrithvi => "👑 King Prithvi Narayan Shah (The Unifier)",
                        UnitType.HeroKaluPande => "🛡️ General Kalu Pande (Grand Commander)",
                        UnitType.Spearman => "🛡️ Gorkha Spearman",
                        UnitType.KhukuriWarrior => "🗡️ Gurkha Khukuri Warrior",
                        UnitType.Archer => "🏹 Himalayan Archer",
                        _ => "🌾 Gorkha Villager"
                    };
                    GUILayout.Label($"<b>{unitName}</b>", _headerStyle);
                    if (GUILayout.Button("✖", _buttonStyle, GUILayout.Width(26), GUILayout.Height(24)))
                    {
                        rts.ClearSelection();
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                        GUILayout.EndArea();
                        return;
                    }
                    GUILayout.EndHorizontal();

                    // Health bar
                    GUILayout.Space(2);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"<color=#AAAAAA>Health:</color> <b>{u.Health}/{u.MaxHealth}</b>", _subHeaderStyle, GUILayout.Width(130));
                    Rect hpRect = GUILayoutUtility.GetRect(190, 12);
                    GUI.DrawTexture(hpRect, _progressBarBgTex);
                    float fillPct = Mathf.Clamp01((float)u.Health / Mathf.Max(1, u.MaxHealth));
                    if (fillPct > 0.01f)
                    {
                        GUI.DrawTexture(new Rect(hpRect.x + 1, hpRect.y + 1, (hpRect.width - 2) * fillPct, hpRect.height - 2), _progressBarFillTex);
                    }
                    GUILayout.EndHorizontal();

                    // Tactical Role & Combat Triangle Counter Tip
                    GUILayout.Space(4);
                    string roleTip = u.Type switch
                    {
                        UnitType.HeroPrithvi => "<color=#ffd700><b>Divine Sun Aura:</b> +20% Attack Damage to nearby allies</color>",
                        UnitType.HeroKaluPande => "<color=#38b6ff><b>Infantry Bastion:</b> +25% Armor / Damage Reduction to nearby allies</color>",
                        UnitType.Spearman => "<color=#4deeea><b>Counter Wall:</b> +35% vs Khukuri Warriors</color>",
                        UnitType.KhukuriWarrior => "<color=#ff7043><b>Counter Flank:</b> +50% vs Archers</color>",
                        UnitType.Archer => "<color=#69f0ae><b>Counter Pierce:</b> +40% vs Spearmen</color>",
                        _ => "<color=#ffd54f><b>Civilian:</b> Harvests timber/slate & delivers resources</color>"
                    };
                    GUILayout.Label(roleTip, _subHeaderStyle);

                    if (u.TryGetComponent<Units.HeroController>(out var hero))
                    {
                        GUILayout.Space(4);
                        GUILayout.BeginHorizontal();
                        // Ability 1
                        string a1Name = hero.GeneralType == Units.HeroGeneralType.PrithviNarayanShah ? "👑 [Q] Call to Arms" : "🛡️ [W] Shield Wall";
                        bool a1Ready = hero.IsAbility1Ready;
                        if (!a1Ready) GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                        if (GUILayout.Button(a1Ready ? $"{a1Name}\n<size=10><color=#FFD700>READY</color></size>" : $"{a1Name}\n<size=10>{hero.Ability1CooldownRemaining:F1}s</size>", _goldButtonStyle, GUILayout.Height(38)))
                        {
                            hero.TriggerAbility1();
                        }
                        GUI.color = Color.white;

                        // Ability 2
                        string a2Name = hero.GeneralType == Units.HeroGeneralType.PrithviNarayanShah ? "⚡ [E] Royal Decree" : "🗡️ [R] Ambush";
                        bool a2Ready = hero.IsAbility2Ready;
                        if (!a2Ready) GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                        if (GUILayout.Button(a2Ready ? $"{a2Name}\n<size=10><color=#69F0AE>READY</color></size>" : $"{a2Name}\n<size=10>{hero.Ability2CooldownRemaining:F1}s</size>", _greenButtonStyle, GUILayout.Height(38)))
                        {
                            hero.TriggerAbility2();
                        }
                        GUI.color = Color.white;
                        GUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                // Multi-Unit Army Formation Dock
                GUILayout.BeginHorizontal();
                GUILayout.Label($"⚔️ <b>Gorkha Army Battalion ({selected.Count} Troops)</b>", _headerStyle);
                if (GUILayout.Button("✖", _buttonStyle, GUILayout.Width(26), GUILayout.Height(24)))
                {
                    rts.ClearSelection();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                    return;
                }
                GUILayout.EndHorizontal();

                // Breakdown counts
                int spearmen = 0, khukuri = 0, archers = 0, villagers = 0;
                foreach (var u in selected)
                {
                    if (u == null) continue;
                    if (u.Type == UnitType.Spearman) spearmen++;
                    else if (u.Type == UnitType.KhukuriWarrior) khukuri++;
                    else if (u.Type == UnitType.Archer) archers++;
                    else if (u.Type == UnitType.Villager) villagers++;
                }

                string breakdown = "";
                if (spearmen > 0) breakdown += $"Spearmen: {spearmen}  ";
                if (khukuri > 0) breakdown += $"Khukuri: {khukuri}  ";
                if (archers > 0) breakdown += $"Archers: {archers}  ";
                if (villagers > 0) breakdown += $"Villagers: {villagers}";
                GUILayout.Label($"<color=#CCCCCC>{breakdown.Trim()}</color>", _subHeaderStyle);

                GUILayout.Space(4);
                GUILayout.Label("<b>Tactical Formations:</b>", _subHeaderStyle);

                // 4 Formation Buttons
                GUILayout.BeginHorizontal();
                var cur = rts.CurrentFormation;

                if (GUILayout.Button("[Z] Line", cur == TacticalFormationType.Line ? _goldButtonStyle : _buttonStyle, GUILayout.Height(30)))
                    rts.SetFormation(TacticalFormationType.Line);
                if (GUILayout.Button("[X] Box", cur == TacticalFormationType.Box ? _goldButtonStyle : _buttonStyle, GUILayout.Height(30)))
                    rts.SetFormation(TacticalFormationType.Box);
                if (GUILayout.Button("[C] Wedge", cur == TacticalFormationType.Wedge ? _goldButtonStyle : _buttonStyle, GUILayout.Height(30)))
                    rts.SetFormation(TacticalFormationType.Wedge);
                if (GUILayout.Button("[V] Staggered", cur == TacticalFormationType.Staggered ? _goldButtonStyle : _buttonStyle, GUILayout.Height(30)))
                    rts.SetFormation(TacticalFormationType.Staggered);

                GUILayout.EndHorizontal();
            }

            // Tactical Command Orders (AoE CBT-04: Attack-Move, Hold, Patrol, Stop)
            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            bool isAttackMoveActive = rts.CommandMode == RTSCommandMode.TargetingAttackMove;
            bool isPatrolActive = rts.CommandMode == RTSCommandMode.TargetingPatrol;

            if (GUILayout.Button(isAttackMoveActive ? "⚔️ <b>[A] Aiming...</b>" : "⚔️ <b>[A] Atk-Move</b>", isAttackMoveActive ? _goldButtonStyle : _buttonStyle, GUILayout.Height(28)))
            {
                rts.SetCommandMode(isAttackMoveActive ? RTSCommandMode.Normal : RTSCommandMode.TargetingAttackMove);
            }
            if (GUILayout.Button("🛡️ <b>[H] Hold</b>", _buttonStyle, GUILayout.Height(28)))
            {
                rts.ExecuteHoldPositionOrder();
            }
            if (GUILayout.Button(isPatrolActive ? "🚶 <b>[P] Aiming...</b>" : "🚶 <b>[P] Patrol</b>", isPatrolActive ? _goldButtonStyle : _buttonStyle, GUILayout.Height(28)))
            {
                rts.SetCommandMode(isPatrolActive ? RTSCommandMode.Normal : RTSCommandMode.TargetingPatrol);
            }
            if (GUILayout.Button("🛑 <b>[S] Halt</b>", _dangerButtonStyle, GUILayout.Height(28)))
            {
                rts.ExecuteStopOrder();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region Vidya Kendra Tech Tree Modal (AGE-03)

        private void DrawTechResearchModal(float vWidth, float vHeight)
        {
            float modalWidth = 680f;
            float modalHeight = 440f;
            float xPos = (vWidth - modalWidth) * 0.5f;
            float yPos = (vHeight - modalHeight) * 0.5f;
            _techModalRect = new Rect(xPos, yPos, modalWidth, modalHeight);

            GUILayout.BeginArea(_techModalRect);
            GUILayout.BeginVertical(_cardStyle);

            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label("🏛️ <b>विद्या केन्द्र (Academy of Warfare & Arts) — Tech Tree</b>", _headerStyle);
            if (GUILayout.Button("✖", _buttonStyle, GUILayout.Width(30), GUILayout.Height(28)))
            {
                _showTechModal = false;
                GorkhaAudio.PlayClick();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("<size=11><color=#CCCCCC>Research ancient metallurgy, architecture, agrarian engineering & logistics to empower your kingdom.</color></size>", _bodyStyle);
            GUILayout.Space(6);

            var tm = Progression.TechTreeManager.Instance;
            if (tm == null)
            {
                GUILayout.Label("Tech Tree Manager not found in scene.", _subHeaderStyle);
                GUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }

            // Current Active Research Bar
            if (tm.CurrentResearch != null)
            {
                GUILayout.BeginVertical(_cardStyle);
                GUILayout.Label($"🔬 <b>Researching:</b> <color=#FFD700>{tm.CurrentResearch.title}</color> ({tm.CurrentResearchRemaining:F1}s remaining)", _subHeaderStyle);
                Rect barRect = GUILayoutUtility.GetRect(modalWidth - 48, 16);
                GUI.DrawTexture(barRect, _progressBarBgTex);
                float fill = (barRect.width - 4) * tm.CurrentResearchProgress;
                if (fill > 0.01f)
                {
                    GUI.DrawTexture(new Rect(barRect.x + 2, barRect.y + 2, fill, barRect.height - 4), _progressBarFillTex);
                }
                GUILayout.EndVertical();
                GUILayout.Space(6);
            }

            // Tech Grid (2 Columns)
            var allTechs = new List<Progression.TechItem>(tm.GetAllTechs());
            for (int i = 0; i < allTechs.Count; i += 2)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 2; j++)
                {
                    int idx = i + j;
                    if (idx < allTechs.Count)
                    {
                        DrawTechItemCard(allTechs[idx], tm, (modalWidth - 48) * 0.5f);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawTechItemCard(Progression.TechItem item, Progression.TechTreeManager tm, float cardWidth)
        {
            GUILayout.BeginVertical(_cardStyle, GUILayout.Width(cardWidth), GUILayout.Height(98));

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{item.iconEmoji} <b>{item.title}</b>", _subHeaderStyle);
            GUILayout.Label($"<size=10><color=#AAAAAA>{item.branch}</color></size>", _bodyStyle, GUILayout.Width(130));
            GUILayout.EndHorizontal();

            GUILayout.Label($"<size=10><color=#E0E0E0>{item.description}</color></size>", _bodyStyle);

            string costStr = "";
            if (item.foodCost > 0) costStr += $"{item.foodCost}F ";
            if (item.woodCost > 0) costStr += $"{item.woodCost}W ";
            if (item.stoneCost > 0) costStr += $"{item.stoneCost}S ";
            if (item.goldCost > 0) costStr += $"{item.goldCost}G ";

            GUILayout.BeginHorizontal();
            GUILayout.Label($"<size=10><color=#FFD700>Cost: {costStr.Trim()}</color></size>", _bodyStyle);

            if (item.isResearched)
            {
                GUI.color = new Color(0.4f, 1f, 0.4f);
                GUILayout.Label("✅ Researched", _subHeaderStyle, GUILayout.Width(100));
                GUI.color = Color.white;
            }
            else if (item.isResearching)
            {
                GUI.color = new Color(1f, 0.85f, 0.2f);
                GUILayout.Label($"⏳ {item.researchProgress * 100f:F0}%", _subHeaderStyle, GUILayout.Width(100));
                GUI.color = Color.white;
            }
            else
            {
                bool canResearch = tm.CanResearch(item.type, out string reason);
                GUI.enabled = canResearch;
                if (GUILayout.Button($"Research ({item.researchDuration:F0}s)", _buttonStyle, GUILayout.Width(110), GUILayout.Height(24)))
                {
                    tm.StartResearch(item.type);
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion

        #region Wonder Victory Banner (AGE-04)

        private void DrawWonderVictoryBanner(float vWidth, float vHeight)
        {
            var wc = Progression.WonderVictoryController.Instance;
            if (wc == null) return;

            if (wc.IsCountdownActive)
            {
                float rem = wc.CountdownRemaining;
                int mins = Mathf.FloorToInt(rem / 60f);
                int secs = Mathf.FloorToInt(rem % 60f);

                float bannerWidth = 440f;
                float xPos = (vWidth - bannerWidth) * 0.5f;
                Rect bannerRect = new Rect(xPos, 74, bannerWidth, 42);

                GUILayout.BeginArea(bannerRect);
                GUILayout.BeginHorizontal(_cardStyle);
                GUILayout.Label($"🏆 <b>WONDER DEFENSE: <color=#FFD700>{mins:D2}:{secs:D2}</color></b> (Survive to Win!)", _headerStyle);
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
            else if (wc.IsVictoryAchieved)
            {
                float modalWidth = 540f;
                float modalHeight = 220f;
                float xPos = (vWidth - modalWidth) * 0.5f;
                float yPos = (vHeight - modalHeight) * 0.5f;

                GUILayout.BeginArea(new Rect(xPos, yPos, modalWidth, modalHeight));
                GUILayout.BeginVertical(_cardStyle);

                GUILayout.Space(8);
                GUILayout.Label("👑 <b>HIMALAYAN GOLDEN AGE VICTORY!</b> 🏆", _headerStyle);
                GUILayout.Space(6);
                GUILayout.Label("<color=#FFD700><b>The Monument of the Ages stands undefeated upon the Himalayan peaks!</b></color>", _subHeaderStyle);
                GUILayout.Label("Your kingdom has unified the lands under peace, prosperity, and martial glory.", _bodyStyle);
                GUILayout.Space(12);

                if (GUILayout.Button("🎉 <b>Continue Governing Gorkha</b>", _goldButtonStyle, GUILayout.Height(44)))
                {
                    GorkhaAudio.PlayClick();
                }

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

        #endregion

        #region Guthi Brotherhood & Historical Campaign Modal (SOC-01 / SOC-03)

        private void DrawGuthiModal(float vWidth, float vHeight)
        {
            float modalWidth = 680f;
            float modalHeight = 460f;
            float xPos = (vWidth - modalWidth) * 0.5f;
            float yPos = (vHeight - modalHeight) * 0.5f;
            _guthiModalRect = new Rect(xPos, yPos, modalWidth, modalHeight);

            GUILayout.BeginArea(_guthiModalRect);
            GUILayout.BeginVertical(_cardStyle);

            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label("🛡️ <b>गुठी समाज (Guthi Brotherhood & Campaign)</b>", _headerStyle);
            if (GUILayout.Button("✖", _buttonStyle, GUILayout.Width(30), GUILayout.Height(28)))
            {
                _showGuthiModal = false;
                GorkhaAudio.PlayClick();
            }
            GUILayout.EndHorizontal();

            var gm = Social.GuthiManager.Instance;
            if (gm == null)
            {
                GUILayout.Label("Guthi Manager not initialized.", _subHeaderStyle);
                GUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }

            // Tab Selector
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_guthiTab == 0 ? "🛡️ <b>[ Guthi Clan Roster ]</b>" : "🛡️ Guthi Clan Roster", _guthiTab == 0 ? _goldButtonStyle : _buttonStyle, GUILayout.Height(34)))
            {
                _guthiTab = 0;
                GorkhaAudio.PlayClick();
            }
            if (GUILayout.Button(_guthiTab == 1 ? "🗺️ <b>[ Himalayan Campaign ]</b>" : "🗺️ Himalayan Campaign", _guthiTab == 1 ? _goldButtonStyle : _buttonStyle, GUILayout.Height(34)))
            {
                _guthiTab = 1;
                GorkhaAudio.PlayClick();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            if (_guthiTab == 0)
            {
                // Tab 0: Guthi Brotherhood & Clan Castle
                GUILayout.BeginVertical(_cardStyle);
                GUILayout.Label($"👑 <b>{gm.GuthiName}</b>", _headerStyle);
                GUILayout.Label($"<color=#FFD700>Rank:</color> <b>{gm.PlayerRank}</b> • <color=#38b6ff>Trophies:</color> <b>{gm.ClanTrophies} 🏆</b> • <color=#69f0ae>Clan Level:</color> <b>Lv. {gm.ClanLevel}</b>", _subHeaderStyle);
                GUILayout.EndVertical();

                GUILayout.Space(6);
                GUILayout.Label($"🏰 <b>Guthi Castle Garrison ({gm.CurrentTroopCount}/{gm.MaxTroopCapacity} Troops):</b>", _subHeaderStyle);

                // Troop List
                GUILayout.BeginVertical(_cardStyle);
                if (gm.CurrentTroopCount == 0)
                {
                    GUILayout.Label("<color=#AAAAAA>No donated troops currently stationed in the Guthi Castle.</color>", _bodyStyle);
                }
                else
                {
                    string troopRoster = "";
                    foreach (var t in gm.DonatedTroops)
                    {
                        troopRoster += t switch
                        {
                            UnitType.KhukuriWarrior => "🗡️ Khukuri  ",
                            UnitType.Spearman => "🛡️ Spearman  ",
                            UnitType.Archer => "🏹 Archer  ",
                            _ => "🌾 Villager  "
                        };
                    }
                    GUILayout.Label(troopRoster, _subHeaderStyle);
                }
                GUILayout.EndVertical();

                GUILayout.Space(6);
                GUILayout.BeginHorizontal();

                // Request Troops Button
                bool canReq = gm.CanRequestTroops;
                GUI.enabled = canReq;
                string reqLabel = canReq ? "📥 <b>Request Reinforcements</b>" : $"⏳ Cooldown ({gm.RequestCooldownRemaining:F0}s)";
                if (GUILayout.Button(reqLabel, _goldButtonStyle, GUILayout.Height(40)))
                {
                    gm.RequestClanTroops(out _);
                }
                GUI.enabled = true;

                // Deploy Garrison Button
                GUI.enabled = gm.CurrentTroopCount > 0;
                if (GUILayout.Button("⚔️ <b>Deploy Garrison Defenders</b>", _greenButtonStyle, GUILayout.Height(40)))
                {
                    gm.DeployGarrisonDefenders(new Vector3(24f, 0, 24f));
                    _showGuthiModal = false;
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            else
            {
                // Tab 1: Historical Campaign Acts
                GUILayout.Label("🗺️ <b>5 Historical Campaign Chapters (Unification of Gorkha):</b>", _subHeaderStyle);
                GUILayout.Space(4);

                foreach (var mission in gm.CampaignMissions)
                {
                    GUILayout.BeginVertical(_cardStyle);
                    GUILayout.BeginHorizontal();
                    string starStr = mission.starsEarned > 0 ? new string('⭐', mission.starsEarned) : "⚪⚪⚪";
                    GUILayout.Label($"<b>{mission.title}</b> ({mission.year})  {starStr}", _subHeaderStyle);
                    GUILayout.Label($"<size=10><color=#FFD700>+{mission.rewardGold}🪙 +{mission.rewardGems}💎</color></size>", _bodyStyle, GUILayout.Width(110));
                    GUILayout.EndHorizontal();

                    GUILayout.Label($"<size=10><color=#CCCCCC>{mission.historicalContext}</color></size>", _bodyStyle);
                    GUILayout.Label($"<size=10><color=#88FF88>🎯 Objective: {mission.objective}</color></size>", _bodyStyle);

                    GUILayout.BeginHorizontal();
                    if (mission.isCompleted)
                    {
                        GUI.color = new Color(0.4f, 1f, 0.4f);
                        GUILayout.Label("✅ Conquered", _subHeaderStyle, GUILayout.Width(110));
                        GUI.color = Color.white;
                    }
                    if (GUILayout.Button(mission.isCompleted ? "Replay Act" : "⚔️ Launch Battle", _buttonStyle, GUILayout.Width(130), GUILayout.Height(24)))
                    {
                        gm.CompleteMission(mission.actNumber, 3);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.Space(4);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}
