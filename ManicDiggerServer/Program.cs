using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Collections;
using ManicDigger;
using System.Threading;
using OpenTK;
using System.Xml;
using System.Diagnostics;
using GameModeFortress;
using ProtoBuf;

namespace ManicDiggerServer
{
    [ProtoContract]
    public class SaveChunk
    {
        [ProtoMember(1, IsRequired = false)]
        public int X;
        [ProtoMember(2, IsRequired = false)]
        public int Y;
        [ProtoMember(3, IsRequired = false)]
        public int Z;
        [ProtoMember(4, IsRequired = false)]
        public int SizeX;
        [ProtoMember(5, IsRequired = false)]
        public int SizeY;
        [ProtoMember(6, IsRequired = false)]
        public int SizeZ;
        [ProtoMember(7, IsRequired = false)]
        public byte[] CompressedChunk;
        [ProtoMember(8, IsRequired = false)]
        public long LastUpdate;
    }
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
        public Dictionary<string, PacketServerFiniteInventory> Inventory;
        [ProtoMember(6, IsRequired = false)]
        public List<SaveChunk> MapChunks;
        [ProtoMember(7, IsRequired = false)]
        public int Seed;
        [ProtoMember(8, IsRequired = false)]
        public long SimulationCurrentFrame;
    }
    public class Server
    {
        [Inject]
        public InfiniteMapChunked map { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public CraftingTableTool craftingtabletool { get; set; }
        public CraftingRecipes craftingrecipes = new CraftingRecipes();
        bool ENABLE_FORTRESS = true;
        public void Start()
        {
            LoadConfig();
            {
                //((GameModeFortress.GameFortress)gameworld).ENABLE_FINITEINVENTORY = !cfgcreative;
                if (File.Exists(manipulator.defaultminesave))
                {
                    Console.WriteLine("Loading savegame...");
                    LoadGame(new MemoryStream(File.ReadAllBytes(manipulator.defaultminesave)));
                    Console.WriteLine("Savegame loaded: " + manipulator.defaultminesave);
                }
                else
                {
                    Seed = new Random().Next();
                    generator.SetSeed(Seed);
                }
            }
            Start(cfgport);
        }
        int Seed;
        private void LoadGame(Stream s)
        {
            ManicDiggerSave save = Serializer.Deserialize<ManicDiggerSave>(s);
            generator.SetSeed(save.Seed);
            Seed = save.Seed;
            map.Reset(map.MapSizeX, map.MapSizeX, map.MapSizeZ);
            foreach (SaveChunk chunk in save.MapChunks)
            {
                //same as in client
                var p = chunk;
                if (p.SizeX == chunksize && p.SizeY == chunksize && p.SizeZ == chunksize)
                {
                    var c = new InfiniteMapChunked.Chunk();
                    map.chunks[p.X / chunksize, p.Y / chunksize, p.Z / chunksize] = c;
                    c.compressed = p.CompressedChunk;
                    c.LastUpdate = p.LastUpdate;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            this.Inventory = save.Inventory;
            this.simulationcurrentframe = save.SimulationCurrentFrame;
        }
        [Inject]
        public WorldGenerator generator { get; set; }
        public Dictionary<string, PacketServerFiniteInventory> Inventory = new Dictionary<string, PacketServerFiniteInventory>();
        public void SaveGame(Stream s)
        {
            ManicDiggerSave save = new ManicDiggerSave();
            save.MapChunks = new List<SaveChunk>();
            for (int cx = 0; cx < map.MapSizeX / chunksize; cx++)
            {
                for (int cy = 0; cy < map.MapSizeY / chunksize; cy++)
                {
                    for (int cz = 0; cz < map.MapSizeZ / chunksize; cz++)
                    {
                        GameModeFortress.InfiniteMapChunked.Chunk c = map.chunks[cx, cy, cz];
                        if (c != null)
                        {
                            SaveChunk chunk = new SaveChunk();
                            chunk.SizeX = chunksize;
                            chunk.SizeY = chunksize;
                            chunk.SizeZ = chunksize;
                            chunk.X = cx * chunksize;
                            chunk.Y = cy * chunksize;
                            chunk.Z = cz * chunksize;
                            if (c.compressed != null) { chunk.CompressedChunk = c.compressed; }
                            else { chunk.CompressedChunk = CompressChunk(c.data); }
                            if (chunk.CompressedChunk == null) { throw new Exception(); }
                            chunk.LastUpdate = c.LastUpdate;
                            save.MapChunks.Add(chunk);
                        }
                    }
                }
            }
            save.Inventory = Inventory;
            save.Seed = Seed;
            save.SimulationCurrentFrame = simulationcurrentframe;
            Serializer.Serialize(s, save);
        }
        MapManipulator manipulator = new MapManipulator() { getfile = new GetFilePathDummy() };
        public void Process11()
        {
            if ((DateTime.Now - lastsave).TotalMinutes > 2)
            {
                if (!ENABLE_FORTRESS)
                {
                    manipulator.SaveMap(map, manipulator.defaultminesave);
                }
                else
                {
                    MemoryStream ms = new MemoryStream();
                    SaveGame(ms);
                    File.WriteAllBytes(manipulator.defaultminesave, ms.ToArray());
                }
                Console.WriteLine("Game saved.");
                lastsave = DateTime.Now;
            }
        }
        DateTime lastsave = DateTime.Now;
        void LoadConfig()
        {
            string filename = "ServerConfig.xml";
            if (!File.Exists(filename))
            {
                Console.WriteLine("Server configuration file not found, creating new.");
                SaveConfig();
                return;
            }
            using (Stream s = new MemoryStream(File.ReadAllBytes(filename)))
            {
                StreamReader sr = new StreamReader(s);
                XmlDocument d = new XmlDocument();
                d.Load(sr);
                int format = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/FormatVersion"));
                cfgname = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Name");
                cfgmotd = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Motd");
                cfgport = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Port"));
                string key = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Key");
                if (key != null)
                {
                    cfgkey = key;
                }
                else
                {
                    cfgkey = Guid.NewGuid().ToString();
                    SaveConfig();
                }
                string creativestr = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Creative");
                if (creativestr == null)
                {
                    cfgcreative = false;
                }
                else
                {
                    cfgcreative =
                        (creativestr != "0"
                        && (!creativestr.Equals(bool.FalseString, StringComparison.InvariantCultureIgnoreCase)));
                }
            }
            Console.WriteLine("Server configuration loaded.");
        }
        bool cfgcreative;
        void SaveConfig()
        {
            string s = "<ManicDiggerServerConfig>"+Environment.NewLine;
            s += "  " + XmlTool.X("FormatVersion", "1") + Environment.NewLine;
            s += "  " + XmlTool.X("Name", cfgname) + Environment.NewLine;
            s += "  " + XmlTool.X("Motd", cfgmotd) + Environment.NewLine;
            s += "  " + XmlTool.X("Port", cfgport.ToString()) + Environment.NewLine;
            s += "  " + XmlTool.X("Key", Guid.NewGuid().ToString()) + Environment.NewLine;
            s += "  " + XmlTool.X("Creative", cfgcreative ? bool.TrueString : bool.FalseString) + Environment.NewLine;
            s += "</ManicDiggerServerConfig>";
            File.WriteAllText("ServerConfig.xml", s);
        }
        string cfgname = "Manic Digger server";
        string cfgmotd = "MOTD";
        public int cfgport = 25565;
        string cfgkey;
        Socket main;
        IPEndPoint iep;
        string fListUrl = "http://fragmer.net/md/heartbeat.php";
        public void SendHeartbeat()
        {
            try
            {
                StringWriter sw = new StringWriter();//&salt={4}
                string staticData = String.Format("name={0}&max={1}&public={2}&port={3}&version={4}&fingerprint={5}"
                    , System.Web.HttpUtility.UrlEncode(cfgname),
                    32, "true", cfgport, GameVersion.Version , cfgkey.Replace("-", ""));

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
                                        "&motd=" + System.Web.HttpUtility.UrlEncode(cfgmotd) +
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

                request.Abort();
                Console.WriteLine("Heartbeat sent.");
            }
            catch
            {
                Console.WriteLine("Unable to send heartbeat.");
            }
        }
        void Start(int port)
        {
            main = new Socket(AddressFamily.InterNetwork,
                   SocketType.Stream, ProtocolType.Tcp);

            iep = new IPEndPoint(IPAddress.Any, port);
            main.Bind(iep);
            main.Listen(10);
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
        double oldtime;
        double accumulator;
        public void Process1()
        {
            if (main == null)
            {
                return;
            }
            if (ENABLE_FORTRESS)
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
                    if (GetSeason(simulationcurrentframe) != GetSeason(simulationcurrentframe - 1))
                    {
                        foreach(var c in clients)
                        {
                            PacketServerSeason p = new PacketServerSeason() { Season = GetSeason(simulationcurrentframe) };
                            SendPacket(c.Key, Serialize(new PacketServer() { PacketId = ServerPacketId.Season, Season=p}));
                        }
                    }
                    accumulator -= dt;
                }
                oldtime = currenttime;
            }
            byte[] data = new byte[1024];
            string stringData;
            int recv;
            if (main.Poll(0, SelectMode.SelectRead)) //Test for new connections
            {
                Socket client1 = main.Accept();
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
            Socket.Select(copyList, null, null, 0);//10000000);

            foreach (Socket clientSocket in copyList)
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
                NotifyFiniteInventory(k.Key);
            }
            CompressUnusedChunks();
            for (int i = 0; i < ChunksSimulated; i++)
            {
                ChunkSimulation();
            }
        }
        int SEASON_EVERY_SECONDS = 60 * 60;//1 hour
        int GetSeason(long frame)
        {
            long everyframes = (int)(1 / SIMULATION_STEP_LENGTH) * SEASON_EVERY_SECONDS;
            return (int)((frame / everyframes) % 4);
        }
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
                    if (!MapUtil.IsValidPos(map, p.x, p.y, p.z)) { continue; }
                    GameModeFortress.InfiniteMapChunked.Chunk c = map.chunks[p.x / chunksize, p.y / chunksize, p.z / chunksize];
                    if (c == null) { continue; }
                    if (c.data == null) { continue; }
                    if (c.LastUpdate < oldesttime)
                    {
                        oldesttime = c.LastUpdate;
                        oldestpos = p;
                    }
                }
                if (simulationcurrentframe - oldesttime > chunksimulation_every)
                {
                    ChunkUpdate(oldestpos, oldesttime);
                    GameModeFortress.InfiniteMapChunked.Chunk c = map.chunks[oldestpos.x / chunksize, oldestpos.y / chunksize, oldestpos.z / chunksize];
                    c.LastUpdate = simulationcurrentframe;
                    return;
                }
            }
        }
        private void ChunkUpdate(Vector3i p, long lastupdate)
        {
            byte[] chunk = map.GetChunk(p.x, p.y, p.z);
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        int block = chunk[MapUtil.Index(xx, yy, zz, chunksize, chunksize)];
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
            if (MapUtil.IsValidPos(map, pos.x, pos.y, pos.z + 1))
            {
                int blockabove = map.GetBlock(pos.x, pos.y, pos.z + 1);
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
                if (!MapUtil.IsValidPos(map, pos.x, pos.y, pos.z - 1))
                {
                    return;
                }
                int under = map.GetBlock(pos.x, pos.y, pos.z - 1);
                if (!(under == (int)TileTypeMinecraft.Dirt
                    || under == (int)TileTypeMinecraft.Grass
                    || under == (int)TileTypeManicDigger.DirtForFarming))
                {
                    return;
                }
                PlaceTree(pos.x, pos.y, pos.z - 1);
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
            if (MapUtil.IsValidPos(map, x, y, z))
            {
                SetBlockAndNotify(x, y, z, blocktype);
            }
        }
        private void BlockTickDirt(Vector3i pos)
        {
            if (MapUtil.IsValidPos(map, pos.x, pos.y, pos.z + 1))
            {
                int block2 = map.GetBlock(pos.x, pos.y, pos.z + 1);
                if (data.GrassGrowsUnder(block2) && !IsShadow(pos.x, pos.y, pos.z))
                {
                    SetBlockAndNotify(pos.x, pos.y, pos.z, (int)TileTypeMinecraft.Grass);
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
        private bool IsShadow(int x, int y, int z)
        {
            for (int i = 1; i < 10; i++)
            {
                if (MapUtil.IsValidPos(map, x, y, z + i) && !data.GrassGrowsUnder(map.GetBlock(x, y, z + i)))
                {
                    return true;
                }
            }
            return false;
        }
        int CompressUnusedIteration = 0;
        private void CompressUnusedChunks()
        {
            int sizex = map.chunks.GetUpperBound(0) + 1;
            int sizey = map.chunks.GetUpperBound(1) + 1;
            int sizez = map.chunks.GetUpperBound(2) + 1;

            for (int i = 0; i < 100; i++)
            {
                var v = MapUtil.Pos(CompressUnusedIteration, map.MapSizeX / chunksize, map.MapSizeY / chunksize);
                GameModeFortress.InfiniteMapChunked.Chunk c = map.chunks[v.x, v.y, v.z];
                var vg = new Vector3i(v.x * chunksize, v.y * chunksize, v.z * chunksize);
                bool stop = false;
                if (c != null)
                {
                    if (c.compressed == null)
                    {
                        bool compress = true;
                        foreach (var k in clients)
                        {
                            int viewdist = (int)(chunkdrawdistance * chunksize * 1.5f);
                            if (DistanceSquared(PlayerBlockPosition(k.Value), vg) <= viewdist * viewdist)
                            {
                                compress = false;
                            }
                        }
                        if (compress)
                        {
                            c.compressed = CompressChunk(c.data);
                            c.data = null;
                            stop = true;
                        }
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
        int SEND_CHUNKS_PER_SECOND = 10;
        private List<Vector3i> UnknownChunksAroundPlayer(int clientid)
        {
            Client c = clients[clientid];
            List<Vector3i> tosend = new List<Vector3i>();
            Vector3i playerpos = PlayerBlockPosition(c);
            foreach (var v in ChunksAroundPlayer(playerpos))
            {
                if (!c.chunksseen.ContainsKey(v))
                {
                    if (MapUtil.IsValidPos(map, v.x, v.y, v.z))
                    {
                        tosend.Add(v);
                    }
                }
            }
            return tosend;
        }
        private int NotifyMapChunks(int clientid, int limit)
        {
            Client c  = clients[clientid];
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
                byte[] chunk = map.GetChunk(v.x, v.y, v.z);
                byte[] compressedchunk = CompressChunk(chunk);
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
                c.chunksseen.Add(v, true);
                sent++;
            }
            return sent;
        }
        void ChunkDirty(Vector3i pos)
        {
            int x = pos.x / chunksize * chunksize;
            int y = pos.y / chunksize * chunksize;
            int z = pos.z / chunksize * chunksize;
            foreach (var k in clients)
            {
                k.Value.chunksseen.Remove(new Vector3i(x, y, z));
            }
        }
        const string invalidplayername = "invalid";
        private void NotifyFiniteInventory(int clientid)
        {
            Client c = clients[clientid];
            if (c.IsInventoryDirty && c.playername != invalidplayername)
            {
                PacketServerFiniteInventory p;
                if (cfgcreative)
                {
                    p = new PacketServerFiniteInventory()
                    {
                        BlockTypeAmount = StartFiniteInventory(),
                        IsFinite = false,
                    };
                }
                else
                {
                    p = GetPlayerInventory(c.playername);
                }
                SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.FiniteInventory, FiniteInventory = p }));
                c.IsInventoryDirty = false;
            }
        }
        PacketServerFiniteInventory GetPlayerInventory(string playername)
        {
            if (Inventory == null)
            {
                Inventory = new Dictionary<string, PacketServerFiniteInventory>();
            }
            if (!Inventory.ContainsKey(playername))
            {
                Inventory[playername] = new PacketServerFiniteInventory()
                {
                    BlockTypeAmount = StartFiniteInventory(),
                    IsFinite = true,
                    Max = FiniteInventoryMax,
                };
            }
            return Inventory[playername];
        }
        public int FiniteInventoryMax = 200;
        Dictionary<int, int> StartFiniteInventory()
        {
            Dictionary<int, int> d = new Dictionary<int, int>();
            d[(int)TileTypeManicDigger.CraftingTable] = 6;
            d[(int)TileTypeManicDigger.Crops1] = 1;
            return d;
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
            SendMessageToAll(string.Format("Player {0} disconnected.", name));
        }
        Vector3i DefaultSpawnPosition()
        {
            return new Vector3i((map.MapSizeX / 2) * 32,
                        MapUtil.blockheight(map, 0, map.MapSizeX / 2, map.MapSizeY / 2) * 32,
                        (map.MapSizeY / 2) * 32);
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
                    string username = packet.Identification.Username;

                    SendServerIdentification(clientid);
                    foreach (var k in clients)
                    {
                        if (k.Value.playername.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                        {
                            username = GenerateUsername(username);
                            break;
                        }
                    }
                    //todo verificationkey
                    clients[clientid].playername = username;
                    Vector3i position = DefaultSpawnPosition();
                    clients[clientid].PositionMul32GlX = position.x;
                    clients[clientid].PositionMul32GlY = position.y;
                    clients[clientid].PositionMul32GlZ = position.z;
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
                                PlayerName = username,
                                PositionAndOrientation = new PositionAndOrientation()
                                {
                                    X = position.x,
                                    Y = position.y,
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
                    SendMessageToAll(string.Format("Player {0} joins.", username));
                    SendLevel(clientid);
                    break;
                case ClientPacketId.SetBlock:
                    int x = packet.SetBlock.X;
                    int y = packet.SetBlock.Y;
                    int z = packet.SetBlock.Z;
                    BlockSetMode mode = packet.SetBlock.Mode == 0 ? BlockSetMode.Destroy : BlockSetMode.Create;
                    byte blocktype = (byte)packet.SetBlock.BlockType;
                    if (mode == BlockSetMode.Destroy)
                    {
                        blocktype = 0; //data.TileIdEmpty
                    }
                    //todo check block type.
                    //map.SetBlock(x, y, z, blocktype);
                    DoCommandBuild(clientid, true, packet.SetBlock);
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
                    SendMessageToAll(string.Format("{0}: {1}", clients[clientid].playername, packet.Message.Message));
                    break;
                case ClientPacketId.Craft:
                    DoCommandCraft(true, packet.Craft);
                    break;
                default:
                    Console.WriteLine("Invalid packet: {0}, clientid:{1}", packet.PacketId, clientid);
                    break;
            }
            return lengthPrefixLength + packetLength;
        }
        private void NotifyBlock(int x, int y, int z, int blocktype)
        {
            foreach (var k in clients)
            {
                SendSetBlock(k.Key, x, y, z, blocktype);
            }
        }
        bool ENABLE_FINITEINVENTORY { get { return !cfgcreative; } }
        private bool DoCommandCraft(bool execute, PacketClientCraft cmd)
        {
            if (map.GetBlock(cmd.X, cmd.Y, cmd.Z) != (int)TileTypeManicDigger.CraftingTable)
            {
                return false;
            }
            if (cmd.RecipeId < 0 || cmd.RecipeId >= craftingrecipes.craftingrecipes.Count)
            {
                return false;
            }
            List<Vector3i> table = craftingtabletool.GetTable(new Vector3i(cmd.X, cmd.Y, cmd.Z));
            List<int> ontable = craftingtabletool.GetOnTable(table);
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
        private bool DoCommandBuild(int player_id, bool execute, PacketClientSetBlock cmd)
        {
            Vector3 v = new Vector3(cmd.X, cmd.Y, cmd.Z);
            if (ENABLE_FINITEINVENTORY)
            {
                Dictionary<int, int> inventory = GetPlayerInventory(clients[player_id].playername).BlockTypeAmount;
                if (cmd.Mode == (int)BlockSetMode.Create)
                {
                    int oldblock = map.GetBlock(cmd.X, cmd.Y, cmd.Z);
                    int blockstoput = 1;
                    if (GameDataTilesManicDigger.IsRailTile(cmd.BlockType))
                    {
                        if (!(oldblock == data.TileIdEmpty
                            || GameDataTilesManicDigger.IsRailTile(oldblock)))
                        {
                            return false;
                        }
                        //count how many rails will be created
                        int oldrailcount = 0;
                        if (GameDataTilesManicDigger.IsRailTile(oldblock))
                        {
                            oldrailcount = MyLinq.Count(
                                DirectionUtils.ToRailDirections(
                                (RailDirectionFlags)(oldblock - GameDataTilesManicDigger.railstart)));
                        }
                        int newrailcount = MyLinq.Count(
                            DirectionUtils.ToRailDirections(
                            (RailDirectionFlags)(cmd.BlockType - GameDataTilesManicDigger.railstart)));
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
                        if (oldblock != data.TileIdEmpty)
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
                    int blocktype = map.GetBlock(cmd.X, cmd.Y, cmd.Z);
                    blocktype = data.PlayerBuildableMaterialType(blocktype);
                    if ((!data.IsValidTileType(blocktype))
                        || blocktype == data.TileIdEmpty)
                    {
                        return false;
                    }
                    int blockstopick = 1;
                    if (GameDataTilesManicDigger.IsRailTile(blocktype))
                    {
                        blockstopick = MyLinq.Count(
                            DirectionUtils.ToRailDirections(
                            (RailDirectionFlags)(blocktype - GameDataTilesManicDigger.railstart)));
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
            if (execute)
            {
                int tiletype = cmd.Mode == (int)BlockSetMode.Create ?
                    (byte)cmd.BlockType : data.TileIdEmpty;
                SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, tiletype);
            }
            return true;
        }
        void SetBlockAndNotify(int x, int y, int z, int blocktype)
        {
            map.SetBlock(x, y, z, blocktype);
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
            if (GameDataTilesManicDigger.IsRailTile(blocktypea) && GameDataTilesManicDigger.IsRailTile(blocktypeb))
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
            string truncated = message.Substring(0, Math.Min(64, message.Length));

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
                using (SocketAsyncEventArgs e = new SocketAsyncEventArgs())
                {
                    e.SetBuffer(packet, 0, packet.Length);
                    clients[clientid].socket.SendAsync(e);
                }
            }
            catch (Exception e)
            {
                KillPlayer(clientid);
            }
        }
        int drawdistance = 128;
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
                    for (int z = 0; z < map.MapSizeZ / chunksize; z++)
                    {
                        yield return new Vector3i(playerpos.x + x*chunksize, playerpos.y + y*chunksize, z*chunksize);
                    }
                }
            }
        }
        byte[] CompressChunk(byte[] chunk)
        {
            return GzipCompression.Compress(chunk);
        }
        byte[] CompressChunk(byte[, ,] chunk)
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
            byte[] compressedchunk = GzipCompression.Compress(ms.ToArray());
            return compressedchunk;
        }
        private void SendLevel(int clientid)
        {
            SendLevelInitialize(clientid);
            List<Vector3i> unknown = UnknownChunksAroundPlayer(clientid);
            int sent = 0;
            for (int i = 0; i < unknown.Count; i++)
            {
                sent += NotifyMapChunks(clientid, 5);
                SendLevelDataChunk(clientid, null, 0, (int)(((float)sent / unknown.Count) * 100));
            }
            SendLevelFinalize(clientid);
        }
        int levelchunksize = 1024;
        private void SendLevelInitialize(int clientid)
        {
            PacketServerLevelInitialize p = new PacketServerLevelInitialize() { };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.LevelInitialize, LevelInitialize = p }));
        }
        private void SendLevelDataChunk(int clientid, byte[] chunk, int chunklength, int percentcomplete)
        {
            PacketServerLevelDataChunk p = new PacketServerLevelDataChunk() { Chunk = chunk, PercentComplete = percentcomplete };
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
                ServerName = cfgname,
                ServerMotd = cfgmotd,
            };
            SendPacket(clientid, Serialize(new PacketServer() { PacketId = ServerPacketId.ServerIdentification, Identification = p }));
        }
        public int SIMULATION_KEYFRAME_EVERY = 4;
        public float SIMULATION_STEP_LENGTH = 1f / 64f;
        class Client
        {
            public Socket socket;
            public List<byte> received = new List<byte>();
            public string playername = invalidplayername;
            public int PositionMul32GlX;
            public int PositionMul32GlY;
            public int PositionMul32GlZ;
            public int positionheading;
            public int positionpitch;
            public Dictionary<Vector3i, bool> chunksseen = new Dictionary<Vector3i, bool>();
            public ManicDigger.Timer notifyMapTimer;
            public bool IsInventoryDirty = true;
        }
        Dictionary<int, Client> clients = new Dictionary<int, Client>();
    }
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server();
            //s.map = server.map;
            
            /*
            var g = new GameModeFortress.GameFortress();
            var data = new GameModeFortress.GameDataTilesManicDigger();
            data.CurrentSeason = g;
            g.audio = new AudioDummy();
            g.data = data;
            var gen = new GameModeFortress.WorldGeneratorSandbox();
            g.map = new GameModeFortress.InfiniteMap() { gen = gen };
            //g.worldgeneratorsandbox = gen;
            g.network = new NetworkClientDummy();
            //g.physics = new CharacterPhysics() { data = data, map = g.map };
            g.terrain = new TerrainDrawerDummy();
            g.viewport = new ViewportDummy();
            //g.the3d = new The3dDummy();
            //g.getfile = new GetFilePathDummy();
            s.gameworld = g;
            g.generator = File.ReadAllText("WorldGenerator.cs");
            int seed = new Random().Next();
            gen.Compile(g.generator, seed);
            g.Seed = seed;
            s.players = g;
            var shadows = new ShadowsSimple() { data = data, map = g };
            g.shadows = shadows;
            g.map.shadows = shadows;
            g.minecartdrawer = new GameModeFortress.MinecartDrawerDummy();
            */
            var map = new GameModeFortress.InfiniteMapChunked();
            map.chunksize = 32;
            var generator = new WorldGenerator();
            map.generator = generator;
            s.chunksize = 32;
            map.Reset(10000, 10000, 128);
            s.map = map;
            s.generator = generator;
            s.data = new GameDataTilesManicDigger();
            s.craftingtabletool = new CraftingTableTool() { map = map };

            if (Debugger.IsAttached)
            {
                new DependencyChecker(typeof(InjectAttribute)).CheckDependencies(
                    s);
            }

            s.Start();
            new Thread((a) => { for (; ; ) { s.SendHeartbeat(); Thread.Sleep(TimeSpan.FromMinutes(1)); } }).Start();
            for (; ; )
            {
                s.Process();
                Thread.Sleep(1);
            }
        }
    }
}