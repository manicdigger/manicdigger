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

        int mapAreaSize { get { return (int)d_Config3d.viewdistance * 2; } }
        int centerAreaSize { get { return (int)d_Config3d.viewdistance / 2; } }
        int mapAreaSizeZ { get { return MapSizeZ; } }

        public void StartTerrain()
        {
            //Toggle fog.
            var p = MapUtil.PlayerArea(mapAreaSize, centerAreaSize, PlayerBlockPosition());
            CurrentRendererMapPositionG = new Vector3i(p.X, p.Y, 0);
            if (RendererMap != null)
            {
                d_Batcher.Clear();
                //throw new NotImplementedException();
            }
            RendererMap = new RenderedChunk[(mapAreaSize / chunksize) * (mapAreaSize / chunksize) * (MapSizeZ / chunksize)];
            for (int i = 0; i < RendererMap.Length; i++)
            {
                RendererMap[i] = new RenderedChunk();
            }
            d_TerrainChunkTesselator.Start();
            shadows = new Shadows3x3x3();
            shadows.Start();
        }
        IShadows3x3x3 shadows;

        public void DrawTerrain()
        {
            d_Batcher.Draw(LocalPlayerPosition);
        }

        public void TryRemap()
        {
            var p = MapUtil.PlayerArea(mapAreaSize, centerAreaSize, PlayerBlockPosition());
            var newMapPosition = new Vector3i(p.X, p.Y, 0);
            //if (PlayerBlockPosition() != oldplayerpos)
            if (CurrentRendererMapPositionG != newMapPosition)
            {
                //todo: check if complete terrain in new area is already downloaded.
                Remap(newMapPosition);
                CurrentRendererMapPositionG = newMapPosition;
            }
        }

        //todo: use this for chunk byte[], not just for terrain renderer meshes.
        public void Remap(Vector3i newMapPosition)
        {
            //make a list of old chunks
            var newRendererMap = new RenderedChunk[RendererMap.Length];
            Dictionary<Vector3i, RenderedChunk> oldChunks = new Dictionary<Vector3i, RenderedChunk>();
            for (int x = 0; x < mapAreaSize / chunksize; x++)
            {
                for (int y = 0; y < mapAreaSize / chunksize; y++)
                {
                    for (int z = 0; z < MapSizeZ / chunksize; z++)
                    {
                        int pos = MapUtil.Index3d(x, y, z, mapAreaSize / chunksize, mapAreaSize / chunksize);
                        int chunkx = x + CurrentRendererMapPositionG.x / chunksize;
                        int chunky = y + CurrentRendererMapPositionG.y / chunksize;
                        int chunkz = z + CurrentRendererMapPositionG.z / chunksize;
                        Vector3i pos2 = new Vector3i(chunkx, chunky, chunkz);
                        oldChunks[pos2] = RendererMap[pos];
                    }
                }
            }
            for (int x = 0; x < mapAreaSize / chunksize; x++)
            {
                for (int y = 0; y < mapAreaSize / chunksize; y++)
                {
                    for (int z = 0; z < MapSizeZ / chunksize; z++)
                    {
                        int pos = MapUtil.Index3d(x, y, z, mapAreaSize / chunksize, mapAreaSize / chunksize);
                        int newchunkx = x + newMapPosition.x / chunksize;
                        int newchunky = y + newMapPosition.y / chunksize;
                        int newchunkz = z + newMapPosition.z / chunksize;
                        Vector3i pos2 = new Vector3i(newchunkx, newchunky, newchunkz);
                        if (oldChunks.ContainsKey(pos2))
                        {
                            //if already loaded
                            newRendererMap[pos] = oldChunks[pos2];
                            oldChunks[pos2] = null;
                        }
                        else
                        {
                            //if needs loading
                            newRendererMap[pos] = new RenderedChunk();
                        }
                    }
                }
            }
            foreach (var k in oldChunks)
            {
                //wasn't used in new area.
                if (k.Value != null && k.Value.ids != null)
                {
                    foreach (var subMeshId in k.Value.ids)
                    {
                        d_Batcher.Remove(subMeshId);
                    }
                }
                //todo: save to disk
            }
            for (int i = 0; i < newRendererMap.Length; i++)
            {
                RendererMap[i] = newRendererMap[i];
            }
        }

        public void UpdateTerrain()
        {
            if (RendererMap == null)
            {
                //Start() not called yet.
                return;
            }

            TryRemap();

            if (lastplacedblock != null)
            {
                Dictionary<Vector3i, bool> ChunksToRedraw = new Dictionary<Vector3i,bool>();
                foreach (var a in MapUtil.BlocksAround(new Vector3(lastplacedblock.Value.x,lastplacedblock.Value.y,lastplacedblock.Value.z)))
                {
                    ChunksToRedraw[new Vector3i((int)a.X / chunksize, (int)a.Y / chunksize, (int)a.Z / chunksize)] = true;
                }
                foreach (var c in ChunksToRedraw.Keys)
                {
                    int xx = c.x - (CurrentRendererMapPositionG.x / chunksize);
                    int yy = c.y - (CurrentRendererMapPositionG.y / chunksize);
                    int zz = c.z - (CurrentRendererMapPositionG.z / chunksize);
                    if (xx >= 0 && yy >= 0 && zz >= 0
                        && xx < mapAreaSize / chunksize && yy < mapAreaSize / chunksize && zz < mapAreaSizeZ / chunksize)
                    {
                        int pos1 = MapUtil.Index3d(xx, yy, zz, mapAreaSize / chunksize, mapAreaSize / chunksize);
                        if (RendererMap[pos1].dirty)
                        {
                            RedrawChunk(xx, yy, zz);
                        }
                    }
                }
                lastplacedblock = null;
            }

            for (; ; )
            {
                Vector3i? v = NearestDirty();
                if (v == null)
                {
                    break;
                }
                RedrawChunk(v.Value.x, v.Value.y, v.Value.z);
                if (framestopwatch.ElapsedMilliseconds > 5)
                {
                    break;
                }
            }
        }
        Vector3i? NearestDirty()
        {
            int nearestdist = int.MaxValue;
            Vector3i? nearestpos = null;
            int px = ((int)player.playerposition.X - CurrentRendererMapPositionG.x) / chunksize;
            int py = ((int)player.playerposition.Z - CurrentRendererMapPositionG.y) / chunksize;
            int pz = ((int)player.playerposition.Y - CurrentRendererMapPositionG.z) / chunksize;
            for (int x = 0; x < mapAreaSize / chunksize; x++)
            {
                for (int y = 0; y < mapAreaSize / chunksize; y++)
                {
                    for (int z = 0; z < MapSizeZ / chunksize; z++)
                    {
                        int pos = MapUtil.Index3d(x, y, z, mapAreaSize / chunksize, mapAreaSize / chunksize);
                        if (RendererMap[pos].dirty)
                        {
                            int dx = px - x;
                            int dy = py - y;
                            int dz = pz - z;
                            int dist = dx * dx + dy * dy + dz * dz;
                            if (dist < nearestdist)
                            {
                                nearestdist = dist;
                                nearestpos = new Vector3i(x, y, z);
                            }
                        }
                    }
                }
            }
            return nearestpos;
        }

        void RedrawChunk(int x, int y, int z)
        {
            int pos = MapUtil.Index3d(x, y, z, mapAreaSize / chunksize, mapAreaSize / chunksize);
            if (RendererMap[pos].ids != null)
            {
                foreach (int loadedSubmesh in RendererMap[pos].ids)
                {
                    d_Batcher.Remove(loadedSubmesh);
                }
            }
            RendererMap[pos].dirty = false;
            chunkupdates++;

            List<int> ids = new List<int>();
            var a = d_TerrainChunkTesselator.MakeChunk(CurrentRendererMapPositionG.x / chunksize + x,
                CurrentRendererMapPositionG.y / chunksize + y, z);
            foreach (var submesh in a)
            {
                if (submesh.indices.Length != 0)
                {
                    Vector3 center = new Vector3(submesh.position.X + chunksize / 2, submesh.position.Z + chunksize / 2, submesh.position.Y + chunksize / 2);
                    float radius = chunksize;
                    ids.Add(d_Batcher.Add(submesh.indices, submesh.indicesCount, submesh.vertices, submesh.verticesCount, submesh.transparent, submesh.texture, center, radius));
                }
            }
            RendererMap[pos].ids = ids.ToArray();
            //if ((updated++) > 2 && framestopwatch.ElapsedMilliseconds > 5)
        }

        public void OnMakeChunk(int chunkx, int chunky, int chunkz)
        {
            CalculateShadows(chunkx, chunky, chunkz);
        }

        unsafe byte[][] chunks3x3x3 = null;
        unsafe byte[][] heightchunks3x3 = null;
        private unsafe void CalculateShadows(int cx,int cy,int cz)
        {
            if (chunks3x3x3 == null)
            {
                chunks3x3x3 = new byte[3*3*3][]; //(byte**)Marshal.AllocHGlobal(sizeof(byte*) * 3 * 3 * 3);
                heightchunks3x3 = new byte[3*3][];//(byte**)Marshal.AllocHGlobal(sizeof(byte*) * 3 * 3);
            }
            for (int i = 0; i < 3 * 3 * 3; i++)
            {
                chunks3x3x3[i] = null;
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
                        Chunk chunk = chunks[MapUtil.Index3d(cx + x-1, cy + y-1, cz + z-1, MapSizeX / chunksize, MapSizeY / chunksize)];
                        if (chunk != null)
                        {
                            chunks3x3x3[MapUtil.Index3d(x, y, z, 3, 3)] = chunk.data;
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
                    byte[] chunk = d_Heightmap.chunks[MapUtil.Index2d(cx + x-1, cy + y-1, d_Map.MapSizeX / chunksize)];
                    heightchunks3x3[MapUtil.Index2d(x, y, 3)] = chunk;
                }
            }

            shadows.Update(d_TerrainChunkTesselator.currentChunkShadows, chunks3x3x3, heightchunks3x3, d_Data.LightRadius, d_Data.IsTransparentForLight, sunlight, cz * chunksize - chunksize);
            
            //for MaybeGetLight
            Array.Copy(d_TerrainChunkTesselator.currentChunkShadows,
                RendererMap[MapUtil.Index3d(cx - CurrentRendererMapPositionG.x / chunksize,
                cy - CurrentRendererMapPositionG.y / chunksize,
                cz - CurrentRendererMapPositionG.z / chunksize, mapAreaSize / chunksize, mapAreaSize / chunksize)].light,
                d_TerrainChunkTesselator.currentChunkShadows.Length);
        }
        public static bool aaa;
        public static int bbb;
        public void RedrawBlock(int x, int y, int z)
        {
            foreach (var a in MapUtil.BlocksAround(new Vector3(x, y, z)))
            {
                int xx = (int)a.X - CurrentRendererMapPositionG.x;
                int yy = (int)a.Y - CurrentRendererMapPositionG.y;
                int zz = (int)a.Z - CurrentRendererMapPositionG.z;
                if (xx < 0 || yy < 0 || zz < 0 || xx >= mapAreaSize || yy >= mapAreaSize || zz >= MapSizeZ)
                {
                    return;
                }
                RendererMap[MapUtil.Index3d(xx / chunksize, yy / chunksize, zz / chunksize, mapAreaSize / chunksize, mapAreaSize / chunksize)].dirty = true;
            }
        }

        void SetChunkDirty(int cx, int cy, int cz, bool dirty)
        {
            if (RendererMap == null)
            {
                return;
            }

            int x = cx * chunksize;
            int y = cy * chunksize;
            int z = cz * chunksize;
            if (x >= CurrentRendererMapPositionG.x
                && y >= CurrentRendererMapPositionG.y
                && z >= CurrentRendererMapPositionG.z
                && x < CurrentRendererMapPositionG.x + mapAreaSize
                && y < CurrentRendererMapPositionG.y + mapAreaSize
                && z < MapSizeZ)
            {
                int xx = x - CurrentRendererMapPositionG.x;
                int yy = y - CurrentRendererMapPositionG.y;
                int zz = z - CurrentRendererMapPositionG.z;
                RendererMap[MapUtil.Index3d(xx / chunksize, yy / chunksize, zz / chunksize, mapAreaSize / chunksize, mapAreaSize / chunksize)].dirty = dirty;
            }
        }

        public class RenderedChunk
        {
            public int[] ids;
            public bool dirty = true;
            public byte[] light = new byte[(16 + 2) * (16 + 2) * (16 + 2)];
        }

        RenderedChunk[] RendererMap;
        Vector3i CurrentRendererMapPositionG;

        public void RedrawAllBlocks()
        {
            if (RendererMap == null)
            {
                return;
            }
            for (int i = 0; i < RendererMap.Length; i++)
            {
                RendererMap[i].dirty = true;
            }
        }

        Vector3i PlayerBlockPosition()
        {
            return new Vector3i((int)LocalPlayerPosition.X, (int)LocalPlayerPosition.Z, (int)LocalPlayerPosition.Y);
        }

        public int TrianglesCount()
        {
            return d_Batcher.TotalTriangleCount;
        }

        bool shadowssimple = false;
        int minlight = 0;
        public int? MaybeGetLight(int x, int y, int z)
        {
            int xx = x - CurrentRendererMapPositionG.x;
            int yy = y - CurrentRendererMapPositionG.y;
            int zz = z - CurrentRendererMapPositionG.z;
            int l;
            if (xx < 0 || yy < 0 || zz < 0 || xx >= mapAreaSize || yy >= mapAreaSize || zz >= MapSizeZ)
            {
                l = 0;
            }
            else
            {
                // returns 0 when unknown
                byte[] light = RendererMap[MapUtil.Index3d(xx / chunksize, yy / chunksize, zz / chunksize, mapAreaSize / chunksize, mapAreaSize / chunksize)].light;
                l = light[MapUtil.Index3d((x % chunksize) + 1, (y % chunksize) + 1, (z % chunksize) + 1, chunksize + 2, chunksize + 2)];
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

        public int maxlight
        {
            get { return 15; }
        }

        public bool ShadowsFull
        {
            get
            {
                return false;
            }
            set
            {
            }
        }
    }
}
