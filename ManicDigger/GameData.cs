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
    public class Csv
    {
        public string[][] data;

        public void LoadCsv(string[] csv)
        {
            List<string[]> table = new List<string[]>();
            for (int i = 0; i < csv.Length; i++)
            {
                string s = csv[i];
                s = s.Replace("\"", "");
                string[] ss = s.Split(new char[] { ',', ';' });
                table.Add(ss);
            }
            data = table.ToArray();
        }

        public string Get(int row, string column)
        {
            string[] rowStrings = data[row];
            return rowStrings[Column(column).Value];
        }

        public int? Column(string columnHeader)
        {
            string[] headers = data[0];
            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i].Equals(columnHeader, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
            }
            return null;
        }

        public int GetInt(int row, string columnHeader)
        {
            return (int)DoubleParse(Get(row, columnHeader));
        }

        public bool GetBool(int row, string columnHeader)
        {
            return BoolParse(Get(row, columnHeader));
        }

        public double GetDouble(int row, string column)
        {
            return DoubleParse(Get(row, column));
        }

        private double DoubleParse(string s)
        {
            double result;
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }
            return 0;
        }

        private bool BoolParse(string s)
        {
            return !(s == "" || s == "0" || s.Equals("false", StringComparison.InvariantCultureIgnoreCase));
        }
    }
    public class GameDataCsv : IGameData
    {
        public ICurrentSeason CurrentSeason = new CurrentSeasonDummy();
        public void Load(string[] csv, string[] defaultmaterialslots, string[] lightlevels)
        {
            this.csv = new Csv();
            this.csv.LoadCsv(csv);
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
            for (int i = 1; i < csv.data.Length; i++)
            {
                string id_ = csv.Get(i, "Id");
                if (id_.Contains("_")) { continue; }//todo
                int id = int.Parse(id_);
                mName[id] = csv.Get(i, "Name");
                mIsValid[id] = true;
                if (csv.Get(i, "Season").Trim() != "")
                {
                    if (CurrentSeason.CurrentSeason != int.Parse(csv.Get(i, "Season")))
                    {
                        continue;
                    }
                }
                mTextureId[id, 0] = csv.GetInt(i, "TextureIdTop");
                mTextureId[id, 1] = csv.GetInt(i, "TextureIdBottom");
                mTextureId[id, 2] = csv.GetInt(i, "TextureIdFront");
                mTextureId[id, 3] = csv.GetInt(i, "TextureIdBack");
                mTextureId[id, 4] = csv.GetInt(i, "TextureIdLeft");
                mTextureId[id, 5] = csv.GetInt(i, "TextureIdRight");
                mTextureIdForInventory[id] = csv.GetInt(i, "TextureIdForInventory");
                mIsBuildable[id] = csv.GetBool(i, "IsBuildable");
                mWhenPlayerPlacesGetsConvertedTo[id] = csv.GetInt(i, "WhenPlayerPlacesGetsConvertedTo");
                mIsFlower[id] = csv.GetBool(i, "IsFlower");
                mRail[id] = (RailDirectionFlags)csv.GetInt(i, "Rail");
                mWalkSpeed[id] = (float)csv.GetDouble(i, "WalkSpeed");
                mIsTransparentForLight[id] = csv.GetBool(i, "IsTransparentForLight");
                mIsSlipperyWalk[id] = csv.GetBool(i, "IsSlipperyWalk");
                LoadSound(mWalkSound, "WalkSound", i, id);
                LoadSound(mBreakSound, "BreakSound", i, id);
                LoadSound(mBuildSound, "BuildSound", i, id);
                LoadSound(mCloneSound, "CloneSound", i, id);
                mIsWater[id] = csv.GetBool(i, "IsFluid");
                mIsTransparent[id] = csv.GetBool(i, "IsTransparent");
                mIsTransparentFully[id] = csv.GetBool(i, "IsTransparentFully");
                mIsEmptyForPhysics[id] = csv.GetBool(i, "IsEmptyForPhysics");
                mLightRadius[id] = csv.GetInt(i, "LightRadius");
                mStartInventoryAmount[id] = csv.GetInt(i, "StartInventoryAmount");
                mStrength[id] = (float)csv.GetDouble(i, "Strength");
            }
        }
        private void LoadSound(string[][] t, string s, int i, int id)
        {
            t[id] = csv.Get(i, s).Split(new char[] { ' ' });
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
        Csv csv;
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
            mStartInventoryAmount = new int[count];
            mStrength = new float[count];

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
        public int[] StartInventoryAmount { get { return mStartInventoryAmount; } }
        public float[] Strength { get { return mStrength; } }

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
