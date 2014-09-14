public class ModGuiTextEditor : GameScreen
{
    public ModGuiTextEditor()
    {
        buffer = new int[maxLines][];
        for (int i = 0; i < maxLines; i++)
        {
            buffer[i] = new int[maxColumns];
        }
        startX = 100;
        startY = 100;
        charSize = 12;
        font = new FontCi();
        font.family = "Courier New";
        font.size = 12;
    }
    bool visible;
    const int maxLines = 128;
    const int maxColumns = 80;
    FontCi font;
    int startX;
    int startY;
    int charSize;
    public override void OnNewFrameDraw2d(Game game, float deltaTime)
    {
        float dt = deltaTime;
        if (!visible)
        {
            return;
        }
        game.Draw2dTexture(game.WhiteTexture(), startX, startY, maxColumns * charSize, maxLines * charSize, null, 0, Game.ColorFromArgb(255, 100, 100, 100), false);
        for (int i = 0; i < maxLines; i++)
        {
            game.Draw2dText(LineToString(buffer[i]), font, startX, startY + charSize * i, null, false);
        }
        int[] spaces = new int[maxColumns];
        for (int i = 0; i < maxColumns; i++)
        {
            spaces[i] = 32;
        }
        spaces[cursorColumn] = 95; //_
        string spacesString = game.platform.CharArrayToString(spaces, cursorColumn + 1);
        game.Draw2dText(spacesString, font, startX, startY + cursorLine * charSize, null, false);
    }
    int[][] buffer;
    int cursorColumn;
    int cursorLine;
    public override void OnKeyDown(Game game_, KeyEventArgs e)
    {
        if (e.GetKeyCode() == game.GetKey(GlKeys.F9))
        {
            visible = !visible;
        }
        if (!visible)
        {
            return;
        }
        if (e.GetKeyCode() == GlKeys.Escape)
        {
            visible = false;
        }
        if (e.GetKeyCode() == GlKeys.Left)
        {
            cursorColumn--;
        }
        if (e.GetKeyCode() == GlKeys.Right)
        {
            cursorColumn++;
        }
        if (e.GetKeyCode() == GlKeys.Up)
        {
            cursorLine--;
        }
        if (e.GetKeyCode() == GlKeys.Down)
        {
            cursorLine++;
        }
        if (e.GetKeyCode() == GlKeys.BackSpace)
        {
            cursorColumn--;
            e.SetKeyCode(GlKeys.Delete);
        }
        if (cursorColumn < 0) { cursorColumn = 0; }
        if (cursorLine < 0) { cursorLine = 0; }
        if (cursorColumn >= maxColumns) { cursorColumn = maxColumns; }
        if (cursorLine > maxLines) { cursorLine = maxLines; }
        if (cursorColumn > LineLength(buffer[cursorLine])) { cursorColumn = LineLength(buffer[cursorLine]); }
        if (e.GetKeyCode() == GlKeys.Delete)
        {
            for (int i = cursorColumn; i < maxColumns - 1; i++)
            {
                buffer[cursorLine][i] = buffer[cursorLine][i + 1];
            }
        }
        e.SetHandled(true);
    }
    public override void OnKeyPress(Game game_, KeyPressEventArgs e)
    {
        if (!visible)
        {
            return;
        }
        if (e.GetKeyChar() == 8) // backspace
        {
            return;
        }
        for (int i = maxColumns - 1; i > cursorColumn; i--)
        {
            buffer[cursorLine][i] = buffer[cursorLine][i - 1];
        }
        buffer[cursorLine][cursorColumn] = e.GetKeyChar();
        cursorColumn++;
        e.SetHandled(true);
    }
    string BufferToString()
    {
        string s = "";
        for (int i = 0; i < maxLines; i++)
        {
            string line = LineToString(buffer[i]);
            s = StringTools.StringAppend(game.platform, s, line);
        }
        return s;
    }
    string LineToString(int[] line)
    {
        if (line == null)
        {
            return "";
        }
        return game.platform.CharArrayToString(line, LineLength(line));
    }
    int LineLength(int[] line)
    {
        for (int i = 0; i < maxColumns; i++)
        {
            if (line[i] == 0)
            {
                return i;
            }
        }
        return maxColumns;
    }
}
