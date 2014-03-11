using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace ManicDigger
{
    public class NetworkHelper
    {
        public static byte HeadingByte(Vector3 orientation)
        {
            return (byte)((((orientation.Y) % (2 * Math.PI)) / (2 * Math.PI)) * 256);
        }
        public static byte PitchByte(Vector3 orientation)
        {
            double xx = (orientation.X + Math.PI) % (2 * Math.PI);
            xx = xx / (2 * Math.PI);
            return (byte)(xx * 256);
        }
    }
}
