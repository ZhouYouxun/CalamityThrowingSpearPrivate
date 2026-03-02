using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureStatigel;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.Projectiles.Melee.BladecrestOathswordThrownBlade;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.ElectrocoagulationTenmonJav
{
    public class ElectrocoagulationTenmonJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.APreHardMode";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 20; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 15; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.shoot = ModContent.ProjectileType<ElectrocoagulationTenmonJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 10f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

        }


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<PurifiedGel>(18);
            recipe.AddIngredient<BlightedGel>(18);
            recipe.AddTile<StaticRefiner>();
            recipe.Register();
        }
    }
}
