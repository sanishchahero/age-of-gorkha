using System.Collections.Generic;
using UnityEngine;
using Gorkha.Economy;
using Gorkha.Units;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Procedural 3D model and hybrid mesh builder for authentic historical Nepali architecture,
    /// Himalayan flora, and military units.
    /// Combines solid 3D stone plinths, terracotta brick cores, tiered pagoda roofs,
    /// golden Gajur spires, and camera-facing artwork facades.
    /// </summary>
    public static class Nepali3DMeshBuilder
    {
        #region Primitive Mesh Helpers

        public static GameObject CreateSubMesh(string name, Mesh mesh, Material material, Transform parent = null, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject obj = new GameObject(name);
            if (parent != null) obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;

            return obj;
        }

        public static Mesh CreateCubeMesh(Vector3 size, Vector3 center = default)
        {
            Mesh mesh = new Mesh { name = "CubeMesh" };
            float hx = size.x * 0.5f, hy = size.y * 0.5f, hz = size.z * 0.5f;
            Vector3 c = center;

            Vector3[] vertices = new Vector3[]
            {
                // Front
                c + new Vector3(-hx, -hy,  hz), c + new Vector3( hx, -hy,  hz), c + new Vector3( hx,  hy,  hz), c + new Vector3(-hx,  hy,  hz),
                // Back
                c + new Vector3( hx, -hy, -hz), c + new Vector3(-hx, -hy, -hz), c + new Vector3(-hx,  hy, -hz), c + new Vector3( hx,  hy, -hz),
                // Top
                c + new Vector3(-hx,  hy,  hz), c + new Vector3( hx,  hy,  hz), c + new Vector3( hx,  hy, -hz), c + new Vector3(-hx,  hy, -hz),
                // Bottom
                c + new Vector3(-hx, -hy, -hz), c + new Vector3( hx, -hy, -hz), c + new Vector3( hx, -hy,  hz), c + new Vector3(-hx, -hy,  hz),
                // Left
                c + new Vector3(-hx, -hy, -hz), c + new Vector3(-hx, -hy,  hz), c + new Vector3(-hx,  hy,  hz), c + new Vector3(-hx,  hy, -hz),
                // Right
                c + new Vector3( hx, -hy,  hz), c + new Vector3( hx, -hy, -hz), c + new Vector3( hx,  hy, -hz), c + new Vector3( hx,  hy,  hz)
            };

            int[] triangles = new int[36];
            for (int i = 0; i < 6; i++)
            {
                int vi = i * 4;
                int ti = i * 6;
                triangles[ti] = vi;
                triangles[ti + 1] = vi + 2;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi;
                triangles[ti + 4] = vi + 3;
                triangles[ti + 5] = vi + 2;
            }

            Vector2[] uvs = new Vector2[24];
            for (int i = 0; i < 6; i++)
            {
                int vi = i * 4;
                uvs[vi] = new Vector2(0, 0);
                uvs[vi + 1] = new Vector2(1, 0);
                uvs[vi + 2] = new Vector2(1, 1);
                uvs[vi + 3] = new Vector2(0, 1);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Creates a cube/plane mesh with tiled UVs on top face for sharp high-resolution terrain rendering.
        /// </summary>
        public static Mesh CreateTiledPlaneMesh(Vector3 size, Vector2 uvTiling)
        {
            Mesh mesh = new Mesh { name = "TiledGroundMesh" };
            float hx = size.x * 0.5f;
            float hy = size.y * 0.5f;
            float hz = size.z * 0.5f;

            Vector3[] vertices = new Vector3[24]
            {
                // Top
                new Vector3(-hx,  hy, -hz), new Vector3( hx,  hy, -hz), new Vector3( hx,  hy,  hz), new Vector3(-hx,  hy,  hz),
                // Front
                new Vector3(-hx, -hy,  hz), new Vector3( hx, -hy,  hz), new Vector3( hx,  hy,  hz), new Vector3(-hx,  hy,  hz),
                // Back
                new Vector3( hx, -hy, -hz), new Vector3(-hx, -hy, -hz), new Vector3(-hx,  hy, -hz), new Vector3( hx,  hy, -hz),
                // Bottom
                new Vector3(-hx, -hy, -hz), new Vector3( hx, -hy, -hz), new Vector3( hx, -hy,  hz), new Vector3(-hx, -hy,  hz),
                // Left
                new Vector3(-hx, -hy, -hz), new Vector3(-hx, -hy,  hz), new Vector3(-hx,  hy,  hz), new Vector3(-hx,  hy, -hz),
                // Right
                new Vector3( hx, -hy,  hz), new Vector3( hx, -hy, -hz), new Vector3( hx,  hy, -hz), new Vector3( hx,  hy,  hz)
            };

            int[] triangles = new int[36];
            for (int i = 0; i < 6; i++)
            {
                int vi = i * 4;
                int ti = i * 6;
                triangles[ti] = vi;
                triangles[ti + 1] = vi + 2;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi;
                triangles[ti + 4] = vi + 3;
                triangles[ti + 5] = vi + 2;
            }

            Vector2[] uvs = new Vector2[24];
            // Top face gets tiled UVs
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(uvTiling.x, 0);
            uvs[2] = new Vector2(uvTiling.x, uvTiling.y);
            uvs[3] = new Vector2(0, uvTiling.y);

            // Other sides get 1x1
            for (int i = 1; i < 6; i++)
            {
                int vi = i * 4;
                uvs[vi] = new Vector2(0, 0);
                uvs[vi + 1] = new Vector2(1, 0);
                uvs[vi + 2] = new Vector2(1, 1);
                uvs[vi + 3] = new Vector2(0, 1);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreatePyramidMesh(float baseSize, float height, float flare = 0.2f, Vector3 center = default)
        {
            Mesh mesh = new Mesh { name = "PagodaTierMesh" };
            float hb = baseSize * 0.5f;
            float topSize = baseSize * 0.45f;
            float ht = topSize * 0.5f;

            Vector3 b0 = center + new Vector3(-hb - flare, 0, -hb - flare);
            Vector3 b1 = center + new Vector3( hb + flare, 0, -hb - flare);
            Vector3 b2 = center + new Vector3( hb + flare, 0,  hb + flare);
            Vector3 b3 = center + new Vector3(-hb - flare, 0,  hb + flare);

            Vector3 t0 = center + new Vector3(-ht, height, -ht);
            Vector3 t1 = center + new Vector3( ht, height, -ht);
            Vector3 t2 = center + new Vector3( ht, height,  ht);
            Vector3 t3 = center + new Vector3(-ht, height,  ht);

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
            {
                int start = verts.Count;
                verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);
                tris.Add(start); tris.Add(start + 2); tris.Add(start + 1);
                tris.Add(start); tris.Add(start + 3); tris.Add(start + 2);
            }

            AddQuad(b0, b1, t1, t0);
            AddQuad(b1, b2, t2, t1);
            AddQuad(b2, b3, t3, t2);
            AddQuad(b3, b0, t0, t3);
            AddQuad(t3, t2, t1, t0);

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateCylinderMesh(float radius, float height, int segments = 12, Vector3 center = default)
        {
            Mesh mesh = new Mesh { name = "CylinderMesh" };
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            float halfH = height * 0.5f;

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                verts.Add(center + new Vector3(x, -halfH, z));
                verts.Add(center + new Vector3(x,  halfH, z));
            }

            for (int i = 0; i < segments; i++)
            {
                int b0 = i * 2;
                int t0 = b0 + 1;
                int b1 = (i + 1) * 2;
                int t1 = b1 + 1;

                tris.Add(b0); tris.Add(t0); tris.Add(t1);
                tris.Add(b0); tris.Add(t1); tris.Add(b1);
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateQuadMesh(float width, float height)
        {
            Mesh mesh = new Mesh { name = "BillboardQuad" };
            float hw = width * 0.5f;

            Vector3[] verts = new Vector3[]
            {
                new Vector3(-hw, 0, 0),
                new Vector3( hw, 0, 0),
                new Vector3( hw, height, 0),
                new Vector3(-hw, height, 0)
            };

            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };

            int[] tris = new int[]
            {
                0, 2, 1, 0, 3, 2,
                0, 1, 2, 0, 2, 3
            };

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateDoubleTriangleFlagMesh(float width = 1.0f, float height = 1.4f)
        {
            Mesh mesh = new Mesh { name = "NepaliFlagMesh" };

            Vector3[] verts = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, height * 0.48f, 0),
                new Vector3(width * 0.85f, height * 0.24f, 0),
                new Vector3(0, height, 0),
                new Vector3(width, height * 0.74f, 0)
            };

            int[] tris = new int[]
            {
                0, 2, 1,
                0, 1, 2,
                1, 4, 3,
                1, 3, 4
            };

            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion

        #region Complete Building Visual Generators

        /// <summary>
        /// Generates the Gorkha Durbar / Town Center:
        /// 3D slate stone plinth steps, 3D red terracotta brick core, 3-tier curved pagoda roofs,
        /// golden Gajur pinnacle, fluttering royal double-triangle flag, and facade artwork.
        /// </summary>
        public static GameObject BuildTownCenter(Transform parent = null)
        {
            GameObject root = new GameObject("TownCenter_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            // 1. Plinth Foundation (Stepped Slate Stone)
            CreateSubMesh("Plinth_Tier1", CreateCubeMesh(new Vector3(3.8f, 0.25f, 3.8f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.125f, 0));
            CreateSubMesh("Plinth_Tier2", CreateCubeMesh(new Vector3(3.4f, 0.20f, 3.4f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.35f, 0));

            // 2. 3D Architectural Pagoda Core (Red Brick & Sal Wood Pillars)
            CreateSubMesh("GroundFloorCore", CreateCubeMesh(new Vector3(2.6f, 1.4f, 2.6f)), GorkhaMaterials.RedBrick, root.transform, new Vector3(0, 1.15f, 0));

            float colOff = 1.25f;
            Vector3[] cols = {
                new Vector3(-colOff, 1.15f, -colOff), new Vector3(colOff, 1.15f, -colOff),
                new Vector3(colOff, 1.15f, colOff), new Vector3(-colOff, 1.15f, colOff)
            };
            foreach (var pos in cols)
            {
                CreateSubMesh("TimberColumn", CreateCubeMesh(new Vector3(0.20f, 1.4f, 0.20f)), GorkhaMaterials.PagodaWood, root.transform, pos);
            }

            // 3. Roof Tier 1 (Lowest Pagoda)
            CreateSubMesh("RoofTier1", CreatePyramidMesh(3.4f, 0.65f, flare: 0.35f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 1.85f, 0));

            // 4. Middle Floor Core & Roof Tier 2
            CreateSubMesh("MidFloorCore", CreateCubeMesh(new Vector3(1.9f, 0.95f, 1.9f)), GorkhaMaterials.RedBrick, root.transform, new Vector3(0, 2.80f, 0));
            CreateSubMesh("RoofTier2", CreatePyramidMesh(2.6f, 0.55f, flare: 0.25f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 3.25f, 0));

            // 5. Top Floor & Roof Tier 3 (Apex)
            CreateSubMesh("TopFloorCore", CreateCubeMesh(new Vector3(1.3f, 0.70f, 1.3f)), GorkhaMaterials.WhitePlaster, root.transform, new Vector3(0, 4.05f, 0));
            CreateSubMesh("RoofTier3", CreatePyramidMesh(1.8f, 0.50f, flare: 0.20f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 4.35f, 0));

            // 6. Golden Gajur Spire (Metallic pinnacle)
            CreateSubMesh("GoldenGajurSpire", CreateCylinderMesh(0.10f, 0.85f), GorkhaMaterials.GoldenGajur, root.transform, new Vector3(0f, 5.10f, 0f));
            CreateSubMesh("GoldenGajurApex", CreateCubeMesh(new Vector3(0.22f, 0.22f, 0.22f)), GorkhaMaterials.GoldenGajur, root.transform, new Vector3(0f, 5.55f, 0f));

            // 7. Fluttering Royal Flag on Mast
            CreateSubMesh("FlagPole", CreateCylinderMesh(0.04f, 2.0f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(1.1f, 3.2f, -0.6f));
            GameObject flagObj = CreateSubMesh("WavingFlag", CreateDoubleTriangleFlagMesh(0.9f, 1.2f), GorkhaMaterials.RoyalRedFlag, root.transform, new Vector3(1.12f, 2.9f, -0.6f));
            flagObj.AddComponent<FlagClothAnimator>();

            // 8. Front Facade Authentic 3D Architectural Entrance Portal
            CreateSubMesh("PalaceDoorArch", CreateCubeMesh(new Vector3(1.1f, 1.25f, 0.12f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.95f, -1.31f));
            CreateSubMesh("PalaceDoorPanels", CreateCubeMesh(new Vector3(0.85f, 1.05f, 0.08f)), GorkhaMaterials.RedBrick, root.transform, new Vector3(0, 0.90f, -1.33f));
            CreateSubMesh("PalaceDoorGajur", CreatePyramidMesh(0.28f, 0.16f), GorkhaMaterials.GoldenGajur, root.transform, new Vector3(0, 1.62f, -1.31f));

            // 9. Palace Chimney Smoke Plume
            GorkhaVFX.AttachChimneySmoke(root.transform, new Vector3(-0.85f, 4.4f, 0.4f));

            // BoxCollider for selection
            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 2.5f, 0);
            col.size = new Vector3(3.8f, 5.2f, 3.8f);

            return root;
        }

        /// <summary>
        /// Generates the Terraced Farm:
        /// 3D slate stone terrace steps, soil beds, swaying wheat crops, and cottage.
        /// </summary>
        public static GameObject BuildFarm(Transform parent = null)
        {
            GameObject root = new GameObject("Farm_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            // 1. Lower Terrace
            CreateSubMesh("TerraceWall1", CreateCubeMesh(new Vector3(3.6f, 0.35f, 3.6f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.175f, 0));
            CreateSubMesh("SoilBed1", CreateCubeMesh(new Vector3(3.4f, 0.10f, 3.4f)), GorkhaMaterials.TerraceDirt, root.transform, new Vector3(0, 0.40f, 0));

            // 2. Stepped Upper Terrace
            CreateSubMesh("TerraceWall2", CreateCubeMesh(new Vector3(2.4f, 0.45f, 2.4f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0.5f, 0.625f, 0.5f));
            CreateSubMesh("SoilBed2", CreateCubeMesh(new Vector3(2.2f, 0.10f, 2.2f)), GorkhaMaterials.TerraceDirt, root.transform, new Vector3(0.5f, 0.90f, 0.5f));

            // 3. Golden Wheat Crops
            CreateSubMesh("WheatCrops1", CreateCubeMesh(new Vector3(1.2f, 0.35f, 3.0f)), GorkhaMaterials.GoldenWheat, root.transform, new Vector3(-0.9f, 0.55f, 0));
            CreateSubMesh("WheatCrops2", CreateCubeMesh(new Vector3(1.8f, 0.40f, 1.8f)), GorkhaMaterials.GoldenWheat, root.transform, new Vector3(0.5f, 1.10f, 0.5f));

            // 4. Farmer Thatch Hut
            CreateSubMesh("HutWalls", CreateCubeMesh(new Vector3(1.2f, 0.90f, 1.2f)), GorkhaMaterials.WhitePlaster, root.transform, new Vector3(-0.8f, 0.85f, -0.8f));
            CreateSubMesh("HutThatchedRoof", CreatePyramidMesh(1.5f, 0.65f), GorkhaMaterials.TerraceDirt, root.transform, new Vector3(-0.8f, 1.30f, -0.8f));

            // 5. Artwork Quad
            CreateSubMesh("FarmArtwork", CreateQuadMesh(3.2f, 3.2f), GorkhaMaterials.FarmSpriteMat, root.transform,
                new Vector3(0, 0.35f, 0), Quaternion.Euler(0, 45f, 0));

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.2f, 0);
            col.size = new Vector3(3.6f, 2.4f, 3.6f);

            return root;
        }

        /// <summary>
        /// Generates the Kot (Fortified Barracks / Armory):
        /// 3D heavy stone bastions, corner watch turrets, central courtyard, and weapons training rack.
        /// </summary>
        public static GameObject BuildBarracks(Transform parent = null)
        {
            GameObject root = new GameObject("Barracks_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            // 1. Heavy Stone Base
            CreateSubMesh("FortressBase", CreateCubeMesh(new Vector3(3.4f, 1.5f, 3.4f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.75f, 0));

            // 2. Corner Watch Turrets
            float tOff = 1.55f;
            Vector3[] turrets = {
                new Vector3(-tOff, 1.2f, -tOff), new Vector3(tOff, 1.2f, -tOff),
                new Vector3(tOff, 1.2f, tOff), new Vector3(-tOff, 1.2f, tOff)
            };
            foreach (var pos in turrets)
            {
                CreateSubMesh("CornerTurret", CreateCubeMesh(new Vector3(0.7f, 1.8f, 0.7f)), GorkhaMaterials.SlateStone, root.transform, pos);
                CreateSubMesh("TurretCap", CreatePyramidMesh(0.85f, 0.35f), GorkhaMaterials.PagodaWood, root.transform, pos + new Vector3(0, 0.95f, 0));
            }

            // 3. Central Training Courtyard & Kot Roof
            CreateSubMesh("KotCourtyard", CreateCubeMesh(new Vector3(2.0f, 0.90f, 2.0f)), GorkhaMaterials.RedBrick, root.transform, new Vector3(0, 1.95f, 0));
            CreateSubMesh("KotRoof", CreatePyramidMesh(2.5f, 0.55f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 2.40f, 0));

            // 4. Weapons Rack & Banner
            CreateSubMesh("WeaponRack", CreateCubeMesh(new Vector3(0.3f, 0.9f, 1.1f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.45f, 1.55f));
            CreateSubMesh("KhukuriStand", CreateCubeMesh(new Vector3(0.08f, 0.4f, 0.8f)), GorkhaMaterials.GoldenGajur, root.transform, new Vector3(0, 0.95f, 1.55f));

            // 5. Artwork Quad
            CreateSubMesh("BarracksArtwork", CreateQuadMesh(3.4f, 3.4f), GorkhaMaterials.BarracksSpriteMat, root.transform,
                new Vector3(0, 0.35f, 0), Quaternion.Euler(0, 45f, 0));

            // 6. Bastion Watch Torches
            GorkhaVFX.AttachTorchFlame(root.transform, new Vector3(-1.55f, 2.3f, -1.55f));
            GorkhaVFX.AttachTorchFlame(root.transform, new Vector3(1.55f, 2.3f, -1.55f));

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.6f, 0);
            col.size = new Vector3(3.6f, 3.2f, 3.6f);

            return root;
        }

        /// <summary>
        /// Generates the Woodcutter Camp:
        /// Sal wood log cabin, stacked timber cords, and chopping stump with embedded Khukuri.
        /// </summary>
        public static GameObject BuildWoodCamp(Transform parent = null)
        {
            GameObject root = new GameObject("WoodCamp_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("CabinPlinth", CreateCubeMesh(new Vector3(3.2f, 0.25f, 3.2f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.125f, 0));
            CreateSubMesh("TimberCabin", CreateCubeMesh(new Vector3(2.2f, 1.3f, 2.0f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(-0.4f, 0.75f, 0));
            CreateSubMesh("CabinRoof", CreatePyramidMesh(2.6f, 0.65f), GorkhaMaterials.ScaffoldingWood, root.transform, new Vector3(-0.4f, 1.40f, 0));

            // Stacked Sal wood logs
            CreateSubMesh("LogPile1", CreateCylinderMesh(0.20f, 1.6f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.9f, 0.25f, -0.5f), Quaternion.Euler(0, 0, 90));
            CreateSubMesh("LogPile2", CreateCylinderMesh(0.20f, 1.6f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.9f, 0.25f, 0.1f), Quaternion.Euler(0, 0, 90));
            CreateSubMesh("LogPile3", CreateCylinderMesh(0.20f, 1.4f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.9f, 0.55f, -0.2f), Quaternion.Euler(0, 0, 90));

            // Chopping Stump
            CreateSubMesh("ChoppingStump", CreateCylinderMesh(0.32f, 0.45f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.8f, 0.25f, 0.9f));

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.2f, 0);
            col.size = new Vector3(3.2f, 2.4f, 3.2f);

            return root;
        }

        /// <summary>
        /// Generates the Stone Quarry:
        /// Rocky excavation pit, slate blocks, and wooden crane.
        /// </summary>
        public static GameObject BuildQuarry(Transform parent = null)
        {
            GameObject root = new GameObject("StoneQuarry_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("QuarryPlinth", CreateCubeMesh(new Vector3(3.2f, 0.25f, 3.2f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.125f, 0));

            // Cut Slate Blocks
            CreateSubMesh("SlateBlock1", CreateCubeMesh(new Vector3(1.2f, 0.7f, 1.0f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(-0.7f, 0.45f, -0.6f));
            CreateSubMesh("SlateBlock2", CreateCubeMesh(new Vector3(1.0f, 0.5f, 0.8f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(-0.5f, 1.00f, -0.5f));
            CreateSubMesh("SlateBlock3", CreateCubeMesh(new Vector3(1.3f, 0.75f, 1.1f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0.7f, 0.50f, 0.5f));

            // Wooden Crane
            CreateSubMesh("CraneMast", CreateCylinderMesh(0.08f, 2.6f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.6f, 1.3f, -0.8f));
            CreateSubMesh("CraneArm", CreateCylinderMesh(0.06f, 1.8f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.6f, 2.4f, -0.2f), Quaternion.Euler(45, 0, 0));

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.3f, 0);
            col.size = new Vector3(3.2f, 2.6f, 3.2f);

            return root;
        }

        /// <summary>
        /// Generates a Traditional Newari House.
        /// </summary>
        public static GameObject BuildHouse(Transform parent = null)
        {
            GameObject root = new GameObject("House_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("HousePlinth", CreateCubeMesh(new Vector3(2.6f, 0.25f, 2.6f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.125f, 0));
            CreateSubMesh("HouseBrickCore", CreateCubeMesh(new Vector3(2.2f, 1.5f, 2.2f)), GorkhaMaterials.RedBrick, root.transform, new Vector3(0, 0.95f, 0));
            CreateSubMesh("HouseRoof", CreatePyramidMesh(2.6f, 0.65f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 1.70f, 0));

            CreateSubMesh("HouseArtwork", CreateQuadMesh(2.4f, 2.6f), GorkhaMaterials.HouseSpriteMat, root.transform,
                new Vector3(0, 0.35f, 0), Quaternion.Euler(0, 45f, 0));

            // House Cottage Chimney Smoke
            GorkhaVFX.AttachChimneySmoke(root.transform, new Vector3(0.55f, 2.2f, 0.35f));

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.2f, 0);
            col.size = new Vector3(2.6f, 2.4f, 2.6f);

            return root;
        }

        /// <summary>
        /// Generates a Mountain Watchtower.
        /// </summary>
        public static GameObject BuildWatchtower(Transform parent = null)
        {
            GameObject root = new GameObject("Watchtower_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("TowerPlinth", CreateCubeMesh(new Vector3(2.2f, 0.30f, 2.2f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.15f, 0));
            CreateSubMesh("TowerPillar", CreateCubeMesh(new Vector3(1.6f, 2.8f, 1.6f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 1.55f, 0));
            
            // Wooden Stilt Braces
            float sOff = 0.95f;
            Vector3[] stilts = {
                new Vector3(-sOff, 1.55f, -sOff), new Vector3(sOff, 1.55f, -sOff),
                new Vector3(sOff, 1.55f, sOff), new Vector3(-sOff, 1.55f, sOff)
            };
            foreach (var s in stilts)
            {
                CreateSubMesh("TimberStilt", CreateCubeMesh(new Vector3(0.18f, 2.8f, 0.18f)), GorkhaMaterials.PagodaWood, root.transform, s);
            }

            CreateSubMesh("ObservationDeck", CreateCubeMesh(new Vector3(2.4f, 0.30f, 2.4f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 3.05f, 0));
            CreateSubMesh("DeckParapet", CreateCubeMesh(new Vector3(2.35f, 0.45f, 2.35f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 3.35f, 0));
            CreateSubMesh("TowerRoof", CreatePyramidMesh(2.8f, 0.75f, flare: 0.25f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 4.05f, 0));
            CreateSubMesh("RoofPinnacle", CreateCylinderMesh(0.08f, 0.55f), GorkhaMaterials.GoldenGajur, root.transform, new Vector3(0, 4.65f, 0));

            // Watchtower Beacon Torches
            GorkhaVFX.AttachTorchFlame(root.transform, new Vector3(0.95f, 3.3f, 0.95f));
            GorkhaVFX.AttachTorchFlame(root.transform, new Vector3(-0.95f, 3.3f, -0.95f));

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 2.2f, 0);
            col.size = new Vector3(2.4f, 4.4f, 2.4f);

            return root;
        }

        /// <summary>
        /// Generates a Traditional Newari Elevated Granary (Bhakari/Dhukuti) (CoC Food Storage ECO-01).
        /// </summary>
        public static GameObject BuildGranary(Transform parent = null)
        {
            GameObject root = new GameObject("Granary_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("GranaryPlinth", CreateCubeMesh(new Vector3(2.8f, 0.25f, 2.8f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.125f, 0));

            float pOff = 1.15f;
            Vector3[] stilts = { new Vector3(-pOff, 0.55f, -pOff), new Vector3(pOff, 0.55f, -pOff), new Vector3(pOff, 0.55f, pOff), new Vector3(-pOff, 0.55f, pOff) };
            foreach (var s in stilts)
            {
                CreateSubMesh("TimberStilt", CreateCylinderMesh(0.12f, 0.60f), GorkhaMaterials.PagodaWood, root.transform, s);
            }

            CreateSubMesh("GrainChamber", CreateCubeMesh(new Vector3(2.4f, 1.4f, 2.4f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 1.55f, 0));
            CreateSubMesh("GranaryRoof", CreatePyramidMesh(3.0f, 0.75f, flare: 0.35f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 2.45f, 0));

            CreateSubMesh("GrainSack1", CreateCubeMesh(new Vector3(0.35f, 0.25f, 0.45f)), GorkhaMaterials.WheatField, root.transform, new Vector3(-0.55f, 0.95f, -1.05f), Quaternion.Euler(0, 15f, 0));
            CreateSubMesh("GrainSack2", CreateCubeMesh(new Vector3(0.32f, 0.22f, 0.42f)), GorkhaMaterials.WheatField, root.transform, new Vector3(0.55f, 0.95f, -1.05f), Quaternion.Euler(0, -20f, 0));

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.4f, 0);
            col.size = new Vector3(2.8f, 2.8f, 2.8f);

            return root;
        }

        /// <summary>
        /// Generates a Mountain Timber Yard & Lumber Stockpile (CoC Wood Storage ECO-01).
        /// </summary>
        public static GameObject BuildTimberYard(Transform parent = null)
        {
            GameObject root = new GameObject("TimberYard_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("TimberYardPlinth", CreateCubeMesh(new Vector3(3.0f, 0.20f, 3.0f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.10f, 0));

            float cOff = 1.25f;
            Vector3[] canopyPillars = { new Vector3(-cOff, 1.0f, -cOff), new Vector3(cOff, 1.0f, -cOff), new Vector3(cOff, 1.0f, cOff), new Vector3(-cOff, 1.0f, cOff) };
            foreach (var p in canopyPillars)
            {
                CreateSubMesh("CanopyPillar", CreateCylinderMesh(0.10f, 1.8f), GorkhaMaterials.PagodaWood, root.transform, p);
            }
            CreateSubMesh("CanopyRoof", CreatePyramidMesh(3.2f, 0.65f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 2.1f, 0));

            CreateSubMesh("LogStack1", CreateCylinderMesh(0.14f, 1.8f), GorkhaMaterials.SalWoodTimber, root.transform, new Vector3(-0.6f, 0.45f, 0), Quaternion.Euler(90f, 0, 0));
            CreateSubMesh("LogStack2", CreateCylinderMesh(0.14f, 1.8f), GorkhaMaterials.SalWoodTimber, root.transform, new Vector3(-0.35f, 0.45f, 0), Quaternion.Euler(90f, 0, 0));
            CreateSubMesh("LogStack3", CreateCylinderMesh(0.14f, 1.8f), GorkhaMaterials.SalWoodTimber, root.transform, new Vector3(-0.48f, 0.70f, 0), Quaternion.Euler(90f, 0, 0));

            CreateSubMesh("LogStack4", CreateCylinderMesh(0.13f, 1.6f), GorkhaMaterials.SalWoodTimber, root.transform, new Vector3(0.5f, 0.45f, 0), Quaternion.Euler(90f, 0, 0));
            CreateSubMesh("LogStack5", CreateCylinderMesh(0.13f, 1.6f), GorkhaMaterials.SalWoodTimber, root.transform, new Vector3(0.75f, 0.45f, 0), Quaternion.Euler(90f, 0, 0));

            CreateSubMesh("SawTrestle", CreateCubeMesh(new Vector3(0.18f, 0.55f, 0.85f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.45f, -1.0f));

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.2f, 0);
            col.size = new Vector3(3.0f, 2.4f, 3.0f);

            return root;
        }

        /// <summary>
        /// Generates a Mountain Slate Depot & Quarry Stockpile (CoC Stone Storage ECO-01).
        /// </summary>
        public static GameObject BuildSlateDepot(Transform parent = null)
        {
            GameObject root = new GameObject("SlateDepot_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("DepotPlinth", CreateCubeMesh(new Vector3(3.0f, 0.30f, 3.0f)), GorkhaMaterials.StoneSlate, root.transform, new Vector3(0, 0.15f, 0));

            CreateSubMesh("StoneBinL", CreateCubeMesh(new Vector3(0.85f, 0.75f, 2.2f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(-0.95f, 0.65f, 0));
            CreateSubMesh("StoneBinR", CreateCubeMesh(new Vector3(0.85f, 0.75f, 2.2f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0.95f, 0.65f, 0));

            CreateSubMesh("SlateStack1", CreateCubeMesh(new Vector3(0.65f, 0.60f, 0.65f)), GorkhaMaterials.StoneSlate, root.transform, new Vector3(-0.95f, 0.85f, -0.5f));
            CreateSubMesh("SlateStack2", CreateCubeMesh(new Vector3(0.65f, 0.75f, 0.65f)), GorkhaMaterials.StoneSlate, root.transform, new Vector3(0.95f, 0.90f, 0.5f));
            CreateSubMesh("CenterBlock", CreateCubeMesh(new Vector3(0.90f, 0.65f, 0.90f)), GorkhaMaterials.StoneSlate, root.transform, new Vector3(0, 0.60f, 0));

            CreateSubMesh("CraneMast", CreateCylinderMesh(0.10f, 2.4f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(1.1f, 1.4f, -1.1f));
            CreateSubMesh("CraneBoom", CreateCylinderMesh(0.08f, 1.6f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.6f, 2.5f, -0.6f), Quaternion.Euler(0, 45f, 35f));

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.3f, 0);
            col.size = new Vector3(3.0f, 2.6f, 3.0f);

            return root;
        }

        /// <summary>
        /// Generates a Fortified Royal Vault / Treasury (CoC Gold Storage ECO-01).
        /// </summary>
        public static GameObject BuildVault(Transform parent = null)
        {
            GameObject root = new GameObject("Vault_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("VaultPlinth", CreateCubeMesh(new Vector3(2.8f, 0.35f, 2.8f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.175f, 0));
            CreateSubMesh("VaultCore", CreateCubeMesh(new Vector3(2.2f, 1.8f, 2.2f)), GorkhaMaterials.TerracottaBrick, root.transform, new Vector3(0, 1.20f, 0));
            CreateSubMesh("VaultIronBands", CreateCubeMesh(new Vector3(2.24f, 0.15f, 2.24f)), GorkhaMaterials.DarkHammeredIron, root.transform, new Vector3(0, 1.15f, 0));

            CreateSubMesh("VaultRoof", CreatePyramidMesh(2.8f, 0.70f, flare: 0.30f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 2.25f, 0));
            CreateSubMesh("VaultGajur", CreatePyramidMesh(0.35f, 0.65f), GorkhaMaterials.MetallicGoldGajur, root.transform, new Vector3(0, 2.95f, 0));

            CreateSubMesh("VaultDoor", CreateCubeMesh(new Vector3(0.70f, 1.15f, 0.12f)), GorkhaMaterials.DarkHammeredIron, root.transform, new Vector3(0, 0.90f, -1.12f));
            CreateSubMesh("TreasureChest", CreateCubeMesh(new Vector3(0.50f, 0.35f, 0.35f)), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0.85f, 0.50f, -0.85f), Quaternion.Euler(0, -30f, 0));

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.5f, 0);
            col.size = new Vector3(2.8f, 3.0f, 2.8f);

            return root;
        }

        /// <summary>
        /// Generates dynamic connected fortification wall segments (CoC & AoE style).
        /// </summary>
        public static GameObject BuildWallSegment(Transform parent, int level, bool north, bool south, bool east, bool west)
        {
            GameObject root = new GameObject("WallSegment_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            Material wallMat;
            Material capMat;
            float wallHeight = 1.35f + (Mathf.Max(1, level) * 0.12f);

            switch (level)
            {
                case 1: // Bamboo & Sal Wood Stakes (Kandeghari)
                    wallMat = GorkhaMaterials.PagodaWood;
                    capMat = GorkhaMaterials.ScaffoldingWood;
                    break;
                case 2: // Terracotta Brick Ramparts (Mato Killa)
                    wallMat = GorkhaMaterials.RedBrick;
                    capMat = GorkhaMaterials.SlateStone;
                    break;
                case 3: // Dressed Himalayan Slate Stone (Dhungo Killa)
                    wallMat = GorkhaMaterials.SlateStone;
                    capMat = GorkhaMaterials.SlateStone;
                    break;
                default: // Lv 4+ Iron-Reinforced Bastion
                    wallMat = GorkhaMaterials.SlateStone;
                    capMat = GorkhaMaterials.GoldenGajur;
                    break;
            }

            // 1. Central Fortress Pillar Post
            CreateSubMesh("PillarBase", CreateCubeMesh(new Vector3(0.85f, wallHeight, 0.85f)), wallMat, root.transform, new Vector3(0, wallHeight * 0.5f, 0));
            CreateSubMesh("PillarCap", CreatePyramidMesh(0.95f, 0.30f), capMat, root.transform, new Vector3(0, wallHeight + 0.15f, 0));

            // 2. North Wall Curtain (+Z)
            if (north)
            {
                CreateSubMesh("Wall_North", CreateCubeMesh(new Vector3(0.65f, wallHeight * 0.90f, 1.0f)), wallMat, root.transform, new Vector3(0, wallHeight * 0.45f, 0.5f));
                CreateSubMesh("Crenel_North", CreateCubeMesh(new Vector3(0.68f, 0.25f, 0.45f)), capMat, root.transform, new Vector3(0, wallHeight * 0.90f + 0.125f, 0.75f));
            }

            // 3. South Wall Curtain (-Z)
            if (south)
            {
                CreateSubMesh("Wall_South", CreateCubeMesh(new Vector3(0.65f, wallHeight * 0.90f, 1.0f)), wallMat, root.transform, new Vector3(0, wallHeight * 0.45f, -0.5f));
                CreateSubMesh("Crenel_South", CreateCubeMesh(new Vector3(0.68f, 0.25f, 0.45f)), capMat, root.transform, new Vector3(0, wallHeight * 0.90f + 0.125f, -0.75f));
            }

            // 4. East Wall Curtain (+X)
            if (east)
            {
                CreateSubMesh("Wall_East", CreateCubeMesh(new Vector3(1.0f, wallHeight * 0.90f, 0.65f)), wallMat, root.transform, new Vector3(0.5f, wallHeight * 0.45f, 0));
                CreateSubMesh("Crenel_East", CreateCubeMesh(new Vector3(0.45f, 0.25f, 0.68f)), capMat, root.transform, new Vector3(0.75f, wallHeight * 0.90f + 0.125f, 0));
            }

            // 5. West Wall Curtain (-X)
            if (west)
            {
                CreateSubMesh("Wall_West", CreateCubeMesh(new Vector3(1.0f, wallHeight * 0.90f, 0.65f)), wallMat, root.transform, new Vector3(-0.5f, wallHeight * 0.45f, 0));
                CreateSubMesh("Crenel_West", CreateCubeMesh(new Vector3(0.45f, 0.25f, 0.68f)), capMat, root.transform, new Vector3(-0.75f, wallHeight * 0.90f + 0.125f, 0));
            }

            // BoxCollider for mouse selection
            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, wallHeight * 0.5f, 0);
            col.size = new Vector3(1.8f, wallHeight + 0.3f, 1.8f);

            return root;
        }

        /// <summary>
        /// Generates the Kaman (Himalayan Bronze Mountain Mortar Cannon).
        /// </summary>
        public static GameObject BuildKamanCannon(Transform parent = null, int level = 1)
        {
            GameObject root = new GameObject("KamanCannon_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            // 1. Stone Reinforcement Plinth
            CreateSubMesh("CannonPlinth", CreateCubeMesh(new Vector3(2.4f, 0.25f, 2.4f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.125f, 0));

            // 2. Heavy Timber Swivel Carriage
            CreateSubMesh("CarriageBase", CreateCubeMesh(new Vector3(1.6f, 0.40f, 1.4f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.45f, 0));
            CreateSubMesh("LeftMount", CreateCubeMesh(new Vector3(0.25f, 0.65f, 0.8f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(-0.65f, 0.80f, 0));
            CreateSubMesh("RightMount", CreateCubeMesh(new Vector3(0.25f, 0.65f, 0.8f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.65f, 0.80f, 0));

            // 3. Heavy Gorkhali Bronze Cannon Barrel (Elevated 25 degrees)
            Material barrelMat = level >= 2 ? GorkhaMaterials.GoldenGajur : GorkhaMaterials.SlateStone;
            GameObject barrel = CreateSubMesh("CannonBarrel", CreateCylinderMesh(0.28f, 1.7f), barrelMat, root.transform,
                new Vector3(0, 1.0f, 0.2f), Quaternion.Euler(65f, 0, 0));

            // 4. Muzzle Ring & Breech
            CreateSubMesh("MuzzleRing", CreateCylinderMesh(0.34f, 0.18f), barrelMat, barrel.transform, new Vector3(0, 0.80f, 0));
            CreateSubMesh("BreechBall", CreateCylinderMesh(0.32f, 0.22f), GorkhaMaterials.SlateStone, barrel.transform, new Vector3(0, -0.80f, 0));

            // 5. Stacked Cannonball Pyramid
            Vector3[] ballOffsets = {
                new Vector3(-0.6f, 0.40f, -0.7f), new Vector3(-0.35f, 0.40f, -0.7f),
                new Vector3(-0.6f, 0.40f, -0.45f), new Vector3(-0.48f, 0.62f, -0.58f)
            };
            foreach (var bPos in ballOffsets)
            {
                CreateSubMesh("CannonBall", CreateCylinderMesh(0.12f, 0.18f), GorkhaMaterials.SlateStone, root.transform, bPos);
            }

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.1f, 0);
            col.size = new Vector3(2.6f, 2.2f, 2.6f);

            return root;
        }

        public static GameObject BuildArrow(Transform parent = null)
        {
            GameObject root = new GameObject("PhysicalArrow_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("Shaft", CreateCylinderMesh(0.025f, 0.95f), GorkhaMaterials.PagodaWood, root.transform, Vector3.zero, Quaternion.Euler(90, 0, 0));
            CreateSubMesh("Tip", CreatePyramidMesh(0.08f, 0.18f), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0, 0.52f), Quaternion.Euler(-90, 0, 0));
            CreateSubMesh("Fletching", CreateDoubleTriangleFlagMesh(0.12f, 0.25f), GorkhaMaterials.RoyalRedFlag, root.transform, new Vector3(0, 0, -0.42f), Quaternion.Euler(0, 90, 90));

            return root;
        }

        public static GameObject BuildCannonball(Transform parent = null)
        {
            GameObject root = new GameObject("Cannonball_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("Core", CreateCylinderMesh(0.24f, 0.35f), GorkhaMaterials.SlateStone, root.transform);
            CreateSubMesh("Band", CreateCylinderMesh(0.26f, 0.10f), GorkhaMaterials.GoldenGajur, root.transform);

            return root;
        }

        public static GameObject BuildSpikeTrap(Transform parent = null)
        {
            GameObject root = new GameObject("SpikeTrap_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            // Sunken pit frame
            CreateSubMesh("PitFrame", CreateCubeMesh(new Vector3(1.7f, 0.08f, 1.7f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.04f, 0));
            CreateSubMesh("PitGravel", CreateCubeMesh(new Vector3(1.4f, 0.06f, 1.4f)), GorkhaMaterials.TerraceDirt, root.transform, new Vector3(0, 0.05f, 0));

            // Hidden Bamboo/Salwood Spikes
            GameObject spikes = new GameObject("SpikesGroup");
            spikes.transform.SetParent(root.transform, false);

            float[] coords = { -0.45f, 0f, 0.45f };
            foreach (var x in coords)
            {
                foreach (var z in coords)
                {
                    CreateSubMesh("Spike", CreatePyramidMesh(0.18f, 0.65f), GorkhaMaterials.PagodaWood, spikes.transform, new Vector3(x, 0.06f, z));
                    CreateSubMesh("SpikeTip", CreatePyramidMesh(0.08f, 0.20f), GorkhaMaterials.GoldenGajur, spikes.transform, new Vector3(x, 0.50f, z));
                }
            }

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.35f, 0);
            col.size = new Vector3(1.8f, 0.7f, 1.8f);

            return root;
        }

        /// <summary>
        /// Stylized Clash of Clans / Fantasy Kingdom floating harvest medallion.
        /// Beautiful circular coin badge facing the 45°/45° isometric camera.
        /// </summary>
        public static GameObject BuildHarvestMedallion(Transform parent = null, ResourceType type = ResourceType.Gold)
        {
            GameObject root = new GameObject("HarvestMedallion_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            root.transform.localRotation = Quaternion.Euler(45f, -45f, 0f);

            // 1. Outer golden coin rim
            CreateSubMesh("CoinRim", CreateCylinderMesh(0.42f, 0.05f), GorkhaMaterials.GoldenGajur, root.transform);

            // 2. Inner resource emblem plate
            Material emblemMat = type == ResourceType.Food ? GorkhaMaterials.GoldenWheat :
                                 type == ResourceType.Wood ? GorkhaMaterials.PagodaWood :
                                 type == ResourceType.Stone ? GorkhaMaterials.SlateStone :
                                 GorkhaMaterials.GoldenGajur;

            CreateSubMesh("InnerDisc", CreateCylinderMesh(0.35f, 0.07f), emblemMat, root.transform, new Vector3(0, 0.005f, 0));

            // 3. Center faceted jewel/star
            CreateSubMesh("CenterStar", CreatePyramidMesh(0.20f, 0.12f), GorkhaMaterials.WhitePlaster, root.transform, new Vector3(0, 0.04f, 0));

            return root;
        }

        /// <summary>
        /// Generates Scaffolding for in-progress construction.
        /// </summary>
        public static GameObject BuildScaffolding(Transform parent = null, Vector3 size = default)
        {
            if (size == default) size = new Vector3(3.2f, 2.5f, 3.2f);

            GameObject root = new GameObject("Scaffolding_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            float halfX = size.x * 0.45f;
            float halfZ = size.z * 0.45f;

            Vector3[] poles = {
                new Vector3(-halfX, size.y * 0.5f, -halfZ), new Vector3(halfX, size.y * 0.5f, -halfZ),
                new Vector3(halfX, size.y * 0.5f, halfZ), new Vector3(-halfX, size.y * 0.5f, halfZ)
            };
            foreach (var pos in poles)
            {
                CreateSubMesh("ScaffoldPole", CreateCubeMesh(new Vector3(0.12f, size.y, 0.12f)), GorkhaMaterials.ScaffoldingWood, root.transform, pos);
            }

            CreateSubMesh("Crossbeam1", CreateCubeMesh(new Vector3(size.x * 0.95f, 0.10f, 0.10f)), GorkhaMaterials.ScaffoldingWood, root.transform, new Vector3(0, size.y * 0.5f, halfZ));
            CreateSubMesh("Crossbeam2", CreateCubeMesh(new Vector3(size.x * 0.95f, 0.10f, 0.10f)), GorkhaMaterials.ScaffoldingWood, root.transform, new Vector3(0, size.y * 0.5f, -halfZ));
            CreateSubMesh("Crossbeam3", CreateCubeMesh(new Vector3(0.10f, 0.10f, size.z * 0.95f)), GorkhaMaterials.ScaffoldingWood, root.transform, new Vector3(halfX, size.y * 0.5f, 0));
            CreateSubMesh("Crossbeam4", CreateCubeMesh(new Vector3(0.10f, 0.10f, size.z * 0.95f)), GorkhaMaterials.ScaffoldingWood, root.transform, new Vector3(-halfX, size.y * 0.5f, 0));

            return root;
        }

        public static GameObject BuildPineTree(Transform parent = null)
        {
            GameObject root = new GameObject("PineTree_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("Trunk", CreateCylinderMesh(0.18f, 1.2f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.6f, 0));
            CreateSubMesh("FoliageTier1", CreatePyramidMesh(1.8f, 1.2f), GorkhaMaterials.AlpineGrass, root.transform, new Vector3(0, 1.0f, 0));
            CreateSubMesh("FoliageTier2", CreatePyramidMesh(1.4f, 1.1f), GorkhaMaterials.AlpineGrass, root.transform, new Vector3(0, 1.8f, 0));
            CreateSubMesh("FoliageTier3", CreatePyramidMesh(0.9f, 0.9f), GorkhaMaterials.AlpineGrass, root.transform, new Vector3(0, 2.5f, 0));

            return root;
        }

        public static GameObject BuildBoulder(Transform parent = null, float scale = 1.0f)
        {
            GameObject root = new GameObject("MountainBoulder_3D");
            if (parent != null) root.transform.SetParent(parent, false);

            CreateSubMesh("BoulderCore", CreateCubeMesh(new Vector3(1.2f * scale, 0.8f * scale, 1.0f * scale)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.4f * scale, 0), Quaternion.Euler(15, 35, 10));
            return root;
        }

        #region Authentic Nepali Weapons, Armor, Shields & Props

        /// <summary>
        /// Procedurally generates a true 3D wedge-profile Gorkha Khukuri blade with recurve belly,
        /// sharp cutting bevel edge, thick dorsal spine, and sacred Cho (Kaudi) notch geometry.
        /// </summary>
        public static Mesh CreateKhukuriBladeMesh(bool isBandit = false)
        {
            Mesh mesh = new Mesh { name = "KhukuriBladeMesh" };

            // Stations along blade from hilt to tip: (Y height, Z forward offset, spine width, blade belly depth)
            float[] stationY = { 0.00f, 0.04f, 0.09f, 0.17f, 0.25f, 0.32f, 0.38f, 0.43f };
            float[] stationZ = { 0.00f, 0.004f, 0.012f, 0.030f, 0.048f, 0.055f, 0.042f, 0.008f };
            float[] spineW   = { 0.014f, 0.013f, 0.012f, 0.011f, 0.010f, 0.009f, 0.007f, 0.003f };
            float[] depthD   = { 0.038f, 0.040f, 0.045f, 0.065f, 0.082f, 0.085f, 0.062f, 0.014f };

            int stations = stationY.Length;
            int vertexCount = stations * 3 + 1; // +1 for the sharp apex tip
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];

            for (int i = 0; i < stations; i++)
            {
                float y = stationY[i];
                float z = stationZ[i];
                float hw = spineW[i] * 0.5f;
                float d = depthD[i];

                int baseIdx = i * 3;
                // Dorsal spine left & right
                vertices[baseIdx + 0] = new Vector3(-hw, y, z);
                vertices[baseIdx + 1] = new Vector3( hw, y, z);
                // Ventral razor edge (protruding forward in +Z relative to spine)
                vertices[baseIdx + 2] = new Vector3(0f, y - 0.01f, z + d);

                float v = (float)i / (stations - 1);
                uvs[baseIdx + 0] = new Vector2(0f, v);
                uvs[baseIdx + 1] = new Vector2(1f, v);
                uvs[baseIdx + 2] = new Vector2(0.5f, v);
            }

            // Apex tip point
            int tipIdx = stations * 3;
            vertices[tipIdx] = new Vector3(0, stationY[stations - 1] + 0.02f, stationZ[stations - 1] + 0.01f);
            uvs[tipIdx] = new Vector2(0.5f, 1f);

            List<int> triangles = new List<int>();
            for (int i = 0; i < stations - 1; i++)
            {
                int b0 = i * 3;
                int b1 = (i + 1) * 3;

                // Spine quad
                triangles.Add(b0 + 0); triangles.Add(b1 + 0); triangles.Add(b1 + 1);
                triangles.Add(b0 + 0); triangles.Add(b1 + 1); triangles.Add(b0 + 1);

                // Left bevel cutting flank
                triangles.Add(b0 + 0); triangles.Add(b0 + 2); triangles.Add(b1 + 2);
                triangles.Add(b0 + 0); triangles.Add(b1 + 2); triangles.Add(b1 + 0);

                // Right bevel cutting flank
                triangles.Add(b0 + 1); triangles.Add(b1 + 1); triangles.Add(b1 + 2);
                triangles.Add(b0 + 1); triangles.Add(b1 + 2); triangles.Add(b0 + 2);
            }

            // Cap the last station to the apex tip
            int lastStation = (stations - 1) * 3;
            triangles.Add(lastStation + 0); triangles.Add(tipIdx); triangles.Add(lastStation + 1);
            triangles.Add(lastStation + 0); triangles.Add(lastStation + 2); triangles.Add(tipIdx);
            triangles.Add(lastStation + 1); triangles.Add(tipIdx); triangles.Add(lastStation + 2);

            // Cap the hilt base
            triangles.Add(0); triangles.Add(1); triangles.Add(2);

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Procedurally generates an elongated diamond-cross-section Gorkha leaf spearhead (Sangram Barchha).
        /// Features twin cutting edges and a reinforced central diamond rib.
        /// </summary>
        public static Mesh CreateSpearheadMesh()
        {
            Mesh mesh = new Mesh { name = "SpearheadMesh" };

            // 5 Stations along leaf spearhead: (Y height, Half-Width, Half-Thickness)
            float[] stationY = { 0.00f, 0.08f, 0.20f, 0.32f, 0.42f };
            float[] halfW    = { 0.015f, 0.038f, 0.045f, 0.026f, 0.002f };
            float[] halfT    = { 0.014f, 0.012f, 0.010f, 0.006f, 0.002f };

            int stations = stationY.Length;
            Vector3[] vertices = new Vector3[stations * 4];
            Vector2[] uvs = new Vector2[stations * 4];

            for (int i = 0; i < stations; i++)
            {
                float y = stationY[i];
                float w = halfW[i];
                float t = halfT[i];
                int b = i * 4;

                vertices[b + 0] = new Vector3(-w, y, 0);   // Left cutting edge
                vertices[b + 1] = new Vector3( w, y, 0);   // Right cutting edge
                vertices[b + 2] = new Vector3(0, y,  t);   // Front central rib
                vertices[b + 3] = new Vector3(0, y, -t);   // Back central rib

                float v = (float)i / (stations - 1);
                uvs[b + 0] = new Vector2(0f, v);
                uvs[b + 1] = new Vector2(1f, v);
                uvs[b + 2] = new Vector2(0.5f, v);
                uvs[b + 3] = new Vector2(0.5f, v);
            }

            List<int> triangles = new List<int>();
            for (int i = 0; i < stations - 1; i++)
            {
                int b0 = i * 4;
                int b1 = (i + 1) * 4;

                // Front-Left facet
                triangles.Add(b0 + 0); triangles.Add(b1 + 0); triangles.Add(b1 + 2);
                triangles.Add(b0 + 0); triangles.Add(b1 + 2); triangles.Add(b0 + 2);

                // Front-Right facet
                triangles.Add(b0 + 2); triangles.Add(b1 + 2); triangles.Add(b1 + 1);
                triangles.Add(b0 + 2); triangles.Add(b1 + 1); triangles.Add(b0 + 1);

                // Back-Right facet
                triangles.Add(b0 + 1); triangles.Add(b1 + 1); triangles.Add(b1 + 3);
                triangles.Add(b0 + 1); triangles.Add(b1 + 3); triangles.Add(b0 + 3);

                // Back-Left facet
                triangles.Add(b0 + 3); triangles.Add(b1 + 3); triangles.Add(b1 + 0);
                triangles.Add(b0 + 3); triangles.Add(b1 + 0); triangles.Add(b0 + 0);
            }

            // Bottom base cap
            triangles.Add(0); triangles.Add(2); triangles.Add(1);
            triangles.Add(0); triangles.Add(1); triangles.Add(3);

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Procedurally generates an authentic Nepali Doko (woven bamboo wicker mountain basket).
        /// Inverted truncated trapezoid with reinforced square base, flared woven open rim, and interior space.
        /// </summary>
        public static Mesh CreateDokoBasketMesh(float baseWidth = 0.22f, float topWidth = 0.44f, float height = 0.50f)
        {
            Mesh mesh = new Mesh { name = "DokoBasketMesh" };

            float hb = baseWidth * 0.5f;
            float ht = topWidth * 0.5f;
            float wall = 0.02f;
            float hbi = hb - wall;
            float hti = ht - wall;

            Vector3[] vertices = new Vector3[]
            {
                // Outer Bottom (0-3)
                new Vector3(-hb, 0, -hb), new Vector3( hb, 0, -hb), new Vector3( hb, 0,  hb), new Vector3(-hb, 0,  hb),
                // Outer Top (4-7)
                new Vector3(-ht, height, -ht), new Vector3( ht, height, -ht), new Vector3( ht, height,  ht), new Vector3(-ht, height,  ht),
                // Inner Bottom (8-11)
                new Vector3(-hbi, wall, -hbi), new Vector3( hbi, wall, -hbi), new Vector3( hbi, wall,  hbi), new Vector3(-hbi, wall,  hbi),
                // Inner Top (12-15)
                new Vector3(-hti, height, -hti), new Vector3( hti, height, -hti), new Vector3( hti, height,  hti), new Vector3(-hti, height,  hti)
            };

            List<int> triangles = new List<int>();

            // Outer 4 sides
            int[] sides = { 0, 1, 5, 4,   1, 2, 6, 5,   2, 3, 7, 6,   3, 0, 4, 7 };
            for (int s = 0; s < 4; s++)
            {
                int i = s * 4;
                triangles.Add(sides[i]); triangles.Add(sides[i + 3]); triangles.Add(sides[i + 2]);
                triangles.Add(sides[i]); triangles.Add(sides[i + 2]); triangles.Add(sides[i + 1]);
            }

            // Outer bottom
            triangles.Add(0); triangles.Add(1); triangles.Add(2);
            triangles.Add(0); triangles.Add(2); triangles.Add(3);

            // Rim quads (linking outer top to inner top)
            for (int s = 0; s < 4; s++)
            {
                int o0 = 4 + s;
                int o1 = 4 + (s + 1) % 4;
                int in0 = 12 + s;
                int in1 = 12 + (s + 1) % 4;

                triangles.Add(o0); triangles.Add(o1); triangles.Add(in1);
                triangles.Add(o0); triangles.Add(in1); triangles.Add(in0);
            }

            // Inner 4 sides
            for (int s = 0; s < 4; s++)
            {
                int b0 = 8 + s;
                int b1 = 8 + (s + 1) % 4;
                int t0 = 12 + s;
                int t1 = 12 + (s + 1) % 4;

                triangles.Add(b0); triangles.Add(b1); triangles.Add(t1);
                triangles.Add(b0); triangles.Add(t1); triangles.Add(t0);
            }

            // Inner floor
            triangles.Add(8); triangles.Add(10); triangles.Add(9);
            triangles.Add(8); triangles.Add(11); triangles.Add(10);

            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                uvs[i] = new Vector2(vertices[i].x * 3.5f, vertices[i].y * 3.5f);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Procedurally generates an authentic faceted Nepali Dhaka Topi with asymmetrical sloping Himalayan peak.
        /// </summary>
        public static Mesh CreateDhakaTopiMesh(float radius = 0.17f, float heightLeft = 0.15f, float heightRight = 0.22f)
        {
            Mesh mesh = new Mesh { name = "DhakaTopiMesh" };

            int segments = 8;
            Vector3[] vertices = new Vector3[segments * 2 + 1];
            Vector2[] uvs = new Vector2[vertices.Length];

            for (int i = 0; i < segments; i++)
            {
                float angle = (i * 360f / segments) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * (radius * 1.08f);

                vertices[i] = new Vector3(x, 0, z);
                uvs[i] = new Vector2((float)i / segments, 0);

                // Top rim slope from left to right across X
                float t = Mathf.InverseLerp(-radius, radius, x);
                float h = Mathf.Lerp(heightLeft, heightRight, t);

                vertices[segments + i] = new Vector3(x * 0.90f, h, z * 0.90f);
                uvs[segments + i] = new Vector2((float)i / segments, 1);
            }

            // Apex center vertex for top crown
            int centerIdx = segments * 2;
            vertices[centerIdx] = new Vector3(radius * 0.20f, (heightLeft + heightRight) * 0.54f, 0);
            uvs[centerIdx] = new Vector2(0.5f, 0.5f);

            List<int> triangles = new List<int>();
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;

                // Wall quads
                triangles.Add(i); triangles.Add(segments + i); triangles.Add(segments + next);
                triangles.Add(i); triangles.Add(segments + next); triangles.Add(next);

                // Top crown fan
                triangles.Add(segments + i); triangles.Add(centerIdx); triangles.Add(segments + next);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static GameObject BuildKhukuriBlade(Transform parent, Vector3 localPos = default, Quaternion localRot = default, bool isBandit = false)
        {
            // 1. Try High-Fidelity Blender Historical FBX Asset
            GameObject fbxPrefab = Resources.Load<GameObject>("GorkhaWeapons/Gorkha_Khukuri_Historical");
            if (fbxPrefab != null)
            {
                GameObject fbxInstance = UnityEngine.Object.Instantiate(fbxPrefab, parent);
                fbxInstance.name = "Weapon_Khukuri";
                fbxInstance.transform.localPosition = localPos;
                fbxInstance.transform.localRotation = localRot == default ? Quaternion.identity : localRot;
                fbxInstance.transform.localScale = Vector3.one * 1.0f;
                foreach (var c in fbxInstance.GetComponentsInChildren<Collider>())
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(c);
                    else UnityEngine.Object.DestroyImmediate(c);
                }
                if (isBandit)
                {
                    foreach (var r in fbxInstance.GetComponentsInChildren<Renderer>())
                    {
                        r.material = GorkhaMaterials.DarkIron;
                    }
                }
                return fbxInstance;
            }

            // Fallback: Procedural Geometry
            GameObject root = new GameObject("Weapon_Khukuri");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            Material bladeMat = isBandit ? GorkhaMaterials.DarkIron : GorkhaMaterials.ForgedSteel;
            Material metalTrimMat = isBandit ? GorkhaMaterials.DarkIron : GorkhaMaterials.PolishedBrass;

            // 1. Bolster & Hilt
            CreateSubMesh("Grip", CreateCylinderMesh(0.030f, 0.16f), GorkhaMaterials.SalWoodDark, root.transform);
            CreateSubMesh("GripRidge", CreateCylinderMesh(0.034f, 0.03f), GorkhaMaterials.SalWoodDark, root.transform, new Vector3(0, 0, 0));
            CreateSubMesh("Bolster", CreateCylinderMesh(0.042f, 0.030f), metalTrimMat, root.transform, new Vector3(0, 0.08f, 0));
            CreateSubMesh("Pommel", CreatePyramidMesh(0.068f, 0.04f), metalTrimMat, root.transform, new Vector3(0, -0.08f, 0), Quaternion.Euler(180, 0, 0));

            // 2. Sculpted Recurve Steel Blade
            CreateSubMesh("RecurveBlade", CreateKhukuriBladeMesh(isBandit), bladeMat, root.transform, new Vector3(0, 0.095f, 0));

            // 3. Sacred Cho (Kaudi) Notch at blade heel
            CreateSubMesh("ChoNotch", CreateCylinderMesh(0.016f, 0.025f), metalTrimMat, root.transform, new Vector3(0, 0.11f, 0.025f));

            return root;
        }

        public static GameObject BuildKhukuriScabbard(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Armor_KhukuriDap");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Curved Leather Scabbard Body
            CreateSubMesh("ScabbardBody", CreateCubeMesh(new Vector3(0.045f, 0.42f, 0.09f)), GorkhaMaterials.LeatherPatuka, root.transform, new Vector3(0, -0.16f, 0.02f), Quaternion.Euler(0, 0, 6f));
            // Brass Protective Chape (Tip)
            CreateSubMesh("Chape", CreatePyramidMesh(0.08f, 0.10f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0.03f, -0.38f, 0.03f), Quaternion.Euler(180, 0, 0));
            // Brass Throat Collar
            CreateSubMesh("Throat", CreateCubeMesh(new Vector3(0.052f, 0.04f, 0.10f)), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(-0.01f, 0.03f, 0.01f));

            // Small Handles for Karda & Chakmak tool knives nestled behind throat
            CreateSubMesh("KardaHilt", CreateCylinderMesh(0.016f, 0.08f), GorkhaMaterials.SalWoodDark, root.transform, new Vector3(-0.02f, 0.06f, 0.03f), Quaternion.Euler(15f, 0, 0));
            CreateSubMesh("ChakmakHilt", CreateCylinderMesh(0.016f, 0.08f), GorkhaMaterials.SalWoodDark, root.transform, new Vector3(-0.02f, 0.06f, -0.01f), Quaternion.Euler(15f, 0, 0));

            return root;
        }

        public static GameObject BuildSpear(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            // 1. Try High-Fidelity Blender Historical FBX Asset
            GameObject fbxPrefab = Resources.Load<GameObject>("GorkhaWeapons/Gorkha_Bhala_Spear");
            if (fbxPrefab != null)
            {
                GameObject fbxInstance = UnityEngine.Object.Instantiate(fbxPrefab, parent);
                fbxInstance.name = "Weapon_Spear";
                fbxInstance.transform.localPosition = localPos;
                fbxInstance.transform.localRotation = localRot == default ? Quaternion.identity : localRot;
                fbxInstance.transform.localScale = Vector3.one * 1.0f;
                foreach (var c in fbxInstance.GetComponentsInChildren<Collider>())
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(c);
                    else UnityEngine.Object.DestroyImmediate(c);
                }
                return fbxInstance;
            }

            GameObject root = new GameObject("Weapon_Spear");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // 2.35m Himalayan Salwood Shaft
            CreateSubMesh("Shaft", CreateCylinderMesh(0.028f, 2.35f), GorkhaMaterials.SalWoodDark, root.transform, new Vector3(0, 0.85f, 0));
            CreateSubMesh("HeelSpike", CreatePyramidMesh(0.055f, 0.14f), GorkhaMaterials.DarkIron, root.transform, new Vector3(0, -0.35f, 0), Quaternion.Euler(180, 0, 0));

            // Polished Brass Collar Socket
            CreateSubMesh("Collar", CreateCylinderMesh(0.048f, 0.12f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, 2.02f, 0));
            CreateSubMesh("CollarRing", CreateCylinderMesh(0.054f, 0.03f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, 1.98f, 0));

            // 3D Diamond Cross-Section Leaf Spearhead
            CreateSubMesh("Spearhead", CreateSpearheadMesh(), GorkhaMaterials.ForgedSteel, root.transform, new Vector3(0, 2.08f, 0));

            // Dynamic Crimson Horsehair Tassel (Chamar)
            CreateSubMesh("HorsehairPlume", CreateDoubleTriangleFlagMesh(0.14f, 0.32f), GorkhaMaterials.RoyalRedFlag, root.transform, new Vector3(0.05f, 1.95f, 0), Quaternion.Euler(0, 90, 0));
            CreateSubMesh("PlumeBack", CreateDoubleTriangleFlagMesh(0.14f, 0.32f), GorkhaMaterials.RoyalRedFlag, root.transform, new Vector3(-0.05f, 1.95f, 0), Quaternion.Euler(0, -90, 0));

            return root;
        }

        public static GameObject BuildRecurveBow(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            // 1. Try High-Fidelity Blender Historical FBX Asset
            GameObject fbxPrefab = Resources.Load<GameObject>("GorkhaWeapons/Gorkha_Recurve_Bow");
            if (fbxPrefab != null)
            {
                GameObject fbxInstance = UnityEngine.Object.Instantiate(fbxPrefab, parent);
                fbxInstance.name = "Weapon_RecurveBow";
                fbxInstance.transform.localPosition = localPos;
                fbxInstance.transform.localRotation = localRot == default ? Quaternion.identity : localRot;
                fbxInstance.transform.localScale = Vector3.one * 1.0f;
                foreach (var c in fbxInstance.GetComponentsInChildren<Collider>())
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(c);
                    else UnityEngine.Object.DestroyImmediate(c);
                }
                return fbxInstance;
            }

            GameObject root = new GameObject("Weapon_RecurveBow");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Center Rawhide Leather Handle
            CreateSubMesh("Grip", CreateCubeMesh(new Vector3(0.045f, 0.18f, 0.055f)), GorkhaMaterials.RawhideLeather, root.transform);

            // Upper Limbs (Reflex curve)
            CreateSubMesh("LimbTop1", CreateCubeMesh(new Vector3(0.032f, 0.38f, 0.035f)), GorkhaMaterials.SalWoodDark, root.transform, new Vector3(0, 0.26f, -0.06f), Quaternion.Euler(-18, 0, 0));
            CreateSubMesh("LimbTop2", CreateCubeMesh(new Vector3(0.026f, 0.22f, 0.030f)), GorkhaMaterials.SalWoodDark, root.transform, new Vector3(0, 0.52f, 0.01f), Quaternion.Euler(26, 0, 0));
            CreateSubMesh("SiyahTop", CreateCubeMesh(new Vector3(0.022f, 0.10f, 0.025f)), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, 0.64f, 0.07f), Quaternion.Euler(40, 0, 0));

            // Lower Limbs (Reflex curve)
            CreateSubMesh("LimbBottom1", CreateCubeMesh(new Vector3(0.032f, 0.38f, 0.035f)), GorkhaMaterials.SalWoodDark, root.transform, new Vector3(0, -0.26f, -0.06f), Quaternion.Euler(18, 0, 0));
            CreateSubMesh("LimbBottom2", CreateCubeMesh(new Vector3(0.026f, 0.22f, 0.030f)), GorkhaMaterials.SalWoodDark, root.transform, new Vector3(0, -0.52f, 0.01f), Quaternion.Euler(-26, 0, 0));
            CreateSubMesh("SiyahBottom", CreateCubeMesh(new Vector3(0.022f, 0.10f, 0.025f)), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, -0.64f, 0.07f), Quaternion.Euler(-40, 0, 0));

            // Taut Bowstring with Nocking Point
            CreateSubMesh("Bowstring", CreateCylinderMesh(0.008f, 1.28f), GorkhaMaterials.WhitePlaster, root.transform, new Vector3(0, 0, -0.14f));
            CreateSubMesh("NockBead", CreateCylinderMesh(0.018f, 0.03f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, 0, -0.14f));

            return root;
        }

        public static GameObject BuildQuiver(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Armory_Quiver");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Rawhide Leather Tube
            CreateSubMesh("QuiverBody", CreateCylinderMesh(0.10f, 0.62f), GorkhaMaterials.RawhideLeather, root.transform);
            CreateSubMesh("GoldRim", CreateCylinderMesh(0.11f, 0.05f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, 0.28f, 0));
            CreateSubMesh("BaseCap", CreateCylinderMesh(0.11f, 0.05f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, -0.28f, 0));

            // 4 Fletched Bamboo Arrows with Bodkin Heads
            Vector3[] arrowOffsets = {
                new Vector3(-0.03f, 0.38f,  0.01f),
                new Vector3( 0.03f, 0.40f,  0.02f),
                new Vector3( 0.00f, 0.44f, -0.03f),
                new Vector3( 0.02f, 0.36f, -0.02f)
            };
            foreach (var off in arrowOffsets)
            {
                CreateSubMesh("Shaft", CreateCylinderMesh(0.016f, 0.32f), GorkhaMaterials.SalWoodDark, root.transform, off);
                CreateSubMesh("Fletch", CreateDoubleTriangleFlagMesh(0.06f, 0.14f), GorkhaMaterials.RoyalRedFlag, root.transform, off + new Vector3(0, 0.12f, 0), Quaternion.Euler(0, 90, 0));
                CreateSubMesh("ArrowHead", CreatePyramidMesh(0.025f, 0.06f), GorkhaMaterials.ForgedSteel, root.transform, off + new Vector3(0, -0.20f, 0));
            }

            return root;
        }

        public static GameObject BuildBucklerShield(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            // 1. Try High-Fidelity Blender Historical FBX Asset
            GameObject fbxPrefab = Resources.Load<GameObject>("GorkhaWeapons/Gorkha_Dhal_Shield");
            if (fbxPrefab != null)
            {
                GameObject fbxInstance = UnityEngine.Object.Instantiate(fbxPrefab, parent);
                fbxInstance.name = "Armor_BucklerShield";
                fbxInstance.transform.localPosition = localPos;
                fbxInstance.transform.localRotation = localRot == default ? Quaternion.identity : localRot;
                fbxInstance.transform.localScale = Vector3.one * 1.0f;
                foreach (var c in fbxInstance.GetComponentsInChildren<Collider>())
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(c);
                    else UnityEngine.Object.DestroyImmediate(c);
                }
                return fbxInstance;
            }

            GameObject root = new GameObject("Armor_BucklerShield");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Convex Hardwood Plate with Polished Brass Rim
            CreateSubMesh("WoodCore", CreateCylinderMesh(0.32f, 0.045f), GorkhaMaterials.PagodaWood, root.transform);
            CreateSubMesh("BrassRim", CreateCylinderMesh(0.35f, 0.028f), GorkhaMaterials.PolishedBrass, root.transform);

            // 8 Radial Reinforcing Iron Studs
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                Vector3 p = new Vector3(Mathf.Cos(a) * 0.12f, Mathf.Sin(a) * 0.12f, 0.025f);
                CreateSubMesh($"Stud_{i}", CreatePyramidMesh(0.035f, 0.035f), GorkhaMaterials.DarkIron, root.transform, p);
            }

            // Central Raised Conical Sun Boss
            CreateSubMesh("CenterBoss", CreatePyramidMesh(0.14f, 0.12f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, 0, 0.04f), Quaternion.Euler(90f, 0, 0));

            return root;
        }

        public static GameObject BuildPaviseShield(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Armor_PaviseShield");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Heavy Infantry Lacquered Shield Face
            CreateSubMesh("ShieldFace", CreateCubeMesh(new Vector3(0.48f, 0.96f, 0.055f)), GorkhaMaterials.RoyalRedFlag, root.transform);
            CreateSubMesh("SpineRidge", CreateCubeMesh(new Vector3(0.07f, 1.00f, 0.09f)), GorkhaMaterials.PagodaWood, root.transform);
            CreateSubMesh("TopTrim", CreateCubeMesh(new Vector3(0.50f, 0.06f, 0.075f)), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, 0.47f, 0));
            CreateSubMesh("BottomTrim", CreateCubeMesh(new Vector3(0.50f, 0.06f, 0.075f)), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, -0.47f, 0));

            // Central Nepali Double-Sun Crest Motif
            CreateSubMesh("SunEmblem", CreatePyramidMesh(0.16f, 0.08f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, 0.12f, 0.045f), Quaternion.Euler(90f, 0, 0));
            CreateSubMesh("MoonEmblem", CreatePyramidMesh(0.12f, 0.06f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, -0.12f, 0.045f), Quaternion.Euler(90f, 0, 0));

            return root;
        }

        public static GameObject BuildLamellarPauldrons(Transform shoulderL, Transform shoulderR, bool isHostile = false)
        {
            GameObject root = new GameObject("Armor_Pauldrons");
            Material plateMat = isHostile ? GorkhaMaterials.DarkIron : GorkhaMaterials.PolishedBrass;

            void AttachPauldron(Transform shoulder, bool isLeft)
            {
                if (shoulder == null) return;
                float sign = isLeft ? -1f : 1f;

                for (int tier = 0; tier < 3; tier++)
                {
                    float yOff = -tier * 0.06f;
                    float scaleW = 0.16f - tier * 0.015f;
                    float xOff = sign * (0.04f + tier * 0.015f);

                    CreateSubMesh($"Plate_{tier}", CreateCubeMesh(new Vector3(scaleW, 0.035f, 0.14f)), plateMat, shoulder, new Vector3(xOff, yOff, 0), Quaternion.Euler(0, 0, sign * (15f + tier * 8f)));
                }
            }

            AttachPauldron(shoulderL, true);
            AttachPauldron(shoulderR, false);
            return root;
        }

        public static GameObject BuildDhakaTopi(Transform headBone, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Headwear_DhakaTopi");
            if (headBone != null) root.transform.SetParent(headBone, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Sculpted Asymmetrical Dhaka Topi with authentic geometric weave
            CreateSubMesh("DhakaBody", CreateDhakaTopiMesh(0.17f, 0.15f, 0.22f), GorkhaMaterials.DhakaFabric, root.transform);

            // Golden Sun & Crossed Khukuri Insignia Pin on front-left
            CreateSubMesh("SunPin", CreatePyramidMesh(0.045f, 0.035f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0.07f, 0.12f, 0.16f), Quaternion.Euler(-35f, 25f, 0));

            return root;
        }

        public static GameObject BuildGorkhaHelmet(Transform headBone, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Headwear_GorkhaHelmet");
            if (headBone != null) root.transform.SetParent(headBone, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Conical Bronze Crown with Neck Shield
            CreateSubMesh("HelmetRim", CreateCylinderMesh(0.33f, 0.08f), GorkhaMaterials.DarkIron, root.transform, new Vector3(0, 0.04f, 0.01f));
            CreateSubMesh("HelmetNeckGuard", CreateCubeMesh(new Vector3(0.32f, 0.12f, 0.05f)), GorkhaMaterials.DarkIron, root.transform, new Vector3(0, -0.02f, -0.14f), Quaternion.Euler(15f, 0, 0));
            CreateSubMesh("HelmetDome", CreatePyramidMesh(0.34f, 0.28f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, 0.14f, 0.01f));

            // Golden Gajur Spire Finial
            CreateSubMesh("GajurSpire", CreatePyramidMesh(0.08f, 0.14f), GorkhaMaterials.GoldenGajur, root.transform, new Vector3(0, 0.28f, 0.01f));

            // Cheek Guards
            CreateSubMesh("CheekL", CreateCubeMesh(new Vector3(0.03f, 0.12f, 0.08f)), GorkhaMaterials.DarkIron, root.transform, new Vector3(-0.15f, -0.04f, 0.04f), Quaternion.Euler(0, 0, -8f));
            CreateSubMesh("CheekR", CreateCubeMesh(new Vector3(0.03f, 0.12f, 0.08f)), GorkhaMaterials.DarkIron, root.transform, new Vector3(0.15f, -0.04f, 0.04f), Quaternion.Euler(0, 0, 8f));

            return root;
        }

        public static GameObject BuildStrawGhum(Transform headBone, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Headwear_StrawGhum");
            if (headBone != null) root.transform.SetParent(headBone, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Wide-Brim Woven Bamboo Conical Sun Hat (Ghum)
            CreateSubMesh("GhumCone", CreatePyramidMesh(0.65f, 0.18f), GorkhaMaterials.WovenBambooDoko, root.transform);
            CreateSubMesh("GhumRim", CreateCylinderMesh(0.66f, 0.025f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.01f, 0));

            return root;
        }

        public static GameObject BuildDokoBasket(Transform backSocket, Transform headBone = null)
        {
            GameObject root = new GameObject("Villager_Doko");
            if (backSocket != null) root.transform.SetParent(backSocket, false);
            root.transform.localPosition = new Vector3(0, 0.05f, -0.22f);
            root.transform.localRotation = Quaternion.Euler(-12f, 0, 0);

            // Woven Bamboo Wicker Basket
            CreateSubMesh("DokoMesh", CreateDokoBasketMesh(0.24f, 0.44f, 0.50f), GorkhaMaterials.WovenBambooDoko, root.transform);
            // Sturdy Wooden Base & Rim Corner Posts
            CreateSubMesh("BasePlate", CreateCubeMesh(new Vector3(0.25f, 0.03f, 0.25f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.015f, 0));

            // Namlo Headstrap (Woven tumpline band passing around forehead)
            if (headBone != null)
            {
                CreateSubMesh("NamloStrap", CreateCubeMesh(new Vector3(0.34f, 0.06f, 0.05f)), GorkhaMaterials.WovenBambooDoko, headBone, new Vector3(0, 0.09f, 0.14f), Quaternion.Euler(15f, 0, 0));
            }

            // Dynamic Resource Payload (Toggled by UnitController.UpdateSackVisual)
            GameObject payload = new GameObject("DokoPayload");
            payload.transform.SetParent(root.transform, false);
            payload.transform.localPosition = new Vector3(0, 0.28f, 0);

            // 1. Wood logs group
            GameObject woodGroup = new GameObject("Payload_Wood");
            woodGroup.transform.SetParent(payload.transform, false);
            CreateSubMesh("TimberLog1", CreateCylinderMesh(0.08f, 0.40f), GorkhaMaterials.PagodaWood, woodGroup.transform, new Vector3(-0.06f, 0, 0), Quaternion.Euler(75f, 0, 0));
            CreateSubMesh("TimberLog2", CreateCylinderMesh(0.08f, 0.42f), GorkhaMaterials.PagodaWood, woodGroup.transform, new Vector3( 0.06f, 0, 0), Quaternion.Euler(80f, 15f, 0));
            CreateSubMesh("TimberLog3", CreateCylinderMesh(0.07f, 0.38f), GorkhaMaterials.PagodaWood, woodGroup.transform, new Vector3( 0f, 0.08f, 0.02f), Quaternion.Euler(70f, -10f, 0));

            // 2. Stone slate rocks group
            GameObject stoneGroup = new GameObject("Payload_Stone");
            stoneGroup.transform.SetParent(payload.transform, false);
            CreateSubMesh("SlateRock1", CreateCubeMesh(new Vector3(0.18f, 0.14f, 0.16f)), GorkhaMaterials.StoneSlate, stoneGroup.transform, new Vector3(-0.05f, 0.02f, 0), Quaternion.Euler(15f, 25f, -10f));
            CreateSubMesh("SlateRock2", CreateCubeMesh(new Vector3(0.16f, 0.15f, 0.15f)), GorkhaMaterials.StoneSlate, stoneGroup.transform, new Vector3(0.06f, 0.03f, 0.02f), Quaternion.Euler(-10f, 40f, 15f));
            CreateSubMesh("SlateRock3", CreateCubeMesh(new Vector3(0.14f, 0.12f, 0.14f)), GorkhaMaterials.StoneSlate, stoneGroup.transform, new Vector3(0f, 0.12f, -0.02f), Quaternion.Euler(20f, -15f, 5f));

            // 3. Food wheat sheaves group
            GameObject foodGroup = new GameObject("Payload_Food");
            foodGroup.transform.SetParent(payload.transform, false);
            CreateSubMesh("WheatSheaf1", CreateCylinderMesh(0.07f, 0.38f), GorkhaMaterials.WheatField, foodGroup.transform, new Vector3(-0.05f, 0.05f, 0), Quaternion.Euler(80f, -10f, 0));
            CreateSubMesh("WheatSheaf2", CreateCylinderMesh(0.07f, 0.40f), GorkhaMaterials.WheatField, foodGroup.transform, new Vector3(0.05f, 0.06f, 0), Quaternion.Euler(82f, 12f, 0));
            CreateSubMesh("WheatSheaf3", CreateCylinderMesh(0.06f, 0.36f), GorkhaMaterials.WheatField, foodGroup.transform, new Vector3(0f, 0.12f, 0.02f), Quaternion.Euler(75f, 0, 0));

            // Also name a child ResourceSack for backwards compatibility
            GameObject legacyAlias = new GameObject("ResourceSack");
            legacyAlias.transform.SetParent(payload.transform, false);

            payload.SetActive(false); // Initially empty
            return root;
        }

        public static GameObject BuildVillagerTool(Transform parent, ResourceType type, Vector3 localPos = default, Quaternion localRot = default)
        {
            // For Food (farming), check Blender Historical Hasiya Sickle
            if (type == ResourceType.Food)
            {
                GameObject sicklePrefab = Resources.Load<GameObject>("GorkhaWeapons/Gorkha_Hasiya_Sickle");
                if (sicklePrefab != null && parent != null)
                {
                    GameObject sickleInstance = UnityEngine.Object.Instantiate(sicklePrefab, parent);
                    sickleInstance.name = "Tool_" + type;
                    sickleInstance.transform.localPosition = localPos;
                    sickleInstance.transform.localRotation = localRot == default ? Quaternion.identity : localRot;
                    sickleInstance.transform.localScale = Vector3.one * 1.0f;
                    foreach (var c in sickleInstance.GetComponentsInChildren<Collider>())
                    {
                        if (Application.isPlaying) Object.Destroy(c);
                        else Object.DestroyImmediate(c);
                    }
                    return sickleInstance;
                }
            }

            // First check if high-fidelity Synty weapon asset is present
            GameObject toolPrefab = FantasyKingdomAssetRegistry.LoadWeaponPrefab(type);
            if (toolPrefab != null && parent != null)
            {
                GameObject toolInstance = UnityEngine.Object.Instantiate(toolPrefab, parent);
                toolInstance.name = "Tool_" + type;
                toolInstance.transform.localPosition = localPos;
                toolInstance.transform.localRotation = localRot == default ? Quaternion.identity : localRot;
                toolInstance.transform.localScale = Vector3.one * 0.85f;

                Material genMat = FantasyKingdomAssetRegistry.GetSyntyGenericMaterial(1);
                if (genMat != null)
                {
                    foreach (var r in toolInstance.GetComponentsInChildren<Renderer>())
                        r.sharedMaterial = genMat;
                }
                foreach (var c in toolInstance.GetComponentsInChildren<Collider>())
                {
                    if (Application.isPlaying) Object.Destroy(c);
                    else Object.DestroyImmediate(c);
                }
                return toolInstance;
            }

            // Procedural Fallback Builder
            GameObject root = new GameObject("Tool_" + type);
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Salwood Handle & Brass Ferrule
            CreateSubMesh("Handle", CreateCylinderMesh(0.028f, 0.65f), GorkhaMaterials.SalWoodDark, root.transform);
            CreateSubMesh("Ferrule", CreateCylinderMesh(0.038f, 0.06f), GorkhaMaterials.PolishedBrass, root.transform, new Vector3(0, 0.28f, 0));

            if (type == ResourceType.Stone)
            {
                // Mining Pickaxe
                CreateSubMesh("PickCore", CreateCubeMesh(new Vector3(0.055f, 0.07f, 0.38f)), GorkhaMaterials.ForgedSteel, root.transform, new Vector3(0, 0.30f, 0));
                CreateSubMesh("PickBeak", CreatePyramidMesh(0.055f, 0.12f), GorkhaMaterials.ForgedSteel, root.transform, new Vector3(0, 0.30f, 0.22f), Quaternion.Euler(90, 0, 0));
                CreateSubMesh("PickChisel", CreatePyramidMesh(0.055f, 0.08f), GorkhaMaterials.ForgedSteel, root.transform, new Vector3(0, 0.30f, -0.21f), Quaternion.Euler(-90, 0, 0));
            }
            else if (type == ResourceType.Food)
            {
                // Authentic Nepali Curved Sickle (Hasiya / Khurpa)
                CreateSubMesh("SickleTang", CreateCubeMesh(new Vector3(0.025f, 0.10f, 0.04f)), GorkhaMaterials.ForgedSteel, root.transform, new Vector3(0, 0.33f, 0));
                CreateSubMesh("SickleBladeBase", CreateCubeMesh(new Vector3(0.015f, 0.08f, 0.18f)), GorkhaMaterials.ForgedSteel, root.transform, new Vector3(0, 0.38f, 0.08f), Quaternion.Euler(20f, 0, 0));
                CreateSubMesh("SickleBladeTip", CreateCubeMesh(new Vector3(0.012f, 0.06f, 0.16f)), GorkhaMaterials.ForgedSteel, root.transform, new Vector3(0, 0.36f, 0.22f), Quaternion.Euler(65f, 0, 0));
            }
            else
            {
                // Woodcutter Broadaxe
                CreateSubMesh("AxeEye", CreateCubeMesh(new Vector3(0.055f, 0.08f, 0.08f)), GorkhaMaterials.ForgedSteel, root.transform, new Vector3(0, 0.28f, 0));
                CreateSubMesh("AxeBlade", CreateCubeMesh(new Vector3(0.020f, 0.18f, 0.16f)), GorkhaMaterials.ForgedSteel, root.transform, new Vector3(0, 0.28f, 0.09f));
                CreateSubMesh("AxeHammer", CreateCubeMesh(new Vector3(0.045f, 0.06f, 0.06f)), GorkhaMaterials.ForgedSteel, root.transform, new Vector3(0, 0.28f, -0.06f));
            }

            return root;
        }

        public static GameObject BuildAnvil(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Armory_Anvil");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Tree Stump Block
            CreateSubMesh("TreeStump", CreateCylinderMesh(0.40f, 0.55f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.275f, 0));

            // Iron Anvil Base & Body
            CreateSubMesh("AnvilBase", CreateCubeMesh(new Vector3(0.38f, 0.12f, 0.52f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.61f, 0));
            CreateSubMesh("AnvilWaist", CreateCubeMesh(new Vector3(0.24f, 0.18f, 0.32f)), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.74f, 0));
            CreateSubMesh("AnvilFace", CreateCubeMesh(new Vector3(0.32f, 0.14f, 0.62f)), GorkhaMaterials.GoldenGajur, root.transform, new Vector3(0, 0.88f, 0));
            CreateSubMesh("AnvilHorn", CreatePyramidMesh(0.20f, 0.25f), GorkhaMaterials.GoldenGajur, root.transform, new Vector3(0, 0.88f, 0.38f), Quaternion.Euler(90, 0, 0));

            return root;
        }

        public static GameObject BuildWeaponRack(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Armory_WeaponRack");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // A-Frame Posts & Crossbeams
            CreateSubMesh("LeftPost", CreateCubeMesh(new Vector3(0.08f, 1.35f, 0.08f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(-0.55f, 0.675f, 0));
            CreateSubMesh("RightPost", CreateCubeMesh(new Vector3(0.08f, 1.35f, 0.08f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.55f, 0.675f, 0));
            CreateSubMesh("BeamUpper", CreateCubeMesh(new Vector3(1.20f, 0.08f, 0.08f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.95f, 0));
            CreateSubMesh("BeamLower", CreateCubeMesh(new Vector3(1.20f, 0.08f, 0.08f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.40f, 0));

            // Racked Spears
            BuildSpear(root.transform, new Vector3(-0.35f, 0.10f, 0), Quaternion.Euler(0, 0, 4f));
            BuildSpear(root.transform, new Vector3(0, 0.10f, 0));
            BuildSpear(root.transform, new Vector3(0.35f, 0.10f, 0), Quaternion.Euler(0, 0, -4f));

            return root;
        }

        public static GameObject BuildArcheryTarget(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Armory_ArcheryTarget");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Tripod Legs
            CreateSubMesh("Leg1", CreateCubeMesh(new Vector3(0.06f, 1.45f, 0.06f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(-0.35f, 0.70f, 0), Quaternion.Euler(0, 0, -14f));
            CreateSubMesh("Leg2", CreateCubeMesh(new Vector3(0.06f, 1.45f, 0.06f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.35f, 0.70f, 0), Quaternion.Euler(0, 0, 14f));
            CreateSubMesh("LegBack", CreateCubeMesh(new Vector3(0.06f, 1.40f, 0.06f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0, 0.68f, -0.38f), Quaternion.Euler(22f, 0, 0));

            // Straw Target Boss (Tilted slightly back)
            GameObject boss = new GameObject("TargetBoss");
            boss.transform.SetParent(root.transform, false);
            boss.transform.localPosition = new Vector3(0, 1.05f, -0.05f);
            boss.transform.localRotation = Quaternion.Euler(-15f, 0, 0);

            CreateSubMesh("StrawRim", CreateCylinderMesh(0.55f, 0.10f), GorkhaMaterials.GoldenWheat, boss.transform);
            CreateSubMesh("OuterRing", CreateCylinderMesh(0.42f, 0.11f), GorkhaMaterials.WhitePlaster, boss.transform);
            CreateSubMesh("InnerRing", CreateCylinderMesh(0.28f, 0.12f), GorkhaMaterials.RoyalRedFlag, boss.transform);
            CreateSubMesh("Bullseye", CreateCylinderMesh(0.12f, 0.13f), GorkhaMaterials.GoldenGajur, boss.transform);

            // Stuck Practice Arrow
            CreateSubMesh("StuckArrow", CreateCylinderMesh(0.02f, 0.45f), GorkhaMaterials.PagodaWood, boss.transform, new Vector3(0.12f, 0.08f, 0.18f), Quaternion.Euler(85, 20, 0));

            return root;
        }

        public static GameObject BuildSupplyCrateCluster(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Outpost_SupplyCluster");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Wooden Freight Crates with crossbraces
            CreateSubMesh("Crate1", CreateCubeMesh(new Vector3(0.65f, 0.65f, 0.65f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(-0.35f, 0.325f, 0));
            CreateSubMesh("Crate2", CreateCubeMesh(new Vector3(0.55f, 0.55f, 0.55f)), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.35f, 0.275f, 0.05f), Quaternion.Euler(0, 15f, 0));
            CreateSubMesh("Crate3", CreateCubeMesh(new Vector3(0.48f, 0.48f, 0.48f)), GorkhaMaterials.GoldenWheat, root.transform, new Vector3(-0.32f, 0.89f, -0.02f), Quaternion.Euler(0, -10f, 0));

            // Iron-Banded Storage Barrels
            CreateSubMesh("Barrel1", CreateCylinderMesh(0.32f, 0.72f), GorkhaMaterials.PagodaWood, root.transform, new Vector3(0.30f, 0.36f, -0.65f));
            CreateSubMesh("Band1", CreateCylinderMesh(0.33f, 0.08f), GorkhaMaterials.SlateStone, root.transform, new Vector3(0.30f, 0.52f, -0.65f));
            CreateSubMesh("Band2", CreateCylinderMesh(0.33f, 0.08f), GorkhaMaterials.SlateStone, root.transform, new Vector3(0.30f, 0.20f, -0.65f));

            return root;
        }

        public static GameObject BuildWindmillBlades(Transform parent, Vector3 localPos = default, Quaternion localRot = default)
        {
            GameObject root = new GameObject("Kinetic_WindmillBlades");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot == default ? Quaternion.identity : localRot;

            // Central Axle Hub Cap
            CreateSubMesh("HubCap", CreateCylinderMesh(0.40f, 0.45f), GorkhaMaterials.PagodaWood, root.transform, Vector3.zero, Quaternion.Euler(90f, 0, 0));
            CreateSubMesh("HubIronRing", CreateCylinderMesh(0.44f, 0.15f), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0, 0.10f), Quaternion.Euler(90f, 0, 0));
            CreateSubMesh("HubCone", CreatePyramidMesh(0.35f, 0.35f), GorkhaMaterials.GoldenGajur, root.transform, new Vector3(0, 0, 0.28f), Quaternion.Euler(90f, 0, 0));

            // 4 Radial Lattice Canvas Sails (cross layout)
            float[] bladeAngles = { 0f, 90f, 180f, 270f };
            foreach (float angle in bladeAngles)
            {
                Quaternion armRot = Quaternion.Euler(0, 0, angle);
                GameObject arm = new GameObject($"BladeArm_{angle:0}");
                arm.transform.SetParent(root.transform, false);
                arm.transform.localRotation = armRot;

                // Main Timber Spar extending outwards
                CreateSubMesh("Spar", CreateCubeMesh(new Vector3(0.12f, 3.8f, 0.12f)), GorkhaMaterials.PagodaWood, arm.transform, new Vector3(0, 1.9f, 0));

                // Canvas Sail Sheet
                CreateSubMesh("SailCanvas", CreateCubeMesh(new Vector3(0.65f, 2.8f, 0.03f)), GorkhaMaterials.WhitePlaster, arm.transform, new Vector3(0.35f, 2.2f, 0.02f));

                // Wooden Lattice Reinforcement Ribs
                for (int r = 0; r < 4; r++)
                {
                    float yOff = 1.2f + r * 0.7f;
                    CreateSubMesh($"Rib_{r}", CreateCubeMesh(new Vector3(0.70f, 0.04f, 0.05f)), GorkhaMaterials.PagodaWood, arm.transform, new Vector3(0.35f, yOff, 0.04f));
                }
            }

            // Smooth continuous rotation in Edit Mode & Play Mode
            var rotator = root.AddComponent<SmoothRotator>();
            rotator.RotationAxis = Vector3.forward;
            rotator.Speed = 28.0f;

            return root;
        }

        #endregion

        /// <summary>
        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent == null) return null;
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Generates a complete stylized military or civilian unit equipped with class weapons, shields, and headwear.
        /// Integrates Synty POLYGON Starter Pack 3D humanoid character models when installed, with seamless procedural fallback.
        /// Limb joints (hips, shoulders, head, torso) are fully articulated for realistic procedural combat animations.
        /// </summary>
        public static GameObject BuildUnit(Transform parent, UnitType unitType, bool isHostile = false)
        {
            string unitName = isHostile ? $"HostileBandit_{unitType}" : $"Gorkha_{unitType}";
            GameObject root = new GameObject(unitName);
            if (parent != null) root.transform.SetParent(parent, false);

            GameObject model = new GameObject("VisualModel");
            model.transform.SetParent(root.transform, false);

            // Soft circular ground shadow disc
            CreateSubMesh("ShadowDisc", CreateCylinderMesh(0.38f, 0.02f), GorkhaMaterials.SlateStone, root.transform, new Vector3(0, 0.01f, 0));

            // 1. Check if high-fidelity Synty POLYGON character prefab is available
            bool isFemale = unitType == UnitType.Villager && (UnityEngine.Random.value > 0.5f);
            GameObject charPrefab = FantasyKingdomAssetRegistry.LoadCharacterPrefab(unitType, isFemale);

            if (charPrefab != null)
            {
                // Instantiate Synty Character (Peasant model for Villagers, Warrior model for Soldiers)
                GameObject charInstance = UnityEngine.Object.Instantiate(charPrefab, model.transform);
                charInstance.name = "Synty_Character";
                charInstance.transform.localPosition = Vector3.zero;
                charInstance.transform.localRotation = Quaternion.identity;
                charInstance.transform.localScale = Vector3.one * 0.90f;

                // Disable default Animator so UnitProceduralAnimator has full procedural joint control
                var animator = charInstance.GetComponent<Animator>();
                if (animator != null) animator.enabled = false;

                // Remove child colliders so the unit's root collider handles physics
                foreach (var c in charInstance.GetComponentsInChildren<Collider>())
                {
                    if (Application.isPlaying) Object.Destroy(c);
                    else Object.DestroyImmediate(c);
                }

                // Apply authentic materials: Peasant texture for villagers, warrior texture for soldiers
                bool isCustomGorkhali = charPrefab.name.Contains("Gorkhali_Warrior");
                bool isPeasant = charPrefab.name.Contains("Peasent") || charPrefab.name.Contains("Peasant");
                Material charMat = isCustomGorkhali ? null : FantasyKingdomAssetRegistry.GetCharacterMaterial(isPeasant, isHostile);

                if (charMat != null)
                {
                    foreach (var r in charInstance.GetComponentsInChildren<Renderer>(true))
                    {
                        r.sharedMaterial = charMat;
                    }
                }

                // Locate skeleton bone joints
                Transform headBone        = FindChildRecursive(charInstance.transform, "Head");
                Transform handR           = FindChildRecursive(charInstance.transform, "Hand_R");
                Transform handL           = FindChildRecursive(charInstance.transform, "Hand_L");
                Transform spineBone       = FindChildRecursive(charInstance.transform, "Spine_02") ?? FindChildRecursive(charInstance.transform, "Spine_01");
                Transform syntyHipL       = FindChildRecursive(charInstance.transform, "UpperLeg_L");
                Transform syntyHipR       = FindChildRecursive(charInstance.transform, "UpperLeg_R");
                Transform kneeL           = FindChildRecursive(charInstance.transform, "LowerLeg_L");
                Transform kneeR           = FindChildRecursive(charInstance.transform, "LowerLeg_R");
                Transform ankleL          = FindChildRecursive(charInstance.transform, "Ankle_L");
                Transform ankleR          = FindChildRecursive(charInstance.transform, "Ankle_R");
                Transform syntyShoulderL  = FindChildRecursive(charInstance.transform, "Shoulder_L");
                Transform syntyShoulderR  = FindChildRecursive(charInstance.transform, "Shoulder_R");
                Transform elbowL          = FindChildRecursive(charInstance.transform, "Elbow_L");
                Transform elbowR          = FindChildRecursive(charInstance.transform, "Elbow_R");
                Transform hips            = FindChildRecursive(charInstance.transform, "Hips");

                // Disable modern/generic hair when equipping enclosed headgear
                Transform maleHair = FindChildRecursive(charInstance.transform, "Character_Hair_Male_01");
                Transform femaleHair = FindChildRecursive(charInstance.transform, "Character_Hair_Female_01");
                bool hideHair = unitType != UnitType.Archer || isHostile;
                if (hideHair)
                {
                    if (maleHair != null) maleHair.gameObject.SetActive(false);
                    if (femaleHair != null) femaleHair.gameObject.SetActive(false);
                }

                // 2. Attach Proud Gorkhali Mustache on adult male soldiers (Synty models only; custom model has native mustache)
                if (!isCustomGorkhali && !isFemale && !isHostile && headBone != null && unitType != UnitType.Villager)
                {
                    GameObject mustachePrefab = FantasyKingdomAssetRegistry.LoadAttachmentPrefab("moustache");
                    if (mustachePrefab != null)
                    {
                        GameObject mustache = UnityEngine.Object.Instantiate(mustachePrefab, headBone);
                        mustache.name = "Gorkhali_Moustache";
                        mustache.transform.localPosition = new Vector3(0, -0.045f, 0.09f);
                        mustache.transform.localRotation = Quaternion.identity;
                        mustache.transform.localScale = Vector3.one * 0.95f;
                        foreach (var c in mustache.GetComponentsInChildren<Collider>())
                        {
                            if (Application.isPlaying) Object.Destroy(c);
                            else Object.DestroyImmediate(c);
                        }
                    }
                }

                // 3. Equip Culturally Authentic Headwear (Synty models only; custom model has native Dhaka Topi)
                if (!isCustomGorkhali && headBone != null)
                {
                    if (isHostile)
                    {
                        // Marauder Iron Spiked War Bandana
                        CreateSubMesh("BanditWrap", CreateCubeMesh(new Vector3(0.34f, 0.10f, 0.34f)), GorkhaMaterials.BanditWarCloth, headBone, new Vector3(0, 0.11f, 0.02f));
                        CreateSubMesh("BanditSpike", CreatePyramidMesh(0.09f, 0.18f), GorkhaMaterials.DarkIron, headBone, new Vector3(0, 0.22f, 0.02f));
                    }
                    else
                    {
                        switch (unitType)
                        {
                            case UnitType.Spearman:
                                // Conical Bronze/Iron Combat Helmet with Golden Gajur Finial & Cheek Guards
                                BuildGorkhaHelmet(headBone, new Vector3(0, 0.14f, 0.01f), Quaternion.identity);
                                break;
                            case UnitType.Archer:
                                // Scout Rawhide Headband & Crimson Eagle Plume
                                CreateSubMesh("ArcherBand", CreateCubeMesh(new Vector3(0.34f, 0.08f, 0.34f)), GorkhaMaterials.RawhideLeather, headBone, new Vector3(0, 0.11f, 0.02f));
                                CreateSubMesh("FeatherPlume", CreateDoubleTriangleFlagMesh(0.12f, 0.28f), GorkhaMaterials.RoyalRedFlag, headBone, new Vector3(0.14f, 0.24f, -0.08f), Quaternion.Euler(-25, 0, 20));
                                break;
                            case UnitType.Villager:
                                // Woven Bamboo Straw Sun Hat (Ghum)
                                BuildStrawGhum(headBone, new Vector3(0, 0.16f, 0.01f), Quaternion.identity);
                                break;
                            case UnitType.KhukuriWarrior:
                            default:
                                // Authentic Geometric Woven Dhaka Topi with Golden Sun Insignia
                                BuildDhakaTopi(headBone, new Vector3(0, 0.14f, 0.01f), Quaternion.Euler(0, 25f, 0));
                                break;
                        }
                    }
                }

                // 4. Equip Armor, Pauldrons, Patuka Sashes & Hip Scabbards
                Transform sashRoot = hips != null ? hips : (spineBone != null ? spineBone : model.transform);

                if (!isCustomGorkhali && (unitType == UnitType.Spearman || unitType == UnitType.KhukuriWarrior))
                {
                    // Layered Lamellar Shoulder Pauldrons
                    BuildLamellarPauldrons(syntyShoulderL, syntyShoulderR, isHostile);
                }

                if (!isCustomGorkhali && unitType == UnitType.KhukuriWarrior)
                {
                    // Wrapped Royal Scarlet Patuka Waist Sash
                    CreateSubMesh("PatukaSash", CreateCubeMesh(new Vector3(0.38f, 0.14f, 0.30f)), GorkhaMaterials.LeatherPatuka, sashRoot, new Vector3(0, 0.04f, 0));
                    // Traditional Hip Scabbard (Dap) with Karda & Chakmak mini-knives
                    BuildKhukuriScabbard(sashRoot, new Vector3(-0.20f, 0.02f, 0.06f), Quaternion.Euler(15f, 25f, -65f));
                }
                else if (unitType == UnitType.Archer && syntyShoulderL != null)
                {
                    // Rawhide Forearm Bracer to protect against bowstring snap
                    CreateSubMesh("ArcherBracer", CreateCylinderMesh(0.08f, 0.16f), GorkhaMaterials.RawhideLeather, syntyShoulderL, new Vector3(0, -0.22f, 0));
                }

                // 5. Equip Class Weapons & Equipment
                Transform rWeaponSocket = handR != null ? handR : (syntyShoulderR != null ? syntyShoulderR : model.transform);
                Transform lWeaponSocket = handL != null ? handL : (syntyShoulderL != null ? syntyShoulderL : model.transform);
                Transform backSocket    = spineBone != null ? spineBone : model.transform;

                switch (unitType)
                {
                    case UnitType.Spearman:
                        // 2.35m Sangram Spear with Crimson Horsehair Tassel held in Right Hand
                        BuildSpear(rWeaponSocket, new Vector3(0.04f, -0.25f, 0.15f), Quaternion.Euler(20f, 0, 0));
                        // Royal Heraldic Pavise Shield with Golden Sun/Moon Crest on Left Arm
                        BuildPaviseShield(lWeaponSocket, new Vector3(-0.06f, 0f, 0.12f), Quaternion.Euler(0, 15f, 0));
                        break;

                    case UnitType.Archer:
                        // Composite Horn/Bamboo Recurve Bow in Left Hand
                        BuildRecurveBow(lWeaponSocket, new Vector3(-0.04f, 0f, 0.14f), Quaternion.Euler(0, 0, 20f));
                        // Rawhide Quiver with 4 Fletched Arrows on Back
                        BuildQuiver(backSocket, new Vector3(0.12f, 0.10f, -0.16f), Quaternion.Euler(15f, 0, -25f));
                        break;

                    case UnitType.Villager:
                        // Dynamic Toolset in Right Hand: Woodcutter Axe, Mining Pickaxe, and Farming Sickle
                        BuildVillagerTool(rWeaponSocket, ResourceType.Wood, new Vector3(0.04f, -0.10f, 0.14f), Quaternion.Euler(50f, 0, 0));
                        var pick = BuildVillagerTool(rWeaponSocket, ResourceType.Stone, new Vector3(0.04f, -0.10f, 0.14f), Quaternion.Euler(50f, 0, 0));
                        if (pick != null) pick.SetActive(false);
                        var sickle = BuildVillagerTool(rWeaponSocket, ResourceType.Food, new Vector3(0.04f, -0.10f, 0.14f), Quaternion.Euler(50f, 0, 0));
                        if (sickle != null) sickle.SetActive(false);
                        // Authentic Woven Bamboo Doko Basket with Forehead Namlo Tumpline & Dynamic Timber/Stone Payload
                        BuildDokoBasket(backSocket, headBone);
                        break;

                    case UnitType.KhukuriWarrior:
                    default:
                        if (isHostile)
                        {
                            // Dual Marauder Jagged Khukuris
                            BuildKhukuriBlade(rWeaponSocket, new Vector3(0.04f, -0.12f, 0.18f), Quaternion.Euler(45f, 0, 0), isBandit: true);
                            BuildKhukuriBlade(lWeaponSocket, new Vector3(-0.04f, -0.12f, 0.18f), Quaternion.Euler(45f, 0, 0), isBandit: true);
                        }
                        else
                        {
                            // Authentic Forged Steel Curved Khukuri with Cho Notch in Right Hand
                            BuildKhukuriBlade(rWeaponSocket, new Vector3(0.04f, -0.12f, 0.18f), Quaternion.Euler(45f, 0, 0), isBandit: false);
                            // Polished Brass Rim Buckler Shield on Left Forearm
                            BuildBucklerShield(lWeaponSocket, new Vector3(-0.06f, 0f, 0.10f), Quaternion.Euler(0, 20f, 0));
                        }
                        break;
                }

                // 6. Attach Articulated Procedural Animator & Apply Class Ready Stance
                var syntyAnim = model.AddComponent<UnitProceduralAnimator>();
                syntyAnim.SetupFullHumanoidRig(
                    hips: hips,
                    spine: spineBone,
                    head: headBone,
                    hipL: syntyHipL,
                    kneeL: kneeL,
                    ankleL: ankleL,
                    hipR: syntyHipR,
                    kneeR: kneeR,
                    ankleR: ankleR,
                    shoulderL: syntyShoulderL,
                    elbowL: elbowL,
                    handL: handL,
                    shoulderR: syntyShoulderR,
                    elbowR: elbowR,
                    handR: handR
                );
                syntyAnim.SetUnitClass(unitType, isHostile);

                BoxCollider syntyCol = root.AddComponent<BoxCollider>();
                syntyCol.center = new Vector3(0, 0.85f, 0);
                syntyCol.size = new Vector3(1.0f, 1.7f, 1.0f);

                return root;
            }

            // Procedural Fallback Builder (Executed if Synty asset pack is not present)
            Material tunicMat = isHostile ? GorkhaMaterials.BanditWarCloth : GorkhaMaterials.PagodaWood;
            Material pantsMat = isHostile ? GorkhaMaterials.TerraceDirt : GorkhaMaterials.WhitePlaster;
            Material trimMat  = isHostile ? GorkhaMaterials.DarkIron : GorkhaMaterials.LeatherPatuka;

            // 1. Torso Root & Body
            GameObject torso = new GameObject("Torso");
            torso.transform.SetParent(model.transform, false);
            torso.transform.localPosition = new Vector3(0, 0.55f, 0);

            CreateSubMesh("TorsoMesh", CreateCubeMesh(new Vector3(0.45f, 0.55f, 0.28f)), tunicMat, torso.transform, new Vector3(0, 0.275f, 0));
            CreateSubMesh("PatukaSash", CreateCubeMesh(new Vector3(0.48f, 0.14f, 0.30f)), trimMat, torso.transform, new Vector3(0, 0.07f, 0));

            // 2. Articulated Hip Joints & Legs
            GameObject hipL = new GameObject("HipLeft");
            hipL.transform.SetParent(model.transform, false);
            hipL.transform.localPosition = new Vector3(-0.12f, 0.55f, 0);
            CreateSubMesh("LegLeft", CreateCubeMesh(new Vector3(0.18f, 0.55f, 0.18f)), pantsMat, hipL.transform, new Vector3(0, -0.275f, 0));

            GameObject hipR = new GameObject("HipRight");
            hipR.transform.SetParent(model.transform, false);
            hipR.transform.localPosition = new Vector3(0.12f, 0.55f, 0);
            CreateSubMesh("LegRight", CreateCubeMesh(new Vector3(0.18f, 0.55f, 0.18f)), pantsMat, hipR.transform, new Vector3(0, -0.275f, 0));

            // 3. Head Pivot & Headwear
            GameObject head = new GameObject("Head");
            head.transform.SetParent(torso.transform, false);
            head.transform.localPosition = new Vector3(0, 0.55f, 0);
            CreateSubMesh("HeadMesh", CreateCubeMesh(new Vector3(0.30f, 0.30f, 0.30f)), GorkhaMaterials.TerraceDirt, head.transform, new Vector3(0, 0.15f, 0));

            if (isHostile)
            {
                CreateSubMesh("BanditWrap", CreateCubeMesh(new Vector3(0.36f, 0.12f, 0.36f)), GorkhaMaterials.BanditWarCloth, head.transform, new Vector3(0, 0.20f, 0));
                CreateSubMesh("BanditSpike", CreatePyramidMesh(0.10f, 0.18f), GorkhaMaterials.DarkIron, head.transform, new Vector3(0, 0.38f, 0));
            }
            else
            {
                switch (unitType)
                {
                    case UnitType.Spearman:
                        BuildGorkhaHelmet(head.transform, new Vector3(0, 0.22f, 0), Quaternion.identity);
                        break;
                    case UnitType.Archer:
                        CreateSubMesh("ArcherBand", CreateCubeMesh(new Vector3(0.34f, 0.10f, 0.34f)), GorkhaMaterials.RawhideLeather, head.transform, new Vector3(0, 0.22f, 0));
                        CreateSubMesh("FeatherPlume", CreateDoubleTriangleFlagMesh(0.12f, 0.28f), GorkhaMaterials.RoyalRedFlag, head.transform, new Vector3(0.14f, 0.32f, -0.10f), Quaternion.Euler(-25, 0, 20));
                        break;
                    case UnitType.Villager:
                        BuildStrawGhum(head.transform, new Vector3(0, 0.26f, 0), Quaternion.identity);
                        break;
                    case UnitType.KhukuriWarrior:
                    default:
                        BuildDhakaTopi(head.transform, new Vector3(0, 0.24f, 0), Quaternion.Euler(0, 25f, 0));
                        break;
                }
            }

            // 4. Shoulder Pivots & Arms
            GameObject shoulderL = new GameObject("ShoulderLeft");
            shoulderL.transform.SetParent(torso.transform, false);
            shoulderL.transform.localPosition = new Vector3(-0.32f, 0.50f, 0);
            CreateSubMesh("ArmLeft", CreateCubeMesh(new Vector3(0.14f, 0.45f, 0.14f)), pantsMat, shoulderL.transform, new Vector3(0, -0.225f, 0));

            GameObject shoulderR = new GameObject("ShoulderRight");
            shoulderR.transform.SetParent(torso.transform, false);
            shoulderR.transform.localPosition = new Vector3(0.32f, 0.50f, 0);
            CreateSubMesh("ArmRight", CreateCubeMesh(new Vector3(0.14f, 0.45f, 0.14f)), pantsMat, shoulderR.transform, new Vector3(0, -0.225f, 0));

            // 5. Weapon Rigging
            switch (unitType)
            {
                case UnitType.Spearman:
                    BuildSpear(shoulderR.transform, new Vector3(0.06f, -0.95f, 0.20f), Quaternion.Euler(15f, 0, 0));
                    BuildPaviseShield(shoulderL.transform, new Vector3(-0.06f, -0.30f, 0.18f), Quaternion.Euler(0, 15f, 0));
                    break;

                case UnitType.Archer:
                    BuildRecurveBow(shoulderL.transform, new Vector3(-0.06f, -0.20f, 0.22f), Quaternion.Euler(0, 0, 25f));
                    BuildQuiver(torso.transform, new Vector3(0.08f, 0.40f, -0.22f), Quaternion.Euler(0, 0, -25f));
                    break;

                case UnitType.Villager:
                    BuildVillagerTool(shoulderR.transform, ResourceType.Wood, new Vector3(0.04f, -0.35f, 0.16f), Quaternion.Euler(45f, 0, 0));
                    var pPick = BuildVillagerTool(shoulderR.transform, ResourceType.Stone, new Vector3(0.04f, -0.35f, 0.16f), Quaternion.Euler(45f, 0, 0));
                    if (pPick != null) pPick.SetActive(false);
                    var pSickle = BuildVillagerTool(shoulderR.transform, ResourceType.Food, new Vector3(0.04f, -0.35f, 0.16f), Quaternion.Euler(45f, 0, 0));
                    if (pSickle != null) pSickle.SetActive(false);
                    BuildDokoBasket(torso.transform, head.transform);
                    break;

                case UnitType.KhukuriWarrior:
                default:
                    BuildKhukuriBlade(shoulderR.transform, new Vector3(0.04f, -0.40f, 0.25f), Quaternion.Euler(35f, 0, 0), isHostile);
                    if (!isHostile)
                    {
                        BuildBucklerShield(shoulderL.transform, new Vector3(-0.06f, -0.30f, 0.12f), Quaternion.Euler(0, 20f, 0));
                    }
                    break;
            }

            // Attach Animator & Apply Stance
            var anim = model.AddComponent<UnitProceduralAnimator>();
            anim.SetupLimbs(hipL.transform, hipR.transform, shoulderL.transform, shoulderR.transform, torso.transform, head.transform);
            anim.SetUnitClass(unitType, isHostile);

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.8f, 0);
            col.size = new Vector3(1.0f, 1.6f, 1.0f);

            return root;
        }

        // Backwards compatibility overload
        public static GameObject BuildUnit(Transform parent = null, bool hasKhukuri = true)
        {
            return BuildUnit(parent, hasKhukuri ? UnitType.KhukuriWarrior : UnitType.Villager, isHostile: false);
        }

        #endregion
    }
}
