public abstract class GamePlatform
{
    // 1) find files no matter if they are in Data\Local\ or Data\Public\
    // 2) find files no matter if game is in debugger, or installed
    // 3) find files no matter if they end in .png or .jpg
    // Returns URL in JavaScript
    public abstract string GetFullFilePath(string filename);
    public abstract int FloatToInt(float value);
    public abstract string[] StringSplit(string value, string separator, IntRef returnLength);
    public abstract int IntParse(string value);
    public abstract float FloatParse(string value);
    public abstract float MathSqrt(float value);
    public abstract string StringTrim(string value);
    public abstract string IntToString(int value);
    public abstract string Timestamp();
    public abstract string StringFormat(string format, string arg0);
    public abstract string StringFormat2(string format, string arg0, string arg1);
    public abstract void ClipboardSetText(string s);
    public abstract TextTexture CreateTextTexture(string text, float fontSize);
    public abstract void TextSize(string text, float fontSize, IntRef outWidth, IntRef outHeight);
    public abstract void Exit();
    public abstract int[] StringToCharArray(string s, IntRef length);
    public abstract string CharArrayToString(int[] charArray, int length);
    public abstract string PathSavegames();
    public abstract string[] DirectoryGetFiles(string path, IntRef length);
    public abstract void WebClientDownloadStringAsync(string url, HttpResponseCi response);
    public abstract string FileName(string fullpath);
    public abstract void AddOnNewFrame(NewFrameHandler handler);
    public abstract void AddOnKeyEvent(KeyEventHandler handler);
    public abstract void AddOnMouseEvent(MouseEventHandler handler);
    public abstract void AddOnTouchEvent(TouchEventHandler handler);
    public abstract int GetCanvasWidth();
    public abstract int GetCanvasHeight();
    public abstract void GlViewport(int x, int y, int width, int height);
    public abstract void GlClearColorBufferAndDepthBuffer();
    public abstract void GlDisableDepthTest();
    public abstract void BindTexture2d(Texture texture);
    public abstract Model CreateModel(ModelData modelData);
    public abstract void DrawModel(Model model);
    public abstract void InitShaders();
    public abstract void SetMatrixUniforms(float[] pMatrix, float[] mvMatrix);
    public abstract void GlClearColorRgbaf(float r, float g, float b, float a);
    public abstract void GlEnableDepthTest();
    public abstract Texture LoadTextureFromFile(string fullPath);
    public abstract string GetLanguageIso6391();
    public abstract int TimeMillisecondsFromStart();
    public abstract void DrawModels(Model[] model, int count);
    public abstract void GlDisableCullFace();
    public abstract void GlEnableCullFace();
    public abstract void ThrowException(string message);
}

public class KeyEventArgs
{
    int keyCode;
    public int GetKeyCode() { return keyCode; }
    public void SetKeyCode(int value) { keyCode = value; }
}

public class KeyPressEventArgs
{
    int keyChar;
    public int GetKeyChar() { return keyChar; }
    public void SetKeyChar(int value) { keyChar = value; }
}

public class GlKeys
{
    public const int PageUp = 33;
    public const int PageDown = 34;
    public const int Left = 37;
    public const int Up = 38;
    public const int Right = 39;
    public const int Down = 40;
}

public abstract class NewFrameHandler
{
    public abstract void OnNewFrame(NewFrameEventArgs args);
}

public abstract class ImageOnLoadHandler
{
    public abstract void OnLoad();
}

public abstract class KeyEventHandler
{
    public abstract void OnKeyDown(KeyEventArgs e);
    public abstract void OnKeyPress(KeyPressEventArgs e);
    public abstract void OnKeyUp(KeyEventArgs e);
}

public class MouseEventArgs
{
    int x;
    int y;
    int movementX;
    int movementY;
    int button;
    public int GetX() { return x; } public void SetX(int value) { x = value; }
    public int GetY() { return y; } public void SetY(int value) { y = value; }
    public int GetMovementX() { return movementX; } public void SetMovementX(int value) { movementX = value; }
    public int GetMovementY() { return movementY; } public void SetMovementY(int value) { movementY = value; }
    public int GetButton() { return button; } public void SetButton(int value) { button = value; }
}

public class MouseWheelEventArgs
{
    int delta;
    float deltaPrecise;
    public int GetDelta() { return delta; } public void SetDelta(int value) { delta = value; }
    public float GetDeltaPrecise() { return deltaPrecise; } public void SetDeltaPrecise(float value) { deltaPrecise = value; }
}

public class MouseButtonEnum
{
    public const int Left = 0;
    public const int Middle = 1;
    public const int Right = 2;
}

public abstract class MouseEventHandler
{
    public abstract void OnMouseDown(MouseEventArgs e);
    public abstract void OnMouseUp(MouseEventArgs e);
    public abstract void OnMouseMove(MouseEventArgs e);
    public abstract void OnMouseWheel(MouseWheelEventArgs e);
}

public class TouchEventArgs
{
    int x;
    int y;
    int id;
    public int GetX() { return x; } public void SetX(int value) { x = value; }
    public int GetY() { return y; } public void SetY(int value) { y = value; }
    public int GetId() { return id; } public void SetId(int value) { id = value; }
}

public abstract class TouchEventHandler
{
    public abstract void OnTouchStart(TouchEventArgs e);
    public abstract void OnTouchMove(TouchEventArgs e);
    public abstract void OnTouchEnd(TouchEventArgs e);
}

public class NewFrameEventArgs
{
    float dt;
    public float GetDt()
    {
        return dt;
    }
    public void SetDt(float p)
    {
        this.dt = p;
    }
}

public abstract class Texture
{
}

public enum TextAlign
{
    Left,
    Center,
    Right
}

public enum TextBaseline
{
    Top,
    Middle,
    Bottom
}

public class IntRef
{
    public static IntRef Create(int value_)
    {
        IntRef intref = new IntRef();
        intref.value = value_;
        return intref;
    }
    internal int value;
}
