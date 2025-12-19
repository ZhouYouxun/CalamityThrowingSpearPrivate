using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptPlayer : ModPlayer
    {
        public bool hasSunsetBuff = false;

        // =========================
        // 自己的计时器字段（不依赖 localAI）
        // =========================
        private int conceptTime;   // 总相位：驱动所有“数学秩序”
        private int fadeTimer;     // Buff 断开后缓慢淡出，避免瞬间消失
        private int spiralIndex;   // 自然粒子：叶序角序列索引

        public override void ResetEffects()
        {
            hasSunsetBuff = false; // 每帧重置 Buff 状态（PBuff.Update 会再置 true）
        }

        public override void PostUpdate()
        {
            // 只在本地玩家生成粒子，避免多人重复刷
            if (Player.whoAmI != Main.myPlayer)
                return;

            // Buff 在：直接维持淡出缓冲；Buff 不在：缓慢衰减
            if (hasSunsetBuff)
                fadeTimer = 60;
            else if (fadeTimer > 0)
                fadeTimer--;

            if (fadeTimer <= 0)
                return;

            conceptTime++;

            // 强度 0~1：用于淡出，也用于控制“无序扰动”的幅度
            float strength = fadeTimer / 60f;

            SpawnConceptVfx(strength);
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            // 你原来的逻辑：保留不动
            if (hasSunsetBuff && Main.rand.NextFloat() < 0.05f) // 5% 概率触发
            {
                Player.statLife += 200; // 恢复 200 生命
                Player.HealEffect(200); // 显示治疗效果
            }
        }

        private void SpawnConceptVfx(float strength)
        {
            // 玩家中心（考虑 gfxOffY）
            Vector2 center = Player.Center + new Vector2(0f, Player.gfxOffY);

            // 允许“非中心点关联”：轻微偏移到玩家速度方向，形成“惯性参考系”感觉
            Vector2 anchor = center + Player.velocity * 4f;

            // 同源概念系配色：冷白 + 科技青 + 紫
            Color cyan = new Color(90, 210, 255);
            Color purple = new Color(175, 120, 255);
            Color white = new Color(255, 255, 255);

            // ========= 时间轴（稳定、有序）=========
            float t = conceptTime * 0.045f;
            float breathe = (float)(Math.Sin(t) * 0.5 + 0.5);     // 0~1 呼吸
            float breathe2 = (float)(Math.Sin(t * 0.5f + 1.3f) * 0.5 + 0.5);

            // ============================================================
            // 1) 3D 结构主骨架：三重正交环（像陀螺/坐标系）
            //    用“旋转矩阵 + 透视投影”做 3D 错觉（有序为主，扰动为辅）
            // ============================================================
            if (conceptTime % 4 == 0)
            {
                // 3D 环尺度（越大越“空间感”）
                float R = 64f; // 主半径（像 4×16 左右的量级）
                float r = 14f; // 次半径（环厚度/截面）

                // 旋转（相位推进）：yaw/pitch/roll
                float yaw = t * 0.55f;
                float pitch = (float)Math.Sin(t * 0.35f) * 0.55f;
                float roll = t * 0.25f + (float)Math.Sin(t * 0.22f) * 0.35f;

                // 每次只采样少量点，靠“稳定演化”形成连续感（节流）
                int pts = 6;

                // 三个正交环：XY / YZ / ZX（概念 = 坐标系被具象化）
                SpawnOrthoRing(anchor, R, r, pts, yaw, pitch, roll, 0, cyan, purple, white, strength, breathe);
                SpawnOrthoRing(anchor, R, r, pts, yaw, pitch, roll, 1, cyan, purple, white, strength, breathe2);
                SpawnOrthoRing(anchor, R, r, pts, yaw, pitch, roll, 2, cyan, purple, white, strength, breathe);
            }

            // ============================================================
            // 2) 时空脉冲：低频椭圆“相位波”（强秩序）
            //    让玩家有“系统在运行/维度在刷新”的感觉
            // ============================================================
            if (conceptTime % 10 == 0)
            {
                Color pulseColor = Color.Lerp(cyan, purple, 0.35f + 0.35f * breathe) * (0.55f * strength);

                Particle pulse = new DirectionalPulseRing(
                    center,
                    Vector2.UnitY * 0.35f,
                    pulseColor,
                    new Vector2(1.05f, 3.15f), // 椭圆比例：像“空间切片”
                    t * 0.9f,
                    0.52f * strength,
                    0.095f * strength,
                    28
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // ============================================================
            // 3) 自然意象：叶序角“星尘/花粉”螺旋（有序+无序混合）
            //    有序：137.5° 均匀填充
            //    无序：受控风扰动（确定性正弦，不靠纯随机堆砌）
            // ============================================================
            if (conceptTime % 2 == 0)
            {
                float golden = 137.5f * (MathHelper.Pi / 180f);
                int n = spiralIndex++;

                float baseR = (float)Math.Sqrt(n % 180) * 4.6f; // 0~约 62
                baseR = MathHelper.Clamp(baseR, 10f, 70f);

                float ang = n * golden + t * 0.35f;
                Vector2 spiral = ang.ToRotationVector2() * baseR;

                // “风扰动”：确定性（可复现），让自然层有轻微无序
                float windA = (float)Math.Sin(t * 1.7f + n * 0.13f);
                float windB = (float)Math.Cos(t * 1.3f - n * 0.09f);
                Vector2 chaos = new Vector2(windA, windB) * (2.4f * strength);

                Vector2 pos = center + spiral + chaos;

                Color dustColor = Color.Lerp(purple, cyan, 0.45f + 0.35f * (float)Math.Sin(n * 0.17f)) * (0.65f * strength);
                float sc = MathHelper.Lerp(0.75f, 1.15f, breathe2) * strength;

                // 点粒子：像“星尘/花粉”，密度克制
                var p = new PointParticle(pos, Vector2.Zero, false, 18, sc, dustColor);
                GeneralParticleHandler.SpawnParticle(p);

                // 低频补一丝“雾”增强层次（同源、克制）
                if (conceptTime % 12 == 0)
                {
                    var mist = new WaterFlavoredParticle(
                        pos,
                        Vector2.Zero,
                        false,
                        20,
                        0.85f * strength,
                        Color.Lerp(cyan, purple, 0.5f) * (0.45f * strength)
                    );
                    GeneralParticleHandler.SpawnParticle(mist);
                }
            }

            // ============================================================
            // 4) 概念“回响”：更长周期的 BloomRing（低频、像空间在呼吸）
            // ============================================================
            if (conceptTime % 30 == 0)
            {
                Color ringColor = Color.Lerp(purple, cyan, 0.55f) * (0.55f * strength);
                var ring = new BloomRing(center, Vector2.Zero, ringColor, 0.75f * strength, 40);
                GeneralParticleHandler.SpawnParticle(ring);

                // 很克制的强光点，像“坐标原点被刷新”
                var flash = new StrongBloom(center, Vector2.Zero, Color.Lerp(cyan, white, 0.25f) * (0.5f * strength), 1.25f * strength, 22);
                GeneralParticleHandler.SpawnParticle(flash);
            }
        }

        /// <summary>
        /// 生成一个“正交环”切片（XY/YZ/ZX），并用 3D 旋转+透视投影产生 3D 错觉。
        /// ringType：0=XY, 1=YZ, 2=ZX
        /// </summary>
        private void SpawnOrthoRing(
            Vector2 anchor,
            float R, float r, int pts,
            float yaw, float pitch, float roll,
            int ringType,
            Color cyan, Color purple, Color white,
            float strength,
            float breathe)
        {
            // 透视参数：camera 越大越“平”，越小越“3D”
            float camera = 220f;
            float fov = 260f;

            // 每个环的相位错开一点，让层级更丰富
            float phase = ringType * 1.7f + breathe * 0.6f;

            for (int i = 0; i < pts; i++)
            {
                float u = (i / (float)pts) * MathHelper.TwoPi + phase;
                float v = (i * 0.9f) + phase * 0.7f;

                // 构造一个“环面点”（Torus Point）：(R + r*cos v) * (cos u, sin u) + r*sin v * axis
                Vector3 p3;

                float cu = (float)Math.Cos(u);
                float su = (float)Math.Sin(u);
                float cv = (float)Math.Cos(v);
                float sv = (float)Math.Sin(v);

                float major = R + r * cv;

                // 三种正交切片：把“环面”的轴放到不同平面
                if (ringType == 0)       // XY 环：Z 负责厚度
                    p3 = new Vector3(major * cu, major * su, r * sv);
                else if (ringType == 1)  // YZ 环：X 负责厚度
                    p3 = new Vector3(r * sv, major * cu, major * su);
                else                     // ZX 环：Y 负责厚度
                    p3 = new Vector3(major * cu, r * sv, major * su);

                // 3D 旋转：用“矩阵式”旋转（yaw/pitch/roll）
                p3 = RotateXYZ(p3, pitch, yaw, roll);

                // 透视投影：z 越大越远，越暗越小
                float z = p3.Z + camera;
                float k = fov / Math.Max(40f, z);

                Vector2 p2 = new Vector2(p3.X, p3.Y) * k;

                // 深度驱动：越近越亮、越大
                float depth01 = MathHelper.Clamp((k - 0.7f) / 0.9f, 0f, 1f);
                Color c = Color.Lerp(purple, cyan, depth01);
                c = Color.Lerp(c, white, 0.12f + 0.25f * depth01) * (0.75f * strength);

                // 有序速度：沿投影切向轻微滑动（像“环在自转”）
                Vector2 tangent = new Vector2(-p2.Y, p2.X);
                tangent = SafeNormalize(tangent, Vector2.UnitY);

                // 无序扰动：确定性微颤，让它“活”但不乱
                float jitter = (float)Math.Sin((ringType + 1) * 1.31f + u * 2.0f) * 0.35f;
                Vector2 micro = tangent.RotatedBy(jitter) * (0.35f + 0.35f * depth01) * strength;

                Vector2 pos = anchor + p2 + micro;

                // Square：概念/系统感最强；寿命短，靠时间演化拼成连续“结构”
                float sc = MathHelper.Lerp(0.65f, 1.05f, depth01) * strength;
                int life = 18;

                var sq = new SquareParticle(pos, micro * 0.8f, false, life, sc, c);
                sq.Rotation = (float)Math.Atan2(tangent.Y, tangent.X);
                GeneralParticleHandler.SpawnParticle(sq);

                // 稀疏点缀：让结构更“概念”，但控量（每环仅 1 个左右）
                if (i == 0 && (conceptTime % 16 == 0))
                {
                    var dot = new PointParticle(pos, Vector2.Zero, false, 16, 1.0f * strength, c * 0.9f);
                    GeneralParticleHandler.SpawnParticle(dot);
                }
            }
        }

        // =========================
        // 工具函数：全部自带，避免依赖不确定扩展
        // =========================

        private static Vector2 SafeNormalize(Vector2 v, Vector2 fallback)
        {
            float lenSq = v.LengthSquared();
            if (lenSq < 1e-6f)
                return fallback;
            float inv = 1f / (float)Math.Sqrt(lenSq);
            return v * inv;
        }

        private static Vector3 RotateXYZ(Vector3 v, float x, float y, float z)
        {
            // 绕 X
            float cx = (float)Math.Cos(x);
            float sx = (float)Math.Sin(x);
            float y1 = v.Y * cx - v.Z * sx;
            float z1 = v.Y * sx + v.Z * cx;
            v = new Vector3(v.X, y1, z1);

            // 绕 Y
            float cy = (float)Math.Cos(y);
            float sy = (float)Math.Sin(y);
            float x2 = v.X * cy + v.Z * sy;
            float z2 = -v.X * sy + v.Z * cy;
            v = new Vector3(x2, v.Y, z2);

            // 绕 Z
            float cz = (float)Math.Cos(z);
            float sz = (float)Math.Sin(z);
            float x3 = v.X * cz - v.Y * sz;
            float y3 = v.X * sz + v.Y * cz;
            return new Vector3(x3, y3, v.Z);
        }
    }
}
