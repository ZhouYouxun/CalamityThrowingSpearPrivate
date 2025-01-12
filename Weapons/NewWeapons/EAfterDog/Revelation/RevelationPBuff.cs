//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria;
//using Terraria.ModLoader;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
//{
//    public class RevelationPBuff : ModBuff
//    {
//        public override void SetStaticDefaults()
//        {
//            Main.debuff[Type] = false; // 设置为增益Buff
//            Main.buffNoSave[Type] = true; // 不保存退出游戏时的状态
//        }

//        public override void Update(Player player, ref int buffIndex)
//        {
//            if (player.TryGetModPlayer<RevelationPlayer>(out RevelationPlayer modPlayer))
//            {
//                modPlayer.RevelationPBuffActive = true; // 启用开关
//                modPlayer.RevelationStackCount++; // 叠层数增加
//            }
//        }
//    }
//}
