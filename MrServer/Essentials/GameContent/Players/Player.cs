using System;
using System.Collections.Generic;
using System.Text;
using MrServer.Essentials.GameContent.Players.DataStructures;

namespace MrServer.Essentials.GameContent.Players
{
    public class Player
    {
        public const int LevelCap = 100;
        public const int MaxQuestsPerDay = 7;

        public CharacterInfo Character { get; set; }
        public UserInfo User { get; set; }

        public Player()
        {
            User = new UserInfo();
            Character = new CharacterInfo(this);
            Character.Player = this;
        }
    }
}
