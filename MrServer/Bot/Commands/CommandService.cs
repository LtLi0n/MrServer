using MrServer.Bot.Client;
using MrServerPackets.Discord.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MrServerPackets.Discord.Models.Messages;
using System.Linq;
using MrServer.Bot.Commands.Nodes;
using MrServer.Additionals.Tools;
using MrServer.Bot.Commands.Attributes;
using System.Reflection;
using MrServer.Bot.Commands.Attributes.Permissions;

namespace MrServer.Bot.Commands
{
    public class CommandService
    {
        private IEnumerable<MethodInfo> _commands { get; set; }

        public IEnumerable<DiscordCommand> commands;

        public DiscordClient Discord { get; private set; }

        private IEnumerable<ICommandNode> _nodes { get; set; }

        public CommandService(DiscordClient DiscordClient)
        {
            commands = Enumerable.Empty<DiscordCommand>();

            Discord = DiscordClient;

            Init().GetAwaiter().GetResult();
        }

        /*public async Task WaitUntilLoaded()
        {
            IEnumerable<ICommandNode> loadingNodes = new List<ICommandNode>(_nodes.Where(x => !x.Loaded));

            while(loadingNodes.Count() > 0)
            {
                loadingNodes = loadingNodes.Where(x => !x.Loaded);

                await Task.Delay(1);
            }
        }*/

        public CommandBuilder CreateCommand(string cmd) => new CommandBuilder(cmd, this);

        public DiscordCommand Command(string cmd) => commands.Where(x => x.CMD == cmd).First();

        //Try to find the command and execute it
        public Task ExecuteAsync(string cmd, string input, IMessage Message, bool IgnoreCase = true)
        {
            Tool.ForEach(commands, async (c) =>
            {
                if(IgnoreCase ? c.CMD.ToLower() == cmd.ToLower() : c.CMD == cmd)
                {
                    await c.RunAsync(new CommandEventArgs(
                        input: input,
                        parameters: c.Parameters,
                        message: Message,
                        cService: this,
                        network: Discord.network));
                }
            });

            return Task.CompletedTask;
        }

        public Task Init()
        {
            IEnumerable<MethodInfo> commandMethods = Enumerable.Empty<MethodInfo>();

            IEnumerable<Type> nodes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<CommandNode>() != null);

            foreach (Type node in nodes)
            {
                IEnumerable<MethodInfo> methods = node.GetMethods().Where(x => x.GetCustomAttribute<Command>() != null);

                Tool.ForEach(methods, async (m) =>
                {
                    Command command = m.GetCustomAttribute(typeof(Command)) as Command;

                    CommandBuilder cb = CreateCommand(command.Name);

                    //Params
                    {
                        ParameterInfo[] parameters = m.GetParameters();

                        bool remainderUsed = false;

                        List<string> args = new List<string>();

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            bool optional = false;

                            ParameterType type = ParameterType.Optional;

                            if (parameters[i].GetCustomAttribute(typeof(Remainder)) != null)
                            {
                                //Prevent debilizm
                                if (remainderUsed) throw new ArgumentException("Attribute \"Remainder\" can only be used once.");

                                remainderUsed = true;

                                type = ParameterType.Unparsed;
                            }

                            cb.AddParameter(parameters[i].Name, type);
                        }
                    }
                    //Attributes [Permissions]
                    {
                        IEnumerable<PermissionAttribute> attributes = m.GetCustomAttributes<PermissionAttribute>();

                        Tool.ForEach(attributes, x =>
                        {
                            cb.AddPermission(x);
                        });
                    }

                    await cb.OnCommand(m);
                });
            }

            return Task.CompletedTask;
        }
    }
}
