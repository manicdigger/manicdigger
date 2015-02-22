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
        glTextures = new int[10];
        glTexturesLength = 10;
        empty = new int[modelsMax];
        emptyCount = 0;
    }

    internal Game game;
    internal FrustumCulling d_FrustumCulling;
    internal bool BindTexture;
    ListInfo[] models;
    int modelsCount;

    int[] empty;
    int emptyCount;


    public int Add(ModelData modelData, bool transparent, int texture, float centerX, float centerY, float centerZ, float radius)
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

        Model model = game.platform.CreateModel(modelData);

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
        game.platform.DeleteModel(models[id].model);
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
                game.platform.BindTexture2d(glTextures[i]);
            }
            game.DrawModels(tocallSolid[i].Lists, tocallSolid[i].Count);
        }
        game.platform.GlDisableCullFace(); // for water.
        for (int i = 0; i < texturesCount; i++)
        {
            if (tocallTransparent[i].Count == 0) { continue; }
            if (BindTexture)
            {
                game.platform.BindTexture2d(glTextures[i]);
            }
            game.DrawModels(tocallTransparent[i].Lists, tocallTransparent[i].Count);
        }
        game.platform.GlEnableCullFace();
    }

    // Finds an index in glTextures array.
    int GetTextureId(int glTexture)
    {
        int id = ArrayIndexOf(glTextures, glTexturesLength, glTexture);
        if (id != -1)
        {
            return id;
        }
        id = ArrayIndexOf(glTextures, glTexturesLength, 0);
        if (id != -1)
        {
            glTextures[id] = glTexture;
            return id;
        }
        int increase = 10;
        //Array.Resize(ref glTextures, glTextures.Length + increase);
        int[] glTextures2 = new int[glTexturesLength + increase];
        for (int i = 0; i < glTexturesLength; i++)
        {
            glTextures2[i] = glTextures[i];
        }
        glTextures = glTextures2;
        glTexturesLength = glTexturesLength + increase;

        glTextures[glTexturesLength - increase] = glTexture;
        return glTexturesLength - increase;
    }

    int ArrayIndexOf(int[] glTextures, int length, int glTexture)
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
                int max = 256;
                tocallSolid[i].Lists = new Model[max];
                tocallSolid[i].Max = max;
                tocallTransparent[i].Lists = new Model[max];
                tocallTransparent[i].Max = max;
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
            ToCall tocall;
            if (!li.transparent)
            {
                tocall = tocallSolid[li.texture];
            }
            else
            {
                tocall = tocallTransparent[li.texture];
            }
            if (tocall.Count >= tocall.Max)
            {
                Model[] old = tocall.Lists;
                Model[] new_ = new Model[tocall.Max * 2];
                for (int k = 0; k < tocall.Max; k++)
                {
                    new_[k] = old[k];
                }
                tocall.Lists = new_;
                tocall.Max = tocall.Max * 2;
            }
            tocall.Lists[tocall.Count++] = models[i].model;
        }
    }

    // Maps from inner texture id to real opengl texture id.
    int[] glTextures;
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
        int count = modelsCount;
        for (int i = 0; i < count; i++)
        {
            if (!models[i].empty)
            {
                ListInfo li = models[i];
                if (li.render)
                {
                    sum += li.indicescount;
                }
            }
        }
        return sum / 3;
    }

    public void Clear()
    {
        int count = modelsCount;
        for (int i = 0; i < count; i++)
        {
            if (!models[i].empty)
            {
                Remove(i);
            }
        }
    }
}

public class ToCall
{
    internal Model[] Lists;
    internal int Count;
    internal int Max;
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
