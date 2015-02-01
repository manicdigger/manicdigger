//Unload chunks currently not seen by players
public class ServerSystemUnloadUnusedChunks : ServerSystem
{
    public ServerSystemUnloadUnusedChunks()
    {
        chunkpos = new Vector3IntRef();
    }
    Vector3IntRef chunkpos;
    public override void Update(Server server, float dt)
    {
        int sizexchunks = server.mapsizexchunks();
        int sizeychunks = server.mapsizeychunks();
        int sizezchunks = server.mapsizezchunks();

        for (int i = 0; i < 100; i++)
        {
            MapUtilCi.PosInt(CompressUnusedIteration, sizexchunks, sizeychunks, chunkpos);
            ServerChunk c = server.d_Map.GetChunkValid(chunkpos.X, chunkpos.Y, chunkpos.Z);
            bool stop = false;
            if (c != null)
            {
                var globalpos = new Vector3i(chunkpos.X * Server.chunksize, chunkpos.Y * Server.chunksize, chunkpos.Z * Server.chunksize);
                bool unload = true;
                foreach (var k in server.clients)
                {
                    if (k.Value.IsBot)
                    {
                        // don't hold chunks in memory for bots
                        continue;
                    }
                    // unload distance = view distance + 60% (prevents chunks from being unloaded too early (square loading vs. circular unloading))
                    int viewdist = (int)(server.chunkdrawdistance * Server.chunksize * 1.8f);
                    if (server.DistanceSquared(server.PlayerBlockPosition(k.Value), globalpos) <= viewdist * viewdist)
                    {
                        //System.Console.WriteLine("No Unload:   {0},{1},{2}", chunkpos.X, chunkpos.Y, chunkpos.Z);
                        unload = false;
                    }
                }
                if (unload)
                {
                    // unload if chunk isn't seen by anyone
                    if (c.DirtyForSaving)
                    {
                        // save changes to disk if necessary
                        server.DoSaveChunk(chunkpos.X, chunkpos.Y, chunkpos.Z, c);
                    }
                    server.d_Map.SetChunkValid(chunkpos.X, chunkpos.Y, chunkpos.Z, null);
                    foreach (var client in server.clients)
                    {
                        // mark chunks unseen for all players
                        server.ClientSeenChunkRemove(client.Key, chunkpos.X, chunkpos.Y, chunkpos.Z);
                    }
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
                // only unload one chunk at a time
                return;
            }
        }
    }
    int CompressUnusedIteration = 0;
}
