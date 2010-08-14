using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

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
    public class PacketClientSetBlock
    {
        [ProtoMember(1, IsRequired = false)]
        public int X;
        [ProtoMember(2, IsRequired = false)]
        public int Y;
        [ProtoMember(3, IsRequired = false)]
        public int Z;
        [ProtoMember(4, IsRequired = false)]
        public byte Mode;
        [ProtoMember(5, IsRequired = false)]
        public int BlockType;
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
    [ProtoContract]
    public class PacketServerIdentification
    {
        [ProtoMember(1, IsRequired = false)]
        public string MdProtocolVersion;
        [ProtoMember(2, IsRequired = false)]
        public string ServerName;
        [ProtoMember(3, IsRequired = false)]
        public string ServerMotd;
    }
    //public class PacketServerPing
    [ProtoContract]
    public class PacketServerLevelInitialize
    {
    }
    [ProtoContract]
    public class PacketServerLevelDataChunk
    {
        [ProtoMember(1, IsRequired = false)]
        public byte[] Chunk;
        [ProtoMember(2, IsRequired = false)]
        public int PercentComplete;
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
        public PacketServerLevelDataChunk LevelDataChunk;
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
        public PacketServerFiniteInventory FiniteInventory;
        [ProtoMember(13, IsRequired = false)]
        public PacketServerSeason Season;
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
    [ProtoContract]
    public class PacketServerFiniteInventory
    {
        [ProtoMember(1, IsRequired = false)]
        public bool IsFinite;
        [ProtoMember(2, IsRequired = false)]
        public Dictionary<int, int> BlockTypeAmount = new Dictionary<int, int>();
        [ProtoMember(3, IsRequired = false)]
        public int Max = 200;
    }
    [ProtoContract]
    public class PacketServerSeason
    {
        [ProtoMember(1, IsRequired = false)]
        public int Season;
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

        ExtendedPacketCommand = 100,
        ExtendedPacketTick = 101,
    }
}
