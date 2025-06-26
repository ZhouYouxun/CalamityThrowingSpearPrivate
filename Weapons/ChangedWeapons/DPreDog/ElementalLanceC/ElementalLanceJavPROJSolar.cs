using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Typeless; // 引入爆炸用弹幕类型

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    /// <summary>
    /// Solar 元素标枪的投掷弹幕逻辑。
    /// 命中敌人后会周期性追击并捶打目标，每次命中后有追踪延迟与华丽特效。
    /// </summary>
    public class ElementalLanceJavPROJSolar : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/DPreDog/ElementalLanceC/ElementalLanceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";

        private int targetID = -1;
        private int chaseCooldown = 0;
        private int hitCount = 0;
        private const int MaxHits = 5;
        private bool justStartedChasing = false;
        private float curveDirection = 1f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarProj").Value;
            Color trailColor = new Color(255, 69, 0);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], trailColor * 0.3f, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            if (targetID != -1)
                Projectile.rotation += 0.35f;
            else
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            if (targetID != -1 && chaseCooldown <= 0)
            {
                if (Main.rand.NextBool(3))
                {
                    SemiCircularSmearVFX semiSmear = new SemiCircularSmearVFX(
                        Projectile.Center,
                        Color.Yellow * Main.rand.NextFloat(0.78f, 0.85f),
                        Main.rand.NextFloat(-8, 8),
                        Main.rand.NextFloat(1.2f, 1.3f) * 2.1f,
                        new Vector2(1f, 0.8f)
                    );
                    GeneralParticleHandler.SpawnParticle(semiSmear);
                }

                if (justStartedChasing)
                {
                    curveDirection = Main.rand.NextBool() ? 1f : -1f;
                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 dir = -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.ToRadians(20));
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare);
                        dust.velocity = dir * Main.rand.NextFloat(4f, 9f);
                        dust.scale = Main.rand.NextFloat(1.2f, 2.0f);
                        dust.noGravity = true;
                    }
                    justStartedChasing = false;
                }

                NPC target = Main.npc[targetID];
                if (target != null && target.active && !target.friendly)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float t = (float)Math.Sin((600 - Projectile.timeLeft) * 0.12f);
                    float speed = MathHelper.Lerp(8f, 32f, (t + 1f) / 2f);
                    direction = direction.RotatedBy(MathHelper.ToRadians(12f) * curveDirection);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, 0.2f);
                }
            }
            else
            {
                Projectile.velocity *= 1.01f;
                if (Projectile.numUpdates % 3 == 0 && targetID == -1)
                {
                    Color outerSparkColor = new Color(255, 69, 0);
                    float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                    float outerSparkScale = 1.2f + scaleBoost;
                    SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            if (chaseCooldown > 0)
                chaseCooldown--;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 300);

            // ✴️ 每次命中都生成爆炸弹幕
            int explosionType = ModContent.ProjectileType<FuckYou>();
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, explosionType, Projectile.damage, Projectile.knockBack, Projectile.owner);

            if (targetID == -1)
            {
                targetID = target.whoAmI;
                chaseCooldown = 30;
                justStartedChasing = true;
            }
            else
            {
                hitCount++;
                chaseCooldown = 30;
                justStartedChasing = true;

                // 🌟 更复杂的命中特效：有序+无序结合的多重粒子效果
                for (int i = 0; i < 15; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Particle trail = new SparkParticle(Projectile.Center, velocity * 0.2f, false, 60, 1.2f + Main.rand.NextFloat(0.3f), Color.Orange);
                    GeneralParticleHandler.SpawnParticle(trail);
                }
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi * i / 16f;
                    Vector2 dir = angle.ToRotationVector2();
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + dir * 6f, DustID.Torch);
                    dust.velocity = dir * 4f;
                    dust.noGravity = true;
                    dust.scale = 1.6f;
                }
                for (int i = 0; i < 30; i++)
                {
                    Dust chaos = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24, 24), DustID.SolarFlare);
                    chaos.velocity = Main.rand.NextVector2Circular(8f, 8f);
                    chaos.scale = Main.rand.NextFloat(1.5f, 2.5f);
                    chaos.fadeIn = 0.6f;
                    chaos.noGravity = true;
                }

                if (hitCount >= MaxHits)
                    Projectile.Kill();
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 💫 华丽退场特效
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36;
                Vector2 offset = angle.ToRotationVector2() * 16f;
                Vector2 velocity = offset.SafeNormalize(Vector2.UnitY) * 6f;
                Particle spark = new SparkParticle(Projectile.Center + offset, velocity, false, 40, 1.5f, Color.Orange);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            for (int i = 0; i < 20; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare);
                d.velocity = Main.rand.NextVector2Circular(6, 6);
                d.scale = Main.rand.NextFloat(1.5f, 2.5f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }
        }
    }
}
