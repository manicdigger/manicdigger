public class CharacterPhysicsState
{
    public CharacterPhysicsState()
    {
        float one = 1;
        movedz = 0;
        playerposition = Vector3Ref.Create(15 + one / 2, 40, 15 + one / 2);
        playerorientation = Vector3Ref.Create(GetPi(), 0, 0);
        curspeed = new Vector3Ref();
        jumpacceleration = 0;
        isplayeronground = false;
    }
    internal float movedz;
    internal Vector3Ref playerposition;
    internal Vector3Ref playerorientation;
    internal Vector3Ref curspeed;
    internal float jumpacceleration;
    internal bool isplayeronground;

    static float GetPi()
    {
        float a = 3141592;
        return a / 1000000;
    }
}

public class Acceleration
{
    public Acceleration()
    {
        float one = 1;
        acceleration1 = one * 9 / 10;
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

        diff = new float[3];
        previousposition = new float[3];
        playerposition = new float[3];
    }
    internal Game game;
    internal bool swimmingtop;
    internal float walldistance;
    internal bool reachedceiling;
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
        return (blocktype.DrawType == Packet_DrawTypeEnum.HalfHeight && blockabove2 == 0 && blockabove == 0) // also check if the block above the stair is empty
            || (game.IsFluid(blocktype) && (!swimmingtop))
            || game.IsEmptyForPhysics(blocktype)
            || game.IsRail(blocktype);
    }

    public void Move(CharacterPhysicsState state, MoveInfo move, float dt, BoolRef soundnow, Vector3Ref push, float modelheight)
    {
        soundnow.value = false;
        shiftkeypressed = move.shiftkeydown;
        Vector3Ref diff1ref = new Vector3Ref();
        VectorTool.ToVectorInFixedSystem
            (move.movedx * move.movespeednow * dt,
            0,
            move.movedy * move.movespeednow * dt, state.playerorientation.X, state.playerorientation.Y, diff1ref);
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
        int cx = game.platform.FloatToInt(game.player.playerposition.X / Game.chunksize);
        int cy = game.platform.FloatToInt(game.player.playerposition.Z / Game.chunksize);
        int cz = game.platform.FloatToInt(game.player.playerposition.Y / Game.chunksize);
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
            newposition.X = state.playerposition.X + state.curspeed.X;
            newposition.Y = state.playerposition.Y + state.curspeed.Y;
            newposition.Z = state.playerposition.Z + state.curspeed.Z;
            if (!move.Swimming)
            {
                newposition.Y = state.playerposition.Y;
            }
            //fast move when looking at the ground.
            float diffx = newposition.X - state.playerposition.X;
            float diffy = newposition.Y - state.playerposition.Y;
            float diffz = newposition.Z - state.playerposition.Z;
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
            newposition.X = state.playerposition.X + diffx * dt;
            newposition.Y = state.playerposition.Y + diffy * dt;
            newposition.Z = state.playerposition.Z + diffz * dt;
        }
        else
        {
            newposition.X = state.playerposition.X + (state.curspeed.X) * dt;
            newposition.Y = state.playerposition.Y + (state.curspeed.Y) * dt;
            newposition.Z = state.playerposition.Z + (state.curspeed.Z) * dt;
        }
        newposition.Y += state.movedz * dt;
        Vector3Ref previousposition = Vector3Ref.Create(state.playerposition.X, state.playerposition.Y, state.playerposition.Z);
        if (!move.ENABLE_NOCLIP)
        {
            swimmingtop = move.wantsjump && !move.Swimming;
            // This is a temporary workaround for crashing at the top of the map.
            // This needs to be cleaned up some other way.
            //try
            {
                float[] v = WallSlide(state,
                    Vec3.FromValues(state.playerposition.X, state.playerposition.Y, state.playerposition.Z),
                    Vec3.FromValues(newposition.X, newposition.Y, newposition.Z),
                    modelheight);
                state.playerposition.X = v[0];
                state.playerposition.Y = v[1];
                state.playerposition.Z = v[2];
            }
            //catch
            {
                // The block probably doesn't exist...
            }
        }
        else
        {
            state.playerposition.X = newposition.X;
            state.playerposition.Y = newposition.Y;
            state.playerposition.Z = newposition.Z;
        }
        if (!(move.ENABLE_FREEMOVE || move.Swimming))
        {
            state.isplayeronground = state.playerposition.Y == previousposition.Y;
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
            state.movedz = Max(0, state.movedz);
        }
    }

    float Max(int a, float b)
    {
        if (a > b)
        {
            return a;
        }
        else
        {
            return b;
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

    float[] diff;
    float[] previousposition;
    float[] playerposition;		//Temporarily stores the player's position. Used in WallSlide()
    public float[] WallSlide(CharacterPhysicsState state, float[] oldposition, float[] newposition, float modelheight)
    {
        bool high = false;
        float stairjumpacceleration = one * 110 / 40;
        if (modelheight >= 2) { high = true; }	//Set high to true if player model is bigger than standard height
        oldposition[1] += walldistance;		//Add walldistance temporarily for ground collisions
        newposition[1] += walldistance;		//Add walldistance temporarily for ground collisions

        reachedceiling = false;
        reachedwall = false;
        reachedwall_1blockhigh = false;
        
        int oldPosX = FloatToInt(Floor(oldposition[0]));
        int oldPosY = FloatToInt(Floor(oldposition[2]));
        int oldPosZ = FloatToInt(Floor(oldposition[1]));
        bool wasonstairs = isHalfHeight(oldPosX, oldPosY, oldPosZ);	//Checks if player is coming from a half height block
        bool steppedonstair = false;	//Indicates if the player just stepped on a stair in this collision tick
        float half = one * 1 / 2;

        playerposition[0] = newposition[0];	//MD: X
        playerposition[1] = newposition[1];	//MD: Z - top axis
        playerposition[2] = newposition[2];	//MD: Y

        //left - positive Y axis
        {
            float p0 = newposition[0] + 0;
            float p1 = newposition[1] + 0;
            float p2 = newposition[2] + walldistance;	//Add walldistance so player is kept at a little distance
            bool newempty = NewEmpty(high, p0, p1, p2);	//Check if block at new position is empty for physics
            if (newposition[2] - oldposition[2] > 0)
            {
            	if (!wasonstairs)
            	{
                	if (!newempty)	//Block is solid
                	{
                		//New Y coordinate of position not valid. Set back to old coordinate.
                	    reachedwall = true;
                        if (NewEmpty(high, p0, p1 + 1, p2)) { reachedwall_1blockhigh = true; }
                	    playerposition[2] = oldposition[2];
                	}
                	else	//Block is empty or halfstep
                	{
                		int x = FloatToInt(Floor(newposition[0]));
                		int y = FloatToInt(Floor(newposition[2]));
                		int z = FloatToInt(Floor(newposition[1]));
                		bool newstairs = isHalfHeight(x, y, z);
                		if (newstairs)
                		{
                			float relPos = p1 - z;	//relative position inside block (between 0 and 1)
                			if (relPos < half && !steppedonstair)
                			{
                	            state.jumpacceleration = stairjumpacceleration;
                	            standingontheground = false;
                				steppedonstair = true;
                			}
                		}
                	}
            	}
            	else
            	{
            		if (!NewEmpty(high, p0, p1, newposition[2]))	//Block is solid
            		{
            			int x = FloatToInt(Floor(newposition[0]));
                		int y = FloatToInt(Floor(newposition[2]));
                		int z = FloatToInt(Floor(newposition[1]));
                		bool nextempty = IsTileEmptyForPhysics(x, y, z + 1) && !isHalfHeight(x, y, z + 1); //Ensure that block is empty and not a halfstep
                		if (!nextempty)
                		{
                			reachedwall = true;
                			playerposition[2] = oldposition[2];
                		}
                		else	//Block is empty
                		{
                			float relPos = p1 - z;	//relative position inside block (between 0 and 1)
                			if (relPos >= half && !steppedonstair)
                			{
                				state.jumpacceleration = stairjumpacceleration;
                	            standingontheground = false;
                				steppedonstair = true;
                			}
                		}
            		}
            	}
            }
        }
        //front - positive X axis
        {
            float p0 = newposition[0] + walldistance;	//Add walldistance so player is kept at a little distance
            float p1 = newposition[1] + 0;
            float p2 = newposition[2] + 0;
            bool newempty = NewEmpty(high, p0, p1, p2);	//Check if block at new position is empty for physics
            if (newposition[0] - oldposition[0] > 0)
            {
                if (!wasonstairs)
            	{
                	if (!newempty)	//Block is solid
                	{
                		//New X coordinate of position not valid. Set back to old coordinate.
                	    reachedwall = true;
                        if (NewEmpty(high, p0, p1 + 1, p2)) { reachedwall_1blockhigh = true; }
                	    playerposition[0] = oldposition[0];
                	}
                	else	//Block is empty or halfstep
                	{
                		int x = FloatToInt(Floor(newposition[0]));
                		int y = FloatToInt(Floor(newposition[2]));
                		int z = FloatToInt(Floor(newposition[1]));
                		bool newstairs = isHalfHeight(x, y, z);
                		if (newstairs)
                		{
                			float relPos = p1 - z;	//relative position inside block (between 0 and 1)
                			if (relPos < half && !steppedonstair)
                			{
                				state.jumpacceleration = stairjumpacceleration;
                	            standingontheground = false;
                				steppedonstair = true;
                			}
                		}
                	}
            	}
            	else
            	{
            		if (!NewEmpty(high, newposition[0], p1, p2))	//Block is solid
            		{
            			int x = FloatToInt(Floor(newposition[0]));
                		int y = FloatToInt(Floor(newposition[2]));
                		int z = FloatToInt(Floor(newposition[1]));
                		bool nextempty = IsTileEmptyForPhysics(x, y, z + 1) && !isHalfHeight(x, y, z + 1); //Ensure that block is empty and not a halfstep
                		if (!nextempty)
                		{
                			reachedwall = true;
                			playerposition[0] = oldposition[0];
                		}
                		else	//Block is empty
                		{
                			float relPos = p1 - z;	//relative position inside block (between 0 and 1)
                			if (relPos >= half && !steppedonstair)
                			{
                				state.jumpacceleration = stairjumpacceleration;
                	            standingontheground = false;
                				steppedonstair = true;
                			}
                		}
            		}
            	}
            }
        }
        //top - negative Z axis. Floor collision.
        {
            float qnewposition0 = newposition[0] + 0;
            float qnewposition1 = newposition[1] - walldistance;
            float qnewposition2 = newposition[2] + 0;
            int x = FloatToInt(Floor(qnewposition0));
            int y = FloatToInt(Floor(qnewposition2));
            int z = FloatToInt(Floor(qnewposition1));
            float a = walldistance;
            //Check if block at new Z coordinate is NOT empty for physics (check for solid block)
            bool newfull = (!IsTileEmptyForPhysics(x, y, z))
            	//These 4 lines let you walk a little over the block's edge. Also part of the 2 block high jump problem
                || (qnewposition0 - Floor(qnewposition0) <= a && (!IsTileEmptyForPhysics(x - 1, y, z)) && (IsTileEmptyForPhysics(x - 1, y, z + 1)) && shiftkeypressed)
                || (qnewposition0 - Floor(qnewposition0) >= (1 - a) && (!IsTileEmptyForPhysics(x + 1, y, z)) && (IsTileEmptyForPhysics(x + 1, y, z + 1)) && shiftkeypressed)
                || (qnewposition2 - Floor(qnewposition2) <= a && (!IsTileEmptyForPhysics(x, y - 1, z)) && (IsTileEmptyForPhysics(x, y - 1, z + 1)) && shiftkeypressed)
                || (qnewposition2 - Floor(qnewposition2) >= (1 - a) && (!IsTileEmptyForPhysics(x, y + 1, z)) && (IsTileEmptyForPhysics(x, y + 1, z + 1)) && shiftkeypressed);
            bool newhalf = isHalfHeight(x, y, z);
            if (newposition[1] - oldposition[1] < 0)
            {
                if (newfull && !steppedonstair)
                {
                	//If new block is solid, don't change height of position (no falling through solid blocks)
                	playerposition[1] = oldposition[1];
                    standingontheground = true;
                }
                else if (newhalf && !steppedonstair)
                {
                	//Block is half height
                	float relPos = qnewposition1 - z;	//relative position inside block (between 0 and 1)
                	if (relPos < half)
                	{
                		playerposition[1] = oldposition[1];
                    	standingontheground = true;
                	}
                }
            }
        }
        //right - negative Y axis
        {
            float p0 = newposition[0] + 0;
            float p1 = newposition[1] + 0;
            float p2 = newposition[2] - walldistance;	//Add walldistance so player is kept at a little distance
            bool newempty = NewEmpty(high, p0, p1, p2);	//Check if block at new position is empty for physics
            if (newposition[2] - oldposition[2] < 0)
            {
                if (!wasonstairs)
            	{
                	if (!newempty)	//Block is solid
                	{
                		//New Y coordinate of position not valid. Set back to old coordinate.
                	    reachedwall = true;
                        if (NewEmpty(high, p0, p1 + 1, p2)) { reachedwall_1blockhigh = true; }
                	    playerposition[2] = oldposition[2];
                	}
                	else	//Block is empty or halfstep
                	{
                		int x = FloatToInt(Floor(newposition[0]));
                		int y = FloatToInt(Floor(newposition[2]));
                		int z = FloatToInt(Floor(newposition[1]));
                		bool newstairs = isHalfHeight(x, y, z);
                		if (newstairs)
                		{
                			float relPos = p1 - z;	//relative position inside block (between 0 and 1)
                			if (relPos < half && !steppedonstair)
                			{
                				state.jumpacceleration = stairjumpacceleration;
                	            standingontheground = false;
                				steppedonstair = true;
                			}
                		}
                	}
            	}
            	else
            	{
            		if (!NewEmpty(high, p0, p1, newposition[2]))	//Block is solid
            		{
            			int x = FloatToInt(Floor(newposition[0]));
                		int y = FloatToInt(Floor(newposition[2]));
                		int z = FloatToInt(Floor(newposition[1]));
                		bool nextempty = IsTileEmptyForPhysics(x, y, z + 1) && !isHalfHeight(x, y, z + 1); //Ensure that block is empty and not a halfstep
                		if (!nextempty)
                		{
                			reachedwall = true;
                			playerposition[2] = oldposition[2];
                		}
                		else	//Block is empty
                		{
                			float relPos = p1 - z;	//relative position inside block (between 0 and 1)
                			if (relPos >= half && !steppedonstair)
                			{
                				state.jumpacceleration = stairjumpacceleration;
                	            standingontheground = false;
                				steppedonstair = true;
                			}
                		}
            		}
            	}
            }
        }
        //back - negative X axis
        {
            float p0 = newposition[0] - walldistance;	//Add walldistance so player is kept at a little distance
            float p1 = newposition[1] + 0;
            float p2 = newposition[2] + 0;
            bool newempty = NewEmpty(high, p0, p1, p2);	//Check if block at new position is empty for physics
            if (newposition[0] - oldposition[0] < 0)
            {
                if (!wasonstairs)
            	{
                	if (!newempty)	//Block is solid
                	{
                		//New X coordinate of position not valid. Set back to old coordinate.
                	    reachedwall = true;
                        if (NewEmpty(high, p0, p1 + 1, p2)) { reachedwall_1blockhigh = true; }
                	    playerposition[0] = oldposition[0];
                	}
                	else	//Block is empty or halfstep
                	{
                		int x = FloatToInt(Floor(newposition[0]));
                		int y = FloatToInt(Floor(newposition[2]));
                		int z = FloatToInt(Floor(newposition[1]));
                		bool newstairs = isHalfHeight(x, y, z);
                		if (newstairs)
                		{
                			float relPos = p1 - z;	//relative position inside block (between 0 and 1)
                			if (relPos < half && !steppedonstair)
                			{
                				state.jumpacceleration = stairjumpacceleration;
                	            standingontheground = false;
                				steppedonstair = true;
                			}
                		}
                	}
            	}
            	else
            	{
            		if (!NewEmpty(high, newposition[0], p1, p2))	//Block is solid
            		{
            			int x = FloatToInt(Floor(newposition[0]));
                		int y = FloatToInt(Floor(newposition[2]));
                		int z = FloatToInt(Floor(newposition[1]));
                		bool nextempty = IsTileEmptyForPhysics(x, y, z + 1) && !isHalfHeight(x, y, z + 1); //Ensure that block is empty and not a halfstep
                		if (!nextempty)
                		{
                			reachedwall = true;
                			playerposition[0] = oldposition[0];
                		}
                		else	//Block is empty
                		{
                			float relPos = p1 - z;	//relative position inside block (between 0 and 1)
                			if (relPos >= half && !steppedonstair)
                			{
                				state.jumpacceleration = stairjumpacceleration;
                	            standingontheground = false;
                				steppedonstair = true;
                			}
                		}
            		}
            	}
            }
        }
        //bottom - positive Z axis. Ceiling collision.
        {
            float p0 = newposition[0] + 0;
            float p1 = newposition[1] + modelheight;	//Add model height to correctly determine ceiling collisions
            float p2 = newposition[2] + 0;
            bool newempty = IsTileEmptyForPhysics(FloatToInt(p0), FloatToInt(p2), FloatToInt(p1));	//Check if block at new position is empty for physics
            if (newposition[1] - oldposition[1] > 0)
            {
                if (!newempty)
                {
                    reachedwall = true;
                    if (NewEmpty(high, p0, p1 + 1, p2)) { reachedwall_1blockhigh = true; }
                    playerposition[1] = oldposition[1];
                }
            }
        }
        playerposition[1] -= walldistance;	//Remove the temporary walldistance again
        return playerposition;	//Return valid position
    }
    
    bool isHalfHeight(int x, int y, int z)
    {
        int block = game.GetBlock(x, y, z);
    	return (game.blocktypes[block].DrawType == Packet_DrawTypeEnum.HalfHeight
    	        || game.IsRail(game.blocktypes[block]));
    }

    float Floor(float aFloat)
    {
        if (aFloat > 0)
        {
            return FloatToInt(aFloat);
        }
        else
        {
            return FloatToInt(aFloat) - 1;
        }
    }

    bool NewEmpty(bool high, float qnewposition0, float qnewposition1, float qnewposition2)
    {
        int x = FloatToInt(Floor(qnewposition0));
        int y = FloatToInt(Floor(qnewposition1));
        int z = FloatToInt(Floor(qnewposition2));
        return IsTileEmptyForPhysics(x, z, y)
                    && IsTileEmptyForPhysics(x, z, y + 1)
                    && (!high || IsTileEmptyForPhysics(x, z, y + 2));
    }

    int FloatToInt(float qnewposition0)
    {
        return game.platform.FloatToInt(qnewposition0);
    }
}

public class BoolRef
{
    internal bool value;
    public bool GetValue() { return value; }
    public void SetValue(bool value_) { value = value_; }
}
