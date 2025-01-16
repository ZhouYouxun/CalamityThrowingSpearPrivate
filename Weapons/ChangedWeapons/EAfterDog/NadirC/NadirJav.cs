using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.StarnightLanceC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Rarities;
using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.NadirC
{
    public class NadirJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "ChangedWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BonusAttackSpeedMultiplier[Item.type] = 0.0f; // 禁用全局攻速加成效果
        }
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 110; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.useTurn = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 16; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.Calamity().donorItem = true;
            Item.shoot = ModContent.ProjectileType<NadirJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 23f; // 更改使用时的武器弹幕飞行速度
        }

        //public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        //{
        //    Projectile.NewProjectile(source, position, velocity, type, damage / 2, knockback, player.whoAmI);
        //    return false;
        //}
        //public override void AddRecipes()
        //{
        //    CreateRecipe().
        //        AddIngredient<ElementalLanceJav>().
        //        AddIngredient<AuricBar>(5).
        //        AddIngredient<TwistingNether>(5).
        //        AddIngredient<DarksunFragment>(8).
        //        AddTile<CosmicAnvil>().
        //        Register();
        //}
    }
}
