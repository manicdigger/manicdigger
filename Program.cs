using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DependencyInjection;
using System.Diagnostics;
using System.IO;

namespace ManicDigger
{
    public class ManicDiggerProgram2
    {
        public void Start()
        {
            var data = new GameDataTilesManicDigger();
            var audio = new AudioOpenAl();
            var network = new ClientNetworkDummy();
            var clientgame = new ClientGame();
            var w = new ManicDiggerGameWindow();
            var getfile = new GetFilePath() { DataPath = "mine" };
            w.clientgame = clientgame;
            w.network = network;
            w.audio = audio;
            w.data = data;
            w.getfile = getfile;
            var p = new CharacterPhysics();
            clientgame.p = p;
            p.clientgame = clientgame;
            network.gui = w;
            network.map1 = w;
            var mapgenerator = new MapGeneratorPlain();
            mapgenerator.data = data;
            mapgenerator.map = clientgame;
            clientgame.mapgenerator = mapgenerator;
            clientgame.gui = w;
            clientgame.getfile = getfile;
            audio.getfile = getfile;
            audio.gameexit = w;
            w.Run();
        }
    }
    public class ManicDiggerProgram : IInternetGameFactory
    {
        static KernelAndBinder b;
        public void MainModule(KernelAndBinder k)
        {
            k.Bind<IGameExit, ManicDiggerGameWindow>();
            k.Bind<IGui, ManicDiggerGameWindow>();
            k.Bind<IMapGenerator, MapGeneratorPlain>();
            k.Bind<IMapStorage, ClientGame>();
            k.Bind<IMap, ManicDiggerGameWindow>();
            k.Bind<IAudio, AudioOpenAl>();
            k.Bind<IClientNetwork, ClientNetworkDummy>();
            k.BindInstance<IInternetGameFactory>(this);
        }
        public void MinecraftModule(KernelAndBinder k)
        {
            k.BindInstance<IGetFilePath>(new GetFilePath() { DataPath = "minecraft" });
            k.Bind<IGameData, GameDataTilesMinecraft>();
        }
        public void ManicDiggerModule(KernelAndBinder k)
        {
            k.BindInstance<IGetFilePath>(new GetFilePath() { DataPath = "manicdigger" });
            k.Bind<IGameData, GameDataTilesManicDigger>();
        }
        #region IInternetGameFactory Members
        public void NewInternetGame()
        {
            KernelAndBinder b = new KernelAndBinder();
            MainModule(b);
            b.BindInstance<IGui>(window);
            b.BindInstance<ManicDiggerGameWindow>(window);
            b.Bind<IClientNetwork, ClientNetworkMinecraft>();
            clientgame = b.Get<ClientGame>();
            network = b.Get<IClientNetwork>();
        }
        ManicDiggerGameWindow window;
        ClientGame clientgame;
        IClientNetwork network;
        public IClientNetwork GetNetwork()
        {
            return network;
        }
        public ClientGame GetClientGame()
        {
            return clientgame;
        }
        #endregion
        [STAThread]
        public static void Main(string[] args)
        {
            //new ManicDiggerProgram2().Start();
            new ManicDiggerProgram().Start(args);
        }
        private void Start(string[] args)
        {
            b = new KernelAndBinder();
            bool digger = args.Length < 1; if (Debugger.IsAttached) digger = false;
            MainModule(b);
            if (!digger)
            {
                MinecraftModule(b);
            }
            else
            {
                ManicDiggerModule(b);
            }
            using (var w = b.Get<ManicDiggerGameWindow>())
            {
                this.window = w;
                w.Run(0, 0);
            }
        }
    }
    public interface IGetFilePath
    {
        string GetFile(string p);
    }
    public class GetFilePathDummy : IGetFilePath
    {
        #region IGetFilePath Members
        public string GetFile(string p)
        {
            return p;
        }
        #endregion
    }
    public class GetFilePath : IGetFilePath
    {
        public GetFilePath()
        {
        }
        public string DataPath;
        public string GetFile(string filename)
        {
            if (!Directory.Exists("data"))
            {
                throw new Exception("data not found");
            }
            string a = Path.Combine(Path.Combine("data", DataPath), filename);
            string b = Path.Combine("data", filename);
            string c = filename;
            if (File.Exists(a))
            {
                return a;
            }
            if (File.Exists(b))
            {
                return b;
            }
            if (File.Exists(c))
            {
                return c;
            }
            throw new Exception(filename + " not found.");
        }
    }
}
