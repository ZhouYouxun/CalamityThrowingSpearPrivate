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
using CalamityMod;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.GraniteJav
{
    public class GraniteJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.APreHardMode";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 24; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 22; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<GraniteJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 10f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        //public override bool AltFunctionUse(Player player) => true;

        /*public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 1)
            {
                // 右键：奇特飞行模式（混乱扰动弹幕）
                Item.shootSpeed = 10f;
            }
            else
            {
                // 左键：箭矢式平飞，包含“死亡下落”逻辑
                Item.useTime = Item.useAnimation = 18;
                Item.shootSpeed = 12f;
            }
            return base.CanUseItem(player);
        }*/

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (proj.WithinBounds(Main.maxProjectiles))
            {
                Main.projectile[proj].localAI[0] = player.altFunctionUse == 1 ? 1f : 0f; // 右键 = 特殊飞行，左键 = 平飞
            }
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Granite, 9);
            recipe.AddIngredient(ItemID.Marble, 9);
            recipe.AddRecipeGroup("AnyGoldBar", 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();  
        }





    }
}
