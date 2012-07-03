using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace ManicDigger
{
    public class CharacterPhysicsState
    {
        public float movedz = 0;
        public Vector3 playerposition = new Vector3(15.5f, 40, 15.5f);
        public Vector3 playerorientation = new Vector3((float)Math.PI, 0, 0);
        public Vector3 curspeed;
        public float jumpacceleration = 0;
        public bool isplayeronground;
    }
    public class Acceleration
    {
        public float acceleration1 = 0.90f;
        public float acceleration2 = 2f;
        public float acceleration3 = 700f;
    }
    public class CharacterPhysics
    {
        [Inject]
        public IMapStorage d_Map;
        [Inject]
        public IGameDataPhysics d_Data;
        bool IsTileEmptyForPhysics(int x, int y, int z)
        {
            if (z >= d_Map.MapSizeZ)
            {
                return true;
            }
            bool ENABLE_FREEMOVE = false;
            if (x < 0 || y < 0 || z < 0)// || z >= mapsizez)
            {
                return ENABLE_FREEMOVE;
            }
            if (x >= d_Map.MapSizeX || y >= d_Map.MapSizeY)// || z >= mapsizez)
            {
                return ENABLE_FREEMOVE;
            }
            //this test is so the player does not walk on water.
            if (d_Data.IsFluid[d_Map.GetBlock(x, y, z)] &&
                !d_Data.IsFluid[d_Map.GetBlock(x, y, z + 1)]) { return true; }
            return d_Map.GetBlock(x, y, z) == 0
                || (d_Map.GetBlock(x, y, z) == d_Data.BlockIdSingleStairs && d_Map.GetBlock(x, y, z+2) == 0 && d_Map.GetBlock(x, y, z+1) == 0) // also check if the block above the stair is empty
                || (d_Data.IsFluid[d_Map.GetBlock(x, y, z)] && (!swimmingtop))
                || d_Data.IsEmptyForPhysics[d_Map.GetBlock(x, y, z)];
        }
        public static float walldistance = 0.3f;
        public static float characterheight = 1.5f;
        public float gravity = 0.3f;
        public float WaterGravityMultiplier = 3;
        public bool enable_acceleration = true;
        public bool standingontheground;
        public class MoveInfo
        {
            public bool ENABLE_FREEMOVE;
            public bool Swimming;
            public Acceleration acceleration;
            public float movespeednow;
            public int movedx;
            public int movedy;
            public bool ENABLE_NOCLIP;
            public bool wantsjump;
            public bool moveup;
            public bool movedown;
            public float jumpstartacceleration;
        }
        public void Move(CharacterPhysicsState state, MoveInfo move, double dt, out bool soundnow, Vector3 push)
        {
            
            soundnow = false;
            Vector3 diff1 = VectorTool.ToVectorInFixedSystem
                (move.movedx * move.movespeednow * (float)dt,
                0,
                move.movedy * move.movespeednow * (float)dt, state.playerorientation.X, state.playerorientation.Y);
            if (push.Length > 0.01f)
            {
                push.Normalize();
                push *= 5;
            }
            diff1 += push * (float)dt;
            if (!(move.ENABLE_FREEMOVE))
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
                state.curspeed *= move.acceleration.acceleration1;
                state.curspeed.X = MakeCloserToZero(state.curspeed.X, move.acceleration.acceleration2 * (float)dt);
                state.curspeed.Y = MakeCloserToZero(state.curspeed.Y, move.acceleration.acceleration2 * (float)dt);
                state.curspeed.Z = MakeCloserToZero(state.curspeed.Z, move.acceleration.acceleration2 * (float)dt);
                diff1.Y += move.moveup ? 2 * move.movespeednow * (float)dt : 0;
                diff1.Y -= move.movedown ? 2 * move.movespeednow * (float)dt : 0;
                state.curspeed += Vector3.Multiply(diff1, move.acceleration.acceleration3 * (float)dt);
                if (state.curspeed.Length > move.movespeednow)
                {
                    state.curspeed.Normalize();
                    state.curspeed *= move.movespeednow;
                }
            }
            else
            {
                if (diff1.Length > 0)
                {
                    diff1.Normalize();
                }
                state.curspeed = diff1 * move.movespeednow;
            }
            Vector3 newposition;
            if (!(move.ENABLE_FREEMOVE))
            {
                newposition = state.playerposition + state.curspeed;
                if (!move.Swimming)
                {
                    newposition.Y = state.playerposition.Y;
                }
                //fast move when looking at the ground.
                var diff = newposition - state.playerposition;
                if (diff.Length > 0)
                {
                    diff.Normalize();
                    diff *= 1 * state.curspeed.Length;
                }
                newposition = state.playerposition + diff * (float)dt;
            }
            else
            {
                newposition = state.playerposition + (state.curspeed) * (float)dt;
            }
            newposition.Y += state.movedz * (float)dt;
            Vector3 previousposition = state.playerposition;
            if (!move.ENABLE_NOCLIP)
            {
                this.swimmingtop = move.wantsjump && !move.Swimming;
                // This is a temporary workaround for crashing at the top of the map.
                // This needs to be cleaned up some other way.
                try
                {
                    state.playerposition = WallSlide(state, state.playerposition, newposition);
                }
                catch
                {
                    // The block probably doesn't exist...
                }
            }
            else
            {
                state.playerposition = newposition;
            }
            if (!(move.ENABLE_FREEMOVE || move.Swimming))
            {
                state.isplayeronground = state.playerposition.Y == previousposition.Y;
                {
                    if (standingontheground && state.isplayeronground)
                    {
                        state.jumpacceleration = 0;
                        state.movedz = 0f;
                    }
                    if (move.wantsjump && state.jumpacceleration == 0 && standingontheground)
                    {
                        state.jumpacceleration = move.jumpstartacceleration;
                        soundnow = true;
                    }
                    
                    if (state.jumpacceleration > 0)
                    {
                        standingontheground = false;
                        state.jumpacceleration = state.jumpacceleration / 2;
                    }
                   
                    //if (!this.reachedceiling)
                    {
                       state.movedz += state.jumpacceleration * 2.1f;
                    }
                }
            }
            else
            {
                state.isplayeronground = true;
            }
            if (state.isplayeronground)
            {
                state.movedz = Math.Max(0, state.movedz);
            }
        }
        float MakeCloserToZero(float a, float b)
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
        public Vector3 WallSlide(CharacterPhysicsState state, Vector3 oldposition, Vector3 newposition)
        {
            reachedceiling = false;
            reachedwall = false;
            //Math.Floor() is needed because casting negative values to integer is not floor.
            Vector3i oldpositioni = new Vector3i((int)Math.Floor(oldposition.X), (int)Math.Floor(oldposition.Z),
                (int)Math.Floor(oldposition.Y));
            bool wasonstairs = false;
            if (MapUtil.IsValidPos(d_Map, oldpositioni.x, oldpositioni.y, oldpositioni.z))
            {
                wasonstairs = d_Map.GetBlock(oldpositioni.x, oldpositioni.y, oldpositioni.z) == d_Data.BlockIdSingleStairs;
            }
            Vector3 playerposition = newposition;
            //left
            {
                var qnewposition = newposition + new Vector3(0, 0, walldistance);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1);
                if (newposition.Z - oldposition.Z > 0)
                {
                    if (!wasonstairs)
                    {
                        if (!newempty)
                        {
                            reachedwall = true;
                            playerposition.Z = oldposition.Z;
                        }
                    }
                    else
                    {
                        bool aboveempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
					    && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2);
						// if the new coord isnt passable stop the player from moving
						if (aboveempty && !IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y)))
						{
							playerposition.Y += 0.5f;
                        	goto ok;
						}
						if (!aboveempty)
						{
                        	reachedwall = true;
                        	playerposition.Z = oldposition.Z;
						}
                        
						
                    }
                }
            }
            //front
            {
                var qnewposition = newposition + new Vector3(walldistance, 0, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1);
                if (newposition.X - oldposition.X > 0)
                {
                    if (!wasonstairs)
                    {
                        if (!newempty)
                        {
                            reachedwall = true;
                            playerposition.X = oldposition.X;
                        }

                    }
                    else
                    {
                        bool aboveempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
					    && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2);
						// if the new coord isnt passable stop the player from moving
						if (aboveempty && !IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y)))
						{
							playerposition.Y += 0.5f;
                        	goto ok;
						}
						if (!aboveempty)
						{
                        	reachedwall = true;
                        	playerposition.X = oldposition.X;
                        }
                        
						
                    }
                }
            }
            //top
            {
                var qnewposition = newposition + new Vector3(0, -walldistance, 0);
                int x = (int)Math.Floor(qnewposition.X);
                int y = (int)Math.Floor(qnewposition.Z);
                int z = (int)Math.Floor(qnewposition.Y);
                float a = walldistance;
                bool newfull = (!IsTileEmptyForPhysics(x, y, z))
                    || (qnewposition.X - Math.Floor(qnewposition.X) <= a && (!IsTileEmptyForPhysics(x - 1, y, z)) && (IsTileEmptyForPhysics(x - 1, y, z + 1)))
                    || (qnewposition.X - Math.Floor(qnewposition.X) >= (1 - a) && (!IsTileEmptyForPhysics(x + 1, y, z)) && (IsTileEmptyForPhysics(x + 1, y, z + 1)))
                    || (qnewposition.Z - Math.Floor(qnewposition.Z) <= a && (!IsTileEmptyForPhysics(x, y - 1, z)) && (IsTileEmptyForPhysics(x, y - 1, z + 1)))
                    || (qnewposition.Z - Math.Floor(qnewposition.Z) >= (1 - a) && (!IsTileEmptyForPhysics(x, y + 1, z)) && (IsTileEmptyForPhysics(x, y + 1, z + 1)));
                if (newposition.Y - oldposition.Y < 0)
                {
                    if (newfull)
                    {
                        playerposition.Y = oldposition.Y;
                        standingontheground = true;
                    }
                    
                }
            }
            //right
            {
                var qnewposition = newposition + new Vector3(0, 0, -walldistance);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1);
                if (newposition.Z - oldposition.Z < 0)
                {
                    if (!wasonstairs)
                    {
                        if (!newempty)
                        {
                            reachedwall = true;
                            playerposition.Z = oldposition.Z;
                        }
                    }
                    else
                    {
                        bool aboveempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
					    && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2);
						// if the new coord isnt passable stop the player from moving
						if (aboveempty && !IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y)))
						{
							playerposition.Y += 0.5f;
                        	goto ok;
						}
						if (!aboveempty)
						{
                        	reachedwall = true;
                        	playerposition.Z = oldposition.Z;
                        }
                        
						
                    }
                }
            }
            //back
            {
                var qnewposition = newposition + new Vector3(-walldistance, 0, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1);
                if (newposition.X - oldposition.X < 0)
                {
                    if (!wasonstairs)
                    {
                        if (!newempty)
                        {
                            reachedwall = true;
                            playerposition.X = oldposition.X;
                        }
                    }
                    else
                    {
                        bool aboveempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
					    && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2);
						// if the new coord isnt passable stop the player from moving
						if (aboveempty && !IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y)))
						{
							playerposition.Y += 0.5f;
                        	goto ok;
						}
						if (!aboveempty)
						{
                        	reachedwall = true;
                        	playerposition.X = oldposition.X;
						}
                        
						
                    }
                }
            }
            //bottom
            {
                var qnewposition = newposition + new Vector3(0, +walldistance + characterheight, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y));
                if (newposition.Y - oldposition.Y > 0)
                {
                    if (!newempty)
                    {
                        playerposition.Y = oldposition.Y;
                        reachedceiling = true;
                    }
                }
            }
        ok:
            bool isonstairs = false;
            Vector3i playerpositioni = new Vector3i((int)Math.Floor(playerposition.X), (int)Math.Floor(playerposition.Z),
                 (int)Math.Floor(playerposition.Y));
            if (MapUtil.IsValidPos(d_Map, playerpositioni.x, playerpositioni.y, playerpositioni.z))
            {
                isonstairs = d_Map.GetBlock(playerpositioni.x, playerpositioni.y, playerpositioni.z) == d_Data.BlockIdSingleStairs;
            }
			
            if (isonstairs && state.jumpacceleration == 0)
            {
                playerposition.Y = ((int)Math.Floor(playerposition.Y)) + 0.5f + walldistance;
            }
            return playerposition;
        }
        public bool swimmingtop;
        public bool reachedceiling;
        public bool reachedwall;
    }
}
