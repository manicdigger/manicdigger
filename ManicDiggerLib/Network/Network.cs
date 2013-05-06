using System;
using System.Net;

namespace ManicDigger
{
    public enum MyNetDeliveryMethod
    {
        Unknown = 0,
        Unreliable = 1,
        UnreliableSequenced = 2,
        ReliableUnordered = 34,
        ReliableSequenced = 35,
        ReliableOrdered = 67,
    }

    public interface INetServer
    {
        INetPeerConfiguration Configuration { get; }
        void Start();
        void Recycle(INetIncomingMessage msg);
        INetIncomingMessage ReadMessage();
        INetOutgoingMessage CreateMessage();
    }
    public interface INetPeerConfiguration
    {
        int Port { get; set; }
    }

    public interface INetClient
    {
        void Start();
        INetConnection Connect(string ip, int port);
        INetIncomingMessage ReadMessage();
        INetOutgoingMessage CreateMessage();
        void SendMessage(INetOutgoingMessage message, MyNetDeliveryMethod method);
    }

    public interface INetConnection
    {
        IPEndPoint RemoteEndPoint { get; }
        void SendMessage(INetOutgoingMessage msg, MyNetDeliveryMethod method, int sequenceChannel);
        void Update();
    }
    public interface INetIncomingMessage
    {
        INetConnection SenderConnection { get; }
        byte[] ReadBytes(int numberOfBytes);
        int LengthBytes { get; }
        MessageType Type { get; }
    }
    public enum MessageType
    {
        Data,
        Connect,
        Disconnect,
    }
    public interface INetOutgoingMessage
    {
        void Write(byte[] source);
    }
}
