//-----------------------------------------------------------------------
//     Author: Lee Nayes, Logikos, Inc. All rights reserved.
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
