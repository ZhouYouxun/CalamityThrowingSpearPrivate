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

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav
{
    public class RedtideJavEXP : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            //if (Main.getGoodWorld)
            //{
            //    Projectile.width = 3500;
            //    Projectile.height = 3500;
            //}
            //else
            {
                Projectile.width = 6 * 16;
                Projectile.height = 6 * 16;
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
           
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 240);
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
