//-----------------------------------------------------------------------
//    ScoreKeeper.cs
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
    using System;

    /// <summary>
    /// Class used for maintaining the game score
    /// </summary>
    public class ScoreKeeper
    {
        /// <summary>
        /// Value representing the current score
        /// </summary>
        private int score = -1;

        /// <summary>
        /// Initializes a new instance of the ScoreKeeper class.
        /// </summary>
        public ScoreKeeper()
        {
            this.Reset();
        }

        /// <summary>
        /// Delegate for the score-changed notification event.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        public delegate void NotifyScoreChanged(object sender, ScoreChangedArgs args);

        /// <summary>
        /// Event used for score-change notifications
        /// </summary>
        public event NotifyScoreChanged ScoreChanged;

        /// <summary>
        /// Gets or sets the current score
        /// </summary>
        public int Score
        {
            get
            {
                return this.score;
            }

            set
            {
                if (this.score != value)
                {
                    int oldScore = this.score;
                    this.score = value;

                    // Trigger the score-changed event
                    this.ScoreChanged?.Invoke(this, new ScoreChangedArgs(oldScore, this.score));
                }
            }
        }

        /// <summary>
        /// Reset the score
        /// </summary>
        public void Reset()
        {
            this.Score = 0;
        }

        /// <summary>
        /// Increment the score.
        /// </summary>
        public void Increment()
        {
            ++this.Score;
        }

        /// <summary>
        /// Class used for score-changed event arguments
        /// </summary>
        public class ScoreChangedArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the ScoreChangedArgs class
            /// </summary>
            /// <param name="oldScore">Previous score</param>
            /// <param name="newScore">New Score</param>
            public ScoreChangedArgs(int oldScore, int newScore)
            {
                this.OldScore = oldScore;
                this.Score = newScore;
            }

            /// <summary>
            /// Gets the old score
            /// </summary>
            public int OldScore { get; private set; }

            /// <summary>
            /// Gets the current (new) score
            /// </summary>
            public int Score { get; private set; }
        }
    }
}
