public class ImageWidget : AbstractMenuWidget
{
	string _textureName;

	public ImageWidget()
	{
		x = 0;
		y = 0;
		sizex = 0;
		sizey = 0;
	}
	//public ImageWidget(float dx, float dy, float dw, float dh, string texture)
	//{
	//	x = dx;
	//	y = dy;
	//	sizex = dw;
	//	sizey = dh;
	//	_textureName = texture;
	//}

	public override void Draw(MainMenu m)
	{
		if (!visible) { return; }
		if (_textureName == null || _textureName == "") { return; }
		m.Draw2dQuad(m.GetTexture(_textureName), x, y, sizex, sizey);
	}

	public void SetTextureName(string name)
	{
		_textureName = name;
	}
}
