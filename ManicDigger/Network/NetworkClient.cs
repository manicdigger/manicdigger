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
    public interface INetworkClient
    {
        void Dispose();
        void Connect(string serverAddress, int port, string username, string auth);
        void Process();
        void SendSetBlock(Vector3 position, BlockSetMode mode, int type);
        event EventHandler<MapLoadingProgressEventArgs> MapLoadingProgress;
        event EventHandler<MapLoadedEventArgs> MapLoaded;
        void SendChat(string s);
        IEnumerable<string> ConnectedPlayers();
        void SendPosition(Vector3 position, Vector3 orientation);
        Dictionary<int, bool> EnablePlayerUpdatePosition { get; set; }
        string ServerName { get; }
        string ServerMotd { get; }
    }
    public class MapLoadingProgressEventArgs : EventArgs
    {
        public int ProgressPercent { get; set; }
        public int ProgressBytes { get; set; }
    }
    public class NetworkClientDummy : INetworkClient
    {
        [Inject]
        public ILocalPlayerPosition player { get; set; }
        public event EventHandler<MapLoadedEventArgs> MapLoaded;
        [Inject]
        public IGui Gui { get; set; }
        [Inject]
        public IMap Map1 { get; set; }
        [Inject]
        public IMapStorage Map { get; set; }
        [Inject]
        public IGameData Data { get; set; }
        [Inject]
        public fCraft.MapGenerator Gen { get; set; }
        public string DEFAULTMAP = "flatgrass";
        public void Dispose()
        {
        }
        public void Connect(string serverAddress, int port, string username, string auth)
        {
            SendChat("/generate " + DEFAULTMAP);
        }
        public void Process()
        {
        }
        public void SendSetBlock(Vector3 position, BlockSetMode mode, int type)
        {
            if (mode == BlockSetMode.Destroy)
            {
                type = Data.TileIdEmpty;
            }
            //block update not needed - client does speculative block update.
            //Map1.SetTileAndUpdate(position, (byte)type);
            Console.WriteLine("player:" + player.LocalPlayerPosition + ", build:" + position);
        }
        public void SendChat(string s)
        {
            if (s == "")
            {
                return;
            }
            string[] ss = s.Split(new char[] { ' ' });
            if (s.StartsWith("/"))
            {
                string cmd = ss[0].Substring(1);
                string arguments;
                if (s.IndexOf(" ") == -1)
                { arguments = ""; }
                else
                { arguments = s.Substring(s.IndexOf(" ")); }
                arguments = arguments.Trim();
                if (cmd == "generate")
                {
                    DoGenerate(arguments, false);
                    Gui.DrawMap();
                }
            }
            Gui.AddChatline(s);
        }
        void DoGenerate(string mode, bool hollow)
        {
            switch (mode)
            {
                case "flatgrass":
                    bool reportedProgress = false;
                    playerMessage("Generating flatgrass map...");
                    for (int i = 0; i < Map.MapSizeX; i++)
                    {
                        for (int j = 0; j < Map.MapSizeY; j++)
                        {
                            for (int k = 1; k < Map.MapSizeZ / 2 - 1; k++)
                            {
                                if (!hollow) Map.SetBlock(i, j, k, Data.TileIdDirt);
                            }
                            Map.SetBlock(i, j, Map.MapSizeZ / 2 - 1, Data.TileIdGrass);
                            for (int k = Map.MapSizeZ / 2; k < Map.MapSizeZ; k++)
                            {
                                if (!hollow) Map.SetBlock(i, j, k, Data.TileIdEmpty);
                            }
                        }
                        if (i > Map.MapSizeX / 2 && !reportedProgress)
                        {
                            reportedProgress = true;
                            playerMessage("Map generation: 50%");
                        }
                    }

                    //map.MakeFloodBarrier();

                    //if (map.Save(filename))
                    //{
                    //    player.Message("Map generation: Done.");
                    //}
                    //else
                    //{
                    //    player.Message(Color.Red, "An error occured while generating the map.");
                    //}
                    break;

                case "empty":
                    playerMessage("Generating empty map...");
                    //map.MakeFloodBarrier();

                    //if (map.Save(filename))
                    //{
                    //    player.Message("Map generation: Done.");
                    //}
                    //else
                    //{
                    //    player.Message(Color.Red, "An error occured while generating the map.");
                    //}

                    break;

                case "hills":
                    playerMessage("Generating terrain...");
                    Gen.GenerateMap(new fCraft.MapGeneratorParameters(
                                                                              5, 1, 0.5, 0.45, 0, 0.5, hollow));
                    break;

                case "mountains":
                    playerMessage("Generating terrain...");
                    Gen.GenerateMap(new fCraft.MapGeneratorParameters(
                                                                              8, 1, 0.5, 0.45, 0.1, 0.5, hollow));
                    break;

                case "lake":
                    playerMessage("Generating terrain...");
                    Gen.GenerateMap(new fCraft.MapGeneratorParameters(
                                                                              1, 0.6, 0.9, 0.45, -0.35, 0.55, hollow));
                    break;

                case "island":
                    playerMessage("Generating terrain...");
                    Gen.GenerateMap(new fCraft.MapGeneratorParameters(1, 0.6, 1, 0.45, 0.3, 0.35, hollow));
                    break;

                default:
                    playerMessage("Unknown map generation mode: " + mode);
                    break;
            }
        }
        private void playerMessage(string p)
        {
            Gui.AddChatline(p);
        }
        public IEnumerable<string> ConnectedPlayers()
        {
            yield return "[local player]";
        }
        #region IClientNetwork Members
        public void SendPosition(Vector3 position, Vector3 orientation)
        {
        }
        #endregion
        #region IClientNetwork Members
        public event EventHandler<MapLoadingProgressEventArgs> MapLoadingProgress;
        #endregion
        Dictionary<int, bool> enablePlayerUpdatePosition = new Dictionary<int, bool>();
        #region INetworkClient Members
        public Dictionary<int, bool> EnablePlayerUpdatePosition { get { return enablePlayerUpdatePosition; } set { enablePlayerUpdatePosition = value; } }
        #endregion
        #region INetworkClient Members
        public string ServerName
        {
            get { return "ServerName"; }
        }
        public string ServerMotd
        {
            get { return "ServerMotd"; }
        }
        #endregion
    }
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
