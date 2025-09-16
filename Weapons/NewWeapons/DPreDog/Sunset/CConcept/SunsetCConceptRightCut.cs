using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptRightCut : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }
        public override void OnSpawn(IEntitySource source)
        {
            // 生成粒子爆炸效果
            Particle blastRing = new CustomPulse(
                Projectile.Center, // 以弹幕为中心
                Vector2.Zero,
                Color.White,
                "CalamityThrowingSpear/Texture/YingYang",
                Vector2.One * 0.33f,
                Main.rand.NextFloat(-10f, 10f),
                0.07f,
                0.15f,
                15
            );
            GeneralParticleHandler.SpawnParticle(blastRing);
        }

        public override void AI()
        {
            // 直线飞行旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // === 飞行科技蓝特效 ===
            if (Main.rand.NextBool(3)) // 大约每3帧一个
            {
                Color[] techBlue = {
            new Color(80, 200, 255),
            new Color(120, 220, 255),
            Color.Cyan,
            new Color(180, 220, 255),
            Color.WhiteSmoke
        };

                // 基准缩放：跟随弹幕scale
                float scaleMult = Projectile.scale;

                // 1) EXO之光（高亮粒子）
                SquishyLightParticle exo = new SquishyLightParticle(
                    Projectile.Center,
                    -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f, 1.0f),
                    0.25f * scaleMult,
                    techBlue[Main.rand.Next(techBlue.Length)],
                    18,
                    opacity: 0.9f,
                    squishStrenght: 1f,
                    maxSquish: 2.5f * scaleMult
                );
                GeneralParticleHandler.SpawnParticle(exo);

                // 2) 辉光球（小亮点）
                if (Main.rand.NextBool(2))
                {
                    GlowOrbParticle orb = new GlowOrbParticle(
                        Projectile.Center,
                        Vector2.Zero,
                        false,
                        12,
                        0.7f * scaleMult,
                        techBlue[Main.rand.Next(techBlue.Length)],
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }

                // 3) 四方粒子（碎片感）
                if (Main.rand.NextBool(4))
                {
                    SquareParticle sq = new SquareParticle(
                        Projectile.Center,
                        Projectile.velocity * 0.05f,
                        false,
                        20,
                        1.0f * scaleMult,
                        techBlue[Main.rand.Next(techBlue.Length)]
                    );
                    GeneralParticleHandler.SpawnParticle(sq);
                }
            }
        }



        public override void OnKill(int timeLeft)
        {


        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetCConceptPBuff>(), 300); // 5 秒

            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);

            // 1️⃣ 尖刺型白色粒子（锐利冲击感）
            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity = forward.RotatedByRandom(MathHelper.ToRadians(45f)) * Main.rand.NextFloat(2f, 6f);
                PointParticle spark = new PointParticle(
                    Projectile.Center,
                    velocity,
                    false,
                    15,
                    1.1f,
                    Color.White
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 2️⃣ 水雾型粒子（能量雾气，暗一些，弥补层次）
            for (int i = 0; i < 8; i++)
            {
                Vector2 dir = forward.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(1.5f, 3.5f);
                Color c = Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat());
                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                    Projectile.Center,
                    dir,
                    false,
                    Main.rand.Next(18, 26),
                    0.9f + Main.rand.NextFloat(0.3f),
                    c * 0.9f
                );
                GeneralParticleHandler.SpawnParticle(mist);
            }

            // 3️⃣ 细长线性粒子（稀疏，科技丝线）
            for (int i = 0; i < 4; i++)
            {
                AltSparkParticle sparkLine = new AltSparkParticle(
                    Projectile.Center - Projectile.velocity * Main.rand.NextFloat(1f, 2f), // 稍微后置
                    forward * 0.2f,   // 微速向前
                    false,
                    12, // 寿命稍长
                    1.2f,
                    Color.Cyan * 0.35f
                );
                GeneralParticleHandler.SpawnParticle(sparkLine);
            }

            // 生成魔法阵
            CreateMagicCircle(target.Center);

            // 播放独特击中音效
            SoundEngine.PlaySound(SoundID.Item30, Projectile.position);
        }







        private void CreateMagicCircle(Vector2 center)
        {
            // === 缩放系数：跟随弹幕 scale ===
            float scaleMult = Projectile.scale;

            // ================== 可调参数（所有关键参数都乘以 scaleMult） ==================
            int ringLayers = 3;     // 同心环层数
            int ringPoints = 48;    // 每圈点数
            float baseRadius = 50f * scaleMult;   // 第一圈半径
            float ringGap = 12f * scaleMult;   // 圈间距
            float spinSign = Main.rand.NextBool() ? 1f : -1f; // 随机顺/逆时针

            int spokes = 12;     // 放射辐条数量
            int spokeSteps = 22;     // 每条辐条段数
            float stepLen = 6f * scaleMult;    // 辐条步进距离
            float curvePerStep = 0.10f; // 弯曲幅度（保持不变，视觉上随半径也会放大）

            int tickCount = 24;    // 刻度短弧数量
            int tickSegs = 6;     // 每个短弧的点数

            // 科技蓝调色板
            Color[] techBlue =
            {
        new Color( 80, 200, 255),
        new Color(120, 220, 255),
        Color.Cyan,
        new Color(180, 220, 255),
        Color.WhiteSmoke
    };

            // ================== 1) 同心环 ==================
            for (int l = 0; l < ringLayers; l++)
            {
                float r = baseRadius + l * ringGap;

                for (int i = 0; i < ringPoints; i++)
                {
                    float ang = MathHelper.TwoPi * i / ringPoints;
                    Vector2 dir = ang.ToRotationVector2();
                    Vector2 pos = center + dir * r;

                    int dustType = (i % 2 == 0) ? DustID.Electric : DustID.GemDiamond;
                    Color c = techBlue[(i + l) % techBlue.Length];

                    Dust d = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 150, c, (1.2f - l * 0.1f) * scaleMult);
                    d.noGravity = true;

                    Vector2 tan = dir.RotatedBy(spinSign * MathHelper.PiOver2);
                    d.velocity = tan * Main.rand.NextFloat(0.6f, 1.2f) * scaleMult
                               + dir * Main.rand.NextFloat(0.2f, 0.6f) * scaleMult;
                    d.fadeIn = 0.8f;
                }
            }

            // ================== 2) 放射辐条 ==================
            for (int s = 0; s < spokes; s++)
            {
                float ang0 = MathHelper.TwoPi * s / spokes + Main.rand.NextFloat(-0.05f, 0.05f);
                float curve = Main.rand.NextBool() ? curvePerStep : -curvePerStep;
                float ang = ang0;

                for (int j = 0; j < spokeSteps; j++)
                {
                    ang += curve;
                    Vector2 dir = ang.ToRotationVector2();
                    Vector2 pos = center + dir * (baseRadius + j * stepLen);

                    int type = (j % 2 == 0) ? DustID.BlueTorch : DustID.GemDiamond;
                    float scale = MathHelper.Lerp(0.9f, 1.6f, j / (float)spokeSteps) * scaleMult;
                    Color c = techBlue[(s + j) % techBlue.Length];

                    Dust d = Dust.NewDustPerfect(pos, type, Vector2.Zero, 120, c, scale);
                    d.noGravity = true;

                    Vector2 tan = dir.RotatedBy(spinSign * MathHelper.PiOver2);
                    float t = j / (float)spokeSteps;
                    d.velocity = dir * MathHelper.Lerp(1.2f, 2.4f, t) * scaleMult
                               + tan * (0.4f + 0.8f * t) * scaleMult;

                    if (j > spokeSteps - 5)
                        d.scale *= 0.6f;
                }
            }

            // ================== 3) 刻度短弧 ==================
            for (int k = 0; k < tickCount; k++)
            {
                float ang = MathHelper.TwoPi * k / tickCount + Main.rand.NextFloat(-0.02f, 0.02f);
                float arcR = baseRadius + ringGap * 0.5f;
                Vector2 dir0 = ang.ToRotationVector2();

                for (int t = 0; t < tickSegs; t++)
                {
                    float a = ang + (t - tickSegs / 2f) * 0.02f * spinSign;
                    Vector2 dir = a.ToRotationVector2();
                    Vector2 pos = center + dir * (arcR + t * scaleMult);

                    Dust d = Dust.NewDustPerfect(pos, DustID.UltraBrightTorch, Vector2.Zero, 160, techBlue[(k + t) % techBlue.Length], 0.9f * scaleMult);
                    d.noGravity = true;
                    d.velocity = dir * 0.8f * scaleMult
                               + dir.RotatedBy(spinSign * MathHelper.PiOver2) * 0.6f * scaleMult;
                }
            }

            // ================== 4) 粒子点缀 ==================
            for (int i = 0; i < 4; i++)
            {
                float a = Main.rand.NextFloat(MathHelper.TwoPi);
                float rad = baseRadius + Main.rand.NextFloat(0f, ringLayers * ringGap);
                Vector2 pos = center + a.ToRotationVector2() * rad;
                Color c = techBlue[Main.rand.Next(techBlue.Length)];

                GlowOrbParticle orb = new GlowOrbParticle(
                    pos,
                    Vector2.Zero,
                    false,
                    Main.rand.Next(8, 14),
                    Main.rand.NextFloat(0.7f, 1.0f) * scaleMult,
                    c,
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }

            for (int i = 0; i < 3; i++)
            {
                float a = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dir = a.ToRotationVector2();
                Vector2 pos = center + dir * (baseRadius + ringGap * 0.8f);
                Color c = techBlue[Main.rand.Next(techBlue.Length)];

                SquareParticle sq = new SquareParticle(
                    pos,
                    dir * Main.rand.NextFloat(0.8f, 1.6f) * scaleMult,
                    false,
                    24,
                    (1.2f + Main.rand.NextFloat(0.6f)) * scaleMult,
                    c
                );
                GeneralParticleHandler.SpawnParticle(sq);
            }
        }










    }
}