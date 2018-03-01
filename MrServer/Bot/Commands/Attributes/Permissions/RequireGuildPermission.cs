using MrServerPackets.Discord.Models.Guilds;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Commands.Attributes.Permissions
{
    class RequireGuildPermission : PermissionAttribute
    {
        public GuildPermission GuildPermission { get; private set; }

        public RequireGuildPermission(GuildPermission GuildPermission) => this.GuildPermission = GuildPermission;
    }
}
