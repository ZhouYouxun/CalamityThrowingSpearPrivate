using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav
{
    public class SawBladeForkHornPlayer : ModPlayer
    {
        public int StackCount;

        public override void ResetEffects()
        {
            // 检查是否装备了SawBladeForkHornJav，如果没有，则移除Buff
            if (Player.HeldItem.type != ModContent.ItemType<SawBladeForkHornJav>())
            {
                StackCount = 0;
                Player.ClearBuff(ModContent.BuffType<SawBladeForkHornPBuff>());
            }
        }

        public void IncreaseStackCount()
        {
            StackCount++; // 增加堆叠层数
            Player.AddBuff(ModContent.BuffType<SawBladeForkHornPBuff>(), 600); // 每次击中后刷新10秒的buff时间
        }
    }

}
