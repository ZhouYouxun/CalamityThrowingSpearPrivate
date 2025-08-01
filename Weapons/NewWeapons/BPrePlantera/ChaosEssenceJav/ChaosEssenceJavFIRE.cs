using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;

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
                // 绘制轨迹粒子特效
                for (int i = 0; i < 1; i++)
                {
                    float t = i / 3f; // ✅ 强制浮点
                    Vector2 spawnPosition = Vector2.Lerp(Projectile.oldPosition, Projectile.position, t);
                    Dust dust = Dust.NewDustPerfect(spawnPosition, DustID.CrimsonTorch);
                    dust.color = Color.OrangeRed;
                    dust.scale = 1.5f;
                    dust.fadeIn = 1f;
                    dust.noGravity = true;
                }

                // 1️⃣ 快速暗红 SparkParticle 拖尾
                if (Main.rand.NextBool(1))
                {
                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(6f, 12f),
                        false,
                        20,
                        Main.rand.NextFloat(0.3f, 0.4f),
                        Color.Lerp(Color.DarkRed, Color.OrangeRed, Main.rand.NextFloat(0.3f, 0.7f))
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 2️⃣ 竹笋尾流
                if (Main.rand.NextBool(2))
                {
                    AltSparkParticle altSpark = new AltSparkParticle(
                        Projectile.Center,
                        -Projectile.velocity * 0.05f,
                        false,
                        14,
                        0.3f,
                        Color.DarkRed * 0.25f
                    );
                    GeneralParticleHandler.SpawnParticle(altSpark);
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