using System;
using System.Collections;
using UnityEngine;
using Gorkha.Audio;
using Gorkha.Units;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Master articulated humanoid procedural animator for RTS units and villagers.
    /// Provides studio-grade skeletal kinematics, biomechanical locomotion, and historical martial animations:
    /// - 10-Bone Skeletal Kinematics (Hips, Spine, Head, Left/Right Thighs, Knees, Ankles, Shoulders, Elbows, Hands)
    /// - Locomotion with Knee Flexion & Elbow Stride: Knees bend naturally up to 45° during forward swing, ankles pitch, elbows flex
    /// - Pelvic vertical bobbing & upper torso counter-rotational torque balance
    /// - Authentic Gorkhali martial stances (Waist Khukuri guard, Phalanx spear, Scout bow, Doko balance lean)
    /// - Fluid combat actions: Khukuri cleave with core coiling, Spearman 0.5m piston lunge, Archer bowstring draw & recoil kick
    /// - Authentic civilian work: Two-handed overhead wood/rock chopping, terraced farm sickle cultivating
    /// - Physical hit flinch & ragdoll-like death collapse onto mountain earth
    /// </summary>
    public class UnitProceduralAnimator : MonoBehaviour
    {
        [Header("Humanoid Joint Pivots")]
        [SerializeField] private Transform _hips;
        [SerializeField] private Transform _spine;
        [SerializeField] private Transform _torso; // Alias / fallback
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _hipLeft;
        [SerializeField] private Transform _kneeLeft;
        [SerializeField] private Transform _ankleLeft;
        [SerializeField] private Transform _hipRight;
        [SerializeField] private Transform _kneeRight;
        [SerializeField] private Transform _ankleRight;
        [SerializeField] private Transform _shoulderLeft;
        [SerializeField] private Transform _elbowLeft;
        [SerializeField] private Transform _handLeft;
        [SerializeField] private Transform _shoulderRight;
        [SerializeField] private Transform _elbowRight;
        [SerializeField] private Transform _handRight;

        [Header("Locomotion Tuning")]
        [SerializeField] private float _walkBobSpeed = 11.5f;
        [SerializeField] private float _legSwingAngle = 32.0f;
        [SerializeField] private float _kneeBendAngle = 42.0f;
        [SerializeField] private float _armSwingAngle = 24.0f;
        [SerializeField] private float _pelvisBounceAmount = 0.038f;

        [Header("Idle Tuning")]
        [SerializeField] private float _idleBreatheSpeed = 2.4f;
        [SerializeField] private float _idleBreatheAmount = 0.018f;

        private Vector3 _lastWorldPos;
        private Vector3 _hipsBasePos;
        private Vector3 _torsoBasePos;
        private Vector3 _shoulderLBasePos;
        private Vector3 _shoulderRBasePos;

        private Quaternion _hipsBaseRot = Quaternion.identity;
        private Quaternion _spineBaseRot = Quaternion.identity;
        private Quaternion _torsoBaseRot = Quaternion.identity;
        private Quaternion _headBaseRot = Quaternion.identity;
        private Quaternion _hipLBaseRot = Quaternion.identity;
        private Quaternion _hipRBaseRot = Quaternion.identity;
        private Quaternion _kneeLBaseRot = Quaternion.identity;
        private Quaternion _kneeRBaseRot = Quaternion.identity;
        private Quaternion _ankleLBaseRot = Quaternion.identity;
        private Quaternion _ankleRBaseRot = Quaternion.identity;
        private Quaternion _shoulderLBaseRot = Quaternion.identity;
        private Quaternion _shoulderRBaseRot = Quaternion.identity;
        private Quaternion _elbowLBaseRot = Quaternion.identity;
        private Quaternion _elbowRBaseRot = Quaternion.identity;
        private Quaternion _handLBaseRot = Quaternion.identity;
        private Quaternion _handRBaseRot = Quaternion.identity;

        private Quaternion _rawSpineRot = Quaternion.identity;
        private Quaternion _rawShoulderLRot = Quaternion.identity;
        private Quaternion _rawShoulderRRot = Quaternion.identity;
        private Quaternion _rawElbowLRot = Quaternion.identity;
        private Quaternion _rawElbowRRot = Quaternion.identity;
        private bool _originalRotCached = false;

        private UnitType _unitType = UnitType.KhukuriWarrior;
        private bool _isHostile = false;

        private float _walkCycleTimer = 0f;
        private float _idleTimer = 0f;
        private bool _isAttacking = false;
        private bool _isDead = false;
        private Coroutine _actionRoutine;

        public bool IsAttacking => _isAttacking;
        public bool IsDead => _isDead;

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

        /// <summary>
        /// Full humanoid 15-joint setup for Synty and articulated character rigs.
        /// </summary>
        public void SetupFullHumanoidRig(
            Transform hips, Transform spine, Transform head,
            Transform hipL, Transform kneeL, Transform ankleL,
            Transform hipR, Transform kneeR, Transform ankleR,
            Transform shoulderL, Transform elbowL, Transform handL,
            Transform shoulderR, Transform elbowR, Transform handR)
        {
            _hips = hips;
            _spine = spine;
            _torso = spine ?? hips;
            _head = head;

            _hipLeft = hipL;
            _kneeLeft = kneeL;
            _ankleLeft = ankleL;

            _hipRight = hipR;
            _kneeRight = kneeR;
            _ankleRight = ankleR;

            _shoulderLeft = shoulderL;
            _elbowLeft = elbowL;
            _handLeft = handL;

            _shoulderRight = shoulderR;
            _elbowRight = elbowR;
            _handRight = handR;

            CacheBasePositions();
            ApplyClassReadyStance();
        }

        /// <summary>
        /// Backwards-compatible setup overload for procedural block characters.
        /// </summary>
        public void SetupLimbs(Transform hipL, Transform hipR, Transform shoulderL, Transform shoulderR, Transform torso, Transform head)
        {
            _hipLeft = hipL;
            _hipRight = hipR;
            _shoulderLeft = shoulderL;
            _shoulderRight = shoulderR;
            _torso = torso;
            _spine = torso;
            _head = head;

            AutoCacheLimbs();
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
                _rawSpineRot = _spineBaseRot;
                _rawShoulderLRot = _shoulderLBaseRot;
                _rawShoulderRRot = _shoulderRBaseRot;
                _rawElbowLRot = _elbowLBaseRot;
                _rawElbowRRot = _elbowRBaseRot;
                _originalRotCached = true;
            }

            if (_isHostile)
            {
                // Aggressive hunched marauder stance with dual forward blades
                _spineBaseRot = _rawSpineRot * Quaternion.Euler(12f, 0, 0);
                _shoulderRBaseRot = _rawShoulderRRot * Quaternion.Euler(-32f, 22f, 16f);
                _shoulderLBaseRot = _rawShoulderLRot * Quaternion.Euler(-32f, -22f, -16f);
                _elbowRBaseRot = _rawElbowRRot * Quaternion.Euler(-55f, 0, 0);
                _elbowLBaseRot = _rawElbowLRot * Quaternion.Euler(-55f, 0, 0);
                return;
            }

            switch (_unitType)
            {
                case UnitType.HeroKaluPande:
                case UnitType.Spearman:
                    // Phalanx guard: spear held low-forward, pavise shield locked across chest
                    _spineBaseRot = _rawSpineRot * Quaternion.Euler(6f, -10f, 0);
                    _shoulderRBaseRot = _rawShoulderRRot * Quaternion.Euler(-32f, 12f, 4f);
                    _shoulderLBaseRot = _rawShoulderLRot * Quaternion.Euler(-24f, -18f, -10f);
                    _elbowRBaseRot = _rawElbowRRot * Quaternion.Euler(-38f, 0, 0);
                    _elbowLBaseRot = _rawElbowLRot * Quaternion.Euler(-52f, 0, 0);
                    break;

                case UnitType.Archer:
                    // Scout posture: upright bow, scanning horizon
                    _spineBaseRot = _rawSpineRot * Quaternion.Euler(0, -10f, 0);
                    _shoulderLBaseRot = _rawShoulderLRot * Quaternion.Euler(-20f, -10f, 0);
                    _shoulderRBaseRot = _rawShoulderRRot * Quaternion.Euler(8f, 14f, 0);
                    _elbowLBaseRot = _rawElbowLRot * Quaternion.Euler(-25f, 0, 0);
                    _elbowRBaseRot = _rawElbowRRot * Quaternion.Euler(-35f, 0, 0);
                    break;

                case UnitType.Villager:
                    // Slight forward lean to counterbalance the heavy woven Doko basket
                    _spineBaseRot = _rawSpineRot * Quaternion.Euler(8f, 0, 0);
                    _shoulderRBaseRot = _rawShoulderRRot * Quaternion.Euler(-16f, 14f, 0);
                    _shoulderLBaseRot = _rawShoulderLRot * Quaternion.Euler(-8f, -10f, 0);
                    _elbowRBaseRot = _rawElbowRRot * Quaternion.Euler(-42f, 0, 0);
                    _elbowLBaseRot = _rawElbowLRot * Quaternion.Euler(-36f, 0, 0);
                    break;

                case UnitType.HeroPrithvi:
                case UnitType.KhukuriWarrior:
                default:
                    // Authentic Gorkhali martial guard: right hand holding curved Khukuri forward at waist height, buckler raised
                    _spineBaseRot = _rawSpineRot * Quaternion.Euler(4f, 6f, 0);
                    _shoulderRBaseRot = _rawShoulderRRot * Quaternion.Euler(-26f, 20f, 14f);
                    _shoulderLBaseRot = _rawShoulderLRot * Quaternion.Euler(-18f, -16f, -12f);
                    _elbowRBaseRot = _rawElbowRRot * Quaternion.Euler(-60f, 0, 0);
                    _elbowLBaseRot = _rawElbowLRot * Quaternion.Euler(-55f, 0, 0);
                    break;
            }

            _torsoBaseRot = _spineBaseRot;
        }

        private void AutoCacheLimbs()
        {
            if (_hips == null) _hips = FindDeepChild(transform, "mixamorig:Hips", "Hips", "Torso");
            if (_spine == null) _spine = FindDeepChild(transform, "mixamorig:Spine2", "mixamorig:Spine1", "mixamorig:Spine", "Spine_02", "Spine_01", "TorsoRoot", "Torso");
            if (_torso == null) _torso = _spine ?? _hips;
            if (_head == null) _head = FindDeepChild(transform, "mixamorig:Head", "Head", "HeadRoot");

            if (_hipLeft == null) _hipLeft = FindDeepChild(transform, "mixamorig:LeftUpLeg", "UpperLeg_L", "HipLeft");
            if (_hipRight == null) _hipRight = FindDeepChild(transform, "mixamorig:RightUpLeg", "UpperLeg_R", "HipRight");
            if (_kneeLeft == null) _kneeLeft = FindDeepChild(transform, "mixamorig:LeftLeg", "LowerLeg_L");
            if (_kneeRight == null) _kneeRight = FindDeepChild(transform, "mixamorig:RightLeg", "LowerLeg_R");
            if (_ankleLeft == null) _ankleLeft = FindDeepChild(transform, "mixamorig:LeftFoot", "Ankle_L");
            if (_ankleRight == null) _ankleRight = FindDeepChild(transform, "mixamorig:RightFoot", "Ankle_R");

            if (_shoulderLeft == null) _shoulderLeft = FindDeepChild(transform, "mixamorig:LeftArm", "mixamorig:LeftShoulder", "Shoulder_L", "Clavicle_L", "ShoulderLeft");
            if (_shoulderRight == null) _shoulderRight = FindDeepChild(transform, "mixamorig:RightArm", "mixamorig:RightShoulder", "Shoulder_R", "Clavicle_R", "ShoulderRight");
            if (_elbowLeft == null) _elbowLeft = FindDeepChild(transform, "mixamorig:LeftForeArm", "Elbow_L");
            if (_elbowRight == null) _elbowRight = FindDeepChild(transform, "mixamorig:RightForeArm", "Elbow_R");
            if (_handLeft == null) _handLeft = FindDeepChild(transform, "mixamorig:LeftHand", "Hand_L");
            if (_handRight == null) _handRight = FindDeepChild(transform, "mixamorig:RightHand", "Hand_R");
        }

        private void CacheBasePositions()
        {
            if (_hips != null)
            {
                _hipsBasePos = _hips.localPosition;
                _hipsBaseRot = _hips.localRotation;
            }
            if (_spine != null)
            {
                _torsoBasePos = _spine.localPosition;
                _spineBaseRot = _spine.localRotation;
                _torsoBaseRot = _spineBaseRot;
            }
            else if (_torso != null)
            {
                _torsoBasePos = _torso.localPosition;
                _torsoBaseRot = _torso.localRotation;
                _spineBaseRot = _torsoBaseRot;
            }

            if (_head != null) _headBaseRot = _head.localRotation;
            if (_hipLeft != null) _hipLBaseRot = _hipLeft.localRotation;
            if (_hipRight != null) _hipRBaseRot = _hipRight.localRotation;
            if (_kneeLeft != null) _kneeLBaseRot = _kneeLeft.localRotation;
            if (_kneeRight != null) _kneeRBaseRot = _kneeRight.localRotation;
            if (_ankleLeft != null) _ankleLBaseRot = _ankleLeft.localRotation;
            if (_ankleRight != null) _ankleRBaseRot = _ankleRight.localRotation;

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
            if (_elbowLeft != null) _elbowLBaseRot = _elbowLeft.localRotation;
            if (_elbowRight != null) _elbowRBaseRot = _elbowRight.localRotation;
            if (_handLeft != null) _handLBaseRot = _handLeft.localRotation;
            if (_handRight != null) _handRBaseRot = _handRight.localRotation;

            if (!_originalRotCached)
            {
                _rawSpineRot = _spineBaseRot;
                _rawShoulderLRot = _shoulderLBaseRot;
                _rawShoulderRRot = _shoulderRBaseRot;
                _rawElbowLRot = _elbowLBaseRot;
                _rawElbowRRot = _elbowRBaseRot;
                _originalRotCached = true;
            }
        }

        private Transform FindDeepChild(Transform parent, params string[] names)
        {
            if (parent == null || names == null || names.Length == 0) return null;
            foreach (var n in names)
            {
                var found = FindDeepChildSingle(parent, n);
                if (found != null) return found;
                if (!n.StartsWith("mixamorig:"))
                {
                    found = FindDeepChildSingle(parent, "mixamorig:" + n);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private Transform FindDeepChildSingle(Transform parent, string name)
        {
            if (parent == null) return null;
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindDeepChildSingle(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void Update()
        {
            if (_isDead) return;

            float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
            float speed = (transform.position - _lastWorldPos).magnitude / deltaTime;
            _lastWorldPos = transform.position;

            bool isMoving = speed > 0.12f;

            if (isMoving)
            {
                _walkCycleTimer += deltaTime * _walkBobSpeed;
                float sin = Mathf.Sin(_walkCycleTimer);
                float cos = Mathf.Cos(_walkCycleTimer);

                // 1. Pelvis / Hips Bounce & Stride Roll
                if (_hips != null)
                {
                    float yBob = Mathf.Abs(sin) * _pelvisBounceAmount;
                    _hips.localPosition = _hipsBasePos + new Vector3(0, yBob, 0);
                    _hips.localRotation = _hipsBaseRot * Quaternion.Euler(0, sin * 3.5f, cos * 2.2f);
                }

                // 2. Spine / Torso Counter-Torque Balance
                Transform activeTorso = _spine ?? _torso;
                if (activeTorso != null)
                {
                    activeTorso.localRotation = _spineBaseRot * Quaternion.Euler(Mathf.Abs(sin) * 2.0f, -sin * 4.5f, 0);
                }

                // 3. Articulated Leg Locomotion with Knee Flexion
                if (_hipLeft != null)
                {
                    _hipLeft.localRotation = _hipLBaseRot * Quaternion.Euler(sin * _legSwingAngle, 0, 0);
                }
                if (_kneeLeft != null)
                {
                    // Knee bends backward during forward swing to clear ground, snaps straight on plant
                    float kneeFlex = Mathf.Clamp01(sin) * _kneeBendAngle;
                    _kneeLeft.localRotation = _kneeLBaseRot * Quaternion.Euler(kneeFlex, 0, 0);
                }
                if (_ankleLeft != null)
                {
                    _ankleLeft.localRotation = _ankleLBaseRot * Quaternion.Euler(-sin * 12.0f, 0, 0);
                }

                if (_hipRight != null)
                {
                    _hipRight.localRotation = _hipRBaseRot * Quaternion.Euler(-sin * _legSwingAngle, 0, 0);
                }
                if (_kneeRight != null)
                {
                    float kneeFlex = Mathf.Clamp01(-sin) * _kneeBendAngle;
                    _kneeRight.localRotation = _kneeRBaseRot * Quaternion.Euler(kneeFlex, 0, 0);
                }
                if (_ankleRight != null)
                {
                    _ankleRight.localRotation = _ankleRBaseRot * Quaternion.Euler(sin * 12.0f, 0, 0);
                }

                // 4. Opposing Arm Swings with Natural Elbow Flexion (when not attacking)
                if (!_isAttacking)
                {
                    if (_shoulderLeft != null)
                    {
                        _shoulderLeft.localRotation = _shoulderLBaseRot * Quaternion.Euler(-sin * _armSwingAngle, 0, 0);
                    }
                    if (_elbowLeft != null)
                    {
                        float eFlex = 12f + Mathf.Max(0, -sin) * 28.0f;
                        _elbowLeft.localRotation = _elbowLBaseRot * Quaternion.Euler(-eFlex, 0, 0);
                    }

                    if (_shoulderRight != null)
                    {
                        _shoulderRight.localRotation = _shoulderRBaseRot * Quaternion.Euler(sin * _armSwingAngle, 0, 0);
                    }
                    if (_elbowRight != null)
                    {
                        float eFlex = 12f + Mathf.Max(0, sin) * 28.0f;
                        _elbowRight.localRotation = _elbowRBaseRot * Quaternion.Euler(-eFlex, 0, 0);
                    }
                }
            }
            else
            {
                _idleTimer += deltaTime * _idleBreatheSpeed;
                float breath = Mathf.Sin(_idleTimer) * _idleBreatheAmount;

                // Ease legs, knees, ankles back to neutral ready stance
                if (_hipLeft != null) _hipLeft.localRotation = Quaternion.Slerp(_hipLeft.localRotation, _hipLBaseRot, 8f * deltaTime);
                if (_kneeLeft != null) _kneeLeft.localRotation = Quaternion.Slerp(_kneeLeft.localRotation, _kneeLBaseRot, 8f * deltaTime);
                if (_ankleLeft != null) _ankleLeft.localRotation = Quaternion.Slerp(_ankleLeft.localRotation, _ankleLBaseRot, 8f * deltaTime);

                if (_hipRight != null) _hipRight.localRotation = Quaternion.Slerp(_hipRight.localRotation, _hipRBaseRot, 8f * deltaTime);
                if (_kneeRight != null) _kneeRight.localRotation = Quaternion.Slerp(_kneeRight.localRotation, _kneeRBaseRot, 8f * deltaTime);
                if (_ankleRight != null) _ankleRight.localRotation = Quaternion.Slerp(_ankleRight.localRotation, _ankleRBaseRot, 8f * deltaTime);

                // Gentle pelvic breathing rhythm
                if (_hips != null)
                {
                    _hips.localPosition = Vector3.Lerp(_hips.localPosition, _hipsBasePos, 6f * deltaTime);
                    _hips.localRotation = Quaternion.Slerp(_hips.localRotation, _hipsBaseRot, 6f * deltaTime);
                }

                Transform activeTorso = _spine ?? _torso;
                if (activeTorso != null)
                {
                    activeTorso.localPosition = Vector3.Lerp(activeTorso.localPosition, _torsoBasePos + new Vector3(0, breath, 0), 6f * deltaTime);
                    activeTorso.localRotation = Quaternion.Slerp(activeTorso.localRotation, _spineBaseRot, 6f * deltaTime);
                }

                // Return shoulders & elbows to relaxed class stance
                if (!_isAttacking)
                {
                    if (_shoulderLeft != null)
                    {
                        _shoulderLeft.localRotation = Quaternion.Slerp(_shoulderLeft.localRotation, _shoulderLBaseRot, 7f * deltaTime);
                        _shoulderLeft.localPosition = Vector3.Lerp(_shoulderLeft.localPosition, _shoulderLBasePos, 7f * deltaTime);
                    }
                    if (_elbowLeft != null)
                    {
                        _elbowLeft.localRotation = Quaternion.Slerp(_elbowLeft.localRotation, _elbowLBaseRot, 7f * deltaTime);
                    }

                    if (_shoulderRight != null)
                    {
                        _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRight.localRotation, _shoulderRBaseRot, 7f * deltaTime);
                        _shoulderRight.localPosition = Vector3.Lerp(_shoulderRight.localPosition, _shoulderRBasePos, 7f * deltaTime);
                    }
                    if (_elbowRight != null)
                    {
                        _elbowRight.localRotation = Quaternion.Slerp(_elbowRight.localRotation, _elbowRBaseRot, 7f * deltaTime);
                    }
                }

                // Vigilant perimeter scanning: head subtly pans left and right
                if (_head != null && !_isAttacking)
                {
                    float scan = Mathf.Sin(_idleTimer * 0.65f) * 11.5f;
                    _head.localRotation = Quaternion.Slerp(_head.localRotation, _headBaseRot * Quaternion.Euler(0, scan, 0), 4f * deltaTime);
                }
            }
        }

        #region Authentic Combat Actions

        /// <summary>
        /// Authentic Gorkhali Khukuri Cleave: Coiling windup, explosive downward diagonal cross-slash, steel clash, fluid recovery.
        /// </summary>
        public void PlayKhukuriSlash(Action onHit = null)
        {
            if (_isDead) return;
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

            Transform activeTorso = _spine ?? _torso;

            // Phase 1: High Windup (Spine coils right 20°, elbow bends 80° pulling curved blade high near ear)
            while (elapsed < windupTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / windupTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot, _shoulderRBaseRot * Quaternion.Euler(-65f, 25f, -15f), t);
                }
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = Quaternion.Slerp(_elbowRBaseRot, _elbowRBaseRot * Quaternion.Euler(-80f, 0, 0), t);
                }
                if (activeTorso != null)
                {
                    activeTorso.localRotation = _spineBaseRot * Quaternion.Euler(-6f * t, -18f * t, 0);
                }
                yield return null;
            }

            // Phase 2: Downward Diagonal Cleave with explosive rotational torque!
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
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = Quaternion.Slerp(_elbowRBaseRot * Quaternion.Euler(-80f, 0, 0), _elbowRBaseRot * Quaternion.Euler(-18f, 0, 0), t);
                }
                if (activeTorso != null)
                {
                    activeTorso.localRotation = _spineBaseRot * Quaternion.Euler(10f * t, Mathf.Lerp(-18f, 24f, t), 0);
                }

                if (!hitTriggered && t >= 0.45f)
                {
                    hitTriggered = true;
                    GorkhaAudio.PlaySwordClash();
                    onHit?.Invoke();
                }
                yield return null;
            }

            if (!hitTriggered) onHit?.Invoke();

            // Phase 3: Smooth Recovery into Martial Guard
            elapsed = 0f;
            Quaternion endR = _shoulderRight != null ? _shoulderRight.localRotation : Quaternion.identity;
            Quaternion endElbowR = _elbowRight != null ? _elbowRight.localRotation : Quaternion.identity;
            Quaternion endTorso = activeTorso != null ? activeTorso.localRotation : Quaternion.identity;

            while (elapsed < recoverTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / recoverTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(endR, _shoulderRBaseRot, t);
                }
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = Quaternion.Slerp(endElbowR, _elbowRBaseRot, t);
                }
                if (activeTorso != null)
                {
                    activeTorso.localRotation = Quaternion.Slerp(endTorso, _spineBaseRot, t);
                }
                yield return null;
            }

            _isAttacking = false;
            _actionRoutine = null;
        }

        /// <summary>
        /// Sangram Spearman Phalanx Thrust: Low crouch lunge, deep forward 0.5m spear reach, shield impact, retract.
        /// </summary>
        public void PlaySpearLunge(Action onHit = null)
        {
            if (_isDead) return;
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

            // Phase 1: Draw back spear shaft & crouch
            while (elapsed < prepTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / prepTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localPosition = Vector3.Lerp(_shoulderRBasePos, _shoulderRBasePos + new Vector3(0, 0, -0.22f), t);
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot, _shoulderRBaseRot * Quaternion.Euler(-18f, 0, 0), t);
                }
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = Quaternion.Slerp(_elbowRBaseRot, _elbowRBaseRot * Quaternion.Euler(-65f, 0, 0), t);
                }
                yield return null;
            }

            // Phase 2: Powerful Forward Piston Drive (0.50m reach)
            elapsed = 0f;
            bool hitTriggered = false;
            while (elapsed < thrustTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / thrustTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localPosition = Vector3.Lerp(_shoulderRBasePos + new Vector3(0, 0, -0.22f), _shoulderRBasePos + new Vector3(0, 0, 0.48f), t);
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot * Quaternion.Euler(-18f, 0, 0), _shoulderRBaseRot * Quaternion.Euler(16f, 0, 0), t);
                }
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = Quaternion.Slerp(_elbowRBaseRot * Quaternion.Euler(-65f, 0, 0), _elbowRBaseRot * Quaternion.Euler(-10f, 0, 0), t);
                }
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localPosition = Vector3.Lerp(_shoulderLBasePos, _shoulderLBasePos + new Vector3(0, 0, 0.22f), t);
                }

                if (!hitTriggered && t >= 0.45f)
                {
                    hitTriggered = true;
                    GorkhaAudio.PlayShieldBlock();
                    onHit?.Invoke();
                }
                yield return null;
            }

            if (!hitTriggered) onHit?.Invoke();

            // Phase 3: Retract back to Phalanx Guard
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
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = Quaternion.Slerp(_elbowRight.localRotation, _elbowRBaseRot, t);
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

        /// <summary>
        /// Recurve Bowshot: Raise composite bow, draw string to ear, spine twist, snap release with recoil kick.
        /// </summary>
        public void PlayBowRelease(Action onRelease = null)
        {
            if (_isDead) return;
            if (_actionRoutine != null) StopCoroutine(_actionRoutine);
            _actionRoutine = StartCoroutine(BowReleaseRoutine(onRelease));
        }

        private IEnumerator BowReleaseRoutine(Action onRelease)
        {
            _isAttacking = true;
            float elapsed = 0f;
            float drawTime = 0.32f;
            float releaseTime = 0.08f;
            float recoverTime = 0.24f;

            Transform activeTorso = _spine ?? _torso;

            // Phase 1: Raise bow arm & draw string to cheek
            while (elapsed < drawTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / drawTime);
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localRotation = Quaternion.Slerp(_shoulderLBaseRot, _shoulderLBaseRot * Quaternion.Euler(-80f, -15f, 10f), t);
                }
                if (_elbowLeft != null)
                {
                    _elbowLeft.localRotation = Quaternion.Slerp(_elbowLBaseRot, _elbowLBaseRot * Quaternion.Euler(-15f, 0, 0), t);
                }
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot, _shoulderRBaseRot * Quaternion.Euler(-68f, 32f, -12f), t);
                }
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = Quaternion.Slerp(_elbowRBaseRot, _elbowRBaseRot * Quaternion.Euler(-95f, 0, 0), t);
                }
                if (activeTorso != null)
                {
                    activeTorso.localRotation = _spineBaseRot * Quaternion.Euler(0, -22f * t, 0);
                }
                yield return null;
            }

            // Phase 2: Snap String Release & Recoil Kick!
            elapsed = 0f;
            GorkhaAudio.PlayBowTwang();
            onRelease?.Invoke();

            while (elapsed < releaseTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / releaseTime);
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localRotation = _shoulderLBaseRot * Quaternion.Euler(-86f, -15f, 10f);
                }
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot * Quaternion.Euler(-68f, 32f, -12f), _shoulderRBaseRot * Quaternion.Euler(-45f, 15f, 0), t);
                }
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = Quaternion.Slerp(_elbowRBaseRot * Quaternion.Euler(-95f, 0, 0), _elbowRBaseRot * Quaternion.Euler(-30f, 0, 0), t);
                }
                yield return null;
            }

            // Phase 3: Nock next arrow & lower bow
            elapsed = 0f;
            Quaternion endL = _shoulderLeft != null ? _shoulderLeft.localRotation : Quaternion.identity;
            Quaternion endR = _shoulderRight != null ? _shoulderRight.localRotation : Quaternion.identity;
            Quaternion endTorso = activeTorso != null ? activeTorso.localRotation : Quaternion.identity;

            while (elapsed < recoverTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / recoverTime);
                if (_shoulderLeft != null) _shoulderLeft.localRotation = Quaternion.Slerp(endL, _shoulderLBaseRot, t);
                if (_elbowLeft != null) _elbowLeft.localRotation = Quaternion.Slerp(_elbowLeft.localRotation, _elbowLBaseRot, t);
                if (_shoulderRight != null) _shoulderRight.localRotation = Quaternion.Slerp(endR, _shoulderRBaseRot, t);
                if (_elbowRight != null) _elbowRight.localRotation = Quaternion.Slerp(_elbowRight.localRotation, _elbowRBaseRot, t);
                if (activeTorso != null) activeTorso.localRotation = Quaternion.Slerp(endTorso, _spineBaseRot, t);
                yield return null;
            }

            _isAttacking = false;
            _actionRoutine = null;
        }

        #endregion

        #region Authentic Civilian Work Actions

        /// <summary>
        /// Two-handed overhead chopping animation for Woodcutter Axe and Mining Pickaxe.
        /// </summary>
        public void PlayHarvestChop(Action onChop = null)
        {
            if (_isDead) return;
            if (_actionRoutine != null) StopCoroutine(_actionRoutine);
            _actionRoutine = StartCoroutine(HarvestChopRoutine(onChop));
        }

        private IEnumerator HarvestChopRoutine(Action onChop)
        {
            _isAttacking = true;
            float elapsed = 0f;
            float raiseTime = 0.22f;
            float strikeTime = 0.12f;
            float recoverTime = 0.26f;

            Transform activeTorso = _spine ?? _torso;

            // Phase 1: Raise axe/pickaxe overhead with both arms
            while (elapsed < raiseTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / raiseTime);
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot, _shoulderRBaseRot * Quaternion.Euler(-92f, 0, 15f), t);
                }
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = Quaternion.Slerp(_elbowRBaseRot, _elbowRBaseRot * Quaternion.Euler(-75f, 0, 0), t);
                }
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localRotation = Quaternion.Slerp(_shoulderLBaseRot, _shoulderLBaseRot * Quaternion.Euler(-75f, 25f, 0), t);
                }
                if (_elbowLeft != null)
                {
                    _elbowLeft.localRotation = Quaternion.Slerp(_elbowLBaseRot, _elbowLBaseRot * Quaternion.Euler(-65f, 0, 0), t);
                }
                if (activeTorso != null)
                {
                    activeTorso.localRotation = _spineBaseRot * Quaternion.Euler(-14f * t, 0, 0);
                }
                yield return null;
            }

            // Phase 2: Power gravity strike down!
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
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = Quaternion.Slerp(_elbowRBaseRot * Quaternion.Euler(-75f, 0, 0), _elbowRBaseRot * Quaternion.Euler(-10f, 0, 0), t);
                }
                if (_shoulderLeft != null)
                {
                    _shoulderLeft.localRotation = Quaternion.Slerp(_shoulderLBaseRot * Quaternion.Euler(-75f, 25f, 0), _shoulderLBaseRot * Quaternion.Euler(45f, 0, 0), t);
                }
                if (_elbowLeft != null)
                {
                    _elbowLeft.localRotation = Quaternion.Slerp(_elbowLBaseRot * Quaternion.Euler(-65f, 0, 0), _elbowLBaseRot * Quaternion.Euler(-12f, 0, 0), t);
                }
                if (activeTorso != null)
                {
                    activeTorso.localRotation = _spineBaseRot * Quaternion.Euler(Mathf.Lerp(-14f, 18f, t), 0, 0);
                }

                if (!hitTriggered && t >= 0.5f)
                {
                    hitTriggered = true;
                    onChop?.Invoke();
                }
                yield return null;
            }

            if (!hitTriggered) onChop?.Invoke();

            // Phase 3: Tool Rebound & return
            elapsed = 0f;
            Quaternion endR = _shoulderRight != null ? _shoulderRight.localRotation : Quaternion.identity;
            Quaternion endL = _shoulderLeft != null ? _shoulderLeft.localRotation : Quaternion.identity;
            Quaternion endTorso = activeTorso != null ? activeTorso.localRotation : Quaternion.identity;

            while (elapsed < recoverTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / recoverTime);
                if (_shoulderRight != null) _shoulderRight.localRotation = Quaternion.Slerp(endR, _shoulderRBaseRot, t);
                if (_elbowRight != null) _elbowRight.localRotation = Quaternion.Slerp(_elbowRight.localRotation, _elbowRBaseRot, t);
                if (_shoulderLeft != null) _shoulderLeft.localRotation = Quaternion.Slerp(endL, _shoulderLBaseRot, t);
                if (_elbowLeft != null) _elbowLeft.localRotation = Quaternion.Slerp(_elbowLeft.localRotation, _elbowLBaseRot, t);
                if (activeTorso != null) activeTorso.localRotation = Quaternion.Slerp(endTorso, _spineBaseRot, t);
                yield return null;
            }

            _isAttacking = false;
            _actionRoutine = null;
        }

        /// <summary>
        /// Dedicated farming cultivation animation: Villager leans forward, sweeping sickle through terraced crops.
        /// </summary>
        public void PlayFarmCultivate(Action onSweep = null)
        {
            if (_isDead) return;
            if (_actionRoutine != null) StopCoroutine(_actionRoutine);
            _actionRoutine = StartCoroutine(FarmCultivateRoutine(onSweep));
        }

        private IEnumerator FarmCultivateRoutine(Action onSweep)
        {
            _isAttacking = true;
            float elapsed = 0f;
            float sweepTime = 0.35f;
            float resetTime = 0.25f;

            Transform activeTorso = _spine ?? _torso;

            // Phase 1: Forward lean & smooth horizontal sickle sweep across crops
            bool sweepTriggered = false;
            while (elapsed < sweepTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / sweepTime);

                if (activeTorso != null)
                {
                    activeTorso.localRotation = _spineBaseRot * Quaternion.Euler(22f, Mathf.Lerp(-18f, 20f, t), 0);
                }
                if (_shoulderRight != null)
                {
                    _shoulderRight.localRotation = Quaternion.Slerp(
                        _shoulderRBaseRot * Quaternion.Euler(25f, -30f, 0),
                        _shoulderRBaseRot * Quaternion.Euler(35f, 40f, 0),
                        t
                    );
                }
                if (_elbowRight != null)
                {
                    _elbowRight.localRotation = _elbowRBaseRot * Quaternion.Euler(-45f, 0, 0);
                }

                if (!sweepTriggered && t >= 0.5f)
                {
                    sweepTriggered = true;
                    onSweep?.Invoke();
                }
                yield return null;
            }

            if (!sweepTriggered) onSweep?.Invoke();

            // Phase 2: Reset to ready lean
            elapsed = 0f;
            Quaternion endTorso = activeTorso != null ? activeTorso.localRotation : Quaternion.identity;
            Quaternion endShoulderR = _shoulderRight != null ? _shoulderRight.localRotation : Quaternion.identity;

            while (elapsed < resetTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / resetTime);
                if (activeTorso != null) activeTorso.localRotation = Quaternion.Slerp(endTorso, _spineBaseRot, t);
                if (_shoulderRight != null) _shoulderRight.localRotation = Quaternion.Slerp(endShoulderR, _shoulderRBaseRot, t);
                if (_elbowRight != null) _elbowRight.localRotation = Quaternion.Slerp(_elbowRight.localRotation, _elbowRBaseRot, t);
                yield return null;
            }

            _isAttacking = false;
            _actionRoutine = null;
        }

        #endregion

        #region Hit Flinch & Death Collapse

        public void PlayHitFlinch()
        {
            if (_isDead || !gameObject.activeInHierarchy) return;
            StartCoroutine(HitFlinchRoutine());
        }

        private IEnumerator HitFlinchRoutine()
        {
            float elapsed = 0f;
            float flinchDuration = 0.22f;
            Transform activeTorso = _spine ?? _torso;

            while (elapsed < flinchDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / flinchDuration) * Mathf.PI);

                if (activeTorso != null)
                {
                    activeTorso.localRotation = _spineBaseRot * Quaternion.Euler(-18f * t, 0, 0);
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

        /// <summary>
        /// Realistic physical death collapse: knees buckle, pelvis drops, torso twists, weapon drops onto earth.
        /// </summary>
        public void PlayDeathCollapse(Action onDissolve = null)
        {
            if (_isDead) return;
            _isDead = true;
            if (_actionRoutine != null) StopCoroutine(_actionRoutine);
            StartCoroutine(DeathCollapseRoutine(onDissolve));
        }

        private IEnumerator DeathCollapseRoutine(Action onDissolve)
        {
            float elapsed = 0f;
            float collapseTime = 0.65f;
            Transform activeTorso = _spine ?? _torso;

            Vector3 startHipsPos = _hips != null ? _hips.localPosition : Vector3.zero;
            Vector3 targetHipsPos = startHipsPos + new Vector3(0, -0.45f, -0.15f);

            while (elapsed < collapseTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / collapseTime);
                float easeT = Mathf.SmoothStep(0, 1, t);

                // Buckle knees forward
                if (_kneeLeft != null) _kneeLeft.localRotation = Quaternion.Slerp(_kneeLBaseRot, _kneeLBaseRot * Quaternion.Euler(55f, 0, 0), easeT);
                if (_kneeRight != null) _kneeRight.localRotation = Quaternion.Slerp(_kneeRBaseRot, _kneeRBaseRot * Quaternion.Euler(50f, 0, 0), easeT);

                // Drop pelvis to ground
                if (_hips != null)
                {
                    _hips.localPosition = Vector3.Lerp(startHipsPos, targetHipsPos, easeT);
                    _hips.localRotation = Quaternion.Slerp(_hipsBaseRot, _hipsBaseRot * Quaternion.Euler(30f, 25f, -15f), easeT);
                }

                // Slump spine & tilt head
                if (activeTorso != null)
                {
                    activeTorso.localRotation = Quaternion.Slerp(_spineBaseRot, _spineBaseRot * Quaternion.Euler(35f, 15f, 20f), easeT);
                }
                if (_head != null)
                {
                    _head.localRotation = Quaternion.Slerp(_headBaseRot, _headBaseRot * Quaternion.Euler(25f, 30f, 0), easeT);
                }

                // Arms fall limp
                if (_shoulderLeft != null) _shoulderLeft.localRotation = Quaternion.Slerp(_shoulderLBaseRot, _shoulderLBaseRot * Quaternion.Euler(45f, 0, -25f), easeT);
                if (_shoulderRight != null) _shoulderRight.localRotation = Quaternion.Slerp(_shoulderRBaseRot, _shoulderRBaseRot * Quaternion.Euler(45f, 0, 25f), easeT);

                yield return null;
            }

            // Linger briefly on ground before dissolve
            yield return new WaitForSeconds(0.60f);
            onDissolve?.Invoke();
        }

        #endregion
    }
}
