public class GameScreen : ClientMod
{
	public GameScreen()
	{
		Initialize(64);

		fontDefault = new FontCi();
	}

	internal Game game;
	int WidgetCount;
	int WidgetMaxCount;
	AbstractMenuWidget[] widgets;
	internal FontCi fontDefault;

	public override void OnKeyPress(Game game_, KeyPressEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnKeyPress(game_.platform, e);

			// send dialog response if necessary
			if (widgets[i].GetEventKeyPressed())
			{
				game_.SendPacketClient(CreateDialogResponse(widgets[i].GetEventName()));
			}
		}
	}
	public override void OnKeyDown(Game game_, KeyEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnKeyDown(game_.platform, e);
		}
	}
	public override void OnTouchStart(Game game_, TouchEventArgs e) { }
	public override void OnTouchEnd(Game game_, TouchEventArgs e) { }
	public override void OnMouseDown(Game game_, MouseEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseDown(game_.platform, e);
			if (widgets[i].HasBeenClicked(e))
			{
				OnButton(widgets[i]);
				// send dialog response if necessary
				if (!game_.platform.StringEmpty(widgets[i].GetEventName()))
				{
					game_.SendPacketClient(CreateDialogResponse(widgets[i].GetEventName()));
				}
			}
		}
	}
	public override void OnMouseUp(Game game_, MouseEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseUp(game_.platform, e);
		}
	}
	public override void OnMouseWheelChanged(Game game_, MouseWheelEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseWheel(game_.platform, e);
		}
	}
	public override void OnMouseMove(Game game_, MouseEventArgs e)
	{
		if (e.GetEmulated() && !e.GetForceUsage())
		{
			return;
		}
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseMove(game_.platform, e);
		}
	}
	public virtual void OnButton(AbstractMenuWidget w) { }

	public void AddWidget(AbstractMenuWidget widget)
	{
		if (WidgetCount >= WidgetMaxCount) { return; }
		widgets[WidgetCount] = widget;
		WidgetCount++;
	}
	public void DrawWidgets(float dt)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].Draw(dt, game.uiRenderer);
		}
	}
	public void Initialize(int maxWidgetCount)
	{
		WidgetCount = 0;
		WidgetMaxCount = maxWidgetCount;
		widgets = new AbstractMenuWidget[WidgetMaxCount];
	}
	public Packet_Client CreateDialogResponse(string widgetId)
	{
		string[] textValues = new string[WidgetCount];
		for (int j = 0; j < WidgetCount; j++)
		{
			string s = widgets[j].GetEventResponse();
			if (s == null)
			{
				s = "";
			}
			textValues[j] = s;
		}
		return ClientPackets.DialogClick(widgetId, textValues, WidgetCount);
	}
}
