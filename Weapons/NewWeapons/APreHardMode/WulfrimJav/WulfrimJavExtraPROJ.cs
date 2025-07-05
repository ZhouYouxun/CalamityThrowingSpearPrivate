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
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
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

            // 添加旋转效果
            Projectile.rotation += 0.5f;



            {
                // 添加浅绿色的点状粒子特效
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 3 * i) * 10f;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Water, null, 0, Color.LightGreen, 1.5f);
                    dust.noGravity = true;
                }

                // 🤖 人工智障搞笑飞行特效
                Projectile.rotation += 0.5f * (float)Math.Sin(Main.GameUpdateCount * 0.2f); // 左右抖动颤抖

                float spiralRadius = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 2f;
                float spiralSpeed = 0.3f;
                float time = Main.GameUpdateCount * spiralSpeed;

                for (int s = 0; s < 2; s++)
                {
                    float spiralOffset = s * MathHelper.Pi;
                    float angle = time + spiralOffset + Main.rand.NextFloat(-0.1f, 0.1f); // 加噪声乱抖

                    Vector2 offset = angle.ToRotationVector2() * spiralRadius;

                    // Dust
                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center + offset,
                            DustID.GemEmerald,
                            offset.RotatedBy(MathHelper.PiOver2) * 0.5f,
                            100,
                            Color.LimeGreen,
                            1.0f
                        );
                        d.noGravity = true;
                    }

                    // Spark 短路火花
                    if (Main.rand.NextBool(5))
                    {
                        Vector2 velocity = offset.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-2f, 2f);

                        Particle spark = new SparkParticle(
                            Projectile.Center + offset,
                            velocity,
                            false,
                            15,
                            0.8f,
                            Color.LimeGreen
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }

            }

            // 受重力影响的速度变化
            Projectile.velocity.Y += 0.15f;
        }

        public override void OnKill(int timeLeft)
        {
            // 在弹幕消失时生成随机大小、随机速度和随机方向的绿色线性粒子
            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, -6f));
                Particle trail = new SparkParticle(Projectile.Center, velocity, false, 60, Main.rand.NextFloat(0.8f, 1.2f), Color.Green);
                GeneralParticleHandler.SpawnParticle(trail);
            }

            // 🤖 人工智障搞笑死亡特效：扇形喷射 + 多层 Dust 跳动
            int sparks = 25;
            for (int i = 0; i < sparks; i++)
            {
                float angle = MathHelper.TwoPi * i / sparks + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(6f, 14f);
                Vector2 velocity = angle.ToRotationVector2() * speed;

                Particle spark = new SparkParticle(
                    Projectile.Center,
                    velocity,
                    false,
                    30,
                    1.0f,
                    Color.LimeGreen
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 多层 Dust 环跳动喷射
            int layers = 3;
            int dustPerLayer = 12;
            float baseRadius = 4f;
            float radiusStep = 8f;
            for (int l = 0; l < layers; l++)
            {
                float radius = baseRadius + l * radiusStep;
                for (int i = 0; i < dustPerLayer; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustPerLayer + Main.rand.NextFloat(-0.1f, 0.1f);
                    float speed = 4f + l * 2f;
                    Vector2 velocity = angle.ToRotationVector2() * speed;

                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.GemEmerald,
                        velocity,
                        100,
                        Color.LimeGreen,
                        Main.rand.NextFloat(0.8f, 1.4f)
                    );
                    d.noGravity = false;
                }
            }

        }


    }
}