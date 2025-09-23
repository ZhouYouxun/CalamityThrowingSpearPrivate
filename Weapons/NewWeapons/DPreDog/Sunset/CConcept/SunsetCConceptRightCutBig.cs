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
    internal class SunsetCConceptRightCutBig : ModProjectile, ILocalizedModType
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
            Projectile.width = Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 350;
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
            //Particle blastRing = new CustomPulse(
            //    Projectile.Center, // 以弹幕为中心
            //    Vector2.Zero,
            //    Color.White,
            //    "CalamityThrowingSpear/Texture/YingYang",
            //    Vector2.One * 0.33f,
            //    Main.rand.NextFloat(-10f, 10f),
            //    0.07f,
            //    0.15f,
            //    15
            //);
            //GeneralParticleHandler.SpawnParticle(blastRing);

            Player owner = Main.player[Projectile.owner];
            int totalCrit = (int)Math.Round(owner.GetTotalCritChance(Projectile.DamageType));
            Projectile.CritChance = totalCrit;
        }
        private int lifeTimer = 0; // 存活帧数计时器
        private NPC lockedTarget;
        private bool initialized = false;
        private const int hoverOffsetY = 20 * 16; // 悬空高度
        private const int smashFrame = 150;       // X 帧后下砸

        /// <summary>
        /// 辅助：找最近的敌人
        /// </summary>
        private NPC FindClosestTarget()
        {
            NPC closest = null;
            float minDist = 4000f; // 搜索范围
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
        public override void AI()
        {


            // 初始化：寻找最近敌人
            if (!initialized)
            {
                initialized = true;
                lockedTarget = FindClosestTarget();
                lifeTimer = 0;
            }

            lifeTimer++;

            // 如果目标失效，弹幕自杀
            if (lockedTarget == null || !lockedTarget.active || lockedTarget.friendly || lockedTarget.dontTakeDamage)
            {
                Projectile.Kill();
                return;
            }

            if (lifeTimer < smashFrame)
            {
                // 悬浮在敌人头顶 hoverOffsetY
                Vector2 targetPos = new Vector2(
                    lockedTarget.Center.X,
                    lockedTarget.Hitbox.Top - hoverOffsetY
                );
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.25f);
                Projectile.velocity = Vector2.Zero;

                // 悬空时固定朝向正下方
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.PiOver4 + MathHelper.PiOver4;
            }
            else if (lifeTimer == smashFrame)
            {
                // 触发下砸：朝下高速
                Projectile.velocity = Vector2.UnitY * 40f;

                // 可以在这里触发一个「预兆特效」
                // 比如小范围收缩环
                Particle pulse = new CustomPulse(
                    Projectile.Center,
                    Vector2.Zero,
                    Color.Cyan,
                    "CalamityThrowingSpear/Texture/SunsetChange",
                    Vector2.One * 0.5f,
                    0f,
                    0.1f,
                    0.4f,
                    20
                );
                GeneralParticleHandler.SpawnParticle(pulse);

                SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
            }
            else
            {
                // 保持高速往下冲刺
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }








            

                {
                    // === 飞行科技蓝特效（垂直上喷版，整体缩小20%）===
                    // ---------- 🔧 可调参数 ----------
                    int exoPerTick = 4;      // 每帧 EXO 强光数量
                    int orbPerTick = 10;     // 每帧 辉光球数量
                    int sparkPerTick = 5;   // 每帧 线性火花数量
                    int squarePerTick = 2;   // 每帧 方形碎片数量
                    int dustPerTick = 4;    // 每帧 Dust 点缀

                    float coneHalfAngle = MathHelper.ToRadians(35f); // 正上方喷射扇形半角
                    float trailSpan = 56f;     // 丝带中心线长度
                    float lissaA = 3f;         // Lissajous 参数 a
                    float lissaB = 4f;         // Lissajous 参数 b
                    float lissaAxAmp = 16f;    // Lissajous X 振幅
                    float lissaAyAmp = 10f;    // Lissajous Y 振幅

                    // ↓ 全局缩放 0.8 (比原先小20%)
                    float globalScale = Projectile.scale * 1.6f * 0.8f;

                    Color[] techBlue = {
                        new Color( 80, 200, 255),
                        new Color(120, 220, 255),
                        Color.Cyan,
                        new Color(180, 220, 255),
                        Color.WhiteSmoke
                    };

                    // ---------- 固定朝向：上/下/切向 ----------
                    Vector2 up = -Vector2.UnitY;
                    Vector2 down = Vector2.UnitY;
                    Vector2 tan = Vector2.UnitX; // 左右方向

                    // ---------- 工具：取“正上方扇形”随机方向与出生点 ----------
                    Vector2 RandConeDir() => up.RotatedBy(Main.rand.NextFloat(-coneHalfAngle, coneHalfAngle));
                    Vector2 RandConePos()
                    {
                        float radial = Main.rand.NextFloat(12f, 42f) * globalScale;
                        return Projectile.Center + RandConeDir() * radial + tan * Main.rand.NextFloat(-8f, 8f) * globalScale;
                    }

                    // ---------- A) Lissajous 丝带辉光 ----------
                    for (int i = 0; i < orbPerTick; i++)
                    {
                        float t = ((Main.GameUpdateCount + i * 9) % 120) / 120f;
                        Vector2 centerLine = Projectile.Center + up * (trailSpan * (0.4f + 0.6f * (float)Math.Sin(MathHelper.TwoPi * t)));

                        Vector2 lissaLocal = new Vector2(
                            lissaAxAmp * (float)Math.Sin(lissaA * MathHelper.TwoPi * t),
                            lissaAyAmp * (float)Math.Sin(lissaB * MathHelper.TwoPi * t)
                        ) * globalScale;

                        Vector2 lissaWorld = up * lissaLocal.Y + tan * lissaLocal.X;

                        var orb = new GlowOrbParticle(
                            centerLine + lissaWorld,
                            up.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.4f, 2.0f),
                            false,
                            Main.rand.Next(14, 20),
                            Main.rand.NextFloat(0.9f, 1.4f) * globalScale,
                            techBlue[Main.rand.Next(techBlue.Length)],
                            true, false, true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }

                    // ---------- B) EXO 扇形喷焰 ----------
                    for (int i = 0; i < exoPerTick; i++)
                    {
                        Vector2 pos = RandConePos();
                        Vector2 vel = RandConeDir() * Main.rand.NextFloat(1.2f, 5.6f);
                        var exo = new SquishyLightParticle(
                            pos, vel,
                            Main.rand.NextFloat(0.28f, 0.40f) * globalScale,
                            techBlue[Main.rand.Next(techBlue.Length)],
                            Main.rand.Next(22, 30),
                            opacity: 1f,
                            squishStrenght: 1f,
                            maxSquish: Main.rand.NextFloat(2.2f, 3.2f) * globalScale,
                            hueShift: 0f
                        );
                        GeneralParticleHandler.SpawnParticle(exo);
                    }

                    // ---------- C) 线性火花 ----------
                    for (int i = 0; i < sparkPerTick; i++)
                    {
                        Vector2 pos = RandConePos();
                        Vector2 vel = RandConeDir() * Main.rand.NextFloat(2.5f, 6.5f);
                        var sp = new SparkParticle(
                            pos, vel,
                            false,
                            Main.rand.Next(24, 36),
                            Main.rand.NextFloat(0.5f, 0.9f) * globalScale,
                            Color.Lerp(new Color(120, 220, 255), Color.White, 0.6f)
                        );
                        GeneralParticleHandler.SpawnParticle(sp);
                    }

                    // ---------- D) 方形碎片 ----------
                    for (int i = 0; i < squarePerTick; i++)
                    {
                        Vector2 pos = RandConePos();
                        Vector2 vel = (RandConeDir() + tan * Main.rand.NextFloat(-0.45f, 0.45f)).SafeNormalize(up) * Main.rand.NextFloat(1.5f, 4.0f);
                        var sq = new SquareParticle(
                            pos, vel,
                            false,
                            Main.rand.Next(20, 30),
                            Main.rand.NextFloat(1.3f, 2.1f) * globalScale,
                            techBlue[Main.rand.Next(techBlue.Length)]
                        );
                        GeneralParticleHandler.SpawnParticle(sq);
                    }

                    // ---------- E) Dust 对数螺旋 ----------
                    for (int i = 0; i < dustPerTick; i++)
                    {
                        float t = Main.rand.NextFloat();
                        float theta = (t * 6.283185f) * (Main.rand.NextBool() ? 1f : -1f);
                        float r = 10f * (float)Math.Exp(0.6f * t) * globalScale;
                        Vector2 pos = Projectile.Center + up * r + tan * (float)Math.Sin(theta) * 4f * globalScale;
                        var d = Dust.NewDustPerfect(
                            pos,
                            Main.rand.NextBool() ? DustID.Electric : DustID.UltraBrightTorch,
                            up.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.6f, 2.4f),
                            150,
                            techBlue[Main.rand.Next(techBlue.Length)],
                            Main.rand.NextFloat(0.9f, 1.3f) * globalScale
                        );
                        d.noGravity = true;
                        d.fadeIn = Main.rand.NextFloat(0.6f, 1.0f);
                        d.scale *= 0.4f; // 把 Dust 的寿命和体积同时缩短到原来的 40%（=减少60%）
                }
            }


            




        }



        public override void OnKill(int timeLeft)
        {


        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/电弧发射器发射")
    with
            { Volume = 2.5f, Pitch = 0.0f }, Projectile.Center);

            // 基础：给 Buff（保留你原逻辑）
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetCConceptPBuff>(), 300);

            // （可选）音效 + 轻微屏幕震动
            // SoundEngine.PlaySound(SoundID.Item74, target.Center);
            float shake = 96f;
            float kLerp = Utils.GetLerpValue(1000f, 0f, Vector2.Distance(target.Center, Main.LocalPlayer.Center), true);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shake * kLerp);



            {


                // ============================ 🔧 可调参数（半径拉大版） ============================
                // 调色板
                Color[] palette =
                {
    new Color( 80, 200, 255),
    new Color(120, 220, 255),
    Color.Cyan,
    new Color(180, 220, 255),
    Color.WhiteSmoke
};

                // 电磁球外圈（半径 ×3，但光珠大小不变）
                float ballRadius = 288f * Projectile.scale;   // 半径放大 (原96 → 288)
                int ballPoints = 120;                      // 外圈点数增加，保持流畅
                float ballJitter = 20f * Projectile.scale;  // 抖动幅度稍增
                float ballOrbScale = 1.05f * Projectile.scale; // 光珠大小保持原值
                int ballOrbLifeMin = 12, ballOrbLifeMax = 18;

                // 闪电丝（长度增加，粒子大小不变）
                int rayCount = 12;                       // 主条数增加
                int raySegments = 36;                       // 段数更多，延伸更远
                float rayStep = 14f * Projectile.scale;   // 每段长度保持原值
                float rayJitter = 8f * Projectile.scale;   // 抖动保持原值
                float rayCurviness = 0.38f;
                int sparksPerSeg = 2;
                int orbsPerSeg = 1;
                float rayWidthFactor = 1.10f * Projectile.scale;
                float rayStopRatioMin = 0.95f, rayStopRatioMax = 1.05f; // 基本跑到最外圈

                // 分叉
                float branchChance = 0.35f;
                int branchSegments = 8;                        // 分叉更长
                float branchSpread = 0.75f;
                float branchStepMul = 0.72f;

                // 中心能量核
                int nucleusCount = 4;
                float nucleusScale = 0.70f * Projectile.scale;
                int nucleusLifeMin = 16, nucleusLifeMax = 22;

                // 爆点
                float burstChance = 0.2f;
                float burstScaleMul = 1.0f * Projectile.scale;
                int burstSparks = 4;
                int burstOrbs = 2;
                // ============================================================================


                // ====== 基础上下文 ======
                Vector2 c = target.Center;
                Vector2 fwd = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                // 小工具
                Vector2 NoisyDir(Vector2 baseDir, float strength)
                {
                    float a = Main.rand.NextFloat(-strength, strength);
                    return baseDir.RotatedBy(a).SafeNormalize(baseDir);
                }
                void BurstAt(Vector2 pos, float scaleMul)
                {
                    for (int i = 0; i < burstSparks; i++)
                    {
                        var sp = new SparkParticle(
                            pos,
                            NoisyDir(fwd, 0.9f) * Main.rand.NextFloat(2.2f, 6.2f),
                            false,
                            Main.rand.Next(14, 20),
                            Main.rand.NextFloat(0.8f, 1.3f) * scaleMul,
                            Color.Lerp(palette[Main.rand.Next(palette.Length)], Color.White, 0.45f)
                        );
                        GeneralParticleHandler.SpawnParticle(sp);
                    }
                    for (int i = 0; i < burstOrbs; i++)
                    {
                        var orb = new GlowOrbParticle(
                            pos,
                            NoisyDir(fwd, 0.6f) * Main.rand.NextFloat(0.3f, 1.4f),
                            false,
                            Main.rand.Next(10, 15),
                            Main.rand.NextFloat(0.9f, 1.25f) * scaleMul,
                            palette[Main.rand.Next(palette.Length)],
                            true, false, true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }

                // ====== 中心能量核 ======
                for (int i = 0; i < nucleusCount; i++)
                {
                    var orb = new GlowOrbParticle(
                        c,
                        Vector2.Zero,
                        false,
                        Main.rand.Next(nucleusLifeMin, nucleusLifeMax),
                        nucleusScale * Main.rand.NextFloat(0.85f, 1.15f),
                        palette[Main.rand.Next(palette.Length)],
                        true, false, true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }

                // ====== 闪电丝 ======
                for (int r = 0; r < rayCount; r++)
                {
                    float ang = MathHelper.TwoPi * (r / (float)rayCount) + Main.rand.NextFloat(-0.7f, 0.7f);
                    Vector2 dir = ang.ToRotationVector2();

                    Vector2 p = c;
                    Vector2 curDir = dir;

                    float stopR = ballRadius * Main.rand.NextFloat(rayStopRatioMin, rayStopRatioMax);

                    for (int s = 0; s < raySegments; s++)
                    {
                        curDir = Vector2.Lerp(curDir, NoisyDir(curDir, rayCurviness), 0.88f).SafeNormalize(curDir);
                        Vector2 jitter = Main.rand.NextVector2Circular(rayJitter, rayJitter);
                        p += curDir * rayStep + jitter;

                        if (Vector2.Distance(c, p) > stopR) break;

                        for (int i = 0; i < sparksPerSeg; i++)
                        {
                            var sp = new SparkParticle(
                                p,
                                curDir * Main.rand.NextFloat(2.8f, 6.8f),
                                false,
                                Main.rand.Next(18, 26),
                                Main.rand.NextFloat(0.85f, 1.35f) * rayWidthFactor,
                                Color.Lerp(palette[Main.rand.Next(palette.Length)], Color.White, 0.40f)
                            );
                            GeneralParticleHandler.SpawnParticle(sp);
                        }
                        for (int i = 0; i < orbsPerSeg; i++)
                        {
                            var orb = new GlowOrbParticle(
                                p,
                                NoisyDir(curDir, 0.5f) * Main.rand.NextFloat(0.2f, 1.0f),
                                false,
                                Main.rand.Next(12, 18),
                                Main.rand.NextFloat(0.95f, 1.35f) * rayWidthFactor,
                                palette[Main.rand.Next(palette.Length)],
                                true, false, true
                            );
                            GeneralParticleHandler.SpawnParticle(orb);
                        }

                        if (Main.rand.NextFloat() < burstChance)
                            BurstAt(p, burstScaleMul);

                        if (s > raySegments / 3 && Main.rand.NextFloat() < branchChance)
                        {
                            Vector2 bDir = NoisyDir(curDir.RotatedBy(Main.rand.NextFloat(-branchSpread, branchSpread)), 0.45f);
                            Vector2 bp = p;
                            for (int bs = 0; bs < branchSegments; bs++)
                            {
                                bDir = Vector2.Lerp(bDir, NoisyDir(bDir, 0.25f), 0.85f).SafeNormalize(bDir);
                                bp += bDir * (rayStep * branchStepMul) + Main.rand.NextVector2Circular(rayJitter * 0.6f, rayJitter * 0.6f);

                                var sp2 = new SparkParticle(
                                    bp,
                                    bDir * Main.rand.NextFloat(2.0f, 4.5f),
                                    false,
                                    Main.rand.Next(12, 18),
                                    Main.rand.NextFloat(0.75f, 1.15f) * rayWidthFactor * 0.95f,
                                    Color.Lerp(palette[Main.rand.Next(palette.Length)], Color.White, 0.35f)
                                );
                                GeneralParticleHandler.SpawnParticle(sp2);

                                if (Main.rand.NextBool(3))
                                {
                                    var o2 = new GlowOrbParticle(
                                        bp,
                                        NoisyDir(bDir, 0.4f) * Main.rand.NextFloat(0.2f, 0.9f),
                                        false,
                                        Main.rand.Next(10, 15),
                                        Main.rand.NextFloat(0.75f, 1.1f) * rayWidthFactor * 0.9f,
                                        palette[Main.rand.Next(palette.Length)],
                                        true, false, true
                                    );
                                    GeneralParticleHandler.SpawnParticle(o2);
                                }

                                if (Vector2.Distance(c, bp) > ballRadius * 0.98f) break;
                            }
                        }
                    }
                }

                // ====== 外圈“电磁球” ======
                for (int i = 0; i < ballPoints; i++)
                {
                    float t = i / (float)ballPoints;
                    float ang = MathHelper.TwoPi * t + Main.rand.NextFloat(-0.03f, 0.03f);
                    float r = ballRadius + Main.rand.NextFloat(-ballJitter, ballJitter);

                    Vector2 edge = c + ang.ToRotationVector2() * r;

                    var orb = new GlowOrbParticle(
                        edge,
                        ang.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0.4f, 1.2f),
                        false,
                        Main.rand.Next(ballOrbLifeMin, ballOrbLifeMax),
                        ballOrbScale * Main.rand.NextFloat(0.9f, 1.2f),
                        palette[(i + Main.rand.Next(2)) % palette.Length],
                        true, false, true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);

                    if (i % 9 == 0)
                    {
                        var sp = new SparkParticle(
                            edge,
                            NoisyDir(ang.ToRotationVector2(), 0.5f) * Main.rand.NextFloat(1.5f, 3.5f),
                            false,
                            Main.rand.Next(14, 18),
                            0.9f * ballOrbScale,
                            Color.Lerp(palette[(i / 2) % palette.Length], Color.White, 0.5f)
                        );
                        GeneralParticleHandler.SpawnParticle(sp);
                    }
                }





            }

            // ---------- 5) 你的“魔法阵”骨架（保留调用） ----------
            CreateMagicCircle(target.Center);

            // 这段暂时弃用
            //        {
            //            // ---------- 🔧 可调参数 ----------
            //            float radiusBase = 64f * Projectile.scale * 1.4f; // 同心结构基础半径
            //            int ringLayers = 4;   // 同心环层数（Dust + Orb 点缀）
            //            int ringPoints = 64;  // 每圈点数
            //            int spokes = 16;  // 放射辐条条数
            //            int sparksBurst = 40;  // 火花爆发数量
            //            int exoBurst = 28;  // EXO 高亮爆发数量
            //            int squareBurst = 22;  // 方形碎片爆发数量
            //            int orbBurst = 36;  // 辉光球点缀数量
            //            float spokeLenStep = 8f * Projectile.scale; // 每段长度
            //            float spiralK = 0.22f; // 对数螺旋增长系数（GlowOrb 用）

            //            Color[] techBlue = {
            //    new Color( 80, 200, 255),
            //    new Color(120, 220, 255),
            //    Color.Cyan,
            //    new Color(180, 220, 255),
            //    Color.WhiteSmoke
            //};

            //            Vector2 c = target.Center;
            //            Vector2 fwd = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            //            float spinSign = Main.rand.NextBool() ? 1f : -1f;

            //            // ---------- 1) 同心环（Dust + GlowOrb 结构化骨架） ----------
            //            for (int l = 0; l < ringLayers; l++)
            //            {
            //                float r = radiusBase + l * (12f * Projectile.scale);
            //                for (int i = 0; i < ringPoints; i++)
            //                {
            //                    float ang = MathHelper.TwoPi * i / ringPoints;
            //                    Vector2 dir = ang.ToRotationVector2();
            //                    Vector2 pos = c + dir * r;

            //                    // Dust（跳动感 + 切向流动）
            //                    var d = Dust.NewDustPerfect(
            //                        pos,
            //                        (i % 2 == 0) ? DustID.Electric : DustID.GemDiamond,
            //                        dir * Main.rand.NextFloat(0.4f, 1.2f) + dir.RotatedBy(spinSign * MathHelper.PiOver2) * Main.rand.NextFloat(0.4f, 1.0f),
            //                        150,
            //                        techBlue[(i + l) % techBlue.Length],
            //                        (1.0f - l * 0.08f) * Projectile.scale * 1.4f
            //                    );
            //                    d.noGravity = true;

            //                    if (i % 8 == 0)
            //                    {
            //                        // GlowOrb 点缀
            //                        var orb = new GlowOrbParticle(
            //                            pos, Vector2.Zero, false,
            //                            Main.rand.Next(10, 16),
            //                            Main.rand.NextFloat(0.9f, 1.3f) * Projectile.scale,
            //                            techBlue[(i + l) % techBlue.Length],
            //                            true, false, true
            //                        );
            //                        GeneralParticleHandler.SpawnParticle(orb);
            //                    }
            //                }
            //            }

            //            // ---------- 2) 放射辐条（方形碎片 + 火花，带弯曲） ----------
            //            for (int s = 0; s < spokes; s++)
            //            {
            //                float ang0 = MathHelper.TwoPi * s / spokes + Main.rand.NextFloat(-0.03f, 0.03f);
            //                float ang = ang0;
            //                float curvePerStep = 0.08f * spinSign; // 轻微弯曲

            //                for (int j = 0; j < 18; j++)
            //                {
            //                    ang += curvePerStep;
            //                    Vector2 dir = ang.ToRotationVector2();
            //                    Vector2 pos = c + dir * (radiusBase + j * spokeLenStep);

            //                    // Square 碎片
            //                    var sq = new SquareParticle(
            //                        pos,
            //                        dir * MathHelper.Lerp(1.2f, 3.2f, j / 18f),
            //                        false,
            //                        Main.rand.Next(18, 26),
            //                        MathHelper.Lerp(1.3f, 2.2f, j / 18f) * Projectile.scale,
            //                        techBlue[(s + j) % techBlue.Length]
            //                    );
            //                    GeneralParticleHandler.SpawnParticle(sq);

            //                    if (j % 3 == 0)
            //                    {
            //                        // Spark 线性火花（更锐利）
            //                        var sp = new SparkParticle(
            //                            pos,
            //                            dir * Main.rand.NextFloat(3f, 7f),
            //                            false,
            //                            Main.rand.Next(16, 22),
            //                            Main.rand.NextFloat(1.0f, 1.5f) * Projectile.scale,
            //                            Color.Lerp(techBlue[(s + j) % techBlue.Length], Color.White, 0.35f)
            //                        );
            //                        GeneralParticleHandler.SpawnParticle(sp);
            //                    }
            //                }
            //            }

            //            // ---------- 3) EXO 高亮 + GlowOrb 对数螺旋（星爆/渲染中心） ----------
            //            for (int i = 0; i < exoBurst; i++)
            //            {
            //                Vector2 dir = (MathHelper.TwoPi * i / exoBurst).ToRotationVector2();
            //                var exo = new SquishyLightParticle(
            //                    c + dir * Main.rand.NextFloat(8f, 28f) * Projectile.scale,
            //                    dir * Main.rand.NextFloat(2f, 6f),
            //                    Main.rand.NextFloat(0.34f, 0.46f) * Projectile.scale,
            //                    techBlue[i % techBlue.Length],
            //                    Main.rand.Next(22, 30),
            //                    opacity: 1f,
            //                    squishStrenght: 1f,
            //                    maxSquish: Main.rand.NextFloat(2.4f, 3.4f) * Projectile.scale,
            //                    hueShift: 0f
            //                );
            //                GeneralParticleHandler.SpawnParticle(exo);
            //            }
            //            // 两臂对数螺旋的 GlowOrb
            //            for (int arm = 0; arm < 2; arm++)
            //            {
            //                float sign = (arm == 0) ? 1f : -1f;
            //                for (int k = 0; k < orbBurst / 2; k++)
            //                {
            //                    float t = k / (float)(orbBurst / 2);
            //                    float theta = sign * (t * MathHelper.TwoPi);
            //                    float r = 10f * (float)Math.Exp(spiralK * t) * Projectile.scale;
            //                    Vector2 pos = c + new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * r;

            //                    var orb = new GlowOrbParticle(
            //                        pos,
            //                        (pos - c).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.6f, 2.2f),
            //                        false,
            //                        Main.rand.Next(12, 18),
            //                        Main.rand.NextFloat(1.0f, 1.5f) * Projectile.scale,
            //                        techBlue[(arm + k) % techBlue.Length],
            //                        true, false, true
            //                    );
            //                    GeneralParticleHandler.SpawnParticle(orb);
            //                }
            //            }

            //            // ---------- 4) 额外层次：Point/Water/AltSpark（保留你的风格） ----------
            //            for (int i = 0; i < sparksBurst; i++)
            //            {
            //                var p = new PointParticle(
            //                    c, fwd.RotatedByRandom(MathHelper.ToRadians(45f)) * Main.rand.NextFloat(2f, 6f),
            //                    false, 15, 1.2f, Color.White
            //                );
            //                GeneralParticleHandler.SpawnParticle(p);
            //            }
            //            for (int i = 0; i < 14; i++)
            //            {
            //                var mist = new WaterFlavoredParticle(
            //                    c,
            //                    fwd.RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(1.5f, 3.5f),
            //                    false,
            //                    Main.rand.Next(18, 28),
            //                    1.0f + Main.rand.NextFloat(0.3f),
            //                    Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat()) * 0.9f
            //                );
            //                GeneralParticleHandler.SpawnParticle(mist);
            //            }
            //            for (int i = 0; i < 10; i++)
            //            {
            //                var line = new AltSparkParticle(
            //                    c - fwd * Main.rand.NextFloat(6f, 14f),
            //                    fwd * 0.25f,
            //                    false,
            //                    14,
            //                    1.2f,
            //                    Color.Cyan * 0.35f
            //                );
            //                GeneralParticleHandler.SpawnParticle(line);
            //            }

            //            // ---------- 5) 你的“魔法阵”骨架（保留调用） ----------
            //            CreateMagicCircle(target.Center);

            //            // （可选）音效 + 轻微屏幕震动
            //            // SoundEngine.PlaySound(SoundID.Item74, c);
            //            float shake = 16f;
            //            float kLerp = Utils.GetLerpValue(1000f, 0f, Vector2.Distance(c, Main.LocalPlayer.Center), true);
            //            Main.LocalPlayer.Calamity().GeneralScreenShakePower =
            //                Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shake * kLerp);
            //        }


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

            // ================== 特效寿命倍率（数值越大 → 存活越短） ==================
            float dustLifeMult = 3f; // 寿命

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

                    Dust d = Dust.NewDustPerfect(
                        pos,
                        dustType,
                        Vector2.Zero,
                        (int)(150 * dustLifeMult), // 寿命缩短
                        c,
                        (1.2f - l * 0.1f) * scaleMult
                    );
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

                    Dust d = Dust.NewDustPerfect(
                        pos,
                        type,
                        Vector2.Zero,
                        (int)(120 * dustLifeMult), // 寿命缩短
                        c,
                        scale
                    );
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

                    Dust d = Dust.NewDustPerfect(
                        pos,
                        DustID.UltraBrightTorch,
                        Vector2.Zero,
                        (int)(160 * dustLifeMult), // 寿命缩短
                        techBlue[(k + t) % techBlue.Length],
                        0.9f * scaleMult
                    );
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