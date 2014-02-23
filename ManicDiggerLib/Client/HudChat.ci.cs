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
        int now = game.p.TimeMillisecondsFromStart();
        if (s.Length > 192)
        {
            ChatLinesAdd(Chatline.Create(StringTools.StringSubstring(game.p, s, 0, 64), now));
            ChatLinesAdd(Chatline.Create(StringTools.StringSubstring(game.p, s, 64, 64), now));
            ChatLinesAdd(Chatline.Create(StringTools.StringSubstring(game.p, s, 128, 64), now));
            ChatLinesAdd(Chatline.Create(StringTools.StringSubstringToEnd(game.p, s, 192), now));
        }
        else if (s.Length > 128)
        {
            ChatLinesAdd(Chatline.Create(StringTools.StringSubstring(game.p, s, 0, 64), now));
            ChatLinesAdd(Chatline.Create(StringTools.StringSubstring(game.p, s, 64, 64), now));
            ChatLinesAdd(Chatline.Create(StringTools.StringSubstringToEnd(game.p, s, 128), now));
        }
        else if (s.Length > 64)
        {
            ChatLinesAdd(Chatline.Create(StringTools.StringSubstring(game.p, s, 0, 64), now));
            ChatLinesAdd(Chatline.Create(StringTools.StringSubstringToEnd(game.p, s, 64), now));
        }
        else
        {
            ChatLinesAdd(Chatline.Create(s, now));
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

    public void DrawChatLines(bool all)
    {
        Chatline[] chatlines2 = new Chatline[1024];
        int chatlines2Count = 0;
        if (!all)
        {
            for (int i = 0; i < ChatLinesCount; i++)
            {
                Chatline c = ChatLines[i];
                if ((one * (game.p.TimeMillisecondsFromStart() - c.timeMilliseconds) / 1000) < ChatScreenExpireTimeSeconds)
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
        for (int i = 0; i < chatlines2Count; i++)
        {
            game.Draw2dText(chatlines2[i].text, font, 20, 90 + i * 25, null, false);
        }
        if (ChatPageScroll != 0)
        {
            game.Draw2dText(game.p.StringFormat("&7Page: {0}", game.p.IntToString(ChatPageScroll)), font, 20, 90 + (-1) * 25, null, false);
        }
    }
    FontCi font;
    public void DrawTypingBuffer()
    {
        string s = GuiTypingBuffer;
        if (IsTeamchat)
        {
            s = game.p.StringFormat("To team: {0}", s);
        }
        game.Draw2dText(game.p.StringFormat("{0}_", s), font, 50, game.p.GetCanvasHeight() - 100, null, true);
    }
}

public class Chatline
{
    internal string text;
    internal int timeMilliseconds;

    internal static Chatline Create(string text_, int timeMilliseconds_)
    {
        Chatline c = new Chatline();
        c.text = text_;
        c.timeMilliseconds = timeMilliseconds_;
        return c;
    }
}
