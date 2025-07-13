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
using Terraria.Audio;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityRangerExpansion.LightingBolts;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.MiracleMatterJav
{
    public class MiracleMatterJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/MiracleMatterJav/MiracleMatterJav";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4; // 允许8次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 20; // 无敌帧冷却时间为14帧
        }
        // 在类中添加一个累积变量，用于存储螺旋角度
        private float spiralAngle = 0f;
        public override void AI()
        {

            Vector2 armPosition = Projectile.Center;
            Vector2 tipPosition = armPosition + Projectile.velocity * Projectile.width * 0.45f;

            // 发光效果
            Color energyColor = Color.Orange;
            Vector2 verticalOffset = Vector2.UnitY.RotatedBy(Projectile.rotation) * 8f;
            if (Math.Cos(Projectile.rotation) < 0f)
                verticalOffset *= -1f;

            // 飞行中的特效逻辑
            EmitSpiralParticles();

            // 发射橙色光粒子
            //if (Main.rand.NextBool(1))
            //{
            //    // 使用默认的生成位置，不进行偏移
            //    SquishyLightParticle exoEnergy = new(tipPosition, -Vector2.UnitY.RotatedByRandom(0.39f) * Main.rand.NextFloat(0.4f, 1.6f), 0.28f, energyColor, 25);
            //    GeneralParticleHandler.SpawnParticle(exoEnergy);
            //}


            // 增加透明度渐变
            Projectile.Opacity = Utils.GetLerpValue(0f, 3f, Projectile.timeLeft, true);

            // 添加光照
            DelegateMethods.v3_1 = energyColor.ToVector3();
            Utils.PlotTileLine(tipPosition - verticalOffset, tipPosition + verticalOffset, 10f, DelegateMethods.CastLightOpen);
            Lighting.AddLight(tipPosition, energyColor.ToVector3());

            {
                // MiracleMatterJav 高科技飞行有序特效

                float time = Main.GameUpdateCount * 0.1f;
                float radius = 12f;
                int points = 5; // 五点环形

                for (int i = 0; i < points; i++)
                {
                    float angle = time + MathHelper.TwoPi / points * i;
                    Vector2 offset = angle.ToRotationVector2() * radius * Main.rand.NextFloat(0.9f, 1.1f);
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center + offset,
                        DustID.Electric,
                        -Projectile.velocity * 0.05f,
                        150,
                        Color.Cyan,
                        Main.rand.NextFloat(0.6f, 1.0f)
                    );
                    dust.noGravity = true;
                }

                // 外层更大半径三点柔和环
                if (Main.GameUpdateCount % 5 == 0)
                {
                    float outerRadius = 24f;
                    int outerPoints = 3;
                    for (int i = 0; i < outerPoints; i++)
                    {
                        float angle = -time * 0.8f + MathHelper.TwoPi / outerPoints * i;
                        Vector2 offset = angle.ToRotationVector2() * outerRadius;
                        Dust dust = Dust.NewDustPerfect(
                            Projectile.Center + offset,
                            DustID.BlueCrystalShard,
                            Vector2.Zero,
                            100,
                            Color.LightBlue,
                            Main.rand.NextFloat(0.8f, 1.2f)
                        );
                        dust.noGravity = true;
                    }
                }

                // 在飞行期间稳定维持有序科技感螺旋光点
                CTSLightingBoltsSystem.Spawn_SagittariusFlightSpiral(Projectile.Center, Main.GameUpdateCount * 0.3f);
            }






            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 25;
            }
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
            }
            if (Projectile.ai[0] == 0f)
            {
                Projectile.localAI[1] += 1f;
                if (Projectile.localAI[1] >= 60f)
                {
                    Projectile.velocity.X *= 0.99f;
                    Projectile.velocity.Y += 0.3f;

                    if (Projectile.velocity.Y > 16f)
                        Projectile.velocity.Y = 16f;
                }
            }

            if (Main.player[Projectile.owner].controlUseTile) // 检测玩家是否按下右键
            {
                if (Projectile.ai[0] == 2f) // 只有扎入的弹幕才能被右键移除
                {
                    Projectile.Kill(); // 直接销毁弹幕
                }
            }

            int dustType = 171;
            if (Main.rand.NextBool(3))
            {
                dustType = 46;
            }
            if (Main.rand.NextBool(9))
            {
                //Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }
            if (Projectile.ai[0] == 0f)
            {
                Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
                Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
                Projectile.rotation += Projectile.spriteDirection * MathHelper.ToRadians(45f);
            }
            //Sticky Behaviour
            Projectile.StickyProjAI(15);
            if (Projectile.ai[0] == 2f)
            {
                Projectile.velocity *= 0f;
            }
        }

        private void EmitSpiralParticles()
        {
            // 每次生成粒子时，偏转角度
            float spiralStep = 0.1f; // 每次偏转的角度（弧度）
            spiralAngle += spiralStep;

            // 发射橙色光粒子
            if (Main.rand.NextBool(1))
            {
                // 计算偏转后的粒子生成位置
                Vector2 spiralOffset = new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * 10f; // 半径为10像素
                Vector2 spawnPosition = Projectile.Center + spiralOffset;

                // 创建粒子
                SquishyLightParticle exoEnergy = new(
                    spawnPosition,
                    -Vector2.UnitY.RotatedByRandom(0.39f) * Main.rand.NextFloat(0.4f, 1.6f),
                    0.28f,
                    Color.Orange, // 原来的粒子颜色
                    25
                );
                GeneralParticleHandler.SpawnParticle(exoEnergy);
            }

            // 防止角度过大，进行归一化
            if (spiralAngle > MathHelper.TwoPi)
                spiralAngle -= MathHelper.TwoPi;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 调用原始的ModifyHitNPCSticky方法，确保粘附逻辑正常
            Projectile.ModifyHitNPCSticky(20);
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);

            // 随机生成四种颜色的sparkColor
            Color sparkColor = Main.rand.Next(4) switch
            {
                0 => Color.Red,
                1 => Color.MediumTurquoise,
                2 => Color.Orange,
                _ => Color.LawnGreen,
            };

            // 从本体弹幕的周围随机360度发射DirectionalPulseRing粒子
            for (int i = 0; i < 2; i++)  // 每次生成两个粒子
            {
                // 生成随机角度
                float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                Vector2 direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

                // 将速度放大3~4倍 (选择倍数范围内的随机值)
                direction *= Main.rand.NextFloat(3f, 4f) * 0.8f; // 保持原先的粒子速度比例

                // 创建并发射DirectionalPulseRing粒子
                DirectionalPulseRing pulse = new DirectionalPulseRing(Projectile.Center, direction, sparkColor, new Vector2(1, 1), 0, Main.rand.NextFloat(0.2f, 0.35f), 0f, 40);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0] = 2f;
            Projectile.timeLeft = 900;
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/New/恒辉之矛音效") with { Volume = 1.0f, Pitch = 0.0f }, Projectile.Center);


            // 0. 随机选择2~4个角度发射2~4个 MiracleMatterJavLight 弹幕
            int numProjectiles = Main.rand.Next(4, 7);  // 随机选择发射4到6个弹幕
            for (int i = 0; i < numProjectiles; i++)
            {
                float angle = MathHelper.ToRadians(Main.rand.Next(0, 360));  // 随机角度
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f;  // 设置速度
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<MiracleMatterJavLight>(), (int)(Projectile.damage * 1.2f), Projectile.knockBack, Main.myPlayer);
            }


            {
                // 定义随机三角朝向
                float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float[] triangleOffsets = { 0f, MathHelper.TwoPi / 3f, MathHelper.TwoPi * 2f / 3f };

                for (int i = 0; i < 30; i++)
                {
                    float triAngle = baseAngle + triangleOffsets[i % 3] + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 dir = triAngle.ToRotationVector2() * Main.rand.NextFloat(10f, 20f);
                    Particle smoke = new HeavySmokeParticle(
                        Projectile.Center + dir,
                        dir * 0.3f,
                        new Color(140, 220, 255) * 0.8f,
                        Main.rand.Next(25, 36),
                        Main.rand.NextFloat(1.0f, 2.5f),
                        0.4f,
                        Main.rand.NextFloat(-0.05f, 0.05f),
                        required: true
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }

                CTSLightingBoltsSystem.Spawn_PlasmaScatter(Projectile.Center);

            }



            {
                // 1. 生成较小的橙黄色和淡黄色爆炸特效（超新星的那个光圈逐渐缩小的特效）
                Vector2 spawnPosition = Projectile.Center;
                Color lightYellowColor = Color.LightYellow;
                float smallerScale = 1.5f; // 较小的扩散大小
                float rotationSpeed = Main.rand.NextFloat(-10f, 10f); // 随机旋转速度
                                                                      // 创建爆炸粒子，颜色为X色
                Particle yellowExplosion = new CustomPulse(spawnPosition, Vector2.Zero, lightYellowColor, "CalamityMod/Particles/LargeBloom", new Vector2(0.5f, 0.5f), -rotationSpeed, smallerScale, smallerScale - 0.5f, 15);
                GeneralParticleHandler.SpawnParticle(yellowExplosion);

                // 2. 发射25个大小和速度差异明显的线性粒子特效
                for (int i = 0; i < 25; i++)
                {
                    // 粒子生成位置为弹幕中心，带有小范围随机偏移
                    Vector2 spawnPosition2 = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);

                    // 随机生成粒子的速度和方向
                    Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(0.5f, 3f);

                    // 粒子大小和颜色
                    float trailScale = Main.rand.NextFloat(0.5f, 1.5f);
                    Color trailColor = Color.Lerp(Color.White, Color.Red, Main.rand.NextFloat());

                    // 创建并生成粒子
                    Particle trail = new SparkParticle(spawnPosition2, velocity, false, 60, trailScale, trailColor);
                    GeneralParticleHandler.SpawnParticle(trail);
                }
                {
                    // 🌐 高科技有序图形整体随机朝向
                    float baseRotation = Main.rand.NextFloat(MathHelper.TwoPi);

                    // 1️⃣ 六芒星 Dust 阵列
                    int vertices = 6; // 六芒星顶点数
                    int particlesPerEdge = 12;
                    float radius = 75f;

                    for (int edge = 0; edge < vertices; edge++)
                    {
                        float currentAngle = MathHelper.TwoPi / vertices * edge + baseRotation;
                        float nextAngle = MathHelper.TwoPi / vertices * ((edge + 1) % vertices) + baseRotation;

                        Vector2 startPoint = currentAngle.ToRotationVector2() * radius;
                        Vector2 endPoint = nextAngle.ToRotationVector2() * radius;

                        for (int i = 0; i <= particlesPerEdge; i++)
                        {
                            float progress = i / (float)particlesPerEdge;
                            Vector2 position = Vector2.Lerp(startPoint, endPoint, progress);
                            Vector2 velocity = position.SafeNormalize(Vector2.Zero) * 5f;

                            Dust magic = Dust.NewDustPerfect(Projectile.Center + position, 267, velocity);
                            magic.scale = 2.2f;
                            magic.fadeIn = 0.7f;
                            magic.color = CalamityUtils.MulticolorLerp(progress, CalamityUtils.ExoPalette);
                            magic.noGravity = true;

                            if (i % (particlesPerEdge / 3) == 0)
                            {
                                Vector2 extraVelocity = velocity.RotatedBy(MathHelper.PiOver4) * 0.5f;
                                Dust extraMagic = Dust.NewDustPerfect(Projectile.Center + position, 267, extraVelocity);
                                extraMagic.scale = 1.5f;
                                extraMagic.fadeIn = 0.3f;
                                extraMagic.color = magic.color * 0.8f;
                                extraMagic.noGravity = true;
                            }
                        }
                    }

                    // 2️⃣ 双椭圆 Dust 阵列
                    int ellipseParticleCount = 100;
                    float longAxisLength = 150f; // 75 * 2
                    float shortAxisLength = 75f;

                    float ellipseRotation1 = baseRotation + MathHelper.PiOver4; // 45°相对于整体旋转
                    float ellipseRotation2 = baseRotation - MathHelper.PiOver4; // -45°

                    for (int i = 0; i < ellipseParticleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / ellipseParticleCount;

                        // 第一个椭圆
                        Vector2 ellipse1Offset = new Vector2(
                            (float)Math.Cos(angle) * longAxisLength,
                            (float)Math.Sin(angle) * shortAxisLength
                        ).RotatedBy(ellipseRotation1);

                        Vector2 ellipse1Velocity = ellipse1Offset * 0.02f;
                        Dust ellipse1Dust = Dust.NewDustPerfect(Projectile.Center + ellipse1Offset, 267, ellipse1Velocity);
                        ellipse1Dust.scale = Main.rand.NextFloat(1.5f, 2f);
                        ellipse1Dust.fadeIn = 0.5f;
                        ellipse1Dust.color = CalamityUtils.MulticolorLerp(i / (float)ellipseParticleCount, CalamityUtils.ExoPalette);
                        ellipse1Dust.noGravity = true;

                        // 第二个椭圆
                        Vector2 ellipse2Offset = new Vector2(
                            (float)Math.Cos(angle) * longAxisLength,
                            (float)Math.Sin(angle) * shortAxisLength
                        ).RotatedBy(ellipseRotation2);

                        Vector2 ellipse2Velocity = ellipse2Offset * 0.02f;
                        Dust ellipse2Dust = Dust.NewDustPerfect(Projectile.Center + ellipse2Offset, 267, ellipse2Velocity);
                        ellipse2Dust.scale = Main.rand.NextFloat(1.5f, 2f);
                        ellipse2Dust.fadeIn = 0.5f;
                        ellipse2Dust.color = CalamityUtils.MulticolorLerp(i / (float)ellipseParticleCount, CalamityUtils.ExoPalette);
                        ellipse2Dust.noGravity = true;
                    }
                }



            }

        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300); // 超位崩解
            SoundEngine.PlaySound(SoundID.Item132.WithVolumeScale(2.5f), Projectile.Center);
        }


    }
}
