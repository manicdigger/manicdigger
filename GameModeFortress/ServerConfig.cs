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
        public int Port { get; set; }               //Port the server runs on
        public int MaxClients { get; set; }
        public string Key { get; set; }             //GUID to uniquely identify the server
        [XmlElement(ElementName="Creative")]
        public bool IsCreative { get; set; }          //Is this a free build server?
        public bool Public { get; set; }            //Advertise this server?
        [XmlElement(IsNullable = true)] //Forces element to appear -
        public string BuildPassword { get; set; }   //Password for Anti-Vandal building
        [XmlElement(IsNullable = true)] //Forces element to appear
        public string AdminPassword { get; set; }   //Password for managing kicks and bans
        public bool AllowFreemove { get; set; }     //Allow character to fly?
        public int MapSizeX { get; set; }
        public int MapSizeY { get; set; }
        public int MapSizeZ { get; set; }
        
        public ServerConfig()
        {
            //Set Defaults
            this.Format = 1;
            this.Name = "Manic Digger server";
            this.Motd = "MOTD";
            this.Port = 25565;
            this.MaxClients = 16;
            this.Key = Guid.NewGuid().ToString();
            this.IsCreative = true;
            this.Public = true;
            this.BuildPassword = null;
            this.AdminPassword = null;
            this.AllowFreemove = true;
            this.MapSizeX = 10000;
            this.MapSizeY = 10000;
            this.MapSizeZ = 128;
        }
    }
}
