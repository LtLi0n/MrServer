using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Network.EventArgModels
{
    public class DiscordMessageReceivedEventArgs : EventArgs
    {
        public IMessage Message { get; }
        public IChannel Channel => Message.Channel;

        public DiscordMessageReceivedEventArgs(IMessage Message) => this.Message = Message;
    }
}
