using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using ManicDigger.Collisions;

namespace GameModeFortress
{
    public class CurrentSeasonDummy : ICurrentSeason
    {
        #region ICurrentSeason Members
        public int CurrentSeason { get { return 0; } }
        #endregion
    }
    public interface ICurrentSeason
    {
        int CurrentSeason { get; }
    }
    public class GameDataTilesManicDigger : IGameData
    {
        public ICurrentSeason CurrentSeason { get; set; }
        public GameDataTilesMinecraft data = new GameDataTilesMinecraft();
        public GameDataTilesManicDigger()
        {
            datanew[(int)TileTypeManicDigger.BrushedMetal] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 0 };
            datanew[(int)TileTypeManicDigger.ChemicalGreen] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 1 };
            datanew[(int)TileTypeManicDigger.Salt] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 2 };
            datanew[(int)TileTypeManicDigger.Roof] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 3 };
            datanew[(int)TileTypeManicDigger.Camouflage] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 4 };
            datanew[(int)TileTypeManicDigger.DirtForFarming] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 5 };
            datanew[(int)TileTypeManicDigger.Apples] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 6 };
            datanew[(int)TileTypeManicDigger.Hay] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 7 };
            datanew[(int)TileTypeManicDigger.Crops1] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 8 };
            datanew[(int)TileTypeManicDigger.Crops2] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 9 };
            datanew[(int)TileTypeManicDigger.Crops3] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 10 };
            datanew[(int)TileTypeManicDigger.Crops4] = new TileTypeData() { Buildable = true, AllTextures = (5 * 16) + 11 };
            datanew[(int)TileTypeManicDigger.CraftingTable] = new TileTypeData() { Buildable = true, AllTextures = (7 * 16) + 0 };
            datanew[(int)TileTypeManicDigger.Minecart] = new TileTypeData() { Buildable = true, AllTextures = (7 * 16) + 1, TextureTop = (7 * 16) + 2 };
            datanew[(int)TileTypeManicDigger.Trampoline] = new TileTypeData() { Buildable = true, AllTextures = (7 * 16) + 9 };
            datanew[(int)TileTypeManicDigger.FillStart] = new TileTypeData() { Buildable = true, AllTextures = (7 * 16) + 10 };
            datanew[(int)TileTypeManicDigger.Cuboid] = new TileTypeData() { Buildable = true, AllTextures = (7 * 16) + 11 };
            datanew[(int)TileTypeManicDigger.FillArea] = new TileTypeData() { Buildable = true, AllTextures = (7 * 16) + 12 };
            datanew[(int)TileTypeMinecraft.Torch] = new TileTypeData() { Buildable = true, AllTextures = (7 * 16) + 13 }; //50
        }
        #region IGameData Members
        public int GetTileTextureId(int tileType, TileSide side)
        {
            if (CurrentSeason.CurrentSeason == 3)
            {
                if (tileType == (int)TileTypeMinecraft.Grass)
                {
                    if (side == TileSide.Top)
                    {
                        return (7 * 16) + 3;
                    }
                    if (side == TileSide.Bottom)
                    {
                        goto standard;
                    }
                    return (7 * 16) + 5;
                }
                if (tileType == (int)TileTypeMinecraft.Water
                    || tileType == (int)TileTypeMinecraft.StationaryWater
                    || tileType == (int)TileTypeMinecraft.InfiniteWaterSource)
                {
                    return (7 * 16) + 4;
                }
                if (tileType == (int)TileTypeMinecraft.Leaves)
                {
                    return (7 * 16) + 6;
                }
            }
        standard:
            if (datanew[tileType] != null)
            {
                if (side == TileSide.Top)
                {
                    return datanew[tileType].TextureTop;
                }
                if (side == TileSide.Bottom)
                {
                    return datanew[tileType].TextureBottom;
                }
                return datanew[tileType].TextureSide;
            }
            if (IsRailTile(tileType))
            {
                //return 1;
                if (side == TileSide.Top)
                {
                    return tileType;
                }
                else
                {
                    return data.GetTileTextureId((int)TileTypeMinecraft.Cobblestone, TileSide.Top);
                }
            }
            return data.GetTileTextureId(tileType, side);
        }
        public byte TileIdEmpty
        {
            get { return data.TileIdEmpty; }
        }
        public byte TileIdGrass
        {
            get { return data.TileIdGrass; }
        }
        public byte TileIdDirt
        {
            get { return data.TileIdDirt; }
        }
        public int[] DefaultMaterialSlots
        {
            get
            {
                var slots = new List<int>();
                slots.Add((int)TileTypeMinecraft.Dirt);
                slots.Add((int)TileTypeMinecraft.Brick);
                slots.Add(railstart + (int)RailDirectionFlags.TwoHorizontalVertical);
                slots.Add(railstart + (int)RailDirectionFlags.Corners);
                slots.Add((int)TileTypeMinecraft.Gravel);
                slots.Add((int)TileTypeMinecraft.Cobblestone);
                slots.Add((int)TileTypeMinecraft.Wood);
                slots.Add((int)TileTypeMinecraft.Leaves);
                slots.Add(railstart + (int)RailDirectionFlags.Horizontal);
                slots.Add(railstart + (int)RailDirectionFlags.Vertical);
                return slots.ToArray();
            }
        }
        public byte GoldTileId
        {
            get { return data.GoldTileId; }
        }
        public int TileIdStone
        {
            get { return data.TileIdStone; }
        }
        public int TileIdWater
        {
            get { return data.TileIdWater; }
        }
        public int TileIdSand
        {
            get { return data.TileIdSand; }
        }
        public bool IsWaterTile(int tiletype)
        {
            if (CurrentSeason.CurrentSeason == 3)
            {
                if (IsRealWater(tiletype))
                {
                    return false;
                }
            }
            if (tiletype == (int)TileTypeManicDigger.FillArea) { return true; }
            return data.IsWaterTile(tiletype);
        }
        private static bool IsRealWater(int tiletype)
        {
            return tiletype == (int)TileTypeMinecraft.Water
                                || tiletype == (int)TileTypeMinecraft.StationaryWater
                                || tiletype == (int)TileTypeMinecraft.InfiniteWaterSource;
        }
        public bool IsBuildableTile(int tiletype)
        {
            if (tiletype == (int)TileTypeMinecraft.Torch)
            {
                return true;
            }
            //temporary
            if (tiletype == railstart + (int)RailDirectionFlags.TwoHorizontalVertical) { return true; }
            if (tiletype == railstart + (int)(RailDirectionFlags.UpLeft | RailDirectionFlags.UpRight |
                RailDirectionFlags.DownLeft | RailDirectionFlags.DownRight)) { return true; }
            if (IsRailTile(tiletype)) { return false; }
            if (datanew[tiletype] != null) { return true; }
            return data.IsValidTileType(tiletype)
                && tiletype != (int)TileTypeMinecraft.Water
                && tiletype != (int)TileTypeMinecraft.InfiniteWaterSource
                && tiletype != (int)TileTypeMinecraft.Lava
                && tiletype != (int)TileTypeMinecraft.InfiniteLavaSource;
        }
        public bool IsValidTileType(int tiletype)
        {
            if (datanew[tiletype] != null) { return true; }
            if (IsRailTile(tiletype)) { return true; }
            return data.IsValidTileType(tiletype);
        }
        public bool IsTransparentTile(int tiletype)
        {
            if (tiletype == (int)TileTypeManicDigger.Crops1) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops2) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops3) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops4) { return true; }
            if (IsRailTile(tiletype)) { return true; }
            if (tiletype == (int)TileTypeManicDigger.FillArea) { return true; }
            return data.IsTransparentTile(tiletype);
        }
        public int PlayerBuildableMaterialType(int p)
        {
            return data.PlayerBuildableMaterialType(p);
        }
        public bool IsBlockFlower(int tiletype)
        {
            if (tiletype == (int)TileTypeManicDigger.Crops1) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops2) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops3) { return true; }
            if (tiletype == (int)TileTypeManicDigger.Crops4) { return true; }
            return data.IsBlockFlower(tiletype);
        }
        #endregion
        #region IGameData Members
        public RailDirectionFlags GetRail(int tiletype)
        {
            if (IsRailTile(tiletype))
            {
                return (RailDirectionFlags)(tiletype - railstart);
            }
            return RailDirectionFlags.None;
        }
        public static bool IsRailTile(int tiletype)
        {
            return tiletype >= railstart && tiletype < railstart + 64;
        }
        #endregion
        public static int railstart = (11 * 16);
        #region IGameData Members
        public int TileIdSingleStairs
        {
            get { return data.TileIdSingleStairs; }
        }
        #endregion
        #region IGameData Members
        public int TileIdSponge
        {
            get { return data.TileIdSponge; }
        }
        #endregion
        #region IGameData Members
        public int GetTileTextureIdForInventory(int tileType)
        {
            if (IsRailTile(tileType))
            {
                return GetTileTextureId(tileType, TileSide.Top);
            }
            if (datanew[tileType] != null)
            {
                return datanew[tileType].TextureSide;
            }
            return data.GetTileTextureIdForInventory(tileType);
        }
        #endregion
        TileTypeData[] datanew = new TileTypeData[256];
        #region IGameData Members
        public string BlockName(int blocktype)
        {
            if (data.IsValidTileType(blocktype))
            {
                return data.BlockName(blocktype);
            }
            if (IsRailTile(blocktype))
            {
                return "Rail";
            }
            return Enum.GetName(typeof(TileTypeManicDigger), blocktype);
        }
        #endregion
        #region IGameData Members
        public bool IsEmptyForPhysics(int blocktype)
        {
            return data.IsEmptyForPhysics(blocktype) || IsCrops(blocktype);
        }
        #endregion
        bool IsCrops(int blocktype)
        {
            return blocktype == (int)TileTypeManicDigger.Crops1
                || blocktype == (int)TileTypeManicDigger.Crops2
                || blocktype == (int)TileTypeManicDigger.Crops3
                || blocktype == (int)TileTypeManicDigger.Crops4;
        }
        #region IGameData Members
        public float BlockWalkSpeed(int blocktype)
        {
            return data.BlockWalkSpeed(blocktype);
        }
        #endregion
        #region IGameData Members
        public bool GrassGrowsUnder(int blocktype)
        {
            return data.GrassGrowsUnder(blocktype) || IsCrops(blocktype);
        }
        #endregion
        #region IGameData Members
        public bool IsSlipperyWalk(int blocktype)
        {
            if (CurrentSeason.CurrentSeason == 3 && IsRealWater(blocktype))
            {
                return true;
            }
            return data.IsSlipperyWalk(blocktype);
        }
        #endregion
        #region IGameData Members
        string[] soundwalksnow = { "walksnow1.wav", "walksnow2.wav", "walksnow3.wav", "walksnow4.wav" };
        public string[] WalkSound(int blocktype)
        {
            if (CurrentSeason.CurrentSeason == 3 &&
                (blocktype == (int)TileTypeMinecraft.Grass
                || blocktype == (int)TileTypeMinecraft.Water))
            {
                return soundwalksnow;
            }
            return data.WalkSound(blocktype);
        }
        #endregion
        #region IGameData Members
        public int TileIdTrampoline { get { return (int)TileTypeManicDigger.Trampoline; } }
        #endregion
        #region IGameData Members
        public bool IsLightEmitting(int blocktype)
        {
            return data.IsLightEmitting(blocktype);
        }
        #endregion
        #region IGameData Members
        public byte TileIdTorch { get { return data.TileIdTorch; } }
        #endregion
        #region IGameData Members
        public int GetLightRadius(int blocktype)
        {
            return data.GetLightRadius(blocktype);
        }
        #endregion
    }
    public enum TileTypeManicDigger
    {
        BrushedMetal = 100,
        ChemicalGreen,
        Salt,
        Roof,
        Camouflage,
        DirtForFarming,
        Apples,
        Hay,
        Crops1,
        Crops2,
        Crops3,
        Crops4,
        CraftingTable,
        Minecart,
        Trampoline,
        FillStart,
        Cuboid,
        FillArea,
    }
}
