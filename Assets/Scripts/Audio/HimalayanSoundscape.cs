using System;
using System.Collections;
using System.Collections.Generic;
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
    /// - Alpine wildlife & temple life: Distant temple bell tolling, chirping pine forest birds, pastoral farm sounds
    /// - Dynamic Tension Music: Peace mode (serene Bansuri flute melodies & Sarangi strings) vs Combat mode (110 BPM Madal war drum cadence & Dhimay alerts)
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
        private AudioSource _waterfallSource;
        private AudioSource _mountainWindSource;
        private AudioSource _ambientEventSource;

        private bool _isInCombat = false;
        private Coroutine _musicRoutine;
        private Coroutine _madalWarDrumRoutine;
        private Coroutine _templeChimeRoutine;
        private Coroutine _pineBirdsRoutine;
        private Coroutine _pastoralLifeRoutine;

        // Continuous Ambience Clips
        private AudioClip _villageCalmClip;
        private AudioClip _villageBusyClip;
        private AudioClip _watershoreClip;
        private AudioClip _waterfallClip;
        private AudioClip _windHighClip;

        // One-Shot Ambient Life Clips
        private AudioClip _templeChimeClip;
        private readonly List<AudioClip> _birdClips = new();
        private readonly List<AudioClip> _pastoralClips = new();

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
                _templeChimeRoutine = StartCoroutine(TempleChimeRoutine());
                _pineBirdsRoutine = StartCoroutine(PineBirdsRoutine());
                _pastoralLifeRoutine = StartCoroutine(PastoralLifeRoutine());
            }
        }

        private void SetupAudioSources()
        {
            _villageAmbienceSource = gameObject.AddComponent<AudioSource>();
            _villageAmbienceSource.playOnAwake = false;
            _villageAmbienceSource.loop = true;
            _villageAmbienceSource.volume = 0.35f;
            _villageAmbienceSource.spatialBlend = 0f;

            _watershoreSource = gameObject.AddComponent<AudioSource>();
            _watershoreSource.playOnAwake = false;
            _watershoreSource.loop = true;
            _watershoreSource.volume = 0.0f;
            _watershoreSource.spatialBlend = 0f;

            _waterfallSource = gameObject.AddComponent<AudioSource>();
            _waterfallSource.playOnAwake = false;
            _waterfallSource.loop = true;
            _waterfallSource.volume = 0.0f;
            _waterfallSource.spatialBlend = 0f;

            _mountainWindSource = gameObject.AddComponent<AudioSource>();
            _mountainWindSource.playOnAwake = false;
            _mountainWindSource.loop = true;
            _mountainWindSource.volume = 0.30f;
            _mountainWindSource.spatialBlend = 0f;

            _ambientEventSource = gameObject.AddComponent<AudioSource>();
            _ambientEventSource.playOnAwake = false;
            _ambientEventSource.spatialBlend = 0.25f;
            _ambientEventSource.volume = 0.65f;
        }

        private void LoadAmbientAudioClips()
        {
            // Continuous Background Ambience
            _villageCalmClip = LoadAudioClip("Assets/Audio/AudioFiles/03_amb/village_calm.wav");
            _villageBusyClip = LoadAudioClip("Assets/Audio/AudioFiles/03_amb/village_busy.wav");
            _watershoreClip = LoadAudioClip("Assets/Audio/AudioFiles/03_amb/watershore_amb.wav");
            _waterfallClip = LoadAudioClip("Assets/Audio/AudioFiles/08_water/wasserfall.wav");
            _windHighClip = LoadAudioClip("Assets/Audio/AudioFiles/03_amb/skywind_1.wav");

            // Distant Temple Chime
            _templeChimeClip = LoadAudioClip("Assets/Audio/AudioFiles/05_sfx/church.wav");

            // Pine Forest Birds (9 distinct mountain bird sounds)
            for (int i = 1; i <= 9; i++)
            {
                AudioClip bird = LoadAudioClip($"Assets/Audio/AudioFiles/06_birds/bird_small_{i}.wav");
                if (bird != null) _birdClips.Add(bird);
            }

            // Pastoral Farm & Village Walla
            AudioClip rooster = LoadAudioClip("Assets/Audio/AudioFiles/07_animals/rooster1_temp.wav");
            if (rooster != null) _pastoralClips.Add(rooster);
            AudioClip cow = LoadAudioClip("Assets/Audio/AudioFiles/07_animals/cow.wav");
            if (cow != null) _pastoralClips.Add(cow);
            AudioClip walla = LoadAudioClip("Assets/Audio/AudioFiles/02_walla/walla_1.wav");
            if (walla != null) _pastoralClips.Add(walla);
            AudioClip kids = LoadAudioClip("Assets/Audio/AudioFiles/02_walla/kids1_temp.wav");
            if (kids != null) _pastoralClips.Add(kids);
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

            if (_waterfallSource != null && _waterfallClip != null)
            {
                _waterfallSource.clip = _waterfallClip;
                _waterfallSource.Play();
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

            // 1. Lake & Waterfall watershore spatial blend (Western flank X < 12)
            float camX = cam.transform.position.x;
            float lakeProximity = Mathf.Clamp01(1f - (camX - (-2f)) / 20f);

            if (_watershoreSource != null)
            {
                _watershoreSource.volume = Mathf.Lerp(_watershoreSource.volume, lakeProximity * 0.35f, 2f * Time.deltaTime);
            }
            if (_waterfallSource != null)
            {
                _waterfallSource.volume = Mathf.Lerp(_waterfallSource.volume, lakeProximity * 0.25f, 2f * Time.deltaTime);
            }

            // 2. High Altitude Mountain Wind Blend (Camera Y > 10)
            if (_mountainWindSource != null)
            {
                float camY = cam.transform.position.y;
                float heightFactor = Mathf.Clamp01((camY - 8f) / 22f);
                _mountainWindSource.volume = Mathf.Lerp(_mountainWindSource.volume, 0.08f + heightFactor * 0.32f, 2f * Time.deltaTime);
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
                // Trigger battle war drum alert & town warning bark
                GorkhaAudio.PlayDhimay(1.0f);
                GorkhaVoiceBarks.PlayTownAlertBark(new Vector3(24f, 2f, 24f));

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

        #region Ambient Environmental Coroutines

        /// <summary>
        /// Distant Temple Bells / Chimes echoing across the valley every 45-75 seconds.
        /// </summary>
        private IEnumerator TempleChimeRoutine()
        {
            yield return new WaitForSeconds(6.0f); // Opening blessing bell

            while (Application.isPlaying)
            {
                if (!_isInCombat && _templeChimeClip != null && _ambientEventSource != null && (GorkhaAudio.Instance == null || !GorkhaAudio.Instance.IsMuted))
                {
                    _ambientEventSource.PlayOneShot(_templeChimeClip, 0.40f);
                }
                yield return new WaitForSeconds(UnityEngine.Random.Range(45f, 75f));
            }
        }

        /// <summary>
        /// Alpine Pine Forest Birds chirping randomly every 14-24 seconds.
        /// </summary>
        private IEnumerator PineBirdsRoutine()
        {
            yield return new WaitForSeconds(4.0f);

            while (Application.isPlaying)
            {
                if (!_isInCombat && _birdClips.Count > 0 && _ambientEventSource != null && (GorkhaAudio.Instance == null || !GorkhaAudio.Instance.IsMuted))
                {
                    AudioClip bird = _birdClips[UnityEngine.Random.Range(0, _birdClips.Count)];
                    _ambientEventSource.PlayOneShot(bird, UnityEngine.Random.Range(0.25f, 0.45f));
                }
                yield return new WaitForSeconds(UnityEngine.Random.Range(14f, 24f));
            }
        }

        /// <summary>
        /// Pastoral farm animals and village chatter during calm peacetime every 35-60 seconds.
        /// </summary>
        private IEnumerator PastoralLifeRoutine()
        {
            yield return new WaitForSeconds(10.0f);

            while (Application.isPlaying)
            {
                if (!_isInCombat && _pastoralClips.Count > 0 && _ambientEventSource != null && (GorkhaAudio.Instance == null || !GorkhaAudio.Instance.IsMuted))
                {
                    AudioClip clip = _pastoralClips[UnityEngine.Random.Range(0, _pastoralClips.Count)];
                    _ambientEventSource.PlayOneShot(clip, UnityEngine.Random.Range(0.20f, 0.35f));
                }
                yield return new WaitForSeconds(UnityEngine.Random.Range(35f, 60f));
            }
        }

        /// <summary>
        /// Peacetime Traditional Nepali Music Director:
        /// Alternates lyrical Bansuri flute runs and soulful Sarangi strings every 20-30 seconds.
        /// </summary>
        private IEnumerator DynamicMusicDirectorRoutine()
        {
            // Initial opening bansuri melody shortly after scene start
            yield return new WaitForSeconds(2.0f);
            if (!_isInCombat && Application.isPlaying)
            {
                GorkhaAudio.PlayBansuri(0.95f);
            }

            int phraseCounter = 0;
            while (Application.isPlaying)
            {
                if (!_isInCombat)
                {
                    yield return new WaitForSeconds(UnityEngine.Random.Range(20f, 30f));
                    if (!_isInCombat && Application.isPlaying)
                    {
                        phraseCounter++;
                        if (phraseCounter % 2 == 1)
                        {
                            GorkhaAudio.PlayBansuri(0.90f);
                        }
                        else
                        {
                            GorkhaAudio.PlaySarangi(0.85f);
                        }
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
        /// Alternates resonant Madal high slaps (Dayan) and low sub-bass thuds (Bayan).
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

        #endregion

        #region Audio Loader Helper

        private static AudioClip LoadAudioClip(string relativeAssetPath)
        {
#if UNITY_EDITOR
            AudioClip asset = AssetDatabase.LoadAssetAtPath<AudioClip>(relativeAssetPath);
            if (asset != null) return asset;
#endif
            string stripped = relativeAssetPath.StartsWith("Assets/") ? relativeAssetPath.Substring(7) : relativeAssetPath;
            string fullPath = System.IO.Path.Combine(Application.dataPath, stripped);
            if (!System.IO.File.Exists(fullPath)) return null;

            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
                if (bytes.Length < 44) return null;
                int channels = BitConverter.ToInt16(bytes, 22);
                int sampleRate = BitConverter.ToInt32(bytes, 24);
                int bitsPerSample = BitConverter.ToInt16(bytes, 34);

                int dataIndex = 12;
                while (dataIndex < bytes.Length - 8)
                {
                    string chunkId = System.Text.Encoding.ASCII.GetString(bytes, dataIndex, 4);
                    int chunkSize = BitConverter.ToInt32(bytes, dataIndex + 4);
                    if (chunkId == "data")
                    {
                        dataIndex += 8;
                        int bytesPerSample = bitsPerSample / 8;
                        int sampleCount = chunkSize / bytesPerSample / channels;
                        float[] floatSamples = new float[sampleCount * channels];
                        for (int i = 0; i < floatSamples.Length; i++)
                        {
                            short s = BitConverter.ToInt16(bytes, dataIndex + i * 2);
                            floatSamples[i] = s / 32768f;
                        }
                        AudioClip clip = AudioClip.Create(System.IO.Path.GetFileNameWithoutExtension(fullPath), sampleCount, channels, sampleRate, false);
                        clip.SetData(floatSamples, 0);
                        return clip;
                    }
                    dataIndex += 8 + chunkSize;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HimalayanSoundscape] Failed to load {fullPath}: {ex.Message}");
            }
            return null;
        }

        #endregion
    }
}
