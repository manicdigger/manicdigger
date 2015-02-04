public class CubeModelData
{
#if CITO
    const
#else
    static
#endif
 int[] cubeVertices = {
            // Front face
            -1, -1,  1,
             1, -1,  1,
             1,  1,  1,
            -1,  1,  1,

            // Back face
            -1, -1, -1,
            -1,  1, -1,
             1,  1, -1,
             1, -1, -1,

            // Top face
            -1,  1, -1,
            -1,  1,  1,
             1,  1,  1,
             1,  1, -1,

            // Bottom face
            -1, -1, -1,
             1, -1, -1,
             1, -1,  1,
            -1, -1,  1,

            // Right face
             1, -1, -1,
             1,  1, -1,
             1,  1,  1,
             1, -1,  1,

            // Left face
            -1, -1, -1,
            -1, -1,  1,
            -1,  1,  1,
            -1,  1, -1
        };

#if CITO
    const
#else
    static
#endif
 int[] cubeTextureCoords = {
            // Front face
            0, 0,
            1, 0,
            1, 1,
            0, 1,

            // Back face
            1, 0,
            1, 1,
            0, 1,
            0, 0,

            // Top face
            0, 1,
            0, 0,
            1, 0,
            1, 1,

            // Bottom face
            1, 1,
            0, 1,
            0, 0,
            1, 0,

            // Right face
            1, 0,
            1, 1,
            0, 1,
            0, 0,

            // Left face
            0, 0,
            1, 0,
            1, 1,
            0, 1
            };

#if CITO
    const
#else
    static
#endif
 int[] cubeVertexIndices = {
            0, 1, 2,      0, 2, 3,    // Front face
            4, 5, 6,      4, 6, 7,    // Back face
            8, 9, 10,     8, 10, 11,  // Top face
            12, 13, 14,   12, 14, 15, // Bottom face
            16, 17, 18,   16, 18, 19, // Right face
            20, 21, 22,   20, 22, 23  // Left face
            };

    public static ModelData GetCubeModelData()
    {
        ModelData m = new ModelData();
        float[] xyz = new float[3 * 4 * 6];
        for (int i = 0; i < 3 * 4 * 6; i++)
        {
            xyz[i] = cubeVertices[i];
        }
        m.setXyz(xyz);
        float[] uv = new float[2 * 4 * 6];
        for (int i = 0; i < 2 * 4 * 6; i++)
        {
            uv[i] = cubeTextureCoords[i];
        }
        m.setUv(uv);
        m.SetVerticesCount(4 * 6);
        m.setIndices(cubeVertexIndices);
        m.SetIndicesCount(3 * 2 * 6);
        return m;
    }
}

public class QuadModelData
{
#if CITO
    const
#else
    static
#endif
 int[] cubeVertices = {
            // Front face
            -1, -1,  0,
             1, -1,  0,
             1,  1,  0,
            -1,  1,  0
        };

#if CITO
    const
#else
    static
#endif
 int[] quadTextureCoords = {
            // Front face
            0, 0,
            1, 0,
            1, 1,
            0, 1
            };

#if CITO
    const
#else
    static
#endif
 int[] quadVertexIndices = {
            0, 1, 2,      0, 2, 3    // Front face
            };

    public static ModelData GetQuadModelData()
    {
        ModelData m = new ModelData();
        float[] xyz = new float[3 * 4];
        for (int i = 0; i < 3 * 4; i++)
        {
            xyz[i] = cubeVertices[i];
        }
        m.setXyz(xyz);
        float[] uv = new float[2 * 4];
        for (int i = 0; i < 2 * 4; i++)
        {
            uv[i] = quadTextureCoords[i];
        }
        m.setUv(uv);
        m.SetVerticesCount(4);
        m.setIndices(quadVertexIndices);
        m.SetIndicesCount(3 * 2);
        return m;
    }
    public static ModelData GetQuadModelData2(
        float sx, float sy, float sw, float sh,
        float dx, float dy, float dw, float dh,
        byte r, byte g, byte b, byte a)
    {
        ModelData m = new ModelData();
        float[] xyz = new float[3 * 4];

        xyz[0] = dx;
        xyz[1] = dy;
        xyz[2] = 0;

        xyz[3] = dx + dw;
        xyz[4] = dy;
        xyz[5] = 0;

        xyz[6] = dx + dw;
        xyz[7] = dy + dh;
        xyz[8] = 0;

        xyz[9] = dx;
        xyz[10] = dy + dh;
        xyz[11] = 0;

        m.setXyz(xyz);

        float[] uv = new float[2 * 4];

        uv[0] = sx;
        uv[1] = sy;

        uv[2] = sx + sw;
        uv[3] = sy;

        uv[4] = sx + sw;
        uv[5] = sy + sh;

        uv[6] = sx;
        uv[7] = sy + sh;
        
        m.setUv(uv);

        byte[] rgba = new byte[4 * 4];
        for (int i = 0; i < 4; i++)
        {
            rgba[i * 4 + 0] = r;
            rgba[i * 4 + 1] = g;
            rgba[i * 4 + 2] = b;
            rgba[i * 4 + 3] = a;
        }

        m.setRgba(rgba);
        
        m.SetVerticesCount(4);
        m.setIndices(quadVertexIndices);
        m.SetIndicesCount(3 * 2);
        return m;
    }
}

public class SphereModelData
{
    static float GetPi()
    {
        float a = 3141592;
        return a / 1000000;
    }

    //http://www.opentk.com/node/732
    //Re: [SL Multitexturing] - Only one texture with gluSphere
    //Posted Sunday, 22 March, 2009 - 23:50 by the Fiddler
    public static ModelData GetSphereModelData(float radius, float height, int segments, int rings)
    {
        int i = 0;
        // Load data into a vertex buffer or a display list afterwards.

        float[] xyz = new float[rings * segments * 3];
        float[] uv = new float[rings * segments * 2];
        byte[] rgba = new byte[rings * segments * 4];

        for (int y = 0; y < rings; y++)
        {
            float yFloat = y;
            float phiFloat = (yFloat / (rings - 1)) * GetPi();
            for (int x = 0; x < segments; x++)
            {
                float xFloat = x;
                float thetaFloat = (xFloat / (segments - 1)) * 2 * GetPi();
                float vxFloat = radius * Platform.Sin(phiFloat) * Platform.Cos(thetaFloat);
                float vyFloat = height * Platform.Cos(phiFloat);
                float vzFloat = radius * Platform.Sin(phiFloat) * Platform.Sin(thetaFloat);
                float uFloat = xFloat / (segments - 1);
                float vFloat = yFloat / (rings - 1);
                xyz[i * 3 + 0] = vxFloat;
                xyz[i * 3 + 1] = vyFloat;
                xyz[i * 3 + 2] = vzFloat;
                uv[i * 2 + 0] = uFloat;
                uv[i * 2 + 1] = vFloat;
                rgba[i * 4 + 0] = 255;
                rgba[i * 4 + 1] = 255;
                rgba[i * 4 + 2] = 255;
                rgba[i * 4 + 3] = 255;
                i++;
            }
        }
        ModelData data = new ModelData();
        data.SetVerticesCount(segments * rings);
        data.SetIndicesCount(segments * rings * 6);
        data.setXyz(xyz);
        data.setUv(uv);
        data.setRgba(rgba);
        data.setIndices(CalculateElements(radius, height, segments, rings));
        //data.setMode(DrawModeEnum.Triangles);
        return data;
    }
    public static int[] CalculateElements(float radius, float height, int segments, int rings)
    {
        int i = 0;
        // Load data into an element buffer or use them as offsets into the vertex array above.
        int[] data = new int[segments * rings * 6];

        for (int y = 0; y < rings - 1; y++)
        {
            for (int x = 0; x < segments - 1; x++)
            {
                data[i++] = ((y + 0) * segments + x);
                data[i++] = ((y + 1) * segments + x);
                data[i++] = ((y + 1) * segments + x + 1);

                data[i++] = ((y + 1) * segments + x + 1);
                data[i++] = ((y + 0) * segments + x + 1);
                data[i++] = ((y + 0) * segments + x);
            }
        }
        return data;
    }
}

//(-1,-1,-1) to (1,1,1)
public class WireframeCube
{
    public static ModelData Get()
    {
        ModelData m = new ModelData();
        m.setMode(DrawModeEnum.Lines);
        m.xyz = new float[3 * 4 * 6];
        m.uv = new float[2 * 4 * 6];
        m.rgba = new byte[4 * 4 * 6];
        m.indices = new int[8 * 6];

        DrawLineLoop(m,
                Vector3Ref.Create(-1, -1, -1),
                Vector3Ref.Create(-1, 1, -1),
                Vector3Ref.Create(1, 1, -1),
                Vector3Ref.Create(1, -1, -1)
            );
        DrawLineLoop(m,
                Vector3Ref.Create(-1, -1, -1),
                Vector3Ref.Create(1, -1, -1),
                Vector3Ref.Create(1, -1, 1),
                Vector3Ref.Create(-1, -1, 1)
            );
        DrawLineLoop(m,
                Vector3Ref.Create(-1, -1, -1),
                Vector3Ref.Create(-1, -1, 1),
                Vector3Ref.Create(-1, 1, 1),
                Vector3Ref.Create(-1, 1, -1)
            );
        DrawLineLoop(m,
                Vector3Ref.Create(-1, -1, 1),
                Vector3Ref.Create(1, -1, 1),
                Vector3Ref.Create(1, 1, 1),
                Vector3Ref.Create(-1, 1, 1)
            );
        DrawLineLoop(m,
                Vector3Ref.Create(-1, 1, -1),
                Vector3Ref.Create(-1, 1, 1),
                Vector3Ref.Create(1, 1, 1),
                Vector3Ref.Create(1, 1, -1)
            );
        DrawLineLoop(m,
                Vector3Ref.Create(1, -1, -1),
                Vector3Ref.Create(1, 1, -1),
                Vector3Ref.Create(1, 1, 1),
                Vector3Ref.Create(1, -1, 1)
            );

        return m;
    }
    static void DrawLineLoop(ModelData m, Vector3Ref p0, Vector3Ref p1, Vector3Ref p2, Vector3Ref p3)
    {
        int startVertex = m.GetVerticesCount();
        AddVertex(m, p0.X, p0.Y, p0.Z, 0, 0, Game.ColorFromArgb(255, 255, 255, 255));
        AddVertex(m, p1.X, p1.Y, p1.Z, 0, 0, Game.ColorFromArgb(255, 255, 255, 255));
        AddVertex(m, p2.X, p2.Y, p2.Z, 0, 0, Game.ColorFromArgb(255, 255, 255, 255));
        AddVertex(m, p3.X, p3.Y, p3.Z, 0, 0, Game.ColorFromArgb(255, 255, 255, 255));
        m.indices[m.indicesCount++] = startVertex + 0;
        m.indices[m.indicesCount++] = startVertex + 1;
        m.indices[m.indicesCount++] = startVertex + 1;
        m.indices[m.indicesCount++] = startVertex + 2;
        m.indices[m.indicesCount++] = startVertex + 2;
        m.indices[m.indicesCount++] = startVertex + 3;
        m.indices[m.indicesCount++] = startVertex + 3;
        m.indices[m.indicesCount++] = startVertex + 0;
    }

    static void AddVertex(ModelData model, float x, float y, float z, float u, float v, int color)
    {
        model.xyz[model.GetXyzCount() + 0] = x;
        model.xyz[model.GetXyzCount() + 1] = y;
        model.xyz[model.GetXyzCount() + 2] = z;
        model.uv[model.GetUvCount() + 0] = u;
        model.uv[model.GetUvCount() + 1] = v;
        model.rgba[model.GetRgbaCount() + 0] = Game.IntToByte(Game.ColorR(color));
        model.rgba[model.GetRgbaCount() + 1] = Game.IntToByte(Game.ColorG(color));
        model.rgba[model.GetRgbaCount() + 2] = Game.IntToByte(Game.ColorB(color));
        model.rgba[model.GetRgbaCount() + 3] = Game.IntToByte(Game.ColorA(color));
        model.verticesCount++;
    }
}
