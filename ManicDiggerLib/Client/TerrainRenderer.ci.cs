public class TerrainRenderer
{
    public TerrainRenderer()
    {
        currentChunk = new int[18 * 18 * 18];
        currentChunkShadows = new byte[18 * 18 * 18];
        tempnearestpos = new int[3];
        unloadxyztemp = new int[2];
        chunksizebits = 4;
        ids = new int[1024];
        idsCount = 0;
    }
    internal Game game;
    int chunkupdates;
    public int ChunkUpdates() { return chunkupdates; }
    public int maxlight() { return 15; }

    IShadows3x3x3 shadows;
    bool terrainRendererStarted;

    int GetChunksize()
    {
        return game.chunksize;
    }

    public void StartTerrain()
    {
        sqrt3half = game.platform.MathSqrt(3) / 2;
        game.d_TerrainChunkTesselator.Start();
        if (shadowssimple)
        {
            shadows = new Shadows3x3x3Simple();
        }
        else
        {
            shadows = new Shadows3x3x3();
        }
        shadows.Start();
        terrainRendererStarted = true;
        chunksize = GetChunksize();
    }

    int chunksize;

    int mapAreaSize() { return game.platform.FloatToInt(game.d_Config3d.viewdistance) * 2; }
    int centerAreaSize() { return game.platform.FloatToInt(game.d_Config3d.viewdistance) / 2; }
    int mapAreaSizeZ() { return mapAreaSize(); }

    int chunksizebits;
    int mapsizexchunks() { return game.MapSizeX >> chunksizebits; }
    int mapsizeychunks() { return game.MapSizeY >> chunksizebits; }
    int mapsizezchunks() { return game.MapSizeZ >> chunksizebits; }

    public void UpdateTerrain()
    {
        if (!terrainRendererStarted)
        {
            //Start() not called yet.
            return;
        }

        if (!(game.lastplacedblockX == -1 && game.lastplacedblockY == -1 && game.lastplacedblockZ == -1))
        {
            HashSetVector3IntRef ChunksToRedraw = new HashSetVector3IntRef();
            Vector3IntRef[] around = BlocksAround7(Vector3IntRef.Create(game.lastplacedblockX, game.lastplacedblockY, game.lastplacedblockZ));
            for (int i = 0; i < 7; i++)
            {
                Vector3IntRef a = around[i];
                ChunksToRedraw.Set(Vector3IntRef.Create(a.X / chunksize, a.Y / chunksize, a.Z / chunksize));
            }
            for (int i = 0; i < ChunksToRedraw.max; i++)
            {
                if (ChunksToRedraw.values[i] == null)
                {
                    break;
                }
                int[] c = new int[3];
                int xx = ChunksToRedraw.values[i].X;
                int yy = ChunksToRedraw.values[i].Y;
                int zz = ChunksToRedraw.values[i].Z;
                if (xx >= 0 && yy >= 0 && zz >= 0
                    && xx < game.MapSizeX / chunksize && yy < game.MapSizeY / chunksize && zz < game.MapSizeZ / chunksize)
                {
                    Chunk chunk = game.chunks[MapUtilCi.Index3d(xx, yy, zz, mapsizexchunks(), mapsizeychunks())];
                    if (chunk == null || chunk.rendered == null)
                    {
                        continue;
                    }
                    if (chunk.rendered.dirty)
                    {
                        RedrawChunk(xx, yy, zz);
                    }
                }
            }
            game.lastplacedblockX = -1;
            game.lastplacedblockY = -1;
            game.lastplacedblockZ = -1;
        }
        int updated = 0;
        for (; ; )
        {
            NearestDirty(tempnearestpos);
            if (tempnearestpos[0] == -1 && tempnearestpos[1] == -1 && tempnearestpos[2] == -1)
            {
                break;
            }
            RedrawChunk(tempnearestpos[0], tempnearestpos[1], tempnearestpos[2]);
            //if (updated++ >= 1)
            {
                break;
            }
            //if (framestopwatch.ElapsedMilliseconds > 5)
            //{
            //    break;
            //}
        }
        UnloadRendererChunks();
    }

    public static Vector3IntRef[] BlocksAround7(Vector3IntRef pos)
    {
        Vector3IntRef[] arr = new Vector3IntRef[7];
        arr[0] = pos;
        arr[1] = Vector3IntRef.Create(pos.X + 1, pos.Y + 0, pos.Z + 0);
        arr[2] = Vector3IntRef.Create(pos.X - 1, pos.Y + 0, pos.Z + 0);
        arr[3] = Vector3IntRef.Create(pos.X + 0, pos.Y + 1, pos.Z + 0);
        arr[4] = Vector3IntRef.Create(pos.X + 0, pos.Y - 1, pos.Z + 0);
        arr[5] = Vector3IntRef.Create(pos.X + 0, pos.Y + 0, pos.Z + 1);
        arr[6] = Vector3IntRef.Create(pos.X + 0, pos.Y + 0, pos.Z - 1);
        return arr;
    }

    int unloadIterationXy;
    int[] unloadxyztemp;
    void UnloadRendererChunks()
    {
        int px = game.platform.FloatToInt(game.player.playerposition.X) / chunksize;
        int py = game.platform.FloatToInt(game.player.playerposition.Z) / chunksize;
        int pz = game.platform.FloatToInt(game.player.playerposition.Y) / chunksize;

        int chunksxy = this.mapAreaSize() / chunksize / 2;
        int chunksz = this.mapAreaSizeZ() / chunksize / 2;

        int startx = px - chunksxy;
        int endx = px + chunksxy;
        int starty = py - chunksxy;
        int endy = py + chunksxy;
        int startz = pz - chunksz;
        int endz = pz + chunksz;

        if (startx < 0) { startx = 0; }
        if (starty < 0) { starty = 0; }
        if (startz < 0) { startz = 0; }
        if (endx >= mapsizexchunks()) { endx = mapsizexchunks() - 1; }
        if (endy >= mapsizeychunks()) { endy = mapsizeychunks() - 1; }
        if (endz >= mapsizezchunks()) { endz = mapsizezchunks() - 1; }


        for (int i = 0; i < 1000; i++)
        {
            unloadIterationXy++;
            if (unloadIterationXy >= mapsizexchunks() * mapsizeychunks() * mapsizezchunks())
            {
                unloadIterationXy = 0;
            }
            Vector3IntRef xyz = new Vector3IntRef();
            MapUtilCi.PosInt(unloadIterationXy, mapsizexchunks(), mapsizeychunks(), xyz);
            int x = xyz.X;
            int y = xyz.Y;
            int z = xyz.Z;
            int pos = MapUtilCi.Index3d(x, y, z, mapsizexchunks(), mapsizeychunks());
            bool unloaded = false;

            Chunk c = game.chunks[pos];
            if (c == null
                || c.rendered == null
                || c.rendered.ids == null)
            {
                continue;
            }
            if (x < startx || y < starty || z < startz
                || x > endx || y > endy || z > endz)
            {
                for (int k = 0; k < c.rendered.idsCount; k++)
                {
                    int loadedSubmesh = c.rendered.ids[k];
                    game.d_Batcher.Remove(loadedSubmesh);
                }
                c.rendered.ids = null;
                c.rendered.dirty = true;
                c.rendered.light = null;
            }
            unloaded = true;
            if (unloaded)
            {
                return;
            }
        }
    }

    const int intMaxValue = 2147483647;
    int[] tempnearestpos;
    void NearestDirty(int[] nearestpos)
    {
        int nearestdist = intMaxValue;
        nearestpos[0] = -1;
        nearestpos[1] = -1;
        nearestpos[2] = -1;
        int px = game.platform.FloatToInt(game.player.playerposition.X) / chunksize;
        int py = game.platform.FloatToInt(game.player.playerposition.Z) / chunksize;
        int pz = game.platform.FloatToInt(game.player.playerposition.Y) / chunksize;

        int chunksxy = this.mapAreaSize() / chunksize / 2;
        int chunksz = this.mapAreaSizeZ() / chunksize / 2;

        int startx = px - chunksxy;
        int endx = px + chunksxy;
        int starty = py - chunksxy;
        int endy = py + chunksxy;
        int startz = pz - chunksz;
        int endz = pz + chunksz;

        if (startx < 0) { startx = 0; }
        if (starty < 0) { starty = 0; }
        if (startz < 0) { startz = 0; }
        if (endx >= mapsizexchunks()) { endx = mapsizexchunks() - 1; }
        if (endy >= mapsizeychunks()) { endy = mapsizeychunks() - 1; }
        if (endz >= mapsizezchunks()) { endz = mapsizezchunks() - 1; }

        for (int x = startx; x <= endx; x++)
        {
            for (int y = starty; y <= endy; y++)
            {
                for (int z = startz; z <= endz; z++)
                {
                    Chunk c = game.chunks[MapUtilCi.Index3d(x, y, z, mapsizexchunks(), mapsizeychunks())];
                    if (c == null || c.rendered == null)
                    {
                        continue;
                    }
                    if (c.rendered.dirty)
                    {
                        int dx = px - x;
                        int dy = py - y;
                        int dz = pz - z;
                        int dist = dx * dx + dy * dy + dz * dz;
                        if (dist < nearestdist)
                        {
                            nearestdist = dist;
                            nearestpos[0] = x;
                            nearestpos[1] = y;
                            nearestpos[2] = z;
                        }
                    }
                }
            }
        }
    }

    public void DrawTerrain()
    {
        game.d_Batcher.Draw(game.player.playerposition.X, game.player.playerposition.Y, game.player.playerposition.Z);
    }

    public bool IsChunkRendered(int cx, int cy, int cz)
    {
        Chunk c = game.chunks[MapUtilCi.Index3d(cx, cy, cz, mapsizexchunks(), mapsizeychunks())];
        if (c == null)
        {
            return false;
        }
        return c.rendered != null && c.rendered.ids != null;
    }

    public void SetChunkDirty(int cx, int cy, int cz, bool dirty, bool blockschanged)
    {
        if (!terrainRendererStarted)
        {
            return;
        }

        if (!IsValidChunkPos(cx, cy, cz))
        {
            return;
        }

        Chunk c = game.chunks[MapUtilCi.Index3d(cx, cy, cz, mapsizexchunks(), mapsizeychunks())];
        if (c == null)
        {
            return;
        }
        if (c.rendered == null)
        {
            c.rendered = new RenderedChunk();
        }
        c.rendered.dirty = dirty;
        if (blockschanged)
        {
            c.rendered.shadowsdirty = true;
        }
    }

    public bool IsValidChunkPos(int cx, int cy, int cz)
    {
        return cx >= 0 && cy >= 0 && cz >= 0
            && cx < game.MapSizeX / chunksize
            && cy < game.MapSizeY / chunksize
            && cz < game.MapSizeZ / chunksize;
    }

    public void RedrawBlock(int x, int y, int z)
    {
        Vector3IntRef[] around = BlocksAround7(Vector3IntRef.Create(x, y, z));
        for (int i = 0; i < 7; i++)
        {
            Vector3IntRef a = around[i];
            int xx = a.X;
            int yy = a.Y;
            int zz = a.Z;
            if (xx < 0 || yy < 0 || zz < 0 || xx >= game.MapSizeX || yy >= game.MapSizeY || zz >= game.MapSizeZ)
            {
                return;
            }
            SetChunkDirty((xx / chunksize), (yy / chunksize), (zz / chunksize), true, true);
        }
    }

    public void RedrawAllBlocks()
    {
        if (!terrainRendererStarted)
        {
            StartTerrain();
        }
        int chunksLength = (game.MapSizeX / chunksize)
            * (game.MapSizeY / chunksize)
            * (game.MapSizeZ / chunksize);
        for (int i = 0; i < chunksLength; i++)
        {
            Chunk c = game.chunks[i];
            if (c == null)
            {
                continue;
            }
            if (c.rendered == null)
            {
                c.rendered = new RenderedChunk();
            }
            c.rendered.dirty = true;
        }
    }

    public IntRef MaybeGetLight(int x, int y, int z)
    {
        //try
        //{
        int l = 0;
        int cx = x / chunksize;
        int cy = y / chunksize;
        int cz = z / chunksize;
        if (game.IsValidPos(x, y, z) && game.IsValidChunkPos(cx, cy, cz, chunksize))
        {
            Chunk c = game.chunks[MapUtilCi.Index3d(cx, cy, cz, mapsizexchunks(), mapsizeychunks())];
            if (c == null
                || c.rendered == null
                || c.rendered.light == null)
            {
                l = 0;
            }
            else
            {
                l = c.rendered.light[MapUtilCi.Index3d((x % chunksize) + 1, (y % chunksize) + 1, (z % chunksize) + 1, chunksize + 2, chunksize + 2)];
            }
        }

        if (l == 0)
        {
            if (z >= game.d_Heightmap.GetBlock(x, y))
            {
                return IntRef.Create(game.sunlight_);
            }
            else
            {
                return IntRef.Create(minlight);
            }
        }
        else
        {
            //return l - 1;
            return IntRef.Create(l);
        }
        //}
        //catch
        //{
        //    return IntRef.Create(maxlight());
        //}
    }

    int[] ids;
    int idsCount;
    void RedrawChunk(int x, int y, int z)
    {
        Chunk c = game.chunks[MapUtilCi.Index3d(x, y, z, mapsizexchunks(), mapsizeychunks())];
        if (c == null)
        {
            return;
        }
        if (c.rendered == null)
        {
            c.rendered = new RenderedChunk();
        }
        if (c.rendered.ids != null)
        {
            for (int i = 0; i < c.rendered.idsCount; i++)
            {
                int loadedSubmesh = c.rendered.ids[i];
                game.d_Batcher.Remove(loadedSubmesh);
            }
        }
        c.rendered.dirty = false;
        chunkupdates++;

        idsCount = 0;
        GetExtendedChunk(x, y, z);
        if (!IsSolidChunk(currentChunk, (chunksize + 2) * (chunksize + 2) * (chunksize + 2)))
        {
            CalculateShadows(x, y, z);
            IntRef retCount = new IntRef();
            VerticesIndicesToLoad[] a = game.d_TerrainChunkTesselator.MakeChunk(x, y, z, currentChunk, currentChunkShadows, game.mLightLevels, retCount);
            for (int i = 0; i < retCount.value; i++)
            {
                VerticesIndicesToLoad submesh = a[i];
                if (submesh.modelData.GetIndicesCount() != 0)
                {
                    float centerVecX = submesh.positionX + chunksize / 2;
                    float centerVecY = submesh.positionZ + chunksize / 2;
                    float centerVecZ = submesh.positionY + chunksize / 2;
                    float radius = sqrt3half * chunksize;
                    ids[idsCount++] = game.d_Batcher.Add(submesh.modelData, submesh.transparent, submesh.texture, centerVecX, centerVecY, centerVecZ, radius);
                }
            }
        }
        int[] idsarr = new int[idsCount];
        for (int i = 0; i < idsCount; i++)
        {
            idsarr[i] = ids[i];
        }
        c.rendered.ids = idsarr;
        c.rendered.idsCount = idsCount;
    }
    float sqrt3half;

    bool IsSolidChunk(int[] currentChunk, int length)
    {
        int block = currentChunk[0];
        for (int i = 0; i < length; i++)
        {
            if (currentChunk[i] != currentChunk[0])
            {
                return false;
            }
        }
        return true;
    }

    int[] currentChunk;
    byte[] currentChunkShadows;

    //For performance, make a local copy of chunk and its surrounding.
    //To render one chunk, we need to know all blocks that touch chunk boundaries.
    //(because to render a single block we need to know all 6 blocks around it).
    //So it's needed to copy 16x16x16 chunk and its Borders to make a 18x18x18 "extended" chunk.
    void GetExtendedChunk(int x, int y, int z)
    {
        game.GetMapPortion(currentChunk, x * chunksize - 1, y * chunksize - 1, z * chunksize - 1,
            chunksize + 2, chunksize + 2, chunksize + 2);
    }

    int[][] chunks3x3x3;
    int[][] heightchunks3x3;
    void CalculateShadows(int cx, int cy, int cz)
    {
        if (chunks3x3x3 == null)
        {
            chunks3x3x3 = new int[3 * 3 * 3][]; //(byte**)Marshal.AllocHGlobal(sizeof(byte*) * 3 * 3 * 3);
            for (int i = 0; i < 3 * 3 * 3; i++)
            {
                chunks3x3x3[i] = new int[chunksize * chunksize * chunksize];
            }
            heightchunks3x3 = new int[3 * 3][];//(byte**)Marshal.AllocHGlobal(sizeof(byte*) * 3 * 3);
        }
        for (int i = 0; i < 3 * 3 * 3; i++)
        {
            int n = chunksize * chunksize * chunksize;
            int[] c = chunks3x3x3[i];
            for (int k = 0; k < n; k++)
            {
                c[k] = 0;
            }
        }
        for (int i = 0; i < 3 * 3; i++)
        {
            heightchunks3x3[i] = null;
        }
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    if (cx + x - 1 < 0 || cx + x - 1 >= game.MapSizeX / chunksize
                        || cy + y - 1 < 0 || cy + y - 1 >= game.MapSizeY / chunksize
                        || cz + z - 1 < 0 || cz + z - 1 >= game.MapSizeZ / chunksize)
                    {
                        continue;
                    }
                    Chunk chunk = game.chunks[MapUtilCi.Index3d(cx + x - 1, cy + y - 1, cz + z - 1, game.MapSizeX / chunksize, game.MapSizeY / chunksize)];
                    if (chunk != null)
                    {
                        game.CopyChunk(chunk, chunks3x3x3[MapUtilCi.Index3d(x, y, z, 3, 3)]);
                    }
                    else
                    {
                        //chunks[0] = null;
                    }
                }
            }
        }
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (cx + x - 1 < 0 || cx + x - 1 >= game.MapSizeX / chunksize
                    || cy + y - 1 < 0 || cy + y - 1 >= game.MapSizeY / chunksize)
                {
                    continue;
                }
                int[] chunk = game.d_Heightmap.chunks[MapUtilCi.Index2d(cx + x - 1, cy + y - 1, game.MapSizeX / chunksize)];
                heightchunks3x3[MapUtilCi.Index2d(x, y, 3)] = chunk;
            }
        }

        int[] lightRadius = new int[GlobalVar.MAX_BLOCKTYPES];
        bool[] isTransparentForLight = new bool[GlobalVar.MAX_BLOCKTYPES];
        for (int i = 0; i < GlobalVar.MAX_BLOCKTYPES; i++)
        {
            if (game.blocktypes[i] == null)
            {
                continue;
            }
            lightRadius[i] = game.blocktypes[i].LightRadius;
            isTransparentForLight[i] = IsTransparentForLight(i);
        }

        shadows.Update(currentChunkShadows, chunks3x3x3, heightchunks3x3, lightRadius, isTransparentForLight, game.sunlight_, cz * chunksize - chunksize);

        //for MaybeGetLight
        Chunk chunkLight = game.chunks[MapUtilCi.Index3d(cx, cy, cz, mapsizexchunks(), mapsizeychunks())];
        if (chunkLight.rendered != null)
        {
            if (chunkLight.rendered.light == null)
            {
                chunkLight.rendered.light = new byte[(chunksize + 2) * (chunksize + 2) * (chunksize + 2)];
            }
            int length = (chunksize + 2) * (chunksize + 2) * (chunksize + 2);
            for (int i = 0; i < length; i++)
            {
                chunkLight.rendered.light[i] = currentChunkShadows[i];
            }
        }
    }

    public bool IsTransparentForLight(int block)
    {
        Packet_BlockType b = game.blocktypes[block];
        return b.DrawType != Packet_DrawTypeEnum.Solid && b.DrawType != Packet_DrawTypeEnum.ClosedDoor;
    }

    public int TrianglesCount()
    {
        return game.d_Batcher.TotalTriangleCount();
    }

    internal bool shadowssimple;
    int minlight;
}


public class HashSetVector3IntRef
{
    public HashSetVector3IntRef()
    {
        max = 16;
        Start();
    }

    public void Start()
    {
        values = new Vector3IntRef[max];
    }

    internal Vector3IntRef[] values;
    internal int max;

    public void Set(Vector3IntRef value)
    {
        int i = 0;
        for (i = 0; i < max; i++)
        {
            if (values[i] == null)
            {
                break;
            }
            if (values[i].X == value.X
                && values[i].Y == value.Y
                && values[i].Z == value.Z)
            {
                return;
            }
        }
        values[i] = Vector3IntRef.Create(value.X, value.Y, value.Z);
    }
}
