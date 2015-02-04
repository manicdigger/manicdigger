public class ClientPackets
{
    public static Packet_Client CreateLoginPacket(GamePlatform platform, string username, string verificationKey)
    {
        Packet_ClientIdentification p = new Packet_ClientIdentification();
        {
            p.Username = username;
            p.MdProtocolVersion = platform.GetGameVersion();
            p.VerificationKey = verificationKey;
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.PlayerIdentification;
        pp.Identification = p;
        return pp;
    }

    public static Packet_Client CreateLoginPacket_(GamePlatform platform, string username, string verificationKey, string serverPassword)
    {
        Packet_ClientIdentification p = new Packet_ClientIdentification();
        {
            p.Username = username;
            p.MdProtocolVersion = platform.GetGameVersion();
            p.VerificationKey = verificationKey;
            p.ServerPassword = serverPassword;
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.PlayerIdentification;
        pp.Identification = p;
        return pp;
    }

    public static Packet_Client Oxygen(int currentOxygen)
    {
        Packet_Client packet = new Packet_Client();
        packet.Id = Packet_ClientIdEnum.Oxygen;
        packet.Oxygen = new Packet_ClientOxygen();
        packet.Oxygen.CurrentOxygen = currentOxygen;
        return packet;
    }

    public static Packet_Client Reload()
    {
        Packet_Client p = new Packet_Client();
        p.Id = Packet_ClientIdEnum.Reload;
        p.Reload = new Packet_ClientReload();
        return p;
    }

    public static Packet_Client Chat(string s, int isTeamchat)
    {
        Packet_ClientMessage p = new Packet_ClientMessage();
        p.Message = s;
        p.IsTeamchat = isTeamchat;
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.Message;
        pp.Message = p;
        return pp;
    }

    public static Packet_Client PingReply()
    {
        Packet_ClientPingReply p = new Packet_ClientPingReply();
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.PingReply;
        pp.PingReply = p;
        return pp;
    }

    public static Packet_Client SetBlock(int x, int y, int z, int mode, int type, int materialslot)
    {
        Packet_ClientSetBlock p = new Packet_ClientSetBlock();
        {
            p.X = x;
            p.Y = y;
            p.Z = z;
            p.Mode = mode;
            p.BlockType = type;
            p.MaterialSlot = materialslot;
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.SetBlock;
        pp.SetBlock = p;
        return pp;
    }

    public static Packet_Client SpecialKeyRespawn()
    {
        Packet_Client p = new Packet_Client();
        {
            p.Id = Packet_ClientIdEnum.SpecialKey;
            p.SpecialKey_ = new Packet_ClientSpecialKey();
            p.SpecialKey_.Key_ = Packet_SpecialKeyEnum.Respawn;
        }
        return p;
    }

    public static Packet_Client FillArea(int startx, int starty, int startz, int endx, int endy, int endz, int blockType, int ActiveMaterial)
    {
        Packet_ClientFillArea p = new Packet_ClientFillArea();
        {
            p.X1 = startx;
            p.Y1 = starty;
            p.Z1 = startz;
            p.X2 = endx;
            p.Y2 = endy;
            p.Z2 = endz;
            p.BlockType = blockType;
            p.MaterialSlot = ActiveMaterial;
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.FillArea;
        pp.FillArea = p;
        return pp;
    }

    public static Packet_Client InventoryClick(Packet_InventoryPosition pos)
    {
        Packet_ClientInventoryAction p = new Packet_ClientInventoryAction();
        p.A = pos;
        p.Action = Packet_InventoryActionTypeEnum.Click;
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.InventoryAction;
        pp.InventoryAction = p;
        return pp;
    }

    public static Packet_Client WearItem(Packet_InventoryPosition from, Packet_InventoryPosition to)
    {
        Packet_ClientInventoryAction p = new Packet_ClientInventoryAction();
        p.A = from;
        p.B = to;
        p.Action = Packet_InventoryActionTypeEnum.WearItem;
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.InventoryAction;
        pp.InventoryAction = p;
        return pp;
    }

    public static Packet_Client MoveToInventory(Packet_InventoryPosition from)
    {
        Packet_ClientInventoryAction p = new Packet_ClientInventoryAction();
        p.A = from;
        p.Action = Packet_InventoryActionTypeEnum.MoveToInventory;
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.InventoryAction;
        pp.InventoryAction = p;
        return pp;
    }

    public static Packet_Client Death(int reason, int sourcePlayer)
    {
        Packet_Client p = new Packet_Client();
        p.Id = Packet_ClientIdEnum.Death;
        p.Death = new Packet_ClientDeath();
        {
            p.Death.Reason = reason;
            p.Death.SourcePlayer = sourcePlayer;
        }
        return p;
    }

    public static Packet_Client Health(int currentHealth)
    {
        Packet_Client p = new Packet_Client();
        {
            p.Id = Packet_ClientIdEnum.Health;
            p.Health = new Packet_ClientHealth();
            p.Health.CurrentHealth = currentHealth;
        }
        return p;
    }

    public static Packet_Client RequestBlob(Game game, string[] required, int requiredCount)
    {
        Packet_ClientRequestBlob p = new Packet_ClientRequestBlob(); //{ RequestBlobMd5 = needed };
        if (GameVersionHelper.ServerVersionAtLeast(game.platform, game.serverGameVersion, 2014, 4, 13))
        {
            p.RequestedMd5 = new Packet_StringList();
            p.RequestedMd5.SetItems(required, requiredCount, requiredCount);
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.RequestBlob;
        pp.RequestBlob = p;
        return pp;
    }

    public static Packet_Client Leave(int reason)
    {
        Packet_Client p = new Packet_Client();
        p.Id = Packet_ClientIdEnum.Leave;
        p.Leave = new Packet_ClientLeave();
        p.Leave.Reason = reason;
        return p;
    }

    public static Packet_Client Craft(int x, int y, int z, int recipeId)
    {
        Packet_ClientCraft cmd = new Packet_ClientCraft();
        cmd.X = x;
        cmd.Y = y;
        cmd.Z = z;
        cmd.RecipeId = recipeId;
        Packet_Client p = new Packet_Client();
        p.Id = Packet_ClientIdEnum.Craft;
        p.Craft = cmd;
        return p;
    }

    public static Packet_Client DialogClick(string widgetId, string[] textValues, int textValuesCount)
    {
        Packet_Client p = new Packet_Client();
        p.Id = Packet_ClientIdEnum.DialogClick;
        p.DialogClick_ = new Packet_ClientDialogClick();
        p.DialogClick_.WidgetId = widgetId;
        p.DialogClick_.SetTextBoxValue(textValues, textValuesCount, textValuesCount);
        return p;
    }

    public static Packet_Client GameResolution(int width, int height)
    {
        Packet_ClientGameResolution p = new Packet_ClientGameResolution();
        p.Width = width;
        p.Height = height;
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.GameResolution;
        pp.GameResolution = p;
        return pp;
    }

    public static Packet_Client SpecialKeyTabPlayerList()
    {
        Packet_Client p = new Packet_Client();
        p.Id = Packet_ClientIdEnum.SpecialKey;
        p.SpecialKey_ = new Packet_ClientSpecialKey();
        p.SpecialKey_.Key_ = Packet_SpecialKeyEnum.TabPlayerList;
        return p;
    }

    public static Packet_Client SpecialKeySelectTeam()
    {
        Packet_Client p = new Packet_Client();
        {
            p.Id = Packet_ClientIdEnum.SpecialKey;
            p.SpecialKey_ = new Packet_ClientSpecialKey();
            p.SpecialKey_.Key_ = Packet_SpecialKeyEnum.SelectTeam;
        }
        return p;
    }

    public static Packet_Client SpecialKeySetSpawn()
    {
        Packet_Client p = new Packet_Client();
        {
            p.Id = Packet_ClientIdEnum.SpecialKey;
            p.SpecialKey_ = new Packet_ClientSpecialKey();
            p.SpecialKey_.Key_ = Packet_SpecialKeyEnum.SetSpawn;
        }
        return p;
    }

    public static Packet_Client ActiveMaterialSlot(int ActiveMaterial)
    {
        Packet_Client p = new Packet_Client();
        {
            p.Id = Packet_ClientIdEnum.ActiveMaterialSlot;
            p.ActiveMaterialSlot = new Packet_ClientActiveMaterialSlot();
            p.ActiveMaterialSlot.ActiveMaterialSlot = ActiveMaterial;
        }
        return p;
    }

    public static Packet_Client MonsterHit(int damage)
    {
        Packet_ClientHealth p = new Packet_ClientHealth();
        p.CurrentHealth = damage;
        Packet_Client packet = new Packet_Client();
        packet.Id = Packet_ClientIdEnum.MonsterHit;
        packet.Health = p;
        return packet;
    }

    public static Packet_Client PositionAndOrientation(Game game, int playerId, float positionX, float positionY, float positionZ, float orientationX, float orientationY, float orientationZ, byte stance)
    {
        Packet_ClientPositionAndOrientation p = new Packet_ClientPositionAndOrientation();
        {
            p.PlayerId = playerId;
            p.X = game.platform.FloatToInt(positionX * 32);
            p.Y = game.platform.FloatToInt(positionY * 32);
            p.Z = game.platform.FloatToInt(positionZ * 32);
            p.Heading = game.platform.FloatToInt(Game.RadToAngle256(orientationY));
            p.Pitch = game.platform.FloatToInt(Game.RadToAngle256(orientationX));
            p.Stance = stance;
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.PositionandOrientation;
        pp.PositionAndOrientation = p;
        return pp;
    }

    public static Packet_Client ServerQuery()
    {
        Packet_ClientServerQuery p1 = new Packet_ClientServerQuery();
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.ServerQuery;
        pp.Query = p1;
        return pp;
    }

    internal static Packet_Client UseEntity(int entityId)
    {
        Packet_Client p = new Packet_Client();
        p.Id = Packet_ClientIdEnum.EntityInteraction;
        p.EntityInteraction = new Packet_ClientEntityInteraction();
        p.EntityInteraction.EntityId = entityId;
        p.EntityInteraction.InteractionType = Packet_EntityInteractionTypeEnum.Use;
        return p;
    }

    internal static Packet_Client HitEntity(int entityId)
    {
        Packet_Client p = new Packet_Client();
        p.Id = Packet_ClientIdEnum.EntityInteraction;
        p.EntityInteraction = new Packet_ClientEntityInteraction();
        p.EntityInteraction.EntityId = entityId;
        p.EntityInteraction.InteractionType = Packet_EntityInteractionTypeEnum.Hit;
        return p;
    }
}

public class ServerPackets
{
    public static Packet_Server Message(string text)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.Message;
        p.Message = new Packet_ServerMessage();
        p.Message.Message = text;
        return p;
    }

    public static Packet_Server LevelInitialize()
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.LevelInitialize;
        p.LevelInitialize = new Packet_ServerLevelInitialize();
        return p;
    }

    public static Packet_Server LevelFinalize()
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.LevelFinalize;
        p.LevelFinalize = new Packet_ServerLevelFinalize();
        return p;
    }

    public static Packet_Server Identification(int assignedClientId, int mapSizeX, int mapSizeY, int mapSizeZ, string version)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.ServerIdentification;
        p.Identification = new Packet_ServerIdentification();
        p.Identification.AssignedClientId = assignedClientId;
        p.Identification.MapSizeX = mapSizeX;
        p.Identification.MapSizeY = mapSizeY;
        p.Identification.MapSizeZ = mapSizeZ;
        p.Identification.ServerName = "Simple";
        p.Identification.MdProtocolVersion = version;
        return p;
    }
    public static byte[] Serialize(Packet_Server packet, IntRef retLength)
    {
        CitoMemoryStream ms = new CitoMemoryStream();
        Packet_ServerSerializer.Serialize(ms, packet);
        byte[] data = ms.ToArray();
        retLength.value = ms.Length();
        return data;
    }

    public static Packet_Server BlockType(int id, Packet_BlockType blockType)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.BlockType;
        p.BlockType = new Packet_ServerBlockType();
        p.BlockType.Id = id;
        p.BlockType.Blocktype = blockType;
        return p;
    }

    public static Packet_Server BlockTypes()
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.BlockTypes;
        p.BlockTypes = new Packet_ServerBlockTypes();
        return p;
    }

    public static Packet_Server Chunk_(int x, int y, int z, int chunksize)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.Chunk_;
        p.Chunk_ = new Packet_ServerChunk();
        p.Chunk_.X = x;
        p.Chunk_.Y = y;
        p.Chunk_.Z = z;
        p.Chunk_.SizeX = chunksize;
        p.Chunk_.SizeY = chunksize;
        p.Chunk_.SizeZ = chunksize;
        return p;
    }

    public static Packet_Server ChunkPart(byte[] compressedChunkPart)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.ChunkPart;
        p.ChunkPart = new Packet_ServerChunkPart();
        p.ChunkPart.CompressedChunkPart = compressedChunkPart;
        return p;
    }

    internal static Packet_Server SetBlock(int x, int y, int z, int block)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.SetBlock;
        p.SetBlock = new Packet_ServerSetBlock();
        p.SetBlock.X = x;
        p.SetBlock.Y = y;
        p.SetBlock.Z = z;
        p.SetBlock.BlockType = block;
        return p;
    }

    internal static Packet_Server PlayerStats(int health, int maxHealth, int oxygen, int maxOxygen)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.PlayerStats;
        p.PlayerStats = new Packet_ServerPlayerStats();
        p.PlayerStats.CurrentHealth = health;
        p.PlayerStats.MaxHealth = maxHealth;
        p.PlayerStats.CurrentOxygen = oxygen;
        p.PlayerStats.MaxOxygen = maxOxygen;
        return p;
    }

    internal static Packet_Server Inventory(Packet_Inventory inventory)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.FiniteInventory;
        p.Inventory = new Packet_ServerInventory();
        p.Inventory.Inventory = inventory;
        return p;
    }

    internal static Packet_Server Ping()
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.Ping;
        p.Ping = new Packet_ServerPing();
        return p;
    }

    internal static Packet_Server DisconnectPlayer(string disconnectReason)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.DisconnectPlayer;
        p.DisconnectPlayer = new Packet_ServerDisconnectPlayer();
        p.DisconnectPlayer.DisconnectReason = disconnectReason;
        return p;
    }

    internal static Packet_Server AnswerQuery(Packet_ServerQueryAnswer answer)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.QueryAnswer;
        p.QueryAnswer = answer;
        return p;
    }

    internal static Packet_Server EntitySpawn(int id, Packet_ServerEntity entity)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.EntitySpawn;
        p.EntitySpawn = new Packet_ServerEntitySpawn();
        p.EntitySpawn.Id = id;
        p.EntitySpawn.Entity_ = entity;
        return p;
    }

    internal static Packet_Server EntityPositionAndOrientation(int id, Packet_PositionAndOrientation positionAndOrientation)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.EntityPosition;
        p.EntityPosition = new Packet_ServerEntityPositionAndOrientation();
        p.EntityPosition.Id = id;
        p.EntityPosition.PositionAndOrientation = positionAndOrientation;
        return p;
    }

    internal static Packet_Server EntityDespawn(int id)
    {
        Packet_Server p = new Packet_Server();
        p.Id = Packet_ServerIdEnum.EntityDespawn;
        p.EntityDespawn = new Packet_ServerEntityDespawn();
        p.EntityDespawn.Id = id;
        return p;
    }
}
