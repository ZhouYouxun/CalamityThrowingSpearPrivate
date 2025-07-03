using CalamityMod.Graphics.Primitives;
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
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ViolenceC
{
    public class ViolenceJavLight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static int MaxUpdate = 7;
        private int Lifetime = 1100;

        private static Color ShaderColorOne = Color.DarkRed;
        private static Color ShaderColorTwo = Color.Black;
        private static Color ShaderEndColor = Color.MediumVioletRed;

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
            // 递增 AI 计数器，用于控制弹幕行为的时间点
            Projectile.ai[0]++;

            // 前 120 帧不会追踪敌人，也不会造成伤害，只是按照初始方向飞行
            if (Projectile.ai[0] <= 120)
            {
                Projectile.friendly = false;

                float time = Projectile.ai[0];
                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                float speed = Projectile.velocity.Length();

                // === 正弦抖动偏移形成蛇形 ===
                float waveFrequency = 0.15f + (time % 60f) / 600f; // 每 60 帧微调频率
                float waveAmplitude = 0.5f + (time % 60f) / 60f * 0.3f; // 每 60 帧振幅从 0.5 -> 0.8

                // 使用 sin(time) 制造横向漂移
                Vector2 waveOffset = forward.RotatedBy(MathHelper.PiOver2) * (float)Math.Sin(time * waveFrequency) * waveAmplitude;

                // === 微弱螺旋式转动叠加 ===
                float spiralAngle = (float)Math.Cos(time * 0.05f) * MathHelper.ToRadians(1.2f);
                Vector2 spiralVelocity = Projectile.velocity.RotatedBy(spiralAngle);

                // === 合成最终速度向量 ===
                Projectile.velocity = (spiralVelocity + waveOffset).SafeNormalize(Vector2.UnitY) * speed;

                // === 可选：微弱减速做尾迹残留 ===
                Projectile.velocity *= 0.995f;

                return;
            }


            // 超过 120 帧后，开始追踪敌人并恢复伤害判定
            Projectile.friendly = true;

            // 如果弹幕剩余寿命小于或等于 5 帧时，生成一个特殊的灰尘效果
            if (Projectile.timeLeft <= 5)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GemDiamond, Projectile.velocity * 0.95f);
                dust.noGravity = true; // 使灰尘无重力
                dust.color = Color.DarkRed; // 设置灰尘颜色为深红色
            }

            // === ViolenceJavLight 在超过 120 帧 螺旋式分段追踪（快速完整公转） ===
            if (Projectile.ai[0] > 120)
            {
                NPC target = Projectile.Center.ClosestNPCAt(1800);
                if (target != null)
                {
                    float spiralAnglePerFrame = MathHelper.ToRadians(90f); // 每帧旋转 90°，4 帧完成一周
                    float homingStrength = 0.15f; // 追踪强度
                    float targetSpeed = Projectile.velocity.Length();

                    Projectile.localAI[1]++;
                    if (Projectile.localAI[1] >= 20f)
                        Projectile.localAI[1] = 0f;

                    if (Projectile.localAI[1] < 16f)
                    {
                        // === 16 帧线性追踪 ===
                        Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Vector2 homingAdjustment = toTarget * targetSpeed;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, homingAdjustment, homingStrength);
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * targetSpeed;
                    }
                    else
                    {
                        // === 4 帧完整 360° 公转 ===
                        Projectile.velocity = Projectile.velocity.RotatedBy(-spiralAnglePerFrame);
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * targetSpeed;
                    }
                }
            }

            {
                // === ViolenceJavLight 复杂三角函数 Dust 飞行特效 ===

                int dustPoints = 8; // 点数量
                float baseRadius = 14f; // 基础半径

                for (int i = 0; i < dustPoints; i++)
                {
                    // 利用时间 + 位置索引让每个点略有不同相位
                    float time = Main.GlobalTimeWrappedHourly * 4f + i;

                    // 半径随 sin 波动（形成呼吸感）
                    float radius = baseRadius + (float)Math.Sin(time * 2f) * 5f;

                    // 动态角度，形成螺旋
                    float angle = time + Projectile.ai[0] * 0.05f; // ai[0] 时间偏移增强动态感

                    // 偏移位置：围绕子弹位置环绕
                    Vector2 offset = angle.ToRotationVector2() * radius;

                    // 创建 Dust
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center + offset,
                        DustID.Blood,
                        -offset.SafeNormalize(Vector2.Zero) * 0.5f, // 让 Dust 微微向内收缩
                        100,
                        Color.Lerp(Color.DarkRed, Color.Red, 0.5f),
                        0.8f
                    );
                    dust.noGravity = true;
                }

            }



            // 在弹幕剩余寿命小于或等于 80 帧时逐渐减速
            if (Projectile.timeLeft <= 80)
                Projectile.velocity *= 0.96f; // 每帧减少速度，达到减速效果
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 释放暗红色的线性粒子特效
            for (int i = 0; i < 3; i++)
            {
                Vector2 leftTrailPos = Projectile.Center + new Vector2(-5f, 0f);
                Vector2 rightTrailPos = Projectile.Center + new Vector2(5f, 0f);

                Particle darkRedTrail = new SparkParticle(leftTrailPos, Projectile.velocity * 0.2f, false, 60, 1f, Color.DarkRed);
                GeneralParticleHandler.SpawnParticle(darkRedTrail);
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
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            Vector2 overallOffset = Projectile.Size * 0.5f;
            overallOffset += Projectile.velocity * 1.4f;
            int numPoints = 46;
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), numPoints);
            return false;
        }


    }

}