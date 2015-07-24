public class QueryClient
{
    internal QueryResult result;
    internal bool queryPerformed;
    internal bool querySuccess;
    internal GamePlatform p;
    internal string serverMessage;
    
    public QueryClient()
    {
        result = new QueryResult();
        querySuccess = false;
        queryPerformed = false;
    }
    
    public void PerformQuery(string ip, int port)
    {
        serverMessage = "";
        NetClient client;
        if (p.EnetAvailable())
        {
            //Create enet client
            EnetNetClient c = new EnetNetClient();
            c.SetPlatform(p);
            client = c;
        }
        else
        {
            //Create TCP client
            TcpNetClient c = new TcpNetClient();
            c.SetPlatform(p);
            client = c;
        }
        //Initialize client
        client.Start();
        client.Connect(ip, port);
        
        //Do network stuff
        SendRequest(client);
        ReadPacket(client);
        
        queryPerformed = true;
    }
    
    void SendRequest(NetClient client)
    {
        //Create request packet
        Packet_Client pp = ClientPackets.ServerQuery();
        
        //Serialize packet
        CitoMemoryStream ms = new CitoMemoryStream();
        Packet_ClientSerializer.Serialize(ms, pp);
        byte[] data = ms.ToArray();
        
        //Send packet to server
        INetOutgoingMessage msg = new INetOutgoingMessage();
        msg.Write(data, ms.Length());
        client.SendMessage(msg, MyNetDeliveryMethod.ReliableOrdered);
    }
    
    void ReadPacket(NetClient client)
    {
        bool success = false;
        int started = p.TimeMillisecondsFromStart();
        int timeout = 2000;
        while (p.TimeMillisecondsFromStart() < started + timeout)
        {
            if (success)
            {
                //Return if already received answer from server
                querySuccess = true;
                return;
            }
            NetIncomingMessage msg;
            msg = client.ReadMessage();
            if (msg == null)
            {
                //Message empty - skip processing.
                continue;
            }
            
            Packet_Server packet = new Packet_Server();
            Packet_ServerSerializer.DeserializeBuffer(msg.message, msg.messageLength, packet);
            
            switch (packet.Id)
            {
                case Packet_ServerIdEnum.QueryAnswer:
                    //Got answer from server. Process it.
                    result.Name = packet.QueryAnswer.Name;
                    result.MOTD = packet.QueryAnswer.MOTD;
                    result.PlayerCount = packet.QueryAnswer.PlayerCount;
                    result.MaxPlayers = packet.QueryAnswer.MaxPlayers;
                    result.PlayerList = packet.QueryAnswer.PlayerList;
                    result.Port = packet.QueryAnswer.Port;
                    result.GameMode = packet.QueryAnswer.GameMode;
                    result.Password = packet.QueryAnswer.Password;
                    result.PublicHash = packet.QueryAnswer.PublicHash;
                    result.ServerVersion = packet.QueryAnswer.ServerVersion;
                    result.MapSizeX = packet.QueryAnswer.MapSizeX;
                    result.MapSizeY = packet.QueryAnswer.MapSizeY;
                    result.MapSizeZ = packet.QueryAnswer.MapSizeZ;
                    result.ServerThumbnail = packet.QueryAnswer.ServerThumbnail;
                    success = true;
                    continue;
                    
                case Packet_ServerIdEnum.DisconnectPlayer:
                    serverMessage = packet.DisconnectPlayer.DisconnectReason;
                    //End method as server kicked us out
                    return;
                    
                default:
                    //Drop all other packets sent by server (not relevant)
                    continue;
            }
        }
        //Set timeout message if query did not finish in time
        serverMessage = "Timeout while querying server!";
    }
    
    public void SetPlatform(GamePlatform p_)
    {
        p = p_;
    }
    
    public QueryResult GetResult()
    {
        return result;
    }
    
    public string GetServerMessage()
    {
        return serverMessage;
    }
}

public class QueryResult
{
    internal string Name;
    internal string MOTD;
    internal int PlayerCount;
    internal int MaxPlayers;
    internal string PlayerList;
    internal int Port;
    internal string GameMode;
    internal bool Password;
    internal string PublicHash;
    internal string ServerVersion;
    internal int MapSizeX;
    internal int MapSizeY;
    internal int MapSizeZ;
    internal byte[] ServerThumbnail;
}