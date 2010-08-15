using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using OpenTK;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace ManicDigger
{
    public class NetworkClientMinecraft : INetworkClient
    {
        [Inject]
        public IMap Map { get; set; }
        [Inject]
        public IClients Clients { get; set; }
        [Inject]
        public IGui Chatlines { get; set; }
        [Inject]
        public ILocalPlayerPosition Position { get; set; }
        [Inject]
        public IGameWorldTodo gameworld { get; set; }
        public event EventHandler<MapLoadedEventArgs> MapLoaded;
        public bool ENABLE_FORTRESS = false;
        public void Connect(string serverAddress, int port, string username, string auth)
        {
            main = new Socket(AddressFamily.InterNetwork,
                   SocketType.Stream, ProtocolType.Tcp);

            iep = new IPEndPoint(IPAddress.Any, port);
            main.Connect(serverAddress, port);
            byte[] n = CreateLoginPacket(username, auth);
            main.Send(n);
        }
        private static byte[] CreateLoginPacket(string username, string verificationKey)
        {
            MemoryStream n = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(n);
            bw.Write((byte)0);//Packet ID 
            bw.Write((byte)0x07);//Protocol version
            bw.Write(NetworkHelper.StringToBytes(username));//Username
            bw.Write(NetworkHelper.StringToBytes(verificationKey));//Verification key
            bw.Write((byte)0);//Unused
            return n.ToArray();
        }
        IPEndPoint iep;
        Socket main;
        public void SendPacket(byte[] packet)
        {
            int sent = main.Send(packet);
            if (sent != packet.Length)
            {
                throw new Exception();
            }
        }
        public void Disconnect()
        {
            ChatLog("---Disconnected---");
            main.Disconnect(false);
        }
        DateTime lastpositionsent;
        public void SendSetBlock(Vector3 position, BlockSetMode mode, int type)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftClientPacketId.SetBlock);
            if (ENABLE_FORTRESS)
            {
                throw new Exception();
            }
            else
            {
                NetworkHelper.WriteInt16(bw, (short)(position.X));//-4
                NetworkHelper.WriteInt16(bw, (short)(position.Z));
                NetworkHelper.WriteInt16(bw, (short)position.Y);
            }
            bw.Write((byte)(mode == BlockSetMode.Create ? 1 : 0));
            bw.Write((byte)type);
            SendPacket(ms.ToArray());
            //tosend.Add(ms.ToArray());
            //Console.WriteLine(this.position.LocalPlayerPosition);
            //Console.WriteLine("p" + position);
            //Console.WriteLine("player:" + lastsentposition + ", build:" + position
            //    + ", block:" + type + ", mode:" + Enum.GetName(typeof(BlockSetMode), mode));
        }
        public void SendChat(string s)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftClientPacketId.Message);
            bw.Write((byte)255);//unused
            NetworkHelper.WriteString64(bw, s);
            SendPacket(ms.ToArray());
            //tosend.Add(ms.ToArray());
        }
        //List<byte[]> tosend = new List<byte[]>();
        /// <summary>
        /// This function should be called in program main loop.
        /// It exits immediately.
        /// </summary>
        public void Process()
        {
            if (main == null)
            {
                return;
            }
            for (; ; )
            {
                if (!main.Poll(0, SelectMode.SelectRead))
                {
                    break;
                }
                byte[] data = new byte[1024];
                int recv;
                try
                {
                    recv = main.Receive(data);
                }
                catch
                {
                    recv = 0;
                }
                if (recv == 0)
                {
                    //disconnected
                    return;
                }
                for (int i = 0; i < recv; i++)
                {
                    received.Add(data[i]);
                }
                for (; ; )
                {
                    if (received.Count < 4)
                    {
                        break;
                    }
                    byte[] packet = new byte[received.Count];
                    int bytesRead;
                    bytesRead = TryReadPacket();
                    if (bytesRead > 0)
                    {
                        received.RemoveRange(0, bytesRead);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (spawned && ((DateTime.Now - lastpositionsent).TotalSeconds > 0.1))
            {
                lastpositionsent = DateTime.Now;
                SendPosition(Position.LocalPlayerPosition, Position.LocalPlayerOrientation);
                //foreach (byte[] b in tosend)
                //{
                //    Console.WriteLine("qp" + position.LocalPlayerPosition);
                //    SendPacket(b);
                //}
                //tosend.Clear();
            }
        }
        public int mapreceivedsizex;
        public int mapreceivedsizey;
        public int mapreceivedsizez;
        Vector3 lastsentposition;
        public void SendPosition(Vector3 position, Vector3 orientation)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftClientPacketId.PositionandOrientation);
            bw.Write((byte)255);//player id, self
            if (ENABLE_FORTRESS)
            {
                NetworkHelper.WriteInt32(bw, (int)((position.X) * 32));//gfd1
                NetworkHelper.WriteInt32(bw, (int)((position.Y + CharacterPhysics.characterheight) * 32));
                NetworkHelper.WriteInt32(bw, (int)(position.Z * 32));
            }
            else
            {
                NetworkHelper.WriteInt16(bw, (short)((position.X) * 32));//gfd1
                NetworkHelper.WriteInt16(bw, (short)((position.Y + CharacterPhysics.characterheight) * 32));
                NetworkHelper.WriteInt16(bw, (short)(position.Z * 32));
            }
            bw.Write(NetworkHelper.HeadingByte(orientation));
            bw.Write(NetworkHelper.PitchByte(orientation));
            SendPacket(ms.ToArray());
            lastsentposition = position;
        }
        bool spawned = false;
        string serverName = "";
        string serverMotd = "";
        public string ServerName { get { return serverName; } set { serverName = value; } }
        public string ServerMotd { get { return serverMotd; } set { serverMotd = value; } }
        public int LocalPlayerId = 255;
        private int TryReadPacket()
        {
            BinaryReader br = new BinaryReader(new MemoryStream(received.ToArray()));
            if (received.Count == 0)
            {
                return 0;
            }
            var packetId = (MinecraftServerPacketId)br.ReadByte();
            int totalread = 1;
            if (packetId != MinecraftServerPacketId.PositionandOrientationUpdate
                 && packetId != MinecraftServerPacketId.PositionUpdate
                && packetId != MinecraftServerPacketId.OrientationUpdate
                && packetId != MinecraftServerPacketId.PlayerTeleport
                && packetId != MinecraftServerPacketId.ExtendedPacketTick)
            {
                Console.WriteLine(Enum.GetName(typeof(MinecraftServerPacketId), packetId));
            }
            switch (packetId)
            {
                case MinecraftServerPacketId.ServerIdentification:
                    {
                        totalread += 1 + NetworkHelper.StringLength + NetworkHelper.StringLength + 1; if (received.Count < totalread) { return 0; }
                        if (ENABLE_FORTRESS)
                        {
                            totalread += NetworkHelper.StringLength; if (received.Count < totalread) { return 0; }
                        }
                        ServerPlayerIdentification p = new ServerPlayerIdentification();
                        p.ProtocolVersion = br.ReadByte();
                        string invalidversionstr = "Invalid game version. Local: {0}, Server: {1}";
                        if (!ENABLE_FORTRESS)
                        {
                            if (!(p.ProtocolVersion == 7 || p.ProtocolVersion == 6))
                            {
                                throw new Exception(string.Format(invalidversionstr,
                                    "Minecraft 7", "Minecraft " + p.ProtocolVersion));
                            }
                        }
                        else
                        {
                            string servergameversion = NetworkHelper.ReadString64(br);
                            if (p.ProtocolVersion != 200)
                            {
                                servergameversion = "Minecraft " + p.ProtocolVersion;
                            }
                            if (servergameversion != GameVersion.Version)
                            {
                                throw new Exception(string.Format(invalidversionstr, GameVersion.Version, servergameversion));
                            }
                        }
                        p.ServerName = NetworkHelper.ReadString64(br);
                        p.ServerMotd = NetworkHelper.ReadString64(br);
                        p.UserType = br.ReadByte();
                        //connected = true;
                        this.serverName = p.ServerName;
                        this.ServerMotd = p.ServerMotd;
                        ChatLog("---Connected---");
                    }
                    break;
                case MinecraftServerPacketId.Ping:
                    {
                    }
                    break;
                case MinecraftServerPacketId.LevelInitialize:
                    {
                        receivedMapStream = new MemoryStream();
                        InvokeMapLoadingProgress(0, 0);
                    }
                    break;
                case MinecraftServerPacketId.LevelDataChunk:
                    {
                        totalread += 2 + 1024 + 1; if (received.Count < totalread) { return 0; }
                        int chunkLength = NetworkHelper.ReadInt16(br);
                        byte[] chunkData = br.ReadBytes(1024);
                        BinaryWriter bw1 = new BinaryWriter(receivedMapStream);
                        byte[] chunkDataWithoutPadding = new byte[chunkLength];
                        for (int i = 0; i < chunkLength; i++)
                        {
                            chunkDataWithoutPadding[i] = chunkData[i];
                        }
                        bw1.Write(chunkDataWithoutPadding);
                        MapLoadingPercentComplete = br.ReadByte();
                        InvokeMapLoadingProgress(MapLoadingPercentComplete, (int)receivedMapStream.Length);
                    }
                    break;
                case MinecraftServerPacketId.LevelFinalize:
                    {
                        totalread += 2 + 2 + 2; if (received.Count < totalread) { return 0; }
                        if (ENABLE_FORTRESS)
                        {
                            totalread += 4; if (received.Count < totalread) { return 0; } //simulationstartframe
                        }
                        mapreceivedsizex = NetworkHelper.ReadInt16(br);
                        mapreceivedsizez = NetworkHelper.ReadInt16(br);
                        mapreceivedsizey = NetworkHelper.ReadInt16(br);
                        receivedMapStream.Seek(0, SeekOrigin.Begin);
                        if (!ENABLE_FORTRESS)
                        {
                            MemoryStream decompressed = new MemoryStream(GzipCompression.Decompress(receivedMapStream.ToArray()));
                            if (decompressed.Length != mapreceivedsizex * mapreceivedsizey * mapreceivedsizez +
                                (decompressed.Length % 1024))
                            {
                                //throw new Exception();
                                Console.WriteLine("warning: invalid map data size");
                            }
                            byte[, ,] receivedmap = new byte[mapreceivedsizex, mapreceivedsizey, mapreceivedsizez];
                            {
                                BinaryReader br2 = new BinaryReader(decompressed);
                                int size = NetworkHelper.ReadInt32(br2);
                                for (int z = 0; z < mapreceivedsizez; z++)
                                {
                                    for (int y = 0; y < mapreceivedsizey; y++)
                                    {
                                        for (int x = 0; x < mapreceivedsizex; x++)
                                        {
                                            receivedmap[x, y, z] = br2.ReadByte();
                                        }
                                    }
                                }
                            }
                            Map.Map.UseMap(receivedmap);
                            Map.Map.MapSizeX = receivedmap.GetUpperBound(0) + 1;
                            Map.Map.MapSizeY = receivedmap.GetUpperBound(1) + 1;
                            Map.Map.MapSizeZ = receivedmap.GetUpperBound(2) + 1;
                            Console.WriteLine("Game loaded successfully.");
                        }
                        else
                        {
                            int simulationstartframe = NetworkHelper.ReadInt32(br);
                            gameworld.LoadState(receivedMapStream.ToArray(), simulationstartframe);
                        }
                        if (MapLoaded != null)
                        {
                            MapLoaded.Invoke(this, new MapLoadedEventArgs() { });
                        }
                        loadedtime = DateTime.Now;
                    }
                    break;
                case MinecraftServerPacketId.SetBlock:
                    {
                        int x;
                        int y;
                        int z;
                        if (ENABLE_FORTRESS)
                        {
                            throw new Exception("SetBlock packet");//no such packet.
                        }
                        else
                        {
                            totalread += 2 + 2 + 2 + 1; if (received.Count < totalread) { return 0; }
                            x = NetworkHelper.ReadInt16(br);
                            z = NetworkHelper.ReadInt16(br);
                            y = NetworkHelper.ReadInt16(br);
                        }
                        byte type = br.ReadByte();
                        try { Map.SetTileAndUpdate(new Vector3(x, y, z), type); }
                        catch { Console.WriteLine("Cannot update tile!"); }
                    }
                    break;
                case MinecraftServerPacketId.SpawnPlayer:
                    {
                        if (ENABLE_FORTRESS)
                        {
                            totalread += 1 + NetworkHelper.StringLength + 4 + 4 + 4 + 1 + 1; if (received.Count < totalread) { return 0; }
                        }
                        else
                        {
                            totalread += 1 + NetworkHelper.StringLength + 2 + 2 + 2 + 1 + 1; if (received.Count < totalread) { return 0; }
                        }
                        byte playerid = br.ReadByte();
                        string playername = NetworkHelper.ReadString64(br);
                        if (ENABLE_FORTRESS && playerid == 255)
                        {
                            spawned = true;
                            break;
                        }
                        connectedplayers.Add(new ConnectedPlayer() { name = playername, id = playerid });
                        if (Clients.Players.ContainsKey(playerid))
                        {
                            //throw new Exception();
                        }
                        Clients.Players[playerid] = new Player();
                        Clients.Players[playerid].Name = playername;
                        if (ENABLE_FORTRESS && ((DateTime.Now - loadedtime).TotalSeconds > 10))
                        {
                            ReadAndUpdatePlayerPosition(br, playerid);
                        }
                        if (!ENABLE_FORTRESS)
                        {
                            ReadAndUpdatePlayerPosition(br, playerid);
                        }
                        if (playerid == 255)
                        {
                            spawned = true;
                        }
                    }
                    break;
                case MinecraftServerPacketId.PlayerTeleport:
                    {
                        if (ENABLE_FORTRESS)
                        {
                            totalread += 1 + (4 + 4 + 4) + 1 + 1; if (received.Count < totalread) { return 0; }
                        }
                        else
                        {
                            totalread += 1 + (2 + 2 + 2) + 1 + 1; if (received.Count < totalread) { return 0; }
                        }
                        byte playerid = br.ReadByte();
                        ReadAndUpdatePlayerPosition(br, playerid);
                    }
                    break;
                case MinecraftServerPacketId.PositionandOrientationUpdate:
                    {
                        totalread += 1 + (1 + 1 + 1) + 1 + 1; if (received.Count < totalread) { return 0; }
                        byte playerid = br.ReadByte();
                        float x = (float)br.ReadSByte() / 32;
                        float y = (float)br.ReadSByte() / 32;
                        float z = (float)br.ReadSByte() / 32;
                        byte heading = br.ReadByte();
                        byte pitch = br.ReadByte();
                        Vector3 v = new Vector3(x, y, z);
                        UpdatePositionDiff(playerid, v);
                    }
                    break;
                case MinecraftServerPacketId.PositionUpdate:
                    {
                        totalread += 1 + 1 + 1 + 1; if (received.Count < totalread) { return 0; }
                        byte playerid = br.ReadByte();
                        float x = (float)br.ReadSByte() / 32;
                        float y = (float)br.ReadSByte() / 32;
                        float z = (float)br.ReadSByte() / 32;
                        Vector3 v = new Vector3(x, y, z);
                        UpdatePositionDiff(playerid, v);
                    }
                    break;
                case MinecraftServerPacketId.OrientationUpdate:
                    {
                        totalread += 1 + 1 + 1; if (received.Count < totalread) { return 0; }
                        byte playerid = br.ReadByte();
                        byte heading = br.ReadByte();
                        byte pitch = br.ReadByte();
                        Clients.Players[playerid].Heading = heading;
                        Clients.Players[playerid].Pitch = pitch;
                    }
                    break;
                case MinecraftServerPacketId.DespawnPlayer:
                    {
                        totalread += 1; if (received.Count < totalread) { return 0; }
                        byte playerid = br.ReadByte();
                        for (int i = 0; i < connectedplayers.Count; i++)
                        {
                            if (connectedplayers[i].id == playerid)
                            {
                                connectedplayers.RemoveAt(i);
                            }
                        }
                        Clients.Players.Remove(playerid);
                    }
                    break;
                case MinecraftServerPacketId.Message:
                    {
                        totalread += 1 + NetworkHelper.StringLength; if (received.Count < totalread) { return 0; }
                        byte unused = br.ReadByte();
                        string message = NetworkHelper.ReadString64(br);
                        Chatlines.AddChatline(message);
                        ChatLog(message);
                    }
                    break;
                case MinecraftServerPacketId.DisconnectPlayer:
                    {
                        totalread += NetworkHelper.StringLength; if (received.Count < totalread) { return 0; }
                        string disconnectReason = NetworkHelper.ReadString64(br);
                        throw new Exception(disconnectReason);
                    }
                case MinecraftServerPacketId.ExtendedPacketCommand:
                    {
                        totalread += 1 + 4 + 4; if (received.Count < totalread) { return 0; }
                        int playerid = br.ReadByte();
                        int cmdframe = NetworkHelper.ReadInt32(br);
                        int length = NetworkHelper.ReadInt32(br);
                        totalread += length; if (received.Count < totalread) { return 0; }
                        byte[] cmd = br.ReadBytes(length);
                        gameworld.EnqueueCommand(playerid, cmdframe, cmd);
                    }
                    break;
                case MinecraftServerPacketId.ExtendedPacketTick:
                    {
                        totalread += 4 + 4; if (received.Count < totalread) { return 0; }
                        int allowedframe = NetworkHelper.ReadInt32(br);
                        int hash = NetworkHelper.ReadInt32(br);

                        totalread += 1; if (received.Count < totalread) { return 0; }
                        int clientscount = br.ReadByte();
                        totalread += clientscount * (1 + (3 * 4) + 1 + 1); if (received.Count < totalread) { return 0; }
                        Dictionary<int, PlayerPosition> playerpositions = new Dictionary<int, PlayerPosition>();
                        for (int i = 0; i < clientscount; i++)
                        {
                            byte playerid = br.ReadByte();
                            //copied
                            float x = (float)((double)NetworkHelper.ReadInt32(br) / 32);
                            float y = (float)((double)NetworkHelper.ReadInt32(br) / 32);
                            float z = (float)((double)NetworkHelper.ReadInt32(br) / 32);
                            byte heading = br.ReadByte();
                            byte pitch = br.ReadByte();
                            playerpositions[playerid] = new PlayerPosition() { position = new Vector3(x, y, z), heading = heading, pitch = pitch };
                        }
                        gameworld.KeyFrame(allowedframe, hash, playerpositions);
                    }
                    break;
                default:
                    {
                        throw new Exception("Invalid packet id");
                    }
            }
            return totalread;
        }
        DateTime loadedtime;
        private void InvokeMapLoadingProgress(int progressPercent, int progressBytes)
        {
            if (MapLoadingProgress != null)
            {
                MapLoadingProgress(this, new MapLoadingProgressEventArgs()
                {
                    ProgressPercent = progressPercent,
                    ProgressBytes = progressBytes
                });
            }
        }
        public bool ENABLE_CHATLOG = true;
        private void ChatLog(string p)
        {
            if (!ENABLE_CHATLOG)
            {
                return;
            }
            string logsdir = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ManicDiggerLogs");
            if (!Directory.Exists(logsdir))
            {
                Directory.CreateDirectory(logsdir);
            }
            string filename = Path.Combine(logsdir, MakeValidFileName(serverName) + ".txt");
            try
            {
                File.AppendAllText(filename, string.Format("{0} {1}\n", DateTime.Now, p));
            }
            catch
            {
                Console.WriteLine("Cannot write to chat log file {0}.", filename);
            }
        }
        private static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"[{0}]", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }
        private void UpdatePositionDiff(byte playerid, Vector3 v)
        {
            if (playerid == 255)
            {
                Position.LocalPlayerPosition += v;
                spawned = true;
            }
            else
            {
                if (!Clients.Players.ContainsKey(playerid))
                {
                    Clients.Players[playerid] = new Player();
                    Clients.Players[playerid].Name = "invalid";
                    //throw new Exception();
                    InvalidPlayerWarning(playerid);
                }
                Clients.Players[playerid].Position += v;
            }
        }
        private static void InvalidPlayerWarning(byte playerid)
        {
            Console.WriteLine("Position update of nonexistent player {0}." + playerid);
        }
        private void ReadAndUpdatePlayerPosition(BinaryReader br, byte playerid)
        {
            float x;
            float y;
            float z;
            if (ENABLE_FORTRESS)
            {
                x = (float)((double)NetworkHelper.ReadInt32(br) / 32);
                y = (float)((double)NetworkHelper.ReadInt32(br) / 32);
                z = (float)((double)NetworkHelper.ReadInt32(br) / 32);
            }
            else
            {
                x = (float)NetworkHelper.ReadInt16(br) / 32;
                y = (float)NetworkHelper.ReadInt16(br) / 32;
                z = (float)NetworkHelper.ReadInt16(br) / 32;
            }
            byte heading = br.ReadByte();
            byte pitch = br.ReadByte();
            Vector3 realpos = new Vector3(x, y, z);
            if (playerid == 255)
            {
                if (!enablePlayerUpdatePosition.ContainsKey(playerid) || enablePlayerUpdatePosition[playerid])
                {
                    Position.LocalPlayerPosition = realpos;
                }
                spawned = true;
            }
            else
            {
                if (!Clients.Players.ContainsKey(playerid))
                {
                    Clients.Players[playerid] = new Player();
                    Clients.Players[playerid].Name = "invalid";
                    InvalidPlayerWarning(playerid);
                }
                if (!enablePlayerUpdatePosition.ContainsKey(playerid) || enablePlayerUpdatePosition[playerid])
                {
                    Clients.Players[playerid].Position = realpos;
                }
                Clients.Players[playerid].Heading = heading;
                Clients.Players[playerid].Pitch = pitch;
            }
        }
        List<byte> received = new List<byte>();
        public void Dispose()
        {
            if (main != null)
            {
                //main.DisconnectAsync(new SocketAsyncEventArgs());
                main.Disconnect(false);
                main = null;
            }
            //throw new NotImplementedException();
        }
        int MapLoadingPercentComplete;
        public MemoryStream receivedMapStream;

        struct ServerPlayerIdentification
        {
            public byte ProtocolVersion;
            public string ServerName;
            public string ServerMotd;
            public byte UserType;
        }
        class ConnectedPlayer
        {
            public int id;
            public string name;
        }
        List<ConnectedPlayer> connectedplayers = new List<ConnectedPlayer>();
        public IEnumerable<string> ConnectedPlayers()
        {
            foreach (ConnectedPlayer p in connectedplayers)
            {
                yield return p.name;
            }
        }
        #region IClientNetwork Members
        public event EventHandler<MapLoadingProgressEventArgs> MapLoadingProgress;
        #endregion
        Dictionary<int, bool> enablePlayerUpdatePosition = new Dictionary<int, bool>();
        #region INetworkClient Members
        public Dictionary<int, bool> EnablePlayerUpdatePosition { get { return enablePlayerUpdatePosition; } set { enablePlayerUpdatePosition = value; } }
        #endregion
    }
    /// <summary>
    /// Client -> Server packet id.
    /// </summary>
    public enum MinecraftClientPacketId
    {
        PlayerIdentification = 0,
        SetBlock = 5,
        PositionandOrientation = 8,
        Message = 0x0d,

        ExtendedPacketCommand = 100,
    }
    /// <summary>
    /// Server -> Client packet id.
    /// </summary>
    public enum MinecraftServerPacketId
    {
        ServerIdentification = 0,
        Ping = 1,
        LevelInitialize = 2,
        LevelDataChunk = 3,
        LevelFinalize = 4,
        SetBlock = 6,
        SpawnPlayer = 7,
        PlayerTeleport = 8,
        PositionandOrientationUpdate = 9,
        PositionUpdate = 10,
        OrientationUpdate = 11,
        DespawnPlayer = 12,
        Message = 13,
        DisconnectPlayer = 14,

        ExtendedPacketCommand = 100,
        ExtendedPacketTick = 101,
    }
}
