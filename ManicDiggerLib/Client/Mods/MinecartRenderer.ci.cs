public class Minecart
{
    internal bool enabled;
    internal float positionX;
    internal float positionY;
    internal float positionZ;
    internal VehicleDirection12 direction;
    internal VehicleDirection12 lastdirection;
    internal float progress;
}

public class ModDrawMinecarts : ClientMod
{
    public ModDrawMinecarts()
    {
        minecarttexture = -1;
    }
    int minecarttexture;

    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        for (int i = 0; i < game.entitiesCount; i++)
        {
            if (game.entities[i] == null) { continue; }
            if (game.entities[i].minecart == null) { continue; }
            Minecart m = game.entities[i].minecart;
            if (!m.enabled) { continue; }
            Draw(game, m.positionX, m.positionY, m.positionZ, m.direction, m.lastdirection, m.progress);
        }
    }

    public void Draw(Game game, float positionX, float positionY, float positionZ, VehicleDirection12 dir, VehicleDirection12 lastdir, float progress)
    {
        float one = 1;
        if (minecarttexture == -1)
        {
            minecarttexture = game.GetTexture("minecart.png");
        }
        game.GLPushMatrix();
        float pX = positionX;
        float pY = positionY;
        float pZ = positionZ;
        pY += -(one * 7 / 10);
        game.GLTranslate(pX, pY, pZ);
        float currot = vehiclerotation(dir);
        float lastrot = vehiclerotation(lastdir);
        //double rot = lastrot + (currot - lastrot) * progress;
        float rot = AngleInterpolation.InterpolateAngle360(game.platform, lastrot, currot, progress);
        game.GLRotate(-rot - 90, 0, 1, 0);
        RectangleFloat[] cc = CuboidRenderer.CuboidNet(8, 8, 8, 0, 0);
        CuboidRenderer.CuboidNetNormalize(cc, 32, 16);
        game.platform.BindTexture2d(minecarttexture);
        CuboidRenderer.DrawCuboid(game, -(one * 5 / 10), -(one * 3 / 10), -(one * 5 / 10), 1, 1, 1, cc, 1);
        game.GLPopMatrix();
    }

    float vehiclerotation(VehicleDirection12 dir)
    {
        switch (dir)
        {
            case VehicleDirection12.VerticalUp:
                return 0;
            case VehicleDirection12.DownRightRight:
            case VehicleDirection12.UpLeftUp:
                return 45;
            case VehicleDirection12.HorizontalRight:
                return 90;
            case VehicleDirection12.UpRightRight:
            case VehicleDirection12.DownLeftDown:
                return 90 + 45;
            case VehicleDirection12.VerticalDown:
                return 180;
            case VehicleDirection12.UpLeftLeft:
            case VehicleDirection12.DownRightDown:
                return 180 + 45;
            case VehicleDirection12.HorizontalLeft:
                return 180 + 90;
            case VehicleDirection12.UpRightUp:
            case VehicleDirection12.DownLeftLeft:
                return 180 + 90 + 45;
            default:
                return 0;
        }
    }
}
