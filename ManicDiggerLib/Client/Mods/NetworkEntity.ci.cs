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
        ToClientEntity(game, packet.EntitySpawn.Entity_, entity);
        game.entities[packet.EntitySpawn.Id] = entity;
        if (packet.EntitySpawn.Id == game.LocalPlayerId)
        {
            entity.networkPosition = null;
            entity.physicsState = new CharacterPhysicsState();
            game.player = entity;
            if (!game.spawned)
            {
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

    public static Entity ToClientEntity(Game game, Packet_ServerEntity entity, Entity old)
    {
        if (entity.Position != null)
        {
            old.networkPosition = ToClientEntityPosition(entity.Position);
            old.networkPosition.PositionLoaded = true;
            old.networkPosition.LastUpdateMilliseconds = game.platform.TimeMillisecondsFromStart();
            old.position = ToClientEntityPosition(entity.Position);
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
                old.drawModel.Model_ = "player2.txt";
            }
            old.drawModel.DownloadSkin = entity.DrawModel.DownloadSkin != 0;
        }
        if (entity.DrawName_ != null)
        {
            old.drawName = new DrawName();
            old.drawName.Name = entity.DrawName_.Name;
            if (!game.platform.StringStartsWithIgnoreCase(old.drawName.Name, "&"))
            {
                old.drawName.Name = game.platform.StringFormat("&f{0}", old.drawName.Name);
            }
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
        if (entity.DrawBlock != null)
        {
        }
        if (entity.Push != null)
        {
            old.push = new Packet_ServerExplosion();
            old.push.RangeFloat = entity.Push.RangeFloat;
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
            game.player.position.x = pos.x;
            game.player.position.y = pos.y;
            game.player.position.z = pos.z;
            entity.networkPosition = null;
        }
    }
}

public class ClientPacketHandlerEntityDespawn : ClientPacketHandler
{
    public override void Handle(Game game, Packet_Server packet)
    {
        game.entities[packet.EntityDespawn.Id] = null;
    }
}
