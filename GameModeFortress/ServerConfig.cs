using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;


namespace GameModeFortress
{
    /// <summary>
    /// Holds configuration for manic digger server.
    /// </summary>
    [XmlRoot(ElementName = "ManicDiggerServerConfig")]
    public class ServerConfig
    {

        public int Format { get; set; }             //XML Format Version Number
        public string Name { get; set; }
        public string Motd { get; set; }            //Message of the day
        public string WelcomeMessage { get; set; }  //Displays when the user logs in.
        public int Port { get; set; }               //Port the server runs on
        public int MaxClients { get; set; }
        public string Key { get; set; }             //GUID to uniquely identify the server
        [XmlElement(ElementName="Creative")]
        public bool IsCreative { get; set; }        //Is this a free build server?
        public bool Public { get; set; }            //Advertise this server?
        [XmlElement(IsNullable = true)] //Forces element to appear -
        public string BuildPassword { get; set; }   //Password for Anti-Vandal building
        [XmlElement(IsNullable = true)] //Forces element to appear
        public string AdminPassword { get; set; }   //Password for managing kicks and bans
        public bool AllowFreemove { get; set; }     //Allow character to fly?
        public int MapSizeX { get; set; }
        public int MapSizeY { get; set; }
        public int MapSizeZ { get; set; }
        [XmlArrayItem(ElementName = "User")]
        public List<string> BannedUsers { get; set; }
        [XmlArrayItem(ElementName = "IP")]
        public List<string> BannedIPs { get; set; }

        /// <summary>
        /// Determines if an ip address has been banned
        /// </summary>
        /// <param name="ipAddress">Use toString() on IPAddress created by using getAddressBytes()</param>
        /// <returns>True means that this address has been banned</returns>
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
                if (username.Equals(banneduser, StringComparison.InvariantCulture))
                    return true;
            }
            return false;
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
            this.Key = Guid.NewGuid().ToString();
            this.IsCreative = true;
            this.Public = true;
            this.BuildPassword = "";
            this.AdminPassword = "";
            this.AllowFreemove = true;
            this.MapSizeX = 10000;
            this.MapSizeY = 10000;
            this.MapSizeZ = 128;
            this.BannedIPs = new List<string>();
            this.BannedUsers = new List<string>();
        }
    }
}
