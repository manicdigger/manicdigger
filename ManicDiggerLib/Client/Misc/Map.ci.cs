public class Map
{
	internal Chunk[] chunks;
	internal int MapSizeX;
	internal int MapSizeY;
	internal int MapSizeZ;


#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
	static int Index3d(int x, int y, int h, int sizex, int sizey)
	{
		return (h * sizey + y) * sizex + x;
	}
#endif

	public int GetBlockValid(int x, int y, int z)
	{
		int cx = x >> Game.chunksizebits;
		int cy = y >> Game.chunksizebits;
		int cz = z >> Game.chunksizebits;
		int chunkpos = Index3d(cx, cy, cz, MapSizeX >> Game.chunksizebits, MapSizeY >> Game.chunksizebits);
		if (chunks[chunkpos] == null)
		{
			return 0;
		}
		else
		{
			int pos = Index3d(x & (Game.chunksize - 1), y & (Game.chunksize - 1), z & (Game.chunksize - 1), Game.chunksize, Game.chunksize);
			return chunks[chunkpos].GetBlockInChunk(pos);
		}
	}

	public Chunk GetChunk(int x, int y, int z)
	{
		x = x / Game.chunksize;
		y = y / Game.chunksize;
		z = z / Game.chunksize;
		return GetChunk_(x, y, z);
	}

	public Chunk GetChunk_(int cx, int cy, int cz)
	{
		int mapsizexchunks = MapSizeX / Game.chunksize;
		int mapsizeychunks = MapSizeY / Game.chunksize;
		Chunk chunk = chunks[Index3d(cx, cy, cz, mapsizexchunks, mapsizeychunks)];
		if (chunk == null)
		{
			Chunk c = new Chunk();
			c.data = new byte[Game.chunksize * Game.chunksize * Game.chunksize];
			c.baseLight = new byte[Game.chunksize * Game.chunksize * Game.chunksize];
			chunks[Index3d(cx, cy, cz, mapsizexchunks, mapsizeychunks)] = c;
			return chunks[Index3d(cx, cy, cz, mapsizexchunks, mapsizeychunks)];
		}
		return chunk;
	}

	public void SetBlockRaw(int x, int y, int z, int tileType)
	{
		Chunk chunk = GetChunk(x, y, z);
		int pos = Index3d(x % Game.chunksize, y % Game.chunksize, z % Game.chunksize, Game.chunksize, Game.chunksize);
		chunk.SetBlockInChunk(pos, tileType);
	}

	public void CopyChunk(Chunk chunk, int[] output)
	{
		int n = Game.chunksize * Game.chunksize * Game.chunksize;
		if (chunk.dataInt != null)
		{
			for (int i = 0; i < n; i++)
			{
				output[i] = chunk.dataInt[i];
			}
		}
		else
		{
			for (int i = 0; i < n; i++)
			{
				output[i] = chunk.data[i];
			}
		}
	}

	public void Reset(int sizex, int sizey, int sizez)
	{
		MapSizeX = sizex;
		MapSizeY = sizey;
		MapSizeZ = sizez;
		chunks = new Chunk[(sizex / Game.chunksize) * (sizey / Game.chunksize) * (sizez / Game.chunksize)];
	}

	public void GetMapPortion(int[] outPortion, int x, int y, int z, int portionsizex, int portionsizey, int portionsizez)
	{
		int outPortionCount = portionsizex * portionsizey * portionsizez;
		for (int i = 0; i < outPortionCount; i++)
		{
			outPortion[i] = 0;
		}

		//int chunksizebits = p.FloatToInt(p.MathLog(chunksize, 2));

		int mapchunksx = MapSizeX / Game.chunksize;
		int mapchunksy = MapSizeY / Game.chunksize;
		int mapchunksz = MapSizeZ / Game.chunksize;
		int mapsizechunks = mapchunksx * mapchunksy * mapchunksz;

		for (int xx = 0; xx < portionsizex; xx++)
		{
			for (int yy = 0; yy < portionsizey; yy++)
			{
				for (int zz = 0; zz < portionsizez; zz++)
				{
					//Find chunk.
					int cx = (x + xx) >> Game.chunksizebits;
					int cy = (y + yy) >> Game.chunksizebits;
					int cz = (z + zz) >> Game.chunksizebits;
					//int cpos = MapUtil.Index3d(cx, cy, cz, MapSizeX / chunksize, MapSizeY / chunksize);
					int cpos = (cz * mapchunksy + cy) * mapchunksx + cx;
					//if (cpos < 0 || cpos >= ((MapSizeX / chunksize) * (MapSizeY / chunksize) * (MapSizeZ / chunksize)))
					if (cpos < 0 || cpos >= mapsizechunks)
					{
						continue;
					}
					Chunk chunk = chunks[cpos];
					if (chunk == null || !chunk.ChunkHasData())
					{
						continue;
					}
					//int pos = MapUtil.Index3d((x + xx) % chunksize, (y + yy) % chunksize, (z + zz) % chunksize, chunksize, chunksize);
					int chunkGlobalX = cx << Game.chunksizebits;
					int chunkGlobalY = cy << Game.chunksizebits;
					int chunkGlobalZ = cz << Game.chunksizebits;

					int inChunkX = (x + xx) - chunkGlobalX;
					int inChunkY = (y + yy) - chunkGlobalY;
					int inChunkZ = (z + zz) - chunkGlobalZ;

					//int pos = MapUtil.Index3d(inChunkX, inChunkY, inChunkZ, chunksize, chunksize);
					int pos = (((inChunkZ << Game.chunksizebits) + inChunkY) << Game.chunksizebits) + inChunkX;

					int block = chunk.GetBlockInChunk(pos);
					//outPortion[MapUtil.Index3d(xx, yy, zz, portionsizex, portionsizey)] = (byte)block;
					outPortion[(zz * portionsizey + yy) * portionsizex + xx] = block;
				}
			}
		}
	}

	public bool IsValidPos(int x, int y, int z)
	{
		if (x < 0 || y < 0 || z < 0)
		{
			return false;
		}
		if (x >= MapSizeX || y >= MapSizeY || z >= MapSizeZ)
		{
			return false;
		}
		return true;
	}

	public bool IsValidChunkPos(int cx, int cy, int cz)
	{
		return cx >= 0 && cy >= 0 && cz >= 0
			&& cx < MapSizeX / Game.chunksize
			&& cy < MapSizeY / Game.chunksize
			&& cz < MapSizeZ / Game.chunksize;
	}

	public int GetBlock(int x, int y, int z)
	{
		if (!IsValidPos(x, y, z))
		{
			return 0;
		}
		return GetBlockValid(x, y, z);
	}

	public void SetChunkDirty(int cx, int cy, int cz, bool dirty, bool blockschanged)
	{
		if (!IsValidChunkPos(cx, cy, cz))
		{
			return;
		}

		Chunk c = chunks[MapUtilCi.Index3d(cx, cy, cz, mapsizexchunks(), mapsizeychunks())];
		if (c == null)
		{
			return;
		}
		if (c.rendered == null)
		{
			c.rendered = new RenderedChunk();
		}
		c.rendered.dirty = dirty;
		if (blockschanged)
		{
			c.baseLightDirty = true;
		}
	}

	public int mapsizexchunks() { return MapSizeX >> Game.chunksizebits; }
	public int mapsizeychunks() { return MapSizeY >> Game.chunksizebits; }
	public int mapsizezchunks() { return MapSizeZ >> Game.chunksizebits; }

	public void SetChunksAroundDirty(int cx, int cy, int cz)
	{
		if (IsValidChunkPos(cx, cy, cz)) { SetChunkDirty(cx - 1, cy, cz, true, false); }
		if (IsValidChunkPos(cx - 1, cy, cz)) { SetChunkDirty(cx - 1, cy, cz, true, false); }
		if (IsValidChunkPos(cx + 1, cy, cz)) { SetChunkDirty(cx + 1, cy, cz, true, false); }
		if (IsValidChunkPos(cx, cy - 1, cz)) { SetChunkDirty(cx, cy - 1, cz, true, false); }
		if (IsValidChunkPos(cx, cy + 1, cz)) { SetChunkDirty(cx, cy + 1, cz, true, false); }
		if (IsValidChunkPos(cx, cy, cz - 1)) { SetChunkDirty(cx, cy, cz - 1, true, false); }
		if (IsValidChunkPos(cx, cy, cz + 1)) { SetChunkDirty(cx, cy, cz + 1, true, false); }
	}

	public void SetMapPortion(int x, int y, int z, int[] chunk, int sizeX, int sizeY, int sizeZ)
	{
		int chunksizex = sizeX;
		int chunksizey = sizeY;
		int chunksizez = sizeZ;
		//if (chunksizex % chunksize != 0) { platform.ThrowException(""); }
		//if (chunksizey % chunksize != 0) { platform.ThrowException(""); }
		//if (chunksizez % chunksize != 0) { platform.ThrowException(""); }
		int chunksize = Game.chunksize;
		Chunk[] localchunks = new Chunk[(chunksizex / chunksize) * (chunksizey / chunksize) * (chunksizez / chunksize)];
		for (int cx = 0; cx < chunksizex / chunksize; cx++)
		{
			for (int cy = 0; cy < chunksizey / chunksize; cy++)
			{
				for (int cz = 0; cz < chunksizex / chunksize; cz++)
				{
					localchunks[Index3d(cx, cy, cz, (chunksizex / chunksize), (chunksizey / chunksize))] = GetChunk(x + cx * chunksize, y + cy * chunksize, z + cz * chunksize);
					FillChunk(localchunks[Index3d(cx, cy, cz, (chunksizex / chunksize), (chunksizey / chunksize))], chunksize, cx * chunksize, cy * chunksize, cz * chunksize, chunk, sizeX, sizeY, sizeZ);
				}
			}
		}
		for (int xxx = 0; xxx < chunksizex; xxx += chunksize)
		{
			for (int yyy = 0; yyy < chunksizex; yyy += chunksize)
			{
				for (int zzz = 0; zzz < chunksizex; zzz += chunksize)
				{
					SetChunkDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize, true, true);
					SetChunksAroundDirty((x + xxx) / chunksize, (y + yyy) / chunksize, (z + zzz) / chunksize);
				}
			}
		}
	}

	public void FillChunk(Chunk destination, int destinationchunksize, int sourcex, int sourcey, int sourcez, int[] source, int sourcechunksizeX, int sourcechunksizeY, int sourcechunksizeZ)
	{
		for (int x = 0; x < destinationchunksize; x++)
		{
			for (int y = 0; y < destinationchunksize; y++)
			{
				for (int z = 0; z < destinationchunksize; z++)
				{
					//if (x + sourcex < source.GetUpperBound(0) + 1
					//    && y + sourcey < source.GetUpperBound(1) + 1
					//    && z + sourcez < source.GetUpperBound(2) + 1)
					{
						destination.SetBlockInChunk(Index3d(x, y, z, destinationchunksize, destinationchunksize)
							, source[Index3d(x + sourcex, y + sourcey, z + sourcez, sourcechunksizeX, sourcechunksizeY)]);
					}
				}
			}
		}
	}

	public int MaybeGetLight(int x, int y, int z)
	{
		int light = -1;
		int cx = x / Game.chunksize;
		int cy = y / Game.chunksize;
		int cz = z / Game.chunksize;
		if (IsValidPos(x, y, z) && IsValidChunkPos(cx, cy, cz))
		{
			Chunk c = chunks[MapUtilCi.Index3d(cx, cy, cz, mapsizexchunks(), mapsizeychunks())];
			if (c == null
				|| c.rendered == null
				|| c.rendered.light == null)
			{
				light = -1;
			}
			else
			{
				light = c.rendered.light[MapUtilCi.Index3d((x % Game.chunksize) + 1, (y % Game.chunksize) + 1, (z % Game.chunksize) + 1, Game.chunksize + 2, Game.chunksize + 2)];
			}
		}
		return light;
	}

	public void SetBlockDirty(int x, int y, int z)
	{
		Vector3IntRef[] around = ModDrawTerrain.BlocksAround7(Vector3IntRef.Create(x, y, z));
		for (int i = 0; i < 7; i++)
		{
			Vector3IntRef a = around[i];
			int xx = a.X;
			int yy = a.Y;
			int zz = a.Z;
			if (xx < 0 || yy < 0 || zz < 0 || xx >= MapSizeX || yy >= MapSizeY || zz >= MapSizeZ)
			{
				return;
			}
			SetChunkDirty((xx / Game.chunksize), (yy / Game.chunksize), (zz / Game.chunksize), true, true);
		}
	}

	public bool IsChunkRendered(int cx, int cy, int cz)
	{
		Chunk c = chunks[MapUtilCi.Index3d(cx, cy, cz, mapsizexchunks(), mapsizeychunks())];
		if (c == null)
		{
			return false;
		}
		return c.rendered != null && c.rendered.ids != null;
	}
}
