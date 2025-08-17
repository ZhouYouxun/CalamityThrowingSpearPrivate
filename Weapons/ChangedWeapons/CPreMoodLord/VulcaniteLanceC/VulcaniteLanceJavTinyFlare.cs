using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using System;
using CalamityMod.Particles;
using Terraria.Audio;
using Terraria.DataStructures;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC
{
    public class VulcaniteLanceJavTinyFlare : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        private float rotationAngle = 0f; // 旋转角度
        private const float rotationSpeed = 0.05f; // 旋转速度

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 350;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool? CanHitNPC(NPC target) => Projectile.timeLeft < 150 && target.CanBeChasedBy(Projectile);
        public override void OnSpawn(IEntitySource source)
        {
          
        }

        public override void AI()
        {
            // 更新旋转角度
            rotationAngle += rotationSpeed;
            if (rotationAngle > MathHelper.TwoPi)
            {
                rotationAngle -= MathHelper.TwoPi;
            }

            if (Projectile.timeLeft == 325)
            {
                int points = 8; // 爆发点数量
                float spread = MathHelper.ToRadians(90f); // 爆发角度范围（90度扇形）
                float baseAngle = Projectile.velocity.ToRotation(); // 以弹幕前进方向为中心

                for (int i = 0; i < points; i++)
                {
                    // 在扇形范围内随机偏移
                    float angle = baseAngle + Main.rand.NextFloat(-spread / 2f, spread / 2f);
                    Vector2 dir = angle.ToRotationVector2();

                    // Dust 岩浆颗粒（大颗、亮）
                    Vector2 dustVel = dir * Main.rand.NextFloat(5f, 9f);
                    int dust = Dust.NewDust(
                        Projectile.Center,
                        0, 0,
                        DustID.InfernoFork,
                        dustVel.X, dustVel.Y,
                        100,
                        Color.OrangeRed,
                        Main.rand.NextFloat(1.8f, 2.6f)
                    );
                    Main.dust[dust].noGravity = false; // ✅ 受重力，下坠感
                    Main.dust[dust].velocity = dustVel;

                    // Spark 熔岩滴落（小颗粒，拖尾感）
                    Vector2 sparkVel = dir * Main.rand.NextFloat(6f, 11f);
                    SparkParticle spark = new SparkParticle(
                        Projectile.Center,
                        sparkVel,
                        true, // ✅ 受重力
                        Main.rand.Next(25, 40), // 生命周期更长
                        Main.rand.NextFloat(1.0f, 1.8f),
                        Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat())
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }


            // 在 `timeLeft <= 325` 时开始生成粒子
            if (Projectile.timeLeft <= 325)
            {
                // 每帧在弹幕中心生成一颗静止火焰粒子
                // 核心粒子
                int fiery = Dust.NewDust(
                    Projectile.Center,
                    0, 0,
                    DustID.InfernoFork,
                    0f, 0f,
                    100,
                    default,
                    Main.rand.NextFloat(1.85f, 2.35f)
                );
                Main.dust[fiery].noGravity = true;
                Main.dust[fiery].velocity = Vector2.Zero;

                //// 粒子轨道旋转角度（围绕弹幕旋转）
                //float spinAngle = Main.GameUpdateCount * 0.2f; // 每帧推进角度，速度可调
                //float radius = 1f * 16f; // 1格半径 = 16像素

                //// 旋转前进
                //for (int i = 0; i < 1; i++)
                //{
                //    float angle = spinAngle + MathHelper.TwoPi / 3f * i;
                //    Vector2 offset = angle.ToRotationVector2() * radius;

                //    int glow = Dust.NewDust(
                //        Projectile.Center + offset,
                //        0, 0,
                //        DustID.InfernoFork,
                //        offset.X * 0.2f,
                //        offset.Y * 0.2f,
                //        100,
                //        default,
                //        Main.rand.NextFloat(1.2f, 1.6f)
                //    );
                //    Main.dust[glow].noGravity = true;
                //    Main.dust[glow].velocity = Vector2.Zero;
                //}

            }



            // 追踪逻辑
            if (Projectile.timeLeft < 275)
                CalamityUtils.HomeInOnNPC(Projectile, true, 1800f, 10f, 20f);
        }
        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item69, Projectile.Center);

            {
                int numParticles = 12;
                float baseSpeed = 8f;
                for (int i = 0; i < numParticles; i++)
                {
                    float angle = MathHelper.TwoPi / numParticles * i;
                    Vector2 velocity = angle.ToRotationVector2() * baseSpeed * Main.rand.NextFloat(0.3f, 0.9f);

                    // Dust 火焰碎屑
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.InfernoFork, velocity.X, velocity.Y, 100, Color.OrangeRed, Main.rand.NextFloat(1.2f, 1.7f));
                    Main.dust[dust].noGravity = false;

                    // Spark 火花
                    SparkParticle spark = new SparkParticle(
                        Projectile.Center,
                        velocity * 1.2f,
                        true,
                        Main.rand.Next(10, 20),
                        Main.rand.NextFloat(0.6f, 1.4f),
                        Color.Yellow
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            // 播放额外的爆炸特效
            Particle blastRing = new CustomPulse(
                Projectile.Center,
                Vector2.Zero,
                Color.Yellow, // 熔岩的亮黄色
                "CalamityThrowingSpear/Texture/ThebigExplosion1",
                Vector2.One * 0.33f,
                Main.rand.NextFloat(-10f, 10f),
                0.078f,
                0.450f,
                30
            );
            GeneralParticleHandler.SpawnParticle(blastRing);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 90); 
        }
    }
}
