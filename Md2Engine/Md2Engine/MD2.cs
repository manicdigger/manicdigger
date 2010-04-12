using System;

namespace Md2Engine
{
    struct md2_header
    {
        public int ident;              // signature. must be equal to "IPD2"
        public int version;            // md2 version. must be equal to 8

        public int skinwidth;          // width of the texture
        public int skinheight;         // height of the texture
        public int framesize;          // size of one frame in bytes

        public int num_skins;          // number of textures
        public int num_xyz;            // number of vertices
        public int num_st;             // number of texture coordinates
        public int num_tris;           // number of triangles
        public int num_glcmds;         // number of opengl commands
        public int num_frames;         // total number of frames

        public int ofs_skins;          // offset to skin names (64 bytes each)
        public int ofs_st;             // offset to s-t texture coordinates
        public int ofs_tris;           // offset to triangles
        public int ofs_frames;         // offset to frame data
        public int ofs_glcmds;         // offset to opengl commands
        public int ofs_end;            // offset to end of file
    }

    struct md2_vertex
    {
        unsafe public fixed byte coords[3]; //x,y,z compressed coordinates (see frame)
        public byte light; //index to vertice's lighting normal
    }

    struct md2_triangle
    {     
	    unsafe public fixed short index_xyz[3];    // indexes to triangle's vertices     	
	    unsafe public fixed short index_st[3];     // indexes to vertice's texture coorinates
    }

    struct md2_frame
    {
        unsafe public fixed float scale[3]; //scale values for x,y,z (multiply)
        unsafe public fixed float translate[3]; //translate values for x,y,z (add)
        unsafe public fixed byte name[16]; //frame name;
    }

    struct md2_compressed_texture
    {
        public short s; //texture x coordinate (compressed, should be divided by skin width)
        public short t; //texture y coordinate (compressed, should be divided by skin height)
    }

    struct md2_glcommand
    {
        public float s;
        public float t;
        public int index;
    }

    unsafe public class MD2utils
    {
        public static string chr(byte pchar) //CHR function, convert ascii code to ASCII character
        {
            byte[] tmp = new byte[1];
            tmp[0] = pchar;
            return System.Text.Encoding.ASCII.GetString(tmp);
        }

        public static string getSignature(int ident) //translate signature to string (example on gow to get bytes from int)
        {
            string tmp = "";

            byte[] b = new byte[4];

            b[3] = (byte)((ident >> 24) & 0xFF);
            b[2] = (byte)((ident >> 16) & 0xFF);
            b[1] = (byte)((ident >> 8) & 0xFF);
            b[0] = (byte)(ident & 0xFF);

            tmp = chr(b[0]) + chr(b[1]) + chr(b[2]) + chr(b[3]);

            return tmp;
        }

        public static string getStrFromCharArray(byte* ca)//convert a char array to a string (STUPID sollution)
        {
            string tmp = "";

            for (int i = 0; i <= 16; i++)
            {
                tmp = tmp + chr(ca[i]);
            }

            return tmp;
        }

        public static string getLiteralsFromString(string input)//get the first part of a string that is only literal
        {
            char[] seps = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            return input.Split(seps)[0];
        }
    }
}
