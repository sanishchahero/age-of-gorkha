using System;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Units;
using Gorkha.Economy;
using Gorkha.Audio;
using Gorkha.Buildings;
using Gorkha.UI;

namespace Gorkha.Social
{
    public enum GuthiEmblem
    {
        DoubleTriangleFlag, // 🇳🇵 Nepal Pennant
        CrossedKhukuris,    // ⚔️ Iconic Martial Insignia
        GoldenTempleBell,   // 🔔 Sacred Monastic Bell
        HimalayanPeak       // 🏔️ Sacred Mountain Summit
    }

    [Serializable]
    public class CampaignMission
    {
        public int actNumber;
        public string title;
        public string nepaliTitle;
        public string year;
        public string historicalContext;
        public string objective;
        public int rewardFood;
        public int rewardGold;
        public int rewardGems;
        public int banditDefendersCount;
        public bool isCompleted = false;
        public int starsEarned = 0;
    }

    /// <summary>
    /// Guthi Brotherhood & Historical Himalayan Campaign System (SOC-01, SOC-02, SOC-03).
    /// Implements medieval Nepalese Guthi cooperatives:
    /// - Clan identity, heraldry, and member roster
    /// - Guthi Castle reinforcement donations for active base defense
    /// - Historical 5-Act Himalayan Campaign Map (Nuwakot, Makwanpur, Kathmandu, Sindhuligadhi, Nalapani)
    /// </summary>
    public class GuthiManager : MonoBehaviour
    {
        public static GuthiManager Instance { get; private set; }

        public event Action OnGuthiUpdated;
        public event Action<CampaignMission> OnMissionCompleted;

        [Header("Guthi Clan Identity")]
        [SerializeField] private string _guthiName = "श्री गोरखनाथ गुठी (Shree Gorakhnath Guthi)";
        [SerializeField] private GuthiEmblem _emblem = GuthiEmblem.DoubleTriangleFlag;
        [SerializeField] private string _playerRank = "गुठी नाइके (Guthi Commander / Nayo)";
        [SerializeField] private int _clanTrophies = 1420;
        [SerializeField] private int _clanLevel = 3;

        [Header("Reinforcements (Guthi Durbar)")]
        [SerializeField] private int _maxTroopCapacity = 20;
        private readonly List<UnitType> _donatedTroops = new();
        private float _requestCooldownTimer = 0f;
        private const float REQUEST_COOLDOWN_TOTAL = 90f; // 90 seconds cooldown between requests

        [Header("Historical Campaign Acts")]
        private readonly List<CampaignMission> _campaignMissions = new();

        public string GuthiName => _guthiName;
        public GuthiEmblem Emblem => _emblem;
        public string PlayerRank => _playerRank;
        public int ClanTrophies => _clanTrophies;
        public int ClanLevel => _clanLevel;
        public int MaxTroopCapacity => _maxTroopCapacity;
        public int CurrentTroopCount => _donatedTroops.Count;
        public IReadOnlyList<UnitType> DonatedTroops => _donatedTroops;
        public IReadOnlyList<CampaignMission> CampaignMissions => _campaignMissions;
        public bool CanRequestTroops => _requestCooldownTimer <= 0f && _donatedTroops.Count < _maxTroopCapacity;
        public float RequestCooldownRemaining => Mathf.Max(0f, _requestCooldownTimer);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeDefaultReinforcements();
            InitializeCampaignActs();
        }

        private void InitializeDefaultReinforcements()
        {
            _donatedTroops.Clear();
            _donatedTroops.Add(UnitType.KhukuriWarrior);
            _donatedTroops.Add(UnitType.KhukuriWarrior);
            _donatedTroops.Add(UnitType.Spearman);
            _donatedTroops.Add(UnitType.Spearman);
            _donatedTroops.Add(UnitType.Archer);
        }

        private void InitializeCampaignActs()
        {
            _campaignMissions.Clear();

            _campaignMissions.Add(new CampaignMission
            {
                actNumber = 1,
                title = "Act I: The Ridge of Nuwakot",
                nepaliTitle = "पहिलो अध्याय: नुवाकोट विजय",
                year = "1744 AD",
                historicalContext = "Young King Prithvi Narayan Shah scales the steep cliffs of Mahamandal to capture the 3-tiered hill fort of Nuwakot.",
                objective = "Breach outer stockades and eliminate 6 Malla defenders.",
                rewardFood = 600,
                rewardGold = 500,
                rewardGems = 10,
                banditDefendersCount = 6,
                isCompleted = true,
                starsEarned = 3
            });

            _campaignMissions.Add(new CampaignMission
            {
                actNumber = 2,
                title = "Act II: Battle of Makwanpur",
                nepaliTitle = "दोस्रो अध्याय: मकवानपुरको युद्ध",
                year = "1762 AD",
                historicalContext = "Gorkha forces under Kaji Kalu Pande confront the heavy vanguard of Gurgin Khan in the southern hills.",
                objective = "Withstand cavalry charge and conquer the southern mountain pass.",
                rewardFood = 1200,
                rewardGold = 1000,
                rewardGems = 15,
                banditDefendersCount = 9,
                isCompleted = false,
                starsEarned = 0
            });

            _campaignMissions.Add(new CampaignMission
            {
                actNumber = 3,
                title = "Act III: Valley of the Gods",
                nepaliTitle = "तेस्रो अध्याय: कान्तिपुर विजय",
                year = "1768 AD",
                historicalContext = "Infiltrate Kathmandu during the holy Indra Jatra chariot festival, taking Hanuman Dhoka palace without civilian bloodshed.",
                objective = "Secure the three royal Durbar squares of the Kathmandu valley.",
                rewardFood = 2000,
                rewardGold = 1800,
                rewardGems = 25,
                banditDefendersCount = 12,
                isCompleted = false,
                starsEarned = 0
            });

            _campaignMissions.Add(new CampaignMission
            {
                actNumber = 4,
                title = "Act IV: Ambush at Sindhuligadhi",
                nepaliTitle = "चौथो अध्याय: सिन्धुलीगढीको लडाइँ",
                year = "1767 AD",
                historicalContext = "Ambush British Captain Kinloch’s expedition forces using hornet nests, tactical rockslides, and downhill Khukuri charges.",
                objective = "Decimate British column in narrow defiles and capture supply trains.",
                rewardFood = 3500,
                rewardGold = 3000,
                rewardGems = 35,
                banditDefendersCount = 16,
                isCompleted = false,
                starsEarned = 0
            });

            _campaignMissions.Add(new CampaignMission
            {
                actNumber = 5,
                title = "Act V: The Ramparts of Nalapani",
                nepaliTitle = "पाँचौँ अध्याय: नालापानी खलङ्गा रक्षा",
                year = "1814 AD",
                historicalContext = "Command Captain Balbhadra Kunwar and 600 brave warriors defending Khalanga fort against 3,500 British troops and artillery.",
                objective = "Endure artillery bombardment and repulse 4 consecutive assault waves.",
                rewardFood = 5000,
                rewardGold = 4500,
                rewardGems = 50,
                banditDefendersCount = 20,
                isCompleted = false,
                starsEarned = 0
            });
        }

        private void Update()
        {
            if (_requestCooldownTimer > 0f)
            {
                _requestCooldownTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Requests reinforcements from clan members (CoC Clan Castle request).
        /// </summary>
        public bool RequestClanTroops(out string reason)
        {
            if (!CanRequestTroops)
            {
                reason = _donatedTroops.Count >= _maxTroopCapacity
                    ? "Guthi Castle is already full!"
                    : $"Request on cooldown! ({_requestCooldownTimer:F0}s remaining)";
                return false;
            }

            _requestCooldownTimer = REQUEST_COOLDOWN_TOTAL;

            // Simulate simulated generous clan donation
            _donatedTroops.Add(UnitType.KhukuriWarrior);
            _donatedTroops.Add(UnitType.Archer);

            GorkhaAudio.PlayEraFanfare();
            GorkhaVoiceBarks.TriggerHeroAbilityCallout("गुठी सहयोगी सेना आइपुग्यो! (Clan reinforcements arrived!)", new Vector3(24f, 0, 24f));
            GorkhaPlayableHUD.SpawnFloatingText(new Vector3(24f, 5f, 24f), "🛡️ +2 CLAN TROOPS DONATED!", Color.cyan);

            OnGuthiUpdated?.Invoke();
            reason = null;
            return true;
        }

        /// <summary>
        /// Deploys all donated troops out of the Guthi Castle to intercept incoming enemy raiders.
        /// </summary>
        public void DeployGarrisonDefenders(Vector3 deployPoint)
        {
            if (_donatedTroops.Count == 0) return;

            GorkhaAudio.PlayHorn();
            GorkhaVoiceBarks.TriggerHeroAbilityCallout("जय महाकाली! गुठीका सिपाहीहरू निस्किए!", deployPoint);

            for (int i = 0; i < _donatedTroops.Count; i++)
            {
                UnitType uType = _donatedTroops[i];
                float angle = i * (360f / Mathf.Max(1, _donatedTroops.Count)) * Mathf.Deg2Rad;
                Vector3 spawnPos = deployPoint + new Vector3(Mathf.Cos(angle) * 2.2f, 0, Mathf.Sin(angle) * 2.2f);

                GameObject troopObj = new GameObject($"GuthiReinforcement_{i + 1}_{uType}");
                troopObj.transform.position = spawnPos;
                var u = troopObj.AddComponent<UnitController>();
                u.SetUnitType(uType);
                u.OrderAttackMove(new Vector3(24f, 0, 24f));

                GorkhaPlayableHUD.SpawnFloatingText(spawnPos + Vector3.up * 2f, "🛡️ Guthi Guard!", Color.cyan);
            }

            _donatedTroops.Clear();
            OnGuthiUpdated?.Invoke();
        }

        /// <summary>
        /// Completes a historical campaign act and deposits rewards into kingdom storage.
        /// </summary>
        public void CompleteMission(int actNumber, int stars)
        {
            var mission = _campaignMissions.Find(m => m.actNumber == actNumber);
            if (mission == null) return;

            mission.isCompleted = true;
            mission.starsEarned = Mathf.Max(mission.starsEarned, stars);

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(ResourceType.Food, mission.rewardFood);
                ResourceManager.Instance.AddResource(ResourceType.Gold, mission.rewardGold);
                ResourceManager.Instance.AddResource(ResourceType.Gems, mission.rewardGems);
            }

            _clanTrophies += stars * 25;

            GorkhaAudio.PlayEraFanfare();
            GorkhaPlayableHUD.SpawnFloatingText(new Vector3(24f, 6f, 24f), $"🏆 ACT {actNumber} CONQUERED! (+{mission.rewardGold} Gold, +{mission.rewardGems} Gems)", Color.yellow);

            OnMissionCompleted?.Invoke(mission);
            OnGuthiUpdated?.Invoke();
        }
    }
}
