using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.SQL.Management
{
    public class CustomGuildSQL : SQL_DB
    {
        public CustomGuildSQL(string path) : base(path)
        {

        }

        public async Task SavePrefix(ulong guildID, string prefix, string defaultPrefix)
        {
            string check = await GetCustomPrefix(guildID, defaultPrefix);

            if (prefix == defaultPrefix) await new SQLiteCommand($"DELETE FROM prefixes WHERE ID = {guildID}", db_Connection).ExecuteNonQueryAsync();
            else if (check == defaultPrefix) await new SQLiteCommand($"INSERT INTO prefixes (Prefix, ID) VALUES ('{prefix}', '{guildID}')", db_Connection).ExecuteNonQueryAsync();
            else await new SQLiteCommand($"UPDATE prefixes SET Prefix = {prefix} WHERE ID = {guildID}", db_Connection).ExecuteNonQueryAsync();
        }

        public Task<string> GetCustomPrefix(ulong guildID, string defaultPrefix)
        {
            string command = $"SELECT Prefix FROM prefixes WHERE ID = {guildID}";

            using (SQLiteDataReader reader = new SQLiteCommand(command, db_Connection).ExecuteReader())
            {
                string prefix = defaultPrefix;

                if (reader.Read()) prefix = reader["Prefix"].ToString();

                return Task.FromResult(prefix);
            }
        }
    }
}
