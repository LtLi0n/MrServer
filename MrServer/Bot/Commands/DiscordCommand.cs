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
using MrServer.Bot.Commands.Attributes;
using MrServer.Bot.Models;
using MrServerPackets.Discord.Models;

namespace MrServer.Bot.Commands
{
    public class DiscordCommand : CommandBase
    {
        public new string CMD => base.CMD;
        public bool IsHidden => Hidden;
        public new string Description => base.Description;

        public IEnumerable<string> Aliases => _aliases;
        public IEnumerable<CommandParameter> Parameters => _parameters;
        public IEnumerable<PermissionAttribute> Permissions => _permissions;

        public string Node => ((CommandNode)_methodInfo.DeclaringType.GetCustomAttributes(typeof(CommandNode), false)[0]).Name;

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

        public async Task RunAsync(CommandEventArgs e, bool internally = false)
        {
            if (e.Channel.ID != 409677778405818368 && e.Channel.ID != 417690504512274452 && e.Channel.ID != 420253228689653781)
            {
                if(!internally) await e.Channel.SendMessageAsync("Commands don't work in unaccepted channels.");
                return;
            }

            bool pass = true;

            List<(PermissionAttribute, string)> notMet = new List<(PermissionAttribute, string)>();

            for(int i = 0; i < _permissions.Length; i++)
            {
                if (ToPermission<RequireRole>(_permissions[i], out RequireRole permRole))
                {
                    if (((SocketGuildUser)e.Message.Author).RoleIDs.Count(x => x == permRole.ID) != 1)
                    {
                        notMet.Add((permRole, $"You don't have role id: `{permRole.ID}`."));
                    }
                }
                else if (ToPermission<RequireOwner>(_permissions[i], out RequireOwner permOwner))
                {
                    if (e.Message.Author.ID != permOwner.ID)
                    {
                        notMet.Add((permOwner, "What do we have here :eyes:"));
                    }
                }
                else if (ToPermission<RequireGuildPermission>(_permissions[i], out RequireGuildPermission permGuild))
                {
                    if (!(((SocketGuildUser)e.Message.Author).Permissions.HasFlag(permGuild.GuildPermission)))
                    {
                        GuildPermission[] guildPerms = Enum.GetValues(typeof(GuildPermission)).Cast<GuildPermission>().Where(x => permGuild.GuildPermission.HasFlag(x)).ToArray();

                        string notifyRequiredPerms = "";

                        for (int gp = 0; gp < guildPerms.Length; gp++) notifyRequiredPerms += Enum.GetName(typeof(GuildPermission), guildPerms[gp]) + " ";

                        notifyRequiredPerms = notifyRequiredPerms.Remove(notifyRequiredPerms.Length - 1, 1);

                        notMet.Add((permGuild, notifyRequiredPerms));
                    }
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
                string toReturn = "__**You don't meet the required permissions:**__";

                foreach((PermissionAttribute, string) perm in notMet)
                {
                    toReturn += $"\n• `{perm.Item1.GetType().Name}` - {perm.Item2}\n";
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
