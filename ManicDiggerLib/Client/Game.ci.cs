public class Game
{
    public Game()
    {
        chunksize = 16;
        player = new CharacterPhysicsState();

        TextureId = new int[MaxBlockTypes][];
        for (int i = 0; i < MaxBlockTypes; i++)
        {
            TextureId[i] = new int[6];
        }
        TextureIdForInventory = new int[MaxBlockTypes];
        language = new Language();
        lastplacedblockX = -1;
        lastplacedblockY = -1;
        lastplacedblockZ = -1;
        mLightLevels = new float[16];
        sunlight_ = 15;
    }

    const int MaxBlockTypes = 1024;

    internal GamePlatform p;
    internal Packet_BlockType[] blocktypes;
    internal Language language;
    internal TerrainChunkTesselatorCi d_TerrainChunkTesselator;

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

    public static int ColorFromArgb(int a, int r, int g, int b)
    {
        int iCol = (a << 24) | (r << 16) | (g << 8) | b;
        return iCol;
    }

    public static byte ColorA(int color)
    {
        byte a = IntToByte(color >> 24);
        return a;
    }

    public static byte ColorR(int color)
    {
        byte r = IntToByte(color >> 16);
        return r;
    }

    public static byte ColorG(int color)
    {
        byte g = IntToByte(color >> 8);
        return g;
    }

    public static byte ColorB(int color)
    {
        byte b = IntToByte(color);
        return b;
    }

    public static float GetPi()
    {
        float a = 3141592;
        return a / 1000000;
    }

    //Indexed by block id and TileSide.
    internal int[][] TextureId;
    internal int[] TextureIdForInventory;

    internal int terrainTexturesPerAtlas;

    internal static int MaxInt(int a, byte b)
    {
        if (a >= b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public void GetMapPortion(int[] outPortion, int x, int y, int z, int portionsizex, int portionsizey, int portionsizez)
    {
        int outPortionCount = portionsizex * portionsizey * portionsizez;
        for (int i = 0; i < outPortionCount; i++)
        {
            outPortion[i] = 0;
        }

        //int chunksizebits = p.FloatToInt(p.MathLog(chunksize, 2));
        if (chunksize != 16)
        {
            p.ThrowException("GetMapPortion");
        }
        int chunksizebits = 4;
        int mapchunksx = MapSizeX / chunksize;
        int mapchunksy = MapSizeY / chunksize;
        int mapchunksz = MapSizeZ / chunksize;
        int mapsizechunks = mapchunksx * mapchunksy * mapchunksz;

        for (int xx = 0; xx < portionsizex; xx++)
        {
            for (int yy = 0; yy < portionsizey; yy++)
            {
                for (int zz = 0; zz < portionsizez; zz++)
                {
                    //Find chunk.
                    int cx = (x + xx) >> chunksizebits;
                    int cy = (y + yy) >> chunksizebits;
                    int cz = (z + zz) >> chunksizebits;
                    //int cpos = MapUtil.Index3d(cx, cy, cz, MapSizeX / chunksize, MapSizeY / chunksize);
                    int cpos = (cz * mapchunksy + cy) * mapchunksx + cx;
                    //if (cpos < 0 || cpos >= ((MapSizeX / chunksize) * (MapSizeY / chunksize) * (MapSizeZ / chunksize)))
                    if (cpos < 0 || cpos >= mapsizechunks)
                    {
                        continue;
                    }
                    Chunk chunk = chunks[cpos];
                    if (chunk == null || !ChunkHasData(chunk))
                    {
                        continue;
                    }
                    //int pos = MapUtil.Index3d((x + xx) % chunksize, (y + yy) % chunksize, (z + zz) % chunksize, chunksize, chunksize);
                    int chunkGlobalX = cx << chunksizebits;
                    int chunkGlobalY = cy << chunksizebits;
                    int chunkGlobalZ = cz << chunksizebits;

                    int inChunkX = (x + xx) - chunkGlobalX;
                    int inChunkY = (y + yy) - chunkGlobalY;
                    int inChunkZ = (z + zz) - chunkGlobalZ;

                    //int pos = MapUtil.Index3d(inChunkX, inChunkY, inChunkZ, chunksize, chunksize);
                    int pos = (((inChunkZ << chunksizebits) + inChunkY) << chunksizebits) + inChunkX;

                    int block = GetBlockInChunk(chunk, pos);
                    //outPortion[MapUtil.Index3d(xx, yy, zz, portionsizex, portionsizey)] = (byte)block;
                    outPortion[(zz * portionsizey + yy) * portionsizex + xx] = block;
                }
            }
        }
    }
    internal int texturesPacked() { return GlobalVar.MAX_BLOCKTYPES_SQRT; } //16x16
    internal int terrainTexture;
    internal Texture[] terrainTextures1d;
    internal ITerrainTextures d_TerrainTextures;

    internal int lastplacedblockX;
    internal int lastplacedblockY;
    internal int lastplacedblockZ;

    internal InfiniteMapChunked2d d_Heightmap;
    internal Config3d d_Config3d;

    //maps light level (0-15) to GL.Color value.
    internal float[] mLightLevels;
    internal MeshBatcher d_Batcher;
    internal int sunlight_;

    public void Draw2dTexture(Texture textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int atlastextures, int color, bool enabledepthtest)
    {
        RectFRef rect = RectFRef.Create(0, 0, 1, 1);
        if (inAtlasId != null)
        {
            TextureAtlasCi.TextureCoords2d(inAtlasId.value, atlastextures, rect);
        }
        p.GlDisableCullFace();
        p.GlEnableTexture2d();
        p.BindTexture2d(textureid);

        if (!enabledepthtest)
        {
            p.GlDisableDepthTest();
        }
        Model model = p.CreateModel(QuadModelData.GetQuadModelData2(rect.x, rect.y, rect.w, rect.h,
            x1, y1, width, height, Game.ColorR(color), Game.ColorG(color), Game.ColorB(color), Game.ColorA(color)));
        p.DrawModel(model);
        p.DeleteModel(model);
        if (!enabledepthtest)
        {
            p.GlEnableDepthTest();
        }
        p.GlDisableCullFace();
        p.GlEnableTexture2d();
    }

    public ModelData CombineModelData(ModelData[] modelDatas, int count)
    {
        ModelData ret = new ModelData();
        int totalIndices = 0;
        int totalVertices = 0;
        for (int i = 0; i < count; i++)
        {
            ModelData m = modelDatas[i];
            totalIndices += m.indicesCount;
            totalVertices += m.verticesCount;
        }
        ret.indices = new int[totalIndices];
        ret.xyz = new float[totalVertices * 3];
        ret.uv = new float[totalVertices * 2];
        ret.rgba = new byte[totalVertices * 4];

        for (int i = 0; i < count; i++)
        {
            ModelData m = modelDatas[i];
            int retVerticesCount = ret.verticesCount;
            int retIndicesCount = ret.indicesCount;
            for (int k = 0; k < m.indicesCount; k++)
            {
                ret.indices[ret.indicesCount++] = m.indices[k] + retVerticesCount;
            }
            for (int k = 0; k < m.verticesCount * 3; k++)
            {
                ret.xyz[retVerticesCount * 3 + k] = m.xyz[k];
            }
            for (int k = 0; k < m.verticesCount * 2; k++)
            {
                ret.uv[retVerticesCount * 2 + k] = m.uv[k];
            }
            for (int k = 0; k < m.verticesCount * 4; k++)
            {
                ret.rgba[retVerticesCount * 4 + k] = m.rgba[k];
            }
            ret.verticesCount += m.verticesCount;
        }
        return ret;
    }

    public void Draw2dTextures(Draw2dData[] todraw, int todrawLength, Texture textureid, float angle)
    {
        ModelData[] modelDatas = new ModelData[512];
        int modelDatasCount = 0;
        for (int i = 0; i < todrawLength; i++)
        {
            Draw2dData d = todraw[i];
            float x1 = d.x1;
            float y1 = d.y1;
            float width = d.width;
            float height = d.height;
            IntRef inAtlasId = d.inAtlasId;
            Texture textureId = textureid;
            int color = d.color;

            RectFRef rect = RectFRef.Create(0, 0, 1, 1);
            if (inAtlasId != null)
            {
                TextureAtlasCi.TextureCoords2d(inAtlasId.value, texturesPacked(), rect);
            }

            ModelData modelData =
                QuadModelData.GetQuadModelData2(rect.x, rect.y, rect.w, rect.h,
                x1, y1, width, height, ColorR(color), ColorG(color), ColorB(color), ColorA(color));
            modelDatas[modelDatasCount++] = modelData;
        }

        ModelData combined = CombineModelData(modelDatas, modelDatasCount);

        p.GlDisableCullFace();
        p.GlEnableTexture2d();
        p.BindTexture2d(textureid);

        p.GlDisableDepthTest();

        Model model = p.CreateModel(combined);
        p.DrawModel(model);
        p.DeleteModel(model);

        p.GlEnableDepthTest();

        p.GlDisableCullFace();
        p.GlEnableTexture2d();
    }
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

public class Chunk
{
    internal byte[] data;
    internal int[] dataInt;
    internal int LastUpdate;
    internal bool IsPopulated;
    internal int LastChange;
    internal RenderedChunk rendered;
}

public class RenderedChunk
{
    public RenderedChunk()
    {
        dirty = true;
        shadowsdirty = true;
    }
    internal int[] ids;
    internal int idsCount;
    internal bool dirty;
    internal bool shadowsdirty;
    internal byte[] light;
}

public class ITerrainTextures
{
    internal Game game;

    public int texturesPacked() { return game.texturesPacked(); }
    public int terrainTexture() { return game.terrainTexture; }
    public Texture[] terrainTextures1d() { return game.terrainTextures1d; }
    public int terrainTexturesPerAtlas() { return game.terrainTexturesPerAtlas; }
}

public class Config3d
{
    public Config3d()
    {
        ENABLE_BACKFACECULLING = true;
        ENABLE_TRANSPARENCY = true;
        ENABLE_MIPMAPS = true;
        ENABLE_VSYNC = false;
        ENABLE_VISIBILITY_CULLING = false;
        viewdistance = 128;
    }
    internal bool ENABLE_BACKFACECULLING;
    internal bool ENABLE_TRANSPARENCY;
    internal bool ENABLE_MIPMAPS;
    internal bool ENABLE_VSYNC;
    internal bool ENABLE_VISIBILITY_CULLING;
    internal float viewdistance;
    public float GetViewDistance() { return viewdistance; }
    public void SetViewDistance(float value) { viewdistance = value; }
}

public class MapUtilCi
{
    public static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }

    public static int Index2d(int x, int y, int sizex)
    {
        return x + y * sizex;
    }

    public static void Pos(int index, int sizex, int sizey, Vector3Ref ret)
    {
        int x = index % sizex;
        int y = (index / sizex) % sizey;
        int h = index / (sizex * sizey);
        ret.X = x;
        ret.Y = y;
        ret.Z = h;
    }

    internal static void PosInt(int index, int sizex, int sizey, Vector3IntRef ret)
    {
        int x = index % sizex;
        int y = (index / sizex) % sizey;
        int h = index / (sizex * sizey);
        ret.X = x;
        ret.Y = y;
        ret.Z = h;
    }
}

public class InfiniteMapChunked2d
{
    internal Game d_Map;
    public const int chunksize = 16;
    internal int[][] chunks;
    public int GetBlock(int x, int y)
    {
        int[] chunk = GetChunk(x, y);
        return chunk[MapUtilCi.Index2d(x % chunksize, y % chunksize, chunksize)];
    }
    public int[] GetChunk(int x, int y)
    {
        int[] chunk = null;
        int kx = x / chunksize;
        int ky = y / chunksize;
        if (chunks[MapUtilCi.Index2d(kx, ky, d_Map.MapSizeX / chunksize)] == null)
        {
            chunk = new int[chunksize * chunksize];// (byte*)Marshal.AllocHGlobal(chunksize * chunksize);
            for (int i = 0; i < chunksize * chunksize; i++)
            {
                chunk[i] = 0;
            }
            chunks[MapUtilCi.Index2d(kx, ky, d_Map.MapSizeX / chunksize)] = chunk;
        }
        chunk = chunks[MapUtilCi.Index2d(kx, ky, d_Map.MapSizeX / chunksize)];
        return chunk;
    }
    public void SetBlock(int x, int y, int blocktype)
    {
        GetChunk(x, y)[MapUtilCi.Index2d(x % chunksize, y % chunksize, chunksize)] = blocktype;
    }
    public void Restart()
    {
        //chunks = new byte[d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize][,];
        int n = (d_Map.MapSizeX / chunksize) * (d_Map.MapSizeY / chunksize);
        chunks = new int[n][];//(byte**)Marshal.AllocHGlobal(n * sizeof(IntPtr));
        for (int i = 0; i < n; i++)
        {
            chunks[i] = null;
        }
    }
    public void ClearChunk(int x, int y)
    {
        int px = x / chunksize;
        int py = y / chunksize;
        chunks[MapUtilCi.Index2d(px, py, d_Map.MapSizeX / chunksize)] = null;
    }
}

public abstract class ClientModManager
{
    public abstract void MakeScreenshot();
    public abstract void SetLocalPosition(float glx, float gly, float glz);
    public abstract float GetLocalPositionX();
    public abstract float GetLocalPositionY();
    public abstract float GetLocalPositionZ();
    public abstract void SetLocalOrientation(float glx, float gly, float glz);
    public abstract float GetLocalOrientationX();
    public abstract float GetLocalOrientationY();
    public abstract float GetLocalOrientationZ();
    public abstract void DisplayNotification(string message);
    public abstract void SendChatMessage(string message);
    public abstract GamePlatform GetPlatform();
    public abstract void ShowGui(int level);
    public abstract void SetFreemove(int level);
    public abstract int GetFreemove();
    public abstract BitmapCi GrabScreenshot();
    public abstract AviWriterCi AviWriterCreate();
    public abstract int GetWindowWidth();
    public abstract int GetWindowHeight();
    public abstract bool IsFreemoveAllowed();
    public abstract void EnableCameraControl(bool enable);
}

public abstract class AviWriterCi
{
    public abstract void Open(string filename, int framerate, int width, int height);
    public abstract void AddFrame(BitmapCi bitmap);
    public abstract void Close();
}

public class BitmapCi
{
    public virtual void Dispose(){}
}

public class FreemoveLevelEnum
{
    public const int None = 0;
    public const int Freemove = 1;
    public const int Noclip = 2;
}

public abstract class ClientMod
{
    public abstract void Start(ClientModManager modmanager);
    public virtual bool OnClientCommand(ClientCommandArgs args) { return false; }
    public virtual void OnNewFrame(NewFrameEventArgs args) { }
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
