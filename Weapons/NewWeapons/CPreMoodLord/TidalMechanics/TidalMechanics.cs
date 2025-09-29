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
using CalamityMod.Rarities;
using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.Yharon;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics
{
    public class TidalMechanics : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            //ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 85;
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 45; // 左键攻击时间
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
            Item.shoot = ModContent.ProjectileType<TidalMechanicsPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 38f; // 更改使用时的武器弹幕飞行速度
        }
        public override bool AltFunctionUse(Player player) => true; // 启用右键功能

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // 右键攻击
            {
                var bubblePlayer = player.GetModPlayer<TidalMechanicsBubblePLAYER>();

                // 检查是否存在泡泡
                if (player.ownedProjectileCounts[ModContent.ProjectileType<TidalMechanicsBubbles>()] > 0)
                {
                    // 如果泡泡存在，移除泡泡
                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<TidalMechanicsBubbles>())
                        {
                            proj.Kill(); // 移除泡泡
                        }
                    }
                    return false; // 保持冷却时间不变，右键无效
                }
                else if (bubblePlayer.CanSpawnBubble()) // 泡泡不存在且冷却结束
                {
                    Item.useTime = Item.useAnimation = 10;
                    //Item.UseSound = SoundID.Item149;
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/潮汐右键生成泡泡"));

                    Item.shoot = ModContent.ProjectileType<TidalMechanicsBubbles>();
                    Item.shootSpeed = 0f; // 泡泡生成在玩家位置，不需要速度

                    // 开始冷却计时
                    bubblePlayer.StartBubbleCooldown();
                }
                else
                {
                    return false; // 右键无效
                }
            }

            else // 左键攻击
            {
                //// 在左键攻击时检查泡泡是否存在
                //if (player.ownedProjectileCounts[ModContent.ProjectileType<TidalMechanicsBubbles>()] > 0)
                //{
                //    return false; // 如果泡泡存在，左键无效
                //}

                Item.useTime = Item.useAnimation = 45;
                Item.UseSound = SoundID.Item1;
                Item.shoot = ModContent.ProjectileType<TidalMechanicsPROJ>();
                Item.shootSpeed = 25f;
            }
            return base.CanUseItem(player);
        }

        //public override void AddRecipes()
        //{
        //    Recipe recipe = CreateRecipe();
        //    recipe.AddIngredient(ItemID.RazorbladeTyphoon, 1);
        //    recipe.AddIngredient<BrinyBaron>();
        //    recipe.AddTile(TileID.MythrilAnvil);
        //    recipe.Register();
        //}
    }
}

