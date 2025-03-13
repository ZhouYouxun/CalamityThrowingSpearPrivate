using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityMod.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewSpears.CPreMoodLord.TidalMechanicsSpear
{
    internal class TidalMechanicsSpear : ModItem
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
            Item.shoot = ModContent.ProjectileType<TidalMechanicsSpearHoldOutSP>(); // 使用新的弹幕
            Item.shootSpeed = 25f; // 更改使用时的武器弹幕飞行速度

            Item.autoReuse = true;
            Item.channel = true; // 允许持续按住左键
        }
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Item.type] = true;
        }

     
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



    }
}
