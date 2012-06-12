using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace ManicDigger.MapTools
{
    public class GroundPhysics
    {
        [Inject]
        public IGameData data;

        public List<ToNotify> blocksToNotify = new List<ToNotify>();
        public struct ToNotify
        {
            public Vector3 pos;
            public int type;
        }

        public void BlockChange(IMapStorage map, int x, int y, int z)
        {
            this.map = map;

            if (IsValidDualPos(x, y, z - 1) && (IsSlideDown(x, y, z, data.BlockIdSand) || IsSlideDown(x, y, z, data.BlockIdGravel)))
            {
                BlockMoveDown(x, y, z - 1, 0);
                BlockChange(map, x, y, z - 1);
            }
            else if (IsValidDualPos(x, y, z) && (IsDestroyOfBase(x, y, z, data.BlockIdSand) || IsDestroyOfBase(x, y, z, data.BlockIdGravel)))
            {
                BlockMoveDown(x, y, z, GetDepth(x, y, z));
                BlockChange(map, x, y, z + 1);
            }
        }

        #region Private Fields

        private IMapStorage map;

        #endregion

        #region Private Methods

        private int GetDepth(int x, int y, int z)
        {
            int startHeight = z;
            while (MapUtil.IsValidPos(this.map, x, y, z) && (IsSoftBlock(this.map.GetBlock(x, y, z)))) 
            {
                z--;
            }

            return (startHeight - z) - 1;
        }

        private bool IsSoftBlock(int blockType)
        {
            if (blockType == SpecialBlockId.Empty)
                return true;
            else if (blockType == data.BlockIdWater)
                return true;
            else if (blockType == 8)
                return true;
            else
                return false;
        }

        private bool IsSlideDown(int x, int y, int z, int blockType)
        {
            return (((IsSoftBlock(this.map.GetBlock(x, y, z - 1)))) && (this.map.GetBlock(x, y, z) == blockType));
        }

        private void BlockMoveDown(int x, int y, int z, int depth)
        {
            this.map.SetBlock(x, y, z - depth, this.map.GetBlock(x, y, z + 1));
            this.blocksToNotify.Add(new ToNotify() { pos = new Vector3(x,y,z - depth), type = this.map.GetBlock(x, y, z + 1) });
            this.map.SetBlock(x, y, z + 1, SpecialBlockId.Empty);
            this.blocksToNotify.Add(new ToNotify() { pos = new Vector3(x,y,z + 1), type = SpecialBlockId.Empty });
        }

        private bool IsDestroyOfBase(int x, int y, int z, int blockType)
        {
            return (IsSoftBlock((this.map.GetBlock(x, y, z))) && (this.map.GetBlock(x, y, z + 1) == blockType));
        }

        private bool IsValidDualPos(int x, int y, int z)
        {
            return MapUtil.IsValidPos(this.map, x, y, z) && MapUtil.IsValidPos(this.map, x, y, z + 1);
        }

        #endregion
    }
}
