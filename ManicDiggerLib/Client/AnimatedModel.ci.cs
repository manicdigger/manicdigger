public class Node
{
    internal string name;
    internal string parentName;
    internal float posx;
    internal float posy;
    internal float posz;
    internal float rotatex;
    internal float rotatey;
    internal float rotatez;
    internal float sizex;
    internal float sizey;
    internal float sizez;
    internal float u;
    internal float v;
    internal float pivotx;
    internal float pivoty;
    internal float pivotz;
    internal float scalex;
    internal float scaley;
    internal float scalez;
}

public class KeyframeType
{
    public const int Position = 1;
    public const int Rotation = 2;
    public const int Size = 3;
    public const int Pivot = 4;
    public const int Scale = 5;

    public static string GetName(int p)
    {
        if (p == Position) { return "pos"; }
        if (p == Rotation) { return "rot"; }
        if (p == Size) { return "siz"; }
        if (p == Pivot) { return "piv"; }
        if (p == Scale) { return "sca"; }
        return "";
    }

    public static int GetValue(string p)
    {
        if (p == "pos") { return Position; }
        if (p == "rot") { return Rotation; }
        if (p == "siz") { return Size; }
        if (p == "piv") { return Pivot; }
        if (p == "sca") { return Scale; }
        return 0;
    }
}

public class Keyframe
{
    internal string animationName;
    internal string nodeName;
    internal int frame;
    internal int keyframeType;
    internal float x;
    internal float y;
    internal float z;
}

public class Animation
{
    internal string name;
    internal int length;
}

public class AnimationGlobal
{
    internal int texw;
    internal int texh;
}

public class AnimatedModel
{
    public AnimatedModel()
    {
        global = new AnimationGlobal();
    }
    internal Node[] nodes;
    internal int nodesCount;
    internal Keyframe[] keyframes;
    internal int keyframesCount;
    internal Animation[] animations;
    internal int animationsCount;
    internal AnimationGlobal global;
}

public class AnimatedModelBinding : TableBinding
{
    internal GamePlatform p;
    internal AnimatedModel m;
    public override void Set(string table, int index, string column, string value)
    {
        if (table == "nodes")
        {
            if (index >= m.nodesCount) { m.nodesCount = index + 1; }
            if (m.nodes[index] == null) { m.nodes[index] = new Node(); }
            Node k = m.nodes[index];
            if (column == "name") { k.name = value; }
            if (column == "paren") { k.parentName = value; }
            if (column == "x") { k.posx = FloatParse(value); }
            if (column == "y") { k.posy = FloatParse(value); }
            if (column == "z") { k.posz = FloatParse(value); }
            if (column == "rotx") { k.rotatex = FloatParse(value); }
            if (column == "roty") { k.rotatey = FloatParse(value); }
            if (column == "rotz") { k.rotatez = FloatParse(value); }
            if (column == "sizex") { k.sizex = FloatParse(value); }
            if (column == "sizey") { k.sizey = FloatParse(value); }
            if (column == "sizez") { k.sizez = FloatParse(value); }
            if (column == "u") { k.u = FloatParse(value); }
            if (column == "v") { k.v = FloatParse(value); }
            if (column == "pivx") { k.pivotx = FloatParse(value); }
            if (column == "pivy") { k.pivoty = FloatParse(value); }
            if (column == "pivz") { k.pivotz = FloatParse(value); }
            if (column == "scalx") { k.scalex = FloatParse(value); }
            if (column == "scaly") { k.scaley = FloatParse(value); }
            if (column == "scalz") { k.scalez = FloatParse(value); }
        }
        if (table == "keyframes")
        {
            if (index >= m.keyframesCount) { m.keyframesCount = index + 1; }
            if (m.keyframes[index] == null) { m.keyframes[index] = new Keyframe(); }
            Keyframe k = m.keyframes[index];
            if (column == "anim") { k.animationName = value; }
            if (column == "node") { k.nodeName = value; }
            if (column == "frame") { k.frame = IntParse(value); }
            if (column == "type") { k.keyframeType = KeyframeType.GetValue(value); }
            if (column == "x") { k.x = FloatParse(value); }
            if (column == "y") { k.y = FloatParse(value); }
            if (column == "z") { k.z = FloatParse(value); }
        }
        if (table == "animations")
        {
            if (index >= m.animationsCount) { m.animationsCount = index + 1; }
            if (m.animations[index] == null) { m.animations[index] = new Animation(); }
            Animation k = m.animations[index];
            if (column == "name") { k.name = value; }
            if (column == "len") { k.length = IntParse(value); }
        }
        if (table == "global")
        {
            AnimationGlobal global = m.global;
            if (column == "texw") { global.texw = IntParse(value); }
            if (column == "texh") { global.texh = IntParse(value); }
        }
    }

    public override void Get(string table, int index, DictionaryStringString items)
    {
        if (table == "nodes")
        {
            Node k = m.nodes[index];
            items.Set("name", k.name);
            items.Set("paren", k.parentName);
            items.Set("x", p.FloatToString(k.posx));
            items.Set("y", p.FloatToString(k.posy));
            items.Set("z", p.FloatToString(k.posz));
            items.Set("rotx", p.FloatToString(k.rotatex));
            items.Set("roty", p.FloatToString(k.rotatey));
            items.Set("rotz", p.FloatToString(k.rotatez));
            items.Set("sizex", p.FloatToString(k.sizex));
            items.Set("sizey", p.FloatToString(k.sizey));
            items.Set("sizez", p.FloatToString(k.sizez));
            items.Set("u", p.FloatToString(k.u));
            items.Set("v", p.FloatToString(k.v));
            items.Set("pivx", p.FloatToString(k.pivotx));
            items.Set("pivy", p.FloatToString(k.pivoty));
            items.Set("pivz", p.FloatToString(k.pivotz));
            items.Set("scalx", p.FloatToString(k.scalex));
            items.Set("scaly", p.FloatToString(k.scaley));
            items.Set("scalz", p.FloatToString(k.scalez));
        }
        if (table == "keyframes")
        {
            Keyframe k = m.keyframes[index];
            items.Set("anim", k.animationName);
            items.Set("node", k.nodeName);
            items.Set("frame", p.FloatToString(k.frame));
            items.Set("type", KeyframeType.GetName(k.frame));
            items.Set("x", p.FloatToString(k.x));
            items.Set("y", p.FloatToString(k.y));
            items.Set("z", p.FloatToString(k.z));
        }
        if (table == "animations")
        {
            Animation k = m.animations[index];
            items.Set("name", k.name);
            items.Set("len", p.FloatToString(k.length));
        }
        if (table == "global")
        {
            AnimationGlobal global = m.global;
            items.Set("texw", p.FloatToString(global.texw));
            items.Set("texh", p.FloatToString(global.texh));
        }
    }

    public void GetTables(string[] name, int[] count)
    {
        name[0] = "nodes"; count[0] = m.nodesCount;
        name[1] = "keyframes"; count[1] = m.keyframesCount;
        name[2] = "animations"; count[2] = m.animationsCount;
        name[3] = "global"; count[3] = 1;
    }

    int IntParse(string s)
    {
        return p.FloatToInt(FloatParse(s));
    }

    float FloatParse(string s)
    {
        FloatRef ret = new FloatRef();
        p.FloatTryParse(s, ret);
        return ret.value;
    }
}

public abstract class TableBinding
{
    public abstract void Set(string table, int index, string column, string value);
    public abstract void Get(string table, int index, DictionaryStringString items);
}

public class TableSerializer
{
    public void Deserialize(GamePlatform p, string data, TableBinding b)
    {
        IntRef linesCount = new IntRef();
        string[] lines = p.ReadAllLines(data, linesCount);
        string[] header = null;
        IntRef headerLength = new IntRef();
        string current = "";
        int currentI = 0;
        for (int i = 0; i < linesCount.value; i++)
        {
            string s = p.StringTrim(lines[i]);
            if (s == "")
            {
                continue;
            }
            if (p.StringStartsWithIgnoreCase(s, "//")
                || p.StringStartsWithIgnoreCase(s, "#"))
            {
                continue;
            }
            if (p.StringStartsWithIgnoreCase(s, "section="))
            {
                current = p.StringReplace(s, "section=", "");

                string sHeader = p.StringTrim(lines[i + 1]);
                header = p.StringSplit(sHeader, "\t", headerLength);
                i++; // header
                currentI = 0;
                continue;
            }
            {
                IntRef ssLength = new IntRef();
                string[] ss = p.StringSplit(s, "\t", ssLength);
                for (int k = 0; k < ssLength.value; k++)
                {
                    b.Set(current, currentI, header[k], ss[k]);
                }

                currentI++;
            }
        }
    }
}

public class AnimatedModelSerializer
{
    const int sectionNodes = 1;
    const int sectionKeyframes = 2;
    const int sectionAnimations = 3;
    const int sectionGlobal = 4;
    public static AnimatedModel Deserialize(GamePlatform p, string data)
    {
        AnimatedModel model = new AnimatedModel();
        model.nodes = new Node[256];
        model.keyframes = new Keyframe[1024];
        model.animations = new Animation[128];
        AnimatedModelBinding b = new AnimatedModelBinding();
        b.p = p;
        b.m = model;
        TableSerializer s = new TableSerializer();
        s.Deserialize(p, data, b);
        return model;
    }

    public static string Serialize(GamePlatform p, AnimatedModel m)
    {
        return null;
    }
}

public class AnimatedModelRenderer
{
    public AnimatedModelRenderer()
    {
        one = 1;
        tempframes = new Keyframe[256];
        tempframesCount = new IntRef();
        tempVec3 = new float[3];
    }
    float one;
    internal Game game;
    internal void Start(Game game_, AnimatedModel model_)
    {
        game = game_;
        m = model_;
    }

    AnimatedModel m;

    int anim;
    const int fps = 60;
    float frame;
    public void Render(float dt)
    {
        frame += dt * fps;
        float length = m.animations[anim].length;
        frame = frame % length;
        DrawNode("root");
    }

    float[] tempVec3;
    void DrawNode(string parent)
    {
        for (int i = 0; i < m.nodesCount; i++)
        {
            Node n = m.nodes[i];
            if (n == null)
            {
                continue;
            }
            if (n.parentName != parent)
            {
                continue;
            }
            game.GLPushMatrix();
            RectangleFloat[] r = new RectangleFloat[6];
            r = CuboidRenderer.CuboidNet(n.sizex, n.sizey, n.sizez, n.u, n.v);
            CuboidRenderer.CuboidNetNormalize(r, m.global.texw, m.global.texh);
            GetAnimation(n, tempVec3, KeyframeType.Scale);
            if (tempVec3[0] != 0 && tempVec3[1] != 0 && tempVec3[2] != 0)
            {
                game.GLScale(tempVec3[0], tempVec3[1], tempVec3[2]);
            }
            GetAnimation(n, tempVec3, KeyframeType.Position);
            tempVec3[0] /= 16;
            tempVec3[1] /= 16;
            tempVec3[2] /= 16;
            if (!IsZero(tempVec3))
            {
                game.GLTranslate(tempVec3[0], tempVec3[1], tempVec3[2]);
            }
            GetAnimation(n, tempVec3, KeyframeType.Rotation);
            if (tempVec3[0] != 0)
            {
                game.GLRotate(tempVec3[0], 1, 0, 0);
            }
            if (tempVec3[1] != 0)
            {
                game.GLRotate(tempVec3[1], 0, 1, 0);
            }
            if (tempVec3[2] != 0)
            {
                game.GLRotate(tempVec3[2], 0, 0, 1);
            }
            GetAnimation(n, tempVec3, KeyframeType.Pivot);
            tempVec3[0] /= 16;
            tempVec3[1] /= 16;
            tempVec3[2] /= 16;
            game.GLTranslate(tempVec3[0], tempVec3[1], tempVec3[2]);
            GetAnimation(n, tempVec3, KeyframeType.Size);
            tempVec3[0] /= 16;
            tempVec3[1] /= 16;
            tempVec3[2] /= 16;
            CuboidRenderer.DrawCuboid2(game, -tempVec3[0] / 2, -tempVec3[1] / 2, -tempVec3[2] / 2, tempVec3[0], tempVec3[1], tempVec3[2], r, 1);
            DrawNode(n.name);
            game.GLPopMatrix();
        }
    }

    bool IsZero(float[] vec)
    {
        return vec[0] == 0
            && vec[1] == 0
            && vec[2] == 0;
    }

    void GetAnimation(Node node, float[] ret, int type)
    {
        GetFrames(node.name, type, tempframes, tempframesCount);
        int currentI = GetFrameCurrent(tempframes, tempframesCount.value);
        if (currentI == -1)
        {
            GetDefaultFrame(node, type, ret);
            return;
        }
        int nextI = (currentI + 1) % tempframesCount.value;

        Keyframe current = tempframes[currentI];
        Keyframe next = tempframes[nextI];
        float t;
        float length = m.animations[anim].length;
        if (next.frame == current.frame)
        {
            t = 0;
        }
        else if (next.frame > current.frame)
        {
            t = (frame - current.frame) / (next.frame - current.frame);
        }
        else
        {
            float end = 0;
            float begin = 0;
            if (frame >= current.frame)
            {
                end = (frame - current.frame);
            }
            else
            {
                end = (length - current.frame);
                begin = frame;
            }
            t = (end + begin) / ((length - current.frame) + next.frame);
        }
        ret[0] = Lerp(current.x, next.x, t);
        ret[1] = Lerp(current.y, next.y, t);
        ret[2] = Lerp(current.z, next.z, t);
    }

    void GetDefaultFrame(Node node, int type, float[] ret)
    {
        switch (type)
        {
            case KeyframeType.Position:
                {
                    ret[0] = node.posx;
                    ret[1] = node.posy;
                    ret[2] = node.posz;
                }
                break;
            case KeyframeType.Rotation:
                {
                    ret[0] = node.rotatex;
                    ret[1] = node.rotatey;
                    ret[2] = node.rotatez;
                }
                break;
            case KeyframeType.Size:
                {
                    ret[0] = node.sizex;
                    ret[1] = node.sizey;
                    ret[2] = node.sizez;
                }
                break;
            case KeyframeType.Pivot:
                {
                    ret[0] = node.pivotx;
                    ret[1] = node.pivoty;
                    ret[2] = node.pivotz;
                }
                break;
            case KeyframeType.Scale:
                {
                    ret[0] = node.scalex;
                    ret[1] = node.scaley;
                    ret[2] = node.scalez;
                }
                break;
        }
    }

    float Lerp(float v0, float v1, float t)
    {
        return v0 + (v1 - v0) * t;
    }


    void GetFrames(string nodeName, int type, Keyframe[] frames, IntRef count)
    {
        count.value = 0;
        string animName = m.animations[anim].name;
        for (int i = 0; i < m.keyframesCount; i++)
        {
            Keyframe k = m.keyframes[i];
            if (k == null)
            {
                continue;
            }
            if (k.nodeName != nodeName)
            {
                continue;
            }
            if (k.animationName != animName)
            {
                continue;
            }
            if (k.keyframeType != type)
            {
                continue;
            }
            frames[count.value++] = k;
        }
    }

    Keyframe[] tempframes;
    IntRef tempframesCount;
    int GetFrameCurrent(Keyframe[] frames, int framesCount)
    {
        string animName = m.animations[anim].name;
        int current = -1;
        for (int i = 0; i < framesCount; i++)
        {
            Keyframe k = frames[i];
            if (k.frame <= frame)
            {
                //any previous frame
                if (current == -1)
                {
                    current = i;
                }
                else
                {
                    //closest previous frame
                    if (k.frame > frames[current].frame)
                    {
                        current = i;
                    }
                }
            }

        }
        if (current == -1)
        {
            //not found. use last frame
            for (int i = 0; i < framesCount; i++)
            {
                Keyframe k = frames[i];
                if (current == -1 || k.frame > frames[current].frame)
                {
                    current = i;
                }
            }
        }
        return current;
    }
}
