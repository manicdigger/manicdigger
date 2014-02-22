using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Renderers;
using System.Drawing;

namespace ManicDigger.Gui
{
    public class HudChat
    {
        public HudChat()
        {
            one = 1;
        }

        public ManicDiggerGameWindow game;
        [Inject]
        public IViewportSize d_ViewportSize;

        public bool IsTyping;
        public string GuiTypingBuffer;
        public float ChatFontSize = 11f;
        public int ChatScreenExpireTimeSeconds = 20;
        public int ChatLinesMaxToDraw = 10;
        public List<Chatline> ChatLines = new List<Chatline>();
        public int ChatPageScroll;
        public bool IsTeamchat;
        public string Name { get { return "Chat"; } }

        public float one;

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
            int now = game.game.p.TimeMillisecondsFromStart();
            if (s.Length > 192)
            {
                ChatLines.Add(new Chatline() { text = s.Substring(0, 64), timeMilliseconds = now });
                ChatLines.Add(new Chatline() { text = s.Substring(64, 64), timeMilliseconds = now });
                ChatLines.Add(new Chatline() { text = s.Substring(128, 64), timeMilliseconds = now });
                ChatLines.Add(new Chatline() { text = s.Substring(192), timeMilliseconds = now });
            }
            else if (s.Length > 128)
            {
                ChatLines.Add(new Chatline() { text = s.Substring(0, 64), timeMilliseconds = now });
                ChatLines.Add(new Chatline() { text = s.Substring(64, 64), timeMilliseconds = now });
                ChatLines.Add(new Chatline() { text = s.Substring(128), timeMilliseconds = now });
            }
            else if (s.Length > 64)
            {
                ChatLines.Add(new Chatline() { text = s.Substring(0, 64), timeMilliseconds = now });
                ChatLines.Add(new Chatline() { text = s.Substring(64), timeMilliseconds = now });
            }
            else
            {
                ChatLines.Add(new Chatline() { text = s, timeMilliseconds = now });
            }
        }
        public void DrawChatLines(bool all)
        {
            /*
            if (chatlines.Count>0 && (DateTime.Now - chatlines[0].time).TotalSeconds > 10)
            {
                chatlines.RemoveAt(0);
            }
            */
            List<Chatline> chatlines2 = new List<Chatline>();
            if (!all)
            {
                foreach (Chatline c in ChatLines)
                {
                    if ((one * (game.game.p.TimeMillisecondsFromStart() - c.timeMilliseconds) / 1000) < ChatScreenExpireTimeSeconds)
                    {
                        chatlines2.Add(c);
                    }
                }
            }
            else
            {
                int first = ChatLines.Count - ChatLinesMaxToDraw * (ChatPageScroll + 1);
                if (first < 0)
                {
                    first = 0;
                }
                int count = ChatLines.Count;
                if (count > ChatLinesMaxToDraw)
                {
                    count = ChatLinesMaxToDraw;
                }
                for (int i = first; i < first + count; i++)
                {
                    chatlines2.Add(ChatLines[i]);
                }
            }
            for (int i = 0; i < chatlines2.Count; i++)
            {
                game.Draw2dText(chatlines2[i].text, 20, 90f + i * 25f, ChatFontSize, Color.White);
            }
            if (ChatPageScroll != 0)
            {
                game.Draw2dText("Page: " + ChatPageScroll, 20, 90f + (-1) * 25f, ChatFontSize, Color.Gray);
            }
        }
        public void DrawTypingBuffer()
        {
            string s = GuiTypingBuffer;
            if (IsTeamchat)
            {
                s = "To team: " + s;
            }
            game.Draw2dText(s + "_", 50, d_ViewportSize.Height - 100, ChatFontSize, Color.White);
        }
    }
    public class Chatline
    {
        public string text;
        public int timeMilliseconds;
    }
}
