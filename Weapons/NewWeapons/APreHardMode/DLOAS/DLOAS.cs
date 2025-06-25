using CalamityMod.Items.Materials;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.BraisedPorkJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.DLOAS
{
    internal class DLOAS : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.APreHardMode";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 12; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 36; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.shoot = ModContent.ProjectileType<DLOASPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 12f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int owner = player.whoAmI;

            // **1. 生成主弹幕 `DLOASPROJ`**
            int projID = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<DLOASPROJ>(), damage, knockback, owner);

            //// **2. 生成 3 条蛇**
            //for (int i = 0; i < 3; i++)
            //{
            //    // **在玩家中心附近的 `半径 32` 圆圈内随机选择一个生成点**
            //    float angle = Main.rand.NextFloat(0, MathHelper.TwoPi); // 0 到 360° 的随机角度
            //    Vector2 spawnOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 32f; // 计算偏移量
            //    Vector2 spawnPosition = player.Center + spawnOffset; // 计算最终生成位置

            //    // **生成头部**
            //    int prev = Projectile.NewProjectile(source, spawnPosition, velocity, ModContent.ProjectileType<DLOASSnake1Head>(), damage, knockback, owner, projID);

            //    // **生成身体**
            //    for (int j = 0; j < 3; j++)
            //    {
            //        prev = Projectile.NewProjectile(source, spawnPosition, velocity, ModContent.ProjectileType<DLOASSnake2Body>(), damage, knockback, owner, prev);
            //    }

            //    // **生成尾巴**
            //    int tailID = Projectile.NewProjectile(source, spawnPosition, velocity, ModContent.ProjectileType<DLOASSnake3Tail>(), damage, knockback, owner, prev);
            //}

            return false; // **防止 `Item.shoot` 额外生成 `DLOASPROJ`**
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.MysticCoilSnake, 1); // 耍蛇者长笛
            recipe.AddIngredient(ItemID.HellstoneBar, 6); // 狱岩锭
            recipe.AddIngredient(ItemID.Hotdog, 1); // 热狗：骨蛇3.33概率掉落
            //recipe.AddIngredient(ItemID.Skelehead, 1); // 骨蛇之头：地狱里的跟骨蛇相关的一幅画
            recipe.AddTile(TileID.Hellforge);
            recipe.Register();
        }
    }
}
