using UnityEngine;
using JemparinganXR.Archery;

namespace JemparinganXR.GameFlow
{
    /// <summary>
    /// Glues player input, camera aiming, bow mechanics, and game flow loop together.
    /// Modular architecture enables seamless replacement of input with Meta XR hand tracking.
    /// </summary>
    public class ArcheryGameplayManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private ArcherCameraController cameraController;
        [SerializeField] private BowController bowController;
        [SerializeField] private GameFlowManager gameFlowManager;
        [SerializeField] private Transform aimTransform;

        private IArcheryInput _input;

        public BowController Bow => bowController;
        public GameFlowManager GameFlow => gameFlowManager;

        private void Awake()
        {
            _input = GetComponent<IArcheryInput>();
            if (_input == null)
            {
                _input = GetComponentInChildren<IArcheryInput>();
            }
            if (_input == null)
            {
                _input = FindFirstObjectByType<MouseKeyboardArcheryInput>();
            }

            if (aimTransform == null && cameraController != null)
            {
                aimTransform = cameraController.transform;
            }
        }

        private void Update()
        {
            if (_input == null || gameFlowManager == null) return;

            GameState state = gameFlowManager.CurrentState;

            if (cameraController != null && (state == GameState.ReadyToShoot || state == GameState.ArrowInFlight))
            {
                cameraController.ApplyAimInput(_input.LookDelta);
            }

            switch (state)
            {
                case GameState.ReadyToShoot:
                    HandleShootingInput();
                    break;

                case GameState.RoundSummary:
                    if (_input.IsContinueRequested)
                    {
                        gameFlowManager.AdvanceToNextRound();
                    }
                    break;

                case GameState.GameOver:
                    if (_input.IsContinueRequested || _input.IsRestartRequested)
                    {
                        gameFlowManager.RestartGame();
                    }
                    break;
            }
        }

        private void HandleShootingInput()
        {
            if (bowController == null) return;

            if (_input.IsCancelRequested && bowController.IsDrawing)
            {
                bowController.CancelDraw();
                return;
            }

            if (_input.IsDrawHeld)
            {
                if (!bowController.IsDrawing)
                {
                    bowController.StartDraw();
                }
                else
                {
                    bowController.UpdateDraw(Time.deltaTime);
                }
            }
            else if (_input.IsDrawReleased && bowController.IsDrawing)
            {
                Vector3 aimDir = aimTransform != null ? aimTransform.forward : transform.forward;
                Vector3 wind = gameFlowManager.CurrentRoundConfig != null
                    ? gameFlowManager.CurrentRoundConfig.GetWindVector()
                    : Vector3.zero;

                Arrow firedArrow = bowController.ReleaseDraw(aimDir, wind);
                if (firedArrow != null)
                {
                    firedArrow.OnResolved += HandleArrowResolved;
                    gameFlowManager.OnArrowFired();
                }
            }
        }

        private void HandleArrowResolved(Arrow arrow, string zoneName, int points, bool isHit)
        {
            if (arrow != null)
            {
                arrow.OnResolved -= HandleArrowResolved;
            }

            if (gameFlowManager != null)
            {
                gameFlowManager.OnArrowResolved(zoneName, points, isHit);
            }
        }
    }
}
