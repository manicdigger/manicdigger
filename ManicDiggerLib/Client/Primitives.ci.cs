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
