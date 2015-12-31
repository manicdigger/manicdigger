using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using ManicDigger;
using OpenTK;
using ProtoBuf;
using System.Xml.Serialization;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using Jint.Delegates;
using System.Diagnostics;
using ManicDigger.ClientNative;



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
    [ProtoMember(11, IsRequired = false)]
    public Dictionary<string, byte[]> moddata;
    [ProtoMember(12, IsRequired = false)]
    public long TimeOfDay;
}
public partial class Server : ICurrentTime, IDropItem
{
    public Server()
    {
        serverPlatform = new ServerPlatformNative();

        server = new ServerCi();
        systems = new ServerSystem[256];
        // This ServerSystem should always be loaded first
        systems[systemsCount++] = new ServerSystemLoadFirst();

        // Regular ServerSystems
        systems[systemsCount++] = new ServerSystemLoadConfig();
        systems[systemsCount++] = new ServerSystemHeartbeat();
        systems[systemsCount++] = new ServerSystemHttpServer();
        systems[systemsCount++] = new ServerSystemUnloadUnusedChunks();
        systems[systemsCount++] = new ServerSystemNotifyMap();
        systems[systemsCount++] = new ServerSystemNotifyPing();
        systems[systemsCount++] = new ServerSystemChunksSimulation();
        systems[systemsCount++] = new ServerSystemBanList();
        systems[systemsCount++] = new ServerSystemModLoader();
        systems[systemsCount++] = new ServerSystemLoadServerClient();
        systems[systemsCount++] = new ServerSystemNotifyEntities();
        systems[systemsCount++] = new ServerSystemMonsterWalk();

        // This ServerSystem should always be loaded last
        systems[systemsCount++] = new ServerSystemLoadLast();

        // Not finished
        // systems[systemsCount++] = new ServerSystemSign();
        // systems[systemsCount++] = new ServerSystemPermissionSign();

        //Load translations
        gameplatform = new GamePlatformNative();
        language.platform = gameplatform;
        language.LoadTranslations();
    }
    

    internal ServerCi server;
    internal ServerSystem[] systems;
    internal int systemsCount;
    internal ServerPlatform serverPlatform;

    GamePlatform gameplatform;
    public GameExit exit;
    public ServerMap d_Map;
    public GameData d_Data;
    public CraftingTableTool d_CraftingTableTool;
    public IGetFileStream d_GetFile;
    public IChunkDb d_ChunkDb;
    public ICompression d_NetworkCompression;
    public NetServer[] mainSockets { get { return server.mainSockets; } set { server.mainSockets = value; } }
    public int mainSocketsCount { get { return server.mainSocketsCount; } set { server.mainSocketsCount = value; } }
    
    public bool LocalConnectionsOnly { get; set; }
    public string[] PublicDataPaths = new string[0];
    public int singleplayerport = 25570;
    public Random rnd = new Random();
    public int SpawnPositionRandomizationRange = 96;
    public bool IsMono = Type.GetType("Mono.Runtime") != null;

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
            Console.WriteLine(language.ServerCannotWriteLog(), filename);
        }
    }
    public void ServerEventLog(string p)
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
            Console.WriteLine(language.ServerCannotWriteLog(), filename);
        }
    }
    public void ChatLog(string p)
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
            Console.WriteLine(language.ServerCannotWriteLog(), filename);
        }
    }

    public bool Public;
    public bool enableshadows = true;
    public Language language = new Language();

    Stopwatch stopwatchDt = new Stopwatch();
    public Stopwatch serverUptime = new Stopwatch();
    public void Process()
    {
        try
        {
            float dt = (float)stopwatchDt.Elapsed.TotalSeconds;
            stopwatchDt.Reset();
            stopwatchDt.Start();

            for (int i = 0; i < systemsCount; i++)
            {
                systems[i].Update(this, dt);
            }
            //Save data
            ProcessSave();
            //Do server stuff
            ProcessMain();

            //When a value of 0 or less is given, don't restart
            if (config.AutoRestartCycle > 0 && serverUptime.Elapsed.TotalHours >= config.AutoRestartCycle)
            {
                //Restart interval elapsed
                Restart();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public void ProcessSave()
    {
        if ((DateTime.UtcNow - lastsave).TotalMinutes > 2)
        {
            DateTime start = DateTime.UtcNow;
            SaveGlobalData();
            Console.WriteLine(language.ServerGameSaved(), (DateTime.UtcNow - start));
            lastsave = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Tell the clients the time
    /// </summary>
    private void NotifySeason(int clientid)
    {
        if (clients[clientid].state == ClientStateOnServer.Connecting)
        {
            return;
        }

        Packet_ServerSeason p = new Packet_ServerSeason()
        {
            Hour = _time.GetQuarterHourPartOfDay(),

            //DayNightCycleSpeedup is used by the client like this:
            //day_length_in_seconds = SecondsInADay / packet.Season.DayNightCycleSpeedup;

            //Set it to 1 if we froze the time, to prevent a division by zero
            DayNightCycleSpeedup = (_time.SpeedOfTime != 0) ? _time.SpeedOfTime : 1,
            Moon = 0,
        };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.Season, Season = p }));
    }

    private GameTime _time = new GameTime();
    private int _nLastHourChangeNotify = 0;

    public void ProcessMain()
    {
        if (server.mainSockets == null)
        {
            return;
        }

        if (_time.Tick())
        {
            if (_time.GetQuarterHourPartOfDay() != _nLastHourChangeNotify)
            {
//#if DEBUG
//                SendMessageToAll("Time of day: " + _time.Time.ToString(@"hh\:mm\:ss") + " Day: " + (int)_time.Time.Days);
//#endif
                //notify clients about the time
                _nLastHourChangeNotify = _time.GetQuarterHourPartOfDay();

                foreach (var c in clients)
                {
                    NotifySeason(c.Key);
                }
            }
        }

        double currenttime = gettime() - starttime;
        double deltaTime = currenttime - oldtime;
        accumulator += deltaTime;
        double dt = SIMULATION_STEP_LENGTH;
        while (accumulator > dt)
        {
            simulationcurrentframe++;
            accumulator -= dt;
        }
        oldtime = currenttime;

        NetIncomingMessage msg;
        Stopwatch s = new Stopwatch();
        s.Start();

        //Process client packets
        for (int i = 0; i < mainSocketsCount; i++)
        {
            NetServer mainSocket = mainSockets[i];
            if (mainSocket == null)
            {
                continue;
            }
            while ((msg = mainSocket.ReadMessage()) != null)
            {
                ProcessNetMessage(msg, mainSocket, s);
            }
        }
        foreach (var k in clients)
        {
            k.Value.socket.Update();
        }

        //Send updates to player
        foreach (var k in clients)
        {
            //k.Value.notifyMapTimer.Update(delegate { NotifyMapChunks(k.Key, 1); });
            NotifyInventory(k.Key);
            NotifyPlayerStats(k.Key);
        }

        //Process Mod timers
        foreach (var k in timers)
        {
            k.Key.Update(k.Value);
        }

        //Reset data displayed in /stat
        if ((DateTime.UtcNow - statsupdate).TotalSeconds >= 2)
        {
            statsupdate = DateTime.UtcNow;
            StatTotalPackets = 0;
            StatTotalPacketsLength = 0;
        }

        //Determine how long it took all operations to finish
        lastServerTick = s.ElapsedMilliseconds;
        if (lastServerTick > 500)
        {
            //Print an error if the value gets too big - TODO: Adjust
            Console.WriteLine("Server process takes too long! Overloaded? ({0}ms)", lastServerTick);
        }
    }

    public void OnConfigLoaded()
    {
        //Initialize server map
        var map = new ServerMap();
        map.server = this;
        map.d_CurrentTime = this;
        map.chunksize = 32;
        for (int i = 0; i < BlockTypes.Length; i++)
        {
            BlockTypes[i] = new BlockType() { };
        }
        map.d_Heightmap = new InfiniteMapChunked2dServer() { chunksize = Server.chunksize, d_Map = map };
        map.Reset(config.MapSizeX, config.MapSizeY, config.MapSizeZ);
        d_Map = map;

        //Load assets (textures, sounds, etc.)
        string[] datapaths = new[] { Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"), "data" };
        string[] datapathspublic = new[] { Path.Combine(datapaths[0], "public"), Path.Combine(datapaths[1], "public") };
        PublicDataPaths = datapathspublic;
        assetLoader = new AssetLoader(datapathspublic);
        LoadAssets();
        var getfile = new GetFileStream(datapaths);
        d_GetFile = getfile;

        //Initialize game components
        var data = new GameData();
        data.Start();
        d_Data = data;
        d_CraftingTableTool = new CraftingTableTool() { d_Map = map, d_Data = data };
        LocalConnectionsOnly = !Public;
        var networkcompression = new CompressionGzip();
        var diskcompression = new CompressionGzip();
        var chunkdb = new ChunkDbCompressed() { d_ChunkDb = new ChunkDbSqlite(), d_Compression = diskcompression };
        d_ChunkDb = chunkdb;
        map.d_ChunkDb = chunkdb;
        d_NetworkCompression = networkcompression;
        d_DataItems = new GameDataItemsBlocks() { d_Data = data };
        if (server.mainSockets == null)
        {
            server.mainSockets = new NetServer[3];
            server.mainSocketsCount = 3;
            mainSockets[0] = new EnetNetServer() { platform = gameplatform };
            if (mainSockets[1] == null)
            {
                mainSockets[1] = new WebSocketNetServer();
            }
            if (mainSockets[2] == null)
            {
                mainSockets[2] = new TcpNetServer();
            }
        }

        all_privileges.AddRange(ServerClientMisc.Privilege.All());

        //Load the savegame file
        {
            if (!Directory.Exists(GameStorePath.gamepathsaves))
            {
                Directory.CreateDirectory(GameStorePath.gamepathsaves);
            }
            Console.WriteLine(language.ServerLoadingSavegame());
            if (!File.Exists(GetSaveFilename()))
            {
                Console.WriteLine(language.ServerCreatingSavegame());
            }
            LoadGame(GetSaveFilename());
            Console.WriteLine(language.ServerLoadedSavegame() + GetSaveFilename());
        }
        if (LocalConnectionsOnly)
        {
            config.Port = singleplayerport;
        }
        Start(config.Port);

        // server monitor
        if (config.ServerMonitor)
        {
            this.serverMonitor = new ServerMonitor(this, exit);
            this.serverMonitor.Start();
        }

        // set up server console interpreter
        this.serverConsoleClient = new ClientOnServer()
        {
            Id = serverConsoleId,
            playername = "Server",
            queryClient = false
        };
        ManicDigger.Group serverGroup = new ManicDigger.Group();
        serverGroup.Name = "Server";
        serverGroup.Level = 255;
        serverGroup.GroupPrivileges = new List<string>();
        serverGroup.GroupPrivileges = all_privileges;
        serverGroup.GroupColor = ServerClientMisc.ClientColor.Red;
        serverConsoleClient.AssignGroup(serverGroup);
        serverConsole = new ServerConsole(this, exit);

        if (config.AutoRestartCycle > 0)
        {
            Console.WriteLine("AutoRestartInterval: {0}", config.AutoRestartCycle);
        }
        else
        {
            Console.WriteLine("AutoRestartInterval: DISABLED");
        }
        serverUptime.Start();
    }
    void Start(int port)
    {
        Port = port;
        mainSockets[0].SetPort(port);
        mainSockets[0].Start();
        if (mainSockets[1] != null)
        {
            mainSockets[1].SetPort(port);
            mainSockets[1].Start();
        }
        if (mainSockets[2] != null)
        {
            mainSockets[2].SetPort(port + 2);
            mainSockets[2].Start();
        }
    }
    internal int Port;
    public void Stop()
    {
        Console.WriteLine("[SERVER] Doing last tick...");
        ProcessMain();
        //Maybe inform mods about shutdown?
        Console.WriteLine("[SERVER] Saving data...");
        DateTime start = DateTime.UtcNow;
        SaveGlobalData();
        Console.WriteLine(language.ServerGameSaved(), (DateTime.UtcNow - start));
        Console.WriteLine("[SERVER] Stopped the server!");
    }
    public void Restart()
    {
        //Server shall exit and be restarted
        exit.SetRestart(true);
        exit.SetExit(true);
    }
    public void Exit()
    {
        //Server shall be shutdown
        exit.SetRestart(false);
        exit.SetExit(true);
    }

    public List<string> all_privileges = new List<string>();

    public List<string> ModPaths = new List<string>();
    public ModManager1 modManager;

    internal string gameMode = "Fortress";

    private ServerMonitor serverMonitor;
    private ServerConsole serverConsole;
    private int serverConsoleId = -1; // make sure that not a regular client is assigned this ID
    public int ServerConsoleId { get { return serverConsoleId; } }
    internal ClientOnServer serverConsoleClient;
    public void ReceiveServerConsole(string message)
    {
        // empty message
        if (string.IsNullOrEmpty(message))
        {
            //Ignore empty messages
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

    public int Seed;
    private void LoadGame(string filename)
    {
        d_ChunkDb.Open(filename);
        byte[] globaldata = d_ChunkDb.GetGlobalData();
        if (globaldata == null)
        {
            //no savegame yet
            if (config.RandomSeed)
            {
                Seed = new Random().Next();
            }
            else
            {
                Seed = config.Seed;
            }
            MemoryStream ms = new MemoryStream();
            SaveGame(ms);
            d_ChunkDb.SetGlobalData(ms.ToArray());
            this._time.Init(TimeSpan.Parse("08:00").Ticks);
            return;
        }
        ManicDiggerSave save = Serializer.Deserialize<ManicDiggerSave>(new MemoryStream(globaldata));
        Seed = save.Seed;
        d_Map.Reset(d_Map.MapSizeX, d_Map.MapSizeY, d_Map.MapSizeZ);
        if (config.IsCreative) this.Inventory = Inventory = new Dictionary<string, PacketServerInventory>(StringComparer.InvariantCultureIgnoreCase);
        else this.Inventory = save.Inventory;
        this.PlayerStats = save.PlayerStats;
        this.simulationcurrentframe = (int)save.SimulationCurrentFrame;
        this._time.Init(save.TimeOfDay);
        this.LastMonsterId = save.LastMonsterId;
        this.moddata = save.moddata;
    }
    public List<ManicDigger.Action> onload = new List<ManicDigger.Action>();
    public List<ManicDigger.Action> onsave = new List<ManicDigger.Action>();
    public int LastMonsterId;
    public Dictionary<string, PacketServerInventory> Inventory = new Dictionary<string, PacketServerInventory>(StringComparer.InvariantCultureIgnoreCase);
    public Dictionary<string, PacketServerPlayerStats> PlayerStats = new Dictionary<string, PacketServerPlayerStats>(StringComparer.InvariantCultureIgnoreCase);
    public void SaveGame(Stream s)
    {
        for (int i = 0; i < onsave.Count; i++)
        {
            onsave[i]();
        }
        ManicDiggerSave save = new ManicDiggerSave();
        SaveAllLoadedChunks();
        if (!config.IsCreative)
        {
            save.Inventory = Inventory;
        }
        save.PlayerStats = PlayerStats;
        save.Seed = Seed;
        save.SimulationCurrentFrame = simulationcurrentframe;
        save.TimeOfDay = _time.Time.Ticks;
        save.LastMonsterId = LastMonsterId;
        save.moddata = moddata;
        Serializer.Serialize(s, save);
    }
    public Dictionary<string, byte[]> moddata = new Dictionary<string, byte[]>();
    public bool BackupDatabase(string backupFilename)
    {
        if (!GameStorePath.IsValidName(backupFilename))
        {
            Console.WriteLine(language.ServerInvalidBackupName() + backupFilename);
            return false;
        }
        if (!Directory.Exists(GameStorePath.gamepathbackup))
        {
            Directory.CreateDirectory(GameStorePath.gamepathbackup);
        }
        string finalFilename = Path.Combine(GameStorePath.gamepathbackup, backupFilename + MapManipulator.BinSaveExtension);
        d_ChunkDb.Backup(finalFilename);
        return true;
    }
    public bool LoadDatabase(string filename)
    {
        d_Map.d_ChunkDb = d_ChunkDb;
        SaveAll();
        if (filename != GetSaveFilename())
        {
            //todo load
        }
        var dbcompressed = (ChunkDbCompressed)d_Map.d_ChunkDb;
        var db = (ChunkDbSqlite)dbcompressed.d_ChunkDb;
        db.temporaryChunks = new Dictionary<ulong, byte[]>();
        d_Map.Clear();
        LoadGame(filename);
        foreach (var k in clients)
        {
            //SendLevelInitialize(k.Key);
            Array.Clear(k.Value.chunksseen, 0, k.Value.chunksseen.Length);
            k.Value.chunksseenTime.Clear();
        }
        return true;
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
                    ServerChunk c = d_Map.GetChunkValid(cx, cy, cz);
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
    public string GetSaveFilename()
    {
        if (SaveFilenameOverride != null)
        {
            return SaveFilenameOverride;
        }
        return Path.Combine(GameStorePath.gamepathsaves, SaveFilenameWithoutExtension + MapManipulator.BinSaveExtension);
    }

    private void SaveGlobalData()
    {
        MemoryStream ms = new MemoryStream();
        SaveGame(ms);
        d_ChunkDb.SetGlobalData(ms.ToArray());
    }
    DateTime lastsave = DateTime.UtcNow;

    public ServerConfig config;
    public ServerBanlist banlist;
    public bool configNeedsSaving;

    public void Dispose()
    {
        if (!disposed)
        {
            //d_MainSocket.Disconnect(false);
        }
        disposed = true;
    }
    bool disposed = false;
    double starttime = gettime();
    static double gettime()
    {
        return (double)DateTime.UtcNow.Ticks / (10 * 1000 * 1000);
    }
    internal int simulationcurrentframe;
    double oldtime;
    double accumulator;
    float lastServerTick;

    private int lastClientId;
    public int GenerateClientId()
    {
        int i = 0;
        while (clients.ContainsKey(i))
        {
            i++;
        }
        return i;
    }

    public GamePlatformNative platform = new GamePlatformNative();

    private void ProcessNetMessage(NetIncomingMessage msg, NetServer mainSocket, Stopwatch s)
    {
        if (msg.SenderConnection == null)
        {
            return;
        }
        int clientid = -1;
        foreach (var k in clients)
        {
            if (k.Value.mainSocket != mainSocket)
            {
                continue;
            }
            if (k.Value.socket.EqualsConnection(msg.SenderConnection))
            {
                clientid = k.Key;
            }
        }
        switch (msg.Type)
        {
            case NetworkMessageType.Connect:
                //new connection
                //ISocket client1 = d_MainSocket.Accept();
                NetConnection client1 = msg.SenderConnection;
                IPEndPointCi iep1 = client1.RemoteEndPoint();

                ClientOnServer c = new ClientOnServer();
                c.mainSocket = mainSocket;
                c.socket = client1;
                c.Ping.SetTimeoutValue(config.ClientConnectionTimeout);
                c.chunksseen = new bool[d_Map.MapSizeX / chunksize * d_Map.MapSizeY / chunksize * d_Map.MapSizeZ / chunksize];
                lock (clients)
                {
                    this.lastClientId = this.GenerateClientId();
                    c.Id = lastClientId;
                    clients[lastClientId] = c;
                }
                //clientid = c.Id;
                c.notifyMapTimer = new Timer()
                {
                    INTERVAL = 1.0 / SEND_CHUNKS_PER_SECOND,
                };
                c.notifyMonstersTimer = new Timer()
                {
                    INTERVAL = 1.0 / SEND_MONSTER_UDAPTES_PER_SECOND,
                };
                break;
            case NetworkMessageType.Data:
                if (clientid == -1)
                {
                    break;
                }

                // process packet
                try
                {
                    TotalReceivedBytes += msg.messageLength;
                    TryReadPacket(clientid, msg.message);
                }
                catch (Exception e)
                {
                    //client problem. disconnect client.
                    Console.WriteLine("Exception at client " + clientid + ". Disconnecting client.");
                    SendPacket(clientid, ServerPackets.DisconnectPlayer(language.ServerClientException()));
                    KillPlayer(clientid);
                    Console.WriteLine(e.ToString());
                }
                break;
            case NetworkMessageType.Disconnect:
                Console.WriteLine("Client disconnected.");
                KillPlayer(clientid);
                break;
        }
    }

    DateTime statsupdate;

    public Dictionary<Timer, Timer.Tick> timers = new Dictionary<Timer, Timer.Tick>();

    private void NotifyPing(int targetClientId, int ping)
    {
        foreach (var k in clients)
        {
            SendPlayerPing(k.Key, targetClientId, ping);
        }
    }
    private void SendPlayerPing(int recipientClientId, int targetClientId, int ping)
    {
        Packet_ServerPlayerPing p = new Packet_ServerPlayerPing()
        {
            ClientId = targetClientId,
            Ping = ping
        };
        SendPacket(recipientClientId, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.PlayerPing, PlayerPing = p }));
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
                    if (d_Map.GetChunkValid(x, y, z) != null)
                    {
                        DoSaveChunk(x, y, z, d_Map.GetChunkValid(x, y, z));
                    }
                }
            }
        }
        SaveGlobalData();
    }

    internal void DoSaveChunk(int x, int y, int z, ServerChunk c)
    {
        MemoryStream ms = new MemoryStream();
        Serializer.Serialize(ms, c);
        ChunkDb.SetChunk(d_ChunkDb, x, y, z, ms.ToArray());
    }

    int SEND_CHUNKS_PER_SECOND = 10;
    int SEND_MONSTER_UDAPTES_PER_SECOND = 3;

    public void LoadChunk(int cx, int cy, int cz)
    {
        d_Map.LoadChunk(cx, cy, cz);
    }

    public const string invalidplayername = "invalid";
    public void NotifyInventory(int clientid)
    {
        ClientOnServer c = clients[clientid];
        if (c.IsInventoryDirty && c.playername != invalidplayername && !c.usingFill)
        {
            Packet_ServerInventory p;
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
                p = ConvertInventory(GetPlayerInventory(c.playername));
            }
            SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.FiniteInventory, Inventory = p }));
            c.IsInventoryDirty = false;
        }
    }

    private Packet_ServerInventory ConvertInventory(PacketServerInventory inv)
    {
        if (inv == null)
        {
            return null;
        }
        Packet_ServerInventory p = new Packet_ServerInventory();
        if (inv.Inventory != null)
        {
            p.Inventory = new Packet_Inventory();
            p.Inventory.Boots = ConvertItem(inv.Inventory.Boots);
            p.Inventory.DragDropItem = ConvertItem(inv.Inventory.DragDropItem);
            p.Inventory.Gauntlet = ConvertItem(inv.Inventory.Gauntlet);
            p.Inventory.Helmet = ConvertItem(inv.Inventory.Helmet);
            // todo
            //p.Inventory.Items = inv.Inventory.Items;
            p.Inventory.Items = new Packet_PositionItem[inv.Inventory.Items.Count];
            p.Inventory.ItemsCount = inv.Inventory.Items.Count;
            p.Inventory.ItemsLength = inv.Inventory.Items.Count;
            {
                int i = 0;
                foreach (var k in inv.Inventory.Items)
                {
                    Packet_PositionItem item = new Packet_PositionItem();
                    item.Key_ = SerializePoint(k.Key.X, k.Key.Y);
                    item.Value_ = ConvertItem(k.Value);
                    item.X = k.Key.X;
                    item.Y = k.Key.Y;
                    p.Inventory.Items[i++] = item;
                }
            }
            p.Inventory.MainArmor = ConvertItem(inv.Inventory.MainArmor);
            p.Inventory.RightHand = new Packet_Item[10];
            p.Inventory.RightHandCount = 10;
            p.Inventory.RightHandLength = 10;
            for (int i = 0; i < inv.Inventory.RightHand.Length; i++)
            {
                if (inv.Inventory.RightHand[i] == null)
                {
                    p.Inventory.RightHand[i] = new Packet_Item();
                }
                else
                {
                    p.Inventory.RightHand[i] = ConvertItem(inv.Inventory.RightHand[i]);
                }
            }
        }
        return p;
    }

    private string SerializePoint(int x, int y)
    {
        return x.ToString() + " " + y.ToString();
    }

    private Packet_Item ConvertItem(Item item)
    {
        if (item == null)
        {
            return null;
        }
        Packet_Item p = new Packet_Item();
        p.BlockCount = item.BlockCount;
        p.BlockId = item.BlockId;
        p.ItemClass = (int)item.ItemClass;
        p.ItemId = item.ItemId;
        return p;
    }

    public void NotifyPlayerStats(int clientid)
    {
        ClientOnServer c = clients[clientid];
        if (c.IsPlayerStatsDirty && c.playername != invalidplayername)
        {
            PacketServerPlayerStats stats = GetPlayerStats(c.playername);
            SendPacket(clientid, ServerPackets.PlayerStats(stats.CurrentHealth, stats.MaxHealth, stats.CurrentOxygen, stats.MaxOxygen));
            c.IsPlayerStatsDirty = false;
        }
    }

    private void HitMonsters(int clientid, int health)
    {
        ClientOnServer c = clients[clientid];
        int mapx = c.PositionMul32GlX / 32;
        int mapy = c.PositionMul32GlZ / 32;
        int mapz = c.PositionMul32GlY / 32;
        //3x3x3 chunks
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
                    ServerChunk chunk = d_Map.GetChunkValid(cx, cy, cz);
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
                                SendSound(clientid, "death.wav", m.X, m.Y, m.Z);
                                break;
                            }
                            SendSound(clientid, "grunt2.wav", m.X, m.Y, m.Z);
                            break;
                        }
                    }
                }
            }
        }
    }
    public PacketServerInventory GetPlayerInventory(string playername)
    {
        if (Inventory == null)
        {
            Inventory = new Dictionary<string, PacketServerInventory>(StringComparer.InvariantCultureIgnoreCase);
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
    public void ResetPlayerInventory(ClientOnServer client)
    {
        if (Inventory == null)
        {
            Inventory = new Dictionary<string, PacketServerInventory>(StringComparer.InvariantCultureIgnoreCase);
        }
        this.Inventory[client.playername] = new PacketServerInventory()
        {
            Inventory = StartInventory(),
        };
        client.IsInventoryDirty = true;
        NotifyInventory(client.Id);
    }

    public PacketServerPlayerStats GetPlayerStats(string playername)
    {
        if (PlayerStats == null)
        {
            PlayerStats = new Dictionary<string, PacketServerPlayerStats>(StringComparer.InvariantCultureIgnoreCase);
        }
        if (!PlayerStats.ContainsKey(playername))
        {
            PlayerStats[playername] = StartPlayerStats();
        }
        return PlayerStats[playername];
    }
    public int FiniteInventoryMax = 200;

    Inventory StartInventory()
    {
        Inventory inv = ManicDigger.Inventory.Create();
        int x = 0;
        int y = 0;
        for (int i = 0; i < d_Data.StartInventoryAmount().Length; i++)
        {
            int amount = d_Data.StartInventoryAmount()[i];
            if (config.IsCreative)
            {
                if (amount > 0 || BlockTypes[i].IsBuildable)
                {
                    inv.Items.Add(new ProtoPoint(x, y), new Item() { ItemClass = ItemClass.Block, BlockId = i, BlockCount = 0 });
                    x++;
                    if (x >= GetInventoryUtil(inv).CellCountX)
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
                if (x >= GetInventoryUtil(inv).CellCountX)
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
        p.CurrentOxygen = 10;
        p.MaxOxygen = 10;
        return p;
    }
    public Vector3i PlayerBlockPosition(ClientOnServer c)
    {
        return new Vector3i(c.PositionMul32GlX / 32, c.PositionMul32GlZ / 32, c.PositionMul32GlY / 32);
    }
    public int DistanceSquared(Vector3i a, Vector3i b)
    {
        int dx = a.x - b.x;
        int dy = a.y - b.y;
        int dz = a.z - b.z;
        return dx * dx + dy * dy + dz * dz;
    }
    public void KillPlayer(int clientid)
    {
        if (!clients.ContainsKey(clientid))
        {
            return;
        }
        if (clients[clientid].queryClient)
        {
            clients.Remove(clientid);
            this.serverMonitor.RemoveMonitorClient(clientid);
            return;
        }
        for (int i = 0; i < modEventHandlers.onplayerleave.Count; i++)
        {
            try
            {
                modEventHandlers.onplayerleave[i](clientid);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mod exception: OnPlayerLeave");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        for (int i = 0; i < modEventHandlers.onplayerdisconnect.Count; i++)
        {
            try
            {
                modEventHandlers.onplayerdisconnect[i](clientid);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mod exception: OnPlayerDisconnect");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        string coloredName = clients[clientid].ColoredPlayername(colorNormal);
        string name = clients[clientid].playername;
        clients.Remove(clientid);
        if (config.ServerMonitor)
        {
            this.serverMonitor.RemoveMonitorClient(clientid);
        }
        foreach (var k in clients)
        {
            SendPacket(k.Key, ServerPackets.EntityDespawn(clientid));
        }
        if (name != "invalid")
        {
            SendMessageToAll(string.Format(language.ServerPlayerDisconnect(), coloredName));
            ServerEventLog(string.Format("{0} disconnects.", name));
        }
    }
    internal string ReceivedKey;
    private DateTime lastQuery = DateTime.UtcNow;
    private void TryReadPacket(int clientid, byte[] data)
    {
        ClientOnServer c = clients[clientid];
        //PacketClient packet = Serializer.Deserialize<PacketClient>(new MemoryStream(data));
        Packet_Client packet = new Packet_Client();
        Packet_ClientSerializer.DeserializeBuffer(data, data.Length, packet);
        if (c.queryClient)
        {
            if (!(packet.Id == Packet_ClientIdEnum.ServerQuery || packet.Id == Packet_ClientIdEnum.PlayerIdentification))
            {
                //Reject all packets other than ServerQuery or PlayerIdentification
                Console.WriteLine("Rejected packet from not authenticated client");
                SendPacket(clientid, ServerPackets.DisconnectPlayer("Either send PlayerIdentification or ServerQuery!"));
                KillPlayer(clientid);
                return;
            }
        }
        if (config.ServerMonitor && !this.serverMonitor.CheckPacket(clientid, packet))
        {
            //Console.WriteLine("Server monitor rejected packet");
            return;
        }
        int realPlayers = 0;
        switch (packet.Id)
        {
            case Packet_ClientIdEnum.PingReply:
                clients[clientid].Ping.Receive(platform);
                clients[clientid].LastPing = ((float)clients[clientid].Ping.RoundtripTimeTotalMilliseconds() / 1000);
                this.NotifyPing(clientid, (int)clients[clientid].Ping.RoundtripTimeTotalMilliseconds());
                break;
            case Packet_ClientIdEnum.PlayerIdentification:
                {
                    foreach (var cl in clients)
                    {
                        if (cl.Value.IsBot)
                        {
                            continue;
                        }
                        realPlayers++;
                    }
                    if (realPlayers > config.MaxClients)
                    {
                        SendPacket(clientid, ServerPackets.DisconnectPlayer(language.ServerTooManyPlayers()));
                        KillPlayer(clientid);
                        break;
                    }
                    if (config.IsPasswordProtected() && packet.Identification.ServerPassword != config.Password)
                    {
                        Console.WriteLine(string.Format("{0} fails to join (invalid server password).", packet.Identification.Username));
                        ServerEventLog(string.Format("{0} fails to join (invalid server password).", packet.Identification.Username));
                        SendPacket(clientid, ServerPackets.DisconnectPlayer(language.ServerPasswordInvalid()));
                        KillPlayer(clientid);
                        break;
                    }
                    SendServerIdentification(clientid);
                    string username = packet.Identification.Username;

                    // allowed characters in username: a-z,A-Z,0-9,-,_ length: 1-16
                    Regex allowedUsername = new Regex(@"^(\w|-){1,16}$");

                    if (string.IsNullOrEmpty(username) || !allowedUsername.IsMatch(username))
                    {
                        SendPacket(clientid, ServerPackets.DisconnectPlayer(language.ServerUsernameInvalid()));
                        ServerEventLog(string.Format("{0} can't join (invalid username: {1}).", (c.socket.RemoteEndPoint()).AddressToString(), username));
                        KillPlayer(clientid);
                        break;
                    }

                    bool isClientLocalhost = ((c.socket.RemoteEndPoint()).AddressToString() == "127.0.0.1");
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
                        SendPacket(clientid, ServerPackets.DisconnectPlayer(language.ServerNoGuests()));
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
                    //Check if client is in ServerClient.txt and assign corresponding group.
                    bool exists = false;
                    foreach (ManicDigger.Client client in serverClient.Clients)
                    {
                        if (client.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (ManicDigger.Group clientGroup in serverClient.Groups)
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
                        //Assign admin group if client connected from localhost
                        if (isClientLocalhost)
                        {
                            clients[clientid].AssignGroup(serverClient.Groups.Find(v => v.Name == "Admin"));
                        }
                        else if (clients[clientid].playername.StartsWith("~"))
                        {
                            clients[clientid].AssignGroup(this.defaultGroupGuest);
                        }
                        else
                        {
                            clients[clientid].AssignGroup(this.defaultGroupRegistered);
                        }
                    }
                    this.SetFillAreaLimit(clientid);
                    this.SendFreemoveState(clientid, clients[clientid].privileges.Contains(ServerClientMisc.Privilege.freemove));
                    c.queryClient = false;
                    clients[clientid].entity.drawName.name = username;
                    if (config.EnablePlayerPushing)
                    {
                        // Player pushing
                        clients[clientid].entity.push = new ServerEntityPush();
                        clients[clientid].entity.push.range = 1;
                    }
                    PlayerEntitySetDirty(clientid);
                }
                break;
            case Packet_ClientIdEnum.RequestBlob:
                {
                    // Set player's spawn position
                    Vector3i position = GetPlayerSpawnPositionMul32(clientid);

                    clients[clientid].PositionMul32GlX = position.x;
                    clients[clientid].PositionMul32GlY = position.y + (int)(0.5 * 32);
                    clients[clientid].PositionMul32GlZ = position.z;

                    string ip = (clients[clientid].socket.RemoteEndPoint()).AddressToString();
                    SendMessageToAll(string.Format(language.ServerPlayerJoin(), clients[clientid].ColoredPlayername(colorNormal)));
                    ServerEventLog(string.Format("{0} {1} joins.", clients[clientid].playername, ip));
                    SendMessage(clientid, colorSuccess + config.WelcomeMessage);
                    SendBlobs(clientid, packet.RequestBlob.RequestedMd5);
                    SendBlockTypes(clientid);
                    SendTranslations(clientid);
                    SendSunLevels(clientid);
                    SendLightLevels(clientid);
                    SendCraftingRecipes(clientid);

                    for (int i = 0; i < modEventHandlers.onplayerjoin.Count; i++)
                    {
                        try
                        {
                            modEventHandlers.onplayerjoin[i](clientid);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Mod exception: OnPlayerJoin");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }

                    SendPacket(clientid, ServerPackets.LevelFinalize());
                    clients[clientid].state = ClientStateOnServer.Playing;
                    NotifySeason(clientid);
                }
                break;
            case Packet_ClientIdEnum.SetBlock:
                {
                    int x = packet.SetBlock.X;
                    int y = packet.SetBlock.Y;
                    int z = packet.SetBlock.Z;
                    if (packet.SetBlock.Mode == Packet_BlockSetModeEnum.Use)	//Check if player only uses block
                    {
                        if (!CheckUsePrivileges(clientid, x, y, z))
                        {
                            break;
                        }
                        DoCommandBuild(clientid, true, packet.SetBlock);
                    }
                    else	//Player builds, deletes or uses block with tool
                    {
                        if (!CheckBuildPrivileges(clientid, x, y, z, packet.SetBlock.Mode))
                        {
                            SendSetBlock(clientid, x, y, z, d_Map.GetBlock(x, y, z)); //revert
                            break;
                        }
                        if (!DoCommandBuild(clientid, true, packet.SetBlock))
                        {
                            SendSetBlock(clientid, x, y, z, d_Map.GetBlock(x, y, z)); //revert
                        }
                        //Only log when building/destroying blocks. Prevents VandalFinder entries
                        if (packet.SetBlock.Mode != Packet_BlockSetModeEnum.UseWithTool)
                            BuildLog(string.Format("{0} {1} {2} {3} {4} {5}", x, y, z, c.playername, (c.socket.RemoteEndPoint()).AddressToString(), d_Map.GetBlock(x, y, z)));
                    }
                }
                break;
            case Packet_ClientIdEnum.FillArea:
                {
                    if (!clients[clientid].privileges.Contains(ServerClientMisc.Privilege.build))
                    {
                        SendMessage(clientid, colorError + language.ServerNoBuildPrivilege());
                        break;
                    }
                    if (clients[clientid].IsSpectator && !config.AllowSpectatorBuild)
                    {
                        SendMessage(clientid, colorError + language.ServerNoSpectatorBuild());
                        break;
                    }
                    Vector3i a = new Vector3i(packet.FillArea.X1, packet.FillArea.Y1, packet.FillArea.Z1);
                    Vector3i b = new Vector3i(packet.FillArea.X2, packet.FillArea.Y2, packet.FillArea.Z2);

                    int blockCount = (Math.Abs(a.x - b.x) + 1) * (Math.Abs(a.y - b.y) + 1) * (Math.Abs(a.z - b.z) + 1);

                    if (blockCount > clients[clientid].FillLimit)
                    {
                        SendMessage(clientid, colorError + language.ServerFillAreaTooLarge());
                        break;
                    }
                    if (!this.IsFillAreaValid(clients[clientid], a, b))
                    {
                        SendMessage(clientid, colorError + language.ServerFillAreaInvalid());
                        break;
                    }
                    this.DoFillArea(clientid, packet.FillArea, blockCount);

                    BuildLog(string.Format("{0} {1} {2} - {3} {4} {5} {6} {7} {8}", a.x, a.y, a.z, b.x, b.y, b.z,
                        c.playername, (c.socket.RemoteEndPoint()).AddressToString(),
                        d_Map.GetBlock(a.x, a.y, a.z)));
                }
                break;
            case Packet_ClientIdEnum.PositionandOrientation:
                {
                    var p = packet.PositionAndOrientation;
                    clients[clientid].PositionMul32GlX = p.X;
                    clients[clientid].PositionMul32GlY = p.Y;
                    clients[clientid].PositionMul32GlZ = p.Z;
                    clients[clientid].positionheading = p.Heading;
                    clients[clientid].positionpitch = p.Pitch;
                    clients[clientid].stance = (byte)p.Stance;
                }
                break;
            case Packet_ClientIdEnum.Message:
                {
                    packet.Message.Message = packet.Message.Message.Trim();
                    // empty message
                    if (string.IsNullOrEmpty(packet.Message.Message))
                    {
                        //Ignore empty messages
                        break;
                    }
                    // server command
                    if (packet.Message.Message.StartsWith("/"))
                    {
                        string[] ss = packet.Message.Message.Split(new[] { ' ' });
                        string command = ss[0].Replace("/", "");
                        string argument = packet.Message.Message.IndexOf(" ") < 0 ? "" : packet.Message.Message.Substring(packet.Message.Message.IndexOf(" ") + 1);
                        try
                        {
                            //Try to execute the given command
                            this.CommandInterpreter(clientid, command, argument);
                        }
                        catch (Exception ex)
                        {
                            //This will notify client of error instead of kicking him in case of an error
                            SendMessage(clientid, "Server error while executing command!", MessageType.Error);
                            SendMessage(clientid, "Details on server console!", MessageType.Error);
                            Console.WriteLine("Client {0} caused a command error.", clientid);
                            Console.WriteLine("Command: /{0}", command);
                            Console.WriteLine("Arguments: {0}", argument);
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    // client command
                    else if (packet.Message.Message.StartsWith("."))
                    {
                        //Ignore clientside commands
                        break;
                    }
                    // chat message
                    else
                    {
                        string message = packet.Message.Message;
                        for (int i = 0; i < modEventHandlers.onplayerchat.Count; i++)
                        {
                            try
                            {
                                message = modEventHandlers.onplayerchat[i](clientid, message, packet.Message.IsTeamchat != 0);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Mod exception: OnPlayerChat");
                                Console.WriteLine(ex.Message);
                                Console.WriteLine(ex.StackTrace);
                            }
                        }
                        if (clients[clientid].privileges.Contains(ServerClientMisc.Privilege.chat))
                        {
                            if (message == null)
                            {
                                break;
                            }
                            SendMessageToAll(string.Format("{0}: {1}", clients[clientid].ColoredPlayername(colorNormal), message));
                            ChatLog(string.Format("{0}: {1}", clients[clientid].playername, message));
                        }
                        else
                        {
                            SendMessage(clientid, string.Format(language.ServerNoChatPrivilege(), colorError));
                        }
                    }
                }
                break;
            case Packet_ClientIdEnum.Craft:
                DoCommandCraft(true, packet.Craft);
                break;
            case Packet_ClientIdEnum.InventoryAction:
                DoCommandInventory(clientid, packet.InventoryAction);
                break;
            case Packet_ClientIdEnum.Health:
                {
                    //todo server side
                    var stats = GetPlayerStats(clients[clientid].playername);
                    stats.CurrentHealth = packet.Health.CurrentHealth;
                    if (stats.CurrentHealth < 1)
                    {
                        //death - reset health. More stuff done in Death packet handling
                        stats.CurrentHealth = stats.MaxHealth;
                    }
                    clients[clientid].IsPlayerStatsDirty = true;
                }
                break;
            case Packet_ClientIdEnum.Death:
                {
                    //Console.WriteLine("Death Packet Received. Client: {0}, Reason: {1}, Source: {2}", clientid, packet.Death.Reason, packet.Death.SourcePlayer);
                    for (int i = 0; i < modEventHandlers.onplayerdeath.Count; i++)
                    {
                        try
                        {
                            modEventHandlers.onplayerdeath[i](clientid, (DeathReason)packet.Death.Reason, packet.Death.SourcePlayer);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Mod exception: OnPlayerDeath");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                }
                break;
            case Packet_ClientIdEnum.Oxygen:
                {
                    //todo server side
                    var stats = GetPlayerStats(clients[clientid].playername);
                    stats.CurrentOxygen = packet.Oxygen.CurrentOxygen;
                    clients[clientid].IsPlayerStatsDirty = true;
                }
                break;
            case Packet_ClientIdEnum.MonsterHit:
                HitMonsters(clientid, packet.Health.CurrentHealth);
                break;
            case Packet_ClientIdEnum.DialogClick:
                for (int i = 0; i < modEventHandlers.ondialogclick.Count; i++)
                {
                    try
                    {
                        modEventHandlers.ondialogclick[i](clientid, packet.DialogClick_.WidgetId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Mod exception: OnDialogClick");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
                for (int i = 0; i < modEventHandlers.ondialogclick2.Count; i++)
                {
                    try
                    {
                        DialogClickArgs args = new DialogClickArgs();
                        args.SetPlayer(clientid);
                        args.SetWidgetId(packet.DialogClick_.WidgetId);
                        args.SetTextBoxValue(packet.DialogClick_.TextBoxValue);
                        modEventHandlers.ondialogclick2[i](args);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Mod exception: OnDialogClick2");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
                break;
            case Packet_ClientIdEnum.Shot:
                int shootSoundIndex = pistolcycle++ % BlockTypes[packet.Shot.WeaponBlock].Sounds.ShootEnd.Length;	//Cycle all given ShootEnd sounds
                PlaySoundAtExceptPlayer((int)DeserializeFloat(packet.Shot.FromX), (int)DeserializeFloat(packet.Shot.FromZ), (int)DeserializeFloat(packet.Shot.FromY), BlockTypes[packet.Shot.WeaponBlock].Sounds.ShootEnd[shootSoundIndex] + ".ogg", clientid);
                if (BlockTypes[packet.Shot.WeaponBlock].ProjectileSpeed == 0)
                {
                    SendBullet(clientid, DeserializeFloat(packet.Shot.FromX), DeserializeFloat(packet.Shot.FromY), DeserializeFloat(packet.Shot.FromZ),
                       DeserializeFloat(packet.Shot.ToX), DeserializeFloat(packet.Shot.ToY), DeserializeFloat(packet.Shot.ToZ), 150);
                }
                else
                {
                    Vector3 from = new Vector3(DeserializeFloat(packet.Shot.FromX), DeserializeFloat(packet.Shot.FromY), DeserializeFloat(packet.Shot.FromZ));
                    Vector3 to = new Vector3(DeserializeFloat(packet.Shot.ToX), DeserializeFloat(packet.Shot.ToY), DeserializeFloat(packet.Shot.ToZ));
                    Vector3 v = to - from;
                    v.Normalize();
                    v *= BlockTypes[packet.Shot.WeaponBlock].ProjectileSpeed;
                    SendProjectile(clientid, DeserializeFloat(packet.Shot.FromX), DeserializeFloat(packet.Shot.FromY), DeserializeFloat(packet.Shot.FromZ),
                        v.X, v.Y, v.Z, packet.Shot.WeaponBlock, DeserializeFloat(packet.Shot.ExplodesAfter));
                    //Handle OnWeaponShot so grenade ammo is correct
                    for (int i = 0; i < modEventHandlers.onweaponshot.Count; i++)
                    {
                        try
                        {
                            modEventHandlers.onweaponshot[i](clientid, packet.Shot.WeaponBlock);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Mod exception: OnWeaponShot");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    return;
                }
                for (int i = 0; i < modEventHandlers.onweaponshot.Count; i++)
                {
                    try
                    {
                        modEventHandlers.onweaponshot[i](clientid, packet.Shot.WeaponBlock);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Mod exception: OnWeaponShot");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
                if (clients[clientid].LastPing < 0.3)
                {
                    if (packet.Shot.HitPlayer != -1)
                    {
                        //client-side shooting
                        for (int i = 0; i < modEventHandlers.onweaponhit.Count; i++)
                        {
                            try
                            {
                                modEventHandlers.onweaponhit[i](clientid, packet.Shot.HitPlayer, packet.Shot.WeaponBlock, packet.Shot.IsHitHead != 0);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Mod exception: OnWeaponHit");
                                Console.WriteLine(ex.Message);
                                Console.WriteLine(ex.StackTrace);
                            }
                        }
                    }
                    return;
                }
                foreach (var k in clients)
                {
                    if (k.Key == clientid)
                    {
                        continue;
                    }
                    Line3D pick = new Line3D();
                    pick.Start = new float[] { DeserializeFloat(packet.Shot.FromX), DeserializeFloat(packet.Shot.FromY), DeserializeFloat(packet.Shot.FromZ) };
                    pick.End = new float[] { DeserializeFloat(packet.Shot.ToX), DeserializeFloat(packet.Shot.ToY), DeserializeFloat(packet.Shot.ToZ) };

                    Vector3 feetpos = new Vector3((float)k.Value.PositionMul32GlX / 32, (float)k.Value.PositionMul32GlY / 32, (float)k.Value.PositionMul32GlZ / 32);
                    //var p = PlayerPositionSpawn;
                    Box3D bodybox = new Box3D();
                    float headsize = (k.Value.ModelHeight - k.Value.EyeHeight) * 2; //0.4f;
                    float h = k.Value.ModelHeight - headsize;
                    float r = 0.35f;

                    bodybox.AddPoint(feetpos.X - r, feetpos.Y + 0, feetpos.Z - r);
                    bodybox.AddPoint(feetpos.X - r, feetpos.Y + 0, feetpos.Z + r);
                    bodybox.AddPoint(feetpos.X + r, feetpos.Y + 0, feetpos.Z - r);
                    bodybox.AddPoint(feetpos.X + r, feetpos.Y + 0, feetpos.Z + r);

                    bodybox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z - r);
                    bodybox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z + r);
                    bodybox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z - r);
                    bodybox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z + r);

                    Box3D headbox = new Box3D();

                    headbox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z - r);
                    headbox.AddPoint(feetpos.X - r, feetpos.Y + h, feetpos.Z + r);
                    headbox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z - r);
                    headbox.AddPoint(feetpos.X + r, feetpos.Y + h, feetpos.Z + r);

                    headbox.AddPoint(feetpos.X - r, feetpos.Y + h + headsize, feetpos.Z - r);
                    headbox.AddPoint(feetpos.X - r, feetpos.Y + h + headsize, feetpos.Z + r);
                    headbox.AddPoint(feetpos.X + r, feetpos.Y + h + headsize, feetpos.Z - r);
                    headbox.AddPoint(feetpos.X + r, feetpos.Y + h + headsize, feetpos.Z + r);

                    if (Intersection.CheckLineBoxExact(pick, headbox) != null)
                    {
                        for (int i = 0; i < modEventHandlers.onweaponhit.Count; i++)
                        {
                            try
                            {
                                modEventHandlers.onweaponhit[i](clientid, k.Key, packet.Shot.WeaponBlock, true);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Mod exception: OnWeaponHit");
                                Console.WriteLine(ex.Message);
                                Console.WriteLine(ex.StackTrace);
                            }
                        }
                    }
                    else if (Intersection.CheckLineBoxExact(pick, bodybox) != null)
                    {
                        for (int i = 0; i < modEventHandlers.onweaponhit.Count; i++)
                        {
                            try
                            {
                                modEventHandlers.onweaponhit[i](clientid, k.Key, packet.Shot.WeaponBlock, false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Mod exception: OnWeaponHit");
                                Console.WriteLine(ex.Message);
                                Console.WriteLine(ex.StackTrace);
                            }
                        }
                    }
                }
                break;
            case Packet_ClientIdEnum.SpecialKey:
                for (int i = 0; i < modEventHandlers.onspecialkey.Count; i++)
                {
                    try
                    {
                        modEventHandlers.onspecialkey[i](clientid, (SpecialKey)packet.SpecialKey_.Key_);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Mod exception: OnSpecialKey");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
                break;
            case Packet_ClientIdEnum.ActiveMaterialSlot:
                clients[clientid].ActiveMaterialSlot = packet.ActiveMaterialSlot.ActiveMaterialSlot;
                for (int i = 0; i < modEventHandlers.changedactivematerialslot.Count; i++)
                {
                    try
                    {
                        modEventHandlers.changedactivematerialslot[i](clientid);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Mod exception: ChangedActiveMaterialSlot");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
                break;
            case Packet_ClientIdEnum.Leave:
                //0: Leave - 1: Crash
                Console.WriteLine("Disconnect reason: {0}", packet.Leave.Reason);
                KillPlayer(clientid);
                break;
            case Packet_ClientIdEnum.Reload:
                break;
            case Packet_ClientIdEnum.ServerQuery:
                //Flood/DDoS-abuse protection
                if ((DateTime.UtcNow - lastQuery) < TimeSpan.FromMilliseconds(200))
                {
                    Console.WriteLine("ServerQuery rejected (too many requests)");
                    SendPacket(clientid, ServerPackets.DisconnectPlayer("Too many requests!"));
                    KillPlayer(clientid);
                    return;
                }
                Console.WriteLine("ServerQuery processed.");
                lastQuery = DateTime.UtcNow;
                //Client only wants server information. No real client.
                List<string> playernames = new List<string>();
                lock (clients)
                {
                    foreach (var k in clients)
                    {
                        if (k.Value.queryClient || k.Value.IsBot)
                        {
                            //Exclude bot players and query clients
                            continue;
                        }
                        playernames.Add(k.Value.playername);
                    }
                }
                //Create query answer
                Packet_ServerQueryAnswer answer = new Packet_ServerQueryAnswer()
                {
                    Name = config.Name,
                    MOTD = config.Motd,
                    PlayerCount = playernames.Count,
                    MaxPlayers = config.MaxClients,
                    PlayerList = string.Join(",", playernames.ToArray()),
                    Port = config.Port,
                    GameMode = gameMode,
                    Password = config.IsPasswordProtected(),
                    PublicHash = ReceivedKey,
                    ServerVersion = GameVersion.Version,
                    MapSizeX = d_Map.MapSizeX,
                    MapSizeY = d_Map.MapSizeY,
                    MapSizeZ = d_Map.MapSizeZ,
                    ServerThumbnail = GenerateServerThumbnail(),
                };
                //Send answer
                SendPacket(clientid, ServerPackets.AnswerQuery(answer));
                //Directly disconnect client after request.
                SendPacket(clientid, ServerPackets.DisconnectPlayer("Query success."));
                KillPlayer(clientid);
                break;
            case Packet_ClientIdEnum.GameResolution:
                //Update client information
                clients[clientid].WindowSize = new int[] { packet.GameResolution.Width, packet.GameResolution.Height };
                //Console.WriteLine("client:{0} --> {1}x{2}", clientid, clients[clientid].WindowSize[0], clients[clientid].WindowSize[1]);
                break;
            case Packet_ClientIdEnum.EntityInteraction:
                switch (packet.EntityInteraction.InteractionType)
                {
                    case Packet_EntityInteractionTypeEnum.Use:
                        for (int i = 0; i < modEventHandlers.onuseentity.Count; i++)
                        {
                            ServerEntityId id = c.spawnedEntities[packet.EntityInteraction.EntityId - 64];
                            modEventHandlers.onuseentity[i](clientid, id.chunkx, id.chunky, id.chunkz, id.id);
                        }
                        break;
                    case Packet_EntityInteractionTypeEnum.Hit:
                        for (int i = 0; i < modEventHandlers.onhitentity.Count; i++)
                        {
                            ServerEntityId id = c.spawnedEntities[packet.EntityInteraction.EntityId - 64];
                            modEventHandlers.onhitentity[i](clientid, id.chunkx, id.chunky, id.chunkz, id.id);
                        }
                        break;
                    default:
                        Console.WriteLine("Unknown EntityInteractionType: {0}, clientid: {1}", packet.EntityInteraction.InteractionType, clientid);
                        break;
                }
                break;
            default:
                Console.WriteLine("Invalid packet: {0}, clientid:{1}", packet.Id, clientid);
                break;
        }
    }

    public bool CheckBuildPrivileges(int player, int x, int y, int z, int mode)
    {
        Server server = this;
        if (!server.PlayerHasPrivilege(player, ServerClientMisc.Privilege.build))
        {
            server.SendMessage(player, server.colorError + server.language.ServerNoBuildPrivilege());
            return false;
        }
        if (server.clients[player].IsSpectator && !server.config.AllowSpectatorBuild)
        {
            server.SendMessage(player, server.colorError + server.language.ServerNoSpectatorBuild());
            return false;
        }
        for (int i = 0; i < server.modEventHandlers.onpermission.Count; i++)
        {
            PermissionArgs args = new PermissionArgs();
            args.SetPlayer(player);
            args.SetX(x);
            args.SetY(y);
            args.SetZ(z);
            server.modEventHandlers.onpermission[i](args);
            if (args.GetAllowed())
            {
                return true;
            }
        }
        if (!server.config.CanUserBuild(server.clients[player], x, y, z)
            && !server.extraPrivileges.ContainsKey(ServerClientMisc.Privilege.build))
        {
            server.SendMessage(player, server.colorError + server.language.ServerNoBuildPermissionHere());
            return false;
        }
        bool retval = true;
        if (mode == Packet_BlockSetModeEnum.Create)
        {
            for (int i = 0; i < modEventHandlers.checkonbuild.Count; i++)
            {
                // All handlers must return true for operation to be permitted.
                try
                {
                    retval = (retval && modEventHandlers.checkonbuild[i](player, x, y, z));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Mod exception: CheckOnBuild");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    // Do not allow interactions when check fails.
                    retval = false;
                }
            }
        }
        else if (mode == Packet_BlockSetModeEnum.Destroy)
        {
            for (int i = 0; i < modEventHandlers.checkondelete.Count; i++)
            {
                // All handlers must return true for operation to be permitted.
                try
                {
                    retval = (retval && modEventHandlers.checkondelete[i](player, x, y, z));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Mod exception: CheckOnDelete");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    // Do not allow interactions when check fails.
                    retval = false;
                }
            }
        }
        return retval;
    }

    public bool CheckUsePrivileges(int player, int x, int y, int z)
    {
        Server server = this;
        if (!server.PlayerHasPrivilege(player, ServerClientMisc.Privilege.use))
        {
            SendMessage(player, colorError + server.language.ServerNoUsePrivilege());
            return false;
        }
        if (server.clients[player].IsSpectator && !server.config.AllowSpectatorUse)
        {
            SendMessage(player, colorError + server.language.ServerNoSpectatorUse());
            return false;
        }
        bool retval = true;
        for (int i = 0; i < modEventHandlers.checkonuse.Count; i++)
        {
            // All handlers must return true for operation to be permitted.
            try
            {
                retval = (retval && modEventHandlers.checkonuse[i](player, x, y, z));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mod exception: CheckOnUse");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                // Do not allow interactions when check fails.
                retval = false;
            }
        }
        return retval;
    }

    public void SendServerRedirect(int clientid, string ip_, int port_)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.ServerRedirect;
        p.Redirect = new Packet_ServerRedirect()
        {
            IP = ip_,
            Port = port_,
        };
        SendPacket(clientid, p);
    }

    public static byte[] GenerateServerThumbnail()
    {
        string filename = Path.Combine(Path.Combine("data", "public"), "thumbnail.png");
        Bitmap bmp;
        if (File.Exists(filename))
        {
            try
            {
                bmp = new Bitmap(filename);
            }
            catch
            {
                //Create empty bitmap in case of failure
                bmp = new Bitmap(64, 64);
            }
        }
        else
        {
            bmp = new Bitmap(64, 64);
        }
        Bitmap bmp2 = bmp;
        if (bmp.Width != 64 || bmp.Height != 64)
        {
            //Resize the image if it does not have the proper size
            bmp2 = new Bitmap(bmp, 64, 64);
        }
        using (MemoryStream ms = new MemoryStream())
        {
            //Convert image to a byte[] for transfer
            bmp2.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }

    private float DeserializeFloat(int p)
    {
        return (float)p / 32;
    }

    private void SendProjectile(int player, float fromx, float fromy, float fromz, float velocityx, float velocityy, float velocityz, int block, float explodesafter)
    {
        foreach (var k in clients)
        {
            if (k.Key == player)
            {
                continue;
            }
            Packet_Server p = new Packet_Server();
            p.Id = Packet_ServerIdEnum.Projectile;
            p.Projectile = new Packet_ServerProjectile()
            {
                FromXFloat = SerializeFloat(fromx),
                FromYFloat = SerializeFloat(fromy),
                FromZFloat = SerializeFloat(fromz),
                VelocityXFloat = SerializeFloat(velocityx),
                VelocityYFloat = SerializeFloat(velocityy),
                VelocityZFloat = SerializeFloat(velocityz),
                BlockId = block,
                ExplodesAfterFloat = SerializeFloat(explodesafter),
                SourcePlayerID = player,
            };
            SendPacket(k.Key, Serialize(p));
        }
    }

    private void SendBullet(int player, float fromx, float fromy, float fromz, float tox, float toy, float toz, float speed)
    {
        foreach (var k in clients)
        {
            if (k.Key == player)
            {
                continue;
            }
            Packet_Server p = new Packet_Server();
            p.Id = Packet_ServerIdEnum.Bullet;
            p.Bullet = new Packet_ServerBullet()
            {
                FromXFloat = SerializeFloat(fromx),
                FromYFloat = SerializeFloat(fromy),
                FromZFloat = SerializeFloat(fromz),
                ToXFloat = SerializeFloat(tox),
                ToYFloat = SerializeFloat(toy),
                ToZFloat = SerializeFloat(toz),
                SpeedFloat = SerializeFloat(speed)
            };
            SendPacket(k.Key, Serialize(p));
        }
    }
    int pistolcycle;
    public Vector3i GetPlayerSpawnPositionMul32(int clientid)
    {
        Vector3i position;
        ManicDigger.Spawn playerSpawn = null;
        // Check if there is a spawn entry for his assign group
        if (clients[clientid].clientGroup.Spawn != null)
        {
            playerSpawn = clients[clientid].clientGroup.Spawn;
        }
        // Check if there is an entry in clients with spawn member (overrides group spawn).
        foreach (ManicDigger.Client client in serverClient.Clients)
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
        return position;
    }

    private void RunInClientSandbox(string script, int clientid)
    {
        var client = GetClient(clientid);
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
        ServerEventLog(string.Format("{0} runs script:\n{1}", client.playername, script));
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

    public string colorNormal = "&f"; //white
    public string colorHelp = "&4"; //red
    public string colorOpUsername = "&2"; //green
    public string colorSuccess = "&2"; //green
    public string colorError = "&4"; //red
    public string colorImportant = "&4"; // red
    public string colorAdmin = "&e"; //yellow
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
    private bool DoCommandCraft(bool execute, Packet_ClientCraft cmd)
    {
        if (d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z) != d_Data.BlockIdCraftingTable())
        {
            return false;
        }
        if (cmd.RecipeId < 0 || cmd.RecipeId >= craftingrecipes.Count)
        {
            return false;
        }
        IntRef tableCount = new IntRef();
        Vector3IntRef[] table = d_CraftingTableTool.GetTable(cmd.X, cmd.Y, cmd.Z, tableCount);
        IntRef ontableCount = new IntRef();
        int[] ontable = d_CraftingTableTool.GetOnTable(table, tableCount.value, ontableCount);
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
                    if (ontableFindAllCount(ontable, ontableCount, ingredient.Type) < ingredient.Amount)
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
                        ReplaceOne(ontable, ontableCount.value, ingredient.Type, d_Data.BlockIdEmpty());
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
            ReplaceOne(ontable, ontableCount.value, d_Data.BlockIdEmpty(), v);
        }
        int zz = 0;
        if (execute)
        {
            for (int k = 0; k < tableCount.value; k++)
            {
                Vector3IntRef v = table[k];
                SetBlockAndNotify(v.X, v.Y, v.Z + 1, ontable[zz]);
                zz++;
            }
        }
        return true;
    }

    private int ontableFindAllCount(int[] ontable, IntRef ontableCount, int p)
    {
        int count = 0;
        for (int i = 0; i < ontableCount.value; i++)
        {
            if (ontable[i] == p)
            {
                count++;
            }
        }
        return count;
    }

    private void ReplaceOne<T>(T[] l, int lCount, T from, T to)
    {
        for (int ii = 0; ii < lCount; ii++)
        {
            if (l[ii].Equals(from))
            {
                l[ii] = to;
                break;
            }
        }
    }
    public IGameDataItems d_DataItems;
    public InventoryUtil GetInventoryUtil(Inventory inventory)
    {
        InventoryUtil util = new InventoryUtil();
        util.d_Inventory = inventory;
        util.d_Items = d_DataItems;
        return util;
    }
    private void DoCommandInventory(int player_id, Packet_ClientInventoryAction cmd)
    {
        Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
        var s = new InventoryServer();
        s.d_Inventory = inventory;
        s.d_InventoryUtil = GetInventoryUtil(inventory);
        s.d_Items = d_DataItems;
        s.d_DropItem = this;

        switch (cmd.Action)
        {
            case Packet_InventoryActionTypeEnum.Click:
                s.InventoryClick(cmd.A);
                break;
            case Packet_InventoryActionTypeEnum.MoveToInventory:
                s.MoveToInventory(cmd.A);
                break;
            case Packet_InventoryActionTypeEnum.WearItem:
                s.WearItem(cmd.A, cmd.B);
                break;
            default:
                break;
        }
        clients[player_id].IsInventoryDirty = true;
        NotifyInventory(player_id);
    }

    private bool IsFillAreaValid(ClientOnServer client, Vector3i a, Vector3i b)
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
    private bool DoFillArea(int player_id, Packet_ClientFillArea fill, int blockCount)
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
        blockType = d_Data.WhenPlayerPlacesGetsConvertedTo()[blockType];

        Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
        var item = inventory.RightHand[fill.MaterialSlot];
        if (item == null)
        {
            return false;
        }
        //This prevents the player's inventory from getting sent to them while using fill (causes excessive bandwith usage)
        clients[player_id].usingFill = true;
        for (int x = startx; x <= endx; ++x)
        {
            for (int y = starty; y <= endy; ++y)
            {
                for (int z = startz; z <= endz; ++z)
                {
                    Packet_ClientSetBlock cmd = new Packet_ClientSetBlock();
                    cmd.X = x;
                    cmd.Y = y;
                    cmd.Z = z;
                    cmd.MaterialSlot = fill.MaterialSlot;
                    if (GetBlock(x, y, z) != 0)
                    {
                        cmd.Mode = Packet_BlockSetModeEnum.Destroy;
                        DoCommandBuild(player_id, true, cmd);
                    }
                    if (blockType != d_Data.BlockIdFillArea())
                    {
                        cmd.Mode = Packet_BlockSetModeEnum.Create;
                        DoCommandBuild(player_id, true, cmd);
                    }
                }
            }
        }
        clients[player_id].usingFill = false;
        return true;
    }
    /// <summary>
    /// Determines if a given client can see the specified chunk<br/>
    /// <b>Attention!</b> Chunk coordinates are NOT world coordinates!<br/>
    /// chunk position = (world position / chunk size)
    /// </summary>
    /// <param name="clientid">Client ID</param>
    /// <param name="vx">Chunk x coordinate</param>
    /// <param name="vy">Chunk y coordinate</param>
    /// <param name="vz">Chunk z coordinate</param>
    /// <returns>true if client can see the chunk, false otherwise</returns>
    public bool ClientSeenChunk(int clientid, int vx, int vy, int vz)
    {
        int pos = MapUtilCi.Index3d(vx, vy, vz, d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize);
        return clients[clientid].chunksseen[pos];
    }
    /// <summary>
    /// Sets a given chunk as seen by the client<br/>
    /// <b>Attention!</b> Chunk coordinates are NOT world coordinates!<br/>
    /// chunk position = (world position / chunk size)
    /// </summary>
    /// <param name="clientid">Client ID</param>
    /// <param name="vx">Chunk x coordinate</param>
    /// <param name="vy">Chunk y coordinate</param>
    /// <param name="vz">Chunk z coordinate</param>
    /// <param name="time"></param>
    public void ClientSeenChunkSet(int clientid, int vx, int vy, int vz, int time)
    {
        int pos = MapUtilCi.Index3d(vx, vy, vz, d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize);
        clients[clientid].chunksseen[pos] = true;
        clients[clientid].chunksseenTime[pos] = time;
        //Console.WriteLine("SeenChunk:   {0},{1},{2} Client: {3}", vx, vy, vz, clientid);
    }
    /// <summary>
    /// Sets a given chunk as unseen by the client<br/>
    /// <b>Attention!</b> Chunk coordinates are NOT world coordinates!<br/>
    /// chunk position = (world position / chunk size)
    /// </summary>
    /// <param name="clientid">Client ID</param>
    /// <param name="vx">Chunk x coordinate</param>
    /// <param name="vy">Chunk y coordinate</param>
    /// <param name="vz">Chunk z coordinate</param>
    public void ClientSeenChunkRemove(int clientid, int vx, int vy, int vz)
    {
        int pos = MapUtilCi.Index3d(vx, vy, vz, d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize);
        clients[clientid].chunksseen[pos] = false;
        clients[clientid].chunksseenTime[pos] = 0;
        //Console.WriteLine("UnseenChunk: {0},{1},{2} Client: {3}", vx, vy, vz, clientid);
    }
    private void SendFillArea(int clientid, Vector3i a, Vector3i b, int blockType, int blockCount)
    {
        // TODO: better to send a chunk?

        Vector3i v = new Vector3i((int)(a.x / chunksize), (int)(a.y / chunksize), (int)(a.z / chunksize));
        Vector3i w = new Vector3i((int)(b.x / chunksize), (int)(b.y / chunksize), (int)(b.z / chunksize));

        // TODO: Is it sufficient to regard only start- and endpoint?
        if (!ClientSeenChunk(clientid, v.x, v.y, v.z) && !ClientSeenChunk(clientid, w.x, w.y, w.z))
        {
            return;
        }

        Packet_ServerFillArea p = new Packet_ServerFillArea()
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
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.FillArea, FillArea = p }));
    }
    private void SetFillAreaLimit(int clientid)
    {
        ClientOnServer client = GetClient(clientid);
        if (client == null)
        {
            return;
        }

        int maxFill = 500;
        if (serverClient.DefaultFillLimit != null)
        {
            maxFill = serverClient.DefaultFillLimit.Value;
        }

        // Check if there is a fill-limit entry for his assigned group.
        if (client.clientGroup.FillLimit != null)
        {
            maxFill = client.clientGroup.FillLimit.Value;
        }

        // Check if there is an entry in clients with fill-limit member (overrides group fill-limit).
        foreach (ManicDigger.Client clientConfig in serverClient.Clients)
        {
            if (clientConfig.Name.Equals(client.playername, StringComparison.InvariantCultureIgnoreCase))
            {
                if (clientConfig.FillLimit != null)
                {
                    maxFill = clientConfig.FillLimit.Value;
                }
                break;
            }
        }
        client.FillLimit = maxFill;
        SendFillAreaLimit(clientid, maxFill);
    }



    private void SendFillAreaLimit(int clientid, int limit)
    {
        Packet_ServerFillAreaLimit p = new Packet_ServerFillAreaLimit()
        {
            Limit = limit
        };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.FillAreaLimit, FillAreaLimit = p }));
    }

    private bool DoCommandBuild(int player_id, bool execute, Packet_ClientSetBlock cmd)
    {
        Vector3 v = new Vector3(cmd.X, cmd.Y, cmd.Z);
        Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
        if (cmd.Mode == Packet_BlockSetModeEnum.Use)
        {
            for (int i = 0; i < modEventHandlers.onuse.Count; i++)
            {
                try
                {
                    modEventHandlers.onuse[i](player_id, cmd.X, cmd.Y, cmd.Z);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Mod exception: OnUse");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
            return true;
        }
        if (cmd.Mode == Packet_BlockSetModeEnum.UseWithTool)
        {
            for (int i = 0; i < modEventHandlers.onusewithtool.Count; i++)
            {
                try
                {
                    modEventHandlers.onusewithtool[i](player_id, cmd.X, cmd.Y, cmd.Z, cmd.BlockType);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Mod exception: OnUseWithTool");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
            return true;
        }
        if (cmd.Mode == Packet_BlockSetModeEnum.Create
            && d_Data.Rail()[cmd.BlockType] != 0)
        {
            return DoCommandBuildRail(player_id, execute, cmd);
        }
        if (cmd.Mode == (int)Packet_BlockSetModeEnum.Destroy
            && d_Data.Rail()[d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z)] != 0)
        {
            return DoCommandRemoveRail(player_id, execute, cmd);
        }
        if (cmd.Mode == Packet_BlockSetModeEnum.Create)
        {
            int oldblock = d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z);
            if (!(oldblock == 0 || BlockTypes[oldblock].IsFluid()))
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
                    if (d_Data.Rail()[item.BlockId] != 0)
                    {
                    }
                    SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, item.BlockId);
                    for (int i = 0; i < modEventHandlers.onbuild.Count; i++)
                    {
                        try
                        {
                            modEventHandlers.onbuild[i](player_id, cmd.X, cmd.Y, cmd.Z);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Mod exception: OnBuild");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
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
            item.BlockId = d_Data.WhenPlayerPlacesGetsConvertedTo()[blockid];
            if (!config.IsCreative)
            {
                GetInventoryUtil(inventory).GrabItem(item, cmd.MaterialSlot);
            }
            SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, SpecialBlockId.Empty);
            for (int i = 0; i < modEventHandlers.ondelete.Count; i++)
            {
                try
                {
                    modEventHandlers.ondelete[i](player_id, cmd.X, cmd.Y, cmd.Z, blockid);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Mod exception: OnDelete");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
        clients[player_id].IsInventoryDirty = true;
        NotifyInventory(player_id);
        return true;
    }

    private bool DoCommandBuildRail(int player_id, bool execute, Packet_ClientSetBlock cmd)
    {
        Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
        int oldblock = d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z);
        //int blockstoput = 1;
        if (!(oldblock == SpecialBlockId.Empty || d_Data.IsRailTile(oldblock)))
        {
            return false;
        }

        //count how many rails will be created
        int oldrailcount = 0;
        if (d_Data.IsRailTile(oldblock))
        {
            oldrailcount = DirectionUtils.RailDirectionFlagsCount(
                (oldblock - d_Data.BlockIdRailstart()));
        }
        int newrailcount = DirectionUtils.RailDirectionFlagsCount(
            (cmd.BlockType - d_Data.BlockIdRailstart()));
        int blockstoput = newrailcount - oldrailcount;

        Item item = inventory.RightHand[cmd.MaterialSlot];
        if (!(item.ItemClass == ItemClass.Block && d_Data.Rail()[item.BlockId] != 0))
        {
            return false;
        }
        item.BlockCount -= blockstoput;
        if (item.BlockCount == 0)
        {
            inventory.RightHand[cmd.MaterialSlot] = null;
        }
        SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, cmd.BlockType);
        for (int i = 0; i < modEventHandlers.onbuild.Count; i++)
        {
            try
            {
                modEventHandlers.onbuild[i](player_id, cmd.X, cmd.Y, cmd.Z);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mod exception: OnBuild");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        clients[player_id].IsInventoryDirty = true;
        NotifyInventory(player_id);
        return true;
    }

    private bool DoCommandRemoveRail(int player_id, bool execute, Packet_ClientSetBlock cmd)
    {
        Inventory inventory = GetPlayerInventory(clients[player_id].playername).Inventory;
        //add to inventory
        int blockid = d_Map.GetBlock(cmd.X, cmd.Y, cmd.Z);
        int blocktype = d_Data.WhenPlayerPlacesGetsConvertedTo()[blockid];
        if ((!IsValid(blocktype))
            || blocktype == SpecialBlockId.Empty)
        {
            return false;
        }
        int blockstopick = 1;
        if (d_Data.IsRailTile(blocktype))
        {
            blockstopick = DirectionUtils.RailDirectionFlagsCount(
                (blocktype - d_Data.BlockIdRailstart()));
        }

        var item = new Item();
        item.ItemClass = ItemClass.Block;
        item.BlockId = d_Data.WhenPlayerPlacesGetsConvertedTo()[blocktype];
        item.BlockCount = blockstopick;
        if (!config.IsCreative)
        {
            GetInventoryUtil(inventory).GrabItem(item, cmd.MaterialSlot);
        }
        SetBlockAndNotify(cmd.X, cmd.Y, cmd.Z, SpecialBlockId.Empty);
        for (int i = 0; i < modEventHandlers.ondelete.Count; i++)
        {
            try
            {
                modEventHandlers.ondelete[i](player_id, cmd.X, cmd.Y, cmd.Z, blockid);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mod exception: OnDelete");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        clients[player_id].IsInventoryDirty = true;
        NotifyInventory(player_id);
        return true;
    }

    private bool IsValid(int blocktype)
    {
        return BlockTypes[blocktype].Name != null;
    }

    public void SetBlockAndNotify(int x, int y, int z, int blocktype)
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
        if (d_Data.IsRailTile(blocktypea) && d_Data.IsRailTile(blocktypeb))
        {
            return true;
        }
        return blocktypea == blocktypeb;
    }
    public byte[] Serialize(Packet_Server p)
    {
        byte[] data = Packet_ServerSerializer.SerializeToBytes(p);
        return data;
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
    public void SendMessageToAll(string message)
    {
        Console.WriteLine("Message to all: " + message);
        foreach (var k in clients)
        {
            SendMessage(k.Key, message);
        }
    }
    private void SendSetBlock(int clientid, int x, int y, int z, int blocktype)
    {
    	if (!ClientSeenChunk(clientid, (int)(x / chunksize), (int)(y / chunksize), (int)(z / chunksize)))
        {
    		// don't send block updates for chunks a player can not see
            return;
        }
        Packet_ServerSetBlock p = new Packet_ServerSetBlock() { X = x, Y = y, Z = z, BlockType = blocktype };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.SetBlock, SetBlock = p }));
    }
    public void SendSound(int clientid, string name, int x, int y, int z)
    {
        Packet_ServerSound p = new Packet_ServerSound() { Name = name, X = x, Y = y, Z = z };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.Sound, Sound = p }));
    }
    private void SendPlayerSpawnPosition(int clientid, int x, int y, int z)
    {
        Packet_ServerPlayerSpawnPosition p = new Packet_ServerPlayerSpawnPosition()
        {
            X = x,
            Y = y,
            Z = z
        };
        SendPacket(clientid, Serialize(new Packet_Server()
        {
            Id = Packet_ServerIdEnum.PlayerSpawnPosition,
            PlayerSpawnPosition = p,
        }));
    }
    
    public void SendMessage(int clientid, string message, MessageType color)
    {
        SendMessage(clientid, MessageTypeToString(color) + message);
    }

    public void SendMessage(int clientid, string message)
    {
        if (clientid == this.serverConsoleId)
        {
            this.serverConsole.Receive(message);
            return;
        }

        SendPacket(clientid, ServerPackets.Message(message));
    }

    int StatTotalPackets = 0;
    int StatTotalPacketsLength = 0;
    public long TotalSentBytes;
    public long TotalReceivedBytes;

    public void SendPacket(int clientid, Packet_Server packet)
    {
        SendPacket(clientid, Serialize(packet));
    }

    public void SendPacket(int clientid, byte[] packet)
    {
        if (clients[clientid].IsBot)
        {
            return;
        }
        StatTotalPackets++;
        StatTotalPacketsLength += packet.Length;
        TotalSentBytes += packet.Length;
        try
        {
            INetOutgoingMessage msg = new INetOutgoingMessage();
            msg.Write(packet, packet.Length);
            clients[clientid].socket.SendMessage(msg, MyNetDeliveryMethod.ReliableOrdered, 0);
        }
        catch (Exception)
        {
            Console.WriteLine("Network exception.");
            KillPlayer(clientid);
        }
    }
    void EmptyCallback(IAsyncResult result)
    {
    }
    public int drawdistance = 128;
    public const int chunksize = 32;
    internal int chunkdrawdistance { get { return drawdistance / chunksize; } }
    public byte[] CompressChunkNetwork(ushort[] chunk)
    {
        return d_NetworkCompression.Compress(Misc.UshortArrayToByteArray(chunk));
    }
    public byte[] CompressChunkNetwork(byte[, ,] chunk)
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

    Packet_StringList GetRequiredBlobMd5()
    {
        Packet_StringList p = new Packet_StringList();
        List<string> list = new List<string>();

        for (int i = 0; i < assets.count; i++)
        {
            list.Add(assets.items[i].md5);
        }

        p.SetItems(list.ToArray(), list.Count, list.Count);
        return p;
    }

    Packet_StringList GetRequiredBlobName()
    {
        Packet_StringList p = new Packet_StringList();
        List<string> list = new List<string>();

        for (int i = 0; i < assets.count; i++)
        {
            list.Add(assets.items[i].name);
        }

        p.SetItems(list.ToArray(), list.Count, list.Count);
        return p;
    }

    AssetLoader assetLoader;
    AssetList assets = new AssetList();

    int BlobPartLength = 1024 * 1;
    private void SendBlobs(int clientid, Packet_StringList list)
    {
        SendPacket(clientid, ServerPackets.LevelInitialize());
        LoadAssets();

        List<Asset> tosend = new List<Asset>();
        for (int i = 0; i < assets.count; i++)
        {
            Asset f = assets.items[i];
            for (int k = 0; k < list.ItemsCount; k++)
            {
                if (f.md5 == list.Items[k])
                {
                    tosend.Add(f);
                }
            }
        }

        for (int i = 0; i < tosend.Count; i++)
        {
            Asset f = tosend[i];
            SendBlobInitialize(clientid, f.md5, f.name);
            byte[] blob = f.data;
            int totalsent = 0;
            foreach (byte[] part in Parts(blob, BlobPartLength))
            {
                SendLevelProgress(clientid,
                    (int)(((float)i / tosend.Count
                                         + ((float)totalsent / blob.Length) / tosend.Count) * 100), language.ServerProgressDownloadingData());
                SendBlobPart(clientid, part);
                totalsent += part.Length;
            }
            SendBlobFinalize(clientid);
        }
        SendLevelProgress(clientid, 0, language.ServerProgressGenerating());
    }

    void LoadAssets()
    {
        FloatRef progress = new FloatRef();
        assetLoader.LoadAssetsAsync(assets, progress);
        while (progress.value < 1)
        {
            Thread.Sleep(1);
        }
    }

    public static IEnumerable<byte[]> Parts(byte[] blob, int partsize)
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
    private void SendBlobInitialize(int clientid, string hash, string name)
    {
        Packet_ServerBlobInitialize p = new Packet_ServerBlobInitialize() { Name = name, Md5 = hash };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.BlobInitialize, BlobInitialize = p }));
    }
    private void SendBlobPart(int clientid, byte[] data)
    {
        Packet_ServerBlobPart p = new Packet_ServerBlobPart() { Data = data };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.BlobPart, BlobPart = p }));
    }
    private void SendBlobFinalize(int clientid)
    {
        Packet_ServerBlobFinalize p = new Packet_ServerBlobFinalize() { };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.BlobFinalize, BlobFinalize = p }));
    }
    public BlockType[] BlockTypes = new BlockType[GlobalVar.MAX_BLOCKTYPES];
    public void SendBlockTypes(int clientid)
    {
        for (int i = 0; i < BlockTypes.Length; i++)
        {
            Packet_ServerBlockType p1 = new Packet_ServerBlockType() { Id = i, Blocktype = BlockTypeConverter.GetBlockType(BlockTypes[i]) };
            SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.BlockType, BlockType = p1 }));
        }
        Packet_ServerBlockTypes p = new Packet_ServerBlockTypes() { };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.BlockTypes, BlockTypes = p }));
    }

    public void SendTranslations(int clientid)
    {
        //Read all lines from server translation and send them to the client
        TranslatedString[] strings = language.AllStrings();
        foreach (TranslatedString transString in strings)
        {
            if (transString == null)
            {
                continue;
            }
            Packet_ServerTranslatedString p = new Packet_ServerTranslatedString()
            {
                Lang = transString.language,
                Id = transString.id,
                Translation = transString.translated
            };
            SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.Translation, Translation = p }));
        }
    }

    public static int SerializeFloat(float p)
    {
        return (int)(p * 32);
    }

    private void SendSunLevels(int clientid)
    {
        Packet_ServerSunLevels p = new Packet_ServerSunLevels();
        p.SetSunlevels(sunlevels, sunlevels.Length, sunlevels.Length);
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.SunLevels, SunLevels = p }));
    }
    private void SendLightLevels(int clientid)
    {
        Packet_ServerLightLevels p = new Packet_ServerLightLevels();
        int[] l = new int[lightlevels.Length];
        for (int i = 0; i < lightlevels.Length; i++)
        {
            l[i] = SerializeFloat(lightlevels[i]);
        }
        p.SetLightlevels(l, l.Length, l.Length);
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.LightLevels, LightLevels = p }));
    }
    private void SendCraftingRecipes(int clientid)
    {
        Packet_CraftingRecipe[] recipes = new Packet_CraftingRecipe[craftingrecipes.Count];
        for (int i = 0; i < craftingrecipes.Count; i++)
        {
            recipes[i] = ConvertCraftingRecipe(craftingrecipes[i]);
        }
        Packet_ServerCraftingRecipes p = new Packet_ServerCraftingRecipes();
        p.SetCraftingRecipes(recipes, recipes.Length, recipes.Length);
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.CraftingRecipes, CraftingRecipes = p }));
    }

    private Packet_CraftingRecipe ConvertCraftingRecipe(CraftingRecipe craftingRecipe)
    {
        if (craftingRecipe == null)
        {
            return null;
        }
        Packet_CraftingRecipe r = new Packet_CraftingRecipe();
        if (craftingRecipe.ingredients != null)
        {
            r.Ingredients = new Packet_Ingredient[craftingRecipe.ingredients.Length];
            for (int i = 0; i < craftingRecipe.ingredients.Length; i++)
            {
                r.Ingredients[i] = ConvertIngredient(craftingRecipe.ingredients[i]);
            }
            r.IngredientsCount = r.Ingredients.Length;
            r.IngredientsLength = r.Ingredients.Length;
        }
        if (craftingRecipe.output != null)
        {
            r.Output = new Packet_Ingredient();
            r.Output.Amount = craftingRecipe.output.Amount;
            r.Output.Type = craftingRecipe.output.Type;
        }
        return r;
    }

    private Packet_Ingredient ConvertIngredient(Ingredient ingredient)
    {
        Packet_Ingredient p = new Packet_Ingredient();
        p.Amount = ingredient.Amount;
        p.Type = ingredient.Type;
        return p;
    }

    private void SendLevelProgress(int clientid, int percentcomplete, string status)
    {
        Packet_ServerLevelProgress p = new Packet_ServerLevelProgress() { PercentComplete = percentcomplete, Status = status };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.LevelDataChunk, LevelDataChunk = p }));
    }
    public RenderHint RenderHint = RenderHint.Fast;
    private void SendServerIdentification(int clientid)
    {
        Packet_ServerIdentification p = new Packet_ServerIdentification()
        {
            MdProtocolVersion = GameVersion.Version,
            AssignedClientId = clientid,
            ServerName = config.Name,
            ServerMotd = config.Motd,
            //UsedBlobsMd5 = new List<byte[]>(new[] { terrainTextureMd5 }),
            //TerrainTextureMd5 = terrainTextureMd5,
            MapSizeX = d_Map.MapSizeX,
            MapSizeY = d_Map.MapSizeY,
            MapSizeZ = d_Map.MapSizeZ,
            DisableShadows = enableshadows ? 0 : 1,
            PlayerAreaSize = playerareasize,
            RenderHint_ = (int)RenderHint,
            RequiredBlobMd5 = GetRequiredBlobMd5(),
            RequiredBlobName = GetRequiredBlobName(),
        };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.ServerIdentification, Identification = p }));
    }

    public void SendFreemoveState(int clientid, bool isEnabled)
    {
        Packet_ServerFreemove p = new Packet_ServerFreemove()
        {
            IsEnabled = isEnabled ? 1 : 0
        };
        SendPacket(clientid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.Freemove, Freemove = p }));
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
    public Dictionary<int, ClientOnServer> clients = new Dictionary<int, ClientOnServer>();
    public Dictionary<string, bool> disabledprivileges = new Dictionary<string, bool>();
    public Dictionary<string, bool> extraPrivileges = new Dictionary<string, bool>();
    public ClientOnServer GetClient(int id)
    {
        if (id == this.serverConsoleId)
        {
            return this.serverConsoleClient;
        }
        if (!clients.ContainsKey(id))
            return null;
        return clients[id];
    }
    public ClientOnServer GetClient(string name)
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

    internal bool serverClientNeedsSaving;
    public ServerClient serverClient;
    public ManicDigger.Group defaultGroupGuest;
    public ManicDigger.Group defaultGroupRegistered;
    public Vector3i defaultPlayerSpawn;

    private Vector3i SpawnToVector3i(ManicDigger.Spawn spawn)
    {
        int x = spawn.x;
        int y = spawn.y;
        int z;
        if (!MapUtil.IsValidPos(d_Map, x, y))
        {
            throw new Exception(language.ServerInvalidSpawnCoordinates());
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
                throw new Exception(language.ServerInvalidSpawnCoordinates());
            }
        }
        return new Vector3i(x * 32, z * 32, y * 32);
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
        d_Data.UseBlockType(platform, id, BlockTypeConverter.GetBlockType(block));
    }
    public void SetBlockType(string name, BlockType block)
    {
        for (int i = 0; i < BlockTypes.Length; i++)
        {
            if (BlockTypes[i].Name == null)
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

    public bool IsSinglePlayer
    {
        get { return mainSockets[0].GetType() == typeof(DummyNetServer); }
    }

    public void SendDialog(int player, string id, Dialog dialog)
    {
        Packet_ServerDialog p = new Packet_ServerDialog()
        {
            DialogId = id,
            Dialog = ConvertDialog(dialog),
        };
        SendPacket(player, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.Dialog, Dialog = p }));
    }

    private Packet_Dialog ConvertDialog(Dialog dialog)
    {
        if (dialog == null)
        {
            return null;
        }
        Packet_Dialog p = new Packet_Dialog();
        p.Height_ = dialog.Height;
        p.IsModal = dialog.IsModal ? 1 : 0;
        if (dialog.Widgets != null)
        {
            Packet_Widget[] widgets = new Packet_Widget[dialog.Widgets.Length];
            for (int i = 0; i < dialog.Widgets.Length; i++)
            {
                widgets[i] = ConvertWidget(dialog.Widgets[i]);
            }
            p.SetWidgets(widgets, widgets.Length, widgets.Length);
        }
        p.Width = dialog.Width;
        return p;
    }

    private Packet_Widget ConvertWidget(Widget widget)
    {
        if (widget == null)
        {
            return null;
        }
        Packet_Widget w = new Packet_Widget();
        w.Click = widget.Click ? 1 : 0;
        w.ClickKey = widget.ClickKey;
        w.Color = widget.Color;
        w.Font = ConvertFont(widget.Font);
        w.Height_ = widget.Height;
        w.Id = widget.Id;
        w.Image = widget.Image;
        w.Text = widget.Text;
        w.Type = (int)widget.Type;
        w.Width = widget.Width;
        w.X = widget.X;
        w.Y = widget.Y;
        return w;
    }

    private Packet_DialogFont ConvertFont(DialogFont dialogFont)
    {
        if (dialogFont == null)
        {
            return null;
        }
        Packet_DialogFont f = new Packet_DialogFont();
        f.FamilyName = dialogFont.FamilyName;
        f.FontStyle = (int)dialogFont.FontStyle;
        f.SizeFloat = SerializeFloat(dialogFont.Size);
        return f;
    }

    public bool PlayerHasPrivilege(int player, string privilege)
    {
        if (extraPrivileges.ContainsKey(privilege))
        {
            return true;
        }
        if (disabledprivileges.ContainsKey(privilege))
        {
            return false;
        }
        return GetClient(player).privileges.Contains(privilege);
    }

    public void PlaySoundAt(int posx, int posy, int posz, string sound)
    {
        PlaySoundAtExceptPlayer(posx, posy, posz, sound, null);
    }

    public void PlaySoundAtExceptPlayer(int posx, int posy, int posz, string sound, int? player)
    {
        Vector3i pos = new Vector3i(posx, posy, posz);
        foreach (var k in clients)
        {
            if (player != null && player == k.Key)
            {
                continue;
            }
            int distance = DistanceSquared(new Vector3i((int)k.Value.PositionMul32GlX / 32, (int)k.Value.PositionMul32GlZ / 32, (int)k.Value.PositionMul32GlY / 32), pos);
            if (distance < 64 * 64)
            {
                SendSound(k.Key, sound, pos.x, posy, posz);
            }
        }
    }

    public void PlaySoundAt(int posx, int posy, int posz, string sound, int range)
    {
        PlaySoundAtExceptPlayer(posx, posy, posz, sound, null, range);
    }
    public void PlaySoundAtExceptPlayer(int posx, int posy, int posz, string sound, int? player, int range)
    {
        Vector3i pos = new Vector3i(posx, posy, posz);
        foreach (var k in clients)
        {
            if (player != null && player == k.Key)
            {
                continue;
            }
            int distance = DistanceSquared(new Vector3i((int)k.Value.PositionMul32GlX / 32, (int)k.Value.PositionMul32GlZ / 32, (int)k.Value.PositionMul32GlY / 32), pos);
            if (distance < range)
            {
                SendSound(k.Key, sound, pos.x, posy, posz);
            }
        }
    }

    public void SendPacketFollow(int player, int target, bool tpp)
    {
        Packet_ServerFollow p = new Packet_ServerFollow()
        {
            Client = target == -1 ? null : clients[target].playername,
            Tpp = tpp ? 1 : 0,
        };
        SendPacket(player, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.Follow, Follow = p }));
    }

    public void SendAmmo(int playerid, Dictionary<int, int> totalAmmo)
    {
        Packet_ServerAmmo p = new Packet_ServerAmmo();
        Packet_IntInt[] t = new Packet_IntInt[totalAmmo.Count];
        int i = 0;
        foreach (var k in totalAmmo)
        {
            t[i++] = new Packet_IntInt() { Key_ = k.Key, Value_ = k.Value };
        }
        p.TotalAmmoCount = totalAmmo.Count;
        p.TotalAmmoLength = totalAmmo.Count;
        p.TotalAmmo = t;
        SendPacket(playerid, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.Ammo, Ammo = p }));
    }

    public void SendExplosion(int player, float x, float y, float z, bool relativeposition, float range, float time)
    {
        Packet_ServerExplosion p = new Packet_ServerExplosion();
        p.XFloat = SerializeFloat(x);
        p.YFloat = SerializeFloat(y);
        p.ZFloat = SerializeFloat(z);
        p.IsRelativeToPlayerPosition = relativeposition ? 1 : 0;
        p.RangeFloat = SerializeFloat(range);
        p.TimeFloat = SerializeFloat(time);
        SendPacket(player, Serialize(new Packet_Server() { Id = Packet_ServerIdEnum.Explosion, Explosion = p }));
    }

    public string GetGroupColor(int playerid)
    {
        return GetClient(playerid).clientGroup.GroupColorString();
    }

    public string GetGroupName(int playerid)
    {
        return GetClient(playerid).clientGroup.Name;
    }

    internal void InstallHttpModule(string name, ManicDigger.Func<string> description, FragLabs.HTTP.IHttpModule module)
    {
        ActiveHttpModule m = new ActiveHttpModule();
        m.name = name;
        m.description = description;
        m.module = module;
        httpModules.Add(m);
    }
    internal List<ActiveHttpModule> httpModules = new List<ActiveHttpModule>();

    public ModEventHandlers modEventHandlers = new ModEventHandlers();

    internal static bool IsTransparentForLight(BlockType b)
    {
        return b.DrawType != DrawType.Solid && b.DrawType != DrawType.ClosedDoor;
    }

    public int GetSimulationCurrentFrame()
    {
        return simulationcurrentframe;
    }

    public GameTime GetTime()
    {
        return _time;
    }

    internal void PlayerEntitySetDirty(int player)
    {
        foreach (var k in clients.Values)
        {
            k.playersDirty[player] = true;
        }
    }

    internal ServerEntity GetEntity(int chunkx, int chunky, int chunkz, int id)
    {
        ServerChunk c = d_Map.GetChunk(chunkx * chunksize, chunky * chunksize, chunkz * chunksize);
        return c.Entities[id];
    }

    internal void SetEntityDirty(ServerEntityId id)
    {
        foreach (var k in clients)
        {
            for (int i = 0; i < k.Value.spawnedEntities.Length; i++)
            {
                ServerEntityId s = k.Value.spawnedEntities[i];
                if (s == null)
                {
                    continue;
                }
                if (s.chunkx == id.chunkx
                    && s.chunky == id.chunky
                    && s.chunkz == id.chunkz
                    && s.id == id.id)
                {
                    k.Value.updateEntity[i] = true;
                }
            }
        }
        ServerChunk chunk = d_Map.GetChunk(id.chunkx * Server.chunksize, id.chunky * Server.chunksize, id.chunkz * Server.chunksize);
        chunk.DirtyForSaving = true;
    }

    internal void DespawnEntity(ServerEntityId id)
    {
        ServerChunk chunk = d_Map.GetChunk(id.chunkx * Server.chunksize, id.chunky * Server.chunksize, id.chunkz * Server.chunksize);
        chunk.Entities[id.id] = null;
        if (id.id == chunk.EntitiesCount - 1)
        {
            chunk.EntitiesCount--;
        }
        chunk.DirtyForSaving = true;
    }

    internal void AddEntity(int x, int y, int z, ServerEntity e)
    {
        ServerChunk c = d_Map.GetChunk(x, y, z);
        if (c.Entities == null)
        {
            c.Entities = new ServerEntity[256];
        }
        if (c.Entities.Length < c.EntitiesCount + 1)
        {
            Array.Resize(ref c.Entities, c.EntitiesCount + 1);
        }
        c.Entities[c.EntitiesCount++] = e;
        c.DirtyForSaving = true;
    }
}

public class ClientOnServer
{
    public ClientOnServer()
    {
        float one = 1;
        entity = new ServerEntity();
        entity.drawName = new ServerEntityDrawName();
        entity.drawName.clientAutoComplete = true;
        entity.position = new ServerEntityPositionAndOrientation();
        entity.position.pitch = 2 * 255 / 4;
        entity.drawModel = new ServerEntityAnimatedModel();
        entity.drawModel.downloadSkin = true;
        Id = -1;
        state = ClientStateOnServer.Connecting;
        queryClient = true;
        received = new List<byte>();
        Ping = new Ping_();
        playername = Server.invalidplayername;
        Model = "player.txt";
        chunksseenTime = new Dictionary<int, int>();
        heightmapchunksseen = new Dictionary<Vector2i, int>();
        IsInventoryDirty = true;
        IsPlayerStatsDirty = true;
        FillLimit = 500;
        privileges = new List<string>();
        displayColor = "&f";
        EyeHeight = one * 15 / 10;
        ModelHeight = one * 17 / 10;
        WindowSize = new int[] { 800, 600 };
        playersDirty = new bool[128];
        for (int i = 0; i < 128; i++)
        {
            playersDirty[i] = true;
        }
        spawnedEntities = new ServerEntityId[64];
        spawnedEntitiesCount = 64;
        updateEntity = new bool[spawnedEntitiesCount];
    }
    internal int Id;
    internal int state; // ClientStateOnServer
    internal bool queryClient;
    internal NetServer mainSocket;
    internal NetConnection socket;
    internal List<byte> received;
    internal Ping_ Ping;
    internal float LastPing;
    internal string playername { get { return entity.drawName.name; } set { entity.drawName.name = value; } }
    internal int PositionMul32GlX { get { return (int)(entity.position.x * 32); } set { entity.position.x = (float)value / 32; } }
    internal int PositionMul32GlY { get { return (int)(entity.position.y * 32); } set { entity.position.y = (float)value / 32; } }
    internal int PositionMul32GlZ { get { return (int)(entity.position.z * 32); } set { entity.position.z = (float)value / 32; } }
    internal int positionheading { get { return (entity.position.heading); } set { entity.position.heading = (byte)value; } }
    internal int positionpitch { get { return (entity.position.pitch); } set { entity.position.pitch = (byte)value; } }
    internal byte stance { get { return entity.position.stance; } set { entity.position.stance = (byte)value; } }
    internal string Model { get { return entity.drawModel.model; } set { entity.drawModel.model = value; } }
    internal string Texture { get { return entity.drawModel.texture; } set { entity.drawModel.texture = value; } }
    internal Dictionary<int, int> chunksseenTime;
    internal bool[] chunksseen;
    internal Dictionary<Vector2i, int> heightmapchunksseen;
    internal Timer notifyMapTimer;
    internal bool IsInventoryDirty;
    internal bool IsPlayerStatsDirty;
    internal int FillLimit;
    //internal List<byte[]> blobstosend = new List<byte[]>();
    internal ManicDigger.Group clientGroup;
    internal bool IsBot;
    public void AssignGroup(ManicDigger.Group newGroup)
    {
        this.clientGroup = newGroup;
        this.privileges.Clear();
        this.privileges.AddRange(newGroup.GroupPrivileges);
        this.color = newGroup.GroupColorString();
    }
    internal List<string> privileges;
    internal string color;
    internal string displayColor { get { return entity.drawName.color; } set { entity.drawName.color = value; } }
    public string ColoredPlayername(string subsequentColor)
    {
        return this.color + this.playername + subsequentColor;
    }
    internal Timer notifyMonstersTimer;
    internal IScriptInterpreter Interpreter;
    internal ScriptConsole Console;

    public override string ToString()
    {
        string ip = "";
        if (this.socket != null)
        {
            ip = (this.socket.RemoteEndPoint()).AddressToString();
        }
        // Format: Playername:Group:Privileges IP
        return string.Format("{0}:{1}:{2} {3}", this.playername, this.clientGroup.Name,
            ServerClientMisc.PrivilegesString(this.privileges), ip);
    }
    internal float EyeHeight { get { return entity.drawModel.eyeHeight; } set { entity.drawModel.eyeHeight = value; } }
    internal float ModelHeight { get { return entity.drawModel.modelHeight; } set { entity.drawModel.modelHeight = value; } }
    internal int ActiveMaterialSlot;
    internal bool IsSpectator;
    internal bool usingFill;
    internal int[] WindowSize;
    internal float notifyPlayerPositionsAccum;
    internal bool[] playersDirty;
    internal ServerEntity entity;
    internal ServerEntityPositionAndOrientation positionOverride;
    internal float notifyEntitiesAccum;
    internal ServerEntityId[] spawnedEntities;
    internal int spawnedEntitiesCount;
    internal ServerEntityId editingSign;
    internal bool[] updateEntity;
}

public class ServerEntityId
{
    internal int chunkx;
    internal int chunky;
    internal int chunkz;
    internal int id;

    internal ServerEntityId Clone()
    {
        ServerEntityId ret = new ServerEntityId();
        ret.chunkx = chunkx;
        ret.chunky = chunky;
        ret.chunkz = chunkz;
        ret.id = id;
        return ret;
    }
}

public class ModEventHandlers
{
    public List<ModDelegates.WorldGenerator> getchunk = new List<ModDelegates.WorldGenerator>();
    public List<ModDelegates.BlockUse> onuse = new List<ModDelegates.BlockUse>();
    public List<ModDelegates.BlockBuild> onbuild = new List<ModDelegates.BlockBuild>();
    public List<ModDelegates.BlockDelete> ondelete = new List<ModDelegates.BlockDelete>();
    public List<ModDelegates.BlockUseWithTool> onusewithtool = new List<ModDelegates.BlockUseWithTool>();
    public List<ModDelegates.ChangedActiveMaterialSlot> changedactivematerialslot = new List<ModDelegates.ChangedActiveMaterialSlot>();
    public List<ModDelegates.BlockUpdate> blockticks = new List<ModDelegates.BlockUpdate>();
    public List<ModDelegates.PopulateChunk> populatechunk = new List<ModDelegates.PopulateChunk>();
    public List<ModDelegates.Command> oncommand = new List<ModDelegates.Command>();
    public List<ModDelegates.WeaponShot> onweaponshot = new List<ModDelegates.WeaponShot>();
    public List<ModDelegates.WeaponHit> onweaponhit = new List<ModDelegates.WeaponHit>();
    public List<ModDelegates.SpecialKey1> onspecialkey = new List<ModDelegates.SpecialKey1>();
    public List<ModDelegates.PlayerJoin> onplayerjoin = new List<ModDelegates.PlayerJoin>();
    public List<ModDelegates.PlayerLeave> onplayerleave = new List<ModDelegates.PlayerLeave>();
    public List<ModDelegates.PlayerDisconnect> onplayerdisconnect = new List<ModDelegates.PlayerDisconnect>();
    public List<ModDelegates.PlayerChat> onplayerchat = new List<ModDelegates.PlayerChat>();
    public List<ModDelegates.PlayerDeath> onplayerdeath = new List<ModDelegates.PlayerDeath>();
    public List<ModDelegates.DialogClick> ondialogclick = new List<ModDelegates.DialogClick>();
    public List<ModDelegates.DialogClick2> ondialogclick2 = new List<ModDelegates.DialogClick2>();
    public List<ModDelegates.LoadWorld> onloadworld = new List<ModDelegates.LoadWorld>();
    public List<ModDelegates.UpdateEntity> onupdateentity = new List<ModDelegates.UpdateEntity>();
    public List<ModDelegates.UseEntity> onuseentity = new List<ModDelegates.UseEntity>();
    public List<ModDelegates.HitEntity> onhitentity = new List<ModDelegates.HitEntity>();
    public List<ModDelegates.Permission> onpermission = new List<ModDelegates.Permission>();
    public List<ModDelegates.CheckBlockUse> checkonuse = new List<ModDelegates.CheckBlockUse>();
    public List<ModDelegates.CheckBlockBuild> checkonbuild = new List<ModDelegates.CheckBlockBuild>();
    public List<ModDelegates.CheckBlockDelete> checkondelete = new List<ModDelegates.CheckBlockDelete>();
}

#region GameTime
public class GameTime
{
    private int _nIngameSecondsEveryRealTimeSecond = 60;
    private int _nDaysPerYear = 4;
    private long _nLastIngameSecond = 0;

    private Stopwatch _watchIngameTime = null;
    private TimeSpan _time = TimeSpan.Zero;

    /// <summary>
    /// This changes how fast time goes on
    /// 0 freezes the time
    /// can be negative to let time run backwards
    /// </summary>
    public int SpeedOfTime
    {
        get { return _nIngameSecondsEveryRealTimeSecond; }
        set { _nIngameSecondsEveryRealTimeSecond = value; }
    }

    /// <summary>
    /// Days of a year
    /// </summary>
    public int DaysPerYear
    {
        get { return _nDaysPerYear; }
        set { _nDaysPerYear = value; }
    }

    /// <summary>
    /// ctor
    /// </summary>
    internal GameTime()
    {
        _watchIngameTime = new Stopwatch();
    }

    /// <summary>
    /// Initializes the GameTime component
    /// </summary>
    /// <param name="ticks"></param>
    internal void Init(long ticks)
    {
        _time = TimeSpan.FromTicks(ticks);
        _watchIngameTime.Start();
    }

    /// <summary>
    /// Allows the time to tick
    /// </summary>
    internal void Start()
    {
        _watchIngameTime.Start();
    }

    /// <summary>
    /// Stops the time from ticking
    /// </summary>
    internal void Stop()
    {
        _watchIngameTime.Stop();
    }

    /// <summary>
    /// returns the time
    /// </summary>
    public TimeSpan Time
    {
        get { return _time; }
    }

    /// <summary>
    /// Hour of the day
    /// </summary>
    public int Hour
    {
        get { return _time.Hours; }
    }

    /// <summary>
    /// Total hours
    /// </summary>
    public double HourTotal
    {
        get { return _time.TotalHours; }
    }

    /// <summary>
    /// Day of the year
    /// </summary>
    public int Day
    {
        get { return (int)(_time.TotalDays % _nDaysPerYear); }
    }

    /// <summary>
    /// Total days
    /// </summary>
    public double DaysTotal
    {
        get { return _time.TotalDays; }
    }

    /// <summary>
    /// The current year
    /// </summary>
    public int Year
    {
        get { return (int)(_time.TotalDays / _nDaysPerYear); }
    }

    /// <summary>
    /// Gets the current season
    /// from 0 to 3
    /// </summary>
    public int Season
    {
        get { return Year % 4; }
    }

    /// <summary>
    /// Returns the amount of 15 mins that passed for this day
    /// </summary>
    /// <returns></returns>
    public int GetQuarterHourPartOfDay()
    {
        //(_tsIngameTime.Hours * 4)
        //for every hour of the current day, we got 4 x 15minutes

        //(_tsIngameTime.Minutes / 15)
        //add the 15 minutes of the current hour

        //TODO: +1 at the end beacause midnight causes daylight :/
        int nReturn = (_time.Hours * 4) + (_time.Minutes / 15) + 1;
        return nReturn;
    }

    /// <summary>
    /// Ticks the clock if neccesarry.
    /// Returns true if time changed
    /// </summary>
    /// <returns></returns>
    internal bool Tick()
    {
        bool blnTicked = false;

        //update the time of day every second
        while (_nLastIngameSecond < (_watchIngameTime.ElapsedMilliseconds / 1000))
        {
            //Update gametime
            _time += TimeSpan.FromSeconds(_nIngameSecondsEveryRealTimeSecond);

            ++_nLastIngameSecond;

            blnTicked = true;
        }

        return blnTicked;
    }

    /// <summary>
    /// change the time of the day
    /// </summary>
    internal void Set(TimeSpan time)
    {
        _time = time;
    }

    /// <summary>
    /// Adds the given time
    /// </summary>
    /// <param name="time"></param>
    internal void Add(TimeSpan time)
    {
        _time += time;
    }
}
#endregion

public class BlockTypeConverter
{
    public static Packet_BlockType GetBlockType(BlockType block)
    {
        Packet_BlockType p = new Packet_BlockType();
        p.AimRadiusFloat = Server.SerializeFloat(block.AimRadius);
        p.AmmoMagazine = block.AmmoMagazine;
        p.AmmoTotal = block.AmmoTotal;
        p.BulletsPerShotFloat = Server.SerializeFloat(block.BulletsPerShot);
        p.DamageBodyFloat = Server.SerializeFloat(block.DamageBody);
        p.DamageHeadFloat = Server.SerializeFloat(block.DamageHead);
        p.DamageToPlayer = block.DamageToPlayer;
        p.DelayFloat = Server.SerializeFloat(block.Delay);
        p.DrawType = (int)block.DrawType;
        p.ExplosionRangeFloat = Server.SerializeFloat(block.ExplosionRange);
        p.ExplosionTimeFloat = Server.SerializeFloat(block.ExplosionTime);
        p.Handimage = block.handimage;
        p.IronSightsAimRadiusFloat = Server.SerializeFloat(block.IronSightsAimRadius);
        p.IronSightsEnabled = block.IronSightsEnabled;
        p.IronSightsFovFloat = Server.SerializeFloat(block.IronSightsFov);
        p.IronSightsImage = block.IronSightsImage;
        p.IronSightsMoveSpeedFloat = Server.SerializeFloat(block.IronSightsMoveSpeed);
        p.IsBuildable = block.IsBuildable;
        p.IsPistol = block.IsPistol;
        p.IsSlipperyWalk = block.IsSlipperyWalk;
        p.IsTool = block.IsTool;
        p.IsUsable = block.IsUsable;
        p.LightRadius = block.LightRadius;
        p.Name = block.Name;
        p.PistolType = (int)block.PistolType;
        p.ProjectileBounce = block.ProjectileBounce;
        p.ProjectileSpeedFloat = Server.SerializeFloat(block.ProjectileSpeed);
        p.Rail = block.Rail;
        p.RecoilFloat = Server.SerializeFloat(block.Recoil);
        p.ReloadDelayFloat = Server.SerializeFloat(block.ReloadDelay);
        p.Sounds = GetSoundSet(block.Sounds);
        p.StartInventoryAmount = block.StartInventoryAmount;
        p.Strength = block.Strength;
        p.TextureIdBack = block.TextureIdBack;
        p.TextureIdBottom = block.TextureIdBottom;
        p.TextureIdForInventory = block.TextureIdForInventory;
        p.TextureIdFront = block.TextureIdFront;
        p.TextureIdLeft = block.TextureIdLeft;
        p.TextureIdRight = block.TextureIdRight;
        p.TextureIdTop = block.TextureIdTop;
        p.WalkableType = (int)block.WalkableType;
        p.WalkSpeedFloat = Server.SerializeFloat(block.WalkSpeed);
        p.WalkSpeedWhenUsedFloat = Server.SerializeFloat(block.WalkSpeedWhenUsed);
        p.WhenPlacedGetsConvertedTo = block.WhenPlayerPlacesGetsConvertedTo;
        p.PickDistanceWhenUsedFloat = Server.SerializeFloat(block.PickDistanceWhenUsed);
        return p;
    }

    private static Packet_SoundSet GetSoundSet(SoundSet soundSet)
    {
        if (soundSet == null)
        {
            return null;
        }
        Packet_SoundSet p = new Packet_SoundSet();
        p.SetBreak1(soundSet.Break, soundSet.Break.Length, soundSet.Break.Length);
        p.SetBuild(soundSet.Build, soundSet.Build.Length, soundSet.Build.Length);
        p.SetClone(soundSet.Clone, soundSet.Clone.Length, soundSet.Clone.Length);
        p.SetReload(soundSet.Reload, soundSet.Reload.Length, soundSet.Reload.Length);
        p.SetShoot(soundSet.Shoot, soundSet.Shoot.Length, soundSet.Shoot.Length);
        p.SetShootEnd(soundSet.ShootEnd, soundSet.ShootEnd.Length, soundSet.ShootEnd.Length);
        p.SetWalk(soundSet.Walk, soundSet.Walk.Length, soundSet.Walk.Length);
        return p;
    }
}

public class MyLinq
{
    public static bool Any<T>(IEnumerable<T> l)
    {
        return l.GetEnumerator().MoveNext();
    }
    public static T First<T>(IEnumerable<T> l)
    {
        var e = l.GetEnumerator();
        e.MoveNext();
        return e.Current;
    }
    public static int Count<T>(IEnumerable<T> l)
    {
        int count = 0;
        foreach (T v in l)
        {
            count++;
        }
        return count;
    }
    public static IEnumerable<T> Take<T>(IEnumerable<T> l, int n)
    {
        int i = 0;
        foreach (var v in l)
        {
            if (i >= n)
            {
                yield break;
            }
            yield return v;
            i++;
        }
    }
    public static IEnumerable<T> Skip<T>(IEnumerable<T> l, int n)
    {
        var iterator = l.GetEnumerator();
        for (int i = 0; i < n; i++)
        {
            if (iterator.MoveNext() == false)
                yield break;
        }
        while (iterator.MoveNext())
            yield return iterator.Current;
    }
}
public interface ICurrentTime
{
    int GetSimulationCurrentFrame();
}
public class CurrentTimeDummy : ICurrentTime
{
    public int GetSimulationCurrentFrame() { return 0; }
}
public static class MapUtil
{
    public static int Index2d(int x, int y, int sizex)
    {
        return x + y * sizex;
    }

    public static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }

    public static Vector3i Pos(int index, int sizex, int sizey)
    {
        int x = index % sizex;
        int y = (index / sizex) % sizey;
        int h = index / (sizex * sizey);
        return new Vector3i(x, y, h);
    }

    public static bool IsValidPos(IMapStorage2 map, int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0)
        {
            return false;
        }
        if (x >= map.GetMapSizeX() || y >= map.GetMapSizeY() || z >= map.GetMapSizeZ())
        {
            return false;
        }
        return true;
    }

    public static bool IsValidPos(IMapStorage2 map, int x, int y)
    {
        if (x < 0 || y < 0)
        {
            return false;
        }
        if (x >= map.GetMapSizeX() || y >= map.GetMapSizeY())
        {
            return false;
        }
        return true;
    }

    public static bool IsValidChunkPos(IMapStorage2 map, int cx, int cy, int cz, int chunksize)
    {
        return cx >= 0 && cy >= 0 && cz >= 0
            && cx < map.GetMapSizeX() / chunksize
            && cy < map.GetMapSizeY() / chunksize
            && cz < map.GetMapSizeZ() / chunksize;
    }

    public static int blockheight(IMapStorage2 map, int tileidempty, int x, int y)
    {
        for (int z = map.GetMapSizeZ() - 1; z >= 0; z--)
        {
            if (map.GetBlock(x, y, z) != tileidempty)
            {
                return z + 1;
            }
        }
        return map.GetMapSizeZ() / 2;
    }

    static ulong pow20minus1 = 1048576 - 1;
    public static Vector3i FromMapPos(ulong v)
    {
        uint z = (uint)(v & pow20minus1);
        v = v >> 20;
        uint y = (uint)(v & pow20minus1);
        v = v >> 20;
        uint x = (uint)(v & pow20minus1);
        return new Vector3i((int)x, (int)y, (int)z);
    }

    public static ulong ToMapPos(int x, int y, int z)
    {
        ulong v = 0;
        v = (ulong)x << 40;
        v |= (ulong)y << 20;
        v |= (ulong)z;
        return v;
    }

    public static int SearchColumn(IMapStorage2 map, int x, int y, int id, int startH)
    {
        for (int h = startH; h > 0; h--)
        {
            if (map.GetBlock(x, y, h) == (byte)id)
            {
                return h;
            }
        }
        return -1; // -1 means 'not found'
    }

    public static int SearchColumn(IMapStorage2 map, int x, int y, int id)
    {
        return SearchColumn(map, x, y, id, map.GetMapSizeZ() - 1);
    }

    public static bool IsSolidChunk(ushort[] chunk)
    {
        for (int i = 0; i <= chunk.GetUpperBound(0); i++)
        {
            if (chunk[i] != chunk[0])
            {
                return false;
            }
        }
        return true;
    }

    public static Point PlayerArea(int playerAreaSize, int centerAreaSize, Vector3i blockPosition)
    {
        Point p = PlayerCenterArea(playerAreaSize, centerAreaSize, blockPosition);
        int x = p.X + centerAreaSize / 2;
        int y = p.Y + centerAreaSize / 2;
        x -= playerAreaSize / 2;
        y -= playerAreaSize / 2;
        return new Point(x, y);
    }

    public static Point PlayerCenterArea(int playerAreaSize, int centerAreaSize, Vector3i blockPosition)
    {
        int px = blockPosition.x;
        int py = blockPosition.y;
        int gridposx = (px / centerAreaSize) * centerAreaSize;
        int gridposy = (py / centerAreaSize) * centerAreaSize;
        return new Point(gridposx, gridposy);
    }
}
public class MapManipulator
{
    public const string BinSaveExtension = ".mddbs";
}
public class Timer
{
    public double INTERVAL { get { return interval; } set { interval = value; } }
    public double MaxDeltaTime { get { return maxDeltaTime; } set { maxDeltaTime = value; } }
    double interval = 1;
    double maxDeltaTime = double.PositiveInfinity;

    double starttime;
    double oldtime;
    public double accumulator;
    public Timer()
    {
        Reset();
    }
    public void Reset()
    {
        starttime = gettime();
    }
    public delegate void Tick();
    public void Update(Tick tick)
    {
        double currenttime = gettime() - starttime;
        double deltaTime = currenttime - oldtime;
        accumulator += deltaTime;
        double dt = INTERVAL;
        if (MaxDeltaTime != double.PositiveInfinity && accumulator > MaxDeltaTime)
        {
            accumulator = MaxDeltaTime;
        }
        while (accumulator >= dt)
        {
            tick();
            accumulator -= dt;
        }
        oldtime = currenttime;
    }
    static double gettime()
    {
        return (double)DateTime.UtcNow.Ticks / (10 * 1000 * 1000);
    }
}
public struct Vector2i
{
    public Vector2i(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public int x;
    public int y;
    public override bool Equals(object obj)
    {
        if (obj is Vector2i)
        {
            Vector2i other = (Vector2i)obj;
            return this.x == other.x && this.y == other.y;
        }
        return base.Equals(obj);
    }
    public static bool operator ==(Vector2i a, Vector2i b)
    {
        return a.x == b.x && a.y == b.y;
    }
    public static bool operator !=(Vector2i a, Vector2i b)
    {
        return !(a.x == b.x && a.y == b.y);
    }
    public override int GetHashCode()
    {
        int hash = 23;
        unchecked
        {
            hash = hash * 37 + x;
            hash = hash * 37 + y;
        }
        return hash;
    }
    public override string ToString()
    {
        return string.Format("[{0}, {1}]", x, y);
    }
}
public struct Vector3i
{
    public int x;
    public int y;
    public int z;

    public Vector3i(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3i Add(Vector3i v)
    {
        return Add(v.x, v.y, v.z);
    }

    public Vector3i Add(int x, int y, int z)
    {
        return new Vector3i(this.x + x, this.y + y, this.z + z);
    }

    public override bool Equals(object obj)
    {
        if (obj is Vector3i)
        {
            Vector3i other = (Vector3i)obj;
            return this.x == other.x && this.y == other.y && this.z == other.z;
        }
        return base.Equals(obj);
    }
    public static bool operator ==(Vector3i a, Vector3i b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }
    public static bool operator !=(Vector3i a, Vector3i b)
    {
        return !(a.x == b.x && a.y == b.y && a.z == b.z);
    }
    public override int GetHashCode()
    {
        int hash = 23;
        unchecked
        {
            hash = hash * 37 + x;
            hash = hash * 37 + y;
            hash = hash * 37 + z;
        }
        return hash;
    }
    public override string ToString()
    {
        return string.Format("[{0}, {1}, {2}]", x, y, z);
    }
}

[ProtoContract()]
public class Ingredient
{
    [ProtoMember(1)]
    public int Type;
    [ProtoMember(2)]
    public int Amount;
}

[ProtoContract()]
public class CraftingRecipe
{
    [ProtoMember(1)]
    public Ingredient[] ingredients;
    [ProtoMember(2)]
    public Ingredient output;
}

public class EntityHeading
{
    public static byte GetHeading(float posx, float posy, float targetx, float targety)
    {
        float deltaX = targetx - posx;
        float deltaY = targety - posy;
        //Angle to x-axis: cos(beta) = x / |length|
        double headingDeg = (360.0 / (2.0 * Math.PI)) * Math.Acos(deltaX / Math.Sqrt(deltaX * deltaX + deltaY * deltaY)) + 90.0;
        //Add 2 Pi if value is negative
        if (deltaY < 0)
        {
            headingDeg = -headingDeg - 180.0;
        }
        if (headingDeg < 0)
        {
            headingDeg += 360.0;
        }
        if (headingDeg > 360.0)
        {
            headingDeg -= 360.0;
        }
        //Convert to value between 0 and 255 and return
        return (byte)((headingDeg / 360.0) * 256.0);
    }
}


public abstract class ServerSystem
{
    public ServerSystem()
    {
        one = 1;
    }
    internal float one;
    public virtual void Update(Server server, float dt) { }
    public virtual void OnRestart(Server server) { }
    public virtual bool OnCommand(Server server, int sourceClientId, string command, string argument) { return false; }
}

public abstract class ServerPlatform
{
    public abstract void QueueUserWorkItem(Action_ action);
    public abstract int FloatToInt(float value);
}

public class ServerPlatformNative : ServerPlatform
{
    public override void QueueUserWorkItem(Action_ action)
    {
        ThreadPool.QueueUserWorkItem((a) => { action.Run(); });
    }

    public override int FloatToInt(float value)
    {
        return (int)value;
    }
}
