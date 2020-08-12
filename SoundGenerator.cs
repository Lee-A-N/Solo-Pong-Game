//-----------------------------------------------------------------------
//    SoundGenerator.cs
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
    using System.Threading;
    using Meadow.Foundation.Audio;
    using Meadow.Hardware;

    /// <summary>
    /// Class used for generating sound effects
    /// </summary>
    public class SoundGenerator : ISounds
    {
        /// <summary>
        /// Object representing the speaker hardware
        /// </summary>
        private readonly PiezoSpeaker speaker;

        /// <summary>
        /// Object representing the speaker's PWM port
        /// </summary>
        private readonly IPwmPort speakerPWM;

        /// <summary>
        /// First of two digital inputs ports used for reading the current sound level setting.
        /// </summary>
        private readonly IDigitalInputPort volumeIn1;

        /// <summary>
        /// Second of two digital inputs ports used for reading the current sound level setting.
        /// </summary>
        private readonly IDigitalInputPort volumeIn2;

        /// <summary>
        /// Initializes a new instance of the SoundGenerator class
        /// </summary>
        /// <param name="inputPort1">First sound-level input port</param>
        /// <param name="inputPort2">Second sound-level input port</param>
        /// <param name="pwmPort">PWM port used to drive the speaker</param>
        public SoundGenerator(IDigitalInputPort inputPort1, IDigitalInputPort inputPort2, IPwmPort pwmPort)
        {
            this.volumeIn1 = inputPort1;
            this.volumeIn2 = inputPort2;
            this.speakerPWM = pwmPort;

            this.speaker = new PiezoSpeaker(this.speakerPWM);

            this.PlayInitialSound();
        }

        /// <summary>
        /// Enumeration of sound-level settings
        /// </summary>
        private enum SoundMode
        {
            /// <summary>
            /// No sound effects
            /// </summary>
            Silent,

            /// <summary>
            /// Low-volume sound effects
            /// </summary>
            Soft,

            /// <summary>
            /// Normal-volume sound effects
            /// </summary>
            Normal
        }

        /// <summary>
        /// Gets the current sound level setting by reading the slide-switch state.
        /// </summary>
        private SoundMode SoundLevel
        {
            get
            {
                if (this.volumeIn1.State == true)
                {
                    return SoundMode.Normal;
                }
                else if (this.volumeIn2.State == true)
                {
                    return SoundMode.Silent;
                }
                else
                {
                    return SoundMode.Soft;
                }
            }
        }

        /// <summary>
        /// Gets the duty-cycle of the PWM signal based on the sound level setting.
        /// </summary>
        private int SoundDutyCycle
        {
            get
            {
                switch (this.SoundLevel)
                {
                    case SoundMode.Silent:
                        return 0;

                    case SoundMode.Soft:
                        return 1;

                    default:
                        return 20;
                }
            }
        }

        /// <summary>
        /// Play the left, right, or top border-hit sound effect.
        /// </summary>
        public void PlayBorderHitSound()
        {
            this.PlaySound(1500, 10);
        }

        /// <summary>
        /// Play the paddle-hit sound effect
        /// </summary>
        public void PlayPaddleHitSound()
        {
            this.PlaySound(1300, 10);
        }

        /// <summary>
        /// Play the game-over sound effect
        /// </summary>
        public void PlayGameOverSound()
        {
            try
            {
                // A thread is needed to get the sound to play
                new Thread(() =>
                {
                    this.PlaySound(50, 500);
                }).Start();
            }
            catch (Exception)
            {
                // Sometimes thread creation fails. Make sure it doesn't crash the game.
                MeadowApp.DebugWriteLine($"Exception in PlayGameOverSound");
            }
        }

        /// <summary>
        /// Play the start-up sound effect
        /// </summary>
        public void PlayStartSound()
        {
            this.PlaySound(2000, 2);
            Thread.Sleep(10);
            this.PlaySound(2000, 2);
        }

        /// <summary>
        /// Play the initial sound effect
        /// </summary>
        public void PlayInitialSound()
        {
            new Thread(() =>
            {
                this.PlaySound(1.1f, 1);
            }).Start();
        }

        /// <summary>
        /// Play the construction-completed sound effect
        /// </summary>
        public void PlayConstructionCompleteSound()
        {
            new Thread(() =>
            {
                this.PlaySound(1200, 1);
                this.PlaySound(1200, 1);
            }).Start();
        }

        /// <summary>
        /// Gets the sound duration based on the sound level setting
        /// </summary>
        /// <param name="normalDuration">Sound duration if the sound level is set to normal</param>
        /// <returns>Sound duration to use</returns>
        private int GetSoundDuration(int normalDuration)
        {
            switch (this.SoundLevel)
            {
                case SoundMode.Silent:
                    return 0;

                case SoundMode.Soft:
                    return normalDuration == 0 ? 0 : 1;

                default:
                    return normalDuration;
            }
        }
        
        /// <summary>
        /// Play a sound-effect
        /// </summary>
        /// <param name="frequency">Sound-effect frequency</param>
        /// <param name="duration">Sound-effect duration</param>
        private void PlaySound(float frequency, int duration)
        {
            this.speakerPWM.DutyCycle = this.SoundDutyCycle;

            if (this.SoundDutyCycle > 0)
            {
                this.speaker.PlayTone(frequency, this.GetSoundDuration(duration));
            }
        }
    }
}
