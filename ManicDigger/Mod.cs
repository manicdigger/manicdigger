using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger
{
    public interface IMod
    {
        void Start(ModManager m);
    }
    public enum DrawType
    {
        Empty,
        Solid,
        Fluid,
        Torch,
        Plant,
    }
    public enum WalkableType
    {
        Empty,
        Fluid,
        Solid,
    }
    public class SoundSet
    {
        public string Walk;
        public string Break;
        public string Build;
        public string Clone;
    }
    public class BlockType
    {
        public BlockType()
        {
        }
        public string TextureIdTop = "unknown";
        public string TextureIdBottom = "unknown";
        public string TextureIdFront = "unknown";
        public string TextureIdBack = "unknown";
        public string TextureIdLeft = "unknown";
        public string TextureIdRight = "unknown";
        public string TextureIdForInventory = "unknown";
        //public bool IsBuildable = true;
        //public string WhenPlayerPlacesGetsConvertedTo;
        //public bool IsFlower;
        public DrawType DrawType;
        public WalkableType WalkableType;
        public int Rail;
        public float WalkSpeed = 1;
        //public bool IsTransparentForLight;
        public bool IsSlipperyWalk;
        public SoundSet Sounds;
        //public bool IsFluid;
        //public bool IsTransparent;
        //public bool IsTransparentFully;
        //public bool IsEmptyForPhysics;
        //public int Season;
        public int LightRadius;
        public int StartInventoryAmount;
        public int Strength;
        public string AllTextures
        {
            set
            {
                TextureIdTop = value;
                TextureIdBottom = value;
                TextureIdFront = value;
                TextureIdBack = value;
                TextureIdLeft = value;
                TextureIdRight = value;
            }
        }
    }
    public class ModManager : IMapStorage
    {
        public void SetBlockType(int id, string name, BlockType block)
        {
        }

        public void SetBlockType(string name, BlockType block)
        {
        }

        public void AddToCreativeInventory(string blockType)
        {
        }

        public void OnBlockBuild(Jint.Delegates.Func<int, int, int> f)
        {
        }

        public void OnBlockDelete(Jint.Delegates.Func<int, int, int> f)
        {
        }

        public void OnBlockUse(Jint.Delegates.Func<int, int, int> f)
        {
        }

        public int MapSizeX { get; set; }
        public int MapSizeY { get; set; }
        public int MapSizeZ { get; set; }

        public int GetBlock(int x, int y, int z)
        {
            return 0;
        }

        public void SetBlock(int x, int y, int z, int tileType)
        {
        }

        private ManicDiggerServer.Server server;
        internal void Start(ManicDiggerServer.Server server)
        {
            this.server = server;
        }
    }
}
