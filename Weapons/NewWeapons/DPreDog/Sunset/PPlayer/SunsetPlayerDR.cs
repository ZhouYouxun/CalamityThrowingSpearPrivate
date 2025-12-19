using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    public class SunsetPlayerDR : ModPlayer
    {
        // 重嶂倾轧

        // ================================
        // 核心状态变量（每帧重算）
        // ================================

        private bool inviolabilityActive;     // 不可伤害性是否激活
        private int inviolabilityTier;         // 档位（由防御力决定）

        private float bonusDR;                 // 伤害减免
        private int bonusDefense;              // 额外防御
        private int bonusLifeRegen;             // 生命回复
        private int extraIFrames;              // 额外无敌帧
        private float finalDamageMultiplier;   // 最终伤害倍率

        // ================================
        // 特效相关（只用于显示）
        // ================================

        private int vfxTimer;                  // 特效计时器
        private int orderedIndex;              // 有序矩阵索引

        // ================================
        // ResetEffects：只做一件事 → 清零
        // ================================

        public override void ResetEffects()
        {
            inviolabilityActive = false;
            inviolabilityTier = 0;

            bonusDR = 0f;
            bonusDefense = 0;
            bonusLifeRegen = 0;
            extraIFrames = 0;
            finalDamageMultiplier = 1f;
        }

        // ================================
        // PostUpdate：统一逻辑入口
        // ================================

        public override void PostUpdate()
        {
            // 只在本地玩家执行
            if (Player.whoAmI != Main.myPlayer)
                return;

            // 必须手持 Sunset
            if (Player.HeldItem.type != ModContent.ItemType<Sunset>())
                return;

            // 防御力必须 > 100
            int defense = Player.statDefense;
            if (defense <= 100)
                return;

            // ===== 根据防御力计算档位 =====
            if (defense > 250) inviolabilityTier = 4;
            else if (defense > 200) inviolabilityTier = 3;
            else if (defense > 150) inviolabilityTier = 2;
            else inviolabilityTier = 1;

            inviolabilityActive = true;

            // ===== 根据档位给予奖励 =====
            switch (inviolabilityTier)
            {
                case 1:
                    bonusDR = 0.10f;
                    extraIFrames = 1;
                    bonusLifeRegen = 3;
                    break;

                case 2:
                    bonusDR = 0.30f;
                    extraIFrames = 3;
                    bonusLifeRegen = 9;
                    break;

                case 3:
                    bonusDR = 0.50f;
                    extraIFrames = 6;
                    bonusLifeRegen = 12;
                    finalDamageMultiplier = 0.5f;
                    break;

                case 4:
                    bonusDR = 0.80f;
                    extraIFrames = 9;
                    finalDamageMultiplier = 0.15f;
                    bonusDefense = 100;
                    bonusLifeRegen = 30;
                    break;
            }

            // ===== 应用数值 =====
            Player.endurance += bonusDR;
            Player.statDefense += bonusDefense;
            Player.lifeRegen += bonusLifeRegen;

            // ===== 特效 =====
            vfxTimer++;
            int throttle = inviolabilityTier >= 3 ? 1 : 2;

            if ((vfxTimer % throttle) == 0)
            {
                float strength = MathHelper.Clamp(bonusDR / 0.8f, 0f, 1f);
                SpawnInviolabilityVfx(strength);
            }
        }

        // ================================
        // 不可伤害性：纵向矩阵护幕特效
        // ================================

        private void SpawnInviolabilityVfx(float strength)
        {
            // 只用 SquareParticle：线框立方体护盾（A：包裹玩家身体）
            // 目标：
            // 1) 3D 立方体缓慢旋转（整体统一，不散）
            // 2) 用矩阵投影到 2D（强 3D 感）
            // 3) 用方块粒子“画边”（12 条边），速度极小
            // 4) 有序扫描：某一条边依次点亮（扫描线）
            // 5) 斜线强调：沿 (X+Y) 的“对角带”周期增强亮度（像矩阵斜线扫过）

            // ---------- 小工具 ----------
            float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);

            Vector2 SafeNormalize(Vector2 v, Vector2 fallback)
            {
                float len = v.Length();
                if (len < 0.0001f)
                    return fallback;
                return v / len;
            }

            float Snap90(float angle)
            {
                float step = (MathHelper.Pi / 2f); // 90°
                return (float)Math.Round(angle / step) * step;
            }

            float SmoothStep01(float x)
            {
                x = Clamp01(x);
                return x * x * (3f - 2f * x);
            }

            // ---------- 基本参数 ----------
            Vector2 center = Player.Center + new Vector2(0f, Player.gfxOffY);

            float tier01 = MathHelper.Clamp(inviolabilityTier / 4f, 0f, 1f);

            // 时间要慢：不追求“飞”，追求“系统在运转”
            float time = vfxTimer;
            float t = time * (0.020f + 0.006f * tier01);

            // 立方体半边长（包裹玩家身体）：档位越高略大一点
            float halfSize = 28f + inviolabilityTier * 8f + 6f * strength;

            // 透视投影：焦距与深度偏置（保证可见且不炸）
            float focal = MathHelper.Lerp(280f, 220f, tier01);      // 越高档透视略强
            float zBias = halfSize * 3.2f + 120f;                  // 把整个立方体推到“镜头前方”

            // 旋转（整体统一）：三轴缓慢旋转，档位越高稍快一点点
            float yaw = t * MathHelper.Lerp(0.55f, 0.95f, tier01);
            float pitch = t * MathHelper.Lerp(0.43f, 0.82f, tier01) + (float)Math.Sin(t * 0.7f) * 0.15f * tier01;
            float roll = (float)Math.Sin(t * 0.9f) * 0.35f * tier01;

            Matrix rot = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            // 颜色：冷静矩阵（天蓝 + 浅灰），保证有最低可见度
            Color baseA = new Color(210, 235, 245); // 浅灰蓝
            Color baseB = new Color(110, 210, 255); // 科幻青
            Color baseC = Color.Lerp(baseA, baseB, 0.40f + 0.45f * tier01);

            // 生命周期：要能“铺成线框”，不能一闪就没
            int life = 34 + inviolabilityTier * 10;

            // 速度：极小（只给一点点“活性”）
            float speed = MathHelper.Lerp(0.03f, 0.10f, 0.35f + 0.65f * strength);

            // 边上采样段数：越高档线越细密
            int seg = 10 + inviolabilityTier * 4; // 10~26
            int pointsPerEdge = seg + 1;

            // 每次只刷一部分点：既省性能，又自带“扫描感”
            int totalPoints = 12 * pointsPerEdge;
            int emitCount = 60 + inviolabilityTier * 22; // 60~148（配合寿命可形成连续线框）

            // 扫描边：依次点亮 12 条边（强有序）
            int scanStepFrames = 10 - inviolabilityTier * 2; // 10,8,6,4
            if (scanStepFrames < 3)
                scanStepFrames = 3;
            int scanEdge = (int)(time / scanStepFrames) % 12;

            // 斜线强调：对角带中心随时间移动（X+Y 方向）
            float diagCenter = (float)Math.Sin(t * 0.6f) * 0.55f; // -0.55 ~ +0.55
            float diagWidth = MathHelper.Lerp(0.20f, 0.32f, tier01); // 带宽（越高档越厚一点）

            // ---------- 3D 顶点（单位立方体） ----------
            Vector3[] v = new Vector3[8]
            {
        new Vector3(-1f, -1f, -1f),
        new Vector3( 1f, -1f, -1f),
        new Vector3( 1f,  1f, -1f),
        new Vector3(-1f,  1f, -1f),
        new Vector3(-1f, -1f,  1f),
        new Vector3( 1f, -1f,  1f),
        new Vector3( 1f,  1f,  1f),
        new Vector3(-1f,  1f,  1f)
            };

            // 12 条边（线框立方体）
            // 顺序刻意固定：方便扫描“边”一条条亮起
            int[,] e = new int[12, 2]
            {
        {0,1},{1,2},{2,3},{3,0}, // 后面那一圈
        {4,5},{5,6},{6,7},{7,4}, // 前面那一圈
        {0,4},{1,5},{2,6},{3,7}  // 连接两圈的竖边
            };

            // ---------- 计算旋转后的 3D 顶点 ----------
            Vector3[] rv = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = v[i] * halfSize;
                rv[i] = Vector3.Transform(p, rot);
            }

            // ---------- 计算投影后的 2D 顶点 ----------
            Vector2[] pv = new Vector2[8];
            float[] z01v = new float[8]; // 深度（用于亮度/大小）
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = rv[i];

                // 透视：z 越大越“靠近镜头”（更大更亮）
                float denom = focal + (p.Z + zBias);
                if (denom < 80f)
                    denom = 80f;

                float persp = focal / denom;
                pv[i] = center + new Vector2(p.X, p.Y) * persp;

                // 归一化深度（大致 0~1）
                float z01 = Clamp01(((p.Z + halfSize) / (halfSize * 2f) + 1f) * 0.5f);
                z01v[i] = z01;
            }

            // ---------- 发射点序列（有序推进） ----------
            int start = orderedIndex % totalPoints;
            orderedIndex += emitCount;

            for (int k = 0; k < emitCount; k++)
            {
                int idx = (start + k) % totalPoints;
                int edgeIndex = idx / pointsPerEdge;
                int sIndex = idx % pointsPerEdge;

                int a = e[edgeIndex, 0];
                int b = e[edgeIndex, 1];

                float u = (pointsPerEdge <= 1) ? 0f : (sIndex / (float)(pointsPerEdge - 1));

                // 2D 线段插值（画边）
                Vector2 pA2 = pv[a];
                Vector2 pB2 = pv[b];
                Vector2 pos = Vector2.Lerp(pA2, pB2, u);

                // 3D 点也插一下：用于“斜线强调”（X+Y 的对角带）
                Vector3 pA3 = rv[a];
                Vector3 pB3 = rv[b];
                Vector3 p3 = Vector3.Lerp(pA3, pB3, u);

                // ---------- 亮度：强有序 ----------
                // 1) 基础亮度（确保可见）
                float baseBright = 0.55f + 0.25f * tier01 + 0.20f * strength;

                // 2) 扫描边增强（“边一条条亮起”）
                float edgeBoost = (edgeIndex == scanEdge) ? 0.55f : 0f;

                // 3) 斜线强调（对角带）：(X+Y) 落在带中心附近就变亮
                float diagCoord = (p3.X + p3.Y) / (halfSize * 2f);     // 大致 -1~1
                float d = Math.Abs(diagCoord - diagCenter);
                float diag01 = 1f - Clamp01(d / diagWidth);
                diag01 = SmoothStep01(diag01);
                float diagBoost = diag01 * (0.35f + 0.25f * tier01);

                // 4) 微弱脉冲（让系统“活着”，但不乱）
                float pulse = 0.88f + 0.12f * (float)Math.Sin(t * 1.1f + (edgeIndex * 0.9f + u * 2.7f));
                float bright = (baseBright + edgeBoost + diagBoost) * pulse;
                if (bright > 1.15f)
                    bright = 1.15f; // 允许略过 1，让它更“科技亮”，但别炸屏
                if (bright < 0.35f)
                    bright = 0.35f; // 保底可见

                // 深度加权：近处更亮一点点（更 3D）
                float z01 = (z01v[a] + z01v[b]) * 0.5f;
                bright *= (0.85f + 0.35f * z01);

                // ---------- 颜色与大小 ----------
                Color c = baseC * bright;
                // 不强制 A=0，避免某些混合下“直接看不见”
                c.A = 255;

                float scale = (0.75f + 0.40f * tier01 + 0.18f * strength) * (0.85f + 0.25f * z01);
                if (diag01 > 0.6f)
                    scale *= 1.08f; // 斜线带略粗一点，强调“斜线突出”

                // ---------- 速度：极小，但沿边方向 + 微弱外扩（上下左右都有分量） ----------
                Vector2 edgeDir = SafeNormalize(pB2 - pA2, Vector2.UnitX);
                Vector2 outward = SafeNormalize(pos - center, Vector2.UnitY);

                // 速度非常小：几乎不飞，主要靠“点亮/熄灭”形成运动感
                Vector2 vel = edgeDir * speed + outward * (speed * 0.35f);

                // ---------- 生成方块粒子 ----------
                SquareParticle sq = new SquareParticle(
                    pos,
                    vel,
                    false,
                    life,
                    scale,
                    c
                );

                // 旋转：锁定 0/90/180/270（直角矩阵味）
                float snapped = Snap90(edgeDir.ToRotation());
                // 给一点点“系统抖动”（极小），避免死板但仍然有序
                float micro = (float)Math.Sin(t * 0.7f + u * 6f) * (0.06f + 0.04f * tier01);
                sq.Rotation = snapped + micro;

                GeneralParticleHandler.SpawnParticle(sq);
            }
        }







        private static float Frac(float x) => x - (float)Math.Floor(x);

        // ================================
        // 伤害处理：只在命中时介入
        // ================================

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            ApplyInviolability(ref hurtInfo);
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            ApplyInviolability(ref hurtInfo);
        }

        private void ApplyInviolability(ref Player.HurtInfo hurtInfo)
        {
            if (!inviolabilityActive)
                return;

            Player.immune = true;
            Player.immuneNoBlink = true;
            Player.immuneTime += extraIFrames;

            // ★ 最高档位：单次伤害硬上限
            if (inviolabilityTier == 4 && hurtInfo.Damage > 20)
            {
                hurtInfo.Damage = 20;
            }

            hurtInfo.Damage = (int)(hurtInfo.Damage * finalDamageMultiplier);
        }



    }
}



/*
──────────────────────────────────────────────────────────────────────────────
【线框立方体 · 矩阵投影特效 说明文档 / 设计注释】

这一整段 SpawnInviolabilityVfx 所做的事情，可以用一句话概括：

    在 3D 空间中构造一个“线框立方体”，
    对其施加统一的旋转矩阵，
    再将其通过透视投影映射到 2D 屏幕上，
    并使用 SquareParticle 沿 12 条边进行“画线式”渲染，
    从而在 2D 游戏中模拟出一个稳定、有序、具有 3D 几何感的护盾结构。

──────────────────────────────────────────────────────────────────────────────
【一、整体设计目标（非常重要）】

本特效的目标不是“粒子飞舞”，而是：

1️⃣ 强有序（Order > Random）
    - 所有粒子都来源于一个明确的几何结构（立方体）
    - 没有随机角度、随机位置、随机方向

2️⃣ 强结构（Structure First）
    - 使用立方体（Cube）这种最基础、最稳定的 3D 几何体
    - 仅渲染“边”（Edges），不渲染面，强调理性、矩阵、科技感

3️⃣ 强一致性（Global Transform）
    - 所有点共享同一个旋转矩阵
    - 所有点共享同一个投影模型
    - 不允许粒子各自“自由发挥”

4️⃣ 慢速变化（Temporal Stability）
    - 旋转很慢
    - 粒子初速度极小
    - 视觉变化主要来自“几何体在动”，而不是“粒子在飞”

这使得该特效：
    看起来像一个“系统在运行”，而不是“特效在爆炸”。

──────────────────────────────────────────────────────────────────────────────
【二、为什么要从 3D → 2D？】

虽然 Terraria 是 2D 游戏，但人类视觉系统对“3D 结构”极其敏感。

只要满足以下三点：
    - 统一坐标系
    - 统一变换（旋转）
    - 深度影响投影结果

人脑就会自动补全“这是一个立体物体”。

这正是线性代数中“投影（Projection）”的实际用途。

──────────────────────────────────────────────────────────────────────────────
【三、3D 几何部分：立方体的构造】

立方体使用 8 个顶点（单位立方体）：

    (-1, -1, -1) 到 (1, 1, 1)

通过 halfSize 进行缩放后，得到一个：
    - 包裹玩家身体的立方体
    - 尺寸随档位 / 强度略微变化

接着定义 12 条边（Edge List）：
    - 后方 4 条
    - 前方 4 条
    - 连接前后面的 4 条

所有渲染都严格沿这些边进行。

📌 重要：  
这一步确保了“粒子永远不会出现在结构之外”。

──────────────────────────────────────────────────────────────────────────────
【四、线性代数核心：旋转矩阵】

使用 Matrix.CreateFromYawPitchRoll 构造 3×3 旋转矩阵：

    R = R_yaw · R_pitch · R_roll

并对所有顶点统一执行：

    p_rotated = R · p

这是一个标准的线性变换（Linear Transformation）：

    - 保持几何结构不变
    - 只改变空间方向

📌 关键点：
    所有顶点共享同一个 R，
    这就是“整体在动，而不是局部在乱动”的数学根源。

──────────────────────────────────────────────────────────────────────────────
【五、从 3D 到 2D：透视投影（Perspective Projection）】

投影公式本质上是：

    x' = x * focal / (z + zBias)
    y' = y * focal / (z + zBias)

其中：
    - focal   ≈ 焦距，控制透视强度
    - zBias   ≈ 把整个立方体推到“镜头前”，防止反向翻转

直观含义是：

    z 越大（越靠近观察者）
    → 投影后的 x、y 越大
    → 看起来“更近 / 更大”

这一步就是：
    线性代数 + 射影几何 在游戏里的直接应用。

──────────────────────────────────────────────────────────────────────────────
【六、为什么“画边”而不是“画点 / 画面”？】

只画边（Wireframe）有三个好处：

1️⃣ 数学感最强
    - 边 = 线性插值（Lerp）
    - 非常“线性代数”

2️⃣ 信息密度高
    - 少量粒子就能表达完整结构

3️⃣ 视觉克制
    - 不会糊成一坨
    - 非常适合“防御 / 护盾 / 系统”语义

每一条边被等分成若干段，
并在这些分段点上生成 SquareParticle，
从而“用粒子画线”。

──────────────────────────────────────────────────────────────────────────────
【七、扫描边 & 斜线强调（视觉节奏）】

为了避免“静态模型感”，但又不引入随机：

1️⃣ 扫描边（Scan Edge）
    - 12 条边按顺序轮流增强亮度
    - 像系统在进行“结构扫描”

2️⃣ 斜线强调（Diagonal Emphasis）
    - 使用 (X + Y) 的对角坐标
    - 形成一条随时间移动的“斜线亮带”

这两者都属于：
    有序、可预测、可复现的时间变化。

──────────────────────────────────────────────────────────────────────────────
【八、粒子运动为什么“几乎不动”？】

因为：
    这个特效表达的是“结构存在”，而不是“能量喷发”。

粒子的速度：
    - 只是为了避免完全静止导致的“假图层感”
    - 真正的运动来自“几何体旋转 + 投影变化”

这是一种非常“工程化”的视觉策略。

──────────────────────────────────────────────────────────────────────────────
【九、如何高效复制 / 复用这套特效？】

如果将来要复用，只需三步：

1️⃣ 替换几何体
    - 把立方体顶点换成：
        · 长方体
        · 四棱锥
        · 正八面体
        · 任意凸多面体

2️⃣ 保留：
    - 统一旋转矩阵
    - 统一投影
    - 沿边线性插值画线

3️⃣ 调整：
    - halfSize（尺寸）
    - seg（边分段数）
    - 扫描节奏

整个框架不需要重写。

──────────────────────────────────────────────────────────────────────────────
【十、和线性代数直接相关的三个关键词（复习用）】

✔ Linear Transformation（线性变换）
    - 用矩阵统一改变空间中的所有点

✔ Projection（投影）
    - 将高维空间的信息映射到低维空间
    - 丢失维度，但保留结构关系

✔ Coordinate System（坐标系）
    - 一切“秩序感”的根源
    - 一旦坐标系统一，视觉就会统一

──────────────────────────────────────────────────────────────────────────────
【总结】

这段代码不是“特效代码”，
而是一次：

    几何 → 代数 → 投影 → 视觉

的完整链路实践。

它的价值不在于炫，
而在于：稳定、可理解、可复用。

──────────────────────────────────────────────────────────────────────────────
*/
