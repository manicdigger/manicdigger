using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;


namespace ManicDigger.Collisions
{
    public class BlockOctreeSearcher
    {
        public BlockOctreeSearcher()
        {
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
        }
        public Box3D StartBox;
        ListBox3d Search(PredicateBox3D query)
        {
            pool_i = 0;
            listpool_i = 0;
            if (StartBox.LengthX() == 0)
            {
                throw new Exception();
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
        public static int created;
        public static int returned;
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
        float[] currentHit = new float[3];
        BlockPosSide[] blockpossides;
        int blockpossides_i;
        public BlockPosSide[] LineIntersection(IsBlockEmpty isEmpty, GetBlockHeight getBlockHeight, Line3D line)
        {
            blockpossides_i = 0;
            List<BlockPosSide> l = new List<BlockPosSide>();
            currentLine = line;
            currentHit[0] = 0;
            currentHit[1] = 0;
            currentHit[2] = 0;
            ListBox3d l1 = Search(PredicateBox3DHit.Create(this));
            for (int i = 0; i < l1.count; i++)
            {
                Box3D node = l1.arr[i];
                float[] hit = currentHit;
                int x = (int)node.MinEdge[0];
                int y = (int)node.MinEdge[2];
                int z = (int)node.MinEdge[1];
                if (!isEmpty(x, y, z))
                {
                    Box3D node2 = node;
                    node2.MaxEdge[1] = node2.MinEdge[1] + getBlockHeight(x, y, z);
                    //BlockPosSide hit2 = new BlockPosSide(0, 0, 0, TileSide.Top);
                    //BlockPosSide hit2 = blockpossides[blockpossides_i];
                    //blockpossides_i++;
                    //hit2.pos = new float[] { x, z, y };
                    BlockPosSide hit2 = Intersection.CheckLineBoxExact(line, node2);
                    if (hit2 != null)
                    {
                        hit2.pos = new float[] { x, z, y };
                        l.Add(hit2);
                    }
                }
            }
            BlockPosSide[] ll = new BlockPosSide[l.Count];
            for (int i = 0; i < l.Count; i++)
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
    public delegate bool IsBlockEmpty(int x, int y, int z);
    public delegate float GetBlockHeight(int x, int y, int z);
    public enum TileSideFlags
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Front = 4,
        Back = 8,
        Left = 16,
        Right = 32,
    }
    public static class Intersection
    {
        //http://www.3dkingdoms.com/weekly/weekly.php?a=3
        static bool GetIntersection(float fDst1, float fDst2, float[] P1, float[] P2, float[] Hit)
        {
            /*Hit = new Vector3();*/
            if ((fDst1 * fDst2) >= 0.0f) return false;
            if (fDst1 == fDst2) return false;
            /*Hit = P1 + (P2 - P1) * (-fDst1 / (fDst2 - fDst1));*/
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
            /*Hit = new Vector3();*/
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
                /*Hit = L1;*/
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

        static float SMALL_NUM = 0.00000001f; // anything that avoids division overflow
        // dot product (3D) which allows vector operations in arguments
        static float dot(float[] u, float[] v) { return u[0] * v[0] + u[1] * v[1] + u[2] * v[2]; }
        static void cross(float[] a, float[] b, float[] result)
        {
            result[0] = a[1] * b[2] - a[2] * b[1];
            result[1] = a[2] * b[0] - a[0] * b[2];
            result[2] = a[0] * b[1] - a[1] * b[0];
        }

        static float[] u = new float[3];// triangle vectors
        static float[] v = new float[3];
        static float[] n = new float[3];


        static float[] dir = new float[3];// ray vectors
        static float[] w0 = new float[3];
        static float[] w = new float[3];

        // intersect_RayTriangle(): intersect a ray with a 3D triangle
        //    Input:  a ray R, and a triangle T
        //    Output: *I = intersection point (when it exists)
        //    Return: -1 = triangle is degenerate (a segment or point)
        //             0 = disjoint (no intersect)
        //             1 = intersect in unique point I1
        //             2 = are in the same plane
        public static int
        RayTriangle(Line3D R, Triangle3D T, float[] I)
        {
            float r, a, b;             // params to calc ray-plane intersect

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
            if (Math.Abs(b) < SMALL_NUM)
            {     // ray is parallel to triangle plane
                if (a == 0)                // ray lies in triangle plane
                    return 2;
                else return 0;             // ray disjoint from plane
            }

            // get intersect point of ray with triangle plane
            r = a / b;
            if (r < 0.0)                   // ray goes away from triangle
                return 0;                  // => no intersect
            // for a segment, also test if (r > 1.0) => no intersect

            I[0] = R.Start[0] + r * dir[0];           // intersect point of ray and plane
            I[1] = R.Start[1] + r * dir[1];
            I[2] = R.Start[2] + r * dir[2];

            // is I inside T?
            float uu, uv, vv, wu, wv, D;
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
            float s, t;
            s = (uv * wv - vv * wu) / D;
            if (s < 0.0 || s > 1.0)        // I is outside T
                return 0;
            t = (uv * wu - uu * wv) / D;
            if (t < 0.0 || (s + t) > 1.0)  // I is outside T
                return 0;

            return 1;                      // I is in T
        }
        const float floatMaxValue = 3.40282e+038f;
        static float[] big = new float[3];
        static float[] closest = new float[3];
        static float[] a = new float[3];
        static float[] b = new float[3];
        static float[] outIntersection = new float[3];
        public static BlockPosSide CheckLineBoxExact(Line3D line, Box3D box)
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
            foreach (Triangle3DAndSide t in BoxTrianglesAndSides(box.MinEdge, box.MaxEdge))
            {
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
            throw new Exception();
        }

        static Triangle3DAndSide[] triangleandside_pool;
        public static Triangle3DAndSide[] BoxTrianglesAndSides(float[] a, float[] b)
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
            foreach (Triangle3D t in BoxTriangles(a, b))
            {
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
        static short[] myelements;
        static float[][] myvertices;
        static Triangle3D[] trianglespool;
        public static Triangle3D[] BoxTriangles(float[] a, float[] b)
        {
            float x = a[0];
            float z = a[1];
            float y = a[2];
            float sx = b[0] - a[0];
            float sz = b[1] - a[1];
            float sy = b[2] - a[2];
            //List<short> myelements = new List<short>();
            if (myelements == null)
            {
                myelements = new short[6 * 6];
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
                short lastelement = (short)myverticesCount;
                Vec3.Set(myvertices[myverticesCount++], x + 0.0f * sx, z + 1.0f * sz, y + 0.0f * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 0.0f * sx, z + 1.0f * sz, y + 1.0f * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1.0f * sx, z + 1.0f * sz, y + 0.0f * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1.0f * sx, z + 1.0f * sz, y + 1.0f * sy);
                myelements[myelementsCount++] = (short)(lastelement + 0);
                myelements[myelementsCount++] = (short)(lastelement + 1);
                myelements[myelementsCount++] = (short)(lastelement + 2);
                myelements[myelementsCount++] = (short)(lastelement + 3);
                myelements[myelementsCount++] = (short)(lastelement + 1);
                myelements[myelementsCount++] = (short)(lastelement + 2);
            }
            //bottom - same as top, but z is 1 less.
            //if (drawbottom)
            {
                short lastelement = (short)myverticesCount;
                Vec3.Set(myvertices[myverticesCount++], x + 0.0f * sx, z + 0 * sz, y + 0.0f * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 0.0f * sx, z + 0 * sz, y + 1.0f * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1.0f * sx, z + 0 * sz, y + 0.0f * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1.0f * sx, z + 0 * sz, y + 1.0f * sy);
                myelements[myelementsCount++] = ((short)(lastelement + 0));
                myelements[myelementsCount++] = ((short)(lastelement + 1));
                myelements[myelementsCount++] = ((short)(lastelement + 2));
                myelements[myelementsCount++] = ((short)(lastelement + 3));
                myelements[myelementsCount++] = ((short)(lastelement + 1));
                myelements[myelementsCount++] = ((short)(lastelement + 2));
            }
            //front
            //if (drawfront)
            {
                short lastelement = (short)myverticesCount;
                Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 0 * sz, y + 0 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 0 * sz, y + 1 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 1 * sz, y + 0 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 1 * sz, y + 1 * sy);
                myelements[myelementsCount++] = ((short)(lastelement + 0));
                myelements[myelementsCount++] = ((short)(lastelement + 1));
                myelements[myelementsCount++] = ((short)(lastelement + 2));
                myelements[myelementsCount++] = ((short)(lastelement + 3));
                myelements[myelementsCount++] = ((short)(lastelement + 1));
                myelements[myelementsCount++] = ((short)(lastelement + 2));
            }
            //back - same as front, but x is 1 greater.
            //if (drawback)
            {
                short lastelement = (short)myverticesCount;
                Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 0 * sz, y + 0 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 0 * sz, y + 1 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 1 * sz, y + 0 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 1 * sz, y + 1 * sy);
                myelements[myelementsCount++] = ((short)(lastelement + 0));
                myelements[myelementsCount++] = ((short)(lastelement + 1));
                myelements[myelementsCount++] = ((short)(lastelement + 2));
                myelements[myelementsCount++] = ((short)(lastelement + 3));
                myelements[myelementsCount++] = ((short)(lastelement + 1));
                myelements[myelementsCount++] = ((short)(lastelement + 2));
            }
            //if (drawleft)
            {
                short lastelement = (short)myverticesCount;
                Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 0 * sz, y + 0 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 1 * sz, y + 0 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 0 * sz, y + 0 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 1 * sz, y + 0 * sy);

                myelements[myelementsCount++] = ((short)(lastelement + 0));
                myelements[myelementsCount++] = ((short)(lastelement + 1));
                myelements[myelementsCount++] = ((short)(lastelement + 2));
                myelements[myelementsCount++] = ((short)(lastelement + 3));
                myelements[myelementsCount++] = ((short)(lastelement + 1));
                myelements[myelementsCount++] = ((short)(lastelement + 2));
            }
            //right - same as left, but y is 1 greater.
            //if (drawright)
            {
                short lastelement = (short)myverticesCount;
                Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 0 * sz, y + 1 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 0 * sx, z + 1 * sz, y + 1 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 0 * sz, y + 1 * sy);
                Vec3.Set(myvertices[myverticesCount++], x + 1 * sx, z + 1 * sz, y + 1 * sy);
                myelements[myelementsCount++] = ((short)(lastelement + 0));
                myelements[myelementsCount++] = ((short)(lastelement + 1));
                myelements[myelementsCount++] = ((short)(lastelement + 2));
                myelements[myelementsCount++] = ((short)(lastelement + 3));
                myelements[myelementsCount++] = ((short)(lastelement + 1));
                myelements[myelementsCount++] = ((short)(lastelement + 2));
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
        private static bool PointInBox(float[] v, Box3D node)
        {
            return v[0] >= node.MinEdge[0] && v[1] >= node.MinEdge[1] && v[2] >= node.MinEdge[1]
                && v[0] <= node.MaxEdge[0] && v[1] <= node.MaxEdge[1] && v[2] <= node.MaxEdge[2];
        }
        private static float[] Interpolate(float[] a, float[] b, float f)
        {
            float x = a[0] + (b[0] - a[0]) * f;
            float y = a[1] + (b[1] - a[1]) * f;
            float z = a[2] + (b[2] - a[2]) * f;
            return new float[] { x, y, z };
        }

    }
}
