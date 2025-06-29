using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.GoldplumeSpearC
{
    public class GoldplumeJavWindPlayer : ModPlayer
    {
        private Projectile goldplumeWind; // 记录生成的 GoldplumeJavWind

        public override void PostUpdate()
        {
            // 检查玩家是否手持 GoldplumeJav
            if (Player.HeldItem.type == ModContent.ItemType<GoldplumeJav>())
            {
                // 如果没有生成 GoldplumeJavWind，则生成
                if (goldplumeWind == null || !goldplumeWind.active)
                {
                    goldplumeWind = Projectile.NewProjectileDirect(
                        Player.GetSource_FromThis(),
                        Player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<GoldplumeJavWind>(),
                        0,
                        0,
                        Player.whoAmI
                    );
                }
            }
            else
            {
                // 如果不再手持 GoldplumeJav，则删除 GoldplumeJavWind
                if (goldplumeWind != null && goldplumeWind.active)
                {
                    goldplumeWind.Kill();
                    goldplumeWind = null;
                }
            }
        }
    }
}