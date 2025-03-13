using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using System;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.BraisedPorkSpear
{
    internal class BraisedPorkSpearPROJ : BaseSpearProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 11;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        // 设定较慢的运动参数
        public override float InitialSpeed => 2.5f;
        public override float ReelbackSpeed => 1.2f;
        public override float ForwardSpeed => 0.6f;

        // 回收前释放 BraisedPorkSpearCloud，方向始终向上，初速度 5f
        public override Action<Projectile> EffectBeforeReelback => (proj) =>
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                                     Projectile.Center,
                                     Vector2.UnitY * -5f, // 始终向上
                                     ModContent.ProjectileType<BraisedPorkSpearCloud>(),
                                     Projectile.damage / 2, // 伤害为原伤害的一半
                                     0f,
                                     Projectile.owner);
        };

        // 飞行期间释放紫色毒气粒子
        public override void ExtraBehavior()
        {
            if (Main.rand.NextBool(3)) // 33% 概率
            {
                Dust poisonGas = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch,
                                                    Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f,
                                                    100, Color.MediumPurple, 1.5f);
                poisonGas.noGravity = true;
                poisonGas.velocity *= 0.3f;
            }
        }

        // 命中敌人时释放 15 个紫色毒气 HeavySmokeParticle
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVelocity = Main.rand.NextVector2Circular(2f, 2f);
                Color poisonColor = Color.Lerp(Color.Purple, Color.DarkViolet, Main.rand.NextFloat()); // 混合紫色毒雾

                Particle poisonSmoke = new HeavySmokeParticle(Projectile.Center,
                                                              dustVelocity * Main.rand.NextFloat(1f, 2.6f),
                                                              poisonColor,
                                                              18,
                                                              Main.rand.NextFloat(0.9f, 1.6f),
                                                              0.35f,
                                                              Main.rand.NextFloat(-1, 1),
                                                              true);
                GeneralParticleHandler.SpawnParticle(poisonSmoke);
            }
        }
    }
}
