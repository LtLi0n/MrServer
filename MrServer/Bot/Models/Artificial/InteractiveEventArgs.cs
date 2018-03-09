using MrServer.Bot.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Models.Artificial
{
    public class InteractiveEventArgs : EventArgs
    {
        public SocketUserMessage Message { get; }
        public SocketChannel Channel => Message.Channel;
        public DiscordClient Discord { get; }
        public InteractiveUser User { get; }

        public string Displayer;

        public bool IsSimulated { get; set; }

        public InteractiveEventArgs(SocketUserMessage msg, DiscordClient discord, InteractiveUser user)
        {
            Message = msg;
            Discord = discord;

            User = user;

            Displayer = string.Empty;

            IsSimulated = false;
        }
    }
}
