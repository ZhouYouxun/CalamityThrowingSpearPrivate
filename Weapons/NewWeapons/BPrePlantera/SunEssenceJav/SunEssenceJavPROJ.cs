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
                    // === 🚩 1️⃣ 十字星（保留，不变） ===
                    Vector2 sparkOffset = Projectile.velocity * -0.3f + Main.rand.NextVector2Circular(2f, 2f);
                    Color startColor = new Color(255, 250, 200, 80);
                    Color endColor = new Color(255, 230, 150, 60);
                    GenericSparkle sparker = new GenericSparkle(
                        Projectile.Center + sparkOffset,
                        Vector2.Zero,
                        startColor,
                        endColor,
                        Main.rand.NextFloat(2.5f, 3.2f),
                        18,
                        Main.rand.NextFloat(-0.02f, 0.02f),
                        2.8f
                    );
                    GeneralParticleHandler.SpawnParticle(sparker);

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
                            Main.rand.Next(20, 40),
                            Main.rand.NextFloat(1.0f, 2.0f), // 放大体积
                            sparkColor * 0.9f
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

                    // === 🚩 3️⃣ Dust 狂野化，生成更多、更远、更亮 ===
                    int dustCount = 8; // 原本约 2-3，直接 ×3
                    for (int i = 0; i < dustCount; i++)
                    {
                        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi));
                        Vector2 dustVelocity = direction * Main.rand.NextFloat(3f, 10f) + Main.rand.NextVector2Circular(3f, 3f); // 速度范围扩大
                        Dust dust = Dust.NewDustPerfect(
                            Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), // 生成范围扩大
                            DustID.SolarFlare,
                            dustVelocity,
                            100,
                            Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat(0.3f, 0.7f)),
                            Main.rand.NextFloat(1.5f, 2.8f) // 粒子体积更大
                        );
                        dust.noGravity = true;
                        dust.fadeIn = Main.rand.NextFloat(0.8f, 1.2f);
                    }
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
                    // 生成高速旋转期间增强特效（替换原段）
                    for (int i = 0; i < 2; i++) // 两个方向
                    {
                        float currentBaseAngle = (i == 0) ? baseAngle1 : baseAngle2;

                        for (int j = 0; j < 2; j++) // 每方向 X 个光点
                        {
                            float randomAngle = currentBaseAngle + Main.rand.NextFloat(-MathHelper.Pi / 60, MathHelper.Pi / 60); // 偏移范围更小，收敛
                            Vector2 particleVelocity = randomAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 19f);

                            Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f); // 收敛范围缩小
                            Color particleColor = Color.White * 0.7f; // 更亮更白
                            float particleScale = Main.rand.NextFloat(0.24f, 0.36f); // 稍大以便可见

                            GeneralParticleHandler.SpawnParticle(new GenericBloom(
                                particlePosition,
                                particleVelocity,
                                particleColor,
                                particleScale,
                                Main.rand.Next(20, 35)
                            ));
                        }
                    }

                    // === 🚩 添加白色/黄色 Dust 环绕 ===
                    int dustCount = 7; // 控制数量适中，保证性能
                    for (int k = 0; k < dustCount; k++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 direction = angle.ToRotationVector2();
                        Vector2 dustPos = Projectile.Center + direction * Main.rand.NextFloat(8f, 20f);
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

                    // === 🚩 添加线性 SparkParticle 放射线但控制范围、形成流动感 ===
                    int sparkCount = 6;
                    for (int s = 0; s < sparkCount; s++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 direction = angle.ToRotationVector2();
                        Vector2 sparkVel = direction * Main.rand.NextFloat(7f, 19f);

                        Color sparkColor = Main.rand.NextBool() ? Color.White : Color.Yellow;
                        Particle spark = new SparkParticle(
                            Projectile.Center + direction * Main.rand.NextFloat(8f, 24f),
                            sparkVel,
                            false,
                            Main.rand.Next(20, 35),
                            Main.rand.NextFloat(0.8f, 1.4f),
                            sparkColor * 0.8f
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

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

