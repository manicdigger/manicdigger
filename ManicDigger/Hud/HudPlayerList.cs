using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Renderers;
using System.Drawing;

namespace ManicDigger
{
    public class HudPlayerList
    {
        public The3d d_The3d;
        [Inject]
        public IViewportSize d_ViewportSize;
        public ServerInformation ServerInfo;

        private int page = 0;
        private int pageCount = 0;

        private float m_tableX;
        private float m_tableY;

        private float m_lineWidth = 2;

        private float m_tableMarginTop = 10;
        private float m_tableMarginBottom = 10;
        private float m_width = 500;
        private float m_height = 500;
        private float m_padding = 5;
        private float m_entryPaddingTopBottom = 2;

        private float m_idColumnWidth = 50;
        private float m_playerColumnWidth = 400;
        private float m_pingColumnWidth = 50;

        private float m_fontSizeNormal = 10f;
        private float m_fontSizeSmall = 8f;
        private float m_fontSizeHeading = 11f;
        private Color m_fontColor = Color.White;

        public void DrawHudPlayerList()
        {
            float heightOffset = 0;

            string row1 = this.ServerInfo.ServerName;

            string row2_1 = "IP: " + this.ServerInfo.ServerIp;
            string row2_2 = this.ServerInfo.ServerMotd;
            string row2_3 = this.ServerInfo.ServerPing.Milliseconds + "ms";

            string row3_1 = "ID";
            string row3_2 = "Player";
            string row3_3 = "Ping";

            string row31_1 = "Players: " + this.ServerInfo.Players.Count;
            string row31_2 = "Page: " + (this.page + 1) + "/" + (this.pageCount + 1);

            this.m_tableX = xcenter(d_ViewportSize.Width, m_width);
            this.m_tableY = m_tableMarginTop;

            // determine how many entries can be displayed
            this.m_height = d_ViewportSize.Height - m_tableMarginTop - m_tableMarginBottom;
            float availableEntrySpace = m_height -
                (d_The3d.TextSize("0", m_fontSizeHeading).Height + 2 * m_padding
                 + d_The3d.TextSize("0", m_fontSizeSmall).Height + 2 * m_padding
                 + d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_padding
                 + d_The3d.TextSize("0", m_fontSizeSmall).Height + 2 * m_padding);

            int entriesPerPage = (int)(availableEntrySpace / (d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_entryPaddingTopBottom));
            this.pageCount = (int)Math.Ceiling((float)(ServerInfo.Players.Count / entriesPerPage));
            if (this.page > this.pageCount) this.page = 0;

            // 1. row - heading: Servername
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX, m_tableY, m_width, d_The3d.TextSize("0", m_fontSizeHeading).Height + 2 * m_padding, null, Color.DarkGreen);
            row1 = cutString(row1, m_fontSizeHeading, m_width - 2 * m_padding);
            d_The3d.Draw2dText(row1, m_tableX + xcenter(m_width, d_The3d.TextSize(row1, m_fontSizeHeading).Width), m_tableY + m_padding, m_fontSizeHeading, m_fontColor);
            heightOffset = d_The3d.TextSize("0", m_fontSizeHeading).Height + 2 * m_padding;

            // 2. row - server info: IP Motd Serverping
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX, m_tableY + heightOffset, m_width, d_The3d.TextSize("0", m_fontSizeSmall).Height + 2 * m_padding, null, Color.DarkSeaGreen);
            // row2_1 - IP align left
            d_The3d.Draw2dText(row2_1, m_tableX + m_padding, m_tableY + heightOffset + m_padding, m_fontSizeSmall, m_fontColor);
            // row2_2 - Motd align center
            row2_2 = cutString(row2_2, m_fontSizeSmall, m_width - 6 * m_padding - 2 * Math.Max(d_The3d.TextSize(row2_1, m_fontSizeSmall).Width, d_The3d.TextSize(row2_3, m_fontSizeSmall).Width));
            d_The3d.Draw2dText(row2_2, m_tableX + xcenter(m_width ,d_The3d.TextSize(row2_2, m_fontSizeSmall).Width), m_tableY + heightOffset + m_padding, m_fontSizeSmall, m_fontColor);
            // row2_3 - Serverping align right
            d_The3d.Draw2dText(row2_3, m_tableX + m_width - d_The3d.TextSize(row2_3, m_fontSizeSmall).Width - m_padding, m_tableY + heightOffset + m_padding, m_fontSizeSmall, m_fontColor);
            heightOffset += d_The3d.TextSize("0", m_fontSizeSmall).Height + 2 * m_padding;

            // 3.1. row infoline: Playercount, Page
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX, m_tableY + heightOffset, m_width, d_The3d.TextSize("0", m_fontSizeSmall).Height + 2 * m_padding, null, Color.DimGray);
            // row31_1 PlayerCount
            d_The3d.Draw2dText(row31_1, m_tableX + m_padding, m_tableY + heightOffset + m_padding, m_fontSizeSmall, m_fontColor);
            // row31_2 PlayerCount
            d_The3d.Draw2dText(row31_2, m_tableX + m_width - d_The3d.TextSize(row31_2, m_fontSizeSmall).Width - m_padding, m_tableY + heightOffset + m_padding, m_fontSizeSmall, m_fontColor);
            heightOffset += d_The3d.TextSize("0", m_fontSizeSmall).Height + 2 * m_padding;

            // 3. row playerlist heading: ID | Player | Ping
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX, m_tableY + heightOffset, m_idColumnWidth, d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_padding, null, Color.DarkGray);
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX + m_idColumnWidth, m_tableY + heightOffset, m_playerColumnWidth, d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_padding, null, Color.DarkGray);
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX + m_idColumnWidth + m_playerColumnWidth, m_tableY + heightOffset, m_pingColumnWidth, d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_padding, null, Color.DarkGray);
            // separation lines
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX + m_idColumnWidth, m_tableY + heightOffset, m_lineWidth, d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_padding, null, Color.DimGray);
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX + m_idColumnWidth + m_playerColumnWidth - m_lineWidth, m_tableY + heightOffset, m_lineWidth, d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_padding, null, Color.DimGray);
            // row3_1 ID
            d_The3d.Draw2dText(row3_1, m_tableX + m_idColumnWidth / 2 - d_The3d.TextSize(row3_1, m_fontSizeNormal).Width / 2, m_tableY + heightOffset + m_padding, m_fontSizeNormal, m_fontColor);
            // row3_2 Player
            d_The3d.Draw2dText(row3_2, m_tableX + m_idColumnWidth + m_playerColumnWidth / 2 - d_The3d.TextSize(row3_2, m_fontSizeNormal).Width / 2, m_tableY + heightOffset + m_padding, m_fontSizeNormal, m_fontColor);
            // row3_3 Ping
            d_The3d.Draw2dText(row3_3, m_tableX + m_idColumnWidth + m_playerColumnWidth + m_pingColumnWidth / 2 - d_The3d.TextSize(row3_3, m_fontSizeNormal).Width / 2, m_tableY + heightOffset + m_padding, m_fontSizeNormal, m_fontColor);
            heightOffset += d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_padding;

            // 4. row actual playerlist
            // entries:
            Color entryRowColor;
            for (int i = page * entriesPerPage; i < Math.Min(ServerInfo.Players.Count,page * entriesPerPage + entriesPerPage); i++)
            {
                if (i % 2 == 0)
                {
                    entryRowColor = Color.Gainsboro;
                }
                else
                {
                    entryRowColor = Color.Honeydew;
                }
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX, m_tableY + heightOffset, m_idColumnWidth, d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_entryPaddingTopBottom, null, entryRowColor);
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX + m_idColumnWidth, m_tableY + heightOffset, m_playerColumnWidth, d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_entryPaddingTopBottom, null, entryRowColor);
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX + m_idColumnWidth + m_playerColumnWidth, m_tableY + heightOffset, m_pingColumnWidth, d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_entryPaddingTopBottom, null, entryRowColor);

                // separation lines
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX + m_idColumnWidth, m_tableY + heightOffset, m_lineWidth, d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_entryPaddingTopBottom, null, Color.DimGray);
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_tableX + m_idColumnWidth + m_playerColumnWidth - m_lineWidth, m_tableY + heightOffset, m_lineWidth, d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_entryPaddingTopBottom, null, Color.DimGray);

                d_The3d.Draw2dText(this.ServerInfo.Players[i].id.ToString(), m_tableX + m_idColumnWidth - d_The3d.TextSize(this.ServerInfo.Players[i].id.ToString(), m_fontSizeNormal).Width - m_padding, m_tableY + heightOffset + m_entryPaddingTopBottom, m_fontSizeNormal, m_fontColor);
                d_The3d.Draw2dText(this.ServerInfo.Players[i].name, m_tableX + m_idColumnWidth + m_padding, m_tableY + heightOffset + m_entryPaddingTopBottom, m_fontSizeNormal, m_fontColor);
                d_The3d.Draw2dText(this.ServerInfo.Players[i].ping.ToString(), m_tableX + m_idColumnWidth + m_playerColumnWidth + m_pingColumnWidth - d_The3d.TextSize(this.ServerInfo.Players[i].ping.ToString(), m_fontSizeNormal).Width - m_padding, m_tableY + heightOffset + m_entryPaddingTopBottom, m_fontSizeNormal, m_fontColor);
                heightOffset += d_The3d.TextSize("0", m_fontSizeNormal).Height + 2 * m_entryPaddingTopBottom;
            }
        }

        public bool NextPage()
        {
            if (this.page < this.pageCount)
            {
                this.page ++;
                return true;
            }
            return false;
        }

        public bool PreviousPage()
        {
            if (this.page > 0)
            {
                this.page --;
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
        private string cutString(string inputString, float fontSize, float maxWidth)
        {
            while(d_The3d.TextSize(inputString, fontSize).Width > maxWidth && inputString.Length > 3)
            {
                inputString = inputString.Remove(inputString.Length-1);
            }
            return inputString;
        }
    }
}