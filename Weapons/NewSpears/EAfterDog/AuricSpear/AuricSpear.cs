using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityMod.Rarities;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
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
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewSpears.EAfterDog.AuricSpear
{
    internal class AuricSpear : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 8848; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = Item.useAnimation = 30;
            Item.knockBack = 8.5f;
            //Item.UseSound = SoundID.Item1;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
            Item.shoot = ModContent.ProjectileType<AuricSpearHoldOut>(); // 使用新的弹幕
            Item.shootSpeed = 25f; // 更改使用时的武器弹幕飞行速度

            Item.autoReuse = true;
            Item.channel = true; // 允许持续按住左键
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 遍历当前世界中的所有弹幕，检查是否已经存在 `AuricSpearHoldOut`
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<AuricSpearHoldOut>())
                {
                    return false; // 如果已经存在一个 `AuricSpearHoldOut`，阻止新的生成
                }
            }

            // 如果没有，创建新的 `AuricSpearHoldOut`
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false; // 阻止生成默认弹幕
        }

    }
}

