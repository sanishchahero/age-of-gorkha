using UnityEngine;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Animates the Nepali double-triangle royal banner with dynamic Himalayan mountain wind simulation.
    /// Uses real-time sine-wave vertex displacement on a 3D procedural flag mesh.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class FlagClothAnimator : MonoBehaviour
    {
        [SerializeField] private float _windFrequency = 3.5f;
        [SerializeField] private float _windAmplitude = 0.12f;
        [SerializeField] private float _windFlutterSpeed = 5.0f;

        private Mesh _mesh;
        private Vector3[] _originalVertices;
        private Vector3[] _displacedVertices;

        private void Start()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter != null && filter.mesh != null)
            {
                _mesh = Instantiate(filter.mesh);
                filter.mesh = _mesh;
                _originalVertices = _mesh.vertices;
                _displacedVertices = new Vector3[_originalVertices.Length];
            }
        }

        private void Update()
        {
            if (_mesh == null || _originalVertices == null) return;

            float time = Time.time * _windFlutterSpeed;

            for (int i = 0; i < _originalVertices.Length; i++)
            {
                Vector3 v = _originalVertices[i];
                // Points further from flag pole (larger X) experience greater wind deflection
                float factor = Mathf.Clamp01(v.x);
                float wave = Mathf.Sin(time + v.x * _windFrequency + v.y * 2.0f) * _windAmplitude * factor;
                float secondaryWave = Mathf.Cos(time * 1.5f + v.x * 4.0f) * (_windAmplitude * 0.4f) * factor;

                _displacedVertices[i] = new Vector3(v.x, v.y, v.z + wave + secondaryWave);
            }

            _mesh.vertices = _displacedVertices;
            _mesh.RecalculateNormals();
        }
    }
}
