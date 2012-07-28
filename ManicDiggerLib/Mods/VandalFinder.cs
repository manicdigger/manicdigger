using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ManicDigger.Mods
{
    public class VandalFinder : IMod
    {
        public void Start(ModManager m)
        {
            this.m = m;
            m.SetBlockType("VandalFinder", new BlockType()
            {
                AllTextures = "VandalFinder",
                DrawType = DrawType.Solid,
                WalkableType = WalkableType.Solid,
                IsUsable = true,
                IsTool = true,
            });
            m.AddToCreativeInventory("VandalFinder");
            m.RegisterOnBlockBuild(OnBuild);
            m.RegisterOnBlockDelete(OnDelete);
            m.RegisterOnBlockUseWithTool(OnUseWithTool);
            m.RegisterOnLoad(OnLoad);
            m.RegisterOnSave(OnSave);
        }
        
        ModManager m;
        public int MaxEntries = 50 * 1000;

        void OnUseWithTool(int player, int x, int y, int z, int tool)
        {
            if (m.GetBlockName(tool) == "VandalFinder")
            {
                ShowBlockLog(player, x, y, z);
            }
        }

        void ShowBlockLog(int player, int x, int y, int z)
        {
            List<string> messages = new List<string>();
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                LogLine l = lines[i];
                if (l.x == x && l.y == y && l.z == z)
                {
                    messages.Add(string.Format("{0} {1} {2} {3}", l.timestamp.ToString(), l.Playername, m.GetBlockName(l.blocktype), l.build ? "build" : "delete"));
                    if (messages.Count > 10)
                    {
                        return;
                    }
                }
            }
            messages.Reverse();
            for (int i = 0; i < messages.Count; i++)
            {
                m.SendMessage(player, messages[i]);
            }
            if (messages.Count == 0)
            {
                m.SendMessage(player, "Block was never changed.");
            }
        }

        class LogLine
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
