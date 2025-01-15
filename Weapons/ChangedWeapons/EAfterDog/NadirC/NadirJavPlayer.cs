using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.NadirC;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.NadirC
{
    public class NadirJavPlayer : ModPlayer
    {
        public override void PostUpdateEquips()
        {
            // 检查玩家手上是否持有 NadirJav 武器
            if (Player.HeldItem.type == ModContent.ItemType<NadirJav>())
            {
                // 将玩家当前速度转换为mph
                float currentSpeedMph = Player.velocity.Length() * 15f; // 假设游戏内速度乘以15转为mph

                // 根据速度计算近战伤害增幅
                float damageBoost = 0f;
                if (currentSpeedMph <= 70f)
                {
                    damageBoost = MathHelper.Clamp(0.35f - (currentSpeedMph / 2f * 0.01f), 0f, 0.35f);
                }

                // 应用增幅
                Player.GetDamage(DamageClass.Melee) += damageBoost;
            }
        }

        


    }
}
