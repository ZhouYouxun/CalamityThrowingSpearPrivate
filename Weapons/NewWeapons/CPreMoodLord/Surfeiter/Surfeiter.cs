using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.FestiveHalberd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    public class Surfeiter : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.CPreMoodLord";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 168; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 75; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.shoot = ModContent.ProjectileType<SurfeiterPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 20f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }

        public override bool AltFunctionUse(Player player) => true; // 允许右键功能

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // 右键
            {
                // 右键时不发射弹幕，只切换鼓的形态
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<SurfeiterDrum>())
                    {
                        if (proj.ModProjectile is SurfeiterDrum drum)
                        {
                            drum.SwitchForm(); // 切换形态
                            SoundEngine.PlaySound(SoundID.Item14, player.Center); // 播放音效
                        }
                    }
                }
                return false; // 阻止武器攻击
            }

            // 左键照常发射弹幕
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<FleshTotem>();
            recipe.AddIngredient(ItemID.Ectoplasm, 30);
            recipe.AddIngredient(ItemID.Bone, 99);
            recipe.AddTile(TileID.AdamantiteForge);
            recipe.Register();
        }
    }
}
