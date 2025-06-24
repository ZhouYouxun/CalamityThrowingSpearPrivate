using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Renderers;
using Terraria.Graphics.Shaders;
using Terraria.GameContent;

namespace CalamityRangerExpansion.LightingBolts
{
    internal class LightingBoltsSystem
    {
        private static ParticlePool<PrettySparkleParticle> _poolPrettySparkle = new ParticlePool<PrettySparkleParticle>(200, () => new PrettySparkleParticle());

        // 原版内置了很多很多的特效【它们可以通过 ParticleOrchestrator.RequestParticleSpawn() 进行调用】，你可以直接检查相关文件来获得相关信息
        // 可以看一下这一段：https://gist.github.com/Rijam/971b5252707860b65b582093580aa49c
        // 想去学习的话可以去这里：namespace Terraria.GameContent.Drawing; public class ParticleOrchestrator
        // 这里面的各种光点和光学效果都是参考了上面这些的，更加倾向于那些魔法，能量，生物，环境，反射相关的特效
        // 注意！！注意！！
        // 本文件里的所有特效均不需要灾厄本体运行，完全独立，因为这些特效是基于原版1.4.4.9的

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



        public static void Spawn_AstralSoulLightsB(Vector2 position)
        {
            float lifespan = 36f; // 生命时长
            float fadeTime = lifespan / 2f; // 过渡时间

            // 颜色池，每个光点独立选择其中一个颜色
            Color[] colorPool = new Color[]
            {
        new Color(255, 164, 94),  // #FFA45E 橙黄色
        new Color(66, 189, 181),  // #42BDB5 亮青色
        new Color(109, 242, 196), // #6DF2C4 薄荷绿色
        new Color(237, 93, 83)    // #ED5D53 深珊瑚红
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

                // 旋转角度随机
                spark.Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;

                // **修改 Scale，使其更接近 TrueExcalibur**
                spark.Scale = new Vector2(5f, 0.5f) * (0.8f + Main.rand.NextFloat() * 0.4f);

                // **确保绘制方式一致**
                spark.DrawVerticalAxis = false;

                // 设定淡入淡出时机
                spark.FadeInNormalizedTime = 5E-06f; // 快速淡入
                spark.FadeOutNormalizedTime = 0.95f; // 柔和消失

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




    }
}
