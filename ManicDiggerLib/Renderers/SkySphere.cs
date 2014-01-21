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
                skymodel = CreateModel(SphereModelData.GetSphereModelData(size, size, 20, 20));
            }
            game.Set3dProjection(size * 2);
            GL.MatrixMode(MatrixMode.Modelview);
            d_MeshBatcher.BindTexture = false;
            GL.PushMatrix();
            GL.Translate(d_LocalPlayerPosition.LocalPlayerPosition);
            GL.Color3(Color.White);
            GL.BindTexture(TextureTarget.Texture2D, SkyTexture);
            DrawModel(skymodel);
            GL.PopMatrix();
            game.Set3dProjection();
        }

        public Model CreateModel(ModelData data)
        {
            int id = GL.GenLists(1);

            GL.NewList(id, ListMode.Compile);
            
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            float[] dataXyz = data.getXyz();
            float[] dataUv = data.getUv();
            byte[] dataRgba = data.getRgba();
            float[] xyz = new float[data.GetXyzCount()];
            float[] uv = new float[data.GetUvCount()];
            byte[] rgba = new byte[data.GetRgbaCount()];

            for (int i = 0; i < data.GetXyzCount(); i++)
            {
                xyz[i] = dataXyz[i];
            }
            for (int i = 0; i < data.GetUvCount(); i++)
            {
                uv[i] = dataUv[i];
            }
            for (int i = 0; i < data.GetRgbaCount(); i++)
            {
                rgba[i] = dataRgba[i];
            }

            GL.VertexPointer(3, VertexPointerType.Float, 3 * 4, xyz);
            GL.ColorPointer(4, ColorPointerType.UnsignedByte, 4 * 1, data.getRgba());
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 2 * 4, uv);

            BeginMode beginmode = BeginMode.Triangles;
            if (data.getMode() == DrawModeEnum.Triangles)
            {
                beginmode = BeginMode.Triangles;
                GL.Enable(EnableCap.Texture2D);
            }
            else if (data.getMode() == DrawModeEnum.Lines)
            {
                beginmode = BeginMode.Lines;
                GL.Disable(EnableCap.Texture2D);
            }
            else
            {
                throw new Exception();
            }

            var dataIndices = data.getIndices();
            ushort[] indices = new ushort[data.GetIndicesCount()];
            for (int i = 0; i < data.GetIndicesCount(); i++)
            {
                indices[i] = (ushort)dataIndices[i];
            }

            GL.DrawElements(beginmode, data.GetIndicesCount(), DrawElementsType.UnsignedShort, indices);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.Disable(EnableCap.Texture2D);

            GL.EndList();
            DisplayListModel m = new DisplayListModel();
            m.listId = id;
            return m;
        }

        class DisplayListModel : Model
        {
            public int listId;
        }

        public void DrawModel(Model model)
        {
            GL.CallList(((DisplayListModel)model).listId);
        }
    }
}
