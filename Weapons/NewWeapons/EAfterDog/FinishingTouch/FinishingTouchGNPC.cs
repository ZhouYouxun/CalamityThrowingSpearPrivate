using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Yharon;
using CalamityMod.NPCs.DevourerofGods;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.DLOAS; // 引用 CalamityMod 的 NPCs 命名空间

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class FinishingTouchGNPC : GlobalNPC
    {
        private static bool eventTriggered = false; // 确保事件在世界中只触发一次
        private bool yharonTriggered = false; // 确保对单个 Yharon 只触发一次

        // 确保每个 NPC 实例有独立的 GlobalNPC 实例
        public override bool InstancePerEntity => true;
        private bool hasGivenXiaoZhiTiaoFT = false; // 用于限制 XiaoZhiTiaoFT 仅触发一次

        public override void AI(NPC npc)
        {
            // 获取 Yharon 的类型 ID
            int yharonType = ModContent.NPCType<Yharon>(); // 使用 CalamityMod 中 Yharon 的类获取类型 ID
            // 检查 NPC 是否是 Yharon 并且符合条件
            if (!eventTriggered && npc.type == yharonType && npc.life <= npc.lifeMax * 0.55f && !yharonTriggered)
            {
                yharonTriggered = true; // 标记当前 Yharon 已触发事件
                eventTriggered = true; // 标记整个世界已经触发该事件

                // 获取玩家对象
                Player player = Main.player[npc.target];

                // 设置弹幕的伤害值
                int projectileDamage = Main.zenithWorld ? 100000 : 300;

                // 在玩家头顶 100 个方块的位置生成弹幕
                Vector2 spawnPosition = player.Center - new Vector2(0, 100 * 16); // 100 个方块高
                Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPosition, new Vector2(0, 40), ModContent.ProjectileType<FinishingTouchEPROJ>(), projectileDamage, 0, player.whoAmI);

                // 添加生成物品的逻辑
                if (!hasGivenXiaoZhiTiaoFT)
                {
                    hasGivenXiaoZhiTiaoFT = true; // 标记物品已生成

                    // 直接将 XiaoZhiTiaoFT 添加到玩家背包
                    player.QuickSpawnItem(npc.GetSource_FromAI(), ModContent.ItemType<XiaoZhiTiaoFT>(), 1);

                    // 在左下角显示提示信息
                    Main.NewText("收下他！", Color.LightGreen);
                }
            }
        }


        public override void OnKill(NPC npc)
        {
            // 检查是否是 DevourerofGodsHead NPC 并且是 Boss
            if (npc.type == ModContent.NPCType<DevourerofGodsHead>())
            {
                // 显示固定文本
                Vector2 textPosition = Main.LocalPlayer.Center - new Vector2(0, Main.LocalPlayer.height / 2 + 20f); // 玩家头顶位置
                CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, "打神吞，特别厉害！", false, false);
            }
        }




    }
}
