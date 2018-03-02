using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Additionals.Design
{
    public partial class DesignFunctions
    {
        public static class Time
        {
            public static string GetTime()
            {
                string minutes = DateTime.Now.Minute.ToString();

                if (minutes.Length == 1) minutes = "0" + minutes;

                return $"{DateTime.Now.Hour}:{minutes}";
            }

            

        public static class Text
        {

        }
    }
}
