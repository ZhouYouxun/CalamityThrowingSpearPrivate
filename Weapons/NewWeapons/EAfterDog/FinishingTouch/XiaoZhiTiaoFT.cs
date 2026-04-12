using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;
using CalamityMod.Items;
using CalamityMod.Rarities;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class XiaoZhiTiaoFT : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;
            // 困难模式前：Orange，价值15金
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 15, 0, 0);
            Item.value = Item.sellPrice(0, 15, 0, 0);

            //Item.width = 44;
            //Item.height = 50;
            //Item.damage = 520;
            //Item.DamageType = DamageClass.Melee;
            //Item.noMelee = true;
            //Item.useTurn = true;
            //Item.noUseGraphic = true;
            //Item.useStyle = ItemUseStyleID.Swing;
            //Item.useTime = Item.useAnimation = 180;
            //Item.knockBack = 8.5f;
            //Item.UseSound = SoundID.Item1;
            //Item.autoReuse = true;
            //Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            //Item.rare = ModContent.RarityType<HotPink>();
            //Item.Calamity().devItem = true;
            //Item.shoot = ModContent.ProjectileType<FinishingTouchEPROJ>();
            //Item.shootSpeed = 0f;
            //Item.crit = 15;
        }

        //public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        //{
        //    // 使用鼠标位置作为发射位置
        //    Vector2 mousePosition = Main.MouseWorld;

        //    // 发射方向为垂直向下，速度为 5f
        //    Vector2 direction = new Vector2(0, 5f);

        //    // 从鼠标位置发射投射物
        //    Projectile.NewProjectile(source, mousePosition, direction, type, damage, knockback, player.whoAmI);
        //    return false; // 返回 false 以避免默认发射逻辑
        //}


        //public override void UpdateAccessory(Player player, bool hideVisual)
        //{
        //    // 综合属性提升
        //    player.GetDamage(DamageClass.Generic) += 0.2f; // 所有职业的攻击力
        //    player.GetAttackSpeed(DamageClass.Generic) += 0.2f; // 所有职业的攻击速度
        //}


        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            int index = tooltips.FindLastIndex(x => x.Mod == "Terraria" && x.Name.StartsWith("Tooltip"));
            if (index != -1)
            {
                if (Main.keyState.PressingShift())
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "ShiftTooltip", GetHjsonText("Items.XiaoZhiTiaoFT.ShiftTipDetailed"))
                    { OverrideColor = Color.LightBlue });
                else
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "NormalTooltip", GetHjsonText("Items.XiaoZhiTiaoFT.ShiftTip"))
                    { OverrideColor = Color.Gray });
            }
        }

        private static string GetHjsonText(string key) => Language.GetTextValue($"Mods.CalamityThrowingSpear.{key}");
    }
}
