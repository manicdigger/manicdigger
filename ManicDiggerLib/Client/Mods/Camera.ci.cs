public class ModCamera : ClientMod
{
    public ModCamera()
    {
        OverheadCamera_cameraEye = new Vector3Ref();
        upVec3 = Vec3.FromValues(0, 1, 0);
    }

    public override void OnBeforeNewFrameDraw3d(Game game, float deltaTime)
    {
        if (game.overheadcamera)
        {
            game.camera = OverheadCamera(game);
        }
        else
        {
            game.camera = FppCamera(game);
        }
    }

    internal Vector3Ref OverheadCamera_cameraEye;
    internal float[] OverheadCamera(Game game)
    {
        game.overheadcameraK.GetPosition(game.platform, OverheadCamera_cameraEye);
        Vector3Ref cameraEye = OverheadCamera_cameraEye;
        Vector3Ref cameraTarget = Vector3Ref.Create(game.overheadcameraK.Center.X, game.overheadcameraK.Center.Y + game.GetCharacterEyesHeight(), game.overheadcameraK.Center.Z);
        FloatRef currentOverheadcameradistance = FloatRef.Create(game.overheadcameradistance);
        LimitThirdPersonCameraToWalls(game, cameraEye, cameraTarget, currentOverheadcameradistance);
        float[] ret = new float[16];
        Mat4.LookAt(ret, Vec3.FromValues(cameraEye.X, cameraEye.Y, cameraEye.Z),
            Vec3.FromValues(cameraTarget.X, cameraTarget.Y, cameraTarget.Z),
            upVec3);
        game.CameraEyeX = cameraEye.X;
        game.CameraEyeY = cameraEye.Y;
        game.CameraEyeZ = cameraEye.Z;
        return ret;
    }
    float[] upVec3;

    internal float[] FppCamera(Game game)
    {
        Vector3Ref forward = new Vector3Ref();
        VectorTool.ToVectorInFixedSystem(0, 0, 1, game.player.position.rotx, game.player.position.roty, forward);
        Vector3Ref cameraEye = new Vector3Ref();
        Vector3Ref cameraTarget = new Vector3Ref();
        float playerEyeX = game.player.position.x;
        float playerEyeY = game.player.position.y + game.GetCharacterEyesHeight();
        float playerEyeZ = game.player.position.z;
        if (!game.ENABLE_TPP_VIEW)
        {
            cameraEye.X = playerEyeX;
            cameraEye.Y = playerEyeY;
            cameraEye.Z = playerEyeZ;
            cameraTarget.X = playerEyeX + forward.X;
            cameraTarget.Y = playerEyeY + forward.Y;
            cameraTarget.Z = playerEyeZ + forward.Z;
        }
        else
        {
            cameraEye.X = playerEyeX + forward.X * -game.tppcameradistance;
            cameraEye.Y = playerEyeY + forward.Y * -game.tppcameradistance;
            cameraEye.Z = playerEyeZ + forward.Z * -game.tppcameradistance;
            cameraTarget.X = playerEyeX;
            cameraTarget.Y = playerEyeY;
            cameraTarget.Z = playerEyeZ;
            FloatRef currentTppcameradistance = FloatRef.Create(game.tppcameradistance);
            LimitThirdPersonCameraToWalls(game, cameraEye, cameraTarget, currentTppcameradistance);
        }
        float[] ret = new float[16];
        Mat4.LookAt(ret, Vec3.FromValues(cameraEye.X, cameraEye.Y, cameraEye.Z),
            Vec3.FromValues(cameraTarget.X, cameraTarget.Y, cameraTarget.Z),
            upVec3);
        game.CameraEyeX = cameraEye.X;
        game.CameraEyeY = cameraEye.Y;
        game.CameraEyeZ = cameraEye.Z;
        return ret;
    }

    internal void LimitThirdPersonCameraToWalls(Game game, Vector3Ref eye, Vector3Ref target, FloatRef curtppcameradistance)
    {
        float one = 1;
        Vector3Ref ray_start_point = target;
        Vector3Ref raytarget = eye;

        Line3D pick = new Line3D();
        float raydirX = (raytarget.X - ray_start_point.X);
        float raydirY = (raytarget.Y - ray_start_point.Y);
        float raydirZ = (raytarget.Z - ray_start_point.Z);

        float raydirLength1 = game.Length(raydirX, raydirY, raydirZ);
        raydirX /= raydirLength1;
        raydirY /= raydirLength1;
        raydirZ /= raydirLength1;
        raydirX = raydirX * (game.tppcameradistance + 1);
        raydirY = raydirY * (game.tppcameradistance + 1);
        raydirZ = raydirZ * (game.tppcameradistance + 1);
        pick.Start = Vec3.FromValues(ray_start_point.X, ray_start_point.Y, ray_start_point.Z);
        pick.End = new float[3];
        pick.End[0] = ray_start_point.X + raydirX;
        pick.End[1] = ray_start_point.Y + raydirY;
        pick.End[2] = ray_start_point.Z + raydirZ;

        //pick terrain
        IntRef pick2Count = new IntRef();
        BlockPosSide[] pick2 = game.Pick(game.s, pick, pick2Count);

        if (pick2Count.value > 0)
        {
            BlockPosSide pick2nearest = game.Nearest(pick2, pick2Count.value, ray_start_point.X, ray_start_point.Y, ray_start_point.Z);
            //pick2.Sort((a, b) => { return (FloatArrayToVector3(a.blockPos) - ray_start_point).Length.CompareTo((FloatArrayToVector3(b.blockPos) - ray_start_point).Length); });

            float pickX = pick2nearest.blockPos[0] - target.X;
            float pickY = pick2nearest.blockPos[1] - target.Y;
            float pickZ = pick2nearest.blockPos[2] - target.Z;
            float pickdistance = game.Length(pickX, pickY, pickZ);
            curtppcameradistance.value = MathCi.MinFloat(pickdistance - 1, curtppcameradistance.value);
            if (curtppcameradistance.value < one * 3 / 10) { curtppcameradistance.value = one * 3 / 10; }
        }

        float cameraDirectionX = target.X - eye.X;
        float cameraDirectionY = target.Y - eye.Y;
        float cameraDirectionZ = target.Z - eye.Z;
        float raydirLength = game.Length(raydirX, raydirY, raydirZ);
        raydirX /= raydirLength;
        raydirY /= raydirLength;
        raydirZ /= raydirLength;
        eye.X = target.X + raydirX * curtppcameradistance.value;
        eye.Y = target.Y + raydirY * curtppcameradistance.value;
        eye.Z = target.Z + raydirZ * curtppcameradistance.value;
    }
}
