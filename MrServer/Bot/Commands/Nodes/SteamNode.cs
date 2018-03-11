using MrServer.Additionals.Storing;
using MrServer.Bot.Commands.Attributes;
using MrServer.Bot.Commands.Attributes.Permissions;
using MrServer.Network;
using MrServerPackets.Discord.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Commands.Nodes
{
    [CommandNode("Steam")]
    [RequireOwner] [Hidden]
    public class SteamNode : ICommandNode
    {
        [Command("SteamGame")]
        public async Task GetGame(string app)
        {
            string json = await NetworkHandler.DownloadJSON($"http://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/?key={Keys.API_KEYS["Steam"]}&appid={app}&", 1);

            EmbedBuilder eb = new EmbedBuilder();

            eb.AddField(x => { x.Name = "test"; x.Value = "test"; });

            await Context.Channel.SendMessageAsync($"http://cdn.akamai.steamstatic.com/steam/apps/{app}/movie480.webm");
        }
    }
}
