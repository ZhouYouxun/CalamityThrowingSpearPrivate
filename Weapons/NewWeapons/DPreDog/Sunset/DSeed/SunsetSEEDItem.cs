using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed
{
    internal class SunsetSEEDItem : ModItem
    {


        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;

            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.useAnimation = 15;
            Item.useTime = 10;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;

            // 放置你的植物 Tile
            Item.createTile = ModContent.TileType<SunsetSEED>();

            // 让它可以放在任何地面（普通家具机制）
            Item.placeStyle = 0;

            // 分类为放置类物品
            Item.value = Item.buyPrice(0, 0, 10, 0);
            Item.rare = ItemRarityID.Blue;
        }

        // 允许放置在任意地面
        public override bool CanUseItem(Player player)
        {
            // 不需要复杂判断，植物像熔岩百合同样允许在任意 solid tile 上生长
            return true;
        }
    }
}
