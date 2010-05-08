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

namespace ManicDiggerServer
{
    public class Server
    {
        public IMapStorage map = new MapStorage();
        public Server()
        {
            map.Map = new byte[256, 256, 64];
            map.MapSizeX = 256;
            map.MapSizeY = 256;
            map.MapSizeZ = 64;
            var gamedata=new ManicDigger.GameDataTilesMinecraft();
            fCraft.MapGenerator Gen = new fCraft.MapGenerator();
            var mapManipulator = new MapManipulator() { getfile = new GetFilePathDummy(), mapgenerator = new MapGeneratorPlain() };
            Gen.data = gamedata;
            Gen.log = new fCraft.FLogDummy();
            Gen.map = new MyFCraftMap() { data = gamedata, map = map, mapManipulator = mapManipulator };
            Gen.rand = new GetRandomDummy();
            //"mountains"
            bool hollow = false;
            Gen.GenerateMap(new fCraft.MapGeneratorParameters(8, 1, 0.5, 0.45, 0.1, 0.5, hollow));
        }
    }
    public class Water
    {
        public void Update()
        {
            if ((DateTime.Now - lastflood).TotalSeconds > 1)
            {
                lastflood = DateTime.Now;
                var curtoflood = new List<Vector3>(toflood.Keys);
                foreach (var v in curtoflood)
                {
                    Flood(v);
                    toflood.Remove(v);
                }
                Console.WriteLine(toflood.Count);
            }
        }
        public IGameData data = new GameDataTilesMinecraft();
        public void BlockChange(IMapStorage map, int x, int y, int z)
        {
            this.flooded = new Dictionary<Vector3, Vector3>();
            this.map = map;
            //water here
            if (MapUtil.IsValidPos(map, x, y, z)
                && data.IsWaterTile(map.GetBlock(x,y,z)))
            {
                Flood(new Vector3(x, y, z));
                return;
            }
            //water around
            foreach (var vv in BlocksAround(new Vector3(x, y, z)))
            {
                if (MapUtil.IsValidPos(map, (int)vv.X, (int)vv.Y, (int)vv.Z) &&
                    data.IsWaterTile(map.GetBlock((int)vv.X, (int)vv.Y, (int)vv.Z)))
                {
                    Flood(vv);
                    return;
                }
            }
            var v = new Vector3(x, y, z);
            toset.Sort((a, b) => (v - a).Length.CompareTo((v - b).Length));
        }
        IMapStorage map;
        Dictionary<Vector3, Vector3> flooded = new Dictionary<Vector3, Vector3>();
        public List<Vector3> toset = new List<Vector3>();
        Dictionary<Vector3, Vector3> toflood = new Dictionary<Vector3, Vector3>();
        DateTime lastflood;
        private void Flood(Vector3 v)
        {
            if (!MapUtil.IsValidPos(map, (int)v.X, (int)v.Y, (int)v.Z))
            {
                return;
            }
            if (flooded.ContainsKey(v))
            {
                return;
            }
            flooded.Add(v, v);
            foreach (Vector3 vv in BlocksAround(v))
            {
                var type = map.GetBlock((int)vv.X, (int)vv.Y, (int)vv.Z);
                if (type == data.TileIdEmpty)
                {
                    toset.Add(vv);
                    toflood[vv] = vv;
                }
            }
        }
        IEnumerable<Vector3> BlocksAround(Vector3 pos)
        {
            yield return pos + new Vector3(-1, 0, 0);
            yield return pos + new Vector3(1, 0, 0);
            yield return pos + new Vector3(0, -1, 0);
            yield return pos + new Vector3(0, 1, 0);
            yield return pos + new Vector3(0, 0, -1);
            //yield return pos + new Vector3(0, 0, 1); //water does not flow up.
        }
    }
    public class ClientException : Exception
    {
        public ClientException(Exception innerException, int clientid)
            : base("Client exception", innerException)
        {
            this.clientid = clientid;
        }
        public int clientid;
    }
    public class ServerNetwork
    {
        public Server server;
        public IMapStorage map;
        Socket main;
        IPEndPoint iep;
        string fListUrl = "http://list.fragmer.net/announce.php";
        public void SendHeartbeat()
        {
            try
            {
                StringWriter sw = new StringWriter();//&salt={4}
                string staticData = String.Format("name={0}&max={1}&public={2}&port={3}&version={4}"
                    , System.Web.HttpUtility.UrlEncode("[Non-Minecraft] Manic Digger, Fortress Mode, Test 1."),
                    32, "true", 25565, "7");

                List<string> playernames = new List<string>();
                lock (clients)
                {
                    foreach (var k in clients)
                    {
                        playernames.Add(k.Value.playername);
                    }
                }
                string requestString = staticData +
                                        "&users=" + clients.Count +
                                        "&hash=" + "0123456789abcdef0123456789abcdef" +
                                        "&motd=" + System.Web.HttpUtility.UrlEncode("MOTD") +
                                        "&server=Manic Digger f" +
                                        "&players=" + string.Join(",", playernames.ToArray());

                var request = (HttpWebRequest)WebRequest.Create(fListUrl);
                request.Method = "POST";
                request.Timeout = 15000; // 15s timeout
                request.ContentType = "application/x-www-form-urlencoded";
                request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                byte[] formData = Encoding.ASCII.GetBytes(requestString);
                request.ContentLength = formData.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(formData, 0, formData.Length);
                    requestStream.Flush();
                }
                request.Abort();
                Console.WriteLine("Heartbeat sent.");
            }
            catch
            {
                Console.WriteLine("Unable to send heartbeat.");
            }
        }
        public void Start(int port)
        {
            main = new Socket(AddressFamily.InterNetwork,
                   SocketType.Stream, ProtocolType.Tcp);

            iep = new IPEndPoint(IPAddress.Any, port);
            main.Bind(iep);
            main.Listen(10);
        }
        int lastclient;
        public void Process()
        {
            try
            {
                Process1();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void Process1()
        {
            if (main == null)
            {
                return;
            }
            byte[] data = new byte[1024];
            string stringData;
            int recv;
            if (main.Poll(0, SelectMode.SelectRead)) //Test for new connections
            {
                Socket client1 = main.Accept();
                IPEndPoint iep1 = (IPEndPoint)client1.RemoteEndPoint;
                
                Client c = new Client();
                c.socket = client1;
                lock (clients)
                {
                    clients[lastclient++] = c;
                }
                //client join event
            }
            ArrayList copyList = new ArrayList();
            foreach (var k in clients)
            {
                copyList.Add(k.Value.socket);
            }
            if (copyList.Count == 0)
            {
                return;
            }
            Socket.Select(copyList, null, null, 0);//10000000);

            foreach (Socket clientSocket in copyList)
            {
                int clientid = -1;
                foreach(var k in new List<KeyValuePair<int,Client>>(clients))
                {
                    if (k.Value != null && k.Value.socket == clientSocket)
                    {
                        clientid = k.Key;
                    }
                }
                Client client = clients[clientid];

                data = new byte[1024];
                try
                {
                    recv = clientSocket.Receive(data);
                }
                catch
                {
                    recv = 0;
                }
                //stringData = Encoding.ASCII.GetString(data, 0, recv);

                if (recv == 0)
                {
                    //client problem. disconnect client.
                    clients.Remove(clientid);
                    foreach (var kk in clients)
                    {
                        SendDespawnPlayer(kk.Key, (byte)clientid);
                    }
                }
                else
                {
                    for (int i = 0; i < recv; i++)
                    {
                        client.received.Add(data[i]);
                    }
                }
            }
            foreach (var k in new List<KeyValuePair<int, Client>>(clients))
            {
                Client c = k.Value;
                try
                {
                    for (; ; )
                    {
                        int bytesRead = TryReadPacket(k.Key);
                        if (bytesRead > 0)
                        {
                            clients[k.Key].received.RemoveRange(0, bytesRead);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (ClientException e)
                {
                    //client problem. disconnect client.
                    clients.Remove(e.clientid);
                    foreach (var kk in clients)
                    {
                        SendDespawnPlayer(kk.Key, (byte)e.clientid);
                    }
                }
            }
            water.Update();
            try
            {
                foreach (var v in water.toset)
                {
                    byte watertype = (byte)TileTypeMinecraft.Water;
                    map.Map[(int)v.X, (int)v.Y, (int)v.Z] = watertype;
                    foreach (var k in clients)
                    {
                        SendSetBlock(k.Key, (int)v.X, (int)v.Y, (int)v.Z, watertype);
                        //SendSetBlock(k.Key, x, z, y, watertype);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            water.toset.Clear();
        }
        public Water water = new Water();
        //returns bytes read.
        private int TryReadPacket(int clientid)
        {
            Client c = clients[clientid];
            BinaryReader br = new BinaryReader(new MemoryStream(c.received.ToArray()));
            if (c.received.Count == 0)
            {
                return 0;
            }
            int packetid = br.ReadByte();
            int totalread = 1;
            switch (packetid)
            {
                case (int)MinecraftClientPacketId.PlayerIdentification:
                    totalread += 1 + NetworkHelper.StringLength + NetworkHelper.StringLength + 1; if (c.received.Count < totalread) { return 0; }
                    byte protocolversion = br.ReadByte();
                    string username = NetworkHelper.ReadString64(br);
                    string verificationkey = NetworkHelper.ReadString64(br);
                    byte unused1 = br.ReadByte();

                    SendServerIdentification(clientid);
                    SendLevel(clientid);
                    //todo verificationkey
                    clients[clientid].playername = username;
                    //send new player spawn to all players
                    foreach (var k in clients)
                    {
                        var cc = k.Key == clientid ? byte.MaxValue : clientid;
                        SendSpawnPlayer(k.Key, (byte)cc, username, (server.map.MapSizeX / 2) * 32,
                        MapUtil.blockheight(server.map, 0, server.map.MapSizeX / 2, server.map.MapSizeY / 2) * 32,
                        (server.map.MapSizeY / 2) * 32, 0, 0);
                    }
                    //send all players spawn to new player
                    foreach (var k in clients)
                    {
                        if (k.Key != clientid)
                        {
                            SendSpawnPlayer(clientid, (byte)k.Key, k.Value.playername, 0, 0, 0, 0, 0);
                        }
                    }
                    break;
                case (int)MinecraftClientPacketId.SetBlock:
                    totalread += 3 * 2 + 1 + 1; if (c.received.Count < totalread) { return 0; }
                    int x = NetworkHelper.ReadInt16(br);
                    int y = NetworkHelper.ReadInt16(br);
                    int z = NetworkHelper.ReadInt16(br);
                    BlockSetMode mode = br.ReadByte()==0?BlockSetMode.Destroy:BlockSetMode.Create;
                    byte blocktype = br.ReadByte();
                    if (mode == BlockSetMode.Destroy)
                    {
                        blocktype = 0; //data.TileIdEmpty
                    }
                    //todo check block type.
                    map.Map[x, z, y] = blocktype;
                    foreach (var k in clients)
                    {
                        SendSetBlock(k.Key, x, z, y, blocktype);
                    }
                    //water
                    water.BlockChange(map, x, z, y);
                    break;
                case (int)MinecraftClientPacketId.PositionandOrientation:
                    totalread += 1 + 3 * 2 + 1 + 1; if (c.received.Count < totalread) { return 0; }
                    byte playerid = br.ReadByte();
                    int xx = NetworkHelper.ReadInt16(br);
                    int yy = NetworkHelper.ReadInt16(br);
                    int zz = NetworkHelper.ReadInt16(br);
                    byte heading = br.ReadByte();
                    byte pitch = br.ReadByte();
                    foreach(var k in clients)
                    {
                        if (k.Key != clientid)
                        {
                            SendPlayerTeleport(k.Key, (byte)clientid, xx, yy, zz, heading, pitch);
                        }
                    }
                    break;
                case (int)MinecraftClientPacketId.Message:
                    totalread += 1 + 64; if (c.received.Count < totalread) { return 0; }
                    byte unused2 = br.ReadByte();
                    string message = NetworkHelper.ReadString64(br);
                    foreach (var k in clients)
                    {
                        SendMessage(k.Key, message);
                    }
                    break;
                default:
                    throw new Exception();
            }
            return totalread;
        }
        private void SendSpawnPlayer(int clientid, byte playerid, string playername, int x, int y, int z, int heading, int pitch)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftServerPacketId.SpawnPlayer);
            bw.Write((byte)playerid);
            NetworkHelper.WriteString64(bw, playername);
            NetworkHelper.WriteInt16(bw, (short)x);
            NetworkHelper.WriteInt16(bw, (short)y);
            NetworkHelper.WriteInt16(bw, (short)z);
            bw.Write((byte)heading);
            bw.Write((byte)pitch);
            SendPacket(clientid, ms.ToArray());
        }
        private void SendSetBlock(int clientid, int x, int y, int z, int blocktype)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftServerPacketId.SetBlock);
            NetworkHelper.WriteInt16(bw, (short)x);
            NetworkHelper.WriteInt16(bw, (short)z);
            NetworkHelper.WriteInt16(bw, (short)y);
            bw.Write((byte)blocktype);
            SendPacket(clientid, ms.ToArray());
        }
        private void SendPlayerTeleport(int clientid, byte playerid, int x, int y, int z, int heading, int pitch)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftServerPacketId.PlayerTeleport);
            bw.Write((byte)playerid);
            NetworkHelper.WriteInt16(bw, (short)x);
            NetworkHelper.WriteInt16(bw, (short)y);
            NetworkHelper.WriteInt16(bw, (short)z);
            bw.Write((byte)heading);
            bw.Write((byte)pitch);
            SendPacket(clientid, ms.ToArray());
        }
        //SendPositionAndOrientationUpdate //delta
        //SendPositionUpdate //delta
        //SendOrientationUpdate
        private void SendDespawnPlayer(int clientid, byte playerid)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftServerPacketId.DespawnPlayer);
            bw.Write((byte)playerid);
            SendPacket(clientid, ms.ToArray());
        }
        private void SendMessage(int clientid, string message)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftServerPacketId.Message);
            bw.Write((byte)clientid);
            NetworkHelper.WriteString64(bw, message);
            SendPacket(clientid, ms.ToArray());
        }
        private void SendDisconnectPlayer(int clientid, string disconnectReason)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftServerPacketId.DisconnectPlayer);
            NetworkHelper.WriteString64(bw, disconnectReason);
            SendPacket(clientid, ms.ToArray());
        }
        public void SendPacket(int clientid, byte[] packet)
        {
            try
            {
                int sent = clients[clientid].socket.Send(packet);
                if (sent != packet.Length)
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                throw new ClientException(e, clientid);
            }
        }
        private void SendLevel(int clientid)
        {
            SendLevelInitialize(clientid);
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            var map = server.map;
            NetworkHelper.WriteInt32(bw, map.MapSizeX * map.MapSizeY * map.MapSizeZ);
            for (int z = 0; z < map.MapSizeZ; z++)
            {
                for (int y = 0; y < map.MapSizeY; y++)
                {
                    for (int x = 0; x < map.MapSizeX; x++)
                    {
                        bw.Write((byte)map.GetBlock(x, y, z));
                    }
                }
            }
            byte[] compressedmap = GzipCompression.Compress(ms.ToArray());
            MemoryStream ms2 = new MemoryStream(compressedmap);
            byte[] buf = new byte[levelchunksize];
            int totalread = 0;
            for (; ; )
            {
                int read = ms2.Read(buf, 0, levelchunksize);
                if (read == 0)
                {
                    break;
                }
                if (read < levelchunksize)
                {
                    byte[] buf2 = new byte[levelchunksize];
                    for (int i = 0; i < buf.Length; i++)
                    {
                        buf2[i] = buf[i];
                    }
                    buf = buf2;
                }
                SendLevelDataChunk(clientid, buf, (int)((totalread / compressedmap.Length) * 100));
                totalread += read;
                //Thread.Sleep(100);
            }
            SendLevelFinalize(clientid);
        }
        int levelchunksize = 1024;
        private void SendLevelInitialize(int clientid)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftServerPacketId.LevelInitialize);
            SendPacket(clientid, ms.ToArray());
        }
        private void SendLevelDataChunk(int clientid, byte[] chunk, int percentcomplete)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftServerPacketId.LevelDataChunk);
            NetworkHelper.WriteInt16(bw, (short)chunk.Length);
            bw.Write((byte[])chunk);
            bw.Write((byte)percentcomplete);
            SendPacket(clientid, ms.ToArray());
        }
        private void SendLevelFinalize(int clientid)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftServerPacketId.LevelFinalize);
            NetworkHelper.WriteInt16(bw, (short)server.map.MapSizeX);
            NetworkHelper.WriteInt16(bw, (short)server.map.MapSizeZ);
            NetworkHelper.WriteInt16(bw, (short)server.map.MapSizeY);
            SendPacket(clientid, ms.ToArray());
        }
        string servername = "Manic Digger server";
        string MOTD = "MOTD";
        int CurrentProtocolVersion = 7;
        private void SendServerIdentification(int clientid)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)MinecraftServerPacketId.ServerIdentification);
            bw.Write((byte)CurrentProtocolVersion);
            NetworkHelper.WriteString64(bw, servername);
            NetworkHelper.WriteString64(bw, MOTD);
            bw.Write((byte)0);
            SendPacket(clientid, ms.ToArray());
        }
        class Client
        {
            public Socket socket;
            public List<byte> received = new List<byte>();
            public string playername = "player";
        }
        Dictionary<int, Client> clients = new Dictionary<int, Client>();
    }
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            ServerNetwork s = new ServerNetwork();
            s.server = server;
            s.map = server.map;
            s.Start(25565);
            new Thread((a) => { for (; ; ) { s.SendHeartbeat(); Thread.Sleep(TimeSpan.FromMinutes(1)); } }).Start();
            for (; ; )
            {
                s.Process();
                Thread.Sleep(1);
            }
        }
    }
}