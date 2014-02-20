using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using ManicDigger.Collisions;
using System.Drawing;
using System.Threading;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace ManicDigger.Renderers
{
    public class TextureAtlas
    {
        public static RectangleF TextureCoords2d(int textureId, int texturesPacked)
        {
            RectangleF r = new RectangleF();
            r.Y = (1.0f / texturesPacked * (int)(textureId / texturesPacked));
            r.X = (1.0f / texturesPacked * (textureId % texturesPacked));
            r.Width = 1.0f / texturesPacked;
            r.Height =  1.0f / texturesPacked;
            return r;
        }
        public static RectangleF TextureCoords1d(int textureId, int texturesPerAtlas, int tilecount)
        {
            RectangleF r = new RectangleF();
            r.Y = (1.0f / texturesPerAtlas * (int)(textureId % texturesPerAtlas));
            r.X = 0;
            r.Width = 1.0f * tilecount;
            r.Height = 1.0f / texturesPerAtlas;
            return r;
        }
    }
    public class TerrainRenderer
    {
        public static int DistanceSquared(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            int dx = x1 - x2;
            int dy = y1 - y2;
            int dz = z1 - z2;
            return dx * dx + dy * dy + dz * dz;
        }
    }
    public enum TorchType
    {
        Normal, Left, Right, Front, Back
    }
    public interface IBlockRendererTorch
    {
        void AddTorch(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, TorchType type);
    }
    public class BlockRendererTorchDummy : IBlockRendererTorch
    {
        #region IBlockRendererTorch Members
        public void AddTorch(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, TorchType type)
        {
        }
        #endregion
    }
    public class BlockRendererTorch : IBlockRendererTorch
    {
        [Inject]
        public IGameData d_Data;
        [Inject]
        public ITerrainTextures d_TerainRenderer;
        public int TopTexture;
        public int SideTexture;
        public void AddTorch(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, TorchType type)
        {
            Color curcolor = Color.White;
            float torchsizexy = 0.16f;
            float topx = 1f / 2f - torchsizexy / 2f;
            float topy = 1f / 2f - torchsizexy / 2f;
            float bottomx = 1f / 2f - torchsizexy / 2f;
            float bottomy = 1f / 2f - torchsizexy / 2f;

            topx += x;
            topy += y;
            bottomx += x;
            bottomy += y;

            if (type == TorchType.Front) { bottomx = x - torchsizexy; }
            if (type == TorchType.Back) { bottomx = x + 1; }
            if (type == TorchType.Left) { bottomy = y - torchsizexy; }
            if (type == TorchType.Right) { bottomy = y + 1; }

            Vector3 top00 = new Vector3(topx, z + 0.9f, topy);
            Vector3 top01 = new Vector3(topx, z + 0.9f, topy + torchsizexy);
            Vector3 top10 = new Vector3(topx + torchsizexy, z + 0.9f, topy);
            Vector3 top11 = new Vector3(topx + torchsizexy, z + 0.9f, topy + torchsizexy);

            if (type == TorchType.Left)
            {
                top01 += new Vector3(0, -0.1f, 0);
                top11 += new Vector3(0, -0.1f, 0);
            }

            if (type == TorchType.Right)
            {
                top10 += new Vector3(0, -0.1f, 0);
                top00 += new Vector3(0, -0.1f, 0);
            }

            if (type == TorchType.Front)
            {
                top10 += new Vector3(0, -0.1f, 0);
                top11 += new Vector3(0, -0.1f, 0);
            }

            if (type == TorchType.Back)
            {
                top01 += new Vector3(0, -0.1f, 0);
                top00 += new Vector3(0, -0.1f, 0);
            }

            Vector3 bottom00 = new Vector3(bottomx, z + 0, bottomy);
            Vector3 bottom01 = new Vector3(bottomx, z + 0, bottomy + torchsizexy);
            Vector3 bottom10 = new Vector3(bottomx + torchsizexy, z + 0, bottomy);
            Vector3 bottom11 = new Vector3(bottomx + torchsizexy, z + 0, bottomy + torchsizexy);

            //top
            {
                int sidetexture = TopTexture;
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(top00.X, top00.Y, top00.Z, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(top01.X, top01.Y, top01.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top10.X, top10.Y, top10.Z, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(top11.X, top11.Y, top11.Z, texrec.Right, texrec.Bottom, curcolor));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }

            //bottom - same as top, but z is 1 less.
            {
                int sidetexture = SideTexture;
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(bottom00.X, bottom00.Y, bottom00.Z, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom01.X, bottom01.Y, bottom01.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom10.X, bottom10.Y, bottom10.Z, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom11.X, bottom11.Y, bottom11.Z, texrec.Right, texrec.Bottom, curcolor));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }

            //front
            {
                int sidetexture = SideTexture;
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(bottom00.X, bottom00.Y, bottom00.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom01.X, bottom01.Y, bottom01.Z, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top00.X, top00.Y, top00.Z, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(top01.X, top01.Y, top01.Z, texrec.Right, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }

            //back - same as front, but x is 1 greater.
            {
                int sidetexture = SideTexture;
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(bottom10.X, bottom10.Y, bottom10.Z, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom11.X, bottom11.Y, bottom11.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top10.X, top10.Y, top10.Z, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(top11.X, top11.Y, top11.Z, texrec.Left, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }

            {
                int sidetexture = SideTexture;
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(bottom00.X, bottom00.Y, bottom00.Z, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top00.X, top00.Y, top00.Z, texrec.Right, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom10.X, bottom10.Y, bottom10.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top10.X, top10.Y, top10.Z, texrec.Left, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 2));
            }

            //right - same as left, but y is 1 greater.
            {
                int sidetexture = SideTexture;
                RectangleF texrec = TextureAtlas.TextureCoords2d(sidetexture, d_TerainRenderer.texturesPacked());
                short lastelement = (short)myvertices.Count;
                myvertices.Add(new VertexPositionTexture(bottom01.X, bottom01.Y, bottom01.Z, texrec.Left, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top01.X, top01.Y, top01.Z, texrec.Left, texrec.Top, curcolor));
                myvertices.Add(new VertexPositionTexture(bottom11.X, bottom11.Y, bottom11.Z, texrec.Right, texrec.Bottom, curcolor));
                myvertices.Add(new VertexPositionTexture(top11.X, top11.Y, top11.Z, texrec.Right, texrec.Top, curcolor));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 0));
                myelements.Add((ushort)(lastelement + 2));
                myelements.Add((ushort)(lastelement + 3));
                myelements.Add((ushort)(lastelement + 1));
                myelements.Add((ushort)(lastelement + 2));
            }
        }
    }
}
