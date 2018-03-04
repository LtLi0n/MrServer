using MrServer.Bot.Client;
using MrServer.Bot.Models;
using MrServer.Network;
using MrServerPackets;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Models.Guilds;
using MrServerPackets.Discord.Packets;
using MrServerPackets.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Commands
{
    public class CommandEventArgs
    {
        public string[] args;

        public SocketUserMessage Message { get; }
        public SocketChannel Channel => Message.Channel;

        private CommandService cService { get; set; }
        public DiscordClient Discord => cService.Discord;

        public CommandEventArgs(
            string input,
            IEnumerable<CommandParameter> parameters,
            SocketUserMessage message,
            CommandService cService,
            NetworkHandler network)
        {
            Message = message;
            this.cService = cService;

            List<string> argsTemp = new List<string>(input.Split(' '));
            List<string> correctArgs = new List<string>();

            bool optionalUsed = false;

            int argIndex = 0;

            CommandParameter[] paramArr = new List<CommandParameter>(parameters).ToArray();

            for (int i = 0; i < paramArr.Length; i++)
            {
                if (argIndex + 1 > argsTemp.Count && paramArr[i].Type != ParameterType.Optional)
                {
                    string expectedParameters = "";

                    foreach (CommandParameter expectedParam in paramArr)
                    {
                        expectedParameters += $"{expectedParam.Name} - {Enum.GetName(typeof(ParameterType), expectedParam.Type)}\n";
                    }

                    throw new Exception($"Not enough parameters specified.\n\nExpected:\n{expectedParameters}");
                }
                    

                if (paramArr[i].Type == ParameterType.Optional)
                {
                    if(argsTemp.Count > argIndex)
                    {
                        if (!string.IsNullOrEmpty(argsTemp[argIndex]))
                        {
                            correctArgs.Add(argsTemp[argIndex]);
                            argIndex++;
                            optionalUsed = true;
                        }
                    }
                }
                else
                {
                    if (optionalUsed) throw new Exception("More optional parameters can only exist by following the first one.");
                    else
                    {
                        if (paramArr[i].Type == ParameterType.Unparsed)
                        {
                            string mergedArgs = "";

                            for (int z = argIndex; z < argsTemp.Count; z++) mergedArgs += ' ' + argsTemp[z];

                            mergedArgs = mergedArgs.Remove(0, 1);

                            correctArgs.Add(mergedArgs);

                            break;
                        }
                        else if (paramArr[i].Type == ParameterType.Required)
                        {
                            correctArgs.Add(argsTemp[argIndex]);
                            argIndex++;
                        }
                    }
                }
            }

            for (; correctArgs.Count < paramArr.Length;) correctArgs.Add(null); // fill with null values because reflection is stupid enough that optional parameters still NEED to be null

            args = correctArgs.ToArray();
        }
    }
}
