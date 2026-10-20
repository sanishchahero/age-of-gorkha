using System;
using UnityEngine;
using Gorkha.Core;
using Gorkha.Graphics;
using Gorkha.Audio;

namespace Gorkha.Economy
{
    public enum ResourceNodeType
    {
        Tree,
        Boulder
    }

    /// <summary>
    /// Interactive harvestable world resource node (Age of Empires & Clash of Clans obstacle style).
    /// Trees provide Wood, and Boulders provide Stone.
    /// Villagers gather resources from nodes until depleted.
    /// Depleted nodes shrink, vanish, and free up terrain grid space for kingdom expansion.
    /// </summary>
    public class ResourceNode : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<ResourceNode> AllNodes = new();

        [Header("Node Configuration")]
        [SerializeField] private ResourceNodeType _nodeType = ResourceNodeType.Tree;
        [SerializeField] private double _initialAmount = 150.0;
        [SerializeField] private double _remainingAmount = 150.0;
        [SerializeField] private int _gridCol;
        [SerializeField] private int _gridRow;

        private Vector3 _originalScale;
        private bool _isDepleted = false;

        public ResourceNodeType NodeType => _nodeType;
        public ResourceType YieldType => _nodeType == ResourceNodeType.Tree ? ResourceType.Wood : ResourceType.Stone;
        public double RemainingAmount => _remainingAmount;
        public double InitialAmount => _initialAmount;
        public bool IsDepleted => _isDepleted;
        public int GridCol => _gridCol;
        public int GridRow => _gridRow;

        private void OnEnable()
        {
            if (!AllNodes.Contains(this)) AllNodes.Add(this);
        }

        private void OnDisable()
        {
            AllNodes.Remove(this);
        }

        private void Awake()
        {
            _originalScale = transform.localScale;
            if (_originalScale == Vector3.zero) _originalScale = Vector3.one;

            // Ensure collider for mouse clicks
            if (!TryGetComponent<Collider>(out _))
            {
                var col = gameObject.AddComponent<BoxCollider>();
                col.size = new Vector3(1.6f, 2.5f, 1.6f);
                col.center = new Vector3(0, 1.25f, 0);
            }
        }

        public void Initialize(ResourceNodeType type, int col, int row, double amount = 150.0)
        {
            _nodeType = type;
            _gridCol = col;
            _gridRow = row;
            _initialAmount = amount;
            _remainingAmount = amount;
            _isDepleted = false;
        }

        /// <summary>
        /// Gathers resources from this node. Returns actual amount gathered.
        /// </summary>
        public double Harvest(double requestedAmount)
        {
            if (_isDepleted) return 0.0;

            double gathered = Math.Min(requestedAmount, _remainingAmount);
            _remainingAmount -= gathered;

            // Gentle visual scale depletion
            float pct = Mathf.Clamp01((float)(_remainingAmount / Math.Max(1.0, _initialAmount)));
            transform.localScale = _originalScale * Mathf.Max(0.35f, pct);

            // Harvest impact particles
            GorkhaVFX.SpawnPlacementDust(transform.position, Vector2.one * 0.4f);

            if (_remainingAmount <= 0.001)
            {
                DepleteAndVanish();
            }

            return gathered;
        }

        private void DepleteAndVanish()
        {
            if (_isDepleted) return;
            _isDepleted = true;

            // Free area in grid so player can build here!
            if (IsometricGrid.Instance != null)
            {
                IsometricGrid.Instance.FreeArea(_gridCol, _gridRow, 1, 1);
            }

            GorkhaAudio.PlayHarvest();
            GorkhaVFX.SpawnPlacementDust(transform.position, Vector2.one * 1.2f);

            Destroy(gameObject, 0.2f);
        }
    }
}
