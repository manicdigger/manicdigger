using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace GameModeFortress
{

    [XmlRoot(ElementName = "ManicDiggerServerConfig")]
    public class ServerConfig
    {
        public int Format { get; set; }             //XML Format Version Number
        public string Name { get; set; }
        public string Motd { get; set; }            //Message of the day
        public string WelcomeMessage { get; set; }  //Displays when the user logs in.
        public int Port { get; set; }               //Port the server runs on
        public int MaxClients { get; set; }
        public bool BuildLogging { get; set; }
        public bool ServerEventLogging { get; set; }
        public bool ChatLogging { get; set; }
        public bool AllowScripting { get; set; }
        public string Key { get; set; }             //GUID to uniquely identify the server
        [XmlElement(ElementName="Creative")]
        public bool IsCreative { get; set; }        //Is this a free build server?
        public bool Public { get; set; }            //Advertise this server?
        public bool AllowGuests { get; set; }
        public bool AllowFreemove { get; set; }     //Allow character to fly?
        public bool Flooding { get; set; }          //Allow flooding water?
        public bool Monsters { get; set; }
        public int MapSizeX { get; set; }
        public int MapSizeY { get; set; }
        public int MapSizeZ { get; set; }
        [XmlArrayItem(ElementName = "User")]
        public List<string> BannedUsers { get; set; }
        [XmlArrayItem(ElementName = "IP")]
        public List<string> BannedIPs { get; set; }
        [XmlArrayItem(ElementName = "Area")]
        public List<AreaConfig> Areas { get; set; }
        [XmlElement(ElementName="MapGenerator")]
        public MapGeneratorConfig Generator { get; set; }

        public bool IsIPBanned(string ipAddress)
        {
            foreach (string bannedip in this.BannedIPs)
            {
                if(bannedip == ipAddress)
                    return true;
            }
            return false;
        }

        public bool IsUserBanned(string username)
        {
            foreach (string banneduser in this.BannedUsers)
            {
                if (username.Equals(banneduser, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public bool UnbanPlayer(string username)
        {
            bool exists = false;
            for (int i = this.BannedUsers.Count - 1; i >= 0; i--)
            {
                string banneduser = this.BannedUsers[i];
                if (banneduser.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    exists = true;
                    this.BannedUsers.RemoveAt(i);
                    break;
                }
            }
            return exists;
        }

        public bool CanUserBuild(ManicDiggerServer.Server.Client client, int x, int y, int z)
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
            this.BuildLogging = false;
            this.ServerEventLogging = false;
            this.ChatLogging = false;
            this.AllowScripting = false;
            this.Key = Guid.NewGuid().ToString();
            this.IsCreative = true;
            this.Public = true;
            this.AllowGuests = true;
            this.AllowFreemove = true;
            this.Flooding = true;
            this.Monsters = true;
            this.MapSizeX = 10000;
            this.MapSizeY = 10000;
            this.MapSizeZ = 128;
            this.BannedIPs = new List<string>();
            this.BannedUsers = new List<string>();
            this.Areas = new List<AreaConfig>();
            this.Generator = new MapGeneratorConfig();
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
        public bool CanUserBuild(ManicDiggerServer.Server.Client client)
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
    
    
    public class MapGeneratorConfig
    {
    	int treeCount;
    	
    	public MapGeneratorConfig()
    	{
    		this.treeCount = 20;
    		this.RandomSeed = true;
            this.EnableCaves = true;
    		this.Seed = 0;
            this.GeneratorType = "NewWorldGenerator";

    	}
    	
    	public bool RandomSeed { get; set; }
        public bool EnableCaves { get; set; }
    	public int Seed { get; set; }
        public string GeneratorType { get; set; }
    	
    	public int TreeCount
    	{
    		get { return this.treeCount; }
    		set
    		{
    			if (value < 0) this.treeCount = 0;
    			else if (value > 100) this.treeCount = 100;
    			else this.treeCount = value;
    		}
    	}

        public ManicDigger.MapTools.IWorldGenerator getGenerator()
        {
            switch(this.GeneratorType)
            {
                case "NewWorldGenerator":
                    return new ManicDigger.MapTools.Generators.NewWorldGenerator();
                case "Noise2DWorldGenerator":
                    return new ManicDigger.MapTools.Generators.Noise2DWorldGenerator();
                case "FlatMapGenerator":
                    return new ManicDigger.MapTools.Generators.FlatMapGenerator();
                case "Noise3DWorldGenerator":
                    return new ManicDigger.MapTools.Generators.Noise3DWorldGenerator();
                default :
                    return new ManicDigger.MapTools.Generators.NewWorldGenerator();
            }
        }
    }

    public static class ServerConfigMisc
    {
        public static List<AreaConfig> getDefaultAreas()
        {
            List<AreaConfig> defaultAreas = new List<AreaConfig>();

            AreaConfig publicArea = new AreaConfig();
            publicArea.Id = 1;
            publicArea.Coords = "0,0,1,10000,5000,128";
            publicArea.PermittedGroups.Add("Guest");
            publicArea.PermittedGroups.Add("Registered");
            defaultAreas.Add(publicArea);
            AreaConfig builderArea = new AreaConfig();
            builderArea.Id = 2;
            builderArea.Coords = "0,5001,1,10000,10000,128";
            builderArea.PermittedGroups.Add("Builder");
            defaultAreas.Add(builderArea);
            AreaConfig adminArea = new AreaConfig();
            adminArea.Id = 3;
            adminArea.Coords = "0,0,1,10000,10000,128";
            adminArea.PermittedGroups.Add("Admin");
            defaultAreas.Add(adminArea);

            return defaultAreas;
        }
    }
}
