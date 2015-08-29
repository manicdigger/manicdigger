public class ModNetworkEntity : ClientMod
{
    public ModNetworkEntity()
    {
        spawn = new ClientPacketHandlerEntitySpawn();
        position = new ClientPacketHandlerEntityPosition();
        despawn = new ClientPacketHandlerEntityDespawn();
    }
    public override void OnNewFrame(Game game, NewFrameEventArgs args)
    {
        game.packetHandlers[Packet_ServerIdEnum.EntitySpawn] = spawn;
        game.packetHandlers[Packet_ServerIdEnum.EntityPosition] = position;
        game.packetHandlers[Packet_ServerIdEnum.EntityDespawn] = despawn;
    }
    ClientPacketHandlerEntitySpawn spawn;
    ClientPacketHandlerEntityPosition position;
    ClientPacketHandlerEntityDespawn despawn;
}

public class ClientPacketHandlerEntitySpawn : ClientPacketHandler
{
    public override void Handle(Game game, Packet_Server packet)
    {
        Entity entity = game.entities[packet.EntitySpawn.Id];
        if (entity == null)
        {
            entity = new Entity();
        }
        ToClientEntity(game, packet.EntitySpawn.Entity_, entity, packet.EntitySpawn.Id != game.LocalPlayerId);
        game.entities[packet.EntitySpawn.Id] = entity;
        if (packet.EntitySpawn.Id == game.LocalPlayerId)
        {
            entity.networkPosition = null;
            game.player = entity;
            if (!game.spawned)
            {
                entity.scripts[entity.scriptsCount++] = new ScriptCharacterPhysics();
                game.MapLoaded();
                game.spawned = true;
            }
        }
    }

    public static EntityPosition_ ToClientEntityPosition(Packet_PositionAndOrientation pos)
    {
        float one = 1;
        EntityPosition_ p = new EntityPosition_();
        p.x = (one * pos.X) / 32;
        p.y = (one * pos.Y) / 32;
        p.z = (one * pos.Z) / 32;
        p.rotx = Game.Angle256ToRad(pos.Pitch);
        p.roty = Game.Angle256ToRad(pos.Heading);
        return p;
    }

    public static Entity ToClientEntity(Game game, Packet_ServerEntity entity, Entity old, bool updatePosition)
    {
        if (entity.Position != null)
        {
            if (old.position == null || updatePosition)
            {
                old.networkPosition = ToClientEntityPosition(entity.Position);
                old.networkPosition.PositionLoaded = true;
                old.networkPosition.LastUpdateMilliseconds = game.platform.TimeMillisecondsFromStart();
                old.position = ToClientEntityPosition(entity.Position);
            }
        }
        if (entity.DrawModel != null)
        {
            old.drawModel = new EntityDrawModel();
            old.drawModel.eyeHeight = game.DeserializeFloat(entity.DrawModel.EyeHeight);
            old.drawModel.ModelHeight = game.DeserializeFloat(entity.DrawModel.ModelHeight);
            old.drawModel.Texture_ = entity.DrawModel.Texture_;
            old.drawModel.Model_ = entity.DrawModel.Model_;
            if (old.drawModel.Model_ == null)
            {
                old.drawModel.Model_ = "player.txt";
            }
            old.drawModel.DownloadSkin = entity.DrawModel.DownloadSkin != 0;
        }
        if (entity.DrawName_ != null)
        {
            old.drawName = new DrawName();
            if (entity.DrawName_.Color != null)
            {
               old.drawName.Name = game.platform.StringFormat2("{0}{1}", entity.DrawName_.Color, entity.DrawName_.Name);
            }
            else
            {
                old.drawName.Name = entity.DrawName_.Name;
            }
            if (!game.platform.StringStartsWithIgnoreCase(old.drawName.Name, "&"))
            {
                old.drawName.Name = game.platform.StringFormat("&f{0}", old.drawName.Name);
            }
            old.drawName.OnlyWhenSelected = entity.DrawName_.OnlyWhenSelected;
            old.drawName.ClientAutoComplete = entity.DrawName_.ClientAutoComplete;
        }
        if (entity.DrawText != null)
        {
            old.drawText = new EntityDrawText();
            old.drawText.text = entity.DrawText.Text;
            float one_ = 1;
            old.drawText.dx = one_ * entity.DrawText.Dx / 32;
            old.drawText.dy = one_ * entity.DrawText.Dy / 32;
            old.drawText.dz = one_ * entity.DrawText.Dz / 32;
        }
        else
        {
            old.drawText = null;
        }
        if (entity.DrawBlock != null)
        {
        }
        if (entity.Push != null)
        {
            old.push = new Packet_ServerExplosion();
            old.push.RangeFloat = entity.Push.RangeFloat;
        }
        else
        {
            old.push = null;
        }
        old.usable = entity.Usable;
        if (entity.DrawArea != null)
        {
            //New DrawArea
            old.drawArea = new EntityDrawArea();
            old.drawArea.x = entity.DrawArea.X;
            old.drawArea.y = entity.DrawArea.Y;
            old.drawArea.z = entity.DrawArea.Z;
            old.drawArea.sizex = entity.DrawArea.Sizex;
            old.drawArea.sizey = entity.DrawArea.Sizey;
            old.drawArea.sizez = entity.DrawArea.Sizez;
        }
        else
        {
            //DrawArea deleted/not present
            old.drawArea = null;
        }
        return old;
    }
}

public class ClientPacketHandlerEntityPosition : ClientPacketHandler
{
    public override void Handle(Game game, Packet_Server packet)
    {
        Entity entity = game.entities[packet.EntityPosition.Id];
        EntityPosition_ pos = ClientPacketHandlerEntitySpawn.ToClientEntityPosition(packet.EntityPosition.PositionAndOrientation);
        entity.networkPosition = pos;
        entity.networkPosition.PositionLoaded = true;
        entity.networkPosition.LastUpdateMilliseconds = game.platform.TimeMillisecondsFromStart();
        if (packet.EntityPosition.Id == game.LocalPlayerId)
        {
            // Override local player position if necessary (teleport)
            game.player.position.x = pos.x;
            game.player.position.y = pos.y;
            game.player.position.z = pos.z;
            game.player.position.rotx = pos.rotx;
            game.player.position.roty = pos.roty;
            game.player.position.rotz = pos.rotz;
            entity.networkPosition = null;
        }
        else if (entity.push != null)
        {
            // Create push force for any player except local player
            entity.push.XFloat = packet.EntityPosition.PositionAndOrientation.X;
            entity.push.YFloat = packet.EntityPosition.PositionAndOrientation.Z;
            entity.push.ZFloat = packet.EntityPosition.PositionAndOrientation.Y;
        }
    }
}

public class ClientPacketHandlerEntityDespawn : ClientPacketHandler
{
    public override void Handle(Game game, Packet_Server packet)
    {
        //Check if Entity has DownloadSkin set and texture is not empty or default player texture
        if (game.entities[packet.EntityDespawn.Id] != null)
        {
            if (game.entities[packet.EntityDespawn.Id].drawModel != null && game.entities[packet.EntityDespawn.Id].drawModel.DownloadSkin)
            {
                int currentTex = game.entities[packet.EntityDespawn.Id].drawModel.CurrentTexture;
                if (currentTex > 0 && currentTex != game.GetTexture("mineplayer.png"))
                {
                    //Entity probably is a player. Set CurrentTexture to -1 and then delete stored texture.
                    game.entities[packet.EntityDespawn.Id].drawModel.CurrentTexture = -1;
                    game.DeleteTexture(game.entities[packet.EntityDespawn.Id].drawName.Name);
                }
            }
        }
        game.entities[packet.EntityDespawn.Id] = null;
    }
}
