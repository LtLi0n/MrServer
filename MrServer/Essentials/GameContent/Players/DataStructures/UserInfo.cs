using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Essentials.GameContent.Players.DataStructures
{
    public class UserInfo
    {
        public ulong UserID { get; set; }
        public ulong ServerID { get; set; }
        public bool alertsOn = true;
        public bool additionalAlertsOn = true;
        public bool phoneMode = false;

        public bool oldProfileMode = false;

        public short AvatarID { get; set; }
    }
}
