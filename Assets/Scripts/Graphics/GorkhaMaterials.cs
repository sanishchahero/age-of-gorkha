using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Centralized material, texture, and shader manager for authentic historical Nepali visuals.
    /// Dynamically loads high-resolution transparent RGBA building and unit artwork,
    /// Himalayan grass terrain textures, and sets up robust URP & Standard PBR materials.
    /// </summary>
    public static class GorkhaMaterials
    {
        private static readonly Dictionary<string, Material> _materialCache = new();
        private static readonly Dictionary<string, Texture2D> _textureCache = new();

        #region Texture Loader

        public static Texture2D LoadTexture(string relativePath)
        {
            if (_textureCache.TryGetValue(relativePath, out Texture2D cached) && cached != null)
            {
                return cached;
            }

            string fullPath = Path.Combine(Application.dataPath, relativePath);
            if (!File.Exists(fullPath))
            {
                string altPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", relativePath);
                if (File.Exists(altPath)) fullPath = altPath;
            }

            if (File.Exists(fullPath))
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(fullPath);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                    if (tex.LoadImage(fileData))
                    {
                        tex.name = Path.GetFileNameWithoutExtension(relativePath);
                        tex.filterMode = FilterMode.Bilinear;
                        tex.wrapMode = TextureWrapMode.Clamp;
                        tex.Apply();
                        _textureCache[relativePath] = tex;
                        return tex;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[GorkhaMaterials] Failed to load texture at {fullPath}: {ex.Message}");
                }
            }

            return null;
        }

        #endregion

        #region Shader Helpers

        private static Shader GetDefaultShader(bool transparent = false)
        {
            Shader shader = null;
            if (transparent)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                         Shader.Find("Universal Render Pipeline/Lit") ??
                         Shader.Find("Unlit/Transparent") ??
                         Shader.Find("Sprites/Default") ??
                         Shader.Find("Standard");
            }
            else
            {
                shader = Shader.Find("Universal Render Pipeline/Lit") ??
                         Shader.Find("Universal Render Pipeline/Unlit") ??
                         Shader.Find("Standard") ??
                         Shader.Find("Diffuse");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard") ?? Shader.Find("Diffuse");
            }

            return shader;
        }

        public static Material GetSpriteCutoutMaterial(string key, string texturePath, Color tint = default)
        {
            if (_materialCache.TryGetValue(key, out Material existing) && existing != null)
            {
                return existing;
            }

            if (tint == default) tint = Color.white;
            Texture2D tex = LoadTexture(texturePath);

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                            Shader.Find("Universal Render Pipeline/Lit") ??
                            Shader.Find("Unlit/Transparent Cutout") ??
                            Shader.Find("Unlit/Transparent") ??
                            Shader.Find("Sprites/Default") ??
                            Shader.Find("Standard");

            Material mat = new Material(shader)
            {
                name = key,
                color = tint
            };

            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
            }

            // Robust Alpha Blending & Double-Sided Settings
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);       // URP Transparent
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0);           // URP Alpha
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0);             // Double-sided
            if (mat.HasProperty("_AlphaClip")) mat.SetFloat("_AlphaClip", 1);   // Cutout
            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.25f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.renderQueue = 3000;

            _materialCache[key] = mat;
            return mat;
        }

        public static Material GetMaterial(string key, Color color, float metallic = 0.1f, float smoothness = 0.35f, bool transparent = false)
        {
            if (_materialCache.TryGetValue(key, out Material existing) && existing != null)
            {
                return existing;
            }

            Shader shader = GetDefaultShader(transparent);
            Material mat = new Material(shader)
            {
                name = key,
                color = color
            };

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            if (transparent)
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
            }

            _materialCache[key] = mat;
            return mat;
        }

        public static Material GetTexturedMaterial(string key, Texture2D texture, Color tint = default, float metallic = 0.05f, float smoothness = 0.25f, Vector2 tiling = default, bool doubleSided = false)
        {
            if (_materialCache.TryGetValue(key, out Material existing) && existing != null)
            {
                return existing;
            }

            if (tint == default) tint = Color.white;
            if (tiling == default) tiling = Vector2.one;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ??
                            Shader.Find("Standard") ??
                            Shader.Find("Universal Render Pipeline/Unlit") ??
                            Shader.Find("Diffuse");

            Material mat = new Material(shader)
            {
                name = key,
                color = tint
            };

            if (texture != null)
            {
                mat.mainTexture = texture;
                mat.mainTextureScale = tiling;
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", texture);
                    mat.SetTextureScale("_BaseMap", tiling);
                }
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (doubleSided && mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0);

            _materialCache[key] = mat;
            return mat;
        }

        #endregion

        #region Building & Unit High-Res Artwork Materials

        public static Material TownCenterSpriteMat => GetSpriteCutoutMaterial("M_TownCenter_Sprite", "Textures/buildings/town_center.png");
        public static Material FarmSpriteMat => GetSpriteCutoutMaterial("M_Farm_Sprite", "Textures/buildings/farm.png");
        public static Material BarracksSpriteMat => GetSpriteCutoutMaterial("M_Barracks_Sprite", "Textures/buildings/barracks.png");
        public static Material HouseSpriteMat => GetSpriteCutoutMaterial("M_House_Sprite", "Textures/buildings/house.png");
        public static Material WatchtowerSpriteMat => GetSpriteCutoutMaterial("M_Watchtower_Sprite", "Textures/buildings/watchtower.png");
        public static Material SpearmanSpriteMat => GetSpriteCutoutMaterial("M_Spearman_Sprite", "Textures/units/spearman.png");

        public static Material GrassTileTerrainMat
        {
            get
            {
                const string key = "M_GrassTile_Tiled";
                if (_materialCache.TryGetValue(key, out Material existing) && existing != null)
                {
                    return existing;
                }

                Texture2D grassTex = LoadTexture("Textures/terrain/grass_tile.png");
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
                Material mat = new Material(shader)
                {
                    name = key,
                    color = Color.white
                };

                if (grassTex != null)
                {
                    grassTex.wrapMode = TextureWrapMode.Repeat;
                    mat.mainTexture = grassTex;
                    mat.mainTextureScale = new Vector2(16, 16);
                    if (mat.HasProperty("_BaseMap"))
                    {
                        mat.SetTexture("_BaseMap", grassTex);
                        mat.SetTextureScale("_BaseMap", new Vector2(16, 16));
                    }
                }
                else
                {
                    mat.color = new Color(0.32f, 0.58f, 0.24f);
                }

                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);

                _materialCache[key] = mat;
                return mat;
            }
        }

        #endregion

        #region Procedural 3D Accent Materials

        private static Texture2D _particleSoftCircleTex;
        public static Texture2D ParticleSoftCircleTex
        {
            get
            {
                if (_particleSoftCircleTex == null)
                {
                    int size = 64;
                    _particleSoftCircleTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                    float center = (size - 1) * 0.5f;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                            float alpha = Mathf.Clamp01(1.0f - dist);
                            alpha = Mathf.Pow(alpha, 1.8f);
                            _particleSoftCircleTex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                        }
                    }
                    _particleSoftCircleTex.Apply();
                }
                return _particleSoftCircleTex;
            }
        }

        public static Material GetParticleMaterial(string key, Color color)
        {
            if (_materialCache.TryGetValue(key, out Material existing) && existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                            Shader.Find("Particles/Standard Unlit") ??
                            Shader.Find("Mobile/Particles/Alpha Blended") ??
                            Shader.Find("Sprites/Default") ??
                            Shader.Find("Universal Render Pipeline/Unlit");

            Material mat = new Material(shader)
            {
                name = key,
                color = color
            };

            mat.mainTexture = ParticleSoftCircleTex;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", ParticleSoftCircleTex);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3100;

            _materialCache[key] = mat;
            return mat;
        }

        #region Authentic Nepali Character & Military Materials

        private static Texture2D _dhakaFabricTex;
        /// <summary>
        /// Procedurally generated authentic Nepali Dhaka geometric diamond textile pattern.
        /// Features traditional crimson red background, repeating golden diamond lattice, and obsidian/ivory accents.
        /// </summary>
        public static Texture2D DhakaFabricTex
        {
            get
            {
                if (_dhakaFabricTex == null)
                {
                    int size = 128;
                    _dhakaFabricTex = new Texture2D(size, size, TextureFormat.RGBA32, true);
                    Color redBase = new Color(0.72f, 0.10f, 0.14f);      // Royal Crimson
                    Color goldAccent = new Color(0.96f, 0.80f, 0.22f);   // Golden Amber
                    Color blackAccent = new Color(0.12f, 0.10f, 0.12f);  // Obsidian
                    Color whiteAccent = new Color(0.94f, 0.92f, 0.88f);  // Off-white ivory

                    int cell = 32;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            int cx = Mathf.Abs((x % cell) - cell / 2);
                            int cy = Mathf.Abs((y % cell) - cell / 2);
                            int dist = cx + cy; // Diamond Manhattan distance

                            Color c;
                            if (dist <= 3)
                                c = whiteAccent;
                            else if (dist <= 6)
                                c = blackAccent;
                            else if (dist <= 10)
                                c = goldAccent;
                            else if (dist <= 12)
                                c = blackAccent;
                            else
                                c = redBase;

                            // Subtle microscopic weave texture
                            float weave = ((x + y) % 2 == 0) ? 1.0f : 0.92f;
                            c *= weave;
                            c.a = 1.0f;

                            _dhakaFabricTex.SetPixel(x, y, c);
                        }
                    }
                    _dhakaFabricTex.filterMode = FilterMode.Bilinear;
                    _dhakaFabricTex.wrapMode = TextureWrapMode.Repeat;
                    _dhakaFabricTex.Apply();
                }
                return _dhakaFabricTex;
            }
        }

        private static Texture2D _wovenBambooTex;
        /// <summary>
        /// Procedural natural bamboo wicker weave texture for Doko baskets and Ghum straw sun hats.
        /// </summary>
        public static Texture2D WovenBambooTex
        {
            get
            {
                if (_wovenBambooTex == null)
                {
                    int size = 128;
                    _wovenBambooTex = new Texture2D(size, size, TextureFormat.RGBA32, true);
                    Color lightBamboo = new Color(0.78f, 0.64f, 0.40f);
                    Color midBamboo   = new Color(0.64f, 0.50f, 0.30f);
                    Color darkBamboo  = new Color(0.44f, 0.32f, 0.18f);
                    Color gapColor    = new Color(0.22f, 0.15f, 0.08f);

                    int stripe = 16;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            int blockX = (x / stripe) % 2;
                            int blockY = (y / stripe) % 2;
                            bool horizontal = (blockX ^ blockY) == 0;

                            int local = horizontal ? (y % stripe) : (x % stripe);
                            Color c;
                            if (local == 0 || local == stripe - 1)
                                c = gapColor;
                            else if (local < 3 || local > stripe - 4)
                                c = darkBamboo;
                            else
                                c = (((horizontal ? x : y) % 4) < 2) ? lightBamboo : midBamboo;

                            c.a = 1.0f;
                            _wovenBambooTex.SetPixel(x, y, c);
                        }
                    }
                    _wovenBambooTex.filterMode = FilterMode.Bilinear;
                    _wovenBambooTex.wrapMode = TextureWrapMode.Repeat;
                    _wovenBambooTex.Apply();
                }
                return _wovenBambooTex;
            }
        }

        /// <summary>High-specular polished steel for razor Khukuri edges, spearheads, and arrowheads.</summary>
        public static Material ForgedSteel => GetMaterial("M_ForgedHimalayanSteel", new Color(0.88f, 0.92f, 0.96f), metallic: 0.95f, smoothness: 0.88f);

        /// <summary>Warm reflective golden brass for weapon bolsters, pommels, helmet crests, and shield bosses.</summary>
        public static Material PolishedBrass => GetMaterial("M_PolishedGorkhaBrass", new Color(0.95f, 0.78f, 0.22f), metallic: 0.88f, smoothness: 0.80f);

        /// <summary>Hammered dark forged iron for spearmen helmet rims, counterweights, and hostile bandit spikes.</summary>
        public static Material DarkIron => GetMaterial("M_HammeredDarkIron", new Color(0.24f, 0.25f, 0.28f), metallic: 0.82f, smoothness: 0.38f);

        /// <summary>Authentic geometric woven Nepali Dhaka cloth for Topis and ceremonial sashes.</summary>
        public static Material DhakaFabric => GetTexturedMaterial("M_DhakaFabric_Textured", DhakaFabricTex, Color.white, metallic: 0.0f, smoothness: 0.12f, new Vector2(4, 4), doubleSided: true);

        /// <summary>Authentic woven bamboo wicker material for the villager Doko basket and Ghum straw sun hat.</summary>
        public static Material WovenBambooDoko => GetTexturedMaterial("M_WovenBamboo_Doko", WovenBambooTex, Color.white, metallic: 0.0f, smoothness: 0.15f, new Vector2(3, 3), doubleSided: true);

        /// <summary>Deep royal scarlet leather for military waist Patuka sashes and Khukuri scabbard (Dap) leather.</summary>
        public static Material LeatherPatuka => GetMaterial("M_LeatherPatuka", new Color(0.72f, 0.08f, 0.12f), metallic: 0.04f, smoothness: 0.38f);

        /// <summary>Dark oiled Himalayan Sal wood for spear shafts, bow risers, and weapon handles.</summary>
        public static Material SalWoodDark => GetMaterial("M_SalWoodDark", new Color(0.26f, 0.15f, 0.08f), metallic: 0.02f, smoothness: 0.42f);

        /// <summary>Tanned rawhide leather for arm bracers, bow grips, and quiver straps.</summary>
        public static Material RawhideLeather => GetMaterial("M_RawhideLeather", new Color(0.55f, 0.40f, 0.26f), metallic: 0.02f, smoothness: 0.25f);

        /// <summary>Weathered dark raider cloth for hostile bandits and marauders.</summary>
        public static Material BanditWarCloth => GetMaterial("M_BanditWarCloth", new Color(0.18f, 0.16f, 0.16f), metallic: 0.10f, smoothness: 0.20f);

        #endregion

        public static Material RedBrick => GetMaterial("M_NepaliRedBrick", new Color(0.72f, 0.24f, 0.18f), metallic: 0.05f, smoothness: 0.25f);
        public static Material PagodaWood => GetMaterial("M_SalWood", new Color(0.28f, 0.16f, 0.08f), metallic: 0.05f, smoothness: 0.3f);
        public static Material GoldenGajur => GetMaterial("M_GoldenGajur", new Color(0.96f, 0.78f, 0.15f), metallic: 0.15f, smoothness: 0.25f);
        public static Material SlateStone => GetMaterial("M_HimalayanSlate", new Color(0.42f, 0.44f, 0.46f), metallic: 0.15f, smoothness: 0.2f);
        public static Material AlpineGrass => GrassTileTerrainMat;
        public static Material TerraceDirt => GetMaterial("M_TerraceDirt", new Color(0.48f, 0.35f, 0.22f), metallic: 0.02f, smoothness: 0.1f);
        public static Material GoldenWheat => GetMaterial("M_GoldenWheat", new Color(0.92f, 0.72f, 0.25f), metallic: 0.0f, smoothness: 0.1f);
        public static Material WhitePlaster => GetMaterial("M_WhitePlaster", new Color(0.92f, 0.90f, 0.85f), metallic: 0.05f, smoothness: 0.4f);
        public static Material RoyalRedFlag
        {
            get
            {
                const string key = "M_RoyalRedFlag";
                if (_materialCache.TryGetValue(key, out Material existing) && existing != null)
                {
                    return existing;
                }

                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ??
                                Shader.Find("Universal Render Pipeline/Unlit") ??
                                Shader.Find("Standard");

                Color flagRed = new Color(0.88f, 0.14f, 0.18f);
                Material mat = new Material(shader)
                {
                    name = key,
                    color = flagRed
                };

                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", flagRed);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", flagRed);
                if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0); // Double-sided
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.0f);

                _materialCache[key] = mat;
                return mat;
            }
        }
        public static Material ScaffoldingWood => GetMaterial("M_ScaffoldingWood", new Color(0.65f, 0.50f, 0.30f), metallic: 0.0f, smoothness: 0.15f);
        public static Material PlacementValid => GetMaterial("M_PlacementValid", new Color(0.1f, 0.95f, 0.35f, 0.55f), metallic: 0.0f, smoothness: 0.9f, transparent: true);
        public static Material PlacementBlocked => GetMaterial("M_PlacementBlocked", new Color(0.95f, 0.15f, 0.15f, 0.55f), metallic: 0.0f, smoothness: 0.9f, transparent: true);
        public static Material HarvestBubble => GetMaterial("M_HarvestBubble", new Color(1.0f, 0.85f, 0.1f, 0.85f), metallic: 0.2f, smoothness: 0.5f, transparent: true);
        public static Material CobblestonePath => GetMaterial("M_CobblestonePath", new Color(0.46f, 0.42f, 0.38f), metallic: 0.05f, smoothness: 0.15f);
        public static Material SelectionGlow => GetMaterial("M_SelectionGlow", new Color(0.25f, 0.98f, 0.45f, 0.70f), metallic: 0.1f, smoothness: 0.9f, transparent: true);
        public static Material SmokeParticle => GetParticleMaterial("M_SmokeParticle", new Color(0.82f, 0.82f, 0.86f, 0.35f));
        public static Material FireParticle => GetParticleMaterial("M_FireParticle", new Color(1.0f, 0.65f, 0.15f, 0.80f));
        public static Material SparkleParticle => GetParticleMaterial("M_SparkleParticle", new Color(1.0f, 0.92f, 0.35f, 0.90f));
        public static Material DustParticle => GetParticleMaterial("M_DustParticle", new Color(0.72f, 0.66f, 0.56f, 0.40f));

        #endregion
    }
}
