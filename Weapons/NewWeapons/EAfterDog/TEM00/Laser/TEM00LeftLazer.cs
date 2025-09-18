using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.Graphics.Effects;
using Terraria.GameContent.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00.Laser
{
    internal class TEM00LeftLazer : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        //public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/TEM00/TEM00";


        // 这些参数不再用 const，而是字段，便于 OnSpawn 调整
        private int Lifetime;
        private float MaxBeamScale;
        private float MaxBeamLength;
        private float BeamTileCollisionWidth;
        private float BeamHitboxCollisionWidth;
        private int NumSamplePoints;
        private float BeamLengthChangeFactor;

        private Vector2 beamVector = Vector2.Zero;

        public override void OnSpawn(IEntitySource source)
        {
            // 在这里统一赋值，方便运行时热重载修改
            Lifetime = 24;
            MaxBeamScale = 1.2f;
            MaxBeamLength = 1200f;
            BeamTileCollisionWidth = 1f;
            BeamHitboxCollisionWidth = 16f;
            NumSamplePoints = 3;
            BeamLengthChangeFactor = 0.75f;

            Projectile.timeLeft = Lifetime;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4;
            Projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            // 第一帧确定方向向量
            if (Projectile.velocity != Vector2.Zero)
            {
                beamVector = Vector2.Normalize(Projectile.velocity);
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity = Vector2.Zero;
            }

            // 激光随时间衰减
            float power = (float)Projectile.timeLeft / Lifetime;
            Projectile.scale = MaxBeamScale * power;

            // 用 LaserScan 探测最大长度
            float[] laserScanResults = new float[NumSamplePoints];
            float scanWidth = Projectile.scale < 1f ? 1f : Projectile.scale;
            Collision.LaserScan(Projectile.Center, beamVector, BeamTileCollisionWidth * scanWidth, MaxBeamLength, laserScanResults);
            float avg = 0f;
            for (int i = 0; i < laserScanResults.Length; ++i)
                avg += laserScanResults[i];
            avg /= NumSamplePoints;
            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], avg, BeamLengthChangeFactor);

            // 生成额外特效：在激光路径上撒 Dust
            ProduceBeamDust();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float _ = float.NaN;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Center,
                Projectile.Center + beamVector * Projectile.ai[0],
                BeamHitboxCollisionWidth * Projectile.scale,
                ref _
            );
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (beamVector == Vector2.Zero || Projectile.velocity != Vector2.Zero)
                return false;

            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            float beamLength = Projectile.ai[0];
            Vector2 centerFloored = Projectile.Center.Floor() + beamVector * Projectile.scale * 10f;
            Vector2 scaleVec = new Vector2(Projectile.scale);

            // 颜色：非常浅的蓝色接近白色
            Color beamColor = Color.Lerp(Color.White, Color.LightBlue, 0.3f);

            DelegateMethods.f_1 = 1f;
            Vector2 beamStartPos = centerFloored - Main.screenPosition;
            Vector2 beamEndPos = beamStartPos + beamVector * beamLength;
            Utils.LaserLineFraming llf = new Utils.LaserLineFraming(DelegateMethods.RainbowLaserDraw);

            // 外层
            DelegateMethods.c_1 = beamColor * 0.85f * Projectile.Opacity;
            Utils.DrawLaser(Main.spriteBatch, tex, beamStartPos, beamEndPos, scaleVec, llf);

            // 内层（叠加几层，逐渐更白更细）
            for (int i = 0; i < 4; ++i)
            {
                beamColor = Color.Lerp(beamColor, Color.White, 0.5f);
                scaleVec *= 0.8f;
                DelegateMethods.c_1 = beamColor * 0.5f * Projectile.Opacity;
                Utils.DrawLaser(Main.spriteBatch, tex, beamStartPos, beamEndPos, scaleVec, llf);
            }

            return false;
        }




        public void ProduceBeamDust()
        {
            // 保护：无方向或无长度时不生成特效
            if (beamVector == Vector2.Zero || Projectile.ai[0] <= 2f)
                return;

            Vector2 start = Projectile.Center;
            Vector2 end = Projectile.Center + beamVector * Projectile.ai[0];

            // =========================
            // ① BloomLineVFX（首位，极重要）
            // =========================
            {
                // 线段长度做个上限，避免离谱过长；粗细随 scale 轻微波动
                float lineLen = Math.Min(Projectile.ai[0], 240f);
                float thickness = 1.4f * MathHelper.Lerp(0.9f, 1.1f, (float)Main.rand.NextDouble());
                Color lineColor = Color.Lerp(Color.White, Color.Cyan, 0.25f) * Projectile.Opacity;

                BloomLineVFX bloomLine = new BloomLineVFX(
                    start,                       // 起点
                    beamVector * lineLen,        // 方向与长度
                    thickness,                   // 粗细
                    lineColor,                   // 颜色（浅蓝靠白）
                    40                           // 生命周期（帧）
                );
                GeneralParticleHandler.SpawnParticle(bloomLine);
            }

            // =========================
            // 路径等距采样：优雅混合三类粒子 + Dust
            // =========================
            int points = Math.Clamp((int)(Projectile.ai[0] / 48f), 3, 14); // 距离越长采样越多
            for (int i = 0; i < points; i++)
            {
                float t = (points == 1) ? 0f : i / (float)(points - 1);
                Vector2 pos = Vector2.Lerp(start, end, t) + Main.rand.NextVector2Circular(2f, 2f);

                // ② EXO之光（SquishyLightParticle）：高亮柔光，主能量感
                if (Main.rand.NextBool(2)) // 约 50% 采样点生成
                {
                    // 冰蓝偏白：科技蓝基调
                    Color exoColor = Color.Lerp(Color.White, new Color(120, 200, 255), 0.55f) * Projectile.Opacity;

                    SquishyLightParticle exoEnergy = new SquishyLightParticle(
                        pos,
                        (-beamVector).RotatedByRandom(0.39f) * Main.rand.NextFloat(0.4f, 1.6f), // 轻微回喷动感
                        0.26f + Main.rand.NextFloat(0.06f), // 细微随机缩放
                        exoColor,
                        25,                  // 寿命
                        opacity: 1f,
                        squishStrenght: 1f,
                        maxSquish: 3f,
                        hueShift: 0f
                    );
                    GeneralParticleHandler.SpawnParticle(exoEnergy);
                }

                // ③ 辉光球（GlowOrbParticle）：清爽的小亮点，提“线”的纯净感
                {
                    Color orbColor = Color.Lerp(new Color(80, 200, 255), Color.White, 0.6f) * Projectile.Opacity;
                    GlowOrbParticle orb = new GlowOrbParticle(
                        pos,
                        Vector2.Zero,
                        false,   // 不受重力
                        6,       // 短寿命灵动
                        0.8f + Main.rand.NextFloat(0.25f),
                        orbColor,
                        true,    // 加法混合
                        false,
                        true     // 中心叠加高亮
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }

                // ④ 四方粒子（SquareParticle）：赛博/数字化味道
                if (Main.rand.NextBool(3)) // 约 33%
                {
                    Color squareC = (Color.Cyan * 1.4f) * Projectile.Opacity;
                    SquareParticle squareParticle = new SquareParticle(
                        pos,
                        beamVector * Main.rand.NextFloat(0.8f, 1.6f), // 沿束方向轻速漂移
                        false,     // 不受重力
                        30,        // 寿命
                        1.5f + Main.rand.NextFloat(0.8f), // 尺寸随机
                        squareC
                    );
                    GeneralParticleHandler.SpawnParticle(squareParticle);
                }

                // ⑤ Dust 混合（电弧 + 蓝晶 + 蓝宝石）：轻量点缀，避免噪点过多
                if (Main.rand.NextBool(2)) // 约 50%
                {
                    // 电弧：细小闪烁，贴着束流
                    Dust d1 = Dust.NewDustPerfect(
                        pos,
                        DustID.Electric,
                        beamVector.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.1f, 0.6f)
                    );
                    d1.noGravity = true;
                    d1.scale = 0.9f + Main.rand.NextFloat(0.4f);

                    // 蓝晶：更“晶体化”的冷光
                    if (Main.rand.NextBool(3))
                    {
                        Dust d2 = Dust.NewDustPerfect(
                            pos,
                            DustID.BlueCrystalShard,
                            Main.rand.NextVector2Circular(0.4f, 0.4f)
                        );
                        d2.noGravity = true;
                        d2.scale = 1.05f + Main.rand.NextFloat(0.5f);
                        d2.color = Color.Lerp(Color.White, Color.LightBlue, 0.5f);
                    }

                    // 蓝宝石：偶发性亮斑
                    if (Main.rand.NextBool(4))
                    {
                        Dust d3 = Dust.NewDustPerfect(
                            pos,
                            DustID.GemSapphire,
                            -beamVector * Main.rand.NextFloat(0.1f, 0.5f)
                        );
                        d3.noGravity = true;
                        d3.scale = 0.9f + Main.rand.NextFloat(0.3f);
                    }
                }
            }
        }







    }
}
