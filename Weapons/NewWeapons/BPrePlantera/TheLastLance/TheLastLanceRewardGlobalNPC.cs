using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance;
using System.Collections.Generic;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TheLastLanceRewardGlobalNPC : GlobalNPC
    {
        private static bool hasAwardedTheLastLance = false; // 确保每个世界只奖励一次

        public override void OnKill(NPC npc)
        {
            // 检查是否是 626 号小动物（海马：NPCID.Seahorse）
            if (npc.type == NPCID.Seahorse && npc.lastInteraction != -1)
            {
                Player player = Main.player[npc.lastInteraction];

                // 确保最后的攻击是由弹幕造成的，并且是 383 号弹幕（Anchor）
                if (player != null && player.active && player.HeldItem.shoot == 383 && !hasAwardedTheLastLance)
                {
                    // 给玩家 TheLastLance
                    player.QuickSpawnItem(player.GetSource_Misc("Reward"), ModContent.ItemType<TheLastLance>());
                    hasAwardedTheLastLance = true; // 确保只奖励一次
                }
            }
        }
    }
}
