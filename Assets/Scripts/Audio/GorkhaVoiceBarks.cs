using System;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Units;
using Gorkha.UI;
using Gorkha.Economy;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gorkha.Audio
{
    public enum UnitBarkType
    {
        Select,
        MoveOrder,
        AttackOrder,
        HarvestOrder,
        DepositSuccess,
        UnderAttack,
        TownAlert
    }

    /// <summary>
    /// Authentic Gorkha RTS Voice Barks & Speech System (Age of Empires / Total War style).
    /// Plays authentic, pitch-shifted masculine Nepali voice lines paired with floating Devanagari speech tags.
    /// Features fallback procedural formant synthesis if audio assets are missing.
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
        private const float BarkCooldown = 0.50f;

        // Selection Clips
        private readonly List<(AudioClip clip, string text)> _selectionBarks = new();
        // Move Clips
        private readonly List<(AudioClip clip, string text)> _moveBarks = new();
        // Attack Clips
        private readonly List<(AudioClip clip, string text)> _attackBarks = new();
        // Gathering Clips
        private AudioClip _clipGatherWood;
        private AudioClip _clipGatherStone;
        private AudioClip _clipGatherFood;
        private AudioClip _clipKaamSuru;
        // Town Alert Clips
        private AudioClip _clipShatruAayo;
        private AudioClip _clipSuraksha;

        // Procedural Fallback Clips
        private AudioClip _fallbackHajur;
        private AudioClip _fallbackAadesh;
        private AudioClip _fallbackHunchha;
        private AudioClip _fallbackAakraman;

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

            LoadOrSynthesizeVoiceClips();
        }

        private void LoadOrSynthesizeVoiceClips()
        {
            // 1. Generate procedural formant fallbacks
            _fallbackHajur = CreateVocalFormantClip(135f, 0.38f, 750f, 1200f, 0.65f);
            _fallbackAadesh = CreateVocalFormantClip(140f, 0.42f, 820f, 1850f, 0.45f);
            _fallbackHunchha = CreateVocalFormantClip(130f, 0.35f, 380f, 950f, 0.50f);
            _fallbackAakraman = CreateVocalFormantClip(165f, 0.48f, 850f, 1300f, 0.70f);

            // 2. Load Selection Barks
            AddSelectionBark("voice_hajur_1", "🇳🇵 \"हजुर!\" (Hajur! - Yes, Sire!)", _fallbackHajur);
            AddSelectionBark("voice_hajur_2", "🇳🇵 \"हजुर, भन्नुहोस्!\" (Hajur, speak!)", _fallbackHajur);
            AddSelectionBark("voice_aadesh_1", "🇳🇵 \"आदेश दिनुहोस्!\" (Aadesh! - Give the order!)", _fallbackAadesh);
            AddSelectionBark("voice_tayar_1", "🇳🇵 \"तयार छु!\" (Tayar chhu! - Ready!)", _fallbackHunchha);
            AddSelectionBark("voice_gorkhali_1", "🇳🇵 \"गोर्खाली तयार छ!\" (Gorkhali is ready!)", _fallbackAadesh);

            // 3. Load Move Barks
            AddMoveBark("voice_hunchha_1", "🇳🇵 \"हुन्छ!\" (Hunchha! - Understood!)", _fallbackHunchha);
            AddMoveBark("voice_aagadi_1", "🇳🇵 \"अगाडि बढौँ!\" (Aagadi! - Marching!)", _fallbackHunchha);
            AddMoveBark("voice_jaandai_1", "🇳🇵 \"म जाँदै छु!\" (Ma jaandai chhu! - On my way!)", _fallbackHunchha);

            // 4. Load Attack Barks
            AddAttackBark("voice_aakraman_1", "⚔️ 🇳🇵 \"आक्रमण!\" (Aakraman! - Attack!)", _fallbackAakraman);
            AddAttackBark("voice_mahakali_1", "⚔️ 🇳🇵 \"जय महाकाली!\" (Jai Mahakali!)", _fallbackAakraman);
            AddAttackBark("voice_ayogorkhali_1", "⚔️ 🇳🇵 \"आयो गोर्खाली!\" (Ayo Gorkhali!)", _fallbackAakraman);
            AddAttackBark("voice_khukuri_1", "⚔️ 🇳🇵 \"खुकुरी प्रहार!\" (Khukuri strike!)", _fallbackAakraman);

            // 5. Load Gathering Barks
            _clipGatherWood = GetClip("voice_gather_wood_1") ?? _fallbackHunchha;
            _clipGatherStone = GetClip("voice_gather_stone_1") ?? _fallbackHunchha;
            _clipGatherFood = GetClip("voice_gather_food_1") ?? _fallbackHunchha;
            _clipKaamSuru = GetClip("voice_kaamsuru_1") ?? _fallbackHunchha;

            // 6. Town Alert Barks
            _clipShatruAayo = GetClip("voice_shatru_1") ?? _fallbackAakraman;
            _clipSuraksha = GetClip("voice_suraksha_1") ?? _fallbackAakraman;
        }

        private void AddSelectionBark(string file, string text, AudioClip fallback)
        {
            AudioClip clip = GetClip(file) ?? fallback;
            _selectionBarks.Add((clip, text));
        }

        private void AddMoveBark(string file, string text, AudioClip fallback)
        {
            AudioClip clip = GetClip(file) ?? fallback;
            _moveBarks.Add((clip, text));
        }

        private void AddAttackBark(string file, string text, AudioClip fallback)
        {
            AudioClip clip = GetClip(file) ?? fallback;
            _attackBarks.Add((clip, text));
        }

        public static void PlaySelectBark(UnitController unit)
        {
            if (Instance == null || unit == null || !unit.IsAlive || unit.IsHostile) return;
            if (Time.time - Instance._lastBarkTime < BarkCooldown) return;
            Instance._lastBarkTime = Time.time;

            if (Instance._selectionBarks.Count == 0) return;
            int idx = UnityEngine.Random.Range(0, Instance._selectionBarks.Count);
            var (clip, text) = Instance._selectionBarks[idx];

            Instance.PlayClip(clip);
            GorkhaPlayableHUD.SpawnFloatingText(unit.transform.position + Vector3.up * 2.1f, text, Color.cyan);
        }

        public static void PlayMoveOrderBark(UnitController unit, Vector3 destination)
        {
            if (Instance == null || unit == null || !unit.IsAlive || unit.IsHostile) return;
            if (Time.time - Instance._lastBarkTime < BarkCooldown) return;
            Instance._lastBarkTime = Time.time;

            if (Instance._moveBarks.Count == 0) return;
            int idx = UnityEngine.Random.Range(0, Instance._moveBarks.Count);
            var (clip, text) = Instance._moveBarks[idx];

            Instance.PlayClip(clip);
            GorkhaPlayableHUD.SpawnFloatingText(unit.transform.position + Vector3.up * 2.1f, text, new Color(0.35f, 0.95f, 0.45f));
        }

        public static void PlayAttackOrderBark(UnitController unit)
        {
            if (Instance == null || unit == null || !unit.IsAlive || unit.IsHostile) return;
            if (Time.time - Instance._lastBarkTime < 0.85f) return;
            Instance._lastBarkTime = Time.time;

            if (Instance._attackBarks.Count == 0) return;
            int idx = UnityEngine.Random.Range(0, Instance._attackBarks.Count);
            var (clip, text) = Instance._attackBarks[idx];

            Instance.PlayClip(clip);
            GorkhaAudio.PlayDhimay(0.95f); // Thunderous war drum accompanying war cry!
            GorkhaPlayableHUD.SpawnFloatingText(unit.transform.position + Vector3.up * 2.3f, text, new Color(1.0f, 0.28f, 0.22f));
        }

        public static void PlayHarvestOrderBark(UnitController unit, ResourceType? type = null)
        {
            if (Instance == null || unit == null || !unit.IsAlive || unit.IsHostile) return;
            if (Time.time - Instance._lastBarkTime < BarkCooldown) return;
            Instance._lastBarkTime = Time.time;

            AudioClip clip;
            string text;

            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case ResourceType.Wood:
                        clip = Instance._clipGatherWood;
                        text = "🪓 🇳🇵 \"काठ काट्न जाँदै छु!\" (Chopping wood!)";
                        break;
                    case ResourceType.Stone:
                        clip = Instance._clipGatherStone;
                        text = "⛏️ 🇳🇵 \"ढुङ्गा निकाल्दै छु!\" (Quarrying stone!)";
                        break;
                    case ResourceType.Food:
                        clip = Instance._clipGatherFood;
                        text = "🌾 🇳🇵 \"अन्न बाली उठाउँदै छु!\" (Harvesting crops!)";
                        break;
                    default:
                        clip = Instance._clipKaamSuru;
                        text = "🇳🇵 \"काम सुरु!\" (Kaam suru! - Starting work!)";
                        break;
                }
            }
            else
            {
                clip = Instance._clipKaamSuru;
                text = "🇳🇵 \"काम सुरु!\" (Kaam suru! - Starting work!)";
            }

            Instance.PlayClip(clip);
            GorkhaPlayableHUD.SpawnFloatingText(unit.transform.position + Vector3.up * 2.1f, text, new Color(1.0f, 0.85f, 0.25f));
        }

        public static void PlayDepositSuccessBark(UnitController unit, int amount, string resourceName)
        {
            if (Instance == null || unit == null || !unit.IsAlive || unit.IsHostile) return;

            Instance.PlayClip(Instance._clipKaamSuru);
            GorkhaPlayableHUD.SpawnFloatingText(unit.transform.position + Vector3.up * 2.1f, $"🇳🇵 \"भण्डारण सफल!\" (+{amount} {resourceName})", Color.yellow);
        }

        public static void PlayTownAlertBark(Vector3 position = default)
        {
            if (Instance == null) return;
            if (Time.time - Instance._lastBarkTime < 1.5f) return;
            Instance._lastBarkTime = Time.time;

            bool isShatru = UnityEngine.Random.value > 0.4f;
            AudioClip clip = isShatru ? Instance._clipShatruAayo : Instance._clipSuraksha;
            string text = isShatru
                ? "⚠️ 🇳🇵 \"शत्रु आयो! हतियार उठाऊ!\" (Enemy approaching! Take up arms!)"
                : "🛡️ 🇳🇵 \"गाउँ रक्षा गरौँ!\" (Defend the settlement!)";

            Instance.PlayClip(clip);
            GorkhaAudio.PlayHorn();

            Vector3 spawnPos = position != default ? position + Vector3.up * 3.0f : new Vector3(24f, 6.0f, 24f);
            GorkhaPlayableHUD.SpawnFloatingText(spawnPos, text, new Color(1.0f, 0.15f, 0.15f));
        }

        public static void TriggerHeroAbilityCallout(string text, Vector3 position)
        {
            if (Instance != null)
            {
                Instance.PlayClip(Instance._clipSuraksha);
            }
            GorkhaAudio.PlayHorn();
            GorkhaPlayableHUD.SpawnFloatingText(position + Vector3.up * 2.8f, $"👑 🇳🇵 \"{text}\"", new Color(1f, 0.85f, 0.2f));
        }

        private void PlayClip(AudioClip clip)
        {
            if (_voiceSource != null && clip != null && (GorkhaAudio.Instance == null || !GorkhaAudio.Instance.IsMuted))
            {
                _voiceSource.PlayOneShot(clip, 1.0f);
            }
        }

        #region Audio Clip Loader (WAV parser + AssetDatabase)

        private static AudioClip GetClip(string filename)
        {
#if UNITY_EDITOR
            AudioClip asset = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/VoiceBarks/{filename}.wav");
            if (asset != null) return asset;
#endif
            return LoadWavFromFile(filename);
        }

        private static AudioClip LoadWavFromFile(string filename)
        {
            string path = System.IO.Path.Combine(Application.dataPath, "Audio", "VoiceBarks", $"{filename}.wav");
            if (!System.IO.File.Exists(path)) return null;

            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(path);
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
                        AudioClip clip = AudioClip.Create(filename, sampleCount, channels, sampleRate, false);
                        clip.SetData(floatSamples, 0);
                        return clip;
                    }
                    dataIndex += 8 + chunkSize;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GorkhaVoiceBarks] Failed to load {filename}: {ex.Message}");
            }
            return null;
        }

        #endregion

        #region Formant Voice Synthesis Fallback

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

                float currentPitch = pitch * (1.0f + 0.08f * Mathf.Sin(progress * Mathf.PI) - 0.12f * progress);
                phase += currentPitch / sampleRate;
                if (phase > 1f) phase -= 1f;

                float glottal = (phase < 0.65f)
                    ? 0.5f * (1f - Mathf.Cos(phase / 0.65f * Mathf.PI))
                    : Mathf.Exp(-(phase - 0.65f) * 14f);

                float f1 = Mathf.Lerp(formant1, formant1 * (1f - vowelShift * 0.4f), progress);
                float f2 = Mathf.Lerp(formant2, formant2 * (1f + vowelShift * 0.3f), progress);

                float resonanceF1 = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.70f;
                float resonanceF2 = Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.35f;

                float consonant = (t < 0.045f) ? (float)(rand.NextDouble() * 2.0 - 1.0) * (1f - t / 0.045f) * 0.35f : 0f;

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
