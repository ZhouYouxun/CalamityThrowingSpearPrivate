using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Sagittarius
{
    public class YouShouldBeAgile : GlobalNPC
    {
        private bool isTrackingEmpress = false; // 是否正在追踪光之女皇
        private int playerHitCount = 0; // 玩家受击次数
        private static bool hasGivenRewardInThisWorld = false; // 确保整个世界只发放一次奖励

        public override bool InstancePerEntity => true; // 每个 NPC 独立的 GlobalNPC 实例

        public override void AI(NPC npc)
        {
            // 检查是否是光之女皇
            if (npc.type == NPCID.HallowBoss)
            {
                // 如果教徒未被击败，且世界奖励未发放，开始追踪
                if (!NPC.downedAncientCultist && !hasGivenRewardInThisWorld)
                {
                    if (!isTrackingEmpress)
                    {
                        isTrackingEmpress = true; // 开始追踪
                        playerHitCount = 0; // 重置玩家受击次数
                    }
                }
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile proj, ref NPC.HitModifiers modifiers)
        {
            // 如果正在追踪光之女皇战斗，并且是敌方弹幕造成的伤害
            if (npc.type == NPCID.HallowBoss && isTrackingEmpress && proj.hostile)
            {
                playerHitCount++; // 增加玩家受击次数
            }
        }

        public override void OnKill(NPC npc)
        {
            // 如果不是光之女皇，直接返回
            if (npc.type != NPCID.HallowBoss) return;

            // 如果教徒未被击败，且世界奖励未发放
            if (!NPC.downedAncientCultist && !hasGivenRewardInThisWorld)
            {
                hasGivenRewardInThisWorld = true; // 标记奖励已发放

                Player player = Main.player[npc.target]; // 获取目标玩家

                // 根据玩家受击次数决定给予的物品
                if (playerHitCount <= 4)
                {
                    player.QuickSpawnItem(npc.GetSource_Loot(), ModContent.ItemType<Sagittarius>(), 1);
                    player.QuickSpawnItem(npc.GetSource_Loot(), ModContent.ItemType<XiaoZhiTiaoSG>(), 1);
                    //Main.NewText("我去，纽币！", 0, 255, 0); // 成功
                }
                else
                {
                    player.QuickSpawnItem(npc.GetSource_Loot(), ModContent.ItemType<XiaoZhiTiaoSG>(), 1);
                    //Main.NewText("回去重练", 255, 0, 0); // 失败
                }

                isTrackingEmpress = false; // 重置追踪状态
            }
        }
    }
}

