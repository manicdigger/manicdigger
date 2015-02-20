// Calculates light in a single 16x16x16 chunk ("base light").
// Does not spread light between chunks.
public class ShadowsBase
{
    public ShadowsBase()
    {
        q_ = new FastQueueInt();
        q_.Initialize(1024);
        lighttoflood = new FastStackInt();
        lighttoflood.Initialize(1024);
        workData = new int[16 * 16 * 16];
    }
    FastStackInt lighttoflood;
    int[] workData;
    const int blocksnearLength = 6;

    public void CalculateChunkBaseLight(Game game, int cx, int cy, int cz, int[] dataLightRadius, bool[] transparentForLight)
    {
        Chunk chunk = game.map.GetChunk(cx * Game.chunksize, cy * Game.chunksize, cz * Game.chunksize);

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
        lighttoflood.Clear();


        int baseheight = cz * Game.chunksize;
        ApplySunlight(game, cx, cy, cz, workLight, dataLightRadius, baseheight, sunlight);
        FloodSunlight(workData, workLight, transparentForLight);
        ApplyLightEmitting(workData, workLight, dataLightRadius);
        FloodLight(workData, workLight, dataLightRadius, transparentForLight);
    }


#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    static void ApplySunlight(Game game, int cx, int cy, int cz, byte[] worklight, int[] dataLightRadius, int baseheight, int sunlight)
    {
        const int zplus = 16 * 16;
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
                    pos += zplus;
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

    void ApplyLightEmitting(int[] workportionArr, byte[] worklightArr, int[] dataLightRadius)
    {
        int[] radiusArr = dataLightRadius;
        const int portionsize = 16;
        const int portionsize3 = portionsize * portionsize * portionsize;
        Vector3IntRef p = new Vector3IntRef();
        for (int pos = 0; pos < portionsize3; pos++)
        {
            if (workportionArr[pos] >= 10) //optimization
            {
                if (radiusArr[workportionArr[pos]] != 0) //optimization
                {
                    if (radiusArr[workportionArr[pos]] > worklightArr[pos])
                    {
                        MapUtilCi.PosInt(pos, portionsize, portionsize, p);
                        int xx = p.X;
                        int yy = p.Y;
                        int zz = p.Z;
                        int l = radiusArr[workportionArr[pos]];
                        lighttoflood.Push(xx);
                        lighttoflood.Push(yy);
                        lighttoflood.Push(zz);
                        worklightArr[pos] = Game.IntToByte(MathCi.MaxInt(l, worklightArr[pos]));
                    }
                }
            }
        }
    }

    void FloodSunlight(int[] workportion, byte[] worklight, bool[] transparent)
    {
        for (int xx = 0; xx < 16; xx++)
        {
            for (int yy = 0; yy < 16; yy++)
            {
                for (int zz = 0; zz < 16; zz++)
                {
                    int pos = Index3d(xx, yy, zz, 16, 16);
                    if (!transparent[workportion[pos]])
                    {
                        continue;
                    }
                    int curlight = worklight[pos];
                    int posXPlus1 = pos + 1;
                    int posYPlus1 = pos + 16;
                    if (xx + 1 < 16
                        && worklight[posXPlus1] != curlight
                        && transparent[workportion[posXPlus1]])
                    {
                        lighttoflood.Push(xx);
                        lighttoflood.Push(yy);
                        lighttoflood.Push(zz);

                        lighttoflood.Push(xx + 1);
                        lighttoflood.Push(yy);
                        lighttoflood.Push(zz);
                    }
                    if (yy + 1 < 16
                        && worklight[posYPlus1] != curlight
                        && transparent[workportion[posYPlus1]])
                    {
                        lighttoflood.Push(xx);
                        lighttoflood.Push(yy);
                        lighttoflood.Push(zz);

                        lighttoflood.Push(xx);
                        lighttoflood.Push(yy + 1);
                        lighttoflood.Push(zz);
                    }
                }
            }
        }
    }

    void FloodLight(int[] portion, byte[] light, int[] dataLightRadius, bool[] dataTransparent)
    {
        while (lighttoflood.Count_() > 0)
        {
            int z = lighttoflood.Pop();
            int y = lighttoflood.Pop();
            int x = lighttoflood.Pop();
            FloodLight_(q_, portion, light, x, y, z, dataLightRadius, dataTransparent);
        }
    }

    const int minlight = 0;

    FastQueueInt q_;
    public static void FloodLight_(FastQueueInt q, int[] portion, byte[] light, int startx, int starty, int startz, int[] dataLightRadius, bool[] dataTransparent)
    {
        const int portionsize = 16;
        int pos = Index3d(startx, starty, startz, portionsize, portionsize);
        if (light[pos] == minlight)
        {
            return;
        }
        int lightradius = dataLightRadius[portion[pos]];
        if (lightradius != 0)
        {
            light[pos] = Game.IntToByte(lightradius);
        }
        //if (light[pos + 1] == light[pos]
        //    && light[pos - 1] == light[pos]
        //    && light[pos + portionsize] == light[pos]
        //    && light[pos - portionsize] == light[pos]
        //    && light[pos + portionsize * portionsize] == light[pos]
        //    && light[pos - portionsize * portionsize] == light[pos])
        //{
        //    return;
        //}

        q.Clear();
        int start = Index3d(startx, starty, startz, portionsize, portionsize);
        q.Push(start);
        for (; ; )
        {
            if (q.Count == 0)
            {
                break;
            }
            int vpos = q.Pop();
            int vlight = light[vpos];
            if (vlight == minlight)
            {
                continue;
            }
            int vblock = portion[vpos];
            if (!dataTransparent[vblock]
                && dataLightRadius[vblock] == 0)
            {
                continue;
            }
            int x = MapUtilCi.PosX(vpos, 16, 16);
            int y = MapUtilCi.PosY(vpos, 16, 16);
            int z = MapUtilCi.PosZ(vpos, 16, 16);
            if (x < 15) { Push(q, light, vlight, vpos + XPlus); }
            if (x > 0) { Push(q, light, vlight, vpos + XMinus); }
            if (y < 15) { Push(q, light, vlight, vpos + YPlus); }
            if (y > 0) { Push(q, light, vlight, vpos + YMinus); }
            if (z < 15) { Push(q, light, vlight, vpos + ZPlus); }
            if (z > 0) { Push(q, light, vlight, vpos + ZMinus); }
        }
    }

    static void Push(FastQueueInt q, byte[] light, int vlight, int n)
    {
        if (light[n] < vlight - 1)
        {
            light[n] = Game.IntToByte(vlight - 1);
            q.Push(n);
        }
    }

    const int XPlus = 1;
    const int XMinus = -1;
    const int YPlus = 16;
    const int YMinus = -16;
    const int ZPlus = 256;
    const int ZMinus = -256;
}


// Calculates final light in chunk, using base light from 3x3x3 chunks around it.
// Floods light between chunks.
public class ShadowsBetweenChunks
{
    public ShadowsBetweenChunks()
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

        q = new FastQueueInt();
        q.Initialize(1024);
    }

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

    public void CalculateShadowsBetweenChunks(Game game, int cx, int cy, int cz, int[] dataLightRadius, bool[] dataTransparent)
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
                        ClearInt(chunksData[Index3d(x, y, z, 3, 3)], 0);
                        ClearByte(chunksLight[Index3d(x, y, z, 3, 3)], 15);
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

    void ClearInt(int[] p, int value)
    {
        for (int i = 0; i < 16 * 16 * 16; i++)
        {
            p[i] = value;
        }
    }

    void ClearByte(byte[] p, byte value)
    {
        for (int i = 0; i < 16 * 16 * 16; i++)
        {
            p[i] = value;
        }
    }

    FastQueueInt q;
    void FloodBetweenChunks(byte[][] chunksLight_, byte[] cLight, byte[] dcLight, int cx, int cy, int cz, int dcx, int dcy, int dcz, int xx, int yy, int zz, int dxx, int dyy, int dzz, int[] dataLightRadius, bool[] dataTransparent)
    {
        int sourceLight = cLight[Index3d(xx, yy, zz, 16, 16)];
        int target = dcLight[Index3d(dxx, dyy, dzz, 16, 16)];
        if (target < sourceLight - 1)
        {
            dcLight[Index3d(dxx, dyy, dzz, 16, 16)] = Game.IntToByte(sourceLight - 1);
            ShadowsBase.FloodLight_(q, chunksData[Index3d(dcx, dcy, dcz, 3, 3)], dcLight, dxx, dyy, dzz, dataLightRadius, dataTransparent);
        }
    }

    void Output(Game game, int cx, int cy, int cz)
    {
        Chunk chunk = game.map.GetChunk(cx * Game.chunksize, cy * Game.chunksize, cz * Game.chunksize);
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


