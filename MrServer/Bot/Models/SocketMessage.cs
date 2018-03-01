using MrServer.Network;
using MrServerPackets.Discord.Interfaces;
using MrServerPackets.Discord.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Models
{
    public class SocketMessage : Message
    {
        public new SocketChannel Channel { get; set; }

        public SocketMessage(NetworkHandler network, SocketChannel channel, Message message)
        {
            Channel = channel;
            base.Content = message.Content;
            base.CreatedAt = message.CreatedAt;
            base.Embed = message.Embed;
            base.ID = message.ID;
        }
    }
}
