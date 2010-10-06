using System;
using System.Collections.Generic;
using System.Text;
using GameModeFortress;
using ManicDigger;
using Utilities;
using System.Threading;

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
            // This is the game object, determined by the mdlink from the launcher
            IInternetGameFactory game = null;

            // Read in the connection info
            ServerConnectInfo info = null;

            // This is a single player game
            if (args.Length == 0)
            {
                info = new ServerConnectInfo();
                info.url = "127.0.0.1";
                info.port = 25570;
                info.username = "Local";
                game = new ManicDiggerProgram2();

                new Thread(game.ServerThread).Start();
            }
            // This is a multiplayer game
            else
            {
                // Read the mdlink file and parse it into the connection info
                info = MDLinkReader.ReadMDLink(args[0]);

                // This is a temp variable for easy access
                string gamemode = info.gamemode.ToLower();

                // Create the game object based on the game mode from mdlink
                if (gamemode.Equals("fortress"))
                {
                    game = new ManicDiggerProgram2();
                }
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
