using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.NPCs.DevourerofGods;
using Terraria.ModLoader.IO;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
{
    public class YourRevelation : GlobalNPC
    {
        private static bool hasGivenXiaoZhiTiaoRE = false; // 静态开关，限制全世界只触发一次

        public override bool InstancePerEntity => true; // 确保每个 NPC 实例独立处理

        // 在 NPC 死亡时触发
        public override void OnKill(NPC npc)
        {
            // 检查是否为 DevourerofGodsHead，并且奖励未被给予过
            if (npc.type == ModContent.NPCType<DevourerofGodsHead>() && !hasGivenXiaoZhiTiaoRE)
            {
                hasGivenXiaoZhiTiaoRE = true; // 开启开关，防止重复发放

                // 获取当前玩家对象
                Player player = Main.LocalPlayer;

                // 给予物品 XiaoZhiTiaoRE
                player.QuickSpawnItem(null, ModContent.ItemType<XiaoZhiTiaoRE>(), 1);

                // 在左下角显示提示信息
                Main.NewText("只有掌握机械的奥秘，才能领悟生命的真谛", Color.LightGreen);
            }
        }


    }
}