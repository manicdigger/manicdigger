using System;
using ManicDigger;
using ManicDiggerServer;
using System.Threading;

namespace GameModeFortress
{
    public class ServerConsole
    {
        private Server server;

        public ServerConsole(Server server)
        {
            this.server = server;

            // run command line reader as seperate thread
            Thread consoleInterpreterThread = new Thread(new ThreadStart(this.CommandLineReader));
            consoleInterpreterThread.Start();
        }

        public void CommandLineReader()
        {
            string input = "";
            while(true)
            {
                input = Console.ReadLine();
                if(input == null)
                {
                    break;
                }
                input = input.Trim();
                server.ReceiveServerConsole(input);
            }
        }

        public void Receive(string message)
        {
            Console.WriteLine(message);
        }
    }
}

