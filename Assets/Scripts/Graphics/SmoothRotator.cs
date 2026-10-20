using UnityEngine;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Smoothly rotates an object around a specified local or world axis.
    /// Operates in BOTH Edit Mode and Play Mode via [ExecuteAlways].
    /// Used for spinning windmill blades, waterwheels, and decorative kinetic props.
    /// </summary>
    [ExecuteAlways]
    public class SmoothRotator : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private Vector3 _rotationAxis = Vector3.forward; // Z axis by default for windmill blades
        [SerializeField] private float _speedDegreesPerSecond = 32.0f;
        [SerializeField] private Space _relativeTo = Space.Self;

        public Vector3 RotationAxis
        {
            get => _rotationAxis;
            set => _rotationAxis = value;
        }

        public float Speed
        {
            get => _speedDegreesPerSecond;
            set => _speedDegreesPerSecond = value;
        }

        private void Update()
        {
            float dt = Application.isPlaying ? Time.deltaTime : 0.016f;
            transform.Rotate(_rotationAxis * (_speedDegreesPerSecond * dt), _relativeTo);
        }
    }
}
