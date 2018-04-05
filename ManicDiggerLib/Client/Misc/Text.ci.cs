public class Text_
{
	internal string text;
	internal int color;
	internal FontCi font;

	internal bool Equals_(Text_ t)
	{
		return this.text == t.text
			&& this.color == t.color
			&& this.font != null
			&& t.font != null
			&& this.font.size == t.font.size
			&& this.font.family == t.font.family
			&& this.font.style == t.font.style;
	}

	public string GetText() { return text; }
	public void SetText(string value) { text = value; }
	public int GetColor() { return color; }
	public void SetColor(int value) { color = value; }
	public FontCi GetFont() { return font; }
	public void SetFont(FontCi value) { font = value; }
	public float GetFontSize() { return font.size; }
	public string GetFontFamily() { return font.family; }
	public int GetFontStyle() { return font.style; }
}
