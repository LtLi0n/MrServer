using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Commands.Attributes
{
    public class CommandNode : Attribute
    {
        public string Name { get; }

        public CommandNode(string name) => Name = name;
    }
}
