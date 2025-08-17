using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.YateveoBloomC
{
    internal class YateveoBloomJavRight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";

        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/APreHardMode/YateveoBloomC/YateveoBloomJav";
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
            Projectile.width = Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 8;
            Projectile.timeLeft = 200;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 10; // 无敌帧冷却时间

        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深绿色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.DarkGreen.ToVector3() * 0.55f);

         
            {
             
                // 🌹 优美螺旋花瓣尾迹
                float spiralRadius = 6f;
                float spiralSpeed = 0.2f;
                float time = Main.GameUpdateCount * spiralSpeed;

                for (int s = 0; s < 2; s++)
                {
                    float spiralOffset = s * MathHelper.Pi;
                    float angle = time + spiralOffset;
                    Vector2 offset = angle.ToRotationVector2() * spiralRadius;

                    if (Main.rand.NextBool(2))
                    {
                        int dustType = Main.rand.Next(new int[] { DustID.RedTorch, DustID.GreenTorch, DustID.Dirt });
                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center + offset,
                            dustType,
                            -Projectile.velocity * 0.1f,
                            100,
                            Color.White,
                            1.2f
                        );
                        d.noGravity = true;
                    }
                }

                EmitGrassDustFromTip();




            }

            if (Projectile.localAI[0] > 20f)
            {
                if (Projectile.velocity.Y < 24f)
                {
                    Projectile.velocity.Y += 0.4f;
                }
            }

        }
        private void EmitGrassDustFromTip()
        {
            if (Main.dedServ)
                return;

            // 飞行方向
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);

            // 预测未来位置（前方 18px，可根据需求调整）
            Vector2 tipPosition = Projectile.Center + direction * 18f;

            // 微随机偏移防止完全重合
            tipPosition += Main.rand.NextVector2CircularEdge(0.5f, 0.5f);

            // 动态偏移角度 (轻微摆动效果)
            float dustVelocityArcOffset = 0.4f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.1f;

            for (float side = -1f; side <= 1f; side += 2f) // 左右两侧
            {
                Dust grassDust = Dust.NewDustPerfect(
                    tipPosition,
                    Main.rand.NextBool() ? DustID.Grass : DustID.GrassBlades,
                    null,
                    100,
                    Color.ForestGreen,
                    Main.rand.NextFloat(0.8f, 1.1f)
                );

                grassDust.velocity = direction.RotatedBy(side * dustVelocityArcOffset) * -3f + Projectile.velocity * 0.3f;
                grassDust.noGravity = true;

                // 可选克隆一层较小光辉
                Dust cloneDust = Dust.CloneDust(grassDust);
                cloneDust.scale *= 0.6f;
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 释放独特的草音效	
            SoundEngine.PlaySound(SoundID.Grass, Projectile.position);
            // 使敌人中毒，持续 180 帧
            target.AddBuff(BuffID.Poisoned, 180);
            Projectile.damage = (int)(Projectile.damage * 0.85);

            {
                // 🌿 强喷射感 Dust 特效
                int dustAmount = 40;
                for (int i = 0; i < dustAmount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustAmount;
                    // 高速喷射速度，模拟喷射感
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);

                    Dust d = Dust.NewDustPerfect(
                        target.Center,
                        Main.rand.NextBool() ? DustID.Grass : DustID.GrassBlades,
                        velocity,
                        50, // 更低透明度更亮眼
                        Color.ForestGreen,
                        Main.rand.NextFloat(1.0f, 1.5f)
                    );
                    d.noGravity = true;
                }

                // 🌿 中心小范围高速叶片 Dust 收缩回弹
                for (int i = 0; i < 20; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(6f, 10f);
                    Dust d = Dust.NewDustPerfect(
                        target.Center,
                        DustID.GrassBlades,
                        velocity,
                        80,
                        Color.GreenYellow,
                        Main.rand.NextFloat(0.8f, 1.3f)
                    );
                    d.noGravity = true;
                }
            }

        }


        public override void OnKill(int timeLeft)
        {
            SpawnRoseBloomDust(Projectile.Center);
        }


        private void SpawnRoseBloomDust(Vector2 center)
        {
            // 播放独特草音效
            SoundEngine.PlaySound(SoundID.Grass, Projectile.position);

            int petals = 100;
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY); // 弹幕面向方向

            // 🌹 层 1：花蕊（中心微颗粒，快散，沿前方微偏移）
            for (int i = 0; i < petals; i++)
            {
                float t = MathHelper.TwoPi * i / petals;
                float r = 2f + 0.5f * (float)Math.Sin(6 * t);

                // 让花蕊围绕飞行方向偏移展开
                Vector2 baseDirection = t.ToRotationVector2();
                Vector2 velocity = baseDirection * r * 1.5f;

                // 在弹幕前方微偏移生成
                Vector2 spawnPos = center + forward * 8f; // 偏移 8px 可自行调整

                Dust d = Dust.NewDustPerfect(
                    spawnPos,
                    DustID.Grass,
                    velocity,
                    100,
                    Color.GreenYellow,
                    1.0f
                );
                d.noGravity = true;
            }

            // 🌹 层 2：花瓣（五瓣玫瑰曲线，中速，沿前方偏移更明显）
            for (int i = 0; i < petals; i++)
            {
                float t = MathHelper.TwoPi * i / petals;
                float r = 6f * (1 + 0.4f * (float)Math.Sin(5 * t));

                Vector2 baseDirection = t.ToRotationVector2();
                Vector2 velocity = baseDirection * r;

                // 在弹幕前方偏移生成（更远）
                Vector2 spawnPos = center + forward * 16f; // 偏移 16px 可自行调整

                Dust d = Dust.NewDustPerfect(
                    spawnPos,
                    DustID.GrassBlades,
                    velocity,
                    100,
                    Color.Green,
                    1.3f
                );
                d.noGravity = true;
            }
        }



    }
}
