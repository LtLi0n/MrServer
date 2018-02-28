using MrServer.Bot.Commands;
using MrServer.Network;
using MrServerPackets;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Models.Guilds;
using MrServerPackets.Discord.Models.Messages;
using MrServerPackets.Discord.Packets;
using MrServerPackets.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Models
{
    public class Channel : IChannel
    {
        private CommandEventArgs _e;

        public Channel(CommandEventArgs e, IChannel copyFrom = null)
        {
            _e = e;

            if (copyFrom != null)
            {
                ID = copyFrom.ID;
                Name = copyFrom.Name;
            }
        }

        public async Task SendMessageAsync(string content, Embed embed = null)
        {
            PacketWriter pw = new PacketWriter(Header.Discord);
            pw.WriteHeader(DiscordPacketTypeHeader.SendMessage_GuildChannel);

            GuildMessage guildMessage = _e.Message as GuildMessage;

            pw.WriteJSON(new GuildMessagePacket { Content = content, Channel = guildMessage.Channel, Guild = guildMessage.Guild, Embed = embed });

            await _e.network.Send<DataTCP>(pw.GetBytes(), _e.network.DiscordTCP);
        }
    }
}
