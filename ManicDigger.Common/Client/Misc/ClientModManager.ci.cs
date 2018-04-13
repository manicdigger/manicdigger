public abstract class ClientModManager
{
	public abstract void MakeScreenshot();
	public abstract void SetLocalPosition(float glx, float gly, float glz);
	public abstract float GetLocalPositionX();
	public abstract float GetLocalPositionY();
	public abstract float GetLocalPositionZ();
	public abstract void SetLocalOrientation(float glx, float gly, float glz);
	public abstract float GetLocalOrientationX();
	public abstract float GetLocalOrientationY();
	public abstract float GetLocalOrientationZ();
	public abstract void DisplayNotification(string message);
	public abstract void SendChatMessage(string message);
	public abstract GamePlatform GetPlatform();
	public abstract void ShowGui(int level);
	public abstract void SetFreemove(int level);
	public abstract int GetFreemove();
	public abstract BitmapCi GrabScreenshot();
	public abstract AviWriterCi AviWriterCreate();
	public abstract int GetWindowWidth();
	public abstract int GetWindowHeight();
	public abstract bool IsFreemoveAllowed();
	public abstract void EnableCameraControl(bool enable);
	public abstract int WhiteTexture();
	public abstract void Draw2dTexture(int textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int color);
	public abstract void Draw2dTextures(Draw2dData[] todraw, int todrawLength, int textureId);
	public abstract void Draw2dText(string text, float x, float y, FontCi font);
	public abstract void OrthoMode();
	public abstract void PerspectiveMode();
	public abstract DictionaryStringString GetPerformanceInfo();
}

public class ClientModManager1 : ClientModManager
{
	internal Game game;

	public override void MakeScreenshot()
	{
		game.platform.SaveScreenshot();
	}

	public override void SetLocalPosition(float glx, float gly, float glz)
	{
		game.player.position.x = glx;
		game.player.position.y = gly;
		game.player.position.z = glz;
	}

	public override float GetLocalPositionX()
	{
		return game.player.position.x;
	}

	public override float GetLocalPositionY()
	{
		return game.player.position.y;
	}

	public override float GetLocalPositionZ()
	{
		return game.player.position.z;
	}

	public override void SetLocalOrientation(float glx, float gly, float glz)
	{
		game.player.position.rotx = glx;
		game.player.position.roty = gly;
		game.player.position.rotz = glz;
	}

	public override float GetLocalOrientationX()
	{
		return game.player.position.rotx;
	}

	public override float GetLocalOrientationY()
	{
		return game.player.position.roty;
	}

	public override float GetLocalOrientationZ()
	{
		return game.player.position.rotz;
	}

	public override void DisplayNotification(string message)
	{
		game.AddChatline(message);
	}

	public override void SendChatMessage(string message)
	{
		game.SendChat(message);
	}

	public override GamePlatform GetPlatform()
	{
		return game.platform;
	}

	public override void ShowGui(int level)
	{
		if (level == 0)
		{
			game.ENABLE_DRAW2D = false;
		}
		else
		{
			game.ENABLE_DRAW2D = true;
		}
	}

	public override void SetFreemove(int level)
	{
		game.controls.SetFreemove(level);
	}

	public override int GetFreemove()
	{
		return game.controls.GetFreemove();
	}

	public override BitmapCi GrabScreenshot()
	{
		return game.platform.GrabScreenshot();
	}

	public override AviWriterCi AviWriterCreate()
	{
		return game.platform.AviWriterCreate();
	}

	public override int GetWindowWidth()
	{
		return game.platform.GetCanvasWidth();
	}

	public override int GetWindowHeight()
	{
		return game.platform.GetCanvasHeight();
	}

	public override bool IsFreemoveAllowed()
	{
		return game.AllowFreemove;
	}

	public override void EnableCameraControl(bool enable)
	{
		game.enableCameraControl = enable;
	}

	public override int WhiteTexture()
	{
		return game.WhiteTexture();
	}

	public override void Draw2dTexture(int textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int color)
	{
		int a = ColorCi.ExtractA(color);
		int r = ColorCi.ExtractR(color);
		int g = ColorCi.ExtractG(color);
		int b = ColorCi.ExtractB(color);
		game.Draw2dTexture(textureid, game.platform.FloatToInt(x1), game.platform.FloatToInt(y1),
			game.platform.FloatToInt(width), game.platform.FloatToInt(height),
			inAtlasId, 0, ColorCi.FromArgb(a, r, g, b), false);
	}

	public override void Draw2dTextures(Draw2dData[] todraw, int todrawLength, int textureId)
	{
		game.Draw2dTextures(todraw, todrawLength, textureId);
	}


	public override void Draw2dText(string text, float x, float y, FontCi font)
	{
		game.Draw2dText(text, font, x, y, null, false);
	}

	public override void OrthoMode()
	{
		game.OrthoMode(GetWindowWidth(), GetWindowHeight());
	}

	public override void PerspectiveMode()
	{
		game.PerspectiveMode();
	}

	public override DictionaryStringString GetPerformanceInfo()
	{
		return game.performanceinfo;
	}
}
