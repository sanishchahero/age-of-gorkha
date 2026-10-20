using System.Collections.Generic;
using UnityEngine;
using Gorkha.Buildings;
using Gorkha.Graphics;
using Gorkha.Audio;

namespace Gorkha.Economy
{
    /// <summary>
    /// Master Craftsman & Builder Management System (Clash of Clans style).
    /// Tracks total and available master builders (Dakshya Karmi).
    /// Manages construction site worker assignment, visual craftsman NPCs,
    /// and builder capacity constraints.
    /// </summary>
    public class BuilderManager : MonoBehaviour
    {
        public static BuilderManager Instance { get; private set; }

        [Header("Builder Capacity")]
        [SerializeField] private int _totalBuilders = 2;
        [SerializeField] private int _maxBuilders = 5;

        private readonly HashSet<BuildingInstance> _busySites = new();
        private readonly Dictionary<BuildingInstance, GameObject> _builderNPCs = new();

        public int TotalBuilders => _totalBuilders;
        public int MaxBuilders => _maxBuilders;
        public int BusyBuildersCount => _busySites.Count;
        public int AvailableBuildersCount => Mathf.Max(0, _totalBuilders - _busySites.Count);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool CanAssignBuilder()
        {
            return AvailableBuildersCount > 0;
        }

        public bool AssignBuilder(BuildingInstance building)
        {
            if (building == null || !CanAssignBuilder()) return false;

            _busySites.Add(building);

            // Spawn active craftsman NPC at building scaffolding
            if (!_builderNPCs.ContainsKey(building))
            {
                GameObject worker = new GameObject("Craftsman_Worker");
                worker.transform.SetParent(building.transform, false);
                worker.transform.localPosition = new Vector3(building.Data.tileWidth * 0.7f, 0, -building.Data.tileHeight * 0.7f);
                Nepali3DMeshBuilder.BuildUnit(worker.transform, hasKhukuri: false);

                // Add hammer bobbing animation
                worker.AddComponent<CraftsmanHammerAnimator>();
                _builderNPCs[building] = worker;
            }

            return true;
        }

        public void ReleaseBuilder(BuildingInstance building)
        {
            if (building == null) return;

            _busySites.Remove(building);

            if (_builderNPCs.TryGetValue(building, out GameObject worker) && worker != null)
            {
                Destroy(worker);
                _builderNPCs.Remove(building);
            }
        }

        public bool HireAdditionalBuilder(int gemCost = 250)
        {
            if (_totalBuilders >= _maxBuilders) return false;

            if (ResourceManager.Instance != null && ResourceManager.Instance.Gems >= gemCost)
            {
                var cost = new Dictionary<ResourceType, double> { { ResourceType.Gems, gemCost } };
                if (ResourceManager.Instance.SpendResources(cost))
                {
                    _totalBuilders++;
                    GorkhaAudio.PlayUpgrade();
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Simple procedural rhythmic hammering animation for builder craftsmen.
    /// </summary>
    public class CraftsmanHammerAnimator : MonoBehaviour
    {
        private float _timer = 0f;
        private Vector3 _basePos;

        private void Start()
        {
            _basePos = transform.localPosition;
        }

        private void Update()
        {
            _timer += Time.deltaTime * 6.0f;
            float hop = Mathf.Abs(Mathf.Sin(_timer)) * 0.12f;
            transform.localPosition = _basePos + new Vector3(0, hop, 0);

            // Subtle torso rotation
            float tilt = Mathf.Sin(_timer) * 12.0f;
            transform.localRotation = Quaternion.Euler(tilt, 45f, 0);
        }
    }
}
