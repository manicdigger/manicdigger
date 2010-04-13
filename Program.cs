using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace ManicDigger
{
    public class InjectAttribute : Attribute
    {
    }
    public class ManicDiggerProgram2 : IInternetGameFactory
    {
        ManicDiggerGameWindow w;
        AudioOpenAl audio;
        public void Start()
        {
            w = new ManicDiggerGameWindow();
            audio = new AudioOpenAl();
            w.audio = audio;
            MakeGame(true);
            w.Run();
        }
        private void MakeGame(bool singleplayer)
        {
            var gamedata = new GameDataTilesMinecraft();
            
            IClientNetwork network;
            if (singleplayer)
            {
                network = new ClientNetworkDummy();
            }
            else
            {
                network = new ClientNetworkMinecraft();
            }
            var clientgame = new ClientGame();
            var mapstorage = clientgame;
            var getfile = new GetFilePath(new[] { "mine", "minecraft" });
            var config3d = new Config3d();
            var mapManipulator = new MapManipulator();
            var terrainDrawer = new TerrainDrawer3d();
            var the3d = w;
            var exit = w;
            var localplayerposition = w;
            var worldfeatures = new WorldFeaturesDrawer();
            var p = new CharacterPhysics();
            var mapgenerator = new MapGeneratorPlain();
            var internetgamefactory = this;
            if (singleplayer)
            {
                var n = (ClientNetworkDummy)network;
                n.player = localplayerposition;
                n.Gui = w;
                n.Map1 = w;
                n.Map = mapstorage;
                n.Data = gamedata;
                n.Gen = new fCraft.MapGenerator();
                n.Gen.data = gamedata;
                n.Gen.log = new fCraft.FLogDummy();
                n.Gen.map = new MyFCraftMap() { data = gamedata, map = mapstorage, mapManipulator = mapManipulator };
                n.Gen.rand = new GetRandomDummy();
            }
            else
            {
                var n = (ClientNetworkMinecraft)network;
                n.Map = w;
                n.Players = clientgame;
                n.Chatlines = w;
                n.Position = localplayerposition;
            }
            terrainDrawer.the3d = the3d;
            terrainDrawer.getfile = getfile;
            terrainDrawer.config3d = config3d;
            terrainDrawer.mapstorage = mapstorage;
            terrainDrawer.data = gamedata;
            terrainDrawer.exit = exit;
            terrainDrawer.localplayerposition = localplayerposition;
            terrainDrawer.worldfeatures = worldfeatures;
            worldfeatures.getfile = getfile;
            worldfeatures.localplayerposition = localplayerposition;
            worldfeatures.mapstorage = mapstorage;
            worldfeatures.the3d = the3d;
            mapManipulator.getfile = getfile;
            mapManipulator.mapgenerator = mapgenerator;
            w.clientgame = clientgame;
            w.network = network;
            w.data = gamedata;
            w.getfile = getfile;
            w.config3d = config3d;
            w.mapManipulator = mapManipulator;
            w.terrain = terrainDrawer;
            w.login = new LoginClientMinecraft();
            w.internetgamefactory = internetgamefactory;
            p.clientgame = clientgame;
            p.data = gamedata;
            clientgame.physics = p;
            clientgame.gui = w;
            mapgenerator.data = gamedata;
            audio.getfile = getfile;
            audio.gameexit = w;
            this.network = network;
            this.clientgame = clientgame;
            this.terraindrawer = terrainDrawer;
        }
        #region IInternetGameFactory Members
        public void NewInternetGame()
        {
            MakeGame(false);
        }
        IClientNetwork network;
        ClientGame clientgame;
        ITerrainDrawer terraindrawer;
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
            return terraindrawer;
        }
        #endregion
    }
    public class ManicDiggerProgram
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                try
                {
                    new ManicDiggerProgram2().Start();
                    //new ManicDiggerProgram().Start(args);
                }
                catch (Exception e)
                {
                    File.WriteAllText("crash.txt", e.ToString());
                    File.AppendAllText("crash.txt", e.StackTrace);
                }
            }
            else
            {
                new ManicDiggerProgram2().Start();
                //new ManicDiggerProgram().Start(args);
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