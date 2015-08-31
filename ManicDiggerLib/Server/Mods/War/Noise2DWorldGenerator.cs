using System;

namespace ManicDigger.Mods.War
{
	public class Noise2DWorldGeneratorWar : IMod
	{
		private ModManager m;
		private byte[,] _heightcache;
		private int chunksize;
		private Random _rnd;
		// required blocks
		private int emtpy;
		private int stone;
		private int dirt;
		private int grass;
		private int sand;
		private int water;
		private int hay;
		private int lava;
		// settings
		private int waterlevel = 10;
		private int seed;
		
		public void PreStart (ModManager m)
		{
			m.RequireMod ("CoreBlocks");
		}
		
		public void Start (ModManager manager)
		{
			m = manager;
			m.RegisterWorldGenerator (GetChunk);
			
			this.emtpy = m.GetBlockId ("Empty");
			this.stone = m.GetBlockId ("Stone");
			this.dirt = m.GetBlockId ("Dirt");
			this.grass = m.GetBlockId ("Grass");
			this.sand = m.GetBlockId ("Sand");
			this.water = m.GetBlockId ("Water");
			this.hay = m.GetBlockId ("Hay");
			this.lava = m.GetBlockId ("Lava");
			
			this.chunksize = m.GetChunkSize ();
			_rnd = new Random ();
			this.seed = m.GetSeed ();
		}
		
		void GetChunk (int x, int y, int z, ushort[] chunk)
		{
			_heightcache = new byte[this.chunksize, this.chunksize];
			x *= this.chunksize;
			y *= this.chunksize;
			z *= this.chunksize;
			
			for (int xx = 0; xx < this.chunksize; xx++)
			{
				for (int yy = 0; yy < this.chunksize; yy++)
				{
					_heightcache [xx, yy] = GetHeight (x + xx, y + yy);
				}
			}
			// chance of get hay fields
			bool IsHay = _rnd.NextDouble () < 0.005 ? false : true;
			
			for (int xx = 0; xx < this.chunksize; xx++)
			{
				for (int yy = 0; yy < this.chunksize; yy++)
				{
					for (int zz = 0; zz < this.chunksize; zz++)
					{
						int pos = m.Index3d (xx, yy, zz, chunksize, chunksize);
						
						chunk [pos] = IsHay
							? (ushort)GetBlock (x + xx, y + yy, z + zz, _heightcache [xx, yy], 0)
							: (ushort)GetBlock (x + xx, y + yy, z + zz, _heightcache [xx, yy], 1);
					}
				}
			}
			if (z == 0)
			{
				for (int xx = 0; xx < this.chunksize; xx++)
				{
					for (int yy = 0; yy < this.chunksize; yy++)
					{
						int pos = m.Index3d (xx, yy, 0, chunksize, chunksize);
						chunk [pos] = (ushort)this.lava;
					}
				}
			}
		}
		
		private int GetBlock (int x, int y, int z, int height, int special)
		{
			int spec = special;
			
			if (z > this.waterlevel)
			{
				if (spec == 0)
				{
					if (z > height)
					{
						return this.emtpy;
					}
					if (z == height)
					{
						return this.grass;
					}
				}
				else
				{
					if (z > height + 1)
					{
						return this.emtpy;
					}
					if (z == height)
					{
						return this.hay;
					}
					if (z == height + 1)
					{
						return this.hay;
					}
				}
				if (z > height - 5)
				{
					return this.dirt;
				}
				return this.stone;
			}
			else
			{
				if (z > height)
				{
					return this.water;
				}
				if (z == height)
				{
					return this.sand;
				}
				return this.stone;
			}
		}
		
		private byte GetHeight (int x, int y)
		{
			x += 30;
			y -= 30;
			//double p = 0.2 + ((findnoise2(x / 100.0, y / 100.0) + 1.0) / 2) * 0.3;
			double p = 0.5;
			double zoom = 150;
			double getnoise = 0;
			int octaves = 6;
			for (int a = 0; a < octaves - 1; a++)
			{//This loops trough the octaves.
				double frequency = Math.Pow (2, a);//This increases the frequency with every loop of the octave.
				double amplitude = Math.Pow (p, a);//This decreases the amplitude with every loop of the octave.
				getnoise += this.noise (((double)x) * frequency / zoom, ((double)y) / zoom * frequency, this.seed) * amplitude;//This uses our perlin noise functions. It calculates all our zoom and frequency and amplitude
			}
			double maxheight = 64;
			int height = (int)(((getnoise + 1) / 2.0) * (maxheight - 5)) + 3;//(int)((getnoise * 128.0) + 128.0);
			if (height > maxheight - 1)
			{
				height = (int)maxheight - 1;
			}
			if (height < 2)
			{
				height = 2;
			}
			return (byte)height;
		}
		
		private double noise (double x, double y, int seed)
		{
			double floorx = (double)((int)x);//This is kinda a cheap way to floor a double integer.
			double floory = (double)((int)y);
			double s, t, u, v;//Integer declaration
			s = findNoise2 (floorx, floory, seed);
			t = findNoise2 (floorx + 1, floory, seed);
			u = findNoise2 (floorx, floory + 1, seed);//Get the surrounding pixels to calculate the transition.
			v = findNoise2 (floorx + 1, floory + 1, seed);
			double int1 = interpolate (s, t, x - floorx);//Interpolate between the values.
			double int2 = interpolate (u, v, x - floorx);//Here we use x-floorx, to get 1st dimension. Don't mind the x-floorx thingie, it's part of the cosine formula.
			return interpolate (int1, int2, y - floory);//Here we use y-floory, to get the 2nd dimension.
		}
		
		private double findNoise2 (double x, double y, int seed)
		{
			int n = (int)x + (int)y * 57;
			return findNoise1 (n, seed);
		}
		
		private double findNoise1 (int n, int seed)
		{
			n += seed;
			n = (n << 13) ^ n;
			int nn = (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
			return 1.0 - ((double)nn / 1073741824.0);
		}
		
		private double interpolate (double a, double b, double x)
		{
			double ft = x * 3.1415927;
			double f = (1.0 - Math.Cos (ft)) * 0.5;
			return a * (1.0 - f) + b * f;
		}
	}
}
