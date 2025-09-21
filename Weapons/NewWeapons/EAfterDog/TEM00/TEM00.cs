using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SHPCK;
using CalamityMod.Items;
using CalamityMod.Rarities;
using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.TheBroken;
using CalamityMod.Tiles.Furniture.CraftingStations;
using static Terraria.ModLoader.ModContent;



namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00
{
    internal class TEM00 : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 1; // 示例伤害
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6.5f;
            Item.useTime = Item.useAnimation = 30; // 决定挥舞速度（影响开枪速率）
            Item.noUseGraphic = true; // 持枪时隐藏物品贴图（使用弹幕显示）
            Item.noMelee = true; // 不直接造成近战伤害（由弹幕控制）

            //Item.UseSound = SoundID.Item1;

            Item.shoot = ModContent.ProjectileType<TEM00Left>(); // 调用专用【蓄力型弹幕】
            Item.shootSpeed = 6f; // 初始速度，真实速度由弹幕控制

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;

            Item.useStyle = ItemUseStyleID.Shoot; // 以投掷方式显示持枪动作[这很重要，否则手会一直挥!]
            Item.autoReuse = true; // 这很重要，否则他会直接飞出去
            Item.channel = true; // 这是最重要的，这是核心
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
                Item.damage = 2500; // 右键伤害
                Item.useTime = 10;
                Item.useAnimation = 45;
                Item.useLimitPerAnimation = 2;
                Item.shoot = ModContent.ProjectileType<TEM00Right>(); // 右键发射 TEM00Right
                Item.shootSpeed = 30f;
                Item.UseSound = SoundID.Item73; // 激光音效
            }
            else // 左键
            {
                Item.damage = 6666;
                Item.useTime = Item.useAnimation = 60;
                Item.shootSpeed = 15f;
                Item.shoot = ModContent.ProjectileType<TEM00Left>(); // 左键发射 TEM00Left
                Item.UseSound = null;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) // 右键逻辑
            {
                // 右键直接发射 TEM00Right，不需要像左键那样限制蓄力弹幕
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<TEM00Right>(), damage, knockback, player.whoAmI);
            }
            else // 左键逻辑（蓄力）
            {
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == Item.shoot)
                    {
                        if (proj.ModProjectile is TEM00Left p && p.CurrentState == TEM00Left.BehaviorState.Aim)
                            return false; // 已有蓄力状态弹幕存在时阻止生成新弹幕
                    }
                }

                // 生成用于蓄力的弹幕
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<TEM00Left>(), damage, knockback, player.whoAmI);
            }

            return false; // 阻止默认射弹
        }


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<TheBroken>(1);
            recipe.AddIngredient<ShadowspecBar>(5);
            recipe.AddTile(TileType<DraedonsForge>());
            recipe.Register();
        }



    }
}
