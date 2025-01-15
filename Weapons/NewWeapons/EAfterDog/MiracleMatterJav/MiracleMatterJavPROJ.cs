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
            Projectile.penetrate = 8; // 允许8次伤害
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
            Projectile.timeLeft = 300;
            return false;
        }

        public override void OnKill(int timeLeft)
        {

            // 0. 随机选择2~4个角度发射2~4个 MiracleMatterJavLight 弹幕
            int numProjectiles = Main.rand.Next(4, 7);  // 随机选择发射4到6个弹幕
            for (int i = 0; i < numProjectiles; i++)
            {
                float angle = MathHelper.ToRadians(Main.rand.Next(0, 360));  // 随机角度
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f;  // 设置速度
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<MiracleMatterJavLight>(), (int)(Projectile.damage * 1.1f), Projectile.knockBack, Main.myPlayer);
            }

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


            // 3. 魔法尘埃特效（六芒星特效）
            int vertices = 6; // 六芒星的顶点数
            int particlesPerEdge = 12; // 每条边的粒子数
            float radius = 75f; // 六芒星的半径

            for (int edge = 0; edge < vertices; edge++)
            {
                // 当前顶点和下一个顶点的角度
                float currentAngle = MathHelper.TwoPi / vertices * edge;
                float nextAngle = MathHelper.TwoPi / vertices * (edge + 1);

                // 当前顶点和下一个顶点的坐标
                Vector2 startPoint = new Vector2((float)Math.Cos(currentAngle), (float)Math.Sin(currentAngle)) * radius;
                Vector2 endPoint = new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * radius;

                // 在当前边上生成粒子
                for (int i = 0; i <= particlesPerEdge; i++)
                {
                    float progress = i / (float)particlesPerEdge;
                    Vector2 position = Vector2.Lerp(startPoint, endPoint, progress); // 插值计算粒子位置
                    Vector2 velocity = position.SafeNormalize(Vector2.Zero) * 5f; // 粒子速度

                    Dust magic = Dust.NewDustPerfect(Projectile.Center + position, 267, velocity); // 267为魔法尘埃类型
                    magic.scale = 2.2f;  // 调整大小，更加显眼
                    magic.fadeIn = 0.7f; // 渐入效果更强
                    magic.color = CalamityUtils.MulticolorLerp(progress, CalamityUtils.ExoPalette); // 使用 ExoPalette 的渐变效果
                    magic.noGravity = true;

                    // 在特定位置添加额外粒子，增强视觉效果
                    if (i % (particlesPerEdge / 3) == 0) // 每条边的 1/3 点添加旋转粒子
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


            //// 4. 新增两个椭圆粒子特效
            //int ellipseParticleCount = 100;
            //float longAxisLength = 75f * 2;  // 长轴
            //float shortAxisLength = 75f * 1; // 短轴

            //// 随机生成初始角度，保证两个椭圆互相垂直
            //float baseRotation = Main.rand.NextFloat(MathHelper.TwoPi); // 随机初始角度
            //float perpendicularRotation = baseRotation + MathHelper.PiOver2; // 垂直的角度

            //for (int i = 0; i < ellipseParticleCount; i++)
            //{
            //    // 第一个椭圆
            //    float angle1 = MathHelper.TwoPi * i / ellipseParticleCount;
            //    Vector2 ellipse1Offset = new Vector2(
            //        (float)Math.Cos(angle1) * longAxisLength,
            //        (float)Math.Sin(angle1) * shortAxisLength
            //    ).RotatedBy(baseRotation); // 旋转随机的角度

            //    Vector2 ellipse1Velocity = ellipse1Offset * 0.02f; // 缓慢扩散
            //    Dust ellipse1Dust = Dust.NewDustPerfect(Projectile.Center + ellipse1Offset, 267, ellipse1Velocity);
            //    ellipse1Dust.scale = Main.rand.NextFloat(1.5f, 2f);
            //    ellipse1Dust.fadeIn = 0.5f;
            //    ellipse1Dust.color = CalamityUtils.MulticolorLerp(i / (float)ellipseParticleCount, CalamityUtils.ExoPalette);
            //    ellipse1Dust.noGravity = true;

            //    // 第二个椭圆
            //    float angle2 = MathHelper.TwoPi * i / ellipseParticleCount;
            //    Vector2 ellipse2Offset = new Vector2(
            //        (float)Math.Cos(angle2) * shortAxisLength,
            //        (float)Math.Sin(angle2) * longAxisLength
            //    ).RotatedBy(perpendicularRotation); // 旋转垂直的角度

            //    Vector2 ellipse2Velocity = ellipse2Offset * 0.02f; // 缓慢扩散
            //    Dust ellipse2Dust = Dust.NewDustPerfect(Projectile.Center + ellipse2Offset, 267, ellipse2Velocity);
            //    ellipse2Dust.scale = Main.rand.NextFloat(1.5f, 2f);
            //    ellipse2Dust.fadeIn = 0.5f;
            //    ellipse2Dust.color = CalamityUtils.MulticolorLerp(i / (float)ellipseParticleCount, CalamityUtils.ExoPalette);
            //    ellipse2Dust.noGravity = true;
            //}


            // 4. 新增两个椭圆粒子特效
            int ellipseParticleCount = 100;
            float longAxisLength = 75f * 2f;  // 长轴为菱形半径的4倍
            float shortAxisLength = 75f * 1f; // 短轴为菱形半径的2倍

            for (int i = 0; i < ellipseParticleCount; i++)
            {
                // 第一个椭圆
                float angle1 = MathHelper.TwoPi * i / ellipseParticleCount;
                Vector2 ellipse1Offset = new Vector2(
                    (float)Math.Cos(angle1) * longAxisLength,
                    (float)Math.Sin(angle1) * shortAxisLength
                ).RotatedBy(MathHelper.PiOver4); // 椭圆旋转45度

                Vector2 ellipse1Velocity = ellipse1Offset * 0.02f; // 缓慢扩散
                Dust ellipse1Dust = Dust.NewDustPerfect(Projectile.Center + ellipse1Offset, 267, ellipse1Velocity);
                ellipse1Dust.scale = Main.rand.NextFloat(1.5f, 2f);
                ellipse1Dust.fadeIn = 0.5f;
                ellipse1Dust.color = CalamityUtils.MulticolorLerp(i / (float)ellipseParticleCount, CalamityUtils.ExoPalette);
                ellipse1Dust.noGravity = true;

                // 第二个椭圆
                float angle2 = MathHelper.TwoPi * i / ellipseParticleCount;
                Vector2 ellipse2Offset = new Vector2(
                    (float)Math.Cos(angle2) * longAxisLength,
                    (float)Math.Sin(angle2) * shortAxisLength
                ).RotatedBy(-MathHelper.PiOver4); // 椭圆旋转-45度

                Vector2 ellipse2Velocity = ellipse2Offset * 0.02f; // 缓慢扩散
                Dust ellipse2Dust = Dust.NewDustPerfect(Projectile.Center + ellipse2Offset, 267, ellipse2Velocity);
                ellipse2Dust.scale = Main.rand.NextFloat(1.5f, 2f);
                ellipse2Dust.fadeIn = 0.5f;
                ellipse2Dust.color = CalamityUtils.MulticolorLerp(i / (float)ellipseParticleCount, CalamityUtils.ExoPalette);
                ellipse2Dust.noGravity = true;
            }


        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300); // 超位崩解
            SoundEngine.PlaySound(SoundID.Item132.WithVolumeScale(2.5f), Projectile.Center);
        }


    }
}
