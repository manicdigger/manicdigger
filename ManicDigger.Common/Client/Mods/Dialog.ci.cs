public class ModDialog : ClientMod
{
	public ModDialog()
	{
		packetHandler = new ClientPacketHandlerDialog();
	}
	ClientPacketHandler packetHandler;

	public override void OnNewFrameDraw2d(Game game, float deltaTime)
	{
		game.packetHandlers[Packet_ServerIdEnum.Dialog] = packetHandler;
		DrawDialogs(game, deltaTime);
	}

	internal void DrawDialogs(Game game, float deltaTime)
	{
		for (int i = 0; i < game.dialogsCount; i++)
		{
			if (game.dialogs[i] == null)
			{
				continue;
			}
			VisibleDialog d = game.dialogs[i];
			d.screen.DrawWidgets(deltaTime);
		}
	}

	public override void OnKeyPress(Game game, KeyPressEventArgs args)
	{
		if (game.guistate != GuiState.ModalDialog
			&& game.guistate != GuiState.Normal)
		{
			return;
		}
		if (game.IsTyping)
		{
			// Do not handle key presses when chat is opened
			return;
		}
		for (int i = 0; i < game.dialogsCount; i++)
		{
			if (game.dialogs[i] == null) { continue; }
			game.dialogs[i].screen.OnKeyPress(game, args);
		}
		for (int k = 0; k < game.dialogsCount; k++)
		{
			if (game.dialogs[k] == null)
			{
				continue;
			}
			VisibleDialog d = game.dialogs[k];
			for (int i = 0; i < d.value.WidgetsCount; i++)
			{
				Packet_Widget w = d.value.Widgets[i];
				if (w == null)
				{
					continue;
				}
				// Only typeable characters are handled by KeyPress (for special characters use KeyDown)
				string valid = "abcdefghijklmnopqrstuvwxyz1234567890\t ";
				if (game.platform.StringContains(valid, game.CharToString(w.ClickKey)))
				{
					if (args.GetKeyChar() == w.ClickKey)
					{
						game.SendPacketClient(ClientPackets.DialogClick(w.Id, new string[0], 0));
						return;
					}
				}
			}
		}
	}

	public override void OnKeyDown(Game game, KeyEventArgs args)
	{
		for (int i = 0; i < game.dialogsCount; i++)
		{
			if (game.dialogs[i] == null) { continue; }
			game.dialogs[i].screen.OnKeyDown(game, args);
		}
		if (game.guistate == GuiState.Normal)
		{
			if (args.GetKeyCode() == game.GetKey(GlKeys.Escape))
			{
				for (int i = 0; i < game.dialogsCount; i++)
				{
					if (game.dialogs[i] == null)
					{
						continue;
					}
					VisibleDialog d = game.dialogs[i];
					if (d.value.IsModal != 0)
					{
						game.dialogs[i] = null;
						return;
					}
				}
				game.ShowEscapeMenu();
				args.SetHandled(true);
				return;
			}
		}
		if (game.guistate == GuiState.ModalDialog)
		{
			// Close modal dialogs when pressing ESC key
			if (args.GetKeyCode() == game.GetKey(GlKeys.Escape))
			{
				for (int i = 0; i < game.dialogsCount; i++)
				{
					if (game.dialogs[i] == null) { continue; }
					if (game.dialogs[i].value.IsModal != 0)
					{
						game.dialogs[i] = null;
					}
				}
				game.SendPacketClient(ClientPackets.DialogClick("Esc", new string[0], 0));
				game.GuiStateBackToGame();
				args.SetHandled(true);
			}
			// Handle TAB key
			if (args.GetKeyCode() == game.GetKey(GlKeys.Tab))
			{
				game.SendPacketClient(ClientPackets.DialogClick("Tab", new string[0], 0));
				args.SetHandled(true);
			}
			return;
		}
	}
	public override void OnKeyUp(Game game, KeyEventArgs args)
	{
		for (int i = 0; i < game.dialogsCount; i++)
		{
			if (game.dialogs[i] == null) { continue; }
			game.dialogs[i].screen.OnKeyUp(game, args);
		}
	}

	public override void OnMouseDown(Game game, MouseEventArgs args)
	{
		for (int i = 0; i < game.dialogsCount; i++)
		{
			if (game.dialogs[i] == null) { continue; }
			game.dialogs[i].screen.OnMouseDown(game, args);
		}
	}

	public override void OnMouseUp(Game game, MouseEventArgs args)
	{
		for (int i = 0; i < game.dialogsCount; i++)
		{
			if (game.dialogs[i] == null) { continue; }
			game.dialogs[i].screen.OnMouseUp(game, args);
		}
	}
}

public class ClientPacketHandlerDialog : ClientPacketHandler
{
	public override void Handle(Game game, Packet_Server packet)
	{
		Packet_ServerDialog d = packet.Dialog;
		if (d.Dialog == null)
		{
			if (game.GetDialogId(d.DialogId) != -1 && game.dialogs[game.GetDialogId(d.DialogId)].value.IsModal != 0)
			{
				game.GuiStateBackToGame();
			}
			if (game.GetDialogId(d.DialogId) != -1)
			{
				game.dialogs[game.GetDialogId(d.DialogId)] = null;
			}
			if (game.DialogsCount_() == 0)
			{
				game.SetFreeMouse(false);
			}
		}
		else
		{
			VisibleDialog d2 = new VisibleDialog();
			d2.key = d.DialogId;
			d2.value = d.Dialog;
			d2.screen = ConvertDialog(game, d2.value);
			d2.screen.game = game;
			if (game.GetDialogId(d.DialogId) == -1)
			{
				for (int i = 0; i < game.dialogsCount; i++)
				{
					if (game.dialogs[i] == null)
					{
						game.dialogs[i] = d2;
						break;
					}
				}
			}
			else
			{
				game.dialogs[game.GetDialogId(d.DialogId)] = d2;
			}
			if (d.Dialog.IsModal != 0)
			{
				game.guistate = GuiState.ModalDialog;
				game.SetFreeMouse(true);
			}
		}
	}

	GameScreen ConvertDialog(Game game, Packet_Dialog p)
	{
		GameScreen s = new GameScreen();
		s.Initialize(p.WidgetsCount);
		for (int i = 0; i < p.WidgetsCount; i++)
		{
			Packet_Widget a = p.Widgets[i];
			AbstractMenuWidget b = null;
			switch (a.Type)
			{
				case Packet_WidgetTypeEnum.ImageWdgt:
					ImageWidget newImg = new ImageWidget();
					if (a.Image == "Solid")
					{
						newImg.SetTextureName(a.Image);
					}
					else if (a.Image != null)
					{
						newImg.SetTextureName(StringTools.StringAppend(game.platform, a.Image, ".png"));
					}
					b = newImg;
					break;
				case Packet_WidgetTypeEnum.TextWdgt:
					TextWidget newTxt = new TextWidget();
					FontCi newFont = new FontCi();
					if (a.Font != null)
					{
						newFont.family = game.ValidFont(a.Font.FamilyName);
						newFont.size = game.DeserializeFloat(a.Font.SizeFloat);
						newFont.style = a.Font.FontStyle;
					}
					newTxt.SetFont(newFont);
					newTxt.SetAlignment(ConvertCi.IntToTextAlign(a.Font.GetAlignment()));
					newTxt.SetBaseline(ConvertCi.IntToTextBaseline(a.Font.GetBaseline()));
					string tmp = a.Text;
					if (tmp != null)
					{
						// dynamic string replacement
						tmp = game.platform.StringReplace(tmp, "!SERVER_IP!", game.ServerInfo.connectdata.Ip);
						tmp = game.platform.StringReplace(tmp, "!SERVER_PORT!", game.platform.IntToString(game.ServerInfo.connectdata.Port));
					}
					newTxt.SetText(tmp);
					b = newTxt;
					break;
				case Packet_WidgetTypeEnum.TextBoxWdgt:
					TextBoxWidget newTbx = new TextBoxWidget();
					newTbx.SetContent(game.platform, a.Text);
					b = newTbx;
					break;
				case Packet_WidgetTypeEnum.ButtonWdgt:
					ButtonWidget newBtn = new ButtonWidget();
					newBtn.SetText(a.Text);
					//newBtn.SetTextureNames();
					b = newBtn;
					break;
			}
			b.SetX(game.xcenter(p.GetWidth()) + a.X);
			b.SetY(game.ycenter(p.GetHeight()) + a.Y);
			b.SetSizeX(a.Width);
			b.SetSizeY(a.Height);
			b.SetColor(a.Color);

			if (!game.platform.StringEmpty(a.Id))
			{
				// set up "click" event handling only when widget has an id assigned
				b.SetEventName(a.Id);
				b.SetClickable(a.Click == 1);
				if (a.ClickKey != 0)
				{
					b.SetEventKeyChar(a.ClickKey);
				}
			}

			s.AddWidget(b);
		}
		return s;
	}
}
