using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    public class SurfeiterDrumGlobalNpc : GlobalNPC
    {
        public override bool InstancePerEntity => true; // 每个 NPC 独立实例化

        // 添加 drumForm 字段，默认值为 -1 表示未被影响
        public int drumForm = -1;

        public override void ResetEffects(NPC npc)
        {
            // 每帧重置 drumForm，防止效果持续存在
            drumForm = -1;
        }
        //1.笞:Flogging-敌怪获得1.02倍的易伤
        //2.杖:Beating-敌怪的接触伤害减少30%
        //3.徒:Imprisoning-敌怪的移动速度减少50%
        //4.流:Banishing-敌怪防御降低40
        //5.死:Executing-2秒的倒计时结束后造成5000点伤害

        private int executionTimer = 120; // 计时器，每 2 秒触发一次伤害

        public override void AI(NPC npc)
        {
            // 检测 Buff 是否存在
            if (npc.lifeMax <= 50000 && npc.HasBuff(ModContent.BuffType<SurfeiterDrumEDebuff>()))
            {
                switch (drumForm)
                {
                    case 0: // 笞 (Flogging) - 敌怪获得1.02倍易伤
                        npc.defense = (int)Math.Floor(npc.defense * 0.98);
                        break;
                    case 1: // 杖 (Beating) - 敌怪接触伤害减少30%
                        npc.damage = (int)(npc.damage * 0.7f);
                        break;
                    case 2: // 徒 (Imprisoning) - 敌怪移动速度减少50%
                        npc.velocity *= 0.5f;
                        break;
                    case 3: // 流 (Banishing) - 敌怪防御降低40
                        npc.defense -= 40;
                        if (npc.defense < 0)
                            npc.defense = 0;
                        break;
                    case 4: // 死 (Executing) - 每 2 秒造成一次 5000 点伤害
                        executionTimer--;

                        if (executionTimer <= 0)
                        {
                            executionTimer = 120; // 重置计时器
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = 5000,
                                Knockback = 0f,
                                HitDirection = 0
                            });

                            // 在敌人头顶弹出黑色的 "5000" 战斗文本
                            CombatText.NewText(npc.Hitbox, Color.Black, "5000", true, false);
                        }
                        break;
                }
            }
            else if (npc.lifeMax > 50000 && npc.HasBuff(ModContent.BuffType<SurfeiterDrumEDebuff>()))
            {
                switch (drumForm)
                {
                    case 0: // 笞 (Flogging) - 敌怪获得1.02倍易伤
                        npc.defense = (int)Math.Floor(npc.defense * 0.98);
                        break;
                    case 1: // 杖 (Beating) - 敌怪接触伤害减少30%
                        npc.damage = (int)(npc.damage * 0.7f);
                        break;
                    case 2: // 徒 (Imprisoning) - 敌怪移动速度减少50%
                        npc.velocity *= 0.9f;
                        break;
                    case 3: // 流 (Banishing) - 敌怪防御降低40
                        npc.defense -= 40;
                        if (npc.defense < 0)
                            npc.defense = 0;
                        break;
                    case 4: // 死 (Executing) - 每 2 秒造成一次 5000 点伤害
                        executionTimer--;

                        if (executionTimer <= 0)
                        {
                            executionTimer = 120; // 重置计时器
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = 300,
                                Knockback = 0f,
                                HitDirection = 0
                            });

                            // 在敌人头顶弹出黑色的 "5000" 战斗文本
                            CombatText.NewText(npc.Hitbox, Color.Black, "200", true, false);
                        }
                        break;
                }
            }
        }


    }
}