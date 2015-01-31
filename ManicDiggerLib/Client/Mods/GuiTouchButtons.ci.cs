public class ModGuiTouchButtons : GameScreen
{
    public ModGuiTouchButtons()
    {
        touchButtonsEnabled = false;
        buttonMenu = new MenuWidget();
        buttonMenu.image = "TouchMenu.png";
        buttonInventory = new MenuWidget();
        buttonInventory.image = "TouchInventory.png";
        buttonTalk = new MenuWidget();
        buttonTalk.image = "TouchTalk.png";
        buttonCamera = new MenuWidget();
        buttonCamera.image = "TouchCamera.png";
        widgets[0] = buttonMenu;
        widgets[1] = buttonInventory;
        widgets[2] = buttonTalk;
        widgets[3] = buttonCamera;
        touchIdMove = -1;
        touchIdRotate = -1;
    }

    bool touchButtonsEnabled;
    MenuWidget buttonMenu;
    MenuWidget buttonInventory;
    MenuWidget buttonTalk;
    MenuWidget buttonCamera;

    public override void OnNewFrameDraw2d(Game game_, float deltaTime)
    {
        if (!touchButtonsEnabled)
        {
            return;
        }

        game = game_;
        float dt = deltaTime;
        int buttonSize = 80;

        if (game.guistate != GuiState.Normal)
        {
            return;
        }

        buttonMenu.x = 16 * Scale();
        buttonMenu.y = (16 + 96 * 0) * Scale();
        buttonMenu.sizex = buttonSize * Scale();
        buttonMenu.sizey = buttonSize * Scale();

        buttonInventory.x = 16 * Scale();
        buttonInventory.y = (16 + 96 * 1) * Scale();
        buttonInventory.sizex = buttonSize * Scale();
        buttonInventory.sizey = buttonSize * Scale();

        buttonTalk.x = 16 * Scale();
        buttonTalk.y = (16 + 96 * 2) * Scale();
        buttonTalk.sizex = buttonSize * Scale();
        buttonTalk.sizey = buttonSize * Scale();

        buttonCamera.x = 16 * Scale();
        buttonCamera.y = (16 + 96 * 3) * Scale();
        buttonCamera.sizex = buttonSize * Scale();
        buttonCamera.sizey = buttonSize * Scale();


        if (!game.platform.IsMousePointerLocked())
        {
            if (game.cameratype == CameraType.Fpp || game.cameratype == CameraType.Tpp)
            {
                game.Draw2dText1("Move", game.Width() * 5 / 100, game.Height() * 85 / 100, game.platform.FloatToInt(Scale() * 50), null, false);
                game.Draw2dText1("Look", game.Width() * 80 / 100, game.Height() * 85 / 100, game.platform.FloatToInt(Scale() * 50), null, false);
            }
            DrawWidgets();
        }
    }

    float Scale()
    {
        return game.Scale();
    }

    public override void OnButton(MenuWidget w)
    {
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

    int touchIdMove;
    int touchMoveStartX;
    int touchMoveStartY;
    int touchIdRotate;
    int touchRotateStartX;
    int touchRotateStartY;

    public override void OnTouchStart(Game game_, TouchEventArgs e)
    {
        touchButtonsEnabled = true;
        ScreenOnTouchStart(e);
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
        ScreenOnTouchEnd(e);
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
