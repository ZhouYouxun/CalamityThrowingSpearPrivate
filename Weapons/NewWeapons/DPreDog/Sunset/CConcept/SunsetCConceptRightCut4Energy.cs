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
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptRightCut4Energy : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/RC/能量";

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(
                "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/RC/能量"
            ).Value;

            CalamityUtils.DrawAfterimagesCentered(
                Projectile,
                ProjectileID.Sets.TrailingMode[Projectile.type],
                lightColor,
                1,
                texture
            );

            return false;
        }



        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
            Projectile.scale = 0.6f;
        }
        public override void OnSpawn(IEntitySource source)
        {

            //// 生成粒子爆炸效果
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
        private bool flightInited = false; // 是否已初始化飞行方向
        private Vector2 launchDir;         // 出生时的“前进方向”，只记录一次




        // 飞行计时器（原特效用）
        private int matterTimer = 0;

        // 能量弹幕的螺旋飞行半径
        private float radius = -1f;

        // 能量弹幕当前公转角度
        private float orbitAngle = 0f;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // ================================
            // 不动：你的全部飞行特效代码
            //（原有特效全部保留，不改一字）
            // ================================
            matterTimer++;

            Vector2 center = Projectile.Center;
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 backDir = -forward;
            Vector2 perp = forward.RotatedBy(MathHelper.Pi / 2f);

            // 调色盘：深黄土、暗棕
            Color dustDark = new Color(40, 30, 25);
            Color dustBrown = new Color(120, 80, 40);
            Color sparkColor = new Color(220, 180, 90);
            Color critStart = new Color(245, 210, 120);
            Color critEnd = new Color(255, 240, 160);

            float t = matterTimer / 60f;

            // ============================================================
            // ① SparkParticle：毛刺状“物质裂纹轨迹”
            // ============================================================
            {
                int lines = 5;
                float baseBackOffset = 6f;
                float maxAngle = MathHelper.ToRadians(22f);

                float wobble = (float)Math.Sin(t * 3.0f) * MathHelper.ToRadians(10f);

                for (int i = 0; i < lines; i++)
                {
                    float angleOffset = Main.rand.NextFloat(-maxAngle, maxAngle);
                    float finalAngle = angleOffset + wobble;
                    Vector2 dir = backDir.RotatedBy(finalAngle);
                    Vector2 pos = center + backDir * baseBackOffset;
                    Vector2 vel = dir * Main.rand.NextFloat(0.15f, 0.28f);

                    Particle trail = new SparkParticle(
                        pos,
                        vel,
                        false,
                        28,
                        0.9f,
                        sparkColor
                    );
                    GeneralParticleHandler.SpawnParticle(trail);

                    int dustType = 0;
                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(
                            pos + dir * Main.rand.NextFloat(1f, 4f)
                                + perp * Main.rand.NextFloat(-1.3f, 1.3f),
                            dustType,
                            vel * 0.15f,
                            0,
                            Color.Lerp(dustDark, dustBrown, Main.rand.NextFloat(0.3f, 1f)),
                            0.6f
                        );
                        d.noGravity = true;
                    }
                }
            }

            // ============================================================
            // ② CritSpark：无序中的秩序 —— 双螺旋“雨刷型”后喷
            // ============================================================
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    int side = s;
                    float phase = t * 3.0f;
                    float swing = side * 0.6f * (float)Math.Sin(phase);
                    float twist = (float)Math.Sin(phase * 1.7f + side * 0.8f) * 0.25f;

                    Vector2 dir = backDir.RotatedBy(swing + twist);

                    float radialOffset = 10f * (float)Math.Sin(phase * 2.3f + side);
                    Vector2 offset = perp * side * radialOffset;

                    Vector2 spawnPos = center + offset;

                    Vector2 vel = dir * Main.rand.NextFloat(7f, 11f)
                                  + perp * side * Main.rand.NextFloat(1.5f, 3.5f);

                    CritSpark spark = new CritSpark(
                        spawnPos,
                        vel,
                        critStart,
                        critEnd,
                        1.2f,
                        20
                    );
                    GeneralParticleHandler.SpawnParticle(spark);

                    int dustType = 0;
                    Dust d = Dust.NewDustPerfect(
                        spawnPos,
                        dustType,
                        vel * 0.12f,
                        0,
                        Color.Lerp(dustDark, dustBrown, Main.rand.NextFloat()),
                        0.8f);
                    d.noGravity = true;
                }
            }

            // ============================================================
            // ③ 后方 45° 喷射分流 Dust
            // ============================================================
            {
                float baseAngleOffset = (float)Math.PI / 4f;
                float spread = MathHelper.ToRadians(10f);
                int dustPerSide = 3;

                for (int s = -1; s <= 1; s += 2)
                {
                    Vector2 baseDir = backDir.RotatedBy(baseAngleOffset * s);

                    for (int i = 0; i < dustPerSide; i++)
                    {
                        float lerp = i / (float)Math.Max(1, dustPerSide - 1);
                        float angOffset = MathHelper.Lerp(-spread, spread, lerp);

                        Vector2 dir = baseDir.RotatedBy(angOffset);
                        Vector2 vel = dir * Main.rand.NextFloat(2.5f, 5.5f);

                        int dustType = 0;
                        Dust d = Dust.NewDustPerfect(
                            center,
                            dustType,
                            vel,
                            0,
                            Color.Lerp(dustDark, dustBrown, Main.rand.NextFloat(0.2f, 0.8f)),
                            0.75f);
                        d.noGravity = true;
                    }
                }
            }

            // ============================================================
            // ⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐
            //  ★★★     新的“能量弹幕螺旋追踪”飞行逻辑（核心）    ★★★
            // ⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐
            // ============================================================

            // 找目标
            NPC target = Projectile.FindTargetWithinRange(900f);
            if (target == null)
                return;

            Vector2 targetCenter = target.Center;

            // 初始化追踪系统
            if (radius == -1f)
            {
                radius = 140f;                 // 初始公转半径
                orbitAngle = Projectile.DirectionTo(targetCenter).ToRotation();
            }

            // 每帧角度推进（顺时针 / 逆时针随机）
            float rotSpeed = 0.25f; // 越大旋转越快
            orbitAngle += rotSpeed;

            // 半径快速收缩
            radius *= 0.94f;
            if (radius < 28f)
                radius = 28f;

            // 计算螺旋轨道位置
            Vector2 desiredPos = targetCenter +
                                 new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * radius;

            // 朝轨道点冲刺
            Vector2 desiredVel = (desiredPos - center).SafeNormalize(Vector2.Zero) * 22f;

            // 平滑速度
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.25f);
        }









        private NPC FindClosestEnemy(Vector2 center, float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;

            foreach (NPC n in Main.npc)
            {
                if (n.CanBeChasedBy() && !n.friendly)
                {
                    float d = Vector2.Distance(center, n.Center);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        closest = n;
                    }
                }
            }
            return closest;
        }



        public override void OnKill(int timeLeft)
        {


        }




        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/焦土开枪")
            {
                Volume = 0.3f,
                Pitch = 0.0f
            }, Projectile.Center);

            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetCConceptPBuff>(), 300);

            // 生成菱形魔法阵（小弹幕专用）
            CreateMagicDiamond(target.Center);

            {
                // 调色盘（只用金色和紫色）
                Color[] palette = {
            new Color(255, 215, 0),   // 金色
            new Color(120, 90, 160)   // 深紫
        };

                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 center = Projectile.Center;

                // ====== 向前散射闪电丝（简化版，小范围） ======
                int rayCount = 4;
                int raySegments = 12;
                float rayLength = 96f * Projectile.scale;
                float step = rayLength / raySegments;
                float jitter = 4f * Projectile.scale;
                float curviness = 0.3f;

                Vector2 NoisyDir(Vector2 baseDir, float strength)
                {
                    float a = Main.rand.NextFloat(-strength, strength);
                    return baseDir.RotatedBy(a).SafeNormalize(baseDir);
                }

                for (int r = 0; r < rayCount; r++)
                {
                    Vector2 dir = forward.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f)));
                    Vector2 p = center;
                    Vector2 curDir = dir;

                    for (int s = 0; s < raySegments; s++)
                    {
                        curDir = Vector2.Lerp(curDir, NoisyDir(curDir, curviness), 0.9f).SafeNormalize(curDir);
                        Vector2 jitterVec = Main.rand.NextVector2Circular(jitter, jitter);
                        p += curDir * step + jitterVec;

                        var sp = new SparkParticle(
                            p,
                            curDir * Main.rand.NextFloat(2.0f, 4.0f),
                            false,
                            Main.rand.Next(14, 18),
                            Main.rand.NextFloat(0.7f, 1.0f) * Projectile.scale,
                            palette[Main.rand.Next(palette.Length)]
                        );
                        GeneralParticleHandler.SpawnParticle(sp);

                        var orb = new GlowOrbParticle(
                            p,
                            NoisyDir(curDir, 0.5f) * Main.rand.NextFloat(0.2f, 0.8f),
                            false,
                            Main.rand.Next(8, 12),
                            Main.rand.NextFloat(0.6f, 0.9f) * Projectile.scale,
                            palette[Main.rand.Next(palette.Length)],
                            true, false, true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }
            }
        }


        private void CreateMagicDiamond(Vector2 center)
        {
            float scaleMult = Projectile.scale;

            int layers = 2;         // 菱形层数
            int pointsPerEdge = 12; // 每条边的点数
            float baseRadius = 36f * scaleMult;  // 第一层菱形半径
            float gap = 10f * scaleMult;         // 层间距

            // 调色盘（只用金色和紫色）
            Color[] palette = {
        new Color(255, 215, 0),
        new Color(120, 90, 160)
    };

            // 为本次菱形生成一个随机角度偏移
            float rotationOffset = Main.rand.NextFloat(MathHelper.TwoPi);

            // 遍历层
            for (int l = 0; l < layers; l++)
            {
                float r = baseRadius + l * gap;

                // 四个顶点方向（0°/90°/180°/270°），加上随机旋转
                Vector2[] corners = {
            new Vector2( r, 0).RotatedBy(rotationOffset),
            new Vector2( 0, r).RotatedBy(rotationOffset),
            new Vector2(-r, 0).RotatedBy(rotationOffset),
            new Vector2( 0,-r).RotatedBy(rotationOffset)
        };

                // 每条边插值
                for (int c = 0; c < 4; c++)
                {
                    Vector2 start = corners[c];
                    Vector2 end = corners[(c + 1) % 4];

                    for (int i = 0; i <= pointsPerEdge; i++)
                    {
                        float t = i / (float)pointsPerEdge;
                        Vector2 pos = center + Vector2.Lerp(start, end, t);

                        // GlowOrb 替代 Dust
                        var orb = new GlowOrbParticle(
                            pos,
                            Vector2.Zero,
                            false,
                            Main.rand.Next(10, 16),
                            Main.rand.NextFloat(0.6f, 1.0f) * scaleMult,
                            palette[(i + l) % palette.Length],
                            true, false, true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }
            }

            // 额外点缀：内部随机光球
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(baseRadius * 0.6f, baseRadius * 0.6f);
                var orb = new GlowOrbParticle(
                    center + offset,
                    Vector2.Zero,
                    false,
                    Main.rand.Next(8, 14),
                    Main.rand.NextFloat(0.6f, 1.0f) * scaleMult,
                    palette[Main.rand.Next(palette.Length)],
                    true, false, true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }






    }
}