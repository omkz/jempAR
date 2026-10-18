using System;
using System.Collections;
using UnityEngine;
using JemparinganXR.Targets;

namespace JemparinganXR.Archery
{
    /// <summary>
    /// Rigidbody-based arrow projectile with aerodynamic flight and target penetration.
    /// Compatible with Unity 6000.2.14f1 physics API.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Arrow : MonoBehaviour
    {
        [Header("Flight Settings")]
        [SerializeField] private float maxLifetime = 8f;
        [SerializeField] private float missDespawnDelay = 4f;
        [SerializeField] private float minSpeedToOrient = 0.5f;

        [Header("Visuals")]
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private GameObject hitEffectPrefab;

        private Rigidbody _rb;
        private Collider _collider;
        private bool _isFlying;
        private bool _isEmbedded;
        private bool _hasResolved;
        private Vector3 _windVector;

        public event Action<Arrow, string, int, bool> OnResolved;

        public bool IsFlying => _isFlying;
        public bool IsEmbedded => _isEmbedded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void Launch(Vector3 velocity, Vector3 wind)
        {
            _windVector = wind;
            _isFlying = true;
            _isEmbedded = false;
            _hasResolved = false;

            _rb.isKinematic = false;
            _rb.useGravity = true;

#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = velocity;
#else
            _rb.velocity = velocity;
#endif

            if (velocity.sqrMagnitude > 0.01f)
            {
                transform.forward = velocity.normalized;
            }

            StartCoroutine(LifetimeRoutine());
        }

        private void FixedUpdate()
        {
            if (!_isFlying || _isEmbedded) return;

#if UNITY_6000_0_OR_NEWER
            Vector3 vel = _rb.linearVelocity;
#else
            Vector3 vel = _rb.velocity;
#endif

            if (_windVector.sqrMagnitude > 0.001f)
            {
                _rb.AddForce(_windVector, ForceMode.Acceleration);
            }

            if (vel.sqrMagnitude > minSpeedToOrient * minSpeedToOrient)
            {
                transform.forward = vel.normalized;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isEmbedded) return;

            TargetHitZone hitZone = collision.gameObject.GetComponent<TargetHitZone>();
            if (hitZone == null)
            {
                hitZone = collision.gameObject.GetComponentInParent<TargetHitZone>();
            }

            BandulanTarget target = collision.gameObject.GetComponentInParent<BandulanTarget>();

            Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            Vector3 hitNormal = collision.contacts.Length > 0 ? collision.contacts[0].normal : -transform.forward;

            if (hitZone != null || target != null)
            {
                EmbedInTarget(collision.transform, hitPoint);

                string zoneName = "Target";
                int points = 1;

                if (hitZone != null)
                {
                    hitZone.OnHit(this, hitPoint, hitNormal);
                    zoneName = hitZone.ZoneName;
                    points = hitZone.PointValue;
                }

                if (target != null)
                {
                    target.ApplyHitImpulse(transform.forward * 1.5f, hitPoint);
                }

                SpawnHitEffect(hitPoint, hitNormal);
                ResolveShot(zoneName, points, true);
            }
            else
            {
                EmbedInGround();
                ResolveShot("Luput", 0, false);
                StartCoroutine(DespawnRoutine(missDespawnDelay));
            }
        }

        private void EmbedInTarget(Transform parent, Vector3 hitPoint)
        {
            _isFlying = false;
            _isEmbedded = true;
            _collider.enabled = false;
            _rb.isKinematic = true;

#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
#else
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
#endif

            transform.position = hitPoint + transform.forward * 0.06f;
            transform.SetParent(parent, true);

            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
            }
        }

        private void EmbedInGround()
        {
            _isFlying = false;
            _isEmbedded = true;
            _collider.enabled = false;
            _rb.isKinematic = true;

#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
#else
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
#endif

            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
            }
        }

        private void SpawnHitEffect(Vector3 position, Vector3 normal)
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, position, Quaternion.LookRotation(normal));
            }
            else
            {
                // Lightweight primitive visual flash
                GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flash.name = "HitFlash";
                flash.transform.position = position;
                flash.transform.localScale = Vector3.one * 0.12f;
                Collider c = flash.GetComponent<Collider>();
                if (c != null) Destroy(c);
                Destroy(flash, 0.35f);
            }
        }

        private void ResolveShot(string zoneName, int points, bool isHit)
        {
            if (_hasResolved) return;
            _hasResolved = true;
            OnResolved?.Invoke(this, zoneName, points, isHit);
        }

        private IEnumerator LifetimeRoutine()
        {
            yield return new WaitForSeconds(maxLifetime);
            if (!_hasResolved)
            {
                ResolveShot("Luput (Hilang)", 0, false);
            }
            if (!_isEmbedded)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator DespawnRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
