using System;
using System.Collections.Generic;
using System.Text;
using ManicDiggerServer;
using ProtoBuf;
using GameModeFortress;
using Jint.Delegates;

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
        OpenDoor,
        ClosedDoor,
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
    public class ModManager
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
            //return -1;
            throw new Exception(name);
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

        public void RegisterOnBlockBuild(Action<int, int, int, int> f)
        {
            server.onbuild.Add(f);
        }

        public void RegisterOnBlockDelete(ManicDiggerServer.Server.Action<int, int, int, int, int> f)
        {
            server.ondelete.Add(f);
        }

        public void RegisterOnBlockUse(Action<int, int, int, int> f)
        {
            server.onuse.Add(f);
        }

        public int GetMapSizeX() { return server.d_Map.MapSizeX; }
        public int GetMapSizeY() { return server.d_Map.MapSizeY; }
        public int GetMapSizeZ() { return server.d_Map.MapSizeZ; }

        public int GetBlock(int x, int y, int z)
        {
            return server.d_Map.GetBlock(x,y,z);
        }

        public string GetBlockName(int blockType)
        {
            return "";
        }

        public string GetBlockNameAt(int x, int y, int z)
        {
            return "";
        }

        public void SetBlock(int x, int y, int z, int tileType)
        {
            server.SetBlockAndNotify(x, y, z, tileType);
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

        public void SetString(string language, string text, string translation)
        {
        }

        public bool IsValidPos(int x, int y, int z)
        {
            return MapUtil.IsValidPos(server.d_Map, x, y, z);
        }

        public void RegisterTimer(Action a, int interval)
        {
            server.timers[new ManicDigger.Timer() { INTERVAL = 5 }] = delegate { a(); };
        }

        public void PlaySoundAt(Vector3i pos, string sound)
        {
            foreach (var k in server.clients)
            {
                int distance = server.DistanceSquared(new Vector3i((int)k.Value.PositionMul32GlX / 32, (int)k.Value.PositionMul32GlZ / 32, (int)k.Value.PositionMul32GlY / 32), pos);
                if (distance < 255)
                {
                    server.SendSound(k.Key, sound);
                }
            }
        }

        public int NearestPlayer(int x, int y, int z)
        {
            int closeplayer = -1;
            int closedistance = -1;
            foreach (var k in server.clients)
            {
                int distance = server.DistanceSquared(new Vector3i((int)k.Value.PositionMul32GlX / 32, (int)k.Value.PositionMul32GlZ / 32, (int)k.Value.PositionMul32GlY / 32), new Vector3i(x, y, z));
                if (closedistance == -1 || distance < closedistance)
                {
                    closedistance = distance;
                    closeplayer = k.Key;
                }
            }
            return closeplayer;
        }

        public void GrabBlock(int player, int block)
        {
            Inventory inventory = server.GetPlayerInventory(server.clients[player].playername).Inventory;
            
            var item = new Item();
            item.ItemClass = ItemClass.Block;
            item.BlockId = server.d_Data.WhenPlayerPlacesGetsConvertedTo[block];
            server.GetInventoryUtil(inventory).GrabItem(item, 0);
        }

        public bool PlayerHasPrivilege(int player, string p)
        {
            return server.clients[player].privileges.Contains(p);
        }

        public bool IsCreative()
        {
            return server.config.IsCreative;
        }

        public bool IsBlockFluid(int block)
        {
            return server.d_Data.IsFluid[block];
        }

        public void NotifyInventory(int player)
        {
            server.clients[player].IsInventoryDirty = true;
            server.NotifyInventory(player);
        }

        public string colorError()
        {
            return server.colorError;
        }

        public void SendMessage(int player, string p)
        {
            server.SendMessage(player, p);
        }

        public void RegisterPrivilege(string p)
        {
        }

        public void RegisterOnBlockUpdate(Action<int, int, int> f)
        {
            server.blockticks.Add(f);
        }

        public bool IsTransparentForLight(int p)
        {
            return server.d_Data.IsTransparentForLight[p];
        }

        public void RegisterWorldGenerator(Action<int, int, int, byte[]> f)
        {
            server.d_Map.getchunk.Add(f);
        }

        public void RegisterOptionBool(string optionname, bool default_)
        {
            modoptions[optionname] = default_;
        }

        Dictionary<string, object> modoptions = new Dictionary<string, object>();

        public int GetChunkSize()
        {
            return Server.chunksize;
        }

        public object GetOption(string optionname)
        {
            return modoptions[optionname];
        }

        public int GetSeed()
        {
            return server.Seed;
        }

        public static int Index3d(int x, int y, int h, int sizex, int sizey)
        {
            return (h * sizey + y) * sizex + x;
        }

        public void RegisterPopulateChunk(Action<int, int, int> f)
        {
            server.populatechunk.Add(f);
        }
    }
}
