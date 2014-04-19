public class ServerSimple
{
    public ServerSimple()
    {
        clients = new ClientSimple[256];
        clientsCount = 0;
        blockTypes = new Packet_BlockType[GlobalVar.MAX_BLOCKTYPES];
        blockTypesCount = 0;
        spawnX = 16;
        spawnY = 16;
        spawnZ = 16;

        ModManagerSimple1 m = new ModManagerSimple1();
        m.Start(this);
        ModSimpleDefault mod = new ModSimpleDefault();
        mod.Start(m);
    }
    INetServer server;
    string saveFilename;
    internal GamePlatform platform;
    public void Start(INetServer server_, string saveFilename_, GamePlatform platform_)
    {
        server = server_;
        saveFilename = saveFilename_;
        platform = platform_;
    }

    public void Update()
    {
        ProcessPackets();
    }

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    ClientSimple[] clients;
    int clientsCount;
    int spawnX;
    int spawnY;
    int spawnZ;

    void ProcessPackets()
    {
        for (; ; )
        {
            INetIncomingMessage msg = server.ReadMessage();
            if (msg == null)
            {
                return;
            }
            switch (msg.Type())
            {
                case NetworkMessageType.Connect:
                    ClientSimple c = new ClientSimple();
                    c.MainSocket = server;
                    c.Connection = msg.SenderConnection();
                    clients[0] = c;
                    clientsCount = 1;
                    break;
                case NetworkMessageType.Data:
                    byte[] data = msg.ReadBytes(msg.LengthBytes());
                    Packet_Client packet = new Packet_Client();
                    Packet_ClientSerializer.DeserializeBuffer(data, msg.LengthBytes(), packet);
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
                    SendPacket(client, ServerPackets.Identification(0, 256, 256, 128, platform.GetGameVersion()));
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
                    int[] chunk = new int[32 * 32 * 32];
                    for (int x = 0; x < 32; x++)
                    {
                        for (int y = 0; y < 32; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                chunk[Index3d(x, y, z, 32, 32)] = 1;
                            }
                        }
                    }
                    byte[] chunkBytes = MiscCi.UshortArrayToByteArray(chunk, 32 * 32 * 32);
                    IntRef compressedLength = new IntRef();
                    byte[] chunkCompressed = platform.GzipCompress(chunkBytes, 32 * 32 * 32 * 2, compressedLength);
                    SendPacket(client, ServerPackets.ChunkPart(chunkCompressed));
                    SendPacket(client, ServerPackets.Chunk_(0, 0, 0, 32));
                    SendPacket(client, ServerPackets.LevelFinalize());
                    for (int i = 0; i < clientsCount; i++)
                    {
                        if (clients[i] == null)
                        {
                            continue;
                        }
                        Packet_PositionAndOrientation pos = new Packet_PositionAndOrientation();
                        pos.X = 32 * spawnX;
                        pos.Y = 32 * spawnY;
                        pos.Z = 32 * spawnZ;
                        SendPacket(client, ServerPackets.Spawn(i, clients[i].Name, pos));
                    }
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
        }
    }

    void SendPacketToAll(Packet_Server packet)
    {
        for (int i = 0; i < clientsCount; i++)
        {
            SendPacket(i, packet);
        }
    }

    void SendPacket(int client, Packet_Server packet)
    {
        IntRef length = new IntRef();
        byte[] data = ServerPackets.Serialize(packet, length);
        INetOutgoingMessage msg = clients[client].MainSocket.CreateMessage();
        msg.Write(data, length.value);
        clients[client].Connection.SendMessage(msg, MyNetDeliveryMethod.ReliableOrdered, 0);
    }
    internal Packet_BlockType[] blockTypes;
    internal int blockTypesCount;
}

public class ServerSimpleRunner : Action_
{
    internal ServerSimple server;
    public override void Run()
    {
        server.Update();
        server.platform.QueueUserWorkItem(this);
    }
}

public abstract class ModManagerSimple
{
    public abstract BlockTypeSimple CreateBlockType(string name);
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
}

public abstract class ModSimple
{
    public abstract void Start(ModManagerSimple manager);
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

    public void SetDrawType(int p)
    {
        block.DrawType = p;
    }

    public void SetWalkableType(int p)
    {
        block.WalkableType = p;
    }

    internal void SetName(string name)
    {
        block.Name = name;
    }
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

        // special
        manager.CreateBlockType("Sponge");
        manager.CreateBlockType("Trampoline");
        manager.CreateBlockType("Adminium");
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
}

public class ClientSimple
{
    internal string Name;
    internal INetConnection Connection;
    internal INetServer MainSocket;
}

public class ChunkSimple
{
    int[] data;
}
