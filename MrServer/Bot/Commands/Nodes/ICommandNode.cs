using MrServer.Bot.Client;
using MrServer.Bot.Commands.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MrServer.Bot.Commands.Nodes
{
    public class ICommandNode
    {
        public CommandEventArgs Context { get; set; }
    }
}
