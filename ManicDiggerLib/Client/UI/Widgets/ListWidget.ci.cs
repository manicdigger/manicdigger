class ListWidget : AbstractMenuWidget
{
	ServerButtonWidget[] listButtons;
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
	}

	public override void Draw(MainMenu m)
	{
		if (!visible) { return; }

		const int padding = 6;
		const int elementSizeY = 64;
		float scale = m.GetScale();

		// calculation only needed if height changed
		if (lastSizeY != sizey)
		{
			entriesPerPage = m.p.FloatToInt(sizey / ((elementSizeY + padding) * scale));
			listButtons = new ServerButtonWidget[entriesPerPage];
			for (int i = 0; i < entriesPerPage; i++)
			{
				ServerButtonWidget b = new ServerButtonWidget();
				b.SetVisible(false);
				listButtons[i] = b;
			}
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
			listButtons[i].sizex = sizex - (1.5f * elementSizeY) - (2 * padding);
			listButtons[i].sizey = elementSizeY;

			listButtons[i].SetTextHeading(e.textTopLeft);
			listButtons[i].SetTextDescription(e.textBottomLeft);
			listButtons[i].SetTextGamemode(e.textBottomRight);
			listButtons[i].SetTextPlayercount(e.textTopRight);
			listButtons[i].SetErrorConnect(e.imageStatusTop != null);
			listButtons[i].SetErrorVersion(e.imageStatusBottom != null);
			listButtons[i].SetServerImage(e.imageMain);

			listButtons[i].SetVisible(true);
		}

		// draw child elements
		for (int i = 0; i < entriesPerPage; i++)
		{
			listButtons[i].Draw(m);
		}
	}

	public override void OnMouseDown(GamePlatform p, MouseEventArgs args)
	{
		if (!HasBeenClicked(args)) { return; }
		for (int i = 0; i < entriesPerPage; i++)
		{
			listButtons[i].OnMouseDown(p, args);
		}
	}
	
	public void AddElement(ListEntry newEntry)
	{
		if (entriesCount < entriesMax)
		{
			listEntries[entriesCount] = newEntry;
			entriesCount++;
		}
	}

	public void Clear()
	{
		listEntries = new ListEntry[entriesMax];
		entriesCount = 0;
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
}

class ListEntry
{
	internal string textTopLeft;
	internal string textTopRight;
	internal string textBottomLeft;
	internal string textBottomRight;
	internal string imageMain;
	internal string imageStatusTop;
	internal string imageStatusBottom;

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
