using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00
{
    internal class TEM00Player : ModPlayer
    {
        public override void ResetEffects()
        {
            bool hasTEM00Left = false;

            // 检查场上是否存在 TEM00Left 弹幕
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<TEM00Left>())
                {
                    hasTEM00Left = true;
                    break;
                }
            }

            // 如果存在，则所有玩家 +50 防御
            if (hasTEM00Left)
            {
                Player.statDefense += 50;
            }
        }
    }
}
