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
using MrServerPackets.Discord.Entities;
using System.Drawing;

namespace MrServer.Bot.Commands.Nodes
{
    [CommandNode("System")]
    //[RequireRole(417325871838265356)]
    public class SystemNode : ICommandNode
    {
        [Command("test")]
        [Hidden]
        public async Task Test(string text1, string text2) => await Context.Channel.SendMessageAsync(text1 + text2);

        [Command("Echo")]
        public async Task Echo([Remainder]string text) => await Context.Channel.SendMessageAsync(text);

        [Command("Ping")]
        public async Task Ping() => await Context.Channel.SendMessageAsync("Pong!\nOk, have a cat picture too https://d1wn0q81ehzw6k.cloudfront.net/additional/thul/media/0eaa14d11e8930f5?w=400&h=400");

        [Command("Help")]
        public async Task Help()
        {
            IEnumerable<DiscordCommand> viewable = Context.Discord.cService.commands.Where(cc => cc.Permissions.Select(p => p.GetType() == typeof(Hidden)).Count() == 1);

            EmbedBuilder eb = new EmbedBuilder();

            eb.Description = $"Hey {Context.Message.Author.Mention}, here's a list of current commands:";

            string toField = $"";

            string nodeName = string.Empty;
            string nodeEmoji = string.Empty;

            Tool.ForEach(viewable, (c) => 
            {
                if (nodeName != c.Node)
                {
                    if (eb.Fields.Count > 0) eb.Fields[eb.Fields.Count - 1].Value = toField;

                    nodeName = c.Node;
                    toField = "\u200b";

                    if (c.Node == "Osu") nodeEmoji = "<:STD:415867031087480833>";
                    else if (c.Node == "System") nodeEmoji = "💡";

                    eb.AddField(x =>
                    {
                        x.Name = $"{nodeName} {nodeEmoji}";
                        x.IsInline = true;
                    });
                }


                toField += $"\t•**`{c.CMD}`**\n";
            });

            if(eb.Fields.Count > 0) eb.Fields[eb.Fields.Count - 1].Value = toField;

            eb.Color = Color.FromArgb(28, 164, 185);

            await Context.Channel.SendMessageAsync(string.Empty, eb.Build());
        }
    }
}
