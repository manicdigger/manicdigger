#region --- Using directives ---

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Media;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
using System.Diagnostics;
using ManicDigger;
using System.Net;
using System.Windows.Forms;
using System.Xml.XPath;
using System.Xml;

#endregion

namespace ManicDigger
{
    public interface IGameExit
    {
        bool exit { get; set; }
    }
    public class GameExitDummy : IGameExit
    {
        #region IGameExit Members
        public bool exit { get; set; }
        #endregion
    }
    public interface IMapStorage
    {
        int MapSizeX { get; set; }
        int MapSizeY { get; set; }
        int MapSizeZ { get; set; }
        int GetBlock(int x, int y, int z);
        void SetBlock(int x, int y, int z, int tileType);
    }
    public interface IMapStoragePortion
    {
        void GetMapPortion(int[] outPortion, int x, int y, int z, int portionsizex, int portionsizey, int portionsizez);
        void SetMapPortion(int x, int y, int z, int[, ,] chunk);
        void UseMap(byte[, ,] map);
    }
    public interface IMapStorageLight
    {
        int GetLight(int x, int y, int z);
    }
    public enum PlayerType
    {
        Player,
        Monster,
    }
    public class Player
    {
        public Vector3? Position;
        public byte Heading;
        public byte Pitch;
        public string Name;
        public AnimationHint AnimationHint = new AnimationHint();
        public PlayerType Type;
        public int MonsterType;
        public int Health;
        public int LastUpdateMilliseconds;
        public string Model = "player.txt";
        public string Texture;
        public float EyeHeight = 1.5f;
        public float ModelHeight = 1.7f;
    }
    public static class MapUtil
    {
        public static int Index2d(int x, int y, int sizex)
        {
            return x + y * sizex;
        }

        public static int Index3d(int x, int y, int h, int sizex, int sizey)
        {
            return (h * sizey + y) * sizex + x;
        }

        public static Vector3i Pos(int index, int sizex, int sizey)
        {
            int x = index % sizex;
            int y = (index / sizex) % sizey;
            int h = index / (sizex * sizey);
            return new Vector3i(x, y, h);
        }

        public static bool IsValidPos(IMapStorage map, int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0)
            {
                return false;
            }
            if (x >= map.MapSizeX || y >= map.MapSizeY || z >= map.MapSizeZ)
            {
                return false;
            }
            return true;
        }

        public static bool IsValidPos(IMapStorage map, int x, int y)
        {
            if (x < 0 || y < 0)
            {
                return false;
            }
            if (x >= map.MapSizeX || y >= map.MapSizeY)
            {
                return false;
            }
            return true;
        }

        public static bool IsValidChunkPos(IMapStorage map, int cx, int cy, int cz, int chunksize)
        {
            return cx >= 0 && cy >= 0 && cz >= 0
                && cx < map.MapSizeX / chunksize
                && cy < map.MapSizeY / chunksize
                && cz < map.MapSizeZ / chunksize;
        }

        public static int blockheight(IMapStorage map, int tileidempty, int x, int y)
        {
            for (int z = map.MapSizeZ - 1; z >= 0; z--)
            {
                if (map.GetBlock(x, y, z) != tileidempty)
                {
                    return z + 1;
                }
            }
            return map.MapSizeZ / 2;
        }

        static ulong pow20minus1 = 1048576 - 1;
        public static Vector3i FromMapPos(ulong v)
        {
            uint z = (uint)(v & pow20minus1);
            v = v >> 20;
            uint y = (uint)(v & pow20minus1);
            v = v >> 20;
            uint x = (uint)(v & pow20minus1);
            return new Vector3i((int)x, (int)y, (int)z);
        }

        public static ulong ToMapPos(int x, int y, int z)
        {
            ulong v = 0;
            v = (ulong)x << 40;
            v |= (ulong)y << 20;
            v |= (ulong)z;
            return v;
        }

        public static int SearchColumn(IMapStorage map, int x, int y, int id, int startH)
        {
            for (int h = startH; h > 0; h--)
            {
                if (map.GetBlock(x, y, h) == (byte)id)
                {
                    return h;
                }
            }
            return -1; // -1 means 'not found'
        }

        public static int SearchColumn(IMapStorage map, int x, int y, int id)
        {
            return SearchColumn(map, x, y, id, map.MapSizeZ - 1);
        }

        public static bool IsSolidChunk(ushort[] chunk)
        {
            for (int i = 0; i <= chunk.GetUpperBound(0); i++)
            {
                if (chunk[i] != chunk[0])
                {
                    return false;
                }
            }
            return true;
        }

        public static Point PlayerArea(int playerAreaSize, int centerAreaSize, Vector3i blockPosition)
        {
            Point p = PlayerCenterArea(playerAreaSize, centerAreaSize, blockPosition);
            int x = p.X + centerAreaSize / 2;
            int y = p.Y + centerAreaSize / 2;
            x -= playerAreaSize / 2;
            y -= playerAreaSize / 2;
            return new Point(x, y);
        }

        public static Point PlayerCenterArea(int playerAreaSize, int centerAreaSize, Vector3i blockPosition)
        {
            int px = blockPosition.x;
            int py = blockPosition.y;
            int gridposx = (px / centerAreaSize) * centerAreaSize;
            int gridposy = (py / centerAreaSize) * centerAreaSize;
            return new Point(gridposx, gridposy);
        }
    }
    public class XmlTool
    {
        public static string XmlVal(XmlDocument d, string path)
        {
            XPathNavigator navigator = d.CreateNavigator();
            XPathNodeIterator iterator = navigator.Select(path);
            foreach (XPathNavigator n in iterator)
            {
                return n.Value;
            }
            return null;
        }
        public static IEnumerable<string> XmlVals(XmlDocument d, string path)
        {
            XPathNavigator navigator = d.CreateNavigator();
            XPathNodeIterator iterator = navigator.Select(path);
            foreach (XPathNavigator n in iterator)
            {
                yield return n.Value;
            }
        }
        public static string X(string name, string value)
        {
            return string.Format("<{0}>{1}</{0}>", name, value);
        }
    }
    public class MapManipulator
    {
        public const string BinSaveExtension = ".mddbs";
    }
    public interface ILocalPlayerPosition
    {
        Vector3 LocalPlayerPosition { get; set; }
        Vector3 LocalPlayerOrientation { get; set; }
        bool Swimming { get; }
        float CharacterEyesHeight { get; set; }
    }
    public interface IClients
    {
        IDictionary<int, Player> Players { get; set; }
    }
}
