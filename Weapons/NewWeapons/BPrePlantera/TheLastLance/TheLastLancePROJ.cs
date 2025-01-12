using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Buffs.StatDebuffs;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TheLastLancePROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";

        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/TheLastLance/TheLastLancePROJ";


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 在弹幕路径上生成双螺旋特效，使用气泡类粒子（Gore bubble）
            float offset = (float)Math.Sin(Projectile.localAI[0] * 0.1f) * 1.5f; // 双螺旋的偏移量
            Vector2 bubblePos1 = Projectile.Center + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * offset;
            Vector2 bubblePos2 = Projectile.Center + Projectile.velocity.RotatedBy(-MathHelper.PiOver2) * offset;
            Gore bubble1 = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), bubblePos1, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
            Gore bubble2 = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), bubblePos2, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
            bubble1.timeLeft = 8 + Main.rand.Next(6);
            bubble2.timeLeft = 8 + Main.rand.Next(6);
            bubble1.scale = Main.rand.NextFloat(0.6f, 1f);
            bubble2.scale = Main.rand.NextFloat(0.6f, 1f);
            bubble1.type = Main.rand.NextBool(3) ? 412 : 411;
            bubble2.type = Main.rand.NextBool(3) ? 412 : 411;

            Projectile.localAI[0]++; // 更新粒子动画
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 根据敌人当前血量设置冻结状态的持续时间
            int freezeDuration = target.life < (target.lifeMax / 2) ? 600 : 300;

            // 对敌人施加冻结状态
            target.AddBuff(ModContent.BuffType<GlacialState>(), freezeDuration); // 冰河时代
            target.AddBuff(BuffID.Frostburn, freezeDuration); // 原版的霜火效果
            target.AddBuff(BuffID.Chilled, freezeDuration); // 原版的寒冷效果

            // 检查敌人是否同时拥有三种冻结状态
            if (target.HasBuff(ModContent.BuffType<GlacialState>()) && target.HasBuff(BuffID.Frostburn) && target.HasBuff(BuffID.Chilled))
            {
                Projectile.damage = (int)(Projectile.damage * 1.75f); // 造成1.75倍伤害
            }

            // 检查玩家是否处于海洋群系，以便造成额外的两倍伤害
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.ZoneBeach)
            {
                Projectile.damage = (int)(Projectile.damage * 2.0f); // 造成两倍伤害
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 播放原版音效 Item69
            SoundEngine.PlaySound(SoundID.Item69, Projectile.position);

            // 生成 3 发随机角度的 TheLastLanceWater 弹幕（正后方）
            for (int i = 0; i < 3; i++)
            {
                // 在 -45 度到 45 度之间随机生成一个角度
                float randomAngle = Main.rand.NextFloat(-45f, 45f);
                Vector2 baseDirection = -Projectile.velocity.SafeNormalize(Vector2.Zero); // 以弹幕的反方向为基准
                Vector2 rotatedDirection = baseDirection.RotatedBy(MathHelper.ToRadians(randomAngle)); // 在基础方向上旋转角度
                Vector2 spawnVelocity = rotatedDirection * 12f; // 固定的初始速度，可以根据需要调整

                // 创建 TheLastLanceWater 弹幕
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    spawnVelocity,
                    ModContent.ProjectileType<TheLastLanceWater>(),
                    (int)(Projectile.damage * 1.0f), // 1.0倍倍率的伤害
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
            // 现有的屏幕震动效果
            float shakePower = 0.35f; // 震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 根据与玩家距离进行衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            // 在弹幕死亡时，向反方向发射大量蓝色Dust特效
            for (int i = 0; i < 20; i++)
            {
                Vector2 dustVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * -1.2f; // 随机角度旋转并反方向发射Dust
                Dust blueDust = Dust.NewDustPerfect(Projectile.position, DustID.BlueCrystalShard, dustVelocity, 0, Color.Blue, 1.5f);
                blueDust.noGravity = true; // 使Dust不受重力影响
            }


            {
                // 生成螺旋式水门粒子特效
                int spiralParticles = 160; // 螺旋粒子的总数
                float spiralRadius = 0f; // 初始半径
                float radiusIncrement = 2.5f; // 螺旋半径的增量
                float initialAngle = Main.rand.NextFloat(0, MathHelper.TwoPi); // 随机生成初始角度
                float angleIncrement = MathHelper.TwoPi / spiralParticles; // 每个粒子之间的角度增量

                // 原始螺旋粒子特效
                for (int i = 0; i < spiralParticles; i++)
                {
                    float angle = initialAngle + i * angleIncrement; // 计算当前粒子的角度，基于随机初始角度偏移
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * spiralRadius; // 计算偏移量，形成圆形轨迹
                    Vector2 position = Projectile.Center + offset; // 计算粒子生成的位置

                    // 创建水门特效粒子
                    Dust spiralDust = Dust.NewDustPerfect(position, DustID.Water, Vector2.Zero, 0, Color.Cyan, 1.2f);
                    spiralDust.noGravity = true; // 使粒子不受重力影响
                    spiralRadius += radiusIncrement; // 增加半径，形成逐渐扩散的螺旋
                }

                // 反方向的螺旋粒子特效
                spiralRadius = 0f; // 重置半径
                for (int i = 0; i < spiralParticles; i++)
                {
                    float angle = initialAngle + i * angleIncrement + MathHelper.Pi; // 计算当前粒子的角度，在原始角度基础上偏移180度（反方向）
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * spiralRadius; // 计算偏移量，形成圆形轨迹
                    Vector2 position = Projectile.Center + offset; // 计算粒子生成的位置

                    // 创建反方向的水门特效粒子
                    Dust spiralDust = Dust.NewDustPerfect(position, DustID.Water, Vector2.Zero, 0, Color.Cyan, 1.2f);
                    spiralDust.noGravity = true; // 使粒子不受重力影响
                    spiralRadius += radiusIncrement; // 增加半径，形成逐渐扩散的螺旋
                }
            }


        }



    }
}