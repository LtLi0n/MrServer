using MrServer.Bot.Commands.Permissions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using MrServer.Bot.Commands.Attributes.Permissions;

namespace MrServer.Bot.Commands
{
    public class CommandBuilder : CommandBase
    {
        ///<summary>Temp flexible collection for parameters.</summary>
        public List<CommandParameter> Parameters;
        ///<summary>Temp flexible collection for permissions.</summary>
        public List<PermissionAttribute> Permissions;

        private CommandService _cService;

        public CommandBuilder(string cmd, CommandService cService) : base(cmd)
        {
            _cService = cService;

            Parameters = new List<CommandParameter>();
            Permissions = new List<PermissionAttribute>();
            CMD = cmd;
        }

        public CommandBuilder Hide()
        {
            Hidden = true;
            return this;
        }

        public CommandBuilder AddPermission(PermissionAttribute permission)
        {
            Permissions.Add(permission);
            return this;
        }

        public CommandBuilder AddParameter(CommandParameter parameter)
        {
            Parameters.Add(parameter);
            return this;
        }

        public CommandBuilder AddParameter(string parameter, ParameterType type)
        {
            Parameters.Add(new CommandParameter(parameter, type));
            return this;
        }

        public async Task OnCommand(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
            await Build();
        }

        private async Task Build() => await Task.Run(() => _cService.commands = _cService.commands.Append(new DiscordCommand(this)));
    }
}
