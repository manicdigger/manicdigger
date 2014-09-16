using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

public class ServerSystemPermissionSign : ServerSystem
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
            server_.modEventHandlers.onhitentity.Add(OnHitEntity);
            server.modEventHandlers.ondialogclick2.Add(OnDialogClick);
        }
    }
    bool started;
    Server server;

    void OnUseWithTool(int player, int x, int y, int z, int tool)
    {
        if (server.modManager.GetBlockName(tool) == "PermissionSign")
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
            e.position.heading = EntityHeading.GetHeading(server.modManager.GetPlayerPositionX(player), server.modManager.GetPlayerPositionY(player), e.position.x, e.position.z);

            e.permissionSign = new ServerEntityPermissionSign();
            e.permissionSign.name = "Admin";
            e.permissionSign.type = PermissionSignType.Group;
            server.AddEntity(x, y, z, e);
        }
    }

    void UpdateEntity(int chunkx, int chunky, int chunkz, int id)
    {
        ServerEntity e = server.GetEntity(chunkx, chunky, chunkz, id);
        if (e.permissionSign == null)
        {
            return;
        }
        if (e.drawModel == null)
        {
            e.drawModel = new ServerEntityAnimatedModel();
        }
        e.drawModel.model = "signmodel.txt";
        e.drawModel.texture = "permissionsignmodel.png";
        e.drawModel.modelHeight = one * 13 / 10;

        if (e.drawText == null)
        {
            e.drawText = new ServerEntityDrawText();
        }
        e.drawText.text = e.permissionSign.name;
        if (e.permissionSign.type == PermissionSignType.Group)
        {
            e.drawText.text = "&4" + e.drawText.text;
        }
        e.drawText.dx = one * 3 / 32;
        e.drawText.dy = one * 36 / 32;
        e.drawText.dz = one * 3 / 32;
        e.usable = true;
        if (e.drawName == null)
        {
            e.drawName = new ServerEntityDrawName();
            e.drawName.name = "Permission Sign";
            e.drawName.onlyWhenSelected = true;
        }
    }

    void OnUseEntity(int player, int chunkx, int chunky, int chunkz, int id)
    {
        ServerEntity e = server.GetEntity(chunkx, chunky, chunkz, id);
        if (e.permissionSign == null)
        {
            return;
        }
        ManicDigger.Dialog d = new ManicDigger.Dialog();
        d.Width = 400;
        d.Height = 400;
        d.IsModal = true;
        d.Widgets = new ManicDigger.Widget[4 + server.serverClient.Groups.Count * 2];
        int widgetCount = 0;
        var font = new DialogFont("Verdana", 11f, DialogFontStyle.Bold);
        d.Widgets[widgetCount++] = Widget.MakeSolid(0, 0, 400, 400, Game.ColorFromArgb(255, 50, 50, 50));
        d.Widgets[widgetCount++] = Widget.MakeTextBox(e.permissionSign.name, font, 50, 50, 200, 50, Game.ColorFromArgb(255, 0, 0, 0));
        for (int i = 0; i < server.serverClient.Groups.Count; i++)
        {
            Group g = server.serverClient.Groups[i];
            Widget button = Widget.MakeSolid(50, 150 + i * 50, 100, 40, Game.ColorFromArgb(255, 100, 100, 100));
            button.ClickKey = (char)13;
            button.Id = "PermissionSignGroup" + g.Name;
            d.Widgets[widgetCount++] = button;
            d.Widgets[widgetCount++] = Widget.MakeText(g.Name, font, 50, 150 + i * 50, Game.ColorFromArgb(255, 0, 0, 0));
        }
        Widget okHandler = Widget.MakeSolid(200, 50, 100, 50, Game.ColorFromArgb(255, 100, 100, 100));
        okHandler.ClickKey = (char)13;
        okHandler.Id = "UsePermissionSign_OK";
        d.Widgets[widgetCount++] = okHandler;
        d.Widgets[widgetCount++] = Widget.MakeText("Set player", font, 200, 50, Game.ColorFromArgb(255, 0, 0, 0));
        ServerEntityId id_ = new ServerEntityId();
        id_.chunkx = chunkx;
        id_.chunky = chunky;
        id_.chunkz = chunkz;
        id_.id = id;
        server.clients[player].editingSign = id_;
        server.SendDialog(player, "UseSign", d);
    }
    
    void OnHitEntity(int player, int chunkx, int chunky, int chunkz, int id)
    {
        ServerEntity e = server.GetEntity(chunkx, chunky, chunkz, id);
        if (e.permissionSign == null)
        {
            return;
        }
        if (e.drawArea == null)
        {
            e.drawArea = new ServerEntityDrawArea();
            int sizex = 32;
            int sizey = 32;
            int sizez = 32;
            e.drawArea.x = (int)e.position.x - sizex / 2;
            e.drawArea.y = (int)e.position.y - sizey / 2;
            e.drawArea.z = (int)e.position.z - sizez / 2;
            e.drawArea.sizex = sizex;
            e.drawArea.sizey = sizey;
            e.drawArea.sizez = sizez;
            e.drawArea.visibleToClientId = player;
        }
        else
        {
            e.drawArea = null;
        }
        ServerEntityId id_ = new ServerEntityId();
        id_.chunkx = chunkx;
        id_.chunky = chunky;
        id_.chunkz = chunkz;
        id_.id = id;
        server.SetEntityDirty(id_);
    }

    void OnDialogClick(DialogClickArgs args)
    {
        string name = null;
        PermissionSignType type = PermissionSignType.Player;
        if (args.GetWidgetId() == "UsePermissionSign_OK")
        {
            name = args.GetTextBoxValue()[1];
            type = PermissionSignType.Player;

            for (int i = 0; i < server.serverClient.Groups.Count; i++)
            {
                Group g = server.serverClient.Groups[i];
                if (name == g.Name)
                {
                    type = PermissionSignType.Group;
                }
            }
        }
        else if (args.GetWidgetId().StartsWith("PermissionSignGroup"))
        {
            for (int i = 0; i < server.serverClient.Groups.Count; i++)
            {
                Group g = server.serverClient.Groups[i];
                if (args.GetWidgetId() == "PermissionSignGroup" + g.Name)
                {
                    name = g.Name;
                    type = PermissionSignType.Group;
                }
            }
        }
        else
        {
            //Return when dialog is not a sign
            return;
        }

        ClientOnServer c = server.clients[args.GetPlayer()];
        ServerEntityId id = c.editingSign;
        if (name != "")
        {
            c.editingSign = null;
            ServerEntity e = server.GetEntity(id.chunkx, id.chunky, id.chunkz, id.id);
            e.permissionSign.name = name;
            e.permissionSign.type = type;
            server.SetEntityDirty(id);
        }
        else
        {
            server.DespawnEntity(id);
        }
        server.SendDialog(args.GetPlayer(), "UseSign", null);
    }
}
