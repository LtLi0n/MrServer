using MrServer.Bot.Commands.Attributes;
using MrServer.Bot.Commands.Attributes.Permissions;
using MrServer.Bot.Commands.Permissions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Commands
{
    public class CommandBase
    {
        protected string[] _aliases;
        protected CommandParameter[] _parameters;
        protected PermissionAttribute[] _permissions;

        protected MethodInfo _methodInfo;

        protected bool Hidden;
        protected string CMD;
        protected string Description;

        protected ulong? RequireID;

        public CommandBase(string cmd)
        {
            CMD = cmd;
            Hidden = false;
            _aliases = new string[0];
            _parameters = new CommandParameter[0];
        }

        public CommandBase(CommandBuilder cb)
        {
            _parameters = cb.Parameters.ToArray();
            _permissions = cb.Permissions.ToArray();

            _aliases = cb._aliases;
            _methodInfo = cb._methodInfo;
            Hidden = cb.Hidden;
            CMD = cb.CMD;
            Description = cb.Description;
        }
    }
}
