using MrServer.Bot.Commands.Attributes;
using MrServer.Bot.Commands.Attributes.Permissions;
using MrServer.Bot.Commands.Permissions;
using MrServer.SQL.Osu;
using MrServerPackets.Discord.Models.Guilds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Commands.Nodes
{
    [CommandNode("Owner")]
    [Hidden] [RequireOwner]
    class OwnerNode : ICommandNode
    {
        private OsuSQL OsuDB => Program.Entry.DataBases.OsuDB;

        [Command("SQL")]
        public async Task SQL([Remainder]string command)
        {
            await OsuDB.ExecuteAsync(command);
        }

        [Command("SayDelete")]
        public async Task SayDelete([Remainder]string text)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(text);
        }
    }
}
