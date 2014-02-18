using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ManicDigger
{
    public partial class ManicDiggerGameWindow
    {
        int chunkupdates;
        public int ChunkUpdates { get { return chunkupdates; } }
        public int maxlight { get { return 15; } }
        public bool ShadowsFull { get { return false; } set { } }

        IShadows3x3x3 shadows;
        bool terrainRendererStarted;

        public void StartTerrain()
        {
            d_TerrainChunkTesselator.Start();
            if (shadowssimple)
            {
                shadows = new Shadows3x3x3Simple();
            }
            else
            {
                shadows = new Shadows3x3x3();
            }
            shadows.Start();
            terrainRendererStarted = true;
        }

        int mapAreaSize { get { return (int)d_Config3d.viewdistance * 2; } }
        int centerAreaSize { get { return (int)d_Config3d.viewdistance / 2; } }
        int mapAreaSizeZ { get { return mapAreaSize; } }

        int chunksizebits = 4;
        int mapsizexchunks { get { return game.MapSizeX >> chunksizebits; } }
        int mapsizeychunks { get { return game.MapSizeY >> chunksizebits; } }
        int mapsizezchunks { get { return game.MapSizeZ >> chunksizebits; } }

        public void UpdateTerrain()
        {
            if (!terrainRendererStarted)
            {
                //Start() not called yet.
                return;
            }

            if ((lastplacedblock != null)
                && !(lastplacedblock.Value.x == -1 && lastplacedblock.Value.y == -1 && lastplacedblock.Value.z == -1))
            {
                Dictionary<string, bool> ChunksToRedraw = new Dictionary<string, bool>();
                foreach (float[] a in BlocksAround(new float[] { lastplacedblock.Value.x, lastplacedblock.Value.y, lastplacedblock.Value.z }))
                {
                    ChunksToRedraw[Vector3ToString(new int[] { (int)((int)a[0] / chunksize), (int)((int)a[1] / chunksize), (int)((int)a[2] / chunksize) })] = true;
                }
                foreach (string key in ChunksToRedraw.Keys)
                {
                    int[] c = new int[3];
                    Vector3FromString(key, c);
                    int xx = c[0];
                    int yy = c[1];
                    int zz = c[2];
                    if (xx >= 0 && yy >= 0 && zz >= 0
                        && xx < game.MapSizeX / chunksize && yy < game.MapSizeY / chunksize && zz < game.MapSizeZ / chunksize)
                    {
                        Chunk chunk = game.chunks[MapUtil.Index3d(xx, yy, zz, mapsizexchunks, mapsizeychunks)];
                        if (chunk == null || chunk.rendered == null)
                        {
                            continue;
                        }
                        if (chunk.rendered.dirty)
                        {
                            RedrawChunk(xx, yy, zz);
                        }
                    }
                }
                lastplacedblock = new Vector3i(-1, -1, -1);
            }
            int updated = 0;
            for (; ; )
            {
                NearestDirty(tempnearestpos);
                if (tempnearestpos[0] == -1 && tempnearestpos[1] == -1 && tempnearestpos[2] == -1)
                {
                    break;
                }
                RedrawChunk(tempnearestpos[0], tempnearestpos[1], tempnearestpos[2]);
                /*if (updated++ >= 1)*/
                {
                    break;
                }
                /*if (framestopwatch.ElapsedMilliseconds > 5)
                {
                    break;
                }*/
            }
            UnloadRendererChunks();
        }

        public static float[][] BlocksAround(float[] pos)
        {
            float[][] arr = new float[7][];
            arr[0] = pos;
            arr[1] = new float[] { pos[0] + 1, pos[1] + 0, pos[2] + 0 };
            arr[2] = new float[] { pos[0] - 1, pos[1] + 0, pos[2] + 0 };
            arr[3] = new float[] { pos[0] + 0, pos[1] + 1, pos[2] + 0 };
            arr[4] = new float[] { pos[0] + 0, pos[1] - 1, pos[2] + 0 };
            arr[5] = new float[] { pos[0] + 0, pos[1] + 0, pos[2] + 1 };
            arr[6] = new float[] { pos[0] + 0, pos[1] + 0, pos[2] - 1 };
            return arr;
        }

        public static string Vector3ToString(int[] p)
        {
            return p[0] + ":" + p[1] + ":" + p[2];
        }

        public static void Vector3FromString(string s, int[] resultpoint)
        {
            string[] ss1 = s.Split(new char[] { ':' });
            resultpoint[0] = int.Parse(ss1[0]);
            resultpoint[1] = int.Parse(ss1[1]);
            resultpoint[2] = int.Parse(ss1[2]);
        }

        int unloadIterationXy;
        int[] unloadxyztemp = new int[] { 0, 0 };
        void UnloadRendererChunks()
        {
            int px = (int)(((int)player.playerposition.X) / chunksize);
            int py = (int)(((int)player.playerposition.Z) / chunksize);
            int pz = (int)(((int)player.playerposition.Y) / chunksize);

            int chunksxy = this.mapAreaSize / chunksize / 2;
            int chunksz = this.mapAreaSizeZ / chunksize / 2;

            int startx = px - chunksxy;
            int endx = px + chunksxy;
            int starty = py - chunksxy;
            int endy = py + chunksxy;
            int startz = pz - chunksz;
            int endz = pz + chunksz;

            if (startx < 0) { startx = 0; }
            if (starty < 0) { starty = 0; }
            if (startz < 0) { startz = 0; }
            if (endx >= mapsizexchunks) { endx = mapsizexchunks - 1; }
            if (endy >= mapsizeychunks) { endy = mapsizeychunks - 1; }
            if (endz >= mapsizezchunks) { endz = mapsizezchunks - 1; }


            for (int i = 0; i < 1000; i++)
            {
                unloadIterationXy++;
                if (unloadIterationXy >= mapsizexchunks * mapsizeychunks * mapsizezchunks)
                {
                    unloadIterationXy = 0;
                }
                var xyz = MapUtil.Pos(unloadIterationXy, mapsizexchunks, mapsizeychunks);
                int x = xyz.x;
                int y = xyz.y;
                int z = xyz.z;
                int pos = MapUtil.Index3d(x, y, z, mapsizexchunks, mapsizeychunks);
                bool unloaded = false;

                Chunk c = game.chunks[pos];
                if (c == null
                    || c.rendered == null
                    || c.rendered.ids == null)
                {
                    continue;
                }
                if (x < startx || y < starty || z < startz
                    || x > endx || y > endy || z > endz)
                {
                    foreach (int loadedSubmesh in c.rendered.ids)
                    {
                        d_Batcher.Remove(loadedSubmesh);
                    }
                    c.rendered.ids = null;
                    c.rendered.dirty = true;
                    c.rendered.light = null;
                }
                unloaded = true;
                if (unloaded)
                {
                    return;
                }
            }
        }
        
        int intMaxValue = 2147483647;
        int[] tempnearestpos = new int[3];
        void NearestDirty(int[] nearestpos)
        {
            int nearestdist = intMaxValue;
            nearestpos[0] = -1;
            nearestpos[1] = -1;
            nearestpos[2] = -1;
            int px = (int)(((int)player.playerposition.X) / chunksize);
            int py = (int)(((int)player.playerposition.Z) / chunksize);
            int pz = (int)(((int)player.playerposition.Y) / chunksize);

            int chunksxy = this.mapAreaSize / chunksize / 2;
            int chunksz = this.mapAreaSizeZ / chunksize / 2;

            int startx = px - chunksxy;
            int endx = px + chunksxy;
            int starty = py - chunksxy;
            int endy = py + chunksxy;
            int startz = pz - chunksz;
            int endz = pz + chunksz;

            if (startx < 0) { startx = 0; }
            if (starty < 0) { starty = 0; }
            if (startz < 0) { startz = 0; }
            if (endx >= mapsizexchunks) { endx = mapsizexchunks - 1; }
            if (endy >= mapsizeychunks) { endy = mapsizeychunks - 1; }
            if (endz >= mapsizezchunks) { endz = mapsizezchunks - 1; }

            for (int x = startx; x <= endx; x++)
            {
                for (int y = starty; y <= endy; y++)
                {
                    for (int z = startz; z <= endz; z++)
                    {
                        Chunk c = game.chunks[MapUtil.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)];
                        if (c == null || c.rendered == null)
                        {
                            continue;
                        }
                        if (c.rendered.dirty)
                        {
                            int dx = px - x;
                            int dy = py - y;
                            int dz = pz - z;
                            int dist = dx * dx + dy * dy + dz * dz;
                            if (dist < nearestdist)
                            {
                                nearestdist = dist;
                                nearestpos[0] = x;
                                nearestpos[1] = y;
                                nearestpos[2] = z;
                            }
                        }
                    }
                }
            }
        }

        public void DrawTerrain()
        {
            d_Batcher.Draw(LocalPlayerPosition);
        }

        public bool IsChunkRendered(int cx, int cy, int cz)
        {
            Chunk c = game.chunks[MapUtil.Index3d(cx, cy, cz, mapsizexchunks, mapsizeychunks)];
            if (c == null)
            {
                return false;
            }
            return c.rendered != null && c.rendered.ids != null;
        }

        void SetChunkDirty(int cx, int cy, int cz, bool dirty, bool blockschanged)
        {
            if (!terrainRendererStarted)
            {
                return;
            }

            if (!MapUtil.IsValidChunkPos(this, cx, cy, cz, chunksize))
            {
                return;
            }

            Chunk c = game.chunks[MapUtil.Index3d(cx, cy, cz, mapsizexchunks, mapsizeychunks)];
            if (c == null)
            {
                return;
            }
            if (c.rendered == null)
            {
                c.rendered = new RenderedChunk();
            }
            c.rendered.dirty = dirty;
            if (blockschanged)
            {
                c.rendered.shadowsdirty = true;
            }
        }

        public void RedrawBlock(int x, int y, int z)
        {
            foreach (float[] a in BlocksAround(new float[] { x, y, z }))
            {
                int xx = (int)a[0];
                int yy = (int)a[1];
                int zz = (int)a[2];
                if (xx < 0 || yy < 0 || zz < 0 || xx >= game.MapSizeX || yy >= game.MapSizeY || zz >= game.MapSizeZ)
                {
                    return;
                }
                SetChunkDirty((int)(xx / chunksize), (int)(yy / chunksize), (int)(zz / chunksize), true, true);
            }
        }

        public void RedrawAllBlocks()
        {
            for (int i = 0; i < game.chunks.Length; i++)
            {
                Chunk c = game.chunks[i];
                if (c == null)
                {
                    continue;
                }
                if (c.rendered == null)
                {
                    c.rendered = new RenderedChunk();
                }
                c.rendered.dirty = true;
            }
        }

        public int? MaybeGetLight(int x, int y, int z)
        {
            try
            {
                int l = 0;
                int cx = x / chunksize;
                int cy = y / chunksize;
                int cz = z / chunksize;
                if (IsValidChunkPosition(cx, cy, cz))
                {
                    Chunk c = game.chunks[MapUtil.Index3d(cx, cy, cz, mapsizexchunks, mapsizeychunks)];
                    if (c == null
                        || c.rendered == null
                        || c.rendered.light == null)
                    {
                        l = 0;
                    }
                    else
                    {
                        l = c.rendered.light[MapUtil.Index3d((x % chunksize) + 1, (y % chunksize) + 1, (z % chunksize) + 1, chunksize + 2, chunksize + 2)];
                    }
                }

                if (l == 0)
                {
                    if (z >= d_Heightmap.GetBlock(x, y))
                    {
                        return sunlight_;
                    }
                    else
                    {
                        return minlight;
                    }
                }
                else
                {
                    //return l - 1;
                    return l;
                }
            }
            catch
            {
                return maxlight;
            }
        }

        void RedrawChunk(int x, int y, int z)
        {
            Chunk c = game.chunks[MapUtil.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)];
            if (c == null)
            {
                return;
            }
            if (c.rendered == null)
            {
                c.rendered = new RenderedChunk();
            }
            if (c.rendered.ids != null)
            {
                foreach (int loadedSubmesh in c.rendered.ids)
                {
                    d_Batcher.Remove(loadedSubmesh);
                }
            }
            c.rendered.dirty = false;
            chunkupdates++;

            List<int> ids = new List<int>();
            GetExtendedChunk(x, y, z);
            if (!IsSolidChunk(currentChunk))
            {
                CalculateShadows(x, y, z);
                IEnumerable<VerticesIndicesToLoad> a = d_TerrainChunkTesselator.MakeChunk(x, y, z, currentChunk, currentChunkShadows, d_Data.LightLevels);
                foreach (VerticesIndicesToLoad submesh in a)
                {
                    if (submesh.indices.Length != 0)
                    {
                        float[] center = new float[] { submesh.position.X + chunksize / 2, submesh.position.Z + chunksize / 2, submesh.position.Y + chunksize / 2 };
                        Vector3 centerVec = new Vector3(center[0], center[1], center[2]);
                        float radius = 0.866025404f * chunksize;
                        ids.Add(d_Batcher.Add(submesh.indices, submesh.indicesCount, submesh.vertices, submesh.verticesCount, submesh.transparent, submesh.texture, centerVec, radius));
                    }
                }
            }
            int[] idsarr = new int[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                idsarr[i] = ids[i];
            }
            c.rendered.ids = idsarr;
        }

        bool IsSolidChunk(int[] currentChunk)
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

        int[] currentChunk = new int[18 * 18 * 18];
        byte[] currentChunkShadows = new byte[18 * 18 * 18];

        //For performance, make a local copy of chunk and its surrounding.
        //To render one chunk, we need to know all blocks that touch chunk boundaries.
        //(because to render a single block we need to know all 6 blocks around it).
        //So it's needed to copy 16x16x16 chunk and its Borders to make a 18x18x18 "extended" chunk.
        private void GetExtendedChunk(int x, int y, int z)
        {
            GetMapPortion(currentChunk, x * chunksize - 1, y * chunksize - 1, z * chunksize - 1,
                chunksize + 2, chunksize + 2, chunksize + 2);
        }

        unsafe int[][] chunks3x3x3 = null;
        unsafe int[][] heightchunks3x3 = null;
        private unsafe void CalculateShadows(int cx, int cy, int cz)
        {
            if (chunks3x3x3 == null)
            {
                chunks3x3x3 = new int[3 * 3 * 3][]; //(byte**)Marshal.AllocHGlobal(sizeof(byte*) * 3 * 3 * 3);
                for (int i = 0; i < 3 * 3 * 3; i++)
                {
                    chunks3x3x3[i] = new int[chunksize * chunksize * chunksize];
                }
                heightchunks3x3 = new int[3 * 3][];//(byte**)Marshal.AllocHGlobal(sizeof(byte*) * 3 * 3);
            }
            for (int i = 0; i < 3 * 3 * 3; i++)
            {
                int n = chunksize * chunksize * chunksize;
                int[] c = chunks3x3x3[i];
                for (int k = 0; k < n; k++)
                {
                    c[k] = 0;
                }
            }
            for (int i = 0; i < 3 * 3; i++)
            {
                heightchunks3x3[i] = null;
            }
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        if (cx + x - 1 < 0 || cx + x - 1 >= MapSizeX / chunksize
                            || cy + y - 1 < 0 || cy + y - 1 >= MapSizeY / chunksize
                            || cz + z - 1 < 0 || cz + z - 1 >= MapSizeZ / chunksize)
                        {
                            continue;
                        }
                        Chunk chunk = game.chunks[MapUtil.Index3d(cx + x - 1, cy + y - 1, cz + z - 1, MapSizeX / chunksize, MapSizeY / chunksize)];
                        if (chunk != null)
                        {
                            game.CopyChunk(chunk, chunks3x3x3[MapUtil.Index3d(x, y, z, 3, 3)]);
                        }
                        else
                        {
                            //chunks[0] = null;
                        }
                    }
                }
            }
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (cx + x - 1 < 0 || cx + x - 1 >= MapSizeX / chunksize
                        || cy + y - 1 < 0 || cy + y - 1 >= MapSizeY / chunksize)
                    {
                        continue;
                    }
                    int[] chunk = d_Heightmap.chunks[MapUtil.Index2d(cx + x - 1, cy + y - 1, d_Map.MapSizeX / chunksize)];
                    heightchunks3x3[MapUtil.Index2d(x, y, 3)] = chunk;
                }
            }

            shadows.Update(currentChunkShadows, chunks3x3x3, heightchunks3x3, d_Data.LightRadius, d_Data.IsTransparentForLight, sunlight, cz * chunksize - chunksize);

            //for MaybeGetLight
            Chunk chunkLight = game.chunks[MapUtil.Index3d(cx, cy, cz, mapsizexchunks, mapsizeychunks)];
            if (chunkLight.rendered != null)
            {
                if (chunkLight.rendered.light == null)
                {
                    chunkLight.rendered.light = new byte[(chunksize + 2) * (chunksize + 2) * (chunksize + 2)];
                }
                Array.Copy(currentChunkShadows,
                    chunkLight.rendered.light,
                    currentChunkShadows.Length);
            }
        }

        public int TrianglesCount()
        {
            return d_Batcher.TotalTriangleCount;
        }

        bool shadowssimple = false;
        int minlight = 0;
    }
}
