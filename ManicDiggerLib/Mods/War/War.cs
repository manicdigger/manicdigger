using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger.Mods
{
    public class War : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("DefaultWar");
        }
        public void Start(ModManager m)
        {
            this.m = m;
            m.SetCreative(true);
            m.SetWorldSize(256, 256, 128);
            m.RegisterOnPlayerJoin(PlayerJoin);
            m.RegisterOnDialogClick(DialogClick);
            m.RenderHint(RenderHint.Nice);
        }

        ModManager m;

        void PlayerJoin(int playerid)
        {
            m.EnableFreemove(playerid, false);
            Dialog d = new Dialog();
            List<Widget> widgets = new List<Widget>();
            Widget background = new Widget();
            background.X = 0;
            background.Y = 0;
            background.Width = 800;
            background.Height = 800;
            background.Image = "SelectTeam";
            widgets.Add(background);
            Widget w1 = new Widget();
            w1.Id = "Team1";
            w1.Text = "Press 1 to join Blue";
            w1.X = 50;
            w1.Y = 400;
            w1.ClickKey = '1';
            widgets.Add(w1);
            Widget w2 = new Widget();
            w2.Text = "Press 2 to join Green";
            w2.Id = "Team2";
            w2.X = 600;
            w2.Y = 400;
            w2.ClickKey = '2';
            widgets.Add(w2);
            Widget w3 = new Widget();
            w3.Text = "Press 3 to spectate";
            w3.Id = "Team3";
            w3.X = 300;
            w3.Y = 400;
            w3.ClickKey = '3';
            widgets.Add(w3);
            d.Width = 800;
            d.Height = 600;
            d.Widgets = widgets.ToArray();
            m.SendDialog(playerid, "SelectTeam" + playerid, d);
        }

        enum Team
        {
            Blue,
            Green,
            Spectator,
        }

        Dictionary<int, Team> teams = new Dictionary<int, Team>();

        void DialogClick(int playerid, string widget)
        {
            if (widget == "Team1")
            {
                m.SendDialog(playerid, "SelectTeam" + playerid, null);
                teams[playerid] = Team.Blue;
                m.EnableFreemove(playerid, false);
            }
            if (widget == "Team2")
            {
                m.SendDialog(playerid, "SelectTeam" + playerid, null);
                teams[playerid] = Team.Green;
                m.EnableFreemove(playerid, false);
            }
            if (widget == "Team3")
            {
                m.SendDialog(playerid, "SelectTeam" + playerid, null);
                teams[playerid] = Team.Spectator;
                m.EnableFreemove(playerid, true);
            }
            Respawn(playerid);
        }

        void Respawn(int playerid)
        {
            int posx = -1;
            int posy = -1;
            int posz = -1;
            switch (teams[playerid])
            {
                case Team.Blue:
                    posx = 50;
                    posy = m.GetMapSizeY() / 2;
                    break;
                case Team.Green:
                    posx = m.GetMapSizeX() - 50;
                    posy = m.GetMapSizeY() / 2;
                    break;
                case Team.Spectator:
                    posx = m.GetMapSizeX() / 2;
                    posy = m.GetMapSizeY() / 2;
                    break;
            }
            posz = BlockHeight(posx, posy);
            m.SetPlayerPosition(playerid, posx, posy, posz);
        }

        public int BlockHeight(int x, int y)
        {
            for (int z = m.GetMapSizeZ() - 1; z >= 0; z--)
            {
                if (m.GetBlock(x, y, z) != 0)
                {
                    return z + 1;
                }
            }
            return m.GetMapSizeZ() / 2;
        }
    }
}
