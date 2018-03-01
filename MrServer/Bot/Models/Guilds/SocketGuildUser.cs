using MrServerPackets.Discord.Interfaces;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Models.Guilds;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Models
{
    public class SocketGuildUser : SocketUser, IGuildUser
    {
        public SocketGuildUser(GuildUser guildUser) : base(guildUser)
        {
            RoleIDs = guildUser.RoleIDs;
            Nickname = guildUser.Nickname;
            Permissions = guildUser.Permissions;
        }

        public ulong[] RoleIDs { get; set; }
        public string Nickname { get; set; }
        public GuildPermission Permissions { get; set; }
    }
}
