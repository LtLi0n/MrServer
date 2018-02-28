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

        public DataBases()
        {
            OsuDB = new OsuSQL("DataBases/osu.sqlite");
        }

        public async Task Open()
        {
            await OsuDB.Open();
        }
    }
}
