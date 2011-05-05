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
    }
    public class Server : ICurrentTime, IDropItem
    {
        [Inject]
        public ServerMap d_Map;
        [Inject]
        public IGameData d_Data;
        [Inject]
        public CraftingTableTool d_CraftingTableTool;
        [Inject]
        public IGetFileStream d_GetFile;
        public CraftingRecipes craftingrecipes = new CraftingRecipes();
        [Inject]
        public IChunkDb d_ChunkDb;
        [Inject]
        public IWorldGenerator d_Generator;
        [Inject]
        public ICompression d_NetworkCompression;
        [Inject]
        public WaterFinite d_Water;
        [Inject]
        public ISocket d_MainSocket;

        public bool LocalConnectionsOnly { get; set; }
        public string[] PublicDataPaths = new string[0];
        public int singleplayerport = 25570;
        public Random rnd = new Random();
        public int SpawnPositionRandomizationRange = 96;
        public bool IsMono = Type.GetType("Mono.Runtime") != null;
        public void Start()
        {
            LoadConfig();
            {
                if (!Directory.Exists(gamepathsaves))
                {
                    Directory.CreateDirectory(gamepathsaves);
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
        }
        int Seed;
        private void LoadGame(string filename)
        {
            d_ChunkDb.Open(filename);
            byte[] globaldata = d_ChunkDb.GetGlobalData();
            if (globaldata == null)
            {
                //no savegame
                Seed = new Random().Next();
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
            this.Inventory = save.Inventory;
            this.simulationcurrentframe = save.SimulationCurrentFrame;
        }
        public Dictionary<string, PacketServerInventory> Inventory = new Dictionary<string, PacketServerInventory>();
        public void SaveGame(Stream s)
        {
            ManicDiggerSave save = new ManicDiggerSave();
            SaveAllLoadedChunks();
            save.Inventory = Inventory;
            save.Seed = Seed;
            save.SimulationCurrentFrame = simulationcurrentframe;
            Serializer.Serialize(s, save);
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
        public string gamepathconfig = GameStorePath.GetStorePath();
        public string gamepathsaves = Path.Combine(GameStorePath.GetStorePath(), "Saves");
        public string SaveFilenameWithoutExtension = "default";
        string GetSaveFilename()
        {
            return Path.Combine(gamepathsaves, SaveFilenameWithoutExtension + MapManipulator.BinSaveExtension);
        }
        public void Process11()
        {
            if ((DateTime.Now - lastsave).TotalMinutes > 2)
            {
                MemoryStream ms = new MemoryStream();
                DateTime start = DateTime.UtcNow;

                SaveGame(ms);
                d_ChunkDb.SetGlobalData(ms.ToArray());

                Console.WriteLine("Game saved. ({0} seconds)", (DateTime.UtcNow - start));
                lastsave = DateTime.Now;
            }
        }
        DateTime lastsave = DateTime.Now;

        public void LoadConfig()
        {
            string filename = "ServerConfig.xml";
            if (!File.Exists(Path.Combine(gamepathconfig, filename)))
            {
                Console.WriteLine("Server configuration file not found, creating new.");
                SaveConfig();
                return;
            }
            try
            {
                using (TextReader textReader = new StreamReader(Path.Combine(gamepathconfig, filename)))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(ServerConfig));
                    config = (ServerConfig)deserializer.Deserialize(textReader);
                    textReader.Close();
                }
            }
            catch //This if for the original format
            {
                using (Stream s = new MemoryStream(File.ReadAllBytes(Path.Combine(gamepathconfig, filename))))
                {
                    config = new ServerConfig();
                    StreamReader sr = new StreamReader(s);
                    XmlDocument d = new XmlDocument();
                    d.Load(sr);
                    int format = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/FormatVersion"));
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
                    config.IsCreative = ReadBool(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Creative"));
                    config.Public = ReadBool(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Public"));
                    config.BuildPassword = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/BuildPassword");
                    config.AdminPassword = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/AdminPassword");
                    if (XmlTool.XmlVal(d, "/ManicDiggerServerConfig/AllowFreemove") != null)
                    {
                        config.AllowFreemove = ReadBool(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/AllowFreemove"));
                    }
                    if (XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeX") != null)
                    {
                        config.MapSizeX = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeX"));
                        config.MapSizeY = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeY"));
                        config.MapSizeZ = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeZ"));
                    }
                }
                //Save with new version.
                SaveConfig();
            }
            Console.WriteLine("Server configuration loaded.");
        }
        private bool ReadBool(string str)
        {
            if (str == null)
            {
                return false;
            }
            else
            {
                return (str != "0"
                    && (!str.Equals(bool.FalseString, StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        public ServerConfig config;

        void SaveConfig()
        {
            //Verify that we have a directory to place the file into.
            if (!Directory.Exists(gamepathconfig))
            {
                Directory.CreateDirectory(gamepathconfig);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(ServerConfig));
            TextWriter textWriter = new StreamWriter(Path.Combine(gamepathconfig, "ServerConfig.xml"));

            //Check to see if config has been initialized
            if (config == null)
            {
                config = new ServerConfig();
            }
            if (config.Admins.Count == 0) { config.Admins.Add(config.DefaultPlayerName); }
            if (config.Builders.Count == 0) { config.Builders.Add(config.DefaultPlayerName); }

            //Serialize the ServerConfig class to XML
            serializer.Serialize(textWriter, config);
            textWriter.Close();

        }

        IPEndPoint iep;
        string fListUrl = "http://fragmer.net/md/heartbeat.php";
        public void SendHeartbeat()
        {
            try
            {
                if (config.Key == null)
                {
                    return;
                }
                StringWriter sw = new StringWriter();//&salt={4}
                string staticData = String.Format("name={0}&max={1}&public={2}&port={3}&version={4}&fingerprint={5}"
                    , System.Web.HttpUtility.UrlEncode(config.Name),
                    config.MaxClients, "true", config.Port, GameVersion.Version, config.Key.Replace("-", ""));

                List<string> playernames = new List<string>();
                lock (clients)
                {
                    foreach (var k in clients)
                    {
                        playernames.Add(k.Value.playername);
                    }
                }
                string requestString = staticData +
                                        "&users=" + clients.Count +
                                        "&motd=" + System.Web.HttpUtility.UrlEncode(config.Motd) +
                                        "&gamemode=Fortress" +
                                        "&players=" + string.Join(",", playernames.ToArray());

                var request = (HttpWebRequest)WebRequest.Create(fListUrl);
                request.Method = "POST";
                request.Timeout = 15000; // 15s timeout
                request.ContentType = "application/x-www-form-urlencoded";
                request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                byte[] formData = Encoding.ASCII.GetBytes(requestString);
                request.ContentLength = formData.Length;

                System.Net.ServicePointManager.Expect100Continue = false; // fixes lighthttpd 417 error

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(formData, 0, formData.Length);
                    requestStream.Flush();
                }

                WebResponse response = request.GetResponse();
                string key = null;
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    key = sr.ReadToEnd();
                }
                if (!writtenServerKey)
                {
                    Console.WriteLine(GetHash(key));
                    writtenServerKey = true;
                }

                request.Abort();
                Console.WriteLine("Heartbeat sent.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Unable to send heartbeat.");
            }
        }
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
        int lastclient;
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
        double starttime = gettime();
        static double gettime()
        {
            return (double)DateTime.Now.Ticks / (10 * 1000 * 1000);
        }
        long simulationcurrentframe;
        public int SimulationCurrentFrame { get { return (int)simulationcurrentframe; } }
        double oldtime;
        double accumulator;
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
                lock (clients)
                {
                    clients[lastclient++] = c;
                }
                c.notifyMapTimer = new ManicDigger.Timer()
                {
                    INTERVAL = 1.0 / SEND_CHUNKS_PER_SECOND,
                };
                if (clients.Count > config.MaxClients)
                {
                    SendDisconnectPlayer(lastclient - 1, "Too many players! Try to connect later.");
                    KillPlayer(lastclient - 1);
                }
                else if (config.IsIPBanned(iep1.Address.ToString()))
                {
                    SendDisconnectPlayer(lastclient - 1, "Your IP has been banned from this server.");
                    KillPlayer(lastclient - 1);
                }
            }
            ArrayList copyList = new ArrayList();
            foreach (var k in clients)
            {
                copyList.Add(k.Value.socket);
            }
            if (copyList.Count == 0)
            {
                return;
            }
            d_MainSocket.Select(copyList, null, null, 0);//10000000);

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
                catch (Exception e)
                {
                    //client problem. disconnect client.
                    Console.WriteLine(e.ToString());
                    SendDisconnectPlayer(k.Key, "exception");
                    KillPlayer(k.Key);
                }
            }
            foreach (var k in clients)
            {
                k.Value.notifyMapTimer.Update(delegate { NotifyMapChunks(k.Key, 1); });
                NotifyInventory(k.Key);
            }
            pingtimer.Update(delegate { foreach (var k in clients) { SendPing(k.Key); } });
            UnloadUnusedChunks();
            for (int i = 0; i < ChunksSimulated; i++)
            {
                ChunkSimulation();
            }
            UpdateWater();
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
                    byte emptytype = (byte)TileTypeMinecraft.Empty;
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
        Timer pingtimer = new Timer() { INTERVAL = 1, MaxDeltaTime = 5 };
        private void NotifySeason(int clientid)
        {
            PacketServerSeason p = new PacketServerSeason()
            {
                Season = GetSeason(simulationcurrentframe),
                Hour = GetHour(simulationcurrentframe),
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
        int GetHour(long frame)
        {
            long everyframes = (int)(1 / SIMULATION_STEP_LENGTH) * DAY_EVERY_SECONDS / 24;
            return (int)((frame / everyframes) % 24);
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
                        if (block == (int)TileTypeMinecraft.Sapling)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickSapling(pos);
                        }
                        if (block == (int)TileTypeMinecraft.BrownMushroom || block == (int)TileTypeMinecraft.RedMushroom)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickMushroom(pos);
                        }
                        if (block == (int)TileTypeMinecraft.YellowFlowerDecorations || block == (int)TileTypeMinecraft.RedRoseDecorations)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickFlower(pos);
                        }
                        if (block == (int)TileTypeMinecraft.Dirt)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickDirt(pos);
                        }
                        if (block == (int)TileTypeMinecraft.Grass)
                        {
                            Vector3i pos = new Vector3i(p.x + xx, p.y + yy, p.z + zz);
                            BlockTickGrass(pos);
                        }
                    }
                }
            }
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
                if (!(under == (int)TileTypeMinecraft.Dirt
                    || under == (int)TileTypeMinecraft.Grass
                    || under == (int)TileTypeManicDigger.DirtForFarming))
                {
                    return;
                }
                MakeAppleTree(pos.x, pos.y, pos.z - 1);
            }
        }
        void PlaceTree(int x, int y, int z)
        {
            int TileIdLeaves = (int)TileTypeMinecraft.Leaves;

            Place(x, y, z + 1, (int)TileTypeMinecraft.TreeTrunk);
            Place(x, y, z + 2, (int)TileTypeMinecraft.TreeTrunk);
            Place(x, y, z + 3, (int)TileTypeMinecraft.TreeTrunk);

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
        private void Place(int x, int y, int z, int blocktype)
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
                int block2 = d_Map.GetBlock(pos.x, pos.y, pos.z + 1);
                if (d_Data.IsTransparentForLight[block2])
                {
                    if (!IsShadow(pos.x, pos.y, pos.z))
                    {
                        SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeMinecraft.Grass);
                    }
                    else
                    {
                        // if 1% chance happens then 1 mushroom will grow up 
                        if (rnd.NextDouble() < 0.01)
                        {
                            int tile = rnd.NextDouble() < 0.6 ? (int)TileTypeMinecraft.RedMushroom : (int)TileTypeMinecraft.BrownMushroom;
                            SetBlockAndNotify(pos.x, pos.y, pos.z + 1, tile);
                        }
                    }
                }
            }
        }
        private void BlockTickGrass(Vector3i pos)
        {
            if (IsShadow(pos.x, pos.y, pos.z))
            {
                SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeMinecraft.Dirt);
            }
        }
        private void MakeAppleTree(int cx, int cy, int cz)
        {
            int x = cx;
            int y = cy;
            int z = cz;
            int TileIdLeaves = (int)TileTypeMinecraft.Leaves;
            int TileIdApples = (int)TileTypeManicDigger.Apples;
            int TileIdTreeTrunk = (int)TileTypeMinecraft.TreeTrunk;
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
            if (rnd.NextDouble() < 0.2) { SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeMinecraft.Empty); return; }
            if (!IsShadow(pos.x, pos.y, pos.z - 1))
            {
                SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeMinecraft.Empty);
            }
            else
            {
                if (d_Map.GetBlock(pos.x, pos.y, pos.z - 1) == (int)TileTypeMinecraft.Dirt) return;
                SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeMinecraft.Empty);
            }
        }
        // floowers will die when they have not light, dirt or grass, or 25% chance happens
        private void BlockTickFlower(Vector3i pos)
        {
            if (!MapUtil.IsValidPos(d_Map, pos.x, pos.y, pos.z)) return;
            if (rnd.NextDouble() < 0.25) { SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeMinecraft.Empty); return; }
            if (IsShadow(pos.x, pos.y, pos.z - 1))
            {
                SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeMinecraft.Empty);
            }
            else
            {
                int under = d_Map.GetBlock(pos.x, pos.y, pos.z - 1);
                if ((under == (int)TileTypeMinecraft.Dirt
                      || under == (int)TileTypeMinecraft.Grass)) return;
                SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeMinecraft.Empty);
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
                        DoSaveChunk(v.x, v.y, v.z, c);
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
        private void DoSaveChunk(int x, int y, int z, Chunk c)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, c);
            ChunkDb.SetChunk(d_ChunkDb, x, y, z, ms.ToArray());
        }
        int SEND_CHUNKS_PER_SECOND = 10;
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
        private void LoadChunk(Vector3i v)
        {
            d_Map.GetBlock(v.x, v.y, v.z);
        }
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
            return sent;
        }
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
            Inventory i = ManicDigger.Inventory.Create();
            i.Items.Add(new ProtoPoint(0, 0), new Item() { ItemClass = ItemClass.Block, BlockId = (int)TileTypeManicDigger.CraftingTable, BlockCount = 6 });
            i.Items.Add(new ProtoPoint(1, 0), new Item() { ItemClass = ItemClass.Block, BlockId = (int)TileTypeManicDigger.Crops1, BlockCount = 1 });
            return i;
        }
        Vector3i PlayerBlockPosition(Client c)
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
            string name = clients[clientid].playername;
            clients.Remove(clientid);
            foreach (var kk in clients)
            {
                SendDespawnPlayer(kk.Key, (byte)clientid);
            }
            if (name != "invalid")
            {
                SendMessageToAll(string.Format("Player {0} disconnected.", name));
            }
        }
        Vector3i DefaultSpawnPosition()
        {
            int x = d_Map.MapSizeX / 2;
            int y = d_Map.MapSizeY / 2 - 1;//antivandal side
            //spawn position randomization disabled.
            //x += rnd.Next(SpawnPositionRandomizationRange) - SpawnPositionRandomizationRange / 2;
            //y += rnd.Next(SpawnPositionRandomizationRange) - SpawnPositionRandomizationRange / 2;
            return new Vector3i(x * 32, MapUtil.blockheight(d_Map, 0, x, y) * 32, y * 32);
        }
        public char[] AllowedUsernameCharacters = ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
            + "1234567890_-").ToCharArray();
        bool IsValidUsername(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                for (int j = 0; j < AllowedUsernameCharacters.Length; j++)
                {
                    if (s[i] == AllowedUsernameCharacters[j])
                    {
                        goto next;
                    }
                }
                return false;
            next:
                ;
            }
            return true;
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
            switch (packet.PacketId)
            {
                case ClientPacketId.PlayerIdentification:
                    SendServerIdentification(clientid);
                    string username = packet.Identification.Username;

                    if (config.IsUserBanned(username))
                    {
                        SendDisconnectPlayer(clientid, "Your username has been banned from this server.");
                        KillPlayer(clientid);
                        break;
                    }

                    if (!IsValidUsername(username))
                    {
                        SendDisconnectPlayer(clientid, "Invalid characters in username.");
                        KillPlayer(clientid);
                        break;
                    }

                    //when duplicate user connects, append a number to name.
                    foreach (var k in clients)
                    {
                        if (k.Value.playername.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                        {
                            username = GenerateUsername(username);
                            break;
                        }
                    }

                    bool isClientLocalhost = (((IPEndPoint)c.socket.RemoteEndPoint).Address.ToString() == "127.0.0.1");

                    if ((ComputeMd5(config.Key.Replace("-", "") + username) != packet.Identification.VerificationKey)
                        && (!isClientLocalhost))
                    {
                        //Account verification failed.
                        username = "~" + username;
                    }

                    if (config.Builders.Contains(username)) { clients[clientid].Rank = Rank.Builder; }
                    if (config.Admins.Contains(username)) { clients[clientid].Rank = Rank.Admin; }

                    clients[clientid].playername = username;
                    break;
                case ClientPacketId.RequestBlob:

                    Vector3i position = DefaultSpawnPosition();
                    clients[clientid].PositionMul32GlX = position.x;
                    clients[clientid].PositionMul32GlY = position.y + (int)(0.5 * 32);
                    clients[clientid].PositionMul32GlZ = position.z;

                    string username1 = clients[clientid].playername;
                    SendMessageToAll(string.Format("Player {0} joins.", username1));
                    SendMessage(clientid, colorSuccess + config.WelcomeMessage);
                    SendLevel(clientid);

                    //.players.Players[clientid] = new Player() { Name = username };
                    //send new player spawn to all players
                    foreach (var k in clients)
                    {
                        int cc = k.Key == clientid ? byte.MaxValue : clientid;
                        {
                            PacketServer pp = new PacketServer();
                            PacketServerSpawnPlayer p = new PacketServerSpawnPlayer()
                            {
                                PlayerId = cc,
                                PlayerName = username1,
                                PositionAndOrientation = new PositionAndOrientation()
                                {
                                    X = position.x,
                                    Y = position.y + (int)(0.5 * 32),
                                    Z = position.z,
                                    Heading = 0,
                                    Pitch = 0,
                                }
                            };
                            pp.PacketId = ServerPacketId.SpawnPlayer;
                            pp.SpawnPlayer = p;
                            SendPacket(k.Key, Serialize(pp));
                        }
                    }
                    //send all players spawn to new player
                    foreach (var k in clients)
                    {
                        if (k.Key != clientid)// || ENABLE_FORTRESS)
                        {
                            {
                                PacketServer pp = new PacketServer();
                                PacketServerSpawnPlayer p = new PacketServerSpawnPlayer()
                                {
                                    PlayerId = k.Key,
                                    PlayerName = k.Value.playername,
                                    PositionAndOrientation = new PositionAndOrientation()
                                    {
                                        X = 0,
                                        Y = 0,
                                        Z = 0,
                                        Heading = 0,
                                        Pitch = 0,
                                    }
                                };
                                pp.PacketId = ServerPacketId.SpawnPlayer;
                                pp.SpawnPlayer = p;
                                SendPacket(clientid, Serialize(pp));
                            }
                        }
                    }

                    NotifySeason(clientid);
                    break;
                case ClientPacketId.SetBlock:
                    int x = packet.SetBlock.X;
                    int y = packet.SetBlock.Y;
                    int z = packet.SetBlock.Z;
                    if ((!string.IsNullOrEmpty(config.BuildPassword))
                        && !config.CanUserBuild(clients[clientid], x, y))
                    {
                        SendMessage(clientid, colorError + "You need permission to build in this section of the world.");
                        SendSetBlock(clientid, x, y, z, d_Map.GetBlock(x, y, z)); //revert
                        break;
                    }
                    BlockSetMode mode = packet.SetBlock.Mode == 0 ? BlockSetMode.Destroy : BlockSetMode.Create;
                    byte blocktype = (byte)packet.SetBlock.BlockType;
                    if (mode == BlockSetMode.Destroy)
                    {
                        blocktype = 0; //data.TileIdEmpty
                    }
                    //todo check block type.
                    //map.SetBlock(x, y, z, blocktype);
                    if (!DoCommandBuild(clientid, true, packet.SetBlock))
                    {
                        SendSetBlock(clientid, x, y, z, d_Map.GetBlock(x, y, z)); //revert
                    }
                    d_Water.BlockChange(d_Map, x, y, z);
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
                                SendPlayerTeleport(k.Key, (byte)clientid, p.X, p.Y, p.Z, p.Heading, p.Pitch);
                            }
                        }
                    }
                    break;
                case ClientPacketId.Message:
                    packet.Message.Message = packet.Message.Message.Trim();
                    if (packet.Message.Message.StartsWith("/msg"))
                    {
                        string[] ss = packet.Message.Message.Split(new[] { ' ' });
                        bool messageSent = false;
                        if (ss.Length >= 3)
                        {
                            foreach (var k in clients)
                            {
                                if (k.Value.playername.Equals(ss[1], StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string msg = string.Join(" ", ss, 2, ss.Length - 2);
                                    SendMessage(k.Key, "msg " + c.playername + ": " + msg);
                                    messageSent = true;
                                    break;
                                }
                            }
                        }
                        if (!messageSent)
                        {
                            SendMessage(clientid, colorError + "Usage: /msg [username] [message]");
                            break;
                        }
                        break;
                    }

                    else if (packet.Message.Message.StartsWith("/op")
                        || packet.Message.Message.StartsWith("/rank"))
                    {
                        if (clients[clientid].Rank != Rank.Admin)
                        {
                            SendMessage(clientid, colorError + "You are not logged in as an administrator and cannot use rank.");
                            break;
                        }
                        string[] ss = packet.Message.Message.Split(new[] { ' ' });
                        if (ss.Length < 2)
                        {
                            SendMessage(clientid, colorError + "Usage: /op [username] [guest/builder/admin].");
                            break;
                        }
                        string argName = ss[1];
                        foreach (var k in clients)
                        {
                            if (k.Value.playername.Equals(argName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                string argRank = k.Value.Rank == Rank.Admin ? "admin" : "builder"; //don't remove admin with "/op user"
                                if (ss.Length > 2) { argRank = ss[2]; }
                                Rank r = Rank.Builder;
                                if (argRank.Equals("guest", StringComparison.InvariantCultureIgnoreCase)) { r = Rank.Guest; }
                                if (argRank.Equals("admin", StringComparison.InvariantCultureIgnoreCase)) { r = Rank.Admin; }


                                k.Value.Rank = r;
                                string name = k.Value.playername;
                                if (r == Rank.Admin)
                                {
                                    if (!config.Admins.Contains(name)) { config.Admins.Add(name); }
                                    config.Builders.Remove(name);
                                    SendMessage(k.Key, "Type /help to see additional commands for administrators.");
                                }
                                else if (r == Rank.Builder)
                                {
                                    if (!config.Builders.Contains(name)) { config.Builders.Add(name); }
                                    config.Admins.Remove(name);
                                }
                                else if (r == Rank.Guest)
                                {
                                    config.Builders.Remove(name);
                                    config.Admins.Remove(name);
                                }
                                SaveConfig();

                                SendMessageToAll(colorSuccess + "New rank for player "
                                    + k.Value.playername + ": " + argRank + ".");
                            }
                        }
                        break;
                    }
                    //for servers without global name verification
                    else if (packet.Message.Message.StartsWith("/login"))
                    {
                        if (string.IsNullOrEmpty(config.BuildPassword))
                        {
                            break;
                        }
                        if (packet.Message.Message.Replace("/login ", "")
                         .Equals(config.AdminPassword, StringComparison.InvariantCultureIgnoreCase))
                        {
                            clients[clientid].Rank = Rank.Admin;
                            SendMessageToAll(colorSuccess + clients[clientid].playername + " can now build.");
                            SendMessage(clientid, "Type /help to see additional commands for administrators.");
                        }
                        else if (packet.Message.Message.Replace("/login ", "")
                            .Equals(config.BuildPassword, StringComparison.InvariantCultureIgnoreCase))
                        {
                            clients[clientid].Rank = Rank.Builder;
                            SendMessageToAll(colorSuccess + clients[clientid].playername + " can now build.");
                        }
                        else
                        {
                            SendMessage(clientid, colorError + "Invalid password.");
                        }
                    }

                    else if (packet.Message.Message.StartsWith("/welcome"))
                    {
                        if (!clients[clientid].IsAdmin)
                        {
                            SendMessage(clientid, "You are not logged in as an administrator and cannot change the Welcome Message.");
                        }
                        else
                        {
                            //Monaiz
                            int messageStart = packet.Message.Message.IndexOf(" ");
                            config.WelcomeMessage = packet.Message.Message.Substring(messageStart);
                            SendMessageToAll("New Welcome Message Set: " + colorSuccess + config.WelcomeMessage);
                            SaveConfig();
                            break;
                        }
                    }
                    else if (packet.Message.Message.StartsWith("/kick"))
                    {
                        if (!clients[clientid].IsAdmin)
                        {
                            SendMessage(clientid, "You are not logged in as an administrator and cannot kick other players.");
                        }
                        else
                        {
                            string[] ss = packet.Message.Message.Split(new[] { ' ' });
                            foreach (var k in clients)
                            {
                                if (k.Value.playername.Equals(ss[1], StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string targetName = k.Value.playername;
                                    string sourcename = clients[clientid].playername;
                                    SendDisconnectPlayer(k.Key, "You were kicked by an administrator.");
                                    KillPlayer(k.Key);

                                    SendMessageToAll(colorError + targetName + " was kicked by " + sourcename);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    else if (packet.Message.Message.StartsWith("/ban "))
                    {
                        if (!clients[clientid].IsAdmin)
                        {
                            SendMessage(clientid, "You are not logged in as an administrator and cannot ban other players.");
                        }
                        else
                        {
                            string[] ss = packet.Message.Message.Split(new[] { ' ' });
                            foreach (var k in clients)
                            {
                                if (k.Value.playername.Equals(ss[1], StringComparison.InvariantCultureIgnoreCase))
                                {
                                    //TODO: Confirm player is not a guest account.
                                    string targetName = k.Value.playername;
                                    string sourcename = clients[clientid].playername;
                                    config.BannedUsers.Add(k.Value.playername);
                                    SaveConfig();
                                    SendMessageToAll(colorError + targetName + " was banned by " + sourcename);

                                    SendDisconnectPlayer(k.Key, "You were banned by an administrator.");
                                    KillPlayer(k.Key);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    else if (packet.Message.Message.StartsWith("/banip"))
                    {
                        if (!clients[clientid].IsAdmin)
                        {
                            SendMessage(clientid, "You are not logged in as an administrator and cannot ban other players.");
                        }
                        else
                        {
                            string[] ss = packet.Message.Message.Split(new[] { ' ' });
                            foreach (var k in clients)
                            {
                                if (k.Value.playername.Equals(ss[1], StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string targetName = k.Value.playername;
                                    string sourcename = clients[clientid].playername;
                                    config.BannedIPs.Add(((IPEndPoint)k.Value.socket.RemoteEndPoint).Address.ToString());
                                    SaveConfig();

                                    SendDisconnectPlayer(k.Key, "You were banned by an administrator.");
                                    k.Value.socket.Disconnect(true);

                                    KillPlayer(k.Key);

                                    SendMessageToAll(colorError + targetName + " was banned by " + sourcename);
                                }
                            }
                            break;
                        }
                    }
                    else if (packet.Message.Message.StartsWith("/list"))
                    {
                        if (!clients[clientid].IsAdmin)
                        {
                            SendMessage(clientid, "You are not logged in as an administrator and cannot access this command.");
                        }
                        else
                        {
                            SendMessage(clientid, colorHelp + "List of Players:");
                            foreach (var k in clients)
                            {
                                SendMessage(clientid, k.Value.playername.ToString() + " " + ((IPEndPoint)k.Value.socket.RemoteEndPoint).Address.ToString());
                            }
                            break;
                        }
                    }
                    else if (packet.Message.Message.StartsWith("/help"))
                    {
                        SendMessage(clientid, colorHelp + "/login [buildpassword]");
                        SendMessage(clientid, colorHelp + "/msg [username] text");
                        if (clients[clientid].IsAdmin)
                        {
                            SendMessage(clientid, colorHelp + "/kick [username]");
                            SendMessage(clientid, colorHelp + "/welcome [login motd message]");
                            SendMessage(clientid, colorHelp + "/ban [username]");
                            SendMessage(clientid, colorHelp + "/banip [username]");
                            SendMessage(clientid, colorHelp + "/list");
                            SendMessage(clientid, colorHelp + "/op [username] [guest/builder/admin]");
                        }
                    }
                    else if (packet.Message.Message.StartsWith("."))
                    {
                        break;
                    }
                    else if (packet.Message.Message.StartsWith("/"))
                    {
                        SendMessage(clientid, colorError + "Invalid command.");
                        break;
                    }
                    else
                    {
                        SendMessageToAll(string.Format("{0}: {1}", PlayerNameColored(clientid), colorNormal + packet.Message.Message));
                    }
                    break;
                case ClientPacketId.Craft:
                    DoCommandCraft(true, packet.Craft);
                    break;
                case ClientPacketId.InventoryAction:
                    DoCommandInventory(clientid, packet.InventoryAction);
                    break;
                default:
                    Console.WriteLine("Invalid packet: {0}, clientid:{1}", packet.PacketId, clientid);
                    break;
            }
            return lengthPrefixLength + packetLength;
        }
        string PlayerNameColored(int clientid)
        {
            return (clients[clientid].CanBuild ? colorOpUsername : "")
                + clients[clientid].playername;
        }
        string colorNormal = "&f"; //white
        string colorHelp = "&4"; //red
        string colorOpUsername = "&2"; //green
        string colorSuccess = "&2"; //green
        string colorError = "&4"; //red
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
            if (cmd.RecipeId < 0 || cmd.RecipeId >= craftingrecipes.craftingrecipes.Count)
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
                    foreach (Ingredient ingredient in craftingrecipes.craftingrecipes[i].ingredients)
                    {
                        if (ontable.FindAll(v => v == ingredient.Type).Count < ingredient.Amount)
                        {
                            goto nextrecipe;
                        }
                    }
                    //remove ingredients
                    foreach (Ingredient ingredient in craftingrecipes.craftingrecipes[i].ingredients)
                    {
                        for (int ii = 0; ii < ingredient.Amount; ii++)
                        {
                            //replace on table
                            ReplaceOne(ontable, ingredient.Type, (int)TileTypeMinecraft.Empty);
                        }
                    }
                    //add output
                    for (int z = 0; z < craftingrecipes.craftingrecipes[i].output.Amount; z++)
                    {
                        outputtoadd.Add(craftingrecipes.craftingrecipes[i].output.Type);
                    }
                }
            nextrecipe:
                ;
            }
            foreach (var v in outputtoadd)
            {
                ReplaceOne(ontable, (int)TileTypeMinecraft.Empty, v);
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
        public IGameDataItems d_DataItems = new GameDataItemsBlocks();
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
        private bool DoCommandBuild(int player_id, bool execute, PacketClientSetBlock cmd)
        {
            Vector3 v = new Vector3(cmd.X, cmd.Y, cmd.Z);
            Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
            if (cmd.Mode == (int)BlockSetMode.Create)
            {
                int oldblock = d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z);
                if (!(oldblock == 0 || d_Data.IsWater[oldblock]))
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
                        SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, item.BlockId);
                        break;
                    default:
                        //todo
                        return false;
                }
            }
            else
            {
                var item = new Item();
                item.ItemClass = ItemClass.Block;
                int blockid = d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z);
                item.BlockId = d_Data.WhenPlayerPlacesGetsConvertedTo[blockid];
                GetInventoryUtil(inventory).GrabItem(item, cmd.MaterialSlot);
                SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, SpecialBlockId.Empty);
            }
            clients[player_id].IsInventoryDirty = true;
            NotifyInventory(player_id);
            /*
            if (ENABLE_FINITEINVENTORY)
            {
                Dictionary<int, int> inventory = GetPlayerInventory(clients[player_id].playername).BlockTypeAmount;
                if (cmd.Mode == (int)BlockSetMode.Create)
                {
                    int oldblock = d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z);
                    int blockstoput = 1;
                    if (GameDataManicDigger.IsRailTile(cmd.BlockType))
                    {
                        if (!(oldblock == SpecialBlockId.Empty
                            || GameDataManicDigger.IsRailTile(oldblock)))
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
                        blockstoput = newrailcount - oldrailcount;
                        //check if player has that many rails
                        int inventoryrail = GetEquivalentCount(inventory, cmd.BlockType);
                        if (blockstoput > inventoryrail)
                        {
                            return false;
                        }
                        if (execute)
                        {
                            RemoveEquivalent(inventory, cmd.BlockType, blockstoput);
                        }
                    }
                    else
                    {
                        //todo when removing rail under minecart, make minecart block in place of removed rail.
                        if (oldblock != SpecialBlockId.Empty)
                        {
                            return false;
                        }
                        //check if player has such block
                        int hasblock = -1; //which equivalent block it has exactly?
                        foreach (var k in inventory)
                        {
                            if (EquivalentBlock(k.Key, cmd.BlockType)
                                && k.Value > 0)
                            {
                                hasblock = k.Key;
                            }
                        }
                        if (hasblock == -1)
                        {
                            return false;
                        }
                        if (execute)
                        {
                            inventory[hasblock]--;
                        }
                    }
                }
                else
                {
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
                    if (TotalAmount(inventory) + blockstopick > FiniteInventoryMax)
                    {
                        return false;
                    }
                    if (execute)
                    {
                        if (!inventory.ContainsKey(blocktype))
                        {
                            inventory[blocktype] = 0;
                        }
                        inventory[blocktype] += blockstopick;
                    }
                }
                clients[player_id].IsInventoryDirty = true;
            }
            else
            {
            }
            */
            return true;
        }
        void SetBlockAndNotify(int x, int y, int z, int blocktype)
        {
            d_Map.SetBlockNotMakingDirty(x, y, z, blocktype);
            NotifyBlock(x, y, z, blocktype);
        }
        int TotalAmount(Dictionary<int, int> inventory)
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
        bool EquivalentBlock(int blocktypea, int blocktypeb)
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
            if (name.Length > 0 && char.IsNumber(name[name.Length - 1]))
            {
                defaultname = name.Substring(0, name.Length - 1);
            }
            for (int i = 1; i < 100; i++)
            {
                foreach (var k in clients)
                {
                    if (k.Value.playername.Equals(defaultname + i))
                    {
                        goto nextname;
                    }
                }
                return defaultname + i;
            nextname:
                ;
            }
            return defaultname;
        }
        private void SendMessageToAll(string message)
        {
            Console.WriteLine(message);
            foreach (var k in clients)
            {
                SendMessage(k.Key, message);
            }
        }
        private void SendSetBlock(int clientid, int x, int y, int z, int blocktype)
        {
            if (!clients[clientid].chunksseen.ContainsKey(new Vector3i((x / chunksize) * chunksize,
                (y / chunksize) * chunksize, (z / chunksize) * chunksize)))
            {
                return;
            }
            PacketServerSetBlock p = new PacketServerSetBlock() { X = x, Y = y, Z = z, BlockType = blocktype };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.SetBlock, SetBlock = p }));
        }
        private void SendPlayerTeleport(int clientid, byte playerid, int x, int y, int z, byte heading, byte pitch)
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
        private void SendDespawnPlayer(int clientid, byte playerid)
        {
            PacketServerDespawnPlayer p = new PacketServerDespawnPlayer() { PlayerId = playerid };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.DespawnPlayer, DespawnPlayer = p }));
        }
        private void SendMessage(int clientid, string message)
        {
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
        int drawdistance = 192;
        public int chunksize = 32;
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
                        yield return new Vector3i(playerpos.x + x * chunksize, playerpos.y + y * chunksize, z * chunksize);
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
        private void SendLevel(int clientid)
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
            SendLevelFinalize(clientid);
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
        public enum Rank
        {
            Guest,
            Builder,
            Admin, //Does this user have the server password and can ban users?
        }
        public struct BlobToSend
        {
            public string Name;
            public byte[] Data;
        }
        public class Client
        {
            public ISocket socket;
            public List<byte> received = new List<byte>();
            public string playername = invalidplayername;
            public int PositionMul32GlX;
            public int PositionMul32GlY;
            public int PositionMul32GlZ;
            public int positionheading;
            public int positionpitch;
            public Dictionary<Vector3i, int> chunksseen = new Dictionary<Vector3i, int>();
            public Dictionary<Vector2i, int> heightmapchunksseen = new Dictionary<Vector2i, int>();
            public ManicDigger.Timer notifyMapTimer;
            public bool IsInventoryDirty = true;
            //public List<byte[]> blobstosend = new List<byte[]>();
            public Rank Rank = Rank.Guest;
            public bool IsGuest { get { return playername.StartsWith("~"); } }
            public bool IsAdmin { get { return Rank == Rank.Admin; } }
            public bool CanBuild { get { return Rank == Rank.Admin || Rank == Rank.Builder; } }
        }
        Dictionary<int, Client> clients = new Dictionary<int, Client>();

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
    }
}
