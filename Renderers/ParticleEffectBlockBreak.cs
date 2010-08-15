using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using ManicDigger.Collisions;

namespace ManicDigger.Renderers
{
    public class Particle
    {
        public Vector3 position;
        public Vector3 direction;
    }
    public class ParticleEffectBlockBreak
    {
        [Inject]
        public IMapStorage map { get; set; }
        [Inject]
        public ITerrainRenderer terrain { get; set; }
        [Inject]
        public IGameData data { get; set; }
        public void DrawImmediateParticleEffects(double deltaTime)
        {
            GL.BindTexture(TextureTarget.Texture2D, terrain.terrainTexture);
            foreach (ParticleEffect p in new List<ParticleEffect>(particleEffects))
            {
                foreach (Particle pp in p.particles)
                {
                    GL.Begin(BeginMode.Triangles);
                    RectangleF texrec = TextureAtlas.TextureCoords(p.textureid, terrain.texturesPacked);
                    GL.TexCoord2(texrec.Left, texrec.Top);
                    GL.Vertex3(pp.position);
                    GL.TexCoord2(texrec.Right, texrec.Top);
                    GL.Vertex3(pp.position + Vector3.Multiply(pp.direction, new Vector3(0, particlesize, particlesize)));
                    GL.TexCoord2(texrec.Right, texrec.Bottom);
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
            if (!MapUtil.IsValidPos(map, (int)v.X, (int)v.Z, (int)v.Y))
            {
                return;
            }
            int tiletype = map.GetBlock((int)v.X, (int)v.Z, (int)v.Y);
            if (!data.IsValidTileType(tiletype))
            {
                return;
            }
            p.textureid = data.GetTileTextureId(tiletype, TileSide.Top);
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
