public class IntersectionCi
{
    //http://www.3dkingdoms.com/weekly/weekly.php?a=3
    public static bool GetIntersection(float fDst1, float fDst2, Vector3Ref P1, Vector3Ref P2, Vector3Ref Hit)
    {
        Hit.X = 0;
        Hit.Y = 0;
        Hit.Z = 0;
        if ((fDst1 * fDst2) >= 0) return false;
        if (fDst1 == fDst2) return false;
        Hit.X = P1.X + (P2.X - P1.X) * (-fDst1 / (fDst2 - fDst1));
        Hit.Y = P1.Y + (P2.Y - P1.Y) * (-fDst1 / (fDst2 - fDst1));
        Hit.Z = P1.Z + (P2.Z - P1.Z) * (-fDst1 / (fDst2 - fDst1));
        return true;
    }

    public static bool InBox(Vector3Ref Hit, Vector3Ref B1, Vector3Ref B2, int Axis)
    {
        if (Axis == 1 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
        if (Axis == 2 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.X > B1.X && Hit.X < B2.X) return true;
        if (Axis == 3 && Hit.X > B1.X && Hit.X < B2.X && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
        return false;
    }

    // returns true if line (L1, L2) intersects with the box (B1, B2)
    // returns intersection point in Hit
    public static bool CheckLineBox(Vector3Ref B1, Vector3Ref B2, Vector3Ref L1, Vector3Ref L2, Vector3Ref Hit)
    {
        Hit.X = 0;
        Hit.Y = 0;
        Hit.Z = 0;

        if (L2.X < B1.X && L1.X < B1.X) return false;
        if (L2.X > B2.X && L1.X > B2.X) return false;
        if (L2.Y < B1.Y && L1.Y < B1.Y) return false;
        if (L2.Y > B2.Y && L1.Y > B2.Y) return false;
        if (L2.Z < B1.Z && L1.Z < B1.Z) return false;
        if (L2.Z > B2.Z && L1.Z > B2.Z) return false;
        if (L1.X > B1.X && L1.X < B2.X &&
            L1.Y > B1.Y && L1.Y < B2.Y &&
            L1.Z > B1.Z && L1.Z < B2.Z)
        {
            Hit = L1;
            return true;
        }
        if ((GetIntersection(L1.X - B1.X, L2.X - B1.X, L1, L2, Hit) && InBox(Hit, B1, B2, 1))
          || (GetIntersection(L1.Y - B1.Y, L2.Y - B1.Y, L1, L2, Hit) && InBox(Hit, B1, B2, 2))
          || (GetIntersection(L1.Z - B1.Z, L2.Z - B1.Z, L1, L2, Hit) && InBox(Hit, B1, B2, 3))
          || (GetIntersection(L1.X - B2.X, L2.X - B2.X, L1, L2, Hit) && InBox(Hit, B1, B2, 1))
          || (GetIntersection(L1.Y - B2.Y, L2.Y - B2.Y, L1, L2, Hit) && InBox(Hit, B1, B2, 2))
          || (GetIntersection(L1.Z - B2.Z, L2.Z - B2.Z, L1, L2, Hit) && InBox(Hit, B1, B2, 3)))
            return true;
        return false;
    }
}

public class Line3D
{
    internal float[] Start;
    internal float[] End;
}
public class Triangle3D
{
    internal float[] PointA;
    internal float[] PointB;
    internal float[] PointC;
}
public class Box3D
{
    public void Set(float x, float y, float z, float size)
    {
        if (MinEdge == null)
        {
            MinEdge = new float[3];
        }
        if (MaxEdge == null)
        {
            MaxEdge = new float[3];
        }
        Vec3.Set(MinEdge, x, y, z);
        Vec3.Set(MaxEdge, x + size, y + size, z + size);
    }
    internal float[] MinEdge;
    internal float[] MaxEdge;
    //public Vector3 MaxEdge { get { return new Vector3(MinEdge.X + size, MinEdge.Y + size, MinEdge.Z + size); } }
    //float size;
    public float LengthX() { return MaxEdge[0] - MinEdge[0]; }
    public float LengthY() { return MaxEdge[1] - MinEdge[1]; }
    public float LengthZ() { return MaxEdge[2] - MinEdge[2]; }
    public void AddPoint(float x, float y, float z)
    {
        //if is empty
        if (MinEdge == null || MaxEdge == null ||
            (MinEdge[0] == 0 && MinEdge[1] == 0 && MinEdge[2] == 0
            && MaxEdge[0] == 0 && MaxEdge[1] == 0 && MaxEdge[2] == 0))
        {
            MinEdge = Vec3.FromValues(x, y, z);
            MaxEdge = Vec3.FromValues(x, y, z);
        }
        MinEdge[0] = Game.MinFloat(MinEdge[0], x);
        MinEdge[1] = Game.MinFloat(MinEdge[1], y);
        MinEdge[2] = Game.MinFloat(MinEdge[2], z);
        MaxEdge[0] = Game.MinFloat(MaxEdge[0], x);
        MaxEdge[1] = Game.MinFloat(MaxEdge[1], y);
        MaxEdge[2] = Game.MinFloat(MaxEdge[2], z);
    }
    public float[] Center()
    {
        return null;
    }

    internal static Box3D Create(int x, int y, int z, int size)
    {
        Box3D b = new Box3D();
        b.Set(x, y, z, size);
        return b;
    }
}

public abstract class PredicateBox3D
{
    public abstract bool Hit(Box3D o);
}
public class ListBox3d
{
    internal Box3D[] arr;
    internal int count;
}
public class ListBlockPosSide
{
    internal BlockPosSide[] arr;
    internal int count;
}

public enum TileSide
{
    Top,
    Bottom,
    Front,
    Back,
    Left,
    Right
}

public class BlockPosSide
{
    public BlockPosSide()
    {
    }
    public static BlockPosSide Create(int x, int y, int z, TileSide side)
    {
        BlockPosSide p=new BlockPosSide();
        p.pos = Vec3.FromValues(x, y, z);
        p.side = side;
        return p;
    }
    internal float[] pos;
    internal TileSide side;
    public float[] Translated()
    {
        if (side == TileSide.Top) { return Vec3.FromValues(pos[0] + 0, pos[1] + 1, pos[2] + 0); }
        if (side == TileSide.Bottom) { return Vec3.FromValues(pos[0] + 0, pos[1] + -1, pos[2] + 0); }
        if (side == TileSide.Front) { return Vec3.FromValues(pos[0] + -1, pos[1] + 0, pos[2] + 0); }
        if (side == TileSide.Back) { return Vec3.FromValues(pos[0] + 1, pos[1] + 0, pos[2] + 0); }
        if (side == TileSide.Left) { return Vec3.FromValues(pos[0] + 0, pos[1] + 0, pos[2] + -1); }
        if (side == TileSide.Right) { return Vec3.FromValues(pos[0] + 0, pos[1] + 0, pos[2] + 1); }
        //throw new Exception();
        return null;
    }
    public float[] Current()
    {
        return pos;
        //throw new Exception();
    }
}

public class Triangle3DAndSide
{
    internal Triangle3D t;
    internal TileSide side;
}

public class BlockOctreeSearcher
{
    internal GamePlatform platform;
    public BlockOctreeSearcher()
    {
        intersection = new Intersection();
        pool = new Box3D[10000];
        for (int i = 0; i < 10000; i++)
        {
            pool[i] = new Box3D();
        }
        listpool = new ListBox3d[50];
        for (int i = 0; i < 50; i++)
        {
            listpool[i] = new ListBox3d();
            listpool[i].arr = new Box3D[1000];
        }
        blockpossides = new BlockPosSide[1000];
        for (int i = 0; i < 1000; i++)
        {
            blockpossides[i] = BlockPosSide.Create(0, 0, 0, TileSide.Top);
        }
        l = new BlockPosSide[1024];
        lCount = 0;
        currentHit = new float[3];
    }
    internal Box3D StartBox;
    ListBox3d Search(PredicateBox3D query)
    {
        pool_i = 0;
        listpool_i = 0;
        if (StartBox.LengthX() == 0)
        {
            //throw new Exception();
            return null;
        }
        return SearchPrivate(query, StartBox);
    }
    ListBox3d SearchPrivate(PredicateBox3D query, Box3D box)
    {
        if (box.LengthX() == 1)
        {
            ListBox3d l1 = newListBox3d();
            l1.count = 1;
            l1.arr[0] = box;
            return l1;
        }
        ListBox3d l = newListBox3d();
        l.count = 0;
        ListBox3d children = Children(box);
        for (int k = 0; k < children.count; k++)
        {
            Box3D child = children.arr[k];
            if (query.Hit(child))
            {
                ListBox3d l2 = SearchPrivate(query, child);
                for (int i = 0; i < l2.count; i++)
                {
                    Box3D n = l2.arr[i];
                    l.arr[l.count++] = n;
                }
                recycleListBox3d(l2);
            }
        }
        recycleListBox3d(children);
        return l;
    }
    Box3D[] pool;
    int pool_i;
    ListBox3d[] listpool;
    int listpool_i;
    Box3D newBox3d()
    {
        return pool[pool_i++];
    }
    void recycleBox3d(Box3D l)
    {
        pool_i--;
        pool[pool_i] = l;
    }
    ListBox3d newListBox3d()
    {
        ListBox3d l = listpool[listpool_i++];
        l.count = 0;
        return l;
    }
    void recycleListBox3d(ListBox3d l)
    {
        listpool_i--;
        listpool[listpool_i] = l;
    }
    ListBox3d Children(Box3D box)
    {
        ListBox3d l = newListBox3d();
        l.count = 8;
        Box3D[] c = l.arr;
        for (int i = 0; i < 8; i++)
        {
            c[i] = newBox3d();
        }
        float x = box.MinEdge[0];
        float y = box.MinEdge[1];
        float z = box.MinEdge[2];
        float size = box.LengthX() / 2;
        c[0].Set(x, y, z, size);
        c[1].Set(x + size, y, z, size);
        c[2].Set(x, y, z + size, size);
        c[3].Set(x + size, y, z + size, size);

        c[4].Set(x, y + size, z, size);
        c[5].Set(x + size, y + size, z, size);
        c[6].Set(x, y + size, z + size, size);
        c[7].Set(x + size, y + size, z + size, size);
        return l;
    }
    public bool BoxHit(Box3D box)
    {
        currentHit[0] = 0;
        currentHit[1] = 0;
        currentHit[2] = 0;
        return Intersection.CheckLineBox(box, currentLine, currentHit);
    }
    Line3D currentLine;
    float[] currentHit;
    BlockPosSide[] blockpossides;
    int blockpossides_i;
    Intersection intersection;
    BlockPosSide[] l;
    int lCount;
    public BlockPosSide[] LineIntersection(DelegateIsBlockEmpty isEmpty, DelegateGetBlockHeight getBlockHeight, Line3D line)
    {
        blockpossides_i = 0;
        lCount = 0;
        currentLine = line;
        currentHit[0] = 0;
        currentHit[1] = 0;
        currentHit[2] = 0;
        ListBox3d l1 = Search(PredicateBox3DHit.Create(this));
        for (int i = 0; i < l1.count; i++)
        {
            Box3D node = l1.arr[i];
            float[] hit = currentHit;
            int x = platform.FloatToInt(node.MinEdge[0]);
            int y = platform.FloatToInt(node.MinEdge[2]);
            int z = platform.FloatToInt(node.MinEdge[1]);
            if (!isEmpty.IsBlockEmpty(x, y, z))
            {
                Box3D node2 = node;
                node2.MaxEdge[1] = node2.MinEdge[1] + getBlockHeight.GetBlockHeight(x, y, z);
                //BlockPosSide hit2 = new BlockPosSide(0, 0, 0, TileSide.Top);
                //BlockPosSide hit2 = blockpossides[blockpossides_i];
                //blockpossides_i++;
                //hit2.pos = new float[] { x, z, y };
                BlockPosSide hit2 = intersection.CheckLineBoxExact(line, node2);
                if (hit2 != null)
                {
                    hit2.pos = Vec3.FromValues(x, z, y);
                    l[lCount++] = hit2;
                }
            }
        }
        BlockPosSide[] ll = new BlockPosSide[lCount];
        for (int i = 0; i < lCount; i++)
        {
            ll[i] = l[i];
        }
        return ll;
    }
}

public class PredicateBox3DHit : PredicateBox3D
{
    public static PredicateBox3DHit Create(BlockOctreeSearcher s_)
    {
        PredicateBox3DHit p = new PredicateBox3DHit();
        p.s = s_;
        return p;
    }
    BlockOctreeSearcher s;
    public override bool Hit(Box3D o)
    {
        return s.BoxHit(o);
    }
}
public abstract class DelegateIsBlockEmpty
{
    public abstract bool IsBlockEmpty(int x, int y, int z);
}
public abstract class DelegateGetBlockHeight
{
    public abstract float GetBlockHeight(int x, int y, int z);
}

public class Intersection
{
    public Intersection()
    {
        float one = 1;
        SMALL_NUM = one / 100000000; // anything that avoids division overflow

        u = new float[3];// triangle vectors
        v = new float[3];
        n = new float[3];

        dir = new float[3];// ray vectors
        w0 = new float[3];
        w = new float[3];

        floatMaxValue = 1000 * 1000;
        floatMaxValue *= 1000;
        big = new float[3];
        closest = new float[3];
        a = new float[3];
        b = new float[3];
        outIntersection = new float[3];
    }

    //http://www.3dkingdoms.com/weekly/weekly.php?a=3
    static bool GetIntersection(float fDst1, float fDst2, float[] P1, float[] P2, float[] Hit)
    {
        // Hit = new Vector3();
        if ((fDst1 * fDst2) >= 0) return false;
        if (fDst1 == fDst2) return false;
        // Hit = P1 + (P2 - P1) * (-fDst1 / (fDst2 - fDst1));
        Hit[0] = P1[0] + (P2[0] - P1[0]) * (-fDst1 / (fDst2 - fDst1));
        Hit[1] = P1[1] + (P2[1] - P1[1]) * (-fDst1 / (fDst2 - fDst1));
        Hit[2] = P1[2] + (P2[2] - P1[2]) * (-fDst1 / (fDst2 - fDst1));
        return true;
    }
    static bool InBox(float[] Hit, float[] B1, float[] B2, int Axis)
    {
        if (Axis == 1 && Hit[2] > B1[2] && Hit[2] < B2[2] && Hit[1] > B1[1] && Hit[1] < B2[1]) return true;
        if (Axis == 2 && Hit[2] > B1[2] && Hit[2] < B2[2] && Hit[0] > B1[0] && Hit[0] < B2[0]) return true;
        if (Axis == 3 && Hit[0] > B1[0] && Hit[0] < B2[0] && Hit[1] > B1[1] && Hit[1] < B2[1]) return true;
        return false;
    }
    // returns true if line (L1, L2) intersects with the box (B1, B2)
    // returns intersection point in Hit
    public static bool CheckLineBox1(float[] B1, float[] B2, float[] L1, float[] L2, float[] Hit)
    {
        // Hit = new Vector3();
        if (L2[0] < B1[0] && L1[0] < B1[0]) return false;
        if (L2[0] > B2[0] && L1[0] > B2[0]) return false;
        if (L2[1] < B1[1] && L1[1] < B1[1]) return false;
        if (L2[1] > B2[1] && L1[1] > B2[1]) return false;
        if (L2[2] < B1[2] && L1[2] < B1[2]) return false;
        if (L2[2] > B2[2] && L1[2] > B2[2]) return false;
        if (L1[0] > B1[0] && L1[0] < B2[0] &&
            L1[1] > B1[1] && L1[1] < B2[1] &&
            L1[2] > B1[2] && L1[2] < B2[2])
        {
            // Hit = L1;
            Hit[0] = L1[0];
            Hit[1] = L1[1];
            Hit[2] = L1[2];
            return true;
        }
        if ((GetIntersection(L1[0] - B1[0], L2[0] - B1[0], L1, L2, Hit) && InBox(Hit, B1, B2, 1))
          || (GetIntersection(L1[1] - B1[1], L2[1] - B1[1], L1, L2, Hit) && InBox(Hit, B1, B2, 2))
          || (GetIntersection(L1[2] - B1[2], L2[2] - B1[2], L1, L2, Hit) && InBox(Hit, B1, B2, 3))
          || (GetIntersection(L1[0] - B2[0], L2[0] - B2[0], L1, L2, Hit) && InBox(Hit, B1, B2, 1))
          || (GetIntersection(L1[1] - B2[1], L2[1] - B2[1], L1, L2, Hit) && InBox(Hit, B1, B2, 2))
          || (GetIntersection(L1[2] - B2[2], L2[2] - B2[2], L1, L2, Hit) && InBox(Hit, B1, B2, 3)))
            return true;
        return false;
    }
    /// <summary>
    /// Warning: randomly returns incorrect hit position (back side of box).
    /// </summary>
    /// <param name="box"></param>
    /// <param name="line"></param>
    /// <param name="hit"></param>
    /// <returns></returns>
    public static bool CheckLineBox(Box3D box, Line3D line, float[] hit)
    {
        return CheckLineBox1(box.MinEdge, box.MaxEdge, line.Start, line.End, hit);
    }
    // Copyright 2001, softSurfer (www.softsurfer.com)
    // This code may be freely used and modified for any purpose
    // providing that this copyright notice is included with it.
    // SoftSurfer makes no warranty for this code, and cannot be held
    // liable for any real or imagined damage resulting from its use.
    // Users of this code must verify correctness for their application.

    // Assume that classes are already given for the objects:
    //    Point and Vector with
    //        coordinates {float x, y, z;}
    //        operators for:
    //            == to test equality
    //            != to test inequality
    //            (Vector)0 = (0,0,0)         (null vector)
    //            Point  = Point ± Vector
    //            Vector = Point - Point
    //            Vector = Scalar * Vector    (scalar product)
    //            Vector = Vector * Vector    (cross product)
    //    Line and Ray and Segment with defining points {Point P0, P1;}
    //        (a Line is infinite, Rays and Segments start at P0)
    //        (a Ray extends beyond P1, but a Segment ends at P1)
    //    Plane with a point and a normal {Point V0; Vector n;}
    //    Triangle with defining vertices {Point V0, V1, V2;}
    //    Polyline and Polygon with n vertices {int n; Point *V;}
    //        (a Polygon has V[n]=V[0])
    //===================================================================

    float SMALL_NUM; // anything that avoids division overflow
    // dot product (3D) which allows vector operations in arguments
    float dot(float[] u, float[] v) { return u[0] * v[0] + u[1] * v[1] + u[2] * v[2]; }
    static void cross(float[] a, float[] b, float[] result)
    {
        result[0] = a[1] * b[2] - a[2] * b[1];
        result[1] = a[2] * b[0] - a[0] * b[2];
        result[2] = a[0] * b[1] - a[1] * b[0];
    }

    float[] u;// triangle vectors
    float[] v;
    float[] n;


    float[] dir;// ray vectors
    float[] w0;
    float[] w;

    // intersect_RayTriangle(): intersect a ray with a 3D triangle
    //    Input:  a ray R, and a triangle T
    //    Output: *I = intersection point (when it exists)
    //    Return: -1 = triangle is degenerate (a segment or point)
    //             0 = disjoint (no intersect)
    //             1 = intersect in unique point I1
    //             2 = are in the same plane
    public int
    RayTriangle(Line3D R, Triangle3D T, float[] I)
    {
        float r;             // params to calc ray-plane intersect
        float a;
        float b;

        I[0] = 0;
        I[1] = 0;
        I[2] = 0;

        // get triangle edge vectors and plane normal
        u[0] = T.PointB[0] - T.PointA[0];
        u[1] = T.PointB[1] - T.PointA[1];
        u[2] = T.PointB[2] - T.PointA[2];
        v[0] = T.PointC[0] - T.PointA[0];
        v[1] = T.PointC[1] - T.PointA[1];
        v[2] = T.PointC[2] - T.PointA[2];
        //n = u.CrossProduct(v);             // cross product
        cross(u, v, n);
        //if (n == (Vector3D)0)            // triangle is degenerate
        //    return -1;                 // do not deal with this case

        dir[0] = R.End[0] - R.Start[0];             // ray direction vector
        dir[1] = R.End[1] - R.Start[1];
        dir[2] = R.End[2] - R.Start[2];
        w0[0] = R.Start[0] - T.PointA[0];
        w0[1] = R.Start[1] - T.PointA[1];
        w0[2] = R.Start[2] - T.PointA[2];
        a = -dot(n, w0);
        b = dot(n, dir);
        if (Game.AbsFloat(b) < SMALL_NUM)
        {     // ray is parallel to triangle plane
            if (a == 0)                // ray lies in triangle plane
                return 2;
            else return 0;             // ray disjoint from plane
        }

        // get intersect point of ray with triangle plane
        r = a / b;
        if (r < 0)                   // ray goes away from triangle
            return 0;                  // => no intersect
        // for a segment, also test if (r > 1.0) => no intersect

        I[0] = R.Start[0] + r * dir[0];           // intersect point of ray and plane
        I[1] = R.Start[1] + r * dir[1];
        I[2] = R.Start[2] + r * dir[2];

        // is I inside T?
        float uu;
        float uv;
        float vv;
        float wu;
        float wv;
        float D;
        uu = dot(u, u);
        uv = dot(u, v);
        vv = dot(v, v);
        w[0] = I[0] - T.PointA[0];
        w[1] = I[1] - T.PointA[1];
        w[2] = I[2] - T.PointA[2];
        wu = dot(w, u);
        wv = dot(w, v);
        D = uv * uv - uu * vv;

        // get and test parametric coords
        float s;
        float t;
        s = (uv * wv - vv * wu) / D;
        if (s < 0 || s > 1)        // I is outside T
            return 0;
        t = (uv * wu - uu * wv) / D;
        if (t < 0 || (s + t) > 1)  // I is outside T
            return 0;

        return 1;                      // I is in T
    }
    float floatMaxValue;
    float[] big;
    float[] closest;
    float[] a;
    float[] b;
    float[] outIntersection;
    public BlockPosSide CheckLineBoxExact(Line3D line, Box3D box)
    {
        if (PointInBox(line.Start, box))
        {
            BlockPosSide p = BlockPosSide.Create(0, 0, 0, TileSide.Top);
            p.pos = line.Start;
            return p;
        }
        Vec3.Set(big, floatMaxValue, floatMaxValue, floatMaxValue);
        Vec3.Set(closest, floatMaxValue, floatMaxValue, floatMaxValue);
        TileSide side = TileSide.Top;
        
        Triangle3DAndSide[] triangles = BoxTrianglesAndSides(box.MinEdge, box.MaxEdge);
        for (int i = 0; i < 12; i++)
        {
            Triangle3DAndSide t = triangles[i];
            if (RayTriangle(line, t.t, outIntersection) != 0)
            {
                a[0] = line.Start[0] - outIntersection[0];
                a[1] = line.Start[1] - outIntersection[1];
                a[2] = line.Start[2] - outIntersection[2];
                b[0] = line.Start[0] - closest[0];
                b[1] = line.Start[1] - closest[1];
                b[2] = line.Start[2] - closest[2];
                if (Vec3.Len(a) < Vec3.Len(b))
                {
                    closest[0] = outIntersection[0];
                    closest[1] = outIntersection[1];
                    closest[2] = outIntersection[2];
                    side = t.side;
                }
            }
        }
        //if (closest == big) { throw new Exception(); }
        if (closest[0] == big[0] && closest[1] == big[1] && closest[2] == big[2]) { return null; }
        BlockPosSide bps = BlockPosSide.Create(0, 0, 0, TileSide.Top);
        bps.pos = closest;
        bps.side = side;
        return bps;
        //if (PointInBox(line.End, box)) { return new TilePosSide() { pos = line.End }; }
        //throw new Exception();
    }

    Triangle3DAndSide[] triangleandside_pool;
    public Triangle3DAndSide[] BoxTrianglesAndSides(float[] aa, float[] bb)
    {
        if (triangleandside_pool == null)
        {
            triangleandside_pool = new Triangle3DAndSide[12];
            for (int i = 0; i < 12; i++)
            {
                triangleandside_pool[i] = new Triangle3DAndSide();
            }
        }
        TileSide side = TileSide.Top;
        TileSide sidei = TileSide.Top;
        int ii = 0;
        Triangle3D[] triangles = BoxTriangles(aa, bb);
        for (int i = 0; i < 12; i++)
        {
            Triangle3D t = triangles[i];
            side = sidei;
            ii++;
            if (ii % 2 == 0)
            {
                if (sidei == TileSide.Top) { sidei = TileSide.Bottom; }
                else if (sidei == TileSide.Bottom) { sidei = TileSide.Front; }
                else if (sidei == TileSide.Front) { sidei = TileSide.Back; }
                else if (sidei == TileSide.Back) { sidei = TileSide.Left; }
                else if (sidei == TileSide.Left) { sidei = TileSide.Right; }
                else if (sidei == TileSide.Right) { sidei = TileSide.Top; }
            }
            //Triangle3DAndSide tt = new Triangle3DAndSide();
            Triangle3DAndSide tt = triangleandside_pool[ii - 1];
            tt.t = t;
            tt.side = side;
            //l.Add(tt);
        }
        return triangleandside_pool;
    }
    int[] myelements;
    float[][] myvertices;
    Triangle3D[] trianglespool;
    public Triangle3D[] BoxTriangles(float[] aa, float[] bb)
    {
        float x = aa[0];
        float z = aa[1];
        float y = aa[2];
        float sx = bb[0] - aa[0];
        float sz = bb[1] - aa[1];
        float sy = bb[2] - aa[2];
        //List<short> myelements = new List<short>();
        if (myelements == null)
        {
            myelements = new int[6 * 6];
            myvertices = new float[6 * 4][];
            for (int i = 0; i < 6 * 4; i++)
            {
                myvertices[i] = new float[3];
            }
            trianglespool = new Triangle3D[6 * 2];
            for (int i = 0; i < 6 * 2; i++)
            {
                trianglespool[i] = new Triangle3D();
            }
        }
        int myverticesCount = 0;
        int myelementsCount = 0;
        //top
        //if (drawtop)
        {
            int lastelement = myverticesCount;
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 1 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 1 * sz, y + 1 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 1 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 1 * sz, y + 1 * sy);
            myelements[myelementsCount++] = (lastelement + 0);
            myelements[myelementsCount++] = (lastelement + 1);
            myelements[myelementsCount++] = (lastelement + 2);
            myelements[myelementsCount++] = (lastelement + 3);
            myelements[myelementsCount++] = (lastelement + 1);
            myelements[myelementsCount++] = (lastelement + 2);
        }
        //bottom - same as top, but z is 1 less.
        //if (drawbottom)
        {
            int lastelement = myverticesCount;
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 0 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 0 * sz, y + 1 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 0 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 0 * sz, y + 1 * sy);
            myelements[myelementsCount++] = ((lastelement + 0));
            myelements[myelementsCount++] = ((lastelement + 1));
            myelements[myelementsCount++] = ((lastelement + 2));
            myelements[myelementsCount++] = ((lastelement + 3));
            myelements[myelementsCount++] = ((lastelement + 1));
            myelements[myelementsCount++] = ((lastelement + 2));
        }
        //front
        //if (drawfront)
        {
            int lastelement = myverticesCount;
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 0 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 0 * sz, y + 1 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 1 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 1 * sz, y + 1 * sy);
            myelements[myelementsCount++] = ((lastelement + 0));
            myelements[myelementsCount++] = ((lastelement + 1));
            myelements[myelementsCount++] = ((lastelement + 2));
            myelements[myelementsCount++] = ((lastelement + 3));
            myelements[myelementsCount++] = ((lastelement + 1));
            myelements[myelementsCount++] = ((lastelement + 2));
        }
        //back - same as front, but x is 1 greater.
        //if (drawback)
        {
            int lastelement = myverticesCount;
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 0 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 0 * sz, y + 1 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 1 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 1 * sz, y + 1 * sy);
            myelements[myelementsCount++] = ((lastelement + 0));
            myelements[myelementsCount++] = ((lastelement + 1));
            myelements[myelementsCount++] = ((lastelement + 2));
            myelements[myelementsCount++] = ((lastelement + 3));
            myelements[myelementsCount++] = ((lastelement + 1));
            myelements[myelementsCount++] = ((lastelement + 2));
        }
        //if (drawleft)
        {
            int lastelement = myverticesCount;
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 0 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 1 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 0 * sz, y + 0 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 1 * sz, y + 0 * sy);

            myelements[myelementsCount++] = ((lastelement + 0));
            myelements[myelementsCount++] = ((lastelement + 1));
            myelements[myelementsCount++] = ((lastelement + 2));
            myelements[myelementsCount++] = ((lastelement + 3));
            myelements[myelementsCount++] = ((lastelement + 1));
            myelements[myelementsCount++] = ((lastelement + 2));
        }
        //right - same as left, but y is 1 greater.
        //if (drawright)
        {
            int lastelement = myverticesCount;
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 0 * sz, y + 1 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 1 * sz, y + 1 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 0 * sz, y + 1 * sy);
            Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 1 * sz, y + 1 * sy);
            myelements[myelementsCount++] = ((lastelement + 0));
            myelements[myelementsCount++] = ((lastelement + 1));
            myelements[myelementsCount++] = ((lastelement + 2));
            myelements[myelementsCount++] = ((lastelement + 3));
            myelements[myelementsCount++] = ((lastelement + 1));
            myelements[myelementsCount++] = ((lastelement + 2));
        }
        //Triangle3D[] triangles = new Triangle3D[myelementsCount / 3];
        for (int i = 0; i < myelementsCount / 3; i++)
        {
            //Triangle3D t = new Triangle3D();
            Triangle3D t = trianglespool[i];
            t.PointA = myvertices[myelements[i * 3 + 0]];
            t.PointB = myvertices[myelements[i * 3 + 1]];
            t.PointC = myvertices[myelements[i * 3 + 2]];
            //triangles[i] = t;
        }
        return trianglespool;
    }
    static bool PointInBox(float[] vv, Box3D node)
    {
        return vv[0] >= node.MinEdge[0] && vv[1] >= node.MinEdge[1] && vv[2] >= node.MinEdge[1]
            && vv[0] <= node.MaxEdge[0] && vv[1] <= node.MaxEdge[1] && vv[2] <= node.MaxEdge[2];
    }
    static float[] Interpolate(float[] aa, float[] bb, float f)
    {
        float x = aa[0] + (bb[0] - aa[0]) * f;
        float y = aa[1] + (bb[1] - aa[1]) * f;
        float z = aa[2] + (bb[2] - aa[2]) * f;
        return Vec3.FromValues(x, y, z);
    }

}
