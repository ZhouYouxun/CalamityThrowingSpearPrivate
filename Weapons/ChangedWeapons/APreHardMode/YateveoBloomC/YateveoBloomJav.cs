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

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.YateveoBloomC
{
    public class YateveoBloomJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "ChangedWeapons.APreHardMode";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 48; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 70; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.Calamity().donorItem = true;
            Item.shoot = ModContent.ProjectileType<YateveoBloomJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 10f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }


        //public override void AddRecipes()
        //{
        //    CreateRecipe().
        //        AddIngredient(ItemID.JungleRose).
        //        AddIngredient(ItemID.RichMahogany, 15).
        //        AddIngredient(ItemID.JungleSpores, 12).
        //        AddIngredient(ItemID.Stinger, 4).
        //        AddIngredient(ItemID.Vine, 2).
        //        AddTile(TileID.Anvils).
        //        Register();
        //}
    }
}