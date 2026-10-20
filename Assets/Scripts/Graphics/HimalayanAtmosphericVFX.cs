using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Gorkha.Buildings;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Manages high-fidelity atmospheric environmental VFX from the Fantasy Kingdom kit:
    /// - Curling chimney smoke plumes from village keeps and hearths
    /// - Windblown autumn leaves swirling through village streets
    /// - Golden sunlit dust and pollen motes floating in the mountain light
    /// - Flocks of mountain eagles and crows circling high in the sky
    /// </summary>
    [ExecuteAlways]
    public class HimalayanAtmosphericVFX : MonoBehaviour
    {
        public static HimalayanAtmosphericVFX Instance { get; private set; }

        public const string VFX_CHIMNEY_SMOKE  = "Assets/Art/FX/VFX_Files/VFX_ChimneySmoke.vfx";
        public const string VFX_LEAVES         = "Assets/Art/FX/VFX_Files/VFX_Leaves.vfx";
        public const string VFX_POLLEN         = "Assets/Art/FX/VFX_Files/VFX_Pollen.vfx";
        public const string VFX_ROUNDING_BIRDS = "Assets/Art/FX/VFX_Files/VFX_RoundingBirds.vfx";
        public const string VFX_GROUP_BIRDS    = "Assets/Art/FX/VFX_Files/VFX_GroupBirds.vfx";

        [Header("VFX Toggles")]
        [SerializeField] private bool _enableChimneySmoke = true;
        [SerializeField] private bool _enableSwirlingLeaves = true;
        [SerializeField] private bool _enableSunlitPollen = true;
        [SerializeField] private bool _enableCirclingBirds = true;

        private VisualEffectAsset _smokeAsset;
        private VisualEffectAsset _leavesAsset;
        private VisualEffectAsset _pollenAsset;
        private VisualEffectAsset _birdsAsset;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
                return;
            }
            Instance = this;
            LoadVFXAssets();
            EnsureAtmosphericVFX();
        }

        private void OnEnable()
        {
            LoadVFXAssets();
            EnsureAtmosphericVFX();
        }

        private void Start()
        {
            EnsureAtmosphericVFX();
        }

        public void LoadVFXAssets()
        {
#if UNITY_EDITOR
            if (_smokeAsset == null)
                _smokeAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(VFX_CHIMNEY_SMOKE);
            if (_leavesAsset == null)
                _leavesAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(VFX_LEAVES);
            if (_pollenAsset == null)
                _pollenAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(VFX_POLLEN);
            if (_birdsAsset == null)
                _birdsAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(VFX_ROUNDING_BIRDS) 
                           ?? UnityEditor.AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(VFX_GROUP_BIRDS);
#endif
        }

        public void EnsureAtmosphericVFX()
        {
            Vector3 villageCenter = new Vector3(24f, 0f, 24f);

            // 1. Swirling Windblown Leaves over the central plaza
            if (_enableSwirlingLeaves && transform.Find("VFX_AtmosphericLeaves") == null)
            {
                SpawnVFXInstance("VFX_AtmosphericLeaves", _leavesAsset, villageCenter + new Vector3(0, 2.5f, 0));
            }

            // 2. Sunlit Pollen Dust Motes
            if (_enableSunlitPollen && transform.Find("VFX_SunlitPollen") == null)
            {
                SpawnVFXInstance("VFX_SunlitPollen", _pollenAsset, villageCenter + new Vector3(0, 3.0f, 0));
            }

            // 3. High Altitude Mountain Birds Circling Overhead
            if (_enableCirclingBirds && transform.Find("VFX_CirclingBirds") == null)
            {
                SpawnVFXInstance("VFX_CirclingBirds", _birdsAsset, villageCenter + new Vector3(0, 32f, 0));
            }

            // 4. Attach Chimney Smoke to active buildings
            if (_enableChimneySmoke)
            {
                AttachChimneySmokes();
            }
        }

        private GameObject SpawnVFXInstance(string objName, VisualEffectAsset asset, Vector3 pos)
        {
            GameObject vfxObj = new GameObject(objName);
            vfxObj.transform.SetParent(transform, false);
            vfxObj.transform.position = pos;

            if (asset != null)
            {
                var vfx = vfxObj.AddComponent<VisualEffect>();
                vfx.visualEffectAsset = asset;
                vfx.Play();
            }

            return vfxObj;
        }

        public void AttachChimneySmokes()
        {
            foreach (var b in BuildingInstance.AllBuildings)
            {
                if (b == null || b.Data == null) continue;
                AttachChimneySmokeToBuilding(b.gameObject, b.Data.buildingType);
            }
        }

        public void AttachChimneySmokeToBuilding(GameObject buildingObj, BuildingType type)
        {
            if (buildingObj == null) return;
            if (buildingObj.transform.Find("ChimneySmokeVFX") != null) return;

            Vector3 smokeOffset = type switch
            {
                BuildingType.TownCenter => new Vector3(0.5f, 6.2f, -1.2f),
                BuildingType.Barracks   => new Vector3(1.8f, 3.5f, 0.8f),
                BuildingType.House      => new Vector3(0.3f, 4.2f, 0.4f),
                _ => Vector3.zero
            };

            if (smokeOffset == Vector3.zero) return;

            GameObject smokeObj = new GameObject("ChimneySmokeVFX");
            smokeObj.transform.SetParent(buildingObj.transform, false);
            smokeObj.transform.localPosition = smokeOffset;

            if (_smokeAsset != null)
            {
                var vfx = smokeObj.AddComponent<VisualEffect>();
                vfx.visualEffectAsset = _smokeAsset;
                vfx.Play();
            }
            else
            {
                // Procedural chimney smoke fallback
                var ps = smokeObj.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 3.5f;
                main.startSpeed = 0.8f;
                main.startSize = 0.45f;
                main.startColor = new Color(0.85f, 0.85f, 0.85f, 0.35f);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 12f;
                shape.radius = 0.15f;

                var emission = ps.emission;
                emission.rateOverTime = 8f;
            }
        }
    }
}
