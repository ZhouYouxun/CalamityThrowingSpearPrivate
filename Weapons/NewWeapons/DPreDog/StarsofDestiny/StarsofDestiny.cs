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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    public class StarsofDestiny : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 240; // 设置伤害值
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
            Item.channel = false; // 允许持续按住左键
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
                Item.damage = 120;
                Item.useTime = 24;
                Item.useAnimation = 24;
                Item.shoot = ModContent.ProjectileType<StarsofDestinyRIGHT>();
                Item.shootSpeed = 15f;
                Item.UseSound = SoundID.Item1;
                Item.useStyle = ItemUseStyleID.Swing;
            }
            else // 左键
            {
                Item.damage = 240;
                Item.useTime = 24;
                Item.useAnimation = 24;
                Item.shootSpeed = 5f; 
                Item.shoot = ModContent.ProjectileType<StarsofDestinyLEFT>();
                Item.UseSound = null;
                Item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(player);
        }


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // W + 左键 / 右键：特殊 SuperSOD
            if (player.controlUp)
            {
                // 统计并清空属于自己的 SODCLK50（“时刻”）
                int momentCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active &&
                        p.owner == player.whoAmI &&
                        p.type == ModContent.ProjectileType<SODCLK50>())
                    {
                        momentCount++;
                        p.Kill();
                    }
                }

                // ★ 若没有任何 SODCLK50，则拒绝发动 SuperSOD
                if (momentCount <= 0)
                {
                    return false;
                }

                // 至少 6 发星弹 + 消耗的时刻数，数量传到 SuperSOD 的 ai[2]
                Vector2 spawnPos = player.Center;
                Vector2 shootVel = new Vector2(0f, -Item.shootSpeed);
                int superDamageFix = momentCount * 2;
                int superDamage = 60 * superDamageFix; // 60x两倍时刻数 的伤害

                Projectile.NewProjectile(
                    source,
                    spawnPos,
                    shootVel,
                    ModContent.ProjectileType<SuperSOD>(),
                    superDamage,
                    knockback,
                    player.whoAmI,
                    0f,
                    0f,
                    momentCount
                );

                SoundEngine.PlaySound(SoundID.Item68, spawnPos);

                // 特殊技触发时，不再执行原有左右键逻辑
                return false;
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
