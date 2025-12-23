using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav
{
    public class PearlwoodJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.BPrePlantera";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 27; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 25; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item68;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.shoot = ModContent.ProjectileType<PearlwoodJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 15f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }

        //public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override bool CanUseItem(Player player)
        {
            // 如果需要仅播放一次（防止多次播放），可使用局部变量或 player.GetModPlayer<>().bool
            SoundStyle useSound = SoundID.Item68 with
            {
                Volume = 0.3f // 降低至 30% 音量
            };
            SoundEngine.PlaySound(useSound, player.Center);

            // 静音原始 UseSound
            Item.UseSound = null;

            return base.CanUseItem(player);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Pearlwood, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }





    }
}
