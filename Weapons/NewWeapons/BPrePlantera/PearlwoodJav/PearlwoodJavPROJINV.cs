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

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav
{
    internal class PearlwoodJavPROJINV : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;            
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 100;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

        }

        public override void AI()
        {
            //Projectile.timeLeft = 2;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 通知玩家更新计数器
            if (Main.player[Projectile.owner].GetModPlayer<PearlwoodJavPLAYER>() is PearlwoodJavPLAYER player)
            {
                player.IncrementHitCounter();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
