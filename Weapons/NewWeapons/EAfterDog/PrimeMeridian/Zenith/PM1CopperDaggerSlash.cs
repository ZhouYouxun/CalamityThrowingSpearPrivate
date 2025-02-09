using CalamityMod.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian.Zenith
{
    internal class PM1CopperDaggerSlash : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Melee/ExobeamSlash";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 2;
            Projectile.Opacity = 1f;
            Projectile.timeLeft = 35;
            Projectile.MaxUpdates = 2;
            Projectile.scale = 0.75f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 12;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Opacity = Projectile.timeLeft / 35f;

            // 橙色刀光特效
            if (Projectile.timeLeft == 34)
            {
                Particle spark2 = new GlowSparkParticle(Projectile.Center, new Vector2(0.1f, 0.1f).RotatedByRandom(100), false, 12, Main.rand.NextFloat(0.03f, 0.05f), Color.White, new Vector2(2, 0.5f), true);
                GeneralParticleHandler.SpawnParticle(spark2);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox.TopLeft(), targetHitbox.Size());

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(Color.White, Color.White, Projectile.identity / 7f % 1f) * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
