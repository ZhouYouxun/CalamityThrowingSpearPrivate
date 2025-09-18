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
            // ========== 可调参数 ==========
            float particleSpacing = 24f;    // 粒子间隔（越小越紧凑）
            int minPoints = 6;              // 最少采样点数
            int maxPoints = 28;             // 最多采样点数

            // BloomLine 设置
            float bloomThickness = 0.6f;    // BloomLine 粗细（比原来大幅缩小）
            int bloomLifetime = 40;         // BloomLine 寿命
            Color bloomColor = Color.Lerp(Color.White, Color.Cyan, 0.2f);

            // 其他粒子设置
            float exoVelocityMult = 2.8f;   // EXO之光速度倍率
            float orbVelocityMult = 1.6f;   // 辉光球速度倍率
            float squareVelocityMult = 2.2f;// 方块粒子速度倍率
            float dustVelocityMult = 2.5f;  // Dust 粒子速度倍率
                                            // ==============================

            if (beamVector == Vector2.Zero || Projectile.ai[0] <= 2f)
                return;

            Vector2 start = Projectile.Center;
            Vector2 end = Projectile.Center + beamVector * Projectile.ai[0];

            // ====== BloomLine 覆盖整条激光 ======
            BloomLineVFX bloomLine = new BloomLineVFX(
                start,
                beamVector * Projectile.ai[0], // 覆盖整条激光
                bloomThickness,
                bloomColor * Projectile.Opacity,
                bloomLifetime
            );
            GeneralParticleHandler.SpawnParticle(bloomLine);

            // ====== 计算路径采样点 ======
            int points = Math.Clamp((int)(Projectile.ai[0] / particleSpacing), minPoints, maxPoints);

            for (int i = 0; i < points; i++)
            {
                float t = (points == 1) ? 0f : i / (float)(points - 1);
                Vector2 pos = Vector2.Lerp(start, end, t);

                // ① EXO之光：亮度极高，速度更快
                if (Main.rand.NextBool(2))
                {
                    SquishyLightParticle exoEnergy = new SquishyLightParticle(
                        pos,
                        (-beamVector).RotatedByRandom(0.39f) * Main.rand.NextFloat(0.8f, 2.2f) * exoVelocityMult,
                        0.28f,
                        Color.Lerp(Color.White, Color.Cyan, 0.55f),
                        25,
                        opacity: 1f,
                        squishStrenght: 1f,
                        maxSquish: 3f,
                        hueShift: 0f
                    );
                    GeneralParticleHandler.SpawnParticle(exoEnergy);
                }

                // ② 辉光球：清爽光点，快速漂移
                if (Main.rand.NextBool(2))
                {
                    GlowOrbParticle orb = new GlowOrbParticle(
                        pos,
                        beamVector.RotatedByRandom(0.5f) * Main.rand.NextFloat(1.2f, 2.5f) * orbVelocityMult,
                        false,
                        10,
                        0.9f,
                        Color.Lerp(Color.White, Color.Cyan, 0.6f),
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }

                // ③ 四方粒子：几何感，往旁边飞
                if (Main.rand.NextBool(3))
                {
                    SquareParticle square = new SquareParticle(
                        pos,
                        beamVector.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(1.5f, 3.2f) * squareVelocityMult,
                        false,
                        30,
                        1.7f + Main.rand.NextFloat(0.6f),
                        Color.Cyan * 1.5f
                    );
                    GeneralParticleHandler.SpawnParticle(square);
                }

                // ④ Dust：小碎光，快速外飘
                if (Main.rand.NextBool(2))
                {
                    Dust d1 = Dust.NewDustPerfect(
                        pos,
                        DustID.Electric,
                        beamVector.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.6f, 1.4f) * dustVelocityMult
                    );
                    d1.noGravity = true;
                    d1.scale = 1.1f + Main.rand.NextFloat(0.4f);

                    if (Main.rand.NextBool(3))
                    {
                        Dust d2 = Dust.NewDustPerfect(
                            pos,
                            DustID.BlueCrystalShard,
                            Main.rand.NextVector2Circular(0.6f, 0.6f) * dustVelocityMult
                        );
                        d2.noGravity = true;
                        d2.scale = 1.2f;
                        d2.color = Color.Lerp(Color.White, Color.LightBlue, 0.5f);
                    }
                }
            }
        }







    }
}
