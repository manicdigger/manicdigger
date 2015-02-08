public class CharacterPhysicsState
{
    public CharacterPhysicsState()
    {
        movedz = 0;
        curspeed = new Vector3Ref();
        jumpacceleration = 0;
        isplayeronground = false;
    }
    internal float movedz;
    internal Vector3Ref curspeed;
    internal float jumpacceleration;
    internal bool isplayeronground;
}

public class Acceleration
{
    public Acceleration()
    {
        acceleration1 = 0.9f;
        acceleration2 = 2;
        acceleration3 = 700;
    }
    internal float acceleration1;
    internal float acceleration2;
    internal float acceleration3;
}

public class Vector3Ref
{
    internal float X;
    internal float Y;
    internal float Z;

    internal float Length()
    {
        return Platform.Sqrt(X * X + Y * Y + Z * Z);
    }

    internal void Normalize()
    {
        float length = Length();
        X = X / length;
        Y = Y / length;
        Z = Z / length;
    }

    internal static Vector3Ref Create(float x, float y, float z)
    {
        Vector3Ref v = new Vector3Ref();
        v.X = x;
        v.Y = y;
        v.Z = z;
        return v;
    }

    public float GetX()
    {
        return X;
    }

    public float GetY()
    {
        return Y;
    }

    public float GetZ()
    {
        return Z;
    }
}

public class Vector3IntRef
{
    internal int X;
    internal int Y;
    internal int Z;

    internal static Vector3IntRef Create(int x, int y, int z)
    {
        Vector3IntRef v = new Vector3IntRef();
        v.X = x;
        v.Y = y;
        v.Z = z;
        return v;
    }
}

public class MoveInfo
{
    internal bool ENABLE_FREEMOVE;
    internal bool Swimming;
    internal Acceleration acceleration;
    internal float movespeednow;
    internal float movedx;
    internal float movedy;
    internal bool ENABLE_NOCLIP;
    internal bool wantsjump;
    internal bool moveup;
    internal bool movedown;
    internal float jumpstartacceleration;
    internal bool shiftkeydown;
}

public class ModCharacterPhysics : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }
        float one = 1;
        float dt = args.GetDt();
        float movespeednow = game.MoveSpeedNow();
        game.movedx = Game.ClampFloat(game.movedx, -1, 1);
        game.movedy = Game.ClampFloat(game.movedy, -1, 1);
        MoveInfo move = new MoveInfo();
        {
            move.movedx = game.movedx;
            move.movedy = game.movedy;
            move.acceleration = game.acceleration;
            move.ENABLE_FREEMOVE = game.ENABLE_FREEMOVE;
            move.ENABLE_NOCLIP = game.ENABLE_NOCLIP;
            move.jumpstartacceleration = game.jumpstartacceleration;
            move.movespeednow = movespeednow;
            move.moveup = game.moveup;
            move.movedown = game.movedown;
            move.Swimming = game.Swimming();
            move.wantsjump = game.wantsjump;
            move.shiftkeydown = game.shiftkeydown;
        }
        game.jumpstartacceleration = (13 + one * 333 / 1000) * game.d_Physics.gravity; // default
        game.acceleration = new Acceleration(); // default
        game.soundnow = new BoolRef();
        if (game.FollowId() == null)
        {
            game.d_Physics.Move(game.player.physicsState, game.player.position, move, dt, game.soundnow, Vector3Ref.Create(game.pushX, game.pushY, game.pushZ), game.entities[game.LocalPlayerId].drawModel.ModelHeight);
        }
        else
        {
            if (game.FollowId().value == game.LocalPlayerId)
            {
                move.movedx = 0;
                move.movedy = 0;
                move.wantsjump = false;
                game.d_Physics.Move(game.player.physicsState, game.player.position, move, dt, game.soundnow, Vector3Ref.Create(game.pushX, game.pushY, game.pushZ), game.entities[game.LocalPlayerId].drawModel.ModelHeight);
            }
        }
    }
}

public class CharacterPhysicsCi
{
    public CharacterPhysicsCi()
    {
        one = 1;
        walldistance = one * 3 / 10;
        gravity = one * 3 / 10;
        WaterGravityMultiplier = 3;
        enable_acceleration = true;
        jumpconst = one * 21 / 10;

        playerposition = new float[3];
    }
    internal Game game;
    internal bool swimmingtop;
    internal float walldistance;
    internal bool reachedwall_1blockhigh;
    internal bool reachedwall;
    internal float gravity;
    internal float WaterGravityMultiplier;
    internal bool enable_acceleration;
    internal bool standingontheground;
    internal float one;
    internal float jumpconst;
    internal bool shiftkeypressed;
    
    public bool IsTileEmptyForPhysics(int x, int y, int z)
    {
        if (z >= game.MapSizeZ)
        {
            return true;
        }
        bool ENABLE_FREEMOVE = false;
        if (x < 0 || y < 0 || z < 0)// || z >= mapsizez)
        {
            return ENABLE_FREEMOVE;
        }
        if (x >= game.MapSizeX || y >= game.MapSizeY)// || z >= mapsizez)
        {
            return ENABLE_FREEMOVE;
        }
        int block = game.GetBlockValid(x, y, z);
        if (block == 0)
        {
            return true;
        }
        Packet_BlockType blocktype = game.blocktypes[block];
        int blockabove = game.GetBlock(x, y, z + 1);
        Packet_BlockType blocktypeabove = game.blocktypes[blockabove];

        //this test is so the player does not walk on water.
        if (game.IsFluid(blocktype) &&
            !game.IsFluid(blocktypeabove)) { return true; }
        int blockabove2 = game.GetBlock(x, y, z + 2);
        return (game.IsFluid(blocktype) && (!swimmingtop))
            || game.IsEmptyForPhysics(blocktype)
            || game.IsRail(blocktype);
    }

    public void Move(CharacterPhysicsState state, EntityPosition_ stateplayerposition, MoveInfo move, float dt, BoolRef soundnow, Vector3Ref push, float modelheight)
    {
        soundnow.value = false;
        shiftkeypressed = move.shiftkeydown;
        Vector3Ref diff1ref = new Vector3Ref();
        VectorTool.ToVectorInFixedSystem
            (move.movedx * move.movespeednow * dt,
            0,
            move.movedy * move.movespeednow * dt, stateplayerposition.rotx, stateplayerposition.roty, diff1ref);
        Vector3Ref diff1 = new Vector3Ref();
        diff1.X = diff1ref.X;
        diff1.Y = diff1ref.Y;
        diff1.Z = diff1ref.Z;
        if (Length(push.X, push.Y, push.Z) > one / 100)
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
        if (game.IsValidChunkPos(cx, cy, cz, Game.chunksize))
        {
            if (game.chunks[MapUtilCi.Index3d(cx, cy, cz,
                game.MapSizeX / Game.chunksize,
                game.MapSizeY / Game.chunksize)] != null)
            {
                loaded = true;
            }
        }
        else
        {
            loaded = true;
        }
        if ((!(move.ENABLE_FREEMOVE)) && loaded)
        {
            if (!move.Swimming)
            {
                state.movedz += -gravity;//gravity
            }
            else
            {
                state.movedz += -gravity * WaterGravityMultiplier; //more gravity because it's slippery.
            }
        }
        if (enable_acceleration)
        {
            state.curspeed.X *= move.acceleration.acceleration1;
            state.curspeed.Y *= move.acceleration.acceleration1;
            state.curspeed.Z *= move.acceleration.acceleration1;
            state.curspeed.X = MakeCloserToZero(state.curspeed.X, move.acceleration.acceleration2 * dt);
            state.curspeed.Y = MakeCloserToZero(state.curspeed.Y, move.acceleration.acceleration2 * dt);
            state.curspeed.Z = MakeCloserToZero(state.curspeed.Z, move.acceleration.acceleration2 * dt);
            diff1.Y += move.moveup ? 2 * move.movespeednow * dt : 0;
            diff1.Y -= move.movedown ? 2 * move.movespeednow * dt : 0;
            state.curspeed.X += diff1.X * move.acceleration.acceleration3 * dt;
            state.curspeed.Y += diff1.Y * move.acceleration.acceleration3 * dt;
            state.curspeed.Z += diff1.Z * move.acceleration.acceleration3 * dt;
            if (state.curspeed.Length() > move.movespeednow)
            {
                state.curspeed.Normalize();
                state.curspeed.X *= move.movespeednow;
                state.curspeed.Y *= move.movespeednow;
                state.curspeed.Z *= move.movespeednow;
            }
        }
        else
        {
            if (Length(diff1.X, diff1.Y, diff1.Z) > 0)
            {
                diff1.Normalize();
            }
            state.curspeed.X = diff1.X * move.movespeednow;
            state.curspeed.Y = diff1.Y * move.movespeednow;
            state.curspeed.Z = diff1.Z * move.movespeednow;
        }
        Vector3Ref newposition = Vector3Ref.Create(0, 0, 0);
        if (!(move.ENABLE_FREEMOVE))
        {
            newposition.X = stateplayerposition.x + state.curspeed.X;
            newposition.Y = stateplayerposition.y + state.curspeed.Y;
            newposition.Z = stateplayerposition.z + state.curspeed.Z;
            if (!move.Swimming)
            {
                newposition.Y = stateplayerposition.y;
            }
            //fast move when looking at the ground.
            float diffx = newposition.X - stateplayerposition.x;
            float diffy = newposition.Y - stateplayerposition.y;
            float diffz = newposition.Z - stateplayerposition.z;
            float difflength = Length(diffx, diffy, diffz);
            if (difflength > 0)
            {
                diffx /= difflength;
                diffy /= difflength;
                diffz /= difflength;
                diffx *= state.curspeed.Length();
                diffy *= state.curspeed.Length();
                diffz *= state.curspeed.Length();
            }
            newposition.X = stateplayerposition.x + diffx * dt;
            newposition.Y = stateplayerposition.y + diffy * dt;
            newposition.Z = stateplayerposition.z + diffz * dt;
        }
        else
        {
            newposition.X = stateplayerposition.x + (state.curspeed.X) * dt;
            newposition.Y = stateplayerposition.y + (state.curspeed.Y) * dt;
            newposition.Z = stateplayerposition.z + (state.curspeed.Z) * dt;
        }
        newposition.Y += state.movedz * dt;
        Vector3Ref previousposition = Vector3Ref.Create(stateplayerposition.x, stateplayerposition.y, stateplayerposition.z);
        if (!move.ENABLE_NOCLIP)
        {
            swimmingtop = move.wantsjump && !move.Swimming;

            float[] v = WallSlide(state,
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
        if (!(move.ENABLE_FREEMOVE || move.Swimming))
        {
            state.isplayeronground = stateplayerposition.y == previousposition.Y;
            {
                if (standingontheground && state.isplayeronground)
                {
                    state.jumpacceleration = 0;
                    state.movedz = 0;
                }
                if (move.wantsjump && state.jumpacceleration == 0 && standingontheground && loaded)
                {
                    state.jumpacceleration = move.jumpstartacceleration;
                    soundnow.value = true;
                }

                if (state.jumpacceleration > 0)
                {
                    standingontheground = false;
                    state.jumpacceleration = state.jumpacceleration / 2;
                }

                //if (!this.reachedceiling)
                {
                    state.movedz += state.jumpacceleration * jumpconst;
                }
            }
        }
        else
        {
            state.isplayeronground = true;
        }
        if (state.isplayeronground)
        {
            state.movedz = MathCi.MaxFloat(0, state.movedz);
        }
    }

    public float Length(float x, float y, float z)
    {
        return Platform.Sqrt(x * x + y * y + z * z);
    }

    public float MakeCloserToZero(float a, float b)
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

    float[] playerposition;		//Temporarily stores the player's position. Used in WallSlide()
    public float[] WallSlide(CharacterPhysicsState state, float[] oldposition, float[] newposition, float modelheight)
    {
        bool high = false;
        if (modelheight >= 2) { high = true; }	//Set high to true if player model is bigger than standard height
        oldposition[1] += walldistance;		//Add walldistance temporarily for ground collisions
        newposition[1] += walldistance;		//Add walldistance temporarily for ground collisions

        reachedwall = false;
        reachedwall_1blockhigh = false;

        playerposition[0] = oldposition[0];
        playerposition[1] = oldposition[1];
        playerposition[2] = oldposition[2];

        // X
        if (IsEmptySpaceForPlayer(high, newposition[0], oldposition[1], oldposition[2]))
        {
            playerposition[0] = newposition[0];
        }
        else
        {
            // For autojump
            reachedwall = true;
            if (IsEmptyPoint(newposition[0], oldposition[1] + 1, oldposition[2])) { reachedwall_1blockhigh = true; }
        }
        // Y
        if (IsEmptySpaceForPlayer(high, oldposition[0], newposition[1], oldposition[2]))
        {
            playerposition[1] = newposition[1];
        }
        // Z
        if (IsEmptySpaceForPlayer(high, oldposition[0], oldposition[1], newposition[2]))
        {
            playerposition[2] = newposition[2];
        }
        else
        {
            // For autojump
            reachedwall = true;
            if (IsEmptyPoint(oldposition[0], oldposition[1] + 1, newposition[2])) { reachedwall_1blockhigh = true; }
        }
        
        standingontheground = (playerposition[1] == oldposition[1]) && (newposition[1] < oldposition[1]);

        playerposition[1] -= walldistance;	//Remove the temporary walldistance again
        return playerposition;	//Return valid position
    }

    bool IsEmptySpaceForPlayer(bool high, float x, float y, float z)
    {
        return IsEmptyPoint(x, y, z)
            && IsEmptyPoint(x, y + 1, z)
            && (!high || IsEmptyPoint(x, y + 2, z));
    }
    
    // Checks if there are no solid blocks in walldistance area around the point
    bool IsEmptyPoint(float x, float y, float z)
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
                        if (BoxPointDistance(minX, minY, minZ, maxX, maxY, maxZ, x, y, z) < walldistance)
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    // Using chebyshev distance
    float BoxPointDistance(float minX, float minY, float minZ, float maxX, float maxY, float maxZ, float pX, float pY, float pZ)
    {
        float dx = Max3(minX - pX, 0, pX - maxX);
        float dy = Max3(minY - pY, 0, pY - maxY);
        float dz = Max3(minZ - pZ, 0, pZ - maxZ);
        return Max3(dx, dy, dz);
    }

    float Max3(float a, float b, float c)
    {
        return MathCi.MaxFloat(MathCi.MaxFloat(a, b), c);
    }

    int FloatToInt(float value)
    {
        return game.platform.FloatToInt(value);
    }
}

public class BoolRef
{
    internal bool value;
    public bool GetValue() { return value; }
    public void SetValue(bool value_) { value = value_; }
}
