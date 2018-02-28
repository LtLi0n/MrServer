using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Commands.Permissions
{
    class RoleCommandPermission : CommandPermission
    {
        public ulong roleID;

        public RoleCommandPermission(ulong roleID) => this.roleID = roleID;
    }
}
