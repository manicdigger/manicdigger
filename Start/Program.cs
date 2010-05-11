using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;
using System.Windows.Forms;

namespace Start
{
    class Program
    {
        static void Main(string[] args)
        {
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            if (args.Length > 0)
            {
                if (args[0].EndsWith(".mdlink", StringComparison.InvariantCultureIgnoreCase))
                {
                    XmlDocument d = new XmlDocument();
                    d.Load(args[0]);
                    string mode = XmlTool.XmlVal(d, "/ManicDiggerLink/GameMode");
                    if (mode.Equals("Fortress", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Process.Start(Path.Combine(appPath, "GameModeFortress"), "\"" + args[0] + "\"");
                    }
                    else if (mode.Equals("Mine", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Process.Start(Path.Combine(appPath, "GameModeMine"), "\"" + args[0] + "\"");
                    }
                    else
                    {
                        throw new Exception("Invalid game mode: " + mode);
                    }
                }             
            }
        }
    }
    public class XmlTool
    {
        public static string XmlVal(XmlDocument d, string path)
        {
            XPathNavigator navigator = d.CreateNavigator();
            XPathNodeIterator iterator = navigator.Select(path);
            foreach (XPathNavigator n in iterator)
            {
                return n.Value;
            }
            return null;
        }
        public static IEnumerable<string> XmlVals(XmlDocument d, string path)
        {
            XPathNavigator navigator = d.CreateNavigator();
            XPathNodeIterator iterator = navigator.Select(path);
            foreach (XPathNavigator n in iterator)
            {
                yield return n.Value;
            }
        }
    }
}
