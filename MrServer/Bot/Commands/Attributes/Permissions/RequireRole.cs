using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Commands.Attributes.Permissions
{
    public class RequireRole : PermissionAttribute
    {
        public ulong ID { get; private set; }

        public RequireRole(ulong ID) => this.ID = ID;
    }
}
