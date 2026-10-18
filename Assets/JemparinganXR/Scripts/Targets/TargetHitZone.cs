using System.Collections;
using UnityEngine;
using JemparinganXR.Archery;

namespace JemparinganXR.Targets
{
    /// <summary>
    /// Configurable scoring zone on the Bandulan target (e.g. Molo, Awak, Klinthing).
    /// Inspector-customizable for point values and visual feedback.
    /// </summary>
    public class TargetHitZone : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [SerializeField] private string zoneName = "Molo";
        [SerializeField] private int pointValue = 3;
        [SerializeField] private Color hitHighlightColor = Color.yellow;
        [SerializeField] private float highlightDuration = 0.35f;

        [Header("Visual Feedback")]
        [SerializeField] private MeshRenderer zoneRenderer;

        private Material _zoneMaterial;
        private Color _originalColor;
        private Coroutine _flashRoutine;

        public string ZoneName => zoneName;
        public int PointValue => pointValue;

        private void Awake()
        {
            if (zoneRenderer == null)
            {
                zoneRenderer = GetComponent<MeshRenderer>();
            }

            if (zoneRenderer != null)
            {
                _zoneMaterial = zoneRenderer.material;
                _originalColor = _zoneMaterial.color;
            }
        }

        public void OnHit(Arrow arrow, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (_zoneMaterial != null)
            {
                if (_flashRoutine != null)
                {
                    StopCoroutine(_flashRoutine);
                }
                _flashRoutine = StartCoroutine(FlashRoutine());
            }
        }

        private IEnumerator FlashRoutine()
        {
            _zoneMaterial.color = hitHighlightColor;
            float elapsed = 0f;
            while (elapsed < highlightDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / highlightDuration;
                _zoneMaterial.color = Color.Lerp(hitHighlightColor, _originalColor, t);
                yield return null;
            }
            _zoneMaterial.color = _originalColor;
            _flashRoutine = null;
        }

        private void OnDestroy()
        {
            if (_zoneMaterial != null)
            {
                Destroy(_zoneMaterial);
            }
        }
    }
}
