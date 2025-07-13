using CalamityMod.Particles;
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

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.WulfrimJav
{
    public class WulfrimJavExtraPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
        public static bool ForceKillForExp = false;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 7; // 只允许一次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 自转逻辑：根据飞行方向左右自动加减
            if (Projectile.velocity.X > 0)
            {
                Projectile.rotation += 0.3f; // 速度适中，避免头晕
            }
            else
            {
                Projectile.rotation -= 0.3f;
            }

            {
                // 🌿 收敛：减少点状粒子数量、缩小大小
                for (int i = 0; i < 2; i++) // 原本 3 -> 2
                {
                    Vector2 offset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 2 * i) * 8f; // 半径缩小
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Water, null, 0, Color.LightGreen, 1.0f); // scale 1.5 -> 1.0
                    dust.noGravity = true;
                }

                // 移除左右抖动颤抖以提升稳定性
                // 可选保留轻微抖动：
                Projectile.rotation += 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.2f); // 0.5f -> 0.2f

                float spiralRadius = 8f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 1.5f; // 半径缩小
                float spiralSpeed = 0.3f;
                float time = Main.GameUpdateCount * spiralSpeed;

                for (int s = 0; s < 2; s++)
                {
                    float spiralOffset = s * MathHelper.Pi;
                    float angle = time + spiralOffset + Main.rand.NextFloat(-0.05f, 0.05f); // 噪声减小

                    Vector2 offset = angle.ToRotationVector2() * spiralRadius;

                    // 🌿 收敛 Dust：
                    if (Main.rand.NextBool(3)) // 触发概率降低
                    {
                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center + offset,
                            DustID.GemEmerald,
                            offset.RotatedBy(MathHelper.PiOver2) * 0.3f, // 速度降低
                            100,
                            Color.LimeGreen,
                            0.7f // scale 1.0 -> 0.7
                        );
                        d.noGravity = true;
                    }
                  
                }
            }

            // 受重力影响的速度变化
            //Projectile.velocity.Y += 0.15f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.1f }, Projectile.Center);

            // 减少一次穿透次数
            Projectile.penetrate--;

            // 若剩余穿透次数用尽则杀死弹幕
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
                return false;
            }

            // 计算反弹角度（正负25度随机）
            float randomOffset = MathHelper.ToRadians(Main.rand.NextFloat(-25f, 25f));

            // X轴反弹检测
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }

            // Y轴反弹检测
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }

            // 应用随机偏移角度
            Projectile.velocity = Projectile.velocity.RotatedBy(randomOffset);

            // 生成收敛的绿色粒子
            for (int i = 0; i < 16; i++) // 数量适度控制
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GemEmerald,
                    Main.rand.NextVector2Circular(1f, 1f),
                    100,
                    Color.LimeGreen,
                    Main.rand.NextFloat(0.96f, 1.0f)); // 比之前更暗淡
                d.noGravity = true;
            }

            // 防止粘在墙壁上
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.0f, Pitch = 0.1f }, Projectile.Center);

            //if (ForceKillForExp)
            //{
            //    // 由玩家右键强制死亡时生成 WulfrimJavExtraExtraEXP
            //    Projectile.NewProjectile(
            //        Projectile.GetSource_FromThis(),
            //        Projectile.Center,
            //        Vector2.Zero,
            //        ModContent.ProjectileType<WulfrimJavExtraExtraEXP>(),
            //        Projectile.damage,
            //        Projectile.knockBack,
            //        Projectile.owner
            //    );
            //}


            {
                // =======================
                // 圆形范围爆散高级版特效
                // =======================

                Vector2 center = Projectile.Center;

                // 🌿 1️⃣ 绿色线性 SparkParticle 环状向外爆散
                //int sparkCount = 20;
                //for (int i = 0; i < sparkCount; i++)
                //{
                //    float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.05f, 0.05f); // 小幅随机扰动
                //    float speed = Main.rand.NextFloat(1f, 4f);
                //    Vector2 velocity = angle.ToRotationVector2() * speed;

                //    Particle spark = new SparkParticle(
                //        center,
                //        velocity.RotatedBy(MathHelper.ToRadians(15f)), // 每个偏转 15°，提升视觉层次
                //        false,
                //        10,
                //        0.8f,
                //        Color.LimeGreen * 0.9f
                //    );
                //    GeneralParticleHandler.SpawnParticle(spark);
                //}

                // 🌿 2️⃣ 多层 Dust 环向外爆散，高级可见范围圆提示
                int layers = 4;
                int dustPerLayer = 12;
                float baseRadius = 16f; // 每层半径递增，第一层 16px
                float radiusStep = 16f; // 递增 16px，最终可到 64px
                for (int l = 0; l < layers; l++)
                {
                    float radius = baseRadius + l * radiusStep;
                    for (int i = 0; i < dustPerLayer; i++)
                    {
                        // 按圆周均匀分布并微扰角度形成层次感
                        float angle = MathHelper.TwoPi * i / dustPerLayer + Main.rand.NextFloat(-0.04f, 0.04f);
                        Vector2 position = center + angle.ToRotationVector2() * radius;

                        // 爆散速度沿切线，提升“旋转扩散”动态
                        Vector2 velocity = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0.5f, 1.5f);

                        Dust d = Dust.NewDustPerfect(
                            position,
                            DustID.GemEmerald,
                            velocity,
                            100,
                            Color.LimeGreen * 0.9f,
                            Main.rand.NextFloat(0.6f, 0.82f)
                        );
                        d.noGravity = true;
                    }
                }

                // 🌿 3️⃣ 中心少量 SparkParticle 抽离核心感（爆心高亮提示）
                for (int i = 0; i < 6; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                    Particle coreSpark = new SparkParticle(
                        center,
                        velocity,
                        false,
                        40,
                        0.7f,
                        Color.GreenYellow * 0.9f
                    );
                    GeneralParticleHandler.SpawnParticle(coreSpark);
                }

            }


        }


    }
}