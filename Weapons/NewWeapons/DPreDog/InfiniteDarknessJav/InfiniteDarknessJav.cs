using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.BloodstoneJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Rarities;
using CalamityMod.Items.Materials;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ElectrocutionHalberd;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.InfiniteDarknessJav
{
    public class InfiniteDarknessJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.DPreDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 114; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 8; // 更改使用时的武器攻击速度
            Item.knockBack = 18f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.shoot = ModContent.ProjectileType<InfiniteDarknessJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 8f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ThrowingKnife,100);
            recipe.AddIngredient<TwistingNether>(3); 
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }

    }
}
