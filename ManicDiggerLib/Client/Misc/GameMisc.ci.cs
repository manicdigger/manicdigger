public class GameScreen : ClientMod
{
    public GameScreen()
    {
        WidgetCount = 64;
        widgets = new MenuWidget[WidgetCount];
    }
    internal Game game;
    public override void OnKeyPress(Game game_, KeyPressEventArgs args) { KeyPress(args); }
    public override void OnKeyDown(Game game_, KeyEventArgs args) { KeyDown(args); }
    public override void OnTouchStart(Game game_, TouchEventArgs e) { ScreenOnTouchStart(e); }
    public void ScreenOnTouchStart(TouchEventArgs e)
    {
        e.SetHandled(MouseDown(e.GetX(), e.GetY()));
    }
    public override void OnTouchEnd(Game game_, TouchEventArgs e) { ScreenOnTouchEnd(e); }
    public void ScreenOnTouchEnd(TouchEventArgs e)
    {
        MouseUp(e.GetX(), e.GetY());
    }
    public override void OnMouseDown(Game game_, MouseEventArgs args) { MouseDown(args.GetX(), args.GetY()); }
    public override void OnMouseUp(Game game_, MouseEventArgs args) { MouseUp(args.GetX(), args.GetY()); }
    public override void OnMouseMove(Game game_, MouseEventArgs args) { MouseMove(args); }
    public virtual void OnBackPressed() { }

    void KeyPress(KeyPressEventArgs e)
    {
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                if (w.type == WidgetType.Textbox)
                {
                    if (w.editing)
                    {
                        if (game.platform.IsValidTypingChar(e.GetKeyChar()))
                        {
                            w.text = StringTools.StringAppend(game.platform, w.text, CharToString(e.GetKeyChar()));
                        }
                    }
                }
            }
        }
    }

    void KeyDown(KeyEventArgs e)
    {
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                if (w.type == WidgetType.Textbox)
                {
                    if (w.editing)
                    {
                        int key = e.GetKeyCode();
                        // pasting text from clipboard
                        if (e.GetCtrlPressed() && key == GlKeys.V)
                        {
                            if (game.platform.ClipboardContainsText())
                            {
                                w.text = StringTools.StringAppend(game.platform, w.text, game.platform.ClipboardGetText());
                            }
                            return;
                        }
                        // deleting characters using backspace
                        if (key == GlKeys.BackSpace)
                        {
                            if (StringTools.StringLength(game.platform, w.text) > 0)
                            {
                                w.text = StringTools.StringSubstring(game.platform, w.text, 0, StringTools.StringLength(game.platform, w.text) - 1);
                            }
                            return;
                        }
                    }
                }
            }
        }
    }

    bool MouseDown(int x, int y)
    {
        bool handled = false;
        bool editingChange = false;
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                if (w.type == WidgetType.Button)
                {
                    w.pressed = pointInRect(x, y, screenx + w.x, screeny + w.y, w.sizex, w.sizey);
                    if (w.pressed) { handled = true; }
                }
                if (w.type == WidgetType.Textbox)
                {
                    w.pressed = pointInRect(x, y, screenx + w.x, screeny + w.y, w.sizex, w.sizey);
                    if (w.pressed) { handled = true; }
                    bool wasEditing = w.editing;
                    w.editing = w.pressed;
                    if (w.editing && (!wasEditing))
                    {
                        game.platform.ShowKeyboard(true);
                        editingChange = true;
                    }
                    if ((!w.editing) && wasEditing && (!editingChange))
                    {
                        game.platform.ShowKeyboard(false);
                    }
                }
            }
        }
        return handled;
    }

    void MouseUp(int x, int y)
    {
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                w.pressed = false;
            }
        }
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                if (w.type == WidgetType.Button)
                {
                    if (pointInRect(x, y, screenx + w.x, screeny + w.y, w.sizex, w.sizey))
                    {
                        OnButton(w);
                    }
                }
            }
        }
    }

    public virtual void OnButton(MenuWidget w) { }

    void MouseMove(MouseEventArgs e)
    {
        if (e.GetEmulated() && !e.GetForceUsage())
        {
            return;
        }
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                w.hover = pointInRect(e.GetX(), e.GetY(), screenx + w.x, screeny + w.y, w.sizex, w.sizey);
            }
        }
    }

    bool pointInRect(float x, float y, float rx, float ry, float rw, float rh)
    {
        return x >= rx && y >= ry && x < rx + rw && y < ry + rh;
    }

    public virtual void OnMouseWheel(MouseWheelEventArgs e) { }
    internal int WidgetCount;
    internal MenuWidget[] widgets;
    public void DrawWidgets()
    {
        for (int i = 0; i < WidgetCount; i++)
        {
            MenuWidget w = widgets[i];
            if (w != null)
            {
                if (!w.visible)
                {
                    continue;
                }
                string text = w.text;
                if (w.selected)
                {
                    text = StringTools.StringAppend(game.platform, "&2", text);
                }
                if (w.type == WidgetType.Button)
                {
                    if (w.buttonStyle == ButtonStyle.Text)
                    {
                        //game.Draw2dText1(text, w.fontSize, w.x, w.y + w.sizey / 2, TextAlign.Left, TextBaseline.Middle);
                    }
                    else
                    {
                        if (w.image != null)
                        {
                            game.Draw2dBitmapFile(w.image, screenx + w.x, screeny + w.y, w.sizex, w.sizey);
                        }
                        else
                        {
                            game.Draw2dTexture(game.WhiteTexture(), screenx + w.x, screeny + w.y, w.sizex, w.sizey, null, 0, w.color, false);
                        }
                        game.Draw2dText1(text, screenx + game.platform.FloatToInt(w.x), screeny + game.platform.FloatToInt(w.y + w.sizey / 2), game.platform.FloatToInt(w.fontSize), null, false);
                    }
                }
                if (w.type == WidgetType.Textbox)
                {
                    if (w.password)
                    {
                        text = CharRepeat(42, StringTools.StringLength(game.platform, w.text)); // '*'
                    }
                    if (w.editing)
                    {
                        text = StringTools.StringAppend(game.platform, text, "_");
                    }
                    //if (w.buttonStyle == ButtonStyle.Text)
                    {
                        game.Draw2dText(text, w.font, screenx + w.x, screeny + w.y, null, false);//, TextAlign.Left, TextBaseline.Top);
                    }
                    //else
                    {
                        //menu.DrawButton(text, w.fontSize, w.x, w.y, w.sizex, w.sizey, (w.hover || w.editing));
                    }
                }
                if (w.type == WidgetType.Label)
                {
                    game.Draw2dText(text, w.font, screenx + w.x, screeny + w.y, IntRef.Create(Game.ColorFromArgb(255, 0, 0, 0)), false);
                }
                if (w.description != null)
                {
                    //menu.DrawText(w.description, w.fontSize, w.x, w.y + w.sizey / 2, TextAlign.Right, TextBaseline.Middle);
                }
            }
        }
    }
    public string CharToString(int a)
    {
        int[] arr = new int[1];
        arr[0] = a;
        return game.platform.CharArrayToString(arr, 1);
    }

    public string CharRepeat(int c, int length)
    {
        int[] charArray = new int[length];
        for (int i = 0; i < length; i++)
        {
            charArray[i] = c;
        }
        return game.platform.CharArrayToString(charArray, length);
    }
    internal int screenx;
    internal int screeny;
}

public class LoginData
{
    internal string ServerAddress;
    internal int Port;
    internal string AuthCode; //Md5(private server key + player name)
    internal string Token;

    internal bool PasswordCorrect;
    internal bool ServerCorrect;
}

public class LoginClientCi
{
    internal LoginResultRef loginResult;
    public void Login(GamePlatform platform, string user, string password, string publicServerKey, string token, LoginResultRef result, LoginData resultLoginData_)
    {
        loginResult = result;
        resultLoginData = resultLoginData_;
        result.value = LoginResult.Connecting;

        LoginUser = user;
        LoginPassword = password;
        LoginToken = token;
        LoginPublicServerKey = publicServerKey;
        shouldLogin = true;
    }
    string LoginUser;
    string LoginPassword;
    string LoginToken;
    string LoginPublicServerKey;

    bool shouldLogin;
    string loginUrl;
    HttpResponseCi loginUrlResponse;
    HttpResponseCi loginResponse;
    LoginData resultLoginData;
    public void Update(GamePlatform platform)
    {
        if (loginResult == null)
        {
            return;
        }

        if (loginUrlResponse == null && loginUrl == null)
        {
            loginUrlResponse = new HttpResponseCi();
            platform.WebClientDownloadDataAsync("http://manicdigger.sourceforge.net/login.php", loginUrlResponse);
        }
        if (loginUrlResponse != null && loginUrlResponse.done)
        {
            loginUrl = platform.StringFromUtf8ByteArray(loginUrlResponse.value, loginUrlResponse.valueLength);
            loginUrlResponse = null;
        }

        if (loginUrl != null)
        {
            if (shouldLogin)
            {
                shouldLogin = false;
                string requestString = platform.StringFormat4("username={0}&password={1}&server={2}&token={3}"
                    , LoginUser, LoginPassword, LoginPublicServerKey, LoginToken);
                IntRef byteArrayLength = new IntRef();
                byte[] byteArray = platform.StringToUtf8ByteArray(requestString, byteArrayLength);
                loginResponse = new HttpResponseCi();
                platform.WebClientUploadDataAsync(loginUrl, byteArray, byteArrayLength.value, loginResponse);
            }
            if (loginResponse != null && loginResponse.done)
            {
                string responseString = platform.StringFromUtf8ByteArray(loginResponse.value, loginResponse.valueLength);
                resultLoginData.PasswordCorrect = !(platform.StringContains(responseString, "Wrong username") || platform.StringContains(responseString, "Incorrect username"));
                resultLoginData.ServerCorrect = !platform.StringContains(responseString, "server");
                if (resultLoginData.PasswordCorrect)
                {
                    loginResult.value = LoginResult.Ok;
                }
                else
                {
                    loginResult.value = LoginResult.Failed;
                }
                IntRef linesCount = new IntRef();
                string[] lines = platform.ReadAllLines(responseString, linesCount);
                if (linesCount.value >= 3)
                {
                    resultLoginData.AuthCode = lines[0];
                    resultLoginData.ServerAddress = lines[1];
                    resultLoginData.Port = platform.IntParse(lines[2]);
                    resultLoginData.Token = lines[3];
                }
                loginResponse = null;
            }
        }
    }
}

public class GameExit
{
    internal bool exit;
    internal bool restart;

    public void SetExit(bool p)
    {
        exit = p;
    }

    public bool GetExit()
    {
        return exit;
    }

    public void SetRestart(bool p)
    {
        restart = p;
    }

    public bool GetRestart()
    {
        return restart;
    }
}

public class TileEnterData
{
    internal int BlockPositionX;
    internal int BlockPositionY;
    internal int BlockPositionZ;
    internal TileEnterDirection EnterDirection;
}

public class UpDown
{
    public const int None = 0;
    public const int Up = 1;
    public const int Down = 2;
}

class StringByteArray
{
    internal string name;
    internal byte[] data;
}

public class RenderHintEnum
{
    public const int Fast = 0;
    public const int Nice = 1;
}

public class Speculative
{
    internal int x;
    internal int y;
    internal int z;
    internal int timeMilliseconds;
    internal int blocktype;
}

public class TimerCi
{
    public TimerCi()
    {
        interval = 1;
        maxDeltaTime = -1;
    }
    internal float interval;
    internal float maxDeltaTime;

    internal float accumulator;
    public void Reset()
    {
        accumulator = 0;
    }
    public int Update(float dt)
    {
        accumulator += dt;
        float constDt = interval;
        if (maxDeltaTime != -1 && accumulator > maxDeltaTime)
        {
            accumulator = maxDeltaTime;
        }
        int updates = 0;
        while (accumulator >= constDt)
        {
            updates++;
            accumulator -= constDt;
        }
        return updates;
    }

    internal static TimerCi Create(int interval_, int maxDeltaTime_)
    {
        TimerCi timer = new TimerCi();
        timer.interval = interval_;
        timer.maxDeltaTime = maxDeltaTime_;
        return timer;
    }
}

public class GetBlockHeight_ : DelegateGetBlockHeight
{
    public static GetBlockHeight_ Create(Game w_)
    {
        GetBlockHeight_ g = new GetBlockHeight_();
        g.w = w_;
        return g;
    }
    internal Game w;
    public override float GetBlockHeight(int x, int y, int z)
    {
        return w.getblockheight(x, y, z);
    }
}

public class IsBlockEmpty_ : DelegateIsBlockEmpty
{
    public static IsBlockEmpty_ Create(Game w_)
    {
        IsBlockEmpty_ g = new IsBlockEmpty_();
        g.w = w_;
        return g;
    }
    Game w;
    public override bool IsBlockEmpty(int x, int y, int z)
    {
        return w.IsTileEmptyForPhysics(x, y, z);
    }
}

public class Sprite
{
    public Sprite()
    {
        size = 40;
    }
    internal float positionX;
    internal float positionY;
    internal float positionZ;
    internal string image;
    internal int size;
    internal int animationcount;
}

public class PlayerDrawInfo
{
    public PlayerDrawInfo()
    {
        anim = new AnimationState();
        AnimationHint_ = new AnimationHint();
    }
    internal AnimationState anim;
    internal NetworkInterpolation interpolation;
    internal float lastnetworkposX;
    internal float lastnetworkposY;
    internal float lastnetworkposZ;
    internal float lastcurposX;
    internal float lastcurposY;
    internal float lastcurposZ;
    internal float lastnetworkrotx;
    internal float lastnetworkroty;
    internal float lastnetworkrotz;
    internal float velocityX;
    internal float velocityY;
    internal float velocityZ;
    internal bool moves;
    internal AnimationHint AnimationHint_;
}

public class PlayerInterpolate : IInterpolation
{
    internal GamePlatform platform;
    public override InterpolatedObject Interpolate(InterpolatedObject a, InterpolatedObject b, float progress)
    {
        PlayerInterpolationState aa = platform.CastToPlayerInterpolationState(a);
        PlayerInterpolationState bb = platform.CastToPlayerInterpolationState(b);
        PlayerInterpolationState cc = new PlayerInterpolationState();
        cc.positionX = aa.positionX + (bb.positionX - aa.positionX) * progress;
        cc.positionY = aa.positionY + (bb.positionY - aa.positionY) * progress;
        cc.positionZ = aa.positionZ + (bb.positionZ - aa.positionZ) * progress;
        //cc.heading = Game.IntToByte(AngleInterpolation.InterpolateAngle256(platform, aa.heading, bb.heading, progress));
        //cc.pitch = Game.IntToByte(AngleInterpolation.InterpolateAngle256(platform, aa.pitch, bb.pitch, progress));
        cc.rotx = DegToRad(AngleInterpolation.InterpolateAngle360(platform, RadToDeg(aa.rotx), RadToDeg(bb.rotx), progress));
        cc.roty = DegToRad(AngleInterpolation.InterpolateAngle360(platform, RadToDeg(aa.roty), RadToDeg(bb.roty), progress));
        cc.rotz = DegToRad(AngleInterpolation.InterpolateAngle360(platform, RadToDeg(aa.rotz), RadToDeg(bb.rotz), progress));
        return cc;
    }
    public static float RadToDeg(float rad)
    {
        return (rad / (2 * Game.GetPi())) * 360;
    }
    public static float DegToRad(float deg)
    {
        return (deg / 360) * 2 * Game.GetPi();
    }
}

public class PlayerInterpolationState : InterpolatedObject
{
    internal float positionX;
    internal float positionY;
    internal float positionZ;
    internal float rotx;
    internal float roty;
    internal float rotz;
    internal byte heading;
    internal byte pitch;
}

public class Bullet_
{
    internal float fromX;
    internal float fromY;
    internal float fromZ;
    internal float toX;
    internal float toY;
    internal float toZ;
    internal float speed;
    internal float progress;
}

public class Expires
{
    internal static Expires Create(float p)
    {
        Expires expires = new Expires();
        expires.totalTime = p;
        expires.timeLeft = p;
        return expires;
    }

    internal float totalTime;
    internal float timeLeft;
}

public class DrawName
{
    internal float TextX;
    internal float TextY;
    internal float TextZ;
    internal string Name;
    internal bool DrawHealth;
    internal float Health;
    internal bool OnlyWhenSelected;
    internal bool ClientAutoComplete;
}

public class Entity
{
    public Entity()
    {
        scripts = new EntityScript[8];
        scriptsCount = 0;
    }
    internal Expires expires;
    internal Sprite sprite;
    internal Grenade_ grenade;
    internal Bullet_ bullet;
    internal Minecart minecart;
    internal PlayerDrawInfo playerDrawInfo;

    internal EntityScript[] scripts;
    internal int scriptsCount;

    // network
    internal EntityPosition_ networkPosition;
    internal EntityPosition_ position;
    internal DrawName drawName;
    internal EntityDrawModel drawModel;
    internal EntityDrawText drawText;
    internal Packet_ServerExplosion push;
    internal bool usable;
    internal Packet_ServerPlayerStats playerStats;
    internal EntityDrawArea drawArea;
}

public class EntityDrawArea
{
    internal int x;
    internal int y;
    internal int z;
    internal int sizex;
    internal int sizey;
    internal int sizez;
    internal bool visible;
}

public class EntityPosition_
{
    internal float x;
    internal float y;
    internal float z;
    internal float rotx;
    internal float roty;
    internal float rotz;

    internal bool PositionLoaded;
    internal int LastUpdateMilliseconds;
}

public class EntityDrawModel
{
    public EntityDrawModel()
    {
        CurrentTexture = -1;
    }
    internal float eyeHeight;
    internal string Model_;
    internal float ModelHeight;
    internal string Texture_;
    internal bool DownloadSkin;

    internal int CurrentTexture;
    internal HttpResponseCi SkinDownloadResponse;
    internal AnimatedModelRenderer renderer;
}

public class EntityDrawText
{
    internal float dx;
    internal float dy;
    internal float dz;
    internal float rotx;
    internal float roty;
    internal float rotz;
    internal string text;
}

public class Vector3Float
{
    internal int x;
    internal int y;
    internal int z;
    internal float value;
}

public class VisibleDialog
{
    internal string key;
    internal Packet_Dialog value;
    internal GameScreen screen;
}

public class RailMapUtil
{
    internal Game game;
    public RailSlope GetRailSlope(int x, int y, int z)
    {
        int tiletype = game.map.GetBlock(x, y, z);
        int railDirectionFlags = game.blocktypes[tiletype].Rail;
        int blocknear;
        if (x < game.map.MapSizeX - 1)
        {
            blocknear = game.map.GetBlock(x + 1, y, z);
            if (railDirectionFlags == RailDirectionFlags.Horizontal &&
                 blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
            {
                return RailSlope.TwoRightRaised;
            }
        }
        if (x > 0)
        {
            blocknear = game.map.GetBlock(x - 1, y, z);
            if (railDirectionFlags == RailDirectionFlags.Horizontal &&
                 blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
            {
                return RailSlope.TwoLeftRaised;

            }
        }
        if (y > 0)
        {
            blocknear = game.map.GetBlock(x, y - 1, z);
            if (railDirectionFlags == RailDirectionFlags.Vertical &&
                  blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
            {
                return RailSlope.TwoUpRaised;
            }
        }
        if (y < game.map.MapSizeY - 1)
        {
            blocknear = game.map.GetBlock(x, y + 1, z);
            if (railDirectionFlags == RailDirectionFlags.Vertical &&
                  blocknear != 0 && game.blocktypes[blocknear].Rail == 0)
            {
                return RailSlope.TwoDownRaised;
            }
        }
        return RailSlope.Flat;
    }
}

public class RailDirectionFlags
{
    public const int None = 0;
    public const int Horizontal = 1;
    public const int Vertical = 2;
    public const int UpLeft = 4;
    public const int UpRight = 8;
    public const int DownLeft = 16;
    public const int DownRight = 32;

    public const int Full = Horizontal | Vertical | UpLeft | UpRight | DownLeft | DownRight;
    public const int TwoHorizontalVertical = Horizontal | Vertical;
    public const int Corners = UpLeft | UpRight | DownLeft | DownRight;
}

public enum RailSlope
{
    Flat, TwoLeftRaised, TwoRightRaised, TwoUpRaised, TwoDownRaised
}

public enum RailDirection
{
    Horizontal,
    Vertical,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

public enum TileExitDirection
{
    Up,
    Down,
    Left,
    Right
}

public enum TileEnterDirection
{
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Each RailDirection on tile can be traversed by train in two directions.
/// </summary>
/// <example>
/// RailDirection.Horizontal -> VehicleDirection12.HorizontalLeft (vehicle goes left and decreases x position),
/// and VehicleDirection12.HorizontalRight (vehicle goes right and increases x position).
/// </example>
public enum VehicleDirection12
{
    HorizontalLeft,
    HorizontalRight,
    VerticalUp,
    VerticalDown,

    UpLeftUp,
    UpLeftLeft,
    UpRightUp,
    UpRightRight,

    DownLeftDown,
    DownLeftLeft,
    DownRightDown,
    DownRightRight
}

public class VehicleDirection12Flags
{
    public const int None = 0;
    public const int HorizontalLeft = 1 << 0;
    public const int HorizontalRight = 1 << 1;
    public const int VerticalUp = 1 << 2;
    public const int VerticalDown = 1 << 3;

    public const int UpLeftUp = 1 << 4;
    public const int UpLeftLeft = 1 << 5;
    public const int UpRightUp = 1 << 6;
    public const int UpRightRight = 1 << 7;

    public const int DownLeftDown = 1 << 8;
    public const int DownLeftLeft = 1 << 9;
    public const int DownRightDown = 1 << 10;
    public const int DownRightRight = 1 << 11;
}

public class DirectionUtils
{
    /// <summary>
    /// VehicleDirection12.UpRightRight -> returns Direction4.Right
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static TileExitDirection ResultExit(VehicleDirection12 direction)
    {
        switch (direction)
        {
            case VehicleDirection12.HorizontalLeft:
                return TileExitDirection.Left;
            case VehicleDirection12.HorizontalRight:
                return TileExitDirection.Right;
            case VehicleDirection12.VerticalUp:
                return TileExitDirection.Up;
            case VehicleDirection12.VerticalDown:
                return TileExitDirection.Down;

            case VehicleDirection12.UpLeftUp:
                return TileExitDirection.Up;
            case VehicleDirection12.UpLeftLeft:
                return TileExitDirection.Left;
            case VehicleDirection12.UpRightUp:
                return TileExitDirection.Up;
            case VehicleDirection12.UpRightRight:
                return TileExitDirection.Right;

            case VehicleDirection12.DownLeftDown:
                return TileExitDirection.Down;
            case VehicleDirection12.DownLeftLeft:
                return TileExitDirection.Left;
            case VehicleDirection12.DownRightDown:
                return TileExitDirection.Down;
            case VehicleDirection12.DownRightRight:
                return TileExitDirection.Right;
            default:
                return TileExitDirection.Down;
        }
    }

    public static RailDirection ToRailDirection(VehicleDirection12 direction)
    {
        switch (direction)
        {
            case VehicleDirection12.HorizontalLeft:
                return RailDirection.Horizontal;
            case VehicleDirection12.HorizontalRight:
                return RailDirection.Horizontal;
            case VehicleDirection12.VerticalUp:
                return RailDirection.Vertical;
            case VehicleDirection12.VerticalDown:
                return RailDirection.Vertical;

            case VehicleDirection12.UpLeftUp:
                return RailDirection.UpLeft;
            case VehicleDirection12.UpLeftLeft:
                return RailDirection.UpLeft;
            case VehicleDirection12.UpRightUp:
                return RailDirection.UpRight;
            case VehicleDirection12.UpRightRight:
                return RailDirection.UpRight;

            case VehicleDirection12.DownLeftDown:
                return RailDirection.DownLeft;
            case VehicleDirection12.DownLeftLeft:
                return RailDirection.DownLeft;
            case VehicleDirection12.DownRightDown:
                return RailDirection.DownRight;
            case VehicleDirection12.DownRightRight:
                return RailDirection.DownRight;
            default:
                return RailDirection.DownLeft;
        }
    }

    public static int ToRailDirectionFlags(RailDirection direction)
    {
        switch (direction)
        {
            case RailDirection.DownLeft:
                return RailDirectionFlags.DownLeft;
            case RailDirection.DownRight:
                return RailDirectionFlags.DownRight;
            case RailDirection.Horizontal:
                return RailDirectionFlags.Horizontal;
            case RailDirection.UpLeft:
                return RailDirectionFlags.UpLeft;
            case RailDirection.UpRight:
                return RailDirectionFlags.UpRight;
            case RailDirection.Vertical:
                return RailDirectionFlags.Vertical;
            default:
                return 0;
        }
    }

    public static VehicleDirection12 Reverse(VehicleDirection12 direction)
    {
        switch (direction)
        {
            case VehicleDirection12.HorizontalLeft:
                return VehicleDirection12.HorizontalRight;
            case VehicleDirection12.HorizontalRight:
                return VehicleDirection12.HorizontalLeft;
            case VehicleDirection12.VerticalUp:
                return VehicleDirection12.VerticalDown;
            case VehicleDirection12.VerticalDown:
                return VehicleDirection12.VerticalUp;

            case VehicleDirection12.UpLeftUp:
                return VehicleDirection12.UpLeftLeft;
            case VehicleDirection12.UpLeftLeft:
                return VehicleDirection12.UpLeftUp;
            case VehicleDirection12.UpRightUp:
                return VehicleDirection12.UpRightRight;
            case VehicleDirection12.UpRightRight:
                return VehicleDirection12.UpRightUp;

            case VehicleDirection12.DownLeftDown:
                return VehicleDirection12.DownLeftLeft;
            case VehicleDirection12.DownLeftLeft:
                return VehicleDirection12.DownLeftDown;
            case VehicleDirection12.DownRightDown:
                return VehicleDirection12.DownRightRight;
            case VehicleDirection12.DownRightRight:
                return VehicleDirection12.DownRightDown;
            default:
                return VehicleDirection12.DownLeftDown;
        }
    }

    public static int ToVehicleDirection12Flags(VehicleDirection12 direction)
    {
        switch (direction)
        {
            case VehicleDirection12.HorizontalLeft:
                return VehicleDirection12Flags.HorizontalLeft;
            case VehicleDirection12.HorizontalRight:
                return VehicleDirection12Flags.HorizontalRight;
            case VehicleDirection12.VerticalUp:
                return VehicleDirection12Flags.VerticalUp;
            case VehicleDirection12.VerticalDown:
                return VehicleDirection12Flags.VerticalDown;

            case VehicleDirection12.UpLeftUp:
                return VehicleDirection12Flags.UpLeftUp;
            case VehicleDirection12.UpLeftLeft:
                return VehicleDirection12Flags.UpLeftLeft;
            case VehicleDirection12.UpRightUp:
                return VehicleDirection12Flags.UpRightUp;
            case VehicleDirection12.UpRightRight:
                return VehicleDirection12Flags.UpRightRight;

            case VehicleDirection12.DownLeftDown:
                return VehicleDirection12Flags.DownLeftDown;
            case VehicleDirection12.DownLeftLeft:
                return VehicleDirection12Flags.DownLeftLeft;
            case VehicleDirection12.DownRightDown:
                return VehicleDirection12Flags.DownRightDown;
            case VehicleDirection12.DownRightRight:
                return VehicleDirection12Flags.DownRightRight;
            default:
                return 0;
        }
    }

    public static TileEnterDirection ResultEnter(TileExitDirection direction)
    {
        switch (direction)
        {
            case TileExitDirection.Up:
                return TileEnterDirection.Down;
            case TileExitDirection.Down:
                return TileEnterDirection.Up;
            case TileExitDirection.Left:
                return TileEnterDirection.Right;
            case TileExitDirection.Right:
                return TileEnterDirection.Left;
            default:
                return TileEnterDirection.Down;
        }
    }
    public static int RailDirectionFlagsCount(int railDirectionFlags)
    {
        int count = 0;
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.DownLeft)) != 0) { count++; }
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.DownRight)) != 0) { count++; }
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.Horizontal)) != 0) { count++; }
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.UpLeft)) != 0) { count++; }
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.UpRight)) != 0) { count++; }
        if ((railDirectionFlags & DirectionUtils.ToRailDirectionFlags(RailDirection.Vertical)) != 0) { count++; }
        return count;
    }

    public static int ToVehicleDirection12Flags_(VehicleDirection12[] directions, int directionsCount)
    {
        int flags = VehicleDirection12Flags.None;
        for (int i = 0; i < directionsCount; i++)
        {
            VehicleDirection12 d = directions[i];
            flags = flags | DirectionUtils.ToVehicleDirection12Flags(d);
        }
        return flags;
    }

    /// <summary>
    /// Enter at TileEnterDirection.Left -> yields VehicleDirection12.UpLeftUp,
    /// VehicleDirection12.HorizontalRight,
    /// VehicleDirection12.DownLeftDown
    /// </summary>
    /// <param name="enter_at"></param>
    /// <returns></returns>
    public static VehicleDirection12[] PossibleNewRails3(TileEnterDirection enter_at)
    {
        VehicleDirection12[] ret = new VehicleDirection12[3];
        switch (enter_at)
        {
            case TileEnterDirection.Left:
                ret[0] = VehicleDirection12.UpLeftUp;
                ret[1] = VehicleDirection12.HorizontalRight;
                ret[2] = VehicleDirection12.DownLeftDown;
                break;
            case TileEnterDirection.Down:
                ret[0] = VehicleDirection12.DownLeftLeft;
                ret[1] = VehicleDirection12.VerticalUp;
                ret[2] = VehicleDirection12.DownRightRight;
                break;
            case TileEnterDirection.Up:
                ret[0] = VehicleDirection12.UpLeftLeft;
                ret[1] = VehicleDirection12.VerticalDown;
                ret[2] = VehicleDirection12.UpRightRight;
                break;
            case TileEnterDirection.Right:
                ret[0] = VehicleDirection12.UpRightUp;
                ret[1] = VehicleDirection12.HorizontalLeft;
                ret[2] = VehicleDirection12.DownRightDown;
                break;
            default:
                return null;
        }
        return ret;
    }
}

public class ClientInventoryController : IInventoryController
{
    public static ClientInventoryController Create(Game game)
    {
        ClientInventoryController c = new ClientInventoryController();
        c.g = game;
        return c;
    }

    Game g;

    public override void InventoryClick(Packet_InventoryPosition pos)
    {
        g.InventoryClick(pos);
    }

    public override void WearItem(Packet_InventoryPosition from, Packet_InventoryPosition to)
    {
        g.WearItem(from, to);
    }

    public override void MoveToInventory(Packet_InventoryPosition from)
    {
        g.MoveToInventory(from);
    }
}

public enum CameraType
{
    Fpp,
    Tpp,
    Overhead
}

public enum TypingState
{
    None,
    Typing,
    Ready
}

public class Player
{
    public Player()
    {
        AnimationHint_ = new AnimationHint();
        Model_ = "player.txt";
        EyeHeight = DefaultEyeHeight();
        ModelHeight = DefaultModelHeight();
        CurrentTexture = -1;
    }
    internal bool PositionLoaded;
    internal float PositionX;
    internal float PositionY;
    internal float PositionZ;
    internal byte Heading;
    internal byte Pitch;
    internal string Name;
    internal AnimationHint AnimationHint_;
    internal PlayerType Type;
    internal int MonsterType;
    internal int Health;
    internal int LastUpdateMilliseconds;
    internal string Model_;
    internal string Texture;
    internal float EyeHeight;
    internal float ModelHeight;
    internal float NetworkX;
    internal float NetworkY;
    internal float NetworkZ;
    internal byte NetworkHeading;
    internal byte NetworkPitch;
    internal PlayerDrawInfo playerDrawInfo;
    internal bool moves;
    internal int CurrentTexture;
    internal HttpResponseCi SkinDownloadResponse;

    public float DefaultEyeHeight()
    {
        float one = 1;
        return one * 15 / 10;
    }

    public float DefaultModelHeight()
    {
        float one = 1;
        return one * 17 / 10;
    }
}

public enum PlayerType
{
    Player,
    Monster
}

public class Grenade_
{
    internal float velocityX;
    internal float velocityY;
    internal float velocityZ;
    internal int block;
    internal int sourcePlayer;
}

public class GetCameraMatrix : IGetCameraMatrix
{
    internal float[] lastmvmatrix;
    internal float[] lastpmatrix;
    public override float[] GetModelViewMatrix()
    {
        return lastmvmatrix;
    }

    public override float[] GetProjectionMatrix()
    {
        return lastpmatrix;
    }
}

public class MenuState
{
    internal int selected;
}

public enum EscapeMenuState
{
    Main,
    Options,
    Graphics,
    Keys,
    Other
}

public class MapLoadingProgressEventArgs
{
    internal int ProgressPercent;
    internal int ProgressBytes;
    internal string ProgressStatus;
}

public class Draw2dData
{
    internal float x1;
    internal float y1;
    internal float width;
    internal float height;
    internal IntRef inAtlasId;
    internal int color;
}

public class Chunk
{
    public Chunk()
    {
        baseLightDirty = true;
    }

    internal byte[] data;
    internal int[] dataInt;
    internal byte[] baseLight;
    internal bool baseLightDirty;
    internal RenderedChunk rendered;

    public int GetBlockInChunk(int pos)
    {
        if (dataInt != null)
        {
            return dataInt[pos];
        }
        else
        {
            return data[pos];
        }
    }

    public void SetBlockInChunk(int pos, int block)
    {
        if (dataInt == null)
        {
            if (block < 255)
            {
                data[pos] = Game.IntToByte(block);
            }
            else
            {
                int n = Game.chunksize * Game.chunksize * Game.chunksize;
                dataInt = new int[n];
                for (int i = 0; i < n; i++)
                {
                    dataInt[i] = data[i];
                }
                data = null;

                dataInt[pos] = block;
            }
        }
        else
        {
            dataInt[pos] = block;
        }
    }

    public bool ChunkHasData()
    {
        return data != null || dataInt != null;
    }
}

public class ChunkEntityClient
{

}

public class RenderedChunk
{
    public RenderedChunk()
    {
        dirty = true;
    }
    internal int[] ids;
    internal int idsCount;
    internal bool dirty;
    internal byte[] light;
}

public class ITerrainTextures
{
    internal Game game;

    public int texturesPacked() { return game.texturesPacked(); }
    public int terrainTexture() { return game.terrainTexture; }
    public int[] terrainTextures1d() { return game.terrainTextures1d; }
    public int terrainTexturesPerAtlas() { return game.terrainTexturesPerAtlas; }
}

public class Config3d
{
    public Config3d()
    {
        ENABLE_BACKFACECULLING = true;
        ENABLE_TRANSPARENCY = true;
        ENABLE_MIPMAPS = true;
        ENABLE_VISIBILITY_CULLING = false;
        viewdistance = 128;
    }
    internal bool ENABLE_BACKFACECULLING;
    internal bool ENABLE_TRANSPARENCY;
    internal bool ENABLE_MIPMAPS;
    internal bool ENABLE_VISIBILITY_CULLING;
    internal float viewdistance;
    public float GetViewDistance() { return viewdistance; }
    public void SetViewDistance(float value) { viewdistance = value; }
    public bool GetEnableTransparency() { return ENABLE_TRANSPARENCY; }
    public void SetEnableTransparency(bool value) { ENABLE_TRANSPARENCY = value; }
    public bool GetEnableMipmaps() { return ENABLE_MIPMAPS; }
    public void SetEnableMipmaps(bool value) { ENABLE_MIPMAPS = value; }
}

public class MapUtilCi
{
    public static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }

    public static int Index2d(int x, int y, int sizex)
    {
        return x + y * sizex;
    }

    public static void Pos(int index, int sizex, int sizey, Vector3Ref ret)
    {
        int x = index % sizex;
        int y = (index / sizex) % sizey;
        int h = index / (sizex * sizey);
        ret.X = x;
        ret.Y = y;
        ret.Z = h;
    }

    internal static void PosInt(int index, int sizex, int sizey, Vector3IntRef ret)
    {
        int x = index % sizex;
        int y = (index / sizex) % sizey;
        int h = index / (sizex * sizey);
        ret.X = x;
        ret.Y = y;
        ret.Z = h;
    }

    public static int PosX(int index, int sizex, int sizey)
    {
        return index % sizex;
    }

    public static int PosY(int index, int sizex, int sizey)
    {
        return (index / sizex) % sizey;
    }

    public static int PosZ(int index, int sizex, int sizey)
    {
        return index / (sizex * sizey);
    }
}

public class InfiniteMapChunked2d
{
    internal Game d_Map;
    public const int chunksize = 16;
    internal int[][] chunks;
    public int GetBlock(int x, int y)
    {
        int[] chunk = GetChunk(x, y);
        return chunk[MapUtilCi.Index2d(x % chunksize, y % chunksize, chunksize)];
    }
    public int[] GetChunk(int x, int y)
    {
        int[] chunk = null;
        int kx = x / chunksize;
        int ky = y / chunksize;
        if (chunks[MapUtilCi.Index2d(kx, ky, d_Map.map.MapSizeX / chunksize)] == null)
        {
            chunk = new int[chunksize * chunksize];// (byte*)Marshal.AllocHGlobal(chunksize * chunksize);
            for (int i = 0; i < chunksize * chunksize; i++)
            {
                chunk[i] = 0;
            }
            chunks[MapUtilCi.Index2d(kx, ky, d_Map.map.MapSizeX / chunksize)] = chunk;
        }
        chunk = chunks[MapUtilCi.Index2d(kx, ky, d_Map.map.MapSizeX / chunksize)];
        return chunk;
    }
    public void SetBlock(int x, int y, int blocktype)
    {
        GetChunk(x, y)[MapUtilCi.Index2d(x % chunksize, y % chunksize, chunksize)] = blocktype;
    }
    public void Restart()
    {
        //chunks = new byte[d_Map.MapSizeX / chunksize, d_Map.MapSizeY / chunksize][,];
        int n = (d_Map.map.MapSizeX / chunksize) * (d_Map.map.MapSizeY / chunksize);
        chunks = new int[n][];//(byte**)Marshal.AllocHGlobal(n * sizeof(IntPtr));
        for (int i = 0; i < n; i++)
        {
            chunks[i] = null;
        }
    }
    public void ClearChunk(int x, int y)
    {
        int px = x / chunksize;
        int py = y / chunksize;
        chunks[MapUtilCi.Index2d(px, py, d_Map.map.MapSizeX / chunksize)] = null;
    }
}

public abstract class ClientModManager
{
    public abstract void MakeScreenshot();
    public abstract void SetLocalPosition(float glx, float gly, float glz);
    public abstract float GetLocalPositionX();
    public abstract float GetLocalPositionY();
    public abstract float GetLocalPositionZ();
    public abstract void SetLocalOrientation(float glx, float gly, float glz);
    public abstract float GetLocalOrientationX();
    public abstract float GetLocalOrientationY();
    public abstract float GetLocalOrientationZ();
    public abstract void DisplayNotification(string message);
    public abstract void SendChatMessage(string message);
    public abstract GamePlatform GetPlatform();
    public abstract void ShowGui(int level);
    public abstract void SetFreemove(int level);
    public abstract int GetFreemove();
    public abstract BitmapCi GrabScreenshot();
    public abstract AviWriterCi AviWriterCreate();
    public abstract int GetWindowWidth();
    public abstract int GetWindowHeight();
    public abstract bool IsFreemoveAllowed();
    public abstract void EnableCameraControl(bool enable);
    public abstract int WhiteTexture();
    public abstract void Draw2dTexture(int textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int color);
    public abstract void Draw2dTextures(Draw2dData[] todraw, int todrawLength, int textureId);
    public abstract void Draw2dText(string text, float x, float y, float fontsize);
    public abstract void OrthoMode();
    public abstract void PerspectiveMode();
    public abstract DictionaryStringString GetPerformanceInfo();
}

public class ClientModManager1 : ClientModManager
{
    internal Game game;

    public override void MakeScreenshot()
    {
        game.platform.SaveScreenshot();
    }

    public override void SetLocalPosition(float glx, float gly, float glz)
    {
        game.player.position.x = glx;
        game.player.position.y = gly;
        game.player.position.z = glz;
    }

    public override float GetLocalPositionX()
    {
        return game.player.position.x;
    }

    public override float GetLocalPositionY()
    {
        return game.player.position.y;
    }

    public override float GetLocalPositionZ()
    {
        return game.player.position.z;
    }

    public override void SetLocalOrientation(float glx, float gly, float glz)
    {
        game.player.position.rotx = glx;
        game.player.position.roty = gly;
        game.player.position.rotz = glz;
    }

    public override float GetLocalOrientationX()
    {
        return game.player.position.rotx;
    }

    public override float GetLocalOrientationY()
    {
        return game.player.position.roty;
    }

    public override float GetLocalOrientationZ()
    {
        return game.player.position.rotz;
    }

    public override void DisplayNotification(string message)
    {
        game.AddChatline(message);
    }

    public override void SendChatMessage(string message)
    {
        game.SendChat(message);
    }

    public override GamePlatform GetPlatform()
    {
        return game.platform;
    }

    public override void ShowGui(int level)
    {
        if (level == 0)
        {
            game.ENABLE_DRAW2D = false;
        }
        else
        {
            game.ENABLE_DRAW2D = true;
        }
    }

    public override void SetFreemove(int level)
    {
        game.controls.SetFreemove(level);
    }

    public override int GetFreemove()
    {
        return game.controls.GetFreemove();
    }

    public override BitmapCi GrabScreenshot()
    {
        return game.platform.GrabScreenshot();
    }

    public override AviWriterCi AviWriterCreate()
    {
        return game.platform.AviWriterCreate();
    }

    public override int GetWindowWidth()
    {
        return game.platform.GetCanvasWidth();
    }

    public override int GetWindowHeight()
    {
        return game.platform.GetCanvasHeight();
    }

    public override bool IsFreemoveAllowed()
    {
        return game.AllowFreemove;
    }

    public override void EnableCameraControl(bool enable)
    {
        game.enableCameraControl = enable;
    }

    public override int WhiteTexture()
    {
        return game.WhiteTexture();
    }

    public override void Draw2dTexture(int textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int color)
    {
        int a = Game.ColorA(color);
        int r = Game.ColorR(color);
        int g = Game.ColorG(color);
        int b = Game.ColorB(color);
        game.Draw2dTexture(textureid, game.platform.FloatToInt(x1), game.platform.FloatToInt(y1),
            game.platform.FloatToInt(width), game.platform.FloatToInt(height),
            inAtlasId, 0, Game.ColorFromArgb(a, r, g, b), false);
    }

    public override void Draw2dTextures(Draw2dData[] todraw, int todrawLength, int textureId)
    {
        game.Draw2dTextures(todraw, todrawLength, textureId);
    }


    public override void Draw2dText(string text, float x, float y, float fontsize)
    {
        FontCi font = new FontCi();
        font.family = "Arial";
        font.size = fontsize;
        game.Draw2dText(text, font, x, y, null, false);
    }

    public override void OrthoMode()
    {
        game.OrthoMode(GetWindowWidth(), GetWindowHeight());
    }

    public override void PerspectiveMode()
    {
        game.PerspectiveMode();
    }

    public override DictionaryStringString GetPerformanceInfo()
    {
        return game.performanceinfo;
    }
}

public abstract class AviWriterCi
{
    public abstract void Open(string filename, int framerate, int width, int height);
    public abstract void AddFrame(BitmapCi bitmap);
    public abstract void Close();
}

public class BitmapCi
{
    public virtual void Dispose() { }
}

public abstract class ClientMod
{
    public virtual void Start(ClientModManager modmanager) { }
    
    public virtual void OnReadOnlyMainThread(Game game, float dt) { }
    public virtual void OnReadOnlyBackgroundThread(Game game, float dt) { }
    public virtual void OnReadWriteMainThread(Game game, float dt) { }
    
    public virtual bool OnClientCommand(Game game, ClientCommandArgs args) { return false; }
    public virtual void OnNewFrame(Game game, NewFrameEventArgs args) { }
    public virtual void OnNewFrameFixed(Game game, NewFrameEventArgs args) { }
    public virtual void OnNewFrameDraw2d(Game game, float deltaTime) { }
    public virtual void OnBeforeNewFrameDraw3d(Game game, float deltaTime) { }
    public virtual void OnNewFrameDraw3d(Game game, float deltaTime) { }
    public virtual void OnNewFrameReadOnlyMainThread(Game game, float deltaTime) { }
    public virtual void OnKeyDown(Game game, KeyEventArgs args) { }
    public virtual void OnKeyPress(Game game, KeyPressEventArgs args) { }
    public virtual void OnKeyUp(Game game, KeyEventArgs args) { }
    public virtual void OnMouseUp(Game game, MouseEventArgs args) { }
    public virtual void OnMouseDown(Game game, MouseEventArgs args) { }
    public virtual void OnMouseMove(Game game, MouseEventArgs args) { }
    public virtual void OnMouseWheelChanged(Game game, MouseWheelEventArgs args) { }
    public virtual void OnTouchStart(Game game, TouchEventArgs e) { }
    public virtual void OnTouchMove(Game game, TouchEventArgs e) { }
    public virtual void OnTouchEnd(Game game, TouchEventArgs e) { }
    public virtual void OnUseEntity(Game game, OnUseEntityArgs e) { }
    public virtual void OnHitEntity(Game game, OnUseEntityArgs e) { }
    public virtual void Dispose(Game game) { }
}

public class ModDrawMain : ClientMod
{
    public override void OnReadOnlyMainThread(Game game, float dt)
    {
        game.MainThreadOnRenderFrame(dt);
    }
}

public class ModUpdateMain : ClientMod
{
    // Should use ReadWrite to be correct but that would be too slow
    public override void OnReadOnlyMainThread(Game game, float dt)
    {
        game.Update(dt);
    }
}

public abstract class EntityScript
{
    public virtual void OnNewFrameFixed(Game game, int entity, float dt) { }
}

public class OnUseEntityArgs
{
    internal int entityId;
}

public class ClientCommandArgs
{
    internal string command;
    internal string arguments;
}

public class TextureAtlasCi
{
    public static void TextureCoords2d(int textureId, int texturesPacked, RectFRef r)
    {
        float one = 1;
        r.y = (one / texturesPacked * (textureId / texturesPacked));
        r.x = (one / texturesPacked * (textureId % texturesPacked));
        r.w = one / texturesPacked;
        r.h = one / texturesPacked;
    }
}

public class StackMatrix4
{
    public StackMatrix4()
    {
        values = new float[max][];
        for (int i = 0; i < max; i++)
        {
            values[i] = Mat4.Create();
        }
    }
    float[][] values;
    const int max = 1024;
    int count_;

    internal void Push(float[] p)
    {
        Mat4.Copy(values[count_], p);
        count_++;
    }

    internal float[] Peek()
    {
        return values[count_ - 1];
    }

    internal int Count()
    {
        return count_;
    }

    internal float[] Pop()
    {
        float[] ret = values[count_ - 1];
        count_--;
        return ret;
    }
}

public class CachedTexture
{
    internal int textureId;
    internal float sizeX;
    internal float sizeY;
    internal int lastuseMilliseconds;
}

public class Text_
{
    internal string text;
    internal float fontsize;
    internal int color;
    internal string fontfamily;
    internal int fontstyle;

    internal bool Equals_(Text_ t)
    {
        return this.text == t.text
            && this.fontsize == t.fontsize
            && this.color == t.color
            && this.fontfamily == t.fontfamily
            && this.fontstyle == t.fontstyle;
    }

    public string GetText() { return text; } public void SetText(string value) { text = value; }
    public float GetFontSize() { return fontsize; } public void SetFontSize(float value) { fontsize = value; }
    public int GetColor() { return color; } public void SetColor(int value) { color = value; }
    public string GetFontFamily() { return fontfamily; } public void SetFontFamily(string value) { fontfamily = value; }
    public int GetFontStyle() { return fontstyle; } public void SetFontStyle(int value) { fontstyle = value; }
}

public class CachedTextTexture
{
    internal Text_ text;
    internal CachedTexture texture;
}

public class FontCi
{
    internal string family;
    internal float size;
    internal int style;

    internal static FontCi Create(string family_, float size_, int style_)
    {
        FontCi f = new FontCi();
        f.family = family_;
        f.size = size_;
        f.style = style_;
        return f;
    }
}

public class TextPart
{
    internal int color;
    internal string text;
}

public class TextColorRenderer
{
    internal GamePlatform platform;

    internal BitmapCi CreateTextTexture(Text_ t)
    {
        IntRef partsCount = new IntRef();
        TextPart[] parts = DecodeColors(t.text, t.color, partsCount);

        float totalwidth = 0;
        float totalheight = 0;
        int[] sizesX = new int[partsCount.value];
        int[] sizesY = new int[partsCount.value];

        for (int i = 0; i < partsCount.value; i++)
        {
            IntRef outWidth = new IntRef();
            IntRef outHeight = new IntRef();
            platform.TextSize(parts[i].text, t.fontsize, outWidth, outHeight);

            sizesX[i] = outWidth.value;
            sizesY[i] = outHeight.value;

            totalwidth += outWidth.value;
            totalheight = MathCi.MaxFloat(totalheight, outHeight.value);
        }

        int size2X = NextPowerOfTwo(platform.FloatToInt(totalwidth) + 1);
        int size2Y = NextPowerOfTwo(platform.FloatToInt(totalheight) + 1);
        BitmapCi bmp2 = platform.BitmapCreate(size2X, size2Y);
        int[] bmp2Pixels = new int[size2X * size2Y];

        float currentwidth = 0;
        for (int i = 0; i < partsCount.value; i++)
        {
            int sizeiX = sizesX[i];
            int sizeiY = sizesY[i];
            if (sizeiX == 0 || sizeiY == 0)
            {
                continue;
            }
            Text_ partText = new Text_();
            partText.text = parts[i].text;
            partText.color = parts[i].color;
            partText.fontsize = t.fontsize;
            partText.fontstyle = t.fontstyle;
            partText.fontfamily = t.fontfamily;
            BitmapCi partBmp = platform.CreateTextTexture(partText);
            int partWidth = platform.FloatToInt(platform.BitmapGetWidth(partBmp));
            int partHeight = platform.FloatToInt(platform.BitmapGetHeight(partBmp));
            int[] partBmpPixels = new int[partWidth * partHeight];
            platform.BitmapGetPixelsArgb(partBmp, partBmpPixels);
            for (int x = 0; x < partWidth; x++)
            {
                for (int y = 0; y < partHeight; y++)
                {
                    if (x + currentwidth >= size2X) { continue; }
                    if (y >= size2Y) { continue; }
                    int c = partBmpPixels[MapUtilCi.Index2d(x, y, partWidth)];
                    if (Game.ColorA(c) > 0)
                    {
                        bmp2Pixels[MapUtilCi.Index2d(platform.FloatToInt(currentwidth) + x, y, size2X)] = c;
                    }
                }
            }
            currentwidth += sizeiX;
        }
        platform.BitmapSetPixelsArgb(bmp2, bmp2Pixels);
        return bmp2;
    }

    public TextPart[] DecodeColors(string s, int defaultcolor, IntRef retLength)
    {
        TextPart[] parts = new TextPart[256];
        int partsCount = 0;
        int currentcolor = defaultcolor;
        int[] currenttext = new int[256];
        int currenttextLength = 0;
        IntRef sLength = new IntRef();
        int[] sChars = platform.StringToCharArray(s, sLength);
        for (int i = 0; i < sLength.value; i++)
        {
            // If a & is found, try to parse a color code
            if (sChars[i] == '&')
            {
                //check if there's a character after it
                if (i + 1 < sLength.value)
                {
                    //try to parse the color code
                    int color = HexToInt(sChars[i + 1]);
                    if (color != -1)
                    {
                        //Color has been parsed successfully
                        if (currenttextLength != 0)
                        {
                            //Add content so far to return value
                            TextPart part = new TextPart();
                            part.text = platform.CharArrayToString(currenttext, currenttextLength);
                            part.color = currentcolor;
                            parts[partsCount++] = part;
                        }
                        //Update current color and reset stored text
                        for (int k = 0; k < currenttextLength; k++)
                        {
                            currenttext[k] = 0;
                        }
                        currenttextLength = 0;
                        currentcolor = GetColor(color);
                        //Increment i to prevent the code from being read again
                        i++;
                    }
                    else
                    {
                        //no valid color code found. display as normal character
                        currenttext[currenttextLength++] = sChars[i];
                    }
                }
                else
                {
                    //if not, just display it as normal character
                    currenttext[currenttextLength++] = sChars[i];
                }
            }
            else
            {
                //Nothing special. Just add the current character
                currenttext[currenttextLength++] = s[i];
            }
        }
        //Add any leftover text parts in current color
        if (currenttextLength != 0)
        {
            TextPart part = new TextPart();
            part.text = platform.CharArrayToString(currenttext, currenttextLength);
            part.color = currentcolor;
            parts[partsCount++] = part;
        }
        retLength.value = partsCount;
        return parts;
    }

    int NextPowerOfTwo(int x)
    {
        x--;
        x |= x >> 1;  // handle  2 bit numbers
        x |= x >> 2;  // handle  4 bit numbers
        x |= x >> 4;  // handle  8 bit numbers
        x |= x >> 8;  // handle 16 bit numbers
        //x |= x >> 16; // handle 32 bit numbers
        x++;
        return x;
    }

    int GetColor(int currentcolor)
    {
        switch (currentcolor)
        {
            case 0: { return Game.ColorFromArgb(255, 0, 0, 0); }
            case 1: { return Game.ColorFromArgb(255, 0, 0, 191); }
            case 2: { return Game.ColorFromArgb(255, 0, 191, 0); }
            case 3: { return Game.ColorFromArgb(255, 0, 191, 191); }
            case 4: { return Game.ColorFromArgb(255, 191, 0, 0); }
            case 5: { return Game.ColorFromArgb(255, 191, 0, 191); }
            case 6: { return Game.ColorFromArgb(255, 191, 191, 0); }
            case 7: { return Game.ColorFromArgb(255, 191, 191, 191); }
            case 8: { return Game.ColorFromArgb(255, 40, 40, 40); }
            case 9: { return Game.ColorFromArgb(255, 64, 64, 255); }
            case 10: { return Game.ColorFromArgb(255, 64, 255, 64); }
            case 11: { return Game.ColorFromArgb(255, 64, 255, 255); }
            case 12: { return Game.ColorFromArgb(255, 255, 64, 64); }
            case 13: { return Game.ColorFromArgb(255, 255, 64, 255); }
            case 14: { return Game.ColorFromArgb(255, 255, 255, 64); }
            case 15: { return Game.ColorFromArgb(255, 255, 255, 255); }
            default: return Game.ColorFromArgb(255, 255, 255, 255);
        }
    }

    int HexToInt(int c)
    {
        if (c == '0') { return 0; }
        if (c == '1') { return 1; }
        if (c == '2') { return 2; }
        if (c == '3') { return 3; }
        if (c == '4') { return 4; }
        if (c == '5') { return 5; }
        if (c == '6') { return 6; }
        if (c == '7') { return 7; }
        if (c == '8') { return 8; }
        if (c == '9') { return 9; }
        if (c == 'a') { return 10; }
        if (c == 'b') { return 11; }
        if (c == 'c') { return 12; }
        if (c == 'd') { return 13; }
        if (c == 'e') { return 14; }
        if (c == 'f') { return 15; }
        return -1;
    }
}

public class CameraMove
{
    internal bool TurnLeft;
    internal bool TurnRight;
    internal bool DistanceUp;
    internal bool DistanceDown;
    internal bool AngleUp;
    internal bool AngleDown;
    internal int MoveX;
    internal int MoveY;
    internal float Distance;
}

public class Kamera
{
    public Kamera()
    {
        one = 1;
        distance = 5;
        Angle = 45;
        MinimumDistance = 2;
        tt = 0;
        MaximumAngle = 89;
        MinimumAngle = 0;
        Center = new Vector3Ref();
    }
    float one;
    public void GetPosition(GamePlatform platform, Vector3Ref ret)
    {
        float cx = platform.MathCos(tt * one / 2) * GetFlatDistance(platform) + Center.X;
        float cy = platform.MathSin(tt * one / 2) * GetFlatDistance(platform) + Center.Z;
        ret.X = cx;
        ret.Y = Center.Y + GetCameraHeightFromCenter(platform);
        ret.Z = cy;
    }
    float distance;
    public float GetDistance() { return distance; }
    public void SetDistance(float value)
    {
        distance = value;
        if (distance < MinimumDistance)
        {
            distance = MinimumDistance;
        }
    }
    internal float Angle;
    internal float MinimumDistance;
    float GetCameraHeightFromCenter(GamePlatform platform)
    {
        return platform.MathSin(Angle * Game.GetPi() / 180) * distance;
    }
    float GetFlatDistance(GamePlatform platform)
    {
        return platform.MathCos(Angle * Game.GetPi() / 180) * distance;
    }
    internal Vector3Ref Center;
    internal float tt;
    public float GetT()
    {
        return tt;
    }
    public void SetT(float value)
    {
        tt = value;
    }
    public void TurnLeft(float p)
    {
        tt += p;
    }
    public void TurnRight(float p)
    {
        tt -= p;
    }
    public void Move(CameraMove camera_move, float p)
    {
        p *= 2;
        p *= 2;
        if (camera_move.TurnLeft)
        {
            TurnLeft(p);
        }
        if (camera_move.TurnRight)
        {
            TurnRight(p);
        }
        if (camera_move.DistanceUp)
        {
            SetDistance(GetDistance() + p);
        }
        if (camera_move.DistanceDown)
        {
            SetDistance(GetDistance() - p);
        }
        if (camera_move.AngleUp)
        {
            Angle += p * 10;
        }
        if (camera_move.AngleDown)
        {
            Angle -= p * 10;
        }
        SetDistance(camera_move.Distance);
        //if (MaximumAngle < MinimumAngle) { throw new Exception(); }
        SetValidAngle();
    }

    void SetValidAngle()
    {
        if (Angle > MaximumAngle) { Angle = MaximumAngle; }
        if (Angle < MinimumAngle) { Angle = MinimumAngle; }
    }

    internal int MaximumAngle;
    internal int MinimumAngle;

    public float GetAngle()
    {
        return Angle;
    }

    public void SetAngle(float value)
    {
        Angle = value;
    }

    public void GetCenter(Vector3Ref ret)
    {
        ret.X = Center.X;
        ret.Y = Center.Y;
        ret.Z = Center.Z;
    }

    public void TurnUp(float p)
    {
        Angle += p;
        SetValidAngle();
    }
}

public abstract class IMapStorage2
{
    public abstract int GetMapSizeX();
    public abstract int GetMapSizeY();
    public abstract int GetMapSizeZ();
    public abstract int GetBlock(int x, int y, int z);
    public abstract void SetBlock(int x, int y, int z, int tileType);
}

public class MapStorage2 : IMapStorage2
{
    public static MapStorage2 Create(Game game)
    {
        MapStorage2 s = new MapStorage2();
        s.game = game;
        return s;
    }
    Game game;
    public override int GetMapSizeX()
    {
        return game.map.MapSizeX;
    }

    public override int GetMapSizeY()
    {
        return game.map.MapSizeY;
    }

    public override int GetMapSizeZ()
    {
        return game.map.MapSizeZ;
    }

    public override int GetBlock(int x, int y, int z)
    {
        return game.map.GetBlock(x, y, z);
    }

    public override void SetBlock(int x, int y, int z, int tileType)
    {
        game.SetBlock(x, y, z, tileType);
    }
}

public class GameDataMonsters
{
    public GameDataMonsters()
    {
        int n = 5;
        MonsterCode = new string[n];
        MonsterName = new string[n];
        MonsterSkin = new string[n];
        MonsterCode[0] = "imp.txt";
        MonsterName[0] = "Imp";
        MonsterSkin[0] = "imp.png";
        MonsterCode[1] = "imp.txt";
        MonsterName[1] = "Fire Imp";
        MonsterSkin[1] = "impfire.png";
        MonsterCode[2] = "dragon.txt";
        MonsterName[2] = "Dragon";
        MonsterSkin[2] = "dragon.png";
        MonsterCode[3] = "zombie.txt";
        MonsterName[3] = "Zombie";
        MonsterSkin[3] = "zombie.png";
        MonsterCode[4] = "cyclops.txt";
        MonsterName[4] = "Cyclops";
        MonsterSkin[4] = "cyclops.png";
    }
    internal string[] MonsterName;
    internal string[] MonsterCode;
    internal string[] MonsterSkin;
}

public enum GuiState
{
    Normal,
    EscapeMenu,
    Inventory,
    MapLoading,
    CraftingRecipes,
    ModalDialog
}

public enum BlockSetMode
{
    Destroy,
    Create,
    Use, //open doors, use crafting table, etc.
    UseWithTool
}

public enum FontType
{
    Nice,
    Simple,
    BlackBackground,
    Default
}

public class SpecialBlockId
{
    public const int Empty = 0;
}

public class GameData
{
    public GameData()
    {
        mBlockIdEmpty = 0;
        mBlockIdDirt = -1;
        mBlockIdSponge = -1;
        mBlockIdTrampoline = -1;
        mBlockIdAdminium = -1;
        mBlockIdCompass = -1;
        mBlockIdLadder = -1;
        mBlockIdEmptyHand = -1;
        mBlockIdCraftingTable = -1;
        mBlockIdLava = -1;
        mBlockIdStationaryLava = -1;
        mBlockIdFillStart = -1;
        mBlockIdCuboid = -1;
        mBlockIdFillArea = -1;
        mBlockIdMinecart = -1;
        mBlockIdRailstart = -128; // 64 rail tiles
    }
    public void Start()
    {
        Initialize(GlobalVar.MAX_BLOCKTYPES);
    }
    public void Update()
    {
    }
    void Initialize(int count)
    {
        mWhenPlayerPlacesGetsConvertedTo = new int[count];
        mIsFlower = new bool[count];
        mRail = new int[count];
        mWalkSpeed = new float[count];
        for (int i = 0; i < count; i++)
        {
            mWalkSpeed[i] = 1;
        }
        mIsSlipperyWalk = new bool[count];
        mWalkSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mWalkSound[i] = new string[SoundCount];
        }
        mBreakSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mBreakSound[i] = new string[SoundCount];
        }
        mBuildSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mBuildSound[i] = new string[SoundCount];
        }
        mCloneSound = new string[count][];
        for (int i = 0; i < count; i++)
        {
            mCloneSound[i] = new string[SoundCount];
        }
        mLightRadius = new int[count];
        mStartInventoryAmount = new int[count];
        mStrength = new float[count];
        mDamageToPlayer = new int[count];
        mWalkableType = new int[count];

        mDefaultMaterialSlots = new int[10];
    }

    public int[] WhenPlayerPlacesGetsConvertedTo() { return mWhenPlayerPlacesGetsConvertedTo; }
    public bool[] IsFlower() { return mIsFlower; }
    public int[] Rail() { return mRail; }
    public float[] WalkSpeed() { return mWalkSpeed; }
    public bool[] IsSlipperyWalk() { return mIsSlipperyWalk; }
    public string[][] WalkSound() { return mWalkSound; }
    public string[][] BreakSound() { return mBreakSound; }
    public string[][] BuildSound() { return mBuildSound; }
    public string[][] CloneSound() { return mCloneSound; }
    public int[] LightRadius() { return mLightRadius; }
    public int[] StartInventoryAmount() { return mStartInventoryAmount; }
    public float[] Strength() { return mStrength; }
    public int[] DamageToPlayer() { return mDamageToPlayer; }
    public int[] WalkableType1() { return mWalkableType; }

    public int[] DefaultMaterialSlots() { return mDefaultMaterialSlots; }

    int[] mWhenPlayerPlacesGetsConvertedTo;
    bool[] mIsFlower;
    int[] mRail;
    float[] mWalkSpeed;
    bool[] mIsSlipperyWalk;
    string[][] mWalkSound;
    string[][] mBreakSound;
    string[][] mBuildSound;
    string[][] mCloneSound;
    int[] mLightRadius;
    int[] mStartInventoryAmount;
    float[] mStrength;
    int[] mDamageToPlayer;
    int[] mWalkableType;

    int[] mDefaultMaterialSlots;

    // TODO: hardcoded IDs
    // few code sections still expect some hardcoded IDs
    int mBlockIdEmpty;
    int mBlockIdDirt;
    int mBlockIdSponge;
    int mBlockIdTrampoline;
    int mBlockIdAdminium;
    int mBlockIdCompass;
    int mBlockIdLadder;
    int mBlockIdEmptyHand;
    int mBlockIdCraftingTable;
    int mBlockIdLava;
    int mBlockIdStationaryLava;
    int mBlockIdFillStart;
    int mBlockIdCuboid;
    int mBlockIdFillArea;
    int mBlockIdMinecart;
    int mBlockIdRailstart; // 64 rail tiles

    public int BlockIdEmpty() { return mBlockIdEmpty; }
    public int BlockIdDirt() { return mBlockIdDirt; }
    public int BlockIdSponge() { return mBlockIdSponge; }
    public int BlockIdTrampoline() { return mBlockIdTrampoline; }
    public int BlockIdAdminium() { return mBlockIdAdminium; }
    public int BlockIdCompass() { return mBlockIdCompass; }
    public int BlockIdLadder() { return mBlockIdLadder; }
    public int BlockIdEmptyHand() { return mBlockIdEmptyHand; }
    public int BlockIdCraftingTable() { return mBlockIdCraftingTable; }
    public int BlockIdLava() { return mBlockIdLava; }
    public int BlockIdStationaryLava() { return mBlockIdStationaryLava; }
    public int BlockIdFillStart() { return mBlockIdFillStart; }
    public int BlockIdCuboid() { return mBlockIdCuboid; }
    public int BlockIdFillArea() { return mBlockIdFillArea; }
    public int BlockIdMinecart() { return mBlockIdMinecart; }
    public int BlockIdRailstart() { return mBlockIdRailstart; }

    // TODO: atm it sets sepcial block id from block name - better use new block property
    public bool SetSpecialBlock(Packet_BlockType b, int id)
    {
        switch (b.Name)
        {
            case "Empty":
                this.mBlockIdEmpty = id;
                return true;
            case "Dirt":
                this.mBlockIdDirt = id;
                return true;
            case "Sponge":
                this.mBlockIdSponge = id;
                return true;
            case "Trampoline":
                this.mBlockIdTrampoline = id;
                return true;
            case "Adminium":
                this.mBlockIdAdminium = id;
                return true;
            case "Compass":
                this.mBlockIdCompass = id;
                return true;
            case "Ladder":
                this.mBlockIdLadder = id;
                return true;
            case "EmptyHand":
                this.mBlockIdEmptyHand = id;
                return true;
            case "CraftingTable":
                this.mBlockIdCraftingTable = id;
                return true;
            case "Lava":
                this.mBlockIdLava = id;
                return true;
            case "StationaryLava":
                this.mBlockIdStationaryLava = id;
                return true;
            case "FillStart":
                this.mBlockIdFillStart = id;
                return true;
            case "Cuboid":
                this.mBlockIdCuboid = id;
                return true;
            case "FillArea":
                this.mBlockIdFillArea = id;
                return true;
            case "Minecart":
                this.mBlockIdMinecart = id;
                return true;
            case "Rail0":
                this.mBlockIdRailstart = id;
                return true;
            default:
                return false;
        }
    }

    public bool IsRailTile(int id)
    {
        return id >= BlockIdRailstart() && id < BlockIdRailstart() + 64;
    }

    public void UseBlockTypes(GamePlatform platform, Packet_BlockType[] blocktypes, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (blocktypes[i] != null)
            {
                UseBlockType(platform, i, blocktypes[i]);
            }
        }
    }

    public void UseBlockType(GamePlatform platform, int id, Packet_BlockType b)
    {
        if (b.Name == null)//!b.IsValid)
        {
            return;
        }
        //public bool[] IsWater { get { return mIsWater; } }
        //            public bool[] IsTransparentForLight { get { return mIsTransparentForLight; } }
        //public bool[] IsEmptyForPhysics { get { return mIsEmptyForPhysics; } }

        if (b.WhenPlacedGetsConvertedTo != 0)
        {
            mWhenPlayerPlacesGetsConvertedTo[id] = b.WhenPlacedGetsConvertedTo;
        }
        else
        {
            mWhenPlayerPlacesGetsConvertedTo[id] = id;
        }
        IsFlower()[id] = b.DrawType == Packet_DrawTypeEnum.Plant;
        Rail()[id] = b.Rail;
        WalkSpeed()[id] = DeserializeFloat(b.WalkSpeedFloat);
        IsSlipperyWalk()[id] = b.IsSlipperyWalk;
        WalkSound()[id] = new string[SoundCount];
        BreakSound()[id] = new string[SoundCount];
        BuildSound()[id] = new string[SoundCount];
        CloneSound()[id] = new string[SoundCount];
        if (b.Sounds != null)
        {
            for (int i = 0; i < b.Sounds.WalkCount; i++)
            {
                WalkSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Walk[i], ".wav");
            }
            for (int i = 0; i < b.Sounds.Break1Count; i++)
            {
                BreakSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Break1[i], ".wav");
            }
            for (int i = 0; i < b.Sounds.BuildCount; i++)
            {
                BuildSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Build[i], ".wav");
            }
            for (int i = 0; i < b.Sounds.CloneCount; i++)
            {
                CloneSound()[id][i] = StringTools.StringAppend(platform, b.Sounds.Clone[i], ".wav");
            }
        }
        LightRadius()[id] = b.LightRadius;
        //StartInventoryAmount { get; }
        Strength()[id] = b.Strength;
        DamageToPlayer()[id] = b.DamageToPlayer;
        WalkableType1()[id] = b.WalkableType;
        SetSpecialBlock(b, id);
    }

    public const int SoundCount = 8;

    float DeserializeFloat(int p)
    {
        float one = 1;
        return (one * p) / 32;
    }
}

public class OnCrashHandlerLeave : OnCrashHandler
{
    public static OnCrashHandlerLeave Create(Game game)
    {
        OnCrashHandlerLeave oncrash = new OnCrashHandlerLeave();
        oncrash.g = game;
        return oncrash;
    }
    Game g;
    public override void OnCrash()
    {
        g.SendLeave(Packet_LeaveReasonEnum.Crash);
    }
}

public class OptionsCi
{
    public OptionsCi()
    {
        float one = 1;
        Shadows = false;
        Font = 0;
        DrawDistance = 32;
        UseServerTextures = true;
        EnableSound = true;
        EnableAutoJump = false;
        ClientLanguage = "";
        Framerate = 0;
        Resolution = 0;
        Fullscreen = false;
        Smoothshadows = true;
        BlockShadowSave = one * 6 / 10;
        EnableBlockShadow = true;
        Keys = new int[256];
    }
    internal bool Shadows;
    internal int Font;
    internal int DrawDistance;
    internal bool UseServerTextures;
    internal bool EnableSound;
    internal bool EnableAutoJump;
    internal string ClientLanguage;
    internal int Framerate;
    internal int Resolution;
    internal bool Fullscreen;
    internal bool Smoothshadows;
    internal float BlockShadowSave;
    internal bool EnableBlockShadow;
    internal int[] Keys;
}

public class TextureAtlas
{
    public static RectFRef TextureCoords2d(int textureId, int texturesPacked)
    {
        float one = 1;
        RectFRef r = new RectFRef();
        r.y = (one / texturesPacked * (textureId / texturesPacked));
        r.x = (one / texturesPacked * (textureId % texturesPacked));
        r.w = one / texturesPacked;
        r.h = one / texturesPacked;
        return r;
    }
}

public abstract class ClientPacketHandler
{
    public ClientPacketHandler()
    {
        one = 1;
    }
    internal float one;
    public abstract void Handle(Game game, Packet_Server packet);
}

public class Map
{
    internal Chunk[] chunks;
    internal int MapSizeX;
    internal int MapSizeY;
    internal int MapSizeZ;


#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    public int GetBlockValid(int x, int y, int z)
    {
        int cx = x >> Game.chunksizebits;
        int cy = y >> Game.chunksizebits;
        int cz = z >> Game.chunksizebits;
        int chunkpos = Index3d(cx, cy, cz, MapSizeX >> Game.chunksizebits, MapSizeY >> Game.chunksizebits);
        if (chunks[chunkpos] == null)
        {
            return 0;
        }
        else
        {
            int pos = Index3d(x & (Game.chunksize - 1), y & (Game.chunksize - 1), z & (Game.chunksize - 1), Game.chunksize, Game.chunksize);
            return chunks[chunkpos].GetBlockInChunk(pos);
        }
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        x = x / Game.chunksize;
        y = y / Game.chunksize;
        z = z / Game.chunksize;
        return GetChunk_(x, y, z);
    }

    public Chunk GetChunk_(int cx, int cy, int cz)
    {
        int mapsizexchunks = MapSizeX / Game.chunksize;
        int mapsizeychunks = MapSizeY / Game.chunksize;
        Chunk chunk = chunks[Index3d(cx, cy, cz, mapsizexchunks, mapsizeychunks)];
        if (chunk == null)
        {
            Chunk c = new Chunk();
            c.data = new byte[Game.chunksize * Game.chunksize * Game.chunksize];
            c.baseLight = new byte[Game.chunksize * Game.chunksize * Game.chunksize];
            chunks[Index3d(cx, cy, cz, mapsizexchunks, mapsizeychunks)] = c;
            return chunks[Index3d(cx, cy, cz, mapsizexchunks, mapsizeychunks)];
        }
        return chunk;
    }

    public void SetBlockRaw(int x, int y, int z, int tileType)
    {
        Chunk chunk = GetChunk(x, y, z);
        int pos = Index3d(x % Game.chunksize, y % Game.chunksize, z % Game.chunksize, Game.chunksize, Game.chunksize);
        chunk.SetBlockInChunk(pos, tileType);
    }

    public void CopyChunk(Chunk chunk, int[] output)
    {
        int n = Game.chunksize * Game.chunksize * Game.chunksize;
        if (chunk.dataInt != null)
        {
            for (int i = 0; i < n; i++)
            {
                output[i] = chunk.dataInt[i];
            }
        }
        else
        {
            for (int i = 0; i < n; i++)
            {
                output[i] = chunk.data[i];
            }
        }
    }

    public void Reset(int sizex, int sizey, int sizez)
    {
        MapSizeX = sizex;
        MapSizeY = sizey;
        MapSizeZ = sizez;
        chunks = new Chunk[(sizex / Game.chunksize) * (sizey / Game.chunksize) * (sizez / Game.chunksize)];
    }

    public void GetMapPortion(int[] outPortion, int x, int y, int z, int portionsizex, int portionsizey, int portionsizez)
    {
        int outPortionCount = portionsizex * portionsizey * portionsizez;
        for (int i = 0; i < outPortionCount; i++)
        {
            outPortion[i] = 0;
        }

        //int chunksizebits = p.FloatToInt(p.MathLog(chunksize, 2));

        int mapchunksx = MapSizeX / Game.chunksize;
        int mapchunksy = MapSizeY / Game.chunksize;
        int mapchunksz = MapSizeZ / Game.chunksize;
        int mapsizechunks = mapchunksx * mapchunksy * mapchunksz;

        for (int xx = 0; xx < portionsizex; xx++)
        {
            for (int yy = 0; yy < portionsizey; yy++)
            {
                for (int zz = 0; zz < portionsizez; zz++)
                {
                    //Find chunk.
                    int cx = (x + xx) >> Game.chunksizebits;
                    int cy = (y + yy) >> Game.chunksizebits;
                    int cz = (z + zz) >> Game.chunksizebits;
                    //int cpos = MapUtil.Index3d(cx, cy, cz, MapSizeX / chunksize, MapSizeY / chunksize);
                    int cpos = (cz * mapchunksy + cy) * mapchunksx + cx;
                    //if (cpos < 0 || cpos >= ((MapSizeX / chunksize) * (MapSizeY / chunksize) * (MapSizeZ / chunksize)))
                    if (cpos < 0 || cpos >= mapsizechunks)
                    {
                        continue;
                    }
                    Chunk chunk = chunks[cpos];
                    if (chunk == null || !chunk.ChunkHasData())
                    {
                        continue;
                    }
                    //int pos = MapUtil.Index3d((x + xx) % chunksize, (y + yy) % chunksize, (z + zz) % chunksize, chunksize, chunksize);
                    int chunkGlobalX = cx << Game.chunksizebits;
                    int chunkGlobalY = cy << Game.chunksizebits;
                    int chunkGlobalZ = cz << Game.chunksizebits;

                    int inChunkX = (x + xx) - chunkGlobalX;
                    int inChunkY = (y + yy) - chunkGlobalY;
                    int inChunkZ = (z + zz) - chunkGlobalZ;

                    //int pos = MapUtil.Index3d(inChunkX, inChunkY, inChunkZ, chunksize, chunksize);
                    int pos = (((inChunkZ << Game.chunksizebits) + inChunkY) << Game.chunksizebits) + inChunkX;

                    int block = chunk.GetBlockInChunk(pos);
                    //outPortion[MapUtil.Index3d(xx, yy, zz, portionsizex, portionsizey)] = (byte)block;
                    outPortion[(zz * portionsizey + yy) * portionsizex + xx] = block;
                }
            }
        }
    }

    public bool IsValidPos(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0)
        {
            return false;
        }
        if (x >= MapSizeX || y >= MapSizeY || z >= MapSizeZ)
        {
            return false;
        }
        return true;
    }

    public bool IsValidChunkPos(int cx, int cy, int cz)
    {
        return cx >= 0 && cy >= 0 && cz >= 0
            && cx < MapSizeX / Game.chunksize
            && cy < MapSizeY / Game.chunksize
            && cz < MapSizeZ / Game.chunksize;
    }

    public int GetBlock(int x, int y, int z)
    {
        if (!IsValidPos(x, y, z))
        {
            return 0;
        }
        return GetBlockValid(x, y, z);
    }

    public void SetChunkDirty(int cx, int cy, int cz, bool dirty, bool blockschanged)
    {
        if (!IsValidChunkPos(cx, cy, cz))
        {
            return;
        }

        Chunk c = chunks[MapUtilCi.Index3d(cx, cy, cz, mapsizexchunks(), mapsizeychunks())];
        if (c == null)
        {
            return;
        }
        if (c.rendered == null)
        {
            c.rendered = new RenderedChunk();
        }
        c.rendered.dirty = dirty;
        if (blockschanged)
        {
            c.baseLightDirty = true;
        }
    }

    public int mapsizexchunks() { return MapSizeX >> Game.chunksizebits; }
    public int mapsizeychunks() { return MapSizeY >> Game.chunksizebits; }
    public int mapsizezchunks() { return MapSizeZ >> Game.chunksizebits; }

    public void SetChunksAroundDirty(int cx, int cy, int cz)
    {
        if (IsValidChunkPos(cx, cy, cz)) { SetChunkDirty(cx - 1, cy, cz, true, false); }
        if (IsValidChunkPos(cx - 1, cy, cz)) { SetChunkDirty(cx - 1, cy, cz, true, false); }
        if (IsValidChunkPos(cx + 1, cy, cz)) { SetChunkDirty(cx + 1, cy, cz, true, false); }
        if (IsValidChunkPos(cx, cy - 1, cz)) { SetChunkDirty(cx, cy - 1, cz, true, false); }
        if (IsValidChunkPos(cx, cy + 1, cz)) { SetChunkDirty(cx, cy + 1, cz, true, false); }
        if (IsValidChunkPos(cx, cy, cz - 1)) { SetChunkDirty(cx, cy, cz - 1, true, false); }
        if (IsValidChunkPos(cx, cy, cz + 1)) { SetChunkDirty(cx, cy, cz + 1, true, false); }
    }

    public void SetMapPortion(int x, int y, int z, int[] chunk, int sizeX, int sizeY, int sizeZ)
    {
        int chunksizex = sizeX;
        int chunksizey = sizeY;
        int chunksizez = sizeZ;
        //if (chunksizex % chunksize != 0) { platform.ThrowException(""); }
        //if (chunksizey % chunksize != 0) { platform.ThrowException(""); }
        //if (chunksizez % chunksize != 0) { platform.ThrowException(""); }
        int chunksize = Game.chunksize;
        Chunk[] localchunks = new Chunk[(chunksizex / chunksize) * (chunksizey / chunksize) * (chunksizez / chunksize)];
        for (int cx = 0; cx < chunksizex / chunksize; cx++)
        {
            for (int cy = 0; cy < chunksizey / chunksize; cy++)
            {
                for (int cz = 0; cz < chunksizex / chunksize; cz++)
                {
                    localchunks[Index3d(cx, cy, cz, (chunksizex / chunksize), (chunksizey / chunksize))] = GetChunk(x + cx * chunksize, y + cy * chunksize, z + cz * chunksize);
                    FillChunk(localchunks[Index3d(cx, cy, cz, (chunksizex / chunksize), (chunksizey / chunksize))], chunksize, cx * chunksize, cy * chunksize, cz * chunksize, chunk, sizeX, sizeY, sizeZ);
                }
            }
        }
        for (int xxx = 0; xxx < chunksizex; xxx += chunksize)
        {
            for (int yyy = 0; yyy < chunksizex; yyy += chunksize)
            {
                for (int zzz = 0; zzz < chunksizex; zzz += chunksize)
                {
                    SetChunkDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize, true, true);
                    SetChunksAroundDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize);
                }
            }
        }
    }

    public void FillChunk(Chunk destination, int destinationchunksize, int sourcex, int sourcey, int sourcez, int[] source, int sourcechunksizeX, int sourcechunksizeY, int sourcechunksizeZ)
    {
        for (int x = 0; x < destinationchunksize; x++)
        {
            for (int y = 0; y < destinationchunksize; y++)
            {
                for (int z = 0; z < destinationchunksize; z++)
                {
                    //if (x + sourcex < source.GetUpperBound(0) + 1
                    //    && y + sourcey < source.GetUpperBound(1) + 1
                    //    && z + sourcez < source.GetUpperBound(2) + 1)
                    {
                        destination.SetBlockInChunk(Index3d(x, y, z, destinationchunksize, destinationchunksize)
                            , source[Index3d(x + sourcex, y + sourcey, z + sourcez, sourcechunksizeX, sourcechunksizeY)]);
                    }
                }
            }
        }
    }

    public int MaybeGetLight(int x, int y, int z)
    {
        int light = -1;
        int cx = x / Game.chunksize;
        int cy = y / Game.chunksize;
        int cz = z / Game.chunksize;
        if (IsValidPos(x, y, z) && IsValidChunkPos(cx, cy, cz))
        {
            Chunk c = chunks[MapUtilCi.Index3d(cx, cy, cz, mapsizexchunks(), mapsizeychunks())];
            if (c == null
                || c.rendered == null
                || c.rendered.light == null)
            {
                light = -1;
            }
            else
            {
                light = c.rendered.light[MapUtilCi.Index3d((x % Game.chunksize) + 1, (y % Game.chunksize) + 1, (z % Game.chunksize) + 1, Game.chunksize + 2, Game.chunksize + 2)];
            }
        }
        return light;
    }

    public void SetBlockDirty(int x, int y, int z)
    {
        Vector3IntRef[] around = ModDrawTerrain.BlocksAround7(Vector3IntRef.Create(x, y, z));
        for (int i = 0; i < 7; i++)
        {
            Vector3IntRef a = around[i];
            int xx = a.X;
            int yy = a.Y;
            int zz = a.Z;
            if (xx < 0 || yy < 0 || zz < 0 || xx >= MapSizeX || yy >= MapSizeY || zz >= MapSizeZ)
            {
                return;
            }
            SetChunkDirty((xx / Game.chunksize), (yy / Game.chunksize), (zz / Game.chunksize), true, true);
        }
    }

    public bool IsChunkRendered(int cx, int cy, int cz)
    {
        Chunk c = chunks[MapUtilCi.Index3d(cx, cy, cz, mapsizexchunks(), mapsizeychunks())];
        if (c == null)
        {
            return false;
        }
        return c.rendered != null && c.rendered.ids != null;
    }
}
