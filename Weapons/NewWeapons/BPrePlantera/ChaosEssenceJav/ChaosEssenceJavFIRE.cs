using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ChaosEssenceJav
{
    public class ChaosEssenceJavFIRE : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public float Time
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override bool? CanDamage() => Time >= 12f; // 前 12 帧不造成伤害

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
        }

        public override void AI()
        {
            // 增加时间计数器
            Time++;

            // 弹幕保持直线运动并逐渐加速
            //Projectile.velocity *= 1.035f;


            {
                // ================= 飞行特效（重构版：收敛主核 + 辅助点缀） =================

                // ===== 基础方向 =====
                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 side = forward.RotatedBy(MathHelper.PiOver2);

                // ===== 时间参数（用于轻微动态秩序）=====
                float time = Main.GameUpdateCount * 0.18f;

                // ================= ① 主核心：Spark（更收敛、更集中）=================
                if (Main.rand.NextBool(1))
                {
                    // 让核心火花主要沿前进方向喷出，只保留较小偏角
                    Vector2 sparkVel =
                        forward.RotatedByRandom(MathHelper.ToRadians(10f)) *
                        Main.rand.NextFloat(7f, 11f);

                    Particle spark = new SparkParticle(
                        Projectile.Center + forward * 4f, // 稍微前移，核心感更强
                        sparkVel,
                        false,
                        20,
                        Main.rand.NextFloat(0.32f, 0.42f),
                        Color.Lerp(Color.DarkRed, Color.OrangeRed, Main.rand.NextFloat(0.35f, 0.75f))
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // ================= ② 次流层：AltSpark（贴体尾流）=================
                if (Main.rand.NextBool(2))
                {
                    AltSparkParticle altSpark = new AltSparkParticle(
                        Projectile.Center - forward * 2f,
                        -forward * Main.rand.NextFloat(0.05f, 0.15f),
                        false,
                        14,
                        0.28f,
                        Color.DarkRed * 0.25f
                    );
                    GeneralParticleHandler.SpawnParticle(altSpark);
                }

                // ================= ③ 辅助点缀：SquashDust（放弃双螺旋，改为“侧翼交替脉冲”）=================
                float wingSwing = (float)Math.Sin(time);
                Vector2 wingOffset = side * 10f * wingSwing + forward * Main.rand.NextFloat(-6f, 6f);

                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center + wingOffset,
                        ModContent.DustType<SquashDust>(),
                        (-forward * Main.rand.NextFloat(2.5f, 4.5f)) + side * Main.rand.NextFloat(-0.8f, 0.8f)
                    );
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(2.0f, 2.6f);
                    dust.color = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat());
                    dust.fadeIn = 2.2f;
                }

                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center - wingOffset,
                        ModContent.DustType<SquashDust>(),
                        (-forward * Main.rand.NextFloat(2.5f, 4.5f)) + side * Main.rand.NextFloat(-0.8f, 0.8f)
                    );
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(2.0f, 2.6f);
                    dust.color = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat());
                    dust.fadeIn = 2.2f;
                }

                // ================= ④ 扰动层：DiamondDust（环境噪声）=================
                if (Main.rand.NextBool(1))
                {
                    Vector2 randPos = Projectile.Center + Main.rand.NextVector2Circular(18f, 18f);

                    Dust dust = Dust.NewDustPerfect(
                        randPos,
                        ModContent.DustType<DiamondDust>(),
                        -forward * Main.rand.NextFloat(0.1f, 0.4f)
                    );
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.6f, 0.9f);
                    dust.color = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat());
                    dust.fadeIn = 1f;
                    dust.noLight = true;
                }

            }




            // 检测是否到达屏幕边缘并反弹
            Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            Vector2 screenPosition = Projectile.Center - Main.screenPosition;

            if (!screenRect.Contains(screenPosition.ToPoint()))
            {
                // 检测碰撞边缘并反弹，入射角等于出射角
                if (screenPosition.X <= 0 || screenPosition.X >= Main.screenWidth)
                    Projectile.velocity.X *= -1;
                if (screenPosition.Y <= 0 || screenPosition.Y >= Main.screenHeight)
                    Projectile.velocity.Y *= -1;
            }

        }

        public override void OnKill(int timeLeft)
        {
            // 消失时生成一圈粒子特效
            int particleCount = 10;
            float angleIncrement = MathHelper.TwoPi / particleCount;

            for (int i = 0; i < particleCount; i++)
            {
                Vector2 velocity = new Vector2(3f, 0f).RotatedBy(angleIncrement * i);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.CrimsonTorch, velocity, 0, Color.OrangeRed);
                dust.scale = 0.32f;
                dust.noGravity = true;
            }


            // 1️⃣ 血焰爆裂 Dust
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(10f, 10f);
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Lava,
                    velocity,
                    0,
                    Color.Lerp(Color.DarkRed, Color.Red, Main.rand.NextFloat(0.3f, 0.7f)),
                    Main.rand.NextFloat(0.2f, 0.8f)
                );
                dust.noGravity = Main.rand.NextBool(3) ? false : true;
            }

            // 2️⃣ 暗红火花射线
            for (int i = 0; i < 10; i++)
            {
                Particle spark = new SparkParticle(
                    Projectile.Center,
                    Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(10f, 16f),
                    false,
                    30,
                    Main.rand.NextFloat(0.18f, 0.5f),
                    Color.Lerp(Color.Red, Color.OrangeRed, Main.rand.NextFloat(0.2f, 0.5f))
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {   
            target.AddBuff(BuffID.OnFire3, 300); // 原版的狱炎效果
            target.AddBuff(BuffID.OnFire, 300); // 原版的着火效果
        }
    }
}