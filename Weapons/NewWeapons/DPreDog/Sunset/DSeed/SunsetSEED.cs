using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed
{
    // 种子物品：放置到地面上生成 1×1 幼苗 Tile（SunsetSEEDTile1）
    public class SunsetSEED : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 可以加 Tooltip 显示成长说明（可选）
            // DisplayName.SetDefault("Sunset Seed");
            // Tooltip.SetDefault("可在坚实地面上种下，60天后长成 3×3 的成熟植株。");
        }

        public override void SetDefaults()
        {
            // 贴图使用同名 SunsetSEED.png（已存在）
            Item.width = 20;
            Item.height = 20;

            Item.maxStack = 999;
            Item.useStyle = ItemUseStyleID.Swing; // 挥动式放置
            Item.useTime = 10;
            Item.useAnimation = 15;
            Item.useTurn = true;
            Item.autoReuse = true;

            Item.consumable = true;                                     // 消耗
            Item.createTile = ModContent.TileType<SunsetSEEDTile1>();   // 放置幼苗 Tile
            Item.placeStyle = 0;

            Item.rare = ItemRarityID.White;
            Item.value = Item.buyPrice(silver: 1);
        }
    }
}
