using System;
using System.Collections;
using UnityEngine;
using Gorkha.Units;
using Gorkha.Buildings;
using Gorkha.Audio;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Simulates true gravitational parabolic flight for physical arrows and artillery projectiles.
    /// On impact, applies damage, triggers particle burst, and stays stuck in the target/ground for realism.
    /// </summary>
    public class BallisticProjectile : MonoBehaviour
    {
        public static BallisticProjectile LaunchArrow(Vector3 startPos, Vector3 targetPos, UnitController targetUnit, int damage = 25, UnitController attacker = null)
        {
            GameObject arrowObj = Nepali3DMeshBuilder.BuildArrow();
            arrowObj.transform.position = startPos;
            var proj = arrowObj.AddComponent<BallisticProjectile>();
            proj.Launch(startPos, targetPos, targetUnit, null, damage, arcHeight: 2.2f, duration: 0.55f, attacker: attacker);
            return proj;
        }

        public static BallisticProjectile LaunchArcherArrow(Vector3 startPos, UnitController targetUnit, int damage = 22, UnitController attacker = null)
        {
            if (targetUnit == null) return null;
            Vector3 targetPos = targetUnit.transform.position + Vector3.up * 0.8f;
            GameObject arrowObj = Nepali3DMeshBuilder.BuildArrow();
            arrowObj.transform.position = startPos;
            var proj = arrowObj.AddComponent<BallisticProjectile>();
            float dist = Vector3.Distance(startPos, targetPos);
            float flightTime = Mathf.Clamp(dist / 18f, 0.35f, 0.75f);
            float arc = Mathf.Clamp(dist * 0.22f, 1.2f, 3.2f);
            proj.Launch(startPos, targetPos, targetUnit, null, damage, arcHeight: arc, duration: flightTime, attacker: attacker);
            return proj;
        }

        private Vector3 _start;
        private Vector3 _target;
        private UnitController _targetUnit;
        private UnitController _attackerUnit;
        private BuildingInstance _targetBuilding;
        private int _damage;
        private float _arcHeight;
        private float _duration;
        private bool _isFlying = false;

        public bool IsFlying => _isFlying;

        public void Launch(Vector3 start, Vector3 target, UnitController targetUnit, BuildingInstance targetBuilding, int damage, float arcHeight, float duration, UnitController attacker = null)
        {
            _start = start;
            _target = target;
            _targetUnit = targetUnit;
            _attackerUnit = attacker;
            _targetBuilding = targetBuilding;
            _damage = damage;
            _arcHeight = arcHeight;
            _duration = duration;

            StartCoroutine(FlightRoutine());
        }

        private IEnumerator FlightRoutine()
        {
            _isFlying = true;
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);

                // Subtly lead moving targets
                Vector3 currentTarget = _target;
                if (_targetUnit != null && _targetUnit.IsAlive)
                {
                    currentTarget = _targetUnit.transform.position + Vector3.up * 0.8f;
                }

                Vector3 currentPos = Vector3.Lerp(_start, currentTarget, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * _arcHeight;

                Vector3 nextPos = Vector3.Lerp(_start, currentTarget, Mathf.Min(1f, t + 0.05f));
                nextPos.y += Mathf.Sin(Mathf.Min(1f, t + 0.05f) * Mathf.PI) * _arcHeight;

                Vector3 dir = nextPos - currentPos;
                if (dir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(dir.normalized);
                }

                transform.position = currentPos;
                yield return null;
            }

            OnImpact();
        }

        private void OnImpact()
        {
            _isFlying = false;

            // Apply Damage
            if (_targetUnit != null && _targetUnit.IsAlive)
            {
                if (_attackerUnit != null)
                {
                    _targetUnit.TakeDamageFrom(_attackerUnit, _damage);
                }
                else
                {
                    _targetUnit.TakeDamage(_damage);
                }
                transform.SetParent(_targetUnit.transform, true); // Stick into unit!
            }
            else if (_targetBuilding != null && _targetBuilding.State == BuildingState.Active)
            {
                _targetBuilding.TakeDamage(_damage);
                transform.SetParent(_targetBuilding.transform, true); // Stick into building!
            }

            // Impact audio and dust
            GorkhaAudio.PlayArrowImpact();
            GorkhaVFX.SpawnPlacementDust(transform.position, Vector2.one * 0.35f);

            // Destroy after staying stuck for 2.8 seconds
            Destroy(gameObject, 2.8f);
        }
    }
}
