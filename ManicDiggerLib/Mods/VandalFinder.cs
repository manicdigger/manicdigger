using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ManicDigger.Mods
{
    public class VandalFinder : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("Default");
            m.RequireMod("BuildLog");
        }
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
            m.RegisterOnBlockUseWithTool(OnUseWithTool);
            lines = (List<ManicDigger.Mods.BuildLog.LogLine>)m.GetGlobalDataNotSaved("LogLines");
        }
        
        ModManager m;
        List<ManicDigger.Mods.BuildLog.LogLine> lines = new List<BuildLog.LogLine>();

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
                ManicDigger.Mods.BuildLog.LogLine l = lines[i];
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
    }
}
