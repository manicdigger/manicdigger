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
