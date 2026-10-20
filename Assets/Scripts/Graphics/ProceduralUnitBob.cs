using UnityEngine;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Simulates authentic walking and breathing bobbing animations for 3D units
    /// without requiring an external skeletal rig or animation clip.
    /// </summary>
    public class ProceduralUnitBob : MonoBehaviour
    {
        [SerializeField] private float _walkBobSpeed = 12.0f;
        [SerializeField] private float _walkBobAmount = 0.10f;
        [SerializeField] private float _walkTiltAmount = 5.0f;
        [SerializeField] private float _idleBreathingSpeed = 2.5f;

        private Vector3 _baseLocalPos;
        private Vector3 _lastWorldPos;
        private float _bobTimer = 0f;

        private void Start()
        {
            _baseLocalPos = transform.localPosition;
            _lastWorldPos = transform.position;
        }

        private void Update()
        {
            float speed = (transform.position - _lastWorldPos).magnitude / Mathf.Max(Time.deltaTime, 0.001f);
            _lastWorldPos = transform.position;

            bool isMoving = speed > 0.1f;

            if (isMoving)
            {
                _bobTimer += Time.deltaTime * _walkBobSpeed;
                float yOffset = Mathf.Abs(Mathf.Sin(_bobTimer)) * _walkBobAmount;
                float tilt = Mathf.Sin(_bobTimer) * _walkTiltAmount;

                transform.localPosition = _baseLocalPos + new Vector3(0, yOffset, 0);
                transform.localRotation = Quaternion.Euler(0, 0, tilt);
            }
            else
            {
                _bobTimer += Time.deltaTime * _idleBreathingSpeed;
                float breathOffset = Mathf.Sin(_bobTimer) * 0.02f;
                transform.localPosition = _baseLocalPos + new Vector3(0, breathOffset, 0);
                transform.localRotation = Quaternion.identity;
            }
        }
    }
}
