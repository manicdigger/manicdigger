using System;
using System.Collections.Generic;
using System.Text;

namespace GameModeFortress
{
    public class FortressModeServerFactory
    {
        public static Server create(Boolean singleplayer)
        {
            Server s = new Server();

            var map = new GameModeFortress.InfiniteMapChunked();
            map.chunksize = 32;
            var generator = new WorldGenerator();
            map.generator = generator;
            s.chunksize = 32;
            map.Reset(10000, 10000, 128);
            s.map = map;
            s.generator = generator;
            s.data = new GameDataTilesManicDigger();
            s.craftingtabletool = new CraftingTableTool() { map = map };
            s.LocalConnectionsOnly = singleplayer;

            return s;
        }
    }
}
