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
        bool[] IsEmptyForPhysics { get; }
        bool[] IsFluid { get; }
        bool[] IsWater { get; }
    }
    public interface IGameData : IGameDataLight, IGameDataPhysics
    {
        void Update();

        bool[] IsTransparent { get; }
        bool[] IsValid { get; }
        bool[] IsTransparentFully { get; }
        //Indexed by block id and TileSide.
        int[,] TextureId { get; }
        int[] TextureIdForInventory { get; }
        bool[] IsBuildable { get; }
        int[] WhenPlayerPlacesGetsConvertedTo { get; }//PlayerBuildableMaterialType
        bool[] IsFlower { get; }
        RailDirectionFlags[] Rail { get; }
        string[] Name { get; }
        float[] WalkSpeed { get; }
        bool[] IsSlipperyWalk { get; }
        string[][] WalkSound { get; }
        string[][] BreakSound { get; }
        string[][] BuildSound { get; }
        string[][] CloneSound { get; }
        int[] StartInventoryAmount { get; }
        float[] Strength { get; }
        int[] DamageToPlayer { get; }
        DrawType[] DrawType1 { get; }

        int[] DefaultMaterialSlots { get; }
        float[] LightLevels { get; } //maps light level (0-15) to GL.Color value.

        //Special blocks
        //Block 0 is empty block.
        int BlockIdEmpty { get; }
        int BlockIdDirt { get; }
        int BlockIdSponge { get; }
        int BlockIdTrampoline { get; }
        int BlockIdAdminium { get; }
        int BlockIdCompass { get; }
        int BlockIdLadder { get; }
        int BlockIdEmptyHand { get; }
        int BlockIdCraftingTable { get; }
        int BlockIdLava { get; }
        int BlockIdStationaryLava { get; }
        int BlockIdFillStart { get; }
        int BlockIdCuboid { get; }
        int BlockIdFillArea { get; }
        int BlockIdMinecart { get; }
        int BlockIdRailstart { get ; }


    }
    public class GlobalVar
    {
        public const int MAX_BLOCKTYPES = 1024;
        public const int MAX_BLOCKTYPES_SQRT = 32;
    }
    public class SpecialBlockId
    {
        public const int Empty = 0;
    }
    public interface ICurrentSeason
    {
        int CurrentSeason { get; }
    }
    public class CurrentSeasonDummy : ICurrentSeason
    {
        #region ICurrentSeason Members
        public int CurrentSeason { get { return 0; } }
        #endregion
    }
    public class GameData : IGameData
    {
        public void Start()
        {
            Initialize(GlobalVar.MAX_BLOCKTYPES);
        }
        public ICurrentSeason CurrentSeason = new CurrentSeasonDummy();
        public void Update()
        {
        }
        private void Initialize(int count)
        {
            mIsFluid = new bool[count];
            mIsWater = new bool[count];
            mIsTransparent = new bool[count];
            mIsValid = new bool[count];
            mIsTransparentForLight = new bool[count];
            mIsTransparentFully = new bool[count];
            mIsEmptyForPhysics = new bool[count];
            mTextureId = new int[count, 6];
            mTextureIdForInventory = new int[count];
            mIsBuildable = new bool[count];
            mWhenPlayerPlacesGetsConvertedTo = new int[count];
            mIsFlower = new bool[count];
            mRail = new RailDirectionFlags[count];
            mName = new string[count];
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
            mDrawType = new DrawType[count];
            mWalkableType = new WalkableType[count];

            mDefaultMaterialSlots = new int[10];
            mLightLevels = new float[16];
            mIsValid[0] = true;
            mIsEmptyForPhysics[0] = true;
        }

        public bool[] IsFluid { get { return mIsFluid; } }
        public bool[] IsWater { get { return mIsWater; } }
        public bool[] IsTransparent { get { return mIsTransparent; } }
        public bool[] IsValid { get { return mIsValid; } }
        public bool[] IsTransparentForLight { get { return mIsTransparentForLight; } }
        public bool[] IsTransparentFully { get { return mIsTransparentFully; } }
        public bool[] IsEmptyForPhysics { get { return mIsEmptyForPhysics; } }
        public int[,] TextureId { get { return mTextureId; } }
        public int[] TextureIdForInventory { get { return mTextureIdForInventory; } }
        public bool[] IsBuildable { get { return mIsBuildable; } }
        public int[] WhenPlayerPlacesGetsConvertedTo { get { return mWhenPlayerPlacesGetsConvertedTo; } }
        public bool[] IsFlower { get { return mIsFlower; } }
        public RailDirectionFlags[] Rail { get { return mRail; } }
        public string[] Name { get { return mName; } }
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
        public DrawType[] DrawType1 { get { return mDrawType; } }
        public WalkableType[] WalkableType1 { get { return mWalkableType; } }

        public int[] DefaultMaterialSlots { get { return mDefaultMaterialSlots; } }
        public float[] LightLevels { get { return mLightLevels; } }

        private bool[] mIsFluid;
        private bool[] mIsWater;
        private bool[] mIsTransparent;
        private bool[] mIsValid;
        private bool[] mIsTransparentForLight;
        private bool[] mIsTransparentFully;
        private bool[] mIsEmptyForPhysics;
        private int[,] mTextureId;
        private int[] mTextureIdForInventory;
        private bool[] mIsBuildable;
        private int[] mWhenPlayerPlacesGetsConvertedTo;
        private bool[] mIsFlower;
        private RailDirectionFlags[] mRail;
        private string[] mName;
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
        private DrawType[] mDrawType;
        private WalkableType[] mWalkableType;

        private int[] mDefaultMaterialSlots;
        private float[] mLightLevels;

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
        public bool SetSpecialBlock(BlockType b, int id)
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

        public void UseBlockTypes(BlockType[] blocktypes, Dictionary<string,int> textureInAtlasIds)
        {
            for (int i = 0; i < blocktypes.Length; i++)
            {
                if (blocktypes[i] != null)
                {
                    UseBlockType(i, blocktypes[i], textureInAtlasIds);
                }
            }
        }

        public void UseBlockType(int id, BlockType b, Dictionary<string,int> textureIds)
        {
            IsValid[id] = b.Name != null;//b.IsValid;
            if (b.Name == null)//!b.IsValid)
            {
                return;
            }
            //public bool[] IsFluid { get { return mIsFluid; } }
            //public bool[] IsWater { get { return mIsWater; } }
            IsFluid[id] = b.DrawType == DrawType.Fluid;
            IsWater[id] = b.Name.Contains("Water"); //todo
            IsTransparent[id] = (b.DrawType != DrawType.Solid) && (b.DrawType != DrawType.Fluid);
            //            public bool[] IsTransparentForLight { get { return mIsTransparentForLight; } }
            IsTransparentForLight[id] = b.DrawType != DrawType.Solid && b.DrawType != DrawType.ClosedDoor;
            //public bool[] IsEmptyForPhysics { get { return mIsEmptyForPhysics; } }

            if ((b.DrawType == DrawType.Ladder) || (b.WalkableType != WalkableType.Solid && b.WalkableType != WalkableType.Fluid))
            {
                IsEmptyForPhysics[id] = true;
            }
            else
            {
                IsEmptyForPhysics[id] = false;
            }

            IsTransparentFully[id] = (b.DrawType != DrawType.Solid) && (b.DrawType != DrawType.Plant)
                 && (b.DrawType != DrawType.OpenDoorLeft) && (b.DrawType != DrawType.OpenDoorRight) && (b.DrawType != DrawType.ClosedDoor);
            //Indexed by block id and TileSide.
            if (textureIds != null)
            {
                TextureId[id, 0] = textureIds[b.TextureIdTop];
                TextureId[id, 1] = textureIds[b.TextureIdBottom];
                TextureId[id, 2] = textureIds[b.TextureIdFront];
                TextureId[id, 3] = textureIds[b.TextureIdBack];
                TextureId[id, 4] = textureIds[b.TextureIdLeft];
                TextureId[id, 5] = textureIds[b.TextureIdRight];
                TextureIdForInventory[id] = textureIds[b.TextureIdForInventory];
            }
            IsBuildable[id] = b.IsBuildable; // todo
            WhenPlayerPlacesGetsConvertedTo[id] = id; // todo
            IsFlower[id] = b.DrawType == DrawType.Plant;
            Rail[id] = (RailDirectionFlags)b.Rail;
            Name[id] = b.Name;
            WalkSpeed[id] = b.WalkSpeed;
            IsSlipperyWalk[id] = b.IsSlipperyWalk;
            WalkSound[id] = (string[])b.Sounds.Walk.Clone();
            for (int i = 0; i < WalkSound[id].Length; i++)
            {
                WalkSound[id][i] += ".wav";
            }
            BreakSound[id] = (string[])b.Sounds.Break.Clone();
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
            DrawType1[id] = b.DrawType;
            WalkableType1[id] = b.WalkableType;
            SetSpecialBlock(b, id);
        }
    }

    public class GameDataMonsters
    {
        public GameDataMonsters(IGetFileStream getfile)
        {
            int n = 5;
            MonsterCode = new string[n];
            MonsterName = new string[n];
            MonsterSkin = new string[n];
            MonsterCode[0] = "imp.txt";
            MonsterName[0] = "Imp";
            MonsterSkin[0] = "imp.png";
            MonsterCode[1] = "imp.txt";
            MonsterName[1] = "Fire Imp";
            MonsterSkin[1] = "impfire.png";
            MonsterCode[2] = "dragon.txt";
            MonsterName[2] = "Dragon";
            MonsterSkin[2] = "dragon.png";
            MonsterCode[3] = "zombie.txt";
            MonsterName[3] = "Zombie";
            MonsterSkin[3] = "zombie.png";
            MonsterCode[4] = "cyclops.txt";
            MonsterName[4] = "Cyclops";
            MonsterSkin[4] = "cyclops.png";
        }
        public string[] MonsterName;
        public string[] MonsterCode;
        public string[] MonsterSkin;
    }
}
