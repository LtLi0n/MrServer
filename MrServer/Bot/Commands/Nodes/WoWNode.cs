using MrServer.Additionals.Design;
using MrServer.Bot.Commands.Attributes;
using MrServer.Bot.Commands.Attributes.Permissions;
using MrServer.Network.WoW;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MrServer.Bot.Commands.Nodes
{
    [CommandNode("WoW")]
    [RequireOwner()] [Hidden]
    public class WoWNode : ICommandNode
    {
        [Command("Boss")]
        public async Task GetBoss(string ID)
        {
            string json = await WoWNetwork.DownloadBoss(int.Parse(ID));

            await Context.Channel.SendMessageAsync($"```json\n{JValue.Parse(json).ToString(Formatting.Indented)}```");
        }

        [Command("Auctions")]
        public async Task GetAuctions(string realm)
        {
            string auctions = await WoWNetwork.DownloadAuctions(realm);

            string json = JValue.Parse(auctions).ToString(Formatting.Indented);

            await Context.Channel.SendMessageAsync($"```json\n{json.Remove(1980, json.Length - 1980)}```");
        }
    }
}
