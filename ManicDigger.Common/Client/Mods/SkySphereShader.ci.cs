//Based on http://csc.lsu.edu/~kooima/misc/cs594/final/index.html

public class ModSkySphereShader : ClientMod
{
	public ModSkySphereShader()
	{
		shader = new ShaderCi();
		started = false;
		skysphereSize = 1000;
		textureStars = new TextureCi();
		textureSky = new TextureCi();
		textureGlow = new TextureCi();
	}
	ModelData skymodel;
	ShaderCi shader;
	TextureCi textureStars;
	TextureCi textureSky;
	TextureCi textureGlow;
	float skysphereSize;

	public override void OnNewFrameDraw3d(Game game, float deltaTime)
	{
		game.SkySphereNight = true;

		game.platform.GlDisableFog();
		DrawSkySphere(game);
		game.SetFog();
	}

	bool started;

	void SetShaderUniforms(Game game)
	{
		game.platform.GlUniform3f(shader.GetUniformLocation("sunPos"), game.sunPositionX, game.sunPositionY, game.sunPositionZ);
		game.platform.GlUniform1i(shader.GetUniformLocation("stars"), 1);
		game.platform.GlUniform1i(shader.GetUniformLocation("color"), 2);
		game.platform.GlUniform1i(shader.GetUniformLocation("glow"), 3);
		//game.pMatrix.Peek();
		//game.mvMatrix.Peek();
	}

	internal void DrawSkySphere(Game game)
	{
		if (!started)
		{
			started = true;

			// create skybox object
			skymodel = SphereModelData.GetSphereModelData(skysphereSize, skysphereSize, 4, 3);
			//skymodel.setMode(DrawModeEnum.Lines);

			// load necessary textures
			BitmapCi starsBmp = game.platform.BitmapCreateFromPng(game.GetFile("skyspherenight.png"), game.GetFileLength("skyspherenight.png"));
			textureStars.Init(game.platform, starsBmp);
			game.platform.BitmapDelete(starsBmp);
			BitmapCi skyBmp = game.platform.BitmapCreateFromPng(game.GetFile("sky.png"), game.GetFileLength("sky.png"));
			textureSky.Init(game.platform, skyBmp);
			game.platform.BitmapDelete(skyBmp);
			BitmapCi glowBmp = game.platform.BitmapCreateFromPng(game.GetFile("glow.png"), game.GetFileLength("glow.png"));
			textureGlow.Init(game.platform, glowBmp);
			game.platform.BitmapDelete(glowBmp);

			// initialize shader
			shader.Init(game.platform);
			shader.Compile(ShaderSources.VertexSkysphere, ShaderType.VertexShader);
			shader.Compile(ShaderSources.FragmentSkysphere, ShaderType.FragmentShader);
			shader.Link();
		}

		// set up shader and its uniform variables
		shader.BeginUse();
		SetShaderUniforms(game);

		// bind required textures to texture units
		game.platform.GlActiveTexture(1);
		textureStars.BeginUse();
		game.platform.GlActiveTexture(2);
		textureSky.BeginUse();
		game.platform.GlActiveTexture(3);
		textureGlow.BeginUse();

		// draw graphics using the shader
		game.platform.GLDisableAlphaTest();
		Draw(game, game.currentfov());
		game.platform.GLEnableAlphaTest();

		// unbind used textures
		game.platform.GlActiveTexture(3);
		textureGlow.EndUse();
		game.platform.GlActiveTexture(2);
		textureSky.EndUse();
		game.platform.GlActiveTexture(1);
		textureStars.EndUse();

		// reset shader program and active texture unit
		shader.EndUse();
		game.platform.GlActiveTexture(0);
	}

	public void Draw(Game game, float fov)
	{
		game.Set3dProjection(skysphereSize + 1, fov);
		game.GLMatrixModeModelView();
		game.GLPushMatrix();
		game.GLTranslate(game.player.position.x,
			game.player.position.y,
			game.player.position.z);
		game.DrawModelData(skymodel);
		game.GLPopMatrix();
		game.Set3dProjection(game.zfar(), fov);
	}
}
