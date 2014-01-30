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
