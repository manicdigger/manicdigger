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
        int BlockIdSingleStairs { get; }
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
        DrawType[] DrawType1 { get; }

        int[] DefaultMaterialSlots { get; }
        float[] LightLevels { get; } //maps light level (0-15) to GL.Color value.

        //Special blocks
        //Block 0 is empty block.
        int BlockIdGrass { get; }
        int BlockIdDirt { get; }
        int BlockIdGravel { get; }
        int BlockIdGold { get; }
        int BlockIdStone { get; }
        int BlockIdWater { get; }
        int BlockIdSand { get; }
        int BlockIdSponge { get; }
        int BlockIdTrampoline { get; }
        int BlockIdTorch { get; }
        int BlockIdAdminium { get; }
        int BlockIdCompass { get; }
        int BlockIdLadder { get; }
        int BlockIdEmptyHand { get; }
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
            int count = 256;
            Initialize(count);
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
        public DrawType[] DrawType1 { get { return mDrawType; } }
        public WalkableType[] WalkableType1 { get { return mWalkableType; } }

        public int[] DefaultMaterialSlots { get { return mDefaultMaterialSlots; } }
        public float[] LightLevels { get { return mLightLevels; } }


        public int BlockIdGrass { get { return mBlockIdGrass; } set { mBlockIdGrass = value; } }
        public int BlockIdDirt { get { return mBlockIdDirt; } set { mBlockIdDirt = value; } }
        public int BlockIdGravel { get { return mBlockIdGravel; } set { mBlockIdGravel = value; } }
        public int BlockIdGold { get { return mBlockIdGold; } set { mBlockIdGold = value; } }
        public int BlockIdStone { get { return mBlockIdStone; } set { mBlockIdStone = value; } }
        public int BlockIdWater { get { return mBlockIdWater; } set { mBlockIdWater = value; } }
        public int BlockIdSand { get { return mBlockIdSand; } set { mBlockIdSand = value; } }
        public int BlockIdSingleStairs { get { return mBlockIdSingleStairs; } set { mBlockIdSingleStairs = value; } }
        public int BlockIdSponge { get { return mBlockIdSponge; } set { mBlockIdSponge = value; } }
        public int BlockIdTrampoline { get { return mBlockIdTrampoline; } set { mBlockIdTrampoline = value; } }
        public int BlockIdTorch { get { return mBlockIdTorch; } set { mBlockIdTorch = value; } }
        public int BlockIdAdminium { get { return mBlockIdAdminium; } set { mBlockIdAdminium = value; } }
        public int BlockIdCompass { get { return mBlockIdCompass; } set { mBlockIdCompass = value; } }
        public int BlockIdLadder { get { return mBlockIdLadder; } set { mBlockIdLadder = value; } }
        public int BlockIdEmptyHand { get { return mBlockIdEmptyHand; } set { mBlockIdEmptyHand = value; } }

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
        private DrawType[] mDrawType;
        private WalkableType[] mWalkableType;

        private int[] mDefaultMaterialSlots;
        private float[] mLightLevels;

        private int mBlockIdGrass = 2;
        private int mBlockIdDirt = 3;
        private int mBlockIdGravel = 13;
        private int mBlockIdGold = 14;
        private int mBlockIdStone = 1;
        private int mBlockIdWater = 9;
        private int mBlockIdSand = 12;
        private int mBlockIdSingleStairs = 44;
        private int mBlockIdSponge = 19;
        private int mBlockIdTrampoline = 114;
        private int mBlockIdTorch = 50;
        private int mBlockIdAdminium = 7;
        private int mBlockIdCompass = 151;
        private int mBlockIdLadder = 152;
        private int mBlockIdEmptyHand = 153;

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
            IsEmptyForPhysics[id] = b.WalkableType != WalkableType.Solid && b.WalkableType != WalkableType.Fluid;
            IsTransparentFully[id] = (b.DrawType != DrawType.Solid) && (b.DrawType != DrawType.Plant)
                 && (b.DrawType != DrawType.OpenDoor) && (b.DrawType != DrawType.ClosedDoor);
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
            IsFlower[id] = b.DrawType == DrawType.Plant || b.DrawType == DrawType.OpenDoor || b.DrawType == DrawType.ClosedDoor;
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
            DrawType1[id] = b.DrawType;
            WalkableType1[id] = b.WalkableType;
        }
    }

    public class GameDataMonsters
    {
        public GameDataMonsters(IGetFileStream getfile)
        {
            int n = 5;
            MonsterCode = new string[n][];
            MonsterName = new string[n];
            MonsterSkin = new string[n];
            MonsterCode[0] = MyStream.ReadAllLines(getfile.GetFile("imp.txt"));
            MonsterName[0] = "Imp";
            MonsterSkin[0] = "imp.png";
            MonsterCode[1] = MyStream.ReadAllLines(getfile.GetFile("imp.txt"));
            MonsterName[1] = "Fire Imp";
            MonsterSkin[1] = "impfire.png";
            MonsterCode[2] = MyStream.ReadAllLines(getfile.GetFile("dragon.txt"));
            MonsterName[2] = "Dragon";
            MonsterSkin[2] = "dragon.png";
            MonsterCode[3] = MyStream.ReadAllLines(getfile.GetFile("zombie.txt"));
            MonsterName[3] = "Zombie";
            MonsterSkin[3] = "zombie.png";
            MonsterCode[4] = MyStream.ReadAllLines(getfile.GetFile("cyclops.txt"));
            MonsterName[4] = "Cyclops";
            MonsterSkin[4] = "cyclops.png";
        }
        public string[] MonsterName;
        public string[][] MonsterCode;
        public string[] MonsterSkin;
    }
}
