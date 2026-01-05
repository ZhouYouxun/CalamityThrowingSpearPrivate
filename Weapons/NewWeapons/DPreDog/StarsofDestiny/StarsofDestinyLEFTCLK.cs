using CalamityMod;
using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    // 左键爆炸后生成的“时间领域”中心钟盘
    public class StarsofDestinyLEFTCLK : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24 * 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 60; // 后续可调
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 59;
        }

        // 用于旋转粒子系统的角度累计
        private float clockRotation = 0f;

        public override void AI()
        {
            // 固定不动
            Projectile.velocity = Vector2.Zero;

            // 累计角度，让钟盘每帧旋转
            clockRotation += 0.05f;

            float innerRadius = 10 * 16;
            float outerRadius = 12 * 16;

            // ◆ 1. 内圈（旋转）→ GlowOrbParticle
            for (int i = 0; i < 18; i++)
            {
                float angle = (MathHelper.TwoPi / 18 * i) + clockRotation;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * innerRadius;

                GlowOrbParticle orb = new GlowOrbParticle(
                    pos,
                    Vector2.Zero,
                    false,
                    6,
                    1.1f,
                    Color.White,
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }

            // ◆ 2. 外环（旋转）→ GlowOrbParticle
            for (int i = 0; i < 26; i++)
            {
                float angle = (MathHelper.TwoPi / 26 * i) + clockRotation * 1.2f;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * outerRadius;

                GlowOrbParticle orb = new GlowOrbParticle(
                    pos,
                    Vector2.Zero,
                    false,
                    6,
                    1.0f,
                    Color.LightYellow,
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }

            // ◆ 3. 刻度线（固定位置，不旋转）→ GlowOrbParticle
            int tickNum = 12;
            for (int i = 0; i < tickNum; i++)
            {
                float angle = MathHelper.TwoPi / tickNum * i;
                Vector2 start = Projectile.Center + angle.ToRotationVector2() * innerRadius;
                Vector2 end = Projectile.Center + angle.ToRotationVector2() * outerRadius;

                int seg = 6;
                for (int j = 0; j < seg; j++)
                {
                    float t = j / (float)(seg - 1);
                    Vector2 pos = Vector2.Lerp(start, end, t);

                    GlowOrbParticle orb = new GlowOrbParticle(
                        pos,
                        Vector2.Zero,
                        false,
                        6,
                        1.15f,
                        Color.LightYellow,
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }

            // ◆ 4. 时针与分针（每粒子点生成 INV）
            float shortHand = innerRadius * 0.5f;
            float longHand = innerRadius * 0.9f;

            CreateClockHand(shortHand, clockRotation * 1.6f, Color.Yellow);
            CreateClockHand(longHand, clockRotation * 2.8f, Color.White);

            // ◆ 5. 星光（装饰）→ GlowOrbParticle
            if (Main.rand.NextBool(6))
            {
                Vector2 p = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
                GlowOrbParticle orb = new GlowOrbParticle(
                    p,
                    (Projectile.Center - p).SafeNormalize(Vector2.Zero) * 0.2f,
                    false,
                    8,
                    Main.rand.NextFloat(1f, 1.4f),
                    Color.White,
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }
        private bool hasSpawnedCLK50 = false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasSpawnedCLK50)
                return;

            {

                Player owner = Main.player[Projectile.owner];

                // ☆ 查找玩家周围是否已经有属于这个玩家的 CLK50
                bool alreadyHasOne = false;
                int clkCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active &&
                        p.owner == owner.whoAmI &&
                        p.type == ModContent.ProjectileType<SODCLK50>())
                    {
                        clkCount++;
                        if (clkCount >= 12) break;
                    }
                }


                if (clkCount < 12)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_OnHit(target),
                        owner.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<SODCLK50>(),
                        Projectile.damage,
                        0f,
                        owner.whoAmI
                    );
                    // ★ 标记为已触发
                    hasSpawnedCLK50 = true;
                }
            }
        }

        // 时针 / 分针
        private void CreateClockHand(float length, float angle, Color color)
        {
            Vector2 dir = angle.ToRotationVector2();
            int width = Main.rand.Next(1, 3);

            for (int j = 0; j < width; j++)
            {
                for (float k = 0f; k < 1f; k += 0.10f)
                {
                    Vector2 pos = Projectile.Center + dir * (length * k) + Main.rand.NextVector2Circular(4f, 4f);

                    // GlowOrbParticle 替换原 Dust
                    GlowOrbParticle orb = new GlowOrbParticle(
                        pos,
                        Vector2.Zero,
                        false,
                        6,
                        1.4f,
                        color,
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);

                    // INV 伤害粒子
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            pos,
                            Vector2.Zero,
                            ModContent.ProjectileType<StarsofDestinyINV>(),
                            (int)(Projectile.damage * 0.6),
                            0f,
                            Projectile.owner
                        );
                    }
                }
            }
        }
    }
}
