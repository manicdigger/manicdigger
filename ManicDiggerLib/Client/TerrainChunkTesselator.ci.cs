public class TerrainChunkTesselatorCi
{
    public TerrainChunkTesselatorCi()
    {
        one = 1;
        EnableSmoothLight = true;
        ENABLE_TEXTURE_TILING = true;
    }
    internal Game game;

    const int chunksize = 16;

    internal int[] currentChunk18;
    internal byte[] currentChunkShadows18;
    internal byte[] currentChunkDraw16;
    internal byte[][] currentChunkDrawCount16;

    internal bool started;
    internal int mapsizex; //cache
    internal int mapsizey;
    internal int mapsizez;

    internal int terrainTexturesPerAtlas;
    internal float terrainTexturesPerAtlasInverse;
    internal const int maxlight = 15;
    internal float maxlightInverse;
    internal bool[] istransparent;
    internal float[] lightlevels;

    internal ModelData[] toreturnatlas1d;
    internal ModelData[] toreturnatlas1dtransparent;

    float one;
    public void Start()
    {
        currentChunk18 = new int[(chunksize + 2) * (chunksize + 2) * (chunksize + 2)];
        currentChunkShadows18 = new byte[(chunksize + 2) * (chunksize + 2) * (chunksize + 2)];
        currentChunkDraw16 = new byte[chunksize * chunksize * chunksize];
        currentChunkDrawCount16 = new byte[chunksize * chunksize * chunksize][];
        mapsizex = game.MapSizeX;
        mapsizey = game.MapSizeY;
        mapsizez = game.MapSizeZ;
        started = true;
        istransparent = new bool[GlobalVar.MAX_BLOCKTYPES];
        maxlightInverse = one / maxlight;
        terrainTexturesPerAtlas = game.terrainTexturesPerAtlas;
        terrainTexturesPerAtlasInverse = one / game.terrainTexturesPerAtlas;

        int toreturnatlas1dLength = Max(1, GlobalVar.MAX_BLOCKTYPES / game.terrainTexturesPerAtlas);
        toreturnatlas1d = new ModelData[toreturnatlas1dLength];
        toreturnatlas1dtransparent = new ModelData[toreturnatlas1dLength];
        for (int i = 0; i < toreturnatlas1dLength; i++)
        {
            toreturnatlas1d[i] = new ModelData();
            toreturnatlas1d[i].xyz = new float[65536 * 3];
            toreturnatlas1d[i].uv = new float[65536 * 2];
            toreturnatlas1d[i].rgba = new byte[65536 * 4];
            toreturnatlas1d[i].indices = new int[65536];

            toreturnatlas1dtransparent[i] = new ModelData();
            toreturnatlas1dtransparent[i].xyz = new float[65536 * 3];
            toreturnatlas1dtransparent[i].uv = new float[65536 * 2];
            toreturnatlas1dtransparent[i].rgba = new byte[65536 * 4];
            toreturnatlas1dtransparent[i].indices = new int[65536];
        }
    }

    int Max(int a, int b)
    {
        if (a > b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public bool IsWater(int tt2)
    {
        return game.IsFluid(game.blocktypes[tt2]);
    }

    public void CalculateVisibleFaces(int[] currentChunk)
    {
        int movez = (chunksize + 2) * (chunksize + 2);
        //unsafe
        {
            int[] currentChunk_ = currentChunk;
            bool[] istransparent_ = istransparent;
            {
                for (int zz = 1; zz < chunksize + 1; zz++)
                {
                    for (int yy = 1; yy < chunksize + 1; yy++)
                    {
                        int posstart = MapUtilCi.Index3d(0, yy, zz, chunksize + 2, chunksize + 2);
                        for (int xx = 1; xx < chunksize + 1; xx++)
                        {
                            int pos = posstart + xx;
                            int tt = currentChunk_[pos];
                            if (tt == 0) { continue; }
                            int draw = TileSideFlagsEnum.None;
                            //Instead of calculating position index with MapUtil.Index(),
                            //relative moves are used
                            //(just addition instead of multiplication - 1.5x - 2x faster)
                            //z+1
                            {
                                int pos2 = pos + movez;
                                int tt2 = currentChunk_[pos2];
                                if (tt2 == 0
                                    || (IsWater(tt2) && (!IsWater(tt)))
                                    || istransparent_[tt2])
                                {
                                    draw |= TileSideFlagsEnum.Top;
                                }
                            }
                            //z-1
                            {
                                int pos2 = pos - movez;
                                int tt2 = currentChunk_[pos2];
                                if (tt2 == 0
                                    || (IsWater(tt2) && (!IsWater(tt)))
                                    || istransparent_[tt2])
                                {
                                    draw |= TileSideFlagsEnum.Bottom;
                                }
                            }
                            //x-1
                            {
                                int pos2 = pos - 1;
                                int tt2 = currentChunk_[pos2];
                                if (tt2 == 0
                                    || (IsWater(tt2) && (!IsWater(tt)))
                                    || istransparent_[tt2])
                                {
                                    draw |= TileSideFlagsEnum.Front;
                                }
                            }
                            //x+1
                            {
                                int pos2 = pos + 1;
                                int tt2 = currentChunk_[pos2];
                                if (tt2 == 0
                                    || (IsWater(tt2) && (!IsWater(tt)))
                                    || istransparent_[tt2])
                                {
                                    draw |= TileSideFlagsEnum.Back;
                                }
                            }
                            //y-1
                            {
                                int pos2 = pos - (chunksize + 2);
                                int tt2 = currentChunk_[pos2];
                                if (tt2 == 0
                                    || (IsWater(tt2) && (!IsWater(tt)))
                                    || istransparent_[tt2])
                                {
                                    draw |= TileSideFlagsEnum.Left;
                                }
                            }
                            //y-1
                            {
                                int pos2 = pos + (chunksize + 2);
                                int tt2 = currentChunk_[pos2];
                                if (tt2 == 0
                                    || (IsWater(tt2) && (!IsWater(tt)))
                                    || istransparent_[tt2])
                                {
                                    draw |= TileSideFlagsEnum.Right;
                                }
                            }
                            currentChunkDraw16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)] = Game.IntToByte(draw);
                        }
                    }
                }
            }
        }
    }

    internal bool EnableSmoothLight;

    public void CalculateTilingCount(int[] currentChunk, int startx, int starty, int startz)
    {
        for (int i = 0; i < chunksize * chunksize * chunksize; i++)
        {
            if (currentChunkDrawCount16[i] == null)
            {
                currentChunkDrawCount16[i] = new byte[6];
            }
            currentChunkDrawCount16[i][0] = 0;
            currentChunkDrawCount16[i][1] = 0;
            currentChunkDrawCount16[i][2] = 0;
            currentChunkDrawCount16[i][3] = 0;
            currentChunkDrawCount16[i][4] = 0;
            currentChunkDrawCount16[i][5] = 0;
        }
        //unsafe
        {
            int[] currentChunk_ = currentChunk;
            for (int zz = 1; zz < chunksize + 1; zz++)
            {
                for (int yy = 1; yy < chunksize + 1; yy++)
                {
                    int pos = MapUtilCi.Index3d(0, yy, zz, chunksize + 2, chunksize + 2);
                    for (int xx = 1; xx < chunksize + 1; xx++)
                    {
                        int tt = currentChunk_[pos + xx];
                        if (tt == 0) { continue; } //faster
                        int x = startx + xx - 1;
                        int y = starty + yy - 1;
                        int z = startz + zz - 1;
                        int draw = currentChunkDraw16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)];
                        if (draw == 0) { continue; } //faster
                        if (EnableSmoothLight)
                        {
                            if ((draw & TileSideFlagsEnum.Top) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Top] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Top, TileSideFlags.Top);
                            }
                            if ((draw & TileSideFlagsEnum.Bottom) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Bottom] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Bottom, TileSideFlags.Bottom);
                            }
                            if ((draw & TileSideFlagsEnum.Front) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Front] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Front, TileSideFlags.Front);
                            }
                            if ((draw & TileSideFlagsEnum.Back) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Back] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Back, TileSideFlags.Back);
                            }
                            if ((draw & TileSideFlagsEnum.Left) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Left] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Left, TileSideFlags.Left);
                            }
                            if ((draw & TileSideFlagsEnum.Right) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Right] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Right, TileSideFlags.Right);
                            }
                        }
                        else
                        {
                            if ((draw & TileSideFlagsEnum.Top) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Top] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Top, TileSideFlagsEnum.Top));
                            }
                            if ((draw & TileSideFlagsEnum.Bottom) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Bottom] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Bottom, TileSideFlagsEnum.Bottom));
                            }
                            if ((draw & TileSideFlagsEnum.Front) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Front] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Front, TileSideFlagsEnum.Front));
                            }
                            if ((draw & TileSideFlagsEnum.Back) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Back] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Back, TileSideFlagsEnum.Back));
                            }
                            if ((draw & TileSideFlagsEnum.Left) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Left] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Left, TileSideFlagsEnum.Left));
                            }
                            if ((draw & TileSideFlagsEnum.Right) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
                                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Right] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Right, TileSideFlagsEnum.Right));
                            }
                        }
                    }
                }
            }
        }
    }

    // tiling reduces number of triangles but causes white dots bug on some graphics cards.
    internal bool ENABLE_TEXTURE_TILING;
    //Texture tiling in one direction.
    public int GetTilingCount(int[] currentChunk, int xx, int yy, int zz, int tt, int x, int y, int z, int shadowratio, int dir, int dirflags)
    {
        if (!ENABLE_TEXTURE_TILING)
        {
            return 1;
        }
        //fixes tree Z-fighting
        if (istransparent[currentChunk[MapUtilCi.Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)]]
            && !IsTransparentFully(currentChunk[MapUtilCi.Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)])) { return 1; }
        if (dir == TileSideEnum.Top || dir == TileSideEnum.Bottom)
        {
            int shadowz = dir == TileSideEnum.Top ? 1 : -1;
            int newxx = xx + 1;
            for (; ; )
            {
                if (newxx >= chunksize + 1) { break; }
                if (currentChunk[MapUtilCi.Index3d(newxx, yy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                int shadowratio2 = GetShadowRatio(newxx, yy, zz + shadowz, x + (newxx - xx), y, z + shadowz);
                if (shadowratio != shadowratio2) { break; }
                if ((currentChunkDraw16[MapUtilCi.Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)] & dirflags) == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                currentChunkDrawCount16[MapUtilCi.Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)][dir] = 0;
                currentChunkDraw16[MapUtilCi.Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)] &= Game.IntToByte(~dirflags);
                newxx++;
            }
            return newxx - xx;
        }
        else if (dir == TileSideEnum.Front || dir == TileSideEnum.Back)
        {
            int shadowx = dir == TileSideEnum.Front ? -1 : 1;
            int newyy = yy + 1;
            for (; ; )
            {
                if (newyy >= chunksize + 1) { break; }
                if (currentChunk[MapUtilCi.Index3d(xx, newyy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                int shadowratio2 = GetShadowRatio(xx + shadowx, newyy, zz, x + shadowx, y + (newyy - yy), z);
                if (shadowratio != shadowratio2) { break; }
                if ((currentChunkDraw16[MapUtilCi.Index3d(xx - 1, newyy - 1, zz - 1, chunksize, chunksize)] & dirflags) == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                currentChunkDrawCount16[MapUtilCi.Index3d(xx - 1, newyy - 1, zz - 1, chunksize, chunksize)][dir] = 0;
                currentChunkDraw16[MapUtilCi.Index3d(xx - 1, newyy - 1, zz - 1, chunksize, chunksize)] &= Game.IntToByte(~dirflags);
                newyy++;
            }
            return newyy - yy;
        }
        else
        {
            int shadowy = dir == TileSideEnum.Left ? -1 : 1;
            int newxx = xx + 1;
            for (; ; )
            {
                if (newxx >= chunksize + 1) { break; }
                if (currentChunk[MapUtilCi.Index3d(newxx, yy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                int shadowratio2 = GetShadowRatio(newxx, yy + shadowy, zz, x + (newxx - xx), y + shadowy, z);
                if (shadowratio != shadowratio2) { break; }
                if ((currentChunkDraw16[MapUtilCi.Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)] & dirflags) == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                currentChunkDrawCount16[MapUtilCi.Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)][dir] = 0;
                currentChunkDraw16[MapUtilCi.Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)] &= Game.IntToByte(~dirflags);
                newxx++;
            }
            return newxx - xx;
        }
    }

    public bool IsTransparentFully(int p)
    {
        Packet_BlockType b = game.blocktypes[p];
        return (b.DrawType != Packet_DrawTypeEnum.Solid) && (b.DrawType != Packet_DrawTypeEnum.Plant)
             && (b.DrawType != Packet_DrawTypeEnum.OpenDoorLeft) && (b.DrawType != Packet_DrawTypeEnum.OpenDoorRight) && (b.DrawType != Packet_DrawTypeEnum.ClosedDoor);
    }

    public int GetShadowRatio(int xx, int yy, int zz, int globalx, int globaly, int globalz)
    {
        return currentChunkShadows18[MapUtilCi.Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];
    }
}

public class TileSideEnum
{
    public const int Top = 0;
    public const int Bottom = 1;
    public const int Front = 2;
    public const int Back = 3;
    public const int Left = 4;
    public const int Right = 5;
}

public class TileSideFlagsEnum
{
    public const int None = 0;
    public const int Top = 1;
    public const int Bottom = 2;
    public const int Front = 4;
    public const int Back = 8;
    public const int Left = 16;
    public const int Right = 32;
}

public class GlobalVar
{
    public const int MAX_BLOCKTYPES = 1024;
    public const int MAX_BLOCKTYPES_SQRT = 32;
}
