using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Humanizer.In;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
{
    public class RevelationSUPERBoom : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        private Player Owner => Main.player[Projectile.owner];
        private int time = 0;

        public override void SetDefaults()
        {
            Projectile.width = 600;
            Projectile.height = 600;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.timeLeft = 31;
        }

        public override void AI()
        {
            if (time < 30) // 在30帧内进行变化
            {
                // 生成较小的橙黄色和淡黄色爆炸特效
                Vector2 spawnPosition = Projectile.Center;
                Color orangeColor = Color.Orange;
                Color lightYellowColor = Color.LightYellow;

                // 逐渐缩小的 scale 和 rotationSpeed
                float progress = 1f - (time / 30f); // 从1逐渐减少到0
                float smallerScale = 1.5f * progress; // 缩小比例
                float rotationSpeed = Main.rand.NextFloat(-10f, 10f) * progress; // 旋转速度随着缩小逐渐减慢

                // 创建两个爆炸粒子，颜色为橙黄色和淡黄色
                Particle orangeExplosion = new CustomPulse(spawnPosition, Vector2.Zero, orangeColor, "CalamityMod/Particles/LargeBloom", new Vector2(0.8f, 0.8f) * progress, rotationSpeed, smallerScale, smallerScale - 0.5f * progress, 15);
                Particle yellowExplosion = new CustomPulse(spawnPosition, Vector2.Zero, lightYellowColor, "CalamityMod/Particles/LargeBloom", new Vector2(0.8f, 0.8f) * progress, -rotationSpeed, smallerScale, smallerScale - 0.5f * progress, 15);

                GeneralParticleHandler.SpawnParticle(orangeExplosion);
                GeneralParticleHandler.SpawnParticle(yellowExplosion);
            }

            if (time >= 30)
            {
                Projectile.Kill();
            }

            time++;
        }


        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/RevelationBIGEXP"));

            // ===================================
            // 🚩【0️⃣ 屏幕震动与基础冲击波（保留，微调）】🚩
            // ===================================
            Owner.Calamity().GeneralScreenShakePower = 10f; // 维持削减后幅度

            Particle finalPulse = new StaticPulseRing(
                Projectile.Center,
                Vector2.Zero,
                Color.White * 0.4f,
                new Vector2(0.33f, 0.33f),
                0f,
                5f,
                0f,
                10
            );
            GeneralParticleHandler.SpawnParticle(finalPulse);

            // ===================================
            // 🚩【1️⃣ 有序：多层“螺旋星体阵”GlowSparkParticle矩阵】
            // ===================================
            int spiralLayers = 4;
            int sparksPerLayer = 24;
            float baseRadius = 20f;
            float radiusStep = 12f;

            for (int layer = 0; layer < spiralLayers; layer++)
            {
                float radius = baseRadius + layer * radiusStep;
                for (int i = 0; i < sparksPerLayer; i++)
                {
                    float angle = MathHelper.TwoPi * i / sparksPerLayer + layer * 0.5f; // 形成螺旋
                    Vector2 dir = angle.ToRotationVector2();
                    Vector2 spawnPos = Projectile.Center + dir * radius;
                    Vector2 velocity = dir.RotatedBy(MathHelper.PiOver4) * Main.rand.NextFloat(5f, 10f);

                    GlowSparkParticle spark = new GlowSparkParticle(
                        spawnPos,
                        velocity,
                        false,
                        Main.rand.Next(40, 55),
                        Main.rand.NextFloat(0.08f, 0.12f),
                        Color.Cyan * 0.8f,
                        new Vector2(0.5f, 1.5f)
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            // ===================================
            // 🚩【2️⃣ 有序：十字星矩阵爆裂】
            // ===================================
            int starCount = 12;
            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * i / starCount;
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(16f, 32f);
                Vector2 spawnPos = Projectile.Center + offset;

                GenericSparkle sparkle = new GenericSparkle(
                    spawnPos,
                    Vector2.Zero,
                    Color.White,
                    Color.Cyan,
                    Main.rand.NextFloat(2.0f, 2.8f),
                    8,
                    Main.rand.NextFloat(-0.05f, 0.05f),
                    1.8f
                );
                GeneralParticleHandler.SpawnParticle(sparkle);
            }

            // ===================================
            // 🚩【3️⃣ 无序：Dust蓝白能量爆散（主导）】
            // ===================================
            int dustAmount = 300; // 超级爆散
            int[] dustTypes = { DustID.BlueCrystalShard, DustID.WhiteTorch };
            Color[] dustColors = { Color.Cyan, Color.White, Color.LightBlue };

            for (int i = 0; i < dustAmount; i++)
            {
                int type = dustTypes[Main.rand.Next(dustTypes.Length)];
                Color color = dustColors[Main.rand.Next(dustColors.Length)];
                Vector2 velocity = Main.rand.NextVector2Circular(18f, 18f);

                int dust = Dust.NewDust(Projectile.Center, 0, 0, type, velocity.X, velocity.Y, 100, color, Main.rand.NextFloat(1.2f, 2.0f));
                Main.dust[dust].noGravity = true;
            }

            // ===================================
            // 🚩【4️⃣ 无序：亮白线性粒子爆散（全面覆盖）】
            // ===================================
            int linearCount = 120;
            for (int i = 0; i < linearCount; i++)
            {
                float angle = MathHelper.ToRadians(i * (360f / linearCount));
                Vector2 direction = angle.ToRotationVector2();
                Color brightWhite = Color.White;
                Vector2 velocity = direction * Main.rand.NextFloat(15f, 30f);

                Particle sparkTrail = new SparkParticle(
                    Projectile.Center,
                    velocity,
                    false,
                    50,
                    Main.rand.NextFloat(0.2f, 0.35f),
                    brightWhite
                );
                GeneralParticleHandler.SpawnParticle(sparkTrail);
            }

            // ===================================
            // 🚩【5️⃣ 有序 + 中和：彩色脉冲环爆发（可适当保留原爆炸感觉）】
            // ===================================
            for (int i = 0; i < 20; i++)
            {
                Color randomColor = Main.rand.Next(3) switch
                {
                    0 => Color.Cyan,
                    1 => Color.LightBlue,
                    _ => Color.White,
                };

                Particle pulse = new CustomPulse(
                    Projectile.Center,
                    Vector2.Zero,
                    randomColor * 0.6f,
                    "CalamityMod/Particles/FlameExplosion",
                    new Vector2(0.4f, 0.4f),
                    Main.rand.NextFloat(-25, 25),
                    0f,
                    (5f - i * 0.25f) * 0.4f / 3f, // 最终大小
                    50
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }
        }










    }
}
