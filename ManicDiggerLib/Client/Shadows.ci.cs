public abstract class IShadows3x3x3
{
    public abstract void Start();
    public abstract void Update(byte[] outputChunkLight,
        int[][] inputMapChunks, int[][] inputHeightmapChunks,
        int[] dataLightRadius, bool[] dataTransparent, int currentSunlight, int baseheight);
}


public class Shadows3x3x3 : IShadows3x3x3
{
    public Shadows3x3x3()
    {
        workportionArr = new int[workportionArrLength];
        worklightArr = new byte[worklightArrLength];
        q = new FastQueueInt();
        q.Initialize(1024);
        lighttoflood = new FastStackVector3IntRef();
        lighttoflood.Initialize(1024);
        blocksnear = new int[6];
        blocksnear[0] = -1;
        blocksnear[1] = 1;
        blocksnear[2] = -chunksizeportion;
        blocksnear[3] = chunksizeportion;
        blocksnear[4] = -chunksizeportion * chunksizeportion;
        blocksnear[5] = chunksizeportion * chunksizeportion;
    }
    const int workportionArrLength = 16 * 16 * 16 * 3 * 3 * 3;
    const int worklightArrLength = 16 * 16 * 16 * 3 * 3 * 3;
    public override void Start()
    {
    }
    public override void Update(byte[] outputChunkLight,
        int[][] inputMapChunks, int[][] inputHeightmapChunks,
        int[] dataLightRadius, bool[] dataTransparent, int currentSunlight, int baseheight)
    {
        this.outputChunkLight = outputChunkLight;
        this.inputMapChunks = inputMapChunks;
        this.inputHeightmapChunks = inputHeightmapChunks;
        this.dataLightRadius = dataLightRadius;
        this.dataTransparent = dataTransparent;
        this.sunlight = currentSunlight;
        this.baseheight = baseheight;

        lighttoflood.Clear();
        for (int i = 0; i < workportionArrLength; i++)
        {
            workportionArr[i] = 0;
        }
        GetPortion();
        for (int i = 0; i < worklightArrLength; i++)
        {
            worklightArr[i] = 0;
        }
        ApplySunlight();
        ApplyLightEmitting();
        FloodSunlight();
        FloodLight();
        SetPortion();
    }

    byte[] outputChunkLight;
    int[][] inputMapChunks;
    int[][] inputHeightmapChunks;
    int[] dataLightRadius;
    bool[] dataTransparent;
    int sunlight;
    int baseheight;
    int[] workportionArr;
    byte[] worklightArr;

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    const int minlight = 0;
    const int chunksize = 16;
    //copies 3x3x3 16*16*16 input chunks to one 48*48*48 temporary portion.
    void GetPortion()
    {
        for (int xx = 0; xx < 3; xx++)
        {
            for (int yy = 0; yy < 3; yy++)
            {
                for (int zz = 0; zz < 3; zz++)
                {
                    //if (IsValidChunkPos(x / chunksize + xx, y / chunksize + yy, z / chunksize + zz))
                    int[] chunk = inputMapChunks[Index3d(xx, yy, zz, 3, 3)];
                    if (chunk != null)
                    {
                        CopyChunk(workportionArr,
                            chunk,
                            xx * chunksize, yy * chunksize, zz * chunksize,
                            chunksize * 3, chunksize * 3, chunksize * 3);
                    }
                }
            }
        }
    }

    void CopyChunk(int[] portionArr, int[] chunkArr, int x, int y, int z,
        int portionsizex, int portionsizey, int portionsizez)
    {
        int[] portion = portionArr;
        int[] chunk = chunkArr;
        {
            for (int zz = 0; zz < 16; zz++)
            {
                for (int yy = 0; yy < 16; yy++)
                {
                    int pos = Index3d(0, yy, zz, 16, 16);
                    int pos2 = Index3d(x + 0, y + yy, z + zz, portionsizex, portionsizey);

                    //pos /= 2;
                    //pos2 /= 2;
                    //portionInt[pos2 + 0] = chunkInt[pos + 0];
                    //portionInt[pos2 + 1] = chunkInt[pos + 1];
                    //portionInt[pos2 + 2] = chunkInt[pos + 2];
                    //portionInt[pos2 + 3] = chunkInt[pos + 3];
                    //portionInt[pos2 + 4] = chunkInt[pos + 4];
                    //portionInt[pos2 + 5] = chunkInt[pos + 5];
                    //portionInt[pos2 + 6] = chunkInt[pos + 6];
                    //portionInt[pos2 + 7] = chunkInt[pos + 7];


                    for (int xx = 0; xx < 16; xx++)
                    {
                        int orig = chunk[pos];
                        portionArr[pos2] = orig;
                        pos++;
                        pos2++;
                    }
                }
            }
        }
    }

    void ApplySunlight()
    {
        const int portionsize = 16 * 3;
        int[] radius = dataLightRadius;
        const int zplus = portionsize * portionsize;
        //fixed (byte* worklight = worklightArr)
        byte[] worklight = worklightArr;
        {
            for (int xx = 0; xx < portionsize; xx++)
            {
                for (int yy = 0; yy < portionsize; yy++)
                {
                    int height = GetLightHeight(xx, yy);
                    int h = height - baseheight;
                    //h = MyMath.Clamp(h, 0, portionsize);
                    if (h < 0) { h = 0; }
                    if (h > portionsize) { continue; }
                    int pos = Index3d(xx, yy, h, portionsize, portionsize);

                    //for (int zz = 0; zz < h; zz++)
                    //{
                    //    worklight[pos] = (byte)minlight;
                    //    pos += zplus;
                    //}

                    for (int zz = h; zz < portionsize; zz++)
                    {
                        //int pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                        worklight[pos] = Game.IntToByte(sunlight);
                        pos += zplus;
                    }
                }
            }
        }
    }

    int GetLightHeight(int xx, int yy)
    {
        int[] chunk = inputHeightmapChunks[MapUtilCi.Index2d(xx / chunksize, yy / chunksize, 3)];
        if (chunk == null)
        {
            //throw new Exception();
            //return 64;
            return 0;
        }
        return chunk[MapUtilCi.Index2d(xx % chunksize, yy % chunksize, chunksize)];
    }

    void ApplyLightEmitting()
    {
        int[] radiusArr = dataLightRadius;
        const int portionsize = 16 * 3;
        const int portionsize3 = portionsize * portionsize * portionsize;
        int[] workportion = workportionArr;
        byte[] worklight = worklightArr;
        int[] radius = radiusArr;
        Vector3IntRef p = new Vector3IntRef();
        {
            int[] portionInt = workportion;
            int portionsize3div2 = portionsize3 / 2;
            for (int pos1 = 0; pos1 < portionsize3div2; pos1++)
            //for (int pos = 0; pos < portionsize3; pos++)
            //for (int xx = 0; xx < portionsize; xx++)
            {
                //for (int yy = 0; yy < portionsize; yy++)
                {
                    // for (int zz = 0; zz < portionsize; zz++)
                    //if (portionInt[pos1] == 0)
                    //{
                    //    continue;
                    //}
                    for (int pos = pos1 * 2; pos < pos1 * 2 + 2; pos++)
                    {
                        if (workportion[pos] >= 10) //optimization
                        {
                            if (radius[workportion[pos]] != 0) //optimization
                            {
                                //var pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                                if (radius[workportion[pos]] > worklight[pos])
                                {
                                    MapUtilCi.PosInt(pos, portionsize, portionsize, p);
                                    int xx = p.X;
                                    int yy = p.Y;
                                    int zz = p.Z;
                                    int l = radius[workportion[pos]];
                                    if (xx > 1 && yy > 1 && zz > 1
                                        && xx < portionsize - 1 && yy < portionsize - 1 && zz < portionsize - 1)
                                    {
                                        lighttoflood.Push(Vector3IntRef.Create(xx, yy, zz));
                                    }
                                    worklight[pos] = Game.IntToByte(Game.MaxInt(l, worklight[pos]));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void FloodSunlight()
    {
        int portionsize = 16 * 3;
        int portionsize3 = Pow3(portionsize);
        //int startx = x;
        //int starty = y;
        //int startz = z;
        int[] radiusArr = dataLightRadius;
        bool[] transparentArr = dataTransparent;
        int[] workportion = workportionArr;
        byte[] worklight = worklightArr;
        int[] radius = radiusArr;
        bool[] transparent = transparentArr;
        Vector3IntRef p = new Vector3IntRef();
        {
            for (int pos = 0; pos < portionsize3 - portionsize; pos++)
            //for (int zz = 1; zz < portionsize - 1; zz++)
            {
                //for (int xx = 1; xx < portionsize - 1; xx++)
                {
                    //for (int yy = 1; yy < portionsize - 1; yy++)
                    {
                        //int pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                        if (!transparent[workportion[pos]])
                        {
                            continue;
                        }
                        int curlight = worklight[pos];
                        if ((worklight[pos + 1] != curlight && transparent[workportion[pos + 1]])
                            || (worklight[pos + portionsize] != curlight && transparent[workportion[pos + portionsize]]))
                        {
                            MapUtilCi.PosInt(pos, portionsize, portionsize, p);
                            int xx = p.X;
                            int yy = p.Y;
                            int zz = p.Z;
                            if (xx > 1 && yy > 1 && zz > 1
                                && xx < portionsize - 1 && yy < portionsize - 1 && zz < portionsize - 1)
                            {
                                lighttoflood.Push(Vector3IntRef.Create(xx, yy, zz));
                                lighttoflood.Push(Vector3IntRef.Create(xx + 1, yy, zz));
                                lighttoflood.Push(Vector3IntRef.Create(xx, yy + 1, zz));
                            }
                        }
                    }
                }
            }
        }
    }

    int Pow3(int x)
    {
        return x * x * x;
    }

    void FloodLight()
    {
        while (lighttoflood.Count() > 0)
        {
            Vector3IntRef k = lighttoflood.Pop();
            FloodLight_(workportionArr, worklightArr, k.X, k.Y, k.Z);
        }
    }

    FastQueueInt q;
    public void FloodLight_(int[] portion, byte[] light, int startx, int starty, int startz)
    {
        const int portionsize = 16 * 3;
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
        if (light[pos + 1] == light[pos]
            && light[pos - 1] == light[pos]
            && light[pos + portionsize] == light[pos]
            && light[pos - portionsize] == light[pos]
            && light[pos + portionsize * portionsize] == light[pos]
            && light[pos - portionsize * portionsize] == light[pos])
        {
            return;
        }

        q.Clear();
        //Vector3i start = new Vector3i();
        //start.x = startx;
        //start.y = starty;
        //start.z = startz;
        int start = Index3d(startx, starty, startz, portionsize, portionsize);
        q.Push(start);
        for (; ; )
        {
            if (q.Count == 0)
            {
                break;
            }
            int vpos = q.Pop();
            //var v = q.Pop();
            //int vpos = MapUtil.Index3d(v.x, v.y, v.z, portionsize, portionsize);
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
            for (int i = 0; i < blocksnearLength; i++)
            {
                int n = vpos + blocksnear[i];

                //int nx = v.x + blocksnear[i].x;
                //int ny = v.y + blocksnear[i].y;
                //int nz = v.z + blocksnear[i].z;
                //if (!IsValidPos(nx, ny, nz))
                //{
                //    continue;
                //}

                //int n = MapUtil.Index3d(nx, ny, nz, portionsize, portionsize);
                if (n < 0 || n >= worklightArrLength) { continue; }
                if (light[n] < vlight - 1)
                {
                    light[n] = Game.IntToByte(vlight - 1);
                    //q.Push(new Vector3i(nx, ny, nz));
                    q.Push(n);
                }
            }
        }
    }

    //int chunksizeportion { get { return chunksize * 3; } }
    const int chunksizeportion = 48; // 16 * 3;

    bool IsValidPos(int vx, int vy, int vz)
    {
        return vx >= 0 && vy >= 0 && vz >= 0
            && vx < chunksizeportion && vy < chunksizeportion && vz < chunksizeportion;
    }

    int[] blocksnear;
    const int blocksnearLength = 6;

    //Vector3i[] blocksnear = new Vector3i[6]
    //{
    //    new Vector3i(-1, 0, 0),
    //    new Vector3i(1, 0, 0),
    //    new Vector3i(0, -1, 0),
    //    new Vector3i(0, 1, 0),
    //    new Vector3i(0, 0, -1),
    //    new Vector3i(0, 0, 1),
    //};

    void SetPortion()
    {
        //int mapsizex = d_Map.MapSizeX;
        //int mapsizey = d_Map.MapSizeY;
        //int mapsizez = d_Map.MapSizeZ;
        for (int xx = -1; xx < chunksize + 1; xx++)
        {
            for (int yy = -1; yy < chunksize + 1; yy++)
            {
                for (int zz = -1; zz < chunksize + 1; zz++)
                {
                    //if (IsValidPos(x + chunksize + xx, y + chunksize + yy, z + chunksize + zz,
                    //    mapsizex, mapsizey, mapsizez))
                    {
                        //d_Light.SetBlock(x + chunksize + xx, y + chunksize + yy, z + chunksize + zz,
                        //    light[MapUtil.Index3d(xx + chunksize, yy + chunksize, zz + chunksize,
                        //    chunksize * 3, chunksize * 3)]);
                        outputChunkLight[Index3d(xx + 1, yy + 1, zz + 1, 18, 18)] =
                            worklightArr[Index3d(xx + chunksize, yy + chunksize, zz + chunksize,
                            chunksize * 3, chunksize * 3)];
                    }
                }
            }
        }
    }

    public static bool IsValidPos_(int x, int y, int z, int MapSizeX, int MapSizeY, int MapSizeZ)
    {
        if (x < 0 || y < 0 || z < 0)
        {
            return false;
        }
        if (x >= MapSizeX || y >= MapSizeY || z >= MapSizeZ)
        {
            return false;
        }
        return true;
    }

    FastStackVector3IntRef lighttoflood;
}


public class FastQueueInt
{
    public void Initialize(int maxCount)
    {
        this.maxCount = maxCount;
        values = new int[maxCount];
        Count = 0;
        start = 0;
        end = 0;
    }
    int maxCount;
    int[] values;
    internal int Count;
    int start;
    int end;
    public void Push(int value)
    {
        values[end] = value;
        Count++;
        end++;
        if (end >= maxCount)
        {
            end = 0;
        }
    }
    public int Pop()
    {
        int value = values[start];
        Count--;
        start++;
        if (start >= maxCount)
        {
            start = 0;
        }
        return value;
    }
    public void Clear()
    {
        Count = 0;
    }
}



public class FastStackVector3IntRef
{
    public void Initialize(int maxCount)
    {
        valuesLength = maxCount;
        values = new Vector3IntRef[maxCount];
    }
    Vector3IntRef[] values;
    int valuesLength;
    internal int count;
    public void Push(Vector3IntRef value)
    {
        while (count >= valuesLength)
        {
            Vector3IntRef[] values2 = new Vector3IntRef[valuesLength * 2];
            for (int i = 0; i < valuesLength; i++)
            {
                values2[i] = values[i];
            }
            values = values2;
            valuesLength = valuesLength * 2;
        }
        values[count] = value;
        count++;
    }
    public Vector3IntRef Pop()
    {
        count--;
        return values[count];
    }
    public void Clear()
    {
        count = 0;
    }

    internal int Count()
    {
        return count;
    }
}

public class Shadows3x3x3Simple : IShadows3x3x3
{
    public override void Start()
    {
    }

    const int shadowlight = 10;
    const int maxlight = 15;

    public override void Update(byte[] outputChunkLight, int[][] inputMapChunks, int[][] inputHeightmapChunks, int[] dataLightRadius, bool[] dataTransparent, int currentSunlight, int baseheight)
    {
        this.inputHeightmapChunks = inputHeightmapChunks;

        int outputChunkLightLength = (chunksize + 2) * (chunksize + 2) * (chunksize + 2);
        for (int i = 0; i < outputChunkLightLength; i++)
        {
            outputChunkLight[i] = Game.IntToByte(shadowlight);
        }
        int zplus = 18 * 18;
        for (int xx = 0; xx < 18; xx++)
        {
            for (int yy = 0; yy < 18; yy++)
            {
                int height = GetLightHeight(16 - 1 + xx, 16 - 1 + yy) - 16;
                int h = height - baseheight;
                if (h < 0) { h = 0; }
                if (h > 18) { continue; }
                int pos = MapUtilCi.Index3d(xx, yy, h, 18, 18);
                
                //for (int zz = 0; zz < h; zz++)
                //{
                //    worklight[pos] = (byte)minlight;
                //    pos += zplus;
                //}
                
                for (int zz = h; zz < 18; zz++)
                {
                    //int pos = MapUtil.Index3d(xx, yy, zz, portionsize, portionsize);
                    outputChunkLight[pos] =  Game.IntToByte(maxlight);
                    pos += zplus;
                }
            }
        }
    }
    int[][] inputHeightmapChunks;
    const int chunksize = 16;
    int GetLightHeight(int xx, int yy)
    {
        int[] chunk = inputHeightmapChunks[MapUtilCi.Index2d(xx / chunksize, yy / chunksize, 3)];
        if (chunk == null)
        {
            //throw new Exception();
            //return 64;
            return 0;
        }
        return chunk[MapUtilCi.Index2d(xx % chunksize, yy % chunksize, chunksize)];
    }
}
