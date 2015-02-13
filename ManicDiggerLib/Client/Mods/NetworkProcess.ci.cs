public class ModNetworkProcess : ClientMod
{
    public ModNetworkProcess()
    {
        CurrentChunk = new byte[1024 * 64];
        CurrentChunkCount = 0;
        receivedchunk = new int[32 * 32 * 32];
        decompressedchunk = new byte[32 * 32 * 32 * 2];
    }
    internal byte[] CurrentChunk;
    internal int CurrentChunkCount;
    int[] receivedchunk;
    byte[] decompressedchunk;

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    Game game;
    public override void OnReadOnlyBackgroundThread(Game game_, float dt)
    {
        game = game_;
        NetworkProcess();
    }

    public void NetworkProcess()
    {
        game.currentTimeMilliseconds = game.platform.TimeMillisecondsFromStart();
        if (game.main == null)
        {
            return;
        }
        NetIncomingMessage msg;
        for (; ; )
        {
            if (game.invalidVersionPacketIdentification != null)
            {
                break;
            }
            msg = game.main.ReadMessage();
            if (msg == null)
            {
                break;
            }
            TryReadPacket(msg.message, msg.messageLength);
        }
    }

    public void TryReadPacket(byte[] data, int dataLength)
    {
        Packet_Server packet = new Packet_Server();
        Packet_ServerSerializer.DeserializeBuffer(data, dataLength, packet);

        ProcessInBackground(packet);

        ProcessPacketTask task = new ProcessPacketTask();
        task.game = game;
        task.packet = packet;
        game.QueueActionCommit(task);

        game.LastReceivedMilliseconds = game.currentTimeMilliseconds;
        //return lengthPrefixLength + packetLength;
    }

    void ProcessInBackground(Packet_Server packet)
    {
        switch (packet.Id)
        {
            case Packet_ServerIdEnum.ChunkPart:
                byte[] arr = packet.ChunkPart.CompressedChunkPart;
                int arrLength = game.platform.ByteArrayLength(arr); // todo
                for (int i = 0; i < arrLength; i++)
                {
                    CurrentChunk[CurrentChunkCount++] = arr[i];
                }
                break;
            case Packet_ServerIdEnum.Chunk_:
                {
                    Packet_ServerChunk p = packet.Chunk_;
                    if (CurrentChunkCount != 0)
                    {
                        game.platform.GzipDecompress(CurrentChunk, CurrentChunkCount, decompressedchunk);
                        {
                            int i = 0;
                            for (int zz = 0; zz < p.SizeZ; zz++)
                            {
                                for (int yy = 0; yy < p.SizeY; yy++)
                                {
                                    for (int xx = 0; xx < p.SizeX; xx++)
                                    {
                                        receivedchunk[Index3d(xx, yy, zz, p.SizeX, p.SizeY)] = (decompressedchunk[i + 1] << 8) + decompressedchunk[i];
                                        i += 2;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int size = p.SizeX * p.SizeY * p.SizeZ;
                        for (int i = 0; i < size; i++)
                        {
                            receivedchunk[i] = 0;
                        }
                    }
                    {
                        game.map.SetMapPortion(p.X, p.Y, p.Z, receivedchunk, p.SizeX, p.SizeY, p.SizeZ);
                        for (int xx = 0; xx < 2; xx++)
                        {
                            for (int yy = 0; yy < 2; yy++)
                            {
                                for (int zz = 0; zz < 2; zz++)
                                {
                                    //d_Shadows.OnSetChunk(p.X + 16 * xx, p.Y + 16 * yy, p.Z + 16 * zz);//todo
                                }
                            }
                        }
                    }
                    game.ReceivedMapLength += CurrentChunkCount;// lengthPrefixLength + packetLength;
                    CurrentChunkCount = 0;
                }
                break;
            case Packet_ServerIdEnum.HeightmapChunk:
                {
                    Packet_ServerHeightmapChunk p = packet.HeightmapChunk;
                    game.platform.GzipDecompress(p.CompressedHeightmap, game.platform.ByteArrayLength(p.CompressedHeightmap), decompressedchunk);
                    int[] decompressedchunk1 = Game.ByteArrayToUshortArray(decompressedchunk, p.SizeX * p.SizeY * 2);
                    for (int xx = 0; xx < p.SizeX; xx++)
                    {
                        for (int yy = 0; yy < p.SizeY; yy++)
                        {
                            int height = decompressedchunk1[MapUtilCi.Index2d(xx, yy, p.SizeX)];
                            game.d_Heightmap.SetBlock(p.X + xx, p.Y + yy, height);
                        }
                    }
                }
                break;
        }
    }
}
