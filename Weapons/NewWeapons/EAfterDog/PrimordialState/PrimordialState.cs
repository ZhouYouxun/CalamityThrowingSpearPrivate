using CalamityMod.Items.DraedonMisc;
using CalamityMod.Items;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav;
using CalamityMod.Projectiles.Pets;
using CalamityMod.Items.Materials;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PolarEssenceJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ChaosEssenceJav;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimordialState
{
    public class PrimordialState : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 64; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 60; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.shoot = ModContent.ProjectileType<PrimordialStatePROJ>(); // 使用新的弹幕
            Item.shootSpeed = 12.5f; // 更改使用时的武器弹幕飞行速度
        }


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<SunEssenceJav>();
            recipe.AddIngredient<PolarEssenceJav>();
            recipe.AddIngredient<ChaosEssenceJav>();
            recipe.AddIngredient<AuricBar>(5);
            recipe.AddIngredient(ItemID.SoulofLight, 1);
            recipe.AddIngredient(ItemID.SoulofNight, 1);
            recipe.AddTile<CosmicAnvil>();
            recipe.Register();
        }
    }
}
