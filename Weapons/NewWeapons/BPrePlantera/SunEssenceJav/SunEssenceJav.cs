using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.FestiveHalberd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Materials;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
{
    public class SunEssenceJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.BPrePlantera";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 15; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 22; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.shoot = ModContent.ProjectileType<SunEssenceJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 24f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup("AnyCobaltBar", 5);
            recipe.AddIngredient(ItemID.Starfury, 1); // 星怒
            recipe.AddIngredient<EssenceofSunlight>(4); // 日光精华
            recipe.AddIngredient(ItemID.SunplateBlock, 150); // 日盘块
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }





    }
}
