using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
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
using CalamityMod.Projectiles.Typeless;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    internal class SunsetASunsetLeft : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
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
            Projectile.penetrate = 3;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 获取枪头位置
            Vector2 gunHeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 0.35f;

            // 计算 PointParticle 旋转角度（左右摆动 30° 到 -30°）
            float oscillationAngle = MathHelper.ToRadians(30f) * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
            Vector2 particleVelocity = Projectile.velocity.RotatedBy(oscillationAngle) * -0.5f;

            // 生成 PointParticle（缩短生命周期）
            PointParticle trail = new PointParticle(
                gunHeadPosition,  // 在枪头位置生成
                particleVelocity, // 向后发射
                false,
                9, // 生命帧数减少到 60% (15 → 9)
                1.1f,
                Color.Gold
            );
            GeneralParticleHandler.SpawnParticle(trail);

            // 计算双螺旋特效（仍然使用 Dust）
            float phaseShift = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 1f;
            Vector2 leftOffset = Projectile.velocity.RotatedBy(MathHelper.PiOver2) * phaseShift;
            Vector2 rightOffset = Projectile.velocity.RotatedBy(-MathHelper.PiOver2) * phaseShift;

            // 生成 Dust（YellowTorch, IchorTorch）
            int dustType = Main.rand.NextBool() ? DustID.YellowTorch : DustID.IchorTorch;
            Dust.NewDustPerfect(Projectile.Center + leftOffset, dustType, -Projectile.velocity * 0.3f, Scale: 1.2f);
            Dust.NewDustPerfect(Projectile.Center + rightOffset, dustType, -Projectile.velocity * 0.3f, Scale: 1.2f);
        }

        public override void OnKill(int timeLeft)
        {


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 explosionPosition = Projectile.Center;

            // 生成爆炸粒子
            Particle explosion = new DetailedExplosion(
                explosionPosition,
                Vector2.Zero,
                Color.OrangeRed * 0.9f,
                Vector2.One,
                Main.rand.NextFloat(-5, 5),
                0.1f * 2.5f, // 修改原始大小
                0.28f * 2.5f, // 修改最终大小
                10
            );
            GeneralParticleHandler.SpawnParticle(explosion);

            // 额外生成多个小型爆炸粒子
            for (int i = 0; i < 5; i++)
            {
                Vector2 randomOffset = Main.rand.NextVector2Circular(16f, 16f);
                SparkleParticle impactParticle = new SparkleParticle(
                    explosionPosition + randomOffset,
                    Vector2.Zero,
                    Color.White,
                    Color.OrangeRed,
                    2.5f,
                    7,
                    0f,
                    2f
                );
                GeneralParticleHandler.SpawnParticle(impactParticle);
            }

            // 生成爆炸弹幕
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                explosionPosition,
                Vector2.Zero,
                ModContent.ProjectileType<FuckYou>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );

            target.AddBuff(ModContent.BuffType<SunsetASunsetEDebuff>(), 300); // 300 帧 = 5 秒
        }
    }
}