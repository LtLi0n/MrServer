using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Additionals.Tools
{
    public static class Tool
    {
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration) action(item);
        }
    }
}
