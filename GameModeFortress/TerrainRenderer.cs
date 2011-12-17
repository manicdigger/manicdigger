using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

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

        public void UpdateTerrain()
        {
            if (RendererMap == null)
            {
                //Start() not called yet.
                return;
            }
            //todo: Call Remap() if needed.
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
                        }
                    }
                }
            }
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

        public class RenderedChunk
        {
            public int[] ids;
            public bool dirty = true;
        }

        RenderedChunk[] RendererMap;
        Vector3i CurrentRendererMapPositionG;

        public void RedrawAllBlocks()
        {
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
