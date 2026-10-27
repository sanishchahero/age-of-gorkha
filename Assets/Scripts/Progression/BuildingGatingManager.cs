using System;
using UnityEngine;
using Gorkha.Buildings;
using Gorkha.Units;

namespace Gorkha.Progression
{
    /// <summary>
    /// Clash of Clans Town Hall / Age of Empires Town Center Gating System (AGE-02).
    /// Regulates structure availability, maximum building counts, and unit recruitments
    /// based on the Gorkha Durbar's attained Historical Era.
    /// </summary>
    public static class BuildingGatingManager
    {
        public static int GetCurrentAge()
        {
            return AgeManager.Instance != null ? AgeManager.Instance.CurrentAgeNumber : 1;
        }

        public static int GetRequiredAge(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.StoneQuarry:
                case BuildingType.Watchtower:
                case BuildingType.Granary:
                case BuildingType.TimberYard:
                case BuildingType.Trap:
                case BuildingType.Academy:
                    return 2;

                case BuildingType.Kaman:
                case BuildingType.SlateDepot:
                case BuildingType.Vault:
                    return 3;

                case BuildingType.Wonder:
                    return 4;

                case BuildingType.Gate:
                    return 2;

                default:
                    return 1;
            }
        }

        public static int GetMaxAllowed(BuildingType type, int age)
        {
            switch (type)
            {
                case BuildingType.TownCenter:
                    return 1;

                case BuildingType.Farm:
                    return age >= 2 ? 2 : 1;

                case BuildingType.WoodCamp:
                    return age >= 2 ? 2 : 1;

                case BuildingType.StoneQuarry:
                    return age >= 2 ? 1 : 0;

                case BuildingType.House:
                    return age == 1 ? 2 : (age == 2 ? 4 : (age == 3 ? 7 : 10));

                case BuildingType.Barracks:
                    return age >= 3 ? 2 : 1;

                case BuildingType.Watchtower:
                    return age == 1 ? 0 : (age == 2 ? 1 : (age == 3 ? 2 : 4));

                case BuildingType.Wall:
                    return age == 1 ? 25 : (age == 2 ? 50 : (age == 3 ? 100 : 200));

                case BuildingType.Kaman:
                    return age >= 3 ? 1 : 0;

                case BuildingType.Trap:
                    return age >= 2 ? 2 : 0;

                case BuildingType.Granary:
                    return age >= 2 ? 1 : 0;

                case BuildingType.TimberYard:
                    return age >= 2 ? 1 : 0;

                case BuildingType.SlateDepot:
                    return age >= 3 ? 1 : 0;

                case BuildingType.Vault:
                    return age >= 3 ? 1 : 0;

                case BuildingType.Academy:
                    return age >= 2 ? 1 : 0;

                case BuildingType.Wonder:
                    return age >= 4 ? 1 : 0;

                default:
                    return 99;
            }
        }

        public static int GetMaxAllowed(BuildingType type)
        {
            return GetMaxAllowed(type, GetCurrentAge());
        }

        public static int GetCurrentCount(BuildingType type)
        {
            var allBuildings = UnityEngine.Object.FindObjectsByType<BuildingInstance>(FindObjectsInactive.Exclude);
            int count = 0;
            foreach (var b in allBuildings)
            {
                if (b != null && b.Data != null && b.Data.buildingType == type)
                {
                    count++;
                }
            }
            return count;
        }

        public static bool IsBuildingUnlocked(BuildingType type)
        {
            return GetCurrentAge() >= GetRequiredAge(type);
        }

        public static bool CanConstruct(BuildingType type, out string reason)
        {
            int age = GetCurrentAge();
            int reqAge = GetRequiredAge(type);

            if (age < reqAge)
            {
                reason = $"🔒 Locked! Requires Era {reqAge} (Upgrade Gorkha Durbar).";
                return false;
            }

            int cur = GetCurrentCount(type);
            int max = GetMaxAllowed(type, age);
            if (cur >= max)
            {
                reason = $"⚠️ Maximum capacity reached ({cur}/{max}) for Era {age}! Upgrade Durbar to build more.";
                return false;
            }

            reason = null;
            return true;
        }

        public static bool IsUnitUnlocked(UnitType type, out string reason)
        {
            return IsUnitUnlocked(type, out _, out reason);
        }

        public static bool IsUnitUnlocked(UnitType type, out int requiredAge, out string reason)
        {
            int currentAge = GetCurrentAge();

            switch (type)
            {
                case UnitType.Spearman:
                case UnitType.Villager:
                    requiredAge = 1;
                    reason = null;
                    return true;

                case UnitType.KhukuriWarrior:
                    requiredAge = 2;
                    if (currentAge < 2)
                    {
                        reason = "🔒 Khukuri Warriors require Era II: Fortified Kot!";
                        return false;
                    }
                    reason = null;
                    return true;

                case UnitType.Archer:
                    requiredAge = 3;
                    if (currentAge < 3)
                    {
                        reason = "🔒 Mountain Archers require Era III: Unification Era!";
                        return false;
                    }
                    reason = null;
                    return true;

                case UnitType.HeroKaluPande:
                    requiredAge = 2;
                    if (currentAge < 2)
                    {
                        reason = "🔒 General Kalu Pande requires Era II: Fortified Kot!";
                        return false;
                    }
                    reason = null;
                    return true;

                case UnitType.HeroPrithvi:
                    requiredAge = 3;
                    if (currentAge < 3)
                    {
                        reason = "🔒 King Prithvi Narayan Shah requires Era III: Unification Era!";
                        return false;
                    }
                    reason = null;
                    return true;

                default:
                    requiredAge = 1;
                    reason = null;
                    return true;
            }
        }
    }
}
