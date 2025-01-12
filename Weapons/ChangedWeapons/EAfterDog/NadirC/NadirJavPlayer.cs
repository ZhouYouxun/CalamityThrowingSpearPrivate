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
                // 获取玩家当前速度与最大速度的比例
                float speedFactor = Player.velocity.Length() / Player.maxRunSpeed;
                // 将比例限制在 0 到 1 之间
                speedFactor = MathHelper.Clamp(speedFactor, 0f, 1f);

                // 根据速度比例计算近战伤害增幅，速度越慢增幅越大（最高 35%）
                float damageBoost = (1f - speedFactor) * 0.35f;

                // 应用增幅
                Player.GetDamage(DamageClass.Melee) += damageBoost;
            }
        }
    }
}
