using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger.Mods
{
    public class FlatWorldGenerator : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("Default");
        }
        public void Start(ModManager m)
        {
            this.m = m;
            m.RegisterWorldGenerator(GetChunk);
        }
        ModManager m;
        void GetChunk(int x, int y, int z, byte[] chunk)
        {
            x *= m.GetChunkSize();
            y *= m.GetChunkSize();
            z *= m.GetChunkSize();
            int mapsizez = m.GetMapSizeZ();
            int chunksize = m.GetChunkSize();
            int adminium = m.GetBlockId("Adminium");
            int stone = m.GetBlockId("Stone");
            int dirt = m.GetBlockId("Dirt");
            int grass = m.GetBlockId("Grass");
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        int block;
                        int globalz = z + zz;
                        if (globalz == 0)
                        {
                            block = adminium;
                        }
                        else if (globalz < mapsizez / 2 - 5)
                        {
                            block = stone;
                        }
                        else if (globalz < mapsizez / 2)
                        {
                            block = dirt;
                        }
                        else if (globalz == mapsizez / 2)
                        {
                            block = grass;
                        }
                        else
                        {
                            block = 0;
                        }
                        int pos = ModManager.Index3d(xx, yy, zz, chunksize, chunksize);
                        chunk[pos] = (byte)block;
                    }
                }
            }
        }
    }
}
