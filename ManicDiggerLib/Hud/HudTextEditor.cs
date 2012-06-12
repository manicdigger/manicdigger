// Copyright (c) 2011-2012 by Henon <meinrad.recheis@gmail.com>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
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
         UpdateEditorViewport();
      }

      public const float FONTSIZE = 9f;
      public const float LINESPACING = 4f;
      public readonly Color CURSOR_COLOR = Color.Black;
      public readonly Color TEXT_COLOR = Color.Black;
      public readonly Color SELECTION_COLOR = Color.LightGray;
      public float CHARACTER_WIDTH, CHARACTER_HEIGHT;
      public float CURSOR_OFFSET_X = 1.0f;

      [Inject]
      public IViewportSize d_ViewportSize;

      private TextEditor m_editor = new TextEditor();

      The3d d_The3d;

      private float m_left = 100;
      private float m_top = 100;
      private float m_padding = 10;
      private float m_width = 500;
      private float m_height = 300;

      public float Width
      {
         get { return m_width; }
         set
         {
            m_width = value;
            UpdateEditorViewport();
         }
      }

      public float Height
      {
         get { return m_height; }
         set
         {
            m_height = value;
            UpdateEditorViewport();
         }
      }

      public float Left
      {
         get { return m_left; }
         set
         {
            m_left = value;
            UpdateEditorViewport();
         }
      }

      public float Top
      {
         get { return m_top; }
         set
         {
            m_top = value;
            UpdateEditorViewport();
         }
      }

      public float Padding
      {
         get { return m_padding; }
         set
         {
            m_padding = value;
            UpdateEditorViewport();
         }
      }

      private void UpdateEditorViewport()
      {
         var cols = (int)Math.Floor((m_width - 2 * m_padding) / CHARACTER_WIDTH);
         var rows = (int)Math.Floor((m_height - 2 * m_padding) / (FONTSIZE + LINESPACING));

         m_editor.ViewportWidth = cols;
         m_editor.ViewportHeight = rows;
      }

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
         var cursor_x = m_left + m_padding + (m_editor.CursorCol - m_editor.ViewportCol) * CHARACTER_WIDTH + CURSOR_OFFSET_X;
         var cursor_y = m_top + m_padding + (m_editor.CursorRow - m_editor.ViewportRow) * (FONTSIZE + LINESPACING) - LINESPACING / 2;
         d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), cursor_x, cursor_y, 1, FONTSIZE * 2, null, CURSOR_COLOR);
      }

      private void DrawEditorBackground()
      {
         d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), m_left, m_top, m_width, m_height, null, new FastColor(200, 250, 250, 250).ToColor());
      }

      private void DrawVisibleLines()
      {
         float y = 0;
         var start_col = m_editor.ViewportCol;
         var end_col = start_col + m_editor.ViewportWidth;
         var skip_rows = Math.Max(0, m_editor.ViewportRow);
         var rows_left_after_skip = Math.Max(MyLinq.Count(m_editor.Lines) - skip_rows, 0);
         var visible_rows = Math.Min(m_editor.ViewportHeight, rows_left_after_skip);
         var row = skip_rows;
         foreach (var line in MyLinq.Take(MyLinq.Skip(m_editor.Lines, skip_rows), visible_rows))
         {
            var safe_end_col=Math.Min(line.Length, end_col);
            var safe_start_col = Math.Min(start_col, safe_end_col);
            var visible_text = line.GetSegment(safe_start_col, safe_end_col);
            if (m_editor.IsSelectionActive)
               DrawSelection(row, safe_start_col, safe_end_col);
            d_The3d.Draw2dText(visible_text, m_left + m_padding, m_top + m_padding + y, FONTSIZE, TEXT_COLOR);
            y += FONTSIZE + LINESPACING;
            row++;
         }
      }

      private void DrawSelection(int row, int start_col, int end_col)
      {
         Vector2i line_selection = m_editor.GetLineSelection(row);
         int sel_start_col = (line_selection.x <= start_col ? start_col : line_selection.x);
         if (sel_start_col > end_col)
            return; // no selection in the visible part of this line;
         var sel_end_col = (line_selection.y >= end_col ? end_col : line_selection.y);
         if (sel_end_col < start_col)
            return; // no selection in the visible part of this line
         if (sel_start_col == sel_end_col)
            return; // no selection to draw.
         var sel_begin_x = m_left + m_padding + (sel_start_col - m_editor.ViewportCol) * CHARACTER_WIDTH + CURSOR_OFFSET_X;
         var sel_begin_y = m_top + m_padding + (row - m_editor.ViewportRow) * (FONTSIZE + LINESPACING);// - LINESPACING / 2;
         var sel_width = (sel_end_col - sel_start_col) * CHARACTER_WIDTH + CURSOR_OFFSET_X;
         d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), sel_begin_x, sel_begin_y, sel_width, FONTSIZE * 2, null, SELECTION_COLOR);
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
               m_editor.CursorCol = 0;
               break;
            case Key.End:
               m_editor.GoToEndOfLine();
               break;
            case Key.ShiftLeft:
            case Key.ShiftRight:
               m_editor.IsShiftKeyDown=true;
               break;
            case Key.ControlLeft:
            case Key.ControlRight:
               m_editor.IsCtrlKeyDown = true;
               break;
            case Key.C:
               if (m_editor.IsCtrlKeyDown)
                  m_editor.Copy();
               break;
            case Key.X:
               if (m_editor.IsCtrlKeyDown)
                  m_editor.Cut();
               break;
            case Key.V:
               if (m_editor.IsCtrlKeyDown)
                  m_editor.Paste();
               break;
            case Key.A:
               if (m_editor.IsCtrlKeyDown)
                  m_editor.SelectAll();
               break;
         }
      }

      public void HandleKeyUp(object sender, KeyboardKeyEventArgs e)
      {
         switch (e.Key)
         {
            case Key.ShiftLeft:
            case Key.ShiftRight:
               m_editor.IsShiftKeyDown = false;
               break;
            case Key.ControlLeft:
            case Key.ControlRight:
               m_editor.IsCtrlKeyDown = false;
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
         //if (m_editor.IsCtrlKeyDown)
         //{
         //   switch (e.KeyChar)
         //   {
         //      case 'c':
         //         m_editor.Copy();
         //         return;
         //      case 'x':
         //         m_editor.Cut();
         //         return;
         //      case 'v':
         //         m_editor.Paste();
         //         return;
         //   }
         //}
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

      private int m_cursor_row = 0;
      private int m_cursor_col = 0;
      private int m_viewport_row = 0; // <-- if text is larger than viewport this tells which is the first visible row in the viewport
      private int m_viewport_col = 0; // <-- if text is larger than viewport this tells which is the first visible col in the viewport
      public int ViewportWidth = 1000; // <-- width in characters. Needs to be set by widget to ensure correct scrolling behavior
      public int ViewportHeight = 1000; // <-- height in lines. Needs to be set by widget to ensure correct scrolling behavior
      public int SelectionStartCol = 0; // <-- note: the value is meaningless without IsSelectionActive == true
      public int SelectionStartRow = 0; // <-- note: the value is meaningless without IsSelectionActive == true
      public int SelectionEndCol = 0; // <-- note: the value is meaningless without IsSelectionActive == true
      public int SelectionEndRow = 0; // <-- note: the value is meaningless without IsSelectionActive == true
      public bool IsSelectionActive = false; // <-- denotes whether the editor has currently selected some text

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

      public int CursorRow
      {
         get { return m_cursor_row; }
         set
         {
            m_cursor_row = value;
            if (m_cursor_row < 0)
               m_cursor_row = 0;
            if (m_cursor_row >= ViewportRow + ViewportHeight)
               m_viewport_row = m_cursor_row - ViewportHeight + 1;
            else if (m_cursor_row < ViewportRow)
               ViewportRow = m_cursor_row;
         }
      }

      public int CursorCol
      {
         get { return m_cursor_col; }
         set
         {
            m_cursor_col = value;
            if (m_cursor_col < 0)
               m_cursor_col = 0;
            m_cursor_col = Math.Min(m_cursor_col, GetLine(CursorRow).Length);
            if (m_cursor_col > ViewportCol + ViewportWidth)
               m_viewport_col = m_cursor_col - ViewportWidth;
            else if (m_cursor_col < ViewportCol)
               ViewportCol = m_cursor_col;
         }
      }

      public void ResetSelection()
      {
         IsSelectionActive=false;
         SelectionStartCol = SelectionEndCol = CursorCol;
         SelectionStartRow = SelectionEndRow = CursorRow;
      }

      private void UpdateSelection(int old_cursor_col, int old_cursor_row)
      {
         if (IsShiftKeyDown == false)
         {
            ResetSelection();
         }
         else
         {
            if (IsSelectionActive == false) // start selection
            {
               SelectionStartCol = old_cursor_col;
               SelectionStartRow = old_cursor_row;
               SelectionEndCol = CursorCol;
               SelectionEndRow = CursorRow;
               IsSelectionActive = true;
            }
            else // expand selection
            {
               SelectionEndCol = CursorCol;
               SelectionEndRow = CursorRow;
            }
            //Console.WriteLine("selection update {0}:{1}->{2}:{3}", SelectionStartCol, SelectionStartRow, SelectionEndCol, SelectionEndRow);
         }
      }

      public int ViewportRow
      {
         get { return m_viewport_row; }
         set { 
            m_viewport_row = value;
            if (m_viewport_row < 0)
               m_viewport_row = 0;
         }
      }

      public int ViewportCol
      {
         get { return m_viewport_col; }
         set
         {
            m_viewport_col = value;
            if (m_viewport_col < 0)
               m_viewport_col = 0;
         }
      }

      public bool IsShiftKeyDown { get; set; }
      public bool IsCtrlKeyDown { get; set; }

      public void MoveCursorLeftRight(int delta_col)
      {
         var old_cursor_col = m_cursor_col;
         var old_cursor_row = m_cursor_row;
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
         UpdateSelection(old_cursor_col, old_cursor_row);
      }

      public void MoveCursorUpDown(int delta_row)
      {
         var old_cursor_col = m_cursor_col;
         var old_cursor_row = m_cursor_row;
         CursorRow += delta_row;
         if (CursorRow < 0)
            CursorRow = 0;
         if (GetLineLength(CursorRow) < CursorCol)
            CursorCol = GetLineLength(CursorRow); // reset to end of line because cursor is beyond the end of line (no "virtual space")
         UpdateSelection(old_cursor_col, old_cursor_row);
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
         if (IsSelectionActive)
            DeleteSelection();
         var lines = Regex.Split( s, @"\r?\n");
         if (lines.Length == 1) // insertion of text without line breaks
         {
            var line = GetLine(CursorRow);
            line.Insert(CursorCol, s);
            CursorCol += s.Length;
            ResetSelection();
            return;
         }
         // insertion of text with line breaks
         Insert(lines[0]);
         InsertCarriageReturn();
         Insert(lines[lines.Length-1]);
         var col = CursorCol;
         if (lines.Length > 2)
         {
            for (int i = lines.Length - 2; i > 0; i--)
            {
               m_lines.Insert(CursorRow, new TextLine(lines[i]));
            }
         }
         CursorCol = col;
         CursorRow += lines.Length;
         ResetSelection();
      }

      public void InsertCarriageReturn()
      {
         if (IsSelectionActive)
            DeleteSelection();
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
         ResetSelection();
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
         if (IsSelectionActive)
         {
            DeleteSelection();
            return;
         }
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
         if (IsSelectionActive)
         {
            DeleteSelection();
            return;
         }
         var line = GetLine(CursorRow);
         if (CursorRow >= m_lines.Count && CursorCol <= line.Length)
            return;
         if (CursorCol < line.Length)
         {
            line.Remove(CursorCol, CursorCol + 1);
         }
         else
         {
            var line_below = GetLine(CursorRow + 1);
            m_lines.RemoveAt(CursorRow + 1);
            line.Append(line_below.Text);
         }
      }

      public void DeleteSelection()
      {
         int[] selection = GetSelection();
         int sel_begin_col = selection[0];
         int sel_begin_row = selection[1];
         int sel_end_col = selection[2];
         int sel_end_row = selection[3];
         // selection does not span multiple lines
         if (sel_begin_row == sel_end_row) 
         {
            DeleteSingleLineSelection(sel_begin_row, sel_begin_col, sel_end_col);
            return;
         }
         // Note: selection spans multiple lines, so we remove the last line, the lines between and then the first line (in order to not shift indices of selected lines)
         DeleteSingleLineSelection(sel_end_row, 0, sel_end_col); // remove last line
         if (sel_end_row - sel_begin_row > 1) // remove entire lines between first and last row of selection
         {
            for (int i = sel_end_row - 1; i > sel_begin_row; i--)
               m_lines.RemoveAt(i);
         }
         DeleteSingleLineSelection(sel_begin_row, sel_begin_col, GetLineLength(sel_begin_row)); // remove first line
         // join the lines before and after the selection by deleting the line break
         var line_below = GetLine(CursorRow + 1);
         m_lines.RemoveAt(CursorRow + 1);
         GetLine(CursorRow).Append(line_below.Text);
         ResetSelection();
      }

      private void DeleteSingleLineSelection(int row, int start_col, int end_col)
      {
         var line = GetLine(row);
         if (start_col == 0 && end_col >= line.Length)
            m_lines.RemoveAt(row); // remove entire line
         else
            line.Remove(start_col, end_col); // remove selected part of the line
         CursorRow = row;
         CursorCol = start_col;
         ResetSelection();
      }

      public void ReplaceSelection(string replacement)
      {
         DeleteSelection();
         Insert(replacement);
      }

      public void GoToEndOfLine()
      {
         CursorCol = GetLine(CursorRow).Length;
      }

      private int[] GetSelection() // return selection and ensure that selection begin is before selection end
      {
         int sel_begin_row, sel_begin_col, sel_end_row, sel_end_col;
         if (SelectionStartRow < SelectionEndRow)
         {
            sel_begin_col = SelectionStartCol;
            sel_begin_row = SelectionStartRow;
            sel_end_col = SelectionEndCol;
            sel_end_row = SelectionEndRow;
         }
         else if (SelectionStartRow == SelectionEndRow)
         {
            sel_begin_col = (SelectionStartCol < SelectionEndCol ? SelectionStartCol : SelectionEndCol);
            sel_begin_row = SelectionStartRow;
            sel_end_col = (SelectionStartCol < SelectionEndCol ? SelectionEndCol : SelectionStartCol);
            sel_end_row = SelectionEndRow;
         }
         else
         {
            sel_begin_col = SelectionEndCol;
            sel_begin_row = SelectionEndRow;
            sel_end_col = SelectionStartCol;
            sel_end_row = SelectionStartRow;
         }
         return new int[4] { sel_begin_col , sel_begin_row, sel_end_col, sel_end_row };
      }

      public Vector2i GetLineSelection(int line) // returned vector contains the start column as x and the end column as y
      {
         int[] selection = GetSelection();
         int sel_begin_col = selection[0];
         int sel_begin_row = selection[1];
         int sel_end_col = selection[2];
         int sel_end_row = selection[3];
         var sel = new Vector2i(0, 0);
         if (line < sel_begin_row || line > sel_end_row)
            return sel; // no selection in this line
         if (line == sel_begin_row)
         {
            sel.x = sel_begin_col;
         }
         else // line > sel_begin_row
         {
            sel.x = 0;
         }
         if (line == sel_end_row)
         {
            sel.y = sel_end_col;
         }
         else // line < sel_end_row
         {
            sel.y = GetLineLength(line);
         }
         return sel;
      }

      private string m_clip_buffer;
      public event Action<object> ClipBufferChanged; // <-- forwarding internal clipbuffer to system clipboard can be hooked into here
      public string ClipBuffer
      {
         get { return m_clip_buffer; }
         set { 
            m_clip_buffer = value;
            if (ClipBufferChanged!=null)
               ClipBufferChanged(this);
         }
      }

      public void Copy()
      {
         ClipBuffer = GetSelectedText();
      }

      public void Cut()
      {
         Copy();
         DeleteSelection();
      }

      public void Paste()
      {
         Insert(ClipBuffer);
      }

      private string GetSelectedText()
      {
         if (!IsSelectionActive)
            return "";
         int[] selection = GetSelection();
         int sel_begin_col = selection[0];
         int sel_begin_row = selection[1];
         int sel_end_col = selection[2];
         int sel_end_row = selection[3];
         // selection does not span multiple lines
         if (sel_begin_row == sel_end_row)
            return GetLine(sel_begin_row).GetSegment( sel_begin_col, sel_end_col);
         // selection spans multiple lines
         var s = new StringBuilder();
         s.AppendLine(GetLine(sel_begin_row).GetSegment(sel_begin_col, GetLineLength(sel_begin_row)));
         if (sel_end_row - sel_begin_row > 1) // add entire lines between first and last row of selection
         {
            for (int i = sel_begin_row + 1; i < sel_end_row; i++)
               s.AppendLine(GetLine(i).Text);
         }
         s.Append(GetLine(sel_end_row).GetSegment(0, sel_end_col));
         return s.ToString();
      }

      public void SelectAll()
      {
         SelectionStartCol = 0;
         SelectionStartRow = 0;
         SelectionEndRow = m_lines.Count - 1;
         SelectionEndCol = GetLine(SelectionEndRow).Length;
         IsSelectionActive = true;
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
