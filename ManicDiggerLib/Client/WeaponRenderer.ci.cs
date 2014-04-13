public class WeaponRenderer
{
    public WeaponRenderer()
    {
        one = 1;
        attackt = 0;
        buildt = 0;
        range = one * 7 / 100;
        speed = 5;
        animperiod = Game.GetPi() / (speed / 2);
        zzzposz = 0;
        t_ = 0;
        zzzx = -27;
        zzzy = -one * 137 / 10;
        zzzposx = -one * 2 / 10;
        zzzposy = -one * 4 / 10;
        attack = -1;
        build = false;
        slowdownTimerSpecial = 32 * 1000;
    }
    float one;

    internal Game game;
    internal BlockRendererTorch d_BlockRendererTorch;

    public int terrainTexture() { return game.terrainTexture; }
    public int texturesPacked() { return game.texturesPacked(); }
    public int GetWeaponTextureId(int side)
    {
        Packet_Item item = game.d_Inventory.RightHand[game.ActiveMaterial];
        if (item == null || IsCompass() || (item != null && item.BlockId == 0))
        {
            //empty hand
            if (side == TileSide.Top) { return game.TextureId[game.d_Data.BlockIdEmptyHand()][TileSide.Top]; }
            return game.TextureId[game.d_Data.BlockIdEmptyHand()][TileSide.Front];
        }
        if (item.ItemClass == Packet_ItemClassEnum.Block)
        {
            return game.TextureId[item.BlockId][side];
        }
        else
        {
            //todo
            return 0;
        }
    }
    const int maxlight = 15;
    public float Light()
    {
        Vector3Ref pos = game.player.playerposition;
        int light = game.MaybeGetLight(game.platform.FloatToInt(pos.X), game.platform.FloatToInt(pos.Z), game.platform.FloatToInt(pos.Y));
        return (one * light) / maxlight;
    }
    public bool IsTorch()
    {
        Packet_Item item = game.d_Inventory.RightHand[game.ActiveMaterial];
        return item != null
            && item.ItemClass == Packet_ItemClassEnum.Block
            && game.blocktypes[item.BlockId].DrawType == Packet_DrawTypeEnum.Torch;
    }
    public bool IsCompass()
    {
        Packet_Item item = game.d_Inventory.RightHand[game.ActiveMaterial];
        return item != null
            && item.ItemClass == Packet_ItemClassEnum.Block
            && item.BlockId == game.d_Data.BlockIdCompass();
    }
    public bool IsEmptyHand()
    {
        Packet_Item item = game.d_Inventory.RightHand[game.ActiveMaterial];
        return item == null || item.BlockId == 0;
    }

    public void SetAttack(bool isattack, bool build)
    {
        this.build = build;
        if (isattack)
        {
            if (attack == -1)
            {
                attack = 0;
            }
        }
        else
        {
            attack = -1;
        }
    }
    float attack;
    bool build;
    ModelData modelData;
    int oldMaterial;
    float oldLight;
    float slowdownTimer;
    float slowdownTimerSpecial;
    internal bool redraw;
    public void DrawWeapon(float dt)
    {
        int light;
        if (IsTorch())
        {
            light = 255;
        }
        else
        {
            light = game.platform.FloatToInt(Light() * 256);
            if (light > 255) { light = 255; }
            if (light < 0) { light = 0; }
        }
        game.platform.BindTexture2d(terrainTexture());

        Packet_Item item = game.d_Inventory.RightHand[game.ActiveMaterial];
        int curmaterial;
        if (item == null)
        {
            curmaterial = 0;
        }
        else
        {
            curmaterial = item.BlockId == 151 ? 128 : item.BlockId;
        }
        float curlight = Light();
        if (curmaterial != oldMaterial || curlight != oldLight || modelData == null || redraw)
        {
            redraw = false;
            modelData = new ModelData();
            modelData.indices = new int[128];
            modelData.xyz = new float[128];
            modelData.uv = new float[128];
            modelData.rgba = new byte[128];
            int x = 0;
            int y = 0;
            int z = 0;
            if (IsEmptyHand() || IsCompass())
            {
                d_BlockRendererTorch.TopTexture = GetWeaponTextureId(TileSide.Top);
                d_BlockRendererTorch.SideTexture = GetWeaponTextureId(TileSide.Front);
                d_BlockRendererTorch.AddTorch(modelData, x, y, z, TorchType.Normal);
            }
            else if (IsTorch())
            {
                d_BlockRendererTorch.TopTexture = GetWeaponTextureId(TileSide.Top);
                d_BlockRendererTorch.SideTexture = GetWeaponTextureId(TileSide.Front);
                d_BlockRendererTorch.AddTorch(modelData, x, y, z, TorchType.Normal);
            }
            else
            {
                DrawCube(modelData, x, y, z, Game.ColorFromArgb(255, light, light, light));
            }
        }
        oldMaterial = curmaterial;
        oldLight = curlight;

        game.platform.GlClearDepthBuffer();
        game.GLMatrixModeModelView();
        game.GLPushMatrix();
        game.GLLoadIdentity();

        game.GLTranslate((one * 3 / 10) + zzzposz - attackt * 5, -(one * 15 / 10) + zzzposx - buildt * 10, -(one * 15 / 10) + zzzposy);
        game.GLRotate(30 + (zzzx) - attackt * 300, 1, 0, 0);
        game.GLRotate(60 + zzzy, 0, 1, 0);
        game.GLScale(one * 8 / 10, one * 8 / 10, one * 8 / 10);

        bool move = !(oldplayerposX == game.player.playerposition.X
            && oldplayerposY == game.player.playerposition.Y
            && oldplayerposZ == game.player.playerposition.Z);
        oldplayerposX = game.player.playerposition.X;
        oldplayerposY = game.player.playerposition.Y;
        oldplayerposZ = game.player.playerposition.Z;
        if (move)
        {
            t_ += dt;
            slowdownTimer = slowdownTimerSpecial;
        }
        else
        {
            if (slowdownTimer == slowdownTimerSpecial)
            {
                slowdownTimer = (animperiod / 2 - (t_ % (animperiod / 2)));
            }
            slowdownTimer -= dt;
            if (slowdownTimer < 0)
            {
                t_ = 0;
            }
            else
            {
                t_ += dt;
            }
        }
        zzzposx = rot(t_);
        zzzposz = rot2(t_);
        if (attack != -1)
        {
            attack += dt * 7;
            if (attack > Game.GetPi() / 2)
            {
                attack = -1;
                if (build)
                {
                    buildt = 0;
                }
                else
                {
                    attackt = 0;
                }
            }
            else
            {
                if (build)
                {
                    buildt = rot(attack / 5);
                    attackt = 0;
                }
                else
                {
                    attackt = rot(attack / 5);
                    buildt = 0;
                }
            }
        }

        game.platform.GlEnableTexture2d();
        game.platform.BindTexture2d(terrainTexture());
        game.DrawModelData(modelData);

        game.GLPopMatrix();
    }
    float attackt;
    float buildt;
    float range;
    float speed;
    float animperiod;
    float oldplayerposX;
    float oldplayerposY;
    float oldplayerposZ;
    float zzzposz;
    float t_;
    float rot(float t)
    {
        return game.platform.MathSin(t * 2 * speed) * range;
    }
    float rot2(float t)
    {
        return game.platform.MathSin((t + Game.GetPi()) * speed) * range;
    }
    void DrawCube(ModelData m, int x, int y, int z, int c)
    {
        //top
        //if (drawtop)
        {
            int sidetexture = GetWeaponTextureId(TileSide.Top);
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, x + 0, z + 1, y + 0, texrec.Left(), texrec.Top(), c);
            AddVertex(m, x + 0, z + 1, y + 1, texrec.Left(), texrec.Bottom(), c);
            AddVertex(m, x + 1, z + 1, y + 0, texrec.Right(), texrec.Top(), c);
            AddVertex(m, x + 1, z + 1, y + 1, texrec.Right(), texrec.Bottom(), c);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }
        //bottom - same as top, but z is 1 less.
        //if (drawbottom)
        {
            int sidetexture = GetWeaponTextureId(TileSide.Bottom);
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, x + 0, z, y + 0, texrec.Left(), texrec.Top(), c);
            AddVertex(m, x + 0, z, y + 1, texrec.Left(), texrec.Bottom(), c);
            AddVertex(m, x + 1, z, y + 0, texrec.Right(), texrec.Top(), c);
            AddVertex(m, x + 1, z, y + 1, texrec.Right(), texrec.Bottom(), c);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }
        // //front
        //if (drawfront)
        {
            int sidetexture = GetWeaponTextureId(TileSide.Front);
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, x + 0, z + 0, y + 0, texrec.Left(), texrec.Bottom(), c);
            AddVertex(m, x + 0, z + 0, y + 1, texrec.Right(), texrec.Bottom(), c);
            AddVertex(m, x + 0, z + 1, y + 0, texrec.Left(), texrec.Top(), c);
            AddVertex(m, x + 0, z + 1, y + 1, texrec.Right(), texrec.Top(), c);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }
        //back - same as front, but x is 1 greater.
        //if (drawback)
        {//todo fix tcoords
            int sidetexture = GetWeaponTextureId(TileSide.Back);
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, x + 1, z + 0, y + 0, texrec.Left(), texrec.Bottom(), c);
            AddVertex(m, x + 1, z + 0, y + 1, texrec.Right(), texrec.Bottom(), c);
            AddVertex(m, x + 1, z + 1, y + 0, texrec.Left(), texrec.Top(), c);
            AddVertex(m, x + 1, z + 1, y + 1, texrec.Right(), texrec.Top(), c);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }
        //if (drawleft)
        {
            int sidetexture = GetWeaponTextureId(TileSide.Left);
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, x + 0, z + 0, y + 0, texrec.Left(), texrec.Bottom(), c);
            AddVertex(m, x + 0, z + 1, y + 0, texrec.Left(), texrec.Top(), c);
            AddVertex(m, x + 1, z + 0, y + 0, texrec.Right(), texrec.Bottom(), c);
            AddVertex(m, x + 1, z + 1, y + 0, texrec.Right(), texrec.Top(), c);
            m.indices[m.indicesCount++] = (lastelement + 0);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 2);
            m.indices[m.indicesCount++] = (lastelement + 1);
            m.indices[m.indicesCount++] = (lastelement + 3);
            m.indices[m.indicesCount++] = (lastelement + 2);
        }
        //right - same as left, but y is 1 greater.
        //if (drawright)
        {//todo fix tcoords
            int sidetexture = GetWeaponTextureId(TileSide.Right);
            RectFRef texrec = TextureAtlas.TextureCoords2d(sidetexture, texturesPacked());
            int lastelement = m.GetVerticesCount();
            AddVertex(m, x + 0, z + 0, y + 1, texrec.Left(), texrec.Bottom(), c);
            AddVertex(m, x + 0, z + 1, y + 1, texrec.Left(), texrec.Top(), c);
            AddVertex(m, x + 1, z + 0, y + 1, texrec.Right(), texrec.Bottom(), c);
            AddVertex(m, x + 1, z + 1, y + 1, texrec.Right(), texrec.Top(), c);
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
    float zzzx;
    float zzzy;
    float zzzposx;
    float zzzposy;
    //float attackprogress = 0;
}
