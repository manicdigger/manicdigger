using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace ManicDigger
{
    public interface IFrustumCulling
    {
        bool SphereInFrustum(float x, float y, float z, float radius);
        void CalcFrustumEquations();
    }
    public class FrustumCullingDummy : IFrustumCulling
    {
        #region IFrustumCulling Members
        public void CalcFrustumEquations()
        {
        }
        #endregion
        #region IFrustumCulling Members
        public bool SphereInFrustum(float x, float y, float z, float radius)
        {
            return true;
        }
        #endregion
    }
    //This is from Mark Morley's tutorial on frustum culling.
    //http://www.crownandcutlass.com/features/technicaldetails/frustum.html
    public class FrustumCulling : IFrustumCulling
    {
        [Inject]
        public IThe3d the3d { get; set; }
        float[,] frustum = new float[6, 4];
        public bool SphereInFrustum(float x, float y, float z, float radius)
        {
            int p;
            float d = 0;

            for (p = 0; p < 6; p++)
            {
                d = frustum[p, 0] * x + frustum[p, 1] * y + frustum[p, 2] * z + frustum[p, 3];
                if (d <= -radius)
                    return false;
            }
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
            Matrix4 matModelView = the3d.ModelViewMatrix;
            Matrix4 matProjection = the3d.ProjectionMatrix;
            Matrix4 matFrustum = matProjection;
            Matrix4.Mult(ref matModelView, ref matProjection, out matFrustum);

            unsafe
            {
                //fixed (float* clip1 = &matFrustum)
                float* clip1 = (float*)(&matFrustum);
                {
                    // Extract the numbers for the RIGHT plane
                    frustum[0, 0] = clip1[3] - clip1[0];
                    frustum[0, 1] = clip1[7] - clip1[4];
                    frustum[0, 2] = clip1[11] - clip1[8];
                    frustum[0, 3] = clip1[15] - clip1[12];

                    // Normalize the result
                    t = (float)Math.Sqrt(frustum[0, 0] * frustum[0, 0] + frustum[0, 1] * frustum[0, 1] + frustum[0, 2] * frustum[0, 2]);
                    frustum[0, 0] /= t;
                    frustum[0, 1] /= t;
                    frustum[0, 2] /= t;
                    frustum[0, 3] /= t;

                    // Extract the numbers for the LEFT plane
                    frustum[1, 0] = clip1[3] + clip1[0];
                    frustum[1, 1] = clip1[7] + clip1[4];
                    frustum[1, 2] = clip1[11] + clip1[8];
                    frustum[1, 3] = clip1[15] + clip1[12];

                    // Normalize the result
                    t = (float)Math.Sqrt(frustum[1, 0] * frustum[1, 0] + frustum[1, 1] * frustum[1, 1] + frustum[1, 2] * frustum[1, 2]);
                    frustum[1, 0] /= t;
                    frustum[1, 1] /= t;
                    frustum[1, 2] /= t;
                    frustum[1, 3] /= t;

                    // Extract the BOTTOM plane
                    frustum[2, 0] = clip1[3] + clip1[1];
                    frustum[2, 1] = clip1[7] + clip1[5];
                    frustum[2, 2] = clip1[11] + clip1[9];
                    frustum[2, 3] = clip1[15] + clip1[13];

                    // Normalize the result
                    t = (float)Math.Sqrt(frustum[2, 0] * frustum[2, 0] + frustum[2, 1] * frustum[2, 1] + frustum[2, 2] * frustum[2, 2]);
                    frustum[2, 0] /= t;
                    frustum[2, 1] /= t;
                    frustum[2, 2] /= t;
                    frustum[2, 3] /= t;

                    // Extract the TOP plane
                    frustum[3, 0] = clip1[3] - clip1[1];
                    frustum[3, 1] = clip1[7] - clip1[5];
                    frustum[3, 2] = clip1[11] - clip1[9];
                    frustum[3, 3] = clip1[15] - clip1[13];

                    // Normalize the result
                    t = (float)Math.Sqrt(frustum[3, 0] * frustum[3, 0] + frustum[3, 1] * frustum[3, 1] + frustum[3, 2] * frustum[3, 2]);
                    frustum[3, 0] /= t;
                    frustum[3, 1] /= t;
                    frustum[3, 2] /= t;
                    frustum[3, 3] /= t;

                    // Extract the FAR plane
                    frustum[4, 0] = clip1[3] - clip1[2];
                    frustum[4, 1] = clip1[7] - clip1[6];
                    frustum[4, 2] = clip1[11] - clip1[10];
                    frustum[4, 3] = clip1[15] - clip1[14];

                    // Normalize the result
                    t = (float)Math.Sqrt(frustum[4, 0] * frustum[4, 0] + frustum[4, 1] * frustum[4, 1] + frustum[4, 2] * frustum[4, 2]);
                    frustum[4, 0] /= t;
                    frustum[4, 1] /= t;
                    frustum[4, 2] /= t;
                    frustum[4, 3] /= t;

                    // Extract the NEAR plane
                    frustum[5, 0] = clip1[3] + clip1[2];
                    frustum[5, 1] = clip1[7] + clip1[6];
                    frustum[5, 2] = clip1[11] + clip1[10];
                    frustum[5, 3] = clip1[15] + clip1[14];

                    // Normalize the result
                    t = (float)Math.Sqrt(frustum[5, 0] * frustum[5, 0] + frustum[5, 1] * frustum[5, 1] + frustum[5, 2] * frustum[5, 2]);
                    frustum[5, 0] /= t;
                    frustum[5, 1] /= t;
                    frustum[5, 2] /= t;
                    frustum[5, 3] /= t;
                }
            }
        }
    }
}
