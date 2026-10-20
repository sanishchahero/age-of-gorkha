using System;
using UnityEngine;
using Gorkha.Units;
using Gorkha.UI;

namespace Gorkha.Audio
{
    public enum UnitBarkType
    {
        Select,
        MoveOrder,
        AttackOrder,
        HarvestOrder,
        DepositSuccess,
        UnderAttack
    }

    /// <summary>
    /// Authentic Gorkha RTS Voice Barks & Speech System (Age of Empires style).
    /// Generates vocalized phonetic audio responses paired with floating speech balloons
    /// in authentic Nepali (Devanagari script + English translation).
    /// </summary>
    public class GorkhaVoiceBarks : MonoBehaviour
    {
        private static GorkhaVoiceBarks _instance;
        public static GorkhaVoiceBarks Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<GorkhaVoiceBarks>();
                    if (_instance == null && Application.isPlaying)
                    {
                        GameObject go = new GameObject("GorkhaVoiceBarks_Manager");
                        _instance = go.AddComponent<GorkhaVoiceBarks>();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private AudioSource _voiceSource;
        private float _lastBarkTime = -99f;
        private const float BarkCooldown = 0.55f;

        private AudioClip _clipHajur;
        private AudioClip _clipAadesh;
        private AudioClip _clipHunchha;
        private AudioClip _clipAakraman;
        private AudioClip _clipJaiMahakali;
        private AudioClip _clipKaamSuru;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            _voiceSource = gameObject.AddComponent<AudioSource>();
            _voiceSource.playOnAwake = false;
            _voiceSource.spatialBlend = 0f;
            _voiceSource.volume = 1.0f;

            GenerateVoiceClips();
        }

        private void GenerateVoiceClips()
        {
            _clipHajur = CreateVocalFormantClip(pitch: 135f, duration: 0.38f, formant1: 750f, formant2: 1200f, vowelShift: 0.65f); // "Ha-jur"
            _clipAadesh = CreateVocalFormantClip(pitch: 140f, duration: 0.42f, formant1: 820f, formant2: 1850f, vowelShift: 0.45f); // "Aa-desh"
            _clipHunchha = CreateVocalFormantClip(pitch: 130f, duration: 0.35f, formant1: 380f, formant2: 950f, vowelShift: 0.50f);  // "Hun-chha"
            _clipAakraman = CreateVocalFormantClip(pitch: 165f, duration: 0.48f, formant1: 850f, formant2: 1300f, vowelShift: 0.70f); // "Aa-kra-man" (War Cry)
            _clipJaiMahakali = CreateVocalFormantClip(pitch: 175f, duration: 0.55f, formant1: 800f, formant2: 1400f, vowelShift: 0.80f); // "Jai Mahakali"
            _clipKaamSuru = CreateVocalFormantClip(pitch: 125f, duration: 0.36f, formant1: 700f, formant2: 1100f, vowelShift: 0.55f); // "Kaam Suru"
        }

        public static void PlaySelectBark(UnitController unit)
        {
            if (Instance == null || unit == null || !unit.IsAlive || unit.IsHostile) return;
            if (Time.time - Instance._lastBarkTime < BarkCooldown) return;
            Instance._lastBarkTime = Time.time;

            int roll = UnityEngine.Random.Range(0, 3);
            string devanagari;
            AudioClip clip;

            switch (roll)
            {
                case 0:
                    devanagari = "🇳🇵 \"हजुर!\" (Hajur! - Yes, Sire!)";
                    clip = Instance._clipHajur;
                    break;
                case 1:
                    devanagari = "🇳🇵 \"आदेश दिनुहोस्!\" (Aadesh! - Command me!)";
                    clip = Instance._clipAadesh;
                    break;
                default:
                    devanagari = "🇳🇵 \"तयार छु!\" (Tayar chhu! - Ready!)";
                    clip = Instance._clipHunchha;
                    break;
            }

            Instance.PlayClip(clip);
            GorkhaPlayableHUD.SpawnFloatingText(unit.transform.position + Vector3.up * 2.1f, devanagari, Color.cyan);
        }

        public static void PlayMoveOrderBark(UnitController unit, Vector3 destination)
        {
            if (Instance == null || unit == null || !unit.IsAlive || unit.IsHostile) return;
            if (Time.time - Instance._lastBarkTime < BarkCooldown) return;
            Instance._lastBarkTime = Time.time;

            int roll = UnityEngine.Random.Range(0, 2);
            string devanagari = roll == 0 ? "🇳🇵 \"हुन्छ!\" (Hunchha! - Understood!)" : "🇳🇵 \"अगाडि बढौँ!\" (Aagadi! - Marching!)";

            Instance.PlayClip(Instance._clipHunchha);
            GorkhaPlayableHUD.SpawnFloatingText(unit.transform.position + Vector3.up * 2.1f, devanagari, new Color(0.35f, 0.95f, 0.45f));
        }

        public static void PlayAttackOrderBark(UnitController unit)
        {
            if (Instance == null || unit == null || !unit.IsAlive || unit.IsHostile) return;
            if (Time.time - Instance._lastBarkTime < 0.9f) return;
            Instance._lastBarkTime = Time.time;

            bool isCry = UnityEngine.Random.value > 0.5f;
            string devanagari = isCry ? "⚔️ 🇳🇵 \"जय महाकाली!\" (Jai Mahakali!)" : "⚔️ 🇳🇵 \"आक्रमण!\" (Aakraman! - Attack!)";
            AudioClip clip = isCry ? Instance._clipJaiMahakali : Instance._clipAakraman;

            Instance.PlayClip(clip);
            GorkhaAudio.PlayDhimay(0.9f); // Thunderous war drum accompanying war cry!
            GorkhaPlayableHUD.SpawnFloatingText(unit.transform.position + Vector3.up * 2.3f, devanagari, new Color(1.0f, 0.28f, 0.22f));
        }

        public static void PlayHarvestOrderBark(UnitController unit)
        {
            if (Instance == null || unit == null || !unit.IsAlive || unit.IsHostile) return;
            if (Time.time - Instance._lastBarkTime < BarkCooldown) return;
            Instance._lastBarkTime = Time.time;

            Instance.PlayClip(Instance._clipKaamSuru);
            GorkhaPlayableHUD.SpawnFloatingText(unit.transform.position + Vector3.up * 2.1f, "🇳🇵 \"काम सुरु!\" (Kaam suru! - Starting work!)", new Color(1.0f, 0.85f, 0.25f));
        }

        public static void PlayDepositSuccessBark(UnitController unit, int amount, string resourceName)
        {
            if (Instance == null || unit == null || !unit.IsAlive || unit.IsHostile) return;

            Instance.PlayClip(Instance._clipHunchha);
            GorkhaPlayableHUD.SpawnFloatingText(unit.transform.position + Vector3.up * 2.1f, $"🇳🇵 \"भण्डारण सफल!\" (+{amount} {resourceName})", Color.yellow);
        }

        private void PlayClip(AudioClip clip)
        {
            if (_voiceSource != null && clip != null && (GorkhaAudio.Instance == null || !GorkhaAudio.Instance.IsMuted))
            {
                _voiceSource.PlayOneShot(clip, 0.95f);
            }
        }

        #region Formant Voice Synthesis

        /// <summary>
        /// Acoustic human voice synthesizer:
        /// Uses vocal-tract formant filtering (F1/F2 bandpass) driven by an impulse glottal excitation pulse.
        /// </summary>
        private AudioClip CreateVocalFormantClip(float pitch, float duration, float formant1, float formant2, float vowelShift)
        {
            const int sampleRate = 44100;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            var rand = new System.Random(pitch.GetHashCode());

            float phase = 0f;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                // Natural human pitch inflection (pitch glides slightly down at end of word)
                float currentPitch = pitch * (1.0f + 0.08f * Mathf.Sin(progress * Mathf.PI) - 0.12f * progress);
                phase += currentPitch / sampleRate;
                if (phase > 1f) phase -= 1f;

                // Glottal excitation pulse (Rosenberg glottal model approximation)
                float glottal = (phase < 0.65f)
                    ? 0.5f * (1f - Mathf.Cos(phase / 0.65f * Mathf.PI))
                    : Mathf.Exp(-(phase - 0.65f) * 14f);

                // Dynamic Formant Shift across syllables
                float f1 = Mathf.Lerp(formant1, formant1 * (1f - vowelShift * 0.4f), progress);
                float f2 = Mathf.Lerp(formant2, formant2 * (1f + vowelShift * 0.3f), progress);

                // Vocal tract resonators (F1 & F2)
                float resonanceF1 = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.70f;
                float resonanceF2 = Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.35f;

                // Soft unvoiced breath consonant transient at beginning ("H", "K", "A")
                float consonant = (t < 0.045f) ? (float)(rand.NextDouble() * 2.0 - 1.0) * (1f - t / 0.045f) * 0.35f : 0f;

                // Word envelope (smooth rise and soft decay)
                float attack = Mathf.Clamp01(t / 0.04f);
                float release = Mathf.Clamp01((duration - t) / 0.07f);
                float wordEnv = attack * release;

                samples[i] = (glottal * (resonanceF1 + resonanceF2) + consonant) * wordEnv * 0.85f;
            }

            AudioClip clip = AudioClip.Create($"VoiceBark_{pitch}_{formant1}", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        #endregion
    }
}
