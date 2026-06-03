using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityThrowingSpear.Weapons.DeveloperWeapons;

namespace CalamityThrowingSpear.Weapons.DeveloperWeapons.Sunset.DSeed
{
    internal class SunsetSEEDItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;

            Item.maxStack = 999;
            Item.consumable = true;

            Item.value = Item.buyPrice(0, 0, 10, 0);
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = false;

            // 仅此一行：把一切放置行为交给 Tile 系统
            Item.DefaultToPlaceableTile(ModContent.TileType<SunsetSEEDTile>());
        }

        public override void HoldItem(Player player)
        {
            if (!DeveloperWeaponInfoUI.CanUseInfoItem(player, Type))
                return;

            if (!Main.mouseRight || !Main.mouseRightRelease)
                return;

            Main.mouseRightRelease = false;
            DeveloperWeaponInfoUI.ShootInfoPanel(player, player.GetSource_ItemUse(Item), DeveloperWeaponInfoPanelId.SunsetSeed);
        }





    }
}
