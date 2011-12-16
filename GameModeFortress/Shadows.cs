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

        public void OnMakeChunk(int chunkx, int chunky, int chunkz)
        {
        }

        public int sunlight
        {
            get
            {
                return 15;
            }
            set
            {
            }
        }

        public void OnSetChunk(int x, int y, int z)
        {
        }

        #endregion
    }
}
