//-----------------------------------------------------------------------
//     Author: Lee Nayes, Logikos, Inc. All rights reserved.
//-----------------------------------------------------------------------

namespace SoloPong
{
    /// <summary>
    /// Interface used for the sound generator object
    /// </summary>
    public interface ISounds
    {
        /// <summary>
        /// Play the left, right, or top border-hit sound effect.
        /// </summary>
        void PlayBorderHitSound();

        /// <summary>
        /// Play the paddle-hit sound effect.
        /// </summary>
        void PlayPaddleHitSound();

        /// <summary>
        /// Play the game-over sound effect
        /// </summary>
        void PlayGameOverSound();

        /// <summary>
        /// Play the game-start sound effect
        /// </summary>
        void PlayStartSound();

        /// <summary>
        /// Play the end-of-construction sound effect
        /// This indicates readiness for a game-start knob press.
        /// </summary>
        void PlayConstructionCompleteSound();
    }
}
