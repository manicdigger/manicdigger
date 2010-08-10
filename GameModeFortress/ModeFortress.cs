using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using OpenTK;

namespace GameModeFortress
{
    public class GameFortress : IGameMode, IMapStorage, IClients, ITerrainInfo, IGameWorld
    {
        [Inject]
        public ITerrainDrawer terrain { get; set; }
        [Inject]
        public IViewport3d viewport { get; set; }
        [Inject]
        public INetworkClient network { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public IShadows shadows { get; set; }
        public void OnPick(Vector3 blockposnew, Vector3 blockposold, Vector3 pos3d, bool right)
        {
            var mode = right ? BlockSetMode.Create : BlockSetMode.Destroy;
            if (IsAnyPlayerInPos(blockposnew))
            {
                return;
            }
            int activematerial = (byte)viewport.MaterialSlots[viewport.activematerial];
            network.SendSetBlock(blockposnew, mode, activematerial);
            if (mode == BlockSetMode.Destroy)
            {
                activematerial = data.TileIdEmpty;
            }
            //speculative
            int x = (int)blockposnew.X;
            int y = (int)blockposnew.Y;
            int z = (int)blockposnew.Z;
            map.SetBlock(x, y, z, (byte)activematerial);

            terrain.UpdateTile(x, y, z);
            shadows.OnLocalBuild(x, y, z);
        }
        public void SendSetBlock(Vector3 vector3, BlockSetMode blockSetMode, int p)
        {
            network.SendSetBlock(vector3, blockSetMode, p);
        }
        private bool IsAnyPlayerInPos(Vector3 blockpos)
        {
            foreach (var k in players)
            {
                Vector3 playerpos = k.Value.Position;
                if (IsPlayerInPos(playerpos, blockpos))
                {
                    return true;
                }
            }
            return IsPlayerInPos(viewport.LocalPlayerPosition, blockpos);
        }
        private bool IsPlayerInPos(Vector3 playerpos, Vector3 blockpos)
        {
            if (Math.Floor(playerpos.X) == blockpos.X
                &&
                (Math.Floor(playerpos.Y + 0.5f) == blockpos.Z
                 || Math.Floor(playerpos.Y + 1 + 0.5f) == blockpos.Z)
                && Math.Floor(playerpos.Z) == blockpos.Y)
            {
                return true;
            }
            return false;
        }
        public void OnNewFrame(double dt)
        {
        }
        public IEnumerable<ICharacterToDraw> Characters
        {
            get { yield break; }
        }
        Vector3 playerpositionspawn = new Vector3(15.5f, 64, 15.5f);
        public Vector3 PlayerPositionSpawn { get { return playerpositionspawn; } set { playerpositionspawn = value; } }
        public IMapStorage map;
        public Vector3 PlayerOrientationSpawn { get { return new Vector3((float)Math.PI, 0, 0); } }
        IDictionary<int, Player> players = new Dictionary<int, Player>();
        public IDictionary<int, Player> Players { get { return players; } set { players = value; } }
        public GameFortress()
        {
            /*
            map.Map = new byte[256, 256, 64];
            map.MapSizeX = 256;
            map.MapSizeY = 256;
            map.MapSizeZ = 64;
            */
        }
        #region IMapStorage Members
        public void SetBlock(int x, int y, int z, int tileType)
        {
            map.SetBlock(x, y, z, (byte)tileType);
            shadows.OnSetBlock(x, y, z);
        }
        #endregion
        #region IMapStorage Members
        public void SetChunk(int x, int y, int z, byte[, ,] chunk)
        {
            map.SetChunk(x, y, z, chunk);
        }
        #endregion
        //float waterlevel = 32;
        #region IMapStorage Members
        //public float WaterLevel { get { return waterlevel; } set { waterlevel = value; } }
        public float WaterLevel { get { return MapSizeZ / 2; } set { } }
        #endregion
        #region IMapStorage Members
        public byte[, ,] Map { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public int MapSizeX { get { return map.MapSizeX; } set { map.MapSizeX = value; } }
        public int MapSizeY { get { return map.MapSizeY; } set { map.MapSizeY = value; } }
        public int MapSizeZ { get { return map.MapSizeZ; } set { map.MapSizeZ = value; } }
        #endregion
        #region IMapStorage Members
        public void Dispose()
        {
        }
        #endregion
        #region IGameMode Members
        public void OnNewMap()
        {
            int x = map.MapSizeX / 2;
            int y = map.MapSizeY / 2;
            playerpositionspawn = new Vector3(x + 0.5f, MapUtil.blockheight(map, data.TileIdEmpty, x, y), y + 0.5f);
        }
        #endregion
        #region IMapStorage Members
        public int GetBlock(int x, int y, int z)
        {
            return map.GetBlock(x, y, z);
        }
        #endregion
        #region IMapStorage Members
        public void UseMap(byte[, ,] map)
        {
            /*
            this.map.Map = map;
            MapSizeX = map.GetUpperBound(0) + 1;
            MapSizeY = map.GetUpperBound(1) + 1;
            MapSizeZ = map.GetUpperBound(2) + 1;
            shadows.ResetShadows();
            */
        }
        #endregion
        MapManipulator mapmanipulator = new MapManipulator();
        #region IGameMode Members
        public byte[] SaveState()
        {
            return mapmanipulator.SaveMap(map);
        }
        public void LoadState(byte[] savegame)
        {
            mapmanipulator.LoadMap(map, savegame);
            shadows.ResetShadows();
        }
        #endregion
        #region IGameMode Members
        public IEnumerable<IModelToDraw> Models
        {
            get { yield break; }
        }
        #endregion
        #region IGameMode Members
        public int FiniteInventoryAmount(int blocktype)
        {
            return 0;
        }
        #endregion
        #region IGameMode Members
        public int FiniteInventoryMax { get { return 0; } }
        #endregion
        #region IGameMode Members
        public double SIMULATIONLAG_SECONDS { get; set; }
        #endregion
        #region ITerrainInfo Members
        public int GetTerrainBlock(int x, int y, int z)
        {
            shadows.OnGetTerrainBlock(x, y, z);
            return GetBlock(x, y, z);
        }
        public System.Drawing.Color GetTerrainBlockColor(int x, int y, int z)
        {
            return System.Drawing.Color.White;
        }
        public int GetLight(int x, int y, int z)
        {
            return shadows.GetLight(x, y, z);
        }
        public float LightMaxValue()
        {
            return shadows.maxlight;
        }
        #endregion
        #region IGameMode Members
        public void ModelClick(int selectedmodelid)
        {
        }
        #endregion
        #region IGameWorld Members
        public string GameInfo
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
        #region IGameWorldRun Members
        public void Tick()
        {
        }
        public void DoCommand(byte[] command, int player_id)
        {
        }
        public int GetStateHash()
        {
            return 0;
        }
        #endregion
        public IMapStorage mapforphysics { get { return this; } }
        public bool ENABLE_FINITEINVENTORY { get; set; }
    }
}
