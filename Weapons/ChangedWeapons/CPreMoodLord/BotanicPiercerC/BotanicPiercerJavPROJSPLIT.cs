using CalamityMod.Particles;
using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.BotanicPiercerC
{
    public class BotanicPiercerJavPROJSPLIT : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";

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
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 300;
        }
        private List<AltSparkParticle> ownedAltSparkParticles = new();

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            Projectile.ai[0]++;

            // 在前6帧内直线飞行
            if (Projectile.ai[0] <= 15)
            {
                return;
            }

            // 6帧后开始追踪最近的敌人
            NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f); // 追踪速度为12f
            }

            // 每5帧生成一个翠绿色的尖锥粒子
            if (Projectile.ai[0] % 5 == 0)
            {
                PointParticle spark = new PointParticle(Projectile.Center, -Projectile.velocity * 0.5f, false, 5, 1.1f, Color.LimeGreen);
                GeneralParticleHandler.SpawnParticle(spark);
            }


            // 🌿 飞行期间持续生成 AltSparkParticle 尾迹
            if (Projectile.ai[0] % 2 == 0) // 每 2 帧生成一次
            {
                AltSparkParticle tail = new AltSparkParticle(
                    Projectile.Center - Projectile.velocity * 1.5f,
                    Projectile.velocity * 0.02f,
                    false,
                    20,  // 稍长寿命以便观察
                    1.2f,
                    Color.LimeGreen * 1.18f
                );
                GeneralParticleHandler.SpawnParticle(tail);
                ownedAltSparkParticles.Add(tail);
            }

            for (int i = ownedAltSparkParticles.Count - 1; i >= 0; i--)
            {
                AltSparkParticle p = ownedAltSparkParticles[i];

                if (p.Time >= p.Lifetime)
                {
                    ownedAltSparkParticles.RemoveAt(i);
                    continue;
                }

                // === 🌿 轨迹复杂化：自然离谱草木灵息尾迹飞行 ===

                // 持续右拐
                p.Velocity = p.Velocity.RotatedBy(MathHelper.ToRadians(2f));

                // 呼吸式加速减速
                float cycle = 24f;
                float scaleFactor = 1f + 0.05f * (float)Math.Sin(MathHelper.TwoPi * p.Time / cycle);
                p.Velocity *= scaleFactor;

                // 周期性轻微脉冲
                if (p.Time % 12 == 0)
                {
                    p.Velocity *= 1.15f;
                }

                // 每 8 帧 ±0.8° 微摆
                if (p.Time % 8 < 4)
                {
                    p.Velocity = p.Velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-0.8f, 0.8f)));
                }
            }



        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ========== 原三角阵 Dust ========== 
            int trianglePoints = 3;
            for (int i = 0; i < trianglePoints; i++)
            {
                float angle = MathHelper.TwoPi * i / trianglePoints;

                for (int j = 0; j < 12; j++)
                {
                    float speed = MathHelper.Lerp(1f, 7f, j / 12f) * Main.rand.NextFloat(0.9f, 1.1f);
                    Color particleColor = Main.rand.NextBool() ? Color.LimeGreen : Color.LightGreen;
                    float scale = MathHelper.Lerp(1.6f, 0.85f, j / 12f) * Main.rand.NextFloat(0.95f, 1.05f);

                    Dust magicDust = Dust.NewDustPerfect(
                        Projectile.Center,
                        107,
                        angle.ToRotationVector2() * speed,
                        100,
                        particleColor,
                        scale
                    );
                    magicDust.noGravity = true;
                    magicDust.fadeIn = Main.rand.NextFloat(0.6f, 1.2f);
                }
            }

            // ========== 原环形 Dust 扩散 ==========
            int ovalPoints = 42;
            for (int i = 0; i < ovalPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / ovalPoints;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 7f);

                Dust ringDust = Dust.NewDustPerfect(
                    Projectile.Center,
                    107,
                    velocity,
                    100,
                    Color.Lerp(Color.LimeGreen, Color.White, Main.rand.NextFloat(0.0f, 0.3f)),
                    Main.rand.NextFloat(0.95f, 1.3f)
                );
                ringDust.noGravity = true;
                ringDust.fadeIn = Main.rand.NextFloat(0.5f, 1f);
            }

            // ========== 新增自定义粒子：DirectionalPulseRing ==========
            Particle pulse = new DirectionalPulseRing(
                Projectile.Center,
                Vector2.Zero,
                Color.LightGreen * 0.8f,
                new Vector2(1f, 1f),
                0f,
                0.2f,
                0.9f,
                20
            );
            GeneralParticleHandler.SpawnParticle(pulse);

            // ========== 新增自定义粒子：PointParticle ==========
            int pointCount = 12;
            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);

                Particle point = new PointParticle(
                    Projectile.Center,
                    velocity,
                    false,
                    18,
                    1.1f,
                    Color.Lerp(Color.LawnGreen, Color.YellowGreen, 0.5f)
                );
                GeneralParticleHandler.SpawnParticle(point);
            }
        }








    }
}
