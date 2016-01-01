public class ScriptCharacterPhysics : EntityScript
{
    public ScriptCharacterPhysics()
    {
        movedz = 0;
        curspeed = new Vector3Ref();
        jumpacceleration = 0;
        isplayeronground = false;
        acceleration = new Acceleration();
        jumpstartacceleration = 0;
        jumpstartaccelerationhalf = 0;
        movespeednow = 0;

        tmpPlayerPosition = new float[3];
        tmpBlockingBlockType = new IntRef();

        constGravity = 0.3f;
        constWaterGravityMultiplier = 3;
        constEnableAcceleration = true;
        constJump = 2.1f;
    }

    internal Game game;

    internal float movedz;
    internal Vector3Ref curspeed;
    internal float jumpacceleration;
    internal bool isplayeronground;
    internal Acceleration acceleration;
    internal float jumpstartacceleration;
    internal float jumpstartaccelerationhalf;
    internal float movespeednow;

    internal float constGravity;
    internal float constWaterGravityMultiplier;
    internal bool constEnableAcceleration;
    internal float constJump;

    public override void OnNewFrameFixed(Game game_, int entity, float dt)
    {
        game = game_;
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }
        movespeednow = game.MoveSpeedNow();
        game.controls.movedx = MathCi.ClampFloat(game.controls.movedx, -1, 1);
        game.controls.movedy = MathCi.ClampFloat(game.controls.movedy, -1, 1);
        Controls move = game.controls;
        jumpstartacceleration = 13.333f * constGravity; // default
        jumpstartaccelerationhalf = 9 * constGravity;
        acceleration.SetDefault();
        game.soundnow = new BoolRef();
        if (game.FollowId() != null && game.FollowId().value == game.LocalPlayerId)
        {
            move.movedx = 0;
            move.movedy = 0;
            move.moveup = false;
            move.wantsjump = false;
        }
        Update(game.player.position, move, dt, game.soundnow, Vector3Ref.Create(game.pushX, game.pushY, game.pushZ), game.entities[game.LocalPlayerId].drawModel.ModelHeight);
    }

    public void Update(EntityPosition_ stateplayerposition, Controls move, float dt, BoolRef soundnow, Vector3Ref push, float modelheight)
    {
        if (game.stopPlayerMove)
        {
            movedz = 0;
            game.stopPlayerMove = false;
        }

        // No air control
        if (!isplayeronground)
        {
            acceleration.acceleration1 = 0.99f;
            acceleration.acceleration2 = 0.2f;
            acceleration.acceleration3 = 70;
        }

        // Trampoline
        {
            int blockunderplayer = game.BlockUnderPlayer();
            if (blockunderplayer != -1 && blockunderplayer == game.d_Data.BlockIdTrampoline()
                && (!isplayeronground) && !game.controls.shiftkeydown)
            {
                game.controls.wantsjump = true;
                jumpstartacceleration = 20.666f * constGravity;
            }
        }

        // Slippery walk on ice and when swimming
        {
            int blockunderplayer = game.BlockUnderPlayer();
            if ((blockunderplayer != -1 && game.d_Data.IsSlipperyWalk()[blockunderplayer]) || game.SwimmingBody())
            {
                acceleration.acceleration1 = 0.99f;
                acceleration.acceleration2 = 0.2f;
                acceleration.acceleration3 = 70;
            }
        }

        soundnow.value = false;
        Vector3Ref diff1ref = new Vector3Ref();
        VectorTool.ToVectorInFixedSystem
            (move.movedx * movespeednow * dt,
             0,
             move.movedy * movespeednow * dt, stateplayerposition.rotx, stateplayerposition.roty, diff1ref);
        Vector3Ref diff1 = new Vector3Ref();
        diff1.X = diff1ref.X;
        diff1.Y = diff1ref.Y;
        diff1.Z = diff1ref.Z;
        if (MiscCi.Vec3Length(push.X, push.Y, push.Z) > 0.01f)
        {
            push.Normalize();
            push.X *= 5;
            push.Y *= 5;
            push.Z *= 5;
        }
        diff1.X += push.X * dt;
        diff1.Y += push.Y * dt;
        diff1.Z += push.Z * dt;

        bool loaded = false;
        int cx = game.platform.FloatToInt(game.player.position.x / Game.chunksize);
        int cy = game.platform.FloatToInt(game.player.position.z / Game.chunksize);
        int cz = game.platform.FloatToInt(game.player.position.y / Game.chunksize);
        if (game.map.IsValidChunkPos(cx, cy, cz))
        {
            if (game.map.chunks[MapUtilCi.Index3d(cx, cy, cz,
                                                  game.map.MapSizeX / Game.chunksize,
                                                  game.map.MapSizeY / Game.chunksize)] != null)
            {
                loaded = true;
            }
        }
        else
        {
            loaded = true;
        }
        if ((!(move.freemove)) && loaded)
        {
            if (!game.SwimmingBody())
            {
                movedz += -constGravity;//gravity
            }
            else
            {
                movedz += -constGravity * constWaterGravityMultiplier; //more gravity because it's slippery.
            }
        }
        game.movedz = movedz;
        if (constEnableAcceleration)
        {
            curspeed.X *= acceleration.acceleration1;
            curspeed.Y *= acceleration.acceleration1;
            curspeed.Z *= acceleration.acceleration1;
            curspeed.X = MakeCloserToZero(curspeed.X, acceleration.acceleration2 * dt);
            curspeed.Y = MakeCloserToZero(curspeed.Y, acceleration.acceleration2 * dt);
            curspeed.Z = MakeCloserToZero(curspeed.Z, acceleration.acceleration2 * dt);
            diff1.Y += move.moveup ? 2 * movespeednow * dt : 0;
            diff1.Y -= move.movedown ? 2 * movespeednow * dt : 0;
            curspeed.X += diff1.X * acceleration.acceleration3 * dt;
            curspeed.Y += diff1.Y * acceleration.acceleration3 * dt;
            curspeed.Z += diff1.Z * acceleration.acceleration3 * dt;
            if (curspeed.Length() > movespeednow)
            {
                curspeed.Normalize();
                curspeed.X *= movespeednow;
                curspeed.Y *= movespeednow;
                curspeed.Z *= movespeednow;
            }
        }
        else
        {
            if (MiscCi.Vec3Length(diff1.X, diff1.Y, diff1.Z) > 0)
            {
                diff1.Normalize();
            }
            curspeed.X = diff1.X * movespeednow;
            curspeed.Y = diff1.Y * movespeednow;
            curspeed.Z = diff1.Z * movespeednow;
        }
        Vector3Ref newposition = Vector3Ref.Create(0, 0, 0);
        if (!(move.freemove))
        {
            newposition.X = stateplayerposition.x + curspeed.X;
            newposition.Y = stateplayerposition.y + curspeed.Y;
            newposition.Z = stateplayerposition.z + curspeed.Z;
            if (!game.SwimmingBody())
            {
                newposition.Y = stateplayerposition.y;
            }
            // Fast move when looking at the ground
            float diffx = newposition.X - stateplayerposition.x;
            float diffy = newposition.Y - stateplayerposition.y;
            float diffz = newposition.Z - stateplayerposition.z;
            float difflength = MiscCi.Vec3Length(diffx, diffy, diffz);
            if (difflength > 0)
            {
                diffx /= difflength;
                diffy /= difflength;
                diffz /= difflength;
                diffx *= curspeed.Length();
                diffy *= curspeed.Length();
                diffz *= curspeed.Length();
            }
            newposition.X = stateplayerposition.x + diffx * dt;
            newposition.Y = stateplayerposition.y + diffy * dt;
            newposition.Z = stateplayerposition.z + diffz * dt;
        }
        else
        {
            newposition.X = stateplayerposition.x + (curspeed.X) * dt;
            newposition.Y = stateplayerposition.y + (curspeed.Y) * dt;
            newposition.Z = stateplayerposition.z + (curspeed.Z) * dt;
        }
        newposition.Y += movedz * dt;
        Vector3Ref previousposition = Vector3Ref.Create(stateplayerposition.x, stateplayerposition.y, stateplayerposition.z);
        if (!move.noclip)
        {
            float[] v = WallSlide(
                Vec3.FromValues(stateplayerposition.x, stateplayerposition.y, stateplayerposition.z),
                Vec3.FromValues(newposition.X, newposition.Y, newposition.Z),
                modelheight);
            stateplayerposition.x = v[0];
            stateplayerposition.y = v[1];
            stateplayerposition.z = v[2];
        }
        else
        {
            stateplayerposition.x = newposition.X;
            stateplayerposition.y = newposition.Y;
            stateplayerposition.z = newposition.Z;
        }
        if (!(move.freemove))
        {
            if ((isplayeronground) || game.SwimmingBody())
            {
                jumpacceleration = 0;
                movedz = 0;
            }
            if ((move.wantsjump || move.wantsjumphalf) && (((jumpacceleration == 0 && isplayeronground) || game.SwimmingBody()) && loaded) && (!game.SwimmingEyes()))
            {
                jumpacceleration = move.wantsjumphalf ? jumpstartaccelerationhalf : jumpstartacceleration;
                soundnow.value = true;
            }

            if (jumpacceleration > 0)
            {
                isplayeronground = false;
                jumpacceleration = jumpacceleration / 2;
            }

            //if (!this.reachedceiling)
            {
                movedz += jumpacceleration * constJump;
            }
        }
        else
        {
            isplayeronground = true;
        }
        game.isplayeronground = isplayeronground;
    }

    public bool IsTileEmptyForPhysics(int x, int y, int z)
    {
        if (z >= game.map.MapSizeZ)
        {
            return true;
        }
        bool enableFreemove = false;
        if (x < 0 || y < 0 || z < 0)// || z >= mapsizez)
        {
            return enableFreemove;
        }
        if (x >= game.map.MapSizeX || y >= game.map.MapSizeY)// || z >= mapsizez)
        {
            return enableFreemove;
        }
        int block = game.map.GetBlockValid(x, y, z);
        if (block == 0)
        {
            return true;
        }
        Packet_BlockType blocktype = game.blocktypes[block];
        return blocktype.WalkableType == Packet_WalkableTypeEnum.Fluid
            || game.IsEmptyForPhysics(blocktype)
            || game.IsRail(blocktype);
    }

    float[] tmpPlayerPosition;		//Temporarily stores the player's position. Used in WallSlide()
    IntRef tmpBlockingBlockType;
    public float[] WallSlide(float[] oldposition, float[] newposition, float modelheight)
    {
        bool high = false;
        if (modelheight >= 2) { high = true; }	//Set high to true if player model is bigger than standard height
        oldposition[1] += game.constWallDistance;		//Add walldistance temporarily for ground collisions
        newposition[1] += game.constWallDistance;		//Add walldistance temporarily for ground collisions

        game.reachedwall = false;
        game.reachedwall_1blockhigh = false;
        game.reachedHalfBlock = false;

        tmpPlayerPosition[0] = oldposition[0];
        tmpPlayerPosition[1] = oldposition[1];
        tmpPlayerPosition[2] = oldposition[2];

        tmpBlockingBlockType.value = 0;

        // X
        if (IsEmptySpaceForPlayer(high, newposition[0], tmpPlayerPosition[1], tmpPlayerPosition[2], tmpBlockingBlockType))
        {
            tmpPlayerPosition[0] = newposition[0];
        }
        else
        {
            // For autojump
            game.reachedwall = true;
            if (IsEmptyPoint(newposition[0], tmpPlayerPosition[1] + 0.5f, tmpPlayerPosition[2], null))
            {
                game.reachedwall_1blockhigh = true;
                if (game.blocktypes[tmpBlockingBlockType.value].DrawType == Packet_DrawTypeEnum.HalfHeight) { game.reachedHalfBlock = true; }
                if (StandingOnHalfBlock(newposition[0], tmpPlayerPosition[1], tmpPlayerPosition[2])) { game.reachedHalfBlock = true; }
            }
        }
        // Y
        if (IsEmptySpaceForPlayer(high, tmpPlayerPosition[0], newposition[1], tmpPlayerPosition[2], tmpBlockingBlockType))
        {
            tmpPlayerPosition[1] = newposition[1];
        }
        // Z
        if (IsEmptySpaceForPlayer(high, tmpPlayerPosition[0], tmpPlayerPosition[1], newposition[2], tmpBlockingBlockType))
        {
            tmpPlayerPosition[2] = newposition[2];
        }
        else
        {
            // For autojump
            game.reachedwall = true;
            if (IsEmptyPoint(tmpPlayerPosition[0], tmpPlayerPosition[1] + 0.5f, newposition[2], null))
            {
                game.reachedwall_1blockhigh = true;
                if (game.blocktypes[tmpBlockingBlockType.value].DrawType == Packet_DrawTypeEnum.HalfHeight) { game.reachedHalfBlock = true; }
                if (StandingOnHalfBlock(tmpPlayerPosition[0], tmpPlayerPosition[1], newposition[2])) { game.reachedHalfBlock = true; }
            }
        }

        isplayeronground = (tmpPlayerPosition[1] == oldposition[1]) && (newposition[1] < oldposition[1]);

        tmpPlayerPosition[1] -= game.constWallDistance;	//Remove the temporary walldistance again
        return tmpPlayerPosition;	//Return valid position
    }

    bool StandingOnHalfBlock(float x, float y, float z)
    {
        int under = game.map.GetBlock(game.platform.FloatToInt(x),
                                      game.platform.FloatToInt(z),
                                      game.platform.FloatToInt(y));
        return game.blocktypes[under].DrawType == Packet_DrawTypeEnum.HalfHeight;
    }
    
    bool IsEmptySpaceForPlayer(bool high, float x, float y, float z, IntRef blockingBlockType)
    {
        return IsEmptyPoint(x, y, z, blockingBlockType)
            && IsEmptyPoint(x, y + 1, z, blockingBlockType)
            && (!high || IsEmptyPoint(x, y + 2, z, blockingBlockType));
    }

    // Checks if there are no solid blocks in walldistance area around the point
    bool IsEmptyPoint(float x, float y, float z, IntRef blockingBlocktype)
    {
        // Test 3x3x3 blocks around the point
        for (int xx = 0; xx < 3; xx++)
        {
            for (int yy = 0; yy < 3; yy++)
            {
                for (int zz = 0; zz < 3; zz++)
                {
                    if (!IsTileEmptyForPhysics(FloatToInt(x + xx - 1), FloatToInt(z + zz - 1), FloatToInt(y + yy - 1)))
                    {
                        // Found a solid block

                        // Get bounding box of the block
                        float minX = FloatToInt(x + xx - 1);
                        float minY = FloatToInt(y + yy - 1);
                        float minZ = FloatToInt(z + zz - 1);
                        float maxX = minX + 1;
                        float maxY = minY + game.getblockheight(FloatToInt(x + xx - 1), FloatToInt(z + zz - 1), FloatToInt(y + yy - 1));
                        float maxZ = minZ + 1;

                        // Check if the block is too close
                        if (BoxPointDistance(minX, minY, minZ, maxX, maxY, maxZ, x, y, z) < game.constWallDistance)
                        {
                            if (blockingBlocktype != null)
                            {
                                blockingBlocktype.value = game.map.GetBlock(FloatToInt(x + xx - 1), FloatToInt(z + zz - 1), FloatToInt(y + yy - 1));
                            }
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    // Using chebyshev distance
    public static float BoxPointDistance(float minX, float minY, float minZ, float maxX, float maxY, float maxZ, float pX, float pY, float pZ)
    {
        float dx = Max3(minX - pX, 0, pX - maxX);
        float dy = Max3(minY - pY, 0, pY - maxY);
        float dz = Max3(minZ - pZ, 0, pZ - maxZ);
        return Max3(dx, dy, dz);
    }

    public static float MakeCloserToZero(float a, float b)
    {
        if (a > 0)
        {
            float c = a - b;
            if (c < 0)
            {
                c = 0;
            }
            return c;
        }
        else
        {
            float c = a + b;
            if (c > 0)
            {
                c = 0;
            }
            return c;
        }
    }

    static float Max3(float a, float b, float c)
    {
        return MathCi.MaxFloat(MathCi.MaxFloat(a, b), c);
    }

    int FloatToInt(float value)
    {
        return game.platform.FloatToInt(value);
    }
}

public class Acceleration
{
    public Acceleration()
    {
        SetDefault();
    }

    internal float acceleration1;
    internal float acceleration2;
    internal float acceleration3;

    public void SetDefault()
    {
        acceleration1 = 0.9f;
        acceleration2 = 2;
        acceleration3 = 700;
    }
}

public class Controls
{
    internal float movedx;
    internal float movedy;
    internal bool wantsjump;
    internal bool wantsjumphalf;
    internal bool moveup;
    internal bool movedown;
    internal bool shiftkeydown;
    internal bool freemove;
    internal bool noclip;

    /// <summary>
    /// Set player freemove mode to the given value
    /// </summary>
    /// <param name="level">Freemove mode as defined in FreemoveLevelEnum</param>
    public void SetFreemove(int level)
    {
        if (level == FreemoveLevelEnum.None)
        {
            freemove = false;
            noclip = false;
        }

        if (level == FreemoveLevelEnum.Freemove)
        {
            freemove = true;
            noclip = false;
        }

        if (level == FreemoveLevelEnum.Noclip)
        {
            freemove = true;
            noclip = true;
        }
    }
    /// <summary>
    /// Get the current player freemove mode
    /// </summary>
    /// <returns>Freemove mode as defined in FreemoveLevelEnum</returns>
    public int GetFreemove()
    {
        if (!freemove)
        {
            return FreemoveLevelEnum.None;
        }
        if (noclip)
        {
            return FreemoveLevelEnum.Noclip;
        }
        else
        {
            return FreemoveLevelEnum.Freemove;
        }
    }
}

public class FreemoveLevelEnum
{
    public const int None = 0;
    public const int Freemove = 1;
    public const int Noclip = 2;
}
