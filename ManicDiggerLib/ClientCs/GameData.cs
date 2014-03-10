using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace ManicDigger
{
    public interface IGameDataLight
    {
        bool[] IsTransparentForLight { get; }
        int[] LightRadius { get; }
    }
    public interface IGameDataPhysics
    {
        bool[] IsWater { get; }
    }
    public class SpecialBlockId
    {
        public const int Empty = 0;
    }
    public class GameData
    {
        public void Start()
        {
            Initialize(GlobalVar.MAX_BLOCKTYPES);
        }
        public void Update()
        {
        }
        private void Initialize(int count)
        {
            mWhenPlayerPlacesGetsConvertedTo = new int[count];
            mIsFlower = new bool[count];
            mRail = new RailDirectionFlags[count];
            mWalkSpeed = new float[count];
            for (int i = 0; i < count; i++)
            {
                mWalkSpeed[i] = 1;
            }
            mIsSlipperyWalk = new bool[count];
            mWalkSound = new string[count][];
            for (int i = 0; i < count; i++)
            {
                mWalkSound[i] = new string[0];
            }
            mBreakSound = new string[count][];
            for (int i = 0; i < count; i++)
            {
                mBreakSound[i] = new string[0];
            }
            mBuildSound = new string[count][];
            for (int i = 0; i < count; i++)
            {
                mBuildSound[i] = new string[0];
            }
            mCloneSound = new string[count][];
            for (int i = 0; i < count; i++)
            {
                mCloneSound[i] = new string[0];
            }
            mLightRadius = new int[count];
            mStartInventoryAmount = new int[count];
            mStrength = new float[count];
            mDamageToPlayer = new int[count];
            mWalkableType = new WalkableType[count];

            mDefaultMaterialSlots = new int[10];
        }

        public int[] WhenPlayerPlacesGetsConvertedTo { get { return mWhenPlayerPlacesGetsConvertedTo; } }
        public bool[] IsFlower { get { return mIsFlower; } }
        public RailDirectionFlags[] Rail { get { return mRail; } }
        public float[] WalkSpeed { get { return mWalkSpeed; } }
        public bool[] IsSlipperyWalk { get { return mIsSlipperyWalk; } }
        public string[][] WalkSound { get { return mWalkSound; } }
        public string[][] BreakSound { get { return mBreakSound; } }
        public string[][] BuildSound { get { return mBuildSound; } }
        public string[][] CloneSound { get { return mCloneSound; } }
        public int[] LightRadius { get { return mLightRadius; } }
        public int[] StartInventoryAmount { get { return mStartInventoryAmount; } }
        public float[] Strength { get { return mStrength; } }
        public int[] DamageToPlayer { get { return mDamageToPlayer; } }
        public WalkableType[] WalkableType1 { get { return mWalkableType; } }

        public int[] DefaultMaterialSlots { get { return mDefaultMaterialSlots; } }

        private int[] mWhenPlayerPlacesGetsConvertedTo;
        private bool[] mIsFlower;
        private RailDirectionFlags[] mRail;
        private float[] mWalkSpeed;
        private bool[] mIsSlipperyWalk;
        private string[][] mWalkSound;
        private string[][] mBreakSound;
        private string[][] mBuildSound;
        private string[][] mCloneSound;
        private int[] mLightRadius;
        private int[] mStartInventoryAmount;
        private float[] mStrength;
         private int[] mDamageToPlayer;
        private WalkableType[] mWalkableType;

        private int[] mDefaultMaterialSlots;

        // TODO: hardcoded IDs
        // few code sections still expect some hardcoded IDs
        private int mBlockIdEmpty = 0;
        private int mBlockIdDirt = -1;
        private int mBlockIdSponge = -1;
        private int mBlockIdTrampoline = -1;
        private int mBlockIdAdminium = -1;
        private int mBlockIdCompass = -1;
        private int mBlockIdLadder = -1;
        private int mBlockIdEmptyHand = -1;
        private int mBlockIdCraftingTable = -1;
        private int mBlockIdLava = -1;
        private int mBlockIdStationaryLava = -1;
        private int mBlockIdFillStart = -1;
        private int mBlockIdCuboid = -1;
        private int mBlockIdFillArea = -1;
        private int mBlockIdMinecart = -1;
        private int mBlockIdRailstart = -128; // 64 rail tiles

        public int BlockIdEmpty { get { return mBlockIdEmpty; } set { mBlockIdEmpty = value; } }
        public int BlockIdDirt { get { return mBlockIdDirt; } set { mBlockIdDirt = value; } }
        public int BlockIdSponge { get { return mBlockIdSponge; } set { mBlockIdSponge = value; } }
        public int BlockIdTrampoline { get { return mBlockIdTrampoline; } set { mBlockIdTrampoline = value; } }
        public int BlockIdAdminium { get { return mBlockIdAdminium; } set { mBlockIdAdminium = value; } }
        public int BlockIdCompass { get { return mBlockIdCompass; } set { mBlockIdCompass = value; } }
        public int BlockIdLadder { get { return mBlockIdLadder; } set { mBlockIdLadder = value; } }
        public int BlockIdEmptyHand { get { return mBlockIdEmptyHand; } set { mBlockIdEmptyHand = value; } }
        public int BlockIdCraftingTable { get { return mBlockIdCraftingTable; } set { mBlockIdCraftingTable = value; } }
        public int BlockIdLava { get { return mBlockIdLava; } set { mBlockIdLava = value; } }
        public int BlockIdStationaryLava { get { return mBlockIdStationaryLava; } set { mBlockIdStationaryLava = value; } }
        public int BlockIdFillStart { get { return mBlockIdFillStart; } set { mBlockIdFillStart = value; } }
        public int BlockIdCuboid { get { return mBlockIdCuboid; } set { mBlockIdCuboid = value; } }
        public int BlockIdFillArea { get { return mBlockIdFillArea; } set { mBlockIdFillArea = value; } }
        public int BlockIdMinecart { get { return mBlockIdMinecart; } set { mBlockIdMinecart = value; } }
        public int BlockIdRailstart { get { return mBlockIdRailstart; } set { mBlockIdRailstart = value; } }

        // TODO: atm it sets sepcial block id from block name - better use new block property
        public bool SetSpecialBlock(Packet_BlockType b, int id)
        {
            switch (b.Name)
            {
                case "Empty":
                    this.BlockIdEmpty = id;
                    return true;
                case "Dirt":
                    this.BlockIdDirt = id;
                    return true;
                case "Sponge":
                    this.BlockIdSponge = id;
                    return true;
                case "Trampoline":
                    this.BlockIdTrampoline = id;
                    return true;
                case "Adminium":
                    this.BlockIdAdminium = id;
                    return true;
                case "Compass":
                    this.BlockIdCompass = id;
                    return true;
                case "Ladder":
                    this.BlockIdLadder = id;
                    return true;
                case "EmptyHand":
                    this.BlockIdEmptyHand = id;
                    return true;
                case "CraftingTable":
                    this.BlockIdCraftingTable = id;
                    return true;
                case "Lava":
                    this.BlockIdLava = id;
                    return true;
                case "StationaryLava":
                    this.BlockIdStationaryLava = id;
                    return true;
                case "FillStart":
                    this.BlockIdFillStart = id;
                    return true;
                case "Cuboid":
                    this.BlockIdCuboid = id;
                    return true;
                case "FillArea":
                    this.BlockIdFillArea = id;
                    return true;
                case "Minecart":
                    this.BlockIdMinecart = id;
                    return true;
                case "Rail0":
                    this.BlockIdRailstart = id;
                    return true;
                default:
                    return false;
            }
        }

        public bool IsRailTile(int id)
        {
            return id >= BlockIdRailstart && id < BlockIdRailstart + 64;
        }

        public void UseBlockTypes(Packet_BlockType[] blocktypes, Dictionary<string,int> textureInAtlasIds)
        {
            for (int i = 0; i < blocktypes.Length; i++)
            {
                if (blocktypes[i] != null)
                {
                    UseBlockType(i, blocktypes[i], textureInAtlasIds);
                }
            }
        }
        
        public void UseBlockType(int id, Packet_BlockType b, Dictionary<string,int> textureIds)
        {
            if (b.Name == null)//!b.IsValid)
            {
                return;
            }
            //public bool[] IsWater { get { return mIsWater; } }
            //            public bool[] IsTransparentForLight { get { return mIsTransparentForLight; } }
            //public bool[] IsEmptyForPhysics { get { return mIsEmptyForPhysics; } }

            if (b.WhenPlacedGetsConvertedTo != 0)
            {
            	mWhenPlayerPlacesGetsConvertedTo[id] = b.WhenPlacedGetsConvertedTo;
            }
            else
            {
            	mWhenPlayerPlacesGetsConvertedTo[id] = id;
            }
            IsFlower[id] = b.DrawType == Packet_DrawTypeEnum.Plant;
            Rail[id] = (RailDirectionFlags)b.Rail;
            WalkSpeed[id] = DeserializeFloat(b.WalkSpeedFloat);
            IsSlipperyWalk[id] = b.IsSlipperyWalk;
            WalkSound[id] = (string[])b.Sounds.Walk.Clone();
            for (int i = 0; i < WalkSound[id].Length; i++)
            {
                WalkSound[id][i] += ".wav";
            }
            BreakSound[id] = (string[])b.Sounds.Break1.Clone();
            for (int i = 0; i < BreakSound[id].Length; i++)
            {
                BreakSound[id][i] += ".wav";
            }
            BuildSound[id] = (string[])b.Sounds.Build.Clone();
            for (int i = 0; i < BuildSound[id].Length; i++)
            {
                BuildSound[id][i] += ".wav";
            }
            CloneSound[id] = (string[])b.Sounds.Clone.Clone();
            for (int i = 0; i < CloneSound[id].Length; i++)
            {
                CloneSound[id][i] += ".wav";
            }
            LightRadius[id] = b.LightRadius;
            //StartInventoryAmount { get; }
            Strength[id] = b.Strength;
            DamageToPlayer[id] = b.DamageToPlayer;
            WalkableType1[id] = (WalkableType)b.WalkableType;
            SetSpecialBlock(b, id);
        }

        private float DeserializeFloat(int p)
        {
            return ((float)p) / 32;
        }
    }
}
