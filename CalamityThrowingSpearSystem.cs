using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace CalamityThrowingSpear
{
    public class CalamityThrowingSpearSystem : ModSystem
    {
        public static bool HasGivenTidalMechanicsTablet;
        public static bool HasGivenTidalMechanicsWeapon;
        public static bool HasGivenSagittariusTablet;
        public static bool HasGivenSagittariusWeapon;

        public override void OnWorldLoad()
        {
            HasGivenTidalMechanicsTablet = false;
            HasGivenTidalMechanicsWeapon = false;
            HasGivenSagittariusTablet = false;
            HasGivenSagittariusWeapon = false;
        }

        public override void OnWorldUnload()
        {
            HasGivenTidalMechanicsTablet = false;
            HasGivenTidalMechanicsWeapon = false;
            HasGivenSagittariusTablet = false;
            HasGivenSagittariusWeapon = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (HasGivenTidalMechanicsTablet)
            {
                tag["HasGivenTidalMechanicsTablet"] = true;
            }

            if (HasGivenTidalMechanicsWeapon)
            {
                tag["HasGivenTidalMechanicsWeapon"] = true;
                tag["HasGivenTidalMechanicsReward"] = true;
            }

            if (HasGivenSagittariusTablet)
            {
                tag["HasGivenSagittariusTablet"] = true;
            }

            if (HasGivenSagittariusWeapon)
            {
                tag["HasGivenSagittariusWeapon"] = true;
                tag["HasGivenSagittariusReward"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            bool legacyTidalReward = tag.GetBool("HasGivenTidalMechanicsReward");
            bool legacySagittariusReward = tag.GetBool("HasGivenSagittariusReward");

            HasGivenTidalMechanicsTablet = tag.GetBool("HasGivenTidalMechanicsTablet") || legacyTidalReward;
            HasGivenTidalMechanicsWeapon = tag.GetBool("HasGivenTidalMechanicsWeapon");
            HasGivenSagittariusTablet = tag.GetBool("HasGivenSagittariusTablet") || legacySagittariusReward;
            HasGivenSagittariusWeapon = tag.GetBool("HasGivenSagittariusWeapon");
        }
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

        //    if (player.HeldItem?.type == ModContent.ItemType<Weapons.DeveloperWeapons.StarsofDestiny.StarsofDestiny>())
        //    {
        //        for (int i = 0; i < 30; i++)
        //        {
        //            int proj = Projectile.NewProjectile(
        //                Entity.GetSource_None(),
        //                player.Center + Main.rand.NextVector2Circular(16f, 16f),
        //                Vector2.Zero,
        //                ModContent.ProjectileType<Weapons.DeveloperWeapons.StarsofDestiny.StarsofDestinyINV>(),
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
