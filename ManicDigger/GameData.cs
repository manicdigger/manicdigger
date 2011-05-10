using System;
using System.Collections.Generic;
using System.Text;

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
        int[] DefaultMaterialSlots { get; }
        float[] LightLevels { get; } //maps light level (0-15) to GL.Color value.

        //Special blocks
        //Block 0 is empty block.
        int BlockIdGrass { get; }
        int BlockIdDirt { get; }
        int BlockIdGold { get; }
        int BlockIdStone { get; }
        int BlockIdWater { get; }
        int BlockIdSand { get; }
        int BlockIdSponge { get; }
        int BlockIdTrampoline { get; }
        int BlockIdTorch { get; }
        int BlockIdAdminium { get; }
    }
    public class SpecialBlockId
    {
        public const int Empty = 0;
    }
    public class GameDataDummy : IGameData
    {
        public GameDataDummy()
        {
            Initialize(256);
        }

        public GameDataDummy(int count)
        {
            Initialize(count);
        }

        private void Initialize(int count)
        {
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
            mIsSlipperyWalk = new bool[count];
            mWalkSound = new string[count][];
            mBreakSound = new string[count][];
            mBuildSound = new string[count][];
            mCloneSound = new string[count][];
            mLightRadius = new int[count];
            mDefaultMaterialSlots = new int[10];
            mLightLevels = new float[16];
        }

        public void Update()
        {
        }

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
        public string[][] BreakSound { get { return mBreakSound; ; } }
        public string[][] BuildSound { get { return mBuildSound; } }
        public string[][] CloneSound { get { return mCloneSound; } }
        public int[] LightRadius { get { return mLightRadius; } }
        public int[] DefaultMaterialSlots { get { return mDefaultMaterialSlots; } }
        public float[] LightLevels { get { return mLightLevels; } }

        public int BlockIdGrass { get; set; }
        public int BlockIdDirt { get; set; }
        public int BlockIdGold { get; set; }
        public int BlockIdStone { get; set; }
        public int BlockIdWater { get; set; }
        public int BlockIdSand { get; set; }
        public int BlockIdSingleStairs { get; set; }
        public int BlockIdSponge { get; set; }
        public int BlockIdTrampoline { get; set; }
        public int BlockIdTorch { get; set; }
        public int BlockIdAdminium { get; set; }

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
        private int[] mDefaultMaterialSlots;
        private float[] mLightLevels;
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
    public class GameDataCsv : IGameData
    {
        public ICurrentSeason CurrentSeason = new CurrentSeasonDummy();
        public void Load(string[] csv, string[] defaultmaterialslots, string[] lightlevels)
        {
            this.blocks = LoadCsv(csv);
            int count = 256;
            Initialize(count);
            Update();
            for (int i = 0; i < 10; i++)
            {
                mDefaultMaterialSlots[i] = int.Parse(defaultmaterialslots[i]);
            }
            for (int i = 0; i < 16; i++)
            {
                mLightLevels[i] = float.Parse(lightlevels[i], System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        public void Update()
        {
            for (int i = 1; i < blocks.Length; i++)
            {
                string[] block = blocks[i];
                string id_ = block[Column("Id").Value];
                if (id_.Contains("_")) { continue; }//todo
                int id = int.Parse(id_);
                mName[id] = block[Column("Name").Value];
                mIsValid[id] = true;
                if (Get(block, "Season").Trim() != "")
                {
                    if (CurrentSeason.CurrentSeason != int.Parse(Get(block, "Season")))
                    {
                        continue;
                    }
                }
                mTextureId[id, 0] = (int)intParse(Get(block, "TextureIdTop"));
                mTextureId[id, 1] = (int)intParse(Get(block, "TextureIdBottom"));
                mTextureId[id, 2] = (int)intParse(Get(block, "TextureIdFront"));
                mTextureId[id, 3] = (int)intParse(Get(block, "TextureIdBack"));
                mTextureId[id, 4] = (int)intParse(Get(block, "TextureIdLeft"));
                mTextureId[id, 5] = (int)intParse(Get(block, "TextureIdRight"));
                mTextureIdForInventory[id] = (int)intParse(block[Column("TextureIdForInventory").Value]);
                mIsBuildable[id] = BoolParse(block[Column("IsBuildable").Value]);
                mWhenPlayerPlacesGetsConvertedTo[id] = (int)intParse(block[Column("WhenPlayerPlacesGetsConvertedTo").Value]);
                mIsFlower[id] = BoolParse(block[Column("IsFlower").Value]);
                mRail[id] = (RailDirectionFlags)intParse(Get(block, "Rail"));
                mWalkSpeed[id] = (float)intParse(Get(block, "WalkSpeed"));
                mIsTransparentForLight[id] = BoolParse(block[Column("IsTransparentForLight").Value]);
                mIsSlipperyWalk[id] = BoolParse(block[Column("IsSlipperyWalk").Value]);
                LoadSound(mWalkSound, "WalkSound", block, id);
                LoadSound(mBreakSound, "BreakSound", block, id);
                LoadSound(mBuildSound, "BuildSound", block, id);
                LoadSound(mCloneSound, "CloneSound", block, id);
                mIsWater[id] = BoolParse(block[Column("IsFluid").Value]);
                mIsTransparent[id] = BoolParse(block[Column("IsTransparent").Value]);
                mIsTransparentFully[id] = BoolParse(block[Column("IsTransparentFully").Value]);
                mIsEmptyForPhysics[id] = BoolParse(block[Column("IsEmptyForPhysics").Value]);
                mLightRadius[id] = (int)intParse(Get(block, "LightRadius"));
            }
        }
        private void LoadSound(string[][] t, string s, string[] block, int id)
        {
            t[id] = Get(block, s).Split(new char[] { ' ' });
            if (t[id].Length == 1 && t[id][0].Length == 0)
            {
                t[id] = new string[0];
            }
            for (int k = 0; k < t[id].Length; k++)
            {
                if (!t[id][k].Contains("."))
                {
                    t[id][k] += ".wav";
                }
            }
        }
        private string[][] LoadCsv(string[] csv)
        {
            List<string[]> table = new List<string[]>();
            for (int i = 0; i < csv.Length; i++)
            {
                string s = csv[i];
                s = s.Replace("\"", "");
                string[] ss = s.Split(new char[] { ',', ';' });
                table.Add(ss);
            }
            return table.ToArray();
        }
        private string Get(string[] block, string column)
        {
            return block[Column(column).Value];
        }
        double intParse(string s)
        {
            double result;
            if (double.TryParse(s, out result))
            {
                return result;
            }
            return 0;
        }
        private bool BoolParse(string s)
        {
            return !(s == "" || s == "0" || s.Equals("false", StringComparison.InvariantCultureIgnoreCase));
        }
        int? Column(string columnHeader)
        {
            string[] headers = blocks[0];
            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i].Equals(columnHeader, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
            }
            return null;
        }
        string[][] blocks;

        private void Initialize(int count)
        {
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
            mDefaultMaterialSlots = new int[10];
            mLightLevels = new float[16];
            mIsValid[0] = true;
            mIsEmptyForPhysics[0] = true;
        }

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
        public int[] DefaultMaterialSlots { get { return mDefaultMaterialSlots; } }
        public float[] LightLevels { get { return mLightLevels; } }


        public int BlockIdGrass { get { return mBlockIdGrass; } set { mBlockIdGrass = value; } }
        public int BlockIdDirt { get { return mBlockIdDirt; } set { mBlockIdDirt = value; } }
        public int BlockIdGold { get { return mBlockIdGold; } set { mBlockIdGold = value; } }
        public int BlockIdStone { get { return mBlockIdStone; } set { mBlockIdStone = value; } }
        public int BlockIdWater { get { return mBlockIdWater; } set { mBlockIdWater = value; } }
        public int BlockIdSand { get { return mBlockIdSand; } set { mBlockIdSand = value; } }
        public int BlockIdSingleStairs { get { return mBlockIdSingleStairs; } set { mBlockIdSingleStairs = value; } }
        public int BlockIdSponge { get { return mBlockIdSponge; } set { mBlockIdSponge = value; } }
        public int BlockIdTrampoline { get { return mBlockIdTrampoline; } set { mBlockIdTrampoline = value; } }
        public int BlockIdTorch { get { return mBlockIdTorch; } set { mBlockIdTorch = value; } }
        public int BlockIdAdminium { get { return mBlockIdAdminium; } set { mBlockIdAdminium = value; } }

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
        private int[] mDefaultMaterialSlots;
        private float[] mLightLevels;

        private int mBlockIdGrass = 2;
        private int mBlockIdDirt = 3;
        private int mBlockIdGold = 14;
        private int mBlockIdStone = 1;
        private int mBlockIdWater = 9;
        private int mBlockIdSand = 12;
        private int mBlockIdSingleStairs = 44;
        private int mBlockIdSponge = 19;
        private int mBlockIdTrampoline = 114;
        private int mBlockIdTorch = 50;
        private int mBlockIdAdminium = 7;
    }
}
