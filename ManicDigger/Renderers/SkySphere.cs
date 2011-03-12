using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace ManicDigger
{
    //http://www.opentk.com/node/732
    //Re: [SL Multitexturing] - Only one texture with gluSphere
    //Posted Sunday, 22 March, 2009 - 23:50 by the Fiddler
    public class SkySphere
    {
        [Inject]
        public MeshBatcher meshbatcher;
        [Inject]
        public ILocalPlayerPosition playerpos;
        [Inject]
        public IThe3d the3d;
        public int SkyTexture = -1;
        int SkyMeshId = -1;
        public void Draw()
        {
            if (SkyTexture == -1)
            {
                throw new InvalidOperationException();
            }
            int size = 1000;
            if (SkyMeshId == -1)
            {
                ushort[] indices = CalculateElements(size, size, 20, 20);
                VertexPositionTexture[] vertices = CalculateVertices(size, size, 20, 20);
                SkyMeshId = meshbatcher.Add(indices, indices.Length, vertices, vertices.Length
                    , false, SkyTexture, new Vector3(0, 0, 0), size * 2);
            }
            the3d.Set3dProjection(size * 2);
            GL.MatrixMode(MatrixMode.Modelview);
            meshbatcher.BindTexture = false;
            GL.PushMatrix();
            GL.Translate(playerpos.LocalPlayerPosition);
            GL.Color3(Color.White);
            GL.BindTexture(TextureTarget.Texture2D, SkyTexture);
            meshbatcher.Draw(playerpos.LocalPlayerPosition);
            GL.PopMatrix();
            the3d.Set3dProjection();
        }
        public VertexPositionTexture[] CalculateVertices(float radius, float height, int segments, int rings)
        {
            int i = 0;
            // Load data into a vertex buffer or a display list afterwards.
            var data = new VertexPositionTexture[rings * segments];

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
                    //Vector3 n = Vector3.Normalize(v);
                    // Horizontal texture projection
                    Vector2 uv = new Vector2()
                    {
                        X = (float)(x / (segments - 1)),
                        Y = (float)(y / (rings - 1))
                    };
                    // Using data[i++] causes i to be incremented multiple times in Mono 2.2 (bug #479506).
                    data[i] = new VertexPositionTexture()
                    {
                        Position = v,
                        //Normal = n,
                        u = uv.X,
                        v = uv.Y,
                        a = byte.MaxValue,
                        r = byte.MaxValue,
                        g = byte.MaxValue,
                        b = byte.MaxValue,
                    };
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
