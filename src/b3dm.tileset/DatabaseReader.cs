using System.Data;

namespace B3dm.Tileset
{
    public static class DatabaseReader
    {
        public static int ReadScalar(IDbConnection conn, string sql)
        {
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText = sql;
            var reader = command.ExecuteReader();
            reader.Read();
            var scalar = reader.GetInt32(0);
            reader.Close();
            conn.Close();
            return scalar;
        }
    }
}
