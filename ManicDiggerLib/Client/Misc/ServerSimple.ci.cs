public class ServerSimple
{
    public ServerSimple()
    {
        one = 1;
        clients = new ClientSimple[256];
        clientsCount = 0;
        blockTypes = new Packet_BlockType[GlobalVar.MAX_BLOCKTYPES];
        blockTypesCount = 0;
        mods = new ModSimple[128];

        ModManagerSimple1 m = new ModManagerSimple1();
        m.Start(this);

        mods[modsCount++] = new ModSimpleDefault();
        mods[modsCount++] = new ModSimpleWorldGenerator();
        for (int i = 0; i < modsCount; i++)
        {
            mods[i].Start(m);
        }

        MapSizeX = 8192;
        MapSizeY = 8192;
        MapSizeZ = 128;
        chunks = new ChunkSimple[(MapSizeX / ChunkSize) * (MapSizeY / ChunkSize)][];
        chunkdrawdistance = 4;
        actions = new QueueAction();
        mainThreadActions = new QueueAction();

        spawnGlX = MapSizeX / 2;
        spawnGlY = MapSizeZ;
        for (int i = 0; i < modsCount; i++)
        {
            int spawnHeight = mods[i].GetHeight();
            if (spawnHeight != -1)
            {
                spawnGlY = spawnHeight;
            }
        }
        spawnGlZ = MapSizeY / 2;
    }

    internal ModSimple[] mods;
    internal int modsCount;


    float one;
    NetServer server;
    string saveFilename;
    internal GamePlatform platform;
    public void Start(NetServer server_, string saveFilename_, GamePlatform platform_)
    {
        server = server_;
        saveFilename = saveFilename_;
        platform = platform_;
        mainThreadActionsLock = platform.MonitorCreate();
    }

    public void Update()
    {
        ProcessPackets();
        NotifyMap();
        NotifyInventory();
        NotifyPing();
        ProcessActions();
    }

    void NotifyPing()
    {
        for (int i = 0; i < clientsCount; i++)
        {
            if (clients[i] == null)
            {
                continue;
            }
            int now = platform.TimeMillisecondsFromStart();
            if ((now - clients[i].pingLastMilliseconds) > 1000)
            {
                SendPacket(i, ServerPackets.Ping());
                clients[i].pingLastMilliseconds = now;
            }
        }
    }

    void NotifyInventory()
    {
        for (int i = 0; i < clientsCount; i++)
        {
            if (clients[i] == null)
            {
                continue;
            }
            if (!clients[i].connected)
            {
                continue;
            }
            if (!clients[i].inventoryDirty)
            {
                continue;
            }
            SendPacket(i, ServerPackets.Inventory(clients[i].inventory));
            clients[i].inventoryDirty = false;
        }
    }

    void NotifyMap()
    {
        for (int i = 0; i < clientsCount; i++)
        {
            if (clients[i] == null)
            {
                continue;
            }
            if (!clients[i].connected)
            {
                continue;
            }
            if (clients[i].notifyMapAction == null)
            {
                NotifyMapAction notify = new NotifyMapAction();
                notify.server = this;
                notify.clientId = i;
                clients[i].notifyMapAction = notify;
                platform.QueueUserWorkItem(notify);
            }
        }
    }

    internal ClientSimple[] clients;
    internal int clientsCount;
    int spawnGlX;
    int spawnGlY;
    int spawnGlZ;

    void ProcessPackets()
    {
        for (; ; )
        {
            NetIncomingMessage msg = server.ReadMessage();
            if (msg == null)
            {
                return;
            }
            switch (msg.Type)
            {
                case NetworkMessageType.Connect:
                    ClientSimple c = new ClientSimple();
                    c.MainSocket = server;
                    c.Connection = msg.SenderConnection;
                    c.chunksseen = new bool[(MapSizeX / ChunkSize) * (MapSizeY / ChunkSize)][];
                    clients[0] = c;
                    clientsCount = 1;
                    break;
                case NetworkMessageType.Data:
                    byte[] data = msg.message;
                    Packet_Client packet = new Packet_Client();
                    Packet_ClientSerializer.DeserializeBuffer(data, msg.messageLength, packet);
                    ProcessPacket(0, packet);
                    break;
                case NetworkMessageType.Disconnect:
                    break;
            }
        }
    }

    void ProcessPacket(int client, Packet_Client packet)
    {
        switch (packet.GetId())
        {
            case Packet_ClientIdEnum.PlayerIdentification:
                {
                    if (packet.Identification == null)
                    {
                        return;
                    }
                    SendPacket(client, ServerPackets.Identification(0, MapSizeX, MapSizeY, MapSizeZ, platform.GetGameVersion()));
                    clients[client].Name = packet.Identification.Username;
                }
                break;
            case Packet_ClientIdEnum.RequestBlob:
                {
                    SendPacket(client, ServerPackets.LevelInitialize());
                    for (int i = 0; i < blockTypesCount; i++)
                    {
                        Packet_BlockType blocktype = blockTypes[i];
                        if (blocktype == null)
                        {
                            blocktype = new Packet_BlockType();
                        }
                        SendPacket(client, ServerPackets.BlockType(i, blocktype));
                    }
                    SendPacket(client, ServerPackets.BlockTypes());
                    SendPacket(client, ServerPackets.LevelFinalize());
                    for (int i = 0; i < clientsCount; i++)
                    {
                        if (clients[i] == null)
                        {
                            continue;
                        }
                        clients[i].glX = spawnGlX;
                        clients[i].glY = spawnGlY;
                        clients[i].glZ = spawnGlZ;
                        Packet_PositionAndOrientation pos = new Packet_PositionAndOrientation();
                        pos.X = platform.FloatToInt(32 * clients[i].glX);
                        pos.Y = platform.FloatToInt(32 * clients[i].glY);
                        pos.Z = platform.FloatToInt(32 * clients[i].glZ);
                        pos.Pitch = 255 / 2;
                        //SendPacket(client, ServerPackets.Spawn(i, clients[i].Name, pos));
                        Packet_ServerEntity e = new Packet_ServerEntity();
                        e.DrawModel = new Packet_ServerEntityAnimatedModel();
                        e.DrawModel.Model_ = "player.txt";
                        e.DrawModel.ModelHeight = platform.FloatToInt((one * 17 / 10) * 32);
                        e.DrawModel.EyeHeight = platform.FloatToInt((one * 15 / 10) * 32);
                        e.Position = pos;
                        SendPacket(client, ServerPackets.EntitySpawn(0, e));
                        SendPacket(client, ServerPackets.PlayerStats(100, 100, 100, 100));
                    }
                    for (int i = 0; i < modsCount; i++)
                    {
                        mods[i].OnPlayerJoin(client);
                    }
                    clients[client].connected = true;
                }
                break;
            case Packet_ClientIdEnum.Message:
                {
                    SendPacketToAll(ServerPackets.Message(platform.StringFormat2("{0}: &f{1}", clients[client].Name, packet.Message.Message)));
                }
                break;
            case Packet_ClientIdEnum.SetBlock:
                {
                    int x = packet.SetBlock.X;
                    int y = packet.SetBlock.Y;
                    int z = packet.SetBlock.Z;
                    int block = packet.SetBlock.BlockType;
                    int mode = packet.SetBlock.Mode;
                    if (mode == Packet_BlockSetModeEnum.Create)
                    {
                    }
                    if (mode == Packet_BlockSetModeEnum.Destroy)
                    {
                        SendPacketToAll(ServerPackets.SetBlock(x, y, z, 0));
                    }
                    if (mode == Packet_BlockSetModeEnum.Use)
                    {
                    }
                    if (mode == Packet_BlockSetModeEnum.UseWithTool)
                    {
                    }
                }
                break;
            case Packet_ClientIdEnum.PositionandOrientation:
                {
                    clients[client].glX = one * packet.PositionAndOrientation.X / 32;
                    clients[client].glY = one * packet.PositionAndOrientation.Y / 32;
                    clients[client].glZ = one * packet.PositionAndOrientation.Z / 32;
                }
                break;
            case Packet_ClientIdEnum.InventoryAction:
                {
                    switch (packet.InventoryAction.Action)
                    {
                        case Packet_InventoryActionTypeEnum.Click:
                            break;
                    }
                }
                break;
        }
    }

    void SendPacketToAll(Packet_Server packet)
    {
        for (int i = 0; i < clientsCount; i++)
        {
            SendPacket(i, packet);
        }
    }

    public void SendPacket(int client, Packet_Server packet)
    {
        IntRef length = new IntRef();
        byte[] data = ServerPackets.Serialize(packet, length);
        INetOutgoingMessage msg = new INetOutgoingMessage();
        msg.Write(data, length.value);
        clients[client].Connection.SendMessage(msg, MyNetDeliveryMethod.ReliableOrdered, 0);
    }
    internal Packet_BlockType[] blockTypes;
    internal int blockTypesCount;
    
    internal ChunkSimple[][] chunks;
    internal int MapSizeX;
    internal int MapSizeY;
    internal int MapSizeZ;
    public const int ChunkSize = 32;
    internal int chunkdrawdistance;

    public void QueueMainThreadAction(Action_ action)
    {
        platform.MonitorEnter(mainThreadActionsLock);
        mainThreadActions.Enqueue(action);
        platform.MonitorExit(mainThreadActionsLock);
    }
    MonitorObject mainThreadActionsLock;
    QueueAction mainThreadActions;
    void ProcessActions()
    {
        Move(mainThreadActions, actions);
        while (actions.Count() > 0)
        {
            Action_ a = actions.Dequeue();
            a.Run();
        }
    }
    QueueAction actions;
    void Move(QueueAction from, QueueAction to)
    {
        platform.MonitorEnter(mainThreadActionsLock);
        int count = from.count;
        for (int i = 0; i < count; i++)
        {
            Action_ task = from.Dequeue();
            to.Enqueue(task);
        }
        platform.MonitorExit(mainThreadActionsLock);
    }
}

public class SendPacketAction : Action_
{
    public static SendPacketAction Create(ServerSimple server_, int client_, Packet_Server packet_)
    {
        SendPacketAction a = new SendPacketAction();
        a.server = server_;
        a.client = client_;
        a.packet = packet_;
        return a;
    }
    internal ServerSimple server;
    internal int client;
    internal Packet_Server packet;
    public override void Run()
    {
        server.SendPacket(client, packet);
    }
}

class NotifyMapAction : Action_
{
    internal ServerSimple server;
    internal int clientId;
    public override void Run()
    {
        int[] nearest = new int[3];
        ClientSimple client = server.clients[clientId];
        int x = server.platform.FloatToInt(client.glX);
        int y = server.platform.FloatToInt(client.glZ);
        int z = server.platform.FloatToInt(client.glY);
        NearestDirty(clientId, x, y, z, nearest);

        if (nearest[0] != -1)
        {
            LoadAndSendChunk(nearest[0], nearest[1], nearest[2]);
        }

        server.clients[clientId].notifyMapAction = null;
    }

    void LoadAndSendChunk(int x, int y, int z)
    {
        ClientSimple c = server.clients[clientId];
        int pos = MapUtilCi.Index2d(x, y, server.MapSizeX / ServerSimple.ChunkSize);
        if (c.chunksseen[pos] == null)
        {
            c.chunksseen[pos] = new bool[server.MapSizeZ / ServerSimple.ChunkSize];
        }
        c.chunksseen[pos][z] = true;

        int[] chunk = new int[32 * 32 * 32];

        for (int i = 0; i < server.modsCount; i++)
        {
            server.mods[i].GenerateChunk(x, y, z, chunk);
        }

        byte[] chunkBytes = MiscCi.UshortArrayToByteArray(chunk, 32 * 32 * 32);
        IntRef compressedLength = new IntRef();
        byte[] chunkCompressed = server.platform.GzipCompress(chunkBytes, 32 * 32 * 32 * 2, compressedLength);
        
        server.QueueMainThreadAction(SendPacketAction.Create(server, clientId, ServerPackets.ChunkPart(chunkCompressed)));
        server.QueueMainThreadAction(SendPacketAction.Create(server, clientId, ServerPackets.Chunk_(x * ServerSimple.ChunkSize, y * ServerSimple.ChunkSize, z * ServerSimple.ChunkSize, ServerSimple.ChunkSize)));
    }

    int mapAreaSize() { return server.chunkdrawdistance * ServerSimple.ChunkSize * 2; }
    int mapAreaSizeZ() { return mapAreaSize(); }

    int mapsizexchunks() { return server.MapSizeX / ServerSimple.ChunkSize; }
    int mapsizeychunks() { return server.MapSizeY / ServerSimple.ChunkSize; }
    int mapsizezchunks() { return server.MapSizeZ / ServerSimple.ChunkSize; }

    const int intMaxValue = 2147483647;
    void NearestDirty(int clientid, int playerx, int playery, int playerz, int[] retNearest)
    {
        int nearestdist = intMaxValue;
        retNearest[0] = -1;
        retNearest[1] = -1;
        retNearest[2] = -1;
        int px = (playerx) / ServerSimple.ChunkSize;
        int py = (playery) / ServerSimple.ChunkSize;
        int pz = (playerz) / ServerSimple.ChunkSize;

        int chunksxy = this.mapAreaSize() / ServerSimple.ChunkSize / 2;
        int chunksz = this.mapAreaSizeZ() / ServerSimple.ChunkSize / 2;

        int startx = px - chunksxy;
        int endx = px + chunksxy;
        int starty = py - chunksxy;
        int endy = py + chunksxy;
        int startz = pz - chunksz;
        int endz = pz + chunksz;

        if (startx < 0) { startx = 0; }
        if (starty < 0) { starty = 0; }
        if (startz < 0) { startz = 0; }
        if (endx >= mapsizexchunks()) { endx = mapsizexchunks() - 1; }
        if (endy >= mapsizeychunks()) { endy = mapsizeychunks() - 1; }
        if (endz >= mapsizezchunks()) { endz = mapsizezchunks() - 1; }

        ClientSimple client = server.clients[clientid];
        for (int x = startx; x <= endx; x++)
        {
            for (int y = starty; y <= endy; y++)
            {
                int pos = MapUtilCi.Index2d(x, y, server.MapSizeX / ServerSimple.ChunkSize);
                if (client.chunksseen[pos] == null)
                {
                    client.chunksseen[pos] = new bool[server.MapSizeZ / ServerSimple.ChunkSize];
                }
                for (int z = startz; z <= endz; z++)
                {
                    bool[] column = client.chunksseen[pos];
                    if (column[z])
                    {
                        continue;
                    }
                    {
                        int dx = px - x;
                        int dy = py - y;
                        int dz = pz - z;
                        int dist = dx * dx + dy * dy + dz * dz;
                        if (dist < nearestdist)
                        {
                            nearestdist = dist;
                            retNearest[0] = x;
                            retNearest[1] = y;
                            retNearest[2] = z;
                        }
                    }
                }
            }
        }
    }
}

public class ModServerSimple : ClientMod
{
    internal ServerSimple server;
    public override void OnReadOnlyBackgroundThread(Game game, float dt)
    {
        server.Update();
    }
}

public abstract class ModManagerSimple
{
    public abstract BlockTypeSimple CreateBlockType(string name);
    public abstract int GetBlockTypeId(string p);
    public abstract void AddToInventory(int player, string block, int amount);
}

public class ModManagerSimple1 : ModManagerSimple
{
    ServerSimple server;
    public override BlockTypeSimple CreateBlockType(string name)
    {
        BlockTypeSimple b = new BlockTypeSimple();
        b.SetName(name);
        server.blockTypes[server.blockTypesCount++] = b.block;
        return b;
    }

    public void Start(ServerSimple serverSimple)
    {
        server = serverSimple;
    }

    public override int GetBlockTypeId(string p)
    {
        for (int i = 0; i < server.blockTypesCount; i++)
        {
            if (server.blockTypes[i] == null)
            {
                continue;
            }
            if (Game.StringEquals(server.blockTypes[i].Name, p))
            {
                return i;
            }
        }
        return -1;
    }

    public override void AddToInventory(int player, string block, int amount)
    {
        Packet_Inventory inv = server.clients[player].inventory;
        for (int i = 0; i < 10; i++)
        {
            if (inv.RightHand[i].BlockId == 0)
            {
                inv.RightHand[i].BlockId = GetBlockTypeId(block);
                inv.RightHand[i].BlockCount = amount;
                break;
            }
        }
        // todo main inventory
        server.clients[player].inventoryDirty = true;
    }
}

public abstract class ModSimple
{
    public abstract void Start(ModManagerSimple manager);
    public virtual void GenerateChunk(int cx, int cy, int cz, int[] chunk) { }
    public virtual int GetHeight() { return -1; }
    public virtual void OnPlayerJoin(int playerId) { }
}

public class BlockTypeSimple
{
    public BlockTypeSimple()
    {
        block = new Packet_BlockType();
    }
    internal Packet_BlockType block;
    public void SetAllTextures(string texture)
    {
        block.TextureIdTop = texture;
        block.TextureIdBottom = texture;
        block.TextureIdFront = texture;
        block.TextureIdBack = texture;
        block.TextureIdLeft = texture;
        block.TextureIdRight = texture;
        block.TextureIdForInventory = texture;
    }
    public void SetDrawType(int p) { block.DrawType = p; }
    public void SetWalkableType(int p) { block.WalkableType = p; }
    public void SetName(string name) { block.Name = name; }
    public void SetTextureTop(string p) { block.TextureIdTop = p; }
    public void SetTextureBack(string p) { block.TextureIdBack = p; }
    public void SetTextureFront(string p) { block.TextureIdFront = p; }
    public void SetTextureLeft(string p) { block.TextureIdLeft = p; }
    public void SetTextureRight(string p) { block.TextureIdRight = p; }
    public void SetTextureBottom(string p) { block.TextureIdBottom = p; }
}

public class ModSimpleDefault : ModSimple
{
    public override void Start(ModManagerSimple manager)
    {
        m = manager;
        BlockTypeSimple empty = manager.CreateBlockType("Empty");
        empty.SetDrawType(Packet_DrawTypeEnum.Empty);
        empty.SetWalkableType(Packet_WalkableTypeEnum.Empty);

        BlockTypeSimple stone = manager.CreateBlockType("Stone");
        stone.SetDrawType(Packet_DrawTypeEnum.Solid);
        stone.SetWalkableType(Packet_WalkableTypeEnum.Solid);
        stone.SetAllTextures("Stone");

        BlockTypeSimple dirt = manager.CreateBlockType("Dirt");
        dirt.SetDrawType(Packet_DrawTypeEnum.Solid);
        dirt.SetWalkableType(Packet_WalkableTypeEnum.Solid);
        dirt.SetAllTextures("Dirt");

        BlockTypeSimple grass = manager.CreateBlockType("Grass");
        grass.SetDrawType(Packet_DrawTypeEnum.Solid);
        grass.SetWalkableType(Packet_WalkableTypeEnum.Solid);
        grass.SetTextureTop("Grass");
        grass.SetTextureBack("GrassSide");
        grass.SetTextureFront("GrassSide");
        grass.SetTextureLeft("GrassSide");
        grass.SetTextureRight("GrassSide");
        grass.SetTextureBottom("Dirt");

        BlockTypeSimple wood = manager.CreateBlockType("Wood");
        wood.SetDrawType(Packet_DrawTypeEnum.Solid);
        wood.SetWalkableType(Packet_WalkableTypeEnum.Solid);
        wood.SetAllTextures("OakWood");

        BlockTypeSimple brick = manager.CreateBlockType("Brick");
        brick.SetDrawType(Packet_DrawTypeEnum.Solid);
        brick.SetWalkableType(Packet_WalkableTypeEnum.Solid);
        brick.SetAllTextures("Brick");

        // special
        manager.CreateBlockType("Sponge");
        manager.CreateBlockType("Trampoline");
        BlockTypeSimple adminium = manager.CreateBlockType("Adminium");
        adminium.SetDrawType(Packet_DrawTypeEnum.Solid);
        adminium.SetWalkableType(Packet_WalkableTypeEnum.Solid);
        adminium.SetAllTextures("Adminium");
        manager.CreateBlockType("Compass");
        manager.CreateBlockType("Ladder");
        manager.CreateBlockType("EmptyHand");
        manager.CreateBlockType("CraftingTable");
        manager.CreateBlockType("Lava");
        manager.CreateBlockType("StationaryLava");
        manager.CreateBlockType("FillStart");
        manager.CreateBlockType("Cuboid");
        manager.CreateBlockType("FillArea");
        manager.CreateBlockType("Minecart");
        manager.CreateBlockType("Rail0");
    }

    ModManagerSimple m;

    public override void OnPlayerJoin(int playerId)
    {
        m.AddToInventory(playerId, "Dirt", 0);
        m.AddToInventory(playerId, "Stone", 0);
        m.AddToInventory(playerId, "Wood", 0);
        m.AddToInventory(playerId, "Brick", 0);
    }
}

public class ModSimpleWorldGenerator : ModSimple
{
    public override void Start(ModManagerSimple manager)
    {
        m = manager;
    }
    ModManagerSimple m;

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    public override void GenerateChunk(int cx, int cy, int cz, int[] chunk)
    {
        int grass = m.GetBlockTypeId("Grass");
        int dirt = m.GetBlockTypeId("Dirt");
        int stone = m.GetBlockTypeId("Stone");
        int adminium = m.GetBlockTypeId("Adminium");
        for (int xx = 0; xx < 32; xx++)
        {
            for (int yy = 0; yy < 32; yy++)
            {
                for (int zz = 0; zz < 32; zz++)
                {
                    int z = cz * ServerSimple.ChunkSize + zz;
                    int height = 32;
                    int block = 0;
                    if (z > height) { block = 0; }
                    else if (z == height) { block = grass; }
                    else if (z > height - 5) { block = dirt; }
                    else { block = stone; }
                    if (z == 0) { block = adminium; }
                    chunk[Index3d(xx, yy, zz, ServerSimple.ChunkSize, ServerSimple.ChunkSize)] = block;
                }
            }
        }
    }

    public override int GetHeight()
    {
        return 33;
    }
}

public class ClientSimple
{
    public ClientSimple()
    {
        inventory = new Packet_Inventory();
        inventory.SetRightHand(new Packet_Item[10], 10, 10);
        for (int i = 0; i < 10; i++)
        {
            inventory.RightHand[i] = new Packet_Item();
        }
    }
    internal string Name;
    internal NetConnection Connection;
    internal NetServer MainSocket;
    internal bool[][] chunksseen;
    internal Action_ notifyMapAction;
    internal float glX;
    internal float glY;
    internal float glZ;
    internal bool connected;
    internal Packet_Inventory inventory;
    internal bool inventoryDirty;
    internal int pingLastMilliseconds;
}

public class ChunkSimple
{
    int[] data;
}
