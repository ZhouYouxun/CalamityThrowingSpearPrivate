using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    public class SunsetPlayerDR : ModPlayer
    {
        private float bonusDR = 0f; // 额外的伤害减免（DR）
        private int bonusDefense = 0; // 额外的防御力
        private float bonusLifeRegen = 0f; // 额外生命恢复
        private int extraInvincibilityFrames = 0; // 额外无敌帧
        private float finalDamageReduction = 1f; // 最终伤害倍率（默认1.0，不影响）

        public override void ResetEffects()
        {
            // 重置增益效果，每帧更新
            bonusDR = 0f;
            bonusDefense = 0;
            bonusLifeRegen = 0f;
            extraInvincibilityFrames = 0;
            finalDamageReduction = 1f;

            // 只有当玩家手持 Sunset 时才给予奖励
            if (Player.HeldItem.type == ModContent.ItemType<Sunset>())
            {
                ApplySunsetBonuses();
            }
        }

        private void ApplySunsetBonuses()
        {
            int defense = Player.statDefense; // 获取当前防御值

            // 根据防御力给予不同等级的奖励
            if (defense > 250)
            {
                bonusDR = 0.8f;
                extraInvincibilityFrames = 120; // 2.0s无敌帧
                finalDamageReduction = 0.15f; // 最终受到伤害 15%
                bonusDefense = 100; // +100防御力
                bonusLifeRegen = 6f; // 生命恢复+6
            }
            else if (defense > 200)
            {
                bonusDR = 0.5f;
                extraInvincibilityFrames = 60; // 1.0s无敌帧
                finalDamageReduction = 0.5f; // 最终受到伤害 -50%
            }
            else if (defense > 150)
            {
                bonusDR = 0.3f;
                extraInvincibilityFrames = 30; // 0.5s无敌帧
            }
            else if (defense > 100)
            {
                bonusDR = 0.1f;
                extraInvincibilityFrames = 6; // 0.1s无敌帧
            }

            // 将奖励应用到玩家属性
            Player.endurance += bonusDR;
            Player.statDefense += bonusDefense;
            Player.lifeRegen += (int)bonusLifeRegen;
        }

        // **玩家被投射物击中时**
        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            base.OnHitByProjectile(proj, hurtInfo);
            ApplyExtraInvincibility(ref hurtInfo);
        }

        // **玩家被NPC击中时**
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            base.OnHitByNPC(npc, hurtInfo);
            ApplyExtraInvincibility(ref hurtInfo);
        }

        // **自定义无敌帧和最终伤害调整**
        private void ApplyExtraInvincibility(ref Player.HurtInfo hurtInfo)
        {
            // **应用额外无敌时间**
            Player.immune = true; // 启用无敌
            Player.immuneNoBlink = true; // 处于无敌状态时玩家不会闪烁
            Player.immuneTime += extraInvincibilityFrames; // 增加无敌帧

            // **减少最终伤害**
            hurtInfo.Damage = (int)(hurtInfo.Damage * finalDamageReduction);
        }
    }
}
