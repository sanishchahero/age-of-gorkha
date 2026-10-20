using System.Collections;
using System.Linq;
using UnityEngine;
using Gorkha.Units;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gorkha.Audio
{
    /// <summary>
    /// Master soundscape director for Gorkha: Rise of the Kingdom.
    /// Manages:
    /// - Multi-tier environmental ambience: Village murmurs, alpine lake shoreline, height-blended mountain wind
    /// - Dynamic Tension Music: Peace mode (serene Bansuri flute melodies) vs Combat mode (110 BPM Madal war drum cadence)
    /// </summary>
    public class HimalayanSoundscape : MonoBehaviour
    {
        private static HimalayanSoundscape _instance;
        public static HimalayanSoundscape Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<HimalayanSoundscape>();
                    if (_instance == null && Application.isPlaying)
                    {
                        GameObject go = new GameObject("HimalayanSoundscape_Manager");
                        _instance = go.AddComponent<HimalayanSoundscape>();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private AudioSource _villageAmbienceSource;
        private AudioSource _watershoreSource;
        private AudioSource _mountainWindSource;

        private bool _isInCombat = false;
        private Coroutine _musicRoutine;
        private Coroutine _madalWarDrumRoutine;

        private AudioClip _villageCalmClip;
        private AudioClip _villageBusyClip;
        private AudioClip _watershoreClip;
        private AudioClip _windHighClip;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            SetupAudioSources();
            LoadAmbientAudioClips();
        }

        private void Start()
        {
            StartAmbience();
            if (Application.isPlaying)
            {
                _musicRoutine = StartCoroutine(DynamicMusicDirectorRoutine());
            }
        }

        private void SetupAudioSources()
        {
            _villageAmbienceSource = gameObject.AddComponent<AudioSource>();
            _villageAmbienceSource.playOnAwake = false;
            _villageAmbienceSource.loop = true;
            _villageAmbienceSource.volume = 0.40f;
            _villageAmbienceSource.spatialBlend = 0f;

            _watershoreSource = gameObject.AddComponent<AudioSource>();
            _watershoreSource.playOnAwake = false;
            _watershoreSource.loop = true;
            _watershoreSource.volume = 0.0f;
            _watershoreSource.spatialBlend = 0f;

            _mountainWindSource = gameObject.AddComponent<AudioSource>();
            _mountainWindSource.playOnAwake = false;
            _mountainWindSource.loop = true;
            _mountainWindSource.volume = 0.35f;
            _mountainWindSource.spatialBlend = 0f;
        }

        private void LoadAmbientAudioClips()
        {
#if UNITY_EDITOR
            _villageCalmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioFiles/03_amb/village_calm.wav");
            _villageBusyClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioFiles/03_amb/village_busy.wav");
            _watershoreClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioFiles/03_amb/watershore_amb.wav");
            _windHighClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioFiles/03_amb/skywind_1.wav");
#endif
        }

        private void StartAmbience()
        {
            if (_villageAmbienceSource != null && _villageCalmClip != null)
            {
                _villageAmbienceSource.clip = _villageCalmClip;
                _villageAmbienceSource.Play();
            }

            if (_watershoreSource != null && _watershoreClip != null)
            {
                _watershoreSource.clip = _watershoreClip;
                _watershoreSource.Play();
            }

            if (_mountainWindSource != null && _windHighClip != null)
            {
                _mountainWindSource.clip = _windHighClip;
                _mountainWindSource.Play();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            // 1. Lake watershore spatial blend (Western flank X < 10)
            if (_watershoreSource != null)
            {
                float camX = cam.transform.position.x;
                float lakeProximity = Mathf.Clamp01(1f - (camX - (-4f)) / 18f);
                _watershoreSource.volume = Mathf.Lerp(_watershoreSource.volume, lakeProximity * 0.35f, 2f * Time.deltaTime);
            }

            // 2. High Altitude Mountain Wind Blend (Camera Y > 12)
            if (_mountainWindSource != null)
            {
                float camY = cam.transform.position.y;
                float heightFactor = Mathf.Clamp01((camY - 8f) / 22f);
                _mountainWindSource.volume = Mathf.Lerp(_mountainWindSource.volume, 0.08f + heightFactor * 0.30f, 2f * Time.deltaTime);
            }

            // 3. Dynamic Village Ambience (Calm vs Busy based on active population)
            if (_villageAmbienceSource != null && _villageBusyClip != null && _villageCalmClip != null)
            {
                int livingVillagers = UnitController.AllUnits.Count(u => u != null && u.IsVillager && u.IsAlive);
                AudioClip targetClip = livingVillagers >= 3 ? _villageBusyClip : _villageCalmClip;
                if (_villageAmbienceSource.clip != targetClip)
                {
                    _villageAmbienceSource.clip = targetClip;
                    _villageAmbienceSource.Play();
                }
            }

            // 4. Track Combat vs Peacetime State
            bool hasActiveHostiles = UnitController.AllUnits.Any(u => u != null && u.IsHostile && u.IsAlive);
            if (hasActiveHostiles != _isInCombat)
            {
                SetCombatState(hasActiveHostiles);
            }
        }

        private void SetCombatState(bool inCombat)
        {
            _isInCombat = inCombat;

            if (_isInCombat)
            {
                // Trigger battle war drum alert
                GorkhaAudio.PlayDhimay(1.0f);
                if (_madalWarDrumRoutine != null) StopCoroutine(_madalWarDrumRoutine);
                _madalWarDrumRoutine = StartCoroutine(MadalWarDrumRoutine());
            }
            else
            {
                if (_madalWarDrumRoutine != null)
                {
                    StopCoroutine(_madalWarDrumRoutine);
                    _madalWarDrumRoutine = null;
                }
                // Victory Fanfare when village threat is cleared
                GorkhaAudio.PlayUpgrade();
            }
        }

        private IEnumerator DynamicMusicDirectorRoutine()
        {
            // Initial opening bansuri melody shortly after scene start
            yield return new WaitForSeconds(2.0f);
            if (!_isInCombat && Application.isPlaying)
            {
                GorkhaAudio.PlayBansuri(0.95f);
            }

            while (Application.isPlaying)
            {
                if (!_isInCombat)
                {
                    // Peacetime: play melodic Bansuri flute run every 18-26 seconds
                    yield return new WaitForSeconds(Random.Range(18f, 26f));
                    if (!_isInCombat && Application.isPlaying)
                    {
                        GorkhaAudio.PlayBansuri(0.90f);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(2.0f);
                }
            }
        }

        /// <summary>
        /// Authentic Nepali folk war rhythm (110 BPM):
        /// Alternates resonant Madal high slaps and low sub-bass thuds.
        /// </summary>
        private IEnumerator MadalWarDrumRoutine()
        {
            const float beatDuration = 60f / 110f; // ~0.545 seconds per beat

            while (_isInCombat && Application.isPlaying)
            {
                // Beat 1: Bass thud (Bayan)
                GorkhaAudio.PlayMadalBass(0.90f);
                yield return new WaitForSeconds(beatDuration * 0.5f);

                // Beat 1.5: High slap (Dayan)
                GorkhaAudio.PlayMadalHigh(0.75f);
                yield return new WaitForSeconds(beatDuration * 0.5f);

                // Beat 2: Double tap high slap
                GorkhaAudio.PlayMadalHigh(0.85f);
                yield return new WaitForSeconds(beatDuration * 0.25f);
                GorkhaAudio.PlayMadalHigh(0.65f);
                yield return new WaitForSeconds(beatDuration * 0.75f);

                // Beat 3: Bass thud + high slap sync
                GorkhaAudio.PlayMadalBass(0.95f);
                yield return new WaitForSeconds(beatDuration * 0.5f);
                GorkhaAudio.PlayMadalHigh(0.80f);
                yield return new WaitForSeconds(beatDuration * 0.5f);
            }
        }
    }
}
