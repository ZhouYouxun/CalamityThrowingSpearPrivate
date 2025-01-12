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
using System.Linq;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.DiseasedPikeC;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PolarEssenceJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.InfiniteDarknessJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.ChaosWindJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.TerraLance;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SHPCK;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.HellionFlowerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TheOtherMiracleMatterJav
{
    public class TheOtherMiracleMatterJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 1200; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 70; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.shoot = ModContent.ProjectileType<TheOtherMiracleMatterJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 33f; // 更改使用时的武器弹幕飞行速度
        }


        public override void AddRecipes()
        {
            // 这把武器是跟群体，范围攻击相关联的
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<ElementalLanceJav>(); // 元素长枪-多功能武器
            recipe.AddIngredient<SHPCK>(); // SHPCK-典型的群体类武器
            recipe.AddIngredient<TerraLance>(); // 泰拉巨枪-轮椅群体武器
            recipe.AddIngredient<ChaosWindJav>(); // 风暴长矛-慢速超大范围群体武器
            recipe.AddIngredient<MiracleMatter>();
            recipe.AddTile(TileType<DraedonsForge>());
            recipe.Register();
        }
    }
}

