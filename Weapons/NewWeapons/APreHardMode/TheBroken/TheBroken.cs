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

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.TheBroken
{
    public class TheBroken : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            //ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true; // 允许右键连续攻击
        }
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 9; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 30; // 更改使用时的武器攻击速度
            Item.knockBack = 1.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<TheBrokenPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 11f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

        }

        //public override bool AltFunctionUse(Player player) => true; // 启用右键功能

        //public override bool CanUseItem(Player player)
        //{
        //    if (player.altFunctionUse == 2) // 右键
        //    {
        //        Item.damage = 50; // 右键伤害较低
        //        Item.useTime = Item.useAnimation = 30; // 更改使用时的武器攻击速度
        //        Item.shootSpeed = 20f; // 右键弹幕速度更快
        //    }
        //    else // 左键
        //    {
        //        Item.damage = 70; // 左键伤害较高
        //        Item.useTime = Item.useAnimation = 15; // 更改使用时的武器攻击速度
        //        Item.shootSpeed = 15f; // 左键弹幕速度较慢
        //    }
        //    return base.CanUseItem(player);
        //}
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronBar, 1);
            recipe.AddIngredient(ItemID.Wood, 2);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}
