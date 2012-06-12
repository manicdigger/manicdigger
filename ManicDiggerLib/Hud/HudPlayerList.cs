using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Renderers;
using System.Drawing;

namespace ManicDigger
{
    public class HudPlayerList
    {
        [Inject]
        public IViewportSize d_ViewportSize; // screen resoultion
        public ServerInformation ServerInfo;
        private The3d d_The3d;
        private int pageCount = 0; //number of pages for player table entries
        private int page = 0; // current displayed page

        // fonts
        public readonly Color TEXT_COLOR = Color.Black;
        public Font HeadingFont = new Font("Verdana", 11f, FontStyle.Bold);
        public Font NormalFont = new Font("Verdana", 10f);
        public Font NormalFontBold = new Font("Verdana", 10f, FontStyle.Bold);
        public Font SmallFont = new Font("Verdana", 8f);
        public Font SmallFontBold = new Font("Verdana", 8f, FontStyle.Bold);

        // playerlist (table) alignment and settings
        private float tableX;
        private float tableY; // (starting point (x,y) of table)

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

        public HudPlayerList()
        {
            d_The3d = new The3d
            {
                d_Config3d = new Config3d(),
                d_TextRenderer = new TextRenderer()
            };
        }

        public void DrawHudPlayerList()
        {
            // table alignment
            tableX = xcenter(d_ViewportSize.Width, tableWidth);
            tableY = tableMarginTop;

            // text to draw
            string row1 = ServerInfo.ServerName;
            row1 = cutText(row1, HeadingFont, tableWidth - 2 * tablePadding);

            string row2_1 = "IP: " + ServerInfo.ServerIp;
            string row2_2 = ServerInfo.ServerMotd;
            string row2_3 = ServerInfo.ServerPing.Milliseconds + "ms";
            row2_2 = cutText(row2_2, SmallFontBold, tableWidth - 6 * tablePadding - 2 * Math.Max(textWidth(row2_1, SmallFont), textWidth(row2_3, SmallFont)));

            string row3_1 = "Players: " + ServerInfo.Players.Count;
            string row3_2 = "Page: " + (page + 1) + "/" + (pageCount + 1);

            string row4_1 = "ID";
            string row4_2 = "Player";
            string row4_3 = "Ping";

            // row heights
            float row1Height = textHeight(row1, HeadingFont) + 2 * tablePadding;
            float row2Height = textHeight(row2_2, SmallFontBold) + 2 * tablePadding;
            float row3Height = textHeight(row3_1, SmallFont) + 2 * tablePadding;
            float row4Height = textHeight(row4_1, NormalFontBold) + 2 * tablePadding;
            float listEntryHeight = textHeight("Player", NormalFont) + 2 * listEntryPaddingTopBottom;

            float heightOffset = 0;

            // determine how many entries can be displayed
            tableHeight = d_ViewportSize.Height - tableMarginTop - tableMarginBottom;
            float availableEntrySpace = tableHeight - row1Height -row2Height - row3Height - row4Height;

            int entriesPerPage = (int)(availableEntrySpace / listEntryHeight);
            pageCount = (int)Math.Ceiling((float)(ServerInfo.Players.Count / entriesPerPage));
            if(page > pageCount)
            {
                page = 0;
            }

            // 1 - heading: Servername
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX, tableY, tableWidth, row1Height, null, Color.DarkGreen);
            d_The3d.Draw2dText(row1, HeadingFont, tableX + xcenter(tableWidth, textWidth(row1, HeadingFont)), tableY + tablePadding, TEXT_COLOR);
            heightOffset += row1Height;

            // 2 - server info: IP Motd Serverping
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX, tableY + heightOffset, tableWidth, row2Height, null, Color.DarkSeaGreen);
            // row2_1 - IP align left
            d_The3d.Draw2dText(row2_1, SmallFont, tableX + tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR);
            // row2_2 - Motd align center
            d_The3d.Draw2dText(row2_2, SmallFontBold, tableX + xcenter(tableWidth, textWidth(row2_2, SmallFont)), tableY + heightOffset + tablePadding, TEXT_COLOR);
            // row2_3 - Serverping align right
            d_The3d.Draw2dText(row2_3, SmallFont, tableX + tableWidth - textWidth(row2_3, SmallFont) - tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR);
            heightOffset += row2Height;

            // 3 - infoline: Playercount, Page
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX, tableY + heightOffset, tableWidth, row3Height, null, Color.DimGray);
            // row3_1 PlayerCount
            d_The3d.Draw2dText(row3_1, SmallFont, tableX + tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR);
            // row3_2 PlayerCount
            d_The3d.Draw2dText(row3_2, SmallFont, tableX + tableWidth - textWidth(row3_2, SmallFont) - tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR);
            heightOffset += row3Height;

            // 4 - playerlist heading: ID | Player | Ping
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX, tableY + heightOffset, tableIdColumnWidth, row4Height, null, Color.DarkGray);
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX + tableIdColumnWidth, tableY + heightOffset, tablePlayerColumnWidth, row4Height, null, Color.DarkGray);
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX + tableIdColumnWidth + tablePlayerColumnWidth, tableY + heightOffset, tablePingColumnWidth, row4Height, null, Color.DarkGray);
            // separation lines
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX + tableIdColumnWidth, tableY + heightOffset, tableLineWidth, row4Height, null, Color.DimGray);
            d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX + tableIdColumnWidth + tablePlayerColumnWidth - tableLineWidth, tableY + heightOffset, tableLineWidth, row4Height, null, Color.DimGray);
            // row4_1 ID - align center
            d_The3d.Draw2dText(row4_1, NormalFontBold, tableX + xcenter(tableIdColumnWidth, textWidth(row4_1, NormalFontBold)), tableY + heightOffset + tablePadding, TEXT_COLOR);
            // row4_2 Player - align center
            d_The3d.Draw2dText(row4_2, NormalFontBold, tableX + tableIdColumnWidth + tablePlayerColumnWidth / 2 - textWidth(row4_2, NormalFontBold) / 2, tableY + heightOffset + tablePadding, TEXT_COLOR);
            // row4_3 Ping - align center
            d_The3d.Draw2dText(row4_3, NormalFontBold, tableX + tableIdColumnWidth + tablePlayerColumnWidth + tablePingColumnWidth / 2 - textWidth(row4_3, NormalFontBold) / 2, tableY + heightOffset + tablePadding, TEXT_COLOR);
            heightOffset += row4Height;

            // 5 - actual playerlist
            // entries:
            Color entryRowColor;
            for(int i = page * entriesPerPage; i < Math.Min(ServerInfo.Players.Count, page * entriesPerPage + entriesPerPage); i++)
            {
                if(i % 2 == 0)
                {
                    entryRowColor = Color.Gainsboro;
                } else
                {
                    entryRowColor = Color.Honeydew;
                }
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX, tableY + heightOffset, tableIdColumnWidth, listEntryHeight, null, entryRowColor);
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX + tableIdColumnWidth, tableY + heightOffset, tablePlayerColumnWidth, listEntryHeight, null, entryRowColor);
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX + tableIdColumnWidth + tablePlayerColumnWidth, tableY + heightOffset, tablePingColumnWidth, listEntryHeight, null, entryRowColor);

                // separation lines
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX + tableIdColumnWidth, tableY + heightOffset, tableLineWidth, listEntryHeight, null, Color.DimGray);
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), tableX + tableIdColumnWidth + tablePlayerColumnWidth - tableLineWidth, tableY + heightOffset, tableLineWidth, listEntryHeight, null, Color.DimGray);

                d_The3d.Draw2dText(ServerInfo.Players[i].id.ToString(), NormalFont, tableX + tableIdColumnWidth - textWidth(ServerInfo.Players[i].id.ToString(), NormalFont) - tablePadding, tableY + heightOffset + listEntryPaddingTopBottom, TEXT_COLOR);
                d_The3d.Draw2dText(ServerInfo.Players[i].name, NormalFont, tableX + tableIdColumnWidth + tablePadding, tableY + heightOffset + listEntryPaddingTopBottom, TEXT_COLOR);
                d_The3d.Draw2dText(ServerInfo.Players[i].ping.ToString(), NormalFont, tableX + tableIdColumnWidth + tablePlayerColumnWidth + tablePingColumnWidth - textWidth(ServerInfo.Players[i].ping.ToString(), NormalFont) - tablePadding, tableY + heightOffset + listEntryPaddingTopBottom, TEXT_COLOR);
                heightOffset += listEntryHeight;
            }
        }

        public bool NextPage()
        {
            if(this.page < this.pageCount)
            {
                this.page ++;
                return true;
            }
            return false;
        }

        public bool PreviousPage()
        {
            if(this.page > 0)
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
        private float textWidth(string text, Font font)
        {
            return d_The3d.d_TextRenderer.MeasureTextSize(text, font).Width;
        }
        private float textHeight(string text, Font font)
        {
            return d_The3d.d_TextRenderer.MeasureTextSize(text, font).Height;
        }
        private string cutText(string text, Font font, float maxWidth)
        {
            while(textWidth(text, font) > maxWidth && text.Length > 3)
            {
                text = text.Remove(text.Length - 1);
            }
            return text;
        }
    }
}