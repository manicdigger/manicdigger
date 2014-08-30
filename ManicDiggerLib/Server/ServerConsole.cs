using System;
using ManicDigger;
using System.Threading;

public class ServerConsole
{
    private Server server;
    public GameExit Exit;

    public ServerConsole(Server server, GameExit exit)
    {
        this.server = server;
        this.Exit = exit;

        // run command line reader as seperate thread
        Thread consoleInterpreterThread = new Thread(new ThreadStart(this.CommandLineReader));
        consoleInterpreterThread.Start();
    }

    public void CommandLineReader()
    {
        string input = "";
        while (!Exit.exit)
        {
            if (server.IsSinglePlayer)
            {
                Thread.Sleep(1000);
            }
            else
            {
                input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }
                input = input.Trim();
                server.ReceiveServerConsole(input);
            }
        }
    }

    public void Receive(string message)
    {
        Console.WriteLine(message);
    }
}


