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
        int DrawDistance { get; set; }
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
        public int DrawDistance { get; set; }
        #endregion
    }
    public class TextureAtlas
    {
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
    public class MeshBatcher
    {
        public MeshBatcher()
        {
        }
        private void genlists()
        {
            lists = GL.GenLists(nlists);
            if (lists == 0)
            {
                throw new Exception();
            }
        }
        public int nlists = 5000;
        int lists = -1;
        int count = 0;
        public void Remove(int p)
        {
            empty.Add(p);
        }
        struct ToAdd
        {
            public ushort[] indices;
            public VertexPositionTexture[] vertices;
            public int id;
        }
        Queue<ToAdd> toadd = new Queue<ToAdd>();
        public int Add(ushort[] p, VertexPositionTexture[] vertexPositionTexture)
        {
            int id;
            lock (toadd)
            {
                if (empty.Count > 0)
                {
                    id = empty[empty.Count - 1];
                    empty.RemoveAt(empty.Count - 1);
                }
                else
                {
                    id = count;
                    count++;
                }
                toadd.Enqueue(new ToAdd() { indices = p, vertices = vertexPositionTexture, id = id });
            }
            return id;
        }
        List<int> empty = new List<int>();
        float addperframe = 0.5f;
        float addcounter = 0;
        Vector3 playerpos;
        public void Draw(Vector3 playerpos)
        {
            this.playerpos = playerpos;
            if (lists == -1)
            {
                genlists();
            }
            lock (toadd)
            {
                addcounter += addperframe;
                while (//addcounter >= 1 &&
                    toadd.Count > 0)
                {
                    addcounter -= 1;
                    ToAdd t = toadd.Dequeue();
                    GL.NewList(lists + t.id, ListMode.Compile);
                    
                    GL.EnableClientState(EnableCap.TextureCoordArray);
                    GL.EnableClientState(EnableCap.VertexArray);
                    GL.EnableClientState(EnableCap.ColorArray);
                    unsafe
                    {
                        fixed (VertexPositionTexture* p = t.vertices)
                        {
                            GL.VertexPointer(3, VertexPointerType.Float, StrideOfVertices, (IntPtr)(0 + (byte*)p));
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, StrideOfVertices, (IntPtr)(12 + (byte*)p));
                            GL.ColorPointer(4, ColorPointerType.UnsignedByte, StrideOfVertices, (IntPtr)(20 + (byte*)p));
                            GL.DrawElements(BeginMode.Triangles, t.indices.Length, DrawElementsType.UnsignedShort, t.indices);
                        }
                    }
                    GL.DisableClientState(EnableCap.TextureCoordArray);
                    GL.DisableClientState(EnableCap.VertexArray);
                    
                    /*
                    GL.Begin(BeginMode.Triangles);
                    for (int ii = 0; ii < t.indices.Length; ii++)
                    {
                        var v = t.vertices[t.indices[ii]];
                        GL.TexCoord2(v.u, v.v);
                        GL.Vertex3(v.Position.X, v.Position.Y, v.Position.Z);
                    }
                    GL.End();
                    */
                    GL.EndList();
                    if (listinfo == null)
                    {
                        listinfo = new ListInfo[nlists];
                    }
                    listinfo[t.id].indicescount = t.indices.Length;
                    listinfo[t.id].center = t.vertices[0].Position;//todo
                }
                if (toadd.Count == 0)
                {
                    addcounter = 0;
                }
            }
            for (int i = 0; i < count; i++)
            {
                if (!empty.Contains(i))
                {
                    GL.CallList(lists + i);
                }
            }
            //depth sorting. is it needed?
            /*
            List<int> alldrawlists = new List<int>();
            for (int i = 0; i < count; i++)
            {
                alldrawlists.Add(i);
            }
            alldrawlists.Sort(f);
            GL.CallLists(count, ListNameType.Int, alldrawlists.ToArray());
            */
        }
        /*
        int f(int a, int b)
        {
        }
        */
        int strideofvertices = -1;
        int StrideOfVertices
        {
            get
            {
                if (strideofvertices == -1) strideofvertices = BlittableValueType.StrideOf(new VertexPositionTexture());
                return strideofvertices;
            }
        }
        struct ListInfo
        {
            public int indicescount;
            public Vector3 center;
        }
        /// <summary>
        /// Indices count in list.
        /// </summary>
        ListInfo[] listinfo;
        public void Clear()
        {
            if (lists != -1)
            {
                GL.DeleteLists(lists, nlists);
            }
            count = 0;
            empty.Clear();
            toadd.Clear();
            listinfo = new ListInfo[nlists];
        }
        public int TotalTriangleCount
        {
            get
            {
                if (listinfo == null)
                {
                    return 0;
                }
                int sum = 0;
                for (int i = 0; i < count; i++)
                {
                    sum += listinfo[i].indicescount;
                }
                return sum / 3;
            }
        }
    }
    public interface ITerrainInfo
    {
        int GetTerrainBlock(int x, int y, int z);
        Color GetTerrainBlockColor(int x, int y, int z);
        int MapSizeX { get; }
        int MapSizeY { get; }
        int MapSizeZ { get; }
    }
    public class TerrainInfoMapStorage : ITerrainInfo
    {
        [Inject]
        public IMapStorage mapstorage { get; set; }
        public int GetTerrainBlock(int x, int y, int z)
        {
            return mapstorage.Map[x, y, z];
        }
        public Color GetTerrainBlockColor(int x, int y, int z)
        {
            return Color.White;
        }
        public int MapSizeX { get { return mapstorage.MapSizeX; } }
        public int MapSizeY { get { return mapstorage.MapSizeY; } }
        public int MapSizeZ { get { return mapstorage.MapSizeZ; } }
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
        public ITerrainInfo mapstorage { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public IGameExit exit { get; set; }
        [Inject]
        public ILocalPlayerPosition localplayerposition { get; set; }
        [Inject]
        public WorldFeaturesDrawer worldfeatures { get; set; }
        public event EventHandler<ExceptionEventArgs> OnCrash;
        public int chunksize = 16;
        public int rsize
        {
            get
            {
                int dd = drawdistance;
                dd = dd - dd % chunksize;
                dd = ((dd * 2) / chunksize) - 1;
                if (dd < 1)
                {
                    dd = 1;
                }
                return dd;
            }
        }
        int drawdistance = 256;
        public int DrawDistance { get { return drawdistance; } set { drawdistance = value; } }
        bool started = false;
        #region ITerrainDrawer Members
        public void Start()
        {
            if (started)
            {
                throw new Exception("Started already.");
            }
            started = true;
            GL.Enable(EnableCap.Texture2D);
            terrainTexture = the3d.LoadTexture(getfile.GetFile("terrain.png"));
            updateThreadRunning++;
            new Thread(UpdateThreadStart).Start();
        }
        Color terraincolor { get { return localplayerposition.Swimming ? Color.FromArgb(255, 100, 100, 255) : Color.White; } }
        bool exit2;
        void UpdateThreadStart()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                UpdateThread();
            }
            else
            {
                try
                {
                    UpdateThread();
                }
                catch (Exception e)
                {
                    if (OnCrash != null)
                    {
                        OnCrash(this, new ExceptionEventArgs() { exception = e });
                    }
                    throw;
                }
            }
        }
        public class ExceptionEventArgs : EventArgs
        {
            public Exception exception;
        }
        int updateThreadRunning = 0;
        void UpdateThread()
        {
            if (updateThreadRunning > 1)
            {
                throw new Exception("Update thread is running already.");
            }
            for (; ; )
            {
                Thread.Sleep(1);
                if (exit.exit || exit2) { break; }
                CheckRespawn();
                Point playerpoint = new Point((int)(localplayerposition.LocalPlayerPosition.X / 16), (int)(localplayerposition.LocalPlayerPosition.Z / 16));
                updater.Draw(playerpoint, rsize);
                ProcessAllPriorityTodos();
                TerrainUpdater oldupdater = updater;
                List<TerrainUpdater.TodoItem> l;
                lock (terrainlock)
                {
                    l = new List<TerrainUpdater.TodoItem>(updater.Todo);
                }
                for (int i = 0; i < l.Count; i++)
                {
                    if (updater != oldupdater) { break; }
                    var ti = l[i];
                    if (exit.exit || exit2) { return; }
                    CheckRespawn();
                    ProcessAllPriorityTodos();
                    ProcessUpdaterTodo(ti);
                }
                updater.Todo.Clear();
            }
            updateThreadRunning--;
        }
        private void CheckRespawn()
        {
            if ((lastplayerposition - localplayerposition.LocalPlayerPosition).Length > 20)
            {
                UpdateAllTiles();
            }
            lastplayerposition = localplayerposition.LocalPlayerPosition;
        }
        int FTodo(TerrainUpdater.TodoItem a, TerrainUpdater.TodoItem b)
        {
            return FPoint(a.position, b.position);
        }
        int FPoint(Point a, Point b)
        {
            return ((new Vector3(a.X * 16, localplayerposition.LocalPlayerPosition.Y, a.Y * 16) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((new Vector3(b.X * 16, localplayerposition.LocalPlayerPosition.Y, b.Y * 16) - localplayerposition.LocalPlayerPosition).Length);
        }
        int FVector3Arr(Vector3[] a, Vector3[] b)
        {
            return ((Vector3.Multiply(a[0], 16) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((Vector3.Multiply(b[0], 16) - localplayerposition.LocalPlayerPosition).Length);
        }
        int FVector3(Vector3 a, Vector3 b)
        {
            return ((Vector3.Multiply(a, 16) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((Vector3.Multiply(b, 16) - localplayerposition.LocalPlayerPosition).Length);
        }
        private void ProcessAllPriorityTodos()
        {
            while (prioritytodo.Count > 0)
            {
                if (exit.exit || exit2) { return; }
                Vector3[] ti;
                lock (prioritytodo)
                {
                    prioritytodo.Sort(FVector3Arr);
                    //todo remove duplicates
                    ti = prioritytodo[0];//.Dequeue();
                    prioritytodo.RemoveAt(0);
                }
                //Prepare list of near chunks to update.
                //This is the slowest part.
                Dictionary<Vector3, VerticesIndicesToLoad> nearchunksadd = new Dictionary<Vector3, VerticesIndicesToLoad>();
                List<Vector3> nearchunksremove = new List<Vector3>();
                for (int i = 0; i < ti.Length; i++)
                {
                    var p = ti[i];
                    var chunk = MakeChunk((int)p.X, (int)p.Y, (int)p.Z);
                    if (chunk != null && chunk.indices.Length != 0)
                    {
                        nearchunksadd.Add(p, chunk);
                    }
                    if (batchedblocks.ContainsKey(p))
                    {
                        nearchunksremove.Add(p);
                    }
                }
                //Update all near chunks at the same time, for flicker-free drawing.
                lock (terrainlock)
                {
                    foreach (Vector3 p in nearchunksremove)
                    {
                        batcher.Remove(batchedblocks[p]);
                        batchedblocks.Remove(p);
                    }
                    foreach (var k in nearchunksadd)
                    {
                        var p = k.Key;
                        var chunk = k.Value;
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
                        //lock (terrainlock)
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
                    //lock (terrainlock)
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
        List<Vector3[]> prioritytodo = new List<Vector3[]>();
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
                        BlockPolygons(indices, vertices, xxx, yyy, zzz);
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
            //GL.Color3(terraincolor);
            //lock (terrainlock)
            {
                batcher.Draw(localplayerposition.LocalPlayerPosition);
            }
            worldfeatures.DrawWorldFeatures();
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
            List<Vector3> l = new List<Vector3>();
            lock (prioritytodo)
            {
                l.Add(new Vector3(xx / chunksize, yy / chunksize, zz / chunksize));
                foreach (Vector3 t in TilesAround(new Vector3(xx, yy, zz)))
                {
                    int x = (int)t.X;
                    int y = (int)t.Y;
                    int z = (int)t.Z;
                    if (x / chunksize != xx / chunksize
                        || y / chunksize != yy / chunksize
                        || z / chunksize != zz / chunksize)
                    {
                        l.Add(new Vector3(x / chunksize, y / chunksize, z / chunksize));
                    }
                }
                prioritytodo.Add(l.ToArray());
            }
        }
        public int TrianglesCount()
        {
            return batcher.TotalTriangleCount;
        }
        public int texturesPacked { get { return 16; } }//16x16
        public int terrainTexture { get; set; }
        public float BlockShadow = 0.6f;
        #endregion
        private void BlockPolygons(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z)
        {
            var tt = mapstorage.GetTerrainBlock(x, y, z);
            bool drawtop = IsTileEmptyForDrawingOrTransparent(x, y, z + 1, tt);
            bool drawbottom = IsTileEmptyForDrawingOrTransparent(x, y, z - 1, tt);
            bool drawfront = IsTileEmptyForDrawingOrTransparent(x - 1, y, z, tt);
            bool drawback = IsTileEmptyForDrawingOrTransparent(x + 1, y, z, tt);
            bool drawleft = IsTileEmptyForDrawingOrTransparent(x, y - 1, z, tt);
            bool drawright = IsTileEmptyForDrawingOrTransparent(x, y + 1, z, tt);
            int tiletype = tt;
            Color color = mapstorage.GetTerrainBlockColor(x, y, z);
            Color colorShadow = Color.FromArgb(color.A,
                (int)(color.R * BlockShadow), (int)(color.G * BlockShadow), (int)(color.B * BlockShadow));
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
            float flowerfix = 0;
            if (data.IsBlockFlower(tiletype))
            {
                drawtop = false;
                drawbottom = false;
                flowerfix = 0.5f;
            }
            RailDirectionFlags rail = data.GetRail(tiletype);
            float blockheight = 1;//= data.GetTerrainBlockHeight(tiletype);
            if (rail != RailDirectionFlags.None)
            {
                blockheight = 0.3f;
                /*
                RailPolygons(myelements, myvertices, x, y, z, rail);
                return;
                */
            }
            float blockheight00 = blockheight;
            float blockheight01 = blockheight;
            float blockheight10 = blockheight;
            float blockheight11 = blockheight;
            if (rail != RailDirectionFlags.None)
            {
                int blocknear;
                if (x < mapstorage.MapSizeX - 1)
                {
                    blocknear = mapstorage.GetTerrainBlock(x + 1, y, z);
                    if (rail == RailDirectionFlags.Horizontal &&
                         blocknear != data.TileIdEmpty && data.GetRail(blocknear) == RailDirectionFlags.None)
                    {
                        blockheight10 += 1;
                        blockheight11 += 1;
                    }
                }
                if (x > 0)
                {
                    blocknear = mapstorage.GetTerrainBlock(x - 1, y, z);
                    if (rail == RailDirectionFlags.Horizontal &&
                         blocknear != data.TileIdEmpty && data.GetRail(blocknear) == RailDirectionFlags.None)
                    {
                        blockheight00 += 1;
                        blockheight01 += 1;
                    }
                }
                if (y > 0)
                {
                    blocknear = mapstorage.GetTerrainBlock(x, y - 1, z);
                    if (rail == RailDirectionFlags.Vertical &&
                          blocknear != data.TileIdEmpty && data.GetRail(blocknear) == RailDirectionFlags.None)
                    {
                        blockheight00 += 1;
                        blockheight10 += 1;
                    }
                }
                if (y < mapstorage.MapSizeY - 1)
                {
                    blocknear = mapstorage.GetTerrainBlock(x, y + 1, z);
                    if (rail == RailDirectionFlags.Vertical &&
                          blocknear != data.TileIdEmpty && data.GetRail(blocknear) == RailDirectionFlags.None)
                    {
                        blockheight01 += 1;
                        blockheight11 += 1;
                    }
                }
            }
            //top
            if (drawtop)
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Top);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z + blockheight00, y + 0.0f, texrec.Left, texrec.Top, color));
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z + blockheight01, y + 1.0f, texrec.Left, texrec.Bottom, color));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z + blockheight10, y + 0.0f, texrec.Right, texrec.Top, color));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z + blockheight11, y + 1.0f, texrec.Right, texrec.Bottom, color));
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
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 0.0f, texrec.Left, texrec.Top, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 1.0f, texrec.Left, texrec.Bottom, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 0.0f, texrec.Right, texrec.Top, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 1.0f, texrec.Right, texrec.Bottom, colorShadow));
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
                myvertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 0, texrec.Left, texrec.Bottom, color));
                myvertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 1, texrec.Right, texrec.Bottom, color));
                myvertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + blockheight00, y + 0, texrec.Left, texrec.Top, color));
                myvertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + blockheight01, y + 1, texrec.Right, texrec.Top, color));
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
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 0, texrec.Left, texrec.Bottom, color));
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 1, texrec.Right, texrec.Bottom, color));
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + blockheight10, y + 0, texrec.Left, texrec.Top, color));
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + blockheight11, y + 1, texrec.Right, texrec.Top, color));
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
                myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0 + flowerfix, texrec.Left, texrec.Bottom, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 0, z + blockheight00, y + 0 + flowerfix, texrec.Left, texrec.Top, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0 + flowerfix, texrec.Right, texrec.Bottom, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 1, z + blockheight10, y + 0 + flowerfix, texrec.Right, texrec.Top, colorShadow));
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
                myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1 - flowerfix, texrec.Left, texrec.Bottom, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 0, z + blockheight01, y + 1 - flowerfix, texrec.Left, texrec.Top, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 1 - flowerfix, texrec.Right, texrec.Bottom, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 1, z + blockheight11, y + 1 - flowerfix, texrec.Right, texrec.Top, colorShadow));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }
        }
        private void RailPolygons(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, RailDirectionFlags rail)
        {
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
            return mapstorage.GetTerrainBlock(x, y, z) == data.TileIdEmpty;
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
            return mapstorage.GetTerrainBlock(x, y, z) == data.TileIdEmpty
                ||(data.IsWaterTile(mapstorage.GetTerrainBlock(x, y, z))
                 && (!data.IsWaterTile(adjacenttiletype)))
                ||data.IsTransparentTile(mapstorage.GetTerrainBlock(x, y, z));
        }
        #region IDisposable Members
        public void Dispose()
        {
            exit2 = true;
            while (updateThreadRunning > 0)
            {
                Thread.Sleep(0);
            }
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
            //GL.Color3(terraincolor);
            GL.Color3(Color.White);
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
            //GL.Color3(terraincolor);
            GL.Color3(Color.White);
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
