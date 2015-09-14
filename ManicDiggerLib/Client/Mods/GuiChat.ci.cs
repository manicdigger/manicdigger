public class ModGuiChat : ClientMod
{
    public ModGuiChat()
    {
        one = 1;
        ChatFontSize = 11;
        ChatScreenExpireTimeSeconds = 20;
        ChatLinesMaxToDraw = 10;
        font = new FontCi();
        font.family = "Arial";
        font.size = ChatFontSize;
        chatlines2 = new Chatline[1024];
    }

    internal Game game;
    internal float ChatFontSize;
    internal int ChatScreenExpireTimeSeconds;
    internal int ChatLinesMaxToDraw;
    internal int ChatPageScroll;
    internal float one;

    public override void OnNewFrameDraw2d(Game game_, float deltaTime)
    {
        game = game_;
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }
        DrawChatLines(game.GuiTyping == TypingState.Typing);
        if (game.GuiTyping == TypingState.Typing)
        {
            DrawTypingBuffer();
        }
    }

    public override void OnMouseDown(Game game_, MouseEventArgs args)
    {
        for (int i = 0; i < chatlines2Count; i++)
        {
            float dx = 20;
            if (!game.platform.IsMousePointerLocked())
            {
                dx += 100;
            }
            float chatlineStartX = dx * game.Scale();
            float chatlineStartY = (90 + i * 25) * game.Scale();
            float chatlineSizeX = 500 * game.Scale();
            float chatlineSizeY = 20 * game.Scale();
            if (args.GetX() > chatlineStartX && args.GetX() < chatlineStartX + chatlineSizeX)
            {
                if (args.GetY() > chatlineStartY && args.GetY() < chatlineStartY + chatlineSizeY)
                {
                    //Mouse over chatline at position i
                    if (chatlines2[i].clickable)
                    {
                        game.platform.OpenLinkInBrowser(chatlines2[i].linkTarget);
                    }
                }
            }
        }
    }

    Chatline[] chatlines2;
    int chatlines2Count;
    public void DrawChatLines(bool all)
    {
        chatlines2Count = 0;
        int timeNow = game.platform.TimeMillisecondsFromStart();
        int scroll;
        if (!all)
        {
            scroll = 0;
        }
        else
        {
            scroll = ChatPageScroll;
        }
        int first = game.ChatLinesCount - ChatLinesMaxToDraw * (scroll + 1);
        if (first < 0)
        {
            first = 0;
        }
        int count = game.ChatLinesCount;
        if (count > ChatLinesMaxToDraw)
        {
            count = ChatLinesMaxToDraw;
        }
        for (int i = first; i < first + count; i++)
        {
            Chatline c = game.ChatLines[i];
            if (all || ((one * (timeNow - c.timeMilliseconds) / 1000) < ChatScreenExpireTimeSeconds))
            {
                chatlines2[chatlines2Count++] = c;
            }
        }
        font.size = ChatFontSize * game.Scale();
        float dx = 20;
        //if (!game.platform.IsMousePointerLocked())
        //{
        //    dx += 100;
        //}
        for (int i = 0; i < chatlines2Count; i++)
        {
            if (chatlines2[i].clickable)
            {
                //Different display of links in chat
                //2 = italic
                //3 = bold italic
                font.style = 3;
            }
            else
            {
                //0 = normal
                //1 = bold
                font.style = 1;
            }
            game.Draw2dText(chatlines2[i].text, font, dx * game.Scale(), (90 + i * 25) * game.Scale(), null, false);
        }
        if (ChatPageScroll != 0)
        {
            game.Draw2dText(game.platform.StringFormat("&7Page: {0}", game.platform.IntToString(ChatPageScroll)), font, dx * game.Scale(), (90 + (-1) * 25) * game.Scale(), null, false);
        }
    }
    FontCi font;
    public void DrawTypingBuffer()
    {
        font.size = ChatFontSize * game.Scale();
        string s = game.GuiTypingBuffer;
        if (game.IsTeamchat)
        {
            s = game.platform.StringFormat("To team: {0}", s);
        }
        if (game.platform.IsSmallScreen())
        {
            game.Draw2dText(game.platform.StringFormat("{0}_", s), font, 50 * game.Scale(), (game.platform.GetCanvasHeight() / 2) - 100 * game.Scale(), null, true);
        }
        else
        {
            game.Draw2dText(game.platform.StringFormat("{0}_", s), font, 50 * game.Scale(), game.platform.GetCanvasHeight() - 100 * game.Scale(), null, true);
        }
    }

    public override void OnKeyDown(Game game_, KeyEventArgs args)
    {
        if (game.guistate != GuiState.Normal)
        {
            //Don't open chat when not in normal game
            return;
        }
        int eKey = args.GetKeyCode();
        if (eKey == game.GetKey(GlKeys.Number7) && game.IsShiftPressed && game.GuiTyping == TypingState.None) // don't need to hit enter for typing commands starting with slash
        {
            game.GuiTyping = TypingState.Typing;
            game.IsTyping = true;
            game.GuiTypingBuffer = "";
            game.IsTeamchat = false;
            args.SetHandled(true);
            return;
        }
        if (eKey == game.GetKey(GlKeys.PageUp) && game.GuiTyping == TypingState.Typing)
        {
            ChatPageScroll++;
            args.SetHandled(true);
        }
        if (eKey == game.GetKey(GlKeys.PageDown) && game.GuiTyping == TypingState.Typing)
        {
            ChatPageScroll--;
            args.SetHandled(true);
        }
        ChatPageScroll = MathCi.ClampInt(ChatPageScroll, 0, game.ChatLinesCount / ChatLinesMaxToDraw);
        if (eKey == game.GetKey(GlKeys.Enter) || eKey == game.GetKey(GlKeys.KeypadEnter))
        {
            if (game.GuiTyping == TypingState.Typing)
            {
                game.typinglog[game.typinglogCount++] = game.GuiTypingBuffer;
                game.typinglogpos = game.typinglogCount;
                game.ClientCommand(game.GuiTypingBuffer);

                game.GuiTypingBuffer = "";
                game.IsTyping = false;

                game.GuiTyping = TypingState.None;
                game.platform.ShowKeyboard(false);
            }
            else if (game.GuiTyping == TypingState.None)
            {
                game.StartTyping();
            }
            else if (game.GuiTyping == TypingState.Ready)
            {
                game.platform.ConsoleWriteLine("Keyboard_KeyDown ready");
            }
            args.SetHandled(true);
            return;
        }
        if (game.GuiTyping == TypingState.Typing)
        {
            int key = eKey;
            if (key == game.GetKey(GlKeys.BackSpace))
            {
                if (StringTools.StringLength(game.platform, game.GuiTypingBuffer) > 0)
                {
                    game.GuiTypingBuffer = StringTools.StringSubstring(game.platform, game.GuiTypingBuffer, 0, StringTools.StringLength(game.platform, game.GuiTypingBuffer) - 1);
                }
                args.SetHandled(true);
                return;
            }
            if (game.keyboardStateRaw[game.GetKey(GlKeys.ControlLeft)] || game.keyboardStateRaw[game.GetKey(GlKeys.ControlRight)])
            {
                if (key == game.GetKey(GlKeys.V))
                {
                    if (game.platform.ClipboardContainsText())
                    {
                        game.GuiTypingBuffer = StringTools.StringAppend(game.platform, game.GuiTypingBuffer, game.platform.ClipboardGetText());
                    }
                    args.SetHandled(true);
                    return;
                }
            }
            if (key == game.GetKey(GlKeys.Up))
            {
                game.typinglogpos--;
                if (game.typinglogpos < 0) { game.typinglogpos = 0; }
                if (game.typinglogpos >= 0 && game.typinglogpos < game.typinglogCount)
                {
                    game.GuiTypingBuffer = game.typinglog[game.typinglogpos];
                }
                args.SetHandled(true);
            }
            if (key == game.GetKey(GlKeys.Down))
            {
                game.typinglogpos++;
                if (game.typinglogpos > game.typinglogCount) { game.typinglogpos = game.typinglogCount; }
                if (game.typinglogpos >= 0 && game.typinglogpos < game.typinglogCount)
                {
                    game.GuiTypingBuffer = game.typinglog[game.typinglogpos];
                }
                if (game.typinglogpos == game.typinglogCount)
                {
                    game.GuiTypingBuffer = "";
                }
                args.SetHandled(true);
            }
            //Handles player name autocomplete in chat
            if (eKey == game.GetKey(GlKeys.Tab) && game.platform.StringTrim(game.GuiTypingBuffer) != "")
            {
                IntRef partsLength = new IntRef();
                string[] parts = game.platform.StringSplit(game.GuiTypingBuffer, " ", partsLength);
                string completed = DoAutocomplete(parts[partsLength.value - 1]);
                if (completed == "")
                {
                    //No completion available. Abort.
                    args.SetHandled(true);
                    return;
                }
                else if (partsLength.value == 1)
                {
                    //Part is first word. Format as "<name>: "
                    game.GuiTypingBuffer = StringTools.StringAppend(game.platform, completed, ": ");
                }
                else
                {
                    //Part is not first. Just complete "<name> "
                    parts[partsLength.value - 1] = completed;
                    game.GuiTypingBuffer = StringTools.StringAppend(game.platform, game.platform.StringJoin(parts, " "), " ");
                }
                args.SetHandled(true);
                return;
            }
            args.SetHandled(true);
            return;
        }
    }

    public override void OnKeyPress(Game game_, KeyPressEventArgs args)
    {
        if (game.guistate != GuiState.Normal)
        {
            //Don't open chat when not in normal game
            return;
        }
        int eKeyChar = args.GetKeyChar();
        int chart = 116;
        int charT = 84;
        int chary = 121;
        int charY = 89;
        if ((eKeyChar == chart || eKeyChar == charT) && game.GuiTyping == TypingState.None)
        {
            game.GuiTyping = TypingState.Typing;
            game.GuiTypingBuffer = "";
            game.IsTeamchat = false;
            return;
        }
        if ((eKeyChar == chary || eKeyChar == charY) && game.GuiTyping == TypingState.None)
        {
            game.GuiTyping = TypingState.Typing;
            game.GuiTypingBuffer = "";
            game.IsTeamchat = true;
            return;
        }
        if (game.GuiTyping == TypingState.Typing)
        {
            int c = eKeyChar;
            if (game.platform.IsValidTypingChar(c))
            {
                game.GuiTypingBuffer = StringTools.StringAppend(game.platform, game.GuiTypingBuffer, game.CharToString(c));
            }
        }
    }

    public string DoAutocomplete(string text)
    {
        if (!game.platform.StringEmpty(text))
        {
            for (int i = 0; i < game.entitiesCount; i++)
            {
                Entity entity = game.entities[i];
                if (entity == null) { continue; }
                if (entity.drawName == null) { continue; }
                if (!entity.drawName.ClientAutoComplete) { continue; }
                DrawName p = entity.drawName;
                //Use substring here because player names are internally in format &xNAME (so we need to cut first 2 characters)
                if (game.platform.StringStartsWithIgnoreCase(StringTools.StringSubstringToEnd(game.platform, p.Name, 2), text))
                {
                    return StringTools.StringSubstringToEnd(game.platform, p.Name, 2);
                }
            }
        }
        return "";
    }
}

public class Chatline
{
    internal string text;
    internal int timeMilliseconds;
    internal bool clickable;
    internal string linkTarget;

    internal static Chatline Create(string text_, int timeMilliseconds_)
    {
        Chatline c = new Chatline();
        c.text = text_;
        c.timeMilliseconds = timeMilliseconds_;
        c.clickable = false;
        return c;
    }

    internal static Chatline CreateClickable(string text_, int timeMilliseconds_, string linkTarget_)
    {
        Chatline c = new Chatline();
        c.text = text_;
        c.timeMilliseconds = timeMilliseconds_;
        c.clickable = true;
        c.linkTarget = linkTarget_;
        return c;
    }
}
