using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;
using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using Terraria.Localization;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.WulfrimSpear
{
    internal class WulfrimSpearPROJ : BaseSpearProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 28;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 90;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
        }

        public override float InitialSpeed => 3f;
        public override float ReelbackSpeed => 1f;
        public override float ForwardSpeed => 0.75f;
        public override Action<Projectile> EffectBeforeReelback => (proj) =>
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0.8f, ModContent.ProjectileType<AuricJavBallPROJ>(), Projectile.damage, Projectile.knockBack * 0.85f, Projectile.owner, 0f, 0f);
        };
    }
}
