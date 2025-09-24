using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    internal class ConceptTimeAcceleration : ModSystem
    {
        // 计时器（单位：tick，60 tick = 1 秒）
        private int accelTimer;

        public override void PostUpdateTime()
        {
            bool hasConceptProj = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active)
                    continue;

                if (proj.type == ModContent.ProjectileType<SunsetCConceptRight>() ||
                    proj.type == ModContent.ProjectileType<SunsetCConceptLeftListener>())
                {
                    hasConceptProj = true;
                    break;
                }
            }

            if (hasConceptProj)
            {
                // 每帧计时，最长 12 秒（720 tick）
                if (accelTimer < 720)
                    accelTimer++;

                // 从 1 → 60 线性提升
                float t = accelTimer / 720f; // 0 → 1
                int dayRate = (int)MathHelper.Lerp(1f, 60f, t);

                Main.dayRate = dayRate;
                Main.fastForwardTimeToDawn = true;
                Main.fastForwardTimeToDusk = true;
            }
            else
            {
                // 重置
                accelTimer = 0;
                Main.dayRate = 1;
                Main.fastForwardTimeToDawn = false;
                Main.fastForwardTimeToDusk = false;
            }
        }
    }
}
