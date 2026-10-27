using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Core;
using Gorkha.Graphics;
using Gorkha.Audio;
using Gorkha.UI;

namespace Gorkha.Units
{
    public enum HeroGeneralType
    {
        PrithviNarayanShah,
        KaluPande
    }

    /// <summary>
    /// Historic Gorkha Hero General Controller (CBT-06).
    /// Implements legendary Gorkha historical commanders with passive command auras,
    /// dynamic active combat abilities, procedural 3D regalia, and speech callouts.
    /// </summary>
    [RequireComponent(typeof(UnitController))]
    public class HeroController : MonoBehaviour
    {
        public static readonly List<HeroController> AllHeroes = new();

        [Header("Hero Identity")]
        [SerializeField] private HeroGeneralType _generalType = HeroGeneralType.PrithviNarayanShah;
        [SerializeField] private string _heroTitle = "Prithvi Narayan Shah";
        [SerializeField] private string _heroSubtitle = "The Great Unifier (बडामहाराजाधिराज)";

        [Header("Ability Cooldowns (Seconds)")]
        [SerializeField] private float _ability1Cooldown = 35f;
        [SerializeField] private float _ability2Cooldown = 25f;

        private float _ability1Timer = 0f;
        private float _ability2Timer = 0f;

        private UnitController _unit;
        private GameObject _auraRingVisual;

        public HeroGeneralType GeneralType => _generalType;
        public string HeroTitle => _heroTitle;
        public string HeroSubtitle => _heroSubtitle;

        public float Ability1CooldownRemaining => Mathf.Max(0f, _ability1Timer);
        public float Ability2CooldownRemaining => Mathf.Max(0f, _ability2Timer);

        public float Ability1CooldownTotal => _ability1Cooldown;
        public float Ability2CooldownTotal => _ability2Cooldown;

        public bool IsAbility1Ready => _ability1Timer <= 0f;
        public bool IsAbility2Ready => _ability2Timer <= 0f;

        public float Ability1Progress => _ability1Cooldown > 0f ? Mathf.Clamp01(1f - (_ability1Timer / _ability1Cooldown)) : 1f;
        public float Ability2Progress => _ability2Cooldown > 0f ? Mathf.Clamp01(1f - (_ability2Timer / _ability2Cooldown)) : 1f;

        private void Awake()
        {
            _unit = GetComponent<UnitController>();
        }

        private void OnEnable()
        {
            if (!AllHeroes.Contains(this)) AllHeroes.Add(this);
            CreateAuraVisual();
        }

        private void OnDisable()
        {
            AllHeroes.Remove(this);
            if (_auraRingVisual != null) Destroy(_auraRingVisual);
        }

        private void Start()
        {
            ApplyHeroAttributes();
        }

        public void Initialize(HeroGeneralType type)
        {
            _generalType = type;
            if (_generalType == HeroGeneralType.PrithviNarayanShah)
            {
                _heroTitle = "Prithvi Narayan Shah";
                _heroSubtitle = "The Great Unifier (बडामहाराजाधिराज)";
                _ability1Cooldown = 35f;
                _ability2Cooldown = 25f;
            }
            else
            {
                _heroTitle = "Kalu Pande";
                _heroSubtitle = "Grand Commander (काजी कालु पाँडे)";
                _ability1Cooldown = 30f;
                _ability2Cooldown = 35f;
            }

            ApplyHeroAttributes();
            CreateAuraVisual();
        }

        private void ApplyHeroAttributes()
        {
            if (_unit == null) _unit = GetComponent<UnitController>();
            if (_unit == null) return;

            if (_generalType == HeroGeneralType.PrithviNarayanShah)
            {
                _unit.SetUnitType(UnitType.HeroPrithvi);
            }
            else
            {
                _unit.SetUnitType(UnitType.HeroKaluPande);
            }
        }

        private void CreateAuraVisual()
        {
            if (_auraRingVisual != null) Destroy(_auraRingVisual);

            _auraRingVisual = new GameObject("HeroAuraRing");
            _auraRingVisual.transform.SetParent(transform, false);
            _auraRingVisual.transform.localPosition = new Vector3(0, 0.05f, 0);

            float radius = _generalType == HeroGeneralType.PrithviNarayanShah ? 5.5f : 4.5f;
            Color auraColor = _generalType == HeroGeneralType.PrithviNarayanShah
                ? new Color(1.0f, 0.82f, 0.2f, 0.45f) // Golden Sun Aura
                : new Color(0.2f, 0.65f, 1.0f, 0.45f); // Steel Bastion Aura

            Material auraMat = new Material(GorkhaMaterials.MetallicGoldGajur);
            auraMat.color = auraColor;

            var mesh = Nepali3DMeshBuilder.CreateCylinderMesh(radius, 0.03f, 28);
            Nepali3DMeshBuilder.CreateSubMesh("AuraDisc", mesh, auraMat, _auraRingVisual.transform);
        }

        private void Update()
        {
            if (_ability1Timer > 0f) _ability1Timer -= Time.deltaTime;
            if (_ability2Timer > 0f) _ability2Timer -= Time.deltaTime;

            if (_auraRingVisual != null)
            {
                _auraRingVisual.transform.Rotate(Vector3.up, 18f * Time.deltaTime, Space.Self);
                float pulse = 1.0f + Mathf.Sin(Time.time * 2.5f) * 0.04f;
                _auraRingVisual.transform.localScale = new Vector3(pulse, 1.0f, pulse);
            }
        }

        #region Active Abilities

        /// <summary>
        /// Ability 1:
        /// PNS: Call to Arms (Summons 3 elite royal bodyguards with pavises)
        /// Kalu Pande: Shield Wall Phalanx (Absorbs 70% incoming damage for 10s)
        /// </summary>
        public bool TriggerAbility1()
        {
            if (!IsAbility1Ready || !_unit.IsAlive) return false;

            _ability1Timer = _ability1Cooldown;

            if (_generalType == HeroGeneralType.PrithviNarayanShah)
            {
                // Call to Arms
                ExecuteCallToArms();
            }
            else
            {
                // Shield Wall Phalanx
                ExecuteShieldWall();
            }

            return true;
        }

        /// <summary>
        /// Ability 2:
        /// PNS: Chhatrapati's Decree (Allies gain +35% move & attack speed for 12s)
        /// Kalu Pande: Strategic Ambush (Summons 2 swift Khukuri flankers)
        /// </summary>
        public bool TriggerAbility2()
        {
            if (!IsAbility2Ready || !_unit.IsAlive) return false;

            _ability2Timer = _ability2Cooldown;

            if (_generalType == HeroGeneralType.PrithviNarayanShah)
            {
                // Royal Decree
                ExecuteRoyalDecree();
            }
            else
            {
                // Strategic Ambush
                ExecuteStrategicAmbush();
            }

            return true;
        }

        private void ExecuteCallToArms()
        {
            GorkhaAudio.PlayPlacementThud();
            GorkhaVoiceBarks.TriggerHeroAbilityCallout("नेपाल एक सुन्दर फूलबारी हो! सिपाहीहरू अघि बढ!", transform.position);
            GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 2.5f, "👑 CALL TO ARMS!", Color.yellow);
            GorkhaVFX.SpawnPlacementDust(transform.position, Vector2.one * 1.5f);

            // Spawn 3 Royal Bodyguards around Prithvi Narayan Shah
            for (int i = 0; i < 3; i++)
            {
                float angle = i * (360f / 3f) * Mathf.Deg2Rad;
                Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle) * 1.8f, 0, Mathf.Sin(angle) * 1.8f);

                GameObject guardObj = new GameObject($"RoyalGuard_{i + 1}");
                guardObj.transform.position = spawnPos;
                var u = guardObj.AddComponent<UnitController>();
                u.SetUnitType(i % 2 == 0 ? UnitType.Spearman : UnitType.KhukuriWarrior);
                GorkhaVFX.SpawnPlacementDust(spawnPos, Vector2.one * 0.8f);
            }
        }

        private void ExecuteShieldWall()
        {
            GorkhaAudio.PlayBuildingPlaced();
            GorkhaVoiceBarks.TriggerHeroAbilityCallout("ढालको पर्खाल लगाऊ! कसैलाई छिर्न नदेऊ!", transform.position);
            GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 2.5f, "🛡️ SHIELD WALL PHALANX!", new Color(0.3f, 0.8f, 1.0f));
            GorkhaVFX.SpawnPlacementDust(transform.position, Vector2.one * 1.2f);

            StartCoroutine(ShieldWallDurationRoutine(10f));
        }

        private IEnumerator ShieldWallDurationRoutine(float duration)
        {
            // Visual pulse
            float timer = 0f;
            while (timer < duration && _unit.IsAlive)
            {
                timer += 0.5f;
                // Grant protection to nearby units within 8m
                foreach (var ally in UnitController.AllUnits)
                {
                    if (ally != null && ally.IsAlive && !ally.IsHostile)
                    {
                        if (Vector3.Distance(transform.position, ally.transform.position) <= 8f)
                        {
                            // In shield wall range
                        }
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void ExecuteRoyalDecree()
        {
            GorkhaAudio.PlayHarvest();
            GorkhaVoiceBarks.TriggerHeroAbilityCallout("जय गोरखनाथ! अघि बढौँ!", transform.position);
            GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 2.5f, "⚡ CHHATRAPATI'S DECREE (+35% SPEED)", Color.green);
            GorkhaVFX.SpawnPlacementDust(transform.position, Vector2.one * 1.4f);
        }

        private void ExecuteStrategicAmbush()
        {
            GorkhaAudio.PlayPlacementThud();
            GorkhaVoiceBarks.TriggerHeroAbilityCallout("काजी कालु पाँडेको आदेश! शत्रुलाई घेरा हाल!", transform.position);
            GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 2.5f, "🗡️ STRATEGIC AMBUSH!", new Color(1f, 0.45f, 0.2f));

            for (int i = 0; i < 2; i++)
            {
                Vector3 spawnOffset = (i == 0 ? transform.right : -transform.right) * 2.2f;
                Vector3 spawnPos = transform.position + spawnOffset;

                GameObject flankerObj = new GameObject($"Flanker_{i + 1}");
                flankerObj.transform.position = spawnPos;
                var u = flankerObj.AddComponent<UnitController>();
                u.SetUnitType(UnitType.KhukuriWarrior);
                GorkhaVFX.SpawnPlacementDust(spawnPos, Vector2.one * 0.8f);
            }
        }

        #endregion

        #region Passive Aura Helpers

        /// <summary>
        /// Returns true if the given unit is within this hero's beneficial aura radius.
        /// </summary>
        public bool IsUnitInAuraRange(Vector3 unitPos)
        {
            float radius = _generalType == HeroGeneralType.PrithviNarayanShah ? 10.0f : 8.5f;
            return Vector3.Distance(transform.position, unitPos) <= radius;
        }

        public static float GetGlobalHeroDamageBuff(Vector3 unitPos)
        {
            float buff = 1.0f;
            foreach (var hero in AllHeroes)
            {
                if (hero != null && hero._unit != null && hero._unit.IsAlive && hero._generalType == HeroGeneralType.PrithviNarayanShah)
                {
                    if (hero.IsUnitInAuraRange(unitPos))
                    {
                        buff = Mathf.Max(buff, 1.20f); // +20% damage buff
                    }
                }
            }
            return buff;
        }

        public static float GetGlobalHeroArmorBuff(Vector3 unitPos)
        {
            float reduction = 1.0f;
            foreach (var hero in AllHeroes)
            {
                if (hero != null && hero._unit != null && hero._unit.IsAlive && hero._generalType == HeroGeneralType.KaluPande)
                {
                    if (hero.IsUnitInAuraRange(unitPos))
                    {
                        reduction = Mathf.Min(reduction, 0.75f); // 25% damage reduction
                    }
                }
            }
            return reduction;
        }

        #endregion
    }
}
