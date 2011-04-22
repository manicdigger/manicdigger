using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ManicDigger.MapTools;
using ManicDigger;
using System.IO;
using ManicDiggerServer;
using ManicDigger.MapTools.Generators;

namespace GameModeFortress
{
    //Server for multiplayer and single-player.
    public class ServerProgram
    {
        public IGameExit d_Exit;
        public static bool Public = false;
        public static string SaveFilenameWithoutExtension = "default";
        public void Start()
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
            server.chunksize = 32;

            map.d_Heightmap = new InfiniteMapChunked2d() { chunksize = server.chunksize, d_Map = map };
            map.Reset(server.config.MapSizeX, server.config.MapSizeY, server.config.MapSizeZ);
            server.d_Map = map;
            server.d_Generator = generator;
            var getfile = new GetFilePath(new[] { "mine", "minecraft" });
            var data = new GameDataCsv();
            data.Load(File.ReadAllLines(getfile.GetFile("blocks.csv")),
                File.ReadAllLines(getfile.GetFile("defaultmaterialslots.csv")));
            server.d_Data = data;
            server.d_CraftingTableTool = new CraftingTableTool() { d_Map = map };
            server.LocalConnectionsOnly = !Public;
            server.d_GetFile = getfile;
            var networkcompression = new CompressionGzip();
            var diskcompression = new CompressionGzip();
            var chunkdb = new ChunkDbCompressed() { d_ChunkDb = new ChunkDbSqlite(), d_Compression = diskcompression };
            server.d_ChunkDb = chunkdb;
            map.d_ChunkDb = chunkdb;
            server.d_NetworkCompression = networkcompression;
            map.d_Data = server.d_Data;
            server.d_Water = new WaterFinite() { data = server.d_Data };
            server.SaveFilenameWithoutExtension = SaveFilenameWithoutExtension;
            server.Start();
            if ((Public) && (server.config.Public))
            {
                new Thread((a) => { for (; ; ) { server.SendHeartbeat(); Thread.Sleep(TimeSpan.FromMinutes(1)); } }).Start();
            }
            for (; ; )
            {
                server.Process();
                Thread.Sleep(1);
                if (d_Exit != null && d_Exit.exit) { return; }
            }
        }
    }
}