using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.EndlessDevourJav
{
    internal class EndlessDevourJavOrbSmall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static int MaxUpdate = 7;
        private int Lifetime = 310;

        private static Color ShaderColorOne = new Color(10, 10, 30);   // 深邃夜空蓝黑
        private static Color ShaderColorTwo = new Color(0, 0, 0);      // 纯黑
        private static Color ShaderEndColor = new Color(40, 0, 60);    // 暗紫星光

        private Vector2 altSpawn;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 21;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
            Projectile.MaxUpdates = MaxUpdate;
            Projectile.penetrate = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        public override void AI()
        {
            Projectile.ai[0]++;

            // === 飞行模式切换控制 ===
            // 每 60 帧切换一次模式：
            // 模式 0: 直线飞行（20 帧）
            // 模式 1: 左旋飞行（40 帧）
            int cycleTime = 60;
            int phase = (int)(Projectile.ai[0] % cycleTime);

            if (Projectile.ai[0] <= 180) // 切换阶段总持续 180 帧
            {
                Projectile.friendly = false; // 暂时无伤

                if (phase < 20)
                {
                    // === 模式 0：保持直线飞行
                    // 不修改速度，继续保持原方向
                }
                else
                {
                    // === 模式 1：左旋飞行，每帧左旋 5°
                    Projectile.velocity = Projectile.velocity.RotatedBy(-MathHelper.ToRadians(5));
                }
            }
            else
            {
                // === 切换结束后开始追踪敌人，恢复伤害 ===
                Projectile.friendly = true;

                // 在剩余 80 帧时开始减速
                if (Projectile.timeLeft <= 80)
                    Projectile.velocity *= 0.96f;

                NPC target = Projectile.Center.ClosestNPCAt(1800);
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f);
                }

                // 生成临近死亡的闪光特效
                if (Projectile.timeLeft <= 5)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.DarkCelestial, Projectile.velocity * 0.95f);
                    dust.noGravity = true;
                }
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 center = Projectile.Center;

            // =============== 🌌 特效 1：深紫深蓝星光 SparkParticle 射线 ===============
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(8f, 8f);
                Vector2 velocity = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(6f, 12f);

                Color sparkColor = Color.Lerp(new Color(40, 0, 60), new Color(20, 20, 60), 0.5f);

                Particle spark = new SparkParticle(
                    center + offset,
                    velocity,
                    false,
                    Main.rand.Next(20, 30),
                    Main.rand.NextFloat(0.4f, 0.7f),
                    sparkColor * 0.8f
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // =============== 🌌 特效 2：瞬闪星芒 GenericSparkle 点状爆发 ===============
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 sparkleVelocity = Main.rand.NextVector2Circular(0.5f, 0.5f);

                GenericSparkle sparkle = new GenericSparkle(
                    center + sparkleOffset,
                    sparkleVelocity,
                    Color.White,
                    new Color(60, 0, 100),
                    Main.rand.NextFloat(1.0f, 1.6f),
                    Main.rand.Next(15, 25),
                    Main.rand.NextFloat(-0.05f, 0.05f),
                    1.5f
                );
                GeneralParticleHandler.SpawnParticle(sparkle);
            }

            // =============== 🌌 特效 3：深色迷雾 HeavySmokeParticle 扩散 ===============
            for (int i = 0; i < 4; i++)
            {
                Vector2 smokeVelocity = Main.rand.NextVector2Circular(1f, 1f);

                Particle smoke = new HeavySmokeParticle(
                    center + Main.rand.NextVector2Circular(6f, 6f),
                    smokeVelocity,
                    Color.Lerp(Color.DarkViolet, Color.Black, 0.5f),
                    30,
                    Main.rand.NextFloat(0.3f, 0.5f),
                    0.4f,
                    Main.rand.NextFloat(-0.1f, 0.1f),
                    false
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }


        private float PrimitiveWidthFunction(float completionRatio)
        {
            float arrowheadCutoff = 0.36f;
            float width = 24f;
            float minHeadWidth = 0.03f;
            float maxHeadWidth = width;
            if (completionRatio <= arrowheadCutoff)
                width = MathHelper.Lerp(minHeadWidth, maxHeadWidth, Utils.GetLerpValue(0f, arrowheadCutoff, completionRatio, true));
            return width;
        }

        private Color PrimitiveColorFunction(float completionRatio)
        {
            float endFadeRatio = 0.41f;
            float completionRatioFactor = 2.7f;
            float globalTimeFactor = 5.3f;
            float endFadeFactor = 3.2f;
            float endFadeTerm = Utils.GetLerpValue(0f, endFadeRatio * 0.5f, completionRatio, true) * endFadeFactor;
            float cosArgument = completionRatio * completionRatioFactor - Main.GlobalTimeWrappedHourly * globalTimeFactor + endFadeTerm;
            float startingInterpolant = (float)Math.Cos(cosArgument) * 0.5f + 0.5f;

            float colorLerpFactor = 0.6f;
            Color startingColor = Color.Lerp(ShaderColorOne, ShaderColorTwo, startingInterpolant * colorLerpFactor);
            return Color.Lerp(startingColor, ShaderEndColor, MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0f, endFadeRatio, completionRatio, true)));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/spark_07"));
            Vector2 overallOffset = Projectile.Size * 0.5f;
            overallOffset += Projectile.velocity * 1.4f;
            int numPoints = 46;
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), numPoints);
            return false;
        }


    }

}