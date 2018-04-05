public class GameData
{
	public GameData()
	{
		mBlockIdEmpty = 0;
		mBlockIdDirt = -1;
		mBlockIdSponge = -1;
		mBlockIdTrampoline = -1;
		mBlockIdAdminium = -1;
		mBlockIdCompass = -1;
		mBlockIdLadder = -1;
		mBlockIdEmptyHand = -1;
		mBlockIdCraftingTable = -1;
		mBlockIdLava = -1;
		mBlockIdStationaryLava = -1;
		mBlockIdFillStart = -1;
		mBlockIdCuboid = -1;
		mBlockIdFillArea = -1;
		mBlockIdMinecart = -1;
		mBlockIdRailstart = -128; // 64 rail tiles
	}
	public void Start()
	{
		Initialize(GlobalVar.MAX_BLOCKTYPES);
	}
	public void Update()
	{
	}
	void Initialize(int count)
	{
		mWhenPlayerPlacesGetsConvertedTo = new int[count];
		mIsFlower = new bool[count];
		mRail = new int[count];
		mWalkSpeed = new float[count];
		for (int i = 0; i < count; i++)
		{
			mWalkSpeed[i] = 1;
		}
		mIsSlipperyWalk = new bool[count];
		mWalkSound = new string[count][];
		for (int i = 0; i < count; i++)
		{
			mWalkSound[i] = new string[SoundCount];
		}
		mBreakSound = new string[count][];
		for (int i = 0; i < count; i++)
		{
			mBreakSound[i] = new string[SoundCount];
		}
		mBuildSound = new string[count][];
		for (int i = 0; i < count; i++)
		{
			mBuildSound[i] = new string[SoundCount];
		}
		mCloneSound = new string[count][];
		for (int i = 0; i < count; i++)
		{
			mCloneSound[i] = new string[SoundCount];
		}
		mLightRadius = new int[count];
		mStartInventoryAmount = new int[count];
		mStrength = new float[count];
		mDamageToPlayer = new int[count];
		mWalkableType = new int[count];

		mDefaultMaterialSlots = new int[10];
	}

	public int[] WhenPlayerPlacesGetsConvertedTo() { return mWhenPlayerPlacesGetsConvertedTo; }
	public bool[] IsFlower() { return mIsFlower; }
	public int[] Rail() { return mRail; }
	public float[] WalkSpeed() { return mWalkSpeed; }
	public bool[] IsSlipperyWalk() { return mIsSlipperyWalk; }
	public string[][] WalkSound() { return mWalkSound; }
	public string[][] BreakSound() { return mBreakSound; }
	public string[][] BuildSound() { return mBuildSound; }
	public string[][] CloneSound() { return mCloneSound; }
	public int[] LightRadius() { return mLightRadius; }
	public int[] StartInventoryAmount() { return mStartInventoryAmount; }
	public float[] Strength() { return mStrength; }
	public int[] DamageToPlayer() { return mDamageToPlayer; }
	public int[] WalkableType1() { return mWalkableType; }

	public int[] DefaultMaterialSlots() { return mDefaultMaterialSlots; }

	int[] mWhenPlayerPlacesGetsConvertedTo;
	bool[] mIsFlower;
	int[] mRail;
	float[] mWalkSpeed;
	bool[] mIsSlipperyWalk;
	string[][] mWalkSound;
	string[][] mBreakSound;
	string[][] mBuildSound;
	string[][] mCloneSound;
	int[] mLightRadius;
	int[] mStartInventoryAmount;
	float[] mStrength;
	int[] mDamageToPlayer;
	int[] mWalkableType;

	int[] mDefaultMaterialSlots;

	// TODO: hardcoded IDs
	// few code sections still expect some hardcoded IDs
	int mBlockIdEmpty;
	int mBlockIdDirt;
	int mBlockIdSponge;
	int mBlockIdTrampoline;
	int mBlockIdAdminium;
	int mBlockIdCompass;
	int mBlockIdLadder;
	int mBlockIdEmptyHand;
	int mBlockIdCraftingTable;
	int mBlockIdLava;
	int mBlockIdStationaryLava;
	int mBlockIdFillStart;
	int mBlockIdCuboid;
	int mBlockIdFillArea;
	int mBlockIdMinecart;
	int mBlockIdRailstart; // 64 rail tiles

	public int BlockIdEmpty() { return mBlockIdEmpty; }
	public int BlockIdDirt() { return mBlockIdDirt; }
	public int BlockIdSponge() { return mBlockIdSponge; }
	public int BlockIdTrampoline() { return mBlockIdTrampoline; }
	public int BlockIdAdminium() { return mBlockIdAdminium; }
	public int BlockIdCompass() { return mBlockIdCompass; }
	public int BlockIdLadder() { return mBlockIdLadder; }
	public int BlockIdEmptyHand() { return mBlockIdEmptyHand; }
	public int BlockIdCraftingTable() { return mBlockIdCraftingTable; }
	public int BlockIdLava() { return mBlockIdLava; }
	public int BlockIdStationaryLava() { return mBlockIdStationaryLava; }
	public int BlockIdFillStart() { return mBlockIdFillStart; }
	public int BlockIdCuboid() { return mBlockIdCuboid; }
	public int BlockIdFillArea() { return mBlockIdFillArea; }
	public int BlockIdMinecart() { return mBlockIdMinecart; }
	public int BlockIdRailstart() { return mBlockIdRailstart; }

	// TODO: atm it sets sepcial block id from block name - better use new block property
	public bool SetSpecialBlock(Packet_BlockType b, int id)
	{
		switch (b.Name)
		{
			case "Empty":
				this.mBlockIdEmpty = id;
				return true;
			case "Dirt":
				this.mBlockIdDirt = id;
				return true;
			case "Sponge":
				this.mBlockIdSponge = id;
				return true;
			case "Trampoline":
				this.mBlockIdTrampoline = id;
				return true;
			case "Adminium":
				this.mBlockIdAdminium = id;
				return true;
			case "Compass":
				this.mBlockIdCompass = id;
				return true;
			case "Ladder":
				this.mBlockIdLadder = id;
				return true;
			case "EmptyHand":
				this.mBlockIdEmptyHand = id;
				return true;
			case "CraftingTable":
				this.mBlockIdCraftingTable = id;
				return true;
			case "Lava":
				this.mBlockIdLava = id;
				return true;
			case "StationaryLava":
				this.mBlockIdStationaryLava = id;
				return true;
			case "FillStart":
				this.mBlockIdFillStart = id;
				return true;
			case "Cuboid":
				this.mBlockIdCuboid = id;
				return true;
			case "FillArea":
				this.mBlockIdFillArea = id;
				return true;
			case "Minecart":
				this.mBlockIdMinecart = id;
				return true;
			case "Rail0":
				this.mBlockIdRailstart = id;
				return true;
			default:
				return false;
		}
	}

	public bool IsRailTile(int id)
	{
		return id >= BlockIdRailstart() && id < BlockIdRailstart() + 64;
	}

	public void UseBlockTypes(Packet_BlockType[] blocktypes, int count)
	{
		for (int i = 0; i < count; i++)
		{
			if (blocktypes[i] != null)
			{
				UseBlockType(i, blocktypes[i]);
			}
		}
	}

	public void UseBlockType(int id, Packet_BlockType b)
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
		IsFlower()[id] = b.DrawType == Packet_DrawTypeEnum.Plant;
		Rail()[id] = b.Rail;
		WalkSpeed()[id] = DeserializeFloat(b.WalkSpeedFloat);
		IsSlipperyWalk()[id] = b.IsSlipperyWalk;
		WalkSound()[id] = new string[SoundCount];
		BreakSound()[id] = new string[SoundCount];
		BuildSound()[id] = new string[SoundCount];
		CloneSound()[id] = new string[SoundCount];
		if (b.Sounds != null)
		{
			for (int i = 0; i < b.Sounds.WalkCount; i++)
			{
				WalkSound()[id][i] = b.Sounds.Walk[i];
			}
			for (int i = 0; i < b.Sounds.Break1Count; i++)
			{
				BreakSound()[id][i] = b.Sounds.Break1[i];
			}
			for (int i = 0; i < b.Sounds.BuildCount; i++)
			{
				BuildSound()[id][i] = b.Sounds.Build[i];
			}
			for (int i = 0; i < b.Sounds.CloneCount; i++)
			{
				CloneSound()[id][i] = b.Sounds.Clone[i];
			}
		}
		LightRadius()[id] = b.LightRadius;
		//StartInventoryAmount { get; }
		Strength()[id] = b.Strength;
		DamageToPlayer()[id] = b.DamageToPlayer;
		WalkableType1()[id] = b.WalkableType;
		SetSpecialBlock(b, id);
	}

	public const int SoundCount = 8;

	float DeserializeFloat(int p)
	{
		float one = 1;
		return (one * p) / 32;
	}
}
