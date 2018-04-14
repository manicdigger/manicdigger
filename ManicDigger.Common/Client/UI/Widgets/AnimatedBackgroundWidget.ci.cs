public class AnimatedBackgroundWidget : AbstractMenuWidget
{
	string textureName;
	int textureId;
	float lastWidth;
	float lastHeight;
	int countX;
	int countY;
	RandomCi rnd;

	float xRot;
	bool xInv;
	float xSpeed;
	float yRot;
	bool yInv;
	float ySpeed;
	int overlap;
	int minspeed;


	public AnimatedBackgroundWidget()
	{
		textureName = null;
		textureId = -1;
		lastWidth = 0;
		lastHeight = 0;
		countX = 0;
		countY = 0;
		rnd = null;

		overlap = 200;
		minspeed = 20;

		xRot = 0;
		xInv = false;
		xSpeed = 0;

		yRot = 0;
		yInv = false;
		ySpeed = 0;
	}

	public void Init(string texName, float texWidth, float texHeight)
	{
		textureName = texName;
		sizex = texWidth;
		sizey = texHeight;
	}

	public override void Draw(float dt, UiRenderer renderer)
	{
		if (!visible) { return; }
		if (sizex <= 0 || sizey <= 0) { return; }
		if (textureName == null ||
			textureId == -1)
		{
			textureId = renderer.GetTexture(textureName);
		}

		GamePlatform p = renderer.GetPlatform();
		if (lastWidth != p.GetCanvasWidth() ||
			lastHeight != p.GetCanvasHeight())
		{
			countX = p.FloatToInt((p.GetCanvasWidth() + (2 * overlap)) / sizex) + 1;
			countY = p.FloatToInt((p.GetCanvasHeight() + (2 * overlap)) / sizey) + 1;
		}
		lastWidth = p.GetCanvasWidth();
		lastHeight = p.GetCanvasHeight();
		if (rnd == null)
		{
			rnd = p.RandomCreate();
			xSpeed = minspeed + rnd.MaxNext(5);
			ySpeed = minspeed + rnd.MaxNext(5);
		}

		Animate(dt);
		
		for (int x = 0; x < countX; x++)
		{
			for (int y = 0; y < countY; y++)
			{
				renderer.Draw2dTexture(textureId, x * sizex + xRot - overlap, y * sizey + yRot - overlap, sizex, sizey, null, 0, ColorCi.FromArgb(255, 255, 255, 255));
			}
		}
	}

	void Animate(float dt)
	{
		float maxDt = 1;
		if (dt > maxDt)
		{
			dt = maxDt;
		}
		if (xInv)
		{
			if (xRot <= -overlap)
			{
				xInv = false;
				xSpeed = minspeed + rnd.MaxNext(5);
			}
			xRot -= xSpeed * dt;
		}
		else
		{
			if (xRot >= overlap)
			{
				xInv = true;
				xSpeed = minspeed + rnd.MaxNext(5);
			}
			xRot += xSpeed * dt;
		}
		if (yInv)
		{
			if (yRot <= -overlap)
			{
				yInv = false;
				ySpeed = minspeed + rnd.MaxNext(5);
			}
			yRot -= ySpeed * dt;
		}
		else
		{
			if (yRot >= overlap)
			{
				yInv = true;
				ySpeed = minspeed + rnd.MaxNext(5);
			}
			yRot += ySpeed * dt;
		}
	}
}
