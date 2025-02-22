using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Items.Weapons.Typeless;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    public class SunsetPlayer : ModPlayer
    {
        // 存放所有违规武器的ID
        private static readonly HashSet<int> BannedWeapons = new HashSet<int>
        {
            ModContent.ItemType<Aestheticus>(), // 美学魔杖
            ModContent.ItemType<LunicEye>(), //星光之眼
            ModContent.ItemType<EyeofMagnus>(), // 马克努斯之眼
            ModContent.ItemType<YanmeisKnife>(), // 雅姆的刀
            ModContent.ItemType<RelicOfDeliverance>() // 遗迹长枪
        };

        // 存放白名单 Buff 的容器
        private static readonly HashSet<int> BuffWhitelist = new HashSet<int>
        {
            ModContent.BuffType<AdrenalineMode>(), // 肾上腺素模式
            BuffID.PotionSickness, // 药水病
            ModContent.BuffType<RageMode>() // 狂怒模式
        };

        // 视觉特效触发标记
        private bool hasTriggeredPunishment = false;

        public override void PostUpdate()
        {
            Player player = Main.LocalPlayer;

            // 确保玩家手持 Sunset
            if (player.HeldItem.type == ModContent.ItemType<Sunset>())
            {
                // 检测玩家背包里是否有违规武器
                bool hasBannedWeapon = false;
                foreach (Item item in player.inventory)
                {
                    if (BannedWeapons.Contains(item.type))
                    {
                        hasBannedWeapon = true;
                        break;
                    }
                }

                if (hasBannedWeapon)
                {
                    // 给予惩罚效果（每帧刷新，确保持续）
                    player.AddBuff(ModContent.BuffType<Dragonfire>(), 300); // 5秒
                    player.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 300);

                    // 只在最初触发时播放粒子效果
                    if (!hasTriggeredPunishment)
                    {
                        TriggerPunishmentEffect(player);
                        hasTriggeredPunishment = true;
                    }
                }
                else
                {
                    // 如果没有，则奖励玩家：解除所有持有的负面 Buff（但 AdrenalineMode 例外）
                    ClearNegativeBuffs(player);

                    // 由于不再处于惩罚状态，重置粒子触发标记
                    hasTriggeredPunishment = false;
                }
            }
            else
            {
                // 玩家未持有 Sunset 时，重置粒子触发标记
                hasTriggeredPunishment = false;
            }
        }

        // **清除所有 Debuff，除了 AdrenalineMode**
        private void ClearNegativeBuffs(Player player)
        {
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int buffType = player.buffType[i];

                // 确保 Buff 存在，并且不是白名单 Buff，并且是负面效果（Debuff）
                if (buffType > 0 && !BuffWhitelist.Contains(buffType) && Main.debuff[buffType])
                {
                    player.DelBuff(i);
                }
            }
        }

        // **触发粒子特效** [仅一次]
        private void TriggerPunishmentEffect(Player player)
        {
            for (int i = 0; i < 50; i++)
            {
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                Dust dust = Dust.NewDustDirect(player.Center, 10, 10, DustID.DesertTorch, velocity.X, velocity.Y);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
            }
        }
    }
}
