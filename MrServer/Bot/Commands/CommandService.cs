using MrServer.Bot.Client;
using MrServerPackets.Discord.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Linq;
using MrServer.Bot.Commands.Nodes;
using MrServer.Additionals.Tools;
using MrServer.Bot.Commands.Attributes;
using System.Reflection;
using MrServer.Bot.Commands.Attributes.Permissions;
using MrServer.Bot.Models;

namespace MrServer.Bot.Commands
{
    public class CommandService
    {
        private IEnumerable<MethodInfo> _methods { get; set; }

        public IEnumerable<DiscordCommand> commands;

        public DiscordClient Discord { get; private set; }

        private IEnumerable<ICommandNode> _nodes { get; set; }

        public CommandService(DiscordClient DiscordClient)
        {
            commands = Enumerable.Empty<DiscordCommand>();

            Discord = DiscordClient;

            Init().GetAwaiter().GetResult();
        }

        public CommandBuilder CreateCommand(string cmd) => new CommandBuilder(cmd, this);

        public DiscordCommand Command(string cmd) => commands.Where(x => x.CMD == cmd).First();

        //Try to find the command and execute it
        public Task<bool> ExecuteAsync(string cmd, string input, SocketUserMessage Message, bool IgnoreCase = true, bool internally = false)
        {
            bool success = false;

            Tool.ForEach(commands, async (c) =>
            {
                if(IgnoreCase ? c.CMD.ToLower() == cmd.ToLower() : c.CMD == cmd)
                {
                    try
                    {
                        await c.RunAsync(new CommandEventArgs(
                            input: input,
                            parameters: c.Parameters,
                            message: Message,
                            cService: this,
                            network: Discord.network), internally);

                        success = true;
                        return;
                    }
                    catch(Exception e) { await Message.Channel.SendMessageAsync(e.Message); return; }

                }
            });

            return Task.FromResult(success);
        }

        ///<summary>Using reflection grabs every command node and loads all commands into the server.</summary>
        public Task Init()
        {
            IEnumerable<Type> nodes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<CommandNode>() != null);

            foreach (Type node in nodes)
            {
                IEnumerable<MethodInfo> methods = node.GetMethods().Where(x => x.GetCustomAttribute<Command>() != null);

                IEnumerable<PermissionAttribute> nodePermissions = node.GetCustomAttributes<PermissionAttribute>();

                Tool.ForEach(methods, (m) =>
                {
                    Command command = m.GetCustomAttribute<Command>();

                    CommandBuilder cb = CreateCommand(command.Name);

                    //Parameters
                    {
                        ParameterInfo[] parameters = m.GetParameters();

                        bool remainderUsed = false;

                        List<string> args = new List<string>();

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            bool optional = false;

                            ParameterType type = parameters[i].HasDefaultValue ? ParameterType.Optional : ParameterType.Required;

                            if (parameters[i].GetCustomAttribute(typeof(Remainder)) != null)
                            {
                                //Prevent debilizm
                                if (remainderUsed) throw new ArgumentException("Attribute \"Remainder\" can only be used once.");

                                remainderUsed = true;

                                type = type == ParameterType.Optional ? ParameterType.UnparsedOptional : ParameterType.UnparsedRequired;
                            }

                            cb.AddParameter(parameters[i].Name, type);
                        }
                    }
                    //Permissions
                    {
                        IEnumerable<PermissionAttribute> attributes = m.GetCustomAttributes<PermissionAttribute>();

                        Tool.ForEach(attributes, x => cb.AddPermission(x));
                        Tool.ForEach(nodePermissions, p => cb.AddPermission(p));
                    }

                    commands = commands.Append(cb.OnCommand(m).Build());
                });
            }

            return Task.CompletedTask;
        }
    }
}
