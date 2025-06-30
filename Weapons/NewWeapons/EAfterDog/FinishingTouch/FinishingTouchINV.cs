using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class FinishingTouchINV : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 50;
            Projectile.extraUpdates = 5;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 1;
            Projectile.alpha = 255; // 完全透明
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);

            // === 无序部分：Dust 火焰瀑布喷射，占总量 50% ===
            int dustCount = 25; // 每帧
            for (int i = 0; i < dustCount; i++)
            {
                float spread = MathHelper.ToRadians(90f); // ±45°
                float angle = Main.rand.NextFloat(-spread, spread);
                Vector2 velocity = forward.RotatedBy(angle) * Main.rand.NextFloat(8f, 22f);
                int type = Main.rand.NextFloat() < 0.7f ? DustID.OrangeTorch : DustID.FlameBurst;
                Dust d = Dust.NewDustDirect(
                    Projectile.Center,
                    0, 0,
                    type,
                    velocity.X,
                    velocity.Y,
                    0,
                    Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat()),
                    Main.rand.NextFloat(1.5f, 2.8f)
                );
                d.noGravity = true;
            }

            // === 有序部分：方向冲击波 PulseWave ===
            if (Main.GameUpdateCount % 5 == 0)
            {
                int pulseLayers = 4;
                for (int i = 0; i < pulseLayers; i++)
                {
                    Particle pulse = new DirectionalPulseRing(
                        Projectile.Center + forward * (20f + i * 10f),
                        forward * (3f + i * 1f),
                        Color.Lerp(Color.OrangeRed, Color.Yellow, i / (float)pulseLayers),
                        new Vector2(1f, 2.5f + i * 0.4f),
                        Projectile.rotation - MathHelper.PiOver4,
                        0.2f + i * 0.05f,
                        0.03f,
                        20
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }

            // === 有序部分：竹笋状拖尾 AltSparkParticle ===
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                AltSparkParticle spark = new AltSparkParticle(
                    Projectile.Center + offset,
                    Projectile.velocity * 0.05f,
                    false,
                    12,
                    1.5f,
                    Color.OrangeRed * 0.2f
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }




        private bool hasHitOnce = false; // 用于追踪是否已命中一次
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasHitOnce)
            {
                hasHitOnce = true;
                Projectile.friendly = false; // 命中后立即关闭友方伤害属性
            }
        }






        public override void OnKill(int timeLeft)
        {
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);

            // === 无序部分：Dust 极限喷射 ===
            int dustCount = 300; // 大爆发
            float spread = MathHelper.ToRadians(150f);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(-spread, spread);
                Vector2 velocity = forward.RotatedBy(angle) * Main.rand.NextFloat(15f, 50f);
                int type = Main.rand.NextFloat() < 0.7f ? DustID.OrangeTorch : DustID.FlameBurst;
                Dust d = Dust.NewDustDirect(
                    Projectile.Center,
                    0, 0,
                    type,
                    velocity.X,
                    velocity.Y,
                    0,
                    Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat()),
                    Main.rand.NextFloat(1.8f, 3.5f)
                );
                d.noGravity = true;
            }

            // === 有序部分：火焰冲击波 ===
            int pulseLayers = 8;
            for (int i = 0; i < pulseLayers; i++)
            {
                Particle pulse = new DirectionalPulseRing(
                    Projectile.Center + forward * (30f + i * 12f),
                    forward * (6f + i * 2f),
                    Color.Lerp(Color.OrangeRed, Color.Yellow, i / (float)pulseLayers),
                    new Vector2(1.2f, 3f + i * 0.5f),
                    Projectile.rotation - MathHelper.PiOver4,
                    0.25f + i * 0.06f,
                    0.02f,
                    35
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // === 有序部分：橙色 SparkParticle 扇形喷射 ===
            int sparkCount = 120;
            float sparkSpread = MathHelper.ToRadians(90f);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.Lerp(-sparkSpread / 2, sparkSpread / 2, i / (float)sparkCount);
                Vector2 dir = forward.RotatedBy(angle);
                Color color = Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat());

                Particle spark = new SparkParticle(
                    Projectile.Center,
                    dir * Main.rand.NextFloat(15f, 40f),
                    false,
                    60,
                    Main.rand.NextFloat(1.2f, 2.2f),
                    color
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }









    }
}