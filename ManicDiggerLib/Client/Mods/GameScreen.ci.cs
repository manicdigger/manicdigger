public class GameScreen : ClientMod
{
	public GameScreen()
	{
		WidgetCount = 64;
		widgets = new AbstractMenuWidget[WidgetCount];
	}

	internal Game game;
	internal int WidgetCount;
	internal AbstractMenuWidget[] widgets;

	public override void OnKeyPress(Game game_, KeyPressEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnKeyPress(game.platform, e);
		}
	}
	public override void OnKeyDown(Game game_, KeyEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnKeyDown(game.platform, e);
		}
	}
	public override void OnTouchStart(Game game_, TouchEventArgs e) { }
	public override void OnTouchEnd(Game game_, TouchEventArgs e) { }
	public override void OnMouseDown(Game game_, MouseEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseDown(game.platform, e);
			if (widgets[i].HasBeenClicked(e))
			{
				OnButton(widgets[i]);
			}
		}
	}
	public override void OnMouseUp(Game game_, MouseEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseUp(game.platform, e);
		}
	}
	public virtual void OnMouseWheel(MouseWheelEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseWheel(game.platform, e);
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
			widgets[i].OnMouseMove(game.platform, e);
		}
	}
	public virtual void OnBackPressed() { }
	public virtual void OnButton(AbstractMenuWidget w) { }

	public void DrawWidgets(float dt)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].Draw(dt, game.uiRenderer);
		}
	}
}
