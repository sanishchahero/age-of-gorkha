using System;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Core;
using Gorkha.CameraSystem;
using Gorkha.Audio;
using Gorkha.UI;
using Gorkha.Graphics;
using Gorkha.Buildings;
using Gorkha.Economy;

namespace Gorkha.Units
{
    /// <summary>
    /// Master RTS Multi-Unit Drag-Box Selection & Control Group Manager.
    /// Features:
    /// - Multi-unit click & drag green marquee selection box with prioritized military filtering.
    /// - Double-click unit to select all visible units of the same type on screen.
    /// - Control Group assignment (Ctrl + 1..9), Recall (1..9), and Double-Tap camera focus.
    /// - Tactical Formations (Line [Z], Box [X], Wedge [C], Staggered [V]).
    /// - Right-click group marching with dynamic slot allocation via ArmyFormationController.
    /// - Coordinated Nepali voice barks and tactical audio cues.
    /// </summary>
    public class RTSSelectionManager : MonoBehaviour
    {
        public static RTSSelectionManager Instance { get; private set; }

        public readonly List<UnitController> SelectedUnits = new();
        public TacticalFormationType CurrentFormation { get; private set; } = TacticalFormationType.Line;

        public event Action OnSelectionChanged;
        public event Action<TacticalFormationType> OnFormationChanged;

        // Control Groups 1..9
        private readonly Dictionary<int, List<UnitController>> _controlGroups = new();
        private int _lastRecalledGroup = -1;
        private float _lastGroupRecallTime = 0f;

        // Marquee Selection Box State
        private bool _isDragging = false;
        private Vector2 _dragStartScreen;
        private const float DragThreshold = 8f;

        // Double Click Detection
        private float _lastClickTime = 0f;
        private UnitController _lastClickedUnit = null;

        // Marquee GUI drawing textures
        private Texture2D _boxBackgroundTex;
        private Texture2D _boxBorderTex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeMarqueeTextures();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeMarqueeTextures()
        {
            if (_boxBackgroundTex == null)
            {
                _boxBackgroundTex = new Texture2D(1, 1);
                _boxBackgroundTex.SetPixel(0, 0, new Color(0.18f, 0.95f, 0.40f, 0.16f)); // Translucent Emerald Green
                _boxBackgroundTex.Apply();
            }

            if (_boxBorderTex == null)
            {
                _boxBorderTex = new Texture2D(1, 1);
                _boxBorderTex.SetPixel(0, 0, new Color(0.25f, 1.0f, 0.48f, 0.88f)); // Bright Glowing Green Border
                _boxBorderTex.Apply();
            }
        }

        private void Update()
        {
            PruneDeadUnits();

            HandleFormationHotkeys();
            HandleControlGroupHotkeys();
            HandleMouseSelection();
            HandleMouseCommands();
        }

        private void PruneDeadUnits()
        {
            bool removed = false;
            for (int i = SelectedUnits.Count - 1; i >= 0; i--)
            {
                if (SelectedUnits[i] == null || !SelectedUnits[i].IsAlive)
                {
                    SelectedUnits.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
            {
                OnSelectionChanged?.Invoke();
            }
        }

        #region Formation Switching

        public void SetFormation(TacticalFormationType formation)
        {
            CurrentFormation = formation;
            string keyHint = formation switch
            {
                TacticalFormationType.Line => "Z",
                TacticalFormationType.Box => "X",
                TacticalFormationType.Wedge => "C",
                _ => "V"
            };

            GorkhaAudio.PlayClick();
            if (SelectedUnits.Count > 0)
            {
                GorkhaPlayableHUD.SpawnFloatingText(
                    SelectedUnits[0].transform.position + Vector3.up * 2.2f,
                    $"Tactical Formation: {formation} [{keyHint}]",
                    new Color(0.3f, 1f, 0.5f));
            }

            OnFormationChanged?.Invoke(formation);
        }

        private void HandleFormationHotkeys()
        {
            if (GorkhaInput.GetKeyDown(KeyCode.Z)) SetFormation(TacticalFormationType.Line);
            else if (GorkhaInput.GetKeyDown(KeyCode.X)) SetFormation(TacticalFormationType.Box);
            else if (GorkhaInput.GetKeyDown(KeyCode.C)) SetFormation(TacticalFormationType.Wedge);
            else if (GorkhaInput.GetKeyDown(KeyCode.V)) SetFormation(TacticalFormationType.Staggered);
        }

        #endregion

        #region Control Groups (Ctrl + 1..9, Recall 1..9, Double-Tap Focus)

        private void HandleControlGroupHotkeys()
        {
            KeyCode[] numKeys = {
                KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
                KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
                KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
            };

            for (int i = 0; i < numKeys.Length; i++)
            {
                int groupNum = i + 1;
                if (GorkhaInput.GetKeyDown(numKeys[i]))
                {
                    if (GorkhaInput.IsCtrlHeld())
                    {
                        AssignControlGroup(groupNum);
                    }
                    else
                    {
                        RecallControlGroup(groupNum);
                    }
                    break;
                }
            }
        }

        public void AssignControlGroup(int groupNum)
        {
            List<UnitController> groupList = new();
            foreach (var u in SelectedUnits)
            {
                if (u != null && u.IsAlive && !u.IsHostile)
                {
                    groupList.Add(u);
                }
            }

            _controlGroups[groupNum] = groupList;
            GorkhaAudio.PlayUpgrade();

            if (groupList.Count > 0)
            {
                GorkhaPlayableHUD.SpawnFloatingText(
                    groupList[0].transform.position + Vector3.up * 2.5f,
                    $"Control Group {groupNum} Created ({groupList.Count} units)",
                    Color.yellow);
            }
        }

        public void RecallControlGroup(int groupNum)
        {
            if (!_controlGroups.TryGetValue(groupNum, out var list) || list == null)
            {
                return;
            }

            // Remove destroyed units
            list.RemoveAll(u => u == null || !u.IsAlive);
            if (list.Count == 0) return;

            // Check for double-tap to focus camera
            bool isDoubleTap = (_lastRecalledGroup == groupNum && Time.time - _lastGroupRecallTime < 0.35f);
            _lastRecalledGroup = groupNum;
            _lastGroupRecallTime = Time.time;

            SelectUnits(list, additive: false);

            if (isDoubleTap && IsometricCameraController.Instance != null)
            {
                Vector3 centroid = Vector3.zero;
                foreach (var u in list) centroid += u.transform.position;
                centroid /= list.Count;
                IsometricCameraController.Instance.FocusOn(centroid);
            }
        }

        #endregion

        #region Mouse Selection & Marquee Drag Box

        private void HandleMouseSelection()
        {
            Vector3 mousePos = GorkhaInput.MousePosition;

            // If mouse is interacting with HUD or placement is active, ignore
            if (BuildingPlacementController.Instance != null && BuildingPlacementController.Instance.IsPlacing)
            {
                _isDragging = false;
                return;
            }

            if (GorkhaPlayableHUD.Instance != null && GorkhaPlayableHUD.Instance.IsPointerOverHUD(mousePos))
            {
                if (!_isDragging) return;
            }

            // Left Mouse Down: begin tracking drag start
            if (GorkhaInput.GetMouseButtonDown(0))
            {
                _dragStartScreen = mousePos;
                _isDragging = false;
            }

            // Left Mouse Held: test if dragged past threshold
            if (GorkhaInput.GetMouseButton(0))
            {
                if (!_isDragging && Vector2.Distance(mousePos, _dragStartScreen) > DragThreshold)
                {
                    _isDragging = true;
                }
            }

            // Left Mouse Up: finalize selection
            if (GorkhaInput.GetMouseButtonUp(0))
            {
                if (_isDragging)
                {
                    _isDragging = false;
                    ApplyMarqueeSelection(_dragStartScreen, mousePos);
                }
                else
                {
                    ApplySingleClickSelection(mousePos);
                }
            }
        }

        private void ApplySingleClickSelection(Vector3 screenPos)
        {
            if (Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 250f))
            {
                // Check if hit a unit
                var unit = hit.collider.GetComponentInParent<UnitController>();
                if (unit != null && !unit.IsHostile && unit.IsAlive)
                {
                    // Double Click Check -> Select All Of Type On Screen
                    if (_lastClickedUnit == unit && Time.time - _lastClickTime < 0.35f)
                    {
                        SelectAllOfTypeOnScreen(unit.Type);
                    }
                    else
                    {
                        bool shift = GorkhaInput.IsShiftHeld();
                        SelectUnit(unit, additive: shift);
                    }

                    _lastClickedUnit = unit;
                    _lastClickTime = Time.time;
                    return;
                }

                // If clicked building or resource node without shift, clear selection
                if (!GorkhaInput.IsShiftHeld())
                {
                    var building = hit.collider.GetComponentInParent<BuildingInstance>();
                    var node = hit.collider.GetComponentInParent<ResourceNode>();
                    if (building != null || node != null)
                    {
                        ClearSelection();
                    }
                }
            }
        }

        private void ApplyMarqueeSelection(Vector2 startPos, Vector2 endPos)
        {
            if (Camera.main == null) return;

            float minX = Mathf.Min(startPos.x, endPos.x);
            float maxX = Mathf.Max(startPos.x, endPos.x);
            float minY = Mathf.Min(startPos.y, endPos.y);
            float maxY = Mathf.Max(startPos.y, endPos.y);

            Rect screenBounds = new Rect(minX, minY, maxX - minX, maxY - minY);

            List<UnitController> militaryInBox = new();
            List<UnitController> civiliansInBox = new();

            foreach (var u in UnitController.AllUnits)
            {
                if (u == null || !u.IsAlive || u.IsHostile) continue;

                Vector3 screenPoint = Camera.main.WorldToScreenPoint(u.transform.position);
                if (screenPoint.z > 0 && screenBounds.Contains(new Vector2(screenPoint.x, screenPoint.y)))
                {
                    if (u.IsVillager)
                    {
                        civiliansInBox.Add(u);
                    }
                    else
                    {
                        militaryInBox.Add(u);
                    }
                }
            }

            bool shift = GorkhaInput.IsShiftHeld();

            // AoE standard: prioritize military troops if both are present in the box
            if (militaryInBox.Count > 0)
            {
                SelectUnits(militaryInBox, additive: shift);
            }
            else if (civiliansInBox.Count > 0)
            {
                SelectUnits(civiliansInBox, additive: shift);
            }
            else if (!shift)
            {
                ClearSelection();
            }
        }

        public void SelectAllOfTypeOnScreen(UnitType type)
        {
            if (Camera.main == null) return;

            List<UnitController> matches = new();
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

            foreach (var u in UnitController.AllUnits)
            {
                if (u == null || !u.IsAlive || u.IsHostile || u.Type != type) continue;

                Vector3 sp = Camera.main.WorldToScreenPoint(u.transform.position);
                if (sp.z > 0 && screenRect.Contains(new Vector2(sp.x, sp.y)))
                {
                    matches.Add(u);
                }
            }

            SelectUnits(matches, additive: false);

            if (matches.Count > 0)
            {
                GorkhaPlayableHUD.SpawnFloatingText(
                    matches[0].transform.position + Vector3.up * 2.2f,
                    $"Selected All {type}s ({matches.Count})",
                    Color.cyan);
            }
        }

        public void SelectUnit(UnitController unit, bool additive = false)
        {
            if (!additive)
            {
                ClearSelection();
            }

            if (unit != null && unit.IsAlive && !unit.IsHostile)
            {
                if (!SelectedUnits.Contains(unit))
                {
                    SelectedUnits.Add(unit);
                    unit.SetSelected(true);
                }
                else if (additive)
                {
                    // Toggle off if holding shift and already selected
                    DeselectUnit(unit);
                    return;
                }

                GorkhaVoiceBarks.PlaySelectBark(unit);
                GorkhaPlayableHUD.Instance?.DeselectBuilding();
            }

            OnSelectionChanged?.Invoke();
        }

        public void SelectUnits(IEnumerable<UnitController> units, bool additive = false)
        {
            if (!additive)
            {
                ClearSelection();
            }

            bool addedAny = false;
            foreach (var u in units)
            {
                if (u != null && u.IsAlive && !u.IsHostile && !SelectedUnits.Contains(u))
                {
                    SelectedUnits.Add(u);
                    u.SetSelected(true);
                    addedAny = true;
                }
            }

            if (addedAny && SelectedUnits.Count > 0)
            {
                GorkhaVoiceBarks.PlaySelectBark(SelectedUnits[0]);
                GorkhaPlayableHUD.Instance?.DeselectBuilding();
            }

            OnSelectionChanged?.Invoke();
        }

        public void DeselectUnit(UnitController unit)
        {
            if (unit != null)
            {
                unit.SetSelected(false);
                SelectedUnits.Remove(unit);
                OnSelectionChanged?.Invoke();
            }
        }

        public void ClearSelection()
        {
            foreach (var u in SelectedUnits)
            {
                if (u != null)
                {
                    u.SetSelected(false);
                }
            }
            SelectedUnits.Clear();
            OnSelectionChanged?.Invoke();
        }

        #endregion

        #region Right-Click Tactical Group Commands

        private void HandleMouseCommands()
        {
            if (SelectedUnits.Count == 0) return;
            if (!GorkhaInput.GetMouseButtonDown(1)) return;
            if (Camera.main == null) return;

            Vector3 mousePos = GorkhaInput.MousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            // 1. Check if clicked a hostile unit (Attack Order)
            if (Physics.Raycast(ray, out RaycastHit hit, 250f))
            {
                var hostile = hit.collider.GetComponentInParent<UnitController>();
                if (hostile != null && hostile.IsHostile && hostile.IsAlive)
                {
                    ExecuteAttackOrder(hostile);
                    return;
                }

                // 2. Check if clicked a harvestable resource node with villagers
                var resNode = hit.collider.GetComponentInParent<ResourceNode>();
                if (resNode != null && !resNode.IsDepleted)
                {
                    ExecuteHarvestOrder(resNode);
                    return;
                }

                // 2b. Check if clicked a farm with villagers
                var farmBuilding = hit.collider.GetComponentInParent<BuildingInstance>();
                if (farmBuilding != null && farmBuilding.Data != null && farmBuilding.Data.buildingType == BuildingType.Farm && farmBuilding.State == BuildingState.Active)
                {
                    ExecuteFarmOrder(farmBuilding);
                    return;
                }
            }

            // 3. Ground Click: March group in formation
            Vector3 targetPos = Vector3.zero;
            bool groundHit = false;

            if (Physics.Raycast(ray, out RaycastHit groundRayHit, 250f))
            {
                targetPos = groundRayHit.point;
                groundHit = true;
            }
            else
            {
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                if (groundPlane.Raycast(ray, out float enter))
                {
                    targetPos = ray.GetPoint(enter);
                    groundHit = true;
                }
            }

            if (groundHit)
            {
                ExecuteMarchOrder(targetPos);
            }
        }

        private void ExecuteAttackOrder(UnitController hostile)
        {
            int militaryCount = 0;
            foreach (var u in SelectedUnits)
            {
                if (u != null && u.IsAlive)
                {
                    u.OrderAttack(hostile);
                    militaryCount++;
                }
            }

            if (militaryCount > 0 && SelectedUnits.Count > 0)
            {
                GorkhaVoiceBarks.PlayAttackOrderBark(SelectedUnits[0]);
                GorkhaPlayableHUD.SpawnFloatingText(
                    hostile.transform.position + Vector3.up * 2.2f,
                    "⚔️ Attack Order!",
                    Color.red);
            }
        }

        private void ExecuteHarvestOrder(ResourceNode node)
        {
            int villagersAssigned = 0;
            foreach (var u in SelectedUnits)
            {
                if (u != null && u.IsAlive && u.IsVillager)
                {
                    u.AssignHarvestTask(node);
                    villagersAssigned++;
                }
            }

            if (villagersAssigned > 0)
            {
                GorkhaVoiceBarks.PlayHarvestOrderBark(SelectedUnits[0]);
                string verb = node.NodeType == ResourceNodeType.Tree ? "🪓 Chopping Timber" : "⛏️ Mining Slate";
                GorkhaPlayableHUD.SpawnFloatingText(
                    node.transform.position + Vector3.up * 2.2f,
                    $"{verb} ({villagersAssigned} Villagers)",
                    Color.yellow);
            }
            else
            {
                GorkhaPlayableHUD.SpawnFloatingText(
                    node.transform.position + Vector3.up * 2.2f,
                    "Only Villagers Can Harvest Resources!",
                    Color.red);
            }
        }

        private void ExecuteFarmOrder(BuildingInstance farm)
        {
            int villagersAssigned = 0;
            foreach (var u in SelectedUnits)
            {
                if (u != null && u.IsAlive && u.IsVillager)
                {
                    u.AssignFarmTask(farm);
                    villagersAssigned++;
                }
            }

            if (villagersAssigned > 0)
            {
                GorkhaVoiceBarks.PlayHarvestOrderBark(SelectedUnits[0]);
                GorkhaPlayableHUD.SpawnFloatingText(
                    farm.transform.position + Vector3.up * 2.2f,
                    $"🌾 Tending Terraced Farm ({villagersAssigned} Villagers)",
                    Color.yellow);
            }
            else
            {
                GorkhaPlayableHUD.SpawnFloatingText(
                    farm.transform.position + Vector3.up * 2.2f,
                    "Only Villagers Can Tend Farms!",
                    Color.red);
            }
        }

        private void ExecuteMarchOrder(Vector3 targetCenter)
        {
            if (SelectedUnits.Count == 0) return;

            // Calculate group centroid
            Vector3 centroid = Vector3.zero;
            foreach (var u in SelectedUnits)
            {
                centroid += u.transform.position;
            }
            centroid /= SelectedUnits.Count;

            Vector3 marchDir = targetCenter - centroid;
            marchDir.y = 0;
            if (marchDir.sqrMagnitude < 0.01f)
            {
                marchDir = Vector3.forward;
            }
            else
            {
                marchDir.Normalize();
            }

            // Calculate formation destination slots
            Vector3[] slots = ArmyFormationController.CalculateFormationSlots(
                CurrentFormation,
                SelectedUnits,
                targetCenter,
                marchDir,
                spacing: 1.6f);

            for (int i = 0; i < SelectedUnits.Count && i < slots.Length; i++)
            {
                if (SelectedUnits[i] != null && SelectedUnits[i].IsAlive)
                {
                    SelectedUnits[i].OrderMove(slots[i]);
                }
            }

            // Destination visual cue & vocal feedback
            GorkhaVFX.SpawnPlacementDust(targetCenter, Vector2.one * 0.75f);
            GorkhaVoiceBarks.PlayMoveOrderBark(SelectedUnits[0], targetCenter);
        }

        #endregion

        #region Marquee Selection GUI Rendering

        private void OnGUI()
        {
            if (!_isDragging) return;

            InitializeMarqueeTextures();

            Vector2 mousePos = GorkhaInput.MousePosition;

            // GUI coordinates have inverted Y relative to Screen coordinates
            float y1 = Screen.height - _dragStartScreen.y;
            float y2 = Screen.height - mousePos.y;

            float x = Mathf.Min(_dragStartScreen.x, mousePos.x);
            float y = Mathf.Min(y1, y2);
            float w = Mathf.Abs(_dragStartScreen.x - mousePos.x);
            float h = Mathf.Abs(y1 - y2);

            Rect boxRect = new Rect(x, y, w, h);

            // Draw translucent green box fill
            if (_boxBackgroundTex != null)
            {
                GUI.DrawTexture(boxRect, _boxBackgroundTex);
            }

            // Draw 1.5px green outline border
            if (_boxBorderTex != null)
            {
                float borderW = 1.5f;
                GUI.DrawTexture(new Rect(x, y, w, borderW), _boxBorderTex); // Top
                GUI.DrawTexture(new Rect(x, y + h - borderW, w, borderW), _boxBorderTex); // Bottom
                GUI.DrawTexture(new Rect(x, y, borderW, h), _boxBorderTex); // Left
                GUI.DrawTexture(new Rect(x + w - borderW, y, borderW, h), _boxBorderTex); // Right
            }
        }

        #endregion
    }
}
