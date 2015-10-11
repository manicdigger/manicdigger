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

    internal bool EnableSmoothLight;

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
    internal bool[] isFluid;
    internal float[] lightlevels;

    internal ModelData[] toreturnatlas1d;
    internal ModelData[] toreturnatlas1dtransparent;

    internal float BlockShadow;
    internal bool option_DarkenBlockSides;
    internal bool option_DoNotDrawEdges;
    internal float AtiArtifactFix;

    VecCito3i[][] c_OcclusionNeighbors;

    float[] ref_blockCornerHeight;
    int[] tmpnPos;

    public TerrainChunkTesselatorCi()
    {
        EnableSmoothLight = true;
        ENABLE_TEXTURE_TILING = true;
        //option_HardWaterTesselation = true;
        _colorWhite = Game.ColorFromArgb(255, 255, 255, 255);
        BlockShadow = 0.6f;
        option_DarkenBlockSides = true;
        option_DoNotDrawEdges = true;
        occ = 0.7f;
        halfocc = 0.4f;
        tmpnPos = new int[7];
        tmpshadowration = new int[TileDirectionEnum.DirectionCounts];
        tmpoccupied = new bool[TileDirectionEnum.DirectionCounts];
        tmpfShadowRation = new float[4];
        tmpv = new VecCito3i();
        ref_blockCornerHeight = new float[4];

        c_OcclusionNeighbors = new VecCito3i[TileSideEnum.SideCount][];

        //Initialize array
        for (int i = 0; i < TileSideEnum.SideCount; i++)
        {
            c_OcclusionNeighbors[i] = new VecCito3i[TileDirectionEnum.DirectionCounts];
        }

        //Top
        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.Center] = VecCito3i.CitoCtr(0, 0, 1);

        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.Top] = VecCito3i.CitoCtr(0, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(0, 1, 1);

        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.Left] = VecCito3i.CitoCtr(-1, 0, 1);
        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.Right] = VecCito3i.CitoCtr(1, 0, 1);

        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(-1, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(1, -1, 1);

        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(-1, 1, 1);
        c_OcclusionNeighbors[TileSideEnum.Top][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(1, 1, 1);

        //Left
        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.Center] = VecCito3i.CitoCtr(-1, 0, 0);

        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.Top] = VecCito3i.CitoCtr(-1, 0, 1);
        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(-1, 0, -1);

        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.Left] = VecCito3i.CitoCtr(-1, -1, 0);
        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.Right] = VecCito3i.CitoCtr(-1, 1, 0);

        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(-1, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(-1, 1, 1);

        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(-1, -1, -1);
        c_OcclusionNeighbors[TileSideEnum.Left][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(-1, 1, -1);

        //Bottom
        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.Center] = VecCito3i.CitoCtr(0, 0, -1);

        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.Top] = VecCito3i.CitoCtr(0, 1, -1);
        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(0, -1, -1);

        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.Left] = VecCito3i.CitoCtr(-1, 0, -1);
        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.Right] = VecCito3i.CitoCtr(1, 0, -1);

        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(-1, 1, -1);
        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(1, 1, -1);

        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(-1, -1, -1);
        c_OcclusionNeighbors[TileSideEnum.Bottom][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(1, -1, -1);

        //Right
        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.Center] = VecCito3i.CitoCtr(1, 0, 0);

        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.Top] = VecCito3i.CitoCtr(1, 0, 1);
        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(1, 0, -1);

        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.Left] = VecCito3i.CitoCtr(1, 1, 0);
        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.Right] = VecCito3i.CitoCtr(1, -1, 0);

        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(1, 1, 1);
        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(1, -1, 1);

        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(1, 1, -1);
        c_OcclusionNeighbors[TileSideEnum.Right][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(1, -1, -1);

        //Back
        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.Center] = VecCito3i.CitoCtr(0, -1, 0);

        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.Top] = VecCito3i.CitoCtr(0, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(0, -1, -1);

        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.Left] = VecCito3i.CitoCtr(1, -1, 0);
        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.Right] = VecCito3i.CitoCtr(-1, -1, 0);

        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(1, -1, 1);
        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(-1, -1, 1);

        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(1, -1, -1);
        c_OcclusionNeighbors[TileSideEnum.Back][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(-1, -1, -1);

        //Front
        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.Center] = VecCito3i.CitoCtr(0, 1, 0);

        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.Top] = VecCito3i.CitoCtr(0, 1, 1);
        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.Bottom] = VecCito3i.CitoCtr(0, 1, -1);

        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.Left] = VecCito3i.CitoCtr(-1, 1, 0);
        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.Right] = VecCito3i.CitoCtr(1, 1, 0);

        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.TopLeft] = VecCito3i.CitoCtr(-1, 1, 1);
        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.TopRight] = VecCito3i.CitoCtr(1, 1, 1);

        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.BottomLeft] = VecCito3i.CitoCtr(-1, 1, -1);
        c_OcclusionNeighbors[TileSideEnum.Front][TileDirectionEnum.BottomRight] = VecCito3i.CitoCtr(1, 1, -1);

    }


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
        mapsizex = game.map.MapSizeX;
        mapsizey = game.map.MapSizeY;
        mapsizez = game.map.MapSizeZ;
        started = true;

        istransparent = new bool[GlobalVar.MAX_BLOCKTYPES];
        for (int i = 0; i < GlobalVar.MAX_BLOCKTYPES; i++)
        {
            istransparent[i] = false;
        }
        isLowered = new bool[GlobalVar.MAX_BLOCKTYPES];
        for (int i = 0; i < GlobalVar.MAX_BLOCKTYPES; i++)
        {
            isLowered[i] = false;
        }
        isFluid = new bool[GlobalVar.MAX_BLOCKTYPES];
        for (int i = 0; i < GlobalVar.MAX_BLOCKTYPES; i++)
        {
            isFluid[i] = false;
        }
        maxlightInverse = 1f / maxlight;
        terrainTexturesPerAtlas = game.terrainTexturesPerAtlas;
        terrainTexturesPerAtlasInverse = 1f / game.terrainTexturesPerAtlas;

        if (game.platform.IsFastSystem())
        {
            AtiArtifactFix = 1 / 32f * 0.25f;  // 32 pixels in block texture
        }
        else
        {
            // WebGL
            AtiArtifactFix = 1 / 32f * 1.5f;  // 32 pixels in block texture
        }

        _texrecWidth = 1 - (AtiArtifactFix * 2);
        _texrecHeight = terrainTexturesPerAtlasInverse * (1 - (AtiArtifactFix * 2));
        _texrecLeft = AtiArtifactFix;
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

    // <summary>
    // Calculate visible block faces for a chunk
    // This method checks which faces of a block should be drawn
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

                    //calculate neighbor block positions
                    int[] nPos = tmpnPos;
                    nPos[TileSideEnum.Top] = pos + movez;
                    nPos[TileSideEnum.Bottom] = pos - movez;
                    nPos[TileSideEnum.Front] = pos + (chunksize + 2);
                    nPos[TileSideEnum.Back] = pos - (chunksize + 2);
                    nPos[TileSideEnum.Left] = pos - 1;
                    nPos[TileSideEnum.Right] = pos + 1;

                    bool blnIsFluid = isFluid[tt];
                    bool blnIsLowered = isLowered[tt];

                    //check which faces are visible
                    draw |= GetFaceVisibility(TileSideEnum.Top, currentChunk, nPos, blnIsFluid, blnIsLowered);
                    draw |= GetFaceVisibility(TileSideEnum.Bottom, currentChunk, nPos, blnIsFluid, blnIsLowered);
                    draw |= GetFaceVisibility(TileSideEnum.Left, currentChunk, nPos, blnIsFluid, blnIsLowered);
                    draw |= GetFaceVisibility(TileSideEnum.Right, currentChunk, nPos, blnIsFluid, blnIsLowered);
                    draw |= GetFaceVisibility(TileSideEnum.Back, currentChunk, nPos, blnIsFluid, blnIsLowered);
                    draw |= GetFaceVisibility(TileSideEnum.Front, currentChunk, nPos, blnIsFluid, blnIsLowered);

                    if (blnIsLowered && draw > TileSideFlagsEnum.None)
                    {
                        //Check if top needs to be rendered
                        if (!TileSideFlagsEnum.HasFlag(draw, TileSideFlagsEnum.Top))
                        {
                            //if one side is visible, top needs to be rendered
                            if (TileSideFlagsEnum.HasFlag(draw, TileSideFlagsEnum.Front | TileSideFlagsEnum.Back | TileSideFlagsEnum.Right | TileSideFlagsEnum.Left))
                            {
                                draw |= TileSideFlagsEnum.Top;
                            }
                        }

                        //check if we have a rail
                        int nRail = Rail(tt);

                        if (nRail > 0)
                        {
                            int nSlope = GetRailSlope(xx, yy, zz);

                            //always draw raised slope sides
                            switch (nSlope)
                            {
                                case RailSlopeEnum.TwoDownRaised: draw |= TileSideFlagsEnum.Right | TileSideFlagsEnum.Front | TileSideFlagsEnum.Back; break;
                                case RailSlopeEnum.TwoUpRaised: draw |= TileSideFlagsEnum.Left | TileSideFlagsEnum.Front | TileSideFlagsEnum.Back; break;
                                case RailSlopeEnum.TwoLeftRaised: draw |= TileSideFlagsEnum.Front | TileSideFlagsEnum.Right | TileSideFlagsEnum.Left; break;
                                case RailSlopeEnum.TwoRightRaised: draw |= TileSideFlagsEnum.Back | TileSideFlagsEnum.Right | TileSideFlagsEnum.Left; break;
                            }
                        }
                    }

                    //Store drawing flags
                    currentChunkDraw16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)] = Game.IntToByte(draw);
                }
            }
        }
    }

    // <summary>
    // Check if a face should be drawn
    // </summary>
    int GetFaceVisibility(int nSide, int[] currentChunk, int[] nPos, bool blnIsFluid, bool blnIsLowered)
    {
        int nReturn = TileSideFlagsEnum.None;

        int nIndex = nPos[nSide];

        int tt2 = currentChunk[nIndex];

        if (tt2 == 0 || (istransparent[tt2] && !isLowered[tt2]) || (isFluid[tt2] && !blnIsFluid))
        {
            //Transparent or none block nearby
            nReturn |= TileSideEnum.ToFlags(nSide);
        }
        else if (blnIsFluid && nSide != TileSideEnum.Bottom)
        {
            //fluid branch
            
            //Commented out because it is causing unnecessary lava layer rendering of map bottom
            //int top = currentChunk[nPos[TileSideEnum.Top]];
            //if (nSide == TileSideEnum.Top)
            //{
            //    //a fluids topside maybe needs to be drawn, even if it is completly surrounded
            //    if (top != 0 && !isFluid[top])
            //    {
            //        //Is surrounded and has a solid block above
            //        nReturn |= TileSideEnum.ToFlags(TileSideEnum.Top);
            //    }
            //}

            //water below?
            if (isFluid[currentChunk[nPos[TileSideEnum.Bottom]]])
            {
                //check if a lowered waterblock is below the neighbor
                if (!isFluid[tt2])
                {
                    int movez = (chunksize + 2) * (chunksize + 2);
                    int nPos2 = nPos[nSide] - movez;

                    if (nPos2 > 0 && isFluid[currentChunk[nPos2]])
                    {
                        nReturn |= TileSideEnum.ToFlags(nSide);
                    }
                }
            }
            else
            {//no water below, nothing to do
            }
        }


        if (isLowered[tt2] && nSide != TileSideEnum.Top)
        {
            //the other block is lowered

            if (!blnIsLowered)
            {
                //The other block is lowered, but this one is not,
                nReturn |= TileSideEnum.ToFlags(nSide);
            }
            else if (nSide == TileSideEnum.Bottom)
            {
                //we need the bottom, since we have a lowered block below
                nReturn |= TileSideFlagsEnum.Bottom;
            }
            else
            {
                //this one is also lowered
                //top is always visible, if a lowered is nearby
                nReturn |= TileSideFlagsEnum.Top;
            }
        }
        //else
        //{//hidden
        //}

        return nReturn;
    }

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

                        if ((draw & TileSideFlagsEnum.Top) != 0)
                        {
                            int shadowratioTop = GetShadowRatio(xx, yy, zz + 1);
                            currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Top] = 1;
                        }
                        if ((draw & TileSideFlagsEnum.Bottom) != 0)
                        {
                            int shadowratioTop = GetShadowRatio(xx, yy, zz - 1);
                            currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Bottom] = 1;
                        }
                        if ((draw & TileSideFlagsEnum.Right) != 0)
                        {
                            int shadowratioTop = GetShadowRatio(xx - 1, yy, zz);
                            currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Left] = 1;
                        }
                        if ((draw & TileSideFlagsEnum.Left) != 0)
                        {
                            int shadowratioTop = GetShadowRatio(xx + 1, yy, zz);
                            currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Right] = 1;
                        }
                        if ((draw & TileSideFlagsEnum.Front) != 0)
                        {
                            int shadowratioTop = GetShadowRatio(xx, yy - 1, zz);
                            currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Back] = 1;
                        }
                        if ((draw & TileSideFlagsEnum.Back) != 0)
                        {
                            int shadowratioTop = GetShadowRatio(xx, yy + 1, zz);
                            currentChunkDrawCount16[Index3d(xx - 1, yy - 1, zz - 1, chunksize, chunksize)][TileSideEnum.Front] = 1;
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
        else if (dir == TileSideEnum.Front || dir == TileSideEnum.Back)
        {
            int shadowx = dir == TileSideEnum.Front ? -1 : 1;
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
            int shadowy = dir == TileSideEnum.Left ? -1 : 1;
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

    public void BuildBlockPolygons(int x, int y, int z)
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

                        BuildSingleBlockPolygon(xxx, yyy, zzz, currentChunk18);
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
            game.platform.FloatToInt(Game.ColorB(color) * fValue));
    }

    internal float occ;
    internal float halfocc;

    bool[] tmpoccupied;
    int[] tmpshadowration;
    float[] tmpfShadowRation;
    void BuildBlockFace(int x, int y, int z, int tileType, float vOffsetX, float vOffsetY, float vOffsetZ,
        float vScaleX, float vScaleY, float vScaleZ, int[] currentChunk, int tileSide)
    {
        int xx = x % chunksize + 1;
        int yy = y % chunksize + 1;
        int zz = z % chunksize + 1;
        VecCito3i[] vNeighbors = c_OcclusionNeighbors[tileSide];

        int[] shadowration = tmpshadowration;
        bool[] occupied = tmpoccupied;

        int shadowratio = GetShadowRatio(vNeighbors[TileDirectionEnum.Center].x + xx,
            vNeighbors[TileDirectionEnum.Center].y + yy,
            vNeighbors[TileDirectionEnum.Center].z + zz);

        //initialize shadow values
        float[] fShadowRation = tmpfShadowRation;
        float shadowratiomain = lightlevels[shadowratio];
        fShadowRation[0] = shadowratiomain;
        fShadowRation[1] = shadowratiomain;
        fShadowRation[2] = shadowratiomain;
        fShadowRation[3] = shadowratiomain;

        if (EnableSmoothLight)
        {
            //Get occupation and int shadowRation
            for (int i = 0; i < TileDirectionEnum.DirectionCounts; i++)
            {
                int vPosX = vNeighbors[i].x + xx;
                int vPosY = vNeighbors[i].y + yy;
                int vPosZ = vNeighbors[i].z + zz;
                int nBlockType = currentChunk[Index3d(vPosX, vPosY, vPosZ, chunksize + 2, chunksize + 2)];

                if (nBlockType != 0)
                {
                    occupied[i] = !IsTransparentForLight(nBlockType);
                    shadowration[i] = shadowratio;
                }
                else
                {
                    occupied[i] = false;
                    shadowration[i] = GetShadowRatio(vPosX, vPosY, vPosZ);
                }
            }

            //Shadows
            CalcShadowRation(TileDirectionEnum.Top, TileDirectionEnum.Left, TileDirectionEnum.TopLeft, CornerEnum.TopLeft, fShadowRation, occupied, shadowration);
            CalcShadowRation(TileDirectionEnum.Top, TileDirectionEnum.Right, TileDirectionEnum.TopRight, CornerEnum.TopRight, fShadowRation, occupied, shadowration);
            CalcShadowRation(TileDirectionEnum.Bottom, TileDirectionEnum.Left, TileDirectionEnum.BottomLeft, CornerEnum.BottomLeft, fShadowRation, occupied, shadowration);
            CalcShadowRation(TileDirectionEnum.Bottom, TileDirectionEnum.Right, TileDirectionEnum.BottomRight, CornerEnum.BottomRight, fShadowRation, occupied, shadowration);
        }
        else
        {//no smoothing
        }

        DrawBlockFace(x, y, z, tileType, tileSide, vOffsetX, vOffsetY, vOffsetZ, vScaleX, vScaleY, vScaleZ, vNeighbors, fShadowRation);
    }

    void CalcShadowRation(int nDir1, int nDir2, int nDirBetween, int nCorner, float[] fShadowRation, bool[] occupied, int[] shadowRationInt)
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

    VecCito3i tmpv;
    void DrawBlockFace(int x, int y, int z, int tileType, int tileSide, float vOffsetX, float vOffsetY, float vOffsetZ, float vScaleX, float vScaleY, float vScaleZ, VecCito3i[] vNeighbors, float[] fShadowRation)
    {
        int color = _colorWhite;

        //Darken shadow sides
        if (option_DarkenBlockSides)
        {
            switch (tileSide)
            {
                case TileSideEnum.Bottom:
                case TileSideEnum.Left:
                case TileSideEnum.Right:
                    color = ColorMultiply(color, BlockShadow);
                    break;
            }
        }

        int sidetexture = TextureId(tileType, tileSide);
        ModelData toreturn = GetModelData(tileType, sidetexture);
        float texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas)) + (AtiArtifactFix * terrainTexturesPerAtlasInverse);
        float texrecBottom = texrecTop +_texrecHeight;
        int lastelement = toreturn.verticesCount;

        VecCito3i v = tmpv;
        float fSlopeModifier = 0f;

        //Calculate the corner points
        vNeighbors[TileDirectionEnum.TopRight].Add(1, 1, 1, v);
        fSlopeModifier = GetCornerHeightModifier(tileSide, CornerEnum.TopRight);
        float xPos = x + vOffsetX + ((v.x * 0.5f) * vScaleX);
        float zPos = z + vOffsetZ + ((v.z * 0.5f) * vScaleZ) + fSlopeModifier;
        float yPos = y + vOffsetY + ((v.y * 0.5f) * vScaleY);
        ModelDataTool.AddVertex(toreturn, xPos, zPos , yPos, _texrecRight, texrecTop, ColorMultiply(color, fShadowRation[CornerEnum.TopRight]));

        vNeighbors[TileDirectionEnum.TopLeft].Add(1, 1, 1, v);
        fSlopeModifier = GetCornerHeightModifier(tileSide, CornerEnum.TopLeft);
        xPos = x + vOffsetX + ((v.x * 0.5f) * vScaleX);
        zPos = z + vOffsetZ + ((v.z * 0.5f) * vScaleZ) + fSlopeModifier;
        yPos = y + vOffsetY + ((v.y * 0.5f) * vScaleY);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecLeft, texrecTop, ColorMultiply(color, fShadowRation[CornerEnum.TopLeft]));

        vNeighbors[TileDirectionEnum.BottomRight].Add(1, 1, 1, v);
        fSlopeModifier = GetCornerHeightModifier(tileSide, CornerEnum.BottomRight);
        xPos = x + vOffsetX + ((v.x * 0.5f) * vScaleX);
        zPos = z + vOffsetZ + ((v.z * 0.5f) * vScaleZ) + fSlopeModifier;
        yPos = y + vOffsetY + ((v.y * 0.5f) * vScaleY);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecRight, texrecBottom, ColorMultiply(color, fShadowRation[CornerEnum.BottomRight]));

        vNeighbors[TileDirectionEnum.BottomLeft].Add(1, 1, 1, v);
        fSlopeModifier = GetCornerHeightModifier(tileSide, CornerEnum.BottomLeft);
        xPos = x + vOffsetX + ((v.x * 0.5f) * vScaleX);
        zPos = z + vOffsetZ + ((v.z * 0.5f) * vScaleZ) + fSlopeModifier;
        yPos = y + vOffsetY + ((v.y * 0.5f) * vScaleY);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecLeft, texrecBottom, ColorMultiply(color, fShadowRation[CornerEnum.BottomLeft]));

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
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Left, nToDraw, TileSideFlagsEnum.Right);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Right, nToDraw, TileSideFlagsEnum.Left);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Back, nToDraw, TileSideFlagsEnum.Front);
        nToDraw = SetVisibleFlag(drawFlags, TileSideEnum.Front, nToDraw, TileSideFlagsEnum.Back);

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

    public void BuildSingleBlockPolygon(int x, int y, int z, int[] currentChunk)
    {
        //slope height
        for (int i = 0; i < 4; i++)
        {
            ref_blockCornerHeight[i] = 0;
        }

        int xx = x % chunksize + 1;
        int yy = y % chunksize + 1;
        int zz = z % chunksize + 1;

        int nToDraw = GetToDrawFlags(xx, yy, zz);
        int tiletype = currentChunk[Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];

        float vOffsetX = 0;
        float vOffsetY = 0;
        float vOffsetZ = 0;
        float vScaleX = 1;
        float vScaleY = 1;
        float vScaleZ = 1;

        if (!isvalid(tiletype))
        {
            return;
        }
        if (nToDraw == TileSideFlagsEnum.None)
        {
            //nothing to do
            return;
        }

        if (option_DoNotDrawEdges)
        {
            //On finite map don't draw borders:
            //they can't be seen without freemove cheat.
            if (z == 0) { nToDraw &= ~TileSideFlagsEnum.Bottom; }
            if (x == 0) { nToDraw &= ~TileSideFlagsEnum.Front; }
            if (x == mapsizex - 1) { nToDraw &= ~TileSideFlagsEnum.Back; }
            if (y == 0) { nToDraw &= ~TileSideFlagsEnum.Left; }
            if (y == mapsizey - 1) { nToDraw &= ~TileSideFlagsEnum.Right; }
        }
        
        if (IsFlower(tiletype))
        {
            //Draw nothing but 2 faces. Prevents flickering.
            nToDraw = TileSideFlagsEnum.Front | TileSideFlagsEnum.Right;

            vScaleX = 0.9f;
            vScaleY = 0.9f;
            vScaleZ = 1f;

            //Draw Front and Left side
            BuildBlockFace(x, y, z, tiletype, 0.5f, 0.05f, 0f, vScaleX, vScaleY, vScaleZ, currentChunk, TileSideEnum.Left);
            BuildBlockFace(x, y, z, tiletype, 0.05f, 0.5f, 0f, vScaleX, vScaleY, vScaleZ, currentChunk, TileSideEnum.Back);
            return;//done
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Cactus)
        {
            //Cactus is thin
            float fScale = 0.875f;
            float fOffset = (1f - fScale) / 2f;

            //left right
            float vLROffsetX = fOffset;
            float vLROffsetY = 0;
            float vLROffsetZ = 0;
            float vLRScaleX = fScale;
            float vLRScaleY = 1f;
            float vLRScaleZ = 1f;

            //front back
            float vFBOffsetX = 0;
            float vFBOffsetY = fOffset;
            float vFBOffsetZ = 0;
            float vFBScaleX = 1f;
            float vFBScaleY = fScale;
            float vFBScaleZ = 1f;

            //Cactus sides need always to be drawn
            BuildBlockFace(x, y, z, tiletype, vLROffsetX, vLROffsetY, vLROffsetZ, vLRScaleX, vLRScaleY, vLRScaleZ, currentChunk, TileSideEnum.Left);
            BuildBlockFace(x, y, z, tiletype, vLROffsetX, vLROffsetY, vLROffsetZ, vLRScaleX, vLRScaleY, vLRScaleZ, currentChunk, TileSideEnum.Right);

            BuildBlockFace(x, y, z, tiletype, vFBOffsetX, vFBOffsetY, vFBOffsetZ, vFBScaleX, vFBScaleY, vFBScaleZ, currentChunk, TileSideEnum.Front);
            BuildBlockFace(x, y, z, tiletype, vFBOffsetX, vFBOffsetY, vFBOffsetZ, vFBScaleX, vFBScaleY, vFBScaleZ, currentChunk, TileSideEnum.Back);

            //continue to draw top and bottom
            nToDraw = nToDraw & (TileSideFlagsEnum.Top | TileSideFlagsEnum.Bottom);
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.OpenDoorLeft ||
                 game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.OpenDoorRight)//TODO: is this one ever used?
        {
            bool blnDrawn = false;
            
            //float fOffset = 0.01f; //does not display on certain distances
            float fOffset = 0.025f;

            //rigt to left
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0 &&
                currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                nToDraw = TileSideFlagsEnum.Left;
                vOffsetX = 0;
                vOffsetY = fOffset; //do not stuck in the wall
                vOffsetZ = 0;
                blnDrawn = true;
            }
            //front to back
            if (!blnDrawn || //draw at least one side
                currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0 &&
                currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
            {
                vOffsetX = fOffset;
                vOffsetY = 0;
                vOffsetZ = 0;
                nToDraw = TileSideFlagsEnum.Front;//do not stuck in the wall
            }
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Fence ||
                 game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.ClosedDoor) // fence tiles automatically when another fence is beside
        {
            bool blnSideDrawn = false;

            //left to right
            if (currentChunk[Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] != 0 ||
                currentChunk[Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] != 0)
            {
                BuildBlockFace(x, y, z, tiletype, 0, -0.5f, 0, vScaleX, vScaleY, vScaleZ, currentChunk, TileSideEnum.Front);
                blnSideDrawn = true;
            }

            //front to back
            if (!blnSideDrawn || // draw at least one side
                currentChunk[Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] != 0 ||
                currentChunk[Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] != 0)
            {
                BuildBlockFace(x, y, z, tiletype, 0.5f, 0, 0, vScaleX, vScaleY, vScaleZ, currentChunk, TileSideEnum.Left);
            }

            return;
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Ladder) // try to fit ladder to best wall or existing ladder
        {
            //bring it away from the wall
            vOffsetX = 0.025f;
            vOffsetY = 0.025f;
            vOffsetZ = 0;
            vScaleX = 0.95f;
            vScaleY = 0.95f;
            vScaleZ = 1f;

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
                    //TODO: remove magic numbers
                case 1: nToDraw |= TileSideFlagsEnum.Right; break;
                case 2: nToDraw |= TileSideFlagsEnum.Front; break;
                case 3: nToDraw |= TileSideFlagsEnum.Back; break;
                default: nToDraw |= TileSideFlagsEnum.Left; break;
            }
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.HalfHeight)
        {
            vScaleX = 1;
            vScaleY = 1;
            vScaleZ = 0.5f;
        }
        else if (game.blocktypes[tiletype].DrawType == Packet_DrawTypeEnum.Flat)
        {
            vScaleX = 1;
            vScaleY = 1;
            vScaleZ = 0.05f;
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
        else if (tiletype == 8)
        {
            //TODO: replace the (x == 8) in this part with (IsLiquid(x)) to make it work for all fluids
            if (currentChunk[Index3d(xx, yy, zz - 1, chunksize + 2, chunksize + 2)] == 8)
            {
                //flow down in the lower block
                vOffsetX = 0;
                vOffsetY = 0;
                vOffsetZ = -0.1f;
            }
            else
            {
                //lower than a normal block
                vScaleX = 1;
                vScaleY = 1;
                vScaleZ = 0.9f;
            }
        }
        else
        {
            int rail = Rail(tiletype);

            if (rail != RailDirectionFlagsEnum.None)
            {
                int slope = GetRailSlope(xx, yy, zz);
                float fSlopeMod = 1.0f;
                vScaleX = 1f;
                vScaleY = 1f;
                vScaleZ = 0.3f;
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
        }
        
        //Draw faces
        for (int i = 0; i < TileSideEnum.SideCount; i++)
        {
            if ((nToDraw & TileSideEnum.ToFlags(i)) != TileSideFlagsEnum.None)
            {
                BuildBlockFace(x, y, z, tiletype, vOffsetX, vOffsetY, vOffsetZ, vScaleX, vScaleY, vScaleZ, currentChunk, i);
            }
        }

    }

    public bool IsTransparentForLight(int block)
    {
        Packet_BlockType b = game.blocktypes[block];
        return b.DrawType != Packet_DrawTypeEnum.Solid && b.DrawType != Packet_DrawTypeEnum.ClosedDoor;
    }
    
    // <summary>
    // Get the model to which the vertices for this tiletype should be added
    // </summary>
    public ModelData GetModelData(int tiletype, int textureid)
    {
        if (isFluid[tiletype] || (istransparent[tiletype] && !isLowered[tiletype]))
        {
            return toreturnatlas1dtransparent[textureid / game.terrainTexturesPerAtlas];
        }
        else
        {
            return toreturnatlas1d[textureid / game.terrainTexturesPerAtlas];
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
        float torchsizexy = 0.16f;
        float topx = 0.5f - torchsizexy / 2;
        float topy = 0.5f - torchsizexy / 2;
        float bottomx = 0.5f - torchsizexy / 2;
        float bottomy = 0.5f - torchsizexy / 2;

        topx += x;
        topy += y;
        bottomx += x;
        bottomy += y;

        if (type == TorchTypeEnum.Front) { bottomx = x - torchsizexy; }
        if (type == TorchTypeEnum.Back) { bottomx = x + 1; }
        if (type == TorchTypeEnum.Left) { bottomy = y - torchsizexy; }
        if (type == TorchTypeEnum.Right) { bottomy = y + 1; }

        Vector3Ref top00 = Vector3Ref.Create(topx, z + 0.9f, topy);
        Vector3Ref top01 = Vector3Ref.Create(topx, z + 0.9f, topy + torchsizexy);
        Vector3Ref top10 = Vector3Ref.Create(topx + torchsizexy, z + 0.9f, topy);
        Vector3Ref top11 = Vector3Ref.Create(topx + torchsizexy, z + 0.9f, topy + torchsizexy);

        if (type == TorchTypeEnum.Left)
        {
            top01.Y += -0.1f;
            top11.Y += -0.1f;
        }

        if (type == TorchTypeEnum.Right)
        {
            top10.Y += -0.1f;
            top00.Y += -0.1f;
        }

        if (type == TorchTypeEnum.Front)
        {
            top10.Y += -0.1f;
            top11.Y += -0.1f;
        }

        if (type == TorchTypeEnum.Back)
        {
            top01.Y += -0.1f;
            top00.Y += -0.1f;
        }

        Vector3Ref bottom00 = Vector3Ref.Create(bottomx, z + 0, bottomy);
        Vector3Ref bottom01 = Vector3Ref.Create(bottomx, z + 0, bottomy + torchsizexy);
        Vector3Ref bottom10 = Vector3Ref.Create(bottomx + torchsizexy, z + 0, bottomy);
        Vector3Ref bottom11 = Vector3Ref.Create(bottomx + torchsizexy, z + 0, bottomy + torchsizexy);

        //top
        {
            int sidetexture = TorchTopTexture;
            float texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas));
            float texrecBottom = texrecTop + _texrecHeight;
            ModelData toreturn = GetModelData(tt, sidetexture);

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
            ModelData toreturn = GetModelData(tt, sidetexture);

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
            ModelData toreturn = GetModelData(tt, sidetexture);

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
            ModelData toreturn = GetModelData(tt, sidetexture);

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
            ModelData toreturn = GetModelData(tt, sidetexture);

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
            ModelData toreturn = GetModelData(tt, sidetexture);

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

            if((b.DrawType == Packet_DrawTypeEnum.HalfHeight) || (b.DrawType == Packet_DrawTypeEnum.Flat) || (b.GetRail() != 0))
            {
                isLowered[i] = true;
            }
            isFluid[i] = b.DrawType == Packet_DrawTypeEnum.Fluid;
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
        BuildBlockPolygons(x, y, z);
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
                    case CornerEnum.TopRight:
                        nIndex = CornerEnum.TopRight;
                        break;
                    case CornerEnum.TopLeft:
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
            case Left: return TileSideFlagsEnum.Front;
            case Right: return TileSideFlagsEnum.Back;
            case Back: return TileSideFlagsEnum.Left;
            case Front: return TileSideFlagsEnum.Right;
            default: return TileSideFlagsEnum.None;
        }
    }
}

public class TileSideFlagsEnum
{
    public const int None = 0;
    public const int Top = 1;
    public const int Bottom = 2;
    public const int Right = 4;
    public const int Left = 8;
    public const int Front = 16;
    public const int Back = 32;

    public static bool HasFlag(int nFlagA, int nFlagB)
    {
        return (nFlagA & nFlagB) != None;
    }
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

    public const int Center = 8;

    public const int DirectionCounts = 9;
}

public class CornerEnum
{
    public const int TopLeft = 0;
    public const int TopRight = 1;

    public const int BottomLeft = 2;
    public const int BottomRight = 3;

    public const int None = -1;
}

