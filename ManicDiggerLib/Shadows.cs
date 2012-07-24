using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger
{
    public partial class ManicDiggerGameWindow
    {
        #region IShadows Members

        public void OnLocalBuild(int x, int y, int z)
        {
        }

        public void OnSetBlock(int x, int y, int z)
        {
        }

        public void ResetShadows()
        {
        }

        public void OnGetTerrainBlock(int x, int y, int z)
        {
        }

        public int sunlight_ = 15;
        public int sunlight { get { return sunlight_; } set { sunlight_ = value; } }

        public void OnSetChunk(int x, int y, int z)
        {
        }

        #endregion
    }
}
