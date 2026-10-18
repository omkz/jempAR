using System;
using UnityEngine;

namespace JemparinganXR.GameFlow
{
    /// <summary>
    /// Configuration data for a single Rambahan (round) in Jemparingan.
    /// Supports distance, movement, wind, and score objectives.
    /// </summary>
    [Serializable]
    public class RoundConfig
    {
        [Header("Round Details")]
        public int roundNumber = 1;
        public string roundTitle = "Rambahan 1";
        public int arrowsPerRound = 4;

        [Header("Target & Environment")]
        [Tooltip("Distance from archer to target in meters.")]
        public float targetDistance = 15f;

        [Tooltip("Target horizontal movement speed. 0 = stationary.")]
        public float targetMovementSpeed = 0f;

        [Tooltip("Wind force applied to arrows. 0 = disabled.")]
        public float windStrength = 0f;
        public Vector3 windDirection = Vector3.right;

        [Header("Objectives")]
        [Tooltip("Target score objective for this rambahan.")]
        public int targetScoreObjective = 4;

        public Vector3 GetWindVector()
        {
            return windDirection.normalized * windStrength;
        }

        public static RoundConfig[] CreateDefaultRambahans()
        {
            return new RoundConfig[]
            {
                new RoundConfig
                {
                    roundNumber = 1,
                    roundTitle = "Rambahan 1: Gladhen Awal (15m)",
                    arrowsPerRound = 4,
                    targetDistance = 15f,
                    targetMovementSpeed = 0f,
                    windStrength = 0f,
                    targetScoreObjective = 4
                },
                new RoundConfig
                {
                    roundNumber = 2,
                    roundTitle = "Rambahan 2: Nglatih Ketepatan (18m)",
                    arrowsPerRound = 4,
                    targetDistance = 18f,
                    targetMovementSpeed = 0f,
                    windStrength = 0f,
                    targetScoreObjective = 4
                },
                new RoundConfig
                {
                    roundNumber = 3,
                    roundTitle = "Rambahan 3: Jarak Sedang (20m)",
                    arrowsPerRound = 4,
                    targetDistance = 20f,
                    targetMovementSpeed = 0f,
                    windStrength = 0f,
                    targetScoreObjective = 6
                },
                new RoundConfig
                {
                    roundNumber = 4,
                    roundTitle = "Rambahan 4: Peningkatan Fokus (22m)",
                    arrowsPerRound = 4,
                    targetDistance = 22f,
                    targetMovementSpeed = 0f,
                    windStrength = 0f,
                    targetScoreObjective = 6
                },
                new RoundConfig
                {
                    roundNumber = 5,
                    roundTitle = "Rambahan 5: Gladhen Utama (25m)",
                    arrowsPerRound = 4,
                    targetDistance = 25f,
                    targetMovementSpeed = 0f,
                    windStrength = 0f,
                    targetScoreObjective = 8
                }
            };
        }
    }
}
