using System;
using System.Collections.Generic;
using UnityEngine;
using JemparinganXR.Targets;

namespace JemparinganXR.GameFlow
{
    public enum GameState
    {
        ReadyToShoot,
        ArrowInFlight,
        RoundSummary,
        GameOver
    }

    [Serializable]
    public struct RoundRecord
    {
        public int roundNumber;
        public int roundScore;
        public int objectiveScore;
        public bool objectiveAchieved;
    }

    /// <summary>
    /// Manages the 5-rambahan game loop, scoring, rounds progression, and game states.
    /// Does not require minimum score to continue; always proceeds cleanly.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        [Header("Round Configurations")]
        [SerializeField] private RoundConfig[] rounds;

        [Header("Scene References")]
        [SerializeField] private BandulanTarget target;

        private int _currentRoundIndex;
        private int _currentArrowInRound;
        private int _totalArrowsShot;
        private int _currentRoundScore;
        private int _totalScore;
        private string _lastShotResult = "-";
        private int _lastShotScore = 0;
        private GameState _currentState;
        private readonly List<RoundRecord> _roundHistory = new List<RoundRecord>();

        public event Action<GameState> OnGameStateChanged;
        public event Action OnScoreUpdated;
        public event Action<RoundRecord> OnRoundCompleted;
        public event Action<int, List<RoundRecord>> OnGameCompleted;

        public RoundConfig CurrentRoundConfig => (rounds != null && _currentRoundIndex >= 0 && _currentRoundIndex < rounds.Length)
            ? rounds[_currentRoundIndex]
            : null;

        public int CurrentRoundIndex => _currentRoundIndex;
        public int CurrentRoundNumber => _currentRoundIndex + 1;
        public int TotalRounds => rounds != null ? rounds.Length : 5;
        public int CurrentArrowInRound => _currentArrowInRound;
        public int TotalArrowsShot => _totalArrowsShot;
        public int CurrentRoundScore => _currentRoundScore;
        public int TotalScore => _totalScore;
        public string LastShotResult => _lastShotResult;
        public int LastShotScore => _lastShotScore;
        public GameState CurrentState => _currentState;
        public IReadOnlyList<RoundRecord> RoundHistory => _roundHistory;

        private void Awake()
        {
            if (rounds == null || rounds.Length == 0)
            {
                rounds = RoundConfig.CreateDefaultRambahans();
            }
        }

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _currentRoundIndex = 0;
            _totalArrowsShot = 0;
            _totalScore = 0;
            _roundHistory.Clear();
            _lastShotResult = "Siap Menembak";
            _lastShotScore = 0;

            StartRound(_currentRoundIndex);
        }

        public void StartRound(int roundIndex)
        {
            _currentRoundIndex = roundIndex;
            _currentArrowInRound = 0;
            _currentRoundScore = 0;

            var config = CurrentRoundConfig;
            if (config != null && target != null)
            {
                target.ClearAttachedArrows();
                target.SetTargetDistance(config.targetDistance);
                target.MovementSpeed = config.targetMovementSpeed;
            }

            SetState(GameState.ReadyToShoot);
            OnScoreUpdated?.Invoke();
        }

        public void OnArrowFired()
        {
            _currentArrowInRound++;
            _totalArrowsShot++;
            SetState(GameState.ArrowInFlight);
            OnScoreUpdated?.Invoke();
        }

        public void OnArrowResolved(string zoneName, int points, bool isHit)
        {
            _lastShotScore = points;
            _lastShotResult = isHit ? $"{zoneName} (+{points})" : "Luput (+0)";

            _currentRoundScore += points;
            _totalScore += points;

            OnScoreUpdated?.Invoke();

            var config = CurrentRoundConfig;
            int maxArrows = config != null ? config.arrowsPerRound : 4;

            if (_currentArrowInRound >= maxArrows)
            {
                EndRound();
            }
            else
            {
                SetState(GameState.ReadyToShoot);
            }
        }

        private void EndRound()
        {
            var config = CurrentRoundConfig;
            int objective = config != null ? config.targetScoreObjective : 4;
            bool achieved = _currentRoundScore >= objective;

            RoundRecord record = new RoundRecord
            {
                roundNumber = CurrentRoundNumber,
                roundScore = _currentRoundScore,
                objectiveScore = objective,
                objectiveAchieved = achieved
            };
            _roundHistory.Add(record);

            SetState(GameState.RoundSummary);
            OnRoundCompleted?.Invoke(record);
        }

        public void AdvanceToNextRound()
        {
            if (_currentState != GameState.RoundSummary) return;

            if (_currentRoundIndex + 1 < TotalRounds)
            {
                StartRound(_currentRoundIndex + 1);
            }
            else
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            SetState(GameState.GameOver);
            OnGameCompleted?.Invoke(_totalScore, _roundHistory);
        }

        public void RestartGame()
        {
            StartGame();
        }

        private void SetState(GameState newState)
        {
            _currentState = newState;
            OnGameStateChanged?.Invoke(newState);
        }
    }
}
