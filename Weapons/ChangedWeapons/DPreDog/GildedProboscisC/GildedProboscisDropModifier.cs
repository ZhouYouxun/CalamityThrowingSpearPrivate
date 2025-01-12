//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria;
//using Terraria.GameContent.ItemDropRules;
//using Terraria.ModLoader;
//using CalamityMod.Items.TreasureBags; // 引用 CalamityMod 的宝藏袋
//using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;
//using CalamityMod;
//using CalamityMod.Items.Weapons.Melee;
//using CalamityMod.Items.Weapons.Ranged;
//using CalamityMod.Items.Weapons.Magic; // 假设 GildedProboscisJav 在此命名空间

//namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC
//{
//    public class GildedProboscisDropModifier : ModSystem
//    {
//        public override void PostAddRecipes()
//        {
//            // 注册一个钩子，用于修改 DragonfollyBag 的掉落规则
//            var bagType = ModContent.ItemType<DragonfollyBag>(); // 获取 DragonfollyBag 的类型

//            // 检查是否存在该物品
//            if (ItemLoader.GetItem(bagType) != null)
//            {
//                // 获取对应的 ItemDropRule 对象
//                ItemDropDatabase itemDropDatabase = Main.ItemDropDatabase;
//                itemDropDatabase.RegisterToItem(bagType, GetCustomDropRule());
//            }
//        }

//        private IItemDropRule GetCustomDropRule()
//        {
//            // 获取原有的掉落规则
//            var originalRule = DropHelper.CalamityStyle(DropHelper.BagWeaponDropRateFraction, new int[]
//            {
//                ModContent.ItemType<GildedProboscis>(), // 原有武器
//                ModContent.ItemType<GoldenEagle>(),
//                ModContent.ItemType<RougeSlash>()
//            });

//            // 添加新的掉落物 GildedProboscisJav
//            return new OneFromRulesRule(1, originalRule, new[]
//            {
//                ItemDropRule.Common(ModContent.ItemType<GildedProboscisJav>(), 1) // 添加新的掉落物
//            });
//        }
//    }
//}
