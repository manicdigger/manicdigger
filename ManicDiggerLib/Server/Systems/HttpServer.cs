using System.Net;
using System;
using System.Collections.Generic;
public class ServerSystemHttpServer : ServerSystem
{
    bool started;
    public override void Update(Server server, float dt)
    {
        if (!started)
        {
            started = true;

            int httpPort = server.Port + 1;
            if (server.config.EnableHTTPServer && (!server.IsSinglePlayer))
            {
                try
                {
                    httpServer = new FragLabs.HTTP.HttpServer(new IPEndPoint(IPAddress.Any, httpPort));
                    MainHttpModule m = new MainHttpModule();
                    m.server = server;
                    m.system = this;
                    httpServer.Install(m);
                    foreach (var module in server.httpModules)
                    {
                        httpServer.Install(module.module);
                    }
                    httpServer.Start();
                    Console.WriteLine(server.language.ServerHTTPServerStarted(), httpPort);
                }
                catch
                {
                    Console.WriteLine(server.language.ServerHTTPServerError(), httpPort);
                }
            }
        }
        for (int i = 0; i < server.httpModules.Count; i++)
        {
            ActiveHttpModule m = server.httpModules[i];
            if (httpServer != null)
            {
                if (!m.installed)
                {
                    m.installed = true;
                    httpServer.Install(m.module);
                }
            }
        }
    }
    internal FragLabs.HTTP.HttpServer httpServer;

    public override void OnRestart(Server server)
    {
        foreach (ActiveHttpModule m in server.httpModules)
        {
            if (m.installed)
            {
                httpServer.Uninstall(m.module);
            }
        }
        server.httpModules.Clear();
    }
}

public class ActiveHttpModule
{
    public string name;
    public ManicDigger.Func<string> description;
    public FragLabs.HTTP.IHttpModule module;
    public bool installed;
}

class MainHttpModule : FragLabs.HTTP.IHttpModule
{
    public Server server;
    public ServerSystemHttpServer system;
    public void Installed(FragLabs.HTTP.HttpServer server)
    {
    }

    public void Uninstalled(FragLabs.HTTP.HttpServer server)
    {
    }

    public bool ResponsibleForRequest(FragLabs.HTTP.HttpRequest request)
    {
        if (request.Uri.AbsolutePath.ToLower() == "/")
        {
            return true;
        }
        return false;
    }

    public bool ProcessAsync(FragLabs.HTTP.ProcessRequestEventArgs args)
    {
        string html = "<html>";
        List<string> modules = new List<string>();
        foreach (var m in server.httpModules)
        {
            modules.Add(m.name);
        }
        modules.Sort();
        foreach (string s in modules)
        {
            foreach (var m in server.httpModules)
            {
                if (m.name == s)
                {
                    html += string.Format("<a href='{0}'>{0}</a> - {1}", m.name, m.description());
                }
            }
        }
        html += "</html>";
        args.Response.Producer = new FragLabs.HTTP.BufferedProducer(html);
        return false;
    }
}