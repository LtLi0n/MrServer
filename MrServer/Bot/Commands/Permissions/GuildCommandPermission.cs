using MrServerPackets.Discord.Models.Guilds;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Commands.Permissions
{
    class GuildCommandPermission : CommandPermission
    {
        public GuildPermission guildPerms;

        public GuildCommandPermission(GuildPermission guildPerms) => this.guildPerms = guildPerms;
    }
}
