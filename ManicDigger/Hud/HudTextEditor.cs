using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using ManicDigger.Renderers;
using OpenTK;
using OpenTK.Input;

namespace ManicDigger.Hud
{
   public class HudTextEditor
   {

      public HudTextEditor()
      {
         d_The3d = new The3d
         {
            d_Config3d = new Config3d(),
            d_TextRenderer = new MonospacedTextRenderer()
         };
         CHARACTER_WIDTH = d_The3d.d_TextRenderer.MeasureTextSize(new string('M', 100), FONTSIZE).Width / 100f; // <--- measuring 100 gives a better result than measuring a single char
         CHARACTER_HEIGHT = d_The3d.d_TextRenderer.MeasureTextSize("gG", FONTSIZE).Height;
      }

      The3d d_The3d;

      public const float FONTSIZE = 9f;
      public const float LINESPACING = 4f;
      public readonly Color CURSOR_COLOR = Color.Black;
      public readonly Color TEXT_COLOR = Color.Black;
      public const float MARGIN_RIGHT = 20;
      public const float MARGIN_TOP = 20;
      public float CHARACTER_WIDTH, CHARACTER_HEIGHT;
      public float CURSOR_OFFSET_X = 1.0f;

      [Inject]
      public IViewportSize d_ViewportSize;

      private TextEditor m_editor = new TextEditor();

      public void SetText(string text)
      {
         m_editor.Text = text;
      }

      public string GetText()
      {
         return m_editor.Text;
      }

      public void Render()
      {
         if (d_The3d != null)
         {
            DrawEditorBackground();
            DrawVisibleLines();
            DrawCursor();
         }
      }

      private void DrawCursor()
      {
         var cursor_x = MARGIN_RIGHT + m_editor.CursorCol * CHARACTER_WIDTH + CURSOR_OFFSET_X;
         var cursor_y = MARGIN_TOP + m_editor.CursorRow * (FONTSIZE + LINESPACING) - LINESPACING / 2;
         d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), cursor_x, cursor_y, 1, FONTSIZE * 2, null, CURSOR_COLOR);
      }

      private void DrawEditorBackground()
      {
         d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), 0, 0, d_ViewportSize.Width, d_ViewportSize.Height, null, new FastColor(200, 250, 250, 250).ToColor());
      }

      private void DrawVisibleLines()
      {
         float y = 0;
         foreach (var line in m_editor.Lines)
         {
            d_The3d.Draw2dText(line.Text, MARGIN_RIGHT, MARGIN_TOP + y, FONTSIZE, TEXT_COLOR);
            y += FONTSIZE + LINESPACING;
         }
      }

      public void HandleKeyDown(object sender, KeyboardKeyEventArgs e)
      {
         switch (e.Key)
         {
            case Key.Up:
               m_editor.MoveCursorUpDown(-1);
               break;
            case Key.Down:
               m_editor.MoveCursorUpDown(1);
               break;
            case Key.Right:
               m_editor.MoveCursorLeftRight(1);
               break;
            case Key.Left:
               m_editor.MoveCursorLeftRight(-1);
               break;
            case Key.BackSpace:
               m_editor.BackSpace();
               break;
            case Key.Delete:
               m_editor.Delete();
               break;
            case Key.Home:
               m_editor.SetCursor(0, m_editor.CursorRow);
               break;
            case Key.End:
               m_editor.GoToEndOfLine();
               break;
         }
      }

      public void HandleKeyPress(object sender, KeyPressEventArgs e)
      {
         char c = e.KeyChar;
         if (c == '\r' || c == '\n')
         {
            m_editor.InsertCarriageReturn();
            return;
         }
         if (c == '\t')
         {
            m_editor.InsertTab();
            return;
         }
         if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c))
         {
            m_editor.Insert(e.KeyChar.ToString());
            return;
         }
      }
   }

   //Text editor model without any GUI dependencies
   public class TextEditor
   {
      public const string TAB = "    ";

      List<TextLine> m_lines = new List<TextLine>() { new TextLine("Hello World!"), new TextLine("bla bla bla") };
      public IEnumerable<TextLine> Lines { get { return m_lines; } }

      public int CursorRow = 0;
      public int CursorCol = 0;

      public string Text
      {
         set
         {
            m_lines.Clear();
            if (value == null)
               return;
            foreach (var line in value.Split('\r', '\n'))
               m_lines.Add(new TextLine(line));
         }
         get
         {
            var text = new StringBuilder();
            foreach (var line in m_lines)
               text.AppendLine(line.Text);
            return text.ToString();
         }
      }

      public void MoveCursorLeftRight(int delta_col)
      {
         CursorCol += delta_col;
         if (CursorCol < 0) // jump to end of previous row
         {
            if (CursorRow > 0)
            {
               CursorRow -= 1;
               CursorCol = GetLineLength(CursorRow);
            }
            else
               CursorCol = 0;
         }
         else if (CursorCol > GetLineLength(CursorRow)) // jump to start of next row
         {
            CursorRow += 1;
            CursorCol = GetLineLength(CursorRow);
         }
      }

      public void MoveCursorUpDown(int delta_row)
      {
         CursorRow += delta_row;
         if (CursorRow < 0)
            CursorRow = 0;
         if (GetLineLength(CursorRow) < CursorCol)
            CursorCol = GetLineLength(CursorRow); // reset to end of line because cursor is beyond the end of line (no "virtual space")
      }

      private int GetLineLength(int row)
      {
         if (row < 0)
            return 0;
         if (m_lines.Count <= row)
            return 0;
         return m_lines[row].Length;
      }

      public void Insert(string s)
      {
         var line = GetLine(CursorRow);
         line.Insert(CursorCol, s);
         CursorCol += s.Length;
      }

      public void InsertCarriageReturn()
      {
         var line = GetLine(CursorRow);
         string removed = "";
         if (line.Length > 0 && CursorCol < line.Length)
         {
            removed = line.GetSegment(CursorCol, line.Length);
            line.Remove(CursorCol, line.Length);
         }
         CursorRow += 1;
         if (CursorRow > m_lines.Count)
            GetLine(CursorRow); // make sure empty lines are generated up to the insertion row index
         m_lines.Insert(CursorRow, new TextLine(removed));
         CursorCol = 0;
      }

      private TextLine GetLine(int row)
      {
         if (row < 0)
            row = 0;
         if (m_lines.Count <= row)
         {
            for (int i = m_lines.Count; i <= row; i++)
               m_lines.Add(new TextLine(""));
         }
         var line = m_lines[row] ?? (m_lines[row] = new TextLine(""));
         return line;
      }

      public void InsertTab()
      {
         Insert(TAB);
      }

      public void BackSpace()
      {
         if (CursorCol == 0 && CursorRow == 0)
            return;
         var line = GetLine(CursorRow);
         if (CursorCol > 0)
         {
            line.Remove(CursorCol - 1, CursorCol);
            CursorCol -= 1;
         }
         else
         {
            m_lines.RemoveAt(CursorRow);
            CursorRow -= 1;
            var line_above = GetLine(CursorRow);
            var length = line_above.Length;
            line_above.Append(line.Text);
            CursorCol = length;
         }
      }

      public void Delete()
      {
         var line = GetLine(CursorRow);
         if (CursorRow >= m_lines.Count && CursorCol <=  line.Length)
            return;
         if (CursorCol < line.Length)
         {
            line.Remove(CursorCol, CursorCol+1);
         }
         else
         {
            var line_below = GetLine(CursorRow+1);
            m_lines.RemoveAt(CursorRow + 1);
            line.Append(line_below.Text);
         }
      }

      public void SetCursor(int col, int row)
      {
         CursorCol=col;
         CursorRow=row;
         CheckCursorPosition();
         // TODO: scroll
      }

      private void CheckCursorPosition()
      {
         if (CursorCol < 0)
            CursorCol = 0;
         if (CursorRow < 0)
            CursorRow = 0;
         CursorCol = Math.Min(CursorCol, GetLine(CursorRow).Length);
      }

      public void GoToEndOfLine()
      {
         CursorCol = GetLine(CursorRow).Length;
         // TODO: scroll
      }
   }

   public class TextLine
   {
      public TextLine(string text)
      {
         m_text = new StringBuilder(text);
      }

      StringBuilder m_text;
      public string Text
      {
         get { return m_text.ToString(); }
         set
         {
            m_text.Remove(0, m_text.Length);
            m_text.Append(value);
         }
      }

      public int Length
      {
         get { return m_text.Length; }
      }

      public void Insert(int col, string s)
      {
         m_text.Insert(col, s);
      }

      public string GetSegment(int col_start, int col_end)
      {
         Debug.Assert(col_end >= col_start);
         col_end = Math.Min(m_text.Length, col_end);
         if (col_start == col_end || col_start > m_text.Length)
            return "";
         var s = new StringBuilder(col_end - col_start);
         for (int i = col_start; i < col_end; i++)
         {
            s.Append(m_text[i]);
         }
         return s.ToString();
      }

      public void Remove(int col_start, int col_end)
      {
         Debug.Assert(col_end >= col_start);
         if (col_start == col_end)
            return;
         m_text.Remove(col_start, col_end - col_start);
      }

      public void Append(string text)
      {
         m_text.Append(text);
      }
   }
}
