using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Utilities;

namespace ManicDigger
{
    public class MDLinkReader
    {
        public static ServerConnectInfo ReadMDLink(string filename)
        {
            // The info we need to connect to the server
            ServerConnectInfo info = new ServerConnectInfo();

            // The mdlink is an xml file
            XmlDocument d = new XmlDocument();
            d.Load(filename);

            // Grab the username
            info.username = XmlTool.XmlVal(d, "/ManicDiggerLink/User");
            // Grab the url
            info.url = XmlTool.XmlVal(d, "/ManicDiggerLink/Ip");
            // Grab the port
            info.port = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerLink/Port"));
            // Grab the gamemode
            info.gamemode = XmlTool.XmlVal(d, "/ManicDiggerLink/GameMode");

            return info;
        }
    }
}
