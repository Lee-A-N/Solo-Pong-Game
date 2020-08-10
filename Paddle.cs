//-----------------------------------------------------------------------
//     Author: Lee Nayes, Logikos, Inc. All rights reserved.
//-----------------------------------------------------------------------

namespace SoloPong
{
    using Meadow.Foundation;

    /// <summary>
    /// Class representing the paddle
    /// </summary>
    public class Paddle
    {
        /// <summary>
        /// Height of the paddle
        /// </summary>
        public const int HEIGHT = 3;

        /// <summary>
        /// How far to move the paddle left or right for one rotation instance
        /// </summary>
        private const int INCREMENT = 30;

        /// <summary>
        /// Amount to shrink the paddle width after each paddle hit. Shrinking
        /// stops once the minimum width is reached.
        /// </summary>
        private const int SHRINK_AMOUNT = 3;

        /// <summary>
        /// Width of the display in pixels
        /// </summary>
        private readonly int displayWidth;

        /// <summary>
        /// Color of the paddle
        /// </summary>
        private readonly Color paddleColor = Color.White;

        /// <summary>
        /// Display background color
        /// </summary>
        private readonly Color backgroundColor;

        /// <summary>
        /// Reference to the graphics object
        /// </summary>
        private readonly AsyncGraphics asyncGraphics; 

        /// <summary>
        /// Y-coordinate of the top of the paddle
        /// </summary>
        private readonly int y;

        /// <summary>
        /// Current x-coordinate of the left of the paddle
        /// </summary>
        private int position;

        /// <summary>
        /// Number of locks currently active for this object instance. Used for deadlock protection.
        /// </summary>
        private int lockCount = 0;

        /// <summary>
        /// Initializes a new instance of the Paddle class
        /// </summary>
        /// <param name="asyncGraphics">Reference to the graphics object</param>
        /// <param name="displayWidth">Width of the display</param>
        /// <param name="displayHeight">Height of the display</param>
        /// <param name="backgroundColor">Background color of the display</param>
        public Paddle(AsyncGraphics asyncGraphics, int displayWidth, int displayHeight, Color backgroundColor)
        {
            this.asyncGraphics = asyncGraphics;

            this.displayWidth = displayWidth;
            this.backgroundColor = backgroundColor;
            this.y = displayHeight - (Paddle.HEIGHT / 2);
        }

        /// <summary>
        /// Gets or sets the width of the paddle in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets the left x-coordinate of the paddle position.
        /// </summary>
        public int Left
        {
            get { return this.position; }
        }

        /// <summary>
        /// Gets the x-coordinate of the right side of the paddle
        /// </summary>
        public int Right
        {
            get { return this.position + this.Width; }
        }

        /// <summary>
        /// Gets the maximum left x-coordinate for the paddle
        /// </summary>
        public int MaxRight
        {
            get { return this.displayWidth - this.Width; }
        }

        /// <summary>
        /// Reset the paddle, drawing it in the game-start position
        /// </summary>
        public void Reset()
        {
            this.position = this.displayWidth / 3;
            this.Width = this.displayWidth / 3;

            // draw the paddle in the starting position
            this.Draw(this.position, this.position + this.Width, this.paddleColor);
        }

        /// <summary>
        /// Move the paddle, making sure it stays between minimum and maximum
        /// display limits, then erase the old paddle position and draw the
        /// new paddle position.
        /// </summary>
        /// <param name="increment">How much to move the paddle.</param>
        public void Move(int increment)
        {
            // calculate the new position
            int newPosition = this.position + increment;

            if (newPosition > this.MaxRight)
            {
                newPosition = this.MaxRight;
            }
            else if (newPosition < 0)
            {
                newPosition = 0;
            }
            else
            {
                // do nothing
            }

            if (this.position != newPosition)
            {
                int right = this.position + this.Width;
                int delta = newPosition - this.position;

                int oldPosition = this.position;
                this.position = newPosition;

                if (delta > 0)
                {
                    if (this.lockCount < 1)
                    {
                        ++this.lockCount;

                        // Moving right
                        lock (this.asyncGraphics.LockObject)
                        {
                            // Erase from the old left x-coordinate to the new left x-coordinate
                            this.Draw(oldPosition, oldPosition + delta, this.backgroundColor);

                            // Draw from the old right x-coordinate to the new right x-coordinate
                            this.Draw(right, right + delta, this.paddleColor);
                        }

                        --this.lockCount;
                    }
                }
                else if (delta < 0)
                {
                    if (this.lockCount < 1)
                    {
                        ++this.lockCount;

                        // Moving left
                        lock (this.asyncGraphics.LockObject)
                        {
                            // Draw from the old left x-coordinate to the new left x-coordinate
                            this.Draw(oldPosition, newPosition, this.paddleColor);

                            // Erase from the old right x-coordinate to the new right x-coordinate
                            this.Draw(right, right + delta, this.backgroundColor);
                        }

                        --this.lockCount;
                    }
                }
                else
                {
                    // do nothing
                }
            }
        }

        /// <summary>
        /// Move the paddle to the right
        /// </summary>
        public void MoveRight()
        {
            this.Move(Paddle.INCREMENT);
        }

        /// <summary>
        /// Move the paddle to the left
        /// </summary>
        public void MoveLeft()
        {
            this.Move(-Paddle.INCREMENT);
        }

        /// <summary>
        /// Shrink the paddle unless the minimum width has been reached
        /// </summary>
        public void Shrink()
        {
            if (this.Width > this.displayWidth / 5)
            {
                int oldRight = this.Right;
                this.Width -= Paddle.SHRINK_AMOUNT;
                this.Draw(oldRight, this.Right, this.backgroundColor);
            }
        }

        /// <summary>
        /// Draw the paddle. Used for both erasing the paddle at the old position
        /// and drawing paddle at the new position.
        /// </summary>
        /// <param name="left">Left x-coordinate</param>
        /// <param name="right">Right x-coordinate</param>
        /// <param name="color">Paddle color</param>
        private void Draw(int left, int right, Color color)
        {
            this.asyncGraphics.DrawLine(left, this.y, right, this.y, color);
        }
    }
}
