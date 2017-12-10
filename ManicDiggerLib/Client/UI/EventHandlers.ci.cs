public class MainMenuNewFrameHandler : NewFrameHandler
{
	public static MainMenuNewFrameHandler Create(MainMenu l)
	{
		MainMenuNewFrameHandler h = new MainMenuNewFrameHandler();
		h.l = l;
		return h;
	}
	MainMenu l;
	public override void OnNewFrame(NewFrameEventArgs args)
	{
		l.OnNewFrame(args);
	}
}

public class MainMenuKeyEventHandler : KeyEventHandler
{
	public static MainMenuKeyEventHandler Create(MainMenu l)
	{
		MainMenuKeyEventHandler h = new MainMenuKeyEventHandler();
		h.l = l;
		return h;
	}
	MainMenu l;
	public override void OnKeyDown(KeyEventArgs e)
	{
		l.HandleKeyDown(e);
	}
	public override void OnKeyUp(KeyEventArgs e)
	{
		l.HandleKeyUp(e);
	}

	public override void OnKeyPress(KeyPressEventArgs e)
	{
		l.HandleKeyPress(e);
	}
}

public class MainMenuMouseEventHandler : MouseEventHandler
{
	public static MainMenuMouseEventHandler Create(MainMenu l)
	{
		MainMenuMouseEventHandler h = new MainMenuMouseEventHandler();
		h.l = l;
		return h;
	}
	MainMenu l;

	public override void OnMouseDown(MouseEventArgs e)
	{
		l.HandleMouseDown(e);
	}

	public override void OnMouseUp(MouseEventArgs e)
	{
		l.HandleMouseUp(e);
	}

	public override void OnMouseMove(MouseEventArgs e)
	{
		l.HandleMouseMove(e);
	}

	public override void OnMouseWheel(MouseWheelEventArgs e)
	{
		l.HandleMouseWheel(e);
	}
}

public class MainMenuTouchEventHandler : TouchEventHandler
{
	public static MainMenuTouchEventHandler Create(MainMenu l)
	{
		MainMenuTouchEventHandler h = new MainMenuTouchEventHandler();
		h.l = l;
		return h;
	}
	MainMenu l;

	public override void OnTouchStart(TouchEventArgs e)
	{
		l.HandleTouchStart(e);
	}

	public override void OnTouchMove(TouchEventArgs e)
	{
		l.HandleTouchMove(e);
	}

	public override void OnTouchEnd(TouchEventArgs e)
	{
		l.HandleTouchEnd(e);
	}
}
