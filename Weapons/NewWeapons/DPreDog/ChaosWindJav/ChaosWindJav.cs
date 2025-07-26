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
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Materials;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.ChaosWindJav
{
    public class ChaosWindJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.DPreDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 200;
            Item.damage = 90; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 15; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item122;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.shoot = ModContent.ProjectileType<ChaosWindJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 5f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }
        public override bool CanUseItem(Player player)
        {
            // 检查是否存在任何一个特定的投射物
            bool anyProjectileExists = Main.projectile.Any(proj => proj.active && (proj.type == ModContent.ProjectileType<ChaosWindJavPROJ>() || proj.type == ModContent.ProjectileType<ChaosWindJavElectromagneticBall>() || proj.type == ModContent.ProjectileType<ChaosWindJavAirburst>()));
            return !anyProjectileExists;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ThunderSpear, 1); // 风暴长矛
            recipe.AddIngredient<ArmoredShell>(3);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }

    }
}
