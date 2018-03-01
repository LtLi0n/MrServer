using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Models.Guilds;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Models
{
    public class SocketGuild : Guild
    {
        public SocketGuild(Guild guild)
        {
            DefaultChannelID = guild.DefaultChannelID;
            ID = guild.ID;
            Name = guild.Name;
            Owner = guild.Owner;
        }
    }
}
