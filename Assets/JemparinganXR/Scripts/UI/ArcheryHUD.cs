using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JemparinganXR.GameFlow;
using JemparinganXR.Archery;

namespace JemparinganXR.UI
{
    /// <summary>
    /// HUD displaying current rambahan, arrow count, scores, draw strength,
    /// and round/game summary dialogs.
    /// </summary>
    public class ArcheryHUD : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private GameFlowManager gameFlowManager;
        [SerializeField] private BowController bowController;

        [Header("In-Game HUD Text")]
        [SerializeField] private Text rambahanText;
        [SerializeField] private Text arrowText;
        [SerializeField] private Text roundScoreText;
        [SerializeField] private Text totalScoreText;
        [SerializeField] private Text lastShotText;
        [SerializeField] private Text objectiveText;

        [Header("Draw Gauge & Aim")]
        [SerializeField] private Image drawGaugeFill;
        [SerializeField] private GameObject crosshair;

        [Header("Round Summary Panel")]
        [SerializeField] private GameObject roundSummaryPanel;
        [SerializeField] private Text roundSummaryTitle;
        [SerializeField] private Text roundSummaryScoreText;
        [SerializeField] private Text roundSummaryStatusText;
        [SerializeField] private Button nextRoundButton;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverTotalScoreText;
        [SerializeField] private Text gameOverBreakdownText;
        [SerializeField] private Button playAgainButton;

        private void Start()
        {
            if (gameFlowManager == null)
            {
                gameFlowManager = FindFirstObjectByType<GameFlowManager>();
            }
            if (bowController == null)
            {
                bowController = FindFirstObjectByType<BowController>();
            }

            if (gameFlowManager != null)
            {
                gameFlowManager.OnGameStateChanged += HandleGameStateChanged;
                gameFlowManager.OnScoreUpdated += UpdateHUD;
                gameFlowManager.OnRoundCompleted += HandleRoundCompleted;
                gameFlowManager.OnGameCompleted += HandleGameCompleted;
            }

            if (bowController != null)
            {
                bowController.OnDrawProgress += HandleDrawProgress;
                bowController.OnDrawCancelled += HandleDrawCancelled;
            }

            if (nextRoundButton != null)
            {
                nextRoundButton.onClick.AddListener(OnNextRoundClicked);
            }
            if (playAgainButton != null)
            {
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            }

            HideAllPopups();
            UpdateHUD();
        }

        private void OnDestroy()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.OnGameStateChanged -= HandleGameStateChanged;
                gameFlowManager.OnScoreUpdated -= UpdateHUD;
                gameFlowManager.OnRoundCompleted -= HandleRoundCompleted;
                gameFlowManager.OnGameCompleted -= HandleGameCompleted;
            }

            if (bowController != null)
            {
                bowController.OnDrawProgress -= HandleDrawProgress;
                bowController.OnDrawCancelled -= HandleDrawCancelled;
            }
        }

        private void UpdateHUD()
        {
            if (gameFlowManager == null) return;

            var config = gameFlowManager.CurrentRoundConfig;
            int maxArrows = config != null ? config.arrowsPerRound : 4;
            int displayedArrow = Mathf.Clamp(gameFlowManager.CurrentArrowInRound + 1, 1, maxArrows);

            if (rambahanText != null)
            {
                rambahanText.text = $"Rambahan: {gameFlowManager.CurrentRoundNumber} / {gameFlowManager.TotalRounds}";
            }

            if (arrowText != null)
            {
                arrowText.text = $"Panah: {displayedArrow} / {maxArrows}  (Total: {gameFlowManager.TotalArrowsShot} / 20)";
            }

            if (roundScoreText != null)
            {
                roundScoreText.text = $"Skor Babak: {gameFlowManager.CurrentRoundScore}";
            }

            if (totalScoreText != null)
            {
                totalScoreText.text = $"Total Skor: {gameFlowManager.TotalScore}";
            }

            if (lastShotText != null)
            {
                lastShotText.text = $"Hasil Terakhir: {gameFlowManager.LastShotResult}";
            }

            if (objectiveText != null && config != null)
            {
                objectiveText.text = $"Target: {config.targetScoreObjective} Poin (Jarak: {config.targetDistance:0}m)";
            }
        }

        private void HandleDrawProgress(float strength)
        {
            if (drawGaugeFill != null)
            {
                drawGaugeFill.fillAmount = strength;
                drawGaugeFill.color = Color.Lerp(new Color(0.2f, 0.8f, 0.2f), new Color(0.9f, 0.3f, 0.1f), strength);
            }
        }

        private void HandleDrawCancelled()
        {
            if (drawGaugeFill != null)
            {
                drawGaugeFill.fillAmount = 0f;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.ReadyToShoot)
            {
                HideAllPopups();
                if (drawGaugeFill != null) drawGaugeFill.fillAmount = 0f;
                if (crosshair != null) crosshair.SetActive(true);
            }
        }

        private void HandleRoundCompleted(RoundRecord record)
        {
            if (crosshair != null) crosshair.SetActive(false);
            if (roundSummaryPanel != null)
            {
                roundSummaryPanel.SetActive(true);

                if (roundSummaryTitle != null)
                {
                    roundSummaryTitle.text = $"Rambahan {record.roundNumber} Selesai";
                }

                if (roundSummaryScoreText != null)
                {
                    roundSummaryScoreText.text = $"Skor Babak: {record.roundScore}  |  Target Objektif: {record.objectiveScore}";
                }

                if (roundSummaryStatusText != null)
                {
                    if (record.objectiveAchieved)
                    {
                        roundSummaryStatusText.text = "★ OBJEKTIF TERCAPAI! ★";
                        roundSummaryStatusText.color = new Color(0.2f, 0.9f, 0.3f);
                    }
                    else
                    {
                        roundSummaryStatusText.text = "Objektif Belum Tercapai (Latihan Dilanjutkan)";
                        roundSummaryStatusText.color = new Color(0.9f, 0.7f, 0.2f);
                    }
                }
            }
        }

        private void HandleGameCompleted(int totalScore, List<RoundRecord> history)
        {
            if (crosshair != null) crosshair.SetActive(false);
            if (roundSummaryPanel != null) roundSummaryPanel.SetActive(false);

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (gameOverTotalScoreText != null)
                {
                    gameOverTotalScoreText.text = $"Total Skor Akhir: {totalScore} Poin";
                }

                if (gameOverBreakdownText != null && history != null)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("Ringkasan 5 Rambahan:");
                    foreach (var rec in history)
                    {
                        string status = rec.objectiveAchieved ? "[TERCAPAI]" : "[-]";
                        sb.AppendLine($"• Rambahan {rec.roundNumber}: {rec.roundScore} Poin {status}");
                    }
                    gameOverBreakdownText.text = sb.ToString();
                }
            }
        }

        public void OnNextRoundClicked()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.AdvanceToNextRound();
            }
        }

        public void OnPlayAgainClicked()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.RestartGame();
            }
        }

        private void HideAllPopups()
        {
            if (roundSummaryPanel != null) roundSummaryPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }
    }
}
