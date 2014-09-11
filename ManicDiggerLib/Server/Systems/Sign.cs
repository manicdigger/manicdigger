using System;
using System.Collections.Generic;
using System.Text;

public class ServerSystemSign : ServerSystem
{
    public override void Update(Server server_, float dt)
    {
        server = server_;
        if (!started)
        {
            started = true;
            server_.modManager.RegisterOnBlockUseWithTool(OnUseWithTool);
        }
    }
    bool started;
    Server server;

    void OnUseWithTool(int player, int x, int y, int z, int tool)
    {
        if (server.modManager.GetBlockName(tool) == "Sign")
        {
            ServerChunk c = server.d_Map.GetChunk(x, y, z);
            if (c == null)
            {
                return;
            }
            ServerEntity e = new ServerEntity();
            e.position = new ServerEntityPositionAndOrientation();
            e.position.x = x + one / 2;
            e.position.y = z;
            e.position.z = y + one / 2;
            e.position.heading = GetHeading(server.modManager.GetPlayerPositionX(player), server.modManager.GetPlayerPositionY(player), e.position.x, e.position.z);
            e.drawModel = new ServerEntityAnimatedModel();
            e.drawModel.model = "signmodel.txt";
            e.drawModel.texture = "signmodel.png";
            e.sign = new ServerEntitySign();
            e.sign.text = "Hello world!";
            if (c.Entities == null)
            {
                c.Entities = new ServerEntity[256];
            }
            if (c.Entities.Length < c.EntitiesCount + 1)
            {
                Array.Resize(ref c.Entities, c.EntitiesCount + 1);
            }
            c.Entities[c.EntitiesCount++] = e;
        }
    }

    byte GetHeading(float posx, float posy, float targetx, float targety)
    {
        float deltaX = targetx - posx;
        float deltaY = targety - posy;
        //Angle to x-axis: cos(beta) = x / |length|
        double headingDeg =  (360.0 / (2.0 * Math.PI)) * Math.Acos(deltaX / Math.Sqrt(deltaX * deltaX + deltaY * deltaY)) + 90.0;
        //Add 2 Pi if value is negative
        if (deltaY < 0)
        {
            headingDeg = -headingDeg - 180.0;
        }
        if (headingDeg < 0)
        {
            headingDeg += 360.0;
        }
        if (headingDeg > 360.0)
        {
            headingDeg -= 360.0;
        }
        //Convert to value between 0 and 255 and return
        return (byte)((headingDeg / 360.0) * 256.0);
    }
}
