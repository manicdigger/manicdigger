using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace GameModeFortress
{
    public interface IServerHeartbeat
    {
        string Name { get; set; }
        string Key { get; set; }

        int MaxClients { get; set; }
        bool Public { get; set; }
        bool PasswordProtected {get;set; }
        bool AllowGuests {get; set; }
        int Port { get; set; }
        string Version { get; set; }
        List<string> Players { get; set; }
        int UsersCount { get; set; }
        string Motd { get; set; }
        string GameMode { get; set; }

        string ReceivedKey { get; set; }

        void SendHeartbeat();
    }
    public class ServerHeartbeat : IServerHeartbeat
    {
        public ServerHeartbeat()
        {
            this.Name = "";
            this.Key = Guid.NewGuid().ToString();
            this.MaxClients = 16;
            this.Public = true;
            this.AllowGuests = true;
            this.Port = 25565;
            this.Version = "Unknown";
            this.Players = new List<string>();
            this.UsersCount = 0;
            this.Motd = "";
            this.GameMode = "Fortress";
        }
        public string fListUrl = null;

        public string Name { get; set; }
        public string Key { get; set; }
        
        public int MaxClients {get;set;}
        public bool Public {get;set;}
        public bool PasswordProtected {get;set; }
        public bool AllowGuests {get; set; }
        public int Port { get; set; }
        public string Version { get; set; }
        public List<string> Players { get; set; }
        public int UsersCount { get; set; }
        public string Motd { get; set; }
        public string GameMode { get; set; }

        public string ReceivedKey { get; set; }
        public void SendHeartbeat()
        {
            if (fListUrl == null)
            {
                WebClient c = new WebClient();
                fListUrl = c.DownloadString("http://manicdigger.sourceforge.net/heartbeat.txt");
            }
            StringWriter sw = new StringWriter();//&salt={4}
            string staticData = String.Format("name={0}&max={1}&public={2}&passwordProtected={3}&allowGuests={4}&port={5}&version={6}&fingerprint={7}"
                , System.Web.HttpUtility.UrlEncode(Name),
                MaxClients, Public, PasswordProtected, AllowGuests, Port, Version, Key.Replace("-", ""));

            string requestString = staticData +
                                    "&users=" + UsersCount +
                                    "&motd=" + System.Web.HttpUtility.UrlEncode(Motd) +
                                    "&gamemode=" + GameMode +
                                    "&players=" + string.Join(",", Players.ToArray());

            var request = (HttpWebRequest)WebRequest.Create(fListUrl);
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
            ReceivedKey = null;
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                ReceivedKey = sr.ReadToEnd();
            }

            request.Abort();
        }
    }
}
