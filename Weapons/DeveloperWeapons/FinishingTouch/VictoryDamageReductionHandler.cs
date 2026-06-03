//using CalamityMod;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria;
//using Terraria.ModLoader;

//namespace CalamityThrowingSpear.Common
//{
//    public static class VictoryDamageReductionHandler
//    {
//        public static void ApplyDamageReductionToNPC(NPC npc)
//        {
//            if (npc.Calamity() != null)
//            {
//                // 降低 40% 的伤害减免
//                npc.Calamity().damageReductionMultiplier *= 0.6f;
//            }
//        }

//        public static void ApplyDamageReductionToPlayer(Player player)
//        {
//            // 减少 35% 攻击伤害
//            player.GetDamage(DamageClass.Generic) -= 0.35f;

//            // 减少 40 点防御
//            player.statDefense -= 40;
//        }
//    }
//}