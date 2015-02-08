public class Game
{
    public Game()
    {
        one = 1;
        performanceinfo = new DictionaryStringString();
        AudioEnabled = true;
        AutoJumpEnabled = false;
        playerPositionSpawnX = 15 + one / 2;
        playerPositionSpawnY = 64;
        playerPositionSpawnZ = 15 + one / 2;

        TextureId = new int[MaxBlockTypes][];
        for (int i = 0; i < MaxBlockTypes; i++)
        {
            TextureId[i] = new int[6];
        }
        TextureIdForInventory = new int[MaxBlockTypes];
        language = new Language();
        lastplacedblockX = -1;
        lastplacedblockY = -1;
        lastplacedblockZ = -1;
        mLightLevels = new float[16];
        sunlight_ = 15;
        mvMatrix = new StackMatrix4();
        pMatrix = new StackMatrix4();
        mvMatrix.Push(Mat4.Create());
        pMatrix.Push(Mat4.Create());
        whitetexture = -1;
        cachedTextTexturesMax = 1024;
        cachedTextTextures = new CachedTextTexture[cachedTextTexturesMax];
        packetLen = new IntRef();
        ENABLE_DRAW2D = true;
        AllowFreemove = true;
        enableCameraControl = true;
        textures = new DictionaryStringInt1024();
        ServerInfo = new ServerInformation();
        menustate = new MenuState();
        mouseleftclick = false;
        mouseleftdeclick = false;
        wasmouseleft = false;
        mouserightclick = false;
        mouserightdeclick = false;
        wasmouseright = false;
        ENABLE_LAG = 0;
        znear = one / 10;
        CameraMatrix = new GetCameraMatrix();
        ENABLE_ZFAR = true;
        TotalAmmo = new int[GlobalVar.MAX_BLOCKTYPES];
        LoadedAmmo = new int[GlobalVar.MAX_BLOCKTYPES];
        AllowedFontsCount = 1;
        AllowedFonts = new string[AllowedFontsCount];
        AllowedFonts[0] = "Verdana";
        fov = Game.GetPi() / 3;
        cameratype = CameraType.Fpp;
        ENABLE_TPP_VIEW = false;
        basemovespeed = 5;
        movespeed = 5;
        RadiusWhenMoving = one * 3 / 10;
        playervelocity = new Vector3Ref();
        LocalPlayerId = -1;
        dialogs = new VisibleDialog[512];
        dialogsCount = 512;
        blockHealth = new DictionaryVector3Float();
        playertexturedefault = -1;
        a = new AnimationState();
        constRotationSpeed = one * 180 / 20;
        modmanager = new ClientModManager1();
        particleEffectBlockBreak = new ModDrawParticleEffectBlockBreak();
        PICK_DISTANCE = one * 37 / 10;
        selectedmodelid = -1;
        grenadetime = 3;
        rotationspeed = one * 15 / 100;
        entities = new Entity[entitiesMax];
        entitiesCount = 512;
        PlayerPushDistance = 2;
        keyboardState = new bool[256];
        overheadcameradistance = 10;
        tppcameradistance = 3;
        TPP_CAMERA_DISTANCE_MIN = 1;
        TPP_CAMERA_DISTANCE_MAX = 10;
        options = new OptionsCi();
        overheadcameraK = new Kamera();
        fillarea = new DictionaryVector3Float();
        fillAreaLimit = 200;
        speculativeCount = 0;
        speculative = new Speculative[speculativeMax];
        typinglog = new string[1024 * 16];
        typinglogCount = 0;
        NewBlockTypes = new Packet_BlockType[GlobalVar.MAX_BLOCKTYPES];
        localplayeranim = new AnimationState();
        localplayeranimationhint = new AnimationHint();
        enable_move = true;
        handTexture = -1;
        modelViewInverted = new float[16];
        GLScaleTempVec3 = Vec3.Create();
        GLRotateTempVec3 = Vec3.Create();
        GLTranslateTempVec3 = Vec3.Create();
        identityMatrix = Mat4.Identity_(Mat4.Create());
        Set3dProjectionTempMat4 = Mat4.Create();
        getAsset = new string[1024 * 2];
        PlayerStats = new Packet_ServerPlayerStats();
        mLightLevels = new float[16];
        for (int i = 0; i < 16; i++)
        {
            mLightLevels[i] = one * i / 15;
        }
        scheduler = new TaskScheduler_();
        soundnow = new BoolRef();
        acceleration = new Acceleration();
        camera = Mat4.Create();
        packetHandlers = new ClientPacketHandler[256];
        player = new Entity();
        player.position = new EntityPosition_();
        player.physicsState = new CharacterPhysicsState();
        currentlyAttackedEntity = -1;
        ChatLinesMax = 1;
        ChatLines = new Chatline[ChatLinesMax];
        ChatLineLength = 64;
        audio = new AudioControl();
    }

    internal AssetList assets;
    internal FloatRef assetsLoadProgress;
    internal TextColorRenderer textColorRenderer;
    internal AudioControl audio;

    public void Start()
    {
        textColorRenderer = new TextColorRenderer();
        textColorRenderer.platform = platform;
        language.platform = platform;
        language.LoadTranslations();
        GameData gamedata = new GameData();
        gamedata.Start();
        Config3d config3d = new Config3d();
        if (platform.IsFastSystem())
        {
            config3d.viewdistance = 128;
        }
        else
        {
            config3d.viewdistance = 32;
        }
        CharacterPhysicsCi physics = new CharacterPhysicsCi();
        //network.d_ResetMap = this;
        ITerrainTextures terrainTextures = new ITerrainTextures();
        terrainTextures.game = this;
        d_TextureAtlasConverter = new TextureAtlasConverter();
        d_TerrainTextures = terrainTextures;
        //InfiniteMapChunked map = new InfiniteMapChunked();// { generator = new WorldGeneratorDummy() };

        FrustumCulling frustumculling = new FrustumCulling();
        frustumculling.d_GetCameraMatrix = this.CameraMatrix;
        frustumculling.platform = platform;
        d_FrustumCulling = frustumculling;

        TerrainChunkTesselatorCi terrainchunktesselator = new TerrainChunkTesselatorCi();
        d_TerrainChunkTesselator = terrainchunktesselator;
        d_Batcher = new MeshBatcher();
        d_Batcher.d_FrustumCulling = frustumculling;
        d_Batcher.game = this;
        d_FrustumCulling = frustumculling;
        //w.d_Map = clientgame.mapforphysics;
        d_Physics = physics;
        d_Data = gamedata;
        d_DataMonsters = new GameDataMonsters();
        d_Config3d = config3d;

        ModDrawParticleEffectBlockBreak particle = new ModDrawParticleEffectBlockBreak();
        this.particleEffectBlockBreak = particle;
        this.d_Data = gamedata;
        d_TerrainTextures = terrainTextures;

        this.Reset(10 * 1000, 10 * 1000, 128);

        //w.d_CurrentShadows = this;
        SunMoonRenderer sunmoonrenderer = new SunMoonRenderer();
        d_SunMoonRenderer = sunmoonrenderer;
        d_SunMoonRenderer = sunmoonrenderer;
        d_Heightmap = new InfiniteMapChunked2d();
        d_Heightmap.d_Map = this;
        d_Heightmap.Restart();
        //this.light = new InfiniteMapChunkedSimple() { d_Map = map };
        //light.Restart();
        d_TerrainChunkTesselator = terrainchunktesselator;
        terrainchunktesselator.game = this;

        //if (fullshadows)
        //{
        //    UseShadowsFull();
        //}
        //else
        //{
        //    UseShadowsSimple();
        //}
        Packet_Inventory inventory = new Packet_Inventory();
        inventory.RightHand = new Packet_Item[10];
        terrainRenderer = new TerrainRenderer();
        terrainRenderer.game = this;
        GameDataItemsClient dataItems = new GameDataItemsClient();
        dataItems.game = this;
        InventoryUtilClient inventoryUtil = new InventoryUtilClient();
        d_Inventory = inventory;
        d_InventoryUtil = inventoryUtil;
        inventoryUtil.d_Inventory = inventory;
        inventoryUtil.d_Items = dataItems;
        d_Physics.game = this;
        d_Inventory = inventory;
        platform.AddOnCrash(OnCrashHandlerLeave.Create(this));

        rnd = platform.RandomCreate();

        clientmods = new ClientMod[128];
        clientmodsCount = 0;
        modmanager.game = this;
        AddMod(new ModAutoCamera());
        AddMod(new ModFpsHistoryGraph());
        AddMod(new ModWalkSound());
        AddMod(new ModFallDamageToPlayer());
        AddMod(new ModBlockDamageToPlayer());
        AddMod(new ModLoadPlayerTextures());
        AddMod(new ModSendPosition());
        AddMod(new ModInterpolatePositions());
        AddMod(new ModRail());

        AddMod(new ModCompass());
        AddMod(new ModGrenade());
        AddMod(new ModBullet());
        AddMod(new ModExpire());
        AddMod(new ModReloadAmmo());
        AddMod(new ModPush());
        if (platform.IsFastSystem())
        {
            AddMod(new ModSkySphereAnimated());
        }
        else
        {
            AddMod(new ModSkySphereStatic());
        }
        AddMod(sunmoonrenderer);
        AddMod(new ModDrawTestModel());
        AddMod(new ModDrawLinesAroundSelectedBlock());
        AddMod(new ModDrawArea());
        AddMod(new ModDrawTerrain());
        AddMod(new ModDrawPlayers());
        AddMod(new ModDrawPlayerNames());
        AddMod(new ModDrawText());
        AddMod(new ModDrawParticleEffectBlockBreak());
        AddMod(new ModDrawSprites());
        AddMod(new ModDrawMinecarts());
        AddMod(new ModDrawHand2d());
        AddMod(new ModDrawHand3d());
        AddMod(new ModGuiCrafting());
        AddMod(new ModDialog());
        AddMod(new ModPicking());
        AddMod(new ModClearInactivePlayersDrawInfo());
        AddMod(new ModCameraKeys());
        AddMod(new ModTrampoline());
        AddMod(new ModSlipperyWalk());
        AddMod(new ModNoAirControl());
        AddMod(new ModCharacterPhysics());
        AddMod(new ModSendActiveMaterial());
        AddMod(new ModCamera());
        AddMod(new ModNetworkEntity());
        AddMod(new ModGuiChat());
        AddMod(new ModGuiInventory());
        AddMod(new ModGuiTouchButtons());
        AddMod(new ModGuiEscapeMenu());
        AddMod(new ModGuiMapLoading());
        AddMod(new ModDraw2dMisc());
        AddMod(new ModScreenshot());
        AddMod(new ModAudio());

        s = new BlockOctreeSearcher();
        s.platform = platform;

        scheduler.Start(platform);
        DrawTask drawTask = new DrawTask();
        drawTask.game = this;
        QueueTaskReadOnlyMainThread(drawTask);

        UnloadRendererChunks unloadRendererChunks = new UnloadRendererChunks();
        unloadRendererChunks.game = this;
        QueueTaskReadOnlyMainThread(unloadRendererChunks);

        QueueTaskReadOnlyMainThread(terrainRenderer);

        UpdateTask update = new UpdateTask();
        update.game = this;
        QueueTaskCommit(update);

        NetworkProcessTask networkProcessTask = new NetworkProcessTask();
        networkProcessTask.game = this;
        QueueTaskReadOnlyBackgroundPerFrame(networkProcessTask);

        //Prevent loding screen from immediately displaying lag symbol
        LastReceivedMilliseconds = platform.TimeMillisecondsFromStart();

        ENABLE_DRAW_TEST_CHARACTER = platform.IsDebuggerAttached();
    }

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    void AddMod(ClientMod mod)
    {
        clientmods[clientmodsCount++] = mod;
        mod.Start(modmanager);
    }

    internal float one;

    const int MaxBlockTypes = 1024;

    internal GamePlatform platform;
    internal Packet_BlockType[] blocktypes;
    internal Language language;
    internal TerrainChunkTesselatorCi d_TerrainChunkTesselator;

    internal Chunk[] chunks;
    internal int MapSizeX;
    internal int MapSizeY;
    internal int MapSizeZ;
    internal const int chunksize = 16;
    internal const int chunksizebits = 4;

    internal Entity player;

    public bool IsFluid(Packet_BlockType block)
    {
        return block.DrawType == Packet_DrawTypeEnum.Fluid;
    }

    public bool IsRail(Packet_BlockType block)
    {
        return block.Rail > 0;	//Does not include Rail0, but this can't be placed.
    }

    public bool IsEmptyForPhysics(Packet_BlockType block)
    {
        return (block.DrawType == Packet_DrawTypeEnum.Ladder)
            || (block.WalkableType != Packet_WalkableTypeEnum.Solid && block.WalkableType != Packet_WalkableTypeEnum.Fluid);
    }

    public int GetBlock(int x, int y, int z)
    {
        if (!IsValidPos(x, y, z))
        {
            return 0;
        }
        return GetBlockValid(x, y, z);
    }

    public int GetBlockValid(int x, int y, int z)
    {
        int cx = x >> chunksizebits;
        int cy = y >> chunksizebits;
        int cz = z >> chunksizebits;
        int chunkpos = Index3d(cx, cy, cz, MapSizeX >> chunksizebits, MapSizeY >> chunksizebits);
        if (chunks[chunkpos] == null)
        {
            return 0;
        }
        else
        {
            int pos = Index3d(x & (chunksize - 1), y & (chunksize - 1), z & (chunksize - 1), chunksize, chunksize);
            return GetBlockInChunk(chunks[chunkpos], pos);
        }
    }

    public int GetBlockInChunk(Chunk chunk, int pos)
    {
        if (chunk.dataInt != null)
        {
            return chunk.dataInt[pos];
        }
        else
        {
            return chunk.data[pos];
        }
    }

    public void SetBlockRaw(int x, int y, int z, int tileType)
    {
        Chunk chunk = GetChunk(x, y, z);
        int pos = Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize);
        SetBlockInChunk(chunk, pos, tileType);
    }

    public void SetBlockInChunk(Chunk chunk, int pos, int block)
    {
        if (chunk.dataInt == null)
        {
            if (block < 255)
            {
                chunk.data[pos] = IntToByte(block);
            }
            else
            {
                int n = chunksize * chunksize * chunksize;
                chunk.dataInt = new int[n];
                for (int i = 0; i < n; i++)
                {
                    chunk.dataInt[i] = chunk.data[i];
                }
                chunk.data = null;

                chunk.dataInt[pos] = block;
            }
        }
        else
        {
            chunk.dataInt[pos] = block;
        }
    }

    internal bool ChunkHasData(Chunk chunk)
    {
        return chunk.data != null || chunk.dataInt != null;
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        x = x / chunksize;
        y = y / chunksize;
        z = z / chunksize;
        int mapsizexchunks = MapSizeX / chunksize;
        int mapsizeychunks = MapSizeY / chunksize;
        Chunk chunk = chunks[Index3d(x, y, z, mapsizexchunks, mapsizeychunks)];
        if (chunk == null)
        {
            Chunk c = new Chunk();
            c.data = new byte[chunksize * chunksize * chunksize];
            chunks[Index3d(x, y, z, mapsizexchunks, mapsizeychunks)] = c;
            return chunks[Index3d(x, y, z, mapsizexchunks, mapsizeychunks)];
        }
        return chunk;
    }

    public bool IsValidPos(int x, int y, int z)
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

    public int blockheight(int x, int y, int z_)
    {
        for (int z = z_; z >= 0; z--)
        {
            if (GetBlock(x, y, z) != 0)
            {
                return z + 1;
            }
        }
        return 0;
    }

    public bool IsValidChunkPos(int cx, int cy, int cz, int chunksize_)
    {
        return cx >= 0 && cy >= 0 && cz >= 0
            && cx < MapSizeX / chunksize_
            && cy < MapSizeY / chunksize_
            && cz < MapSizeZ / chunksize_;
    }

    public void CopyChunk(Chunk chunk, int[] output)
    {
        int n = chunksize * chunksize * chunksize;
        if (chunk.dataInt != null)
        {
            for (int i = 0; i < n; i++)
            {
                output[i] = chunk.dataInt[i];
            }
        }
        else
        {
            for (int i = 0; i < n; i++)
            {
                output[i] = chunk.data[i];
            }
        }
    }

    public static byte IntToByte(int a)
    {
#if CITO
        return a.LowByte;
#else
        return (byte)a;
#endif
    }

    public static int ColorFromArgb(int a, int r, int g, int b)
    {
        int iCol = (a << 24) | (r << 16) | (g << 8) | b;
        return iCol;
    }

    public static int ColorA(int color)
    {
        byte a = IntToByte(color >> 24);
        return a;
    }

    public static int ColorR(int color)
    {
        byte r = IntToByte(color >> 16);
        return r;
    }

    public static int ColorG(int color)
    {
        byte g = IntToByte(color >> 8);
        return g;
    }

    public static int ColorB(int color)
    {
        byte b = IntToByte(color);
        return b;
    }

    public static float GetPi()
    {
        float a = 3141592;
        return a / 1000000;
    }

    //Indexed by block id and TileSide.
    internal int[][] TextureId;
    internal int[] TextureIdForInventory;

    internal int terrainTexturesPerAtlas;

    public void GetMapPortion(int[] outPortion, int x, int y, int z, int portionsizex, int portionsizey, int portionsizez)
    {
        int outPortionCount = portionsizex * portionsizey * portionsizez;
        for (int i = 0; i < outPortionCount; i++)
        {
            outPortion[i] = 0;
        }

        //int chunksizebits = p.FloatToInt(p.MathLog(chunksize, 2));
        if (chunksize != 16)
        {
            platform.ThrowException("GetMapPortion");
        }

        int mapchunksx = MapSizeX / chunksize;
        int mapchunksy = MapSizeY / chunksize;
        int mapchunksz = MapSizeZ / chunksize;
        int mapsizechunks = mapchunksx * mapchunksy * mapchunksz;

        for (int xx = 0; xx < portionsizex; xx++)
        {
            for (int yy = 0; yy < portionsizey; yy++)
            {
                for (int zz = 0; zz < portionsizez; zz++)
                {
                    //Find chunk.
                    int cx = (x + xx) >> chunksizebits;
                    int cy = (y + yy) >> chunksizebits;
                    int cz = (z + zz) >> chunksizebits;
                    //int cpos = MapUtil.Index3d(cx, cy, cz, MapSizeX / chunksize, MapSizeY / chunksize);
                    int cpos = (cz * mapchunksy + cy) * mapchunksx + cx;
                    //if (cpos < 0 || cpos >= ((MapSizeX / chunksize) * (MapSizeY / chunksize) * (MapSizeZ / chunksize)))
                    if (cpos < 0 || cpos >= mapsizechunks)
                    {
                        continue;
                    }
                    Chunk chunk = chunks[cpos];
                    if (chunk == null || !ChunkHasData(chunk))
                    {
                        continue;
                    }
                    //int pos = MapUtil.Index3d((x + xx) % chunksize, (y + yy) % chunksize, (z + zz) % chunksize, chunksize, chunksize);
                    int chunkGlobalX = cx << chunksizebits;
                    int chunkGlobalY = cy << chunksizebits;
                    int chunkGlobalZ = cz << chunksizebits;

                    int inChunkX = (x + xx) - chunkGlobalX;
                    int inChunkY = (y + yy) - chunkGlobalY;
                    int inChunkZ = (z + zz) - chunkGlobalZ;

                    //int pos = MapUtil.Index3d(inChunkX, inChunkY, inChunkZ, chunksize, chunksize);
                    int pos = (((inChunkZ << chunksizebits) + inChunkY) << chunksizebits) + inChunkX;

                    int block = GetBlockInChunk(chunk, pos);
                    //outPortion[MapUtil.Index3d(xx, yy, zz, portionsizex, portionsizey)] = (byte)block;
                    outPortion[(zz * portionsizey + yy) * portionsizex + xx] = block;
                }
            }
        }
    }
    internal int texturesPacked() { return GlobalVar.MAX_BLOCKTYPES_SQRT; } //16x16
    internal int terrainTexture;
    internal int[] terrainTextures1d;
    internal ITerrainTextures d_TerrainTextures;

    internal int lastplacedblockX;
    internal int lastplacedblockY;
    internal int lastplacedblockZ;

    internal InfiniteMapChunked2d d_Heightmap;
    internal Config3d d_Config3d;

    //maps light level (0-15) to GL.Color value.
    internal float[] mLightLevels;
    internal MeshBatcher d_Batcher;
    internal int sunlight_;

    public void Draw2dTexture(int textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int atlastextures, int color, bool enabledepthtest)
    {
        if (color == ColorFromArgb(255, 255, 255, 255) && inAtlasId == null)
        {
            Draw2dTextureSimple(textureid, x1, y1, width, height, enabledepthtest);
        }
        else
        {
            Draw2dTextureInAtlas(textureid, x1, y1, width, height, inAtlasId, atlastextures, color, enabledepthtest);
        }
    }

    Model quadModel;
    void Draw2dTextureSimple(int textureid, float x1, float y1, float width, float height, bool enabledepthtest)
    {
        RectFRef rect = RectFRef.Create(0, 0, 1, 1);
        platform.GlDisableCullFace();
        platform.GlEnableTexture2d();
        platform.BindTexture2d(textureid);

        if (!enabledepthtest)
        {
            platform.GlDisableDepthTest();
        }
        if (quadModel == null)
        {
            quadModel = platform.CreateModel(QuadModelData.GetQuadModelData());
        }
        GLPushMatrix();
        GLTranslate(x1, y1, 0);
        GLScale(width, height, 0);
        GLScale(one / 2, one / 2, 0);
        GLTranslate(one, one, 0);

        DrawModel(quadModel);
        GLPopMatrix();

        if (!enabledepthtest)
        {
            platform.GlEnableDepthTest();
        }
        platform.GlEnableCullFace();
        platform.GlEnableTexture2d();
    }

    void Draw2dTextureInAtlas(int textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int atlastextures, int color, bool enabledepthtest)
    {
        RectFRef rect = RectFRef.Create(0, 0, 1, 1);
        if (inAtlasId != null)
        {
            TextureAtlasCi.TextureCoords2d(inAtlasId.value, atlastextures, rect);
        }
        platform.GlDisableCullFace();
        platform.GlEnableTexture2d();
        platform.BindTexture2d(textureid);

        if (!enabledepthtest)
        {
            platform.GlDisableDepthTest();
        }
        ModelData data = QuadModelData.GetQuadModelData2(rect.x, rect.y, rect.w, rect.h,
            x1, y1, width, height, Game.IntToByte(Game.ColorR(color)), Game.IntToByte(Game.ColorG(color)), Game.IntToByte(Game.ColorB(color)), Game.IntToByte(Game.ColorA(color)));
        DrawModelData(data);
        if (!enabledepthtest)
        {
            platform.GlEnableDepthTest();
        }
        platform.GlEnableCullFace();
        platform.GlEnableTexture2d();
    }

    public ModelData CombineModelData(ModelData[] modelDatas, int count)
    {
        ModelData ret = new ModelData();
        int totalIndices = 0;
        int totalVertices = 0;
        for (int i = 0; i < count; i++)
        {
            ModelData m = modelDatas[i];
            totalIndices += m.indicesCount;
            totalVertices += m.verticesCount;
        }
        ret.indices = new int[totalIndices];
        ret.xyz = new float[totalVertices * 3];
        ret.uv = new float[totalVertices * 2];
        ret.rgba = new byte[totalVertices * 4];

        for (int i = 0; i < count; i++)
        {
            ModelData m = modelDatas[i];
            int retVerticesCount = ret.verticesCount;
            int retIndicesCount = ret.indicesCount;
            for (int k = 0; k < m.indicesCount; k++)
            {
                ret.indices[ret.indicesCount++] = m.indices[k] + retVerticesCount;
            }
            for (int k = 0; k < m.verticesCount * 3; k++)
            {
                ret.xyz[retVerticesCount * 3 + k] = m.xyz[k];
            }
            for (int k = 0; k < m.verticesCount * 2; k++)
            {
                ret.uv[retVerticesCount * 2 + k] = m.uv[k];
            }
            for (int k = 0; k < m.verticesCount * 4; k++)
            {
                ret.rgba[retVerticesCount * 4 + k] = m.rgba[k];
            }
            ret.verticesCount += m.verticesCount;
        }
        return ret;
    }

    public void Draw2dTextures(Draw2dData[] todraw, int todrawLength, int textureid)
    {
        ModelData[] modelDatas = new ModelData[512];
        int modelDatasCount = 0;
        for (int i = 0; i < todrawLength; i++)
        {
            Draw2dData d = todraw[i];
            float x1 = d.x1;
            float y1 = d.y1;
            float width = d.width;
            float height = d.height;
            IntRef inAtlasId = d.inAtlasId;
            int textureId = textureid;
            int color = d.color;

            RectFRef rect = RectFRef.Create(0, 0, 1, 1);
            if (inAtlasId != null)
            {
                TextureAtlasCi.TextureCoords2d(inAtlasId.value, texturesPacked(), rect);
            }

            ModelData modelData =
                QuadModelData.GetQuadModelData2(rect.x, rect.y, rect.w, rect.h,
                x1, y1, width, height, Game.IntToByte(ColorR(color)), Game.IntToByte(ColorG(color)), Game.IntToByte(ColorB(color)), Game.IntToByte(ColorA(color)));
            modelDatas[modelDatasCount++] = modelData;
        }

        ModelData combined = CombineModelData(modelDatas, modelDatasCount);

        platform.GlDisableCullFace();
        platform.GlEnableTexture2d();
        platform.BindTexture2d(textureid);

        platform.GlDisableDepthTest();

        DrawModelData(combined);

        platform.GlEnableDepthTest();

        platform.GlDisableCullFace();
        platform.GlEnableTexture2d();
    }

    internal bool currentMatrixModeProjection;
    internal StackMatrix4 mvMatrix;
    internal StackMatrix4 pMatrix;

    public void GLMatrixModeModelView()
    {
        currentMatrixModeProjection = false;
    }

    public void GLMatrixModeProjection()
    {
        currentMatrixModeProjection = true;
    }

    public void SetMatrixUniforms()
    {
        platform.SetMatrixUniformProjection(pMatrix.Peek());
        platform.SetMatrixUniformModelView(mvMatrix.Peek());
    }

    public void SetMatrixUniformProjection()
    {
        platform.SetMatrixUniformProjection(pMatrix.Peek());
    }

    public void SetMatrixUniformModelView()
    {
        platform.SetMatrixUniformModelView(mvMatrix.Peek());
    }

    public void GLLoadMatrix(float[] m)
    {
        if (currentMatrixModeProjection)
        {
            if (pMatrix.Count() > 0)
            {
                pMatrix.Pop();
            }
            pMatrix.Push(m);
        }
        else
        {
            if (mvMatrix.Count() > 0)
            {
                mvMatrix.Pop();
            }
            mvMatrix.Push(m);
        }
    }

    public void GLPopMatrix()
    {
        if (currentMatrixModeProjection)
        {
            if (pMatrix.Count() > 1)
            {
                pMatrix.Pop();
            }
        }
        else
        {
            if (mvMatrix.Count() > 1)
            {
                mvMatrix.Pop();
            }
        }
    }

    float[] GLScaleTempVec3;
    public void GLScale(float x, float y, float z)
    {
        float[] m;
        if (currentMatrixModeProjection)
        {
            m = pMatrix.Peek();
        }
        else
        {
            m = mvMatrix.Peek();
        }
        Vec3.Set(GLScaleTempVec3, x, y, z);
        Mat4.Scale(m, m, GLScaleTempVec3);
    }

    float[] GLRotateTempVec3;
    public void GLRotate(float angle, float x, float y, float z)
    {
        angle /= 360;
        angle *= 2 * Game.GetPi();
        float[] m;
        if (currentMatrixModeProjection)
        {
            m = pMatrix.Peek();
        }
        else
        {
            m = mvMatrix.Peek();
        }
        Vec3.Set(GLRotateTempVec3, x, y, z);
        Mat4.Rotate(m, m, angle, GLRotateTempVec3);
    }

    float[] GLTranslateTempVec3;
    public void GLTranslate(float x, float y, float z)
    {
        float[] m;
        if (currentMatrixModeProjection)
        {
            m = pMatrix.Peek();
        }
        else
        {
            m = mvMatrix.Peek();
        }
        Vec3.Set(GLTranslateTempVec3, x, y, z);
        Mat4.Translate(m, m, GLTranslateTempVec3);
    }

    public void GLPushMatrix()
    {
        if (currentMatrixModeProjection)
        {
            pMatrix.Push(pMatrix.Peek());
        }
        else
        {
            mvMatrix.Push(mvMatrix.Peek());
        }
    }

    float[] identityMatrix;
    public void GLLoadIdentity()
    {
        if (currentMatrixModeProjection)
        {
            if (pMatrix.Count() > 0)
            {
                pMatrix.Pop();
            }
            pMatrix.Push(identityMatrix);
        }
        else
        {
            if (mvMatrix.Count() > 0)
            {
                mvMatrix.Pop();
            }
            mvMatrix.Push(identityMatrix);
        }
    }

    public void GLOrtho(float left, float right, float bottom, float top, float zNear, float zFar)
    {
        if (currentMatrixModeProjection)
        {
            float[] m = pMatrix.Peek();
            Mat4.Ortho(m, left, right, bottom, top, zNear, zFar);
        }
        else
        {
            platform.ThrowException("GLOrtho");
        }
    }

    public void OrthoMode(int width, int height)
    {
        //GL.Disable(EnableCap.DepthTest);
        GLMatrixModeProjection();
        GLPushMatrix();
        GLLoadIdentity();
        GLOrtho(0, width, height, 0, 0, 1);
        SetMatrixUniformProjection();

        GLMatrixModeModelView();
        GLPushMatrix();
        GLLoadIdentity();
        SetMatrixUniformModelView();
    }

    public void PerspectiveMode()
    {
        // Enter into our projection matrix mode
        GLMatrixModeProjection();
        // Pop off the last matrix pushed on when in projection mode (Get rid of ortho mode)
        GLPopMatrix();
        SetMatrixUniformProjection();

        // Go back to our model view matrix like normal
        GLMatrixModeModelView();
        GLPopMatrix();
        SetMatrixUniformModelView();
        //GL.LoadIdentity();
        //GL.Enable(EnableCap.DepthTest);
    }

    public int WhiteTexture()
    {
        if (this.whitetexture == -1)
        {
            BitmapCi bmp = platform.BitmapCreate(1, 1);
            int[] pixels = new int[1];
            pixels[0] = ColorFromArgb(255, 255, 255, 255);
            platform.BitmapSetPixelsArgb(bmp, pixels);
            this.whitetexture = platform.LoadTextureFromBitmap(bmp);
        }
        return this.whitetexture;
    }
    int whitetexture;

    public float getblockheight(int x, int y, int z)
    {
        float RailHeight = one * 3 / 10;
        if (!IsValidPos(x, y, z))
        {
            return 1;
        }
        if (blocktypes[GetBlock(x, y, z)].Rail != 0)
        {
            return RailHeight;
        }
        if (blocktypes[GetBlock(x, y, z)].DrawType == Packet_DrawTypeEnum.HalfHeight)
        {
            return one / 2;
        }
        if (blocktypes[GetBlock(x, y, z)].DrawType == Packet_DrawTypeEnum.Flat)
        {
            return one / 20;
        }
        return 1;
    }

    internal CachedTextTexture[] cachedTextTextures;
    internal int cachedTextTexturesMax;

    public void DeleteUnusedCachedTextTextures()
    {
        int now = platform.TimeMillisecondsFromStart();
        for (int i = 0; i < cachedTextTexturesMax; i++)
        {
            CachedTextTexture t = cachedTextTextures[i];
            if (t == null)
            {
                continue;
            }
            if ((one * (now - t.texture.lastuseMilliseconds) / 1000) > 1)
            {
                platform.GLDeleteTexture(t.texture.textureId);
                cachedTextTextures[i] = null;
            }
        }
    }

    CachedTexture GetCachedTextTexture(Text_ t)
    {
        for (int i = 0; i < cachedTextTexturesMax; i++)
        {
            CachedTextTexture ct = cachedTextTextures[i];
            if (ct == null)
            {
                continue;
            }
            if (ct.text.Equals_(t))
            {
                return ct.texture;
            }
        }
        return null;
    }
    
    public void UpdateTextRendererFont()
    {
        platform.SetTextRendererFont(Font);
    }

    public void Draw2dText(string text, FontCi font, float x, float y, IntRef color, bool enabledepthtest)
    {
        if (text == null || platform.StringTrim(text) == "")
        {
            return;
        }
        if (color == null) { color = IntRef.Create(Game.ColorFromArgb(255, 255, 255, 255)); }
        Text_ t = new Text_();
        t.text = text;
        t.color = color.value;
        t.fontsize = font.size;
        t.fontfamily = font.family;
        t.fontstyle = font.style;
        CachedTexture ct;

        if (GetCachedTextTexture(t) == null)
        {
            ct = MakeTextTexture(t);
            if (ct == null)
            {
                return;
            }
            for (int i = 0; i < cachedTextTexturesMax; i++)
            {
                if (cachedTextTextures[i] == null)
                {
                    CachedTextTexture ct1 = new CachedTextTexture();
                    ct1.text = t;
                    ct1.texture = ct;
                    cachedTextTextures[i] = ct1;
                    break;
                }
            }
        }

        ct = GetCachedTextTexture(t);
        ct.lastuseMilliseconds = platform.TimeMillisecondsFromStart();
        platform.GLDisableAlphaTest();
        Draw2dTexture(ct.textureId, x, y, ct.sizeX, ct.sizeY, null, 0, Game.ColorFromArgb(255, 255, 255, 255), enabledepthtest);
        platform.GLEnableAlphaTest();
        DeleteUnusedCachedTextTextures();
    }

    CachedTexture MakeTextTexture(Text_ t)
    {
        CachedTexture ct = new CachedTexture();
        BitmapCi bmp = textColorRenderer.CreateTextTexture(t);
        ct.sizeX = platform.BitmapGetWidth(bmp);
        ct.sizeY = platform.BitmapGetHeight(bmp);
        ct.textureId = platform.LoadTextureFromBitmap(bmp);
        platform.BitmapDelete(bmp);
        return ct;
    }

    internal float FloorFloat(float a)
    {
        if (a >= 0)
        {
            return platform.FloatToInt(a);
        }
        else
        {
            return platform.FloatToInt(a) - 1;
        }
    }

    public byte[] Serialize(Packet_Client packet, IntRef retLength)
    {
        CitoMemoryStream ms = new CitoMemoryStream();
        Packet_ClientSerializer.Serialize(ms, packet);
        byte[] data = ms.ToArray();
        retLength.value = ms.Length();
        return data;
    }

    public void SendPacket(byte[] packet, int packetLength)
    {
        //try
        //{
        INetOutgoingMessage msg = new INetOutgoingMessage();
        msg.Write(packet, packetLength);
        main.SendMessage(msg, MyNetDeliveryMethod.ReliableOrdered);
        //}
        //catch
        //{
        //    game.p.ConsoleWriteLine("SendPacket error");
        //}
    }

    internal NetClient main;

    IntRef packetLen;
    public void SendPacketClient(Packet_Client packetClient)
    {
        byte[] packet = Serialize(packetClient, packetLen);
        SendPacket(packet, packetLen.value);
    }

    internal bool IsTeamchat;
    internal void SendChat(string s)
    {
        SendPacketClient(ClientPackets.Chat(s, IsTeamchat ? 1 : 0));
    }

    internal void SendPingReply()
    {
        SendPacketClient(ClientPackets.PingReply());
    }

    internal void SendSetBlock(int x, int y, int z, int mode, int type, int materialslot)
    {
        SendPacketClient(ClientPackets.SetBlock(x,y,z,mode,type, materialslot));
    }
    internal int ActiveMaterial;

    internal void SendFillArea(int startx, int starty, int startz, int endx, int endy, int endz, int blockType)
    {
        SendPacketClient(ClientPackets.FillArea(startx, starty, startz, endx, endy, endz, blockType, ActiveMaterial));
    }

    internal void InventoryClick(Packet_InventoryPosition pos)
    {
        SendPacketClient(ClientPackets.InventoryClick(pos));
    }

    internal void WearItem(Packet_InventoryPosition from, Packet_InventoryPosition to)
    {
        SendPacketClient(ClientPackets.WearItem(from, to));
    }

    internal void MoveToInventory(Packet_InventoryPosition from)
    {
        SendPacketClient(ClientPackets.MoveToInventory(from));
    }

    internal DictionaryStringString performanceinfo;


    internal Chatline[] ChatLines;
    internal int ChatLinesMax;
    internal int ChatLinesCount;
    internal int ChatLineLength;
    internal string GuiTypingBuffer;
    internal bool IsTyping;

    public void AddChatline(string s)
    {
        Game game = this;
        if (game.platform.StringEmpty(s))
        {
            return;
        }
        //Check for links in chatline
        bool containsLink = false;
        string linkTarget = "";
        //Normal HTTP links
        if (game.platform.StringContains(s, "http://"))
        {
            containsLink = true;
            IntRef r = new IntRef();
            string[] temp = game.platform.StringSplit(s, " ", r);
            for (int i = 0; i < r.value; i++)
            {
                if (game.platform.StringIndexOf(temp[i], "http://") != -1)
                {
                    linkTarget = temp[i];
                    break;
                }
            }
        }
        //Secure HTTPS links
        if (game.platform.StringContains(s, "https://"))
        {
            containsLink = true;
            IntRef r = new IntRef();
            string[] temp = game.platform.StringSplit(s, " ", r);
            for (int i = 0; i < r.value; i++)
            {
                if (game.platform.StringIndexOf(temp[i], "https://") != -1)
                {
                    linkTarget = temp[i];
                    break;
                }
            }
        }
        int now = game.platform.TimeMillisecondsFromStart();
        //Display message in multiple lines if it's longer than one line
        if (s.Length > ChatLineLength)
        {
            for (int i = 0; i <= s.Length / ChatLineLength; i++)
            {
                int displayLength = ChatLineLength;
                if (s.Length - (i * ChatLineLength) < ChatLineLength)
                {
                    displayLength = s.Length - (i * ChatLineLength);
                }
                if (containsLink)
                    ChatLinesAdd(Chatline.CreateClickable(StringTools.StringSubstring(game.platform, s, i * ChatLineLength, displayLength), now, linkTarget));
                else
                    ChatLinesAdd(Chatline.Create(StringTools.StringSubstring(game.platform, s, i * ChatLineLength, displayLength), now));
            }
        }
        else
        {
            if (containsLink)
                ChatLinesAdd(Chatline.CreateClickable(s, now, linkTarget));
            else
                ChatLinesAdd(Chatline.Create(s, now));
        }
    }

    void ChatLinesAdd(Chatline chatline)
    {
        if (ChatLinesCount >= ChatLinesMax)
        {
            Chatline[] lines2 = new Chatline[ChatLinesMax * 2];
            for (int i = 0; i < ChatLinesMax; i++)
            {
                lines2[i] = ChatLines[i];
            }
            ChatLines = lines2;
            ChatLinesMax *= 2;
        }
        ChatLines[ChatLinesCount++] = chatline;
    }

    internal bool ENABLE_DRAW2D;
    internal bool ENABLE_FREEMOVE;
    internal bool ENABLE_NOCLIP;
    internal bool AllowFreemove;
    internal bool enableCameraControl;

    internal void Respawn()
    {
        SendPacketClient(ClientPackets.SpecialKeyRespawn());
        player.physicsState.movedz = 0;
    }

    public static bool IsTransparentForLight(Packet_BlockType b)
    {
        return b.DrawType != Packet_DrawTypeEnum.Solid && b.DrawType != Packet_DrawTypeEnum.ClosedDoor;
    }

    internal GuiState guistate;
    internal bool overheadcamera;
    public bool GetFreeMouse()
    {
        if (overheadcamera)
        {
            return true;
        }
        return !platform.IsMousePointerLocked();
    }
    bool mousePointerLockShouldBe;
    public void SetFreeMouse(bool value)
    {
        mousePointerLockShouldBe = !value;
        if (value)
        {
            platform.ExitMousePointerLock();
        }
        else
        {
            platform.RequestMousePointerLock();
        }
    }
    internal MapLoadingProgressEventArgs maploadingprogress;

    public void MapLoadingStart()
    {
        guistate = GuiState.MapLoading;
        SetFreeMouse(true);
        maploadingprogress = new MapLoadingProgressEventArgs();
        fontMapLoading = FontCi.Create("Arial", 14, 0);
    }

    internal FontCi fontMapLoading;

    internal string invalidVersionDrawMessage;
    internal Packet_Server invalidVersionPacketIdentification;

    DictionaryStringInt1024 textures;
    internal int GetTexture(string p)
    {
        if (!textures.Contains(p))
        {
            BoolRef found = new BoolRef();
            BitmapCi bmp = platform.BitmapCreateFromPng(GetFile(p), GetFileLength(p));
            int texture = platform.LoadTextureFromBitmap(bmp);
            textures.Set(p, texture);
            platform.BitmapDelete(bmp);
        }
        return textures.Get(p);
    }

    internal int GetTextureOrLoad(string name, BitmapCi bmp)
    {
        if (!textures.Contains(name))
        {
            BoolRef found = new BoolRef();
            textures.Set(name, platform.LoadTextureFromBitmap(bmp));
        }
        return textures.Get(name);
    }

    internal int xcenter(float width)
    {
        return platform.FloatToInt((platform.GetCanvasWidth() / 2 - width / 2));
    }

    internal int ycenter(float height)
    {
        return platform.FloatToInt((platform.GetCanvasHeight() / 2 - height / 2));
    }

    internal ServerInformation ServerInfo;
    internal bool AudioEnabled;
    internal bool AutoJumpEnabled;
    internal MenuState menustate;
    internal bool mouseleftclick;
    internal bool mouseleftdeclick;
    internal bool wasmouseleft;
    internal bool mouserightclick;
    internal bool mouserightdeclick;
    internal bool wasmouseright;
    internal int ENABLE_LAG;

    public int Width()
    {
        return platform.GetCanvasWidth();
    }

    public int Height()
    {
        return platform.GetCanvasHeight();
    }

    internal float znear;

    internal GetCameraMatrix CameraMatrix;

    float[] Set3dProjectionTempMat4;
    public void Set3dProjection(float zfar, float fov)
    {
        float aspect_ratio = one * Width() / Height();
        Mat4.Perspective(Set3dProjectionTempMat4, fov, aspect_ratio, znear, zfar);
        CameraMatrix.lastpmatrix = Set3dProjectionTempMat4;
        GLMatrixModeProjection();
        GLLoadMatrix(Set3dProjectionTempMat4);
        SetMatrixUniformProjection();
    }
    internal bool ENABLE_ZFAR;

    internal float zfar()
    {
        if (d_Config3d.viewdistance >= 256)
        {
            return d_Config3d.viewdistance * 2;
        }
        return ENABLE_ZFAR ? d_Config3d.viewdistance : 99999;
    }

    internal Packet_ServerPlayerStats PlayerStats;

    internal int[] TotalAmmo;
    internal int[] LoadedAmmo;

    string[] AllowedFonts;
    int AllowedFontsCount;

    internal string ValidFont(string family)
    {
        for (int i = 0; i < AllowedFontsCount; i++)
        {
            if (AllowedFonts[i] == family)
            {
                return family;
            }
        }
        return AllowedFonts[0];
    }

    internal int SelectedBlockPositionX;
    internal int SelectedBlockPositionY;
    internal int SelectedBlockPositionZ;
    internal int SelectedEntityId;

    internal bool IsWater(int blockType)
    {
        string name = blocktypes[blockType].Name;
        if (name == null)
        {
            return false;
        }
        return platform.StringContains(name, "Water"); // todo
    }

    internal int mouseCurrentX;
    internal int mouseCurrentY;
    internal Packet_Inventory d_Inventory;

    internal float fov;

    internal float currentfov()
    {
        if (IronSights)
        {
            Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
            if (item != null && item.ItemClass == Packet_ItemClassEnum.Block)
            {
                if (DeserializeFloat(blocktypes[item.BlockId].IronSightsFovFloat) != 0)
                {
                    return this.fov * DeserializeFloat(blocktypes[item.BlockId].IronSightsFovFloat);
                }
            }
        }
        return this.fov;
    }

    internal bool IronSights;

    internal float DeserializeFloat(int value)
    {
        return (one * value) / 32;
    }

    internal int BlockUnderPlayer()
    {
        if (!IsValidPos(platform.FloatToInt(player.position.x),
            platform.FloatToInt(player.position.z),
            platform.FloatToInt(player.position.y) - 1))
        {
            return -1;
        }
        int blockunderplayer = GetBlock(platform.FloatToInt(player.position.x),
            platform.FloatToInt(player.position.z),
            platform.FloatToInt(player.position.y) - 1);
        return blockunderplayer;
    }

    internal CameraType cameratype;
    internal bool ENABLE_TPP_VIEW;

    internal Vector3Ref playerdestination;
    internal void SetCamera(CameraType type)
    {
        if (type == CameraType.Fpp)
        {
            cameratype = CameraType.Fpp;
            SetFreeMouse(false);
            ENABLE_TPP_VIEW = false;
            overheadcamera = false;
        }
        else if (type == CameraType.Tpp)
        {
            cameratype = CameraType.Tpp;
            ENABLE_TPP_VIEW = true;
        }
        else
        {
            cameratype = CameraType.Overhead;
            overheadcamera = true;
            SetFreeMouse(true);
            ENABLE_TPP_VIEW = true;
            playerdestination = Vector3Ref.Create(player.position.x, player.position.y, player.position.z);
        }
    }
    internal float basemovespeed;
    internal float movespeed;
    internal float BuildDelay()
    {
        float default_ = (one * 95 / 100) * (1 / basemovespeed);
        Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
        if (item == null || item.ItemClass != Packet_ItemClassEnum.Block)
        {
            return default_;
        }
        float delay = DeserializeFloat(blocktypes[item.BlockId].DelayFloat);
        if (delay == 0)
        {
            return default_;
        }
        return delay;
    }

    internal Packet_InventoryPosition InventoryPositionMaterialSelector(int materialId)
    {
        Packet_InventoryPosition pos = new Packet_InventoryPosition();
        pos.Type = Packet_InventoryPositionTypeEnum.MaterialSelector;
        pos.MaterialId = materialId;
        return pos;
    }

    internal Packet_InventoryPosition InventoryPositionMainArea(int x, int y)
    {
        Packet_InventoryPosition pos = new Packet_InventoryPosition();
        pos.Type = Packet_InventoryPositionTypeEnum.MainArea;
        pos.AreaX = x;
        pos.AreaY = y;
        return pos;
    }

    internal RailDirection PickHorizontalVertical(float xfract, float yfract)
    {
        float x = xfract;
        float y = yfract;
        if (y >= x && y >= (1 - x))
        {
            return RailDirection.Vertical;
        }
        if (y < x && y < (1 - x))
        {
            return RailDirection.Vertical;
        }
        return RailDirection.Horizontal;
    }

    internal RailDirection PickCorners(float xfract, float zfract)
    {
        float half = one / 2;
        if (xfract < half && zfract < half)
        {
            return RailDirection.UpLeft;
        }
        if (xfract >= half && zfract < half)
        {
            return RailDirection.UpRight;
        }
        if (xfract < half && zfract >= half)
        {
            return RailDirection.DownLeft;
        }
        return RailDirection.DownRight;
    }

    internal IntRef BlockInHand()
    {
        Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
        if (item != null && item.ItemClass == Packet_ItemClassEnum.Block)
        {
            return IntRef.Create(item.BlockId);
        }
        return null;
    }

    internal float RadiusWhenMoving;

    internal float CurrentRecoil()
    {
        Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
        if (item == null || item.ItemClass != Packet_ItemClassEnum.Block)
        {
            return 0;
        }
        return DeserializeFloat(blocktypes[item.BlockId].RecoilFloat);
    }
    internal Vector3Ref playervelocity;

    internal float CurrentAimRadius()
    {
        Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
        if (item == null || item.ItemClass != Packet_ItemClassEnum.Block)
        {
            return 0;
        }
        float radius = (DeserializeFloat(blocktypes[item.BlockId].AimRadiusFloat) / 800) * Width();
        if (IronSights)
        {
            radius = (DeserializeFloat(blocktypes[item.BlockId].IronSightsAimRadiusFloat) / 800) * Width();
        }
        return radius + RadiusWhenMoving * radius * (MathCi.MinFloat(playervelocity.Length() / movespeed, 1));
    }

    internal RandomCi rnd;

    internal PointFloatRef GetAim()
    {
        if (CurrentAimRadius() <= 1)
        {
            return PointFloatRef.Create(0, 0);
        }
        float half = one / 2;
        float x;
        float y;
        for (; ; )
        {
            x = (rnd.NextFloat() - half) * CurrentAimRadius() * 2;
            y = (rnd.NextFloat() - half) * CurrentAimRadius() * 2;
            float dist1 = platform.MathSqrt(x * x + y * y);
            if (dist1 <= CurrentAimRadius())
            {
                break;
            }
        }
        return PointFloatRef.Create(x, y);
    }

    public static float ClampFloat(float value, float min, float max)
    {
        float result = value;
        if (value > max)
        {
            result = max;
        }
        if (value < min)
        {
            result = min;
        }
        return result;
    }

    public static int ClampInt(int value, int min, int max)
    {
        int result = value;
        if (value > max)
        {
            result = max;
        }
        if (value < min)
        {
            result = min;
        }
        return result;
    }

    internal GameData d_Data;
    internal TerrainRenderer terrainRenderer;

    public const int maxlight = 15;

    public int MaybeGetLight(int x, int y, int z)
    {
        IntRef ret = terrainRenderer.MaybeGetLight(x, y, z);
        if (ret == null)
        {
            return maxlight;
        }
        return ret.value;
    }

    public void Draw2dBitmapFile(string filename, float x, float y, float w, float h)
    {
        Draw2dTexture(GetTexture(filename), x, y, w, h, null, 0, ColorFromArgb(255, 255, 255, 255), false);
    }
    internal int maxdrawdistance;
    public void ToggleFog()
    {
        int[] drawDistances = new int[10];
        int drawDistancesCount = 0;
        drawDistances[drawDistancesCount++] = 32;
        if (maxdrawdistance >= 64) { drawDistances[drawDistancesCount++] = 64; }
        if (maxdrawdistance >= 128) { drawDistances[drawDistancesCount++] = 128; }
        if (maxdrawdistance >= 256) { drawDistances[drawDistancesCount++] = 256; }
        if (maxdrawdistance >= 512) { drawDistances[drawDistancesCount++] = 512; }
        for (int i = 0; i < drawDistancesCount; i++)
        {
            if (d_Config3d.viewdistance == drawDistances[i])
            {
                d_Config3d.viewdistance = drawDistances[(i + 1) % drawDistancesCount];
                terrainRenderer.StartTerrain();
                return;
            }
        }
        d_Config3d.viewdistance = drawDistances[0];
        terrainRenderer.StartTerrain();
    }

    internal int LocalPlayerId;

    internal float GetCharacterEyesHeight()
    {
        return entities[LocalPlayerId].drawModel.eyeHeight;
    }

    internal void SetCharacterEyesHeight(float value)
    {
        entities[LocalPlayerId].drawModel.eyeHeight = value;
    }

    public float EyesPosX() { return player.position.x; }
    public float EyesPosY() { return player.position.y + GetCharacterEyesHeight(); }
    public float EyesPosZ() { return player.position.z; }

    public void AudioPlay(string file)
    {
        if (!AudioEnabled)
        {
            return;
        }
        AudioPlayAt(file, EyesPosX(), EyesPosY(), EyesPosZ());
    }

    public void AudioPlayAt(string file, float x, float y, float z)
    {
        if (file == null)
        {
            return;
        }
        if (!AudioEnabled)
        {
            return;
        }
        if (assetsLoadProgress.value != 1)
        {
            return;
        }
        string file_ = platform.StringReplace(file, ".wav", ".ogg");

        if (GetFileLength(file_) == 0)
        {
            platform.ConsoleWriteLine(platform.StringFormat("File not found: {0}", file));
            return;
        }

        Sound_ s = new Sound_();
        s.name = file_;
        s.x = x;
        s.y = y;
        s.z = z;
        audio.Add(s);
    }

    public void AudioPlayLoop(string file, bool play, bool restart)
    {
        if ((!AudioEnabled) && play)
        {
            return;
        }
        if (assetsLoadProgress.value != 1)
        {
            return;
        }

        string file_ = platform.StringReplace(file, ".wav", ".ogg");

        if (GetFileLength(file_) == 0)
        {
            platform.ConsoleWriteLine(platform.StringFormat("File not found: {0}", file));
            return;
        }

        if (play)
        {
            Sound_ s = null;
            bool alreadyPlaying = false;
            for (int i = 0; i < audio.soundsCount; i++)
            {
                if (audio.sounds[i] == null) { continue; }
                if (audio.sounds[i].name == file_)
                {
                    alreadyPlaying = true;
                    s = audio.sounds[i];
                }
            }
            if (!alreadyPlaying)
            {
                s = new Sound_();
                s.name = file_;
                s.loop = true;
                audio.Add(s);
            }
            s.x = EyesPosX();
            s.y = EyesPosY();
            s.z = EyesPosZ();
        }
        else
        {
            for (int i = 0; i < audio.soundsCount; i++)
            {
                if (audio.sounds[i] == null) { continue; }
                if (audio.sounds[i].name == file_)
                {
                    audio.sounds[i].stop = true;
                }
            }
        }
    }

    public int MaterialSlots_(int i)
    {
        Packet_Item item = d_Inventory.RightHand[i];
        int m = d_Data.BlockIdDirt();
        if (item != null && item.ItemClass == Packet_ItemClassEnum.Block)
        {
            m = d_Inventory.RightHand[i].BlockId;
        }
        return m;
    }

    internal bool IsTileEmptyForPhysics(int x, int y, int z)
    {
        if (z >= MapSizeZ)
        {
            return true;
        }
        if (x < 0 || y < 0 || z < 0)// || z >= mapsizez)
        {
            return ENABLE_FREEMOVE;
        }
        if (x >= MapSizeX || y >= MapSizeY)// || z >= mapsizez)
        {
            return ENABLE_FREEMOVE;
        }
        int block = GetBlockValid(x, y, z);
        return block == SpecialBlockId.Empty
            || block == d_Data.BlockIdFillArea()
            || IsWater(block);
    }

    internal bool IsTileEmptyForPhysicsClose(int x, int y, int z)
    {
        return IsTileEmptyForPhysics(x, y, z)
            || (IsValidPos(x, y, z) && blocktypes[GetBlock(x, y, z)].DrawType == Packet_DrawTypeEnum.HalfHeight)
            || (IsValidPos(x, y, z) && IsEmptyForPhysics(blocktypes[GetBlock(x, y, z)]));
    }

    internal bool IsUsableBlock(int blocktype)
    {
        return d_Data.IsRailTile(blocktype) || blocktypes[blocktype].IsUsable;
    }

    internal bool IsWearingWeapon()
    {
        return d_Inventory.RightHand[ActiveMaterial] != null;
    }

    internal void ApplyDamageToPlayer(int damage, int damageSource, int sourceId)
    {
        PlayerStats.CurrentHealth -= damage;
        if (PlayerStats.CurrentHealth <= 0)
        {
            AudioPlay("death.wav");
            SendPacketClient(ClientPackets.Death(damageSource, sourceId));

            //Respawn(); //Death is not respawn ;)
        }
        else
        {
            AudioPlay(rnd.Next() % 2 == 0 ? "grunt1.wav" : "grunt2.wav");
        }
        SendPacketClient(ClientPackets.Health(PlayerStats.CurrentHealth));
    }
    
    public int GetPlayerEyesBlockX()
    {
        return platform.FloatToInt(MathFloor(player.position.x));
    }
    public int GetPlayerEyesBlockY()
    {
        return platform.FloatToInt(MathFloor(player.position.z));
    }
    public int GetPlayerEyesBlockZ()
    {
        return platform.FloatToInt(MathFloor(player.position.y + entities[LocalPlayerId].drawModel.eyeHeight));
    }

    public int MathFloor(float a)
    {
        if (a >= 0)
        {
            return platform.FloatToInt(a);
        }
        else
        {
            return platform.FloatToInt(a) - 1;
        }
    }

    internal void SetChunksAroundDirty(int cx, int cy, int cz)
    {
        if (IsValidChunkPos(cx, cy, cz, chunksize)) { terrainRenderer.SetChunkDirty(cx - 1, cy, cz, true, false); }
        if (IsValidChunkPos(cx - 1, cy, cz, chunksize)) { terrainRenderer.SetChunkDirty(cx - 1, cy, cz, true, false); }
        if (IsValidChunkPos(cx + 1, cy, cz, chunksize)) { terrainRenderer.SetChunkDirty(cx + 1, cy, cz, true, false); }
        if (IsValidChunkPos(cx, cy - 1, cz, chunksize)) { terrainRenderer.SetChunkDirty(cx, cy - 1, cz, true, false); }
        if (IsValidChunkPos(cx, cy + 1, cz, chunksize)) { terrainRenderer.SetChunkDirty(cx, cy + 1, cz, true, false); }
        if (IsValidChunkPos(cx, cy, cz - 1, chunksize)) { terrainRenderer.SetChunkDirty(cx, cy, cz - 1, true, false); }
        if (IsValidChunkPos(cx, cy, cz + 1, chunksize)) { terrainRenderer.SetChunkDirty(cx, cy, cz + 1, true, false); }
    }

    internal void Reset(int sizex, int sizey, int sizez)
    {
        MapSizeX = sizex;
        MapSizeY = sizey;
        MapSizeZ = sizez;
        chunks = new Chunk[(sizex / chunksize) * (sizey / chunksize) * (sizez / chunksize)];
        // SetAllChunksNotDirty();
    }

    internal void UpdateColumnHeight(int x, int y)
    {
        //todo faster
        int height = MapSizeZ - 1;
        for (int i = MapSizeZ - 1; i >= 0; i--)
        {
            height = i;
            if (!Game.IsTransparentForLight(blocktypes[GetBlock(x, y, i)]))
            {
                break;
            }
        }
        d_Heightmap.SetBlock(x, y, height);
    }

    internal void ShadowsOnSetBlock(int x, int y, int z)
    {
        int oldheight = d_Heightmap.GetBlock(x, y);
        UpdateColumnHeight(x, y);
        //update shadows in all chunks below
        int newheight = d_Heightmap.GetBlock(x, y);
        int min = MathCi.MinInt(oldheight, newheight);
        int max = MathCi.MaxInt(oldheight, newheight);
        for (int i = min; i < max; i++)
        {
            if (i / chunksize != z / chunksize)
            {
                terrainRenderer.SetChunkDirty(x / chunksize, y / chunksize, i / chunksize, true, false);
            }
        }
        //Todo: too many redraws. Optimize.
        //Now placing a single block updates 27 chunks,
        //and each of those chunk updates calculates light from 27 chunks.
        //So placing a block is often 729x slower than it should be.
        for (int xx = 0; xx < 3; xx++)
        {
            for (int yy = 0; yy < 3; yy++)
            {
                for (int zz = 0; zz < 3; zz++)
                {
                    int cx = x / chunksize + xx - 1;
                    int cy = y / chunksize + yy - 1;
                    int cz = z / chunksize + zz - 1;
                    if (IsValidChunkPos(cx, cy, cz, chunksize))
                    {
                        terrainRenderer.SetChunkDirty(cx, cy, cz, true, false);
                    }
                }
            }
        }
    }

    internal void SetBlock(int x, int y, int z, int tileType)
    {
        SetBlockRaw(x, y, z, tileType);
        terrainRenderer.SetChunkDirty(x / chunksize, y / chunksize, z / chunksize, true, true);
        //d_Shadows.OnSetBlock(x, y, z);
        ShadowsOnSetBlock(x, y, z);
        lastplacedblockX = x;
        lastplacedblockY = y;
        lastplacedblockZ = z;
    }

    internal VisibleDialog[] dialogs;
    internal int dialogsCount;

    internal int DialogsCount_()
    {
        int count = 0;
        for (int i = 0; i < dialogsCount; i++)
        {
            if (dialogs[i] != null)
            {
                count++;
            }
        }
        return count;
    }

    internal int GetDialogId(string name)
    {
        for (int i = 0; i < dialogsCount; i++)
        {
            if (dialogs[i] == null)
            {
                continue;
            }
            if (dialogs[i].key == name)
            {
                return i;
            }
        }
        return -1;
    }

    internal DictionaryVector3Float blockHealth;

    internal float GetCurrentBlockHealth(int x, int y, int z)
    {
        if (blockHealth.ContainsKey(x, y, z))
        {
            return blockHealth.Get(x, y, z);
        }
        int blocktype = GetBlock(x, y, z);
        return d_Data.Strength()[blocktype];
    }

    internal Vector3IntRef currentAttackedBlock;
    internal int currentlyAttackedEntity;

    internal void SendRequestBlob(string[] required, int requiredCount)
    {
        SendPacketClient(ClientPackets.RequestBlob(this, required, requiredCount));
    }

    internal int currentTimeMilliseconds;
    internal GameDataMonsters d_DataMonsters;
    internal int ReceivedMapLength;

    void InvalidPlayerWarning(int playerid)
    {
        platform.ConsoleWriteLine(platform.StringFormat("Position update of nonexistent player {0}.", platform.IntToString(playerid)));
    }

    internal bool EnablePlayerUpdatePosition(int kKey)
    {
        return true;
    }

    internal bool EnablePlayerUpdatePositionContainsKey(int kKey)
    {
        return false;
    }

    internal byte localstance;
    internal bool spawned;

    internal int LastReceivedMilliseconds;
    internal int playertexturedefault;
    public const string playertexturedefaultfilename = "mineplayer.png";
    internal bool ENABLE_DRAW_TEST_CHARACTER;
    internal AnimationState a;
    internal ModSkySphereStatic skysphere;
    internal int reloadblock;
    internal int reloadstartMilliseconds;
    internal int lastOxygenTickMilliseconds;
    internal int typinglogpos;
    internal TypingState GuiTyping;
    internal ConnectData connectdata;
    internal bool issingleplayer;
    internal bool IsShiftPressed;
    internal bool reconnect;
    internal bool exitToMainMenu;
    internal float constRotationSpeed;
    internal void SendLeave(int reason)
    {
        SendPacketClient(ClientPackets.Leave(reason));
    }
    internal FrustumCulling d_FrustumCulling;
    internal CharacterPhysicsCi d_Physics;
    internal ClientModManager1 modmanager;
    internal ClientMod[] clientmods;
    internal int clientmodsCount;
    internal bool SkySphereNight;
    internal ModDrawParticleEffectBlockBreak particleEffectBlockBreak;
    internal int lastchunkupdates;
    internal int lasttitleupdateMilliseconds;
    internal bool ENABLE_DRAWPOSITION;

    public int SerializeFloat(float p)
    {
        return platform.FloatToInt(p * 32);
    }

    public float WeaponAttackStrength()
    {
        return NextFloat(2, 4);
    }

    public float NextFloat(float min, float max)
    {
        return rnd.NextFloat() * (max - min) + min;
    }

    public byte HeadingByte(float orientationX, float orientationY, float orientationZ)
    {
        return Game.IntToByte(platform.FloatToInt((((orientationY) % (2 * Game.GetPi())) / (2 * Game.GetPi())) * 256));
    }

    public byte PitchByte(float orientationX, float orientationY, float orientationZ)
    {
        float xx = (orientationX + Game.GetPi()) % (2 * Game.GetPi());
        xx = xx / (2 * Game.GetPi());
        return Game.IntToByte(platform.FloatToInt(xx * 256));
    }

    public void PlaySoundAt(string name, float x, float y, float z)
    {
        if (x == 0 && y == 0 && z == 0)
        {
            AudioPlay(name);
        }
        else
        {
            AudioPlayAt(name, x, z, y);
        }
    }

    internal void InvokeMapLoadingProgress(int progressPercent, int progressBytes, string status)
    {
        maploadingprogress = new MapLoadingProgressEventArgs();
        maploadingprogress.ProgressPercent = progressPercent;
        maploadingprogress.ProgressBytes = progressBytes;
        maploadingprogress.ProgressStatus = status;
    }

    internal void Log(string p)
    {
        AddChatline(p);
    }

    internal void SetTileAndUpdate(int x, int y, int z, int type)
    {
        SetBlock(x, y, z, type);
        RedrawBlock(x, y, z);
    }

    internal void RedrawBlock(int x, int y, int z)
    {
        terrainRenderer.RedrawBlock(x, y, z);
    }

    internal bool IsFillBlock(int blocktype)
    {
        return blocktype == d_Data.BlockIdFillArea()
            || blocktype == d_Data.BlockIdFillStart()
            || blocktype == d_Data.BlockIdCuboid();
    }

    internal bool IsAnyPlayerInPos(int blockposX, int blockposY, int blockposZ)
    {
        for (int i = 0; i < entitiesCount; i++)
        {
            if (entities[i] == null)
            {
                continue;
            }
            if (entities[i].drawModel == null)
            {
                continue;
            }
            Entity p = entities[i];
            if (p.networkPosition == null || (p.networkPosition != null && p.networkPosition.PositionLoaded))
            {
                if (IsPlayerInPos(p.position.x, p.position.y, p.position.z,
                    blockposX, blockposY, blockposZ))
                {
                    return true;
                }
            }
        }
        return IsPlayerInPos(player.position.x, player.position.y, player.position.z,
            blockposX, blockposY, blockposZ);
    }

    bool IsPlayerInPos(float playerposX, float playerposY, float playerposZ,
                       int blockposX, int blockposY, int blockposZ)
    {
        if (FloorFloat(playerposX) == blockposX
            &&
            (FloorFloat(playerposY + (one / 2)) == blockposZ
             || FloorFloat(playerposY + 1 + (one / 2)) == blockposZ)
            && FloorFloat(playerposZ) == blockposY)
        {
            return true;
        }
        return false;
    }
    internal float PICK_DISTANCE;
    internal bool leftpressedpicking;
    internal int selectedmodelid;
    internal int pistolcycle;
    internal int lastironsightschangeMilliseconds;
    internal int grenadecookingstartMilliseconds;
    internal float grenadetime;
    internal int lastpositionsentMilliseconds;

    internal float mouseDeltaX;
    internal float mouseDeltaY;
    float rotationspeed;
    internal void UpdateMouseViewportControl(float dt)
    {
        if (!overheadcamera)
        {
            if (platform.IsMousePointerLocked())
            {
                player.position.roty += mouseDeltaX * rotationspeed * (one / 75);
                player.position.rotx += mouseDeltaY * rotationspeed * (one / 75);
                player.position.rotx = Game.ClampFloat(player.position.rotx,
                    Game.GetPi() / 2 + (one * 15 / 1000),
                    (Game.GetPi() / 2 + Game.GetPi() - (one * 15 / 1000)));
            }

            player.position.rotx += touchOrientationDy * constRotationSpeed * (one / 75);
            player.position.roty += touchOrientationDx * constRotationSpeed * (one / 75);
            touchOrientationDx = 0;
            touchOrientationDy = 0;
        }
        if (guistate == GuiState.Normal && platform.Focused() && cameratype == CameraType.Overhead)
        {
            if (mouseMiddle || mouseRight)
            {
                overheadcameraK.TurnLeft(mouseDeltaX / 70);
                overheadcameraK.TurnUp(mouseDeltaY / 3);
            }
        }
        mouseDeltaX = 0;
        mouseDeltaY = 0;
    }

    internal string Follow;
    internal IntRef FollowId()
    {
        for (int i = 0; i < entitiesCount; i++)
        {
            if (entities[i] == null)
            {
                continue;
            }
            if (entities[i].drawName == null)
            {
                continue;
            }
            DrawName p = entities[i].drawName;
            if (p.Name == Follow)
            {
                return IntRef.Create(i);
            }
        }
        return null;
    }

    public float Dist(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        float dz = z2 - z1;
        return platform.MathSqrt(dx * dx + dy * dy + dz * dz);
    }

    internal bool IsValid(int blocktype)
    {
        return blocktypes[blocktype].Name != null;
    }

    internal int TextSizeWidth(string s, int size)
    {
        IntRef width = new IntRef();
        IntRef height = new IntRef();
        platform.TextSize(s, size, width, height);
        return width.value;
    }

    internal int TextSizeHeight(string s, int size)
    {
        IntRef width = new IntRef();
        IntRef height = new IntRef();
        platform.TextSize(s, size, width, height);
        return height.value;
    }

    ModelData circleModelData;
    public void Circle3i(float x, float y, float radius)
    {
        float angle;
        GLPushMatrix();
        GLLoadIdentity();

        int n = 32;
        if (circleModelData == null)
        {
            circleModelData = new ModelData();
            circleModelData.setMode(DrawModeEnum.Lines);
            circleModelData.indices = new int[n * 2];
            circleModelData.xyz = new float[3 * n];
            circleModelData.rgba = new byte[4 * n];
            circleModelData.uv = new float[2 * n];
            circleModelData.indicesCount = n * 2;
            circleModelData.verticesCount = n;
        }

        for (int i = 0; i < n; i++)
        {
            circleModelData.indices[i * 2] = i;
            circleModelData.indices[i * 2 + 1] = (i + 1) % (n);
        }
        for (int i = 0; i < n; i++)
        {
            angle = (i * 2 * Game.GetPi() / n);
            circleModelData.xyz[i * 3 + 0] = x + (platform.MathCos(angle) * radius);
            circleModelData.xyz[i * 3 + 1] = y + (platform.MathSin(angle) * radius);
            circleModelData.xyz[i * 3 + 2] = 0;
        }
        for (int i = 0; i < 4 * n; i++)
        {
            circleModelData.rgba[i] = 255;
        }
        for (int i = 0; i < 2 * n; i++)
        {
            circleModelData.uv[i] = 0;
        }

        DrawModelData(circleModelData);

        GLPopMatrix();
    }

    internal int totaltimeMilliseconds;

    internal Entity[] entities;
    internal int entitiesCount;
    internal const int entitiesMax = 4096;
    public const int entityMonsterIdStart = 128;
    public const int entityMonsterIdCount = 128;
    public const int entityLocalIdStart = 256;

    internal void EntityAddLocal(Entity entity)
    {
        for (int i = entityLocalIdStart; i < entitiesCount; i++)
        {
            if (entities[i] == null)
            {
                entities[i] = entity;
                return;
            }
        }
        entities[entitiesCount++] = entity;
    }

    internal float PlayerPushDistance;

    internal Entity CreateBulletEntity(float fromX, float fromY, float fromZ, float toX, float toY, float toZ, float speed)
    {
        Entity entity = new Entity();

        Bullet_ bullet = new Bullet_();
        bullet.fromX = fromX;
        bullet.fromY = fromY;
        bullet.fromZ = fromZ;
        bullet.toX = toX;
        bullet.toY = toY;
        bullet.toZ = toZ;
        bullet.speed = speed;
        entity.bullet = bullet;

        entity.sprite = new Sprite();
        entity.sprite.image = "Sponge.png";
        entity.sprite.size = 4;
        entity.sprite.animationcount = 0;

        return entity;
    }

    public bool Vec3Equal(float ax, float ay, float az, float bx, float by, float bz)
    {
        return ax == bx && ay == by && az == bz;
    }

    internal bool[] keyboardState;

    public const int KeyAltLeft = 5;
    public const int KeyAltRight = 6;

    internal bool Swimming()
    {
        int eyesBlock = GetPlayerEyesBlock();
        if (eyesBlock == -1) { return true; }
        return d_Data.WalkableType1()[eyesBlock] == Packet_WalkableTypeEnum.Fluid;
    }

    internal bool WaterSwimming()
    {
        if (GetPlayerEyesBlock() == -1) { return true; }
        return IsWater(GetPlayerEyesBlock());
    }

    internal bool LavaSwimming()
    {
        return IsLava(GetPlayerEyesBlock());
    }

    internal int GetPlayerEyesBlock()
    {
        float pX = player.position.x;
        float pY = player.position.y;
        float pZ = player.position.z;
        pY += entities[LocalPlayerId].drawModel.eyeHeight;
        int bx = MathFloor(pX);
        int by = MathFloor(pZ);
        int bz = MathFloor(pY);

        if (!IsValidPos(bx, by, bz))
        {
            if (pY < WaterLevel())
            {
                return -1;
            }
            return 0;
        }
        return GetBlockValid(bx, by, bz);
    }

    public float WaterLevel() { return MapSizeZ / 2; }

    internal bool IsLava(int blockType)
    {
        return platform.StringContains(blocktypes[blockType].Name, "Lava"); // todo
    }

    internal int terraincolor()
    {
        if (WaterSwimming())
        {
            return Game.ColorFromArgb(255, 78, 95, 140);
        }
        else if (LavaSwimming())
        {
            return Game.ColorFromArgb(255, 222, 101, 46);
        }
        else
        {
            return Game.ColorFromArgb(255, 255, 255, 255);
        }
    }

    internal void SetAmbientLight(int color)
    {
        int r = Game.ColorR(color);
        int g = Game.ColorG(color);
        int b = Game.ColorB(color);
        platform.GlLightModelAmbient(r, g, b);
    }

    internal OptionsCi options;

    internal int GetKey(int key)
    {
        if (options == null)
        {
            return key;
        }
        if (options.Keys[key] != 0)
        {
            return options.Keys[key];
        }
        return key;
    }

    internal float MoveSpeedNow()
    {
        float movespeednow = movespeed;
        {
            //walk faster on cobblestone
            int blockunderplayer = BlockUnderPlayer();
            if (blockunderplayer != -1)
            {
                float floorSpeed = d_Data.WalkSpeed()[blockunderplayer];
                if (floorSpeed != 0)
                {
                    movespeednow *= floorSpeed;
                }
            }
        }
        if (keyboardState[GetKey(GlKeys.ShiftLeft)])
        {
            //enable_acceleration = false;
            movespeednow *= one * 2 / 10;
        }
        Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
        if (item != null && item.ItemClass == Packet_ItemClassEnum.Block)
        {
            float itemSpeed = DeserializeFloat(blocktypes[item.BlockId].WalkSpeedWhenUsedFloat);
            if (itemSpeed != 0)
            {
                movespeednow *= itemSpeed;
            }
            if (IronSights)
            {
                float ironSightsSpeed = DeserializeFloat(blocktypes[item.BlockId].IronSightsMoveSpeedFloat);
                if (ironSightsSpeed != 0)
                {
                    movespeednow *= ironSightsSpeed;
                }
            }
        }
        return movespeednow;
    }

    internal float VectorAngleGet(float qX, float qY, float qZ)
    {
        return (platform.MathAcos(qX / Length(qX, qY, qZ)) * MathCi.Sign(qZ));
    }

    internal float Length(float x, float y, float z)
    {
        return platform.MathSqrt(x * x + y * y + z * z);
    }

    internal void HandleMaterialKeys(int eKey)
    {
        if (eKey == GetKey(GlKeys.Number1)) { ActiveMaterial = 0; }
        if (eKey == GetKey(GlKeys.Number2)) { ActiveMaterial = 1; }
        if (eKey == GetKey(GlKeys.Number3)) { ActiveMaterial = 2; }
        if (eKey == GetKey(GlKeys.Number4)) { ActiveMaterial = 3; }
        if (eKey == GetKey(GlKeys.Number5)) { ActiveMaterial = 4; }
        if (eKey == GetKey(GlKeys.Number6)) { ActiveMaterial = 5; }
        if (eKey == GetKey(GlKeys.Number7)) { ActiveMaterial = 6; }
        if (eKey == GetKey(GlKeys.Number8)) { ActiveMaterial = 7; }
        if (eKey == GetKey(GlKeys.Number9)) { ActiveMaterial = 8; }
        if (eKey == GetKey(GlKeys.Number0)) { ActiveMaterial = 9; }
    }

    internal void UseVsync()
    {
        platform.SetVSync((ENABLE_LAG == 1) ? false : true);
    }

    internal void ToggleVsync()
    {
        ENABLE_LAG++;
        ENABLE_LAG = ENABLE_LAG % 3;
        UseVsync();
    }

    internal void GuiStateBackToGame()
    {
        guistate = GuiState.Normal;
        SetFreeMouse(false);
    }

    internal float overheadcameradistance;
    internal float tppcameradistance;
    internal int TPP_CAMERA_DISTANCE_MIN;
    internal int TPP_CAMERA_DISTANCE_MAX;
    internal void MouseWheelChanged(MouseWheelEventArgs e)
    {
        float eDeltaPrecise = e.GetDeltaPrecise();
        if (keyboardState[GetKey(GlKeys.LShift)])
        {
            if (cameratype == CameraType.Overhead)
            {
                overheadcameradistance -= eDeltaPrecise;
                if (overheadcameradistance < TPP_CAMERA_DISTANCE_MIN) { overheadcameradistance = TPP_CAMERA_DISTANCE_MIN; }
                if (overheadcameradistance > TPP_CAMERA_DISTANCE_MAX) { overheadcameradistance = TPP_CAMERA_DISTANCE_MAX; }
            }
            if (cameratype == CameraType.Tpp)
            {
                tppcameradistance -= eDeltaPrecise;
                if (tppcameradistance < TPP_CAMERA_DISTANCE_MIN) { tppcameradistance = TPP_CAMERA_DISTANCE_MIN; }
                if (tppcameradistance > TPP_CAMERA_DISTANCE_MAX) { tppcameradistance = TPP_CAMERA_DISTANCE_MAX; }
            }
        }
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnMouseWheelChanged(this, e);
        }
        if ((guistate != GuiState.Inventory)
            && (!keyboardState[GetKey(GlKeys.LShift)]))
        {
            ActiveMaterial -= platform.FloatToInt(eDeltaPrecise);
            ActiveMaterial = ActiveMaterial % 10;
            while (ActiveMaterial < 0)
            {
                ActiveMaterial += 10;
            }
        }
    }

    internal void Connect(string serverAddress, int port, string username, string auth)
    {
        main.Start();
        main.Connect(serverAddress, port);
        SendPacketClient(ClientPackets.CreateLoginPacket(platform, username, auth));
    }

    internal void Connect_(string serverAddress, int port, string username, string auth, string serverPassword)
    {
        main.Start();
        main.Connect(serverAddress, port);
        SendPacketClient(ClientPackets.CreateLoginPacket_(platform, username, auth, serverPassword));
    }

    internal void RedrawAllBlocks()
    {
        terrainRenderer.RedrawAllBlocks();
    }

    //public const int clearcolorR = 171;
    //public const int clearcolorG = 202;
    //public const int clearcolorB = 228;
    //public const int clearcolorA = 255;
    public const int clearcolorR = 0;
    public const int clearcolorG = 0;
    public const int clearcolorB = 0;
    public const int clearcolorA = 255;

    internal void SetFog()
    {
        if (d_Config3d.viewdistance >= 512)
        {
            return;
        }
        //Density for linear fog
        //float density = 0.3f;
        // use this density for exp2 fog (0.0045f was a bit too much at close ranges)
        float density = one * 25 / 10000; // 0.0025f;

        int fogR;
        int fogG;
        int fogB;
        int fogA;

        if (SkySphereNight && (!terrainRenderer.shadowssimple))
        {
            fogR = 0;
            fogG = 0;
            fogB = 0;
            fogA = 255;
        }
        else
        {
            fogR = clearcolorR;
            fogG = clearcolorG;
            fogB = clearcolorB;
            fogA = clearcolorA;
        }
        platform.GlEnableFog();
        platform.GlHintFogHintNicest();
        //old linear fog
        //GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
        // looks better
        platform.GlFogFogModeExp2();
        platform.GlFogFogColor(fogR, fogG, fogB, fogA);
        platform.GlFogFogDensity(density);
        //Unfortunately not used for exp/exp2 fog
        //float fogsize = 10;
        //if (d_Config3d.viewdistance <= 64)
        //{
        //    fogsize = 5;
        //}
        // //float fogstart = d_Config3d.viewdistance - fogsize + 200;
        //float fogstart = d_Config3d.viewdistance - fogsize;
        //GL.Fog(FogParameter.FogStart, fogstart);
        //GL.Fog(FogParameter.FogEnd, fogstart + fogsize);
    }

    internal BlockPosSide Nearest(BlockPosSide[] pick2, int pick2Count, float x, float y, float z)
    {
        float minDist = 1000 * 1000;
        BlockPosSide nearest = null;
        for (int i = 0; i < pick2Count; i++)
        {
            float dist = Dist(pick2[i].blockPos[0], pick2[i].blockPos[1], pick2[i].blockPos[2], x, y, z);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = pick2[i];
            }
        }
        return nearest;
    }

    internal BlockOctreeSearcher s;

    internal Kamera overheadcameraK;

    internal void FillChunk(Chunk destination, int destinationchunksize, int sourcex, int sourcey, int sourcez, int[] source, int sourcechunksizeX, int sourcechunksizeY, int sourcechunksizeZ)
    {
        for (int x = 0; x < destinationchunksize; x++)
        {
            for (int y = 0; y < destinationchunksize; y++)
            {
                for (int z = 0; z < destinationchunksize; z++)
                {
                    //if (x + sourcex < source.GetUpperBound(0) + 1
                    //    && y + sourcey < source.GetUpperBound(1) + 1
                    //    && z + sourcez < source.GetUpperBound(2) + 1)
                    {
                        SetBlockInChunk(destination, Index3d(x, y, z, destinationchunksize, destinationchunksize)
                            , source[Index3d(x + sourcex, y + sourcey, z + sourcez, sourcechunksizeX, sourcechunksizeY)]);
                    }
                }
            }
        }
    }

    internal void SetMapPortion(int x, int y, int z, int[] chunk, int sizeX, int sizeY, int sizeZ)
    {
        int chunksizex = sizeX;
        int chunksizey = sizeY;
        int chunksizez = sizeZ;
        if (chunksizex % chunksize != 0) { platform.ThrowException(""); }
        if (chunksizey % chunksize != 0) { platform.ThrowException(""); }
        if (chunksizez % chunksize != 0) { platform.ThrowException(""); }
        Chunk[] localchunks = new Chunk[(chunksizex / chunksize) * (chunksizey / chunksize) * (chunksizez / chunksize)];
        for (int cx = 0; cx < chunksizex / chunksize; cx++)
        {
            for (int cy = 0; cy < chunksizey / chunksize; cy++)
            {
                for (int cz = 0; cz < chunksizex / chunksize; cz++)
                {
                    localchunks[Index3d(cx, cy, cz, (chunksizex / chunksize), (chunksizey / chunksize))] = GetChunk(x + cx * chunksize, y + cy * chunksize, z + cz * chunksize);
                    FillChunk(localchunks[Index3d(cx, cy, cz, (chunksizex / chunksize), (chunksizey / chunksize))], chunksize, cx * chunksize, cy * chunksize, cz * chunksize, chunk, sizeX, sizeY, sizeZ);
                }
            }
        }
        for (int xxx = 0; xxx < chunksizex; xxx += chunksize)
        {
            for (int yyy = 0; yyy < chunksizex; yyy += chunksize)
            {
                for (int zzz = 0; zzz < chunksizex; zzz += chunksize)
                {
                    terrainRenderer.SetChunkDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize, true, true);
                    SetChunksAroundDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize);
                }
            }
        }
    }

    internal void ChatLog(string p)
    {
        if (!platform.ChatLog(this.ServerInfo.ServerName, p))
        {
            platform.ConsoleWriteLine(platform.StringFormat(language.CannotWriteChatLog(), this.ServerInfo.ServerName));
        }
    }

    //value is original block.
    internal DictionaryVector3Float fillarea;
    internal Vector3IntRef fillstart;
    internal Vector3IntRef fillend;
    internal int fillAreaLimit;

    internal void ClearFillArea()
    {
        for (int i = 0; i < fillarea.itemsCount; i++)
        {
            Vector3Float k = fillarea.items[i];
            if (k == null)
            {
                continue;
            }
            SetBlock(k.x, k.y, k.z, platform.FloatToInt(k.value));
            RedrawBlock(k.x, k.y, k.z);
        }
        fillarea.Clear();
    }

    internal void FillFill(Vector3IntRef a_, Vector3IntRef b_)
    {
        int startx = MathCi.MinInt(a_.X, b_.X);
        int endx = MathCi.MaxInt(a_.X, b_.X);
        int starty = MathCi.MinInt(a_.Y, b_.Y);
        int endy = MathCi.MaxInt(a_.Y, b_.Y);
        int startz = MathCi.MinInt(a_.Z, b_.Z);
        int endz = MathCi.MaxInt(a_.Z, b_.Z);
        for (int x = startx; x <= endx; x++)
        {
            for (int y = starty; y <= endy; y++)
            {
                for (int z = startz; z <= endz; z++)
                {
                    if (fillarea.Count() > fillAreaLimit)
                    {
                        ClearFillArea();
                        return;
                    }
                    if (!IsFillBlock(GetBlock(x, y, z)))
                    {
                        fillarea.Set(x, y, z, GetBlock(x, y, z));
                        SetBlock(x, y, z, d_Data.BlockIdFillArea());
                        RedrawBlock(x, y, z);
                    }
                }
            }
        }
    }

    internal void OnPickUseWithTool(int posX, int posY, int posZ)
    {
        SendSetBlock(posX, posY, posZ, Packet_BlockSetModeEnum.UseWithTool, d_Inventory.RightHand[ActiveMaterial].BlockId, ActiveMaterial);
    }

    internal void KeyUp(int eKey)
    {
        for (int i = 0; i < clientmodsCount; i++)
        {
            KeyEventArgs args_ = new KeyEventArgs();
            args_.SetKeyCode(eKey);
            clientmods[i].OnKeyUp(this, args_);
            if (args_.GetHandled())
            {
                return;
            }
        }
        keyboardState[eKey] = false;
        if (eKey == GetKey(GlKeys.ShiftLeft) || eKey == GetKey(GlKeys.ShiftRight))
        {
            IsShiftPressed = false;
        }
    }
    internal float playerPositionSpawnX;
    internal float playerPositionSpawnY;
    internal float playerPositionSpawnZ;

    internal void MapLoaded()
    {
        terrainRenderer.StartTerrain();
        RedrawAllBlocks();
        materialSlots = d_Data.DefaultMaterialSlots();
        GuiStateBackToGame();

        playerPositionSpawnX = player.position.x;
        playerPositionSpawnY = player.position.y;
        playerPositionSpawnZ = player.position.z;
    }
    internal int[] materialSlots;

    internal void Draw2dText1(string text, int x, int y, int fontsize, IntRef color, bool enabledepthtest)
    {
        FontCi font = new FontCi();
        font.family = "Arial";
        font.size = fontsize;
        Draw2dText(text, font, x, y, color, enabledepthtest);
    }

    internal InventoryUtilClient d_InventoryUtil;
    internal void UseInventory(Packet_Inventory packet_Inventory)
    {
        d_Inventory = packet_Inventory;
        d_InventoryUtil.d_Inventory = packet_Inventory;
    }

    internal void KeyPress(int eKeyChar)
    {
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            KeyPressEventArgs args_ = new KeyPressEventArgs();
            args_.SetKeyChar(eKeyChar);
            clientmods[i].OnKeyPress(this, args_);
            if (args_.GetHandled())
            {
                return;
            }
        }
    }

    public string CharToString(int c)
    {
        int[] arr = new int[1];
        arr[0] = c;
        return platform.CharArrayToString(arr, 1);
    }

    internal Speculative[] speculative;
    internal int speculativeCount;
    internal const int speculativeMax = 8 * 1024;

    internal void SendSetBlockAndUpdateSpeculative(int material, int x, int y, int z, int mode)
    {
        SendSetBlock(x, y, z, mode, material, ActiveMaterial);

        Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
        if (item != null && item.ItemClass == Packet_ItemClassEnum.Block)
        {
            //int blockid = d_Inventory.RightHand[d_Viewport.ActiveMaterial].BlockId;
            int blockid = material;
            if (mode == Packet_BlockSetModeEnum.Destroy)
            {
                blockid = SpecialBlockId.Empty;
            }
            Speculative s_ = new Speculative();
            s_.x = x;
            s_.y = y;
            s_.z = z;
            s_.blocktype = GetBlock(x, y, z);
            s_.timeMilliseconds = platform.TimeMillisecondsFromStart();
            AddSpeculative(s_);
            SetBlock(x, y, z, blockid);
            RedrawBlock(x, y, z);
        }
        else
        {
            //TODO
        }
    }

    void AddSpeculative(Speculative s_)
    {
        for (int i = 0; i < speculativeCount; i++)
        {
            if (speculative[i] == null)
            {
                speculative[i] = s_;
                return;
            }
        }
        speculative[speculativeCount++] = s_;
    }

    internal void OnNewFrame(float dt)
    {
        for (int i = 0; i < speculativeCount; i++)
        {
            Speculative s_ = speculative[i];
            if (s_ == null)
            {
                continue;
            }
            if ((one * (platform.TimeMillisecondsFromStart() - s_.timeMilliseconds) / 1000) > 2)
            {
                RedrawBlock(s_.x, s_.y, s_.z);
                speculative[i] = null;
            }
        }
    }

    internal void OnPick(int blockposX, int blockposY, int blockposZ, int blockposoldX, int blockposoldY, int blockposoldZ, float[] collisionPos, bool right)
    {
        float xfract = collisionPos[0] - MathFloor(collisionPos[0]);
        float zfract = collisionPos[2] - MathFloor(collisionPos[2]);
        int activematerial = MaterialSlots_(ActiveMaterial);
        int railstart = d_Data.BlockIdRailstart();
        if (activematerial == railstart + RailDirectionFlags.TwoHorizontalVertical
            || activematerial == railstart + RailDirectionFlags.Corners)
        {
            RailDirection dirnew;
            if (activematerial == railstart + RailDirectionFlags.TwoHorizontalVertical)
            {
                dirnew = PickHorizontalVertical(xfract, zfract);
            }
            else
            {
                dirnew = PickCorners(xfract, zfract);
            }
            int dir = d_Data.Rail()[GetBlock(blockposoldX, blockposoldY, blockposoldZ)];
            if (dir != 0)
            {
                blockposX = blockposoldX;
                blockposY = blockposoldY;
                blockposZ = blockposoldZ;
            }
            activematerial = railstart + (dir | DirectionUtils.ToRailDirectionFlags(dirnew));
            //Console.WriteLine(blockposold);
            //Console.WriteLine(xfract + ":" + zfract + ":" + activematerial + ":" + dirnew);
        }
        int x = platform.FloatToInt(blockposX);
        int y = platform.FloatToInt(blockposY);
        int z = platform.FloatToInt(blockposZ);
        int mode = right ? Packet_BlockSetModeEnum.Create : Packet_BlockSetModeEnum.Destroy;
        {
            if (IsAnyPlayerInPos(x, y, z) || activematerial == 151)
            {
                return;
            }
            Vector3IntRef v = Vector3IntRef.Create(x, y, z);
            Vector3IntRef oldfillstart = fillstart;
            Vector3IntRef oldfillend = fillend;
            if (mode == Packet_BlockSetModeEnum.Create)
            {
                if (blocktypes[activematerial].IsTool)
                {
                    OnPickUseWithTool(blockposX, blockposY, blockposZ);
                    return;
                }

                //if (GameDataManicDigger.IsDoorTile(activematerial))
                //{
                //    if (z + 1 == d_Map.MapSizeZ || z == 0) return;
                //}

                if (activematerial == d_Data.BlockIdCuboid())
                {
                    ClearFillArea();

                    if (fillstart != null)
                    {
                        Vector3IntRef f = fillstart;
                        if (!IsFillBlock(GetBlock(f.X, f.Y, f.Z)))
                        {
                            fillarea.Set(f.X, f.Y, f.Z, GetBlock(f.X, f.Y, f.Z));
                        }
                        SetBlock(f.X, f.Y, f.Z, d_Data.BlockIdFillStart());


                        FillFill(v, fillstart);
                    }
                    if (!IsFillBlock(GetBlock(v.X, v.Y, v.Z)))
                    {
                        fillarea.Set(v.X, v.Y, v.Z, GetBlock(v.X, v.Y, v.Z));
                    }
                    SetBlock(v.X, v.Y, v.Z, d_Data.BlockIdCuboid());
                    fillend = v;
                    RedrawBlock(v.X, v.Y, v.Z);
                    return;
                }
                if (activematerial == d_Data.BlockIdFillStart())
                {
                    ClearFillArea();
                    if (!IsFillBlock(GetBlock(v.X, v.Y, v.Z)))
                    {
                        fillarea.Set(v.X, v.Y, v.Z, GetBlock(v.X, v.Y, v.Z));
                    }
                    SetBlock(v.X, v.Y, v.Z, d_Data.BlockIdFillStart());
                    fillstart = v;
                    fillend = null;
                    RedrawBlock(v.X, v.Y, v.Z);
                    return;
                }
                if (fillarea.ContainsKey(v.X, v.Y, v.Z))// && fillarea[v])
                {
                    SendFillArea(fillstart.X, fillstart.Y, fillstart.Z, fillend.X, fillend.Y, fillend.Z, activematerial);
                    ClearFillArea();
                    fillstart = null;
                    fillend = null;
                    return;
                }
            }
            else
            {
                if (blocktypes[activematerial].IsTool)
                {
                    OnPickUseWithTool(blockposX, blockposY, blockposoldZ);
                    return;
                }
                //delete fill start
                if (fillstart != null && fillstart.X == v.X && fillstart.Y == v.Y && fillstart.Z == v.Z)
                {
                    ClearFillArea();
                    fillstart = null;
                    fillend = null;
                    return;
                }
                //delete fill end
                if (fillend != null && fillend.X == v.X && fillend.Y == v.Y && fillend.Z == v.Z)
                {
                    ClearFillArea();
                    fillend = null;
                    return;
                }
            }
            if (mode == Packet_BlockSetModeEnum.Create && activematerial == d_Data.BlockIdMinecart())
            {
                //CommandRailVehicleBuild cmd2 = new CommandRailVehicleBuild();
                //cmd2.x = (short)x;
                //cmd2.y = (short)y;
                //cmd2.z = (short)z;
                //TrySendCommand(MakeCommand(CommandId.RailVehicleBuild, cmd2));
                return;
            }
            //if (TrySendCommand(MakeCommand(CommandId.Build, cmd)))
            SendSetBlockAndUpdateSpeculative(activematerial, x, y, z, mode);
        }
    }

    internal void Set3dProjection1(float zfar_)
    {
        Set3dProjection(zfar_, currentfov());
    }

    internal void Set3dProjection2()
    {
        Set3dProjection1(zfar());
    }

    internal void SendGameResolution()
    {
        SendPacketClient(ClientPackets.GameResolution(Width(), Height()));
    }

    bool sendResize;
    internal void OnResize()
    {
        platform.GlViewport(0, 0, Width(), Height());
        this.Set3dProjection2();
        //Notify server of size change
        if (sendResize)
        {
            SendGameResolution();
        }
    }

    internal void Reconnect()
    {
        reconnect = true;
    }

    internal Packet_ServerRedirect redirectTo;
    internal void ExitAndSwitchServer(Packet_ServerRedirect newServer)
    {
        if (issingleplayer)
        {
            platform.SinglePlayerServerExit();
        }
        redirectTo = newServer;
        exitToMainMenu = true;
    }
    
    internal Packet_ServerRedirect GetRedirect()
    {
        return redirectTo;
    }

    internal void ExitToMainMenu_()
    {
        if (issingleplayer)
        {
            platform.SinglePlayerServerExit();
        }
        redirectTo = null;
        exitToMainMenu = true;
    }

    internal void ClientCommand(string s_)
    {
        if (s_ == "")
        {
            return;
        }
        IntRef ssCount = new IntRef();
        string[] ss = platform.StringSplit(s_, " ", ssCount);
        if (StringTools.StringStartsWith(platform, s_, "."))
        {
            string strFreemoveNotAllowed = language.FreemoveNotAllowed();
            //try
            {
                string cmd = StringTools.StringSubstringToEnd(platform, ss[0], 1);
                string arguments;
                if (platform.StringIndexOf(s_, " ") == -1)
                {
                    arguments = "";
                }
                else
                {
                    arguments = StringTools.StringSubstringToEnd(platform, s_, platform.StringIndexOf(s_, " "));
                }
                arguments = platform.StringTrim(arguments);
                if (cmd == "pos")
                {
                    ENABLE_DRAWPOSITION = BoolCommandArgument(arguments);
                }
                else if (cmd == "fog")
                {
                    int foglevel;
                    foglevel = platform.IntParse(arguments);
                    //if (foglevel <= 16)
                    //{
                    //    terrain.DrawDistance = (int)Math.Pow(2, foglevel);
                    //}
                    //else
                    {
                        int foglevel2 = foglevel;
                        if (foglevel2 > 1024)
                        {
                            foglevel2 = 1024;
                        }
                        if (foglevel2 % 2 == 0)
                        {
                            foglevel2--;
                        }
                        d_Config3d.viewdistance = foglevel2;
                        //terrain.UpdateAllTiles();
                    }
                    OnResize();
                }
                else if (cmd == "noclip")
                {
                    ENABLE_NOCLIP = BoolCommandArgument(arguments);
                }
                else if (cmd == "freemove")
                {
                    if (this.AllowFreemove)
                    {
                        ENABLE_FREEMOVE = BoolCommandArgument(arguments);
                    }
                    else
                    {
                        Log(strFreemoveNotAllowed);
                        return;
                    }
                }
                else if (cmd == "fov")
                {
                    int arg = platform.IntParse(arguments);
                    int minfov = 1;
                    int maxfov = 179;
                    if (!issingleplayer)
                    {
                        minfov = 60;
                    }
                    if (arg < minfov || arg > maxfov)
                    {
                        platform.ThrowException(platform.StringFormat2("Valid field of view: {0}-{1}", platform.IntToString(minfov), platform.IntToString(maxfov)));
                    }
                    float fov_ = (2 * Game.GetPi() * (one * arg / 360));
                    this.fov = fov_;
                    OnResize();
                }
                else if (cmd == "clients")
                {
                    Log("Clients:");
                    for (int i = 0; i < entitiesCount; i++)
                    {
                        Entity entity = entities[i];
                        if (entity == null) { continue; }
                        if (entity.drawName == null) { continue; }
                        if (!entity.drawName.ClientAutoComplete) { continue; }
                        Log(platform.StringFormat2("{0} {1}", platform.IntToString(i), entities[i].drawName.Name));
                    }
                }
                else if (cmd == "movespeed")
                {
                    //try
                    //{
                    if (this.AllowFreemove)
                    {
                        if (platform.FloatParse(arguments) <= 500)
                        {
                            movespeed = basemovespeed * platform.FloatParse(arguments);
                            AddChatline(platform.StringFormat("Movespeed: {0}x", arguments));
                        }
                        else
                        {
                            AddChatline("Entered movespeed to high! max. 500x");
                        }
                    }
                    else
                    {
                        Log(strFreemoveNotAllowed);
                        return;
                    }
                    //}
                    //catch
                    //{
                    //    AddChatline("Invalid value!");
                    //    AddChatline("USE: .movespeed [movespeed]");
                    //}
                }
                else if (cmd == "gui")
                {
                    ENABLE_DRAW2D = BoolCommandArgument(arguments);
                }
                else if (cmd == "reconnect")
                {
                    Reconnect();
                }
                else if (cmd == "serverinfo")
                {
                    //Fetches server info from given adress
                    IntRef splitCount = new IntRef();
                    string[] split = platform.StringSplit(arguments, ":", splitCount);
                    if (splitCount.value == 2)
                    {
                        QueryClient qClient = new QueryClient();
                        qClient.SetPlatform(platform);
                        qClient.PerformQuery(split[0], platform.IntParse(split[1]));
                        if (qClient.querySuccess)
                        {
                            //Received result
                            QueryResult r = qClient.GetResult();
                            AddChatline(r.GameMode);
                            AddChatline(platform.IntToString(r.MapSizeX));
                            AddChatline(platform.IntToString(r.MapSizeY));
                            AddChatline(platform.IntToString(r.MapSizeZ));
                            AddChatline(platform.IntToString(r.MaxPlayers));
                            AddChatline(r.MOTD);
                            AddChatline(r.Name);
                            AddChatline(platform.IntToString(r.PlayerCount));
                            AddChatline(r.PlayerList);
                            AddChatline(platform.IntToString(r.Port));
                            AddChatline(r.PublicHash);
                            AddChatline(r.ServerVersion);
                        }
                        AddChatline(qClient.GetServerMessage());
                    }
                }
                else
                {
                    for (int i = 0; i < clientmodsCount; i++)
                    {
                        ClientCommandArgs args = new ClientCommandArgs();
                        args.arguments = arguments;
                        args.command = cmd;
                        clientmods[i].OnClientCommand(this, args);
                    }
                    string chatline = StringTools.StringSubstring(platform, GuiTypingBuffer, 0, MathCi.MinInt(GuiTypingBuffer.Length, 256));
                    SendChat(chatline);
                }
            }
            //catch (Exception e) { AddChatline(new StringReader(e.Message).ReadLine()); }
        }
        else
        {
            string chatline = StringTools.StringSubstring(platform, GuiTypingBuffer, 0, MathCi.MinInt(StringTools.StringLength(platform, GuiTypingBuffer), 4096));
            SendChat(chatline);
        }
    }
    public bool BoolCommandArgument(string arguments)
    {
        arguments = platform.StringTrim(arguments);
        return (arguments == "" || arguments == "1" || arguments == "on" || arguments == "yes");
    }
    internal string[] typinglog;
    internal int typinglogCount;

    string[] getAsset;
    internal void ProcessServerIdentification(Packet_Server packet)
    {
        this.LocalPlayerId = packet.Identification.AssignedClientId;
        this.ServerInfo.connectdata = this.connectdata;
        this.ServerInfo.ServerName = packet.Identification.ServerName;
        this.ServerInfo.ServerMotd = packet.Identification.ServerMotd;
        this.d_TerrainChunkTesselator.ENABLE_TEXTURE_TILING = packet.Identification.RenderHint_ == RenderHintEnum.Fast;
        Packet_StringList requiredMd5 = packet.Identification.RequiredBlobMd5;
        Packet_StringList requiredName = packet.Identification.RequiredBlobName;
        ChatLog("[GAME] Processed server identification");
        int getCount = 0;
        if (requiredMd5 != null)
        {
            ChatLog(platform.StringFormat("[GAME] Server has {0} assets", platform.IntToString(requiredMd5.ItemsCount)));
            for (int i = 0; i < requiredMd5.ItemsCount; i++)
            {
                string md5 = requiredMd5.Items[i];
                
                //check if file with that content is already in cache
                if (platform.IsCached(md5))
                {
                    //File has been cached. load cached version.
                    Asset cachedAsset = platform.LoadAssetFromCache(md5);
                    string name;
                    if (requiredName != null)
                    {
                        name = requiredName.Items[i];
                    }
                    else // server older than 2014-07-13.
                    {
                        name = cachedAsset.name;
                    }
                    SetFile(name, cachedAsset.md5, cachedAsset.data, cachedAsset.dataLength);
                }
                else
                {
                    //Asset not present in cache
                    if (requiredName != null)
                    {
                        //If list of names is given (server > 2014-07-13) lookup if asset is already loaded
                        if (!HasAsset(md5, requiredName.Items[i]))
                        {
                            //Request asset from server if not already loaded
                            getAsset[getCount++] = md5;
                        }
                    }
                    else
                    {
                        //Server didn't send list of required asset names
                        getAsset[getCount++] = md5;
                    }
                }
            }
            ChatLog(platform.StringFormat("[GAME] Will download {0} missing assets", platform.IntToString(getCount)));
        }
        SendGameResolution();
        ChatLog("[GAME] Sent window resolution to server");
        sendResize = true;
        SendRequestBlob(getAsset, getCount);
        ChatLog("[GAME] Sent BLOB request");
        if (packet.Identification.MapSizeX != MapSizeX
            || packet.Identification.MapSizeY != MapSizeY
            || packet.Identification.MapSizeZ != MapSizeZ)
        {
            Reset(packet.Identification.MapSizeX,
                packet.Identification.MapSizeY,
                packet.Identification.MapSizeZ);
            d_Heightmap.Restart();
        }
        terrainRenderer.shadowssimple = packet.Identification.DisableShadows == 1 ? true : false;
        maxdrawdistance = packet.Identification.PlayerAreaSize / 2;
        if (maxdrawdistance == 0)
        {
            maxdrawdistance = 128;
        }
        ChatLog("[GAME] Map initialized");
    }

    bool HasAsset(string md5, string name)
    {
        for (int i = 0; i < assets.count; i++)
        {
            if (assets.items[i].md5 == md5)
            {
                if (assets.items[i].name == name)
                {
                    //Check both MD5 and name as there might be files with same content
                    return true;
                }
            }
        }
        return false;
    }

    internal bool handRedraw;
    internal bool handSetAttackBuild;
    internal bool handSetAttackDestroy;

    internal string serverGameVersion;
    internal void ProcessPacket(Packet_Server packet)
    {
        if (packetHandlers[packet.Id] != null)
        {
            packetHandlers[packet.Id].Handle(this, packet);
        }
        switch (packet.Id)
        {
            case Packet_ServerIdEnum.ServerIdentification:
                {
                    string invalidversionstr = language.InvalidVersionConnectAnyway();

                    serverGameVersion = packet.Identification.MdProtocolVersion;
                    if (serverGameVersion != platform.GetGameVersion())
                    {
                        ChatLog("[GAME] Different game versions");
                        string q = platform.StringFormat2(invalidversionstr, platform.GetGameVersion(), serverGameVersion);
                        invalidVersionDrawMessage = q;
                        invalidVersionPacketIdentification = packet;
                    }
                    else
                    {
                        ProcessServerIdentification(packet);
                    }
                    ReceivedMapLength = 0;
                }
                break;
            case Packet_ServerIdEnum.Ping:
                {
                    this.SendPingReply();
                    this.ServerInfo.ServerPing.Send(platform);
                }
                break;
            case Packet_ServerIdEnum.PlayerPing:
                {
                    this.ServerInfo.ServerPing.Receive(platform);
                }
                break;
            case Packet_ServerIdEnum.LevelInitialize:
                {
                    ChatLog("[GAME] Initialized map loading");
                    ReceivedMapLength = 0;
                    InvokeMapLoadingProgress(0, 0, language.Connecting());
                }
                break;
            case Packet_ServerIdEnum.LevelDataChunk:
                {
                    InvokeMapLoadingProgress(packet.LevelDataChunk.PercentComplete, ReceivedMapLength, packet.LevelDataChunk.Status);
                }
                break;
            case Packet_ServerIdEnum.LevelFinalize:
                {
                    ChatLog("[GAME] Finished map loading");
                }
                break;
            case Packet_ServerIdEnum.SetBlock:
                {
                    int x = packet.SetBlock.X;
                    int y = packet.SetBlock.Y;
                    int z = packet.SetBlock.Z;
                    int type = packet.SetBlock.BlockType;
                    //try
                    {
                        SetTileAndUpdate(x, y, z, type);
                    }
                    //catch { Console.WriteLine("Cannot update tile!"); }
                }
                break;
            case Packet_ServerIdEnum.FillArea:
                {
                    int ax = packet.FillArea.X1;
                    int ay = packet.FillArea.Y1;
                    int az = packet.FillArea.Z1;
                    int bx = packet.FillArea.X2;
                    int by = packet.FillArea.Y2;
                    int bz = packet.FillArea.Z2;

                    int startx = MathCi.MinInt(ax, bx);
                    int endx = MathCi.MaxInt(ax, bx);
                    int starty = MathCi.MinInt(ay, by);
                    int endy = MathCi.MaxInt(ay, by);
                    int startz = MathCi.MinInt(az, bz);
                    int endz = MathCi.MaxInt(az, bz);

                    int blockCount = packet.FillArea.BlockCount;
                    {
                        for (int x = startx; x <= endx; x++)
                        {
                            for (int y = starty; y <= endy; y++)
                            {
                                for (int z = startz; z <= endz; z++)
                                {
                                    // if creative mode is off and player run out of blocks
                                    if (blockCount == 0)
                                    {
                                        return;
                                    }
                                    //try
                                    {
                                        SetTileAndUpdate(x, y, z, packet.FillArea.BlockType);
                                    }
                                    //catch
                                    //{
                                    //    Console.WriteLine("Cannot update tile!");
                                    //}
                                    blockCount--;
                                }
                            }
                        }
                    }
                }
                break;
            case Packet_ServerIdEnum.FillAreaLimit:
                {
                    this.fillAreaLimit = packet.FillAreaLimit.Limit;
                    if (this.fillAreaLimit > 100000)
                    {
                        this.fillAreaLimit = 100000;
                    }
                }
                break;
            case Packet_ServerIdEnum.Freemove:
                {
                    this.AllowFreemove = packet.Freemove.IsEnabled != 0;
                    if (!this.AllowFreemove)
                    {
                        ENABLE_FREEMOVE = false;
                        ENABLE_NOCLIP = false;
                        movespeed = basemovespeed;
                        Log(language.MoveNormal());
                    }
                }
                break;
            case Packet_ServerIdEnum.PlayerSpawnPosition:
                {
                    int x = packet.PlayerSpawnPosition.X;
                    int y = packet.PlayerSpawnPosition.Y;
                    int z = packet.PlayerSpawnPosition.Z;
                    this.playerPositionSpawnX = x;
                    this.playerPositionSpawnY = z;
                    this.playerPositionSpawnZ = y;
                    Log(platform.StringFormat(language.SpawnPositionSetTo(), platform.StringFormat3("{0},{1},{2}", platform.IntToString(x), platform.IntToString(y), platform.IntToString(z))));
                }
                break;
            case Packet_ServerIdEnum.Message:
                {
                    AddChatline(packet.Message.Message);
                    ChatLog(packet.Message.Message);
                }
                break;
            case Packet_ServerIdEnum.DisconnectPlayer:
                {
                    ChatLog(platform.StringFormat("[GAME] Disconnected by the server ({0})", packet.DisconnectPlayer.DisconnectReason));
                    //When server disconnects player, return to main menu
                    platform.MessageBoxShowError(packet.DisconnectPlayer.DisconnectReason, "Disconnected from server");
                    ExitToMainMenu_();
                    break;
                }
            case Packet_ServerIdEnum.PlayerStats:
                {
                    Packet_ServerPlayerStats p = packet.PlayerStats;
                    this.PlayerStats = p;
                }
                break;
            case Packet_ServerIdEnum.FiniteInventory:
                {
                    //check for null so it's possible to connect
                    //to old versions of game (before 2011-05-05)
                    if (packet.Inventory.Inventory != null)
                    {
                        //d_Inventory.CopyFrom(ConvertInventory(packet.Inventory.Inventory));
                        UseInventory(packet.Inventory.Inventory);
                    }
                    //FiniteInventory = packet.FiniteInventory.BlockTypeAmount;
                    //ENABLE_FINITEINVENTORY = packet.FiniteInventory.IsFinite;
                    //FiniteInventoryMax = packet.FiniteInventory.Max;
                }
                break;
            case Packet_ServerIdEnum.Season:
                {
                    packet.Season.Hour -= 1;
                    if (packet.Season.Hour < 0)
                    {
                        //shouldn't happen
                        packet.Season.Hour = 12 * HourDetail;
                    }
                    int sunlight = NightLevels[packet.Season.Hour];
                    SkySphereNight = sunlight < 8;
                    d_SunMoonRenderer.day_length_in_seconds = 60 * 60 * 24 / packet.Season.DayNightCycleSpeedup;
                    int hour = packet.Season.Hour / HourDetail;
                    if (d_SunMoonRenderer.GetHour() != hour)
                    {
                        d_SunMoonRenderer.SetHour(hour);
                    }

                    if (sunlight_ != sunlight)
                    {
                        sunlight_ = sunlight;
                        //d_Shadows.ResetShadows();
                        RedrawAllBlocks();
                    }
                }
                break;
            case Packet_ServerIdEnum.BlobInitialize:
                {
                    blobdownload = new CitoMemoryStream();
                    //blobdownloadhash = ByteArrayToString(packet.BlobInitialize.hash);
                    blobdownloadname = packet.BlobInitialize.Name;
                    blobdownloadmd5 = packet.BlobInitialize.Md5;
                }
                break;
            case Packet_ServerIdEnum.BlobPart:
                {
                    int length = platform.ByteArrayLength(packet.BlobPart.Data);
                    blobdownload.Write(packet.BlobPart.Data, 0, length);
                    ReceivedMapLength += length;
                }
                break;
            case Packet_ServerIdEnum.BlobFinalize:
                {
                    byte[] downloaded = blobdownload.ToArray();

                    if (blobdownloadname != null) // old servers
                    {
                        SetFile(blobdownloadname, blobdownloadmd5, downloaded, blobdownload.Length());
                    }
                    blobdownload = null;
                }
                break;
            case Packet_ServerIdEnum.Sound:
                {
                    PlaySoundAt(packet.Sound.Name, packet.Sound.X, packet.Sound.Y, packet.Sound.Z);
                }
                break;
            case Packet_ServerIdEnum.RemoveMonsters:
                {
                    for (int i = entityMonsterIdStart; i < entityMonsterIdStart + entityMonsterIdCount; i++)
                    {
                        entities[i] = null;
                    }
                }
                break;
            case Packet_ServerIdEnum.Translation:
                language.Override(packet.Translation.Lang, packet.Translation.Id, packet.Translation.Translation);
                break;
            case Packet_ServerIdEnum.BlockType:
                NewBlockTypes[packet.BlockType.Id] = packet.BlockType.Blocktype;
                break;
            case Packet_ServerIdEnum.SunLevels:
                NightLevels = packet.SunLevels.Sunlevels;
                break;
            case Packet_ServerIdEnum.LightLevels:
                for (int i = 0; i < packet.LightLevels.LightlevelsCount; i++)
                {
                    mLightLevels[i] = DeserializeFloat(packet.LightLevels.Lightlevels[i]);
                }
                break;
            case Packet_ServerIdEnum.Follow:
                IntRef oldFollowId = FollowId();
                Follow = packet.Follow.Client;
                if (packet.Follow.Tpp != 0)
                {
                    SetCamera(CameraType.Overhead);
                    player.position.rotx = Game.GetPi();
                    GuiStateBackToGame();
                }
                else
                {
                    SetCamera(CameraType.Fpp);
                }
                break;
            case Packet_ServerIdEnum.Bullet:
                EntityAddLocal(CreateBulletEntity(
                    DeserializeFloat(packet.Bullet.FromXFloat),
                    DeserializeFloat(packet.Bullet.FromYFloat),
                    DeserializeFloat(packet.Bullet.FromZFloat),
                    DeserializeFloat(packet.Bullet.ToXFloat),
                    DeserializeFloat(packet.Bullet.ToYFloat),
                    DeserializeFloat(packet.Bullet.ToZFloat),
                    DeserializeFloat(packet.Bullet.SpeedFloat)));
                break;
            case Packet_ServerIdEnum.Ammo:
                if (!ammostarted)
                {
                    ammostarted = true;
                    for (int i = 0; i < packet.Ammo.TotalAmmoCount; i++)
                    {
                        Packet_IntInt k = packet.Ammo.TotalAmmo[i];
                        LoadedAmmo[k.Key_] = MathCi.MinInt(k.Value_, blocktypes[k.Key_].AmmoMagazine);
                    }
                }
                TotalAmmo = new int[GlobalVar.MAX_BLOCKTYPES];
                for (int i = 0; i < packet.Ammo.TotalAmmoCount; i++)
                {
                    TotalAmmo[packet.Ammo.TotalAmmo[i].Key_] = packet.Ammo.TotalAmmo[i].Value_;
                }
                break;
            case Packet_ServerIdEnum.Explosion:
                {
                    Entity entity = new Entity();
                    entity.expires = new Expires();
                    entity.expires.timeLeft = DeserializeFloat(packet.Explosion.TimeFloat);
                    entity.push = packet.Explosion;
                    EntityAddLocal(entity);
                }
                break;
            case Packet_ServerIdEnum.Projectile:
                {
                    Entity entity = new Entity();

                    Sprite sprite = new Sprite();
                    sprite.image = "ChemicalGreen.png";
                    sprite.size = 14;
                    sprite.animationcount = 0;
                    sprite.positionX = DeserializeFloat(packet.Projectile.FromXFloat);
                    sprite.positionY = DeserializeFloat(packet.Projectile.FromYFloat);
                    sprite.positionZ = DeserializeFloat(packet.Projectile.FromZFloat);
                    entity.sprite = sprite;

                    Grenade_ grenade = new Grenade_();
                    grenade.velocityX = DeserializeFloat(packet.Projectile.VelocityXFloat);
                    grenade.velocityY = DeserializeFloat(packet.Projectile.VelocityYFloat);
                    grenade.velocityZ = DeserializeFloat(packet.Projectile.VelocityZFloat);
                    grenade.block = packet.Projectile.BlockId;
                    grenade.sourcePlayer = packet.Projectile.SourcePlayerID;
                    entity.grenade = grenade;

                    entity.expires = Expires.Create(DeserializeFloat(packet.Projectile.ExplodesAfterFloat));

                    EntityAddLocal(entity);
                }
                break;
            case Packet_ServerIdEnum.BlockTypes:
                blocktypes = NewBlockTypes;
                NewBlockTypes = new Packet_BlockType[GlobalVar.MAX_BLOCKTYPES];

                int textureInAtlasIdsCount = 1024;
                string[] textureInAtlasIds = new string[textureInAtlasIdsCount];
                int lastTextureId = 0;
                for (int i = 0; i < GlobalVar.MAX_BLOCKTYPES; i++)
                {
                    if (blocktypes[i] != null)
                    {
                        string[] to_load = new string[7];
                        int to_loadLength = 7;
                        {
                            to_load[0] = blocktypes[i].TextureIdLeft;
                            to_load[1] = blocktypes[i].TextureIdRight;
                            to_load[2] = blocktypes[i].TextureIdFront;
                            to_load[3] = blocktypes[i].TextureIdBack;
                            to_load[4] = blocktypes[i].TextureIdTop;
                            to_load[5] = blocktypes[i].TextureIdBottom;
                            to_load[6] = blocktypes[i].TextureIdForInventory;
                        }
                        for (int k = 0; k < to_loadLength; k++)
                        {
                            if (!Contains(textureInAtlasIds, textureInAtlasIdsCount, to_load[k]))
                            {
                                textureInAtlasIds[lastTextureId++] = to_load[k];
                            }
                        }
                    }
                }
                d_Data.UseBlockTypes(platform, blocktypes, GlobalVar.MAX_BLOCKTYPES);
                for (int i = 0; i < GlobalVar.MAX_BLOCKTYPES; i++)
                {
                    Packet_BlockType b = blocktypes[i];
                    if (b == null)
                    {
                        continue;
                    }
                    //Indexed by block id and TileSide.
                    if (textureInAtlasIds != null)
                    {
                        TextureId[i][0] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdTop);
                        TextureId[i][1] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdBottom);
                        TextureId[i][2] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdFront);
                        TextureId[i][3] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdBack);
                        TextureId[i][4] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdLeft);
                        TextureId[i][5] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdRight);
                        TextureIdForInventory[i] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdForInventory);
                    }
                }
                UseTerrainTextures(textureInAtlasIds, textureInAtlasIdsCount);
                handRedraw = true;
                RedrawAllBlocks();
                break;
            case Packet_ServerIdEnum.ServerRedirect:
                ChatLog("[GAME] Received server redirect");
                //Leave current server
                SendLeave(Packet_LeaveReasonEnum.Leave);
                //Exit game screen and create new game instance
                ExitAndSwitchServer(packet.Redirect);
                break;
        }
    }
    internal ClientPacketHandler[] packetHandlers;

    void CacheAsset(Asset asset)
    {
        //Check if checksum is given (prevents crash on old servers)
        if (asset.md5 == null)
        {
            return;
        }
        //Check if given checksum is valid
        if (!platform.IsChecksum(asset.md5))
        {
            //Skip saving
            return;
        }
        //Only cache a file if it's not already cached
        if (!platform.IsCached(asset.md5))
        {
            platform.SaveAssetToCache(asset);
        }
    }

    void SetFile(string name, string md5, byte[] downloaded, int downloadedLength)
    {
        string nameLowercase = platform.StringToLower(name);
        
        //Create new asset from given data
        Asset newAsset = new Asset();
        newAsset.data = downloaded;
        newAsset.dataLength = downloadedLength;
        newAsset.name = nameLowercase;
        newAsset.md5 = md5;
        
        for (int i = 0; i < assets.count; i++)
        {
            if (assets.items[i] == null)
            {
                continue;
            }
            if (assets.items[i].name == nameLowercase)
            {
                if (options.UseServerTextures)
                {
                    //If server textures are allowed, replace content of current asset
                    assets.items[i] = newAsset;
                }
                //Cache asset for later use
                CacheAsset(newAsset);
                return;
            }
        }
        //Add new asset to asset list
        assets.items[assets.count++] = newAsset;
        
        //Store new asset in cache
        CacheAsset(newAsset);
    }

    internal int handTexture;

    bool ammostarted;
    internal Packet_BlockType[] NewBlockTypes;
    internal string blobdownloadname;
    internal string blobdownloadmd5;
    internal CitoMemoryStream blobdownload;
    internal SunMoonRenderer d_SunMoonRenderer;
    internal int[] NightLevels;
    public const int HourDetail = 4;
    public static int[] ByteArrayToUshortArray(byte[] input, int inputLength)
    {
        int outputLength = inputLength / 2;
        int[] output = new int[outputLength];
        for (int i = 0; i < outputLength; i++)
        {
            output[i] = (input[i * 2 + 1] << 8) + input[i * 2];
        }
        return output;
    }

    internal byte[] GetFile(string p)
    {
        string pLowercase = platform.StringToLower(p);
        for (int i = 0; i < assets.count; i++)
        {
            if (assets.items[i].name == pLowercase)
            {
                return assets.items[i].data;
            }
        }
        return null;
    }

    internal int GetFileLength(string p)
    {
        string pLowercase = platform.StringToLower(p);
        for (int i = 0; i < assets.count; i++)
        {
            if (assets.items[i].name == pLowercase)
            {
                return assets.items[i].dataLength;
            }
        }
        return 0;
    }

    internal void InvalidVersionAllow()
    {
        if (invalidVersionDrawMessage != null)
        {
            invalidVersionDrawMessage = null;
            ProcessServerIdentification(invalidVersionPacketIdentification);
            invalidVersionPacketIdentification = null;
        }
    }

    internal int maxTextureSize; // detected at runtime
    internal int atlas1dheight() { return maxTextureSize; }
    internal int atlas2dtiles() { return GlobalVar.MAX_BLOCKTYPES_SQRT; } // 16x16
    internal TextureAtlasConverter d_TextureAtlasConverter;

    internal void UseTerrainTextureAtlas2d(BitmapCi atlas2d, int atlas2dWidth)
    {
        terrainTexture = platform.LoadTextureFromBitmap(atlas2d);
        int[] terrainTextures1d_;
        int terrainTextures1dCount = 0;
        {
            terrainTexturesPerAtlas = atlas1dheight() / (atlas2dWidth / atlas2dtiles());
            IntRef atlasesidCount = new IntRef();
            BitmapCi[] atlases1d = d_TextureAtlasConverter.Atlas2dInto1d(platform, atlas2d, atlas2dtiles(), atlas1dheight(), atlasesidCount);
            terrainTextures1d_ = new int[atlasesidCount.value];
            for (int i = 0; i < atlasesidCount.value; i++)
            {
                BitmapCi bmp = atlases1d[i];
                int texture = platform.LoadTextureFromBitmap(bmp);
                terrainTextures1d_[terrainTextures1dCount++] = texture;
                platform.BitmapDelete(bmp);
            }
        }
        this.terrainTextures1d = terrainTextures1d_;
    }

    internal void UseTerrainTextures(string[] textureIds, int textureIdsCount)
    {
        //todo bigger than 32x32
        int tilesize = 32;
        BitmapData_ atlas2d = BitmapData_.Create(tilesize * atlas2dtiles(), tilesize * atlas2dtiles());

        for (int i = 0; i < textureIdsCount; i++)
        {
            if (textureIds[i] == null)
            {
                continue;
            }
            byte[] fileData = GetFile(StringTools.StringAppend(platform, textureIds[i], ".png"));
            if (fileData == null)
            {
                fileData = GetFile("Unknown.png");
            }
            if (fileData == null)
            {
                continue;
            }
            BitmapCi bmp = platform.BitmapCreateFromPng(fileData, platform.ByteArrayLength(fileData));
            if (platform.BitmapGetWidth(bmp) != tilesize)
            {
                platform.BitmapDelete(bmp);
                continue;
            }
            if (platform.BitmapGetHeight(bmp) != tilesize)
            {
                platform.BitmapDelete(bmp);
                continue;
            }
            int[] bmpPixels = new int[tilesize * tilesize];
            platform.BitmapGetPixelsArgb(bmp, bmpPixels);

            int x = i % texturesPacked();
            int y = i / texturesPacked();
            for (int xx = 0; xx < tilesize; xx++)
            {
                for (int yy = 0; yy < tilesize; yy++)
                {
                    int c = bmpPixels[xx + yy * tilesize];
                    atlas2d.SetPixel(x * tilesize + xx, y * tilesize + yy, c);
                }
            }

            platform.BitmapDelete(bmp);
        }
        BitmapCi bitmap = platform.BitmapCreate(atlas2d.width, atlas2d.height);
        platform.BitmapSetPixelsArgb(bitmap, atlas2d.argb);
        UseTerrainTextureAtlas2d(bitmap, atlas2d.width);
    }

    int IndexOf(string[] arr, int arrLength, string value)
    {
        for (int i = 0; i < arrLength; i++)
        {
            if (StringEquals(arr[i], value))
            {
                return i;
            }
        }
        return -1;
    }

    public static bool StringEquals(string strA, string strB)
    {
        if (strA == null && strB == null)
        {
            return true;
        }
        if (strA == null || strB == null)
        {
            return false;
        }
        return strA == strB;
    }

    bool Contains(string[] arr, int arrLength, string value)
    {
        return IndexOf(arr, arrLength, value) != -1;
    }

    internal AnimationState localplayeranim;
    internal AnimationHint localplayeranimationhint;

    internal bool enable_move;

    public const int DISCONNECTED_ICON_AFTER_SECONDS = 10;
    internal void KeyDown(int eKey)
    {
        for (int i = 0; i < clientmodsCount; i++)
        {
            KeyEventArgs args_ = new KeyEventArgs();
            args_.SetKeyCode(eKey);
            clientmods[i].OnKeyDown(this, args_);
            if (args_.GetHandled())
            {
                return;
            }
        }
        keyboardState[eKey] = true;
        InvalidVersionAllow();
        if (eKey == GetKey(GlKeys.F6))
        {
            float lagSeconds = one * (platform.TimeMillisecondsFromStart() - LastReceivedMilliseconds) / 1000;
            if ((lagSeconds >= DISCONNECTED_ICON_AFTER_SECONDS) || guistate == GuiState.MapLoading)
            {
                Reconnect();
            }
        }
        if (eKey == GetKey(GlKeys.ShiftLeft) || eKey == GetKey(GlKeys.ShiftRight))
        {
            IsShiftPressed = true;
        }
        if (guistate == GuiState.Normal)
        {
            string strFreemoveNotAllowed = "You are not allowed to enable freemove.";

            if (eKey == GetKey(GlKeys.F1))
            {
                if (!this.AllowFreemove)
                {
                    Log(strFreemoveNotAllowed);
                    return;
                }
                movespeed = basemovespeed * 1;
                Log("Move speed: 1x.");
            }
            if (eKey == GetKey(GlKeys.F2))
            {
                if (!this.AllowFreemove)
                {
                    Log(strFreemoveNotAllowed);
                    return;
                }
                movespeed = basemovespeed * 10;
                Log(platform.StringFormat(language.MoveSpeed(), platform.IntToString(10)));
            }
            if (eKey == GetKey(GlKeys.F3))
            {
                if (!this.AllowFreemove)
                {
                    Log(strFreemoveNotAllowed);
                    return;
                }
                player.physicsState.movedz = 0;
                if (!ENABLE_FREEMOVE)
                {
                    ENABLE_FREEMOVE = true;
                    Log(language.MoveFree());
                }
                else if (ENABLE_FREEMOVE && (!ENABLE_NOCLIP))
                {
                    ENABLE_NOCLIP = true;
                    Log(language.MoveFreeNoclip());
                }
                else if (ENABLE_FREEMOVE && ENABLE_NOCLIP)
                {
                    ENABLE_FREEMOVE = false;
                    ENABLE_NOCLIP = false;
                    Log(language.MoveNormal());
                }
            }
            if (eKey == GetKey(GlKeys.I))
            {
                drawblockinfo = !drawblockinfo;
            }
            int playerx = platform.FloatToInt(player.position.x);
            int playery = platform.FloatToInt(player.position.z);
            if ((playerx >= 0 && playerx < MapSizeX)
                && (playery >= 0 && playery < MapSizeY))
            {
                performanceinfo.Set("height", platform.StringFormat("height:{0}", platform.IntToString(d_Heightmap.GetBlock(playerx, playery))));
            }
            if (eKey == GetKey(GlKeys.F5))
            {
                CameraChange();
            }
            if (eKey == GetKey(GlKeys.Plus) || eKey == GetKey(GlKeys.KeypadPlus))
            {
                if (cameratype == CameraType.Overhead)
                {
                    overheadcameradistance -= 1;
                }
                else if (cameratype == CameraType.Tpp)
                {
                    tppcameradistance -= 1;
                }
            }
            if (eKey == GetKey(GlKeys.Minus) || eKey == GetKey(GlKeys.KeypadMinus))
            {
                if (cameratype == CameraType.Overhead)
                {
                    overheadcameradistance += 1;
                }
                else if (cameratype == CameraType.Tpp)
                {
                    tppcameradistance += 1;
                }
            }
            if (overheadcameradistance < TPP_CAMERA_DISTANCE_MIN) { overheadcameradistance = TPP_CAMERA_DISTANCE_MIN; }
            if (overheadcameradistance > TPP_CAMERA_DISTANCE_MAX) { overheadcameradistance = TPP_CAMERA_DISTANCE_MAX; }

            if (tppcameradistance < TPP_CAMERA_DISTANCE_MIN) { tppcameradistance = TPP_CAMERA_DISTANCE_MIN; }
            if (tppcameradistance > TPP_CAMERA_DISTANCE_MAX) { tppcameradistance = TPP_CAMERA_DISTANCE_MAX; }

            if (eKey == GetKey(GlKeys.F6))
            {
                RedrawAllBlocks();
            }
            if (eKey == GlKeys.F8)
            {
                ToggleVsync();
                if (ENABLE_LAG == 0) { Log(language.FrameRateVsync()); }
                if (ENABLE_LAG == 1) { Log(language.FrameRateUnlimited()); }
                if (ENABLE_LAG == 2) { Log(language.FrameRateLagSimulation()); }
            }
            if (eKey == GetKey(GlKeys.Tab))
            {
                SendPacketClient(ClientPackets.SpecialKeyTabPlayerList());
            }
            if (eKey == GetKey(GlKeys.E))
            {
                if (currentAttackedBlock != null)
                {
                    int posX = currentAttackedBlock.X;
                    int posY = currentAttackedBlock.Y;
                    int posZ = currentAttackedBlock.Z;
                    int blocktype = GetBlock(currentAttackedBlock.X, currentAttackedBlock.Y, currentAttackedBlock.Z);
                    if (IsUsableBlock(blocktype))
                    {
                        if (d_Data.IsRailTile(blocktype))
                        {
                            player.position.x = posX + (one / 2);
                            player.position.y = posZ + 1;
                            player.position.z = posY + (one / 2);
                            ENABLE_FREEMOVE = false;
                        }
                        else
                        {
                            SendSetBlock(posX, posY, posZ, Packet_BlockSetModeEnum.Use, 0, ActiveMaterial);
                        }
                    }
                }
                if (currentlyAttackedEntity != -1)
                {
                    if (entities[currentlyAttackedEntity].usable)
                    {
                        for (int i = 0; i < clientmodsCount; i++)
                        {
                            if (clientmods[i] == null) { continue; }
                            OnUseEntityArgs args = new OnUseEntityArgs();
                            args.entityId = currentlyAttackedEntity;
                            clientmods[i].OnUseEntity(this, args);
                        }
                        SendPacketClient(ClientPackets.UseEntity(currentlyAttackedEntity));
                    }
                }
            }
            if (eKey == GetKey(GlKeys.O))
            {
                Respawn();
            }
            if (eKey == GetKey(GlKeys.L))
            {
                SendPacketClient(ClientPackets.SpecialKeySelectTeam());
            }
            if (eKey == GetKey(GlKeys.P))
            {
                SendPacketClient(ClientPackets.SpecialKeySetSpawn());

                playerPositionSpawnX = player.position.x;
                playerPositionSpawnY = player.position.y;
                playerPositionSpawnZ = player.position.z;

                player.position.x = platform.FloatToInt(player.position.x) + one / 2;
                //player.playerposition.Y = player.playerposition.Y;
                player.position.z = platform.FloatToInt(player.position.z) + one / 2;
            }
            if (eKey == GetKey(GlKeys.F))
            {
                ToggleFog();
                Log(platform.StringFormat(language.FogDistance(), platform.IntToString(platform.FloatToInt(d_Config3d.viewdistance))));
                OnResize();
            }
            if (eKey == GetKey(GlKeys.B))
            {
                ShowInventory();
                return;
            }
            HandleMaterialKeys(eKey);
        }
        if (guistate == GuiState.Inventory)
        {
            if (eKey == GetKey(GlKeys.B)
                || eKey == GetKey(GlKeys.Escape))
            {
                GuiStateBackToGame();
            }
            return;
        }
        if (guistate == GuiState.MapLoading)
        {
            //Return to main menu when ESC key is pressed while loading
            if (eKey == GetKey(GlKeys.Escape))
            {
                ExitToMainMenu_();
            }
        }
        if (guistate == GuiState.CraftingRecipes)
        {
            if (eKey == GetKey(GlKeys.Escape))
            {
                GuiStateBackToGame();
            }
        }
        if (guistate == GuiState.Normal)
        {
            if (eKey == GetKey(GlKeys.Escape))
            {
                EscapeMenuStart();
                return;
            }
        }
    }

    internal bool escapeMenuRestart;
    public void EscapeMenuStart()
    {
        guistate = GuiState.EscapeMenu;
        menustate = new MenuState();
        platform.ExitMousePointerLock();
        escapeMenuRestart = true;
    }

    public void ShowEscapeMenu()
    {
        guistate = GuiState.EscapeMenu;
        menustate = new MenuState();
        SetFreeMouse(true);
    }

    public void ShowInventory()
    {
        guistate = GuiState.Inventory;
        menustate = new MenuState();
        SetFreeMouse(true);
    }

    public void CameraChange()
    {
        if (Follow != null)
        {
            //Prevents switching camera mode when following
            return;
        }
        if (cameratype == CameraType.Fpp)
        {
            cameratype = CameraType.Tpp;
            ENABLE_TPP_VIEW = true;
        }
        else if (cameratype == CameraType.Tpp)
        {
            cameratype = CameraType.Overhead;
            overheadcamera = true;
            SetFreeMouse(true);
            ENABLE_TPP_VIEW = true;
            playerdestination = Vector3Ref.Create(player.position.x, player.position.y, player.position.z);
        }
        else if (cameratype == CameraType.Overhead)
        {
            cameratype = CameraType.Fpp;
            SetFreeMouse(false);
            ENABLE_TPP_VIEW = false;
            overheadcamera = false;
        }
        else
        {
            platform.ThrowException("");
        }
    }
    internal bool drawblockinfo;

    internal void UpdateTitleFps(float dt)
    {
        float elapsed = one * (platform.TimeMillisecondsFromStart() - lasttitleupdateMilliseconds) / 1000;
        if (elapsed >= 1)
        {
            lasttitleupdateMilliseconds = platform.TimeMillisecondsFromStart();
            int chunkupdates = terrainRenderer.ChunkUpdates();
            performanceinfo.Set("chunk updates", platform.StringFormat(language.ChunkUpdates(), platform.IntToString(chunkupdates - lastchunkupdates)));
            lastchunkupdates = terrainRenderer.ChunkUpdates();
            performanceinfo.Set("triangles", platform.StringFormat(language.Triangles(), platform.IntToString(terrainRenderer.TrianglesCount())));
        }
        if (!titleset)
        {
            platform.SetTitle(language.GameName());
            titleset = true;
        }
    }
    bool titleset;

    internal void Draw2d(float dt)
    {
        OrthoMode(Width(), Height());
        switch (guistate)
        {
            case GuiState.Normal:
                {
                    if (!ENABLE_DRAW2D)
                    {
                        PerspectiveMode();
                        return;
                    }
                }
                break;
            case GuiState.Inventory:
                {
                    //d_The3d.ResizeGraphics(Width, Height);
                    //d_The3d.OrthoMode(d_HudInventory.ConstWidth, d_HudInventory.ConstHeight);
                    //d_The3d.PerspectiveMode();
                }
                break;
            case GuiState.MapLoading:
                {
                }
                break;
            case GuiState.ModalDialog:
                {
                }
                break;
            case GuiState.EscapeMenu:
                {
                    if (!ENABLE_DRAW2D)
                    {
                        PerspectiveMode();
                        return;
                    }
                }
                break;
            case GuiState.CraftingRecipes:
                {
                }
                break;
        }
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnNewFrameDraw2d(this, dt);
        }

        //d_The3d.OrthoMode(Width, Height);

        PerspectiveMode();
    }

    public const int ChatFontSize = 11;

    internal BoolRef soundnow;

    internal float movedx;
    internal float movedy;
    internal float pushX;
    internal float pushY;
    internal float pushZ;
    internal bool wantsjump;
    internal bool shiftkeydown;
    internal bool moveup;
    internal bool movedown;
    internal float jumpstartacceleration;
    internal Acceleration acceleration;

    internal void FrameTick(float dt)
    {
        //if ((DateTime.Now - lasttodo).TotalSeconds > BuildDelay && todo.Count > 0)
        //UpdateTerrain();
        NewFrameEventArgs args_ = new NewFrameEventArgs();
        args_.SetDt(dt);
        for (int i = 0; i < clientmodsCount; i++)
        {
            clientmods[i].OnNewFrameFixed(this, args_);
        }
        OnNewFrame(dt);

        if (guistate == GuiState.MapLoading) { return; }

        if (guistate == GuiState.EscapeMenu)
        {
        }
        else if (guistate == GuiState.Inventory)
        {
        }
        else if (guistate == GuiState.MapLoading)
        {
            //todo back to game when escape key pressed.
        }
        else if (guistate == GuiState.CraftingRecipes)
        {
        }
        else if (guistate == GuiState.ModalDialog)
        {
        }

        float orientationX = platform.MathSin(player.position.roty);
        float orientationY = 0;
        float orientationZ = -platform.MathCos(player.position.roty);
        platform.AudioUpdateListener(EyesPosX(), EyesPosY(), EyesPosZ(), orientationX, orientationY, orientationZ);

        playervelocity.X = player.position.x - lastplayerpositionX;
        playervelocity.Y = player.position.y - lastplayerpositionY;
        playervelocity.Z = player.position.z - lastplayerpositionZ;
        playervelocity.X *= 75;
        playervelocity.Y *= 75;
        playervelocity.Z *= 75;
        lastplayerpositionX = player.position.x;
        lastplayerpositionY = player.position.y;
        lastplayerpositionZ = player.position.z;
    }

    public void Update(float dt)
    {
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnNewFrameReadOnlyMainThread(this, dt);
        }
    }

    float lastplayerpositionX;
    float lastplayerpositionY;
    float lastplayerpositionZ;

    public BlockPosSide[] Pick(BlockOctreeSearcher s_, Line3D line, IntRef retCount)
    {
        //pick terrain
        int minX = platform.FloatToInt(MathCi.MinFloat(line.Start[0], line.End[0]));
        int minY = platform.FloatToInt(MathCi.MinFloat(line.Start[1], line.End[1]));
        int minZ = platform.FloatToInt(MathCi.MinFloat(line.Start[2], line.End[2]));
        if (minX < 0) { minX = 0; }
        if (minY < 0) { minY = 0; }
        if (minZ < 0) { minZ = 0; }
        int maxX = platform.FloatToInt(MathCi.MaxFloat(line.Start[0], line.End[0]));
        int maxY = platform.FloatToInt(MathCi.MaxFloat(line.Start[1], line.End[1]));
        int maxZ = platform.FloatToInt(MathCi.MaxFloat(line.Start[2], line.End[2]));
        if (maxX > MapSizeX) { maxX = MapSizeX; }
        if (maxY > MapSizeZ) { maxY = MapSizeZ; }
        if (maxZ > MapSizeY) { maxZ = MapSizeY; }
        int sizex = maxX - minX + 1;
        int sizey = maxY - minY + 1;
        int sizez = maxZ - minZ + 1;
        int size = BitTools.NextPowerOfTwo(MathCi.MaxInt(sizex, MathCi.MaxInt(sizey, sizez)));
        s_.StartBox = Box3D.Create(minX, minY, minZ, size);
        //s_.StartBox = Box3D.Create(0, 0, 0, BitTools.NextPowerOfTwo(MaxInt(MapSizeX, MaxInt(MapSizeY, MapSizeZ))));
        BlockPosSide[] pick2 = s_.LineIntersection(IsBlockEmpty_.Create(this), GetBlockHeight_.Create(this), line, retCount);
        PickSort(pick2, retCount.value, line.Start[0], line.Start[1], line.Start[2]);
        return pick2;
    }

    float[] modelViewInverted;

    void PickSort(BlockPosSide[] pick, int pickCount, float x, float y, float z)
    {
        bool changed = false;
        do
        {
            changed = false;
            for (int i = 0; i < pickCount - 1; i++)
            {
                float dist = Dist(pick[i].blockPos[0], pick[i].blockPos[1], pick[i].blockPos[2], x, y, z);
                float distNext = Dist(pick[i + 1].blockPos[0], pick[i + 1].blockPos[1], pick[i + 1].blockPos[2], x, y, z);
                if (dist > distNext)
                {
                    changed = true;

                    BlockPosSide swapTemp = pick[i];
                    pick[i] = pick[i + 1];
                    pick[i + 1] = swapTemp;
                }
            }
        }
        while (changed);
    }

    internal bool mouseLeft;
    internal bool mouseMiddle;
    internal bool mouseRight;

    internal void MouseDown(MouseEventArgs args)
    {
        if (args.GetButton() == MouseButtonEnum.Left) { mouseLeft = true; }
        if (args.GetButton() == MouseButtonEnum.Middle) { mouseMiddle = true; }
        if (args.GetButton() == MouseButtonEnum.Right) { mouseRight = true; }
        if (args.GetButton() == MouseButtonEnum.Left)
        {
            mouseleftclick = true;
        }
        if (args.GetButton() == MouseButtonEnum.Right)
        {
            mouserightclick = true;
        }
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnMouseDown(this, args);
        }
        if (mousePointerLockShouldBe)
        {
            platform.RequestMousePointerLock();
            mouseDeltaX = 0;
            mouseDeltaY = 0;
        }
        InvalidVersionAllow();
    }

    internal void MouseUp(MouseEventArgs args)
    {
        if (args.GetButton() == MouseButtonEnum.Left) { mouseLeft = false; }
        if (args.GetButton() == MouseButtonEnum.Middle) { mouseMiddle = false; }
        if (args.GetButton() == MouseButtonEnum.Right) { mouseRight = false; }
        if (args.GetButton() == MouseButtonEnum.Left)
        {
            mouseleftdeclick = true;
        }
        if (args.GetButton() == MouseButtonEnum.Right)
        {
            mouserightdeclick = true;
        }
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnMouseUp(this, args);
        }
    }

    public GamePlatform GetPlatform()
    {
        return platform;
    }

    public void SetPlatform(GamePlatform value)
    {
        platform = value;
    }

    internal int Font;
    internal GameExit d_Exit;

    internal void OnFocusChanged()
    {
        if (guistate == GuiState.Normal)
        {
            EscapeMenuStart();
        }
    }

    internal void OnLoad()
    {
        int maxTextureSize_ = platform.GlGetMaxTextureSize();
        if (maxTextureSize_ < 1024)
        {
            maxTextureSize_ = 1024;
        }
        maxTextureSize = maxTextureSize_;
        //Start();
        //Connect();
        MapLoadingStart();
        platform.GlClearColorRgbaf(0, 0, 0, 1);
        if (d_Config3d.ENABLE_BACKFACECULLING)
        {
            platform.GlDepthMask(true);
            platform.GlEnableDepthTest();
            platform.GlCullFaceBack();
            platform.GlEnableCullFace();
        }
        platform.GlEnableLighting();
        platform.GlEnableColorMaterial();
        platform.GlColorMaterialFrontAndBackAmbientAndDiffuse();
        platform.GlShadeModelSmooth();
    }

    internal void Connect__()
    {
        if (connectdata.ServerPassword == null || connectdata.ServerPassword == "")
        {
            Connect(connectdata.Ip, connectdata.Port, connectdata.Username, connectdata.Auth);
        }
        else
        {
            Connect_(connectdata.Ip, connectdata.Port, connectdata.Username, connectdata.Auth, connectdata.ServerPassword);
        }
        MapLoadingStart();
    }

    public void OnRenderFrame(float deltaTime)
    {
        TaskScheduler(deltaTime);
    }

    internal float[] camera;
    float accumulator;
    internal void MainThreadOnRenderFrame(float deltaTime)
    {
        UpdateResize();

        if (guistate == GuiState.MapLoading)
        {
            platform.GlClearColorRgbaf(0, 0, 0, 1);
        }
        else
        {
            platform.GlClearColorRgbaf(one * Game.clearcolorR / 255, one * Game.clearcolorG / 255, one * Game.clearcolorB / 255, one * Game.clearcolorA / 255);
        }

        if (guistate == GuiState.Normal && enableCameraControl)
        {
            UpdateMouseViewportControl(deltaTime);
        }

        //Sleep is required in Mono for running the terrain background thread.
        platform.ApplicationDoEvents();

        accumulator += deltaTime;
        float dt = one / 75;

        while (accumulator >= dt)
        {
            FrameTick(dt);
            accumulator -= dt;
        }

        if (guistate == GuiState.MapLoading)
        {
            GotoDraw2d(deltaTime);
            return;
        }

        if (ENABLE_LAG == 2)
        {
            platform.ThreadSpinWait(20 * 1000 * 1000);
        }

        SetAmbientLight(terraincolor());
        UpdateTitleFps(deltaTime);
        platform.GlClearColorBufferAndDepthBuffer();
        platform.BindTexture2d(d_TerrainTextures.terrainTexture());

        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnBeforeNewFrameDraw3d(this, deltaTime);
        }
        GLMatrixModeModelView();
        GLLoadMatrix(camera);
        CameraMatrix.lastmvmatrix = camera;

        d_FrustumCulling.CalcFrustumEquations();

        bool drawgame = guistate != GuiState.MapLoading;
        if (drawgame)
        {
            platform.GlEnableDepthTest();
            for (int i = 0; i < clientmodsCount; i++)
            {
                if (clientmods[i] == null) { continue; }
                clientmods[i].OnNewFrameDraw3d(this, deltaTime);
            }
        }
        GotoDraw2d(deltaTime);
    }

    int lastWidth;
    int lastHeight;
    void UpdateResize()
    {
        if (lastWidth != platform.GetCanvasWidth()
            || lastHeight != platform.GetCanvasHeight())
        {
            lastWidth = platform.GetCanvasWidth();
            lastHeight = platform.GetCanvasHeight();
            OnResize();
        }
    }

    bool startedconnecting;
    internal void GotoDraw2d(float dt)
    {
        SetAmbientLight(Game.ColorFromArgb(255, 255, 255, 255));
        Draw2d(dt);

        NewFrameEventArgs args_ = new NewFrameEventArgs();
        args_.SetDt(dt);
        for (int i = 0; i < clientmodsCount; i++)
        {
            clientmods[i].OnNewFrame(this, args_);
        }

        mouseleftclick = mouserightclick = false;
        mouseleftdeclick = mouserightdeclick = false;
        if ((!issingleplayer)
            || (issingleplayer && platform.SinglePlayerServerLoaded())
            || (!platform.SinglePlayerServerAvailable()))
        {
            if (!startedconnecting)
            {
                startedconnecting = true;
                Connect__();
            }
        }
    }

    public float Scale()
    {
        //Only scale things on mobile devices
        if (platform.IsSmallScreen())
        {
            float scale = one * Width() / 1280;
            return scale;
        }
        else
        {
            return one;
        }
    }

    public void OnTouchStart(TouchEventArgs e)
    {
        InvalidVersionAllow();
        mouseCurrentX = e.GetX();
        mouseCurrentY = e.GetY();
        mouseleftclick = true;

        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnTouchStart(this, e);
            if (e.GetHandled())
            {
                return;
            }
        }
    }

    internal float touchMoveDx;
    internal float touchMoveDy;
    internal float touchOrientationDx;
    internal float touchOrientationDy;

    public void OnTouchMove(TouchEventArgs e)
    {
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnTouchMove(this, e);
            if (e.GetHandled())
            {
                return;
            }
        }
    }

    public void OnTouchEnd(TouchEventArgs e)
    {
        mouseCurrentX = 0;
        mouseCurrentY = 0;
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnTouchEnd(this, e);
            if (e.GetHandled())
            {
                return;
            }
        }
    }

    public void OnBackPressed()
    {
    }

    public void MouseMove(MouseEventArgs e)
    {
        mouseCurrentX = e.GetX();
        mouseCurrentY = e.GetY();
        mouseDeltaX += e.GetMovementX();
        mouseDeltaY += e.GetMovementY();
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnMouseMove(this, e);
        }
    }

    TaskScheduler_ scheduler;

    void TaskScheduler(float deltaTime)
    {
        scheduler.Update(deltaTime);
    }

    public void QueueTaskReadOnlyBackgroundPerFrame(Task task)
    {
        scheduler.QueueTaskReadOnlyBackgroundPerFrame(task);
    }

    public void QueueTaskCommit(Task task)
    {
        scheduler.QueueTaskCommit(task);
    }

    public void QueueTaskReadOnlyMainThread(Task task)
    {
        scheduler.QueueTaskReadOnlyMainThread(task);
    }

    public void DrawModel(Model model)
    {
        SetMatrixUniformModelView();
        platform.DrawModel(model);
    }

    public void DrawModels(Model[] model, int count)
    {
        SetMatrixUniformModelView();
        platform.DrawModels(model, count);
    }

    public void DrawModelData(ModelData data)
    {
        SetMatrixUniformModelView();
        platform.DrawModelData(data);
    }

    public void Dispose()
    {
        terrainRenderer.Clear();
        for (int i = 0; i < textures.count; i++)
        {
            if (textures.items[i] == null)
            {
                continue;
            }
            platform.GLDeleteTexture(textures.items[i].value);
        }
        for (int i = 0; i < cachedTextTexturesMax; i++)
        {
            if (cachedTextTextures[i] == null)
            {
                continue;
            }
            if (cachedTextTextures[i].texture == null)
            {
                continue;
            }
            platform.GLDeleteTexture(cachedTextTextures[i].texture.textureId);
        }
    }
    
    public void StartTyping()
    {
        GuiTyping = TypingState.Typing;
        IsTyping = true;
        GuiTypingBuffer = "";
        IsTeamchat = false;
    }

    public void StopTyping()
    {
        GuiTyping = TypingState.None;
    }

    internal float sunPositionX;
    internal float sunPositionY;
    internal float sunPositionZ;
    internal float moonPositionX;
    internal float moonPositionY;
    internal float moonPositionZ;
    internal bool isNight;
    internal bool fancySkysphere;

    internal static float Angle256ToRad(int value)
    {
        float one_ = 1;
        return ((one_ * value) / 255) * GetPi() * 2;
    }

    internal static float RadToAngle256(float value)
    {
        return (value / (2 * GetPi())) * 255;
    }
}
