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
        StatsLines = new string[StatsMaxLineCount];
        displayFont = new FontCi();
        displayFont.size = 10;
    }

    float one;
    int lasttitleupdateMilliseconds;
    int fpscount;
    float longestframedt;
    FontCi displayFont;

    public override void Start(ClientModManager modmanager)
    {
        m = modmanager;
    }
    float[] dtHistory;
    const int MaxCount = 300;
    int StatsLineCount;
    const int StatsMaxLineCount = 8;
    string[] StatsLines;
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

            StatsLineCount = 0;
            for (int i = 0; i < m.GetPerformanceInfo().size; i++)
            {
                if (m.GetPerformanceInfo().items[i] == null)
                {
                    continue;
                }
                if (StatsLineCount >= StatsMaxLineCount)
                {
                    // Prevent running out of bounds
                    break;
                }
                StatsLines[StatsLineCount++] = m.GetPerformanceInfo().items[i].value;
            }
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
                for (int i = 0; i < StatsLineCount; i++)
                {
                    m.Draw2dText(StatsLines[i], 20 + 200 * (i/4), 20 + 1.5f * (i%4) * displayFont.size, displayFont);
                }
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
        m.Draw2dText("60", posx, posy - historyheight * (one * 60 / 60), displayFont);
        m.Draw2dText("75", posx, posy - historyheight * (one * 60 / 75), displayFont);
        m.Draw2dText("30", posx, posy - historyheight * (one * 60 / 30), displayFont);
        m.Draw2dText("150", posx, posy - historyheight * (one * 60 / 150), displayFont);
    }
}
