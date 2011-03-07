using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;

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
        void SetWorldOptions(int worldId, string p);
        void JoinMultiplayer(string p, int p_2);
        ServerInfo[] GetServers();
        void LoginGuest(string p);
        bool LoginAccount(string p, string p_2);
        void StartSinglePlayer(int id);
    }
    public class MdLoginData
    {
        public string AuthKey;
        public bool PasswordCorrect;
        public bool ServerCorrect;
    }
    public class MdLogin
    {
        public string LoginUrl = "http://fragmer.net/md/login.php";
        public MdLoginData Login(string username, string password, string publicServerKey)
        {
            StringWriter sw = new StringWriter();//&salt={4}
            string requestString = String.Format("username={0}&password={1}&server={2}"
                , username, password, publicServerKey);

            var request = (HttpWebRequest)WebRequest.Create(LoginUrl);
            request.Method = "POST";
            request.Timeout = 15000; // 15s timeout
            request.ContentType = "application/x-www-form-urlencoded";
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

            byte[] formData = Encoding.ASCII.GetBytes(requestString);
            request.ContentLength = formData.Length;

            System.Net.ServicePointManager.Expect100Continue = false; // fixes lighthttpd 417 error

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Flush();
            }

            WebResponse response = request.GetResponse();

            string key = null;
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                key = sr.ReadToEnd();
            }

            request.Abort();
            MdLoginData data = new MdLoginData();
            data.PasswordCorrect = !(key.Contains("Wrong username") || key.Contains("Incorrect username"));
            data.ServerCorrect = !key.Contains("server");
            data.AuthKey = key;
            return data;
        }
    }
}
