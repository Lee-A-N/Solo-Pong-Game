//-----------------------------------------------------------------------
//    ISounds.cs
//
//    Copyright 2020 Lee Nayes, Logikos, Inc.
//
//    This file is part of Solo Pong.
//
//    Solo Pong is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Solo Pong is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Solo Pong (in COPYING.txt).  If not, 
//    see <https://www.gnu.org/licenses/>.
//
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
