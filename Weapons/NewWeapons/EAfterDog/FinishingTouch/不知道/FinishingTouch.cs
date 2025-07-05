//using CalamityMod.Items;
//using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.BloodstoneJav;
//using CalamityMod.Sounds; // 引入 Calamity 声音库
//using System;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria;
//using Terraria.Audio;
//using CalamityMod.NPCs.Yharon;
//using Terraria.DataStructures;
//using CalamityMod.Rarities;
//using CalamityMod;
//using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.BotanicPiercerC;
//using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TyphonsGreedC;
//using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC;
//using CalamityMod.Items.Materials;
//using CalamityMod.Tiles.Furniture.CraftingStations;
//using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
//{
//    public class FinishingTouch : ModItem
//    {
//        public override void SetStaticDefaults()
//        {
//            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;

//            // 注册四帧动画，每六帧切换一次
//            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 4));
//            ItemID.Sets.AnimatesAsSoul[Type] = true;
//        }

//        public override void SetDefaults()
//        {
//            Item.width = 44;
//            Item.height = 50;
//            Item.damage = 3500;
//            Item.DamageType = DamageClass.Melee;
//            Item.noMelee = true;
//            Item.useTurn = true;
//            Item.noUseGraphic = true;
//            Item.useStyle = ItemUseStyleID.Swing;
//            Item.useTime = Item.useAnimation = 240;
//            Item.knockBack = 8.5f;
//            Item.UseSound = SoundID.Item1;
//            Item.autoReuse = true;
//            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
//            Item.rare = ModContent.RarityType<HotPink>();
//            Item.Calamity().devItem = true;
//            Item.shoot = ModContent.ProjectileType<FinishingTouchDASH>(); // 初始设置为蓄力冲刺弹幕
//            Item.shootSpeed = 0f;
//            Item.crit = 4;
//        }

//        public override bool AltFunctionUse(Player player) => true;
//        public override bool CanUseItem(Player player)
//        {
//            if (player.altFunctionUse == 2) // 检测是否为右键使用
//            {
//                // 播放龙吼音效
//                SoundEngine.PlaySound(Yharon.ShortRoarSound, player.position);

//                // 切换为抛射物弹幕，并设置使用时间和动画为60帧
//                Item.shoot = ModContent.ProjectileType<FinishingTouchPROJ>();
//                Item.shootSpeed = 30f;
//                Item.useTime = Item.useAnimation = 60;
//            }
//            else
//            {
//                // 限制左键只能生成一个FinishingTouchDASH弹幕
//                if (player.ownedProjectileCounts[Item.shoot] > 0)
//                    return false;

//                // 播放龙吼音效
//                SoundEngine.PlaySound(Yharon.ShortRoarSound, player.position);

//                // 恢复为默认的左键蓄力冲刺弹幕，并设置使用时间和动画为0帧
//                Item.shoot = ModContent.ProjectileType<FinishingTouchDASH>();
//                Item.shootSpeed = 0f;
//                Item.useTime = Item.useAnimation = 240;
//            }
//            return base.CanUseItem(player);
//        }



//        public override void AddRecipes()
//        {
//            Recipe recipe = CreateRecipe();
//            recipe.AddIngredient<GildedProboscisJav>();
//            recipe.AddIngredient<YharonSoulFragment>(8);
//            recipe.AddTile<CosmicAnvil>();
//            recipe.Register();
//        }





//    }
//}
