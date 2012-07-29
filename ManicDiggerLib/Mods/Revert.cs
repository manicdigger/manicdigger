using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger.Mods
{
    public class Revert : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("BuildLog");
        }
        public void Start(ModManager m)
        {
            this.m = m;
            m.RegisterPrivilege("revert");
            m.RegisterCommandHelp("revert", "/revert [playername] [number of changes]");
            lines = (List<ManicDigger.Mods.BuildLog.LogLine>)m.GetGlobalDataNotSaved("LogLines");
            m.RegisterOnCommand(OnCommand);
        }
        ModManager m;
        public int MaxRevert = 2000;
        List<ManicDigger.Mods.BuildLog.LogLine> lines = new List<BuildLog.LogLine>();
        bool OnCommand(int player, string command, string argument)
        {
            if (command.Equals("revert", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!m.PlayerHasPrivilege(player, "revert"))
                {
                    m.SendMessage(player, m.colorError() + "Insufficient privileges to use revert.");
                    return true;
                }
                string targetplayername;
                int n;
                try
                {
                    string[] args = argument.Split(' ');
                    targetplayername = args[0];
                    n = int.Parse(args[1]);
                }
                catch
                {
                    m.SendMessage(player, m.colorError() + "Invalid arguments. Type /help to see command's usage.");
                    return false;
                }
                if (n > MaxRevert)
                {
                    m.SendMessage(player, m.colorError() + string.Format("Can't revert more than {0} block changes", MaxRevert));
                }

                int reverted = 0;
                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    ManicDigger.Mods.BuildLog.LogLine l = lines[i];
                    if (l.Playername.Equals(targetplayername, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (l.build)
                        {
                            m.SetBlock(l.x, l.y, l.z, 0);
                        }
                        else
                        {
                            m.SetBlock(l.x, l.y, l.z, l.blocktype);
                        }
                        reverted++;
                        if (reverted >= n)
                        {
                            break;
                        }
                    }
                }
                if (reverted == 0)
                {
                    m.SendMessage(player, string.Format(m.colorError() + "Not reverted any block changes by player {0}.", targetplayername));
                }
                else
                {
                    m.SendMessageToAll(string.Format("Reverted {0} block changes by player {1}", reverted, targetplayername));
                }
                return true;
            }
            return false;
        }
    }
}
