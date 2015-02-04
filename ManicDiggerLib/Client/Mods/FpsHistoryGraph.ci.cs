public class ModFpsHistoryGraph : ClientMod
{
    public ModFpsHistoryGraph()
    {
        one = 1;
        drawfpstext = false;
        drawfpsgraph = false;
        dtHistory = new float[MaxCount];
        for (int i = 0; i < MaxCount; i++)
        {
            dtHistory[i] = 0;
        }
        todraw = new Draw2dData[MaxCount];
        for (int i = 0; i < MaxCount; i++)
        {
            todraw[i] = new Draw2dData();
        }
    }

    float one;
    int lasttitleupdateMilliseconds;
    int fpscount;
    string fpstext;
    float longestframedt;

    public override void Start(ClientModManager modmanager)
    {
        m = modmanager;
    }
    float[] dtHistory;
    const int MaxCount = 300;
    ClientModManager m;

    public override void OnNewFrame(Game game, NewFrameEventArgs args)
    {
        float dt = args.GetDt();
        UpdateGraph(dt);
        UpdateTitleFps(dt);
        Draw();
    }

    void UpdateTitleFps(float dt)
    {
        GamePlatform p = m.GetPlatform();
        fpscount++;
        longestframedt = MathCi.MaxFloat(longestframedt, dt);
        float elapsed = one * (p.TimeMillisecondsFromStart() - lasttitleupdateMilliseconds) / 1000;
        if (elapsed >= 1)
        {
            string fpstext1 = "";
            lasttitleupdateMilliseconds = p.TimeMillisecondsFromStart();
            fpstext1 = StringTools.StringAppend(p, fpstext1, p.StringFormat("FPS: {0}", p.IntToString(p.FloatToInt((one * fpscount) / elapsed))));
            fpstext1 = StringTools.StringAppend(p, fpstext1, p.StringFormat(" (min: {0})", p.IntToString(p.FloatToInt(one / longestframedt))));
            longestframedt = 0;
            fpscount = 0;
            m.GetPerformanceInfo().Set("fps", fpstext1);

            string s = "";
            string[] l = new string[64];
            int lCount = 0;
            for (int i = 0; i < m.GetPerformanceInfo().count; i++)
            {
                if (m.GetPerformanceInfo().items[i] == null)
                {
                    continue;
                }
                l[lCount++] = m.GetPerformanceInfo().items[i].value;
            }

            int perline = 2;
            for (int i = 0; i < lCount; i++)
            {
                s = StringTools.StringAppend(p, s, l[i]);
                if ((i % perline == 0) && (i != lCount - 1))
                {
                    s = StringTools.StringAppend(p, s, ", ");
                }
                if (i % perline != 0)
                {
                    s = StringTools.StringAppend(p, s, "\n");
                }
            }
            fpstext = s;
        }
    }

    public override void OnKeyDown(Game game, KeyEventArgs args)
    {
        if (args.GetKeyCode() == GlKeys.F7)
        {
            if (!drawfpsgraph)
            {
                drawfpstext = true;
                drawfpsgraph = true;
            }
            else
            {
                drawfpstext = false;
                drawfpsgraph = false;
            }
        }
    }

    bool drawfpstext;
    bool drawfpsgraph;

    public override bool OnClientCommand(Game game, ClientCommandArgs args)
    {
        if (args.command == "fps")
        {
            IntRef argumentsLength = new IntRef();
            string[] arguments = m.GetPlatform().StringSplit(args.arguments, " ", argumentsLength);
            if (m.GetPlatform().StringTrim(args.arguments) == "")
            {
                drawfpstext = true;
            }
            else if (arguments[0] == "1")
            {
                drawfpstext = true;
                drawfpsgraph = false;
            }
            else if (arguments[0] == "2")
            {
                drawfpstext = true;
                drawfpsgraph = true;
            }
            else
            {
                drawfpstext = false;
                drawfpsgraph = false;
            }
            return true;
        }
        return false;
    }

    void UpdateGraph(float dt)
    {
        for (int i = 0; i < MaxCount - 1; i++)
        {
            dtHistory[i] = dtHistory[i + 1];
        }
        dtHistory[MaxCount - 1] = dt;
    }

    void Draw()
    {
        if (drawfpsgraph || drawfpstext)
        {
            m.OrthoMode();
            if (drawfpsgraph)
            {
                DrawGraph();
            }
            if (drawfpstext)
            {
                m.Draw2dText(fpstext, 20, 20, ChatFontSize);
            }
            m.PerspectiveMode();
        }
    }

    Draw2dData[] todraw;
    void DrawGraph()
    {
        float maxtime = 0;
        for (int i = 0; i < MaxCount; i++)
        {
            float v = dtHistory[i];
            if (v > maxtime)
            {
                maxtime = v;
            }
        }
        int historyheight = 80;
        int posx = 25;
        int posy = m.GetWindowHeight() - historyheight - 20;
        int[] colors = new int[2];
        colors[0] = Game.ColorFromArgb(255, 0, 0, 0);
        colors[1] = Game.ColorFromArgb(255, 255, 0, 0);
        int linecolor = Game.ColorFromArgb(255, 255, 255, 255);

        for (int i = 0; i < MaxCount; i++)
        {
            float time = dtHistory[i];
            time = (time * 60) * historyheight;
            int c = InterpolationCi.InterpolateColor(m.GetPlatform(), (one * i) / MaxCount, colors, 2);
            todraw[i].x1 = posx + i;
            todraw[i].y1 = posy - time;
            todraw[i].width = 1;
            todraw[i].height = time;
            todraw[i].inAtlasId = null;
            todraw[i].color = c;
        }
        m.Draw2dTextures(todraw, MaxCount, m.WhiteTexture());

        m.Draw2dTexture(m.WhiteTexture(), posx, posy - historyheight, MaxCount, 1, null, linecolor);
        m.Draw2dTexture(m.WhiteTexture(), posx, posy - historyheight * (one * 60 / 75), MaxCount, 1, null, linecolor);
        m.Draw2dTexture(m.WhiteTexture(), posx, posy - historyheight * (one * 60 / 30), MaxCount, 1, null, linecolor);
        m.Draw2dTexture(m.WhiteTexture(), posx, posy - historyheight * (one * 60 / 150), MaxCount, 1, null, linecolor);
        m.Draw2dText("60", posx, posy - historyheight * (one * 60 / 60), 6);
        m.Draw2dText("75", posx, posy - historyheight * (one * 60 / 75), 6);
        m.Draw2dText("30", posx, posy - historyheight * (one * 60 / 30), 6);
        m.Draw2dText("150", posx, posy - historyheight * (one * 60 / 150), 6);
    }

    const int ChatFontSize = 11;
}
