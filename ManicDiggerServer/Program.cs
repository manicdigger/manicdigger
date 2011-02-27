using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Collections;
using ManicDigger;
using System.Threading;
using OpenTK;
using System.Xml;
using System.Diagnostics;
using GameModeFortress;
using ProtoBuf;
using ManicDigger.MapTools;

namespace ManicDiggerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.LoadConfig();
            var map = new ManicDiggerServer.ServerMap();
            map.currenttime = server;
            map.chunksize = 32;
            var generator = new WorldGenerator();
            map.generator = generator;
            server.chunksize = 32;
            map.heightmap = new InfiniteMapChunked2d() { chunksize = 32, map = map };
            map.Reset(server.cfgmapsizex, server.cfgmapsizey, server.cfgmapsizez);
            server.map = map;
            server.generator = generator;
            server.data = new GameDataTilesManicDigger();
            map.data = server.data;
            server.craftingtabletool = new CraftingTableTool() { map = map };
            bool singleplayer = false;
            foreach (string arg in args)
            {
                if (arg.Equals("singleplayer", StringComparison.InvariantCultureIgnoreCase))
                {
                    singleplayer = true;
                }
            }
            server.LocalConnectionsOnly = singleplayer;
            server.getfile = new GetFilePath(new[] { "mine", "minecraft" });
            var compression = new CompressionGzip();
            var chunkdb = new ChunkDbCompressed() { chunkdb = new ChunkDbSqlite(), compression = compression };
            server.chunkdb = chunkdb;
            map.chunkdb = chunkdb;
            server.networkcompression = compression;
            server.water = new WaterFinite() { data = server.data };
            if (Debugger.IsAttached)
            {
                new DependencyChecker(typeof(InjectAttribute)).CheckDependencies(
                    server, generator, map);
            }
            server.Start();
            if ((!singleplayer) && (server.cfgpublic))
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