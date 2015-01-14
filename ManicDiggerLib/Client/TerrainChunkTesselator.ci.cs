//Block definition:
// 
//      Z
//      |
//      | 
//      |
//      +----- X
//     / 
//    /
//   Y
//

// <summary>
// Generates triangles for a single 16x16x16 chunk.
// Needs to know the surrounding of the chunk (18x18x18 blocks total).
// This class is heavily inlined and unrolled for performance.
// Special-shape (rare) blocks don't need as much performance.
// </summary>
public class TerrainChunkTesselatorCi
{
    //internal float texrecTop;
    internal float _texrecLeft;
    internal float _texrecRight;
    internal float _texrecWidth;
    internal float _texrecHeight;
    internal int _colorWhite;

    VecCito3i[][] c_OcclusionNeighbors;
    float[] ref_blockCornerHeight;

    public TerrainChunkTesselatorCi()
    {
        one = 1;
        EnableSmoothLight = true;
        ENABLE_TEXTURE_TILING = true;
        //option_PreciseWaterTesselation = true;
        _colorWhite = Game.ColorFromArgb(255, 255, 255, 255);
        BlockShadow = 0.7f;
        DONOTDRAWEDGES = true;
        AtiArtifactFix = 0.995f;
        occ = 0.7f;
        halfocc = 0.4f;

        Yellowness = 1f; // lower is yellower
        Blueness = 0.9f; // lower is blue-er

        c_OcclusionNeighbors = new VecCito3i[TileSideEnum.SideCount][];

        //Initialize array
        for (int i = 0; i < TileSideEnum.SideCount; i++)
        {
            c_OcclusionNeighbors[i] = new VecCito3i[TileDirectionEnum.DirectionCounts];
        }

        //Top
        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.Top] = VecCito3i.CitoCtr(0, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(0, 1, 1);

        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.Left] = VecCito3i.CitoCtr(-1, 0, 1);
        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.Right] = VecCito3i.CitoCtr(1, 0, 1);

        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(-1, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(1, -1, 1);

        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(-1, 1, 1);
        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(1, 1, 1);

        //Left
        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.Top] = VecCito3i.CitoCtr(-1, 0, 1);
        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(-1, 0, -1);

        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.Left] = VecCito3i.CitoCtr(-1, -1, 0);
        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.Right] = VecCito3i.CitoCtr(-1, 1, 0);

        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(-1, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(-1, 1, 1);

        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(-1, -1, -1);
        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(-1, 1, -1);

        //Bottom
        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.Top] = VecCito3i.CitoCtr(0, 1, -1);
        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(-1, 0, -1);

        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.Left] = VecCito3i.CitoCtr(-1, 0, -1);
        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.Right] = VecCito3i.CitoCtr(1, 0, -1);

        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(-1, 1, -1);
        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(1, 1, -1);

        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(-1, -1, -1);
        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(1, -1, -1);

        //Right
        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.Top] = VecCito3i.CitoCtr(1, 0, 1);
        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(1, 0, -1);

        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.Left] = VecCito3i.CitoCtr(1, -1, 0);
        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.Right] = VecCito3i.CitoCtr(1, 1, 0);

        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(1, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(1, 1, 1);

        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(1, -1, -1);
        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(1, 1, -1);

        //Back
        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.Top] = VecCito3i.CitoCtr(0, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(0, -1, -1);

        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.Left] = VecCito3i.CitoCtr(1, -1, 0);
        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.Right] = VecCito3i.CitoCtr(-1, -1, 0);

        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(1, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(-1, -1, 1);

        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(1, -1, -1);
        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(-1, -1, -1);

        //Front
        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.Top] = VecCito3i.CitoCtr(0, 1, 1);
        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(0, 1, -1);

        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.Left] = VecCito3i.CitoCtr(-1, 1, 0);
        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.Right] = VecCito3i.CitoCtr(1, 1, 0);

        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(-1, 1, 1);
        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(1, 1, 1);

        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(-1, 1, -1);
        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(1, 1, -1);


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
    internal bool[] isLowered;
    internal float[] lightlevels;

    internal ModelData[] toreturnatlas1d;
    internal ModelData[] toreturnatlas1dtransparent;

    internal float BlockShadow;
    internal bool DONOTDRAWEDGES;
    internal bool option_HardWaterTesselation;
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

    static int Index3dVec(VecCito3i v)
    {
        return Index3d(v.x, v.y, v.z, chunksize + 2, chunksize + 2);
    }

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
        isLowered = new bool[GlobalVar.MAX_BLOCKTYPES];
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

    // <summary>
    // Calculate visible faces for a chunk
    // </summary>
    // <param name="currentChunk"></param>
    public void CalculateVisibleFaces(int[] currentChunk)
    {
        int movez = (chunksize + 2) * (chunksize + 2);

        for (int zz = 1; zz < chunksize + 1; zz++)
        {
            for (int yy = 1; yy < chunksize + 1; yy++)
            {
                int posstart = Index3d(0, yy, zz, chunksize + 2, chunksize + 2);
                for (int xx = 1; xx < chunksize + 1; xx++)
                {
                    int pos = posstart + xx;
                    int tt = currentChunk[pos];
                    if (tt == 0) { continue; } //nothing to do

                    int draw = TileSideFlagsEnum.None;

                    //Instead of calculating position index with MapUtil.Index(),
                    //relative moves are used
                    //(just addition instead of multiplication - 1.5x - 2x faster)
                    int[] nPos = new int[7];
                    nPos[TileSideEnum.Top] = pos + movez;
                    nPos[TileSideEnum.Bottom] = pos - movez;
                    nPos[TileSideEnum.Front] = pos + (chunksize + 2);
                    nPos[TileSideEnum.Back] = pos - (chunksize + 2);
                    nPos[TileSideEnum.Left] = pos - 1;
                    nPos[TileSideEnum.Right] = pos + 1;

                    bool blnIsFluid = IsWater(tt);

                    draw |= GetFaceVisibility(TileSideEnum.Top, currentChunk, nPos, blnIsFluid);
                    draw |= GetFaceVisibility(TileSideEnum.Bottom, currentChunk, nPos, blnIsFluid);
                    draw |= GetFaceVisibility(TileSideEnum.Left, currentChunk, nPos, blnIsFluid);
                    draw |= GetFaceVisibility(TileSideEnum.Right, currentChunk, nPos, blnIsFluid);
                    draw |= GetFaceVisibility(TileSideEnum.Back, currentChunk, nPos, blnIsFluid);
                    draw |= GetFaceVisibility(TileSideEnum.Front, currentChunk, nPos, blnIsFluid);

                    currentChunkDraw16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)] = Game.IntToByte(draw);
                }
            }
        }
    }

    // <summary>
    // Check if a face should be drawn
    // </summary>
    int GetFaceVisibility(int nSide, int[] currentChunk, int[] nPos, bool blnIsFluid)
    {
        int nReturn = TileSideFlagsEnum.None;

        int tt2 = currentChunk[nPos[nSide]];

        if (tt2 == 0 || istransparent[tt2] || (IsWater(tt2) && !blnIsFluid))
        {
            //Transparent nearbz
            return TileSideEnum.ToFlags(nSide);
        }
        else if (blnIsFluid && nSide != TileSideEnum.Bottom)
        {
            int top = currentChunk[nPos[TileSideEnum.Top]];

            if (nSide == TileSideEnum.Top)
            {
                //a fluids topside maybe needs to be drawn, even if it is completly surrounded
                if (top != 0 && !IsWater(top))
                {
                    //Is surrounded and has a solid block above
                    return TileSideEnum.ToFlags(TileSideEnum.Top);
                }
            }
            else if (option_HardWaterTesselation)
            {
                //water below?
                if (IsWater(currentChunk[nPos[TileSideEnum.Bottom]]))
                {
                    //check if a lowered waterblock is below the neighbor
                    if (!IsWater(tt2))
                    {
                        int movez = (chunksize + 2) * (chunksize + 2);
                        int nPos2 = nPos[nSide] - movez;

                        if (nPos2 > 0 && IsWater(currentChunk[nPos2]))
                        {
                            return TileSideEnum.ToFlags(nSide);
                        }
                    }
                }
                else
                {//no water below, nothing to do
                }
            }
            else
            {//hidden
            }
        }
        else
        {//hidden
        }
        
        return nReturn;
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
                                int shadowratioTop = GetShadowRatioOld(xx, yy, zz + 1, x, y, z + 1);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Top] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Top, TileSideFlags.Top);
                            }
                            if ((draw & TileSideFlagsEnum.Bottom) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx, yy, zz - 1, x, y, z - 1);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Bottom] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Bottom, TileSideFlags.Bottom);
                            }
                            if ((draw & TileSideFlagsEnum.Front) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx - 1, yy, zz, x - 1, y, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Left] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Front, TileSideFlags.Front);
                            }
                            if ((draw & TileSideFlagsEnum.Back) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx + 1, yy, zz, x + 1, y, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Right] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Back, TileSideFlags.Back);
                            }
                            if ((draw & TileSideFlagsEnum.Left) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx, yy - 1, zz, x, y - 1, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Back] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Left, TileSideFlags.Left);
                            }
                            if ((draw & TileSideFlagsEnum.Right) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx, yy + 1, zz, x, y + 1, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Front] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Right, TileSideFlags.Right);
                            }
                        }
                        else
                        {
                            if ((draw & TileSideFlagsEnum.Top) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx, yy, zz + 1, x, y, z + 1);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Top] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Top, TileSideFlagsEnum.Top));
                            }
                            if ((draw & TileSideFlagsEnum.Bottom) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx, yy, zz - 1, x, y, z - 1);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Bottom] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Bottom, TileSideFlagsEnum.Bottom));
                            }
                            if ((draw & TileSideFlagsEnum.Front) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx - 1, yy, zz, x - 1, y, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Left] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Left, TileSideFlagsEnum.Front));
                            }
                            if ((draw & TileSideFlagsEnum.Back) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx + 1, yy, zz, x + 1, y, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Right] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Right, TileSideFlagsEnum.Back));
                            }
                            if ((draw & TileSideFlagsEnum.Left) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx, yy - 1, zz, x, y - 1, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Back] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Back, TileSideFlagsEnum.Left));
                            }
                            if ((draw & TileSideFlagsEnum.Right) != 0)
                            {
                                int shadowratioTop = GetShadowRatioOld(xx, yy + 1, zz, x, y + 1, z);
                                currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Front] = Game.IntToByte(GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSideEnum.Front, TileSideFlagsEnum.Right));
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
                int shadowratio2 = GetShadowRatioOld(newxx, yy, zz + shadowz, x + (newxx - xx), y, z + shadowz);
                if (shadowratio != shadowratio2) { break; }
                if ((currentChunkDraw16[Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)] & dirflags) == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                currentChunkDrawCount16[Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)][dir] = 0;
                currentChunkDraw16[Index3d(newxx - 1, yy - 1, zz - 1, chunksize, chunksize)] &= Game.IntToByte(~dirflags);
                newxx++;
            }
            return newxx - xx;
        }
        else if (dir == TileSideEnum.Left || dir == TileSideEnum.Right)
        {
            int shadowx = dir == TileSideEnum.Left ? -1 : 1;
            int newyy = yy + 1;
            for (; ; )
            {
                if (newyy >= chunksize + 1) { break; }
                if (currentChunk[Index3d(xx, newyy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                int shadowratio2 = GetShadowRatioOld(xx + shadowx, newyy, zz, x + shadowx, y + (newyy - yy), z);
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
            int shadowy = dir == TileSideEnum.Back ? -1 : 1;
            int newxx = xx + 1;
            for (; ; )
            {
                if (newxx >= chunksize + 1) { break; }
                if (currentChunk[Index3d(newxx, yy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                int shadowratio2 = GetShadowRatioOld(newxx, yy + shadowy, zz, x + (newxx - xx), y + shadowy, z);
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

#if !CITO
    [System.Obsolete("Use GetShadowRation(int,int,int) instead")]
#endif
    public int GetShadowRatioOld(int xx, int yy, int zz, int globalx, int globaly, int globalz)
    {
        return GetShadowRatio(xx, yy, zz);
    }

    public int GetShadowRatioVec(VecCito3i v)
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

    int ColorMultiply(int color, float fValue)
    {
        return Game.ColorFromArgb(Game.ColorA(color),
            game.platform.FloatToInt(Game.ColorR(color) * fValue),
            game.platform.FloatToInt(Game.ColorG(color) * fValue),
            game.platform.FloatToInt(Game.ColorB(color) * fValue * Yellowness));
    }

    internal float occ;
    internal float halfocc;

    void CalcSmoothBlockFace(int x, int y, int z, int tileType, VecCito3f vOffset, VecCito3f vScale, int[] currentChunk, int tileSide)
    {
        int xx = x % chunksize + 1;
        int yy = y % chunksize + 1;
        int zz = z % chunksize + 1;
        VecCito3i[] vNeighbors = c_OcclusionNeighbors[tileSide];

        int[] shadowration = new int[TileDirectionEnum.DirectionCounts];
        bool[] occupied = new bool[TileDirectionEnum.DirectionCounts];
        int shadowratio = GetShadowRatio(xx, yy, zz + 1);

        //Get occupation and int shadowRation
        for (int i = 0; i < TileDirectionEnum.DirectionCounts; i++)
        {
            VecCito3i vPos = vNeighbors[i].Add(xx, yy, zz);
            int nBlockType = currentChunk[Index3dVec(vNeighbors[i].Add(xx, yy, zz))];

            if (nBlockType != 0)
            {
                occupied[i] = !IsTransparentForLight(nBlockType);
                shadowration[i] = shadowratio;
            }
            else
            {
                occupied[i] = false;
                shadowration[i] = GetShadowRatioVec(vPos);
            }
        }

        //initialize shadow values
        float[] fShadowRation = new float[4];
        float shadowratiomain = lightlevels[shadowratio];
        fShadowRation[0] = shadowratiomain;
        fShadowRation[1] = shadowratiomain;
        fShadowRation[2] = shadowratiomain;
        fShadowRation[3] = shadowratiomain;

        //Shadows
        CalcShadowRation(TileDirectionEnum.Top, TileDirectionEnum.Left, TileDirectionEnum.TopLeft, CornerEnum.TopLeft, fShadowRation, occupied, shadowration);
        CalcShadowRation(TileDirectionEnum.Top, TileDirectionEnum.Right, TileDirectionEnum.TopRight, CornerEnum.TopRight, fShadowRation, occupied, shadowration);
        CalcShadowRation(TileDirectionEnum.Bottom, TileDirectionEnum.Left, TileDirectionEnum.BottomLeft, CornerEnum.BottomLeft, fShadowRation, occupied, shadowration);
        CalcShadowRation(TileDirectionEnum.Bottom, TileDirectionEnum.Right, TileDirectionEnum.BottomRight, CornerEnum.BottomRight, fShadowRation, occupied, shadowration);

        DrawBlockFace(x, y, z, tileType, tileSide, vOffset, vScale, vNeighbors, fShadowRation);
    }

    void CalcShadowRation(int nDir1, int nDirBetween, int nDir2, int nCorner, float[] fShadowRation, bool[] occupied, int[] shadowRationInt)
    {
        if (occupied[nDir1] && occupied[nDir2]) 
        {
            fShadowRation[nCorner] *= halfocc;
        }
        else
        {
            byte facesconsidered = 1;
            if (!occupied[nDir1]) { fShadowRation[nCorner] += lightlevels[shadowRationInt[nDir1]]; facesconsidered++; }
            if (!occupied[nDir2]) { fShadowRation[nCorner] += lightlevels[shadowRationInt[nDir2]]; facesconsidered++; }
            if (!occupied[nDirBetween]) { fShadowRation[nCorner] += lightlevels[shadowRationInt[nDirBetween]]; facesconsidered++; }
            fShadowRation[nCorner] /= facesconsidered;

            if (occupied[nDir1] || occupied[nDir2] || occupied[nDirBetween])
            {
                fShadowRation[nCorner] *= occ;
            }
        }
    }

    void DrawBlockFace(int x, int y, int z, int tileType, int tileSide, VecCito3f vOffset, VecCito3f vScale, VecCito3i[] vNeighbors, float[] fShadowRation)
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

        VecCito3i v;
        float fSlopeModifier = 0f;

        //Calculate the corner points
        v = vNeighbors[TileDirectionEnum.TopRight].Add(1, 1, 1);
        fSlopeModifier = GetCornerHeightModifier(tileSide, CornerEnum.TopRight);
        float xPos = x + vOffset.x + ((v.x * 0.5f) * vScale.x);
        float zPos = z + vOffset.z + ((v.z * 0.5f) * vScale.z) + fSlopeModifier;
        float yPos = y + vOffset.y + ((v.y * 0.5f) * vScale.y);
        ModelDataTool.AddVertex(toreturn, xPos, zPos , yPos, _texrecRight, texrecTop, ColorMultiply(color, fShadowRation[CornerEnum.TopRight]));

        v = vNeighbors[TileDirectionEnum.TopLeft].Add(1, 1, 1);
        fSlopeModifier = GetCornerHeightModifier(tileSide, CornerEnum.TopLeft);
        xPos = x + vOffset.x + ((v.x * 0.5f) * vScale.x);
        zPos = z + vOffset.z + ((v.z * 0.5f) * vScale.z) + fSlopeModifier;
        yPos = y + vOffset.y + ((v.y * 0.5f) * vScale.y);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecLeft, texrecTop, ColorMultiply(color, fShadowRation[CornerEnum.TopLeft]));

        v = vNeighbors[TileDirectionEnum.BottomRight].Add(1, 1, 1);
        fSlopeModifier = GetCornerHeightModifier(tileSide, CornerEnum.BottomRight);
        xPos = x + vOffset.x + ((v.x * 0.5f) * vScale.x);
        zPos = z + vOffset.z + ((v.z * 0.5f) * vScale.z) + fSlopeModifier;
        yPos = y + vOffset.y + ((v.y * 0.5f) * vScale.y);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecRight, texrecBottom, ColorMultiply(color, fShadowRation[CornerEnum.BottomRight]));

        v = vNeighbors[TileDirectionEnum.BottomLeft].Add(1, 1, 1);
        fSlopeModifier = GetCornerHeightModifier(tileSide, CornerEnum.BottomLeft);
        xPos = x + vOffset.x + ((v.x * 0.5f) * vScale.x);
        zPos = z + vOffset.z + ((v.z * 0.5f) * vScale.z) + fSlopeModifier;
        yPos = y + vOffset.y + ((v.y * 0.5f) * vScale.y);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecLeft, texrecBottom, ColorMultiply(color, fShadowRation[CornerEnum.BottomLeft]));

        if (tileSide == TileSideEnum.Right)
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

    // <summary>
    // Returns the sides to draw for this block
    // </summary>
    int GetToDrawFlags(int xx, int yy, int zz)
    {
        int nToDraw = TileSideFlagsEnum.None;

        byte[] drawFlags = currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)];

        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Top, nToDraw, TileSideFlagsEnum.Top);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Bottom, nToDraw, TileSideFlagsEnum.Bottom);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Left, nToDraw, TileSideFlagsEnum.Front);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Right, nToDraw, TileSideFlagsEnum.Back);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Back, nToDraw, TileSideFlagsEnum.Left);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Front, nToDraw, TileSideFlagsEnum.Right);

        return nToDraw;
    }

    // <summary>
    // Sets the visible flag in the nCurrentFlags if this side needs to be drawn
    // </summary>
    int SetVisibleFlag(byte[] drawFlags, int tileSideIndex, int nCurrentFlags, int nFlagToSet)
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

    public void SmoothLightBlockPolygons(int x, int y, int z, int[] currentChunk)
    {
        //slope height
        ref_blockCornerHeight = new float[4];

        int xx = x % chunksize + 1;
        int yy = y % chunksize + 1;
        int zz = z % chunksize + 1;

        int nToDraw = GetToDrawFlags(xx, yy, zz);
        int tiletype = currentChunk[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];
        int rail = Rail(tiletype);

        VecCito3f vOffset = VecCito3f.CitoCtr(0, 0, 0);
        VecCito3f vScale = VecCito3f.CitoCtr(1, 1, 1);

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

            vScale = VecCito3f.CitoCtr(0.5f, 0.5f, 0.5f);

            //Draw Front and Left side
            CalcSmoothBlockFace(x, y, z, tiletype, VecCito3f.CitoCtr(0.5f, 0.25f, 0f), vScale, currentChunk, TileSideEnum.Left);
            CalcSmoothBlockFace(x, y, z, tiletype, VecCito3f.CitoCtr(0.25f, 0.5f, 0f), vScale, currentChunk, TileSideEnum.Back);
            return;
        }
        if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Cactus)
        {
            //Cactus is thin
            vOffset = VecCito3f.CitoCtr(0.2f, 0.2f, 0);
            vScale = VecCito3f.CitoCtr(0.625f, 0.625f, 1f);
            flowerfix = 0.0625f;
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.OpenDoorLeft)
        {
            nToDraw ^= TileSideFlagsEnum.Top;
            nToDraw ^= TileSideFlagsEnum.Bottom;
            flowerfix = 0.9f;
            //x-1, x1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0 &&
                currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                nToDraw ^= TileSideFlagsEnum.Back;
                nToDraw ^= TileSideFlagsEnum.Front;
                nToDraw ^= TileSideFlagsEnum.Right;

                nToDraw |= TileSideFlagsEnum.Left;
            }
            //y-1, y1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0 &&
                currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                nToDraw ^= TileSideFlagsEnum.Left;
                nToDraw ^= TileSideFlagsEnum.Right;
                nToDraw ^= TileSideFlagsEnum.Front;

                nToDraw |= TileSideFlagsEnum.Back;
            }
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.OpenDoorRight)
        {
            nToDraw ^= TileSideFlagsEnum.Top;
            nToDraw ^= TileSideFlagsEnum.Bottom;

            flowerfix = 0.9f;
            //x-1, x1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0 &&
                currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                nToDraw ^= TileSideFlagsEnum.Back;
                nToDraw ^= TileSideFlagsEnum.Front;
                nToDraw ^= TileSideFlagsEnum.Left;

                nToDraw |= TileSideFlagsEnum.Right;
            }
            //y-1, y1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0 &&
                currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                nToDraw ^= TileSideFlagsEnum.Back;
                nToDraw ^= TileSideFlagsEnum.Right;
                nToDraw ^= TileSideFlagsEnum.Left;

                nToDraw |= TileSideFlagsEnum.Front;
            }
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Fence ||
                 game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.ClosedDoor) // fence tiles automatically when another fence is beside
        {
            nToDraw = TileSideFlagsEnum.None;

            //x-1, x1
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] != 0 ||
                currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] != 0)
            {
                nToDraw |= TileSideFlagsEnum.Left;
            }
            //y-1, y1
            if (currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] != 0 ||
                currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] != 0)
            {
                nToDraw |= TileSideFlagsEnum.Front;
            }
            if ((nToDraw & (TileSideFlagsEnum.Back | TileSideFlagsEnum.Front | TileSideFlagsEnum.Right | TileSideFlagsEnum.Left)) == 0)
            {
                nToDraw |= TileSideFlagsEnum.Back;
                nToDraw |= TileSideFlagsEnum.Left;
            }
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Ladder) // try to fit ladder to best wall or existing ladder
        {
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
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.HalfHeight)
        {
            vScale = VecCito3f.CitoCtr(1, 1, 0.5f);
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Flat)
        {
            vScale = VecCito3f.CitoCtr(1, 1, 0.05f);
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Torch)
        {
            int type = TorchTypeEnum.Normal;
            if (CanSupportTorch(currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Front; }
            if (CanSupportTorch(currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Back; }
            if (CanSupportTorch(currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Left; }
            if (CanSupportTorch(currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchTypeEnum.Right; }
            TorchSideTexture = TextureId(tiletype, TileSideEnum.Left);
            TorchTopTexture = TextureId(tiletype, TileSideEnum.Top);
            AddTorch(x, y, z, type, tiletype);
            return;
        }
        else if (rail != RailDirectionFlagsEnum.None)
        {
            int slope = GetRailSlope(xx, yy, zz);
            float fSlopeMod = 1.0f;
            vScale = VecCito3f.CitoCtr(1f, 1f, 0.3f);
            if (slope == RailSlopeEnum.TwoRightRaised)
            {
                ref_blockCornerHeight[CornerEnum.TopRight] = fSlopeMod;
                ref_blockCornerHeight[CornerEnum.BottomRight] = fSlopeMod;
            }
            else if (slope == RailSlopeEnum.TwoLeftRaised)
            {
                ref_blockCornerHeight[CornerEnum.TopLeft] = fSlopeMod;
                ref_blockCornerHeight[CornerEnum.BottomLeft] = fSlopeMod;
            }
            else if (slope == RailSlopeEnum.TwoUpRaised)
            {
                ref_blockCornerHeight[CornerEnum.TopLeft] = fSlopeMod;
                ref_blockCornerHeight[CornerEnum.TopRight] = fSlopeMod;
            }
            else if (slope == RailSlopeEnum.TwoDownRaised)
            {
                ref_blockCornerHeight[CornerEnum.BottomLeft] = fSlopeMod;
                ref_blockCornerHeight[CornerEnum.BottomRight] = fSlopeMod;
            }
        }
        else if (tiletype == 8)
        {
            //TODO: replace the (x == 8) in this part with (IsLiquid(x)) to make it work for all fluids
            if (currentChunk[Index3d(xx, yy, zz - 1, chunksize + 2, chunksize + 2)] == 8)
            {
                //flow down in the lower block
                vOffset = VecCito3f.CitoCtr(0, 0, -0.1f);
            }
            else
            {
                //lower than a normal block
                vScale = VecCito3f.CitoCtr(1, 1, 0.9f);
            }
        }
        
        //Draw faces
        for (int i = 0; i < TileSideEnum.SideCount; i++)
        {
            if ((nToDraw & TileSideEnum.ToFlags(i)) != TileSideFlagsEnum.None)
            {
                CalcSmoothBlockFace(x, y, z, tiletype, vOffset, vScale, currentChunk, i);
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

            if((b.DrawType == Packet_DrawTypeEnum.HalfHeight) || (b.GetRail() != 0))
            {
                isLowered[i] = true;
            }
            
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
#if CITO
            CalculateSmoothBlockPolygons(x, y, z);
#else
            throw new System.Exception("SmoothLight disabled not implemented");
#endif
        }
        VerticesIndicesToLoad[] ret = GetFinalVerticesIndices(x, y, z, retCount);
        return ret;
    }

    // <summary>
    // Gets the CornerHeightModifier for a side corner out of the ref_blockCornerHeight
    // </summary>
    float GetCornerHeightModifier(int side, int corner)
    {
        int nIndex = CornerEnum.None;
        switch (side)
        {
            case TileSideEnum.Bottom:
                {
                    nIndex = CornerEnum.None; //bottom height is never modified
                    break;
                }
            case TileSideEnum.Right:
                switch (corner)
                {
                    //right side is mirrored
                    case CornerEnum.TopLeft:
                        nIndex = CornerEnum.TopRight;
                        break;
                    case CornerEnum.TopRight:
                        nIndex = CornerEnum.BottomRight;
                        break;
                }
                break;
            case TileSideEnum.Left:
                switch (corner)
                {
                    case CornerEnum.TopLeft:
                        nIndex = CornerEnum.TopLeft;
                        break;
                    case CornerEnum.TopRight:
                        nIndex = CornerEnum.BottomLeft;
                        break;
                }
                break;
            case TileSideEnum.Front:
                switch (corner)
                {
                    case CornerEnum.TopLeft:
                        nIndex = CornerEnum.BottomLeft;
                        break;
                    case CornerEnum.TopRight:
                        nIndex = CornerEnum.BottomRight;
                        break;
                }
                break;
            case TileSideEnum.Back:
                switch (corner)
                {
                    case CornerEnum.TopLeft:
                        nIndex = CornerEnum.TopRight;
                        break;
                    case CornerEnum.TopRight:
                        nIndex = CornerEnum.TopLeft;
                        break;
                }
                break;
            case TileSideEnum.Top:
                //Top side uses the same corner definition
                nIndex = corner;
                break;
        }

        if (nIndex != CornerEnum.None)
        {
            //Get height modifier
            return ref_blockCornerHeight[nIndex];
        }
        else
        {
            //default, do not modify
            return 0f;
        }
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
    public const int Left = 2;
    public const int Right = 3;
    public const int Back = 4;
    public const int Front = 5;

    public const int SideCount = 6;

    // <summary>
    // Convert to TileSideEnumFlags
    // </summary>
    public static int ToFlags(int nValue)
    {
        switch (nValue)
        {
            case Top: return TileSideFlagsEnum.Top;
            case Bottom: return TileSideFlagsEnum.Bottom;
            case Left: return TileSideFlagsEnum.Left;
            case Right: return TileSideFlagsEnum.Right;
            case Back: return TileSideFlagsEnum.Back;
            case Front: return TileSideFlagsEnum.Front;
            default: return TileSideFlagsEnum.None;
        }
    }
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

public class TileDirectionEnum
{
    public const int Top = 0;
    public const int Bottom = 1;

    public const int Left = 2;
    public const int Right = 3;

    public const int TopLeft = 4;
    public const int TopRight = 5;

    public const int BottomLeft = 6;
    public const int BottomRight = 7;

    public const int DirectionCounts = 8;
}

public class CornerEnum
{
    public const int TopLeft = 0;
    public const int TopRight = 1;

    public const int BottomLeft = 2;
    public const int BottomRight = 3;

    public const int None = -1;
}

public class VecCito3i
{
    public int x;
    public int y;
    public int z;

    public static VecCito3i CitoCtr(int _x, int _y, int _z)
    {
        VecCito3i v = new VecCito3i();
        v.x = _x;
        v.y = _y;
        v.z = _z;

        return v;
    }

    public VecCito3i Add(int _x, int _y, int _z)
    {
        return CitoCtr(this.x + _x, this.y + _y, this.z + _z);
    }
}

public class VecCito3f
{
    public float x;
    public float y;
    public float z;

    public static VecCito3f CitoCtr(float _x, float _y, float _z)
    {
        VecCito3f v = new VecCito3f();
        v.x = _x;
        v.y = _y;
        v.z = _z;

        return v;
    }

    public VecCito3f Add(float _x, float _y, float _z)
    {
        return CitoCtr(this.x + _x, this.y + _y, this.z + _z);
    }
}