using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Bot.Commands
{
    public enum ParameterType
    {
        /// <summary> Catches a single required parameter. </summary>
        Required,
        /// <summary> Catches a single optional parameter. </summary>
        Optional,
        /// <summary> Catches an unparsed required parameter. </summary>
        UnparsedRequired,
        /// <summary> Catches all remaining text as a single optional parameter. </summary>
        UnparsedOptional
    }

    public class CommandParameter
    {
        public string Name { get; }
        public int ID { get; set; }
        public ParameterType Type { get; }

        public CommandParameter(string name, ParameterType type)
        {
            Name = name;
            Type = type;
        }
    }
}
