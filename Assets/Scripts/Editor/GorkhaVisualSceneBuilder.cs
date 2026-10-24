#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Gorkha.Core;
using Gorkha.Buildings;
using Gorkha.Economy;
using Gorkha.Graphics;
using Gorkha.CameraSystem;
using Gorkha.Units;

namespace Gorkha.Editor
{
    /// <summary>
    /// Editor automation tool for Gorkha: Rise of the Kingdom.
    /// Provides one-click scene creation, 3D prefab generation, and atmospheric setup.
    /// </summary>
    public static class GorkhaVisualSceneBuilder
    {
        [MenuItem("Gorkha/Play Settlement Scene %p", false, 0)]
        public static void StartPlayMode()
        {
            if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainSettlementScene.unity")
            {
                EditorSceneManager.OpenScene("Assets/Scenes/MainSettlementScene.unity");
            }
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Gorkha/Generate 3D Settlement Scene & Prefabs", false, 1)]
        public static void GenerateComplete3DScene()
        {
            // 1. Ensure project directories exist
            EnsureAssetFolders();

            // 2. Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Build Core Hierarchy
            GameObject root = new GameObject("[Gorkha_Systems]");

            GameObject gridObj = new GameObject("IsometricGrid");
            gridObj.transform.SetParent(root.transform);
            gridObj.AddComponent<IsometricGrid>();

            GameObject econObj = new GameObject("ResourceManager");
            econObj.transform.SetParent(root.transform);
            econObj.AddComponent<ResourceManager>();

            GameObject placeObj = new GameObject("BuildingPlacementController");
            placeObj.transform.SetParent(root.transform);
            placeObj.AddComponent<BuildingPlacementController>();

            GameObject envObj = new GameObject("HimalayanEnvironment_3D");
            envObj.transform.SetParent(root.transform);
            envObj.AddComponent<HimalayanEnvironment3D>();

            GameObject initObj = new GameObject("GorkhaGameInitializer");
            initObj.transform.SetParent(root.transform);
            initObj.AddComponent<GorkhaGameInitializer>();

            // 4. Build Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.68f, 0.79f, 0.90f);
            cam.transform.position = new Vector3(24f, 26f, 10f);
            cam.transform.rotation = Quaternion.Euler(45.0f, 45.0f, 0.0f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<IsometricCameraController>();

            // 5. Generate and Save 3D Prefabs
            GenerateAndSavePrefabs();

            // 6. Save Scene
            string scenePath = "Assets/Scenes/MainSettlementScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"<color=#00FF7F><b>[Gorkha 3D]</b> Complete 3D Settlement Scene successfully built and saved to {scenePath}!</color>");
        }

        private static void EnsureAssetFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
            AssetDatabase.Refresh();
        }

        private static void GenerateAndSavePrefabs()
        {
            // Town Center
            GameObject tc = Nepali3DMeshBuilder.BuildTownCenter();
            tc.AddComponent<BuildingInstance>();
            PrefabUtility.SaveAsPrefabAsset(tc, "Assets/Prefabs/TownCenter_3D.prefab");
            Object.DestroyImmediate(tc);

            // Farm
            GameObject farm = Nepali3DMeshBuilder.BuildFarm();
            farm.AddComponent<BuildingInstance>();
            PrefabUtility.SaveAsPrefabAsset(farm, "Assets/Prefabs/Farm_3D.prefab");
            Object.DestroyImmediate(farm);

            // Barracks
            GameObject barracks = Nepali3DMeshBuilder.BuildBarracks();
            barracks.AddComponent<BuildingInstance>();
            PrefabUtility.SaveAsPrefabAsset(barracks, "Assets/Prefabs/Barracks_3D.prefab");
            Object.DestroyImmediate(barracks);

            // Wood Camp
            GameObject wood = Nepali3DMeshBuilder.BuildWoodCamp();
            wood.AddComponent<BuildingInstance>();
            PrefabUtility.SaveAsPrefabAsset(wood, "Assets/Prefabs/WoodCamp_3D.prefab");
            Object.DestroyImmediate(wood);

            // Stone Quarry
            GameObject quarry = Nepali3DMeshBuilder.BuildQuarry();
            quarry.AddComponent<BuildingInstance>();
            PrefabUtility.SaveAsPrefabAsset(quarry, "Assets/Prefabs/StoneQuarry_3D.prefab");
            Object.DestroyImmediate(quarry);

            // Gorkha Soldier
            GameObject unit = Nepali3DMeshBuilder.BuildUnit();
            PrefabUtility.SaveAsPrefabAsset(unit, "Assets/Prefabs/GorkhaSoldier_3D.prefab");
            Object.DestroyImmediate(unit);

            // Pine Tree
            GameObject tree = Nepali3DMeshBuilder.BuildPineTree();
            PrefabUtility.SaveAsPrefabAsset(tree, "Assets/Prefabs/PineTree_3D.prefab");
            Object.DestroyImmediate(tree);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Gorkha/Regenerate Unit 3D Prefabs", false, 10)]
        public static void RegenerateUnitPrefabs()
        {
            EnsureAssetFolders();

            // Khukuri Warrior
            GameObject kw = Nepali3DMeshBuilder.BuildUnit(null, UnitType.KhukuriWarrior, false);
            PrefabUtility.SaveAsPrefabAsset(kw, "Assets/Prefabs/GorkhaSoldier_3D.prefab");
            Object.DestroyImmediate(kw);

            // Spearman
            GameObject sp = Nepali3DMeshBuilder.BuildUnit(null, UnitType.Spearman, false);
            PrefabUtility.SaveAsPrefabAsset(sp, "Assets/Prefabs/Spearman_3D.prefab");
            Object.DestroyImmediate(sp);

            // Archer
            GameObject ar = Nepali3DMeshBuilder.BuildUnit(null, UnitType.Archer, false);
            PrefabUtility.SaveAsPrefabAsset(ar, "Assets/Prefabs/Archer_3D.prefab");
            Object.DestroyImmediate(ar);

            // Villager
            GameObject vi = Nepali3DMeshBuilder.BuildUnit(null, UnitType.Villager, false);
            PrefabUtility.SaveAsPrefabAsset(vi, "Assets/Prefabs/Villager_3D.prefab");
            Object.DestroyImmediate(vi);

            // Hostile Bandit Raider
            GameObject bandit = Nepali3DMeshBuilder.BuildUnit(null, UnitType.KhukuriWarrior, true);
            PrefabUtility.SaveAsPrefabAsset(bandit, "Assets/Prefabs/HostileBandit_3D.prefab");
            Object.DestroyImmediate(bandit);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF7F><b>[Gorkha 3D]</b> All Unit 3D Prefabs regenerated with Mixamo character models!</color>");
        }

        [MenuItem("Gorkha/Upgrade Scene Units to Mixamo Models", false, 11)]
        public static void UpgradeSceneUnits()
        {
            var units = Object.FindObjectsByType<UnitController>(FindObjectsSortMode.None);
            int count = 0;
            foreach (var u in units)
            {
                u.SetUnitType(u.Type);
                EditorUtility.SetDirty(u.gameObject);
                count++;
            }
            Debug.Log($"<color=#00FF7F><b>[Gorkha 3D]</b> Successfully upgraded {count} scene units to Mixamo 3D models!</color>");
        }
    }
}
#endif
