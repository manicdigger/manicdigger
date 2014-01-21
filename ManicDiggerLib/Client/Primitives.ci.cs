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
