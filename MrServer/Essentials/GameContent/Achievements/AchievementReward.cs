using MrServer.Essentials.GameContent.Players;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrServer.Essentials.GameContent.Achievements
{
    public class AchievementReward
    {
        private int? TitleID { get; set; }
        private int[] ItemIDs { get; set; }
        private long? Gold { get; set; }
        private long? XP { get; set; }

        public void GrantReward(Player player)
        {
            if (TitleID.HasValue) //player.Character.Titles.Add(ITitle.TitleCollection[TitleID.Value]);
            if (ItemIDs != null)
            {
                for (int i = 0; i < ItemIDs.Length; i++)
                {
                    //player.Character.Inventory.AddItem(ItemIDs[i]);
                }
            }
            if (Gold.HasValue) player.Character.Gold += Gold.Value;
            if (XP.HasValue) player.Character.TOTAL_XP += XP.Value;
        }

        public AchievementReward(int? TitleID = null, long? Gold = null, long? XP = null, int[] ItemIDs = null)
        {
            if (TitleID.HasValue) this.TitleID = TitleID;
            if (Gold.HasValue) this.Gold = Gold;
            if (XP.HasValue) this.XP = XP;
            if (ItemIDs != null) this.ItemIDs = ItemIDs;
        }
    }
}
