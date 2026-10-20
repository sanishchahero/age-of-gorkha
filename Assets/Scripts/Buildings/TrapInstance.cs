using System.Collections;
using UnityEngine;
using Gorkha.Units;
using Gorkha.Audio;
using Gorkha.Graphics;
using Gorkha.UI;

namespace Gorkha.Buildings
{
    /// <summary>
    /// Himalayan Concealed Trap Component (Clash of Clans Spring & Spike Trap style).
    /// Camouflaged pit trap that springs when enemy raiders march across its perimeter.
    /// Deals high burst puncture damage and can auto-rearm.
    /// </summary>
    public class TrapInstance : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private float _triggerRadius = 1.35f;
        [SerializeField] private int _burstDamage = 85;
        [SerializeField] private float _rearmDelay = 10f;

        private BuildingInstance _building;
        private bool _isArmed = true;
        private Transform _spikesGroup;

        public bool IsArmed => _isArmed;

        private void Awake()
        {
            _building = GetComponent<BuildingInstance>();
            _spikesGroup = transform.Find("SpikeTrap_3D/SpikesGroup");
        }

        private void Start()
        {
            if (_spikesGroup == null)
            {
                _spikesGroup = transform.Find("SpikeTrap_3D/SpikesGroup");
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || !_isArmed) return;
            if (_building != null && _building.State != BuildingState.Active) return;

            // Check if any hostile unit stepped on the trap
            foreach (var u in UnitController.AllUnits)
            {
                if (u != null && u.IsHostile && u.IsAlive)
                {
                    float dist = Vector3.Distance(transform.position, u.transform.position);
                    if (dist <= _triggerRadius)
                    {
                        TriggerTrap();
                        break;
                    }
                }
            }
        }

        private void TriggerTrap()
        {
            _isArmed = false;

            GorkhaAudio.PlayHarvest();
            GorkhaAudio.PlayClick();
            GorkhaVFX.SpawnPlacementDust(transform.position, Vector2.one * 1.6f);
            GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 2.2f, "💥 SPIKE TRAP TRIGGERED!", Color.red);

            // Deal burst damage to all nearby hostiles
            foreach (var u in UnitController.AllUnits)
            {
                if (u != null && u.IsHostile && u.IsAlive)
                {
                    if (Vector3.Distance(transform.position, u.transform.position) <= _triggerRadius * 1.3f)
                    {
                        u.TakeDamage(_burstDamage);
                    }
                }
            }

            StartCoroutine(SpringAndRearmRoutine());
        }

        private IEnumerator SpringAndRearmRoutine()
        {
            // Spring animation: Spikes jump up
            if (_spikesGroup != null)
            {
                float t = 0f;
                Vector3 origPos = _spikesGroup.localPosition;
                Vector3 poppedPos = origPos + Vector3.up * 0.45f;

                while (t < 0.15f)
                {
                    t += Time.deltaTime;
                    _spikesGroup.localPosition = Vector3.Lerp(origPos, poppedPos, t / 0.15f);
                    yield return null;
                }

                yield return new WaitForSeconds(1.8f);

                // Slowly retract into pit
                t = 0f;
                while (t < 0.8f)
                {
                    t += Time.deltaTime;
                    _spikesGroup.localPosition = Vector3.Lerp(poppedPos, origPos, t / 0.8f);
                    yield return null;
                }
            }

            // Auto-rearm cooldown
            yield return new WaitForSeconds(_rearmDelay);
            _isArmed = true;
            GorkhaPlayableHUD.SpawnFloatingText(transform.position + Vector3.up * 1.5f, "⚙️ Trap Re-armed", Color.gray);
        }
    }
}
