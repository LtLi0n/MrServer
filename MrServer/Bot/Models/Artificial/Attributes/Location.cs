using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Models.Artificial
{
    public class Location : InteractiveMessageAttribute
    {
        private string _hierarchy;

        public Location(string hierarchy) => _hierarchy = hierarchy;

        public override string ToString() => _hierarchy;
    }
}
