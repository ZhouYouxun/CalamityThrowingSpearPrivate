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
using CalamityMod.Particles;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.WulfrimJav
{
    public class WulfrimJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/WulfrimJav/WulfrimJav";
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
            Projectile.width = Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 400;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.LimeGreen.ToVector3() * 1.55f);

            // 弹幕保持直线运动并逐渐加速
            //Projectile.velocity *= 1.01f;

            {
                // 释放亮绿色粒子特效
                if (Main.rand.NextBool(5))
                {
                    Vector2 trailPos = Projectile.Center;
                    float trailScale = Main.rand.NextFloat(0.8f, 1.2f); // 粒子缩放
                    Color trailColor = Color.LimeGreen;

                    // 创建粒子
                    Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
                    GeneralParticleHandler.SpawnParticle(trail);
                }

                // 🤖 双螺旋破烂绿尾迹（飞行期间）
                float spiralRadius = 8f;
                float spiralSpeed = 0.2f;
                float time = Main.GameUpdateCount * spiralSpeed;

                for (int s = 0; s < 2; s++)
                {
                    float spiralOffset = s * MathHelper.Pi;
                    float angle = time + spiralOffset;
                    Vector2 offset = angle.ToRotationVector2() * spiralRadius;

                    // 浅绿色 Dust 尾迹
                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center + offset,
                            Main.rand.NextBool() ? DustID.GemEmerald : DustID.GrassBlades,
                            -Projectile.velocity * 0.2f,
                            100,
                            Color.LimeGreen,
                            1.0f
                        );
                        d.noGravity = true;
                    }

                    // 稀疏绿色 SparkParticle 模拟小型机械残屑
                    if (Main.rand.NextBool(4))
                    {
                        Particle spark = new SparkParticle(
                            Projectile.Center + offset,
                            offset.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.5f, 1.5f),
                            false,
                            20,
                            0.8f,
                            Color.LimeGreen
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }

            }



            // 每帧增加 ai[0] 计数
            Projectile.ai[0]++;

            // 前x帧禁用方块碰撞（防止在平地上某些角度射不出来）
            if (Projectile.ai[0] < 5)
            {
                Projectile.tileCollide = false; // 禁用碰撞
            }
            else
            {
                Projectile.tileCollide = true; // 启用碰撞
            }


        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {


        }



        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item15, Projectile.position);
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/钨钢矛命中破碎音效") with { Volume = 0.74f, Pitch = -0.75f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.7f, Pitch = 0.3f }, Projectile.Center);


            {
                // 在弹幕死亡时额外生成扇形射出 4-6 发 WulfrimJavExtraPROJ
                int extraCount = Main.rand.Next(4, 7);
                for (int i = 0; i < extraCount; i++)
                {
                    float angle = MathHelper.ToRadians(Main.rand.NextFloat(-60f, 60f));
                    Vector2 velocity = Projectile.velocity.RotatedBy(angle).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(6f, 10f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<WulfrimJavExtraPROJ>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }

                // 原特效收敛 50%
                int sparks = 10; // 原本 20
                for (int i = 0; i < sparks; i++)
                {
                    float angle = MathHelper.TwoPi * i / sparks + Main.rand.NextFloat(-0.05f, 0.05f);
                    float speed = Main.rand.NextFloat(3f, 6f); // 原本 6f-12f
                    Vector2 velocity = angle.ToRotationVector2() * speed;

                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        velocity,
                        false,
                        35,
                        1.0f,
                        Color.LimeGreen
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                int circles = 1; // 原本 2
                int dustPerCircle = 8; // 原本 16
                float baseRadius = 4f;
                float radiusStep = 8f;
                for (int c = 0; c < circles; c++)
                {
                    float radius = baseRadius + c * radiusStep;
                    for (int i = 0; i < dustPerCircle; i++)
                    {
                        float angle = MathHelper.TwoPi * i / dustPerCircle + Main.rand.NextFloat(-0.1f, 0.1f);
                        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f); // 原本 4f-8f

                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center,
                            Main.rand.NextBool() ? DustID.GemEmerald : DustID.GrassBlades,
                            velocity,
                            100,
                            Color.LimeGreen,
                            Main.rand.NextFloat(0.8f, 1.2f)
                        );
                        d.noGravity = false;
                    }
                }

            }





        }


    }
}