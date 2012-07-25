using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using ManicDigger.Collisions;
using ManicDigger.Hud;

namespace ManicDigger.Renderers
{
    public class WeaponBlockInfo
    {
        [Inject]
        public ITerrainTextures d_Terrain;
        [Inject]
        public IActiveMaterial d_Viewport;
        [Inject]
        public ILocalPlayerPosition d_LocalPlayerPosition;
        [Inject]
        public IGameData d_Data;
        [Inject]
        public IMapStorage d_Map;
        [Inject]
        public IShadowsGetLight d_Shadows;
        [Inject]
        public Inventory d_Inventory;
        public int terrainTexture { get { return d_Terrain.terrainTexture; } }
        public int texturesPacked { get { return d_Terrain.texturesPacked; } }
        public int GetWeaponTextureId(TileSide side)
        {
            Item item = d_Inventory.RightHand[d_Viewport.ActiveMaterial];
            if (item == null || IsCompass())
            {
                //empty hand
                if (side == TileSide.Top) { return d_Data.TextureId[d_Data.BlockIdEmptyHand, (int)TileSide.Top]; }
                return d_Data.TextureId[d_Data.BlockIdEmptyHand, (int)TileSide.Front];
            }
            if (item.ItemClass == ItemClass.Block)
            {
                return d_Data.TextureId[item.BlockId, (int)side];
            }
            else
            {
                //todo
                return 0;
            }
        }
        public float Light
        {
            get
            {
                Vector3 pos = d_LocalPlayerPosition.LocalPlayerPosition;
                //if ((int)pos.X >= 0 && (int)pos.Y >= 0 && (int)pos.Z >= 0
                //    && (int)pos.X < d_Map.MapSizeX
                //    && (int)pos.Z < d_Map.MapSizeY
                //    && (int)pos.Y < d_Map.MapSizeZ)
                try
                {
                    int? light = d_Shadows.MaybeGetLight((int)pos.X, (int)pos.Z, (int)pos.Y);
                    if (light == null) { light = d_Shadows.maxlight; }
                    return (float)light.Value / d_Shadows.maxlight;
                }
                catch
                {
                    return 1f / d_Shadows.maxlight;
                }
            }
        }
        public bool IsTorch()
        {
            Item item = d_Inventory.RightHand[d_Viewport.ActiveMaterial];
            return item != null
                && item.ItemClass == ItemClass.Block
                && item.BlockId == d_Data.BlockIdTorch;
        }
        public bool IsCompass()
        {
            Item item = d_Inventory.RightHand[d_Viewport.ActiveMaterial];
            return item != null
                && item.ItemClass == ItemClass.Block
                && item.BlockId == d_Data.BlockIdCompass;
        }
        public bool IsEmptyHand()
        {
            Item item = d_Inventory.RightHand[d_Viewport.ActiveMaterial];
            return item == null;
        }
    }
    public class WeaponRenderer
    {
        [Inject]
        public WeaponBlockInfo d_Info;
        [Inject]
        public BlockRendererTorch d_BlockRendererTorch;
        [Inject]
        public ILocalPlayerPosition d_LocalPlayerPosition;
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
        List<ushort> myelements;
        List<VertexPositionTexture> myvertices;
        int oldMaterial;
        float oldLight;
        float slowdownTimer;
        public void DrawWeapon(float dt)
        {
            int light;
            if (d_Info.IsTorch())
            {
                light = 255;
            }
            else
            {
                light = (int)(d_Info.Light * 256);
                if (light > 255) { light = 255; }
                if (light < 0) { light = 0; }
            }
            GL.Color3(Color.FromArgb(light, light, light));
            GL.BindTexture(TextureTarget.Texture2D, d_Info.terrainTexture);

            Item item = d_Info.d_Inventory.RightHand[d_Info.d_Viewport.ActiveMaterial];
            int curmaterial;
            if (item == null)
            {
                curmaterial = 0;
            }
            else
            {
            	curmaterial = item.BlockId == 151 ? 128 : item.BlockId;
            }
            float curlight = d_Info.Light;
            if (curmaterial != oldMaterial || curlight != oldLight || myelements == null)
            {
                myelements = new List<ushort>();
                myvertices = new List<VertexPositionTexture>();
                int x = 0;
                int y = 0;
                int z = 0;
                if (d_Info.IsEmptyHand() || d_Info.IsCompass())
                {
                    d_BlockRendererTorch.TopTexture = d_Info.GetWeaponTextureId(TileSide.Top);
                    d_BlockRendererTorch.SideTexture = d_Info.GetWeaponTextureId(TileSide.Front);
                    d_BlockRendererTorch.AddTorch(myelements, myvertices, x, y, z, TorchType.Normal);
                }
                else if (d_Info.IsTorch())
                {
                    d_BlockRendererTorch.TopTexture = d_Info.GetWeaponTextureId(TileSide.Top);
                    d_BlockRendererTorch.SideTexture = d_Info.GetWeaponTextureId(TileSide.Front);
                    d_BlockRendererTorch.AddTorch(myelements, myvertices, x, y, z, TorchType.Normal);
                }
                else
                {
                    DrawCube(myelements, myvertices, x, y, z);
                }
            }
            oldMaterial = curmaterial;
            oldLight = curlight;
            for (int i = 0; i < myvertices.Count; i++)
            {
                var v = myvertices[i];
                myvertices[i] = v;
            }
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Translate(0.3 + zzzposz - attackt * 5, -1.5f + zzzposx - buildt * 10, -1.5f + zzzposy);
            GL.Rotate(30 + (zzzx) - attackt * 300, new Vector3(1, 0, 0));
            GL.Rotate(60 + zzzy, new Vector3(0, 1, 0));
            GL.Scale(0.8, 0.8, 0.8);

            bool move = oldplayerpos != d_LocalPlayerPosition.LocalPlayerPosition;
            oldplayerpos = d_LocalPlayerPosition.LocalPlayerPosition;
            if (move)
            {
                t += dt;
                slowdownTimer = float.MaxValue;
            }
            else
            {
                if (slowdownTimer == float.MaxValue)
                {
                    slowdownTimer = (float)(animperiod / 2 - (t % (animperiod / 2)));
                }
                slowdownTimer -= dt;
                if (slowdownTimer < 0)
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
            GL.BindTexture(TextureTarget.Texture2D, d_Info.terrainTexture);
            GL.Enable(EnableCap.Texture2D);
            for (int i = 0; i < myelements.Count; i++)
            {
                GL.TexCoord2(myvertices[myelements[i]].u, myvertices[myelements[i]].v);
                GL.Vertex3(myvertices[myelements[i]].Position);
            }
            GL.End();
            GL.PopMatrix();
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
                int sidetexture = d_Info.GetWeaponTextureId(TileSide.Top);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_Info.texturesPacked);
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
                int sidetexture = d_Info.GetWeaponTextureId(TileSide.Bottom);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_Info.texturesPacked);
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
                int sidetexture = d_Info.GetWeaponTextureId(TileSide.Front);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_Info.texturesPacked);
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
                int sidetexture = d_Info.GetWeaponTextureId(TileSide.Back);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_Info.texturesPacked);
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
                int sidetexture = d_Info.GetWeaponTextureId(TileSide.Left);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_Info.texturesPacked);
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
                int sidetexture = d_Info.GetWeaponTextureId(TileSide.Right);
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_Info.texturesPacked);
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
