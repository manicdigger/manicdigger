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
            byte[] pixels = new byte[4];
            pixels[0] = 255;
            pixels[1] = 255;
            pixels[2] = 255;
            pixels[3] = 255;
            platform.BitmapSetPixelsRgba(bmp, pixels);
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

    const int maxlight = 15;

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

    internal Player[] players;
    internal int playersCount;
    internal int LocalPlayerId;

    internal float GetCharacterEyesHeight()
    {
        return players[LocalPlayerId].EyeHeight;
    }

    internal void SetCharacterEyesHeight(float value)
    {
        players[LocalPlayerId].EyeHeight = value;
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
        return platform.FloatToInt(MathFloor(player.playerposition.Y + players[LocalPlayerId].EyeHeight));
    }

    float MathFloor(float a)
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
}

public enum PlayerType
{
    Player,
    Monster
}

public class Projectile_
{
    internal float positionX;
    internal float positionY;
    internal float positionZ;
    internal float velocityX;
    internal float velocityY;
    internal float velocityZ;
    internal int startMilliseconds;
    internal int block;
    internal float explodesafter;
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
            mWalkSound[i] = new string[0];
        }
        mBreakSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mBreakSound[i] = new string[0];
        }
        mBuildSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mBuildSound[i] = new string[0];
        }
        mCloneSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mCloneSound[i] = new string[0];
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
        WalkSound()[id] = new string[b.Sounds.WalkCount];
        for (int i = 0; i < b.Sounds.WalkCount; i++)
        {
            WalkSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Walk[i], ".wav");
        }
        BreakSound()[id] = new string[b.Sounds.Break1Count];
        for (int i = 0; i < b.Sounds.Break1Count; i++)
        {
            BreakSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Break1[i], ".wav");
        }
        BuildSound()[id] = new string[b.Sounds.BuildCount];
        for (int i = 0; i < b.Sounds.BuildCount; i++)
        {
            BuildSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Build[i], ".wav");
        }
        CloneSound()[id] = new string[b.Sounds.CloneCount];
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

    float DeserializeFloat(int p)
    {
        float one = 1;
        return (one * p) / 32;
    }
}