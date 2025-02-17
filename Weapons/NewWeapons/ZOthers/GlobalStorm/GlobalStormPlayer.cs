using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.ZOthers.GlobalStorm
{
    public class GlobalStormPlayer : ModPlayer
    {
        private int cloudProjID = -1;

        public override void PostUpdate()
        {
            if (Player.HeldItem.type == ModContent.ItemType<GlobalStorm>())
            {
                if (cloudProjID < 0 || !Main.projectile[cloudProjID].active)
                {
                    cloudProjID = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center - new Vector2(0, 60), Vector2.Zero, ModContent.ProjectileType<GlobalStormCloud>(), (int)Player.GetTotalDamage<RangedDamageClass>().ApplyTo(Player.HeldItem.damage), 0f, Player.whoAmI);
                }
            }
            else if (cloudProjID >= 0 && Main.projectile[cloudProjID].active)
            {
                Main.projectile[cloudProjID].Kill();
                cloudProjID = -1;
            }
        }
    }
}
