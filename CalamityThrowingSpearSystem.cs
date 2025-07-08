using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace CalamityThrowingSpear
{
    public class CalamityThrowingSpearSystem : ModSystem
    {
        //private int starsofDestinyCooldown = 0;
        //public override void PostUpdatePlayers()
        //{
        //    if (Main.netMode == NetmodeID.Server)
        //        return;

        //    Player player = Main.LocalPlayer;
        //    if (starsofDestinyCooldown > 0)
        //    {
        //        starsofDestinyCooldown--;
        //        return;
        //    }

        //    if (player.HeldItem?.type == ModContent.ItemType<Weapons.NewWeapons.DPreDog.StarsofDestiny.StarsofDestiny>())
        //    {
        //        for (int i = 0; i < 30; i++)
        //        {
        //            int proj = Projectile.NewProjectile(
        //                Entity.GetSource_None(),
        //                player.Center + Main.rand.NextVector2Circular(16f, 16f),
        //                Vector2.Zero,
        //                ModContent.ProjectileType<Weapons.NewWeapons.DPreDog.StarsofDestiny.StarsofDestinyINV>(),
        //                0,
        //                0f,
        //                player.whoAmI
        //            );
        //            Main.projectile[proj].timeLeft = 2;
        //        }
        //        starsofDestinyCooldown = 60 * 60 * 10; // 10分钟冷却 
        //    }
        //}
    }
}
