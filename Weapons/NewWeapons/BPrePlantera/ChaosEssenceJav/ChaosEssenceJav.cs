using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ChaosEssenceJav
{
    public class ChaosEssenceJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.BPrePlantera";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 48; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 20; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.shoot = ModContent.ProjectileType<ChaosEssenceJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 15f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            //recipe.AddRecipeGroup("AnyCobaltBar", 5); // 一级锭
            //recipe.AddIngredient(ItemID.FieryGreatsword, 1); // 火山大剑
            recipe.AddIngredient<EssenceofHavoc>(10); // 混乱精华
            recipe.AddIngredient<BrimstoneSlag>(15); // 硫磺火石砖
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }





    }
}
