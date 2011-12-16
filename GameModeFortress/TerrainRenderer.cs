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

        public void Start()
        {
        }

        public void Draw()
        {
            d_Batcher.Draw(LocalPlayerPosition);

        }

        public void UpdateTile(int x, int y, int z)
        {
        }

        public void UpdateAllTiles()
        {
            List<int> ids = new List<int>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 8; z++)
                    {
                        var a = d_TerrainChunkTesselator.MakeChunk(MapSizeX / 16 / 2 + x, MapSizeY / 16 / 2 + y, z);
                        foreach (var submesh in a)
                        {
                            if (submesh.indices.Length != 0)
                            {
                                Vector3 center = new Vector3(submesh.position.X + chunksize / 2, submesh.position.Z + chunksize / 2, submesh.position.Y + chunksize / 2);
                                float radius = chunksize;
                                ids.Add(d_Batcher.Add(submesh.indices, submesh.indicesCount, submesh.vertices, submesh.verticesCount, submesh.transparent, submesh.texture, center, radius));
                            }
                        }
                    }
                }
            }
        }

        public int TrianglesCount()
        {
            return 0;
        }

        bool shadowssimple;
    }
}
