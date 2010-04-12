using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

namespace Md2Engine
{
    interface IMesh
    {
        void addTriangle(int pa, int pb, int pc);
        void addVertex(float px, float py, float pz, int fr);
        void calcNormals();
        int findAnim(string pn);
        void getAnimations();
        string getName();
        void load3DS(string filename);
        void loadASC(string filename);
        void loadMD2(string filename);
        void setName(string nn);
    }
    public class Mesh : IMesh
    {
        private string name;

        public List<Frame> framePool; //contains the vertexes
        public List<Triangle> trianglePool; //contains the triangles as triplets of indexex in the vertexPool of a frame, also contains the texture coordinates indexes into the pool
        public List<TexCoordinate> texcoordPool; //all the texture coordinates
        public List<Range> animationPool; //contains a list with all known animations, (name, start ftame, end frame)
        public List<GLCommand> glCommandPool; //contains a list of triangle strips and fans for faster rendering

        public Mesh()
        {
            name = "unknown";

            framePool = new List<Frame>();
            trianglePool = new List<Triangle>();
            texcoordPool = new List<TexCoordinate>();
            animationPool = new List<Range>();
            glCommandPool = new List<GLCommand>();
        }

        public int findAnim(string pn)//finds the animation index with teh specified name (returns -1 for failure)
        {
            int pos = -1;

            for (int i = 0; i < animationPool.Count; i++)
            {
                if (animationPool[i].getName().Equals(pn))
                {
                    pos = i;
                    i = animationPool.Count;
                }
            }

            return pos;
        }

        public void getAnimations() //reads the frames and builds the animationPool
        {
            int anim = 0;

            for (int i = 0; i < framePool.Count; i++)
            {
                anim = findAnim(MD2utils.getLiteralsFromString(framePool[i].getName()));

                if (anim >= 0)
                {
                    animationPool[anim].setEnd(animationPool[anim].getEnd() + 1);
                }
                else
                {
                    animationPool.Add(new Range(MD2utils.getLiteralsFromString(framePool[i].getName()), i, i));
                }
            }
        }

        // NAME setter/getter

        public void setName(string nn)
        {
            name = nn;
        }

        public string getName()
        {
            return name;
        }

        public void addVertex(float px, float py, float pz, int fr) //add a vertex to the pool of the specified fr frame
        {
            Vertex tmp = new Vertex(px, py, pz);
            framePool[fr].vertexPool.Add(tmp);
        }

        public void addTriangle(int pa, int pb, int pc) //add a triangle to the pool
        {
            Triangle tmp = new Triangle(pa, pb, pc);
            trianglePool.Add(tmp);
        }

        public void calcNormals()//calculate the normals of the vertexes for shading
        {
            Vector a = new Vector();
            Vector b = new Vector();
            Vector c = new Vector();

            Vector va = new Vector();
            Vector vb = new Vector();

            Vector norm = new Vector();

            for (int i = 0; i < framePool.Count; i++)
            {
                for (int k = 0; k < framePool[i].vertexPool.Count; k++)
                {
                    framePool[i].normalsPool.Add(new Vector()); //add the normals for each vector
                }

                for (int j = 0; j < trianglePool.Count; j++)
                {
                    a.setX(framePool[i].vertexPool[trianglePool[j].getA()].getX());
                    a.setY(framePool[i].vertexPool[trianglePool[j].getA()].getY());
                    a.setZ(framePool[i].vertexPool[trianglePool[j].getA()].getZ());

                    b.setX(framePool[i].vertexPool[trianglePool[j].getB()].getX());
                    b.setY(framePool[i].vertexPool[trianglePool[j].getB()].getY()); //make vectors from the 3 corners of the triangle
                    b.setZ(framePool[i].vertexPool[trianglePool[j].getB()].getZ());

                    c.setX(framePool[i].vertexPool[trianglePool[j].getC()].getX());
                    c.setY(framePool[i].vertexPool[trianglePool[j].getC()].getY());
                    c.setZ(framePool[i].vertexPool[trianglePool[j].getC()].getZ());

                    va = new Vector(a, b); //make two coplanar vectors
                    vb = new Vector(a, c);

                    norm = Vector.dotProduct(va, vb); //dot product into the normal vector

                    norm.normalize();

                    framePool[i].vertexPool[trianglePool[j].getA()].setConnections(framePool[i].vertexPool[trianglePool[j].getA()].getConnections() + 1);
                    framePool[i].vertexPool[trianglePool[j].getB()].setConnections(framePool[i].vertexPool[trianglePool[j].getB()].getConnections() + 1); //increase connections number for the three corner vertexes
                    framePool[i].vertexPool[trianglePool[j].getC()].setConnections(framePool[i].vertexPool[trianglePool[j].getC()].getConnections() + 1);

                    framePool[i].normalsPool[trianglePool[j].getA()].setX(framePool[i].normalsPool[trianglePool[j].getA()].getX() + norm.getX());
                    framePool[i].normalsPool[trianglePool[j].getA()].setY(framePool[i].normalsPool[trianglePool[j].getA()].getY() + norm.getY());
                    framePool[i].normalsPool[trianglePool[j].getA()].setZ(framePool[i].normalsPool[trianglePool[j].getA()].getZ() + norm.getZ());

                    framePool[i].normalsPool[trianglePool[j].getB()].setX(framePool[i].normalsPool[trianglePool[j].getB()].getX() + norm.getX());
                    framePool[i].normalsPool[trianglePool[j].getB()].setY(framePool[i].normalsPool[trianglePool[j].getB()].getY() + norm.getY());
                    framePool[i].normalsPool[trianglePool[j].getB()].setZ(framePool[i].normalsPool[trianglePool[j].getB()].getZ() + norm.getZ());

                    framePool[i].normalsPool[trianglePool[j].getC()].setX(framePool[i].normalsPool[trianglePool[j].getC()].getX() + norm.getX());
                    framePool[i].normalsPool[trianglePool[j].getC()].setY(framePool[i].normalsPool[trianglePool[j].getC()].getY() + norm.getY());
                    framePool[i].normalsPool[trianglePool[j].getC()].setZ(framePool[i].normalsPool[trianglePool[j].getC()].getZ() + norm.getZ());
                }
            }

            for (int i = 0; i<framePool.Count; i++)
            {
                for (int j = 0; j<framePool[i].vertexPool.Count; j++)
                {
                    framePool[i].normalsPool[j].setX(framePool[i].normalsPool[j].getX() / framePool[i].vertexPool[j].getConnections());
                    framePool[i].normalsPool[j].setY(framePool[i].normalsPool[j].getY() / framePool[i].vertexPool[j].getConnections());
                    framePool[i].normalsPool[j].setZ(framePool[i].normalsPool[j].getZ() / framePool[i].vertexPool[j].getConnections());
                }
            }
        }

        public void load3DS(string filename)//load a mesh from a 3DS file
        {
            //[TODO]
        }

        unsafe public void loadMD2(string filename)//load a mesh from a Quake II MD2 file
        {
            md2_header header; //structure to read the MD2 header

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read); //stream object
            BinaryReader br = new BinaryReader(fs); //reader object

            //HEADER - will read the header in the header structure
            byte[] buff = br.ReadBytes(Marshal.SizeOf(typeof(md2_header)));

            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned); //prevent the GC from affecting the buffer
            header = (md2_header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(md2_header)); //get the header struct from the file
            handle.Free();
            //HEADER END

            //TRIANGLES
            md2_triangle trt; //structure to hold a MD2 triangle

            br.BaseStream.Seek(header.ofs_tris, SeekOrigin.Begin); //seek to the triangles offset in the MD2 file

            for (int i = 0; i <= header.num_tris - 1; i++)
            {
                buff = br.ReadBytes(Marshal.SizeOf(typeof(md2_triangle)));

                handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                trt = (md2_triangle)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(md2_triangle));
                handle.Free();

                trianglePool.Add(new Triangle(trt.index_xyz[2], trt.index_xyz[1], trt.index_xyz[0]));
                trianglePool[i].setTABC(trt.index_st[2], trt.index_st[1], trt.index_st[0]);
            }
            //TRIANGLES END

            //FRAME

            for (int i = 0; i < header.num_frames - 1; i++)
            {
                framePool.Add(new Frame());
            }//create frame objects for the pool

            br.BaseStream.Seek(header.ofs_frames, SeekOrigin.Begin);

            for (int j = 0; j < header.num_frames-1; j++)
            {
                buff = br.ReadBytes(Marshal.SizeOf(typeof(md2_frame)));

                handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                md2_frame ffrm = (md2_frame)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(md2_frame));
                handle.Free();

                framePool[j].setName(MD2utils.getStrFromCharArray(ffrm.name));

                md2_vertex tvrt;
                float tx, ty, tz;

                for (int i = 0; i <= header.num_xyz - 1; i++)
                {
                    buff = br.ReadBytes(Marshal.SizeOf(typeof(md2_vertex)));

                    handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                    tvrt = (md2_vertex)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(md2_vertex));
                    handle.Free();

                    tx = tvrt.coords[0] * ffrm.scale[0] + ffrm.translate[0];
                    ty = tvrt.coords[1] * ffrm.scale[1] + ffrm.translate[1];
                    tz = tvrt.coords[2] * ffrm.scale[2] + ffrm.translate[2];

                    framePool[j].vertexPool.Add(new Vertex(tx, ty, tz));
                }
            }
            //END FRAME

            //TEXTURE COORDINATES (texture must be vertical flipped sometimes ?!)
            md2_compressed_texture ctex; //compressed texture as read from md2

            br.BaseStream.Seek(header.ofs_st, SeekOrigin.Begin);

            for (int i = 0; i < header.num_st; i++)
            {
                buff = br.ReadBytes(Marshal.SizeOf(typeof(md2_compressed_texture)));

                handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                ctex = (md2_compressed_texture)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(md2_compressed_texture));
                handle.Free();

                texcoordPool.Add(new TexCoordinate(((float)ctex.s / header.skinwidth), ((float)ctex.t / header.skinheight)));
            }
            //TEXTURE COORDINATES END

            //GLCOMMANDS START
            md2_glcommand cmd; //compressed texture as read from md2
            int typecnt = -1;
            int cnt = 0;

            br.BaseStream.Seek(header.ofs_glcmds, SeekOrigin.Begin);

            buff = br.ReadBytes(Marshal.SizeOf(typeof(int)));

            handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            typecnt = (int)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(int));
            handle.Free();

            while (typecnt != 0)
            {
                glCommandPool.Add(new GLCommand());

                if (typecnt < 0)
                {
                    glCommandPool[cnt].setType(1);
                }
                else
                {
                    glCommandPool[cnt].setType(2);
                }

                for (int i = 0; i < Math.Abs(typecnt); i++)
                {
                    buff = br.ReadBytes(Marshal.SizeOf(typeof(md2_glcommand)));

                    handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                    cmd = (md2_glcommand)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(md2_glcommand));
                    handle.Free();

                    glCommandPool[cnt].packets.Add(new GLCommPacket(cmd.s, cmd.t, cmd.index));
                }

                buff = br.ReadBytes(Marshal.SizeOf(typeof(int)));

                handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                typecnt = (int)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(int));
                handle.Free();

                cnt++;
            }
            //GLCOMMANDS END

            getAnimations();
            calcNormals();

            fs.Close();
        }

        public void loadASC(string filename)//load a mesh from an ASC file
        {
            FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);

            string buffer = "";

            sr.ReadLine();
            sr.ReadLine();//skip 1st two lines

            buffer = sr.ReadLine(); //get the object name;
            buffer = buffer.Substring(buffer.IndexOf("\"") + 1, buffer.Length - buffer.IndexOf("\"") - 2);
            name = buffer;

            buffer = sr.ReadLine();
            buffer = buffer.Substring(buffer.IndexOf(":") + 2, buffer.Length - buffer.IndexOf(":") - 2);

            int nv = Convert.ToInt32(buffer.Substring(0, buffer.IndexOf(" ")));//no of vertices

            buffer = buffer.Substring(buffer.IndexOf(":") + 2, buffer.Length - buffer.IndexOf(":") - 2);

            int nf = Convert.ToInt32(buffer);

            //read vertices + text coords
            sr.ReadLine();

            Frame tmpfrm = new Frame();

            for (int i = 0; i < nv; i++)
            {
                buffer = sr.ReadLine();
                float vx, vy, vz, vu, vv;

                vx = (float)Convert.ToDouble(buffer.Substring(buffer.IndexOf("x: ") + 3, buffer.IndexOf("y") - buffer.IndexOf("x: ") - 3));
                vy = (float)Convert.ToDouble(buffer.Substring(buffer.IndexOf("y: ") + 3, buffer.IndexOf("z") - buffer.IndexOf("y: ") - 3));
                vz = (float)Convert.ToDouble(buffer.Substring(buffer.IndexOf("z: ") + 3, buffer.IndexOf("u") - buffer.IndexOf("z: ") - 3));
                vu = (float)Convert.ToDouble(buffer.Substring(buffer.IndexOf("u: ") + 3, buffer.IndexOf("v") - buffer.IndexOf("u: ") - 3));
                vv = (float)Convert.ToDouble(buffer.Substring(buffer.IndexOf("v: ") + 3, buffer.Length - buffer.IndexOf("v: ") - 3));

                tmpfrm.vertexPool.Add(new Vertex(vx, vy, vz));//add vertex
                texcoordPool.Add(new TexCoordinate(vu, vv));//add tex coordinates
            }

            framePool.Add(tmpfrm);

            //read triangles
            sr.ReadLine();

            for (int i = 0; i < nf; i++)
            {
                buffer = sr.ReadLine();

                int fa, fb, fc;

                fa = Convert.ToInt32(buffer.Substring(buffer.IndexOf("A: ") + 3, buffer.IndexOf("B") - buffer.IndexOf("A: ") - 3));
                fb = Convert.ToInt32(buffer.Substring(buffer.IndexOf("B: ") + 3, buffer.IndexOf("C") - buffer.IndexOf("B: ") - 3));
                fc = Convert.ToInt32(buffer.Substring(buffer.IndexOf("C: ") + 3, buffer.Length - buffer.IndexOf("C: ") - 3));

                trianglePool.Add(new Triangle(fa, fb, fc));//add triangle
                trianglePool[i].setTABC(fa, fb, fc);//set tex coord pointers for the corners
            }

            //animationPool[0] = new Range("default01", 0, 0);
            getAnimations();
            calcNormals();

            sr.Close();
        }
    }
}
