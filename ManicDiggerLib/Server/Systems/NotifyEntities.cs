using System;
using System.Collections.Generic;
using System.Text;

public class ServerSystemNotifyEntities : ServerSystem
{
    public override void Update(Server server, float dt)
    {
        foreach (var k in server.clients)
        {
            ClientOnServer c = k.Value;
            if (c.IsBot)
            {
                //Bots don't need to be sent packets with other player's positions
                continue;
            }
            NotifyPlayers(server, k.Key);
            NotifyPlayerPositions(server, k.Key, dt);
            NotifyEntities(server, k.Key);
        }
    }

    //PlayerName = c.displayColor + c.playername,

    //    //spectators invisible to players
    //if (clients[playerid].IsSpectator && (!clients[clientid].IsSpectator))
    //{
    //    //Set spectator position to some fake value
    //    sentpos[0] = -1000 * 32;
    //    sentpos[1] = -1000 * 32;
    //    sentpos[2] = 0;
    //}

    void NotifyPlayers(Server server, int clientid)
    {
        ClientOnServer c = server.clients[clientid];
        // EntitySpawn
        foreach (var k in server.clients)
        {
            if (k.Value.state != ClientStateOnServer.Playing)
            {
                continue;
            }
            if (c.playersDirty[k.Key])
            {
                Packet_ServerEntity e = ToNetworkEntity(server.serverPlatform, k.Value.entity);
                server.SendPacket(clientid, ServerPackets.EntitySpawn(k.Key, e));
                c.playersDirty[k.Key] = false;
            }
        }
        // EntityDespawn
    }

    int PlayerPositionUpdatesPerSecond = 10;

    // EntityPositionAndOrientation
    void NotifyPlayerPositions(Server server, int clientid, float dt)
    {
        ClientOnServer c = server.clients[clientid];
        c.notifyPlayerPositionsAccum += dt;
        if (c.notifyPlayerPositionsAccum < (one / PlayerPositionUpdatesPerSecond))
        {
            return;
        }
        c.notifyPlayerPositionsAccum = 0;
        foreach (var k in server.clients)
        {
            if (k.Value.state != ClientStateOnServer.Playing)
            {
                continue;
            }
            if (k.Key == clientid && (k.Value.positionOverride == null))
            {
                continue;
            }
            if (server.DistanceSquared(server.PlayerBlockPosition(server.clients[k.Key]), server.PlayerBlockPosition(server.clients[clientid])) > server.config.PlayerDrawDistance * server.config.PlayerDrawDistance)
            {
                continue;
            }
            if (k.Value.positionOverride != null)
            {
                k.Value.entity.position = k.Value.positionOverride;
                k.Value.positionOverride = null;
            }
            Packet_PositionAndOrientation position = ToNetworkEntityPosition(server.serverPlatform, server.clients[k.Key].entity.position);
            server.SendPacket(clientid, ServerPackets.EntityPositionAndOrientation(k.Key, position));
        }
    }

    void NotifyEntities(Server server, int clientid)
    {
        ClientOnServer c = server.clients[clientid];
        int mapx = c.PositionMul32GlX / 32;
        int mapy = c.PositionMul32GlZ / 32;
        int mapz = c.PositionMul32GlY / 32;
    }

    Packet_PositionAndOrientation ToNetworkEntityPosition(ServerPlatform platform, ServerEntityPositionAndOrientation position)
    {
        Packet_PositionAndOrientation p = new Packet_PositionAndOrientation();
        p.X = platform.FloatToInt(position.x * 32);
        p.Y = platform.FloatToInt(position.y * 32);
        p.Z = platform.FloatToInt(position.z * 32);
        p.Heading = position.heading;
        p.Pitch = position.pitch;
        p.Stance = position.stance;
        return p;
    }

    Packet_ServerEntity ToNetworkEntity(ServerPlatform platform, ServerEntity entity)
    {
        Packet_ServerEntity p = new Packet_ServerEntity();
        if (entity.position != null)
        {
            p.Position = ToNetworkEntityPosition(platform, entity.position);
        }
        if (entity.drawModel != null)
        {
            p.DrawModel = new Packet_ServerEntityAnimatedModel();
            p.DrawModel.EyeHeight = platform.FloatToInt(entity.drawModel.eyeHeight * 32);
            p.DrawModel.Model_ = entity.drawModel.model;
            p.DrawModel.ModelHeight = platform.FloatToInt(entity.drawModel.modelHeight * 32);
            p.DrawModel.Texture_ = entity.drawModel.texture;
            p.DrawModel.DownloadSkin = entity.drawModel.downloadSkin ? 1 : 0;
        }
        if (entity.drawName != null)
        {
            p.DrawName_ = new Packet_ServerEntityDrawName();
            p.DrawName_.Name = entity.drawName.name;
        }
        if (entity.drawText != null)
        {
            p.DrawText = new Packet_ServerEntityDrawText();
            p.DrawText.Dx = platform.FloatToInt(entity.drawText.dx * 32);
            p.DrawText.Dy = platform.FloatToInt(entity.drawText.dy * 32);
            p.DrawText.Dz = platform.FloatToInt(entity.drawText.dz * 32);
            p.DrawText.Rotx = platform.FloatToInt(entity.drawText.rotx);
            p.DrawText.Roty = platform.FloatToInt(entity.drawText.roty);
            p.DrawText.Rotz = platform.FloatToInt(entity.drawText.rotz);
            p.DrawText.Text = entity.drawText.text;
        }
        if (entity.drawBlock != null)
        {
            p.DrawBlock = new Packet_ServerEntityDrawBlock();
            p.DrawBlock.BlockType = entity.drawBlock.blockType;
        }
        if (entity.push != null)
        {
            p.Push = new Packet_ServerEntityPush();
            p.Push.RangeFloat = platform.FloatToInt(entity.push.range * 32);
        }
        return p;
    }
}
