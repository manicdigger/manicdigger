using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.ClientNative;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

public class ServerSystemLoadConfig : ServerSystem
{
    public override void Update(Server server, float dt)
    {
        if (!loaded)
        {
            loaded = true;
            LoadConfig(server);
            server.OnConfigLoaded();
        }
        if (server.configNeedsSaving)
        {
            server.configNeedsSaving = false;
            SaveConfig(server);
        }
    }
    bool loaded;
    public void LoadConfig(Server server)
    {
        string filename = "ServerConfig.txt";
        if (!File.Exists(Path.Combine(GameStorePath.gamepathconfig, filename)))
        {
            Console.WriteLine(server.language.ServerConfigNotFound());
            SaveConfig(server);
            return;
        }
        try
        {
            using (TextReader textReader = new StreamReader(Path.Combine(GameStorePath.gamepathconfig, filename)))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(ServerConfig));
                server.config = (ServerConfig)deserializer.Deserialize(textReader);
                textReader.Close();
            }
        }
        catch //This if for the original format
        {
            try
            {
                using (Stream s = new MemoryStream(File.ReadAllBytes(Path.Combine(GameStorePath.gamepathconfig, filename))))
                {
                    server.config = new ServerConfig();
                    StreamReader sr = new StreamReader(s);
                    XmlDocument d = new XmlDocument();
                    d.Load(sr);
                    server.config.Format = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Format"));
                    server.config.Name = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Name");
                    server.config.Motd = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Motd");
                    server.config.Port = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Port"));
                    string maxclients = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MaxClients");
                    if (maxclients != null)
                    {
                        server.config.MaxClients = int.Parse(maxclients);
                    }
                    string key = XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Key");
                    if (key != null)
                    {
                        server.config.Key = key;
                    }
                    server.config.IsCreative = Misc.ReadBool(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Creative"));
                    server.config.Public = Misc.ReadBool(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/Public"));
                    server.config.AllowGuests = Misc.ReadBool(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/AllowGuests"));
                    if (XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeX") != null)
                    {
                        server.config.MapSizeX = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeX"));
                        server.config.MapSizeY = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeY"));
                        server.config.MapSizeZ = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/MapSizeZ"));
                    }
                    server.config.BuildLogging = bool.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/BuildLogging"));
                    server.config.ServerEventLogging = bool.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/ServerEventLogging"));
                    server.config.ChatLogging = bool.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/ChatLogging"));
                    server.config.AllowScripting = bool.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/AllowScripting"));
                    server.config.ServerMonitor = bool.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/ServerMonitor"));
                    server.config.ClientConnectionTimeout = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/ClientConnectionTimeout"));
                    server.config.ClientPlayingTimeout = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerServerConfig/ClientPlayingTimeout"));
                }
                //Save with new version.
                SaveConfig(server);
            }
            catch
            {
                //ServerConfig is really messed up. Backup a copy, then create a new one.
                try
                {
                    File.Copy(Path.Combine(GameStorePath.gamepathconfig, filename), Path.Combine(GameStorePath.gamepathconfig, filename + ".old"));
                    Console.WriteLine(server.language.ServerConfigCorruptBackup());
                }
                catch
                {
                    Console.WriteLine(server.language.ServerConfigCorruptNoBackup());
                }
                server.config = null;
                SaveConfig(server);
            }
        }
        server.language.OverrideLanguage = server.config.ServerLanguage;  //Switch to user-defined language.
        Console.WriteLine(server.language.ServerConfigLoaded());
    }

    public void SaveConfig(Server server)
    {
        //Verify that we have a directory to place the file into.
        if (!Directory.Exists(GameStorePath.gamepathconfig))
        {
            Directory.CreateDirectory(GameStorePath.gamepathconfig);
        }

        XmlSerializer serializer = new XmlSerializer(typeof(ServerConfig));
        TextWriter textWriter = new StreamWriter(Path.Combine(GameStorePath.gamepathconfig, "ServerConfig.txt"));

        //Check to see if config has been initialized
        if (server.config == null)
        {
            server.config = new ServerConfig();
            //Set default language to user's locale
            server.config.ServerLanguage = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            //Ask for config parameters the first time the server is started
            string line;
            bool wantsconfig = false;
            Console.WriteLine(server.language.ServerSetupFirstStart());
            Console.WriteLine(server.language.ServerSetupQuestion());
            line = Console.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                if (line.Equals(server.language.ServerSetupAccept(), StringComparison.InvariantCultureIgnoreCase))
                    wantsconfig = true;
                else
                    wantsconfig = false;
            }
            //Only ask these questions if user wants to
            if (wantsconfig)
            {
                Console.WriteLine(server.language.ServerSetupPublic());
                line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    bool choice;
                    if (line.Equals(server.language.ServerSetupAccept(), StringComparison.InvariantCultureIgnoreCase))
                        choice = true;
                    else
                        choice = false;
                    server.config.Public = choice;
                }
                Console.WriteLine(server.language.ServerSetupName());
                line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    server.config.Name = line;
                }
                Console.WriteLine(server.language.ServerSetupMOTD());
                line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    server.config.Motd = line;
                }
                Console.WriteLine(server.language.ServerSetupWelcomeMessage());
                line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    server.config.WelcomeMessage = line;
                }
                Console.WriteLine(server.language.ServerSetupPort());
                line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    int port;
                    try
                    {
                        port = int.Parse(line);
                        if (port > 0 && port <= 65565)
                        {
                            server.config.Port = port;
                        }
                        else
                        {
                            Console.WriteLine(server.language.ServerSetupPortInvalidValue());
                        }
                    }
                    catch
                    {
                        Console.WriteLine(server.language.ServerSetupPortInvalidInput());
                    }
                }
                Console.WriteLine(server.language.ServerSetupMaxClients());
                line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    int players;
                    try
                    {
                        players = int.Parse(line);
                        if (players > 0)
                        {
                            server.config.MaxClients = players;
                        }
                        else
                        {
                            Console.WriteLine(server.language.ServerSetupMaxClientsInvalidValue());
                        }
                    }
                    catch
                    {
                        Console.WriteLine(server.language.ServerSetupMaxClientsInvalidInput());
                    }
                }
                Console.WriteLine(server.language.ServerSetupEnableHTTP());
                line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    bool choice;
                    if (line.Equals(server.language.ServerSetupAccept(), StringComparison.InvariantCultureIgnoreCase))
                        choice = true;
                    else
                        choice = false;
                    server.config.EnableHTTPServer = choice;
                }
            }
        }
        if (server.config.Areas.Count == 0)
        {
            server.config.Areas = ServerConfigMisc.getDefaultAreas();
        }
        //Serialize the ServerConfig class to XML
        serializer.Serialize(textWriter, server.config);
        textWriter.Close();
    }
}
