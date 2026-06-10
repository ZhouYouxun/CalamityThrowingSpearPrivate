using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityThrowingSpear.Global;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.DiseasedPikeC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.BansheeHookC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.NadirC;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PolarEssenceJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.InfiniteDarknessJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.TerraLance;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            Item.rare = ModContent.RarityType<BurnishedAuric>();
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
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<GildedProboscisJav>(); // 镀金鸟喙
            recipe.AddIngredient<AmidiasTridentJav>(); // 海王三叉戟-能够扎在别人身上
            recipe.AddIngredient<PearlwoodJav>(); // 珍珠木-直接做成了他的下位
            recipe.AddRecipeGroup(CTSRecipeGroups.NadirGroup); // 天底 / Nadir
            recipe.AddIngredient<MiracleMatter>();
            recipe.AddTile(TileType<DraedonsForge>());
            recipe.Register();
        }
        
    }
}
