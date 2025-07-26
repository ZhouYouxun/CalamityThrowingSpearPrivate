using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee.Spears;
using CalamityMod.Items.Materials;
using CalamityMod.Items;
using CalamityMod;
using CalamityMod.Items.Weapons.Melee
;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.DiseasedPikeC
{
    public class DiseasedJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "ChangedWeapons.CPreMoodLord";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 50; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 45; // 更改使用时的武器攻击速度
            Item.knockBack = 2.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.shoot = ModContent.ProjectileType<DiseasedJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 17f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3; i++) // 发射三发弹幕
            {
                // 为每一发生成一个 -3° 到 +3° 的随机角度
                float randomOffset = MathHelper.ToRadians(Main.rand.Next(-3, 4));
                Vector2 perturbedVelocity = velocity.RotatedBy(randomOffset); // 应用偏移角度

                // 创建弹幕
                Projectile.NewProjectile(
                    source,
                    position,
                    perturbedVelocity,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );
            }

            return false; // 不执行默认发射逻辑
        }

        //public override void AddRecipes()
        //{
        //    Recipe recipe = CreateRecipe();
        //    recipe.AddIngredient<DiseasedPike>();
        //    recipe.Register();
        //}
    }
}

