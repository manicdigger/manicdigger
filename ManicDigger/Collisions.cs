using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;


namespace ManicDigger.Collisions
{
    public interface ITriangleContainer
    {
    }
    public struct Line3D
    {
        public Vector3 Start;
        public Vector3 End;
    }
    public struct Triangle3D
    {
        public Vector3 PointA;
        public Vector3 PointB;
        public Vector3 PointC;
    }
    public struct Box3D
    {
        public Box3D(float x, float y, float z, float size)
        {
            this.MinEdge = new Vector3(x, y, z);
            this.MaxEdge = new Vector3(x + size, y + size, z + size);
        }
        public Vector3 MinEdge;
        public Vector3 MaxEdge;
        //public Vector3 MaxEdge { get { return new Vector3(MinEdge.X + size, MinEdge.Y + size, MinEdge.Z + size); } }
        //float size;
        public float LengthX { get { return MaxEdge.X - MinEdge.X; } }
        public float LengthY { get { return MaxEdge.Y - MinEdge.Y; } }
        public float LengthZ { get { return MaxEdge.Z - MinEdge.Z; } }
        public void AddPoint(float x, float y, float z)
        {
            //if is empty
            if (MinEdge == new Vector3(0, 0, 0) && MaxEdge == new Vector3(0, 0, 0))
            {
                MinEdge = new Vector3(x, y, z);
                MaxEdge = new Vector3(x, y, z);
            }
            MinEdge.X = Math.Min(MinEdge.X, x);
            MinEdge.Y = Math.Min(MinEdge.Y, y);
            MinEdge.Z = Math.Min(MinEdge.Z, z);
            MaxEdge.X = Math.Max(MaxEdge.X, x);
            MaxEdge.Y = Math.Max(MaxEdge.Y, y);
            MaxEdge.Z = Math.Max(MaxEdge.Z, z);
        }
        public Vector3 Center()
        {
            return (MinEdge + MaxEdge) / 2;
        }
    }
    public interface ITriangleSearcher
    {
        int AddTriangle(Triangle3D triangle);
        void DeleteTriangle(int triangle_id);
        IEnumerable<Vector3> LineIntersection(Line3D line);
    }
    public class BlockOctreeSearcher
    {
        public Box3D StartBox;
        IEnumerable<Box3D> Search(Predicate<Box3D> query)
        {
            if (StartBox.LengthX == 0)
            {
                throw new Exception();
            }
            return SearchPrivate(query, StartBox);
        }
        IEnumerable<Box3D> SearchPrivate(Predicate<Box3D> query, Box3D box)
        {
            if (box.LengthX == 1)
            {
                yield return box;
                yield break;
            }
            foreach (Box3D child in Children(box))
            {
                if (query(child))
                {
                    foreach (Box3D n in SearchPrivate(query, child))
                    {
                        yield return n;
                    }
                }
            }
        }
        IEnumerable<Box3D> Children(Box3D box)
        {
            float x = box.MinEdge.X;
            float y = box.MinEdge.Y;
            float z = box.MinEdge.Z;
            float size = box.LengthX / 2;
            yield return new Box3D(x, y, z, size);
            yield return new Box3D(x + size, y, z, size);
            yield return new Box3D(x, y, z + size, size);
            yield return new Box3D(x + size, y, z + size, size);

            yield return new Box3D(x, y + size, z, size);
            yield return new Box3D(x + size, y + size, z, size);
            yield return new Box3D(x, y + size, z + size, size);
            yield return new Box3D(x + size, y + size, z + size, size);
        }
        public delegate bool IsBlockEmpty(int x, int y, int z);
        public delegate float GetBlockHeight(int x, int y, int z);
        bool BoxHit(Box3D box)
        {
            return Intersection.CheckLineBox(box, currentLine, out currentHit);
        }
        Line3D currentLine;
        Vector3 currentHit;
        public IEnumerable<BlockPosSide> LineIntersection(IsBlockEmpty isEmpty, GetBlockHeight getBlockHeight, Line3D line)
        {
            currentLine = line;
            currentHit = new Vector3();
            foreach (var node in Search(BoxHit))
            {
                Vector3 hit = currentHit;
                int x = (int)node.MinEdge.X;
                int y = (int)node.MinEdge.Z;
                int z = (int)node.MinEdge.Y;
                if (!isEmpty(x, y, z))
                {
                    var node2 = node;
                    node2.MaxEdge.Y = node2.MinEdge.Y + getBlockHeight(x, y, z);
                    var hit2 = Intersection.CheckLineBoxExact(line, node2);
                    if (hit2 != null)
                    {
                        yield return hit2.Value;
                    }
                }
            }
        }
    }
    public enum TileSide
    {
        Top,
        Bottom,
        Front,
        Back,
        Left,
        Right,
    }
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
    public struct BlockPosSide
    {
        public BlockPosSide(int x, int y, int z, TileSide side)
        {
            this.pos = new Vector3(x, y, z);
            this.side = side;
        }
        public Vector3 pos;
        public TileSide side;
        public Vector3 Translated()
        {
            if (side == TileSide.Top) { return pos + new Vector3(0, 0, 0); }
            if (side == TileSide.Bottom) { return pos + new Vector3(0, -1, 0); }
            if (side == TileSide.Front) { return pos + new Vector3(-1, 0, 0); }
            if (side == TileSide.Back) { return pos + new Vector3(0, 0, 0); }
            if (side == TileSide.Left) { return pos + new Vector3(0, 0, -1); }
            if (side == TileSide.Right) { return pos + new Vector3(0, 0, 0); }
            throw new Exception();
        }
        public Vector3 Current()
        {
            //these are block coordinates. 0.1f is used instead of 1f,
            //because some blocks have height less than 1.
            //After substracting 0.1f from 0.3f block height and Math.flooring
            //it will be a correct block coordinate.
            //todo check.
            if (side == TileSide.Top) { return pos + new Vector3(0, -0.1f, 0); }
            if (side == TileSide.Bottom) { return pos + new Vector3(0, 0, 0); }
            if (side == TileSide.Front) { return pos + new Vector3(0, 0, 0); }
            if (side == TileSide.Back) { return pos + new Vector3(-0.1f, 0, 0); }
            if (side == TileSide.Left) { return pos + new Vector3(0, 0, 0); }
            if (side == TileSide.Right) { return pos + new Vector3(0, 0, -0.1f); }
            throw new Exception();
        }
    }
    public static class Intersection
    {
        //http://www.3dkingdoms.com/weekly/weekly.php?a=3
        static bool GetIntersection(float fDst1, float fDst2, Vector3 P1, Vector3 P2, out Vector3 Hit)
        {
            Hit = new Vector3();
            if ((fDst1 * fDst2) >= 0.0f) return false;
            if (fDst1 == fDst2) return false;
            Hit = P1 + (P2 - P1) * (-fDst1 / (fDst2 - fDst1));
            return true;
        }
        static bool InBox(Vector3 Hit, Vector3 B1, Vector3 B2, int Axis)
        {
            if (Axis == 1 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
            if (Axis == 2 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.X > B1.X && Hit.X < B2.X) return true;
            if (Axis == 3 && Hit.X > B1.X && Hit.X < B2.X && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
            return false;
        }
        // returns true if line (L1, L2) intersects with the box (B1, B2)
        // returns intersection point in Hit
        public static bool CheckLineBox(Vector3 B1, Vector3 B2, Vector3 L1, Vector3 L2, out Vector3 Hit)
        {
            Hit = new Vector3();
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
            if ((GetIntersection(L1.X - B1.X, L2.X - B1.X, L1, L2, out Hit) && InBox(Hit, B1, B2, 1))
              || (GetIntersection(L1.Y - B1.Y, L2.Y - B1.Y, L1, L2, out Hit) && InBox(Hit, B1, B2, 2))
              || (GetIntersection(L1.Z - B1.Z, L2.Z - B1.Z, L1, L2, out Hit) && InBox(Hit, B1, B2, 3))
              || (GetIntersection(L1.X - B2.X, L2.X - B2.X, L1, L2, out Hit) && InBox(Hit, B1, B2, 1))
              || (GetIntersection(L1.Y - B2.Y, L2.Y - B2.Y, L1, L2, out Hit) && InBox(Hit, B1, B2, 2))
              || (GetIntersection(L1.Z - B2.Z, L2.Z - B2.Z, L1, L2, out Hit) && InBox(Hit, B1, B2, 3)))
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
        public static bool CheckLineBox(Box3D box, Line3D line, out Vector3 hit)
        {
            return CheckLineBox(box.MinEdge, box.MaxEdge, line.Start, line.End, out hit);
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
        static float dot(Vector3 u, Vector3 v) { return (u).X * (v).X + (u).Y * (v).Y + (u).Z * (v).Z; }

        // intersect_RayTriangle(): intersect a ray with a 3D triangle
        //    Input:  a ray R, and a triangle T
        //    Output: *I = intersection point (when it exists)
        //    Return: -1 = triangle is degenerate (a segment or point)
        //             0 = disjoint (no intersect)
        //             1 = intersect in unique point I1
        //             2 = are in the same plane
        public static int
        RayTriangle(Line3D R, Triangle3D T, out Vector3 I)
        {
            Vector3 u, v, n;             // triangle vectors
            Vector3 dir, w0, w;          // ray vectors
            float r, a, b;             // params to calc ray-plane intersect

            I = new Vector3();

            // get triangle edge vectors and plane normal
            u = T.PointB - T.PointA;
            v = T.PointC - T.PointA;
            //n = u.CrossProduct(v);             // cross product
            Vector3.Cross(ref u, ref v, out n);
            //if (n == (Vector3D)0)            // triangle is degenerate
            //    return -1;                 // do not deal with this case

            dir = R.End - R.Start;             // ray direction vector
            w0 = R.Start - T.PointA;
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

            I = R.Start + r * dir;           // intersect point of ray and plane

            // is I inside T?
            float uu, uv, vv, wu, wv, D;
            uu = dot(u, u);
            uv = dot(u, v);
            vv = dot(v, v);
            w = I - T.PointA;
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
        public static BlockPosSide? CheckLineBoxExact(Line3D line, Box3D box)
        {
            if (PointInBox(line.Start, box)) { return new BlockPosSide() { pos = line.Start }; }
            Vector3 big = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 closest = big;
            TileSide side = TileSide.Top;
            foreach (Triangle3DAndSide t in BoxTrianglesAndSides(box.MinEdge, box.MaxEdge))
            {
                Vector3 i;
                if (RayTriangle(line, t.t, out i) != 0)
                {
                    if ((line.Start - i).Length < (line.Start - closest).Length)
                    {
                        closest = i;
                        side = t.side;
                    }
                }
            }
            //if (closest == big) { throw new Exception(); }
            if (closest == big) { return null; }
            return new BlockPosSide() { pos = closest, side = side };
            //if (PointInBox(line.End, box)) { return new TilePosSide() { pos = line.End }; }
            throw new Exception();
        }
        public class Triangle3DAndSide
        {
            public Triangle3D t;
            public TileSide side;
        }
        public static IEnumerable<Triangle3DAndSide> BoxTrianglesAndSides(Vector3 a, Vector3 b)
        {
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
                yield return new Triangle3DAndSide() { t = t, side = side };
            }
        }
        public static IEnumerable<Triangle3D> BoxTriangles(Vector3 a, Vector3 b)
        {
            float x = a.X;
            float z = a.Y;
            float y = a.Z;
            float sx = b.X - a.X;
            float sz = b.Y - a.Y;
            float sy = b.Z - a.Z;
            List<short> myelements = new List<short>();
            List<Vector3> myvertices = new List<Vector3>();
            //top
            //if (drawtop)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 0.0f * sx, z + 1.0f * sz, y + 0.0f * sy));
                myvertices.Add(new Vector3(x + 0.0f * sx, z + 1.0f * sz, y + 1.0f * sy));
                myvertices.Add(new Vector3(x + 1.0f * sx, z + 1.0f * sz, y + 0.0f * sy));
                myvertices.Add(new Vector3(x + 1.0f * sx, z + 1.0f * sz, y + 1.0f * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            //bottom - same as top, but z is 1 less.
            //if (drawbottom)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 0.0f * sx, z + 0 * sz, y + 0.0f * sy));
                myvertices.Add(new Vector3(x + 0.0f * sx, z + 0 * sz, y + 1.0f * sy));
                myvertices.Add(new Vector3(x + 1.0f * sx, z + 0 * sz, y + 0.0f * sy));
                myvertices.Add(new Vector3(x + 1.0f * sx, z + 0 * sz, y + 1.0f * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            //front
            //if (drawfront)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 0 * sx, z + 0 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 0 * sx, z + 0 * sz, y + 1 * sy));
                myvertices.Add(new Vector3(x + 0 * sx, z + 1 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 0 * sx, z + 1 * sz, y + 1 * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            //back - same as front, but x is 1 greater.
            //if (drawback)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 1 * sx, z + 0 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 0 * sz, y + 1 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 1 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 1 * sz, y + 1 * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            //if (drawleft)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 0 * sx, z + 0 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 0 * sx, z + 1 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 0 * sz, y + 0 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 1 * sz, y + 0 * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            //right - same as left, but y is 1 greater.
            //if (drawright)
            {
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new Vector3(x + 0 * sx, z + 0 * sz, y + 1 * sy));
                myvertices.Add(new Vector3(x + 0 * sx, z + 1 * sz, y + 1 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 0 * sz, y + 1 * sy));
                myvertices.Add(new Vector3(x + 1 * sx, z + 1 * sz, y + 1 * sy));
                myelements.Add((short)(lastelement + 0));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
                myelements.Add((short)(lastelement + 3));
                myelements.Add((short)(lastelement + 1));
                myelements.Add((short)(lastelement + 2));
            }
            for (int i = 0; i < myelements.Count / 3; i++)
            {
                Triangle3D t = new Triangle3D();
                t.PointA = myvertices[myelements[i * 3 + 0]];
                t.PointB = myvertices[myelements[i * 3 + 1]];
                t.PointC = myvertices[myelements[i * 3 + 2]];
                yield return t;
            }
        }
        private static bool PointInBox(Vector3 v, Box3D node)
        {
            return v.X >= node.MinEdge.X && v.Y >= node.MinEdge.Y && v.Z >= node.MinEdge.Z
                && v.X <= node.MaxEdge.X && v.Y <= node.MaxEdge.Y && v.Z <= node.MaxEdge.Z;
        }
        private static Vector3 Interpolate(Vector3 a, Vector3 b, float f)
        {
            float x = a.X + (b.X - a.X) * f;
            float y = a.Y + (b.Y - a.Y) * f;
            float z = a.Z + (b.Z - a.Z) * f;
            return new Vector3(x, y, z);
        }
    }
}