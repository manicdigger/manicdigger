public class TileEnterData
{
	internal int BlockPositionX;
	internal int BlockPositionY;
	internal int BlockPositionZ;
	internal TileEnterDirection EnterDirection;
}

public class UpDown
{
	public const int None = 0;
	public const int Up = 1;
	public const int Down = 2;
}

class StringByteArray
{
	internal string name;
	internal byte[] data;
}

public class RenderHintEnum
{
	public const int Fast = 0;
	public const int Nice = 1;
}

public class Speculative
{
	internal int x;
	internal int y;
	internal int z;
	internal int timeMilliseconds;
	internal int blocktype;
}

public class TimerCi
{
	public TimerCi()
	{
		interval = 1;
		maxDeltaTime = -1;
	}
	internal float interval;
	internal float maxDeltaTime;

	internal float accumulator;
	public void Reset()
	{
		accumulator = 0;
	}
	public int Update(float dt)
	{
		accumulator += dt;
		float constDt = interval;
		if (maxDeltaTime != -1 && accumulator > maxDeltaTime)
		{
			accumulator = maxDeltaTime;
		}
		int updates = 0;
		while (accumulator >= constDt)
		{
			updates++;
			accumulator -= constDt;
		}
		return updates;
	}

	internal static TimerCi Create(int interval_, int maxDeltaTime_)
	{
		TimerCi timer = new TimerCi();
		timer.interval = interval_;
		timer.maxDeltaTime = maxDeltaTime_;
		return timer;
	}
}

public class GetBlockHeight_ : DelegateGetBlockHeight
{
	public static GetBlockHeight_ Create(Game w_)
	{
		GetBlockHeight_ g = new GetBlockHeight_();
		g.w = w_;
		return g;
	}
	internal Game w;
	public override float GetBlockHeight(int x, int y, int z)
	{
		return w.getblockheight(x, y, z);
	}
}

public class IsBlockEmpty_ : DelegateIsBlockEmpty
{
	public static IsBlockEmpty_ Create(Game w_)
	{
		IsBlockEmpty_ g = new IsBlockEmpty_();
		g.w = w_;
		return g;
	}
	Game w;
	public override bool IsBlockEmpty(int x, int y, int z)
	{
		return w.IsTileEmptyForPhysics(x, y, z);
	}
}

public class Sprite
{
	public Sprite()
	{
		size = 40;
	}
	internal float positionX;
	internal float positionY;
	internal float positionZ;
	internal string image;
	internal int size;
	internal int animationcount;
}

public class PlayerDrawInfo
{
	public PlayerDrawInfo()
	{
		anim = new AnimationState();
		AnimationHint_ = new AnimationHint();
	}
	internal AnimationState anim;
	internal NetworkInterpolation interpolation;
	internal float lastnetworkposX;
	internal float lastnetworkposY;
	internal float lastnetworkposZ;
	internal float lastcurposX;
	internal float lastcurposY;
	internal float lastcurposZ;
	internal float lastnetworkrotx;
	internal float lastnetworkroty;
	internal float lastnetworkrotz;
	internal float velocityX;
	internal float velocityY;
	internal float velocityZ;
	internal bool moves;
	internal AnimationHint AnimationHint_;
}

public class PlayerInterpolate : IInterpolation
{
	internal GamePlatform platform;
	public override InterpolatedObject Interpolate(InterpolatedObject a, InterpolatedObject b, float progress)
	{
		PlayerInterpolationState aa = platform.CastToPlayerInterpolationState(a);
		PlayerInterpolationState bb = platform.CastToPlayerInterpolationState(b);
		PlayerInterpolationState cc = new PlayerInterpolationState();
		cc.positionX = aa.positionX + (bb.positionX - aa.positionX) * progress;
		cc.positionY = aa.positionY + (bb.positionY - aa.positionY) * progress;
		cc.positionZ = aa.positionZ + (bb.positionZ - aa.positionZ) * progress;
		//cc.heading = ConvertCi.IntToByte(AngleInterpolation.InterpolateAngle256(platform, aa.heading, bb.heading, progress));
		//cc.pitch = ConvertCi.IntToByte(AngleInterpolation.InterpolateAngle256(platform, aa.pitch, bb.pitch, progress));
		cc.rotx = DegToRad(AngleInterpolation.InterpolateAngle360(platform, RadToDeg(aa.rotx), RadToDeg(bb.rotx), progress));
		cc.roty = DegToRad(AngleInterpolation.InterpolateAngle360(platform, RadToDeg(aa.roty), RadToDeg(bb.roty), progress));
		cc.rotz = DegToRad(AngleInterpolation.InterpolateAngle360(platform, RadToDeg(aa.rotz), RadToDeg(bb.rotz), progress));
		return cc;
	}
	public static float RadToDeg(float rad)
	{
		return (rad / (2 * Game.GetPi())) * 360;
	}
	public static float DegToRad(float deg)
	{
		return (deg / 360) * 2 * Game.GetPi();
	}
}

public class PlayerInterpolationState : InterpolatedObject
{
	internal float positionX;
	internal float positionY;
	internal float positionZ;
	internal float rotx;
	internal float roty;
	internal float rotz;
	internal byte heading;
	internal byte pitch;
}

public class Bullet_
{
	internal float fromX;
	internal float fromY;
	internal float fromZ;
	internal float toX;
	internal float toY;
	internal float toZ;
	internal float speed;
	internal float progress;
}

public class Expires
{
	internal static Expires Create(float p)
	{
		Expires expires = new Expires();
		expires.totalTime = p;
		expires.timeLeft = p;
		return expires;
	}

	internal float totalTime;
	internal float timeLeft;
}

public class DrawName
{
	internal float TextX;
	internal float TextY;
	internal float TextZ;
	internal string Name;
	internal bool DrawHealth;
	internal float Health;
	internal bool OnlyWhenSelected;
	internal bool ClientAutoComplete;
}

public class Vector3Float
{
	internal int x;
	internal int y;
	internal int z;
	internal float value;
}

public class VisibleDialog
{
	internal string key;
	internal Packet_Dialog value;
	internal GameScreen screen;
}

public class RailMapUtil
{
	internal Game game;
	public RailSlope GetRailSlope(int x, int y, int z)
	{
		int tiletype = game.map.GetBlock(x, y, z);
		int railDirectionFlags = game.blocktypes[tiletype].Rail;
		int blocknear;
		if (x < game.map.MapSizeX - 1)
		{
			blocknear = game.map.GetBlock(x + 1, y, z);
			if (railDirectionFlags == RailDirectionFlags.Horizontal &&
				 blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
			{
				return RailSlope.TwoRightRaised;
			}
		}
		if (x > 0)
		{
			blocknear = game.map.GetBlock(x - 1, y, z);
			if (railDirectionFlags == RailDirectionFlags.Horizontal &&
				 blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
			{
				return RailSlope.TwoLeftRaised;

			}
		}
		if (y > 0)
		{
			blocknear = game.map.GetBlock(x, y - 1, z);
			if (railDirectionFlags == RailDirectionFlags.Vertical &&
				  blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
			{
				return RailSlope.TwoUpRaised;
			}
		}
		if (y < game.map.MapSizeY - 1)
		{
			blocknear = game.map.GetBlock(x, y + 1, z);
			if (railDirectionFlags == RailDirectionFlags.Vertical &&
				  blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
			{
				return RailSlope.TwoDownRaised;
			}
		}
		return RailSlope.Flat;
	}
}

public class RailDirectionFlags
{
	public const int None = 0;
	public const int Horizontal = 1;
	public const int Vertical = 2;
	public const int UpLeft = 4;
	public const int UpRight = 8;
	public const int DownLeft = 16;
	public const int DownRight = 32;

	public const int Full = Horizontal | Vertical | UpLeft | UpRight | DownLeft | DownRight;
	public const int TwoHorizontalVertical = Horizontal | Vertical;
	public const int Corners = UpLeft | UpRight | DownLeft | DownRight;
}

public enum RailSlope
{
	Flat, TwoLeftRaised, TwoRightRaised, TwoUpRaised, TwoDownRaised
}

public enum RailDirection
{
	Horizontal,
	Vertical,
	UpLeft,
	UpRight,
	DownLeft,
	DownRight
}

public enum TileExitDirection
{
	Up,
	Down,
	Left,
	Right
}

public enum TileEnterDirection
{
	Up,
	Down,
	Left,
	Right
}

/// <summary>
/// Each RailDirection on tile can be traversed by train in two directions.
/// </summary>
/// <example>
/// RailDirection.Horizontal -> VehicleDirection12.HorizontalLeft (vehicle goes left and decreases x position),
/// and VehicleDirection12.HorizontalRight (vehicle goes right and increases x position).
/// </example>
public enum VehicleDirection12
{
	HorizontalLeft,
	HorizontalRight,
	VerticalUp,
	VerticalDown,

	UpLeftUp,
	UpLeftLeft,
	UpRightUp,
	UpRightRight,

	DownLeftDown,
	DownLeftLeft,
	DownRightDown,
	DownRightRight
}

public class VehicleDirection12Flags
{
	public const int None = 0;
	public const int HorizontalLeft = 1 << 0;
	public const int HorizontalRight = 1 << 1;
	public const int VerticalUp = 1 << 2;
	public const int VerticalDown = 1 << 3;

	public const int UpLeftUp = 1 << 4;
	public const int UpLeftLeft = 1 << 5;
	public const int UpRightUp = 1 << 6;
	public const int UpRightRight = 1 << 7;

	public const int DownLeftDown = 1 << 8;
	public const int DownLeftLeft = 1 << 9;
	public const int DownRightDown = 1 << 10;
	public const int DownRightRight = 1 << 11;
}

public class DirectionUtils
{
	/// <summary>
	/// VehicleDirection12.UpRightRight -> returns Direction4.Right
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public static TileExitDirection ResultExit(VehicleDirection12 direction)
	{
		switch (direction)
		{
			case VehicleDirection12.HorizontalLeft:
				return TileExitDirection.Left;
			case VehicleDirection12.HorizontalRight:
				return TileExitDirection.Right;
			case VehicleDirection12.VerticalUp:
				return TileExitDirection.Up;
			case VehicleDirection12.VerticalDown:
				return TileExitDirection.Down;

			case VehicleDirection12.UpLeftUp:
				return TileExitDirection.Up;
			case VehicleDirection12.UpLeftLeft:
				return TileExitDirection.Left;
			case VehicleDirection12.UpRightUp:
				return TileExitDirection.Up;
			case VehicleDirection12.UpRightRight:
				return TileExitDirection.Right;

			case VehicleDirection12.DownLeftDown:
				return TileExitDirection.Down;
			case VehicleDirection12.DownLeftLeft:
				return TileExitDirection.Left;
			case VehicleDirection12.DownRightDown:
				return TileExitDirection.Down;
			case VehicleDirection12.DownRightRight:
				return TileExitDirection.Right;
			default:
				return TileExitDirection.Down;
		}
	}

	public static RailDirection ToRailDirection(VehicleDirection12 direction)
	{
		switch (direction)
		{
			case VehicleDirection12.HorizontalLeft:
				return RailDirection.Horizontal;
			case VehicleDirection12.HorizontalRight:
				return RailDirection.Horizontal;
			case VehicleDirection12.VerticalUp:
				return RailDirection.Vertical;
			case VehicleDirection12.VerticalDown:
				return RailDirection.Vertical;

			case VehicleDirection12.UpLeftUp:
				return RailDirection.UpLeft;
			case VehicleDirection12.UpLeftLeft:
				return RailDirection.UpLeft;
			case VehicleDirection12.UpRightUp:
				return RailDirection.UpRight;
			case VehicleDirection12.UpRightRight:
				return RailDirection.UpRight;

			case VehicleDirection12.DownLeftDown:
				return RailDirection.DownLeft;
			case VehicleDirection12.DownLeftLeft:
				return RailDirection.DownLeft;
			case VehicleDirection12.DownRightDown:
				return RailDirection.DownRight;
			case VehicleDirection12.DownRightRight:
				return RailDirection.DownRight;
			default:
				return RailDirection.DownLeft;
		}
	}

	public static int ToRailDirectionFlags(RailDirection direction)
	{
		switch (direction)
		{
			case RailDirection.DownLeft:
				return RailDirectionFlags.DownLeft;
			case RailDirection.DownRight:
				return RailDirectionFlags.DownRight;
			case RailDirection.Horizontal:
				return RailDirectionFlags.Horizontal;
			case RailDirection.UpLeft:
				return RailDirectionFlags.UpLeft;
			case RailDirection.UpRight:
				return RailDirectionFlags.UpRight;
			case RailDirection.Vertical:
				return RailDirectionFlags.Vertical;
			default:
				return 0;
		}
	}

	public static VehicleDirection12 Reverse(VehicleDirection12 direction)
	{
		switch (direction)
		{
			case VehicleDirection12.HorizontalLeft:
				return VehicleDirection12.HorizontalRight;
			case VehicleDirection12.HorizontalRight:
				return VehicleDirection12.HorizontalLeft;
			case VehicleDirection12.VerticalUp:
				return VehicleDirection12.VerticalDown;
			case VehicleDirection12.VerticalDown:
				return VehicleDirection12.VerticalUp;

			case VehicleDirection12.UpLeftUp:
				return VehicleDirection12.UpLeftLeft;
			case VehicleDirection12.UpLeftLeft:
				return VehicleDirection12.UpLeftUp;
			case VehicleDirection12.UpRightUp:
				return VehicleDirection12.UpRightRight;
			case VehicleDirection12.UpRightRight:
				return VehicleDirection12.UpRightUp;

			case VehicleDirection12.DownLeftDown:
				return VehicleDirection12.DownLeftLeft;
			case VehicleDirection12.DownLeftLeft:
				return VehicleDirection12.DownLeftDown;
			case VehicleDirection12.DownRightDown:
				return VehicleDirection12.DownRightRight;
			case VehicleDirection12.DownRightRight:
				return VehicleDirection12.DownRightDown;
			default:
				return VehicleDirection12.DownLeftDown;
		}
	}

	public static int ToVehicleDirection12Flags(VehicleDirection12 direction)
	{
		switch (direction)
		{
			case VehicleDirection12.HorizontalLeft:
				return VehicleDirection12Flags.HorizontalLeft;
			case VehicleDirection12.HorizontalRight:
				return VehicleDirection12Flags.HorizontalRight;
			case VehicleDirection12.VerticalUp:
				return VehicleDirection12Flags.VerticalUp;
			case VehicleDirection12.VerticalDown:
				return VehicleDirection12Flags.VerticalDown;

			case VehicleDirection12.UpLeftUp:
				return VehicleDirection12Flags.UpLeftUp;
			case VehicleDirection12.UpLeftLeft:
				return VehicleDirection12Flags.UpLeftLeft;
			case VehicleDirection12.UpRightUp:
				return VehicleDirection12Flags.UpRightUp;
			case VehicleDirection12.UpRightRight:
				return VehicleDirection12Flags.UpRightRight;

			case VehicleDirection12.DownLeftDown:
				return VehicleDirection12Flags.DownLeftDown;
			case VehicleDirection12.DownLeftLeft:
				return VehicleDirection12Flags.DownLeftLeft;
			case VehicleDirection12.DownRightDown:
				return VehicleDirection12Flags.DownRightDown;
			case VehicleDirection12.DownRightRight:
				return VehicleDirection12Flags.DownRightRight;
			default:
				return 0;
		}
	}

	public static TileEnterDirection ResultEnter(TileExitDirection direction)
	{
		switch (direction)
		{
			case TileExitDirection.Up:
				return TileEnterDirection.Down;
			case TileExitDirection.Down:
				return TileEnterDirection.Up;
			case TileExitDirection.Left:
				return TileEnterDirection.Right;
			case TileExitDirection.Right:
				return TileEnterDirection.Left;
			default:
				return TileEnterDirection.Down;
		}
	}
	public static int RailDirectionFlagsCount(int railDirectionFlags)
	{
		int count = 0;
		if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.DownLeft)) != 0) { count++; }
		if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.DownRight)) != 0) { count++; }
		if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.Horizontal)) != 0) { count++; }
		if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.UpLeft)) != 0) { count++; }
		if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.UpRight)) != 0) { count++; }
		if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.Vertical)) != 0) { count++; }
		return count;
	}

	public static int ToVehicleDirection12Flags_(VehicleDirection12[] directions, int directionsCount)
	{
		int flags = VehicleDirection12Flags.None;
		for (int i = 0; i < directionsCount; i++)
		{
			VehicleDirection12 d = directions[i];
			flags = flags | DirectionUtils.ToVehicleDirection12Flags(d);
		}
		return flags;
	}

	/// <summary>
	/// Enter at TileEnterDirection.Left -> yields VehicleDirection12.UpLeftUp,
	/// VehicleDirection12.HorizontalRight,
	/// VehicleDirection12.DownLeftDown
	/// </summary>
	/// <param name="enter_at"></param>
	/// <returns></returns>
	public static VehicleDirection12[] PossibleNewRails3(TileEnterDirection enter_at)
	{
		VehicleDirection12[] ret = new VehicleDirection12[3];
		switch (enter_at)
		{
			case TileEnterDirection.Left:
				ret[0] = VehicleDirection12.UpLeftUp;
				ret[1] = VehicleDirection12.HorizontalRight;
				ret[2] = VehicleDirection12.DownLeftDown;
				break;
			case TileEnterDirection.Down:
				ret[0] = VehicleDirection12.DownLeftLeft;
				ret[1] = VehicleDirection12.VerticalUp;
				ret[2] = VehicleDirection12.DownRightRight;
				break;
			case TileEnterDirection.Up:
				ret[0] = VehicleDirection12.UpLeftLeft;
				ret[1] = VehicleDirection12.VerticalDown;
				ret[2] = VehicleDirection12.UpRightRight;
				break;
			case TileEnterDirection.Right:
				ret[0] = VehicleDirection12.UpRightUp;
				ret[1] = VehicleDirection12.HorizontalLeft;
				ret[2] = VehicleDirection12.DownRightDown;
				break;
			default:
				return null;
		}
		return ret;
	}
}

public class ClientInventoryController : IInventoryController
{
	public static ClientInventoryController Create(Game game)
	{
		ClientInventoryController c = new ClientInventoryController();
		c.g = game;
		return c;
	}

	Game g;

	public override void InventoryClick(Packet_InventoryPosition pos)
	{
		g.InventoryClick(pos);
	}

	public override void WearItem(Packet_InventoryPosition from, Packet_InventoryPosition to)
	{
		g.WearItem(from, to);
	}

	public override void MoveToInventory(Packet_InventoryPosition from)
	{
		g.MoveToInventory(from);
	}
}

public enum CameraType
{
	Fpp,
	Tpp,
	Overhead
}

public enum TypingState
{
	None,
	Typing,
	Ready
}

public class Player
{
	public Player()
	{
		AnimationHint_ = new AnimationHint();
		Model_ = "player.txt";
		EyeHeight = DefaultEyeHeight();
		ModelHeight = DefaultModelHeight();
		CurrentTexture = -1;
	}
	internal bool PositionLoaded;
	internal float PositionX;
	internal float PositionY;
	internal float PositionZ;
	internal byte Heading;
	internal byte Pitch;
	internal string Name;
	internal AnimationHint AnimationHint_;
	internal PlayerType Type;
	internal int MonsterType;
	internal int Health;
	internal int LastUpdateMilliseconds;
	internal string Model_;
	internal string Texture;
	internal float EyeHeight;
	internal float ModelHeight;
	internal float NetworkX;
	internal float NetworkY;
	internal float NetworkZ;
	internal byte NetworkHeading;
	internal byte NetworkPitch;
	internal PlayerDrawInfo playerDrawInfo;
	internal bool moves;
	internal int CurrentTexture;
	internal HttpResponseCi SkinDownloadResponse;

	public float DefaultEyeHeight()
	{
		float one = 1;
		return one * 15 / 10;
	}

	public float DefaultModelHeight()
	{
		float one = 1;
		return one * 17 / 10;
	}
}

public enum PlayerType
{
	Player,
	Monster
}

public class Grenade_
{
	internal float velocityX;
	internal float velocityY;
	internal float velocityZ;
	internal int block;
	internal int sourcePlayer;
}

public class GetCameraMatrix : IGetCameraMatrix
{
	internal float[] lastmvmatrix;
	internal float[] lastpmatrix;
	public override float[] GetModelViewMatrix()
	{
		return lastmvmatrix;
	}

	public override float[] GetProjectionMatrix()
	{
		return lastpmatrix;
	}
}

public class MenuState
{
	internal int selected;
}

public enum EscapeMenuState
{
	Main,
	Options,
	Graphics,
	Keys,
	Other
}

public class MapLoadingProgressEventArgs
{
	internal int ProgressPercent;
	internal int ProgressBytes;
	internal string ProgressStatus;
}

public class Draw2dData
{
	internal float x1;
	internal float y1;
	internal float width;
	internal float height;
	internal IntRef inAtlasId;
	internal int color;
}

public class ITerrainTextures
{
	internal Game game;

	public int texturesPacked() { return game.texturesPacked(); }
	public int terrainTexture() { return game.terrainTexture; }
	public int[] terrainTextures1d() { return game.terrainTextures1d; }
	public int terrainTexturesPerAtlas() { return game.terrainTexturesPerAtlas; }
}

public class Config3d
{
	public Config3d()
	{
		ENABLE_BACKFACECULLING = true;
		ENABLE_TRANSPARENCY = true;
		ENABLE_MIPMAPS = true;
		ENABLE_VISIBILITY_CULLING = false;
		viewdistance = 128;
	}
	internal bool ENABLE_BACKFACECULLING;
	internal bool ENABLE_TRANSPARENCY;
	internal bool ENABLE_MIPMAPS;
	internal bool ENABLE_VISIBILITY_CULLING;
	internal float viewdistance;
	public float GetViewDistance() { return viewdistance; }
	public void SetViewDistance(float value) { viewdistance = value; }
	public bool GetEnableTransparency() { return ENABLE_TRANSPARENCY; }
	public void SetEnableTransparency(bool value) { ENABLE_TRANSPARENCY = value; }
	public bool GetEnableMipmaps() { return ENABLE_MIPMAPS; }
	public void SetEnableMipmaps(bool value) { ENABLE_MIPMAPS = value; }
}

public abstract class AviWriterCi
{
	public abstract void Open(string filename, int framerate, int width, int height);
	public abstract void AddFrame(BitmapCi bitmap);
	public abstract void Close();
}

public abstract class EntityScript
{
	public virtual void OnNewFrameFixed(Game game, int entity, float dt) { }
}

public class OnUseEntityArgs
{
	internal int entityId;
}

public class ClientCommandArgs
{
	internal string command;
	internal string arguments;
}

public class TextureAtlasCi
{
	public static void TextureCoords2d(int textureId, int texturesPacked, RectFRef r)
	{
		float one = 1;
		r.y = (one / texturesPacked * (textureId / texturesPacked));
		r.x = (one / texturesPacked * (textureId % texturesPacked));
		r.w = one / texturesPacked;
		r.h = one / texturesPacked;
	}
}

public class StackMatrix4
{
	public StackMatrix4()
	{
		values = new float[max][];
		for (int i = 0; i < max; i++)
		{
			values[i] = Mat4.Create();
		}
	}
	float[][] values;
	const int max = 1024;
	int count_;

	internal void Push(float[] p)
	{
		Mat4.Copy(values[count_], p);
		count_++;
	}

	internal float[] Peek()
	{
		return values[count_ - 1];
	}

	internal int Count()
	{
		return count_;
	}

	internal float[] Pop()
	{
		float[] ret = values[count_ - 1];
		count_--;
		return ret;
	}
}

public class CachedTexture
{
	internal int textureId;
	internal float sizeX;
	internal float sizeY;
	internal int lastuseMilliseconds;
}

public class CachedTextTexture
{
	internal Text_ text;
	internal CachedTexture texture;
}

public class GameDataMonsters
{
	public GameDataMonsters()
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
	internal string[] MonsterName;
	internal string[] MonsterCode;
	internal string[] MonsterSkin;
}

public enum GuiState
{
	Normal,
	EscapeMenu,
	Inventory,
	MapLoading,
	CraftingRecipes,
	ModalDialog
}

public enum BlockSetMode
{
	Destroy,
	Create,
	Use, //open doors, use crafting table, etc.
	UseWithTool
}

public enum FontType
{
	Nice,
	Simple,
	BlackBackground,
	Default
}

public class SpecialBlockId
{
	public const int Empty = 0;
}

public class OptionsCi
{
	public OptionsCi()
	{
		float one = 1;
		Shadows = false;
		Font = 0;
		DrawDistance = 32;
		UseServerTextures = true;
		EnableSound = true;
		EnableAutoJump = false;
		ClientLanguage = "";
		Framerate = 0;
		Resolution = 0;
		Fullscreen = false;
		Smoothshadows = true;
		BlockShadowSave = one * 6 / 10;
		EnableBlockShadow = true;
		Keys = new int[256];
	}
	internal bool Shadows;
	internal int Font;
	internal int DrawDistance;
	internal bool UseServerTextures;
	internal bool EnableSound;
	internal bool EnableAutoJump;
	internal string ClientLanguage;
	internal int Framerate;
	internal int Resolution;
	internal bool Fullscreen;
	internal bool Smoothshadows;
	internal float BlockShadowSave;
	internal bool EnableBlockShadow;
	internal int[] Keys;
}

public class TextureAtlas
{
	public static RectFRef TextureCoords2d(int textureId, int texturesPacked)
	{
		float one = 1;
		RectFRef r = new RectFRef();
		r.y = (one / texturesPacked * (textureId / texturesPacked));
		r.x = (one / texturesPacked * (textureId % texturesPacked));
		r.w = one / texturesPacked;
		r.h = one / texturesPacked;
		return r;
	}
}
