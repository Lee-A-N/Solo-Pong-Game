//-----------------------------------------------------------------------
//     Author: Lee Nayes, Logikos, Inc. All rights reserved.
//-----------------------------------------------------------------------

namespace SoloPong
{
    using System;
    using System.Threading;
    using System.Timers;
    using Meadow.Foundation;

    /// <summary>
    /// Class representing the ball.
    /// </summary>
    public class Ball
    {
        /// <summary>
        /// Milliseconds between ball position updates.
        /// </summary>
        private const int MOVE_INTERVAL = 200;

        /// <summary>
        /// Reference to the graphics interface
        /// </summary>
        private readonly AsyncGraphics asyncGraphics;

        /// <summary>
        /// Timer used for ball position updates
        /// </summary>
        private readonly System.Timers.Timer moveTimer = new System.Timers.Timer(Ball.MOVE_INTERVAL);

        /// <summary>
        /// Reference to the game's paddle object. Needed for collision detection.
        /// </summary>
        private readonly Paddle paddle;

        /// <summary>
        /// Random number generator
        /// </summary>
        private readonly Random random = new Random();

        /// <summary>
        /// Reference to the sound generator object
        /// </summary>
        private readonly ISounds soundGenerator;

        /// <summary>
        /// Reference to the score-keeper object
        /// </summary>
        private readonly ScoreKeeper scoreKeeper;

        /// <summary>
        /// Background color of the display
        /// </summary>
        private readonly Color backgroundColor;

        /// <summary>
        /// Maximum x coordinate for the upper left of the ball's enclosing rectangle
        /// </summary>
        private readonly int maxX;

        /// <summary>
        /// Maximum y coordinate for the upper left of the ball's enclosing rectangle
        /// </summary>
        private readonly int maxY;

        /// <summary>
        /// Minimum y coordinate for the upper left of the ball's enclosing rectangle
        /// </summary>
        private readonly int minY;

        /// <summary>
        /// Width of the ball
        /// </summary>
        private readonly int width = 10;

        /// <summary>
        /// Height of the ball
        /// </summary>
        private readonly int height = 10;

        /// <summary>
        /// Width of the display
        /// </summary>
        private readonly int displayWidth;

        /// <summary>
        /// Color to use for the ball
        /// </summary>
        private readonly Color color = Color.Red;

        /// <summary>
        /// Current x coordinate of the ball's enclosing rectangle
        /// </summary>
        private int xPosition = 5;

        /// <summary>
        /// Current y coordinate of the ball's enclosing rectangle
        /// </summary>
        private int yPosition;

        /// <summary>
        /// X increment to use for the next ball position update
        /// </summary>
        private int xIncrement;

        /// <summary>
        /// Y increment to use for the next ball position update
        /// </summary>
        private int yIncrement;

        /// <summary>
        /// Value indicating whether the current move is complete
        /// </summary>
        private bool isMoveComplete = true;

        /// <summary>
        /// Number of locks currently active for this object instance. Used for deadlock protection.
        /// </summary>
        private int lockCount = 0;

        /// <summary>
        /// Initializes a new instance of the Ball class.
        /// </summary>
        /// <param name="asyncGraphics">Graphics object reference</param>
        /// <param name="displayWidth">Width of the display</param>
        /// <param name="displayHeight">Height of the display</param>
        /// <param name="backgroundColor">Background color of the display</param>
        /// <param name="paddle">Reference to the game's paddle object</param>
        /// <param name="soundGenerator">Reference to the game's sound generator object</param>
        /// <param name="minimumY">Minimum y coordinate to use the the ball's enclosing rectangle</param>
        /// <param name="scoreKeeper">Reference to the game's scorekeeper object</param>
        public Ball(
            AsyncGraphics asyncGraphics, 
            int displayWidth, 
            int displayHeight, 
            Color backgroundColor, 
            Paddle paddle, 
            ISounds soundGenerator, 
            int minimumY,
            ScoreKeeper scoreKeeper)
        {
            this.asyncGraphics = asyncGraphics;
            this.soundGenerator = soundGenerator;

            this.scoreKeeper = scoreKeeper;

            this.backgroundColor = backgroundColor;
            this.paddle = paddle;

            this.displayWidth = displayWidth;
            this.maxX = displayWidth - this.width;
            this.maxY = displayHeight - this.height - Paddle.HEIGHT;
            this.minY = minimumY;
            this.yPosition = this.minY + 5;
 
            this.moveTimer.AutoReset = true;
            this.moveTimer.Elapsed += this.MoveTimer_Elapsed;
        }

        /// <summary>
        /// Delegate used for game-over notification
        /// </summary>
        /// <param name="sender">Object that initiated the event</param>
        /// <param name="args">Event arguments</param>
        public delegate void NotifyGameOver(object sender, EventArgs args);

        /// <summary>
        /// Event used for game-over notification
        /// </summary>
        public event NotifyGameOver ExplosionOccurred;

        /// <summary>
        /// Reset the ball position for the start of a new game instance
        /// </summary>
        public void Reset()
        {
            this.xPosition = this.random.Next(5, this.displayWidth - 5 - this.width);
            this.yPosition = this.minY + 5;

            int xDirectionRandom = this.random.Next(0, 1);
            int directionMultiplier = xDirectionRandom > 0 ? -1 : 1;

            int incrementRandom = this.random.Next(6, 10);

            this.xIncrement = incrementRandom * directionMultiplier;
            this.yIncrement = 20 - incrementRandom;

            // draw the ball in the starting position
            this.Draw(this.xPosition, this.yPosition, this.color);
        }

        /// <summary>
        /// Start periodic ball movement
        /// </summary>
        public void StartMoving()
        {
            this.moveTimer.Start();
        }

        /// <summary>
        /// Stop periodic ball movement
        /// </summary>
        public void StopMoving()
        {
            this.moveTimer.Stop();
        }

        /// <summary>
        /// Event handler called when the movement timer elapses
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void MoveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                MeadowApp.DebugWriteLine("Move timer elapsed");

                if (this.isMoveComplete)
                {
                    this.isMoveComplete = false;

                    if (!this.CheckForCollision())
                    {
                        int oldX = this.xPosition;
                        int oldY = this.yPosition;

                        MeadowApp.DebugWriteLine($"Move timer elapsed: {oldX},{oldY} {xIncrement},{yIncrement}");

                        this.xPosition += this.xIncrement;

                        if (this.xPosition > this.maxX)
                        {
                            this.xPosition = this.maxX;
                        }

                        MeadowApp.DebugWriteLine($"new x = {this.xPosition}");

                        if (this.xPosition < 0)
                        {
                            this.xPosition = 0;
                        }

                        this.yPosition += this.yIncrement;

                        MeadowApp.DebugWriteLine($"new y = {this.yPosition}");

                        if (this.yPosition > this.maxY)
                        {
                            this.yPosition = this.maxY;
                        }

                        if (this.yPosition < this.minY)
                        {
                            this.yPosition = this.minY;
                        }

                        MeadowApp.DebugWriteLine($"new x,y = {this.xPosition},{this.yPosition}");

                        if (!(this.xPosition == oldX && this.yPosition == oldY))
                        {
                            if (this.lockCount < 1)
                            {
                                ++this.lockCount;

                                lock (this.asyncGraphics.LockObject)
                                {
                                    if (this.moveTimer.Enabled)
                                    {
                                        this.Draw(oldX, oldY, this.backgroundColor);
                                        this.Draw(this.xPosition, this.yPosition, this.color);
                                    }
                                }

                                --this.lockCount;
                            }
                        }
                    }

                    this.isMoveComplete = true;
                }

                MeadowApp.DebugWriteLine("leaving move timer elapsed");
            }
            catch (Exception ex)
            {
                MeadowApp.DebugWriteLine($"Exception in MoveTimer_Elapsed: {ex}");
                this.isMoveComplete = true;
            }
        }

        /// <summary>
        /// Modify the X increment to use for the next ball position move.
        /// Adjusting the x and y speed on border and paddle collisions
        /// makes the game more interesting. :)
        /// </summary>
        /// <param name="extraX">Additional value to add to the x increment</param>
        private void ChangeXIncrement(int extraX)
        {
            this.xIncrement += this.random.Next(-1, 1);
            this.xIncrement += extraX;

            if (this.xIncrement < 0)
            {
                if (this.xIncrement > -7)
                {
                    this.xIncrement = -7;
                }
            }
            else
            {
                if (this.xIncrement < 7)
                {
                    this.xIncrement = 7;
                }
            }
        }

        /// <summary>
        /// Modify the Y increment to use for the next ball position move.
        /// Adjusting the x and y speed on border and paddle collisions
        /// makes the game more interesting. :)
        /// </summary>
        /// <param name="extraY">Additional value to add to the y increment</param>
        private void ChangeYIncrement(int extraY)
        {
            this.yIncrement += this.random.Next(-1, 1) + extraY;

            if (this.yIncrement < 0)
            {
                if (this.yIncrement > -7)
                {
                    this.yIncrement = -7;
                }
            }
            else
            {
                if (this.yIncrement < 7)
                {
                    this.yIncrement = 7;
                }
            }
        }

        /// <summary>
        /// Get the adjustments to use for the x and y increments based on the position
        /// at which the ball strikes the paddle. Changing the x and y velocity like
        /// this makes the game more interesting. :)
        /// </summary>
        /// <param name="ballCenterX">Current x coordinate of the bottom of the ball</param>
        /// <param name="extraX">Out: Extra value to add to the x increment</param>
        /// <param name="extraY">Out: Extra value to add to the y increment</param>
        private void GetVelocityChangeAdjustments(int ballCenterX, out int extraX, out int extraY)
        {
            float oneThirdPaddleWidth = this.paddle.Width / 3;

            if (this.xIncrement > 0)
            {
                // traveling right
                if (ballCenterX < this.paddle.Left + oneThirdPaddleWidth)
                {
                    extraX = -2;    // a bit less horizontal velocity
                    extraY = -2;    // a bit more vertical velocity
                }
                else if (ballCenterX > this.paddle.Left + (oneThirdPaddleWidth * 2))
                {
                    extraX = 2;    // a bit more horizontal velocity
                    extraY = 2;    // a bit less vertical velocity
                }
                else
                {
                    extraX = 0;
                    extraY = 0;
                }
            }
            else
            {
                // traveling left
                if (ballCenterX < this.paddle.Left + oneThirdPaddleWidth)
                {
                    extraX = -2;   // a bit more horizontal velocity
                    extraY = 2;    // a bit less vertical velocity
                }
                else if (ballCenterX > this.paddle.Left + (oneThirdPaddleWidth * 2))
                {
                    extraX = 2;    // a bit less horizontal velocity
                    extraY = -2;    // a bit more vertical velocity
                }
                else
                {
                    extraX = 0;
                    extraY = 0;
                }
            }
        }

        /// <summary>
        /// Check for collisions between the ball and the sides, top and bottom of the display,
        /// also checking for collisions with the paddle.
        /// </summary>
        /// <returns>Value indicating that the paddle was missed, signaling game-over</returns>
        private bool CheckForCollision()
        {
            bool isPaddleMissed = false;
            bool isBorderHit = false;

            int ballCenterX = this.xPosition + (this.width / 2);

            MeadowApp.DebugWriteLine($"checkForCollision: {ballCenterX},{this.yPosition}");

            if (this.yPosition >= this.maxY)
            {
                if (ballCenterX >= this.paddle.Left && ballCenterX < this.paddle.Right)
                {
                    // Paddle hit!
                    this.soundGenerator.PlayPaddleHitSound();
                    this.scoreKeeper.Increment();

                    this.GetVelocityChangeAdjustments(ballCenterX, out int extraX, out int extraY);

                    this.yIncrement = -this.yIncrement;

                    // Apply the x and y increments
                    this.ChangeXIncrement(extraX);
                    this.ChangeYIncrement(extraY);

                    // Make the paddle smaller
                    this.paddle.Shrink();
                }
                else
                {
                    // Missed the paddle.  Time to explode...
                    this.asyncGraphics.Stop();
                    this.StopMoving();
                    this.soundGenerator.PlayGameOverSound();
                    this.Explode();

                    isPaddleMissed = true;
                }
            }
            else
            {
                if (this.xPosition >= this.maxX || this.xPosition <= 0)
                {
                    MeadowApp.DebugWriteLine("x border hit");
                    isBorderHit = true;
                    this.xIncrement = -this.xIncrement;
                    this.ChangeYIncrement(0);
                }

                if (this.yPosition <= this.minY)
                {
                    MeadowApp.DebugWriteLine("y border hit");
                    isBorderHit = true;
                    this.yIncrement = -this.yIncrement;
                    this.ChangeXIncrement(0);
                }

                if (isBorderHit)
                {
                    this.soundGenerator.PlayBorderHitSound();
                }
            }

            MeadowApp.DebugWriteLine($"leaving checkForCollision: {isPaddleMissed}");
            return isPaddleMissed;
        }

        /// <summary>
        /// Display the game-over animation and trigger the game-over event.
        /// </summary>
        private void Explode()
        {
            int x = this.xPosition + (this.width / 2);
            int y = this.yPosition - Paddle.HEIGHT;
            int radius = 4;
            int blackRadius = 0;

            for (int ii = 0; ii < 4; ++ii)
            {
                this.asyncGraphics.DrawCircle(x, y, radius, Color.Yellow);
                this.asyncGraphics.DrawCircle(x, y, (int)(radius * 0.7), Color.Orange);
                this.asyncGraphics.DrawCircle(x, y, (int)(radius * 0.4), Color.Red);

                blackRadius = (int)(radius * 0.2);
                this.asyncGraphics.DrawCircle(x, y, blackRadius, Color.Black);

                this.asyncGraphics.ShowDirect();
                Thread.Sleep(100);

                radius *= 2;
            }

            this.ExplosionOccurred?.Invoke(this, null);
        }

        /// <summary>
        /// Draw the ball in the current position. This is used for both erasing the ball at the
        /// previous position and drawing the ball at the new position.
        /// </summary>
        /// <param name="x">x coordinate of the upper left of the ball's enclosing rectangle</param>
        /// <param name="y">y coordinate of the upper left of the ball's enclosing rectangle</param>
        /// <param name="color">Color used to draw the ball</param>
        private void Draw(int x, int y, Color color)
        {
            this.asyncGraphics.DrawCircle(
                x: x + (this.width / 2),
                y: y + (this.height / 2),
                radius: this.width / 2,
                color: color);
        }
    }
}
