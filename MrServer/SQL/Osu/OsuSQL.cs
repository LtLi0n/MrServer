using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using MrServerPackets.ApiStructures.Osu;
using System.Threading.Tasks;
using System.Data.Common;

using MrServerPackets.ApiStructures.Osu.Database;
using System.Timers;

namespace MrServer.SQL.Osu
{
    public class OsuSQL : SQL_DB
    {
        public static class ResetType
        {
            public interface ResetDaily { }
            public interface ResetWeekly { }
            public interface ResetMonthly { }
        }

        public const string table_name = "osu_users";

        private DateTime LastDailyReset;
        private DateTime LastWeeklyReset;
        private DateTime LastMonthlyReset;

        public const int ResetHour = 3; //03:00

        private Timer timer_1min;

        public OsuSQL(string path) : base(path)
        {
            timer_1min = new Timer(60000);
            timer_1min.Elapsed += Timer_1min_Elapsed;
            timer_1min.Start();
        }

        public new async Task Open()
        {
            db_Connection.Open();
            await Init();
        }

        private Task Init()
        {
            string command_str = $"SELECT Date FROM dates WHERE Type = 'Osu!'";

            using (SQLiteDataReader reader = new SQLiteCommand(command_str, db_Connection).ExecuteReader())
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!reader.Read()) break;

                    switch (i)
                    {
                        case 0: LastDailyReset = new DateTime(long.Parse(reader["Date"].ToString())); break;
                        case 1: LastWeeklyReset = new DateTime(long.Parse(reader["Date"].ToString())); break;
                        case 2: LastMonthlyReset = new DateTime(long.Parse(reader["Date"].ToString())); break;
                    }
                }
            }

            return Task.CompletedTask;
        }

        //Check if reset is up, reset everyone by <daily/weekly/monthly> if it is
        private async void Timer_1min_Elapsed(object sender, ElapsedEventArgs e)
        {
            ulong[] users_std_ID = await GetTrackedUsers(OsuGameModes.STD);
            ulong[] users_taiko_ID = await GetTrackedUsers(OsuGameModes.Taiko);
            ulong[] users_ctb_ID = await GetTrackedUsers(OsuGameModes.CtB);
            ulong[] users_mania_ID = await GetTrackedUsers(OsuGameModes.Mania);

            Console.WriteLine($"Updating osu! database - Users: [STD: {users_std_ID.Length}] | Taiko: {users_taiko_ID.Length} | CtB: {users_ctb_ID.Length} | Mania: {users_mania_ID.Length}");

            foreach (ulong userID in users_std_ID)
            {
                OsuUser osuUser = await Network.Osu.OsuNetwork.DownloadOsuUser(userID, 0, 3);
                if(osuUser != null) await WriteOsuGameModeUser(osuUser, OsuGameModes.STD);
            }

            foreach (ulong userID in users_taiko_ID)
            {
                OsuUser osuUser = await Network.Osu.OsuNetwork.DownloadOsuUser(userID, 1, 3);
                if (osuUser != null) await WriteOsuGameModeUser(osuUser, OsuGameModes.Taiko);
            }

            foreach (ulong userID in users_ctb_ID)
            {
                OsuUser osuUser = await Network.Osu.OsuNetwork.DownloadOsuUser(userID, 2, 3);
                if (osuUser != null) await WriteOsuGameModeUser(osuUser, OsuGameModes.CtB);
            }

            foreach (ulong userID in users_std_ID)
            {
                OsuUser osuUser = await Network.Osu.OsuNetwork.DownloadOsuUser(userID, 3, 3);
                if (osuUser != null) await WriteOsuGameModeUser(osuUser, OsuGameModes.Mania);
            }

            if (LastDailyReset.AddDays(1).Ticks < DateTime.Now.Ticks)
            {
                Console.WriteLine("Daily TotalHit data is being reset...");
                await ResetTotalHits<ResetType.ResetDaily>(users_std_ID, OsuGameModes.STD);
                await ResetTotalHits<ResetType.ResetDaily>(users_taiko_ID, OsuGameModes.Taiko);
                await ResetTotalHits<ResetType.ResetDaily>(users_ctb_ID, OsuGameModes.CtB);
                await ResetTotalHits<ResetType.ResetDaily>(users_mania_ID, OsuGameModes.Mania);
                Console.WriteLine("Daily TotalHit data has been resetted!");
            }
            if (LastWeeklyReset.AddDays(7).Ticks < DateTime.Now.Ticks)
            {
                Console.WriteLine("Weekly TotalHit data is being reset...");
                await ResetTotalHits<ResetType.ResetWeekly>(users_std_ID, OsuGameModes.STD);
                await ResetTotalHits<ResetType.ResetWeekly>(users_taiko_ID, OsuGameModes.Taiko);
                await ResetTotalHits<ResetType.ResetWeekly>(users_ctb_ID, OsuGameModes.CtB);
                await ResetTotalHits<ResetType.ResetWeekly>(users_mania_ID, OsuGameModes.Mania);
                Console.WriteLine("Weekly TotalHit data has been resetted!");
            }
            if (LastMonthlyReset.AddDays(30).Ticks < DateTime.Now.Ticks)
            {
                Console.WriteLine("Monthly TotalHit data is being reset...");
                await ResetTotalHits<ResetType.ResetMonthly>(users_std_ID, OsuGameModes.STD);
                await ResetTotalHits<ResetType.ResetMonthly>(users_taiko_ID, OsuGameModes.Taiko);
                await ResetTotalHits<ResetType.ResetMonthly>(users_ctb_ID, OsuGameModes.CtB);
                await ResetTotalHits<ResetType.ResetMonthly>(users_mania_ID, OsuGameModes.Mania);
                Console.WriteLine("Monthly TotalHit data has been resetted!");
            }
        }

        private Task<ulong[]> GetTrackedUsers(OsuGameModes gameMode)
        {
            List<ulong> users = new List<ulong>();
            {
                string command_read_str = $"SELECT UserID, GameModes FROM {table_name}";

                SQLiteCommand command_read = new SQLiteCommand(command_read_str, db_Connection);

                using (SQLiteDataReader reader = command_read.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if(((OsuGameModes) byte.Parse(reader["GameModes"].ToString())).HasFlag(gameMode))
                        {
                            users.Add(ulong.Parse(reader["UserID"].ToString()));
                        }
                    }
                }
            }

            return Task.FromResult(users.ToArray());
        }

        //Reset TotalHit count to 0 for daily/weekly/monthly leaderboard
        private async Task ResetTotalHits<T>(ulong[] usersID, OsuGameModes gameMode)
        {
            string command_update_user_str = $"UPDATE {table_name}_{GameModeString(gameMode)} SET ";

            if (typeof(T) == typeof(ResetType.ResetDaily))
            {
                LastDailyReset = DateTime.Now.Subtract(DateTime.Now.TimeOfDay).AddHours(ResetHour);
                await new SQLiteCommand($"UPDATE Dates SET Date = '{LastDailyReset.Ticks}' WHERE ID = '1'", db_Connection).ExecuteNonQueryAsync();
                command_update_user_str += "HitsDaily";
            }
            else if (typeof(T) == typeof(ResetType.ResetWeekly))
            {
                LastWeeklyReset = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-((int)(DateTime.Now.DayOfWeek + 6) % 7)).AddHours(ResetHour);
                await new SQLiteCommand($"UPDATE Dates SET Date = '{LastWeeklyReset.Ticks}' WHERE ID = '2'", db_Connection).ExecuteNonQueryAsync();
                command_update_user_str += "HitsWeekly";
            }
            else if (typeof(T) == typeof(ResetType.ResetMonthly))
            {
                LastMonthlyReset = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddHours(ResetHour);
                await new SQLiteCommand($"UPDATE Dates SET Date = '{LastMonthlyReset.Ticks}' WHERE ID = '3'", db_Connection).ExecuteNonQueryAsync();
                command_update_user_str += "HitsMonthly";
            }

            command_update_user_str += $"= '0' WHERE UserID = ";

            foreach (int userID in usersID) await new SQLiteCommand(command_update_user_str + $"'{userID}'", db_Connection).ExecuteNonQueryAsync();
        }

        public async Task<OsuGameModeUserDB> GetOsuGameModeUser(ulong DiscordID, OsuGameModes GameMode)
        {
            OsuBoundUserDB boundUser = await GetBoundUserBy_DiscordID(DiscordID);
            return boundUser != null ? await GetGameModeUserBy_OsuID(boundUser.UserID, GameMode) : null;
        }

        public async Task RemoveOsuGameModeUser(ulong userID, OsuGameModes gameMode = OsuGameModes.STD)
        {
            string command_str = $"DELETE FROM {table_name}_{GameModeString(gameMode)} WHERE UserID = '{userID}'";

            await new SQLiteCommand(command_str, db_Connection).ExecuteNonQueryAsync();
        }

        public async Task WriteOsuGameModeUser(OsuUser osuUser, OsuGameModes gameMode = OsuGameModes.STD)
        {
            OsuGameModeUserDB gameModeUser = await GetGameModeUserBy_OsuID(osuUser.UserID, gameMode);

            string cmd = string.Empty;

            if (gameModeUser != null)
            {
                await gameModeUser.Update(osuUser);

                cmd = $"UPDATE {table_name}_{GameModeString(gameMode)} SET " +
                    $"PP = '{gameModeUser.PP}'," +
                    $"TotalHits = '{gameModeUser.TotalHits}'," +
                    $"HitsDaily = '{gameModeUser.HitsDaily}'," +
                    $"HitsWeekly = '{gameModeUser.HitsWeekly}'," +
                    $"HitsMonthly = '{gameModeUser.HitsMonthly}'," +
                    $"HitsSince = '{gameModeUser.HitsSince}'" +
                    $"WHERE UserID = '{gameModeUser.UserID}'";
            }
            else
            {
                gameModeUser = new OsuGameModeUserDB(osuUser);

                cmd =
                    $"INSERT INTO {table_name}_{GameModeString(gameMode)} " +
                    $"(UserID, PP, TotalHits, HitsDaily, HitsWeekly, HitsMonthly, HitsSince) VALUES " +
                    $"('{gameModeUser.UserID}', '{gameModeUser.PP}', '{osuUser.TotalHits.Count_Total}', '{gameModeUser.HitsDaily}', '{gameModeUser.HitsWeekly}', '{gameModeUser.HitsMonthly}', '{gameModeUser.HitsSince}')";
            }
            try
            {
                await new SQLiteCommand(cmd, db_Connection).ExecuteNonQueryAsync();
            }
            catch(Exception e)
            {

            }
        }

        public async Task<OsuGameModeUserDB> GetGameModeUserBy_OsuID(ulong userID, OsuGameModes gameMode)
        {
            DbDataReader reader = await new SQLiteCommand($"SELECT * FROM {table_name}_{GameModeString(gameMode)} WHERE UserID = '{userID}'", db_Connection).ExecuteReaderAsync();

            if (reader.Read())
            {
                return new OsuGameModeUserDB()
                {
                    UserID = ulong.Parse(reader["UserID"].ToString()),
                    PP = float.Parse(reader["PP"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
                    TotalHits = int.Parse(reader["TotalHits"].ToString()),

                    HitsDaily = int.Parse(reader["HitsDaily"].ToString()),
                    HitsWeekly = int.Parse(reader["HitsWeekly"].ToString()),
                    HitsMonthly = int.Parse(reader["HitsMonthly"].ToString()),
                    HitsSince = int.Parse(reader["HitsSince"].ToString())
                };
            }
            else return null;
        }

        public async Task RemoveBoundUser(ulong DiscordID)
        {
            await new SQLiteCommand(
                $"DELETE FROM osu_users WHERE DiscordID = '{DiscordID}'"
                , db_Connection).ExecuteNonQueryAsync();
        }

        public async Task RegisterBoundOsuUser(OsuUser OsuUser, ulong DiscordID) =>
    await new SQLiteCommand(
            $"INSERT INTO {table_name} " +
            $"(DiscordID, UserID, UserName, GameModes, Country) " +
            $"VALUES ('{DiscordID}', '{OsuUser.UserID}', '{OsuUser.UserName}', '{(byte)OsuGameModes.None}', '{OsuUser.Country}')",
        db_Connection).ExecuteNonQueryAsync();

        public async Task UpdateBoundOsuUser(OsuUser osuUser, OsuBoundUserDB boundUser) =>
            await new SQLiteCommand(
                $"UPDATE {table_name} " +
                $"SET " +
                $"UserName = '{osuUser.UserName}'," +
                $"GameModes = '{(byte)boundUser.GameModes}'," +
                $"Country = '{osuUser.Country}' " +
                $"WHERE UserID = '{boundUser.UserID}'",
        db_Connection).ExecuteNonQueryAsync();

        public async Task UpdateBoundOsuUser(OsuBoundUserDB boundUser) =>
            await new SQLiteCommand(
                $"UPDATE {table_name} " +
                $"SET " +
                $"GameModes = '{(byte)boundUser.GameModes}' " +
                $"WHERE UserID = '{boundUser.UserID}'",
                db_Connection).ExecuteNonQueryAsync();

        public async Task<OsuBoundUserDB> GetBoundUserBy_DiscordID(ulong DiscordID) => await GetBoundUser(new SQLiteCommand($"select * from {table_name} where DiscordID = '{DiscordID}'", db_Connection));
        public async Task<OsuBoundUserDB> GetBoundUserBy_OsuID(ulong UserID) => await GetBoundUser(new SQLiteCommand($"select * from {table_name} where UserID = '{UserID}'", db_Connection));
        public async Task<OsuBoundUserDB> GetBoundUserBy_UserName(string UserName) => await GetBoundUser(new SQLiteCommand($"select * from {table_name} where UserName = '{UserName}' COLLATE NOCASE", db_Connection));

        private async Task<OsuBoundUserDB> GetBoundUser(SQLiteCommand command)
        {
            DbDataReader reader = await command.ExecuteReaderAsync();

            OsuBoundUserDB binder;

            if (reader.Read())
            {
                return binder = new OsuBoundUserDB()
                {
                    DiscordID = ulong.Parse(reader["DiscordID"].ToString()),
                    UserID = ulong.Parse(reader["UserID"].ToString()),
                    UserName = reader["UserName"].ToString(),
                    GameModes = (OsuGameModes)byte.Parse(reader["GameModes"].ToString()),
                    Country = reader["Country"].ToString()
                };
            }
            else return null;
        }

        public static string GameModeString(OsuGameModes GameMode) => Enum.GetName(typeof(OsuGameModes), GameMode).ToLower();

        public static string GameModeString(int GameMode)
        {
            switch (GameMode)
            {
                case 0: return "std";
                case 1: return "taiko";
                case 2: return "ctb";
                case 3: return "mania";
            }
            throw new ArgumentOutOfRangeException("Variable 'gamemode' can only range from 0 to 3.");
        }
    }
}
