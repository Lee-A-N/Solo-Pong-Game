//-----------------------------------------------------------------------
//     Author: Lee Nayes, Logikos, Inc. All rights reserved.
//-----------------------------------------------------------------------

namespace SoloPong
{
    using Meadow.Foundation;

    /// <summary>
    /// Class representing a color-band that spans the display from left to right.
    /// Text is displayed within the banner.
    /// </summary>
    public class Banner
    {
        /// <summary>
        /// Height of the banner
        /// </summary>
        public const int HEIGHT = 18;

        /// <summary>
        /// Text to display when the game starts
        /// </summary>
        public const string START_TEXT = "Press knob to start";

        /// <summary>
        /// Text to display in the banner when the current game is over
        /// </summary>
        public const string RESTART_TEXT = "Press to restart";

        /// <summary>
        /// Text to display in front of the current score
        /// </summary>
        public const string SCORE_TEXT = "SCORE: ";

        /// <summary>
        /// Width of the banner
        /// </summary>
        private readonly int width;

        /// <summary>
        /// Top of the banner
        /// </summary>
        private readonly int top;

        /// <summary>
        /// Reference to the graphics object
        /// </summary>
        private readonly AsyncGraphics asyncGraphics;

        /// <summary>
        /// Background color of the display
        /// </summary>
        private readonly Color backgroundColor;

        /// <summary>
        /// Color to use for the banner
        /// </summary>
        private readonly Color color;

        /// <summary>
        /// Initializes a new instance of the Banner class
        /// </summary>
        /// <param name="displayWidth">Width of the display</param>
        /// <param name="graphics">Graphics object reference</param>
        /// <param name="fontHeight">Height of the text font</param>
        /// <param name="backgroundColor">Background color of the display</param>
        /// <param name="color">Banner color</param>
        /// <param name="top">Top y coordinate for the banner</param>
        public Banner(int displayWidth, AsyncGraphics graphics, int fontHeight, Color backgroundColor, Color color, int top)
        {
            this.width = displayWidth;
            this.Height = Banner.HEIGHT;
            this.asyncGraphics = graphics;
            this.FontHeight = fontHeight;
            this.backgroundColor = backgroundColor;
            this.color = color;
            this.top = top;
        }

        /// <summary>
        /// Gets or sets the font height.
        /// </summary>
        public int FontHeight { get; set; }

        /// <summary>
        /// Gets or sets the text displayed on the banner
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the banner height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Draw the banner
        /// </summary>
        public void Draw()
        {
            this.asyncGraphics.DrawRectangle(
                left: 0,
                top: this.top,
                width: this.width,
                height: this.Height,
                color: this.color);

            int y = this.top + ((this.Height - this.FontHeight) / 2);
            this.asyncGraphics.DrawText(this.Text, 5, y, Color.Black);
        }

        /// <summary>
        /// Erase the banner
        /// </summary>
        public void Hide()
        {
            this.asyncGraphics.DrawRectangle(
                left: 0,
                top: this.top,
                width: this.width,
                height: this.Height,
                color: this.backgroundColor);
        }

        /// <summary>
        /// Handler sor the score-changed event
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        public void OnScoreChanged(object sender, ScoreKeeper.ScoreChangedArgs args)
        {
            int y = this.top + ((this.Height - this.FontHeight) / 2);

            // erase the old score
            this.asyncGraphics.DrawText(args.OldScore.ToString(), this.width / 2, y, this.color);

            // draw the new score
            this.asyncGraphics.DrawText(args.Score.ToString(), this.width / 2, y, Color.Black);
        }
    }
}
