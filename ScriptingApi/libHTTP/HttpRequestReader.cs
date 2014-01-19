using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace FragLabs.HTTP
{
    /// <summary>
    /// Reads HTTP requests from a given socket.
    /// </summary>
    class HttpRequestReader : IDisposable
    {
        /// <summary>
        /// Event handler for when reading a request is complete.
        /// </summary>
        /// <param name="request"></param>
        public delegate void ReadCompleteEventHandler(HttpRequest request);

        /// <summary>
        /// Event triggered when reading a request is completed.
        /// </summary>
        public event ReadCompleteEventHandler ReadComplete;

        /// <summary>
        /// Event handler for erronous http requests.
        /// </summary>
        /// <param name="httpStatusCode"></param>
        public delegate void HttpErrorEventHandler(HttpStatusCode httpStatusCode, HttpRequest request);

        /// <summary>
        /// Event triggered when an http error is encountered.
        /// </summary>
        public event HttpErrorEventHandler HttpError;

        /// <summary>
        /// Socket the request is being read from.
        /// </summary>
        Socket socket = null;

        /// <summary>
        /// Arguments used for async operations.
        /// </summary>
        SocketAsyncEventArgs asyncArgs = null;

        /// <summary>
        /// Number of bytes to read, maximum, per read operation.
        /// </summary>
        int bufferSize = 4096;

        /// <summary>
        /// Request text to be processed. Does NOT contain the full body.
        /// </summary>
        string requestText = "";

        /// <summary>
        /// Current cursor position in the requestBodyEncoded array.
        /// </summary>
        int requestBodyWritePosition = 0;

        /// <summary>
        /// The encoded request body, if any.
        /// </summary>
        byte[] requestBodyEncoded = null;

        /// <summary>
        /// A buffer temporarily holding request body data that was received in the same packet as headers.
        /// Most clients probably don't do this.
        /// </summary>
        byte[] recvOverflowBuffer = null;

        /// <summary>
        /// HTTP request being read.
        /// </summary>
        HttpRequest request = null;

        /// <summary>
        /// Is processing complete?
        /// </summary>
        bool processingComplete = false;

        /// <summary>
        /// Current request processing state.
        /// </summary>
        ProcessingState processingState = ProcessingState.InitialLine;

        /// <summary>
        /// Maximum size allowed for the request body.
        /// </summary>
        int MaxBodySize = 0;

        /// <summary>
        /// Create a new request reader to read from the given socket.
        /// </summary>
        /// <param name="sock">Socket to read request from.</param>
        public HttpRequestReader(Socket sock, int MaxBodySize)
        {
            socket = sock;
            this.MaxBodySize = MaxBodySize;
            asyncArgs = new SocketAsyncEventArgs();
            asyncArgs.SetBuffer(new byte[bufferSize], 0, bufferSize);
            asyncArgs.Completed += ProcessRecv;
            request = new HttpRequest()
            {
                ClientSocket = sock,
                Body = new byte[0],
                Headers = new Dictionary<string,string>(),
                Method = default(HttpMethod),
                Uri = null,
                Version = default(Version)
            };
        }

        /// <summary>
        /// Read the HTTP request from the socket.
        /// </summary>
        /// <returns></returns>
        public void AsyncReadRequest()
        {
            SockRecv(socket);
        }

        /// <summary>
        /// Starts an async recv operation.
        /// </summary>
        /// <param name="sock"></param>
        void SockRecv(Socket sock)
        {
            asyncArgs.SetBuffer(0, bufferSize);
            if (!sock.ReceiveAsync(asyncArgs))
                ProcessRecv(sock, asyncArgs);
        }

        /// <summary>
        /// Callback when receiving data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void ProcessRecv(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                //  disposing prevents memory leaks
                args.Dispose();
                asyncArgs = new SocketAsyncEventArgs();
                asyncArgs.SetBuffer(new byte[bufferSize], 0, bufferSize);
                asyncArgs.Completed += ProcessRecv;

                if (processingState == ProcessingState.Body)
                {
                    for (int i = 0; i < args.BytesTransferred; i++)
                    {
                        if (requestBodyWritePosition == requestBodyEncoded.Length)
                            break;
                        requestBodyEncoded[requestBodyWritePosition++] = args.Buffer[i];
                    }
                    ProcessRequestBody();
                    if (!processingComplete)
                        SockRecv((Socket)sender);
                }
                else
                {
                    var bytesInHeader = PreProcessBuffer(args);
                    requestText += Encoding.UTF8.GetString(args.Buffer, 0, bytesInHeader);
                    ProcessRequestText();
                    if (!processingComplete)
                        SockRecv((Socket)sender);
                }
            }
            else
            {
                //  error, connection broken
                processingComplete = true;
            }
        }

        /// <summary>
        /// Examines the received buffer for termination of HTTP headers.
        /// </summary>
        /// <returns>Number of bytes that make up the received header text.</returns>
        int PreProcessBuffer(SocketAsyncEventArgs args)
        {
            //  look in the buffer for CR+LF CR+LF OR LF LF that terminates headers
            //  if found, trunicate the text to be processed and store extra data on the request body
            int headerEnd = IndexOf(new byte[]{ 13, 10, 13, 10 }, args.Buffer);
            if (headerEnd > -1) headerEnd += 4;
            else
            {
                headerEnd = IndexOf(new byte[] { 10, 10 }, args.Buffer);
                if (headerEnd > -1) headerEnd += 2;
            }
            if (headerEnd < 0)
                return args.BytesTransferred;

            //  copy data from the buffer if possible
            if (headerEnd < args.BytesTransferred)
            {
                recvOverflowBuffer = new byte[args.BytesTransferred - headerEnd];
                int index = 0;
                for (int i = headerEnd; i < args.BytesTransferred; i++)
                {
                    recvOverflowBuffer[index++] = args.Buffer[i];
                }
            }

            return headerEnd;
        }

        /// <summary>
        /// Returns the index of the supplied pattern in the supplied data.
        /// Returns -1 when not found.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        int IndexOf(byte[] pattern, byte[] data)
        {
            int ret = -1;

            for (int i = 0; i < data.Length - pattern.Length; i++)
            {
                var matched = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (data[i + j] != pattern[j])
                    {
                        matched = false;
                        break;
                    }
                }
                if (matched)
                {
                    ret = i;
                    break;
                }
            }

            return ret;
        }

        void InitRequestBody()
        {
            int contentLength = 0;
            if (!Int32.TryParse(request.Headers["Content-Length"], out contentLength) || contentLength > MaxBodySize)
            {
                ParsingError(HttpStatusCode.RequestEntityTooLarge);
                return;
            }

            requestBodyEncoded = new byte[contentLength];

            if (recvOverflowBuffer != null)
            {
                for (int i = 0; i < recvOverflowBuffer.Length; i++)
                {
                    requestBodyEncoded[i] = recvOverflowBuffer[i];
                }
                requestBodyWritePosition = recvOverflowBuffer.Length;
            }

            ProcessRequestBody();
        }

        void ProcessRequestBody()
        {
            if (processingComplete || processingState != ProcessingState.Body)
                return;

            if (requestBodyWritePosition < requestBodyEncoded.Length)
                return;

            request.Body = requestBodyEncoded;

            processingComplete = true;
            if (ReadComplete != null)
                ReadComplete(request);
        }

        /// <summary>
        /// Processes the request text received so far.
        /// </summary>
        void ProcessRequestText()
        {
            if (processingComplete)
                return;

            switch (processingState)
            {
                case ProcessingState.InitialLine:
                    {
                        var lfIndex = requestText.IndexOf("\n");
                        if (lfIndex > -1)
                        {
                            var initialLine = requestText.Substring(0, lfIndex + 1).Trim();
                            requestText = requestText.Substring(lfIndex + 1);
                            ParseInitialLine(initialLine);
                            if (!processingComplete)
                            {
                                processingState = ProcessingState.Headers;
                                ProcessRequestText();
                            }
                        }
                    }
                    break;
                case ProcessingState.Headers:
                    {
                        var lfIndex = requestText.IndexOf("\n");
                        while (lfIndex > -1)
                        {
                            var line = requestText.Substring(0, lfIndex + 1).Trim();
                            if (line == "")
                            {
                                processingState = ProcessingState.Body;
                                break;
                            }
                            requestText = requestText.Substring(lfIndex + 1);
                            ParseHeader(line);
                            if (processingComplete)
                                return;
                            lfIndex = requestText.IndexOf("\n");
                        }

                        if (processingState == ProcessingState.Body)
                        {
                            //  determine if a body should be read or not
                            if ((request.Method == HttpMethod.POST ||
                                request.Method == HttpMethod.PUT ||
                                request.Method == HttpMethod.PATCH) &&
                                request.Headers.ContainsKey("Content-Length"))
                            {
                                //  body required
                                InitRequestBody();
                            }
                            else
                            {
                                //  done reading
                                processingComplete = true;
                                if (ReadComplete != null)
                                    ReadComplete(request);
                            }
                        }
                    }
                    break;
            }
        }

        void ParseInitialLine(string line)
        {
            var bits = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (bits.Length < 3)
                bits = line.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (bits.Length < 3)
            {
                ParsingError(HttpStatusCode.BadRequest);
                return;
            }
            HttpMethod method;
            try
            {
                method = (HttpMethod)Enum.Parse(typeof(HttpMethod), bits[0], true);
            }
            catch
            {
                ParsingError(HttpStatusCode.BadRequest);
                return;
            }
            request.Method = method;
            var uriStr = bits[1];
            if (!uriStr.StartsWith("http://") && !uriStr.StartsWith("https://"))
                uriStr = "http://localhost" + uriStr;
            request.Uri = new Uri(uriStr);
            if (bits[2] == "HTTP/1.0")
                request.Version = HttpVersion.Version10;
            else if (bits[2] == "HTTP/1.1")
                request.Version = HttpVersion.Version11;
            else
            {
                ParsingError(HttpStatusCode.BadRequest);
                return;
            }
        }

        void ParseHeader(string line)
        {
            var seperatorIndex = line.IndexOf(":");
            if (seperatorIndex > -1)
            {
                var header = line.Substring(0, seperatorIndex);
                var value = line.Substring(seperatorIndex + 1).Trim();
                if (!request.Headers.ContainsKey(header))
                    request.Headers.Add(header, value);
                else
                    request.Headers[header] = value;
            }
            else
            {
                ParsingError(HttpStatusCode.BadRequest);
            }
        }

        void ParsingError(HttpStatusCode httpStatusCode)
        {
            processingComplete = true;
            if (HttpError != null)
                HttpError(httpStatusCode, request);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    enum ProcessingState
    {
        InitialLine,
        Headers,
        Body
    }
}
