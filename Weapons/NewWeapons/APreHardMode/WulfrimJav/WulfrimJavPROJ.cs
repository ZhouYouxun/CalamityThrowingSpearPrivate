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

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

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
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item15, Projectile.position);
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/New/钨钢矛击中1") with { Volume = 1.0f, Pitch = -0.75f }, Projectile.Center);

            // 定义damage和knockback，使用当前弹幕的数值
            int damage = Projectile.damage; // 获取当前弹幕的伤害值
            float knockback = Projectile.knockBack; // 获取当前弹幕的击退值

            // 在敌人上方50个方块处召唤WulfrimJavExtraPROJ
            Vector2 spawnPos = target.Center - new Vector2(0, 50 * 16); // 大约50个方块
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<WulfrimJavExtraPROJ>(), damage, knockback, Projectile.owner);

            {
                // 🤖 菱形特效 500% 升级版：复杂 + 搞笑 + 宏伟
                for (int i = 0; i < 75; i++)
                {
                    float theta = MathHelper.TwoPi * i / 75f;
                    float dynamicScale = 5f + 2f * (float)Math.Sin(4 * theta); // 半径动态缩放
                    float chaoticFactorX = (float)Math.Cos(2 * theta + Math.Cos(7 * theta));
                    float chaoticFactorY = (float)Math.Sin(3 * theta + Math.Sin(5 * theta));

                    Vector2 puffDustVelocity = new Vector2(chaoticFactorX, chaoticFactorY).SafeNormalize(Vector2.UnitY) * dynamicScale;

                    Dust magic = Dust.NewDustPerfect(
                        target.Center,
                        267,
                        puffDustVelocity,
                        0,
                        Color.LightGreen,
                        1.8f
                    );
                    magic.fadeIn = 0.5f;
                    magic.noGravity = true;
                }
            }
        }



        public override void OnKill(int timeLeft)
        {
            // 🤖 扇形绿色 SparkParticle 喷射（破烂机械爆散感）
            int sparks = 20;
            for (int i = 0; i < sparks; i++)
            {
                float angle = MathHelper.TwoPi * i / sparks + Main.rand.NextFloat(-0.05f, 0.05f);
                float speed = Main.rand.NextFloat(6f, 12f);
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

            // 🤖 多层环状浅绿 Dust 喷射
            int circles = 2;
            int dustPerCircle = 16;
            float baseRadius = 4f;
            float radiusStep = 8f;
            for (int c = 0; c < circles; c++)
            {
                float radius = baseRadius + c * radiusStep;
                for (int i = 0; i < dustPerCircle; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustPerCircle + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);

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

            // 🤖 搞笑机械“噗”音效
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.7f, Pitch = 0.3f }, Projectile.Center);
        }


    }
}