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
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.BraisedPorkJav;
using CalamityMod.Items.Weapons.Melee;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.SausageMakerC;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.EndlessDevourJav
{
    public class EndlessDevourJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.DPreDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 125; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = Item.useAnimation = 80; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = null;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.shoot = ModContent.ProjectileType<EndlessDevourJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 15f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

            Item.autoReuse = true;
            Item.channel = true; // 允许持续按住左键
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
           
            // 遍历当前世界中的所有弹幕
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == Item.shoot)
                {
                    // 检查是否为 Aim 状态
                    if (proj.ModProjectile is EndlessDevourJavPROJ BJ && BJ.CurrentState == EndlessDevourJavPROJ.BehaviorState.Aim)
                    {
                        return false; // 如果已经存在一个 Aim 状态的弹幕，阻止新的生成
                                      // Fire 阶段的弹幕不会影响这个判断
                    }
                }
            }

            // 左键攻击逻辑 - 创建新的弹幕
            int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false; // 阻止生成默认弹幕
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.MagicMirror, 1); // 两种邪恶长枪
            recipe.AddIngredient<DarkPlasma>(3);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();

            /*Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient<SausageMakerJav>(1); // 两种邪恶长枪
            recipe2.AddIngredient<DarkPlasma>(3);
            recipe2.AddTile(TileID.LunarCraftingStation);
            recipe2.Register();*/
        }






    }
}
