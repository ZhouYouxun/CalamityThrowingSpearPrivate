using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC
{
    public class GildedProboscisJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "ChangedWeapons.DPreDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 300; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 12; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.shoot = ModContent.ProjectileType<GildedProboscisJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 5f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }

        public override void UpdateInventory(Player player)
        {
            if (Main.zenithWorld)
                Item.SetNameOverride(this.GetLocalizedValue("GFBName"));
        }
        //public override void ModifyTooltips(List<TooltipLine> list) => list.FindAndReplace("[GFB]", this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));

        //public override void AddRecipes()
        //{
        //    Recipe recipe = CreateRecipe();
        //    recipe.AddIngredient<GildedProboscis>();
        //    recipe.Register();
        //}
    }
}
