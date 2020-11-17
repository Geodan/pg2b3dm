using System;
using System.Collections.Generic;
using Npgsql;

namespace B3dm.Tileset
{
    public static class LodsRepository
    {
        public static List<int> GetLods(NpgsqlConnection conn, string geometryTable, string lodcolumn, string query="")
        {
            var where = (query != string.Empty ? $"where {query}" : String.Empty);
            var res = new List<int>();
            var sql = $"select distinct({lodcolumn}) from {geometryTable} {where} order by {lodcolumn}";
            conn.Open();
            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                var id = reader.GetInt32(0);
                res.Add(id);
            }

            reader.Close();
            conn.Close();
            return res;
        }

    }
}
