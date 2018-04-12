using ProtoBuf;
using System.Collections.Generic;

namespace ManicDigger.Server
{
	[ProtoContract]
	public class ManicDiggerSave
	{
		[ProtoMember(1, IsRequired = false)]
		public int MapSizeX;
		[ProtoMember(2, IsRequired = false)]
		public int MapSizeY;
		[ProtoMember(3, IsRequired = false)]
		public int MapSizeZ;
		[ProtoMember(4, IsRequired = false)]
		public Dictionary<string, PacketServerInventory> Inventory;
		[ProtoMember(7, IsRequired = false)]
		public int Seed;
		[ProtoMember(8, IsRequired = false)]
		public long SimulationCurrentFrame;
		[ProtoMember(9, IsRequired = false)]
		public Dictionary<string, PacketServerPlayerStats> PlayerStats;
		[ProtoMember(10, IsRequired = false)]
		public int LastMonsterId;
		[ProtoMember(11, IsRequired = false)]
		public Dictionary<string, byte[]> moddata;
		[ProtoMember(12, IsRequired = false)]
		public long TimeOfDay;
	}
}
