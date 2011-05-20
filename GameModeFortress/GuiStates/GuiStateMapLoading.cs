using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace ManicDigger
{
    public partial class ManicDiggerGameWindow
    {
        MapLoadingProgressEventArgs maploadingprogress;
        private void MapLoadingStart()
        {
            guistate = GuiState.MapLoading;
            FreeMouse = true;
            maploadingprogress = new MapLoadingProgressEventArgs();
        }
        private void MapLoadingDraw()
        {
            d_The3d.Draw2dBitmapFile(Path.Combine("gui", "background.png"), 0, 0, 1024 * ((float)Width / 800), 1024 * ((float)Height / 600));
            string connecting = "Connecting...";
            if (maploadingprogress.ProgressStatus != null)
            {
                connecting = maploadingprogress.ProgressStatus;
            }
            string progress = string.Format("{0}%\n", maploadingprogress.ProgressPercent);
            string progress1 = string.Format("{0} KB", (maploadingprogress.ProgressBytes / 1024));
            d_The3d.Draw2dText(ServerName, xcenter(d_The3d.TextSize(ServerName, 14).Width), Height / 2 - 150, 14, Color.White);
            d_The3d.Draw2dText(ServerMotd, xcenter(d_The3d.TextSize(ServerMotd, 14).Width), Height / 2 - 100, 14, Color.White);
            d_The3d.Draw2dText(connecting, xcenter(d_The3d.TextSize(connecting, 14).Width), Height / 2 - 50, 14, Color.White);
            if (maploadingprogress.ProgressPercent > 0)
            {
                d_The3d.Draw2dText(progress, xcenter(d_The3d.TextSize(progress, 14).Width), Height / 2 - 20, 14, Color.White);
                d_The3d.Draw2dText(progress1, xcenter(d_The3d.TextSize(progress1, 14).Width), Height / 2 + 10, 14, Color.White);
                //float progressratio = (float)maploadingprogress.ProgressBytes
                //    / ((float)maploadingprogress.ProgressBytes / ((float)maploadingprogress.ProgressPercent / 100));
                float progressratio = (float)maploadingprogress.ProgressPercent / 100;
                int sizex = 400;
                int sizey = 40;
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), xcenter(sizex), Height / 2 + 70, sizex, sizey, null, Color.Black);
                Color c = Interpolation.InterpolateColor(progressratio, new FastColor(Color.Red), new FastColor(Color.Yellow), new FastColor(Color.Green)).ToColor();
                d_The3d.Draw2dTexture(d_The3d.WhiteTexture(), xcenter(sizex), Height / 2 + 70, progressratio * sizex, sizey, null, c);
            }
        }
    }
}
