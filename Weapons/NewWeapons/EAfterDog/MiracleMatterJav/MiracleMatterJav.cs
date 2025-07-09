using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using System.Linq;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.TerraLance;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.DiseasedPikeC;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PolarEssenceJav;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.InfiniteDarknessJav;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.BansheeHookC;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.MiracleMatterJav
{
    public class MiracleMatterJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 135; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = 10;
            Item.useAnimation = 45;
            Item.useLimitPerAnimation = 3;
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.shoot = ModContent.ProjectileType<MiracleMatterJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 24f; // 更改使用时的武器弹幕飞行速度
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.zenithWorld)
            {
                int numProjectiles = 8;
                float totalSpread = MathHelper.ToRadians(3f * (numProjectiles - 1)); // 总夹角
                float startRotation = -totalSpread / 2f; // 左边起始角

                for (int i = 0; i < numProjectiles; i++)
                {
                    float rotation = startRotation + MathHelper.ToRadians(3f) * i;
                    Vector2 perturbedSpeed = velocity.RotatedBy(rotation);
                    Projectile.NewProjectile(
                        source,
                        position,
                        perturbedSpeed,
                        type,
                        damage,
                        knockback,
                        player.whoAmI
                    );
                }

                // 防止默认再发一发，避免重复
                return false;
            }

            // 默认情况下正常发射
            return true;
        }

        public override void AddRecipes()
        {
            // 这把武器对应的是单体，粘附相关联的
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<DiseasedJav>(); // 瘟疫枪-单体高速武器
            recipe.AddIngredient<BansheeHookJav>(); // 女妖之爪-回旋式回收式追踪单体+群体武器
            recipe.AddIngredient<InfiniteDarknessJav>(); // 无边黑暗-单体武器
            recipe.AddIngredient<SoulHunterJav>(); // 魂狩，较慢速单体武器
            recipe.AddIngredient<MiracleMatter>();
            recipe.AddTile(TileType<DraedonsForge>());
            recipe.Register();
        }
    }
}
