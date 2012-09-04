using System;
using System.Collections.Generic;
using System.Text;
using ManicDiggerServer;
using ProtoBuf;
using GameModeFortress;
using Jint.Delegates;
using System.Net;

namespace ManicDigger
{
    public class ModManager1 : ModManager
    {
        public void SetBlockType(int id, string name, BlockType block)
        {
            if (block.Sounds == null)
            {
                block.Sounds = defaultSounds;
            }
            server.SetBlockType(id, name, block);
        }

        public void SetBlockType(string name, BlockType block)
        {
            if (block.Sounds == null)
            {
                block.Sounds = defaultSounds;
            }
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

        public void RegisterOnBlockDelete(ManicDigger.Action<int, int, int, int, int> f)
        {
            server.ondelete.Add(f);
        }

        public void RegisterOnBlockUse(ManicDigger.Action<int, int, int, int> f)
        {
            server.onuse.Add(f);
        }

        public void RegisterOnBlockUseWithTool(ManicDigger.Action<int, int, int, int, int> f)
        {
            server.onusewithtool.Add(f);
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
            return server.BlockTypes[blockType].Name;
        }

        public string GetBlockNameAt(int x, int y, int z)
        {
            return GetBlockName(GetBlock(x,y,z));
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

        public void RegisterTimer(ManicDigger.Action a, double interval)
        {
            server.timers[new ManicDigger.Timer() { INTERVAL = interval }] = delegate { a(); };
        }

        public void PlaySoundAt(int posx, int posy, int posz, string sound)
        {
            Vector3i pos = new Vector3i(posx, posy, posz);
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
            Inventory inventory = server.GetPlayerInventory(server.GetClient(player).playername).Inventory;
            
            var item = new Item();
            item.ItemClass = ItemClass.Block;
            item.BlockId = server.d_Data.WhenPlayerPlacesGetsConvertedTo[block];
            server.GetInventoryUtil(inventory).GrabItem(item, 0);
        }

        public bool PlayerHasPrivilege(int player, string p)
        {
            return server.GetClient(player).privileges.Contains(p);
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
            server.GetClient(player).IsInventoryDirty = true;
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

        public void RegisterWorldGenerator(Action<int, int, int, ushort[]> f)
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

        public int Index3d(int x, int y, int h, int sizex, int sizey)
        {
            return (h * sizey + y) * sizex + x;
        }

        public void RegisterPopulateChunk(Action<int, int, int> f)
        {
            server.populatechunk.Add(f);
        }

        public void SetDefaultSounds(SoundSet defaultSounds)
        {
            this.defaultSounds = defaultSounds;
        }
        SoundSet defaultSounds;

        public byte[] GetGlobalData(string name)
        {
            if (server.moddata.ContainsKey(name))
            {
                return server.moddata[name];
            }
            return null;
        }

        public void SetGlobalData(string name, byte[] value)
        {
            server.moddata[name] = value;
        }

        public void RegisterOnLoad(Action f)
        {
            server.onload.Add(f);
        }

        public void RegisterOnSave(Action f)
        {
            server.onsave.Add(f);
        }

        public void RegisterOnCommand(Func<int,string,string,bool> f)
        {
            server.oncommand.Add(f);
        }

        public string GetPlayerIp(int player)
        {
            return ((IPEndPoint)server.GetClient(player).socket.RemoteEndPoint).Address.ToString();
        }

        public string GetPlayerName(int player)
        {
            return server.GetClient(player).playername;
        }

        public List<string> required = new List<string>();

        public void RequireMod(string modname)
        {
            required.Add(modname);
        }

        public void SetGlobalDataNotSaved(string name, object value)
        {
            notsaved[name] = value;
        }

        public object GetGlobalDataNotSaved(string name)
        {
            return notsaved[name];
        }
        Dictionary<string, object> notsaved = new Dictionary<string, object>();

        public void SendMessageToAll(string message)
        {
            server.SendMessageToAll(message);
        }

        public void RegisterCommandHelp(string command, string help)
        {
            server.commandhelps[command] = help;
        }

        public void AddToStartInventory(string blocktype, int amount)
        {
            server.d_Data.StartInventoryAmount[GetBlockId(blocktype)] = amount;
        }

        public long GetCurrentTick()
        {
            return server.SimulationCurrentFrame;
        }

        GameTime t = new GameTime();

        public double GetCurrentYearTotal()
        {
            t.Ticks = server.SimulationCurrentFrame;
            return t.YearTotal;
        }

        public double GetCurrentHourTotal()
        {
            t.Ticks = server.SimulationCurrentFrame;
            return t.HourTotal;
        }

        public void UpdateBlockTypes()
        {
            foreach (var k in server.clients)
            {
                server.SendBlockTypes(k.Key);
            }
        }

        public double GetGameYearRealHours()
        {
            return t.GameYearRealHours;
        }

        public void SetGameYearRealHours(double hours)
        {
            t.GameYearRealHours = hours;
        }

        public double GetGameDayRealHours()
        {
            return t.GameDayRealHours;
        }

        public void SetGameDayRealHours(double hours)
        {
            t.GameDayRealHours = hours;
        }

        public void EnableShadows(bool value)
        {
            server.enableshadows = value;
        }

        public float GetPlayerPositionX(int player)
        {
            return server.GetClient(player).PositionMul32GlX / 32;
        }

        public float GetPlayerPositionY(int player)
        {
            return server.GetClient(player).PositionMul32GlZ / 32;
        }

        public float GetPlayerPositionZ(int player)
        {
            return server.GetClient(player).PositionMul32GlY / 32;
        }

        public void SetPlayerPosition(int player, float x, float y, float z)
        {
            foreach(var k in server.clients)
            {
                server.SendPlayerTeleport(k.Key, player, (int)(x * 32), (int)(z * 32), (int)(y * 32),
                    (byte)server.GetClient(player).positionheading, (byte)server.GetClient(player).positionpitch);
            }
        }

        public int[] AllPlayers()
        {
            List<int> players = new List<int>();
            foreach (var k in server.clients)
            {
                players.Add(k.Key);
            }
            return players.ToArray();
        }

        public void SetPlayerAreaSize(int size)
        {
            server.playerareasize = size;
            server.centerareasize = size / 2;
            server.drawdistance = size / 2;
        }

        public bool IsSinglePlayer()
        {
            return server.IsSinglePlayer;
        }

        public void AddPermissionArea(int x1, int y1, int z1, int x2, int y2, int z2, int permissionLevel)
        {
            AreaConfig area = new AreaConfig();
            area.Level = permissionLevel;
            area.Coords = string.Format("{0},{1},{2},{3},{4},{5}", x1, y1, z1, x2, y2, z2);
            server.config.Areas.Add(area);
            server.SaveConfig();
        }

        public void RemovePermissionArea(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            for (int i = server.config.Areas.Count - 1; i >= 0; i--)
            {
                string coords = string.Format("{0},{1},{2},{3},{4},{5}", x1, y1, z1, x2, y2, z2);
                if (server.config.Areas[i].Coords == coords)
                {
                    server.config.Areas.RemoveAt(i);
                    server.SaveConfig();
                }
            }
        }

        public int GetPlayerPermissionLevel(int playerid)
        {
            return server.clients[playerid].clientGroup.Level;
        }
    }
}
