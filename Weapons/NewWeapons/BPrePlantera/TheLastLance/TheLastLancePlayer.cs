using CalamityMod.Buffs.StatDebuffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.GameInput;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TheLastLancePlayer : ModPlayer
    {
        private float defenseBonus = 0f;
        private bool hasRevived = false; // 标记是否已经复活过
        private int reviveCooldown = 0; // 用于复活冷却的计时器

        public override void ResetEffects()
        {
            defenseBonus = 0f; // 重置防御加成
            if (reviveCooldown > 0)
            {
                reviveCooldown--; // 每帧减少冷却计时器
                if (reviveCooldown <= 0)
                {
                    hasRevived = false; // 冷却结束后重置复活标志
                }
            }
        }

        public override void PostUpdateEquips()
        {
            // 如果玩家手上拿着 TheLastLance，则防御力增加 5%
            if (Player.HeldItem.type == ModContent.ItemType<TheLastLance>())
            {
                defenseBonus += Player.statDefense * 0.05f; // 提升防御力 5%

                // 检查是否存在 TheLastLanceDASH 弹幕，并且玩家的水平速度不为 0
                bool hasLanceDashProjectile = Main.projectile.Any(proj => proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<TheLastLanceDASH>());
                if (hasLanceDashProjectile && Player.velocity.X != 0)
                {
                    defenseBonus += Player.statDefense * 0.20f; // 额外提升防御力 20%
                }
            }

            Player.statDefense += (int)defenseBonus; // 将防御加成应用到玩家的防御力上
        }

        // 当玩家受到致命伤害时调用
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            // 如果玩家还未复活过，并且手持 TheLastLance 武器，阻止死亡并触发复活效果
            if (!hasRevived && Player.HeldItem.type == ModContent.ItemType<TheLastLance>())
            {
                hasRevived = true; // 标记为已复活

                if (DownedBossSystem.downedLeviathan) // 如果击败了利维坦
                {
                    reviveCooldown = 60 * 60; // 那么冷却时间缩短至60秒（60*60帧）
                }
                else
                {
                    reviveCooldown = 80 * 60; // 冷却时间设置为80秒（80*60帧）
                }

                // 计算 TheLastLancePBuff 持续时间，以恢复到最大生命值的 25%
                int maxLife = Player.statLifeMax2;
                int lifeToRestore = maxLife / 4;
                int buffDuration = lifeToRestore; // 每帧恢复 1 点生命值，因此持续时间等于恢复的生命值数量

                // 阻止玩家死亡并恢复生命值
                Player.statLife = 1; // 设置玩家生命值为 1，防止立即死亡
                Player.AddBuff(ModContent.BuffType<TheLastLancePBuff>(), buffDuration); // 添加 TheLastLancePBuff

                // 在玩家手上生成一个 TheLastLanceDASH 弹幕，并将超级冲刺开关打开
                int projIndex = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<TheLastLanceDASH>(), 0, 0, Player.whoAmI);
                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                {
                    Projectile proj = Main.projectile[projIndex];
                    if (proj.ModProjectile is TheLastLanceDASH dashProj)
                    {
                        dashProj.SetSuperDash(); // 调用方法设置超级冲刺
                    }
                }


                return false; // 阻止玩家死亡
            }
            return true; // 允许玩家死亡
        }

        // 当玩家被 NPC 攻击时调用
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            // 如果玩家手上拿着 TheLastLance，则对敌人施加长达 10 秒的三种状态效果
            if (Player.HeldItem.type == ModContent.ItemType<TheLastLance>())
            {
                npc.AddBuff(ModContent.BuffType<GlacialState>(), 600); // 冰河时代，来自 CalamityMod
                npc.AddBuff(BuffID.Frostburn, 600); // 原版的霜火效果
                npc.AddBuff(BuffID.Chilled, 600); // 原版的寒冷效果
            }
        }

        // 当玩家被投射物击中时调用
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            // 如果玩家手上拿着 TheLastLance，则对敌人施加长达 10 秒的三种状态效果
            if (Player.HeldItem.type == ModContent.ItemType<TheLastLance>())
            {
                NPC target = Main.npc[proj.owner];
                target.AddBuff(ModContent.BuffType<GlacialState>(), 600); // 冰河时代，来自 CalamityMod
                target.AddBuff(BuffID.Frostburn, 600); // 原版的霜火效果
                target.AddBuff(BuffID.Chilled, 600); // 原版的寒冷效果
            }
        }
        bool hasLanceDashProjectile = false;

        // 禁用交互，如果在冲刺期间的话
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // 检查是否存在 TheLastLanceDASH 弹幕
            bool hasLanceDashProjectile = Main.projectile.Any(proj => proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<TheLastLanceDASH>());

            if (hasLanceDashProjectile)
            {
                // 禁用鼠标左右键交互
                Player.controlUseItem = false; // 禁用物品使用（左键）
                Player.controlUseTile = false; // 禁用环境交互（右键）
            }
            // 这个效果不需要手动还原，因为他每帧都会重置

            base.ProcessTriggers(triggersSet); // 保留其他输入处理
        }

    }
}
