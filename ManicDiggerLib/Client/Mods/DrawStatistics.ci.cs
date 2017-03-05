public class ModFpsHistoryGraph : ClientMod
{
	float one;
	int lasttitleupdateMilliseconds;
	int fpscount;
	float longestframedt;
	FontCi displayFont;

	float[] dtHistory;
	const int MaxCount = 300;
	int StatsLineCount;
	const int StatsMaxLineCount = 8;
	string[] StatsLines;
	ClientModManager m;

	bool ENABLE_STATS;
	bool DRAW_FPS_TEXT;
	bool DRAW_FPS_GRAPH;
	bool DRAW_POSITION;

	public ModFpsHistoryGraph()
	{
		one = 1;
		ENABLE_STATS = false;
		DRAW_FPS_TEXT = false;
		DRAW_FPS_GRAPH = false;
		DRAW_POSITION = false;
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
		PositionLines = new string[PositionLinesCount];
	}

	public override void Start(ClientModManager modmanager)
	{
		m = modmanager;
	}

	public override void OnNewFrame(Game game, NewFrameEventArgs args)
	{
		float dt = args.GetDt();
		UpdateFpsHistory(dt);
		UpdateStatisticsText(dt);
		DrawLocalPosition(game);
		Draw();
	}

	public override void OnKeyDown(Game game, KeyEventArgs args)
	{
		if (args.GetKeyCode() == GlKeys.F7)
		{
			// When hotkey is used, switch all stats on/off
			if (!ENABLE_STATS)
			{
				ENABLE_STATS = true;
				DRAW_FPS_TEXT = true;
				DRAW_FPS_GRAPH = true;
				DRAW_POSITION = true;
			}
			else
			{
				ENABLE_STATS = false;
				DRAW_FPS_TEXT = false;
				DRAW_FPS_GRAPH = false;
				DRAW_POSITION = false;
			}
		}
	}

	public override bool OnClientCommand(Game game, ClientCommandArgs args)
	{
		if (args.command == "fps")
		{
			IntRef argumentsLength = new IntRef();
			string[] arguments = m.GetPlatform().StringSplit(args.arguments, " ", argumentsLength);
			if (m.GetPlatform().StringTrim(args.arguments) == "")
			{
				DRAW_FPS_TEXT = true;
			}
			else if (arguments[0] == "1")
			{
				DRAW_FPS_TEXT = true;
				DRAW_FPS_GRAPH = false;
			}
			else if (arguments[0] == "2")
			{
				DRAW_FPS_TEXT = true;
				DRAW_FPS_GRAPH = true;
			}
			else
			{
				DRAW_FPS_TEXT = false;
				DRAW_FPS_GRAPH = false;
			}
			return true;
		}
		else if (args.command == "pos")
		{
			DRAW_POSITION = game.BoolCommandArgument(args.arguments);
			return true;
		}
		return false;
	}

	void UpdateFpsHistory(float dt)
	{
		for (int i = 0; i < MaxCount - 1; i++)
		{
			dtHistory[i] = dtHistory[i + 1];
		}
		dtHistory[MaxCount - 1] = dt;
	}

	void UpdateStatisticsText(float dt)
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

	void Draw()
	{
		if (DRAW_FPS_GRAPH || DRAW_FPS_TEXT)
		{
			m.OrthoMode();
			if (DRAW_FPS_GRAPH)
			{
				DrawFpsGraph();
			}
			if (DRAW_FPS_TEXT)
			{
				for (int i = 0; i < StatsLineCount; i++)
				{
					m.Draw2dText(StatsLines[i], 20 + 200 * (i / 4), 20 + 1.5f * (i % 4) * displayFont.size, displayFont);
				}
			}
			m.PerspectiveMode();
		}
	}

	Draw2dData[] todraw;
	void DrawFpsGraph()
	{
		// general size settings
		int historyheight = 80;
		int historyscale = 30;
		int posx = m.GetWindowWidth() - MaxCount - 20;
		int posy = historyheight + 20;

		// color settings
		int color_graph = Game.ColorFromArgb(128, 220, 20, 20);
		int color_outofrange = Game.ColorFromArgb(255, 255, 255, 0);
		int color_lines = Game.ColorFromArgb(255, 255, 255, 255);

		// assemble and draw the graph
		for (int i = 0; i < MaxCount; i++)
		{
			float time = dtHistory[i];
			time = (time * 60) * historyscale;

			todraw[i].x1 = posx + i;
			todraw[i].y1 = posy - time;
			todraw[i].width = 1;
			todraw[i].height = time;
			todraw[i].inAtlasId = null;
			todraw[i].color = color_graph;
			if (todraw[i].height > historyheight)
			{
				// value too big. clamp and mark
				todraw[i].y1 = posy - historyheight;
				todraw[i].height = historyheight;
				todraw[i].color = color_outofrange;
			}
		}
		m.Draw2dTextures(todraw, MaxCount, m.WhiteTexture());

		// draw legend
		m.Draw2dTexture(m.WhiteTexture(), posx, posy - historyscale, MaxCount, 1, null, color_lines);
		m.Draw2dTexture(m.WhiteTexture(), posx, posy - historyscale * (one * 60 / 30), MaxCount, 1, null, color_lines);
		m.Draw2dText("60", posx, posy - historyscale * (one * 60 / 60), displayFont);
		m.Draw2dText("30", posx, posy - historyscale * (one * 60 / 30), displayFont);
		m.Draw2dText("FPS", posx, posy - historyheight, displayFont);
	}

	string[] PositionLines;
	const int PositionLinesCount = 5;
	void DrawLocalPosition(Game game)
	{
		if (DRAW_POSITION)
		{
			GamePlatform p = m.GetPlatform();

			float heading = (((m.GetLocalOrientationY()) % (2 * Game.GetPi())) / (2 * Game.GetPi())) * 256;
			float pitch = (((m.GetLocalOrientationX() + Game.GetPi()) % (2 * Game.GetPi())) / (2 * Game.GetPi())) * 256;

			// Y and Z axis are swapped for display as Z axis depicts height
			PositionLines[0] = p.StringFormat("X: {0}", p.IntToString(game.MathFloor(m.GetLocalPositionX())));
			PositionLines[1] = p.StringFormat("Y: {0}", p.IntToString(game.MathFloor(m.GetLocalPositionZ())));
			PositionLines[2] = p.StringFormat("Z: {0}", p.IntToString(game.MathFloor(m.GetLocalPositionY())));
			PositionLines[3] = p.StringFormat("Heading: {0}", p.IntToString(Game.IntToByte(p.FloatToInt(heading))));
			PositionLines[4] = p.StringFormat("Pitch: {0}", p.IntToString(Game.IntToByte(p.FloatToInt(pitch))));

			m.OrthoMode();
			for (int i = 0; i < PositionLinesCount; i++)
			{
				m.Draw2dText(PositionLines[i], 20, m.GetWindowHeight() - 100 + 1.5f * i * displayFont.size, displayFont);
			}
			m.PerspectiveMode();
		}
	}
}
