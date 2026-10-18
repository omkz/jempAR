using UnityEngine;

namespace JemparinganXR.Archery
{
    /// <summary>
    /// Abstract input interface for archery. Allows mouse/keyboard for prototyping
    /// and clean drop-in replacement with Meta XR Hand Tracking in later phases.
    /// </summary>
    public interface IArcheryInput
    {
        /// <summary>
        /// Aim delta in screen space (e.g., mouse movement).
        /// </summary>
        Vector2 LookDelta { get; }

        /// <summary>
        /// True while the draw action is held down (e.g. Left Mouse Button held).
        /// </summary>
        bool IsDrawHeld { get; }

        /// <summary>
        /// True on the frame the draw action is released (e.g. Left Mouse Button up).
        /// </summary>
        bool IsDrawReleased { get; }

        /// <summary>
        /// True if the player requests cancelling an active draw (e.g. Right Mouse Button).
        /// </summary>
        bool IsCancelRequested { get; }

        /// <summary>
        /// True if the player presses continue to advance round (e.g. Space / Enter).
        /// </summary>
        bool IsContinueRequested { get; }

        /// <summary>
        /// True if the player requests restarting the session (e.g. R key).
        /// </summary>
        bool IsRestartRequested { get; }
    }
}
