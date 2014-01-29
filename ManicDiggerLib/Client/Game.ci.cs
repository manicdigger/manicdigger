﻿public class Game
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

    public int GetBlock(int x, int y, int z)
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
            return GetBlockInChunk(chunks[chunkpos], MapUtilCi.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize));
        }
    }

    public int GetBlockInChunk(Chunk chunk, int pos)
    {
        if (chunk.dataInt != null)
        {
            return chunk.dataInt[pos];
        }
        else
        {
            return chunk.data[pos];
        }
    }

    public void SetBlockRaw(int x, int y, int z, int tileType)
    {
        Chunk chunk = GetChunk(x, y, z);
        int pos = MapUtilCi.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize);
        SetBlockInChunk(chunk, pos, tileType);
    }

    public void SetBlockInChunk(Chunk chunk, int pos, int block)
    {
        if (chunk.dataInt == null)
        {
            if (block < 255)
            {
                chunk.data[pos] = IntToByte(block);
            }
            else
            {
                int n = chunksize * chunksize * chunksize;
                chunk.dataInt = new int[n];
                for (int i = 0; i < n; i++)
                {
                    chunk.dataInt[i] = chunk.data[i];
                }
                chunk.data = null;

                chunk.dataInt[pos] = block;
            }
        }
        else
        {
            chunk.dataInt[pos] = block;
        }
    }

    internal bool ChunkHasData(Chunk chunk)
    {
        return chunk.data != null || chunk.dataInt != null;
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        x = x / chunksize;
        y = y / chunksize;
        z = z / chunksize;
        int mapsizexchunks = MapSizeX / chunksize;
        int mapsizeychunks = MapSizeY / chunksize;
        Chunk chunk = chunks[MapUtilCi.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)];
        if (chunk == null)
        {
            Chunk c = new Chunk();
            c.data = new byte[chunksize * chunksize * chunksize];
            chunks[MapUtilCi.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)] = c;
            return chunks[MapUtilCi.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)];
        }
        return chunk;
    }

    public bool IsValidPos(int x, int y, int z)
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

    public int blockheight(int x, int y)
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

    public bool IsValidChunkPos(int cx, int cy, int cz, int chunksize_)
    {
        return cx >= 0 && cy >= 0 && cz >= 0
            && cx < MapSizeX / chunksize_
            && cy < MapSizeY / chunksize_
            && cz < MapSizeZ / chunksize_;
    }

    public void CopyChunk(Chunk chunk, int[] output)
    {
        int n = chunksize * chunksize * chunksize;
        if (chunk.dataInt != null)
        {
            for (int i = 0; i < n; i++)
            {
                output[i] = chunk.dataInt[i];
            }
        }
        else
        {
            for (int i = 0; i < n; i++)
            {
                output[i] = chunk.data[i];
            }
        }
    }

    public static byte IntToByte(int a)
    {
#if CITO
        return a.LowByte;
#else
        return (byte)a;
#endif
    }
}

public class Chunk
{
    internal byte[] data;
    internal int[] dataInt;
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