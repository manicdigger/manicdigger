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

namespace ManicDiggerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server();
            var map = new GameModeFortress.InfiniteMapChunked();
            map.chunksize = 32;
            var generator = new WorldGenerator();
            map.generator = generator;
            s.chunksize = 32;
            map.Reset(10000, 10000, 128);
            s.map = map;
            s.generator = generator;
            s.data = new GameDataTilesManicDigger();
            s.craftingtabletool = new CraftingTableTool() { map = map };
            bool singleplayer = false;
            foreach (string arg in args)
            {
                if (arg.Equals("singleplayer", StringComparison.InvariantCultureIgnoreCase))
                {
                    singleplayer = true;
                }
            }
            if (Debugger.IsAttached)
            {
                new DependencyChecker(typeof(InjectAttribute)).CheckDependencies(
                    s, generator, map);
            }
            s.LocalConnectionsOnly = singleplayer;
            s.getfile = new GetFilePath(new[] { "mine", "minecraft" });
            s.Start();
            if ((!singleplayer) && (s.cfgpublic))
            {
                new Thread((a) => { for (; ; ) { s.SendHeartbeat(); Thread.Sleep(TimeSpan.FromMinutes(1)); } }).Start();
            }
            for (; ; )
            {
                s.Process();
                Thread.Sleep(1);
            }
        }
    }
}