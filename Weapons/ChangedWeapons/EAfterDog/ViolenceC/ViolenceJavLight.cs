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
    internal class ViolenceJavLight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static int MaxUpdate = 7;
        private int Lifetime = 310;

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
                Projectile.friendly = false; // 暂时关闭伤害判定
                                             // 每一帧向左旋转 5 度
                Projectile.velocity = Projectile.velocity.RotatedBy(-MathHelper.ToRadians(5));
                return; // 终止后续逻辑，继续按照原有方向运动
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

            // 追踪逻辑：在超过 120 帧后开始寻找并追踪最近的敌人
            if (Projectile.ai[0] > 120)
            {
                NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找半径 1800 内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero); // 计算指向目标的单位方向向量
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f); // 以平滑插值的方式调整速度方向，追踪目标
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
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak"));
            Vector2 overallOffset = Projectile.Size * 0.5f;
            overallOffset += Projectile.velocity * 1.4f;
            int numPoints = 46;
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), numPoints);
            return false;
        }


    }

}