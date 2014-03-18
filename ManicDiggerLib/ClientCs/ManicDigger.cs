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
    public interface IClients
    {
        Player[] Players { get; set; }
    }
}
