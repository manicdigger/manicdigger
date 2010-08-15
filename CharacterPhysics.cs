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
    }
    public class CharacterPhysics
    {
        [Inject]
        public IMapStorage map { get; set; }
        [Inject]
        public IGameData data { get; set; }
        void Update()
        {
        }
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
        public static float walldistance = 0.3f;
        public static float characterheight = 1.5f;
        public void Move(CharacterPhysicsState state, double dt)
        {
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
                    }
                    else
                    {
                        if (!newempty)
                        {
                            playerposition.Y += 0.5f;
                            goto ok;
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
                        if (!newempty)
                        {
                            playerposition.Y += 0.5f;
                            goto ok;
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
                        if (!newempty)
                        {
                            playerposition.Y += 0.5f;
                            goto ok;
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
                        if (!newempty)
                        {
                            playerposition.Y += 0.5f;
                            goto ok;
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
            if (MapUtil.IsValidPos(map, playerpositioni.x, playerpositioni.y, playerpositioni.z))
            {
                isonstairs = map.GetBlock(playerpositioni.x, playerpositioni.y, playerpositioni.z) == data.TileIdSingleStairs;
            }
            if (isonstairs)
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
