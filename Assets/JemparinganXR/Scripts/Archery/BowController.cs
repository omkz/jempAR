using System;
using UnityEngine;

namespace JemparinganXR.Archery
{
    /// <summary>
    /// Controls traditional Javanese Gandhewa bow drawing, string deformation,
    /// and arrow launching.
    /// </summary>
    public class BowController : MonoBehaviour
    {
        [Header("Launch Speeds")]
        [SerializeField] private float minLaunchSpeed = 16f;
        [SerializeField] private float maxLaunchSpeed = 38f;
        [SerializeField] private float drawDuration = 1.2f;

        [Header("References")]
        [SerializeField] private Arrow arrowPrefab;
        [SerializeField] private Transform arrowSpawnPoint;
        [SerializeField] private Transform upperStringTip;
        [SerializeField] private Transform lowerStringTip;
        [SerializeField] private Transform stringNockPoint;
        [SerializeField] private LineRenderer bowstringRenderer;
        [SerializeField] private GameObject loadedArrowVisual;

        [Header("Visual Deformation")]
        [SerializeField] private float maxDrawDistance = 0.40f;
        [SerializeField] private float recoilDuration = 0.12f;

        private bool _isDrawing;
        private float _drawTimer;
        private float _currentDrawStrength;
        private Vector3 _nockRestLocalPosition;
        private float _recoilTimer;

        public event Action<float> OnDrawProgress;
        public event Action<Arrow, float> OnArrowFired;
        public event Action OnDrawCancelled;

        public bool IsDrawing => _isDrawing;
        public float CurrentDrawStrength => _currentDrawStrength;
        public float MinLaunchSpeed => minLaunchSpeed;
        public float MaxLaunchSpeed => maxLaunchSpeed;

        private void Awake()
        {
            if (stringNockPoint != null)
            {
                _nockRestLocalPosition = stringNockPoint.localPosition;
            }
            UpdateBowstringVisual();
            SetLoadedArrowVisible(true);
        }

        public void StartDraw()
        {
            if (_isDrawing) return;
            _isDrawing = true;
            _drawTimer = 0f;
            _currentDrawStrength = 0f;
            SetLoadedArrowVisible(true);
            OnDrawProgress?.Invoke(0f);
        }

        public void UpdateDraw(float deltaTime)
        {
            if (!_isDrawing) return;

            _drawTimer += deltaTime;
            _currentDrawStrength = Mathf.Clamp01(_drawTimer / drawDuration);

            if (stringNockPoint != null)
            {
                Vector3 drawnPos = _nockRestLocalPosition - Vector3.forward * (_currentDrawStrength * maxDrawDistance);
                stringNockPoint.localPosition = drawnPos;
            }

            UpdateBowstringVisual();
            OnDrawProgress?.Invoke(_currentDrawStrength);
        }

        public Arrow ReleaseDraw(Vector3 aimDirection, Vector3 windVector)
        {
            if (!_isDrawing) return null;

            float launchSpeed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, _currentDrawStrength);
            float finalStrength = _currentDrawStrength;

            _isDrawing = false;
            _drawTimer = 0f;
            _currentDrawStrength = 0f;

            Arrow spawnedArrow = null;
            if (arrowPrefab != null)
            {
                Vector3 spawnPos = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;
                Quaternion spawnRot = Quaternion.LookRotation(aimDirection);
                spawnedArrow = Instantiate(arrowPrefab, spawnPos, spawnRot);
                spawnedArrow.Launch(aimDirection.normalized * launchSpeed, windVector);
            }

            SetLoadedArrowVisible(false);
            TriggerRecoil();
            OnArrowFired?.Invoke(spawnedArrow, finalStrength);
            return spawnedArrow;
        }

        public void CancelDraw()
        {
            if (!_isDrawing) return;
            _isDrawing = false;
            _drawTimer = 0f;
            _currentDrawStrength = 0f;

            if (stringNockPoint != null)
            {
                stringNockPoint.localPosition = _nockRestLocalPosition;
            }

            UpdateBowstringVisual();
            SetLoadedArrowVisible(true);
            OnDrawCancelled?.Invoke();
        }

        public void SetLoadedArrowVisible(bool visible)
        {
            if (loadedArrowVisual != null)
            {
                loadedArrowVisual.SetActive(visible);
            }
        }

        private void TriggerRecoil()
        {
            _recoilTimer = recoilDuration;
        }

        private void Update()
        {
            if (!_isDrawing && _recoilTimer > 0f)
            {
                _recoilTimer -= Time.deltaTime;
                if (stringNockPoint != null)
                {
                    float t = Mathf.Clamp01(_recoilTimer / recoilDuration);
                    stringNockPoint.localPosition = Vector3.Lerp(_nockRestLocalPosition, stringNockPoint.localPosition, t);
                }
                UpdateBowstringVisual();
            }
            else if (!_isDrawing && stringNockPoint != null && stringNockPoint.localPosition != _nockRestLocalPosition)
            {
                stringNockPoint.localPosition = _nockRestLocalPosition;
                UpdateBowstringVisual();
            }
        }

        public void UpdateBowstringVisual()
        {
            if (bowstringRenderer == null || upperStringTip == null || lowerStringTip == null || stringNockPoint == null)
            {
                return;
            }

            bowstringRenderer.positionCount = 3;
            bowstringRenderer.SetPosition(0, upperStringTip.position);
            bowstringRenderer.SetPosition(1, stringNockPoint.position);
            bowstringRenderer.SetPosition(2, lowerStringTip.position);
        }
    }
}
