using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Placeables;
using CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC
{
    public class TenebreusTidesJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "ChangedWeapons.CPreMoodLord";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 440; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 30; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.Calamity().donorItem = true;
            Item.shoot = ModContent.ProjectileType<TenebreusTidesJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 10f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

            Item.autoReuse = true;
            Item.channel = true; // 允许持续按住左键
        }


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //// 左键攻击保护机制 - 检测是否已经存在指定类型的弹幕
            //foreach (Projectile proj in Main.projectile)
            //{
            //    if (proj.active && proj.owner == player.whoAmI && proj.type == type) // 检查是否已经存在左键攻击的弹幕
            //    {
            //        return false; // 如果已存在，则阻止生成新的弹幕
            //    }
            //}


            // 遍历当前世界中的所有弹幕
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == Item.shoot)
                {
                    // 检查是否为 Aim 状态
                    if (proj.ModProjectile is TenebreusTidesJavPROJ TTJ && TTJ.CurrentState == TenebreusTidesJavPROJ.BehaviorState.Aim)
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
}
