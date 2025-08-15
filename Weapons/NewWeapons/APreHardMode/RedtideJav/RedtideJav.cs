using CalamityMod.Items.Materials;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.WulfrimJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Weapons.Melee;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.YateveoBloomC;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav
{
    public class RedtideJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.APreHardMode";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 25; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 45; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<RedtideJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 17f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // 右键
            {
                Item.damage = 22;
                Item.shoot = ModContent.ProjectileType<RedtideJavRight>();
                Item.useTime = Item.useAnimation = 32; // 快速
                Item.shootSpeed = 2f; // 保持或可微调
            }
            else // 左键
            {
                Item.damage = 32;
                Item.shoot = ModContent.ProjectileType<RedtideJavPROJ>();
                Item.useTime = Item.useAnimation = 50; // 慢速
                Item.shootSpeed = 17f; // 保持或可微调
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) // 右键：单发直射
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            }
            else // 左键：散射三发 ±10° 且伤害速度浮动
            {
                //int projectiles = 3;
                //for (int i = 0; i < projectiles; i++)
                //{
                //    // ±10° 随机散射
                //    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f));
                //    Vector2 perturbedSpeed = velocity.RotatedBy(angleOffset) * Main.rand.NextFloat(0.9f, 1.1f); // 速度浮动 ±10%
                //    int variedDamage = (int)(damage * Main.rand.NextFloat(0.9f, 1.1f)); // 伤害浮动 ±10%
                //    Projectile.NewProjectile(source, position, perturbedSpeed, type, variedDamage, knockback, player.whoAmI);
                //}

                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            }
            return false; // 防止默认再发射一次
        }

        //public override void AddRecipes()
        //{
        //    Recipe recipe = CreateRecipe();
        //    recipe.AddIngredient<RedtideSpear>();
        //    //recipe.AddTile(TileID.Anvils);
        //    recipe.Register();
        //}

    }
}
