using MrServer.SQL.Osu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MrServer.SQL
{
    public abstract class SQL_DB
    {
        protected SQLiteConnection db_Connection;

        public SQL_DB(string path) => db_Connection = new SQLiteConnection($"Data Source={path};Version=3;");

        public virtual void Open() => db_Connection.Open();
        public virtual void Close() => db_Connection.Close();

        ///<summary> Default = ExecuteNonQueryAsync </summary>
        public virtual async Task ExecuteAsync(string command) => await new SQLiteCommand(command, db_Connection).ExecuteNonQueryAsync();
    }
}
