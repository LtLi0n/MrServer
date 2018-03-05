using MrServer.Network;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Models.Guilds;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Models
{
    public class SocketGuild : Guild
    {
        protected NetworkHandler _network;

        public SocketGuild(NetworkHandler network, Guild guild)
        {
            DefaultChannelID = guild.DefaultChannelID;
            ID = guild.ID;
            Name = guild.Name;
            Owner = guild.Owner;

            _network = network;
        }

        public Guild CommunicationGuild => new Guild()
        {
            DefaultChannelID = base.DefaultChannelID,
            ID = base.ID,
            Name = base.Name,
            Owner = base.Owner
        };
    }
}
