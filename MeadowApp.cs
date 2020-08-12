//-----------------------------------------------------------------------
//    MeadowApp.cs
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
//
// This is the "Main" class for the Solo Pong game.
//
// Hardware: Meadow F7 board, Rotary Encoder, 3-position SPDT On/Off/On switch, ST7789 LDC display, 
//              Piezo speaker, 0.1 microFarad capacitor, 2 10K resistors
//
// All of the hardware is include in the Pro Hack Kit.
//
// Wiring:
//
//  SPDT 3-position On/Off/On switch (used for volume settings Mute/Soft/Normal):
//      top - 10K resistor to ground, pin D03
//      middle - Vcc
//      bottom - 10K resistor to ground, pin D04
//
//  Rotary Encoder:
//      CLK - pin D10
//      DT - pin D09
//      SW - 0.1 microFarad capacitor to ground, pin D08
//      + - Vcc
//      GND - Ground
//
//  Piezo Speaker:
//      Red: Pin D07
//      Black: Ground
//
//  ST7789 Display:
//      GND: Ground
//      VCC: Vcc
//      SCL: SCK
//      SDA: MOSI
//      RES: Pin D00
//      DC: Pin D01
//      
//  ------------------------------------------
//  NuGet Packages needed:
//      Meadow.Foundation
//      Meadow.Foundation.Displays.TftSpi
//-----------------------------------------------------------------------

namespace SoloPong
{
    using System;
    using System.Timers;
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation;
    using Meadow.Foundation.Displays.Tft;
    using Meadow.Foundation.Graphics;
    using Meadow.Foundation.Sensors.Rotary;
    using Meadow.Hardware;
    using Meadow.Peripherals.Sensors.Rotary;

    /// <summary>
    /// Main class for the Solo Pong application
    /// </summary>
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        /// <summary>
        /// Interval used for knob-rotation debounce. This is used to
        /// throttle paddle movement so that it does not out-pace
        /// screen refresh, causing the paddle to jump from one place
        /// on the screen to another.
        /// </summary>
        private const int KNOB_ROTATION_DEBOUNCE_INTERVAL = 100;

        /// <summary>
        /// Object representing the display hardware
        /// </summary>
        private readonly St7789 st7789;

        /// <summary>
        /// Reference to the Meadow graphics library facade object
        /// </summary>
        private readonly AsyncGraphics asyncGraphics;

        /// <summary>
        /// Object representing the rotary encoder
        /// </summary>
        private readonly RotaryEncoderWithButton rotaryPaddle;

        /// <summary>
        /// Reference for the sound-generator object
        /// </summary>
        private readonly ISounds soundGenerator;

        /// <summary>
        /// Width of the display
        /// </summary>
        private readonly int displayWidth;

        /// <summary>
        /// Height of the display
        /// </summary>
        private readonly int displayHeight;

        /// <summary>
        /// Background color of the display
        /// </summary>
        private readonly Color backgroundColor;

        /// <summary>
        /// Timer used for knob-rotation debounce
        /// </summary>
        private readonly System.Timers.Timer debounceTimer = new System.Timers.Timer(MeadowApp.KNOB_ROTATION_DEBOUNCE_INTERVAL);

        /// <summary>
        /// Reference to the scoreKeeper object
        /// </summary>
        private readonly ScoreKeeper scoreKeeper;

        /// <summary>
        /// Reference to the paddle object
        /// </summary>
        private readonly Paddle paddle;

        /// <summary>
        /// Reference to the ball object
        /// </summary>
        private readonly Ball ball;

        /// <summary>
        /// Reference to the instruction banner object
        /// </summary>
        private readonly Banner instructionBanner;

        /// <summary>
        /// Reference to the score banner object
        /// </summary>
        private readonly Banner scoreBanner;

        /// <summary>
        /// Value indicating whether knob-rotation debounce is active
        /// </summary>
        private bool isDebounceActive = false;

        /// <summary>
        /// Value incremented or decremented to add "momentum" to knob
        /// rotation, eliminating weird effects when changing from clockwise
        /// to counter-clockwise rotation or vice-versa.
        /// </summary>
        private int directionCounter = 0;

        /// <summary>
        /// Value used for debouncing rotary-paddle clicks, which are very noisy.
        /// </summary>
        private int rotaryPaddleClickCount = 0;

        /// <summary>
        /// Initializes a new instance of the MeadowApp class
        /// </summary>
        public MeadowApp()
        {
            MeadowApp.DebugWriteLine("Initializing...");

            this.soundGenerator = new SoundGenerator(
                Device.CreateDigitalInputPort(Device.Pins.D03),
                Device.CreateDigitalInputPort(Device.Pins.D04),
                Device.CreatePwmPort(Device.Pins.D07));

            var config = new SpiClockConfiguration(48000, SpiClockConfiguration.Mode.Mode3);

            this.rotaryPaddle = new RotaryEncoderWithButton(Device, Device.Pins.D10, Device.Pins.D09, Device.Pins.D08, debounceDuration: 100);
            this.rotaryPaddle.Rotated += this.RotaryPaddle_Rotated;

            this.st7789 = new St7789(
                device: Device,
                spiBus: Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240, 
                height: 240);

            this.displayWidth = Convert.ToInt32(this.st7789.Width);
            this.displayHeight = Convert.ToInt32(this.st7789.Height);

            GraphicsLibrary graphics = new GraphicsLibrary(this.st7789)
            {
                Rotation = GraphicsLibrary.RotationType._270Degrees
            };
            this.asyncGraphics = new AsyncGraphics(graphics);

            this.debounceTimer.AutoReset = false;
            this.debounceTimer.Elapsed += this.DebounceTimer_Elapsed;

            this.backgroundColor = Color.Blue;

            this.scoreBanner = new Banner(this.displayWidth, this.asyncGraphics, fontHeight: 16, this.backgroundColor, color: Color.Yellow, top: 0)
            {
                Text = Banner.SCORE_TEXT
            };

            this.instructionBanner = new Banner(
                displayWidth: this.displayWidth, 
                graphics: this.asyncGraphics, 
                fontHeight: 16, 
                backgroundColor: this.backgroundColor, 
                color: Color.White, 
                top: Banner.HEIGHT * 2);
            this.ShowInstructionBanner(Banner.START_TEXT);

            this.paddle = new Paddle(this.asyncGraphics, this.displayWidth, this.displayWidth, this.backgroundColor);

            this.scoreKeeper = new ScoreKeeper();
            this.scoreKeeper.ScoreChanged += this.scoreBanner.OnScoreChanged;

            this.ball = new Ball(
                asyncGraphics: this.asyncGraphics, 
                displayWidth: this.displayWidth, 
                displayHeight: this.displayHeight, 
                backgroundColor: this.backgroundColor, 
                paddle: this.paddle, 
                soundGenerator: this.soundGenerator, 
                minimumY: Banner.HEIGHT + 1,
                scoreKeeper: this.scoreKeeper);

            this.ball.ExplosionOccurred += this.OnExplosionOccurred;

            this.rotaryPaddle.Clicked += this.RotaryPaddle_Clicked;

            this.soundGenerator.PlayConstructionCompleteSound();
        }

        /// <summary>
        /// Wrapper function for writing lines of text to the console.
        /// This is used so that all debug output can be turned on or off
        /// by uncommenting or commenting a single line.  This could be enhanced
        /// to display only certain debug levels or debug message groups.
        /// </summary>
        /// <param name="s">Line of text to write to the console</param>
        public static void DebugWriteLine(string s)
        {
            // Console.WriteLine(s);
        }

        /// <summary>
        /// Display the instruction banner.
        /// </summary>
        /// <param name="text">Text to display in the instruction banner</param>
        public void ShowInstructionBanner(string text)
        {
            this.instructionBanner.Text = text;
            this.instructionBanner.Draw();
            this.asyncGraphics.ShowDirect();
        }

        /// <summary>
        /// Load the display screen content
        /// </summary>
        /// <param name="eraseInstructionBanner">Value indicating whether the instruction banner should be erased</param>
        /// <param name="instructionBannerText">Text to display in the instruction banner</param>
        /// <param name="showScoreBanner">Value indicating whether the score banner should be displayed</param>
        private void LoadScreen(bool eraseInstructionBanner, string instructionBannerText, bool showScoreBanner)
        {
            this.asyncGraphics.DrawRectangle(
                left: 0, 
                top: 0,
                width: this.displayWidth,
                height: this.displayHeight, 
                color: this.backgroundColor);

            if (eraseInstructionBanner)
            {
                this.instructionBanner.Hide();
            }
            else
            {
                this.ShowInstructionBanner(instructionBannerText);
            }

            if (showScoreBanner)
            {
                this.scoreBanner.Draw();
            }

            this.asyncGraphics.ShowDirect();
        }

        /// <summary>
        /// Handler called when the rotary encoder is rotated.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void RotaryPaddle_Rotated(object sender, Meadow.Peripherals.Sensors.Rotary.RotaryTurnedEventArgs e)
        {
            try
            {
                if (!this.isDebounceActive)
                {
                    this.isDebounceActive = true;
                    this.debounceTimer.Start();

                    if (e.Direction == RotationDirection.Clockwise)
                    {
                        ++this.directionCounter;

                        if (this.directionCounter > 0)
                        {
                            this.paddle.MoveLeft();
                            this.directionCounter = 1;
                        }
                    }
                    else
                    {
                        --this.directionCounter;

                        if (this.directionCounter < 0)
                        {
                            this.paddle.MoveRight();
                            this.directionCounter = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MeadowApp.DebugWriteLine($"RotaryPaddle_Rotated exception: {ex}");
            }
        }

        /// <summary>
        /// Handler called when rotary encoder debounce timer elapses.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void DebounceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.isDebounceActive = false;
        }

        /// <summary>
        /// Game-over event handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        private void OnExplosionOccurred(object sender, EventArgs args)
        {
            this.ShowInstructionBanner(Banner.RESTART_TEXT);
        }

        /// <summary>
        /// Handler called when the encoder knob is clicked (pressed and released).
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void RotaryPaddle_Clicked(object sender, EventArgs e)
        {
            ++this.rotaryPaddleClickCount;

            if (this.rotaryPaddleClickCount <= 1)
            {
                this.soundGenerator.PlayStartSound();
                MeadowApp.DebugWriteLine("Processing knob click");
                this.asyncGraphics.Stop();
                this.ball.StopMoving();
                this.asyncGraphics.Clear();
                this.LoadScreen(eraseInstructionBanner: true, string.Empty, showScoreBanner: true);
                this.paddle.Reset();
                this.ball.Reset();
                this.scoreKeeper.Reset();
                this.soundGenerator.PlayStartSound();
                this.ball.StartMoving();
                this.asyncGraphics.Start();
            }

            this.rotaryPaddleClickCount = 0;
        }
    }
}