using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    internal class SunsetASunsetGProj : GlobalProjectile
    {
        public override void AI(Projectile projectile)
        {
            // 检测是否存在带有 SunsetASunsetedEDebuff 的敌人
            bool hasDebuffedNPC = false;

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.HasBuff(ModContent.BuffType<SunsetASunsetEDebuff>()))
                {
                    hasDebuffedNPC = true;
                    break;
                }
            }

            // 如果存在 Debuff，则降低敌人弹幕速度
            if (hasDebuffedNPC && projectile.hostile)
            {
                projectile.velocity *= 0.95f; // 敌人弹幕速度减少 5%
            }
        }
    }
}