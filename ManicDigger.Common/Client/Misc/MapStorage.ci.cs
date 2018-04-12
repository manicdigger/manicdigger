public abstract class IMapStorage2
{
	public abstract int GetMapSizeX();
	public abstract int GetMapSizeY();
	public abstract int GetMapSizeZ();
	public abstract int GetBlock(int x, int y, int z);
	public abstract void SetBlock(int x, int y, int z, int tileType);
}

public class MapStorage2 : IMapStorage2
{
	public static MapStorage2 Create(Game game)
	{
		MapStorage2 s = new MapStorage2();
		s.game = game;
		return s;
	}
	Game game;
	public override int GetMapSizeX()
	{
		return game.map.MapSizeX;
	}

	public override int GetMapSizeY()
	{
		return game.map.MapSizeY;
	}

	public override int GetMapSizeZ()
	{
		return game.map.MapSizeZ;
	}

	public override int GetBlock(int x, int y, int z)
	{
		return game.map.GetBlock(x, y, z);
	}

	public override void SetBlock(int x, int y, int z, int tileType)
	{
		game.SetBlock(x, y, z, tileType);
	}
}
