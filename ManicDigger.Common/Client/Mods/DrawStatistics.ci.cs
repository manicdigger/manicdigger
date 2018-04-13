public class ModFpsHistoryGraph : ClientMod
{
	float one;
	int lasttitleupdateMilliseconds;
	int fpscount;
	float longestframedt;
	FontCi displayFont;
	FontCi displayFontHeadings;

	const int MaxCount = 300;
	float[] dtHistory;
	float[] chunkUpdateHistory;
	float[] pingHistory;

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
		chunkUpdateHistory = new float[MaxCount];
		for (int i = 0; i < MaxCount; i++)
		{
			chunkUpdateHistory[i] = 0;
		}
		pingHistory = new float[MaxCount];
		for (int i = 0; i < MaxCount; i++)
		{
			pingHistory[i] = 0;
		}
		todraw = new Draw2dData[MaxCount];
		for (int i = 0; i < MaxCount; i++)
		{
			todraw[i] = new Draw2dData();
		}
		StatsLines = new string[StatsMaxLineCount];
		displayFont = new FontCi();
		displayFont.size = 10;
		displayFontHeadings = new FontCi();
		displayFontHeadings.size = 10;
		displayFontHeadings.style = 1;
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

	const int HISTORY_FPS_SCALE = 30;
	void UpdateFpsHistory(float dt)
	{
		for (int i = 0; i < MaxCount - 1; i++)
		{
			dtHistory[i] = dtHistory[i + 1];
		}
		dtHistory[MaxCount - 1] = dt * 60 * HISTORY_FPS_SCALE;
	}

	void UpdateChunkHistory(int updates)
	{
		for (int i = 0; i < MaxCount - 1; i++)
		{
			chunkUpdateHistory[i] = chunkUpdateHistory[i + 1];
		}
		chunkUpdateHistory[MaxCount - 1] = updates;
	}

	void UpdatePingHistory(int ping)
	{
		for (int i = 0; i < MaxCount - 1; i++)
		{
			pingHistory[i] = pingHistory[i + 1];
		}
		pingHistory[MaxCount - 1] = ping;
	}

	void UpdateStatisticsText(float dt)
	{
		GamePlatform p = m.GetPlatform();
		fpscount++;
		longestframedt = MathCi.MaxFloat(longestframedt, dt);
		float elapsed = one * (p.TimeMillisecondsFromStart() - lasttitleupdateMilliseconds) / 1000;
		if (elapsed >= 1)
		{
			lasttitleupdateMilliseconds = p.TimeMillisecondsFromStart();
			string fpstext1 = p.IntToString(p.FloatToInt((one * fpscount) / elapsed));
			fpstext1 = StringTools.StringAppend(p, fpstext1, p.StringFormat(" (min: {0})", p.IntToString(p.FloatToInt(one / longestframedt))));
			longestframedt = 0;
			fpscount = 0;
			m.GetPerformanceInfo().Set("FPS", fpstext1);

			StatsLineCount = 0;
			for (int i = 0; i < m.GetPerformanceInfo().size; i++)
			{
				if (m.GetPerformanceInfo().items[i] == null)
				{
					continue;
				}
				if (m.GetPerformanceInfo().items[i].key == "Chunk updates")
				{
					UpdateChunkHistory(p.IntParse(m.GetPerformanceInfo().items[i].value));
				}
				if (m.GetPerformanceInfo().items[i].key == "Ping")
				{
					UpdatePingHistory(p.IntParse(m.GetPerformanceInfo().items[i].value));
				}
				if (StatsLineCount >= StatsMaxLineCount)
				{
					// Prevent running out of bounds
					break;
				}
				StatsLines[StatsLineCount++] = p.StringFormat2("{0}: {1}", m.GetPerformanceInfo().items[i].key, m.GetPerformanceInfo().items[i].value);
			}
		}
	}

	void Draw()
	{
		if (ENABLE_STATS)
		{
			// switch to orthographic mode
			m.OrthoMode();

			if (DRAW_FPS_GRAPH || DRAW_FPS_TEXT)
			{
				if (DRAW_FPS_GRAPH)
				{
					DrawFpsGraph(m.GetWindowWidth() - MaxCount - 20, 20);
				}
				if (DRAW_FPS_TEXT)
				{
					for (int i = 0; i < StatsLineCount; i++)
					{
						m.Draw2dText(StatsLines[i], 20 + 200 * (i / 4), 20 + 1.5f * (i % 4) * displayFont.size, displayFont);
					}
				}

			}

			// draw additional graphs
			DrawChunkGraph(m.GetWindowWidth() - MaxCount - 20, 120);
			DrawPingGraph(m.GetWindowWidth() - MaxCount - 20, 220);

			// switch back to perspective mode
			m.PerspectiveMode();
		}
	}

	void DrawFpsGraph(int posx, int posy)
	{
		// general size settings
		const int historyheight = 80;

		// color settings
		int color_graph = ColorCi.FromArgb(128, 220, 20, 20);
		int color_outofrange = ColorCi.FromArgb(128, 255, 255, 0);
		int color_lines = ColorCi.FromArgb(255, 255, 255, 255);

		posy += historyheight;

		// draw graph
		DrawGraph(posx, posy, MaxCount, historyheight, dtHistory, color_graph, color_outofrange);

		// draw legend
		m.Draw2dTexture(m.WhiteTexture(), posx, posy - HISTORY_FPS_SCALE, MaxCount, 1, null, color_lines);
		m.Draw2dTexture(m.WhiteTexture(), posx, posy - HISTORY_FPS_SCALE * (one * 60 / 30), MaxCount, 1, null, color_lines);
		m.Draw2dText("60", posx, posy - HISTORY_FPS_SCALE * (one * 60 / 60), displayFont);
		m.Draw2dText("30", posx, posy - HISTORY_FPS_SCALE * (one * 60 / 30), displayFont);
		m.Draw2dText("FPS", posx, posy - historyheight, displayFontHeadings);
	}

	void DrawChunkGraph(int posx, int posy)
	{
		// general size settings
		const int historyheight = 80;

		// color settings
		int color_graph = ColorCi.FromArgb(128, 20, 20, 220);
		int color_outofrange = ColorCi.FromArgb(128, 60, 60, 220);
		int color_lines = ColorCi.FromArgb(255, 255, 255, 255);

		posy += historyheight;

		// draw graph
		DrawGraph(posx, posy, MaxCount, historyheight, chunkUpdateHistory, color_graph, color_outofrange);

		// draw legend
		m.Draw2dTexture(m.WhiteTexture(), posx, posy - 60, MaxCount, 1, null, color_lines);
		m.Draw2dTexture(m.WhiteTexture(), posx, posy - 30, MaxCount, 1, null, color_lines);
		m.Draw2dText("60", posx, posy - 60, displayFont);
		m.Draw2dText("30", posx, posy - 30, displayFont);
		m.Draw2dText("Chunk updates", posx, posy - historyheight, displayFontHeadings);
	}

	void DrawPingGraph(int posx, int posy)
	{
		// general size settings
		const int historyheight = 80;

		// color settings
		int color_graph = ColorCi.FromArgb(128, 20, 220, 20);
		int color_outofrange = ColorCi.FromArgb(128, 120, 220, 20);
		int color_lines = ColorCi.FromArgb(255, 255, 255, 255);

		posy += historyheight;

		// draw graph
		DrawGraph(posx, posy, MaxCount, historyheight, pingHistory, color_graph, color_outofrange);

		// draw legend
		m.Draw2dTexture(m.WhiteTexture(), posx, posy - 40, MaxCount, 1, null, color_lines);
		m.Draw2dTexture(m.WhiteTexture(), posx, posy - 20, MaxCount, 1, null, color_lines);
		m.Draw2dText("40 ms", posx, posy - 40, displayFont);
		m.Draw2dText("20 ms", posx, posy - 20, displayFont);
		m.Draw2dText("Server Ping", posx, posy - historyheight, displayFontHeadings);
	}

	Draw2dData[] todraw;
	/// <summary>
	/// Draw a graph into the scene using the given parameters.
	/// </summary>
	/// <param name="posX">Bottom left X coordinate.</param>
	/// <param name="posY">Bottom left Y coordinate.</param>
	/// <param name="sizeX">Size of the graph in X direction. Must be lower than or equal to data array size!</param>
	/// <param name="sizeY">Size of the graph in Y direction. Any value higher than this will be clamped and treated as "outlier".</param>
	/// <param name="data">Data source for the graph.</param>
	/// <param name="color_graph">Color used for drawing the graph.</param>
	/// <param name="color_outlier">Color used to highlight outliers.</param>
	void DrawGraph(float posX, float posY, int sizeX, int sizeY, float[] data, int color_graph, int color_outlier)
	{
		int color_background = ColorCi.FromArgb(80, 0, 0, 0);

		// draw background
		const int margin = 4;
		m.Draw2dTexture(m.WhiteTexture(), posX - margin, posY + margin, sizeX + 2 * margin, -sizeY - 2 * margin, null, color_background);

		// assemble the graph
		for (int i = 0; i < sizeX; i++)
		{
			todraw[i].x1 = posX + i;
			todraw[i].y1 = posY - data[i];
			todraw[i].width = 1;
			todraw[i].height = data[i];
			todraw[i].inAtlasId = null;
			todraw[i].color = color_graph;
			if (data[i] > sizeY)
			{
				// value too big. clamp and mark
				todraw[i].y1 = posY - sizeY;
				todraw[i].height = sizeY;
				todraw[i].color = color_outlier;
			}
		}
		// draw the result
		m.Draw2dTextures(todraw, sizeX, m.WhiteTexture());
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
			PositionLines[3] = p.StringFormat("Heading: {0}", p.IntToString(ConvertCi.IntToByte(p.FloatToInt(heading))));
			PositionLines[4] = p.StringFormat("Pitch: {0}", p.IntToString(ConvertCi.IntToByte(p.FloatToInt(pitch))));

			m.OrthoMode();
			for (int i = 0; i < PositionLinesCount; i++)
			{
				m.Draw2dText(PositionLines[i], 20, m.GetWindowHeight() - 100 + 1.5f * i * displayFont.size, displayFont);
			}
			m.PerspectiveMode();
		}
	}
}
