using MrServer.Bot.Commands.Attributes;
using MrServer.Bot.Commands.Attributes.Permissions;
using MrServer.Bot.Commands.Permissions;
using MrServer.Bot.Models.Artificial.Models;
using MrServer.SQL.Management;
using MrServer.SQL.Osu;
using MrServerPackets.Discord.Models.Guilds;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Commands.Nodes
{
    [CommandNode("Owner")]
    [Hidden] [RequireOwner]
    class OwnerNode : ICommandNode
    {
        private OsuSQL OsuDB => Program.Entry.DataBases.OsuDB;
        private CustomGuildSQL GuildDB => Program.Entry.DataBases.GuildDB;

        [Command("SQL")]
        public async Task SQL(string db, [Remainder]string command)
        {
            string dbLower = db.ToLower();

            if (dbLower == "osu") await OsuDB.ExecuteAsync(command);
            else if (dbLower == "guilds") await GuildDB.ExecuteAsync(command);
        }

        [Command("SayDelete")]
        public async Task SayDelete([Remainder]string text)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(text);
        }

        [Command("GuildSettings")]
        public async Task CreateInteractiveMessage() =>
            await Context.Discord.guildManagementHandler.AddUser(Context.Message, new Models.Artificial.InteractiveUser(Context.Channel.ID, Context.Message.Author.ID), Context.Discord);
    }
}
