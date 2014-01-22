using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace ManicDigger
{
    public class CharacterPhysics
    {
        [Inject]
        public ManicDiggerGameWindow game;
        [Inject]
        public IGameData d_Data;
        bool IsTileEmptyForPhysics(int x, int y, int z)
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
            //this test is so the player does not walk on water.
            if (game.blocktypes[game.GetBlock(x, y, z)].IsFluid() &&
                !game.blocktypes[game.GetBlock(x, y, z + 1)].IsFluid()) { return true; }
            return game.GetBlock(x, y, z) == 0
                || (game.blocktypes[game.GetBlock(x, y, z)].DrawType == DrawType.HalfHeight && game.GetBlock(x, y, z+2) == 0 && game.GetBlock(x, y, z+1) == 0) // also check if the block above the stair is empty
                || (game.blocktypes[game.GetBlock(x, y, z)].IsFluid() && (!swimmingtop))
                || d_Data.IsEmptyForPhysics[game.GetBlock(x, y, z)];
        }
        public static float walldistance = 0.3f;
        //public static float characterheight = 1.5f;
        public float gravity = 0.3f;
        public float WaterGravityMultiplier = 3;
        public bool enable_acceleration = true;
        public bool standingontheground;
        public void Move(CharacterPhysicsState state, MoveInfo move, double dt, out bool soundnow, Vector3 push, float modelheight)
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
            bool loaded = false;
            int cx = (int)(game.LocalPlayerPosition.X / game.chunksize);
            int cy = (int)(game.LocalPlayerPosition.Z / game.chunksize);
            int cz = (int)(game.LocalPlayerPosition.Y / game.chunksize);
            if (MapUtil.IsValidChunkPos(game, cx, cy, cz, game.chunksize))
            {
                if (game.chunks[MapUtil.Index3d(cx, cy, cz,
                    game.MapSizeX / game.chunksize,
                    game.MapSizeY / game.chunksize)] != null)
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
                state.curspeed.X = MakeCloserToZero(state.curspeed.X, move.acceleration.acceleration2 * (float)dt);
                state.curspeed.Y = MakeCloserToZero(state.curspeed.Y, move.acceleration.acceleration2 * (float)dt);
                state.curspeed.Z = MakeCloserToZero(state.curspeed.Z, move.acceleration.acceleration2 * (float)dt);
                diff1.Y += move.moveup ? 2 * move.movespeednow * (float)dt : 0;
                diff1.Y -= move.movedown ? 2 * move.movespeednow * (float)dt : 0;
                state.curspeed.X += diff1.X * move.acceleration.acceleration3 * (float)dt;
                state.curspeed.Y += diff1.Y * move.acceleration.acceleration3 * (float)dt;
                state.curspeed.Z += diff1.Z * move.acceleration.acceleration3 * (float)dt;
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
                if (diff1.Length > 0)
                {
                    diff1.Normalize();
                }
                state.curspeed.X = diff1.X * move.movespeednow;
                state.curspeed.Y = diff1.Y * move.movespeednow;
                state.curspeed.Z = diff1.Z * move.movespeednow;
            }
            Vector3 newposition;
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
                newposition.X = state.playerposition.X + diffx * (float)dt;
                newposition.Y = state.playerposition.Y + diffy * (float)dt;
                newposition.Z = state.playerposition.Z + diffz * (float)dt;
            }
            else
            {
                newposition.X = state.playerposition.X + (state.curspeed.X) * (float)dt;
                newposition.Y = state.playerposition.Y + (state.curspeed.Y) * (float)dt;
                newposition.Z = state.playerposition.Z + (state.curspeed.Z) * (float)dt;
            }
            newposition.Y += state.movedz * (float)dt;
            Vector3 previousposition = game.ToVector3(state.playerposition);
            if (!move.ENABLE_NOCLIP)
            {
                this.swimmingtop = move.wantsjump && !move.Swimming;
                // This is a temporary workaround for crashing at the top of the map.
                // This needs to be cleaned up some other way.
                try
                {
                    Vector3 v = WallSlide(state, game.ToVector3(state.playerposition), newposition, modelheight);
                    state.playerposition.X = v.X;
                    state.playerposition.Y = v.Y;
                    state.playerposition.Z = v.Z;
                }
                catch
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

        float Length(float x, float y, float z)
        {
            return Platform.Sqrt(x * x + y * y + z * z);
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
        public Vector3 WallSlide(CharacterPhysicsState state, Vector3 oldposition, Vector3 newposition, float modelheight)
        {
            bool high = false;
            if (modelheight >= 2) { high = true; }
            oldposition.Y += walldistance;
            newposition.Y += walldistance;

            reachedceiling = false;
            reachedwall = false;
            //Math.Floor() is needed because casting negative values to integer is not floor.
            Vector3i oldpositioni = new Vector3i((int)Math.Floor(oldposition.X), (int)Math.Floor(oldposition.Z),
                (int)Math.Floor(oldposition.Y));
            Vector3 playerposition = newposition;
            //left
            {
                var qnewposition = newposition + new Vector3(0, 0, walldistance);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
                && (!high || IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2));
                if (newposition.Z - oldposition.Z > 0)
                {
                    if (!newempty)
                    {
                        reachedwall = true;
                        playerposition.Z = oldposition.Z;
                    }
                }
            }
            //front
            {
                var qnewposition = newposition + new Vector3(walldistance, 0, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
                && (!high || IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2));
                if (newposition.X - oldposition.X > 0)
                {
                    if (!newempty)
                    {
                        reachedwall = true;
                        playerposition.X = oldposition.X;
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
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
                && (!high || IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2));
                if (newposition.Z - oldposition.Z < 0)
                {
                    if (!newempty)
                    {
                        reachedwall = true;
                        playerposition.Z = oldposition.Z;
                    }
                }
            }
            //back
            {
                var qnewposition = newposition + new Vector3(-walldistance, 0, 0);
                bool newempty = IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y))
                && IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 1)
                && (!high || IsTileEmptyForPhysics((int)Math.Floor(qnewposition.X), (int)Math.Floor(qnewposition.Z), (int)Math.Floor(qnewposition.Y) + 2));
                if (newposition.X - oldposition.X < 0)
                {
                    if (!newempty)
                    {
                        reachedwall = true;
                        playerposition.X = oldposition.X;
                    }
                }
            }
            //bottom
            {
                var qnewposition = newposition + new Vector3(0, modelheight, 0);
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
        //ok:
            playerposition.Y -= walldistance;
            return playerposition;
        }
        public bool swimmingtop;
        public bool reachedceiling;
        public bool reachedwall;
    }
}
