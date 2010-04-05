using System;
using System.Collections.Generic;
using System.Text;
using DependencyInjection;
using OpenTK;
using ManicDigger.Collisions;
using System.Drawing;
using System.Threading;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace ManicDigger
{
    public class TextureAtlasRef
    {
        public int TextureId;
        public int TextureInAtlasId;
    }
    public interface ITerrainDrawer
    {
        void Start();
        void Draw();
        void UpdateAllTiles();
        void UpdateTile(int x, int y, int z);
        int TrianglesCount();
        int texturesPacked { get; }
        int terrainTexture { get; }
    }
    public class TextureAtlas
    {
        //warning! buffer zone!
        public static RectangleF TextureCoords(int textureId, int texturesPacked)
        {
            float bufferRatio = 0.0f;//0.1
            RectangleF r = new RectangleF();
            r.Y = (1.0f / texturesPacked * (int)(textureId / texturesPacked)) + ((bufferRatio) * (1.0f / texturesPacked));
            r.X = (1.0f / texturesPacked * (textureId % texturesPacked)) + ((bufferRatio) * (1.0f / texturesPacked));
            r.Width = (1f - 2f * bufferRatio) * 1.0f / texturesPacked;
            r.Height = (1f - 2f * bufferRatio) * 1.0f / texturesPacked;
            return r;
        }
    }
    public class TerrainDrawer : ITerrainDrawer
    {
        [Inject]
        public IThe3d the3d { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public Config3d config3d { get; set; }
        [Inject]
        public IMapStorage mapstorage { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public IGameExit exit { get; set; }
        [Inject]
        public ILocalPlayerPosition localplayerposition { get; set; }
        public struct Vbo
        {
            public int VboID, EboID, NumElements;
            public Box3D box;
        }
        //List<Vbo> vbo = new List<Vbo>();
        Dictionary<Vector3, ICollection<Vbo>> vbo = new Dictionary<Vector3, ICollection<Vbo>>();
        Queue<List<VerticesIndicesToLoad>> vbotoload = new Queue<List<VerticesIndicesToLoad>>();
        /// <summary>
        /// Background thread generating vertices and indices.
        /// Actual vbo loading must be done in the main thread (it is fast).
        /// </summary>
        void bgworker()
        {
            for (; ; )
            {
                if (exit.exit)
                {
                    return;
                }
                Vector3? pp = null;
                lock (toupdate)
                {
                    if (toupdate.Count > 0)
                    {
                        pp = toupdate.Dequeue();
                    }
                }
                if (pp != null)
                {
                    Vector3 p = pp.Value;
                    //lock (clientgame.mapupdate)//does not work, clientgame can get replaced
                    {
                        //try
                        {
                            IEnumerable<VerticesIndicesToLoad> q = MakeChunk((int)p.X * buffersize, (int)p.Y * buffersize, (int)p.Z * buffersize, buffersize);
                            List<Vector3> toremove = new List<Vector3>();
                            if (q != null)
                            {
                                lock (vbotoload)
                                {
                                    //foreach (var qq in q)
                                    {
                                        vbotoload.Enqueue(ToList(q));
                                    }
                                }
                            }
                        }
                        //catch
                        //{ }
                    }
                }
                Thread.Sleep(0);
            }
        }
        List<T> ToList<T>(IEnumerable<T> v)
        {
            List<T> l = new List<T>();
            foreach (T vv in v)
            {
                l.Add(vv);
            }
            return l;
        }
        Queue<Vector3> toupdate = new Queue<Vector3>();
        int buffersize = 32; //32,45
        public void UpdateTile(int x, int y, int z)
        {
            Vector3 bufferpos = new Vector3(x / buffersize, y / buffersize, z / buffersize);
            lock (toupdate)
            {
                //if we are on a chunk boundary, then update near chunks too.
                if (x % buffersize == 0)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(-1, 0, 0));
                }
                if (x % buffersize == buffersize - 1)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(1, 0, 0));
                }
                if (y % buffersize == 0)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, -1, 0));
                }
                if (y % buffersize == buffersize - 1)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, 1, 0));
                }
                if (z % buffersize == 0)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, 0, -1));
                }
                if (z % buffersize == buffersize - 1)
                {
                    toupdate.Enqueue(bufferpos + new Vector3(0, 0, 1));
                }
                toupdate.Enqueue(bufferpos);///bbb z / buffersize
            }
        }
        public void UpdateAllTiles()
        {
            lock (toupdate)
            {
                toupdate.Clear();
            }
            lock (vbotoload)
            {
                vbotoload.Clear();
            }
            foreach (var v in vbo)
            {
                foreach (var vv in v.Value)
                {
                    var a = vv.VboID;
                    var b = vv.EboID;
                    GL.DeleteBuffers(1, ref a);
                    GL.DeleteBuffers(1, ref b);
                }
            }
            vbo.Clear();
            for (int i = 0; i < 1; i++)
                for (int x = 0; x < mapstorage.MapSizeX / buffersize; x++)
                    for (int y = 0; y < mapstorage.MapSizeY / buffersize; y++)
                        for (int z = 0; z < mapstorage.MapSizeZ / buffersize; z++)//bbb mapsizez / buffersize
                            //DrawUpdateChunk(x, y, z);
                            lock (toupdate)
                            {
                                toupdate.Enqueue(new Vector3(x, y, z));
                            }
        }
        public int texturesPacked { get { return 16; } }//16x16
        bool DONOTDRAWEDGES = true;
        private bool IsValidPos(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0)
            {
                return false;
            }
            if (x >= mapstorage.MapSizeX || y >= mapstorage.MapSizeY || z >= mapstorage.MapSizeZ)
            {
                return false;
            }
            return true;
        }
        bool IsTileEmptyForDrawing(int x, int y, int z)
        {
            if (!IsValidPos(x, y, z))
            {
                return true;
            }
            return mapstorage.Map[x, y, z] == (byte)TileTypeMinecraft.Empty;
        }
        bool IsTileEmptyForDrawingOrTransparent(int x, int y, int z, int adjacenttiletype)
        {
            if (!config3d.ENABLE_TRANSPARENCY)
            {
                return IsTileEmptyForDrawing(x, y, z);
            }
            if (!IsValidPos(x, y, z))
            {
                return true;
            }
            return mapstorage.Map[x, y, z] == data.TileIdEmpty
                || (mapstorage.Map[x, y, z] == data.TileIdWater
                 && !(adjacenttiletype == data.TileIdWater))
                || mapstorage.Map[x, y, z] == (byte)TileTypeMinecraft.Glass
                || mapstorage.Map[x, y, z] == (byte)TileTypeMinecraft.InfiniteWaterSource
                || mapstorage.Map[x, y, z] == (byte)TileTypeMinecraft.Leaves;
        }
        public List<VerticesIndicesToLoad> MakeChunk(int startx, int starty, int startz, int size)
        {
            List<VerticesIndicesToLoad> list = new List<VerticesIndicesToLoad>();
            List<ushort> myelements = new List<ushort>();
            List<VertexPositionTexture> myvertices = new List<VertexPositionTexture>();
            for (int x = startx; x < startx + size; x++)
                for (int y = starty; y < starty + size; y++)
                    for (int z = startz; z < startz + size; z++)//bbb startz+size
                    {
                        //if (x == 0 && z == 31 & y == 128)
                        {
                        }
                        if (IsTileEmptyForDrawing(x, y, z)) { continue; }
                        var tt = mapstorage.Map[x, y, z];
                        bool drawtop = IsTileEmptyForDrawingOrTransparent(x, y, z + 1, tt);
                        bool drawbottom = IsTileEmptyForDrawingOrTransparent(x, y, z - 1, tt);
                        bool drawfront = IsTileEmptyForDrawingOrTransparent(x - 1, y, z, tt);
                        bool drawback = IsTileEmptyForDrawingOrTransparent(x + 1, y, z, tt);
                        bool drawleft = IsTileEmptyForDrawingOrTransparent(x, y - 1, z, tt);
                        bool drawright = IsTileEmptyForDrawingOrTransparent(x, y + 1, z, tt);
                        if (DONOTDRAWEDGES)
                        {
                            //if the game is fillrate limited, then this makes it much faster.
                            //(39fps vs vsync 75fps)
                            //bbb.
                            if (z == 0) { drawbottom = false; }
                            if (x == 0) { drawfront = false; }
                            if (x == 256 - 1) { drawback = false; }
                            if (y == 0) { drawleft = false; }
                            if (y == 256 - 1) { drawright = false; }
                        }
                        //top
                        if (drawtop)
                        {
                            int sidetexture = data.GetTileTextureId(mapstorage.Map[x, y, z], TileSide.Top);
                            RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z + 1.0f, y + 0.0f, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z + 1.0f, y + 1.0f, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z + 1.0f, y + 0.0f, texrec.Right, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z + 1.0f, y + 1.0f, texrec.Right, texrec.Bottom));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //bottom - same as top, but z is 1 less.
                        if (drawbottom)
                        {
                            int sidetexture = data.GetTileTextureId(mapstorage.Map[x, y, z], TileSide.Bottom);
                            RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 0.0f, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 1.0f, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 0.0f, texrec.Right, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 1.0f, texrec.Right, texrec.Bottom));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //front
                        if (drawfront)
                        {
                            int sidetexture = data.GetTileTextureId(mapstorage.Map[x, y, z], TileSide.Front);
                            RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 0, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 1, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //back - same as front, but x is 1 greater.
                        if (drawback)
                        {//todo fix tcoords
                            int sidetexture = data.GetTileTextureId(mapstorage.Map[x, y, z], TileSide.Back);
                            RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 1, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 0, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 1, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        if (drawleft)
                        {
                            int sidetexture = data.GetTileTextureId(mapstorage.Map[x, y, z], TileSide.Left);
                            RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 0, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 0, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        //right - same as left, but y is 1 greater.
                        if (drawright)
                        {//todo fix tcoords
                            int sidetexture = data.GetTileTextureId(mapstorage.Map[x, y, z], TileSide.Right);
                            RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                            short lastelement = (short)myvertices.Count;
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1, texrec.Left, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 1, texrec.Left, texrec.Top));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 1, texrec.Right, texrec.Bottom));
                            myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 1, texrec.Right, texrec.Top));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 0));
                            myelements.Add((ushort)(lastelement + 2));
                            myelements.Add((ushort)(lastelement + 3));
                            myelements.Add((ushort)(lastelement + 1));
                            myelements.Add((ushort)(lastelement + 2));
                        }
                        if (myvertices.Count > ushort.MaxValue)
                        {
                            var aa = myelements.ToArray();
                            var bb = myvertices.ToArray();
                            list.Add(new VerticesIndicesToLoad()
                            {
                                position = new Vector3(startx / size, starty / size, startz / size),
                                indices = aa,
                                vertices = bb,
                            });
                            myelements = new List<ushort>();
                            myvertices = new List<VertexPositionTexture>();
                        }
                    }
            if (myelements.Count != 0)
            {
                var a = myelements.ToArray();
                var b = myvertices.ToArray();
                list.Add(new VerticesIndicesToLoad()
                {
                    position = new Vector3(startx / size, starty / size, startz / size),
                    indices = a,
                    vertices = b,
                });
            }
            return list;
        }
        public void Start()
        {
            new Thread(bgworker).Start();
            GL.Enable(EnableCap.Texture2D);
            terrainTexture = the3d.LoadTexture(getfile.GetFile("terrain.png"));
        }
        public int terrainTexture { get; set; }
        float chunkupdateframecounter = 0;
        float vboupdatesperframe = 0.5f;
        public void Draw()
        {
            chunkupdateframecounter += vboupdatesperframe;
            while (chunkupdateframecounter >= 1)
            {
                chunkupdateframecounter -= 1;
                List<VerticesIndicesToLoad> v = null;
                lock (vbotoload)
                {
                    if (vbotoload.Count > 0)
                    {
                        v = vbotoload.Dequeue();
                    }
                }
                if (v != null && v.Count > 0)
                {
                    List<Vbo> vbolist = new List<Vbo>();
                    foreach (var vv in v)
                    {
                        var vbo1 = LoadVBO(vv.vertices, vv.indices);
                        foreach (var vvv in vv.vertices)
                        {
                            vbo1.box.AddPoint(vvv.Position.X, vvv.Position.Y, vvv.Position.Z);
                        }
                        vbolist.Add(vbo1);
                    }
                    if (!vbo.ContainsKey(v[0].position))
                    {
                        vbo[v[0].position] = new List<Vbo>();
                    }
                    //delete old vbo
                    vbo[v[0].position] = vbolist;
                    //DrawUpdateChunk(((int)v.X), ((int)v.Y), ((int)v.Z));
                }
            }
            GL.BindTexture(TextureTarget.Texture2D, terrainTexture);
            GL.Color3(terraincolor);
            var z = new List<Vbo>(VisibleVbo());
            if (z.Count != lastvisiblevbo && vbotoload.Count == 0)
            {
                Console.WriteLine("Hardware buffers: " + z.Count);
                lastvisiblevbo = z.Count;
            }
            z.Sort(f);
            foreach (var k in z)
            {
                Draw(k);
            }
            if (ENABLE_WATER)
            {
                DrawWater();
            }
            DrawMapEdges();
        }
        public bool ENABLE_WATER = true;
        int? watertexture;
        int? rocktexture;
        bool waternotfoundwritten = false;
        bool rocknotfoundwritten = false;
        Color terraincolor { get { return localplayerposition.Swimming ? Color.FromArgb(255, 100, 100, 255) : Color.White; } }
        private void DrawWater()
        {
            if (waternotfoundwritten) { return; }
            if (watertexture == null)
            {
                try
                { watertexture = the3d.LoadTexture(getfile.GetFile("water.png")); }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("water.png not found."); waternotfoundwritten = true; return;
                }
            }
            GL.BindTexture(TextureTarget.Texture2D, watertexture.Value);
            GL.Enable(EnableCap.Texture2D);
            GL.Color3(terraincolor);
            GL.Begin(BeginMode.Quads);
            foreach (Rectangle r in AroundMap())
            {
                DrawWaterQuad(r.X, r.Y, r.Width, r.Height,
                    mapstorage.WaterLevel);
            }
            GL.End();
        }
        private void DrawMapEdges()
        {
            if (rocknotfoundwritten) { return; }
            if (rocktexture == null)
            {
                try
                { rocktexture = the3d.LoadTexture(getfile.GetFile("rock.png")); }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("rock.png not found."); rocknotfoundwritten = true; return;
                }
            }
            GL.BindTexture(TextureTarget.Texture2D, rocktexture.Value);
            GL.Enable(EnableCap.Texture2D);
            GL.Color3(terraincolor);
            GL.Begin(BeginMode.Quads);
            foreach (IEnumerable<Point> r in MapEdges())
            {
                List<Point> rr = new List<Point>(r);
                DrawRockQuad(rr[0].X, rr[0].Y, rr[1].X, rr[1].Y,
                    mapstorage.WaterLevel - 2);
            }
            foreach (Rectangle r in AroundMap())
            {
                DrawWaterQuad(r.X, r.Y, r.Width, r.Height,
                    mapstorage.WaterLevel - 2);
            }
            DrawWaterQuad(0, 0, mapstorage.MapSizeX, mapstorage.MapSizeY, 0);
            GL.End();
        }
        private IEnumerable<IEnumerable<Point>> MapEdges()
        {
            yield return new Point[] { new Point(0, 0), new Point(mapstorage.MapSizeX, 0) };
            yield return new Point[] { new Point(mapstorage.MapSizeX, 0), new Point(mapstorage.MapSizeX, mapstorage.MapSizeY) };
            yield return new Point[] { new Point(mapstorage.MapSizeX, mapstorage.MapSizeY), new Point(0, mapstorage.MapSizeY) };
            yield return new Point[] { new Point(0, mapstorage.MapSizeY), new Point(0, 0) };
        }
        private void DrawRockQuad(int x1, int y1, int x2, int y2, float height)
        {
            RectangleF rect = new RectangleF(0, 0, Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1)), height);
            GL.TexCoord2(rect.Right, rect.Bottom); GL.Vertex3(x2, 0, y2);
            GL.TexCoord2(rect.Right, rect.Top); GL.Vertex3(x2, height, y2);
            GL.TexCoord2(rect.Left, rect.Top); GL.Vertex3(x1, height, y1);
            GL.TexCoord2(rect.Left, rect.Bottom); GL.Vertex3(x1, 0, y1);
        }
        int watersizex = 10 * 1000;
        int watersizey = 10 * 1000;
        IEnumerable<Rectangle> AroundMap()
        {
            yield return new Rectangle(-watersizex, -watersizey, mapstorage.MapSizeX + watersizex * 2, watersizey);
            yield return new Rectangle(-watersizex, mapstorage.MapSizeY, mapstorage.MapSizeX + watersizex * 2, watersizey);
            yield return new Rectangle(-watersizex, 0, watersizex, mapstorage.MapSizeY);
            yield return new Rectangle(mapstorage.MapSizeX, 0, watersizex, mapstorage.MapSizeY);
        }
        void DrawWaterQuad(float x1, float y1, float width, float height, float z1)
        {
            RectangleF rect = new RectangleF(0, 0, 1 * width, 1 * height);
            float x2 = x1 + width;
            float y2 = y1 + height;
            GL.TexCoord2(rect.Right, rect.Bottom); GL.Vertex3(x2, z1, y2);
            GL.TexCoord2(rect.Right, rect.Top); GL.Vertex3(x2, z1, y1);
            GL.TexCoord2(rect.Left, rect.Top); GL.Vertex3(x1, z1, y1);
            GL.TexCoord2(rect.Left, rect.Bottom); GL.Vertex3(x1, z1, y2);
        }
        int lastvisiblevbo = 0;
        private void DeleteVbo(Vbo pp)
        {
        }
        int f(Vbo a, Vbo b)
        {
            var aa = (a.box.Center() - localplayerposition.LocalPlayerPosition).Length;
            var bb = (b.box.Center() - localplayerposition.LocalPlayerPosition).Length;
            return aa.CompareTo(bb);
        }
        public IEnumerable<Vbo> VisibleVbo()
        {
            foreach (var k in vbo)
            {
                foreach (var kk in k.Value)
                {
                    if (!config3d.ENABLE_VISIBILITY_CULLING || (kk.box.Center() - localplayerposition.LocalPlayerPosition).Length < config3d.viewdistance)
                    {
                        yield return kk;
                    }
                }
            }
        }
        int strideofvertices = -1;
        int StrideOfVertices
        {
            get
            {
                if (strideofvertices == -1) strideofvertices = BlittableValueType.StrideOf(CubeVertices);
                return strideofvertices;
            }
        }
        Vbo LoadVBO<TVertex>(TVertex[] vertices, ushort[] elements) where TVertex : struct
        {
            Vbo handle = new Vbo();
            int size;

            // To create a VBO:
            // 1) Generate the buffer handles for the vertex and element buffers.
            // 2) Bind the vertex buffer handle and upload your vertex data. Check that the buffer was uploaded correctly.
            // 3) Bind the element buffer handle and upload your element data. Check that the buffer was uploaded correctly.

            GL.GenBuffers(1, out handle.VboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * StrideOfVertices), vertices,
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * StrideOfVertices != size)
                throw new ApplicationException("Vertex data not uploaded correctly");

            GL.GenBuffers(1, out handle.EboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(ushort)), elements,//aaa sizeof(short)
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (elements.Length * sizeof(ushort) != size)//aaa ushort
                throw new ApplicationException("Element data not uploaded correctly");

            handle.NumElements = elements.Length;
            return handle;
        }

        void Draw(Vbo handle)
        {
            // To draw a VBO:
            // 1) Ensure that the VertexArray client state is enabled.
            // 2) Bind the vertex and element buffer handles.
            // 3) Set up the data pointers (vertex, normal, color) according to your vertex format.
            // 4) Call DrawElements. (Note: the last parameter is an offset into the element buffer
            //    and will usually be IntPtr.Zero).

            //GL.EnableClientState(EnableCap.ColorArray);
            GL.EnableClientState(EnableCap.TextureCoordArray);
            GL.EnableClientState(EnableCap.VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboID);

            GL.VertexPointer(3, VertexPointerType.Float, StrideOfVertices, new IntPtr(0));
            //GL.ColorPointer(4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf(CubeVertices), new IntPtr(12));
            GL.TexCoordPointer(2, TexCoordPointerType.Float, StrideOfVertices, new IntPtr(12));

            GL.DrawElements(BeginMode.Triangles, handle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);//aaa
        }
        VertexPositionTexture[] CubeVertices = new VertexPositionTexture[]
        {
            new VertexPositionTexture( 0.0f,  1.0f,  0.0f, 0, 0),
            new VertexPositionTexture( 0.0f,  1.0f,  1.0f, 0, 1),
            new VertexPositionTexture( 1.0f,  1.0f,  0.0f, 1, 0),
            new VertexPositionTexture( 1.0f,  1.0f,  1.0f, 1, 1),
        };

        short[] CubeElements = new short[]
        {
            0, 1, 2, 2, 3, 0, // front face
            3, 2, 6, 6, 7, 3, // top face
            7, 6, 5, 5, 4, 7, // back face
            4, 0, 3, 3, 7, 4, // left face
            0, 1, 5, 5, 4, 0, // bottom face
            1, 5, 6, 6, 2, 1, // right face
        };
        #region ITerrainDrawer Members
        public int TrianglesCount()
        {
            int totaltriangles = 0;
            foreach (var k in VisibleVbo())
            {
                totaltriangles += k.NumElements / 3;
            }
            return totaltriangles;
        }
        #endregion
    }
}
