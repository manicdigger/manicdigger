using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Renderers;

namespace ManicDigger
{
    public interface IIsChunkDirty
    {
        bool IsChunkReady(int x, int y, int z);
        bool IsChunkDirty(int x, int y, int z);
        void SetChunkDirty(int x, int y, int z, bool dirty);
        Vector3i? NearestDirty(int x, int y, int z, bool must_be_in_frustum);
        void SetAllChunksNotDirty();
    }
    public class IsChunkDirtyDummy : IIsChunkDirty
    {
        #region IIsChunkReady Members
        public bool IsChunkReady(int x, int y, int z)
        {
            return true;
        }
        #endregion
        #region IIsChunkReady Members
        public bool IsChunkDirty(int x, int y, int z)
        {
            return false;
        }
        public void SetChunkDirty(int x, int y, int z, bool p)
        {
        }
        #endregion
        #region IIsChunkReady Members
        public void SetAllChunksDirty()
        {
        }
        #endregion
        public Vector3i? NearestDirty(int x, int y, int z, bool p)
        {
            return null;
        }
        #region IIsChunkReady Members
        public void SetAllChunksNotDirty()
        {
        }
        #endregion
    }
    //Spatial grid for quick determination of nearest dirty chunk.
    //Tests and rejects bigchunksize^3=64 chunks at once.
    public class DirtyChunks : IIsChunkDirty
    {
        [Inject]
        public IMapStorage d_MapStorage;
        [Inject]
        public IFrustumCulling d_Frustum;
        public int bigchunksize = 4;
        public int chunksize = 16;
        public int chunkdrawdistance = 16;
        public int chunkdrawdistance_z = 4;

        public int mapsizexchunksbig;
        public int mapsizeychunksbig;
        public int mapsizezchunksbig;
        public int mapsizexchunks;
        public int mapsizeychunks;
        public int mapsizezchunks;
        public bool[, ,] big;
        public bool[, ,] small;
        int bigchunkdrawdistance { get { return Math.Max(1, chunkdrawdistance / bigchunksize); } }
        int bigchunkdrawdistance_z { get { return Math.Max(1, chunkdrawdistance_z / bigchunksize); } }
        public void Start()
        {
            mapsizexchunks = d_MapStorage.MapSizeX / chunksize;
            mapsizeychunks = d_MapStorage.MapSizeY / chunksize;
            mapsizezchunks = d_MapStorage.MapSizeZ / chunksize;
            mapsizexchunksbig = mapsizexchunks / bigchunksize;
            mapsizeychunksbig = mapsizeychunks / bigchunksize;
            mapsizezchunksbig = mapsizezchunks / bigchunksize;
            small = new bool[mapsizexchunks, mapsizeychunks, mapsizezchunks];
            big = new bool[mapsizexchunksbig, mapsizeychunksbig, mapsizezchunksbig];
        }
        public Vector3i? NearestDirty(int x, int y, int z, bool must_be_in_frustum)
        {
            Vector3i[] chunksnear = ChunksNear();
            int xb = x / bigchunksize;
            int yb = y / bigchunksize;
            int zb = z / bigchunksize;
            for (int i = 0; i < chunksnear.Length; i++)
            {
                Vector3i v = chunksnear[i];
                int vx = v.x + xb;
                int vy = v.y + yb;
                int vz = v.z + zb;
                if (!IsValidBigChunkPosition(vx, vy, vz))
                {
                    continue;
                }
                if (big[vx, vy, vz])
                {
                    if (must_be_in_frustum)
                    {
                        if (!IsBigChunkInFrustum(vx, vy, vz))
                        {
                            continue;
                        }
                    }
                    Vector3i nearest = new Vector3i();
                    int nearest_dist = int.MaxValue;
                    for (int xx = 0; xx < bigchunksize; xx++)
                    {
                        for (int yy = 0; yy < bigchunksize; yy++)
                        {
                            for (int zz = 0; zz < bigchunksize; zz++)
                            {
                                int curx = vx * bigchunksize + xx;
                                int cury = vy * bigchunksize + yy;
                                int curz = vz * bigchunksize + zz;
                                if (small[curx, cury, curz])
                                {
                                    if (must_be_in_frustum && (!IsSmallChunkInFrustum(curx, cury, curz)))
                                    {
                                        continue;
                                    }
                                    int curdist = TerrainRenderer.DistanceSquared(x, y, z, curx, cury, curz);
                                    if (curdist < nearest_dist)
                                    {
                                        nearest_dist = curdist;
                                        nearest = new Vector3i(curx, cury, curz);
                                    }
                                }
                            }
                        }
                    }
                    if (TerrainRenderer.DistanceSquared(nearest.x, nearest.y, nearest.z, x, y, z)
                        > sqrt3 * (chunkdrawdistance) * (chunkdrawdistance))
                    {
                        if (!must_be_in_frustum)
                        {
                            return null;
                        }
                        continue;
                    }

                    if (nearest_dist != int.MaxValue)
                    {
                        return nearest;
                    }
                }
            }
            return null;
        }
        static float sqrt3 = (float)Math.Sqrt(3);
        private bool IsSmallChunkInFrustum(int x, int y, int z)
        {
            return d_Frustum.SphereInFrustum(x * chunksize + chunksize / 2,
                           z * chunksize + chunksize / 2,
                           y * chunksize + chunksize / 2,
                           (chunksize / 2) * sqrt3);
        }
        private bool IsBigChunkInFrustum(int vx, int vy, int vz)
        {
            return d_Frustum.SphereInFrustum(vx * bigchunksize * chunksize + chunksize * bigchunksize / 2,
                                    vz * bigchunksize * chunksize + chunksize * bigchunksize / 2,
                                    vy * bigchunksize * chunksize + chunksize * bigchunksize / 2,
                                    (bigchunksize * chunksize / 2) * sqrt3);
        }
        private bool IsValidBigChunkPosition(int xx, int yy, int zz)
        {
            return xx >= 0 && yy >= 0 && zz >= 0
                && xx < mapsizexchunksbig
                && yy < mapsizeychunksbig
                && zz < mapsizezchunksbig;
        }
        private bool IsValidChunkPosition(int xx, int yy, int zz)
        {
            return xx >= 0 && yy >= 0 && zz >= 0
                && xx < mapsizexchunks
                && yy < mapsizeychunks
                && zz < mapsizezchunks;
        }
        Vector3i[] chunksnear;
        int lastchunkdrawdistance = 0;
        Vector3i[] ChunksNear()
        {
            if (chunksnear == null || lastchunkdrawdistance != bigchunkdrawdistance)
            {
                lastchunkdrawdistance = bigchunkdrawdistance;
                List<Vector3i> l = new List<Vector3i>();
                for (int x = -bigchunkdrawdistance; x <= bigchunkdrawdistance; x++)
                {
                    for (int y = -bigchunkdrawdistance; y <= bigchunkdrawdistance; y++)
                    {
                        for (int z = -bigchunkdrawdistance_z; z < bigchunkdrawdistance_z; z++)
                        {
                            l.Add(new Vector3i(x, y, z));
                        }
                    }
                }
                l.Sort((a, b) => { return (a.x * a.x + a.y * a.y + a.z * a.z).CompareTo(b.x * b.x + b.y * b.y + b.z * b.z); });
                chunksnear = l.ToArray();
            }
            return chunksnear;
        }
        #region IIsChunkReady Members
        public bool IsChunkReady(int x, int y, int z)
        {
            return IsChunkDirty(x, y, z);
        }
        public bool IsChunkDirty(int x, int y, int z)
        {
            return small[x, y, z];
        }
        public void SetChunkDirty(int x, int y, int z, bool dirty)
        {
            small[x, y, z] = dirty;
            if (dirty)
            {
                big[x / bigchunksize, y / bigchunksize, z / bigchunksize] = true;
            }
            else
            {
                for (int xx = 0; xx < bigchunksize; xx++)
                {
                    for (int yy = 0; yy < bigchunksize; yy++)
                    {
                        for (int zz = 0; zz < bigchunksize; zz++)
                        {
                            if (small[(x / bigchunksize) * bigchunksize + xx, (y / bigchunksize) * bigchunksize + yy, (z / bigchunksize) * bigchunksize + zz])
                            {
                                goto end;
                            }
                        }
                    }
                }
                big[x / bigchunksize, y / bigchunksize, z / bigchunksize] = false;
            }
        end:
            ;
        }
        #endregion
        #region IIsChunkReady Members
        public void SetAllChunksNotDirty()
        {
        }
        #endregion
    }
}
