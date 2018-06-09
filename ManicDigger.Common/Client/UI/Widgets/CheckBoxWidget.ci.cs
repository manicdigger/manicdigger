public class CheckBoxWidget : AbstractMenuWidget
{
	TextWidget _textDescription;
	TextWidget _textState;
	bool _stateChecked;
	string _textureNameUnchecked;
	string _textureNameChecked;

	public CheckBoxWidget()
	{
		_stateChecked = false;
		_textureNameUnchecked = "button.png";
		_textureNameChecked = "button_sel.png";
		clickable = true;
		focusable = true;

		FontCi fontDefault = new FontCi();
		_textState = new TextWidget();
		_textState.SetFont(fontDefault);
		_textState.SetAlignment(TextAlign.Center);
		_textState.SetBaseline(TextBaseline.Middle);
		_textDescription = new TextWidget();
		_textDescription.SetFont(fontDefault);
		_textDescription.SetBaseline(TextBaseline.Middle);
	}

	public override void Draw(float dt, UiRenderer renderer)
	{
		if (!visible) { return; }
		if (sizex <= 0 || sizey <= 0) { return; }

		_textState.x = x + sizey / 2;
		_textState.y = y + sizey / 2;
		_textDescription.x = x + sizey + 6;
		_textDescription.y = y + sizey / 2;

		// TODO: use atlas texture
		renderer.Draw2dTexture(renderer.GetTexture(_stateChecked || hasKeyboardFocus ? _textureNameChecked : _textureNameUnchecked), x, y, sizey, sizey, null, 0, color);
		_textState.Draw(dt, renderer);
		_textDescription.Draw(dt, renderer);
	}
	public override void OnMouseDown(GamePlatform p, MouseEventArgs args)
	{
		if (!HasBeenClicked(args)) { return; }
		_stateChecked = !_stateChecked;
		UpdateCheckboxText();
	}

	public bool GetChecked()
	{
		return _stateChecked;
	}
	public void SetChecked(bool value)
	{
		_stateChecked = value;
		UpdateCheckboxText();
	}
	public void SetDescription(string text)
	{
		_textDescription.SetText(text);
	}

	void UpdateCheckboxText()
	{
		_textState.SetText(_stateChecked ? "X" : null);
	}
}
