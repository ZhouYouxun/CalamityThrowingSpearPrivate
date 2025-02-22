using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget
{
    internal class SunsetBForgetPlayer : ModPlayer
    {
        public bool hasSunsetBuff = false;

        public override void ResetEffects()
        {
            hasSunsetBuff = false; // 每帧重置 Buff 状态
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (hasSunsetBuff)
            {
                hurtInfo.Damage = (int)(hurtInfo.Damage * 0.8f); // 受到的伤害减少 20%
            }
        }
    }
}