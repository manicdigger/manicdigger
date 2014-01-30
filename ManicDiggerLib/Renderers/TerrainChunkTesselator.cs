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
        internal TerrainChunkTesselatorCi tesselator = new TerrainChunkTesselatorCi();
        [Inject]
        public ITerrainTextures d_TerrainTextures;
        [Inject]
        public ManicDiggerGameWindow game;
        public int chunksize = 16; //16x16
        public float BlockShadow { get { return tesselator.BlockShadow; } set { tesselator.BlockShadow = value; } }
        int maxblocktypes = GlobalVar.MAX_BLOCKTYPES;
        public bool EnableSmoothLight { get { return tesselator.EnableSmoothLight; } set { tesselator.EnableSmoothLight = value; } }
        int[] currentChunk18 { get { return tesselator.currentChunk18; } set { tesselator.currentChunk18 = value; } }
        bool started { get { return tesselator.started; } set { tesselator.started = value; } }
        int mapsizex { get { return tesselator.mapsizex; } set { tesselator.mapsizex = value; } }
        int mapsizey { get { return tesselator.mapsizey; } set { tesselator.mapsizey = value; } }
        int mapsizez { get { return tesselator.mapsizez; } set { tesselator.mapsizez = value; } }
        public void Start()
        {
            tesselator.game = game.game;
            tesselator.Start();
        }
        bool[] istransparent { get { return tesselator.istransparent; } set { tesselator.istransparent = value; } }
        float[] lightlevels { get { return tesselator.lightlevels; } set { tesselator.lightlevels = value; } }

        public IEnumerable<VerticesIndicesToLoad> MakeChunk(int x, int y, int z,
            int[] chunk18, byte[] shadows18, float[] lightlevels_)
        {
            this.currentChunk18 = chunk18;
            this.currentChunkShadows18 = shadows18;
            this.lightlevels = lightlevels_;

            for (int i = 0; i < maxblocktypes; i++)
            {
                Packet_BlockType b = game.game.blocktypes[i];
                istransparent[i] = (b.DrawType != Packet_DrawTypeEnum.Solid) && (b.DrawType != Packet_DrawTypeEnum.Fluid);
            }

            if (x < 0 || y < 0 || z < 0) { yield break; }
            if (!started) { throw new Exception("not started"); }
            if (x >= mapsizex / chunksize
                || y >= mapsizey / chunksize
                || z >= mapsizez / chunksize) { yield break; }

            for (int i = 0; i < toreturnatlas1d.Length; i++)
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
            foreach (VerticesIndicesToLoad v in GetFinalVerticesIndices(x, y, z))
            {
                yield return v;
            }
        }
        IEnumerable<VerticesIndicesToLoad> GetFinalVerticesIndices(int x, int y, int z)
        {
            for (int i = 0; i < toreturnatlas1d.Length; i++)
            {
                if (toreturnatlas1d[i].indicesCount > 0)
                {
                    yield return GetVerticesIndices(toreturnatlas1d[i], x, y, z, d_TerrainTextures.terrainTextures1d[i % d_TerrainTextures.terrainTexturesPerAtlas], false);
                }
            }
            for (int i = 0; i < toreturnatlas1dtransparent.Length; i++)
            {
                if (toreturnatlas1dtransparent[i].indicesCount > 0)
                {
                    yield return GetVerticesIndices(toreturnatlas1dtransparent[i], x, y, z, d_TerrainTextures.terrainTextures1d[i % d_TerrainTextures.terrainTexturesPerAtlas], true);
                }
            }
        }

        VerticesIndicesToLoad GetVerticesIndices(ModelData m, int x, int y, int z, int texture, bool transparent)
        {
            VerticesIndicesToLoad v = new VerticesIndicesToLoad();
            v.indices = new ushort[m.indicesCount];
            for (int i = 0; i < m.indicesCount; i++)
            {
                v.indices[i] = (ushort)m.indices[i];
            }
            v.indicesCount = m.indicesCount;

            v.vertices = new VertexPositionTexture[m.verticesCount];
            for (int i = 0; i < m.verticesCount; i++)
            {
                VertexPositionTexture vert = new VertexPositionTexture();
                vert.Position.X = m.xyz[i * 3 + 0];
                vert.Position.Y = m.xyz[i * 3 + 1];
                vert.Position.Z = m.xyz[i * 3 + 2];
                vert.u = m.uv[i * 2 + 0];
                vert.v = m.uv[i * 2 + 1];
                vert.r = m.rgba[i * 4 + 0];
                vert.g = m.rgba[i * 4 + 1];
                vert.b = m.rgba[i * 4 + 2];
                vert.a = m.rgba[i * 4 + 3];
                v.vertices[i] = vert;
            }
            v.verticesCount = m.verticesCount;
            v.position = new Vector3(x * chunksize, y * chunksize, z * chunksize);
            v.texture = texture;
            v.transparent = transparent;

            return v;
        }

        private void CalculateBlockPolygons(int x, int y, int z)
        {
            tesselator.CalculateBlockPolygons(x, y, z);
        }
        
        private void CalculateSmoothBlockPolygons(int x, int y, int z)
        {
            tesselator.CalculateSmoothBlockPolygons(x, y, z);
        }

        ModelData[] toreturnatlas1d { get { return tesselator.toreturnatlas1d; } set { tesselator.toreturnatlas1d = value; } }
        ModelData[] toreturnatlas1dtransparent { get { return tesselator.toreturnatlas1dtransparent; } set { tesselator.toreturnatlas1dtransparent = value; } }
        class VerticesIndices
        {
            public ushort[] indices;
            public int indicesCount;
            public VertexPositionTexture[] vertices;
            public int verticesCount;
        }

        byte[] currentChunkShadows18 { get { return tesselator.currentChunkShadows18; } set { tesselator.currentChunkShadows18 = value; } }
        byte[] currentChunkDraw16 { get { return tesselator.currentChunkDraw16; } set { tesselator.currentChunkDraw16 = value; } }
        byte[][] currentChunkDrawCount16 { get { return tesselator.currentChunkDrawCount16; } set { tesselator.currentChunkDrawCount16 = value; } }
        void CalculateVisibleFaces(int[] currentChunk)
        {
            tesselator.CalculateVisibleFaces(currentChunk);
        }

        private void CalculateTilingCount(int[] currentChunk, int startx, int starty, int startz)
        {
            tesselator.CalculateTilingCount(currentChunk, startx, starty, startz);
        }

        bool isvalid(int tt)
        {
            return tesselator.isvalid(tt);
        }

        public bool ENABLE_TEXTURE_TILING { get { return tesselator.ENABLE_TEXTURE_TILING; } set { tesselator.ENABLE_TEXTURE_TILING = value; } }
   }
}
