using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net;

namespace Start
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string tempfile = Path.Combine(Path.GetTempPath(), "tmp.mdlink");
            if (args.Length > 0)
            {
                RunLink(args[0]);
            }
            else
            {
                var f = new ManicDigger.ServerSelector();
                System.Windows.Forms.Application.Run(f);
                System.Windows.Forms.Application.Exit();
                if (File.Exists(tempfile))
                {
                    File.Delete(tempfile);
                }
                if (f.SelectedServer != null)
                {
                    try
                    {
                        if (!f.SelectedServerMinecraft)
                        {
                            WebClient c = new WebClient();
                            c.Headers[HttpRequestHeader.Cookie] = f.Cookie;
                            c.DownloadFile("http://fragmer.net/md/play.php?server=" + f.SelectedServer, tempfile);
                            RunLink(tempfile);
                        }
                        else
                        {
                            string ip = f.LoginIp;
                            string port = f.LoginPort;
                            string user = f.LoginUser;
                            string password = f.LoginPassword;
                            string s = string.Format(@"<?xml version=""1.0""?>
<ManicDiggerLink>
	<Ip>{0}</Ip>
	<Port>{1}</Port>
	<GameMode>Mine</GameMode>
	<User>{2}</User>
    <Password>{3}</Password>
</ManicDiggerLink>", ip, port, user, password);
                            File.WriteAllText(tempfile, s);
                            RunLink(tempfile);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                }
                RunLink(f.SinglePlayer);
            }
        }
        private static void RunLink(string filename)
        {
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            if (filename != null && filename.EndsWith(".mdlink", StringComparison.InvariantCultureIgnoreCase))
            {
                XmlDocument d = new XmlDocument();
                d.Load(filename);
                string mode = XmlTool.XmlVal(d, "/ManicDiggerLink/GameMode");
                if (mode.Equals("Fortress", StringComparison.InvariantCultureIgnoreCase))
                {
                    Process.Start(Path.Combine(appPath, "GameModeFortress.exe"), "\"" + filename + "\"");
                }
                else if (mode.Equals("Mine", StringComparison.InvariantCultureIgnoreCase))
                {
                    Process.Start(Path.Combine(appPath, "GameModeMine.exe"), "\"" + filename + "\"");
                }
                else
                {
                    throw new Exception("Invalid game mode: " + mode);
                }
            }
            if (filename == "Fortress")
            {
                Process.Start(Path.Combine(appPath, "GameModeFortress.exe"));
            }
            if (filename == "Mine")
            {
                Process.Start(Path.Combine(appPath, "GameModeMine.exe"));
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
