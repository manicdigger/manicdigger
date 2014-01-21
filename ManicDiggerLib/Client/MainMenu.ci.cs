public class MainMenu
{
    public MainMenu()
    {
        one = 1;
    }

    float one;

    public void Start(Gl gl_)
    {
        this.gl = gl_;

        xRot = 0;
        xSpeed = 0;

        yRot = 0;
        ySpeed = 0;

        z = -5;

        filter = 0;

        crateTextures = new WebGLTexture[3];
        crateImages = new HTMLImageElement[3];

        mvMatrix = Mat4.Create();
        pMatrix = Mat4.Create();

        currentlyPressedKeys = new bool[256];
        gl.AddOnNewFrame(MainMenuNewFrameHandler.Create(this));
        gl.AddOnKeyEvent(MainMenuKeyEventHandler.Create(this));
        gl.AddOnMouseEvent(MainMenuMouseEventHandler.Create(this));
        gl.AddOnTouchEvent(MainMenuTouchEventHandler.Create(this));
    }

    void InitShaders()
    {
        string vertexShaderSource = "    attribute vec3 aVertexPosition;"
   + "attribute vec2 aTextureCoord;"

   + "uniform mat4 uMVMatrix;"
   + "uniform mat4 uPMatrix;"

   + "varying vec2 vTextureCoord;"


    + "void main(void) {"
    + "    gl_Position = uPMatrix * uMVMatrix * vec4(aVertexPosition, 1.0);"
    + "    vTextureCoord = aTextureCoord;"
    + "}";

        string fragmentShaderSource = "     precision mediump float;"

    + "varying vec2 vTextureCoord;"
   + "uniform sampler2D uSampler;"

   + "void main(void) {"
   + "gl_FragColor = texture2D(uSampler, vec2(vTextureCoord.s, vTextureCoord.t));"
   + "}";

        WebGLShader vertexShader = gl.CreateShader(Gl.VertexShader);
        gl.ShaderSource(vertexShader, vertexShaderSource);
        gl.CompileShader(vertexShader);
        WebGLShader fragmentShader = gl.CreateShader(Gl.FragmentShader);
        gl.ShaderSource(fragmentShader, fragmentShaderSource);
        gl.CompileShader(fragmentShader);

        shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertexShader);
        gl.AttachShader(shaderProgram, fragmentShader);
        gl.LinkProgram(shaderProgram);

        //if (gl.GetProgramParameter(shaderProgram, Gl.LinkStatus) == null)
        //{
        //    //alert("Could not initialise shaders");
        //    //return;
        //}

        gl.UseProgram(shaderProgram);

        shaderProgramvertexPositionAttribute = gl.GetAttribLocation(shaderProgram, "aVertexPosition");
        gl.EnableVertexAttribArray(shaderProgramvertexPositionAttribute);

        shaderProgramtextureCoordAttribute = gl.GetAttribLocation(shaderProgram, "aTextureCoord");
        gl.EnableVertexAttribArray(shaderProgramtextureCoordAttribute);

        shaderProgrampMatrixUniform = gl.GetUniformLocation(shaderProgram, "uPMatrix");
        shaderProgrammvMatrixUniform = gl.GetUniformLocation(shaderProgram, "uMVMatrix");
        shaderProgramsamplerUniform = gl.GetUniformLocation(shaderProgram, "uSampler");
    }
    Gl gl;

    int viewportWidth;
    int viewportHeight;

    float[] mvMatrix;
    float[] pMatrix;

    WebGLProgram shaderProgram;
    int shaderProgramvertexPositionAttribute;
    int shaderProgramtextureCoordAttribute;
    WebGLUniformLocation shaderProgrampMatrixUniform;
    WebGLUniformLocation shaderProgrammvMatrixUniform;
    WebGLUniformLocation shaderProgramsamplerUniform;

    WebGLBuffer cubeVertexPositionBuffer;
    WebGLBuffer cubeVertexTextureCoordBuffer;
    WebGLBuffer cubeVertexIndexBuffer;

    bool[] currentlyPressedKeys;

    public void HandleKeyDown(KeyEventArgs e)
    {
        currentlyPressedKeys[e.GetKeyCode()] = true;
    }

    public void HandleKeyUp(KeyEventArgs e)
    {
        currentlyPressedKeys[e.GetKeyCode()] = false;
    }

    public void HandleKeyPress(KeyPressEventArgs e)
    {
        if (e.GetKeyChar() == 70 || e.GetKeyChar() == 102) // 'F', 'f'
        {
            filter += 1;
            if (filter == 3)
            {
                filter = 0;
            }
        }
    }

    void HandleKeys()
    {
        float elapsed_ = elapsed * 60;
        if (currentlyPressedKeys[33])
        {
            // Page Up
            z -= (one / 20) * elapsed_;
        }
        if (currentlyPressedKeys[34])
        {
            // Page Down
            z += (one / 20) * elapsed_;
        }
        if (currentlyPressedKeys[37])
        {
            // Left cursor key
            ySpeed -= 1 * elapsed_;
        }
        if (currentlyPressedKeys[39])
        {
            // Right cursor key
            ySpeed += 1 * elapsed_;
        }
        if (currentlyPressedKeys[38])
        {
            // Up cursor key
            xSpeed -= 1 * elapsed_;
        }
        if (currentlyPressedKeys[40])
        {
            // Down cursor key
            xSpeed += 1 * elapsed_;
        }
    }

    Model CreateModel(ModelData data)
    {
        Model m = new Model();
        cubeVertexPositionBuffer = gl.CreateBuffer();
        gl.BindBuffer(Gl.ArrayBuffer_, cubeVertexPositionBuffer);

        gl.BufferDataFloat(Gl.ArrayBuffer_, data.xyz, Gl.StaticDraw);
        m.xyzBufferitemSize = 3;
        m.xyzBuffernumItems = data.verticesCount;

        cubeVertexTextureCoordBuffer = gl.CreateBuffer();
        gl.BindBuffer(Gl.ArrayBuffer_, cubeVertexTextureCoordBuffer);

        gl.BufferDataFloat(Gl.ArrayBuffer_, data.uv, Gl.StaticDraw);
        m.uvBufferitemSize = 2;
        m.uvBuffernumItems = data.verticesCount;

        cubeVertexIndexBuffer = gl.CreateBuffer();
        gl.BindBuffer(Gl.ElementArrayBuffer, cubeVertexIndexBuffer);
        gl.BufferDataUshort(Gl.ElementArrayBuffer, data.indices, Gl.StaticDraw);
        m.indexBufferitemSize = 1;
        m.indexBuffernumItems = data.indicesCount;
        return m;
    }

    public void HandleLoadedTexture(WebGLTexture[] textures, HTMLImageElement[] images)
    {
        // gl.PixelStorei(Gl.UnpackFlipYWebgl, 1);

        gl.BindTexture(Gl.Texture2d, textures[0]);
        gl.TexImage2DImage(Gl.Texture2d, 0, Gl.Rgba, Gl.Rgba, Gl.UnsignedByte, images[0]);
        gl.TexParameteri(Gl.Texture2d, Gl.TextureMagFilter, Gl.Nearest);
        gl.TexParameteri(Gl.Texture2d, Gl.TextureMinFilter, Gl.Nearest);

        gl.BindTexture(Gl.Texture2d, textures[1]);
        gl.TexImage2DImage(Gl.Texture2d, 0, Gl.Rgba, Gl.Rgba, Gl.UnsignedByte, images[1]);
        gl.TexParameteri(Gl.Texture2d, Gl.TextureMagFilter, Gl.Linear);
        gl.TexParameteri(Gl.Texture2d, Gl.TextureMinFilter, Gl.Linear);

        gl.BindTexture(Gl.Texture2d, textures[2]);
        gl.TexImage2DImage(Gl.Texture2d, 0, Gl.Rgba, Gl.Rgba, Gl.UnsignedByte, images[2]);
        gl.TexParameteri(Gl.Texture2d, Gl.TextureMagFilter, Gl.Linear);
        gl.TexParameteri(Gl.Texture2d, Gl.TextureMinFilter, Gl.LinearMipmapNearest);
        gl.GenerateMipmap(Gl.Texture2d);

        gl.BindTexture(Gl.Texture2d, null);
    }

    public void ImageOnLoad()
    {
        HandleLoadedTexture(crateTextures, crateImages);
    }

    WebGLTexture[] crateTextures;
    HTMLImageElement[] crateImages;

    void InitTexture()
    {
        HTMLImageElement crateImage = gl.CreateHTMLImageElement();

        for (int i = 0; i < 3; i++)
        {
            WebGLTexture texture = gl.CreateTexture();
            crateImages[i] = crateImage;
            crateTextures[i] = texture;
        }

        crateImage.SetOnLoad(MainMenuImageOnLoadHandler.Create(this));
        crateImage.SetSrc("logo.png");
    }

    void DrawScene()
    {
        gl.Viewport(0, 0, viewportWidth, viewportHeight);
        gl.Clear(Gl.ColorBufferBit | Gl.DepthBufferBit);
        {
            //Mat4.Perspective(pMatrix, 45, one * viewportWidth / viewportHeight, one / 100, one * 1000);
            //Mat4.Identity_(mvMatrix);
            //Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(0, 0, z));
        }
        {
            Mat4.Identity_(pMatrix);
            Mat4.Ortho(pMatrix, 0, gl.GetCanvasWidth(), gl.GetCanvasHeight(), 0, 0, 10);
        }
        
        float scale = one * gl.GetCanvasWidth() / 800;
        float size = one * 80 / 100;
        Draw2dQuad(0, gl.GetCanvasWidth() / 2 - 800 * scale / 2 * size, 0, 800 * scale * size, 288 * scale * size);
    }

    Model cubeModel;
    public void Draw2dQuad(int textureid, float dx, float dy, float dw, float dh)
    {
        Mat4.Identity_(mvMatrix);
        Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(dx, dy, 0));
        Mat4.Scale(mvMatrix, mvMatrix, Vec3.FromValues(dw, dh, 0));
        Mat4.Scale(mvMatrix, mvMatrix, Vec3.FromValues(one / 2, one / 2, 0));
        Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(one, one, 0));
        SetMatrixUniforms();
        if (cubeModel == null)
        {
            cubeModel = CreateModel(QuadModelData.GetQuadModelData());
        }
        DrawModel(cubeModel);
    }

    void DrawModel(Model model)
    {
        gl.BindBuffer(Gl.ArrayBuffer_, cubeVertexPositionBuffer);
        gl.VertexAttribPointer(shaderProgramvertexPositionAttribute, model.xyzBufferitemSize, Gl.Float, false, 0, 0);

        gl.BindBuffer(Gl.ArrayBuffer_, cubeVertexTextureCoordBuffer);
        gl.VertexAttribPointer(shaderProgramtextureCoordAttribute, model.uvBufferitemSize, Gl.Float, false, 0, 0);

        gl.ActiveTexture(Gl.Texture0);
        gl.BindTexture(Gl.Texture2d, crateTextures[filter]);
        gl.Uniform1i(shaderProgramsamplerUniform, 0);

        gl.BindBuffer(Gl.ElementArrayBuffer, cubeVertexIndexBuffer);
        gl.DrawElements(Gl.Triangles, model.indexBuffernumItems, Gl.UnsignedShort, 0);
    }

    void SetMatrixUniforms()
    {
        gl.UniformMatrix4fv(shaderProgrampMatrixUniform, false, pMatrix);
        gl.UniformMatrix4fv(shaderProgrammvMatrixUniform, false, mvMatrix);
    }

    float degToRad(float degrees)
    {
        return degrees * GlMatrixMath.PI() / 180;
    }

    float xRot;
    float xSpeed;

    float yRot;
    float ySpeed;

    float z;

    int filter;

    bool initialized;

    float elapsed;

    void Animate()
    {
        xRot += xSpeed * elapsed;
        yRot += ySpeed * elapsed;
    }

    public void OnNewFrame(NewFrameEventArgs args)
    {
        elapsed = args.GetDt();
        if (!initialized)
        {
            initialized = true;
            InitShaders();
            InitTexture();

            gl.ClearColor(0, 0, 0, 1);
            gl.Enable(Gl.DepthTest);
        }
        viewportWidth = gl.GetCanvasWidth();
        viewportHeight = gl.GetCanvasHeight();
        HandleKeys();
        DrawScene();
        Animate();
    }

    public static void RunMain()
    {
        Gl g = Gl.Create();
        MainMenu l = new MainMenu();
        l.Start(g);
        g.Start();
    }

    public void HandleMouseDown(MouseEventArgs e)
    {
        mousePressed = true;
        previousMouseX = e.GetX();
        previousMouseY = e.GetY();
    }

    public void HandleMouseUp(MouseEventArgs e)
    {
        mousePressed = false;
    }

    bool mousePressed;

    int previousMouseX;
    int previousMouseY;

    public void HandleMouseMove(MouseEventArgs e)
    {
        float dx = e.GetX() - previousMouseX;
        float dy = e.GetY() - previousMouseY;
        previousMouseX = e.GetX();
        previousMouseY = e.GetY();
        if (mousePressed)
        {
            ySpeed += dx / 10;
            xSpeed += dy / 10;
        }
    }

    public void HandleMouseWheel(MouseWheelEventArgs e)
    {
        z += e.GetDeltaPrecise() / 5;
    }

    public void HandleTouchStart(TouchEventArgs e)
    {
        touchId = e.GetId();
        previousTouchX = e.GetX();
        previousTouchY = e.GetY();
    }

    int touchId;
    int previousTouchX;
    int previousTouchY;

    public void HandleTouchMove(TouchEventArgs e)
    {
        if (e.GetId() != touchId)
        {
            return;
        }
        float dx = e.GetX() - previousTouchX;
        float dy = e.GetY() - previousTouchY;
        previousTouchX = e.GetX();
        previousTouchY = e.GetY();

        ySpeed += dx / 10;
        xSpeed += dy / 10;
    }

    public void HandleTouchEnd(TouchEventArgs e)
    {
    }
}

public class Model
{
    internal int xyzBufferitemSize;
    internal int xyzBuffernumItems;
    internal int uvBufferitemSize;
    internal int uvBuffernumItems;
    internal int indexBufferitemSize;
    internal int indexBuffernumItems;
}

public class ModelData
{
    internal int verticesCount;
    public int GetVerticesCount() { return verticesCount; }
    public void SetVerticesCount(int value) { verticesCount = value; }
    internal int indicesCount;
    public int GetIndicesCount() { return indicesCount; }
    public void SetIndicesCount(int value) { indicesCount = value; }
    internal float[] xyz;
    public int GetXyzCount() { return verticesCount * 3; }
    internal byte[] rgba;
    public int GetRgbaCount() { return verticesCount * 4; }
    internal float[] uv;
    public int GetUvCount() { return verticesCount * 2; }
    internal int[] indices;
    internal int mode;

    public float[] getXyz() { return xyz; }
    public void setXyz(float[] p) { xyz = p; }
    public byte[] getRgba() { return rgba; }
    public void setRgba(byte[] p) { rgba = p; }
    public float[] getUv() { return uv; }
    public void setUv(float[] p) { uv = p; }
    public int[] getIndices() { return indices; }
    public void setIndices(int[] p) { indices = p; }
    public int getMode() { return mode; }
    public void setMode(int p) { mode = p; }
}

public class MainMenuNewFrameHandler : NewFrameHandler
{
    public static MainMenuNewFrameHandler Create(MainMenu l)
    {
        MainMenuNewFrameHandler h = new MainMenuNewFrameHandler();
        h.l = l;
        return h;
    }
    MainMenu l;
    public override void OnNewFrame(NewFrameEventArgs args)
    {
        l.OnNewFrame(args);
    }
}

public class MainMenuImageOnLoadHandler : ImageOnLoadHandler
{
    public static MainMenuImageOnLoadHandler Create(MainMenu l)
    {
        MainMenuImageOnLoadHandler h = new MainMenuImageOnLoadHandler();
        h.l = l;
        return h;
    }
    MainMenu l;

    public override void OnLoad()
    {
        l.ImageOnLoad();
    }
}

public class MainMenuKeyEventHandler : KeyEventHandler
{
    public static MainMenuKeyEventHandler Create(MainMenu l)
    {
        MainMenuKeyEventHandler h = new MainMenuKeyEventHandler();
        h.l = l;
        return h;
    }
    MainMenu l;
    public override void OnKeyDown(KeyEventArgs e)
    {
        l.HandleKeyDown(e);
    }
    public override void OnKeyUp(KeyEventArgs e)
    {
        l.HandleKeyUp(e);
    }

    public override void OnKeyPress(KeyPressEventArgs e)
    {
        l.HandleKeyPress(e);
    }
}

public class MainMenuMouseEventHandler : MouseEventHandler
{
    public static MainMenuMouseEventHandler Create(MainMenu l)
    {
        MainMenuMouseEventHandler h = new MainMenuMouseEventHandler();
        h.l = l;
        return h;
    }
    MainMenu l;

    public override void OnMouseDown(MouseEventArgs e)
    {
        l.HandleMouseDown(e);
    }

    public override void OnMouseUp(MouseEventArgs e)
    {
        l.HandleMouseUp(e);
    }

    public override void OnMouseMove(MouseEventArgs e)
    {
        l.HandleMouseMove(e);
    }

    public override void OnMouseWheel(MouseWheelEventArgs e)
    {
        l.HandleMouseWheel(e);
    }
}

public class MainMenuTouchEventHandler : TouchEventHandler
{
    public static MainMenuTouchEventHandler Create(MainMenu l)
    {
        MainMenuTouchEventHandler h = new MainMenuTouchEventHandler();
        h.l = l;
        return h;
    }
    MainMenu l;

    public override void OnTouchStart(TouchEventArgs e)
    {
        l.HandleTouchStart(e);
    }

    public override void OnTouchMove(TouchEventArgs e)
    {
        l.HandleTouchMove(e);
    }

    public override void OnTouchEnd(TouchEventArgs e)
    {
        l.HandleTouchEnd(e);
    }
}
