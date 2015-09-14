using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

public class ServerSystemHeartbeat : ServerSystem
{
    public ServerSystemHeartbeat()
    {
        d_Heartbeat = new ServerHeartbeat();
        elapsed = 60;
    }
    float elapsed;
    public override void Update(Server server, float dt)
    {
        elapsed += dt;
        while (elapsed >= 60)
        {
            elapsed -= 60;
            if ((server.Public) && (server.config.Public))
            {
                d_Heartbeat.GameMode = server.gameMode;
                server.serverPlatform.QueueUserWorkItem(ActionSendHeartbeat.Create(this, server));
            }
        }
    }
    ServerHeartbeat d_Heartbeat;

    public void SendHeartbeat(Server server)
    {
        if (server.config == null)
        {
            return;
        }
        if (server.config.Key == null)
        {
            return;
        }
        d_Heartbeat.Name = server.config.Name;
        d_Heartbeat.MaxClients = server.config.MaxClients;
        d_Heartbeat.PasswordProtected = server.config.IsPasswordProtected();
        d_Heartbeat.AllowGuests = server.config.AllowGuests;
        d_Heartbeat.Port = server.config.Port;
        d_Heartbeat.Version = ManicDigger.ClientNative.GameVersion.Version;
        d_Heartbeat.Key = server.config.Key;
        d_Heartbeat.Motd = server.config.Motd;
        List<string> playernames = new List<string>();
        lock (server.clients)
        {
            foreach (var k in server.clients)
            {
                if (k.Value.IsBot)
                {
                    //Exclude bot players from appearing on server list
                    continue;
                }
                playernames.Add(k.Value.playername);
            }
        }
        d_Heartbeat.Players = playernames;
        d_Heartbeat.UsersCount = playernames.Count;
        try
        {
            d_Heartbeat.SendHeartbeat();
            server.ReceivedKey = d_Heartbeat.ReceivedKey;
            if (!writtenServerKey)
            {
                Console.WriteLine("hash: " + GetHash(d_Heartbeat.ReceivedKey));
                writtenServerKey = true;
            }
            Console.WriteLine(server.language.ServerHeartbeatSent());
        }
        catch (Exception e)
        {
            #if DEBUG
                // Only display full error message when running in Debug mode
                Console.WriteLine(e.ToString());
            #endif
            // Short error output when running normally
            Console.WriteLine("{0} ({1})", server.language.ServerHeartbeatError(), e.Message);
        }
    }
    bool writtenServerKey = false;
    public string hashPrefix = "server=";
    string GetHash(string hash)
    {
        try
        {
            if (hash.Contains(hashPrefix))
            {
                hash = hash.Substring(hash.IndexOf(hashPrefix) + hashPrefix.Length);
            }
        }
        catch
        {
            return "";
        }
        return hash;
    }
}

public class ActionSendHeartbeat : Action_
{
    public static ActionSendHeartbeat Create(ServerSystemHeartbeat s_, Server server_)
    {
        ActionSendHeartbeat a = new ActionSendHeartbeat();
        a.s = s_;
        a.server = server_;
        return a;
    }
    ServerSystemHeartbeat s;
    Server server;
    public override void Run()
    {
        s.SendHeartbeat(server);
    }
}

public class ServerHeartbeat
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
    }
    public string fListUrl = null;

    public string Name { get; set; }
    public string Key { get; set; }

    public int MaxClients { get; set; }
    public bool Public { get; set; }
    public bool PasswordProtected { get; set; }
    public bool AllowGuests { get; set; }
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
                                "&gamemode=" + System.Web.HttpUtility.UrlEncode(GameMode)  +
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
