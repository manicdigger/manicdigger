//Based on http://csc.lsu.edu/~kooima/misc/cs594/final/index.html
//But instead of pixel shader it uses vertex colors.
public class ModSkySphereAnimated : ClientMod
{
    public ModSkySphereAnimated()
    {
        stars = new ModSkySphereStatic();
    }
    ModelData skymodel;
    ClientMod stars;

    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        game.SkySphereNight = true;
        stars.OnNewFrameDraw3d(game, deltaTime);

        game.platform.GlDisableFog();
        DrawSkySphere(game);
        game.SetFog();
    }

    internal bool started;
    const int textureSize = 512;

    internal void DrawSkySphere(Game game)
    {
        if (!started)
        {
            started = true;
            BitmapCi skyBmp = game.platform.BitmapCreateFromPng(game.GetFile("sky.png"), game.GetFileLength("sky.png"));
            BitmapCi glowBmp = game.platform.BitmapCreateFromPng(game.GetFile("glow.png"), game.GetFileLength("glow.png"));
            skyPixels = new int[textureSize * textureSize * 4];
            glowPixels = new int[textureSize * textureSize * 4];
            game.platform.BitmapGetPixelsArgb(skyBmp, skyPixels);
            game.platform.BitmapGetPixelsArgb(glowBmp, glowPixels);
            game.platform.BitmapDelete(skyBmp);
            game.platform.BitmapDelete(glowBmp);
        }

        game.platform.GLDisableAlphaTest();
        game.platform.GlDisableDepthTest();
        Draw(game, game.currentfov());
        game.platform.GLEnableAlphaTest();
        game.platform.GlEnableDepthTest();
    }

    int[] skyPixels;
    int[] glowPixels;

    public void Draw(Game game, float fov)
    {
        int size = 1000;
        if (game.fancySkysphere)
        {
            //Fancy skysphere with higher resolution
            skymodel = GetSphereModelData2(skymodel, game.platform, size, size, 64, 64, skyPixels, glowPixels, game.sunPositionX, game.sunPositionY, game.sunPositionZ);
        }
        else
        {
            //Normal resolution. Far more FPS
            skymodel = GetSphereModelData2(skymodel, game.platform, size, size, 20, 20, skyPixels, glowPixels, game.sunPositionX, game.sunPositionY, game.sunPositionZ);
        }
        game.Set3dProjection(size * 2, fov);
        game.GLMatrixModeModelView();
        game.GLPushMatrix();
        game.GLTranslate(game.player.position.x,
            game.player.position.y,
            game.player.position.z);
        game.platform.BindTexture2d(0);
        game.DrawModelData(skymodel);
        game.GLPopMatrix();
        game.Set3dProjection(game.zfar(), fov);
    }

    public ModelData GetSphereModelData2(ModelData data,
    GamePlatform platform,
    float radius, float height, int segments, int rings,
    int[] skyPixels_, int[] glowPixels_,
    float sunX, float sunY, float sunZ)
    {
        int i = 0;

        if (data == null)
        {
            data = new ModelData();
            data.xyz = new float[rings * segments * 3];
            data.uv = new float[rings * segments * 2];
            data.rgba = new byte[rings * segments * 4];
            data.SetVerticesCount(segments * rings);
            data.SetIndicesCount(segments * rings * 6);
            data.setIndices(SphereModelData.CalculateElements(radius, height, segments, rings));
        }
        
        // Load data into a vertex buffer or a display list afterwards.

        for (int y = 0; y < rings; y++)
        {
            float yFloat = y;
            float phiFloat = (yFloat / (rings - 1)) * Game.GetPi();
            for (int x = 0; x < segments; x++)
            {
                float xFloat = x;
                float theta = (xFloat / (segments - 1)) * 2 * Game.GetPi();
                float vx = radius * Platform.Sin(phiFloat) * Platform.Cos(theta);
                float vy = height * Platform.Cos(phiFloat);
                float vz = radius * Platform.Sin(phiFloat) * Platform.Sin(theta);
                float u = xFloat / (segments - 1);
                float v = yFloat / (rings - 1);
                data.xyz[i * 3 + 0] = vx;
                data.xyz[i * 3 + 1] = vy;
                data.xyz[i * 3 + 2] = vz;
                data.uv[i * 2 + 0] = u;
                data.uv[i * 2 + 1] = v;

                float vertexLength = platform.MathSqrt(vx * vx + vy * vy + vz * vz);
                float vertexXNormalized = vx / vertexLength;
                float vertexYNormalized = vy / vertexLength;
                float vertexZNormalized = vz / vertexLength;

                float sunLength = platform.MathSqrt(sunX * sunX + sunY * sunY + sunZ * sunZ);
                if (sunLength == 0) { sunLength = 1; }
                float sunXNormalized = sunX / sunLength;
                float sunYNormalized = sunY / sunLength;
                float sunZNormalized = sunZ / sunLength;

                // Compute the proximity of this fragment to the sun.

                float dx = vertexXNormalized - sunXNormalized;
                float dy = vertexYNormalized - sunYNormalized;
                float dz = vertexZNormalized - sunZNormalized;
                float proximityToSun = 1 - (platform.MathSqrt(dx * dx + dy * dy + dz * dz) / 2);

                // Look up the sky color and glow colors.
                float one = 1;

                int skyColor = Texture2d(platform, skyPixels_, (sunYNormalized + 2) / 4, 1 - ((vertexYNormalized + 1) / 2));

                float skyColorA = one * Game.ColorA(skyColor) / 255;
                float skyColorR = one * Game.ColorR(skyColor) / 255;
                float skyColorG = one * Game.ColorG(skyColor) / 255;
                float skyColorB = one * Game.ColorB(skyColor) / 255;

                int glowColor = Texture2d(platform, glowPixels_, (sunYNormalized + one) / 2, 1 - proximityToSun);
                float glowColorA = one * Game.ColorA(glowColor) / 255;
                float glowColorR = one * Game.ColorR(glowColor) / 255;
                float glowColorG = one * Game.ColorG(glowColor) / 255;
                float glowColorB = one * Game.ColorB(glowColor) / 255;

                // Combine the color and glow giving the pixel value.
                float colorR = skyColorR + glowColorR * glowColorA;
                float colorG = skyColorG + glowColorG * glowColorA;
                float colorB = skyColorB + glowColorB * glowColorA;
                float colorA = skyColorA;

                if (colorR > 1) { colorR = 1; }
                if (colorG > 1) { colorG = 1; }
                if (colorB > 1) { colorB = 1; }
                if (colorA > 1) { colorA = 1; }

                data.rgba[i * 4 + 0] = Game.IntToByte(platform.FloatToInt(colorR * 255));
                data.rgba[i * 4 + 1] = Game.IntToByte(platform.FloatToInt(colorG * 255));
                data.rgba[i * 4 + 2] = Game.IntToByte(platform.FloatToInt(colorB * 255));
                data.rgba[i * 4 + 3] = Game.IntToByte(platform.FloatToInt(colorA * 255));
                i++;
            }
        }
        //data.setMode(DrawModeEnum.Triangles);
        return data;
    }

    static int Texture2d(GamePlatform platform, int[] pixelsArgb, float x, float y)
    {
        int px = platform.FloatToInt(x * (textureSize - 1));
        int py = platform.FloatToInt(y * (textureSize - 1));
        px = positive_modulo(px, (textureSize - 1));
        py = positive_modulo(py, (textureSize - 1));
        return pixelsArgb[MapUtilCi.Index2d(px, py, textureSize)];
    }

    static int positive_modulo(int i, int n)
    {
        return (i % n + n) % n;
    }
}
