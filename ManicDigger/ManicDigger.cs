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
using ManicDigger.Collisions;
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
    public interface IAddChatLine
    {
        void AddChatline(string s);
    }
    public class AddChatLineDummy : ManicDigger.IAddChatLine
    {
        #region IGui Members
        public void AddChatline(string s)
        {
        }
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
        void GetMapPortion(byte[] outPortion, int x, int y, int z, int portionsizex, int portionsizey, int portionsizez);
        void SetMapPortion(int x, int y, int z, byte[, ,] chunk);
        void UseMap(byte[, ,] map);
    }
    public interface IMapStorageLight
    {
        int GetLight(int x, int y, int z);
    }
    public interface IWaterLevel
    {
        float WaterLevel { get; set; }
    }
    public class Player
    {
        public Vector3 Position;
        public byte Heading;
        public byte Pitch;
        public string Name;
        public AnimationHint AnimationHint = new AnimationHint();
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
        public static byte[] ToFlatMap(byte[, ,] map)
        {
            int sizex = map.GetUpperBound(0) + 1;
            int sizey = map.GetUpperBound(1) + 1;
            int sizez = map.GetUpperBound(2) + 1;
            byte[] flatmap = new byte[sizex * sizey * sizez];
            for (int x = 0; x < sizex; x++)
            {
                for (int y = 0; y < sizey; y++)
                {
                    for (int z = 0; z < sizez; z++)
                    {
                        flatmap[Index3d(x, y, z, sizex, sizey)] = map[x, y, z];
                    }
                }
            }
            return flatmap;
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
        public static bool IsSolidChunk(byte[] chunk)
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
        public static bool IsSolidChunk(byte[, ,] chunk)
        {
            for (int x = 0; x <= chunk.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= chunk.GetUpperBound(1); y++)
                {
                    for (int z = 0; z <= chunk.GetUpperBound(2); z++)
                    {
                        if (chunk[x, y, z] != chunk[0, 0, 0])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
    public class MapStorage : IMapStorage, IMapStoragePortion
    {
        byte[, ,] map;
        public object mapupdate = new object();
        public byte[, ,] Map { get { return map; } set { map = value; } }
        public int MapSizeX { get; set; }
        public int MapSizeY { get; set; }
        public int MapSizeZ { get; set; }
        #region IMapStorage Members
        public void SetBlock(int x, int y, int z, int tileType)
        {
            map[x, y, z] = (byte)tileType;
        }
        #endregion
        #region IMapStorage Members
        public int GetBlock(int x, int y, int z)
        {
            return map[x, y, z];
        }
        #endregion
        #region IMapStorage Members
        public void UseMap(byte[, ,] map)
        {
            this.map = map;
        }
        #endregion
        #region IMapStorage Members
        public void SetMapPortion(int x, int y, int z, byte[, ,] chunk)
        {
            throw new NotImplementedException();
        }
        #endregion
        public void Reset(int sizex, int sizey, int sizez)
        {
        }
        public void GetMapPortion(byte[] outPortion, int x, int y, int z, int portionsizex, int portionsizey, int portionsizez)
        {
            throw new NotImplementedException();
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
        public const string XmlSaveExtension = ".mdxs.gz";
        public const string BinSaveExtension = ".mddbs";
        public const string MinecraftMapSaveExtension = ".dat";
    }
    public interface IInternetGameFactory
    {
        void NewInternetGame();
    }
    public class InternetGameFactoryDummy : IInternetGameFactory
    {
        #region IInternetGameFactory Members
        public void NewInternetGame()
        {
        }
        #endregion
    }
    public class PlayMp3 : IDisposable
    {
        private string _command;
        private bool isOpen;
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);
        bool closed = false;
        public void Close()
        {
            try
            {
                _command = "close MediaFile";
                mciSendString(_command, null, 0, IntPtr.Zero);
                isOpen = false;
            }
            catch
            {
                Console.WriteLine("winmm.dll problem");
            }
        }
        public void Open(string sFileName)
        {
            if (!File.Exists(sFileName))
            {
                Console.WriteLine("Music file not found: " + sFileName);
            }
            try
            {
                _command = "open \"" + sFileName + "\" type mpegvideo alias MediaFile";
                mciSendString(_command, null, 0, IntPtr.Zero);
                isOpen = true;
            }
            catch
            {
                Console.WriteLine("winmm.dll problem");
            }
        }
        public void Play(bool loop)
        {
            try
            {
                if (isOpen)
                {
                    _command = "play MediaFile";
                    if (loop)
                        _command += " REPEAT";
                    mciSendString(_command, null, 0, IntPtr.Zero);
                }
            }
            catch
            {
                Console.WriteLine("winmm.dll problem");
            }
        }
        #region IDisposable Members
        public void Dispose()
        {
            if (!closed)
            {
                Close();
                closed = true;
            }
        }
        #endregion
    }
    public class Viewport
    {
    }
    public interface IMap
    {
        //void LoadMap(byte[, ,] map);
        IMapStorage Map { get; }
        void SetTileAndUpdate(Vector3 pos, int type);
        void UpdateAllTiles();
    }
    public class MapDummy : ManicDigger.IMap
    {
        #region IMap Members
        public void SetTileAndUpdate(OpenTK.Vector3 pos, int type)
        {
        }
        #endregion
        IMapStorage map = new MapStorage();
        #region IMap Members
        public IMapStorage Map { get { return map; } }
        #endregion
        #region IMap Members
        public void UpdateAllTiles()
        {
        }
        #endregion
    }
    public interface ILocalPlayerPosition
    {
        Vector3 LocalPlayerPosition { get; set; }
        Vector3 LocalPlayerOrientation { get; set; }
        bool Swimming { get; }
        float CharacterHeight { get; set; }
    }
    public class LocalPlayerPositionDummy : ILocalPlayerPosition
    {
        #region ILocalPlayerPosition Members
        public OpenTK.Vector3 LocalPlayerOrientation { get; set; }
        public OpenTK.Vector3 LocalPlayerPosition { get; set; }
        public bool Swimming { get { return false; } }
        #endregion
        #region ILocalPlayerPosition Members
        public float CharacterHeight { get; set; }
        #endregion
    }
    public interface IClients
    {
        IDictionary<int, Player> Players { get; set; }
    }
    public class PlayersDummy : IClients
    {
        IDictionary<int, Player> players = new Dictionary<int, Player>();
        #region IPlayers Members
        public IDictionary<int, Player> Players { get { return players; } set { players = value; } }
        #endregion
    }
    public enum BlockSetMode
    {
        Destroy,
        Create,
    }
    public interface ILogger
    {
        void LogPerformance(string key, string value);
    }
}