using ManicDigger.Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;

namespace ManicDigger.Server
{
	partial class Server
	{
		public int Seed;
		public int LastMonsterId;

		public List<ManicDigger.Action> onload = new List<ManicDigger.Action>();
		public List<ManicDigger.Action> onsave = new List<ManicDigger.Action>();

		public Dictionary<string, PacketServerInventory> Inventory = new Dictionary<string, PacketServerInventory>(StringComparer.InvariantCultureIgnoreCase);
		public Dictionary<string, PacketServerPlayerStats> PlayerStats = new Dictionary<string, PacketServerPlayerStats>(StringComparer.InvariantCultureIgnoreCase);
		public Dictionary<string, byte[]> moddata = new Dictionary<string, byte[]>();

		private void LoadSavegame(ServerMap d_Map, string filename)
		{
			// open save file
			d_Map.d_ChunkDb.Open(filename);
			d_Map.Reset(d_Map.MapSizeX, d_Map.MapSizeY, d_Map.MapSizeZ);

			// process save metadata
			byte[] globaldata = d_Map.d_ChunkDb.GetGlobalData();
			if (globaldata == null)
			{
				// no metadata present. initialize new.
				if (config.RandomSeed)
				{
					Seed = new Random().Next();
				}
				else
				{
					Seed = config.Seed;
				}
				MemoryStream ms = new MemoryStream();
				SerializeGameMetadata(ms);
				d_Map.d_ChunkDb.SetGlobalData(ms.ToArray());
				this._time.Init(TimeSpan.Parse("08:00").Ticks);
				return;
			}

			// load existing metadata
			ManicDiggerSave save = Serializer.Deserialize<ManicDiggerSave>(new MemoryStream(globaldata));
			Seed = save.Seed;
			if (config.IsCreative)
			{
				this.Inventory = new Dictionary<string, PacketServerInventory>(StringComparer.InvariantCultureIgnoreCase);
			}
			else
			{
				this.Inventory = save.Inventory;
			}
			this.PlayerStats = save.PlayerStats;
			this.simulationcurrentframe = (int)save.SimulationCurrentFrame;
			this._time.Init(save.TimeOfDay);
			this.LastMonsterId = save.LastMonsterId;
			this.moddata = save.moddata;
		}

		private void SaveGameMetadata()
		{
			MemoryStream ms = new MemoryStream();
			SerializeGameMetadata(ms);
			d_Map.d_ChunkDb.SetGlobalData(ms.ToArray());
		}

		public void SerializeGameMetadata(Stream s)
		{
			for (int i = 0; i < onsave.Count; i++)
			{
				onsave[i]();
			}
			ManicDiggerSave save = new ManicDiggerSave();
			if (!config.IsCreative)
			{
				save.Inventory = Inventory;
			}
			save.PlayerStats = PlayerStats;
			save.Seed = Seed;
			save.SimulationCurrentFrame = simulationcurrentframe;
			save.TimeOfDay = _time.Time.Ticks;
			save.LastMonsterId = LastMonsterId;
			save.moddata = moddata;
			Serializer.Serialize(s, save);
		}

		public bool BackupDatabase(string backupFilename)
		{
			if (!GameStorePath.IsValidName(backupFilename))
			{
				Console.WriteLine(language.ServerInvalidBackupName() + backupFilename);
				return false;
			}
			if (!Directory.Exists(GameStorePath.gamepathbackup))
			{
				Directory.CreateDirectory(GameStorePath.gamepathbackup);
			}
			string finalFilename = Path.Combine(GameStorePath.gamepathbackup, backupFilename + MapManipulator.BinSaveExtension);
			d_Map.d_ChunkDb.Backup(finalFilename);
			return true;
		}

		public bool LoadDatabase(string filename)
		{
			// save all pending changes
			ProcessSave(true);
			if (filename != GetSaveFilename())
			{
				//TODO: load
			}
			// reset temporary chunks
			var dbcompressed = (ChunkDbCompressed)d_Map.d_ChunkDb;
			var db = (ChunkDbSqlite)dbcompressed.d_ChunkDb;
			db.temporaryChunks = new Dictionary<ulong, byte[]>();
			// clear all currently loaded chunks
			d_Map.Clear();
			// load new file
			LoadSavegame(d_Map, filename);
			foreach (var k in clients)
			{
				//SendLevelInitialize(k.Key);
				Array.Clear(k.Value.chunksseen, 0, k.Value.chunksseen.Length);
				k.Value.chunksseenTime.Clear();
			}
			return true;
		}

		private void SaveAllLoadedChunks()
		{
			List<DbChunk> tosave = new List<DbChunk>();
			for (int cx = 0; cx < d_Map.MapSizeX / chunksize; cx++)
			{
				for (int cy = 0; cy < d_Map.MapSizeY / chunksize; cy++)
				{
					for (int cz = 0; cz < d_Map.MapSizeZ / chunksize; cz++)
					{
						// Load chunk
						ServerChunk c = d_Map.GetChunkValid(cx, cy, cz);
						if (c == null)
						{
							continue;
						}
						if (!c.DirtyForSaving)
						{
							continue;
						}
						c.DirtyForSaving = false;

						// Serialize chunk and queue for saving
						MemoryStream ms = new MemoryStream();
						Serializer.Serialize(ms, c);
						tosave.Add(new DbChunk() { Position = new Xyz() { X = cx, Y = cy, Z = cz }, Chunk = ms.ToArray() });

						// Create batches containing a maximum of 200 chunks
						if (tosave.Count > 200)
						{
							d_Map.d_ChunkDb.SetChunks(tosave);
							tosave.Clear();
						}
					}
				}
			}
			// Send chunks to storage backend
			d_Map.d_ChunkDb.SetChunks(tosave);
		}

		public string SaveFilenameWithoutExtension = "default";
		public string SaveFilenameOverride;
		public string GetSaveFilename()
		{
			if (SaveFilenameOverride != null)
			{
				return SaveFilenameOverride;
			}
			return Path.Combine(GameStorePath.gamepathsaves, SaveFilenameWithoutExtension + MapManipulator.BinSaveExtension);
		}

		DateTime lastsave = DateTime.UtcNow;
		public void ProcessSave(bool forceSave = false)
		{
			if ((DateTime.UtcNow - lastsave).TotalMinutes > 2 || forceSave)
			{
				DateTime start = DateTime.UtcNow;
				SaveAllLoadedChunks();
				SaveGameMetadata();
				Console.WriteLine(language.ServerGameSaved(), (DateTime.UtcNow - start));
				lastsave = DateTime.UtcNow;
			}
		}

		internal void DoSaveChunk(int x, int y, int z, ServerChunk c)
		{
			MemoryStream ms = new MemoryStream();
			Serializer.Serialize(ms, c);
			ChunkDb.SetChunk(d_Map.d_ChunkDb, x, y, z, ms.ToArray());
		}
	}
}
