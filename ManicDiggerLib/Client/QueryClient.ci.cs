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
        //p = new GamePlatformNative();
    }
    
    public void PerformQuery(string ip, int port)
    {
        serverMessage = "";
        if (p.EnetAvailable())
        {
            //Create enet client
            EnetNetClient client = new EnetNetClient();
            client.SetPlatform(p);
            
            //Initialize client
            client.Start();
            client.Connect(ip, port);
            
            //Do network stuff
            SendRequest(client);
            ReadPacket(client);
        }
        else
        {
            p.ThrowException("Network not implemented");
        }
        queryPerformed = true;
    }
    
    void SendRequest(EnetNetClient client)
    {
        //Create request packet
        Packet_ClientServerQuery p1 = new Packet_ClientServerQuery();
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.ServerQuery;
        pp.Query = p1;
            
        //Serialize packet
        CitoMemoryStream ms = new CitoMemoryStream();
        Packet_ClientSerializer.Serialize(ms, pp);
        byte[] data = ms.ToArray();
            
        //Send packet to server
        INetOutgoingMessage msg = client.CreateMessage();
        msg.Write(data, ms.Length());
        client.SendMessage(msg, MyNetDeliveryMethod.ReliableOrdered);
    }
    
    void ReadPacket(EnetNetClient client)
    {
        bool success = false;
        int started = p.TimeMillisecondsFromStart();
        int timeout = 1000;
        while (p.TimeMillisecondsFromStart() < started + timeout)
        {
            if (success)
            {
                //Return if already received answer from server
                querySuccess = true;
                return;
            }
            INetIncomingMessage msg;
            msg = client.ReadMessage();
            if (msg == null)
            {
                //Message empty - skip processing.
                continue;
            }
            
            Packet_Server packet = new Packet_Server();
            Packet_ServerSerializer.DeserializeBuffer(msg.ReadBytes(msg.LengthBytes()), msg.LengthBytes(), packet);
            
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
        serverMessage = "&4No message received from server!";
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