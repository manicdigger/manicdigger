public abstract class GamePlatform
{
    // 1) find files no matter if they are in Data\Local\ or Data\Public\
    // 2) find files no matter if game is in debugger, or installed
    // 3) find files no matter if they end in .png or .jpg
    // Returns URL in JavaScript
    public abstract string GetFullFilePath(string filename, BoolRef found);
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
    public abstract string PathCombine(string part1, string part2);
    public abstract string[] DirectoryGetFiles(string path, IntRef length);
    public abstract string[] FileReadAllLines(string path, IntRef length);
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
    public abstract void BindTexture2d(int texture);
    public abstract Model CreateModel(ModelData modelData);
    public abstract void DrawModel(Model model);
    public abstract void InitShaders();
    public abstract void SetMatrixUniforms(float[] pMatrix, float[] mvMatrix);
    public abstract void GlClearColorRgbaf(float r, float g, float b, float a);
    public abstract void GlEnableDepthTest();
    public abstract int LoadTextureFromFile(string fullPath);
    public abstract string GetLanguageIso6391();
    public abstract int TimeMillisecondsFromStart();
    public abstract void DrawModels(Model[] model, int count);
    public abstract void GlDisableCullFace();
    public abstract void GlEnableCullFace();
    public abstract void ThrowException(string message);
    public abstract void DeleteModel(Model model);
    public abstract void GlEnableTexture2d();
    public abstract BitmapCi BitmapCreate(int width, int height);
    public abstract void BitmapSetPixelsRgba(BitmapCi bmp, byte[] pixels);
    public abstract int LoadTextureFromBitmap(BitmapCi bmp);
    public abstract void GLLineWidth(int width);
    public abstract void GLDisableAlphaTest();
    public abstract void GLEnableAlphaTest();
    public abstract void GLDeleteTexture(int id);
    public abstract BitmapCi CreateTextTexture2(Text_ t);
    public abstract float BitmapGetWidth(BitmapCi bmp);
    public abstract float BitmapGetHeight(BitmapCi bmp);
    public abstract void BitmapDelete(BitmapCi bmp);
    public abstract void DrawModelData(ModelData data);
    public abstract bool FloatTryParse(string s, FloatRef ret);
    public abstract float MathCos(float a);
    public abstract float MathSin(float a);
    public abstract void AudioPlay(string path, float x, float y, float z);
    public abstract void AudioPlayLoop(string path, bool play, bool restart);
    public abstract void AudioUpdateListener(float posX, float posY, float posZ, float orientX, float orientY, float orientZ);
    public abstract void ConsoleWriteLine(string p);
    public abstract DummyNetOutgoingMessage CastToDummyNetOutgoingMessage(INetOutgoingMessage message);
    public abstract MonitorObject MonitorCreate();
    public abstract void MonitorEnter(MonitorObject monitorObject);
    public abstract void MonitorExit(MonitorObject monitorObject);
    public abstract bool EnetAvailable();
    public abstract EnetHost EnetCreateHost();
    public abstract void EnetHostInitializeServer(EnetHost host, int port, int peerLimit);
    public abstract bool EnetHostService(EnetHost host, int timeout, EnetEventRef enetEvent);
    public abstract bool EnetHostCheckEvents(EnetHost host, EnetEventRef event_);
    public abstract EnetPeer EnetHostConnect(EnetHost host, string hostName, int port, int data, int channelLimit);
    public abstract void EnetPeerSend(EnetPeer peer, byte channelID, byte[] data, int dataLength, int flags);
    public abstract EnetNetConnection CastToEnetNetConnection(INetConnection connection);
    public abstract EnetNetOutgoingMessage CastToEnetNetOutgoingMessage(INetOutgoingMessage msg);
    public abstract void EnetHostInitialize(EnetHost host, IPEndPointCi address, int peerLimit, int channelLimit, int incomingBandwidth, int outgoingBandwidth);
    public abstract void SaveScreenshot();
    public abstract BitmapCi GrabScreenshot();
    public abstract AviWriterCi AviWriterCreate();
    public abstract bool StringEmpty(string data);
    public abstract float FloatModulo(float a, int b);
    public abstract void SetFreeMouse(bool value);
    public abstract UriCi ParseUri(string uri);
    public abstract OptionsCi LoadOptions();
    public abstract void SaveOptions(OptionsCi options);
    public abstract bool StringContains(string a, string b);
    public abstract RandomCi RandomCreate();
    public abstract void GlClearDepthBuffer();
    public abstract string PathStorage();
    public abstract string StringReplace(string s, string from, string to);
    public abstract PlayerInterpolationState CastToPlayerInterpolationState(InterpolatedObject a);
    public abstract void GlLightModelAmbient(int r, int g, int b);
}

public abstract class RandomCi
{
    public abstract float NextFloat();
    public abstract int Next();
}

public class OptionsCi
{
    public OptionsCi()
    {
        float one = 1;
        Shadows = false;
        Font = 0;
        DrawDistance = 256;
        UseServerTextures = true;
        EnableSound = true;
        Framerate = 0;
        Resolution = 0;
        Fullscreen = false;
        Smoothshadows = true;
        BlockShadowSave = one * 6 / 10;
        Keys = new int[256];
    }
    internal bool Shadows;
    internal int Font;
    internal int DrawDistance;
    internal bool UseServerTextures;
    internal bool EnableSound;
    internal int Framerate;
    internal int Resolution;
    internal bool Fullscreen;
    internal bool Smoothshadows;
    internal float BlockShadowSave;
    internal int[] Keys;
}

public class UriCi
{
    internal string url;
    internal string ip;
    internal int port;
    internal DictionaryStringString get;
    public string GetUrl() { return url; }
    public string GetIp() { return ip; }
    public int GetPort() { return port; }
    public DictionaryStringString GetGet() { return get; }
}

public class EnetHost
{
}

public abstract class EnetEvent
{
    public abstract EnetEventType Type();
    public abstract EnetPeer Peer();
    public abstract EnetPacket Packet();
}

public class EnetEventRef
{
    internal EnetEvent e;
}

public enum EnetEventType
{
    None,
    Connect,
    Disconnect,
    Receive
}

public class EnetPacketFlags
{
    public const int None = 0;
    public const int Reliable = 1;
    public const int Unsequenced = 2;
    public const int NoAllocate = 4;
    public const int UnreliableFragment = 8;
}

public abstract class EnetPeer
{
    public abstract int UserData();
    public abstract void SetUserData(int value);
    public abstract IPEndPointCi GetRemoteAddress();
}

public abstract class EnetPacket
{
    public abstract int GetBytesCount();
    public abstract byte[] GetBytes();
    public abstract void Dispose();
}

public class MonitorObject
{
}

public class FloatRef
{
    public static FloatRef Create(float value_)
    {
        FloatRef f = new FloatRef();
        f.value = value_;
        return f;
    }
    internal float value;
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
    public const int F1 = 112;
    public const int F2 = 113;
    public const int F3 = 114;
    public const int F4 = 115;
    public const int F5 = 116;
    public const int F6 = 117;
    public const int F7 = 118;
    public const int F8 = 119;
    public const int F9 = 120;
    public const int F10 = 121;
    public const int F11 = 122;
    public const int F12 = 123;
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
    public int GetValue() { return value; }
    public void SetValue(int value_) { value = value_; }
}
