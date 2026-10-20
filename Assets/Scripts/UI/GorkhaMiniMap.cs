using System;
using UnityEngine;
using Gorkha.Core;
using Gorkha.Buildings;
using Gorkha.Economy;
using Gorkha.Units;
using Gorkha.CameraSystem;
using Gorkha.Audio;

namespace Gorkha.UI
{
    /// <summary>
    /// Interactive Live Mini-Map & Radar System (Age of Empires & Clash of Clans style).
    /// Features:
    /// - 2D Isometric Diamond projection aligned 1:1 with camera orientation
    /// - Live real-time blips: Town Center (Gold), Storages/Camps (Amber), Barracks (Crimson),
    ///   Houses/Watchtowers (Slate), Harvestable Trees & Boulders (Green/Stone), and moving Units (Yellow/Cyan)
    /// - Dynamic Camera Frustum Viewport indicator tracking camera pan & zoom
    /// - Click-to-Pan: Clicking or dragging any location on the mini-map smoothly pans the RTS camera
    /// - Minimizable radar panel for clean viewport customization
    /// </summary>
    public class GorkhaMiniMap : MonoBehaviour
    {
        public static GorkhaMiniMap Instance { get; private set; }

        [Header("Mini-Map Dimensions")]
        [SerializeField] private float _mapSize = 220f;
        [SerializeField] private float _padding = 12f;
        [SerializeField] private bool _isExpanded = true;

        private Rect _mapRect;
        private Rect _diamondRect;
        private bool _isDraggingOnMap = false;

        // Custom Procedural Blip Textures
        private Texture2D _bgCardTex;
        private Texture2D _borderDiamondTex;
        private Texture2D _townCenterBlipTex;
        private Texture2D _buildingBlipTex;
        private Texture2D _storageBlipTex;
        private Texture2D _barracksBlipTex;
        private Texture2D _treeBlipTex;
        private Texture2D _boulderBlipTex;
        private Texture2D _villagerBlipTex;
        private Texture2D _militaryBlipTex;
        private Texture2D _hostileBlipTex;
        private Texture2D _wallBlipTex;
        private Texture2D _viewportFrameTex;

        public Rect MapRect => _mapRect;
        public bool IsExpanded => _isExpanded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreateBlipTextures();
        }

        private void CreateBlipTextures()
        {
            _bgCardTex = MakeSolidTex(16, 16, new Color(0.08f, 0.11f, 0.15f, 0.92f));
            _borderDiamondTex = MakeSolidTex(2, 2, new Color(0.85f, 0.72f, 0.45f, 0.65f));

            _townCenterBlipTex = MakeCircleTex(10, new Color(1.0f, 0.84f, 0.0f, 1.0f), new Color(0.8f, 0.15f, 0.15f, 1.0f));
            _buildingBlipTex = MakeSolidTex(6, 6, new Color(0.24f, 0.55f, 0.82f, 0.95f));
            _storageBlipTex = MakeSolidTex(6, 6, new Color(0.92f, 0.60f, 0.20f, 0.95f));
            _barracksBlipTex = MakeSolidTex(6, 6, new Color(0.82f, 0.25f, 0.20f, 0.95f));
            _wallBlipTex = MakeSolidTex(3, 3, new Color(0.70f, 0.65f, 0.60f, 0.95f));

            _treeBlipTex = MakeCircleTex(4, new Color(0.20f, 0.65f, 0.25f, 0.90f), Color.clear);
            _boulderBlipTex = MakeCircleTex(4, new Color(0.60f, 0.65f, 0.70f, 0.90f), Color.clear);

            _villagerBlipTex = MakeCircleTex(5, new Color(1.0f, 0.95f, 0.35f, 1.0f), new Color(0.3f, 0.25f, 0.0f, 0.8f));
            _militaryBlipTex = MakeCircleTex(5, new Color(0.15f, 0.90f, 1.0f, 1.0f), new Color(0.0f, 0.3f, 0.4f, 0.8f));
            _hostileBlipTex = MakeCircleTex(7, new Color(1.0f, 0.18f, 0.18f, 1.0f), new Color(1.0f, 0.85f, 0.1f, 1.0f));

            _viewportFrameTex = MakeSolidTex(2, 2, new Color(1.0f, 1.0f, 1.0f, 0.85f));
        }

        private Texture2D MakeSolidTex(int width, int height, Color col)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        private Texture2D MakeCircleTex(int size, Color fill, Color border)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = (size - 1) * 0.5f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= radius - 0.7f)
                    {
                        tex.SetPixel(x, y, fill);
                    }
                    else if (d <= radius)
                    {
                        tex.SetPixel(x, y, border != Color.clear ? border : fill);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            float scale = GorkhaPlayableHUD.GetEffectiveScale();
            Matrix4x4 origMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1.0f));

            float vWidth = Screen.width / scale;
            float vHeight = Screen.height / scale;

            float totalHeight = _isExpanded ? _mapSize + 28f : 30f;
            _mapRect = new Rect(vWidth - _mapSize - _padding, vHeight - totalHeight - _padding, _mapSize, totalHeight);

            // Background card
            GUI.DrawTexture(_mapRect, _bgCardTex);

            // Header bar
            Rect headerRect = new Rect(_mapRect.x, _mapRect.y, _mapRect.width, 24);
            GUI.Label(new Rect(_mapRect.x + 8, _mapRect.y + 3, 110, 20), "🗺️ <b>Radar View</b>");

            string toggleLabel = _isExpanded ? "▼" : "▲";
            if (GUI.Button(new Rect(_mapRect.x + _mapRect.width - 24, _mapRect.y + 3, 20, 18), toggleLabel))
            {
                _isExpanded = !_isExpanded;
                GorkhaAudio.PlayClick();
            }

            if (_isExpanded)
            {
                // Diamond Radar Area
                float diamondSide = _mapSize - 14f;
                _diamondRect = new Rect(_mapRect.x + 7f, _mapRect.y + 26f, diamondSide, diamondSide);

                HandleMiniMapInput();

                DrawDiamondGridBorder(_diamondRect);
                DrawResourceBlips(_diamondRect);
                DrawBuildingBlips(_diamondRect);
                DrawUnitBlips(_diamondRect);
                DrawCameraViewportFrustum(_diamondRect);
            }

            GUI.matrix = origMatrix;
        }

        private void HandleMiniMapInput()
        {
            Event e = Event.current;
            if (e == null) return;

            Vector2 mousePos = GUI.matrix.inverse.MultiplyPoint3x4(e.mousePosition);

            if (e.type == EventType.MouseDown && e.button == 0 && _diamondRect.Contains(mousePos))
            {
                _isDraggingOnMap = true;
                PanCameraToMiniMapPosition(mousePos);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _isDraggingOnMap)
            {
                PanCameraToMiniMapPosition(mousePos);
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                _isDraggingOnMap = false;
            }
        }

        private void PanCameraToMiniMapPosition(Vector2 mousePos)
        {
            float cx = _diamondRect.x + (_diamondRect.width * 0.5f);
            float cy = _diamondRect.y + (_diamondRect.height * 0.5f);
            float R = (_diamondRect.width * 0.5f) * 0.90f;

            float dx = (mousePos.x - cx) / R;
            float dy = (cy - mousePos.y) / R;

            float worldDim = GetWorldDimension();

            // Reverse isometric diamond mapping:
            // x = (dx + dy + 1) * worldDim / 2
            // z = (dy + 1 - dx) * worldDim / 2
            float worldX = (dx + dy + 1.0f) * 0.5f * worldDim;
            float worldZ = (dy + 1.0f - dx) * 0.5f * worldDim;

            worldX = Mathf.Clamp(worldX, 4f, worldDim - 4f);
            worldZ = Mathf.Clamp(worldZ, 4f, worldDim - 4f);

            if (IsometricCameraController.Instance != null)
            {
                IsometricCameraController.Instance.FocusOn(new Vector3(worldX, 0, worldZ));
            }
        }

        private float GetWorldDimension()
        {
            int gridW = IsometricGrid.Instance != null ? IsometricGrid.Instance.GridWidth : 24;
            float cellSize = IsometricGrid.Instance != null ? IsometricGrid.Instance.CellSize : 2.0f;
            return gridW * cellSize;
        }

        private Vector2 WorldToMiniMap(Vector3 worldPos, Rect rect)
        {
            float cx = rect.x + (rect.width * 0.5f);
            float cy = rect.y + (rect.height * 0.5f);
            float R = (rect.width * 0.5f) * 0.90f;

            float worldDim = GetWorldDimension();
            float normX = worldPos.x / Math.Max(1.0f, worldDim);
            float normZ = worldPos.z / Math.Max(1.0f, worldDim);

            // Isometric diamond projection (Yaw 45°)
            float u = normX - normZ;
            float v = (normX + normZ) - 1.0f;

            float screenX = cx + (u * R);
            float screenY = cy - (v * R);

            return new Vector2(screenX, screenY);
        }

        private void DrawDiamondGridBorder(Rect rect)
        {
            float cx = rect.x + (rect.width * 0.5f);
            float cy = rect.y + (rect.height * 0.5f);
            float R = (rect.width * 0.5f) * 0.90f;

            Vector2 top = new Vector2(cx, cy - R);
            Vector2 bottom = new Vector2(cx, cy + R);
            Vector2 right = new Vector2(cx + R, cy);
            Vector2 left = new Vector2(cx - R, cy);

            DrawLine(top, right, new Color(0.85f, 0.72f, 0.45f, 0.75f), 1.5f);
            DrawLine(right, bottom, new Color(0.85f, 0.72f, 0.45f, 0.75f), 1.5f);
            DrawLine(bottom, left, new Color(0.85f, 0.72f, 0.45f, 0.75f), 1.5f);
            DrawLine(left, top, new Color(0.85f, 0.72f, 0.45f, 0.75f), 1.5f);

            // Subtle Crosshairs
            DrawLine(new Vector2(cx - 6, cy), new Vector2(cx + 6, cy), new Color(1f, 1f, 1f, 0.25f), 1f);
            DrawLine(new Vector2(cx, cy - 6), new Vector2(cx, cy + 6), new Color(1f, 1f, 1f, 0.25f), 1f);
        }

        private void DrawResourceBlips(Rect rect)
        {
            foreach (var node in ResourceNode.AllNodes)
            {
                if (node == null || node.IsDepleted) continue;

                Vector2 pos = WorldToMiniMap(node.transform.position, rect);
                Texture2D tex = node.NodeType == ResourceNodeType.Tree ? _treeBlipTex : _boulderBlipTex;
                GUI.DrawTexture(new Rect(pos.x - 2, pos.y - 2, 4, 4), tex);
            }
        }

        private void DrawBuildingBlips(Rect rect)
        {
            foreach (var b in BuildingInstance.AllBuildings)
            {
                if (b == null || b.Data == null) continue;

                Vector2 pos = WorldToMiniMap(b.transform.position, rect);

                switch (b.Data.buildingType)
                {
                    case BuildingType.TownCenter:
                        GUI.DrawTexture(new Rect(pos.x - 5, pos.y - 5, 10, 10), _townCenterBlipTex);
                        break;
                    case BuildingType.Farm:
                    case BuildingType.WoodCamp:
                    case BuildingType.StoneQuarry:
                        GUI.DrawTexture(new Rect(pos.x - 3, pos.y - 3, 6, 6), _storageBlipTex);
                        break;
                    case BuildingType.Barracks:
                        GUI.DrawTexture(new Rect(pos.x - 3, pos.y - 3, 6, 6), _barracksBlipTex);
                        break;
                    case BuildingType.Wall:
                        GUI.DrawTexture(new Rect(pos.x - 1.5f, pos.y - 1.5f, 3, 3), _wallBlipTex);
                        break;
                    default:
                        GUI.DrawTexture(new Rect(pos.x - 3, pos.y - 3, 6, 6), _buildingBlipTex);
                        break;
                }
            }
        }

        private void DrawUnitBlips(Rect rect)
        {
            foreach (var u in UnitController.AllUnits)
            {
                if (u == null) continue;

                Vector2 pos = WorldToMiniMap(u.transform.position, rect);
                if (u.IsHostile)
                {
                    GUI.DrawTexture(new Rect(pos.x - 3.5f, pos.y - 3.5f, 7, 7), _hostileBlipTex);
                }
                else
                {
                    Texture2D tex = u.IsVillager ? _villagerBlipTex : _militaryBlipTex;
                    GUI.DrawTexture(new Rect(pos.x - 2.5f, pos.y - 2.5f, 5, 5), tex);
                }
            }
        }

        private void DrawCameraViewportFrustum(Rect rect)
        {
            if (IsometricCameraController.Instance == null) return;

            Vector3 camFocal = IsometricCameraController.Instance.FocalPoint;
            Vector2 camCenter = WorldToMiniMap(camFocal, rect);

            float zoom = IsometricCameraController.Instance.Zoom;
            float boxSize = Mathf.Clamp(zoom * 0.95f, 16f, 44f);

            // Draw white viewport diamond/box
            Rect viewRect = new Rect(camCenter.x - boxSize * 0.5f, camCenter.y - boxSize * 0.5f, boxSize, boxSize);
            DrawRectOutline(viewRect, new Color(1.0f, 1.0f, 1.0f, 0.85f), 1.5f);
        }

        private void DrawRectOutline(Rect r, Color color, float thickness)
        {
            DrawLine(new Vector2(r.x, r.y), new Vector2(r.x + r.width, r.y), color, thickness);
            DrawLine(new Vector2(r.x + r.width, r.y), new Vector2(r.x + r.width, r.y + r.height), color, thickness);
            DrawLine(new Vector2(r.x + r.width, r.y + r.height), new Vector2(r.x, r.y + r.height), color, thickness);
            DrawLine(new Vector2(r.x, r.y + r.height), new Vector2(r.x, r.y), color, thickness);
        }

        private void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            GUI.color = color;
            float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * Mathf.Rad2Deg;
            float length = Vector2.Distance(pointA, pointB);

            Matrix4x4 matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, pointA);
            GUI.DrawTexture(new Rect(pointA.x, pointA.y - width * 0.5f, length, width), _viewportFrameTex);
            GUI.matrix = matrix;
            GUI.color = Color.white;
        }
    }
}
