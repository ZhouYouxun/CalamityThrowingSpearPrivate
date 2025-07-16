using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    /// <summary>
    /// Nebula 系能量激光弹幕。飞行时释放脉冲圈，命中后释放华丽紫色尘埃。
    /// </summary>
    public class ElementalLanceJavPROJN : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/Ranged/AMRShot"; // 可替换为自定义贴图？

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 600;
            Projectile.alpha = 100;
            Projectile.extraUpdates = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 模拟 NitroShot 式拖尾闪光
            if (Projectile.timeLeft < 597 && Projectile.timeLeft > 450)
            {
                AltSparkParticle spark = new AltSparkParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.05f,
                    false,
                    15,
                    1.0f,
                    Color.MediumPurple * 0.15f
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 能量环脉冲（每 10 帧）
            if (Projectile.ai[0] % 10 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Particle pulse = new DirectionalPulseRing(
                        Projectile.Center,
                        Projectile.velocity * 0.5f,
                        Color.MediumPurple * 0.6f,
                        new Vector2(1f, 2.3f),
                        Projectile.rotation - MathHelper.PiOver4 - MathHelper.PiOver4,
                        0.22f,
                        0.035f,
                        20
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }

            Projectile.ai[0]++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 华丽尘埃爆散（无 spark）
            for (int i = 0; i < 24; i++)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleCrystalShard);
                d.velocity = offset * Main.rand.NextFloat(2f, 6f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.3f, 2.1f);
                d.fadeIn = 0.5f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.Center, Projectile.velocity, Projectile.width, Projectile.height);
            return true;
        }
    }
}
