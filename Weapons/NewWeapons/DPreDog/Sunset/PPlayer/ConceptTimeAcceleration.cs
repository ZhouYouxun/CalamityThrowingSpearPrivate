using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.System
{
    public class ConceptTimeAcceleration : ModSystem
    {
        // ============================================================
        // ★ 右键蓄能系统字段（左键不使用）
        // ============================================================
        private int accelTimer = 0;                // 右键蓄能计时器
        private const int chargeDuration = 3060;   // 51秒
        private bool triggeredFinal = false;       // 是否已经触发第50秒日食

        // ============================================================
        // ★ 左键加速系统字段（独立，不和右键冲突）
        // ============================================================
        private int leftTimeBoost = 0;             // 左键平稳加速用
        private const int leftBoostRate = 6;       // 左键 dayRate 固定值（可调整）

        public override void PostUpdateTime()
        {
            bool rightHoldExists = false;
            bool leftHoldExists = false;

            // ---------------------------------------------------------
            // ① 检测两类弹幕存在情况
            // ---------------------------------------------------------
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active)
                    continue;

                if (proj.type == ModContent.ProjectileType<SunsetCConceptRight>())
                    rightHoldExists = true;

                if (proj.type == ModContent.ProjectileType<SunsetCConceptLeftListener>())
                    leftHoldExists = true;
            }

            // ============================================================
            // ★ 情况 A — 没有任何监听弹幕 → 全部重置
            // ============================================================
            if (!rightHoldExists && !leftHoldExists)
            {
                ResetAll();
                return;
            }

            // ============================================================
            // ★ 情况 B — 右键弹幕存在 → 使用“超蓄能 + 日食版”
            // ============================================================
            if (rightHoldExists)
            {
                RunRightClickAcceleration();
                return;     // 右键优先，不执行左键逻辑
            }

            // ============================================================
            // ★ 情况 C — 只有左键存在 → 使用普通版加速（原始逻辑）
            // ============================================================
            if (leftHoldExists)
            {
                RunLeftClickBasicAcceleration();
                return;
            }
        }

        // ======================================================================
        // ★ 右键：50 秒蓄能加速 + 终极日食触发
        // ======================================================================
        private void RunRightClickAcceleration()
        {
            // 若之前被左键加速干扰，立即清空左键状态
            leftTimeBoost = 0;

            if (!triggeredFinal)
            {
                accelTimer++;

                // 速度从 1 → 40（平滑过渡）
                float t = accelTimer / (float)chargeDuration;
                t = MathHelper.Clamp(t, 0f, 1f);
                int maxRate = 40;

                Main.dayRate = (int)MathHelper.Lerp(1, maxRate, t);

                Main.fastForwardTimeToDawn = true;
                Main.fastForwardTimeToDusk = true;
            }

            // 达到 50 秒 → 触发
            if (!triggeredFinal && accelTimer >= chargeDuration)
            {
                triggeredFinal = true;

                // 播放你的最终音效（你说会换路径）
                SoundEngine.PlaySound(
                    new SoundStyle("CalamityThrowingSpear/Sound/380mmExploded"),
                    Main.LocalPlayer.Center);

                // 停止时间加速
                Main.dayRate = 1;
                Main.fastForwardTimeToDawn = false;
                Main.fastForwardTimeToDusk = false;

                // 触发日食
                Main.eclipse = true;
                Main.dayTime = true;
                Main.time = 0;

                // 屏幕震动效果（以玩家为中心，强度 95）
                float shakePower = 95f;
                float distanceFactor = 1f; // 以玩家为中心，不需要距离衰减
                Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                    MathF.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            }
        }

        // ======================================================================
        // ★ 左键：原版“平稳时间加速”（固定 dayRate，不蓄能）
        // ======================================================================
        private void RunLeftClickBasicAcceleration()
        {
            // 若右键刚触发过终极事件，左键不接管加速
            if (triggeredFinal)
                return;

            leftTimeBoost++;

            // 使用一个原始平滑加速效果（不蓄能、不触发事件）
            // 这里 dayRate 固定到 6（你可以调）
            Main.dayRate = leftBoostRate;

            Main.fastForwardTimeToDawn = true;
            Main.fastForwardTimeToDusk = true;
        }

        // ======================================================================
        // ★ 全部重置（两种弹幕都消失时触发）
        // ======================================================================
        private void ResetAll()
        {
            accelTimer = 0;
            triggeredFinal = false;
            leftTimeBoost = 0;

            Main.dayRate = 1;
            Main.fastForwardTimeToDawn = false;
            Main.fastForwardTimeToDusk = false;
        }
    }
}
