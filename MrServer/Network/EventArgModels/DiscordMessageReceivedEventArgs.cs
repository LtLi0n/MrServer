using MrServer.Bot.Commands;
using MrServer.Bot.Models;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Models.Guilds;
using MrServerPackets.Headers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Network.EventArgModels
{
    public class DiscordMessageReceivedEventArgs : EventArgs
    {
        public SocketUserMessage Message { get; }
        public SocketChannel Channel;

        public DiscordMessageReceivedEventArgs(UserMessage msg, NetworkHandler network)
        {
            if (typeof(GuildChannel).IsAssignableFrom(msg.Channel.GetType()))
            {
                SocketGuild guild = new SocketGuild(network, ((GuildChannel)msg.Channel).Guild);

                SocketGuildChannel channel = new SocketGuildChannel(guild, new SocketChannel(network, msg.Channel));

                Message = new SocketUserMessage(network, new SocketGuildChannel(guild, channel), (Message)msg, new SocketGuildUser(msg.Author as GuildUser));
            }
        }
    }
}
