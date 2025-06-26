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
    /// <summary>
    /// Vortex 元素投掷弹幕，命中后失效，向上飞行并释放新的弹幕。
    /// 同时拥有击中特效和死亡特效，特效由有序粒子与无序 Dust 构成。
    /// </summary>
    public class ElementalLanceJavPROJVortex : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/DPreDog/ElementalLanceC/ElementalLanceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";

        private bool triggered = false; // 是否已经击中过敌人
        private int releaseTimer = 0; // 击中后计时器

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
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
            Projectile.localNPCHitCooldown = 14;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarProj").Value;
            Color trailColor = new Color(0, 128, 128);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], trailColor * 0.3f, 1);
            return false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.DarkGreen.ToVector3() * 0.55f);

            // ✴️ 无论状态如何都释放持续飞行粒子
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1.5f, 3.5f);
                SquareParticle trail = new SquareParticle(Projectile.Center, vel, false, 20, Main.rand.NextFloat(1.0f, 1.6f), Color.Teal);
                GeneralParticleHandler.SpawnParticle(trail);
            }
            if (Main.rand.NextBool(4))
            {
                int dustType = Utils.SelectRandom(Main.rand, 99, 202, 229);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType);
                dust.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
                dust.scale = Main.rand.NextFloat(0.9f, 1.5f);
                dust.noGravity = true;
            }

            // ⛔ 拖尾仅在未触发命中前播放
            if (!triggered && Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(0, 128, 128);
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

        
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 300);

            if (!triggered)
            {
                triggered = true;
                Projectile.friendly = false;

                // 强制朝上飞行 + 设置存活时间
                Projectile.velocity = Vector2.UnitY * -18f; // 朝上
                Projectile.timeLeft = 20; // 保留时间为 X 帧

                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi * i / 24f;
                    Vector2 vel = angle.ToRotationVector2() * 4f;
                    SquareParticle p = new SquareParticle(Projectile.Center, vel, false, 30, 1.7f + Main.rand.NextFloat(0.6f), Color.Cyan * 1.5f);
                    GeneralParticleHandler.SpawnParticle(p);
                }

                for (int i = 0; i < 36; i++)
                {
                    int dustType = Utils.SelectRandom(Main.rand, 99, 202, 229);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType);
                    dust.velocity = Main.rand.NextVector2Circular(6, 6);
                    dust.scale = Main.rand.NextFloat(1.4f, 2.2f);
                    dust.noGravity = true;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 32; i++)
            {
                float angle = MathHelper.TwoPi * i / 32f;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                SquareParticle p = new SquareParticle(Projectile.Center, vel, false, 30, 2.0f + Main.rand.NextFloat(0.3f), Color.Cyan);
                GeneralParticleHandler.SpawnParticle(p);
            }

            for (int i = 0; i < 30; i++)
            {
                int dustType = Utils.SelectRandom(Main.rand, 99, 202, 229);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType);
                d.velocity = Main.rand.NextVector2Circular(5, 5);
                d.scale = Main.rand.NextFloat(1.5f, 2.4f);
                d.fadeIn = 0.5f;
                d.noGravity = true;
            }

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<ElementalLanceJavPROJV>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

        }
    }
}
