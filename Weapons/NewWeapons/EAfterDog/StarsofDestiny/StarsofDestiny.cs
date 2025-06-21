using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SHPCK;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.StarsofDestiny
{
    internal class StarsofDestiny : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 6500; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 30; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.autoReuse = true;

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;

            Item.shoot = ModContent.ProjectileType<StarsofDestinyLEFT>(); // 使用新的弹幕
            Item.shootSpeed = 10f; // 更改使用时的武器弹幕飞行速度
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
                Item.damage = 750;
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.shoot = ModContent.ProjectileType<StarsofDestinyRIGHT>();
                Item.shootSpeed = 15f;
                Item.UseSound = SoundID.Item1;
                Item.useStyle = ItemUseStyleID.Swing;
            }
            else // 左键
            {
                Item.damage = 1500;
                Item.useTime = 18;
                Item.useAnimation = 18;
                Item.shootSpeed = 5f; 
                Item.shoot = ModContent.ProjectileType<StarsofDestinyLEFT>();
                Item.UseSound = null;
                Item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(player);
        }


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 遍历当前世界中的所有弹幕
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == Item.shoot)
                {
                    // 检查是否为 Aim 状态
                    if (proj.ModProjectile is StarsofDestinyLEFT SDL && SDL.CurrentState == StarsofDestinyLEFT.BehaviorState.Aim)
                    {
                        return false; // 如果已经存在一个 Aim 状态的弹幕，阻止新的生成
                                      // Fire 阶段的弹幕不会影响这个判断
                    }
                }
            }

            // 区分左键和右键逻辑
            if (player.altFunctionUse == 2) // 右键逻辑
            {
                // 添加偏移角度 ±X°
                float randomOffset = Main.rand.NextFloat(-0f, 0f);
                Vector2 adjustedVelocity = velocity.RotatedBy(MathHelper.ToRadians(randomOffset));

                // 创建右键的弹幕
                Projectile.NewProjectile(source, position, adjustedVelocity, type, damage, knockback, player.whoAmI);
            }
            else // 左键逻辑
            {
                // 创建新的弹幕
                int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            }
            return false; // 阻止生成默认弹幕
        }






    }
}
