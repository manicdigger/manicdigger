using System;
using System.Collections.Generic;
using System.Text;
using ManicDiggerServer;
using ProtoBuf;

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
        Transparent,
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
    [ProtoContract]
    public class SoundSet
    {
        [ProtoMember(1)]
        public string[] Walk = new string[0];
        [ProtoMember(2)]
        public string[] Break = new string[0];
        [ProtoMember(3)]
        public string[] Build = new string[0];
        [ProtoMember(4)]
        public string[] Clone = new string[0];
    }
    [ProtoContract]
    public class BlockType
    {
        public BlockType()
        {
        }
        [ProtoMember(1)]
        public string TextureIdTop = "Unknown";
        [ProtoMember(2)]
        public string TextureIdBottom = "Unknown";
        [ProtoMember(3)]
        public string TextureIdFront = "Unknown";
        [ProtoMember(4)]
        public string TextureIdBack = "Unknown";
        [ProtoMember(5)]
        public string TextureIdLeft = "Unknown";
        [ProtoMember(6)]
        public string TextureIdRight = "Unknown";
        [ProtoMember(7)]
        public string TextureIdForInventory = "Unknown";

        //public bool IsBuildable = true;
        //public string WhenPlayerPlacesGetsConvertedTo;
        //public bool IsFlower;
        [ProtoMember(8)]
        public DrawType DrawType;
        [ProtoMember(9)]
        public WalkableType WalkableType;
        [ProtoMember(10)]
        public int Rail;
        [ProtoMember(11)]
        public float WalkSpeed = 1;
        //public bool IsTransparentForLight;
        [ProtoMember(12)]
        public bool IsSlipperyWalk;
        [ProtoMember(13)]
        public SoundSet Sounds;
        //public bool IsFluid;
        //public bool IsTransparent;
        //public bool IsTransparentFully;
        //public bool IsEmptyForPhysics;
        //public int Season;
        [ProtoMember(14)]
        public int LightRadius;
        [ProtoMember(15)]
        public int StartInventoryAmount;
        [ProtoMember(16)]
        public int Strength;
        [ProtoMember(17)]
        public string Name;
        [ProtoMember(18)]
        public bool IsBuildable;
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
                TextureIdForInventory = value;
            }
        }
    }
    public class ModManager : IMapStorage
    {
        public void SetBlockType(int id, string name, BlockType block)
        {
            server.SetBlockType(id, name, block);
        }

        public void SetBlockType(string name, BlockType block)
        {
            server.SetBlockType(name, block);
        }

        public int GetBlockId(string name)
        {
            for (int i = 0; i < server.BlockTypes.Length; i++)
            {
                if (server.BlockTypes[i].Name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public void AddToCreativeInventory(string blockType)
        {
            int id = GetBlockId(blockType);
            if (id == -1)
            {
                throw new Exception(blockType);
            }
            server.BlockTypes[id].IsBuildable = true;
            server.d_Data.UseBlockType(id, server.BlockTypes[id], null);
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

        private Server server;
        internal void Start(ManicDiggerServer.Server server)
        {
            this.server = server;
        }

        public void SetSunLevels(int[] sunLevels)
        {
            server.SetSunLevels(sunLevels);
        }

        public void SetLightLevels(float[] lightLevels)
        {
            server.SetLightLevels(lightLevels);
        }

        public void AddCraftingRecipe(string output, int outputAmount, string Input0, int Input0Amount)
        {
            if (GetBlockId(output) == -1) { throw new Exception(output); }
            if (GetBlockId(Input0) == -1) { throw new Exception(Input0); }
            CraftingRecipe r = new CraftingRecipe();
            r.ingredients = new Ingredient[]
            {
                new Ingredient(){Type=GetBlockId(Input0), Amount=Input0Amount},
            };
            r.output = new Ingredient() { Type = GetBlockId(output), Amount = outputAmount };
            server.craftingrecipes.Add(r);
        }

        public void AddCraftingRecipe2(string output, int outputAmount, string Input0, int Input0Amount, string Input1, int Input1Amount)
        {
            if (GetBlockId(output) == -1) { throw new Exception(output); }
            if (GetBlockId(Input1) == -1) { throw new Exception(Input1); }
            CraftingRecipe r = new CraftingRecipe();
            r.ingredients = new Ingredient[]
            {
                new Ingredient(){Type=GetBlockId(Input0), Amount=Input0Amount},
                new Ingredient(){Type=GetBlockId(Input1), Amount=Input1Amount},
            };
            r.output = new Ingredient() { Type = GetBlockId(output), Amount = outputAmount };
            server.craftingrecipes.Add(r);
        }

        public void AddCraftingRecipe3(string output, int outputAmount, string Input0, int Input0Amount, string Input1, int Input1Amount, string Input2, int Input2Amount)
        {
            if (GetBlockId(output) == -1) { throw new Exception(output); }
            if (GetBlockId(Input0) == -1) { throw new Exception(Input0); }
            if (GetBlockId(Input1) == -1) { throw new Exception(Input1); }
            if (GetBlockId(Input2) == -1) { throw new Exception(Input2); }
            CraftingRecipe r = new CraftingRecipe();
            r.ingredients = new Ingredient[]
            {
                new Ingredient(){Type=GetBlockId(Input0), Amount=Input0Amount},
                new Ingredient(){Type=GetBlockId(Input1), Amount=Input1Amount},
                new Ingredient(){Type=GetBlockId(Input2), Amount=Input2Amount},
            };
            r.output = new Ingredient() { Type = GetBlockId(output), Amount = outputAmount };
            server.craftingrecipes.Add(r);
        }
    }
}
