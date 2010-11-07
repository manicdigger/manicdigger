using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Drawing;
using ManicDigger.Collisions;

namespace ManicDigger
{
    public class TerrainChunkRenderer
    {
        [Inject]
        public ITerrainInfo mapstorage { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public IBlockDrawerTorch blockdrawertorch { get; set; }
        [Inject]
        public Config3d config3d { get; set; }
        [Inject]
        public ITerrainRenderer terrainrenderer { get; set; }//textures
        RailMapUtil railmaputil;
        public bool DONOTDRAWEDGES = true;
        public int chunksize = 16; //16x16
        public int texturesPacked = 16;
        public float BlockShadow = 0.6f;
        public bool ENABLE_ATLAS1D = true;
        int maxblocktypes = 256;
        byte[, ,] currentChunk;
        bool started = false;
        int mapsizex; //cache
        int mapsizey;
        int mapsizez;
        void Start()
        {
            currentChunk = new byte[chunksize + 2, chunksize + 2, chunksize + 2];
            currentChunkShadows = new float[chunksize + 2, chunksize + 2, chunksize + 2];
            currentChunkDraw = new byte[chunksize, chunksize, chunksize, 6];
            mapsizex = mapstorage.MapSizeX;
            mapsizey = mapstorage.MapSizeY;
            mapsizez = mapstorage.MapSizeZ;
            started = true;
        }
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
                    toreturnatlas1d[i] = new VerticesIndices();
                    toreturnatlas1dtransparent[i] = new VerticesIndices();
                }
            }
            //else //torch block
            {
                toreturnmain = new VerticesIndices();
                toreturntransparent = new VerticesIndices();
            }
            GetExtendedChunk(x, y, z);
            if (IsSolidChunk(currentChunk)) { yield break; }
            ResetCurrentShadows();
            CalculateVisibleFaces(currentChunk);
            CalculateTilingCount(currentChunk, x * chunksize, y * chunksize, z * chunksize);
            CalculateBlockPolygons(x, y, z);
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
                    if (toreturnatlas1d[i].indices.Count > 0)
                    {
                        yield return new VerticesIndicesToLoad()
                        {
                            indices = toreturnatlas1d[i].indices.ToArray(),
                            vertices = toreturnatlas1d[i].vertices.ToArray(),
                            position =
                                new Vector3(x * chunksize, y * chunksize, z * chunksize),
                            texture = terrainrenderer.terrainTextures1d[i % terrainrenderer.terrainTexturesPerAtlas],
                        };
                    }
                }
                for (int i = 0; i < toreturnatlas1dtransparent.Length; i++)
                {
                    if (toreturnatlas1dtransparent[i].indices.Count > 0)
                    {
                        yield return new VerticesIndicesToLoad()
                        {
                            indices = toreturnatlas1dtransparent[i].indices.ToArray(),
                            vertices = toreturnatlas1dtransparent[i].vertices.ToArray(),
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
                if (toreturnmain.indices.Count > 0)
                {
                    yield return new VerticesIndicesToLoad()
                    {
                        indices = toreturnmain.indices.ToArray(),
                        vertices = toreturnmain.vertices.ToArray(),
                        position =
                            new Vector3(x * chunksize, y * chunksize, z * chunksize),
                        texture = terrainrenderer.terrainTexture,
                    };
                }
                if (toreturntransparent.indices.Count > 0)
                {
                    yield return new VerticesIndicesToLoad()
                    {
                        indices = toreturntransparent.indices.ToArray(),
                        vertices = toreturntransparent.vertices.ToArray(),
                        position =
                            new Vector3(x * chunksize, y * chunksize, z * chunksize),
                        transparent = true,
                        texture = terrainrenderer.terrainTexture,
                    };
                }
            }
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
                        BlockPolygons(xxx, yyy, zzz, currentChunk);
                    }
                }
            }
        }
        private void ResetCurrentShadows()
        {
            for (int xx = 0; xx < chunksize + 2; xx++)
            {
                for (int yy = 0; yy < chunksize + 2; yy++)
                {
                    for (int zz = 0; zz < chunksize + 2; zz++)
                    {
                        currentChunkShadows[xx, yy, zz] = float.NaN;
                    }
                }
            }
        }
        private bool IsSolidChunk(byte[, ,] currentChunk)
        {
            for (int xx = 0; xx < chunksize + 2; xx++)
            {
                for (int yy = 0; yy < chunksize + 2; yy++)
                {
                    for (int zz = 0; zz < chunksize + 2; zz++)
                    {
                        if (currentChunk[xx, yy, zz] != currentChunk[0, 0, 0])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private void GetExtendedChunk(int x, int y, int z)
        {
            byte[] mapchunk = mapstorage.GetChunk(x * chunksize, y * chunksize, z * chunksize);          
            for (int xx = 0; xx < chunksize + 2; xx++)
            {
                for (int yy = 0; yy < chunksize + 2; yy++)
                {
                    for (int zz = 0; zz < chunksize + 2; zz++)
                    {
                        int xxx = x * chunksize + xx - 1;
                        int yyy = y * chunksize + yy - 1;
                        int zzz = z * chunksize + zz - 1;
                        if (xx != 0 && yy != 0 && zz != 0
                            && xx != chunksize + 1 && yy != chunksize + 1 && zz != chunksize + 1)
                        {
                            currentChunk[xx, yy, zz] = mapchunk[MapUtil.Index(xx - 1, yy - 1, zz - 1, chunksize, chunksize)];
                        }
                        else
                        {
                            if (!IsValidPos(xxx, yyy, zzz))
                            {
                                continue;
                            }
                            currentChunk[xx, yy, zz] = (byte)mapstorage.GetTerrainBlock(xxx, yyy, zzz);
                        }
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
            public List<ushort> indices = new List<ushort>();
            public List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();
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
        float[, ,] currentChunkShadows;
        byte[, , ,] currentChunkDraw;
        void CalculateVisibleFaces(byte[,,] currentChunk)
        {
            for (int xx = 1; xx < chunksize + 1; xx++)
            {
                for (int yy = 1; yy < chunksize + 1; yy++)
                {
                    for (int zz = 1; zz < chunksize + 1; zz++)
                    {
                        byte tt = currentChunk[xx, yy, zz];
                        if (tt == 0) { continue; }
                        currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Top] = IsTileEmptyForDrawingOrTransparent(xx, yy, zz + 1, tt, currentChunk) ? (byte)1 : (byte)0;
                        currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Bottom] = IsTileEmptyForDrawingOrTransparent(xx, yy, zz - 1, tt, currentChunk) ? (byte)1 : (byte)0;
                        currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Front] = IsTileEmptyForDrawingOrTransparent(xx - 1, yy, zz, tt, currentChunk) ? (byte)1 : (byte)0;
                        currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Back] = IsTileEmptyForDrawingOrTransparent(xx + 1, yy, zz, tt, currentChunk) ? (byte)1 : (byte)0;
                        currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Left] = IsTileEmptyForDrawingOrTransparent(xx, yy - 1, zz, tt, currentChunk) ? (byte)1 : (byte)0;
                        currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Right] = IsTileEmptyForDrawingOrTransparent(xx, yy + 1, zz, tt, currentChunk) ? (byte)1 : (byte)0;
                    }
                }
            }
        }
        private void CalculateTilingCount(byte[, ,] currentChunk, int startx, int starty, int startz)
        {
            for (int xx = 1; xx < chunksize + 1; xx++)
            {
                for (int yy = 1; yy < chunksize + 1; yy++)
                {
                    for (int zz = 1; zz < chunksize + 1; zz++)
                    {
                        byte tt = currentChunk[xx, yy, zz];
                        int x = startx + xx - 1;
                        int y = starty + yy - 1;
                        int z = startz + zz - 1;
                        if (currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Top] > 0)
                        {
                            float shadowratioTop = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                            currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Top] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Top);
                        }
                        if (currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Bottom] > 0)
                        {
                            float shadowratioTop = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
                            currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Bottom] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Bottom);
                        }
                        if (currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Front] > 0)
                        {
                            float shadowratioTop = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
                            currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Front] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Front);
                        }
                        if (currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Back] > 0)
                        {
                            float shadowratioTop = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
                            currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Back] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Back);
                        }
                        if (currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Left] > 0)
                        {
                            float shadowratioTop = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
                            currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Left] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Left);
                        }
                        if (currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Right] > 0)
                        {
                            float shadowratioTop = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
                            currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Right] = (byte)GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratioTop, TileSide.Right);
                        }
                    }
                }
            }
        }
        private void BlockPolygons(int x, int y, int z, byte[, ,] currentChunk)
        {
            int xx = x % chunksize + 1;
            int yy = y % chunksize + 1;
            int zz = z % chunksize + 1;
            var tt = currentChunk[xx, yy, zz];
            if (!data.IsValidTileType(tt))
            {
                return;
            }
            byte drawtop = currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Top];
            byte drawbottom = currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Bottom];
            byte drawfront = currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Front];
            byte drawback = currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Back];
            byte drawleft = currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Left];
            byte drawright = currentChunkDraw[xx - 1, yy - 1, zz - 1, (int)TileSide.Right];
            int tiletype = tt;
            if (drawtop == 0 && drawbottom == 0 && drawfront == 0 && drawback == 0 && drawleft == 0 && drawright == 0)
            {
                return;
            }
            Color color = mapstorage.GetTerrainBlockColor(x, y, z);
            Color colorShadowSide = Color.FromArgb(color.A,
                (int)(color.R * BlockShadow),
                (int)(color.G * BlockShadow),
                (int)(color.B * BlockShadow));
            if (DONOTDRAWEDGES)
            {
                //if the game is fillrate limited, then this makes it much faster.
                //(39fps vs vsync 75fps)
                //bbb.
                if (z == 0) { drawbottom = 0; }
                if (x == 0) { drawfront = 0; }
                if (x == 256 - 1) { drawback = 0; }
                if (y == 0) { drawleft = 0; }
                if (y == 256 - 1) { drawright = 0; }
            }
            float flowerfix = 0;
            if (data.IsBlockFlower(tiletype))
            {
                drawtop = 0;
                drawbottom = 0;
                flowerfix = 0.5f;
            }
            RailDirectionFlags rail = data.GetRail(tiletype);
            float blockheight = 1;//= data.GetTerrainBlockHeight(tiletype);
            if (rail != RailDirectionFlags.None)
            {
                blockheight = 0.3f;
                /*
                RailPolygons(myelements, myvertices, x, y, z, rail);
                return;
                */
            }
            if (tt == data.TileIdSingleStairs)
            {
                blockheight = 0.5f;
            }
            if (tt == data.TileIdTorch)
            {
                TorchType type = TorchType.Normal;
                if (CanSupportTorch(currentChunk[xx - 1, yy, zz])) { type = TorchType.Front; }
                if (CanSupportTorch(currentChunk[xx + 1, yy, zz])) { type = TorchType.Back; }
                if (CanSupportTorch(currentChunk[xx, yy - 1, zz])) { type = TorchType.Left; }
                if (CanSupportTorch(currentChunk[xx, yy + 1, zz])) { type = TorchType.Right; }
                blockdrawertorch.AddTorch(toreturnmain.indices, toreturnmain.vertices, x, y, z, type);
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
            Color curcolor = color;
            //top
            if (drawtop > 0)
            {
                curcolor = color;
                float shadowratio = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                if (shadowratio != 1)
                {
                    curcolor = Color.FromArgb(color.A,
                        (int)(color.R * shadowratio),
                        (int)(color.G * shadowratio),
                        (int)(color.B * shadowratio));
                }
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Top);
                int tilecount = drawtop;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 0.0f, z + blockheight00, y + 0.0f, texrec.Left, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0.0f, z + blockheight01, y + 1.0f, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1.0f * tilecount, z + blockheight10, y + 0.0f, texrec.Right, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1.0f * tilecount, z + blockheight11, y + 1.0f, texrec.Right, texrec.Bottom, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
            //bottom - same as top, but z is 1 less.
            if (drawbottom > 0)
            {
                curcolor = colorShadowSide;
                float shadowratio = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
                if (shadowratio != 1)
                {
                    curcolor = Color.FromArgb(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratio)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratio)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratio)));
                }
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Bottom);
                int tilecount = drawbottom;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 0.0f, texrec.Left, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 1.0f, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1.0f * tilecount, z, y + 0.0f, texrec.Right, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1.0f * tilecount, z, y + 1.0f, texrec.Right, texrec.Bottom, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
            //front
            if (drawfront > 0)
            {
                curcolor = color;
                float shadowratio = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
                if (shadowratio != 1)
                {
                    curcolor = Color.FromArgb(color.A,
                        (int)(color.R * shadowratio),
                        (int)(color.G * shadowratio),
                        (int)(color.B * shadowratio));
                }
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Front);
                int tilecount = drawfront;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 0, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 1 * tilecount, texrec.Right, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + blockheight00, y + 0, texrec.Left, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + blockheight01, y + 1 * tilecount, texrec.Right, texrec.Top, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
            //back - same as front, but x is 1 greater.
            if (drawback > 0)
            {
                curcolor = color;
                float shadowratio = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
                if (shadowratio != 1)
                {
                    curcolor = Color.FromArgb(color.A,
                        (int)(color.R * shadowratio),
                        (int)(color.G * shadowratio),
                        (int)(color.B * shadowratio));
                }
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Back);
                int tilecount = drawback;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 0, texrec.Right, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 1 * tilecount, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + blockheight10, y + 0, texrec.Right, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + blockheight11, y + 1 * tilecount, texrec.Left, texrec.Top, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
            if (drawleft > 0)
            {
                curcolor = colorShadowSide;
                float shadowratio = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
                if (shadowratio != 1)
                {
                    curcolor = Color.FromArgb(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratio)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratio)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratio)));
                }

                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Left);
                int tilecount = drawleft;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0 + flowerfix, texrec.Right, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0, z + blockheight00, y + 0 + flowerfix, texrec.Right, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 * tilecount, z + 0, y + 0 + flowerfix, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 * tilecount, z + blockheight10, y + 0 + flowerfix, texrec.Left, texrec.Top, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
            //right - same as left, but y is 1 greater.
            if (drawright > 0)
            {
                curcolor = colorShadowSide;
                float shadowratio = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
                if (shadowratio != 1)
                {
                    curcolor = Color.FromArgb(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratio)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratio)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratio)));
                }

                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Right);
                int tilecount = drawright;
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1 - flowerfix, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0, z + blockheight01, y + 1 - flowerfix, texrec.Left, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 * tilecount, z + 0, y + 1 - flowerfix, texrec.Right, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 * tilecount, z + blockheight11, y + 1 - flowerfix, texrec.Right, texrec.Top, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
        }
        private int GetTilingCount(byte[, ,] currentChunk, int xx, int yy, int zz, byte tt, int x, int y, int z, float shadowratio, TileSide dir)
        {
            //fixes tree Z-fighting
            if (data.IsTransparentTile(currentChunk[xx, yy, zz]) && !data.IsTransparentTileFully(currentChunk[xx, yy, zz])) { return 1; }
            if (dir == TileSide.Top || dir == TileSide.Bottom)
            {
                int shadowz = dir == TileSide.Top ? 1 : -1;
                int newxx = xx + 1;
                for (; ; )
                {
                    if (newxx >= chunksize + 1) { break; }
                    if (currentChunk[newxx, yy, zz] != tt) { break; }
                    float shadowratio2 = GetShadowRatio(newxx, yy, zz + shadowz, x + (newxx - xx), y, z + shadowz);
                    if (shadowratio != shadowratio2) { break; }
                    if (currentChunkDraw[newxx - 1, yy - 1, zz - 1, (int)dir] == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                    currentChunkDraw[newxx - 1, yy - 1, zz - 1, (int)dir] = 0;
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
                    if (currentChunk[xx, newyy, zz] != tt) { break; }
                    float shadowratio2 = GetShadowRatio(xx + shadowx, newyy, zz, x + shadowx, y + (newyy - yy), z);
                    if (shadowratio != shadowratio2) { break; }
                    if (currentChunkDraw[xx - 1, newyy - 1, zz - 1, (int)dir] == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                    currentChunkDraw[xx - 1, newyy - 1, zz - 1, (int)dir] = 0;
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
                    if (currentChunk[newxx, yy, zz] != tt) { break; }
                    float shadowratio2 = GetShadowRatio(newxx, yy + shadowy, zz, x + (newxx - xx), y + shadowy, z);
                    if (shadowratio != shadowratio2) { break; }
                    if (currentChunkDraw[newxx - 1, yy - 1, zz - 1, (int)dir] == 0) { break; } // fixes water and rail problem (chunk-long stripes)
                    currentChunkDraw[newxx - 1, yy - 1, zz - 1, (int)dir] = 0;
                    newxx++;
                }
                return newxx - xx;
            }
        }
        private VerticesIndices GetToReturn(byte tiletype, int textureid)
        {
            if (ENABLE_ATLAS1D)
            {
                if (!(data.IsTransparentTile(tiletype) || data.IsWaterTile(tiletype)))
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
                if (!(data.IsTransparentTile(tiletype) || data.IsWaterTile(tiletype)))
                {
                    return toreturnmain;
                }
                else
                {
                    return toreturntransparent;
                }
            }
        }
        bool IsTileEmptyForDrawingOrTransparent(int xx, int yy, int zz, int adjacenttiletype, byte[, ,] currentChunk)
        {
            byte tt = currentChunk[xx, yy, zz];
            if (!config3d.ENABLE_TRANSPARENCY)
            {
                return tt == data.TileIdEmpty;
            }
            return tt == data.TileIdEmpty
                || (data.IsWaterTile(tt)
                 && (!data.IsWaterTile(adjacenttiletype)))
                || data.IsTransparentTile(tt);
        }
        float GetShadowRatio(int xx, int yy, int zz, int globalx, int globaly, int globalz)
        {
            if (float.IsNaN(currentChunkShadows[xx, yy, zz]))
            {
                if (IsValidPos(globalx, globaly, globalz))
                {
                    currentChunkShadows[xx, yy, zz] = (float)mapstorage.GetLight(globalx, globaly, globalz)
                        / mapstorage.LightMaxValue();
                }
                else
                {
                    currentChunkShadows[xx, yy, zz] = 1;
                }
            }
            return currentChunkShadows[xx, yy, zz];
        }
        private bool CanSupportTorch(byte blocktype)
        {
            return blocktype != data.TileIdEmpty
                && blocktype != data.TileIdTorch;
        }
    }
}