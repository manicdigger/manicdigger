using System;
using System.Collections.Generic;
using System.Text;

namespace FragLabs.HTTP.Modules
{
    /// <summary>
    /// Creates a test POST form on /testpost
    /// </summary>
    public class TestPost : IHttpModule
    {
        public void Installed(HttpServer server)
        {
        }

        public void Uninstalled(HttpServer server)
        {
        }

        public bool ResponsibleForRequest(HttpRequest request)
        {
            if (request.Uri.AbsolutePath.StartsWith("/testpost"))
                return true;
            return false;
        }

        public bool ProcessAsync(ProcessRequestEventArgs args)
        {
            var postDump = "";
            if (args.Request.Method == HttpMethod.POST)
            {
                postDump = String.Format("Raw POST data: <strong>{0}</strong><br />", Encoding.UTF8.GetString(args.Request.Body));
            }
            var html = @"<html>
    <body>
        <form action='/testpost' method='post'>
            " + postDump + @"
            <input type='text' name='testvar' value='' />
            <input type='submit' />
        </form>
    </body>
</html>";
            args.Response.Producer = new BufferedProducer(html);
            return false;
        }
    }
}
