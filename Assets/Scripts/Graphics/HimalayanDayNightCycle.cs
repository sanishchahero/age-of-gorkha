using UnityEngine;

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
    /// Features:
    /// - Continuous or stepped planetary rotation of the Himalayan Sun.
    /// - Golden-hour amber sunset, crystal alpine noon, and starry indigo night.
    /// - Dynamic Trilight Ambient & Mountain Mist Fog tinting.
    /// - Nighttime torch & lantern illumination for kingdom buildings.
    /// </summary>
    public class HimalayanDayNightCycle : MonoBehaviour
    {
        public static HimalayanDayNightCycle Instance { get; private set; }

        [Header("Time Configuration")]
        [SerializeField] private bool _autoAdvance = true;
        [SerializeField] private float _cycleDurationMinutes = 8.0f; // 8 minutes full day-night cycle
        [SerializeField] private TimeOfDay _currentTime = TimeOfDay.Noon;

        private float _timeProgress = 0.35f; // 0.0 = Midnight, 0.25 = Dawn, 0.50 = Noon, 0.75 = Sunset
        private Light _sunLight;

        public TimeOfDay CurrentTime => _currentTime;
        public float TimeProgress => _timeProgress;

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

        private void ApplyLightingForProgress(float t)
        {
            FindSunLight();
            if (_sunLight == null) return;

            // Calculate Sun Angle: 0 = midnight, 0.25 = sunrise (0 deg), 0.50 = noon (90 deg), 0.75 = sunset (180 deg)
            float sunAngle = (t * 360f) - 90f;
            _sunLight.transform.rotation = Quaternion.Euler(Mathf.Clamp(Mathf.Sin(t * Mathf.PI * 2f - Mathf.PI * 0.5f) * 60f, -40f, 65f), -35f + Mathf.Cos(t * Mathf.PI * 2f) * 20f, 0f);

            // Time Categories
            Color sunColor;
            float sunIntensity;
            Color skyAmbient;
            Color equatorAmbient;
            Color groundAmbient;
            Color fogColor;

            if (t >= 0.20f && t < 0.35f)
            {
                // Dawn (Cool alpine blue to golden peach)
                _currentTime = TimeOfDay.Dawn;
                sunColor = new Color(1.0f, 0.85f, 0.65f);
                sunIntensity = 0.85f;
                skyAmbient = new Color(0.55f, 0.65f, 0.85f);
                equatorAmbient = new Color(0.85f, 0.65f, 0.55f);
                groundAmbient = new Color(0.20f, 0.25f, 0.20f);
                fogColor = new Color(0.70f, 0.72f, 0.80f);
            }
            else if (t >= 0.35f && t < 0.65f)
            {
                // Noon (Crisp crystal alpine sunshine)
                _currentTime = TimeOfDay.Noon;
                sunColor = new Color(1.0f, 0.96f, 0.90f);
                sunIntensity = 1.20f;
                skyAmbient = new Color(0.62f, 0.76f, 0.92f);
                equatorAmbient = new Color(0.76f, 0.70f, 0.60f);
                groundAmbient = new Color(0.25f, 0.36f, 0.20f);
                fogColor = new Color(0.68f, 0.76f, 0.86f);
            }
            else if (t >= 0.65f && t < 0.80f)
            {
                // Sunset (Golden hour & crimson Himalayan alpenglow)
                _currentTime = TimeOfDay.Sunset;
                sunColor = new Color(1.0f, 0.60f, 0.30f);
                sunIntensity = 0.95f;
                skyAmbient = new Color(0.75f, 0.50f, 0.65f);
                equatorAmbient = new Color(0.85f, 0.45f, 0.30f);
                groundAmbient = new Color(0.22f, 0.18f, 0.18f);
                fogColor = new Color(0.80f, 0.55f, 0.45f);
            }
            else
            {
                // Night (Starry indigo mountain night)
                _currentTime = TimeOfDay.Night;
                sunColor = new Color(0.25f, 0.35f, 0.60f);
                sunIntensity = 0.22f;
                skyAmbient = new Color(0.12f, 0.16f, 0.28f);
                equatorAmbient = new Color(0.15f, 0.18f, 0.25f);
                groundAmbient = new Color(0.08f, 0.10f, 0.12f);
                fogColor = new Color(0.12f, 0.14f, 0.22f);
            }

            _sunLight.color = sunColor;
            _sunLight.intensity = sunIntensity;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = skyAmbient;
            RenderSettings.ambientEquatorColor = equatorAmbient;
            RenderSettings.ambientGroundColor = groundAmbient;
            RenderSettings.fogColor = fogColor;
        }
    }
}
