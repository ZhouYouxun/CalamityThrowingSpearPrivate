using System;
using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.GalvanizingGlaiveC
{
    public class GalvanizingGlaiveJavGaussFlux : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, new Color(120, 210, 255).ToVector3() * 0.9f);

            // 查找范围内最近的敌人并追踪
            NPC target = Projectile.Center.ClosestNPCAt(1800);
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f);
            }

            // 自身旋转计时
            Projectile.ai[0]++;
            Projectile.rotation += 0.12f;

            Vector2 center = Projectile.Center;

            // ================= 方形收缩科幻结构 =================
            // 呼吸缩放：整体会有收缩/膨胀的科技感
            float pulse = (float)Math.Sin(Projectile.ai[0] * 0.12f) * 0.5f + 0.5f;
            float outerRadius = MathHelper.Lerp(42f, 26f, pulse);
            float innerRadius = MathHelper.Lerp(24f, 14f, pulse);

            // 只有部分帧生成，避免太密
            if (Main.GameUpdateCount % 2 == 0)
            {
                // ---------- 外层旋转方环 ----------
                int outerPoints = 16;
                for (int i = 0; i < outerPoints; i++)
                {
                    float progress = (float)i / outerPoints;
                    Vector2 offset = GetSquareLoopOffset(progress, outerRadius).RotatedBy(Projectile.ai[0] * 0.05f);
                    Vector2 pos = center + offset;

                    Dust dust = Dust.NewDustPerfect(pos, 261);
                    dust.color = Color.Lerp(Color.Cyan, Color.BlueViolet, 0.35f + 0.35f * (float)Math.Sin(progress * MathHelper.TwoPi + Projectile.ai[0] * 0.08f));
                    dust.velocity = offset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * 0.45f;
                    dust.scale = 1.05f + 0.35f * (float)Math.Sin(progress * MathHelper.TwoPi * 2f + Projectile.ai[0] * 0.09f);
                    dust.noGravity = true;
                }

                // ---------- 内层反向旋转方环 ----------
                int innerPoints = 12;
                for (int i = 0; i < innerPoints; i++)
                {
                    float progress = (float)i / innerPoints;
                    Vector2 offset = GetSquareLoopOffset(progress, innerRadius).RotatedBy(-Projectile.ai[0] * 0.075f + MathHelper.PiOver4);
                    Vector2 pos = center + offset;

                    Dust dust = Dust.NewDustPerfect(pos, 226);
                    dust.color = Utils.SelectRandom(Main.rand, Color.White, Color.LightBlue, new Color(170, 220, 255));
                    dust.velocity = Vector2.Zero;
                    dust.scale = 0.95f + 0.2f * (float)Math.Cos(progress * MathHelper.TwoPi + Projectile.ai[0] * 0.1f);
                    dust.noGravity = true;
                }
            }

            // ================= 圆形辅助旋流 =================
            // 保留“转圈”的感觉，但不再是原来那种简单堆圈
            if (Main.GameUpdateCount % 3 == 0)
            {
                int orbitCount = 6;
                float orbitRadius = MathHelper.Lerp(10f, 20f, 1f - pulse);

                for (int i = 0; i < orbitCount; i++)
                {
                    float angle = Projectile.ai[0] * 0.14f + MathHelper.TwoPi * i / orbitCount;
                    Vector2 offset = angle.ToRotationVector2() * orbitRadius;
                    Vector2 pos = center + offset;

                    Dust dust = Dust.NewDustPerfect(pos, 135);
                    dust.color = Utils.SelectRandom(Main.rand, Color.AliceBlue, Color.CornflowerBlue, Color.Cyan);
                    dust.velocity = offset.SafeNormalize(Vector2.UnitX) * 0.25f;
                    dust.scale = 0.9f;
                    dust.noGravity = true;
                }
            }

            // ================= 中心脉冲粒子 =================
            if (Main.rand.NextBool(3))
            {
                Particle pulseParticle = new CustomPulse(
                    center,
                    Vector2.Zero,
                    Color.Lerp(Color.LightBlue, Color.BlueViolet, Main.rand.NextFloat()) * 0.75f,
                    "CalamityMod/Particles/HighResFoggyCircleHardEdge",
                    Vector2.One * MathHelper.Lerp(0.35f, 0.6f, pulse),
                    Main.rand.NextFloat(-3f, 3f),
                    0.01f,
                    0.08f,
                    10
                );
                GeneralParticleHandler.SpawnParticle(pulseParticle);
            }

            // ================= 前进方向细碎喷流 =================
            if (Main.rand.NextBool(2))
            {
                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);

                Particle spark = new SparkParticle(
                    center + forward * 6f,
                    forward.RotatedByRandom(MathHelper.ToRadians(18f)) * Main.rand.NextFloat(2.5f, 5.5f),
                    false,
                    Main.rand.Next(12, 18),
                    Main.rand.NextFloat(0.5f, 0.8f),
                    Color.Lerp(Color.LightBlue, Color.BlueViolet, Main.rand.NextFloat())
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D squareTex = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSquareParticleBig").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = (float)Math.Sin(Projectile.ai[0] * 0.12f) * 0.5f + 0.5f;
            float shrinkA = MathHelper.Lerp(1.15f, 0.55f, pulse);
            float shrinkB = MathHelper.Lerp(0.95f, 0.35f, pulse);

            Color drawColorA = Color.Lerp(Color.Cyan, Color.BlueViolet, 0.35f) with { A = 0 };
            Color drawColorB = Color.Lerp(Color.White, Color.LightBlue, 0.55f) with { A = 0 };

            // 外层大方形：旋转 + 收缩
            Main.EntitySpriteDraw(
                squareTex,
                drawPos,
                null,
                drawColorA * 0.42f,
                Projectile.ai[0] * 0.055f + MathHelper.PiOver4,
                squareTex.Size() * 0.5f,
                shrinkA,
                SpriteEffects.None
            );

            // 内层反向方形：反向旋转 + 更强收缩
            Main.EntitySpriteDraw(
                squareTex,
                drawPos,
                null,
                drawColorB * 0.32f,
                -Projectile.ai[0] * 0.085f,
                squareTex.Size() * 0.5f,
                shrinkB,
                SpriteEffects.None
            );

            // 中心核心小方块
            Main.EntitySpriteDraw(
                squareTex,
                drawPos,
                null,
                Color.White with { A = 0 } * 0.18f,
                Projectile.ai[0] * 0.12f + MathHelper.PiOver4,
                squareTex.Size() * 0.5f,
                0.22f + 0.05f * (float)Math.Sin(Projectile.ai[0] * 0.2f),
                SpriteEffects.None
            );

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300);
        }

        // 在正方形边框上按 progress(0~1) 取点
        private Vector2 GetSquareLoopOffset(float progress, float radius)
        {
            progress %= 1f;
            if (progress < 0.25f)
            {
                float t = progress / 0.25f;
                return new Vector2(MathHelper.Lerp(-radius, radius, t), -radius);
            }
            if (progress < 0.5f)
            {
                float t = (progress - 0.25f) / 0.25f;
                return new Vector2(radius, MathHelper.Lerp(-radius, radius, t));
            }
            if (progress < 0.75f)
            {
                float t = (progress - 0.5f) / 0.25f;
                return new Vector2(MathHelper.Lerp(radius, -radius, t), radius);
            }

            float t2 = (progress - 0.75f) / 0.25f;
            return new Vector2(-radius, MathHelper.Lerp(radius, -radius, t2));
        }
    }
}