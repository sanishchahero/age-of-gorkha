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
    /// 1. Top Screen: Player Era & Kingdom Profile (Left), Wonder/Raid Banner (Center), Resource Bar & Time of Day Widget (Right)
    /// 2. Bottom-Right: Iconic CoC Action Hub ([BUILD], [ATTACK], [GUTHI], [TECH])
    /// 3. Bottom-Left: Non-overlapping Minimap Radar View
    /// 4. Bottom-Center: Contextual Floating Trays (Construction Drawer, Building Inspector, Army Battalion Bar)
    /// 5. Fullscreen Popups: Vidya Kendra Tech Tree and Guthi Brotherhood / Campaign Modals with smooth scrolling
    /// </summary>
    public class GorkhaPlayableHUD : MonoBehaviour
    {
        public static GorkhaPlayableHUD Instance { get; private set; }

        private static float _userScaleMultiplier = 1.20f;
        public static float UserScaleMultiplier
        {
            get => _userScaleMultiplier;
            set
            {
                _userScaleMultiplier = Mathf.Clamp(value, 0.75f, 2.5f);
                PlayerPrefs.SetFloat("Gorkha_UIScale", _userScaleMultiplier);
            }
        }

        public static float GetEffectiveScale()
        {
            float autoScale = Mathf.Clamp(Screen.height / 800f, 1.0f, 2.5f);
            return autoScale * _userScaleMultiplier;
        }

        public static void CycleUserScale()
        {
            if (_userScaleMultiplier < 1.15f) UserScaleMultiplier = 1.20f;
            else if (_userScaleMultiplier < 1.35f) UserScaleMultiplier = 1.40f;
            else if (_userScaleMultiplier < 1.60f) UserScaleMultiplier = 1.65f;
            else if (_userScaleMultiplier < 1.85f) UserScaleMultiplier = 2.0f;
            else UserScaleMultiplier = 1.0f;
        }

        // Active Selections & State
        private BuildingInstance _selectedBuilding;
        private UnitController _selectedUnit;
        private readonly List<FloatingTextItem> _floatingTexts = new();
        private float _lastClickTime = 0f;

        // Drawers and Modals
        private bool _showBuildTray = false;
        private int _selectedBuildCategory = 0; // 0=Economy, 1=Storage, 2=Defenses, 3=Monuments
        private bool _showTechModal = false;
        private Vector2 _techScrollPos = Vector2.zero;
        private bool _showGuthiModal = false;
        private int _guthiTab = 0; // 0=Clan, 1=Campaign
        private Vector2 _guthiScrollPos = Vector2.zero;
        private Vector2 _inspectorScrollPos = Vector2.zero;

        // Custom GUI Textures
        private Texture2D _cardBgTex;
        private Texture2D _panelBgTex;
        private Texture2D _btnNormalTex;
        private Texture2D _btnHoverTex;
        private Texture2D _btnGoldNormalTex;
        private Texture2D _btnGoldHoverTex;
        private Texture2D _btnCrimsonNormalTex;
        private Texture2D _btnCrimsonHoverTex;
        private Texture2D _btnBlueNormalTex;
        private Texture2D _btnBlueHoverTex;
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
        private GUIStyle _panelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _goldButtonStyle;
        private GUIStyle _crimsonButtonStyle;
        private GUIStyle _blueButtonStyle;
        private GUIStyle _greenButtonStyle;
        private GUIStyle _dangerButtonStyle;
        private GUIStyle _floaterStyle;
        private GUIStyle _dockCardStyle;
        private bool _stylesInitialized = false;

        // UI Hitbox Rectangles
        private Rect _topLeftProfileRect;
        private Rect _topRightResourceRect;
        private Rect _topRightUtilityRect;
        private Rect _bottomRightActionRect;
        private Rect _bottomCenterDrawerRect;
        private Rect _inspectorRect;
        private Rect _unitInspectorRect;
        private Rect _techModalRect;
        private Rect _guthiModalRect;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _userScaleMultiplier = PlayerPrefs.GetFloat("Gorkha_UIScale", 1.20f);
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
            _showBuildTray = false; // Close build drawer when inspecting a building
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
            Vector2 guiPos = new Vector2(screenPos.x / scale, (Screen.height - screenPos.y) / scale);

            if (_topLeftProfileRect.Contains(guiPos)) return true;
            if (_topRightResourceRect.Contains(guiPos)) return true;
            if (_topRightUtilityRect.Contains(guiPos)) return true;
            if (_bottomRightActionRect.Contains(guiPos)) return true;
            if (_showBuildTray && _bottomCenterDrawerRect.Contains(guiPos)) return true;
            if (_selectedBuilding != null && _inspectorRect.Contains(guiPos)) return true;
            if (RTSSelectionManager.Instance != null && RTSSelectionManager.Instance.SelectedUnits.Count > 0 && _unitInspectorRect.Contains(guiPos)) return true;
            if (_showTechModal && _techModalRect.Contains(guiPos)) return true;
            if (_showGuthiModal && _guthiModalRect.Contains(guiPos)) return true;
            if (GorkhaMiniMap.Instance != null && GorkhaMiniMap.Instance.MapRect.Contains(guiPos)) return true;

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
                    Duration = 1.4f
                });
            }
        }

        private void Update()
        {
            HandleGlobalHotkeys();
            Handle3DMouseInteractions();
            UpdateFloatingTexts();
        }

        private void HandleGlobalHotkeys()
        {
            // Escape: Back out of modals, drawers, blueprints, or selections in clean order
            if (GorkhaInput.GetKeyDown(KeyCode.Escape))
            {
                if (BuildingPlacementController.Instance != null && BuildingPlacementController.Instance.IsPlacing)
                {
                    BuildingPlacementController.Instance.CancelPlacement();
                    GorkhaAudio.PlayClick();
                }
                else if (_showTechModal)
                {
                    _showTechModal = false;
                    GorkhaAudio.PlayClick();
                }
                else if (_showGuthiModal)
                {
                    _showGuthiModal = false;
                    GorkhaAudio.PlayClick();
                }
                else if (_showBuildTray)
                {
                    _showBuildTray = false;
                    GorkhaAudio.PlayClick();
                }
                else if (_selectedBuilding != null)
                {
                    DeselectBuilding();
                    GorkhaAudio.PlayClick();
                }
                else if (RTSSelectionManager.Instance != null && RTSSelectionManager.Instance.SelectedUnits.Count > 0)
                {
                    RTSSelectionManager.Instance.ClearSelection();
                    GorkhaAudio.PlayClick();
                }
            }

            // [B] Toggle Build Drawer
            if (GorkhaInput.GetKeyDown(KeyCode.B) && !IsPlacingBlueprint())
            {
                _showBuildTray = !_showBuildTray;
                if (_showBuildTray) DeselectBuilding();
                GorkhaAudio.PlayClick();
            }

            // [G] Toggle Guthi Modal
            if (GorkhaInput.GetKeyDown(KeyCode.G))
            {
                _showGuthiModal = !_showGuthiModal;
                GorkhaAudio.PlayClick();
            }

            // [T] Toggle Tech Tree Modal
            if (GorkhaInput.GetKeyDown(KeyCode.T))
            {
                _showTechModal = !_showTechModal;
                GorkhaAudio.PlayClick();
            }

            // [N] Cycle Time of Day
            if (GorkhaInput.GetKeyDown(KeyCode.N))
            {
                HimalayanDayNightCycle.Instance?.CycleNextTimeOfDay();
                GorkhaAudio.PlayClick();
            }

            // Hero Skills [Q], [W], [E], [R]
            if (RTSSelectionManager.Instance != null)
            {
                var selected = RTSSelectionManager.Instance.SelectedUnits;
                if (selected.Count == 1 && selected[0] != null && selected[0].TryGetComponent<Units.HeroController>(out var hero))
                {
                    if (GorkhaInput.GetKeyDown(KeyCode.Q)) hero.TriggerAbility1();
                    else if (GorkhaInput.GetKeyDown(KeyCode.E) || GorkhaInput.GetKeyDown(KeyCode.W) || GorkhaInput.GetKeyDown(KeyCode.R)) hero.TriggerAbility2();
                }
            }
        }

        private bool IsPlacingBlueprint()
        {
            return BuildingPlacementController.Instance != null && BuildingPlacementController.Instance.IsPlacing;
        }

        private void Handle3DMouseInteractions()
        {
            if (IsPlacingBlueprint()) return;

            // Left Click Handling
            if (GorkhaInput.GetMouseButtonDown(0))
            {
                Vector2 mousePos = GorkhaInput.MousePosition;
                if (IsPointerOverHUD(mousePos)) return;

                if (Camera.main == null) return;
                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit, 200f))
                {
                    // 1. Harvest bubble click
                    if (hit.collider.name.Contains("Bubble") || (hit.collider.transform.parent != null && hit.collider.transform.parent.name.Contains("Bubble")))
                    {
                        var b = hit.collider.GetComponentInParent<BuildingInstance>();
                        if (b != null && b.State == BuildingState.Active && b.Data != null && b.Data.hasProduction)
                        {
                            double amt = b.CollectResource();
                            SpawnFloatingText(b.transform.position + Vector3.up * 3.5f, $"+{(int)amt} {b.Data.productionType}", new Color(1f, 0.9f, 0.2f));
                            return;
                        }
                    }

                    // 2. Harvestable Resource Node click (Villager assign)
                    var resNode = hit.collider.GetComponentInParent<ResourceNode>();
                    if (resNode != null && !resNode.IsDepleted)
                    {
                        var villager = UnitController.FindNearestIdleVillager(resNode.transform.position);
                        if (villager != null)
                        {
                            villager.AssignHarvestTask(resNode);
                            string verb = resNode.NodeType == ResourceNodeType.Tree ? "🪓 Chopping Timber" : "⛏️ Mining Slate";
                            SpawnFloatingText(resNode.transform.position + Vector3.up * 2f, $"{verb}!", Color.yellow);
                            GorkhaAudio.PlayClick();
                        }
                        else
                        {
                            SpawnFloatingText(resNode.transform.position + Vector3.up * 2f, "All Villagers Busy!", Color.red);
                            GorkhaAudio.PlayError();
                        }
                        return;
                    }

                    // 3. Building selection
                    var building = hit.collider.GetComponentInParent<BuildingInstance>();
                    if (building != null)
                    {
                        if (Time.time - _lastClickTime < 0.35f && _selectedBuilding == building)
                        {
                            IsometricCameraController.Instance?.FocusOn(building.transform.position);
                        }
                        _lastClickTime = Time.time;

                        if (_selectedBuilding != null && _selectedBuilding != building)
                        {
                            _selectedBuilding.Deselect();
                        }

                        _selectedBuilding = building;
                        _selectedBuilding.Select();
                        _selectedUnit = null;
                        _showBuildTray = false;
                        RTSSelectionManager.Instance?.ClearSelection();
                        GorkhaAudio.PlayClick();
                        return;
                    }

                    // 4. Unit selection
                    var unit = hit.collider.GetComponentInParent<UnitController>();
                    if (unit != null)
                    {
                        DeselectBuilding();
                        _selectedUnit = unit;
                        _showBuildTray = false;
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

                    // 5. Ground click deselect
                    DeselectBuilding();
                    _showBuildTray = false;
                }
            }

            // Right Click Handling (Move / Harvest / Attack)
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

            // CoC High Contrast Procedural Textures
            _cardBgTex = MakeBorderedTexture(32, 32, new Color(0.10f, 0.13f, 0.18f, 0.95f), new Color(0.85f, 0.70f, 0.28f, 0.90f), 2);
            _panelBgTex = MakeBorderedTexture(32, 32, new Color(0.07f, 0.09f, 0.13f, 0.96f), new Color(0.40f, 0.45f, 0.55f, 0.85f), 2);
            _btnNormalTex = MakeBorderedTexture(32, 32, new Color(0.18f, 0.24f, 0.33f, 0.96f), new Color(0.38f, 0.48f, 0.62f, 0.90f), 2);
            _btnHoverTex = MakeBorderedTexture(32, 32, new Color(0.26f, 0.35f, 0.48f, 0.98f), new Color(0.98f, 0.82f, 0.32f, 1.0f), 2);

            _btnGoldNormalTex = MakeBorderedTexture(32, 32, new Color(0.82f, 0.58f, 0.14f, 0.98f), new Color(1.0f, 0.90f, 0.40f, 1.0f), 2);
            _btnGoldHoverTex = MakeBorderedTexture(32, 32, new Color(0.95f, 0.72f, 0.20f, 1.0f), new Color(1.0f, 0.98f, 0.70f, 1.0f), 2);

            _btnCrimsonNormalTex = MakeBorderedTexture(32, 32, new Color(0.75f, 0.18f, 0.18f, 0.98f), new Color(1.0f, 0.45f, 0.45f, 1.0f), 2);
            _btnCrimsonHoverTex = MakeBorderedTexture(32, 32, new Color(0.88f, 0.25f, 0.25f, 1.0f), new Color(1.0f, 0.70f, 0.70f, 1.0f), 2);

            _btnBlueNormalTex = MakeBorderedTexture(32, 32, new Color(0.18f, 0.45f, 0.85f, 0.98f), new Color(0.45f, 0.75f, 1.0f, 1.0f), 2);
            _btnBlueHoverTex = MakeBorderedTexture(32, 32, new Color(0.25f, 0.55f, 0.98f, 1.0f), new Color(0.70f, 0.90f, 1.0f, 1.0f), 2);

            _btnGreenNormalTex = MakeBorderedTexture(32, 32, new Color(0.16f, 0.58f, 0.26f, 0.98f), new Color(0.35f, 0.90f, 0.48f, 1.0f), 2);
            _btnGreenHoverTex = MakeBorderedTexture(32, 32, new Color(0.22f, 0.72f, 0.34f, 1.0f), new Color(0.55f, 0.98f, 0.65f, 1.0f), 2);

            _btnDangerTex = MakeBorderedTexture(32, 32, new Color(0.68f, 0.18f, 0.18f, 0.98f), new Color(0.95f, 0.35f, 0.35f, 1.0f), 2);
            _progressBarBgTex = MakeBorderedTexture(16, 16, new Color(0.05f, 0.07f, 0.10f, 0.95f), new Color(0.25f, 0.30f, 0.38f, 0.85f), 1);
            _progressBarFillTex = MakeSolidTexture(16, 16, new Color(0.22f, 0.88f, 0.38f, 1.0f));

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _headerStyle.normal.textColor = new Color(1.0f, 0.92f, 0.65f);

            _subHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft
            };
            _subHeaderStyle.normal.textColor = new Color(0.92f, 0.92f, 0.92f);

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            _bodyStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            _cardStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 8, 8),
                border = new RectOffset(4, 4, 4, 4)
            };
            _cardStyle.normal.background = _cardBgTex;

            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 8, 8),
                border = new RectOffset(4, 4, 4, 4)
            };
            _panelStyle.normal.background = _panelBgTex;

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
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
                border = new RectOffset(3, 3, 3, 3)
            };
            _goldButtonStyle.normal.background = _btnGoldNormalTex;
            _goldButtonStyle.hover.background = _btnGoldHoverTex;
            _goldButtonStyle.normal.textColor = new Color(0.12f, 0.08f, 0.02f);
            _goldButtonStyle.hover.textColor = Color.black;

            _crimsonButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(3, 3, 3, 3)
            };
            _crimsonButtonStyle.normal.background = _btnCrimsonNormalTex;
            _crimsonButtonStyle.hover.background = _btnCrimsonHoverTex;
            _crimsonButtonStyle.normal.textColor = Color.white;
            _crimsonButtonStyle.hover.textColor = new Color(1.0f, 0.90f, 0.90f);

            _blueButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(3, 3, 3, 3)
            };
            _blueButtonStyle.normal.background = _btnBlueNormalTex;
            _blueButtonStyle.hover.background = _btnBlueHoverTex;
            _blueButtonStyle.normal.textColor = Color.white;
            _blueButtonStyle.hover.textColor = new Color(0.85f, 0.95f, 1.0f);

            _greenButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(3, 3, 3, 3)
            };
            _greenButtonStyle.normal.background = _btnGreenNormalTex;
            _greenButtonStyle.hover.background = _btnGreenHoverTex;
            _greenButtonStyle.normal.textColor = Color.white;
            _greenButtonStyle.hover.textColor = new Color(0.9f, 1.0f, 0.9f);

            _dangerButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
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
                fontSize = 11,
                padding = new RectOffset(4, 4, 3, 3),
                fixedHeight = 62
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

            // 1. Top Screen UI Elements
            DrawTopLeftKingdomProfile(vWidth, vHeight);
            DrawTopRightResourceCluster(vWidth, vHeight);
            DrawWonderVictoryBanner(vWidth, vHeight);

            // 2. Bottom-Right Action Hub (Clash of Clans Architecture)
            DrawBottomRightActionHub(vWidth, vHeight);

            // 3. Bottom-Center Floating Contextual Drawers
            if (IsPlacingBlueprint())
            {
                DrawPlacementActionBar(vWidth, vHeight);
            }
            else if (_showBuildTray)
            {
                DrawConstructionTray(vWidth, vHeight);
            }
            else if (_selectedBuilding != null)
            {
                DrawBuildingInspectorModal(vWidth, vHeight);
            }
            else if (RTSSelectionManager.Instance != null && RTSSelectionManager.Instance.SelectedUnits.Count > 0)
            {
                DrawTacticalUnitInspector(vWidth, vHeight);
            }

            // 4. Overlays & Floating Numbers
            DrawFloatingTexts(scale);
            AgeManager.Instance?.DrawCelebrationOverlay(vWidth, vHeight);

            // 5. Fullscreen Modals
            if (_showTechModal) DrawTechResearchModal(vWidth, vHeight);
            if (_showGuthiModal) DrawGuthiModal(vWidth, vHeight);

            GUI.matrix = origMatrix;
        }

        #region Top Left: Kingdom & Era Profile Capsule

        private void DrawTopLeftKingdomProfile(float vWidth, float vHeight)
        {
            float profileWidth = 265f;
            float profileHeight = 54f;
            _topLeftProfileRect = new Rect(12f, 10f, profileWidth, profileHeight);

            GUILayout.BeginArea(_topLeftProfileRect);
            GUILayout.BeginVertical(_cardStyle);

            // Era Banner & Historical Span
            GUILayout.BeginHorizontal();
            if (AgeManager.Instance != null)
            {
                var curAge = AgeManager.Instance.CurrentAgeData;
                GUI.color = curAge.themeColor;
                GUILayout.Label($"🏛️ <b>{curAge.name}</b>", _subHeaderStyle);
                GUI.color = Color.white;
                GUILayout.Label($"<size=9><color=#CCCCCC>{curAge.historicalSpan}</color></size>", _bodyStyle, GUILayout.Width(75));
            }
            else
            {
                GUILayout.Label("🇳🇵 <b>Gorkha Kingdom</b>", _subHeaderStyle);
            }
            GUILayout.EndHorizontal();

            // Row 2: Builders, Population, Shield
            GUILayout.BeginHorizontal();
            if (BuilderManager.Instance != null)
            {
                int free = BuilderManager.Instance.AvailableBuildersCount;
                int total = BuilderManager.Instance.TotalBuilders;
                Color bCol = free > 0 ? new Color(0.4f, 1.0f, 0.4f) : new Color(1.0f, 0.5f, 0.4f);
                GUI.color = bCol;
                GUILayout.Label($"🔨 <b>{free}/{total}</b>", _bodyStyle, GUILayout.Width(50));
                GUI.color = Color.white;
            }

            if (ResourceManager.Instance != null)
            {
                int pop = ResourceManager.Instance.CurrentPopulation;
                int maxPop = ResourceManager.Instance.MaxPopulation;
                Color popCol = pop >= maxPop ? new Color(1.0f, 0.4f, 0.4f) : new Color(0.7f, 0.9f, 1.0f);
                GUI.color = popCol;
                GUILayout.Label($"👥 <b>{pop}/{maxPop}</b>", _bodyStyle, GUILayout.Width(62));
                GUI.color = Color.white;

                if (ResourceManager.Instance.HasActiveShield)
                {
                    int secs = Mathf.RoundToInt(ResourceManager.Instance.ShieldRemainingSeconds);
                    GUILayout.Label($"🛡️ <color=#FFD700>{secs / 60}:{secs % 60:D2}</color>", _bodyStyle);
                }
                else
                {
                    GUILayout.Label("⚔️ <color=#FF6666>Unshielded</color>", _bodyStyle);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region Top Right: Resource Cluster & Utility Toolbar

        private void DrawTopRightResourceCluster(float vWidth, float vHeight)
        {
            float clusterWidth = 560f;
            float clusterHeight = 60f;
            float xPos = vWidth - clusterWidth - 12f;
            _topRightResourceRect = new Rect(xPos, 8f, clusterWidth, clusterHeight);

            GUILayout.BeginArea(_topRightResourceRect);
            GUILayout.BeginVertical(_panelStyle);

            // Row 1: Resource Capsules
            GUILayout.BeginHorizontal();
            if (ResourceManager.Instance != null)
            {
                DrawResourceCapsule("🌾 Food", ResourceManager.Instance.Food, ResourceManager.Instance.GetCapacity(ResourceType.Food), new Color(0.95f, 0.85f, 0.35f));
                DrawResourceCapsule("🪵 Wood", ResourceManager.Instance.Wood, ResourceManager.Instance.GetCapacity(ResourceType.Wood), new Color(0.88f, 0.68f, 0.48f));
                DrawResourceCapsule("🪨 Stone", ResourceManager.Instance.Stone, ResourceManager.Instance.GetCapacity(ResourceType.Stone), new Color(0.78f, 0.82f, 0.88f));
                DrawResourceCapsule("🪙 Gold", ResourceManager.Instance.Gold, ResourceManager.Instance.GetCapacity(ResourceType.Gold), new Color(1.0f, 0.85f, 0.20f));
                DrawResourceCapsule("💎 Gems", ResourceManager.Instance.Gems, 9999, new Color(0.35f, 0.85f, 1.0f));
            }
            GUILayout.EndHorizontal();

            // Row 2: Utility & Time of Day Quick Controls
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();

            // Time of Day Selector Button (Prominently styled with icon and label)
            if (HimalayanDayNightCycle.Instance != null)
            {
                var tod = HimalayanDayNightCycle.Instance.CurrentTime;
                string todLabel = tod switch
                {
                    TimeOfDay.Dawn => "🌅 Dawn",
                    TimeOfDay.Noon => "☀️ Noon",
                    TimeOfDay.Sunset => "🌄 Sunset",
                    _ => "🌙 Night"
                };
                if (GUILayout.Button(todLabel, _goldButtonStyle, GUILayout.Width(88), GUILayout.Height(20)))
                {
                    HimalayanDayNightCycle.Instance.CycleNextTimeOfDay();
                    GorkhaAudio.PlayClick();
                }
            }

            // Quick Center Map
            if (GUILayout.Button("🎯 Center", _buttonStyle, GUILayout.Width(66), GUILayout.Height(20)))
            {
                GorkhaAudio.PlayClick();
                IsometricCameraController.Instance?.ResetView();
            }

            // UI Scale Cycle
            string scaleLabel = $"🔍 {(int)(_userScaleMultiplier * 100)}%";
            if (GUILayout.Button(scaleLabel, _buttonStyle, GUILayout.Width(64), GUILayout.Height(20)))
            {
                CycleUserScale();
                GorkhaAudio.PlayClick();
            }

            // Sound Mute Toggle
            if (GorkhaAudio.Instance != null)
            {
                string soundLabel = GorkhaAudio.Instance.IsMuted ? "🔇 Muted" : "🔊 Sound";
                if (GUILayout.Button(soundLabel, _buttonStyle, GUILayout.Width(72), GUILayout.Height(20)))
                {
                    GorkhaAudio.Instance.IsMuted = !GorkhaAudio.Instance.IsMuted;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();

            _topRightUtilityRect = _topRightResourceRect;
        }

        private void DrawResourceCapsule(string name, double amount, double capacity, Color color)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUI.color = color;
            GUILayout.Label($"{name}: <b>{(int)amount}</b>", _subHeaderStyle);
            GUI.color = Color.white;

            // Mini Capacity Gauge
            float pct = Mathf.Clamp01((float)(amount / Math.Max(1.0, capacity)));
            Rect gaugeRect = GUILayoutUtility.GetRect(65, 3, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(gaugeRect, _progressBarBgTex);
            if (pct > 0.01f)
            {
                GUI.DrawTexture(new Rect(gaugeRect.x, gaugeRect.y, gaugeRect.width * pct, gaugeRect.height), _progressBarFillTex);
            }
            GUILayout.EndVertical();
        }

        #endregion

        #region Bottom-Right: Iconic Clash of Clans Action Hub

        private void DrawBottomRightActionHub(float vWidth, float vHeight)
        {
            float hubWidth = 148f;
            float hubHeight = 142f;
            float xPos = vWidth - hubWidth - 12f;
            float yPos = vHeight - hubHeight - 12f;
            _bottomRightActionRect = new Rect(xPos, yPos, hubWidth, hubHeight);

            GUILayout.BeginArea(_bottomRightActionRect);

            // Row 1: Big Build & Attack Buttons
            GUILayout.BeginHorizontal();
            string buildLabel = _showBuildTray ? "✖\n<size=11><b>CLOSE</b></size>" : "🔨\n<size=11><b>BUILD</b></size>";
            if (GUILayout.Button(buildLabel, _goldButtonStyle, GUILayout.Width(68), GUILayout.Height(66)))
            {
                _showBuildTray = !_showBuildTray;
                if (_showBuildTray) DeselectBuilding();
                GorkhaAudio.PlayClick();
            }

            if (GUILayout.Button("⚔️\n<size=11><b>RAID</b></size>", _crimsonButtonStyle, GUILayout.Width(68), GUILayout.Height(66)))
            {
                GorkhaAudio.PlayClick();
                if (GorkhaRaidBattleController.Instance != null)
                {
                    GorkhaRaidBattleController.Instance.OpenOutpostSelectionMenu();
                }
                else
                {
                    SpawnHostileRaidWave();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // Row 2: Secondary Hub Buttons (Guthi Clan & Tech Tree)
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🛡️ Guthi", _blueButtonStyle, GUILayout.Width(68), GUILayout.Height(38)))
            {
                _showGuthiModal = !_showGuthiModal;
                GorkhaAudio.PlayClick();
            }

            if (GUILayout.Button("🏛️ Tech", _buttonStyle, GUILayout.Width(68), GUILayout.Height(38)))
            {
                _showTechModal = !_showTechModal;
                GorkhaAudio.PlayClick();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        #endregion

        #region Bottom-Center: Placement Action Bar

        private void DrawPlacementActionBar(float vWidth, float vHeight)
        {
            var bp = BuildingPlacementController.Instance.CurrentBlueprint;
            string bpName = bp != null ? bp.displayName : "Structure";
            bool isRelocating = BuildingPlacementController.Instance.IsRelocating;

            float barWidth = 520f;
            float barHeight = 52f;
            float xPos = (vWidth - barWidth) * 0.5f;
            float yPos = vHeight - barHeight - 12f;
            _bottomCenterDrawerRect = new Rect(xPos, yPos, barWidth, barHeight);

            GUILayout.BeginArea(_bottomCenterDrawerRect);
            GUILayout.BeginHorizontal(_cardStyle);

            string statusText = isRelocating
                ? $"📦 Relocating <b>{bpName}</b> — Click ground to place!"
                : $"🔨 Placing <b>{bpName}</b> — Click ground to construct!";
            GUILayout.Label(statusText, _headerStyle);

            if (GUILayout.Button("❌ Cancel (Esc)", _dangerButtonStyle, GUILayout.Width(130), GUILayout.Height(34)))
            {
                GorkhaAudio.PlayClick();
                BuildingPlacementController.Instance.CancelPlacement();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        #endregion

        #region Bottom-Center: Construction Drawer (Clash of Clans Style)

        private void DrawConstructionTray(float vWidth, float vHeight)
        {
            float drawerWidth = Mathf.Min(vWidth - 340f, 840f);
            float drawerHeight = 138f;
            float xPos = (vWidth - drawerWidth) * 0.5f;
            float yPos = vHeight - drawerHeight - 12f;
            _bottomCenterDrawerRect = new Rect(xPos, yPos, drawerWidth, drawerHeight);

            GUILayout.BeginArea(_bottomCenterDrawerRect);
            GUILayout.BeginVertical(_cardStyle);

            // Category Tabs Header
            GUILayout.BeginHorizontal();
            string[] tabs = { "🌾 Economy", "📦 Storage", "⚔️ Military & Defense", "🏆 Monuments & Tech" };
            for (int i = 0; i < tabs.Length; i++)
            {
                GUIStyle style = _selectedBuildCategory == i ? _goldButtonStyle : _buttonStyle;
                if (GUILayout.Button(tabs[i], style, GUILayout.Height(28)))
                {
                    _selectedBuildCategory = i;
                    GorkhaAudio.PlayClick();
                }
            }

            // Close Drawer Button
            if (GUILayout.Button("✖", _buttonStyle, GUILayout.Width(30), GUILayout.Height(28)))
            {
                _showBuildTray = false;
                GorkhaAudio.PlayClick();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // Content Row for Selected Category
            GUILayout.BeginHorizontal();
            if (_selectedBuildCategory == 0)
            {
                // Economy Tab
                DrawGatedDockCard(BuildingType.Farm, "🌾", "Farm", "30W • +2.5/s", () =>
                    StartBlueprintPlacement(BuildingType.Farm, "Terraced Farm", 2, 2, ResourceType.Food, 2.5, woodCost: 30));

                DrawGatedDockCard(BuildingType.WoodCamp, "🪵", "Wood Camp", "30F • +2.0/s", () =>
                    StartBlueprintPlacement(BuildingType.WoodCamp, "Woodcutter Camp", 2, 2, ResourceType.Wood, 2.0, foodCost: 30));

                DrawGatedDockCard(BuildingType.StoneQuarry, "🪨", "Quarry", "50W • +1.0/s", () =>
                    StartBlueprintPlacement(BuildingType.StoneQuarry, "Stone Quarry", 2, 2, ResourceType.Stone, 1.0, woodCost: 50));

                DrawGatedDockCard(BuildingType.House, "🏠", "House", "40W • +5 Pop", () =>
                    StartBlueprintPlacement(BuildingType.House, "Newari House", 2, 2, ResourceType.Food, 0.0, woodCost: 40));
            }
            else if (_selectedBuildCategory == 1)
            {
                // Storage Tab
                DrawGatedDockCard(BuildingType.Granary, "🌾", "Granary", "80W, 40S • +1k F", () =>
                    StartBlueprintPlacement(BuildingType.Granary, "Granary (Bhakari)", 2, 2, ResourceType.Food, 0.0, woodCost: 80, stoneCost: 40, isStorage: true, storageType: ResourceType.Food, storageCapacity: 1000));

                DrawGatedDockCard(BuildingType.TimberYard, "🪵", "Timber Yard", "80W, 40S • +1k W", () =>
                    StartBlueprintPlacement(BuildingType.TimberYard, "Timber Yard", 2, 2, ResourceType.Wood, 0.0, woodCost: 80, stoneCost: 40, isStorage: true, storageType: ResourceType.Wood, storageCapacity: 1000));

                DrawGatedDockCard(BuildingType.SlateDepot, "🪨", "Slate Depot", "80W, 40S • +1k S", () =>
                    StartBlueprintPlacement(BuildingType.SlateDepot, "Slate Depot", 2, 2, ResourceType.Stone, 0.0, woodCost: 80, stoneCost: 40, isStorage: true, storageType: ResourceType.Stone, storageCapacity: 1000));

                DrawGatedDockCard(BuildingType.Vault, "🪙", "Vault", "100W, 60S • +1k G", () =>
                    StartBlueprintPlacement(BuildingType.Vault, "Treasury Vault (Dhukuti)", 2, 2, ResourceType.Gold, 0.0, woodCost: 100, stoneCost: 60, isStorage: true, storageType: ResourceType.Gold, storageCapacity: 1000));
            }
            else if (_selectedBuildCategory == 2)
            {
                // Defense & Military Tab
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
            }
            else
            {
                // Monuments & Tech Tab
                DrawGatedDockCard(BuildingType.Academy, "🏛️", "Academy", "120W, 60S", () =>
                    StartBlueprintPlacement(BuildingType.Academy, "Vidya Kendra (Academy)", 2, 2, ResourceType.Food, 0.0, woodCost: 120, stoneCost: 60));

                DrawGatedDockCard(BuildingType.Wonder, "🏆", "Wonder", "500W, 500S, 300G", () =>
                    StartBlueprintPlacement(BuildingType.Wonder, "Himalayan Wonder", 3, 3, ResourceType.Food, 0.0, woodCost: 500, stoneCost: 500));
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
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
                GUI.color = new Color(0.65f, 0.65f, 0.65f, 0.70f);
                if (GUILayout.Button($"🔒 <b>{name}</b>{countText}\n<size=10><color=#FF8888>{lockReason}</color></size>", _dockCardStyle))
                {
                    GorkhaAudio.PlayError();
                    SpawnFloatingText(new Vector3(24f, 2f, 24f), lockReason, Color.red);
                }
                GUI.color = Color.white;
            }
            else
            {
                if (GUILayout.Button($"{icon} <b>{name}</b>{countText}\n<size=10><color=#D4B070>{costDesc}</color></size>", _dockCardStyle))
                {
                    _showBuildTray = false;
                    onClick?.Invoke();
                }
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

        #region Bottom-Center: Building Inspector Panel (Clash of Clans Style)

        private void DrawBuildingInspectorModal(float vWidth, float vHeight)
        {
            var b = _selectedBuilding;
            if (b == null) return;

            float panelWidth = Mathf.Min(vWidth - 340f, 580f);
            float panelHeight = 240f;
            if (b.Data != null && (b.Data.buildingType == BuildingType.TownCenter || b.Data.buildingType == BuildingType.Barracks))
            {
                panelHeight = 270f;
            }

            float xPos = (vWidth - panelWidth) * 0.5f;
            float yPos = vHeight - panelHeight - 12f;
            _inspectorRect = new Rect(xPos, yPos, panelWidth, panelHeight);

            GUILayout.BeginArea(_inspectorRect);
            GUILayout.BeginVertical(_cardStyle);

            // Title & Level Header
            GUILayout.BeginHorizontal();
            string displayName = b.Data != null ? b.Data.displayName : "Kingdom Building";
            GUILayout.Label($"🏰 <b>{displayName}</b>", _headerStyle);
            GUILayout.Label($"<color=#FFD700><b>[ Lv. {b.Level} ]</b></color>", _subHeaderStyle, GUILayout.Width(75));

            if (GUILayout.Button("✖", _buttonStyle, GUILayout.Width(28), GUILayout.Height(24)))
            {
                GorkhaAudio.PlayClick();
                DeselectBuilding();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }
            GUILayout.EndHorizontal();

            _inspectorScrollPos = GUILayout.BeginScrollView(_inspectorScrollPos);

            // Construction / Upgrade Progress or Operational State
            if (b.State == BuildingState.Constructing || b.State == BuildingState.Upgrading)
            {
                string stateTitle = b.State == BuildingState.Constructing ? "Under Construction" : "Upgrading";
                GUILayout.Label($"🔨 <b>{stateTitle}</b> ({b.RemainingConstructionTime:F1}s remaining)", _subHeaderStyle);

                Rect progRect = GUILayoutUtility.GetRect(panelWidth - 28, 14);
                GUI.DrawTexture(progRect, _progressBarBgTex);
                float fillW = (progRect.width - 4) * Mathf.Clamp01(b.ConstructionProgress);
                if (fillW > 0.01f)
                {
                    GUI.DrawTexture(new Rect(progRect.x + 2, progRect.y + 2, fillW, progRect.height - 4), _progressBarFillTex);
                }

                GUILayout.Space(4);
                if (GUILayout.Button("⚡ Finish Instantly (3 Gems)", _goldButtonStyle, GUILayout.Height(32)))
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
                        SpawnFloatingText(b.transform.position + Vector3.up * 3f, "Need 3 Gems!", Color.red);
                    }
                }
            }
            else
            {
                // Operational Stats
                GUILayout.Label($"🟢 Status: <b>Operational</b>  •  ❤️ <b>{b.Health}/{b.MaxHealth} HP</b>", _subHeaderStyle);

                if (b.Data != null && b.Data.hasProduction)
                {
                    double currentRate = b.Data.baseProductionRate * b.Level;
                    GUILayout.Label($"📈 Yield: <b>+{currentRate:F1} {b.Data.productionType} / sec</b>", _subHeaderStyle);
                }

                if (b.Data != null && b.Data.isStorage)
                {
                    double curAmt = ResourceManager.Instance != null ? ResourceManager.Instance.GetAmount(b.Data.storageType) : 0;
                    double maxCap = ResourceManager.Instance != null ? ResourceManager.Instance.GetCapacity(b.Data.storageType) : 1000;
                    GUILayout.Label($"📦 Kingdom {b.Data.storageType}: <b>{(int)curAmt} / {(int)maxCap}</b>", _subHeaderStyle);
                }

                // CoC Building Action Buttons (Upgrade, Collect, Move)
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();

                // Upgrade Button
                if (b.Data != null)
                {
                    var nextCost = b.Data.GetCostDictionary(b.Level);
                    string costStr = FormatCostString(nextCost);
                    bool canAfford = ResourceManager.Instance != null && ResourceManager.Instance.CanAfford(nextCost);

                    GUI.enabled = canAfford;
                    string upgText = canAfford ? $"⬆️ Upgrade (Lv {b.Level + 1})\n<size=10>{costStr}</size>" : $"❌ Cannot Afford\n<size=10>{costStr}</size>";
                    if (GUILayout.Button(upgText, _greenButtonStyle, GUILayout.Height(38)))
                    {
                        if (b.StartUpgrade())
                        {
                            SpawnFloatingText(b.transform.position + Vector3.up * 3f, "Upgrade Started!", Color.green);
                        }
                    }
                    GUI.enabled = true;
                }

                // Collect Button (if producer)
                if (b.Data != null && b.Data.hasProduction)
                {
                    if (GUILayout.Button("🌾 Collect\n<size=10>Store Yield</size>", _goldButtonStyle, GUILayout.Width(100), GUILayout.Height(38)))
                    {
                        double amt = b.CollectResource();
                        SpawnFloatingText(b.transform.position + Vector3.up * 3.5f, $"+{(int)amt} {b.Data.productionType}", Color.yellow);
                    }
                }

                // Relocate Button
                if (GUILayout.Button("📦 Relocate\n<size=10>Move Base</size>", _buttonStyle, GUILayout.Width(100), GUILayout.Height(38)))
                {
                    GorkhaAudio.PlayClick();
                    var target = b;
                    DeselectBuilding();
                    BuildingPlacementController.Instance.StartRelocation(target);
                    SpawnFloatingText(target.transform.position + Vector3.up * 3f, "Click new ground location!", Color.cyan);
                    GUILayout.EndHorizontal();
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                    return;
                }
                GUILayout.EndHorizontal();

                // Specific Building Action Drawers (Barracks, Town Center, Academy)
                if (b.Data != null && b.Data.buildingType == BuildingType.Barracks)
                {
                    DrawBarracksRecruitmentRow(b);
                }
                else if (b.Data != null && b.Data.buildingType == BuildingType.TownCenter)
                {
                    DrawTownCenterRecruitmentRow(b);
                }
                else if (b.Data != null && b.Data.buildingType == BuildingType.Academy)
                {
                    GUILayout.Space(4);
                    if (GUILayout.Button("🔬 <b>Open Tech Tree Research (Vidya Kendra)</b>", _goldButtonStyle, GUILayout.Height(34)))
                    {
                        _showTechModal = true;
                        GorkhaAudio.PlayClick();
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawBarracksRecruitmentRow(BuildingInstance b)
        {
            GUILayout.Space(4);
            GUILayout.Label("⚔️ <b>Train Army Units:</b>", _subHeaderStyle);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("🗡️ Spearman\n<size=10>25 Food</size>", _buttonStyle, GUILayout.Height(36)))
            {
                TrainUnit(UnitType.Spearman, foodCost: 25);
            }

            bool khukuriUnlocked = BuildingGatingManager.IsUnitUnlocked(UnitType.KhukuriWarrior, out string kReason);
            if (khukuriUnlocked)
            {
                if (GUILayout.Button("⚔️ Khukuri\n<size=10>35F, 15W</size>", _buttonStyle, GUILayout.Height(36)))
                {
                    TrainUnit(UnitType.KhukuriWarrior, foodCost: 35, woodCost: 15);
                }
            }
            else
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                if (GUILayout.Button($"🔒 Khukuri\n<size=9>{kReason}</size>", _buttonStyle, GUILayout.Height(36)))
                {
                    GorkhaAudio.PlayError();
                    SpawnFloatingText(b.transform.position + Vector3.up * 3f, kReason, Color.red);
                }
                GUI.color = Color.white;
            }

            bool archerUnlocked = BuildingGatingManager.IsUnitUnlocked(UnitType.Archer, out string aReason);
            if (archerUnlocked)
            {
                if (GUILayout.Button("🏹 Archer\n<size=10>30F, 20W</size>", _buttonStyle, GUILayout.Height(36)))
                {
                    TrainUnit(UnitType.Archer, foodCost: 30, woodCost: 20);
                }
            }
            else
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                if (GUILayout.Button($"🔒 Archer\n<size=9>{aReason}</size>", _buttonStyle, GUILayout.Height(36)))
                {
                    GorkhaAudio.PlayError();
                    SpawnFloatingText(b.transform.position + Vector3.up * 3f, aReason, Color.red);
                }
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawTownCenterRecruitmentRow(BuildingInstance b)
        {
            GUILayout.Space(4);
            GUILayout.Label("👨‍🌾 <b>Civilian & Hero Recruitment:</b>", _subHeaderStyle);
            GUILayout.BeginHorizontal();

            bool popFull = ResourceManager.Instance != null && !ResourceManager.Instance.CanSupportUnit();
            bool hasFood = ResourceManager.Instance != null && ResourceManager.Instance.Food >= 50;

            GUI.enabled = !popFull && hasFood;
            if (GUILayout.Button("🌾 Villager (50F)", _buttonStyle, GUILayout.Height(34)))
            {
                TrainVillager(50);
            }
            GUI.enabled = true;

            // General Kalu Pande
            bool kaluUnlocked = BuildingGatingManager.IsUnitUnlocked(UnitType.HeroKaluPande, out string kaluReason);
            bool kaluAlive = HeroManager.Instance != null && HeroManager.Instance.IsHeroAlive(HeroGeneralType.KaluPande);
            if (kaluAlive)
            {
                GUI.color = new Color(0.4f, 0.8f, 1f, 0.8f);
                GUILayout.Button("🛡️ Kalu Pande (Active)", _buttonStyle, GUILayout.Height(34));
                GUI.color = Color.white;
            }
            else if (kaluUnlocked)
            {
                if (GUILayout.Button("🛡️ Kalu Pande (150F, 100G)", _goldButtonStyle, GUILayout.Height(34)))
                {
                    HeroManager.Instance?.SummonHero(HeroGeneralType.KaluPande, b.transform.position + new Vector3(2.5f, 0, 0));
                }
            }
            else
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                GUILayout.Button($"🔒 Kalu ({kaluReason})", _buttonStyle, GUILayout.Height(34));
                GUI.color = Color.white;
            }

            // King Prithvi Narayan Shah
            bool prithviUnlocked = BuildingGatingManager.IsUnitUnlocked(UnitType.HeroPrithvi, out string pReason);
            bool prithviAlive = HeroManager.Instance != null && HeroManager.Instance.IsHeroAlive(HeroGeneralType.PrithviNarayanShah);
            if (prithviAlive)
            {
                GUI.color = new Color(1f, 0.85f, 0.2f, 0.8f);
                GUILayout.Button("👑 King Prithvi (Active)", _buttonStyle, GUILayout.Height(34));
                GUI.color = Color.white;
            }
            else if (prithviUnlocked)
            {
                if (GUILayout.Button("👑 King Prithvi (250F, 200G)", _goldButtonStyle, GUILayout.Height(34)))
                {
                    HeroManager.Instance?.SummonHero(HeroGeneralType.PrithviNarayanShah, b.transform.position + new Vector3(-2.5f, 0, 0));
                }
            }
            else
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                GUILayout.Button($"🔒 King ({pReason})", _buttonStyle, GUILayout.Height(34));
                GUI.color = Color.white;
            }

            GUILayout.EndHorizontal();
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

        #region Bottom-Center: Tactical Unit & Army Inspector

        private void DrawTacticalUnitInspector(float vWidth, float vHeight)
        {
            var rts = RTSSelectionManager.Instance;
            if (rts == null || rts.SelectedUnits.Count == 0) return;

            var selected = rts.SelectedUnits;
            bool isSingleHero = selected.Count == 1 && selected[0] != null && selected[0].IsHero;
            float panelWidth = Mathf.Min(vWidth - 340f, 540f);
            float panelHeight = selected.Count == 1 ? (isSingleHero ? 190f : 150f) : 180f;
            float xPos = (vWidth - panelWidth) * 0.5f;
            float yPos = vHeight - panelHeight - 12f;

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
                        UnitType.HeroPrithvi => "👑 King Prithvi Narayan Shah",
                        UnitType.HeroKaluPande => "🛡️ General Kalu Pande",
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

                    // HP Bar
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"<color=#AAAAAA>Health:</color> <b>{u.Health}/{u.MaxHealth}</b>", _subHeaderStyle, GUILayout.Width(130));
                    Rect hpRect = GUILayoutUtility.GetRect(190, 10);
                    GUI.DrawTexture(hpRect, _progressBarBgTex);
                    float fillPct = Mathf.Clamp01((float)u.Health / Mathf.Max(1, u.MaxHealth));
                    if (fillPct > 0.01f)
                    {
                        GUI.DrawTexture(new Rect(hpRect.x + 1, hpRect.y + 1, (hpRect.width - 2) * fillPct, hpRect.height - 2), _progressBarFillTex);
                    }
                    GUILayout.EndHorizontal();

                    if (u.TryGetComponent<HeroController>(out var hero))
                    {
                        GUILayout.Space(4);
                        GUILayout.BeginHorizontal();
                        string a1Name = hero.GeneralType == HeroGeneralType.PrithviNarayanShah ? "👑 [Q] Call to Arms" : "🛡️ [Q] Shield Wall";
                        bool a1Ready = hero.IsAbility1Ready;
                        if (!a1Ready) GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                        if (GUILayout.Button(a1Ready ? $"{a1Name}\n<size=10><color=#FFD700>READY</color></size>" : $"{a1Name}\n<size=10>{hero.Ability1CooldownRemaining:F1}s</size>", _goldButtonStyle, GUILayout.Height(36)))
                        {
                            hero.TriggerAbility1();
                        }
                        GUI.color = Color.white;

                        string a2Name = hero.GeneralType == HeroGeneralType.PrithviNarayanShah ? "⚡ [E] Royal Decree" : "🗡️ [E] Ambush";
                        bool a2Ready = hero.IsAbility2Ready;
                        if (!a2Ready) GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                        if (GUILayout.Button(a2Ready ? $"{a2Name}\n<size=10><color=#69F0AE>READY</color></size>" : $"{a2Name}\n<size=10>{hero.Ability2CooldownRemaining:F1}s</size>", _greenButtonStyle, GUILayout.Height(36)))
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
                // Multi-Unit Formation & Orders
                GUILayout.BeginHorizontal();
                GUILayout.Label($"⚔️ <b>Gorkha Battalion ({selected.Count} Troops)</b>", _headerStyle);
                if (GUILayout.Button("✖", _buttonStyle, GUILayout.Width(26), GUILayout.Height(24)))
                {
                    rts.ClearSelection();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                    return;
                }
                GUILayout.EndHorizontal();

                // Tactical Formations
                GUILayout.BeginHorizontal();
                var cur = rts.CurrentFormation;
                if (GUILayout.Button("[Z] Line", cur == TacticalFormationType.Line ? _goldButtonStyle : _buttonStyle, GUILayout.Height(26)))
                    rts.SetFormation(TacticalFormationType.Line);
                if (GUILayout.Button("[X] Box", cur == TacticalFormationType.Box ? _goldButtonStyle : _buttonStyle, GUILayout.Height(26)))
                    rts.SetFormation(TacticalFormationType.Box);
                if (GUILayout.Button("[C] Wedge", cur == TacticalFormationType.Wedge ? _goldButtonStyle : _buttonStyle, GUILayout.Height(26)))
                    rts.SetFormation(TacticalFormationType.Wedge);
                if (GUILayout.Button("[V] Staggered", cur == TacticalFormationType.Staggered ? _goldButtonStyle : _buttonStyle, GUILayout.Height(26)))
                    rts.SetFormation(TacticalFormationType.Staggered);
                GUILayout.EndHorizontal();
            }

            // Command Orders Row (Attack-Move, Hold, Patrol, Stop)
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            bool isAtkMove = rts.CommandMode == RTSCommandMode.TargetingAttackMove;
            bool isPatrol = rts.CommandMode == RTSCommandMode.TargetingPatrol;

            if (GUILayout.Button(isAtkMove ? "⚔️ <b>[A] Aiming...</b>" : "⚔️ [A] Atk-Move", isAtkMove ? _goldButtonStyle : _buttonStyle, GUILayout.Height(26)))
            {
                rts.SetCommandMode(isAtkMove ? RTSCommandMode.Normal : RTSCommandMode.TargetingAttackMove);
            }
            if (GUILayout.Button("🛡️ [H] Hold", _buttonStyle, GUILayout.Height(26)))
            {
                rts.ExecuteHoldPositionOrder();
            }
            if (GUILayout.Button(isPatrol ? "🚶 <b>[P] Aiming...</b>" : "🚶 [P] Patrol", isPatrol ? _goldButtonStyle : _buttonStyle, GUILayout.Height(26)))
            {
                rts.SetCommandMode(isPatrol ? RTSCommandMode.Normal : RTSCommandMode.TargetingPatrol);
            }
            if (GUILayout.Button("🛑 [S] Halt", _dangerButtonStyle, GUILayout.Height(26)))
            {
                rts.ExecuteStopOrder();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region Fullscreen Modals: Vidya Kendra Tech Tree (AGE-03)

        private void DrawTechResearchModal(float vWidth, float vHeight)
        {
            float modalWidth = Mathf.Min(vWidth - 40f, 720f);
            float modalHeight = Mathf.Min(vHeight - 40f, 480f);
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

            GUILayout.Label("<size=11><color=#CCCCCC>Research ancient metallurgy, architecture, agriculture & logistics.</color></size>", _bodyStyle);
            GUILayout.Space(4);

            var tm = TechTreeManager.Instance;
            if (tm != null)
            {
                if (tm.CurrentResearch != null)
                {
                    GUILayout.BeginVertical(_panelStyle);
                    GUILayout.Label($"🔬 <b>Researching:</b> <color=#FFD700>{tm.CurrentResearch.title}</color> ({tm.CurrentResearchRemaining:F1}s remaining)", _subHeaderStyle);
                    Rect barRect = GUILayoutUtility.GetRect(modalWidth - 40, 14);
                    GUI.DrawTexture(barRect, _progressBarBgTex);
                    float fill = (barRect.width - 4) * tm.CurrentResearchProgress;
                    if (fill > 0.01f)
                    {
                        GUI.DrawTexture(new Rect(barRect.x + 2, barRect.y + 2, fill, barRect.height - 4), _progressBarFillTex);
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(4);
                }

                _techScrollPos = GUILayout.BeginScrollView(_techScrollPos);
                var allTechs = new List<TechItem>(tm.GetAllTechs());
                for (int i = 0; i < allTechs.Count; i += 2)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = 0; j < 2; j++)
                    {
                        int idx = i + j;
                        if (idx < allTechs.Count)
                        {
                            DrawTechItemCard(allTechs[idx], tm, (modalWidth - 50) * 0.5f);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);
                }
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawTechItemCard(TechItem item, TechTreeManager tm, float cardWidth)
        {
            GUILayout.BeginVertical(_panelStyle, GUILayout.Width(cardWidth), GUILayout.Height(92));

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{item.iconEmoji} <b>{item.title}</b>", _subHeaderStyle);
            GUILayout.Label($"<size=10><color=#AAAAAA>{item.branch}</color></size>", _bodyStyle, GUILayout.Width(110));
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
                GUILayout.Label("✅ Researched", _subHeaderStyle, GUILayout.Width(95));
                GUI.color = Color.white;
            }
            else if (item.isResearching)
            {
                GUI.color = new Color(1f, 0.85f, 0.2f);
                GUILayout.Label($"⏳ {item.researchProgress * 100f:F0}%", _subHeaderStyle, GUILayout.Width(95));
                GUI.color = Color.white;
            }
            else
            {
                bool canResearch = tm.CanResearch(item.type, out string _);
                GUI.enabled = canResearch;
                if (GUILayout.Button($"Research ({item.researchDuration:F0}s)", _goldButtonStyle, GUILayout.Width(110), GUILayout.Height(24)))
                {
                    tm.StartResearch(item.type);
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion

        #region Fullscreen Modals: Guthi Brotherhood & Campaign (SOC-01 / SOC-03)

        private void DrawGuthiModal(float vWidth, float vHeight)
        {
            float modalWidth = Mathf.Min(vWidth - 40f, 720f);
            float modalHeight = Mathf.Min(vHeight - 40f, 480f);
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
            if (GUILayout.Button(_guthiTab == 0 ? "🛡️ <b>[ Guthi Clan Roster ]</b>" : "🛡️ Guthi Clan Roster", _guthiTab == 0 ? _goldButtonStyle : _buttonStyle, GUILayout.Height(32)))
            {
                _guthiTab = 0;
                GorkhaAudio.PlayClick();
            }
            if (GUILayout.Button(_guthiTab == 1 ? "🗺️ <b>[ Himalayan Campaign ]</b>" : "🗺️ Himalayan Campaign", _guthiTab == 1 ? _goldButtonStyle : _buttonStyle, GUILayout.Height(32)))
            {
                _guthiTab = 1;
                GorkhaAudio.PlayClick();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            _guthiScrollPos = GUILayout.BeginScrollView(_guthiScrollPos);

            if (_guthiTab == 0)
            {
                // Tab 0: Guthi Brotherhood & Garrison
                GUILayout.BeginVertical(_panelStyle);
                GUILayout.Label($"👑 <b>{gm.GuthiName}</b>", _headerStyle);
                GUILayout.Label($"<color=#FFD700>Rank:</color> <b>{gm.PlayerRank}</b> • <color=#38b6ff>Trophies:</color> <b>{gm.ClanTrophies} 🏆</b> • <color=#69f0ae>Clan Level:</color> <b>Lv. {gm.ClanLevel}</b>", _subHeaderStyle);
                GUILayout.EndVertical();

                GUILayout.Space(4);
                GUILayout.Label($"🏰 <b>Guthi Castle Garrison ({gm.CurrentTroopCount}/{gm.MaxTroopCapacity} Troops):</b>", _subHeaderStyle);

                GUILayout.BeginVertical(_panelStyle);
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

                bool canReq = gm.CanRequestTroops;
                GUI.enabled = canReq;
                string reqLabel = canReq ? "📥 <b>Request Reinforcements</b>" : $"⏳ Cooldown ({gm.RequestCooldownRemaining:F0}s)";
                if (GUILayout.Button(reqLabel, _goldButtonStyle, GUILayout.Height(38)))
                {
                    gm.RequestClanTroops(out _);
                }
                GUI.enabled = true;

                GUI.enabled = gm.CurrentTroopCount > 0;
                if (GUILayout.Button("⚔️ <b>Deploy Garrison Defenders</b>", _greenButtonStyle, GUILayout.Height(38)))
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
                    GUILayout.BeginVertical(_panelStyle);
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

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region Wonder Victory Banner (AGE-04)

        private void DrawWonderVictoryBanner(float vWidth, float vHeight)
        {
            var wc = WonderVictoryController.Instance;
            if (wc == null) return;

            if (wc.IsCountdownActive)
            {
                float rem = wc.CountdownRemaining;
                int mins = Mathf.FloorToInt(rem / 60f);
                int secs = Mathf.FloorToInt(rem % 60f);

                float bannerWidth = 440f;
                float xPos = (vWidth - bannerWidth) * 0.5f;
                Rect bannerRect = new Rect(xPos, 70f, bannerWidth, 38f);

                GUILayout.BeginArea(bannerRect);
                GUILayout.BeginHorizontal(_cardStyle);
                GUILayout.Label($"🏆 <b>WONDER DEFENSE: <color=#FFD700>{mins:D2}:{secs:D2}</color></b> (Survive to Win!)", _headerStyle);
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
            else if (wc.IsVictoryAchieved)
            {
                float modalWidth = 500f;
                float modalHeight = 180f;
                float xPos = (vWidth - modalWidth) * 0.5f;
                float yPos = (vHeight - modalHeight) * 0.5f;

                GUILayout.BeginArea(new Rect(xPos, yPos, modalWidth, modalHeight));
                GUILayout.BeginVertical(_cardStyle);

                GUILayout.Space(6);
                GUILayout.Label("👑 <b>HIMALAYAN GOLDEN AGE VICTORY!</b> 🏆", _headerStyle);
                GUILayout.Space(4);
                GUILayout.Label("<color=#FFD700><b>The Monument of the Ages stands undefeated upon the Himalayan peaks!</b></color>", _subHeaderStyle);
                GUILayout.Space(8);

                if (GUILayout.Button("🎉 <b>Continue Governing Gorkha</b>", _goldButtonStyle, GUILayout.Height(38)))
                {
                    GorkhaAudio.PlayClick();
                }

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

        #endregion

        #region Floating World Space Texts

        private void DrawFloatingTexts(float scale)
        {
            if (Camera.main == null) return;

            foreach (var f in _floatingTexts)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(f.WorldPos);
                if (screenPos.z > 0)
                {
                    float alpha = Mathf.Clamp01(1f - (f.Timer / f.Duration));
                    Color c = f.TextColor;
                    c.a = alpha;

                    float vx = screenPos.x / scale;
                    float vy = (Screen.height - screenPos.y) / scale;

                    _floaterStyle.normal.textColor = new Color(0, 0, 0, alpha * 0.75f);
                    GUI.Label(new Rect(vx - 119, vy - 19, 240, 32), f.Text, _floaterStyle);

                    _floaterStyle.normal.textColor = c;
                    GUI.Label(new Rect(vx - 120, vy - 20, 240, 32), f.Text, _floaterStyle);
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

        #endregion
    }
}
