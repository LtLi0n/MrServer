using MrServer.SQL.Management;
using MrServer.SQL.Osu;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.SQL
{
    public class DataBases
    {
        public OsuSQL OsuDB { get; private set; }
        public CustomGuildSQL GuildDB { get; private set; }

        public DataBases()
        {
            OsuDB = new OsuSQL("DataBases/osu.sqlite");
            GuildDB = new CustomGuildSQL("DataBases/custom_guilds.sqlite");
        }

        public async Task Open()
        {
            await OsuDB.Open();
            await GuildDB.Open();
        }
    }
}
