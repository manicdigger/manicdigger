using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Renderers;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;

namespace ManicDigger
{
    //Eventually all calls to OpenGL should be here.
    //This class should become replaceable with DirectX.
    public class The3d : IThe3d, IDraw2d, IGetCameraMatrix
    {
        [Inject]
        public ITerrainTextures d_Terrain;
        [Inject]
        public Config3d d_Config3d;
        [Inject]
        public TextRenderer d_TextRenderer;
        [Inject]
        public IGetFilePath d_GetFile;
        [Inject]
        public IViewportSize d_ViewportSize;
        public bool ALLOW_NON_POWER_OF_TWO = false;
        public int LoadTexture(string filename)
        {
            Bitmap bmp = new Bitmap(filename);
            return LoadTexture(bmp);
        }
        //http://www.opentk.com/doc/graphics/textures/loading
        public int LoadTexture(Bitmap bmpArg)
        {
            Bitmap bmp = bmpArg;
            bool convertedbitmap = false;
            if ((!ALLOW_NON_POWER_OF_TWO) &&
                (!(BitTools.IsPowerOfTwo((uint)bmp.Width) && BitTools.IsPowerOfTwo((uint)bmp.Height))))
            {
                Bitmap bmp2 = new Bitmap((int)BitTools.NextPowerOfTwo((uint)bmp.Width),
                    (int)BitTools.NextPowerOfTwo((uint)bmp.Height));
                using (Graphics g = Graphics.FromImage(bmp2))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.DrawImage(bmp, 0, 0, bmp2.Width, bmp2.Height);
                }
                convertedbitmap = true;
                bmp = bmp2;
            }
            GL.Enable(EnableCap.Texture2D);
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            if (!d_Config3d.ENABLE_MIPMAPS)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            }
            else
            {
                //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); //DOES NOT WORK ON ATI GRAPHIC CARDS
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1); //DOES NOT WORK ON ???
                int[] MipMapCount = new int[1];
                GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureMaxLevel, out MipMapCount[0]);
                if (MipMapCount[0] == 0)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
                }
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 4);
            }
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            bmp.UnlockBits(bmp_data);

            GL.Enable(EnableCap.DepthTest);

            if (d_Config3d.ENABLE_TRANSPARENCY)
            {
                GL.Enable(EnableCap.AlphaTest);
                GL.AlphaFunc(AlphaFunction.Greater, 0.5f);
            }


            if (d_Config3d.ENABLE_TRANSPARENCY)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
                //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, new Color4(0, 0, 0, byte.MaxValue));
            }

            if (convertedbitmap)
            {
                bmp.Dispose();
            }
            return id;
        }
        #region IThe3d Members
        public Matrix4 ModelViewMatrix { get; set; }
        public Matrix4 ProjectionMatrix { get; set; }
        #endregion
        struct TextAndSize
        {
            public string text;
            public float size;
            public override int GetHashCode()
            {
                return text.GetHashCode() % size.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (obj is TextAndSize)
                {
                    TextAndSize other = (TextAndSize)obj;
                    return this.text == other.text && this.size == other.size;
                }
                return base.Equals(obj);
            }
        }
        Dictionary<TextAndSize, SizeF> textsizes = new Dictionary<TextAndSize, SizeF>();
        public SizeF TextSize(string text, float fontsize)
        {
            SizeF size;
            if (textsizes.TryGetValue(new TextAndSize() { text = text, size = fontsize }, out size))
            {
                return size;
            }
            using (Font font = new Font("Verdana", fontsize))
            {
                using (Bitmap bmp = new Bitmap(1, 1))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        size = g.MeasureString(text, font);
                    }
                }
            }
            textsizes[new TextAndSize() { text = text, size = fontsize }] = size;
            return size;
        }
        public class CachedTexture
        {
            public int textureId;
            public SizeF size;
            public DateTime lastuse;
        }
        public Dictionary<Text, CachedTexture> cachedTextTextures = new Dictionary<Text, CachedTexture>();
        CachedTexture MakeTextTexture(Text t)
        {
            Bitmap bmp = d_TextRenderer.MakeTextTexture(t);
            int texture = LoadTexture(bmp);
            return new CachedTexture() { textureId = texture, size = bmp.Size };
        }
        void DeleteUnusedCachedTextTextures()
        {
            List<Text> toremove = new List<Text>();
            DateTime now = DateTime.UtcNow;
            foreach (var k in cachedTextTextures)
            {
                var ct = k.Value;
                if ((now - ct.lastuse).TotalSeconds > 1)
                {
                    GL.DeleteTexture(ct.textureId);
                    toremove.Add(k.Key);
                }
            }
            foreach (var k in toremove)
            {
                cachedTextTextures.Remove(k);
            }
        }
        public void Draw2dText(string text, float x, float y, float fontsize, Color? color)
        {
            if (text == null || text.Trim() == "")
            {
                return;
            }
            if (color == null) { color = Color.White; }
            var t = new Text();
            t.text = text;
            t.color = color.Value;
            t.fontsize = fontsize;
            CachedTexture ct;
            if (!cachedTextTextures.ContainsKey(t))
            {
                ct = MakeTextTexture(t);
                if (ct == null)
                {
                    return;
                }
                cachedTextTextures.Add(t, ct);
            }
            ct = cachedTextTextures[t];
            ct.lastuse = DateTime.UtcNow;
            GL.Disable(EnableCap.AlphaTest);
            Draw2dTexture(ct.textureId, x, y, ct.size.Width, ct.size.Height, null);
            GL.Enable(EnableCap.AlphaTest);
            DeleteUnusedCachedTextTextures();
        }
        public void Draw2dBitmapFile(string filename, float x1, float y1, float width, float height)
        {
            if (!textures.ContainsKey(filename))
            {
                textures[filename] = LoadTexture(d_GetFile.GetFile(filename));
            }
            Draw2dTexture(textures[filename], x1, y1, width, height, null);
        }
        public void Draw2dTexture(int textureid, float x1, float y1, float width, float height, int? inAtlasId)
        {
            Draw2dTexture(textureid, x1, y1, width, height, inAtlasId, Color.White);
        }
        public void Draw2dTexture(int textureid, float x1, float y1, float width, float height, int? inAtlasId, Color color)
        {
            RectangleF rect;
            if (inAtlasId == null)
            {
                rect = new RectangleF(0, 0, 1, 1);
            }
            else
            {
                rect = TextureAtlas.TextureCoords2d(inAtlasId.Value, d_Terrain.texturesPacked);
            }
            GL.PushAttrib(AttribMask.ColorBufferBit);
            GL.Color3(color);
            GL.BindTexture(TextureTarget.Texture2D, textureid);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Begin(BeginMode.Quads);
            float x2 = x1 + width;
            float y2 = y1 + height;
            GL.TexCoord2(rect.Right, rect.Bottom); GL.Vertex2(x2, y2);
            GL.TexCoord2(rect.Right, rect.Top); GL.Vertex2(x2, y1);
            GL.TexCoord2(rect.Left, rect.Top); GL.Vertex2(x1, y1);
            GL.TexCoord2(rect.Left, rect.Bottom); GL.Vertex2(x1, y2);
            GL.End();
            GL.Enable(EnableCap.DepthTest);
            GL.PopAttrib();
        }
        VertexPositionTexture[] draw2dtexturesVertices;
        ushort[] draw2dtexturesIndices;
        int draw2dtexturesMAX = 512;
        public void Draw2dTextures(Draw2dData[] todraw, int textureid)
        {
            GL.PushAttrib(AttribMask.ColorBufferBit);
            GL.BindTexture(TextureTarget.Texture2D, textureid);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);

            VertexPositionTexture[] vertices;
            ushort[] indices;
            if (todraw.Length >= draw2dtexturesMAX)
            {
                vertices = new VertexPositionTexture[todraw.Length * 4];
                indices = new ushort[todraw.Length * 4];
            }
            else
            {
                if (draw2dtexturesVertices == null)
                {
                    draw2dtexturesVertices = new VertexPositionTexture[draw2dtexturesMAX * 4];
                    draw2dtexturesIndices = new ushort[draw2dtexturesMAX * 4];
                }
                vertices = draw2dtexturesVertices;
                indices = draw2dtexturesIndices;
            }
            ushort i = 0;
            foreach (Draw2dData v in todraw)
            {
                RectangleF rect;
                if (v.inAtlasId == null)
                {
                    rect = new RectangleF(0, 0, 1, 1);
                }
                else
                {
                    rect = TextureAtlas.TextureCoords2d(v.inAtlasId.Value, d_Terrain.texturesPacked);
                }
                float x2 = v.x1 + v.width;
                float y2 = v.y1 + v.height;
                vertices[i] = new VertexPositionTexture(x2, y2, 0, rect.Right, rect.Bottom, v.color);
                vertices[i + 1] = new VertexPositionTexture(x2, v.y1, 0, rect.Right, rect.Top, v.color);
                vertices[i + 2] = new VertexPositionTexture(v.x1, v.y1, 0, rect.Left, rect.Top, v.color);
                vertices[i + 3] = new VertexPositionTexture(v.x1, y2, 0, rect.Left, rect.Bottom, v.color);
                indices[i] = i;
                indices[i + 1] = (ushort)(i + 1);
                indices[i + 2] = (ushort)(i + 2);
                indices[i + 3] = (ushort)(i + 3);
                i += 4;
            }
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            unsafe
            {
                fixed (VertexPositionTexture* p = vertices)
                {
                    GL.VertexPointer(3, VertexPointerType.Float, StrideOfVertices, (IntPtr)(0 + (byte*)p));
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, StrideOfVertices, (IntPtr)(12 + (byte*)p));
                    GL.ColorPointer(4, ColorPointerType.UnsignedByte, StrideOfVertices, (IntPtr)(20 + (byte*)p));
                    GL.DrawElements(BeginMode.Quads, i, DrawElementsType.UnsignedShort, indices);
                }
            }
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.ColorArray);

            GL.Enable(EnableCap.DepthTest);
            GL.PopAttrib();
        }
        int strideofvertices = -1;
        int StrideOfVertices
        {
            get
            {
                if (strideofvertices == -1) strideofvertices = BlittableValueType.StrideOf(new VertexPositionTexture());
                return strideofvertices;
            }
        }
        public int WhiteTexture()
        {
            if (whitetexture == -1)
            {
                var bmp = new Bitmap(1, 1);
                bmp.SetPixel(0, 0, Color.White);
                whitetexture = LoadTexture(bmp);
            }
            return whitetexture;
        }
        int whitetexture = -1;
        Dictionary<string, int> textures = new Dictionary<string, int>();
        public float fov = MathHelper.PiOver3;
        public void Set3dProjection()
        {
            Set3dProjection(zfar);
        }
        public void Set3dProjection(float zfar)
        {
            float aspect_ratio = d_ViewportSize.Width / (float)d_ViewportSize.Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(fov, aspect_ratio, znear, zfar);
            ProjectionMatrix = perpective;
            //Matrix4 perpective = Matrix4.CreateOrthographic(800 * 0.10f, 600 * 0.10f, 0.0001f, zfar);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
        }
        public float znear = 0.1f;
        float zfar { get { return ENABLE_ZFAR ? d_Config3d.viewdistance : 99999; } }
        public bool ENABLE_ZFAR = true;
    }
}
