public class ModAutoCamera : ClientMod
{
    public override void Start(ClientModManager modmanager)
    {
        m = modmanager;
        p = modmanager.GetPlatform();
        one = 1;
        cameraPoints = new CameraPoint[256];
        cameraPointsCount = 0;
        playingTime = -1;
        position = new float[3];
        orientation = new float[3];
    }
    ClientModManager m;
    GamePlatform p;

    float one;
    CameraPoint[] cameraPoints;
    int cameraPointsCount;
    float playingTime;
    
    public override bool OnClientCommand(Game game, ClientCommandArgs args)
    {
        if (args.command == "cam")
        {
            IntRef argumentsLength = new IntRef();
            string[] arguments = p.StringSplit(args.arguments, " ", argumentsLength);
            if (p.StringTrim(args.arguments) == "")
            {
                m.DisplayNotification("&6AutoCamera help.");
                m.DisplayNotification("&6.cam p&f - add a point to path");
                m.DisplayNotification("&6.cam start [real seconds]&f - play the path");
                m.DisplayNotification("&6.cam rec [real seconds] [video seconds]&f - play and record to .avi file");
                m.DisplayNotification("&6.cam stop&f - stop playing and recording");
                m.DisplayNotification("&6.cam clear&f - remove all points from path");
                m.DisplayNotification("&6.cam save&f - copy path points to clipboard");
                m.DisplayNotification("&6.cam load [points]&f - load path points");
                return true;
            }
            if (arguments[0] == "p")
            {
                m.DisplayNotification("Point defined.");
                CameraPoint point = new CameraPoint();
                point.positionGlX = m.GetLocalPositionX();
                point.positionGlY = m.GetLocalPositionY();
                point.positionGlZ = m.GetLocalPositionZ();
                point.orientationGlX = m.GetLocalOrientationX();
                point.orientationGlY = m.GetLocalOrientationY();
                point.orientationGlZ = m.GetLocalOrientationZ();
                cameraPoints[cameraPointsCount++] = point;
            }
            if (arguments[0] == "start" || arguments[0] == "play" || arguments[0] == "rec")
            {
                if (!m.IsFreemoveAllowed())
                {
                    m.DisplayNotification("Free move not allowed.");
                    return true;
                }
                if (cameraPointsCount == 0)
                {
                    m.DisplayNotification("No points defined. Enter points with \".cam p\" command.");
                    return true;
                }
                playingSpeed = 1;
                float totalRecTime = -1;
                if (arguments[0] == "rec")
                {
                    if (argumentsLength.value >= 3)
                    {
                        // video time
                        totalRecTime = p.FloatParse(arguments[2]);
                    }
                    avi = m.AviWriterCreate();
                    avi.Open(p.StringFormat("{0}.avi", p.Timestamp()), framerate, m.GetWindowWidth(), m.GetWindowHeight());
                }
                if (argumentsLength.value >= 2)
                {
                    // play time
                    float totalTime = p.FloatParse(arguments[1]);
                    playingSpeed = TotalDistance() / totalTime;

                    if (totalRecTime == -1)
                    {
                        recspeed = 10;
                    }
                    else
                    {
                        recspeed = totalTime / totalRecTime;
                    }
                }
                playingTime = 0;
                firstFrameDone = false;
                previousPositionX = m.GetLocalPositionX();
                previousPositionY = m.GetLocalPositionY();
                previousPositionZ = m.GetLocalPositionZ();
                previousOrientationX = m.GetLocalOrientationX();
                previousOrientationY = m.GetLocalOrientationY();
                previousOrientationZ = m.GetLocalOrientationZ();
                m.ShowGui(0);
                previousFreemove = m.GetFreemove();
                m.SetFreemove(FreemoveLevelEnum.Noclip);
                m.EnableCameraControl(false);
            }
            if (arguments[0] == "stop")
            {
                m.DisplayNotification("Camera stopped.");
                Stop();
            }
            if (arguments[0] == "clear")
            {
                m.DisplayNotification("Camera points cleared.");
                cameraPointsCount = 0;
                Stop();
            }
            if (arguments[0] == "save")
            {
                string s = "1,";
                for (int i = 0; i < cameraPointsCount; i++)
                {
                    CameraPoint point = cameraPoints[i];
                    s = p.StringFormat2("{0}{1},", s, p.IntToString(p.FloatToInt(point.positionGlX * 100)));
                    s = p.StringFormat2("{0}{1},", s, p.IntToString(p.FloatToInt(point.positionGlY * 100)));
                    s = p.StringFormat2("{0}{1},", s, p.IntToString(p.FloatToInt(point.positionGlZ * 100)));
                    s = p.StringFormat2("{0}{1},", s, p.IntToString(p.FloatToInt(point.orientationGlX * 1000)));
                    s = p.StringFormat2("{0}{1},", s, p.IntToString(p.FloatToInt(point.orientationGlY * 1000)));
                    s = p.StringFormat2("{0}{1}", s, p.IntToString(p.FloatToInt(point.orientationGlZ * 1000)));
                    if (i != cameraPointsCount - 1)
                    {
                        s = p.StringFormat("{0},", s);
                    }
                }
                p.ClipboardSetText(s);
                m.DisplayNotification("Camera points copied to clipboard.");
            }
            if (arguments[0] == "load")
            {
                IntRef pointsLength = new IntRef();
                string[] points = p.StringSplit(arguments[1], ",", pointsLength);
                int n = (pointsLength.value - 1) / 6;
                cameraPointsCount = 0;
                for (int i = 0; i < n; i++)
                {
                    CameraPoint point = new CameraPoint();
                    point.positionGlX = one * p.IntParse(points[1 + i * 6 + 0]) / 100;
                    point.positionGlY = one * p.IntParse(points[1 + i * 6 + 1]) / 100;
                    point.positionGlZ = one * p.IntParse(points[1 + i * 6 + 2]) / 100;
                    point.orientationGlX = one * p.IntParse(points[1 + i * 6 + 3]) / 1000;
                    point.orientationGlY = one * p.IntParse(points[1 + i * 6 + 4]) / 1000;
                    point.orientationGlZ = one * p.IntParse(points[1 + i * 6 + 5]) / 1000;
                    cameraPoints[cameraPointsCount++] = point;
                }
                m.DisplayNotification(p.StringFormat("Camera points loaded: {0}", p.IntToString(n)));
            }
            return true;
        }
        return false;
    }

    AviWriterCi avi;

    void Stop()
    {
        m.ShowGui(1);
        m.EnableCameraControl(true);
        if (playingTime != -1)
        {
            m.SetFreemove(previousFreemove);
            m.SetLocalPosition(previousPositionX, previousPositionY, previousPositionZ);
            m.SetLocalOrientation(previousOrientationX, previousOrientationY, previousOrientationZ);
        }
        playingTime = -1;
        if (avi != null)
        {
            avi.Close();
            avi = null;
        }
    }

    int previousFreemove;
    float previousPositionX;
    float previousPositionY;
    float previousPositionZ;
    float previousOrientationX;
    float previousOrientationY;
    float previousOrientationZ;

    float[] position;
    float[] orientation;
    float playingSpeed;

    const int framerate = 60;

    // Todo: cubic interpolation
    public override void OnNewFrame(Game game, NewFrameEventArgs args)
    {
        float dt = args.GetDt();
        if (playingTime == -1)
        {
            return;
        }
        playingTime += dt;
        float playingDist = playingTime * playingSpeed;

        UpdateAvi(dt);

        float distA = 0;
        int foundPoint = -1;
        for (int i = 0; i < cameraPointsCount - 1; i++)
        {
            CameraPoint a = cameraPoints[i];
            CameraPoint b = cameraPoints[i + 1];
            float dist = Distance(a, b);
            if (playingDist >= distA && playingDist < distA + dist)
            {
                foundPoint = i;
                break;
            }
            distA += dist;
        }
        if (foundPoint == -1)
        {
            Stop();
            return;
        }
        {
            CameraPoint a = cameraPoints[foundPoint];
            CameraPoint b = cameraPoints[foundPoint + 1];
            CameraPoint aminus = a;
            CameraPoint bplus = b;
            if (foundPoint -1 >= 0)
            {
                aminus = cameraPoints[foundPoint - 1];
            }
            if (foundPoint + 2 < cameraPointsCount)
            {
                bplus = cameraPoints[foundPoint + 2];
            }

            float t = (playingDist - distA) / Distance(a, b);
            float x = q(t, aminus.positionGlX, a.positionGlX, b.positionGlX, bplus.positionGlX);
            float y = q(t, aminus.positionGlY, a.positionGlY, b.positionGlY, bplus.positionGlY);
            float z = q(t, aminus.positionGlZ, a.positionGlZ, b.positionGlZ, bplus.positionGlZ);
            m.SetLocalPosition(x, y, z);

            float orientx = q(t, aminus.orientationGlX, a.orientationGlX, b.orientationGlX, bplus.orientationGlX);
            float orienty = q(t, aminus.orientationGlY, a.orientationGlY, b.orientationGlY, bplus.orientationGlY);
            float orientz = q(t, aminus.orientationGlZ, a.orientationGlZ, b.orientationGlZ, bplus.orientationGlZ);
            m.SetLocalOrientation(orientx, orienty, orientz);
        }
    }

    // http://stackoverflow.com/questions/939874/is-there-a-java-library-with-3d-spline-functions/2623619#2623619
    // Catmull-Rom spline interpolation function
    public static float q(float t, float p0, float p1, float p2, float p3)
    {
        float one_ = 1;
        return (one_ / 2) * ((2 * p1) + (-p0 + p2) * t
                + (2 * p0 - 5 * p1 + 4 * p2 - p3) * (t * t) + (-p0 + 3 * p1 - 3
                * p2 + p3)
                * (t * t * t));
    }

    float recspeed;
    float writeAccum;
    bool firstFrameDone;
    void UpdateAvi(float dt)
    {
        if (avi == null)
        {
            return;
        }
        if (!firstFrameDone)
        {
            // skip first frame, because screen is not redrawn yet.
            firstFrameDone = true;
            return;
        }
        writeAccum += dt;
        float totalTime = playingSpeed * TotalDistance();
        if (writeAccum >= one / framerate * recspeed)
        {
            writeAccum -= one / framerate * recspeed;

            BitmapCi bmp = m.GrabScreenshot();
            avi.AddFrame(bmp);
            bmp.Dispose();
        }
    }

    float TotalDistance()
    {
        float totalDistance = 0;
        for (int i = 0; i < cameraPointsCount - 1; i++)
        {
            CameraPoint a = cameraPoints[i];
            CameraPoint b = cameraPoints[i + 1];
            float dist = Distance(a, b);
            totalDistance += dist;
        }
        return totalDistance;
    }

    float Distance(CameraPoint a, CameraPoint b)
    {
        float dx = a.positionGlX - b.positionGlX;
        float dy = a.positionGlY - b.positionGlY;
        float dz = a.positionGlZ - b.positionGlZ;
        return p.MathSqrt(dx * dx + dy * dy + dz * dz);
    }
}

public class CameraPoint
{
    internal float positionGlX;
    internal float positionGlY;
    internal float positionGlZ;
    internal float orientationGlX;
    internal float orientationGlY;
    internal float orientationGlZ;
}
