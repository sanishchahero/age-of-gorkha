using System;
using UnityEngine;
using Gorkha.Economy;
using Gorkha.Units;
using Gorkha.Audio;
using Gorkha.Graphics;
using Gorkha.UI;

namespace Gorkha.Progression
{
    /// <summary>
    /// Historic Hero Generals Manager (CBT-06).
    /// Regulates the summoning, gating, and battlefield presence of Gorkha's iconic military leaders:
    /// Prithvi Narayan Shah (The Great Unifier) and Kalu Pande (Grand Commander).
    /// </summary>
    public class HeroManager : MonoBehaviour
    {
        public static HeroManager Instance { get; private set; }

        public event Action<HeroController> OnHeroSummoned;
        public event Action<HeroController> OnHeroFallen;

        private HeroController _activePrithvi;
        private HeroController _activeKaluPande;

        public HeroController ActivePrithvi => _activePrithvi;
        public HeroController ActiveKaluPande => _activeKaluPande;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool IsHeroAlive(HeroGeneralType type)
        {
            return type == HeroGeneralType.PrithviNarayanShah
                ? (_activePrithvi != null && _activePrithvi.GetComponent<UnitController>().IsAlive)
                : (_activeKaluPande != null && _activeKaluPande.GetComponent<UnitController>().IsAlive);
        }

        public int GetRequiredAge(HeroGeneralType type)
        {
            return type == HeroGeneralType.PrithviNarayanShah ? 3 : 2;
        }

        public (double food, double gold) GetHeroCost(HeroGeneralType type)
        {
            return type == HeroGeneralType.PrithviNarayanShah
                ? (250.0, 200.0)
                : (150.0, 100.0);
        }

        public bool CanSummonHero(HeroGeneralType type, out string reason)
        {
            int currentAge = AgeManager.Instance != null ? AgeManager.Instance.CurrentAgeNumber : 1;
            int reqAge = GetRequiredAge(type);

            if (currentAge < reqAge)
            {
                string heroName = type == HeroGeneralType.PrithviNarayanShah ? "Prithvi Narayan Shah" : "Kalu Pande";
                reason = $"🔒 {heroName} requires Era {reqAge} (Upgrade Gorkha Durbar)!";
                return false;
            }

            if (IsHeroAlive(type))
            {
                reason = "General is already commanding on the battlefield!";
                return false;
            }

            var (foodCost, goldCost) = GetHeroCost(type);
            var rm = ResourceManager.Instance;
            if (rm != null)
            {
                if (rm.GetAmount(ResourceType.Food) < foodCost)
                {
                    reason = $"Insufficient Food! (Need {foodCost})";
                    return false;
                }
                if (rm.GetAmount(ResourceType.Gold) < goldCost)
                {
                    reason = $"Insufficient Gold! (Need {goldCost})";
                    return false;
                }
            }

            reason = null;
            return true;
        }

        public HeroController SummonHero(HeroGeneralType type, Vector3 spawnPosition)
        {
            if (!CanSummonHero(type, out string reason))
            {
                Debug.LogWarning($"[HeroManager] Cannot summon hero: {reason}");
                return null;
            }

            var (foodCost, goldCost) = GetHeroCost(type);
            var rm = ResourceManager.Instance;
            if (rm != null)
            {
                rm.Spend(ResourceType.Food, foodCost);
                rm.Spend(ResourceType.Gold, goldCost);
            }

            string objName = type == HeroGeneralType.PrithviNarayanShah
                ? "Hero_PrithviNarayanShah"
                : "Hero_KaluPande";

            GameObject heroObj = new GameObject(objName);
            heroObj.transform.position = spawnPosition;

            var unitCtrl = heroObj.AddComponent<UnitController>();
            var heroCtrl = heroObj.AddComponent<HeroController>();

            heroCtrl.Initialize(type);

            if (type == HeroGeneralType.PrithviNarayanShah)
            {
                _activePrithvi = heroCtrl;
            }
            else
            {
                _activeKaluPande = heroCtrl;
            }

            GorkhaAudio.PlayEraFanfare();
            GorkhaVoiceBarks.TriggerHeroAbilityCallout(
                type == HeroGeneralType.PrithviNarayanShah
                    ? "बडामहाराजाधिराज पृथ्वीनारायण शाह रणभूमिमा!"
                    : "काजी कालु पाँडे वीर सेनापति रणभूमिमा!",
                spawnPosition);

            GorkhaVFX.SpawnPlacementDust(spawnPosition, Vector2.one * 2.2f);
            GorkhaPlayableHUD.SpawnFloatingText(spawnPosition + Vector3.up * 3.0f, $"👑 {heroCtrl.HeroTitle} ARRIVES!", Color.yellow);

            OnHeroSummoned?.Invoke(heroCtrl);
            return heroCtrl;
        }

        public void NotifyHeroFallen(HeroController hero)
        {
            if (hero == _activePrithvi) _activePrithvi = null;
            if (hero == _activeKaluPande) _activeKaluPande = null;
            OnHeroFallen?.Invoke(hero);
        }
    }
}
