using System;
using System.Collections;
using UnityEngine;
using Gorkha.Audio;
using Gorkha.Units;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Articulated procedural skeletal animator for RTS units.
    /// Drives human joint walk cycles, breathing, and true combat animations:
    /// - Opposing hip leg swings and counter-phase shoulder arm swings
    /// - Class-specific combat ready postures (Khukuri waist guard, Spearman phalanx, Archer scan, Villager Doko lean)
    /// - Khukuri slash (windup, diagonal cross-slash, recovery)
    /// - Spearman lunge (two-handed forward thrust with 0.45m reach)
    /// - Archer bowstring draw & release with recoil
    /// - Villager two-handed harvest chopping
    /// - Hit flinch knockback
    /// </summary>
    public class UnitProceduralAnimator : MonoBehaviour
    {
        [Header("Joint Pivots")]
        [SerializeField] private Transform _hipLeft;
        [SerializeField] private Transform _hipRight;
        [SerializeField] private Transform _shoulderLeft;
        [SerializeField] private Transform _shoulderRight;
        [SerializeField] private Transform _torso;
        [SerializeField] private Transform _head;

        [Header("Walk Tuning")]
        [SerializeField] private float _walkBobSpeed = 11.5f;
        [SerializeField] private float _legSwingAngle = 32.0f;
        [SerializeField] private float _armSwingAngle = 24.0f;
        [SerializeField] private float _torsoBounceAmount = 0.055f;

        [Header("Idle Tuning")]
        [SerializeField] private float _idleBreatheSpeed = 2.4f;
        [SerializeField] private float _idleBreatheAmount = 0.02f;

        private Vector3 _lastWorldPos;
        private Vector3 _torsoBasePos;
        private Vector3 _shoulderLBasePos;
        private Vector3 _shoulderRBasePos;
        private Quaternion _hipLBaseRot = Quaternion.identity;
        private Quaternion _hipRBaseRot = Quaternion.identity;
        private Quaternion _torsoBaseRot = Quaternion.identity;
        private Quaternion _headBaseRot = Quaternion.identity;
        private Quaternion _shoulderLBaseRot = Quaternion.identity;
        private Quaternion _shoulderRBaseRot = Quaternion.identity;

        private Quaternion _rawTorsoRot = Quaternion.identity;
        private Quaternion _rawShoulderLRot = Quaternion.identity;
        private Quaternion _rawShoulderRRot = Quaternion.identity;
        private bool _originalRotCached = false;

        private UnitType _unitType = UnitType.KhukuriWarrior;
        private bool _isHostile = false;

        private float _walkCycleTimer = 0f;
        private float _idleTimer = 0f;
        private bool _isAttacking = false;
        private Coroutine _actionRoutine;

        public bool IsAttacking => _isAttacking;

        private void Awake()
        {
            AutoCacheLimbs();
        }

        private void Start()
        {
            _lastWorldPos = transform.position;
            CacheBasePositions();
            ApplyClassReadyStance();
        }

        public void SetupLimbs(Transform hipL, Transform hipR, Transform shoulderL, Transform shoulderR, Transform torso, Transform head)
        {
            _hipLeft = hipL;
            _hipRight = hipR;
            _shoulderLeft = shoulderL;
            _shoulderRight = shoulderR;
            _torso = torso;
            _head = head;

            CacheBasePositions();
            ApplyClassReadyStance();
        }

        public void SetUnitClass(UnitType unitType, bool isHostile = false)
        {
            _unitType = unitType;
            _isHostile = isHostile;
            ApplyClassReadyStance();
        }

        private void ApplyClassReadyStance()
        {
            if (!_originalRotCached)
            {
                _rawTorsoRot = _torsoBaseRot;
                _rawShoulderLRot = _shoulderLBaseRot;
                _rawShoulderRRot = _shoulderRBaseRot;
                _originalRotCached = true;
            }

            if (_isHostile)
            {
                // Aggressive hunched marauder stance with dual forward blades
                _torsoBaseRot = _rawTorsoRot * Quaternion.Euler(10f, 0, 0);
                _shoulderRBaseRot = _rawShoulderRRot * Quaternion.Euler(-30f, 22f, 16f);
                _shoulderLBaseRot = _rawShoulderLRot * Quaternion.Euler(-30f, -22f, -16f);
                return;
            }

            switch (_unitType)
            {
                case UnitType.Spearman:
                    // Phalanx stance: spear lowered forward at 25 deg, pavise shield squared to torso
                    _torsoBaseRot = _rawTorsoRot * Quaternion.Euler(5f, -8f, 0);
                    _shoulderRBaseRot = _rawShoulderRRot * Quaternion.Euler(-32f, 10f, 4f);
                    _shoulderLBaseRot = _rawShoulderLRot * Quaternion.Euler(-22f, -18f, -10f);
                    break;

                case UnitType.Archer:
                    // Scout stance: upright bow, scanning posture
                    _torsoBaseRot = _rawTorsoRot * Quaternion.Euler(0, -10f, 0);
                    _shoulderLBaseRot = _rawShoulderLRot * Quaternion.Euler(-18f, -10f, 0);
                    _shoulderRBaseRot = _rawShoulderRRot * Quaternion.Euler(6f, 12f, 0);
                    break;

                case UnitType.Villager:
                    // Slight forward lean to counterbalance the heavy woven Doko basket
                    _torsoBaseRot = _rawTorsoRot * Quaternion.Euler(7f, 0, 0);
                    _shoulderRBaseRot = _rawShoulderRRot * Quaternion.Euler(-14f, 15f, 0);
                    _shoulderLBaseRot = _rawShoulderLRot * Quaternion.Euler(0, -8f, 0);
                    break;

                case UnitType.KhukuriWarrior:
                default:
                    // Authentic Gorkhali martial guard: right hand holding curved Khukuri forward at waist height, buckler raised
                    _torsoBaseRot = _rawTorsoRot * Quaternion.Euler(4f, 6f, 0);
                    _shoulderRBaseRot = _rawShoulderRRot * Quaternion.Euler(-26f, 20f, 14f);
                    _shoulderLBaseRot = _rawShoulderLRot * Quaternion.Euler(-18f, -16f, -12f);
                    break;
            }
        }

        private void AutoCacheLimbs()
        {
            if (_hipLeft == null) _hipLeft = FindDeepChild(transform, "UpperLeg_L") ?? FindDeepChild(transform, "HipLeft");
            if (_hipRight == null) _hipRight = FindDeepChild(transform, "UpperLeg_R") ?? FindDeepChild(transform, "HipRight");
            if (_shoulderLeft == null) _shoulderLeft = FindDeepChild(transform, "Shoulder_L") ?? FindDeepChild(transform, "Clavicle_L") ?? FindDeepChild(transform, "ShoulderLeft");
            if (_shoulderRight == null) _shoulderRight = FindDeepChild(transform, "Shoulder_R") ?? FindDeepChild(transform, "Clavicle_R") ?? FindDeepChild(transform, "ShoulderRight");
            if (_torso == null) _torso = FindDeepChild(transform, "Hips") ?? FindDeepChild(transform, "Spine_01") ?? FindDeepChild(transform, "TorsoRoot") ?? FindDeepChild(transform, "Torso");
            if (_head == null) _head = FindDeepChild(transform, "Head") ?? FindDeepChild(transform, "HeadRoot");
        }

        private void CacheBasePositions()
        {
            if (_torso != null)
            {
                _torsoBasePos = _torso.localPosition;
                _torsoBaseRot = _torso.localRotation;
            }
            if (_head != null)
            {
                _headBaseRot = _head.localRotation;
            }
            if (_hipLeft != null)
            {
                _hipLBaseRot = _hipLeft.localRotation;
            }
            if (_hipRight != null)
            {
                _hipRBaseRot = _hipRight.localRotation;
            }
            if (_shoulderLeft != null)
            {
                _shoulderLBasePos = _shoulderLeft.localPosition;
                _shoulderLBaseRot = _shoulderLeft.localRotation;
            }
            if (_shoulderRight != null)
            {
                _shoulderRBasePos = _shoulderRight.localPosition;
                _shoulderRBaseRot = _shoulderRight.localRotation;
            }

            if (!_originalRotCached)
            {
                _rawTorsoRot = _torsoBaseRot;
                _rawShoulderLRot = _shoulderLBaseRot;
                _rawShoulderRRot = _shoulderRBaseRot;
                _originalRotCached = true;
            }
        }

        private Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void Update()
        {
            float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
            float speed = (transform.position - _lastWorldPos).magnitude / deltaTime;
            _lastWorldPos = transform.position;

            bool isMoving = speed > 0.12f;

            if (isMoving)
            {
                _walkCycleTimer += deltaTime * _walkBobSpeed;
                float sinSwing = Mathf.Sin(_walkCycleTimer);

                // Articulated Hip Rotations
                if (_hipLeft != null)
                {
                    _hipLeft.localRotation = _hipLBaseRot * Quaternion.Euler(sinSwing * _legSwingAngle, 0, 0);
                }
                if (_hipRight != null)
                {
                    _hipRight.localRotation = _hipRBaseRot * Quaternion.Euler(-sinSwing * _legSwingAngle, 0, 0);
                }

                // Vertical Torso Bounce & Subtle Tilt
                if (_torso != null)
                {
                    float yBob = Mathf.Abs(Mathf.Sin(_walkCycleTimer)) * _torsoBounceAmount;
                    _torso.localPosition = _torsoBasePos + new Vector3(0, yBob, 0);
                    _torso.localRotation = _torsoBaseRot * Quaternion.Euler(0, 0, sinSwing * 2.5f);
                }

                // Opposing Arm Swings (Only if not in combat/harvest action)
                if (!_isAttacking)
                {
                    if (_shoulderLeft != null)
                    {
                        _shoulderLeft.localRotation = _shoulderLBaseRot * Quaternion.Euler(-sinSwing * _armSwingAngle, 0, 0);
                    }
                    if (_shoulderRight != null)
                    {
                        _shoulderRight.localRotation = _shoulderRBaseRot * Quaternion.Euler(sinSwing * _armSwingAngle, 0, 0);
                    }
                }
            }
            else
            {
                _idleTimer += deltaTime * _idleBreatheSpeed;
                float breath = Mathf.Sin(_idleTimer) * _idleBreatheAmount;

                // Ease legs back to neutral stance
                if (_hipLeft != null)
                {
                    _hipLeft.localRotation = Quaternion.Slerp(_hipLeft.localRotation, _hipLBaseRot, 8f * deltaTime);
                }
                if (_hipRight != null)
                {
                    _hipRight.localRotation = Quaternion.Slerp(_hipRight.localRotation, _hipRBaseRot, 8f * deltaTime);
                }

                // Gentle breathing rhythm on Torso
                if (_torso != null)
                {
                    _torso.localPosition = Vector3.Lerp(_torso.localPosition, _torsoBasePos + new Vector3(0, breath, 0), 6f * deltaTime);
                    _torso.localRotation = Quaternion.Slerp(_torso.localRotation, _torsoBaseRot, 6f * deltaTime);
                }

                // Return shoulders to relaxed ready posture
                if (!_isAttacking)
                {
                    if (_shoulderLeft != null)
                    {
                        _shoulderLeft.localRotation = Quaternion.Slerp(_shoulderLeft.localRotation, _shoulderLBaseRot, 7f * deltaTime);
                        _shoulderLeft.localPosition = Vector3.Lerp(_shoulderLeft.localPosition, _shoulderLBasePos, 7f * deltaTime);
                    }
                    if (_shoulderRight != null)
                    {
                        _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRight.localRotation, _shoulderRBaseRot, 7f * deltaTime);
                        _shoulderRight.localPosition = Vector3.Lerp(_shoulderRight.localPosition, _shoulderRBasePos, 7f * deltaTime);
                    }
                }

                // Vigilant perimeter scanning: head subtly pans left and right
                if (_head != null && !_isAttacking)
                {
                    float scan = Mathf.Sin(_idleTimer * 0.75f) * 8.5f;
                    _head.localRotation = Quaternion.Slerp(_head.localRotation, _headBaseRot * Quaternion.Euler(0, scan, 0), 4f * deltaTime);
                }
            }
        }

        #region Combat & Harvest Actions

        public void PlayKhukuriSlash(Action onHit = null)
        {
            if (_actionRoutine != null) StopCoroutine(_actionRoutine);
            _actionRoutine = StartCoroutine(KhukuriSlashRoutine(onHit));
        }

        private IEnumerator KhukuriSlashRoutine(Action onHit)
        {
            _isAttacking = true;
            float elapsed = 0f;
            float windupTime = 0.16f;
            float slashTime = 0.12f;
            float recoverTime = 0.22f;

            // Phase 1: High Windup (Arm pulls back and up)
            while (elapsed < windupTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / windupTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot, _shoulderRBaseRot * Quaternion.Euler(-65f, 25f, -15f), t);
                }
                if (_torso != null)
                {
                    _torso.localRotation = _torsoBaseRot * Quaternion.Euler(0, -18f * t, 0);
                }
                yield return null;
            }

            // Phase 2: Downward Diagonal Cross-Slash!
            elapsed = 0f;
            bool hitTriggered = false;
            while (elapsed < slashTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slashTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot * Quaternion.Euler(-65f, 25f, -15f), _shoulderRBaseRot * Quaternion.Euler(55f, -35f, 20f), t);
                }
                if (_torso != null)
                {
                    _torso.localRotation = _torsoBaseRot * Quaternion.Euler(0, Mathf.Lerp(-18f, 22f, t), 0);
                }

                if (!hitTriggered && t >= 0.5f)
                {
                    hitTriggered = true;
                    GorkhaAudio.PlaySwordClash();
                    onHit?.Invoke();
                }
                yield return null;
            }

            if (!hitTriggered) onHit?.Invoke();

            // Phase 3: Smooth Recovery
            elapsed = 0f;
            Quaternion endRot = _shoulderRight != null ? _shoulderRight.localRotation : Quaternion.identity;
            Quaternion endTorso = _torso != null ? _torso.localRotation : Quaternion.identity;

            while (elapsed < recoverTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / recoverTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(endRot, _shoulderRBaseRot, t);
                }
                if (_torso != null)
                {
                    _torso.localRotation = Quaternion.Slerp(endTorso, _torsoBaseRot, t);
                }
                yield return null;
            }

            _isAttacking = false;
            _actionRoutine = null;
        }

        public void PlaySpearLunge(Action onHit = null)
        {
            if (_actionRoutine != null) StopCoroutine(_actionRoutine);
            _actionRoutine = StartCoroutine(SpearLungeRoutine(onHit));
        }

        private IEnumerator SpearLungeRoutine(Action onHit)
        {
            _isAttacking = true;
            float elapsed = 0f;
            float prepTime = 0.14f;
            float thrustTime = 0.12f;
            float retractTime = 0.24f;

            // Phase 1: Draw back spear
            while (elapsed < prepTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / prepTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localPosition = Vector3.Lerp(_shoulderRBasePos, _shoulderRBasePos + new Vector3(0, 0, -0.22f), t);
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot, _shoulderRBaseRot * Quaternion.Euler(-15f, 0, 0), t);
                }
                yield return null;
            }

            // Phase 2: Powerful Forward Spear Drive (0.45m reach)
            elapsed = 0f;
            bool hitTriggered = false;
            while (elapsed < thrustTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / thrustTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localPosition = Vector3.Lerp(_shoulderRBasePos + new Vector3(0, 0, -0.22f), _shoulderRBasePos + new Vector3(0, 0, 0.45f), t);
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot * Quaternion.Euler(-15f, 0, 0), _shoulderRBaseRot * Quaternion.Euler(18f, 0, 0), t);
                }
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localPosition = Vector3.Lerp(_shoulderLBasePos, _shoulderLBasePos + new Vector3(0, 0, 0.25f), t);
                }

                if (!hitTriggered && t >= 0.5f)
                {
                    hitTriggered = true;
                    GorkhaAudio.PlayShieldBlock();
                    onHit?.Invoke();
                }
                yield return null;
            }

            if (!hitTriggered) onHit?.Invoke();

            // Phase 3: Retract back to guard stance
            elapsed = 0f;
            Vector3 thrustPosR = _shoulderRight != null ? _shoulderRight.localPosition : _shoulderRBasePos;
            Vector3 thrustPosL = _shoulderLeft != null ? _shoulderLeft.localPosition : _shoulderLBasePos;

            while (elapsed < retractTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / retractTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localPosition = Vector3.Lerp(thrustPosR, _shoulderRBasePos, t);
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRight.localRotation, _shoulderRBaseRot, t);
                }
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localPosition = Vector3.Lerp(thrustPosL, _shoulderLBasePos, t);
                }
                yield return null;
            }

            _isAttacking = false;
            _actionRoutine = null;
        }

        public void PlayBowRelease(Action onRelease = null)
        {
            if (_actionRoutine != null) StopCoroutine(_actionRoutine);
            _actionRoutine = StartCoroutine(BowReleaseRoutine(onRelease));
        }

        private IEnumerator BowReleaseRoutine(Action onRelease)
        {
            _isAttacking = true;
            float elapsed = 0f;
            float drawTime = 0.35f;
            float releaseTime = 0.08f;
            float recoverTime = 0.25f;

            // Phase 1: Raise bow arm & draw string to cheek
            while (elapsed < drawTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / drawTime);
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localRotation = Quaternion.Slerp(_shoulderLBaseRot, _shoulderLBaseRot * Quaternion.Euler(-80f, -15f, 10f), t);
                }
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot, _shoulderRBaseRot * Quaternion.Euler(-68f, 32f, -12f), t);
                }
                if (_torso != null)
                {
                    _torso.localRotation = _torsoBaseRot * Quaternion.Euler(0, -20f * t, 0);
                }
                yield return null;
            }

            // Phase 2: Snap Release!
            elapsed = 0f;
            GorkhaAudio.PlayBowTwang();
            onRelease?.Invoke();

            while (elapsed < releaseTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / releaseTime);
                // Slight bow vibration kick
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localRotation = _shoulderLBaseRot * Quaternion.Euler(-85f, -15f, 10f);
                }
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot * Quaternion.Euler(-68f, 32f, -12f), _shoulderRBaseRot * Quaternion.Euler(-45f, 15f, 0), t);
                }
                yield return null;
            }

            // Phase 3: Lower bow smoothly
            elapsed = 0f;
            Quaternion endL = _shoulderLeft != null ? _shoulderLeft.localRotation : Quaternion.identity;
            Quaternion endR = _shoulderRight != null ? _shoulderRight.localRotation : Quaternion.identity;
            Quaternion endTorso = _torso != null ? _torso.localRotation : Quaternion.identity;

            while (elapsed < recoverTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / recoverTime);
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localRotation = Quaternion.Slerp(endL, _shoulderLBaseRot, t);
                }
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(endR, _shoulderRBaseRot, t);
                }
                if (_torso != null)
                {
                    _torso.localRotation = Quaternion.Slerp(endTorso, _torsoBaseRot, t);
                }
                yield return null;
            }

            _isAttacking = false;
            _actionRoutine = null;
        }

        public void PlayHarvestChop(Action onChop = null)
        {
            if (_actionRoutine != null) StopCoroutine(_actionRoutine);
            _actionRoutine = StartCoroutine(HarvestChopRoutine(onChop));
        }

        private IEnumerator HarvestChopRoutine(Action onChop)
        {
            _isAttacking = true;
            float elapsed = 0f;
            float raiseTime = 0.22f;
            float strikeTime = 0.12f;
            float recoverTime = 0.28f;

            // Phase 1: Raise axe overhead
            while (elapsed < raiseTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / raiseTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot, _shoulderRBaseRot * Quaternion.Euler(-92f, 0, 15f), t);
                }
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localRotation = Quaternion.Slerp(_shoulderLBaseRot, _shoulderLBaseRot * Quaternion.Euler(-75f, 25f, 0), t);
                }
                if (_torso != null)
                {
                    _torso.localRotation = _torsoBaseRot * Quaternion.Euler(-12f * t, 0, 0);
                }
                yield return null;
            }

            // Phase 2: Power chop down!
            elapsed = 0f;
            bool hitTriggered = false;
            while (elapsed < strikeTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / strikeTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot * Quaternion.Euler(-92f, 0, 15f), _shoulderRBaseRot * Quaternion.Euler(58f, 0, 0), t);
                }
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localRotation = Quaternion.Slerp(_shoulderLBaseRot * Quaternion.Euler(-75f, 25f, 0), _shoulderLBaseRot * Quaternion.Euler(45f, 0, 0), t);
                }
                if (_torso != null)
                {
                    _torso.localRotation = _torsoBaseRot * Quaternion.Euler(Mathf.Lerp(-12f, 16f, t), 0, 0);
                }

                if (!hitTriggered && t >= 0.5f)
                {
                    hitTriggered = true;
                    GorkhaAudio.PlayHarvest();
                    onChop?.Invoke();
                }
                yield return null;
            }

            if (!hitTriggered) onChop?.Invoke();

            // Phase 3: Rebound & return
            elapsed = 0f;
            Quaternion endR = _shoulderRight != null ? _shoulderRight.localRotation : Quaternion.identity;
            Quaternion endL = _shoulderLeft != null ? _shoulderLeft.localRotation : Quaternion.identity;
            Quaternion endTorso = _torso != null ? _torso.localRotation : Quaternion.identity;

            while (elapsed < recoverTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / recoverTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(endR, _shoulderRBaseRot, t);
                }
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localRotation = Quaternion.Slerp(endL, _shoulderLBaseRot, t);
                }
                if (_torso != null)
                {
                    _torso.localRotation = Quaternion.Slerp(endTorso, _torsoBaseRot, t);
                }
                yield return null;
            }

            _isAttacking = false;
            _actionRoutine = null;
        }

        public void PlayHitFlinch()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(HitFlinchRoutine());
            }
        }

        private IEnumerator HitFlinchRoutine()
        {
            float elapsed = 0f;
            float flinchDuration = 0.22f;

            while (elapsed < flinchDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / flinchDuration) * Mathf.PI);

                if (_torso != null)
                {
                    _torso.localRotation = _torsoBaseRot * Quaternion.Euler(-18f * t, 0, 0);
                }
                if (_head != null)
                {
                    _head.localRotation = _headBaseRot * Quaternion.Euler(-14f * t, 0, 0);
                }
                if (!_isAttacking)
                {
                    if (_shoulderLeft != null)
                    {
                        _shoulderLeft.localRotation = _shoulderLBaseRot * Quaternion.Euler(15f * t, -20f * t, -15f * t);
                    }
                    if (_shoulderRight != null)
                    {
                        _shoulderRight.localRotation = _shoulderRBaseRot * Quaternion.Euler(15f * t, 20f * t, 15f * t);
                    }
                }
                yield return null;
            }
        }

        #endregion
    }
}
