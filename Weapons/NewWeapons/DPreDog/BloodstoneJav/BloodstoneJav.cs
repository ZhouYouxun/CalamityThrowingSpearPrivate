using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav;
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
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.HeartSword;
using CalamityMod;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.BloodstoneJav
{
    public class BloodstoneJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.DPreDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 200; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 24; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.shoot = ModContent.ProjectileType<BloodstoneJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 16f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

            Item.autoReuse = true;
            Item.channel = true; // 允许持续按住左键
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
                Item.damage = 240;
                Item.useTime = 75;
                Item.useAnimation = 75;
                Item.useLimitPerAnimation = 1;
                Item.shoot = ModContent.ProjectileType<BloodstoneRIGHT>();
                Item.shootSpeed = 12f;
                Item.UseSound = SoundID.Item1;
                Item.useStyle = ItemUseStyleID.Swing;
            }
            else // 左键
            {
                Item.damage = 360;
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.useLimitPerAnimation = 1;
                Item.shootSpeed = 5f;
                Item.shoot = ModContent.ProjectileType<BloodstoneJavPROJ>();
                Item.UseSound = null;
                Item.autoReuse = true;
                Item.channel = true;
                Item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) // 右键逻辑
            {
                float[] angles = { -12f, -8f, -4f, 0f, 4f, 8f, 12f }; // 扇形发射角度数组

                // 异步发射弹幕
                Task.Run(async () =>
                {
                    for (int i = 0; i < angles.Length; i++)
                    {
                        // 每次发射时重新获取玩家位置
                        Vector2 currentPosition = player.MountedCenter;

                        // 计算当前角度的速度
                        Vector2 rotatedVelocity = velocity.RotatedBy(MathHelper.ToRadians(angles[i]));

                        // 发射弹幕
                        Projectile.NewProjectile(
                            source,
                            currentPosition, // 使用动态获取的玩家位置
                            rotatedVelocity,
                            type,
                            damage,
                            knockback,
                            player.whoAmI
                        );

                        // 每两发之间延迟 5 帧（~83 毫秒）
                        await Task.Delay(83);
                    }
                });

                return false; // 阻止生成默认弹幕
            }

            {
                // 遍历当前世界中的所有弹幕
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == Item.shoot)
                    {
                        // 检查是否为 Aim 状态
                        if (proj.ModProjectile is BloodstoneJavPROJ BJ && BJ.CurrentState == BloodstoneJavPROJ.BehaviorState.Aim)
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

       
        }


        public override void ModifyTooltips(List<TooltipLine> list) => list.FindAndReplace("[GFB]", this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<HeartSword>(1);
            recipe.AddIngredient<BloodstoneCore>(4);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}
