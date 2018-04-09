using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace ManicDigger.Server
{
	/// <summary>
	/// Chunk storage using SQLite to store data
	/// </summary>
	public class ChunkDbSqlite : IChunkDb
	{
		SQLiteConnection sqliteConn;
		string databasefile;
		public void Open(string filename)
		{
			if (sqliteConn != null)
			{
				// close existing connections before opening a new one
				Close();
			}
			databasefile = filename;
			bool newdatabase = false;
			if (!File.Exists(databasefile))
			{
				newdatabase = true;
			}
			StringBuilder b = new StringBuilder();
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Data Source", databasefile);
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Version", "3");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "New", "True");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Compress", "True");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Journal Mode", "Off");
			sqliteConn = new SQLiteConnection(b.ToString());
			sqliteConn.Open();
			if (newdatabase)
			{
				CreateTables(sqliteConn);
			}
			if (!integrityCheck(sqliteConn))
			{
				Console.WriteLine("Database is possibly corrupted.");
				//repair(sqliteConn);
			}
			//vacuum(sqliteBckConn);
		}
		public void Close()
		{
			sqliteConn.Close();
			sqliteConn.Dispose();
		}
		private void CreateTables(SQLiteConnection sqliteConn)
		{
			SQLiteCommand sqlite_cmd;
			sqlite_cmd = sqliteConn.CreateCommand();
			sqlite_cmd.CommandText = "CREATE TABLE chunks (position integer PRIMARY KEY, data BLOB);";
			sqlite_cmd.ExecuteNonQuery();
		}
		public void Backup(string backupFilename)
		{
			if (databasefile == backupFilename)
			{
				Console.WriteLine(string.Format("Cannot overwrite current running database. Chose another destination."));
				return;
			}
			if (File.Exists(backupFilename))
			{
				Console.WriteLine(string.Format("File {0} exists. Overwriting file.", backupFilename));
			}
			StringBuilder b = new StringBuilder();
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Data Source", backupFilename);
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Version", "3");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "New", "True");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Compress", "True");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Journal Mode", "Off");
			SQLiteConnection sqliteBckConn = new SQLiteConnection(b.ToString());
			sqliteBckConn.Open();
			sqliteConn.BackupDatabase(sqliteBckConn, sqliteBckConn.Database, sqliteConn.Database, -1, null, 10);
			// shrink database
			vacuum(sqliteBckConn);
			sqliteBckConn.Close();
			sqliteBckConn.Dispose();
		}
		private void vacuum(SQLiteConnection sqliteConn)
		{
			using (SQLiteCommand command = sqliteConn.CreateCommand())
			{
				command.CommandText = "vacuum;";
				command.ExecuteNonQuery();
			}
		}
		private bool integrityCheck(SQLiteConnection sqliteConn)
		{
			bool okay = false;
			using (SQLiteCommand command = sqliteConn.CreateCommand())
			{
				command.CommandText = "PRAGMA integrity_check";
				SQLiteDataReader sqlite_datareader = command.ExecuteReader();
				Console.WriteLine(string.Format("Database: {0}. Running SQLite integrity check:", sqliteConn.DataSource));
				while (sqlite_datareader.Read())
				{
					Console.WriteLine(sqlite_datareader[0].ToString());
					if (sqlite_datareader[0].ToString() == "ok")
					{
						okay = true;
						break;
					}
				}
			}
			return okay;
		}
		private void repair(SQLiteConnection sqliteConn)
		{
			Console.WriteLine(string.Format("Database: {0}. Repairing database:", sqliteConn.DataSource));
			/*
	        SQLiteCommand cmd = sqliteConn.CreateCommand();
	        cmd.CommandText = "SELECT data FROM chunks";
	        SQLiteDataReader sqlite_datareader = cmd.ExecuteReader();
	        while (sqlite_datareader.Read())
	        {
	            object data = sqlite_datareader["data"];
	            //return data as byte[];
	        }
	        */
		}
		public IEnumerable<byte[]> GetChunks(IEnumerable<Xyz> chunkpositions)
		{
			using (SQLiteTransaction transaction = sqliteConn.BeginTransaction())
			{
				foreach (var xyz in chunkpositions)
				{
					ulong pos = MapUtil.ToMapPos(xyz.X, xyz.Y, xyz.Z);
					yield return GetChunk(pos);
				}
				transaction.Commit();
			}
		}




		public byte[] GetChunk(ulong position)
		{
			if (ReadOnly)
			{
				if (temporaryChunks.ContainsKey(position))
				{
					return temporaryChunks[position];
				}
			}
			SQLiteCommand cmd = sqliteConn.CreateCommand();
			cmd.CommandText = "SELECT data FROM chunks WHERE position=?";
			cmd.Parameters.Add(CreateParameter("position", DbType.UInt64, position, cmd));
			SQLiteDataReader sqlite_datareader = cmd.ExecuteReader();
			while (sqlite_datareader.Read())
			{
				object data = sqlite_datareader["data"];
				return data as byte[];
			}
			return null;
		}
		public void DeleteChunks(IEnumerable<Xyz> chunkpositions)
		{
			using (SQLiteTransaction transaction = sqliteConn.BeginTransaction())
			{
				foreach (var xyz in chunkpositions)
				{
					DeleteChunk(MapUtil.ToMapPos(xyz.X, xyz.Y, xyz.Z));
				}
				transaction.Commit();
			}
		}
		public void DeleteChunk(ulong position)
		{
			if (ReadOnly)
			{
				temporaryChunks.Remove(position);
				return;
			}
			DbCommand cmd = sqliteConn.CreateCommand();
			cmd.CommandText = "DELETE FROM chunks WHERE position=?";
			cmd.Parameters.Add(CreateParameter("position", DbType.UInt64, position, cmd));
			cmd.ExecuteNonQuery();
		}
		public void SetChunks(IEnumerable<DbChunk> chunks)
		{
			if (ReadOnly)
			{
				foreach (DbChunk c in chunks)
				{
					ulong pos = MapUtil.ToMapPos(c.Position.X, c.Position.Y, c.Position.Z);
					temporaryChunks[pos] = (byte[])c.Chunk.Clone();
				}
				return;
			}
			using (SQLiteTransaction transaction = sqliteConn.BeginTransaction())
			{
				foreach (DbChunk c in chunks)
				{
					ulong pos = MapUtil.ToMapPos(c.Position.X, c.Position.Y, c.Position.Z);
					InsertChunk(pos, c.Chunk);
				}
				transaction.Commit();
			}
		}


		public Dictionary<Xyz, byte[]> GetChunksFromFile(IEnumerable<Xyz> chunkpositions, string filename)
		{
			Dictionary<Xyz, byte[]> chunks = new Dictionary<Xyz, byte[]>();
			if (!File.Exists(filename))
			{
				Console.WriteLine(string.Format("File {0} does not exist.", filename));
				return null;
			}
			StringBuilder b = new StringBuilder();
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Data Source", filename);
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Version", "3");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "New", "True");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Compress", "True");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Journal Mode", "Off");
			SQLiteConnection conn = new SQLiteConnection(b.ToString());
			conn.Open();
			using (SQLiteTransaction transaction = conn.BeginTransaction())
			{
				foreach (var xyz in chunkpositions)
				{
					ulong pos = MapUtil.ToMapPos(xyz.X, xyz.Y, xyz.Z);
					chunks.Add(xyz, GetChunkFromFile(pos, conn));
				}
				transaction.Commit();
				conn.Close();
				conn.Dispose();
			}
			return chunks;
		}
		private byte[] GetChunkFromFile(ulong position, SQLiteConnection conn)
		{
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT data FROM chunks WHERE position=?";
			cmd.Parameters.Add(CreateParameter("position", DbType.UInt64, position, cmd));
			SQLiteDataReader sqlite_datareader = cmd.ExecuteReader();
			while (sqlite_datareader.Read())
			{
				object data = sqlite_datareader["data"];
				return data as byte[];
			}
			return null;
		}
		public void SetChunksToFile(IEnumerable<DbChunk> chunks, string filename)
		{
			bool newDatabase = true;
			if (databasefile == filename)
			{
				Console.WriteLine(string.Format("Cannot overwrite current running database. Chose another destination."));
				return;
			}

			if (File.Exists(filename))
			{
				Console.WriteLine(string.Format("File {0} exists. Overwriting file.", filename));
				newDatabase = false;
			}

			StringBuilder b = new StringBuilder();
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Data Source", filename);
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Version", "3");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "New", "True");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Compress", "True");
			DbConnectionStringBuilder.AppendKeyValuePair(b, "Journal Mode", "Off");
			SQLiteConnection sqliteConn = new SQLiteConnection(b.ToString());
			sqliteConn.Open();

			if (newDatabase)
			{
				CreateTables(sqliteConn);
			}

			using (SQLiteTransaction transaction = sqliteConn.BeginTransaction())
			{
				foreach (DbChunk c in chunks)
				{
					ulong pos = MapUtil.ToMapPos(c.Position.X, c.Position.Y, c.Position.Z);
					DbCommand cmd = sqliteConn.CreateCommand();
					cmd.CommandText = "INSERT OR REPLACE INTO chunks (position, data) VALUES (?,?)";
					cmd.Parameters.Add(CreateParameter("position", DbType.UInt64, pos, cmd));
					cmd.Parameters.Add(CreateParameter("data", DbType.Object, c.Chunk, cmd));
					cmd.ExecuteNonQuery();
				}
				transaction.Commit();
			}
			sqliteConn.Close();
			sqliteConn.Dispose();

		}


		//when read only don't save this to disk
		public Dictionary<ulong, byte[]> temporaryChunks = new Dictionary<ulong, byte[]>();
		void InsertChunk(ulong position, byte[] data)
		{
			DbCommand cmd = sqliteConn.CreateCommand();
			cmd.CommandText = "INSERT OR REPLACE INTO chunks (position, data) VALUES (?,?)";
			cmd.Parameters.Add(CreateParameter("position", DbType.UInt64, position, cmd));
			cmd.Parameters.Add(CreateParameter("data", DbType.Object, data, cmd));
			cmd.ExecuteNonQuery();
		}
		DbParameter CreateParameter(string parameterName, DbType dbType, object value, DbCommand command)
		{
			DbParameter p = command.CreateParameter();
			p.ParameterName = parameterName;
			p.DbType = dbType;
			p.Value = value;
			return p;
		}
		public byte[] GetGlobalData()
		{
			try
			{
				return GetChunk(ulong.MaxValue / 2);
			}
			catch
			{
				return null;
			}
		}
		public void SetGlobalData(byte[] data)
		{
			InsertChunk(ulong.MaxValue / 2, data);
		}

		bool ReadOnly;
		public bool GetReadOnly()
		{
			return ReadOnly;
		}
		public void SetReadOnly(bool value)
		{
			ReadOnly = value;
		}
	}
}
