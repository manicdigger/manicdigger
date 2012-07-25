using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using GameModeFortress;
using ManicDigger;
using ManicDigger.MapTools;
using OpenTK;
using ProtoBuf;
using System.Xml.Serialization;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;

namespace ManicDiggerServer
{
    public class ClientException : Exception
    {
        public ClientException(Exception innerException, int clientid)
            : base("Client exception", innerException)
        {
            this.clientid = clientid;
        }
        public int clientid;
    }
    [ProtoContract]
    public class ManicDiggerSave
    {
        [ProtoMember(1, IsRequired = false)]
        public int MapSizeX;
        [ProtoMember(2, IsRequired = false)]
        public int MapSizeY;
        [ProtoMember(3, IsRequired = false)]
        public int MapSizeZ;
        [ProtoMember(4, IsRequired = false)]
        public Dictionary<string, PacketServerInventory> Inventory;
        [ProtoMember(7, IsRequired = false)]
        public int Seed;
        [ProtoMember(8, IsRequired = false)]
        public long SimulationCurrentFrame;
        [ProtoMember(9, IsRequired = false)]
        public Dictionary<string, PacketServerPlayerStats> PlayerStats;
        [ProtoMember(10, IsRequired = false)]
        public int LastMonsterId;
    }
    public partial class Server : ICurrentTime, IDropItem
    {
        public IGameExit exit;
        [Inject]
        public ServerMap d_Map;
        [Inject]
        public GameData d_Data;
        [Inject]
        public CraftingTableTool d_CraftingTableTool;
        [Inject]
        public IGetFileStream d_GetFile;
        [Inject]
        public IChunkDb d_ChunkDb;
        [Inject]
        public IWorldGenerator d_Generator;
        [Inject]
        public ICompression d_NetworkCompression;
        [Inject]
        public WaterFinite d_Water;
        [Inject]
        public GroundPhysics d_GroundPhysics;
        [Inject]
        public ISocket d_MainSocket;
        [Inject]
        public IServerHeartbeat d_Heartbeat;

        public bool LocalConnectionsOnly { get; set; }
        public string[] PublicDataPaths = new string[0];
        public int singleplayerport = 25570;
        public Random rnd = new Random();
        public int SpawnPositionRandomizationRange = 96;
        public bool IsMono = Type.GetType("Mono.Runtime") != null;

        public void Exit()
        {
            exit.exit = true;
            System.Diagnostics.Process.GetCurrentProcess().Kill(); //stops Console.ReadLine() in ServerConsole thread.
        }

        public string serverpathlogs = Path.Combine(GameStorePath.GetStorePath(), "Logs");
        private void BuildLog(string p)
        {
            if (!config.BuildLogging)
            {
                return;
            }
            if (!Directory.Exists(serverpathlogs))
            {
                Directory.CreateDirectory(serverpathlogs);
            }
            string filename = Path.Combine(serverpathlogs, "BuildLog.txt");
            try
            {
                File.AppendAllText(filename, string.Format("{0} {1}\n", DateTime.Now, p));
            }
            catch
            {
                Console.WriteLine("Cannot write to server log file {0}.", filename);
            }
        }
        private void ServerEventLog(string p)
        {
            if (!config.ServerEventLogging)
            {
                return;
            }
            if (!Directory.Exists(serverpathlogs))
            {
                Directory.CreateDirectory(serverpathlogs);
            }
            string filename = Path.Combine(serverpathlogs, "ServerEventLog.txt");
            try
            {
                File.AppendAllText(filename, string.Format("{0} {1}\n", DateTime.Now, p));
            }
            catch
            {
                Console.WriteLine("Cannot write to server log file {0}.", filename);
            }
        }
        private void ChatLog(string p)
        {
            if (!config.ChatLogging)
            {
                return;
            }
            if (!Directory.Exists(serverpathlogs))
            {
                Directory.CreateDirectory(serverpathlogs);
            }
            string filename = Path.Combine(serverpathlogs, "ChatLog.txt");
            try
            {
                File.AppendAllText(filename, string.Format("{0} {1}\n", DateTime.Now, p));
            }
            catch
            {
                Console.WriteLine("Cannot write to server log file {0}.", filename);
            }
        }

        public bool Public;

        public void Start()
        {
            Server server = this;
            server.LoadConfig();
            var map = new ManicDiggerServer.ServerMap();
            map.d_CurrentTime = server;
            map.chunksize = 32;
            for (int i = 0; i < BlockTypes.Length; i++)
            {
                BlockTypes[i] = new BlockType() { };
            }

            // TODO: make it possible to change the world generator at run-time!
            var generator = server.config.Generator.getGenerator();
            generator.ChunkSize = map.chunksize;
            // apply chunk size to generator
            map.d_Generator = generator;
            //server.chunksize = 32;

            map.d_Heightmap = new InfiniteMapChunked2d() { chunksize = Server.chunksize, d_Map = map };
            map.Reset(server.config.MapSizeX, server.config.MapSizeY, server.config.MapSizeZ);
            server.d_Map = map;
            server.d_Generator = generator;
            string[] datapaths = new[] { Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"), "data" };
            string[] datapathspublic = new[] { Path.Combine(datapaths[0], "public"), Path.Combine(datapaths[1], "public") };
            server.PublicDataPaths = datapathspublic;
            var getfile = new GetFileStream(datapaths);
            var data = new GameData();
            data.Start();
            //data.Load(MyStream.ReadAllLines(getfile.GetFile("blocks.csv")),
            //    MyStream.ReadAllLines(getfile.GetFile("defaultmaterialslots.csv")),
            //    MyStream.ReadAllLines(getfile.GetFile("lightlevels.csv")));
            //var craftingrecipes = new CraftingRecipes();
            //craftingrecipes.data = data;
            //craftingrecipes.Load(MyStream.ReadAllLines(getfile.GetFile("craftingrecipes.csv")));
            server.d_Data = data;
            server.d_CraftingTableTool = new CraftingTableTool() { d_Map = map };
            server.LocalConnectionsOnly = !Public;
            server.d_GetFile = getfile;
            var networkcompression = new CompressionGzip();
            var diskcompression = new CompressionGzip();
            var chunkdb = new ChunkDbCompressed() { d_ChunkDb = new ChunkDbSqlite(), d_Compression = diskcompression };
            server.d_ChunkDb = chunkdb;
            map.d_ChunkDb = chunkdb;
            server.d_NetworkCompression = networkcompression;
            map.d_Data = server.d_Data;
            server.d_DataItems = new GameDataItemsBlocks() { d_Data = data };
            server.d_Water = new WaterFinite() { data = server.d_Data };
            server.d_GroundPhysics = new GroundPhysics() { data = server.d_Data };
            server.SaveFilenameWithoutExtension = SaveFilenameWithoutExtension;
            if (d_MainSocket == null)
            {
                server.d_MainSocket = new SocketNet()
                {
                    d_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                };
            }
            server.d_Heartbeat = new ServerHeartbeat();
            if ((Public) && (server.config.Public))
            {
                new Thread((a) => { for (; ; ) { server.SendHeartbeat(); Thread.Sleep(TimeSpan.FromMinutes(1)); } }).Start();
            }
            
            
            LoadMods();


            {
                if (!Directory.Exists(GameStorePath.gamepathsaves))
                {
                    Directory.CreateDirectory(GameStorePath.gamepathsaves);
                }
                Console.WriteLine("Loading savegame...");
                if (!File.Exists(GetSaveFilename()))
                {
                    Console.WriteLine("Creating new savegame file.");
                }
                LoadGame(GetSaveFilename());
                Console.WriteLine("Savegame loaded: " + GetSaveFilename());
            }
            if (LocalConnectionsOnly)
            {
                config.Port = singleplayerport;
            }
            Start(config.Port);

            LoadServerClient();

            // server monitor
            if (config.ServerMonitor)
            {
                this.serverMonitor = new ServerMonitor(this, exit);
                this.serverMonitor.Start();
            }

            // set up server console interpreter
            this.serverConsoleClient = new Client()
            {
                Id = this.serverConsoleId,
                playername = "Server"
            };
            GameModeFortress.Group serverGroup = new GameModeFortress.Group();
            serverGroup.Name = "Server";
            serverGroup.Level = 255;
            serverGroup.GroupPrivileges = new List<ServerClientMisc.Privilege>();
            foreach (ServerClientMisc.Privilege priv in Enum.GetValues(typeof(ServerClientMisc.Privilege)))
            {
                serverGroup.GroupPrivileges.Add(priv);
            }
            serverGroup.GroupColor = ServerClientMisc.ClientColor.Red;
            this.serverConsoleClient.AssignGroup(serverGroup);
            this.serverConsole = new ServerConsole(this, exit);
        }

        private void LoadMods()
        {
            ModManager m = new ModManager();
            m.Start(this);
            //todo
            IMod[] mods = new IMod[]
            {
                new ManicDigger.Mods.Default(),
                new ManicDigger.Mods.DefaultWorldGenerator(),
                new ManicDigger.Mods.Vegetation(),
                new ManicDigger.Mods.Doors(),
                new ManicDigger.Mods.GroundPhysics(),
                new ManicDigger.Mods.Monsters(),
                new ManicDigger.Mods.Tnt(),
                new ManicDigger.Mods.CraftingTable(),
                new ManicDigger.Mods.WaterFinite(),
            };
            for (int i = 0; i < mods.Length; i++)
            {
                mods[i].Start(m);
            }
        }

        private ServerMonitor serverMonitor;
        private ServerConsole serverConsole;
        private int serverConsoleId = -1; // make sure that not a regular client is assigned this ID
        public int ServerConsoleId {get { return serverConsoleId; } }
        private Client serverConsoleClient;

        public void ReceiveServerConsole(string message)
        {
            if (message == null)
            {
                return;
            }
            // server command
            if (message.StartsWith("/"))
            {
                string[] ss = message.Split(new[] { ' ' });
                string command = ss[0].Replace("/", "");
                string argument = message.IndexOf(" ") < 0 ? "" : message.Substring(message.IndexOf(" ") + 1);
                this.CommandInterpreter(serverConsoleId, command, argument);
                return;
            }
            // client command
            if (message.StartsWith("."))
            {
                return;
            }
            // chat message
            SendMessageToAll(string.Format("{0}: {1}", serverConsoleClient.ColoredPlayername(colorNormal), message));
            ChatLog(string.Format("{0}: {1}", serverConsoleClient.playername, message));
        }

        int Seed;
        private void LoadGame(string filename)
        {
            d_ChunkDb.Open(filename);
            byte[] globaldata = d_ChunkDb.GetGlobalData();
            if (globaldata == null)
            {
                //no savegame
                d_Generator.treeCount = config.Generator.TreeCount;
                if (config.Generator.RandomSeed)
                {
                    Seed = new Random().Next();
                }
                else
                {
                    Seed = config.Generator.Seed;
                }
                d_Generator.SetSeed(Seed);
                MemoryStream ms = new MemoryStream();
                SaveGame(ms);
                d_ChunkDb.SetGlobalData(ms.ToArray());
                return;
            }
            ManicDiggerSave save = Serializer.Deserialize<ManicDiggerSave>(new MemoryStream(globaldata));
            d_Generator.SetSeed(save.Seed);
            Seed = save.Seed;
            d_Map.Reset(d_Map.MapSizeX, d_Map.MapSizeX, d_Map.MapSizeZ);
            if (config.IsCreative) this.Inventory = Inventory = new Dictionary<string, PacketServerInventory>();
            else this.Inventory = save.Inventory;
            this.PlayerStats = save.PlayerStats;
            this.simulationcurrentframe = save.SimulationCurrentFrame;
            this.LastMonsterId = save.LastMonsterId;
        }
        public int LastMonsterId;
        public Dictionary<string, PacketServerInventory> Inventory = new Dictionary<string, PacketServerInventory>();
        public Dictionary<string, PacketServerPlayerStats> PlayerStats = new Dictionary<string, PacketServerPlayerStats>();
        public void SaveGame(Stream s)
        {
            ManicDiggerSave save = new ManicDiggerSave();
            SaveAllLoadedChunks();
            if (!config.IsCreative)
            {
                save.Inventory = Inventory;
            }
            save.PlayerStats = PlayerStats;
            save.Seed = Seed;
            save.SimulationCurrentFrame = simulationcurrentframe;
            save.LastMonsterId = LastMonsterId;
            Serializer.Serialize(s, save);
        }
        public void BackupDatabase(string backupFilename)
        {
            d_ChunkDb.Backup(backupFilename);
        }
        private void SaveAllLoadedChunks()
        {
            List<DbChunk> tosave = new List<DbChunk>();
            for (int cx = 0; cx < d_Map.MapSizeX / chunksize; cx++)
            {
                for (int cy = 0; cy < d_Map.MapSizeY / chunksize; cy++)
                {
                    for (int cz = 0; cz < d_Map.MapSizeZ / chunksize; cz++)
                    {
                        Chunk c = d_Map.chunks[cx, cy, cz];
                        if (c == null)
                        {
                            continue;
                        }
                        if (!c.DirtyForSaving)
                        {
                            continue;
                        }
                        c.DirtyForSaving = false;
                        MemoryStream ms = new MemoryStream();
                        Serializer.Serialize(ms, c);
                        tosave.Add(new DbChunk() { Position = new Xyz() { X = cx, Y = cy, Z = cz }, Chunk = ms.ToArray() });
                        if (tosave.Count > 200)
                        {
                            d_ChunkDb.SetChunks(tosave);
                            tosave.Clear();
                        }
                    }
                }
            }
            d_ChunkDb.SetChunks(tosave);
        }

        public string SaveFilenameWithoutExtension = "default";
        public string SaveFilenameOverride;
        string GetSaveFilename()
        {
            if (SaveFilenameOverride != null)
            {
                return SaveFilenameOverride;
            }
            return Path.Combine(GameStorePath.gamepathsaves, SaveFilenameWithoutExtension + MapManipulator.BinSaveExtension);
        }
        public void Process11()
        {
            if ((DateTime.Now - lastsave).TotalMinutes > 2)
            {
                DateTime start = DateTime.UtcNow;
                SaveGlobalData();
                Console.WriteLine("Game saved. ({0} seconds)", (DateTime.UtcNow - start));
                lastsave = DateTime.Now;
            }
        }

        private void SaveGlobalData()
        {
            MemoryStream ms = new MemoryStream();
            SaveGame(ms);
            d_ChunkDb.SetGlobalData(ms.ToArray());
        }
        DateTime lastsave = DateTime.Now;

        public void LoadConfig()
        {
            string filename = "ServerConfig.xml";
            if (!File.Exists(Path.Combine(GameStorePath.gamepathconfig, filename)))
            {
                Console.WriteLine("Server configuration file not found, creating new.");
                SaveConfig();
                return;
            }
            try
            {
                using (TextReader textReader = new StreamReader(Path.Combine(GameStorePath.gamepathconfig, filename)))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(ServerConfig));
                    config = (ServerConfig)deserializer.Deserialize(textReader);
                    textReader.Close();
                }
            }
            catch //This if for the original format
            {
                using (Stream s = new MemoryStream(File.ReadAllBytes(Path.Combine(GameStorePath.gamepathconfig, filename))))
                {
                    config = new ServerConfig();
                    StreamReader sr = new StreamReader(s);
                    XmlDocument d = new XmlDocument();
                    d.Load(sr);
                    config.Format = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Format"));
                    config.Name = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Name");
                    config.Motd = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Motd");
                    config.Port = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Port"));
                    string maxclients = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MaxClients");
                    if (maxclients != null)
                    {
                        config.MaxClients = int.Parse(maxclients);
                    }
                    string key = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Key");
                    if (key != null)
                    {
                        config.Key = key;
                    }
                    config.IsCreative = Misc.ReadBool(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Creative"));
                    config.Public = Misc.ReadBool(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Public"));
                    config.AllowGuests = Misc.ReadBool(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/AllowGuests"));
                    if (XmlTool.XmlVal(d, "/ManicDiggerServerConfig/AllowFreemove") != null)
                    {
                        config.AllowFreemove = Misc.ReadBool(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/AllowFreemove"));
                    }
                    if (XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeX") != null)
                    {
                        config.MapSizeX = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeX"));
                        config.MapSizeY = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeY"));
                        config.MapSizeZ = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeZ"));
                    }
                    config.BuildLogging = bool.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/BuildLogging"));
                    config.ServerEventLogging = bool.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/ServerEventLogging"));
                    config.ChatLogging = bool.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/ChatLogging"));
                    config.AllowScripting = bool.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/AllowScripting"));
                    config.ServerMonitor = bool.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/ServerMonitor"));
                    config.ClientConnectionTimeout = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/ClientConnectionTimeout"));
                    config.ClientPlayingTimeout = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/ClientPlayingTimeout"));
                }
                //Save with new version.
                SaveConfig();
            }
            Console.WriteLine("Server configuration loaded.");
        }

        public ServerConfig config;

        public void SaveConfig()
        {
            //Verify that we have a directory to place the file into.
            if (!Directory.Exists(GameStorePath.gamepathconfig))
            {
                Directory.CreateDirectory(GameStorePath.gamepathconfig);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(ServerConfig));
            TextWriter textWriter = new StreamWriter(Path.Combine(GameStorePath.gamepathconfig, "ServerConfig.xml"));

            //Check to see if config has been initialized
            if (config == null)
            {
                config = new ServerConfig();
            }
            if (config.Areas.Count == 0)
            {
                config.Areas = ServerConfigMisc.getDefaultAreas();
            }
            //Serialize the ServerConfig class to XML
            serializer.Serialize(textWriter, config);
            textWriter.Close();
        }

        public void SendHeartbeat()
        {
            if (config.Key == null)
            {
                return;
            }
            d_Heartbeat.Name = config.Name;
            d_Heartbeat.MaxClients = config.MaxClients;
            d_Heartbeat.PasswordProtected = config.IsPasswordProtected();
            d_Heartbeat.AllowGuests = config.AllowGuests;
            d_Heartbeat.Port = config.Port;
            d_Heartbeat.Version = GameVersion.Version;
            d_Heartbeat.Key = config.Key;
            d_Heartbeat.UsersCount = clients.Count;
            d_Heartbeat.Motd = config.Motd;
            List<string> playernames = new List<string>();
            lock (clients)
            {
                foreach (var k in clients)
                {
                    playernames.Add(k.Value.playername);
                }
            }
            d_Heartbeat.Players = playernames;
            try
            {
                d_Heartbeat.SendHeartbeat();
                if (!writtenServerKey)
                {
                    Console.WriteLine("hash: " + GetHash(d_Heartbeat.ReceivedKey));
                    writtenServerKey = true;
                }
                Console.WriteLine("Heartbeat sent.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Unable to send heartbeat.");
            }
        }

        IPEndPoint iep;
        bool writtenServerKey = false;
        public string hashPrefix = "server=";
        string GetHash(string hash)
        {
            try
            {
                if (hash.Contains(hashPrefix))
                {
                    hash = hash.Substring(hash.IndexOf(hashPrefix) + hashPrefix.Length);
                }
            }
            catch
            {
                return "";
            }
            return hash;
        }
        void Start(int port)
        {
            if (LocalConnectionsOnly)
            {
                iep = new IPEndPoint(IPAddress.Loopback, port);
            }
            else
            {
                iep = new IPEndPoint(IPAddress.Any, port);
            }
            d_MainSocket.Bind(iep);
            d_MainSocket.Listen(10);
        }
        public void Process()
        {
            try
            {
                Process11();
                Process1();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void Dispose()
        {
            if (!disposed)
            {
                d_MainSocket.Disconnect(false);
            }
            disposed = true;
        }
        bool disposed = false;
        double starttime = gettime();
        static double gettime()
        {
            return (double)DateTime.Now.Ticks / (10 * 1000 * 1000);
        }
        long simulationcurrentframe;
        public int SimulationCurrentFrame { get { return (int)simulationcurrentframe; } }
        double oldtime;
        double accumulator;

        private int lastClientId;
        private int GenerateClientId()
        {
            int i = 0;
            while (clients.ContainsKey(i))
            {
                i++;
            }
            return i;
        }
        public void Process1()
        {
            if (d_MainSocket == null)
            {
                return;
            }
            {
                double currenttime = gettime() - starttime;
                double deltaTime = currenttime - oldtime;
                accumulator += deltaTime;
                double dt = SIMULATION_STEP_LENGTH;
                while (accumulator > dt)
                {
                    simulationcurrentframe++;
                    /*
                    gameworld.Tick();
                    if (simulationcurrentframe % SIMULATION_KEYFRAME_EVERY == 0)
                    {
                        foreach (var k in clients)
                        {
                            SendTick(k.Key);
                        }
                    }
                    */
                    if ((GetSeason(simulationcurrentframe) != GetSeason(simulationcurrentframe - 1))
                        || GetHour(simulationcurrentframe) != GetHour(simulationcurrentframe - 1))
                    {
                        foreach (var c in clients)
                        {
                            NotifySeason(c.Key);
                        }
                    }
                    accumulator -= dt;
                }
                oldtime = currenttime;
            }
            byte[] data = new byte[1024];
            string stringData;
            int recv;
            if (d_MainSocket.Poll(0, SelectMode.SelectRead)) //Test for new connections
            {
                ISocket client1 = d_MainSocket.Accept();
                IPEndPoint iep1 = (IPEndPoint)client1.RemoteEndPoint;

                Client c = new Client();
                c.socket = client1;
                c.Ping.TimeoutValue = config.ClientConnectionTimeout;
                c.chunksseen = new bool[d_Map.MapSizeX / chunksize * d_Map.MapSizeY / chunksize * d_Map.MapSizeZ / chunksize];
                lock (clients)
                {
                    this.lastClientId = this.GenerateClientId();
                    c.Id = lastClientId;
                    clients[lastClientId] = c;
                }
                c.notifyMapTimer = new ManicDigger.Timer()
                {
                    INTERVAL = 1.0 / SEND_CHUNKS_PER_SECOND,
                };
                c.notifyMonstersTimer = new ManicDigger.Timer()
                {
                    INTERVAL = 1.0 / SEND_MONSTER_UDAPTES_PER_SECOND,
                };
                if (clients.Count > config.MaxClients)
                {
                    SendDisconnectPlayer(this.lastClientId, "Too many players! Try to connect later.");
                    KillPlayer(this.lastClientId);
                }
                else if (config.IsIPBanned(iep1.Address.ToString()))
                {
                    SendDisconnectPlayer(this.lastClientId, "Your IP has been banned from this server.");
                    ServerEventLog(string.Format("Banned IP {0} tries to connect.", iep1.Address.ToString()));
                    KillPlayer(this.lastClientId);
                }
            }
            ArrayList copyList = new ArrayList();
            foreach (var k in clients)
            {
                copyList.Add(k.Value.socket);
            }
            //if (copyList.Count == 0)
            //{
            //    return;
            //}
            if (copyList.Count != 0)
            {
                d_MainSocket.Select(copyList, null, null, 0);//10000000);
            }

            foreach (ISocket clientSocket in copyList)
            {
                int clientid = -1;
                foreach (var k in new List<KeyValuePair<int, Client>>(clients))
                {
                    if (k.Value != null && k.Value.socket == clientSocket)
                    {
                        clientid = k.Key;
                    }
                }
                Client client = clients[clientid];

                data = new byte[1024];
                try
                {
                    recv = clientSocket.Receive(data);
                }
                catch
                {
                    recv = 0;
                }
                //stringData = Encoding.ASCII.GetString(data, 0, recv);

                if (recv == 0)
                {
                    //client problem. disconnect client.
                    KillPlayer(clientid);
                }
                else
                {
                    for (int i = 0; i < recv; i++)
                    {
                        client.received.Add(data[i]);
                    }
                }
            }
            foreach (var k in new List<KeyValuePair<int, Client>>(clients))
            {
                Client c = k.Value;
                try
                {
                    for (; ; )
                    {
                        int bytesRead = TryReadPacket(k.Key);
                        if (bytesRead > 0)
                        {
                            clients[k.Key].received.RemoveRange(0, bytesRead);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch
                {
                    //client problem. disconnect client.
                    Console.WriteLine("Exception at client " + k.Key + ". Disconnecting client.");
                    SendDisconnectPlayer(k.Key, "Your client threw an exception at server.");
                    KillPlayer(k.Key);
                }
            }
            NotifyMap();
            foreach (var k in clients)
            {
                //k.Value.notifyMapTimer.Update(delegate { NotifyMapChunks(k.Key, 1); });
                NotifyInventory(k.Key);
                NotifyPlayerStats(k.Key);
                if (config.Monsters)
                {
                    k.Value.notifyMonstersTimer.Update(delegate { NotifyMonsters(k.Key); });
                }
            }
            pingtimer.Update(
            delegate
            {
                List<int> keysToDelete = new List<int>();
                foreach (var k in clients)
                {
                    // Check if client is alive. Detect half-dropped connections.
                    if (!k.Value.Ping.Send()/*&& k.Value.state == ClientStateOnServer.Playing*/)
                    {
                        if (k.Value.Ping.Timeout())
                        {
                            Console.WriteLine(k.Key + ": ping timeout. Disconnecting...");
                            keysToDelete.Add(k.Key);
                        }
                    }
                    else
                    {
                        SendPing(k.Key);
                    }
                }

                foreach (int key in keysToDelete)
                {
                    KillPlayer(key);
                }
            }
            );
            UnloadUnusedChunks();
            for (int i = 0; i < ChunksSimulated; i++)
            {
                ChunkSimulation();
            }
            if (config.Flooding)
            {
                UpdateWater();
            }
            tntTimer.Update(UpdateTnt);
            NotifyGroundPhysics();
        }

        private void UpdateTnt()
        {
            int startQueueCount = tntStack.Count;
            int now = 0;
            while (now++ < 3)
            {
                if (tntStack.Count == 0)
                {
                    return;
                }
                Vector3i pos = tntStack.Pop();
                int closeplayer = -1;
                int closedistance = -1;
                foreach (var k in clients)
                {
                    int distance = DistanceSquared(new Vector3i((int)k.Value.PositionMul32GlX / 32, (int)k.Value.PositionMul32GlZ / 32, (int)k.Value.PositionMul32GlY / 32), pos);
                    if (closedistance == -1 || distance < closedistance)
                    {
                        closedistance = distance;
                        closeplayer = k.Key;
                    }
                    if (distance < 255)
                    {
                        SendSound(k.Key, "tnt.wav");
                    }
                }
                Inventory inventory = GetPlayerInventory(clients[closeplayer].playername).Inventory;
                for (int xx = 0; xx < tntRange; xx++)
                {
                    for (int yy = 0; yy < tntRange; yy++)
                    {
                        for (int zz = 0; zz < tntRange; zz++)
                        {
                            if (sphereEq(xx - (tntRange - 1) / 2, yy - (tntRange - 1) / 2, zz - (tntRange - 1) / 2, tntRange / 2) <= 0)
                            {
                                Vector3i pos2 = new Vector3i(pos.x + xx - tntRange / 2,
                                pos.y + yy - tntRange / 2,
                                pos.z + zz - tntRange / 2);
                                if (!MapUtil.IsValidPos(d_Map, pos2.x, pos2.y, pos2.z))
                                {
                                    continue;
                                }
                                int block = d_Map.GetBlock(pos2.x, pos2.y, pos2.z);
                                if (tntStack.Count < tntMax
                                    && pos2 != pos
                                    && block == (int)TileTypeManicDigger.TNT)
                                {
                                    tntStack.Push(pos2);
                                    tntTimer.accumulator = tntTimer.INTERVAL;
                                }
                                else
                                {
                                    if ((block != 0)
                                        && (block != (int)TileTypeManicDigger.Adminium)
                                        && !(d_Data.IsFluid[block]))
                                    {
                                        SetBlockAndNotify(pos2.x, pos2.y, pos2.z, 0);
                                        if (!config.IsCreative)
                                        {
                                            // chance to get some of destruced blocks
                                            if (rnd.NextDouble() < .20f)
                                            {
                                                var item = new Item();
                                                item.ItemClass = ItemClass.Block;
                                                item.BlockId = d_Data.WhenPlayerPlacesGetsConvertedTo[block];
                                                GetInventoryUtil(inventory).GrabItem(item, 0);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                clients[closeplayer].IsInventoryDirty = true;
                NotifyInventory(closeplayer);
            }
        }
        private int sphereEq(int x, int y, int z, int r)
        {
            return x * x + y * y + z * z - r * r;
        }

        public int tntRange = 10; // sphere diameter
        ManicDigger.Timer tntTimer = new ManicDigger.Timer() { INTERVAL = 5 };
        Stack<Vector3i> tntStack = new Stack<Vector3i>();
        public int tntMax = 10;

        private void NotifyGroundPhysics()
        {
            foreach (var v in d_GroundPhysics.blocksToNotify)
            {
                foreach (var k in clients)
                {
                    SendSetBlock(k.Key, (int)v.pos.X, (int)v.pos.Y, (int)v.pos.Z, v.type);
                }
            }
            d_GroundPhysics.blocksToNotify.Clear();
        }

        private void UpdateWater()
        {
            d_Water.Update();
            try
            {
                foreach (var v in d_Water.tosetwater)
                {
                    byte watertype = (byte)(TileTypeManicDigger.Water1 + v.level - 1);
                    d_Map.SetBlock((int)v.pos.X, (int)v.pos.Y, (int)v.pos.Z, watertype);
                    foreach (var k in clients)
                    {
                        SendSetBlock(k.Key, (int)v.pos.X, (int)v.pos.Y, (int)v.pos.Z, watertype);
                        //SendSetBlock(k.Key, x, z, y, watertype);
                    }
                }
                foreach (var v in d_Water.tosetempty)
                {
                    byte emptytype = (byte)TileTypeManicDigger.Empty;
                    d_Map.SetBlock((int)v.X, (int)v.Y, (int)v.Z, emptytype);
                    foreach (var k in clients)
                    {
                        SendSetBlock(k.Key, (int)v.X, (int)v.Y, (int)v.Z, emptytype);
                        //SendSetBlock(k.Key, x, z, y, watertype);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            d_Water.tosetwater.Clear();
            d_Water.tosetempty.Clear();
        }

        private void SendPing(int clientid)
        {
            PacketServerPing p = new PacketServerPing()
            {
            };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.Ping, Ping = p }));
        }
        private void NotifyPing(int targetClientId, int ping)
        {
            foreach (var k in clients)
            {
                SendPlayerPing(k.Key, targetClientId, ping);
            }
        }
        private void SendPlayerPing(int recipientClientId, int targetClientId, int ping)
        {
            PacketServerPlayerPing p = new PacketServerPlayerPing()
            {
                ClientId = targetClientId,
                Ping = ping
            };
            SendPacket(recipientClientId, Serialize(new PacketServer() { PacketId = ServerPacketId.PlayerPing, PlayerPing = p }));
        }

        ManicDigger.Timer pingtimer = new ManicDigger.Timer() { INTERVAL = 1, MaxDeltaTime = 5 };
        private void NotifySeason(int clientid)
        {
            if (clients[clientid].state == ClientStateOnServer.Connecting)
            {
                return;
            }
            PacketServerSeason p = new PacketServerSeason()
            {
                Season = GetSeason(simulationcurrentframe),
                Hour = GetHour(simulationcurrentframe) + 1,
                DayNightCycleSpeedup = (60 * 60 * 24) / DAY_EVERY_SECONDS,
                //Moon = GetMoon(simulationcurrentframe),
                Moon = 0,
            };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.Season, Season = p }));
        }
        int SEASON_EVERY_SECONDS = 60 * 60;//1 hour
        int GetSeason(long frame)
        {
            long everyframes = (int)(1 / SIMULATION_STEP_LENGTH) * SEASON_EVERY_SECONDS;
            return (int)((frame / everyframes) % 4);
        }
        int DAY_EVERY_SECONDS = 60 * 60;//1 hour
        public int HourDetail = 4;
        public int GameStartHour = 9; //9 am
        int GetHour(long frame)
        {
            long everyframes = (int)(1 / SIMULATION_STEP_LENGTH) * DAY_EVERY_SECONDS / (24 * HourDetail);
            long startframe = (everyframes * HourDetail * GameStartHour);
            return (int)(((frame + startframe) / everyframes) % (24 * HourDetail));
        }
        /*
        int GetMoon(long frame)
        {
            long everyframes = (int)(1 / SIMULATION_STEP_LENGTH) * DAY_EVERY_SECONDS * 4;
            return (int)((frame / everyframes) % 2);
        }
        */
        int ChunksSimulated = 1;
        int chunksimulation_every { get { return (int)(1 / SIMULATION_STEP_LENGTH) * 60 * 10; } }//10 minutes

        void ChunkSimulation()
        {
            foreach (var k in clients)
            {
                var pos = PlayerBlockPosition(k.Value);

                long oldesttime = long.MaxValue;
                Vector3i oldestpos = new Vector3i();

                foreach (var p in ChunksAroundPlayer(pos))
                {
                    if (!MapUtil.IsValidPos(d_Map, p.x, p.y, p.z)) { continue; }
                    Chunk c = d_Map.chunks[p.x / chunksize, p.y / chunksize, p.z / chunksize];
                    if (c == null) { continue; }
                    if (c.data == null) { continue; }
                    if (c.LastUpdate < oldesttime)
                    {
                        oldesttime = c.LastUpdate;
                        oldestpos = p;
                    }
                    if (!c.IsPopulated)
                    {
                        PopulateChunk(p);
                        c.IsPopulated = true;
                    }
                }
                if (simulationcurrentframe - oldesttime > chunksimulation_every)
                {
                    ChunkUpdate(oldestpos, oldesttime);
                    Chunk c = d_Map.chunks[oldestpos.x / chunksize, oldestpos.y / chunksize, oldestpos.z / chunksize];
                    c.LastUpdate = (int)simulationcurrentframe;
                    return;
                }
            }
        }

        private void PopulateChunk(Vector3i p)
        {
            d_Generator.PopulateChunk(d_Map, p.x / chunksize, p.y / chunksize, p.z / chunksize);
        }

        private void ChunkUpdate(Vector3i p, long lastupdate)
        {          
            if (config.Monsters)
            {
                AddMonsters(p);
            }
            byte[] chunk = d_Map.GetChunk(p.x, p.y, p.z);
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        int block = chunk[MapUtil.Index3d(xx, yy, zz, chunksize, chunksize)];

                        if (block == (int)TileTypeManicDigger.DirtForFarming)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickCrops(pos);
                        }
                        if (block == (int)TileTypeManicDigger.Sapling)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickSapling(pos);
                        }
                        if (block == (int)TileTypeManicDigger.BrownMushroom || block == (int)TileTypeManicDigger.RedMushroom)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickMushroom(pos);
                        }
                        if (block == (int)TileTypeManicDigger.YellowFlowerDecorations || block == (int)TileTypeManicDigger.RedRoseDecorations)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickFlower(pos);
                        }
                        if (block == (int)TileTypeManicDigger.Dirt)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickDirt(pos);
                        }
                        if (block == (int)TileTypeManicDigger.Grass)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickGrass(pos);
                        }
                    }
                }
            }
        }

        public int[] MonsterTypesUnderground = new int[] { 1, 2 };
        public int[] MonsterTypesOnGround = new int[] { 0, 3, 4 };

        private void AddMonsters(Vector3i p)
        {
            Chunk chunk = d_Map.chunks[p.x / chunksize, p.y / chunksize, p.z / chunksize];
            int tries = 0;
            while (chunk.Monsters.Count < 1)
            {
                int xx = rnd.Next(chunksize);
                int yy = rnd.Next(chunksize);
                int zz = rnd.Next(chunksize);
                int px = p.x + xx;
                int py = p.y + yy;
                int pz = p.z + zz;
                if ((!MapUtil.IsValidPos(d_Map, px, py, pz))
                    || (!MapUtil.IsValidPos(d_Map, px, py, pz + 1))
                    || (!MapUtil.IsValidPos(d_Map, px, py, pz - 1)))
                {
                    continue;
                }
                int type;
                int height = MapUtil.blockheight(d_Map, 0, px, py);
                if (pz >= height)
                {
                    type = MonsterTypesOnGround[rnd.Next(MonsterTypesOnGround.Length)];
                }
                else
                {
                    type = MonsterTypesUnderground[rnd.Next(MonsterTypesUnderground.Length)];
                }
                if (d_Map.GetBlock(px, py, pz) == 0
                    && d_Map.GetBlock(px, py, pz + 1) == 0
                    && d_Map.GetBlock(px, py, pz - 1) != 0
                    && (!d_Data.IsFluid[d_Map.GetBlock(px, py, pz - 1)]))
                {
                    chunk.Monsters.Add(new Monster() { X = px, Y = py, Z = pz, Id = NewMonsterId(), Health = 20, MonsterType = type });
                }
                if (tries++ > 500)
                {
                    break;
                }
            }
        }
        public int NewMonsterId()
        {
            return LastMonsterId++;
        }

        private void BlockTickCrops(Vector3i pos)
        {
            if (MapUtil.IsValidPos(d_Map, pos.x, pos.y, pos.z + 1))
            {
                int blockabove = d_Map.GetBlock(pos.x, pos.y, pos.z + 1);
                if (blockabove == (int)TileTypeManicDigger.Crops1) { blockabove = (int)TileTypeManicDigger.Crops2; }
                else if (blockabove == (int)TileTypeManicDigger.Crops2) { blockabove = (int)TileTypeManicDigger.Crops3; }
                else if (blockabove == (int)TileTypeManicDigger.Crops3) { blockabove = (int)TileTypeManicDigger.Crops4; }
                else { return; }
                SetBlockAndNotify(pos.x, pos.y, pos.z + 1, blockabove);
            }
        }
        private void BlockTickSapling(Vector3i pos)
        {
            if (!IsShadow(pos.x, pos.y, pos.z))
            {
                if (!MapUtil.IsValidPos(d_Map, pos.x, pos.y, pos.z - 1))
                {
                    return;
                }
                int under = d_Map.GetBlock(pos.x, pos.y, pos.z - 1);
                if (!(under == (int)TileTypeManicDigger.Dirt
                    || under == (int)TileTypeManicDigger.Grass
                    || under == (int)TileTypeManicDigger.DirtForFarming))
                {
                    return;
                }
                MakeAppleTree(pos.x, pos.y, pos.z - 1);
            }
        }
        void PlaceTree(int x, int y, int z)
        {
            int TileIdLeaves = (int)TileTypeManicDigger.Leaves;

            Place(x, y, z + 1, (int)TileTypeManicDigger.TreeTrunk);
            Place(x, y, z + 2, (int)TileTypeManicDigger.TreeTrunk);
            Place(x, y, z + 3, (int)TileTypeManicDigger.TreeTrunk);

            Place(x + 1, y, z + 3, TileIdLeaves);
            Place(x - 1, y, z + 3, TileIdLeaves);
            Place(x, y + 1, z + 3, TileIdLeaves);
            Place(x, y - 1, z + 3, TileIdLeaves);

            Place(x + 1, y + 1, z + 3, TileIdLeaves);
            Place(x + 1, y - 1, z + 3, TileIdLeaves);
            Place(x - 1, y + 1, z + 3, TileIdLeaves);
            Place(x - 1, y - 1, z + 3, TileIdLeaves);

            Place(x + 1, y, z + 4, TileIdLeaves);
            Place(x - 1, y, z + 4, TileIdLeaves);
            Place(x, y + 1, z + 4, TileIdLeaves);
            Place(x, y - 1, z + 4, TileIdLeaves);

            Place(x, y, z + 4, TileIdLeaves);
        }
        public void Place(int x, int y, int z, int blocktype)
        {
            if (MapUtil.IsValidPos(d_Map, x, y, z))
            {
                SetBlockAndNotify(x, y, z, blocktype);
            }
        }
        private void BlockTickDirt(Vector3i pos)
        {
            if (MapUtil.IsValidPos(d_Map, pos.x, pos.y, pos.z + 1))
            {
                int roofBlock = d_Map.GetBlock(pos.x, pos.y, pos.z + 1);
                if (d_Data.IsTransparentForLight[roofBlock])
                {
                    if (IsShadow(pos.x, pos.y, pos.z) && !reflectedSunnyLight(pos.x, pos.y, pos.z))
                    {
                        // if 1% chance happens then 1 mushroom will grow up );
                        if (rnd.NextDouble() < 0.01)
                        {
                            int tile = rnd.NextDouble() < 0.6 ? (int)TileTypeManicDigger.RedMushroom : (int)TileTypeManicDigger.BrownMushroom;
                            SetBlockAndNotify(pos.x, pos.y, pos.z + 1, tile);
                        }
                    }
                    else
                    {
                        SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeManicDigger.Grass);
                    }
                }
            }
        }
        private void BlockTickGrass(Vector3i pos)
        {
            if (IsShadow(pos.x, pos.y, pos.z)
                && !(reflectedSunnyLight(pos.x, pos.y, pos.z) && d_Data.IsTransparentForLight[d_Map.GetBlock(pos.x, pos.y, pos.z + 1)]))
            {
                SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeManicDigger.Dirt);
            }
        }
        private void MakeAppleTree(int cx, int cy, int cz)
        {
            int x = cx;
            int y = cy;
            int z = cz;
            int TileIdLeaves = (int)TileTypeManicDigger.Leaves;
            int TileIdApples = (int)TileTypeManicDigger.Apples;
            int TileIdTreeTrunk = (int)TileTypeManicDigger.TreeTrunk;
            int treeHeight = rnd.Next(4, 6);
            int xx = 0;
            int yy = 0;
            int dir = 0;

            for (int i = 0; i < treeHeight; i++)
            {
                Place(x, y, z + i, TileIdTreeTrunk);
                if (i == treeHeight - 1)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 2; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));

                            Place(x + xx, y + yy, z + i, TileIdTreeTrunk);
                            float appleChance = 0.45f;
                            int tile;
                            tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx, y + yy, z + i + 1, tile);
                            tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx + 1, y + yy, z + i, tile);
                            tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx - 1, y + yy, z + i, tile);
                            tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx, y + yy + 1, z + i, tile);
                            tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx, y + yy - 1, z + i, tile);
                        }
                    }
                }
            }
        }
        private void PlaceIfEmpty(int x, int y, int z, int blocktype)
        {
            if (MapUtil.IsValidPos(d_Map, x, y, z) && d_Map.GetBlock(x, y, z) == 0)
            {
                SetBlockAndNotify(x, y, z, blocktype);
            }
        }
        // mushrooms will die when they have not shadow or dirt, or 20% chance happens
        private void BlockTickMushroom(Vector3i pos)
        {
            if (!MapUtil.IsValidPos(d_Map, pos.x, pos.y, pos.z)) return;
            if (rnd.NextDouble() < 0.2) { SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeManicDigger.Empty); return; }
            if (!IsShadow(pos.x, pos.y, pos.z - 1))
            {
                SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeManicDigger.Empty);
            }
            else
            {
                if (d_Map.GetBlock(pos.x, pos.y, pos.z - 1) == (int)TileTypeManicDigger.Dirt) return;
                SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeManicDigger.Empty);
            }
        }
        // floowers will die when they have not light, dirt or grass , or 2% chance happens
        private void BlockTickFlower(Vector3i pos)
        {
            if (!MapUtil.IsValidPos(d_Map, pos.x, pos.y, pos.z)) return;
            if (rnd.NextDouble() < 0.02) { SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeManicDigger.Empty); return; }
            if (IsShadow(pos.x, pos.y, pos.z - 1))
            {
                SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeManicDigger.Empty);
            }
            else
            {
                int under = d_Map.GetBlock(pos.x, pos.y, pos.z - 1);
                if ((under == (int)TileTypeManicDigger.Dirt
                      || under == (int)TileTypeManicDigger.Grass)) return;
                SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeManicDigger.Empty);
            }
        }
        private bool IsShadow(int x, int y, int z)
        {
            for (int i = 1; i < 10; i++)
            {
                if (MapUtil.IsValidPos(d_Map, x, y, z + i) && !d_Data.IsTransparentForLight[d_Map.GetBlock(x, y, z + i)])
                {
                    return true;
                }
            }
            return false;
        }
        // The true if on a cube gets the sunlight reflected from another cubes
        private bool reflectedSunnyLight(int x, int y, int z)
        {
            for (int i = x - 2; i <= x + 2; i++)
                for (int j = y - 2; j <= y + 2; j++)
                {
                    if (!IsShadow(i, j, z))
                    {
                        return true;
                    }
                }
            return false;
        }

        int CompressUnusedIteration = 0;
        private void UnloadUnusedChunks()
        {
            int sizex = d_Map.chunks.GetUpperBound(0) + 1;
            int sizey = d_Map.chunks.GetUpperBound(1) + 1;
            int sizez = d_Map.chunks.GetUpperBound(2) + 1;

            for (int i = 0; i < 100; i++)
            {
                var v = MapUtil.Pos(CompressUnusedIteration, d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize);
                Chunk c = d_Map.chunks[v.x, v.y, v.z];
                var vg = new Vector3i(v.x * chunksize, v.y * chunksize, v.z * chunksize);
                bool stop = false;
                if (c != null)
                {
                    bool unload = true;
                    foreach (var k in clients)
                    {
                        int viewdist = (int)(chunkdrawdistance * chunksize * 1.5f);
                        if (DistanceSquared(PlayerBlockPosition(k.Value), vg) <= viewdist * viewdist)
                        {
                            unload = false;
                        }
                    }
                    if (unload)
                    {
                        if (c.DirtyForSaving)
                        {
                            DoSaveChunk(v.x, v.y, v.z, c);
                        }
                        d_Map.chunks[v.x, v.y, v.z] = null;
                        stop = true;
                    }
                }
                CompressUnusedIteration++;
                if (CompressUnusedIteration >= sizex * sizey * sizez)
                {
                    CompressUnusedIteration = 0;
                }
                if (stop)
                {
                    return;
                }
            }
        }

        //on exit
        public void SaveAll()
        {
            for (int x = 0; x < d_Map.MapSizeX / chunksize; x++)
            {
                for (int y = 0; y < d_Map.MapSizeY / chunksize; y++)
                {
                    for (int z = 0; z < d_Map.MapSizeZ / chunksize; z++)
                    {
                        if (d_Map.chunks[x, y, z] != null)
                        {
                            DoSaveChunk(x, y, z, d_Map.chunks[x, y, z]);
                        }
                    }
                }
            }
            SaveGlobalData();
        }

        private void DoSaveChunk(int x, int y, int z, Chunk c)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, c);
            ChunkDb.SetChunk(d_ChunkDb, x, y, z, ms.ToArray());
        }
        int SEND_CHUNKS_PER_SECOND = 10;
        int SEND_MONSTER_UDAPTES_PER_SECOND = 3;
        /*
        private List<Vector3i> UnknownChunksAroundPlayer(int clientid)
        {
            Client c = clients[clientid];
            List<Vector3i> tosend = new List<Vector3i>();
            Vector3i playerpos = PlayerBlockPosition(c);
            foreach (var v in ChunksAroundPlayer(playerpos))
            {
                Chunk chunk = d_Map.chunks[v.x / chunksize, v.y / chunksize, v.z / chunksize];
                if (chunk == null)
                {
                    LoadChunk(v);
                    chunk = d_Map.chunks[v.x / chunksize, v.y / chunksize, v.z / chunksize];
                }
                int chunkupdatetime = chunk.LastChange;
                if (!c.chunksseen.ContainsKey(v) || c.chunksseen[v] < chunkupdatetime)
                {
                    if (MapUtil.IsValidPos(d_Map, v.x, v.y, v.z))
                    {
                        tosend.Add(v);
                    }
                }
            }
            return tosend;
        }
        */
        private void LoadChunk(int cx, int cy, int cz)
        {
            d_Map.LoadChunk(cx, cy, cz);
        }

        /*
        private int NotifyMapChunks(int clientid, int limit)
        {
            Client c = clients[clientid];
            Vector3i playerpos = PlayerBlockPosition(c);
            if (playerpos == new Vector3i())
            {
                return 0;
            }
            List<Vector3i> tosend = UnknownChunksAroundPlayer(clientid);
            tosend.Sort((a, b) => DistanceSquared(a, playerpos).CompareTo(DistanceSquared(b, playerpos)));
            int sent = 0;
            foreach (var v in tosend)
            {
                if (sent >= limit)
                {
                    break;
                }
                byte[] chunk = d_Map.GetChunk(v.x, v.y, v.z);
                c.chunksseen[v] = (int)simulationcurrentframe;
                sent++;
                if (MapUtil.IsSolidChunk(chunk) && chunk[0] == 0)
                {
                    //don't send empty chunk.
                    continue;
                }
                byte[] compressedchunk = CompressChunkNetwork(chunk);
                if (!c.heightmapchunksseen.ContainsKey(new Vector2i(v.x, v.y)))
                {
                    byte[] heightmapchunk = d_Map.GetHeightmapChunk(v.x, v.y);
                    byte[] compressedHeightmapChunk = d_NetworkCompression.Compress(heightmapchunk);
                    PacketServerHeightmapChunk p1 = new PacketServerHeightmapChunk()
                    {
                        X = v.x,
                        Y = v.y,
                        SizeX = chunksize,
                        SizeY = chunksize,
                        CompressedHeightmap = compressedHeightmapChunk,
                    };
                    SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.HeightmapChunk, HeightmapChunk = p1 }));
                    c.heightmapchunksseen.Add(new Vector2i(v.x, v.y), (int)simulationcurrentframe);
                }
                PacketServerChunk p = new PacketServerChunk()
                {
                    X = v.x,
                    Y = v.y,
                    Z = v.z,
                    SizeX = chunksize,
                    SizeY = chunksize,
                    SizeZ = chunksize,
                    CompressedChunk = compressedchunk,
                };
                SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.Chunk, Chunk = p }));
            }
        }
         
        /*
Vector3i playerpos = PlayerBlockPosition(clients[clientid]);
var around = new List<Vector3i>(ChunksAroundPlayer(playerpos));
for (int i = 0; i < around.Count; i++)
{
var v = around[i];
d_Map.GetBlock(v.x, v.y, v.z); //force load
if (i % 10 == 0)
{
    SendLevelProgress(clientid, (int)(((float)i / around.Count) * 100), "Generating world...");
}
}
ChunkSimulation();

List<Vector3i> unknown = UnknownChunksAroundPlayer(clientid);
int sent = 0;
for (int i = 0; i < unknown.Count; i++)
{
sent += NotifyMapChunks(clientid, 5);
SendLevelProgress(clientid, (int)(((float)sent / unknown.Count) * 100), "Downloading map...");
if (sent >= unknown.Count) { break; }
}
*/
        /*
            SendLevelFinalize(clientid);
            return sent;
        }
        */
        const string invalidplayername = "invalid";
        private void NotifyInventory(int clientid)
        {
            Client c = clients[clientid];
            if (c.IsInventoryDirty && c.playername != invalidplayername)
            {
                PacketServerInventory p;
                /*
                if (config.IsCreative)
                {
                    p = new PacketServerInventory()
                    {
                        BlockTypeAmount = StartInventory(),
                        IsFinite = false,
                    };
                }
                else
                */
                {
                    p = GetPlayerInventory(c.playername);
                }
                SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.FiniteInventory, Inventory = p }));
                c.IsInventoryDirty = false;
            }
        }
        private void NotifyPlayerStats(int clientid)
        {
            Client c = clients[clientid];
            if (c.IsPlayerStatsDirty && c.playername != invalidplayername)
            {
                PacketServerPlayerStats p = GetPlayerStats(c.playername);
                SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.PlayerStats, PlayerStats = p }));
                c.IsPlayerStatsDirty = false;
            }
        }
        private void HitMonsters(int clientid, int health)
        {
            Client c = clients[clientid];
            int mapx = c.PositionMul32GlX / 32;
            int mapy = c.PositionMul32GlZ / 32;
            int mapz = c.PositionMul32GlY / 32;
            //3x3x3 chunks
            List<PacketServerMonster> p = new List<PacketServerMonster>();
            for (int xx = -1; xx < 2; xx++)
            {
                for (int yy = -1; yy < 2; yy++)
                {
                    for (int zz = -1; zz < 2; zz++)
                    {
                        int cx = (mapx / chunksize) + xx;
                        int cy = (mapy / chunksize) + yy;
                        int cz = (mapz / chunksize) + zz;
                        if (!MapUtil.IsValidChunkPos(d_Map, cx, cy, cz, chunksize))
                        {
                            continue;
                        }
                        Chunk chunk = d_Map.chunks[cx, cy, cz];
                        if (chunk == null || chunk.Monsters == null)
                        {
                            continue;
                        }
                        foreach (Monster m in chunk.Monsters)
                        {
                            Vector3i mpos = new Vector3i { x = m.X, y = m.Y, z = m.Z };
                            Vector3i ppos = new Vector3i
                            {
                                x = clients[clientid].PositionMul32GlX / 32,
                                y = clients[clientid].PositionMul32GlZ / 32,
                                z = clients[clientid].PositionMul32GlY / 32
                            };
                            if (DistanceSquared(mpos, ppos) < 15)
                            {
                                m.Health -= health;
                                //Console.WriteLine("HIT! -2 = " + m.Health);
                                if (m.Health <= 0)
                                {
                                    chunk.Monsters.Remove(m);
                                    SendSound(clientid, "death.wav");
                                    break;
                                }
                                SendSound(clientid, "grunt2.wav");
                                break;
                            }
                        }
                    }
                }
            }
        }
        private void NotifyMonsters(int clientid)
        {
            Client c = clients[clientid];
            int mapx = c.PositionMul32GlX / 32;
            int mapy = c.PositionMul32GlZ / 32;
            int mapz = c.PositionMul32GlY / 32;
            //3x3x3 chunks
            List<PacketServerMonster> p = new List<PacketServerMonster>();
            for (int xx = -1; xx < 2; xx++)
            {
                for (int yy = -1; yy < 2; yy++)
                {
                    for (int zz = -1; zz < 2; zz++)
                    {
                        int cx = (mapx / chunksize) + xx;
                        int cy = (mapy / chunksize) + yy;
                        int cz = (mapz / chunksize) + zz;
                        if (!MapUtil.IsValidChunkPos(d_Map, cx, cy, cz, chunksize))
                        {
                            continue;
                        }
                        Chunk chunk = d_Map.chunks[cx, cy, cz];
                        if (chunk == null || chunk.Monsters == null)
                        {
                            continue;
                        }
                        foreach (Monster m in new List<Monster>(chunk.Monsters))
                        {
                            MonsterWalk(m);
                        }
                        foreach (Monster m in chunk.Monsters)
                        {
                            float progress = m.WalkProgress;
                            if (progress < 0) //delay
                            {
                                progress = 0;
                            }
                            byte heading = 0;
                            if (m.WalkDirection.x == -1 && m.WalkDirection.y == 0) { heading = (byte)(((int)byte.MaxValue * 3) / 4); }
                            if (m.WalkDirection.x == 1 && m.WalkDirection.y == 0) { heading = byte.MaxValue / 4; }
                            if (m.WalkDirection.x == 0 && m.WalkDirection.y == -1) { heading = 0; }
                            if (m.WalkDirection.x == 0 && m.WalkDirection.y == 1) { heading = byte.MaxValue / 2; }
                            var mm = new PacketServerMonster()
                            {
                                Id = m.Id,
                                MonsterType = m.MonsterType,
                                Health = m.Health,
                                PositionAndOrientation = new PositionAndOrientation()
                                {
                                    Heading = heading,
                                    Pitch = 0,
                                    X = (int)((m.X + progress * m.WalkDirection.x) * 32 + 16),
                                    Y = (int)((m.Z + progress * m.WalkDirection.z) * 32),
                                    Z = (int)((m.Y + progress * m.WalkDirection.y) * 32 + 16),
                                }
                            };
                            p.Add(mm);
                        }
                    }
                }
            }
            //send only nearest monsters
            p.Sort((a, b) =>
            {
                Vector3i posA = new Vector3i(a.PositionAndOrientation.X, a.PositionAndOrientation.Y, a.PositionAndOrientation.Z);
                Vector3i posB = new Vector3i(b.PositionAndOrientation.X, b.PositionAndOrientation.Y, b.PositionAndOrientation.Z);
                Client client = clients[clientid];
                Vector3i posPlayer = new Vector3i(client.PositionMul32GlX, client.PositionMul32GlY, client.PositionMul32GlZ);
                return DistanceSquared(posA, posPlayer).CompareTo(DistanceSquared(posB, posPlayer));
            }
            );
            if (p.Count > sendmaxmonsters)
            {
                p.RemoveRange(sendmaxmonsters, p.Count - sendmaxmonsters);
            }
            SendPacket(clientid, Serialize(new PacketServer()
            {
                PacketId = ServerPacketId.Monster,
                Monster = new PacketServerMonsters() { Monsters = p.ToArray() }
            }));
        }
        int sendmaxmonsters = 10;
        void MonsterWalk(Monster m)
        {
            m.WalkProgress += 0.3f;
            if (m.WalkProgress < 1)
            {
                return;
            }
            int oldcx = m.X / chunksize;
            int oldcy = m.Y / chunksize;
            int oldcz = m.Z / chunksize;
            d_Map.chunks[oldcx, oldcy, oldcz].Monsters.Remove(m);
            m.X += m.WalkDirection.x;
            m.Y += m.WalkDirection.y;
            m.Z += m.WalkDirection.z;
            int newcx = m.X / chunksize;
            int newcy = m.Y / chunksize;
            int newcz = m.Z / chunksize;
            if (d_Map.chunks[newcx, newcy, newcz].Monsters == null)
            {
                d_Map.chunks[newcx, newcy, newcz].Monsters = new List<Monster>();
            }
            d_Map.chunks[newcx, newcy, newcz].Monsters.Add(m);
            /*
            if (rnd.Next(3) == 0)
            {
                m.WalkDirection = new Vector3i();
                m.WalkProgress = -2;
                return;
            }
            */
            List<Vector3i> l = new List<Vector3i>();
            for (int zz = -1; zz < 2; zz++)
            {
                if (d_Map.GetBlock(m.X + 1, m.Y, m.Z + zz) == 0
                     && d_Map.GetBlock(m.X + 1, m.Y, m.Z + zz - 1) != 0)
                {
                    l.Add(new Vector3i(1, 0, zz));
                }
                if (d_Map.GetBlock(m.X - 1, m.Y, m.Z + zz) == 0
                    && d_Map.GetBlock(m.X - 1, m.Y, m.Z + zz - 1) != 0)
                {
                    l.Add(new Vector3i(-1, 0, zz));
                }
                if (d_Map.GetBlock(m.X, m.Y + 1, m.Z + zz) == 0
                    && d_Map.GetBlock(m.X, m.Y + 1, m.Z + zz - 1) != 0)
                {
                    l.Add(new Vector3i(0, 1, zz));
                }
                if (d_Map.GetBlock(m.X, m.Y - 1, m.Z + zz) == 0
                    && d_Map.GetBlock(m.X, m.Y - 1, m.Z + zz - 1) != 0)
                {
                    l.Add(new Vector3i(0, -1, zz));
                }
            }
            Vector3i dir;
            if (l.Count > 0)
            {
                dir = l[rnd.Next(l.Count)];
            }
            else
            {
                dir = new Vector3i();
            }
            m.WalkDirection = dir;
            m.WalkProgress = 0;
        }
        PacketServerInventory GetPlayerInventory(string playername)
        {
            if (Inventory == null)
            {
                Inventory = new Dictionary<string, PacketServerInventory>();
            }
            if (!Inventory.ContainsKey(playername))
            {
                Inventory[playername] = new PacketServerInventory()
                {
                    Inventory = StartInventory(),
                    /*
                    IsFinite = true,
                    Max = FiniteInventoryMax,
                    */
                };
            }
            return Inventory[playername];
        }
        PacketServerPlayerStats GetPlayerStats(string playername)
        {
            if (PlayerStats == null)
            {
                PlayerStats = new Dictionary<string, PacketServerPlayerStats>();
            }
            if (!PlayerStats.ContainsKey(playername))
            {
                PlayerStats[playername] = StartPlayerStats();
            }
            return PlayerStats[playername];
        }
        public int FiniteInventoryMax = 200;
        /*
        Dictionary<int, int> StartInventory()
        {
            Dictionary<int, int> d = new Dictionary<int, int>();
            d[(int)TileTypeManicDigger.CraftingTable] = 6;
            d[(int)TileTypeManicDigger.Crops1] = 1;
            return d;
        }
        */
        Inventory StartInventory()
        {
            Inventory inv = ManicDigger.Inventory.Create();
            int x = 0;
            int y = 0;
            for (int i = 0; i < d_Data.StartInventoryAmount.Length; i++)
            {
                int amount = d_Data.StartInventoryAmount[i];
                if (config.IsCreative)
                {
                    if (amount > 0 || d_Data.IsBuildable[i])
                    {
                        inv.Items.Add(new ProtoPoint(x, y), new Item() { ItemClass = ItemClass.Block, BlockId = i, BlockCount = 0 });
                        x++;
                        if (x >= GetInventoryUtil(inv).CellCount.X)
                        {
                            x = 0;
                            y++;
                        }
                    }
                }
                else if (amount > 0)
                {
                    inv.Items.Add(new ProtoPoint(x, y), new Item() { ItemClass = ItemClass.Block, BlockId = i, BlockCount = amount });
                    x++;
                    if (x >= GetInventoryUtil(inv).CellCount.X)
                    {
                        x = 0;
                        y++;
                    }
                }
            }
            return inv;
        }
        PacketServerPlayerStats StartPlayerStats()
        {
            var p = new PacketServerPlayerStats();
            p.CurrentHealth = 20;
            p.MaxHealth = 20;
            return p;
        }
        public Vector3i PlayerBlockPosition(Client c)
        {
            return new Vector3i(c.PositionMul32GlX / 32, c.PositionMul32GlZ / 32, c.PositionMul32GlY / 32);
        }
        int DistanceSquared(Vector3i a, Vector3i b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            int dz = a.z - b.z;
            return dx * dx + dy * dy + dz * dz;
        }
        private void KillPlayer(int clientid)
        {
            if (!clients.ContainsKey(clientid))
            {
                return;
            }
            string coloredName = clients[clientid].ColoredPlayername(colorNormal);
            string name = clients[clientid].playername;
            clients.Remove(clientid);
            foreach (var kk in clients)
            {
                SendDespawnPlayer(kk.Key, clientid);
            }
            if (name != "invalid")
            {
                SendMessageToAll(string.Format("Player {0} disconnected.", coloredName));
                ServerEventLog(string.Format("{0} disconnects.", name));
            }
        }

        //returns bytes read.
        private int TryReadPacket(int clientid)
        {
            Client c = clients[clientid];
            MemoryStream ms = new MemoryStream(c.received.ToArray());
            if (c.received.Count == 0)
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
            PacketClient packet = Serializer.DeserializeWithLengthPrefix<PacketClient>(ms, PrefixStyle.Base128);
            //int packetid = br.ReadByte();
            //int totalread = 1;

            if (config.ServerMonitor && !this.serverMonitor.CheckPacket(clientid, packet))
            {
                return lengthPrefixLength + packetLength;
            }
            switch (packet.PacketId)
            {
                case ClientPacketId.PingReply:
                    clients[clientid].Ping.Receive();
                    this.NotifyPing(clientid, clients[clientid].Ping.RoundtripTime.Milliseconds);
                    break;
                case ClientPacketId.PlayerIdentification:
                    {
                        if (config.IsPasswordProtected() && packet.Identification.ServerPassword != config.Password)
                        {
                            Console.WriteLine(string.Format("{0} fails to join (invalid server password).", packet.Identification.Username));
                            ServerEventLog(string.Format("{0} fails to join (invalid server password).", packet.Identification.Username));
                            SendDisconnectPlayer(clientid, "Invalid server password.");
                            KillPlayer(clientid);
                            break;
                        }
                        SendServerIdentification(clientid);
                        string username = packet.Identification.Username;

                        // allowed characters in username: a-z,A-Z,0-9,-,_ length: 1-16
                        Regex allowedUsername = new Regex(@"^(\w|-){1,16}$");

                        if (string.IsNullOrEmpty(username) || !allowedUsername.IsMatch(username))
                        {
                            SendDisconnectPlayer(clientid, "Invalid username (allowed characters: a-z,A-Z,0-9,-,_; max. length: 16).");
                            ServerEventLog(string.Format("{0} can't join (invalid username: {1}).", ((IPEndPoint)c.socket.RemoteEndPoint).Address.ToString(), username));
                            KillPlayer(clientid);
                            break;
                        }

                        bool isClientLocalhost = (((IPEndPoint)c.socket.RemoteEndPoint).Address.ToString() == "127.0.0.1");
                        bool verificationFailed = false;

                        if ((ComputeMd5(config.Key.Replace("-", "") + username) != packet.Identification.VerificationKey)
                            && (!isClientLocalhost))
                        {
                            //Account verification failed.
                            username = "~" + username;
                            verificationFailed = true;
                        }

                        if (!config.AllowGuests && verificationFailed)
                        {
                            SendDisconnectPlayer(clientid, "Guests are not allowed on this server. Login or register an account.");
                            KillPlayer(clientid);
                            break;
                        }

                        if (config.IsUserBanned(username))
                        {
                            SendDisconnectPlayer(clientid, "Your username has been banned from this server.");
                            ServerEventLog(string.Format("{0} fails to join (banned username: {1}).", ((IPEndPoint)c.socket.RemoteEndPoint).Address.ToString(), username));
                            KillPlayer(clientid);
                            break;
                        }

                        //When a duplicate user connects, append a number to name.
                        foreach (var k in clients)
                        {
                            if (k.Value.playername.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // If duplicate is a registered user, kick duplicate. It is likely that the user lost connection before.
                                if (!verificationFailed && !isClientLocalhost)
                                {
                                    KillPlayer(k.Key);
                                    break;
                                }

                                // Duplicates are handled as guests.
                                username = GenerateUsername(username);
                                if (!username.StartsWith("~")) { username = "~" + username; }
                                break;
                            }
                        }
                        clients[clientid].playername = username;

                        // Assign group to new client
                        //Check if client is in ServerClient.xml and assign corresponding group.
                        bool exists = false;
                        foreach (GameModeFortress.Client client in serverClient.Clients)
                        {
                            if (client.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                            {
                                foreach (GameModeFortress.Group clientGroup in serverClient.Groups)
                                {
                                    if (clientGroup.Name.Equals(client.Group))
                                    {
                                        exists = true;
                                        clients[clientid].AssignGroup(clientGroup);
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        if (!exists)
                        {
                            if (clients[clientid].playername.StartsWith("~"))
                            {
                                clients[clientid].AssignGroup(this.defaultGroupGuest);
                            }
                            else
                            {
                                clients[clientid].AssignGroup(this.defaultGroupRegistered);
                            }
                        }
                        if (isClientLocalhost)
                        {
                            clients[clientid].AssignGroup(serverClient.Groups.Find(v => v.Name == "Admin"));
                        }
                        this.SendFillAreaLimit(clientid, clients[clientid].FillLimit);
                    }
                    break;
                case ClientPacketId.RequestBlob:
                    {
                        // Set player's spawn position
                        Vector3i position;
                        GameModeFortress.Spawn playerSpawn = null;
                        // Check if there is a spawn entry for his assign group
                        if (clients[clientid].clientGroup.Spawn != null)
                        {
                            playerSpawn = clients[clientid].clientGroup.Spawn;
                        }
                        // Check if there is an entry in clients with spawn member (overrides group spawn).
                        foreach (GameModeFortress.Client client in serverClient.Clients)
                        {
                            if (client.Name.Equals(clients[clientid].playername, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (client.Spawn != null)
                                {
                                    playerSpawn = client.Spawn;
                                }
                                break;
                            }
                        }

                        if (playerSpawn == null)
                        {
                            position = new Vector3i(this.defaultPlayerSpawn.x * 32, this.defaultPlayerSpawn.z * 32, this.defaultPlayerSpawn.y * 32);
                        }
                        else
                        {
                            position = this.SpawnToVector3i(playerSpawn);
                        }

                        clients[clientid].PositionMul32GlX = position.x;
                        clients[clientid].PositionMul32GlY = position.y + (int)(0.5 * 32);
                        clients[clientid].PositionMul32GlZ = position.z;

                        string ip = ((IPEndPoint)clients[clientid].socket.RemoteEndPoint).Address.ToString();
                        SendMessageToAll(string.Format("Player {0} joins.", clients[clientid].ColoredPlayername(colorNormal)));
                        ServerEventLog(string.Format("{0} {1} joins.", clients[clientid].playername, ip));
                        SendMessage(clientid, colorSuccess + config.WelcomeMessage);
                        SendBlobs(clientid);
                        SendBlockTypes(clientid);
                        SendSunLevels(clientid);
                        SendLightLevels(clientid);
                        SendCraftingRecipes(clientid);

                        //notify all players about new player spawn
                        PacketServerSpawnPlayer p = new PacketServerSpawnPlayer()
                        {
                            PlayerId = clientid,
                            PlayerName = clients[clientid].playername,
                            PositionAndOrientation = new PositionAndOrientation()
                            {
                                X = position.x,
                                Y = position.y + (int)(0.5 * 32),
                                Z = position.z,
                                Heading = 0,
                                Pitch = 0,
                            }
                        };
                        PacketServer pp = new PacketServer() {PacketId = ServerPacketId.SpawnPlayer, SpawnPlayer = p};
                        foreach (var k in clients)
                        {
                            SendPacket(k.Key, Serialize(pp));
                        }

                        //send all players spawn to new player
                        foreach (var k in clients)
                        {
                            if (k.Key != clientid)
                            {
                                p = new PacketServerSpawnPlayer()
                                {
                                    PlayerId = k.Key,
                                    PlayerName = k.Value.playername,
                                    PositionAndOrientation = new PositionAndOrientation()
                                    {
                                        X = k.Value.PositionMul32GlX,
                                        Y = k.Value.PositionMul32GlY,
                                        Z = k.Value.PositionMul32GlZ,
                                        Heading = (byte)k.Value.positionheading,
                                        Pitch = (byte)k.Value.positionpitch,
                                    }
                                };
                                pp = new PacketServer() {PacketId = ServerPacketId.SpawnPlayer, SpawnPlayer = p};
                                SendPacket(clientid, Serialize(pp));
                            }
                        }
                        clients[clientid].state = ClientStateOnServer.LoadingGenerating;
                        NotifySeason(clientid);
                    }
                    break;
                case ClientPacketId.SetBlock:
                    {
                        int x = packet.SetBlock.X;
                        int y = packet.SetBlock.Y;
                        int z = packet.SetBlock.Z;
                        if (!clients[clientid].privileges.Contains(ServerClientMisc.Privilege.build))
                        {
                            SendMessage(clientid, colorError + "Insufficient privileges to build.");
                            SendSetBlock(clientid, x, y, z, d_Map.GetBlock(x, y, z)); //revert
                            break;
                        }
                        if (!config.CanUserBuild(clients[clientid], x, y, z))
                        {
                            SendMessage(clientid, colorError + "You need permission to build in this section of the world.");
                            SendSetBlock(clientid, x, y, z, d_Map.GetBlock(x, y, z)); //revert
                            break;
                        }
                        if (!DoCommandBuild(clientid, true, packet.SetBlock))
                        {
                            SendSetBlock(clientid, x, y, z, d_Map.GetBlock(x, y, z)); //revert
                        }
                        BuildLog(string.Format("{0} {1} {2} {3} {4} {5}", x, y, z, c.playername, ((IPEndPoint)c.socket.RemoteEndPoint).Address.ToString(), d_Map.GetBlock(x, y, z)));
                        d_Water.BlockChange(d_Map, x, y, z);
                        d_GroundPhysics.BlockChange(d_Map, x, y, z);
                    }
                    break;
                case ClientPacketId.FillArea:
                    {
                        if (!clients[clientid].privileges.Contains(ServerClientMisc.Privilege.build))
                        {
                            SendMessage(clientid, colorError + "Insufficient privileges to build.");
                            break;
                        }
                        Vector3i a = new Vector3i(packet.FillArea.X1, packet.FillArea.Y1, packet.FillArea.Z1);
                        Vector3i b = new Vector3i(packet.FillArea.X2, packet.FillArea.Y2, packet.FillArea.Z2);

                        int blockCount = (Math.Abs(a.x - b.x) + 1) * (Math.Abs(a.y - b.y) + 1) * (Math.Abs(a.z - b.z) + 1);

                        if (blockCount > clients[clientid].FillLimit)
                        {
                            SendMessage(clientid, colorError + "Fill area is too large.");
                            break;
                        }
                        if (!this.IsFillAreaValid(clients[clientid], a, b))
                        {
                            SendMessage(clientid, colorError + "Fillarea is invalid or contains blocks in an area you are not allowed to build in.");
                            break;
                        }
                        this.DoFillArea(clientid, packet.FillArea, blockCount);

                        BuildLog(string.Format("{0} {1} {2} - {3} {4} {5} {6} {7} {8}", a.x, a.y, a.z, b.x, b.y, b.z,
                            c.playername, ((IPEndPoint)c.socket.RemoteEndPoint).Address.ToString(),
                            d_Map.GetBlock(a.x, a.y, a.z)));
                    }
                    break;
                case ClientPacketId.PositionandOrientation:
                    {
                        var p = packet.PositionAndOrientation;
                        clients[clientid].PositionMul32GlX = p.X;
                        clients[clientid].PositionMul32GlY = p.Y;
                        clients[clientid].PositionMul32GlZ = p.Z;
                        clients[clientid].positionheading = p.Heading;
                        clients[clientid].positionpitch = p.Pitch;
                        foreach (var k in clients)
                        {
                            if (k.Key != clientid)
                            {
                                SendPlayerTeleport(k.Key, clientid, p.X, p.Y, p.Z, p.Heading, p.Pitch);
                            }
                        }
                    }
                    break;
                case ClientPacketId.Message:
                    {
                        packet.Message.Message = packet.Message.Message.Trim();
                        // server command
                        if (packet.Message.Message.StartsWith("/"))
                        {
                            string[] ss = packet.Message.Message.Split(new[] { ' ' });
                            string command = ss[0].Replace("/", "");
                            string argument = packet.Message.Message.IndexOf(" ") < 0 ? "" : packet.Message.Message.Substring(packet.Message.Message.IndexOf(" ") + 1);
                            this.CommandInterpreter(clientid, command, argument);
                        }
                        // client command
                        else if (packet.Message.Message.StartsWith("."))
                        {
                            break;
                        }
                        // chat message
                        else
                        {
                            if (clients[clientid].privileges.Contains(ServerClientMisc.Privilege.chat))
                            {
                                SendMessageToAll(string.Format("{0}: {1}", clients[clientid].ColoredPlayername(colorNormal), packet.Message.Message));
                                ChatLog(string.Format("{0}: {1}", clients[clientid].playername, packet.Message.Message));
                            }
                            else
                            {
                                SendMessage(clientid, string.Format("{0}Insufficient privileges to chat.", colorError));
                            }
                        }
                    }
                    break;
                case ClientPacketId.Craft:
                    DoCommandCraft(true, packet.Craft);
                    break;
                case ClientPacketId.InventoryAction:
                    DoCommandInventory(clientid, packet.InventoryAction);
                    break;
                case ClientPacketId.Health:
                    {
                        //todo server side
                        var stats = GetPlayerStats(clients[clientid].playername);
                        stats.CurrentHealth = packet.Health.CurrentHealth;
                        if (stats.CurrentHealth < 1)
                        {
                            //death
                            //todo respawn
                            stats.CurrentHealth = stats.MaxHealth;
                        }
                        clients[clientid].IsPlayerStatsDirty = true;
                    }
                    break;
                case ClientPacketId.MonsterHit:
                    HitMonsters(clientid, packet.Health.CurrentHealth);
                    break;
                default:
                    Console.WriteLine("Invalid packet: {0}, clientid:{1}", packet.PacketId, clientid);
                    break;
            }
            return lengthPrefixLength + packetLength;
        }

        private void RunInClientSandbox(string script, int clientid)
        {
            var client = clients[clientid];
            if (!config.AllowScripting)
            {
                SendMessage(clientid, "Server scripts disabled.", MessageType.Error);
                return;
            }
            if (!client.privileges.Contains(ServerClientMisc.Privilege.run))
            {
                SendMessage(clientid, "Insufficient privileges to access this command.", MessageType.Error);
                return;
            }
            ServerEventLog(string.Format("{0} runs script:\n{1}", clients[clientid].playername, script));
            if (client.Interpreter == null)
            {
                client.Interpreter = new JavaScriptInterpreter();
                client.Console = new ScriptConsole(this, clientid);
                client.Console.InjectConsoleCommands(client.Interpreter);
                client.Interpreter.SetVariables(new Dictionary<string, object>() { { "client", client }, { "server", this }, });
                client.Interpreter.Execute("function inspect(obj) { for( property in obj) { out(property)}}");
            }
            var interpreter = client.Interpreter;
            object result;
            SendMessage(clientid, colorNormal + script);
            if (interpreter.Execute(script, out result))
            {
                try
                {
                    SendMessage(clientid, colorSuccess + " => " + result);
                }
                catch (FormatException e) // can happen
                {
                    SendMessage(clientid, colorError + "Error. " + e.Message);
                }
                return;
            }
            SendMessage(clientid, colorError + "Error.");
        }

        string colorNormal = "&f"; //white
        string colorHelp = "&4"; //red
        string colorOpUsername = "&2"; //green
        string colorSuccess = "&2"; //green
        string colorError = "&4"; //red
        string colorImportant = "&4"; // red
        string colorAdmin = "&e"; //yellow
        public enum MessageType { Normal, Important, Help, OpUsername, Success, Error, Admin, White, Red, Green, Yellow }
        private string MessageTypeToString(MessageType type)
        {
            switch (type)
            {
                case MessageType.Normal:
                case MessageType.White:
                    return colorNormal;
                case MessageType.Important:
                    return colorImportant;
                case MessageType.Help:
                case MessageType.Red:
                    return colorHelp;
                case MessageType.OpUsername:
                case MessageType.Green:
                    return colorOpUsername;
                case MessageType.Error:
                    return colorError;
                case MessageType.Success:
                    return colorSuccess;
                case MessageType.Admin:
                case MessageType.Yellow:
                    return colorAdmin;
                default:
                    return colorNormal;
            }
        }
        bool CompareByteArray(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) { return false; }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) { return false; }
            }
            return true;
        }
        private void NotifyBlock(int x, int y, int z, int blocktype)
        {
            foreach (var k in clients)
            {
                SendSetBlock(k.Key, x, y, z, blocktype);
            }
        }
        bool ENABLE_FINITEINVENTORY { get { return !config.IsCreative; } }
        private bool DoCommandCraft(bool execute, PacketClientCraft cmd)
        {
            if (d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z) != (int)TileTypeManicDigger.CraftingTable)
            {
                return false;
            }
            if (cmd.RecipeId < 0 || cmd.RecipeId >= craftingrecipes.Count)
            {
                return false;
            }
            List<Vector3i> table = d_CraftingTableTool.GetTable(new Vector3i(cmd.X, cmd.Y, cmd.Z));
            List<int> ontable = d_CraftingTableTool.GetOnTable(table);
            List<int> outputtoadd = new List<int>();
            //for (int i = 0; i < craftingrecipes.Count; i++)
            int i = cmd.RecipeId;
            {
                //try apply recipe. if success then try until fail.
                for (; ; )
                {
                    //check if ingredients available
                    foreach (Ingredient ingredient in craftingrecipes[i].ingredients)
                    {
                        if (ontable.FindAll(v => v == ingredient.Type).Count < ingredient.Amount)
                        {
                            goto nextrecipe;
                        }
                    }
                    //remove ingredients
                    foreach (Ingredient ingredient in craftingrecipes[i].ingredients)
                    {
                        for (int ii = 0; ii < ingredient.Amount; ii++)
                        {
                            //replace on table
                            ReplaceOne(ontable, ingredient.Type, (int)TileTypeManicDigger.Empty);
                        }
                    }
                    //add output
                    for (int z = 0; z < craftingrecipes[i].output.Amount; z++)
                    {
                        outputtoadd.Add(craftingrecipes[i].output.Type);
                    }
                }
            nextrecipe:
                ;
            }
            foreach (var v in outputtoadd)
            {
                ReplaceOne(ontable, (int)TileTypeManicDigger.Empty, v);
            }
            int zz = 0;
            if (execute)
            {
                foreach (var v in table)
                {
                    SetBlockAndNotify(v.x, v.y, v.z + 1, ontable[zz]);
                    zz++;
                }
            }
            return true;
        }
        private void ReplaceOne<T>(List<T> l, T from, T to)
        {
            for (int ii = 0; ii < l.Count; ii++)
            {
                if (l[ii].Equals(from))
                {
                    l[ii] = to;
                    break;
                }
            }
        }
        public IGameDataItems d_DataItems;
        InventoryUtil GetInventoryUtil(Inventory inventory)
        {
            InventoryUtil util = new InventoryUtil();
            util.d_Inventory = inventory;
            util.d_Items = d_DataItems;
            return util;
        }
        private void DoCommandInventory(int player_id, PacketClientInventoryAction cmd)
        {
            Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
            var s = new InventoryServer();
            s.d_Inventory = inventory;
            s.d_InventoryUtil = GetInventoryUtil(inventory);
            s.d_Items = d_DataItems;
            s.d_DropItem = this;

            switch (cmd.Action)
            {
                case InventoryActionType.Click:
                    s.InventoryClick(cmd.A);
                    break;
                case InventoryActionType.MoveToInventory:
                    s.MoveToInventory(cmd.A);
                    break;
                case InventoryActionType.WearItem:
                    s.WearItem(cmd.A, cmd.B);
                    break;
                default:
                    break;
            }
            clients[player_id].IsInventoryDirty = true;
            NotifyInventory(player_id);
        }

        private bool IsFillAreaValid(Client client, Vector3i a, Vector3i b)
        {
            if (!MapUtil.IsValidPos(this.d_Map, a.x, a.y, a.z) || !MapUtil.IsValidPos(this.d_Map, b.x, b.y, b.z))
            {
                return false;
            }

            // TODO: Is there a more efficient way?
            int startx = Math.Min(a.x, b.x);
            int endx = Math.Max(a.x, b.x);
            int starty = Math.Min(a.y, b.y);
            int endy = Math.Max(a.y, b.y);
            int startz = Math.Min(a.z, b.z);
            int endz = Math.Max(a.z, b.z);
            for (int x = startx; x <= endx; x++)
            {
                for (int y = starty; y <= endy; y++)
                {
                    for (int z = startz; z <= endz; z++)
                    {
                        if (!config.CanUserBuild(client, x, y, z))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private bool DoFillArea(int player_id, PacketClientFillArea fill, int blockCount)
        {
            Vector3i a = new Vector3i(fill.X1, fill.Y1, fill.Z1);
            Vector3i b = new Vector3i(fill.X2, fill.Y2, fill.Z2);

            int startx = Math.Min(a.x, b.x);
            int endx = Math.Max(a.x, b.x);
            int starty = Math.Min(a.y, b.y);
            int endy = Math.Max(a.y, b.y);
            int startz = Math.Min(a.z, b.z);
            int endz = Math.Max(a.z, b.z);

            int blockType = fill.BlockType;
            if (blockType == (int)TileTypeManicDigger.FillArea)
            {
                blockType = SpecialBlockId.Empty;
            }
            blockType = d_Data.WhenPlayerPlacesGetsConvertedTo[blockType];

            Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
            var item = inventory.RightHand[fill.MaterialSlot];
            if (item == null)
            {
                return false;
            }

            // crafting mode
            if (!config.IsCreative)
            {
                // grab items
                if (blockType == SpecialBlockId.Empty)
                {
                    var newItem = new Item();
                    newItem.ItemClass = ItemClass.Block;
                    for (int x = startx; x <= endx; x++)
                    {
                        for (int y = starty; y <= endy; y++)
                        {
                            for (int z = startz; z <= endz; z++)
                            {
                                newItem.BlockId = d_Data.WhenPlayerPlacesGetsConvertedTo[d_Map.GetBlock(x, y, z)];
                                GetInventoryUtil(inventory).GrabItem(newItem, fill.MaterialSlot);
                                d_Map.SetBlockNotMakingDirty(x, y, z, blockType);
                            }
                        }
                    }
                }
                // place and lose items
                else
                {
                    int newBlockCount = 0;
                    var newItem = new Item();
                    newItem.ItemClass = ItemClass.Block;
                    // Anon-method here to break out of the nested loop instead of using goto.
                    Jint.Delegates.Action fillArea = delegate
                    {
                        for (int x = startx; x <= endx; ++x)
                        {
                            for (int y = starty; y <= endy; ++y)
                            {
                                for (int z = startz; z <= endz; ++z)
                                {
                                    // Player got out of blocks. Stop fill here.
                                    if (item.BlockCount == 0)
                                    {
                                        inventory.RightHand[fill.MaterialSlot] = null;
                                        return;
                                    }
                                    // Grab item before replacing it with new block.
                                    newItem.BlockId = d_Data.WhenPlayerPlacesGetsConvertedTo[d_Map.GetBlock(x, y, z)];
                                    GetInventoryUtil(inventory).GrabItem(newItem, fill.MaterialSlot);
                                    item.BlockCount--;
                                    newBlockCount++;
                                    d_Map.SetBlockNotMakingDirty(x, y, z, blockType);
                                }
                            }
                        }
                    };
                    fillArea();
                    blockCount = newBlockCount;
                }
                clients[player_id].IsInventoryDirty = true;
                NotifyInventory(player_id);
            }
            // creative mode
            else
            {
                for (int x = startx; x <= endx; x++)
                {
                    for (int y = starty; y <= endy; y++)
                    {
                        for (int z = startz; z <= endz; z++)
                        {
                            d_Map.SetBlockNotMakingDirty(x, y, z, blockType);
                        }
                    }
                }
            }

            // notify clients
            foreach (var k in clients)
            {
                SendFillArea(k.Key, a, b, blockType, blockCount);
            }
            return true;
        }
        bool ClientSeenChunk(int clientid, int vx, int vy, int vz)
        {
            int pos = MapUtil.Index3d(vx / chunksize, vy / chunksize, vz / chunksize, d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize);
            return clients[clientid].chunksseen[pos];
        }
        void ClientSeenChunkSet(int clientid, int vx, int vy, int vz, int time)
        {
            int pos = MapUtil.Index3d(vx / chunksize, vy / chunksize, vz / chunksize, d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize);
            clients[clientid].chunksseen[pos] = true;
            clients[clientid].chunksseenTime[pos] = time;
        }
        private void SendFillArea(int clientid, Vector3i a, Vector3i b, int blockType, int blockCount)
        {
            // TODO: better to send a chunk?

            Vector3i v = new Vector3i((a.x / chunksize) * chunksize,
                (a.y / chunksize) * chunksize, (a.z / chunksize) * chunksize);
            Vector3i w = new Vector3i((b.x / chunksize) * chunksize,
                (b.y / chunksize) * chunksize, (b.z / chunksize) * chunksize);

            // TODO: Is it sufficient to regard only start- and endpoint?
            if (!ClientSeenChunk(clientid, v.x, v.y, v.z) && !ClientSeenChunk(clientid, w.x, w.y, w.z))
            {
                return;
            }

            PacketServerFillArea p = new PacketServerFillArea()
            {
                X1 = a.x,
                Y1 = a.y,
                Z1 = a.z,
                X2 = b.x,
                Y2 = b.y,
                Z2 = b.z,
                BlockType = blockType,
                BlockCount = blockCount
            };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.FillArea, FillArea = p }));
        }
        private void SendFillAreaLimit(int clientid, int limit)
        {
            PacketServerFillAreaLimit p = new PacketServerFillAreaLimit()
            {
                Limit = limit
            };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.FillAreaLimit, FillAreaLimit = p }));
        }

        private bool DoCommandBuild(int player_id, bool execute, PacketClientSetBlock cmd)
        {
            Vector3 v = new Vector3(cmd.X, cmd.Y, cmd.Z);
            Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
            if (cmd.Mode == BlockSetMode.Use)
            {
                UseDoor(cmd.X, cmd.Y, cmd.Z);
                if (d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z) == (int)TileTypeManicDigger.TNT)
                {
                    if (!clients[player_id].privileges.Contains(ServerClientMisc.Privilege.use_tnt))
                    {
                        SendMessage(player_id, colorError + "Insufficient privileges to use TNT.");
                        return false;
                    }
                    UseTnt(cmd.X, cmd.Y, cmd.Z);
                }
                return true;
            }
            if (cmd.Mode == BlockSetMode.Create
                && d_Data.Rail[cmd.BlockType] != 0)
            {
                return DoCommandBuildRail(player_id, execute, cmd);
            }
            if (cmd.Mode == (int)BlockSetMode.Destroy
                && d_Data.Rail[d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z)] != 0)
            {
                return DoCommandRemoveRail(player_id, execute, cmd);
            }
            if (cmd.Mode == BlockSetMode.Create)
            {
                int oldblock = d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z);
                if (!(oldblock == 0 || d_Data.IsFluid[oldblock]))
                {
                    return false;
                }
                var item = inventory.RightHand[cmd.MaterialSlot];
                if (item == null)
                {
                    return false;
                }
                switch (item.ItemClass)
                {
                    case ItemClass.Block:
                        item.BlockCount--;
                        if (item.BlockCount == 0)
                        {
                            inventory.RightHand[cmd.MaterialSlot] = null;
                        }
                        if (d_Data.Rail[item.BlockId] != 0)
                        {
                        }
                        if (item.BlockId == (int)TileTypeManicDigger.DoorBottomClosed)
                        {
                            if (d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z + 1) != 0)
                            {
                                return false;
                            }
                            SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z + 1, (int)TileTypeManicDigger.DoorTopClosed);
                        }
                        SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, item.BlockId);
                        break;
                    default:
                        //TODO
                        return false;
                }
            }
            else
            {
                var item = new Item();
                item.ItemClass = ItemClass.Block;
                int blockid = d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z);
                item.BlockId = d_Data.WhenPlayerPlacesGetsConvertedTo[blockid];
                if (!config.IsCreative)
                {
                    GetInventoryUtil(inventory).GrabItem(item, cmd.MaterialSlot);
                }
                SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, SpecialBlockId.Empty);
                if (GameDataManicDigger.IsDoorTile(blockid) && GameDataManicDigger.IsDoorTile(d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z + 1)))
                {
                    SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z + 1, SpecialBlockId.Empty);
                }
                if (GameDataManicDigger.IsDoorTile(blockid) && GameDataManicDigger.IsDoorTile(d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z - 1)))
                {
                    SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z - 1, SpecialBlockId.Empty);
                }
            }
            clients[player_id].IsInventoryDirty = true;
            NotifyInventory(player_id);
            return true;
        }

        private void UseDoor(int x, int y, int z)
        {
            if (GameDataManicDigger.IsDoorTile(d_Map.GetBlock(x, y, z)))
            {
                for (int zz = -1; zz < 2; zz++)
                {
                    if (d_Map.GetBlock(x, y, z + zz) == (int)TileTypeManicDigger.DoorBottomClosed)
                    {
                        SetBlockAndNotify(x, y, z + zz, (int)TileTypeManicDigger.DoorBottomOpen);
                    }
                    else if (d_Map.GetBlock(x, y, z + zz) == (int)TileTypeManicDigger.DoorBottomOpen)
                    {
                        SetBlockAndNotify(x, y, z + zz, (int)TileTypeManicDigger.DoorBottomClosed);
                    }
                    if (d_Map.GetBlock(x, y, z + zz) == (int)TileTypeManicDigger.DoorTopClosed)
                    {
                        SetBlockAndNotify(x, y, z + zz, (int)TileTypeManicDigger.DoorTopOpen);
                    }
                    else if (d_Map.GetBlock(x, y, z + zz) == (int)TileTypeManicDigger.DoorTopOpen)
                    {
                        SetBlockAndNotify(x, y, z + zz, (int)TileTypeManicDigger.DoorTopClosed);
                    }
                }
            }
        }

        private void UseTnt(int x, int y, int z)
        {
            if (d_Map.GetBlock(x, y, z) == (int)TileTypeManicDigger.TNT)
            {
                if (tntStack.Count < tntMax)
                {
                    tntStack.Push(new Vector3i(x, y, z));
                }
            }
        }

        private bool DoCommandBuildRail(int player_id, bool execute, PacketClientSetBlock cmd)
        {
            Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
            int oldblock = d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z);
            //int blockstoput = 1;
            if (!(oldblock == SpecialBlockId.Empty || GameDataManicDigger.IsRailTile(oldblock)))
            {
                return false;
            }

            //count how many rails will be created
            int oldrailcount = 0;
            if (GameDataManicDigger.IsRailTile(oldblock))
            {
                oldrailcount = MyLinq.Count(
                    DirectionUtils.ToRailDirections(
                    (RailDirectionFlags)(oldblock - GameDataManicDigger.railstart)));
            }
            int newrailcount = MyLinq.Count(
                DirectionUtils.ToRailDirections(
                (RailDirectionFlags)(cmd.BlockType - GameDataManicDigger.railstart)));
            int blockstoput = newrailcount - oldrailcount;

            Item item = inventory.RightHand[cmd.MaterialSlot];
            if (!(item.ItemClass == ItemClass.Block && d_Data.Rail[item.BlockId] != 0))
            {
                return false;
            }
            item.BlockCount -= blockstoput;
            if (item.BlockCount == 0)
            {
                inventory.RightHand[cmd.MaterialSlot] = null;
            }
            SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, cmd.BlockType);

            clients[player_id].IsInventoryDirty = true;
            NotifyInventory(player_id);
            return true;
        }

        private bool DoCommandRemoveRail(int player_id, bool execute, PacketClientSetBlock cmd)
        {
            Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
            //add to inventory
            int blocktype = d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z);
            blocktype = d_Data.WhenPlayerPlacesGetsConvertedTo[blocktype];
            if ((!d_Data.IsValid[blocktype])
                || blocktype == SpecialBlockId.Empty)
            {
                return false;
            }
            int blockstopick = 1;
            if (GameDataManicDigger.IsRailTile(blocktype))
            {
                blockstopick = MyLinq.Count(
                    DirectionUtils.ToRailDirections(
                    (RailDirectionFlags)(blocktype - GameDataManicDigger.railstart)));
            }

            var item = new Item();
            item.ItemClass = ItemClass.Block;
            item.BlockId = d_Data.WhenPlayerPlacesGetsConvertedTo[blocktype];
            item.BlockCount = blockstopick;
            if (!config.IsCreative)
            {
                GetInventoryUtil(inventory).GrabItem(item, cmd.MaterialSlot);
            }
            SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, SpecialBlockId.Empty);

            clients[player_id].IsInventoryDirty = true;
            NotifyInventory(player_id);
            return true;
        }
        void SetBlockAndNotify(int x, int y, int z, int blocktype)
        {
            d_Map.SetBlockNotMakingDirty(x, y, z, blocktype);
            NotifyBlock(x, y, z, blocktype);
        }
        private int TotalAmount(Dictionary<int, int> inventory)
        {
            int sum = 0;
            foreach (var k in inventory)
            {
                sum += k.Value;
            }
            return sum;
        }
        private void RemoveEquivalent(Dictionary<int, int> inventory, int blocktype, int count)
        {
            int removed = 0;
            for (int i = 0; i < count; i++)
            {
                foreach (var k in new Dictionary<int, int>(inventory))
                {
                    if (EquivalentBlock(k.Key, blocktype)
                        && k.Value > 0)
                    {
                        inventory[k.Key]--;
                        removed++;
                        goto removenext;
                    }
                }
            removenext:
                ;
            }
            if (removed != count)
            {
                //throw new Exception();
            }
        }
        private int GetEquivalentCount(Dictionary<int, int> inventory, int blocktype)
        {
            int count = 0;
            foreach (var k in inventory)
            {
                if (EquivalentBlock(k.Key, blocktype))
                {
                    count += k.Value;
                }
            }
            return count;
        }
        private bool EquivalentBlock(int blocktypea, int blocktypeb)
        {
            if (GameDataManicDigger.IsRailTile(blocktypea) && GameDataManicDigger.IsRailTile(blocktypeb))
            {
                return true;
            }
            return blocktypea == blocktypeb;
        }
        private byte[] Serialize(PacketServer p)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.SerializeWithLengthPrefix(ms, p, PrefixStyle.Base128);
            return ms.ToArray();
        }
        private string GenerateUsername(string name)
        {
            string defaultname = name;
            int appendNumber = 1;
            bool exists;

            do
            {
                exists = false;
                foreach (var k in clients)
                {
                    if (k.Value.playername.Equals(defaultname + appendNumber))
                    {
                        exists = true;
                        appendNumber++;
                        break;
                    }
                }
            } while (exists);

            return defaultname + appendNumber;
        }
        public void ServerMessageToAll(string message, MessageType color)
        {
            this.SendMessageToAll(MessageTypeToString(color) + message);
            ServerEventLog(string.Format("SERVER MESSAGE: {0}.", message));
        }
        private void SendMessageToAll(string message)
        {
            Console.WriteLine("Message to all: " + message);
            foreach (var k in clients)
            {
                SendMessage(k.Key, message);
            }
        }
        private void SendSetBlock(int clientid, int x, int y, int z, int blocktype)
        {
            if (!ClientSeenChunk(clientid, (x / chunksize) * chunksize,
                (y / chunksize) * chunksize, (z / chunksize) * chunksize))
            {
                return;
            }
            PacketServerSetBlock p = new PacketServerSetBlock() { X = x, Y = y, Z = z, BlockType = blocktype };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.SetBlock, SetBlock = p }));
        }
        private void SendSound(int clientid, string name)
        {
            PacketServerSound p = new PacketServerSound() { Name = name };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.Sound, Sound = p }));
        }
        private void SendPlayerSpawnPosition(int clientid, int x, int y, int z)
        {
            PacketServerPlayerSpawnPosition p = new PacketServerPlayerSpawnPosition()
            {
                X = x,
                Y = y,
                Z = z
            };
            SendPacket(clientid, Serialize(new PacketServer()
            {
                PacketId = ServerPacketId.PlayerSpawnPosition,
                PlayerSpawnPosition = p,
            }));
        }
        private void SendPlayerTeleport(int clientid, int playerid, int x, int y, int z, byte heading, byte pitch)
        {
            PacketServerPositionAndOrientation p = new PacketServerPositionAndOrientation()
            {
                PlayerId = playerid,
                PositionAndOrientation = new PositionAndOrientation()
                {
                    X = x,
                    Y = y,
                    Z = z,
                    Heading = heading,
                    Pitch = pitch,
                }
            };
            SendPacket(clientid, Serialize(new PacketServer()
            {
                PacketId = ServerPacketId.PlayerPositionAndOrientation,
                PositionAndOrientation = p,
            }));
        }
        //SendPositionAndOrientationUpdate //delta
        //SendPositionUpdate //delta
        //SendOrientationUpdate
        private void SendDespawnPlayer(int clientid, int playerid)
        {
            PacketServerDespawnPlayer p = new PacketServerDespawnPlayer() { PlayerId = playerid };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.DespawnPlayer, DespawnPlayer = p }));
        }

        public void SendMessage(int clientid, string message, MessageType color)
        {
            SendMessage(clientid, MessageTypeToString(color) + message);
        }
        private void SendMessage(int clientid, string message)
        {
            if (clientid == this.serverConsoleId)
            {
                this.serverConsole.Receive(message);
                return;
            }

            string truncated = message; //.Substring(0, Math.Min(64, message.Length));

            PacketServerMessage p = new PacketServerMessage();
            p.PlayerId = clientid;
            p.Message = truncated;
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.Message, Message = p }));
        }
        private void SendDisconnectPlayer(int clientid, string disconnectReason)
        {
            PacketServerDisconnectPlayer p = new PacketServerDisconnectPlayer() { DisconnectReason = disconnectReason };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.DisconnectPlayer, DisconnectPlayer = p }));
        }
        public void SendPacket(int clientid, byte[] packet)
        {
            try
            {
                //if (IsMono)
                {
                    clients[clientid].socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, EmptyCallback, new object());
                }
                //commented out because SocketAsyncEventArgs
                //doesn't work on Mono and .NET Framework 2.0 without Service Pack
                //is Socket.SendAsync() better than BeginSend()?
                //else
                //{
                //    using (SocketAsyncEventArgs e = new SocketAsyncEventArgs())
                //    {
                //        e.SetBuffer(packet, 0, packet.Length);
                //        clients[clientid].socket.SendAsync(e);
                //    }
                //}
            }
            catch (Exception e)
            {
                KillPlayer(clientid);
            }
        }
        void EmptyCallback(IAsyncResult result)
        {
        }
        int drawdistance = 128;
        public const int chunksize = 32;
        int chunkdrawdistance { get { return drawdistance / chunksize; } }
        IEnumerable<Vector3i> ChunksAroundPlayer(Vector3i playerpos)
        {
            playerpos.x = (playerpos.x / chunksize) * chunksize;
            playerpos.y = (playerpos.y / chunksize) * chunksize;
            for (int x = -chunkdrawdistance; x <= chunkdrawdistance; x++)
            {
                for (int y = -chunkdrawdistance; y <= chunkdrawdistance; y++)
                {
                    for (int z = 0; z < d_Map.MapSizeZ / chunksize; z++)
                    {
                        var p = new Vector3i(playerpos.x + x * chunksize, playerpos.y + y * chunksize, z * chunksize);
                        if (MapUtil.IsValidPos(d_Map, p.x, p.y, p.z))
                        {
                            yield return p;
                        }
                    }
                }
            }
        }
        byte[] CompressChunkNetwork(byte[] chunk)
        {
            return d_NetworkCompression.Compress(chunk);
        }
        byte[] CompressChunkNetwork(byte[, ,] chunk)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            for (int z = 0; z <= chunk.GetUpperBound(2); z++)
            {
                for (int y = 0; y <= chunk.GetUpperBound(1); y++)
                {
                    for (int x = 0; x <= chunk.GetUpperBound(0); x++)
                    {
                        bw.Write((byte)chunk[x, y, z]);
                    }
                }
            }
            byte[] compressedchunk = d_NetworkCompression.Compress(ms.ToArray());
            return compressedchunk;
        }
        struct PublicFile
        {
            public string Name;
            public byte[] Data;
        }
        List<PublicFile> PublicFiles()
        {
            List<PublicFile> files = new List<PublicFile>();
            foreach (string path in PublicDataPaths)
            {
                try
                {
                    if (!Directory.Exists(path))
                    {
                        continue;
                    }
                    foreach (string s in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            FileInfo f = new FileInfo(s);
                            if ((f.Attributes & FileAttributes.Hidden) != 0)
                            {
                                continue;
                            }
                            //cache[f.Name] = File.ReadAllBytes(s);
                            files.Add(new PublicFile()
                            {
                                Name = f.Name,
                                Data = File.ReadAllBytes(s),
                            });
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }
            return files;
        }
        int BlobPartLength = 1024 * 4;
        private void SendBlobs(int clientid)
        {
            SendLevelInitialize(clientid);

            List<PublicFile> files = PublicFiles();
            for (int i = 0; i < files.Count; i++)
            {
                PublicFile f = files[i];
                SendBlobInitialize(clientid, f.Name == "terrain.png" ? terrainTextureMd5 : new byte[16], f.Name);
                byte[] blob = f.Data;
                int totalsent = 0;
                foreach (byte[] part in Parts(blob, BlobPartLength))
                {
                    SendLevelProgress(clientid,
                        (int)(((float)i / files.Count
                        + ((float)totalsent / blob.Length) / files.Count) * 100), "Downloading data...");
                    SendBlobPart(clientid, part);
                    totalsent += part.Length;
                }
                SendBlobFinalize(clientid);
            }
            SendLevelProgress(clientid, 0, "Generating world...");
        }
        static IEnumerable<byte[]> Parts(byte[] blob, int partsize)
        {
            int i = 0;
            for (; ; )
            {
                if (i >= blob.Length) { break; }
                int curpartsize = blob.Length - i;
                if (curpartsize > partsize) { curpartsize = partsize; }
                byte[] part = new byte[curpartsize];
                for (int ii = 0; ii < curpartsize; ii++)
                {
                    part[ii] = blob[i + ii];
                }
                yield return part;
                i += curpartsize;
            }
        }
        private void SendBlobInitialize(int clientid, byte[] hash, string name)
        {
            PacketServerBlobInitialize p = new PacketServerBlobInitialize() { hash = hash, name = name };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.BlobInitialize, BlobInitialize = p }));
        }
        private void SendBlobPart(int clientid, byte[] data)
        {
            PacketServerBlobPart p = new PacketServerBlobPart() { data = data };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.BlobPart, BlobPart = p }));
        }
        private void SendBlobFinalize(int clientid)
        {
            PacketServerBlobFinalize p = new PacketServerBlobFinalize() { };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.BlobFinalize, BlobFinalize = p }));
        }
        public BlockType[] BlockTypes = new BlockType[256];
        private void SendBlockTypes(int clientid)
        {
            PacketServerBlockTypes p = new PacketServerBlockTypes() { blocktypes = BlockTypes };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.BlockTypes, BlockTypes = p }));
        }
        private void SendSunLevels(int clientid)
        {
            PacketServerSunLevels p = new PacketServerSunLevels() { sunlevels = sunlevels };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.SunLevels, SunLevels = p }));
        }
        private void SendLightLevels(int clientid)
        {
            PacketServerLightLevels p = new PacketServerLightLevels() { lightlevels = lightlevels };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.LightLevels, LightLevels = p }));
        }
        private void SendCraftingRecipes(int clientid)
        {
            PacketServerCraftingRecipes p = new PacketServerCraftingRecipes() { CraftingRecipes = craftingrecipes.ToArray() };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.CraftingRecipes, CraftingRecipes = p }));
        }
        private void SendLevelInitialize(int clientid)
        {
            PacketServerLevelInitialize p = new PacketServerLevelInitialize() { };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.LevelInitialize, LevelInitialize = p }));
        }
        private void SendLevelProgress(int clientid, int percentcomplete, string status)
        {
            PacketServerLevelProgress p = new PacketServerLevelProgress() { PercentComplete = percentcomplete, Status = status };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.LevelDataChunk, LevelDataChunk = p }));
        }
        private void SendLevelFinalize(int clientid)
        {
            PacketServerLevelFinalize p = new PacketServerLevelFinalize() { };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.LevelFinalize, LevelFinalize = p }));
        }
        private void SendServerIdentification(int clientid)
        {
            PacketServerIdentification p = new PacketServerIdentification()
            {
                MdProtocolVersion = GameVersion.Version,
                AssignedClientId = clientid,
                ServerName = config.Name,
                ServerMotd = config.Motd,
                UsedBlobsMd5 = new List<byte[]>(new[] { terrainTextureMd5 }),
                TerrainTextureMd5 = terrainTextureMd5,
                DisallowFreemove = !config.AllowFreemove,
                MapSizeX = d_Map.MapSizeX,
                MapSizeY = d_Map.MapSizeY,
                MapSizeZ = d_Map.MapSizeZ,
            };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.ServerIdentification, Identification = p }));
        }
        byte[] terrainTextureMd5 { get { byte[] b = new byte[16]; b[0] = 1; return b; } }
        MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] ComputeMd5(byte[] b)
        {
            return md5.ComputeHash(b);
        }
        string ComputeMd5(string input)
        {
            // step 1, calculate MD5 hash from input
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
        public int SIMULATION_KEYFRAME_EVERY = 4;
        public float SIMULATION_STEP_LENGTH = 1f / 64f;
        public struct BlobToSend
        {
            public string Name;
            public byte[] Data;
        }
        public enum ClientStateOnServer
        {
            Connecting,
            LoadingGenerating,
            LoadingSending,
            Playing,
        }
        public class Client
        {
            public int Id = -1;
            public ClientStateOnServer state = ClientStateOnServer.Connecting;
            public int maploadingsentchunks = 0;
            public ISocket socket;
            public List<byte> received = new List<byte>();
            public Ping Ping = new Ping();
            public string playername = invalidplayername;
            public int PositionMul32GlX;
            public int PositionMul32GlY;
            public int PositionMul32GlZ;
            public int positionheading;
            public int positionpitch;
            public Dictionary<int, int> chunksseenTime = new Dictionary<int, int>();
            public bool[] chunksseen;
            public Dictionary<Vector2i, int> heightmapchunksseen = new Dictionary<Vector2i, int>();
            public ManicDigger.Timer notifyMapTimer;
            public bool IsInventoryDirty = true;
            public bool IsPlayerStatsDirty = true;
            public int FillLimit = 500;
            //public List<byte[]> blobstosend = new List<byte[]>();
            public GameModeFortress.Group clientGroup;
            public void AssignGroup(GameModeFortress.Group newGroup)
            {
                this.clientGroup = newGroup;
                this.privileges.Clear();
                this.privileges.AddRange(newGroup.GroupPrivileges);
                this.color = newGroup.GroupColorString();
            }
            public List<ServerClientMisc.Privilege> privileges = new List<ServerClientMisc.Privilege>();
            public string color;
            public string ColoredPlayername(string subsequentColor)
            {
                return this.color + this.playername + subsequentColor;
            }
            public ManicDigger.Timer notifyMonstersTimer;
            public IScriptInterpreter Interpreter;
            public ScriptConsole Console;

            public override string ToString()
            {
                string ip = "";
                if (this.socket != null)
                {
                    ip = ((IPEndPoint)this.socket.RemoteEndPoint).Address.ToString();
                }
                // Format: Playername:Group:Privileges IP
                return string.Format("{0}:{1}:{2} {3}", this.playername, this.clientGroup.Name,
                    ServerClientMisc.PrivilegesString(this.privileges), ip);
            }
        }
        Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public Client GetClient(int id)
        {
            if (id == this.serverConsoleId)
            {
                return this.serverConsoleClient;
            }
            if (!clients.ContainsKey(id))
                return null;
            return clients[id];
        }
        public Client GetClient(string name)
        {
            if (serverConsoleClient.playername.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            {
                return serverConsoleClient;
            }
            foreach (var k in clients)
            {
                if (k.Value.playername.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return k.Value;
                }
            }
            return null;
        }

        public ServerClient serverClient;
        public GameModeFortress.Group defaultGroupGuest;
        public GameModeFortress.Group defaultGroupRegistered;
        public Vector3i defaultPlayerSpawn;

        private Vector3i SpawnToVector3i(GameModeFortress.Spawn spawn)
        {
            int x = spawn.x;
            int y = spawn.y;
            int z;
            if (!MapUtil.IsValidPos(d_Map, x, y))
            {
                throw new Exception("Invalid default spawn coordinates!");
            }

            if (spawn.z == null)
            {
                z = MapUtil.blockheight(d_Map, 0, x, y);
            }
            else
            {
                z = spawn.z.Value;
                if (!MapUtil.IsValidPos(d_Map, x, y, z))
                {
                    throw new Exception("Invalid default spawn coordinates!");
                }
            }
            return new Vector3i(x * 32, z * 32, y * 32);
        }

        public void LoadServerClient()
        {
            string filename = "ServerClient.xml";
            if (!File.Exists(Path.Combine(GameStorePath.gamepathconfig, filename)))
            {
                Console.WriteLine("Server client configuration file not found, creating new.");
                SaveServerClient();
            }
            else
            {
                try
                {
                    using (TextReader textReader = new StreamReader(Path.Combine(GameStorePath.gamepathconfig, filename)))
                    {
                        XmlSerializer deserializer = new XmlSerializer(typeof(ServerClient));
                        serverClient = (ServerClient)deserializer.Deserialize(textReader);
                        textReader.Close();
                        serverClient.Groups.Sort();
                        SaveServerClient();
                    }
                }
                catch //This if for the original format
                {
                    using (Stream s = new MemoryStream(File.ReadAllBytes(Path.Combine(GameStorePath.gamepathconfig, filename))))
                    {
                        serverClient = new ServerClient();
                        StreamReader sr = new StreamReader(s);
                        XmlDocument d = new XmlDocument();
                        d.Load(sr);
                        serverClient.Format = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerClient/Format"));
                        serverClient.DefaultGroupGuests = XmlTool.XmlVal(d, "/ManicDiggerServerClient/DefaultGroupGuests");
                        serverClient.DefaultGroupRegistered = XmlTool.XmlVal(d, "/ManicDiggerServerClient/DefaultGroupRegistered");
                    }
                    //Save with new version.
                    SaveServerClient();
                }
            }
            if (serverClient.DefaultSpawn == null)
            {
                // server sets a default spawn (middle of map)
                int x = d_Map.MapSizeX / 2;
                int y = d_Map.MapSizeY / 2;
                this.defaultPlayerSpawn =  DontSpawnPlayerInWater(new Vector3i(x, y, MapUtil.blockheight(d_Map, 0, x, y)));
            }
            else
            {
                this.defaultPlayerSpawn = this.SpawnToVector3i(serverClient.DefaultSpawn);
            }

            this.defaultGroupGuest = serverClient.Groups.Find(
                delegate(GameModeFortress.Group grp)
                {
                    return grp.Name.Equals(serverClient.DefaultGroupGuests);
                }
            );
            if (this.defaultGroupGuest == null)
            {
                throw new Exception("Default guest group not found!");
            }
            this.defaultGroupRegistered = serverClient.Groups.Find(
                delegate(GameModeFortress.Group grp)
                {
                    return grp.Name.Equals(serverClient.DefaultGroupRegistered);
                }
            );
            if (this.defaultGroupRegistered == null)
            {
                throw new Exception("Default registered group not found!");
            }
            Console.WriteLine("Server client configuration loaded.");
        }

        public void SaveServerClient()
        {
            //Verify that we have a directory to place the file into.
            if (!Directory.Exists(GameStorePath.gamepathconfig))
            {
                Directory.CreateDirectory(GameStorePath.gamepathconfig);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(ServerClient));
            TextWriter textWriter = new StreamWriter(Path.Combine(GameStorePath.gamepathconfig, "ServerClient.xml"));

            //Check to see if config has been initialized
            if (serverClient == null)
            {
                serverClient = new ServerClient();
            }
            if (serverClient.Groups.Count == 0)
            {
                serverClient.Groups = ServerClientMisc.getDefaultGroups();
            }
            if (serverClient.Clients.Count == 0)
            {
                serverClient.Clients = ServerClientMisc.getDefaultClients();
            }
            //Serialize the ServerConfig class to XML
            serializer.Serialize(textWriter, serverClient);
            textWriter.Close();
        }

        public int dumpmax = 30;
        public void DropItem(ref Item item, Vector3i pos)
        {
            switch (item.ItemClass)
            {
                case ItemClass.Block:
                    for (int i = 0; i < dumpmax; i++)
                    {
                        if (item.BlockCount == 0) { break; }
                        //find empty position that is nearest to dump place AND has a block under.
                        Vector3i? nearpos = FindDumpPlace(pos);
                        if (nearpos == null)
                        {
                            break;
                        }
                        SetBlockAndNotify(nearpos.Value.x, nearpos.Value.y, nearpos.Value.z, item.BlockId);
                        item.BlockCount--;
                    }
                    if (item.BlockCount == 0)
                    {
                        item = null;
                    }
                    break;
                default:
                    //todo
                    break;
            }
        }
        private Vector3i? FindDumpPlace(Vector3i pos)
        {
            List<Vector3i> l = new List<Vector3i>();
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    for (int z = 0; z < 10; z++)
                    {
                        int xx = pos.x + x - 10 / 2;
                        int yy = pos.y + y - 10 / 2;
                        int zz = pos.z + z - 10 / 2;
                        if (!MapUtil.IsValidPos(d_Map, xx, yy, zz))
                        {
                            continue;
                        }
                        if (d_Map.GetBlock(xx, yy, zz) == SpecialBlockId.Empty
                            && d_Map.GetBlock(xx, yy, zz - 1) != SpecialBlockId.Empty)
                        {
                            bool playernear = false;
                            foreach (var player in clients)
                            {
                                if (Length(Minus(PlayerBlockPosition(player.Value), new Vector3i(xx, yy, zz))) < 3)
                                {
                                    playernear = true;
                                }
                            }
                            if (!playernear)
                            {
                                l.Add(new Vector3i(xx, yy, zz));
                            }
                        }
                    }
                }
            }
            l.Sort((a, b) => Length(Minus(a, pos)).CompareTo(Length(Minus(b, pos))));
            if (l.Count > 0)
            {
                return l[0];
            }
            return null;
        }
        private Vector3i Minus(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        int Length(Vector3i v)
        {
            return (int)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        public void SetBlockType(int id, string name, BlockType block)
        {
            BlockTypes[id] = block;
            block.Name = name;
            d_Data.UseBlockType(id, block, null);
        }
        public void SetBlockType(string name, BlockType block)
        {
            for (int i = 0; i < BlockTypes.Length; i++)
            {
                if (BlockTypes[i] == null)
                {
                    SetBlockType(i, name, block);
                    return;
                }
            }
        }

        int[] sunlevels;
        public void SetSunLevels(int[] sunLevels)
        {
            this.sunlevels = sunLevels;
        }
        float[] lightlevels;
        public void SetLightLevels(float[] lightLevels)
        {
            this.lightlevels = lightLevels;
        }
        public List<CraftingRecipe> craftingrecipes = new List<CraftingRecipe>();
    }

    public class Ping
    {
        public TimeSpan RoundtripTime { get; set; }

        private bool ready;
        private DateTime timeSend;
        private int timeout = 10; //in seconds
        public int TimeoutValue
        {
            get { return timeout; }
            set { timeout = value; }
        }

        public Ping()
        {
            this.RoundtripTime = TimeSpan.MinValue;
            this.ready = true;
            this.timeSend = DateTime.MinValue;
        }

        public bool Send()
        {
            if (!ready)
            {
                return false;
            }
            ready = false;
            this.timeSend = DateTime.UtcNow;
            return true;
        }

        public bool Receive()
        {
            if (ready)
            {
                return false;
            }
            this.RoundtripTime = DateTime.UtcNow.Subtract(this.timeSend);
            ready = true;
            return true;
        }

        public bool Timeout()
        {
            if (DateTime.UtcNow.Subtract(this.timeSend).Seconds > this.timeout)
            {
                this.ready = true;
                return true;
            }
            return false;
        }
    }
}