using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.StreamGougeC
{
    public class StreamGougeJavEXP : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            if (Main.getGoodWorld)
            {
                Projectile.width = Projectile.height = 3975;
            }
            else
            {
                Projectile.width = Projectile.height = 575;
            }
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 300);
        }

        public override void AI()
        {

        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
