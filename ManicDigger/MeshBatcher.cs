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
        public IFrustumCulling frustumculling;
        public MeshBatcher()
        {
        }
        private void genlists()
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
        //int lists = -1;
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
            public VertexPositionTexture[] vertices;
            public int id;
            public bool transparent;
            public int texture;
            public Vector3 center;
            public float radius;
        }
        Queue<ToAdd> toadd = new Queue<ToAdd>();
        public int Add(ushort[] p, VertexPositionTexture[] vertexPositionTexture, bool transparent, int texture, Vector3 center, float radius)
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
                    indices = p,
                    vertices = vertexPositionTexture,
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
                genlists();
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
        float addperframe = 0.5f;
        float addcounter = 0;
        Vector3 playerpos;
        public void Draw(Vector3 playerpos)
        {
            this.playerpos = playerpos;
            AllocateLists();
            lock (toadd)
            {
                addcounter += addperframe;
                while (//addcounter >= 1 &&
                    toadd.Count > 0)
                {
                    addcounter -= 1;
                    ToAdd t = toadd.Dequeue();
                    GL.NewList(GetList(t.id), ListMode.Compile);

                    GL.BindTexture(TextureTarget.Texture2D, t.texture);
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
                            GL.DrawElements(BeginMode.Triangles, t.indices.Length, DrawElementsType.UnsignedShort, t.indices);
                        }
                    }
                    GL.DisableClientState(EnableCap.TextureCoordArray);
                    GL.DisableClientState(EnableCap.VertexArray);
                    GL.DisableClientState(EnableCap.ColorArray);

                    GL.EndList();
                    ListInfo li = GetListInfo(t.id);
                    li.indicescount = t.indices.Length;
                    li.center = t.center;
                    li.radius = t.radius;
                    li.transparent = t.transparent;
                    li.empty = false;
                }
                if (toadd.Count == 0)
                {
                    addcounter = 0;
                }
            }
            if (tocall == null)
            {
                tocall = new int[MAX_DISPLAY_LISTS];
            }
            if (tocall2 == null)
            {
                tocall2 = new int[MAX_DISPLAY_LISTS];
            }
            UpdateCulling();
            int tocallpos = 0;
            int tocallpos2 = 0;
            PrepareToCall(ref tocallpos, ref tocallpos2);
            GL.CallLists(tocallpos, ListNameType.Int, tocall);
            tocallpos = 0;
            GL.Disable(EnableCap.CullFace);//for water.
            GL.CallLists(tocallpos2, ListNameType.Int, tocall2);
            GL.Enable(EnableCap.CullFace);
        }
        private void PrepareToCall(ref int tocallpos, ref int tocallpos2)
        {
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
                    tocall[tocallpos] = GetList(i);
                    tocallpos++;
                }
                else
                {
                    tocall2[tocallpos2] = GetList(i);
                    tocallpos2++;
                }
            }
        }
        private void UpdateCulling()
        {
            int licount = this.count;
            for (int i = 0; i < licount; i++)
            {
                ListInfo li = listinfo[i];
                Vector3 center = li.center;
                li.render = frustumculling.SphereInFrustum(center.X, center.Y, center.Z, li.radius);
            }
        }
        public int MAX_DISPLAY_LISTS = 32 * 1024;
        int[] tocall;
        int[] tocall2;
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
