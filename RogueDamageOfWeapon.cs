using Terraria;
using Terraria.ModLoader;
using CalamityThrowingSpear.Global;
using Terraria.ID;
using CalamityMod;

namespace CalamityThrowingSpear
{
    public class RogueDamageOfWeapon : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (ModContent.GetInstance<CTSConfigs>().EnableRogue)
            {
                // 自动检测：本模组 + damage>0 + maxStack==1 + 非配饰 + DamageType == Melee
                if (item.ModItem != null &&
                    item.ModItem.Mod == ModContent.GetInstance<CalamityThrowingSpearMod>() &&
                    item.damage > 0 &&
                    item.maxStack == 1 &&
                    !item.accessory &&
                    item.DamageType == DamageClass.Melee)
                {
                    item.DamageType = ModContent.GetInstance<RogueDamageClass>();
                }
            }
        }
    }

    public class RogueDamageOfProjectile : GlobalProjectile
    {
        public override void SetDefaults(Projectile projectile)
        {
            if (ModContent.GetInstance<CTSConfigs>().EnableRogue)
            {
                // 自动检测：本模组 + DamageType == Melee
                if (projectile.ModProjectile != null &&
                    projectile.ModProjectile.Mod == ModContent.GetInstance<CalamityThrowingSpearMod>() &&
                    projectile.DamageType == DamageClass.Melee)
                {
                    projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
                }
            }
        }
    }
}
