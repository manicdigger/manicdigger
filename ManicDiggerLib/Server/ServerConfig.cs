using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;


[XmlRoot(ElementName = "ManicDiggerServerConfig")]
public class ServerConfig
{
    public int Format { get; set; }             //XML Format Version Number
    public string Name { get; set; }
    public string Motd { get; set; }            //Message of the day
    public string WelcomeMessage { get; set; }  //Displays when the user logs in.
    public int Port { get; set; }               //Port the server runs on
    public int MaxClients { get; set; }
    public int AutoRestartCycle { get; set; }
    public bool ServerMonitor { get; set; }
    public int ClientConnectionTimeout { get; set; }
    public int ClientPlayingTimeout { get; set; }
    public bool BuildLogging { get; set; }
    public bool ServerEventLogging { get; set; }
    public bool ChatLogging { get; set; }
    public bool AllowScripting { get; set; }
    public string Key { get; set; }             //GUID to uniquely identify the server
    [XmlElement(ElementName = "Creative")]
    public bool IsCreative { get; set; }        //Is this a free build server?
    public bool Public { get; set; }            //Advertise this server?
    [XmlElement(IsNullable = true)]
    public string Password { get; set; }
    public bool AllowGuests { get; set; }
    public bool Monsters { get; set; }
    public int MapSizeX { get; set; }
    public int MapSizeY { get; set; }
    public int MapSizeZ { get; set; }
    [XmlArrayItem(ElementName = "Area")]
    public List<AreaConfig> Areas { get; set; }
    public int Seed { get; set; }
    public bool RandomSeed { get; set; }
    public bool EnableHTTPServer { get; set; }
    public bool AllowSpectatorUse { get; set; }
    public bool AllowSpectatorBuild { get; set; }
    public string ServerLanguage { get; set; }
    public int PlayerDrawDistance { get; set; }
    public bool EnablePlayerPushing { get; set; }

    public bool IsPasswordProtected()
    {
        return !string.IsNullOrEmpty(this.Password);
    }

    public bool CanUserBuild(ClientOnServer client, int x, int y, int z)
    {
        bool canBuild = false;
        // TODO: fast tree datastructure
        foreach (AreaConfig area in this.Areas)
        {
            if (area.IsInCoords(x, y, z))
            {
                if (area.CanUserBuild(client))
                {
                    return true;
                }
            }
        }
        return canBuild;
    }

    public ServerConfig()
    {
        //Set Defaults
        this.Format = 1;
        this.Name = "Manic Digger server";
        this.Motd = "MOTD";
        this.WelcomeMessage = "Welcome to my Manic Digger server!";
        this.Port = 25565;
        this.MaxClients = 16;
        this.ServerMonitor = true;
        this.ClientConnectionTimeout = 600;
        this.ClientPlayingTimeout = 60;
        this.BuildLogging = false;
        this.ServerEventLogging = false;
        this.ChatLogging = false;
        this.AllowScripting = false;
        this.Key = Guid.NewGuid().ToString();
        this.IsCreative = true;
        this.Public = true;
        this.AllowGuests = true;
        this.Monsters = false;
        this.MapSizeX = 9984;
        this.MapSizeY = 9984;
        this.MapSizeZ = 128;
        this.Areas = new List<AreaConfig>();
        this.AutoRestartCycle = 6;
        this.Seed = 0;
        this.RandomSeed = true;
        this.EnableHTTPServer = false;
        this.AllowSpectatorUse = false;
        this.AllowSpectatorBuild = false;
        this.ServerLanguage = "en";
        this.PlayerDrawDistance = 128;
        this.EnablePlayerPushing = true;
    }
}

public class AreaConfig
{
    public int Id { get; set; }
    private int x1;
    private int x2;
    private int y1;
    private int y2;
    private int z1;
    private int z2;
    private string coords;
    [XmlArrayItem(ElementName = "Group")]
    public List<string> PermittedGroups { get; set; }
    [XmlArrayItem(ElementName = "User")]
    public List<string> PermittedUsers { get; set; }
    [XmlElement(IsNullable = true)]
    public int? Level { get; set; }

    public AreaConfig()
    {
        this.Id = -1;
        this.Coords = "0,0,0,0,0,0";
        this.PermittedGroups = new List<string>();
        this.PermittedUsers = new List<string>();
    }
    public string Coords
    {
        get { return this.coords; }
        set
        {
            this.coords = value;
            string[] myCoords = this.Coords.Split(new char[] { ',' });
            x1 = Convert.ToInt32(myCoords[0]);
            x2 = Convert.ToInt32(myCoords[3]);
            y1 = Convert.ToInt32(myCoords[1]);
            y2 = Convert.ToInt32(myCoords[4]);
            z1 = Convert.ToInt32(myCoords[2]);
            z2 = Convert.ToInt32(myCoords[5]);
        }
    }
    public bool IsInCoords(int x, int y, int z)
    {
        if (x >= x1 && x <= x2 && y >= y1 && y <= y2 && z >= z1 && z <= z2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CanUserBuild(ClientOnServer client)
    {
        if (this.Level != null)
        {
            if (client.clientGroup.Level >= this.Level)
            {
                return true;
            }
        }
        foreach (string allowedGroup in this.PermittedGroups)
        {
            if (allowedGroup.Equals(client.clientGroup.Name))
            {
                return true;
            }
        }
        foreach (string allowedUser in this.PermittedUsers)
        {
            if (allowedUser.Equals(client.playername, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public override string ToString()
    {
        string permittedGroupsString = "";

        if (this.PermittedGroups.Count > 0)
        {
            permittedGroupsString = this.PermittedGroups[0].ToString();
            for (int i = 1; i < this.PermittedGroups.Count; i++)
            {
                permittedGroupsString += "," + this.PermittedGroups[i].ToString();
            }
        }

        string permittedUsersString = "";
        if (this.PermittedUsers.Count > 0)
        {
            permittedUsersString = this.PermittedUsers[0].ToString();
            for (int i = 1; i < this.PermittedUsers.Count; i++)
            {
                permittedUsersString += "," + this.PermittedUsers[i].ToString();
            }
        }

        string levelString = "";
        if (Level != null)
        {
            levelString = this.Level.ToString();
        }

        return Id + ":" + Coords + ":" + permittedGroupsString + ":" + permittedUsersString + ":" + levelString;
    }
}

public static class ServerConfigMisc
{
    public static List<AreaConfig> getDefaultAreas()
    {
        List<AreaConfig> defaultAreas = new List<AreaConfig>();

        AreaConfig publicArea = new AreaConfig();
        publicArea.Id = 1;
        publicArea.Coords = "0,0,1,9984,5000,128";
        publicArea.PermittedGroups.Add("Guest");
        defaultAreas.Add(publicArea);
        AreaConfig protectedArea = new AreaConfig();
        protectedArea.Id = 2;
        protectedArea.Coords = "0,0,1,9984,9984,128";
        protectedArea.PermittedGroups.Add("Builder");
        protectedArea.PermittedGroups.Add("Moderator");
        protectedArea.PermittedGroups.Add("Admin");
        defaultAreas.Add(protectedArea);

        return defaultAreas;
    }
}
