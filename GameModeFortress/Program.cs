using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

namespace GameModeFortress
{
    public class ManicDiggerProgram2 : IInternetGameFactory
    {
        public string GameUrl = null;
        ManicDiggerGameWindow w;
        AudioOpenAl audio;
        public void Start()
        {
            w = new ManicDiggerGameWindow();
            audio = new AudioOpenAl();
            w.audio = audio;
            MakeGame(true);
            w.GameUrl = GameUrl;
            w.Run();
        }
        private void MakeGame(bool singleplayer)
        {
            var gamedata = new GameDataTilesManicDigger();

            INetworkClient network;
            if (singleplayer)
            {
                network = new NetworkClientDummy();
            }
            else
            {
                network = new NetworkClientMinecraft();
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
            var physics = new CharacterPhysics();
            var mapgenerator = new MapGeneratorPlain();
            var internetgamefactory = this;
            if (singleplayer)
            {
                var n = (NetworkClientDummy)network;
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
                var n = (NetworkClientMinecraft)network;
                n.Map = w;
                n.Clients = clientgame;
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
            terrainDrawer.OnCrash += (a, b) => { CrashReporter.Crash(b.exception); };
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
            var game = new GameFortress();
            game.physics = physics;
            game.ticks = new TicksDummy() { game = game };
            game.terrain = terrainDrawer;
            w.game = game;
            w.login = new LoginClientMinecraft();
            w.internetgamefactory = internetgamefactory;
            physics.clientgame = clientgame;
            physics.data = gamedata;
            clientgame.physics = physics;
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
        INetworkClient network;
        ClientGame clientgame;
        ITerrainDrawer terraindrawer;
        public INetworkClient GetNetwork()
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
            new CrashReporter().Start(Start, args);
        }
        private static void Start(string[] args)
        {
            new ManicDiggerProgram2().Start();
        }
    }
}
