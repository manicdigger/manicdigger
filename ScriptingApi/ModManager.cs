using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ManicDigger
{
    public interface ModManager
    {
        void SetBlockType(int id, string name, BlockType block);
        void SetBlockType(string name, BlockType block);
        int GetBlockId(string name);
        void AddToCreativeInventory(string blockType);
        void RegisterOnBlockBuild(ManicDigger.Action<int, int, int, int> f);
        void RegisterOnBlockDelete(ManicDigger.Action<int, int, int, int, int> f);
        void RegisterOnBlockUse(ManicDigger.Action<int, int, int, int> f);
        void RegisterOnBlockUseWithTool(ManicDigger.Action<int, int, int, int, int> f);
        int GetMapSizeX();
        int GetMapSizeY();
        int GetMapSizeZ();
        int GetBlock(int x, int y, int z);
        string GetBlockName(int blockType);
        string GetBlockNameAt(int x, int y, int z);
        void SetBlock(int x, int y, int z, int tileType);
        void SetSunLevels(int[] sunLevels);
        void SetLightLevels(float[] lightLevels);
        void AddCraftingRecipe(string output, int outputAmount, string Input0, int Input0Amount);
        void AddCraftingRecipe2(string output, int outputAmount, string Input0, int Input0Amount, string Input1, int Input1Amount);
        void AddCraftingRecipe3(string output, int outputAmount, string Input0, int Input0Amount, string Input1, int Input1Amount, string Input2, int Input2Amount);
        void SetString(string language, string text, string translation);
        bool IsValidPos(int x, int y, int z);
        void RegisterTimer(ManicDigger.Action a, double interval);
        void PlaySoundAt(int x, int y, int z, string sound);
        int NearestPlayer(int x, int y, int z);
        void GrabBlock(int player, int block);
        bool PlayerHasPrivilege(int player, string p);
        bool IsCreative();
        bool IsBlockFluid(int block);
        void NotifyInventory(int player);
        string colorError();
        void SendMessage(int player, string p);
        void RegisterPrivilege(string p);
        void RegisterOnBlockUpdate(ManicDigger.Action<int, int, int> f);
        bool IsTransparentForLight(int p);
        void RegisterWorldGenerator(ManicDigger.Action<int, int, int, ushort[]> f);
        void RegisterOptionBool(string optionname, bool default_);
        int GetChunkSize();
        object GetOption(string optionname);
        int GetSeed();
        int Index3d(int x, int y, int h, int sizex, int sizey);
        void RegisterPopulateChunk(ManicDigger.Action<int, int, int> f);
        void SetDefaultSounds(SoundSet defaultSounds);
        byte[] GetGlobalData(string name);
        void SetGlobalData(string name, byte[] value);
        void RegisterOnLoad(ManicDigger.Action f);
        void RegisterOnSave(ManicDigger.Action f);
        void RegisterOnCommand(ManicDigger.Func<int, string, string, bool> f);
        string GetPlayerIp(int player);
        string GetPlayerName(int player);
        void RequireMod(string modname);
        void SetGlobalDataNotSaved(string name, object value);
        object GetGlobalDataNotSaved(string name);
        void SendMessageToAll(string message);
        void RegisterCommandHelp(string command, string help);
        void AddToStartInventory(string blocktype, int amount);
        long GetCurrentTick();
        double GetCurrentYearTotal();
        double GetCurrentHourTotal();
        double GetGameYearRealHours();
        void SetGameYearRealHours(double hours);
        double GetGameDayRealHours();
        void SetGameDayRealHours(double hours);
        void UpdateBlockTypes();
        void EnableShadows(bool value);
        float GetPlayerPositionX(int player);
        float GetPlayerPositionY(int player);
        float GetPlayerPositionZ(int player);
        void SetPlayerPosition(int player, float x, float y, float z);
        int[] AllPlayers();
    }

    public class ModInfo
    {
        public string[] RequiredMods;
    }

    public interface IMod
    {
        void PreStart(ModManager m);
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
        Ladder,
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
        [ProtoMember(19)]
        public bool IsUsable;
        [ProtoMember(20)]
        public bool IsTool;
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

    public delegate void Action();
    public delegate void Action<T1, T2>(T1 t1, T2 t2);
    public delegate void Action<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate void Action<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate void Action<T1, T2, T3, T4, T5>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
    public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
    public delegate void Action<T>(T obj);

    public delegate TResult Func<TResult>();
    public delegate TResult Func<T1, T2, TResult>(T1 t1, T2 t2);
    public delegate TResult Func<T1, T2, T3, TResult>(T1 t1, T2 t2, T3 t3);
}