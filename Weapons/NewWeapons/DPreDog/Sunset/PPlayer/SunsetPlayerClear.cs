using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.Particles;
using System; // ← 新增：粒子类型/生成器在这

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    public class SunsetPlayer : ModPlayer
    {
        // 概念一致性

        // 存放所有违规武器的ID
        private static readonly HashSet<int> BannedWeapons = new HashSet<int>
        {
            ModContent.ItemType<Aestheticus>(), // 美学魔杖
            ModContent.ItemType<LunicEye>(), //星光之眼
            ModContent.ItemType<EyeofMagnus>(), // 马克努斯之眼
            //ModContent.ItemType<YanmeisKnife>(), // 雅姆的刀
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

        // 清除特效冷却（避免“每帧清除=每帧爆闪”）
        private int cleanseVfxCooldown = 0;

        public override void PostUpdate()
        {
            Player player = Main.LocalPlayer;

            // 冷却递减
            if (cleanseVfxCooldown > 0)
                cleanseVfxCooldown--;

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
                    // 如果没有，则奖励玩家：解除所有持有的负面 Buff（但白名单例外）
                    bool clearedAny = ClearNegativeBuffs(player);
                    if (clearedAny)
                        TryTriggerCleanseEffect(player);

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

        // **清除所有 Debuff（保留白名单）**
        // 返回：是否真的清掉了任何一个 debuff（用于决定要不要播放一次性特效）
        private bool ClearNegativeBuffs(Player player)
        {
            bool clearedAny = false;

            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int buffType = player.buffType[i];

                // 确保 Buff 存在，并且不是白名单 Buff，并且是负面效果（Debuff）
                if (buffType > 0 && !BuffWhitelist.Contains(buffType) && Main.debuff[buffType])
                {
                    player.DelBuff(i);
                    clearedAny = true;
                }
            }

            return clearedAny;
        }

        // 一次性净化特效入口（带冷却）
        private void TryTriggerCleanseEffect(Player player)
        {
            if (Main.dedServ)
                return;

            // 你想要“冲击性”但不能刷屏：给个短冷却（约 0.5 秒）
            if (cleanseVfxCooldown > 0)
                return;

            cleanseVfxCooldown = 30;
            //TriggerCleanseEffect(player);
        }

        // **触发净化粒子特效** [仅一次]：向上冲击 + 几何结构（两种粒子拼接）
        private void TriggerCleanseEffect(Player player)
        {
            Vector2 center = player.Center + new Vector2(0f, -10f); // 稍微抬高，让整体“往上”
            int sides = 8; // 八边形：更“EXO/秩序感”
            float baseRadius = 54f;
            float topRadius = 34f;
            float height = 68f;

            // 颜色基调：外缘更亮、更“镁粉燃烧”的白橙；节点偏红更锋利
            Color exoCore = Color.Lerp(Color.Orange, Color.White, 0.55f);
            Color exoEdge = Color.Lerp(Color.OrangeRed, Color.White, 0.35f);
            Color orbHot = Color.Lerp(Color.Red, Color.White, 0.15f);

            // 预计算顶点（底环 + 上环），构成立体“上冲棱柱”
            Vector2[] baseV = new Vector2[sides];
            Vector2[] topV = new Vector2[sides];

            for (int i = 0; i < sides; i++)
            {
                float ang = MathHelper.TwoPi * i / sides;
                Vector2 dir = Vector2.UnitX.RotatedBy(ang);

                baseV[i] = center + dir * baseRadius;
                topV[i] = center + dir * topRadius - Vector2.UnitY * height;

                // 节点辉光球：底部节点（更红）+ 顶部节点（更白更亮）
                SpawnGlowOrb(baseV[i], -Vector2.UnitY * 0.5f + dir * 0.25f, 8, 0.85f, orbHot);
                SpawnGlowOrb(topV[i], -Vector2.UnitY * 0.9f, 7, 0.75f, exoCore);

                // 顶点“向上拔起”的 Squish 光（像被拉伸的光刃）
                Vector2 tangent = dir.RotatedBy(MathHelper.Pi / 2f);
                float sign = (i % 2 == 0) ? 1f : -1f;
                Vector2 spikeVel = -Vector2.UnitY * 1.35f + tangent * 0.35f * sign + dir * 0.15f;

                SpawnExoSquishy(baseV[i], spikeVel, 0.32f, exoCore, 25, squishStrength: 1f, maxSquish: 3f);
            }

            // 画底环、顶环、竖边（全是“线段拼接”），并统一朝上冲
            for (int i = 0; i < sides; i++)
            {
                int j = (i + 1) % sides;

                // 每条边给一个“有秩序”的速度基底：向上 + 少量切向旋
                Vector2 dirI = (baseV[i] - center);
                if (dirI != Vector2.Zero)
                    dirI.Normalize();

                Vector2 tangentI = dirI.RotatedBy(MathHelper.Pi / 2f);
                float alt = (i % 2 == 0) ? 1f : -1f;

                Vector2 upSwirl = -Vector2.UnitY * (1.05f + 0.10f * i) + tangentI * 0.22f * alt;

                // 底环边
                SpawnExoLine(baseV[i], baseV[j], 7, upSwirl, exoEdge, 0.20f, 0.28f, 18, 24);

                // 顶环边（更亮一点）
                SpawnExoLine(topV[i], topV[j], 6, upSwirl * 1.05f, exoCore, 0.18f, 0.26f, 16, 22);

                // 竖边（棱柱上冲感的关键）
                SpawnExoLine(baseV[i], topV[i], 8, -Vector2.UnitY * 1.35f + tangentI * 0.12f * alt, exoCore, 0.18f, 0.30f, 18, 25);

                // 斜向“切割线”（让几何更复杂、更冲击）
                int k = (i + 2) % sides;
                SpawnExoLine(baseV[i], topV[k], 6, -Vector2.UnitY * 1.25f + tangentI * 0.18f * alt, exoEdge, 0.16f, 0.24f, 16, 22);
            }

            // 中心“冲击柱”：少量但很亮（收束视觉焦点）
            Vector2 topCenter = center - Vector2.UnitY * (height + 18f);
            for (int s = 0; s < 5; s++)
            {
                float t = s / 4f;
                Vector2 pos = Vector2.Lerp(center, topCenter, t);
                float scale = MathHelper.Lerp(0.26f, 0.34f, 1f - t);
                SpawnExoSquishy(pos, -Vector2.UnitY * (1.6f - 0.15f * s), scale, exoCore, 22, squishStrength: 1f, maxSquish: 3f);
                SpawnGlowOrb(pos, Vector2.Zero, 5, 0.75f - 0.08f * s, exoCore);
            }
        }

        // 线段拼接：用 SquishyLightParticle 画“几何线”，统一向上冲（不靠随机）
        private static void SpawnExoLine(Vector2 start, Vector2 end, int steps, Vector2 velocityBase, Color color, float scaleA, float scaleB, int lifeA, int lifeB)
        {
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;

                // 中段更亮更粗一点（有冲击中心）
                float hump = (float)Math.Sin(t * MathHelper.Pi);

                Vector2 pos = Vector2.Lerp(start, end, t);
                Vector2 vel = velocityBase * (0.85f + 0.55f * hump);

                float scale = MathHelper.Lerp(scaleA, scaleB, hump);
                int life = (int)MathHelper.Lerp(lifeA, lifeB, hump);

                float squish = 0.9f + 0.35f * hump;
                float maxSquish = 2.6f + 0.6f * hump;

                SpawnExoSquishy(pos, vel, scale, color, life, squish, maxSquish);

                // 节点补一颗小辉光球，让线条“像电路一样有点”
                if (i == 0 || i == steps || i == steps / 2)
                    SpawnGlowOrb(pos, Vector2.Zero, 5, 0.65f, color);
            }
        }

        // 9.EXO之光（Squish 粒子）
        private static void SpawnExoSquishy(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime, float squishStrength, float maxSquish)
        {
            SquishyLightParticle exoEnergy = new(
                position,
                velocity,
                scale,
                color,
                lifetime,
                opacity: 1f,
                squishStrenght: squishStrength,
                maxSquish: maxSquish,
                hueShift: 0f
            );
            GeneralParticleHandler.SpawnParticle(exoEnergy);
        }

        // 10.辉光球（GlowOrbParticle）
        private static void SpawnGlowOrb(Vector2 position, Vector2 velocity, int lifetime, float scale, Color color)
        {
            GlowOrbParticle orb = new GlowOrbParticle(
                position,
                velocity,
                false,
                lifetime,
                scale,
                color,
                true,
                false,
                true
            );
            GeneralParticleHandler.SpawnParticle(orb);
        }

        // **触发粒子特效** [仅一次]（你原来的惩罚特效先不动）
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
