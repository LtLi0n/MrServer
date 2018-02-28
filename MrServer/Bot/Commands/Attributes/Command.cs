using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Commands.Attributes
{
    public class Command : Attribute
    {
        public string Name;

        public Command(string cmd) => Name = cmd; 
    }
}
