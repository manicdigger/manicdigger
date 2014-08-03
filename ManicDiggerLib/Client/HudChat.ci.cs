public class HudChat
{
    public HudChat()
    {
        one = 1;
        ChatFontSize = 11;
        ChatScreenExpireTimeSeconds = 20;
        ChatLinesMaxToDraw = 10;
        font = new FontCi();
        font.family = "Arial";
        font.size = ChatFontSize;
        ChatLinesMax = 1;
        ChatLines = new Chatline[ChatLinesMax];
        chatlines2 = new Chatline[1024];
        ChatLineLength = 64;
    }

    internal Game game;
    internal bool IsTyping;
    internal string GuiTypingBuffer;
    internal float ChatFontSize;
    internal int ChatScreenExpireTimeSeconds;
    internal int ChatLinesMaxToDraw;
    internal Chatline[] ChatLines;
    internal int ChatLinesMax;
    internal int ChatLinesCount;
    internal int ChatPageScroll;
    internal bool IsTeamchat;
    internal int ChatLineLength;
    internal float one;

    public void Render()
    {
        DrawChatLines(IsTyping);
        if (IsTyping)
        {
            DrawTypingBuffer();
        }
    }
    public void AddChatline(string s)
    {
        if (game.platform.StringEmpty(s))
        {
            return;
        }
        //Check for links in chatline
        bool containsLink = false;
        string linkTarget = "";
        //Normal HTTP links
        if (game.platform.StringContains(s, "http://"))
        {
            containsLink = true;
            IntRef r = new IntRef();
            string[] temp = game.platform.StringSplit(s, " ", r);
            for (int i = 0; i < r.value; i++)
            {
                if (game.platform.StringIndexOf(temp[i], "http://") != -1)
                {
                    linkTarget = temp[i];
                    break;
                }
            }
        }
        //Secure HTTPS links
        if (game.platform.StringContains(s, "https://"))
        {
            containsLink = true;
            IntRef r = new IntRef();
            string[] temp = game.platform.StringSplit(s, " ", r);
            for (int i = 0; i < r.value; i++)
            {
                if (game.platform.StringIndexOf(temp[i], "https://") != -1)
                {
                    linkTarget = temp[i];
                    break;
                }
            }
        }
        int now = game.platform.TimeMillisecondsFromStart();
        //Display message in multiple lines if it's longer than one line
        if (s.Length > ChatLineLength)
        {
            for (int i = 0; i <= s.Length / ChatLineLength; i++)
            {
                int displayLength = ChatLineLength;
                if (s.Length - (i * ChatLineLength) < ChatLineLength)
                {
                    displayLength = s.Length - (i * ChatLineLength);
                }
                if (containsLink)
                    ChatLinesAdd(Chatline.CreateClickable(StringTools.StringSubstring(game.platform, s, i * ChatLineLength, displayLength), now, linkTarget));
                else
                    ChatLinesAdd(Chatline.Create(StringTools.StringSubstring(game.platform, s, i * ChatLineLength, displayLength), now));
            }
        }
        else
        {
            if (containsLink)
                ChatLinesAdd(Chatline.CreateClickable(s, now, linkTarget));
            else
                ChatLinesAdd(Chatline.Create(s, now));
        }
    }

    public void OnMouseDown(MouseEventArgs args)
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

    void ChatLinesAdd(Chatline chatline)
    {
        if (ChatLinesCount >= ChatLinesMax)
        {
            Chatline[] lines2 = new Chatline[ChatLinesMax * 2];
            for (int i = 0; i < ChatLinesMax; i++)
            {
                lines2[i] = ChatLines[i];
            }
            ChatLines = lines2;
            ChatLinesMax *= 2;
        }
        ChatLines[ChatLinesCount++] = chatline;
    }

    Chatline[] chatlines2;
    int chatlines2Count;
    public void DrawChatLines(bool all)
    {
        chatlines2Count = 0;
        if (!all)
        {
            for (int i = 0; i < ChatLinesCount; i++)
            {
                Chatline c = ChatLines[i];
                if ((one * (game.platform.TimeMillisecondsFromStart() - c.timeMilliseconds) / 1000) < ChatScreenExpireTimeSeconds)
                {
                    chatlines2[chatlines2Count++] = c;
                }
            }
        }
        else
        {
            int first = ChatLinesCount - ChatLinesMaxToDraw * (ChatPageScroll + 1);
            if (first < 0)
            {
                first = 0;
            }
            int count = ChatLinesCount;
            if (count > ChatLinesMaxToDraw)
            {
                count = ChatLinesMaxToDraw;
            }
            for (int i = first; i < first + count; i++)
            {
                chatlines2[chatlines2Count++] = ChatLines[i];
            }
        }
        font.size = ChatFontSize * game.Scale();
        float dx = 20;
        if (!game.platform.IsMousePointerLocked())
        {
            dx += 100;
        }
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
        string s = GuiTypingBuffer;
        if (IsTeamchat)
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
