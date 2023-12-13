public class CoreRenderer
{
    public float CameraEyeX;
    public float CameraEyeY;
    public float CameraEyeZ;
    public float one;
    public CoreRenderer()
    {
        one = 1;
        CameraMatrix = new GetCameraMatrix();
        CameraEyeX = -1;
        CameraEyeY = -1;
        CameraEyeZ = -1;
        znear = 1.0f / 10;

        mvMatrix = new StackMatrix4();
        pMatrix = new StackMatrix4();
        mvMatrix.Push(Mat4.Create());
        pMatrix.Push(Mat4.Create());

        GLScaleTempVec3 = Vec3.Create();
        GLRotateTempVec3 = Vec3.Create();
        GLTranslateTempVec3 = Vec3.Create();
        identityMatrix = Mat4.Identity_(Mat4.Create());
        Set3dProjectionTempMat4 = Mat4.Create();

        whitetexture = -1;
        cachedTextTexturesMax = 1024;
        cachedTextTextures = new CachedTextTexture[cachedTextTexturesMax];
        for (int i = 0; i < cachedTextTexturesMax; i++)
        {
            cachedTextTextures[i] = null;
        }
    }
    internal GetCameraMatrix CameraMatrix;

    internal StackMatrix4 mvMatrix;
    internal StackMatrix4 pMatrix;
    internal float znear;

    internal GamePlatform platform;

    internal int texturesPacked() { return GlobalVar.MAX_BLOCKTYPES_SQRT; } //16x16
    internal TextColorRenderer textColorRenderer;

    internal CachedTextTexture[] cachedTextTextures;
    internal int cachedTextTexturesMax;

    public void Start(GamePlatform platform_)
    {
        this.platform = platform_;
        textColorRenderer = new TextColorRenderer();
        textColorRenderer.platform = platform;

    }

    int whitetexture;
    public int WhiteTexture()
    {
        if (this.whitetexture == -1)
        {
            BitmapCi bmp = platform.BitmapCreate(1, 1);
            int[] pixels = new int[1];
            pixels[0] = ColorCi.FromArgb(255, 255, 255, 255);
            platform.BitmapSetPixelsArgb(bmp, pixels);
            this.whitetexture = platform.LoadTextureFromBitmap(bmp);
        }
        return this.whitetexture;
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

    public void Draw2dTexture(int textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int atlastextures, int color, bool enabledepthtest)
    {
        platform.GLDisableAlphaTest();
        if (color == ColorCi.FromArgb(255, 255, 255, 255) && inAtlasId == null)
        {
            Draw2dTextureSimple(textureid, x1, y1, width, height, enabledepthtest);
        }
        else
        {
            Draw2dTextureInAtlas(textureid, x1, y1, width, height, inAtlasId, atlastextures, color, enabledepthtest);
        }
        platform.GLEnableAlphaTest();
    }
    Model quadModel;
    void Draw2dTextureSimple(int textureid, float x1, float y1, float width, float height, bool enabledepthtest)
    {
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
            x1, y1, width, height, ConvertCi.IntToByte(ColorCi.ExtractR(color)), ConvertCi.IntToByte(ColorCi.ExtractG(color)), ConvertCi.IntToByte(ColorCi.ExtractB(color)), ConvertCi.IntToByte(ColorCi.ExtractA(color)));
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
            dstx, dsty, dstwidth, dstheight, ConvertCi.IntToByte(ColorCi.ExtractR(color)), ConvertCi.IntToByte(ColorCi.ExtractG(color)), ConvertCi.IntToByte(ColorCi.ExtractB(color)), ConvertCi.IntToByte(ColorCi.ExtractA(color)));
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
                x1, y1, width, height, ConvertCi.IntToByte(ColorCi.ExtractR(color)), ConvertCi.IntToByte(ColorCi.ExtractG(color)), ConvertCi.IntToByte(ColorCi.ExtractB(color)), ConvertCi.IntToByte(ColorCi.ExtractA(color)));
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


    public void Draw2dText(string text, FontCi font, float x, float y, IntRef color, bool enabledepthtest)
    {
        if (text == null || platform.StringTrim(text) == "")
        {
            return;
        }
        if (color == null) { color = IntRef.Create(ColorCi.FromArgb(255, 255, 255, 255)); }
        Text_ t = new Text_();
        t.text = text;
        t.color = color.value;
        t.font = font;
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
        Draw2dTexture(ct.textureId, x, y, ct.sizeX, ct.sizeY, null, 0, ColorCi.FromArgb(255, 255, 255, 255), enabledepthtest);
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

    float[] Set3dProjectionTempMat4;
    public void Set3dProjection(float zfar, float fov)
    {
        float aspect_ratio = 1.0f * platform.GetCanvasWidth() / platform.GetCanvasHeight();
        Mat4.Perspective(Set3dProjectionTempMat4, fov, aspect_ratio, znear, zfar);
        CameraMatrix.lastpmatrix = Set3dProjectionTempMat4;
        GLMatrixModeProjection();
        GLLoadMatrix(Set3dProjectionTempMat4);
        SetMatrixUniformProjection();
    }
    internal bool currentMatrixModeProjection;

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

    public void Dispose()
    {
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
    public void RemoveCahedTextures()
    {
        for (int i = 0; i < cachedTextTexturesMax; i++)
        {
            cachedTextTextures[i] = null;
        }

    }

    ShaderCi GenerateTextureshader;

    public void GenerateTexture(Game game, int tileType, int id2, int id3) {
        game.platform.GenerateTextureStart();

        float[] perspective = new float[16];
        Mat4.Perspective(perspective, 0.013f, 1, 64, 256);
        game.platform.SetMatrixUniformProjection(perspective);
        //game.platform.SetMatrixUniform()

        float[] viewMatrix = new float[16];

        Mat4.Multiply(viewMatrix, Mat4.RotationX(3.14159265359f / 4), Mat4.RotationY(-3.14159265359f / 4));


        ModelData data = new ModelData();

            for (int i = 0; i < TileSideEnum.SideCount; i++)
        {

            AddBlockFaceS(game,data,0, 0, 0, tileType, i, 0, 0,0, 1, 1, 1);

        }

        if (GenerateTextureshader==null) {
            GenerateTextureshader = new ShaderCi();
            GenerateTextureshader.Init(game.platform);
            GenerateTextureshader.Compile(ShaderSources.VertexSkysphere, ShaderType.VertexShader);
            GenerateTextureshader.Compile(ShaderSources.FragmentSkysphere, ShaderType.FragmentShader);
            GenerateTextureshader.Link();
        }
        GenerateTextureshader.BeginUse();
        game.platform.GlUniform3f(GenerateTextureshader.GetUniformLocation("modelPosition"), -65.5f - 1.5f, -92.631f - 1.5f, -65.5f - 1.5f);
        game.platform.GlUniform1i(GenerateTextureshader.GetUniformLocation("visibilityMask"), 0xff);
        game.platform.GlUniform1i(GenerateTextureshader.GetUniformLocation("voxelSize"), 1);
        game.platform.GlUniform1i(GenerateTextureshader.GetUniformLocation("glow"), 3);
        game.platform.GlActiveTexture(game.terrainTexture);
        game.platform.GenerateTextureEnd(0, 0);

    }

    static int ColorMultiply(Game game, int c, float fValue)
    {
        return ColorCi.FromArgb(ColorCi.ExtractA(c),
            game.platform.FloatToInt(ColorCi.ExtractR(c) * fValue),
            game.platform.FloatToInt(ColorCi.ExtractG(c) * fValue),
            game.platform.FloatToInt(ColorCi.ExtractB(c) * fValue));
    }
   
     //tp implement inventory rendering
    static ModelData AddBlockFaceS(Game game_, ModelData toreturn , int x, int y, int z, int tileType, int tileSide, float vOffsetX, float vOffsetY, float vOffsetZ, float vScaleX, float vScaleY, float vScaleZ)
    {
        int color = ColorCi.FromArgb(255, 255, 255, 255);
        float terrainTexturesPerAtlas = game_.terrainTexturesPerAtlas;
        float terrainTexturesPerAtlasInverse = 1f / game_.terrainTexturesPerAtlas;
        float AtiArtifactFix;
        if (game_.platform.IsFastSystem())
        {
            AtiArtifactFix = 1 / 32f * 0.25f;   
        }
        else
        {
             AtiArtifactFix = 1 / 32f * 1.5f;  
        }

        float _texrecWidth = 1 - (AtiArtifactFix * 2);
        float _texrecHeight = terrainTexturesPerAtlasInverse * (1 - (AtiArtifactFix * 2));
        float _texrecLeft = AtiArtifactFix;
        float _texrecRight = _texrecLeft + _texrecWidth;

        int sidetexture = game_.TextureId[tileType][tileSide];

        float texrecTop = (terrainTexturesPerAtlasInverse * (sidetexture % terrainTexturesPerAtlas)) + (AtiArtifactFix * terrainTexturesPerAtlasInverse);
        float texrecBottom = texrecTop + _texrecHeight;
        int lastelement = toreturn.verticesCount;

   

        VecCito3i v = new VecCito3i();
        float fSlopeModifier = 0f;

        float xPos = x + vOffsetX + ((v.x * 0.5f) * vScaleX);
        float zPos = z + vOffsetZ + ((v.z * 0.5f) * vScaleZ) + fSlopeModifier;
        float yPos = y + vOffsetY + ((v.y * 0.5f) * vScaleY);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecRight, texrecTop, ColorMultiply(game_,color, 1));

        xPos = x + vOffsetX + ((v.x * 0.5f) * vScaleX);
        zPos = z + vOffsetZ + ((v.z * 0.5f) * vScaleZ) + fSlopeModifier;
        yPos = y + vOffsetY + ((v.y * 0.5f) * vScaleY);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecLeft, texrecTop, ColorMultiply(game_,color, 1));

        xPos = x + vOffsetX + ((v.x * 0.5f) * vScaleX);
        zPos = z + vOffsetZ + ((v.z * 0.5f) * vScaleZ) + fSlopeModifier;
        yPos = y + vOffsetY + ((v.y * 0.5f) * vScaleY);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecRight, texrecBottom, ColorMultiply(game_,color,1));

        xPos = x + vOffsetX + ((v.x * 0.5f) * vScaleX);
        zPos = z + vOffsetZ + ((v.z * 0.5f) * vScaleZ) + fSlopeModifier;
        yPos = y + vOffsetY + ((v.y * 0.5f) * vScaleY);
        ModelDataTool.AddVertex(toreturn, xPos, zPos, yPos, _texrecLeft, texrecBottom, ColorMultiply(game_,color,1));

        {
            ModelDataTool.AddIndex(toreturn, (lastelement + 0));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
            ModelDataTool.AddIndex(toreturn, (lastelement + 1));
            ModelDataTool.AddIndex(toreturn, (lastelement + 3));
            ModelDataTool.AddIndex(toreturn, (lastelement + 2));
        }
        return toreturn;
    }

}
