using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Particles;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using System;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class FinishingTouchDASHINV : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_89"; // 使用原版光点

        private int trackTimer = 0;
        private bool sticking = false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.extraUpdates = 1;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // === 飞行追踪 FinishingTouchJav ===
            Projectile.velocity *= 1.01f; // 逐渐加速

            if (trackTimer < 60)
            {
                // 计算目标方向（玩家指向鼠标）
                Player player = Main.player[Projectile.owner];
                Vector2 playerPos = player.Center;
                Vector2 mouseWorld = Main.MouseWorld;
                Vector2 directionToMouse = (mouseWorld - playerPos).SafeNormalize(Vector2.UnitY);
                Vector2 targetPosition = playerPos + directionToMouse * 16f * 3f;

                // 计算目标方向向量
                Vector2 toTarget = targetPosition - Projectile.Center;
                float desiredSpeed = Projectile.velocity.Length() + 0.8f;

                // 在前 60 帧内缓慢转向目标方向（实现拐弯）
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget.SafeNormalize(Vector2.UnitY) * desiredSpeed, 0.06f);

                trackTimer++;
            }
            else
            {
                Player player = Main.player[Projectile.owner];
                Vector2 playerPos = player.Center;
                Vector2 mouseWorld = Main.MouseWorld;

                // 计算玩家到鼠标的方向
                Vector2 directionToMouse = (mouseWorld - playerPos).SafeNormalize(Vector2.UnitY);

                // 目标点 = 玩家位置 + (方向 * 16 * 6)
                Vector2 targetPosition = playerPos + directionToMouse * 16f * 3f;

                // 计算方向并平滑追踪
                Vector2 toTarget = targetPosition - Projectile.Center;
                float desiredSpeed = Projectile.velocity.Length() + 0.8f; // 平滑加速
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget.SafeNormalize(Vector2.UnitY) * desiredSpeed, 0.08f);

                // 当接近目标点时自动销毁
                if (toTarget.Length() < 12f) // 接近阈值可微调
                {
                    Projectile.Kill();
                }
            }


            // === 飞行特效 ===
            // Spark 橙色火花
            if (Main.rand.NextBool(3))
            {
                Particle spark = new SparkParticle(
                    Projectile.Center,
                    Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 5f),
                    false,
                    25,
                    Main.rand.NextFloat(0.8f, 1.2f),
                    Color.Orange
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 橙红 Dust
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Torch,
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    100,
                    Color.OrangeRed,
                    Main.rand.NextFloat(0.9f, 1.3f)
                );
                d.noGravity = true;
            }

            // GlowOrb 形成火焰灵动轨迹（螺旋散布感）
            if (Main.GameUpdateCount % 3 == 0)
            {
                float angle = Main.GlobalTimeWrappedHourly * 8f;
                Vector2 offset = angle.ToRotationVector2() * 12f;
                GlowOrbParticle orb = new GlowOrbParticle(
                    Projectile.Center + offset,
                    Vector2.Zero,
                    false,
                    20,
                    0.6f,
                    Color.Orange,
                    true,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 爆发火焰粒子
            for (int i = 0; i < 20; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(4f, 4f));
                d.scale = Main.rand.NextFloat(1.2f, 1.8f);
                d.color = Color.Orange;
            }

            // 爆发火花
            for (int i = 0; i < 8; i++)
            {
                Particle p = new SparkParticle(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(4f, 4f),
                    false,
                    40,
                    1.2f,
                    Color.OrangeRed
                );
                GeneralParticleHandler.SpawnParticle(p);
            }

            // 爆发橙色收缩光环
            Particle ring = new CustomPulse(
                Projectile.Center,
                Vector2.Zero,
                Color.Orange,
                "CalamityMod/Particles/HighResHollowCircleHardEdge",
                Vector2.One,
                Main.rand.NextFloat(-5f, 5f),
                0.05f,
                0.2f,
                20
            );
            GeneralParticleHandler.SpawnParticle(ring);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY;
            Vector2 origin = texture.Size() * 0.5f;

            // Extra_89 多层脉动圆环
            Texture2D ringTex = Terraria.GameContent.TextureAssets.Extra[89].Value;
            float pulse = 1f + 0.08f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f);

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 0.6f;
                Color ringColor = new Color(255, 100, 20, 0) * 1.6f; // 橙红
                float scale = (0.25f + 0.05f * i) * pulse * 2.5f;
                Main.EntitySpriteDraw(
                    ringTex,
                    drawPosition,
                    null,
                    ringColor,
                    angle,
                    ringTex.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            return false;
        }
    }
}
