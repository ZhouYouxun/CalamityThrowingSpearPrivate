using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TLLGiveItem : GlobalNPC
    {
        private static bool hasGivenItem = false; // 用于确保物品只给一次

        public override void OnKill(NPC npc)
        {
            if (hasGivenItem) // 如果已经给予物品，则不再继续
                return;

            // 检测最后击杀该 NPC 的玩家
            int lastInteractionPlayerIndex = npc.lastInteraction;
            if (lastInteractionPlayerIndex >= 0 && lastInteractionPlayerIndex < Main.maxPlayers)
            {
                Player player = Main.player[lastInteractionPlayerIndex];

                // 检查玩家是否在海洋群系
                if (player.ZoneBeach)
                {
                    // 给予物品 XiaoZhiTiaoLL
                    player.QuickSpawnItem(player.GetSource_Misc("Reward"), ModContent.ItemType<XiaoZhiTiaoLL>());

                    // 设置开关，确保只触发一次
                    hasGivenItem = true;

                    // 可选：在聊天框提示玩家
                    //Main.NewText("You have received XiaoZhiTiaoLL!", 255, 255, 0);
                }
            }
        }
    }
}