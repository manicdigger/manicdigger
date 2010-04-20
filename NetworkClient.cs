using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace ManicDigger
{
    public interface INetworkClient
    {
        void Dispose();
        void Connect(string serverAddress, int port, string username, string auth);
        void Process();
        void SendSetBlock(Vector3 position, BlockSetMode mode, int type);
        event EventHandler<MapLoadingProgressEventArgs> MapLoadingProgress;
        event EventHandler<MapLoadedEventArgs> MapLoaded;
        void SendChat(string s);
        IEnumerable<string> ConnectedPlayers();
        void SendPosition(Vector3 position, Vector3 orientation);
    }
    public class MapLoadingProgressEventArgs : EventArgs
    {
        public int ProgressPercent { get; set; }
    }
    public class NetworkClientDummy : INetworkClient
    {
        [Inject]
        public ILocalPlayerPosition player { get; set; }
        public event EventHandler<MapLoadedEventArgs> MapLoaded;
        [Inject]
        public IGui Gui { get; set; }
        [Inject]
        public IMap Map1 { get; set; }
        [Inject]
        public IMapStorage Map { get; set; }
        [Inject]
        public IGameData Data { get; set; }
        [Inject]
        public fCraft.MapGenerator Gen { get; set; }
        public void Dispose()
        {
        }
        public void Connect(string serverAddress, int port, string username, string auth)
        {
        }
        public void Process()
        {
        }
        public void SendSetBlock(Vector3 position, BlockSetMode mode, int type)
        {
            if (mode == BlockSetMode.Destroy)
            {
                type = Data.TileIdEmpty;
            }
            Map1.SetTileAndUpdate(position, (byte)type);
            //Console.WriteLine("build:" + position);
            Console.WriteLine("player:" + player.LocalPlayerPosition + ", build:" + position);
        }
        public void SendChat(string s)
        {
            if (s == "")
            {
                return;
            }
            string[] ss = s.Split(new char[] { ' ' });
            if (s.StartsWith("/"))
            {
                string cmd = ss[0].Substring(1);
                string arguments;
                if (s.IndexOf(" ") == -1)
                { arguments = ""; }
                else
                { arguments = s.Substring(s.IndexOf(" ")); }
                arguments = arguments.Trim();
                if (cmd == "generate")
                {
                    DoGenerate(arguments, false);
                    Gui.DrawMap();
                }
            }
            Gui.AddChatline(s);
        }
        void DoGenerate(string mode, bool hollow)
        {
            switch (mode)
            {
                case "flatgrass":
                    bool reportedProgress = false;
                    playerMessage("Generating flatgrass map...");
                    for (int i = 0; i < Map.MapSizeX; i++)
                    {
                        for (int j = 0; j < Map.MapSizeY; j++)
                        {
                            for (int k = 1; k < Map.MapSizeZ / 2 - 1; k++)
                            {
                                if (!hollow) Map.SetBlock(i, j, k, Data.TileIdDirt);
                            }
                            Map.SetBlock(i, j, Map.MapSizeZ / 2 - 1, Data.TileIdGrass);
                        }
                        if (i > Map.MapSizeX / 2 && !reportedProgress)
                        {
                            reportedProgress = true;
                            playerMessage("Map generation: 50%");
                        }
                    }

                    //map.MakeFloodBarrier();

                    //if (map.Save(filename))
                    //{
                    //    player.Message("Map generation: Done.");
                    //}
                    //else
                    //{
                    //    player.Message(Color.Red, "An error occured while generating the map.");
                    //}
                    break;

                case "empty":
                    playerMessage("Generating empty map...");
                    //map.MakeFloodBarrier();

                    //if (map.Save(filename))
                    //{
                    //    player.Message("Map generation: Done.");
                    //}
                    //else
                    //{
                    //    player.Message(Color.Red, "An error occured while generating the map.");
                    //}

                    break;

                case "hills":
                    playerMessage("Generating terrain...");
                    Gen.GenerateMap(new fCraft.MapGeneratorParameters(
                                                                              5, 1, 0.5, 0.45, 0, 0.5, hollow));
                    break;

                case "mountains":
                    playerMessage("Generating terrain...");
                    Gen.GenerateMap(new fCraft.MapGeneratorParameters(
                                                                              8, 1, 0.5, 0.45, 0.1, 0.5, hollow));
                    break;

                case "lake":
                    playerMessage("Generating terrain...");
                    Gen.GenerateMap(new fCraft.MapGeneratorParameters(
                                                                              1, 0.6, 0.9, 0.45, -0.35, 0.55, hollow));
                    break;

                case "island":
                    playerMessage("Generating terrain...");
                    Gen.GenerateMap(new fCraft.MapGeneratorParameters(1, 0.6, 1, 0.45, 0.3, 0.35, hollow));
                    break;

                default:
                    playerMessage("Unknown map generation mode: " + mode);
                    break;
            }
        }
        private void playerMessage(string p)
        {
            Gui.AddChatline(p);
        }
        public IEnumerable<string> ConnectedPlayers()
        {
            yield return "[local player]";
        }
        #region IClientNetwork Members
        public void SendPosition(Vector3 position, Vector3 orientation)
        {
        }
        #endregion
        #region IClientNetwork Members
        public event EventHandler<MapLoadingProgressEventArgs> MapLoadingProgress;
        #endregion
    }
    public class MapLoadedEventArgs : EventArgs
    {
        public byte[, ,] map;
    }
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
        public event EventHandler<MapLoadedEventArgs> MapLoaded;

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
            bw.Write(StringToBytes(username));//Username
            bw.Write(StringToBytes(verificationKey));//Verification key
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
            bw.Write((byte)ClientPacketId.SetBlock);
            WriteInt16(bw, (short)(position.X));//-4
            WriteInt16(bw, (short)(position.Z));
            WriteInt16(bw, (short)position.Y);
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
            bw.Write((byte)ClientPacketId.Message);
            bw.Write((byte)255);//unused
            WriteString64(bw, s);
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
        Vector3 lastsentposition;
        public void SendPosition(Vector3 position, Vector3 orientation)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)ClientPacketId.PositionandOrientation);
            bw.Write((byte)255);//player id, self
            WriteInt16(bw, (short)((position.X) * 32));//gfd1
            WriteInt16(bw, (short)((position.Y + CharacterPhysics.characterheight) * 32));
            WriteInt16(bw, (short)(position.Z * 32));
            bw.Write((byte)((((orientation.Y) % (2 * Math.PI)) / (2 * Math.PI)) * 256));
            bw.Write(PitchByte());
            SendPacket(ms.ToArray());
            lastsentposition = position;
        }
        private byte PitchByte()
        {
            double xx = (Position.LocalPlayerOrientation.X + Math.PI) % (2 * Math.PI);
            xx = xx / (2 * Math.PI);
            return (byte)(xx * 256);
        }
        bool spawned = false;
        string ServerName;
        private int TryReadPacket()
        {
            BinaryReader br = new BinaryReader(new MemoryStream(received.ToArray()));
            if (received.Count == 0)
            {
                return 0;
            }
            var packetId = (ServerPacketId)br.ReadByte();
            int totalread = 1;
            if (packetId != ServerPacketId.PositionandOrientationUpdate
                 && packetId != ServerPacketId.PositionUpdate
                && packetId != ServerPacketId.OrientationUpdate
                && packetId != ServerPacketId.PlayerTeleport)
            {
                Console.WriteLine(Enum.GetName(typeof(ServerPacketId), packetId));
            }
            if (packetId == ServerPacketId.ServerIdentification)
            {
                totalread += 1 + 64 + 64 + 1; if (received.Count < totalread) { return 0; }
                ServerPlayerIdentification p = new ServerPlayerIdentification();
                p.ProtocolVersion = br.ReadByte();
                if (p.ProtocolVersion != 7)
                {
                    throw new Exception();
                }
                p.ServerName = ReadString64(br);
                p.ServerMotd = ReadString64(br);
                p.UserType = br.ReadByte();
                //connected = true;
                this.ServerName = p.ServerName;
                ChatLog("---Connected---");
            }
            else if (packetId == ServerPacketId.Ping)
            {
            }
            else if (packetId == ServerPacketId.LevelInitialize)
            {
                receivedMapStream = new MemoryStream();
                InvokeMapLoadingProgress(0);
            }
            else if (packetId == ServerPacketId.LevelDataChunk)
            {
                totalread += 2 + 1024 + 1; if (received.Count < totalread) { return 0; }
                int chunkLength = ReadInt16(br);
                byte[] chunkData = br.ReadBytes(1024);
                BinaryWriter bw1 = new BinaryWriter(receivedMapStream);
                byte[] chunkDataWithoutPadding = new byte[chunkLength];
                for (int i = 0; i < chunkLength; i++)
                {
                    chunkDataWithoutPadding[i] = chunkData[i];
                }
                bw1.Write(chunkDataWithoutPadding);
                MapLoadingPercentComplete = br.ReadByte();
                InvokeMapLoadingProgress(MapLoadingPercentComplete);
            }
            else if (packetId == ServerPacketId.LevelFinalize)
            {
                totalread += 2 + 2 + 2; if (received.Count < totalread) { return 0; }
                mapreceivedsizex = ReadInt16(br);
                mapreceivedsizez = ReadInt16(br);
                mapreceivedsizey = ReadInt16(br);
                receivedMapStream.Seek(0, SeekOrigin.Begin);
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
                    int size = ReadInt32(br2);
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
                if (MapLoaded != null)
                {
                    MapLoaded.Invoke(this, new MapLoadedEventArgs() { map = receivedmap });
                }
            }
            else if (packetId == ServerPacketId.SetBlock)
            {
                totalread += 2 + 2 + 2 + 1; if (received.Count < totalread) { return 0; }
                int x = ReadInt16(br);
                int z = ReadInt16(br);
                int y = ReadInt16(br);
                byte type = br.ReadByte();
                try { Map.SetTileAndUpdate(new Vector3(x, y, z), type); }
                catch { Console.WriteLine("Cannot update tile!"); }
            }
            else if (packetId == ServerPacketId.SpawnPlayer)
            {
                totalread += 1 + 64 + 2 + 2 + 2 + 1 + 1; if (received.Count < totalread) { return 0; }
                byte playerid = br.ReadByte();
                string playername = ReadString64(br);
                connectedplayers.Add(new ConnectedPlayer() { name = playername, id = playerid });
                if (Clients.Players.ContainsKey(playerid))
                {
                    //throw new Exception();
                }
                Clients.Players[playerid] = new Player();
                ReadAndUpdatePlayerPosition(br, playerid);
            }
            else if (packetId == ServerPacketId.PlayerTeleport)
            {
                totalread += 1 + (2 + 2 + 2) + 1 + 1; if (received.Count < totalread) { return 0; }
                byte playerid = br.ReadByte();
                ReadAndUpdatePlayerPosition(br, playerid);
            }
            else if (packetId == ServerPacketId.PositionandOrientationUpdate)
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
            else if (packetId == ServerPacketId.PositionUpdate)
            {
                totalread += 1 + 1 + 1 + 1; if (received.Count < totalread) { return 0; }
                byte playerid = br.ReadByte();
                float x = (float)br.ReadSByte() / 32;
                float y = (float)br.ReadSByte() / 32;
                float z = (float)br.ReadSByte() / 32;
                Vector3 v = new Vector3(x, y, z);
                UpdatePositionDiff(playerid, v);
            }
            else if (packetId == ServerPacketId.OrientationUpdate)
            {
                totalread += 1 + 1 + 1; if (received.Count < totalread) { return 0; }
                byte playerid = br.ReadByte();
                byte heading = br.ReadByte();
                byte pitch = br.ReadByte();
                Clients.Players[playerid].Heading = heading;
                Clients.Players[playerid].Pitch = pitch;
            }
            else if (packetId == ServerPacketId.DespawnPlayer)
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
            else if (packetId == ServerPacketId.Message)
            {
                totalread += 1 + 64; if (received.Count < totalread) { return 0; }
                byte unused = br.ReadByte();
                string message = ReadString64(br);
                Chatlines.AddChatline(message);
                ChatLog(message);
            }
            else if (packetId == ServerPacketId.DisconnectPlayer)
            {
                totalread += 64; if (received.Count < totalread) { return 0; }
                string disconnectReason = ReadString64(br);
                throw new Exception(disconnectReason);
            }
            else
            {
                throw new Exception();
            }
            return totalread;
        }
        private void InvokeMapLoadingProgress(int progress)
        {
            if (MapLoadingProgress != null)
            {
                MapLoadingProgress(this, new MapLoadingProgressEventArgs()
                {
                    ProgressPercent = progress
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
            string logsdir = "logs";
            if (!Directory.Exists(logsdir))
            {
                Directory.CreateDirectory(logsdir);
            }
            string filename=Path.Combine(logsdir, MakeValidFileName(ServerName) + ".txt");
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
                    //throw new Exception();
                    Console.WriteLine("Position update of nonexistent player {0}." + playerid);
                }
                Clients.Players[playerid].Position += v;
            }
        }
        private void ReadAndUpdatePlayerPosition(BinaryReader br, byte playerid)
        {
            float x = (float)ReadInt16(br) / 32;
            float y = (float)ReadInt16(br) / 32;
            float z = (float)ReadInt16(br) / 32;
            byte heading = br.ReadByte();
            byte pitch = br.ReadByte();
            Vector3 realpos = new Vector3(x, y, z) + new Vector3(0.5f, 0, 0.5f);
            if (playerid == 255)
            {
                Position.LocalPlayerPosition = realpos;
                spawned = true;
            }
            else
            {
                if (!Clients.Players.ContainsKey(playerid))
                {
                    Clients.Players[playerid] = new Player();
                }
                Clients.Players[playerid].Position = realpos;
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
        enum ClientPacketId
        {
            PlayerIdentification = 0,
            SetBlock = 5,
            PositionandOrientation = 8,
            Message = 0x0d,
        }
        enum ServerPacketId
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
        }
        private static byte[] StringToBytes(string s)
        {
            byte[] b = Encoding.ASCII.GetBytes(s);
            byte[] bb = new byte[64];
            for (int i = 0; i < bb.Length; i++)
            {
                bb[i] = 32; //' '
            }
            for (int i = 0; i < b.Length; i++)
            {
                bb[i] = b[i];
            }
            return bb;
        }
        private static string BytesToString(byte[] s)
        {
            string b = Encoding.ASCII.GetString(s).Trim();
            return b;
        }
        public int mapreceivedsizex;
        public int mapreceivedsizey;
        public int mapreceivedsizez;
        int ReadInt32(BinaryReader br)
        {
            byte[] array = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }
            return BitConverter.ToInt32(array, 0);
        }
        int ReadInt16(BinaryReader br)
        {
            byte[] array = br.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }
            return BitConverter.ToInt16(array, 0);
        }
        void WriteInt16(BinaryWriter bw, short v)
        {
            byte[] array = BitConverter.GetBytes((short)v);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }
            bw.Write(array);
        }
        int MapLoadingPercentComplete;
        public MemoryStream receivedMapStream;
        static string ReadString64(BinaryReader br)
        {
            return BytesToString(br.ReadBytes(64));
        }
        static void WriteString64(BinaryWriter bw, string s)
        {
            bw.Write(StringToBytes(s));
        }
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
    }
}
