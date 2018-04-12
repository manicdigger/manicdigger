namespace ManicDigger.Server
{
	public struct Xyz
	{
		public int X;
		public int Y;
		public int Z;
		public override int GetHashCode()
		{
			return X ^ Y ^ Z;
		}
		public override bool Equals(object obj)
		{
			if (obj is Xyz)
			{
				Xyz other = (Xyz)obj;
				return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
			}
			return base.Equals(obj);
		}
	}

	public struct DbChunk
	{
		public Xyz Position;
		public byte[] Chunk;
	}
}
