//-----------------------------------------------------------------------
//    Program.cs
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
    using System.Threading;
    using Meadow;

    /// <summary>
    /// Program class for the Solo Pong game
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Gets or sets the application interface
        /// </summary>
        public static IApp App { get; set; }

        /// <summary>
        /// Function called when the app is started
        /// </summary>
        /// <param name="args">Start-up arguments</param>
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--exitOnDebug")
            {
                return;
            }

            // instantiate and run new meadow app
            Program.App = new MeadowApp();

            // Keep the application running
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
