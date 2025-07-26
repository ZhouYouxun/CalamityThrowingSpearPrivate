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
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;
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
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav
{
    public class SoulHunterJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.DPreDog";
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
            Item.useTime = Item.useAnimation = 45; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();
            Item.shoot = ModContent.ProjectileType<SoulHunterJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 27f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }

        public override void ModifyTooltips(List<TooltipLine> list) => list.FindAndReplace("[GFB]", this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<Surfeiter>(1);
            recipe.AddIngredient<ReaperTooth>(6);
            recipe.AddIngredient<RuinousSoul>(5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}

