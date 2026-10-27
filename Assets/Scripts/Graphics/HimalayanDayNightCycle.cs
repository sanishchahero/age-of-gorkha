using System.Collections.Generic;
using UnityEngine;
using Gorkha.Buildings;

namespace Gorkha.Graphics
{
    public enum TimeOfDay
    {
        Dawn,
        Noon,
        Sunset,
        Night
    }

    /// <summary>
    /// Himalayan Dynamic Day-Night Cycle & Atmospheric Shading (Phase 6).
    /// Professional RTS / Clash of Clans aesthetic:
    /// - Golden-hour amber sunset, crystal alpine noon, fresh morning dawn, and radiant cobalt moonlight night.
    /// - Direct sun & moon always maintain a high isometric angle (>= 28 deg) so terrain, units, and structures are crisp and visible.
    /// - Dynamic Trilight Ambient & Mountain Mist Fog tinting with high visual contrast.
    /// - Warm golden lantern & torch point lights illuminate village buildings during sunset and night.
    /// </summary>
    public class HimalayanDayNightCycle : MonoBehaviour
    {
        public static HimalayanDayNightCycle Instance { get; private set; }

        [Header("Time Configuration")]
        [SerializeField] private bool _autoAdvance = false; // Default manual for testing convenience
        [SerializeField] private float _cycleDurationMinutes = 8.0f;
        [SerializeField] private TimeOfDay _currentTime = TimeOfDay.Noon;

        private float _timeProgress = 0.45f; // 0.25 = Dawn, 0.45 = Noon, 0.72 = Sunset, 0.95 = Night
        private Light _sunLight;
        private readonly List<Light> _villageLanterns = new();
        private float _lastLanternCheckTime = 0f;

        public TimeOfDay CurrentTime => _currentTime;
        public float TimeProgress => _timeProgress;
        public bool AutoAdvance
        {
            get => _autoAdvance;
            set => _autoAdvance = value;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            FindSunLight();
            ApplyLightingForProgress(_timeProgress);
        }

        private void FindSunLight()
        {
            if (_sunLight == null)
            {
                var sunObj = GameObject.Find("HimalayanSun_DirectionalLight");
                if (sunObj != null) _sunLight = sunObj.GetComponent<Light>();
                else
                {
                    var allLights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
                    foreach (var l in allLights)
                    {
                        if (l.type == LightType.Directional)
                        {
                            _sunLight = l;
                            break;
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (_autoAdvance && Application.isPlaying)
            {
                float rate = 1.0f / (_cycleDurationMinutes * 60f);
                _timeProgress = (_timeProgress + rate * Time.deltaTime) % 1.0f;
                ApplyLightingForProgress(_timeProgress);
            }

            // Periodically refresh village lanterns if new buildings are placed
            if (Time.time - _lastLanternCheckTime > 2.0f)
            {
                _lastLanternCheckTime = Time.time;
                UpdateVillageLanterns(_currentTime);
            }
        }

        public void SetTimeOfDay(TimeOfDay tod)
        {
            _currentTime = tod;
            _timeProgress = tod switch
            {
                TimeOfDay.Dawn => 0.25f,
                TimeOfDay.Noon => 0.45f,
                TimeOfDay.Sunset => 0.72f,
                TimeOfDay.Night => 0.95f,
                _ => 0.45f
            };
            ApplyLightingForProgress(_timeProgress);
        }

        public void CycleNextTimeOfDay()
        {
            var nextTod = (TimeOfDay)(((int)_currentTime + 1) % 4);
            SetTimeOfDay(nextTod);
        }

        private void ApplyLightingForProgress(float t)
        {
            FindSunLight();
            if (_sunLight == null) return;

            Color sunColor;
            float sunIntensity;
            Quaternion sunRotation;
            Color skyAmbient;
            Color equatorAmbient;
            Color groundAmbient;
            Color fogColor;
            float fogDensity;

            if (t >= 0.18f && t < 0.35f)
            {
                // Dawn: Fresh alpine morning with rose-gold illumination
                _currentTime = TimeOfDay.Dawn;
                sunRotation = Quaternion.Euler(38f, -55f, 0f);
                sunColor = new Color(1.0f, 0.88f, 0.72f);
                sunIntensity = 1.08f;
                skyAmbient = new Color(0.60f, 0.70f, 0.88f);
                equatorAmbient = new Color(0.78f, 0.68f, 0.58f);
                groundAmbient = new Color(0.30f, 0.35f, 0.28f);
                fogColor = new Color(0.72f, 0.76f, 0.84f);
                fogDensity = 0.0035f;
            }
            else if (t >= 0.35f && t < 0.65f)
            {
                // Noon: Crisp crystal alpine daylight with sharp shadows
                _currentTime = TimeOfDay.Noon;
                sunRotation = Quaternion.Euler(56f, -35f, 0f);
                sunColor = new Color(1.0f, 0.97f, 0.92f);
                sunIntensity = 1.25f;
                skyAmbient = new Color(0.65f, 0.78f, 0.94f);
                equatorAmbient = new Color(0.78f, 0.72f, 0.62f);
                groundAmbient = new Color(0.30f, 0.40f, 0.24f);
                fogColor = new Color(0.72f, 0.80f, 0.88f);
                fogDensity = 0.0040f;
            }
            else if (t >= 0.65f && t < 0.82f)
            {
                // Sunset: Golden-hour Himalayan alpenglow (warm, rich, high visibility)
                _currentTime = TimeOfDay.Sunset;
                sunRotation = Quaternion.Euler(32f, -18f, 0f);
                sunColor = new Color(1.0f, 0.74f, 0.45f);
                sunIntensity = 1.15f;
                skyAmbient = new Color(0.75f, 0.62f, 0.80f);
                equatorAmbient = new Color(0.85f, 0.58f, 0.42f);
                groundAmbient = new Color(0.38f, 0.32f, 0.28f);
                fogColor = new Color(0.82f, 0.65f, 0.55f);
                fogDensity = 0.0035f;
            }
            else
            {
                // Night: Stylized Clash of Clans cobalt moonlight with high readability
                _currentTime = TimeOfDay.Night;
                sunRotation = Quaternion.Euler(48f, -42f, 0f); // High angle moon - NEVER underground!
                sunColor = new Color(0.68f, 0.82f, 1.0f);    // Crisp silver-cyan moonlight
                sunIntensity = 0.78f;                        // Strong direct moonlight for clear silhouettes
                skyAmbient = new Color(0.42f, 0.54f, 0.78f); // Luminous deep azure sky fill
                equatorAmbient = new Color(0.36f, 0.44f, 0.60f);
                groundAmbient = new Color(0.26f, 0.32f, 0.44f);
                fogColor = new Color(0.28f, 0.35f, 0.50f);
                fogDensity = 0.0030f;
            }

            _sunLight.transform.rotation = sunRotation;
            _sunLight.color = sunColor;
            _sunLight.intensity = sunIntensity;
            _sunLight.shadowStrength = _currentTime == TimeOfDay.Night ? 0.50f : 0.72f;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = skyAmbient;
            RenderSettings.ambientEquatorColor = equatorAmbient;
            RenderSettings.ambientGroundColor = groundAmbient;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;

            UpdateVillageLanterns(_currentTime);
        }

        private void UpdateVillageLanterns(TimeOfDay tod)
        {
            bool lanternsActive = tod == TimeOfDay.Sunset || tod == TimeOfDay.Night;
            float targetIntensity = tod == TimeOfDay.Night ? 1.6f : (tod == TimeOfDay.Sunset ? 0.9f : 0f);
            Color lanternColor = new Color(1.0f, 0.75f, 0.32f); // Warm flame amber

            var buildings = FindObjectsByType<BuildingInstance>(FindObjectsInactive.Exclude);
            foreach (var b in buildings)
            {
                if (b == null) continue;
                Transform lanternObj = b.transform.Find("NightLantern_WarmLight");

                if (lanternObj == null && lanternsActive)
                {
                    GameObject obj = new GameObject("NightLantern_WarmLight");
                    obj.transform.SetParent(b.transform, false);
                    obj.transform.localPosition = new Vector3(0, 2.2f, 0);

                    Light l = obj.AddComponent<Light>();
                    l.type = LightType.Point;
                    float range = 8.0f;
                    if (b.Data != null)
                    {
                        if (b.Data.buildingType == BuildingType.TownCenter || b.Data.buildingType == BuildingType.Wonder) range = 14.0f;
                        else if (b.Data.buildingType == BuildingType.Barracks || b.Data.buildingType == BuildingType.Academy) range = 11.0f;
                    }
                    l.range = range;
                    l.color = lanternColor;
                    l.intensity = targetIntensity;
                    l.shadows = LightShadows.None;
                    _villageLanterns.Add(l);
                }
                else if (lanternObj != null)
                {
                    var l = lanternObj.GetComponent<Light>();
                    if (l != null)
                    {
                        l.enabled = lanternsActive;
                        l.intensity = targetIntensity;
                        l.color = lanternColor;
                    }
                }
            }
        }
    }
}
