//Unload chunks currently not seen by players
public class ServerSystemUnloadUnusedChunks : ServerSystem
{
    public override void Update(Server server, float dt)
    {
        int sizex = server.mapsizexchunks();
        int sizey = server.mapsizeychunks();
        int sizez = server.mapsizezchunks();

        for (int i = 0; i < 100; i++)
        {
            var v = MapUtil.Pos(CompressUnusedIteration, server.d_Map.MapSizeX / Server.chunksize, server.d_Map.MapSizeY / Server.chunksize);
            ServerChunk c = server.d_Map.GetChunkValid(v.x, v.y, v.z);
            var vg = new Vector3i(v.x * Server.chunksize, v.y * Server.chunksize, v.z * Server.chunksize);
            bool stop = false;
            if (c != null)
            {
                bool unload = true;
                foreach (var k in server.clients)
                {
                    int viewdist = (int)(server.chunkdrawdistance * Server.chunksize * 1.5f);
                    if (server.DistanceSquared(server.PlayerBlockPosition(k.Value), vg) <= viewdist * viewdist)
                    {
                        unload = false;
                    }
                }
                if (unload)
                {
                    if (c.DirtyForSaving)
                    {
                        server.DoSaveChunk(v.x, v.y, v.z, c);
                    }
                    server.d_Map.SetChunkValid(v.x, v.y, v.z, null);
                    stop = true;
                }
            }
            CompressUnusedIteration++;
            if (CompressUnusedIteration >= sizex * sizey * sizez)
            {
                CompressUnusedIteration = 0;
            }
            if (stop)
            {
                return;
            }
        }
    }
    int CompressUnusedIteration = 0;
}
