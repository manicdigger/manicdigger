using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;
using System.Data.Common;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace GameModeFortress
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
    public class ChunkDb
    {
        public static byte[] GetChunk(IChunkDb db, int x, int y, int z)
        {
            List<byte[]> chunks = new List<byte[]>(db.GetChunks(new Xyz[] { new Xyz() { X = x, Y = y, Z = z } }));
            if (chunks.Count > 1)
            {
                throw new Exception();
            }
            if (chunks.Count == 0)
            {
                return null;
            }
            return chunks[0];
        }
        public static void SetChunk(IChunkDb db, int x, int y, int z, byte[] c)
        {
            db.SetChunks(new DbChunk[] { new DbChunk() { Position = new Xyz() { X = x, Y = y, Z = z }, Chunk = c } });
        }
    }
    public interface IChunkDb
    {
        void Open(string filename);
        IEnumerable<byte[]> GetChunks(IEnumerable<Xyz> chunkpositions);
        void SetChunks(IEnumerable<DbChunk> chunks);
        byte[] GetGlobalData();
        void SetGlobalData(byte[] data);
    }
    public class ChunkDbCompressed : IChunkDb
    {
        [Inject]
        public IChunkDb d_ChunkDb;
        [Inject]
        public ICompression d_Compression;
        #region IChunkDb Members
        public void Open(string filename)
        {
            d_ChunkDb.Open(filename);
        }
        public IEnumerable<byte[]> GetChunks(IEnumerable<Xyz> chunkpositions)
        {
            foreach(byte[] b in d_ChunkDb.GetChunks(chunkpositions))
            {
                if (b == null)
                {
                    yield return null;
                }
                else
                {
                    yield return d_Compression.Decompress(b);
                }
            }
        }
        public void SetChunks(IEnumerable<DbChunk> chunks)
        {
            List<DbChunk> compressed = new List<DbChunk>();
            foreach (DbChunk c in chunks)
            {
                byte[] b;
                if (c.Chunk == null)
                {
                    b = null;
                }
                else
                {
                    b = d_Compression.Compress(c.Chunk);
                }
                compressed.Add(new DbChunk() { Position = c.Position, Chunk = b });
            }
            d_ChunkDb.SetChunks(compressed);
        }
        public byte[] GetGlobalData()
        {
            byte[] globaldata = d_ChunkDb.GetGlobalData();
            if (globaldata == null)
            {
                return null;
            }
            else
            {
                return d_Compression.Decompress(globaldata);
            }
        }
        public void SetGlobalData(byte[] data)
        {
            if (data == null)
            {
                d_ChunkDb.SetGlobalData(null);
            }
            else
            {
                d_ChunkDb.SetGlobalData(d_Compression.Compress(data));
            }
        }
        #endregion
    }
    public class ChunkDbDummy : IChunkDb
    {
        string currentFilename = null;
        Dictionary<Xyz, byte[]> chunks = new Dictionary<Xyz, byte[]>();
        #region IChunkDb Members
        public void Open(string filename)
        {
            if (filename != currentFilename)
            {
                currentFilename = filename;
                chunks.Clear();
            }
        }
        public IEnumerable<byte[]> GetChunks(IEnumerable<Xyz> chunkpositions)
        {
            foreach (Xyz pos in chunkpositions)
            {
                byte[] c;
                if (chunks.TryGetValue(pos, out c))
                {
                    yield return c;
                }
                else
                {
                    yield return null;
                }
            }
        }
        public void SetChunks(IEnumerable<DbChunk> chunks)
        {
            foreach (DbChunk c in chunks)
            {
                this.chunks[c.Position] = c.Chunk;
            }
        }
        byte[] globaldata = null;
        public byte[] GetGlobalData()
        {
            return globaldata;
        }
        public void SetGlobalData(byte[] data)
        {
            globaldata = data;
        }
        #endregion
    }
    public class ChunkDbSqlite : IChunkDb
    {
        SQLiteConnection sqliteConn;
        public void Open(string filename)
        {
            string databasefile = filename;
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
                CreateTables();
            }
        }
        public void Close()
        {
            sqliteConn.Close();
        }
        private void CreateTables()
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqliteConn.CreateCommand();
            sqlite_cmd.CommandText = "CREATE TABLE chunks (position integer PRIMARY KEY, data BLOB);";
            sqlite_cmd.ExecuteNonQuery();
        }
        #region IChunkDb Members
        public IEnumerable<byte[]> GetChunks(IEnumerable<Xyz> chunkpositions)
        {
            using (SQLiteTransaction transaction = sqliteConn.BeginTransaction())
            {
                foreach (var xyz in chunkpositions)
                {
                    ulong pos = ManicDigger.MapUtil.ToMapPos(xyz.X, xyz.Y, xyz.Z);
                    yield return GetChunk(pos);
                }
                transaction.Commit();
            }
        }
        public byte[] GetChunk(ulong position)
        {
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
        public void SetChunks(IEnumerable<DbChunk> chunks)
        {
            using (SQLiteTransaction transaction = sqliteConn.BeginTransaction())
            {
                foreach (DbChunk c in chunks)
                {
                    ulong pos = ManicDigger.MapUtil.ToMapPos(c.Position.X, c.Position.Y, c.Position.Z);
                    InsertChunk(pos, c.Chunk);
                }
                transaction.Commit();
            }
        }
        #endregion
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
        #region IChunkDb Members
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
        #endregion
    }
}
