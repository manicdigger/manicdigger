public class CharacterPhysicsState
{
    public CharacterPhysicsState()
    {
        float one = 1;
        movedz = 0;
        playerposition = Vector3Ref.Create(15 + one / 2, 40, 15 + one / 2);
        playerorientation = Vector3Ref.Create(GetPi(), 0, 0);
        curspeed = new Vector3Ref();
        jumpacceleration = 0;
        isplayeronground = false;
    }
    internal float movedz;
    internal Vector3Ref playerposition;
    internal Vector3Ref playerorientation;
    internal Vector3Ref curspeed;
    internal float jumpacceleration;
    internal bool isplayeronground;

    static float GetPi()
    {
        float a = 3141592;
        return a / 1000000;
    }
}

public class Acceleration
{
    public Acceleration()
    {
        float one = 1;
        acceleration1 = one * 9 / 10;
        acceleration2 = 2;
        acceleration3 = 700;
    }
    internal float acceleration1;
    internal float acceleration2;
    internal float acceleration3;
}

public class Vector3Ref
{
    internal float X;
    internal float Y;
    internal float Z;

    internal float Length()
    {
        return Platform.Sqrt(X * X + Y * Y + Z * Z);
    }

    internal void Normalize()
    {
        float length = Length();
        X = X / length;
        Y = Y / length;
        Z = Z / length;
    }

    internal static Vector3Ref Create(float x, float y, float z)
    {
        Vector3Ref v = new Vector3Ref();
        v.X = x;
        v.Y = y;
        v.Z = z;
        return v;
    }
}

public class MoveInfo
{
    internal bool ENABLE_FREEMOVE;
    internal bool Swimming;
    internal Acceleration acceleration;
    internal float movespeednow;
    internal int movedx;
    internal int movedy;
    internal bool ENABLE_NOCLIP;
    internal bool wantsjump;
    internal bool moveup;
    internal bool movedown;
    internal float jumpstartacceleration;
}
