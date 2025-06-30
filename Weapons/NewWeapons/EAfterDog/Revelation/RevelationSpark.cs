using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
{
    public class RevelationSpark : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public float Time
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public const int DustType = 226;
        public const float MaxHomingDistance = 1200f;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            //Projectile.arrow = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // 确保速度固定
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 22f;

            if (!Main.dedServ && Time > 5f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 spawnPosition = Vector2.Lerp(Projectile.oldPosition, Projectile.position, i / 3f);
                    Dust dust = Dust.NewDustPerfect(spawnPosition, DustType);
                    dust.color = Main.hslToRgb((Main.rand.NextFloat(-0.04f, 0.04f) + Time / 80f) % 1f, 0.8f, 0.6f);
                    dust.scale = 1.3f;
                    dust.fadeIn = 1f;
                    dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    dust.velocity = Vector2.Zero;
                    dust.noGravity = true;
                }
            }

            {
                // === 独立随机偏移以避免多发同步 ===
                if (Projectile.ai[1] == 0f)
                {
                    Projectile.ai[1] = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4); // 每发火花独立生成随机偏转范围
                }
                float randomOffset = (float)Projectile.ai[1];

                if (Time > 60f)
                {
                    NPC target = Projectile.Center.ClosestNPCAt(1800);
                    if (target != null)
                    {
                        Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = direction.RotatedBy(randomOffset) * 18f; // 独立随机偏转追踪
                    }
                }
                else
                {
                    Projectile.ai[2]++; // 占位
                }

                if (Time > 30f)
                {
                    float updatedTime = Time - 30f;
                    if (updatedTime % 120f > 90f)
                    {
                        Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.PiOver2 / 20f * (0.8f + Main.rand.NextFloat(0.4f))); // 随机微调
                    }
                    else if (updatedTime % 120f > 30f)
                    {
                        Projectile.velocity = Projectile.velocity.RotatedBy((float)Math.Sin((updatedTime - 30f) % 60f / 60f * MathHelper.TwoPi) * MathHelper.ToRadians(15f) * (0.8f + Main.rand.NextFloat(0.4f)));
                    }
                }
            }

            Time++;
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                // 🚩 Dust 爆散，狂野程度 ×3
                for (int i = 0; i < 60; i++) // 原20 ×3
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustType);
                    dust.color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f);
                    dust.scale = Main.rand.NextFloat(1.0f, 1.8f); // 放大范围
                    dust.velocity = Main.rand.NextVector2Circular(10f, 10f); // 加快扩散
                    dust.noGravity = true;
                }

                // 🚩 可选：线性粒子补充动感
                for (int i = 0; i < 15; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        velocity,
                        false,
                        40,
                        1.2f,
                        Color.Cyan
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }



    }
}
