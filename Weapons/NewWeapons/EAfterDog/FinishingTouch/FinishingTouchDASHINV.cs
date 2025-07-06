using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Particles;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using System;
using Terraria.DataStructures;

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
            Projectile.timeLeft = 220;
            Projectile.extraUpdates = 1;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;
        }
        // 在类内声明用于唯一化每个弹幕的李萨如参数：
        private float phaseOffset;
        private float freqOffset;

        public override void OnSpawn(IEntitySource source)
        {
            phaseOffset = Main.rand.NextFloat(0f, MathHelper.TwoPi);
            freqOffset = Main.rand.NextFloat(0.85f, 1.15f);

            // 给每个弹幕分配唯一索引（建议通过 Projectile.whoAmI % 总数量 或直接分配）
            orbitIndex = Main.rand.Next(0, 12); // 若生成12个则在生成时传入索引以实现严格平分
        }
        private int orbitIndex; // 在类内添加



        public override void AI()
        {
            // === 飞行追踪 FinishingTouchJav ===
            Projectile.velocity *= 1.01f; // 逐渐加速

            if (trackTimer < 60)
            {
                Player player = Main.player[Projectile.owner];
                Vector2 playerPos = player.Center;
                Vector2 mouseWorld = Main.MouseWorld;
                Vector2 directionToMouse = (mouseWorld - playerPos).SafeNormalize(Vector2.UnitY);
                Vector2 targetPosition = playerPos + directionToMouse * 16f * 3f;

                Vector2 toTarget = targetPosition - Projectile.Center;
                float desiredSpeed = Projectile.velocity.Length() + 0.8f;

                // === 李萨如轨迹偏移（确保每个弹幕独立） ===
                float t = Main.GameUpdateCount * 0.15f * freqOffset;
                float A = 24f; // 横向振幅（适度）
                float B = 16f; // 纵向振幅
                float a = 2f;
                float b = 3f;

                Vector2 lissOffset = new Vector2(
                    A * (float)Math.Sin(a * t + phaseOffset),
                    B * (float)Math.Sin(b * t)
                );

                // 综合目标位置加李萨如偏移形成动态追踪
                Vector2 dynamicTarget = targetPosition + lissOffset;
                Vector2 dynamicToTarget = dynamicTarget - Projectile.Center;

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dynamicToTarget.SafeNormalize(Vector2.UnitY) * desiredSpeed, 0.06f);

                trackTimer++;
            }
            else
            {
                Player player = Main.player[Projectile.owner];

                // 同步速度
                Projectile.velocity = player.velocity;

                // === 椭圆公转环绕枪头 ===
                float angle = Main.GameUpdateCount * 0.1f + orbitIndex * MathHelper.TwoPi / 12f; // 平分12个弹幕
                float radiusX = 10f * 16f; // X轴半径
                float radiusY = 10f * 16f; // Y轴半径
                Vector2 orbitOffset = new Vector2(
                    radiusX * (float)Math.Cos(angle),
                    radiusY * (float)Math.Sin(angle)
                );

                Projectile.Center = player.Center + orbitOffset;
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
            for (int i = 0; i < 18; i++)
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
            //Particle ring = new CustomPulse(
            //    Projectile.Center,
            //    Vector2.Zero,
            //    Color.Orange,
            //    "CalamityMod/Particles/HighResHollowCircleHardEdge",
            //    Vector2.One,
            //    Main.rand.NextFloat(-5f, 5f),
            //    0.05f,
            //    0.2f,
            //    20
            //);
            //GeneralParticleHandler.SpawnParticle(ring);
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
