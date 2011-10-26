using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using ManicDigger;

namespace GameModeFortress
{
    [ProtoContract]
    public class PacketClientIdentification
    {
        [ProtoMember(1, IsRequired = false)]
        public string MdProtocolVersion;
        [ProtoMember(2, IsRequired = false)]
        public string Username;
        [ProtoMember(3, IsRequired = false)]
        public string VerificationKey;
    }
    [ProtoContract]
    public class PacketClientRequestBlob
    {
        [ProtoMember(1, IsRequired = false)]
        public List<byte[]> RequestBlobMd5; //todo, currently ignored.
    }
    [ProtoContract]
    public class PacketClientSetBlock
    {
        [ProtoMember(1, IsRequired = false)]
        public int X;
        [ProtoMember(2, IsRequired = false)]
        public int Y;
        [ProtoMember(3, IsRequired = false)]
        public int Z;
        [ProtoMember(4, IsRequired = false)]
        public BlockSetMode Mode;
        [ProtoMember(5, IsRequired = false)]
        [Obsolete]
        public int BlockType;
        [ProtoMember(6, IsRequired = false)]
        public int MaterialSlot;
    }
    [ProtoContract]
    public class PacketClientPositionAndOrientation
    {
        [ProtoMember(1, IsRequired = false)]
        public int PlayerId;
        [ProtoMember(2, IsRequired = false)]
        public int X;
        [ProtoMember(3, IsRequired = false)]
        public int Y;
        [ProtoMember(4, IsRequired = false)]
        public int Z;
        [ProtoMember(5, IsRequired = false)]
        public byte Heading;
        [ProtoMember(6, IsRequired = false)]
        public byte Pitch;
    }
    [ProtoContract]
    public class PacketClientMessage
    {
        [ProtoMember(1, IsRequired = false)]
        public string Message;
    }
    public enum InventoryActionType
    {
        Click,
        WearItem,
        MoveToInventory,
    }
    [ProtoContract]
    public class PacketClientInventoryAction
    {
        [ProtoMember(1, IsRequired = false)]
        public InventoryActionType Action;
        [ProtoMember(2, IsRequired = false)]
        public InventoryPosition A;
        [ProtoMember(3, IsRequired = false)]
        public InventoryPosition B;
    }
    [ProtoContract]
    public class PacketServerIdentification
    {
        [ProtoMember(1, IsRequired = false)]
        public string MdProtocolVersion;
        [ProtoMember(2, IsRequired = false)]
        public string ServerName;
        [ProtoMember(3, IsRequired = false)]
        public string ServerMotd;
        [ProtoMember(4, IsRequired = false)]
        public List<byte[]> UsedBlobsMd5; //todo, currently ignored.
        [ProtoMember(5, IsRequired = false)]
        public byte[] TerrainTextureMd5; //todo, currently ignored.
        [ProtoMember(6, IsRequired = false)]
        public bool DisallowFreemove;
        [ProtoMember(7, IsRequired = false)]
        public int MapSizeX = 10000;
        [ProtoMember(8, IsRequired = false)]
        public int MapSizeY = 10000;
        [ProtoMember(9, IsRequired = false)]
        public int MapSizeZ = 128;
    }
    //public class PacketServerPing
    [ProtoContract]
    public class PacketServerLevelInitialize
    {
    }
    [ProtoContract]
    public class PacketServerBlobInitialize
    {
        [ProtoMember(1, IsRequired = false)]
        public byte[] hash; //todo, currently ignored.
        [ProtoMember(2, IsRequired = false)]
        public string name;
    }
    [ProtoContract]
    public class PacketServerBlobPart
    {
        [ProtoMember(1, IsRequired = false)]
        public byte[] data;
        [ProtoMember(2, IsRequired = false)]
        public bool lastpart;
    }
    [ProtoContract]
    public class PacketServerBlobFinalize
    {
    }
    [ProtoContract]
    public class PacketServerLevelProgress
    {
        //[ProtoMember(1, IsRequired = false)]
        //public byte[] Chunk;
        [ProtoMember(2, IsRequired = false)]
        public int PercentComplete;
        [ProtoMember(3, IsRequired = false)]
        public string Status;
        [ProtoMember(4, IsRequired = false)]
        public int PercentCompleteSubitem;
    }
    [ProtoContract]
    public class PacketServerLevelFinalize
    {
    }
    [ProtoContract]
    public class PacketServerSetBlock
    {
        [ProtoMember(1, IsRequired = false)]
        public int X;
        [ProtoMember(2, IsRequired = false)]
        public int Y;
        [ProtoMember(3, IsRequired = false)]
        public int Z;
        [ProtoMember(4, IsRequired = false)]
        public int BlockType;
    }
    [ProtoContract]
    public class PacketServerSpawnPlayer
    {
        [ProtoMember(1, IsRequired = false)]
        public int PlayerId;
        [ProtoMember(2, IsRequired = false)]
        public string PlayerName;
        [ProtoMember(3, IsRequired = false)]
        public PositionAndOrientation PositionAndOrientation;
    }
    [ProtoContract]
    public class PositionAndOrientation
    {
        [ProtoMember(1, IsRequired = false)]
        public int X;
        [ProtoMember(2, IsRequired = false)]
        public int Y;
        [ProtoMember(3, IsRequired = false)]
        public int Z;
        [ProtoMember(4, IsRequired = false)]
        public byte Heading;
        [ProtoMember(5, IsRequired = false)]
        public byte Pitch;
    }
    [ProtoContract]
    public class PacketServerPositionAndOrientation
    {
        [ProtoMember(1, IsRequired = false)]
        public int PlayerId;
        [ProtoMember(2, IsRequired = false)]
        public PositionAndOrientation PositionAndOrientation;
    }
    [ProtoContract]
    public class PacketServerMessage
    {
        [ProtoMember(1, IsRequired = false)]
        public int PlayerId;
        [ProtoMember(2, IsRequired = false)]
        public string Message;
    }
    [ProtoContract]
    public class PacketServerDespawnPlayer
    {
        [ProtoMember(1, IsRequired = false)]
        public int PlayerId;
    }
    [ProtoContract]
    public class PacketServerDisconnectPlayer
    {
        [ProtoMember(1, IsRequired = false)]
        public string DisconnectReason;
    }
    [ProtoContract]
    public class PacketServerSound
    {
        [ProtoMember(1, IsRequired = false)]
        public string Name;
    }
    [ProtoContract]
    public class PacketServer
    {
        [ProtoMember(90, IsRequired = false)]
        public ServerPacketId PacketId;
        [ProtoMember(1, IsRequired = false)]
        public PacketServerIdentification Identification;
        //1 ping
        [ProtoMember(2, IsRequired = false)]
        public PacketServerLevelInitialize LevelInitialize;
        [ProtoMember(3, IsRequired = false)]
        public PacketServerLevelProgress LevelDataChunk;
        [ProtoMember(4, IsRequired = false)]
        public PacketServerLevelFinalize LevelFinalize;
        [ProtoMember(5, IsRequired = false)]
        public PacketServerSetBlock SetBlock;
        [ProtoMember(6, IsRequired = false)]
        public PacketServerSpawnPlayer SpawnPlayer;
        [ProtoMember(7, IsRequired = false)]
        public PacketServerPositionAndOrientation PositionAndOrientation;
        [ProtoMember(8, IsRequired = false)]
        public PacketServerDespawnPlayer DespawnPlayer;
        [ProtoMember(9, IsRequired = false)]
        public PacketServerMessage Message;
        [ProtoMember(10, IsRequired = false)]
        public PacketServerDisconnectPlayer DisconnectPlayer;
        [ProtoMember(11, IsRequired = false)]
        public PacketServerChunk Chunk;
        [ProtoMember(12, IsRequired = false)]
        public PacketServerInventory Inventory;
        [ProtoMember(13, IsRequired = false)]
        public PacketServerSeason Season;
        [ProtoMember(14, IsRequired = false)]
        public PacketServerBlobInitialize BlobInitialize;
        [ProtoMember(15, IsRequired = false)]
        public PacketServerBlobPart BlobPart;
        [ProtoMember(16, IsRequired = false)]
        public PacketServerBlobFinalize BlobFinalize;
        [ProtoMember(17, IsRequired = false)]
        public PacketServerHeightmapChunk HeightmapChunk;
        [ProtoMember(18, IsRequired = false)]
        public PacketServerPing Ping;
        [ProtoMember(19, IsRequired = false)]
        public PacketServerSound Sound;
        [ProtoMember(20, IsRequired = false)]
        public PacketServerPlayerStats PlayerStats;
        [ProtoMember(21, IsRequired = false)]
        public PacketServerMonsters Monster;
    }
    [ProtoContract]
    public class PacketClient
    {
        [ProtoMember(1, IsRequired = false)]
        public ClientPacketId PacketId;
        [ProtoMember(2, IsRequired = false)]
        public PacketClientIdentification Identification;
        [ProtoMember(3, IsRequired = false)]
        public PacketClientSetBlock SetBlock;
        [ProtoMember(4, IsRequired = false)]
        public PacketClientPositionAndOrientation PositionAndOrientation;
        [ProtoMember(5, IsRequired = false)]
        public PacketClientMessage Message;
        [ProtoMember(6, IsRequired = false)]
        public PacketClientCraft Craft;
        [ProtoMember(7, IsRequired = false)]
        public PacketClientRequestBlob RequestBlob;
        [ProtoMember(8, IsRequired = false)]
        public PacketClientInventoryAction InventoryAction;
        [ProtoMember(9, IsRequired = false)]
        public PacketClientHealth Health;
    }
    [ProtoContract]
    public class PacketServerChunk
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
    }
    //needed for drawing shadows.
    //sent before any chunks or blocks in the column.
    [ProtoContract]
    public class PacketServerHeightmapChunk
    {
        [ProtoMember(1, IsRequired = false)]
        public int X;
        [ProtoMember(2, IsRequired = false)]
        public int Y;
        [ProtoMember(3, IsRequired = false)]
        public int SizeX;
        [ProtoMember(4, IsRequired = false)]
        public int SizeY;
        [ProtoMember(5, IsRequired = false)]
        public byte[] CompressedHeightmap;
    }
    [ProtoContract]
    public class PacketServerInventory
    {
        /*
        [ProtoMember(1, IsRequired = false)]
        public bool IsFinite;
        [ProtoMember(2, IsRequired = false)]
        public Dictionary<int, int> BlockTypeAmount = new Dictionary<int, int>();
        [ProtoMember(3, IsRequired = false)]
        public int Max = 200;
        */
        [ProtoMember(4, IsRequired = false)]
        public ManicDigger.Inventory Inventory;
    }
    [ProtoContract]
    public class PacketServerPlayerStats
    {
        [ProtoMember(1, IsRequired = false)]
        public int CurrentHealth = 20;
        [ProtoMember(2, IsRequired = false)]
        public int MaxHealth = 20;
    }
    [ProtoContract]
    public class PacketServerMonsters
    {
        [ProtoMember(1, IsRequired = false)]
        public PacketServerMonster[] Monsters;
    }
    [ProtoContract]
    public class PacketServerMonster
    {
        [ProtoMember(1, IsRequired = false)]
        public int Id;
        [ProtoMember(2, IsRequired = false)]
        public int MonsterType;
        [ProtoMember(3, IsRequired = false)]
        public PositionAndOrientation PositionAndOrientation;
    }
    //Temporary, for client-side health.
    //Todo fix because it allows cheating.
    [ProtoContract]
    public class PacketClientHealth
    {
        [ProtoMember(1, IsRequired = false)]
        public int CurrentHealth;
    }
    [ProtoContract]
    public class PacketServerSeason
    {
        [ProtoMember(1, IsRequired = false)]
        public int Season;
        [ProtoMember(2, IsRequired = false)]
        public int Hour; //1-24*4
        [ProtoMember(3, IsRequired = false)]
        public int DayNightCycleSpeedup = 24; //used for predicting sun speed.
        [ProtoMember(4, IsRequired = false)]
        public int Moon;
    }
    [ProtoContract]
    public class PacketServerPing
    {
    }
    [ProtoContract]
    public class PacketClientCraft
    {
        [ProtoMember(1, IsRequired = false)]
        public int X;
        [ProtoMember(2, IsRequired = false)]
        public int Y;
        [ProtoMember(3, IsRequired = false)]
        public int Z;
        [ProtoMember(4, IsRequired = false)]
        public int RecipeId;
    }
    /// <summary>
    /// Client -> Server packet id.
    /// </summary>
    public enum ClientPacketId
    {
        PlayerIdentification = 0,
        SetBlock = 5,
        PositionandOrientation = 8,
        Craft = 9,
        Message = 0x0d,
        RequestBlob = 50,
        InventoryAction = 51,
        Health = 52,
        ExtendedPacketCommand = 100,
    }
    /// <summary>
    /// Server -> Client packet id.
    /// </summary>
    public enum ServerPacketId
    {
        ServerIdentification = 0,
        Ping = 1,
        LevelInitialize = 2,
        LevelDataChunk = 3,
        LevelFinalize = 4,
        SetBlock = 6,
        SpawnPlayer = 7,
        PlayerPositionAndOrientation = 8,
        PositionUpdate = 10,
        OrientationUpdate = 11,
        DespawnPlayer = 12,
        Message = 13,
        DisconnectPlayer = 14,
        Chunk = 15,
        FiniteInventory = 16,
        Season = 17,
        BlobInitialize = 18,
        BlobPart = 19,
        BlobFinalize = 20,
        HeightmapChunk = 21,
        Sound = 22,
        PlayerStats = 23,
        Monster = 24,
        ActiveMonsters = 25,
        
        RemoveMonsters = 50,

        ExtendedPacketCommand = 100,
        ExtendedPacketTick = 101,
    }
}
