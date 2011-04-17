#region Using Statements
using System;
using System.Diagnostics;
using System.Threading;
using GameModeFortress;
using ManicDigger;
using ManicDigger.MapTools;
using ManicDigger.MapTools.Generators;
using System.IO;
#endregion

namespace ManicDiggerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.LoadConfig();
            var map = new ManicDiggerServer.ServerMap();
            map.d_CurrentTime = server;
            map.chunksize = 32;

            // TODO: make it possible to change the world generator at run-time!
            var generator = new Noise2DWorldGenerator();
            generator.ChunkSize = map.chunksize;
            // apply chunk size to generator
            map.d_Generator = generator;

            map.d_Heightmap = new InfiniteMapChunked2d() { chunksize = 32, d_Map = map };
            map.Reset(server.config.MapSizeX, server.config.MapSizeY, server.config.MapSizeZ);
            server.d_Map = map;
            server.d_Generator = generator;
            var getfile = new GetFilePath(new[] { "mine", "minecraft" });
            var data = new GameDataCsv();
            data.Load(File.ReadAllLines(getfile.GetFile("blocks.csv")),
                File.ReadAllLines(getfile.GetFile("defaultmaterialslots.csv")));
            server.d_Data = data;
            map.d_Data = server.d_Data;
            server.d_CraftingTableTool = new CraftingTableTool() { d_Map = map };
            bool singleplayer = false;
            foreach (string arg in args)
            {
                if (arg.Equals("singleplayer", StringComparison.InvariantCultureIgnoreCase))
                {
                    singleplayer = true;
                }
            }
            server.LocalConnectionsOnly = singleplayer;
            server.d_GetFile = getfile;
            var compression = new CompressionGzip();
            var chunkdb = new ChunkDbCompressed() { d_ChunkDb = new ChunkDbSqlite(), d_Compression = compression };
            server.d_ChunkDb = chunkdb;
            map.d_ChunkDb = chunkdb;
            server.d_NetworkCompression = compression;
            server.d_Water = new WaterFinite() { data = server.d_Data };
            if (Debugger.IsAttached)
            {
                new DependencyChecker(typeof(InjectAttribute)).CheckDependencies(
                    server, generator, map);
            }
            server.Start();
            if ((!singleplayer) && (server.config.Public))
            {
                new Thread((a) => { for (; ; ) { server.SendHeartbeat(); Thread.Sleep(TimeSpan.FromMinutes(1)); } }).Start();
            }
            for (; ; )
            {
                server.Process();
                Thread.Sleep(1);
            }
        }
    }
}