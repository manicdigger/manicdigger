#region Using Statements
using System;
using System.IO;
using ManicDigger;
using ProtoBuf;
using System.Collections.Generic;
using Jint.Delegates;
#endregion


[ProtoContract()]
public class Monster
{
    [ProtoMember(1, IsRequired = false)]
    public int Id;
    [ProtoMember(2, IsRequired = false)]
    public int MonsterType;
    [ProtoMember(3, IsRequired = false)]
    public int X;
    [ProtoMember(4, IsRequired = false)]
    public int Y;
    [ProtoMember(5, IsRequired = false)]
    public int Z;
    public int Health;
    public Vector3i WalkDirection;
    public float WalkProgress = 0;
}
[ProtoContract()]
public class ServerChunk
{
    [ProtoMember(1, IsRequired = false)]
    public byte[] dataOld;
    [ProtoMember(6, IsRequired = false)]
    public ushort[] data;
    [ProtoMember(2, IsRequired = false)]
    public long LastUpdate;
    [ProtoMember(3, IsRequired = false)]
    public bool IsPopulated;
    [ProtoMember(4, IsRequired = false)]
    public int LastChange;
    public bool DirtyForSaving;
    [ProtoMember(5, IsRequired = false)]
    public List<Monster> Monsters = new List<Monster>();
    [ProtoMember(7, IsRequired = false)]
    public int EntitiesCount;
    [ProtoMember(8, IsRequired = false)]
    public ServerEntity[] Entities;
}

public class ServerMap : IMapStorage2
{
    internal Server server;
    internal IChunkDb d_ChunkDb;
    internal ICurrentTime d_CurrentTime;
    internal ServerChunk[][] chunks;
    internal bool wasChunkGenerated;

    internal int MapSizeX;
    internal int MapSizeY;
    internal int MapSizeZ;
    public override int GetMapSizeX() { return MapSizeX; }
    public override int GetMapSizeY() { return MapSizeY; }
    public override int GetMapSizeZ() { return MapSizeZ; }
    public override int GetBlock(int x, int y, int z)
    {
        ServerChunk chunk = GetChunk(x, y, z);
        //return chunk.data[MapUtilCi.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)];
        unchecked
        {
            return chunk.data[MapUtilCi.Index3d(moduloChunk(x), moduloChunk(y), moduloChunk(z), chunksize, chunksize)];
        }
    }
    public override void SetBlock(int x, int y, int z, int tileType)
    {
        ServerChunk chunk = GetChunk(x, y, z);
        unchecked
        {
            chunk.data[MapUtilCi.Index3d(moduloChunk(x), moduloChunk(y),moduloChunk( z), chunksize, chunksize)] = (ushort)tileType;
        }
        //chunk.data[MapUtilCi.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)] = (ushort)tileType;
        chunk.LastChange = d_CurrentTime.GetSimulationCurrentFrame();
        chunk.DirtyForSaving = true;
        UpdateColumnHeight(x, y);
    }
    void UpdateColumnHeight(int x, int y)
    {
        //todo faster
        int height = MapSizeZ - 1;
        for (int i = MapSizeZ - 1; i >= 0; i--)
        {
            height = i;
            if (MapUtil.IsValidPos(this, x, y, i) && !Server.IsTransparentForLight(server.BlockTypes[GetBlock(x, y, i)]))
            {
                break;
            }
        }
        d_Heightmap.SetBlock(x, y, height);
    }
    public void SetBlockNotMakingDirty(int x, int y, int z, int tileType)
    {
        ServerChunk chunk = GetChunk(x, y, z);
        unchecked
        {
            chunk.data[MapUtilCi.Index3d(moduloChunk(x), moduloChunk(y), moduloChunk( z), chunksize, chunksize)] = (ushort)tileType;
        }
        //chunk.data[MapUtilCi.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize)] = (ushort)tileType;
        chunk.DirtyForSaving = true;
        UpdateColumnHeight(x, y);
    }

    public void LoadChunk(int cx, int cy, int cz)
    {
        ServerChunk chunk = GetChunkValid(cx, cy, cz);
        if (chunk == null)
        {
            GetChunk(cx * chunksize, cy * chunksize, cz * chunksize);
        }
    }

    public ServerChunk GetChunk_(int chunkx, int chunky, int chunkz)
    {
        return GetChunk(chunkx * chunksize, chunky * chunksize, chunkz * chunksize);
    }

    public ServerChunk GetChunk(int x, int y, int z)
    {
        x = invertChunk(x);// / chunksize;
        y = invertChunk(y); // / chunksize;
        z = invertChunk(z); // / chunksize;
        ServerChunk chunk = GetChunkValid(x, y, z);
        if (chunk == null)
        {
            wasChunkGenerated = true;
            unchecked
            {
                byte[] serializedChunk = ChunkDb.GetChunk(d_ChunkDb, x, y, z);
                if (serializedChunk != null)
                {
                    SetChunkValid(x, y, z, DeserializeChunk(serializedChunk));
                    //todo get heightmap from disk
                    UpdateChunkHeight(x, y, z);
                    return GetChunkValid(x, y, z);
                }

                // get chunk
                ushort[] newchunk = new ushort[chunksize * chunksize * chunksize];
                for (int i = 0; i < server.modEventHandlers.getchunk.Count; i++)
                {
                    server.modEventHandlers.getchunk[i](x, y, z, newchunk);
                }
                SetChunkValid(x, y, z, new ServerChunk() { data = newchunk });
                GetChunkValid(x, y, z).DirtyForSaving = true;
                UpdateChunkHeight(x, y, z);
                return GetChunkValid(x, y, z);
            }
        }
        return chunk;
    }

    void UpdateChunkHeight(int x, int y, int z)
    {
        unchecked
        {
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    //UpdateColumnHeight(x * chunksize + xx, y * chunksize + yy);

                    int inChunkHeight = GetColumnHeightInChunk(GetChunkValid(x, y, z).data, xx, yy);
                    if (inChunkHeight != 0)//not empty column
                    {
                        int oldHeight = d_Heightmap.GetBlock(x * chunksize + xx, y * chunksize + yy);
                        d_Heightmap.SetBlock(x * chunksize + xx, y * chunksize + yy, Math.Max(oldHeight, inChunkHeight + z * chunksize));
                    }
                }
            }
        }
    }

    int GetColumnHeightInChunk(ushort[] chunk, int xx, int yy)
    {
        int height = chunksize - 1;
        unchecked
        {
            for (int i = chunksize - 1; i >= 0; i--)
            {
                height = i;
                if (!Server.IsTransparentForLight(server.BlockTypes[chunk[MapUtilCi.Index3d(xx, yy, i, chunksize, chunksize)]]))
                {
                    break;
                }
            }
            return height;
        }
    }

    ServerChunk DeserializeChunk(byte[] serializedChunk)
    {
        ServerChunk c = Serializer.Deserialize<ServerChunk>(new MemoryStream(serializedChunk));
        unchecked
        {
            //convert savegame to new format
            if (c.dataOld != null)
            {
                c.data = new ushort[chunksize * chunksize * chunksize];
                for (int i = 0; i < c.dataOld.Length; i++)
                {
                    c.data[i] = c.dataOld[i];
                }
                c.dataOld = null;
            }
            if (c.Entities != null)
            {
                c.EntitiesCount = c.Entities.Length;
            }
            return c;
        }
    }
    private int chunksize = 16;
    private double invertedChunkSize = 1.0 / 16;
    private bool isPower2Chunk = true;
    public int ChunkSize
    {
        get { return chunksize; }
        set
        {
            chunksize = value;
            invertedChunkSize = 1.0 / chunksize;
            isPower2Chunk = (chunksize & (chunksize - 1)) == 0 && chunksize != 0;
        }
    }
    public int moduloChunk(int num)
    {
        if (isPower2Chunk)
            return num & (chunksize-1);
        else
            return num % chunksize;
    }
    public int invertChunk(int num)
    {
        return (int)(num * invertedChunkSize);
    }

    public void Reset(int sizex, int sizey, int sizez)
    {
        MapSizeX = sizex;
        MapSizeY = sizey;
        MapSizeZ = sizez;
        chunks = new ServerChunk[invertChunk(sizex) * invertChunk(sizey)][];
        //chunks = new ServerChunk[(sizex / chunksize) * (sizey / chunksize)][];
        d_Heightmap.Restart();
    }

    public InfiniteMapChunked2dServer d_Heightmap;
    public unsafe ushort[] GetHeightmapChunk(int x, int y)
    {
        //todo don't copy
        unchecked
        {
            ushort[] chunk2d = d_Heightmap.GetChunk(x, y);
            ushort[] chunk = new ushort[chunksize * chunksize];
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    chunk[MapUtilCi.Index2d(xx, yy, chunksize)] = chunk2d[MapUtilCi.Index2d(xx, yy, chunksize)];
                }
            }
            //todo ushort[]
            return chunk;
        }
    }

    public ServerChunk GetChunkValid(int cx, int cy, int cz)
    {
        unchecked
        {
            ServerChunk[] column = chunks[MapUtilCi.Index2d(cx, cy,invertChunk( MapSizeX))];
            //ServerChunk[] column = chunks[MapUtilCi.Index2d(cx, cy, MapSizeX / chunksize)];
            if (column == null)
            {
                return null;
            }
            return column[cz];
        }
    }

    public void SetChunkValid(int cx, int cy, int cz, ServerChunk chunk)
    {
        unchecked
        {
            ServerChunk[] column = chunks[MapUtilCi.Index2d(cx, cy, invertChunk(MapSizeX))];
            if (column == null)
            {
                column = new ServerChunk[invertChunk(MapSizeZ)];
                chunks[MapUtilCi.Index2d(cx, cy, invertChunk( MapSizeX))] = column;
            }
            column[cz] = chunk;
        }
    }

    public void Clear()
    {
        Array.Clear(chunks, 0, chunks.Length);
    }
}
