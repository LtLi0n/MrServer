using MrServer.Bot.Commands.Permissions;
using MrServerPackets.Discord.Models.Guilds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using MrServer.Bot.Commands.Nodes;
using MrServer.Bot.Commands.Attributes.Permissions;

namespace MrServer.Bot.Commands
{
    public class DiscordCommand : CommandBase
    {
        public new string CMD => base.CMD;
        public bool IsHidden => Hidden;
        public new string Description => base.Description;

        public IEnumerable<string> Aliases => _aliases;
        public IEnumerable<CommandParameter> Parameters => _parameters;

        ///<summary> Checks if permission supplied can be converted back to derived type. </summary>
        private bool ToPermission<T>(PermissionAttribute o, out T result) where T : PermissionAttribute
        {
            bool success = false;

            if (typeof(T).IsAssignableFrom(o.GetType()))
            {
                result = (T)o;
                success = true;
            }
            else result = null;

            return success;
        }

        public async Task RunAsync(CommandEventArgs e)
        {
            if (e.Message.Channel.ID != 409677778405818368 && e.Message.Channel.ID != 417690504512274452)
            {
                await e.Channel.SendMessageAsync("Commands don't work in unaccepted channels.");
                return;
            }

            bool pass = true;

            List<PermissionAttribute> notMet = new List<PermissionAttribute>();

            for(int i = 0; i < _permissions.Length; i++)
            {
                if (ToPermission<RequireRole>(_permissions[i], out RequireRole permRole))
                {
                    if (((GuildMessage)e.Message).Author.RoleIDs.Count(x => x == permRole.ID) != 1) notMet.Add(permRole);
                }
                else if (ToPermission<RequireOwner>(_permissions[i], out RequireOwner permOwner))
                {
                    if (!((((GuildMessage)e.Message).Author).ID == permOwner.ID)) notMet.Add(permOwner);
                }
            }

            if (notMet.Count == 0)
            {
                dynamic node = Activator.CreateInstance(_methodInfo.ReflectedType);

                node.Context = e;

                _methodInfo.Invoke(node, e.args);
            }
            else
            {
                string toReturn = "You don't meet the required permissions:";

                foreach(PermissionAttribute perm in notMet)
                {
                    toReturn += $"\n{perm.GetType().Name}\n";
                }

                await e.Channel.SendMessageAsync(toReturn);
            }
        }

        public DiscordCommand(CommandBuilder cb) : base(cb) { }

        private void SetParameters(CommandParameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++) parameters[i].ID = i;

            _parameters = parameters;
        }
    }
}
