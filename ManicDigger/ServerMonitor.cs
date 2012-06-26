using System;
using System.Collections;
using System.Collections.Generic;
using ManicDigger;
using ManicDiggerServer;
using System.Threading;

namespace GameModeFortress
{
    public class ServerMonitor
    {
        public int MaxPackets = 500; // max number of packets - packet flood protection
        public int MaxBlocks = 30; // max number of blocks which can be set within the time intervall
        public int MaxMessages = 3; // max number of chat messages per time intervall
        public int TimeIntervall = 3; // in seconds, resets count values
        public IGameExit Exit;
        public TimeSpan MessageBanTime = new TimeSpan(0, 1, 0);// 1 minute

        private Server server;
        private Dictionary<int, MonitorClient> monitorClients;

        public ServerMonitor(Server server, IGameExit exit)
        {
            this.server = server;
            this.Exit = exit;
            this.monitorClients = new Dictionary<int, MonitorClient>();
        }

        public void Start()
        {
            Thread serverMonitorThread = new Thread(new ThreadStart(this.Process));
            serverMonitorThread.Start();
        }

        private void Process()
        {
            while(!Exit.exit)
            {
                Thread.Sleep(TimeSpan.FromSeconds(TimeIntervall));
                foreach (var k in monitorClients)
                {
                    k.Value.BlocksSet = 0;
                    k.Value.MessagesSent = 0;
                    k.Value.PacketsReceived = 0;
                }
            }
        }

        public bool CheckPacket(int clientId, PacketClient packet)
        {
            if(!monitorClients.ContainsKey(clientId))
            {
                monitorClients.Add(clientId, new MonitorClient(){ Id = clientId});
            }

            monitorClients[clientId].PacketsReceived++;
            if (monitorClients[clientId].PacketsReceived > MaxPackets)
            {
                server.Kick(server.ServerConsoleId, clientId, "Packet Overflow");
                return false;
            }

            switch(packet.PacketId)
            {
                case ClientPacketId.SetBlock:
                case ClientPacketId.FillArea:
                    if (monitorClients[clientId].SetBlockPunished())
                    {
                        // TODO: revert block at client
                        return false;
                    }
                    if (monitorClients[clientId].BlocksSet < MaxBlocks)
                    {
                        monitorClients[clientId].BlocksSet++;
                        return true;
                    }
                    // punish client
                    return this.ActionSetBlock(clientId);
                case ClientPacketId.Message:
                    if (monitorClients[clientId].MessagePunished())
                    {
                        server.SendMessage(clientId, "Spam protection: Your message has not been sent.", Server.MessageType.Error);
                        return false;
                    }
                    if (monitorClients[clientId].MessagesSent < MaxMessages)
                    {
                        monitorClients[clientId].MessagesSent++;
                        return true;
                    }
                    // punish client
                    return this.ActionMessage(clientId);
                default:
                    return true;
            }


        }

        // Actions which will be taken when client exceeds a limit.
        private bool ActionSetBlock(int clientId)
        {
            this.monitorClients[clientId].SetBlockPunishment = new Punishment();//infinte duration
            this.server.ServerMessageToAll(string.Format("{0} exceeds set block limit.", server.GetClient(clientId).playername), Server.MessageType.Important);
            return false;
        }
        private bool ActionMessage(int clientId)
        {
            this.monitorClients[clientId].MessagePunishment = new Punishment(MessageBanTime);
            this.server.ServerMessageToAll(string.Format("Spam protection: {0} has been muted for {1} minutes.", server.GetClient(clientId).playername, MessageBanTime.TotalMinutes), Server.MessageType.Important);
            return false;
        }

        private class MonitorClient
        {
            public int Id = -1;
            public int PacketsReceived = 0;
            public int BlocksSet = 0;
            public int MessagesSent = 0;

            public Punishment SetBlockPunishment;
            public bool SetBlockPunished()
            {
                if (this.SetBlockPunishment == null)
                {
                    return false;
                }
                return this.SetBlockPunishment.Active();
            }

            public Punishment MessagePunishment;
            public bool MessagePunished()
            {
                if (this.MessagePunishment == null)
                {
                    return false;
                }
                return this.MessagePunishment.Active();
            }
        }

        private class Punishment
        {
            private DateTime punishmentStartDate;
            private bool permanent;
            private TimeSpan duration;

            public Punishment(TimeSpan duration)
            {
                this.punishmentStartDate = DateTime.UtcNow;
                this.duration = duration;
                this.permanent = false;
            }
            public Punishment()
            {
                this.punishmentStartDate = DateTime.UtcNow;
                this.duration = TimeSpan.MinValue;
                this.permanent = true;
            }
            public bool Active()
            {
                if (this.permanent)
                {
                    return true;
                }
                if (DateTime.UtcNow.Subtract(this.punishmentStartDate).CompareTo(duration) == -1)
                {
                    return true;
                }
                return false;
            }
        }
    }
}

