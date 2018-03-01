using MrServer.Bot.Commands;
using MrServer.Network;
using MrServerPackets;
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
        public SocketGuild Guild { get; }

        public SocketGuildChannel(SocketGuild guild, SocketChannel channel) : base(channel)
        {
            Guild = guild;

            base.ID = channel.ID;
            base.Name = channel.Name;
        }

        public override async Task SendMessageAsync(string content, Embed embed = null)
        {
            PacketWriter pw = new PacketWriter(Header.Discord);
            pw.WriteHeader(DiscordPacketTypeHeader.SendMessage_GuildChannel);

            var packet = new GuildMessagePacket { Content = content, Channel = new GuildChannel(this, Guild), Embed = embed };

            pw.WriteJSON(packet);

            await _network.Send<DataTCP>(pw.GetBytes(), _network.DiscordTCP);
        }
    }
}
