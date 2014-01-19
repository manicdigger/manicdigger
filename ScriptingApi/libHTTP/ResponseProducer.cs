using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FragLabs.HTTP
{
    public interface IResponseProducer : IDisposable
    {
        /// <summary>
        /// Gets if the producer has been connected.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets the http request.
        /// </summary>
        HttpRequest Request { get; }

        void Connect(HttpRequest request);
        void Disconnect();
        bool ReadAsync(ProducerEventArgs e);
        byte[] Read();
        void Dispose();
        Dictionary<string, string> AdditionalHeaders(HttpRequest request);

        /// <summary>
        /// Hook for modifying the http response before headers are sent.
        /// </summary>
        /// <param name="response"></param>
        void BeforeHeaders(HttpResponse response);
    }

    /// <summary>
    /// An HTTP response producer.
    /// </summary>
    public abstract class ResponseProducer : IResponseProducer
    {
        /// <summary>
        /// Gets if the producer has been connected.
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// Gets the http request.
        /// </summary>
        public HttpRequest Request { get; private set; }

        public ResponseProducer()
        {
            IsDisposed = false;
            Connected = false;
        }

        public virtual void Connect(HttpRequest request)
        {
            Connected = true;
            Request = request;
        }

        public virtual void Disconnect()
        {
            Connected = false;
        }

        public virtual bool ReadAsync(ProducerEventArgs e)
        {
            e.Buffer = Read();
            if (e.Buffer != null)
                e.ByteCount = e.Buffer.Length;
            else
                e.ByteCount = 0;
            return false;
        }

        public virtual byte[] Read()
        {
            return null;
        }

        public bool IsDisposed { get; private set; }
        public virtual void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("ResponseProducer");
            IsDisposed = true;
        }

        public virtual Dictionary<string, string> AdditionalHeaders(HttpRequest request)
        {
            return null;
        }

        /// <summary>
        /// Hook for modifying the http response before headers are sent.
        /// </summary>
        /// <param name="response"></param>
        public virtual void BeforeHeaders(HttpResponse response)
        {
        }
    }

    public class BufferedProducer : ResponseProducer
    {
        byte[] buffer;

        public BufferedProducer(string html) : this(html, Encoding.UTF8) { }
        public BufferedProducer(string html, Encoding encoding) : this(encoding.GetBytes(html)) { }

        public BufferedProducer(byte[] data)
        {
            buffer = data;
        }

        public override byte[] Read()
        {
            if (buffer == null)
                return null;
            var ret = buffer;
            buffer = null;
            return ret;
        }

        public override Dictionary<string, string> AdditionalHeaders(HttpRequest request)
        {
            var ret = new Dictionary<string, string>();
            ret.Add("Content-Length", buffer.Length.ToString());
            return ret;
        }
    }

    public class StreamProducer : ResponseProducer
    {
        Stream backingStream;
        byte[] readBuffer;

        public StreamProducer(Stream stream)
            : base()
        {
            backingStream = stream;
            readBuffer = new byte[1024];
        }

        public override void Disconnect()
        {
            base.Disconnect();
            backingStream.Close();
        }

        public override void Dispose()
        {
            base.Dispose();
            backingStream.Dispose();
        }

        public override Dictionary<string, string> AdditionalHeaders(HttpRequest request)
        {
            long length = -1;
            try
            {
                length = backingStream.Length;
            }
            catch (Exception)
            {
                //  Length not supported most likely
                return null;
            }

            var ret = new Dictionary<string, string>();
            ret.Add("Content-Length", length.ToString());
            return ret;
        }

        public override bool ReadAsync(ProducerEventArgs e)
        {
            backingStream.BeginRead(readBuffer, 0, readBuffer.Length, new AsyncCallback(ReadCallback), e);
            return true;
        }

        void ReadCallback(IAsyncResult result)
        {
            var read = backingStream.EndRead(result);
            var asyncArgs = (ProducerEventArgs)result.AsyncState;
            if (read > 0)
            {
                asyncArgs.Buffer = readBuffer;
                asyncArgs.ByteCount = read;
                asyncArgs.Complete(this);
            }
            else
            {
                asyncArgs.Buffer = null;
                asyncArgs.ByteCount = 0;
                asyncArgs.Complete(this);
            }
        }
    }

    public class SeekableStream : ResponseProducer
    {
        string file = "";
        long startOffset = 0;
        long endOffset = 0;
        long currentOffset = 0;
        Stream stream = null;
        int chunkSize = 4096;
        byte[] readBuffer;

        public SeekableStream(string localFile)
            : base()
        {
            IsPartial = false;
            file = localFile;
        }

        public int ChunkSize
        {
            get
            {
                return chunkSize;
            }
            set
            {
                chunkSize = value;
            }
        }

        public bool IsPartial
        {
            get;
            private set;
        }

        public override void BeforeHeaders(HttpResponse response)
        {
            if (IsPartial)
                response.StatusCode = HttpStatusCode.PartialContent;
        }

        public override void Connect(HttpRequest request)
        {
            readBuffer = new byte[chunkSize];
            base.Connect(request);
            stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            stream.Seek(startOffset, SeekOrigin.Begin);
            currentOffset = startOffset;
        }

        public override Dictionary<string, string> AdditionalHeaders(HttpRequest request)
        {
            ReadRangeHeader(request);

            var headers = new Dictionary<string, string>();

            headers.Add("Content-Type", ContentType());
            headers.Add("Content-Length", ContentLength());
            headers.Add("Accept-Ranges", "bytes");

            if (IsPartial)
            {
                headers.Add("Content-Range", ContentRange());
            }

            return headers;
        }

        public override bool ReadAsync(ProducerEventArgs e)
        {
            if (endOffset - currentOffset < 1)
            {
                e.Buffer = null;
                e.ByteCount = 0;
                return false;
            }
            if (currentOffset + chunkSize >= endOffset)
                readBuffer = new byte[endOffset - currentOffset];
            stream.BeginRead(readBuffer, 0, readBuffer.Length, new AsyncCallback(ReadCallback), e);
            return true;
        }

        void ReadCallback(IAsyncResult result)
        {
            var read = stream.EndRead(result);
            var asyncArgs = (ProducerEventArgs)result.AsyncState;
            if (read > 0)
            {
                currentOffset += read;
                asyncArgs.Buffer = readBuffer;
                asyncArgs.ByteCount = read;
                asyncArgs.Complete(this);
            }
            else
            {
                asyncArgs.Buffer = null;
                asyncArgs.ByteCount = 0;
                asyncArgs.Complete(this);
            }
        }

        void ReadRangeHeader(HttpRequest request)
        {
            if (request.Headers.ContainsKey("Range"))
            {
                var rangeHeader = request.Headers["Range"];
                var words = rangeHeader.Split("=".ToCharArray());
                if (words[0].Trim().ToLower() == "bytes")
                {
                    IsPartial = true;
                    var rangeInfo = words[1].Trim().Split("-".ToCharArray());
                    if (rangeInfo[0] != "")
                        startOffset = Convert.ToInt64(rangeInfo[0]);
                    if (rangeInfo[1] == "")
                        endOffset = new FileInfo(file).Length;
                    else
                        endOffset = Convert.ToInt64(rangeInfo[1]) + 1;
                    if (rangeInfo[0] == "")
                    {
                        endOffset = new FileInfo(file).Length;
                        startOffset = endOffset - Convert.ToInt64(rangeInfo[1]);
                    }
                }
            }
            else
            {
                endOffset = new FileInfo(file).Length;
            }
        }

        public string ContentRange()
        {
            return String.Format("bytes {0}-{1}/{2}", startOffset, (endOffset - 1), new FileInfo(file).Length);
        }

        public string ContentType()
        {
            return "video/mp4";
        }

        public string ContentLength()
        {
            if (IsPartial)
            {
                return (endOffset - startOffset).ToString();
            }
            return new FileInfo(file).Length.ToString();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (stream != null)
            {
                try
                {
                    stream.Close();
                }
                catch (Exception) { }
                try
                {
                    stream.Dispose();
                }
                catch (Exception) { }
                stream = null;
            }
        }
    }

    public class ProducerEventArgs : EventArgs
    {
        public event EventHandler<ProducerEventArgs> Completed;
        public byte[] Buffer;
        public int ByteCount;
        public void Complete(object sender)
        {
            if (Completed != null)
                Completed(sender, this);
        }
    }
}
