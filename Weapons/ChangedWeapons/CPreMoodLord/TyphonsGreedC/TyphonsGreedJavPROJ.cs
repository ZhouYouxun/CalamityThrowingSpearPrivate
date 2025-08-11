using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TyphonsGreedC
{
    public class TyphonsGreedJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/TyphonsGreedC/TyphonsGreedJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        private int phase = 1; // 1: 飞行模式, 2: 旋转模式
        private bool hasCollided = false; // 是否已经发生过碰撞
        private int timeCounter = 0; // 计时器
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 7; // 允许x次穿透
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            timeCounter++;


            // 三螺旋粒子特效逻辑
            int[] dustTypes = { 108, 31, 14 }; // 粒子类型数组
            float radius = 10f; // 螺旋半径
            float rotationSpeed = 0.1f; // 螺旋旋转速度

            for (int i = 0; i < 3; i++) // 三条螺旋
            {
                float angle = (Projectile.localAI[0] * rotationSpeed + MathHelper.TwoPi / 3 * i) % MathHelper.TwoPi;
                Vector2 offset = angle.ToRotationVector2() * radius;

                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, dustTypes[i], Vector2.Zero, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.5f; // 与弹幕方向相反的漂移效果
            }


            if (phase == 1)
            {
                Projectile.velocity -= Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.03f;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

                if (timeCounter >= 160 || hasCollided)
                {
                    TransitionToSpinMode();
                }
            }
            else if (phase == 2)
            {
                Projectile.rotation += MathHelper.ToRadians(10f);

                if (Main.rand.NextBool(3))
                {
                    CreateSmokeEffect();
                }

                if (timeCounter % 45 == 0)
                {
                    SpawnBubbleProjectile();
                }
            }
        }

        private void TransitionToSpinMode()
        {
            phase = 2;
            timeCounter = 0;
            ReleaseTransitionParticles();
            SpawnInitialBubbles();
        }

        private void SpawnInitialBubbles()
        {
            int bubbleCount = 8;
            float angleStep = MathHelper.TwoPi / bubbleCount;

            for (int i = 0; i < bubbleCount; i++)
            {
                Vector2 direction = Vector2.UnitX.RotatedBy(i * angleStep);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 5f, ModContent.ProjectileType<TyphonsGreedJavBubble2>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }

            SoundEngine.PlaySound(SoundID.Item110, Projectile.Center);
        }

        private void SpawnBubbleProjectile()
        {
            // 随机选择一个360度范围内的角度
            float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);

            // 根据随机角度计算发射方向
            Vector2 bubbleDirection = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

            // 发射子弹
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                bubbleDirection * 5f, // 调整速度大小
                ModContent.ProjectileType<TyphonsGreedJavBubble>(),
                (int)(Projectile.damage * 0.8),
                Projectile.knockBack,
                Projectile.owner
            );
        }


        private void CreateSmokeEffect()
        {
            Color smokeColor = Color.DarkBlue;
            Particle smoke = new HeavySmokeParticle(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 1.5f),
                                                    Projectile.velocity * 0.5f, smokeColor, 30, Projectile.scale * Main.rand.NextFloat(0.3f, 0.75f), 1.0f, MathHelper.ToRadians(2f), true);
            GeneralParticleHandler.SpawnParticle(smoke);
        }

        private void ReleaseTransitionParticles()
        {
            int innerCircleCount = 10; // 内圈粒子数量
            int outerCircleCount = 20; // 外圈粒子数量
            float innerCircleRadius = 10f; // 内圈半径
            float outerCircleRadius = 20f; // 外圈半径

            // 随机颜色数组
            Color[] oceanColors = new Color[]
            {
        Color.LightBlue,
        Color.Cyan,
        Color.DarkBlue,
        Color.DeepSkyBlue,
        Color.SlateBlue
            };

            // 绘制内圈
            for (int i = 0; i < innerCircleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / innerCircleCount;
                Vector2 offset = angle.ToRotationVector2() * innerCircleRadius;
                Vector2 velocity = offset * 0.2f; // 内圈粒子速度较慢
                Color randomColor = oceanColors[Main.rand.Next(oceanColors.Length)]; // 随机颜色

                Particle innerParticle = new HeavySmokeParticle(
                    Projectile.Center + offset, velocity, randomColor,
                    30, 1.0f, 0.8f, MathHelper.ToRadians(2f), true);

                GeneralParticleHandler.SpawnParticle(innerParticle);
            }

            // 绘制外圈
            for (int i = 0; i < outerCircleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / outerCircleCount;
                Vector2 offset = angle.ToRotationVector2() * outerCircleRadius;
                Vector2 velocity = offset * 0.4f; // 外圈粒子速度较快
                Color randomColor = oceanColors[Main.rand.Next(oceanColors.Length)]; // 随机颜色

                Particle outerParticle = new HeavySmokeParticle(
                    Projectile.Center + offset, velocity, randomColor,
                    40, 1.5f, 0.9f, MathHelper.ToRadians(3f), true);

                GeneralParticleHandler.SpawnParticle(outerParticle);
            }
        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!hasCollided)
            {
                hasCollided = true;
                TransitionToSpinMode();
            }

            Projectile.penetrate = Projectile.penetrate - 1;

            if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y;

            SpawnExtraBubblesOnCollide();
            return false;
        }

        private void SpawnExtraBubblesOnCollide()
        {
            int bubbleCount = Main.rand.Next(2, 5);
            for (int i = 0; i < bubbleCount; i++)
            {
                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, randomDirection * 5f, ModContent.ProjectileType<TyphonsGreedJavBubble>(), (int)(Projectile.damage * 0.5), Projectile.knockBack, Projectile.owner);
            }
        }
        public override void OnKill(int timeLeft)
        {
            // 定义Z字形路径的三个关键点
            Vector2 startPoint = Projectile.Center + new Vector2(-20f, -20f); // 左上
            Vector2 middlePoint = Projectile.Center + new Vector2(20f, 0f);   // 中间
            Vector2 endPoint = Projectile.Center + new Vector2(-20f, 20f);    // 左下

            // 绘制从左上到中间的线
            int segmentCount1 = 10; // 线段粒子数量
            for (int i = 0; i <= segmentCount1; i++)
            {
                float lerpFactor = i / (float)segmentCount1;
                Vector2 position = Vector2.Lerp(startPoint, middlePoint, lerpFactor); // 插值计算位置
                Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 1.5f); // 随机扩散速度
                Particle spark = new SparkParticle(position, velocity, false, 60, Main.rand.NextFloat(1f, 1.5f), Color.Cyan);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 绘制从中间到左下的线
            int segmentCount2 = 10; // 线段粒子数量
            for (int i = 0; i <= segmentCount2; i++)
            {
                float lerpFactor = i / (float)segmentCount2;
                Vector2 position = Vector2.Lerp(middlePoint, endPoint, lerpFactor); // 插值计算位置
                Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 1.5f); // 随机扩散速度
                Particle spark = new SparkParticle(position, velocity, false, 60, Main.rand.NextFloat(1f, 1.5f), Color.LightBlue);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }




        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 设置轨迹着色器并加载指定纹理
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));

            // 获取弹幕的纹理
            Texture2D projectileTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            // 克隆弹幕的历史位置数组，用于渲染轨迹
            Vector2[] drawPoints = (Vector2[])Projectile.oldPos.Clone();
            // 根据弹幕的旋转角度计算向前的方向向量
            Vector2 aimAheadDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();

            // 调整轨迹的起点位置，使其与尖端对齐
            drawPoints[0] += aimAheadDirection * -12f;
            drawPoints[1] = drawPoints[0] - (Projectile.rotation + MathHelper.PiOver4).ToRotationVector2() * Vector2.Distance(drawPoints[0], drawPoints[1]);

            // 对历史位置进行平滑处理，保证轨迹更加自然
            for (int i = 0; i < drawPoints.Length; i++)
            {
                drawPoints[i] -= (Projectile.oldRot[i] + MathHelper.PiOver4).ToRotationVector2() * Projectile.height * 0.5f;
            }

            // 当弹幕的历史位置足够多时，渲染轨迹
            if (Projectile.ai[0] > Projectile.oldPos.Length)
            {
                int numPointsRendered = 24; // 需要渲染的轨迹点数量
                PrimitiveRenderer.RenderTrail(
                    drawPoints, // 轨迹的点数据
                    new(
                        PrimitiveWidthFunction,          // 轨迹宽度逻辑
                        PrimitiveColorFunction,         // 轨迹颜色逻辑
                        (_) => Projectile.Size * 0.5f,  // 轨迹大小逻辑
                        shader: GameShaders.Misc["CalamityMod:TrailStreak"], // 使用的轨迹着色器
                        smoothen: true                  // 是否平滑轨迹
                    ),
                    numPointsRendered // 实际渲染的点数量
                );
            }

            // 绘制弹幕本体
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(
                projectileTexture,                // 弹幕的纹理
                drawPosition,                     // 绘制位置
                null,                             // 不使用裁剪矩形
                lightColor,                       // 绘制颜色
                Projectile.rotation,              // 绘制旋转角度
                projectileTexture.Size() * 0.5f,  // 原点居中
                Projectile.scale,                 // 缩放比例
                SpriteEffects.None,               // 不使用镜像效果
                0                                 // 绘制层级
            );

            // 绘制弹幕的拖影效果
            CalamityUtils.DrawAfterimagesCentered(
                Projectile,                          // 当前弹幕
                ProjectileID.Sets.TrailingMode[Projectile.type], // 拖影模式
                lightColor,                          // 拖影颜色
                1                                    // 拖影缩放倍数
            );

            return false; // 禁止游戏自动绘制弹幕
        }

        // 着色器效果 负责绘制旋转的着色器宽度
        internal float PrimitiveWidthFunction(float completionRatio)
        {
            float tipWidthFactor = MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0.01f, 0.04f, completionRatio));
            float bodyWidthFactor = (float)Math.Pow(Utils.GetLerpValue(1f, 0.04f, completionRatio), 0.9D);
            return (float)Math.Pow(tipWidthFactor * bodyWidthFactor, 0.1D) * 40f; // 调整宽度为40
        }

        // 着色器颜色逻辑，调整为深海风格
        internal Color PrimitiveColorFunction(float completionRatio)
        {
            float fadeInterpolant = (float)Math.Cos(Main.GlobalTimeWrappedHourly * -9f + completionRatio * 6f + Projectile.identity * 2f) * 0.5f + 0.5f;

            // 动态深海蓝紫色渐变
            fadeInterpolant = MathHelper.Lerp(0.2f, 0.8f, fadeInterpolant);
            Color frontFade = Color.Lerp(Color.Cyan, Color.DarkBlue, fadeInterpolant);
            frontFade = Color.Lerp(frontFade, Color.SlateBlue, 0.5f); // 更深色的蓝紫渐变
            Color backFade = Color.DeepSkyBlue;

            return Color.Lerp(frontFade, backFade, (float)Math.Pow(completionRatio, 1.2D)) * (float)Math.Pow(1f - completionRatio, 1.1D) * Projectile.Opacity;
        }

   
    }
}
