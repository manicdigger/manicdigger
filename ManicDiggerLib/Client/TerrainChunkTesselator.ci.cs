// Generates triangles for a single 16x16x16 chunk.
// Needs to know the surrounding of the chunk (18x18x18 blocks total).
// This class is heavily inlined and unrolled for performance.
// Special-shape (rare) blocks don't need as much performance.
public class TerrainChunkTesselatorCi
{
    private class TileDirectionEnum
    {
        public const int Top = 0;
        public const int Bottom = 1;

        public const int Left = 2;
        public const int Right = 3;

        public const int TopLeft = 4;
        public const int TopRight = 5;

        public const int BottomLeft = 6;
        public const int BottomRight = 7;
    }

    private class CornerEnum
    {
        public const int TopLeft = 0;
        public const int TopRight = 1;

        public const int BottomLeft = 2;
        public const int BottomRight = 3;
    }

    
    //internal float texrecTop;
    internal float _texrecLeft;
    internal float _texrecRight;
    internal float _texrecWidth;
    internal float _texrecHeight;
    internal int _colorWhite;

    public TerrainChunkTesselatorCi()
    {
        one = 1;
        EnableSmoothLight = true;
        ENABLE_TEXTURE_TILING = true;
        _colorWhite = Game.ColorFromArgb(255, 255, 255, 255);
        BlockShadow = 0.7f;
        DONOTDRAWEDGES = true;
        AtiArtifactFix = 0.995f;
        occ = 0.7f;
        halfocc = 0.4f;

        Yellowness = 1f; // lower is yellower
        Blueness = 0.9f; // lower is blue-er

        c_OcclusionNeighbors = new Vector3i[8][];

        c_OcclusionNeighbors[TileSideEnum.Top] = new Vector3i[] { new Vector3i(+0, -1, +1),
                                                                  new Vector3i(+0, +1, +1),

                                                                  new Vector3i(-1, +0, +1),
                                                                  new Vector3i(+1, +0, +1),

                                                                  new Vector3i(-1, -1, +1),
                                                                  new Vector3i(+1, -1, +1),
                                                                  
                                                                  new Vector3i(-1, +1, +1),
                                                                  new Vector3i(+1, +1, +1)};

        c_OcclusionNeighbors[TileSideEnum.Front] = new Vector3i[] {new Vector3i(-1, +0, +1),
                                                                   new Vector3i(-1, +0, -1),

                                                                   new Vector3i(-1, -1, +0),
                                                                   new Vector3i(-1, +1, +0),

                                                                   new Vector3i(-1, -1, +1),
                                                                   new Vector3i(-1, +1, +1),
                                                                  
                                                                   new Vector3i(-1, -1, -1),
                                                                   new Vector3i(-1, +1, -1)};

        c_OcclusionNeighbors[TileSideEnum.Bottom] = new Vector3i[]{new Vector3i(+0, +1, -1),
                                                                   new Vector3i(-1, +0, -1),

                                                                   new Vector3i(-1, +0, -1),
                                                                   new Vector3i(+1, +0, -1),

                                                                   new Vector3i(-1, +1, -1),
                                                                   new Vector3i(+1, +1, -1),
                                                                  
                                                                   new Vector3i(-1, -1, -1),
                                                                   new Vector3i(+1, -1, -1)};

        c_OcclusionNeighbors[TileSideEnum.Back] = new Vector3i[]{  new Vector3i(+1, +0, +1),
                                                                   new Vector3i(+1, +0, -1),

                                                                   new Vector3i(+1, -1, +0),
                                                                   new Vector3i(+1, +1, +0),

                                                                   new Vector3i(+1, -1, +1),
                                                                   new Vector3i(+1, +1, +1),
                                                                  
                                                                   new Vector3i(+1, -1, -1),
                                                                   new Vector3i(+1, +1, -1)};

        c_OcclusionNeighbors[TileSideEnum.Left] = new Vector3i[]{  new Vector3i(+0, -1, +1),
                                                                   new Vector3i(+0, -1, -1),

                                                                   new Vector3i(+1, -1, +0),
                                                                   new Vector3i(-1, -1, +0),

                                                                   new Vector3i(+1, -1, +1),
                                                                   new Vector3i(-1, -1, +1),
                                                                  
                                                                   new Vector3i(+1, -1, -1),
                                                                   new Vector3i(-1, -1, -1)};

        c_OcclusionNeighbors[TileSideEnum.Right] = new Vector3i[]{ new Vector3i(+0, +1, +1),
                                                                   new Vector3i(+0, +1, -1),

                                                                   new Vector3i(-1, +1, +0),
                                                                   new Vector3i(+1, +1, +0),

                                                                   new Vector3i(-1, +1, +1),
                                                                   new Vector3i(+1, +1, +1),
                                                                  
                                                                   new Vector3i(-1, +1, -1),
                                                                   new Vector3i(+1, +1, -1)};



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
    static int Index3d(Vector3i v)
    {
        return Index3d(v.x, v.y, v.z, chunksize + 2, chunksize + 2);
    }

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

        _texrecWidth = AtiArtifactFix;
        _texrecHeight = terrainTexturesPerAtlasInverse * AtiArtifactFix;
        _texrecLeft = 0f;
        _texrecRight = _texrecLeft + _texrecWidth;

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

    [System.Obsolete("Use GetShadowRation(int,int,int) instead")]
    public int GetShadowRatio(int xx, int yy, int zz, int globalx, int globaly, int globalz)
    {
        return GetShadowRatio(xx, yy, zz);
    }

    public int GetShadowRatio(Vector3i v)
    {
        return GetShadowRatio(v.x, v.y, v.z);
    }

    public int GetShadowRatio(int xx, int yy, int zz)
    {
        return currentChunkShadows18[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];
    }

    public void CalculateSmoothBlockPolygons(int x, int y, int z)
    {
        for (int xx = 0; xx < chunksize; xx++)
        {
            for (int yy = 0; yy < chunksize; yy++)
            {
                for (int zz = 0; zz < chunksize; zz++)
                {
                    //Most blocks aren't rendered at all, quickly reject them.
                    if (currentChunkDraw16[Index3d(xx, yy, zz, chunksize, chunksize)] != 0)
                    {
                        int xxx = x * chunksize + xx;
                        int yyy = y * chunksize + yy;
                        int zzz = z * chunksize + zz;

                        SmoothLightBlockPolygons(xxx, yyy, zzz, currentChunk18);
                    }
                }
            }
        }
    }

    private int ColorMultiply(int color, float fValue)
    {
        return Game.ColorFromArgb(Game.ColorA(color),
            game.platform.FloatToInt(Game.ColorR(color) * fValue),
            game.platform.FloatToInt(Game.ColorG(color) * fValue),
            game.platform.FloatToInt(Game.ColorB(color) * fValue * Yellowness));
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

    #region CalcSmoothBlockFace
    private void CalcSmoothBlockFace(int x, int y, int z, int tileType, Vector3f vOffset, Vector3f vScale, int[] currentChunk, int tileSide)
    {
        int xx = x % chunksize + 1;
        int yy = y % chunksize + 1;
        int zz = z % chunksize + 1;
        Vector3i[] vNeighbors = c_OcclusionNeighbors[tileSide];

        int shadowratio = GetShadowRatio(xx, yy, zz + 1);

        int top = currentChunk[Index3d(vNeighbors[TileDirectionEnum.Top].Add(xx, yy, zz))];
        int bottom = currentChunk[Index3d(vNeighbors[TileDirectionEnum.Bottom].Add(xx, yy, zz))];

        int left = currentChunk[Index3d(vNeighbors[TileDirectionEnum.Left].Add(xx, yy, zz))];
        int right = currentChunk[Index3d(vNeighbors[TileDirectionEnum.Right].Add(xx, yy, zz))];

        int topleft = currentChunk[Index3d(vNeighbors[TileDirectionEnum.TopLeft].Add(xx, yy, zz))];
        int topright = currentChunk[Index3d(vNeighbors[TileDirectionEnum.TopRight].Add(xx, yy, zz))];

        int bottomleft = currentChunk[Index3d(vNeighbors[TileDirectionEnum.BottomLeft].Add(xx, yy, zz))];
        int bottomright = currentChunk[Index3d(vNeighbors[TileDirectionEnum.BottomRight].Add(xx, yy, zz))];

        int shadowratioB = shadowratio;//down
        int shadowratioR = shadowratio;//right
        int shadowratioT = shadowratio;//up
        int shadowratioL = shadowratio;//left
        int shadowratioTL = shadowratio;//leftup
        int shadowratioTR = shadowratio;//rightup
        int shadowratioBL = shadowratio;//leftdown
        int shadowratioBR = shadowratio;//rightdown

        //check occupied blocks
        CheckOccupation(top, ref topoccupied, ref shadowratioT, vNeighbors[TileDirectionEnum.Top].Add(xx, yy, zz));
        CheckOccupation(bottom, ref bottomoccupied, ref shadowratioB, vNeighbors[TileDirectionEnum.Bottom].Add(xx, yy, zz));
        CheckOccupation(left, ref leftoccupied, ref shadowratioL, vNeighbors[TileDirectionEnum.Left].Add(xx, yy, zz));
        CheckOccupation(right, ref rightoccupied, ref shadowratioR, vNeighbors[TileDirectionEnum.Right].Add(xx, yy, zz));
        CheckOccupation(topleft, ref topleftoccupied, ref shadowratioTL, vNeighbors[TileDirectionEnum.TopLeft].Add(xx, yy, zz));
        CheckOccupation(topright, ref toprightoccupied, ref shadowratioTR, vNeighbors[TileDirectionEnum.TopRight].Add(xx, yy, zz));
        CheckOccupation(bottomleft, ref bottomleftoccupied, ref shadowratioBL, vNeighbors[TileDirectionEnum.BottomLeft].Add(xx, yy, zz));
        CheckOccupation(bottomright, ref bottomrightoccupied, ref shadowratioBR, vNeighbors[TileDirectionEnum.BottomRight].Add(xx, yy, zz));

        //initialize shadow values
        float[] fShadowRation = new float[4];
        float shadowratiomain = lightlevels[shadowratio];
        for (int i = 0; i < fShadowRation.Length; i++)
        {
            fShadowRation[i] = shadowratiomain;
        }

        #region Shadow
        //get occupied blocks for ao and smoothing

        //topleft vertex
        if (leftoccupied && topoccupied) { }
        else
        {
            byte facesconsidered = 1;
            if (!topoccupied) { fShadowRation[CornerEnum.TopLeft] += lightlevels[shadowratioT]; ++facesconsidered; }
            if (!topleftoccupied) { fShadowRation[CornerEnum.TopLeft] += lightlevels[shadowratioTL]; ++facesconsidered; }
            if (!leftoccupied) { fShadowRation[CornerEnum.TopLeft] += lightlevels[shadowratioL]; ++facesconsidered; }
            fShadowRation[CornerEnum.TopLeft] /= facesconsidered;
        }
        //topright vertex
        if (topoccupied && rightoccupied) { }
        else
        {
            byte facesconsidered = 4;
            if (topoccupied) { facesconsidered -= 1; } else { fShadowRation[CornerEnum.TopRight] += lightlevels[shadowratioT]; }
            if (toprightoccupied) { facesconsidered -= 1; } else { fShadowRation[CornerEnum.TopRight] += lightlevels[shadowratioTR]; }
            if (rightoccupied) { facesconsidered -= 1; } else { fShadowRation[CornerEnum.TopRight] += lightlevels[shadowratioR]; }
            fShadowRation[CornerEnum.TopRight] /= facesconsidered;
        }
        //bottomright vertex
        if (bottomoccupied && rightoccupied) { }
        else
        {
            byte facesconsidered = 4;
            if (bottomoccupied) { facesconsidered -= 1; } else { fShadowRation[CornerEnum.BottomRight] += lightlevels[shadowratioB]; }
            if (bottomrightoccupied) { facesconsidered -= 1; } else { fShadowRation[CornerEnum.BottomRight] += lightlevels[shadowratioBR]; }
            if (rightoccupied) { facesconsidered -= 1; } else { fShadowRation[CornerEnum.BottomRight] += lightlevels[shadowratioR]; }
            fShadowRation[CornerEnum.BottomRight] /= facesconsidered;
        }
        //bottomleft
        if (bottomoccupied && leftoccupied) { }
        else
        {
            byte facesconsidered = 4;
            if (bottomoccupied) { facesconsidered -= 1; } else { fShadowRation[CornerEnum.BottomLeft] += lightlevels[shadowratioB]; }
            if (bottomleftoccupied) { facesconsidered -= 1; } else { fShadowRation[CornerEnum.BottomLeft] += lightlevels[shadowratioBL]; }
            if (leftoccupied) { facesconsidered -= 1; } else { fShadowRation[CornerEnum.BottomLeft] += lightlevels[shadowratioL]; }
            fShadowRation[CornerEnum.BottomLeft] /= facesconsidered;
        }


        //ambient occlusion, corners with 2 blocks get full occlusion, others half
        if (topoccupied && rightoccupied) { fShadowRation[CornerEnum.TopRight] *= halfocc; }
        else if (topoccupied || rightoccupied || toprightoccupied) { fShadowRation[CornerEnum.TopRight] *= occ; }

        // next:
        if (topoccupied && leftoccupied) { fShadowRation[CornerEnum.TopLeft] *= halfocc; }
        else if (topoccupied || leftoccupied || topleftoccupied) { fShadowRation[CornerEnum.TopLeft] *= occ; }

        // next1:
        if (bottomoccupied && rightoccupied) { fShadowRation[CornerEnum.BottomRight] *= halfocc; }
        else if (bottomoccupied || rightoccupied || bottomrightoccupied) { fShadowRation[CornerEnum.BottomRight] *= occ; }

        // next2:
        if (bottomoccupied && leftoccupied) { fShadowRation[CornerEnum.BottomLeft] *= halfocc; }
        else if (bottomoccupied || leftoccupied || bottomleftoccupied) { fShadowRation[CornerEnum.BottomLeft] *= occ; }

        #endregion

        DrawBlockFace(x, y, z, tileType, tileSide, vOffset, vScale, vNeighbors, fShadowRation);
    }

    private void DrawBlockFace(int x, int y, int z, int tileType, int tileSide, Vector3f vOffset, Vector3f vScale, Vector3i[] vNeighbors, float[] fShadowRation)
    {
        //shadowratioTR = shadowratioTL = shadowratioRB = shadowratiofLB = 0x1;
        int color = _colorWhite;

        //Bottom is darker
        if (tileSide == TileSideEnum.Bottom)
        {
            color = ColorMultiply(color, BlockShadow);
        }

        int sidetexture = TextureId(tileType, tileSide);
        ModelData toreturn = GetToReturn(tileType, sidetexture);
        float texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
        float texrecBottom = texrecTop + _texrecHeight;
        int lastelement = toreturn.verticesCount;

        Vector3i v;

        //Calculate the corner points
        v = vNeighbors[TileDirectionEnum.TopRight].Add(1, 1, 1);
        float xPos = x + vOffset.x + ((v.x * 0.5f) * vScale.x);
        float zPos = z + vOffset.z + ((v.z * 0.5f) * vScale.z);
        float yPos = y + vOffset.y + ((v.y * 0.5f) * vScale.y);
        ModelDataTool.AddVertex(toreturn, xPos, zPos , yPos, _texrecRight, texrecTop, ColorMultiply(color, fShadowRation[CornerEnum.TopRight]));

        v = vNeighbors[TileDirectionEnum.TopLeft].Add(1, 1, 1);
        xPos = x + vOffset.x + ((v.x * 0.5f) * vScale.x);
        zPos = z + vOffset.z + ((v.z * 0.5f) * vScale.z);
        yPos = y + vOffset.y + ((v.y * 0.5f) * vScale.y);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecLeft, texrecTop, ColorMultiply(color, fShadowRation[CornerEnum.TopLeft]));

        v = vNeighbors[TileDirectionEnum.BottomRight].Add(1, 1, 1);
        xPos = x + vOffset.x + ((v.x * 0.5f) * vScale.x);
        zPos = z + vOffset.z + ((v.z * 0.5f) * vScale.z);
        yPos = y + vOffset.y + ((v.y * 0.5f) * vScale.y);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecRight, texrecBottom, ColorMultiply(color, fShadowRation[CornerEnum.BottomRight]));

        v = vNeighbors[TileDirectionEnum.BottomLeft].Add(1, 1, 1);
        xPos = x + vOffset.x + ((v.x * 0.5f) * vScale.x);
        zPos = z + vOffset.z + ((v.z * 0.5f) * vScale.z);
        yPos = y + vOffset.y + ((v.y * 0.5f) * vScale.y);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecLeft, texrecBottom, ColorMultiply(color, fShadowRation[CornerEnum.BottomLeft]));

        if (tileSide == TileSideEnum.Back)
        {
            //Draw backwards, so the visible side points outward
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
        }
        else
        {
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }
    }
    #endregion

    private void CheckOccupation(int nBlockType, ref bool blnOccupied, ref int shadowratio, Vector3i vPos)
    {
        if (nBlockType != 0)
        {
            blnOccupied = !IsTransparentForLight(nBlockType);
        }
        else
        {
            blnOccupied = false;
            shadowratio = GetShadowRatio(vPos);
        }
    }

    /// <summary>
    /// Returns the sides to draw for this block
    /// </summary>
    private int GetToDrawFlags(int xx, int yy, int zz)
    {
        int nToDraw = TileSideFlagsEnum.None;

        byte[] drawFlags = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)];

        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Top, nToDraw, TileSideFlagsEnum.Top);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Bottom, nToDraw, TileSideFlagsEnum.Bottom);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Front, nToDraw, TileSideFlagsEnum.Front);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Back, nToDraw, TileSideFlagsEnum.Back);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Left, nToDraw, TileSideFlagsEnum.Left);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Right, nToDraw, TileSideFlagsEnum.Right);

        return nToDraw;
    }

    /// <summary>
    /// Sets the visible flag in the nCurrentFlags if this side needs to be drawn
    /// </summary>
    private int SetVisibleFlag(byte[] drawFlags, int tileSideIndex, int nCurrentFlags, int nFlagToSet)
    {
        if (drawFlags[tileSideIndex] > 0)
        {
            return nCurrentFlags | nFlagToSet;
        }
        else
        {
            return nCurrentFlags;
        }
    }



    private static Vector3i[][] c_OcclusionNeighbors;

    #region SmoothLightBlockPolygons
    public void SmoothLightBlockPolygons(int x, int y, int z, int[] currentChunk)
    {
        float blockheight = 1;//= data.GetTerrainBlockHeight(tiletype);

        //slope
        float blockheight00 = blockheight;
        float blockheight01 = blockheight;
        float blockheight10 = blockheight;
        float blockheight11 = blockheight;

        int xx = x % chunksize + 1;
        int yy = y % chunksize + 1;
        int zz = z % chunksize + 1;

        int nToDraw = GetToDrawFlags(xx, yy, zz);
        int tiletype = currentChunk[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];;
        int rail = Rail(tiletype);

        Vector3f vOffset = new Vector3f(0, 0, 0);
        Vector3f vScale = new Vector3f(1, 1, 1);

        if (!isvalid(tiletype))
        {
            return;
        }
        if (nToDraw == TileSideFlagsEnum.None)
        {
            //nothing to do
            return;
        }
        int color = _colorWhite; //mapstorage.GetTerrainBlockColor(x, y, z);
        int colorShadowSide = ColorMultiply(color, BlockShadow);
        _texrecLeft = 0;
        if (DONOTDRAWEDGES)
        {
            //On finite map don't draw borders:
            //they can't be seen without freemove cheat.
            if (z == 0) { nToDraw ^= TileSideFlagsEnum.Bottom; }
            if (x == 0) { nToDraw ^= TileSideFlagsEnum.Front; }
            if (x == mapsizex - 1) { nToDraw ^= TileSideFlagsEnum.Back; }
            if (y == 0) { nToDraw ^= TileSideFlagsEnum.Left; }
            if (y == mapsizey - 1) { nToDraw ^= TileSideFlagsEnum.Right; }
        }
        float flowerfix = 0;
        if (IsFlower(tiletype))
        {
            //Draw nothing but 2 faces. Prevents flickering.
            nToDraw = TileSideFlagsEnum.Left | TileSideFlagsEnum.Front;

            vScale = new Vector3f(0.5f, 0.5f, 0.5f);

            //Draw Front and Left side
            CalcSmoothBlockFace(x, y, z, tiletype, new Vector3f(0.5f, 0.25f, 0f), vScale, currentChunk, TileSideEnum.Front);
            CalcSmoothBlockFace(x, y, z, tiletype, new Vector3f(0.25f, 0.5f, 0f), vScale, currentChunk, TileSideEnum.Left);
            return;
        }
        if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Cactus)
        {
            //Cactus is thin
            vOffset = new Vector3f(0.2f, 0.2f, 0);
            vScale = new Vector3f(0.625f, 0.625f, 1f);
            flowerfix = 0.0625f;
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.OpenDoorLeft)
        {
            #region OpenDoorLeft
            nToDraw ^= TileSideFlagsEnum.Top;
            nToDraw ^= TileSideFlagsEnum.Bottom;
            flowerfix = 0.9f;
            //x-1, x+1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0 && 
                currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                nToDraw ^= TileSideFlagsEnum.Back;
                nToDraw ^= TileSideFlagsEnum.Front;
                nToDraw ^= TileSideFlagsEnum.Right;

                nToDraw |= TileSideFlagsEnum.Left;
            }
            //y-1, y+1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0 && 
                currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                nToDraw ^= TileSideFlagsEnum.Left;
                nToDraw ^= TileSideFlagsEnum.Right;
                nToDraw ^= TileSideFlagsEnum.Front;

                nToDraw |= TileSideFlagsEnum.Back;
            }
            #endregion
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.OpenDoorRight)
        {
            #region OpenDoorRight
            nToDraw ^= TileSideFlagsEnum.Top;
            nToDraw ^= TileSideFlagsEnum.Bottom;

            flowerfix = 0.9f;
            //x-1, x+1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0 && 
                currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                nToDraw ^= TileSideFlagsEnum.Back;
                nToDraw ^= TileSideFlagsEnum.Front;
                nToDraw ^= TileSideFlagsEnum.Left;

                nToDraw |= TileSideFlagsEnum.Right;
            }
            //y-1, y+1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0 && 
                currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                nToDraw ^= TileSideFlagsEnum.Back;
                nToDraw ^= TileSideFlagsEnum.Right;
                nToDraw ^= TileSideFlagsEnum.Left;

                nToDraw |= TileSideFlagsEnum.Front;
            }
            #endregion
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Fence ||
                 game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.ClosedDoor) // fence tiles automatically when another fence is beside
        {
            #region Fence/Door
            nToDraw = TileSideFlagsEnum.None;

            //x-1, x+1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] != 0 || 
                currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] != 0)
            {
                nToDraw |= TileSideFlagsEnum.Left;
            }
            //y-1, y+1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] != 0 || 
                currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] != 0)
            {
                nToDraw |= TileSideFlagsEnum.Front;
            }
            if ((nToDraw & (TileSideFlagsEnum.Back | TileSideFlagsEnum.Front | TileSideFlagsEnum.Right | TileSideFlagsEnum.Left) ) == 0)
            {
                nToDraw |= TileSideFlagsEnum.Back;
                nToDraw |= TileSideFlagsEnum.Left;
            }
            #endregion
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Ladder) // try to fit ladder to best wall or existing ladder
        {
            #region Ladder
            flowerfix = 0.95f; // 0.95f;

            nToDraw = TileSideFlagsEnum.None;

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
                case 1: nToDraw |= TileSideFlagsEnum.Left; break;
                case 2: nToDraw |= TileSideFlagsEnum.Back; break;
                case 3: nToDraw |= TileSideFlagsEnum.Front; break;
                default: nToDraw |= TileSideFlagsEnum.Right; break;
            }
            #endregion
        }
        else if (rail != RailDirectionFlagsEnum.None)
        {
            blockheight = 0.3f; 
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.HalfHeight)
        {
            blockheight = 0.5f; // 0.5f;
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Flat)
        {
            blockheight = 0.05f; // 0.05f;
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Torch)
        {
            int type = TorchTypeEnum.Normal;
            if (CanSupportTorch(currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Front; }
            if (CanSupportTorch(currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Back; }
            if (CanSupportTorch(currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Left; }
            if (CanSupportTorch(currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Right; }
            TorchSideTexture = TextureId(tiletype, TileSideEnum.Front);
            TorchTopTexture = TextureId(tiletype, TileSideEnum.Top);
            AddTorch(x, y, z, type, tiletype);
            return;
        }
        else if (rail != RailDirectionFlagsEnum.None)
        {
            int slope = GetRailSlope(xx, yy, zz);
            if (slope == RailSlopeEnum.TwoRightRaised)
            {
                blockheight10 += 1;
                blockheight11 += 1;
            }
            else if (slope == RailSlopeEnum.TwoLeftRaised)
            {
                blockheight00 += 1;
                blockheight01 += 1;
            }
            else if (slope == RailSlopeEnum.TwoUpRaised)
            {
                blockheight00 += 1;
                blockheight10 += 1;
            }
            else if (slope == RailSlopeEnum.TwoDownRaised)
            {
                blockheight01 += 1;
                blockheight11 += 1;
            }
        }
        else if (tiletype == 8)
        {
            #region liquid
            if (currentChunk[Index3d(xx, yy , zz - 1, chunksize + 2, chunksize + 2)] == 8)
            {
                //flow down in the lower block
                vOffset = new Vector3f(0, 0, -0.1f);
            }
            else
            {
                //lower than a normal block
                vScale = new Vector3f(1, 1, 0.9f);
            }
            #endregion
        }
        
        //Draw faces
        if ((nToDraw & TileSideFlagsEnum.Top) != TileSideFlagsEnum.None)
        {
            CalcSmoothBlockFace(x, y, z, tiletype, vOffset, vScale, currentChunk, TileSideEnum.Top);
        }
        if ((nToDraw & TileSideFlagsEnum.Bottom) != TileSideFlagsEnum.None)
        {
            CalcSmoothBlockFace(x, y, z, tiletype, vOffset, vScale, currentChunk, TileSideEnum.Bottom);
        }
        if ((nToDraw & TileSideFlagsEnum.Front) != TileSideFlagsEnum.None)
        {
            CalcSmoothBlockFace(x, y, z, tiletype, vOffset, vScale, currentChunk, TileSideEnum.Front);
        }
        if ((nToDraw & TileSideFlagsEnum.Back) != TileSideFlagsEnum.None)
        {
            CalcSmoothBlockFace(x, y, z, tiletype, vOffset, vScale, currentChunk, TileSideEnum.Back);
        }
        if ((nToDraw & TileSideFlagsEnum.Left) != TileSideFlagsEnum.None)
        {
            CalcSmoothBlockFace(x, y, z, tiletype, vOffset, vScale, currentChunk, TileSideEnum.Left);
        }
        if ((nToDraw & TileSideFlagsEnum.Right) != TileSideFlagsEnum.None)
        {
            CalcSmoothBlockFace(x, y, z, tiletype, vOffset, vScale, currentChunk, TileSideEnum.Right);
        }
    }
    #endregion

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

        int curcolor = _colorWhite;
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

        _texrecLeft = 0;

        //top
        {
            int sidetexture = TorchTopTexture;
            float texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            float texrecBottom = texrecTop + _texrecHeight;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, top00.X, top00.Y, top00.Z, _texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, top01.X, top01.Y, top01.Z, _texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top10.X, top10.Y, top10.Z, _texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, top11.X, top11.Y, top11.Z, _texrecRight, texrecBottom, curcolor);
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
            float texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            float texrecBottom = texrecTop + _texrecHeight;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, bottom00.X, bottom00.Y, bottom00.Z, _texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom01.X, bottom01.Y, bottom01.Z, _texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom10.X, bottom10.Y, bottom10.Z, _texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom11.X, bottom11.Y, bottom11.Z, _texrecRight, texrecBottom, curcolor);
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
            float texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            float texrecBottom = texrecTop + _texrecHeight;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, bottom00.X, bottom00.Y, bottom00.Z, _texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom01.X, bottom01.Y, bottom01.Z, _texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top00.X, top00.Y, top00.Z, _texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, top01.X, top01.Y, top01.Z, _texrecRight, texrecTop, curcolor);
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
            float texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            float texrecBottom = texrecTop + _texrecHeight;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, bottom10.X, bottom10.Y, bottom10.Z, _texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom11.X, bottom11.Y, bottom11.Z, _texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top10.X, top10.Y, top10.Z, _texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, top11.X, top11.Y, top11.Z, _texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }

        {
            int sidetexture = TorchSideTexture;
            float texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            float texrecBottom = texrecTop + _texrecHeight;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, bottom00.X, bottom00.Y, bottom00.Z, _texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top00.X, top00.Y, top00.Z, _texrecRight, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom10.X, bottom10.Y, bottom10.Z, _texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top10.X, top10.Y, top10.Z, _texrecLeft, texrecTop, curcolor);
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
            float texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            float texrecBottom = texrecTop + _texrecHeight;
            ModelData toreturn = GetToReturn(tt, sidetexture);

            int lastelement = toreturn.verticesCount;
            ModelDataTool.AddVertex(toreturn, bottom01.X, bottom01.Y, bottom01.Z, _texrecLeft, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top01.X, top01.Y, top01.Z, _texrecLeft, texrecTop, curcolor);
            ModelDataTool.AddVertex(toreturn, bottom11.X, bottom11.Y, bottom11.Z, _texrecRight, texrecBottom, curcolor);
            ModelDataTool.AddVertex(toreturn, top11.X, top11.Y, top11.Z, _texrecRight, texrecTop, curcolor);
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
            throw new System.Exception("SmoothLight disabled not implemented");
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
