using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger.Collisions;

namespace ManicDigger
{
    public class GameDataTilesManicDigger : IGameData
    {
        public int GetTileTextureId(int tileType, TileSide side)
        {
            TileTypesManicDigger tt = (TileTypesManicDigger)tileType;
            if (tt == TileTypesManicDigger.Grass)
            {
                if (side == TileSide.Top) { return 0; }
                if (side == TileSide.Bottom) { return 1; }
                return 2;
            }
            if (tt == TileTypesManicDigger.Wall) { return 2; }
            if (tt == TileTypesManicDigger.Dirt) { return 3; }
            if (tt == TileTypesManicDigger.Gold) { return 4; }
            //if ((int)tt < 3) { return (int)tt - 1; }
            return 255;
        }
        public byte TileIdEmpty { get { return (int)TileTypesManicDigger.Empty; } }
        public byte TileIdGrass { get { return (int)TileTypesManicDigger.Grass; } }
        public byte TileIdDirt { get { return (int)TileTypesManicDigger.Dirt; } }
        public int[] DefaultMaterialSlots
        {
            get
            {
                int[] m = new int[10];
                for (int i = 0; i < 10; i++)
                {
                    m[i] = (i + 1);
                }
                return m;
            }
        }
        public byte GoldTileId { get { return (int)TileTypesManicDigger.Gold; } }
        #region IGameData Members
        public int TileIdStone
        {
            get { return TileIdDirt; }//todo
        }
        public int TileIdWater
        {
            get { return TileIdDirt; }//todo
        }
        public int TileIdSand
        {
            get { return TileIdDirt; }//todo
        }
        public bool IsWaterTile(int tiletype)
        {
            return tiletype == TileIdWater;
        }
        #endregion
        #region IGameData Members
        public bool IsBuildableTile(int tiletype)
        {
            return tiletype != TileIdWater;
        }
        #endregion
        #region IGameData Members
        public bool IsValidTileType(int tiletype)
        {
            return tiletype < (int)TileTypesManicDigger.Count;
        }
        #endregion
    }
    public enum TileTypesManicDigger
    {
        Empty,
        Grass,
        Floor,
        Wall,
        Dirt,
        Gold,
        Count,
    }
}
