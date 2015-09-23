using System;
using System.Collections.Generic;
using System.Drawing;

namespace ManicDigger.Mods.Fortress
{
	public class PlayerList : IMod
	{
		public void PreStart(ModManager m) { }
		
		public void Start(ModManager manager)
		{
			m = manager;
			m.RegisterOnSpecialKey(OnTabKey);
			m.RegisterOnDialogClick(OnTabResponse);
			m.RegisterTimer(UpdateTab, 1);
		}
		
		ModManager m;
		
		public string getPrefix(int playerID)
		{
			return "[" + m.GetGroupColor(playerID) + m.GetGroupName(playerID) + "&0] ";
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
				widgets.Add(Widget.MakeText(getPrefix(AllPlayers[i]) + m.GetPlayerName(AllPlayers[i]), NormalFont, tableX + tableIdColumnWidth + tablePadding, tableY + heightOffset + listEntryPaddingTopBottom, TEXT_COLOR.ToArgb()));
				string pingString;
				if (m.IsBot(AllPlayers[i]))
				{
					pingString = "BOT";
				}
				else
				{
					pingString = ((int)(m.GetPlayerPing(AllPlayers[i]) * 1000)).ToString();
				}
				widgets.Add(Widget.MakeText(pingString, NormalFont, tableX + tableIdColumnWidth + tablePlayerColumnWidth + tablePingColumnWidth - textWidth(pingString, NormalFont) - tablePadding, tableY + heightOffset + listEntryPaddingTopBottom, TEXT_COLOR.ToArgb()));
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
			foreach (var k in new Dictionary<string, bool>(tabOpen))
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
