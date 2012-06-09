using System;
using System.Collections;
using System.Collections.Generic;
using ManicDigger;
using ManicDiggerServer;
using ProtoBuf;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace GameModeFortress
{
    public class ServerConsole
    {
        private Server server;

        public ServerConsole(Server server)
        {
            this.server = server;
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

