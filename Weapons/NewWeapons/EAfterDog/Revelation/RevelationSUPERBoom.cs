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
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/EMP"));

            // 释放大量小型粒子组成垂直激光
            for (int i = 0; i < 25; i++)
            {
                Color randomColor = Main.rand.Next(4) switch
                {
                    0 => Color.Red,
                    1 => Color.MediumTurquoise,
                    2 => Color.Orange,
                    _ => Color.LawnGreen,
                };
                GlowSparkParticle spark = new GlowSparkParticle(Projectile.Center + Main.rand.NextVector2Circular(5, 5), Vector2.UnitY * Main.rand.NextFloat(-20f, 20f), false, Main.rand.Next(40, 50), Main.rand.NextFloat(0.04f, 0.095f), randomColor, new Vector2(0.3f, 1.6f));
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 释放大量小型粒子组成水平激光
            for (int i = 0; i < 25; i++)
            {
                Color randomColor = Main.rand.Next(4) switch
                {
                    0 => Color.Red,
                    1 => Color.MediumTurquoise,
                    2 => Color.Orange,
                    _ => Color.LawnGreen,
                };
                GlowSparkParticle spark = new GlowSparkParticle(Projectile.Center + Main.rand.NextVector2Circular(5, 5), Vector2.UnitX * Main.rand.NextFloat(-20f, 20f), false, Main.rand.Next(40, 50), Main.rand.NextFloat(0.04f, 0.095f), randomColor, new Vector2(1.6f, 0.3f));
                GeneralParticleHandler.SpawnParticle(spark);
            }


            // 屏幕震动，震动幅度削减到 1/5
            Owner.Calamity().GeneralScreenShakePower = 2.9f;

            // 生成一个逐渐向外扩散的小型冲击波
            Particle finalPulse = new StaticPulseRing(Projectile.Center, Vector2.Zero, Color.White * 0.4f, new Vector2(0.33f, 0.33f), 0f, 5f, 0f, 10); // 大小为原来的 1/3
            GeneralParticleHandler.SpawnParticle(finalPulse);

            // 在死亡时生成50个亮白色的线性粒子
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.ToRadians(i * (360f / 50f)); // 将360度均分成50个方向
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)); // 计算方向向量
                Color brightWhite = Color.White; // 亮白色
                Vector2 particleVelocity = direction * Main.rand.NextFloat(10f, 20f); // 随机化速度

                // 创建亮白色线性粒子
                Particle trail = new SparkParticle(
                    Projectile.Center,
                    particleVelocity,
                    false,
                    40, // 生命周期
                    Main.rand.NextFloat(0.15f, 0.25f), // 粒子大小随机化
                    brightWhite
                );
                GeneralParticleHandler.SpawnParticle(trail);
            }

            for (int i = 0; i < 10; i++)
            {
                Color randomColor = Main.rand.Next(4) switch
                {
                    0 => Color.Red,
                    1 => Color.MediumTurquoise,
                    2 => Color.Orange,
                    _ => Color.LawnGreen,
                };

                Particle pulse2 = new CustomPulse(
                    Projectile.Center,
                    Vector2.Zero,
                    randomColor * 0.7f,
                    "CalamityMod/Particles/FlameExplosion",
                    new Vector2(0.2f, 0.2f), // 缩小为原来的1/5大小
                    Main.rand.NextFloat(-20, 20),
                    0f,
                    (4f - i * 0.28f) * 0.2f, // 缩小为原来的1/5大小
                    50
                );
                GeneralParticleHandler.SpawnParticle(pulse2);
            }

        }










    }
}
