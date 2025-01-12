using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.StarGunSagittarius
{
    public class YouShouldBeAgile : ModPlayer
    {
        private bool hasGivenItemInThisWorld = false; // 默认没给过奖品
        private bool isTrackingEmpress = false;
        private int playerHitCount = 0;

        public override void PostUpdate()
        {
            // 如果物品已经发放，不再继续检测
            if (hasGivenItemInThisWorld)
            {
                return;
            }

            // 检查场上是否存在光之女皇
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.type == NPCID.HallowBoss)
                {
                    // 如果光之女皇存在且没有开始追踪，则开始追踪
                    if (!isTrackingEmpress)
                    {
                        isTrackingEmpress = true;
                        playerHitCount = 0; // 重置玩家受击次数
                    }
                    return; // 光之女皇存在时继续追踪
                }
            }
            
            if (!hasGivenItemInThisWorld)
            {
                if(NPC.downedEmpressOfLight) // 如果光之女皇死了
                {
                    // 如果光之女皇已经死亡且之前在追踪中
                    if (isTrackingEmpress)
                    {
                        // 根据玩家受击次数决定给予的物品
                        if (playerHitCount <= 4)
                        {
                            Player.QuickSpawnItem(null, ModContent.ItemType<StarGunSagittarius>(), 1);
                            Player.QuickSpawnItem(null, ModContent.ItemType<XiaoZhiTiaoSG>(), 1);
                            Main.NewText("完美的战斗，这是你的奖品！", 0, 255, 0);
                            hasGivenItemInThisWorld = true; // 设定该世界已经发放过奖励（仅此一次，给两件商品）
                        }
                        else
                        {
                            Player.QuickSpawnItem(null, ModContent.ItemType<XiaoZhiTiaoSG>(), 1);
                            Main.NewText("你似乎在敏捷程度上还需发力，如果你想获得这件奖品的话", 255, 0, 0);
                        }

                        
                        isTrackingEmpress = false;
                    }
                }
                
            }
                
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            // 如果正在追踪光之女皇战斗，并且是敌方弹幕造成的伤害
            if (isTrackingEmpress && proj.hostile)
            {
                playerHitCount++; // 增加玩家受击次数
            }
        }
    }
}
