using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.EndlessDevourJav;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    // 右键的隐形卫星弹幕：围绕 StarsofDestinyRIGHT 椭圆轨道旋转
    internal class StarsofDestinyRIGHTINV : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static int MaxUpdate = 7;
        private int Lifetime = 310;

        // 主题颜色改成白黄色系
        private static Color ShaderColorOne = new Color(255, 245, 210);   // 暖白
        private static Color ShaderColorTwo = new Color(220, 180, 80);    // 金黄
        private static Color ShaderEndColor = new Color(255, 255, 255);   // 纯白收尾

        // 椭圆轨道计时器（不用 localAI，遵守你的规则）
        private float orbitTimer = 0f;

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
            // =====================================================
            // 参数区（你想调什么就改什么）———————————————
            // =====================================================

            // 主公转参数
            float baseRadius = 50f;          // 原始轨道半径
            float rotateSpeed = 0.035f;      // 公转角速度
            float catchLerp = 0.12f;         // 跟随轨道点的平滑程度

            // 平滑噪声（主灵动来源）
            float noiseRangeX = 38f;         // 噪声幅度 X
            float noiseRangeY = 28f;         // 噪声幅度 Y
            float noiseFreqX = 0.8f;         // 噪声频率 X
            float noiseFreqY = 1.25f;        // 噪声频率 Y

            // 微波动（增加细节感）
            float microRange = 5f;
            float microFreq = 3.2f;

            // =====================================================
            // 准备阶段（目标弹幕检测）
            // =====================================================

            int targetIndex = (int)Projectile.ai[0];
            if (!Main.projectile[targetIndex].active)
            {
                Projectile.Kill();
                return;
            }

            Projectile parent = Main.projectile[targetIndex];
            Vector2 center = parent.Center;

            // =====================================================
            // 核心：Advanced 灵动公转轨迹
            // =====================================================

            // 1) 更新公转角度
            if (Projectile.localAI[1] == 0)
                Projectile.localAI[1] = Main.rand.NextFloat(MathHelper.TwoPi);
            Projectile.localAI[1] += rotateSpeed;
            float angle = Projectile.localAI[1];

            // 2) 基础圆周轨迹
            Vector2 baseOffset = angle.ToRotationVector2() * baseRadius;

            // 3) Perlin-like long smooth noise（平滑噪声：高阶随机）
            float t = Main.GlobalTimeWrappedHourly;

            float noiseX = (float)Math.Sin(t * noiseFreqX + Projectile.whoAmI * 0.37f) * noiseRangeX;
            float noiseY = (float)Math.Sin(t * noiseFreqY + Projectile.whoAmI * 0.51f) * noiseRangeY;

            Vector2 noiseOffset = new Vector2(noiseX, noiseY);

            // 4) 微波动（更细节的小幅脉动）
            float micro = (float)Math.Sin(t * microFreq + Projectile.whoAmI * 1.3f);
            Vector2 microOffset = micro.ToRotationVector2() * microRange;

            // 5) 最终轨迹点
            Vector2 targetPos = center + baseOffset + noiseOffset + microOffset;

            // 6) 用 Lerp 平滑追踪（保证运动丝滑灵动）
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, catchLerp);

            // =====================================================
            // 旋转方向（朝向轨迹运动方向）
            // =====================================================

            Vector2 vel = targetPos - Projectile.Center;
            if (vel != Vector2.Zero)
                Projectile.rotation = vel.ToRotation() + MathHelper.PiOver2;

            Projectile.timeLeft = 5;
        }











        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 这里暂时不加额外效果，保持简单的接触伤害即可
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
            // 保留原始动态逻辑，只是颜色换成白黄系
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
