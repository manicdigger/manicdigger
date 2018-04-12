public class Screen
{
	public Screen()
	{
		WidgetCount = 0;
		WidgetMaxCount = 64;
		widgets = new AbstractMenuWidget[WidgetMaxCount];

		fontTitle = new FontCi();
		fontTitle.size = 20;
		fontTitle.style = 1;
		fontDefault = new FontCi();
		fontMessage = new FontCi();
		fontMessage.style = 3;
	}

	internal MainMenu menu;
	internal FontCi fontTitle;
	internal FontCi fontDefault;
	internal FontCi fontMessage;
	int WidgetMaxCount;
	internal int WidgetCount;
	AbstractMenuWidget[] widgets;
	internal UiRenderer uiRenderer;

	public virtual void Render(float dt) { }
	public virtual void OnKeyDown(KeyEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnKeyDown(menu.p, e);
		}
	}
	public virtual void OnKeyPress(KeyPressEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnKeyPress(menu.p, e);
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
			widgets[i].OnMouseDown(menu.p, e);
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
			widgets[i].OnMouseUp(menu.p, e);
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
			widgets[i].OnMouseMove(menu.p, e);
		}
	}
	public virtual void OnBackPressed() { }
	public virtual void LoadTranslations() { }
	public virtual void OnButton(AbstractMenuWidget w) { }
	public virtual void OnMouseWheel(MouseWheelEventArgs e)
	{
		for (int i = 0; i < WidgetCount; i++)
		{
			widgets[i].OnMouseWheel(menu.p, e);
		}
	}
	public void AddWidgetNew(AbstractMenuWidget widget)
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
