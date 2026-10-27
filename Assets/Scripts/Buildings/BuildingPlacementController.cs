using UnityEngine;
using Gorkha.Core;
using Gorkha.Economy;
using Gorkha.Graphics;
using Gorkha.Audio;
using Gorkha.UI;
using Gorkha.Progression;

namespace Gorkha.Buildings
{
    /// <summary>
    /// Handles 3D drag-and-drop building placement (Clash of Clans style).
    /// Tracks mouse position across the 3D terrain, snaps to the grid,
    /// renders live green/red footprint overlays, and commits placements.
    /// </summary>
    public class BuildingPlacementController : MonoBehaviour
    {
        public static BuildingPlacementController Instance { get; private set; }

#pragma warning disable 0649
        [Header("Preview Setup")]
        [SerializeField] private Material _validPlacementMaterial;
        [SerializeField] private Material _blockedPlacementMaterial;
#pragma warning restore 0649

        private BuildingData _currentBlueprint;
        private GameObject _previewInstance;
        private Vector2Int _hoveredGridPos;
        private bool _isPlacing = false;
        private bool _isValidPlacement = false;

        public bool IsPlacing => _isPlacing;
        public BuildingData CurrentBlueprint => _currentBlueprint;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private GameObject _gridOverlay;
        private bool _isRelocating = false;
        private BuildingInstance _relocatingBuilding = null;
        private Vector2Int _originalGridPos;

        public bool IsRelocating => _isRelocating;

        public void StartRelocation(BuildingInstance building)
        {
            if (building == null || building.Data == null) return;

            CancelPlacement();

            _isPlacing = true;
            _isRelocating = true;
            _relocatingBuilding = building;
            _currentBlueprint = building.Data;
            _originalGridPos = new Vector2Int(building.GridCol, building.GridRow);

            // Free current grid occupancy so building can be relocated
            if (IsometricGrid.Instance != null)
            {
                IsometricGrid.Instance.FreeArea(building.GridCol, building.GridRow, building.Data.tileWidth, building.Data.tileHeight);
            }

            // Create 3D Ground Footprint Grid Projector
            float cellSize = IsometricGrid.Instance != null ? IsometricGrid.Instance.CellSize : 2.0f;
            float gw = building.Data.tileWidth * cellSize;
            float gh = building.Data.tileHeight * cellSize;

            _gridOverlay = new GameObject("PlacementGridOverlay_3D");
            _gridOverlay.transform.SetParent(building.transform, false);
            _gridOverlay.transform.localPosition = new Vector3(0, 0.04f, 0);

            Nepali3DMeshBuilder.CreateSubMesh(
                "GridFootprintPlane",
                Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(gw * 0.96f, 0.02f, gh * 0.96f)),
                GorkhaMaterials.PlacementValid,
                _gridOverlay.transform
            );

            _hoveredGridPos = _originalGridPos;
            building.TriggerBounce(1.12f);
            UpdatePlacementPosition();
        }

        public void StartPlacement(BuildingData blueprint)
        {
            if (blueprint == null) return;

            CancelPlacement();

            _currentBlueprint = blueprint;
            _isPlacing = true;
            _isRelocating = false;
            _relocatingBuilding = null;

            // Spawn preview ghost instance
            if (FantasyKingdomAssetRegistry.HasPrefab(blueprint.buildingType))
            {
                _previewInstance = new GameObject($"Preview_{blueprint.buildingType}");
                FantasyKingdomAssetRegistry.InstantiateBuildingVisual(blueprint.buildingType, _previewInstance.transform, isPreview: true);
                DisableColliders(_previewInstance);
            }
            else if (blueprint.prefab != null)
            {
                _previewInstance = Instantiate(blueprint.prefab);
                DisableColliders(_previewInstance);
            }
            else
            {
                _previewInstance = new GameObject($"Preview_{blueprint.buildingType}");
                int tcTier = AgeManager.Instance != null ? AgeManager.Instance.CurrentAgeIndex + 1 : 1;
                switch (blueprint.buildingType)
                {
                    case BuildingType.TownCenter:
                        Nepali3DMeshBuilder.BuildTownCenter(_previewInstance.transform, tcTier);
                        break;
                    case BuildingType.Farm:
                        Nepali3DMeshBuilder.BuildFarm(_previewInstance.transform);
                        break;
                    case BuildingType.Barracks:
                        Nepali3DMeshBuilder.BuildBarracks(_previewInstance.transform);
                        break;
                    case BuildingType.WoodCamp:
                        Nepali3DMeshBuilder.BuildWoodCamp(_previewInstance.transform);
                        break;
                    case BuildingType.StoneQuarry:
                        Nepali3DMeshBuilder.BuildQuarry(_previewInstance.transform);
                        break;
                    case BuildingType.House:
                        Nepali3DMeshBuilder.BuildHouse(_previewInstance.transform);
                        break;
                    case BuildingType.Watchtower:
                        Nepali3DMeshBuilder.BuildWatchtower(_previewInstance.transform);
                        break;
                    case BuildingType.Granary:
                        Nepali3DMeshBuilder.BuildGranary(_previewInstance.transform);
                        break;
                    case BuildingType.TimberYard:
                        Nepali3DMeshBuilder.BuildTimberYard(_previewInstance.transform);
                        break;
                    case BuildingType.SlateDepot:
                        Nepali3DMeshBuilder.BuildSlateDepot(_previewInstance.transform);
                        break;
                    case BuildingType.Vault:
                        Nepali3DMeshBuilder.BuildVault(_previewInstance.transform);
                        break;
                    case BuildingType.Kaman:
                        Nepali3DMeshBuilder.BuildKamanCannon(_previewInstance.transform, 1);
                        break;
                    case BuildingType.Wall:
                        Nepali3DMeshBuilder.BuildWallSegment(_previewInstance.transform, 1, false, false, false, false);
                        break;
                    case BuildingType.Trap:
                        Nepali3DMeshBuilder.BuildSpikeTrap(_previewInstance.transform);
                        break;
                    default:
                        Nepali3DMeshBuilder.BuildTownCenter(_previewInstance.transform);
                        break;
                }
                DisableColliders(_previewInstance);
            }

            // Create 3D Ground Footprint Grid Projector
            float cellSize = IsometricGrid.Instance != null ? IsometricGrid.Instance.CellSize : 2.0f;
            float gw = blueprint.tileWidth * cellSize;
            float gh = blueprint.tileHeight * cellSize;

            _gridOverlay = new GameObject("PlacementGridOverlay_3D");
            _gridOverlay.transform.SetParent(_previewInstance.transform, false);
            _gridOverlay.transform.localPosition = new Vector3(0, 0.04f, 0);

            // Ground tile decal
            Nepali3DMeshBuilder.CreateSubMesh(
                "GridFootprintPlane",
                Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(gw * 0.96f, 0.02f, gh * 0.96f)),
                GorkhaMaterials.PlacementValid,
                _gridOverlay.transform
            );

            // Default to center of grid
            _hoveredGridPos = new Vector2Int(IsometricGrid.Instance.GridWidth / 2, IsometricGrid.Instance.GridHeight / 2);
            UpdatePlacementPosition();
        }

        private void Update()
        {
            if (!_isPlacing) return;

            // Track mouse raycast onto 3D ground plane
            Ray ray = Camera.main.ScreenPointToRay(GorkhaInput.MousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 worldHit = ray.GetPoint(enter);
                Vector2Int newGridPos = IsometricGrid.Instance.WorldToGridCoordinate(worldHit);

                if (newGridPos != _hoveredGridPos)
                {
                    _hoveredGridPos = newGridPos;
                    UpdatePlacementPosition();
                    GorkhaAudio.PlayGridSnap();
                }
            }

            // Quick placement on left click
            if (GorkhaInput.GetMouseButtonDown(0))
            {
                ConfirmPlacement();
            }

            // Cancel placement on right click or Escape
            if (GorkhaInput.GetMouseButtonDown(1) || GorkhaInput.GetCancelDown())
            {
                CancelPlacement();
            }
        }

        private void UpdatePlacementPosition()
        {
            if (IsometricGrid.Instance == null || _currentBlueprint == null) return;

            _isValidPlacement = IsometricGrid.Instance.CanPlaceAt(
                _hoveredGridPos.x,
                _hoveredGridPos.y,
                _currentBlueprint.tileWidth,
                _currentBlueprint.tileHeight
            );

            Vector3 worldPos = IsometricGrid.Instance.GridToWorldPosition(
                _hoveredGridPos.x,
                _hoveredGridPos.y,
                _currentBlueprint.tileWidth,
                _currentBlueprint.tileHeight
            );

            Material mat = _isValidPlacement
                ? (_validPlacementMaterial != null ? _validPlacementMaterial : GorkhaMaterials.PlacementValid)
                : (_blockedPlacementMaterial != null ? _blockedPlacementMaterial : GorkhaMaterials.PlacementBlocked);

            if (_isRelocating && _relocatingBuilding != null)
            {
                _relocatingBuilding.transform.position = worldPos + new Vector3(0, 0.25f, 0); // Slight float during drag

                if (_gridOverlay != null)
                {
                    var ren = _gridOverlay.GetComponentInChildren<MeshRenderer>();
                    if (ren != null) ren.sharedMaterial = mat;
                }
            }
            else if (_previewInstance != null)
            {
                _previewInstance.transform.position = worldPos;
                ApplyPreviewMaterial(_previewInstance, mat);

                if (_gridOverlay != null)
                {
                    var ren = _gridOverlay.GetComponentInChildren<MeshRenderer>();
                    if (ren != null) ren.sharedMaterial = mat;
                }
            }
        }

        public bool ConfirmPlacement()
        {
            if (!_isPlacing || _currentBlueprint == null) return false;

            // Handle Building Relocation
            if (_isRelocating && _relocatingBuilding != null)
            {
                if (!_isValidPlacement)
                {
                    GorkhaAudio.PlayError();
                    GorkhaPlayableHUD.SpawnFloatingText(_relocatingBuilding.transform.position + Vector3.up * 2f, "Position Blocked!", Color.red);
                    return false;
                }

                Vector3 newPos = IsometricGrid.Instance.GridToWorldPosition(
                    _hoveredGridPos.x,
                    _hoveredGridPos.y,
                    _currentBlueprint.tileWidth,
                    _currentBlueprint.tileHeight
                );

                _relocatingBuilding.RelocateTo(_hoveredGridPos.x, _hoveredGridPos.y, newPos);
                IsometricGrid.Instance.OccupyArea(
                    _relocatingBuilding.BuildingId,
                    _hoveredGridPos.x,
                    _hoveredGridPos.y,
                    _currentBlueprint.tileWidth,
                    _currentBlueprint.tileHeight
                );

                GorkhaAudio.PlayBuild();
                float rw = _currentBlueprint.tileWidth * 2.0f;
                float rh = _currentBlueprint.tileHeight * 2.0f;
                GorkhaVFX.SpawnPlacementDust(newPos, new Vector2(rw, rh));
                GorkhaPlayableHUD.SpawnFloatingText(newPos + Vector3.up * 2.5f, $"{_currentBlueprint.displayName} Relocated!", Color.green);

                if (_gridOverlay != null) Destroy(_gridOverlay);
                _isRelocating = false;
                _isPlacing = false;
                _relocatingBuilding = null;
                _currentBlueprint = null;
                return true;
            }

            if (!_isValidPlacement)
            {
                GorkhaAudio.PlayError();
                GorkhaPlayableHUD.SpawnFloatingText(_previewInstance.transform.position + Vector3.up * 2f, "Position Blocked!", Color.red);
                return false;
            }

            // Check Historical Age and Capacity Gating (AGE-01 / AGE-02)
            if (!BuildingGatingManager.CanConstruct(_currentBlueprint.buildingType, out string lockReason))
            {
                GorkhaAudio.PlayError();
                GorkhaPlayableHUD.SpawnFloatingText(_previewInstance.transform.position + Vector3.up * 2f, lockReason, Color.red);
                return false;
            }

            // Check Builder Availability
            if (BuilderManager.Instance != null && !BuilderManager.Instance.CanAssignBuilder())
            {
                GorkhaAudio.PlayError();
                GorkhaPlayableHUD.SpawnFloatingText(_previewInstance.transform.position + Vector3.up * 2f, "All Builders Busy!", Color.red);
                return false;
            }

            var cost = _currentBlueprint.GetCostDictionary(0);
            if (!ResourceManager.Instance.SpendResources(cost))
            {
                GorkhaAudio.PlayError();
                GorkhaPlayableHUD.SpawnFloatingText(_previewInstance.transform.position + Vector3.up * 2f, "Not Enough Resources!", Color.red);
                return false;
            }

            // Spawn actual building instance
            Vector3 worldPos = IsometricGrid.Instance.GridToWorldPosition(
                _hoveredGridPos.x,
                _hoveredGridPos.y,
                _currentBlueprint.tileWidth,
                _currentBlueprint.tileHeight
            );

            GameObject buildingObj = new GameObject(_currentBlueprint.displayName);
            buildingObj.transform.position = worldPos;
            BuildingInstance instance = buildingObj.AddComponent<BuildingInstance>();
            instance.Initialize(_currentBlueprint, _hoveredGridPos.x, _hoveredGridPos.y, startAsConstructing: true);

            if (HimalayanAtmosphericVFX.Instance != null)
            {
                HimalayanAtmosphericVFX.Instance.AttachChimneySmokeToBuilding(buildingObj, _currentBlueprint.buildingType);
            }

            IsometricGrid.Instance.OccupyArea(
                instance != null ? instance.BuildingId : "Building",
                _hoveredGridPos.x,
                _hoveredGridPos.y,
                _currentBlueprint.tileWidth,
                _currentBlueprint.tileHeight
            );

            GorkhaAudio.PlayBuild();
            float bw = _currentBlueprint.tileWidth * 2.0f;
            float bh = _currentBlueprint.tileHeight * 2.0f;
            GorkhaVFX.SpawnPlacementDust(worldPos, new Vector2(bw, bh));
            if (instance != null) instance.TriggerBounce(1.15f);

            GorkhaPlayableHUD.SpawnFloatingText(worldPos + Vector3.up * 2.5f, $"{_currentBlueprint.displayName} Construction Started!", Color.green);

            CancelPlacement();
            return true;
        }

        public void CancelPlacement()
        {
            if (_isRelocating && _relocatingBuilding != null)
            {
                Vector3 origPos = IsometricGrid.Instance.GridToWorldPosition(
                    _originalGridPos.x,
                    _originalGridPos.y,
                    _currentBlueprint.tileWidth,
                    _currentBlueprint.tileHeight
                );

                _relocatingBuilding.RelocateTo(_originalGridPos.x, _originalGridPos.y, origPos);
                IsometricGrid.Instance.OccupyArea(
                    _relocatingBuilding.BuildingId,
                    _originalGridPos.x,
                    _originalGridPos.y,
                    _currentBlueprint.tileWidth,
                    _currentBlueprint.tileHeight
                );

                if (_gridOverlay != null) Destroy(_gridOverlay);
                _isRelocating = false;
                _relocatingBuilding = null;
            }

            _isPlacing = false;
            _currentBlueprint = null;
            if (_previewInstance != null)
            {
                Destroy(_previewInstance);
                _previewInstance = null;
            }
        }

        private void DisableColliders(GameObject obj)
        {
            foreach (var col in obj.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
        }

        private void ApplyPreviewMaterial(GameObject obj, Material mat)
        {
            if (mat == null) return;
            foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
            {
                if (renderer.gameObject.name == "GridFootprintPlane") continue;
                renderer.material = mat;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_isPlacing || _currentBlueprint == null || IsometricGrid.Instance == null) return;

            Gizmos.color = _isValidPlacement ? new Color(0, 1, 0, 0.45f) : new Color(1, 0, 0, 0.45f);

            for (int dx = 0; dx < _currentBlueprint.tileWidth; dx++)
            {
                for (int dy = 0; dy < _currentBlueprint.tileHeight; dy++)
                {
                    Vector3 center = IsometricGrid.Instance.GridToWorldPosition(_hoveredGridPos.x + dx, _hoveredGridPos.y + dy);
                    Gizmos.DrawCube(center + Vector3.up * 0.05f, new Vector3(IsometricGrid.Instance.CellSize * 0.95f, 0.05f, IsometricGrid.Instance.CellSize * 0.95f));
                }
            }
        }
    }
}
