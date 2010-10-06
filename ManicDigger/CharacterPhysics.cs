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
        public IMapStorage map { get; set; }
        [Inject]
        public IGameData data { get; set; }
        bool IsTileEmptyForPhysics(int x, int y, int z)
        {
            if (z >= map.MapSizeZ)
            {
                return true;
            }
            bool ENABLE_FREEMOVE = false;
            if (x < 0 || y < 0 || z < 0)// || z >= mapsizez)
            {
                return ENABLE_FREEMOVE;
            }
            if (x >= map.MapSizeX || y >= map.MapSizeY)// || z >= mapsizez)
            {
                return ENABLE_FREEMOVE;
            }
            //this test is so the player does not walk on water.
            if (data.IsWaterTile(map.GetBlock(x, y, z)) &&
                !data.IsWaterTile(map.GetBlock(x, y, z + 1))) { return true; }
            return map.GetBlock(x, y, z) == data.TileIdEmpty
                || map.GetBlock(x, y, z) == data.TileIdSingleStairs
                || (data.IsWaterTile(map.GetBlock(x, y, z)) && (!swimmingtop))
                || data.IsEmptyForPhysics(map.GetBlock(x, y, z));
        }

        bool IsTileEmptyForStairs(int x, int y, int z)
        {

            return map.GetBlock(x, y, z) == data.TileIdSingleStairs;
        }

        
        public static float walldistance = 0.3f; // char size
        public static float characterheight = 1.2f;
        public float gravity = 0.023f; // was 0.3f- no longer is constant speed
        public float maxgravity = -0.5f; // new - sets a maximum fall speed
        public bool jumping = false; // new - just to keep you from jumping over and over in the air
        public float thrust = 0f;
        public float thrustspeed = 0.1f;
        public float fallspeed = 0.0f; // new - adjusts with gravity and jumping for your actual Z movement
        public float WaterGravityMultiplier = 15;
        public bool enable_acceleration = true;
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
            public float jumpstartacceleration;
        }
        public void Move(CharacterPhysicsState state, MoveInfo move, double dt, out bool soundnow)
        {
            soundnow = false;
            var diff1 = VectorTool.toVectorInFixedSystem1
                (move.movedx * move.movespeednow * (float)dt,
                0,
                move.movedy * move.movespeednow * (float)dt, state.playerorientation.X, state.playerorientation.Y);
            if (!(move.ENABLE_FREEMOVE))
            {
                if (!move.Swimming)
                {
                    // new stuff here
                    fallspeed -= gravity;
                    if (fallspeed < maxgravity)
                    {
                        fallspeed = maxgravity;
                    }


                    if (thrust > 0 && fallspeed < 0)
                    {
                        fallspeed = thrustspeed;
                        thrust -= 1;
                    }
                    else
                    {
                        thrust = 0; 
                    }

                    if (this.reachedceiling && fallspeed > 0f)
                    {
                        fallspeed = -0.2f;
                        state.jumpacceleration = 0f;
                    }
                    state.movedz += fallspeed;

                    // state.movedz += -gravity;//gravity  // old version
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
                state.playerposition = WallSlide(state.playerposition, newposition);
            }
            else
            {
                state.playerposition = newposition;
            }
            if (!(move.ENABLE_FREEMOVE || move.Swimming))
            {
                state.isplayeronground = state.playerposition.Y == previousposition.Y;
                {

                    if (move.wantsjump && state.isplayeronground && state.jumpacceleration <= 0 && jumping == false) // added jumping check
                    {

                            state.jumpacceleration = move.jumpstartacceleration;
                            fallspeed = state.jumpacceleration * 9;
                            jumping = true;
                            soundnow = true;
             
                       
                    }
                    if (state.jumpacceleration < 0)
                    {
                        state.jumpacceleration = 0;
                        state.movedz = 0;
                    }
                    if (state.jumpacceleration > 0)
                    {
                        state.jumpacceleration -= (float)dt * 2.8f;
                    }
                    if (!this.reachedceiling)
                    {
                        state.movedz += state.jumpacceleration * 2;
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
        public Vector3 WallSlide(Vector3 oldposition, Vector3 newposition)
        {
            reachedceiling = false;
            reachedwall = false;
            //Math.Floor() is needed because casting negative values to integer is not floor.
            Vector3i oldpositioni = new Vector3i((int)Math.Floor(oldposition.X), (int)Math.Floor(oldposition.Z),
                (int)Math.Floor(oldposition.Y));
            bool wasonstairs = false;
            if (MapUtil.IsValidPos(map, oldpositioni.x, oldpositioni.y, oldpositioni.z))
            {
                wasonstairs = map.GetBlock(oldpositioni.x, oldpositioni.y, oldpositioni.z) == data.TileIdSingleStairs;
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
                        else
                        {
                            // IsTileEmptyForStairs
                            int x = (int)Math.Floor(qnewposition.X);
                            int y = (int)Math.Floor(qnewposition.Y);
                            int z = (int)Math.Floor(qnewposition.Z);
                            bool newstairs = (!IsTileEmptyForStairs(x, z, y));
                            bool newstairs2 = (!IsTileEmptyForStairs(x, z, y + 1));

                            if (!newstairs2)
                            {
                                reachedwall = true;
                                playerposition.Z = oldposition.Z;
                            }
                            if (!newstairs && newstairs2)
                            {

                            }

                        }
                    }
                    else
                    {
                        if (!newempty)
                        {
                            bool nextempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
                                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2);
                            if (!nextempty)
                            {
                                reachedwall = true;
                                playerposition.Z = oldposition.Z;

                            }
                            else
                            {
                                playerposition.Y += 0.5f;
                                goto ok;
                            }
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
                        else
                        {
                            // IsTileEmptyForStairs
                            int x = (int)Math.Floor(qnewposition.X);
                            int y = (int)Math.Floor(qnewposition.Y);
                            int z = (int)Math.Floor(qnewposition.Z);
                            bool newstairs = (!IsTileEmptyForStairs(x, z, y));
                            bool newstairs2 = (!IsTileEmptyForStairs(x, z, y + 1));

                            if (!newstairs2)
                            {
                                reachedwall = true;
                                playerposition.X = oldposition.X;
                            }
                            if (!newstairs && newstairs2)
                            {

                            }

                        }

                    }
                    else
                    {
                        if (!newempty)
                        {
                            bool nextempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
                                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2);
                            if (!nextempty)
                            {
                                reachedwall = true;
                                playerposition.X = oldposition.X;

                            }
                            else
                            {
                                playerposition.Y += 0.5f;
                                goto ok;
                            }
                        }
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
                        else
                        {
                            // IsTileEmptyForStairs
                            int x = (int)Math.Floor(qnewposition.X);
                            int y = (int)Math.Floor(qnewposition.Y);
                            int z = (int)Math.Floor(qnewposition.Z);
                            bool newstairs = (!IsTileEmptyForStairs(x, z, y));
                            bool newstairs2 = (!IsTileEmptyForStairs(x, z, y + 1));

                            if (!newstairs2)
                            {
                                reachedwall = true;
                                playerposition.Z = oldposition.Z;
                            }
                            if (!newstairs && newstairs2)
                            {

                            }

                        }
                    }
                    else
                    {
                        if (!newempty)
                        {
                         
                            bool nextempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
                                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2);
                            if (!nextempty)
                            {
                                reachedwall = true;
                                playerposition.Z = oldposition.Z;

                            }
                            else
                            {
                                playerposition.Y += 0.5f;
                                goto ok;
                            }


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
                        else
                        {
                            // IsTileEmptyForStairs
                            int x = (int)Math.Floor(qnewposition.X);
                            int y = (int)Math.Floor(qnewposition.Y);
                            int z = (int)Math.Floor(qnewposition.Z);
                            bool newstairs = (!IsTileEmptyForStairs(x, z, y));
                            bool newstairs2 = (!IsTileEmptyForStairs(x, z, y + 1));

                            if (!newstairs2)
                            {
                                reachedwall = true;
                                playerposition.X = oldposition.X;
                            }
                            if (!newstairs && newstairs2)
                            {
                                
                            }

                        }
                    }
                    else
                    {
                        if (!newempty)
                        {
                            bool nextempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
                                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2);
                            if (!nextempty)
                            {
                                reachedwall = true;
                                playerposition.X = oldposition.X;

                            }
                            else
                            {
                                playerposition.Y += 0.5f;
                                goto ok;
                            }
                        }
                    }
                }
            }
            //bottom of block
            {
                var qnewposition = newposition + new Vector3(0, +walldistance + characterheight, 0);
                qnewposition.X = playerposition.X; // updates to prevent going through the 
                qnewposition.Z = playerposition.Z; // updates to prevent going through the ceiling rarely
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y));
                if (newposition.Y - oldposition.Y > 0)
                {
                    if (!newempty)
                    {

                        playerposition.Y = oldposition.Y;
                        reachedceiling = true;
                        
                    }
                    else
                    {
                        // IsTileEmptyForStairs
                        int x = (int)Math.Floor(qnewposition.X);
                        int y = (int)Math.Floor(qnewposition.Y); 
                        int z = (int)Math.Floor(qnewposition.Z); 
                        bool newstairs = (!IsTileEmptyForStairs(x, z, y));
              
                        if (!newstairs)
                        {
                            playerposition.Y = oldposition.Y;
                            reachedceiling = true;
                        }

                    }
                }
            }

            //top of block
            {
                var qnewposition = newposition + new Vector3(0, -walldistance, 0);
                qnewposition.X = playerposition.X;
                qnewposition.Z = playerposition.Z;
                int x = (int)Math.Floor(qnewposition.X);
                int y = (int)Math.Floor(qnewposition.Y); 
                int z = (int)Math.Floor(qnewposition.Z); 
                float a = walldistance;
                bool newfull = (!IsTileEmptyForPhysics(x, z, y))
                    || (qnewposition.X - Math.Floor(qnewposition.X) <= a && (!IsTileEmptyForPhysics(x - 1, z, y)) && (IsTileEmptyForPhysics(x - 1, z, y + 1)))
                    || (qnewposition.X - Math.Floor(qnewposition.X) >= (1 - a) && (!IsTileEmptyForPhysics(x + 1, z, y)) && (IsTileEmptyForPhysics(x + 1, z, y + 1)))
                    || (qnewposition.Z - Math.Floor(qnewposition.Z) <= a && (!IsTileEmptyForPhysics(x, z - 1, y)) && (IsTileEmptyForPhysics(x, z - 1, y + 1)))
                    || (qnewposition.Z - Math.Floor(qnewposition.Z) >= (1 - a) && (!IsTileEmptyForPhysics(x, z + 1, y)) && (IsTileEmptyForPhysics(x, z + 1, y + 1)));
                if (newposition.Y - oldposition.Y < 0)
                {
                    if (newfull)
                    {

                        playerposition.Y = oldposition.Y;
                        jumping = false;
                        fallspeed = 0f;
                    }
                    else
                    {
                        // IsTileEmptyForStairs
                        bool newstairs = (!IsTileEmptyForStairs(x, z, y))
                        || (qnewposition.X - Math.Floor(qnewposition.X) <= a && (!IsTileEmptyForStairs(x - 1, z, y)) && (IsTileEmptyForStairs(x - 1, z, y + 1)))
                        || (qnewposition.X - Math.Floor(qnewposition.X) >= (1 - a) && (!IsTileEmptyForStairs(x + 1, z, y)) && (IsTileEmptyForStairs(x + 1, z, y + 1)))
                        || (qnewposition.Z - Math.Floor(qnewposition.Z) <= a && (!IsTileEmptyForStairs(x, z - 1, y)) && (IsTileEmptyForStairs(x, y - 1, y + 1)))
                        || (qnewposition.Z - Math.Floor(qnewposition.Z) >= (1 - a) && (!IsTileEmptyForStairs(x, z + 1, y)) && (IsTileEmptyForStairs(x, z + 1, y + 1)));

                        if (!newstairs && qnewposition.Y - y < 0.5)
                        {
                            //Console.WriteLine("small" + z + "qnewposition" + qnewposition.Y);
                            playerposition.Y = oldposition.Y;
                            jumping = false;
                            fallspeed = 0f;
                        }

                    }
                }
            }
        ok:
            bool isonstairs = false;
            Vector3i playerpositioni = new Vector3i((int)Math.Floor(playerposition.X), (int)Math.Floor(playerposition.Z),
                 (int)Math.Floor(playerposition.Y));
            if (MapUtil.IsValidPos(map, playerpositioni.x, playerpositioni.y, playerpositioni.z))
            {
                isonstairs = map.GetBlock(playerpositioni.x, playerpositioni.y, playerpositioni.z) == data.TileIdSingleStairs;
            }
            if (isonstairs)
            {
                // new detection- checks if we should connect with a stair- and makes sure we aren't going up from jumping already
                if (playerposition.Y < ((int)Math.Floor(playerposition.Y)) + 0.5f + walldistance && fallspeed <= 0f)
                {
                    jumping = false;
                    fallspeed = 0f;
                    playerposition.Y += 0.03f;

                }
                //playerposition.Y = ((int)Math.Floor(playerposition.Y)) + 0.5f + walldistance;
            }
            return playerposition;
        }
        public bool swimmingtop;
        public bool reachedceiling;
        public bool reachedwall;
    }
}