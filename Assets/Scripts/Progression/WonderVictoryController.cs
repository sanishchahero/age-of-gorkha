using System;
using UnityEngine;
using Gorkha.Buildings;
using Gorkha.Economy;
using Gorkha.Audio;
using Gorkha.UI;

namespace Gorkha.Progression
{
    /// <summary>
    /// Age of Empires Wonder Victory Condition System (AGE-04).
    /// Once the Himalayan Wonder of the World is constructed to completion (Stage 4),
    /// a defensive survival countdown commences. Defending the monument secures total victory.
    /// </summary>
    public class WonderVictoryController : MonoBehaviour
    {
        public static WonderVictoryController Instance { get; private set; }

        public event Action OnWonderStarted;
        public event Action OnWonderVictoryAchieved;

        [Header("Victory Timer Configuration")]
        [SerializeField] private float _countdownTotalSeconds = 300f; // 5 minutes standard AoE duration
        private float _countdownRemaining = 300f;
        private bool _isCountdownActive = false;
        private bool _isVictoryAchieved = false;
        private BuildingInstance _wonderBuilding;

        public bool IsCountdownActive => _isCountdownActive;
        public bool IsVictoryAchieved => _isVictoryAchieved;
        public float CountdownRemaining => Mathf.Max(0f, _countdownRemaining);
        public float CountdownTotal => _countdownTotalSeconds;
        public float CountdownProgress => _countdownTotalSeconds > 0f ? Mathf.Clamp01(1f - (_countdownRemaining / _countdownTotalSeconds)) : 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (_isVictoryAchieved) return;

            // Check if Wonder building exists and is active
            if (!_isCountdownActive)
            {
                CheckForCompletedWonder();
            }
            else
            {
                if (_wonderBuilding == null || _wonderBuilding.Health <= 0)
                {
                    // Wonder collapsed!
                    _isCountdownActive = false;
                    GorkhaPlayableHUD.SpawnFloatingText(new Vector3(24f, 8f, 24f), "💥 THE HIMALAYAN WONDER HAS FALLEN!", Color.red);
                    return;
                }

                _countdownRemaining -= Time.deltaTime;
                if (_countdownRemaining <= 0f)
                {
                    AchieveVictory();
                }
            }
        }

        private void CheckForCompletedWonder()
        {
            foreach (var b in BuildingInstance.AllBuildings)
            {
                if (b != null && b.Data != null && b.Data.buildingType == BuildingType.Wonder && b.State == BuildingState.Active)
                {
                    _wonderBuilding = b;
                    StartVictoryCountdown();
                    break;
                }
            }
        }

        private void StartVictoryCountdown()
        {
            _isCountdownActive = true;
            _countdownRemaining = _countdownTotalSeconds;

            GorkhaAudio.PlayEraFanfare();
            GorkhaVoiceBarks.TriggerHeroAbilityCallout("हिमालको आश्चर्य निर्माण सम्पन्न भयो! गाउँको रक्षा गरौँ!", _wonderBuilding.transform.position);
            GorkhaPlayableHUD.SpawnFloatingText(_wonderBuilding.transform.position + Vector3.up * 7.5f, "🏆 HIMALAYAN WONDER COMPLETE! DEFEND FOR 5 MINUTES!", Color.yellow);

            OnWonderStarted?.Invoke();
        }

        private void AchieveVictory()
        {
            _isCountdownActive = false;
            _isVictoryAchieved = true;

            GorkhaAudio.PlayEraFanfare();
            GorkhaVoiceBarks.TriggerHeroAbilityCallout("विजयी भयौँ! गोर्खा साम्राज्यको जय होस्!", new Vector3(24f, 0, 24f));

            OnWonderVictoryAchieved?.Invoke();
            Debug.Log("[WonderVictoryController] 🏆 HIMALAYAN WONDER VICTORY ACHIEVED! Kingdom unified under golden peace.");
        }
    }
}
