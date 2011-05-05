using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

namespace ManicDigger.Tests
{
    public class TestAudio : MyGameWindow
    {
        public IGetFileStream getfile;
        public override void OnLoad(EventArgs e)
        {
            var audio = new AudioOpenAl();
            audio.d_GameExit = new GameExitDummy();
            audio.d_GetFile = getfile;
            this.audio = audio;
        }
        public IAudio audio;
        double timeaccum;
        int i;
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            timeaccum += e.Time;
            double every = 0.2;
            while (timeaccum >= every)
            {
                audio.Play("rail" + ((++i % 4) + 1) + ".wav");
                timeaccum -= every;
            }
        }
    }
}
