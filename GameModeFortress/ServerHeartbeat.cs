using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameModeFortress
{
    public class ServerHeartbeat
    {
        public Server s;
        public Thread t;
        public Boolean running;

        public ServerHeartbeat(Server s)
        {
            this.s = s;
            this.t = new Thread(new ThreadStart(DoWork));
            this.running = false;
        }

        public void DoWork()
        {
            while (running && s != null)
            {
                s.SendHeartbeat();
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        public void Start()
        {
            try
            {
                this.running = true;
                t.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("The heartbeat thread ran into issues...");
            }
        }
    }
}
