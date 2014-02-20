public class MeshBatcher
{
    public MeshBatcher()
    {
        int modelsMax = 1024 * 16;
        models = new ListInfo[modelsMax];
        for (int i = 0; i < modelsMax; i++)
        {
            models[i] = new ListInfo();
        }
        modelsCount = 0;
        BindTexture = true;
        glTextures = new Texture[10];
        glTexturesLength = 10;
        empty = new int[modelsMax];
        emptyCount = 0;
    }

    internal Game game;
    internal IFrustumCulling d_FrustumCulling;
    internal bool BindTexture;
    ListInfo[] models;
    int modelsCount;

    int[] empty;
    int emptyCount;


    public int Add(ModelData modelData, bool transparent, Texture texture, float centerX, float centerY, float centerZ, float radius)
    {
        int id;
        if (emptyCount > 0)
        {
            id = empty[emptyCount - 1];
            emptyCount--;
        }
        else
        {
            id = modelsCount;
            modelsCount++;
        }

        Model model = game.p.CreateModel(modelData);

        ListInfo li = models[id];
        li.indicescount = modelData.GetIndicesCount();
        li.centerX = centerX;
        li.centerY = centerY;
        li.centerZ = centerZ;
        li.radius = radius;
        li.transparent = transparent;
        li.empty = false;
        li.texture = GetTextureId(texture);
        li.model = model;

        return id;
    }

    public void Remove(int id)
    {
        models[id].empty = true;
        empty[emptyCount++] = id;
    }

    public void Draw(float playerPositionX, float playerPositionY, float playerPositionZ)
    {
        UpdateCulling();
        SortListsByTexture();

        //Need to first render all solid lists (to fill z-buffer), then transparent.
        for (int i = 0; i < texturesCount; i++)
        {
            if (tocallSolid[i].Count == 0) { continue; }
            if (BindTexture)
            {
                game.p.BindTexture2d(glTextures[i]);
            }
            game.p.DrawModels(tocallSolid[i].Lists, tocallSolid[i].Count);
        }
        game.p.GlDisableCullFace(); // for water.
        for (int i = 0; i < texturesCount; i++)
        {
            if (tocallTransparent[i].Count == 0) { continue; }
            if (BindTexture)
            {
                game.p.BindTexture2d(glTextures[i]);
            }
            game.p.DrawModels(tocallTransparent[i].Lists, tocallTransparent[i].Count);
        }
        game.p.GlEnableCullFace();
    }

    // Finds an index in glTextures array.
    int GetTextureId(Texture glTexture)
    {
        int id = ArrayIndexOf(glTextures, glTexturesLength, glTexture);
        if (id != -1)
        {
            return id;
        }
        id = ArrayIndexOf(glTextures, glTexturesLength, null);
        if (id != -1)
        {
            glTextures[id] = glTexture;
            return id;
        }
        int increase = 10;
        //Array.Resize(ref glTextures, glTextures.Length + increase);
        Texture[] glTextures2 = new Texture[glTexturesLength + increase];
        for (int i = 0; i < glTexturesLength; i++)
        {
            glTextures2[i] = glTextures[i];
        }
        glTextures = glTextures2;
        glTexturesLength = glTexturesLength + increase;

        glTextures[glTexturesLength - increase] = glTexture;
        return glTexturesLength - increase;
    }

    int ArrayIndexOf(Texture[] glTextures, int length, Texture glTexture)
    {
        for (int i = 0; i < length; i++)
        {
            if (glTextures[i] == glTexture)
            {
                return i;
            }
        }
        return -1;
    }

    public const int MAX_DISPLAY_LISTS = 32 * 1024;
    void SortListsByTexture()
    {
        if (tocallSolid == null)
        {
            tocallSolid = new ToCall[texturesCount];
            tocallTransparent = new ToCall[texturesCount];
            for (int i = 0; i < texturesCount; i++)
            {
                tocallSolid[i] = new ToCall();
                tocallTransparent[i] = new ToCall();
            }
            for (int i = 0; i < texturesCount; i++)
            {
                tocallSolid[i].Lists = new Model[MAX_DISPLAY_LISTS];
                tocallTransparent[i].Lists = new Model[MAX_DISPLAY_LISTS];
            }
        }
        for (int i = 0; i < texturesCount; i++)
        {
            tocallSolid[i].Count = 0;
            tocallTransparent[i].Count = 0;
        }
        for (int i = 0; i < modelsCount; i++)
        {
            ListInfo li = models[i];
            if (!li.render)
            {
                continue;
            }
            if (li.empty)
            {
                continue;
            }
            if (!li.transparent)
            {
                ToCall tocall = tocallSolid[li.texture];
                tocall.Lists[tocall.Count++] = models[i].model;
            }
            else
            {
                ToCall tocall = tocallTransparent[li.texture];
                tocall.Lists[tocall.Count++] = models[i].model;
            }
        }
    }

    // Maps from inner texture id to real opengl texture id.
    Texture[] glTextures;
    int glTexturesLength;
    ToCall[] tocallSolid;
    ToCall[] tocallTransparent;
    // todo dynamic
    public const int texturesCount = 10;

    // Not really needed because display lists perform (at least on some computers)
    // their own frustum culling automatically.
    void UpdateCulling()
    {
        int licount = modelsCount;
        for (int i = 0; i < licount; i++)
        {
            ListInfo li = models[i];
            float centerX = li.centerX;
            float centerY = li.centerY;
            float centerZ = li.centerZ;
            li.render = d_FrustumCulling.SphereInFrustum(centerX, centerY, centerZ, li.radius);
        }
    }

    public int TotalTriangleCount()
    {
        int sum = 0;
        for (int i = 0; i < modelsCount; i++)
        {
            if (!EmptyContains(i))
            {
                ListInfo li = models[i];
                if (li == null)
                {
                    continue;
                }
                if (li.render)
                {
                    sum += li.indicescount;
                }
            }
        }
        return sum / 3;
    }

    bool EmptyContains(int id)
    {
        for (int i = 0; i < emptyCount; i++)
        {
            if (empty[i] == id)
            {
                return true;
            }
        }
        return false;
    }
}

public class ToCall
{
    internal Model[] Lists;
    internal int Count;
}

public class ToAdd
{
    internal ModelData modelData;
    internal int id;
    internal bool transparent;
    internal int texture;
    internal float centerX;
    internal float centerY;
    internal float centerZ;
    internal float radius;
}

public class ListInfo
{
    public ListInfo()
    {
        render = true;
    }
    internal bool empty;
    internal int indicescount;
    internal float centerX;
    internal float centerY;
    internal float centerZ;
    internal float radius;
    internal bool transparent;
    internal bool render;
    internal int texture;
    internal Model model;
}
