using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee.Spears;
using CalamityMod.Items.Materials;
using CalamityMod.Items;
using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.CustomRecipes;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.GalvanizingGlaiveC
{
    public class GalvanizingGlaiveJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "ChangedWeapons.CPreMoodLord";
        public override void SetDefaults()
        {
            CalamityGlobalItem modItem = Item.Calamity();
            Item.width = 44;
            Item.height = 50;
            Item.damage = 120; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 22; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.shoot = ModContent.ProjectileType<GalvanizingGlaiveJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 25f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
            //modItem.UsesCharge = true;
            //modItem.MaxCharge = 135f;
            //modItem.ChargePerUse = 0.075f;
        }


        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MysteriousCircuitry>(12).
                AddIngredient<DubiousPlating>(18).
                AddIngredient<InfectedArmorPlating>(10).
                AddIngredient<LifeAlloy>(5).
                AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(3, out Func<bool> condition), condition).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}

