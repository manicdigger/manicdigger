using System;
using System.Collections.Generic;
using System.Text;
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
    public class TerrainDrawerDummy : ITerrainDrawer
    {
        #region ITerrainDrawer Members
        public void Start()
        {
        }
        public void Draw()
        {
        }
        public void UpdateAllTiles()
        {
        }
        public void UpdateTile(int x, int y, int z)
        {
        }
        public int TrianglesCount()
        {
            return 0;
        }
        public int texturesPacked { get; set; }
        public int terrainTexture { get; set; }
        #endregion
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
    public class TileDrawer
    {
    }
    class TerrainUpdater
    {
        Rectangle currentRect = nullRect;
        static Rectangle nullRect = new Rectangle(int.MinValue, int.MinValue, 1, 1);
        public void Draw(Point playerpos, int rsize)
        {
            if (rsize % 2 != 1)
            {
                throw new Exception();
            }
            var newrect = PlayerRectangle(playerpos, rsize);
            int z = (rsize - 1) / 2;
            //if teleport then draw starting from center.
            if (!currentRect.IntersectsWith(newrect))
            {
                Todo.Add(new TodoItem() { action = TodoAction.Clear });
                /*
                if (currentRect != nullRect)
                {
                    foreach (Point p in Points(currentRect))
                    {
                        Todo.Add(new TodoItem() { action = TodoAction.Delete, position = p });
                    }
                }
                */
                for (int v = 0; v <= z; v++)
                {
                    Point center = new Point((newrect.Right + newrect.Left) / 2, (newrect.Bottom + newrect.Top) / 2);
                    foreach (Point p in SquareEdgesPoints(v))
                    {
                        Point pp = new Point(center.X + p.X, center.Y + p.Y);
                        Todo.Add(new TodoItem() { action = TodoAction.Add, position = pp });
                    }
                }
            }
            else
            {
                if (currentRect == nullRect)
                {
                    throw new Exception();
                }
                foreach (Rectangle r in OldTiles(currentRect, newrect))
                {
                    foreach (Point p in Points(r))
                    {
                        Todo.Add(new TodoItem() { action = TodoAction.Delete, position = p });
                    }
                }
                foreach (Rectangle r in NewTiles(currentRect, newrect))
                {
                    foreach (Point p in Points(r))
                    {
                        Todo.Add(new TodoItem() { action = TodoAction.Add, position = p });
                    }
                }
            }
            currentRect = newrect;
        }
        private IEnumerable<Point> SquareEdgesPoints(int v)
        {
            if (v < 0)
            {
                throw new ArgumentException();
            }
            if (v == 0)
            {
                yield return new Point(0, 0);
                yield break;
            }
            Rectangle r = new Rectangle(-v, -v, 2 * v + 1, 2 * v + 1);
            //up
            for (int x = r.Left + 1; x < r.Right - 1; x++)
            {
                yield return new Point(x, r.Top);
            }
            //bottom
            for (int x = r.Left + 1; x < r.Right - 1; x++)
            {
                yield return new Point(x, r.Bottom - 1);
            }
            //left
            for (int y = r.Top; y < r.Bottom; y++)
            {
                yield return new Point(r.Left, y);
            }
            //right
            for (int y = r.Top; y < r.Bottom; y++)
            {
                yield return new Point(r.Right - 1, y);
            }
        }
        IEnumerable<Point> Points(Rectangle r)
        {
            for (int x = r.X; x < r.Right; x++)
            {
                for (int y = r.Y; y < r.Bottom; y++)
                {
                    yield return new Point(x, y);
                }
            }
        }
        public enum TodoAction
        {
            Add,
            Delete,
            Clear,
        }
        public struct TodoItem
        {
            public TodoAction action;
            public Point position;
        }
        public List<TodoItem> Todo = new List<TodoItem>();
        public IEnumerable<Rectangle> OldTiles(Rectangle a, Rectangle b)
        {
            Region r = new Region(b);
            r.Complement(a);
            foreach (RectangleF rr in r.GetRegionScans(new System.Drawing.Drawing2D.Matrix()))
            {
                yield return new Rectangle((int)rr.X, (int)rr.Y, (int)rr.Width, (int)rr.Height);
            }
        }
        public IEnumerable<Rectangle> NewTiles(Rectangle a, Rectangle b)
        {
            Region r = new Region(a);
            r.Complement(b);
            foreach (RectangleF rr in r.GetRegionScans(new System.Drawing.Drawing2D.Matrix()))
            {
                yield return new Rectangle((int)rr.X, (int)rr.Y, (int)rr.Width, (int)rr.Height);
            }
        }
        public Rectangle PlayerRectangle(Point p, int rsize)
        {
            return new Rectangle(p.X - (rsize - 1) / 2, p.Y - (rsize - 1) / 2, rsize, rsize);
        }
    }
    public class Vbo
    {
        public int VboID, EboID, NumElements;
        public Box3D box;
        public int realindicescount = 0;
        public int realverticescount = 0;
        public bool valid;
    }
    /// <summary>
    /// Memory allocator for adding indices+vertices lists
    /// to OpenTK hardware Vertex Buffer objects.
    /// </summary>
    public class MeshBatcher
    {
        List<Vbo> vbolist = new List<Vbo>();
        ushort maxvbosize = 60 * 1000;
        public int Add(ushort[] indices, VertexPositionTexture[] vertices)
        {
            if (indices.Length == 0) { throw new Exception("Zero indices."); }
            if (indices.Length > maxvbosize) { throw new Exception("Too many indices."); }
            if (vertices.Length > maxvbosize) { throw new Exception("Too many vertices."); }
            int vboid = -1;
            int entryid = -1;
            //-todo defragment vbo.
            //-if there is free space then try to use it
            if (deleted.ContainsKey(indices.Length))
            {
                //Console.WriteLine("deleted count:" + deleted[indices.Length].Count);
                foreach (int id in deleted[indices.Length])
                {
                    if (indices.Length == entries[id].indicesrange.Count
                        && vertices.Length == entries[id].verticesrange.Count)
                    {
                        vboid = entries[id].vboid;
                        entryid = id;
                        break;
                    }
                }
                if (entryid != -1)
                {
                    if (deleted.ContainsKey(indices.Length))
                    {
                        deleted[indices.Length].Remove(entryid);
                    }
                    goto ok;
                }
            }
            //-if it won't fit in the last vbo, then make a new vbo
            if (vbolist.Count == 0 ||
                (vbolist[vbolist.Count - 1].realindicescount + indices.Length) >= maxvbosize ||
                (vbolist[vbolist.Count - 1].realverticescount + vertices.Length) >= maxvbosize)
            {
                vbolist.Add(new Vbo());
            }
            vboid = vbolist.Count - 1;
            Range verticesrange = new Range(vbolist[vboid].realverticescount, vertices.Length);
            Range indicesrange = new Range(vbolist[vboid].realindicescount, indices.Length);
            vbolist[vboid].realverticescount += vertices.Length;
            vbolist[vboid].realindicescount += indices.Length;
            entries.Add(new Entry() { vboid = vboid, verticesrange = verticesrange, indicesrange = indicesrange });
            entryid = entries.Count - 1;
        ok: ;
            //vbo = LoadVBO(vertices, indices);
            //for (int i = 0; i < indices.Length; i++)
            //{
            //    indices[i]
            //}
            //translate indices
            int translateindices = entries[entryid].verticesrange.Start; //vbolist[vboid].realverticescount - vertices.Length;
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = (ushort)(indices[i] + translateindices);
            }
            if (!toadd.ContainsKey(vboid))
            {
                toadd.Add(vboid, new List<ToAdd>());
            }
            toadd[vboid].Add(new ToAdd()
            {
                vertices = vertices,
                indices = indices,
                vbo = vbolist[vboid],
                indicesstart = entries[entryid].indicesrange.Start,
                verticesstart = entries[entryid].verticesrange.Start,
            });

            return entryid;
        }
        struct ToAdd
        {
            public Vbo vbo;
            public ushort[] indices;
            public VertexPositionTexture[] vertices;
            public int indicesstart;
            public int verticesstart;
        }
        Dictionary<int, List<ToAdd>> toadd = new Dictionary<int, List<ToAdd>>();
        struct Range
        {
            public Range(int start, int count)
            {
                this.Start = start;
                this.Count = count;
            }
            public int Start;
            public int Count;
        }
        struct Entry
        {
            public int vboid;
            public Range indicesrange;
            public Range verticesrange;
        }
        List<Entry> entries = new List<Entry>();
        internal void Draw()
        {
            //if (vbo != null)
            foreach (Vbo vbo in vbolist)
            {
                Draw(vbo);
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
        void LoadVBO<TVertex>(Vbo handle, TVertex[] vertices, ushort[] elements) where TVertex : struct
        {
            //Vbo handle = new Vbo();
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
            //return handle;
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
        public void Remove(int id)
        {
            int vboid = entries[id].vboid;
            if (!toadd.ContainsKey(entries[id].vboid))
            {
                toadd[vboid] = new List<ToAdd>();
            }
            var a = entries[id].verticesrange;
            var b = entries[id].indicesrange;
            toadd[vboid].Add(new ToAdd()
            {
                verticesstart = a.Start,
                vertices = new VertexPositionTexture[a.Count],
                vbo = vbolist[vboid],
                indicesstart = b.Start,
                indices = new ushort[b.Count],
            });
            if (!deleted.ContainsKey(b.Count))
            {
                deleted[b.Count] = new List<int>();
            }
            deleted[b.Count].Add(id);
        }
        //indices count-entry id
        Dictionary<int, List<int>> deleted = new Dictionary<int, List<int>>();
        public int TotalTriangleCount
        {
            get
            {
                int sum = 0;
                foreach (Vbo vbo in vbolist)
                {
                    sum += vbo.NumElements / 3;
                }
                return sum;
            }
        }
        public void Clear()
        {
            foreach(Vbo vbo in vbolist)
            {
                GL.DeleteBuffers(1, ref vbo.EboID);
                GL.DeleteBuffers(1, ref vbo.VboID);
            }
            vbolist.Clear();
            deleted.Clear();
            entries.Clear();
            toadd.Clear();
        }
        public void Update(int count)
        {
            foreach (var v in toadd)
            {
                if (!vbolist[v.Key].valid)
                {
                    LoadVBO(vbolist[v.Key], new VertexPositionTexture[maxvbosize], new ushort[maxvbosize]);
                    vbolist[v.Key].valid = true;
                }
                //add vertices
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbolist[v.Key].VboID);
                IntPtr VideoMemoryIntPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
                foreach (var vv in v.Value)
                {
                    unsafe
                    {
                        fixed (VertexPositionTexture* SystemMemory = &vv.vertices[0])
                        {
                            VertexPositionTexture* VideoMemory = (VertexPositionTexture*)VideoMemoryIntPtr.ToPointer();
                            //if (VideoMemory == null) { return; }//wrong
                            for (int i = 0; i < vv.vertices.Length; i++)
                                VideoMemory[i + vv.verticesstart] = SystemMemory[i]; // simulate what GL.BufferData would do
                        }
                    }
                }
                GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            }
            foreach (var v in toadd)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbolist[v.Key].EboID);
                IntPtr VideoMemoryIntPtr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.WriteOnly);
                foreach (var vv in v.Value)
                {
                    unsafe
                    {
                        fixed (ushort* SystemMemory = &vv.indices[0])
                        {
                            ushort* VideoMemory = (ushort*)VideoMemoryIntPtr.ToPointer();
                            //if (VideoMemory == null) { return; }//wrong
                            for (int i = 0; i < vv.indices.Length; i++)
                                VideoMemory[i + vv.indicesstart] = (ushort)(SystemMemory[i]); // simulate what GL.BufferData would do
                        }
                    }

                }
                GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
            }
            toadd.Clear();
        }
    }
    /// <summary>
    /// </summary>
    /// <remarks>
    /// Requires OpenTK.
    /// </remarks>
    public class TerrainDrawer3d : ITerrainDrawer, IDisposable
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
        [Inject]
        public WorldFeaturesDrawer worldfeatures { get; set; }
        public int chunksize = 16;
        public int rsize { get { return (256 / chunksize) - 1; } }
        #region ITerrainDrawer Members
        public void Start()
        {
            GL.Enable(EnableCap.Texture2D);
            terrainTexture = the3d.LoadTexture(getfile.GetFile("terrain.png"));
            new Thread(updatethread).Start();
        }
        Color terraincolor { get { return localplayerposition.Swimming ? Color.FromArgb(255, 100, 100, 255) : Color.White; } }
        bool exit2;
        void updatethread()
        {
            for (; ; )
            {
                Thread.Sleep(1);
                if (exit.exit || exit2) { return; }
                CheckRespawn();
                Point playerpoint = new Point((int)(localplayerposition.LocalPlayerPosition.X / 16), (int)(localplayerposition.LocalPlayerPosition.Z / 16));
                updater.Draw(playerpoint, rsize);
                ProcessAllPriorityTodos();
                List<TerrainUpdater.TodoItem> l = new List<TerrainUpdater.TodoItem>(updater.Todo);
                foreach (var ti in updater.Todo)
                {
                    if (exit.exit || exit2) { return; }
                    CheckRespawn();
                    ProcessAllPriorityTodos();
                    ProcessUpdaterTodo(ti);
                }
                updater.Todo.Clear();
            }
        }
        private void CheckRespawn()
        {
            if ((lastplayerposition - localplayerposition.LocalPlayerPosition).Length > 20)
            {
                UpdateAllTiles();
            }
            lastplayerposition = localplayerposition.LocalPlayerPosition;
        }
        int f(Vector3 a, Vector3 b)
        {
            return ((Vector3.Multiply(a, 16) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((Vector3.Multiply(b, 16) - localplayerposition.LocalPlayerPosition).Length);
        }
        private void ProcessAllPriorityTodos()
        {
            while (prioritytodo.Count > 0)
            {
                if (exit.exit || exit2) { return; }
                Vector3 ti;
                lock (prioritytodo)
                {
                    prioritytodo.Sort(f);
                    //todo remove duplicates
                    ti = prioritytodo[0];//.Dequeue();
                    prioritytodo.RemoveAt(0);
                }
                var p = ti;
                var chunk = MakeChunk((int)p.X, (int)p.Y, (int)p.Z);
                lock (terrainlock)
                {
                    if (batchedblocks.ContainsKey(p))
                    {
                        batcher.Remove(batchedblocks[p]);
                    }
                    if (chunk != null && chunk.indices.Length != 0)
                    {
                        batchedblocks[p] = batcher.Add(chunk.indices, chunk.vertices);
                    }
                }
            }
        }
        private void ProcessUpdaterTodo(TerrainUpdater.TodoItem ti)
        {
            var p = ti.position;
            if (ti.action == TerrainUpdater.TodoAction.Add)
            {
                for (int z = 0; z < mapstorage.MapSizeZ / 16; z++)
                {
                    try
                    {
                        lock (terrainlock)
                        {
                            var chunk = MakeChunk(p.X, p.Y, z);
                            if (chunk != null && chunk.indices.Length != 0)
                            {
                                batchedblocks[new Vector3(p.X, p.Y, z)] = batcher.Add(chunk.indices, chunk.vertices);
                            }
                        }
                    }
                    catch { Console.WriteLine("Chunk error"); }
                }
            }
            else if (ti.action == TerrainUpdater.TodoAction.Delete)
            {
                for (int z = 0; z < mapstorage.MapSizeZ / 16; z++)
                {
                    lock (terrainlock)
                    {
                        if (batchedblocks.ContainsKey(new Vector3(p.X, p.Y, z)))
                        {
                            batcher.Remove(batchedblocks[new Vector3(p.X, p.Y, z)]);
                            batchedblocks.Remove(new Vector3(p.X, p.Y, z));
                        }
                    }
                }
            }
            else
            {
                UpdateAllTiles(true);
            }
        }
        /// <summary>
        /// Contains chunk positions.
        /// </summary>
        //Queue<Vector3> prioritytodo = new Queue<Vector3>();
        List<Vector3> prioritytodo = new List<Vector3>();
        VerticesIndicesToLoad MakeChunk(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0) { return null; }
            if (x >= mapstorage.MapSizeX / chunksize || y >= mapstorage.MapSizeY / chunksize || z >= mapstorage.MapSizeZ / chunksize) { return null; }
            List<ushort> indices = new List<ushort>();
            List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();
            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        int xxx = x * chunksize + xx;
                        int yyy = y * chunksize + yy;
                        int zzz = z * chunksize + zz;
                        TileElements(indices, vertices, xxx, yyy, zzz);
                    }
                }
            }
            return new VerticesIndicesToLoad()
            {
                indices = indices.ToArray(),
                vertices = vertices.ToArray(),
                position =
                    new Vector3(x * 16, y * 16, z * 16)
            };
        }
        object terrainlock = new object();
        public void Draw()
        {
            Update();
            GL.Color3(terraincolor);
            lock (terrainlock)
            {
                batcher.Update(500);
                batcher.Draw();
            }
            worldfeatures.DrawWorldFeatures();
        }
        private void Update()
        {
        }
        Dictionary<Vector3, int> batchedblocks = new Dictionary<Vector3, int>();
        Vector3 lastplayerposition;
        TerrainUpdater updater = new TerrainUpdater();
        MeshBatcher batcher = new MeshBatcher();
        public void UpdateAllTiles()
        {
            UpdateAllTiles(false);
        }
        public void UpdateAllTiles(bool updaterInduced)
        {
            lock (terrainlock)
            {
                if (!updaterInduced)
                {
                    updater = new TerrainUpdater();
                }
                batchedblocks.Clear();
                batcher.Clear();
            }
            return;
        }
        enum UpdateType
        {
            Add,
            Delete,
        }
        struct ToUpdate
        {
            public UpdateType type;
            public Vector3 position;
        }
        IEnumerable<Vector3> TilesAround(Vector3 pos)
        {
            yield return new Vector3(pos + new Vector3(0, 0, 0));
            yield return new Vector3(pos + new Vector3(+1, 0, 0));
            yield return new Vector3(pos + new Vector3(-1, 0, 0));
            yield return new Vector3(pos + new Vector3(0, +1, 0));
            yield return new Vector3(pos + new Vector3(0, -1, 0));
            yield return new Vector3(pos + new Vector3(0, 0, +1));
            yield return new Vector3(pos + new Vector3(0, 0, -1));
        }
        public void UpdateTile(int xx, int yy, int zz)
        {
            lock (prioritytodo)
            {
                prioritytodo.Add(new Vector3(xx / chunksize, yy / chunksize, zz / chunksize));
                foreach (Vector3 t in TilesAround(new Vector3(xx, yy, zz)))
                {
                    int x = (int)t.X;
                    int y = (int)t.Y;
                    int z = (int)t.Z;
                    if (x / chunksize != xx / chunksize
                        || y / chunksize != yy / chunksize
                        || z / chunksize != zz / chunksize)
                    {
                        prioritytodo.Add(new Vector3(x / chunksize, y / chunksize, z / chunksize));
                    }
                }
            }
        }
        public int TrianglesCount()
        {
            return batcher.TotalTriangleCount;
        }
        public int texturesPacked { get { return 16; } }//16x16
        public int terrainTexture { get; set; }
        #endregion
        private void TileElements(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z)
        {
            var tt = mapstorage.Map[x, y, z];
            bool drawtop = IsTileEmptyForDrawingOrTransparent(x, y, z + 1, tt);
            bool drawbottom = IsTileEmptyForDrawingOrTransparent(x, y, z - 1, tt);
            bool drawfront = IsTileEmptyForDrawingOrTransparent(x - 1, y, z, tt);
            bool drawback = IsTileEmptyForDrawingOrTransparent(x + 1, y, z, tt);
            bool drawleft = IsTileEmptyForDrawingOrTransparent(x, y - 1, z, tt);
            bool drawright = IsTileEmptyForDrawingOrTransparent(x, y + 1, z, tt);
            int tiletype = mapstorage.Map[x, y, z];
            if (!data.IsValidTileType(tiletype))
            {
                return;
            }
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
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Top);
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
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Bottom);
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
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Front);
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
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Back);
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
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Left);
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
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Right);
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
        }
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
        #region IDisposable Members
        public void Dispose()
        {
            exit2 = true;
        }
        #endregion
    }
    /// <summary>
    /// </summary>
    /// <remarks>
    /// Requires OpenTK.
    /// </remarks>
    public class WorldFeaturesDrawer
    {
        [Inject]
        public IThe3d the3d { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public IMapStorage mapstorage { get; set; }
        [Inject]
        public ILocalPlayerPosition localplayerposition { get; set; }
        public void DrawWorldFeatures()
        {
            if (ENABLE_WATER)
            {
                DrawWater();
                DrawMapEdges();
            }
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
        int watersizex = 1 * 1000;
        int watersizey = 1 * 1000;
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
    }
}
