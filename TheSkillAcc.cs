//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.ModLoader;
//using Terraria;

//namespace CalamityThrowingSpear
//{
//    internal class TheSkillAcc : ModItem
//    {
//        public override void SetStaticDefaults()
//        {

//        }

//        public override void UpdateAccessory(Player player, bool hideVisual)
//        {
//            if (player.GetModPlayer<TheSkill>() is TheSkill skill)
//            {
//                skill.skillEnabled = true; // 启用开关
//            }
//        }

//        public override void SetDefaults()
//        {
//            Item.width = 20;
//            Item.height = 20;
//            Item.accessory = true;
//            Item.rare = 2;
//            Item.value = Item.sellPrice(silver: 50);
//        }
//    }
//}