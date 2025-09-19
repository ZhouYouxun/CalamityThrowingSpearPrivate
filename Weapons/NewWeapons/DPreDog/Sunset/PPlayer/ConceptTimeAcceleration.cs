using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept;
using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    internal class ConceptTimeAcceleration : ModSystem
    {
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
                Main.dayRate = 60;                 // 加速倍率
                Main.fastForwardTimeToDawn = true; // 告诉游戏“时间正在加速”
                Main.fastForwardTimeToDusk = true; // 不管白天黑夜都加速
            }
            else
            {
                Main.dayRate = 1;
                Main.fastForwardTimeToDawn = false;
                Main.fastForwardTimeToDusk = false;
            }

        }





    }
}
