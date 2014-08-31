using System.Collections.Generic;

public class ServerSystemNotifyMonsters : ServerSystem
{
    public override void Update(Server server, float dt)
    {
        if (server.config.Monsters)
        {
            foreach (var k in server.clients)
            {
                k.Value.notifyMonstersTimer.Update(delegate { NotifyMonsters(server, k.Key); });
            }
        }
    }

    void NotifyMonsters(Server server, int clientid)
    {
        ClientOnServer c = server.clients[clientid];
        int mapx = c.PositionMul32GlX / 32;
        int mapy = c.PositionMul32GlZ / 32;
        int mapz = c.PositionMul32GlY / 32;
        //3x3x3 chunks
        List<Packet_ServerMonster> p = new List<Packet_ServerMonster>();
        for (int xx = -1; xx < 2; xx++)
        {
            for (int yy = -1; yy < 2; yy++)
            {
                for (int zz = -1; zz < 2; zz++)
                {
                    int cx = (mapx / Server.chunksize) + xx;
                    int cy = (mapy / Server.chunksize) + yy;
                    int cz = (mapz / Server.chunksize) + zz;
                    if (!MapUtil.IsValidChunkPos(server.d_Map, cx, cy, cz, Server.chunksize))
                    {
                        continue;
                    }
                    ServerChunk chunk = server.d_Map.GetChunkValid(cx, cy, cz);
                    if (chunk == null || chunk.Monsters == null)
                    {
                        continue;
                    }
                    foreach (Monster m in new List<Monster>(chunk.Monsters))
                    {
                        MonsterWalk(server, m);
                    }
                    foreach (Monster m in chunk.Monsters)
                    {
                        float progress = m.WalkProgress;
                        if (progress < 0) //delay
                        {
                            progress = 0;
                        }
                        byte heading = 0;
                        if (m.WalkDirection.x == -1 && m.WalkDirection.y == 0) { heading = (byte)(((int)byte.MaxValue * 3) / 4); }
                        if (m.WalkDirection.x == 1 && m.WalkDirection.y == 0) { heading = byte.MaxValue / 4; }
                        if (m.WalkDirection.x == 0 && m.WalkDirection.y == -1) { heading = 0; }
                        if (m.WalkDirection.x == 0 && m.WalkDirection.y == 1) { heading = byte.MaxValue / 2; }
                        var mm = new Packet_ServerMonster()
                        {
                            Id = m.Id,
                            MonsterType = m.MonsterType,
                            Health = m.Health,
                            PositionAndOrientation = new Packet_PositionAndOrientation()
                            {
                                Heading = heading,
                                Pitch = 0,
                                X = (int)((m.X + progress * m.WalkDirection.x) * 32 + 16),
                                Y = (int)((m.Z + progress * m.WalkDirection.z) * 32),
                                Z = (int)((m.Y + progress * m.WalkDirection.y) * 32 + 16),
                            }
                        };
                        p.Add(mm);
                    }
                }
            }
        }
        //send only nearest monsters
        p.Sort((a, b) =>
        {
            Vector3i posA = new Vector3i(a.PositionAndOrientation.X, a.PositionAndOrientation.Y, a.PositionAndOrientation.Z);
            Vector3i posB = new Vector3i(b.PositionAndOrientation.X, b.PositionAndOrientation.Y, b.PositionAndOrientation.Z);
            ClientOnServer client = server.clients[clientid];
            Vector3i posPlayer = new Vector3i(client.PositionMul32GlX, client.PositionMul32GlY, client.PositionMul32GlZ);
            return server.DistanceSquared(posA, posPlayer).CompareTo(server.DistanceSquared(posB, posPlayer));
        }
        );
        if (p.Count > sendmaxmonsters)
        {
            p.RemoveRange(sendmaxmonsters, p.Count - sendmaxmonsters);
        }
        server.SendPacket(clientid, server.Serialize(new Packet_Server()
        {
            Id = Packet_ServerIdEnum.Monster,
            Monster = new Packet_ServerMonsters() { Monsters = p.ToArray() }
        }));
    }
    int sendmaxmonsters = 10;
    void MonsterWalk(Server server, Monster m)
    {
        m.WalkProgress += 0.3f;
        if (m.WalkProgress < 1)
        {
            return;
        }
        int oldcx = m.X / Server.chunksize;
        int oldcy = m.Y / Server.chunksize;
        int oldcz = m.Z / Server.chunksize;
        server.d_Map.GetChunkValid(oldcx, oldcy, oldcz).Monsters.Remove(m);
        m.X += m.WalkDirection.x;
        m.Y += m.WalkDirection.y;
        m.Z += m.WalkDirection.z;
        int newcx = m.X / Server.chunksize;
        int newcy = m.Y / Server.chunksize;
        int newcz = m.Z / Server.chunksize;
        if (server.d_Map.GetChunkValid(newcx, newcy, newcz).Monsters == null)
        {
            server.d_Map.GetChunkValid(newcx, newcy, newcz).Monsters = new List<Monster>();
        }
        server.d_Map.GetChunkValid(newcx, newcy, newcz).Monsters.Add(m);
        /*
        if (rnd.Next(3) == 0)
        {
            m.WalkDirection = new Vector3i();
            m.WalkProgress = -2;
            return;
        }
        */
        List<Vector3i> l = new List<Vector3i>();
        for (int zz = -1; zz < 2; zz++)
        {
            if (server.d_Map.GetBlock(m.X + 1, m.Y, m.Z + zz) == 0
                 && server.d_Map.GetBlock(m.X + 1, m.Y, m.Z + zz - 1) != 0)
            {
                l.Add(new Vector3i(1, 0, zz));
            }
            if (server.d_Map.GetBlock(m.X - 1, m.Y, m.Z + zz) == 0
                && server.d_Map.GetBlock(m.X - 1, m.Y, m.Z + zz - 1) != 0)
            {
                l.Add(new Vector3i(-1, 0, zz));
            }
            if (server.d_Map.GetBlock(m.X, m.Y + 1, m.Z + zz) == 0
                && server.d_Map.GetBlock(m.X, m.Y + 1, m.Z + zz - 1) != 0)
            {
                l.Add(new Vector3i(0, 1, zz));
            }
            if (server.d_Map.GetBlock(m.X, m.Y - 1, m.Z + zz) == 0
                && server.d_Map.GetBlock(m.X, m.Y - 1, m.Z + zz - 1) != 0)
            {
                l.Add(new Vector3i(0, -1, zz));
            }
        }
        Vector3i dir;
        if (l.Count > 0)
        {
            dir = l[server.rnd.Next(l.Count)];
        }
        else
        {
            dir = new Vector3i();
        }
        m.WalkDirection = dir;
        m.WalkProgress = 0;
    }
}
