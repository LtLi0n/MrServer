using MrServer.Bot.Commands;
using MrServer.Network;
using MrServerPackets;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Models.Guilds;
using MrServerPackets.Discord.Packets;
using MrServerPackets.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Models
{
    public class SocketChannel : Channel
    {
        protected NetworkHandler _network;

        public SocketChannel(NetworkHandler network, Channel channel = null)
        {
            _network = network;
            Extract(channel);
        }
        protected SocketChannel(SocketChannel channel)
        {
            _network = channel._network;
            Extract(channel);
        }

        private void Extract(Channel channel)
        {
            base.ID = channel.ID;
            base.Name = channel.Name;
        }

        public virtual Task SendMessageAsync(string content, Embed embed = null) => Task.CompletedTask;
    }
}
