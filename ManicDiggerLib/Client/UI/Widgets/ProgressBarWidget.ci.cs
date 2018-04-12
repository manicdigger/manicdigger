public class ProgressBarWidget : AbstractMenuWidget
{
	TextWidget _text;

	public ProgressBarWidget()
	{
		_text = new TextWidget();
	}

	public override void Draw(UiRenderer renderer)
	{
		if (!visible) { return; }
		if (sizex <= 0 || sizey <= 0) { return; }
	}
}
