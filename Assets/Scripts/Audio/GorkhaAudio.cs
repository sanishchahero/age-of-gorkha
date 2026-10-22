using System;
using UnityEngine;

namespace Gorkha.Audio
{
    /// <summary>
    /// Procedural Real-time Audio Synthesizer for Gorkha: Rise of the Kingdom.
    /// Generates crisp, authentic sound effects dynamically at runtime via AudioClip.Create.
    /// Requires zero external .wav or .mp3 audio asset files.
    /// </summary>
    public class GorkhaAudio : MonoBehaviour
    {
        private static GorkhaAudio _instance;
        public static GorkhaAudio Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<GorkhaAudio>();
                    if (_instance == null && Application.isPlaying)
                    {
                        GameObject go = new GameObject("GorkhaAudio_Manager");
                        _instance = go.AddComponent<GorkhaAudio>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private AudioSource _sfxSource;
        private AudioSource _ambientSource;
        private bool _isMuted = false;

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                if (_sfxSource != null) _sfxSource.mute = _isMuted;
                if (_ambientSource != null) _ambientSource.mute = _isMuted;
            }
        }

        private AudioClip _chimeClip;
        private AudioClip _hammerClip;
        private AudioClip _fanfareClip;
        private AudioClip _hornClip;
        private AudioClip _clickClip;
        private AudioClip _gridSnapClip;
        private AudioClip _errorBuzzClip;
        private AudioClip _buildingBounceClip;
        private AudioClip _ambientWindClip;
        private AudioClip _arrowClip;
        private AudioClip _cannonClip;

        // Traditional Himalayan Instruments & Combat SFX
        private AudioClip _bansuriClip;
        private AudioClip _madalHighClip;
        private AudioClip _madalBassClip;
        private AudioClip _dhimayDrumClip;
        private AudioClip _sarangiClip;
        private AudioClip _swordClashClip;
        private AudioClip _shieldBlockClip;
        private AudioClip _bowTwangClip;
        private AudioClip _arrowImpactClip;
        private AudioClip _structureBreachClip;

        // Villager Gathering & Economy SFX (Phase 1 AoE + CoC)
        private AudioClip _woodChopClip;
        private AudioClip _stonePickClip;
        private AudioClip _wheatRustleClip;
        private AudioClip _depositClip;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // Guarantee exactly ONE active AudioListener in the scene so audio renders cleanly
            AudioListener[] allListeners = FindObjectsByType<AudioListener>();
            AudioListener primaryListener = null;

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                primaryListener = mainCam.GetComponent<AudioListener>();
                if (primaryListener == null) primaryListener = mainCam.gameObject.AddComponent<AudioListener>();
            }
            else if (allListeners.Length > 0)
            {
                primaryListener = allListeners[0];
            }
            else
            {
                primaryListener = gameObject.AddComponent<AudioListener>();
            }

            foreach (var l in allListeners)
            {
                if (l != null && l != primaryListener)
                {
                    if (Application.isPlaying) Destroy(l);
                    else DestroyImmediate(l);
                }
            }

            AudioListener.pause = false;
            AudioListener.volume = 1.0f;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f; // 2D crisp UI audio
            _sfxSource.volume = 1.0f;

            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.playOnAwake = false;
            _ambientSource.loop = true;
            _ambientSource.spatialBlend = 0f;
            _ambientSource.volume = 0.40f;

            GenerateAudioClips();
            StartAmbientWind();
        }

        private void GenerateAudioClips()
        {
            _chimeClip = CreateHarvestChimeClip();
            _hammerClip = CreateHammerImpactClip();
            _fanfareClip = CreateFanfareClip();
            _hornClip = CreateHornClip();
            _clickClip = CreateClickClip();
            _gridSnapClip = CreateGridSnapClip();
            _errorBuzzClip = CreateErrorBuzzClip();
            _buildingBounceClip = CreateBuildingBounceClip();
            _ambientWindClip = CreateAmbientBreezeClip();
            _arrowClip = CreateArrowShotClip();
            _cannonClip = CreateCannonShotClip();

            // Traditional Himalayan & Tactical Clips
            _bansuriClip = CreateBansuriFluteClip();
            _madalHighClip = CreateMadalHighSlapClip();
            _madalBassClip = CreateMadalBassThudClip();
            _dhimayDrumClip = CreateDhimayWarDrumClip();
            _sarangiClip = CreateSarangiRiffClip();
            _swordClashClip = CreateSwordClashClip();
            _shieldBlockClip = CreateShieldBlockClip();
            _bowTwangClip = CreateBowTwangClip();
            _arrowImpactClip = CreateArrowImpactClip();
            _structureBreachClip = CreateStructureBreachClip();

            // Phase 1 Villager & Storage Clips
            _woodChopClip = CreateWoodChopClip();
            _stonePickClip = CreateStonePickClip();
            _wheatRustleClip = CreateWheatRustleClip();
            _depositClip = CreateDepositClip();
        }

        public static void PlayWoodChop(float volume = 0.90f)
        {
            if (Instance != null && !Instance._isMuted && Instance._woodChopClip != null && Instance._sfxSource != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._woodChopClip, volume);
            }
        }

        public static void PlayStonePick(float volume = 0.90f)
        {
            if (Instance != null && !Instance._isMuted && Instance._stonePickClip != null && Instance._sfxSource != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._stonePickClip, volume);
            }
        }

        public static void PlayWheatRustle(float volume = 0.85f)
        {
            if (Instance != null && !Instance._isMuted && Instance._wheatRustleClip != null && Instance._sfxSource != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._wheatRustleClip, volume);
            }
        }

        public static void PlayDeposit(float volume = 0.95f)
        {
            if (Instance != null && !Instance._isMuted && Instance._depositClip != null && Instance._sfxSource != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._depositClip, volume);
            }
        }

        private void StartAmbientWind()
        {
            if (_ambientSource != null && _ambientWindClip != null)
            {
                _ambientSource.clip = _ambientWindClip;
                _ambientSource.Play();
            }
        }

        public static void PlayHarvest()
        {
            if (Instance != null && !Instance._isMuted && Instance._chimeClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._chimeClip, 0.95f);
            }
        }

        public static void PlayGridSnap()
        {
            if (Instance != null && !Instance._isMuted && Instance._gridSnapClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._gridSnapClip, 0.70f);
            }
        }

        public static void PlayError()
        {
            if (Instance != null && !Instance._isMuted && Instance._errorBuzzClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._errorBuzzClip, 0.85f);
            }
        }

        public static void PlayBuildingBounce()
        {
            if (Instance != null && !Instance._isMuted && Instance._buildingBounceClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._buildingBounceClip, 0.75f);
            }
        }

        public static void PlayBuild()
        {
            if (Instance != null && !Instance._isMuted && Instance._hammerClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._hammerClip, 0.95f);
            }
        }

        public static void PlayUpgrade()
        {
            if (Instance != null && !Instance._isMuted && Instance._fanfareClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._fanfareClip, 1.0f);
            }
        }

        public static void PlayRecruit()
        {
            if (Instance != null && !Instance._isMuted && Instance._hornClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._hornClip, 0.95f);
            }
        }

        public static void PlayClick()
        {
            if (Instance != null && !Instance._isMuted && Instance._clickClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._clickClip, 0.90f);
            }
        }

        public static void PlayArrowFire()
        {
            if (Instance != null && !Instance._isMuted && Instance._arrowClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._arrowClip, 0.85f);
            }
        }

        public static void PlayCannon()
        {
            if (Instance != null && !Instance._isMuted && Instance._cannonClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._cannonClip, 1.0f);
            }
        }

        public static void PlayBansuri(float volume = 0.85f)
        {
            if (Instance != null && !Instance._isMuted && Instance._bansuriClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._bansuriClip, volume);
            }
        }

        public static void PlayMadalHigh(float volume = 0.85f)
        {
            if (Instance != null && !Instance._isMuted && Instance._madalHighClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._madalHighClip, volume);
            }
        }

        public static void PlayMadalBass(float volume = 0.85f)
        {
            if (Instance != null && !Instance._isMuted && Instance._madalBassClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._madalBassClip, volume);
            }
        }

        public static void PlayDhimay(float volume = 0.95f)
        {
            if (Instance != null && !Instance._isMuted && Instance._dhimayDrumClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._dhimayDrumClip, volume);
            }
        }

        public static void PlaySarangi(float volume = 0.85f)
        {
            if (Instance != null && !Instance._isMuted && Instance._sarangiClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._sarangiClip, volume);
            }
        }

        public static void PlaySwordClash(float volume = 0.85f)
        {
            if (Instance != null && !Instance._isMuted && Instance._swordClashClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._swordClashClip, volume);
            }
        }

        public static void PlayShieldBlock(float volume = 0.85f)
        {
            if (Instance != null && !Instance._isMuted && Instance._shieldBlockClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._shieldBlockClip, volume);
            }
        }

        public static void PlayBowTwang(float volume = 0.80f)
        {
            if (Instance != null && !Instance._isMuted && Instance._bowTwangClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._bowTwangClip, volume);
            }
        }

        public static void PlayArrowImpact(float volume = 0.85f)
        {
            if (Instance != null && !Instance._isMuted && Instance._arrowImpactClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._arrowImpactClip, volume);
            }
        }

        public static void PlayStructureBreach(float volume = 0.95f)
        {
            if (Instance != null && !Instance._isMuted && Instance._structureBreachClip != null)
            {
                Instance._sfxSource.PlayOneShot(Instance._structureBreachClip, volume);
            }
        }

        #region Procedural Audio Synthesizers

        private AudioClip CreateHarvestChimeClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.45f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            // 3-note ascending chime: C6 (1046.5Hz), E6 (1318.5Hz), G6 (1567.9Hz)
            float[] freqs = { 1046.5f, 1318.5f, 1567.98f };
            float noteDuration = duration / 3f;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                int noteIndex = Mathf.Clamp((int)(t / noteDuration), 0, freqs.Length - 1);
                float noteT = t - (noteIndex * noteDuration);

                float freq = freqs[noteIndex];
                float envelope = Mathf.Exp(-noteT * 12.0f); // Quick exponential bell decay
                float sample = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope;
                // Add harmonic sheen
                sample += 0.35f * Mathf.Sin(4f * Mathf.PI * freq * t) * envelope;

                samples[i] = sample * 0.85f;
            }

            AudioClip clip = AudioClip.Create("HarvestChime", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateHammerImpactClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.22f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            System.Random rand = new System.Random(42);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 22f);

                // Woody pitch drop
                float freq = Mathf.Lerp(260f, 65f, t / duration);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 40f);

                samples[i] = (tone * 0.7f + noise * 0.3f) * env * 0.90f;
            }

            AudioClip clip = AudioClip.Create("HammerImpact", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateFanfareClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.65f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            // Royal fanfare chord: G4 (392Hz) -> C5 (523Hz) -> E5 (659Hz) -> G5 (784Hz)
            float[] freqs = { 392f, 523.25f, 659.25f, 783.99f };
            float step = duration / 4f;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                int idx = Mathf.Clamp((int)(t / step), 0, freqs.Length - 1);
                float localT = t - (idx * step);

                float freq = freqs[idx];
                float env = Mathf.Exp(-localT * (idx == freqs.Length - 1 ? 4f : 8f));
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.5f * Mathf.Sin(4f * Mathf.PI * freq * t);

                samples[i] = wave * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create("FanfareLevelUp", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateHornClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.55f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            // Gorkha Narasingha battle horn call: 220Hz (A3) with brassy rich harmonics
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / 0.1f) * Mathf.PI * 0.5f) * Mathf.Exp(-t * 3.5f);

                float baseFreq = 220f + Mathf.Sin(t * 15f) * 4f; // subtle vibrato
                float wave = Mathf.Sin(2f * Mathf.PI * baseFreq * t)
                           + 0.6f * Mathf.Sin(4f * Mathf.PI * baseFreq * t)
                           + 0.3f * Mathf.Sin(6f * Mathf.PI * baseFreq * t);

                samples[i] = wave * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create("GorkhaHorn", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateClickClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.04f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 120f);
                float wave = Mathf.Sin(2f * Mathf.PI * 1400f * t);
                samples[i] = wave * env * 0.80f;
            }

            AudioClip clip = AudioClip.Create("ButtonClick", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateGridSnapClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.025f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 220f);
                float wave = Mathf.Sin(2f * Mathf.PI * 2200f * t);
                samples[i] = wave * env * 0.75f;
            }

            AudioClip clip = AudioClip.Create("GridSnapTick", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateErrorBuzzClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.18f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float gate = Mathf.Sin(t * 35f) > 0 ? 1f : 0.2f;
                float wave = Mathf.Sin(2f * Mathf.PI * 165f * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * 330f * t);
                samples[i] = wave * gate * 0.80f;
            }

            AudioClip clip = AudioClip.Create("ErrorBuzz", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateBuildingBounceClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.08f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 30f);
                float freq = Mathf.Lerp(520f, 260f, t / duration);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                samples[i] = wave * env * 0.80f;
            }

            AudioClip clip = AudioClip.Create("BuildingBounce", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateAmbientBreezeClip()
        {
            const int sampleRate = 44100;
            const float duration = 4.0f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            System.Random rand = new System.Random(1337);
            float prev = 0f;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                // Soft low-pass filtered noise
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
                prev = prev * 0.94f + white * 0.06f;

                // Slow undulating wind swell
                float swell = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * (1f / duration) * t);

                // Subtle temple bowl resonance (216Hz - Himalayan singing bowl)
                float bowl = 0.08f * Mathf.Sin(2f * Mathf.PI * 216f * t);

                samples[i] = (prev * swell + bowl) * 0.60f;
            }

            AudioClip clip = AudioClip.Create("HimalayanAmbientBreeze", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateArrowShotClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.16f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(442);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(1200f, 320f, t / duration);
                float env = Mathf.Exp(-t * 22f);
                float noise = (float)(rand.NextDouble() * 0.35 - 0.175);
                samples[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) + noise) * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create("ArrowShot", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateCannonShotClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.85f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(999);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(120f, 38f, t / duration);
                float env = Mathf.Exp(-t * 5.5f);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 12f);
                samples[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.85f + noise * 0.65f) * env;
            }

            AudioClip clip = AudioClip.Create("CannonBlast", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateBansuriFluteClip()
        {
            const int sampleRate = 44100;
            const float duration = 2.4f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(777);

            // Raga Pahadi pentatonic melody: E5 (659Hz) -> G5 (784Hz) -> A5 (880Hz) -> D6 (1175Hz) -> E6 (1319Hz)
            float[] noteFreqs = { 659.25f, 783.99f, 880.00f, 1174.66f, 1318.51f };
            float[] noteDurations = { 0.45f, 0.45f, 0.45f, 0.50f, 0.55f };

            float prevNoise = 0f;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;

                // Determine active note
                float accum = 0f;
                int noteIdx = 0;
                float noteStart = 0f;
                for (int n = 0; n < noteDurations.Length; n++)
                {
                    if (t >= accum && t < accum + noteDurations[n])
                    {
                        noteIdx = n;
                        noteStart = accum;
                        break;
                    }
                    accum += noteDurations[n];
                }

                float noteT = t - noteStart;
                float dur = noteDurations[noteIdx];
                float baseFreq = noteFreqs[noteIdx];

                // Soft attack (0.06s) and gentle decay
                float attack = Mathf.Clamp01(noteT / 0.06f);
                float release = Mathf.Clamp01((dur - noteT) / 0.08f);
                float noteEnv = attack * release;

                // Expressive Himalayan bamboo vibrato (5.2Hz)
                float vibrato = Mathf.Sin(2f * Mathf.PI * 5.2f * t) * 0.016f;
                float currentFreq = baseFreq * (1f + vibrato);

                // Bansuri Overtones: Pure fundamental + mellow 2nd harmonic (octave) + warm 3rd
                float wave = Mathf.Sin(2f * Mathf.PI * currentFreq * t)
                           + 0.30f * Mathf.Sin(4f * Mathf.PI * currentFreq * t)
                           + 0.10f * Mathf.Sin(6f * Mathf.PI * currentFreq * t);

                // Breath turbulence (low-pass filtered white noise)
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
                prevNoise = prevNoise * 0.85f + white * 0.15f;
                float breath = prevNoise * 0.06f;

                samples[i] = (wave * 0.75f + breath) * noteEnv * 0.80f;
            }

            AudioClip clip = AudioClip.Create("BansuriMelody", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateMadalHighSlapClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.22f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(101);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 26f);

                // Treble head pitch drop: 420Hz down to 240Hz
                float freq = Mathf.Lerp(420f, 240f, t / duration);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);

                // Metallic overtone (iron/rice tuning paste "Khari" resonance at 1120Hz)
                float khari = Mathf.Sin(2f * Mathf.PI * 1120f * t) * Mathf.Exp(-t * 45f) * 0.45f;

                // Sharp initial skin slap crack
                float slap = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 80f) * 0.35f;

                samples[i] = (tone * 0.6f + khari + slap) * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create("MadalHighSlap", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateMadalBassThudClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.36f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 9.5f);

                // Deep bass pitch dive: 130Hz -> 52Hz
                float freq = Mathf.Lerp(130f, 52f, t / duration);
                float sub = Mathf.Sin(2f * Mathf.PI * freq * t);
                float harmonic = 0.35f * Mathf.Sin(4f * Mathf.PI * freq * t);

                samples[i] = (sub + harmonic) * env * 0.88f;
            }

            AudioClip clip = AudioClip.Create("MadalBassThud", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateDhimayWarDrumClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.85f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(303);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 4.8f);

                // Massive thunderous sub-bass sweep: 82Hz -> 34Hz
                float freq = Mathf.Lerp(82f, 34f, t / duration);
                float sub = Mathf.Sin(2f * Mathf.PI * freq * t);
                float body = 0.4f * Mathf.Sin(4f * Mathf.PI * freq * t);

                // Stick strike crack
                float stick = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 60f) * 0.45f;

                samples[i] = (sub * 0.85f + body + stick) * env * 0.90f;
            }

            AudioClip clip = AudioClip.Create("DhimayWarDrum", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateSarangiRiffClip()
        {
            const int sampleRate = 44100;
            const float duration = 1.6f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            // Sarangi sliding phrase: A4 (440Hz) -> C5 (523Hz) -> G4 (392Hz)
            float[] freqs = { 440.0f, 523.25f, 392.0f };
            float step = duration / 3f;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                int idx = Mathf.Clamp((int)(t / step), 0, freqs.Length - 1);
                float localT = t - (idx * step);

                float targetFreq = freqs[idx];
                // Bowed vibrato
                float vib = Mathf.Sin(2f * Mathf.PI * 6.0f * t) * 0.022f;
                float freq = targetFreq * (1f + vib);

                // Sawtooth-saw string approximation with body resonance
                float phase = (t * freq) % 1.0f;
                float saw = (phase * 2.0f - 1.0f);
                float body = Mathf.Sin(2f * Mathf.PI * 450f * t) * 0.35f;

                float env = Mathf.Sin(Mathf.Clamp01(localT / (step * 0.15f)) * Mathf.PI * 0.5f) * Mathf.Exp(-localT * 2.2f);

                samples[i] = (saw * 0.65f + body) * env * 0.80f;
            }

            AudioClip clip = AudioClip.Create("SarangiRiff", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateSwordClashClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.35f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(505);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 14f);

                // Cross-ringing steel modes: 3200Hz and 4850Hz
                float steel1 = Mathf.Sin(2f * Mathf.PI * 3200f * t);
                float steel2 = Mathf.Sin(2f * Mathf.PI * 4850f * t);
                float steelRing = (steel1 + 0.6f * steel2);

                // Blade scraping transient
                float scrape = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 55f);

                samples[i] = (steelRing * 0.7f + scrape * 0.4f) * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create("SwordClash", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateShieldBlockClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.24f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(606);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 20f);

                // Heavy timber body thump: 220Hz -> 110Hz
                float freq = Mathf.Lerp(220f, 110f, t / duration);
                float wood = Mathf.Sin(2f * Mathf.PI * freq * t);

                // Iron rim metallic tap: 1750Hz
                float iron = Mathf.Sin(2f * Mathf.PI * 1750f * t) * Mathf.Exp(-t * 40f) * 0.35f;

                // Splinter friction
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 45f) * 0.3f;

                samples[i] = (wood * 0.7f + iron + noise) * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create("ShieldBlock", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateBowTwangClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.20f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 22f);

                // Bowstring pluck: 520Hz -> 310Hz
                float freq = Mathf.Lerp(520f, 310f, t / duration);
                float twang = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.4f * Mathf.Sin(4f * Mathf.PI * freq * t);

                samples[i] = twang * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create("BowTwang", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateArrowImpactClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.14f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(808);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 32f);

                // Sharp penetration crack + wood/meat thud (240Hz -> 90Hz)
                float freq = Mathf.Lerp(240f, 90f, t / duration);
                float thud = Mathf.Sin(2f * Mathf.PI * freq * t);
                float crack = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 90f);

                samples[i] = (thud * 0.65f + crack * 0.55f) * env * 0.88f;
            }

            AudioClip clip = AudioClip.Create("ArrowImpact", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateStructureBreachClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.85f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(909);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 4.5f);

                // Deep stone rubble rumble: 55Hz -> 28Hz
                float freq = Mathf.Lerp(55f, 28f, t / duration);
                float rumble = Mathf.Sin(2f * Mathf.PI * freq * t);

                // Masonry crumbling crunch
                float crunch = (float)(rand.NextDouble() * 2.0 - 1.0) * (0.4f + 0.6f * Mathf.Sin(t * 25f));

                samples[i] = (rumble * 0.7f + crunch * 0.45f) * env * 0.90f;
            }

            AudioClip clip = AudioClip.Create("StructureBreach", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateWoodChopClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.18f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(101);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 26f);

                // Woody impact pitch drop: 280Hz -> 65Hz
                float freq = Mathf.Lerp(280f, 65f, t / duration);
                float woodTone = Mathf.Sin(2f * Mathf.PI * freq * t);
                float barkCrunch = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 55f);

                samples[i] = (woodTone * 0.70f + barkCrunch * 0.45f) * env * 0.92f;
            }

            AudioClip clip = AudioClip.Create("WoodChop", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateStonePickClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.22f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(202);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 18f);

                // Sharp metallic pickaxe ring (2700Hz + 1920Hz harmonic)
                float metalRing1 = Mathf.Sin(2f * Mathf.PI * 2700f * t);
                float metalRing2 = Mathf.Sin(2f * Mathf.PI * 1920f * t) * 0.6f;
                float flintCrack = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 80f);

                samples[i] = ((metalRing1 + metalRing2) * 0.65f + flintCrack * 0.45f) * env * 0.88f;
            }

            AudioClip clip = AudioClip.Create("StonePick", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateWheatRustleClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.32f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(303);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration)); // Smooth bell envelope

                // Grain stalk swish
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0);
                float gentleTone = Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.15f;

                samples[i] = (noise * 0.7f + gentleTone) * env * 0.70f;
            }

            AudioClip clip = AudioClip.Create("WheatRustle", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateDepositClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.45f;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            // Rising 3-tone arpeggio (C5 -> E5 -> G5) + heavy chest thud
            float[] chord = { 523.25f, 659.25f, 783.99f };
            float step = duration / 3f;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                int idx = Mathf.Clamp((int)(t / step), 0, chord.Length - 1);
                float localT = t - (idx * step);

                float freq = chord[idx];
                float chime = Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-localT * 9f);
                float chestThud = Mathf.Sin(2f * Mathf.PI * 90f * t) * Mathf.Exp(-t * 12f) * 0.5f;

                samples[i] = (chime * 0.8f + chestThud) * 0.90f;
            }

            AudioClip clip = AudioClip.Create("DepositResource", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        #endregion
    }
}
