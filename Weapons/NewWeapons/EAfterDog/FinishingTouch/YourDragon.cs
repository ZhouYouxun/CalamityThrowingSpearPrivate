using CalamityMod.NPCs.Bumblebirb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class YourDragon : GlobalNPC
    {
        private static bool hasGivenXiaoZhiTiaoFT = false; // 静态开关，限制当前世界内只触发一次

        public override bool InstancePerEntity => true; // 每个 NPC 实例独立处理

        // 检测 NPC 死亡逻辑
        public override void OnKill(NPC npc)
        {
            // 检查是否为 Bumblefuck【Dragonfolly】 且奖励未被触发过
            if (npc.type == ModContent.NPCType<Dragonfolly>() && !hasGivenXiaoZhiTiaoFT)
            {
                hasGivenXiaoZhiTiaoFT = true; // 标记物品已触发

                // 获取当前玩家对象
                Player player = Main.LocalPlayer;

                // 给予物品 XiaoZhiTiaoFT
                player.QuickSpawnItem(null, ModContent.ItemType<XiaoZhiTiaoFT>(), 1);

                // 显示提示文本
                Main.NewText("巨龙之火在不久的将来会重新燃起", Color.OrangeRed);
            }
        }

     
    }
}