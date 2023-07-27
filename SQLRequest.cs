using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyHomie
{
    internal class SQLRequest
    {
        public static DataTable SQLite(string sqlQuery)
        {
            try
            {
                var connect = new SQLiteConnection("DataSource=casino.db");
                connect.Open();
                DataTable dt = new DataTable();
                var adapter = new SQLiteDataAdapter(sqlQuery, connect);
                adapter.Fill(dt);

                connect.Close();
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public static void InsertSQL(List<BGRankStruct> list, int server)
        {
            using (var con = new SQLiteConnection("DataSource=casino.db"))
            {
                con.Open();

                using (var cmd = new SQLiteCommand(con))
                {
                    using (var transaction = con.BeginTransaction())
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            cmd.CommandText = @$"INSERT INTO {(server == 0 ? "HSBGeu" : "HSBGus")}
                                                 VALUES ('{list[i].accountid}', {list[i].rank}, {list[i].rating})";
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                con.Close();
            }
        }
    }
}
