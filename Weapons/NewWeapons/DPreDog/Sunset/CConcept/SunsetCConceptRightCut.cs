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
            Projectile.width = Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 30;
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

            Player owner = Main.player[Projectile.owner];
            int totalCrit = (int)Math.Round(owner.GetTotalCritChance(Projectile.DamageType));
            Projectile.CritChance = totalCrit;
        }



        private int lifeTimer = 0; // 存活帧数计时器
        private bool flightInited = false; // 是否已初始化飞行方向
        private Vector2 launchDir;         // 出生时的“前进方向”，只记录一次

        public override void AI()
        {

            // ===== 在 AI() 里，原来的飞行逻辑整段替换为下面这段 =====

            // 仅第一次进入时，记录“出生前进方向”
            if (!flightInited)
            {
                flightInited = true;

                // 以出生初速度为基准；若为零，用朝向/朝右兜底
                if (Projectile.velocity.LengthSquared() > 0.0001f)
                    launchDir = Vector2.Normalize(Projectile.velocity);
                else
                    launchDir = (Projectile.spriteDirection >= 0) ? Vector2.UnitX : -Vector2.UnitX;

                // 可选：保证 launchDir 单位化
                if (launchDir.LengthSquared() < 0.5f)
                    launchDir = (Projectile.direction >= 0) ? Vector2.UnitX : -Vector2.UnitX;

                lifeTimer = 0; // 第一次进来重置计时
            }

            lifeTimer++;

            // iOS 风格缓动参数（可调）
            const int backFrames = 5;     // 退后用 5 帧
            const int dashAccelFrames = 20; // 前冲的加速阶段时长
            const float backSpeedStart = 10f; // 刚退后时较快
            const float backSpeedEnd = 2f;  // 退后尾声放缓
            const float dashSpeedStart = 6f;  // 前冲起步较慢
            const float dashSpeedEnd = 228f; // 前冲末端较快（爆发）

            // 固定正向 / 反向
            Vector2 dirFwd = launchDir;       // 一直用“出生时前进方向”
            Vector2 dirBack = -launchDir;      // 其反向：退后用

            float speed;

            // 阶段一：退后（仅前 5 帧，ease-out：先快后慢）
            if (lifeTimer <= backFrames)
            {
                float t = lifeTimer / (float)backFrames;         // 0→1
                float easeOut = 1f - MathF.Pow(1f - t, 0.7f);    // 缓出
                                                                 // 先快后慢：从 backSpeedStart 过渡到 backSpeedEnd
                speed = MathHelper.Lerp(backSpeedStart, backSpeedEnd, easeOut);
                Projectile.velocity = dirBack * speed;



                // —— 退后阶段：正后方扇形喷射特效（仅 backFrames 帧内播放） ——
                // 🔧 可调参数
                float coneHalfAngle = MathHelper.ToRadians(40f); // 扇形半角
                float radialMin = 6f;     // 出生点离中心最小半径
                float radialMax = 22f;    // 出生点离中心最大半径
                float speedMin = 0.8f;    // 粒子初速区间
                float speedMax = 3.2f;

                int exoPerTick = 4;       // EXO 主体光束
                int orbPerTick = 5;       // GlowOrb 丝带点缀
                int sparkPerTick = 6;     // 线性火花
                int squarePerTick = 2;    // 方形碎片

                // 🎨 调色盘：金色 + 紫色
                Color[] palette = {
    new Color(255, 215, 0),   // 金
    new Color(120, 90, 160)   // 深紫
};

                // 基向量：前/后/切向（以出生方向为准）
                Vector2 fwd = dirFwd;
                Vector2 back = dirBack;
                Vector2 tan = fwd.RotatedBy(MathHelper.PiOver2);

                // 工具函数
                Vector2 RandConeDir() => back.RotatedBy(Main.rand.NextFloat(-coneHalfAngle, coneHalfAngle));
                Vector2 RandSpawnPos()
                {
                    Vector2 d = RandConeDir();
                    float r = Main.rand.NextFloat(radialMin, radialMax);
                    return Projectile.Center + d * r + tan * Main.rand.NextFloat(-5f, 5f);
                }

                // 1) EXO 主体光束
                for (int i = 0; i < exoPerTick; i++)
                {
                    Vector2 pos = RandSpawnPos();
                    Vector2 vel = RandConeDir() * Main.rand.NextFloat(speedMin, speedMax);
                    var exo = new SquishyLightParticle(
                        pos, vel,
                        Main.rand.NextFloat(0.28f, 0.36f) * Projectile.scale,
                        palette[Main.rand.Next(palette.Length)],
                        Main.rand.Next(18, 26),
                        opacity: 1f,
                        squishStrenght: 1f,
                        maxSquish: Main.rand.NextFloat(2.2f, 3.0f),
                        hueShift: 0f
                    );
                    GeneralParticleHandler.SpawnParticle(exo);
                }

                // 2) GlowOrb 丝带亮点
                for (int i = 0; i < orbPerTick; i++)
                {
                    Vector2 pos = RandSpawnPos();
                    Vector2 vel = RandConeDir() * Main.rand.NextFloat(0.4f, 1.5f);
                    var orb = new GlowOrbParticle(
                        pos, vel,
                        false,
                        Main.rand.Next(12, 18),
                        Main.rand.NextFloat(0.9f, 1.3f) * Projectile.scale,
                        palette[Main.rand.Next(palette.Length)],
                        true, false, true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }

                // 3) 火花（锐利喷射线）
                for (int i = 0; i < sparkPerTick; i++)
                {
                    Vector2 pos = RandSpawnPos();
                    Vector2 vel = RandConeDir() * Main.rand.NextFloat(1.5f, 3.5f);
                    var sp = new SparkParticle(
                        pos, vel,
                        false,
                        Main.rand.Next(18, 24),
                        Main.rand.NextFloat(0.8f, 1.1f) * Projectile.scale,
                        palette[Main.rand.Next(palette.Length)]
                    );
                    GeneralParticleHandler.SpawnParticle(sp);
                }

                // 4) 方形碎片（稀疏点缀）
                for (int i = 0; i < squarePerTick; i++)
                {
                    Vector2 pos = RandSpawnPos();
                    Vector2 vel = (RandConeDir() + tan * Main.rand.NextFloat(-0.25f, 0.25f)).SafeNormalize(back)
                                  * Main.rand.NextFloat(0.8f, 2.0f);
                    var sq = new SquareParticle(
                        pos, vel,
                        false,
                        Main.rand.Next(20, 28),
                        Main.rand.NextFloat(1.0f, 1.6f) * Projectile.scale,
                        palette[Main.rand.Next(palette.Length)]
                    );
                    GeneralParticleHandler.SpawnParticle(sq);
                }



            }
            else
            {
                // 阶段二：前冲（第 6 帧开始，ease-in：先慢后快）
                float t = (lifeTimer - backFrames) / (float)dashAccelFrames; // 0→1
                t = Math.Clamp(t, 0f, 1f);
                float easeIn = MathF.Pow(t, 1.8f); // 缓入

                // 从 dashSpeedStart 加速到 dashSpeedEnd；超过加速阶段后保持 dashSpeedEnd
                speed = MathHelper.Lerp(dashSpeedStart, dashSpeedEnd, easeIn);
                Projectile.velocity = dirFwd * speed;
            }

            // 旋转跟随速度方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;








            // === 飞行科技蓝特效 ===
            if (Main.rand.NextBool(1)) // 大约每x帧一个
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