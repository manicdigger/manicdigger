using System.Collections.Generic;

namespace Md2Engine
{
    interface IGLCommand
    {
        byte getType();
        void setType(byte pt);
    }
    public class GLCommand : Md2Engine.IGLCommand
    {
        byte type; //1 - strip, 2 - fan

        public List<GLCommPacket> packets;

        public GLCommand()
        {
            packets = new List<GLCommPacket>();
        }

        public void setType(byte pt)
        {
            type = pt;
        }

        public byte getType()
        {
            return type;
        }
    }
}
