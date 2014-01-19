using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace FragLabs.HTTP
{
    /// <summary>
    /// An HTTP request.
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// Socket connected to the HTTP client.
        /// </summary>
        public Socket ClientSocket { get; set; }

        /// <summary>
        /// HTTP request method.
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Request URI.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// HTTP version.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Request headers.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Request body, if any.
        /// </summary>
        public byte[] Body { get; set; }
    }
}
