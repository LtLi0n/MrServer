using MrServerPackets.Discord.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Models
{
    public class SocketUser : User
    {
        public SocketUser(User user)
        {
            base.AvatarID = user.AvatarID;
            base.Discriminator = user.Discriminator;
            base.ID = user.ID;
            base.IsBot = user.IsBot;
            base.Username = user.Username;
        }
    }
}
