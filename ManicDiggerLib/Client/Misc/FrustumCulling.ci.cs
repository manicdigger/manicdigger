//This is from Mark Morley's tutorial on frustum culling.
//http://www.crownandcutlass.com/features/technicaldetails/frustum.html
//"This page and its contents are Copyright 2000 by Mark Morley
//Unless otherwise noted, you may use any and all code examples provided herein in any way you want.
//All other content, including but not limited to text and images, may not be reproduced without consent.
//This file was last edited on Wednesday, 24-Jan-2001 13:24:38 PST"
public class FrustumCulling
{
    internal GamePlatform platform;
    internal IGetCameraMatrix d_GetCameraMatrix;
    float frustum00;
    float frustum01;
    float frustum02;
    float frustum03;

    float frustum10;
    float frustum11;
    float frustum12;
    float frustum13;

    float frustum20;
    float frustum21;
    float frustum22;
    float frustum23;

    float frustum30;
    float frustum31;
    float frustum32;
    float frustum33;

    float frustum40;
    float frustum41;
    float frustum42;
    float frustum43;

    float frustum50;
    float frustum51;
    float frustum52;
    float frustum53;
    public bool SphereInFrustum(float x, float y, float z, float radius)
    {
        float d = 0;

        d = frustum00 * x + frustum01 * y + frustum02 * z + frustum03;
        if (d <= -radius)
            return false;
        d = frustum10 * x + frustum11 * y + frustum12 * z + frustum13;
        if (d <= -radius)
            return false;
        d = frustum20 * x + frustum21 * y + frustum22 * z + frustum23;
        if (d <= -radius)
            return false;
        d = frustum30 * x + frustum31 * y + frustum32 * z + frustum33;
        if (d <= -radius)
            return false;
        d = frustum40 * x + frustum41 * y + frustum42 * z + frustum43;
        if (d <= -radius)
            return false;
        d = frustum50 * x + frustum51 * y + frustum52 * z + frustum53;
        if (d <= -radius)
            return false;

        return true;
    }
    /// <summary>
    /// Calculating the frustum planes.
    /// </summary>
    /// <remarks>
    /// From the current OpenGL modelview and projection matrices,
    /// calculate the frustum plane equations (Ax+By+Cz+D=0, n=(A,B,C))
    /// The equations can then be used to see on which side points are.
    /// </remarks>
    public void CalcFrustumEquations()
    {
        float t;

        // Retrieve matrices from OpenGL
        float[] matModelView = d_GetCameraMatrix.GetModelViewMatrix();
        float[] matProjection = d_GetCameraMatrix.GetProjectionMatrix();
        float[] matFrustum = Mat4.Create();
        //Matrix4.Mult(ref matModelView, ref matProjection, out matFrustum);
        Mat4.Multiply(matFrustum, matProjection, matModelView);

        //unsafe
        {
            //fixed (float* clip1 = &matFrustum)
            //float* clip1 = (float*)(&matFrustum);
            float[] clip1 = matFrustum;
            {
                // Extract the numbers for the RIGHT plane
                frustum00 = clip1[3] - clip1[0];
                frustum01 = clip1[7] - clip1[4];
                frustum02 = clip1[11] - clip1[8];
                frustum03 = clip1[15] - clip1[12];

                // Normalize the result
                t = platform.MathSqrt(frustum00 * frustum00 + frustum01 * frustum01 + frustum02 * frustum02);
                frustum00 /= t;
                frustum01 /= t;
                frustum02 /= t;
                frustum03 /= t;

                // Extract the numbers for the LEFT plane
                frustum10 = clip1[3] + clip1[0];
                frustum11 = clip1[7] + clip1[4];
                frustum12 = clip1[11] + clip1[8];
                frustum13 = clip1[15] + clip1[12];

                // Normalize the result
                t = platform.MathSqrt(frustum10 * frustum10 + frustum11 * frustum11 + frustum12 * frustum12);
                frustum10 /= t;
                frustum11 /= t;
                frustum12 /= t;
                frustum13 /= t;

                // Extract the BOTTOM plane
                frustum20 = clip1[3] + clip1[1];
                frustum21 = clip1[7] + clip1[5];
                frustum22 = clip1[11] + clip1[9];
                frustum23 = clip1[15] + clip1[13];

                // Normalize the result
                t = platform.MathSqrt(frustum20 * frustum20 + frustum21 * frustum21 + frustum22 * frustum22);
                frustum20 /= t;
                frustum21 /= t;
                frustum22 /= t;
                frustum23 /= t;

                // Extract the TOP plane
                frustum30 = clip1[3] - clip1[1];
                frustum31 = clip1[7] - clip1[5];
                frustum32 = clip1[11] - clip1[9];
                frustum33 = clip1[15] - clip1[13];

                // Normalize the result
                t = platform.MathSqrt(frustum30 * frustum30 + frustum31 * frustum31 + frustum32 * frustum32);
                frustum30 /= t;
                frustum31 /= t;
                frustum32 /= t;
                frustum33 /= t;

                // Extract the FAR plane
                frustum40 = clip1[3] - clip1[2];
                frustum41 = clip1[7] - clip1[6];
                frustum42 = clip1[11] - clip1[10];
                frustum43 = clip1[15] - clip1[14];

                // Normalize the result
                t = platform.MathSqrt(frustum40 * frustum40 + frustum41 * frustum41 + frustum42 * frustum42);
                frustum40 /= t;
                frustum41 /= t;
                frustum42 /= t;
                frustum43 /= t;

                // Extract the NEAR plane
                frustum50 = clip1[3] + clip1[2];
                frustum51 = clip1[7] + clip1[6];
                frustum52 = clip1[11] + clip1[10];
                frustum53 = clip1[15] + clip1[14];

                // Normalize the result
                t = platform.MathSqrt(frustum50 * frustum50 + frustum51 * frustum51 + frustum52 * frustum52);
                frustum50 /= t;
                frustum51 /= t;
                frustum52 /= t;
                frustum53 /= t;
            }
        }
    }
}

public abstract class IGetCameraMatrix
{
    public abstract float[] GetModelViewMatrix();
    public abstract float[] GetProjectionMatrix();
}
