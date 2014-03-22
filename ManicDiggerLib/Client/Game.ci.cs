public class Game
{
    public Game()
    {
        one = 1;
        chunksize = 16;
        player = new CharacterPhysicsState();

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
        mvMatrix = new StackFloatArray();
        pMatrix = new StackFloatArray();
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
        compassid = -1;
        needleid = -1;
        compassangle = 0;
        compassvertex = 1;
        dialogs = new VisibleDialog[512];
        dialogsCount = 512;
        blockHealth = new DictionaryVector3Float();
        playertexturedefault = -1;
        a = new AnimationState();
        skyspheretexture = -1;
        skyspherenighttexture = -1;
        skysphere = new SkySphere();
        rotation_speed = one * 180 / 20;
        modmanager = new ClientModManager1();
        particleEffectBlockBreak = new ParticleEffectBlockBreak();
        PICK_DISTANCE = one * 35 / 10;
        selectedmodelid = -1;
        grenadetime = 3;
        rotationspeed = one * 15 / 100;
        bouncespeedmultiply = one * 5 / 10;
        walldistance = one * 3 / 10;
        entities = new Entity[4096];
        entitiesCount = 4096;
        PlayerPushDistance = 2;
        projectilegravity = 20;
        keyboardState = new bool[256];
        overheadcameradistance = 10;
        tppcameradistance = 3;
        TPP_CAMERA_DISTANCE_MIN = 1;
        TPP_CAMERA_DISTANCE_MAX = 10;
        options = new OptionsCi();
        BlockDamageToPlayerTimer = TimerCi.Create(BlockDamageToPlayerEvery, BlockDamageToPlayerEvery * 2);
        overheadcameraK = new Kamera();
        fillarea = new DictionaryVector3Float();
        fillAreaLimit = 200;
        walksoundtimer = 0;
        lastwalksound = 0;
        stepsoundduration = one * 4 / 10;
        speculativeCount = 1024 * 8;
        speculative = new Speculative[speculativeCount];
        typinglog = new string[1024 * 16];
        typinglogCount = 0;
        CurrentChunk = new byte[1024 * 64];
        CurrentChunkCount = 0;
        NewBlockTypes = new Packet_BlockType[GlobalVar.MAX_BLOCKTYPES];
        files = new DictionaryStringByteArray();
    }
    float one;

    const int MaxBlockTypes = 1024;

    internal GamePlatform platform;
    internal Packet_BlockType[] blocktypes;
    internal Language language;
    internal TerrainChunkTesselatorCi d_TerrainChunkTesselator;

    internal Chunk[] chunks;
    internal int MapSizeX;
    internal int MapSizeY;
    internal int MapSizeZ;
    internal int chunksize;

    internal CharacterPhysicsState player;

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

        int cx = x / chunksize;
        int cy = y / chunksize;
        int cz = z / chunksize;
        int chunkpos = MapUtilCi.Index3d(cx, cy, cz, MapSizeX / chunksize, MapSizeY / chunksize);
        if (chunks[chunkpos] == null)
        {
            return 0;
        }
        else
        {
            return GetBlockInChunk(chunks[chunkpos], MapUtilCi.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize));
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
        int pos = MapUtilCi.Index3d(x % chunksize, y % chunksize, z % chunksize, chunksize, chunksize);
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
        Chunk chunk = chunks[MapUtilCi.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)];
        if (chunk == null)
        {
            Chunk c = new Chunk();
            c.data = new byte[chunksize * chunksize * chunksize];
            chunks[MapUtilCi.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)] = c;
            return chunks[MapUtilCi.Index3d(x, y, z, mapsizexchunks, mapsizeychunks)];
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

    public int blockheight(int x, int y)
    {
        for (int z = MapSizeZ - 1; z >= 0; z--)
        {
            if (GetBlock(x, y, z) != 0)
            {
                return z + 1;
            }
        }
        return MapSizeZ / 2;
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

    public static byte ColorA(int color)
    {
        byte a = IntToByte(color >> 24);
        return a;
    }

    public static byte ColorR(int color)
    {
        byte r = IntToByte(color >> 16);
        return r;
    }

    public static byte ColorG(int color)
    {
        byte g = IntToByte(color >> 8);
        return g;
    }

    public static byte ColorB(int color)
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

    internal static int MaxInt(int a, int b)
    {
        if (a >= b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    internal static int MinInt(int a, int b)
    {
        if (a <= b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

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
        int chunksizebits = 4;
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
            x1, y1, width, height, Game.ColorR(color), Game.ColorG(color), Game.ColorB(color), Game.ColorA(color));
        platform.DrawModelData(data);
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

    public void Draw2dTextures(Draw2dData[] todraw, int todrawLength, int textureid, float angle)
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
                x1, y1, width, height, ColorR(color), ColorG(color), ColorB(color), ColorA(color));
            modelDatas[modelDatasCount++] = modelData;
        }

        ModelData combined = CombineModelData(modelDatas, modelDatasCount);

        platform.GlDisableCullFace();
        platform.GlEnableTexture2d();
        platform.BindTexture2d(textureid);

        platform.GlDisableDepthTest();

        platform.DrawModelData(combined);

        platform.GlEnableDepthTest();

        platform.GlDisableCullFace();
        platform.GlEnableTexture2d();
    }

    internal bool currentMatrixModeProjection;
    internal StackFloatArray mvMatrix;
    internal StackFloatArray pMatrix;

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
        platform.SetMatrixUniforms(pMatrix.Peek(), mvMatrix.Peek());
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
            if (pMatrix.Count() > 0)
            {
                mvMatrix.Pop();
            }
            mvMatrix.Push(m);
        }

        SetMatrixUniforms();
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

        SetMatrixUniforms();
    }

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
        Mat4.Scale(m, m, Vec3.FromValues(x, y, z));

        SetMatrixUniforms();
    }

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
        Mat4.Rotate(m, m, angle, Vec3.FromValues(x, y, z));
        SetMatrixUniforms();
    }

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
        Mat4.Translate(m, m, Vec3.FromValues(x, y, z));
        SetMatrixUniforms();
    }

    public void GLPushMatrix()
    {
        if (currentMatrixModeProjection)
        {
            pMatrix.Push(Mat4.CloneIt(pMatrix.Peek()));
        }
        else
        {
            mvMatrix.Push(Mat4.CloneIt(mvMatrix.Peek()));
        }
        SetMatrixUniforms();
    }

    public void GLLoadIdentity()
    {
        if (currentMatrixModeProjection)
        {
            if (pMatrix.Count() > 0)
            {
                pMatrix.Pop();
            }
            pMatrix.Push(Mat4.Identity_(Mat4.Create()));
        }
        else
        {
            if (mvMatrix.Count() > 0)
            {
                mvMatrix.Pop();
            }
            mvMatrix.Push(Mat4.Identity_(Mat4.Create()));
        }
        SetMatrixUniforms();
    }

    public void GLOrtho(float left, float right, float bottom, float top, float zNear, float zFar)
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
        Mat4.Ortho(m, left, right, bottom, top, zNear, zFar);
        SetMatrixUniforms();
    }

    public void OrthoMode(int width, int height)
    {
        //GL.Disable(EnableCap.DepthTest);
        GLMatrixModeProjection();
        GLPushMatrix();
        GLLoadIdentity();
        GLOrtho(0, width, height, 0, 0, 1);
        GLMatrixModeModelView();
        GLPushMatrix();
        GLLoadIdentity();
    }

    public void PerspectiveMode()
    {
        // Enter into our projection matrix mode
        GLMatrixModeProjection();
        // Pop off the last matrix pushed on when in projection mode (Get rid of ortho mode)
        GLPopMatrix();
        // Go back to our model view matrix like normal
        GLMatrixModeModelView();
        GLPopMatrix();
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

    public static float MinFloat(float a, float b)
    {
        if (a <= b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public static float AbsFloat(float b)
    {
        if (b >= 0)
        {
            return b;
        }
        else
        {
            return 0 - b;
        }
    }

    public static float MaxFloat(float a, float b)
    {
        if (a >= b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    Model wireframeCube;
    public void DrawLinesAroundSelectedBlock(float x, float y, float z)
    {
        if (x == -1 && y == -1 && z == -1)
        {
            return;
        }

        float pickcubeheight = getblockheight(platform.FloatToInt(x),platform.FloatToInt(z),platform.FloatToInt(y));

        float posx = x + one/2;
        float posy = y + pickcubeheight * one/2;
        float posz = z + one / 2;

        platform.GLLineWidth(2);
        float size = one * 51 / 100;
        platform.BindTexture2d(0);

        if (wireframeCube == null)
        {
            ModelData data = WireframeCube.Get();
            wireframeCube = platform.CreateModel(data);
        }
        GLPushMatrix();
        GLTranslate(posx, posy, posz);
        GLScale(size, pickcubeheight * size, size);
        platform.DrawModel(wireframeCube);
        GLPopMatrix();
    }

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
        BitmapCi bmp = platform.CreateTextTexture2(t);
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
        INetOutgoingMessage msg = main.CreateMessage();
        msg.Write(packet, packetLength);
        main.SendMessage(msg, MyNetDeliveryMethod.ReliableOrdered);
        //}
        //catch
        //{
        //    game.p.ConsoleWriteLine("SendPacket error");
        //}
    }

    internal INetClient main;

    IntRef packetLen;
    public void SendPacketClient(Packet_Client packetClient)
    {
        byte[] packet = Serialize(packetClient, packetLen);
        SendPacket(packet, packetLen.value);
    }

    internal void SendChat(string s)
    {
        Packet_ClientMessage p = new Packet_ClientMessage();
        p.Message = s;
        p.IsTeamchat = d_HudChat.IsTeamchat ? 1 : 0;
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.Message;
        pp.Message = p;
        SendPacketClient(pp);
    }

    internal HudChat d_HudChat;

    internal void SendPingReply()
    {
        Packet_ClientPingReply p = new Packet_ClientPingReply();
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.PingReply;
        pp.PingReply = p;
        SendPacketClient(pp);
    }

    internal void SendSetBlock(int x, int y, int z, int mode, int type, int materialslot)
    {
        Packet_ClientSetBlock p = new Packet_ClientSetBlock();
        {
            p.X = x;
            p.Y = y;
            p.Z = z;
            p.Mode = mode;
            p.BlockType = type;
            p.MaterialSlot = materialslot;
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.SetBlock;
        pp.SetBlock = p;
        SendPacketClient(pp);
    }
    internal int ActiveMaterial;

    internal void SendFillArea(int startx, int starty, int startz, int endx, int endy, int endz, int blockType)
    {
        Packet_ClientFillArea p = new Packet_ClientFillArea();
        {
            p.X1 = startx;
            p.Y1 = starty;
            p.Z1 = startz;
            p.X2 = endx;
            p.Y2 = endy;
            p.Z2 = endz;
            p.BlockType = blockType;
            p.MaterialSlot = ActiveMaterial;
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.FillArea;
        pp.FillArea = p;
        SendPacketClient(pp);
    }

    internal void InventoryClick(Packet_InventoryPosition pos)
    {
        Packet_ClientInventoryAction p = new Packet_ClientInventoryAction();
        p.A = pos;
        p.Action = Packet_InventoryActionTypeEnum.Click;
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.InventoryAction;
        pp.InventoryAction = p;
        SendPacketClient(pp);
    }

    internal void WearItem(Packet_InventoryPosition from, Packet_InventoryPosition to)
    {
        Packet_ClientInventoryAction p = new Packet_ClientInventoryAction();
        p.A = from;
        p.B = to;
        p.Action = Packet_InventoryActionTypeEnum.WearItem;
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.InventoryAction;
        pp.InventoryAction = p;
        SendPacketClient(pp);
    }

    internal void MoveToInventory(Packet_InventoryPosition from)
    {
        Packet_ClientInventoryAction p = new Packet_ClientInventoryAction();
        p.A = from;
        p.Action = Packet_InventoryActionTypeEnum.MoveToInventory;
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.InventoryAction;
        pp.InventoryAction = p;
        SendPacketClient(pp);
    }

    internal DictionaryStringString performanceinfo;

    internal void AddChatline(string message)
    {
        d_HudChat.AddChatline(message);
    }

    internal bool ENABLE_DRAW2D;
    internal bool ENABLE_FREEMOVE;
    internal bool ENABLE_NOCLIP;
    internal bool AllowFreemove;
    internal bool enableCameraControl;

    internal void Respawn()
    {
        Packet_Client p = new Packet_Client();
        {
            p.Id = Packet_ClientIdEnum.SpecialKey;
            p.SpecialKey_ = new Packet_ClientSpecialKey();
            p.SpecialKey_.Key_ = Packet_SpecialKeyEnum.Respawn;
        }
        SendPacketClient(p);
        player.movedz = 0;
    }

    public static bool IsTransparentForLight(Packet_BlockType b)
    {
        return b.DrawType != Packet_DrawTypeEnum.Solid && b.DrawType != Packet_DrawTypeEnum.ClosedDoor;
    }

    internal GuiState guistate;
    internal bool freemouse;
    internal bool overheadcamera;
    public bool GetFreeMouse()
    {
        if (overheadcamera)
        {
            return true;
        }
        return freemouse;
    }
    public void SetFreeMouse(bool value)
    {
        platform.SetFreeMouse(value);
        freemouse = value;
    }
    internal MapLoadingProgressEventArgs maploadingprogress;

    public void MapLoadingStart()
    {
        guistate = GuiState.MapLoading;
        SetFreeMouse(true);
        maploadingprogress = new MapLoadingProgressEventArgs();
        fontMapLoading = FontCi.Create("Arial", 14, 0);
    }

    FontCi fontMapLoading;

    internal string invalidVersionDrawMessage;
    internal Packet_Server invalidVersionPacketIdentification;

    public void MapLoadingDraw()
    {
        int Width = platform.GetCanvasWidth();
        int Height = platform.GetCanvasHeight();

        Draw2dTexture(GetTexture(platform.PathCombine("gui", "background.png")), 0, 0, 1024 * (one * Width / 800), 1024 * (one * Height / 600), null, 0, Game.ColorFromArgb(255, 255, 255, 255), false);
        string connecting = language.Connecting();
        if (maploadingprogress.ProgressStatus != null)
        {
            connecting = maploadingprogress.ProgressStatus;
        }

        if (invalidVersionDrawMessage != null)
        {
            Draw2dText(invalidVersionDrawMessage, fontMapLoading, xcenter(TextSizeWidth(invalidVersionDrawMessage, 14)), Height / 2 - 50, null, false);
            string connect = "Click to connect";
            Draw2dText(connect, fontMapLoading, xcenter(TextSizeWidth(connect, 14)), Height / 2 + 50, null, false);
            return;
        }

        IntRef serverNameWidth = new IntRef();
        IntRef serverNameHeight = new IntRef();
        platform.TextSize(this.ServerInfo.ServerName, 14, serverNameWidth, serverNameHeight);
        Draw2dText(this.ServerInfo.ServerName, fontMapLoading, xcenter(serverNameWidth.value), Height / 2 - 150, null, false);

        IntRef serverMotdWidth = new IntRef();
        IntRef serverMotdHeight = new IntRef();
        platform.TextSize(this.ServerInfo.ServerMotd, 14, serverMotdWidth, serverMotdHeight);
        Draw2dText(this.ServerInfo.ServerMotd, fontMapLoading, xcenter(serverMotdWidth.value), Height / 2 - 100, null, false);

        IntRef connectingWidth = new IntRef();
        IntRef connectingHeight = new IntRef();
        platform.TextSize(connecting, 14, connectingWidth, connectingHeight);
        Draw2dText(connecting, fontMapLoading, xcenter(connectingWidth.value), Height / 2 - 50, null, false);

        string progress = platform.StringFormat(language.ConnectingProgressPercent(), platform.IntToString(maploadingprogress.ProgressPercent));
        string progress1 = platform.StringFormat(language.ConnectingProgressKilobytes(), platform.IntToString(maploadingprogress.ProgressBytes / 1024));

        if (maploadingprogress.ProgressPercent > 0)
        {
            IntRef progressWidth = new IntRef();
            IntRef progressHeight = new IntRef();
            platform.TextSize(progress, 14, progressWidth, progressHeight);
            Draw2dText(progress, fontMapLoading, xcenter(progressWidth.value), Height / 2 - 20, null, false);

            IntRef progress1Width = new IntRef();
            IntRef progress1Height = new IntRef();
            platform.TextSize(progress1, 14, progress1Width, progress1Height);
            Draw2dText(progress1, fontMapLoading, xcenter(progress1Width.value), Height / 2 + 10, null, false);

            float progressratio = one * maploadingprogress.ProgressPercent / 100;
            int sizex = 400;
            int sizey = 40;
            Draw2dTexture(WhiteTexture(), xcenter(sizex), Height / 2 + 70, sizex, sizey, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
            int red = Game.ColorFromArgb(255, 255, 0, 0);
            int yellow = Game.ColorFromArgb(255, 255, 255, 0);
            int green = Game.ColorFromArgb(255, 0, 255, 0);
            int[] colors = new int[3];
            colors[0] = red;
            colors[1] = yellow;
            colors[2] = green;
            int c = InterpolationCi.InterpolateColor(platform, progressratio, colors, 3);
            Draw2dTexture(WhiteTexture(), xcenter(sizex), Height / 2 + 70, progressratio * sizex, sizey, null, 0, c, false);
        }
    }

    DictionaryStringInt1024 textures;
    internal int GetTexture(string p)
    {
        if (!textures.Contains(p))
        {
            BoolRef found = new BoolRef();
            textures.Set(p, platform.LoadTextureFromFile(platform.GetFullFilePath(p, found)));
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
    internal EscapeMenuState escapemenustate;
    internal bool AudioEnabled;
    internal MenuState menustate;
    internal bool mouseleftclick;
    internal bool mouseleftdeclick;
    internal bool wasmouseleft;
    internal bool mouserightclick;
    internal bool mouserightdeclick;
    internal bool wasmouseright;
    internal int ENABLE_LAG;

    internal void DrawScreenshotFlash()
    {
        Draw2dTexture(WhiteTexture(), 0, 0, platform.GetCanvasWidth(), platform.GetCanvasHeight(), null, 0, ColorFromArgb(255,255,255,255), false);
        string screenshottext = "&0Screenshot";
        IntRef textWidth = new IntRef();
        IntRef textHeight = new IntRef();
        platform.TextSize(screenshottext, 50, textWidth, textHeight);
        FontCi font = new FontCi();
        font.family = "Arial";
        font.size = 50;
        Draw2dText(screenshottext, font, xcenter(textWidth.value), ycenter(textHeight.value), null, false);
    }

    internal int Width()
    {
        return platform.GetCanvasWidth();
    }

    internal int Height()
    {
        return platform.GetCanvasHeight();
    }

    internal float znear;

    internal GetCameraMatrix CameraMatrix;

    public void Set3dProjection(float zfar, float fov)
    {
        float aspect_ratio = one * Width() / Height();
        float[] perspective = Mat4.Create();
        Mat4.Perspective(perspective, fov, aspect_ratio, znear, zfar);
        CameraMatrix.lastpmatrix = perspective;
        GLMatrixModeProjection();
        GLLoadMatrix(perspective);
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

    //Size of Health/Oxygen bar
    const int barSizeX = 20;
    const int barSizeY = 120;
    const int barOffset = 30;
    const int barDistanceToMargin = 40;

    public void DrawPlayerHealth()
    {
        if (PlayerStats != null)
        {
            float progress = one * PlayerStats.CurrentHealth / PlayerStats.MaxHealth;
            int posX = barDistanceToMargin;
            int posY = Height() - barDistanceToMargin;
            Draw2dTexture(WhiteTexture(), posX, posY - barSizeY, barSizeX, barSizeY, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
            Draw2dTexture(WhiteTexture(), posX, posY - (progress * barSizeY), barSizeX, (progress) * barSizeY, null, 0, Game.ColorFromArgb(255, 255, 0, 0), false);
        }
        //if (test) { d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), 50, 50, 200, 200, null, Color.Red); }
    }

    public void DrawPlayerOxygen()
    {
        if (PlayerStats != null)
        {
            if (PlayerStats.CurrentOxygen < PlayerStats.MaxOxygen)
            {
                float progress = one * PlayerStats.CurrentOxygen / PlayerStats.MaxOxygen;
                int posX = barDistanceToMargin + barOffset;
                int posY = Height() - barDistanceToMargin;
                Draw2dTexture(WhiteTexture(), posX, posY - barSizeY, barSizeX, barSizeY, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
                Draw2dTexture(WhiteTexture(), posX, posY - (progress * barSizeY), barSizeX, (progress) * barSizeY, null, 0, Game.ColorFromArgb(255, 0, 0, 255), false);
            }
        }
    }

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

    internal bool IsWater(int blockType)
    {
        return platform.StringContains(blocktypes[blockType].Name, "Water"); // todo
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

    internal IntRef BlockUnderPlayer()
    {
        if (!IsValidPos(platform.FloatToInt(player.playerposition.X),
            platform.FloatToInt(player.playerposition.Z),
            platform.FloatToInt(player.playerposition.Y) - 1))
        {
            return null;
        }
        int blockunderplayer = GetBlock(platform.FloatToInt(player.playerposition.X),
            platform.FloatToInt(player.playerposition.Z),
            platform.FloatToInt(player.playerposition.Y) - 1);
        return IntRef.Create(blockunderplayer);
    }

    internal void DrawEnemyHealthUseInfo(string name, float progress, bool useInfo)
    {
        int y = useInfo ? 55 : 35;
        Draw2dTexture(WhiteTexture(), xcenter(300), 40, 300, y, null, 0, Game.ColorFromArgb(255,0,0,0), false);
        Draw2dTexture(WhiteTexture(), xcenter(300), 40, 300 * progress, y, null, 0, Game.ColorFromArgb(255, 255, 0, 0), false);
        FontCi font = new FontCi();
        font.family = "Arial";
        font.size = 14;
        IntRef w = new IntRef();
        IntRef h = new IntRef();
        platform.TextSize(name, 14, w, h);
        Draw2dText(name, font, xcenter(w.value), 40, null, false);
        if (useInfo)
        {
            name = platform.StringFormat(language.PressToUse(), "E");
            platform.TextSize(name, 10, w, h);
            FontCi font2 = new FontCi();
            font2.family = "Arial";
            font2.size = 10;
            Draw2dText(name, font2, xcenter(w.value), 70, null, false);
        }
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
            playerdestination = Vector3Ref.Create(player.playerposition.X, player.playerposition.Y, player.playerposition.Z);
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
        return radius + RadiusWhenMoving * radius * (Game.MinFloat(playervelocity.Length() / movespeed, 1));
    }

    RandomCi rnd;

    internal PointFloatRef GetAim()
    {
        if (rnd == null)
        {
            rnd = platform.RandomCreate();
        }
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

    public void Draw2dBitmapFile(string filename, int x, int y, int w, int h)
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
        return entities[LocalPlayerId].player.EyeHeight;
    }

    internal void SetCharacterEyesHeight(float value)
    {
        entities[LocalPlayerId].player.EyeHeight = value;
    }

    public float EyesPosX() { return player.playerposition.X; }
    public float EyesPosY() { return player.playerposition.Y + GetCharacterEyesHeight(); }
    public float EyesPosZ() { return player.playerposition.Z; }

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
        if (!AudioEnabled)
        {
            return;
        }
        BoolRef found = new BoolRef();
        string fullpath = platform.GetFullFilePath(file, found);
        if (!found.value)
        {
            platform.ConsoleWriteLine(platform.StringFormat("File not found: {0}", file));
            return;
        }
        platform.AudioPlay(fullpath, EyesPosX(), EyesPosY(), EyesPosZ());
    }

    public void AudioPlayLoop(string file, bool play, bool restart)
    {
        if ((!AudioEnabled) && play)
        {
            return;
        }
        BoolRef found = new BoolRef();
        string fullpath = platform.GetFullFilePath(file, found);
        if (!found.value)
        {
            platform.ConsoleWriteLine(platform.StringFormat("File not found: {0}", file));
            return;
        }
        platform.AudioPlayLoop(fullpath, play, restart);
    }

    public int[] MaterialSlots()
    {
        int[] m = new int[10];
        for (int i = 0; i < 10; i++)
        {
            Packet_Item item = d_Inventory.RightHand[i];
            m[i] = d_Data.BlockIdDirt();
            if (item != null && item.ItemClass == Packet_ItemClassEnum.Block)
            {
                m[i] = d_Inventory.RightHand[i].BlockId;
            }
        }
        return m;
    }

    int compassid;
    int needleid;
    float compassangle;
    float compassvertex;

    bool CompassInActiveMaterials()
    {
        for (int i = 0; i < 10; i++)
        {
            if (MaterialSlots()[i] == d_Data.BlockIdCompass())
            {
                return true;
            }
        }
        return false;
    }

    public void DrawCompass()
    {
        if (!CompassInActiveMaterials()) return;
        if (compassid == -1)
        {
            BoolRef found = new BoolRef();
            compassid = platform.LoadTextureFromFile(platform.GetFullFilePath("compass.png", found));
            needleid = platform.LoadTextureFromFile(platform.GetFullFilePath("compassneedle.png", found));
        }
        float size = 175;
        float posX = Width() - 100;
        float posY = 100;
        float playerorientation = -((player.playerorientation.Y / (2 * Game.GetPi())) * 360);

        compassvertex += (playerorientation - compassangle) / 50;
        compassvertex *= (one * 9 / 10);
        compassangle += compassvertex;

        Draw2dData[] todraw = new Draw2dData[1];
        todraw[0] = new Draw2dData();
        todraw[0].x1 = posX - size / 2;
        todraw[0].y1 = posY - size / 2;
        todraw[0].width = size;
        todraw[0].height = size;
        todraw[0].inAtlasId = null;
        todraw[0].color = Game.ColorFromArgb(255, 255, 255, 255);

        Draw2dTexture(compassid, posX - size / 2, posY - size / 2, size, size, null, 0, Game.ColorFromArgb(255, 255, 255, 255), false);
        Draw2dTextures(todraw, 1, needleid, compassangle);
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
        return GetBlock(x, y, z) == SpecialBlockId.Empty
            || GetBlock(x, y, z) == d_Data.BlockIdFillArea()
            || IsWater(GetBlock(x, y, z));
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
            {
                Packet_Client p = new Packet_Client();
                p.Id = Packet_ClientIdEnum.Death;
                p.Death = new Packet_ClientDeath();
                {
                    p.Death.Reason = damageSource;
                    p.Death.SourcePlayer = sourceId;
                }
                SendPacketClient(p);
            }

            //Respawn(); //Death is not respawn ;)
        }
        else
        {
            AudioPlay(rnd.Next() % 2 == 0 ? "grunt1.wav" : "grunt2.wav");
        }
        {
            Packet_Client p = new Packet_Client();
            {
                p.Id = Packet_ClientIdEnum.Health;
                p.Health = new Packet_ClientHealth();
                p.Health.CurrentHealth = PlayerStats.CurrentHealth;
            }
            SendPacketClient(p);
        }
    }

    int GetPlayerEyesBlockX()
    {
        return platform.FloatToInt(MathFloor(player.playerposition.X));
    }
    int GetPlayerEyesBlockY()
    {
        return platform.FloatToInt(MathFloor(player.playerposition.Z));
    }
    int GetPlayerEyesBlockZ()
    {
        return platform.FloatToInt(MathFloor(player.playerposition.Y + entities[LocalPlayerId].player.EyeHeight));
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

    int lastfalldamagetimeMilliseconds;
    internal void UpdateFallDamageToPlayer()
    {
        //fallspeed 4 is 10 blocks high
        //fallspeed 5.5 is 20 blocks high
        float fallspeed = player.movedz / (-basemovespeed);

        //test = false;
        //if (fallspeed > 5.5f)
        //{
        //    test = true;
        //}

        int posX = GetPlayerEyesBlockX();
        int posY = GetPlayerEyesBlockY();
        int posZ = GetPlayerEyesBlockZ();
        if ((blockheight(posX, posY) < posZ - 8)
            || fallspeed > 3)
        {
            AudioPlayLoop("fallloop.wav", fallspeed > 2, true);
        }
        else
        {
            AudioPlayLoop("fallloop.wav", false, true);
        }

        //fall damage

        if (IsValidPos(posX, posY, posZ - 3))
        {
            int blockBelow = GetBlock(posX, posY, posZ - 3);
            if ((blockBelow != 0) && (!IsWater(blockBelow)))
            {
                float severity = 0;
                if (fallspeed < 4) { return; }
                else if (fallspeed < (one * 45 / 10)) { severity = (one * 3 / 10); }
                else if (fallspeed < 5) { severity = (one * 5 / 10); }
                else if (fallspeed < (one * 55 / 10)) { severity = (one * 6 / 10); }
                else if (fallspeed < 6) { severity = (one * 8 / 10); }
                else { severity = 1; }
                if ((one * (platform.TimeMillisecondsFromStart() - lastfalldamagetimeMilliseconds) / 1000) < 1)
                {
                    return;
                }
                lastfalldamagetimeMilliseconds = platform.TimeMillisecondsFromStart();
                ApplyDamageToPlayer(platform.FloatToInt(severity * PlayerStats.MaxHealth), Packet_DeathReasonEnum.FallDamage, 0);	//Maybe give ID of last player touched?
            }
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
        int min = Game.MinInt(oldheight, newheight);
        int max = Game.MaxInt(oldheight, newheight);
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

    internal int DialogsCount()
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

    internal void DrawDialogs()
    {
        for (int i = 0; i < dialogsCount; i++)
        {
            if (dialogs[i] == null)
            {
                continue;
            }
            VisibleDialog d = dialogs[i];
            int x = Width() / 2 - d.value.Width / 2;
            int y = Height() / 2 - d.value.Height_ / 2;
            for (int k = 0; k < d.value.WidgetsCount; k++)
            {
                Packet_Widget w = d.value.Widgets[k];
                if (w == null)
                {
                    continue;
                }
                if (w.Text != null)
                {
                    w.Text = platform.StringReplace(w.Text, "!SERVER_IP!", ServerInfo.connectdata.Ip);
                    w.Text = platform.StringReplace(w.Text, "!SERVER_PORT!", platform.IntToString(ServerInfo.connectdata.Port));
                    if (w.Font != null)
                    {
                        FontCi font = FontCi.Create(ValidFont(w.Font.FamilyName), DeserializeFloat(w.Font.SizeFloat), w.Font.FontStyle);
                        Draw2dText(w.Text, font, w.X + x, w.Y + y, IntRef.Create(w.Color), false);
                    }
                    else
                    {
                        FontCi font = FontCi.Create("Arial", 12, 0);
                        Draw2dText(w.Text, font, w.X + x, w.Y + y, IntRef.Create(w.Color), false);
                    }
                }
                if (w.Image == "Solid")
                {
                    Draw2dTexture(WhiteTexture(), w.X + x, w.Y + y, w.Width, w.Height_, null, 0, w.Color, false);
                }
                else if (w.Image != null)
                {
                    Draw2dBitmapFile(StringTools.StringAppend(platform, w.Image, ".png"), w.X + x, w.Y + y, w.Width, w.Height_);
                }
            }
        }
    }

    internal void DrawEnemyHealthCommon(string name, float progress)
    {
        DrawEnemyHealthUseInfo(name, 1, false);
    }

    internal Vector3IntRef currentAttackedBlock;

    internal void DrawEnemyHealthBlock()
    {
        if (currentAttackedBlock != null)
        {
            int x = currentAttackedBlock.X;
            int y = currentAttackedBlock.Y;
            int z = currentAttackedBlock.Z;
            int blocktype = GetBlock(x, y, z);
            float health = GetCurrentBlockHealth(x, y, z);
            float progress = health / d_Data.Strength()[blocktype];
            if (IsUsableBlock(blocktype))
            {
                DrawEnemyHealthUseInfo(language.Get(StringTools.StringAppend(platform, "Block_", blocktypes[blocktype].Name)), progress, true);
            }
            DrawEnemyHealthCommon(language.Get(StringTools.StringAppend(platform, "Block_", blocktypes[blocktype].Name)), progress);
        }
    }

    internal void SendRequestBlob()
    {
        Packet_ClientRequestBlob p = new Packet_ClientRequestBlob(); //{ RequestBlobMd5 = needed };
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.RequestBlob;
        pp.RequestBlob = p;
        SendPacketClient(pp);
    }

    public const int MonsterIdFirst = 1000;
    internal int currentTimeMilliseconds;
    internal GameDataMonsters d_DataMonsters;
    internal int ReceivedMapLength;

    internal void ReadAndUpdatePlayerPosition(Packet_PositionAndOrientation positionAndOrientation, int playerid)
    {
        float x = (one * positionAndOrientation.X / 32);
        float y = (one * positionAndOrientation.Y / 32);
        float z = (one * positionAndOrientation.Z / 32);
        byte heading = Game.IntToByte(positionAndOrientation.Heading);
        byte pitch = Game.IntToByte(positionAndOrientation.Pitch);
        bool leanleft = false;
        bool leanright = false;
        if (positionAndOrientation.Stance == 1)
        {
            leanleft = true;
        }
        if (positionAndOrientation.Stance == 2)
        {
            leanright = true;
        }
        float realposX = x;
        float realposY = y;
        float realposZ = z;
        if (playerid == this.LocalPlayerId)
        {
            if (!EnablePlayerUpdatePositionContainsKey(playerid) || EnablePlayerUpdatePosition(playerid))
            {
                player.playerposition.X = realposX;
                player.playerposition.Y = realposY;
                player.playerposition.Z = realposZ;
                // LocalPlayerOrientation = HeadingPitchToOrientation(heading, pitch);
                localstance = Game.IntToByte(positionAndOrientation.Stance);
            }
            spawned = true;
        }
        else
        {
            if (entities[playerid] == null)
            {
                entities[playerid] = new Entity();
                entities[playerid].player = new Player();
                entities[playerid].player.Name = "invalid";
                InvalidPlayerWarning(playerid);
            }
            if (!EnablePlayerUpdatePositionContainsKey(playerid) || EnablePlayerUpdatePosition(playerid))
            {
                entities[playerid].player.NetworkX = realposX;
                entities[playerid].player.NetworkY = realposY;
                entities[playerid].player.NetworkZ = realposZ;
                entities[playerid].player.PositionLoaded = true;
            }
            entities[playerid].player.NetworkHeading = heading;
            entities[playerid].player.NetworkPitch = pitch;
            entities[playerid].player.AnimationHint_.leanleft = leanleft;
            entities[playerid].player.AnimationHint_.leanright = leanright;
            entities[playerid].player.LastUpdateMilliseconds = platform.TimeMillisecondsFromStart();
        }
    }

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

    internal int MapLoadingPercentComplete;
    internal string MapLoadingStatus;
    internal int LastReceivedMilliseconds;
    internal int screenshotflash;
    internal int playertexturedefault;
    public const string playertexturedefaultfilename = "mineplayer.png";
    internal bool ENABLE_DRAW_TEST_CHARACTER;
    internal AnimationState a;
    internal int skyspheretexture;
    internal int skyspherenighttexture;
    internal SkySphere skysphere;
    internal int reloadblock;
    internal int reloadstartMilliseconds;
    internal int PreviousActiveMaterialBlock;
    internal int lastOxygenTickMilliseconds;
    internal bool freemousejustdisabled;
    internal int typinglogpos;
    internal TypingState GuiTyping;
    internal ConnectData connectdata;
    internal bool issingleplayer;
    internal bool StartedSinglePlayerServer;
    internal bool IsShiftPressed;
    internal bool reconnect;
    internal float rotation_speed;
    internal void SendLeave(int reason)
    {
        Packet_Client p = new Packet_Client();
        p.Id = Packet_ClientIdEnum.Leave;
        p.Leave = new Packet_ClientLeave();
        p.Leave.Reason = reason;
        SendPacketClient(p);
    }
    internal HudInventory d_HudInventory;
    internal WeaponRenderer d_Weapon;
    internal IFrustumCulling d_FrustumCulling;
    internal CharacterPhysicsCi d_Physics;
    internal ClientModManager1 modmanager;
    internal ClientMod[] clientmods;
    internal int clientmodsCount;
    internal bool SkySphereNight;
    internal ParticleEffectBlockBreak particleEffectBlockBreak;
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

    internal void SendPosition(float positionX, float positionY, float positionZ, float orientationX, float orientationY, float orientationZ)
    {
        Packet_ClientPositionAndOrientation p = new Packet_ClientPositionAndOrientation();
        {
            p.PlayerId = this.LocalPlayerId;//self
            p.X = platform.FloatToInt(positionX * 32);
            p.Y = platform.FloatToInt(positionY * 32);
            p.Z = platform.FloatToInt(positionZ * 32);
            p.Heading = HeadingByte(orientationX, orientationY, orientationZ);
            p.Pitch = PitchByte(orientationX, orientationY, orientationZ);
            p.Stance = localstance;
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.PositionandOrientation;
        pp.PositionAndOrientation = p;
        SendPacketClient(pp);
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
            if (entities[i].player == null)
            {
                continue;
            }
            Player p = entities[i].player;
            if (p.PositionLoaded)
            {
                if (IsPlayerInPos(p.PositionX, p.PositionY, p.PositionZ,
                    blockposX, blockposY, blockposZ))
                {
                    return true;
                }
            }
        }
        return IsPlayerInPos(player.playerposition.X, player.playerposition.Y, player.playerposition.Z,
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

    internal void CraftingRecipeSelected(int x, int y, int z, IntRef recipe)
    {
        if (recipe == null)
        {
            return;
        }
        Packet_ClientCraft cmd = new Packet_ClientCraft();
        cmd.X = x;
        cmd.Y = y;
        cmd.Z = z;
        cmd.RecipeId = recipe.value;
        Packet_Client p = new Packet_Client();
        p.Id = Packet_ClientIdEnum.Craft;
        p.Craft = cmd;
        SendPacketClient(p);
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
            player.playerorientation.Y += mouseDeltaX * rotationspeed * dt;
            player.playerorientation.X += mouseDeltaY * rotationspeed * dt;
            player.playerorientation.X = Game.ClampFloat(player.playerorientation.X,
                Game.GetPi() / 2 + (one * 15 / 1000),
                (Game.GetPi() / 2 + Game.GetPi() - (one * 15 / 1000)));
        }
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
            if (entities[i].player == null)
            {
                continue;
            }
            Player p = entities[i].player;
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

    public void DrawBlockInfo()
    {
        int x = SelectedBlockPositionX;
        int y = SelectedBlockPositionZ;
        int z = SelectedBlockPositionY;
        //string info = "None";
        if (!IsValidPos(x, y, z))
        {
            return;
        }
        int blocktype = GetBlock(x, y, z);
        if (!IsValid(blocktype))
        {
            return;
        }
        currentAttackedBlock = Vector3IntRef.Create(x, y, z);
        DrawEnemyHealthBlock();
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

    internal void DrawAmmo()
    {
        Packet_Item item = d_Inventory.RightHand[ActiveMaterial];
        if (item != null && item.ItemClass == Packet_ItemClassEnum.Block)
        {
            if (blocktypes[item.BlockId].IsPistol)
            {
                int loaded = LoadedAmmo[item.BlockId];
                int total = TotalAmmo[item.BlockId];
                string s = platform.StringFormat2("{0}/{1}", platform.IntToString(loaded), platform.IntToString(total - loaded));
                FontCi font = new FontCi();
                font.family = "Arial";
                font.size = 18;
                Draw2dText(s, font, Width() - TextSizeWidth(s, 18) - 50,
                    Height() - TextSizeHeight(s, 18) - 50, loaded == 0 ? IntRef.Create(Game.ColorFromArgb(255, 255, 0, 0)) : IntRef.Create(Game.ColorFromArgb(255, 255, 255, 255)), false);
                if (loaded == 0)
                {
                    font.size = 14;
                    string pressR = "Press R to reload";
                    Draw2dText(pressR, font, Width() - TextSizeWidth(pressR, 14) - 50,
                        Height() - TextSizeHeight(s, 14) - 80, IntRef.Create(Game.ColorFromArgb(255, 255, 0, 0)), false);
                }
            }
        }
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

        platform.DrawModelData(circleModelData);

        GLPopMatrix();
    }

    internal void DrawAim()
    {
        int aimwidth = 32;
        int aimheight = 32;

        if (CurrentAimRadius() > 1)
        {
            float fov_ = this.currentfov();
            Circle3i(Width() / 2, Height() / 2, CurrentAimRadius() * this.fov / fov_);
        }
        Draw2dBitmapFile("target.png", Width() / 2 - aimwidth / 2, Height() / 2 - aimheight / 2, aimwidth, aimheight);
    }

    internal void DrawSkySphere()
    {
        if (skyspheretexture == -1)
        {
            BoolRef found = new BoolRef();
            skyspheretexture = platform.LoadTextureFromFile(platform.GetFullFilePath("skysphere.png", found));
            skyspherenighttexture = platform.LoadTextureFromFile(platform.GetFullFilePath("skyspherenight.png", found));
        }
        int texture = SkySphereNight ? skyspherenighttexture : skyspheretexture;
        if (terrainRenderer.shadowssimple) //d_Shadows.GetType() == typeof(ShadowsSimple))
        {
            texture = skyspheretexture;
        }
        skysphere.SkyTexture = texture;
        skysphere.Draw(currentfov());
    }
    internal int totaltimeMilliseconds;

    float bouncespeedmultiply;
    float walldistance;
    internal Vector3Ref GrenadeBounce(Vector3Ref oldposition, Vector3Ref newposition, Vector3Ref velocity, float dt)
    {
        bool ismoving = velocity.Length() > 100 * dt;
        float modelheight = walldistance;
        oldposition.Y += walldistance;
        newposition.Y += walldistance;

        //Math.Floor() is needed because casting negative values to integer is not floor.
        Vector3IntRef oldpositioni = Vector3IntRef.Create(MathFloor(oldposition.X),
            MathFloor(oldposition.Z),
            MathFloor(oldposition.Y));
        float playerpositionX = newposition.X;
        float playerpositionY = newposition.Y;
        float playerpositionZ = newposition.Z;
        //left
        {
            float qnewpositionX = newposition.X;
            float qnewpositionY = newposition.Y;
            float qnewpositionZ = newposition.Z + walldistance;
            bool newempty = IsTileEmptyForPhysics(MathFloor(qnewpositionX), MathFloor(qnewpositionZ), MathFloor(qnewpositionY))
            && IsTileEmptyForPhysics(MathFloor(qnewpositionX), MathFloor(qnewpositionZ), MathFloor(qnewpositionY) + 1);
            if (newposition.Z - oldposition.Z > 0)
            {
                if (!newempty)
                {
                    velocity.Z = -velocity.Z;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.Z = oldposition.Z - newposition.Z;
                }
            }
        }
        //front
        {
            float qnewpositionX = newposition.X + walldistance;
            float qnewpositionY = newposition.Y;
            float qnewpositionZ = newposition.Z;
            bool newempty = IsTileEmptyForPhysics(MathFloor(qnewpositionX), MathFloor(qnewpositionZ), MathFloor(qnewpositionY))
            && IsTileEmptyForPhysics(MathFloor(qnewpositionX), MathFloor(qnewpositionZ), MathFloor(qnewpositionY) + 1);
            if (newposition.X - oldposition.X > 0)
            {
                if (!newempty)
                {
                    velocity.X = -velocity.X;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.X = oldposition.X - newposition.X;
                }
            }
        }
        //top
        {
            float qnewpositionX = newposition.X;
            float qnewpositionY = newposition.Y - walldistance;
            float qnewpositionZ = newposition.Z;
            int x = MathFloor(qnewpositionX);
            int y = MathFloor(qnewpositionZ);
            int z = MathFloor(qnewpositionY);
            float a_ = walldistance;
            bool newfull = (!IsTileEmptyForPhysics(x, y, z))
                || (qnewpositionX - MathFloor(qnewpositionX) <= a_ && (!IsTileEmptyForPhysics(x - 1, y, z)) && (IsTileEmptyForPhysics(x - 1, y, z + 1)))
                || (qnewpositionX - MathFloor(qnewpositionX) >= (1 - a_) && (!IsTileEmptyForPhysics(x + 1, y, z)) && (IsTileEmptyForPhysics(x + 1, y, z + 1)))
                || (qnewpositionZ - MathFloor(qnewpositionZ) <= a_ && (!IsTileEmptyForPhysics(x, y - 1, z)) && (IsTileEmptyForPhysics(x, y - 1, z + 1)))
                || (qnewpositionZ - MathFloor(qnewpositionZ) >= (1 - a_) && (!IsTileEmptyForPhysics(x, y + 1, z)) && (IsTileEmptyForPhysics(x, y + 1, z + 1)));
            if (newposition.Y - oldposition.Y < 0)
            {
                if (newfull)
                {
                    velocity.Y = -velocity.Y;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.Y = oldposition.Y - newposition.Y;
                }
            }
        }
        //right
        {
            float qnewpositionX = newposition.X;
            float qnewpositionY = newposition.Y;
            float qnewpositionZ = newposition.Z - walldistance;
            bool newempty = IsTileEmptyForPhysics(MathFloor(qnewpositionX), MathFloor(qnewpositionZ), MathFloor(qnewpositionY))
            && IsTileEmptyForPhysics(MathFloor(qnewpositionX), MathFloor(qnewpositionZ), MathFloor(qnewpositionY) + 1);
            if (newposition.Z - oldposition.Z < 0)
            {
                if (!newempty)
                {
                    velocity.Z = -velocity.Z;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.Z = oldposition.Z - newposition.Z;
                }
            }
        }
        //back
        {
            float qnewpositionX = newposition.X - walldistance;
            float qnewpositionY = newposition.Y;
            float qnewpositionZ = newposition.Z;
            bool newempty = IsTileEmptyForPhysics(MathFloor(qnewpositionX), MathFloor(qnewpositionZ), MathFloor(qnewpositionY))
            && IsTileEmptyForPhysics(MathFloor(qnewpositionX), MathFloor(qnewpositionZ), MathFloor(qnewpositionY) + 1);
            if (newposition.X - oldposition.X < 0)
            {
                if (!newempty)
                {
                    velocity.X = -velocity.X;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.X = oldposition.X - newposition.X;
                }
            }
        }
        //bottom
        {
            float qnewpositionX = newposition.X;
            float qnewpositionY = newposition.Y + modelheight;
            float qnewpositionZ = newposition.Z;
            bool newempty = IsTileEmptyForPhysics(MathFloor(qnewpositionX), MathFloor(qnewpositionZ), MathFloor(qnewpositionY));
            if (newposition.Y - oldposition.Y > 0)
            {
                if (!newempty)
                {
                    velocity.Y = -velocity.Y;
                    velocity.X *= bouncespeedmultiply;
                    velocity.Y *= bouncespeedmultiply;
                    velocity.Z *= bouncespeedmultiply;
                    if (ismoving)
                    {
                        AudioPlayAt("grenadebounce.ogg", newposition.X, newposition.Y, newposition.Z);
                    }
                    //playerposition.Y = oldposition.Y - newposition.Y;
                }
            }
        }
        //ok:
        playerpositionY -= walldistance;
        return Vector3Ref.Create(playerpositionX, playerpositionY, playerpositionZ);
    }

    internal Entity[] entities;
    internal int entitiesCount;

    internal void EntityExpire(float dt)
    {
        for (int i = 0; i < entitiesCount; i++)
        {
            Entity entity = entities[i];
            if (entity == null) { continue; }
            if (entity.expires == null) { continue; }
            entity.expires.timeLeft -= dt;
            if (entity.expires.timeLeft <= 0)
            {
                if (entity.grenade != null)
                {
                    GrenadeExplosion(i);
                }
                entities[i] = null;
            }
        }
    }

    const int localEntityId = 3000;

    internal void EntityAddLocal(Entity entity)
    {
        for (int i = localEntityId; i < entitiesCount; i++)
        {
            if (entities[i] == null)
            {
                entities[i] = entity;
                break;
            }
        }
    }

    internal void GetEntitiesPush(Vector3Ref push)
    {
        float LocalPlayerPositionX = player.playerposition.X;
        float LocalPlayerPositionY = player.playerposition.Y;
        float LocalPlayerPositionZ = player.playerposition.Z;
        for (int i = 0; i < entitiesCount; i++)
        {
            Entity entity = entities[i];
            if (entity == null) { continue; }
            if (entity.push == null) { continue; }
            float kposX = DeserializeFloat(entity.push.XFloat);
            float kposY = DeserializeFloat(entity.push.ZFloat);
            float kposZ = DeserializeFloat(entity.push.YFloat);
            if (entity.push.IsRelativeToPlayerPosition != 0)
            {
                kposX += LocalPlayerPositionX;
                kposY += LocalPlayerPositionY;
                kposZ += LocalPlayerPositionZ;
            }
            float dist = Dist(kposX, kposY, kposZ, LocalPlayerPositionX, LocalPlayerPositionY, LocalPlayerPositionZ);
            if (dist < DeserializeFloat(entity.push.RangeFloat))
            {
                float diffX = LocalPlayerPositionX - kposX;
                float diffY = LocalPlayerPositionY - kposY;
                float diffZ = LocalPlayerPositionZ - kposZ;
                push.X += diffX;
                push.Y += diffY;
                push.Z += diffZ;
            }
        }
    }

    internal float PlayerPushDistance;

    internal void DrawSprites()
    {
        for (int i = 0; i < entitiesCount; i++)
        {
            Entity entity = entities[i];
            if (entity == null) { continue; }
            if (entity.sprite == null) { continue; }
            Sprite b = entity.sprite;
            GLMatrixModeModelView();
            GLPushMatrix();
            GLTranslate(b.positionX, b.positionY, b.positionZ);
            GLRotate(0 - player.playerorientation.Y * 360 / (2 * Game.GetPi()), 0, 1, 0);
            GLRotate(0 - player.playerorientation.X * 360 / (2 * Game.GetPi()), 1, 0, 0);
            GLScale((one * 2 / 100), (one * 2 / 100), (one * 2 / 100));
            GLTranslate(0 - b.size / 2, 0 - b.size / 2, 0);
            //d_Draw2d.Draw2dTexture(night ? moontexture : suntexture, 0, 0, ImageSize, ImageSize, null, Color.White);
            IntRef n = null;
            if (b.animationcount > 0)
            {
                float progress = one - (entity.expires.timeLeft / entity.expires.totalTime);
                n = IntRef.Create(platform.FloatToInt(progress * (b.animationcount * b.animationcount - 1)));
            }
            Draw2dTexture(GetTexture(b.image), 0, 0, b.size, b.size, n, b.animationcount, Game.ColorFromArgb(255, 255, 255, 255), true);
            GLPopMatrix();
        }
    }

    float projectilegravity;

    internal void UpdateGrenade(int grenadeEntityId, float dt)
    {
        float LocalPlayerPositionX = player.playerposition.X;
        float LocalPlayerPositionY = player.playerposition.Y;
        float LocalPlayerPositionZ = player.playerposition.Z;

        Entity grenadeEntity = entities[grenadeEntityId];
        Sprite grenadeSprite = grenadeEntity.sprite;
        Grenade_ grenade = grenadeEntity.grenade;

        float oldposX = grenadeEntity.sprite.positionX;
        float oldposY = grenadeSprite.positionY;
        float oldposZ = grenadeSprite.positionZ;
        float newposX = grenadeSprite.positionX + grenade.velocityX * dt;
        float newposY = grenadeSprite.positionY + grenade.velocityY * dt;
        float newposZ = grenadeSprite.positionZ + grenade.velocityZ * dt;
        grenade.velocityY += -projectilegravity * dt;

        Vector3Ref velocity = Vector3Ref.Create(grenade.velocityX, grenade.velocityY, grenade.velocityZ);
        Vector3Ref bouncePosition = GrenadeBounce(Vector3Ref.Create(oldposX, oldposY, oldposZ), Vector3Ref.Create(newposX, newposY, newposZ), velocity, dt);
        grenade.velocityX = velocity.X;
        grenade.velocityY = velocity.Y;
        grenade.velocityZ = velocity.Z;
        grenadeSprite.positionX = bouncePosition.X;
        grenadeSprite.positionY = bouncePosition.Y;
        grenadeSprite.positionZ = bouncePosition.Z;
    }

    void GrenadeExplosion(int grenadeEntityId)
    {
        float LocalPlayerPositionX = player.playerposition.X;
        float LocalPlayerPositionY = player.playerposition.Y;
        float LocalPlayerPositionZ = player.playerposition.Z;

        Entity grenadeEntity = entities[grenadeEntityId];
        Sprite grenadeSprite = grenadeEntity.sprite;
        Grenade_ grenade = grenadeEntity.grenade;

        AudioPlayAt("grenadeexplosion.ogg", grenadeSprite.positionX, grenadeSprite.positionY, grenadeSprite.positionZ);

        {
            Entity entity = new Entity();

            Sprite spritenew = new Sprite();
            spritenew.image = "ani5.jpg";
            spritenew.positionX = grenadeSprite.positionX;
            spritenew.positionY = grenadeSprite.positionY + 1;
            spritenew.positionZ = grenadeSprite.positionZ;
            spritenew.size = 200;
            spritenew.animationcount = 4;

            entity.sprite = spritenew;
            entity.expires = Expires.Create(1);
            EntityAddLocal(entity);
        }

        {
            Packet_ServerExplosion explosion = new Packet_ServerExplosion();
            explosion.XFloat = SerializeFloat(grenadeSprite.positionX);
            explosion.YFloat = SerializeFloat(grenadeSprite.positionZ);
            explosion.ZFloat = SerializeFloat(grenadeSprite.positionY);
            explosion.RangeFloat = blocktypes[grenade.block].ExplosionRangeFloat;
            explosion.IsRelativeToPlayerPosition = 0;
            explosion.TimeFloat = blocktypes[grenade.block].ExplosionTimeFloat;

            Entity entity = new Entity();
            entity.push = explosion;
            entity.expires = new Expires();
            entity.expires.timeLeft = DeserializeFloat(blocktypes[grenade.block].ExplosionTimeFloat);
            EntityAddLocal(entity);
        }

        float dist = Dist(LocalPlayerPositionX, LocalPlayerPositionY, LocalPlayerPositionZ, grenadeSprite.positionX, grenadeSprite.positionY, grenadeSprite.positionZ);
        float dmg = (1 - dist / DeserializeFloat(blocktypes[grenade.block].ExplosionRangeFloat)) * DeserializeFloat(blocktypes[grenade.block].DamageBodyFloat);
        if (dmg > 0)
        {
            ApplyDamageToPlayer(platform.FloatToInt(dmg), Packet_DeathReasonEnum.Explosion, grenade.sourcePlayer);
        }
    }

    internal void UpdateBullets(float dt)
    {
        for (int i = 0; i < entitiesCount; i++)
        {
            Entity entity = entities[i];
            if (entity == null) { continue; }
            if (entity.bullet == null) { continue; }

            Bullet_ b = entity.bullet;
            if (b.progress < 1)
            {
                b.progress = 1;
            }

            float dirX = b.toX - b.fromX;
            float dirY = b.toY - b.fromY;
            float dirZ = b.toZ - b.fromZ;
            float length = Dist(0, 0, 0, dirX, dirY, dirZ);
            dirX /= length;
            dirY /= length;
            dirZ /= length;

            float posX = b.fromX;
            float posY = b.fromY;
            float posZ = b.fromZ;
            posX += dirX * (b.progress + b.speed * dt);
            posY += dirY * (b.progress + b.speed * dt);
            posZ += dirZ * (b.progress + b.speed * dt);
            b.progress += b.speed * dt;

            entity.sprite.positionX = posX;
            entity.sprite.positionY = posY;
            entity.sprite.positionZ = posZ;

            if (b.progress > length)
            {
                entities[i] = null;
            }
        }
    }

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

    internal void InterpolatePositions(float dt)
    {
        for (int i = 0; i < entitiesCount; i++)
        {
            Entity e = entities[i];
            if (e == null) { continue; }
            if (e.player == null) { continue; }

            if (e.player.playerDrawInfo == null)
            {
                e.player.playerDrawInfo = new PlayerDrawInfo();
                NetworkInterpolation n = new NetworkInterpolation();
                PlayerInterpolate playerInterpolate = new PlayerInterpolate();
                playerInterpolate.platform = platform;
                n.req = playerInterpolate;
                n.DELAYMILLISECONDS = 500;
                n.EXTRAPOLATE = false;
                n.EXTRAPOLATION_TIMEMILLISECONDS = 300;
                e.player.playerDrawInfo.interpolation = n;
            }
            e.player.playerDrawInfo.interpolation.DELAYMILLISECONDS = Game.MaxInt(100, ServerInfo.ServerPing.RoundtripTimeTotalMilliseconds());
            Player p = e.player;

            PlayerDrawInfo info = p.playerDrawInfo;
            float realposX = p.NetworkX;
            float realposY = p.NetworkY;
            float realposZ = p.NetworkZ;
            if ((!Vec3Equal(realposX, realposY, realposZ,
                            info.lastrealposX, info.lastrealposY, info.lastrealposZ))
                || p.Heading != info.lastrealheading
                || p.Pitch != info.lastrealpitch)
            {
                PlayerInterpolationState state = new PlayerInterpolationState();
                state.positionX = realposX;
                state.positionY = realposY;
                state.positionZ = realposZ;
                state.heading = p.NetworkHeading;
                state.pitch = p.NetworkPitch;
                info.interpolation.AddNetworkPacket(state, totaltimeMilliseconds);
            }
            PlayerInterpolationState curstate = platform.CastToPlayerInterpolationState(info.interpolation.InterpolatedState(totaltimeMilliseconds));
            if (curstate == null)
            {
                curstate = new PlayerInterpolationState();
            }
            //do not interpolate player position if player is controlled by game world
            if (EnablePlayerUpdatePositionContainsKey(i) && !EnablePlayerUpdatePosition(i))
            {
                curstate.positionX = p.NetworkX;
                curstate.positionY = p.NetworkY;
                curstate.positionZ = p.NetworkZ;
            }
            float curposX = curstate.positionX;
            float curposY = curstate.positionY;
            float curposZ = curstate.positionZ;
            info.velocityX = curposX - info.lastcurposX;
            info.velocityY = curposY - info.lastcurposY;
            info.velocityZ = curposZ - info.lastcurposZ;
            p.moves = (!Vec3Equal(curposX, curposY, curposZ, info.lastcurposX, info.lastcurposY, info.lastcurposZ));
            info.lastcurposX = curposX;
            info.lastcurposY = curposY;
            info.lastcurposZ = curposZ;
            info.lastrealposX = realposX;
            info.lastrealposY = realposY;
            info.lastrealposZ = realposZ;
            info.lastrealheading = p.Heading;
            info.lastrealpitch = p.Pitch;

            p.PositionX = curposX;
            p.PositionY = curposY;
            p.PositionZ = curposZ;
            p.Heading = curstate.heading;
            p.Pitch = curstate.pitch;
        }
    }

    bool Vec3Equal(float ax, float ay, float az, float bx, float by, float bz)
    {
        return ax == bx && ay == by && az == bz;
    }

    internal void SetPlayers()
    {
        for (int i = 0; i < entitiesCount; i++)
        {
            Entity e = entities[i];
            if (e == null) { continue; }
            if (e.player == null) { continue; }
            if (e.push == null)
            {
                e.push = new Packet_ServerExplosion();
            }
            e.push.RangeFloat = SerializeFloat(PlayerPushDistance);
            e.push.XFloat = SerializeFloat(e.player.PositionX);
            e.push.YFloat = SerializeFloat(e.player.PositionZ);
            e.push.ZFloat = SerializeFloat(e.player.PositionY);

            if ((!e.player.PositionLoaded) ||
                (i == this.LocalPlayerId) || (e.player.Name == "")
                || (e.player.playerDrawInfo == null)
                || (e.player.playerDrawInfo.interpolation == null))
            {
                e.drawName = null;
            }
            else
            {
                if (e.drawName == null)
                {
                    e.drawName = new DrawName();
                }
                e.drawName.TextX = e.player.PositionX;
                e.drawName.TextY = e.player.PositionY + e.player.ModelHeight + one * 8 / 10;
                e.drawName.TextZ = e.player.PositionZ;
                e.drawName.Name = e.player.Name;
                if (e.player.Type == PlayerType.Monster)
                {
                    e.drawName.DrawHealth = true;
                    e.drawName.Health = one * e.player.Health / 20;
                }
            }
        }
    }

    internal bool[] keyboardState;

    const int KeyAltLeft = 5;
    const int KeyAltRight = 6;
    internal void DrawPlayerNames()
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
            int kKey = i;
            DrawName p = entities[i].drawName;
            //todo if picking
            if ((Dist(player.playerposition.X, player.playerposition.Y, player.playerposition.Z, p.TextX, p.TextY, p.TextZ) < 20)
                || keyboardState[KeyAltLeft] || keyboardState[KeyAltRight])
            {
                string name = p.Name;
                {
                    float posX = p.TextX;
                    float posY = p.TextY;
                    float posZ = p.TextZ;
                    float shadow = (one * MaybeGetLight(platform.FloatToInt(posX), platform.FloatToInt(posZ), platform.FloatToInt(posY))) / maxlight;
                    //do not interpolate player position if player is controlled by game world
                    //if (EnablePlayerUpdatePositionContainsKey(kKey) && !EnablePlayerUpdatePosition(kKey))
                    //{
                    //    posX = p.NetworkX;
                    //    posY = p.NetworkY;
                    //    posZ = p.NetworkZ;
                    //}
                    GLPushMatrix();
                    GLTranslate(posX, posY, posZ);
                    //if (p.Type == PlayerType.Monster)
                    //{
                    //    GLTranslate(0, 1, 0);
                    //}
                    GLRotate(-player.playerorientation.Y * 360 / (2 * Game.GetPi()), 0, 1, 0);
                    GLRotate(-player.playerorientation.X * 360 / (2 * Game.GetPi()), 1, 0, 0);
                    float scale = one * 2 / 100;
                    GLScale(scale, scale, scale);

                    //Color c = Color.FromArgb((int)(shadow * 255), (int)(shadow * 255), (int)(shadow * 255));
                    //Todo: Can't change text color because text has outline anyway.
                    if (p.DrawHealth)
                    {
                        Draw2dTexture(WhiteTexture(), -26, -11, 52, 12, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
                        Draw2dTexture(WhiteTexture(), -25, -10, 50 * (one * p.Health), 10, null, 0, Game.ColorFromArgb(255, 255, 0, 0), false);
                    }
                    FontCi font = new FontCi();
                    font.family = "Arial";
                    font.size = 14;
                    Draw2dText(name, font, -TextSizeWidth(name, 14) / 2, 0, IntRef.Create(Game.ColorFromArgb(255, 255, 255, 255)), true);
                    //                        GL.Translate(0, 1, 0);
                    GLPopMatrix();
                }
            }
        }
    }

    internal bool Swimming()
    {
        if (GetPlayerEyesBlock() == -1) { return true; }
        return d_Data.WalkableType1()[GetPlayerEyesBlock()] == Packet_WalkableTypeEnum.Fluid;
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
        float pX = player.playerposition.X;
        float pY = player.playerposition.Y;
        float pZ = player.playerposition.Z;
        pY += entities[LocalPlayerId].player.EyeHeight;
        if (!IsValidPos(MathFloor(pX), MathFloor(pZ), MathFloor(pY)))
        {
            if (pY < WaterLevel())
            {
                return -1;
            }
            return 0;
        }
        return GetBlock(platform.FloatToInt(pX), platform.FloatToInt(pZ), platform.FloatToInt(pY));
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
            IntRef blockunderplayer = BlockUnderPlayer();
            if (blockunderplayer != null)
            {
                movespeednow *= d_Data.WalkSpeed()[blockunderplayer.value];
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
            movespeednow *= DeserializeFloat(blocktypes[item.BlockId].WalkSpeedWhenUsedFloat);
            if (IronSights)
            {
                movespeednow *= DeserializeFloat(blocktypes[item.BlockId].IronSightsMoveSpeedFloat);
            }
        }
        return movespeednow;
    }

    internal float VectorAngleGet(float qX, float qY, float qZ)
    {
        return (platform.MathAcos(qX / Length(qX, qY, qZ)) * MathSign(qZ));
    }

    int MathSign(float q)
    {
        if (q < 0)
        {
            return -1;
        }
        else if (q == 0)
        {
            return 0;
        }
        else
        {
            return 1;
        }
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
        freemousejustdisabled = true;
    }

    internal float overheadcameradistance;
    internal float tppcameradistance;
    internal int TPP_CAMERA_DISTANCE_MIN;
    internal int TPP_CAMERA_DISTANCE_MAX;
    internal void MouseWheelChanged(float eDeltaPrecise)
    {
        if (keyboardState[GetKey(GlKeys.LControl)])
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
    }

    internal Packet_Client CreateLoginPacket(string username, string verificationKey)
    {
        Packet_ClientIdentification p = new Packet_ClientIdentification();
        {
            p.Username = username;
            p.MdProtocolVersion = platform.GetGameVersion();
            p.VerificationKey = verificationKey;
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.PlayerIdentification;
        pp.Identification = p;
        return pp;
    }

    internal Packet_Client CreateLoginPacket_(string username, string verificationKey, string serverPassword)
    {
        Packet_ClientIdentification p = new Packet_ClientIdentification();
        {
            p.Username = username;
            p.MdProtocolVersion = platform.GetGameVersion();
            p.VerificationKey = verificationKey;
            p.ServerPassword = serverPassword;
        }
        Packet_Client pp = new Packet_Client();
        pp.Id = Packet_ClientIdEnum.PlayerIdentification;
        pp.Identification = p;
        return pp;
    }

    internal void Connect(string serverAddress, int port, string username, string auth)
    {
        main.Start();
        main.Connect(serverAddress, port);
        Packet_Client n = CreateLoginPacket(username, auth);
        SendPacketClient(n);
    }

    internal void Connect_(string serverAddress, int port, string username, string auth, string serverPassword)
    {
        main.Start();
        main.Connect(serverAddress, port);
        Packet_Client n = CreateLoginPacket_(username, auth, serverPassword);
        SendPacketClient(n);
    }

    internal void RedrawAllBlocks()
    {
        terrainRenderer.RedrawAllBlocks();
    }

    public const int clearcolorR = 171;
    public const int clearcolorG = 202;
    public const int clearcolorB = 228;
    public const int clearcolorA = 255;

    internal void SetFog()
    {            //Density for linear fog
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

    public const int BlockDamageToPlayerEvery = 1;
    TimerCi BlockDamageToPlayerTimer;

    //TODO server side?
    internal void UpdateBlockDamageToPlayer(float dt)
    {
        float pX = player.playerposition.X;
        float pY = player.playerposition.Y;
        float pZ = player.playerposition.Z;
        pY += entities[LocalPlayerId].player.EyeHeight;
        int block1 = 0;
        int block2 = 0;
        if (IsValidPos(MathFloor(pX), MathFloor(pZ), MathFloor(pY)))
        {
            block1 = GetBlock(platform.FloatToInt(pX), platform.FloatToInt(pZ), platform.FloatToInt(pY));
        }
        if (IsValidPos(MathFloor(pX), MathFloor(pZ), MathFloor(pY) - 1))
        {
            block2 = GetBlock(platform.FloatToInt(pX), platform.FloatToInt(pZ), platform.FloatToInt(pY) - 1);
        }

        int damage = d_Data.DamageToPlayer()[block1] + d_Data.DamageToPlayer()[block2];
        if (damage > 0)
        {
            int hurtingBlock = block1;	//Use block at eyeheight as source block
            if (hurtingBlock == 0) { hurtingBlock = block2; }	//Fallback to block at feet if eyeheight block is air
            int times = BlockDamageToPlayerTimer.Update(dt);
            for (int i = 0; i < times; i++)
            {
                ApplyDamageToPlayer(damage, Packet_DeathReasonEnum.BlockDamage, hurtingBlock);
            }
        }

        //Player drowning
        int deltaTime = platform.FloatToInt(one * (platform.TimeMillisecondsFromStart() - lastOxygenTickMilliseconds)); //Time in milliseconds
        if (deltaTime >= 1000)
        {
            if (WaterSwimming())
            {
                PlayerStats.CurrentOxygen -= 1;
                if (PlayerStats.CurrentOxygen <= 0)
                {
                    PlayerStats.CurrentOxygen = 0;
                    int dmg = platform.FloatToInt(one * PlayerStats.MaxHealth / 10);
                    if (dmg < 1)
                    {
                        dmg = 1;
                    }
                    ApplyDamageToPlayer(dmg, Packet_DeathReasonEnum.Drowning, block1);
                }
            }
            else
            {
                PlayerStats.CurrentOxygen = PlayerStats.MaxOxygen;
            }
            {
                Packet_Client packet = new Packet_Client();
                packet.Id = Packet_ClientIdEnum.Oxygen;
                packet.Oxygen = new Packet_ClientOxygen();
                packet.Oxygen.CurrentOxygen = PlayerStats.CurrentOxygen;
                SendPacketClient(packet);
            }
            lastOxygenTickMilliseconds = platform.TimeMillisecondsFromStart();
        }
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
    internal void LimitThirdPersonCameraToWalls(Vector3Ref eye, Vector3Ref target, FloatRef curtppcameradistance)
    {
        Vector3Ref ray_start_point = target;
        Vector3Ref raytarget = eye;

        Line3D pick = new Line3D();
        float raydirX = (raytarget.X - ray_start_point.X);
        float raydirY = (raytarget.Y - ray_start_point.Y);
        float raydirZ = (raytarget.Z - ray_start_point.Z);

        float raydirLength1 = Length(raydirX, raydirY, raydirZ);
        raydirX /= raydirLength1;
        raydirY /= raydirLength1;
        raydirZ /= raydirLength1;
        raydirX = raydirX * (tppcameradistance + 1);
        raydirY = raydirY * (tppcameradistance + 1);
        raydirZ = raydirZ * (tppcameradistance + 1);
        pick.Start = Vec3.FromValues(ray_start_point.X, ray_start_point.Y, ray_start_point.Z);
        pick.End = new float[3];
        pick.End[0] = ray_start_point.X + raydirX;
        pick.End[1] = ray_start_point.Y + raydirY;
        pick.End[2] = ray_start_point.Z + raydirZ;

        //pick terrain
        s.StartBox = Box3D.Create(0, 0, 0, BitTools.NextPowerOfTwo(Game.MaxInt(MapSizeX, Game.MaxInt(MapSizeY, MapSizeZ))));
        IntRef pick2Count = new IntRef();
        BlockPosSide[] pick2 = s.LineIntersection(IsBlockEmpty_.Create(this), GetBlockHeight_.Create(this), pick, pick2Count);

        if (pick2Count.value > 0)
        {
            BlockPosSide pick2nearest = Nearest(pick2, pick2Count.value, ray_start_point.X, ray_start_point.Y, ray_start_point.Z);
            //pick2.Sort((a, b) => { return (FloatArrayToVector3(a.blockPos) - ray_start_point).Length.CompareTo((FloatArrayToVector3(b.blockPos) - ray_start_point).Length); });

            float pickX = pick2nearest.blockPos[0] - target.X;
            float pickY = pick2nearest.blockPos[1] - target.Y;
            float pickZ = pick2nearest.blockPos[2] - target.Z;
            float pickdistance = Length(pickX, pickY, pickZ);
            curtppcameradistance.value = Game.MinFloat(pickdistance - 1, curtppcameradistance.value);
            if (curtppcameradistance.value < one * 3 / 10) { curtppcameradistance.value = one * 3 / 10; }
        }

        float cameraDirectionX = target.X - eye.X;
        float cameraDirectionY = target.Y - eye.Y;
        float cameraDirectionZ = target.Z - eye.Z;
        float raydirLength = Length(raydirX, raydirY, raydirZ);
        raydirX /= raydirLength;
        raydirY /= raydirLength;
        raydirZ /= raydirLength;
        eye.X = target.X + raydirX * curtppcameradistance.value;
        eye.Y = target.Y + raydirY * curtppcameradistance.value;
        eye.Z = target.Z + raydirZ * curtppcameradistance.value;
    }

    public const int upX = 0;
    public const int upY = 1;
    public const int upZ = 0;
    internal Kamera overheadcameraK;
    internal Vector3Ref OverheadCamera_cameraEye;
    internal float[] OverheadCamera()
    {
        overheadcameraK.GetPosition(platform, OverheadCamera_cameraEye);
        Vector3Ref cameraEye = OverheadCamera_cameraEye;
        Vector3Ref cameraTarget = Vector3Ref.Create(overheadcameraK.Center.X, overheadcameraK.Center.Y + GetCharacterEyesHeight(), overheadcameraK.Center.Z);
        FloatRef currentOverheadcameradistance = FloatRef.Create(overheadcameradistance);
        LimitThirdPersonCameraToWalls(cameraEye, cameraTarget, currentOverheadcameradistance);
        float[] ret = new float[16];
        Mat4.LookAt(ret, Vec3.FromValues(cameraEye.X, cameraEye.Y, cameraEye.Z),
            Vec3.FromValues(cameraTarget.X, cameraTarget.Y, cameraTarget.Z),
            Vec3.FromValues(upX, upY, upZ));
        return ret;
    }

    internal float[] FppCamera()
    {
        Vector3Ref forward = new Vector3Ref();
        VectorTool.ToVectorInFixedSystem(0, 0, 1, player.playerorientation.X, player.playerorientation.Y, forward);
        Vector3Ref cameraEye = new Vector3Ref();
        Vector3Ref cameraTarget = new Vector3Ref();
        float playerEyeX = player.playerposition.X;
        float playerEyeY = player.playerposition.Y + GetCharacterEyesHeight();
        float playerEyeZ = player.playerposition.Z;
        if (!ENABLE_TPP_VIEW)
        {
            cameraEye.X = playerEyeX;
            cameraEye.Y = playerEyeY;
            cameraEye.Z = playerEyeZ;
            cameraTarget.X = playerEyeX + forward.X;
            cameraTarget.Y = playerEyeY + forward.Y;
            cameraTarget.Z = playerEyeZ + forward.Z;
        }
        else
        {
            cameraEye.X = playerEyeX + forward.X * -tppcameradistance;
            cameraEye.Y = playerEyeY + forward.Y * -tppcameradistance;
            cameraEye.Z = playerEyeZ + forward.Z * -tppcameradistance;
            cameraTarget.X = playerEyeX;
            cameraTarget.Y = playerEyeY;
            cameraTarget.Z = playerEyeZ;
            FloatRef currentTppcameradistance = FloatRef.Create(tppcameradistance);
            LimitThirdPersonCameraToWalls(cameraEye, cameraTarget, currentTppcameradistance);
        }
        float[] ret = new float[16];
        Mat4.LookAt(ret, Vec3.FromValues(cameraEye.X, cameraEye.Y, cameraEye.Z),
            Vec3.FromValues(cameraTarget.X, cameraTarget.Y, cameraTarget.Z),
            Vec3.FromValues(upX, upY, upZ));
        return ret;
    }

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
                        SetBlockInChunk(destination, MapUtilCi.Index3d(x, y, z, destinationchunksize, destinationchunksize)
                            , source[MapUtilCi.Index3d(x + sourcex, y + sourcey, z + sourcez, sourcechunksizeX, sourcechunksizeY)]);
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
                    localchunks[MapUtilCi.Index3d(cx, cy, cz, (chunksizex / chunksize), (chunksizey / chunksize))] = GetChunk(x + cx * chunksize, y + cy * chunksize, z + cz * chunksize);
                    FillChunk(localchunks[MapUtilCi.Index3d(cx, cy, cz, (chunksizex / chunksize), (chunksizey / chunksize))], chunksize, cx * chunksize, cy * chunksize, cz * chunksize, chunk, sizeX, sizeY, sizeZ);
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
        int startx = Game.MinInt(a_.X, b_.X);
        int endx = Game.MaxInt(a_.X, b_.X);
        int starty = Game.MinInt(a_.Y, b_.Y);
        int endy = Game.MaxInt(a_.Y, b_.Y);
        int startz = Game.MinInt(a_.Z, b_.Z);
        int endz = Game.MaxInt(a_.Z, b_.Z);
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

    internal float HeadingToOrientationX(byte heading)
    {
        float x = (one * heading / 256) * 2 * Game.GetPi();
        return x;
    }

    internal float PitchToOrientationY(byte pitch)
    {
        float y = ((one * pitch / 256) * 2 * Game.GetPi()) - Game.GetPi();
        return y;
    }

    internal void OnPickUseWithTool(int posX, int posY, int posZ)
    {
        SendSetBlock(posX, posY, posZ, Packet_BlockSetModeEnum.UseWithTool, d_Inventory.RightHand[ActiveMaterial].BlockId, ActiveMaterial);
    }

    internal void KeyUp(int eKey)
    {
        keyboardState[eKey] = false;
        for (int i = 0; i < clientmodsCount; i++)
        {
            KeyEventArgs args_ = new KeyEventArgs();
            args_.SetKeyCode(eKey);
            clientmods[i].OnKeyUp(args_);
        }
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

        playerPositionSpawnX = player.playerposition.X;
        playerPositionSpawnY = player.playerposition.Y;
        playerPositionSpawnZ = player.playerposition.Z;
    }
    internal int[] materialSlots;

    internal int GetSoundCount(string[] soundwalk)
    {
        int count = 0;
        for (int i = 0; i < GameData.SoundCount; i++)
        {
            if (soundwalk[i] != null)
            {
                count++;
            }
        }
        return count;
    }

    internal string[] soundwalkcurrent()
    {
        IntRef b = BlockUnderPlayer();
        if (b != null)
        {
            return d_Data.WalkSound()[b.value];
        }
        return d_Data.WalkSound()[0];
    }

    float walksoundtimer;
    int lastwalksound;
    float stepsoundduration;
    internal void UpdateWalkSound(float dt)
    {
        if (dt == -1)
        {
            dt = stepsoundduration / 2;
        }
        walksoundtimer += dt;
        string[] soundwalk = soundwalkcurrent();
        if (GetSoundCount(soundwalk) == 0)
        {
            return;
        }
        if (walksoundtimer >= stepsoundduration)
        {
            walksoundtimer = 0;
            lastwalksound++;
            if (lastwalksound >= GetSoundCount(soundwalk))
            {
                lastwalksound = 0;
            }
            if ((rnd.Next() % 100) < 40)
            {
                lastwalksound = rnd.Next() % (GetSoundCount(soundwalk));
            }
            AudioPlay(soundwalk[lastwalksound]);
        }
    }

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
        int chart = 116;
        int charT = 84;
        int chary = 121;
        int charY = 89;
        if ((eKeyChar == chart || eKeyChar == charT) && GuiTyping == TypingState.None)
        {
            GuiTyping = TypingState.Typing;
            d_HudChat.GuiTypingBuffer = "";
            d_HudChat.IsTeamchat = false;
            return;
        }
        if ((eKeyChar == chary || eKeyChar == charY) && GuiTyping == TypingState.None)
        {
            GuiTyping = TypingState.Typing;
            d_HudChat.GuiTypingBuffer = "";
            d_HudChat.IsTeamchat = true;
            return;
        }
        if (GuiTyping == TypingState.Typing)
        {
            int c = eKeyChar;
            if (platform.IsValidTypingChar(c))
            {
                d_HudChat.GuiTypingBuffer = StringTools.StringAppend(platform, d_HudChat.GuiTypingBuffer, CharToString(c));
            }
            int charTab = 9;
            //Handles player name autocomplete in chat
            if (c == charTab && platform.StringTrim(d_HudChat.GuiTypingBuffer) != "")
            {
                for (int i = 0; i < entitiesCount; i++)
                {
                    if (entities[i] == null)
                    {
                        continue;
                    }
                    if (entities[i].player == null)
                    {
                        continue;
                    }
                    Player p = entities[i].player;
                    if (p.Type != PlayerType.Player)
                    {
                        continue;
                    }
                    //Use substring here because player names are internally in format &xNAME (so we need to cut first 2 characters)
                    if (platform.StringStartsWithIgnoreCase(StringTools.StringSubstringToEnd(platform, p.Name, 2), d_HudChat.GuiTypingBuffer))
                    {
                        d_HudChat.GuiTypingBuffer = StringTools.StringAppend(platform, StringTools.StringSubstringToEnd(platform, p.Name, 2), ": ");
                        break;
                    }
                }
            }
        }
        for (int k = 0; k < dialogsCount; k++)
        {
            if (dialogs[k] == null)
            {
                continue;
            }
            VisibleDialog d = dialogs[k];
            for (int i = 0; i < d.value.WidgetsCount; i++)
            {
                Packet_Widget w = d.value.Widgets[i];
                if (w == null)
                {
                    continue;
                }
                string valid = (StringTools.StringAppend(platform, "abcdefghijklmnopqrstuvwxyz1234567890\t ", CharToString(27)));
                if (platform.StringContains(valid, CharToString(w.ClickKey)))
                {
                    if (eKeyChar == w.ClickKey)
                    {
                        Packet_Client p = new Packet_Client();
                        p.Id = Packet_ClientIdEnum.DialogClick;
                        p.DialogClick_ = new Packet_ClientDialogClick();
                        p.DialogClick_.WidgetId = w.Id;
                        SendPacketClient(p);
                        return;
                    }
                }
            }
        }
    }
    string CharToString(int c)
    {
        int[] arr = new int[1];
        arr[0] = c;
        return platform.CharArrayToString(arr, 1);
    }

    internal void DrawMouseCursor()
    {
        Draw2dBitmapFile(platform.PathCombine("gui", "mousecursor.png"), mouseCurrentX, mouseCurrentY, 32, 32);
    }

    internal Speculative[] speculative;
    internal int speculativeCount;

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
                s_ = null;
            }
        }
    }

    internal void OnPick(int blockposX, int blockposY, int blockposZ, int blockposoldX, int blockposoldY, int blockposoldZ, float[] collisionPos, bool right)
    {
        float xfract = collisionPos[0] - MathFloor(collisionPos[0]);
        float zfract = collisionPos[2] - MathFloor(collisionPos[2]);
        int activematerial = MaterialSlots()[ActiveMaterial];
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

    internal void OnResize()
    {
        platform.GlViewport(0, 0, Width(), Height());
        this.Set3dProjection2();
    }

    internal void Reconnect()
    {
        reconnect = true;
        platform.WindowExit();
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
                        if (entities[i] == null)
                        {
                            continue;
                        }
                        if (entities[i].player == null)
                        {
                            continue;
                        }
                        Log(platform.StringFormat2("{0} {1}", platform.IntToString(i), entities[i].player.Name));
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
                else if (cmd == "testmodel")
                {
                    ENABLE_DRAW_TEST_CHARACTER = BoolCommandArgument(arguments);
                }
                else if (cmd == "gui")
                {
                    ENABLE_DRAW2D = BoolCommandArgument(arguments);
                }
                else if (cmd == "reconnect")
                {
                    Reconnect();
                }
                else
                {
                    for (int i = 0; i < clientmodsCount; i++)
                    {
                        ClientCommandArgs args = new ClientCommandArgs();
                        args.arguments = arguments;
                        args.command = cmd;
                        clientmods[i].OnClientCommand(args);
                    }
                    string chatline = StringTools.StringSubstring(platform, d_HudChat.GuiTypingBuffer, 0, MinInt(d_HudChat.GuiTypingBuffer.Length, 256));
                    SendChat(chatline);
                }
            }
            //catch (Exception e) { AddChatline(new StringReader(e.Message).ReadLine()); }
        }
        else
        {
            string chatline = StringTools.StringSubstring(platform, d_HudChat.GuiTypingBuffer, 0, Game.MinInt(StringTools.StringLength(platform, d_HudChat.GuiTypingBuffer), 4096));
            SendChat(chatline);
        }
    }
    bool BoolCommandArgument(string arguments)
    {
        arguments = platform.StringTrim(arguments);
        return (arguments == "" || arguments == "1" || arguments == "on" || arguments == "yes");
    }
    internal string[] typinglog;
    internal int typinglogCount;

    internal void ProcessServerIdentification(Packet_Server packet)
    {
        this.LocalPlayerId = packet.Identification.AssignedClientId;
        this.ServerInfo.connectdata = this.connectdata;
        this.ServerInfo.ServerName = packet.Identification.ServerName;
        this.ServerInfo.ServerMotd = packet.Identification.ServerMotd;
        this.d_TerrainChunkTesselator.ENABLE_TEXTURE_TILING = packet.Identification.RenderHint_ == RenderHintEnum.Fast;
        ChatLog("---Connected---");
        SendRequestBlob();
        if (packet.Identification.MapSizeX != MapSizeX
            || packet.Identification.MapSizeY != MapSizeY
            || packet.Identification.MapSizeZ != MapSizeZ)
        {
            Reset(packet.Identification.MapSizeX,
                packet.Identification.MapSizeY,
                packet.Identification.MapSizeZ);
            d_Heightmap.Restart();
        }
        //serverterraintexture = ByteArrayToString(packet.Identification.TerrainTextureMd5);
        terrainRenderer.shadowssimple = packet.Identification.DisableShadows == 1 ? true : false;
        maxdrawdistance = packet.Identification.PlayerAreaSize / 2;
        if (maxdrawdistance == 0)
        {
            maxdrawdistance = 128;
        }
    }

    internal void ProcessPacket(Packet_Server packet)
    {
        switch (packet.Id)
        {
            case Packet_ServerIdEnum.ServerIdentification:
                {
                    string invalidversionstr = language.InvalidVersionConnectAnyway();

                    string servergameversion = packet.Identification.MdProtocolVersion;
                    if (servergameversion != platform.GetGameVersion())
                    {
                        string q = platform.StringFormat2(invalidversionstr, platform.GetGameVersion(), servergameversion);
                        invalidVersionDrawMessage = q;
                        invalidVersionPacketIdentification = packet;
                    }
                    else
                    {
                        ProcessServerIdentification(packet);
                    }
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
                    for (int i = 0; i < this.ServerInfo.Players.count; i++)
                    {
                        ConnectedPlayer k = ServerInfo.Players.items[i];
                        if (k == null)
                        {
                            continue;
                        }
                        if (k.id == packet.PlayerPing.ClientId)
                        {
                            if (k.id == this.LocalPlayerId)
                            {
                                this.ServerInfo.ServerPing.Receive(platform);
                            }
                            k.ping = packet.PlayerPing.Ping;
                            break;
                        }
                    }
                }
                break;
            case Packet_ServerIdEnum.LevelInitialize:
                {
                    ReceivedMapLength = 0;
                    InvokeMapLoadingProgress(0, 0, language.Connecting());
                }
                break;
            case Packet_ServerIdEnum.LevelDataChunk:
                {
                    MapLoadingPercentComplete = packet.LevelDataChunk.PercentComplete;
                    MapLoadingStatus = packet.LevelDataChunk.Status;
                    InvokeMapLoadingProgress(MapLoadingPercentComplete, ReceivedMapLength, MapLoadingStatus);
                }
                break;
            case Packet_ServerIdEnum.LevelFinalize:
                {
                    //d_Data.Load(MyStream.ReadAllLines(d_GetFile.GetFile("blocks.csv")),
                    //    MyStream.ReadAllLines(d_GetFile.GetFile("defaultmaterialslots.csv")),
                    //    MyStream.ReadAllLines(d_GetFile.GetFile("lightlevels.csv")));
                    //d_CraftingRecipes.Load(MyStream.ReadAllLines(d_GetFile.GetFile("craftingrecipes.csv")));

                    MapLoaded();
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

                    int startx = MinInt(ax, bx);
                    int endx = MaxInt(ax, bx);
                    int starty = MinInt(ay, by);
                    int endy = MaxInt(ay, by);
                    int startz = MinInt(az, bz);
                    int endz = MaxInt(az, bz);

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
            case Packet_ServerIdEnum.SpawnPlayer:
                {
                    int playerid = packet.SpawnPlayer.PlayerId;
                    string playername = packet.SpawnPlayer.PlayerName;
                    bool isnewplayer = true;
                    for (int i = 0; i < ServerInfo.Players.count; i++)
                    {
                        ConnectedPlayer p = ServerInfo.Players.items[i];
                        if (p == null)
                        {
                            continue;
                        }
                        if (p.id == playerid)
                        {
                            isnewplayer = false;
                            p.name = playername;
                        }
                    }
                    if (isnewplayer)
                    {
                        ConnectedPlayer p = new ConnectedPlayer();
                        p.name = playername;
                        p.id = playerid;
                        p.ping = -1;
                        this.ServerInfo.Players.Add(p);
                    }
                    entities[playerid] = new Entity();
                    entities[playerid].player = new Player();
                    entities[playerid].player.Name = playername;
                    entities[playerid].player.Model = packet.SpawnPlayer.Model_;
                    entities[playerid].player.Texture = packet.SpawnPlayer.Texture_;
                    entities[playerid].player.EyeHeight = DeserializeFloat(packet.SpawnPlayer.EyeHeightFloat);
                    entities[playerid].player.ModelHeight = DeserializeFloat(packet.SpawnPlayer.ModelHeightFloat);
                    ReadAndUpdatePlayerPosition(packet.SpawnPlayer.PositionAndOrientation, playerid);
                    if (playerid == this.LocalPlayerId)
                    {
                        spawned = true;
                    }
                }
                break;
            case Packet_ServerIdEnum.PlayerPositionAndOrientation:
                {
                    int playerid = packet.PositionAndOrientation.PlayerId;
                    ReadAndUpdatePlayerPosition(packet.PositionAndOrientation.PositionAndOrientation, playerid);
                }
                break;
            case Packet_ServerIdEnum.Monster:
                {
                    if (packet.Monster.Monsters == null)
                    {
                        break;
                    }
                    for (int i = 0; i < packet.Monster.MonstersCount; i++)
                    {
                        Packet_ServerMonster k = packet.Monster.Monsters[i];
                        int id = k.Id + MonsterIdFirst;
                        if (entities[id] == null)
                        {
                            entities[id] = new Entity();
                            entities[id].player = new Player();
                            entities[id].player.Name = d_DataMonsters.MonsterName[k.MonsterType];
                        }
                        ReadAndUpdatePlayerPosition(k.PositionAndOrientation, id);
                        entities[id].player.Type = PlayerType.Monster;
                        entities[id].player.Health = k.Health;
                        entities[id].player.MonsterType = k.MonsterType;
                    }
                    //remove all old monsters that were not sent by server now.

                    //this causes monster flicker on chunk boundaries,
                    //commented out
                    //foreach (int id in new List<int>(players.Keys))
                    //{
                    //    if (id >= MonsterIdFirst)
                    //    {
                    //        if (!updatedMonsters.ContainsKey(id))
                    //        {
                    //            players.Remove(id);
                    //        }
                    //    }
                    //}
                }
                break;
            case Packet_ServerIdEnum.DespawnPlayer:
                {
                    int playerid = packet.DespawnPlayer.PlayerId;
                    for (int i = 0; i < this.ServerInfo.Players.count; i++)
                    {
                        ConnectedPlayer p = ServerInfo.Players.items[i];
                        if (p == null)
                        {
                            continue;
                        }
                        if (p.id == playerid)
                        {
                            this.ServerInfo.Players.RemoveAt(i);
                        }
                    }
                    entities[playerid] = null;
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
                    platform.MessageBoxShowError(packet.DisconnectPlayer.DisconnectReason, "Disconnected from server");
                    platform.Exit();
                    //Not needed anymore - avoids "cryptic" error messages on being kicked/banned
                    //throw new Exception(packet.DisconnectPlayer.DisconnectReason);
                    break;
                }
            case Packet_ServerIdEnum.ChunkPart:
                byte[] arr = packet.ChunkPart.CompressedChunkPart;
                int arrLength = platform.ByteArrayLength(arr); // todo
                for (int i = 0; i < arrLength; i++)
                {
                    CurrentChunk[CurrentChunkCount++] = arr[i];
                }
                break;
            case Packet_ServerIdEnum.Chunk_:
                {
                    Packet_ServerChunk p = packet.Chunk_;
                    int[] receivedchunk;
                    if (CurrentChunkCount != 0)
                    {
                        byte[] decompressedchunk = platform.GzipDecompress(CurrentChunk, CurrentChunkCount);
                        receivedchunk = new int[p.SizeX * p.SizeY * p.SizeZ];
                        {
                            int i = 0;
                            for (int zz = 0; zz < p.SizeZ; zz++)
                            {
                                for (int yy = 0; yy < p.SizeY; yy++)
                                {
                                    for (int xx = 0; xx < p.SizeX; xx++)
                                    {
                                        receivedchunk[MapUtilCi.Index3d(xx, yy, zz, p.SizeX, p.SizeY)] = (decompressedchunk[i + 1] << 8) + decompressedchunk[i];
                                        i += 2;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        receivedchunk = new int[p.SizeX * p.SizeY * p.SizeZ];
                    }
                    {
                        SetMapPortion(p.X, p.Y, p.Z, receivedchunk, p.SizeX, p.SizeY, p.SizeZ);
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
                    ReceivedMapLength += CurrentChunkCount;// lengthPrefixLength + packetLength;
                    CurrentChunkCount = 0;
                }
                break;
            case Packet_ServerIdEnum.HeightmapChunk:
                {
                    Packet_ServerHeightmapChunk p = packet.HeightmapChunk;
                    byte[] decompressedchunk = platform.GzipDecompress(p.CompressedHeightmap, platform.ByteArrayLength(p.CompressedHeightmap));
                    int[] decompressedchunk1 = ByteArrayToUshortArray(decompressedchunk, p.SizeX * p.SizeY * 2);
                    for (int xx = 0; xx < p.SizeX; xx++)
                    {
                        for (int yy = 0; yy < p.SizeY; yy++)
                        {
                            int height = decompressedchunk1[MapUtilCi.Index2d(xx, yy, p.SizeX)];
                            d_Heightmap.SetBlock(p.X + xx, p.Y + yy, height);
                        }
                    }
                }
                break;
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
                    ReceivedMapLength = 0; //todo
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

                    //if (ENABLE_PER_SERVER_TEXTURES || Options.UseServerTextures)
                    //{
                    //    if (blobdownloadhash == serverterraintexture)
                    //    {
                    //        using (Bitmap bmp = new Bitmap(new MemoryStream(downloaded)))
                    //        {
                    //            d_TerrainTextures.UseTerrainTextureAtlas2d(bmp);
                    //        }
                    //    }
                    //}

                    if (blobdownloadname != null) // old servers
                    {
                        SetFile(blobdownloadname, downloaded);
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
                    for (int i = MonsterIdFirst; i < MonsterIdFirst + 1000; i++)
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
            case Packet_ServerIdEnum.CraftingRecipes:
                d_CraftingRecipes = packet.CraftingRecipes.CraftingRecipes;
                break;
            case Packet_ServerIdEnum.Dialog:
                Packet_ServerDialog d = packet.Dialog;
                if (d.Dialog == null)
                {
                    if (GetDialogId(d.DialogId) != -1 && dialogs[GetDialogId(d.DialogId)].value.IsModal != 0)
                    {
                        GuiStateBackToGame();
                    }
                    dialogs[GetDialogId(d.DialogId)] = null;
                    if (DialogsCount() == 0)
                    {
                        SetFreeMouse(false);
                    }
                }
                else
                {
                    VisibleDialog d2 = new VisibleDialog();
                    d2.key = d.DialogId;
                    d2.value = d.Dialog;
                    if (GetDialogId(d.DialogId) == -1)
                    {
                        for (int i = 0; i < dialogsCount; i++)
                        {
                            if (dialogs[i] == null)
                            {
                                dialogs[i] = d2;
                                break;
                            }
                        }
                    }
                    else
                    {
                        dialogs[GetDialogId(d.DialogId)] = d2;
                    }
                    if (d.Dialog.IsModal != 0)
                    {
                        guistate = GuiState.ModalDialog;
                        SetFreeMouse(true);
                    }
                }
                break;
            case Packet_ServerIdEnum.Follow:
                IntRef oldFollowId = FollowId();
                Follow = packet.Follow.Client;
                if (packet.Follow.Tpp != 0)
                {
                    SetCamera(CameraType.Overhead);
                    player.playerorientation.X = Game.GetPi();
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
                        LoadedAmmo[k.Key_] = MinInt(k.Value_, blocktypes[k.Key_].AmmoMagazine);
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
                d_Weapon.redraw = true;
                RedrawAllBlocks();
                break;
        }
    }

    DictionaryStringByteArray files;
    void SetFile(string blobdownloadname, byte[] downloaded)
    {
        files.Set(blobdownloadname, downloaded);
    }

    bool ammostarted;
    internal Packet_CraftingRecipe[] d_CraftingRecipes;
    internal Packet_BlockType[] NewBlockTypes;
    internal bool ENABLE_PER_SERVER_TEXTURES;
    internal string blobdownloadname;
    internal CitoMemoryStream blobdownload;
    internal SunMoonRenderer d_SunMoonRenderer;
    internal int[] NightLevels;
    public const int HourDetail = 4;
    internal byte[] CurrentChunk;
    internal int CurrentChunkCount;
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
        return files.Get(p);
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
    {            //todo bigger than 32x32
        int tilesize = 32;
        BitmapData_ atlas2d = BitmapData_.Create(tilesize * atlas2dtiles(), tilesize * atlas2dtiles());

        for (int i = 0; i < textureIdsCount; i++)
        {
            if (textureIds[i] == null)
            {
                continue;
            }
            byte[] fileData = GetFile(StringTools.StringAppend(platform, textureIds[i], ".png"));
            BitmapCi bmp = platform.BitmapCreateFromPng(fileData, platform.ByteArrayLength(fileData));
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
            if (arr[i] == value)
            {
                return i;
            }
        }
        return -1;
    }

    bool Contains(string[] arr, int arrLength, string value)
    {
        return IndexOf(arr, arrLength, value) != -1;
    }

    internal void TryReadPacket(byte[] data, int dataLength)
    {
        Packet_Server packet = new Packet_Server();
        Packet_ServerSerializer.DeserializeBuffer(data, dataLength, packet);
        ProcessPacket(packet);
        LastReceivedMilliseconds = currentTimeMilliseconds;
        //return lengthPrefixLength + packetLength;
    }

    internal void NetworkProcess()
    {
        currentTimeMilliseconds = platform.TimeMillisecondsFromStart();
        if (main == null)
        {
            return;
        }
        INetIncomingMessage msg;
        for (; ; )
        {
            if (invalidVersionPacketIdentification != null)
            {
                break;
            }
            msg = main.ReadMessage();
            if (msg == null)
            {
                break;
            }
            TryReadPacket(msg.ReadBytes(msg.LengthBytes()), msg.LengthBytes());
        }
        if (spawned && ((platform.TimeMillisecondsFromStart() - lastpositionsentMilliseconds) > 100))
        {
            lastpositionsentMilliseconds = platform.TimeMillisecondsFromStart();
            SendPosition(player.playerposition.X, player.playerposition.Y, player.playerposition.Z,
                player.playerorientation.X, player.playerorientation.Y, player.playerorientation.Z);
        }
        int now = platform.TimeMillisecondsFromStart();
        for (int i = 0; i < entitiesCount; i++)
        {
            if (entities[i] == null)
            {
                continue;
            }
            if (entities[i].player == null)
            {
                continue;
            }
            int kKey = i;
            Player p = entities[i].player;
            if ((one * (now - p.LastUpdateMilliseconds) / 1000) > 2)
            {
                p.playerDrawInfo = null;
                p.PositionLoaded = false;
            }
        }
    }

    internal string skinserver;
    internal void LoadPlayerTextures()
    {
        for (int i = 0; i < entitiesCount; i++)
        {
            Entity e = entities[i];
            if (e == null) { continue; }
            if (e.player == null) { continue; }
            if (e.player.CurrentTexture != -1)
            {
                continue;
            }
            // a) download skin
            if (!issingleplayer && e.player.Type == PlayerType.Player)
            {
                if (e.player.SkinDownloadResponse == null)
                {
                    e.player.SkinDownloadResponse = new HttpResponseCi();
                    string url = StringTools.StringAppend(platform, skinserver, StringTools.StringSubstringToEnd(platform, e.player.Name, 2));
                    url = StringTools.StringAppend(platform, url, ".png");
                    platform.WebClientDownloadDataAsync(url, e.player.SkinDownloadResponse);
                    continue;
                }
                if (!e.player.SkinDownloadResponse.done)
                {
                    continue;
                }
                BitmapCi bmp_ = platform.BitmapCreateFromPng(e.player.SkinDownloadResponse.value, e.player.SkinDownloadResponse.valueLength);
                if (bmp_ != null)
                {
                    e.player.CurrentTexture = GetTextureOrLoad(e.player.Texture, bmp_);
                    platform.BitmapDelete(bmp_);
                    continue;
                }
            }
            // b) file skin
            if (e.player.Texture == null)
            {
                e.player.CurrentTexture = GetTexture("mineplayer.png");
                continue;
            }

            byte[] file = GetFile(e.player.Texture);
            if (file == null)
            {
                e.player.CurrentTexture = 0;
                continue;
            }
            BitmapCi bmp = platform.BitmapCreateFromPng(file, platform.ByteArrayLength(file));
            if (bmp == null)
            {
                e.player.CurrentTexture = 0;
                continue;
            }
            e.player.CurrentTexture = GetTextureOrLoad(e.player.Texture, bmp);
            platform.BitmapDelete(bmp);
        }
    }
}

class StringByteArray
{
    internal string name;
    internal byte[] data;
}

class DictionaryStringByteArray
{
    public DictionaryStringByteArray()
    {
        items = new StringByteArray[1024];
        itemsCount = 1024;
    }
    internal StringByteArray[] items;
    internal int itemsCount;

    internal void Set(string name, byte[] value)
    {
        for (int i = 0; i < itemsCount; i++)
        {
            if (items[i] == null) { continue; }
            if (items[i].name == name)
            {
                items[i].data = value;
                return;
            }
        }
        for (int i = 0; i < itemsCount; i++)
        {
            if (items[i] == null)
            {
                items[i] = new StringByteArray();
                items[i].name = name;
                items[i].data = value;
                return;
            }
        }
    }

    internal byte[] Get(string name)
    {
        for (int i = 0; i < itemsCount; i++)
        {
            if (items[i] == null) { continue; }
            if (items[i].name == name)
            {
                return items[i].data;
            }
        }
        return null;
    }
}

public class RenderHintEnum
{
    public const int Fast = 0;
    public const int Nice = 1;
}

public class Speculative
{
    internal int x;
    internal int y;
    internal int z;
    internal int timeMilliseconds;
    internal int blocktype;
}

public class TimerCi
{
    public TimerCi()
    {
        interval = 1;
        maxDeltaTime = -1;
    }
    internal float interval;
    internal float maxDeltaTime;

    internal float accumulator;
    public void Reset()
    {
        accumulator = 0;
    }
    public int Update(float dt)
    {
        accumulator += dt;
        float constDt = interval;
        if (maxDeltaTime != -1 && accumulator > maxDeltaTime)
        {
            accumulator = maxDeltaTime;
        }
        int updates = 0;
        while (accumulator >= constDt)
        {
            updates++;
            accumulator -= constDt;
        }
        return updates;
    }

    internal static TimerCi Create(int interval_, int maxDeltaTime_)
    {
        TimerCi timer = new TimerCi();
        timer.interval = interval_;
        timer.maxDeltaTime = maxDeltaTime_;
        return timer;
    }
}

public class GetBlockHeight_ : DelegateGetBlockHeight
{
    public static GetBlockHeight_ Create(Game w_)
    {
        GetBlockHeight_ g = new GetBlockHeight_();
        g.w = w_;
        return g;
    }
    internal Game w;
    public override float GetBlockHeight(int x, int y, int z)
    {
        return w.getblockheight(x, y, z);
    }
}

public class IsBlockEmpty_ : DelegateIsBlockEmpty
{
    public static IsBlockEmpty_ Create(Game w_)
    {
        IsBlockEmpty_ g = new IsBlockEmpty_();
        g.w = w_;
        return g;
    }
    Game w;
    public override bool IsBlockEmpty(int x, int y, int z)
    {
        return w.IsTileEmptyForPhysics(x, y, z);
    }
}

public class Sprite
{
    public Sprite()
    {
        size = 40;
    }
    internal float positionX;
    internal float positionY;
    internal float positionZ;
    internal string image;
    internal int size;
    internal int animationcount;
}

public class PlayerDrawInfo
{
    public PlayerDrawInfo()
    {
        anim = new AnimationState();
        interpolation = new NetworkInterpolation();
    }
    internal AnimationState anim;
    internal NetworkInterpolation interpolation;
    internal float lastrealposX;
    internal float lastrealposY;
    internal float lastrealposZ;
    internal float lastcurposX;
    internal float lastcurposY;
    internal float lastcurposZ;
    internal byte lastrealheading;
    internal byte lastrealpitch;
    internal float velocityX;
    internal float velocityY;
    internal float velocityZ;
}

public class PlayerInterpolate : IInterpolation
{
    internal GamePlatform platform;
    public override InterpolatedObject Interpolate(InterpolatedObject a, InterpolatedObject b, float progress)
    {
        PlayerInterpolationState aa = platform.CastToPlayerInterpolationState(a);
        PlayerInterpolationState bb = platform.CastToPlayerInterpolationState(b);
        PlayerInterpolationState cc = new PlayerInterpolationState();
        cc.positionX = aa.positionX + (bb.positionX - aa.positionX) * progress;
        cc.positionY = aa.positionY + (bb.positionY - aa.positionY) * progress;
        cc.positionZ = aa.positionZ + (bb.positionZ - aa.positionZ) * progress;
        cc.heading = Game.IntToByte(AngleInterpolation.InterpolateAngle256(platform, aa.heading, bb.heading, progress));
        cc.pitch = Game.IntToByte(AngleInterpolation.InterpolateAngle256(platform, aa.pitch, bb.pitch, progress));
        return cc;
    }
}

public class PlayerInterpolationState : InterpolatedObject
{
    internal float positionX;
    internal float positionY;
    internal float positionZ;
    internal byte heading;
    internal byte pitch;
}

public class Bullet_
{
    internal float fromX;
    internal float fromY;
    internal float fromZ;
    internal float toX;
    internal float toY;
    internal float toZ;
    internal float speed;
    internal float progress;
}

public class Expires
{
    internal static Expires Create(float p)
    {
        Expires expires = new Expires();
        expires.totalTime = p;
        expires.timeLeft = p;
        return expires;
    }

    internal float totalTime;
    internal float timeLeft;
}

public class DrawName
{
    internal float TextX;
    internal float TextY;
    internal float TextZ;
    internal string Name;
    internal bool DrawHealth;
    internal float Health;
}

public class Entity
{
    internal Expires expires;
    internal Packet_ServerExplosion push;
    internal Sprite sprite;
    internal Grenade_ grenade;
    internal Bullet_ bullet;
    internal Player player;
    internal DrawName drawName;
}

public class DictionaryVector3Float
{
    public DictionaryVector3Float()
    {
        itemsCount = 16 * 1024;
        items = new Vector3Float[itemsCount];
    }
    internal Vector3Float[] items;
    internal int itemsCount;
    internal bool ContainsKey(int x, int y, int z)
    {
        return ItemIndex(x, y, z) != -1;
    }

    int ItemIndex(int x, int y, int z)
    {
        for (int i = 0; i < itemsCount; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            Vector3Float item = items[i];
            if (item.x == x && item.y == y && item.z == z)
            {
                return i;
            }
        }
        return -1;
    }

    internal float Get(int x, int y, int z)
    {
        return items[ItemIndex(x, y, z)].value;
    }

    internal void Remove(int x, int y, int z)
    {
        if (ItemIndex(x, y, z) == -1)
        {
            return;
        }
        items[ItemIndex(x, y, z)] = null;
    }

    internal void Set(int x, int y, int z, float value)
    {
        int index = ItemIndex(x, y, z);
        if (index != -1)
        {
            items[index].value = value;
        }
        else
        {
            for (int i = 0; i < itemsCount; i++)
            {
                if (items[i] == null)
                {
                    Vector3Float item = new Vector3Float();
                    item.x = x;
                    item.y = y;
                    item.z = z;
                    item.value = value;
                    items[i] = item;
                    return;
                }
            }
        }
    }

    internal int Count()
    {
        int count = 0;
        for (int i = 0; i < itemsCount; i++)
        {
            if (items[i] != null)
            {
                count++;
            }
        }
        return count;
    }

    internal void Clear()
    {
        for (int i = 0; i < itemsCount; i++)
        {
            items[i] = null;
        }
    }
}

public class Vector3Float
{
    internal int x;
    internal int y;
    internal int z;
    internal float value;
}

public class VisibleDialog
{
    internal string key;
    internal Packet_Dialog value;
}

public class RailMapUtil
{
    internal Game game;
    public RailSlope GetRailSlope(int x, int y, int z)
    {
        int tiletype = game.GetBlock(x, y, z);
        int railDirectionFlags = game.blocktypes[tiletype].Rail;
        int blocknear;
        if (x < game.MapSizeX - 1)
        {
            blocknear = game.GetBlock(x + 1, y, z);
            if (railDirectionFlags == RailDirectionFlags.Horizontal &&
                 blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
            {
                return RailSlope.TwoRightRaised;
            }
        }
        if (x > 0)
        {
            blocknear = game.GetBlock(x - 1, y, z);
            if (railDirectionFlags == RailDirectionFlags.Horizontal &&
                 blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
            {
                return RailSlope.TwoLeftRaised;

            }
        }
        if (y > 0)
        {
            blocknear = game.GetBlock(x, y - 1, z);
            if (railDirectionFlags == RailDirectionFlags.Vertical &&
                  blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
            {
                return RailSlope.TwoUpRaised;
            }
        }
        if (y < game.MapSizeY - 1)
        {
            blocknear = game.GetBlock(x, y + 1, z);
            if (railDirectionFlags == RailDirectionFlags.Vertical &&
                  blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
            {
                return RailSlope.TwoDownRaised;
            }
        }
        return RailSlope.Flat;
    }
}

public class RailDirectionFlags
{
    public const int None = 0;
    public const int Horizontal = 1;
    public const int Vertical = 2;
    public const int UpLeft = 4;
    public const int UpRight = 8;
    public const int DownLeft = 16;
    public const int DownRight = 32;

    public const int Full = Horizontal | Vertical | UpLeft | UpRight | DownLeft | DownRight;
    public const int TwoHorizontalVertical = Horizontal | Vertical;
    public const int Corners = UpLeft | UpRight | DownLeft | DownRight;
}

public enum RailSlope
{
    Flat, TwoLeftRaised, TwoRightRaised, TwoUpRaised, TwoDownRaised
}

public enum RailDirection
{
    Horizontal,
    Vertical,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

public enum TileExitDirection
{
    Up,
    Down,
    Left,
    Right
}

public enum TileEnterDirection
{
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Each RailDirection on tile can be traversed by train in two directions.
/// </summary>
/// <example>
/// RailDirection.Horizontal -> VehicleDirection12.HorizontalLeft (vehicle goes left and decreases x position),
/// and VehicleDirection12.HorizontalRight (vehicle goes right and increases x position).
/// </example>
public enum VehicleDirection12
{
    HorizontalLeft,
    HorizontalRight,
    VerticalUp,
    VerticalDown,

    UpLeftUp,
    UpLeftLeft,
    UpRightUp,
    UpRightRight,

    DownLeftDown,
    DownLeftLeft,
    DownRightDown,
    DownRightRight
}

public class VehicleDirection12Flags
{
    public const int None = 0;
    public const int HorizontalLeft = 1 << 0;
    public const int HorizontalRight = 1 << 1;
    public const int VerticalUp = 1 << 2;
    public const int VerticalDown = 1 << 3;

    public const int UpLeftUp = 1 << 4;
    public const int UpLeftLeft = 1 << 5;
    public const int UpRightUp = 1 << 6;
    public const int UpRightRight = 1 << 7;

    public const int DownLeftDown = 1 << 8;
    public const int DownLeftLeft = 1 << 9;
    public const int DownRightDown = 1 << 10;
    public const int DownRightRight = 1 << 11;
}

public class DirectionUtils
{
    /// <summary>
    /// VehicleDirection12.UpRightRight -> returns Direction4.Right
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static TileExitDirection ResultExit(VehicleDirection12 direction)
    {
        switch (direction)
        {
            case VehicleDirection12.HorizontalLeft:
                return TileExitDirection.Left;
            case VehicleDirection12.HorizontalRight:
                return TileExitDirection.Right;
            case VehicleDirection12.VerticalUp:
                return TileExitDirection.Up;
            case VehicleDirection12.VerticalDown:
                return TileExitDirection.Down;

            case VehicleDirection12.UpLeftUp:
                return TileExitDirection.Up;
            case VehicleDirection12.UpLeftLeft:
                return TileExitDirection.Left;
            case VehicleDirection12.UpRightUp:
                return TileExitDirection.Up;
            case VehicleDirection12.UpRightRight:
                return TileExitDirection.Right;

            case VehicleDirection12.DownLeftDown:
                return TileExitDirection.Down;
            case VehicleDirection12.DownLeftLeft:
                return TileExitDirection.Left;
            case VehicleDirection12.DownRightDown:
                return TileExitDirection.Down;
            case VehicleDirection12.DownRightRight:
                return TileExitDirection.Right;
            default:
                return TileExitDirection.Down;
        }
    }

    public static RailDirection ToRailDirection(VehicleDirection12 direction)
    {
        switch (direction)
        {
            case VehicleDirection12.HorizontalLeft:
                return RailDirection.Horizontal;
            case VehicleDirection12.HorizontalRight:
                return RailDirection.Horizontal;
            case VehicleDirection12.VerticalUp:
                return RailDirection.Vertical;
            case VehicleDirection12.VerticalDown:
                return RailDirection.Vertical;

            case VehicleDirection12.UpLeftUp:
                return RailDirection.UpLeft;
            case VehicleDirection12.UpLeftLeft:
                return RailDirection.UpLeft;
            case VehicleDirection12.UpRightUp:
                return RailDirection.UpRight;
            case VehicleDirection12.UpRightRight:
                return RailDirection.UpRight;

            case VehicleDirection12.DownLeftDown:
                return RailDirection.DownLeft;
            case VehicleDirection12.DownLeftLeft:
                return RailDirection.DownLeft;
            case VehicleDirection12.DownRightDown:
                return RailDirection.DownRight;
            case VehicleDirection12.DownRightRight:
                return RailDirection.DownRight;
            default:
                return RailDirection.DownLeft;
        }
    }

    public static int ToRailDirectionFlags(RailDirection direction)
    {
        switch (direction)
        {
            case RailDirection.DownLeft:
                return RailDirectionFlags.DownLeft;
            case RailDirection.DownRight:
                return RailDirectionFlags.DownRight;
            case RailDirection.Horizontal:
                return RailDirectionFlags.Horizontal;
            case RailDirection.UpLeft:
                return RailDirectionFlags.UpLeft;
            case RailDirection.UpRight:
                return RailDirectionFlags.UpRight;
            case RailDirection.Vertical:
                return RailDirectionFlags.Vertical;
            default:
                return 0;
        }
    }

    public static VehicleDirection12 Reverse(VehicleDirection12 direction)
    {
        switch (direction)
        {
            case VehicleDirection12.HorizontalLeft:
                return VehicleDirection12.HorizontalRight;
            case VehicleDirection12.HorizontalRight:
                return VehicleDirection12.HorizontalLeft;
            case VehicleDirection12.VerticalUp:
                return VehicleDirection12.VerticalDown;
            case VehicleDirection12.VerticalDown:
                return VehicleDirection12.VerticalUp;

            case VehicleDirection12.UpLeftUp:
                return VehicleDirection12.UpLeftLeft;
            case VehicleDirection12.UpLeftLeft:
                return VehicleDirection12.UpLeftUp;
            case VehicleDirection12.UpRightUp:
                return VehicleDirection12.UpRightRight;
            case VehicleDirection12.UpRightRight:
                return VehicleDirection12.UpRightUp;

            case VehicleDirection12.DownLeftDown:
                return VehicleDirection12.DownLeftLeft;
            case VehicleDirection12.DownLeftLeft:
                return VehicleDirection12.DownLeftDown;
            case VehicleDirection12.DownRightDown:
                return VehicleDirection12.DownRightRight;
            case VehicleDirection12.DownRightRight:
                return VehicleDirection12.DownRightDown;
            default:
                return VehicleDirection12.DownLeftDown;
        }
    }

    public static int ToVehicleDirection12Flags(VehicleDirection12 direction)
    {
        switch (direction)
        {
            case VehicleDirection12.HorizontalLeft:
                return VehicleDirection12Flags.HorizontalLeft;
            case VehicleDirection12.HorizontalRight:
                return VehicleDirection12Flags.HorizontalRight;
            case VehicleDirection12.VerticalUp:
                return VehicleDirection12Flags.VerticalUp;
            case VehicleDirection12.VerticalDown:
                return VehicleDirection12Flags.VerticalDown;

            case VehicleDirection12.UpLeftUp:
                return VehicleDirection12Flags.UpLeftUp;
            case VehicleDirection12.UpLeftLeft:
                return VehicleDirection12Flags.UpLeftLeft;
            case VehicleDirection12.UpRightUp:
                return VehicleDirection12Flags.UpRightUp;
            case VehicleDirection12.UpRightRight:
                return VehicleDirection12Flags.UpRightRight;

            case VehicleDirection12.DownLeftDown:
                return VehicleDirection12Flags.DownLeftDown;
            case VehicleDirection12.DownLeftLeft:
                return VehicleDirection12Flags.DownLeftLeft;
            case VehicleDirection12.DownRightDown:
                return VehicleDirection12Flags.DownRightDown;
            case VehicleDirection12.DownRightRight:
                return VehicleDirection12Flags.DownRightRight;
            default:
                return 0;
        }
    }

    public static TileEnterDirection ResultEnter(TileExitDirection direction)
    {
        switch (direction)
        {
            case TileExitDirection.Up:
                return TileEnterDirection.Down;
            case TileExitDirection.Down:
                return TileEnterDirection.Up;
            case TileExitDirection.Left:
                return TileEnterDirection.Right;
            case TileExitDirection.Right:
                return TileEnterDirection.Left;
            default:
                return TileEnterDirection.Down;
        }
    }
    public static int RailDirectionFlagsCount(int railDirectionFlags)
    {
        int count = 0;
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.DownLeft)) != 0) { count++; }
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.DownRight)) != 0) { count++; }
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.Horizontal)) != 0) { count++; }
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.UpLeft)) != 0) { count++; }
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.UpRight)) != 0) { count++; }
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.Vertical)) != 0) { count++; }
        return count;
    }

    public static int ToVehicleDirection12Flags_(VehicleDirection12[] directions, int directionsCount)
    {
        int flags = VehicleDirection12Flags.None;
        for (int i = 0; i < directionsCount; i++)
        {
            VehicleDirection12 d = directions[i];
            flags = flags | DirectionUtils.ToVehicleDirection12Flags(d);
        }
        return flags;
    }

    /// <summary>
    /// Enter at TileEnterDirection.Left -> yields VehicleDirection12.UpLeftUp,
    /// VehicleDirection12.HorizontalRight,
    /// VehicleDirection12.DownLeftDown
    /// </summary>
    /// <param name="enter_at"></param>
    /// <returns></returns>
    public static VehicleDirection12[] PossibleNewRails3(TileEnterDirection enter_at)
    {
        VehicleDirection12[] ret = new VehicleDirection12[3];
        switch (enter_at)
        {
            case TileEnterDirection.Left:
                ret[0] = VehicleDirection12.UpLeftUp;
                ret[1] = VehicleDirection12.HorizontalRight;
                ret[2] = VehicleDirection12.DownLeftDown;
                break;
            case TileEnterDirection.Down:
                ret[0] = VehicleDirection12.DownLeftLeft;
                ret[1] = VehicleDirection12.VerticalUp;
                ret[2] = VehicleDirection12.DownRightRight;
                break;
            case TileEnterDirection.Up:
                ret[0] = VehicleDirection12.UpLeftLeft;
                ret[1] = VehicleDirection12.VerticalDown;
                ret[2] = VehicleDirection12.UpRightRight;
                break;
            case TileEnterDirection.Right:
                ret[0] = VehicleDirection12.UpRightUp;
                ret[1] = VehicleDirection12.HorizontalLeft;
                ret[2] = VehicleDirection12.DownRightDown;
                break;
            default:
                return null;
        }
        return ret;
    }
}

public class ClientInventoryController : IInventoryController
{
    public static ClientInventoryController Create(Game game)
    {
        ClientInventoryController c = new ClientInventoryController();
        c.g = game;
        return c;
    }

    Game g;

    public override void InventoryClick(Packet_InventoryPosition pos)
    {
        g.InventoryClick(pos);
    }

    public override void WearItem(Packet_InventoryPosition from, Packet_InventoryPosition to)
    {
        g.WearItem(from, to);
    }

    public override void MoveToInventory(Packet_InventoryPosition from)
    {
        g.MoveToInventory(from);
    }
}

public enum CameraType
{
    Fpp,
    Tpp,
    Overhead
}

public enum TypingState
{
    None,
    Typing,
    Ready
}

public class Player
{
    public Player()
    {
        float one = 1;
        AnimationHint_ = new AnimationHint();
        Model = "player.txt";
        EyeHeight = one * 15 / 10;
        ModelHeight = one * 17 / 10;
        CurrentTexture = -1;
    }
    internal bool PositionLoaded;
    internal float PositionX;
    internal float PositionY;
    internal float PositionZ;
    internal byte Heading;
    internal byte Pitch;
    internal string Name;
    internal AnimationHint AnimationHint_;
    internal PlayerType Type;
    internal int MonsterType;
    internal int Health;
    internal int LastUpdateMilliseconds;
    internal string Model;
    internal string Texture;
    internal float EyeHeight;
    internal float ModelHeight;
    internal float NetworkX;
    internal float NetworkY;
    internal float NetworkZ;
    internal byte NetworkHeading;
    internal byte NetworkPitch;
    internal PlayerDrawInfo playerDrawInfo;
    internal bool moves;
    internal int CurrentTexture;
    internal HttpResponseCi SkinDownloadResponse;
}

public enum PlayerType
{
    Player,
    Monster
}

public class Grenade_
{
    internal float velocityX;
    internal float velocityY;
    internal float velocityZ;
    internal int block;
    internal int sourcePlayer;
}

public class GetCameraMatrix : IGetCameraMatrix
{
    internal float[] lastmvmatrix;
    internal float[] lastpmatrix;
    public override float[] GetModelViewMatrix()
    {
        return lastmvmatrix;
    }

    public override float[] GetProjectionMatrix()
    {
        return lastpmatrix;
    }
}

public class MenuState
{
    internal int selected;
}

public enum EscapeMenuState
{
    Main,
    Options,
    Graphics,
    Keys,
    Other
}

public class MapLoadingProgressEventArgs
{
    internal int ProgressPercent;
    internal int ProgressBytes;
    internal string ProgressStatus;
}

public class Draw2dData
{
    internal float x1;
    internal float y1;
    internal float width;
    internal float height;
    internal IntRef inAtlasId;
    internal int color;
}

public class Chunk
{
    internal byte[] data;
    internal int[] dataInt;
    internal int LastUpdate;
    internal bool IsPopulated;
    internal int LastChange;
    internal RenderedChunk rendered;
}

public class RenderedChunk
{
    public RenderedChunk()
    {
        dirty = true;
        shadowsdirty = true;
    }
    internal int[] ids;
    internal int idsCount;
    internal bool dirty;
    internal bool shadowsdirty;
    internal byte[] light;
}

public class ITerrainTextures
{
    internal Game game;

    public int texturesPacked() { return game.texturesPacked(); }
    public int terrainTexture() { return game.terrainTexture; }
    public int[] terrainTextures1d() { return game.terrainTextures1d; }
    public int terrainTexturesPerAtlas() { return game.terrainTexturesPerAtlas; }
}

public class Config3d
{
    public Config3d()
    {
        ENABLE_BACKFACECULLING = true;
        ENABLE_TRANSPARENCY = true;
        ENABLE_MIPMAPS = true;
        ENABLE_VSYNC = false;
        ENABLE_VISIBILITY_CULLING = false;
        viewdistance = 128;
    }
    internal bool ENABLE_BACKFACECULLING;
    internal bool ENABLE_TRANSPARENCY;
    internal bool ENABLE_MIPMAPS;
    internal bool ENABLE_VSYNC;
    internal bool ENABLE_VISIBILITY_CULLING;
    internal float viewdistance;
    public float GetViewDistance() { return viewdistance; }
    public void SetViewDistance(float value) { viewdistance = value; }
}

public class MapUtilCi
{
    public static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }

    public static int Index2d(int x, int y, int sizex)
    {
        return x + y * sizex;
    }

    public static void Pos(int index, int sizex, int sizey, Vector3Ref ret)
    {
        int x = index % sizex;
        int y = (index / sizex) % sizey;
        int h = index / (sizex * sizey);
        ret.X = x;
        ret.Y = y;
        ret.Z = h;
    }

    internal static void PosInt(int index, int sizex, int sizey, Vector3IntRef ret)
    {
        int x = index % sizex;
        int y = (index / sizex) % sizey;
        int h = index / (sizex * sizey);
        ret.X = x;
        ret.Y = y;
        ret.Z = h;
    }
}

public class InfiniteMapChunked2d
{
    internal Game d_Map;
    public const int chunksize = 16;
    internal int[][] chunks;
    public int GetBlock(int x, int y)
    {
        int[] chunk = GetChunk(x, y);
        return chunk[MapUtilCi.Index2d(x % chunksize, y % chunksize, chunksize)];
    }
    public int[] GetChunk(int x, int y)
    {
        int[] chunk = null;
        int kx = x / chunksize;
        int ky = y / chunksize;
        if (chunks[MapUtilCi.Index2d(kx, ky, d_Map.MapSizeX / chunksize)] == null)
        {
            chunk = new int[chunksize * chunksize];// (byte*)Marshal.AllocHGlobal(chunksize * chunksize);
            for (int i = 0; i < chunksize * chunksize; i++)
            {
                chunk[i] = 0;
            }
            chunks[MapUtilCi.Index2d(kx, ky, d_Map.MapSizeX / chunksize)] = chunk;
        }
        chunk = chunks[MapUtilCi.Index2d(kx, ky, d_Map.MapSizeX / chunksize)];
        return chunk;
    }
    public void SetBlock(int x, int y, int blocktype)
    {
        GetChunk(x, y)[MapUtilCi.Index2d(x % chunksize, y % chunksize, chunksize)] = blocktype;
    }
    public void Restart()
    {
        //chunks = new byte[d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize][,];
        int n = (d_Map.MapSizeX / chunksize) * (d_Map.MapSizeY / chunksize);
        chunks = new int[n][];//(byte**)Marshal.AllocHGlobal(n * sizeof(IntPtr));
        for (int i = 0; i < n; i++)
        {
            chunks[i] = null;
        }
    }
    public void ClearChunk(int x, int y)
    {
        int px = x / chunksize;
        int py = y / chunksize;
        chunks[MapUtilCi.Index2d(px, py, d_Map.MapSizeX / chunksize)] = null;
    }
}

public abstract class ClientModManager
{
    public abstract void MakeScreenshot();
    public abstract void SetLocalPosition(float glx, float gly, float glz);
    public abstract float GetLocalPositionX();
    public abstract float GetLocalPositionY();
    public abstract float GetLocalPositionZ();
    public abstract void SetLocalOrientation(float glx, float gly, float glz);
    public abstract float GetLocalOrientationX();
    public abstract float GetLocalOrientationY();
    public abstract float GetLocalOrientationZ();
    public abstract void DisplayNotification(string message);
    public abstract void SendChatMessage(string message);
    public abstract GamePlatform GetPlatform();
    public abstract void ShowGui(int level);
    public abstract void SetFreemove(int level);
    public abstract int GetFreemove();
    public abstract BitmapCi GrabScreenshot();
    public abstract AviWriterCi AviWriterCreate();
    public abstract int GetWindowWidth();
    public abstract int GetWindowHeight();
    public abstract bool IsFreemoveAllowed();
    public abstract void EnableCameraControl(bool enable);
    public abstract int WhiteTexture();
    public abstract void Draw2dTexture(int textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int color);
    public abstract void Draw2dTextures(Draw2dData[] todraw, int todrawLength, int textureId);
    public abstract void Draw2dText(string text, float x, float y, float fontsize);
    public abstract void OrthoMode();
    public abstract void PerspectiveMode();
    public abstract DictionaryStringString GetPerformanceInfo();
}

public class ClientModManager1 : ClientModManager
{
    internal Game game;

    public override void MakeScreenshot()
    {
        game.platform.SaveScreenshot();
    }

    public override void SetLocalPosition(float glx, float gly, float glz)
    {
        game.player.playerposition.X = glx;
        game.player.playerposition.Y = gly;
        game.player.playerposition.Z = glz;
    }

    public override float GetLocalPositionX()
    {
        return game.player.playerposition.X;
    }

    public override float GetLocalPositionY()
    {
        return game.player.playerposition.Y;
    }

    public override float GetLocalPositionZ()
    {
        return game.player.playerposition.Z;
    }

    public override void SetLocalOrientation(float glx, float gly, float glz)
    {
        game.player.playerorientation.X = glx;
        game.player.playerorientation.Y = gly;
        game.player.playerorientation.Z = glz;
    }

    public override float GetLocalOrientationX()
    {
        return game.player.playerorientation.X;
    }

    public override float GetLocalOrientationY()
    {
        return game.player.playerorientation.Y;
    }

    public override float GetLocalOrientationZ()
    {
        return game.player.playerorientation.Z;
    }

    public override void DisplayNotification(string message)
    {
        game.AddChatline(message);
    }

    public override void SendChatMessage(string message)
    {
        game.SendChat(message);
    }

    public override GamePlatform GetPlatform()
    {
        return game.platform;
    }

    public override void ShowGui(int level)
    {
        if (level == 0)
        {
            game.ENABLE_DRAW2D = false;
        }
        else
        {
            game.ENABLE_DRAW2D = true;
        }
    }

    public override void SetFreemove(int level)
    {
        if (level == FreemoveLevelEnum.None)
        {
            game.ENABLE_FREEMOVE = false;
            game.ENABLE_NOCLIP = false;
        }

        if (level == FreemoveLevelEnum.Freemove)
        {
            game.ENABLE_FREEMOVE = true;
            game.ENABLE_NOCLIP = false;
        }

        if (level == FreemoveLevelEnum.Noclip)
        {
            game.ENABLE_FREEMOVE = true;
            game.ENABLE_NOCLIP = true;
        }
    }

    public override int GetFreemove()
    {
        if (!game.ENABLE_FREEMOVE)
        {
            return FreemoveLevelEnum.None;
        }
        if (game.ENABLE_NOCLIP)
        {
            return FreemoveLevelEnum.Noclip;
        }
        else
        {
            return FreemoveLevelEnum.Freemove;
        }
    }

    public override BitmapCi GrabScreenshot()
    {
        return game.platform.GrabScreenshot();
    }

    public override AviWriterCi AviWriterCreate()
    {
        return game.platform.AviWriterCreate();
    }

    public override int GetWindowWidth()
    {
        return game.platform.GetCanvasWidth();
    }

    public override int GetWindowHeight()
    {
        return game.platform.GetCanvasHeight();
    }

    public override bool IsFreemoveAllowed()
    {
        return game.AllowFreemove;
    }

    public override void EnableCameraControl(bool enable)
    {
        game.enableCameraControl = enable;
    }

    public override int WhiteTexture()
    {
        return game.WhiteTexture();
    }

    public override void Draw2dTexture(int textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int color)
    {
        int a = Game.ColorA(color);
        int r = Game.ColorR(color);
        int g = Game.ColorG(color);
        int b = Game.ColorB(color);
        game.Draw2dTexture(textureid, game.platform.FloatToInt(x1), game.platform.FloatToInt(y1),
            game.platform.FloatToInt(width), game.platform.FloatToInt(height),
            inAtlasId, 0, Game.ColorFromArgb(a, r, g, b), false);
    }

    public override void Draw2dTextures(Draw2dData[] todraw, int todrawLength, int textureId)
    {
        game.Draw2dTextures(todraw, todrawLength, textureId, 0);
    }


    public override void Draw2dText(string text, float x, float y, float fontsize)
    {
        FontCi font = new FontCi();
        font.family = "Arial";
        font.size = fontsize;
        game.Draw2dText(text, font, x, y, null, false);
    }

    public override void OrthoMode()
    {
        game.OrthoMode(GetWindowWidth(), GetWindowHeight());
    }

    public override void PerspectiveMode()
    {
        game.PerspectiveMode();
    }

    public override DictionaryStringString GetPerformanceInfo()
    {
        return game.performanceinfo;
    }
}

public abstract class AviWriterCi
{
    public abstract void Open(string filename, int framerate, int width, int height);
    public abstract void AddFrame(BitmapCi bitmap);
    public abstract void Close();
}

public class BitmapCi
{
    public virtual void Dispose(){}
}

public class FreemoveLevelEnum
{
    public const int None = 0;
    public const int Freemove = 1;
    public const int Noclip = 2;
}

public abstract class ClientMod
{
    public abstract void Start(ClientModManager modmanager);
    public virtual bool OnClientCommand(ClientCommandArgs args) { return false; }
    public virtual void OnNewFrame(NewFrameEventArgs args) { }
    public virtual void OnKeyDown(KeyEventArgs args) { }
    public virtual void OnKeyUp(KeyEventArgs args) { }
}

public class ClientCommandArgs
{
    internal string command;
    internal string arguments;
}

public class TextureAtlasCi
{
    public static void TextureCoords2d(int textureId, int texturesPacked, RectFRef r)
    {
        float one = 1;
        r.y = (one / texturesPacked * (textureId / texturesPacked));
        r.x = (one / texturesPacked * (textureId % texturesPacked));
        r.w = one / texturesPacked;
        r.h = one / texturesPacked;
    }
}

public class StackFloatArray
{
    public StackFloatArray()
    {
        values = new float[max][];
    }
    float[][] values;
    const int max = 1024;
    int count;

    internal void Push(float[] p)
    {
        values[count++] = p;
    }

    internal float[] Peek()
    {
        return values[count - 1];
    }

    internal int Count()
    {
        return count;
    }

    internal float[] Pop()
    {
        float[] ret = values[count - 1];
        count--;
        return ret;
    }
}

public class CachedTexture
{
    internal int textureId;
    internal float sizeX;
    internal float sizeY;
    internal int lastuseMilliseconds;
}

public class Text_
{
    internal string text;
    internal float fontsize;
    internal int color;
    internal string fontfamily;
    internal int fontstyle;

    internal bool Equals_(Text_ t)
    {
        return this.text == t.text
            && this.fontsize == t.fontsize
            && this.color == t.color
            && this.fontfamily == t.fontfamily
            && this.fontstyle == t.fontstyle;
    }
}

public class CachedTextTexture
{
    internal Text_ text;
    internal CachedTexture texture;
}

public class FontCi
{
    internal string family;
    internal float size;
    internal int style;

    internal static FontCi Create(string family_, float size_, int style_)
    {
        FontCi f = new FontCi();
        f.family = family_;
        f.size = size_;
        f.style = style_;
        return f;
    }
}

public class CameraMove
{
    internal bool TurnLeft;
    internal bool TurnRight;
    internal bool DistanceUp;
    internal bool DistanceDown;
    internal bool AngleUp;
    internal bool AngleDown;
    internal int MoveX;
    internal int MoveY;
    internal float Distance;
}

public class Kamera
{
    public Kamera()
    {
        one = 1;
        distance = 5;
        Angle = 45;
        MinimumDistance = 2;
        tt = 0;
        MaximumAngle = 89;
        MinimumAngle = 0;
        Center = new Vector3Ref();
    }
    float one;
    public void GetPosition(GamePlatform platform, Vector3Ref ret)
    {
        float cx = platform.MathCos(tt * one / 2) * GetFlatDistance(platform) + Center.X;
        float cy = platform.MathSin(tt * one / 2) * GetFlatDistance(platform) + Center.Z;
        ret.X = cx;
        ret.Y = Center.Y + GetCameraHeightFromCenter(platform);
        ret.Z = cy;
    }
    float distance;
    public float GetDistance() { return distance; }
    public void SetDistance(float value)
    {
        distance = value;
        if (distance < MinimumDistance)
        {
            distance = MinimumDistance;
        }
    }
    internal float Angle;
    internal float MinimumDistance;
    float GetCameraHeightFromCenter(GamePlatform platform)
    {
        return platform.MathSin(Angle * Game.GetPi() / 180) * distance;
    }
    float GetFlatDistance(GamePlatform platform)
    {
        return platform.MathCos(Angle * Game.GetPi() / 180) * distance;
    }
    internal Vector3Ref Center;
    internal float tt;
    public float GetT()
    {
        return tt;
    }
    public void SetT(float value)
    {
        tt = value;
    }
    public void TurnLeft(float p)
    {
        tt += p;
    }
    public void TurnRight(float p)
    {
        tt -= p;
    }
    public void Move(CameraMove camera_move, float p)
    {
        p *= 2;
        p *= 2;
        if (camera_move.TurnLeft)
        {
            TurnLeft(p);
        }
        if (camera_move.TurnRight)
        {
            TurnRight(p);
        }
        if (camera_move.DistanceUp)
        {
            SetDistance(GetDistance() + p);
        }
        if (camera_move.DistanceDown)
        {
            SetDistance(GetDistance() - p);
        }
        if (camera_move.AngleUp)
        {
            Angle += p * 10;
        }
        if (camera_move.AngleDown)
        {
            Angle -= p * 10;
        }
        SetDistance(camera_move.Distance);
        //if (MaximumAngle < MinimumAngle) { throw new Exception(); }
        if (Angle > MaximumAngle) { Angle = MaximumAngle; }
        if (Angle < MinimumAngle) { Angle = MinimumAngle; }
    }
    internal int MaximumAngle;
    internal int MinimumAngle;

    public float GetAngle()
    {
        return Angle;
    }

    public void SetAngle(float value)
    {
        Angle = value;
    }

    public void GetCenter(Vector3Ref ret)
    {
        ret.X = Center.X;
        ret.Y = Center.Y;
        ret.Z = Center.Z;
    }
}

public class GameDataMonsters
{
    public GameDataMonsters()
    {
        int n = 5;
        MonsterCode = new string[n];
        MonsterName = new string[n];
        MonsterSkin = new string[n];
        MonsterCode[0] = "imp.txt";
        MonsterName[0] = "Imp";
        MonsterSkin[0] = "imp.png";
        MonsterCode[1] = "imp.txt";
        MonsterName[1] = "Fire Imp";
        MonsterSkin[1] = "impfire.png";
        MonsterCode[2] = "dragon.txt";
        MonsterName[2] = "Dragon";
        MonsterSkin[2] = "dragon.png";
        MonsterCode[3] = "zombie.txt";
        MonsterName[3] = "Zombie";
        MonsterSkin[3] = "zombie.png";
        MonsterCode[4] = "cyclops.txt";
        MonsterName[4] = "Cyclops";
        MonsterSkin[4] = "cyclops.png";
    }
    internal string[] MonsterName;
    internal string[] MonsterCode;
    internal string[] MonsterSkin;
}

public enum GuiState
{
    Normal,
    EscapeMenu,
    Inventory,
    MapLoading,
    CraftingRecipes,
    ModalDialog
}

public enum BlockSetMode
{
    Destroy,
    Create,
    Use, //open doors, use crafting table, etc.
    UseWithTool
}

public enum FontType
{
    Nice,
    Simple,
    BlackBackground,
    Default
}

public class SpecialBlockId
{
    public const int Empty = 0;
}

public class GameData
{
    public GameData()
    {
        mBlockIdEmpty = 0;
        mBlockIdDirt = -1;
        mBlockIdSponge = -1;
        mBlockIdTrampoline = -1;
        mBlockIdAdminium = -1;
        mBlockIdCompass = -1;
        mBlockIdLadder = -1;
        mBlockIdEmptyHand = -1;
        mBlockIdCraftingTable = -1;
        mBlockIdLava = -1;
        mBlockIdStationaryLava = -1;
        mBlockIdFillStart = -1;
        mBlockIdCuboid = -1;
        mBlockIdFillArea = -1;
        mBlockIdMinecart = -1;
        mBlockIdRailstart = -128; // 64 rail tiles
    }
    public void Start()
    {
        Initialize(GlobalVar.MAX_BLOCKTYPES);
    }
    public void Update()
    {
    }
    void Initialize(int count)
    {
        mWhenPlayerPlacesGetsConvertedTo = new int[count];
        mIsFlower = new bool[count];
        mRail = new int[count];
        mWalkSpeed = new float[count];
        for (int i = 0; i < count; i++)
        {
            mWalkSpeed[i] = 1;
        }
        mIsSlipperyWalk = new bool[count];
        mWalkSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mWalkSound[i] = new string[SoundCount];
        }
        mBreakSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mBreakSound[i] = new string[SoundCount];
        }
        mBuildSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mBuildSound[i] = new string[SoundCount];
        }
        mCloneSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mCloneSound[i] = new string[SoundCount];
        }
        mLightRadius = new int[count];
        mStartInventoryAmount = new int[count];
        mStrength = new float[count];
        mDamageToPlayer = new int[count];
        mWalkableType = new int[count];

        mDefaultMaterialSlots = new int[10];
    }

    public int[] WhenPlayerPlacesGetsConvertedTo() { return mWhenPlayerPlacesGetsConvertedTo; }
    public bool[] IsFlower() { return mIsFlower; }
    public int[] Rail() { return mRail; }
    public float[] WalkSpeed() { return mWalkSpeed; }
    public bool[] IsSlipperyWalk() { return mIsSlipperyWalk; }
    public string[][] WalkSound() { return mWalkSound; }
    public string[][] BreakSound() { return mBreakSound; }
    public string[][] BuildSound() { return mBuildSound; }
    public string[][] CloneSound() { return mCloneSound; }
    public int[] LightRadius() { return mLightRadius; }
    public int[] StartInventoryAmount() { return mStartInventoryAmount; }
    public float[] Strength() { return mStrength; }
    public int[] DamageToPlayer() { return mDamageToPlayer; }
    public int[] WalkableType1() { return mWalkableType; }

    public int[] DefaultMaterialSlots() { return mDefaultMaterialSlots; }

    int[] mWhenPlayerPlacesGetsConvertedTo;
    bool[] mIsFlower;
    int[] mRail;
    float[] mWalkSpeed;
    bool[] mIsSlipperyWalk;
    string[][] mWalkSound;
    string[][] mBreakSound;
    string[][] mBuildSound;
    string[][] mCloneSound;
    int[] mLightRadius;
    int[] mStartInventoryAmount;
    float[] mStrength;
    int[] mDamageToPlayer;
    int[] mWalkableType;

    int[] mDefaultMaterialSlots;

    // TODO: hardcoded IDs
    // few code sections still expect some hardcoded IDs
    int mBlockIdEmpty;
    int mBlockIdDirt;
    int mBlockIdSponge;
    int mBlockIdTrampoline;
    int mBlockIdAdminium;
    int mBlockIdCompass;
    int mBlockIdLadder;
    int mBlockIdEmptyHand;
    int mBlockIdCraftingTable;
    int mBlockIdLava;
    int mBlockIdStationaryLava;
    int mBlockIdFillStart;
    int mBlockIdCuboid;
    int mBlockIdFillArea;
    int mBlockIdMinecart;
    int mBlockIdRailstart; // 64 rail tiles

    public int BlockIdEmpty() { return mBlockIdEmpty; }
    public int BlockIdDirt() { return mBlockIdDirt; }
    public int BlockIdSponge() { return mBlockIdSponge; }
    public int BlockIdTrampoline() { return mBlockIdTrampoline; }
    public int BlockIdAdminium() { return mBlockIdAdminium; }
    public int BlockIdCompass() { return mBlockIdCompass; }
    public int BlockIdLadder() { return mBlockIdLadder; }
    public int BlockIdEmptyHand() { return mBlockIdEmptyHand; }
    public int BlockIdCraftingTable() { return mBlockIdCraftingTable; }
    public int BlockIdLava() { return mBlockIdLava; }
    public int BlockIdStationaryLava() { return mBlockIdStationaryLava; }
    public int BlockIdFillStart() { return mBlockIdFillStart; }
    public int BlockIdCuboid() { return mBlockIdCuboid; }
    public int BlockIdFillArea() { return mBlockIdFillArea; }
    public int BlockIdMinecart() { return mBlockIdMinecart; }
    public int BlockIdRailstart() { return mBlockIdRailstart; }

    // TODO: atm it sets sepcial block id from block name - better use new block property
    public bool SetSpecialBlock(Packet_BlockType b, int id)
    {
        switch (b.Name)
        {
            case "Empty":
                this.mBlockIdEmpty = id;
                return true;
            case "Dirt":
                this.mBlockIdDirt = id;
                return true;
            case "Sponge":
                this.mBlockIdSponge = id;
                return true;
            case "Trampoline":
                this.mBlockIdTrampoline = id;
                return true;
            case "Adminium":
                this.mBlockIdAdminium = id;
                return true;
            case "Compass":
                this.mBlockIdCompass = id;
                return true;
            case "Ladder":
                this.mBlockIdLadder = id;
                return true;
            case "EmptyHand":
                this.mBlockIdEmptyHand = id;
                return true;
            case "CraftingTable":
                this.mBlockIdCraftingTable = id;
                return true;
            case "Lava":
                this.mBlockIdLava = id;
                return true;
            case "StationaryLava":
                this.mBlockIdStationaryLava = id;
                return true;
            case "FillStart":
                this.mBlockIdFillStart = id;
                return true;
            case "Cuboid":
                this.mBlockIdCuboid = id;
                return true;
            case "FillArea":
                this.mBlockIdFillArea = id;
                return true;
            case "Minecart":
                this.mBlockIdMinecart = id;
                return true;
            case "Rail0":
                this.mBlockIdRailstart = id;
                return true;
            default:
                return false;
        }
    }

    public bool IsRailTile(int id)
    {
        return id >= BlockIdRailstart() && id < BlockIdRailstart() + 64;
    }

    public void UseBlockTypes(GamePlatform platform, Packet_BlockType[] blocktypes, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (blocktypes[i] != null)
            {
                UseBlockType(platform, i, blocktypes[i]);
            }
        }
    }

    public void UseBlockType(GamePlatform platform, int id, Packet_BlockType b)
    {
        if (b.Name == null)//!b.IsValid)
        {
            return;
        }
        //public bool[] IsWater { get { return mIsWater; } }
        //            public bool[] IsTransparentForLight { get { return mIsTransparentForLight; } }
        //public bool[] IsEmptyForPhysics { get { return mIsEmptyForPhysics; } }

        if (b.WhenPlacedGetsConvertedTo != 0)
        {
            mWhenPlayerPlacesGetsConvertedTo[id] = b.WhenPlacedGetsConvertedTo;
        }
        else
        {
            mWhenPlayerPlacesGetsConvertedTo[id] = id;
        }
        IsFlower()[id] = b.DrawType == Packet_DrawTypeEnum.Plant;
        Rail()[id] = b.Rail;
        WalkSpeed()[id] = DeserializeFloat(b.WalkSpeedFloat);
        IsSlipperyWalk()[id] = b.IsSlipperyWalk;
        WalkSound()[id] = new string[SoundCount];
        for (int i = 0; i < b.Sounds.WalkCount; i++)
        {
            WalkSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Walk[i], ".wav");
        }
        BreakSound()[id] = new string[SoundCount];
        for (int i = 0; i < b.Sounds.Break1Count; i++)
        {
            BreakSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Break1[i], ".wav");
        }
        BuildSound()[id] = new string[SoundCount];
        for (int i = 0; i < b.Sounds.BuildCount; i++)
        {
            BuildSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Build[i], ".wav");
        }
        CloneSound()[id] = new string[SoundCount];
        for (int i = 0; i < b.Sounds.CloneCount; i++)
        {
            CloneSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Clone[i], ".wav");
        }
        LightRadius()[id] = b.LightRadius;
        //StartInventoryAmount { get; }
        Strength()[id] = b.Strength;
        DamageToPlayer()[id] = b.DamageToPlayer;
        WalkableType1()[id] = b.WalkableType;
        SetSpecialBlock(b, id);
    }

    public const int SoundCount = 8;

    float DeserializeFloat(int p)
    {
        float one = 1;
        return (one * p) / 32;
    }
}