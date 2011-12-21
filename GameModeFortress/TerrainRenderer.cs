using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Diagnostics;

namespace ManicDigger
{
    public partial class ManicDiggerGameWindow
    {
        int chunkupdates;
        public int ChunkUpdates { get { return chunkupdates; } }

        int mapAreaSize { get { return (int)d_Config3d.viewdistance; } }
        int centerAreaSize { get { return (int)d_Config3d.viewdistance / 2; } }

        public void StartTerrain()
        {
            //Toggle fog.
            var p = MapUtil.PlayerArea(mapAreaSize, centerAreaSize, PlayerBlockPosition());
            CurrentRendererMapPositionG = new Vector3i(p.X, p.Y, 0);
            if (RendererMap != null)
            {
                throw new NotImplementedException();
            }
            RendererMap = new RenderedChunk[(mapAreaSize / chunksize) * (mapAreaSize / chunksize) * (MapSizeZ / chunksize)];
            for (int i = 0; i < RendererMap.Length; i++)
            {
                RendererMap[i] = new RenderedChunk();
            }
        }

        public void DrawTerrain()
        {
            d_Batcher.Draw(LocalPlayerPosition);
        }

        public void TryRemap()
        {
            var p = MapUtil.PlayerArea(mapAreaSize, centerAreaSize, PlayerBlockPosition());
            var newMapPosition = new Vector3i(p.X, p.Y, 0);
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            TryRemap();

            for (int x = 0; x < mapAreaSize / chunksize; x++)
            {
                for (int y = 0; y < mapAreaSize / chunksize; y++)
                {
                    for (int z = 0; z < MapSizeZ / chunksize; z++)
                    {
                        int pos = MapUtil.Index3d(x, y, z, mapAreaSize / chunksize, mapAreaSize / chunksize);
                        if (RendererMap[pos].dirty)
                        {
                            if (RendererMap[pos].ids != null)
                            {
                                foreach (int loadedSubmesh in RendererMap[pos].ids)
                                {
                                    d_Batcher.Remove(loadedSubmesh);
                                }
                            }
                            RendererMap[pos].dirty = false;
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
                            if (stopwatch.ElapsedMilliseconds > 2)
                            {
                                goto exit;
                            }
                        }
                    }
                }
            }
        exit:
            ;
        }

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

        bool shadowssimple;
    }
}
