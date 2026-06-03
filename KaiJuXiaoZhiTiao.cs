using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using CalamityThrowingSpear.Weapons.DeveloperWeapons;

namespace CalamityThrowingSpear
{
    public class KaiJuXiaoZhiTiao : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;
            //Item.accessory = true;
            // 困难模式前：Orange，价值15金
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 15, 0, 0);
            Item.value = Item.sellPrice(0, 15, 0, 0);
            DeveloperWeaponInfoUI.ApplyInfoItemDefaults(Item);

        }
        public override bool CanUseItem(Player player) => DeveloperWeaponInfoUI.CanUseInfoItem(player, Type);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return DeveloperWeaponInfoUI.ShootInfoPanel(player, source, DeveloperWeaponInfoPanelId.Guide);
        }

    }
}
