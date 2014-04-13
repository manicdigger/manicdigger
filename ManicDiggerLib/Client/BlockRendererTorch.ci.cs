public class TextureAtlas
{
    public static RectFRef TextureCoords2d(int textureId, int texturesPacked)
    {
        float one = 1;
        RectFRef r = new RectFRef();
        r.y = (one / texturesPacked * (textureId / texturesPacked));
        r.x = (one / texturesPacked * (textureId % texturesPacked));
        r.w = one / texturesPacked;
        r.h = one / texturesPacked;
        return r;
    }
}

public enum TorchType
{
    Normal, Left, Right, Front, Back
}

public class BlockRendererTorch
{
    internal GameData d_Data;
    internal ITerrainTextures d_TerainRenderer;
    internal int TopTexture;
    internal int SideTexture;
    public void AddTorch(ModelData m, int x, int y, int z, TorchType type)
    {
        float one = 1;
        int curcolor = Game.ColorFromArgb(255, 255, 255, 255);
        float torchsizexy = one * 16 / 100;
        float topx = one / 2 - torchsizexy / 2;
        float topy = one / 2 - torchsizexy / 2;
        float bottomx = one / 2 - torchsizexy / 2;
        float bottomy = one / 2 - torchsizexy / 2;

        topx += x;
        topy += y;
        bottomx += x;
        bottomy += y;

        if (type == TorchType.Front) { bottomx = x - torchsizexy; }
        if (type == TorchType.Back) { bottomx = x + 1; }
        if (type == TorchType.Left) { bottomy = y - torchsizexy; }
        if (type == TorchType.Right) { bottomy = y + 1; }

        Vector3Ref top00 = Vector3Ref.Create(topx, z + (one * 9 / 10), topy);
        Vector3Ref top01 = Vector3Ref.Create(topx, z + (one * 9 / 10), topy + torchsizexy);
        Vector3Ref top10 = Vector3Ref.Create(topx + torchsizexy, z + (one * 9 / 10), topy);
        Vector3Ref top11 = Vector3Ref.Create(topx + torchsizexy, z + (one * 9 / 10), topy + torchsizexy);

        if (type == TorchType.Left)
        {
            top01.Y += -(one * 1 / 10);
            top11.Y += -(one * 1 / 10);
        }

        if (type == TorchType.Right)
        {
            top10.Y += -(one * 1 / 10);
            top00.Y += -(one * 1 / 10);
        }

        if (type == TorchType.Front)
        {
            top10.Y += -(one * 1 / 10);
            top11.Y += -(one * 1 / 10);
        }

        if (type == TorchType.Back)
        {
            top01.Y += -(one * 1 / 10);
            top00.Y += -(one * 1 / 10);
        }

        Vector3Ref bottom00 = Vector3Ref.Create(bottomx, z + 0, bottomy);
        Vector3Ref bottom01 = Vector3Ref.Create(bottomx, z + 0, bottomy + torchsizexy);
        Vector3Ref bottom10 = Vector3Ref.Create(bottomx + torchsizexy, z + 0, bottomy);
        Vector3Ref bottom11 = Vector3Ref.Create(bottomx + torchsizexy, z + 0, bottomy + torchsizexy);

        //top
        {
            int sidetexture = TopTexture;
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, top00.X, top00.Y, top00.Z, texrec.Left(), texrec.Top(), curcolor);
            AddVertex(m, top01.X, top01.Y, top01.Z, texrec.Left(), texrec.Bottom(), curcolor);
            AddVertex(m, top10.X, top10.Y, top10.Z, texrec.Right(), texrec.Top(), curcolor);
            AddVertex(m, top11.X, top11.Y, top11.Z, texrec.Right(), texrec.Bottom(), curcolor);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }

        //bottom - same as top, but z is 1 less.
        {
            int sidetexture = SideTexture;
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, bottom00.X, bottom00.Y, bottom00.Z, texrec.Left(), texrec.Top(), curcolor);
            AddVertex(m, bottom01.X, bottom01.Y, bottom01.Z, texrec.Left(), texrec.Bottom(), curcolor);
            AddVertex(m, bottom10.X, bottom10.Y, bottom10.Z, texrec.Right(), texrec.Top(), curcolor);
            AddVertex(m, bottom11.X, bottom11.Y, bottom11.Z, texrec.Right(), texrec.Bottom(), curcolor);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }

        //front
        {
            int sidetexture = SideTexture;
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, bottom00.X, bottom00.Y, bottom00.Z, texrec.Left(), texrec.Bottom(), curcolor);
            AddVertex(m, bottom01.X, bottom01.Y, bottom01.Z, texrec.Right(), texrec.Bottom(), curcolor);
            AddVertex(m, top00.X, top00.Y, top00.Z, texrec.Left(), texrec.Top(), curcolor);
            AddVertex(m, top01.X, top01.Y, top01.Z, texrec.Right(), texrec.Top(), curcolor);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }

        //back - same as front, but x is 1 greater.
        {
            int sidetexture = SideTexture;
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, bottom10.X, bottom10.Y, bottom10.Z, texrec.Right(), texrec.Bottom(), curcolor);
            AddVertex(m, bottom11.X, bottom11.Y, bottom11.Z, texrec.Left(), texrec.Bottom(), curcolor);
            AddVertex(m, top10.X, top10.Y, top10.Z, texrec.Right(), texrec.Top(), curcolor);
            AddVertex(m, top11.X, top11.Y, top11.Z, texrec.Left(), texrec.Top(), curcolor);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }

        {
            int sidetexture = SideTexture;
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, bottom00.X, bottom00.Y, bottom00.Z, texrec.Right(), texrec.Bottom(), curcolor);
            AddVertex(m, top00.X, top00.Y, top00.Z, texrec.Right(), texrec.Top(), curcolor);
            AddVertex(m, bottom10.X, bottom10.Y, bottom10.Z, texrec.Left(), texrec.Bottom(), curcolor);
            AddVertex(m, top10.X, top10.Y, top10.Z, texrec.Left(), texrec.Top(), curcolor);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }

        //right - same as left, but y is 1 greater.
        {
            int sidetexture = SideTexture;
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, bottom01.X, bottom01.Y, bottom01.Z, texrec.Left(), texrec.Bottom(), curcolor);
            AddVertex(m, top01.X, top01.Y, top01.Z, texrec.Left(), texrec.Top(), curcolor);
            AddVertex(m, bottom11.X, bottom11.Y, bottom11.Z, texrec.Right(), texrec.Bottom(), curcolor);
            AddVertex(m, top11.X, top11.Y, top11.Z, texrec.Right(), texrec.Top(), curcolor);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }
    }
    public void AddVertex(ModelData model, float x, float y, float z, float u, float v, int color)
    {
        model.xyz[model.GetXyzCount() + 0] = x;
        model.xyz[model.GetXyzCount() + 1] = y;
        model.xyz[model.GetXyzCount() + 2] = z;
        model.uv[model.GetUvCount() + 0] = u;
        model.uv[model.GetUvCount() + 1] = v;
        model.rgba[model.GetRgbaCount() + 0] = Game.IntToByte(Game.ColorR(color));
        model.rgba[model.GetRgbaCount() + 1] = Game.IntToByte(Game.ColorG(color));
        model.rgba[model.GetRgbaCount() + 2] = Game.IntToByte(Game.ColorB(color));
        model.rgba[model.GetRgbaCount() + 3] = Game.IntToByte(Game.ColorA(color));
        model.verticesCount++;
    }
}
