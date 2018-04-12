class KeyListenerWidget : AbstractMenuWidget
{
	int listenForKey;
	bool keyPressed;

	public KeyListenerWidget()
	{
		listenForKey = -1;
		keyPressed = false;
	}

	public override void Draw(float dt, UiRenderer renderer) { }

	public override void OnKeyPress(GamePlatform p, KeyPressEventArgs args)
	{
		if (listenForKey == args.GetKeyChar())
		{
			keyPressed = true;
		}
	}

	public void SetlistenChar(int listenChar)
	{
		listenForKey = listenChar;
	}
	public bool GetKeyPressed()
	{
		return keyPressed;
	}
	public void ResetKeyPressed()
	{
		keyPressed = false;
	}
}
