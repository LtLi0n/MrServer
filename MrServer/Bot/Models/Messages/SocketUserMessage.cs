using MrServer.Network;
using MrServerPackets.Discord.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Models
{
    public class SocketUserMessage : SocketMessage
    {
        public SocketUser Author { get; set; }

        public SocketUserMessage(NetworkHandler network, SocketChannel channel, Message message, SocketUser author) : base(network, channel, message)
        {
            Author = author;
        }
    }
}
