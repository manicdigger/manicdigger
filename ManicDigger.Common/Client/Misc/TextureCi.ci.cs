public class TextureCi
{
	GamePlatform p;
	int textureid;

	public TextureCi()
	{
		p = null;
		textureid = -1;
	}

	/// <summary>
	/// Initialize the class. CiTo does not support parameters for constructors.
	/// </summary>
	public void Init(GamePlatform platform, BitmapCi bmp)
	{
		p = platform;
		textureid = p.LoadTextureFromBitmap(bmp);
	}

	/// <summary>
	/// Free allocated resources on the graphics card.
	/// </summary>
	public void Dispose()
	{
		if (textureid >= 0)
		{
			p.GLDeleteTexture(textureid);
		}
		p = null;
	}

	/// <summary>
	/// Begin using this texture.
	/// </summary>
	public void BeginUse()
	{
		if (textureid < 0) { return; }
		p.GlEnableTexture2d();
		p.BindTexture2d(textureid);
	}

	/// <summary>
	/// End using this texture.
	/// </summary>
	public void EndUse()
	{
		if (textureid < 0) { return; }
		p.BindTexture2d(0);
		p.GlDisableTexture2d();
	}
}
