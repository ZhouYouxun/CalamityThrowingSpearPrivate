using CalamityMod.Buffs.DamageOverTime;
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
using CalamityMod.Projectiles.Typeless;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    internal class SunsetASunsetLeft : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
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
            Projectile.penetrate = 3;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 获取枪头位置
            Vector2 gunHeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 0.35f;

            {
                if (Main.rand.NextBool(2)) // 控制生成频率
                {
                    // === 能量雾化粒子（WaterFlavoredParticle） ===
                    float angle = Projectile.ai[0] * 0.1f + Main.GameUpdateCount * 0.2f;
                    Vector2 offset = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle)) * 6f;

                    WaterFlavoredParticle mist = new WaterFlavoredParticle(
                        Projectile.Center + offset,                    // 中心 + 数学扰动
                        -Projectile.velocity * 0.1f,                   // 轻微反向速度，制造扩散感
                        false,                                         // 不受重力
                        Main.rand.Next(18, 26),                        // 生命周期
                        0.9f + Main.rand.NextFloat(0.3f),              // 缩放
                        Color.Gold * Main.rand.NextFloat(0.8f, 1.0f)   // 能量黄色
                    );
                    GeneralParticleHandler.SpawnParticle(mist);

                    // === 高能火花（PointParticle） ===
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 sparkVel = (-Projectile.velocity * 0.5f)
                                           .RotatedByRandom(MathHelper.ToRadians(12)); // 扰动角度

                        PointParticle spark = new PointParticle(
                            Projectile.Center,
                            sparkVel,
                            false,
                            15,
                            1.0f + Main.rand.NextFloat(0.3f),
                            Main.rand.NextBool() ? Color.Orange : Color.Yellow
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }

            }

            // 计算双螺旋特效（仍然使用 Dust）
            float phaseShift = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 1f;
            Vector2 leftOffset = Projectile.velocity.RotatedBy(MathHelper.PiOver2) * phaseShift;
            Vector2 rightOffset = Projectile.velocity.RotatedBy(-MathHelper.PiOver2) * phaseShift;

            // 生成 Dust（YellowTorch, IchorTorch）
            int dustType = Main.rand.NextBool() ? DustID.YellowTorch : DustID.IchorTorch;
            Dust.NewDustPerfect(Projectile.Center + leftOffset, dustType, -Projectile.velocity * 0.3f, Scale: 1.2f);
            Dust.NewDustPerfect(Projectile.Center + rightOffset, dustType, -Projectile.velocity * 0.3f, Scale: 1.2f);
        }

        public override void OnKill(int timeLeft)
        {


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 explosionPosition = Projectile.Center;

            // 生成爆炸粒子
            Particle explosion = new DetailedExplosion(
                explosionPosition,
                Vector2.Zero,
                Color.OrangeRed * 0.9f,
                Vector2.One,
                Main.rand.NextFloat(-5, 5),
                0.1f * 2.5f, // 修改原始大小
                0.28f * 2.5f, // 修改最终大小
                10
            );
            GeneralParticleHandler.SpawnParticle(explosion);

            {
                Vector2 pos = Projectile.Center;

                // ================= 爆心闪光 =================
                for (int i = 0; i < 8; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                    SparkleParticle sparkle = new SparkleParticle(
                        pos + offset,
                        Vector2.Zero,
                        Color.Gold,                // 主色：金黄
                        Color.OrangeRed,           // 边缘：橙红
                        2.0f + Main.rand.NextFloat(0.5f),
                        10 + Main.rand.Next(5),
                        Main.rand.NextFloat(-0.05f, 0.05f),
                        2.2f
                    );
                    GeneralParticleHandler.SpawnParticle(sparkle);
                }

                // ================= 魔法阵外围光点 =================
                int ringCount = 12; // 环绕光点数量
                float radius = 48f;
                for (int i = 0; i < ringCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringCount;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    SparkleParticle sparkle = new SparkleParticle(
                        pos + offset,
                        Vector2.Zero,
                        Color.WhiteSmoke,
                        Color.Orange,
                        0.6f,
                        14,
                        0f,
                        1.8f
                    );
                    GeneralParticleHandler.SpawnParticle(sparkle);
                }



                // ================= 轻型烟雾环绕 =================
                int lightSmokeCount = 30;
                float lightRadius = 64f; // 初始生成半径更大
                for (int i = 0; i < lightSmokeCount; i++)
                {
                    // 在更大范围的圆环内生成
                    Vector2 spawnOffset = Main.rand.NextVector2Circular(lightRadius, lightRadius);
                    Vector2 spawnPos = pos + spawnOffset;

                    // 速度 = 径向向外 + 随机扰动
                    Vector2 velocity = spawnOffset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1f, 3f)
                                       + Main.rand.NextVector2Circular(0.5f, 0.5f);

                    Particle smokeL = new HeavySmokeParticle(
                        spawnPos,
                        velocity,
                        Color.Yellow,
                        26, // 稍微延长寿命
                        Main.rand.NextFloat(1.2f, 1.8f),
                        0.35f,
                        Main.rand.NextFloat(-1f, 1f),
                        false // ❌ 轻烟
                    );
                    GeneralParticleHandler.SpawnParticle(smokeL);
                }

                // ================= 重型烟雾冲击 =================
                int heavySmokeCount = 15;
                float heavyRadius = 32f;
                for (int i = 0; i < heavySmokeCount; i++)
                {
                    // 半径更大，像“环形冲击”
                    Vector2 spawnOffset = Main.rand.NextVector2Circular(heavyRadius, heavyRadius);
                    Vector2 spawnPos = pos + spawnOffset;

                    // 径向外扩 + 上飘
                    Vector2 velocity = spawnOffset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(2f, 5f)
                                       + new Vector2(0f, -Main.rand.NextFloat(1f, 2.5f));

                    Particle smokeH = new HeavySmokeParticle(
                        spawnPos,
                        velocity,
                        Color.Gray,
                        36, // 更长寿命
                        Projectile.scale * Main.rand.NextFloat(1.0f, 1.6f), // 更大尺寸
                        1.0f,
                        MathHelper.ToRadians(Main.rand.NextFloat(1.5f, 3.5f)),
                        true // ✅ 重烟
                    );
                    GeneralParticleHandler.SpawnParticle(smokeH);
                }



            }

            // 生成爆炸弹幕
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                explosionPosition,
                Vector2.Zero,
                ModContent.ProjectileType<FuckYou>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );

            target.AddBuff(ModContent.BuffType<SunsetASunsetEDebuff>(), 300); // 300 帧 = 5 秒
        }
    }
}