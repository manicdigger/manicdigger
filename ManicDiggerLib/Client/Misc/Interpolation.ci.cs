public class InterpolatedObject
{
}
public abstract class IInterpolation
{
    public abstract InterpolatedObject Interpolate(InterpolatedObject a, InterpolatedObject b, float progress);
}
public abstract class INetworkInterpolation
{
    public abstract void AddNetworkPacket(InterpolatedObject c, int timeMilliseconds);
    public abstract InterpolatedObject InterpolatedState(int timeMilliseconds);
}
public class Packet_
{
    internal int timestampMilliseconds;
    internal InterpolatedObject content;
}
public class NetworkInterpolation : INetworkInterpolation
{
    public NetworkInterpolation()
    {
        received = new Packet_[128];
        DELAYMILLISECONDS = 200;
        EXTRAPOLATION_TIMEMILLISECONDS = 200;
    }
    internal IInterpolation req;
    internal bool EXTRAPOLATE;
    internal int DELAYMILLISECONDS;
    internal int EXTRAPOLATION_TIMEMILLISECONDS;
    Packet_[] received;
    int receivedCount;
    public override void AddNetworkPacket(InterpolatedObject c, int timeMilliseconds)
    {
        Packet_ p = new Packet_();
        p.content = c;
        p.timestampMilliseconds = timeMilliseconds;

        int max = 100;
        if (receivedCount >= max)
        {
            for (int i = 0; i < max - 1; i++)
            {
                received[i] = received[i + 1];
            }
            receivedCount = max - 1;
        }

        received[receivedCount++] = p;
    }
    public override InterpolatedObject InterpolatedState(int timeMilliseconds)
    {
        int curtimeMilliseconds = timeMilliseconds;
        int interpolationtimeMilliseconds = curtimeMilliseconds - DELAYMILLISECONDS;
        int p1;
        int p2;
        if (receivedCount == 0)
        {
            return null;
        }
        InterpolatedObject result;
        if (receivedCount > 0 && interpolationtimeMilliseconds < received[0].timestampMilliseconds)
        {
            p1 = 0;
            p2 = 0;
        }
        //extrapolate
        else if (EXTRAPOLATE && (receivedCount >= 2)
            && interpolationtimeMilliseconds > received[receivedCount - 1].timestampMilliseconds)
        {
            p1 = receivedCount - 2;
            p2 = receivedCount - 1;
            interpolationtimeMilliseconds = MathCi.MinInt(interpolationtimeMilliseconds, received[receivedCount - 1].timestampMilliseconds + EXTRAPOLATION_TIMEMILLISECONDS);
        }
        else
        {
            p1 = 0;
            for (int i = 0; i < receivedCount; i++)
            {
                if (received[i].timestampMilliseconds <= interpolationtimeMilliseconds)
                {
                    p1 = i;
                }
            }
            p2 = p1;
            if (receivedCount - 1 > p1)
            {
                p2++;
            }
        }
        if (p1 == p2)
        {
            result = received[p1].content;
        }
        else
        {
            float one = 1;
            result = req.Interpolate(received[p1].content, received[p2].content,
                (one * (interpolationtimeMilliseconds - received[p1].timestampMilliseconds)
                / (received[p2].timestampMilliseconds - received[p1].timestampMilliseconds)));
        }
        return result;
    }
}

public class AngleInterpolation
{
    public static int InterpolateAngle256(GamePlatform platform, int a, int b, float progress)
    {
        if (progress != 0 && b != a)
        {
            int diff = NormalizeAngle256(b - a);
            if (diff >= CircleHalf256)
            {
                diff -= CircleFull256;
            }
            a += platform.FloatToInt(progress * diff);
        }
        return NormalizeAngle256(a);
    }
    const int CircleHalf256 = 128;
    const int CircleFull256 = 256;
    static int NormalizeAngle256(int v)
    {
        return (v + shortMaxValue / 2) % 256;
    }
    public static float InterpolateAngle360(GamePlatform platform, float a, float b, float progress)
    {
        if (progress != 0 && b != a)
        {
            float diff = NormalizeAngle360(platform, b - a);
            if (diff >= CircleHalf360)
            {
                diff -= CircleFull360;
            }
            a += (progress * diff);
        }
        return NormalizeAngle360(platform, a);
    }
    const int CircleHalf360 = 180;
    const int CircleFull360 = 360;
    const int shortMaxValue = 32767;
    static float NormalizeAngle360(GamePlatform platform, float v)
    {
        return platform.FloatModulo(v + ((shortMaxValue / 2) / 360) * 360, 360);
    }
}
