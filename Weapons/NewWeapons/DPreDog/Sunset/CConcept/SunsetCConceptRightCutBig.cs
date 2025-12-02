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
        private const int hoverOffsetY = 35 * 16; // 悬空高度
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
            //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;

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
                Projectile.rotation = Projectile.velocity.ToRotation();
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
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver4 - MathHelper.PiOver4;
            }







            {

                // === 飞行科技蓝特效（垂直上喷版，整体缩小20%）===
                // ---------- 🔧 可调参数 ----------
                int exoPerTick = 4;      // 每帧 EXO 强光数量
                int orbPerTick = 10;     // 每帧 辉光球数量
                int sparkPerTick = 5;    // 每帧 线性火花数量
                int squarePerTick = 2;   // 每帧 方形碎片数量
                int dustPerTick = 4;     // 每帧 Dust 点缀

                float coneHalfAngle = MathHelper.ToRadians(35f); // 正上方喷射扇形半角
                float trailSpan = 56f;     // 丝带中心线长度
                float lissaA = 3f;         // Lissajous 参数 a
                float lissaB = 4f;         // Lissajous 参数 b
                float lissaAxAmp = 16f;    // Lissajous X 振幅
                float lissaAyAmp = 10f;    // Lissajous Y 振幅

                // ↓ 全局缩放 0.8 (比原先小20%)
                float globalScale = Projectile.scale * 1.6f * 0.8f;

                // 调色盘：统一管理
                Color[] techBlue = {
    Color.White,
    new Color(255, 215, 0),        // 金
    new Color(183, 173, 224),      // 浅紫
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
                        techBlue[Main.rand.Next(techBlue.Length)] // ✅ 改成调色盘，不再 Lerp 白色
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
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 对 Boss：若当前血量 ≤ 最大生命值 15%，造成最大生命值 100% 的额外伤害
            if (target.boss && target.life <= target.lifeMax * 0.15f)
            {
                // +100% 最大生命值的额外伤害（等价于 FinalDamage *= 2）
                float bonus = target.lifeMax; // 直接加等同于 100% maxHP 的额外伤害
                modifiers.FinalDamage += bonus;
            }
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
            float shake = 326f;
            float kLerp = Utils.GetLerpValue(1000f, 0f, Vector2.Distance(target.Center, Main.LocalPlayer.Center), true);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shake * kLerp);



            {

                // ============================ 🔧 可调参数 ============================
                Color[] palette =
                {
            Color.White,
            new Color(255, 215, 0),        // 金
            new Color(120, 90, 160)        // 深紫
        };

                float ballRadius = 288f * Projectile.scale;
                int ballPoints = 120;
                float ballJitter = 20f * Projectile.scale;
                float ballOrbScale = 1.05f * Projectile.scale;
                int ballOrbLifeMin = 12, ballOrbLifeMax = 18;

                int rayCount = 12;
                int raySegments = 36;
                float rayStep = 14f * Projectile.scale;
                float rayJitter = 8f * Projectile.scale;
                float rayCurviness = 0.38f;
                int sparksPerSeg = 2;
                int orbsPerSeg = 1;
                float rayWidthFactor = 1.10f * Projectile.scale;
                float rayStopRatioMin = 0.95f, rayStopRatioMax = 1.05f;

                float branchChance = 0.35f;
                int branchSegments = 8;
                float branchSpread = 0.75f;
                float branchStepMul = 0.72f;

                int nucleusCount = 4;
                float nucleusScale = 0.70f * Projectile.scale;
                int nucleusLifeMin = 16, nucleusLifeMax = 22;

                float burstChance = 0.2f;
                float burstScaleMul = 1.0f * Projectile.scale;
                int burstSparks = 4;
                int burstOrbs = 2;
                // ====================================================================

                Vector2 c = target.Center;
                Vector2 fwd = Projectile.velocity.SafeNormalize(Vector2.UnitY);

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
                            palette[Main.rand.Next(palette.Length)]
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
                                palette[Main.rand.Next(palette.Length)]
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
                                    palette[Main.rand.Next(palette.Length)]
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
                            palette[(i / 2) % palette.Length]
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





        // 仅 GlowOrb 版本：严格贴近参考图，并确保所有内线连接到外圈
        // 仅 GlowOrb 版本：外层改成 9角星芒爆炸（GlowOrb），原来的双外环注释掉
        private void CreateMagicCircle(Vector2 center)
        {
            float s = Projectile.scale * 2.5f;

            Color[] pal =
            {
        new Color(120, 90, 160),   // 深紫
        new Color(155, 120, 200),  // 次亮
        new Color(183, 173, 224)   // 高光
    };

            float R_outer_in = (50f + 2f * 12f) * s;
            float R_outer_out = R_outer_in + 8f * s;
            float R_spokeEnd = R_outer_in - 0.5f * s;
            float R_mid = R_outer_in - 18f * s;
            float R_tick = R_mid - 18f * s;
            float R_coreA = R_tick * 0.40f;
            float R_coreB = R_coreA * 0.55f;

            float EPS_HIT = 1.25f * s;

            void PutOrb(Vector2 p, float scale, Color c, int lifeMin = 12, int lifeMax = 18)
            {
                var orb = new GlowOrbParticle(p, Vector2.Zero, false,
                    Main.rand.Next(lifeMin, lifeMax),
                    scale,
                    c, true, false, true);
                GeneralParticleHandler.SpawnParticle(orb);
            }

            void LineOrbs(Vector2 a, Vector2 b, float step, float scaleMin, float scaleMax, Color c)
            {
                Vector2 d = b - a;
                float len = d.Length();
                if (len <= 0.5f) return;
                Vector2 dir = d / len;
                for (float t = 0; t <= len; t += step)
                    PutOrb(a + dir * t, Main.rand.NextFloat(scaleMin, scaleMax), c);
            }

            void RingOrbs(Vector2 c, float r, int points, float scMin, float scMax, Color col)
            {
                for (int i = 0; i < points; i++)
                {
                    float ang = MathHelper.TwoPi * i / points;
                    PutOrb(c + ang.ToRotationVector2() * r, Main.rand.NextFloat(scMin, scMax), col);
                }
            }

            void DashedArc(Vector2 c, float r, float ang0, float sweep, int dashCount, float fillRatio,
                           float step, float scMin, float scMax, Color col)
            {
                for (int d = 0; d < dashCount; d++)
                {
                    float a0 = ang0 + sweep * (d / (float)dashCount);
                    float a1 = ang0 + sweep * ((d + 1) / (float)dashCount);
                    float take = MathHelper.Lerp(a0, a1, fillRatio);

                    int n = Math.Max(2, (int)((take - a0) * r / step));
                    Vector2 pPrev = c + a0.ToRotationVector2() * r;
                    for (int i = 1; i <= n; i++)
                    {
                        float a = MathHelper.Lerp(a0, take, i / (float)n);
                        Vector2 p = c + a.ToRotationVector2() * r;
                        LineOrbs(pPrev, p, step, scMin, scMax, col);
                        pPrev = p;
                    }
                }
            }

            void ZigZagToRing(float ang, float rStart, float rEnd, float amp, float segLen, Color col)
            {
                Vector2 n = ang.ToRotationVector2();
                Vector2 t = n.RotatedBy(MathHelper.PiOver2);
                float L = Math.Max(0f, rEnd - rStart);
                int segs = Math.Max(1, (int)(L / segLen));
                Vector2 prev = center + n * rStart + t * (amp);
                int side = -1;

                for (int i = 1; i <= segs; i++)
                {
                    float r = rStart + segLen * i;
                    if (i == segs) r = rEnd + EPS_HIT;

                    Vector2 cur = center + n * r + t * (amp * side);
                    LineOrbs(prev, cur, 4.0f * Projectile.scale, 0.90f * Projectile.scale, 1.20f * Projectile.scale, col);
                    prev = cur;
                    side *= -1;
                }
            }

            // ===== 外层：9角星芒爆炸（GlowOrb）=====
            int starPoints = 9;
            int pointsPerEdge = 50;
            float innerR = R_outer_in * 0.55f; // 内圈半径
            float outerR = R_outer_in * 1.05f; // 外圈半径


            for (int sp = 0; sp < starPoints; sp++)
            {
                float ang0 = MathHelper.TwoPi * sp / starPoints;
                float ang1 = MathHelper.TwoPi * (sp + 1) / starPoints;

                Vector2 inner0 = center + ang0.ToRotationVector2() * innerR;
                Vector2 outerMid = center + ((ang0 + ang1) / 2f).ToRotationVector2() * outerR;
                Vector2 inner1 = center + ang1.ToRotationVector2() * innerR;

                Vector2[] verts = { inner0, outerMid, inner1 };

                for (int v = 0; v < 2; v++)
                {
                    Vector2 a = verts[v];
                    Vector2 b = verts[v + 1];
                    for (int i = 0; i <= pointsPerEdge; i++)
                    {
                        float t = i / (float)pointsPerEdge;
                        Vector2 pos = Vector2.Lerp(a, b, t);
                        var orb = new GlowOrbParticle(
                            pos,
                            Vector2.Zero,
                            false,
                            Main.rand.Next(14, 20),
                            Main.rand.NextFloat(0.9f, 1.3f) * Projectile.scale,
                            pal[Main.rand.Next(pal.Length)],
                            true, false, true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }
            }


            int outerPts = Math.Max(220, (int)(MathHelper.TwoPi * R_outer_out / (4.0f * Projectile.scale)));
            RingOrbs(center, R_outer_out, outerPts, 0.90f * Projectile.scale, 1.15f * Projectile.scale, pal[0]);
            RingOrbs(center, R_outer_in, outerPts, 0.85f * Projectile.scale, 1.10f * Projectile.scale, pal[1]);

            // ===== 内部结构保持不变 =====

            // 主辐条
            float[] spokeDeg =
            {
        -92f, -58f, -22f, 12f, 48f, 86f, 132f, 182f, 220f, 258f, 302f, 338f
    };
            foreach (float deg in spokeDeg)
            {
                float ang = MathHelper.ToRadians(deg);
                Vector2 a = center + ang.ToRotationVector2() * (R_coreB * 1.1f);
                Vector2 b = center + ang.ToRotationVector2() * (R_spokeEnd + EPS_HIT);
                LineOrbs(a, b, 4.2f * Projectile.scale, 0.95f * Projectile.scale, 1.25f * Projectile.scale, pal[0]);
            }

            // 扇区折线
            (float ang, float r0Mul, float r1Mul, float ampMul, float segMul)[] zigs =
            {
        (MathHelper.ToRadians(-30f), 0.55f, 0.98f, 0.10f, 0.18f),
        (MathHelper.ToRadians( 28f), 0.52f, 0.98f, 0.12f, 0.17f),
        (MathHelper.ToRadians( 58f), 0.55f, 0.98f, 0.10f, 0.18f),
        (MathHelper.ToRadians(210f), 0.55f, 0.98f, 0.10f, 0.18f),
        (MathHelper.ToRadians(255f), 0.55f, 0.98f, 0.10f, 0.18f),
        (MathHelper.ToRadians(302f), 0.52f, 0.98f, 0.12f, 0.17f),
    };
            foreach (var z in zigs)
            {
                float r0 = MathHelper.Lerp(R_coreB * 1.1f, R_mid, z.r0Mul);
                float r1 = MathHelper.Lerp(R_mid, R_spokeEnd, z.r1Mul);
                float amp = (R_outer_in - R_mid) * z.ampMul;
                float seg = (R_spokeEnd - R_coreB) * z.segMul;
                ZigZagToRing(z.ang, r0, r1, amp, seg, pal[1]);
            }

            // 内侧短划
            int tickCount = 24;
            for (int i = 0; i < tickCount; i++)
            {
                float ang = MathHelper.TwoPi * i / tickCount + Main.rand.NextFloat(-0.03f, 0.03f);
                Vector2 n = ang.ToRotationVector2();
                Vector2 t = n.RotatedBy(MathHelper.PiOver2);
                bool radial = (i % 3 != 0);
                float len = radial ? (8f * s) : (10f * s);
                Vector2 a = center + n * (R_tick - (radial ? len * 0.4f : len * 0.5f));
                Vector2 b = radial ? (a + n * len) : (a + t * len);

                LineOrbs(a, b, 3.6f * Projectile.scale, 0.85f * Projectile.scale, 1.05f * Projectile.scale, pal[(i & 1) == 0 ? 0 : 1]);
            }

            // 内圈虚线
            DashedArc(center, R_coreA, -MathHelper.PiOver2, MathHelper.TwoPi, 10, 0.35f,
                      4.6f * Projectile.scale, 0.80f * Projectile.scale, 1.05f * Projectile.scale, pal[2]);

            // 中心小圆 + 核心点
            int corePts = Math.Max(40, (int)(MathHelper.TwoPi * R_coreB / (3.6f * Projectile.scale)));
            RingOrbs(center, R_coreB, corePts, 0.85f * Projectile.scale, 1.10f * Projectile.scale, pal[0]);

            PutOrb(center, 1.35f * Projectile.scale, pal[1], 14, 20);
        }






    }
}