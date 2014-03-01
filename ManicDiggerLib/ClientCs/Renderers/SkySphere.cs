using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace ManicDigger.Renderers
{
    //http://www.opentk.com/node/732
    //Re: [SL Multitexturing] - Only one texture with gluSphere
    //Posted Sunday, 22 March, 2009 - 23:50 by the Fiddler
    public class SkySphere
    {
        public ManicDiggerGameWindow game;
        [Inject]
        public MeshBatcher d_MeshBatcher;
        [Inject]
        public ILocalPlayerPosition d_LocalPlayerPosition;
        [Inject]
        public IThe3d d_The3d;
        public int SkyTexture = -1;
        //int SkyMeshId = -1;
        Model skymodel;
        public void Draw()
        {
            if (SkyTexture == -1)
            {
                throw new InvalidOperationException();
            }
            int size = 1000;
            if (skymodel == null)
            {
                skymodel = game.game.platform.CreateModel(SphereModelData.GetSphereModelData(size, size, 20, 20));
            }
            game.Set3dProjection(size * 2);
            game.GLMatrixModeModelView();
            d_MeshBatcher.BindTexture = false;
            game.GLPushMatrix();
            game.GLTranslate(d_LocalPlayerPosition.LocalPlayerPosition.X,
                d_LocalPlayerPosition.LocalPlayerPosition.Y,
                d_LocalPlayerPosition.LocalPlayerPosition.Z);
            GL.Color3(Color.White);
            GL.BindTexture(TextureTarget.Texture2D, SkyTexture);
            game.game.platform.DrawModel(skymodel);
            game.GLPopMatrix();
            game.Set3dProjection();
        }
    }
}
