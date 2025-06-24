using CalamityMod.Dusts;
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

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    internal class StarsofDestinyLSTAR : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        private int noTileHitCounter = 90;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public ref float Time => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.alpha = 50;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            {
                // 🌌 彗星轨迹扰动粒子（核心 + 扩散）
                if (Main.rand.NextBool(1)) // 仍保持每帧最多一次
                {
                    // 主方形粒子（中心）
                    SquareParticle sq = new SquareParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                        Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                        false,
                        Main.rand.Next(18, 32),
                        Main.rand.NextFloat(0.8f, 1.3f),
                        Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat()) * 0.6f
                    );
                    GeneralParticleHandler.SpawnParticle(sq);

                    // 每隔3~6帧，触发扩散小环（不全时间激活，控制负载）
                    if (Projectile.timeLeft % Main.rand.Next(3, 7) == 0)
                    {
                        int dustCount = 4;
                        float radius = Main.rand.NextFloat(8f, 14f);
                        for (int i = 0; i < dustCount; i++)
                        {
                            float angle = MathHelper.TwoPi / dustCount * i + Main.rand.NextFloat(-0.2f, 0.2f); // 微乱角
                            Vector2 offset = angle.ToRotationVector2() * radius;
                            Vector2 velocity = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.4f, 1.2f);

                            Dust d = Dust.NewDustPerfect(
                                Projectile.Center + offset,
                                DustID.Electric,
                                velocity,
                                100,
                                Color.LightBlue * 0.8f,
                                Main.rand.NextFloat(0.9f, 1.3f)
                            );
                            d.noGravity = true;
                            d.fadeIn = 1.1f;
                        }
                    }
                }


                // 🌠 彗星飞行路径中的 “恒星闪烁粒子”
                if (Main.rand.NextBool(2))
                {
                    Vector2 spawnOffset = Main.rand.NextVector2Circular(12f, 12f);
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + spawnOffset,
                        DustID.FireworkFountain_Yellow,
                        Vector2.Zero,
                        120,
                        Color.Gold,
                        Main.rand.NextFloat(0.9f, 1.2f)
                    );
                    d.fadeIn = 1.3f;
                    d.noGravity = true;
                }

            }


            if (Projectile.ai[0] == 3f)
                CalamityUtils.HomeInOnNPC(Projectile, true, 300f, 12f, 20);

            noTileHitCounter -= 1;
            if (noTileHitCounter == 0)
                Projectile.tileCollide = true;

            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 20 + Main.rand.Next(40);
                //if (Main.rand.NextBool(5))
                //SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
            }

            Projectile.alpha -= 15;
            int alphaControl = 150;
            if (Projectile.Center.Y >= Projectile.ai[1])
                alphaControl = 0;
            if (Projectile.alpha < alphaControl)
                Projectile.alpha = alphaControl;

            Projectile.localAI[0] += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * Projectile.direction;
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * Projectile.direction;

            if (Main.rand.NextBool(16))
            {
                Vector2 rotational = Vector2.UnitX.RotatedByRandom(1.5707963705062866).RotatedBy((double)Projectile.velocity.ToRotation(), default);
                int astralDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default, 1f);
                Main.dust[astralDust].velocity = rotational * 0.66f;
                Main.dust[astralDust].position = Projectile.Center + rotational * 12f;
            }

            if (Main.rand.NextBool(48) && Main.netMode != NetmodeID.Server)
            {
                int starry = Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f), 16, 1f);
                Main.gore[starry].velocity *= 0.66f;
                Main.gore[starry].velocity += Projectile.velocity * 0.3f;
            }

            if (Projectile.ai[1] == 1f)
            {
                Projectile.light = 0.9f;
                if (Main.rand.NextBool(10))
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default, 1f);
                if (Main.rand.NextBool(20) && Main.netMode != NetmodeID.Server)
                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, new Vector2(Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f), Main.rand.Next(16, 18), 1f);
            }







            Time++;
        }

        //public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        //{
        //    if (Projectile.DamageType != DamageClass.Ranged)
        //        target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 120);
        //}

        //public override void OnHitPlayer(Player target, Player.HurtInfo info)
        //{
        //    if (Projectile.DamageType != DamageClass.Ranged)
        //        target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 120);
        //}

        public override Color? GetAlpha(Color lightColor) => new Color(200, 100, 250, Projectile.alpha);

        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[0] == 1f)
                return;

            Projectile.position.X = Projectile.position.X + Projectile.width / 2;
            Projectile.position.Y = Projectile.position.Y + Projectile.height / 2;
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.position.X = Projectile.position.X - Projectile.width / 2;
            Projectile.position.Y = Projectile.position.Y - Projectile.height / 2;
            for (int i = 0; i < 5; i++)
            {
                int starryDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 1.2f);
                Main.dust[starryDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[starryDust].scale = 0.5f;
                    Main.dust[starryDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 5; j++)
            {
                int starryDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 1.7f);
                Main.dust[starryDust2].noGravity = true;
                Main.dust[starryDust2].velocity *= 5f;
                starryDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 1f);
                Main.dust[starryDust2].velocity *= 2f;
            }
            if (Main.netMode != NetmodeID.Server)
            {
                for (int k = 0; k < 3; k++)
                    Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, new Vector2(Projectile.velocity.X * 0.05f, Projectile.velocity.Y * 0.05f), Main.rand.Next(16, 18), 1f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.DrawStarTrail(Color.Coral, Color.White);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 2);
            return false;
        }
        public override bool? CanDamage() => Time >= 12f; // 初始的时候不会造成伤害，直到12为止

    }
}
