using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget
{
    internal class SunsetBForgetPlayer : ModPlayer
    {
        // Buff 每帧 Update 只负责把这个开关置 true（轻量、稳定）
        public bool hasSunsetBuff;

        // 自己的计时器字段（不依赖任何 localAI 之类的计数器）
        private int eternalLoveTime;   // 总时间：驱动所有“数学秩序”
        private int outerIndex;        // 外环刻度索引：保证数量稳定且均匀
        private int spiralIndex;       // 花粉螺旋索引：叶序角填充
        private int fadeTimer;         // 视觉淡出缓冲：Buff 断开时不瞬间消失


        public override void ResetEffects()
        {
            hasSunsetBuff = false; // 每帧重置 Buff 状态
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (hasSunsetBuff)
            {
                hurtInfo.Damage = (int)(hurtInfo.Damage * 0.8f); // 受到的伤害减少 20%
            }
        }


        public override void PostUpdate()
        {
            // 只在本地玩家生成粒子：省性能，也避免多人环境重复生成
            if (Player.whoAmI != Main.myPlayer)
                return;

            // Buff 持续则维持淡出缓冲，否则逐帧衰减
            if (hasSunsetBuff)
                fadeTimer = 60; // 约 1 秒缓冲
            else if (fadeTimer > 0)
                fadeTimer--;

            if (fadeTimer <= 0)
                return;

            eternalLoveTime++;

            // 强度：用于淡出，同时也让“无序扰动”变得可控
            float strength = fadeTimer / 60f; // 0~1
            SpawnEternalLoveFx(strength);
        }

        private void SpawnEternalLoveFx(float strength)
        {
            Vector2 center = Player.Center + new Vector2(0f, Player.gfxOffY);

            // =========================
            // 色彩主题：同源蓝紫梦境科技
            // =========================
            Color cyan = new Color(95, 195, 255);
            Color purple = new Color(175, 120, 255);
            Color white = new Color(255, 255, 255);

            // =========================
            // 数学节奏：有序（呼吸）+ 无序（受控扰动）
            // =========================
            float t = eternalLoveTime * 0.045f; // 基础时间轴（越小越“稳定”）
            float breathe = (float)(Math.Sin(t) * 0.5 + 0.5);              // 0~1：主呼吸
            float breathe2 = (float)(Math.Sin(t * 0.5f + 1.7f) * 0.5 + 0.5); // 0~1：副呼吸（错相）

            // ============================================================
            // 0）核心柔光（低频稳定）：让玩家“被守护”而不是“被闪瞎”
            // ============================================================
            if (eternalLoveTime % 8 == 0)
            {
                float coreScale = MathHelper.Lerp(0.45f, 0.75f, breathe) * strength;
                Color coreColor = Color.Lerp(cyan, purple, breathe2) * (0.75f * strength);

                var core = new GenericBloom(center, Vector2.Zero, coreColor, coreScale, 22);
                GeneralParticleHandler.SpawnParticle(core);
            }

            // ============================================================
            // 1）外环：誓约刻度（强秩序）—— 固定数量、匀速循环、几何稳定
            // ============================================================
            if (eternalLoveTime % 12 == 0)
            {
                // 外环半径：约 (4.5×16) / (2.8×16)，椭圆 + 缓慢进动
                float a = 72f;
                float b = 45f;
                float tilt = (float)Math.Sin(t * 0.22f) * 0.25f; // 椭圆整体轻微转轴（不随机）

                int count = 12;
                int idx = outerIndex++ % count;

                float ang = t * 0.75f + idx * MathHelper.TwoPi / count;

                Vector2 onEllipse = Rotate(new Vector2((float)Math.Cos(ang) * a, (float)Math.Sin(ang) * b), tilt);

                // 切线方向（有序）：刻度沿环“滑动”
                Vector2 tangent = Rotate(new Vector2(-(float)Math.Sin(ang), (float)Math.Cos(ang)), tilt);
                Vector2 drift = SafeNormalize(tangent, Vector2.UnitY) * (0.7f + 0.6f * breathe) * strength;

                // 受控无序：伪噪声（确定性），让刻度“活着”但不乱飞
                Vector2 microChaos = new Vector2(
                    (float)Math.Sin(idx * 1.37f + t * 2.1f),
                    (float)Math.Cos(idx * 1.11f - t * 1.9f)
                ) * (0.55f * strength);

                Vector2 pos = center + onEllipse + microChaos;

                Color tickColor = Color.Lerp(cyan, white, 0.25f + 0.35f * breathe) * (0.85f * strength);
                float tickScale = MathHelper.Lerp(0.7f, 1.0f, breathe) * strength;

                var tick = new SquareParticle(pos, drift, false, 22, tickScale, tickColor);
                tick.Rotation = ToRotation(tangent) + 0.15f * (float)Math.Sin(t * 1.3f + idx);
                GeneralParticleHandler.SpawnParticle(tick);

                // 每 24 帧补一个细火花：强调“刻度在流动”，但不靠堆量
                if (eternalLoveTime % 24 == 0)
                {
                    var spark = new SparkParticle(
                        pos,
                        drift * 0.7f,
                        false,
                        18,
                        0.9f * strength,
                        Color.Lerp(cyan, purple, 0.6f) * (0.9f * strength)
                    );
                    spark.Rotation = ToRotation(tangent);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            // ============================================================
            // 2）中层：花粉螺旋（自然几何）—— 叶序角 137.5° + 受控风扰动
            // ============================================================
            if (eternalLoveTime % 6 == 0)
            {
                // Golden Angle（叶序角）：自然界最经典的“均匀填充”角度
                float golden = 137.5f * (MathHelper.Pi / 180f);

                int n = spiralIndex++;

                // sqrt(n) 半径：花盘扩张；用取模限制范围，形成“持续、循环的生长”
                float r = (float)Math.Sqrt(n % 160) * 5.0f; // 0~约 63
                r = MathHelper.Clamp(r, 8f, 62f);

                float ang = n * golden + t * 0.45f; // 螺旋整体缓慢旋转（永恒感）
                Vector2 spiral = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * r;

                // “风”：确定性的扰动（无序感），但强度受 strength 控制，不会炸
                float wind = (float)Math.Sin(t * 1.7f + n * 0.13f);
                float wind2 = (float)Math.Cos(t * 1.3f - n * 0.09f);
                Vector2 pollenChaos = new Vector2(wind, wind2) * (2.2f * strength);

                Vector2 pos = center + spiral + pollenChaos;

                // 速度：以“缓慢漂浮”为主（大自然），同时继承螺旋方向（秩序）
                Vector2 baseDir = SafeNormalize(spiral, Vector2.UnitY);
                Vector2 vel = (baseDir * 0.35f + new Vector2(wind2, -wind) * 0.12f) * strength;

                Color pollenColor = Color.Lerp(purple, cyan, 0.4f + 0.35f * (float)Math.Sin(n * 0.21f)) * (0.75f * strength);
                float pollenScale = MathHelper.Lerp(0.85f, 1.25f, breathe2) * strength;

                var pollen = new PointParticle(pos, vel, false, 18, pollenScale, pollenColor);
                GeneralParticleHandler.SpawnParticle(pollen);

                // 每 12 帧补一丝“露雾”：自然意象的载体（同源色系，不跳风格）
                if (eternalLoveTime % 12 == 0)
                {
                    var mist = new WaterFlavoredParticle(
                        pos,
                        vel * 0.8f,
                        false,
                        22,
                        0.85f * strength,
                        Color.Lerp(cyan, purple, 0.5f) * (0.55f * strength)
                    );
                    GeneralParticleHandler.SpawnParticle(mist);
                }
            }

            // ============================================================
            // 3）内核：心跳脉冲（情感节奏）—— 低频椭圆脉冲环 + 柔光心跳点
            // ============================================================
            if (eternalLoveTime % 30 == 0)
            {
                float ringRot = t * 0.9f;
                Color pulseColor = Color.Lerp(cyan, purple, breathe) * (0.6f * strength);

                Particle pulse = new DirectionalPulseRing(
                    center,
                    Vector2.UnitY * 0.6f,           // 轻微方向性：像“心跳推动空气”
                    pulseColor,
                    new Vector2(1.25f, 2.9f),       // 椭圆比例（数学美感）
                    ringRot,
                    0.14f * strength,               // 初始 scale
                    0.05f * strength,               // 外扩速度
                    28
                );
                GeneralParticleHandler.SpawnParticle(pulse);

                var heart = new SquishyLightParticle(
                    center,
                    Vector2.Zero,
                    0.28f * strength,
                    Color.Lerp(white, purple, 0.35f) * (0.65f * strength),
                    20
                );
                GeneralParticleHandler.SpawnParticle(heart);
            }

            // ============================================================
            // 4）誓约回响（长周期）：让玩家感到“永远在”，但不刷屏
            // ============================================================
            if (eternalLoveTime % 90 == 0)
            {
                Color echoColor = Color.Lerp(purple, cyan, 0.55f) * (0.55f * strength);

                var ring = new BloomRing(center, Vector2.Zero, echoColor, 0.75f * strength, 40);
                GeneralParticleHandler.SpawnParticle(ring);

                // 四瓣微光：自然的花瓣意象 + 完全对称的几何秩序
                for (int i = 0; i < 4; i++)
                {
                    float ang = i * (MathHelper.Pi / 2f) + t * 0.35f;
                    Vector2 off = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * (18f + 8f * breathe);

                    var orb = new GlowOrbParticle(
                        center + off,
                        Vector2.Zero,
                        false,
                        10,
                        0.85f * strength,
                        Color.Lerp(cyan, white, 0.25f) * (0.7f * strength),
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }
        }

        // =========================
        // 小工具：全部写在本文件，避免依赖不确定的扩展方法
        // =========================

        private static Vector2 SafeNormalize(Vector2 v, Vector2 fallback)
        {
            float lenSq = v.LengthSquared();
            if (lenSq < 0.0001f)
                return fallback;

            float invLen = 1f / (float)Math.Sqrt(lenSq);
            return v * invLen;
        }

        private static Vector2 Rotate(Vector2 v, float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }

        private static float ToRotation(Vector2 v) => (float)Math.Atan2(v.Y, v.X);
    }
}