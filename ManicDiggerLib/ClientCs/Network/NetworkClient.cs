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
    public class MapLoadedEventArgs : EventArgs
    {
        public byte[, ,] map;
    }
    public class NetworkHelper
    {
        public static byte[] StringToBytes(string s)
        {
            byte[] b = Encoding.ASCII.GetBytes(s);
            byte[] bb = new byte[64];
            for (int i = 0; i < bb.Length; i++)
            {
                bb[i] = 32; //' '
            }
            for (int i = 0; i < b.Length; i++)
            {
                bb[i] = b[i];
            }
            return bb;
        }
        private static string BytesToString(byte[] s)
        {
            string b = Encoding.ASCII.GetString(s).Trim();
            return b;
        }
        public static int ReadInt32(BinaryReader br)
        {
            byte[] array = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }
            return BitConverter.ToInt32(array, 0);
        }
        public static int ReadInt16(BinaryReader br)
        {
            byte[] array = br.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }
            return BitConverter.ToInt16(array, 0);
        }
        public static void WriteInt16(BinaryWriter bw, short v)
        {
            byte[] array = BitConverter.GetBytes((short)v);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }
            bw.Write(array);
        }
        public static void WriteInt32(BinaryWriter bw, int v)
        {
            byte[] array = BitConverter.GetBytes((int)v);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }
            bw.Write(array);
        }
        public static string ReadString64(BinaryReader br)
        {
            return BytesToString(br.ReadBytes(64));
        }
        public static void WriteString64(BinaryWriter bw, string s)
        {
            bw.Write(StringToBytes(s));
        }
        public static int StringLength = 64;
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
    public interface IGameWorldTodo
    {
        void KeyFrame(int allowedframe, int hash, Dictionary<int, PlayerPosition> playerpositions);
        void EnqueueCommand(int playerid, int frame, byte[] cmd);
        void LoadState(byte[] savegame, int simulationstartframe);
    }
    public class GameWorldTodoDummy : IGameWorldTodo
    {
        #region IGameWorldTodo Members
        public void KeyFrame(int allowedframe, int hash, Dictionary<int, PlayerPosition> playerpositions)
        {
        }
        public void EnqueueCommand(int playerid, int frame, byte[] cmd)
        {
        }
        public void LoadState(byte[] savegame, int simulationstartframe)
        {
        }
        #endregion
    }
    public class PlayerPosition
    {
        public Vector3 position;
        public byte heading;
        public byte pitch;
    }
}
