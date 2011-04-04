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
        public ITerrainInfo mapstorage;
        [Inject]
        public IGameData data;
        [Inject]
        public IBlockRendererTorch blockrenderertorch;
        [Inject]
        public Config3d config3d;
        [Inject]
        public ITerrainRenderer terrainrenderer; //textures
        [Inject]
        public IShadows shadows;
        RailMapUtil railmaputil;
        public bool DONOTDRAWEDGES = true;
        public int chunksize = 16; //16x16
        public int texturesPacked = 16;
        public float BlockShadow = 0.6f;
        public bool ENABLE_ATLAS1D = true;
        int maxblocktypes = 256;
        byte[] currentChunk;
        bool started = false;
        int mapsizex; //cache
        int mapsizey;
        int mapsizez;
        void Start()
        {
            currentChunk = new byte[(chunksize + 2) * (chunksize + 2) * (chunksize + 2)];
            currentChunkShadows = new byte[chunksize + 2, chunksize + 2, chunksize + 2];
            currentChunkDraw = new byte[chunksize, chunksize, chunksize];
            currentChunkDrawCount = new byte[chunksize, chunksize, chunksize, 6];
            mapsizex = mapstorage.MapSizeX;
            mapsizey = mapstorage.MapSizeY;
            mapsizez = mapstorage.MapSizeZ;
            started = true;
            istransparent = data.IsTransparent;
            iswater = data.IsWater;
            isvalid = data.IsValid;
            maxlight = shadows.maxlight;
            maxlightInverse = 1f / maxlight;
            terrainTexturesPerAtlas = terrainrenderer.terrainTexturesPerAtlas;
            terrainTexturesPerAtlasInverse = 1f / terrainrenderer.terrainTexturesPerAtlas;
            allvi.Initialize(VI_MAX);
            for (int i = 0; i < VI_MAX; i++)
            {
                allvi.Push(new VerticesIndices()
                    {
                        indices = new ushort[ushort.MaxValue],
                        vertices = new VertexPositionTexture[ushort.MaxValue],
                    });
            }
        }
        int terrainTexturesPerAtlas;
        float terrainTexturesPerAtlasInverse;
        int VI_MAX = 20;
        int maxlight;
        float maxlightInverse;
        bool[] istransparent;
        bool[] iswater;
        bool[] isvalid;
        public IEnumerable<VerticesIndicesToLoad> MakeChunk(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0) { yield break; }
            if (!started) { Start(); }
            if (x >= mapsizex / chunksize
                || y >= mapsizey / chunksize
                || z >= mapsizez / chunksize) { yield break; }
            if (ENABLE_ATLAS1D)
            {
                toreturnatlas1d = new VerticesIndices[maxblocktypes / terrainrenderer.terrainTexturesPerAtlas];
                toreturnatlas1dtransparent = new VerticesIndices[maxblocktypes / terrainrenderer.terrainTexturesPerAtlas];
                for (int i = 0; i < toreturnatlas1d.Length; i++)
                {
                    //Manual memory allocation for performance - reuse arrays.
                    toreturnatlas1d[i] = allvi.Pop();
                    toreturnatlas1d[i].verticesCount = 0;
                    toreturnatlas1d[i].indicesCount = 0;
                    toreturnatlas1dtransparent[i] = allvi.Pop();
                    toreturnatlas1dtransparent[i].verticesCount = 0;
                    toreturnatlas1dtransparent[i].indicesCount = 0;
                }
            }
            //else //torch block
            {
                toreturnmain = allvi.Pop();
                toreturnmain.verticesCount = 0;
                toreturnmain.indicesCount = 0;
                toreturntransparent = allvi.Pop();
                toreturntransparent.verticesCount = 0;
                toreturntransparent.indicesCount = 0;
            }
            GetExtendedChunk(x, y, z);
            if (IsSolidChunk(currentChunk)) { FreeVi(); yield break; }
            ResetCurrentShadows();
            shadows.OnMakeChunk(x, y, z);
            CalculateVisibleFaces(currentChunk);
            CalculateTilingCount(currentChunk, x * chunksize, y * chunksize, z * chunksize);
            CalculateBlockPolygons(x, y, z);
            foreach (VerticesIndicesToLoad v in GetFinalVerticesIndices(x, y, z))
            {
                yield return v;
            }
            FreeVi();
        }
        void FreeVi()
        {
            for (int i = 0; i < toreturnatlas1d.Length; i++)
            {
                allvi.Push(toreturnatlas1d[i]);
            }
            for (int i = 0; i < toreturnatlas1dtransparent.Length; i++)
            {
                allvi.Push(toreturnatlas1dtransparent[i]);
            }
            allvi.Push(toreturnmain);
            allvi.Push(toreturntransparent);
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
                            texture = terrainrenderer.terrainTextures1d[i % terrainrenderer.terrainTexturesPerAtlas],
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
                            texture = terrainrenderer.terrainTextures1d[i % terrainrenderer.terrainTexturesPerAtlas],
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
                        texture = terrainrenderer.terrainTexture,
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
                        texture = terrainrenderer.terrainTexture,
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
        private void ResetCurrentShadows()
        {
            Array.Clear(currentChunkShadows, 0, currentChunkShadows.Length);
        }
        private bool IsSolidChunk(byte[] currentChunk)
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
            int chunksize = this.chunksize;
            byte[] mapchunk = mapstorage.GetChunk(x * chunksize, y * chunksize, z * chunksize);
            for (int yy = 1; yy < chunksize + 1; yy++)
            {
                for (int zz = 1; zz < chunksize + 1; zz++)
                {
                    int pos = MapUtil.Index3d(1, yy, zz, chunksize + 2, chunksize + 2);
                    int pos2 = MapUtil.Index3d(1 - 1, yy - 1, zz - 1, chunksize, chunksize);
                    for (int xx = 0; xx < chunksize; xx++)
                    {
                        currentChunk[pos] = mapchunk[pos2];
                        //Don't calculate index in inner loop: move across whole row with just pos++;
                        pos++;
                        pos2++;
                    }
                }
            }
            CopyBorders(x, y, z, chunksize);
        }
        private void CopyBorders(int x, int y, int z, int chunksize)
        {
            //copy borders of this chunk

            //z-1
            if (z > 0)
            {
                byte[] mapchunk_ = mapstorage.GetChunk(x * chunksize, y * chunksize, (z - 1) * chunksize);
                for (int xx = 1; xx < chunksize + 1; xx++)
                {
                    for (int yy = 1; yy < chunksize + 1; yy++)
                    {
                        currentChunk[MapUtil.Index3d(xx, yy, 0, chunksize + 2, chunksize + 2)]
                            = mapchunk_[MapUtil.Index3d(xx - 1, yy - 1, chunksize - 1, chunksize, chunksize)];
                    }
                }
            }
            //z+1
            if ((z + 1) < mapsizez / chunksize)
            {
                byte[] mapchunk_ = mapstorage.GetChunk(x * chunksize, y * chunksize, (z + 1) * chunksize);
                for (int xx = 1; xx < chunksize + 1; xx++)
                {
                    for (int yy = 1; yy < chunksize + 1; yy++)
                    {
                        currentChunk[MapUtil.Index3d(xx, yy, chunksize + 1, chunksize + 2, chunksize + 2)]
                            = mapchunk_[MapUtil.Index3d(xx - 1, yy - 1, 0, chunksize, chunksize)];
                    }
                }
            }
            //x - 1
            if (x > 0)
            {
                byte[] mapchunk_ = mapstorage.GetChunk((x - 1) * chunksize, y * chunksize, z * chunksize);
                for (int zz = 1; zz < chunksize + 1; zz++)
                {
                    for (int yy = 1; yy < chunksize + 1; yy++)
                    {
                        currentChunk[MapUtil.Index3d(0, yy, zz, chunksize + 2, chunksize + 2)]
                            = mapchunk_[MapUtil.Index3d(chunksize - 1, yy - 1, zz - 1, chunksize, chunksize)];
                    }
                }
            }
            //x + 1
            if ((x + 1) < mapsizex / chunksize)
            {
                byte[] mapchunk_ = mapstorage.GetChunk((x + 1) * chunksize, y * chunksize, z * chunksize);
                for (int zz = 1; zz < chunksize + 1; zz++)
                {
                    for (int yy = 1; yy < chunksize + 1; yy++)
                    {
                        currentChunk[MapUtil.Index3d(chunksize + 1, yy, zz, chunksize + 2, chunksize + 2)]
                            = mapchunk_[MapUtil.Index3d(0, yy - 1, zz - 1, chunksize, chunksize)];
                    }
                }
            }
            //y - 1
            if (y > 0)
            {
                byte[] mapchunk_ = mapstorage.GetChunk(x * chunksize, (y - 1) * chunksize, z * chunksize);
                for (int xx = 1; xx < chunksize + 1; xx++)
                {
                    for (int zz = 1; zz < chunksize + 1; zz++)
                    {
                        currentChunk[MapUtil.Index3d(xx, 0, zz, chunksize + 2, chunksize + 2)]
                            = mapchunk_[MapUtil.Index3d(xx - 1, chunksize - 1, zz - 1, chunksize, chunksize)];
                    }
                }
            }
            //y + 1
            if (y > 0)
            {
                byte[] mapchunk_ = mapstorage.GetChunk(x * chunksize, (y + 1) * chunksize, z * chunksize);
                for (int xx = 1; xx < chunksize + 1; xx++)
                {
                    for (int zz = 1; zz < chunksize + 1; zz++)
                    {
                        currentChunk[MapUtil.Index3d(xx, chunksize + 1, zz, chunksize + 2, chunksize + 2)]
                            = mapchunk_[MapUtil.Index3d(xx - 1, 0, zz - 1, chunksize, chunksize)];
                    }
                }
            }
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
        FastStack<VerticesIndices> allvi = new FastStack<VerticesIndices>();
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
        byte[, ,] currentChunkShadows;
        byte[, ,] currentChunkDraw;
        byte[, , ,] currentChunkDrawCount;
        void CalculateVisibleFaces(byte[] currentChunk)
        {
            int chunksize = this.chunksize;
            int movez = (chunksize + 2) * (chunksize + 2);
            unsafe
            {
                fixed (byte* currentChunk_ = currentChunk )
                fixed (bool* iswater_ = iswater)
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
                                byte tt = currentChunk_[pos];
                                if (tt == 0) { continue; }
                                int draw = (int)TileSideFlags.None;
                                //Instead of calculating position index with MapUtil.Index(),
                                //relative moves are used
                                //(just addition instead of multiplication - 1.5x - 2x faster)
                                //z+1
                                {
                                    int pos2 = pos + movez;
                                    byte tt2 = currentChunk_[pos2];
                                    if (tt2 == 0
                                        || (iswater_[tt2] && (!iswater_[tt]))
                                        || istransparent_[tt2])
                                    {
                                        draw |= (int)TileSideFlags.Top;
                                    }
                                }
                                //z-1
                                {
                                    int pos2 = pos - movez;
                                    byte tt2 = currentChunk_[pos2];
                                    if (tt2 == 0
                                        || (iswater_[tt2] && (!iswater_[tt]))
                                        || istransparent_[tt2])
                                    {
                                        draw |= (int)TileSideFlags.Bottom;
                                    }
                                }
                                //x-1
                                {
                                    int pos2 = pos - 1;
                                    byte tt2 = currentChunk_[pos2];
                                    if (tt2 == 0
                                        || (iswater_[tt2] && (!iswater_[tt]))
                                        || istransparent_[tt2])
                                    {
                                        draw |= (int)TileSideFlags.Front;
                                    }
                                }
                                //x+1
                                {
                                    int pos2 = pos + 1;
                                    byte tt2 = currentChunk_[pos2];
                                    if (tt2 == 0
                                        || (iswater_[tt2] && (!iswater_[tt]))
                                        || istransparent_[tt2])
                                    {
                                        draw |= (int)TileSideFlags.Back;
                                    }
                                }
                                //y-1
                                {
                                    int pos2 = pos - (chunksize + 2);
                                    byte tt2 = currentChunk_[pos2];
                                    if (tt2 == 0
                                        || (iswater_[tt2] && (!iswater_[tt]))
                                        || istransparent_[tt2])
                                    {
                                        draw |= (int)TileSideFlags.Left;
                                    }
                                }
                                //y-1
                                {
                                    int pos2 = pos + (chunksize + 2);
                                    byte tt2 = currentChunk_[pos2];
                                    if (tt2 == 0
                                        || (iswater_[tt2] && (!iswater_[tt]))
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
        private void CalculateTilingCount(byte[] currentChunk, int startx, int starty, int startz)
        {
            Array.Clear(currentChunkDrawCount, 0, currentChunkDrawCount.Length);
            unsafe
            {
                fixed(byte* currentChunk_ = currentChunk)
                for (int zz = 1; zz < chunksize + 1; zz++)
                {
                    for (int yy = 1; yy < chunksize + 1; yy++)
                    {
                        int pos = MapUtil.Index3d(0, yy, zz, chunksize + 2, chunksize + 2);
                        for (int xx = 1; xx < chunksize + 1; xx++)
                        {
                            byte tt = currentChunk_[pos + xx];
                            if (tt == 0) { continue; } //faster
                            int x = startx + xx - 1;
                            int y = starty + yy - 1;
                            int z = startz + zz - 1;
                            int draw = currentChunkDraw[xx - 1, yy - 1, zz - 1];
                            if (draw == 0) { continue; } //faster
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
        float texrecLeft;
        float texrecTop;
        float texrecWidth;
        float texrecHeight;
        private void BlockPolygons(int x, int y, int z, byte[] currentChunk)
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
            FastColor color = mapstorage.GetTerrainBlockColor(x, y, z);
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
            if (data.IsFlower[tiletype])
            {
                drawtop = 0;
                drawbottom = 0;
                flowerfix = 0.5f;
            }
            RailDirectionFlags rail = data.Rail[tiletype];
            float blockheight = 1;//= data.GetTerrainBlockHeight(tiletype);
            if (rail != RailDirectionFlags.None)
            {
                blockheight = 0.3f;
                /*
                RailPolygons(myelements, myvertices, x, y, z, rail);
                return;
                */
            }
            if (tt == data.BlockIdSingleStairs)
            {
                blockheight = 0.5f;
            }
            if (tt == data.BlockIdTorch)
            {
                TorchType type = TorchType.Normal;
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx - 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Front; }
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx + 1, yy, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Back; }
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx, yy - 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Left; }
                if (CanSupportTorch(currentChunk[MapUtil.Index3d(xx, yy + 1, zz, chunksize + 2, chunksize + 2)])) { type = TorchType.Right; }
                List<ushort> torchelements = new List<ushort>();
                List<VertexPositionTexture> torchvertices = new List<VertexPositionTexture>();
                blockrenderertorch.AddTorch(torchelements, torchvertices, x, y, z, type);
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
                    railmaputil = new RailMapUtil() { data = data, mapstorage = mapstorage };
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
            FastColor curcolor = color;
            texrecLeft = 0;
            texrecHeight = terrainTexturesPerAtlasInverse;
            //top
            if (drawtop > 0)
            {
                curcolor = color;
                int shadowratio = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                if (shadowratio != maxlight)
                {
                    float shadowratiof = ((float)shadowratio) * maxlightInverse;
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratiof),
                        (int)(color.G * shadowratiof),
                        (int)(color.B * shadowratiof));
                }
                int sidetexture = data.TextureId[tiletype, (int)TileSide.Top];
                int tilecount = drawtop;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = tilecount;
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
                    float shadowratiof = ((float)shadowratio) * maxlightInverse;
                    curcolor = new FastColor(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratiof)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratiof)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratiof)));
                }
                int sidetexture = data.TextureId[tiletype, (int)TileSide.Bottom];
                int tilecount = drawbottom;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = tilecount;
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
                    float shadowratiof = ((float)shadowratio) * maxlightInverse;
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratiof),
                        (int)(color.G * shadowratiof),
                        (int)(color.B * shadowratiof));
                }
                int sidetexture = data.TextureId[tiletype, (int)TileSide.Front];
                int tilecount = drawfront;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = tilecount;
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
                    float shadowratiof = ((float)shadowratio) * maxlightInverse;
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratiof),
                        (int)(color.G * shadowratiof),
                        (int)(color.B * shadowratiof));
                }
                int sidetexture = data.TextureId[tiletype, (int)TileSide.Back];
                int tilecount = drawback;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = tilecount;
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
                    float shadowratiof = ((float)shadowratio) * maxlightInverse;
                    curcolor = new FastColor(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratiof)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratiof)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratiof)));
                }

                int sidetexture = data.TextureId[tiletype, (int)TileSide.Left];
                int tilecount = drawleft;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = tilecount;
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
                    float shadowratiof = ((float)shadowratio) * maxlightInverse;
                    curcolor = new FastColor(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratiof)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratiof)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratiof)));
                }

                int sidetexture = data.TextureId[tiletype, (int)TileSide.Right];
                int tilecount = drawright;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                texrecTop = (terrainTexturesPerAtlasInverse * (int)(sidetexture % terrainTexturesPerAtlas));
                texrecWidth = tilecount;
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
        int waterLevelsCount = 8;
        int PartialWaterBlock = 118;
        private float Max(int a, int b, int c, int d)
        {
            return Math.Max(Math.Max(Math.Max(a, b), c), d);
        }
        private int GetWaterLevel(byte tt)
        {
            if (tt >= PartialWaterBlock && tt < PartialWaterBlock + waterLevelsCount)
            {
                return tt - PartialWaterBlock;
            }
            if (data.IsWater[tt])
            {
                return waterLevelsCount;
            }
            if (tt == 0)
            {
                return -1;
            }
            return -1;
        }
        //Texture tiling in one direction.
        private int GetTilingCount(byte[] currentChunk, int xx, int yy, int zz, byte tt, int x, int y, int z, int shadowratio, TileSide dir, TileSideFlags dirflags)
        {
            //fixes tree Z-fighting
            if (istransparent[currentChunk[MapUtil.Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)]]
                && !data.IsTransparentFully[currentChunk[MapUtil.Index3d(xx, yy, zz, chunksize + 2, chunksize + 2)]]) { return 1; }
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
        private VerticesIndices GetToReturn(byte tiletype, int textureid)
        {
            if (ENABLE_ATLAS1D)
            {
                if (!(istransparent[tiletype] || iswater[tiletype]))
                {
                    return toreturnatlas1d[textureid / terrainrenderer.terrainTexturesPerAtlas];
                }
                else
                {
                    return toreturnatlas1dtransparent[textureid / terrainrenderer.terrainTexturesPerAtlas];
                }
            }
            else
            {
                if (!(istransparent[tiletype] || iswater[tiletype]))
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
            if (currentChunkShadows[xx, yy, zz] == 0)
            {
                if (IsValidPos(globalx, globaly, globalz))
                {
                    currentChunkShadows[xx, yy, zz] = (byte)(mapstorage.GetLight(globalx, globaly, globalz) + 1);
                }
                else
                {
                    currentChunkShadows[xx, yy, zz] = (byte)(maxlight + 1);
                }
            }
            return currentChunkShadows[xx, yy, zz] - 1;
        }
        private bool CanSupportTorch(byte blocktype)
        {
            return blocktype != SpecialBlockId.Empty
                && blocktype != data.BlockIdTorch;
        }
    }
}