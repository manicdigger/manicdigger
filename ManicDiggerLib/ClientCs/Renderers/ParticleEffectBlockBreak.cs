using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace ManicDigger.Renderers
{
    public class Particle
    {
        public Vector3 position;
        public Vector3 direction;
    }
    public class ParticleEffectBlockBreak
    {
        public ManicDiggerGameWindow d_Map;
        public ManicDiggerGameWindow d_Shadows;
        public ITerrainTextures d_Terrain;
        public GameData d_Data;
        public void DrawImmediateParticleEffects(double deltaTime)
        {
            GL.BindTexture(TextureTarget.Texture2D, d_Terrain.terrainTexture());
            foreach (ParticleEffect p in new List<ParticleEffect>(particleEffects))
            {
                foreach (Particle pp in p.particles)
                {
                    float l = p.light;
                    GL.Begin(BeginMode.Triangles);
                    RectangleF texrec = TextureAtlas.TextureCoords2d(p.textureid, d_Terrain.texturesPacked());
                    GL.TexCoord2(texrec.Left, texrec.Top);
                    GL.Color3(l, l, l);
                    GL.Vertex3(pp.position);
                    GL.TexCoord2(texrec.Right, texrec.Top);
                    GL.Color3(l, l, l);
                    GL.Vertex3(pp.position + Vector3.Multiply(pp.direction, new Vector3(0, particlesize, particlesize)));
                    GL.TexCoord2(texrec.Right, texrec.Bottom);
                    GL.Color3(l, l, l);
                    GL.Vertex3(pp.position + Vector3.Multiply(pp.direction, new Vector3(particlesize, 0, particlesize)));
                    Vector3 delta = pp.direction;
                    delta = Vector3.Multiply(delta, (float)deltaTime * particlespeed);
                    pp.direction.Y -= (float)deltaTime * particlegravity;
                    pp.position += delta;
                    GL.End();
                }
                if ((DateTime.Now - p.start) >= particletime)
                {
                    particleEffects.Remove(p);
                }
            }
        }
        float particlesize = 0.6f;
        float particlespeed = 5;
        float particlegravity = 2f;
        int particlecount = 20;
        TimeSpan particletime = TimeSpan.FromSeconds(5);
        int maxparticleeffects = 50;
        List<ParticleEffect> particleEffects = new List<ParticleEffect>();
        class ParticleEffect
        {
            public Vector3 center;
            public DateTime start;
            public List<Particle> particles = new List<Particle>();
            public int textureid;
            public float light = 1f;
        }
        Random rnd = new Random();
        public void StartParticleEffect(Vector3 v)
        {
            if (particleEffects.Count >= maxparticleeffects)
            {
                return;
            }
            ParticleEffect p = new ParticleEffect();
            p.center = v + new Vector3(0.5f, 0.5f, 0.5f);
            p.start = DateTime.Now;
            if (!MapUtil.IsValidPos(d_Map, (int)v.X, (int)v.Z, (int)v.Y))
            {
                return;
            }
            int tiletype = d_Map.GetBlock((int)v.X, (int)v.Z, (int)v.Y);
            if (!d_Map.IsValid(tiletype))
            {
                return;
            }
            p.textureid = d_Shadows.game.TextureId[tiletype][(int)TileSide.Top];
            p.light = (float)d_Shadows.MaybeGetLight((int)v.X, (int)v.Z, (int)v.Y) / d_Shadows.maxlight;
            for (int i = 0; i < particlecount; i++)
            {
                Particle pp = new Particle();
                pp.position = p.center;
                pp.direction = new Vector3((float)rnd.NextDouble() - 0.5f,
                    (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
                pp.direction.Normalize();
                p.particles.Add(pp);
            }
            particleEffects.Add(p);
        }
    }
}
