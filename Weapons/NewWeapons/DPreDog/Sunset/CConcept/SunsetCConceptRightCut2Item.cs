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
    internal class SunsetCConceptRightCut2Item : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/RC/物质";

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(
                "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/RC/物质"
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
            Projectile.penetrate = 2;
            Projectile.timeLeft = 200;
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

            // --- 物质弹幕：初始化减速到 5% ---
            initialSpeed = Projectile.velocity.Length();       // 记录出生时的原始速度
            Projectile.velocity *= 0.05f;                      // 将速度降到 5%


            Player owner = Main.player[Projectile.owner];
            int totalCrit = (int)Math.Round(owner.GetTotalCritChance(Projectile.DamageType));
            Projectile.CritChance = totalCrit;

        }

        private float initialSpeed = 0f;


        private int lifeTimer = 0; // 存活帧数计时器
        private bool flightInited = false; // 是否已初始化飞行方向
        private Vector2 launchDir;         // 出生时的“前进方向”，只记录一次

        private int matterTimer = 0;
        private NPC materialTarget = null;



        private NPC FindMaterialTarget(Vector2 center, Vector2 forwardVel)
        {
            NPC closest = null;
            float maxDetect = 800f;                 // 最大搜索距离
            float forwardAngle = forwardVel.ToRotation();
            float cone = MathHelper.ToRadians(20f); // 前方±20度锥形

            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy())
                    continue;

                float dist = Vector2.Distance(center, npc.Center);
                if (dist > maxDetect)
                    continue;

                Vector2 toNpc = (npc.Center - center).SafeNormalize(Vector2.UnitX);
                float angToNpc = toNpc.ToRotation();

                float diff = Math.Abs(MathHelper.WrapAngle(angToNpc - forwardAngle));

                // 必须在前方 ±20° 扇形内才算目标
                if (diff <= cone)
                {
                    maxDetect = dist;  // 继续取最近的
                    closest = npc;
                }
            }

            return closest;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // ======================================================
            // 物质弹幕：轻度追踪（仅追踪前方 ±20° 扇形的敌人）
            // ======================================================

            // 搜索或更新目标
            materialTarget = FindMaterialTarget(Projectile.Center, Projectile.velocity);

            // 如果找到目标 → 轻微调整朝向
            if (materialTarget != null && materialTarget.active)
            {
                Vector2 toEnemy = (materialTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                float curRot = Projectile.velocity.ToRotation();
                float targetRot = toEnemy.ToRotation();

                // 方向差
                float diff = MathHelper.WrapAngle(targetRot - curRot);

                // 每帧最多转 6°（轻度追踪，不会太强）
                float maxTurn = MathHelper.ToRadians(6f);

                float turnAmount = MathHelper.Clamp(diff, -maxTurn, maxTurn);

                // 应用旋转
                Projectile.velocity = Projectile.velocity.RotatedBy(turnAmount);
            }


            // ======================================================
            // 物质弹幕：指数加速（上限 = 初始速度的 150%）
            // ======================================================
            if (initialSpeed > 0f)
            {
                // 每帧指数加速
                Projectile.velocity *= 1.02f;

                // 若超过上限（150% 初速），强制 clamp
                float maxSpeed = initialSpeed * 1.5f;
                if (Projectile.velocity.Length() > maxSpeed)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * maxSpeed;
                }
            }



            matterTimer++;

            Vector2 center = Projectile.Center;
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 backDir = -forward;
            Vector2 perp = forward.RotatedBy(MathHelper.PiOver2);

            // 调色盘：深黄土、暗棕
            Color matterCore = new Color(210, 170, 90);        // 物质核心偏金黄
            Color matterDark = new Color(80, 50, 30);          // 深棕
            Color matterSmoke = new Color(150, 110, 60) * 0.9f;

            float t = matterTimer / 60f;

            // ============================================================
            // ① 裂纹主干：CrackParticle —— 更细、更随机、双裂纹结构
            // ============================================================
            {
                int count = 2; // 每次两个裂纹

                for (int i = 0; i < count; i++)
                {
                    // 随机偏移：让裂纹不重叠（自然、优雅）
                    Vector2 posOffset =
                        forward * Main.rand.NextFloat(-2f, 2f) +
                        perp * Main.rand.NextFloat(-2f, 2f);

                    Vector2 spawnPos = center + posOffset;

                    // 随机角度抖动（但仍紧贴后方方向）
                    float angleJitter = Main.rand.NextFloat(-0.35f, 0.35f);
                    Vector2 crackDir = backDir.RotatedBy(angleJitter);

                    // 随机速度，让裂纹有大小不一的拉扯感
                    Vector2 crackVel = crackDir * Main.rand.NextFloat(1.6f, 3.3f);

                    // 缩放更小（你要的 0.5）
                    float scaleSmall = Main.rand.NextFloat(0.45f, 0.6f);

                    CrackParticle crack = new CrackParticle(
                        spawnPos,                                  // 轻微偏移后的中心
                        crackVel,                                  // 后方速度 + 随机
                        matterCore,                                // 物质色
                        new Vector2(scaleSmall, scaleSmall),        // 更小、更秀气的裂纹
                        Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
                        scaleSmall,                                // 初始缩放
                        scaleSmall,                                // 最终缩放保持一致
                        Main.rand.Next(22, 34)                     // 寿命有点随机
                    );

                    GeneralParticleHandler.SpawnParticle(crack);
                }
            }

            // ============================================================
            // ② oldPos 轨迹：HeavySmoke + Dust —— 让飞过的路径像“物质撕开的痕迹”
            // ============================================================
            if (matterTimer % 3 == 0)
            {
                for (int i = 2; i < Projectile.oldPos.Length; i += 4)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero)
                        continue;

                    Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f;

                    Vector2 smokeVel = backDir * Main.rand.NextFloat(1.6f, 3.6f) +
                                       perp * Main.rand.NextFloat(-1.5f, 1.5f);

                    HeavySmokeParticle smoke = new HeavySmokeParticle(
                        pos,
                        smokeVel,
                        matterSmoke,
                        26,
                        0.9f + Main.rand.NextFloat(0.3f),
                        0.85f,
                        MathHelper.ToRadians(Main.rand.NextFloat(-3f, 3f)),
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);

                    int dustType = 0;
                    Dust d = Dust.NewDustPerfect(
                        pos,
                        dustType,
                        smokeVel * 0.12f,
                        0,
                        Color.Lerp(matterDark, matterCore, Main.rand.NextFloat(0.2f, 0.7f)),
                        0.8f
                    );
                    d.noGravity = true;
                }
            }

            // ============================================================
            // ③ 近身 Dust 数学螺旋：两条相位相反的“物质螺旋”
            //     —— 每帧形态改变，保证数学美感
            // ============================================================
            {
                float baseRadius = 8f + 3f * (float)Math.Sin(t * 2.1f); // 半径随时间呼吸
                int dustSeg = 10;

                for (int arm = 0; arm < 2; arm++)
                {
                    float armPhase = t * (arm == 0 ? 2.7f : -2.2f); // 两条臂相反相位

                    for (int i = 0; i < dustSeg; i++)
                    {
                        float k = i / (float)dustSeg;
                        float spiralR = baseRadius + 2.5f * k;
                        float ang = armPhase + k * MathHelper.TwoPi * 0.25f;

                        // 在 forward / perp 平面中做螺旋
                        Vector2 offset =
                            forward * (float)Math.Cos(ang) * spiralR +
                            perp * (float)Math.Sin(ang) * spiralR;

                        Vector2 pos = center + offset;

                        int dustType = 0;
                        Dust d = Dust.NewDustPerfect(
                            pos,
                            dustType,
                            backDir * Main.rand.NextFloat(0.25f, 0.7f),
                            0,
                            Color.Lerp(matterDark, matterCore, 0.3f + 0.5f * k),
                            0.7f
                        );
                        d.noGravity = true;
                    }
                }
            }

            // ============================================================
            // ④ DirectionalPulseRing：沿飞行轴的椭圆冲击波（强化版）
            //     —— rotation + 45°，更夸张、更明显的物质层“推开”脉冲
            // ============================================================
            if (matterTimer % 4 == 0)
            {
                Particle pulse = new DirectionalPulseRing(
                    center,
                    backDir * 1.8f,                 // 更强的外扩速度（原 1.1f）
                    matterCore,                     // 主色不变
                    new Vector2(1.0f, 3.0f),        // 椭圆拉得更夸张（原 2.3f）
                    Projectile.rotation + MathHelper.PiOver4,  // ✨加 45° 旋转
                    0.26f,                          // 初始缩放更大（原 0.18）
                    0.055f,                         // 最终缩放更大（原 0.035）
                    26                              // 更长寿命（原 22）
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }


            // ============================================================
            // ⑤ 小型十字星 CritSpark：沿轨迹随机闪现“物质晶点”
            //     —— 我选用小型十字星，可以多放一些
            // ============================================================
            if (matterTimer % 3 == 0)
            {
                int count = 3;

                for (int i = 0; i < count; i++)
                {
                    Vector2 origin = center;

                    // 偶尔从 oldPos 中取点作为闪光位置，强化“轨迹”感
                    if (Projectile.oldPos.Length > 4)
                    {
                        int idx = 1 + i;
                        if (idx < Projectile.oldPos.Length && Projectile.oldPos[idx] != Vector2.Zero)
                            origin = Projectile.oldPos[idx] + Projectile.Size * 0.5f;
                    }

                    Vector2 dir = backDir.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4));
                    Vector2 vel = dir * Main.rand.NextFloat(4f, 8f);

                    CritSpark spark = new CritSpark(
                        origin,
                        vel,
                        Color.Lerp(matterCore, Color.White, 0.25f),   // 起始偏暖
                        Color.Lerp(matterCore, Color.White, 0.85f),   // 结束更亮
                        0.95f,
                        18
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }



        public override void OnKill(int timeLeft)
        {
            Vector2 center = Projectile.Center;

            Color dustDark = new Color(40, 30, 25);
            Color dustBrown = new Color(120, 80, 40);
            Color sparkColor = new Color(220, 180, 90);
            Color critStart = new Color(245, 210, 120);
            Color critEnd = new Color(255, 240, 160);
            Color sparkleCore = new Color(255, 230, 150);

            int dustType = 0;

            float baseRot = Main.rand.NextFloat(MathHelper.TwoPi);

            // ============================================================
            // ① 外层等边三角形（主结构）
            // ============================================================
            float triR = 112f; // 稍微放大一点，增强仪式感
            Vector2[] v = new Vector2[3];

            for (int i = 0; i < 3; i++)
            {
                float ang = baseRot + MathHelper.TwoPi * i / 3f;
                v[i] = center + ang.ToRotationVector2() * triR;
            }


            // ============================================================
            // ③ 内层“小等边三角形”（额外结构：使图案更复杂）
            // ============================================================
            float triInner = triR * 0.55f;
            Vector2[] v2 = new Vector2[3];


            // ============================================================
            // 小三角边：CritSpark 能量边缘（旋转 + 辐射扩散）
            // ============================================================
            for (int edge = 0; edge < 3; edge++)
            {
                Vector2 a = v2[edge];
                Vector2 b = v2[(edge + 1) % 3];

                int seg = 20; // 边上 20 个粒子

                for (int i = 0; i < seg; i++)
                {
                    float k = i / (float)(seg - 1);
                    Vector2 pos = Vector2.Lerp(a, b, k);

                    // =============================
                    // 旋转角度：沿三角边推进时不断增加（数学美感）
                    // =============================
                    float rot = baseRot + k * MathHelper.TwoPi * 0.33f
                                + Main.rand.NextFloat(-0.15f, 0.15f);

                    // 粒子最终方向 = 从中心指向此边点 + 旋转扰动
                    Vector2 outward = (pos - center).SafeNormalize(Vector2.UnitY).RotatedBy(rot);

                    // 爆散速度（真正能看见的速度）
                    Vector2 vel = outward * Main.rand.NextFloat(9f, 18f);

                    // =============================
                    // CritSpark：小型十字星粒子（爆散）
                    // =============================
                    CritSpark spark = new CritSpark(
                        pos,
                        vel,
                        Color.White,              // 起始色
                        Color.Lerp(Color.LightBlue, Color.White, 0.5f), // 结束色
                        1f,                       // 缩放
                        20                        // 寿命
                    );

                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }


            // ============================================================
            // ⑤ 内部“顶点裂纹” CritSpark（保持）
            // ============================================================
            for (int i = 0; i < 3; i++)
            {
                Vector2 vp = v[i];
                Vector2 dir = (vp - center).SafeNormalize(Vector2.UnitY);

                for (int r = 0; r < 4; r++)
                {
                    Vector2 spawnPos = Vector2.Lerp(center, vp, 0.25f + 0.15f * r);

                    CritSpark spark = new CritSpark(
                        spawnPos,
                        dir * Main.rand.NextFloat(9f, 14f),
                        critStart,
                        critEnd,
                        1.25f,
                        18
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            // ============================================================
            // ⑥ 三角形主体闪光：GenericSparkle
            // ============================================================
            for (int i = 0; i < 3; i++)
            {
                GenericSparkle g = new GenericSparkle(
                    v[i],
                    Vector2.Zero,
                    sparkleCore,
                    sparkColor,
                    Main.rand.NextFloat(2.0f, 2.6f),
                    12,
                    Main.rand.NextFloat(-0.03f, 0.03f),
                    1.8f
                );
                GeneralParticleHandler.SpawnParticle(g);
            }

            GenericSparkle g2 = new GenericSparkle(
                center,
                Vector2.Zero,
                sparkleCore,
                sparkColor,
                Main.rand.NextFloat(3.2f, 3.8f),
                16,
                Main.rand.NextFloat(-0.02f, 0.02f),
                2.2f
            );
            GeneralParticleHandler.SpawnParticle(g2);
        }





    }
}