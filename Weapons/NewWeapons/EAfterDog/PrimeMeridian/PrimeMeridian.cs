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
using CalamityMod.Rarities;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian
{
    public class PrimeMeridian : ModItem
    {
        // 这把武器的知识点包括:
        // 1. 长按举起长柄武器，并进行一段时间的蓄力间隔攻击
        // 此攻击方式特别适用于法杖
        // 2. 自定义挥舞
        // 懂的都懂，不多解释
        // 3. 右键切换形态
        // 由于右键不支持长按，而这两种攻击方式都是长按，因此我整了一套右键切换形态逻辑，点击右键在两种形态之间循环切换
        // 4. 传统激光的绘制与编写
        // 5. 许多小的有的没的的弹幕
        // 6. 更多着色器的使用
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
            Item.shoot = ModContent.ProjectileType<PrimeMeridianHouldOut>(); // 使用新的弹幕
            Item.shootSpeed = 25f; // 更改使用时的武器弹幕飞行速度

            Item.autoReuse = true;
            Item.channel = true; // 允许持续按住左键
        }
        private int currentMode = 0; // 0 = 形态1（PrimeMeridianHouldOut），1 = 形态2（PrimeMeridianRIGHT）
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Item.type] = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // 右键切换形态
            {
                // 播放切换音效
                SoundEngine.PlaySound(SoundID.Item149, player.position);

                // 切换形态（0 <-> 1）
                currentMode = 1 - currentMode;

                // 根据模式更新武器的弹幕类型
                if (currentMode == 0)
                {
                    Item.shoot = ModContent.ProjectileType<PrimeMeridianHouldOut>();
                    CombatText.NewText(player.getRect(), Color.Cyan, "法杖形态");
                }
                else
                {
                    Item.shoot = ModContent.ProjectileType<PrimeMeridianRIGHT>();
                    CombatText.NewText(player.getRect(), Color.Orange, "长枪形态");
                }

                return false; // 右键不会发射弹幕，只负责切换形态
            }
            else // 左键攻击
            {
                Item.damage = 3500;
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.shootSpeed = 25f;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.channel = true;

                // 依据当前模式设置弹幕
                if (currentMode == 0)
                    Item.shoot = ModContent.ProjectileType<PrimeMeridianHouldOut>();
                else
                    Item.shoot = ModContent.ProjectileType<PrimeMeridianRIGHT>();
            }

            return base.CanUseItem(player);
        }


        // 参考对象：
        // ArkOfTheAncients_SwungBlade
        // ExobladeProj
        // TerratomereHoldoutProj
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 左键攻击保护机制 - 检测是否已经存在指定类型的弹幕
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == type) // 检查是否已存在左键攻击的弹幕
                {
                    return false; // 如果已存在，则阻止生成新的弹幕
                }
            }

            // 左键攻击逻辑 - 创建新的弹幕
            int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false; // 阻止默认弹幕            
        }






        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Zenith, 1);
            recipe.AddIngredient<Nadir>();
            recipe.AddIngredient<ShadowspecBar>(5);
            recipe.Register();
        }
    }
}
