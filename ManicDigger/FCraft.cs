using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace ManicDigger
{
    public class MyFCraftMap : fCraft.IFMap
    {
        [Inject]
        public IMapStorage map { get; set; }
        [Inject]
        public MapManipulator mapManipulator { get; set; }
        #region IFMap Members
        public int MapSizeX
        {
            get
            {
                return map.MapSizeX;
            }
            set
            {
                map.MapSizeX = value;
            }
        }
        public int MapSizeY
        {
            get
            {
                return map.MapSizeY;
            }
            set
            {
                map.MapSizeY = value;
            }
        }
        public int MapSizeZ
        {
            get
            {
                return map.MapSizeZ;
            }
            set
            {
                map.MapSizeZ = value;
            }
        }
        public int SpawnX
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }
        public int SpawnY
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }
        public int SpawnZ
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }
        public void LoadMapArray(byte[] data, int offset)
        {
            var ms = new MemoryStream(data);
            ms.Seek(offset, SeekOrigin.Begin);
            mapManipulator.LoadMapArray(map, ms);
        }
        public bool ValidateBlockTypes()
        {
            return true;
        }
        [Inject]
        public IGameData data { get; set; }
        public void ClearMap()
        {
            for (int x = 0; x < MapSizeX; x++)
            {
                for (int y = 0; y < MapSizeY; y++)
                {
                    for (int z = 0; z < MapSizeZ; z++)
                    {
                        map.SetBlock(x, y, z, data.TileIdEmpty);
                    }
                }
            }
        }
        public void SetBlock(int x, int y, int z, int tileType)
        {
            map.SetBlock(x, y, z, (byte)tileType);
        }
        #endregion
    }
    public class GetRandomDummy : fCraft.IGetRandom
    {
        #region IGetRandom Members
        Random rnd1 = new Random();
        public Random rnd
        {
            get { return rnd1; }
        }
        #endregion
    }
}
//http://fcraft.svn.sourceforge.net/viewvc/fcraft/fCraft/fCraft/World/MapLoaderDat.cs?revision=99
//http://fcraft.svn.sourceforge.net/viewvc/fcraft/fCraft/fCraft/World/MapGenerator.cs?revision=85
//author: Fragmer, license: MIT
namespace fCraft
{
    public interface IFLogger
    {
        void Log(string s);
        void Log(string s, FLogType type);
        void Log(string message, FLogType type, params object[] values);
    }
    public interface IFMap
    {
        int MapSizeX { get; set; }
        int MapSizeY { get; set; }
        int MapSizeZ { get; set; }
        int SpawnX { get; set; }
        int SpawnY { get; set; }
        int SpawnZ { get; set; }
        void LoadMapArray(byte[] data, int offset);
        bool ValidateBlockTypes();
        void SetBlock(int x, int y, int z, int tileType);
        void ClearMap();
    }
    public enum FLogType
    {
        SystemActivity,
        Error,
        Debug,
    }
    public class FLogDummy : IFLogger
    {
        public void Log(string message, FLogType type, params object[] values)
        {
            Log(String.Format(message, values), type);
        }
        #region IFLogger Members
        public void Log(string s)
        {
        }
        public void Log(string s, FLogType type)
        {
        }
        #endregion
    }
    public class MapLoaderDAT
    {
        [Inject]
        public IFLogger log { get; set; }
        public void Load(string fileName, IFMap map)
        {
            log.Log("Converting {0}...", FLogType.SystemActivity, fileName);
            byte[] temp = new byte[8];
            //Map map = new Map(world);
            byte[] data;
            int length;
            try
            {
                using (FileStream stream = File.OpenRead(fileName))
                {
                    stream.Seek(-4, SeekOrigin.End);
                    stream.Read(temp, 0, sizeof(int));
                    stream.Seek(0, SeekOrigin.Begin);
                    length = BitConverter.ToInt32(temp, 0);
                    data = new byte[length];
                    using (GZipStream reader = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        reader.Read(data, 0, length);
                    }
                }

                //if( data[0] == 0xBE && data[1] == 0xEE && data[2] == 0xEF ) {
                for (int i = 0; i < length - 1; i++)
                {
                    if (data[i] == 0xAC && data[i + 1] == 0xED)
                    {

                        // bypassing the header crap
                        int pointer = i + 6;
                        Array.Copy(data, pointer, temp, 0, sizeof(short));
                        pointer += IPAddress.HostToNetworkOrder(BitConverter.ToInt16(temp, 0));
                        pointer += 13;

                        int headerEnd = 0;
                        // find the end of serialization listing
                        for (headerEnd = pointer; headerEnd < data.Length - 1; headerEnd++)
                        {
                            if (data[headerEnd] == 0x78 && data[headerEnd + 1] == 0x70)
                            {
                                headerEnd += 2;
                                break;
                            }
                        }

                        // start parsing serialization listing
                        int offset = 0;
                        while (pointer < headerEnd)
                        {
                            if (data[pointer] == 'Z') offset++;
                            else if (data[pointer] == 'I' || data[pointer] == 'F') offset += 4;
                            else if (data[pointer] == 'J') offset += 8;

                            pointer += 1;
                            Array.Copy(data, pointer, temp, 0, sizeof(short));
                            short skip = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(temp, 0));
                            pointer += 2;

                            // look for relevant variables
                            Array.Copy(data, headerEnd + offset - 4, temp, 0, sizeof(int));
                            if (MemCmp(data, pointer, "width"))
                            {
                                map.MapSizeX = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                            }
                            else if (MemCmp(data, pointer, "depth"))
                            {
                                map.MapSizeZ = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                            }
                            else if (MemCmp(data, pointer, "height"))
                            {
                                map.MapSizeY = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                            }
                            else if (MemCmp(data, pointer, "xSpawn"))
                            {
                                map.SpawnX = (short)(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0)) * 32 + 16);
                            }
                            else if (MemCmp(data, pointer, "ySpawn"))
                            {
                                map.SpawnZ = (short)(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0)) * 32 + 16);
                            }
                            else if (MemCmp(data, pointer, "zSpawn"))
                            {
                                map.SpawnY = (short)(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0)) * 32 + 16);
                            }

                            pointer += skip;
                        }

                        // find the start of the block array
                        bool foundBlockArray = false;
                        offset = Array.IndexOf<byte>(data, 0x00, headerEnd);
                        while (offset != -1 && offset < data.Length - 2)
                        {
                            if (data[offset] == 0x00 && data[offset + 1] == 0x78 && data[offset + 2] == 0x70)
                            {
                                foundBlockArray = true;
                                pointer = offset + 7;
                            }
                            offset = Array.IndexOf<byte>(data, 0x00, offset + 1);
                        }

                        // copy the block array... or fail
                        if (foundBlockArray)
                        {
                            map.LoadMapArray(data, pointer);
                            if (!map.ValidateBlockTypes())
                            {
                                throw new Exception("Map validation failed: unknown block types found. Either parsing has done wrong, or this is an incompatible format.");
                            }
                        }
                        else
                        {
                            throw new Exception("Could not locate block array.");
                        }
                        break;
                    }
                    //}
                }
            }
            catch (Exception ex)
            {
                log.Log("Conversion failed: {0}", FLogType.Error, ex.Message);
                log.Log(ex.StackTrace, FLogType.Debug);
            }
            log.Log("Conversion completed succesfully succesful.", FLogType.SystemActivity, fileName);
        }

        static bool MemCmp(byte[] data, int offset, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (offset + i >= data.Length || data[offset + i] != value[i]) return false;
            }
            return true;
        }
    }
    public interface IGetRandom
    {
        Random rnd { get; }
    }
    public class MapGeneratorParameters
    {
        public MapGeneratorParameters(double _roughness, double _smoothingOver, double _smoothingUnder, double _water, double _midpoint, double _sides, bool _hollow)
        {
            roughness = _roughness;
            smoothingOver = _smoothingOver;
            smoothingUnder = _smoothingUnder;
            midpoint = _midpoint;
            sides = _sides;
            water = _water;
            hollow = _hollow;
        }
        public double roughness, gBigSize, smoothingOver, smoothingUnder, water, midpoint, sides;
        public bool hollow;
    }
    public class MapGenerator
    {
        [Inject]
        public IGetRandom rand { get; set; }
        [Inject]
        public IFMap map { get; set; }
        [Inject]
        public IFLogger log { get; set; }
        [Inject]
        public IGameData data { get; set; }
        MapGeneratorParameters parameters;
        public void GenerateMap(MapGeneratorParameters parameters)
        {
            this.parameters = parameters;
            double[,] heightmap = GenerateHeightmap(map.MapSizeX, map.MapSizeY);
            Feedback("Filling...");
            map.ClearMap();
            double minheight = double.MaxValue;
            for (int x = 0; x < map.MapSizeX; x++)
            {
                for (int y = 0; y < map.MapSizeY; y++)
                {
                    double level = heightmap[x, y];
                    int levelQuantized = (int)(level * map.MapSizeZ);
                    int waterQuantized = (int)(parameters.water * map.MapSizeZ);
                    if (levelQuantized < minheight)
                    {
                        minheight = levelQuantized;
                    }
                    if (level > parameters.water)
                    {
                        level = (level - parameters.water) * parameters.smoothingOver + parameters.water;
                        map.SetBlock(x, y, levelQuantized, data.TileIdGrass);
                        if (!parameters.hollow)
                        {
                            for (int i = levelQuantized - 1; i > 0; i--)
                            {
                                if (levelQuantized - i < 5)
                                {
                                    map.SetBlock(x, y, i, data.TileIdDirt);
                                }
                                else
                                {
                                    map.SetBlock(x, y, i, data.TileIdStone);
                                }
                            }
                        }
                    }
                    else
                    {
                        level = (level - parameters.water) * parameters.smoothingUnder + parameters.water;

                        map.SetBlock(x, y, waterQuantized, data.TileIdWater);
                        if (!parameters.hollow)
                        {
                            for (int i = waterQuantized - 1; i >= levelQuantized; i--)
                            {
                                map.SetBlock(x, y, i, data.TileIdWater);
                            }
                        }
                        map.SetBlock(x, y, levelQuantized, data.TileIdSand);
                        if (!parameters.hollow)
                        {
                            for (int i = levelQuantized - 1; i > 0; i--)
                            {
                                map.SetBlock(x, y, i, data.TileIdStone);
                            }
                        }
                    }
                }
            }
            //map.MakeFloodBarrier();
            //map.Save(filename);
            Feedback("Done.");
        }
        void Feedback(string message)
        {
            //player.Message("Map generation: " + message);
            log.Log("Map generation: " + message, FLogType.SystemActivity);
        }
        double[,] GenerateHeightmap(int iWidth, int iHeight)
        {
            double c1, c2, c3, c4;
            double[,] points = new double[iWidth + 1, iHeight + 1];

            //Assign the four corners of the intial grid random color values
            //These will end up being the colors of the four corners
            c1 = parameters.sides + (rand.rnd.NextDouble() - 0.5) * 0.05;
            c2 = parameters.sides + (rand.rnd.NextDouble() - 0.5) * 0.05;
            c3 = parameters.sides + (rand.rnd.NextDouble() - 0.5) * 0.05;
            c4 = parameters.sides + (rand.rnd.NextDouble() - 0.5) * 0.05;
            parameters.gBigSize = iWidth + iHeight;
            DivideGrid(ref points, 0, 0, iWidth, iHeight, c1, c2, c3, c4, true);
            return points;
        }
        public void DivideGrid(ref double[,] points, double x, double y, int width, int height, double c1, double c2, double c3, double c4, bool isTop)
        {
            double Edge1, Edge2, Edge3, Edge4, Middle;

            int newWidth = width / 2;
            int newHeight = height / 2;

            if (width > 1 || height > 1)
            {
                if (isTop)
                {
                    Middle = ((c1 + c2 + c3 + c4) / 4) + parameters.midpoint;	//Randomly displace the midpoint!
                }
                else
                {
                    Middle = ((c1 + c2 + c3 + c4) / 4) + Displace(newWidth + newHeight);	//Randomly displace the midpoint!
                }
                Edge1 = ((c1 + c2) / 2);	//Calculate the edges by averaging the two corners of each edge.
                Edge2 = ((c2 + c3) / 2);
                Edge3 = ((c3 + c4) / 2);
                Edge4 = ((c4 + c1) / 2);//
                //Make sure that the midpoint doesn't accidentally "randomly displaced" past the boundaries!
                Middle = Rectify(Middle);
                Edge1 = Rectify(Edge1);
                Edge2 = Rectify(Edge2);
                Edge3 = Rectify(Edge3);
                Edge4 = Rectify(Edge4);
                //Do the operation over again for each of the four new grids.
                DivideGrid(ref points, x, y, newWidth, newHeight, c1, Edge1, Middle, Edge4, false);
                DivideGrid(ref points, x + newWidth, y, width - newWidth, newHeight, Edge1, c2, Edge2, Middle, false);
                if (isTop) Feedback("Heightmap: 50%");
                DivideGrid(ref points, x + newWidth, y + newHeight, width - newWidth, height - newHeight, Middle, Edge2, c3, Edge3, false);
                DivideGrid(ref points, x, y + newHeight, newWidth, height - newHeight, Edge4, Middle, Edge3, c4, false);
                if (isTop) Feedback("Heightmap: 100%");
            }
            else
            {
                //This is the "base case," where each grid piece is less than the size of a pixel.
                //The four corners of the grid piece will be averaged and drawn as a single pixel.
                double c = (c1 + c2 + c3 + c4) / 4;

                points[(int)(x), (int)(y)] = c;
                if (width == 2)
                {
                    points[(int)(x + 1), (int)(y)] = c;
                }
                if (height == 2)
                {
                    points[(int)(x), (int)(y + 1)] = c;
                }
                if ((width == 2) && (height == 2))
                {
                    points[(int)(x + 1), (int)(y + 1)] = c;
                }
            }
        }
        double Rectify(double iNum)
        {
            if (iNum < 0)
            {
                iNum = 0;
            }
            else if (iNum > 1.0)
            {
                iNum = 1.0;
            }
            return iNum;
        }
        double Displace(double SmallSize)
        {
            double Max = SmallSize / parameters.gBigSize * parameters.roughness;
            return (rand.rnd.NextDouble() - 0.5) * Max;
        }
    }
}