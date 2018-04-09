using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace ManicDigger.Common
{
	/// <summary>
	/// TCP connection that works with raw data.
	/// Used only by client in PlatformNative.
	/// TODO: Merge with TcpConnection if possible
	/// </summary>
	public class TcpConnectionRaw
	{
		public Socket sock;
		public string address;

		Encoding encoding = Encoding.UTF8;

		public TcpConnectionRaw(Socket s)
		{
			this.sock = s;
			address = s.RemoteEndPoint.ToString();
			this.BeginReceive();
		}
		Stopwatch st = new Stopwatch();
		private void BeginReceive()
		{
			this.sock.BeginReceive(
				this.dataRcvBuf, 0,
				this.dataRcvBuf.Length,
				SocketFlags.None,
				new AsyncCallback(this.OnBytesReceived),
				this);
		}
		byte[] dataRcvBuf = new byte[1024 * 8];
		static int i = 0;
		protected void OnBytesReceived(IAsyncResult result)
		{
			int nBytesRec;
			try
			{
				nBytesRec = this.sock.EndReceive(result);
			}
			catch
			{
				try
				{
					this.sock.Close();
				}
				catch
				{
				}
				if (Disconnected != null)
				{
					Disconnected(null, new ConnectionEventArgs() { });
				}
				return;
			}
			if (nBytesRec <= 0)
			{
				try
				{
					this.sock.Close();
				}
				catch
				{
				}
				if (Disconnected != null)
				{
					Disconnected(null, new ConnectionEventArgs() { });
				}
				return;
			}

			byte[] receivedBytes = new byte[nBytesRec];
			for (int i = 0; i < nBytesRec; i++)
			{
				receivedBytes[i] = dataRcvBuf[i];
			}

			if (nBytesRec > 0)
			{
				ReceivedData.Invoke(this, new MessageEventArgs() { data = receivedBytes });
			}

			st.Reset();
			st.Start();

			this.sock.BeginReceive(
				this.dataRcvBuf, 0,
				this.dataRcvBuf.Length,
				SocketFlags.None,
				new AsyncCallback(this.OnBytesReceived),
				this);
		}
		public void Send(byte[] data)
		{
			try
			{
				sock.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
			}
			catch (Exception e)
			{
				// Dump error to console
				Console.WriteLine("TCPConnectionRaw.Send() error: " + e.ToString());
			}
		}
		void OnSend(IAsyncResult result)
		{
			sock.EndSend(result);
		}
		public event EventHandler<MessageEventArgs> ReceivedData;
		public event EventHandler<ConnectionEventArgs> Disconnected;

		public override string ToString()
		{
			if (address != null)
			{
				return address.ToString();
			}
			return base.ToString();
		}
	}

	/// <summary>
	/// TCP connection that transparently adds a message length integer to messages on the network.
	/// Used only by server.
	/// </summary>
	public class TcpConnection
	{
		public Socket sock;
		public string address;

		public TcpConnection(Socket s)
		{
			this.sock = s;
			address = s.RemoteEndPoint.ToString();
			this.BeginReceive();
		}
		void BeginReceive()
		{
			try
			{
				this.sock.BeginReceive(
					this.dataRcvBuf, 0,
					this.dataRcvBuf.Length,
					SocketFlags.None,
					new AsyncCallback(this.OnBytesReceived),
					this);
			}
			catch
			{
				InvokeDisconnected();
			}
		}
		public bool connected;
		byte[] dataRcvBuf = new byte[1024 * 8];
		protected void OnBytesReceived(IAsyncResult result)
		{
			try
			{
				int nBytesRec;
				try
				{
					nBytesRec = this.sock.EndReceive(result);
				}
				catch
				{
					try
					{
						this.sock.Close();
					}
					catch
					{
					}
					InvokeDisconnected();
					return;
				}
				if (nBytesRec <= 0)
				{
					try
					{
						this.sock.Close();
					}
					catch
					{
					}
					InvokeDisconnected();
					return;
				}

				for (int i = 0; i < nBytesRec; i++)
				{
					receivedBytes.Add(dataRcvBuf[i]);
				}

				//packetize
				while (receivedBytes.Count >= 4)
				{
					byte[] receivedBytesArray = receivedBytes.ToArray();
					int packetLength = ReadInt(receivedBytesArray, 0);
					if (receivedBytes.Count >= 4 + packetLength)
					{
						//read packet
						byte[] packet = new byte[packetLength];
						for (int i = 0; i < packetLength; i++)
						{
							packet[i] = receivedBytesArray[4 + i];
						}
						receivedBytes.RemoveRange(0, 4 + packetLength);
						ReceivedMessage.Invoke(this, new MessageEventArgs()
						{
							ClientId = this,
							data = packet
						});
					}
					else
					{
						break;
					}
				}

				this.sock.BeginReceive(
					this.dataRcvBuf, 0,
					this.dataRcvBuf.Length,
					SocketFlags.None,
					new AsyncCallback(this.OnBytesReceived),
					this);
			}
			catch
			{
				InvokeDisconnected();
			}
		}

		void InvokeDisconnected()
		{
			if (Disconnected != null)
			{
				if (sock != null)
				{
					sock.Close();
					sock = null;
					if (connected)
					{
						Disconnected(null, new ConnectionEventArgs() { ClientId = this });
					}
				}
			}
		}

		public void Send(byte[] data)
		{
			try
			{
				int length = data.Length;
				byte[] data2 = new byte[length + 4];
				WriteInt(data2, 0, length);
				for (int i = 0; i < length; i++)
				{
					data2[4 + i] = data[i];
				}
				sock.BeginSend(data2, 0, data2.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
			}
			catch
			{
				InvokeDisconnected();
			}
		}
		void OnSend(IAsyncResult result)
		{
			try
			{
				sock.EndSend(result);
			}
			catch
			{
				InvokeDisconnected();
			}
		}
		List<byte> receivedBytes = new List<byte>();
		public event EventHandler<MessageEventArgs> ReceivedMessage;
		public event EventHandler<ConnectionEventArgs> Disconnected;

		void WriteInt(byte[] writeBuf, int writePos, int n)
		{
			int a = (n >> 24) & 0xFF;
			int b = (n >> 16) & 0xFF;
			int c = (n >> 8) & 0xFF;
			int d = n & 0xFF;
			writeBuf[writePos] = (byte)(a);
			writeBuf[writePos + 1] = (byte)(b);
			writeBuf[writePos + 2] = (byte)(c);
			writeBuf[writePos + 3] = (byte)(d);
		}

		int ReadInt(byte[] readBuf, int readPos)
		{
			int n = readBuf[readPos] << 24;
			n |= readBuf[readPos + 1] << 16;
			n |= readBuf[readPos + 2] << 8;
			n |= readBuf[readPos + 3];
			return n;
		}

		public override string ToString()
		{
			if (address != null)
			{
				return address.ToString();
			}
			return base.ToString();
		}
	}

	public class ConnectionEventArgs : System.EventArgs
	{
		public TcpConnection ClientId;
	}

	public class MessageEventArgs : System.EventArgs
	{
		public TcpConnection ClientId;
		public byte[] data;
	}
}
