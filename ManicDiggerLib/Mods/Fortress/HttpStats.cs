using System;
using System.Collections.Generic;
using System.Text;
using FragLabs.HTTP;

namespace ManicDigger.Mods.Fortress
{
    public class HttpStats : IMod
    {
        public void PreStart(ModManager m)
        {
        }

        public void Start(ModManager m)
        {
            var module = new HttpInfoModule();
            module.m = m;
            m.InstallHttpModule("Stats", () => "Basic server stats", module);
        }
    }
    public class HttpInfoModule : IHttpModule
    {
        public void Installed(HttpServer server)
        {
            start = DateTime.UtcNow;
        }

        DateTime start;
        public ModManager m;

        public void Uninstalled(HttpServer server)
        {
        }

        public bool ResponsibleForRequest(HttpRequest request)
        {
            if (request.Uri.AbsolutePath.ToLower() == "/stats")
                return true;
            return false;
        }

        public bool ProcessAsync(ProcessRequestEventArgs args)
        {
            string html = "<html>";
            html += "Uptime: " + (DateTime.UtcNow - start).ToString();
            html += "</html>";
            args.Response.Producer = new BufferedProducer(html);
            return false;
        }
    }
}
