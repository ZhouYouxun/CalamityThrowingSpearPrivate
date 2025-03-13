using CalamityMod.Items;
using CalamityMod.Rarities;
using CalamityThrowingSpear.Weapons.NewSpears.BPrePlantera.TheLastLanceSpear;
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
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.TheBrokenSpear
{
    internal class TheBrokenSpear : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 5000; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 50; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
            Item.shoot = ModContent.ProjectileType<TheBrokenSpearHoldOut>(); // 使用新的弹幕
            Item.shootSpeed = 25f; // 更改使用时的武器弹幕飞行速度

            Item.autoReuse = true;
            Item.channel = false;
        }
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Item.type] = true;
        }

        // 这是一套长按的逻辑

        //public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        //{
        //    // 左键攻击保护机制 - 检测是否已经存在指定类型的弹幕
        //    foreach (Projectile proj in Main.projectile)
        //    {
        //        if (proj.active && proj.owner == player.whoAmI && proj.type == type) // 检查是否已存在左键攻击的弹幕
        //        {
        //            return false; // 如果已存在，则阻止生成新的弹幕
        //        }
        //    }

        //    // 左键攻击逻辑 - 创建新的弹幕
        //    int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        //    return false; // 阻止默认弹幕            
        //}

        private int attackType = 0; // 记录当前攻击类型，0 = 普通挥舞，1 = 旋转攻击

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 确保当前武器只允许存在一个弹幕
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == type)
                {
                    return false; // 如果已经有弹幕存在，则不生成新的
                }
            }

            // 生成新的弹幕，并通过 ai[0] 传递攻击类型
            int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, attackType);

            // 轮流切换攻击模式，保证每次攻击不同
            attackType = (attackType + 1) % 2; // 0 = 普通挥舞，1 = 旋转攻击

            return false; // 阻止默认弹幕
        }




    }
}
