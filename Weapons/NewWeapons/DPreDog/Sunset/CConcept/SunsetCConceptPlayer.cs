using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptPlayer : ModPlayer
    {
        public bool hasSunsetBuff = false;

        public override void ResetEffects()
        {
            hasSunsetBuff = false; // 每帧重置 Buff 状态
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (hasSunsetBuff && Main.rand.NextFloat() < 0.05f) // 5% 概率触发
            {
                Player.statLife += 200; // 恢复 200 生命
                Player.HealEffect(200); // 显示治疗效果
            }
        }
    }
}