public class ListWidget : AbstractMenuWidget
{
	ServerButtonWidget[] listButtons;
	ButtonWidget wbtn_pageUp;
	ButtonWidget wbtn_pageDown;
	TextWidget wtxt_pageNr;
	ListEntry[] listEntries;
	const int entriesMax = 1024;
	int entriesCount;
	int entriesPerPage;
	int currentPage;
	float lastSizeY;

	public ListWidget()
	{
		clickable = true;
		listButtons = null;
		listEntries = new ListEntry[entriesMax];
		entriesCount = 0;
		entriesPerPage = 0;
		currentPage = 0;
		lastSizeY = 0;

		wbtn_pageUp = new ButtonWidget();
		wbtn_pageUp.SetVisible(false);
		wbtn_pageDown = new ButtonWidget();
		wbtn_pageDown.SetVisible(false);
		FontCi fontPageNr = new FontCi();
		wtxt_pageNr = new TextWidget();
		wtxt_pageNr.SetAlignment(TextAlign.Right);
		wtxt_pageNr.SetBaseline(TextBaseline.Middle);
		wtxt_pageNr.SetFont(fontPageNr);
		wtxt_pageNr.SetVisible(false);
	}

	public override void Draw(float dt, UiRenderer renderer)
	{
		if (!visible) { return; }
		if (sizex <= 0 || sizey <= 0) { return; }

		const int padding = 6;
		const int elementSizeY = 64;
		float scale = renderer.GetScale();

		// calculation only needed if height changed
		if (lastSizeY != sizey)
		{
			entriesPerPage = renderer.GetPlatform().FloatToInt(sizey / ((elementSizeY + padding) * scale));
			listButtons = new ServerButtonWidget[entriesPerPage];
			for (int i = 0; i < entriesPerPage; i++)
			{
				ServerButtonWidget b = new ServerButtonWidget();
				b.SetVisible(false);
				listButtons[i] = b;
			}
			UpdateScrollButtons();
			lastSizeY = sizey;
		}

		// update list content
		for (int i = 0; i < entriesPerPage; i++)
		{
			listButtons[i].SetVisible(false);

			int index = i + (entriesPerPage * currentPage);
			if (index > entriesPerPage)
			{
				// Skip entries if list exceeds server limit
				continue;
			}
			ListEntry e = listEntries[index];
			if (e == null)
			{
				// Skip entries when reaching the end of the list
				continue;
			}

			listButtons[i].x = x + (elementSizeY / 2 + padding) * scale;
			listButtons[i].y = y + i * (elementSizeY + padding) * scale;
			listButtons[i].sizex = sizex - (1.5f * elementSizeY * scale) - (2 * padding * scale);
			listButtons[i].sizey = elementSizeY * scale;

			listButtons[i].SetTextHeading(e.textTopLeft);
			listButtons[i].SetTextDescription(e.textBottomLeft);
			listButtons[i].SetTextGamemode(e.textBottomRight);
			listButtons[i].SetTextPlayercount(e.textTopRight);
			listButtons[i].SetErrorConnect(e.imageStatusTop != null);
			listButtons[i].SetErrorVersion(e.imageStatusBottom != null);
			listButtons[i].SetThumbnail(e.imageMain);

			listButtons[i].SetVisible(true);
		}

		wbtn_pageUp.x = x + sizex - elementSizeY * scale;
		wbtn_pageUp.y = y + (entriesPerPage - 1) * (elementSizeY + padding) * scale;
		wbtn_pageUp.sizex = 64 * scale;
		wbtn_pageUp.sizey = 64 * scale;
		string texName = "serverlist_nav_down.png";
		wbtn_pageUp.SetTextureNames(texName, texName, texName);

		wbtn_pageDown.x = x + sizex - elementSizeY * scale;
		wbtn_pageDown.y = y;
		wbtn_pageDown.sizex = 64 * scale;
		wbtn_pageDown.sizey = 64 * scale;
		texName = "serverlist_nav_up.png";
		wbtn_pageDown.SetTextureNames(texName, texName, texName);

		wtxt_pageNr.SetText(renderer.GetPlatform().IntToString(currentPage + 1));
		wtxt_pageNr.x = x + sizex - (elementSizeY / 2) * scale;
		wtxt_pageNr.y = y + (sizey / 2);
		wtxt_pageNr.SetAlignment(TextAlign.Center);
		wtxt_pageNr.SetBaseline(TextBaseline.Middle);

		// draw child elements
		for (int i = 0; i < entriesPerPage; i++)
		{
			listButtons[i].Draw(dt, renderer);
		}
		wbtn_pageUp.Draw(dt, renderer);
		wbtn_pageDown.Draw(dt, renderer);
		wtxt_pageNr.Draw(dt, renderer);
	}
	public override void OnMouseDown(GamePlatform p, MouseEventArgs args)
	{
		if (!HasBeenClicked(args)) { return; }
		for (int i = 0; i < entriesPerPage; i++)
		{
			listButtons[i].OnMouseDown(p, args);
		}
		wbtn_pageUp.OnMouseDown(p, args);
		if (wbtn_pageUp.HasBeenClicked(args))
		{
			PageUp();
		}
		wbtn_pageDown.OnMouseDown(p, args);
		if (wbtn_pageDown.HasBeenClicked(args))
		{
			PageDown();
		}
	}
	public override void OnMouseWheel(GamePlatform p, MouseWheelEventArgs e)
	{
		if (e.GetDelta() < 0)
		{
			PageUp();
		}
		else if (e.GetDelta() > 0)
		{
			PageDown();
		}
	}

	public int GetPage()
	{
		return currentPage;
	}
	public bool IsLastPage()
	{
		// determine if page is the last containing servers
		return ((currentPage + 1) * entriesPerPage >= entriesMax) ||
			(listEntries[(currentPage + 1) * entriesPerPage] == null);
	}
	public void AddElement(ListEntry newEntry)
	{
		if (entriesCount < entriesMax)
		{
			listEntries[entriesCount] = newEntry;
			entriesCount++;
		}
		UpdateScrollButtons();
	}
	public ListEntry GetElement(int index)
	{
		if (index < 0 ||
			index >= entriesCount ||
			listEntries == null)
		{
			return null;
		}
		return listEntries[index];
	}
	public int GetEntriesCount()
	{
		return entriesCount;
	}
	public int GetEntriesPerPage()
	{
		return entriesPerPage;
	}
	public void Clear()
	{
		listEntries = new ListEntry[entriesMax];
		entriesCount = 0;
		UpdateScrollButtons();
	}
	public int GetIndexSelected()
	{
		for (int i = 0; i < entriesPerPage; i++)
		{
			if (listButtons[i].hasKeyboardFocus)
			{
				return i + currentPage * entriesPerPage;
			}
		}
		return -1;
	}

	void PageUp()
	{
		if (!IsLastPage())
		{
			currentPage++;
			UpdateScrollButtons();
		}
	}
	void PageDown()
	{
		if (currentPage > 0)
		{
			currentPage--;
			UpdateScrollButtons();
		}
	}
	void UpdateScrollButtons()
	{
		// hide scroll buttons
		wbtn_pageDown.SetVisible((currentPage == 0) ? false : true);
		wbtn_pageUp.SetVisible(IsLastPage() ? false : true);
	}
}

public class ListEntry
{
	public string textTopLeft;
	public string textTopRight;
	public string textBottomLeft;
	public string textBottomRight;
	public string imageMain;
	public string imageStatusTop;
	public string imageStatusBottom;

	public ListEntry()
	{
		textTopLeft = null;
		textTopRight = null;
		textBottomLeft = null;
		textBottomRight = null;
		imageMain = null;
		imageStatusTop = null;
		imageStatusBottom = null;
	}
}
