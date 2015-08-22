public class ModDrawTerrain : ClientMod
{
    public ModDrawTerrain()
    {
        currentChunk = new int[18 * 18 * 18];
        currentChunkShadows = new byte[18 * 18 * 18];
        tempnearestpos = new int[3];
        ids = new int[1024];
        idsCount = 0;
        redraw = new TerrainRendererRedraw[128];
        redrawCount = 0;
        CalculateShadowslightRadius = new int[GlobalVar.MAX_BLOCKTYPES];
        CalculateShadowsisTransparentForLight = new bool[GlobalVar.MAX_BLOCKTYPES];
        lightBase = new LightBase();
        lightBetweenChunks = new LightBetweenChunks();

        lastPerformanceInfoupdateMilliseconds = 0;
        lastchunkupdates = 0;
        started = false;
    }

    internal Game game;
    int chunkupdates;
    public int ChunkUpdates() { return chunkupdates; }
    public int maxlight() { return 15; }

    bool terrainRendererStarted;

    bool started;
    int lastPerformanceInfoupdateMilliseconds;
    int lastchunkupdates;

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    public override void  OnNewFrameDraw3d(Game game_, float deltaTime)
    {
        game = game_;
        if (!started)
        {
            started = true;
        }
        if (game.shouldRedrawAllBlocks)
        {
            game.shouldRedrawAllBlocks = false;
            RedrawAllBlocks();
        }
        DrawTerrain();
        UpdatePerformanceInfo(deltaTime);
    }

    internal void UpdatePerformanceInfo(float dt)
    {
        float elapsed = 1f * (game.platform.TimeMillisecondsFromStart() - lastPerformanceInfoupdateMilliseconds) / 1000;
        int triangles = TrianglesCount();
        if (elapsed >= 1)
        {
            lastPerformanceInfoupdateMilliseconds = game.platform.TimeMillisecondsFromStart();
            int chunkupdates_ = ChunkUpdates();
            game.performanceinfo.Set("chunk updates", game.platform.StringFormat(game.language.ChunkUpdates(), game.platform.IntToString(chunkupdates_ - lastchunkupdates)));
            lastchunkupdates = ChunkUpdates();
            game.performanceinfo.Set("triangles", game.platform.StringFormat(game.language.Triangles(), game.platform.IntToString(triangles)));
        }
    }

    public override void Dispose(Game game_)
    {
        Clear();
    }

    public void StartTerrain()
    {
        sqrt3half = game.platform.MathSqrt(3) / 2;
        game.d_TerrainChunkTesselator.Start();
        terrainRendererStarted = true;
        chunksize = Game.chunksize;
    }

    int chunksize;

    int mapAreaSize() { return game.platform.FloatToInt(game.d_Config3d.viewdistance) * 2; }
    int centerAreaSize() { return game.platform.FloatToInt(game.d_Config3d.viewdistance) / 2; }
    int mapAreaSizeZ() { return mapAreaSize(); }

    int mapsizexchunks() { return game.map.mapsizexchunks(); }
    int mapsizeychunks() { return game.map.mapsizeychunks(); }
    int mapsizezchunks() { return game.map.mapsizezchunks(); }

    public override void OnReadOnlyBackgroundThread(Game game_, float dt)
    {
        game = game_;
        UpdateTerrain();
        game_.QueueActionCommit(TerrainRendererCommit.Create(this));
    }

    public void MainThreadCommit()
    {
        for (int i = 0; i < redrawCount; i++)
        {
            DoRedraw(redraw[i]);
            redraw[i] = null;
        }
        redrawCount = 0;
    }

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
                    && xx < game.map.MapSizeX / chunksize && yy < game.map.MapSizeY / chunksize && zz < game.map.MapSizeZ / chunksize)
                {
                    Chunk chunk = game.map.chunks[Index3d(xx, yy, zz, mapsizexchunks(), mapsizeychunks())];
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

    const int intMaxValue = 2147483647;
    int[] tempnearestpos;
    void NearestDirty(int[] nearestpos)
    {
        int nearestdist = intMaxValue;
        nearestpos[0] = -1;
        nearestpos[1] = -1;
        nearestpos[2] = -1;
        int px = game.platform.FloatToInt(game.player.position.x) / chunksize;
        int py = game.platform.FloatToInt(game.player.position.z) / chunksize;
        int pz = game.platform.FloatToInt(game.player.position.y) / chunksize;

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
        int mapsizexchunks_ = mapsizexchunks();
        int mapsizeychunks_ = mapsizeychunks();

        for (int x = startx; x <= endx; x++)
        {
            for (int y = starty; y <= endy; y++)
            {
                for (int z = startz; z <= endz; z++)
                {
                    Chunk c = game.map.chunks[Index3d(x, y, z, mapsizexchunks_, mapsizeychunks_)];
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
        game.d_Batcher.Draw(game.player.position.x, game.player.position.y, game.player.position.z);
    }

    public void RedrawAllBlocks()
    {
        if (!terrainRendererStarted)
        {
            StartTerrain();
        }
        int chunksLength = (game.map.MapSizeX / chunksize)
            * (game.map.MapSizeY / chunksize)
            * (game.map.MapSizeZ / chunksize);
        for (int i = 0; i < chunksLength; i++)
        {
            Chunk c = game.map.chunks[i];
            if (c == null)
            {
                continue;
            }
            if (c.rendered == null)
            {
                c.rendered = new RenderedChunk();
            }
            c.rendered.dirty = true;
            c.baseLightDirty = true;
        }
    }

    int[] ids;
    int idsCount;
    void DoRedraw(TerrainRendererRedraw r)
    {
        idsCount = 0;
        Chunk c = r.c;
        if (c.rendered.ids != null)
        {
            for (int i = 0; i < c.rendered.idsCount; i++)
            {
                int loadedSubmesh = c.rendered.ids[i];
                game.d_Batcher.Remove(loadedSubmesh);
            }
        }
        for (int i = 0; i < r.dataCount; i++)
        {
            VerticesIndicesToLoad submesh = r.data[i];
            if (submesh.modelData.GetIndicesCount() != 0)
            {
                float centerVecX = submesh.positionX + chunksize / 2;
                float centerVecY = submesh.positionZ + chunksize / 2;
                float centerVecZ = submesh.positionY + chunksize / 2;
                float radius = sqrt3half * chunksize;
                ids[idsCount++] = game.d_Batcher.Add(submesh.modelData, submesh.transparent, submesh.texture, centerVecX, centerVecY, centerVecZ, radius);
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

    void RedrawChunk(int x, int y, int z)
    {
        Chunk c = game.map.chunks[MapUtilCi.Index3d(x, y, z, mapsizexchunks(), mapsizeychunks())];
        if (c == null)
        {
            return;
        }
        if (c.rendered == null)
        {
            c.rendered = new RenderedChunk();
        }
        c.rendered.dirty = false;
        chunkupdates++;

        GetExtendedChunk(x, y, z);

        TerrainRendererRedraw r = new TerrainRendererRedraw();
        r.c = c;

        VerticesIndicesToLoad[] a = null;
        IntRef retCount = new IntRef();
        if (!IsSolidChunk(currentChunk, (chunksize + 2) * (chunksize + 2) * (chunksize + 2)))
        {
            CalculateShadows(x, y, z);
            a = game.d_TerrainChunkTesselator.MakeChunk(x, y, z, currentChunk, currentChunkShadows, game.mLightLevels, retCount);
        }

        r.data = new VerticesIndicesToLoad[retCount.value];
        for (int i = 0; i < retCount.value; i++)
        {
            r.data[i] = VerticesIndicesToLoadClone(a[i]);
        }
        r.dataCount = retCount.value;
        redraw[redrawCount++] = r;
    }

    VerticesIndicesToLoad VerticesIndicesToLoadClone(VerticesIndicesToLoad source)
    {
        VerticesIndicesToLoad dest = new VerticesIndicesToLoad();
        dest.modelData = ModelDataClone(source.modelData);
        dest.positionX = source.positionX;
        dest.positionY = source.positionY;
        dest.positionZ = source.positionZ;
        dest.texture = source.texture;
        dest.transparent = source.transparent;
        return dest;
    }

    ModelData ModelDataClone(ModelData source)
    {
        ModelData dest = new ModelData();
        dest.xyz = new float[source.GetXyzCount()];
        for (int i = 0; i < source.GetXyzCount(); i++)
        {
            dest.xyz[i] = source.xyz[i];
        }
        dest.uv = new float[source.GetUvCount()];
        for (int i = 0; i < source.GetUvCount(); i++)
        {
            dest.uv[i] = source.uv[i];
        }
        dest.rgba = new byte[source.GetRgbaCount()];
        for (int i = 0; i < source.GetRgbaCount(); i++)
        {
            dest.rgba[i] = source.rgba[i];
        }
        dest.indices = new int[source.GetIndicesCount()];
        for (int i = 0; i < source.GetIndicesCount(); i++)
        {
            dest.indices[i] = source.indices[i];
        }
        dest.SetVerticesCount(source.GetVerticesCount());
        dest.SetIndicesCount(source.GetIndicesCount());
        return dest;
    }

    TerrainRendererRedraw[] redraw;
    int redrawCount;

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
        game.map.GetMapPortion(currentChunk, x * chunksize - 1, y * chunksize - 1, z * chunksize - 1,
            chunksize + 2, chunksize + 2, chunksize + 2);
    }

    int[] CalculateShadowslightRadius;
    bool[] CalculateShadowsisTransparentForLight;
    LightBase lightBase;
    LightBetweenChunks lightBetweenChunks;
    void CalculateShadows(int cx, int cy, int cz)
    {
        for (int i = 0; i < GlobalVar.MAX_BLOCKTYPES; i++)
        {
            if (game.blocktypes[i] == null)
            {
                continue;
            }
            CalculateShadowslightRadius[i] = game.blocktypes[i].LightRadius;
            CalculateShadowsisTransparentForLight[i] = IsTransparentForLight(i);
        }

        for (int xx = 0; xx < 3; xx++)
        {
            for (int yy = 0; yy < 3; yy++)
            {
                for (int zz = 0; zz < 3; zz++)
                {
                    int cx1 = cx + xx - 1;
                    int cy1 = cy + yy - 1;
                    int cz1 = cz + zz - 1;
                    if (!game.map.IsValidChunkPos(cx1, cy1, cz1))
                    {
                        continue;
                    }
                    Chunk c = game.map.GetChunk(cx1 * chunksize, cy1 * chunksize, cz1 * chunksize);
                    if (c.baseLightDirty)
                    {
                        lightBase.CalculateChunkBaseLight(game, cx1, cy1, cz1, CalculateShadowslightRadius, CalculateShadowsisTransparentForLight);
                        c.baseLightDirty = false;
                    }
                }
            }
        }

        Chunk chunk = game.map.GetChunk(cx * chunksize, cy * chunksize, cz * chunksize);

        if (chunk.rendered.light == null)
        {
            chunk.rendered.light = new byte[18 * 18 * 18];
            for (int i = 0; i < 18 * 18 * 18; i++)
            {
                chunk.rendered.light[i] = 15;
            }
        }
        
        lightBetweenChunks.CalculateLightBetweenChunks(game, cx, cy, cz, CalculateShadowslightRadius, CalculateShadowsisTransparentForLight);

        for (int i = 0; i < 18 * 18 * 18; i++)
        {
            currentChunkShadows[i] = chunk.rendered.light[i];
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

    internal void Clear()
    {
        game.d_Batcher.Clear();
    }
}

public class TerrainRendererCommit : Action_
{
    public static TerrainRendererCommit Create(ModDrawTerrain renderer)
    {
        TerrainRendererCommit c = new TerrainRendererCommit();
        c.renderer = renderer;
        return c;
    }
    ModDrawTerrain renderer;
    public override void Run()
    {
        renderer.MainThreadCommit();
    }
}

public class TerrainRendererRedraw
{
    internal Chunk c;
    internal VerticesIndicesToLoad[] data;
    internal int dataCount;
}

public class ModUnloadRendererChunks : ClientMod
{
    public ModUnloadRendererChunks()
    {
        unloadxyztemp = new Vector3IntRef();
    }

    Game game;
    public override void OnReadOnlyBackgroundThread(Game game_, float dt)
    {
        game = game_;

        chunksize = Game.chunksize;
        mapsizexchunks = game.map.MapSizeX / chunksize;
        mapsizeychunks = game.map.MapSizeY / chunksize;
        mapsizezchunks = game.map.MapSizeZ / chunksize;

        int px = game.platform.FloatToInt(game.player.position.x) / chunksize;
        int py = game.platform.FloatToInt(game.player.position.z) / chunksize;
        int pz = game.platform.FloatToInt(game.player.position.y) / chunksize;

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
        if (endx >= mapsizexchunks) { endx = mapsizexchunks - 1; }
        if (endy >= mapsizeychunks) { endy = mapsizeychunks - 1; }
        if (endz >= mapsizezchunks) { endz = mapsizezchunks - 1; }

        int mapsizexchunks_ = mapsizexchunks;
        int mapsizeychunks_ = mapsizeychunks;
        int mapsizezchunks_ = mapsizezchunks;

        int count;
        if (game.platform.IsFastSystem())
        {
            count = 1000;
        }
        else
        {
            count = 250;
        }

        for (int i = 0; i < count; i++)
        {
            unloadIterationXy++;
            if (unloadIterationXy >= mapsizexchunks_ * mapsizeychunks_ * mapsizezchunks_)
            {
                unloadIterationXy = 0;
            }
            MapUtilCi.PosInt(unloadIterationXy, mapsizexchunks_, mapsizeychunks_, unloadxyztemp);
            int x = unloadxyztemp.X;
            int y = unloadxyztemp.Y;
            int z = unloadxyztemp.Z;
            int pos = MapUtilCi.Index3d(x, y, z, mapsizexchunks_, mapsizeychunks_);
            bool unloaded = false;

            Chunk c = game.map.chunks[pos];
            if (c == null
                || c.rendered == null
                || c.rendered.ids == null)
            {
                continue;
            }
            if (x < startx || y < starty || z < startz
                || x > endx || y > endy || z > endz)
            {
                int unloadChunkPos = pos;

                UnloadRendererChunksCommit commit = new UnloadRendererChunksCommit();
                commit.game = game;
                commit.unloadChunkPos = unloadChunkPos;
                game.QueueActionCommit(commit);
            }
            unloaded = true;
            if (unloaded)
            {
                break;
            }
        }
    }

    int mapAreaSize() { return game.platform.FloatToInt(game.d_Config3d.viewdistance) * 2; }
    int centerAreaSize() { return game.platform.FloatToInt(game.d_Config3d.viewdistance) / 2; }
    int mapAreaSizeZ() { return mapAreaSize(); }

    int mapsizexchunks;
    int mapsizeychunks;
    int mapsizezchunks;

    int chunksize;

    int unloadIterationXy;
    Vector3IntRef unloadxyztemp;
}

public class UnloadRendererChunksCommit : Action_
{
    internal Game game;
    internal int unloadChunkPos;
    public override void Run()
    {
        if (unloadChunkPos != -1)
        {
            Chunk c = game.map.chunks[unloadChunkPos];
            for (int k = 0; k < c.rendered.idsCount; k++)
            {
                int loadedSubmesh = c.rendered.ids[k];
                game.d_Batcher.Remove(loadedSubmesh);
            }
            c.rendered.ids = null;
            c.rendered.dirty = true;
            c.rendered.light = null;

            unloadChunkPos = -1;
        }
    }
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
