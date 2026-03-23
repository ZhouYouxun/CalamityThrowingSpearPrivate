using CalamityMod.Items.Materials;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.MiracleMatterJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Placeables.Furniture.CraftingStations;
using CalamityMod.Rarities;
using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Sagittarius
{
    public class Sagittarius : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 70; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = 12;
            Item.useAnimation = 60;
            Item.useLimitPerAnimation = 4;
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item114;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
            Item.shoot = ModContent.ProjectileType<SagittariusPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 15f; // 更改使用时的武器弹幕飞行速度
        }
        public override bool AltFunctionUse(Player player) => false; // 右键功能
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
    Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 随机一个方向（360°）
            Vector2 randomDir = Main.rand.NextVector2Unit();

            // 保持原有速度大小
            Vector2 newVelocity = randomDir * velocity.Length();

            // 发射弹幕
            Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI);

            return false; // 阻止原本默认发射
        }


        //public override void AddRecipes()
        //{
        //    Recipe recipe = CreateRecipe();
        //    recipe.AddIngredient(ItemID.PiercingStarlight, 1);
        //    //recipe.AddIngredient<BrinyBaron>();
        //    recipe.AddTile(TileID.MythrilAnvil);
        //    recipe.Register();
        //}
    }
}