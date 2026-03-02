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
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC
{
    public class ScourgeoftheCosmosJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "ChangedWeapons.EAfterDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 140; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = 15;
            Item.useAnimation = 25; // 更改使用时的武器攻击速度
            Item.useLimitPerAnimation = 2;
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item109;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.shoot = ModContent.ProjectileType<ScourgeoftheCosmosJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 19f; // 更改使用时的武器弹幕飞行速度
        }
        //public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        //{
        //    float offsetDistance = 24f; // 两个弹幕之间的间距

        //    // 计算平行偏移方向
        //    Vector2 offset = Vector2.Normalize(velocity.RotatedBy(MathHelper.PiOver2)) * offsetDistance;

        //    // 生成第一发弹幕（左侧）
        //    Projectile.NewProjectile(source, position - offset, velocity, type, damage, knockback, player.whoAmI);
        //    // 生成第二发弹幕（右侧）
        //    Projectile.NewProjectile(source, position + offset, velocity, type, damage, knockback, player.whoAmI);

        //    return false; // 阻止默认的弹幕生成，确保只发射这两发
        //}

        //public override void AddRecipes()
        //{
        //    CreateRecipe().
        //        AddIngredient(ItemID.ScourgeoftheCorruptor).
        //        AddIngredient<Bonebreaker>().
        //        AddIngredient<CosmiliteBar>(10).
        //        AddTile<CosmicAnvil>().
        //        Register();
        //}
    }
}
