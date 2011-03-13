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
    public interface ITerrainRenderer
    {
        void Start();
        void Draw();
        void UpdateAllTiles();
        void UpdateTile(int x, int y, int z);
        int TrianglesCount();
        int texturesPacked { get; }
        int terrainTexture { get; }
        int ChunkUpdates { get; }
        int[] terrainTextures1d { get; }
        int terrainTexturesPerAtlas { get; }
        void UseTerrainTextureAtlas2d(Bitmap atlas2d);
    }
    public class TerrainDrawerDummy : ITerrainRenderer
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
        #region ITerrainRenderer Members
        public int[] terrainTextures1d { get; set; }
        public int terrainTexturesPerAtlas { get; set; }
        #endregion
        #region ITerrainRenderer Members
        public void UseTerrainTextureAtlas2d(Bitmap atlas2d)
        {
        }
        #endregion
    }
    public class TextureAtlas
    {
        public static RectangleF TextureCoords2d(int textureId, int texturesPacked)
        {
            RectangleF r = new RectangleF();
            r.Y = (1.0f / texturesPacked * (int)(textureId / texturesPacked));
            r.X = (1.0f / texturesPacked * (textureId % texturesPacked));
            r.Width = 1.0f / texturesPacked;
            r.Height =  1.0f / texturesPacked;
            return r;
        }
        public static RectangleF TextureCoords1d(int textureId, int texturesPerAtlas, int tilecount)
        {
            RectangleF r = new RectangleF();
            r.Y = (1.0f / texturesPerAtlas * (int)(textureId % texturesPerAtlas));
            r.X = 0;
            r.Width = 1.0f * tilecount;
            r.Height = 1.0f / texturesPerAtlas;
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
        public static IEnumerable<T> Take<T>(IEnumerable<T> l, int n)
        {
            int i = 0;
            foreach (var v in l)
            {
                if (i >= n)
                {
                    yield break;
                }
                yield return v;
                i++;
            }
        }
    }
    public interface ITerrainInfo
    {
        int GetTerrainBlock(int x, int y, int z);
        FastColor GetTerrainBlockColor(int x, int y, int z);
        int MapSizeX { get; }
        int MapSizeY { get; }
        int MapSizeZ { get; }
        int GetLight(int globalx, int globaly, int globalz);
        float LightMaxValue();
        byte[] GetChunk(int x, int y, int z);
    }
    public enum RailSlope
    {
        Flat, TwoLeftRaised, TwoRightRaised, TwoUpRaised, TwoDownRaised
    }
    public class RailMapUtil
    {
        [Inject]
        public ITerrainInfo mapstorage;
        [Inject]
        public IGameData data;
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
    //Finds chunks near player that should be updated,
    //and calls TerrainChunkRenderer to generate them,
    //and sends ready triangles to MeshBatcher.
    //It makes sure only chunks in view distance are kept in MeshBatcher.
    public class TerrainRenderer : ITerrainRenderer, IDisposable
    {
        [Inject]
        public MeshBatcher batcher;
        [Inject]
        public IThe3d the3d;
        [Inject]
        public IGetFilePath getfile;
        [Inject]
        public Config3d config3d;
        [Inject]
        public ITerrainInfo mapstorage;
        [Inject]
        public IGameData data;
        [Inject]
        public IGameExit exit;
        [Inject]
        public ILocalPlayerPosition localplayerposition;
        [Inject]
        public IWorldFeaturesDrawer worldfeatures;
        [Inject]
        public TerrainChunkRenderer terrainchunkdrawer;
        [Inject]
        public IFrustumCulling frustumculling;
        [Inject]
        public DirtyChunks ischunkready;
        [Inject]
        public TextureAtlasConverter textureatlasconverter;
        public event EventHandler<ExceptionEventArgs> OnCrash;
        
        public int chunksize = 16;
        public bool DONOTDRAWEDGES = true;
        public int texturesPacked { get { return 16; } } //16x16
        public int terrainTexture { get; set; }

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
            using (var atlas2d = new Bitmap(getfile.GetFile("terrain.png")))
            {
                UseTerrainTextureAtlas2d(atlas2d);
            }
            updateThreadRunning++;
            Start2();
            new Thread(UpdateThreadStart).Start();
        }
        private void Start2()
        {
            this.mapsizex = mapstorage.MapSizeX;
            this.mapsizey = mapstorage.MapSizeY;
            this.mapsizez = mapstorage.MapSizeZ;
            this.mapsizexchunks = mapstorage.MapSizeX / chunksize;
            this.mapsizeychunks = mapstorage.MapSizeY / chunksize;
            this.mapsizezchunks = mapstorage.MapSizeZ / chunksize;
        }
        int mapsizex;//cache
        int mapsizey;
        int mapsizez;
        int mapsizexchunks;
        int mapsizeychunks;
        int mapsizezchunks;
        public void UseTerrainTextureAtlas2d(Bitmap atlas2d)
        {
            terrainTexture = the3d.LoadTexture(atlas2d);
            List<int> terrainTextures1d = new List<int>();
            {
                terrainTexturesPerAtlas = atlas1dheight / (atlas2d.Width / atlas2dtiles);
                List<Bitmap> atlases1d = textureatlasconverter.Atlas2dInto1d(atlas2d, atlas2dtiles, atlas1dheight);
                foreach (Bitmap bmp in atlases1d)
                {
                    terrainTextures1d.Add(the3d.LoadTexture(bmp));
                    bmp.Dispose();
                }
            }
            this.terrainTextures1d = terrainTextures1d.ToArray();
        }
        public int atlas1dheight = 2048;
        public int atlas2dtiles = 16; // 16x16
        public int[] terrainTextures1d { get; set; }
        public int terrainTexturesPerAtlas { get; set; }
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
            restart:
            try
            {
                Start2();
                BatchedBlocksClear();
                for (; ; )
                {
                    Thread.Sleep(1);
                    if (exit.exit || exit2) { goto exit; }
                    DeleteChunksAway();
                    UpdateChunksNear();
                }
            }
            catch
            {
                goto restart;
            }
            exit:
            updateThreadRunning--;
        }
        int sometimes_update_behind_counter;
        int sometimes_update_behind_every = 20;
        private void UpdateChunksNear()
        {
            Vector3 playerpos = localplayerposition.LocalPlayerPosition;
            int pdx = (int)playerpos.X / chunksize;
            int pdy = (int)playerpos.Z / chunksize;
            int pdz = (int)playerpos.Y / chunksize;
            ischunkready.chunkdrawdistance = chunkdrawdistance;
            ischunkready.chunkdrawdistance_z = chunkdrawdistance; //8
            bool update_behind = (sometimes_update_behind_counter++ % sometimes_update_behind_every == 0);
            Vector3i? v = ischunkready.NearestDirty(pdx, pdy, pdz, this.updated && (!update_behind));
            if (v != null)
            {
                bool updated = UpdateChunk(v.Value.x, v.Value.y, v.Value.z);
                this.updated = updated;
            }
            else
            {
                updated = false;
            }
        }
        private void DeleteChunksAway()
        {
            foreach (Vector3i v in new List<Vector3i>(FindChunksToDelete()))
            {
                int[] ids = batchedchunks[v.x, v.y, v.z];
                if (ids != null)
                {
                    foreach (int k in ids)
                    {
                        batcher.Remove(k);
                    }
                    batchedchunks[v.x, v.y, v.z] = null;
                    ischunkready.SetChunkDirty(v.x, v.y, v.z, true);
                }
                batchedchunkspositions.Remove(v);
            }
        }
        bool updated = true;
        private bool UpdateChunk(int x, int y, int z)
        {
            if (!ischunkready.IsChunkDirty(x, y, z))
            {
                return false;
            }
            ischunkready.SetChunkDirty(x, y, z, false);
            //if any chunk around is dirty too, update it at the same time. 
            //(no flicker on chunk boundaries)            
            List<Vector3i> l = new List<Vector3i>();
            l.Add(new Vector3i(x, y, z));
            if (IsValidChunkPosition(x - 1, y, z) && ischunkready.IsChunkDirty(x - 1, y, z)) { l.Add(new Vector3i(x - 1, y, z)); ischunkready.SetChunkDirty(x - 1, y, z, false); }
            if (IsValidChunkPosition(x + 1, y, z) && ischunkready.IsChunkDirty(x + 1, y, z)) { l.Add(new Vector3i(x + 1, y, z)); ischunkready.SetChunkDirty(x + 1, y, z, false); }
            if (IsValidChunkPosition(x, y - 1, z) && ischunkready.IsChunkDirty(x, y - 1, z)) { l.Add(new Vector3i(x, y - 1, z)); ischunkready.SetChunkDirty(x, y - 1, z, false); }
            if (IsValidChunkPosition(x, y + 1, z) && ischunkready.IsChunkDirty(x, y + 1, z)) { l.Add(new Vector3i(x, y + 1, z)); ischunkready.SetChunkDirty(x, y + 1, z, false); }
            if (IsValidChunkPosition(x, y, z - 1) && ischunkready.IsChunkDirty(x, y, z - 1)) { l.Add(new Vector3i(x, y, z - 1)); ischunkready.SetChunkDirty(x, y, z - 1, false); }
            if (IsValidChunkPosition(x, y, z + 1) && ischunkready.IsChunkDirty(x, y, z + 1)) { l.Add(new Vector3i(x, y, z + 1)); ischunkready.SetChunkDirty(x, y, z + 1, false); }
            UpdateChunks(l.ToArray());
            return true;
        }
        private bool InFrustum(int x, int y, int z)
        {
            return frustumculling.SphereInFrustum(x * chunksize + chunksize / 2, z * chunksize + chunksize / 2, y * chunksize + chunksize / 2, chunksize);
        }
        private void UpdateChunks(Vector3i[] l)
        {
            Dictionary<Vector3i, List<VerticesIndicesToLoad>> toadd = new Dictionary<Vector3i, List<VerticesIndicesToLoad>>();
            List<Vector3i> toremove = new List<Vector3i>();
            foreach (Vector3i v in l)
            {
                IEnumerable<VerticesIndicesToLoad> chunk = MakeChunk(v.x, v.y, v.z);
                var chunkk = new List<VerticesIndicesToLoad>(chunk);
                toadd[v] = chunkk;
                toremove.Add(v);
            }

            //lock to remove and add at the same time (no flicker)
            lock (terrainlock)
            {
                foreach (Vector3i q in toremove)
                {
                    //do remove old
                    if (batchedchunks[q.x, q.y, q.z] != null)
                    {
                        foreach (int id in batchedchunks[q.x, q.y, q.z])
                        {
                            batcher.Remove(id);
                        }
                        batchedchunks[q.x, q.y, q.z] = null;
                        batchedchunkspositions.Remove(q);
                    }
                }
                foreach(var q in toadd)
                {
                    List<int> ids = new List<int>();
                    //do add
                    foreach (VerticesIndicesToLoad v in q.Value)
                    {
                        if (v.indices.Length != 0)
                        {
                            Vector3 center = new Vector3(v.position.X + chunksize / 2, v.position.Z + chunksize / 2, v.position.Y + chunksize / 2);
                            float radius = chunksize;
                            ids.Add(batcher.Add(v.indices, v.indicesCount, v.vertices, v.verticesCount, v.transparent, v.texture, center, radius));
                        }
                    }
                    if (ids.Count > 0)
                    {
                        batchedchunks[q.Key.x, q.Key.y, q.Key.z] = ids.ToArray();
                    }
                    else
                    {
                        batchedchunks[q.Key.x, q.Key.y, q.Key.z] = new int[0];
                    }
                    batchedchunkspositions.Add(q.Key);
                }
            }
        }
        int chunksToDeleteIterationSpeed = 100;
        private IEnumerable<Vector3i> FindChunksToDelete()
        {
            if (batchedchunkspositions.list.Count == 0)
            {
                yield break;
            }
            Vector3 playerpos = localplayerposition.LocalPlayerPosition;
            int px = (int)playerpos.X - (int)playerpos.X % chunksize;
            int py = (int)playerpos.Y - (int)playerpos.Y % chunksize;
            int pz = (int)playerpos.Z - (int)playerpos.Z % chunksize;
            int viewdistsquared = (int)((chunkdrawdistance + 4) * chunksize * 1.5);
            viewdistsquared = viewdistsquared * viewdistsquared;
            foreach(Vector3i k in batchedchunkspositions.Iterate(chunksToDeleteIterationSpeed))
            {
                int x = k.x;
                int y = k.y;
                int z = k.z;
                if (DistanceSquared(x * chunksize, z * chunksize, y * chunksize, px, py, pz) > viewdistsquared)
                {
                    yield return new Vector3i(x, y, z);
                }
            }
        }
        public static int DistanceSquared(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            int dx = x1 - x2;
            int dy = y1 - y2;
            int dz = z1 - z2;
            return dx * dx + dy * dy + dz * dz;
        }
        private bool IsValidChunkPosition(int xx, int yy)
        {
            return xx >= 0 && yy >= 0 && xx < mapsizex / chunksize && yy < mapsizey / chunksize;
        }
        private bool IsValidChunkPosition(int xx, int yy, int zz)
        {
            return xx >= 0 && yy >= 0 && zz >= 0
                && xx < mapsizexchunks
                && yy < mapsizeychunks
                && zz < mapsizezchunks;
        }
        private IEnumerable<VerticesIndicesToLoad> MakeChunk(int x, int y, int z)
        {
            chunkupdates++;
            return terrainchunkdrawer.MakeChunk(x, y, z);
        }
        int chunkupdates = 0;
        public int ChunkUpdates { get { return chunkupdates; } }
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
        int[, ,][] batchedchunks = new int[100, 100, 100][];
        IterableSet<Vector3i> batchedchunkspositions = new IterableSet<Vector3i>();
        void BatchedBlocksClear()
        {
            batchedchunks = new int[mapstorage.MapSizeX / chunksize,
                mapstorage.MapSizeY / chunksize,
                mapstorage.MapSizeZ / chunksize][];
        }
        public void UpdateAllTiles()
        {
            //Problem: Some chunks are not being updated
            //- random winter chunks stay in summer, and can only be fixed by placing a block
            //or by calling this function second or third time (F6 key).
            //Bad fix: do it 10 times.
            //Locking whole terrain thread doesn't help.
            for (int i = 0; i < 10; i++)
            {
                foreach (var v in batchedchunkspositions.Iterate(batchedchunkspositions.dictionary.Keys.Count))
                {
                    ischunkready.SetChunkDirty(v.x, v.y, v.z, true);
                }
            }
            return;
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
            ischunkready.SetChunkDirty(xx / chunksize, yy / chunksize, zz / chunksize, true);
            foreach (Vector3 t in TilesAround(new Vector3(xx, yy, zz)))
            {
                int x = (int)t.X;
                int y = (int)t.Y;
                int z = (int)t.Z;
                if (x / chunksize != xx / chunksize
                    || y / chunksize != yy / chunksize
                    || z / chunksize != zz / chunksize)
                {
                    if (IsValidChunkPosition(x / chunksize, y / chunksize, z / chunksize))
                    {
                        ischunkready.SetChunkDirty(x / chunksize, y / chunksize, z / chunksize, true);
                    }
                }
            }
            return;
        }
        public int TrianglesCount()
        {
            return batcher.TotalTriangleCount;
        }     
        #endregion
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
    public enum TorchType
    {
        Normal, Left, Right, Front, Back
    }
    public interface IBlockDrawerTorch
    {
        void AddTorch(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, TorchType type);
    }
    public class BlockDrawerTorchDummy : IBlockDrawerTorch
    {
        #region IBlockDrawerTorch Members
        public void AddTorch(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, TorchType type)
        {
        }
        #endregion
    }
    public class BlockDrawerTorch : IBlockDrawerTorch
    {
        [Inject]
        public IGameData data;
        [Inject]
        public ITerrainRenderer terraindrawer;
        public void AddTorch(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, TorchType type)
        {
            int tiletype = data.TileIdTorch;
            Color curcolor = Color.White;
            float torchsizexy = 0.2f;
            float topx = 1f / 2f - torchsizexy / 2f;
            float topy = 1f / 2f - torchsizexy / 2f;
            float bottomx = 1f / 2f - torchsizexy / 2f;
            float bottomy = 1f / 2f - torchsizexy / 2f;
            topx += x;
            topy += y;
            bottomx += x;
            bottomy += y;
            if (type == TorchType.Front) { bottomx = x - torchsizexy; }
            if (type == TorchType.Back) { bottomx = x + 1; }
            if (type == TorchType.Left) { bottomy = y - torchsizexy; }
            if (type == TorchType.Right) { bottomy = y + 1; }
            Vector3 top00 = new Vector3(topx, z + 1, topy);
            Vector3 top01 = new Vector3(topx, z + 1, topy + torchsizexy);
            Vector3 top10 = new Vector3(topx + torchsizexy, z + 1, topy);
            Vector3 top11 = new Vector3(topx + torchsizexy, z + 1, topy + torchsizexy);
            Vector3 bottom00 = new Vector3(bottomx, z + 0, bottomy);
            Vector3 bottom01 = new Vector3(bottomx, z + 0, bottomy + torchsizexy);
            Vector3 bottom10 = new Vector3(bottomx + torchsizexy, z + 0, bottomy);
            Vector3 bottom11 = new Vector3(bottomx + torchsizexy, z + 0, bottomy + torchsizexy);
            //top
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Top);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, terraindrawer.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(top00.X, top00.Y, top00.Z, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(top01.X, top01.Y, top01.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top10.X, top10.Y, top10.Z, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(top11.X, top11.Y, top11.Z, texrec.Right, texrec.Bottom, curcolor));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }
            //bottom - same as top, but z is 1 less.
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Bottom);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, terraindrawer.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(bottom00.X, bottom00.Y, bottom00.Z, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom01.X, bottom01.Y, bottom01.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom10.X, bottom10.Y, bottom10.Z, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom11.X, bottom11.Y, bottom11.Z, texrec.Right, texrec.Bottom, curcolor));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }
            //front
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Front);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, terraindrawer.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(bottom00.X, bottom00.Y, bottom00.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom01.X, bottom01.Y, bottom01.Z, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top00.X, top00.Y, top00.Z, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(top01.X, top01.Y, top01.Z, texrec.Right, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }
            //back - same as front, but x is 1 greater.
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Back);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, terraindrawer.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(bottom10.X, bottom10.Y, bottom10.Z, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom11.X, bottom11.Y, bottom11.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top10.X, top10.Y, top10.Z, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(top11.X, top11.Y, top11.Z, texrec.Left, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Left);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, terraindrawer.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(bottom00.X, bottom00.Y, bottom00.Z, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top00.X, top00.Y, top00.Z, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom10.X, bottom10.Y, bottom10.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top10.X, top10.Y, top10.Z, texrec.Left, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }
            //right - same as left, but y is 1 greater.
            {
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Right);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, terraindrawer.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(bottom01.X, bottom01.Y, bottom01.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top01.X, top01.Y, top01.Z, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom11.X, bottom11.Y, bottom11.Z, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top11.X, top11.Y, top11.Z, texrec.Right, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }
        }
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
    //Old class for tiling water and bedrock around finite map.
    public class WorldFeaturesDrawer : IWorldFeaturesDrawer
    {
        [Inject]
        public IThe3d the3d;
        [Inject]
        public IGetFilePath getfile;
        [Inject]
        public IMapStorage mapstorage;
        [Inject]
        public ILocalPlayerPosition localplayerposition;
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
