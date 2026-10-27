using System;
using System.Collections.Generic;
using UnityEngine;
using Gorkha.Economy;

namespace Gorkha.Buildings
{
    public enum BuildingType
    {
        TownCenter,
        Farm,
        WoodCamp,
        StoneQuarry,
        Barracks,
        House,
        Watchtower,
        Wall,
        Gate,
        Kaman,
        Trap,
        Granary,
        TimberYard,
        SlateDepot,
        Vault,
        Academy,
        Wonder
    }

    [Serializable]
    public struct ResourceCost
    {
        public ResourceType type;
        public double amount;
    }

    /// <summary>
    /// Blueprint ScriptableObject defining static properties of a building type.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuildingData", menuName = "Gorkha/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Header("General Info")]
        public BuildingType buildingType;
        public string displayName = "Building";
        [TextArea] public string description = "A kingdom structure.";
        public GameObject prefab;

        [Header("Grid Footprint (Tiles)")]
        public int tileWidth = 2;
        public int tileHeight = 2;

        [Header("Construction")]
        public float constructionDurationSeconds = 10f;
        public List<ResourceCost> baseCost = new();

        [Header("Production & Capacity")]
        public bool hasProduction = false;
        public ResourceType productionType;
        public double baseProductionRate = 0.0;
        public int populationCapacity = 0;

        [Header("Storage Capacity (CoC ECO-01)")]
        public bool isStorage = false;
        public ResourceType storageType = ResourceType.Food;
        public double baseStorageCapacity = 1000.0;

        public Dictionary<ResourceType, double> GetCostDictionary(int level = 0)
        {
            var dict = new Dictionary<ResourceType, double>();
            float multiplier = Mathf.Pow(1.35f, Mathf.Max(0, level));
            foreach (var cost in baseCost)
            {
                dict[cost.type] = Math.Round(cost.amount * multiplier);
            }
            return dict;
        }
    }
}
