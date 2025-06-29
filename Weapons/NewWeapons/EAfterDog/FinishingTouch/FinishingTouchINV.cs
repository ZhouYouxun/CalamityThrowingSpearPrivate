using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class FinishingTouchINV : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 100;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255; // 完全透明
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 💥 持续生成橙色椭圆形光波
            Particle pulse = new DirectionalPulseRing(
                Projectile.Center,
                Projectile.velocity * 0.75f,
                Color.Orange,
                new Vector2(1f, 2.5f),
                Projectile.rotation - MathHelper.PiOver4,
                0.2f,
                0.03f,
                20
            );
            GeneralParticleHandler.SpawnParticle(pulse);

            // 🎇 生成多条竹笋状拖尾（随机）
            for (int i = 0; i < 2; i++) // 可根据需要调整数量
            {
                Vector2 spawnOffset = Main.rand.NextVector2Circular(6f, 6f);
                AltSparkParticle spark = new AltSparkParticle(
                    Projectile.Center + spawnOffset,
                    Projectile.velocity * 0.01f,
                    false,
                    8,
                    1.3f,
                    Color.Cyan * 0.135f
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 🟧 有序线性橙色拖尾粒子
            for (int i = 0; i < 8; i++)
            {
                Particle trail = new SparkParticle(
                    Projectile.Center,
                    Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3f),
                    false,
                    60,
                    1.0f,
                    Color.Orange
                );
                GeneralParticleHandler.SpawnParticle(trail);
            }

            // 🧪 Dust 火花（红橙）
            for (int i = 0; i < 6; i++)
            {
                int dust = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Torch,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    150,
                    Color.OrangeRed,
                    1.2f
                );
                Main.dust[dust].noGravity = true;
            }

            // 💨 轻型白色烟雾
            for (int i = 0; i < 3; i++)
            {
                Particle smokeL = new HeavySmokeParticle(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(1f, 1f),
                    Color.WhiteSmoke,
                    18,
                    Main.rand.NextFloat(0.9f, 1.6f),
                    0.35f,
                    Main.rand.NextFloat(-1, 1),
                    false
                );
                GeneralParticleHandler.SpawnParticle(smokeL);
            }

            // 🌫️ 重型灰色烟雾
            Particle smokeH = new HeavySmokeParticle(
                Projectile.Center + new Vector2(0, -10),
                new Vector2(0, -1) * 5f,
                Color.Gray,
                30,
                Main.rand.NextFloat(0.7f, 1.3f),
                1.0f,
                MathHelper.ToRadians(2f),
                true
            );
            GeneralParticleHandler.SpawnParticle(smokeH);
        }
    }
}