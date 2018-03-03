using MrServer.Additionals.Tools;
using MrServer.Bot.Client;
using MrServer.Bot.Commands;
using MrServer.Network;
using MrServerPackets;
using MrServerPackets.Discord.Entities;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Packets;
using MrServerPackets.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Models
{
    public class SocketGuildChannel : SocketChannel
    {
        private DiscordClient _discord => Program.Entry.DiscordClient;

        public SocketGuild Guild { get; }

        public SocketGuildChannel(SocketGuild guild, SocketChannel channel) : base(channel)
        {
            Guild = guild;

            base.ID = channel.ID;
            base.Name = channel.Name;
        }

        public override async Task<SocketUserMessage> SendMessageAsync(string content, Embed embed = null, bool attachID = false)
        {
            PacketWriter pw = new PacketWriter(Header.Discord);
            pw.WriteHeader(DiscordHeader.SendMessage);

            //Attach 
            if (attachID)
            {
                usedIDs.RemoveAll(x => (DateTime.Now - x.Item2).Minutes > 5);

                string randID = Tool.RandomString();

                //Well if this actually happens, I will invest all my money into lottery [1 in 1,3 * 10 ^ 36 chance]
                //Fun fact: that's like gettings tails 100 times in a row!
                while (usedIDs.Exists(x => x.Item1 == randID)) randID = Tool.RandomString();

                if (embed == null) embed = new EmbedBuilder() { Footer = new EmbedFooterBuilder() { Text = randID } }.Build();
                else
                {
                    if (embed.Footer.HasValue) embed.Footer = new EmbedFooter { Text = randID, IconUrl = embed.Footer.Value.Text };
                    else embed.Footer = new EmbedFooter() { Text = randID };
                }

                usedIDs.Add((randID, DateTime.Now));
            }

            MessagePacket packet = new GuildMessagePacket(
                new GuildChannel(new Guild() { ID = Guild.ID }, this),
                new MessagePacket(new Message()
                {
                    Content = content,
                    Embed = embed
                }));

            string json = JsonConvert.SerializeObject(packet, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

            pw.WriteJSON(packet, true);

            await _network.Send<DataTCP>(pw.GetBytes(), _network.DiscordTCP);

            return await _discord.GetBotMessageAsync(this, content, embed);
        }

        public GuildChannel CommunicationGuild => new GuildChannel(Guild.CommunicationGuild, base.CommunicationChannel);
    }
}
