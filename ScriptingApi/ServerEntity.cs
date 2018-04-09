using ProtoBuf;

namespace ManicDigger
{
	[ProtoContract()]
	public class ServerEntity
	{
		[ProtoMember(1, IsRequired = false)]
		public ServerEntityPositionAndOrientation position;
		[ProtoMember(2, IsRequired = false)]
		public ServerEntityDrawName drawName;
		[ProtoMember(3, IsRequired = false)]
		public ServerEntityAnimatedModel drawModel;
		[ProtoMember(4, IsRequired = false)]
		public ServerEntityDrawText drawText;
		[ProtoMember(5, IsRequired = false)]
		public ServerEntityPush push;
		[ProtoMember(7, IsRequired = false)]
		public bool usable;
		[ProtoMember(9, IsRequired = false)]
		public ServerEntityDrawArea drawArea;

		[ProtoMember(6, IsRequired = false)]
		public ServerEntitySign sign;
		[ProtoMember(8, IsRequired = false)]
		public ServerEntityPermissionSign permissionSign;
	}

	[ProtoContract()]
	public class ServerEntityDrawArea
	{
		public int x;
		public int y;
		public int z;
		public int sizex;
		public int sizey;
		public int sizez;
		public int visibleToClientId;
	}

	[ProtoContract()]
	public class ServerEntityDrawName
	{
		[ProtoMember(1)]
		public string name;
		[ProtoMember(2)]
		public bool onlyWhenSelected;
		[ProtoMember(3)]
		public bool clientAutoComplete;
		[ProtoMember(4)]
		public string color;
	}

	[ProtoContract()]
	public class ServerEntitySign
	{
		[ProtoMember(1)]
		public string text;
	}

	[ProtoContract()]
	public class ServerEntityPermissionSign
	{
		[ProtoMember(1)]
		public string name;
		[ProtoMember(2)]
		public PermissionSignType type;
	}

	public enum PermissionSignType
	{
		Player,
		Group,
	}

	[ProtoContract()]
	public class ServerEntityAnimatedModel
	{
		[ProtoMember(1, IsRequired = false)]
		public string model;
		[ProtoMember(2, IsRequired = false)]
		public string texture;
		[ProtoMember(3, IsRequired = false)]
		public float eyeHeight;
		[ProtoMember(4, IsRequired = false)]
		public float modelHeight;
		[ProtoMember(5, IsRequired = false)]
		public bool downloadSkin;
	}

	[ProtoContract()]
	public class ServerEntityPositionAndOrientation
	{
		[ProtoMember(1, IsRequired = false)]
		public float x;
		[ProtoMember(2, IsRequired = false)]
		public float y;
		[ProtoMember(3, IsRequired = false)]
		public float z;
		[ProtoMember(4, IsRequired = false)]
		public byte heading;
		[ProtoMember(5, IsRequired = false)]
		public byte pitch;
		[ProtoMember(6, IsRequired = false)]
		public byte stance;

		public ServerEntityPositionAndOrientation Clone()
		{
			ServerEntityPositionAndOrientation ret = new ServerEntityPositionAndOrientation();
			ret.x = x;
			ret.y = y;
			ret.z = z;
			ret.heading = heading;
			ret.pitch = pitch;
			ret.stance = stance;
			return ret;
		}
	}

	[ProtoContract()]
	public class ServerEntityDrawText
	{
		[ProtoMember(1, IsRequired = false)]
		public string text;
		[ProtoMember(2, IsRequired = false)]
		public float dx;
		[ProtoMember(3, IsRequired = false)]
		public float dy;
		[ProtoMember(4, IsRequired = false)]
		public float dz;
		[ProtoMember(5, IsRequired = false)]
		public float rotx;
		[ProtoMember(6, IsRequired = false)]
		public float roty;
		[ProtoMember(7, IsRequired = false)]
		public float rotz;
	}

	[ProtoContract()]
	public class ServerEntityPush
	{
		[ProtoMember(1)]
		public float range;
	}

	public class ServerEntityId
	{
		public int chunkx;
		public int chunky;
		public int chunkz;
		public int id;

		public ServerEntityId() { }
		public ServerEntityId(int cx, int cy, int cz, int eid)
		{
			chunkx = cx;
			chunky = cy;
			chunkz = cz;
			id = eid;
		}

		public ServerEntityId Clone()
		{
			return new ServerEntityId(chunkx, chunky, chunkz, id);
		}
	}
}
