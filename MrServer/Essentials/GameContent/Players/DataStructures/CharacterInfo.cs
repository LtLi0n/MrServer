using MrServer.Essentials.GameContent.Players.DataStructures.Statistics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Essentials.GameContent.Players.DataStructures
{
    public class CharacterInfo
    {
        private enum XpTo { XpCap, Level, LevelXp }

        [JsonIgnore] public Player Player { get; set; }
        //STATS
        public DateTime NameChangedAt { get; set; }
        public string Name { get; set; }
        public int HP { set; get; }
        public long Gold { set; get; }

        [JsonIgnore] public int PlayerTopID { get; set; }
        public long TOTAL_XP { get; set; }
        public long XP => Get_X_FromXp(XpTo.LevelXp);
        [JsonIgnore] public long XP_CAP => Get_X_FromXp(XpTo.XpCap);
        [JsonIgnore] public short Level => (short)Get_X_FromXp(XpTo.Level);
        [JsonIgnore] public int MAX_HP => Level * 500;

        public Task AddXp(long Xp)
        {
            short oldLevel = Level;
            TOTAL_XP += Xp;

            //User has leveled up
            if (oldLevel < Level) HP = MAX_HP;

            return Task.CompletedTask;
        }

        public CharacterStatistics Stats { get; set; }

        private long Get_X_FromXp(XpTo x)
        {
            long tempXp = TOTAL_XP;
            long tempLevel = 1;
            long tempXpCap = 500;

            while (tempXp >= tempXpCap && tempLevel < Player.LevelCap)
            {
                tempLevel++;
                tempXp -= tempXpCap;

                /*
                double multiplier = tempLevel / 1.3;
                /if (tempLevel < 20) tempXpCap += tempLevel * 250;

                /if (tempLevel < 20) tempXpCap += tempLevel * (200 + (tempLevel * 10));
                /else tempXpCap += (int)Math.Round((tempLevel * tempLevel) * multiplier);

                tempXpCap += (tempLevel * 200) + (tempLevel * (tempLevel * (1 + (int)Math.Sqrt(tempLevel)) * 8));
                */

                tempXpCap += (long)(
                        ((tempLevel * 500) + (tempLevel * (tempLevel * (1 + (int)Math.Sqrt(tempLevel)) * 8))) *
                        (1 + ((tempLevel * tempLevel) / 1500.0)));
            }

            switch (x)
            {
                case XpTo.XpCap: return tempXpCap;
                case XpTo.Level: return tempLevel;
                default: return tempXp;
            }
        }

        [JsonConstructor]
        public CharacterInfo(Player player)
        {
            Player = player;

            TOTAL_XP = 0;
            HP = MAX_HP;

            Stats = new CharacterStatistics();
        }
    }
}
