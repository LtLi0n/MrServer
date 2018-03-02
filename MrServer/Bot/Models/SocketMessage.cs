using MrServer.Bot.Client;
using MrServer.Network;
using MrServerPackets;
using MrServerPackets.Discord.Interfaces;
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
    public class SocketMessage : Message
    {
        public new SocketChannel Channel { get; set; }

        protected NetworkHandler _network;

        public SocketMessage(NetworkHandler network, SocketChannel channel, Message message)
        {
            Channel = channel;
            base.Content = message.Content;
            base.CreatedAt = message.CreatedAt;
            base.Embed = message.Embed;
            base.ID = message.ID;

            _network = network;
        }

        public Message CommunicationMessage => new Message()
        {
            Channel = base.Channel,
            Content = base.Content,
            CreatedAt = base.CreatedAt,
            Embed = base.Embed,
            ID = base.ID
        };

        public MessagePacket ToMessagePacket(string action) => new MessagePacket(CommunicationMessage, action);

        public GuildMessagePacket ToGuildMessagePacket(string action) => Channel as SocketGuildChannel != null ?
            new GuildMessagePacket(
                channel: new GuildChannel(((Channel as SocketGuildChannel).Guild.CommunicationGuild), Channel.CommunicationChannel),
                msg: ToMessagePacket(action)) :
            throw new InvalidCastException("SocketChannel is not of type SocketGuildChannel");
    }
}
