using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics
{
    public class YouShouldBeFast : ModPlayer
    {
        private bool hasGivenItemInThisWorld = false; // 默认是没有给过
        private bool isCountingTime = false;
        private int startTime;

        private int startFrame;

        public override void PostUpdate()
        {
            // 检查场上是否存在猪龙鱼公爵
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.type == NPCID.DukeFishron)
                {
                    if (!isCountingTime)
                    {
                        // 如果发现猪龙鱼公爵出现，记录开始帧数
                        startFrame = (int)Main.GameUpdateCount;
                        isCountingTime = true;
                    }
                    return; // 如果有猪龙鱼公爵，继续等待其死亡
                }
            }

            if(!hasGivenItemInThisWorld) // 如果这个世界还没有给过
            {
                if (!(NPC.downedAncientCultist)) // 如果教徒还存活
                {
                    if(NPC.downedFishron) //但是朱砂已经死了
                    {
                        // 如果正在计时且猪龙鱼公爵已消失，停止计时
                        if (isCountingTime)
                        {
                            int endFrame = (int)Main.GameUpdateCount; // 记录结束帧数
                            int elapsedFrames = endFrame - startFrame; // 计算总帧数
                            int elapsedTime = elapsedFrames / 60; // 将帧数转换为秒

                            // 广播击杀所用时间
                            Main.NewText($"你击败猪龙鱼公爵的时间是 {elapsedTime} 秒！", 255, 255, 0);

                            // 根据时间决定给予的物品
                            if (elapsedTime < 120) // 小于120秒
                            {
                                Player.QuickSpawnItem(null, ModContent.ItemType<TidalMechanics>(), 1);
                                Player.QuickSpawnItem(null, ModContent.ItemType<XiaoZhiTiaoTM>(), 1);
                                Main.NewText("你战胜了海洋，这是你的奖品！", 0, 255, 0);
                                hasGivenItemInThisWorld = true; // 只有满足条件（成功了）就会给两件奖品，并且这个世界不会再给第2次
                            }
                            else // 大于等于120秒
                            {
                                Player.QuickSpawnItem(null, ModContent.ItemType<XiaoZhiTiaoTM>(), 1);
                                Main.NewText("速度还有待提升，如果想获得奖励的话，就看看给你的这个东西吧！", 255, 0, 0);
                            }

                            isCountingTime = false;
                        }
                    }
                    
                }                  
            }
        }
    }
}
