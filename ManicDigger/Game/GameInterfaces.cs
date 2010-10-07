using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger
{
    public interface IGameClient
    {
        void Start();
    }

    public interface ISinglePlayer : IGameClient, IServer
    {
    }

    public interface IOnlineGame : IGameClient
    {
        ServerConnectInfo connectinfo { get; set; }
    }

    public interface IServer
    {
        void ServerThread();
    }
}
