using MrServer.Additionals.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MrServer.Bot.Commands.Permissions;
using MrServerPackets.Discord.Models.Guilds;
using MrServer.Bot.Commands.Attributes;
using MrServer.Bot.Commands.Attributes.Permissions;

namespace MrServer.Bot.Commands.Nodes
{
    [CommandNode("System")]
    public class SystemNode : ICommandNode
    {
        [Command("Echo")]
        public async Task Echo([Remainder]string text) => await Context.Channel.SendMessageAsync(text);

        [Command("Ping")]
        [RequireOwner]
        public async Task Ping() => await Context.Channel.SendMessageAsync("Pong!\nOk, have a cat picture too https://d1wn0q81ehzw6k.cloudfront.net/additional/thul/media/0eaa14d11e8930f5?w=400&h=400");

        [Command("Help")]
        public async Task Help()
        {
            IEnumerable<DiscordCommand> viewable = Context.Discord.cService.commands.Where(cc => !cc.IsHidden);

            string toReturn = $"Hey {Context.Message.Author.Mention}, here's a list of current commands:```swift\n";

            Tool.ForEach(viewable, (c) => { toReturn += $"\n• {c.CMD}"; });

            await Context.Channel.SendMessageAsync($"{toReturn}```");
        }
    }
}
