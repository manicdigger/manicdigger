using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

public class ServerSystemSign : ServerSystem
{
    public override void Update(Server server_, float dt)
    {
        server = server_;
        if (!started)
        {
            started = true;
            server_.modManager.RegisterOnBlockUseWithTool(OnUseWithTool);
            server_.modEventHandlers.onupdateentity.Add(UpdateEntity);
            server_.modEventHandlers.onuseentity.Add(OnUseEntity);
            server.modEventHandlers.ondialogclick2.Add(OnDialogClick);
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

            if (!server.CheckBuildPrivileges(player, x, y, z, Packet_BlockSetModeEnum.Create))
            {
                return;
            }

            ServerEntity e = new ServerEntity();
            e.position = new ServerEntityPositionAndOrientation();
            e.position.x = x + one / 2;
            e.position.y = z;
            e.position.z = y + one / 2;
            e.position.heading = EntityHeading.GetHeading(server.modManager.GetPlayerPositionX(player), server.modManager.GetPlayerPositionY(player), e.position.x, e.position.z);
            e.sign = new ServerEntitySign();
            e.sign.text = "Hello world!";
            server.AddEntity(x, y, z, e);
        }
    }

    void UpdateEntity(int chunkx, int chunky, int chunkz, int id)
    {
        ServerEntity e = server.GetEntity(chunkx, chunky, chunkz, id);
        if (e.sign == null)
        {
            return;
        }
        if (e.drawModel == null)
        {
            e.drawModel = new ServerEntityAnimatedModel();
        }
        e.drawModel.model = "signmodel.txt";
        e.drawModel.texture = "signmodel.png";
        e.drawModel.modelHeight = one * 13 / 10;

        if (e.drawText == null)
        {
            e.drawText = new ServerEntityDrawText();
        }
        e.drawText.text = e.sign.text;
        e.drawText.dx = one * 3 / 32;
        e.drawText.dy = one * 36 / 32;
        e.drawText.dz = one * 3 / 32;
        e.usable = true;
        if (e.drawName == null)
        {
            e.drawName = new ServerEntityDrawName();
            e.drawName.name = "Sign";
            e.drawName.onlyWhenSelected = true;
        }
    }

    void OnUseEntity(int player, int chunkx, int chunky, int chunkz, int id)
    {
        ServerEntity e = server.GetEntity(chunkx, chunky, chunkz, id);
        if (e.sign == null)
        {
            return;
        }
        if (!server.CheckBuildPrivileges(player, (int)e.position.x, (int)e.position.z, (int)e.position.y, Packet_BlockSetModeEnum.Use))
        {
            return;
        }
        ManicDigger.Dialog d = new ManicDigger.Dialog();
        d.Width = 400;
        d.Height = 200;
        d.IsModal = true;
        d.Widgets = new ManicDigger.Widget[4];
        int widgetCount = 0;
        var font = new DialogFont("Verdana", 11f, DialogFontStyle.Bold);
        d.Widgets[widgetCount++] = Widget.MakeSolid(0, 0, 300, 200, Game.ColorFromArgb(255, 50, 50, 50));
        d.Widgets[widgetCount++] = Widget.MakeTextBox(e.sign.text, font, 50, 50, 200, 50, Game.ColorFromArgb(255, 0, 0, 0));
        Widget okHandler = Widget.MakeSolid(100, 100, 100, 50, Game.ColorFromArgb(255, 100, 100, 100));
        okHandler.ClickKey = (char)13;
        okHandler.Id = "UseSign_OK";
        d.Widgets[widgetCount++] = okHandler;
        d.Widgets[widgetCount++] = Widget.MakeText("OK", font, 100, 100, Game.ColorFromArgb(255, 0, 0, 0));
        ServerEntityId id_ = new ServerEntityId();
        id_.chunkx = chunkx;
        id_.chunky = chunky;
        id_.chunkz = chunkz;
        id_.id = id;
        server.clients[player].editingSign = id_;
        server.SendDialog(player, "UseSign", d);
    }

    void OnDialogClick(DialogClickArgs args)
    {
        if (args.GetWidgetId() != "UseSign_OK")
        {
            //Return when dialog is not a sign
            return;
        }
        var c = server.clients[args.GetPlayer()];
        string newText = args.GetTextBoxValue()[1];
        ServerEntityId id = c.editingSign;
        if (newText != "")
        {
            c.editingSign = null;
            ServerEntity e = server.GetEntity(id.chunkx, id.chunky, id.chunkz, id.id);
            e.sign.text = newText;
            server.SetEntityDirty(id);
        }
        else
        {
            server.DespawnEntity(id);
        }
        server.SendDialog(args.GetPlayer(), "UseSign", null);
    }
}
