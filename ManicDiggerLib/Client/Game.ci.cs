public class Game
{
    public Game()
    {
        chunksize = 16;
        p = GamePlatform.Create();
        player = new CharacterPhysicsState();
    }

    internal GamePlatform p;
    internal Packet_BlockType[] blocktypes;

    internal Chunk[] chunks;
    internal int MapSizeX;
    internal int MapSizeY;
    internal int MapSizeZ;
    internal int chunksize;

    internal CharacterPhysicsState player;

    public bool IsFluid(Packet_BlockType block)
    {
        return block.DrawType == Packet_DrawTypeEnum.Fluid;
    }

    public bool IsEmptyForPhysics(Packet_BlockType block)
    {
        return (block.DrawType == Packet_DrawTypeEnum.Ladder)
            || (block.WalkableType != Packet_WalkableTypeEnum.Solid && block.WalkableType != Packet_WalkableTypeEnum.Fluid);
    }

    internal int GetBlock(int x, int y, int z)
    {
        if (!IsValidPos(x, y, z))
        {
            return 0;
        }

        int cx = x / chunksize;
        int cy = y / chunksize;
        int cz = z / chunksize;
        int chunkpos = MapUtilCi.Index3d(cx, cy, cz, MapSizeX / chunksize, MapSizeY / chunksize);
        if (chunks[chunkpos] == null)
        {
            return 0;
        }
        else
        {
            return chunks[chunkpos].data[MapUtilCi.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)];
        }
    }

    internal bool IsValidPos(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0)
        {
            return false;
        }
        if (x >= MapSizeX || y >= MapSizeY || z >= MapSizeZ)
        {
            return false;
        }
        return true;
    }

    internal int blockheight(int x, int y)
    {
        for (int z = MapSizeZ - 1; z >= 0; z--)
        {
            if (GetBlock(x, y, z) != 0)
            {
                return z + 1;
            }
        }
        return MapSizeZ / 2;
    }

    internal bool IsValidChunkPos(int cx, int cy, int cz, int chunksize_)
    {
        return cx >= 0 && cy >= 0 && cz >= 0
            && cx < MapSizeX / chunksize_
            && cy < MapSizeY / chunksize_
            && cz < MapSizeZ / chunksize_;
    }
}

public class Chunk
{
    internal int[] data;
    internal int LastUpdate;
    internal bool IsPopulated;
    internal int LastChange;
}

public class MapUtilCi
{
    public static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
}
