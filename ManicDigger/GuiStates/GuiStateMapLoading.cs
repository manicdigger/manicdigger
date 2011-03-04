using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace ManicDigger
{
    public partial class ManicDiggerGameWindow
    {
        MapLoadingProgressEventArgs maploadingprogress;
        private void MapLoadingStart()
        {
            guistate = GuiState.MapLoading;
            freemouse = true;
            maploadingprogress = new MapLoadingProgressEventArgs();
        }
        private void MapLoadingDraw()
        {
            string connecting = "Connecting...";
            if (maploadingprogress.ProgressStatus != null)
            {
                connecting = maploadingprogress.ProgressStatus;
            }
            string progress = string.Format("{0}%\n", maploadingprogress.ProgressPercent);
            string progress1 = string.Format("{0} KB", (maploadingprogress.ProgressBytes / 1024));
            the3d.Draw2dText(network.ServerName, xcenter(the3d.TextSize(network.ServerName, 14).Width), Height / 2 - 150, 14, Color.White);
            the3d.Draw2dText(network.ServerMotd, xcenter(the3d.TextSize(network.ServerMotd, 14).Width), Height / 2 - 100, 14, Color.White);
            the3d.Draw2dText(connecting, xcenter(the3d.TextSize(connecting, 14).Width), Height / 2 - 50, 14, Color.White);
            if (maploadingprogress.ProgressPercent > 0)
            {
                the3d.Draw2dText(progress, xcenter(the3d.TextSize(progress, 14).Width), Height / 2 - 20, 14, Color.White);
                the3d.Draw2dText(progress1, xcenter(the3d.TextSize(progress1, 14).Width), Height / 2 + 10, 14, Color.White);
                //float progressratio = (float)maploadingprogress.ProgressBytes
                //    / ((float)maploadingprogress.ProgressBytes / ((float)maploadingprogress.ProgressPercent / 100));
                float progressratio = (float)maploadingprogress.ProgressPercent / 100;
                int sizex = 400;
                int sizey = 40;
                the3d.Draw2dTexture(the3d.WhiteTexture(), xcenter(sizex), Height / 2 + 70, sizex, sizey, null, Color.Black);
                Color c = Interpolation.InterpolateColor(progressratio, new FastColor(Color.Red), new FastColor(Color.Yellow), new FastColor(Color.Green)).ToColor();
                the3d.Draw2dTexture(the3d.WhiteTexture(), xcenter(sizex), Height / 2 + 70, progressratio * sizex, sizey, null, c);
            }
        }
    }
}
