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
        int ChunkUpdates { get; }
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
        #region ITerrainDrawer Members
        public int ChunkUpdates { get; set; }
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
    public class Vbo
    {
        public int VboID, EboID, NumElements;
        public Box3D box;
        public int realindicescount = 0;
        public int realverticescount = 0;
        public bool valid;
    }
    public class MyLinq
    {
        public static bool Any<T>(IEnumerable<T> l)
        {
            return l.GetEnumerator().MoveNext();
        }
        public static T First<T>(IEnumerable<T> l)
        {
            var e = l.GetEnumerator();
            e.MoveNext();
            return e.Current;
        }
        public static int Count<T>(IEnumerable<T> l)
        {
            int count = 0;
            foreach (T v in l)
            {
                count++;
            }
            return count;
        }
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
            empty[p] = 0;
        }
        struct ToAdd
        {
            public ushort[] indices;
            public VertexPositionTexture[] vertices;
            public int id;
            public bool transparent;
        }
        Queue<ToAdd> toadd = new Queue<ToAdd>();
        public int Add(ushort[] p, VertexPositionTexture[] vertexPositionTexture)
        {
            return Add(p, vertexPositionTexture, false);
        }
        public int Add(ushort[] p, VertexPositionTexture[] vertexPositionTexture, bool transparent)
        {
            int id;
            lock (toadd)
            {
                if (empty.Count > 0)
                {
                    id = MyLinq.First(empty.Keys);
                    empty.Remove(id);
                }
                else
                {
                    id = count;
                    count++;
                }
                toadd.Enqueue(new ToAdd() { indices = p, vertices = vertexPositionTexture, id = id, transparent = transparent });
            }
            return id;
        }
        Dictionary<int, int> empty = new Dictionary<int, int>();
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
                    listinfo[t.id].transparent = t.transparent;
                }
                if (toadd.Count == 0)
                {
                    addcounter = 0;
                }
            }
            for (int i = 0; i < count; i++)
            {
                if (!empty.ContainsKey(i))
                {
                    if (!listinfo[i].transparent)
                    {
                        GL.CallList(lists + i);
                    }
                }
            }
            GL.Disable(EnableCap.CullFace);//for water.
            for (int i = 0; i < count; i++)
            {
                if (!empty.ContainsKey(i))
                {
                    if (listinfo[i].transparent)
                    {
                        GL.CallList(lists + i);
                    }
                }
            }
            GL.Enable(EnableCap.CullFace);
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
            public bool transparent;
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
                    if (!empty.ContainsKey(i))
                    {
                        sum += listinfo[i].indicescount;
                    }
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
            return mapstorage.GetBlock(x, y, z);
        }
        public Color GetTerrainBlockColor(int x, int y, int z)
        {
            return Color.White;
        }
        public int MapSizeX { get { return mapstorage.MapSizeX; } }
        public int MapSizeY { get { return mapstorage.MapSizeY; } }
        public int MapSizeZ { get { return mapstorage.MapSizeZ; } }
    }
    public enum RailSlope
    {
        Flat, TwoLeftRaised, TwoRightRaised, TwoUpRaised, TwoDownRaised
    }
    public class RailMapUtil
    {
        [Inject]
        public ITerrainInfo mapstorage { get; set; }
        [Inject]
        public IGameData data { get; set; }
        public RailSlope GetRailSlope(int x, int y, int z)
        {
            int tiletype = mapstorage.GetTerrainBlock(x, y, z);
            RailDirectionFlags rail = data.GetRail(tiletype);
            int blocknear;
            if (x < mapstorage.MapSizeX - 1)
            {
                blocknear = mapstorage.GetTerrainBlock(x + 1, y, z);
                if (rail == RailDirectionFlags.Horizontal &&
                     blocknear != data.TileIdEmpty && data.GetRail(blocknear) == RailDirectionFlags.None)
                {
                    return RailSlope.TwoRightRaised;
                }
            }
            if (x > 0)
            {
                blocknear = mapstorage.GetTerrainBlock(x - 1, y, z);
                if (rail == RailDirectionFlags.Horizontal &&
                     blocknear != data.TileIdEmpty && data.GetRail(blocknear) == RailDirectionFlags.None)
                {
                    return RailSlope.TwoLeftRaised;

                }
            }
            if (y > 0)
            {
                blocknear = mapstorage.GetTerrainBlock(x, y - 1, z);
                if (rail == RailDirectionFlags.Vertical &&
                      blocknear != data.TileIdEmpty && data.GetRail(blocknear) == RailDirectionFlags.None)
                {
                    return RailSlope.TwoUpRaised;
                }
            }
            if (y < mapstorage.MapSizeY - 1)
            {
                blocknear = mapstorage.GetTerrainBlock(x, y + 1, z);
                if (rail == RailDirectionFlags.Vertical &&
                      blocknear != data.TileIdEmpty && data.GetRail(blocknear) == RailDirectionFlags.None)
                {
                    return RailSlope.TwoDownRaised;
                }
            }
            return RailSlope.Flat;
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
        public ITerrainInfo mapstorage { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public IGameExit exit { get; set; }
        [Inject]
        public ILocalPlayerPosition localplayerposition { get; set; }
        [Inject]
        public IWorldFeaturesDrawer worldfeatures { get; set; }
        public event EventHandler<ExceptionEventArgs> OnCrash;
        public int chunksize = 16;
        public int chunkdrawdistance
        {
            get
            {
                int dd = drawdistance;
                dd = dd - dd % chunksize;
                //dd = ((dd * 2) / chunksize) - 1;
                dd = dd / chunksize;
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
                Point playerpoint = new Point((int)(localplayerposition.LocalPlayerPosition.X / chunksize), (int)(localplayerposition.LocalPlayerPosition.Z / chunksize));
                ProcessAllPriorityTodos();
                List<TodoItem> l = new List<TodoItem>();
                foreach (var k in batchedblocks)
                {
                    var v = k.Key;
                    if ((new Vector3(v.X * chunksize, localplayerposition.LocalPlayerPosition.Y, v.Y * chunksize) - localplayerposition.LocalPlayerPosition).Length > chunkdrawdistance * chunksize)
                    {
                        l.Add(new TodoItem() { position = new Point((int)v.X, (int)v.Y), action = TodoAction.Delete });
                    }
                }
                for (int x = -chunkdrawdistance; x <= chunkdrawdistance; x++)
                {
                    for (int y = -chunkdrawdistance; y <= chunkdrawdistance; y++)
                    {
                        int xx = (int)localplayerposition.LocalPlayerPosition.X / chunksize + x;
                        int yy = (int)localplayerposition.LocalPlayerPosition.Z / chunksize + y;
                        bool add = true;
                        for (int z = 0; z < mapstorage.MapSizeZ / chunksize; z++)
                        {
                            if (batchedblocks.ContainsKey(new Vector3(xx, yy, z)))
                            {
                                add = false;
                            }
                        }
                        if (add && (new Vector3(xx * chunksize, localplayerposition.LocalPlayerPosition.Y, yy * chunksize) - localplayerposition.LocalPlayerPosition).Length <= chunkdrawdistance * chunksize
                            && IsValidChunkPosition(xx, yy))
                        {
                            l.Add(new TodoItem() { position = new Point(xx, yy), action = TodoAction.Add });
                        }
                    }
                }
                l.Sort(FTodo);
                for (int i = 0; i < Math.Min(5, l.Count); i++)//l.Count; i++)
                {
                    var ti = l[i];
                    if (exit.exit || exit2) { break; }
                    CheckRespawn();
                    ProcessAllPriorityTodos();
                    ProcessUpdaterTodo(ti);
                }
            }
            updateThreadRunning--;
        }
        private bool IsValidChunkPosition(int xx, int yy)
        {
            return xx >= 0 && yy >= 0 && xx < mapstorage.MapSizeX / chunksize && yy < mapstorage.MapSizeY / chunksize;
        }
        private void CheckRespawn()
        {
            if ((lastplayerposition - localplayerposition.LocalPlayerPosition).Length > 20)
            {
                UpdateAllTiles();
            }
            lastplayerposition = localplayerposition.LocalPlayerPosition;
        }
        int FTodo(TodoItem a, TodoItem b)
        {
            if (a.action == TodoAction.Delete && b.action == TodoAction.Add)
            {
                return -1;
            }
            if (a.action == TodoAction.Add && b.action == TodoAction.Delete)
            {
                return 1;
            }
            return FPoint(a.position, b.position);
        }
        int FPoint(Point a, Point b)
        {
            return ((new Vector3(a.X * chunksize, localplayerposition.LocalPlayerPosition.Y, a.Y * chunksize) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((new Vector3(b.X * chunksize, localplayerposition.LocalPlayerPosition.Y, b.Y * chunksize) - localplayerposition.LocalPlayerPosition).Length);
        }
        int FVector3Arr(Vector3[] a, Vector3[] b)
        {
            return ((Vector3.Multiply(a[0], chunksize) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((Vector3.Multiply(b[0], chunksize) - localplayerposition.LocalPlayerPosition).Length);
        }
        int FVector3(Vector3 a, Vector3 b)
        {
            return ((Vector3.Multiply(a, chunksize) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((Vector3.Multiply(b, chunksize) - localplayerposition.LocalPlayerPosition).Length);
        }
        int FVector3Chunk(Vector3 a, Vector3 b)
        {
            return ((new Vector3(a.X * chunksize, a.Z * chunksize, a.Y * chunksize) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((new Vector3(b.X * chunksize, b.Z * chunksize, b.Y * chunksize) - localplayerposition.LocalPlayerPosition).Length);
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
                Dictionary<Vector3, VerticesIndicesToLoad[]> nearchunksadd = new Dictionary<Vector3, VerticesIndicesToLoad[]>();
                List<Vector3> nearchunksremove = new List<Vector3>();
                for (int i = 0; i < ti.Length; i++)
                {
                    var p = ti[i];
                    var chunk = MakeChunk((int)p.X, (int)p.Y, (int)p.Z);
                    var chunkk = new List<VerticesIndicesToLoad>(chunk);
                    if (chunkk.Count > 0)
                    {
                        nearchunksadd.Add(p, chunkk.ToArray());
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
                        foreach (int id in batchedblocks[p])
                        {
                            batcher.Remove(id);
                        }
                        batchedblocks.Remove(p);
                    }
                    foreach (var k in nearchunksadd)
                    {
                        var p = k.Key;
                        var chunk = k.Value;
                        List<int> ids = new List<int>();
                        foreach (var cc in chunk)
                        {
                            ids.Add(batcher.Add(cc.indices, cc.vertices, cc.transparent));
                        }
                        batchedblocks[p] = ids.ToArray();
                    }
                }
            }
        }
        private void ProcessUpdaterTodo(TodoItem ti)
        {
            var p = ti.position;
            if (ti.action == TodoAction.Add)
            {
                for (int z = 0; z < mapstorage.MapSizeZ / chunksize; z++)
                {
                    try
                    {
                        //lock (terrainlock)
                        {
                            var chunk = MakeChunk(p.X, p.Y, z);
                            var chunkk = new List<VerticesIndicesToLoad>(chunk);
                            List<int> ids = new List<int>();
                            foreach (VerticesIndicesToLoad v in chunkk)
                            {
                                if (v.indices.Length != 0)
                                {
                                    ids.Add(batcher.Add(v.indices, v.vertices, v.transparent));
                                }
                            }
                            if (ids.Count > 0)
                            {
                                batchedblocks[new Vector3(p.X, p.Y, z)] = ids.ToArray();
                            }
                        }
                    }
                    catch { Console.WriteLine("Chunk error"); }
                }
            }
            else if (ti.action == TodoAction.Delete)
            {
                for (int z = 0; z < mapstorage.MapSizeZ / chunksize; z++)
                {
                    //lock (terrainlock)
                    {
                        if (batchedblocks.ContainsKey(new Vector3(p.X, p.Y, z)))
                        {
                            foreach (int id in batchedblocks[new Vector3(p.X, p.Y, z)])
                            {
                                batcher.Remove(id);
                            }
                            batchedblocks.Remove(new Vector3(p.X, p.Y, z));
                        }
                    }
                }
            }
            else
            {
                UpdateAllTiles();
            }
        }
        int chunkupdates = 0;
        public int ChunkUpdates { get { return chunkupdates; } }
        /// <summary>
        /// Contains chunk positions.
        /// </summary>
        //Queue<Vector3> prioritytodo = new Queue<Vector3>();
        List<Vector3[]> prioritytodo = new List<Vector3[]>();
        IEnumerable<VerticesIndicesToLoad> MakeChunk(int x, int y, int z)
        {
            chunkupdates++;
            if (x < 0 || y < 0 || z < 0) { yield break; }
            if (x >= mapstorage.MapSizeX / chunksize || y >= mapstorage.MapSizeY / chunksize || z >= mapstorage.MapSizeZ / chunksize) { yield break; }
            List<ushort> indices = new List<ushort>();
            List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();
            List<ushort> transparentindices = new List<ushort>();
            List<VertexPositionTexture> transparentvertices = new List<VertexPositionTexture>();
            byte[, ,] currentChunk = new byte[chunksize + 2, chunksize + 2, chunksize + 2];
            for (int xx = 0; xx < chunksize + 2; xx++)
            {
                for (int yy = 0; yy < chunksize + 2; yy++)
                {
                    for (int zz = 0; zz < chunksize + 2; zz++)
                    {
                        int xxx = x * chunksize + xx - 1;
                        int yyy = y * chunksize + yy - 1;
                        int zzz = z * chunksize + zz - 1;
                        if (!IsValidPos(xxx, yyy, zzz))
                        {
                            continue;
                        }
                        currentChunk[xx, yy, zz] = (byte)mapstorage.GetTerrainBlock(xxx, yyy, zzz);
                    }
                }
            }

            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        int xxx = x * chunksize + xx;
                        int yyy = y * chunksize + yy;
                        int zzz = z * chunksize + zz;

                        if (!(data.IsTransparentTile(currentChunk[xx + 1, yy + 1, zz + 1])
                            || data.IsWaterTile(currentChunk[xx + 1, yy + 1, zz + 1])))
                        {
                            BlockPolygons(indices, vertices, xxx, yyy, zzz, currentChunk);
                        }
                        else
                        {
                            BlockPolygons(transparentindices, transparentvertices, xxx, yyy, zzz, currentChunk);
                        }
                    }
                }
            }
            if (indices.Count > 0)
            {
                yield return new VerticesIndicesToLoad()
                {
                    indices = indices.ToArray(),
                    vertices = vertices.ToArray(),
                    position =
                        new Vector3(x * chunksize, y * chunksize, z * chunksize)
                };
            }
            if (transparentindices.Count > 0)
            {
                yield return new VerticesIndicesToLoad()
                {
                    indices = transparentindices.ToArray(),
                    vertices = transparentvertices.ToArray(),
                    position =
                        new Vector3(x * chunksize, y * chunksize, z * chunksize),
                    transparent = true
                };
            }
        }
        object terrainlock = new object();
        public void Draw()
        {
            //GL.Color3(terraincolor);
            worldfeatures.DrawWorldFeatures();
            lock (terrainlock)            
            {
                GL.BindTexture(TextureTarget.Texture2D, terrainTexture);
                //must be drawn last, for transparent blocks.
                batcher.Draw(localplayerposition.LocalPlayerPosition);
            }
        }
        Dictionary<Vector3, int[]> batchedblocks = new Dictionary<Vector3, int[]>();
        Vector3 lastplayerposition;
        MeshBatcher batcher = new MeshBatcher();
        public void UpdateAllTiles()
        {
            lock (terrainlock)
            {
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
        RailMapUtil railmaputil;
        #endregion
        private void BlockPolygons(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, byte[, ,] currentChunk)
        {
            int xx = x % chunksize + 1;
            int yy = y % chunksize + 1;
            int zz = z % chunksize + 1;
            var tt = currentChunk[xx, yy, zz];
            if (!data.IsValidTileType(tt))
            {
                return;
            }
            bool drawtop = IsTileEmptyForDrawingOrTransparent(xx, yy, zz + 1, tt, currentChunk);
            bool drawbottom = IsTileEmptyForDrawingOrTransparent(xx, yy, zz - 1, tt, currentChunk);
            bool drawfront = IsTileEmptyForDrawingOrTransparent(xx - 1, yy, zz, tt, currentChunk);
            bool drawback = IsTileEmptyForDrawingOrTransparent(xx + 1, yy, zz, tt, currentChunk);
            bool drawleft = IsTileEmptyForDrawingOrTransparent(xx, yy - 1, zz, tt, currentChunk);
            bool drawright = IsTileEmptyForDrawingOrTransparent(xx, yy + 1, zz, tt, currentChunk);
            int tiletype = tt;
            if (!(drawtop || drawbottom || drawfront || drawback || drawleft || drawright))
            {
                return;
            }
            Color color = mapstorage.GetTerrainBlockColor(x, y, z);
            Color colorShadow = Color.FromArgb(color.A,
                (int)(color.R * BlockShadow), (int)(color.G * BlockShadow), (int)(color.B * BlockShadow));
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
            if (tt == data.TileIdSingleStairs)
            {
                blockheight = 0.5f;
            }
            //slope
            float blockheight00 = blockheight;
            float blockheight01 = blockheight;
            float blockheight10 = blockheight;
            float blockheight11 = blockheight;
            if (rail != RailDirectionFlags.None)
            {
                if (railmaputil == null)
                {
                    railmaputil = new RailMapUtil() { data = data, mapstorage = mapstorage };
                }
                RailSlope slope = railmaputil.GetRailSlope(x, y, z);
                if (slope == RailSlope.TwoRightRaised)
                {
                    blockheight10 += 1;
                    blockheight11 += 1;
                }
                if (slope == RailSlope.TwoLeftRaised)
                {
                    blockheight00 += 1;
                    blockheight01 += 1;
                }
                if (slope == RailSlope.TwoUpRaised)
                {
                    blockheight00 += 1;
                    blockheight10 += 1;
                }
                if (slope == RailSlope.TwoDownRaised)
                {
                    blockheight01 += 1;
                    blockheight11 += 1;
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
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Back);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 0, texrec.Right, texrec.Bottom, color));
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 1, texrec.Left, texrec.Bottom, color));
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + blockheight10, y + 0, texrec.Right, texrec.Top, color));
                myvertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + blockheight11, y + 1, texrec.Left, texrec.Top, color));
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
                myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0 + flowerfix, texrec.Right, texrec.Bottom, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 0, z + blockheight00, y + 0 + flowerfix, texrec.Right, texrec.Top, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0 + flowerfix, texrec.Left, texrec.Bottom, colorShadow));
                myvertices.Add(new VertexPositionTexture(x + 1, z + blockheight10, y + 0 + flowerfix, texrec.Left, texrec.Top, colorShadow));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }
            //right - same as left, but y is 1 greater.
            if (drawright)
            {
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
        bool IsTileEmptyForDrawingOrTransparent(int xx, int yy, int zz, int adjacenttiletype, byte[, ,] currentChunk)
        {
            byte tt = currentChunk[xx, yy, zz];
            if (!config3d.ENABLE_TRANSPARENCY)
            {
                return tt == data.TileIdEmpty;
            }            
            return tt == data.TileIdEmpty
                || (data.IsWaterTile(tt)
                 && (!data.IsWaterTile(adjacenttiletype)))
                || data.IsTransparentTile(tt);
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
    public interface IWorldFeaturesDrawer
    {
        void DrawWorldFeatures();
    }
    public class WorldFeaturesDrawerDummy : IWorldFeaturesDrawer
    {
        #region IWorldFeaturesDrawer Members
        public void DrawWorldFeatures()
        {
        }
        #endregion
    }
    /// <summary>
    /// </summary>
    /// <remarks>
    /// Requires OpenTK.
    /// </remarks>
    public class WorldFeaturesDrawer : IWorldFeaturesDrawer
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
                DrawMapEdges();
                DrawWater();
            }
        }
        public bool ENABLE_WATER = true;
        int? watertexture;
        int? rocktexture;
        bool waternotfoundwritten = false;
        bool rocknotfoundwritten = false;
        Color terraincolor { get { return Color.White; } }//return localplayerposition.Swimming ? Color.FromArgb(255, 78, 95, 140) : Color.White; } }
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
