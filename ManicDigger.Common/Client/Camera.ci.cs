public class Camera {
    public float CameraEyeX;
    public float CameraEyeY;
    public float CameraEyeZ;

    public Camera() {
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


    }
    internal GetCameraMatrix CameraMatrix;

    internal StackMatrix4 mvMatrix;
    internal StackMatrix4 pMatrix;
    internal float znear;

    internal GamePlatform platform;

    public void SetPlatform(GamePlatform platform_) {
        this.platform = platform_;
    }

    float[] Set3dProjectionTempMat4;
    public void Set3dProjection(float zfar, float fov)
    {
        float aspect_ratio =  1.0f * platform.GetCanvasWidth() / platform.GetCanvasHeight(); 
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


}