using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Essentials.GameContent.Achievements
{
    public class Achievement
    {
        public string Name { get; set; }
        public string Description { get; set; }

        private int? ID_Child { get; set; }
        public Achievement Child(AchievementManager AchievementManager) => ID_Child.HasValue ? AchievementManager.Achievements[ID_Child.Value] : null;

        public AchievementReward AchievementReward { get; set; }

        public Achievement(string Name, string Description, AchievementReward AchievementReward = null, int? ChildID = null)
        {
            this.Name = Name;
            this.Description = Description;
            this.AchievementReward = AchievementReward;
            ID_Child = ChildID;
        }
    }
}
