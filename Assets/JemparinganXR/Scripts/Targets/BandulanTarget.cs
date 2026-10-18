using UnityEngine;
using JemparinganXR.Archery;

namespace JemparinganXR.Targets
{
    /// <summary>
    /// Controls the traditional hanging cylindrical Bandulan target.
    /// Handles pendulum swing on arrow impact, optional horizontal movement,
    /// distance positioning, and cleaning up embedded arrows.
    /// </summary>
    public class BandulanTarget : MonoBehaviour
    {
        [Header("Target Hierarchy")]
        [SerializeField] private Transform standRoot;
        [SerializeField] private Transform suspensionPivot;
        [SerializeField] private Transform bandulanCylinder;

        [Header("Pendulum Physics")]
        [SerializeField] private float damping = 1.8f;
        [SerializeField] private float gravityTorque = 12f;
        [SerializeField] private float impulseMultiplier = 8f;

        [Header("Challenge Options")]
        [SerializeField] private float movementSpeed = 0f;
        [SerializeField] private float movementAmplitude = 1.5f;

        private Vector3 _baseStandPosition;
        private Vector2 _swingAngle;
        private Vector2 _swingVelocity;

        public float MovementSpeed
        {
            get => movementSpeed;
            set => movementSpeed = value;
        }

        private void Awake()
        {
            if (standRoot == null) standRoot = transform;
            _baseStandPosition = standRoot.position;
        }

        private void Update()
        {
            UpdateMovement();
            UpdatePendulumSwing();
        }

        private void UpdateMovement()
        {
            if (movementSpeed > 0f)
            {
                float xOffset = Mathf.Sin(Time.time * movementSpeed) * movementAmplitude;
                standRoot.position = _baseStandPosition + Vector3.right * xOffset;
            }
        }

        private void UpdatePendulumSwing()
        {
            if (suspensionPivot == null) return;

            _swingVelocity -= (_swingAngle * gravityTorque + _swingVelocity * damping) * Time.deltaTime;
            _swingAngle += _swingVelocity * Time.deltaTime;

            suspensionPivot.localRotation = Quaternion.Euler(_swingAngle.x, 0f, _swingAngle.y);
        }

        public void ApplyHitImpulse(Vector3 impulse, Vector3 hitPoint)
        {
            float pitchTorque = impulse.z * impulseMultiplier;
            float rollTorque = -impulse.x * impulseMultiplier;

            _swingVelocity.x += pitchTorque;
            _swingVelocity.y += rollTorque;
        }

        public void SetTargetDistance(float distance)
        {
            if (standRoot == null) standRoot = transform;
            _baseStandPosition = new Vector3(_baseStandPosition.x, _baseStandPosition.y, distance);
            standRoot.position = _baseStandPosition;
        }

        public void ClearAttachedArrows()
        {
            Arrow[] embeddedArrows = GetComponentsInChildren<Arrow>();
            for (int i = embeddedArrows.Length - 1; i >= 0; i--)
            {
                if (embeddedArrows[i] != null)
                {
                    Destroy(embeddedArrows[i].gameObject);
                }
            }
        }
    }
}
