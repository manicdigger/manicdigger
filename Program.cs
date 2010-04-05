using System;
using System.Collections.Generic;
using System.Text;
using DependencyInjection;
using System.Diagnostics;
using System.IO;

namespace ManicDigger
{
    /*
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
    */
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
        void GameModule(KernelAndBinder b)
        {
            if (!digger)
            {
                MinecraftModule(b);
            }
            else
            {
                ManicDiggerModule(b);
            }
        }
        public void MinecraftModule(KernelAndBinder k)
        {
            k.BindInstance<IGetFilePath>(new GetFilePath(new[] { "mine", "minecraft" }));
            k.Bind<IGameData, GameDataTilesMinecraft>();
        }
        public void ManicDiggerModule(KernelAndBinder k)
        {
            k.BindInstance<IGetFilePath>(new GetFilePath(new[] { "manicdigger" }));
            k.Bind<IGameData, GameDataTilesManicDigger>();
        }
        #region IInternetGameFactory Members
        public void NewInternetGame()
        {
            KernelAndBinder b = new KernelAndBinder();
            MainModule(b);
            GameModule(b);
            b.BindInstance<IGui>(window);
            b.BindInstance<ManicDiggerGameWindow>(window);
            b.Bind<IClientNetwork, ClientNetworkMinecraft>();
            clientgame = b.Get<ClientGame>();
            network = b.Get<IClientNetwork>();
            terrain = b.Get<ITerrainDrawer>();
        }
        ManicDiggerGameWindow window;
        ClientGame clientgame;
        IClientNetwork network;
        ITerrainDrawer terrain;
        public IClientNetwork GetNetwork()
        {
            return network;
        }
        public ClientGame GetClientGame()
        {
            return clientgame;
        }
        public ITerrainDrawer GetTerrain()
        {
            return terrain;
        }
        #endregion
        [STAThread]
        public static void Main(string[] args)
        {
            //new ManicDiggerProgram2().Start();
            if (!Debugger.IsAttached)
            {
                try
                {
                    new ManicDiggerProgram().Start(args);
                }
                catch (Exception e)
                {
                    File.WriteAllText("crash.txt", e.ToString());
                    File.AppendAllText("crash.txt", e.StackTrace);
                }
            }
            else
            {
                new ManicDiggerProgram().Start(args);
            }
        }
        bool digger;
        private void Start(string[] args)
        {
            b = new KernelAndBinder();
            digger = args.Length < 1; if (Debugger.IsAttached) digger = false;
            MainModule(b);
            GameModule(b);
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
        public GetFilePath(IEnumerable<string> datapath)
        {
            this.DataPath = new List<string>(datapath);
        }
        List<string> DataPath;
        public string GetFile(string filename)
        {
            if (!Directory.Exists("data"))
            {
                throw new Exception("data not found");
            }
            List<string> paths = new List<string>();
            foreach (string s in DataPath)
            {
                paths.Add(Path.Combine("data", s));
            }
            paths.Add("data");
            paths.Add("");
            foreach (string path in paths)
            {
                bool again = false;
                string filename2 = filename;
            tryagain:
                string a = Path.Combine(path, filename2);
                if (File.Exists(a))
                {
                    return a;
                }
                if (!again && filename2.EndsWith(".png"))
                {
                    filename2 = filename2.Replace(".png", ".jpg");
                    again = true;
                    goto tryagain;
                }
                if (!again && filename2.EndsWith(".jpg"))
                {
                    filename2 = filename2.Replace(".jpg", ".png");
                    again = true;
                    goto tryagain;
                }
            }
            throw new FileNotFoundException(filename + " not found.");
        }
    }
}