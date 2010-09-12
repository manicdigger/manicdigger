using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using ManicDigger.Collisions;

namespace ManicDigger
{
    public class WeaponBlockInfo
    {
        [Inject]
        public ITerrainRenderer terrain { get; set; }
        [Inject]
        public IViewport3d viewport { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public IMapStorage map { get; set; }
        public IShadows shadows { get; set; }
        public int terrainTexture { get { return terrain.terrainTexture; } }
        public int texturesPacked { get { return terrain.texturesPacked; } }
        public int GetWeaponTextureId(TileSide side)
        {
            return data.GetTileTextureId(viewport.MaterialSlots[viewport.activematerial], side);
        }
        public float Light
        {
            get
            {
                Vector3 pos = viewport.LocalPlayerPosition;
                if ((int)pos.X >= 0 && (int)pos.Y >= 0 && (int)pos.Z >= 0
                    && (int)pos.X < map.MapSizeX
                    && (int)pos.Z < map.MapSizeY
                    && (int)pos.Y < map.MapSizeZ)
                {
                    int? light = shadows.MaybeGetLight((int)pos.X, (int)pos.Z, (int)pos.Y);
                    if (light == null) { light = shadows.maxlight; }
                    return (float)light.Value / shadows.maxlight;
                }
                return 1;
            }
        }
        public bool IsTorch() { return viewport.MaterialSlots[viewport.activematerial] == data.TileIdTorch; }
    }
    public class WeaponRenderer
    {
        [Inject]
        public WeaponBlockInfo info { get; set; }
        [Inject]
        public IBlockDrawerTorch blockdrawertorch { get; set; }
        //[Inject]
        //public IKeyboard keyboard { get; set; }
        [Inject]
        public ILocalPlayerPosition playerpos { get; set; }
        public void SetAttack(bool isattack, bool build)
        {
            this.build = build;
            if (isattack)
            {
                if (attack == -1)
                {
                    attack = 0;
                }
            }
            else
            {
                attack = -1;
            }
        }
        float attack = -1;
        bool build = false;
        public void DrawWeapon(float dt)
        {
            int light;
            if (info.IsTorch())
            {
                light = 255;
            }
            else
            {
                light = (int)(info.Light * 256);
                if (light > 255) { light = 255; }
                if (light < 0) { light = 0; }
            }
            GL.Color3(Color.FromArgb(light, light, light));
            GL.BindTexture(TextureTarget.Texture2D, info.terrainTexture);
            List<ushort> myelements = new List<ushort>();
            List<VertexPositionTexture> myvertices = new List<VertexPositionTexture>();
            int x = 0;
            int y = 0;
            int z = 0;
            if (info.IsTorch())
            {
                blockdrawertorch.AddTorch(myelements, myvertices, x, y, z, TorchType.Normal);
            }
            else
            {
                DrawCube(myelements, myvertices, x, y, z);
            }
            for (int i = 0; i < myvertices.Count; i++)
            {
                var v = myvertices[i];
                //v.Position += new Vector3(-0.5f, 0, -0.5f);
                //v.Position += new Vector3(2, 2, 2);
                /*
                Matrix4 m2;
                Matrix4.CreateRotationY(0.9f, out m2);
                v.Position = Vector3.TransformVector(v.Position, m2);

                Matrix4 m3;
                Matrix4.CreateRotationX(0.3f, out m3);
                v.Position = Vector3.TransformVector(v.Position, m3);
                */

                //Matrix4 m;
                //Matrix4.CreateRotationY(-player.playerorientation.Y, out m);
                //v.Position = Vector3.TransformPosition(v.Position, m);

                ////Matrix4.CreateRotationX(player.playerorientation.X, out m);
                ////v.Position = Vector3.TransformPosition(v.Position, m);

                //v.Position += new Vector3(0, -0.2f, 0);
                //v.Position += player.playerposition;
                //v.Position += toVectorInFixedSystem1(0.7f, 0, 1, player.playerorientation.X, player.playerorientation.Y);
                myvertices[i] = v;
            }
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Translate(0.3 + zzzposz - attackt * 5, -1.5f + zzzposx - buildt * 10, -1.5f + zzzposy);
            //GL.Scale(2, 2, 2);
            GL.Rotate(30 + (zzzx) - attackt * 300, new Vector3(1, 0, 0));//zzz += 0.01f
            GL.Rotate(60 + zzzy, new Vector3(0, 1, 0));
            GL.Scale(0.8, 0.8, 0.8);
            //GL.Rotate(0-(zzz+=0.05f), new Vector3(0, 1, 0));
            //GL.Translate(0, -2, 0);


            //if (keyboard.keyboardstate[OpenTK.Input.Key.Left]) zzzx += -0.1f;
            //if (keyboard.keyboardstate[OpenTK.Input.Key.Right]) zzzx += 0.1f;
            //if (keyboard.keyboardstate[OpenTK.Input.Key.Up]) zzzy += 0.1f;
            //if (keyboard.keyboardstate[OpenTK.Input.Key.Down]) zzzy += -0.1f;
            //if (keyboard.keyboardstate[OpenTK.Input.Key.Keypad4]) zzzposx += -0.1f;
            //if (keyboard.keyboardstate[OpenTK.Input.Key.Keypad6]) zzzposx += 0.1f;
            //if (keyboard.keyboardstate[OpenTK.Input.Key.Keypad8]) zzzposz += 0.1f;
            //if (keyboard.keyboardstate[OpenTK.Input.Key.Keypad2]) zzzposz += -0.1f;

            bool move = oldplayerpos != playerpos.LocalPlayerPosition;
            oldplayerpos = playerpos.LocalPlayerPosition;
            if (move)
            {
                t += dt;
            }
            else
            {
                float f = CharacterRendererMonsterCode.Normalize(t, (float)animperiod / 2);
                if (Math.Abs(f) < 0.02f)
                {
                    t = 0;
                }
                else
                {
                    t += dt;
                }
            }
            zzzposx = rot(t);
            zzzposz = rot2(t);
            if (attack != -1)
            {
                attack += dt * 7;
                if (attack > Math.PI / 2)
                {
                    attack = -1;
                    if (build)
                    {
                        buildt = 0;
                    }
                    else
                    {
                        attackt = 0;
                    }
                }
                else
                {
                    if (build)
                    {
                        buildt = rot(attack / 5);
                        attackt = 0;
                    }
                    else
                    {
                        attackt = rot(attack / 5);
                        buildt = 0;
                    }
                }
            }

            GL.Begin(BeginMode.Triangles);
            GL.BindTexture(TextureTarget.Texture2D, info.terrainTexture);
            GL.Enable(EnableCap.Texture2D);
            for (int i = 0; i < myelements.Count; i++)
            {
                GL.TexCoord2(myvertices[myelements[i]].u, myvertices[myelements[i]].v);
                GL.Vertex3(myvertices[myelements[i]].Position);
            }
            GL.End();
            GL.PopMatrix();
            //Console.WriteLine("({0}||{1}):({2}||{3})", zzzx, zzzy, zzzposx, zzzposy);
            //(-19,00004||-13,70002):(-0,2000001||-1,3)
        }
        float attackt = 0;
        float buildt;
        float range = 0.07f;
        const float speed = 5;
        float animperiod = (float)Math.PI / (speed / 2);
        Vector3 oldplayerpos;
        float zzzposz;
        float t = 0;
        float rot(float t)
        {
            return (float)Math.Sin(t * 2 * speed) * range;
        }
        float rot2(float t)
        {
            return (float)Math.Sin((t + Math.PI) * speed) * range;
        }
        private void DrawCube(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z)
        {
            //top
            //if (drawtop)
            {
                int sidetexture = info.GetWeaponTextureId(TileSide.Top);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, info.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z + 1.0f, y + 0.0f, texrec.Left, texrec.Top));
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z + 1.0f, y + 1.0f, texrec.Left, texrec.Bottom));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z + 1.0f, y + 0.0f, texrec.Right, texrec.Top));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z + 1.0f, y + 1.0f, texrec.Right, texrec.Bottom));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }
            //bottom - same as top, but z is 1 less.
            //if (drawbottom)
            {
                int sidetexture = info.GetWeaponTextureId(TileSide.Bottom);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, info.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 0.0f, texrec.Left, texrec.Top));
                myvertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 1.0f, texrec.Left, texrec.Bottom));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 0.0f, texrec.Right, texrec.Top));
                myvertices.Add(new VertexPositionTexture(x + 1.0f, z, y + 1.0f, texrec.Right, texrec.Bottom));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }
            ////front
            //if (drawfront)
            {
                int sidetexture = info.GetWeaponTextureId(TileSide.Front);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, info.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0, texrec.Left, texrec.Bottom));
                myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1, texrec.Right, texrec.Bottom));
                myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 0, texrec.Left, texrec.Top));
                myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 1, texrec.Right, texrec.Top));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }
            //back - same as front, but x is 1 greater.
            //if (drawback)
            {//todo fix tcoords
                int sidetexture = info.GetWeaponTextureId(TileSide.Back);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, info.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0, texrec.Left, texrec.Bottom));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 1, texrec.Right, texrec.Bottom));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 0, texrec.Left, texrec.Top));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 1, texrec.Right, texrec.Top));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }
            //if (drawleft)
            {
                int sidetexture = info.GetWeaponTextureId(TileSide.Left);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, info.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0, texrec.Left, texrec.Bottom));
                myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 0, texrec.Left, texrec.Top));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 0, texrec.Right, texrec.Bottom));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 0, texrec.Right, texrec.Top));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }
            //right - same as left, but y is 1 greater.
            //if (drawright)
            {//todo fix tcoords
                int sidetexture = info.GetWeaponTextureId(TileSide.Right);
                RectangleF texrec = TextureAtlas.TextureCoords(sidetexture, info.texturesPacked);
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1, texrec.Left, texrec.Bottom));
                myvertices.Add(new VertexPositionTexture(x + 0, z + 1, y + 1, texrec.Left, texrec.Top));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 0, y + 1, texrec.Right, texrec.Bottom));
                myvertices.Add(new VertexPositionTexture(x + 1, z + 1, y + 1, texrec.Right, texrec.Top));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }
        }
        float zzzx = -27;
        float zzzy = -13.7f;
        float zzzposx = -0.2f;
        float zzzposy = -0.4f;
        float attackprogress = 0;
    }
}
