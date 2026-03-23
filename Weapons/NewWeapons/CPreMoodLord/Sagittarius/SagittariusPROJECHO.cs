using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Sagittarius
{
    public class SagittariusPROJECHO : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Sagittarius/Sagittarius";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";

        public ref float Time => ref Projectile.ai[1];

        private int timer = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Color wrapColor = Color.LightGoldenrodYellow * 0.6f;

            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 3f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, wrapColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

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
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            timer++;

            // ===== 方向固定直线冲击 =====
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // ================= 保留充能环 =================
            float progress = MathHelper.Clamp(timer / 120f, 0f, 1f);
            float triggerChance = MathHelper.Lerp(0.2f, 1f, progress);

            if (Main.rand.NextFloat() < triggerChance)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(5f * 16f, 7f * 16f);
                Vector2 offset = angle.ToRotationVector2() * radius;

                CTSLightingBoltsSystem.Spawn_SagittariusEchoCharging(Projectile.Center + offset);
            }

            // ================= 前端推进粒子（弧线推进感） =================
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 futurePos = Projectile.Center + Projectile.velocity * 0.6f;

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel =
                    forward.RotatedByRandom(MathHelper.ToRadians(15f))
                    * Main.rand.NextFloat(4f, 8f);

                Particle spark = new GlowSparkParticle(
                    futurePos,
                    vel,
                    false,
                    10,
                    0.15f,
                    Color.Gold,
                    new Vector2(2.2f, 0.45f),
                    true,
                    false,
                    1
                );

                GeneralParticleHandler.SpawnParticle(spark);
            }

            // ================= 后方尾焰拖拽 =================
            if (Main.rand.NextBool(2))
            {
                Vector2 backPos = Projectile.Center - forward * 8f;

                Particle trail = new GlowSparkParticle(
                    backPos,
                    -forward * Main.rand.NextFloat(1f, 3f),
                    false,
                    12,
                    0.12f,
                    Color.LightGoldenrodYellow * 0.7f,
                    new Vector2(1.8f, 0.4f),
                    true,
                    false,
                    1
                );

                GeneralParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 0.55f);

            Time++;
        }

        public override bool? CanDamage() => Time >= 10f;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 pos = target.Center;
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            // ================= 核心爆闪 =================
            for (int i = 0; i < 10; i++)
            {
                Particle core = new GlowSparkParticle(
                    pos,
                    Main.rand.NextVector2Circular(1f, 1f),
                    false,
                    6,
                    0.22f,
                    Color.Gold,
                    new Vector2(1.6f, 0.6f),
                    true,
                    false,
                    1
                );
                GeneralParticleHandler.SpawnParticle(core);
            }

            // ================= 前向冲击喷流 =================
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel =
                    forward.RotatedByRandom(MathHelper.ToRadians(10f))
                    * Main.rand.NextFloat(6f, 12f);

                Particle jet = new GlowSparkParticle(
                    pos,
                    vel,
                    false,
                    Main.rand.Next(8, 14),
                    Main.rand.NextFloat(0.12f, 0.2f),
                    new Color(255, 210, 80),
                    new Vector2(2.6f, 0.45f),
                    true,
                    false,
                    1
                );

                GeneralParticleHandler.SpawnParticle(jet);
            }

            // ================= 后方扇形召唤SPIT =================
            int spitCount = 12;
            float arc = MathHelper.ToRadians(120f);

            for (int i = 0; i < spitCount; i++)
            {
                float t = (float)i / (spitCount - 1);
                Vector2 backward = -Projectile.velocity.SafeNormalize(Vector2.UnitX);

                float angleOffset = MathHelper.Lerp(-arc / 2f, arc / 2f, t);
                Vector2 dir = backward.RotatedBy(angleOffset);
                Vector2 spawnPos = pos + dir * Main.rand.NextFloat(30f * 16f, 60f * 16f);

                Vector2 velocity = (pos - spawnPos).SafeNormalize(Vector2.UnitX) * 18f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velocity,
                    ModContent.ProjectileType<SagittariusSPIT>(),
                    Projectile.damage / 13,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            // ================= Debuff =================
            target.AddBuff(ModContent.BuffType<SagittariusEDebuff>(), 1200);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 60; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);

                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    vel,
                    new Color(255, 200, 100),
                    20,
                    Main.rand.NextFloat(1.2f, 2f),
                    0.4f,
                    Main.rand.NextFloat(-1, 1),
                    true
                );

                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
    }
}