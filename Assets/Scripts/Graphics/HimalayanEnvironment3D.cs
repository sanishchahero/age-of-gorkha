using System;
using UnityEngine;
using UnityEngine.Rendering;
using Gorkha.Core;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Constructs the high-fidelity 3D Himalayan mountain valley environment:
    /// - Richly textured alpine ground using Atlas_Ground_Rocks with 16x16 tiled detail
    /// - Living alpine lake & stream on western flank with animated wave refraction & foam (WaterLake.mat)
    /// - Waving meadow grass and wild flower clusters (Scatter_Grass with Atlas_Foliage wind sway)
    /// - Fortified stone kingdom perimeter walls, gate arches & corner towers (SM_Town_Wall_Border)
    /// - Golden-hour directional lighting & Desktop URP Volume Profile with SSAO ambient occlusion
    /// - Living atmospheric VFX: chimney smoke plumes, swirling autumn leaves, pollen, and soaring birds
    /// Operates in BOTH Edit Mode and Play Mode via [ExecuteAlways].
    /// </summary>
    [ExecuteAlways]
    public class HimalayanEnvironment3D : MonoBehaviour
    {
        [Header("Environment Configuration")]
        [SerializeField] private bool _setupLighting = true;
        [SerializeField] private bool _setupPostProcessing = true;
        [SerializeField] private bool _buildAlpineLake = true;
        [SerializeField] private bool _buildFortifiedWalls = true;
        [SerializeField] private bool _spawnMeadowGrass = true;
        [SerializeField] private bool _spawnPerimeterForest = true;
        [SerializeField] private bool _spawnAtmosphericVFX = true;

        [Header("Scenery Density")]
        [SerializeField] private int _treeCount = 44;
        [SerializeField] private int _boulderCount = 22;
        [SerializeField] private int _grassClumpCount = 38;

        private void Awake()
        {
            EnsureEnvironment();
        }

        private void OnEnable()
        {
            EnsureEnvironment();
        }

        private void Start()
        {
            EnsureEnvironment();
        }

        public void EnsureEnvironment()
        {
            // 1. Central Textured Alpine Ground & Pavements
            if (transform.Find("SettlementGround_3D") == null)
            {
                BuildValleyTerrain();
            }

            // 2. Scenic Living Alpine Lake & Water Surface
            if (_buildAlpineLake && transform.Find("AlpineLake_3D") == null)
            {
                BuildAlpineLake();
            }

            // 3. Fortified Stone Perimeter Walls & Gates
            if (_buildFortifiedWalls && transform.Find("PerimeterWalls_3D") == null)
            {
                BuildFortifiedPerimeterWalls();
            }

            // 4. Waving Wind-Animated Meadow Grass Scatter
            if (_spawnMeadowGrass && transform.Find("MeadowGrass_3D") == null)
            {
                SpawnMeadowGrassScatter();
            }

            // 5. SpeedTree 9 Wind-Animated Pine Forests & Granite Boulders
            if (_spawnPerimeterForest && transform.Find("PerimeterScenery_3D") == null)
            {
                SpawnPerimeterScenery();
            }

            // 6. Directional Lighting & Desktop Volume Profile (SSAO, ACES Tonemapping, Bloom)
            if (_setupLighting)
            {
                SetupHimalayanAtmosphere();
            }

            // 7. Atmospheric Environmental VFX (Chimney Smoke, Swirling Leaves, Sunlit Pollen, Birds)
            if (_spawnAtmosphericVFX)
            {
                EnsureAtmosphericVFX();
            }
        }

        public void BuildValleyTerrain()
        {
            int width = IsometricGrid.Instance != null ? IsometricGrid.Instance.GridWidth : 24;
            int height = IsometricGrid.Instance != null ? IsometricGrid.Instance.GridHeight : 24;
            float cellSize = IsometricGrid.Instance != null ? IsometricGrid.Instance.CellSize : 2.0f;

            float mapWidth = width * cellSize;
            float mapHeight = height * cellSize;

            // 1. Central Settlement Ground Plane with Sharp UV Tiling
            GameObject ground = new GameObject("SettlementGround_3D");
            ground.transform.SetParent(transform, false);
            ground.transform.position = new Vector3(mapWidth * 0.5f, -0.05f, mapHeight * 0.5f);

            MeshFilter filter = ground.AddComponent<MeshFilter>();
            // 16x16 UV tiling gives crisp rock pebbles, grass tufts, and mountain loam
            filter.sharedMesh = Nepali3DMeshBuilder.CreateTiledPlaneMesh(
                new Vector3(mapWidth + 12f, 0.2f, mapHeight + 12f),
                new Vector2(16f, 16f)
            );

            MeshRenderer renderer = ground.AddComponent<MeshRenderer>();
            Material groundMat = FantasyKingdomAssetRegistry.LoadMaterial(FantasyKingdomAssetRegistry.MAT_GROUND_ROCKS)
                              ?? GorkhaMaterials.AlpineGrass;
            renderer.sharedMaterial = groundMat;
            renderer.receiveShadows = true;

            // Ground Box Collider for Raycasts and building placement
            BoxCollider col = ground.AddComponent<BoxCollider>();
            col.size = new Vector3(mapWidth + 32f, 0.2f, mapHeight + 32f);

            // 2. Cobblestone Walkway Network (Connecting Plaza to Workshops & Military)
            GameObject walkways = new GameObject("CobblestoneWalkways_3D");
            walkways.transform.SetParent(transform, false);

            float cx = 24.0f;
            float cz = 24.0f;

            // Central Durbar Flagstone Plaza
            Nepali3DMeshBuilder.CreateSubMesh(
                "DurbarPlaza_Paving",
                Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(7.4f, 0.04f, 7.4f)),
                GorkhaMaterials.CobblestonePath,
                walkways.transform,
                new Vector3(cx, 0.02f, cz)
            );

            // North Road towards Kot (Barracks)
            Nepali3DMeshBuilder.CreateSubMesh(
                "Road_North_Kot",
                Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(2.0f, 0.04f, 5.8f)),
                GorkhaMaterials.CobblestonePath,
                walkways.transform,
                new Vector3(cx, 0.02f, cz + 5.8f)
            );

            // West Road towards Farmstead & Windmill
            Nepali3DMeshBuilder.CreateSubMesh(
                "Road_West_Farm",
                Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(5.8f, 0.04f, 2.0f)),
                GorkhaMaterials.CobblestonePath,
                walkways.transform,
                new Vector3(cx - 5.8f, 0.02f, cz)
            );

            // East Road towards Lumber Camp
            Nepali3DMeshBuilder.CreateSubMesh(
                "Road_East_WoodCamp",
                Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(5.8f, 0.04f, 2.0f)),
                GorkhaMaterials.CobblestonePath,
                walkways.transform,
                new Vector3(cx + 5.8f, 0.02f, cz)
            );

            // South Road towards Main Gate
            Nepali3DMeshBuilder.CreateSubMesh(
                "Road_South_Gate",
                Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(2.4f, 0.04f, 18.0f)),
                GorkhaMaterials.CobblestonePath,
                walkways.transform,
                new Vector3(cx, 0.02f, cz - 11.5f)
            );

            // Decorative Stone Steles / Chortens at Plaza Corners
            float pOff = 3.9f;
            Vector3[] pillars = {
                new Vector3(cx - pOff, 0.4f, cz - pOff), new Vector3(cx + pOff, 0.4f, cz - pOff),
                new Vector3(cx - pOff, 0.4f, cz + pOff), new Vector3(cx + pOff, 0.4f, cz + pOff)
            };
            foreach (var pPos in pillars)
            {
                Nepali3DMeshBuilder.CreateSubMesh("ChortenBase", Nepali3DMeshBuilder.CreateCubeMesh(new Vector3(0.50f, 0.85f, 0.50f)), GorkhaMaterials.SlateStone, walkways.transform, pPos);
                Nepali3DMeshBuilder.CreateSubMesh("ChortenCap", Nepali3DMeshBuilder.CreatePyramidMesh(0.60f, 0.40f), GorkhaMaterials.GoldenGajur, walkways.transform, pPos + new Vector3(0, 0.55f, 0));
            }
        }

        public void BuildAlpineLake()
        {
            GameObject lakeRoot = new GameObject("AlpineLake_3D");
            lakeRoot.transform.SetParent(transform, false);

            // Lake water plane on western flank
            Vector3 lakeCenter = new Vector3(-12.0f, -0.08f, 24.0f);
            Vector3 lakeSize = new Vector3(24.0f, 0.05f, 54.0f);

            GameObject waterPlane = new GameObject("WaterSurface");
            waterPlane.transform.SetParent(lakeRoot.transform, false);
            waterPlane.transform.position = lakeCenter;

            MeshFilter mf = waterPlane.AddComponent<MeshFilter>();
            mf.sharedMesh = Nepali3DMeshBuilder.CreateTiledPlaneMesh(lakeSize, new Vector2(4f, 8f));

            MeshRenderer mr = waterPlane.AddComponent<MeshRenderer>();
            Material waterMat = FantasyKingdomAssetRegistry.LoadMaterial(FantasyKingdomAssetRegistry.MAT_WATER_LAKE)
                             ?? GorkhaMaterials.WhitePlaster;
            mr.sharedMaterial = waterMat;
            mr.receiveShadows = true;

            // Submerged gravel lakebed
            GameObject bed = new GameObject("LakeBed");
            bed.transform.SetParent(lakeRoot.transform, false);
            bed.transform.position = lakeCenter + new Vector3(0, -0.6f, 0);

            MeshFilter bmf = bed.AddComponent<MeshFilter>();
            bmf.sharedMesh = Nepali3DMeshBuilder.CreateTiledPlaneMesh(lakeSize + new Vector3(2f, 0, 2f), new Vector2(8f, 16f));

            MeshRenderer bmr = bed.AddComponent<MeshRenderer>();
            bmr.sharedMaterial = FantasyKingdomAssetRegistry.LoadMaterial(FantasyKingdomAssetRegistry.MAT_GROUND_ROCKS)
                              ?? GorkhaMaterials.SlateStone;

            // Shoreline Granite Boulders
            System.Random rand = new System.Random(8848);
            for (int i = 0; i < 14; i++)
            {
                float z = -2.0f + (i * 4.0f) + (float)(rand.NextDouble() * 1.5f - 0.75f);
                float x = -0.5f - (float)(rand.NextDouble() * 1.8f);
                float scale = 0.65f + (float)rand.NextDouble() * 0.75f;

                GameObject rock = FantasyKingdomAssetRegistry.InstantiateRock(lakeRoot.transform, i, scale);
                rock.transform.position = new Vector3(x, 0.02f, z);
                rock.transform.rotation = Quaternion.Euler(0, (float)rand.NextDouble() * 360f, 0);
            }
        }

        public void BuildFortifiedPerimeterWalls()
        {
            GameObject wallRoot = new GameObject("PerimeterWalls_3D");
            wallRoot.transform.SetParent(transform, false);

            float minX = 0f;
            float maxX = 48f;
            float minZ = 0f;
            float maxZ = 48f;
            float wallY = 0f;

            // 1. Four Corner Bastions
            Vector3[] cornerPositions = {
                new Vector3(minX, wallY, minZ),
                new Vector3(maxX, wallY, minZ),
                new Vector3(maxX, wallY, maxZ),
                new Vector3(minX, wallY, maxZ)
            };
            float[] cornerRotations = { 0f, -90f, 180f, 90f };

            for (int i = 0; i < 4; i++)
            {
                FantasyKingdomAssetRegistry.InstantiateTownWall(
                    wallRoot.transform,
                    FantasyKingdomAssetRegistry.PREFAB_WALL_CORNER,
                    cornerPositions[i],
                    Quaternion.Euler(0, cornerRotations[i], 0),
                    scale: 0.38f
                );
            }

            // 2. Grand South Gatehouse Archway (Main entrance at x=24, z=0)
            FantasyKingdomAssetRegistry.InstantiateTownWall(
                wallRoot.transform,
                FantasyKingdomAssetRegistry.PREFAB_WALL_GATE,
                new Vector3(24.0f, wallY, minZ),
                Quaternion.Euler(0, 0, 0),
                scale: 0.38f
            );

            // 3. North Mountain Gatehouse (at x=24, z=48)
            FantasyKingdomAssetRegistry.InstantiateTownWall(
                wallRoot.transform,
                FantasyKingdomAssetRegistry.PREFAB_WALL_GATE,
                new Vector3(24.0f, wallY, maxZ),
                Quaternion.Euler(0, 180f, 0),
                scale: 0.38f
            );

            // 4. Straight Wall Segments along East Boundary (x=48)
            for (float z = 8f; z <= 40f; z += 8f)
            {
                FantasyKingdomAssetRegistry.InstantiateTownWall(
                    wallRoot.transform,
                    FantasyKingdomAssetRegistry.PREFAB_WALL_STRAIGHT,
                    new Vector3(maxX, wallY, z),
                    Quaternion.Euler(0, -90f, 0),
                    scale: 0.38f
                );
            }

            // 5. Straight Wall Segments along South & North Boundaries
            float[] gateSpannedX = { 8f, 16f, 32f, 40f };
            foreach (float x in gateSpannedX)
            {
                // South Wall
                FantasyKingdomAssetRegistry.InstantiateTownWall(
                    wallRoot.transform,
                    FantasyKingdomAssetRegistry.PREFAB_WALL_STRAIGHT,
                    new Vector3(x, wallY, minZ),
                    Quaternion.Euler(0, 0, 0),
                    scale: 0.38f
                );

                // North Wall
                FantasyKingdomAssetRegistry.InstantiateTownWall(
                    wallRoot.transform,
                    FantasyKingdomAssetRegistry.PREFAB_WALL_STRAIGHT,
                    new Vector3(x, wallY, maxZ),
                    Quaternion.Euler(0, 180f, 0),
                    scale: 0.38f
                );
            }
        }

        public void SpawnMeadowGrassScatter()
        {
            GameObject grassRoot = new GameObject("MeadowGrass_3D");
            grassRoot.transform.SetParent(transform, false);

            System.Random rand = new System.Random(1923);

            for (int i = 0; i < _grassClumpCount; i++)
            {
                // Cluster grass around settlement periphery, roadsides, and lake edge
                float x, z;
                double zone = rand.NextDouble();
                if (zone < 0.35)
                {
                    // Lake edge
                    x = 0.5f + (float)rand.NextDouble() * 3.5f;
                    z = (float)(rand.NextDouble() * 46f + 1f);
                }
                else if (zone < 0.70)
                {
                    // North/East forest edge
                    x = (float)(rand.NextDouble() * 44f + 2f);
                    z = (float)(rand.NextDouble() > 0.5 ? 43f + rand.NextDouble() * 4f : 1f + rand.NextDouble() * 4f);
                }
                else
                {
                    // Open meadow corners
                    x = (float)(rand.NextDouble() * 46f + 1f);
                    z = (float)(rand.NextDouble() * 46f + 1f);
                }

                float scale = 0.75f + (float)rand.NextDouble() * 0.5f;
                FantasyKingdomAssetRegistry.InstantiateScatterGrass(grassRoot.transform, i, new Vector3(x, 0.01f, z), scale);
            }
        }

        public void SetupHimalayanAtmosphere()
        {
            // 1. Directional Golden Hour Sun
            Light sun = FindAnyObjectByType<Light>();
            if (sun == null || sun.type != LightType.Directional)
            {
                GameObject sunObj = new GameObject("HimalayanSun_DirectionalLight");
                sunObj.transform.SetParent(transform, false);
                sun = sunObj.AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            sun.transform.rotation = Quaternion.Euler(50.0f, -35.0f, 0.0f);
            sun.color = new Color(1.0f, 0.95f, 0.88f); // Warm alpine sun
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.shadowResolution = LightShadowResolution.VeryHigh;
            sun.shadowStrength = 0.75f;
            sun.shadowNormalBias = 0.35f;

            // 2. Mountain Mist Fog & Trilight Ambient
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.76f, 0.92f);   // Alpine Sky Blue
            RenderSettings.ambientEquatorColor = new Color(0.76f, 0.70f, 0.60f);// Valley Terraces
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.36f, 0.20f); // Forest Shadow Floor

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.74f, 0.83f, 0.91f);
            RenderSettings.fogDensity = 0.007f;

            // 3. Auto-Configure Desktop Volume Profile (SSAO, ACES Tonemapping, Bloom)
            if (_setupPostProcessing)
            {
                Volume volume = FindAnyObjectByType<Volume>();
                if (volume == null)
                {
                    GameObject volObj = new GameObject("Global Volume");
                    volume = volObj.AddComponent<Volume>();
                    volume.isGlobal = true;
                }

#if UNITY_EDITOR
                var desktopProfile = UnityEditor.AssetDatabase.LoadAssetAtPath<VolumeProfile>(FantasyKingdomAssetRegistry.PROFILE_DESKTOP);
                if (desktopProfile != null)
                {
                    volume.sharedProfile = desktopProfile;
                }
#endif
            }
        }

        public void EnsureAtmosphericVFX()
        {
            HimalayanAtmosphericVFX vfx = FindAnyObjectByType<HimalayanAtmosphericVFX>();
            if (vfx == null)
            {
                GameObject vfxObj = new GameObject("HimalayanAtmosphericVFX_Manager");
                vfx = vfxObj.AddComponent<HimalayanAtmosphericVFX>();
            }
            vfx.EnsureAtmosphericVFX();
        }

        public void SpawnPerimeterScenery()
        {
            int width = IsometricGrid.Instance != null ? IsometricGrid.Instance.GridWidth : 24;
            int height = IsometricGrid.Instance != null ? IsometricGrid.Instance.GridHeight : 24;
            float cellSize = IsometricGrid.Instance != null ? IsometricGrid.Instance.CellSize : 2.0f;

            float mapWidth = width * cellSize;
            float mapHeight = height * cellSize;

            GameObject sceneryRoot = new GameObject("PerimeterScenery_3D");
            sceneryRoot.transform.SetParent(transform, false);

            System.Random rand = new System.Random(1769);

            for (int i = 0; i < _treeCount; i++)
            {
                float x, z;
                if (i % 2 == 0)
                {
                    x = (float)(rand.NextDouble() * (mapWidth + 14f) - 7f);
                    z = (float)(rand.NextDouble() > 0.5 ? mapHeight + rand.NextDouble() * 12f : -rand.NextDouble() * 8f);
                }
                else
                {
                    x = (float)(rand.NextDouble() > 0.5 ? mapWidth + rand.NextDouble() * 12f : -rand.NextDouble() * 8f);
                    z = (float)(rand.NextDouble() * (mapHeight + 14f) - 7f);
                }

                GameObject tree = FantasyKingdomAssetRegistry.InstantiatePineTree(sceneryRoot.transform, i);
                float scale = 0.85f + (float)rand.NextDouble() * 0.45f;
                tree.transform.position = new Vector3(x, 0, z);
                tree.transform.localScale = Vector3.one * scale;
                tree.transform.rotation = Quaternion.Euler(0, (float)rand.NextDouble() * 360f, 0);
            }

            for (int i = 0; i < _boulderCount; i++)
            {
                float x = (float)(rand.NextDouble() * (mapWidth + 10f) - 5f);
                float z = (float)(rand.NextDouble() > 0.5 ? mapHeight + rand.NextDouble() * 8f : -rand.NextDouble() * 6f);

                GameObject boulder = FantasyKingdomAssetRegistry.InstantiateRock(sceneryRoot.transform, i, 0.7f + (float)rand.NextDouble() * 0.8f);
                boulder.transform.position = new Vector3(x, 0, z);
                boulder.transform.rotation = Quaternion.Euler(0, (float)rand.NextDouble() * 360f, 0);
            }
        }
    }
}
