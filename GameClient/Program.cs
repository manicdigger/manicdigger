using System;
using System.Collections.Generic;
using System.Text;
using GameModeFortress;
using ManicDigger;
using Utilities;

namespace GameClient
{
    class Program
    {
        static void Main(string[] args)
        {
            new CrashReporter("", "ManicDiggerCrash.log").Start(RunGame, args);
        }

        public static void RunGame(string[] args)
        {
            // Exit now if no arguments were passed
            if (args.Length == 0)
            {
                return;
            }

            // This is the game object, determined by the mdlink from the launcher
            IInternetGameFactory game = null;

            // Read in the connection info
            ServerConnectInfo info = MDLinkReader.ReadMDLink(args[0]);

            // This is a temp variable for easy access
            string gamemode = info.gamemode.ToLower();

            // Create the game object based on the game mode from mdlink
            if (gamemode.Equals("fortress"))
            {
                game = new ManicDiggerProgram2();
            }

            // If the game is not null, start the game
            if (game != null)
            {
                // Assign the connect info
                game.connectinfo = info;

                // Start the game
                game.Start();
            }
        }
    }
}
