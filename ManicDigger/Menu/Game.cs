using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using ManicDigger;

namespace GameMenu
{
    public class ServerInfo
    {
        public string Hash;
        public string Name;
        public string Motd;
        public int Port;
        public string Ip;
        public string Version;
        public int Users;
        public int Max;
        public string GameMode;
        public string Players;
    }
    public interface Game
    {
        string[] GetWorlds();
        bool IsLoggedIn { get; set; }
        string LoginName { get; set; }
        void SetWorldOptions(int worldId, string name);
        void JoinMultiplayer(string hash);
        void JoinMultiplayer(string host, int port);
        ServerInfo[] GetServers();
        void LoginGuest(string p);
        bool LoginAccount(string user, string password);
        void StartSinglePlayer(int id);
        void DeleteWorld(int worldId);
        void StartAndJoinLocalServer(int worldId);
    }
}
