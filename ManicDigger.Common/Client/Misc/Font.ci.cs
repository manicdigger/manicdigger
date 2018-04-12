public class FontCi
{
	internal string family;
	internal float size;
	/// <summary>
	/// The font style to use. Can be one of the following:<br/>
	/// 0: Regular<br/>
	/// 1: Bold<br/>
	/// 2: Italic<br/>
	/// 3: Bold Italic<br/>
	/// 4: Underline<br/>
	/// 5: Bold Underline<br/>
	/// 6: Italic Underline<br/>
	/// 7: Bold Italic Underline<br/>
	/// 8: Strikethrough<br/>
	/// </summary>
	internal int style;

	public FontCi()
	{
		// Default font style
		family = "Arial";
		size = 12;
		style = 0;
	}

	internal static FontCi Create(string family_, float size_, int style_)
	{
		FontCi f = new FontCi();
		f.family = family_;
		f.size = size_;
		f.style = style_;
		return f;
	}

	public float GetFontSize() { return size; }
	public string GetFontFamily() { return family; }
	public int GetFontStyle() { return style; }
}
