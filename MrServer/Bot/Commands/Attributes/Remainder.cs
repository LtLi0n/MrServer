using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class Remainder : Attribute
    {
    }
}
