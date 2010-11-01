using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace ManicDigger.Renderers
{
    public interface IDraw2d
    {
        void Draw2dTexture(int textureid, float x1, float y1, float width, float height, int? inAtlasId, Color color);
        int WhiteTexture();
        void Draw2dText(string text, float x, float y, float fontsize, Color? color);
        void Draw2dTextures(Draw2dData[] todraw, int textureid);
    }
    public class FpsHistoryGraphRenderer
    {
        [Inject]
        public IViewportSize viewportsize { get; set; }
        [Inject]
        public IDraw2d draw { get; set; }
        List<float> m_fpshistory = new List<float>();
        List<float> fpshistory
        {
            get
            {
                while (m_fpshistory.Count < 300)
                {
                    m_fpshistory.Add(0);
                }
                return m_fpshistory;
            }
        }
        public void DrawFpsHistoryGraph()
        {
            float maxtime = 0;
            foreach (var v in fpshistory)
            {
                if (v > maxtime)
                {
                    maxtime = v;
                }
            }
            float historyheight = 80;
            int posx = 25;
            int posy = viewportsize.Height - (int)historyheight - 20;
            Color[] colors = new[] { Color.Black, Color.Red };
            Color linecolor = Color.White;

            Draw2dData[] todraw = new Draw2dData[fpshistory.Count];
            for (int i = 0; i < fpshistory.Count; i++)
            {
                float time = fpshistory[i];
                time = (time * 60) * historyheight;
                Color c = Interpolation.InterpolateColor((float)i / fpshistory.Count, colors);
                todraw[i] = new Draw2dData() { x1 = posx + i, y1 = posy - time, width = 1, height = time, inAtlasId = null, color = c };
            }
            draw.Draw2dTextures(todraw, draw.WhiteTexture());

            draw.Draw2dTexture(draw.WhiteTexture(), posx, posy - historyheight, fpshistory.Count, 1, null, linecolor);
            draw.Draw2dTexture(draw.WhiteTexture(), posx, posy - historyheight * (60f / 75), fpshistory.Count, 1, null, linecolor);
            draw.Draw2dTexture(draw.WhiteTexture(), posx, posy - historyheight * (60f / 30), fpshistory.Count, 1, null, linecolor);
            draw.Draw2dTexture(draw.WhiteTexture(), posx, posy - historyheight * (60f / 150), fpshistory.Count, 1, null, linecolor);
            draw.Draw2dText("60", posx, posy - historyheight * (60f / 60), 6, null);
            draw.Draw2dText("75", posx, posy - historyheight * (60f / 75), 6, null);
            draw.Draw2dText("30", posx, posy - historyheight * (60f / 30), 6, null);
            draw.Draw2dText("150", posx, posy - historyheight * (60f / 150), 6, null);
        }
        public void Update(float dt)
        {
            fpshistory.RemoveAt(0);
            fpshistory.Add(dt);
        }
    }
}
