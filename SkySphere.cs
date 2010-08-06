using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK;

namespace ManicDigger
{
    //http://www.opentk.com/node/732
    //Re: [SL Multitexturing] - Only one texture with gluSphere
    //Posted Sunday, 22 March, 2009 - 23:50 by the Fiddler
    public class SkySphere
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct VertexP3N3T2
        {
            public Vector3 Position, Normal;
            public Vector2 TexCoord;
        }
        public VertexP3N3T2[] CalculateVertices(float radius, float height, int segments, int rings)
        {
            int i = 0;
            // Load data into a vertex buffer or a display list afterwards.
            var data = new VertexP3N3T2[rings * segments];

            for (double y = 0; y < rings; y++)
            {
                double phi = (y / (rings - 1)) * Math.PI;
                for (double x = 0; x < segments; x++)
                {
                    double theta = (x / (segments - 1)) * 2 * Math.PI;

                    Vector3 v = new Vector3()
                    {
                        X = (float)(radius * Math.Sin(phi) * Math.Cos(theta)),
                        Y = (float)(height * Math.Cos(phi)),
                        Z = (float)(radius * Math.Sin(phi) * Math.Sin(theta)),
                    };
                    Vector3 n = Vector3.Normalize(v);
                    // Horizontal texture projection
                    Vector2 uv = new Vector2()
                    {
                        X = (float)(x / (segments - 1)),
                        Y = (float)(y / (rings - 1))
                    };
                    // Using data[i++] causes i to be incremented multiple times in Mono 2.2 (bug #479506).
                    data[i] = new VertexP3N3T2() { Position = v, Normal = n, TexCoord = uv };
                    i++;

                    // Top - down texture projection.
                    //Vector2 uv = new Vector2()
                    //{
                    //    X = (float)(Math.Atan2(n.X, n.Z) / Math.PI / 2 + 0.5),
                    //    Y = (float)(Math.Asin(n.Y) / Math.PI / 2 + 0.5)
                    //};
                }
            }
            return data;
        }
        public ushort[] CalculateElements(float radius, float height, int segments, int rings)
        {
            int i = 0;
            // Load data into an element buffer or use them as offsets into the vertex array above.
            var data = new ushort[segments * rings * 6];

            for (int y = 0; y < rings - 1; y++)
            {
                for (int x = 0; x < segments - 1; x++)
                {
                    data[i++] = (ushort)((y + 0) * segments + x);
                    data[i++] = (ushort)((y + 1) * segments + x);
                    data[i++] = (ushort)((y + 1) * segments + x + 1);

                    data[i++] = (ushort)((y + 1) * segments + x + 1);
                    data[i++] = (ushort)((y + 0) * segments + x + 1);
                    data[i++] = (ushort)((y + 0) * segments + x);
                }
            }
            return data;
        }
    }
}
