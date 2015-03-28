using ProtoBuf;

[ProtoContract()]
public class ServerEntity
{
    [ProtoMember(1, IsRequired = false)]
    internal ServerEntityPositionAndOrientation position;
    [ProtoMember(2, IsRequired = false)]
    internal ServerEntityDrawName drawName;
    [ProtoMember(3, IsRequired = false)]
    internal ServerEntityAnimatedModel drawModel;
    [ProtoMember(4, IsRequired = false)]
    internal ServerEntityDrawText drawText;
    [ProtoMember(5, IsRequired = false)]
    internal ServerEntityPush push;
    [ProtoMember(7, IsRequired = false)]
    internal bool usable;
    [ProtoMember(9, IsRequired = false)]
    internal ServerEntityDrawArea drawArea;

    [ProtoMember(6, IsRequired = false)]
    internal ServerEntitySign sign;
    [ProtoMember(8, IsRequired = false)]
    internal ServerEntityPermissionSign permissionSign;
}

[ProtoContract()]
public class ServerEntityDrawArea
{
    internal int x;
    internal int y;
    internal int z;
    internal int sizex;
    internal int sizey;
    internal int sizez;
    internal int visibleToClientId;
}

[ProtoContract()]
public class ServerEntityDrawName
{
    [ProtoMember(1)]
    internal string name;
    [ProtoMember(2)]
    internal bool onlyWhenSelected;
    [ProtoMember(3)]
    internal bool clientAutoComplete;
    [ProtoMember(4)]
    internal string color;
}

[ProtoContract()]
public class ServerEntitySign
{
    [ProtoMember(1)]
    internal string text;
}

[ProtoContract()]
public class ServerEntityPermissionSign
{
    [ProtoMember(1)]
    internal string name;
    [ProtoMember(2)]
    internal PermissionSignType type;
}

public enum PermissionSignType
{
    Player,
    Group,
}

[ProtoContract()]
public class ServerEntityAnimatedModel
{
    [ProtoMember(1, IsRequired = false)]
    internal string model;
    [ProtoMember(2, IsRequired = false)]
    internal string texture;
    [ProtoMember(3, IsRequired = false)]
    internal float eyeHeight;
    [ProtoMember(4, IsRequired = false)]
    internal float modelHeight;
    [ProtoMember(5, IsRequired = false)]
    internal bool downloadSkin;
}

[ProtoContract()]
public class ServerEntityPositionAndOrientation
{
    [ProtoMember(1, IsRequired = false)]
    internal float x;
    [ProtoMember(2, IsRequired = false)]
    internal float y;
    [ProtoMember(3, IsRequired = false)]
    internal float z;
    [ProtoMember(4, IsRequired = false)]
    internal byte heading;
    [ProtoMember(5, IsRequired = false)]
    internal byte pitch;
    [ProtoMember(6, IsRequired = false)]
    internal byte stance;

    internal ServerEntityPositionAndOrientation Clone()
    {
        ServerEntityPositionAndOrientation ret = new ServerEntityPositionAndOrientation();
        ret.x = x;
        ret.y = y;
        ret.z = z;
        ret.heading = heading;
        ret.pitch = pitch;
        ret.stance = stance;
        return ret;
    }
}

[ProtoContract()]
public class ServerEntityDrawText
{
    [ProtoMember(1, IsRequired = false)]
    internal string text;
    [ProtoMember(2, IsRequired = false)]
    internal float dx;
    [ProtoMember(3, IsRequired = false)]
    internal float dy;
    [ProtoMember(4, IsRequired = false)]
    internal float dz;
    [ProtoMember(5, IsRequired = false)]
    internal float rotx;
    [ProtoMember(6, IsRequired = false)]
    internal float roty;
    [ProtoMember(7, IsRequired = false)]
    internal float rotz;
}

[ProtoContract()]
public class ServerEntityPush
{
    [ProtoMember(1)]
    internal float range;
}
