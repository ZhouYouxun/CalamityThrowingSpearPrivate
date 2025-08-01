using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Sounds;
using Terraria.Audio;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
{
    public class SunEssenceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SunEssenceJav/SunEssenceJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        private bool isSpinning = false; // 标记是否进入高速旋转模式
        private int spinDuration = 90; // 高速旋转持续时间

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        // 用于记录可控的 SparkParticle 粒子（用于轨迹操控）
        private readonly List<SparkParticle> ownedSparkParticles = new();

        public override bool PreDraw(ref Color lightColor)
        {
            // 如果未进入高速旋转模式，保持原状
            if (!isSpinning)
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
                return false;
            }

            // 获取纹理资源和位置
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 背光效果部分 - 亮白色光晕
            float chargeOffset = 3f; // 控制充能效果扩散的偏移量
            float spinProgress = MathHelper.Clamp((90 - spinDuration) / 60f, 0f, 1f); // 线性增强过程，持续60帧
            Color chargeColor = Color.White * (0.6f * spinProgress); // 根据进度调整透明度
            chargeColor.A = 0; // 设置透明度

            // 修复旋转逻辑，确保与速度方向同步
            float rotation = Projectile.rotation;
            SpriteEffects direction = SpriteEffects.None;

            // 绘制充能效果 - 圆周上绘制多个充能光效
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
            }

            // 渲染实际的投射物本体
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 7;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // 进入旋转模式前正常飞行
            if (!isSpinning)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

                // 生成飞行期间复杂太阳能量混合特效
                if (Main.rand.NextFloat() < 0.4f) // 保持触发率保证持续性
                {
                    // === 🚩 1️⃣ 十字星（缩小但翻倍数量，带轻微偏移） ===
                    for (int i = 0; i < 2; i++) // 翻倍数量
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(6f, 6f); // 轻微偏移
                        Color startColor = new Color(255, 250, 200, 80);
                        Color endColor = new Color(255, 230, 150, 60);

                        GenericSparkle sparkle = new GenericSparkle(
                            Projectile.Center + offset,
                            Vector2.Zero,
                            startColor,
                            endColor,
                            Main.rand.NextFloat(1.2f, 1.6f), // ✂️大小砍半
                            18,
                            Main.rand.NextFloat(-0.02f, 0.02f),
                            2.8f
                        );
                        GeneralParticleHandler.SpawnParticle(sparkle);
                    }

                    // === 🚩 2️⃣ SparkParticle 狂野化，生成更多、更快、范围更大 ===
                    int sparkCount = 6; // 原本约 2，直接 ×3
                    for (int i = 0; i < sparkCount; i++)
                    {
                        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(Main.rand.NextFloat(-1.2f, 1.2f));
                        Vector2 sparkVelocity = direction * Main.rand.NextFloat(6f, 14f); // 速度 ×2-3
                        Color sparkColor = Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat(0.2f, 0.8f));

                        Particle spark = new SparkParticle(
                            Projectile.Center + direction * Main.rand.NextFloat(10f, 30f), // 初始位置随机外扩
                            sparkVelocity,
                            false,
                            Main.rand.Next(15, 30),
                            Main.rand.NextFloat(0.5f, 1.0f), // 放大体积
                            sparkColor * 0.9f
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

                    //// === 🚩 3️⃣ Dust 狂野化，生成更多、更远、更亮 ===【更有序的太阳粒子轨迹环绕（围绕中心）】
                    //int dustCount = 14; // 强调节奏
                    //float baseRadius = 8f;
                    //float rotationOffset = Main.GameUpdateCount * 0.15f; // 让它缓慢旋转

                    //for (int i = 0; i < dustCount; i++)
                    //{
                    //    float angle = MathHelper.TwoPi * i / dustCount + rotationOffset;
                    //    Vector2 direction = angle.ToRotationVector2();

                    //    Vector2 spawnPos = Projectile.Center + direction * baseRadius;
                    //    Vector2 velocity = direction * Main.rand.NextFloat(1.5f, 3f); // 温和速度

                    //    Dust dust = Dust.NewDustPerfect(
                    //        spawnPos,
                    //        DustID.SolarFlare,
                    //        velocity,
                    //        100,
                    //        Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat(0.3f, 0.7f)),
                    //        Main.rand.NextFloat(1.1f, 1.6f)
                    //    );
                    //    dust.noGravity = true;
                    //    dust.fadeIn = Main.rand.NextFloat(0.8f, 1.1f);
                    //}

                }
                {
                    // 初始化计数器
                    Projectile.ai[1]++; // 用于记录持续帧数
                    float t = MathHelper.Clamp(Projectile.ai[1] / 30f, 0f, 1f); // 从0到1线性收束【 / ？代表着每多少帧调整一度】

                    // 计算当前的偏移角度（最大为60度，逐帧收束到0）
                    float maxAngle = MathHelper.ToRadians(60f);
                    float offsetAngle = maxAngle * (1f - t); // 线性收敛角度

                    // 两个方向：后方左右偏角
                    Vector2 baseDir = -Projectile.velocity.SafeNormalize(Vector2.UnitY); // 反向方向
                    Vector2 dirLeft = baseDir.RotatedBy(-offsetAngle);
                    Vector2 dirRight = baseDir.RotatedBy(offsetAngle);

                    // 可调粒子速度
                    float speed = Main.rand.NextFloat(1.5f, 3f);

                    // 可调大小
                    float scale = Main.rand.NextFloat(1.1f, 1.6f);

                    // 可调颜色
                    Color dustColor = Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat(0.3f, 0.7f));

                    // 左粒子
                    Dust dustLeft = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.SolarFlare,
                        dirLeft * speed,
                        100,
                        dustColor,
                        scale
                    );
                    dustLeft.noGravity = true;
                    dustLeft.fadeIn = Main.rand.NextFloat(0.8f, 1.1f);

                    // 右粒子
                    Dust dustRight = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.SolarFlare,
                        dirRight * speed,
                        100,
                        dustColor,
                        scale
                    );
                    dustRight.noGravity = true;
                    dustRight.fadeIn = Main.rand.NextFloat(0.8f, 1.1f);

                }





            }
            else
            {
                // 高速旋转逻辑
                Projectile.rotation += 0.45f;

                // 获得追踪能力
                NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 24f, 0.08f); // 追踪速度非常快，因为是直接粘在别人身上
                }

                // 定义旋转速度（每帧旋转的角度）
                float rotationSpeed = MathHelper.ToRadians(7f); // 每帧旋转 7 度
                Projectile.ai[0] += rotationSpeed;

                // 获取两个相反方向的基础角度
                float baseAngle1 = Projectile.ai[0];
                float baseAngle2 = baseAngle1 + MathHelper.Pi; // 相差180度

                // 生成两个方向的粒子特效
                {
                    // === 1️⃣ 黄金螺旋 Bloom 粒子（替代原对称发射）===
                    int bloomCount = 2; // 粒子数量稍降
                    float goldenAngle = MathHelper.ToRadians(137.5f); // 黄金角
                    float baseAngle = Projectile.ai[0] * goldenAngle; // 利用 ai[0] 叠加旋转

                    for (int i = 0; i < bloomCount; i++)
                    {
                        float angle = baseAngle + i * goldenAngle;
                        Vector2 dir = angle.ToRotationVector2();

                        Vector2 pos = Projectile.Center + dir * Main.rand.NextFloat(4f, 12f); // 轻微偏移
                        Vector2 vel = dir * Main.rand.NextFloat(8.4f, 18.4f); // 速

                        Color color = Color.White * 0.82f; // 亮度
                        float scale = Main.rand.NextFloat(0.18f, 0.26f); // 缩小

                        GeneralParticleHandler.SpawnParticle(new GenericBloom(
                            pos,
                            vel,
                            color,
                            scale,
                            Main.rand.Next(16, 28)
                        ));
                    }

                    // === 2️⃣ Dust 环绕效果（从半径扩大后的大圆环触发）===
                    int dustCount = 3;
                    float dustRadius = 3f * 16f; // 半径扩大为 3 格（48像素）

                    for (int k = 0; k < dustCount; k++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 direction = angle.ToRotationVector2();
                        Vector2 dustPos = Projectile.Center + direction * dustRadius;
                        Vector2 dustVel = direction * Main.rand.NextFloat(2f, 5f);

                        Dust dust = Dust.NewDustPerfect(
                            dustPos,
                            Main.rand.NextBool() ? DustID.SolarFlare : DustID.Torch,
                            dustVel,
                            0,
                            Main.rand.NextBool() ? Color.White : Color.Yellow,
                            Main.rand.NextFloat(1.2f, 2f)
                        );
                        dust.noGravity = true;
                        dust.fadeIn = Main.rand.NextFloat(0.8f, 1.2f);
                    }

                    // === 3️⃣ 改良 SparkParticle 放射线（加入偏移 + 蛇形轨迹）===
                    int sparkCount = 1;
                    float baseSparkAngle = Projectile.ai[1] * MathHelper.ToRadians(15f); // 每次旋转 ? 度
                    float sparkRadius = 16f; // 起始位置圆环半径

                    for (int s = 0; s < sparkCount; s++)
                    {
                        // 每个粒子在基础角度上加上少量随机扰动，避免完全对称
                        float randomOffset = Main.rand.NextFloat(-MathHelper.Pi / 24f, MathHelper.Pi / 24f); // -7.5° ~ +7.5°
                        float angle = baseSparkAngle + MathHelper.TwoPi * s / sparkCount + randomOffset;
                        Vector2 dir = angle.ToRotationVector2();

                        Vector2 spawnPos = Projectile.Center + dir * sparkRadius;
                        Vector2 sparkVel = dir * Main.rand.NextFloat(7f, 19f);

                        Color sparkColor = Main.rand.NextBool() ? Color.White : Color.Yellow;

                        Particle spark = new SparkParticle(
                            spawnPos,
                            sparkVel,
                            false,
                            Main.rand.Next(25, 40),
                            Main.rand.NextFloat(0.8f, 1.4f),
                            sparkColor * 0.8f
                        );
                        GeneralParticleHandler.SpawnParticle(spark);

                        // 记录引用
                        ownedSparkParticles.Add((SparkParticle)spark);
                    }


                    // 修改已生成的 SparkParticle 飞行轨迹（持续左拐）
                    for (int i = ownedSparkParticles.Count - 1; i >= 0; i--)
                    {
                        SparkParticle p = ownedSparkParticles[i];

                        if (p.Time >= p.Lifetime)
                        {
                            ownedSparkParticles.RemoveAt(i);
                            continue;
                        }

                        // 🚩让每颗粒子持续左拐（每帧 -2°）
                        float rotateAmount = MathHelper.ToRadians(2f);
                        p.Velocity = p.Velocity.RotatedBy(-rotateAmount);

                        // （可选）加入轻微位置偏移，模拟火焰扩张感
                        // p.Position += Main.rand.NextVector2Circular(0.1f, 0.1f);
                    }



                    // 每帧推进角度
                    Projectile.ai[1] += 1f;

                    // 每帧推进旋转角度
                    Projectile.ai[0] += 1f; // 黄金螺旋计数器
                    Projectile.ai[1] += 1f; // Spark 旋转角计数器


                }

                // 旋转攻击结束判断
                if (--spinDuration <= 0)
                {
                    Projectile.Kill(); // 在结束时触发 OnKill
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 300); // 原版的破晓效果

            if (!isSpinning)
            {
                isSpinning = true;
                Projectile.velocity = Vector2.Zero; // 停止移动
                Projectile.timeLeft = spinDuration + 15; // 保证旋转期间不消失
            }
            else
            {
                // 旋转期间每次造成伤害时召唤羽毛
                //int featherCount = Main.dayTime ? 3 : 1; // 白天时获得强化
                int featherCount = 3; // 不再变得强化，而是固定三个
                for (int i = 0; i < featherCount; i++)
                {
                    // 在主弹幕正上方50个方块的位置，以该点为圆心，半径15个方块的范围内随机生成
                    Vector2 featherSpawnPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-15f, 15f) * 16f, -50f * 16f);

                    // 计算羽毛向主弹幕位置的速度向量
                    Vector2 featherVelocity = (Projectile.Center - featherSpawnPosition).SafeNormalize(Vector2.UnitY) * 45f;

                    // 生成羽毛弹幕
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), featherSpawnPosition, featherVelocity, ModContent.ProjectileType<SunEssenceJavFeather>(), (int)(Projectile.damage * 0.6f), 0, Projectile.owner);
                }

            }
        }


        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item90, Projectile.Center);

            // 生成随机数量（2~3个）的 SunEssenceJavLightPoint 弹幕
            //int lightPointCount = Main.rand.Next(2, 4); // 2~3 个弹幕
            //for (int i = 0; i < lightPointCount; i++)
            //{
            //    // 在绝对正上方 ±45 度范围内随机选择角度
            //    float randomAngle = MathHelper.ToRadians(Main.rand.Next(-45, 46)); // -45到45度
            //    Vector2 velocity = (Vector2.UnitY.RotatedBy(randomAngle) * -1f) * Main.rand.NextFloat(4f, 8f) * 0.75f;

            //    // 发射 SunEssenceJavLightPoint 弹幕
            //    Projectile.NewProjectile(
            //        Projectile.GetSource_FromThis(),
            //        Projectile.Center,
            //        velocity,
            //        ModContent.ProjectileType<SunEssenceJavLightPoint>(),
            //        (int)(Projectile.damage * 1.0f), // 伤害倍率为 1.0
            //        Projectile.knockBack,
            //        Projectile.owner
            //    );
            //}

            // 播放爆炸特效
            Particle blastRing = new CustomPulse(
                Projectile.Center,
                Vector2.Zero,
                Color.Gold, // 亮黄色
                "CalamityThrowingSpear/Texture/ThebigExplosion1",
                Vector2.One * 0.33f,
                Main.rand.NextFloat(-10f, 10f),
                0.078f,
                0.450f,
                30
            );
            GeneralParticleHandler.SpawnParticle(blastRing);

            CreateSunParticleEffect();
        }





        // 生成太阳形状的粒子特效
        // 🚩 替换原 CreateSunParticleEffect，生成真正太阳爆发特效
        private void CreateSunParticleEffect()
        {
            int particleCount = 50; // 大幅增加，形成完整圆周和内部充实感
            float radius = 120f; // 半径扩大 4 倍

            Vector2 center = Projectile.Center;

            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 direction = angle.ToRotationVector2();

                // === 🚩 Dust (太阳流动) ===
                Vector2 dustPos = center + direction * radius * Main.rand.NextFloat(0.8f, 1.2f);
                Vector2 dustVel = direction.RotatedByRandom(MathHelper.ToRadians(20f)) * Main.rand.NextFloat(2f, 6f);
                Dust dust = Dust.NewDustPerfect(
                    dustPos,
                    Main.rand.NextBool() ? DustID.SolarFlare : DustID.Torch,
                    dustVel,
                    0,
                    Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat(0.3f, 0.8f)),
                    Main.rand.NextFloat(1.5f, 2.8f)
                );
                dust.noGravity = true;
                dust.fadeIn = Main.rand.NextFloat(1f, 1.6f);

                // === 🚩 SparkParticle (线性日冕射线) ===
                if (i % 5 == 0) // 每 5 个角度生成一次，避免过量
                {
                    Vector2 sparkVel = direction * Main.rand.NextFloat(8f, 16f);
                    Color sparkColor = Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat(0.4f, 0.7f));
                    Particle spark = new SparkParticle(
                        center + direction * Main.rand.NextFloat(20f, 40f),
                        sparkVel,
                        false,
                        Main.rand.Next(30, 50),
                        Main.rand.NextFloat(1.0f, 1.8f),
                        sparkColor * 0.9f
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // === 🚩 HeavySmokeParticle (轻型太阳冠烟雾) ===
                if (i % 3 == 0) // 每 3 个角度生成一次
                {
                    Particle smoke = new HeavySmokeParticle(
                        center + direction * Main.rand.NextFloat(30f, 80f),
                        direction * Main.rand.NextFloat(0.5f, 2f),
                        Color.Lerp(Color.Orange, Color.WhiteSmoke, Main.rand.NextFloat(0.3f, 0.7f)),
                        Main.rand.Next(20, 36),
                        Main.rand.NextFloat(1.2f, 2.0f),
                        0.4f,
                        Main.rand.NextFloat(-0.05f, 0.05f),
                        false
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
            }
        }


        // 创建粒子特效
        private void CreateParticleEffect()
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, velocity, Scale: 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
                dust.color = Color.Orange;
            }
        }

   


    }
}

