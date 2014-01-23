public class VectorTool
{
    public static void ToVectorInFixedSystem(float dx, float dy, float dz, float orientationx, float orientationy, Vector3Ref output)
    {
        //Don't calculate for nothing ...
        if (dx == 0 && dy == 0 && dz == 0)
        {
            output.X = 0;
            output.Y = 0;
            output.Z = 0;
            return;
        }

        //Convert to Radian : 360° = 2PI
        float xRot = orientationx;//Math.toRadians(orientation.X);
        float yRot = orientationy;//Math.toRadians(orientation.Y);

        //Calculate the formula
        float x = (dx * Platform.Cos(yRot) + dy * Platform.Sin(xRot) * Platform.Sin(yRot) - dz * Platform.Cos(xRot) * Platform.Sin(yRot));
        float y = (dy * Platform.Cos(xRot) + dz * Platform.Sin(xRot));
        float z = (dx * Platform.Sin(yRot) - dy * Platform.Sin(xRot) * Platform.Cos(yRot) + dz * Platform.Cos(xRot) * Platform.Cos(yRot));

        //Return the vector expressed in the global axis system
        output.X = x;
        output.Y = y;
        output.Z = z;
    }
}
