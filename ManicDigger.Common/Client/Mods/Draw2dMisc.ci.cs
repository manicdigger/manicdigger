public class ModDraw2dMisc : ClientMod
{
	public override void OnNewFrameDraw2d(Game game, float deltaTime)
	{
		if (game.guistate == GuiState.Normal)
		{
			DrawAim(game);
		}
		if (game.guistate != GuiState.MapLoading)
		{
			DrawEnemyHealthBlock(game);
			DrawAmmo(game);
			DrawBlockInfo(game);
		}
		DrawMouseCursor(game);
		DrawDisconnected(game);
	}

	public void DrawBlockInfo(Game game)
	{
		if (!game.drawblockinfo)
		{
			return;
		}
		int x = game.SelectedBlockPositionX;
		int y = game.SelectedBlockPositionZ;
		int z = game.SelectedBlockPositionY;
		//string info = "None";
		if (!game.map.IsValidPos(x, y, z))
		{
			return;
		}
		int blocktype = game.map.GetBlock(x, y, z);
		if (!game.IsValid(blocktype))
		{
			return;
		}
		game.currentAttackedBlock = Vector3IntRef.Create(x, y, z);
		DrawEnemyHealthBlock(game);
	}

	internal void DrawMouseCursor(Game game)
	{
		if (!game.GetFreeMouse())
		{
			return;
		}
		if (!game.platform.MouseCursorIsVisible())
		{
			game.Draw2dBitmapFile("mousecursor.png", game.mouseCurrentX, game.mouseCurrentY, 32, 32);
		}
	}

	internal void DrawEnemyHealthBlock(Game game)
	{
		if (game.currentAttackedBlock != null)
		{
			int x = game.currentAttackedBlock.X;
			int y = game.currentAttackedBlock.Y;
			int z = game.currentAttackedBlock.Z;
			int blocktype = game.map.GetBlock(x, y, z);
			float health = game.GetCurrentBlockHealth(x, y, z);
			float progress = health / game.d_Data.Strength()[blocktype];
			if (game.IsUsableBlock(blocktype))
			{
				DrawEnemyHealthUseInfo(game, game.language.Get(StringTools.StringAppend(game.platform, "Block_", game.blocktypes[blocktype].Name)), progress, true);
			}
			DrawEnemyHealthCommon(game, game.language.Get(StringTools.StringAppend(game.platform, "Block_", game.blocktypes[blocktype].Name)), progress);
		}
		if (game.currentlyAttackedEntity != -1)
		{
			Entity e = game.entities[game.currentlyAttackedEntity];
			if (e == null)
			{
				return;
			}
			float health;
			if (e.playerStats != null)
			{
				health = game.one * e.playerStats.CurrentHealth / e.playerStats.MaxHealth;
			}
			else
			{
				health = 1;
			}
			string name = "Unknown";
			if (e.drawName != null)
			{
				name = e.drawName.Name;
			}
			if (e.usable)
			{
				DrawEnemyHealthUseInfo(game, game.language.Get(name), health, true);
			}
			DrawEnemyHealthCommon(game, game.language.Get(name), health);
		}
	}

	internal void DrawEnemyHealthCommon(Game game, string name, float progress)
	{
		DrawEnemyHealthUseInfo(game, name, 1, false);
	}

	internal void DrawEnemyHealthUseInfo(Game game, string name, float progress, bool useInfo)
	{
		int y = useInfo ? 55 : 35;
		game.Draw2dTexture(game.WhiteTexture(), game.xcenter(300), 40, 300, y, null, 0, ColorCi.FromArgb(255, 0, 0, 0), false);
		game.Draw2dTexture(game.WhiteTexture(), game.xcenter(300), 40, 300 * progress, y, null, 0, ColorCi.FromArgb(255, 255, 0, 0), false);
		FontCi font = new FontCi();
		font.size = 14;
		IntRef w = new IntRef();
		IntRef h = new IntRef();
		game.platform.TextSize(name, font, w, h);
		game.Draw2dText(name, font, game.xcenter(w.value), 40, null, false);
		if (useInfo)
		{
			name = game.platform.StringFormat(game.language.PressToUse(), "E");
			FontCi font2 = new FontCi();
			font2.size = 10;
			game.platform.TextSize(name, font2, w, h);
			game.Draw2dText(name, font2, game.xcenter(w.value), 70, null, false);
		}
	}

	internal void DrawAim(Game game)
	{
		if (game.cameratype == CameraType.Overhead)
		{
			return;
		}
		int aimwidth = 32;
		int aimheight = 32;
		game.platform.BindTexture2d(0);
		if (game.CurrentAimRadius() > 1)
		{
			float fov_ = game.currentfov();
			game.Circle3i(game.Width() / 2, game.Height() / 2, game.CurrentAimRadius() * game.fov / fov_);
		}
		game.Draw2dBitmapFile("target.png", game.Width() / 2 - aimwidth / 2, game.Height() / 2 - aimheight / 2, aimwidth, aimheight);
	}

	internal void DrawAmmo(Game game)
	{
		Packet_Item item = game.d_Inventory.RightHand[game.ActiveMaterial];
		if (item != null && item.ItemClass == Packet_ItemClassEnum.Block)
		{
			if (game.blocktypes[item.BlockId].IsPistol)
			{
				int loaded = game.LoadedAmmo[item.BlockId];
				int total = game.TotalAmmo[item.BlockId];
				string s = game.platform.StringFormat2("{0}/{1}", game.platform.IntToString(loaded), game.platform.IntToString(total - loaded));
				FontCi font = new FontCi();
				font.size = 18;
				game.Draw2dText(s, font, game.Width() - game.TextSizeWidth(s, font) - 50,
					game.Height() - game.TextSizeHeight(s, font) - 50, loaded == 0 ? IntRef.Create(ColorCi.FromArgb(255, 255, 0, 0)) : IntRef.Create(ColorCi.FromArgb(255, 255, 255, 255)), false);
				if (loaded == 0)
				{
					font.size = 14;
					string pressR = "Press R to reload";
					game.Draw2dText(pressR, font, game.Width() - game.TextSizeWidth(pressR, font) - 50,
						game.Height() - game.TextSizeHeight(s, font) - 80, IntRef.Create(ColorCi.FromArgb(255, 255, 0, 0)), false);
				}
			}
		}
	}

	void DrawDisconnected(Game game)
	{
		float one = 1;
		float lagSeconds = one * (game.platform.TimeMillisecondsFromStart() - game.LastReceivedMilliseconds) / 1000;
		if ((lagSeconds >= Game.DISCONNECTED_ICON_AFTER_SECONDS && lagSeconds < 60 * 60 * 24)
			&& game.invalidVersionDrawMessage == null && !(game.issingleplayer && (!game.platform.SinglePlayerServerLoaded())))
		{
			game.Draw2dBitmapFile("disconnected.png", game.Width() - 100, 50, 50, 50);
			FontCi font = new FontCi();
			font.size = 12;
			game.Draw2dText(game.platform.IntToString(game.platform.FloatToInt(lagSeconds)), font, game.Width() - 100, 50 + 50 + 10, null, false);
			game.Draw2dText("Press F6 to reconnect", font, game.Width() / 2 - 200 / 2, 50, null, false);
		}
	}
}
