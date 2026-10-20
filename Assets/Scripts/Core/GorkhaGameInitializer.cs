using UnityEngine;
using Gorkha.Buildings;
using Gorkha.Economy;
using Gorkha.Graphics;
using Gorkha.CameraSystem;
using Gorkha.Units;
using Gorkha.Audio;
using Gorkha.UI;

namespace Gorkha.Core
{
    /// <summary>
    /// Master Scene Initializer for the 3D Unity Client.
    /// Operates in BOTH Edit Mode and Play Mode via [ExecuteAlways].
    /// Guarantees that the 3D Himalayan terrain, golden sun lighting, kingdom buildings,
    /// camera framing, audio synthesizer, and playable HUD are instantly constructed and visible.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    public class GorkhaGameInitializer : MonoBehaviour
    {
        [Header("Auto-Initialization")]
        [SerializeField] private bool _spawnInitialSettlement = true;
        [SerializeField] private bool _spawnAmbientPatrols = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapOnPlay()
        {
            if (FindAnyObjectByType<GorkhaGameInitializer>() == null)
            {
                GameObject bootstrapper = new GameObject("[Gorkha_KingdomManager]");
                bootstrapper.AddComponent<GorkhaGameInitializer>();
            }
        }

        private void Awake()
        {
            InitializeAllSystems();
        }

        private void OnEnable()
        {
            InitializeAllSystems();
        }

        private void Start()
        {
            InitializeAllSystems();

            if (Application.isPlaying && _spawnAmbientPatrols)
            {
                SpawnInitialUnits();
            }
        }

        public void InitializeAllSystems()
        {
            EnsureCoreManagers();

            if (_spawnInitialSettlement)
            {
                SpawnInitialKingdomBuildings();
                SpawnInitialResourceNodes();

                if (HimalayanAtmosphericVFX.Instance != null)
                {
                    HimalayanAtmosphericVFX.Instance.AttachChimneySmokes();
                }
            }
        }

        private void EnsureCoreManagers()
        {
            // 1. Isometric Grid
            if (IsometricGrid.Instance == null && FindAnyObjectByType<IsometricGrid>() == null)
            {
                GameObject gridObj = new GameObject("IsometricGrid_Manager");
                gridObj.AddComponent<IsometricGrid>();
            }

            // 2. Resource Manager
            if (ResourceManager.Instance == null && FindAnyObjectByType<ResourceManager>() == null)
            {
                GameObject econObj = new GameObject("ResourceManager_Manager");
                econObj.AddComponent<ResourceManager>();
            }

            // 3. Audio Synthesizer, Voice Barks & Himalayan Soundscape
            if (GorkhaAudio.Instance == null && FindAnyObjectByType<GorkhaAudio>() == null)
            {
                GameObject audioObj = new GameObject("GorkhaAudio_Manager");
                audioObj.AddComponent<GorkhaAudio>();
            }

            if (GorkhaVoiceBarks.Instance == null && FindAnyObjectByType<GorkhaVoiceBarks>() == null)
            {
                GameObject barksObj = new GameObject("GorkhaVoiceBarks_Manager");
                barksObj.AddComponent<GorkhaVoiceBarks>();
            }

            if (HimalayanSoundscape.Instance == null && FindAnyObjectByType<HimalayanSoundscape>() == null)
            {
                GameObject soundscapeObj = new GameObject("HimalayanSoundscape_Manager");
                soundscapeObj.AddComponent<HimalayanSoundscape>();
            }

            // 4. Placement Controller
            if (BuildingPlacementController.Instance == null && FindAnyObjectByType<BuildingPlacementController>() == null)
            {
                GameObject placeObj = new GameObject("PlacementController_Manager");
                placeObj.AddComponent<BuildingPlacementController>();
            }

            // 5. 3D Himalayan Environment & Lighting
            HimalayanEnvironment3D env = FindAnyObjectByType<HimalayanEnvironment3D>();
            if (env == null)
            {
                GameObject envObj = new GameObject("HimalayanEnvironment_3D");
                env = envObj.AddComponent<HimalayanEnvironment3D>();
            }
            env.EnsureEnvironment();

            // 6. Playable HUD Controller
            if (GorkhaPlayableHUD.Instance == null && FindAnyObjectByType<GorkhaPlayableHUD>() == null)
            {
                GameObject hudObj = new GameObject("GorkhaPlayableHUD_Manager");
                hudObj.AddComponent<GorkhaPlayableHUD>();
            }

            // 7. Builder Manager (Dakshya Karmi)
            if (BuilderManager.Instance == null && FindAnyObjectByType<BuilderManager>() == null)
            {
                GameObject bObj = new GameObject("BuilderManager_Manager");
                bObj.AddComponent<BuilderManager>();
            }

            // 8. Interactive Radar Mini-Map
            if (GorkhaMiniMap.Instance == null && FindAnyObjectByType<GorkhaMiniMap>() == null)
            {
                GameObject mapObj = new GameObject("GorkhaMiniMap_Manager");
                mapObj.AddComponent<GorkhaMiniMap>();
            }

            // 9. RTS Multi-Unit Selection & Tactical Formation Manager
            if (RTSSelectionManager.Instance == null && FindAnyObjectByType<RTSSelectionManager>() == null)
            {
                GameObject rtsObj = new GameObject("RTSSelectionManager_Manager");
                rtsObj.AddComponent<RTSSelectionManager>();
            }

            // 9. Isometric Camera Setup & Framing
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("MainCamera");
                cam = camObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.68f, 0.79f, 0.90f); // Himalayan Sky Blue

            if (cam.GetComponent<AudioListener>() == null && FindAnyObjectByType<AudioListener>() == null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }
            AudioListener.pause = false;
            AudioListener.volume = 1.0f;

            var camController = cam.GetComponent<IsometricCameraController>();
            if (camController == null)
            {
                camController = cam.gameObject.AddComponent<IsometricCameraController>();
            }
            camController.ResetView();
        }

        private void SpawnInitialKingdomBuildings()
        {
            // If buildings already exist in the scene, don't duplicate
            if (FindAnyObjectByType<BuildingInstance>() != null) return;
            if (IsometricGrid.Instance == null) return;

            // Center is grid (11, 11) -> World (24, 0, 24)
            // 1. Town Center (Gorkha Durbar) at (11, 11)
            SpawnBuildingDirect(BuildingType.TownCenter, "Gorkha Durbar (Town Center)", 11, 11, 2, 2, hasProduction: true, prodType: ResourceType.Gold, rate: 1.0);

            // 2. Terraced Farm at (7, 11)
            SpawnBuildingDirect(BuildingType.Farm, "Terraced Farm", 7, 11, 2, 2, hasProduction: true, prodType: ResourceType.Food, rate: 2.5);

            // 3. Woodcutter Camp at (15, 11)
            SpawnBuildingDirect(BuildingType.WoodCamp, "Woodcutter Camp", 15, 11, 2, 2, hasProduction: true, prodType: ResourceType.Wood, rate: 2.0);

            // 4. Barracks (Kot) at (11, 15)
            SpawnBuildingDirect(BuildingType.Barracks, "Kot (Barracks)", 11, 15, 2, 2, hasProduction: false, prodType: ResourceType.Food, rate: 0.0);

            // 5. Newari House at (8, 7)
            SpawnBuildingDirect(BuildingType.House, "Newari House", 8, 7, 2, 2, hasProduction: false, prodType: ResourceType.Food, rate: 0.0);

            // 6. Watchtower at (15, 7)
            SpawnBuildingDirect(BuildingType.Watchtower, "Watchtower", 15, 7, 2, 2, hasProduction: false, prodType: ResourceType.Food, rate: 0.0);
        }

        private void SpawnBuildingDirect(BuildingType type, string displayName, int col, int row, int width, int height, bool hasProduction, ResourceType prodType, double rate)
        {
            if (IsometricGrid.Instance == null) return;
            if (!IsometricGrid.Instance.CanPlaceAt(col, row, width, height)) return;

            BuildingData data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingType = type;
            data.displayName = displayName;
            data.tileWidth = width;
            data.tileHeight = height;
            data.hasProduction = hasProduction;
            data.productionType = prodType;
            data.baseProductionRate = rate;
            data.constructionDurationSeconds = 5.0f;
            data.prefab = FantasyKingdomAssetRegistry.GetPrefab(type);

            Vector3 worldPos = IsometricGrid.Instance.GridToWorldPosition(col, row, width, height);
            GameObject obj = new GameObject(displayName);
            obj.transform.position = worldPos;

            BuildingInstance instance = obj.AddComponent<BuildingInstance>();
            instance.Initialize(data, col, row, startAsConstructing: false);

            IsometricGrid.Instance.OccupyArea(instance.BuildingId, col, row, width, height);
        }

        private void SpawnInitialResourceNodes()
        {
            if (IsometricGrid.Instance == null) return;
            if (FindAnyObjectByType<ResourceNode>() != null) return;

            // Harvestable Pine Trees (AoE Wood gathering & CoC Obstacle clearing)
            (int x, int y)[] trees = {
                (4, 4), (5, 4), (4, 5),
                (19, 4), (19, 5), (18, 4),
                (4, 18), (5, 18), (4, 19),
                (19, 18), (18, 19), (19, 19),
                (3, 11), (3, 12),
                (20, 11), (20, 12)
            };

            foreach (var (x, y) in trees)
            {
                if (!IsometricGrid.Instance.CanPlaceAt(x, y, 1, 1)) continue;

                Vector3 pos = IsometricGrid.Instance.GridToWorldPosition(x, y);
                GameObject treeObj = new GameObject($"HarvestableTree_{x}_{y}");
                treeObj.transform.position = pos;
                FantasyKingdomAssetRegistry.InstantiatePineTree(treeObj.transform, x + y);

                var node = treeObj.AddComponent<ResourceNode>();
                node.Initialize(ResourceNodeType.Tree, x, y, 150.0);
                IsometricGrid.Instance.OccupyArea(treeObj.name, x, y, 1, 1);
            }

            // Harvestable Slate Boulders (AoE Stone mining)
            (int x, int y)[] boulders = {
                (4, 8), (4, 9),
                (19, 8), (19, 9),
                (7, 4), (8, 4),
                (13, 19), (14, 19)
            };

            foreach (var (x, y) in boulders)
            {
                if (!IsometricGrid.Instance.CanPlaceAt(x, y, 1, 1)) continue;

                Vector3 pos = IsometricGrid.Instance.GridToWorldPosition(x, y);
                GameObject boulderObj = new GameObject($"HarvestableBoulder_{x}_{y}");
                boulderObj.transform.position = pos;
                FantasyKingdomAssetRegistry.InstantiateRock(boulderObj.transform, x + y, 1.25f);

                var node = boulderObj.AddComponent<ResourceNode>();
                node.Initialize(ResourceNodeType.Boulder, x, y, 200.0);
                IsometricGrid.Instance.OccupyArea(boulderObj.name, x, y, 1, 1);
            }
        }

        private void SpawnInitialUnits()
        {
            if (IsometricGrid.Instance == null) return;
            if (FindAnyObjectByType<UnitController>() != null) return;

            (UnitType type, int x, int y)[] units = {
                (UnitType.KhukuriWarrior, 10, 10),
                (UnitType.Spearman, 13, 10),
                (UnitType.Archer, 15, 14),
                (UnitType.KhukuriWarrior, 11, 13),
                (UnitType.Villager, 8, 11),
                (UnitType.Villager, 14, 12)
            };

            foreach (var u in units)
            {
                Vector3 pos = IsometricGrid.Instance.GridToWorldPosition(u.x, u.y);
                GameObject unitObj = new GameObject($"Unit_{u.type}");
                unitObj.transform.position = pos;
                var controller = unitObj.AddComponent<UnitController>();
                controller.SetUnitType(u.type);
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    public static class GorkhaEditorAutoSetup
    {
        static GorkhaEditorAutoSetup()
        {
            UnityEditor.EditorApplication.delayCall += EnsureActiveSceneConfigured;
        }

        private static void EnsureActiveSceneConfigured()
        {
            if (Application.isPlaying) return;
            var mgr = Object.FindAnyObjectByType<GorkhaGameInitializer>();
            if (mgr == null)
            {
                GameObject bootstrapper = new GameObject("[Gorkha_KingdomManager]");
                mgr = bootstrapper.AddComponent<GorkhaGameInitializer>();
            }
            mgr.InitializeAllSystems();
        }
    }
#endif
}
