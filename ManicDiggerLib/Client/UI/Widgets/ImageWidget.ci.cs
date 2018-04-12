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

	public override void Draw(float dt, UiRenderer renderer)
	{
		if (!visible) { return; }
		if (sizex == 0 || sizey == 0) { return; }
		if (_textureName == null || _textureName == "") { return; }
		if (_textureName == "Solid")
		{
			renderer.Draw2dTexture(renderer.GetWhiteTextureId(), x, y, sizex, sizey, null, 0, color);
		}
		else
		{
			renderer.Draw2dTexture(renderer.GetTexture(_textureName), x, y, sizex, sizey, null, 0, color);
		}
	}

	public void SetTextureName(string name)
	{
		_textureName = name;
	}
}
