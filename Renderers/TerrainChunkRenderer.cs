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
        RailMapUtil railmaputil;
        public bool DONOTDRAWEDGES = true;
        public int chunksize = 16; //16x16
        public int texturesPacked = 16;
        public float BlockShadow = 0.6f;
        public IEnumerable<VerticesIndicesToLoad> MakeChunk(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0) { yield break; }
            if (x >= mapstorage.MapSizeX / chunksize || y >= mapstorage.MapSizeY / chunksize || z >= mapstorage.MapSizeZ / chunksize) { yield break; }
            List<ushort> indices = new List<ushort>();
            List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();
            List<ushort> transparentindices = new List<ushort>();
            List<VertexPositionTexture> transparentvertices = new List<VertexPositionTexture>();
            byte[, ,] currentChunk = new byte[chunksize + 2, chunksize + 2, chunksize + 2];
            for (int xx = 0; xx < chunksize + 2; xx++)
            {
                for (int yy = 0; yy < chunksize + 2; yy++)
                {
                    for (int zz = 0; zz < chunksize + 2; zz++)
                    {
                        int xxx = x * chunksize + xx - 1;
                        int yyy = y * chunksize + yy - 1;
                        int zzz = z * chunksize + zz - 1;
                        if (!IsValidPos(xxx, yyy, zzz))
                        {
                            continue;
                        }
                        currentChunk[xx, yy, zz] = (byte)mapstorage.GetTerrainBlock(xxx, yyy, zzz);
                    }
                }
            }
            currentChunkShadows = new float[chunksize + 2, chunksize + 2, chunksize + 2];
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
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        int xxx = x * chunksize + xx;
                        int yyy = y * chunksize + yy;
                        int zzz = z * chunksize + zz;

                        if (!(data.IsTransparentTile(currentChunk[xx + 1, yy + 1, zz + 1])
                            || data.IsWaterTile(currentChunk[xx + 1, yy + 1, zz + 1])))
                        {
                            BlockPolygons(indices, vertices, xxx, yyy, zzz, currentChunk);
                        }
                        else
                        {
                            BlockPolygons(transparentindices, transparentvertices, xxx, yyy, zzz, currentChunk);
                        }
                    }
                }
            }
            if (indices.Count > 0)
            {
                yield return new VerticesIndicesToLoad()
                {
                    indices = indices.ToArray(),
                    vertices = vertices.ToArray(),
                    position =
                        new Vector3(x * chunksize, y * chunksize, z * chunksize)
                };
            }
            if (transparentindices.Count > 0)
            {
                yield return new VerticesIndicesToLoad()
                {
                    indices = transparentindices.ToArray(),
                    vertices = transparentvertices.ToArray(),
                    position =
                        new Vector3(x * chunksize, y * chunksize, z * chunksize),
                    transparent = true
                };
            }
        }
        private bool IsValidPos(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0)
            {
                return false;
            }
            if (x >= mapstorage.MapSizeX || y >= mapstorage.MapSizeY || z >= mapstorage.MapSizeZ)
            {
                return false;
            }
            return true;
        }
        float[, ,] currentChunkShadows;
        private void BlockPolygons(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, byte[, ,] currentChunk)
        {
            int xx = x % chunksize + 1;
            int yy = y % chunksize + 1;
            int zz = z % chunksize + 1;
            var tt = currentChunk[xx, yy, zz];
            if (!data.IsValidTileType(tt))
            {
                return;
            }
            bool drawtop = IsTileEmptyForDrawingOrTransparent(xx, yy, zz + 1, tt, currentChunk);
            bool drawbottom = IsTileEmptyForDrawingOrTransparent(xx, yy, zz - 1, tt, currentChunk);
            bool drawfront = IsTileEmptyForDrawingOrTransparent(xx - 1, yy, zz, tt, currentChunk);
            bool drawback = IsTileEmptyForDrawingOrTransparent(xx + 1, yy, zz, tt, currentChunk);
            bool drawleft = IsTileEmptyForDrawingOrTransparent(xx, yy - 1, zz, tt, currentChunk);
            bool drawright = IsTileEmptyForDrawingOrTransparent(xx, yy + 1, zz, tt, currentChunk);
            int tiletype = tt;
            if (!(drawtop || drawbottom || drawfront || drawback || drawleft || drawright))
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
                if (z == 0) { drawbottom = false; }
                if (x == 0) { drawfront = false; }
                if (x == 256 - 1) { drawback = false; }
                if (y == 0) { drawleft = false; }
                if (y == 256 - 1) { drawright = false; }
            }
            float flowerfix = 0;
            if (data.IsBlockFlower(tiletype))
            {
                drawtop = false;
                drawbottom = false;
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
                blockdrawertorch.AddTorch(myelements, myvertices, x, y, z, type);
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
            if (drawtop)
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
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z + blockheight00, y + 0.0f, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z + blockheight01, y + 1.0f, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z + blockheight10, y + 0.0f, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z + blockheight11, y + 1.0f, texrec.Right, texrec.Bottom, curcolor));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }
            //bottom - same as top, but z is 1 less.
            if (drawbottom)
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
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 0.0f, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 1.0f, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 0.0f, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 1.0f, texrec.Right, texrec.Bottom, curcolor));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }
            //front
            if (drawfront)
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
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 0, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 1, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + blockheight00, y + 0, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + blockheight01, y + 1, texrec.Right, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }
            //back - same as front, but x is 1 greater.
            if (drawback)
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
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 0, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 1, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + blockheight10, y + 0, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + blockheight11, y + 1, texrec.Left, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }
            if (drawleft)
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
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0 + flowerfix, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 0, z + blockheight00, y + 0 + flowerfix, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0 + flowerfix, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1, z + blockheight10, y + 0 + flowerfix, texrec.Left, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }
            //right - same as left, but y is 1 greater.
            if (drawright)
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
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1 - flowerfix, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 0, z + blockheight01, y + 1 - flowerfix, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 1 - flowerfix, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(x + 1, z + blockheight11, y + 1 - flowerfix, texrec.Right, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
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