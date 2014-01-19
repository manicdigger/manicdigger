using System;
using System.Collections.Generic;
using System.Text;

namespace FragLabs.HTTP
{
    /// <summary>
    /// HTTP module.
    /// </summary>
    public interface IHttpModule
    {
        /// <summary>
        /// Executed when module is installed into a server.
        /// Single module instances might be installed into multiple servers.
        /// </summary>
        /// <param name="server"></param>
        void Installed(HttpServer server);

        /// <summary>
        /// Executed when module is uninstalled from a server.
        /// </summary>
        /// <param name="server"></param>
        void Uninstalled(HttpServer server);

        /// <summary>
        /// Determines if the module will be responsible for processing the request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        bool ResponsibleForRequest(HttpRequest request);

        /// <summary>
        /// Process a request.
        /// Anything long running should be run in a thread so this method can return quickly.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        bool ProcessAsync(ProcessRequestEventArgs args);
    }

    public class ProcessRequestEventArgs : EventArgs
    {
        public event EventHandler<ProcessRequestEventArgs> Completed;
        public HttpResponse Response;
        public HttpRequest Request;
        public void Complete(object sender)
        {
            if (Completed != null)
                Completed(sender, this);
        }
    }
}
