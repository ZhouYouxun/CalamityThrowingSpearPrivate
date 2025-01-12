//using CalamityMod.Items.Weapons.Melee;
//using CalamityMod.Items;
//using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria;
//using CalamityMod.Items.Materials;
//using CalamityMod.Rarities;
//using CalamityMod;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian
//{
//    public class PrimeMeridian : ModItem
//    {
//        public override void SetDefaults()
//        {
//            Item.width = 44;
//            Item.height = 50;
//            Item.damage = 100000; // 设置伤害值
//            Item.DamageType = DamageClass.Melee; // 设置为近战武器
//            Item.noMelee = true;
//            Item.useTurn = true;
//            Item.noUseGraphic = true;
//            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
//            Item.useTime = Item.useAnimation = 50; // 更改使用时的武器攻击速度
//            Item.knockBack = 8.5f;
//            Item.UseSound = SoundID.Item1;
//            Item.autoReuse = true;
//            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
//            Item.rare = ModContent.RarityType<HotPink>();
//            Item.Calamity().devItem = true;
//            Item.shoot = ModContent.ProjectileType<PrimeMeridianPROJ>(); // 使用新的弹幕
//            Item.shootSpeed = 25f; // 更改使用时的武器弹幕飞行速度
//        }


//        public override void AddRecipes()
//        {
//            Recipe recipe = CreateRecipe();
//            recipe.AddIngredient(ItemID.Zenith, 1);
//            recipe.AddIngredient<Nadir>();
//            recipe.AddIngredient<ArkoftheCosmos>();
//            recipe.AddIngredient<ShadowspecBar>(5);
//            recipe.Register();
//        }
//    }
//}

