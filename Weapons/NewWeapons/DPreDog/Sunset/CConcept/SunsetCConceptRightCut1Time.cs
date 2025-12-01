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
    internal class SunsetCConceptRightCut1Time : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/RC/时间";
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(
                "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/RC/时间"
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
            Projectile.timeLeft = 600;
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
                                           // 计时器字段（类里已有就别重复加）

        private int timePatternTimer = 0;

        // --- 字段初始化（放在类里） ---
        private int turnTimer = 0;
        private int turnInterval = 18;   // 每 18 帧拐弯一次（你可以改）
        private bool turnLeft = true;    // 左右切换
        private float turnAngle45 = MathHelper.ToRadians(45f);
        private float turnAngle90 = MathHelper.ToRadians(90f);

        private int turnCount = 0;     // <<< 新增，用来计数

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;








            {

                // ======================================================
                // ★ 时间弹幕飞行逻辑（朝敌人倾向性拐弯 + 随机偏差 + 第4次必中）
                // ======================================================

                turnTimer++;

                // 常态加速
                Projectile.velocity *= 1.01f;

                NPC target = null;
                float maxDetect = 900f;

                // 搜索最近目标（距离 + 可视）
                for (int i = 0; i < Main.npc.Length; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                    {
                        float d = Vector2.Distance(Projectile.Center, npc.Center);
                        if (d < maxDetect)
                        {
                            maxDetect = d;
                            target = npc;
                        }
                    }
                }

                if (turnTimer >= turnInterval)
                {
                    turnTimer = 0;
                    turnCount++;   // <<< 每次拐弯计数

                    // ============================================================
                    // ① 计算朝向敌人的角度（有偏移 / 无偏移）
                    // ============================================================
                    float targetAngle;

                    if (target != null)
                    {
                        Vector2 dirToEnemy = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                        float baseAng = dirToEnemy.ToRotation();

                        // --------------------------------------------------------
                        // 前三次拐弯：允许 ±10° 随机偏移
                        // 第4次拐弯：精准命中，不偏移
                        // --------------------------------------------------------
                        if (turnCount < 4)
                        {
                            baseAng += Main.rand.NextFloat(
                                -MathHelper.ToRadians(5f),
                                 MathHelper.ToRadians(5f)
                            );
                        }
                        else
                        {
                            // 第4次：保持 baseAng 精准朝向目标
                        }

                        targetAngle = baseAng;
                    }
                    else
                    {
                        // 没目标 → 随机方向
                        targetAngle = Projectile.velocity.ToRotation() +
                                      Main.rand.NextFloat(-MathHelper.ToRadians(30f), MathHelper.ToRadians(30f));
                    }

                    // ============================================================
                    // ② 转向角度幅度：5°~90° 之间随机
                    // ============================================================
                    float turnStrength = Main.rand.NextFloat(
                        MathHelper.ToRadians(5f),
                        MathHelper.ToRadians(90f)
                    );

                    float currentAng = Projectile.velocity.ToRotation();
                    float diff = MathHelper.WrapAngle(targetAngle - currentAng);

                    float finalTurn = turnStrength * Math.Sign(diff); // 应用方向
                    Projectile.velocity = Projectile.velocity.RotatedBy(finalTurn);

                    // ============================================================
                    // ③ 拐弯瞬间：反喷特效（Dust + Square）
                    // ============================================================
                    Vector2 backDir2 = -Projectile.velocity.SafeNormalize(Vector2.UnitX);

                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 vel = backDir2.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) *
                                      Main.rand.NextFloat(4f, 9f);

                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center,
                            DustID.Torch,
                            vel,
                            0,
                            new Color(255, 230, 160),
                            1.1f
                        );
                        d.noGravity = true;
                    }

                    for (int k = 0; k < 2; k++)
                    {
                        Vector2 vel = backDir2 * Main.rand.NextFloat(3.5f, 6.5f);

                        SquareParticle sq = new SquareParticle(
                            Projectile.Center,
                            vel,
                            false,
                            25,
                            1.6f + Main.rand.NextFloat(0.4f),
                            new Color(255, 240, 150)
                        );
                        GeneralParticleHandler.SpawnParticle(sq);
                    }

                    // ============================================================
                    // ④ 拐弯音效：Item15
                    // ============================================================
                    SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);

                    // ============================================================
                    // ⑤ 重置计数（第4次必中之后重头来）
                    // ============================================================
                    if (turnCount >= 4)
                        turnCount = 0;
                }


            }










            timePatternTimer++;

            Vector2 center = Projectile.Center;
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = forward.RotatedBy(MathHelper.Pi / 2f);
            Vector2 backDir = -forward;

            // 调色
            Color dustMain = new Color(210, 170, 90);       // 土黄
            Color dustBright = new Color(255, 235, 150);    // 亮金
            Color squareColor = new Color(255, 240, 120);   // 亮黄方块

            float t = timePatternTimer / 60f;

            // ============================================================
            // ① 三重旋转椭圆钟盘（尺寸×3，密度×3，每帧形态和相位都在变）
            // ============================================================
            float baseA = 24f + 4f * (float)Math.Sin(t * 1.7f);
            float baseB = 15f + 3f * (float)Math.Cos(t * 2.1f);

            int segments = 36;

            for (int ring = 0; ring < 3; ring++)
            {
                float ringFactor = 0.6f + 0.25f * ring;
                float a = baseA * ringFactor;
                float b = baseB * ringFactor;

                float baseAngle = t * (1.1f + 0.3f * ring);

                for (int i = 0; i < segments; i++)
                {
                    float k = i / (float)segments;
                    float theta = MathHelper.TwoPi * k;

                    float ex = a * (float)Math.Cos(theta);
                    float ey = b * (float)Math.Sin(theta);

                    Vector2 local = ex * forward + ey * perp;
                    local = local.RotatedBy(baseAngle);

                    Vector2 pos = center + local;

                    float scale = 0.7f + 0.2f * ring;
                    Color useColor = (ring == 2) ? dustBright : dustMain;

                    Dust d = Dust.NewDustPerfect(pos, DustID.Torch, Vector2.Zero, 0, useColor, scale);
                    d.noGravity = true;
                }
            }

            // ============================================================
            // ② 多指针系统：时针 / 分针 / 秒针
            // ============================================================
            float hourAngle = t * 0.4f;
            float minuteAngle = t * 2.0f;
            float secondAngle = t * 5.0f;

            Vector2 hourDir = forward.RotatedBy(hourAngle);
            Vector2 minuteDir = forward.RotatedBy(minuteAngle);
            Vector2 secondDir = forward.RotatedBy(secondAngle);

            Vector2 hourEnd = center + hourDir * 18f;
            Vector2 minuteEnd = center + minuteDir * 24f;
            Vector2 secondEnd = center + secondDir * 30f;

            Dust dh = Dust.NewDustPerfect(hourEnd, DustID.GoldFlame, Vector2.Zero, 0, dustBright, 1.2f);
            dh.noGravity = true;

            Dust dm = Dust.NewDustPerfect(minuteEnd, DustID.GoldFlame, Vector2.Zero, 0, dustBright, 1.0f);
            dm.noGravity = true;

            Dust ds = Dust.NewDustPerfect(secondEnd, DustID.GoldFlame, Vector2.Zero, 0, dustBright, 0.8f);
            ds.noGravity = true;

            if (timePatternTimer % 2 == 0)
            {
                Dust dhTrail = Dust.NewDustPerfect(center + hourDir * 10f, DustID.Torch, hourDir * 0.4f, 0, dustMain, 0.8f);
                dhTrail.noGravity = true;

                Dust dmTrail = Dust.NewDustPerfect(center + minuteDir * 14f, DustID.Torch, minuteDir * 0.4f, 0, dustMain, 0.8f);
                dmTrail.noGravity = true;

                Dust dsTrail = Dust.NewDustPerfect(center + secondDir * 18f, DustID.Torch, secondDir * 0.5f, 0, dustMain, 0.7f);
                dsTrail.noGravity = true;
            }

            // ============================================================
            // ③ 「有序 + 无序」的时间喷射尾流（往后喷射）
            // ============================================================
            if (timePatternTimer % 3 == 0)
            {
                for (int j = -1; j <= 1; j++)
                {
                    float sideOffset = j * 0.35f + (float)Math.Sin(t * 2.8f + j) * 0.15f;
                    Vector2 mainDir = backDir.RotatedBy(sideOffset);

                    for (int k = 0; k < 3; k++)
                    {
                        float speed = Main.rand.NextFloat(2.2f, 4.4f);
                        float sideJitter = Main.rand.NextFloat(-0.25f, 0.25f);

                        Vector2 vel = mainDir.RotatedBy(sideJitter) * speed;

                        Dust jet = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, dustMain, 0.9f);
                        jet.noGravity = true;
                    }
                }
            }

            // ============================================================
            // ④ SquareParticle：时间碎片（强化版：大体积 + 高密度 + 规则喷射扇面）
            // ============================================================
            if (timePatternTimer % 1 == 0)
            {
                // 以 -forward 为轴，构建一个对称扇面（有序）
                int fanCount = 8;                         // 原本 2 → ×4
                float fanSpread = MathHelper.ToRadians(90f); // 扇面总角度

                for (int i = 0; i < fanCount; i++)
                {
                    float lerp = fanCount == 1 ? 0f : i / (float)(fanCount - 1);
                    float angOffset = MathHelper.Lerp(-fanSpread * 0.5f, fanSpread * 0.5f, lerp);

                    // 基础喷射方向（有序扇面）
                    Vector2 dir = backDir.RotatedBy(angOffset);

                    // 轻微随时间摆动，让扇面整体缓慢晃动（保持优雅规律）
                    float wobble = (float)Math.Sin(t * 2.0f + i) * 0.12f;
                    dir = dir.RotatedBy(wobble);

                    // 体积翻倍（原 1.8f 起步 → 3.6f 起步）
                    float scale = 2.6f + Main.rand.NextFloat(1.4f);

                    // 速度也提升一些，保证视觉支配感
                    float speed = Main.rand.NextFloat(3.0f, 6.0f);
                    Vector2 vel = dir * speed;

                    SquareParticle sq = new SquareParticle(
                        center + dir * Main.rand.NextFloat(4f, 10f),
                        vel,
                        false,
                        32,
                        scale,
                        squareColor
                    );
                    GeneralParticleHandler.SpawnParticle(sq);
                }
            }
        }



        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);


            Vector2 center = Projectile.Center;

            Color dustMain = new Color(210, 170, 90);
            Color dustBright = new Color(255, 235, 150);
            Color squareColor = new Color(255, 240, 120);
            Color smokeColor = new Color(200, 160, 80) * 0.8f; // 土黄且稍透明

            // 随机整体旋转，让每次法阵都有细微差异
            float baseRot = Main.rand.NextFloat(MathHelper.TwoPi);
            float baseRot2 = Main.rand.NextFloat(MathHelper.TwoPi);

            // ================================
            // ① 外层齿轮冠（大半径 + 锯齿感）
            // ================================
            int gearSeg = 72;
            float gearR = 120f;        // 大盘半径（约原中盘 80%）
            float tooth = 10f;         // 齿长

            for (int i = 0; i < gearSeg; i++)
            {
                float ang = MathHelper.TwoPi * i / gearSeg + baseRot;
                bool isTooth = (i % 2 == 0);
                float r = gearR + (isTooth ? tooth : 0f);

                Vector2 dir = ang.ToRotationVector2();
                Vector2 pos = center + dir * r;

                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame, Vector2.Zero, 0, dustBright, isTooth ? 1.3f : 1.0f);
                d.noGravity = true;
            }

            // ================================
            // ② 中层断续环（断点式时间刻度）
            // ================================
            int dashSeg = 64;
            float dashR = 95f;

            for (int i = 0; i < dashSeg; i++)
            {
                // 每 4 段留一段空缺，形成断续环
                if (i % 4 == 0)
                    continue;

                float ang = MathHelper.TwoPi * i / dashSeg + baseRot * 0.7f;
                Vector2 dir = ang.ToRotationVector2();
                Vector2 pos = center + dir * dashR;

                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, Vector2.Zero, 0, dustMain, 1.0f);
                d.noGravity = true;
            }

            // ================================
            // ③ 内层符文环（小一圈的稳定结构）
            // ================================
            int innerSeg = 40;
            float innerR = 70f;

            for (int i = 0; i < innerSeg; i++)
            {
                float ang = MathHelper.TwoPi * i / innerSeg + baseRot2;
                Vector2 dir = ang.ToRotationVector2();
                Vector2 pos = center + dir * innerR;

                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, Vector2.Zero, 0, dustMain, 0.9f);
                d.noGravity = true;
            }

            // ================================
            // ④ 12 个时间节点（取代传统“线型刻度”）
            // ================================
            int nodeCount = 12;
            float nodeR = 100f;

            for (int i = 0; i < nodeCount; i++)
            {
                float ang = MathHelper.TwoPi * i / nodeCount + baseRot2 * 1.1f;
                Vector2 dir = ang.ToRotationVector2();

                // 每个节点做一个小“花瓣”团，而不是一条线
                for (int j = 0; j < 6; j++)
                {
                    float offsetR = nodeR + Main.rand.NextFloat(-4f, 4f);
                    float smallAng = ang + Main.rand.NextFloat(-0.12f, 0.12f);

                    Vector2 dir2 = smallAng.ToRotationVector2();
                    Vector2 pos = center + dir2 * offsetR;

                    Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame, Vector2.Zero, 0, dustBright, 0.95f);
                    d.noGravity = true;
                }

                // 节点中心再放一点方块碎片，让时间感“破碎”
                SquareParticle sq = new SquareParticle(
                    center + dir * (nodeR + 2f),
                    dir * Main.rand.NextFloat(4.0f, 7.5f),
                    false,
                    32,
                    2.0f + Main.rand.NextFloat(0.8f),
                    squareColor
                );
                GeneralParticleHandler.SpawnParticle(sq);
            }

            // ================================
            // ⑤ 三重同心重型烟雾（土黄，高速外扩，作为时间风暴）
            // ================================
            for (int ring = 0; ring < 3; ring++)
            {
                float radius = 40f + ring * 20f;        // 40 / 60 / 80
                int seg = 20 + ring * 6;                // 20 / 26 / 32

                for (int i = 0; i < seg; i++)
                {
                    float ang = MathHelper.TwoPi * i / seg + baseRot * 0.4f;
                    Vector2 dir = ang.ToRotationVector2();

                    HeavySmokeParticle smoke = new HeavySmokeParticle(
                        center + dir * radius,
                        dir * Main.rand.NextFloat(15.0f, 35.0f),   // 速度极快（按你指定的）
                        smokeColor,
                        34,
                        0.9f + Main.rand.NextFloat(0.4f),
                        0.9f,
                        MathHelper.ToRadians(Main.rand.NextFloat(-5f, 5f)),
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
            }

            // ================================
            // ⑥ 中央时间脉冲圈（收束用的核心波纹）
            // ================================
            int pulseSeg = 40;
            float pulseR = 48f;
            for (int i = 0; i < pulseSeg; i++)
            {
                float ang = MathHelper.TwoPi * i / pulseSeg + baseRot * 1.3f;
                Vector2 dir = ang.ToRotationVector2();
                Vector2 pos = center + dir * pulseR;

                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame, dir * 4.2f, 0, dustBright, 1.05f);
                d.noGravity = true;
            }
        }





        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
           
        }





    }
}