using System.Collections.Generic;

public class ServerSystemChunksSimulation : ServerSystem
{
    public override void Update(Server server, float dt)
    {
        //Update all loaded chunks
        for (int i = 0; i < ChunksSimulated; i++)
        {
            ChunkSimulation(server);
        }
    }

    int ChunksSimulated = 1;
    int chunksimulation_every(Server server) { return (int)(1 / server.SIMULATION_STEP_LENGTH) * 60 * 10; }//10 minutes

    void ChunkSimulation(Server server)
    {
        foreach (var k in server.clients)
        {
            var pos = server.PlayerBlockPosition(k.Value);

            long oldesttime = long.MaxValue;
            Vector3i oldestpos = new Vector3i();

            foreach (var p in ChunksAroundPlayer(server, pos))
            {
                if (!MapUtil.IsValidPos(server.d_Map, p.x, p.y, p.z)) { continue; }
                ServerChunk c = server.d_Map.GetChunkValid(p.x / Server.chunksize, p.y / Server.chunksize, p.z / Server.chunksize);
                if (c == null) { continue; }
                if (c.data == null) { continue; }
                if (c.LastUpdate > server.simulationcurrentframe) { c.LastUpdate = server.simulationcurrentframe; }
                if (c.LastUpdate < oldesttime)
                {
                    oldesttime = c.LastUpdate;
                    oldestpos = p;
                }
                if (!c.IsPopulated)
                {
                    PopulateChunk(server, p);
                    c.IsPopulated = true;
                }
            }
            if (server.simulationcurrentframe - oldesttime > chunksimulation_every(server))
            {
                ChunkUpdate(server, oldestpos, oldesttime);
                ServerChunk c = server.d_Map.GetChunkValid(oldestpos.x / Server.chunksize, oldestpos.y / Server.chunksize, oldestpos.z / Server.chunksize);
                c.LastUpdate = (int)server.simulationcurrentframe;
                return;
            }
        }
    }

    void PopulateChunk(Server server, Vector3i p)
    {
        for (int i = 0; i < server.modEventHandlers.populatechunk.Count; i++)
        {
            server.modEventHandlers.populatechunk[i](p.x / Server.chunksize, p.y / Server.chunksize, p.z / Server.chunksize);
        }
        //d_Generator.PopulateChunk(d_Map, p.x / chunksize, p.y / chunksize, p.z / chunksize);
    }

    void ChunkUpdate(Server server, Vector3i p, long lastupdate)
    {
        if (server.config.Monsters)
        {
            AddMonsters(server, p);
        }
        ServerChunk chunk = server.d_Map.GetChunk(p.x, p.y, p.z);
        for (int xx = 0; xx < Server.chunksize; xx++)
        {
            for (int yy = 0; yy < Server.chunksize; yy++)
            {
                for (int zz = 0; zz < Server.chunksize; zz++)
                {
                    int block = chunk.data[MapUtilCi.Index3d(xx, yy, zz, Server.chunksize, Server.chunksize)];

                    for (int i = 0; i < server.modEventHandlers.blockticks.Count; i++)
                    {
                        server.modEventHandlers.blockticks[i](p.x + xx, p.y + yy, p.z + zz);
                    }
                }
            }
        }
    }
    IEnumerable<Vector3i> ChunksAroundPlayer(Server server, Vector3i playerpos)
    {
        playerpos.x = (playerpos.x / Server.chunksize) * Server.chunksize;
        playerpos.y = (playerpos.y / Server.chunksize) * Server.chunksize;
        for (int x = -server.chunkdrawdistance; x <= server.chunkdrawdistance; x++)
        {
            for (int y = -server.chunkdrawdistance; y <= server.chunkdrawdistance; y++)
            {
                for (int z = 0; z < server.d_Map.MapSizeZ / Server.chunksize; z++)
                {
                    var p = new Vector3i(playerpos.x + x * Server.chunksize, playerpos.y + y * Server.chunksize, z * Server.chunksize);
                    if (MapUtil.IsValidPos(server.d_Map, p.x, p.y, p.z))
                    {
                        yield return p;
                    }
                }
            }
        }
    }

    public int[] MonsterTypesUnderground = new int[] { 1, 2 };
    public int[] MonsterTypesOnGround = new int[] { 0, 3, 4 };

    public void AddMonsters(Server server, Vector3i p)
    {
        ServerChunk chunk = server.d_Map.GetChunkValid(p.x / Server.chunksize, p.y / Server.chunksize, p.z / Server.chunksize);
        int tries = 0;
        while (chunk.Monsters.Count < 1)
        {
            int xx = server.rnd.Next(Server.chunksize);
            int yy = server.rnd.Next(Server.chunksize);
            int zz = server.rnd.Next(Server.chunksize);
            int px = p.x + xx;
            int py = p.y + yy;
            int pz = p.z + zz;
            if ((!MapUtil.IsValidPos(server.d_Map, px, py, pz))
                || (!MapUtil.IsValidPos(server.d_Map, px, py, pz + 1))
                || (!MapUtil.IsValidPos(server.d_Map, px, py, pz - 1)))
            {
                continue;
            }
            int type;
            int height = MapUtil.blockheight(server.d_Map, 0, px, py);
            if (pz >= height)
            {
                type = MonsterTypesOnGround[server.rnd.Next(MonsterTypesOnGround.Length)];
            }
            else
            {
                type = MonsterTypesUnderground[server.rnd.Next(MonsterTypesUnderground.Length)];
            }
            if (server.d_Map.GetBlock(px, py, pz) == 0
                && server.d_Map.GetBlock(px, py, pz + 1) == 0
                && server.d_Map.GetBlock(px, py, pz - 1) != 0
                && (!server.BlockTypes[server.d_Map.GetBlock(px, py, pz - 1)].IsFluid()))
            {
                chunk.Monsters.Add(new Monster() { X = px, Y = py, Z = pz, Id = NewMonsterId(server), Health = 20, MonsterType = type });
            }
            if (tries++ > 500)
            {
                break;
            }
        }
    }
    public int NewMonsterId(Server server)
    {
        return server.LastMonsterId++;
    }
}
