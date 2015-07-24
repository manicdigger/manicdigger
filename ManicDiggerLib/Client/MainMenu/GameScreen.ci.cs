public class ScreenGame : Screen
{
    public ScreenGame()
    {
        game = new Game();
    }
    Game game;

    public void Start(GamePlatform platform_, bool singleplayer_, string singleplayerSavePath_, ConnectData connectData_)
    {
        platform = platform_;
        singleplayer = singleplayer_;
        singleplayerSavePath = singleplayerSavePath_;
        connectData = connectData_;

        game.platform = platform;
        game.issingleplayer = singleplayer;
        game.assets = menu.assets;
        game.assetsLoadProgress = menu.assetsLoadProgress;

        game.Start();
        Connect(platform);
    }

    ServerSimple serverSimple;
    ModServerSimple serverSimpleMod;

    void Connect(GamePlatform platform)
    {
        if (singleplayer)
        {
            if (platform.SinglePlayerServerAvailable())
            {
                platform.SinglePlayerServerStart(singleplayerSavePath);
            }
            else
            {
                serverSimple = new ServerSimple();
                DummyNetwork network = platform.SinglePlayerServerGetNetwork();
                network.Start(platform.MonitorCreate(), platform.MonitorCreate());
                DummyNetServer server = new DummyNetServer();
                server.network = network;
                server.platform = platform;
                server.Start();
                serverSimple.Start(server, singleplayerSavePath, platform);

                serverSimpleMod = new ModServerSimple();
                serverSimpleMod.server = serverSimple;
                game.AddMod(serverSimpleMod);
                platform.SinglePlayerServerGetNetwork().ServerReceiveBuffer.Enqueue(new ByteArray());
            }

            connectData = new ConnectData();
            connectData.Username = "Local";
            game.connectdata = connectData;

            DummyNetClient netclient = new DummyNetClient();
            netclient.SetPlatform(platform);
            netclient.SetNetwork(platform.SinglePlayerServerGetNetwork());
            game.main = netclient;
        }
        else
        {
            game.connectdata = connectData;
            if (platform.EnetAvailable())
            {
                EnetNetClient client = new EnetNetClient();
                client.SetPlatform(platform);
                game.main = client;
            }
            else if (platform.TcpAvailable())
            {
                TcpNetClient client = new TcpNetClient();
                client.SetPlatform(platform);
                game.main = client;
            }
            else if (platform.WebSocketAvailable())
            {
                WebSocketClient client = new WebSocketClient();
                client.SetPlatform(platform);
                game.main = client;
            }
            else
            {
                platform.ThrowException("Network not implemented");
            }
        }
    }

    GamePlatform platform;
    ConnectData connectData;
    bool singleplayer;
    string singleplayerSavePath;

    public override void Render(float dt)
    {
        if (game.reconnect)
        {
            game.Dispose();
            menu.StartGame(singleplayer, singleplayerSavePath, connectData);
            return;
        }
        if (game.exitToMainMenu)
        {
            game.Dispose();
            if (game.GetRedirect() != null)
            {
                //Query new server for public key
                QueryClient qclient = new QueryClient();
                qclient.SetPlatform(platform);
                qclient.PerformQuery(game.GetRedirect().GetIP(), game.GetRedirect().GetPort());
                if (qclient.queryPerformed && !qclient.querySuccess)
                {
                    //Query did not succeed. Back to main menu
                    platform.MessageBoxShowError(qclient.GetServerMessage(), "Redirection error");
                    menu.StartMainMenu();
                    return;
                }
                QueryResult qresult = qclient.GetResult();
                //Get auth hash for new server
                LoginClientCi lic = new LoginClientCi();
                LoginData lidata = new LoginData();
                string token = platform.StringSplit(qresult.PublicHash, "=", new IntRef())[1];
                lic.Login(platform, connectData.Username, "", token, platform.GetPreferences().GetString("Password", ""), new LoginResultRef(), lidata);
                while (lic.loginResult.value == LoginResult.Connecting)
                {
                    lic.Update(platform);
                }
                //Check if login was successful
                if (!lidata.ServerCorrect)
                {
                    //Invalid server adress
                    platform.MessageBoxShowError("Invalid server address!", "Redirection error!");
                    menu.StartMainMenu();
                }
                else if (!lidata.PasswordCorrect)
                {
                    //Authentication failed
                    menu.StartLogin(token, null, 0);
                }
                else if (lidata.ServerAddress != null && lidata.ServerAddress != "")
                {
                    //Finally switch to the new server
                    menu.ConnectToGame(lidata, connectData.Username);
                }
            }
            else
            {
                menu.StartMainMenu();
            }
            return;
        }
        game.OnRenderFrame(dt);
    }

    public override void OnKeyDown(KeyEventArgs e)
    {
        game.KeyDown(e.GetKeyCode());
    }

    public override void OnKeyUp(KeyEventArgs e)
    {
        game.KeyUp(e.GetKeyCode());
    }

    public override void OnKeyPress(KeyPressEventArgs e)
    {
        game.KeyPress(e.GetKeyChar());
    }

    public override void OnMouseDown(MouseEventArgs e)
    {
        if (!game.platform.Focused())
        {
            return;
        }
        game.MouseDown(e);
    }

    public override void OnMouseMove(MouseEventArgs e)
    {
        if (!game.platform.Focused())
        {
            return;
        }
        game.MouseMove(e);
    }

    public override void OnMouseUp(MouseEventArgs e)
    {
        if (!game.platform.Focused())
        {
            return;
        }
        game.MouseUp(e);
    }

    public override void OnMouseWheel(MouseWheelEventArgs e)
    {
        game.MouseWheelChanged(e);
    }

    public override void OnTouchStart(TouchEventArgs e)
    {
        game.OnTouchStart(e);
    }

    public override void OnTouchMove(TouchEventArgs e)
    {
        game.OnTouchMove(e);
    }

    public override void OnTouchEnd(TouchEventArgs e)
    {
        game.OnTouchEnd(e);
    }

    public override void OnBackPressed()
    {
        game.OnBackPressed();
    }
}
