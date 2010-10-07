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

        public static IGameClient SinglePlayer()
        {
            // Create the game
            ISinglePlayer game = new GMFortress();

            // Run the server thread, so we can connect
            new Thread(game.ServerThread).Start();

            return game;
        }
        public static IGameClient MultiPlayer(string[] args)
        {
            IOnlineGame game = null;

            // Read the mdlink file and parse it into the connection info
            ServerConnectInfo info = MDLinkReader.ReadMDLink(args[0]);

            // This is a temp variable for easy access
            string gamemode = info.gamemode.ToLower();

            // Create the game object based on the game mode from mdlink
            if (gamemode.Equals("fortress"))
            {
                // Create the game
                game = new GMFortress();
                // Assign the connection info
                game.connectinfo = info;
            }

            return game;
        }

        public static void RunGame(string[] args)
        {
            // This is the game object, determined by the mdlink from the launcher
            IGameClient game = null;

            // This is a single player game
            if (args.Length == 0)
            {
                game = SinglePlayer();
            }
            // This is a multiplayer game
            else
            {
                game = MultiPlayer(args);
            }

            // If the game is not null, start the game
            if (game != null)
            {
                // Start the game
                game.Start();
            }
        }
    }
}
