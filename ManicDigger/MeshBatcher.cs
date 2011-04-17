using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ManicDigger
{
    public class MeshBatcher
    {
        [Inject]
        public IFrustumCulling d_FrustumCulling;
        public MeshBatcher()
        {
        }
        private void GenLists()
        {
            int lists = GL.GenLists(listincrease);
            if (lists == 0)
            {
                throw new Exception();
            }
            this.lists.Add(lists);
        }
        public int listincrease = 1024;
        private int GetList(int i)
        {
            return lists[i / listincrease] + i % listincrease;
        }
        private void ClearLists()
        {
            if (lists != null)
            {
                foreach (int l in lists)
                {
                    GL.DeleteLists(l, listincrease);
                }
            }
        }
        List<int> lists = new List<int>();
        int count = 0;
        public void Remove(int p)
        {
            GetListInfo(p).empty = true;
            empty[p] = 0;
        }
        struct ToAdd
        {
            public ushort[] indices;
            public int indicesCount;
            public VertexPositionTexture[] vertices;
            public int verticesCount;
            public int id;
            public bool transparent;
            public int texture;
            public Vector3 center;
            public float radius;
        }
        Queue<ToAdd> toadd = new Queue<ToAdd>();
        public bool BindTexture = true;
        //Just saves provided arguments.
        //Display lists are created later, in Draw() called from the main thread.
        public int Add(ushort[] indices, int indicesCount, VertexPositionTexture[] vertices, int verticesCount, bool transparent, int texture, Vector3 center, float radius)
        {
            int id;
            lock (toadd)
            {
                if (empty.Count > 0)
                {
                    id = MyLinq.First(empty.Keys);
                    empty.Remove(id);
                }
                else
                {
                    id = count;
                    count++;
                }
                toadd.Enqueue(new ToAdd()
                {
                    indices = indices,
                    indicesCount = indicesCount,
                    vertices = vertices,
                    verticesCount = verticesCount,
                    id = id,
                    transparent = transparent,
                    texture = texture,
                    center = center,
                    radius = radius
                });
            }
            return id;
        }
        private void AllocateLists()
        {
            while (count >= lists.Count * listincrease)
            {
                GenLists();
            }
            while (count >= listinfoCount)
            {
                Array.Resize(ref listinfo, listinfoCount + listincrease);
                for (int i = listinfoCount; i < listinfoCount + listincrease; i++)
                {
                    listinfo[i] = new ListInfo();
                }
                listinfoCount += listincrease;
            }
        }
        Dictionary<int, int> empty = new Dictionary<int, int>();
        Vector3 playerpos;
        public void Draw(Vector3 playerpos)
        {
            this.playerpos = playerpos;
            AllocateLists();
            //Create display lists, which were saved with Add() call
            //in another thread.
            lock (toadd)
            {
                while (toadd.Count > 0)
                {
                    ToAdd t = toadd.Dequeue();
                    GL.NewList(GetList(t.id), ListMode.Compile);
                    
                    GL.EnableClientState(EnableCap.TextureCoordArray);
                    GL.EnableClientState(EnableCap.VertexArray);
                    GL.EnableClientState(EnableCap.ColorArray);
                    unsafe
                    {
                        fixed (VertexPositionTexture* p = t.vertices)
                        {
                            GL.VertexPointer(3, VertexPointerType.Float, StrideOfVertices, (IntPtr)(0 + (byte*)p));
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, StrideOfVertices, (IntPtr)(12 + (byte*)p));
                            GL.ColorPointer(4, ColorPointerType.UnsignedByte, StrideOfVertices, (IntPtr)(20 + (byte*)p));
                            GL.DrawElements(BeginMode.Triangles, t.indicesCount, DrawElementsType.UnsignedShort, t.indices);
                        }
                    }
                    GL.DisableClientState(EnableCap.TextureCoordArray);
                    GL.DisableClientState(EnableCap.VertexArray);
                    GL.DisableClientState(EnableCap.ColorArray);

                    GL.EndList();
                    ListInfo li = GetListInfo(t.id);
                    li.indicescount = t.indicesCount;
                    li.center = t.center;
                    li.radius = t.radius;
                    li.transparent = t.transparent;
                    li.empty = false;
                    li.texture = GetTextureId(t.texture);
                }
            }
            UpdateCulling();
            
            //Group display lists by used texture to minimize
            //number of GL.BindTexture() calls.
            SortListsByTexture();

            //Need to first render all solid lists (to fill z-buffer), then transparent.
            for (int i = 0; i < texturesCount; i++)
            {
                if (tocallSolid[i].Count == 0) { continue; }
                if (BindTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, glTextures[i]);
                }
                GL.CallLists(tocallSolid[i].Count, ListNameType.Int, tocallSolid[i].Lists);
            }
            GL.Disable(EnableCap.CullFace);//for water.
            for (int i = 0; i < texturesCount; i++)
            {
                if (tocallTransparent[i].Count == 0) { continue; }
                if (BindTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, glTextures[i]);
                }
                GL.CallLists(tocallTransparent[i].Count, ListNameType.Int, tocallTransparent[i].Lists);
            }
            GL.Enable(EnableCap.CullFace);
        }
        //Finds an index in glTextures array.
        private int GetTextureId(int glTexture)
        {
            int id = Array.IndexOf(glTextures, glTexture);
            if (id != -1)
            {
                return id;
            }
            id = Array.IndexOf(glTextures, 0);
            if (id != -1)
            {
                glTextures[id] = glTexture;
                return id;
            }
            int increase = 10;
            Array.Resize(ref glTextures, glTextures.Length + increase);
            glTextures[glTextures.Length - increase] = glTexture;
            return glTextures.Length - increase;
        }
        //Maps from our inner texture id to real opengl texture id.
        int[] glTextures = new int[10];
        ToCall[] tocallSolid;
        ToCall[] tocallTransparent;
        class ToCall
        {
            public int[] Lists;
            public int Count;
        }
        //todo dynamic
        public int texturesCount = 10;
        private void SortListsByTexture()
        {
            if (tocallSolid == null)
            {
                tocallSolid = new ToCall[texturesCount];
                tocallTransparent = new ToCall[texturesCount];
                for (int i = 0; i < texturesCount; i++)
                {
                    tocallSolid[i] = new ToCall();
                    tocallTransparent[i] = new ToCall();
                }
                for (int i = 0; i < texturesCount; i++)
                {
                    tocallSolid[i].Lists = new int[MAX_DISPLAY_LISTS];
                    tocallTransparent[i].Lists = new int[MAX_DISPLAY_LISTS];
                }
            }
            for (int i = 0; i < texturesCount; i++)
            {
                tocallSolid[i].Count = 0;
                tocallTransparent[i].Count = 0;
            }
            for (int i = 0; i < count; i++)
            {
                ListInfo li = listinfo[i];
                if (!li.render)
                {
                    continue;
                }
                if (li.empty)
                {
                    continue;
                }
                if (!li.transparent)
                {
                    ToCall tocall = tocallSolid[li.texture];
                    tocall.Lists[tocall.Count] = GetList(i);
                    tocall.Count++;
                }
                else
                {
                    ToCall tocall = tocallTransparent[li.texture];
                    tocall.Lists[tocall.Count] = GetList(i);
                    tocall.Count++;
                }
            }
        }
        //Not really needed because display lists perform (at least on some computers)
        //their own frustum culling automatically.
        private void UpdateCulling()
        {
            int licount = this.count;
            for (int i = 0; i < licount; i++)
            {
                ListInfo li = listinfo[i];
                Vector3 center = li.center;
                li.render = d_FrustumCulling.SphereInFrustum(center.X, center.Y, center.Z, li.radius);
            }
        }
        public int MAX_DISPLAY_LISTS = 32 * 1024;
        int strideofvertices = -1;
        int StrideOfVertices
        {
            get
            {
                if (strideofvertices == -1) strideofvertices = BlittableValueType.StrideOf(new VertexPositionTexture());
                return strideofvertices;
            }
        }
        class ListInfo
        {
            public bool empty;
            public int indicescount;
            public Vector3 center;
            public float radius;
            public bool transparent;
            public bool render = true;
            public int texture;
        }
        ListInfo[] listinfo = new ListInfo[0];
        int listinfoCount = 0;
        private ListInfo GetListInfo(int id)
        {
            return listinfo[id];
        }
        public void Clear()
        {
            ClearLists();
            count = 0;
            empty.Clear();
            toadd.Clear();
            listinfo = new ListInfo[0];
            listinfoCount = 0;
        }
        public int TotalTriangleCount
        {
            get
            {
                if (listinfo == null)
                {
                    return 0;
                }
                int sum = 0;
                for (int i = 0; i < count; i++)
                {
                    if (!empty.ContainsKey(i))
                    {
                        if (i < listinfoCount)
                        {
                            ListInfo li = GetListInfo(i);
                            if (li.render)
                            {
                                sum += li.indicescount;
                            }
                        }
                    }
                }
                return sum / 3;
            }
        }
    }
}
