// Generates triangles for a single 16x16x16 chunk.
// Needs to know the surrounding of the chunk (18x18x18 blocks total).
// This class is heavily inlined and unrolled for performance.
// Special-shape (rare) blocks don't need as much performance.
public class TerrainChunkTesselatorCi
{
    public TerrainChunkTesselatorCi()
    {
        one = 1;
        EnableSmoothLight = true;
        ENABLE_TEXTURE_TILING = true;
        ColorWhite = Game.ColorFromArgb(255, 255, 255, 255);
        BlockShadow = one * 6 / 10;
        DONOTDRAWEDGES = true;
        AtiArtifactFix = one * 995 / 1000; // 0.995f;
        occ = one * 7 / 10; // 0.7f
        halfocc = one * 4 / 10; // 0.4f

        Yellowness = 1; // lower is yellower//0.7
        Blueness = one * 9 / 10; // lower is blue-er
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
    internal bool[] ishalfheight;
    internal float[] lightlevels;

    internal ModelData[] toreturnatlas1d;
    internal ModelData[] toreturnatlas1dtransparent;

    internal float BlockShadow;
    internal bool DONOTDRAWEDGES;
    internal float AtiArtifactFix;

    internal float Yellowness;
    internal float Blueness;

    float one;

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

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
        ishalfheight = new bool[GlobalVar.MAX_BLOCKTYPES];
        maxlightInverse = one / maxlight;
        terrainTexturesPerAtlas = game.terrainTexturesPerAtlas;
        terrainTexturesPerAtlasInverse = one / game.terrainTexturesPerAtlas;

        toreturnatlas1dLength = Max(1, GlobalVar.MAX_BLOCKTYPES / game.terrainTexturesPerAtlas);
        toreturnatlas1d = new ModelData[toreturnatlas1dLength];
        toreturnatlas1dtransparent = new ModelData[toreturnatlas1dLength];
        for (int i = 0; i < toreturnatlas1dLength; i++)
        {
            toreturnatlas1d[i] = new ModelData();
            int max = 1024;
            toreturnatlas1d[i].xyz = new float[max * 3];
            toreturnatlas1d[i].uv = new float[max * 2];
            toreturnatlas1d[i].rgba = new byte[max * 4];
            toreturnatlas1d[i].indices = new int[max];
            toreturnatlas1d[i].verticesMax = max;
            toreturnatlas1d[i].indicesMax = max;

            toreturnatlas1dtransparent[i] = new ModelData();
            toreturnatlas1dtransparent[i].xyz = new float[max * 3];
            toreturnatlas1dtransparent[i].uv = new float[max * 2];
            toreturnatlas1dtransparent[i].rgba = new byte[max * 4];
            toreturnatlas1dtransparent[i].indices = new int[max];
            toreturnatlas1dtransparent[i].verticesMax = max;
            toreturnatlas1dtransparent[i].indicesMax = max;
        }
    }
    int toreturnatlas1dLength;

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
            bool[] ishalfheight_ = ishalfheight;
            {
                for (int zz = 1; zz < chunksize + 1; zz++)
                {
                    for (int yy = 1; yy < chunksize + 1; yy++)
                    {
                        int posstart = Index3d(0, yy, zz, chunksize + 2, chunksize + 2);
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
                                    || istransparent_[tt2]
                                    || ishalfheight_[tt])
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
                            currentChunkDraw16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)] = Game.IntToByte(draw);
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
                    int pos = Index3d(0, yy, zz, chunksize + 2, chunksize + 2);
                    for (int xx = 1; xx < chunksize + 1; xx++)
                    {
                        int tt = currentChunk_[pos + xx];
                        if (tt == 0) { continue; } //faster
                        int x = startx + xx - 1;
                        int y = starty + yy - 1;
                        int z = startz + zz - 1;
                        int draw = currentChunkDraw16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)];
                        if (draw == 0) { continue; } //faster
                        if (EnableSmoothLight)
                        {
                            if ((draw & TileSideFlagsEnum.Top) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Top] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Top, TileSideFlags.Top);
                            }
                            if ((draw & TileSideFlagsEnum.Bottom) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Bottom] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Bottom, TileSideFlags.Bottom);
                            }
                            if ((draw & TileSideFlagsEnum.Front) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Front] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Front, TileSideFlags.Front);
                            }
                            if ((draw & TileSideFlagsEnum.Back) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Back] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Back, TileSideFlags.Back);
                            }
                            if ((draw & TileSideFlagsEnum.Left) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Left] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Left, TileSideFlags.Left);
                            }
                            if ((draw & TileSideFlagsEnum.Right) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Right] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Right, TileSideFlags.Right);
                            }
                        }
                        else
                        {
                            if ((draw & TileSideFlagsEnum.Top) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Top] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Top, TileSideFlagsEnum.Top));
                            }
                            if ((draw & TileSideFlagsEnum.Bottom) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Bottom] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Bottom, TileSideFlagsEnum.Bottom));
                            }
                            if ((draw & TileSideFlagsEnum.Front) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Front] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Front, TileSideFlagsEnum.Front));
                            }
                            if ((draw & TileSideFlagsEnum.Back) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Back] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Back, TileSideFlagsEnum.Back));
                            }
                            if ((draw & TileSideFlagsEnum.Left) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Left] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Left, TileSideFlagsEnum.Left));
                            }
                            if ((draw & TileSideFlagsEnum.Right) != 0)
                            {
                                int shadowratioTop = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Right] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Right, TileSideFlagsEnum.Right));
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
        if (istransparent[currentChunk[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)]]
            && !IsTransparentFully(currentChunk[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)])) { return 1; }
        if (dir == TileSideEnum.Top || dir == TileSideEnum.Bottom)
        {
            int shadowz = dir == TileSideEnum.Top ? 1 : -1;
            int newxx = xx + 1;
            for (; ; )
            {
                if (newxx >= chunksize + 1) { break; }
                if (currentChunk[Index3d(newxx, yy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                int shadowratio2 = GetShadowRatio(newxx, yy, zz + shadowz, x + (newxx - xx), y, z + shadowz);
                if (shadowratio != shadowratio2) { break; }
                if ((currentChunkDraw16[Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)] & dirflags) == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                currentChunkDrawCount16[Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)][dir] = 0;
                currentChunkDraw16[Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)] &= Game.IntToByte(~dirflags);
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
                if (currentChunk[Index3d(xx, newyy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                int shadowratio2 = GetShadowRatio(xx + shadowx, newyy, zz, x + shadowx, y + (newyy - yy), z);
                if (shadowratio != shadowratio2) { break; }
                if ((currentChunkDraw16[Index3d(xx - 1, newyy - 1, zz - 1, chunksize, chunksize)] & dirflags) == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                currentChunkDrawCount16[Index3d(xx - 1, newyy - 1, zz - 1, chunksize, chunksize)][dir] = 0;
                currentChunkDraw16[Index3d(xx - 1, newyy - 1, zz - 1, chunksize, chunksize)] &= Game.IntToByte(~dirflags);
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
                if (currentChunk[Index3d(newxx, yy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                int shadowratio2 = GetShadowRatio(newxx, yy + shadowy, zz, x + (newxx - xx), y + shadowy, z);
                if (shadowratio != shadowratio2) { break; }
                if ((currentChunkDraw16[Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)] & dirflags) == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                currentChunkDrawCount16[Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)][dir] = 0;
                currentChunkDraw16[Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)] &= Game.IntToByte(~dirflags);
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
        return currentChunkShadows18[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];
    }

    public void CalculateBlockPolygons(int x, int y, int z)
    {
        for (int xx = 0; xx < chunksize; xx++)
        {
            for (int yy = 0; yy < chunksize; yy++)
            {
                for (int zz = 0; zz < chunksize; zz++)
                {
                    int xxx = x * chunksize + xx;
                    int yyy = y * chunksize + yy;
                    int zzz = z * chunksize + zz;
                    //Most blocks aren't rendered at all, quickly reject them.
                    if (currentChunkDraw16[Index3d(xx, yy, zz, chunksize, chunksize)] != 0)
                    {
                        BlockPolygons(xxx, yyy, zzz, currentChunk18);
                    }
                }
            }
        }
    }

    public void CalculateSmoothBlockPolygons(int x, int y, int z)
    {
        for (int xx = 0; xx < chunksize; xx++)
        {
            for (int yy = 0; yy < chunksize; yy++)
            {
                for (int zz = 0; zz < chunksize; zz++)
                {
                    int xxx = x * chunksize + xx;
                    int yyy = y * chunksize + yy;
                    int zzz = z * chunksize + zz;
                    //Most blocks aren't rendered at all, quickly reject them.
                    if (currentChunkDraw16[Index3d(xx, yy, zz, chunksize, chunksize)] != 0)
                    {
                        SmoothLightBlockPolygons(xxx, yyy, zzz, currentChunk18);
                    }
                }
            }
        }
    }

    internal float texrecLeft;
    internal float texrecTop;
    internal float texrecWidth;
    internal float texrecHeight;
    internal int ColorWhite;

    public void BlockPolygons(int x, int y, int z, int[] currentChunk)
    {
        int xx = x % chunksize + 1;
        int yy = y % chunksize + 1;
        int zz = z % chunksize + 1;
        int tt = currentChunk[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];
        if (!isvalid(tt))
        {
            return;
        }
        byte drawtop = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Top];
        byte drawbottom = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Bottom];
        byte drawfront = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Front];
        byte drawback = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Back];
        byte drawleft = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Left];
        byte drawright = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Right];
        int tiletype = tt;
        if (drawtop == 0 && drawbottom == 0 && drawfront == 0 && drawback == 0 && drawleft == 0 && drawright == 0)
        {
            return;
        }
        int color = ColorWhite; //mapstorage.GetTerrainBlockColor(x, y, z);
        int colorShadowSide = Game.ColorFromArgb(Game.ColorA(color),
            game.platform.FloatToInt(Game.ColorR(color) * BlockShadow),
            game.platform.FloatToInt(Game.ColorG(color) * BlockShadow),
            game.platform.FloatToInt(Game.ColorB(color) * BlockShadow));
        if (DONOTDRAWEDGES)
        {
            //On finite map don't draw borders:
            //they can't be seen without freemove cheat.
            if (z == 0) { drawbottom = 0; }
            if (x == 0) { drawfront = 0; }
            if (x == mapsizex - 1) { drawback = 0; }
            if (y == 0) { drawleft = 0; }
            if (y == mapsizey - 1) { drawright = 0; }
        }
        float flowerfix = 0;
        if (IsFlower(tiletype))
        {
            //Draw nothing but 2 faces. Prevents flickering.
            drawtop = 0;
            drawbottom = 0;
            drawback = 0;
            drawright = 0;
            drawfront = 1;
            drawleft = 1;
            flowerfix = one / 2; // 0.5f;
        }
        if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.OpenDoorLeft)
        {
            drawtop = 0;
            drawbottom = 0;
            flowerfix = one * 9 / 10; // 0.9f;
            //x-1, x+1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0
                && currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                drawback = 0;
                drawfront = 0;
                drawleft = 1;
                drawright = 0;
            }
            //y-1, y+1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0
                && currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                drawback = 1;
                drawfront = 0;
                drawleft = 0;
                drawright = 0;
            }
        }
        if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.OpenDoorRight)
        {
            drawtop = 0;
            drawbottom = 0;
            flowerfix = one * 9 / 10; // 0.9f;
            //x-1, x+1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0
                && currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                drawback = 0;
                drawfront = 0;
                drawleft = 0;
                drawright = 1;
            }
            //y-1, y+1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0
                && currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                drawback = 0;
                drawfront = 1;
                drawleft = 0;
                drawright = 0;
            }
        }
        if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Fence || game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.ClosedDoor) // fence tiles automatically when another fence is beside
        {
            drawtop = 0;
            drawbottom = 0;
            drawfront = 0;
            drawback = 0;
            drawleft = 0;
            drawright = 0;
            flowerfix = one / 2;// 0.5f;

            //x-1, x+1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] != 0
                || currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] != 0)
            {
                drawleft = 1;
            }
            //y-1, y+1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] != 0
                || currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] != 0)
            {
                drawfront = 1;
            }
            if (drawback == 0 && drawfront == 0 && drawleft == 0 && drawright == 0)
            {
                drawback = 1;
                drawleft = 1;
            }
        }
        if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Ladder) // try to fit ladder to best wall or existing ladder
        {
            drawtop = 0;
            drawbottom = 0;
            flowerfix = one * 95 / 100; // 0.95f;
            drawfront = 0;
            drawback = 0;
            drawleft = 0;
            drawright = 0;
            int ladderAtPositionMatchWall = getBestLadderWall(xx, yy, zz, currentChunk);
            if (ladderAtPositionMatchWall < 0)
            {

                int ladderbelow = getBestLadderInDirection(xx, yy, zz, currentChunk, -1);
                int ladderabove = getBestLadderInDirection(xx, yy, zz, currentChunk, 1);

                if (ladderbelow != 0)
                {
                    ladderAtPositionMatchWall = getBestLadderWall(xx, yy, zz + ladderbelow, currentChunk);
                }
                else if (ladderabove != 0)
                {
                    ladderAtPositionMatchWall = getBestLadderWall(xx, yy, zz + ladderabove, currentChunk);
                }
            }
            switch (ladderAtPositionMatchWall)
            {
                case 1: drawleft = 1; break;
                case 2: drawback = 1; break;
                case 3: drawfront = 1; break;
                default: drawright = 1; break;
            }
        }
        int rail = Rail(tiletype);
        float blockheight = 1;//= data.GetTerrainBlockHeight(tiletype);
        if (rail != 0)
        {
            blockheight = one * 3 / 10; // 0.3f;
        }
        if (game.blocktypes[tt].DrawType == Packet_DrawTypeEnum.HalfHeight)
        {
            blockheight = one * 1 / 2; // 0.5f;
        }
        if (game.blocktypes[tt].DrawType == Packet_DrawTypeEnum.Torch)
        {
            int type = TorchTypeEnum.Normal;
            if (CanSupportTorch(currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Front; }
            if (CanSupportTorch(currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Back; }
            if (CanSupportTorch(currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Left; }
            if (CanSupportTorch(currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Right; }
            TorchSideTexture = TextureId(tt, TileSideEnum.Front);
            TorchTopTexture = TextureId(tt, TileSideEnum.Top);
            AddTorch(x, y, z, type, currentChunk[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)]);
            return;
        }
        //slope
        float blockheight00 = blockheight;
        float blockheight01 = blockheight;
        float blockheight10 = blockheight;
        float blockheight11 = blockheight;
        if (rail != 0)
        {
            int slope = GetRailSlope(xx, yy, zz);
            if (slope == RailSlopeEnum.TwoRightRaised)
            {
                blockheight10 += 1;
                blockheight11 += 1;
            }
            if (slope == RailSlopeEnum.TwoLeftRaised)
            {
                blockheight00 += 1;
                blockheight01 += 1;
            }
            if (slope == RailSlopeEnum.TwoUpRaised)
            {
                blockheight00 += 1;
                blockheight10 += 1;
            }
            if (slope == RailSlopeEnum.TwoDownRaised)
            {
                blockheight01 += 1;
                blockheight11 += 1;
            }
        }
        
        //if (tt >= PartialWaterBlock && tt < PartialWaterBlock + waterLevelsCount)
        //{
        //    int waterlevel = tt - PartialWaterBlock;

        //    int[] wl = new int[9];
        //    wl[0] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz, chunksize + 2, chunksize + 2)]);
        //    wl[1] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy - 1, zz, chunksize + 2, chunksize + 2)]);
        //    wl[2] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz, chunksize + 2, chunksize + 2)]);
        //    wl[3] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy + 0, zz, chunksize + 2, chunksize + 2)]);
        //    wl[4] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy + 0, zz, chunksize + 2, chunksize + 2)]);
        //    wl[5] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy + 0, zz, chunksize + 2, chunksize + 2)]);
        //    wl[6] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz, chunksize + 2, chunksize + 2)]);
        //    wl[7] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy + 1, zz, chunksize + 2, chunksize + 2)]);
        //    wl[8] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz, chunksize + 2, chunksize + 2)]);
        //    if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[0] = waterLevelsCount - 1; }
        //    if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy - 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[1] = waterLevelsCount - 1; }
        //    if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[2] = waterLevelsCount - 1; }
        //    if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy + 0, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[3] = waterLevelsCount - 1; }
        //    if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy + 0, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[4] = waterLevelsCount - 1; }
        //    if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy + 0, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[5] = waterLevelsCount - 1; }
        //    if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[6] = waterLevelsCount - 1; }
        //    if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy + 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[7] = waterLevelsCount - 1; }
        //    if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[8] = waterLevelsCount - 1; }

        //    //00: maximum of (-1,-1), (0,-1), (-1,0)
        //    blockheight00 = ((float)Max(waterlevel, wl[0], wl[1], wl[3]) + 1) / waterLevelsCount;
        //    blockheight01 = ((float)Max(waterlevel, wl[3], wl[6], wl[7]) + 1) / waterLevelsCount;
        //    blockheight10 = ((float)Max(waterlevel, wl[1], wl[2], wl[5]) + 1) / waterLevelsCount;
        //    blockheight11 = ((float)Max(waterlevel, wl[5], wl[7], wl[8]) + 1) / waterLevelsCount;

        //    if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx, yy, zz + 1, chunksize + 2, chunksize + 2)]) > 0)
        //    {
        //        blockheight00 = 1;
        //        blockheight01 = 1;
        //        blockheight10 = 1;
        //        blockheight11 = 1;
        //    }
        //}
        
        int curcolor = color;
        texrecLeft = 0;//0
        texrecHeight = terrainTexturesPerAtlasInverse * AtiArtifactFix;
        //top
        if (drawtop > 0)
        {
            curcolor = color;
            int shadowratio = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
            if (shadowratio != maxlight)
            {
                float shadowratiof = lightlevels[shadowratio];
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof));
            }
            int sidetexture = TextureId(tiletype, TileSideEnum.Top);
            int tilecount = drawtop;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix);
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 0, z + blockheight00, y + 0, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 0, z + blockheight01, y + 1, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + blockheight10, y + 0, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + blockheight11, y + 1, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }
        //bottom - same as top, but z is 1 less.
        if (drawbottom > 0)
        {
            curcolor = colorShadowSide;
            int shadowratio = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
            if (shadowratio != maxlight)
            {
                float shadowratiof = lightlevels[shadowratio];
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Min(Game.ColorR(curcolor), Game.ColorR(color) * shadowratiof)),
                    game.platform.FloatToInt(Min(Game.ColorG(curcolor), Game.ColorG(color) * shadowratiof)),
                    game.platform.FloatToInt(Min(Game.ColorB(curcolor), Game.ColorB(color) * shadowratiof)));
            }
            int sidetexture = TextureId(tiletype, TileSideEnum.Bottom);
            int tilecount = drawbottom;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix);
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 0, z, y + 0, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 0, z, y + 1, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z, y + 0, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z, y + 1, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }
        //front
        if (drawfront > 0)
        {
            curcolor = color;
            int shadowratio = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
            if (shadowratio != maxlight)
            {
                float shadowratiof = lightlevels[shadowratio];
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof));
            }
            int sidetexture = TextureId(tiletype, TileSideEnum.Front);
            int tilecount = drawfront;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix);
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 0 + flowerfix, z + 0, y + 0, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 0 + flowerfix, z + 0, y + 1 * tilecount, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 0 + flowerfix, z + blockheight00, y + 0, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 0 + flowerfix, z + blockheight01, y + 1 * tilecount, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }
        //back - same as front, but x is 1 greater.
        if (drawback > 0)
        {
            curcolor = color;
            int shadowratio = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
            if (shadowratio != maxlight)
            {
                float shadowratiof = lightlevels[shadowratio];
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof));
            }
            int sidetexture = TextureId(tiletype, TileSideEnum.Back);
            int tilecount = drawback;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix);
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 1 - flowerfix, z + 0, y + 0, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 - flowerfix, z + 0, y + 1 * tilecount, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 - flowerfix, z + blockheight10, y + 0, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 - flowerfix, z + blockheight11, y + 1 * tilecount, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }
        if (drawleft > 0)
        {
            curcolor = colorShadowSide;
            int shadowratio = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
            if (shadowratio != maxlight)
            {
                float shadowratiof = lightlevels[shadowratio];
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Min(Game.ColorR(curcolor), Game.ColorR(color) * shadowratiof)),
                    game.platform.FloatToInt(Min(Game.ColorG(curcolor), Game.ColorG(color) * shadowratiof)),
                    game.platform.FloatToInt(Min(Game.ColorB(curcolor), Game.ColorB(color) * shadowratiof)));
            }

            int sidetexture = TextureId(tiletype, TileSideEnum.Left);
            int tilecount = drawleft;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix); //tilingcount*fix
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 0, z + 0, y + 0 + flowerfix, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 0, z + blockheight00, y + 0 + flowerfix, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + 0, y + 0 + flowerfix, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + blockheight10, y + 0 + flowerfix, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }
        //right - same as left, but y is 1 greater.
        if (drawright > 0)
        {
            curcolor = colorShadowSide;
            int shadowratio = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
            if (shadowratio != maxlight)
            {
                float shadowratiof = lightlevels[shadowratio];
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Min(Game.ColorR(curcolor), Game.ColorR(color) * shadowratiof)),
                    game.platform.FloatToInt(Min(Game.ColorG(curcolor), Game.ColorG(color) * shadowratiof)),
                    game.platform.FloatToInt(Min(Game.ColorB(curcolor), Game.ColorB(color) * shadowratiof)));
            }

            int sidetexture = TextureId(tiletype, TileSideEnum.Right);
            int tilecount = drawright;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix); //tilingcount*fix
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 0, z + 0, y + 1 - flowerfix, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 0, z + blockheight01, y + 1 - flowerfix, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + 0, y + 1 - flowerfix, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + blockheight11, y + 1 - flowerfix, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }
    }

    internal float occ;
    internal float halfocc;
    internal bool topoccupied;
    internal bool bottomoccupied;
    internal bool leftoccupied;
    internal bool rightoccupied;
    internal bool topleftoccupied;
    internal bool toprightoccupied;
    internal bool bottomleftoccupied;
    internal bool bottomrightoccupied;
    
    public void SmoothLightBlockPolygons(int x, int y, int z, int[] currentChunk)
    {
        int xx = x % chunksize + 1;
        int yy = y % chunksize + 1;
        int zz = z % chunksize + 1;
        int tt = currentChunk[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];
        if (!isvalid(tt))
        {
            return;
        }
        byte drawtop = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Top];
        byte drawbottom = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Bottom];
        byte drawfront = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Front];
        byte drawback = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Back];
        byte drawleft = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Left];
        byte drawright = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Right];
        int tiletype = tt;
        if (drawtop == 0 && drawbottom == 0 && drawfront == 0 && drawback == 0 && drawleft == 0 && drawright == 0)
        {
            return;
        }
        int color = ColorWhite; //mapstorage.GetTerrainBlockColor(x, y, z);
        int colorShadowSide = Game.ColorFromArgb(Game.ColorA(color),
            game.platform.FloatToInt(Game.ColorR(color) * BlockShadow),
            game.platform.FloatToInt(Game.ColorG(color) * BlockShadow),
            game.platform.FloatToInt(Game.ColorB(color) * BlockShadow));
        if (DONOTDRAWEDGES)
        {
            //On finite map don't draw borders:
            //they can't be seen without freemove cheat.
            if (z == 0) { drawbottom = 0; }
            if (x == 0) { drawfront = 0; }
            if (x == mapsizex - 1) { drawback = 0; }
            if (y == 0) { drawleft = 0; }
            if (y == mapsizey - 1) { drawright = 0; }
        }
        float flowerfix = 0;
        if (IsFlower(tiletype))
        {
            //Draw nothing but 2 faces. Prevents flickering.
            drawtop = 0;
            drawbottom = 0;
            drawback = 0;
            drawright = 0;
            drawfront = 1;
            drawleft = 1;
            flowerfix = one / 2; // 0.5f;
        }
        if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.OpenDoorLeft)
        {
            drawtop = 0;
            drawbottom = 0;
            flowerfix = one * 9 / 10; // 0.9f;
            //x-1, x+1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0
                && currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                drawback = 0;
                drawfront = 0;
                drawleft = 1;
                drawright = 0;
            }
            //y-1, y+1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0
                && currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                drawback = 1;
                drawfront = 0;
                drawleft = 0;
                drawright = 0;
            }
        }
        if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.OpenDoorRight)
        {
            drawtop = 0;
            drawbottom = 0;
            flowerfix = one * 9 / 10; // 0.9f;
            //x-1, x+1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0
                && currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                drawback = 0;
                drawfront = 0;
                drawleft = 0;
                drawright = 1;
            }
            //y-1, y+1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0
                && currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                drawback = 0;
                drawfront = 1;
                drawleft = 0;
                drawright = 0;
            }
        }
        if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Fence
            || game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.ClosedDoor) // fence tiles automatically when another fence is beside
        {
            drawtop = 0;
            drawbottom = 0;
            drawfront = 0;
            drawback = 0;
            drawleft = 0;
            drawright = 0;
            flowerfix = one / 2; // 0.5f;

            //x-1, x+1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] != 0
                || currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] != 0)
            {
                drawleft = 1;
            }
            //y-1, y+1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] != 0
                || currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] != 0)
            {
                drawfront = 1;
            }
            if (drawback == 0 && drawfront == 0 && drawleft == 0 && drawright == 0)
            {
                drawback = 1;
                drawleft = 1;
            }
        }
        if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Ladder) // try to fit ladder to best wall or existing ladder
        {
            drawtop = 0;
            drawbottom = 0;
            flowerfix = one * 95 / 100; // 0.95f;
            drawfront = 0;
            drawback = 0;
            drawleft = 0;
            drawright = 0;
            int ladderAtPositionMatchWall = getBestLadderWall(xx, yy, zz, currentChunk);
            if (ladderAtPositionMatchWall < 0)
            {

                int ladderbelow = getBestLadderInDirection(xx, yy, zz, currentChunk, -1);
                int ladderabove = getBestLadderInDirection(xx, yy, zz, currentChunk, 1);

                if (ladderbelow != 0)
                {
                    ladderAtPositionMatchWall = getBestLadderWall(xx, yy, zz + ladderbelow, currentChunk);
                }
                else if (ladderabove != 0)
                {
                    ladderAtPositionMatchWall = getBestLadderWall(xx, yy, zz + ladderabove, currentChunk);
                }
            }
            switch (ladderAtPositionMatchWall)
            {
                case 1: drawleft = 1; break;
                case 2: drawback = 1; break;
                case 3: drawfront = 1; break;
                default: drawright = 1; break;
            }
        }
        int rail = Rail(tiletype);
        float blockheight = 1;//= data.GetTerrainBlockHeight(tiletype);
        if (rail != RailDirectionFlagsEnum.None)
        {
            blockheight = one * 3 / 10; // 0.3f;
            // RailPolygons(myelements, myvertices, x, y, z, rail);
            // return;
        }
        if (game.blocktypes[tt].DrawType == Packet_DrawTypeEnum.HalfHeight)
        {
            blockheight = one / 2; // 0.5f;
        }
        if (game.blocktypes[tt].DrawType == Packet_DrawTypeEnum.Torch)
        {
            int type = TorchTypeEnum.Normal;
            if (CanSupportTorch(currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Front; }
            if (CanSupportTorch(currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Back; }
            if (CanSupportTorch(currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Left; }
            if (CanSupportTorch(currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Right; }
            TorchSideTexture = TextureId(tt, TileSideEnum.Front);
            TorchTopTexture = TextureId(tt, TileSideEnum.Top);
            AddTorch(x, y, z, type, tt);
            return;
        }
        //slope
        float blockheight00 = blockheight;
        float blockheight01 = blockheight;
        float blockheight10 = blockheight;
        float blockheight11 = blockheight;
        if (rail != RailDirectionFlagsEnum.None)
        {
            int slope = GetRailSlope(xx, yy, zz);
            if (slope == RailSlopeEnum.TwoRightRaised)
            {
                blockheight10 += 1;
                blockheight11 += 1;
            }
            if (slope == RailSlopeEnum.TwoLeftRaised)
            {
                blockheight00 += 1;
                blockheight01 += 1;
            }
            if (slope == RailSlopeEnum.TwoUpRaised)
            {
                blockheight00 += 1;
                blockheight10 += 1;
            }
            if (slope == RailSlopeEnum.TwoDownRaised)
            {
                blockheight01 += 1;
                blockheight11 += 1;
            }
        }
        //if stationary water block, make slightly lower than terrain
        if (tt == 8)
        {
            //Only do this, when no other water block is above to prevent gaps
            if (currentChunk[Index3d(xx, yy, zz+1, chunksize + 2, chunksize + 2)] != 8)
            {
                blockheight00 = one * 9 / 10; // 0.9f;
                blockheight01 = one * 9 / 10;
                blockheight10 = one * 9 / 10;
                blockheight11 = one * 9 / 10;
            }
        }
        int curcolor = color;
        int curcolor2 = color;
        int curcolor3 = color;
        int curcolor4 = color;
        texrecLeft = 0;
        texrecHeight = terrainTexturesPerAtlasInverse * AtiArtifactFix;
        //top
        if (drawtop > 0)
        {
            bool occluded = false;
            bool occdirnorthwest = true;
            //bool applysmoothing = true;
            int shadowratio = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
            //if (true)
            {
                int top = currentChunk[Index3d(xx, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                int bottom = currentChunk[Index3d(xx, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                int left = currentChunk[Index3d(xx - 1, yy, zz + 1, chunksize + 2, chunksize + 2)];
                int right = currentChunk[Index3d(xx + 1, yy, zz + 1, chunksize + 2, chunksize + 2)];
                int topleft = currentChunk[Index3d(xx - 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                int topright = currentChunk[Index3d(xx + 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                int bottomleft = currentChunk[Index3d(xx - 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                int bottomright = currentChunk[Index3d(xx + 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                int shadowratio3 = shadowratio;//down
                int shadowratio4 = shadowratio;//right
                int shadowratio5 = shadowratio;//up
                int shadowratio2 = shadowratio;//left
                int shadowratio7 = shadowratio;//leftup
                int shadowratio6 = shadowratio;//rightup
                int shadowratio9 = shadowratio;//leftdown
                int shadowratio8 = shadowratio;//rightdown
                //check occupied blocks
                //todo: if top !=0 { if transparentforlight { etc
                if (top != 0) { if (!IsTransparentForLight(top)) { topoccupied = true; } else { topoccupied = false; } }
                else { topoccupied = false; shadowratio5 = GetShadowRatio(xx, yy - 1, zz + 1, x, y - 1, z + 1); }
                if (topleft != 0) { if (!IsTransparentForLight(topleft)) { topleftoccupied = true; } else { topleftoccupied = false; } }
                else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx - 1, yy - 1, zz + 1, x - 1, y, z + 1); }
                if (topright != 0) { if (!IsTransparentForLight(topright)) { toprightoccupied = true; } else { toprightoccupied = false; } }
                else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx + 1, yy - 1, zz + 1, x - 1, y, z + 1); }
                if (left != 0) { if (!IsTransparentForLight(left)) { leftoccupied = true; } else { leftoccupied = false; } }
                else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx - 1, yy, zz + 1, x - 1, y, z + 1); }
                if (right != 0) { if (!IsTransparentForLight(right)) { rightoccupied = true; } else { rightoccupied = false; } }
                else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx + 1, yy, zz + 1, x + 1, y, z + 1); }
                if (bottom != 0) { if (!IsTransparentForLight(bottom)) { bottomoccupied = true; } else { bottomoccupied = false; } }
                else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx, yy + 1, zz + 1, x, y + 1, z + 1); }
                if (bottomright != 0) { if (!IsTransparentForLight(bottomright)) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx + 1, yy + 1, zz + 1, x - 1, y, z + 1); }
                if (bottomleft != 0) { if (!IsTransparentForLight(bottomleft)) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx - 1, yy + 1, zz + 1, x - 1, y, z + 1); }


                float shadowratiomain = lightlevels[shadowratio];
                float shadowratiof5 = shadowratiomain;
                float shadowratiof4 = shadowratiomain;
                float shadowratiof3 = shadowratiomain;
                float shadowratiof2 = shadowratiomain;

                //get occupied blocks for ao and smoothing

                if (shadowratio9 == shadowratio8 && shadowratio8 == shadowratio7 && shadowratio7 == shadowratio6 &&
                    shadowratio6 == shadowratio5 && shadowratio5 == shadowratio4 && shadowratio4 == shadowratio3 &&
                    shadowratio3 == shadowratio2 && shadowratio2 == shadowratio)
                {
                    //no shadow tiles near, just do occlusion
                    //goto done;
                }
                else
                {
                    //topleft vertex
                    if (leftoccupied && topoccupied) { }
                    else
                    {
                        byte facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio5]; }
                        if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio7]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio2]; }
                        shadowratiof4 /= facesconsidered;
                    }
                    //topright vertex
                    if (topoccupied && rightoccupied) { }
                    else
                    {
                        byte facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio5]; }
                        if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio6]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio4]; }
                        shadowratiof5 /= facesconsidered;
                    }
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { }
                    else
                    {
                        byte facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio3]; }
                        if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio8]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio4]; }
                        shadowratiof3 /= facesconsidered;
                    }
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { }
                    else
                    {
                        byte facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio3]; }
                        if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio9]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio2]; }
                        shadowratiof2 /= facesconsidered;
                    }
                }
            // done:
                //ambient occlusion, corners with 2 blocks get full occlusion, others half
                if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; }
                else
                {
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                }
            // next:
                if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; }
                else
                {
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                }
            // next1:
                if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; }
                else
                {
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                }
            // next2:
                if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; }
                else
                {
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                }
            // next3:
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof2 * Yellowness));

                curcolor2 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof3 * Yellowness));

                curcolor3 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof4 * Yellowness));

                curcolor4 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof5 * Yellowness));
            }
            int sidetexture = TextureId(tiletype, TileSideEnum.Top);
            int tilecount = drawtop;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = AtiArtifactFix; //tilingcount*fix
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 0, z + blockheight00, y + 0, texrecLeft, texrecTop, curcolor3);//leftbottom 4
            ModelDataTool.AddVertex(toreturn, x + 0, z + blockheight01, y + 1, texrecLeft, texrecBottom, curcolor);//rightbottom 2
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + blockheight10, y + 0, texrecRight, texrecTop, curcolor4);//topleft  3
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + blockheight11, y + 1, texrecRight, texrecBottom, curcolor2);//topright  * tilecount

            //revert triangles to fix gradient problem
            //if occluded, revert to proper occlusion direction

            if (occluded)
            {
                if (!occdirnorthwest)
                {
                    ModelDataTool.AddIndex(toreturn,(lastelement + 0));//0
                    ModelDataTool.AddIndex(toreturn,(lastelement + 1));//1
                    ModelDataTool.AddIndex(toreturn,(lastelement + 3));//2
                    ModelDataTool.AddIndex(toreturn,(lastelement + 3));//1
                    ModelDataTool.AddIndex(toreturn,(lastelement + 2));//3
                    ModelDataTool.AddIndex(toreturn,(lastelement + 0));//2
                }
                else
                {
                    ModelDataTool.AddIndex(toreturn,(lastelement + 0));//0
                    ModelDataTool.AddIndex(toreturn,(lastelement + 1));//1
                    ModelDataTool.AddIndex(toreturn,(lastelement + 2));//2
                    ModelDataTool.AddIndex(toreturn,(lastelement + 1));//1
                    ModelDataTool.AddIndex(toreturn,(lastelement + 3));//3
                    ModelDataTool.AddIndex(toreturn,(lastelement + 2));//2
                }
            }

            else if (Game.ColorR(curcolor) != Game.ColorR(curcolor4) || Game.ColorR(curcolor3) == Game.ColorR(curcolor2))
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//2
            }
            else
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
            }
        }



        //bottom - same as top, but z is 1 less.
        if (drawbottom > 0)
        {
            bool occluded = false;
            bool occdirnorthwest = true;
            //bool applysmoothing = true;
            int shadowratio = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
            //if (true)
            {
                int top = currentChunk[Index3d(xx, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                int bottom = currentChunk[Index3d(xx, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                int left = currentChunk[Index3d(xx - 1, yy, zz - 1, chunksize + 2, chunksize + 2)];
                int right = currentChunk[Index3d(xx + 1, yy, zz - 1, chunksize + 2, chunksize + 2)];
                int topleft = currentChunk[Index3d(xx - 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                int topright = currentChunk[Index3d(xx + 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                int bottomleft = currentChunk[Index3d(xx - 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                int bottomright = currentChunk[Index3d(xx + 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                int shadowratio3 = shadowratio;//down
                int shadowratio4 = shadowratio;//right
                int shadowratio5 = shadowratio;//up
                int shadowratio2 = shadowratio;//left
                int shadowratio7 = shadowratio;//leftup
                int shadowratio6 = shadowratio;//rightup
                int shadowratio9 = shadowratio;//leftdown
                int shadowratio8 = shadowratio;//rightdown
                //check occupied blocks
                //todo: if top !=0 { if transparentforlight { etc

                if (top != 0) { if (!IsTransparentForLight(top)) { topoccupied = true; } else { topoccupied = false; } }
                else { topoccupied = false; shadowratio5 = GetShadowRatio(xx, yy + 1, zz - 1, x, y - 1, z - 1); }
                if (topleft != 0) { if (!IsTransparentForLight(topleft)) { topleftoccupied = true; } else { topleftoccupied = false; } }
                else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx - 1, yy + 1, zz - 1, x - 1, y, z - 1); }
                if (topright != 0) { if (!IsTransparentForLight(topright)) { toprightoccupied = true; } else { toprightoccupied = false; } }
                else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx + 1, yy + 1, zz - 1, x - 1, y, z - 1); }
                if (left != 0) { if (!IsTransparentForLight(left)) { leftoccupied = true; } else { leftoccupied = false; } }
                else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx - 1, yy, zz - 1, x - 1, y, z - 1); }
                if (right != 0) { if (!IsTransparentForLight(right)) { rightoccupied = true; } else { rightoccupied = false; } }
                else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx + 1, yy, zz - 1, x + 1, y, z - 1); }
                if (bottom != 0) { if (!IsTransparentForLight(bottom)) { bottomoccupied = true; } else { bottomoccupied = false; } }
                else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx, yy - 1, zz - 1, x, y + 1, z - 1); }
                if (bottomright != 0) { if (!IsTransparentForLight(bottomright)) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx + 1, yy - 1, zz - 1, x - 1, y, z - 1); }
                if (bottomleft != 0) { if (!IsTransparentForLight(bottomleft)) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx - 1, yy - 1, zz - 1, x - 1, y, z - 1); }


                float shadowratiomain = lightlevels[shadowratio];
                float shadowratiof5 = shadowratiomain;
                float shadowratiof4 = shadowratiomain;
                float shadowratiof3 = shadowratiomain;
                float shadowratiof2 = shadowratiomain;

                //get occupied blocks for ao and smoothing

                if (shadowratio9 == shadowratio8 && shadowratio8 == shadowratio7 && shadowratio7 == shadowratio6 &&
                    shadowratio6 == shadowratio5 && shadowratio5 == shadowratio4 && shadowratio4 == shadowratio3 &&
                    shadowratio3 == shadowratio2 && shadowratio2 == shadowratio)
                {
                    //no shadow tiles near, just do occlusion
                    //goto done;
                }
                else
                {
                    //topleft vertex
                    if (leftoccupied && topoccupied) { }
                    else
                    {
                        byte facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio5]; }
                        if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio7]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio2]; }
                        shadowratiof4 /= facesconsidered;
                    }
                // toprightvertex:
                    //topright vertex
                    if (topoccupied && rightoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio5]; }
                        if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio6]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio4]; }
                        shadowratiof5 /= facesconsidered;
                    }
                // bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio3]; }
                        if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio8]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio4]; }
                        shadowratiof3 /= facesconsidered;
                    }
                // bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio3]; }
                        if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio9]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio2]; }
                        shadowratiof2 /= facesconsidered;
                    }
                }
            // done:
                //ambient occlusion, corners with 2 blocks get full occlusion, others half
                if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; }
                else
                {
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                }
            // next:
                if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; }
                else
                {
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                }
            // next1:
                if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; }
                else
                {
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                }
            // next2:
                if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; }
                else
                {
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                }
            // next3:
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof2 * Yellowness));

                curcolor2 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof3 * Yellowness));

                curcolor3 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof4 * Yellowness));

                curcolor4 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof5 * Yellowness));
            }
            int sidetexture = TextureId(tiletype, TileSideEnum.Bottom);
            int tilecount = drawbottom;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = AtiArtifactFix; //tilingcount*fix
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 0, z, y + 0, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 0, z, y + 1, texrecLeft, texrecBottom, curcolor3);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z, y + 0, texrecRight, texrecTop, curcolor2);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z, y + 1, texrecRight, texrecBottom, curcolor4);

            //revert triangles to fix gradient problem
            //if occluded, revert to proper occlusion direction

            if (occluded)
            {
                if (occdirnorthwest)
                {
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//3
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//2
                }
                else
                {
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//3
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                }
            }

            else if (Game.ColorR(curcolor) != Game.ColorR(curcolor4) || Game.ColorR(curcolor3) == Game.ColorR(curcolor2))
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
            }
            else
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//2
            }

        }
        //front
        if (drawfront > 0)
        {
            bool occluded = false;
            bool occdirnorthwest = true;
            //bool applysmoothing = true;
            int shadowratio = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
            //if (true)
            {
                int top = currentChunk[Index3d(xx - 1, yy, zz + 1, chunksize + 2, chunksize + 2)];
                int bottom = currentChunk[Index3d(xx - 1, yy, zz - 1, chunksize + 2, chunksize + 2)];
                int left = currentChunk[Index3d(xx - 1, yy - 1, zz, chunksize + 2, chunksize + 2)];
                int right = currentChunk[Index3d(xx - 1, yy + 1, zz, chunksize + 2, chunksize + 2)];
                int topleft = currentChunk[Index3d(xx - 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                int topright = currentChunk[Index3d(xx - 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                int bottomleft = currentChunk[Index3d(xx - 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                int bottomright = currentChunk[Index3d(xx - 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                int shadowratio3 = shadowratio;//down
                int shadowratio4 = shadowratio;//right
                int shadowratio5 = shadowratio;//up
                int shadowratio2 = shadowratio;//left
                int shadowratio7 = shadowratio;//leftup
                int shadowratio6 = shadowratio;//rightup
                int shadowratio9 = shadowratio;//leftdown
                int shadowratio8 = shadowratio;//rightdown
                //check occupied blocks
                //todo: if top !=0 { if transparentforlight { etc
                if (top != 0) { if (!IsTransparentForLight(top)) { topoccupied = true; } else { topoccupied = false; } }
                else { topoccupied = false; shadowratio5 = GetShadowRatio(xx - 1, yy, zz + 1, x - 1, y, z + 1); }
                if (topleft != 0) { if (!IsTransparentForLight(topleft)) { topleftoccupied = true; } else { topleftoccupied = false; } }
                else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx - 1, yy - 1, zz + 1, x - 1, y - 1, z + 1); }
                if (topright != 0) { if (!IsTransparentForLight(topright)) { toprightoccupied = true; } else { toprightoccupied = false; } }
                else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx - 1, yy + 1, zz + 1, x - 1, y + 1, z + 1); }
                if (left != 0) { if (!IsTransparentForLight(left)) { leftoccupied = true; } else { leftoccupied = false; } }
                else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx - 1, yy - 1, zz, x - 1, y - 1, z); }
                if (right != 0) { if (!IsTransparentForLight(right)) { rightoccupied = true; } else { rightoccupied = false; } }
                else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx - 1, yy + 1, zz, x - 1, y + 1, z); }
                if (bottom != 0) { if (!IsTransparentForLight(bottom)) { bottomoccupied = true; } else { bottomoccupied = false; } }
                else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx - 1, yy, zz - 1, x - 1, y, z - 1); }
                if (bottomright != 0) { if (!IsTransparentForLight(bottomright)) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx - 1, yy + 1, zz - 1, x - 1, y + 1, z - 1); }
                if (bottomleft != 0) { if (!IsTransparentForLight(bottomleft)) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx - 1, yy - 1, zz - 1, x - 1, y - 1, z - 1); }


                float shadowratiomain = lightlevels[shadowratio];
                float shadowratiof5 = shadowratiomain;
                float shadowratiof4 = shadowratiomain;
                float shadowratiof3 = shadowratiomain;
                float shadowratiof2 = shadowratiomain;

                if (shadowratio9 == shadowratio8 && shadowratio8 == shadowratio7 && shadowratio7 == shadowratio6 &&
                    shadowratio6 == shadowratio5 && shadowratio5 == shadowratio4 && shadowratio4 == shadowratio3 &&
                    shadowratio3 == shadowratio2 && shadowratio2 == shadowratio)
                {
                    //no shadow tiles near, just do occlusion
                    //goto done;
                }
                else
                {
                    //topleft vertex
                    if (leftoccupied && topoccupied) { }
                    else
                    {
                        byte facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio5]; }
                        if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio7]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio2]; }
                        shadowratiof4 /= facesconsidered;
                    }
                // toprightvertex:
                    //topright vertex
                    if (topoccupied && rightoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio5]; }
                        if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio6]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio4]; }
                        shadowratiof5 /= facesconsidered;
                    }
                // bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio3]; }
                        if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio8]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio4]; }
                        shadowratiof3 /= facesconsidered;
                    }
                // bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio3]; }
                        if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio9]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio2]; }
                        shadowratiof2 /= facesconsidered;
                    }
                }
            // done:
                //ambient occlusion, corners with 2 blocks get full occlusion, others half
                if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; }
                else
                {
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                }
            // next:
                if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; }
                else
                {
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                }
            // next1:
                if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; }
                else
                {
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                }
            // next2:
                if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; }
                else
                {
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                }
            // next3:
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof2 * Yellowness));

                curcolor2 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof3 * Yellowness));

                curcolor3 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof4 * Yellowness));

                curcolor4 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof5 * Yellowness));
            }
            int sidetexture = TextureId(tiletype, TileSideEnum.Front);
            int tilecount = drawfront;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = AtiArtifactFix; //tilingcount*fix
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 0 + flowerfix, z + 0, y + 0, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 0 + flowerfix, z + 0, y + 1 * tilecount, texrecRight, texrecBottom, curcolor2);
            ModelDataTool.AddVertex(toreturn, x + 0 + flowerfix, z + blockheight00, y + 0, texrecLeft, texrecTop, curcolor3);
            ModelDataTool.AddVertex(toreturn, x + 0 + flowerfix, z + blockheight01, y + 1 * tilecount, texrecRight, texrecTop, curcolor4);
            if (occluded)
            {
                if (!occdirnorthwest)
                {
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//3
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                }
                else
                {
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//3
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//2
                }
            }

            else if (Game.ColorR(curcolor) != Game.ColorR(curcolor4) || Game.ColorR(curcolor3) == Game.ColorR(curcolor2))
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
            }
            else
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//2
            }

        }
        //back - same as front, but x is 1 greater.
        if (drawback > 0)
        {
            bool occluded = false;
            bool occdirnorthwest = true;
            //bool applysmoothing = true;
            int shadowratio = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
            //if (true)
            {
                int top = currentChunk[Index3d(xx + 1, yy, zz + 1, chunksize + 2, chunksize + 2)];
                int bottom = currentChunk[Index3d(xx + 1, yy, zz - 1, chunksize + 2, chunksize + 2)];
                int left = currentChunk[Index3d(xx + 1, yy - 1, zz, chunksize + 2, chunksize + 2)];
                int right = currentChunk[Index3d(xx + 1, yy + 1, zz, chunksize + 2, chunksize + 2)];
                int topleft = currentChunk[Index3d(xx + 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                int topright = currentChunk[Index3d(xx + 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                int bottomleft = currentChunk[Index3d(xx + 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                int bottomright = currentChunk[Index3d(xx + 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                int shadowratio3 = shadowratio;//down
                int shadowratio4 = shadowratio;//right
                int shadowratio5 = shadowratio;//up
                int shadowratio2 = shadowratio;//left
                int shadowratio7 = shadowratio;//leftup
                int shadowratio6 = shadowratio;//rightup
                int shadowratio9 = shadowratio;//leftdown
                int shadowratio8 = shadowratio;//rightdown
                //check occupied blocks
                //todo: if top !=0 { if transparentforlight { etc
                if (top != 0) { if (!IsTransparentForLight(top)) { topoccupied = true; } else { topoccupied = false; } }
                else { topoccupied = false; shadowratio5 = GetShadowRatio(xx + 1, yy, zz + 1, x - 1, y, z + 1); }
                if (topleft != 0) { if (!IsTransparentForLight(topleft)) { topleftoccupied = true; } else { topleftoccupied = false; } }
                else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx + 1, yy - 1, zz + 1, x - 1, y - 1, z + 1); }
                if (topright != 0) { if (!IsTransparentForLight(topright)) { toprightoccupied = true; } else { toprightoccupied = false; } }
                else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx + 1, yy + 1, zz + 1, x - 1, y + 1, z + 1); }
                if (left != 0) { if (!IsTransparentForLight(left)) { leftoccupied = true; } else { leftoccupied = false; } }
                else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx + 1, yy - 1, zz, x - 1, y - 1, z); }
                if (right != 0) { if (!IsTransparentForLight(right)) { rightoccupied = true; } else { rightoccupied = false; } }
                else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx + 1, yy + 1, zz, x - 1, y + 1, z); }
                if (bottom != 0) { if (!IsTransparentForLight(bottom)) { bottomoccupied = true; } else { bottomoccupied = false; } }
                else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx + 1, yy, zz - 1, x - 1, y, z - 1); }
                if (bottomright != 0) { if (!IsTransparentForLight(bottomright)) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx + 1, yy + 1, zz - 1, x - 1, y + 1, z - 1); }
                if (bottomleft != 0) { if (!IsTransparentForLight(bottomleft)) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx + 1, yy - 1, zz - 1, x - 1, y - 1, z - 1); }


                float shadowratiomain = lightlevels[shadowratio];
                float shadowratiof5 = shadowratiomain;
                float shadowratiof4 = shadowratiomain;
                float shadowratiof3 = shadowratiomain;
                float shadowratiof2 = shadowratiomain;

                if (shadowratio9 == shadowratio8 && shadowratio8 == shadowratio7 && shadowratio7 == shadowratio6 &&
                    shadowratio6 == shadowratio5 && shadowratio5 == shadowratio4 && shadowratio4 == shadowratio3 &&
                    shadowratio3 == shadowratio2 && shadowratio2 == shadowratio)
                {
                    //no shadow tiles near, just do occlusion
                    //goto done;
                }
                else
                {
                    //topleft vertex
                    if (leftoccupied && topoccupied) { }
                    else
                    {
                        byte facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio5]; }
                        if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio7]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio2]; }
                        shadowratiof4 /= facesconsidered;
                    }
                // toprightvertex:
                    //topright vertex
                    if (topoccupied && rightoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio5]; }
                        if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio6]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio4]; }
                        shadowratiof5 /= facesconsidered;
                    }
                // bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio3]; }
                        if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio8]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio4]; }
                        shadowratiof3 /= facesconsidered;
                    }
                // bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio3]; }
                        if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio9]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio2]; }
                        shadowratiof2 /= facesconsidered;
                    }
                }
            // done:
                //ambient occlusion, corners with 2 blocks get full occlusion, others half
                if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; }
                else
                {
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                }
            // next:
                if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; }
                else
                {
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                }
            // next1:
                if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; }
                else
                {
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                }
            // next2:
                if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; }
                else
                {
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                }
            // next3:
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof2 * Yellowness));

                curcolor2 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof3 * Yellowness));

                curcolor3 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof4 * Yellowness));

                curcolor4 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(color) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorG(color) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorB(color) * shadowratiof5 * Yellowness));
            }
            int sidetexture = TextureId(tiletype, TileSideEnum.Back);
            int tilecount = drawback;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = AtiArtifactFix; //tilingcount*fix
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 1 - flowerfix, z + 0, y + 0, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 - flowerfix, z + 0, y + 1 * tilecount, texrecLeft, texrecBottom, curcolor2);
            ModelDataTool.AddVertex(toreturn, x + 1 - flowerfix, z + blockheight10, y + 0, texrecRight, texrecTop, curcolor3);
            ModelDataTool.AddVertex(toreturn, x + 1 - flowerfix, z + blockheight11, y + 1 * tilecount, texrecLeft, texrecTop, curcolor4);
            if (occluded)
            {
                if (!occdirnorthwest)
                {
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//3
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                }
                else
                {
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//3
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//2
                }
            }

            else if (Game.ColorR(curcolor) != Game.ColorR(curcolor4) || Game.ColorR(curcolor3) == Game.ColorR(curcolor2))
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
            }
            else
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//2
            }
        }
        if (drawleft > 0)
        {
            bool occluded = false;
            bool occdirnorthwest = true;
            //bool applysmoothing = true;
            int shadowratio = GetShadowRatio(xx, yy - 1, zz, x + 1, y, z);
            //if (true)
            {
                int top = currentChunk[Index3d(xx, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                int bottom = currentChunk[Index3d(xx, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                int left = currentChunk[Index3d(xx + 1, yy - 1, zz, chunksize + 2, chunksize + 2)];
                int right = currentChunk[Index3d(xx - 1, yy - 1, zz, chunksize + 2, chunksize + 2)];
                int topleft = currentChunk[Index3d(xx + 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                int topright = currentChunk[Index3d(xx - 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                int bottomleft = currentChunk[Index3d(xx + 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                int bottomright = currentChunk[Index3d(xx - 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                int shadowratio3 = shadowratio;//down
                int shadowratio4 = shadowratio;//right
                int shadowratio5 = shadowratio;//up
                int shadowratio2 = shadowratio;//left
                int shadowratio7 = shadowratio;//leftup
                int shadowratio6 = shadowratio;//rightup
                int shadowratio9 = shadowratio;//leftdown
                int shadowratio8 = shadowratio;//rightdown
                //check occupied blocks
                //todo: if top !=0 { if transparentforlight { etc
                if (top != 0) { if (!IsTransparentForLight(top)) { topoccupied = true; } else { topoccupied = false; } }
                else { topoccupied = false; shadowratio5 = GetShadowRatio(xx, yy - 1, zz + 1, x - 1, y, z + 1); }
                if (topleft != 0) { if (!IsTransparentForLight(topleft)) { topleftoccupied = true; } else { topleftoccupied = false; } }
                else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx + 1, yy - 1, zz + 1, x - 1, y - 1, z + 1); }
                if (topright != 0) { if (!IsTransparentForLight(topright)) { toprightoccupied = true; } else { toprightoccupied = false; } }
                else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx - 1, yy - 1, zz + 1, x - 1, y + 1, z + 1); }
                if (left != 0) { if (!IsTransparentForLight(left)) { leftoccupied = true; } else { leftoccupied = false; } }
                else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx + 1, yy - 1, zz, x - 1, y - 1, z); }
                if (right != 0) { if (!IsTransparentForLight(right)) { rightoccupied = true; } else { rightoccupied = false; } }
                else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx - 1, yy - 1, zz, x - 1, y + 1, z); }
                if (bottom != 0) { if (!IsTransparentForLight(bottom)) { bottomoccupied = true; } else { bottomoccupied = false; } }
                else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx, yy - 1, zz - 1, x - 1, y, z - 1); }
                if (bottomright != 0) { if (!IsTransparentForLight(bottomright)) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx - 1, yy - 1, zz - 1, x - 1, y + 1, z - 1); }
                if (bottomleft != 0) { if (!IsTransparentForLight(bottomleft)) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx + 1, yy - 1, zz - 1, x - 1, y - 1, z - 1); }


                float shadowratiomain = lightlevels[shadowratio];
                float shadowratiof5 = shadowratiomain;
                float shadowratiof4 = shadowratiomain;
                float shadowratiof3 = shadowratiomain;
                float shadowratiof2 = shadowratiomain;

                if (shadowratio9 == shadowratio8 && shadowratio8 == shadowratio7 && shadowratio7 == shadowratio6 &&
                    shadowratio6 == shadowratio5 && shadowratio5 == shadowratio4 && shadowratio4 == shadowratio3 &&
                    shadowratio3 == shadowratio2 && shadowratio2 == shadowratio)
                {
                    //no shadow tiles near, just do occlusion
                    // goto done;
                }
                else
                {
                    //topleft vertex
                    if (leftoccupied && topoccupied) { }
                    else
                    {
                        byte facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio5]; }
                        if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio7]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio2]; }
                        shadowratiof4 /= facesconsidered;
                    }
                // toprightvertex:
                    //topright vertex
                    if (topoccupied && rightoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio5]; }
                        if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio6]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio4]; }
                        shadowratiof5 /= facesconsidered;
                    }
                // bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio3]; }
                        if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio8]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio4]; }
                        shadowratiof3 /= facesconsidered;
                    }
                // bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio3]; }
                        if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio9]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio2]; }
                        shadowratiof2 /= facesconsidered;
                    }
                }
            // done:
                //ambient occlusion, corners with 2 blocks get full occlusion, others half
                if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; }
                else
                {
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                }
            // next:
                if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; }
                else
                {
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                }
            // next1:
                if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; }
                else
                {
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                }
            // next2:
                if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; }
                else
                {
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                }
            // next3:
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof2 * Yellowness));

                curcolor2 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof3 * Yellowness));

                curcolor3 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof4 * Yellowness));

                curcolor4 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof5 * Yellowness));
            }

            int sidetexture = TextureId(tiletype, TileSideEnum.Left);
            int tilecount = drawleft;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = AtiArtifactFix; //tilingcount*fix
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 0, z + 0, y + 0 + flowerfix, texrecRight, texrecBottom, curcolor2);
            ModelDataTool.AddVertex(toreturn, x + 0, z + blockheight00, y + 0 + flowerfix, texrecRight, texrecTop, curcolor4);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + 0, y + 0 + flowerfix, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + blockheight10, y + 0 + flowerfix, texrecLeft, texrecTop, curcolor3);
            if (occluded)
            {
                if (occdirnorthwest)
                {
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//3
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                }
                else
                {
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//3
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                }
            }

            else if (Game.ColorR(curcolor) != Game.ColorR(curcolor4) || Game.ColorR(curcolor3) == Game.ColorR(curcolor2))
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
            }
            else
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
            }
        }
        //right - same as left, but y is 1 greater.
        if (drawright > 0)
        {
            bool occluded = false;
            bool occdirnorthwest = true;
            //bool applysmoothing = true;
            int shadowratio = GetShadowRatio(xx, yy + 1, zz, x + 1, y, z);
            //if (true)
            {
                int top = currentChunk[Index3d(xx, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                int bottom = currentChunk[Index3d(xx, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                int left = currentChunk[Index3d(xx - 1, yy + 1, zz, chunksize + 2, chunksize + 2)];
                int right = currentChunk[Index3d(xx + 1, yy + 1, zz, chunksize + 2, chunksize + 2)];
                int topleft = currentChunk[Index3d(xx - 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                int topright = currentChunk[Index3d(xx + 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                int bottomleft = currentChunk[Index3d(xx - 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                int bottomright = currentChunk[Index3d(xx + 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                int shadowratio3 = shadowratio;//down
                int shadowratio4 = shadowratio;//right
                int shadowratio5 = shadowratio;//up
                int shadowratio2 = shadowratio;//left
                int shadowratio7 = shadowratio;//leftup
                int shadowratio6 = shadowratio;//rightup
                int shadowratio9 = shadowratio;//leftdown
                int shadowratio8 = shadowratio;//rightdown
                //check occupied blocks
                //todo: if top !=0 { if transparentforlight { etc
                if (top != 0) { if (!IsTransparentForLight(top)) { topoccupied = true; } else { topoccupied = false; } }
                else { topoccupied = false; shadowratio5 = GetShadowRatio(xx, yy + 1, zz + 1, x - 1, y, z + 1); }
                if (topleft != 0) { if (!IsTransparentForLight(topleft)) { topleftoccupied = true; } else { topleftoccupied = false; } }
                else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx - 1, yy + 1, zz + 1, x - 1, y - 1, z + 1); }
                if (topright != 0) { if (!IsTransparentForLight(topright)) { toprightoccupied = true; } else { toprightoccupied = false; } }
                else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx + 1, yy + 1, zz + 1, x - 1, y + 1, z + 1); }
                if (left != 0) { if (!IsTransparentForLight(left)) { leftoccupied = true; } else { leftoccupied = false; } }
                else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx - 1, yy + 1, zz, x - 1, y - 1, z); }
                if (right != 0) { if (!IsTransparentForLight(right)) { rightoccupied = true; } else { rightoccupied = false; } }
                else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx + 1, yy + 1, zz, x - 1, y + 1, z); }
                if (bottom != 0) { if (!IsTransparentForLight(bottom)) { bottomoccupied = true; } else { bottomoccupied = false; } }
                else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx, yy + 1, zz - 1, x - 1, y, z - 1); }
                if (bottomright != 0) { if (!IsTransparentForLight(bottomright)) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx + 1, yy + 1, zz - 1, x - 1, y + 1, z - 1); }
                if (bottomleft != 0) { if (!IsTransparentForLight(bottomleft)) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx - 1, yy + 1, zz - 1, x - 1, y - 1, z - 1); }


                float shadowratiomain = lightlevels[shadowratio];
                float shadowratiof5 = shadowratiomain;
                float shadowratiof4 = shadowratiomain;
                float shadowratiof3 = shadowratiomain;
                float shadowratiof2 = shadowratiomain;

                if (shadowratio9 == shadowratio8 && shadowratio8 == shadowratio7 && shadowratio7 == shadowratio6 &&
                    shadowratio6 == shadowratio5 && shadowratio5 == shadowratio4 && shadowratio4 == shadowratio3 &&
                    shadowratio3 == shadowratio2 && shadowratio2 == shadowratio)
                {
                    //no shadow tiles near, just do occlusion
                    // goto done;
                }
                else
                {
                    //topleft vertex
                    if (leftoccupied && topoccupied) { }
                    else
                    {
                        byte facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio5]; }
                        if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio7]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += lightlevels[shadowratio2]; }
                        shadowratiof4 /= facesconsidered;
                    }
                // toprightvertex:
                    //topright vertex
                    if (topoccupied && rightoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio5]; }
                        if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio6]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += lightlevels[shadowratio4]; }
                        shadowratiof5 /= facesconsidered;
                    }
                // bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio3]; }
                        if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio8]; }
                        if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += lightlevels[shadowratio4]; }
                        shadowratiof3 /= facesconsidered;
                    }
                // bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { }
                    else
                    {
                        int facesconsidered = 4;
                        if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio3]; }
                        if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio9]; }
                        if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += lightlevels[shadowratio2]; }
                        shadowratiof2 /= facesconsidered;
                    }
                }
            // done:
                //ambient occlusion, corners with 2 blocks get full occlusion, others half
                if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; }
                else
                {
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                }
            // next:
                if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; }
                else
                {
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                }
            // next1:
                if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; }
                else
                {
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                }
            // next2:
                if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; }
                else
                {
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                }
            // next3:
                curcolor = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof2),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof2 * Yellowness));

                curcolor2 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof3),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof3 * Yellowness));

                curcolor3 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof4),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof4 * Yellowness));

                curcolor4 = Game.ColorFromArgb(Game.ColorA(color),
                    game.platform.FloatToInt(Game.ColorR(colorShadowSide) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorG(colorShadowSide) * shadowratiof5),
                    game.platform.FloatToInt(Game.ColorB(colorShadowSide) * shadowratiof5 * Yellowness));
            }
            int sidetexture = TextureId(tiletype, TileSideEnum.Right);
            int tilecount = drawright;
            ModelData toreturn = GetToReturn(tt, sidetexture);
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = AtiArtifactFix; //tilingcount*fix
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, x + 0, z + 0, y + 1 - flowerfix, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, x + 0, z + blockheight01, y + 1 - flowerfix, texrecLeft, texrecTop, curcolor3);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + 0, y + 1 - flowerfix, texrecRight, texrecBottom, curcolor2);
            ModelDataTool.AddVertex(toreturn, x + 1 * tilecount, z + blockheight11, y + 1 - flowerfix, texrecRight, texrecTop, curcolor4);
            if (occluded)
            {
                if (occdirnorthwest)
                {
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//3
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                }
                else
                {
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                    ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                    ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                    ModelDataTool.AddIndex(toreturn, (lastelement + 1));//3
                    ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                }
            }

            else if (Game.ColorR(curcolor) != Game.ColorR(curcolor4) || Game.ColorR(curcolor3) == Game.ColorR(curcolor2))
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//2
            }
            else
            {
                ModelDataTool.AddIndex(toreturn, (lastelement + 1));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//0
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
                ModelDataTool.AddIndex(toreturn, (lastelement + 0));//3
                ModelDataTool.AddIndex(toreturn, (lastelement + 2));//1
                ModelDataTool.AddIndex(toreturn, (lastelement + 3));//2
            }
        }
    }

    public bool IsTransparentForLight(int block)
    {
        Packet_BlockType b = game.blocktypes[block];
        return b.DrawType != Packet_DrawTypeEnum.Solid && b.DrawType != Packet_DrawTypeEnum.ClosedDoor;
    }
    
    public ModelData GetToReturn(int tiletype, int textureid)
    {
        if (!(istransparent[tiletype] || IsWater(tiletype)))
        {
            return toreturnatlas1d[textureid / game.terrainTexturesPerAtlas];
        }
        else
        {
            return toreturnatlas1dtransparent[textureid / game.terrainTexturesPerAtlas];
        }
    }

    public int TextureId(int tiletype, int side)
    {
        return game.TextureId[tiletype][side];
    }

    public bool CanSupportTorch(int blocktype)
    {
        return blocktype != 0
            && game.blocktypes[blocktype].DrawType != Packet_DrawTypeEnum.Torch;
    }

    public void AddVertex(ModelData model, float x, float y, float z, float u, float v, int color)
    {
        model.xyz[model.GetXyzCount() + 0] = x;
        model.xyz[model.GetXyzCount() + 1] = y;
        model.xyz[model.GetXyzCount() + 2] = z;
        model.uv[model.GetUvCount() + 0] = u;
        model.uv[model.GetUvCount() + 1] = v;
        model.rgba[model.GetRgbaCount() + 0] = Game.IntToByte(Game.ColorR(color));
        model.rgba[model.GetRgbaCount() + 1] = Game.IntToByte(Game.ColorG(color));
        model.rgba[model.GetRgbaCount() + 2] = Game.IntToByte(Game.ColorB(color));
        model.rgba[model.GetRgbaCount() + 3] = Game.IntToByte(Game.ColorA(color));
        model.verticesCount++;
    }

    float Min(float a, float b)
    {
        if (a < b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    internal int TorchTopTexture;
    internal int TorchSideTexture;

    public int GetRailSlope(int xx, int yy, int zz)
    {
        int tiletype = currentChunk18[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];
        int rail = Rail(tiletype);
        int blocknear;
        //if (x < d_MapStorage.MapSizeX - 1)
        {
            blocknear = currentChunk18[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)];
            if (rail == RailDirectionFlagsEnum.Horizontal &&
                 blocknear != 0 && Rail(blocknear) == RailDirectionFlagsEnum.None)
            {
                return RailSlopeEnum.TwoRightRaised;
            }
        }
        //if (x > 0)
        {
            blocknear = currentChunk18[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)];
            if (rail == RailDirectionFlagsEnum.Horizontal &&
                 blocknear != 0 && Rail(blocknear) == RailDirectionFlagsEnum.None)
            {
                return RailSlopeEnum.TwoLeftRaised;

            }
        }
        //if (y > 0)
        {
            blocknear = currentChunk18[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)];
            if (rail == RailDirectionFlagsEnum.Vertical &&
                  blocknear != 0 && Rail(blocknear) == RailDirectionFlagsEnum.None)
            {
                return RailSlopeEnum.TwoUpRaised;
            }
        }
        //if (y < d_MapStorage.MapSizeY - 1)
        {
            blocknear = currentChunk18[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)];
            if (rail == RailDirectionFlagsEnum.Vertical &&
                  blocknear != 0 && Rail(blocknear) == RailDirectionFlagsEnum.None)
            {
                return RailSlopeEnum.TwoDownRaised;
            }
        }
        return RailSlopeEnum.Flat;
    }

    public int Rail(int tiletype)
    {
        return game.blocktypes[tiletype].Rail;
    }

    public bool IsFlower(int tiletype)
    {
        return game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Plant;
    }

    public bool isvalid(int tt)
    {
        return game.blocktypes[tt].Name != null;
    }

    public int getBestLadderWall(int x, int y, int z, int[] currentChunk)
    {
        bool front = false;
        bool back = false;
        bool left = false;
        //bool right=false;
        int wallscount = 0;
        if (currentChunk[Index3d(x, y - 1, z, chunksize + 2, chunksize + 2)] != 0)
        {
            front = true;
            wallscount++;
        }
        if (currentChunk[Index3d(x, y + 1, z, chunksize + 2, chunksize + 2)] != 0)
        {
            back = true;
            wallscount++;
        }
        int c = currentChunk[Index3d(x - 1, y, z, chunksize + 2, chunksize + 2)];
        if (c != 0)
        {
            left = true;
            wallscount++;
        }
        if (currentChunk[Index3d(x + 1, y, z, chunksize + 2, chunksize + 2)] != 0)
        {
            //right = true;
            wallscount++;
        }
        if (wallscount != 1)
        {
            return -1;
        }
        else
        {
            if (front)
            {
                return 0;
            }
            else if (back)
            {
                return 1;
            }
            else if (left)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
    }

    public int getBestLadderInDirection(int x, int y, int z, int[] currentChunk, int dir)
    {
        int dz = dir;
        int result = 0;
        //try
        {
            while ((Index3d(x, y, z + dz, chunksize + 2, chunksize + 2) >= 0)
                && (Index3d(x, y, z + dz, chunksize + 2, chunksize + 2) < (chunksize + 2) * (chunksize + 2) * (chunksize + 2))
                && currentChunk[Index3d(x, y, z + dz, chunksize + 2, chunksize + 2)] == 152)
            {
                result = dz;
                if (getBestLadderWall(x, y, z + dz, currentChunk) != -1) return result;
                dz += dir;
            }
        }
        //catch { }
        return 0;
    }

    public void AddTorch(int x, int y, int z, int type, int tt)
    {
        TerrainChunkTesselatorCi d_TerainRenderer = this;

        int curcolor = ColorWhite;
        float torchsizexy = one * 16 / 100; // 0.16f;
        float topx = one / 2 - torchsizexy / 2;
        float topy = one / 2 - torchsizexy / 2;
        float bottomx = one / 2 - torchsizexy / 2;
        float bottomy = one / 2 - torchsizexy / 2;

        topx += x;
        topy += y;
        bottomx += x;
        bottomy += y;

        if (type == TorchTypeEnum.Front) { bottomx = x - torchsizexy; }
        if (type == TorchTypeEnum.Back) { bottomx = x + 1; }
        if (type == TorchTypeEnum.Left) { bottomy = y - torchsizexy; }
        if (type == TorchTypeEnum.Right) { bottomy = y + 1; }

        Vector3Ref top00 = Vector3Ref.Create(topx, z + one * 9 / 10, topy);
        Vector3Ref top01 = Vector3Ref.Create(topx, z + one * 9 / 10, topy + torchsizexy);
        Vector3Ref top10 = Vector3Ref.Create(topx + torchsizexy, z + one * 9 / 10, topy);
        Vector3Ref top11 = Vector3Ref.Create(topx + torchsizexy, z + one * 9 / 10, topy + torchsizexy);

        if (type == TorchTypeEnum.Left)
        {
            top01.Y += -one / 10; // 0.1f
            top11.Y += -one / 10;
        }

        if (type == TorchTypeEnum.Right)
        {
            top10.Y += -one / 10;
            top00.Y += -one / 10;
        }

        if (type == TorchTypeEnum.Front)
        {
            top10.Y += -one / 10;
            top11.Y += -one / 10;
        }

        if (type == TorchTypeEnum.Back)
        {
            top01.Y += -one / 10;
            top00.Y += -one / 10;
        }

        Vector3Ref bottom00 = Vector3Ref.Create(bottomx, z + 0, bottomy);
        Vector3Ref bottom01 = Vector3Ref.Create(bottomx, z + 0, bottomy + torchsizexy);
        Vector3Ref bottom10 = Vector3Ref.Create(bottomx + torchsizexy, z + 0, bottomy);
        Vector3Ref bottom11 = Vector3Ref.Create(bottomx + torchsizexy, z + 0, bottomy + torchsizexy);

        texrecLeft = 0;
        texrecHeight = terrainTexturesPerAtlasInverse * AtiArtifactFix;

        //top
        {
            int sidetexture = TorchTopTexture;
            int tilecount = 1;
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix);
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, top00.X, top00.Y, top00.Z, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, top01.X, top01.Y, top01.Z, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top10.X, top10.Y, top10.Z, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, top11.X, top11.Y, top11.Z, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }

        //bottom - same as top, but z is 1 less.
        {
            int sidetexture = TorchSideTexture;
            int tilecount = 1;
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix);
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, bottom00.X, bottom00.Y, bottom00.Z, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom01.X, bottom01.Y, bottom01.Z, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom10.X, bottom10.Y, bottom10.Z, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom11.X, bottom11.Y, bottom11.Z, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }

        //front
        {
            int sidetexture = TorchSideTexture;
            int tilecount = 1;
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix);
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, bottom00.X, bottom00.Y, bottom00.Z, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom01.X, bottom01.Y, bottom01.Z, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top00.X, top00.Y, top00.Z, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, top01.X, top01.Y, top01.Z, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }

        //back - same as front, but x is 1 greater.
        {
            int sidetexture = TorchSideTexture;
            int tilecount = 1;
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix);
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, bottom10.X, bottom10.Y, bottom10.Z, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom11.X, bottom11.Y, bottom11.Z, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top10.X, top10.Y, top10.Z, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, top11.X, top11.Y, top11.Z, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }

        {
            int sidetexture = TorchSideTexture;
            int tilecount = 1;
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix);
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, bottom00.X, bottom00.Y, bottom00.Z, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top00.X, top00.Y, top00.Z, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom10.X, bottom10.Y, bottom10.Z, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top10.X, top10.Y, top10.Z, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }

        //right - same as left, but y is 1 greater.
        {
            int sidetexture = TorchSideTexture;
            int tilecount = 1;
            texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            texrecWidth = (tilecount * AtiArtifactFix);
            float texrecBottom = texrecTop + texrecHeight;
            float texrecRight = texrecLeft + texrecWidth;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, bottom01.X, bottom01.Y, bottom01.Z, texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top01.X, top01.Y, top01.Z, texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom11.X, bottom11.Y, bottom11.Z, texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top11.X, top11.Y, top11.Z, texrecRight, texrecTop, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }
    }

    public VerticesIndicesToLoad GetVerticesIndices(ModelData m, int x, int y, int z, int texture, bool transparent)
    {
        VerticesIndicesToLoad v = new VerticesIndicesToLoad();
        v.modelData = m;
        v.positionX = x * chunksize;
        v.positionY = y * chunksize;
        v.positionZ = z * chunksize;
        v.texture = texture;
        v.transparent = transparent;
        return v;
    }

    public VerticesIndicesToLoad[] GetFinalVerticesIndices(int x, int y, int z, IntRef retCount)
    {
        VerticesIndicesToLoad[] ret = new VerticesIndicesToLoad[toreturnatlas1dLength + toreturnatlas1dLength];
        retCount.value = 0;
        for (int i = 0; i < toreturnatlas1dLength; i++)
        {
            if (toreturnatlas1d[i].indicesCount > 0)
            {
                ret[retCount.value++] = GetVerticesIndices(toreturnatlas1d[i], x, y, z, game.d_TerrainTextures.terrainTextures1d()[i % game.d_TerrainTextures.terrainTexturesPerAtlas()], false);
            }
        }
        for (int i = 0; i < toreturnatlas1dLength; i++)
        {
            if (toreturnatlas1dtransparent[i].indicesCount > 0)
            {
                ret[retCount.value++] = GetVerticesIndices(toreturnatlas1dtransparent[i], x, y, z, game.d_TerrainTextures.terrainTextures1d()[i % game.d_TerrainTextures.terrainTexturesPerAtlas()], true);
            }
        }
        return ret;
    }

    public VerticesIndicesToLoad[] MakeChunk(int x, int y, int z,
    int[] chunk18, byte[] shadows18, float[] lightlevels_, IntRef retCount)
    {
        this.currentChunk18 = chunk18;
        this.currentChunkShadows18 = shadows18;
        this.lightlevels = lightlevels_;

        for (int i = 0; i < GlobalVar.MAX_BLOCKTYPES; i++)
        {
            Packet_BlockType b = game.blocktypes[i];
            if (b == null)
            {
                continue;
            }
            istransparent[i] = (b.DrawType != Packet_DrawTypeEnum.Solid) && (b.DrawType != Packet_DrawTypeEnum.Fluid);
            ishalfheight[i] = (b.DrawType == Packet_DrawTypeEnum.HalfHeight) || (b.GetRail() != 0);
        }

        if (x < 0 || y < 0 || z < 0) { retCount.value = 0; return new VerticesIndicesToLoad[0]; }
        if (!started) { game.platform.ThrowException("not started"); }
        if (x >= mapsizex / chunksize
            || y >= mapsizey / chunksize
            || z >= mapsizez / chunksize) { retCount.value = 0; return new VerticesIndicesToLoad[0]; }

        for (int i = 0; i < toreturnatlas1dLength; i++)
        {
            toreturnatlas1d[i].verticesCount = 0;
            toreturnatlas1d[i].indicesCount = 0;
            toreturnatlas1dtransparent[i].verticesCount = 0;
            toreturnatlas1dtransparent[i].indicesCount = 0;
        }

        CalculateVisibleFaces(currentChunk18);
        CalculateTilingCount(currentChunk18, x * chunksize, y * chunksize, z * chunksize);
        if (EnableSmoothLight)
        {
            CalculateSmoothBlockPolygons(x, y, z);
        }
        else
        {
            CalculateBlockPolygons(x, y, z);
        }
        VerticesIndicesToLoad[] ret = GetFinalVerticesIndices(x, y, z, retCount);
        return ret;
    }
}

public class VerticesIndicesToLoad
{
    internal ModelData modelData;
    internal float positionX;
    internal float positionY;
    internal float positionZ;
    internal bool transparent;
    internal int texture;
}

public class TorchTypeEnum
{
    public const int Normal = 0;
    public const int Left = 1;
    public const int Right = 2;
    public const int Front = 3;
    public const int Back = 4;
}

public class RailDirectionFlagsEnum
{
    public const int None = 0;
    public const int Horizontal = 1;
    public const int Vertical = 2;
    public const int UpLeft = 4;
    public const int UpRight = 8;
    public const int DownLeft = 16;
    public const int DownRight = 32;
}

public class RailSlopeEnum
{
    public const int Flat = 0;
    public const int TwoLeftRaised = 1;
    public const int TwoRightRaised = 2;
    public const int TwoUpRaised = 3;
    public const int TwoDownRaised = 4;
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
