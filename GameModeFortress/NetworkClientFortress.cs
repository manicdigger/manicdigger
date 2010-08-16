using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using System.IO;
using System.Net;
using System.Net.Sockets;
using OpenTK;
using System.Text.RegularExpressions;
using ProtoBuf;
using System.Diagnostics;

namespace GameModeFortress
{
    public interface INetworkPacketReceived
    {
        bool NetworkPacketReceived(PacketServer packet);
    }
    public interface INetworkClientFortress : INetworkClient
    {
        void SendPacketClient(PacketClient packetClient);
    }
    public class NetworkClientFortress : INetworkClientFortress
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
        public INetworkPacketReceived NetworkPacketReceived { get; set; }
        public event EventHandler<MapLoadedEventArgs> MapLoaded;
        public bool ENABLE_FORTRESS = true;
        public void Connect(string serverAddress, int port, string username, string auth)
        {
            main = new Socket(AddressFamily.InterNetwork,
                   SocketType.Stream, ProtocolType.Tcp);

            iep = new IPEndPoint(IPAddress.Any, port);
            main.Connect(serverAddress, port);
            byte[] n = CreateLoginPacket(username, auth);
            main.Send(n);
        }
        private byte[] CreateLoginPacket(string username, string verificationKey)
        {
            PacketClientIdentification p = new PacketClientIdentification()
            {
                Username = username,
                MdProtocolVersion = GameVersion.Version,
                VerificationKey = verificationKey
            };
            return Serialize(new PacketClient() { PacketId = ClientPacketId.PlayerIdentification, Identification = p });
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
            PacketClientSetBlock p = new PacketClientSetBlock()
            {
                X = (int)position.X,
                Y = (int)position.Y,
                Z = (int)position.Z,
                Mode = (mode == BlockSetMode.Create ? (byte)1 : (byte)0),
                BlockType = type
            };
            SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.SetBlock, SetBlock = p }));
        }
        public void SendPacketClient(PacketClient packet)
        {
            SendPacket(Serialize(packet));
        }
        public void SendChat(string s)
        {
            PacketClientMessage p = new PacketClientMessage() {  Message = s };
            SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.Message, Message = p }));
        }
        private byte[] Serialize(PacketClient p)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.SerializeWithLengthPrefix(ms, p, PrefixStyle.Base128);
            return ms.ToArray();
        }
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
            }
        }
        public int mapreceivedsizex;
        public int mapreceivedsizey;
        public int mapreceivedsizez;
        Vector3 lastsentposition;
        public void SendPosition(Vector3 position, Vector3 orientation)
        {
            PacketClientPositionAndOrientation p = new PacketClientPositionAndOrientation()
            {
                PlayerId = 255,//self
                X = (int)((position.X) * 32),
                Y = (int)((position.Y + CharacterPhysics.characterheight) * 32),
                Z = (int)(position.Z * 32),
                Heading = HeadingByte(orientation),
                Pitch = PitchByte(orientation),
            };
            SendPacket(Serialize(new PacketClient() { PacketId = ClientPacketId.PositionandOrientation, PositionAndOrientation = p }));
            lastsentposition = position;
        }
        public static byte HeadingByte(Vector3 orientation)
        {
            return (byte)((((orientation.Y) % (2 * Math.PI)) / (2 * Math.PI)) * 256);
        }
        public static byte PitchByte(Vector3 orientation)
        {
            double xx = (orientation.X + Math.PI) % (2 * Math.PI);
            xx = xx / (2 * Math.PI);
            return (byte)(xx * 256);
        }
        bool spawned = false;
        string serverName = "";
        string serverMotd = "";
        public string ServerName { get { return serverName; } set { serverName = value; } }
        public string ServerMotd { get { return serverMotd; } set { serverMotd = value; } }
        public int LocalPlayerId = 255;
        private int TryReadPacket()
        {
            MemoryStream ms = new MemoryStream(received.ToArray());
            if (received.Count == 0)
            {
                return 0;
            }
            int packetLength;
            int lengthPrefixLength;
            bool packetLengthOk = Serializer.TryReadLengthPrefix(ms, PrefixStyle.Base128, out packetLength);
            lengthPrefixLength = (int)ms.Position;
            if (!packetLengthOk || lengthPrefixLength + packetLength > ms.Length)
            {
                return 0;
            }
            ms.Position = 0;
            PacketServer packet = Serializer.DeserializeWithLengthPrefix<PacketServer>(ms, PrefixStyle.Base128);
            if (Debugger.IsAttached
                && packet.PacketId != ServerPacketId.PositionUpdate
                && packet.PacketId != ServerPacketId.OrientationUpdate
                && packet.PacketId != ServerPacketId.PlayerPositionAndOrientation
                && packet.PacketId != ServerPacketId.ExtendedPacketTick
                && packet.PacketId != ServerPacketId.Chunk)
            {
                Console.WriteLine(Enum.GetName(typeof(MinecraftServerPacketId), packet.PacketId));
            }
            switch (packet.PacketId)
            {
                case ServerPacketId.ServerIdentification:
                    {
                        string invalidversionstr = "Invalid game version. Local: {0}, Server: {1}";
                        {
                            string servergameversion = packet.Identification.MdProtocolVersion;
                            if (servergameversion != GameVersion.Version)
                            {
                                throw new Exception(string.Format(invalidversionstr, GameVersion.Version, servergameversion));
                            }
                        }
                        this.serverName = packet.Identification.ServerName;
                        this.ServerMotd = packet.Identification.ServerMotd;
                        ChatLog("---Connected---");
                    }
                    break;
                case ServerPacketId.Ping:
                    {
                    }
                    break;
                case ServerPacketId.LevelInitialize:
                    {
                        ReceivedMapLength = 0;
                        InvokeMapLoadingProgress(0, 0);
                    }
                    break;
                case ServerPacketId.LevelDataChunk:
                    {
                        MapLoadingPercentComplete = packet.LevelDataChunk.PercentComplete;
                        InvokeMapLoadingProgress(MapLoadingPercentComplete, (int)ReceivedMapLength);
                    }
                    break;
                case ServerPacketId.LevelFinalize:
                    {
                        if (MapLoaded != null)
                        {
                            MapLoaded.Invoke(this, new MapLoadedEventArgs() { });
                        }
                        loadedtime = DateTime.Now;
                    }
                    break;
                case ServerPacketId.SetBlock:
                    {
                        int x = packet.SetBlock.X;
                        int y = packet.SetBlock.Y;
                        int z = packet.SetBlock.Z;
                        int type = packet.SetBlock.BlockType;
                        try { Map.SetTileAndUpdate(new Vector3(x, y, z), type); }
                        catch { Console.WriteLine("Cannot update tile!"); }
                    }
                    break;
                case ServerPacketId.SpawnPlayer:
                    {
                        int playerid = packet.SpawnPlayer.PlayerId;
                        string playername = packet.SpawnPlayer.PlayerName;
                        connectedplayers.Add(new ConnectedPlayer() { name = playername, id = playerid });
                        Clients.Players[playerid] = new Player();
                        Clients.Players[playerid].Name = playername;
                        ReadAndUpdatePlayerPosition(packet.SpawnPlayer.PositionAndOrientation, playerid);
                        if (playerid == 255)
                        {
                            spawned = true;
                        }
                    }
                    break;
                case ServerPacketId.PlayerPositionAndOrientation:
                    {
                        int playerid = packet.PositionAndOrientation.PlayerId;
                        ReadAndUpdatePlayerPosition(packet.PositionAndOrientation.PositionAndOrientation, playerid);
                    }
                    break;
                case ServerPacketId.DespawnPlayer:
                    {
                        int playerid = packet.DespawnPlayer.PlayerId;
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
                case ServerPacketId.Message:
                    {
                        Chatlines.AddChatline(packet.Message.Message);
                        ChatLog(packet.Message.Message);
                    }
                    break;
                case ServerPacketId.DisconnectPlayer:
                    {
                        throw new Exception(packet.DisconnectPlayer.DisconnectReason);
                    }
                case ServerPacketId.Chunk:
                    {
                        var p = packet.Chunk;
                        byte[] decompressedchunk = GzipCompression.Decompress(p.CompressedChunk);
                        byte[, ,] receivedchunk = new byte[p.SizeX, p.SizeY, p.SizeZ];
                        {
                            BinaryReader br2 = new BinaryReader(new MemoryStream(decompressedchunk));
                            for (int zz = 0; zz < p.SizeZ; zz++)
                            {
                                for (int yy = 0; yy < p.SizeY; yy++)
                                {
                                    for (int xx = 0; xx < p.SizeX; xx++)
                                    {
                                        receivedchunk[xx, yy, zz] = br2.ReadByte();
                                    }
                                }
                            }
                        }
                        Map.Map.SetChunk(p.X, p.Y, p.Z, receivedchunk);
                        ReceivedMapLength += lengthPrefixLength + packetLength;
                    }
                    break;
                default:
                    {
                        bool handled = NetworkPacketReceived.NetworkPacketReceived(packet);
                        if (!handled)
                        {
                            Console.WriteLine("Invalid packet id: " + packet.PacketId);
                        }
                    }
                    break;
            }
            return lengthPrefixLength + packetLength;
        }
        int ReceivedMapLength = 0;
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
        private static void InvalidPlayerWarning(int playerid)
        {
            Console.WriteLine("Position update of nonexistent player {0}." + playerid);
        }
        private void ReadAndUpdatePlayerPosition(PositionAndOrientation positionAndOrientation, int playerid)
        {
            float x = (float)((double)positionAndOrientation.X / 32);
            float y = (float)((double)positionAndOrientation.Y / 32);
            float z = (float)((double)positionAndOrientation.Z / 32);
            byte heading = positionAndOrientation.Heading;
            byte pitch = positionAndOrientation.Pitch;
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
                main.Disconnect(false);
                main = null;
            }
        }
        int MapLoadingPercentComplete;
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
}
