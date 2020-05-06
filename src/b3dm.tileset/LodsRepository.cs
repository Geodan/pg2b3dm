using System;
using System.Collections.Generic;
using Npgsql;

namespace B3dm.Tileset
{
    public static class LodsRepository
    {
        public static List<int> GetLods(NpgsqlConnection conn, string geometryTable, string lodcolumn)
        {
            var res = new List<int>();
            var sql = $"select distinct({lodcolumn}) from {geometryTable} order by {lodcolumn}";

            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                var id = reader.GetInt32(0);
                res.Add(id);
            }

            reader.Close();
            return res;
        }

    }
}
