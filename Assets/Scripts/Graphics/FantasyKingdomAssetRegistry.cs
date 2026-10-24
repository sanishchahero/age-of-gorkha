using System;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Buildings;
using Gorkha.Economy;
using Gorkha.Units;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Calibration metadata for a Fantasy Kingdom 3D prefab.
    /// Provides exact center offsets, grounding height, and proportional scale.
    /// </summary>
    public struct BuildingCalibration
    {
        public string PrefabPath;
        public Vector3 CenterOffset; // Center in unscaled mesh space
        public float Scale;
        public float YOffset;
        public float CustomHealth;

        public BuildingCalibration(string path, Vector3 center, float scale, float yOffset = 0.02f, float customHealth = 500f)
        {
            PrefabPath = path;
            CenterOffset = center;
            Scale = scale;
            YOffset = yOffset;
            CustomHealth = customHealth;
        }
    }

    /// <summary>
    /// Master registry and visual factory connecting Unity 6 Fantasy Kingdom stylized 3D assets
    /// to kingdom structures, defensive buildings, and natural Himalayan flora.
    /// Operates in BOTH Edit Mode and Play Mode.
    /// </summary>
    public static class FantasyKingdomAssetRegistry
    {
        // -------------------------------------------------------------
        // Asset Path Constants (Assets/Art/Prefabs/...)
        // -------------------------------------------------------------
        public const string PREFAB_TOWN_CENTER = "Assets/Art/Prefabs/Houses/Preset_Church_01_A.prefab";
        public const string PREFAB_BARRACKS    = "Assets/Art/Prefabs/Houses/Preset_Blacksmith_01.prefab";
        public const string PREFAB_FARM        = "Assets/Art/Prefabs/Houses/Preset_House_Windmill_01.prefab";
        public const string PREFAB_WOOD_CAMP   = "Assets/Art/Prefabs/Houses/Preset_Shelter_02.prefab";
        public const string PREFAB_QUARRY      = "Assets/Art/Prefabs/Houses/Preset_Hut_01.prefab";
        public const string PREFAB_HOUSE       = "Assets/Art/Prefabs/Houses/Preset_House_01_A.prefab";
        public const string PREFAB_WATCHTOWER  = "Assets/Art/Prefabs/Houses/Preset_Tower_01.prefab";

        // Foliage & Rocks
        public const string PREFAB_PINE_A      = "Assets/Art/Prefabs/Foliage/Trees and Bushes/TreePine_VarA.prefab";
        public const string PREFAB_PINE_B      = "Assets/Art/Prefabs/Foliage/Trees and Bushes/TreePine_VarB.prefab";
        public const string PREFAB_PINE_C      = "Assets/Art/Prefabs/Foliage/Trees and Bushes/TreePine_VarC.prefab";
        public const string PREFAB_ROCK_01     = "Assets/Art/Prefabs/Rocks/Rocks_01.prefab";
        public const string PREFAB_ROCK_11     = "Assets/Art/Prefabs/Rocks/Rocks_11.prefab";

        // Materials & Post-Processing
        public const string MAT_GROUND_ROCKS   = "Assets/Art/Materials/Atlas Materials/Atlas_Ground_Rocks.mat";
        public const string MAT_WATER_LAKE     = "Assets/Art/Materials/Water/WaterLake.mat";
        public const string PROFILE_DESKTOP    = "Assets/URP/VolumeProfiles/Global Volume Profile Desktop.asset";

        // Fortified Perimeter Walls & Gates
        public const string PREFAB_WALL_STRAIGHT = "Assets/Art/Prefabs/Town Wall Border/SM_Town_Wall_Border_01.prefab";
        public const string PREFAB_WALL_CORNER   = "Assets/Art/Prefabs/Town Wall Border/SM_Town_Wall_Border_04.prefab";
        public const string PREFAB_WALL_GATE     = "Assets/Art/Prefabs/Town Wall Border/SM_Town_Wall_Border_10.prefab";

        // Compositions & Water
        public const string PREFAB_WATER         = "Assets/Art/Prefabs/Compositions/Water.prefab";

        // -------------------------------------------------------------
        // Calibration Dictionary (Exact Mathematical Bounds & Centering)
        // -------------------------------------------------------------
        private static readonly Dictionary<BuildingType, BuildingCalibration> _calibrations = new()
        {
            {
                BuildingType.TownCenter,
                new BuildingCalibration(
                    PREFAB_TOWN_CENTER,
                    new Vector3(1.25f, -2.50f, -3.82f),
                    scale: 0.45f,
                    yOffset: 0.02f,
                    customHealth: 2000f
                )
            },
            {
                BuildingType.Barracks,
                new BuildingCalibration(
                    PREFAB_BARRACKS,
                    new Vector3(-0.65f, -2.50f, 1.71f),
                    scale: 0.38f,
                    yOffset: 0.02f,
                    customHealth: 850f
                )
            },
            {
                BuildingType.Farm,
                new BuildingCalibration(
                    PREFAB_FARM,
                    new Vector3(1.20f, -2.50f, -1.97f),
                    scale: 0.38f,
                    yOffset: 0.02f,
                    customHealth: 450f
                )
            },
            {
                BuildingType.WoodCamp,
                new BuildingCalibration(
                    PREFAB_WOOD_CAMP,
                    new Vector3(3.75f, -1.26f, -2.50f),
                    scale: 0.45f,
                    yOffset: 0.02f,
                    customHealth: 500f
                )
            },
            {
                BuildingType.StoneQuarry,
                new BuildingCalibration(
                    PREFAB_QUARRY,
                    new Vector3(3.01f, -2.50f, -3.59f),
                    scale: 0.45f,
                    yOffset: 0.02f,
                    customHealth: 550f
                )
            },
            {
                BuildingType.House,
                new BuildingCalibration(
                    PREFAB_HOUSE,
                    new Vector3(1.93f, -2.50f, -2.49f),
                    scale: 0.38f,
                    yOffset: 0.02f,
                    customHealth: 400f
                )
            },
            {
                BuildingType.Watchtower,
                new BuildingCalibration(
                    PREFAB_WATCHTOWER,
                    new Vector3(0.02f, -0.75f, 0.19f),
                    scale: 0.40f,
                    yOffset: 0.02f,
                    customHealth: 750f
                )
            }
        };

        // Cache loaded prefab assets
        private static readonly Dictionary<string, GameObject> _loadedPrefabCache = new();

        /// <summary>
        /// Checks if a Fantasy Kingdom 3D prefab is configured for the given building type.
        /// </summary>
        public static bool HasPrefab(BuildingType type)
        {
            return _calibrations.ContainsKey(type);
        }

        /// <summary>
        /// Retrieves the registered building calibration.
        /// </summary>
        public static bool TryGetCalibration(BuildingType type, out BuildingCalibration calibration)
        {
            return _calibrations.TryGetValue(type, out calibration);
        }

        /// <summary>
        /// Loads the source prefab GameObject by building type (with caching).
        /// </summary>
        public static GameObject GetPrefab(BuildingType type)
        {
            if (_calibrations.TryGetValue(type, out var cal))
            {
                return LoadPrefab(cal.PrefabPath);
            }
            return null;
        }

        /// <summary>
        /// Loads a prefab at the given path via AssetDatabase (Editor) or Resources.
        /// </summary>
        public static GameObject LoadPrefab(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            if (_loadedPrefabCache.TryGetValue(path, out var cached) && cached != null)
            {
                return cached;
            }

            GameObject loaded = null;

#if UNITY_EDITOR
            loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#endif

            if (loaded == null)
            {
                // Try Resources fallback
                string resPath = path.Replace("Assets/Resources/", "").Replace(".prefab", "");
                loaded = Resources.Load<GameObject>(resPath);
            }

            if (loaded != null)
            {
                _loadedPrefabCache[path] = loaded;
            }

            return loaded;
        }

        /// <summary>
        /// Instantiates a beautifully centered and scaled Fantasy Kingdom 3D model for a building.
        /// Disables child colliders so raycasts and clicks cleanly target the parent BuildingInstance.
        /// </summary>
        public static GameObject InstantiateBuildingVisual(BuildingType type, Transform parent, bool isPreview = false)
        {
            if (!_calibrations.TryGetValue(type, out var cal)) return null;

            GameObject prefab = LoadPrefab(cal.PrefabPath);
            if (prefab == null) return null;

            // 1. Create clean root anchor
            GameObject anchor = new GameObject($"FK_Visual_{type}");
            if (parent != null)
            {
                anchor.transform.SetParent(parent, false);
            }
            anchor.transform.localPosition = Vector3.zero;
            anchor.transform.localRotation = Quaternion.identity;
            anchor.transform.localScale = Vector3.one;

            // 2. Instantiate source model inside anchor
            GameObject model = UnityEngine.Object.Instantiate(prefab, anchor.transform);
            model.name = prefab.name;

            // 3. Apply exact mathematical centering and scaling
            // CenterOffset is negated and scaled so the model's geometric center aligns with (0, yOffset, 0)
            model.transform.localPosition = new Vector3(
                -cal.CenterOffset.x * cal.Scale,
                cal.YOffset,
                -cal.CenterOffset.z * cal.Scale
            );
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one * cal.Scale;

            // 4. Disable colliders on visual children (BuildingInstance manages its own root collider)
            foreach (var col in model.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // 5. Kinetic machinery (spinning windmill blades) and armory/outpost props
            AttachBuildingDecorationsAndMachinery(type, anchor, model);

            return anchor;
        }

        /// <summary>
        /// Decorates buildings with kinetic machinery (spinning windmill sails)
        /// and immersive outpost dressing (armory weapon racks, blacksmith anvils, archery targets, freight crates).
        /// </summary>
        private static void AttachBuildingDecorationsAndMachinery(BuildingType type, GameObject anchor, GameObject model)
        {
            if (anchor == null) return;

            switch (type)
            {
                case BuildingType.Farm:
                    if (model != null)
                    {
                        // Attach kinetic windmill blades at the hub axle of Preset_House_Windmill_01
                        // Hub axle in unscaled model coordinates is (0.97f, 14.88f, -2.69f)
                        Vector3 hubLocalPos = new Vector3(0.97f, 14.88f, -2.69f);
                        Nepali3DMeshBuilder.BuildWindmillBlades(model.transform, hubLocalPos);
                    }
                    // Supply and grain barrel cluster outside the farmstead
                    Nepali3DMeshBuilder.BuildSupplyCrateCluster(anchor.transform, new Vector3(2.2f, 0, -1.8f), Quaternion.Euler(0, 30f, 0));
                    break;

                case BuildingType.Barracks:
                    // Outdoor Kot armory yard: Anvil, Weapon Rack with spears, Archery target
                    Nepali3DMeshBuilder.BuildAnvil(anchor.transform, new Vector3(2.2f, 0, 1.2f), Quaternion.Euler(0, -20f, 0));
                    Nepali3DMeshBuilder.BuildWeaponRack(anchor.transform, new Vector3(-2.0f, 0, 1.4f), Quaternion.Euler(0, 25f, 0));
                    Nepali3DMeshBuilder.BuildArcheryTarget(anchor.transform, new Vector3(-2.2f, 0, -1.6f), Quaternion.Euler(0, -35f, 0));
                    break;

                case BuildingType.WoodCamp:
                    // Forestry lumber crates and woodsman tools
                    Nepali3DMeshBuilder.BuildSupplyCrateCluster(anchor.transform, new Vector3(1.8f, 0, 1.2f), Quaternion.Euler(0, 15f, 0));
                    Nepali3DMeshBuilder.BuildVillagerTool(anchor.transform, ResourceType.Wood, new Vector3(1.5f, 0.4f, -1.2f), Quaternion.Euler(75f, 30f, 0));
                    break;

                case BuildingType.StoneQuarry:
                    // Mining pickaxes and slate shipping crates
                    Nepali3DMeshBuilder.BuildSupplyCrateCluster(anchor.transform, new Vector3(-1.8f, 0, 1.4f), Quaternion.Euler(0, -25f, 0));
                    Nepali3DMeshBuilder.BuildVillagerTool(anchor.transform, ResourceType.Stone, new Vector3(1.6f, 0.35f, -1.0f), Quaternion.Euler(60f, -40f, 0));
                    break;

                case BuildingType.TownCenter:
                    // Royal command post weapon rack & storehouse crates
                    Nepali3DMeshBuilder.BuildWeaponRack(anchor.transform, new Vector3(2.5f, 0, -2.2f), Quaternion.Euler(0, -40f, 0));
                    Nepali3DMeshBuilder.BuildSupplyCrateCluster(anchor.transform, new Vector3(-2.6f, 0, -2.0f), Quaternion.Euler(0, 30f, 0));
                    break;
            }

            // Disable colliders on visual children so building clicks hit the root BuildingInstance collider
            foreach (var col in anchor.GetComponentsInChildren<Collider>())
            {
                if (col.gameObject != anchor && (anchor.transform.parent == null || col.gameObject != anchor.transform.parent.gameObject))
                {
                    col.enabled = false;
                }
            }
        }

        /// <summary>
        /// Instantiates a wind-animated Himalayan pine tree from the Fantasy Kingdom kit.
        /// Falls back to procedural mesh if prefab is not available.
        /// </summary>
        public static GameObject InstantiatePineTree(Transform parent = null, int variant = 0)
        {
            string path = (variant % 3) switch
            {
                1 => PREFAB_PINE_B,
                2 => PREFAB_PINE_C,
                _ => PREFAB_PINE_A
            };

            GameObject prefab = LoadPrefab(path);
            if (prefab != null)
            {
                GameObject tree = UnityEngine.Object.Instantiate(prefab, parent);
                tree.name = $"FK_PineTree_{variant}";
                tree.transform.localPosition = Vector3.zero;
                tree.transform.localRotation = Quaternion.identity;
                tree.transform.localScale = Vector3.one * 0.90f;
                return tree;
            }

            // Procedural fallback
            return Nepali3DMeshBuilder.BuildPineTree(parent);
        }

        /// <summary>
        /// Instantiates a granite Himalayan boulder from the Fantasy Kingdom kit.
        /// Falls back to procedural mesh if prefab is not available.
        /// </summary>
        public static GameObject InstantiateRock(Transform parent = null, int variant = 0, float scale = 1.0f)
        {
            string path = (variant % 2 == 1) ? PREFAB_ROCK_11 : PREFAB_ROCK_01;
            GameObject prefab = LoadPrefab(path);
            if (prefab != null)
            {
                GameObject rock = UnityEngine.Object.Instantiate(prefab, parent);
                rock.name = $"FK_Rock_{variant}";
                rock.transform.localPosition = Vector3.zero;
                rock.transform.localRotation = Quaternion.Euler(0, (variant * 73f) % 360f, 0);
                rock.transform.localScale = Vector3.one * (scale * 0.85f);
                return rock;
            }

            // Procedural fallback
            return Nepali3DMeshBuilder.BuildBoulder(parent, scale);
        }

        private static readonly Dictionary<string, Material> _loadedMaterialCache = new();

        /// <summary>
        /// Loads a material asset with caching.
        /// </summary>
        public static Material LoadMaterial(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (_loadedMaterialCache.TryGetValue(path, out var cached) && cached != null)
                return cached;

            Material loaded = null;
#if UNITY_EDITOR
            loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
#endif
            if (loaded != null)
            {
                _loadedMaterialCache[path] = loaded;
            }
            return loaded;
        }

        /// <summary>
        /// Retrieves one of the Synty POLYGON Starter pack materials (palette variants).
        /// Returns null now that Mixamo and authentic high-fidelity FBX assets are used.
        /// </summary>
        public static Material GetSyntyStarterMaterial(int index = 1)
        {
            return null;
        }

        /// <summary>
        /// Retrieves one of the Synty POLYGON Generic materials (palette variants).
        /// Returns null now that Mixamo and authentic high-fidelity FBX assets are used.
        /// </summary>
        public static Material GetSyntyGenericMaterial(int index = 1)
        {
            return null;
        }

        /// <summary>
        /// Retrieves character texture palette material. Returns null to preserve native embedded FBX textures.
        /// </summary>
        public static Material GetCharacterMaterial(bool isPeasant, bool isHostile)
        {
            return null;
        }

        /// <summary>
        /// Loads high-fidelity humanoid character FBX models (Mixamo & custom historical rigs) tailored by unit role.
        /// </summary>
        public static GameObject LoadCharacterPrefab(UnitType unitType, bool isFemale = false)
        {
            // 1. Check Resources/GorkhaCharacters/ (Mixamo models organized by unit role)
            string resName = unitType switch
            {
                UnitType.Villager => "Character_Villager",
                UnitType.Spearman => "Character_Spearman",
                UnitType.Archer => "Character_Archer",
                UnitType.KhukuriWarrior => "Character_KhukuriWarrior",
                _ => "Character_KhukuriWarrior"
            };

            GameObject resModel = Resources.Load<GameObject>("GorkhaCharacters/" + resName);
            if (resModel != null) return resModel;

            // 2. Check Assets/Art/Meshes/ downloaded Mixamo models
            string meshName = unitType switch
            {
                UnitType.Villager => "character_villager.fbx",
                UnitType.Spearman => "character_soldier.fbx",
                UnitType.Archer => "character_arrow.fbx",
                UnitType.KhukuriWarrior => "character.fbx",
                _ => "character.fbx"
            };

            GameObject directAsset = LoadPrefab("Assets/Art/Meshes/" + meshName);
            if (directAsset != null) return directAsset;

            // Space-separated filename fallback
            string spaceName = unitType switch
            {
                UnitType.Villager => "character villager.fbx",
                UnitType.Spearman => "character soldier.fbx",
                UnitType.Archer => "character arrow.fbx",
                UnitType.KhukuriWarrior => "character.fbx",
                _ => "character.fbx"
            };
            directAsset = LoadPrefab("Assets/Art/Meshes/" + spaceName);
            if (directAsset != null) return directAsset;

            // 3. Check Assets/Art/Characters/ fallback
            GameObject charAsset = LoadPrefab("Assets/Art/Characters/" + resName + ".fbx");
            if (charAsset != null) return charAsset;

            // 4. Historical custom Gorkhali warrior fallback
            GameObject customWarrior = Resources.Load<GameObject>("GorkhaCharacters/Gorkhali_Warrior_Historical");
            if (customWarrior != null) return customWarrior;

            return null;
        }

        /// <summary>
        /// Loads the humanoid character prefab (male or female) - backwards compatibility.
        /// </summary>
        public static GameObject LoadCharacterPrefab(bool isFemale = false)
        {
            return LoadCharacterPrefab(UnitType.KhukuriWarrior, isFemale);
        }

        /// <summary>
        /// Loads 3D tool/weapon prefabs (woodcutter axe, miner pickaxe).
        /// Returns null to use authentic Gorkha weapon models and procedural Salwood/Steel fallbacks.
        /// </summary>
        public static GameObject LoadWeaponPrefab(ResourceType resource)
        {
            return null;
        }

        /// <summary>
        /// Loads character cosmetic facial hair or hairstyle attachments.
        /// Returns null for Mixamo characters who have their own native hair and features.
        /// </summary>
        public static GameObject LoadAttachmentPrefab(string attachmentName)
        {
            return null;
        }

        /// <summary>
        /// Instantiates a wind-animated grass clump from the Fantasy Kingdom kit.
        /// </summary>
        public static GameObject InstantiateScatterGrass(Transform parent, int variant, Vector3 pos, float scale = 1.0f)
        {
            int index = (variant % 16) + 1;
            string path = $"Assets/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_{index:D2}.prefab";
            GameObject prefab = LoadPrefab(path);
            if (prefab != null)
            {
                GameObject grass = UnityEngine.Object.Instantiate(prefab, parent);
                grass.name = $"FK_Grass_{variant}";
                grass.transform.position = pos;
                grass.transform.rotation = Quaternion.Euler(0, (variant * 47f) % 360f, 0);
                grass.transform.localScale = Vector3.one * scale;

                foreach (var col in grass.GetComponentsInChildren<Collider>())
                    col.enabled = false;

                return grass;
            }
            return null;
        }

        /// <summary>
        /// Instantiates a modular stone wall or gatehouse from the Fantasy Kingdom town wall set.
        /// </summary>
        public static GameObject InstantiateTownWall(Transform parent, string prefabPath, Vector3 pos, Quaternion rot, float scale = 1.0f)
        {
            GameObject prefab = LoadPrefab(prefabPath);
            if (prefab != null)
            {
                GameObject wall = UnityEngine.Object.Instantiate(prefab, parent);
                wall.name = prefab.name;
                wall.transform.position = pos;
                wall.transform.rotation = rot;
                wall.transform.localScale = Vector3.one * scale;

                foreach (var col in wall.GetComponentsInChildren<Collider>())
                    col.enabled = false;

                return wall;
            }
            return null;
        }
    }
}
