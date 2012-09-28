using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

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
            m.RegisterOnWeaponHit(Hit);
            m.RegisterOnSpecialKey(RespawnKey);
            m.RegisterOnSpecialKey(OnTabKey);
            m.RegisterOnDialogClick(OnTabResponse);
            m.RegisterOnSpecialKey(OnSelectTeamKey);
            m.RegisterTimer(UpdateTab, 1);
        }

        public bool EnableTeamkill = true;

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
        string BlueColor = "&1";
        string GreenColor = "&2";
        //string SpectatorColor = "&7";

        void DialogClick(int playerid, string widget)
        {
            if (widget == "Team1")
            {
                m.SendDialog(playerid, "SelectTeam" + playerid, null);
                teams[playerid] = Team.Blue;
                m.SetPlayerModel(playerid, "player.txt", "playerblue.png");
                m.EnableFreemove(playerid, false);
                m.SendMessageToAll(string.Format("{0} joins {1}&f team.", m.GetPlayerName(playerid), BlueColor + " " + "Blue"));
                Respawn(playerid);
            }
            if (widget == "Team2")
            {
                m.SendDialog(playerid, "SelectTeam" + playerid, null);
                teams[playerid] = Team.Green;
                m.SetPlayerModel(playerid, "player.txt", "playergreen.png");
                m.EnableFreemove(playerid, false);
                m.SendMessageToAll(string.Format("{0} joins {1}&f team.", m.GetPlayerName(playerid), GreenColor + " " + "Green"));
                Respawn(playerid);
            }
            if (widget == "Team3")
            {
                m.SendDialog(playerid, "SelectTeam" + playerid, null);
                teams[playerid] = Team.Spectator;
                m.SetPlayerModel(playerid, "player.txt", "mineplayer.png");
                m.EnableFreemove(playerid, true);
                m.SendMessageToAll(string.Format("{0} becomes a &7 spectator&f.", m.GetPlayerName(playerid)));
                Respawn(playerid);
            }
        }

        void OnSelectTeamKey(int player, SpecialKey key)
        {
            if (key != SpecialKey.SelectTeam)
            {
                return;
            }
            PlayerJoin(player);
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

        void Hit(int sourceplayer, int targetplayer, int block, bool head)
        {
            if (!EnableTeamkill)
            {
                if (teams[sourceplayer] == teams[targetplayer])
                {
                    return;
                }
            }
            int health = m.GetPlayerHealth(targetplayer);
            health -= head ? 10 : 3;
            if (health <= 0)
            {
                m.PlaySoundAt((int)m.GetPlayerPositionX(targetplayer),
                    (int)m.GetPlayerPositionY(targetplayer),
                    (int)m.GetPlayerPositionZ(targetplayer), "death.ogg");
                m.SetPlayerHealth(targetplayer, m.GetPlayerMaxHealth(targetplayer), m.GetPlayerMaxHealth(targetplayer));
                Respawn(targetplayer);
                if (teams[sourceplayer] == teams[targetplayer])
                {
                    m.SendMessageToAll(string.Format("{0} kills {1} - " + m.colorError() + "TEAMKILL", m.GetPlayerName(sourceplayer), m.GetPlayerName(targetplayer)));
                    
                }
                else
                {
                   m.SendMessageToAll(string.Format("{0} kills {1}", m.GetPlayerName(sourceplayer), m.GetPlayerName(targetplayer)));
                }
            }
            else
            {
                m.SetPlayerHealth(targetplayer, health, m.GetPlayerMaxHealth(targetplayer));
                m.PlaySoundAt((int)m.GetPlayerPositionX(targetplayer),
                    (int)m.GetPlayerPositionY(targetplayer),
                    (int)m.GetPlayerPositionZ(targetplayer), "grunt1.ogg");
            }
        }

        void RespawnKey(int player, SpecialKey key)
        {
            if (key != SpecialKey.Respawn)
            {
                return;
            }
            m.PlaySoundAt((int)m.GetPlayerPositionX(player),
                (int)m.GetPlayerPositionY(player),
                (int)m.GetPlayerPositionZ(player), "death.ogg");
            m.SetPlayerHealth(player, m.GetPlayerMaxHealth(player), m.GetPlayerMaxHealth(player));
            Respawn(player);
            m.SendMessageToAll(string.Format("{0} dies", m.GetPlayerName(player)));
        }

        void OnTabKey(int player, SpecialKey key)
        {
            if (key != SpecialKey.TabPlayerList)
            {
                return;
            }
            tabOpen[m.GetPlayerName(player)] = true;
            Dialog d = new Dialog();
            d.IsModal = true;
            List<Widget> widgets = new List<Widget>();
            
            // table alignment
            float tableX = xcenter(m.GetScreenResolution(player)[0], tableWidth);
            float tableY = tableMarginTop;
            
            // text to draw
            string row1 = m.GetServerName();
            row1 = cutText(row1, HeadingFont, tableWidth - 2 * tablePadding);

            string row2 = m.GetServerMotd();
            row2 = cutText(row2, SmallFontBold, tableWidth - 2 * tablePadding);
            
            
            string row3_1 = "IP: " + m.GetServerIp() + ":" + m.GetServerPort();
            string row3_2 = (int)(m.GetPlayerPing(player) * 1000) + "ms";

            string row4_1 = "Players: " + m.AllPlayers().Length;
            string row4_2 = "Page: " + (page + 1) + "/" + (pageCount + 1);

            string row5_1 = "ID";
            string row5_2 = "Player";
            string row5_3 = "Ping";

            // row heights
            float row1Height = textHeight(row1, HeadingFont) + 2 * tablePadding;
            float row2Height = textHeight(row2, SmallFontBold) + 2 * tablePadding;
            float row3Height = textHeight(row3_1, SmallFont) + 2 * tablePadding;
            float row4Height = textHeight(row4_1, SmallFont) + 2 * tablePadding;
            float row5Height = textHeight(row5_1, NormalFontBold) + 2 * tablePadding;
            float listEntryHeight = textHeight("Player", NormalFont) + 2 * listEntryPaddingTopBottom;

            float heightOffset = 0;
            
            // determine how many entries can be displayed
            tableHeight = m.GetScreenResolution(player)[1] - tableMarginTop - tableMarginBottom;
            float availableEntrySpace = tableHeight - row1Height - row2Height - row3Height - row4Height - (row5Height + tableLineWidth);

            int entriesPerPage = (int)(availableEntrySpace / listEntryHeight);
            pageCount = (int)Math.Ceiling((float)(m.AllPlayers().Length / entriesPerPage));
            if (page > pageCount)
            {
                page = 0;
            }

            // 1 - heading: Servername
            widgets.Add(Widget.MakeSolid(tableX, tableY, tableWidth, row1Height, Color.DarkGreen.ToArgb()));
            widgets.Add(Widget.MakeText(row1, HeadingFont, tableX + xcenter(tableWidth, textWidth(row1, HeadingFont)), tableY + tablePadding, TEXT_COLOR.ToArgb()));
            heightOffset += row1Height;
            
            // 2 - MOTD
            widgets.Add(Widget.MakeSolid(tableX, tableY + heightOffset, tableWidth, row2Height, Color.ForestGreen.ToArgb()));
            widgets.Add(Widget.MakeText(row2, SmallFontBold, tableX + xcenter(tableWidth, textWidth(row2, SmallFontBold)), tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
            heightOffset += row2Height;
            
            // 3 - server info: IP Motd Serverping
            widgets.Add(Widget.MakeSolid(tableX, tableY + heightOffset, tableWidth, row3Height, Color.DarkSeaGreen.ToArgb()));
            // row3_1 - IP align left
            widgets.Add(Widget.MakeText(row3_1, SmallFont, tableX + tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
            // row3_2 - Serverping align right
            widgets.Add(Widget.MakeText(row3_2, SmallFont, tableX + tableWidth - textWidth(row3_2, SmallFont) - tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
            heightOffset += row3Height;
            
            // 4 - infoline: Playercount, Page
            widgets.Add(Widget.MakeSolid(tableX, tableY + heightOffset, tableWidth, row4Height, Color.DimGray.ToArgb()));
            // row4_1 PlayerCount
            widgets.Add(Widget.MakeText(row4_1, SmallFont, tableX + tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
            // row4_2 PlayerCount
            widgets.Add(Widget.MakeText(row4_2, SmallFont, tableX + tableWidth - textWidth(row4_2, SmallFont) - tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
            heightOffset += row4Height;
            
            // 5 - playerlist heading: ID | Player | Ping
            widgets.Add(Widget.MakeSolid(tableX, tableY + heightOffset, tableIdColumnWidth, row5Height, Color.DarkGray.ToArgb()));
            widgets.Add(Widget.MakeSolid(tableX + tableIdColumnWidth, tableY + heightOffset, tablePlayerColumnWidth, row5Height, Color.DarkGray.ToArgb()));
            widgets.Add(Widget.MakeSolid(tableX + tableIdColumnWidth + tablePlayerColumnWidth, tableY + heightOffset, tablePingColumnWidth, row5Height, Color.DarkGray.ToArgb()));
            // separation lines
            widgets.Add(Widget.MakeSolid(tableX + tableIdColumnWidth, tableY + heightOffset, tableLineWidth, row5Height, Color.DimGray.ToArgb()));
            widgets.Add(Widget.MakeSolid(tableX + tableIdColumnWidth + tablePlayerColumnWidth - tableLineWidth, tableY + heightOffset, tableLineWidth, row5Height, Color.DimGray.ToArgb()));
            // row4_1 ID - align center
            widgets.Add(Widget.MakeText(row5_1, NormalFontBold, tableX + xcenter(tableIdColumnWidth, textWidth(row5_1, NormalFontBold)), tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
            // row4_2 Player - align center
            widgets.Add(Widget.MakeText(row5_2, NormalFontBold, tableX + tableIdColumnWidth + tablePlayerColumnWidth / 2 - textWidth(row5_2, NormalFontBold) / 2, tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
            // row4_3 Ping - align center
            widgets.Add(Widget.MakeText(row5_3, NormalFontBold, tableX + tableIdColumnWidth + tablePlayerColumnWidth + tablePingColumnWidth / 2 - textWidth(row5_3, NormalFontBold) / 2, tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
            heightOffset += row5Height;
            // horizontal line
            widgets.Add(Widget.MakeSolid(tableX, tableY + heightOffset, tableWidth, tableLineWidth, Color.DimGray.ToArgb()));
            heightOffset += tableLineWidth;
            
            // 6 - actual playerlist
            // entries:
            Color entryRowColor;
            int[] AllPlayers = m.AllPlayers();
            for (int i = page * entriesPerPage; i < Math.Min(AllPlayers.Length, page * entriesPerPage + entriesPerPage); i++)
            {
                if (i % 2 == 0)
                {
                    entryRowColor = Color.Gainsboro;
                }
                else
                {
                    entryRowColor = Color.Honeydew;
                }
                widgets.Add(Widget.MakeSolid(tableX, tableY + heightOffset, tableIdColumnWidth, listEntryHeight, entryRowColor.ToArgb()));
                widgets.Add(Widget.MakeSolid(tableX + tableIdColumnWidth, tableY + heightOffset, tablePlayerColumnWidth, listEntryHeight, entryRowColor.ToArgb()));
                widgets.Add(Widget.MakeSolid(tableX + tableIdColumnWidth + tablePlayerColumnWidth, tableY + heightOffset, tablePingColumnWidth, listEntryHeight, entryRowColor.ToArgb()));

                // separation lines
                widgets.Add(Widget.MakeSolid(tableX + tableIdColumnWidth, tableY + heightOffset, tableLineWidth, listEntryHeight, Color.DimGray.ToArgb()));
                widgets.Add(Widget.MakeSolid(tableX + tableIdColumnWidth + tablePlayerColumnWidth - tableLineWidth, tableY + heightOffset, tableLineWidth, listEntryHeight, Color.DimGray.ToArgb()));

                widgets.Add(Widget.MakeText(AllPlayers[i].ToString(), NormalFont, tableX + tableIdColumnWidth - textWidth(AllPlayers[i].ToString(), NormalFont) - tablePadding, tableY + heightOffset + listEntryPaddingTopBottom, TEXT_COLOR.ToArgb()));
                widgets.Add(Widget.MakeText(m.GetPlayerName(AllPlayers[i]), NormalFont, tableX + tableIdColumnWidth + tablePadding, tableY + heightOffset + listEntryPaddingTopBottom, TEXT_COLOR.ToArgb()));
                int ping = (int)(m.GetPlayerPing(AllPlayers[i]) * 1000);
                widgets.Add(Widget.MakeText(ping.ToString(), NormalFont, tableX + tableIdColumnWidth + tablePlayerColumnWidth + tablePingColumnWidth - textWidth(ping.ToString(), NormalFont) - tablePadding, tableY + heightOffset + listEntryPaddingTopBottom, TEXT_COLOR.ToArgb()));
                heightOffset += listEntryHeight;
            }
            var wtab = Widget.MakeSolid(0, 0, 0, 0, 0);
            wtab.ClickKey = '\t';
            wtab.Id = "Tab";
            widgets.Add(wtab);
            var wesc = Widget.MakeSolid(0, 0, 0, 0, 0);
            wesc.ClickKey = (char)27;
            wesc.Id = "Esc";
            widgets.Add(wesc);
            
            d.Width = m.GetScreenResolution(player)[0];
            d.Height = m.GetScreenResolution(player)[1];
            d.Widgets = widgets.ToArray();
            m.SendDialog(player, "PlayerList", d);
        }

        private int pageCount = 0; //number of pages for player table entries
        private int page = 0; // current displayed page

        // fonts
        public readonly Color TEXT_COLOR = Color.Black;
        public DialogFont HeadingFont = new DialogFont("Verdana", 11f, DialogFontStyle.Bold);
        public DialogFont NormalFont = new DialogFont("Verdana", 10f, DialogFontStyle.Regular);
        public DialogFont NormalFontBold = new DialogFont("Verdana", 10f, DialogFontStyle.Bold);
        public DialogFont SmallFont = new DialogFont("Verdana", 8f, DialogFontStyle.Regular);
        public DialogFont SmallFontBold = new DialogFont("Verdana", 8f, DialogFontStyle.Bold);
        
        private float tableMarginTop = 10;
        private float tableMarginBottom = 10;
        private float tableWidth = 500;
        private float tableHeight = 500;
        private float tablePadding = 5;
        private float listEntryPaddingTopBottom = 2;
        private float tableIdColumnWidth = 50;
        private float tablePlayerColumnWidth = 400;
        private float tablePingColumnWidth = 50;
        private float tableLineWidth = 2;

        public bool NextPage()
        {
            if (this.page < this.pageCount)
            {
                this.page++;
                return true;
            }
            return false;
        }

        public bool PreviousPage()
        {
            if (this.page > 0)
            {
                this.page--;
                return true;
            }
            return false;
        }

        private float xcenter(float outerWidth, float innerWidth)
        {
            return (outerWidth / 2 - innerWidth / 2);
        }
        private float ycenter(float outerHeight, float innerHeight)
        {
            return (outerHeight / 2 - innerHeight / 2);
        }
        private float textWidth(string text, DialogFont font)
        {
            return m.MeasureTextSize(text, font)[0];
        }
        private float textHeight(string text, DialogFont font)
        {
            return m.MeasureTextSize(text, font)[1];
        }
        private string cutText(string text, DialogFont font, float maxWidth)
        {
            while (textWidth(text, font) > maxWidth && text.Length > 3)
            {
                text = text.Remove(text.Length - 1);
            }
            return text;
        }

        void OnTabResponse(int player, string widgetid)
        {
            if (widgetid == "Tab" || widgetid == "Esc")
            {
                m.SendDialog(player, "PlayerList", null);
                tabOpen.Remove(m.GetPlayerName(player));
            }
        }

        Dictionary<string, bool> tabOpen = new Dictionary<string, bool>();

        void UpdateTab()
        {
            foreach (var k in new Dictionary<string,bool>(tabOpen))
            {
                foreach (int p in m.AllPlayers())
                {
                    if (k.Key == m.GetPlayerName(p))
                    {
                        OnTabKey(p, SpecialKey.TabPlayerList);
                        goto nexttab;
                    }
                }
                //player disconnected
                tabOpen.Remove(k.Key);
            nexttab:
                ;
            }
        }
    }
}
