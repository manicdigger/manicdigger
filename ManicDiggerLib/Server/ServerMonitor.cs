using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using ManicDigger;
using System.Threading;
using ManicDigger.ClientNative;

public class ServerMonitor
{
    private ServerMonitorConfig config;
    public GameExit Exit;
    private Server server;
    private Dictionary<int, MonitorClient> monitorClients;

    public ServerMonitor(Server server, GameExit exit)
    {
        this.server = server;
        this.LoadConfig();
        this.Exit = exit;
        this.monitorClients = new Dictionary<int, MonitorClient>();
    }

    public bool RemoveMonitorClient(int clientid)
    {
        return this.monitorClients.Remove(clientid);
    }

    public void Start()
    {
        Thread serverMonitorThread = new Thread(new ThreadStart(this.Process));
        serverMonitorThread.Start();
    }

    private void Process()
    {
        while (!Exit.exit)
        {
            Thread.Sleep(TimeSpan.FromSeconds(config.TimeIntervall));
            foreach (var k in monitorClients)
            {
                k.Value.BlocksSet = 0;
                k.Value.MessagesSent = 0;
                k.Value.PacketsReceived = 0;
            }
        }
    }

    public bool CheckPacket(int clientId, Packet_Client packet)
    {
        if (!monitorClients.ContainsKey(clientId))
        {
            monitorClients.Add(clientId, new MonitorClient() { Id = clientId });
        }

        monitorClients[clientId].PacketsReceived++;
        if (monitorClients[clientId].PacketsReceived > config.MaxPackets)
        {
            server.Kick(server.ServerConsoleId, clientId, "Packet Overflow");
            return false;
        }

        switch (packet.Id)
        {
            case Packet_ClientIdEnum.SetBlock:
            case Packet_ClientIdEnum.FillArea:
                if (monitorClients[clientId].SetBlockPunished())
                {
                    // TODO: revert block at client
                    return false;
                }
                if (monitorClients[clientId].BlocksSet < config.MaxBlocks)
                {
                    monitorClients[clientId].BlocksSet++;
                    return true;
                }
                // punish client
                return this.ActionSetBlock(clientId);
            case Packet_ClientIdEnum.Message:
                if (monitorClients[clientId].MessagePunished())
                {
                    server.SendMessage(clientId, server.language.ServerMonitorChatNotSent(), Server.MessageType.Error);
                    return false;
                }
                if (monitorClients[clientId].MessagesSent < config.MaxMessages)
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
        this.server.ServerMessageToAll(string.Format(server.language.ServerMonitorBuildingDisabled(), server.GetClient(clientId).playername), Server.MessageType.Important);
        return false;
    }
    private bool ActionMessage(int clientId)
    {
        this.monitorClients[clientId].MessagePunishment = new Punishment(new TimeSpan(0, 0, config.MessageBanTime));
        this.server.ServerMessageToAll(string.Format(server.language.ServerMonitorChatMuted(), server.GetClient(clientId).playername, config.MessageBanTime), Server.MessageType.Important);
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


    public class ServerMonitorConfig
    {
        public int MaxPackets; // max number of packets - packet flood protection
        public int MaxBlocks; // max number of blocks which can be set within the time intervall
        public int MaxMessages; // max number of chat messages per time intervall
        public int MessageBanTime;// how long gets a player muted (in seconds)
        public int TimeIntervall; // in seconds, resets count values

        public ServerMonitorConfig()
        {
            //Set Defaults
            this.MaxPackets = 500;
            this.MaxBlocks = 50;
            this.MaxMessages = 3;
            this.MessageBanTime = 60;
            this.TimeIntervall = 3;
        }
    }
    string filename = "ServerMonitor.txt";
    private void LoadConfig()
    {
        if (!File.Exists(Path.Combine(GameStorePath.gamepathconfig, filename)))
        {
            Console.WriteLine(server.language.ServerMonitorConfigNotFound());
            SaveConfig();
        }
        else
        {
            try
            {
                using (TextReader textReader = new StreamReader(Path.Combine(GameStorePath.gamepathconfig, filename)))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(ServerMonitorConfig));
                    this.config = (ServerMonitorConfig)deserializer.Deserialize(textReader);
                    textReader.Close();
                    SaveConfig();
                }
            }
            catch //This if for the original format
            {
                using (Stream s = new MemoryStream(File.ReadAllBytes(Path.Combine(GameStorePath.gamepathconfig, filename))))
                {
                    this.config = new ServerMonitorConfig();
                    StreamReader sr = new StreamReader(s);
                    XmlDocument d = new XmlDocument();
                    d.Load(sr);
                }
                //Save with new version.
                SaveConfig();
            }
        }
        Console.WriteLine(server.language.ServerMonitorConfigLoaded());
    }
    public void SaveConfig()
    {
        //Verify that we have a directory to place the file into.
        if (!Directory.Exists(GameStorePath.gamepathconfig))
        {
            Directory.CreateDirectory(GameStorePath.gamepathconfig);
        }

        XmlSerializer serializer = new XmlSerializer(typeof(ServerMonitorConfig));
        TextWriter textWriter = new StreamWriter(Path.Combine(GameStorePath.gamepathconfig, filename));

        //Check to see if config has been initialized.
        if (this.config == null)
        {
            this.config = new ServerMonitorConfig();
        }
        //Serialize the config class to XML.
        serializer.Serialize(textWriter, this.config);
        textWriter.Close();
    }
}
