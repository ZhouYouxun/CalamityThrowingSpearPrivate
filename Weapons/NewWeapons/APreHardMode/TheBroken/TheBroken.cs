using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.GraniteJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.TheBroken
{
    public class TheBroken : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true; // 允许右键连续攻击
        }
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 10; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 30; // 更改使用时的武器攻击速度
            Item.knockBack = 1.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<TheBrokenPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 10f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

        }

        //public override bool AltFunctionUse(Player player) => true; // 启用右键功能

        /*public override bool CanUseItem(Player player)
        {
            *if (player.altFunctionUse == 2) // 右键：扎入模式
            {
                Item.damage = 13; // 左键+5伤害
                Item.useTime = Item.useAnimation = 70; // 比左键慢
                Item.shootSpeed = 13f; // 比左键快 2f
            }
            else // 左键：正常扔刀雨模式
            {
                Item.damage = 9;
                Item.useTime = Item.useAnimation = 30;
                Item.shootSpeed = 11f;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(
                source,
                position,
                velocity,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            if (proj.WithinBounds(Main.maxProjectiles))
            {
                Main.projectile[proj].localAI[0] = player.altFunctionUse == 2 ? 1f : 0f; // <<< 正确赋值
            }

            return false; // 阻止自动发射
        }*/


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SilverBar, 1);
            recipe.AddIngredient(ItemID.Wood, 2);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}
