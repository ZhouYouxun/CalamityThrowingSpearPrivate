using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    public class ElementalLanceJavPROJNebula : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/DPreDog/ElementalLanceC/ElementalLanceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";

        private bool hasTracking = false;
        private bool hitEnemy = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarProj").Value;
            Color trailColor = new Color(128, 0, 128);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], trailColor * 0.3f, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.55f);

            if (!hitEnemy)
            {
                Projectile.velocity *= 1.01f;

                // 飞行拖尾特效（未命中时保留）
                if (Projectile.numUpdates % 3 == 0)
                {
                    Color outerSparkColor = new Color(128, 0, 128);
                    float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                    float outerSparkScale = 1.2f + scaleBoost;
                    SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 更复杂粒子效果
                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror);
                    d.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(1.2f, 1.8f);
                }

                if (Main.rand.NextBool(5))
                {
                    SquareParticle pulse = new SquareParticle(
                        Projectile.Center,
                        Main.rand.NextVector2Circular(2f, 2f),
                        false,
                        30,
                        1.4f,
                        Color.MediumPurple * 0.8f
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }
            else
            {
                // 命中后衰减
                Projectile.velocity *= 0.96f;
                Projectile.Opacity -= 0.02f;
                if (Projectile.Opacity <= 0f)
                    Projectile.Kill();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitEnemy = true;
            hasTracking = false;

            Projectile.netUpdate = true;
            Projectile.friendly = false;

            target.AddBuff(ModContent.BuffType<ElementalMix>(), 300);

            // 命中粒子效果
            for (int i = 0; i < 20; i++)
            {
                Vector2 dir = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleCrystalShard);
                d.velocity = dir * Main.rand.NextFloat(2f, 4f);
                d.scale = Main.rand.NextFloat(1f, 1.5f);
                d.fadeIn = 0.5f;
                d.noGravity = true;
            }

            // 生成 NebulaSLASH（原有）
            int slashCount = Main.rand.Next(2, 4);
            for (int i = 0; i < slashCount; i++)
            {
                Vector2 dir = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir,
                    ModContent.ProjectileType<NebulaSLASH>(), (int)(Projectile.damage * 0.75), Projectile.knockBack, Projectile.owner);
            }

            // 环绕目标发射激光
            int count = Main.rand.Next(3, 6);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(1000f, 1000f);
                Vector2 spawn = target.Center + offset;
                Vector2 velocity = (target.Center - spawn).SafeNormalize(Vector2.Zero) * 12f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawn,
                    velocity,
                    ModContent.ProjectileType<ElementalLanceJavPROJN>(), // 你后续要制作的激光弹幕
                    (int)(Projectile.damage * 0.6f),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 无序粒子：紫色烟雾特效 + Dust 混合
            for (int i = 0; i < 25; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    velocity,
                    Color.MediumPurple * 0.8f,
                    18,
                    Main.rand.NextFloat(0.9f, 1.6f),
                    0.35f,
                    Main.rand.NextFloat(-1f, 1f),
                    false
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            for (int i = 0; i < 20; i++)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch);
                d.velocity = dir * Main.rand.NextFloat(3f, 6f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.4f, 2.2f);
            }
        }




    }
}
