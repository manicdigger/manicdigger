class CheckBoxWidget : AbstractMenuWidget
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

		FontCi fontDefault = new FontCi();
		_textState = new TextWidget();
		_textState.SetFont(fontDefault);
		_textState.SetAlignment(TextAlign.Center);
		_textState.SetBaseline(TextBaseline.Middle);
		_textDescription = new TextWidget();
		_textDescription.SetFont(fontDefault);
		_textDescription.SetBaseline(TextBaseline.Middle);
	}

	public override void Draw(MainMenu m)
	{
		_textState.x = x + sizey / 2;
		_textState.y = y + sizey / 2;
		_textDescription.x = x + sizey + 6;
		_textDescription.y = y + sizey / 2;

		m.Draw2dQuad(m.GetTexture(_stateChecked ? _textureNameChecked : _textureNameUnchecked), x, y, sizey, sizey);
		_textState.Draw(m);
		_textDescription.Draw(m);
	}
	public override void OnMouseDown(GamePlatform p, MouseEventArgs args)
	{
		if (!HasBeenClicked(args)) { return; }
		_stateChecked = !_stateChecked;
		UpdateCheckboxText();
	}

	public bool IsChecked()
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
