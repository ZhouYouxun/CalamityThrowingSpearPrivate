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
using CalamityMod.Projectiles.Typeless;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ChaosEssenceJav
{
    public class ChaosEssenceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/ChaosEssenceJav/ChaosEssenceJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
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
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
            Projectile.scale = 0.6f;
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.005f;

            // 生成随机的 Lava 粒子效果
            if (Main.rand.NextBool(3)) // 控制粒子生成频率，1/3 的几率生成一个粒子
            {
                Vector2 dustPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2), Main.rand.NextFloat(-Projectile.height / 2, Projectile.height / 2));
                Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Lava);
                dust.velocity = Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f); // 粒子速度带有随机化效果
                dust.noGravity = true; // 设置粒子无重力
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f); // 随机缩放
            }


            // 1️⃣ 快速火花拖尾
            if (Main.rand.NextBool(2))
            {
                Particle spark = new SparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(8f, 16f),
                    false,
                    25,
                    Main.rand.NextFloat(1.0f, 1.8f),
                    Color.Lerp(Color.OrangeRed, Color.DarkRed, Main.rand.NextFloat(0.3f, 0.7f))
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 2️⃣ 小范围烟雾粒子尾焰
            if (Main.GameUpdateCount % 6 == 0)
            {
                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    -Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(1f, 1f),
                    Color.Lerp(Color.DarkRed, Color.Black, Main.rand.NextFloat(0.3f, 0.6f)),
                    30,
                    Main.rand.NextFloat(0.8f, 1.4f),
                    0.6f,
                    Main.rand.NextFloat(-0.02f, 0.02f),
                    false
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 3️⃣ 脉冲波层次感
            if (Main.GameUpdateCount % 12 == 0)
            {
                Particle pulse = new DirectionalPulseRing(
                    Projectile.Center,
                    Vector2.Zero,
                    Color.Lerp(Color.DarkRed, Color.OrangeRed, Main.rand.NextFloat(0.3f, 0.7f)),
                    new Vector2(1.0f, 2.5f),
                    Main.rand.NextFloat(6f),
                    0.15f,
                    0.02f,
                    20
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300); // 原版的狱炎效果
            target.AddBuff(BuffID.OnFire, 300); // 原版的着火效果
            //// 释放 Fuckyou 爆炸弹幕，大小 1.5 倍，伤害 1.0 倍
            //int fuckyouProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FuckYou>(), (int)(Projectile.damage * 1.0f), 0, Projectile.owner);
            //Main.projectile[fuckyouProj].scale = 1.5f;

            //// 释放菱形特效
            //SpawnDiamondParticleEffect();

            //// 随机发射三个 ChaosEssenceJavFIRE 弹幕
            //for (int i = 0; i < 3; i++)
            //{
            //    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(0f, 360f)); // 0 到 360 度随机角度
            //    Vector2 direction = Vector2.UnitY.RotatedBy(angleOffset); // 从中心向外随机方向
            //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 30f, ModContent.ProjectileType<ChaosEssenceJavFIRE>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            //}


        }
        public override void OnKill(int timeLeft)
        {
            // 播放爆炸音效
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);


            {
                // 1️⃣ 大范围火焰尘火爆炸
                for (int i = 0; i < 100; i++)
                {
                    int type = Main.rand.Next(new int[] { DustID.Lava, DustID.Ash, DustID.Torch, DustID.Blood });
                    Vector2 velocity = Main.rand.NextVector2Circular(12f, 12f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, type, velocity, 0, Color.Lerp(Color.OrangeRed, Color.DarkRed, Main.rand.NextFloat(0.3f, 0.7f)), Main.rand.NextFloat(1.8f, 2.8f));
                    dust.noGravity = Main.rand.NextBool(4) ? false : true;
                }

                // 2️⃣ 高速射线火花
                for (int i = 0; i < 40; i++)
                {
                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(12f, 20f),
                        false,
                        35,
                        Main.rand.NextFloat(1.2f, 2.0f),
                        Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat(0.2f, 0.6f))
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 3️⃣ 大范围黑红烟雾升腾
                for (int i = 0; i < 30; i++)
                {
                    Particle smoke = new HeavySmokeParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(40f, 40f),
                        Main.rand.NextVector2Circular(3f, 3f),
                        Color.Lerp(Color.DarkRed, Color.Black, Main.rand.NextFloat(0.3f, 0.7f)),
                        40,
                        Main.rand.NextFloat(1.4f, 2.2f),
                        0.5f,
                        Main.rand.NextFloat(-0.03f, 0.03f),
                        false
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }

                // 4️⃣ 电光裂纹（线性粒子）
                for (int i = 0; i < 12; i++)
                {
                    Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                    LineParticle line = new LineParticle(
                        Projectile.Center,
                        dir * Main.rand.NextFloat(20f, 40f),
                        false,
                        40,
                        Main.rand.NextFloat(0.8f, 1.4f),
                        Color.Lerp(Color.Red, Color.DarkViolet, Main.rand.NextFloat(0.3f, 0.7f))
                    );
                    GeneralParticleHandler.SpawnParticle(line);
                }

                // 5️⃣ 爆心冲击脉冲波
                Particle pulse = new DirectionalPulseRing(
                    Projectile.Center,
                    Vector2.Zero,
                    Color.Lerp(Color.DarkRed, Color.OrangeRed, 0.5f),
                    new Vector2(2.5f, 4f),
                    Main.rand.NextFloat(6f),
                    0.18f,
                    0.02f,
                    30
                );
                GeneralParticleHandler.SpawnParticle(pulse);

            }



            // 6️⃣ 爆炸弹幕（保留）
            int fuckyouProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FuckYou>(), (int)(Projectile.damage * 1.0f), 0, Projectile.owner);
            Main.projectile[fuckyouProj].scale = 1.5f;

            // 7️⃣ 保留的菱形特效
            SpawnDiamondParticleEffect();

            // 8️⃣ 地狱火焰弹幕发射
            int projectileCount = 6;
            for (int i = 0; i < projectileCount; i++)
            {
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(0f, 360f));
                Vector2 dir = Vector2.UnitY.RotatedBy(angleOffset);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir * 30f, ModContent.ProjectileType<ChaosEssenceJavFIRE>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }


        private void SpawnDiamondParticleEffect()
        {
            Vector2[] diamondCorners = {
                new Vector2(0, -20), // 上顶点
                new Vector2(15, 0),  // 右顶点
                new Vector2(0, 20),  // 下顶点
                new Vector2(-15, 0)  // 左顶点
            };

            // 生成菱形的四条边，每条边用 LineParticle 表示
            for (int i = 0; i < 4; i++)
            {
                Vector2 start = Projectile.Center + diamondCorners[i];
                Vector2 end = Projectile.Center + diamondCorners[(i + 1) % 4];
                Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);

                LineParticle line = new LineParticle(start, direction * 15, false, 30, 0.75f, Color.DarkBlue);
                GeneralParticleHandler.SpawnParticle(line);
            }
        }

    }
}
