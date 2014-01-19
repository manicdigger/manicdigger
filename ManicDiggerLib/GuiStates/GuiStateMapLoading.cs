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
            Draw2dBitmapFile(Path.Combine("gui", "background.png"), 0, 0, 1024 * ((float)Width / 800), 1024 * ((float)Height / 600));
            string connecting = Language.Connecting;
            if (maploadingprogress.ProgressStatus != null)
            {
                connecting = maploadingprogress.ProgressStatus;
            }
            string progress = string.Format(Language.ConnectingProgressPercent, maploadingprogress.ProgressPercent);
            string progress1 = string.Format(Language.ConnectingProgressKilobytes, (maploadingprogress.ProgressBytes / 1024));
            Draw2dText(this.ServerInfo.ServerName, xcenter(TextSize(this.ServerInfo.ServerName, 14).Width), Height / 2 - 150, 14, Color.White);
            Draw2dText(this.ServerInfo.ServerMotd, xcenter(TextSize(this.ServerInfo.ServerMotd, 14).Width), Height / 2 - 100, 14, Color.White);
            Draw2dText(connecting, xcenter(TextSize(connecting, 14).Width), Height / 2 - 50, 14, Color.White);
            if (maploadingprogress.ProgressPercent > 0)
            {
                Draw2dText(progress, xcenter(TextSize(progress, 14).Width), Height / 2 - 20, 14, Color.White);
                Draw2dText(progress1, xcenter(TextSize(progress1, 14).Width), Height / 2 + 10, 14, Color.White);
                //float progressratio = (float)maploadingprogress.ProgressBytes
                //    / ((float)maploadingprogress.ProgressBytes / ((float)maploadingprogress.ProgressPercent / 100));
                float progressratio = (float)maploadingprogress.ProgressPercent / 100;
                int sizex = 400;
                int sizey = 40;
                Draw2dTexture(WhiteTexture(), xcenter(sizex), Height / 2 + 70, sizex, sizey, null, Color.Black);
                Color c = Interpolation.InterpolateColor(progressratio, new FastColor(Color.Red), new FastColor(Color.Yellow), new FastColor(Color.Green)).ToColor();
                Draw2dTexture(WhiteTexture(), xcenter(sizex), Height / 2 + 70, progressratio * sizex, sizey, null, c);
            }
        }
    }
}
