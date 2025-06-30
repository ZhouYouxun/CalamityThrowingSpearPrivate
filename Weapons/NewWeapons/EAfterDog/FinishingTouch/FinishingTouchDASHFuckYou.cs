using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;



namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class FinishingTouchDASHFuckYou : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 500;
        }

        public override void SetDefaults()
        {
            Projectile.width = 1080;
            Projectile.height = 1080;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3; // Lasts so long due to visuals.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 300; // Under absolutely no circumstances should this explosion hit more than once.
        }

        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => Projectile.direction = Main.player[Projectile.owner].direction;
    }
}
