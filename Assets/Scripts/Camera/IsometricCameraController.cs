using UnityEngine;
using Gorkha.Core;

namespace Gorkha.CameraSystem
{
    /// <summary>
    /// Smooth 3D Isometric RTS Camera Controller (Clash of Clans / Age of Empires style).
    /// Features:
    /// - Pure 45-degree isometric viewing angle (Pitch 45°, Yaw 45°)
    /// - Frames ground settlement focal point without transform fighting
    /// - Always guarantees focal point at settlement center (24, 0, 24)
    /// - Smooth keyboard WASD / Arrow panning along isometric diagonals
    /// - 1:1 Ground raycast mouse dragging (middle or right mouse)
    /// - Smooth scroll-wheel zoom (10m close-up to 42m kingdom overview)
    /// - Map boundary clamping
    /// - One-click re-centering on kingdom center
    /// </summary>
    [ExecuteAlways]
    public class IsometricCameraController : MonoBehaviour
    {
        public static IsometricCameraController Instance { get; private set; }

        [Header("Focal Point & Bounds")]
        [SerializeField] private Vector3 _focalPoint = new Vector3(24f, 0f, 24f);
        [SerializeField] private Vector2 _minBounds = new Vector2(4f, 4f);
        [SerializeField] private Vector2 _maxBounds = new Vector2(44f, 44f);

        [Header("Movement & Drag")]
        [SerializeField] private float _panSpeed = 28f;
        [SerializeField] private float _smoothTime = 0.10f;

        [Header("Zoom")]
        [SerializeField] private float _minZoom = 10f;
        [SerializeField] private float _maxZoom = 42f;
        [SerializeField] private float _initialZoom = 24f;
        [SerializeField] private float _zoomSensitivity = 12f;

        private Vector3 _targetFocalPoint;
        private Vector3 _currentVelocity;
        private float _currentZoom = 24f;
        private float _targetZoom = 24f;
        private Vector3 _dragStartGroundPos;
        private bool _isDragging = false;
        private Camera _cam;

        public Vector3 FocalPoint => _targetFocalPoint;
        public float Zoom => _currentZoom;

        private void Awake()
        {
            Instance = this;
            SetupCamera();
        }

        private void OnEnable()
        {
            Instance = this;
            SetupCamera();
        }

        private void SetupCamera()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = GetComponentInChildren<Camera>();
            if (_cam == null) _cam = Camera.main;

            // Ensure AudioListener is present on the camera or in the scene
            if (_cam != null)
            {
                if (_cam.GetComponent<AudioListener>() == null && FindAnyObjectByType<AudioListener>() == null)
                {
                    _cam.gameObject.AddComponent<AudioListener>();
                }
            }

            // Ensure AudioListener global volume and pause states are active
            AudioListener.pause = false;
            AudioListener.volume = 1.0f;

            // Ensure valid focal point at settlement center
            if (_focalPoint.sqrMagnitude < 1f || _focalPoint == Vector3.zero)
            {
                _focalPoint = new Vector3(24f, 0f, 24f);
            }

            _targetFocalPoint = _focalPoint;
            _currentZoom = _initialZoom > 0f ? _initialZoom : 24f;
            _targetZoom = _currentZoom;

            // Immediately position and orient to settlement center (24, 0, 24)
            transform.rotation = Quaternion.Euler(45f, 45f, 0f);
            Vector3 isoDir = Quaternion.Euler(45f, 45f, 0f) * Vector3.forward;
            transform.position = _focalPoint - (isoDir * _currentZoom);
        }

        [Header("Edge Panning")]
        [SerializeField] private bool _enableEdgePanning = true;
        [SerializeField] private float _edgeThresholdPixels = 18f;

        private void Update()
        {
            if (Application.isPlaying)
            {
                HandleKeyboardInput();
                HandleEdgePanning();
                HandleMouseDrag();
                HandleZoom();
                UpdateCameraTransform();
            }
            else
            {
                // In Edit mode, maintain focal framing
                if (_focalPoint.sqrMagnitude < 1f) _focalPoint = new Vector3(24f, 0f, 24f);
                transform.rotation = Quaternion.Euler(45f, 45f, 0f);
                Vector3 isoDir = Quaternion.Euler(45f, 45f, 0f) * Vector3.forward;
                transform.position = _focalPoint - (isoDir * 24f);
            }
        }

        private void HandleKeyboardInput()
        {
            Vector2 move = GorkhaInput.GetMoveAxis();
            if (move.sqrMagnitude > 0.001f)
            {
                Vector3 forwardFlat = new Vector3(1f, 0f, 1f).normalized;
                Vector3 rightFlat = new Vector3(1f, 0f, -1f).normalized;

                Vector3 delta = (forwardFlat * move.y + rightFlat * move.x) * (_panSpeed * Time.deltaTime);
                _targetFocalPoint += delta;
                ClampFocalPoint();
            }
        }

        private void HandleEdgePanning()
        {
            if (!_enableEdgePanning || _isDragging) return;

            Vector2 mousePos = GorkhaInput.MousePosition;
            if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height) return;

            Vector2 edgeMove = Vector2.zero;
            if (mousePos.x <= _edgeThresholdPixels) edgeMove.x -= 1f;
            else if (mousePos.x >= Screen.width - _edgeThresholdPixels) edgeMove.x += 1f;

            if (mousePos.y <= _edgeThresholdPixels) edgeMove.y -= 1f;
            else if (mousePos.y >= Screen.height - _edgeThresholdPixels) edgeMove.y += 1f;

            if (edgeMove.sqrMagnitude > 0.001f)
            {
                Vector3 forwardFlat = new Vector3(1f, 0f, 1f).normalized;
                Vector3 rightFlat = new Vector3(1f, 0f, -1f).normalized;

                Vector3 delta = (forwardFlat * edgeMove.y + rightFlat * edgeMove.x).normalized * (_panSpeed * 0.85f * Time.deltaTime);
                _targetFocalPoint += delta;
                ClampFocalPoint();
            }
        }

        public void FocusOn(Vector3 worldTarget, float? zoom = null)
        {
            _targetFocalPoint = new Vector3(worldTarget.x, 0, worldTarget.z);
            ClampFocalPoint();
            if (zoom.HasValue)
            {
                _targetZoom = Mathf.Clamp(zoom.Value, _minZoom, _maxZoom);
            }
        }

        private void HandleMouseDrag()
        {
            if (GorkhaInput.GetMouseButtonDown(1) || GorkhaInput.GetMouseButtonDown(2))
            {
                if (RaycastGroundPlane(out Vector3 hit))
                {
                    _dragStartGroundPos = hit;
                    _isDragging = true;
                }
            }

            if (_isDragging && (GorkhaInput.GetMouseButton(1) || GorkhaInput.GetMouseButton(2)))
            {
                if (RaycastGroundPlane(out Vector3 hit))
                {
                    Vector3 diff = _dragStartGroundPos - hit;
                    _targetFocalPoint += diff;
                    ClampFocalPoint();
                    if (RaycastGroundPlane(out Vector3 newHit))
                    {
                        _dragStartGroundPos = newHit;
                    }
                }
            }

            if (GorkhaInput.GetMouseButtonUp(1) || GorkhaInput.GetMouseButtonUp(2))
            {
                _isDragging = false;
            }
        }

        private void HandleZoom()
        {
            float scroll = GorkhaInput.GetScrollWheel();
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                _targetZoom -= scroll * _zoomSensitivity * 4f;
                _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
            }

            _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.deltaTime * 12f);
        }

        private void UpdateCameraTransform()
        {
            _focalPoint = Vector3.SmoothDamp(_focalPoint, _targetFocalPoint, ref _currentVelocity, _smoothTime);

            Vector3 isoDir = Quaternion.Euler(45f, 45f, 0f) * Vector3.forward;
            transform.position = _focalPoint - (isoDir * _currentZoom);
            transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        }

        private bool RaycastGroundPlane(out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;
            Camera c = _cam != null ? _cam : Camera.main;
            if (c == null) return false;

            Ray ray = c.ScreenPointToRay(GorkhaInput.MousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                hitPoint = ray.GetPoint(enter);
                return true;
            }
            return false;
        }

        private void ClampFocalPoint()
        {
            _targetFocalPoint.x = Mathf.Clamp(_targetFocalPoint.x, _minBounds.x, _maxBounds.x);
            _targetFocalPoint.z = Mathf.Clamp(_targetFocalPoint.z, _minBounds.y, _maxBounds.y);
        }

        public void FocusOn(Vector3 worldPos)
        {
            _targetFocalPoint = new Vector3(worldPos.x, 0f, worldPos.z);
            ClampFocalPoint();
        }

        public void ResetView()
        {
            _focalPoint = new Vector3(24f, 0f, 24f);
            _targetFocalPoint = _focalPoint;
            _targetZoom = 24f;
            _currentZoom = 24f;
            Vector3 isoDir = Quaternion.Euler(45f, 45f, 0f) * Vector3.forward;
            transform.position = _focalPoint - (isoDir * _currentZoom);
            transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        }
    }
}
