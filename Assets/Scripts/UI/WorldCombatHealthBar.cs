using UnityEngine;
using Gorkha.Graphics;

namespace Gorkha.UI
{
    /// <summary>
    /// Sleek world-space combat health bar floating above units and buildings.
    /// Matches Clash of Clans & Age of Empires visual design:
    /// - Hidden when unit/building is at full health and peace
    /// - Automatically appears upon taking damage with green/yellow/red health gradient
    /// - Faces camera viewport and auto-fades after combat resolves
    /// </summary>
    public class WorldCombatHealthBar : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Vector3 _offset = new Vector3(0, 1.85f, 0);
        [SerializeField] private float _barWidth = 0.95f;
        [SerializeField] private float _barHeight = 0.10f;
        [SerializeField] private float _autoHideDelay = 4.0f;

        private GameObject _container;
        private Transform _fillTransform;
        private MeshRenderer _fillRenderer;
        private Material _fillMaterial;
        private float _lastDamageTime = -99f;
        private int _maxHealth = 100;
        private int _currentHealth = 100;
        private bool _isBuilding = false;

        public void Initialize(int maxHealth, Vector3 offset, bool isBuilding = false)
        {
            _maxHealth = Mathf.Max(1, maxHealth);
            _currentHealth = _maxHealth;
            _offset = offset;
            _isBuilding = isBuilding;

            if (_isBuilding)
            {
                _barWidth = 1.8f;
                _barHeight = 0.14f;
            }

            BuildBarVisuals();
            SetVisible(false);
        }

        private void BuildBarVisuals()
        {
            if (_container != null) Destroy(_container);

            _container = new GameObject("CombatHealthBar_Visual");
            _container.transform.SetParent(transform, false);
            _container.transform.localPosition = _offset;

            // 1. Dark Slate Border Plate
            GameObject border = new GameObject("BarBorder");
            border.transform.SetParent(_container.transform, false);
            var bFilter = border.AddComponent<MeshFilter>();
            bFilter.sharedMesh = Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(_barWidth + 0.08f, _barHeight + 0.04f, 0.02f));
            var bRen = border.AddComponent<MeshRenderer>();
            bRen.sharedMaterial = GorkhaMaterials.SlateStone;

            // 2. Health Fill Bar
            GameObject fill = new GameObject("BarFill");
            fill.transform.SetParent(_container.transform, false);
            fill.transform.localPosition = new Vector3(0, 0, -0.015f);

            var fFilter = fill.AddComponent<MeshFilter>();
            fFilter.sharedMesh = Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(_barWidth, _barHeight, 0.02f));

            _fillRenderer = fill.AddComponent<MeshRenderer>();
            _fillMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
            _fillMaterial.color = new Color(0.25f, 0.85f, 0.35f); // Green initially
            _fillRenderer.sharedMaterial = _fillMaterial;

            _fillTransform = fill.transform;
        }

        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            _currentHealth = Mathf.Max(0, currentHealth);
            _maxHealth = Mathf.Max(1, maxHealth);
            _lastDamageTime = Time.time;

            if (_container == null)
            {
                BuildBarVisuals();
            }

            float pct = Mathf.Clamp01((float)_currentHealth / _maxHealth);

            // Scale fill bar from left edge
            if (_fillTransform != null)
            {
                _fillTransform.localScale = new Vector3(pct, 1f, 1f);
                float xOffset = -(_barWidth * 0.5f) * (1f - pct);
                _fillTransform.localPosition = new Vector3(xOffset, 0, -0.015f);
            }

            // Color gradient: Green -> Yellow -> Red
            if (_fillMaterial != null)
            {
                Color col;
                if (pct > 0.55f)
                    col = Color.Lerp(Color.yellow, new Color(0.2f, 0.85f, 0.3f), (pct - 0.55f) / 0.45f);
                else
                    col = Color.Lerp(new Color(0.9f, 0.15f, 0.15f), Color.yellow, pct / 0.55f);

                _fillMaterial.color = col;
            }

            SetVisible(_currentHealth > 0 && pct < 0.999f);
        }

        private void LateUpdate()
        {
            if (_container == null || !_container.activeSelf) return;

            // Face Camera
            Camera cam = Camera.main;
            if (cam != null)
            {
                _container.transform.rotation = cam.transform.rotation;
            }

            // Auto-hide when combat resolves
            if (Time.time - _lastDamageTime > _autoHideDelay && _currentHealth >= _maxHealth * 0.99f)
            {
                SetVisible(false);
            }
        }

        public void SetVisible(bool visible)
        {
            if (_container != null)
            {
                _container.SetActive(visible);
            }
        }

        private void OnDestroy()
        {
            if (_fillMaterial != null)
            {
                Destroy(_fillMaterial);
            }
        }
    }
}
