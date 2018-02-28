using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Alias : Attribute
    {
        public string[] aliases;

        public Alias(params string[] aliases) => this.aliases = aliases;
    }
}
