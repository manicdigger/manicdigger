public class AnimationState
{
    public AnimationState()
    {
        bodyrotation = -1;
        light = 1;
    }
    internal float interp;
    internal Variables data;
    internal float headbodydelta;
    internal bool fullbodyrotate;
    internal float lastheading;
    internal float bodyrotation;
    internal float speed;
    internal float light;

    public float GetInterp()
    {
        return interp;
    }
}

public class AnimationHint
{
    internal bool InVehicle;
    internal float DrawFixX;
    internal float DrawFixY;
    internal float DrawFixZ;
    internal bool leanleft;
    internal bool leanright;
}

public abstract class ICharacterRenderer
{
    public abstract string[] Animations(IntRef retCount);
    public abstract float GetAnimPeriod();
    public abstract void SetAnimPeriod(float value);
    public abstract void SetAnimation(string p);
    public abstract void DrawCharacter(AnimationState animstate, float posX, float posY, float posZ, byte heading, byte pitch, bool moves, float dt, int playertexture, AnimationHint animationhint, float playerspeed);
}

public class CharacterRendererMonsterCode : ICharacterRenderer
{
    public CharacterRendererMonsterCode()
    {
        one = 1;
        animperiod = one * 3 / 10;
        animperiodUpDown = one * 8 / 10;
        jumpheight = one * 25 / 1000; //0.025f;
        code = new ValueArray[8 * 1024];
    }
    float one;
    internal Game game;
    public void Load(string[] origcode, int origcodeCount)
    {
        this.codeCount = 0;
        for (int i = 0; i < origcodeCount; i++)
        {
            string s = origcode[i];
            if (StringTools.StringLength(game.platform, game.platform.StringTrim(s)) == 0
                || StringTools.StringStartsWith(game.platform, s, "//"))
            {
                continue;
            }
            IntRef ssLength = new IntRef();
            string[] ss = game.platform.StringSplit(s, ",", ssLength);

            Value__[] ss2 = new Value__[ssLength.value];

            for (int ii = 0; ii < ssLength.value; ii++)
            {
                ss[ii] = game.platform.StringTrim(ss[ii]);
                FloatRef d = new FloatRef();
                ss2[ii] = new Value__();
                if (game.platform.FloatTryParse(ss[ii], d))
                {
                    ss2[ii].valueFloat = d.value;
                    ss2[ii].type_ = Value__.TypeFloat;
                }
                else
                {
                    ss2[ii].valueString = ss[ii];
                    ss2[ii].type_ = Value__.TypeString;
                }
            }
            ValueArray arr = new ValueArray();
            arr.items = ss2;
            arr.count = ssLength.value;
            this.code[codeCount++] = arr;
        }
    }
    ValueArray[] code;
    int codeCount;
    float animperiod;
    float animperiodUpDown;
    float characterlight;
    float outofphase;
    float speed;
    public override float GetAnimPeriod() { return animperiod; }
    public override void SetAnimPeriod(float value) { animperiod = value; }
    public override void DrawCharacter(AnimationState animstate, float posX, float posY, float posZ, byte heading, byte pitch, bool moves, float dt, int playertexture, AnimationHint animationhint, float playerspeed)
    {
        animstate.interp += dt;
        animstate.speed = playerspeed * 10;
        speed = animstate.speed;
        //Caps maximum player arm/leg movement. Fixes "crazy arms" at 10x speed.
        if (speed > 2)
        {
            speed = 2;
            animstate.speed = speed;
        }
        if (animationhint.InVehicle)
        {
            moves = false;
        }
        if (animstate.data == null)
        {
            Variables v = new Variables();
            animstate.data = v;
        }
        Variables variables = animstate.data;
        float headingdeg = ((one * heading) / 256) * 360;
        characterlight = 127 + (heading / 2);
        //Reset player orientation when not yet loaded (prevents wrong-way-heads)
        if (animstate.bodyrotation == -1)
        {
            animstate.bodyrotation = headingdeg;
        }
        //keep track of how far neck is turned
        if (!moves)
        {
            if (headingdeg > animstate.lastheading && !(Game.AbsFloat(headingdeg - animstate.lastheading) > 180))
            {
                animstate.headbodydelta += Game.AbsFloat(headingdeg - animstate.lastheading);
            }
            if (headingdeg < animstate.lastheading && !(Game.AbsFloat(headingdeg - animstate.lastheading) > 180))
            {
                animstate.headbodydelta -= Game.AbsFloat(headingdeg - animstate.lastheading);
            }
        }
        float speed1 = one * 375 / 1000;
        //slowly realign body when walking straight forward
        if (moves && !(animationhint.leanleft || animationhint.leanright))
        {
            if (animstate.headbodydelta > 0)
            {
                animstate.headbodydelta -= (500 * dt) * (speed * speed1);
                animstate.bodyrotation = headingdeg - animstate.headbodydelta;
            }
            if (animstate.headbodydelta < 0)
            {
                animstate.headbodydelta += (500 * dt) * (speed * speed1);
                animstate.bodyrotation = headingdeg - animstate.headbodydelta;
            }
        }
        //rotate body when strafing
        if (animationhint.leanleft)
        {
            animstate.headbodydelta -= (500 * dt);
            animstate.bodyrotation = headingdeg - animstate.headbodydelta;
        }
        if (animationhint.leanright)
        {
            animstate.headbodydelta += (500 * dt);
            animstate.bodyrotation = headingdeg - animstate.headbodydelta;
        }
        //restrict neck rotation
        if (!(animstate.headbodydelta <= -45) && !(animstate.headbodydelta >= 45))
        {
            animstate.fullbodyrotate = false;
        }
        if (animstate.headbodydelta >= 45)
        {
            animstate.fullbodyrotate = true;
            animstate.headbodydelta = 45;
            animstate.bodyrotation = (headingdeg - animstate.headbodydelta);
        }
        if (animstate.headbodydelta <= -45)
        {
            animstate.fullbodyrotate = true;
            animstate.headbodydelta = -45;
            animstate.bodyrotation = (headingdeg - animstate.headbodydelta);
        }
        animstate.lastheading = headingdeg;

        game.GLMatrixModeModelView();
        game.GLPushMatrix();
        game.GLTranslate(posX, posY, posZ);

        variables.SetFloat("heading", heading);
        variables.SetFloat("pitch", pitch);
        variables.SetFloat("headingdeg", (headingdeg));
        variables.SetFloat("pitchdeg", (one * pitch / 256) * 360);
        variables.SetFloat("updown", UpDown(animstate.interp, animperiodUpDown));
        variables.SetFloat("limbrotation1", LeftLegRotation(animstate.interp, animperiod));
        variables.SetFloat("limbrotation2", RightLegRotation(animstate.interp, animperiod));
        variables.SetFloat("skin", playertexture);
        variables.SetFloat("dt", dt);
        variables.SetFloat("time", animstate.interp);
        variables.SetFloat("anim", currentanim);
        variables.SetFloat("hintleanleft", GetLeanLeft(animationhint.leanleft));
        variables.SetFloat("hintleanright", GetLeanRight(animationhint.leanright));
        variables.SetFloat("bodyrotation", animstate.bodyrotation);
        variables.SetFloat("fullbodyrotate", GetFullBodyRotate(animstate.fullbodyrotate));
        IntRef animationsCount = new IntRef();
        string[] animations = Animations(animationsCount);
        for (int i = 0; i < animationsCount.value; i++)
        {
            variables.SetFloat(animations[i], i);
        }
        int skinsizex = 64;
        int skinsizey = 32;
        int pc = 0;
        for (; ; )
        {
            if (pc >= codeCount)
            {
                break;
            }
            Value__[] ss = code[pc].items;
            int ssCount = code[pc].count;
            if (ssCount > 0)
            {
                switch (ss[0].valueString)
                {
                    case "set":
                        {
                            variables.SetFloat(ss[1].valueString, GetFloat(ss[2], variables));
                        }
                        break;
                    case "pushmatrix":
                        {
                            game.GLPushMatrix();
                        }
                        break;
                    case "popmatrix":
                        {
                            game.GLPopMatrix();
                        }
                        break;
                    case "mul":
                        {
                            variables.SetFloat(ss[1].valueString, GetFloat(ss[1], variables) * GetFloat(ss[2], variables));
                        }
                        break;
                    case "add":
                        {
                            variables.SetFloat(ss[1].valueString, GetFloat(ss[1], variables) + GetFloat(ss[2], variables));
                        }
                        break;
                    case "rotate":
                        {
                            game.GLRotate(
                                GetFloat(ss[1], variables),
                                GetFloat(ss[2], variables),
                                GetFloat(ss[3], variables),
                                GetFloat(ss[4], variables));
                        }
                        break;
                    case "translate":
                        {
                            game.GLTranslate(
                                GetFloat(ss[1], variables),
                                GetFloat(ss[2], variables),
                                GetFloat(ss[3], variables));
                        }
                        break;
                    case "scale":
                        {
                            game.GLScale(
                                GetFloat(ss[1], variables),
                                GetFloat(ss[2], variables),
                                GetFloat(ss[3], variables));
                        }
                        break;
                    case "makecoords":
                        {
                            RectangleFloat[] coords = CuboidRenderer.CuboidNet(
                               GetFloat(ss[2], variables),
                               GetFloat(ss[3], variables),
                               GetFloat(ss[4], variables),
                               GetFloat(ss[5], variables),
                               GetFloat(ss[6], variables));
                            CuboidRenderer.CuboidNetNormalize(coords, skinsizex, skinsizey);
                            SetVariableCoords(variables, ss[1].valueString, coords);
                        }
                        break;
                    case "drawcuboid":
                        {
                            game.SetMatrixUniforms();
                            game.platform.BindTexture2d(game.platform.FloatToInt(GetFloat(ss[7], variables)));
                            CuboidRenderer.DrawCuboid(game,
                               GetFloat(ss[1], variables),
                                GetFloat(ss[2], variables),
                                GetFloat(ss[3], variables),
                               GetFloat(ss[4], variables),
                                GetFloat(ss[5], variables),
                                GetFloat(ss[6], variables),
                               GetVariableCoords(ss[8].valueString, variables),
                               animstate.light
                                );
                        }
                        break;
                    case "skinsize":
                        {
                            skinsizex = game.platform.FloatToInt(GetFloat(ss[1], variables));
                            skinsizey = game.platform.FloatToInt(GetFloat(ss[2], variables));
                        }
                        break;
                    case "dim":
                        {
                            //if (!variables.ContainsKey(ss[1].valueString))
                            {
                                variables.SetFloat(ss[1].valueString, GetFloat(ss[2], variables));
                            }
                        }
                        break;
                    case "fun":
                        {
                            if (ss[2].valueString == "tri")
                            {
                                variables.SetFloat(ss[1].valueString, TriWave(GetFloat(ss[3], variables)));
                            }
                            if (ss[2].valueString == "sin")
                            {
                                variables.SetFloat(ss[1].valueString, game.platform.MathSin(GetFloat(ss[3], variables)));
                            }
                            if (ss[2].valueString == "abs")
                            {
                                variables.SetFloat(ss[1].valueString, Game.AbsFloat(GetFloat(ss[3], variables)));
                            }
                        }
                        break;
                    case "ifeq":
                        {
                            if (variables.ContainsKey(ss[1].valueString)
                                && GetFloat(ss[1], variables) != GetFloat(ss[2], variables))
                            {
                                //find endif
                                for (int i = pc; i < codeCount; i++)
                                {
                                    if ((code[i].items[0].valueString) == "endif")
                                    {
                                        pc = i;
                                        // goto next;
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            pc++;
        }
        game.GLPopMatrix();
    }

    RectangleFloat[] GetVariableCoords(string p, Variables variables)
    {
        return variables.GetCoords(p);
    }

    void SetVariableCoords(Variables variables, string p, RectangleFloat[] coords)
    {
        variables.SetCoords(p, coords);
    }

    float GetFloat(Value__ value__, Variables variables)
    {
        if (value__.type_ == Value__.TypeFloat)
        {
            return value__.valueFloat;
        }
        else
        {
            return variables.GetFloat(value__.valueString);
        }
    }

    public static float Normalize(float p, float period)
    {
        return (p % period);//(2 * Math.PI * period));
    }
    float jumpheight;
    float UpDown(float time, float period)
    {
        //float jumpheight = 0.10f;
        //return (float)TriWave(2 * Math.PI * time / (period / 2)) * jumpheight + jumpheight / 2;
        float jumpheightNow = jumpheight * speed;
        return game.platform.MathSin(2 * Game.GetPi() * time / (period / 2)) * jumpheightNow + jumpheightNow / 2;
    }
    float LeftLegRotation(float time, float period)
    {
        //return (float)TriWave(2 * Math.PI * time / period) * 90;
        outofphase = Game.GetPi();
        return (((game.platform.MathCos(2 * Game.GetPi() * time / (one / 2) + outofphase) * speed) * 30));// *speed
    }
    float RightLegRotation(float time, float period)
    {
        //return (float)TriWave(2 * Math.PI * time / period + Math.PI) * 90;
        return ((game.platform.MathCos(2 * Game.GetPi() * time / (one / 2)) * speed) * 30);
    }
    float GetLeanLeft(bool leaning)
    {
        if (leaning)
        { return 1; }
        else { return 0; }
    }
    float GetLeanRight(bool leaning)
    {
        if (leaning)
        { return 1; }
        else { return 0; }
    }
    float GetFullBodyRotate(bool fullbodyrotate)
    {
        if (fullbodyrotate)
        { return 1; }
        else { return 0; }
    }
    float TriWave(float t)
    {
        float period = 2 * Game.GetPi();
        t += Game.GetPi() / 2;
        return Game.AbsFloat(one * 2 * (t / period - game.FloorFloat(t / period + (one * 5 / 10)))) * 2 - 1;
    }
    public override string[] Animations(IntRef retCount)
    {
        string[] availableanimations = new string[1024];
        int count = 0;
        for (int i = 0; i < codeCount; i++)
        {
            if ((code[i].items[0].valueString) == "exportanim") //&& code[i].Length > 1)
            {
                string name = (code[i].items[1]).valueString;
                if (!contains(availableanimations, count, name))
                {
                    availableanimations[count++] = name;
                }
            }
        }
        retCount.value = count;
        return availableanimations;
    }

    bool contains(string[] list, int count, string name)
    {
        for (int i = 0; i < count; i++)
        {
            if (list[i] == name)
            {
                return true;
            }
        }
        return false;
    }
    internal int currentanim;
    public override void SetAnimation(string p)
    {
        IntRef count = new IntRef();
        string[] animations = Animations(count);
        for (int i = 0; i < count.value; i++)
        {
            if (animations[i] == p)
            {
                currentanim = i;
            }
        }
    }

    public void SetGame(Game game_)
    {
        game = game_;
    }
}

public class CuboidRenderer
{
    //Maps description of position of 6 faces
    //of a single cuboid in texture file to UV coordinates (in pixels)
    //(one RectangleF in texture file for each 3d face of cuboid).
    //Arguments:
    // Size (in pixels) in 2d cuboid net.
    // Start position of 2d cuboid net in texture file.
    public static RectangleFloat[] CuboidNet(float tsizex, float tsizey, float tsizez, float tstartx, float tstarty)
    {
        RectangleFloat[] coords = new RectangleFloat[6];
        {
            coords[0] = RectangleFloat.Create(tsizez + tstartx, tsizez + tstarty, tsizex, tsizey);//front
            coords[1] = RectangleFloat.Create(2 * tsizez + tsizex + tstartx, tsizez + tstarty, tsizex, tsizey);//back
            coords[2] = RectangleFloat.Create(0 + tstartx, tsizez + tstarty, tsizez, tsizey);//left
            coords[3] = RectangleFloat.Create(tsizez + tsizex + tstartx, tsizez + tstarty, tsizez, tsizey);//right
            coords[4] = RectangleFloat.Create(tsizez + tstartx, 0 + tstarty, tsizex, tsizez);//top
            coords[5] = RectangleFloat.Create(tsizez + tsizex + tstartx, 0 + tstarty, tsizex, tsizez);//bottom
        }
        return coords;
    }
    //Divides CuboidNet() result by texture size, to get relative coordinates. (0-1, not 0-32 pixels).
    public static void CuboidNetNormalize(RectangleFloat[] coords, float texturewidth, float textureheight)
    {
        for (int i = 0; i < 6; i++)
        {
            coords[i] = RectangleFloat.Create((coords[i].X / texturewidth), (coords[i].Y / textureheight),
                (coords[i].Width / texturewidth), (coords[i].Height / textureheight));
        }
    }
    public static void DrawCuboid(Game game, float posX, float posY, float posZ,
        float sizeX, float sizeY, float sizeZ,
        RectangleFloat[] texturecoords, float light)
    {
        ModelData data = new ModelData();
        data.xyz = new float[4 * 6 * 3];
        data.uv = new float[4 * 6 * 2];
        data.rgba = new byte[4 * 6 * 4];
        int light255 = game.platform.FloatToInt(light * 255);
        int color = Game.ColorFromArgb(255, light255, light255, light255);

        RectangleFloat rect;

        //front
        rect = texturecoords[0];
        AddVertex(data, posX, posY, posZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX, posY, posZ + sizeZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX, posY + sizeY, posZ + sizeZ, rect.X + rect.Width, rect.Y, color);
        AddVertex(data, posX, posY + sizeY, posZ, rect.X, rect.Y, color);

        //back
        rect = texturecoords[1];
        AddVertex(data, posX + sizeX, posY, posZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX + sizeX, posY, posZ + sizeZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ + sizeZ, rect.X + rect.Width, rect.Y, color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ, rect.X, rect.Y, color);

        //left
        rect = texturecoords[2];
        AddVertex(data, posX + sizeX, posY, posZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX, posY, posZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX, posY + sizeY, posZ, rect.X + rect.Width, rect.Y, color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ, rect.X, rect.Y, color);

        //right
        rect = texturecoords[3];
        AddVertex(data, posX + sizeX, posY, posZ + sizeZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX, posY, posZ + sizeZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX, posY + sizeY, posZ + sizeZ, rect.X, rect.Y, color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ + sizeZ, rect.X + rect.Width, rect.Y, color);

        //top
        rect = texturecoords[4];
        AddVertex(data, posX, posY + sizeY, posZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX, posY + sizeY, posZ + sizeZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ + sizeZ, rect.X + rect.Width, rect.Y, color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ, rect.X, rect.Y, color);

        //bottom
        rect = texturecoords[5];
        AddVertex(data, posX, posY, posZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX, posY, posZ + sizeZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX + sizeX, posY, posZ + sizeZ, rect.X + rect.Width, rect.Y, color);
        AddVertex(data, posX + sizeX, posY, posZ, rect.X, rect.Y, color);

        data.indices = new int[6 * 6];
        for (int i = 0; i < 6; i++)
        {
            data.indices[i * 6 + 0] = i * 4 + 3;
            data.indices[i * 6 + 1] = i * 4 + 2;
            data.indices[i * 6 + 2] = i * 4 + 0;
            data.indices[i * 6 + 3] = i * 4 + 2;
            data.indices[i * 6 + 4] = i * 4 + 1;
            data.indices[i * 6 + 5] = i * 4 + 0;
        }
        data.indicesCount = 36;



        game.platform.GlDisableCullFace();
        game.DrawModelData(data);
        game.platform.GlEnableCullFace();
    }
    public static void AddVertex(ModelData model, float x, float y, float z, float u, float v, int color)
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

    // correct
    public static void DrawCuboid2(Game game, float posX, float posY, float posZ,
        float sizeX, float sizeY, float sizeZ,
        RectangleFloat[] texturecoords, float light)
    {
        ModelData data = new ModelData();
        data.xyz = new float[4 * 6 * 3];
        data.uv = new float[4 * 6 * 2];
        data.rgba = new byte[4 * 6 * 4];
        int light255 = game.platform.FloatToInt(light * 255);
        int color = Game.ColorFromArgb(255, light255, light255, light255);

        RectangleFloat rect;

        //
        rect = texturecoords[2];
        AddVertex(data, posX, posY, posZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX, posY, posZ + sizeZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX, posY + sizeY, posZ + sizeZ, rect.X + rect.Width, rect.Y, color);
        AddVertex(data, posX, posY + sizeY, posZ, rect.X, rect.Y, color);

        //
        rect = texturecoords[3];
        AddVertex(data, posX + sizeX, posY, posZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX + sizeX, posY, posZ + sizeZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ + sizeZ, rect.X + rect.Width, rect.Y, color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ, rect.X, rect.Y, color);

        //
        rect = texturecoords[1];
        AddVertex(data, posX + sizeX, posY, posZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX, posY, posZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX, posY + sizeY, posZ, rect.X + rect.Width, rect.Y, color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ, rect.X, rect.Y, color);

        //
        rect = texturecoords[0];
        AddVertex(data, posX + sizeX, posY, posZ + sizeZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX, posY, posZ + sizeZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX, posY + sizeY, posZ + sizeZ, rect.X, rect.Y, color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ + sizeZ, rect.X + rect.Width, rect.Y, color);

        //top
        rect = texturecoords[4];
        AddVertex(data, posX, posY + sizeY, posZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX, posY + sizeY, posZ + sizeZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ + sizeZ, rect.X + rect.Width, rect.Y, color);
        AddVertex(data, posX + sizeX, posY + sizeY, posZ, rect.X, rect.Y, color);

        //bottom
        rect = texturecoords[5];
        AddVertex(data, posX, posY, posZ, rect.X, rect.Bottom(), color);
        AddVertex(data, posX, posY, posZ + sizeZ, rect.X + rect.Width, rect.Bottom(), color);
        AddVertex(data, posX + sizeX, posY, posZ + sizeZ, rect.X + rect.Width, rect.Y, color);
        AddVertex(data, posX + sizeX, posY, posZ, rect.X, rect.Y, color);

        data.indices = new int[6 * 6];
        for (int i = 0; i < 6; i++)
        {
            data.indices[i * 6 + 0] = i * 4 + 3;
            data.indices[i * 6 + 1] = i * 4 + 2;
            data.indices[i * 6 + 2] = i * 4 + 0;
            data.indices[i * 6 + 3] = i * 4 + 2;
            data.indices[i * 6 + 4] = i * 4 + 1;
            data.indices[i * 6 + 5] = i * 4 + 0;
        }
        data.indicesCount = 36;



        game.platform.GlDisableCullFace();
        game.DrawModelData(data);
        game.platform.GlEnableCullFace();
    }
}

public class ValueArray
{
    internal Value__[] items;
    internal int count;
}

public class RectangleFloat
{
    internal float X;
    internal float Y;
    internal float Width;
    internal float Height;

    public float Bottom()
    {
        return Y + Height;
    }

    public static RectangleFloat Create(float x_, float y_, float width_, float height_)
    {
        RectangleFloat r = new RectangleFloat();
        r.X = x_;
        r.Y = y_;
        r.Width = width_;
        r.Height = height_;
        return r;
    }
}

public class Value__
{
    internal int type_;
    internal float valueFloat;
    internal string valueString;
    internal RectangleFloat[] valueCoords;
    public const int TypeFloat = 0;
    public const int TypeString = 1;
    public const int TypeCoords = 2;
}

public class Variables
{
    public Variables()
    {
        keys = new string[128];
        items = new Value__[128];
    }
    internal string[] keys;
    internal Value__[] items;
    internal int itemsCount;
    public void SetFloat(string name, float value)
    {
        int id = GetIdOrCreate(name);
        items[id].type_ = Value__.TypeFloat;
        items[id].valueFloat = value;
    }

    int GetIdOrCreate(string name)
    {
        int id = GetKeyId(name);
        if (GetKeyId(name) == -1)
        {
            id = itemsCount;
            keys[id] = name;
            items[id] = new Value__();
            itemsCount++;
        }
        return id;
    }

    int GetKeyId(string name)
    {
        for (int i = 0; i < itemsCount; i++)
        {
            if (keys[i] == name)
            {
                return i;
            }
        }
        return -1;
    }

    internal bool ContainsKey(string name)
    {
        return GetKeyId(name) != -1;
    }

    internal float GetFloat(string name)
    {
        int id = GetKeyId(name);
        return items[id].valueFloat;
    }

    internal RectangleFloat[] GetCoords(string name)
    {
        int id = GetKeyId(name);
        return items[id].valueCoords;
    }

    internal void SetCoords(string name, RectangleFloat[] coords)
    {
        int id = GetIdOrCreate(name);
        items[id].type_ = Value__.TypeCoords;
        items[id].valueCoords = coords;
    }
}
