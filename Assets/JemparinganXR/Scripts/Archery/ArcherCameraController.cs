using UnityEngine;

namespace JemparinganXR.Archery
{
    /// <summary>
    /// Controls camera rotation for a seated traditional archer (sila position).
    /// Clamps yaw and pitch to natural head movement angles.
    /// </summary>
    public class ArcherCameraController : MonoBehaviour
    {
        [Header("Aiming Limits")]
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 30f;
        [SerializeField] private float minYaw = -50f;
        [SerializeField] private float maxYaw = 50f;

        [Header("Smoothing")]
        [SerializeField] private float smoothSpeed = 25f;

        private float _targetPitch;
        private float _targetYaw;
        private float _currentPitch;
        private float _currentYaw;

        public float CurrentPitch => _currentPitch;
        public float CurrentYaw => _currentYaw;

        private void Start()
        {
            Vector3 euler = transform.localEulerAngles;
            _targetPitch = NormalizeAngle(euler.x);
            _targetYaw = NormalizeAngle(euler.y);
            _currentPitch = _targetPitch;
            _currentYaw = _targetYaw;
        }

        public void ApplyAimInput(Vector2 lookDelta)
        {
            _targetYaw = Mathf.Clamp(_targetYaw + lookDelta.x, minYaw, maxYaw);
            _targetPitch = Mathf.Clamp(_targetPitch - lookDelta.y, minPitch, maxPitch);
        }

        private void Update()
        {
            _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, Time.deltaTime * smoothSpeed);
            _currentYaw = Mathf.Lerp(_currentYaw, _targetYaw, Time.deltaTime * smoothSpeed);

            transform.localRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        public void ResetAim()
        {
            _targetPitch = 0f;
            _targetYaw = 0f;
            _currentPitch = 0f;
            _currentYaw = 0f;
            transform.localRotation = Quaternion.identity;
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
    }
}
