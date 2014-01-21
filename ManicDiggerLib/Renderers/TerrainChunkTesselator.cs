using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Drawing;
using ManicDigger.Collisions;

namespace ManicDigger.Renderers
{
    //Generates triangles for a single 16x16x16 chunk.
    //Needs to know the surrounding of the chunk (18x18x18 blocks total).
    //This class is heavily inlined and unrolled for performance.
    //Special-shape (rare) blocks don't need as much performance.
    public class TerrainChunkTesselator
    {
        [Inject]
        public IMapStorage d_MapStorage;
        [Inject]
        public IMapStoragePortion d_MapStoragePortion;
        [Inject]
        public IMapStorageLight d_MapStorageLight;
        [Inject]
        public IGameData d_Data;
        [Inject]
        public BlockRendererTorch d_BlockRendererTorch;
        [Inject]
        public Config3d d_Config3d;
        [Inject]
        public ITerrainTextures d_TerrainTextures;
        [Inject]
        public ManicDiggerGameWindow game;
        RailMapUtil railmaputil;
        public bool DONOTDRAWEDGES = true;
        public int chunksize = 16; //16x16
        public int texturesPacked = GlobalVar.MAX_BLOCKTYPES_SQRT;
        public float BlockShadow = 0.6f;
        public bool ENABLE_ATLAS1D = true;
        int maxblocktypes = GlobalVar.MAX_BLOCKTYPES;
        public float occ = 0.7f;
        public float halfocc = 0.4f;
        public bool topoccupied = false;
        public bool bottomoccupied = false;
        public bool leftoccupied = false;
        public bool rightoccupied = false;
        public bool topleftoccupied = false;
        public bool toprightoccupied = false;
        public bool bottomleftoccupied = false;
        public bool bottomrightoccupied = false;
        public bool EnableSmoothLight = true;
        public float AtiArtifactFix = 0.995f;
        public float Yellowness = 1f;//lower is yellower//0.7
        public float Blueness = 0.9f;//lower is blue-er
        ushort[] currentChunk;
        bool started = false;
        int mapsizex; //cache
        int mapsizey;
        int mapsizez;
        public void Start()
        {
            currentChunk = new ushort[(chunksize + 2) * (chunksize + 2) * (chunksize + 2)];
            currentChunkShadows = new byte[(chunksize + 2) * (chunksize + 2) * (chunksize + 2)];
            currentChunkDraw = new byte[chunksize, chunksize, chunksize];
            currentChunkDrawCount = new byte[chunksize, chunksize, chunksize, 6];
            mapsizex = d_MapStorage.MapSizeX;
            mapsizey = d_MapStorage.MapSizeY;
            mapsizez = d_MapStorage.MapSizeZ;
            started = true;
            istransparent = d_Data.IsTransparent;
            isvalid = d_Data.IsValid;
            maxlight = game.maxlight;
            maxlightInverse = 1f / maxlight;
            terrainTexturesPerAtlas = d_TerrainTextures.terrainTexturesPerAtlas;
            terrainTexturesPerAtlasInverse = 1f / d_TerrainTextures.terrainTexturesPerAtlas;
            
        }
        int terrainTexturesPerAtlas;
        float terrainTexturesPerAtlasInverse;
        int maxlight;
        float maxlightInverse;
        bool[] istransparent;
        bool[] isvalid;

        void NewVi(ref VerticesIndices vi)
        {
            if (vi == null)
            {
                vi = new VerticesIndices()
                {
                    indices = new ushort[ushort.MaxValue],
                    vertices = new VertexPositionTexture[ushort.MaxValue],
                };
            }
            vi.verticesCount = 0;
            vi.indicesCount = 0;
        }

        public IEnumerable<VerticesIndicesToLoad> MakeChunk(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0) { yield break; }
            if (!started) { throw new Exception("not started"); }
            if (x >= mapsizex / chunksize
                || y >= mapsizey / chunksize
                || z >= mapsizez / chunksize) { yield break; }
            if (ENABLE_ATLAS1D)
            {
                if (toreturnatlas1d == null)
                {
                    toreturnatlas1d = new VerticesIndices[Math.Max(1, maxblocktypes / d_TerrainTextures.terrainTexturesPerAtlas)];
                    toreturnatlas1dtransparent = new VerticesIndices[Math.Max(1, maxblocktypes / d_TerrainTextures.terrainTexturesPerAtlas)];
                }
                for (int i = 0; i < toreturnatlas1d.Length; i++)
                {
                    //Manual memory allocation for performance - reuse arrays.
                    NewVi(ref toreturnatlas1d[i]);
                    NewVi(ref toreturnatlas1dtransparent[i]);
                }
            }
            //else //torch block
            {
                NewVi(ref toreturnmain);
                NewVi(ref toreturntransparent);
            }
            GetExtendedChunk(x, y, z);
            if (IsSolidChunk(currentChunk)) { yield break; }
            //ResetCurrentShadows();
            game.OnMakeChunk(x, y, z);
            CalculateVisibleFaces(currentChunk);
            CalculateTilingCount(currentChunk, x * chunksize, y * chunksize, z * chunksize);
            if (EnableSmoothLight)
            {
                CalculateSmoothBlockPolygons(x, y, z);
            }
            else
            {
                CalculateBlockPolygons(x, y, z);
            }
            foreach (VerticesIndicesToLoad v in GetFinalVerticesIndices(x, y, z))
            {
                yield return v;
            }
        }
        IEnumerable<VerticesIndicesToLoad> GetFinalVerticesIndices(int x, int y, int z)
        {
            if (ENABLE_ATLAS1D)
            {
                for (int i = 0; i < toreturnatlas1d.Length; i++)
                {
                    if (toreturnatlas1d[i].indicesCount > 0)
                    {
                        yield return new VerticesIndicesToLoad()
                        {
                            indices = Clone(toreturnatlas1d[i].indices, toreturnatlas1d[i].indicesCount),
                            indicesCount = toreturnatlas1d[i].indicesCount,
                            vertices = Clone(toreturnatlas1d[i].vertices, toreturnatlas1d[i].verticesCount),
                            verticesCount = toreturnatlas1d[i].verticesCount,
                            position =
                                new Vector3(x * chunksize, y * chunksize, z * chunksize),
                            texture = d_TerrainTextures.terrainTextures1d[i % d_TerrainTextures.terrainTexturesPerAtlas],
                        };
                    }
                }
                for (int i = 0; i < toreturnatlas1dtransparent.Length; i++)
                {
                    if (toreturnatlas1dtransparent[i].indicesCount > 0)
                    {
                        yield return new VerticesIndicesToLoad()
                        {
                            indices = Clone(toreturnatlas1dtransparent[i].indices, toreturnatlas1dtransparent[i].indicesCount),
                            indicesCount = toreturnatlas1dtransparent[i].indicesCount,
                            vertices = Clone(toreturnatlas1dtransparent[i].vertices, toreturnatlas1dtransparent[i].verticesCount),
                            verticesCount = toreturnatlas1dtransparent[i].verticesCount,
                            position =
                                new Vector3(x * chunksize, y * chunksize, z * chunksize),
                            texture = d_TerrainTextures.terrainTextures1d[i % d_TerrainTextures.terrainTexturesPerAtlas],
                            transparent = true,
                        };
                    }
                }
            }
            //else //torch block
            {
                if (toreturnmain.indicesCount > 0)
                {
                    yield return new VerticesIndicesToLoad()
                    {
                        indices = Clone(toreturnmain.indices, toreturnmain.indicesCount),
                        indicesCount = toreturnmain.indicesCount,
                        vertices = Clone(toreturnmain.vertices, toreturnmain.verticesCount),
                        verticesCount = toreturnmain.verticesCount,
                        position =
                            new Vector3(x * chunksize, y * chunksize, z * chunksize),
                        texture = d_TerrainTextures.terrainTexture,
                    };
                }
                if (toreturntransparent.indicesCount > 0)
                {
                    yield return new VerticesIndicesToLoad()
                    {
                        indices = Clone(toreturntransparent.indices, toreturntransparent.indicesCount),
                        indicesCount = toreturntransparent.indicesCount,
                        vertices = Clone(toreturntransparent.vertices, toreturntransparent.verticesCount),
                        verticesCount = toreturntransparent.verticesCount,
                        position =
                            new Vector3(x * chunksize, y * chunksize, z * chunksize),
                        transparent = true,
                        texture = d_TerrainTextures.terrainTexture,
                    };
                }
            }
        }
        T[] Clone<T>(T[] arr, int length)
        {
            T[] copy = new T[length];
            Array.Copy(arr, copy, length);
            return copy;
        }
        private void CalculateBlockPolygons(int x, int y, int z)
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
                        if (currentChunkDraw[xx, yy, zz] != 0)
                        {                            
                            BlockPolygons(xxx, yyy, zzz, currentChunk);
                        }
                    }
                }
            }
        }
        private void CalculateSmoothBlockPolygons(int x, int y, int z)
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
                        if (currentChunkDraw[xx, yy, zz] != 0)
                        {
                            SmoothLightBlockPolygons(xxx, yyy, zzz, currentChunk);
                        }
                    }
                }
            }
        }
        private void ResetCurrentShadows()
        {
            Array.Clear(currentChunkShadows, 0, currentChunkShadows.Length);
        }
        private bool IsSolidChunk(ushort[] currentChunk)
        {
            int block = currentChunk[0];
            for (int i = 0; i < currentChunk.Length; i++)
            {
                if (currentChunk[i] != currentChunk[0])
                {
                    return false;
                }
            }
            return true;
        }
        //For performance, make a local copy of chunk and its surrounding.
        //To render one chunk, we need to know all blocks that touch chunk boundaries.
        //(because to render a single block we need to know all 6 blocks around it).
        //So it's needed to copy 16x16x16 chunk and its Borders to make a 18x18x18 "extended" chunk.
        private void GetExtendedChunk(int x, int y, int z)
        {
            d_MapStoragePortion.GetMapPortion(currentChunk, x * chunksize - 1, y * chunksize - 1, z * chunksize - 1,
                chunksize + 2, chunksize + 2, chunksize + 2);
        }
        VerticesIndices toreturnmain;
        VerticesIndices toreturntransparent;
        VerticesIndices[] toreturnatlas1d;
        VerticesIndices[] toreturnatlas1dtransparent;
        class VerticesIndices
        {
            public ushort[] indices;
            public int indicesCount;
            public VertexPositionTexture[] vertices;
            public int verticesCount;
        }
        private bool IsValidPos(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0)
            {
                return false;
            }
            if (x >= mapsizex || y >= mapsizey || z >= mapsizez)
            {
                return false;
            }
            return true;
        }
        public byte[] currentChunkShadows;
        byte[, ,] currentChunkDraw;
        byte[, , ,] currentChunkDrawCount;
        void CalculateVisibleFaces(ushort[] currentChunk)
        {
            int chunksize = this.chunksize;
            int movez = (chunksize + 2) * (chunksize + 2);
            unsafe
            {
                fixed (ushort* currentChunk_ = currentChunk )
                fixed (bool* istransparent_ = istransparent)
                {
                    for (int zz = 1; zz < chunksize + 1; zz++)
                    {
                        for (int yy = 1; yy < chunksize + 1; yy++)
                        {
                            int posstart = MapUtil.Index3d(0, yy, zz, chunksize + 2, chunksize + 2);
                            for (int xx = 1; xx < chunksize + 1; xx++)
                            {
                                int pos = posstart + xx;
                                int tt = currentChunk_[pos];
                                if (tt == 0) { continue; }
                                int draw = (int)TileSideFlags.None;
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
                                        draw |= (int)TileSideFlags.Top;
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
                                        draw |= (int)TileSideFlags.Bottom;
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
                                        draw |= (int)TileSideFlags.Front;
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
                                        draw |= (int)TileSideFlags.Back;
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
                                        draw |= (int)TileSideFlags.Left;
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
                                        draw |= (int)TileSideFlags.Right;
                                    }
                                }
                                currentChunkDraw[xx - 1, yy - 1, zz - 1] = (byte)draw;
                            }
                        }
                    }
                }
            }
        }

        bool IsWater(int tt2)
        {
            return game.blocktypes[tt2].IsFluid();
        }

        private void CalculateTilingCount(ushort[] currentChunk, int startx, int starty, int startz)
        {
            Array.Clear(currentChunkDrawCount, 0, currentChunkDrawCount.Length);
            unsafe
            {
                fixed(ushort* currentChunk_ = currentChunk)
                for (int zz = 1; zz < chunksize + 1; zz++)
                {
                    for (int yy = 1; yy < chunksize + 1; yy++)
                    {
                        int pos = MapUtil.Index3d(0, yy, zz, chunksize + 2, chunksize + 2);
                        for (int xx = 1; xx < chunksize + 1; xx++)
                        {
                            int tt = currentChunk_[pos + xx];
                            if (tt == 0) { continue; } //faster
                            int x = startx + xx - 1;
                            int y = starty + yy - 1;
                            int z = startz + zz - 1;
                            int draw = currentChunkDraw[xx - 1, yy - 1, zz - 1];
                            if (draw == 0) { continue; } //faster
                            if (EnableSmoothLight)
                            {
                                if ((draw & (int)TileSideFlags.Top) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Top] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Top, TileSideFlags.Top);
                                }
                                if ((draw & (int)TileSideFlags.Bottom) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Bottom] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Bottom, TileSideFlags.Bottom);
                                }
                                if ((draw & (int)TileSideFlags.Front) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Front] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Front, TileSideFlags.Front);
                                }
                                if ((draw & (int)TileSideFlags.Back) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Back] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Back, TileSideFlags.Back);
                                }
                                if ((draw & (int)TileSideFlags.Left) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Left] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Left, TileSideFlags.Left);
                                }
                                if ((draw & (int)TileSideFlags.Right) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Right] = 1;// (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Right, TileSideFlags.Right);
                                }
                            }
                            else
                            {
                                if ((draw & (int)TileSideFlags.Top) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Top] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Top, TileSideFlags.Top);
                                }
                                if ((draw & (int)TileSideFlags.Bottom) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Bottom] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Bottom, TileSideFlags.Bottom);
                                }
                                if ((draw & (int)TileSideFlags.Front) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Front] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Front, TileSideFlags.Front);
                                }
                                if ((draw & (int)TileSideFlags.Back) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Back] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Back, TileSideFlags.Back);
                                }
                                if ((draw & (int)TileSideFlags.Left) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Left] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Left, TileSideFlags.Left);
                                }
                                if ((draw & (int)TileSideFlags.Right) != 0)
                                {
                                    int shadowratioTop = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
                                    currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Right] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Right, TileSideFlags.Right);
                                }
                            }
                        }
                    }
                }
            }
        }
        float texrecLeft;
        float texrecTop;
        float texrecWidth;
        float texrecHeight;
        FastColor ColorWhite = new FastColor(Color.White);
        private void BlockPolygons(int x, int y, int z, ushort[] currentChunk)
        {
            int xx = x % chunksize + 1;
            int yy = y % chunksize + 1;
            int zz = z % chunksize + 1;
            var tt = currentChunk[MapUtil.Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];
            if (!isvalid[tt])
            {
                return;
            }
            byte drawtop = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Top];
            byte drawbottom = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Bottom];
            byte drawfront = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Front];
            byte drawback = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Back];
            byte drawleft = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Left];
            byte drawright = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Right];
            int tiletype = tt;
            if (drawtop == 0 && drawbottom == 0 && drawfront == 0 && drawback == 0 && drawleft == 0 && drawright == 0)
            {
                return;
            }
            FastColor color = ColorWhite; //mapstorage.GetTerrainBlockColor(x, y, z);
            FastColor colorShadowSide = new FastColor(color.A,
                (int)(color.R * BlockShadow),
                (int)(color.G * BlockShadow),
                (int)(color.B * BlockShadow));
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
            if (d_Data.IsFlower[tiletype])
            {
                drawtop = 0;
                drawbottom = 0;
                flowerfix = 0.5f;
            }
            if (d_Data.DrawType1[tiletype] == DrawType.OpenDoorLeft)
            {
                drawtop = 0;
                drawbottom = 0;
                flowerfix = 0.9f;
                //x-1, x+1
                if (currentChunk[MapUtil.Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0
                    && currentChunk[MapUtil.Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
                {
                    drawback = 0;
                    drawfront = 0;
                    drawleft = 1;
                    drawright = 0;
                }
                //y-1, y+1
                if (currentChunk[MapUtil.Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0
                    && currentChunk[MapUtil.Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
                {
                    drawback = 1;
                    drawfront = 0;
                    drawleft = 0;
                    drawright = 0;
                }
            }
            if (d_Data.DrawType1[tiletype] == DrawType.OpenDoorRight)
            {
                drawtop = 0;
                drawbottom = 0;
                flowerfix = 0.9f;
                //x-1, x+1
                if (currentChunk[MapUtil.Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0
                    && currentChunk[MapUtil.Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
                {
                    drawback = 0;
                    drawfront = 0;
                    drawleft = 0;
                    drawright = 1;
                }
                //y-1, y+1
                if (currentChunk[MapUtil.Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0
                    && currentChunk[MapUtil.Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
                {
                    drawback = 0;
                    drawfront = 1;
                    drawleft = 0;
                    drawright = 0;
                }
            }
            if (d_Data.DrawType1[tiletype] == DrawType.Fence || d_Data.DrawType1[tiletype] == DrawType.ClosedDoor) // fence tiles automatically when another fence is beside
            {
                drawtop = 0;
                drawbottom = 0;
                drawfront = 0;
                drawback = 0;
                drawleft = 0;
                drawright = 0;
                flowerfix = 0.5f;

                //x-1, x+1
                if (currentChunk[MapUtil.Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] != d_Data.BlockIdEmpty
                    || currentChunk[MapUtil.Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] != d_Data.BlockIdEmpty)
                {
                    drawleft = 1;
                }
                //y-1, y+1
                if (currentChunk[MapUtil.Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] != d_Data.BlockIdEmpty
                    || currentChunk[MapUtil.Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] != d_Data.BlockIdEmpty)
                {
                    drawfront = 1;
                }
                if (drawback == 0 && drawfront == 0 && drawleft == 0 && drawright == 0)
                {
                    drawback = 1;
                    drawleft = 1;
                }
            }
            if (d_Data.DrawType1[tiletype] == DrawType.Ladder) // try to fit ladder to best wall or existing ladder
            {
                drawtop = 0;
                drawbottom = 0;
                flowerfix = 0.95f;
                drawfront = 0;
                drawback = 0;
                drawleft = 0;
                drawright = 0;
                int ladderAtPositionMatchWall = getBestLadderWall(xx, yy, zz, currentChunk);
                if (ladderAtPositionMatchWall < 0)
                {

                    int ladderbelow = getBestLadderInDirection(xx, yy, zz, currentChunk, -1);
                    int ladderabove = getBestLadderInDirection(xx, yy, zz, currentChunk, +1);

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
            RailDirectionFlags rail = d_Data.Rail[tiletype];
            float blockheight = 1;//= data.GetTerrainBlockHeight(tiletype);
            if (rail != RailDirectionFlags.None)
            {
                blockheight = 0.3f;
                /*
                RailPolygons(myelements, myvertices, x, y, z, rail);
                return;
                */
            }
            if (d_Data.DrawType1[tt] == DrawType.HalfHeight)
            {
                blockheight = 0.5f;
            }
            if (d_Data.DrawType1[tt] == DrawType.Torch)
            {
                TorchType type = TorchType.Normal;
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Front; }
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Back; }
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Left; }
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Right; }
                List<ushort> torchelements = new List<ushort>();
                List<VertexPositionTexture> torchvertices = new List<VertexPositionTexture>();
                d_BlockRendererTorch.SideTexture = d_Data.TextureId[tt, (int)TileSide.Front];
                d_BlockRendererTorch.TopTexture = d_Data.TextureId[tt, (int)TileSide.Top];
                d_BlockRendererTorch.AddTorch(torchelements, torchvertices, x, y, z, type);
                int oldverticescount = toreturnmain.verticesCount;
                for (int i = 0; i < torchelements.Count; i++)
                {
                    toreturnmain.indices[toreturnmain.indicesCount++] = (ushort)(torchelements[i] + oldverticescount);
                }
                for (int i = 0; i < torchvertices.Count; i++)
                {
                    toreturnmain.vertices[toreturnmain.verticesCount++] = torchvertices[i];
                }
                return;
            }
            //slope
            float blockheight00 = blockheight;
            float blockheight01 = blockheight;
            float blockheight10 = blockheight;
            float blockheight11 = blockheight;
            if (rail != RailDirectionFlags.None)
            {
                if (railmaputil == null)
                {
                    railmaputil = new RailMapUtil() { d_Data = d_Data, d_MapStorage = d_MapStorage };
                }
                RailSlope slope = railmaputil.GetRailSlope(x, y, z);
                if (slope == RailSlope.TwoRightRaised)
                {
                    blockheight10 += 1;
                    blockheight11 += 1;
                }
                if (slope == RailSlope.TwoLeftRaised)
                {
                    blockheight00 += 1;
                    blockheight01 += 1;
                }
                if (slope == RailSlope.TwoUpRaised)
                {
                    blockheight00 += 1;
                    blockheight10 += 1;
                }
                if (slope == RailSlope.TwoDownRaised)
                {
                    blockheight01 += 1;
                    blockheight11 += 1;
                }
            }
            /*
            if (tt >= PartialWaterBlock && tt < PartialWaterBlock + waterLevelsCount)
            {
                int waterlevel = tt - PartialWaterBlock;

                int[] wl = new int[9];
                wl[0] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz, chunksize + 2, chunksize + 2)]);
                wl[1] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy - 1, zz, chunksize + 2, chunksize + 2)]);
                wl[2] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz, chunksize + 2, chunksize + 2)]);
                wl[3] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy + 0, zz, chunksize + 2, chunksize + 2)]);
                wl[4] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy + 0, zz, chunksize + 2, chunksize + 2)]);
                wl[5] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy + 0, zz, chunksize + 2, chunksize + 2)]);
                wl[6] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz, chunksize + 2, chunksize + 2)]);
                wl[7] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy + 1, zz, chunksize + 2, chunksize + 2)]);
                wl[8] = GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz, chunksize + 2, chunksize + 2)]);
                if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[0] = waterLevelsCount - 1; }
                if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy - 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[1] = waterLevelsCount - 1; }
                if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[2] = waterLevelsCount - 1; }
                if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy + 0, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[3] = waterLevelsCount - 1; }
                if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy + 0, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[4] = waterLevelsCount - 1; }
                if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy + 0, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[5] = waterLevelsCount - 1; }
                if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[6] = waterLevelsCount - 1; }
                if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 0, yy + 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[7] = waterLevelsCount - 1; }
                if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)]) >= 0) { wl[8] = waterLevelsCount - 1; }

                //00: maximum of (-1,-1), (0,-1), (-1,0)
                blockheight00 = ((float)Max(waterlevel, wl[0], wl[1], wl[3]) + 1) / waterLevelsCount;
                blockheight01 = ((float)Max(waterlevel, wl[3], wl[6], wl[7]) + 1) / waterLevelsCount;
                blockheight10 = ((float)Max(waterlevel, wl[1], wl[2], wl[5]) + 1) / waterLevelsCount;
                blockheight11 = ((float)Max(waterlevel, wl[5], wl[7], wl[8]) + 1) / waterLevelsCount;

                if (GetWaterLevel(currentChunk[MapUtil.Index3d(xx, yy, zz + 1, chunksize + 2, chunksize + 2)]) > 0)
                {
                    blockheight00 = 1;
                    blockheight01 = 1;
                    blockheight10 = 1;
                    blockheight11 = 1;
                }
            }
            */
            FastColor curcolor = color;
            texrecLeft = 0;//0
            texrecHeight = (float) terrainTexturesPerAtlasInverse * AtiArtifactFix;
            //top
            if (drawtop > 0)
            {
                curcolor = color;
                int shadowratio = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                if (shadowratio != maxlight)
                {
                    float shadowratiof = d_Data.LightLevels[shadowratio];
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratiof),
                        (int)(color.G * shadowratiof),
                        (int)(color.B * shadowratiof));
                }
                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Top];
                int tilecount = drawtop;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = (tilecount * AtiArtifactFix);
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0.0f, z + blockheight00, y + 0.0f, texrecLeft, texrecTop, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0.0f, z + blockheight01, y + 1.0f, texrecLeft, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1.0f * tilecount, z + blockheight10, y + 0.0f, texrecRight, texrecTop, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1.0f * tilecount, z + blockheight11, y + 1.0f, texrecRight, texrecBottom, curcolor));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
            }
            //bottom - same as top, but z is 1 less.
            if (drawbottom > 0)
            {
                curcolor = colorShadowSide;
                int shadowratio = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
                if (shadowratio != maxlight)
                {
                    float shadowratiof = d_Data.LightLevels[shadowratio];
                    curcolor = new FastColor(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratiof)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratiof)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratiof)));
                }
                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Bottom];
                int tilecount = drawbottom;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = (tilecount * AtiArtifactFix);
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0.0f, z, y + 0.0f, texrecLeft, texrecTop, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0.0f, z, y + 1.0f, texrecLeft, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1.0f * tilecount, z, y + 0.0f, texrecRight, texrecTop, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1.0f * tilecount, z, y + 1.0f, texrecRight, texrecBottom, curcolor));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
            }
            //front
            if (drawfront > 0)
            {
                curcolor = color;
                int shadowratio = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
                if (shadowratio != maxlight)
                {
                    float shadowratiof = d_Data.LightLevels[shadowratio];
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratiof),
                        (int)(color.G * shadowratiof),
                        (int)(color.B * shadowratiof));
                }
                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Front];
                int tilecount = drawfront;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = (tilecount * AtiArtifactFix);
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 0, texrecLeft, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 1 * tilecount, texrecRight, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0 + flowerfix, z + blockheight00, y + 0, texrecLeft, texrecTop, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0 + flowerfix, z + blockheight01, y + 1 * tilecount, texrecRight, texrecTop, curcolor));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
            }
            //back - same as front, but x is 1 greater.
            if (drawback > 0)
            {
                curcolor = color;
                int shadowratio = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
                if (shadowratio != maxlight)
                {
                    float shadowratiof = d_Data.LightLevels[shadowratio];
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratiof),
                        (int)(color.G * shadowratiof),
                        (int)(color.B * shadowratiof));
                }
                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Back];
                int tilecount = drawback;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = (tilecount * AtiArtifactFix);
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 0, texrecRight, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 1 * tilecount, texrecLeft, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 - flowerfix, z + blockheight10, y + 0, texrecRight, texrecTop, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 - flowerfix, z + blockheight11, y + 1 * tilecount, texrecLeft, texrecTop, curcolor));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
            }
            if (drawleft > 0)
            {
                curcolor = colorShadowSide;
                int shadowratio = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
                if (shadowratio != maxlight)
                {
                    float shadowratiof = d_Data.LightLevels[shadowratio];
                    curcolor = new FastColor(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratiof)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratiof)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratiof)));
                }

                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Left];
                int tilecount = drawleft;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = (tilecount * AtiArtifactFix); //tilingcount*fix
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0, z + 0, y + 0 + flowerfix, texrecRight, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0, z + blockheight00, y + 0 + flowerfix, texrecRight, texrecTop, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 * tilecount, z + 0, y + 0 + flowerfix, texrecLeft, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 * tilecount, z + blockheight10, y + 0 + flowerfix, texrecLeft, texrecTop, curcolor));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
            }
            //right - same as left, but y is 1 greater.
            if (drawright > 0)
            {
                curcolor = colorShadowSide;
                int shadowratio = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
                if (shadowratio != maxlight)
                {
                    float shadowratiof = d_Data.LightLevels[shadowratio];
                    curcolor = new FastColor(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratiof)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratiof)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratiof)));
                }

                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Right];
                int tilecount = drawright;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = (tilecount * AtiArtifactFix); //tilingcount*fix
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0, z + 0, y + 1 - flowerfix, texrecLeft, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0, z + blockheight01, y + 1 - flowerfix, texrecLeft, texrecTop, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 * tilecount, z + 0, y + 1 - flowerfix, texrecRight, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 * tilecount, z + blockheight11, y + 1 - flowerfix, texrecRight, texrecTop, curcolor));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));
                toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));
            }
        }
        private void SmoothLightBlockPolygons(int x, int y, int z, ushort[] currentChunk)
        {
            int xx = x % chunksize + 1;
            int yy = y % chunksize + 1;
            int zz = z % chunksize + 1;
            var tt = currentChunk[MapUtil.Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];
            if (!isvalid[tt])
            {
                return;
            }
            byte drawtop = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Top];
            byte drawbottom = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Bottom];
            byte drawfront = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Front];
            byte drawback = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Back];
            byte drawleft = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Left];
            byte drawright = currentChunkDrawCount[xx - 1, yy - 1, zz - 1, (int)TileSide.Right];
            int tiletype = tt;
            if (drawtop == 0 && drawbottom == 0 && drawfront == 0 && drawback == 0 && drawleft == 0 && drawright == 0)
            {
                return;
            }
            FastColor color = ColorWhite; //mapstorage.GetTerrainBlockColor(x, y, z);
            FastColor colorShadowSide = new FastColor(color.A,
                (int)(color.R * BlockShadow),
                (int)(color.G * BlockShadow),
                (int)(color.B * BlockShadow));
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
            if (d_Data.IsFlower[tiletype])
            {
                drawtop = 0;
                drawbottom = 0;
                flowerfix = 0.5f;
            }
            if (d_Data.DrawType1[tiletype] == DrawType.OpenDoorLeft)
            {
                drawtop = 0;
                drawbottom = 0;
                flowerfix = 0.9f;
                //x-1, x+1
                if (currentChunk[MapUtil.Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0
                    && currentChunk[MapUtil.Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
                {
                    drawback = 0;
                    drawfront = 0;
                    drawleft = 1;
                    drawright = 0;
                }
                //y-1, y+1
                if (currentChunk[MapUtil.Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0
                    && currentChunk[MapUtil.Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
                {
                    drawback = 1;
                    drawfront = 0;
                    drawleft = 0;
                    drawright = 0;
                }
            }
            if (d_Data.DrawType1[tiletype] == DrawType.OpenDoorRight)
            {
                drawtop = 0;
                drawbottom = 0;
                flowerfix = 0.9f;
                //x-1, x+1
                if (currentChunk[MapUtil.Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] == 0
                    && currentChunk[MapUtil.Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] == 0)
                {
                    drawback = 0;
                    drawfront = 0;
                    drawleft = 0;
                    drawright = 1;
                }
                //y-1, y+1
                if (currentChunk[MapUtil.Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] == 0
                    && currentChunk[MapUtil.Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] == 0)
                {
                    drawback = 0;
                    drawfront = 1;
                    drawleft = 0;
                    drawright = 0;
                }
            }
            if (d_Data.DrawType1[tiletype] == DrawType.Fence || d_Data.DrawType1[tiletype] == DrawType.ClosedDoor) // fence tiles automatically when another fence is beside
            {
                drawtop = 0;
                drawbottom = 0;
                drawfront = 0;
                drawback = 0;
                drawleft = 0;
                drawright = 0;
                flowerfix = 0.5f;

                //x-1, x+1
                if (currentChunk[MapUtil.Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)] != d_Data.BlockIdEmpty
                    || currentChunk[MapUtil.Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)] != d_Data.BlockIdEmpty)
                {
                    drawleft = 1;
                }
                //y-1, y+1
                if (currentChunk[MapUtil.Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)] != d_Data.BlockIdEmpty
                    || currentChunk[MapUtil.Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)] != d_Data.BlockIdEmpty)
                {
                    drawfront = 1;
                }
                if (drawback == 0 && drawfront == 0 && drawleft == 0 && drawright == 0)
                {
                    drawback = 1;
                    drawleft = 1;
                }
            }
            if (d_Data.DrawType1[tiletype] == DrawType.Ladder) // try to fit ladder to best wall or existing ladder
            {
                drawtop = 0;
                drawbottom = 0;
                flowerfix = 0.95f;
                drawfront = 0;
                drawback = 0;
                drawleft = 0;
                drawright = 0;
                int ladderAtPositionMatchWall = getBestLadderWall(xx, yy, zz, currentChunk);
                if (ladderAtPositionMatchWall < 0)
                {

                    int ladderbelow = getBestLadderInDirection(xx, yy, zz, currentChunk, -1);
                    int ladderabove = getBestLadderInDirection(xx, yy, zz, currentChunk, +1);

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
            RailDirectionFlags rail = d_Data.Rail[tiletype];
            float blockheight = 1;//= data.GetTerrainBlockHeight(tiletype);
            if (rail != RailDirectionFlags.None)
            {
                blockheight = 0.3f;
                /*
                RailPolygons(myelements, myvertices, x, y, z, rail);
                return;
                */
            }
            if (d_Data.DrawType1[tt] == DrawType.HalfHeight)
            {
                blockheight = 0.5f;
            }
            if (d_Data.DrawType1[tt] == DrawType.Torch)
            {
                TorchType type = TorchType.Normal;
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Front; }
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Back; }
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Left; }
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Right; }
                List<ushort> torchelements = new List<ushort>();
                List<VertexPositionTexture> torchvertices = new List<VertexPositionTexture>();
                d_BlockRendererTorch.SideTexture = d_Data.TextureId[tt, (int)TileSide.Front];
                d_BlockRendererTorch.TopTexture = d_Data.TextureId[tt, (int)TileSide.Top];
                d_BlockRendererTorch.AddTorch(torchelements, torchvertices, x, y, z, type);
                int oldverticescount=toreturnmain.verticesCount;
                for (int i = 0; i < torchelements.Count; i++)
                {
                    toreturnmain.indices[toreturnmain.indicesCount++] = (ushort)(torchelements[i] + oldverticescount);
                }
                for (int i = 0; i < torchvertices.Count; i++)
                {
                    toreturnmain.vertices[toreturnmain.verticesCount++] = torchvertices[i];
                }
                return;
            }
            //slope
            float blockheight00 = blockheight;
            float blockheight01 = blockheight;
            float blockheight10 = blockheight;
            float blockheight11 = blockheight;
            if (rail != RailDirectionFlags.None)
            {
                if (railmaputil == null)
                {
                    railmaputil = new RailMapUtil() { d_Data = d_Data, d_MapStorage = d_MapStorage };
                }
                RailSlope slope = railmaputil.GetRailSlope(x, y, z);
                if (slope == RailSlope.TwoRightRaised)
                {
                    blockheight10 += 1;
                    blockheight11 += 1;
                }
                if (slope == RailSlope.TwoLeftRaised)
                {
                    blockheight00 += 1;
                    blockheight01 += 1;
                }
                if (slope == RailSlope.TwoUpRaised)
                {
                    blockheight00 += 1;
                    blockheight10 += 1;
                }
                if (slope == RailSlope.TwoDownRaised)
                {
                    blockheight01 += 1;
                    blockheight11 += 1;
                }
            }
            //if stationary water block, make slightly lower than terrain
            if (tt == 8)
            {
                blockheight00 = 0.9f;
                blockheight01 = 0.9f;
                blockheight10 = 0.9f;
                blockheight11 = 0.9f;
            }
            FastColor curcolor = color;
            FastColor curcolor2 = color;
            FastColor curcolor3 = color;
            FastColor curcolor4 = color;
            texrecLeft = 0;
            texrecHeight = (float)terrainTexturesPerAtlasInverse * AtiArtifactFix;
            //top
            if (drawtop > 0)
            {
                bool occluded = false;
                bool occdirnorthwest = true;
                //bool applysmoothing = true;
                int shadowratio = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                //if (true)
                {
                    int top = currentChunk[MapUtil.Index3d(xx, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottom = currentChunk[MapUtil.Index3d(xx, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int left = currentChunk[MapUtil.Index3d(xx - 1, yy, zz + 1, chunksize + 2, chunksize + 2)];
                    int right = currentChunk[MapUtil.Index3d(xx + 1, yy, zz + 1, chunksize + 2, chunksize + 2)];
                    int topleft = currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int topright = currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottomleft = currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottomright = currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
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
                    if (top != 0) { if (!d_Data.IsTransparentForLight[top]) { topoccupied = true; } else { topoccupied = false; } }
                    else { topoccupied = false; shadowratio5 = GetShadowRatio(xx, yy - 1, zz + 1, x, y - 1, z + 1); }
                    if (topleft != 0) { if (!d_Data.IsTransparentForLight[topleft]) { topleftoccupied = true; } else { topleftoccupied = false; } }
                    else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx - 1, yy - 1, zz + 1, x - 1, y, z + 1); }
                    if (topright != 0) { if (!d_Data.IsTransparentForLight[topright]) { toprightoccupied = true; } else { toprightoccupied = false; } }
                    else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx + 1, yy - 1, zz + 1, x - 1, y, z + 1); }
                    if (left != 0) { if (!d_Data.IsTransparentForLight[left]) { leftoccupied = true; } else { leftoccupied = false; } }
                    else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx - 1, yy, zz + 1, x - 1, y, z + 1); }
                    if (right != 0) { if (!d_Data.IsTransparentForLight[right]) { rightoccupied = true; } else { rightoccupied = false; } }
                    else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx + 1, yy, zz + 1, x + 1, y, z + 1); }
                    if (bottom != 0) { if (!d_Data.IsTransparentForLight[bottom]) { bottomoccupied = true; } else { bottomoccupied = false; } }
                    else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx, yy + 1, zz + 1, x, y + 1, z + 1); }
                    if (bottomright != 0) { if (!d_Data.IsTransparentForLight[bottomright]) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                    else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx + 1, yy + 1, zz + 1, x - 1, y, z + 1); }
                    if (bottomleft != 0) { if (!d_Data.IsTransparentForLight[bottomleft]) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                    else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx - 1, yy + 1, zz + 1, x - 1, y, z + 1); }
                    
                    
                    float shadowratiomain = d_Data.LightLevels[shadowratio];
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
                        goto done;
                    }
                    //topleft vertex
                    if (leftoccupied && topoccupied) { goto toprightvertex; }
                   byte facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio5]; }
                    if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio7]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof4 /= facesconsidered;
                toprightvertex:
                        //topright vertex
                    if (topoccupied && rightoccupied) { goto bottomrightvertex; }
                    facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio5]; }
                    if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio6]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof5 /= facesconsidered;
                bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { goto bottomleftvertex; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio8]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof3 /= facesconsidered;
                bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { goto done; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio9]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof2 /= facesconsidered;
                done:
                    //ambient occlusion, corners with 2 blocks get full occlusion, others half
                    if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; goto next; }
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true;  occdirnorthwest = false; shadowratiof5 *= occ; }
                next:
                    if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; goto next1; }
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                next1:
                    if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; goto next2; }
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                next2:
                    if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; goto next3; }
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                next3: 
                        curcolor = new FastColor(color.A,
                            (int)(color.R * shadowratiof2),
                            (int)(color.G * shadowratiof2),
                            (int)(color.B * shadowratiof2 * Yellowness));

                        curcolor2 = new FastColor(color.A,
                            (int)(color.R * shadowratiof3),
                            (int)(color.G * shadowratiof3),
                            (int)(color.B * shadowratiof3 * Yellowness));

                        curcolor3 = new FastColor(color.A,
                            (int)(color.R * shadowratiof4),
                            (int)(color.G * shadowratiof4),
                            (int)(color.B * shadowratiof4 * Yellowness));

                        curcolor4 = new FastColor(color.A,
                            (int)(color.R * shadowratiof5),
                            (int)(color.G * shadowratiof5),
                            (int)(color.B * shadowratiof5 * Yellowness));
                    }
                    int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Top];
                    int tilecount = drawtop;
                    VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                    texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                    texrecWidth = AtiArtifactFix; //tilingcount*fix
                    float texrecBottom = texrecTop + texrecHeight;
                    float texrecRight = texrecLeft + texrecWidth;
                    short lastelement = (short)toreturn.verticesCount;
                    toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0.0f, z + blockheight00, y + 0.0f, texrecLeft, texrecTop, curcolor3));//leftbottom 4
                    toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0.0f, z + blockheight01, y + 1.0f, texrecLeft, texrecBottom, curcolor));//rightbottom 2
                    toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1.0f * tilecount, z + blockheight10, y + 0.0f, texrecRight, texrecTop, curcolor4));//topleft  3
                    toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1.0f * tilecount, z + blockheight11, y + 1.0f, texrecRight, texrecBottom, curcolor2));//topright  * tilecount

                    //revert triangles to fix gradient problem
                    //if occluded, revert to proper occlusion direction

                    if (occluded)
                    {
                        if (!occdirnorthwest)
                        {
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//3
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//2
                        }
                        else
                        {
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//3
                            toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                        }
                    }

                    else if (curcolor.R != curcolor4.R || curcolor3.R == curcolor2.R)
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//2
                    }
                    else
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
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
                    int top = currentChunk[MapUtil.Index3d(xx, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int bottom = currentChunk[MapUtil.Index3d(xx, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int left = currentChunk[MapUtil.Index3d(xx - 1, yy, zz - 1, chunksize + 2, chunksize + 2)];
                    int right = currentChunk[MapUtil.Index3d(xx + 1, yy, zz - 1, chunksize + 2, chunksize + 2)];
                    int topleft = currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int topright = currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int bottomleft = currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int bottomright = currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
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
                
                    if (top != 0) { if (!d_Data.IsTransparentForLight[top]) { topoccupied = true; } else { topoccupied = false; } }
                    else { topoccupied = false; shadowratio5 = GetShadowRatio(xx, yy + 1, zz - 1, x, y - 1, z - 1); }
                    if (topleft != 0) { if (!d_Data.IsTransparentForLight[topleft]) { topleftoccupied = true; } else { topleftoccupied = false; } }
                    else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx - 1, yy + 1, zz - 1, x - 1, y, z - 1); }
                    if (topright != 0) { if (!d_Data.IsTransparentForLight[topright]) { toprightoccupied = true; } else { toprightoccupied = false; } }
                    else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx + 1, yy + 1, zz - 1, x - 1, y, z - 1); }
                    if (left != 0) { if (!d_Data.IsTransparentForLight[left]) { leftoccupied = true; } else { leftoccupied = false; } }
                    else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx - 1, yy, zz - 1, x - 1, y, z - 1); }
                    if (right != 0) { if (!d_Data.IsTransparentForLight[right]) { rightoccupied = true; } else { rightoccupied = false; } }
                    else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx + 1, yy, zz - 1, x + 1, y, z - 1); }
                    if (bottom != 0) { if (!d_Data.IsTransparentForLight[bottom]) { bottomoccupied = true; } else { bottomoccupied = false; } }
                    else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx, yy - 1, zz - 1, x, y + 1, z - 1); }
                    if (bottomright != 0) { if (!d_Data.IsTransparentForLight[bottomright]) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                    else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx + 1, yy - 1, zz - 1, x - 1, y, z - 1); }
                    if (bottomleft != 0) { if (!d_Data.IsTransparentForLight[bottomleft]) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                    else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx - 1, yy - 1, zz - 1, x - 1, y, z - 1); }


                    float shadowratiomain = d_Data.LightLevels[shadowratio];
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
                        goto done;
                    }
                    //topleft vertex
                    if (leftoccupied && topoccupied) { goto toprightvertex; }
                    byte facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio5]; }
                    if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio7]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof4 /= facesconsidered;
                toprightvertex:
                    //topright vertex
                    if (topoccupied && rightoccupied) { goto bottomrightvertex; }
                    facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio5]; }
                    if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio6]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof5 /= facesconsidered;
                bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { goto bottomleftvertex; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio8]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof3 /= facesconsidered;
                bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { goto done; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio9]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof2 /= facesconsidered;
                done:
                    //ambient occlusion, corners with 2 blocks get full occlusion, others half
                    if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; goto next; }
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                next:
                    if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; goto next1; }
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                next1:
                    if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; goto next2; }
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                next2:
                    if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; goto next3; }
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                next3:
                    curcolor = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof2),
                        (int)(colorShadowSide.G * shadowratiof2),
                        (int)(colorShadowSide.B * shadowratiof2 * Yellowness));

                    curcolor2 = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof3),
                        (int)(colorShadowSide.G * shadowratiof3),
                        (int)(colorShadowSide.B * shadowratiof3 * Yellowness));

                    curcolor3 = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof4),
                        (int)(colorShadowSide.G * shadowratiof4),
                        (int)(colorShadowSide.B * shadowratiof4 * Yellowness));

                    curcolor4 = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof5),
                        (int)(colorShadowSide.G * shadowratiof5),
                        (int)(colorShadowSide.B * shadowratiof5 * Yellowness));
                }
                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Bottom];
                int tilecount = drawbottom;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = AtiArtifactFix; //tilingcount*fix
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0.0f, z, y + 0.0f, texrecLeft, texrecTop, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0.0f, z, y + 1.0f, texrecLeft, texrecBottom, curcolor3));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1.0f * tilecount, z, y + 0.0f, texrecRight, texrecTop, curcolor2));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1.0f * tilecount, z, y + 1.0f, texrecRight, texrecBottom, curcolor4));

                //revert triangles to fix gradient problem
                //if occluded, revert to proper occlusion direction
                
                if (occluded)
                {
                    if (occdirnorthwest)
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//2
                    }
                    else
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    }
                }

                else if (curcolor.R != curcolor4.R || curcolor3.R == curcolor2.R)
                {
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//3
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                }
                else
                {
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//3
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//2
                }
                
            }
            //front
            if (drawfront > 0)
            {
                bool occluded = false;
                bool occdirnorthwest = true;
                //bool applysmoothing = true;
                int shadowratio = GetShadowRatio(xx-1, yy, zz, x-1, y, z);
                //if (true)
                {
                    int top = currentChunk[MapUtil.Index3d(xx - 1, yy, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottom = currentChunk[MapUtil.Index3d(xx - 1, yy, zz - 1, chunksize + 2, chunksize + 2)];
                    int left = currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz, chunksize + 2, chunksize + 2)];
                    int right = currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz, chunksize + 2, chunksize + 2)];
                    int topleft = currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int topright = currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottomleft = currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int bottomright = currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
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
                    if (top != 0) { if (!d_Data.IsTransparentForLight[top]) { topoccupied = true; } else { topoccupied = false; } }
                    else { topoccupied = false; shadowratio5 = GetShadowRatio(xx - 1, yy, zz + 1, x - 1, y, z + 1); }
                    if (topleft != 0) { if (!d_Data.IsTransparentForLight[topleft]) { topleftoccupied = true; } else { topleftoccupied = false; } }
                    else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx - 1, yy - 1, zz + 1, x - 1, y - 1, z + 1); }
                    if (topright != 0) { if (!d_Data.IsTransparentForLight[topright]) { toprightoccupied = true; } else { toprightoccupied = false; } }
                    else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx - 1, yy + 1, zz + 1, x - 1, y+1, z + 1); }
                    if (left != 0) { if (!d_Data.IsTransparentForLight[left]) { leftoccupied = true; } else { leftoccupied = false; } }
                    else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx - 1, yy - 1, zz, x - 1, y-1, z); }
                    if (right != 0) { if (!d_Data.IsTransparentForLight[right]) { rightoccupied = true; } else { rightoccupied = false; } }
                    else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx - 1, yy + 1, zz, x - 1, y+1, z); }
                    if (bottom != 0) { if (!d_Data.IsTransparentForLight[bottom]) { bottomoccupied = true; } else { bottomoccupied = false; } }
                    else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx - 1, yy, zz - 1, x-1, y, z - 1); }
                    if (bottomright != 0) { if (!d_Data.IsTransparentForLight[bottomright]) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                    else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx - 1, yy + 1, zz - 1, x - 1, y + 1, z - 1); }
                    if (bottomleft != 0) { if (!d_Data.IsTransparentForLight[bottomleft]) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                    else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx - 1, yy - 1, zz - 1, x - 1, y - 1, z - 1); }


                    float shadowratiomain = d_Data.LightLevels[shadowratio];
                    float shadowratiof5 = shadowratiomain;
                    float shadowratiof4 = shadowratiomain;
                    float shadowratiof3 = shadowratiomain;
                    float shadowratiof2 = shadowratiomain;

                    if (shadowratio9 == shadowratio8 && shadowratio8 == shadowratio7 && shadowratio7 == shadowratio6 &&
                        shadowratio6 == shadowratio5 && shadowratio5 == shadowratio4 && shadowratio4 == shadowratio3 &&
                        shadowratio3 == shadowratio2 && shadowratio2 == shadowratio)
                    {
                        //no shadow tiles near, just do occlusion
                        goto done;
                    }
                    //topleft vertex
                    if (leftoccupied && topoccupied) { goto toprightvertex; }
                    byte facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio5]; }
                    if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio7]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof4 /= facesconsidered;
                toprightvertex:
                    //topright vertex
                    if (topoccupied && rightoccupied) { goto bottomrightvertex; }
                    facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio5]; }
                    if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio6]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof5 /= facesconsidered;
                bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { goto bottomleftvertex; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio8]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof3 /= facesconsidered;
                bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { goto done; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio9]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof2 /= facesconsidered;
                done:
                    //ambient occlusion, corners with 2 blocks get full occlusion, others half
                    if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; goto next; }
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                next:
                    if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; goto next1; }
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                next1:
                    if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; goto next2; }
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                next2:
                    if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; goto next3; }
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                next3:
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratiof2),
                        (int)(color.G * shadowratiof2),
                        (int)(color.B * shadowratiof2 * Yellowness));

                    curcolor2 = new FastColor(color.A,
                        (int)(color.R * shadowratiof3),
                        (int)(color.G * shadowratiof3),
                        (int)(color.B * shadowratiof3 * Yellowness));

                    curcolor3 = new FastColor(color.A,
                        (int)(color.R * shadowratiof4),
                        (int)(color.G * shadowratiof4),
                        (int)(color.B * shadowratiof4 * Yellowness));

                    curcolor4 = new FastColor(color.A,
                        (int)(color.R * shadowratiof5),
                        (int)(color.G * shadowratiof5),
                        (int)(color.B * shadowratiof5 * Yellowness));
                }
                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Front];
                int tilecount = drawfront;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = AtiArtifactFix; //tilingcount*fix
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 0, texrecLeft, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 1 * tilecount, texrecRight, texrecBottom, curcolor2));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0 + flowerfix, z + blockheight00, y + 0, texrecLeft, texrecTop, curcolor3));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0 + flowerfix, z + blockheight01, y + 1 * tilecount, texrecRight, texrecTop, curcolor4));
                if (occluded)
                {
                    if (!occdirnorthwest)
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    }
                    else
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//2
                    }
                }

                else if (curcolor.R != curcolor4.R || curcolor3.R == curcolor2.R)
                {
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//3
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                }
                else
                {
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//3
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//2
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
                    int top = currentChunk[MapUtil.Index3d(xx + 1, yy, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottom = currentChunk[MapUtil.Index3d(xx + 1, yy, zz - 1, chunksize + 2, chunksize + 2)];
                    int left = currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz, chunksize + 2, chunksize + 2)];
                    int right = currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz, chunksize + 2, chunksize + 2)];
                    int topleft = currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int topright = currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottomleft = currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int bottomright = currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
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
                    if (top != 0) { if (!d_Data.IsTransparentForLight[top]) { topoccupied = true; } else { topoccupied = false; } }
                    else { topoccupied = false; shadowratio5 = GetShadowRatio(xx + 1, yy, zz + 1, x - 1, y, z + 1); }
                    if (topleft != 0) { if (!d_Data.IsTransparentForLight[topleft]) { topleftoccupied = true; } else { topleftoccupied = false; } }
                    else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx + 1, yy - 1, zz + 1, x - 1, y - 1, z + 1); }
                    if (topright != 0) { if (!d_Data.IsTransparentForLight[topright]) { toprightoccupied = true; } else { toprightoccupied = false; } }
                    else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx + 1, yy + 1, zz + 1, x - 1, y + 1, z + 1); }
                    if (left != 0) { if (!d_Data.IsTransparentForLight[left]) { leftoccupied = true; } else { leftoccupied = false; } }
                    else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx + 1, yy - 1, zz, x - 1, y - 1, z); }
                    if (right != 0) { if (!d_Data.IsTransparentForLight[right]) { rightoccupied = true; } else { rightoccupied = false; } }
                    else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx + 1, yy + 1, zz, x - 1, y + 1, z); }
                    if (bottom != 0) { if (!d_Data.IsTransparentForLight[bottom]) { bottomoccupied = true; } else { bottomoccupied = false; } }
                    else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx + 1, yy, zz - 1, x - 1, y, z - 1); }
                    if (bottomright != 0) { if (!d_Data.IsTransparentForLight[bottomright]) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                    else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx + 1, yy + 1, zz - 1, x - 1, y + 1, z - 1); }
                    if (bottomleft != 0) { if (!d_Data.IsTransparentForLight[bottomleft]) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                    else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx + 1, yy - 1, zz - 1, x - 1, y - 1, z - 1); }


                    float shadowratiomain = d_Data.LightLevels[shadowratio];
                    float shadowratiof5 = shadowratiomain;
                    float shadowratiof4 = shadowratiomain;
                    float shadowratiof3 = shadowratiomain;
                    float shadowratiof2 = shadowratiomain;

                    if (shadowratio9 == shadowratio8 && shadowratio8 == shadowratio7 && shadowratio7 == shadowratio6 &&
                        shadowratio6 == shadowratio5 && shadowratio5 == shadowratio4 && shadowratio4 == shadowratio3 &&
                        shadowratio3 == shadowratio2 && shadowratio2 == shadowratio)
                    {
                        //no shadow tiles near, just do occlusion
                        goto done;
                    }
                    //topleft vertex
                    if (leftoccupied && topoccupied) { goto toprightvertex; }
                    byte facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio5]; }
                    if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio7]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof4 /= facesconsidered;
                toprightvertex:
                    //topright vertex
                    if (topoccupied && rightoccupied) { goto bottomrightvertex; }
                    facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio5]; }
                    if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio6]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof5 /= facesconsidered;
                bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { goto bottomleftvertex; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio8]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof3 /= facesconsidered;
                bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { goto done; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio9]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof2 /= facesconsidered;
                done:
                    //ambient occlusion, corners with 2 blocks get full occlusion, others half
                    if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; goto next; }
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                next:
                    if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; goto next1; }
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                next1:
                    if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; goto next2; }
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                next2:
                    if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; goto next3; }
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                next3:
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratiof2),
                        (int)(color.G * shadowratiof2),
                        (int)(color.B * shadowratiof2 * Yellowness));

                    curcolor2 = new FastColor(color.A,
                        (int)(color.R * shadowratiof3),
                        (int)(color.G * shadowratiof3),
                        (int)(color.B * shadowratiof3 * Yellowness));

                    curcolor3 = new FastColor(color.A,
                        (int)(color.R * shadowratiof4),
                        (int)(color.G * shadowratiof4),
                        (int)(color.B * shadowratiof4 * Yellowness));

                    curcolor4 = new FastColor(color.A,
                        (int)(color.R * shadowratiof5),
                        (int)(color.G * shadowratiof5),
                        (int)(color.B * shadowratiof5 * Yellowness));
                }
                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Back];
                int tilecount = drawback;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = AtiArtifactFix; //tilingcount*fix
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 0, texrecRight, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 1 * tilecount, texrecLeft, texrecBottom, curcolor2));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 - flowerfix, z + blockheight10, y + 0, texrecRight, texrecTop, curcolor3));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 - flowerfix, z + blockheight11, y + 1 * tilecount, texrecLeft, texrecTop, curcolor4));
                if (occluded)
                {
                    if (!occdirnorthwest)
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    }
                    else
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//2
                    }
                }

                else if (curcolor.R != curcolor4.R || curcolor3.R == curcolor2.R)
                {
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//3
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                }
                else
                {
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//3
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//2
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
                    int top = currentChunk[MapUtil.Index3d(xx, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottom = currentChunk[MapUtil.Index3d(xx, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int left = currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz, chunksize + 2, chunksize + 2)];
                    int right = currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz, chunksize + 2, chunksize + 2)];
                    int topleft = currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int topright = currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottomleft = currentChunk[MapUtil.Index3d(xx + 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int bottomright = currentChunk[MapUtil.Index3d(xx - 1, yy - 1, zz - 1, chunksize + 2, chunksize + 2)];
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
                    if (top != 0) { if (!d_Data.IsTransparentForLight[top]) { topoccupied = true; } else { topoccupied = false; } }
                    else { topoccupied = false; shadowratio5 = GetShadowRatio(xx, yy - 1, zz + 1, x - 1, y, z + 1); }
                    if (topleft != 0) { if (!d_Data.IsTransparentForLight[topleft]) { topleftoccupied = true; } else { topleftoccupied = false; } }
                    else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx + 1, yy - 1, zz + 1, x - 1, y - 1, z + 1); }
                    if (topright != 0) { if (!d_Data.IsTransparentForLight[topright]) { toprightoccupied = true; } else { toprightoccupied = false; } }
                    else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx - 1, yy - 1, zz + 1, x - 1, y + 1, z + 1); }
                    if (left != 0) { if (!d_Data.IsTransparentForLight[left]) { leftoccupied = true; } else { leftoccupied = false; } }
                    else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx + 1, yy - 1, zz, x - 1, y - 1, z); }
                    if (right != 0) { if (!d_Data.IsTransparentForLight[right]) { rightoccupied = true; } else { rightoccupied = false; } }
                    else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx - 1, yy - 1, zz, x - 1, y + 1, z); }
                    if (bottom != 0) { if (!d_Data.IsTransparentForLight[bottom]) { bottomoccupied = true; } else { bottomoccupied = false; } }
                    else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx, yy - 1, zz - 1, x - 1, y, z - 1); }
                    if (bottomright != 0) { if (!d_Data.IsTransparentForLight[bottomright]) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                    else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx - 1, yy - 1, zz - 1, x - 1, y + 1, z - 1); }
                    if (bottomleft != 0) { if (!d_Data.IsTransparentForLight[bottomleft]) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                    else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx + 1, yy - 1, zz - 1, x - 1, y - 1, z - 1); }


                    float shadowratiomain = d_Data.LightLevels[shadowratio];
                    float shadowratiof5 = shadowratiomain;
                    float shadowratiof4 = shadowratiomain;
                    float shadowratiof3 = shadowratiomain;
                    float shadowratiof2 = shadowratiomain;

                    if (shadowratio9 == shadowratio8 && shadowratio8 == shadowratio7 && shadowratio7 == shadowratio6 &&
                        shadowratio6 == shadowratio5 && shadowratio5 == shadowratio4 && shadowratio4 == shadowratio3 &&
                        shadowratio3 == shadowratio2 && shadowratio2 == shadowratio)
                    {
                        //no shadow tiles near, just do occlusion
                        goto done;
                    }
                    //topleft vertex
                    if (leftoccupied && topoccupied) { goto toprightvertex; }
                    byte facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio5]; }
                    if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio7]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof4 /= facesconsidered;
                toprightvertex:
                    //topright vertex
                    if (topoccupied && rightoccupied) { goto bottomrightvertex; }
                    facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio5]; }
                    if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio6]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof5 /= facesconsidered;
                bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { goto bottomleftvertex; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio8]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof3 /= facesconsidered;
                bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { goto done; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio9]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof2 /= facesconsidered;
                done:
                    //ambient occlusion, corners with 2 blocks get full occlusion, others half
                    if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; goto next; }
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                next:
                    if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; goto next1; }
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                next1:
                    if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; goto next2; }
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                next2:
                    if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; goto next3; }
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                next3:
                    curcolor = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof2),
                        (int)(colorShadowSide.G * shadowratiof2),
                        (int)(colorShadowSide.B * shadowratiof2 * Yellowness));

                    curcolor2 = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof3),
                        (int)(colorShadowSide.G * shadowratiof3),
                        (int)(colorShadowSide.B * shadowratiof3 * Yellowness));

                    curcolor3 = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof4),
                        (int)(colorShadowSide.G * shadowratiof4),
                        (int)(colorShadowSide.B * shadowratiof4 * Yellowness));

                    curcolor4 = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof5),
                        (int)(colorShadowSide.G * shadowratiof5),
                        (int)(colorShadowSide.B * shadowratiof5 * Yellowness));
                }

                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Left];
                int tilecount = drawleft;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = AtiArtifactFix; //tilingcount*fix
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0, z + 0, y + 0 + flowerfix, texrecRight, texrecBottom, curcolor2));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0, z + blockheight00, y + 0 + flowerfix, texrecRight, texrecTop, curcolor4));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 * tilecount, z + 0, y + 0 + flowerfix, texrecLeft, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 * tilecount, z + blockheight10, y + 0 + flowerfix, texrecLeft, texrecTop, curcolor3));
                if (occluded)
                {
                    if (occdirnorthwest)
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    }
                    else
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    }
                }

                else if (curcolor.R != curcolor4.R || curcolor3.R == curcolor2.R)
                {
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//3
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                }
                else
                {
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//3
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
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
                    int top = currentChunk[MapUtil.Index3d(xx, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottom = currentChunk[MapUtil.Index3d(xx, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int left = currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz, chunksize + 2, chunksize + 2)];
                    int right = currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz, chunksize + 2, chunksize + 2)];
                    int topleft = currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int topright = currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz + 1, chunksize + 2, chunksize + 2)];
                    int bottomleft = currentChunk[MapUtil.Index3d(xx - 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
                    int bottomright = currentChunk[MapUtil.Index3d(xx + 1, yy + 1, zz - 1, chunksize + 2, chunksize + 2)];
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
                    if (top != 0) { if (!d_Data.IsTransparentForLight[top]) { topoccupied = true; } else { topoccupied = false; } }
                    else { topoccupied = false; shadowratio5 = GetShadowRatio(xx, yy + 1, zz + 1, x - 1, y, z + 1); }
                    if (topleft != 0) { if (!d_Data.IsTransparentForLight[topleft]) { topleftoccupied = true; } else { topleftoccupied = false; } }
                    else { topleftoccupied = false; shadowratio7 = GetShadowRatio(xx - 1, yy + 1, zz + 1, x - 1, y - 1, z + 1); }
                    if (topright != 0) { if (!d_Data.IsTransparentForLight[topright]) { toprightoccupied = true; } else { toprightoccupied = false; } }
                    else { toprightoccupied = false; shadowratio6 = GetShadowRatio(xx + 1, yy + 1, zz + 1, x - 1, y + 1, z + 1); }
                    if (left != 0) { if (!d_Data.IsTransparentForLight[left]) { leftoccupied = true; } else { leftoccupied = false; } }
                    else { leftoccupied = false; shadowratio2 = GetShadowRatio(xx - 1, yy + 1, zz, x - 1, y - 1, z); }
                    if (right != 0) { if (!d_Data.IsTransparentForLight[right]) { rightoccupied = true; } else { rightoccupied = false; } }
                    else { rightoccupied = false; shadowratio4 = GetShadowRatio(xx + 1, yy + 1, zz, x - 1, y + 1, z); }
                    if (bottom != 0) { if (!d_Data.IsTransparentForLight[bottom]) { bottomoccupied = true; } else { bottomoccupied = false; } }
                    else { bottomoccupied = false; shadowratio3 = GetShadowRatio(xx, yy + 1, zz - 1, x - 1, y, z - 1); }
                    if (bottomright != 0) { if (!d_Data.IsTransparentForLight[bottomright]) { bottomrightoccupied = true; } else { bottomrightoccupied = false; } }
                    else { bottomrightoccupied = false; shadowratio8 = GetShadowRatio(xx + 1, yy + 1, zz - 1, x - 1, y + 1, z - 1); }
                    if (bottomleft != 0) { if (!d_Data.IsTransparentForLight[bottomleft]) { bottomleftoccupied = true; } else { bottomleftoccupied = false; } }
                    else { bottomleftoccupied = false; shadowratio9 = GetShadowRatio(xx - 1, yy + 1, zz - 1, x - 1, y - 1, z - 1); }


                    float shadowratiomain = d_Data.LightLevels[shadowratio];
                    float shadowratiof5 = shadowratiomain;
                    float shadowratiof4 = shadowratiomain;
                    float shadowratiof3 = shadowratiomain;
                    float shadowratiof2 = shadowratiomain;

                    if (shadowratio9 == shadowratio8 && shadowratio8 == shadowratio7 && shadowratio7 == shadowratio6 &&
                        shadowratio6 == shadowratio5 && shadowratio5 == shadowratio4 && shadowratio4 == shadowratio3 &&
                        shadowratio3 == shadowratio2 && shadowratio2 == shadowratio)
                    {
                        //no shadow tiles near, just do occlusion
                        goto done;
                    }
                    //topleft vertex
                    if (leftoccupied && topoccupied) { goto toprightvertex; }
                    byte facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio5]; }
                    if (topleftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio7]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof4 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof4 /= facesconsidered;
                toprightvertex:
                    //topright vertex
                    if (topoccupied && rightoccupied) { goto bottomrightvertex; }
                    facesconsidered = 4;
                    if (topoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio5]; }
                    if (toprightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio6]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof5 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof5 /= facesconsidered;
                bottomrightvertex:
                    //bottomright vertex
                    if (bottomoccupied && rightoccupied) { goto bottomleftvertex; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomrightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio8]; }
                    if (rightoccupied) { facesconsidered -= 1; } else { shadowratiof3 += d_Data.LightLevels[shadowratio4]; }
                    shadowratiof3 /= facesconsidered;
                bottomleftvertex:
                    //bottomleft
                    if (bottomoccupied && leftoccupied) { goto done; }
                    facesconsidered = 4;
                    if (bottomoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio3]; }
                    if (bottomleftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio9]; }
                    if (leftoccupied) { facesconsidered -= 1; } else { shadowratiof2 += d_Data.LightLevels[shadowratio2]; }
                    shadowratiof2 /= facesconsidered;
                done:
                    //ambient occlusion, corners with 2 blocks get full occlusion, others half
                    if (topoccupied && rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= halfocc; goto next; }
                    if (topoccupied || rightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                    else if (toprightoccupied) { occluded = true; occdirnorthwest = false; shadowratiof5 *= occ; }
                next:
                    if (topoccupied && leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= halfocc; goto next1; }
                    if (topoccupied || leftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                    else if (topleftoccupied) { occluded = true; occdirnorthwest = true; shadowratiof4 *= occ; }
                next1:
                    if (bottomoccupied && rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= halfocc; goto next2; }
                    if (bottomoccupied || rightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                    else if (bottomrightoccupied) { occluded = true; occdirnorthwest = true; shadowratiof3 *= occ; }
                next2:
                    if (bottomoccupied && leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= halfocc; goto next3; }
                    if (bottomoccupied || leftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                    else if (bottomleftoccupied) { occluded = true; occdirnorthwest = false; shadowratiof2 *= occ; }
                next3:
                    curcolor = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof2),
                        (int)(colorShadowSide.G * shadowratiof2),
                        (int)(colorShadowSide.B * shadowratiof2 * Yellowness));

                    curcolor2 = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof3),
                        (int)(colorShadowSide.G * shadowratiof3),
                        (int)(colorShadowSide.B * shadowratiof3 * Yellowness));

                    curcolor3 = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof4),
                        (int)(colorShadowSide.G * shadowratiof4),
                        (int)(colorShadowSide.B * shadowratiof4 * Yellowness));

                    curcolor4 = new FastColor(color.A,
                        (int)(colorShadowSide.R * shadowratiof5),
                        (int)(colorShadowSide.G * shadowratiof5),
                        (int)(colorShadowSide.B * shadowratiof5 * Yellowness));
                }
                int sidetexture = d_Data.TextureId[tiletype, (int)TileSide.Right];
                int tilecount = drawright;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = AtiArtifactFix; //tilingcount*fix
                float texrecBottom = texrecTop + texrecHeight;
                float texrecRight = texrecLeft + texrecWidth;
                short lastelement = (short)toreturn.verticesCount;
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0, z + 0, y + 1 - flowerfix, texrecLeft, texrecBottom, curcolor));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 0, z + blockheight01, y + 1 - flowerfix, texrecLeft, texrecTop, curcolor3));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 * tilecount, z + 0, y + 1 - flowerfix, texrecRight, texrecBottom, curcolor2));
                toreturn.vertices[toreturn.verticesCount++] = (new VertexPositionTexture(x + 1 * tilecount, z + blockheight11, y + 1 - flowerfix, texrecRight, texrecTop, curcolor4));
                if (occluded)
                {
                    if (occdirnorthwest)
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                    }
                    else
                    {
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//3
                        toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    }
                }

                else if (curcolor.R != curcolor4.R || curcolor3.R == curcolor2.R)
                {
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//0
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//3
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//2
                }
                else
                {
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 1));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//0
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 0));//3
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 2));//1
                    toreturn.indices[toreturn.indicesCount++] = ((ushort)(lastelement + 3));//2
                }
              }
        }
        int waterLevelsCount = 8;
        int PartialWaterBlock = 118;
        private float Max(int a, int b, int c, int d)
        {
            return Math.Max(Math.Max(Math.Max(a, b), c), d);
        }
        /*
        int waterLevelsCount = 8;
        int PartialWaterBlock = 118;
        private int GetWaterLevel(int tt)
        {
            if (tt >= PartialWaterBlock && tt < PartialWaterBlock + waterLevelsCount)
            {
                return tt - PartialWaterBlock;
            }
            if (d_Data.IsWater[tt])
            {
                return waterLevelsCount;
            }
            if (tt == 0)
            {
                return -1;
            }
            return -1;
        }
        */
        public bool ENABLE_TEXTURE_TILING = true; // tiling reduces number of triangles but causes white dots bug on some graphics cards.
        //Texture tiling in one direction.
        private int GetTilingCount(ushort[] currentChunk, int xx, int yy, int zz, int tt, int x, int y, int z, int shadowratio, TileSide dir, TileSideFlags dirflags)
        {
            if (!ENABLE_TEXTURE_TILING)
            {
                return 1;
            }
            //fixes tree Z-fighting
            if (istransparent[currentChunk[MapUtil.Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)]]
                && !d_Data.IsTransparentFully[currentChunk[MapUtil.Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)]]) { return 1; }
            if (dir == TileSide.Top || dir == TileSide.Bottom)
            {
                int shadowz = dir == TileSide.Top ? 1 : -1;
                int newxx = xx + 1;
                for (; ; )
                {
                    if (newxx >= chunksize + 1) { break; }
                    if (currentChunk[MapUtil.Index3d(newxx, yy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                    int shadowratio2 = GetShadowRatio(newxx, yy, zz + shadowz, x + (newxx - xx), y, z + shadowz);
                    if (shadowratio != shadowratio2) { break; }
                    if ((currentChunkDraw[newxx - 1, yy - 1, zz - 1] & (int)dirflags) == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                    currentChunkDrawCount[newxx - 1, yy - 1, zz - 1, (int)dir] = 0;
                    currentChunkDraw[newxx - 1, yy - 1, zz - 1] &= (byte)~dirflags;
                    newxx++;
                }
                return newxx - xx;
            }
            else if (dir == TileSide.Front || dir == TileSide.Back)
            {
                int shadowx = dir == TileSide.Front ? -1 : 1;
                int newyy = yy + 1;
                for (; ; )
                {
                    if (newyy >= chunksize + 1) { break; }
                    if (currentChunk[MapUtil.Index3d(xx, newyy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                    int shadowratio2 = GetShadowRatio(xx + shadowx, newyy, zz, x + shadowx, y + (newyy - yy), z);
                    if (shadowratio != shadowratio2) { break; }
                    if ((currentChunkDraw[xx - 1, newyy - 1, zz - 1] & (int)dirflags) == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                    currentChunkDrawCount[xx - 1, newyy - 1, zz - 1, (int)dir] = 0;
                    currentChunkDraw[xx - 1, newyy - 1, zz - 1] &= (byte)~dirflags;
                    newyy++;
                }
                return newyy - yy;
            }
            else
            {
                int shadowy = dir == TileSide.Left ? -1 : 1;
                int newxx = xx + 1;
                for (; ; )
                {
                    if (newxx >= chunksize + 1) { break; }
                    if (currentChunk[MapUtil.Index3d(newxx, yy, zz, chunksize + 2, chunksize + 2)] != tt) { break; }
                    int shadowratio2 = GetShadowRatio(newxx, yy + shadowy, zz, x + (newxx - xx), y + shadowy, z);
                    if (shadowratio != shadowratio2) { break; }
                    if ((currentChunkDraw[newxx - 1, yy - 1, zz - 1] & (int)dirflags) == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                    currentChunkDrawCount[newxx - 1, yy - 1, zz - 1, (int)dir] = 0;
                    currentChunkDraw[newxx - 1, yy - 1, zz - 1] &= (byte)~dirflags;
                    newxx++;
                }
                return newxx - xx;
            }
        }
        private VerticesIndices GetToReturn(int tiletype, int textureid)
        {
            if (ENABLE_ATLAS1D)
            {
                if (!(istransparent[tiletype] || IsWater(tiletype)))
                {
                    return toreturnatlas1d[textureid / d_TerrainTextures.terrainTexturesPerAtlas];
                }
                else
                {
                    return toreturnatlas1dtransparent[textureid / d_TerrainTextures.terrainTexturesPerAtlas];
                }
            }
            else
            {
                if (!(istransparent[tiletype] || IsWater(tiletype)))
                {
                    return toreturnmain;
                }
                else
                {
                    return toreturntransparent;
                }
            }
        }
        //Calculate shadows lazily, many blocks don't need them.
        int GetShadowRatio(int xx, int yy, int zz, int globalx, int globaly, int globalz)
        {
            return currentChunkShadows[MapUtil.Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)];
            /*
            if (currentChunkShadows[xx, yy, zz] == 0)
            {
                if (IsValidPos(globalx, globaly, globalz))
                {
                    currentChunkShadows[xx, yy, zz] = (byte)(d_MapStorageLight.GetLight(globalx, globaly, globalz) + 1);
                }
                else
                {
                    currentChunkShadows[xx, yy, zz] = (byte)(maxlight + 1);
                }
            }
            return currentChunkShadows[xx, yy, zz] - 1;
            */
        }
        private bool CanSupportTorch(int blocktype)
        {
            return blocktype != SpecialBlockId.Empty
                && d_Data.DrawType1[blocktype] != DrawType.Torch;
        }
         private int getBestLadderWall(int x, int y, int z, ushort[] currentChunk)
        {
        	bool front=false;
        	bool back=false;
        	bool left=false;
        	//bool right=false;
        	int wallscount = 0;
            if (currentChunk[MapUtil.Index3d(x, y - 1, z, chunksize + 2, chunksize + 2)] != 0)
            {
            	front = true;
            	wallscount++;
            }
            if (currentChunk[MapUtil.Index3d(x, y + 1, z, chunksize + 2, chunksize + 2)] != 0)
            {
            	back = true;
            	wallscount++;
            }
            int c = currentChunk[MapUtil.Index3d(x - 1, y, z, chunksize + 2, chunksize + 2)];
        	if (c != 0)
            {
            	left = true;
            	wallscount++;
            }
            if (currentChunk[MapUtil.Index3d(x + 1, y, z, chunksize + 2, chunksize + 2)] != 0)
            {
                //right = true;
            	wallscount++;
            }
            if (wallscount != 1) {
            	return -1;
            } else {
            	if (front) {
            		return 0;
            	}
            	else if (back) {
            		return 1;
            	}
            	else if (left) {
            		return 2;
            	}
            	else {
            		return 3;
            	}
            }
        }
        int getBestLadderInDirection(int x, int y, int z, ushort[] currentChunk, int dir) {
        	int dz = dir;
        	int result = 0;
        	try
        	{
	        	while (currentChunk[MapUtil.Index3d(x, y, z + dz, chunksize + 2, chunksize + 2)] == 152)
	        	{
	        		result = dz;
	        		if (getBestLadderWall(x, y, z + dz, currentChunk) != -1) return result;
	        		dz += dir;
	        	}
        	}
        	catch { }
        	return 0;
        }
   }
}