public class ChosenGameTypeEnum
{
    const int None = 0;
    const int Singleplayer = 1;
    const int Multiplayer = 2;
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

public class MainMenu
{
    public void Start(Gl gl_)
    {
        this.gl = gl_;

        mvMatrix = Mat4.Create();
        pMatrix = Mat4.Create();

        gl.AddOnNewFrame(MainMenuNewFrameHandler.Create(this));
    }

    void InitShaders()
    {
        string vertexShaderSource = "    attribute vec3 aVertexPosition;"
            + ""
    + "uniform mat4 uMVMatrix;"
    + "uniform mat4 uPMatrix;"

    + "void main(void) {"
        + "gl_Position = uPMatrix * uMVMatrix * vec4(aVertexPosition, 1.0);"
    + "}";

        string fragmentShaderSource = "    precision mediump float;"
        + "void main(void) {"
            + "gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);"
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

        if (gl.GetProgramParameter(shaderProgram, Gl.LinkStatus) == null)
        {
            //alert("Could not initialise shaders");
            //return;
        }

        gl.UseProgram(shaderProgram);

        shaderProgramvertexPositionAttribute = gl.GetAttribLocation(shaderProgram, "aVertexPosition");
        gl.EnableVertexAttribArray(shaderProgramvertexPositionAttribute);

        shaderProgrampMatrixUniform = gl.GetUniformLocation(shaderProgram, "uPMatrix");
        shaderProgrammvMatrixUniform = gl.GetUniformLocation(shaderProgram, "uMVMatrix");
    }
    Gl gl;

    int viewportWidth;
    int viewportHeight;

    float[] mvMatrix;
    float[] pMatrix;

    WebGLProgram shaderProgram;
    int shaderProgramvertexPositionAttribute;
    WebGLUniformLocation shaderProgrampMatrixUniform;
    WebGLUniformLocation shaderProgrammvMatrixUniform;

    WebGLBuffer triangleVertexPositionBuffer;
    int triangleVertexPositionBufferitemSize;
    int triangleVertexPositionBuffernumItems;
    WebGLBuffer squareVertexPositionBuffer;
    int squareVertexPositionBufferitemSize;
    int squareVertexPositionBuffernumItems;

    void InitBuffers()
    {
        triangleVertexPositionBuffer = gl.CreateBuffer();
        gl.BindBuffer(Gl.ArrayBuffer_, triangleVertexPositionBuffer);
        float[] vertices = new float[9];
        vertices[0] = 0; vertices[1] = 1; vertices[2] = 0;
        vertices[3] = -1; vertices[4] = -1; vertices[5] = 0;
        vertices[6] = 1; vertices[7] = -1; vertices[8] = 0;
        gl.BufferDataFloat(Gl.ArrayBuffer_, vertices, Gl.StaticDraw);
        triangleVertexPositionBufferitemSize = 3;
        triangleVertexPositionBuffernumItems = 3;

        squareVertexPositionBuffer = gl.CreateBuffer();
        gl.BindBuffer(Gl.ArrayBuffer_, squareVertexPositionBuffer);
        vertices = new float[12];
        vertices[0] = 1; vertices[1] = 1; vertices[2] = 0;
        vertices[3] = -1; vertices[4] = 1; vertices[5] = 0;
        vertices[6] = 1; vertices[7] = -1; vertices[8] = 0;
        vertices[9] = -1; vertices[10] = -1; vertices[11] = 0;

        gl.BufferDataFloat(Gl.ArrayBuffer_, vertices, Gl.StaticDraw);
        squareVertexPositionBufferitemSize = 3;
        squareVertexPositionBuffernumItems = 4;
    }

    void DrawScene()
    {
        gl.Viewport(0, 0, viewportWidth, viewportHeight);
        gl.Clear(Gl.ColorBufferBit | Gl.DepthBufferBit);

        float one = 1;
        Mat4.Perspective(pMatrix, 45, one * viewportWidth / viewportHeight, one / 10, 100);

        Mat4.Identity_(mvMatrix);

        Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(-one * 3 / 2, 0, -7));
        gl.BindBuffer(Gl.ArrayBuffer_, triangleVertexPositionBuffer);
        gl.VertexAttribPointer(shaderProgramvertexPositionAttribute, triangleVertexPositionBufferitemSize, Gl.Float, false, 0, 0);
        SetMatrixUniforms();
        gl.DrawArrays(Gl.Triangles, 0, triangleVertexPositionBuffernumItems);

        Mat4.Translate(mvMatrix, mvMatrix, Vec3.FromValues(3, 0, 0));
        gl.BindBuffer(Gl.ArrayBuffer_, squareVertexPositionBuffer);
        gl.VertexAttribPointer(shaderProgramvertexPositionAttribute, squareVertexPositionBufferitemSize, Gl.Float, false, 0, 0);
        SetMatrixUniforms();
        gl.DrawArrays(Gl.TriangleStrip, 0, squareVertexPositionBuffernumItems);
    }

    void SetMatrixUniforms()
    {
        gl.UniformMatrix4fv(shaderProgrampMatrixUniform, false, pMatrix);
        gl.UniformMatrix4fv(shaderProgrammvMatrixUniform, false, mvMatrix);
    }

    bool initialized;

    public void OnNewFrame(NewFrameEventArgs args)
    {
        if (!initialized)
        {
            initialized = true;
            InitShaders();
            InitBuffers();
            InitTexture();
            gl.ClearColor(0, 0, 0, 1);
            gl.Enable(Gl.DepthTest);
        }
        viewportWidth = gl.GetCanvasWidth();
        viewportHeight = gl.GetCanvasHeight();
        DrawScene();
    }

    void InitTexture()
    {
    }

    public static void RunMain()
    {
        Gl g = Gl.Create();
        MainMenu l = new MainMenu();
        l.Start(g);
        g.Start();
    }

    internal void ImageOnLoad()
    {
    }
}
