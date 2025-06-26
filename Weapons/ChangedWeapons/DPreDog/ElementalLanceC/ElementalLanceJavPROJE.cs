using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod;
using CalamityMod.Particles;
using Terraria.ID;
using System;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    public class ElementalLanceJavPROJE : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private NPC target;
        private int currentCountdown = 5;
        private static readonly bool[][,] DigitMasks = new bool[][,]
        {
            // Digit 5
            new bool[,]
            {
                { true, true, true, true, true },
                { true, false, false, false, false },
                { true, true, true, true, false },
                { false, false, false, false, true },
                { false, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false },
            },
            // Digit 4
            new bool[,]
            {
                { false, false, false, true, false },
                { false, false, true, true, false },
                { false, true, false, true, false },
                { true, false, false, true, false },
                { true, true, true, true, true },
                { false, false, false, true, false },
                { false, false, false, true, false },
            },
            // Digit 3
            new bool[,]
            {
                { true, true, true, true, false },
                { false, false, false, false, true },
                { false, false, true, true, false },
                { false, false, false, false, true },
                { false, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false },
            },
            // Digit 2
            new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { false, false, false, false, true },
                { false, false, false, true, false },
                { false, false, true, false, false },
                { false, true, false, false, false },
                { true, true, true, true, true },
            },
            // Digit 1
            new bool[,]
            {
                { false, false, true, false, false },
                { false, true, true, false, false },
                { true, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { true, true, true, true, true },
            }
        };
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void OnSpawn(IEntitySource source)
        {
            float minDist = float.MaxValue;
            NPC closest = null;
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = npc;
                    }
                }
            }

            if (closest != null)
                target = closest;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 59)
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.timeLeft <= 2)
            {
                Projectile.friendly = false;
            }
            if (target == null || !target.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = target.Center;

            int timeLeft = Projectile.timeLeft;
            int newCountdown = 0;
            if (timeLeft >= 300) newCountdown = 5;
            else if (timeLeft >= 240) newCountdown = 4;
            else if (timeLeft >= 180) newCountdown = 3;
            else if (timeLeft >= 120) newCountdown = 2;
            else if (timeLeft >= 60) newCountdown = 1;

            if (newCountdown != currentCountdown)
            {
                currentCountdown = newCountdown;
                DrawDigit(currentCountdown);
            }
        }

        private void DrawDigit(int digit)
        {
            digit = Math.Clamp(digit, 1, 5);
            Vector2 center = Projectile.Center - new Vector2(0, 50f);
            bool[,] mask = DigitMasks[digit - 1];

            float pixelSize = 24f; // 整体大小
            for (int y = 0; y < mask.GetLength(0); y++)
            {
                for (int x = 0; x < mask.GetLength(1); x++)
                {
                    if (!mask[y, x]) continue;

                    Vector2 offset = new Vector2((x - 1) * pixelSize, (y - 2) * pixelSize);
                    Vector2 pos = center + offset + Main.rand.NextVector2Circular(1.2f, 1.2f);
                    Vector2 vel = Main.rand.NextVector2Circular(0.1f, 0.4f);

                    Dust d = Dust.NewDustPerfect(pos, DustID.Shadowflame, vel, 0, Color.Black, 2.2f);
                    d.noGravity = true;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            Particle boom = new CustomPulse(
                Projectile.Center,
                Vector2.Zero,
                Color.Black,
                "CalamityMod/Particles/DetailedExplosion",
                Vector2.One * 2.2f,
                Main.rand.NextFloat(-15f, 15f),
                0.14f,
                0.95f,
                32,
                false
            );
            GeneralParticleHandler.SpawnParticle(boom);

            // 大爆炸 Dust 效果
            int particleCount = Main.rand.Next(180, 240);
            float baseExplosionForce = 6.5f;
            float divergenceFactor = 1.5f;
            Vector2 explosionOrigin = Projectile.Center;

            for (int i = 0; i < particleCount; i++)
            {
                int dustType = DustID.Shadowflame;
                float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                float divergenceStrength = baseExplosionForce + Main.rand.NextFloat(-3f, 5f);
                float spread = Main.rand.NextFloat(0.5f, 1.5f);

                Vector2 divergence = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * divergenceStrength;
                divergence += divergenceFactor * divergence.Length() * new Vector2(
                    (float)Math.Cos(angle + MathHelper.PiOver4),
                    (float)Math.Sin(angle + MathHelper.PiOver4)
                );

                Dust dust = Dust.NewDustPerfect(explosionOrigin, dustType, divergence * spread, 0, Color.Black, Main.rand.NextFloat(0.72f, 1.55f)); // 最后一个只是寿命
                dust.noGravity = Main.rand.NextBool();
                dust.fadeIn = Main.rand.NextFloat(0.5f, 1.5f);
            }

            // 黑色线性火花
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Particle trail = new SparkParticle(Projectile.Center, vel * 0.2f, false, 60, 1.3f, Color.Black);
                GeneralParticleHandler.SpawnParticle(trail);
            }

            // 黑色方形粒子
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                SquareParticle p = new SquareParticle(Projectile.Center, vel, false, 30, 1.7f + Main.rand.NextFloat(0.6f), Color.Black);
                GeneralParticleHandler.SpawnParticle(p);
            }

            // 黑色烟雾粒子
            for (int i = 0; i < 24; i++)
            {
                Particle smokeL = new HeavySmokeParticle(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(2f, 2f),
                    Color.Black,
                    18,
                    Main.rand.NextFloat(0.9f, 1.6f),
                    0.35f,
                    Main.rand.NextFloat(-1, 1),
                    false
                );
                GeneralParticleHandler.SpawnParticle(smokeL);
            }

            float shakePower = 5f;
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
        }






    }
}
