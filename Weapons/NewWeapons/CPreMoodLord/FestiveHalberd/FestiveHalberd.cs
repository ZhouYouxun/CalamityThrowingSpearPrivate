using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.GraniteJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.FestiveHalberd
{
    public class FestiveHalberd : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.CPreMoodLord";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 102; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 60; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.shoot = ModContent.ProjectileType<FestiveHalberdPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 10f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ChristmasTreeSword, 1); // 圣诞树剑
            recipe.AddIngredient(ItemID.Ectoplasm, 10);
            //recipe.AddIngredient(ItemID.NorthPole, 1); // 北极
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }





    }
}
