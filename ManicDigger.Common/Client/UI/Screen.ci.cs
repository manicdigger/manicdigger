/// <summary>
/// A Screen is a collection of widgets and offers basic handling for user interaction.
/// </summary>
public abstract class Screen
{
	public Screen()
	{
		WidgetCount = 0;
		WidgetMaxCount = 64;
		widgets = new AbstractMenuWidget[WidgetMaxCount];

		fontDefault = new FontCi();
	}

	int WidgetMaxCount;
	int WidgetCount;
	AbstractMenuWidget[] widgets;
	internal FontCi fontDefault;
	internal GamePlatform gamePlatform;
	internal UiRenderer uiRenderer;

	/// <summary>
	/// Render all widgets that are part of this screen.
	/// </summary>
	/// <param name="dt">milliseconds since last rendered frame</param>
	public virtual void Render(float dt) { }
	public virtual void OnKeyDown(KeyEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnKeyDown(gamePlatform, e);
		}
	}
	public virtual void OnKeyPress(KeyPressEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnKeyPress(gamePlatform, e);
		}
	}
	public virtual void OnKeyUp(KeyEventArgs e) { }
	public virtual void OnTouchStart(TouchEventArgs e) { }
	public virtual void OnTouchMove(TouchEventArgs e) { }
	public virtual void OnTouchEnd(TouchEventArgs e) { }
	public virtual void OnMouseDown(MouseEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseDown(gamePlatform, e);
			if (widgets[i].HasBeenClicked(e))
			{
				OnButton(widgets[i]);
			}
		}
	}
	public virtual void OnMouseUp(MouseEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseUp(gamePlatform, e);
		}
	}
	public virtual void OnMouseMove(MouseEventArgs e)
	{
		if (e.GetEmulated() && !e.GetForceUsage())
		{
			return;
		}
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseMove(gamePlatform, e);
		}
	}
	public virtual void OnBackPressed() { }
	public virtual void OnButton(AbstractMenuWidget w) { }
	public virtual void OnMouseWheel(MouseWheelEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseWheel(gamePlatform, e);
		}
	}
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
			widgets[i].Draw(dt, uiRenderer);
		}
	}
}
