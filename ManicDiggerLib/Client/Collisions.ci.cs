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

