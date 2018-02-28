using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Commands.Attributes.Permissions
{
    public class RequireOwner : PermissionAttribute
    {
        public ulong ID => Program.Entry.ME;
    }
}
