//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.ShadowJav
//{
//    public class XiaoZhiTiaoS : ModItem
//    {
//        public override void SetDefaults()
//        {
//            Item.width = 64;
//            Item.height = 64;
//            Item.accessory = true;
//            // 困难模式前：Orange，价值15金
//            Item.rare = ItemRarityID.Orange;
//            Item.value = Item.buyPrice(0, 15, 0, 0);
//            Item.value = Item.sellPrice(0, 15, 0, 0);

//        }

//        public override void UpdateAccessory(Player player, bool hideVisual)
//        {
//            // 综合属性提升
//            player.GetDamage(DamageClass.Generic) += 0.2f; // 所有职业的攻击力
//            player.GetAttackSpeed(DamageClass.Generic) += 0.2f; // 所有职业的攻击速度
//        }


//    }
//}