using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Renderers;
using Terraria.Graphics.Shaders;
using Terraria.GameContent;

namespace CalamityThrowingSpear
{
    public class CTSLightingBoltsSystem
    {
        private static ParticlePool<PrettySparkleParticle> _poolPrettySparkle = new ParticlePool<PrettySparkleParticle>(200, () => new PrettySparkleParticle());

        // 原版内置了很多很多的特效【它们可以通过 ParticleOrchestrator.RequestParticleSpawn() 进行调用】，你可以直接检查相关文件来获得相关信息
        // 可以看一下这一段：https://gist.github.com/Rijam/971b5252707860b65b582093580aa49c
        // 想去学习的话可以去这里：namespace Terraria.GameContent.Drawing; public class ParticleOrchestrator
        // 这里面的各种光点和光学效果都是参考了上面这些的，更加倾向于那些魔法，能量，生物，环境，反射相关的特效
        // 注意！！注意！！
        // 本文件里的所有特效均不需要灾厄Calamity运行！这些东西完全独立，因为这些特效是基于原版1.4.4.9 Terraria的！


        // 神圣晶石子弹-命中
        public static void Spawn_IonizingRadiation(Vector2 position)
        {
            float triangleAngleOffset = MathHelper.ToRadians(60f);
            float baseRotation = MathHelper.ToRadians(-30f); // 让扇形角度正确对齐
            float distance = 10f; // 光点到中心的距离
            int triangleCount = 3;

            for (int i = 0; i < triangleCount; i++)
            {
                float rotationOffset = baseRotation + i * MathHelper.ToRadians(120f);
                for (int j = 0; j < 3; j++)
                {
                    float angle = rotationOffset + j * triangleAngleOffset;
                    Vector2 spawnOffset = angle.ToRotationVector2() * distance;

                    PrettySparkleParticle particle = _poolPrettySparkle.RequestParticle();
                    particle.ColorTint = new Color(1f, 1f, 0.2f, 1f); // 亮黄色
                    particle.LocalPosition = position + spawnOffset;
                    particle.Rotation = angle;
                    particle.Scale = new Vector2(2f, 0.5f);
                    particle.FadeInNormalizedTime = 5E-06f;
                    particle.FadeOutNormalizedTime = 0.95f;
                    particle.TimeToLive = 30;
                    particle.FadeOutEnd = 30;
                    particle.FadeInEnd = 15;
                    particle.FadeOutStart = 15;
                    particle.AdditiveAmount = 0.35f;
                    Main.ParticleSystem_World_OverPlayers.Add(particle);
                }
            }
        }


        // 永恒子弹-升级一次
        public static void Spawn_FlowerPattern(Vector2 position)
        {
            int maxParticles = 25;
            float radius = 30f; // 花瓣大小
            int petals = 5; // 花瓣数量
            float petalCurveFactor = 0.2f; // 弯曲程度

            // 计算花瓣角度
            for (int petalIndex = 0; petalIndex < petals; petalIndex++)
            {
                float baseAngle = MathHelper.TwoPi / petals * petalIndex;

                // 生成花瓣的光点
                for (int i = 0; i < maxParticles / petals; i++)
                {
                    float progress = (float)i / (maxParticles / petals - 1); // 归一化 0~1
                    float angleOffset = (float)Math.Sin(progress * MathHelper.Pi) * petalCurveFactor; // 让路径弯曲
                    float angle = baseAngle + angleOffset;
                    Vector2 offset = angle.ToRotationVector2() * (radius * progress); // 生成弯曲的路径
                    Vector2 spawnPos = position + offset;

                    PrettySparkleParticle particle = _poolPrettySparkle.RequestParticle();
                    particle.ColorTint = new Color(0.3f, 1f, 0.3f, 1f); // 翠绿色
                    particle.LocalPosition = spawnPos;
                    particle.Rotation = angle;
                    particle.Scale = new Vector2(1.8f, 0.8f);
                    particle.FadeInNormalizedTime = 5E-06f;
                    particle.FadeOutNormalizedTime = 0.95f;
                    particle.TimeToLive = 45;
                    particle.FadeOutEnd = 45;
                    particle.FadeInEnd = 15;
                    particle.FadeOutStart = 30;
                    particle.AdditiveAmount = 0.4f;
                    Main.ParticleSystem_World_OverPlayers.Add(particle);
                }
            }

            // 添加中心的粉红色光点
            PrettySparkleParticle centerParticle = _poolPrettySparkle.RequestParticle();
            centerParticle.ColorTint = new Color(1f, 0.5f, 0.5f, 1f); // 粉红色
            centerParticle.LocalPosition = position;
            centerParticle.Scale = new Vector2(2f, 1f);
            centerParticle.FadeInNormalizedTime = 5E-06f;
            centerParticle.FadeOutNormalizedTime = 0.95f;
            centerParticle.TimeToLive = 50;
            centerParticle.FadeOutEnd = 50;
            centerParticle.FadeInEnd = 20;
            centerParticle.FadeOutStart = 30;
            centerParticle.AdditiveAmount = 0.5f;
            Main.ParticleSystem_World_OverPlayers.Add(centerParticle);
        }

        // 泰拉巨箭-分裂
        public static void Spawn_AncientForestWisdom(Vector2 position)
        {
            int totalPoints = 16; // 总光点数量
            int pointsPerSide = totalPoints / 4; // 每条边上的光点数量
            float squareHalfSize = 40f; // 正方形半边长（5×16 像素，整体宽高 80×80）

            Color deepForestGreen = new Color(0.1f, 0.6f, 0.1f, 1f); // 深绿色
            Vector2[] squareOffsets =
            {
                new Vector2(1, 0), // 右
                new Vector2(0, 1), // 下
                new Vector2(-1, 0), // 左
                new Vector2(0, -1) // 上
            };

            // 遍历四条边
            for (int side = 0; side < 4; side++)
            {
                Vector2 baseDirection = squareOffsets[side]; // 方向单位向量
                Vector2 perpendicularDirection = new Vector2(-baseDirection.Y, baseDirection.X); // 计算垂直方向的向量

                // 在每条边上生成光点
                for (int i = 0; i < pointsPerSide; i++)
                {
                    float progress = (float)i / (pointsPerSide - 1); // 归一化 0~1
                    Vector2 offset = baseDirection * squareHalfSize + perpendicularDirection * (progress - 0.5f) * squareHalfSize * 2;
                    Vector2 spawnPos = position + offset;

                    PrettySparkleParticle particle = _poolPrettySparkle.RequestParticle();
                    particle.ColorTint = deepForestGreen;
                    particle.LocalPosition = spawnPos;
                    particle.Scale = new Vector2(1.8f, 0.8f);
                    particle.FadeInNormalizedTime = 5E-06f;
                    particle.FadeOutNormalizedTime = 0.95f;
                    particle.TimeToLive = 45;
                    particle.FadeOutEnd = 45;
                    particle.FadeInEnd = 15;
                    particle.FadeOutStart = 30;
                    particle.AdditiveAmount = 0.4f;
                    Main.ParticleSystem_World_OverPlayers.Add(particle);
                }
            }
        }

        // 幽灵箭-命中
        public static void Spawn_SpectralWhispers(Vector2 position)
        {
            int ghostPoints = 12; // 幽魂光点数量
            float spreadRadius = 50f; // 光点扩散半径
            float waveIntensity = 10f; // 漂浮波动幅度
            float lifetime = 60; // 光点存活时间

            // 颜色设定：浅蓝色 & 白色
            Color spectralBlue = new Color(0.5f, 0.8f, 1f, 0.8f); // 淡蓝色
            Color spectralWhite = new Color(1f, 1f, 1f, 0.8f); // 纯白色

            for (int i = 0; i < ghostPoints; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机角度
                float distance = Main.rand.NextFloat(spreadRadius * 0.5f, spreadRadius); // 让光点随机分布
                Vector2 basePos = position + angle.ToRotationVector2() * distance;

                // 生成光点
                PrettySparkleParticle particle = _poolPrettySparkle.RequestParticle();
                particle.ColorTint = Main.rand.NextBool(3) ? spectralWhite : spectralBlue; // 3:1 概率使用浅蓝色或白色
                particle.LocalPosition = basePos;
                particle.Scale = new Vector2(1.2f, 1.2f);
                particle.FadeInNormalizedTime = 0.05f;
                particle.FadeOutNormalizedTime = 0.9f;
                particle.TimeToLive = (int)(lifetime * Main.rand.NextFloat(0.8f, 1.2f)); // 随机生存时间
                particle.FadeOutEnd = particle.TimeToLive;
                particle.FadeInEnd = (int)(particle.TimeToLive * 0.3f);
                particle.FadeOutStart = (int)(particle.TimeToLive * 0.7f);
                particle.AdditiveAmount = 0.6f;

                // 给点随机性
                float waveFrequency = Main.rand.NextFloat(0.05f, 0.15f); // 波动频率
                float waveAmplitude = Main.rand.NextFloat(waveIntensity * 0.5f, waveIntensity); // 波动大小
                particle.Velocity = new Vector2(0, (float)Math.Sin(Main.GameUpdateCount * waveFrequency) * waveAmplitude);

                Main.ParticleSystem_World_OverPlayers.Add(particle);
            }
        }


        // 山铜弹-命中
        public static void Spawn_BlossomPathIndicator(Vector2 position, Player player)
        {
            int lightCount = Main.rand.Next(3, 5); // 生成 3~4 个光点
            float minOffset = 8 * 16f;
            float maxOffset = 10 * 16f;

            // 颜色：粉红色 & 浅粉红色
            Color softPink = new Color(1f, 0.6f, 0.8f, 0.9f); // 粉红色
            Color lightPink = new Color(1f, 0.8f, 0.9f, 0.9f); // 浅粉红色

            // 计算光点偏移方向（朝玩家的反方向）
            Vector2 direction = new Vector2(-player.direction, 0); // -1 或 1
            for (int i = 0; i < lightCount; i++)
            {
                // 在 3×16 到 5×16 之间随机偏移
                float distance = Main.rand.NextFloat(minOffset, maxOffset);
                Vector2 spawnPosition = position + direction * distance;

                // 让光点位置稍微随机一些，使其不会完全对齐
                spawnPosition.X += Main.rand.NextFloat(-20f, 20f); // 左右扩散
                spawnPosition.Y += Main.rand.NextFloat(-35f, 35f); // 上下扩散

                // 创建光点
                PrettySparkleParticle particle = _poolPrettySparkle.RequestParticle();
                particle.ColorTint = Main.rand.NextBool() ? softPink : lightPink; // 随机选择粉红色或浅粉红色
                particle.LocalPosition = spawnPosition;
                particle.Scale = new Vector2(1.5f, 1.5f);
                particle.FadeInNormalizedTime = 0.1f;
                particle.FadeOutNormalizedTime = 0.9f;
                particle.TimeToLive = Main.rand.Next(30, 45); // 30~45 帧
                particle.FadeOutEnd = particle.TimeToLive;
                particle.FadeInEnd = (int)(particle.TimeToLive * 0.3f);
                particle.FadeOutStart = (int)(particle.TimeToLive * 0.7f);
                particle.AdditiveAmount = 0.5f;

                Main.ParticleSystem_World_OverPlayers.Add(particle);
            }
        }

        // 龙豪弹-命中
        public static void Spawn_GreenShimmerPath(Vector2 position, Player player)
        {
            int pointCount = Main.rand.Next(5, 8); // 生成 5~7 个光点
            float minDistance = 48f; // 3 × 16
            float maxDistance = 64f; // 4 × 16
            float radius = 16f; // 圆的半径（2 × 16）

            // 颜色：翠绿色 & 中等绿色
            Color brightGreen = new Color(0.3f, 1f, 0.3f, 1f); // 翠绿色
            Color mediumGreen = new Color(0.2f, 0.8f, 0.2f, 1f); // 中等绿色

            // 计算偏移方向（从 `position` 朝向玩家的方向）
            Vector2 directionToPlayer = (player.Center - position).SafeNormalize(Vector2.Zero);
            float travelDistance = Main.rand.NextFloat(minDistance, maxDistance);
            Vector2 travelPoint = position + directionToPlayer * travelDistance; // 计算最终落点

            // 在 `travelPoint` 附近的 `radius` 范围内随机生成光点
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 spawnPos = travelPoint + Main.rand.NextVector2Circular(radius, radius); // 让光点在圆内随机分布

                // 创建光点
                PrettySparkleParticle particle = _poolPrettySparkle.RequestParticle();
                particle.ColorTint = Main.rand.NextBool() ? brightGreen : mediumGreen; // 随机选择绿色
                particle.LocalPosition = spawnPos;
                particle.Scale = new Vector2(1.5f, 1.5f);
                particle.FadeInNormalizedTime = 5E-06f; // 几乎瞬间淡入
                particle.FadeOutNormalizedTime = 0.95f; // 让光点消失得更加柔和
                particle.TimeToLive = Main.rand.Next(40, 60); // 40~60 帧的生命周期
                particle.FadeOutEnd = particle.TimeToLive;
                particle.FadeInEnd = (int)(particle.TimeToLive * 0.3f);
                particle.FadeOutStart = (int)(particle.TimeToLive * 0.5f); // 让光点更早进入淡出阶段
                particle.AdditiveAmount = 0.35f; // 让光点的发光更自然

                Main.ParticleSystem_World_OverPlayers.Add(particle);
            }
        }

        private static ParticlePool<FadingParticle> _poolFading = new ParticlePool<FadingParticle>(100, () => new FadingParticle());
        // 熔渣弹-命中【橙色光圈，但是被废弃了】
        public static void Spawn_ExpandingOrangeRing(Vector2 position)
        {
            int ringCount = Main.rand.Next(1, 3); // 生成 1~2 个光环
            float expansionSpeed = 0.1f; // 扩散速度
            float lifetime = 40; // 粒子存活时间

            // 颜色：橘黄色
            Color orangeColor = new Color(1f, 0.6f, 0.2f, 1f);

            for (int i = 0; i < ringCount; i++)
            {
                // 生成光环粒子
                FadingParticle ringParticle = _poolFading.RequestParticle();

                // **修正贴图，使用 Keybrand 的光环贴图**
                ringParticle.SetBasicInfo(TextureAssets.Extra[174], null, Vector2.Zero, position);
                ringParticle.SetTypeInfo(lifetime);

                // 颜色 & 透明度设定
                ringParticle.ColorTint = orangeColor;
                ringParticle.ColorTint.A = 200; // 让光环稍微透明一点

                // 设定大小 & 扩散
                ringParticle.Scale = Vector2.One * (0.5f + Main.rand.NextFloat() * 0.5f);
                ringParticle.ScaleVelocity = Vector2.One * expansionSpeed;
                ringParticle.ScaleAcceleration = -ringParticle.ScaleVelocity / lifetime; // 让光环在消失前放缓扩散速度

                // 设定淡入淡出
                ringParticle.FadeInNormalizedTime = 0.1f;
                ringParticle.FadeOutNormalizedTime = 0.9f;

                // 添加到粒子系统
                Main.ParticleSystem_World_OverPlayers.Add(ringParticle);
            }
        }



        // 空弹-命中
        public static void Spawn_GhostlyImpact(Vector2 position)
        {
            int ghostPointCount = Main.rand.Next(3, 7); // 生成 3~7 个光点
            float minRadius = 48f; // 3×16
            float maxRadius = 80f; // 5×16
            float speed = 2f; // 速度，向中心收缩

            // 幽灵蓝色光点
            Color ghostBlue = new Color(0.5f, 0.8f, 1f, 1f);

            for (int i = 0; i < ghostPointCount; i++)
            {
                // 随机生成光点的初始位置，在 3×16 ~ 5×16 半径范围内
                Vector2 spawnOffset = Main.rand.NextVector2Circular(minRadius, maxRadius);
                Vector2 spawnPosition = position + spawnOffset;

                // 计算朝向中心但带有随机偏移的速度方向
                Vector2 velocityDirection = (position - spawnPosition).SafeNormalize(Vector2.Zero);
                velocityDirection = velocityDirection.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)); // 添加偏移
                Vector2 velocity = velocityDirection * speed;

                // 创建光点
                PrettySparkleParticle particle = _poolPrettySparkle.RequestParticle();
                particle.ColorTint = ghostBlue;
                particle.LocalPosition = spawnPosition;
                particle.Velocity = velocity; // 让光点朝向中心点位移动
                particle.Scale = new Vector2(1.5f, 1.5f);
                particle.FadeInNormalizedTime = 5E-06f; // 快速淡入
                particle.FadeOutNormalizedTime = 0.95f; // 柔和消失
                particle.TimeToLive = Main.rand.Next(40, 60); // 40~60 帧
                particle.FadeOutEnd = particle.TimeToLive;
                particle.FadeInEnd = (int)(particle.TimeToLive * 0.3f);
                particle.FadeOutStart = (int)(particle.TimeToLive * 0.7f);
                particle.AdditiveAmount = 0.45f; // 提高光点的发光感

                Main.ParticleSystem_World_OverPlayers.Add(particle);
            }
        }




        // 调星弹-命中
        public static void Spawn_AstralSoulLightsA(Vector2 position)
        {
            float lifespan = 36f; // 生命时长
            float fadeTime = lifespan / 2f; // 过渡时间

            // 颜色池，确保每个光点独立选取其中一个颜色，而不是混合颜色
            // 你可以直接下载某一个武器的贴图到本地，然后用photoshop打开选取这个颜色，点开，你就能看到它的16进制和rgb编码了
            Color[] colorPool = new Color[]
            {
        new Color(255, 164, 94),  // #FFA45E
        new Color(66, 189, 181),  // #42BDB5
        new Color(109, 242, 196), // #6DF2C4
        new Color(237, 93, 83)    // #ED5D53
            };

            // 生成 3~7 个光点
            int pointCount = Main.rand.Next(3, 8);
            for (int i = 0; i < pointCount; i++)
            {
                PrettySparkleParticle spark = _poolPrettySparkle.RequestParticle();

                // 随机选择颜色
                spark.ColorTint = colorPool[Main.rand.Next(colorPool.Length)];

                // 位置设定
                spark.LocalPosition = position;

                // 旋转角度随机（无运动，仅用于光点方向的随机变化）
                spark.Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;

                // 设定光点大小
                spark.Scale = new Vector2(2f, 2f) * (0.8f + Main.rand.NextFloat() * 0.4f);

                // 设定淡入淡出时机
                spark.FadeInNormalizedTime = 5E-06f; // 极短时间淡入
                spark.FadeOutNormalizedTime = 0.95f; // 生命接近结束时淡出

                // 设定生命周期
                spark.TimeToLive = lifespan;
                spark.FadeOutEnd = lifespan;
                spark.FadeInEnd = fadeTime;
                spark.FadeOutStart = fadeTime;

                // 设定额外亮度
                spark.AdditiveAmount = 0.35f;

                // 添加到粒子系统
                Main.ParticleSystem_World_OverPlayers.Add(spark);
            }
        }

        // 上面这些是CRE的
        // ---------------------------------------------分界线---------------------------------------------
        // 下面这些是CTS的


        // 宇宙贪吃蛇-吃苹果
        public static void Apple_OnKill(Vector2 position)
        {
            float lifespan = 36f; // 粒子存活时间
            float fadeTime = lifespan / 2f; // 淡入淡出过渡时间

            // 颜色池：以苹果为主题的明亮暖色系
            Color[] colorPool = new Color[]
            {
        new Color(255, 100, 80),   // 苹果红
        new Color(255, 200, 120),  // 果肉橙
        new Color(255, 255, 160),  // 柔光黄
        new Color(160, 255, 180)   // 清新绿
            };

            int pointCount = Main.rand.Next(5, 9); // 更密集一些
            for (int i = 0; i < pointCount; i++)
            {
                PrettySparkleParticle spark = _poolPrettySparkle.RequestParticle();

                // ✦ 随机颜色
                spark.ColorTint = colorPool[Main.rand.Next(colorPool.Length)];

                // ✦ 位置设定
                spark.LocalPosition = position;

                // ✦ 旋转角度随机（视觉动态）
                spark.Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;

                // ✦ 放大缩放大小（强烈视觉冲击）
                spark.Scale = new Vector2(
                    Main.rand.NextFloat(4.5f, 6f),     // X方向大
                    Main.rand.NextFloat(0.5f, 1f)      // Y方向细长
                );

                // ✦ 垂直方向随机决定绘制方式（让粒子更不规整）
                spark.DrawVerticalAxis = Main.rand.NextBool();

                // ✦ 动态参数
                spark.FadeInNormalizedTime = 1E-06f;
                spark.FadeOutNormalizedTime = 0.93f;

                spark.TimeToLive = lifespan;
                spark.FadeOutEnd = lifespan;
                spark.FadeInEnd = fadeTime;
                spark.FadeOutStart = fadeTime;

                spark.AdditiveAmount = 0.45f; // 增加发光量

                // ✦ 添加到粒子系统
                Main.ParticleSystem_World_OverPlayers.Add(spark);
            }
        }

        // 幻星-第一次反弹
        public static void Spawn_CelestialBurst(Vector2 center)
        {
            int rays = 6; // 六芒星
            float radius = 48f;

            for (int i = 0; i < rays; i++)
            {
                float angle = MathHelper.TwoPi * i / rays;
                Vector2 dir = angle.ToRotationVector2();

                for (int j = 0; j < 3; j++) // 每条射线布点
                {
                    float distance = (j + 1) * radius / 3f;
                    Vector2 pos = center + dir * distance;

                    PrettySparkleParticle p = _poolPrettySparkle.RequestParticle();

                    // ✦ 设置基本视觉参数
                    p.LocalPosition = pos;
                    p.ColorTint = Main.rand.NextBool() ? Color.Orange : Color.LightBlue;
                    p.Scale = new Vector2(1.3f, 0.8f);
                    p.FadeInNormalizedTime = 0.05f;
                    p.FadeOutNormalizedTime = 0.9f;
                    p.TimeToLive = Main.rand.Next(36, 50);
                    p.FadeOutEnd = p.TimeToLive;
                    p.FadeInEnd = (int)(p.TimeToLive * 0.25f);
                    p.FadeOutStart = (int)(p.TimeToLive * 0.75f);
                    p.AdditiveAmount = 0.55f;

                    // ✦ 浮动运动：在上下 + 左右两个方向上缓慢漂移
                    float waveFreq = Main.rand.NextFloat(0.05f, 0.15f); // 波动频率
                    float waveAmp = Main.rand.NextFloat(0.5f, 1.5f);     // 波动幅度
                    float timeOffset = Main.rand.NextFloat(0, MathHelper.TwoPi); // 每个粒子偏移不同

                    // 绑定更新逻辑，在每帧更新中偏移位置（模拟上下左右浮动）
                    float xWave = (float)Math.Sin(Main.GameUpdateCount * waveFreq + timeOffset) * waveAmp;
                    float yWave = (float)Math.Cos(Main.GameUpdateCount * waveFreq + timeOffset) * waveAmp * 0.3f; // ✅ Y方向大幅减缓

                    p.Velocity = new Vector2(xWave, yWave);


                    Main.ParticleSystem_World_OverPlayers.Add(p);
                }
            }
        }

        // 女妖之爪-飞行
        public static void Spawn_BansheeSoulOrbs(Vector2 center, float rotationAngle)
        {
            float radius = 32f; // 半径为 2 × 16 = 32 像素

            // 粉色光点
            Vector2 pinkOffset = rotationAngle.ToRotationVector2() * radius;
            PrettySparkleParticle pink = _poolPrettySparkle.RequestParticle();
            pink.ColorTint = new Color(1f, 0.5f, 0.9f); // 粉红色
            pink.LocalPosition = center + pinkOffset;
            pink.Rotation = rotationAngle;
            pink.Scale = new Vector2(2f, 1f);
            pink.FadeInNormalizedTime = 5E-06f;
            pink.FadeOutNormalizedTime = 0.95f;
            pink.TimeToLive = 30;
            pink.FadeOutEnd = 30;
            pink.FadeInEnd = 15;
            pink.FadeOutStart = 15;
            pink.AdditiveAmount = 0.45f;
            Main.ParticleSystem_World_OverPlayers.Add(pink);

            // 蓝色光点（相差180度）
            float blueAngle = rotationAngle + MathHelper.Pi;
            Vector2 blueOffset = blueAngle.ToRotationVector2() * radius;
            PrettySparkleParticle blue = _poolPrettySparkle.RequestParticle();
            blue.ColorTint = new Color(0.4f, 0.8f, 1f); // 天蓝色
            blue.LocalPosition = center + blueOffset;
            blue.Rotation = blueAngle;
            blue.Scale = new Vector2(2f, 1f);
            blue.FadeInNormalizedTime = 5E-06f;
            blue.FadeOutNormalizedTime = 0.95f;
            blue.TimeToLive = 30;
            blue.FadeOutEnd = 30;
            blue.FadeInEnd = 15;
            blue.FadeOutStart = 15;
            blue.AdditiveAmount = 0.45f;
            Main.ParticleSystem_World_OverPlayers.Add(blue);
        }

        // 高斯分割刀-命中-能量弹模式、核子风暴-冷却完成、潮汐-死亡
        public static void Spawn_GaussDischargeShards(Vector2 position)
        {
            int pointCount = Main.rand.Next(6, 10); // 数量浮动
            for (int i = 0; i < pointCount; i++)
            {
                PrettySparkleParticle p = _poolPrettySparkle.RequestParticle();

                p.ColorTint = Main.rand.NextBool() ? new Color(180, 240, 255) : new Color(100, 180, 255);
                p.LocalPosition = position + Main.rand.NextVector2Circular(24f, 24f);
                p.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                p.Scale = new Vector2(1.5f, 0.8f) * Main.rand.NextFloat(0.8f, 1.3f);
                p.FadeInNormalizedTime = 0.01f;
                p.FadeOutNormalizedTime = 0.92f;
                p.TimeToLive = Main.rand.Next(28, 42);
                p.FadeInEnd = 10;
                p.FadeOutStart = 20;
                p.FadeOutEnd = p.TimeToLive;
                p.AdditiveAmount = 0.4f;
                Main.ParticleSystem_World_OverPlayers.Add(p);
            }
        }
        // 高斯分割刀-命中-漩涡模式
        public static void Spawn_GaussSingularityPulse(Vector2 position)
        {
            float radius = 32f;
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 2; i++) // 两个对立点
            {
                float angle = baseAngle + i * MathHelper.Pi;
                Vector2 offset = angle.ToRotationVector2() * radius;

                PrettySparkleParticle p = _poolPrettySparkle.RequestParticle();
                p.ColorTint = i == 0 ? new Color(0.6f, 0.8f, 1f) : new Color(1f, 1f, 0.8f);
                p.LocalPosition = position + offset;
                p.Rotation = angle;
                p.Scale = new Vector2(2.2f, 1f);
                p.FadeInNormalizedTime = 0.01f;
                p.FadeOutNormalizedTime = 0.9f;
                p.TimeToLive = 36;
                p.FadeOutEnd = 36;
                p.FadeInEnd = 10;
                p.FadeOutStart = 20;
                p.AdditiveAmount = 0.55f;
                Main.ParticleSystem_World_OverPlayers.Add(p);
            }
        }
        // 元素长枪-星辰
        public static void Spawn_StardustNova_Simple(Vector2 position, Color color, float rotationOffset)
        {
            float angleStep = MathHelper.ToRadians(120f); // 三角形分布
            float angleSpacing = MathHelper.ToRadians(60f); // 每个角的间隔
            float radius = 32f;

            for (int i = 0; i < 3; i++) // 3 个点
            {
                float angle = rotationOffset + i * angleSpacing;
                Vector2 spawnOffset = angle.ToRotationVector2() * radius;

                PrettySparkleParticle particle = _poolPrettySparkle.RequestParticle();
                particle.ColorTint = color;
                particle.LocalPosition = position + spawnOffset;
                particle.Rotation = angle;
                particle.Scale = new Vector2(2f, 0.5f);
                particle.FadeInNormalizedTime = 5E-06f;
                particle.FadeOutNormalizedTime = 0.95f;
                particle.TimeToLive = 30;
                particle.FadeOutEnd = 30;
                particle.FadeInEnd = 15;
                particle.FadeOutStart = 15;
                particle.AdditiveAmount = 0.35f;

                Main.ParticleSystem_World_OverPlayers.Add(particle);
            }
        }




        // 贯星之枪-默认弹幕、恒辉长枪-默认弹幕
        public static void Spawn_SagittariusFlightSpiral(Vector2 center, float angleOffset)
        {
            float radius = 24f;
            float angle = Main.GameUpdateCount * 0.1f + angleOffset; // 单螺旋角度随时间变化
            Vector2 offset = angle.ToRotationVector2() * radius;

            var spark = _poolPrettySparkle.RequestParticle();
            spark.ColorTint = new Color(1f, 1f, 0.6f, 1f); // 柔和金白色
            spark.LocalPosition = center + offset;
            spark.Rotation = angle;
            spark.Scale = new Vector2(1.8f, 0.6f);
            spark.FadeInNormalizedTime = 0.01f;
            spark.FadeOutNormalizedTime = 0.9f;
            spark.TimeToLive = 36;
            spark.FadeOutEnd = 36;
            spark.FadeInEnd = 10;
            spark.FadeOutStart = 20;
            spark.AdditiveAmount = 0.5f;
            Main.ParticleSystem_World_OverPlayers.Add(spark);
        }

        // 贯星之枪-分裂弹幕
        public static void Spawn_SagittariusSpitBirth(Vector2 center)
        {
            int count = 8;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f);

                var spark = _poolPrettySparkle.RequestParticle();
                spark.ColorTint = new Color(1f, 0.9f, 0.5f, 1f); // 偏金黄
                spark.LocalPosition = center;
                spark.Velocity = velocity;
                spark.Scale = new Vector2(1.6f, 0.6f);
                spark.FadeInNormalizedTime = 0.01f;
                spark.FadeOutNormalizedTime = 0.85f;
                spark.TimeToLive = 40;
                spark.FadeOutEnd = 40;
                spark.FadeInEnd = 10;
                spark.FadeOutStart = 20;
                spark.AdditiveAmount = 0.45f;
                Main.ParticleSystem_World_OverPlayers.Add(spark);
            }
        }

        // 贯星之枪-重型蓄力弹幕
        public static void Spawn_SagittariusEchoCharging(Vector2 center)
        {
            int count = Main.rand.Next(2, 4);
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                var spark = _poolPrettySparkle.RequestParticle();
                spark.ColorTint = new Color(1f, 1f, 0.8f, 1f); // 圣光色
                spark.LocalPosition = center;
                spark.Velocity = vel;
                spark.Scale = new Vector2(1.5f, 1.0f);
                spark.FadeInNormalizedTime = 0.01f;
                spark.FadeOutNormalizedTime = 0.9f;
                spark.TimeToLive = 50;
                spark.FadeOutEnd = 50;
                spark.FadeInEnd = 10;
                spark.FadeOutStart = 30;
                spark.AdditiveAmount = 0.55f;
                Main.ParticleSystem_World_OverPlayers.Add(spark);
            }
        }

        // 画龙点睛-冲刺
        public static void Spawn_FlamingPentagonOrbs(Vector2 center, float rotationAngle)
        {
            int points = 5;
            float radius = 120f; // 半径可调
            float angleStep = MathHelper.TwoPi / points;

            for (int i = 0; i < points; i++)
            {
                float angle = rotationAngle + i * angleStep;
                Vector2 offset = angle.ToRotationVector2() * radius;

                PrettySparkleParticle orb = _poolPrettySparkle.RequestParticle();
                orb.ColorTint = new Color(1f, 0.5f, 0f); // 橙色
                orb.LocalPosition = center + offset;
                orb.Rotation = angle;
                orb.Scale = new Vector2(2.5f, 1.2f);
                orb.FadeInNormalizedTime = 0.01f;
                orb.FadeOutNormalizedTime = 0.95f;
                orb.TimeToLive = 30; // 可调
                orb.FadeOutEnd = 30;
                orb.FadeInEnd = 10;
                orb.FadeOutStart = 15;
                orb.AdditiveAmount = 0.45f;

                Main.ParticleSystem_World_OverPlayers.Add(orb);
            }
        }


        // 泰拉巨枪-命中
        public static void Spawn_TerraLanceForestSpirals(Vector2 position, float timeOffset = 0f)
        {
            int spiralCount = 2; // 双螺旋
            int pointsPerSpiral = 8; // 每条螺旋的光点数量
            float baseRadius = 24f; // 平均半径
            float radiusOscillation = 4f; // 半径波动范围

            Color forestGreen = new Color(0.2f, 0.9f, 0.2f, 1f); // 鲜绿色森林光
            Color deepForestGreen = new Color(0.1f, 0.6f, 0.1f, 1f); // 深绿色

            float globalTime = Main.GameUpdateCount * 0.08f + timeOffset;

            for (int spiral = 0; spiral < spiralCount; spiral++)
            {
                float spiralOffset = spiral * MathHelper.Pi; // 双螺旋相差180°

                for (int i = 0; i < pointsPerSpiral; i++)
                {
                    float progress = (float)i / pointsPerSpiral;
                    float angle = globalTime + progress * MathHelper.TwoPi + spiralOffset;
                    float radius = baseRadius + (float)Math.Sin(globalTime + progress * MathHelper.TwoPi) * radiusOscillation;
                    Vector2 offset = angle.ToRotationVector2() * radius;

                    PrettySparkleParticle particle = _poolPrettySparkle.RequestParticle();
                    particle.ColorTint = Main.rand.NextBool(3) ? forestGreen : deepForestGreen;
                    particle.LocalPosition = position + offset;
                    particle.Rotation = angle;
                    particle.Scale = new Vector2(1.6f, 0.6f);
                    particle.FadeInNormalizedTime = 0.01f;
                    particle.FadeOutNormalizedTime = 0.9f;
                    particle.TimeToLive = 40;
                    particle.FadeOutEnd = 40;
                    particle.FadeInEnd = 10;
                    particle.FadeOutStart = 25;
                    particle.AdditiveAmount = 0.5f;
                    Main.ParticleSystem_World_OverPlayers.Add(particle);
                }
            }
        }


        // 恒辉-命中
        public static void Spawn_PlasmaScatter(Vector2 position)
        {
            int count = Main.rand.Next(12, 18);
            for (int i = 0; i < count; i++)
            {
                PrettySparkleParticle p = _poolPrettySparkle.RequestParticle();
                p.ColorTint = Main.rand.NextBool() ? Color.Cyan : Color.White;
                p.LocalPosition = position + Main.rand.NextVector2Circular(40f, 40f);
                p.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                p.Scale = new Vector2(1.8f, 0.8f);
                p.FadeInNormalizedTime = 0.01f;
                p.FadeOutNormalizedTime = 0.9f;
                p.TimeToLive = 40;
                p.FadeOutEnd = 40;
                p.FadeInEnd = 10;
                p.FadeOutStart = 25;
                p.AdditiveAmount = 0.5f;
                Main.ParticleSystem_World_OverPlayers.Add(p);
            }
        }


        // 探针-飞行期间
        public static void Spawn_ParallelPlasmaLines(Vector2 center, Vector2 dirShort, Vector2 dirLong)
        {
            Color[] colors = {
        new Color(200, 255, 255),
        new Color(255, 240, 180),
        new Color(220, 220, 255)
    };

            float shortSpacing = 10f;
            int shortPoints = 3;

            float longSpacing = 12f;
            int longPoints = 5;

            // 1️⃣ 短线（多排，形成收敛“条纹”）
            for (int row = -1; row <= 1; row++)
            {
                for (int i = 0; i < shortPoints; i++)
                {
                    float offsetFactor = i - (shortPoints - 1) / 2f; // -1, 0, 1
                    Vector2 offset = dirShort.RotatedBy(row * 0.08f) * shortSpacing * offsetFactor + dirShort.RotatedBy(MathHelper.PiOver2) * row * 4f;

                    PrettySparkleParticle p = _poolPrettySparkle.RequestParticle();
                    p.ColorTint = colors[Main.rand.Next(colors.Length)];
                    p.LocalPosition = center + offset;
                    p.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    p.Scale = new Vector2(1.5f, 0.6f);
                    p.FadeInNormalizedTime = 0.01f;
                    p.FadeOutNormalizedTime = 0.95f;
                    p.TimeToLive = 36;
                    p.FadeOutEnd = 36;
                    p.FadeInEnd = 10;
                    p.FadeOutStart = 20;
                    p.AdditiveAmount = 0.5f;
                    Main.ParticleSystem_World_OverPlayers.Add(p);
                }
            }

            // 2️⃣ 长线（多排，形成收敛“射线”）
            for (int row = -1; row <= 1; row++)
            {
                for (int i = 0; i < longPoints; i++)
                {
                    float offsetFactor = i - (longPoints - 1) / 2f; // -2, -1, 0, 1, 2
                    Vector2 offset = dirLong.RotatedBy(row * 0.06f) * longSpacing * offsetFactor + dirLong.RotatedBy(MathHelper.PiOver2) * row * 5f;

                    PrettySparkleParticle p = _poolPrettySparkle.RequestParticle();
                    p.ColorTint = colors[Main.rand.Next(colors.Length)];
                    p.LocalPosition = center + offset;
                    p.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    p.Scale = new Vector2(1.8f, 0.7f);
                    p.FadeInNormalizedTime = 0.01f;
                    p.FadeOutNormalizedTime = 0.95f;
                    p.TimeToLive = 40;
                    p.FadeOutEnd = 40;
                    p.FadeInEnd = 12;
                    p.FadeOutStart = 24;
                    p.AdditiveAmount = 0.55f;
                    Main.ParticleSystem_World_OverPlayers.Add(p);
                }
            }
        }



        // 珍珠木-死亡
        public static void Spawn_PinkHolyExplosion(Vector2 position)
        {
            int count = Main.rand.Next(8, 12); // 光点数量
            for (int i = 0; i < count; i++)
            {
                PrettySparkleParticle p = _poolPrettySparkle.RequestParticle();
                p.ColorTint = Main.rand.NextBool() ? new Color(1f, 0.6f, 0.8f, 1f) : new Color(1f, 0.8f, 0.9f, 1f); // 粉红 / 浅粉
                p.LocalPosition = position + Main.rand.NextVector2Circular(32f, 32f); // 爆发范围
                p.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                p.Scale = new Vector2(2f, 1f);
                p.FadeInNormalizedTime = 0.01f;
                p.FadeOutNormalizedTime = 0.9f;
                p.TimeToLive = Main.rand.Next(36, 50);
                p.FadeOutEnd = p.TimeToLive;
                p.FadeInEnd = 12;
                p.FadeOutStart = 24;
                p.AdditiveAmount = 0.5f;
                Main.ParticleSystem_World_OverPlayers.Add(p);
            }
        }

        // 珍珠木-命中
        public static void Spawn_RainbowHolySpirals(Vector2 position)
        {
            Color[] rainbowColors = new Color[]
            {
        Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet
            };

            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            float radius = 24f;

            for (int i = 0; i < rainbowColors.Length; i++)
            {
                float angle = baseAngle + i * MathHelper.TwoPi / rainbowColors.Length;
                Vector2 offset = angle.ToRotationVector2() * radius;

                PrettySparkleParticle p = _poolPrettySparkle.RequestParticle();
                p.ColorTint = rainbowColors[i];
                p.LocalPosition = position + offset;
                p.Rotation = angle;
                p.Scale = new Vector2(1.8f, 0.8f);
                p.FadeInNormalizedTime = 0.01f;
                p.FadeOutNormalizedTime = 0.9f;
                p.TimeToLive = 40;
                p.FadeOutEnd = 40;
                p.FadeInEnd = 12;
                p.FadeOutStart = 24;
                p.AdditiveAmount = 0.5f;
                Main.ParticleSystem_World_OverPlayers.Add(p);
            }
        }

        // 电磁凝胶-命中
        public static void Spawn_StaticElectricSparkle(Vector2 center, float radius = 24f, float rotationOffset = 0f)
        {
            int count = 10; // 严格保持 ≤10 颗光点
            Color[] colors = new Color[]
            {
        new Color(255, 182, 193), // 浅粉红
        new Color(173, 216, 230), // 浅蓝
            };

            float angleStep = MathHelper.TwoPi / count;

            for (int i = 0; i < count; i++)
            {
                float angle = rotationOffset + angleStep * i;
                Vector2 offset = angle.ToRotationVector2() * radius;

                PrettySparkleParticle spark = _poolPrettySparkle.RequestParticle();
                spark.ColorTint = colors[i % colors.Length];
                spark.LocalPosition = center + offset;
                spark.Rotation = angle;
                spark.Scale = new Vector2(1.5f, 0.7f);
                spark.FadeInNormalizedTime = 0.02f;
                spark.FadeOutNormalizedTime = 0.92f;
                spark.TimeToLive = Main.rand.Next(36, 48);
                spark.FadeOutEnd = spark.TimeToLive;
                spark.FadeInEnd = 8;
                spark.FadeOutStart = 24;
                spark.AdditiveAmount = 0.5f;
                Main.ParticleSystem_World_OverPlayers.Add(spark);
            }
        }

        // 初始银枪【单体光点】
        public static void Spawn_SilverSpearGlow(Vector2 position)
        {
            PrettySparkleParticle particle = _poolPrettySparkle.RequestParticle();

            // ✦ 颜色：亮白 + 浅蓝调（不变）
            particle.ColorTint = new Color(0.9f, 0.9f, 1f, 1f);

            // ✦ 稳定偏移：根据位置生成有规律偏移（避免集中堆积）
            float offsetIndex = (position.X + position.Y) * 0.01f;
            float offsetRadius = 10f;
            Vector2 offset = offsetRadius * new Vector2((float)Math.Sin(offsetIndex), (float)Math.Cos(offsetIndex));
            particle.LocalPosition = position + offset;

            // ✦ 稳定旋转（基于坐标或时间）
            particle.Rotation = (position.X * 0.001f + position.Y * 0.002f) % MathHelper.TwoPi;

            // ✦ 规则缩放：在允许范围内使用余弦起伏
            float scaleOsc = (float)Math.Cos(offsetIndex * 1.2f) * 0.5f + 0.5f;
            particle.Scale = new Vector2(
                MathHelper.Lerp(1.5f, 2.0f, scaleOsc),
                MathHelper.Lerp(0.3f, 0.5f, scaleOsc)
            );

            // ✦ 淡入淡出流畅（保留）
            particle.FadeInNormalizedTime = 5E-06f;
            particle.FadeOutNormalizedTime = 0.95f;

            // ✦ 生命周期稳定为 36 帧（不再随机）
            particle.TimeToLive = 36;
            particle.FadeOutEnd = 36;
            particle.FadeInEnd = 10;
            particle.FadeOutStart = 26;

            // ✦ 发光量不变
            particle.AdditiveAmount = 0.4f;

            // ✦ 添加到粒子系统
            Main.ParticleSystem_World_OverPlayers.Add(particle);
        }



        // 上面这些是CTS的
        // ---------------------------------------------分界线---------------------------------------------
        // 下面这些是CX除了CTS部分的




    }
}

