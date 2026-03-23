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
using CalamityMod.Items.Weapons.Melee
;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.StarnightLanceC
{
    public class StarnightLanceJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "ChangedWeapons.BPrePlantera";

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 84; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 15; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.shoot = ModContent.ProjectileType<StarnightLanceJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 10f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }


        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CryonicBar>(12).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}

