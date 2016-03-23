public class Game
{
    public Game()
    {
        one = 1;
        map = new Map();
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
        for (int i = 0; i < cachedTextTexturesMax; i++)
        {
            cachedTextTextures[i] = null;
        }
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
        PICK_DISTANCE = 4.1f;
        selectedmodelid = -1;
        grenadetime = 3;
        rotationspeed = one * 15 / 100;
        entities = new Entity[entitiesMax];
        for (int i = 0; i < entitiesMax; i++)
        {
            entities[i] = null;
        }
        entitiesCount = 512;
        PlayerPushDistance = 2;
        const int KeysMax = 256;
        keyboardState = new bool[KeysMax];
        for (int i = 0; i < KeysMax; i++)
        {
            keyboardState[i] = false;
        }
        keyboardStateRaw = new bool[KeysMax];
        for (int i = 0; i < KeysMax; i++)
        {
            keyboardStateRaw[i] = false;
        }
        overheadcameradistance = 10;
        tppcameradistance = 3;
        TPP_CAMERA_DISTANCE_MIN = 1;
        TPP_CAMERA_DISTANCE_MAX = 10;
        options = new OptionsCi();
        overheadcameraK = new Kamera();
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
        soundnow = new BoolRef();
        camera = Mat4.Create();
        packetHandlers = new ClientPacketHandler[256];
        player = new Entity();
        player.position = new EntityPosition_();
        currentlyAttackedEntity = -1;
        ChatLinesMax = 1;
        ChatLines = new Chatline[ChatLinesMax];
        ChatLineLength = 64;
        audio = new AudioControl();
        CameraEyeX = -1;
        CameraEyeY = -1;
        CameraEyeZ = -1;
        controls = new Controls();
        movedz = 0;
        taskScheduler = new TaskScheduler();
        commitActions = ListAction.Create(16 * 1024);
        constWallDistance = 0.3f;
        mouseSmoothing = true;
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

        ITerrainTextures terrainTextures = new ITerrainTextures();
        terrainTextures.game = this;
        d_TextureAtlasConverter = new TextureAtlasConverter();
        d_TerrainTextures = terrainTextures;

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
        d_Data = gamedata;
        d_DataMonsters = new GameDataMonsters();
        d_Config3d = config3d;

        ModDrawParticleEffectBlockBreak particle = new ModDrawParticleEffectBlockBreak();
        this.particleEffectBlockBreak = particle;
        this.d_Data = gamedata;
        d_TerrainTextures = terrainTextures;

        map.Reset(256, 256, 128);

        SunMoonRenderer sunmoonrenderer = new SunMoonRenderer();
        d_SunMoonRenderer = sunmoonrenderer;
        d_SunMoonRenderer = sunmoonrenderer;
        d_Heightmap = new InfiniteMapChunked2d();
        d_Heightmap.d_Map = this;
        d_Heightmap.Restart();
        d_TerrainChunkTesselator = terrainchunktesselator;
        terrainchunktesselator.game = this;

        Packet_Inventory inventory = new Packet_Inventory();
        inventory.RightHand = new Packet_Item[10];
        GameDataItemsClient dataItems = new GameDataItemsClient();
        dataItems.game = this;
        InventoryUtilClient inventoryUtil = new InventoryUtilClient();
        d_Inventory = inventory;
        d_InventoryUtil = inventoryUtil;
        inventoryUtil.d_Inventory = inventory;
        inventoryUtil.d_Items = dataItems;
        d_Inventory = inventory;
        platform.AddOnCrash(OnCrashHandlerLeave.Create(this));

        rnd = platform.RandomCreate();

        clientmods = new ClientMod[128];
        clientmodsCount = 0;
        modmanager.game = this;
        AddMod(new ModDrawMain());
        AddMod(new ModUpdateMain());
        AddMod(new ModNetworkProcess());
        AddMod(new ModUnloadRendererChunks());
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
        AddMod(new ModDebugChunk());
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
        AddMod(new ModSendActiveMaterial());
        AddMod(new ModCamera());
        AddMod(new ModNetworkEntity());
        AddMod(new ModGuiInventory());
        AddMod(new ModGuiTouchButtons());
        AddMod(new ModGuiEscapeMenu());
        AddMod(new ModGuiMapLoading());
        AddMod(new ModDraw2dMisc());
        AddMod(new ModGuiPlayerStats());
        AddMod(new ModGuiChat());
        AddMod(new ModScreenshot());
        AddMod(new ModAudio());

        s = new BlockOctreeSearcher();
        s.platform = platform;

        //Prevent loding screen from immediately displaying lag symbol
        LastReceivedMilliseconds = platform.TimeMillisecondsFromStart();

        ENABLE_DRAW_TEST_CHARACTER = platform.IsDebuggerAttached();

        int maxTextureSize_ = platform.GlGetMaxTextureSize();
        if (maxTextureSize_ < 1024)
        {
            maxTextureSize_ = 1024;
        }
        maxTextureSize = maxTextureSize_;
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

    public void AddMod(ClientMod mod)
    {
        clientmods[clientmodsCount++] = mod;
        mod.Start(modmanager);
    }

    // Main game loop
    public void OnRenderFrame(float deltaTime)
    {
        taskScheduler.Update(this, deltaTime);
    }
    TaskScheduler taskScheduler;

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

        mouseSmoothingAccum += deltaTime;
        float constMouseDt = 1f / 300;
        while (mouseSmoothingAccum > constMouseDt)
        {
            mouseSmoothingAccum -= constMouseDt;
            UpdateMouseViewportControl(constMouseDt);
        }

        //Sleep is required in Mono for running the terrain background thread.
        platform.ApplicationDoEvents();

        accumulator += deltaTime;
        if (accumulator > 1)
        {
            accumulator = 1;
        }
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

    internal float one;

    const int MaxBlockTypes = 1024;

    internal GamePlatform platform;
    internal Packet_BlockType[] blocktypes;
    internal Language language;
    internal TerrainChunkTesselatorCi d_TerrainChunkTesselator;

    internal Map map;
    internal const int chunksize = 16;
    internal const int chunksizebits = 4;

    internal Entity player;
    internal float constWallDistance;

    public bool IsRail(Packet_BlockType block)
    {
        return block.Rail > 0;	//Does not include Rail0, but this can't be placed.
    }

    public bool IsEmptyForPhysics(Packet_BlockType block)
    {
        return (block.DrawType == Packet_DrawTypeEnum.Ladder)
            || (block.WalkableType != Packet_WalkableTypeEnum.Solid && block.WalkableType != Packet_WalkableTypeEnum.Fluid);
    }
    
    public int blockheight(int x, int y, int z_)
    {
        for (int z = z_; z >= 0; z--)
        {
            if (map.GetBlock(x, y, z) != 0)
            {
                return z + 1;
            }
        }
        return 0;
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

    public void Draw2dTexturePart(int textureid, float srcwidth, float srcheight, float dstx, float dsty, float dstwidth, float dstheight, int color, bool enabledepthtest)
    {
        RectFRef rect = RectFRef.Create(0, 0, srcwidth, srcheight);
        platform.GlDisableCullFace();
        platform.GlEnableTexture2d();
        platform.BindTexture2d(textureid);

        if (!enabledepthtest)
        {
            platform.GlDisableDepthTest();
        }
        ModelData data = QuadModelData.GetQuadModelData2(rect.x, rect.y, rect.w, rect.h,
            dstx, dsty, dstwidth, dstheight, Game.IntToByte(Game.ColorR(color)), Game.IntToByte(Game.ColorG(color)), Game.IntToByte(Game.ColorB(color)), Game.IntToByte(Game.ColorA(color)));
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
        if (!map.IsValidPos(x, y, z))
        {
            return 1;
        }
        if (blocktypes[map.GetBlock(x, y, z)].Rail != 0)
        {
            return RailHeight;
        }
        if (blocktypes[map.GetBlock(x, y, z)].DrawType == Packet_DrawTypeEnum.HalfHeight)
        {
            return one / 2;
        }
        if (blocktypes[map.GetBlock(x, y, z)].DrawType == Packet_DrawTypeEnum.Flat)
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
    internal bool AllowFreemove;
    internal bool enableCameraControl;

    internal bool stopPlayerMove;

    internal void Respawn()
    {
        SendPacketClient(ClientPackets.SpecialKeyRespawn());
        stopPlayerMove = true;
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

    internal bool DeleteTexture(string name)
    {
        if (name != null && textures.Contains(name))
        {
            int id = textures.Get(name);
            textures.Remove(name);
            platform.GLDeleteTexture(id);
            return true;
        }
        return false;
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
        if (!map.IsValidPos(platform.FloatToInt(player.position.x),
            platform.FloatToInt(player.position.z),
            platform.FloatToInt(player.position.y) - 1))
        {
            return -1;
        }
        int blockunderplayer = map.GetBlock(platform.FloatToInt(player.position.x),
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

    internal GameData d_Data;

    public const int minlight = 0;
    public const int maxlight = 15;

    public int GetLight(int x, int y, int z)
    {
        int light = map.MaybeGetLight(x, y, z);

        if (light == -1)
        {
            if ((x >= 0 && x < map.MapSizeX)
                && (y >= 0 && y < map.MapSizeY)
                && (z >= d_Heightmap.GetBlock(x, y)))
            {
                return sunlight_;
            }
            else
            {
                return minlight;
            }
        }
        else
        {
            return light;
        }
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
                RedrawAllBlocks();
                return;
            }
        }
        d_Config3d.viewdistance = drawDistances[0];
        RedrawAllBlocks();
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
        if (z >= map.MapSizeZ)
        {
            return true;
        }
        // Allow movement outside map when freemove is enabled
        if (x < 0 || y < 0 || z < 0)
        {
            return controls.GetFreemove() != FreemoveLevelEnum.None;
        }
        if (x >= map.MapSizeX || y >= map.MapSizeY)
        {
            return controls.GetFreemove() != FreemoveLevelEnum.None;
        }
        int block = map.GetBlockValid(x, y, z);
        return block == SpecialBlockId.Empty
            || block == d_Data.BlockIdFillArea()
            || IsWater(block);
    }

    internal bool IsTileEmptyForPhysicsClose(int x, int y, int z)
    {
        return IsTileEmptyForPhysics(x, y, z)
            || (map.IsValidPos(x, y, z) && blocktypes[map.GetBlock(x, y, z)].DrawType == Packet_DrawTypeEnum.HalfHeight)
            || (map.IsValidPos(x, y, z) && IsEmptyForPhysics(blocktypes[map.GetBlock(x, y, z)]));
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
            PlayerStats.CurrentHealth = 0;
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
    
    internal void UpdateColumnHeight(int x, int y)
    {
        //todo faster
        int height = map.MapSizeZ - 1;
        for (int i = map.MapSizeZ - 1; i >= 0; i--)
        {
            height = i;
            if (!Game.IsTransparentForLight(blocktypes[map.GetBlock(x, y, i)]))
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
                map.SetChunkDirty(x / chunksize, y / chunksize, i / chunksize, true, true);
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
                    if (map.IsValidChunkPos(cx, cy, cz))
                    {
                        map.SetChunkDirty(cx, cy, cz, true, false);
                    }
                }
            }
        }
    }

    internal void SetBlock(int x, int y, int z, int tileType)
    {
        map.SetBlockRaw(x, y, z, tileType);
        map.SetChunkDirty(x / chunksize, y / chunksize, z / chunksize, true, true);
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
        int blocktype = map.GetBlock(x, y, z);
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
    internal ClientModManager1 modmanager;
    internal ClientMod[] clientmods;
    internal int clientmodsCount;
    internal bool SkySphereNight;
    internal ModDrawParticleEffectBlockBreak particleEffectBlockBreak;
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
        map.SetBlockDirty(x, y, z);
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
            Entity e = entities[i];
            if (e == null)
            {
                continue;
            }
            if (e.drawModel == null)
            {
                continue;
            }
            if (e.networkPosition == null || (e.networkPosition != null && e.networkPosition.PositionLoaded))
            {
                if (IsPlayerInPos(e.position.x, e.position.y, e.position.z,
                    blockposX, blockposY, blockposZ, e.drawModel.ModelHeight))
                {
                    return true;
                }
            }
        }
        return IsPlayerInPos(player.position.x, player.position.y, player.position.z,
            blockposX, blockposY, blockposZ, player.drawModel.ModelHeight);
    }

    bool IsPlayerInPos(float playerposX, float playerposY, float playerposZ,
                       int blockposX, int blockposY, int blockposZ, float playerHeight)
    {
        for (int i = 0; i < FloorFloat(playerHeight) + 1; i++)
        {
            if (ScriptCharacterPhysics.BoxPointDistance(blockposX, blockposZ, blockposY,
                blockposX + 1, blockposZ + 1, blockposY + 1,
                playerposX, playerposY + i + constWallDistance, playerposZ) < constWallDistance)
            {
                return true;
            }
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
    float mouseSmoothingVelX;
    float mouseSmoothingVelY;
    bool mouseSmoothing;
    float mouseSmoothingAccum;

    internal void UpdateMouseViewportControl(float dt)
    {
        if (mouseSmoothing)
        {
            float constMouseSmoothing1 = 0.85f;
            float constMouseSmoothing2 = 0.8f;
            mouseSmoothingVelX = mouseSmoothingVelX + mouseDeltaX / (300 / 75) * constMouseSmoothing2;
            mouseSmoothingVelY = mouseSmoothingVelY + mouseDeltaY / (300 / 75) * constMouseSmoothing2;
            mouseSmoothingVelX = mouseSmoothingVelX * constMouseSmoothing1;
            mouseSmoothingVelY = mouseSmoothingVelY * constMouseSmoothing1;
        }
        else
        {
            mouseSmoothingVelX = mouseDeltaX;
            mouseSmoothingVelY = mouseDeltaY;
        }

        if (guistate == GuiState.Normal && enableCameraControl && platform.Focused())
        {
            if (!overheadcamera)
            {
                if (platform.IsMousePointerLocked())
                {
                    player.position.roty += mouseSmoothingVelX * rotationspeed * 1f / 75;
                    player.position.rotx += mouseSmoothingVelY * rotationspeed * 1f / 75;
                    player.position.rotx = MathCi.ClampFloat(player.position.rotx,
                        Game.GetPi() / 2 + (one * 15 / 1000),
                        (Game.GetPi() / 2 + Game.GetPi() - (one * 15 / 1000)));
                }

                player.position.rotx += touchOrientationDy * constRotationSpeed * (one / 75);
                player.position.roty += touchOrientationDx * constRotationSpeed * (one / 75);
                touchOrientationDx = 0;
                touchOrientationDy = 0;
            }
            if (cameratype == CameraType.Overhead)
            {
                if (mouseMiddle || mouseRight)
                {
                    overheadcameraK.TurnLeft(mouseDeltaX / 70);
                    overheadcameraK.TurnUp(mouseDeltaY / 3);
                }
            }
        }

        mouseDeltaX = 0;
        mouseDeltaY = 0;
    }

    internal string Follow;
    internal IntRef FollowId()
    {
        if (Follow == null)
        {
            return null;
        }
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
    internal bool[] keyboardStateRaw;

    public const int KeyAltLeft = 5;
    public const int KeyAltRight = 6;

    internal bool SwimmingEyes()
    {
        int eyesBlock = GetPlayerEyesBlock();
        if (eyesBlock == -1) { return true; }
        return d_Data.WalkableType1()[eyesBlock] == Packet_WalkableTypeEnum.Fluid;
    }

    internal bool SwimmingBody()
    {
        int block = map.GetBlock(platform.FloatToInt(player.position.x), platform.FloatToInt(player.position.z), platform.FloatToInt(player.position.y + 1));
        if (block == -1) { return true; }
        return d_Data.WalkableType1()[block] == Packet_WalkableTypeEnum.Fluid;
    }

    internal bool WaterSwimmingEyes()
    {
        if (GetPlayerEyesBlock() == -1) { return true; }
        return IsWater(GetPlayerEyesBlock());
    }

    internal bool WaterSwimmingCamera()
    {
        if (GetCameraBlock() == -1) { return true; }
        return IsWater(GetCameraBlock());
    }

    internal bool LavaSwimmingCamera()
    {
        return IsLava(GetCameraBlock());
    }

    int GetCameraBlock()
    {
        int bx = MathFloor(CameraEyeX);
        int by = MathFloor(CameraEyeZ);
        int bz = MathFloor(CameraEyeY);

        if (!map.IsValidPos(bx, by, bz))
        {
            return 0;
        }
        return map.GetBlockValid(bx, by, bz);
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

        if (!map.IsValidPos(bx, by, bz))
        {
            if (pY < WaterLevel())
            {
                return -1;
            }
            return 0;
        }
        return map.GetBlockValid(bx, by, bz);
    }

    public float WaterLevel() { return map.MapSizeZ / 2; }

    internal bool IsLava(int blockType)
    {
        string name = blocktypes[blockType].Name;
        if (name == null)
        {
            return false;
        }
        return platform.StringContains(name, "Lava"); // todo
    }

    internal int terraincolor()
    {
        if (WaterSwimmingCamera())
        {
            return Game.ColorFromArgb(255, 78, 95, 140);
        }
        else if (LavaSwimmingCamera())
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

    internal bool shadowssimple;
    internal bool shouldRedrawAllBlocks;
    internal void RedrawAllBlocks()
    {
        shouldRedrawAllBlocks = true;
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

        if (SkySphereNight && (!shadowssimple))
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

    internal void ChatLog(string p)
    {
        if (!platform.ChatLog(this.ServerInfo.ServerName, p))
        {
            platform.ConsoleWriteLine(platform.StringFormat(language.CannotWriteChatLog(), this.ServerInfo.ServerName));
        }
    }

    internal int fillAreaLimit;

    internal void KeyUp(int eKey)
    {
        keyboardStateRaw[eKey] = false;
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
            s_.blocktype = map.GetBlock(x, y, z);
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

    internal void RevertSpeculative(float dt)
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
            //Client command starting with a "."
            string strFreemoveNotAllowed = language.FreemoveNotAllowed();
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

            // Command requiring no arguments
            if (cmd == "clients")
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
            else if (cmd == "reconnect")
            {
                Reconnect();
            }
            else if (cmd == "m")
            {
                mouseSmoothing = !mouseSmoothing;
                if (mouseSmoothing) { Log("Mouse smoothing enabled."); }
                else { Log("Mouse smoothing disabled."); }
            }
            // Commands requiring boolean arguments
            else if (cmd == "pos")
            {
                ENABLE_DRAWPOSITION = BoolCommandArgument(arguments);
            }
            else if (cmd == "noclip")
            {
                if (this.AllowFreemove)
                {
                    stopPlayerMove = true;
                    if (BoolCommandArgument(arguments))
                    {
                        controls.SetFreemove(FreemoveLevelEnum.Noclip);
                    }
                    else
                    {
                        controls.SetFreemove(FreemoveLevelEnum.None);
                    }
                }
                else
                {
                    Log(strFreemoveNotAllowed);
                    return;
                }
            }
            else if (cmd == "freemove")
            {
                if (this.AllowFreemove)
                {
                    stopPlayerMove = true;
                    if (BoolCommandArgument(arguments))
                    {
                        controls.SetFreemove(FreemoveLevelEnum.Freemove);
                    }
                    else
                    {
                        controls.SetFreemove(FreemoveLevelEnum.None);
                    }
                }
                else
                {
                    Log(strFreemoveNotAllowed);
                    return;
                }
            }
            else if (cmd == "gui")
            {
                ENABLE_DRAW2D = BoolCommandArgument(arguments);
            }
            // Commands requiring numeric arguments
            else if (arguments != "")
            {
                if (cmd == "fog")
                {
                    int foglevel;
                    foglevel = platform.IntParse(arguments);
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
                    }
                    OnResize();
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
                        Log(platform.StringFormat2("Valid field of view: {0}-{1}", platform.IntToString(minfov), platform.IntToString(maxfov)));
                    }
                    else
                    {
                        float fov_ = (2 * Game.GetPi() * (one * arg / 360));
                        this.fov = fov_;
                        OnResize();
                    }
                }
                else if (cmd == "movespeed")
                {
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
            }
            else
            {
                //Send client command to server if none matches
                string chatline = StringTools.StringSubstring(platform, GuiTypingBuffer, 0, MathCi.MinInt(GuiTypingBuffer.Length, 256));
                SendChat(chatline);
            }
            //Process clientside mod commands anyway
            for (int i = 0; i < clientmodsCount; i++)
            {
                ClientCommandArgs args = new ClientCommandArgs();
                args.arguments = arguments;
                args.command = cmd;
                clientmods[i].OnClientCommand(this, args);
            }
        }
        else
        {
            //Regular chat message or server command. Send to server
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
        if (packet.Identification.MapSizeX != map.MapSizeX
            || packet.Identification.MapSizeY != map.MapSizeY
            || packet.Identification.MapSizeZ != map.MapSizeZ)
        {
            map.Reset(packet.Identification.MapSizeX,
                packet.Identification.MapSizeY,
                packet.Identification.MapSizeZ);
            d_Heightmap.Restart();
        }
        shadowssimple = packet.Identification.DisableShadows == 1 ? true : false;
        //maxdrawdistance = packet.Identification.PlayerAreaSize / 2;
        //if (maxdrawdistance == 0)
        //{
        //    maxdrawdistance = 128;
        //}
        maxdrawdistance = 256;
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

    public void SetFile(string name, string md5, byte[] downloaded, int downloadedLength)
    {
        string nameLowercase = platform.StringToLower(name);

        // Update mouse cursor if changed
        if (nameLowercase == "mousecursor.png")
        {
            platform.SetWindowCursor(0, 0, 32, 32, downloaded, downloadedLength);
        }

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

    internal bool ammostarted;
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

    internal AnimationState localplayeranim;
    internal AnimationHint localplayeranimationhint;

    internal bool enable_move;

    public const int DISCONNECTED_ICON_AFTER_SECONDS = 10;
    internal void KeyDown(int eKey)
    {
        keyboardStateRaw[eKey] = true;
        if (guistate != GuiState.MapLoading)
        {
            // only handle keys once game has been loaded
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
                stopPlayerMove = true;
                if (controls.GetFreemove() == FreemoveLevelEnum.None)
                {
                    controls.SetFreemove(FreemoveLevelEnum.Freemove);
                    Log(language.MoveFree());
                }
                else if (controls.GetFreemove() == FreemoveLevelEnum.Freemove)
                {
                    controls.SetFreemove(FreemoveLevelEnum.Noclip);
                    Log(language.MoveFreeNoclip());
                }
                else if (controls.GetFreemove() == FreemoveLevelEnum.Noclip)
                {
                    controls.SetFreemove(FreemoveLevelEnum.None);
                    Log(language.MoveNormal());
                }
            }
            if (eKey == GetKey(GlKeys.I))
            {
                drawblockinfo = !drawblockinfo;
            }
            int playerx = platform.FloatToInt(player.position.x);
            int playery = platform.FloatToInt(player.position.z);
            if ((playerx >= 0 && playerx < map.MapSizeX)
                && (playery >= 0 && playery < map.MapSizeY))
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
                    int blocktype = map.GetBlock(currentAttackedBlock.X, currentAttackedBlock.Y, currentAttackedBlock.Z);
                    if (IsUsableBlock(blocktype))
                    {
                        if (d_Data.IsRailTile(blocktype))
                        {
                            player.position.x = posX + (one / 2);
                            player.position.y = posZ + 1;
                            player.position.z = posY + (one / 2);
                            // disable freemove when mounting rails
                            stopPlayerMove = true;
                            controls.SetFreemove(FreemoveLevelEnum.None);
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

    internal void Draw2d(float dt)
    {
        if (!ENABLE_DRAW2D)
        {
            return;
        }

        OrthoMode(Width(), Height());

        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnNewFrameDraw2d(this, dt);
        }

        PerspectiveMode();
    }

    public const int ChatFontSize = 11;

    internal BoolRef soundnow;

    internal Controls controls;
    internal float pushX;
    internal float pushY;
    internal float pushZ;

    internal void FrameTick(float dt)
    {
        NewFrameEventArgs args_ = new NewFrameEventArgs();
        args_.SetDt(dt);
        for (int i = 0; i < clientmodsCount; i++)
        {
            clientmods[i].OnNewFrameFixed(this, args_);
        }
        for (int i = 0; i < entitiesCount; i++)
        {
            Entity e = entities[i];
            if (e == null) { continue; }
            for (int k = 0; k < e.scriptsCount; k++)
            {
                e.scripts[k].OnNewFrameFixed(this, i, dt);
            }
        }
        RevertSpeculative(dt);

        if (guistate == GuiState.MapLoading) { return; }

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
        if (maxX > map.MapSizeX) { maxX = map.MapSizeX; }
        if (maxY > map.MapSizeZ) { maxY = map.MapSizeZ; }
        if (maxZ > map.MapSizeY) { maxZ = map.MapSizeY; }
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
        if (!e.GetEmulated() || e.GetForceUsage())
        {
            // Set x and y only for real MouseMove events
            mouseCurrentX = e.GetX();
            mouseCurrentY = e.GetY();
        }
        if (e.GetEmulated() || e.GetForceUsage())
        {
            // Get delta only from emulated events (actual events negate previous ones)
            mouseDeltaX += e.GetMovementX();
            mouseDeltaY += e.GetMovementY();
        }
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].OnMouseMove(this, e);
        }
    }

    internal ListAction commitActions;
    public void QueueActionCommit(Action_ action)
    {
        commitActions.Add(action);
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
        for (int i = 0; i < clientmodsCount; i++)
        {
            if (clientmods[i] == null) { continue; }
            clientmods[i].Dispose(this);
        }
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

    internal float CameraEyeX;
    internal float CameraEyeY;
    internal float CameraEyeZ;

    internal bool isplayeronground;

    internal bool reachedwall;
    internal bool reachedwall_1blockhigh;
    internal bool reachedHalfBlock;
    internal float movedz;
}
