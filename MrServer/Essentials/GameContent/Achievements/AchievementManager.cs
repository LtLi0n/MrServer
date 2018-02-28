using MrServer.Essentials.GameContent.Handling;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Essentials.GameContent.Achievements
{
    public class AchievementManager
    {
        public Achievement[] Achievements { get; set; }

        private GameHandler GameHandler { get; set; }

        public AchievementManager(GameHandler GameHandler)
        {
            Achievement WorkAchievement1_ID_0 = new Achievement("Mining Rookie", "Mine for atleast 100 hours in total.");
            Achievement WorkAchievement2_ID_1 = new Achievement("Advanced Miner", "Mine for atleast 250 hours in total.", ChildID: 0);
            Achievement WorkAchievement3_ID_2 = new Achievement("Experienced Miner", "Mine for atleast 500 hours in total.", ChildID: 1);
            Achievement WorkAchievement4_ID_3 = new Achievement("Mining God", "Mine for atleast 1000 hours in total.", ChildID: 2);

            Achievements = new Achievement[]
            {
                WorkAchievement1_ID_0,
                WorkAchievement2_ID_1,
                WorkAchievement3_ID_2,
                WorkAchievement4_ID_3
            };

            this.GameHandler = GameHandler;
        }
    }
}
