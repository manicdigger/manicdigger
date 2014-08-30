using System;

namespace ManicDigger.Mods
{
    /// <summary>
    /// This class contains core settings for the Manic Digger server. Adapted for War Mod (light levels, render hint)
    /// </summary>
    public class CoreWar : IMod
    {
        public void PreStart(ModManager m) { }
        public void Start(ModManager m)
        {
            //Render hint to send to clients
            m.RenderHint(RenderHint.Nice);
            
            //Different serverside view distance if singleplayer
            if (m.IsSinglePlayer())
            {
                m.SetPlayerAreaSize(512);
            }
            else
            {
                m.SetPlayerAreaSize(256);
            }
            
            //Set up server time
            m.SetGameDayRealHours(1);
            m.SetGameYearRealHours(24);
            
            //Set up day/night cycle
            m.SetSunLevels(sunLevels);
            m.SetLightLevels(lightLevels);
        }
        
        float[] lightLevels = new float[]
        {
            0f,
            0.0666666667f,
            0.1333333333f,
            0.2f,
            0.2666666667f,
            0.3333333333f,
            0.4f,
            0.4666666667f,
            0.5333333333f,
            0.6f,
            0.6666666667f,
            0.7333333333f,
            0.8f,
            0.8666666667f,
            0.9333333333f,
            1f,
        };
        
        int[] sunLevels = new int[]
        {
            15,//0 hour
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,//6 hour
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,//12 hour
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,//18 hour
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
        };
    }
}
