using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics
{
    public class YouShouldBeFast : GlobalNPC
    {
        private bool isCountingTime = false; // 是否正在计时
        private int startFrame; // 记录开始帧数
        private static bool hasGivenRewardInThisWorld = false; // 确保整个世界只发放一次奖励

        public override bool InstancePerEntity => true; // 每个 NPC 独立的 GlobalNPC 实例

        public override void AI(NPC npc)
        {
            // 检查是否是猪龙鱼公爵
            if (npc.type == NPCID.DukeFishron)
            {
                // 如果教徒未被击败，且猪龙鱼公爵已被生成，开始计时
                if (!NPC.downedAncientCultist && !NPC.downedFishron)
                {
                    if (!isCountingTime)
                    {
                        startFrame = (int)Main.GameUpdateCount; // 开始计时
                        isCountingTime = true;
                    }
                }
            }
        }

        public override void OnKill(NPC npc)
        {
            // 如果不是猪龙鱼公爵，直接返回
            if (npc.type != NPCID.DukeFishron) return;

            // 确保教徒未被击败，并且在当前世界奖励未发放
            if (!NPC.downedAncientCultist && !hasGivenRewardInThisWorld)
            {
                hasGivenRewardInThisWorld = true; // 标记奖励已发放

                // 计算击杀时间
                int endFrame = (int)Main.GameUpdateCount;
                int elapsedFrames = endFrame - startFrame;
                int elapsedTime = elapsedFrames / 60; // 转换为秒

                Player player = Main.player[npc.target]; // 获取目标玩家

                // 根据击杀时间发放奖励并输出信息
                if (elapsedTime < 120)
                {
                    player.QuickSpawnItem(npc.GetSource_Loot(), ModContent.ItemType<TidalMechanics>(), 1);
                    player.QuickSpawnItem(npc.GetSource_Loot(), ModContent.ItemType<XiaoZhiTiaoTM>(), 1);
                    Main.NewText(Language.GetTextValue("Mods.CalamityThrowingSpear.TheSpecialText.TidalMechanicsFast"), 0, 255, 0); // 奖励文字
                }
                else
                {
                    player.QuickSpawnItem(npc.GetSource_Loot(), ModContent.ItemType<XiaoZhiTiaoTM>(), 1);
                    Main.NewText(Language.GetTextValue("Mods.CalamityThrowingSpear.TheSpecialText.TidalMechanicsSlow"), 255, 0, 0); // 鼓励文字
                }

                isCountingTime = false; // 重置计时状态
            }
        }
    }
}
