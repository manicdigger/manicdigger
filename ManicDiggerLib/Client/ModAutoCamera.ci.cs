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
    
    public override bool OnClientCommand(ClientCommandArgs args)
    {
        if (args.command == "cam")
        {
            IntRef argumentsLength = new IntRef();
            string[] arguments = p.StringSplit(args.arguments, " ", argumentsLength);
            if (p.StringTrim(args.arguments) == "")
            {
                m.DisplayNotification("Camera help.");
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
                previousPositionX = m.GetLocalPositionX();
                previousPositionY = m.GetLocalPositionY();
                previousPositionZ = m.GetLocalPositionZ();
                previousOrientationX = m.GetLocalOrientationX();
                previousOrientationY = m.GetLocalOrientationY();
                previousOrientationZ = m.GetLocalOrientationZ();
                m.ShowGui(0);
                previousFreemove = m.GetFreemove();
                m.SetFreemove(FreemoveLevelEnum.Noclip);
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
        playingTime = -1;
        m.ShowGui(1);
        m.SetFreemove(previousFreemove);
        m.SetLocalPosition(previousPositionX, previousPositionY, previousPositionZ);
        m.SetLocalOrientation(previousOrientationX, previousOrientationY, previousOrientationZ);
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
    public override void OnNewFrame(NewFrameEventArgs args)
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
            float dist = Distance(a, b);
            float dx = (b.positionGlX - a.positionGlX) / dist;
            float dy = (b.positionGlY - a.positionGlY) / dist;
            float dz = (b.positionGlZ - a.positionGlZ) / dist;

            float x = a.positionGlX + dx * (playingDist - distA);
            float y = a.positionGlY + dy * (playingDist - distA);
            float z = a.positionGlZ + dz * (playingDist - distA);
            m.SetLocalPosition(x, y, z);

            float dorientx = (b.orientationGlX - a.orientationGlX) / dist;
            float dorienty = (b.orientationGlY - a.orientationGlY) / dist;
            float dorientz = (b.orientationGlZ - a.orientationGlZ) / dist;
            float orientx = a.orientationGlX + dorientx * (playingDist - distA);
            float orienty = a.orientationGlY + dorienty * (playingDist - distA);
            float orientz = a.orientationGlZ + dorientz * (playingDist - distA);
            m.SetLocalOrientation(orientx, orienty, orientz);
        }
    }

    float recspeed;
    float writeAccum;
    void UpdateAvi(float dt)
    {
        if (avi == null)
        {
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
