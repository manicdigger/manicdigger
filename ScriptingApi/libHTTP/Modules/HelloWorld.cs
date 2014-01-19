using System;
using System.Collections.Generic;
using System.Text;

namespace FragLabs.HTTP.Modules
{
    /// <summary>
    /// HTTP module that prints out a Hello World html page when /helloworld is requested.
    /// </summary>
    public class HelloWorld : IHttpModule
    {
        public void Installed(HttpServer server)
        {
            
        }

        public void Uninstalled(HttpServer server)
        {
            
        }

        public bool ResponsibleForRequest(HttpRequest request)
        {
            if (request.Uri.AbsolutePath.ToLower() == "/helloworld")
                return true;
            return false;
        }

        public bool ProcessAsync(ProcessRequestEventArgs args)
        {
            var html = @"<!doctype html>
<html>
    <head>
        <title>Hello World</title>
        <style type='text/css'>
            * { font-family: Arial, sans-serif  }
        </style>
    </head>
    <body>
        <h1>Hello World</h1>
        <p>This is a demo from a simple HttpModule for the lightweight, async focused HTTP library libHTTP.</p>
    </body>
</html>";
            args.Response.Headers.Add("Content-Type", "text/html");
            args.Response.Producer = new BufferedProducer(html);
            return false;
        }
    }
}
