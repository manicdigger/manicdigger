using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ManicDigger.Mods
{
    public class BuildLog : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("Default");
        }
        public void Start(ModManager m)
        {
            this.m = m;
            m.RegisterOnBlockBuild(OnBuild);
            m.RegisterOnBlockDelete(OnDelete);
            m.RegisterOnLoad(OnLoad);
            m.RegisterOnSave(OnSave);
            m.SetGlobalDataNotSaved("LogLines", lines);
        }
        ModManager m;
        public int MaxEntries = 50 * 1000;
        public class LogLine
        {
            public DateTime timestamp;
            public short x;
            public short y;
            public short z;
            public int blocktype;
            public bool build;
            public string Playername;
            public string ip;
        }

        List<LogLine> lines = new List<LogLine>();

        void OnLoad()
        {
            try
            {
                byte[] b = m.GetGlobalData("BuildLog");
                if (b != null)
                {
                    MemoryStream ms = new MemoryStream(b);
                    BinaryReader br = new BinaryReader(ms);
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        var l = new LogLine();
                        l.timestamp = new DateTime(br.ReadInt64());
                        l.x = br.ReadInt16();
                        l.y = br.ReadInt16();
                        l.z = br.ReadInt16();
                        l.blocktype = br.ReadInt16();
                        l.build = br.ReadBoolean();
                        l.Playername = br.ReadString();
                        l.ip = br.ReadString();
                        lines.Add(l);
                    }
                }
            }
            catch
            {
                //corrupted
                OnSave();
            }
        }

        void OnSave()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((int)lines.Count);
            for (int i = 0; i < lines.Count; i++)
            {
                bw.Write((long)lines[i].timestamp.Ticks);
                bw.Write((short)lines[i].x);
                bw.Write((short)lines[i].y);
                bw.Write((short)lines[i].z);
                bw.Write((short)lines[i].blocktype);
                bw.Write((bool)lines[i].build);
                bw.Write((string)lines[i].Playername);
                bw.Write((string)lines[i].ip);
            }
            m.SetGlobalData("BuildLog", ms.ToArray());
        }

        void OnBuild(int player, int x, int y, int z)
        {
            lines.Add(new LogLine()
            {
                ip = m.GetPlayerIp(player),
                blocktype = m.GetBlock(x, y, z),
                build = true,
                x = (short)x,
                y = (short)y,
                z = (short)z,
                Playername = m.GetPlayerName(player),
                timestamp = DateTime.UtcNow,
            });
            if (lines.Count > MaxEntries)
            {
                lines.RemoveRange(0, 1000);
            }
        }

        void OnDelete(int player, int x, int y, int z, int oldblock)
        {
            lines.Add(new LogLine()
            {
                ip = m.GetPlayerIp(player),
                blocktype = oldblock,
                build = false,
                x = (short)x,
                y = (short)y,
                z = (short)z,
                Playername = m.GetPlayerName(player),
                timestamp = DateTime.UtcNow,
            });
            if (lines.Count > MaxEntries)
            {
                lines.RemoveRange(0, 1000);
            }
        }
    }
}
