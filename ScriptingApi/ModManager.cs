using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using System.Runtime.Serialization;

namespace ManicDigger
{
    public interface ModManager
    {
        int GetMaxBlockTypes();
        void SetBlockType(int id, string name, BlockType block);
        void SetBlockType(string name, BlockType block);
        int GetBlockId(string name);
        void AddToCreativeInventory(string blockType);
        void RegisterOnBlockBuild(ModDelegates.BlockBuild f);
        void RegisterOnBlockDelete(ModDelegates.BlockDelete f);
        void RegisterOnBlockUse(ModDelegates.BlockUse f);
        void RegisterOnBlockUseWithTool(ModDelegates.BlockUseWithTool f);
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
        void PlaySoundAt(int x, int y, int z, string sound, int range);
        int NearestPlayer(int x, int y, int z);
        void GrabBlock(int player, int block);
        void GrabBlocks(int player, int block, int amount);
        bool PlayerHasPrivilege(int player, string p);
        bool IsCreative();
        bool IsBlockFluid(int block);
        void NotifyInventory(int player);
        string colorError();
        void SendMessage(int player, string p);
        void RegisterPrivilege(string p);
        void RegisterOnBlockUpdate(ModDelegates.BlockUpdate f);
        bool IsTransparentForLight(int p);
        void RegisterWorldGenerator(ModDelegates.WorldGenerator f);
        void RegisterOptionBool(string optionname, bool default_);
        int GetChunkSize();
        object GetOption(string optionname);
        int GetSeed();
        int Index3d(int x, int y, int h, int sizex, int sizey);
        void RegisterPopulateChunk(ModDelegates.PopulateChunk f);
        void SetDefaultSounds(SoundSet defaultSounds);
        byte[] GetGlobalData(string name);
        void SetGlobalData(string name, byte[] value);
        void RegisterOnLoad(ManicDigger.Action f);
        void RegisterOnSave(ManicDigger.Action f);
        void RegisterOnCommand(ModDelegates.Command f);
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
        int GetPlayerHeading(int player);
        int GetPlayerPitch(int player);
        int GetPlayerStance(int player);
        void SetPlayerOrientation(int player, int heading, int pitch, int stance);
        int[] AllPlayers();
        void SetPlayerAreaSize(int size);
        bool IsSinglePlayer();
        void AddPermissionArea(int x1, int y1, int z1, int x2, int y2, int z2, int permissionLevel);
        void RemovePermissionArea(int x1, int y1, int z1, int x2, int y2, int z2);
        int GetPlayerPermissionLevel(int playerid);
        void SetCreative(bool creative);
        void SetWorldSize(int x, int y, int z);
        void RegisterOnPlayerJoin(ModDelegates.PlayerJoin a);
        void RegisterOnPlayerLeave(ModDelegates.PlayerLeave a);
        void RegisterOnPlayerDisconnect(ModDelegates.PlayerDisconnect a);
        void RegisterOnPlayerChat(ModDelegates.PlayerChat a);
        int[] GetScreenResolution(int playerid);
        void SendDialog(int player, string id, Dialog dialog);
        void RegisterOnDialogClick(ModDelegates.DialogClick a);
        void SetPlayerModel(int player, string model, string texture);
        void RenderHint(RenderHint hint);
        void EnableFreemove(int playerid, bool enable);
        int GetPlayerHealth(int playerid);
        int GetPlayerMaxHealth(int playerid);
        void SetPlayerHealth(int playerid, int health, int maxhealth);
        int GetPlayerOxygen(int playerid);
        int GetPlayerMaxOxygen(int playerid);
        void SetPlayerOxygen(int playerid, int oxygen, int maxoxygen);
        void RegisterOnWeaponHit(ModDelegates.WeaponHit a);
        void RegisterOnSpecialKey(ModDelegates.SpecialKey1 a);
        float[] GetDefaultSpawnPosition(int player);
        string GetServerName();
        string GetServerMotd();
        float[] MeasureTextSize(string text, DialogFont font);
        string GetServerIp();
        string GetServerPort();
        float GetPlayerPing(int player);
        int AddBot(string name);
        bool IsBot(int player);
        void SetPlayerHeight(int playerid, float eyeheight, float modelheight);
        void DisablePrivilege(string privilege); //todo privileges
        void RegisterChangedActiveMaterialSlot(ModDelegates.ChangedActiveMaterialSlot a);
        Inventory GetInventory(int player);
        int GetActiveMaterialSlot(int player);
        void FollowPlayer(int player, int target, bool tpp);
        void SetPlayerSpectator(int playerid, bool isSpectator);
        BlockType GetBlockType(int block);
        void NotifyAmmo(int playerid, Dictionary<int, int> dictionary);
        void RegisterOnWeaponShot(ModDelegates.WeaponShot a);
        void LogChat(string s);
        void EnableExtraPrivilegeToAll(string privilege, bool enable);
        void LogServerEvent(string serverEvent);
        void RegisterOnLoadWorld(ModDelegates.LoadWorld a);
        void SetWorldDatabaseReadOnly(bool readOnly);
        string CurrentWorld();
        void LoadWorld(string filename);
        string[] GetModPaths();
        void SendExplosion(int targetplayer, float dx, float dy, float dz, bool relativeposition, float range, float time);
        void DisconnectPlayer(int player);
        void DisconnectPlayer(int player, string message);
        string GetGroupColor(int player);
        string GetGroupName(int player);
        void InstallHttpModule(string name, Func<string> description, FragLabs.HTTP.IHttpModule module);
        int GetMaxPlayers();
        ServerClient GetServerClient();
        long TotalReceivedBytes();
        long TotalSentBytes();
        void SetPlayerNameColor(int player, string color);
    }

    public enum SpecialKey
    {
        Respawn,
        SetSpawn,
        TabPlayerList,
        SelectTeam,
    }

    public class ModDelegates
    {
        public delegate void BlockBuild(int player, int x, int y, int z);
        public delegate void BlockDelete(int player, int x, int y, int z, int oldblock);
        public delegate void BlockUse(int player, int x, int y, int z);
        public delegate void BlockUseWithTool(int player, int x, int y, int z, int tool);
        public delegate void BlockUpdate(int x, int y, int z);
        public delegate void WorldGenerator(int x, int y, int z, ushort[] chunk);
        public delegate void PopulateChunk(int x, int y, int z);
        public delegate bool Command(int player, string command, string argument);
        public delegate void PlayerJoin(int player);
        public delegate void PlayerLeave(int player);
        public delegate void PlayerDisconnect(int player);
        public delegate string PlayerChat(int player, string message, bool toteam);
        public delegate void DialogClick(int player, string widgetId);
        public delegate void WeaponHit(int sourcePlayer, int targetPlayer, int block, bool headshot);
        public delegate void WeaponShot(int sourceplayer, int block);
        public delegate void SpecialKey1(int player, SpecialKey key);
        public delegate void ChangedActiveMaterialSlot(int player);
        public delegate void LoadWorld();
    }

    public enum ItemClass
    {
        Block,
        Weapon,
        MainArmor,
        Boots,
        Helmet,
        Gauntlet,
        Shield,
        Other,
    }

    [ProtoContract]
    public class Item
    {
        [ProtoMember(1, IsRequired = false)]
        public ItemClass ItemClass;
        [ProtoMember(2, IsRequired = false)]
        public string ItemId;
        [ProtoMember(3, IsRequired = false)]
        public int BlockId;
        [ProtoMember(4, IsRequired = false)]
        public int BlockCount = 1;
    }

    [ProtoContract]
    public class Inventory
    {
        [OnDeserialized()]
        void OnDeserialized()
        {
            /*
            LeftHand = new Item[10];
            if (LeftHandProto != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (LeftHandProto.ContainsKey(i))
                    {
                        LeftHand[i] = LeftHandProto[i];
                    }
                }
            }
            */
            RightHand = new Item[10];
            if (RightHandProto != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (RightHandProto.ContainsKey(i))
                    {
                        RightHand[i] = RightHandProto[i];
                    }
                }
            }
        }
        [OnSerializing()]
        void OnSerializing()
        {
            Dictionary<int, Item> d;// = new Dictionary<int, Item>();
            /*
            for (int i = 0; i < 10; i++)
            {
                if (LeftHand[i] != null)
                {
                    d[i] = LeftHand[i];
                }
            }
            LeftHandProto = d;
            */
            d = new Dictionary<int, Item>();
            for (int i = 0; i < 10; i++)
            {
                if (RightHand[i] != null)
                {
                    d[i] = RightHand[i];
                }
            }
            RightHandProto = d;
        }
        //dictionary because protobuf-net can't serialize array of nulls.
        //[ProtoMember(1, IsRequired = false)]
        //public Dictionary<int, Item> LeftHandProto;
        [ProtoMember(2, IsRequired = false)]
        public Dictionary<int, Item> RightHandProto;
        //public Item[] LeftHand = new Item[10];
        public Item[] RightHand = new Item[10];
        [ProtoMember(3, IsRequired = false)]
        public Item MainArmor;
        [ProtoMember(4, IsRequired = false)]
        public Item Boots;
        [ProtoMember(5, IsRequired = false)]
        public Item Helmet;
        [ProtoMember(6, IsRequired = false)]
        public Item Gauntlet;
        [ProtoMember(7, IsRequired = false)]
        public Dictionary<ProtoPoint, Item> Items = new Dictionary<ProtoPoint, Item>();
        [ProtoMember(8, IsRequired = false)]
        public Item DragDropItem;
        public void CopyFrom(Inventory inventory)
        {
            //this.LeftHand = inventory.LeftHand;
            this.RightHand = inventory.RightHand;
            this.MainArmor = inventory.MainArmor;
            this.Boots = inventory.Boots;
            this.Helmet = inventory.Helmet;
            this.Gauntlet = inventory.Gauntlet;
            this.Items = inventory.Items;
            this.DragDropItem = inventory.DragDropItem;
        }
        public static Inventory Create()
        {
            Inventory i = new Inventory();
            //i.LeftHand = new Item[10];
            i.RightHand = new Item[10];
            return i;
        }
    }

    [ProtoContract]
    public class ProtoPoint
    {
        [ProtoMember(1, IsRequired = false)]
        public int X;
        [ProtoMember(2, IsRequired = false)]
        public int Y;
        public ProtoPoint()
        {
        }
        public ProtoPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public override bool Equals(object obj)
        {
            ProtoPoint obj2 = obj as ProtoPoint;
            if (obj2 != null)
            {
                return this.X == obj2.X
                    && this.Y == obj2.Y;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return X ^ Y;
        }
    }

    public enum RenderHint
    {
        Fast,
        Nice,
    }

    [ProtoContract]
    public class Dialog
    {
        [ProtoMember(1,IsRequired=false)]
        public Widget[] Widgets;
        [ProtoMember(2, IsRequired = false)]
        public int Width;
        [ProtoMember(3, IsRequired = false)]
        public int Height;
        [ProtoMember(4, IsRequired = false)]
        public bool IsModal;
    }

    [ProtoContract]
    public class DialogFont
    {
        public DialogFont()
        {
        }
        public DialogFont(string FamilyName, float Size, DialogFontStyle FontStyle)
        {
            this.FamilyName = FamilyName;
            this.Size = Size;
            this.FontStyle = FontStyle;
        }
        [ProtoMember(1, IsRequired = false)]
        public string FamilyName = "Verdana";
        [ProtoMember(2, IsRequired = false)]
        public float Size = 11f;
        [ProtoMember(3, IsRequired = false)]
        public DialogFontStyle FontStyle;
    }
    [Flags]
    public enum DialogFontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8,
    }

    [ProtoContract]
    public class Widget
    {
        [ProtoMember(1, IsRequired = false)]
        public string Id;
        [ProtoMember(2, IsRequired = false)]
        public bool Click;
        [ProtoMember(3, IsRequired = false)]
        public int X;
        [ProtoMember(4, IsRequired = false)]
        public int Y;
        [ProtoMember(5, IsRequired = false)]
        public int Width;
        [ProtoMember(6, IsRequired = false)]
        public int Height;
        [ProtoMember(7, IsRequired = false)]
        public string Text;
        [ProtoMember(8, IsRequired = false)]
        public char ClickKey;
        [ProtoMember(9, IsRequired = false)]
        public string Image;
        [ProtoMember(10, IsRequired = false)]
        public int Color = -1; //white
        [ProtoMember(11, IsRequired = false)]
        public DialogFont Font;
        [ProtoMember(12, IsRequired = false)]
        public WidgetType Type;
        public const string SolidImage = "Solid";
        public static Widget MakeSolid(float x, float y, float width, float height, int color)
        {
            Widget w = new Widget();
            w.Type = WidgetType.Image;
            w.Image = SolidImage;
            w.X = (int)x;
            w.Y = (int)y;
            w.Width = (int)width;
            w.Height = (int)height;
            w.Color = color;
            return w;
        }

        public static Widget MakeText(string text, DialogFont Font, float x, float y, int textColor)
        {
            Widget w = new Widget();
            w.Type = WidgetType.Text;
            w.Text = text;
            w.X = (int)x;
            w.Y = (int)y;
            w.Font = Font;
            w.Color = textColor;
            return w;
        }
    }

    public enum WidgetType
    {
        Image,
        Text,
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
        OpenDoorLeft,
        OpenDoorRight,
        ClosedDoor,
        Ladder,
        Fence,
        HalfHeight,
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
        [ProtoMember(5)]
        public string[] Shoot = new string[0];
        [ProtoMember(6)]
        public string[] ShootEnd = new string[0];
        [ProtoMember(7)]
        public string[] Reload = new string[0];
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
        [ProtoMember(21)]
        public string handimage;
        [ProtoMember(22)]
        public bool IsPistol;
        [ProtoMember(23)]
        public int AimRadius;
        [ProtoMember(24)]
        public float Recoil;
        [ProtoMember(25)]
        public float Delay;
        [ProtoMember(26)]
        public float BulletsPerShot;
        [ProtoMember(27)]
        public float WalkSpeedWhenUsed = 1;
        [ProtoMember(28)]
        public bool IronSightsEnabled;
        [ProtoMember(29)]
        public float IronSightsMoveSpeed = 1;
        [ProtoMember(30)]
        public string IronSightsImage;
        [ProtoMember(31)]
        public float IronSightsAimRadius;
        [ProtoMember(32)]
        public float IronSightsFov;
        [ProtoMember(33)]
        public int AmmoMagazine;
        [ProtoMember(34)]
        public int AmmoTotal;
        [ProtoMember(35)]
        public float ReloadDelay;
        [ProtoMember(36)]
        public float ExplosionRange;
        [ProtoMember(37)]
        public float ExplosionTime;
        [ProtoMember(38)]
        public float ProjectileSpeed; // 0 is infinite
        [ProtoMember(39)]
        public bool ProjectileBounce;
        [ProtoMember(40)]
        public float DamageBody;
        [ProtoMember(41)]
        public float DamageHead;
        [ProtoMember(42)]
        public PistolType PistolType;
        [ProtoMember(43)]
        public int DamageToPlayer = 0;
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

        public bool IsFluid()
        {
            return DrawType == DrawType.Fluid;
        }

        public bool IsEmptyForPhysics()
        {
            return (DrawType == DrawType.Ladder)
                || (WalkableType != WalkableType.Solid && WalkableType != WalkableType.Fluid);
        }
    }

    public enum PistolType
    {
        Normal,
        Grenade,
    }

    public delegate void Action();
    public delegate void Action<T1, T2>(T1 t1, T2 t2);
    public delegate void Action<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate void Action<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate void Action<T1, T2, T3, T4, T5>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
    public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
    public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
    public delegate void Action<T>(T obj);

    public delegate TResult Func<TResult>();
    public delegate TResult Func<T1, T2, TResult>(T1 t1, T2 t2);
    public delegate TResult Func<T1, T2, T3, TResult>(T1 t1, T2 t2, T3 t3);
    public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
}