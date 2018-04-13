public class ModGuiTouchButtons : GameScreen
{
	public ModGuiTouchButtons()
	{
		touchButtonsEnabled = false;
		touchIdMove = -1;
		touchIdRotate = -1;
		fontGuiTouchButtons = new FontCi();

		string texName = "TouchMenu.png";
		buttonMenu = new ButtonWidget();
		buttonMenu.SetTextureNames(texName, texName, texName);
		AddWidget(buttonMenu);
		texName = "TouchInventory.png";
		buttonInventory = new ButtonWidget();
		buttonInventory.SetTextureNames(texName, texName, texName);
		AddWidget(buttonInventory);
		texName = "TouchTalk.png";
		buttonTalk = new ButtonWidget();
		buttonTalk.SetTextureNames(texName, texName, texName);
		AddWidget(buttonTalk);
		texName = "TouchCamera.png";
		buttonCamera = new ButtonWidget();
		buttonCamera.SetTextureNames(texName, texName, texName);
		AddWidget(buttonCamera);
		textMove = new TextWidget();
		textMove.SetFont(fontGuiTouchButtons);
		textMove.SetText("Move");
		AddWidget(textMove);
		textLook = new TextWidget();
		textLook.SetFont(fontGuiTouchButtons);
		textLook.SetText("Look");
		AddWidget(textLook);
	}

	ButtonWidget buttonMenu;
	ButtonWidget buttonInventory;
	ButtonWidget buttonTalk;
	ButtonWidget buttonCamera;
	TextWidget textMove;
	TextWidget textLook;
	FontCi fontGuiTouchButtons;

	bool touchButtonsEnabled;
	int touchIdMove;
	int touchMoveStartX;
	int touchMoveStartY;
	int touchIdRotate;
	int touchRotateStartX;
	int touchRotateStartY;

	public override void OnNewFrameDraw2d(Game game_, float deltaTime)
	{
		if (!touchButtonsEnabled) { return; }

		game = game_;
		if (game.guistate != GuiState.Normal) { return; }

		float scale = game.Scale();
		int buttonSize = 80;

		buttonMenu.x = 16 * scale;
		buttonMenu.y = (16 + 96 * 0) * scale;
		buttonMenu.sizex = buttonSize * scale;
		buttonMenu.sizey = buttonSize * scale;

		buttonInventory.x = 16 * scale;
		buttonInventory.y = (16 + 96 * 1) * scale;
		buttonInventory.sizex = buttonSize * scale;
		buttonInventory.sizey = buttonSize * scale;

		buttonTalk.x = 16 * scale;
		buttonTalk.y = (16 + 96 * 2) * scale;
		buttonTalk.sizex = buttonSize * scale;
		buttonTalk.sizey = buttonSize * scale;

		buttonCamera.x = 16 * scale;
		buttonCamera.y = (16 + 96 * 3) * scale;
		buttonCamera.sizex = buttonSize * scale;
		buttonCamera.sizey = buttonSize * scale;

		fontGuiTouchButtons.size = scale * 50;
		textMove.x = game.Width() * 5 / 100;
		textMove.y = game.Height() * 85 / 100;
		textLook.x = game.Width() * 80 / 100;
		textLook.y = game.Height() * 85 / 100;

		if (!game.platform.IsMousePointerLocked())
		{
			if (game.cameratype == CameraType.Fpp || game.cameratype == CameraType.Tpp)
			{
				textMove.SetVisible(true);
				textLook.SetVisible(true);
			}
		}
		DrawWidgets(deltaTime);
	}

	public override void OnButton(AbstractMenuWidget w)
	{
		if (!touchButtonsEnabled) { return; }

		if (w == buttonMenu)
		{
			game.ShowEscapeMenu();
		}
		if (w == buttonInventory)
		{
			game.ShowInventory();
		}
		if (w == buttonTalk)
		{
			if (game.GuiTyping == TypingState.None)
			{
				game.StartTyping();
				game.platform.ShowKeyboard(true);
			}
			else
			{
				game.StopTyping();
				game.platform.ShowKeyboard(false);
			}
		}
		if (w == buttonCamera)
		{
			game.CameraChange();
		}
	}

	public override void OnTouchStart(Game game_, TouchEventArgs e)
	{
		touchButtonsEnabled = true;
		if (e.GetHandled()) { return; }
		if (e.GetX() <= game.Width() / 2)
		{
			if (touchIdMove == -1)
			{
				touchIdMove = e.GetId();
				touchMoveStartX = e.GetX();
				touchMoveStartY = e.GetY();
				game.touchMoveDx = 0;
				if (e.GetY() < game.Height() * 50 / 100)
				{
					game.touchMoveDy = 1;
				}
				else
				{
					game.touchMoveDy = 0;
				}
			}
		}
		if (((touchIdMove != -1)
			&& (e.GetId() != touchIdMove))
			|| (e.GetX() > game.Width() / 2))
		{
			if (touchIdRotate == -1)
			{
				touchIdRotate = e.GetId();
				touchRotateStartX = e.GetX();
				touchRotateStartY = e.GetY();
			}
		}
	}

	public override void OnTouchMove(Game game, TouchEventArgs e)
	{
		float one = 1;
		if (e.GetId() == touchIdMove)
		{
			float range = game.Width() * one / 20;
			game.touchMoveDx = e.GetX() - touchMoveStartX;
			game.touchMoveDy = -((e.GetY() - 1) - touchMoveStartY);
			float length = game.Length(game.touchMoveDx, game.touchMoveDy, 0);
			if (e.GetY() < game.Height() * 50 / 100)
			{
				game.touchMoveDx = 0;
				game.touchMoveDy = 1;
			}
			else
			{
				if (length > 0)
				{
					game.touchMoveDx /= length;
					game.touchMoveDy /= length;
				}
			}
		}
		if (e.GetId() == touchIdRotate)
		{
			game.touchOrientationDx += (e.GetX() - touchRotateStartX) / (game.Width() * one / 40);
			game.touchOrientationDy += (e.GetY() - touchRotateStartY) / (game.Width() * one / 40);
			touchRotateStartX = e.GetX();
			touchRotateStartY = e.GetY();
		}
	}

	public override void OnTouchEnd(Game game_, TouchEventArgs e)
	{
		if (e.GetHandled()) { return; }
		if (e.GetId() == touchIdMove)
		{
			touchIdMove = -1;
			game.touchMoveDx = 0;
			game.touchMoveDy = 0;
		}
		if (e.GetId() == touchIdRotate)
		{
			touchIdRotate = -1;
			game.touchOrientationDx = 0;
			game.touchOrientationDy = 0;
		}
	}
}
