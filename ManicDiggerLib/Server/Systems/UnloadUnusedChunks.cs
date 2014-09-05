//Unload chunks currently not seen by players
public class ServerSystemUnloadUnusedChunks : ServerSystem
{
    public ServerSystemUnloadUnusedChunks()
    {
        v = new Vector3IntRef();
    }
    Vector3IntRef v;
    public override void Update(Server server, float dt)
    {
        int sizexchunks = server.mapsizexchunks();
        int sizeychunks = server.mapsizeychunks();
        int sizezchunks = server.mapsizezchunks();

        for (int i = 0; i < 100; i++)
        {
            MapUtilCi.PosInt(CompressUnusedIteration, sizexchunks, sizeychunks, v);
            ServerChunk c = server.d_Map.GetChunkValid(v.X, v.Y, v.Z);
            bool stop = false;
            if (c != null)
            {
                var vg = new Vector3i(v.X * Server.chunksize, v.Y * Server.chunksize, v.Z * Server.chunksize);
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
                        server.DoSaveChunk(v.X, v.Y, v.Z, c);
                    }
                    server.d_Map.SetChunkValid(v.X, v.Y, v.Z, null);
                    stop = true;
                }
            }
            CompressUnusedIteration++;
            if (CompressUnusedIteration >= sizexchunks * sizeychunks * sizezchunks)
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
