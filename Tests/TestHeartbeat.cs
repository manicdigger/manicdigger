using System;
using System.Collections.Generic;
using System.Text;
using GameModeFortress;

namespace ManicDigger.Tests
{
    public class TestHeartbeat : MyGameWindow
    {
        public override void OnLoad(EventArgs e)
        {
            ServerHeartbeat heartbeat = new ServerHeartbeat();
            heartbeat.Name = "Heartbeat test";
            heartbeat.Motd = "Test!";
            heartbeat.Key = Guid.Empty.ToString();
            heartbeat.Players.Add("A");
            heartbeat.SendHeartbeat();
        }
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
        }
    }
}
