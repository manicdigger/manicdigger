using System;

namespace ManicDigger.Mods
{
    /// <summary>
    /// This class contains core settings for the Manic Digger server
    /// </summary>
    public class Core : IMod
    {
        public void PreStart(ModManager m) { }
        public void Start(ModManager m)
        {
            //Render hint to send to clients
            m.RenderHint(RenderHint.Fast);
            
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
            02,//0 hour
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            03,
            04,
            05,
            06,
            07,//6 hour
            08,
            09,
            10,
            11,
            12,
            13,
            14,
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
            14,
            13,
            12,
            11,
            10,
            09,//18 hour
            08,
            07,
            06,
            05,
            04,
            03,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
            02,
        };
    }
}
