using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    internal class SurfeiterPlayer : ModPlayer
    {
        public override void PostUpdateEquips()
        {
            Player player = Main.LocalPlayer;

            // 检测玩家是否手持 Surfeiter 武器
            if (player.HeldItem.type == ModContent.ItemType<Surfeiter>())
            {
                bool drumExists = false;

                // 检查是否已存在 SurfeiterDrum
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<SurfeiterDrum>())
                    {
                        drumExists = true;
                        break;
                    }
                }

                // 如果不存在则生成
                if (!drumExists)
                {
                    Projectile.NewProjectile(player.GetSource_Accessory(player.HeldItem),
                        player.Center + new Vector2(0, -15 * 16),
                        Vector2.Zero,
                        ModContent.ProjectileType<SurfeiterDrum>(),
                        1,
                        0,
                        player.whoAmI);
                }
            }
            else
            {
                // 删除所有 SurfeiterDrum 弹幕
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<SurfeiterDrum>())
                    {
                        proj.Kill();
                    }
                }
            }
        }

    }
}
