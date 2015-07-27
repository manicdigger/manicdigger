// Floods light inside a chunk
public class LightFlood
{
    public LightFlood()
    {
        q = new FastQueueInt();
        q.Initialize(1024);
    }
    
    FastQueueInt q;

    const int minlight = 0;

    public const int XPlus = 1;
    public const int XMinus = -1;
    public const int YPlus = 16;
    public const int YMinus = -16;
    public const int ZPlus = 256;
    public const int ZMinus = -256;

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    public void FloodLight(int[] chunk, byte[] light, int startx, int starty, int startz, int[] dataLightRadius, bool[] dataTransparent)
    {
        int start = Index3d(startx, starty, startz, 16, 16);
        if (light[start] == minlight)
        {
            return;
        }

        q.Clear();
        q.Push(start);
        for (; ; )
        {
            if (q.Count == 0)
            {
                break;
            }
            int vPos = q.Pop();
            int vLight = light[vPos];
            if (vLight == minlight)
            {
                continue;
            }
            int vBlock = chunk[vPos];
            if (!dataTransparent[vBlock]
                && dataLightRadius[vBlock] == 0)
            {
                continue;
            }
            int x = MapUtilCi.PosX(vPos, 16, 16);
            int y = MapUtilCi.PosY(vPos, 16, 16);
            int z = MapUtilCi.PosZ(vPos, 16, 16);
            if (x < 15) { Push(q, light, vLight, vPos + XPlus); }
            if (x > 0) { Push(q, light, vLight, vPos + XMinus); }
            if (y < 15) { Push(q, light, vLight, vPos + YPlus); }
            if (y > 0) { Push(q, light, vLight, vPos + YMinus); }
            if (z < 15) { Push(q, light, vLight, vPos + ZPlus); }
            if (z > 0) { Push(q, light, vLight, vPos + ZMinus); }
        }
    }

    static void Push(FastQueueInt q_, byte[] light, int vLight, int newPos)
    {
        if (light[newPos] < vLight - 1)
        {
            light[newPos] = Game.IntToByte(vLight - 1);
            q_.Push(newPos);
        }
    }
}

// Calculates light in a single 16x16x16 chunk ("base light").
// Does not spread light between chunks.
public class LightBase
{
    public LightBase()
    {
        flood = new LightFlood();
        workData = new int[16 * 16 * 16];
    }
    LightFlood flood;
    int[] workData;

    public void CalculateChunkBaseLight(Game game, int cx, int cy, int cz, int[] dataLightRadius, bool[] transparentForLight)
    {
        Chunk chunk = game.map.GetChunk_(cx, cy, cz);

        if (chunk.data != null)
        {
            for (int i = 0; i < 16 * 16 * 16; i++)
            {
                workData[i] = chunk.data[i];
            }
        }
        if (chunk.dataInt != null)
        {
            for (int i = 0; i < 16 * 16 * 16; i++)
            {
                workData[i] = chunk.dataInt[i];
            }
        }

        int sunlight = game.sunlight_;
        byte[] workLight = chunk.baseLight;
        for (int i = 0; i < 16 * 16 * 16; i++)
        {
            workLight[i] = 0;
        }

        Sunlight(game, cx, cy, cz, workLight, dataLightRadius, sunlight);
        SunlightFlood(workData, workLight, dataLightRadius, transparentForLight);
        LightEmitting(workData, workLight, dataLightRadius, transparentForLight);
    }

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif
    
    static void Sunlight(Game game, int cx, int cy, int cz, byte[] worklight, int[] dataLightRadius, int sunlight)
    {
        int baseheight = cz * Game.chunksize;
        for (int xx = 0; xx < 16; xx++)
        {
            for (int yy = 0; yy < 16; yy++)
            {
                int height = GetLightHeight(game, cx, cy, xx, yy);
                int h = height - baseheight;
                if (h < 0) { h = 0; }
                if (h > 16) { continue; }
                int pos = Index3d(xx, yy, h, 16, 16);

                for (int zz = h; zz < 16; zz++)
                {
                    worklight[pos] = Game.IntToByte(sunlight);
                    pos += LightFlood.ZPlus;
                }
            }
        }
    }
    
    static int GetLightHeight(Game game, int cx, int cy, int xx, int yy)
    {
        int[] chunk = game.d_Heightmap.chunks[MapUtilCi.Index2d(cx, cy, game.map.MapSizeX / Game.chunksize)];
        if (chunk == null)
        {
            return 0;
        }
        return chunk[MapUtilCi.Index2d(xx % Game.chunksize, yy % Game.chunksize, Game.chunksize)];
    }
    
    void SunlightFlood(int[] workportion, byte[] worklight, int[] dataLightRadius, bool[] dataTransparent)
    {
        for (int xx = 0; xx < 16; xx++)
        {
            for (int yy = 0; yy < 16; yy++)
            {
                for (int zz = 0; zz < 16; zz++)
                {
                    int pos = Index3d(xx, yy, zz, 16, 16);
                    if (!dataTransparent[workportion[pos]])
                    {
                        continue;
                    }
                    int curlight = worklight[pos];
                    int posXPlus1 = pos + LightFlood.XPlus;
                    int posYPlus1 = pos + LightFlood.YPlus;
                    if (xx + 1 < 16
                        && worklight[posXPlus1] != curlight
                        && dataTransparent[workportion[posXPlus1]])
                    {
                        flood.FloodLight(workportion, worklight, xx, yy, zz, dataLightRadius, dataTransparent);
                        flood.FloodLight(workportion, worklight, xx + 1, yy, zz, dataLightRadius, dataTransparent);
                    }
                    if (yy + 1 < 16
                        && worklight[posYPlus1] != curlight
                        && dataTransparent[workportion[posYPlus1]])
                    {
                        flood.FloodLight(workportion, worklight, xx, yy, zz, dataLightRadius, dataTransparent);
                        flood.FloodLight(workportion, worklight, xx, yy + 1, zz, dataLightRadius, dataTransparent);
                    }
                }
            }
        }
    }
    
    void LightEmitting(int[] workportion, byte[] worklight, int[] dataLightRadius, bool[] dataTransparent)
    {
        const int portionsize = 16;
        const int portionsize3 = portionsize * portionsize * portionsize;
        for (int pos = 0; pos < portionsize3; pos++)
        {
            if (workportion[pos] >= 10) //optimization
            {
                if (dataLightRadius[workportion[pos]] != 0) //optimization
                {
                    if (dataLightRadius[workportion[pos]] > worklight[pos])
                    {
                        int xx = MapUtilCi.PosX(pos, portionsize, portionsize);
                        int yy = MapUtilCi.PosY(pos, portionsize, portionsize);
                        int zz = MapUtilCi.PosZ(pos, portionsize, portionsize);
                        int l = dataLightRadius[workportion[pos]];
                        worklight[pos] = Game.IntToByte(MathCi.MaxInt(l, worklight[pos]));
                        flood.FloodLight(workportion, worklight, xx, yy, zz, dataLightRadius, dataTransparent);
                    }
                }
            }
        }
    }
}


// Calculates final light in chunk, using base light from 3x3x3 chunks around it.
// Floods light between chunks.
public class LightBetweenChunks
{
    public LightBetweenChunks()
    {
        chunksLight = new byte[3 * 3 * 3][];
        for (int i = 0; i < 3 * 3 * 3; i++)
        {
            chunksLight[i] = new byte[16 * 16 * 16];
        }
        chunksData = new int[3 * 3 * 3][];
        for (int i = 0; i < 3 * 3 * 3; i++)
        {
            chunksData[i] = new int[16 * 16 * 16];
        }

        flood = new LightFlood();
    }

    LightFlood flood;

    byte[][] chunksLight;
    int[][] chunksData;

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    public void CalculateLightBetweenChunks(Game game, int cx, int cy, int cz, int[] dataLightRadius, bool[] dataTransparent)
    {
        Input(game, cx, cy, cz);
        FloodBetweenChunks_(dataLightRadius, dataTransparent);
        Output(game, cx, cy, cz);
    }

    void Input(Game game, int cx, int cy, int cz)
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    int pcx = cx + x - 1;
                    int pcy = cy + y - 1;
                    int pcz = cz + z - 1;
                    if (!game.map.IsValidChunkPos(pcx, pcy, pcz))
                    {
                        ArrayFillInt(chunksData[Index3d(x, y, z, 3, 3)], 16 * 16 * 16, 0);
                        ArrayFillByte(chunksLight[Index3d(x, y, z, 3, 3)], 16 * 16 * 16, 0);
                        continue;
                    }
                    Chunk p = game.map.GetChunk_(pcx, pcy, pcz);
                    int[] data = chunksData[Index3d(x, y, z, 3, 3)];
                    if (p.data != null)
                    {
                        for (int i = 0; i < 16 * 16 * 16; i++)
                        {
                            data[i] = p.data[i];
                        }
                    }
                    if (p.dataInt != null)
                    {
                        for (int i = 0; i < 16 * 16 * 16; i++)
                        {
                            data[i] = p.dataInt[i];
                        }
                    }
                    byte[] light = chunksLight[Index3d(x, y, z, 3, 3)];
                    for (int i = 0; i < 16 * 16 * 16; i++)
                    {
                        light[i] = p.baseLight[i];
                    }
                }
            }
        }
    }

    void FloodBetweenChunks_(int[] dataLightRadius, bool[] dataTransparent)
    {
        for (int i = 0; i < 2; i++)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        byte[] clight = chunksLight[Index3d(x, y, z, 3, 3)];
                        // Up
                        // For each point (xx,yy,15)
                        // Flood to point (xx,yy,0) on chunk (x,y,z+1).
                        if (z < 2)
                        {
                            byte[] dclight = chunksLight[Index3d(x, y, z + 1, 3, 3)];
                            for (int xx = 0; xx < 16; xx++)
                            {
                                for (int yy = 0; yy < 16; yy++)
                                {
                                    FloodBetweenChunks(chunksLight, clight, dclight, x, y, z, x, y, z + 1, xx, yy, 15, xx, yy, 0, dataLightRadius, dataTransparent);
                                }
                            }
                        }
                        if (z > 0)
                        {
                            byte[] dclight = chunksLight[Index3d(x, y, z - 1, 3, 3)];
                            for (int xx = 0; xx < 16; xx++)
                            {
                                for (int yy = 0; yy < 16; yy++)
                                {
                                    FloodBetweenChunks(chunksLight, clight, dclight, x, y, z, x, y, z - 1, xx, yy, 0, xx, yy, 15, dataLightRadius, dataTransparent);
                                }
                            }
                        }
                        if (x < 2)
                        {
                            byte[] dclight = chunksLight[Index3d(x + 1, y, z, 3, 3)];
                            for (int yy = 0; yy < 16; yy++)
                            {
                                for (int zz = 0; zz < 16; zz++)
                                {
                                    FloodBetweenChunks(chunksLight, clight, dclight, x, y, z, x + 1, y, z, 15, yy, zz, 0, yy, zz, dataLightRadius, dataTransparent);
                                }
                            }
                        }
                        if (x > 0)
                        {
                            byte[] dclight = chunksLight[Index3d(x - 1, y, z, 3, 3)];
                            for (int yy = 0; yy < 16; yy++)
                            {
                                for (int zz = 0; zz < 16; zz++)
                                {
                                    FloodBetweenChunks(chunksLight, clight, dclight, x, y, z, x - 1, y, z, 0, yy, zz, 15, yy, zz, dataLightRadius, dataTransparent);
                                }
                            }
                        }
                        if (y < 2)
                        {
                            byte[] dclight = chunksLight[Index3d(x, y + 1, z, 3, 3)];
                            for (int xx = 0; xx < 16; xx++)
                            {
                                for (int zz = 0; zz < 16; zz++)
                                {
                                    FloodBetweenChunks(chunksLight, clight, dclight, x, y, z, x, y + 1, z, xx, 15, zz, xx, 0, zz, dataLightRadius, dataTransparent);
                                }
                            }
                        }
                        if (y > 0)
                        {
                            byte[] dclight = chunksLight[Index3d(x, y - 1, z, 3, 3)];
                            for (int xx = 0; xx < 16; xx++)
                            {
                                for (int zz = 0; zz < 16; zz++)
                                {
                                    FloodBetweenChunks(chunksLight, clight, dclight, x, y, z, x, y - 1, z, xx, 0, zz, xx, 15, zz, dataLightRadius, dataTransparent);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void ArrayFillInt(int[] arr, int n, int value)
    {
        for (int i = 0; i < n; i++)
        {
            arr[i] = value;
        }
    }

    void ArrayFillByte(byte[] arr, int n, byte value)
    {
        for (int i = 0; i < n; i++)
        {
            arr[i] = value;
        }
    }

    void FloodBetweenChunks(byte[][] chunksLight_, byte[] cLight, byte[] dcLight, int cx, int cy, int cz, int dcx, int dcy, int dcz, int xx, int yy, int zz, int dxx, int dyy, int dzz, int[] dataLightRadius, bool[] dataTransparent)
    {
        int sourceLight = cLight[Index3d(xx, yy, zz, 16, 16)];
        int targetLight = dcLight[Index3d(dxx, dyy, dzz, 16, 16)];
        if (targetLight < sourceLight - 1)
        {
            dcLight[Index3d(dxx, dyy, dzz, 16, 16)] = Game.IntToByte(sourceLight - 1);
            flood.FloodLight(chunksData[Index3d(dcx, dcy, dcz, 3, 3)], dcLight, dxx, dyy, dzz, dataLightRadius, dataTransparent);
        }
    }
    
    void Output(Game game, int cx, int cy, int cz)
    {
        Chunk chunk = game.map.GetChunk_(cx, cy, cz);
        for (int x = 0; x < 18; x++)
        {
            for (int y = 0; y < 18; y++)
            {
                for (int z = 0; z < 18; z++)
                {
                    int baseglobalx = Game.chunksize - 1 + x;
                    int baseglobaly = Game.chunksize - 1 + y;
                    int baseglobalz = Game.chunksize - 1 + z;
                    byte light = 15;
                    int basecx = baseglobalx / 16;
                    int basecy = baseglobaly / 16;
                    int basecz = baseglobalz / 16;
                    {
                        int basexx = baseglobalx % 16;
                        int baseyy = baseglobaly % 16;
                        int basezz = baseglobalz % 16;
                        light = chunksLight[Index3d(basecx, basecy, basecz, 3, 3)][Index3d(basexx, baseyy, basezz, 16, 16)];
                    }
                    chunk.rendered.light[Index3d(x, y, z, 18, 18)] = light;
                }
            }
        }
    }
}
