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
        public string Key { get; set; }             //GUID to uniquely identify the server
        [XmlElement(ElementName="Creative")]
        public bool IsCreative { get; set; }        //Is this a free build server?
        public bool Public { get; set; }            //Advertise this server?
        [XmlElement(IsNullable = true)] //Forces element to appear -
        public string BuildPassword { get; set; }   //Password for Anti-Vandal building
        [XmlElement(IsNullable = true)] //Forces element to appear
        public string AdminPassword { get; set; }   //Password for managing kicks and bans
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
        [XmlArrayItem(ElementName = "Admin")]
        public List<string> Admins { get; set; }    // AutoAdmin
        [XmlArrayItem(ElementName = "Builder")]
        public List<string> Builders { get; set; }  // AutoBuilder
        [XmlArrayItem(ElementName = "Mod")]
        public List<string> Mods { get; set; }      // AutoMods
        [XmlArrayItem(ElementName = "Area")]
        public List<AreaConfig> Areas { get; set; }
        [XmlElement(ElementName="MapGenerator")]
        public MapGeneratorConfig Generator { get; set; }

        public bool IsAdmin(string username)
        {
            foreach (string admin in this.Admins)
            {
                if (admin.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        public bool RemoveAdmin(string username)
        {
            bool exists = false;
            for (int i = this.Admins.Count - 1; i >= 0; i--)
            {
                string admin = this.Admins[i];
                if (admin.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    exists = true;
                    this.Admins.RemoveAt(i);
                    break;
                }
            }
            return exists;
        }

        public bool IsMod(string username)
        {
            foreach (string mod in this.Mods)
            {
                if (mod.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        public bool RemoveMod(string username)
        {
            bool exists = false;
            for (int i = this.Mods.Count - 1; i >= 0; i--)
            {
                string mod = this.Mods[i];
                if (mod.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    exists = true;
                    this.Mods.RemoveAt(i);
                    break;
                }
            }
            return exists;
        }

        public bool IsBuilder(string username)
        {
            foreach (string builder in this.Builders)
            {
                if (builder.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
             }
             return false;
        }
        public bool RemoveBuilder(string username)
        {
            bool exists = false;
            for (int i = this.Builders.Count - 1; i >= 0; i--)
            {
                string builder = this.Builders[i];
                if (builder.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    exists = true;
                    this.Builders.RemoveAt(i);
                    break;
                }
            }
            return exists;
        }

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

        public bool IsAutoAdmin(string username)
        {
            foreach (string Adminuser in this.Admins)
            {
                if (username.Equals(Adminuser, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
       }

        public bool IsAutoBuilder(string username)
        {
            foreach (string Builderuser in this.Builders)
            {
                if (username.Equals(Builderuser, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

		//Example name.
		//Default serialized config becomes "<Admins><Admin>Player name?</Admin></Admins>"
		//instead of "<Admins></Admins>".
		//It makes editing xml in notepad easier.
		//Contains "?" so nobody can use it.
        [XmlIgnore]
        public string DefaultPlayerName = "Player name?";

        public bool CanUserBuild(ManicDiggerServer.Server.Client client, int x, int y)
        {
            bool canBuild = false;
            foreach (AreaConfig area in this.Areas)
            {
                if (area.IsInCoords(x, y))
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
            this.WelcomeMessage = "Welcome to my Manic Digger server!"; //This is just the default when nothing has been set.
            this.Port = 25565;
            this.MaxClients = 16;
            this.BuildLogging = false;
            this.ServerEventLogging = false;
            this.ChatLogging = false;
            this.Key = Guid.NewGuid().ToString();
            this.IsCreative = true;
            this.Public = true;
            this.BuildPassword = "";
            this.AdminPassword = "";
            this.AllowFreemove = false;
            this.Flooding = true;
            this.Monsters = true;
            this.MapSizeX = 10000;
            this.MapSizeY = 10000;
            this.MapSizeZ = 128;
            this.BannedIPs = new List<string>();
            this.BannedUsers = new List<string>();
            this.Admins = new List<string>();
            this.Builders = new List<string>();
            this.Mods = new List<string>();
            this.Areas = new List<AreaConfig>();
            this.Generator = new MapGeneratorConfig();
        }
    }


    public class AreaConfig
    {
        private int x1;
        private int x2;
        private int y1;
        private int y2;
        private string coords;
        private bool isGuestAllowed;
        private bool isUserAllowed;
        private bool isBuilderAllowed;
        private bool isAdminAllowed;
        private string[] usersAllowed;
        private string permittedUsers;

        public AreaConfig()
        {
            this.isGuestAllowed = false;
            this.isUserAllowed = false;
            this.isBuilderAllowed = false;
            this.isAdminAllowed = false;
            this.Coords = "0,0,0,0";
            this.PermittedUsers = "";
        }

        public string Coords
        {
            get { return this.coords; }
            set
            {
                this.coords = value;
                string[] myCoords = this.Coords.Split(new char[] { ',' });
                x1 = Convert.ToInt32(myCoords[0]);
                x2 = Convert.ToInt32(myCoords[2]);
                y1 = Convert.ToInt32(myCoords[1]);
                y2 = Convert.ToInt32(myCoords[3]);
            }
        }


        public string PermittedUsers
        {
            get { return this.permittedUsers; }
            set
            {
                this.permittedUsers = value;
                this.isGuestAllowed = value.Contains("[Guest]");
                this.isUserAllowed = value.Contains("[User]");
                this.isBuilderAllowed = value.Contains("[Builder]");
                this.isAdminAllowed = value.Contains("[Admin]");

                string tmpUsers = value.Replace("[Guest]", "").Replace("[User]", "").Replace("[Builder]", "").Replace("[Admin]", "").Replace(",,", ",");

                this.usersAllowed = tmpUsers.Split(new char[] { ',' });
            }
        }

        public bool IsInCoords(int x, int y)
        {
            if (x >= x1 && x <= x2 && y >= y1 && y <= y2)
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
            if (this.isGuestAllowed)
                return true;
            else if (this.isUserAllowed && !client.playername.StartsWith("~"))
                return true;
            else if (this.isBuilderAllowed && client.CanBuild)
                return true;
            else if (this.isAdminAllowed && client.IsAdmin)
                return true;
            else
            {
                foreach (String user in this.usersAllowed)
                {
                    if (client.playername == user)
                        return true;
                }
            }

            return false;
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
}
