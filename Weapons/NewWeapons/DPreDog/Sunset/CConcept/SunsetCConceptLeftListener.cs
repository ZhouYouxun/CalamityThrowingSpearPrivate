using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System.Collections.Generic;
using CalamityMod;
using Terraria.Audio;
using CalamityMod.Particles;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptLeftListener : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        //public override string Texture => "CalamityMod/Projectiles/InvisibleProj"; // 他需要有一个贴图贴图

        private List<int> subordinateProjectiles = new List<int>(); // 存储子弹幕的 ID
        public int Time;
        private const int SpinTime = 160; // 旋转阶段持续时间
        private const float SpinSpeed = 8f; // 旋转速度
        private bool HasTransitioned = false; // 标志是否已经进入固定状态

        public Player Owner => Main.player[Projectile.owner];
        public float SpinCompletion => Utils.GetLerpValue(0f, SpinTime, Time, true);
        public ref float InitialDirection => ref Projectile.ai[0];
        public ref float SpinDirection => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.MaxUpdates = 2;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            base.OnSpawn(source);
            SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);
        }
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);

        }

        private List<(SquishyLightParticle particle, float angle, float radius, float radialSpeed, float angularSpeed)>
    spinSquishyRing = new();




        public override void AI()
        {
            // **确保初始方向被正确记录**
            if (Time == 0)
            {
                InitialDirection = Owner.DirectionTo(Main.MouseWorld).ToRotation();
                SpinDirection = Main.rand.NextBool().ToDirectionInt();
                Projectile.netUpdate = true;
            }

            // **旋转阶段**
            if (Time < SpinTime)
            {
                // **计算旋转角度**
                Projectile.rotation = (float)Math.Pow(SpinCompletion, 0.82) * MathHelper.Pi * SpinDirection * 12f
                                      + InitialDirection + MathHelper.PiOver4;

                // 使投射物与玩家保持一致并瞄准鼠标位置
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
                }

                // 对齐到玩家中心
                Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
                Owner.heldProj = Projectile.whoAmI;
            }
            else if (!HasTransitioned)
            {
                // **立正阶段，进入渐进式移动**
                HasTransitioned = true;
            }

            // **如果已经进入立正阶段，则逐渐向前移动**
            if (HasTransitioned)
            {
                Vector2 targetPosition = Owner.Center + InitialDirection.ToRotationVector2() * (80f + 15f * 16f);
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPosition, 0.1f); // **平滑移动**
                Projectile.rotation = InitialDirection + MathHelper.PiOver4;
                SpawnTargetProjectiles();
            }

            // **固定阶段开始生成方形粒子**
            if (HasTransitioned)
            {
                GenerateParticles();
            }

            // **确保玩家手臂始终与长枪保持水平**
            ManipulatePlayerArmPositions();




            // 旋转期间的特效-----------------------------------------------------
            {
                // ========================= AI() 内：替换“旋转阶段”整块 =========================
                {
                    if (SpinCompletion >= 0f && SpinCompletion < 1f)
                    {
                        Vector2 center = Projectile.Center;
                        float baseRadius = 3f * 16f;         // 初始圆环半径
                        int ringCount = 8;                    // 有序分布数量（适度克制）
                        float phase = Main.GlobalTimeWrappedHourly * 2.8f; // 旋转相位，优雅而不急躁

                        // 银灰色主色系
                        Color[] techColors =
                        {
                            Color.Silver,
                            Color.LightGray,
                            new Color(180, 190, 200),
                            Color.WhiteSmoke
                        };
                        Color ringColor = techColors[Main.rand.Next(techColors.Length)];

 
                        // ---------- 1) 环形线性粒子：改为 SquishyLightParticle ----------
                        if ((Time % 3) == 0) // 节流：每 3 帧打一圈
                        {
                            for (int i = 0; i < ringCount; i++)
                            {
                                float angle = MathHelper.TwoPi * i / ringCount + phase; // 有序分布 + 相位
                                float radius = baseRadius;
                                float radialSpeed = Main.rand.NextFloat(6f, 10f);        // 初速度（向外）
                                float angularSpeed = 0.04f * SpinDirection;              // 轻微随武器旋向的涡旋

                                Vector2 spawnPos = center + angle.ToRotationVector2() * radius;

                                // 生成 SquishyLightParticle（EXO之光）
                                SquishyLightParticle exoEnergy = new SquishyLightParticle(
                                    spawnPos,
                                    Vector2.Zero,
                                    0.48f,
                                    Color.Orange,
                                    35,
                                    opacity: 1f,
                                    squishStrenght: 1f,
                                    maxSquish: 3f,
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(exoEnergy);

                                // ✅ 存进 spinSquishyRing
                                spinSquishyRing.Add((exoEnergy, angle, radius, radialSpeed, angularSpeed));

                            }
                        }


                        // ---------- 2) 十字星：GenericSparkle ----------
                        // 稍低频：每 4 帧射 2 颗，先外射后渐回追武器（视觉“呼吸”）
                        if ((Time % 4) == 0)
                        {
                            int starCount = 2;
                            for (int s = 0; s < starCount; s++)
                            {
                                float starAngle = phase + s * MathHelper.Pi;                // 对称两向
                                Vector2 dir = starAngle.ToRotationVector2();

                                // 生成十字星（GenericSparkle）
                                GenericSparkle sparker = new GenericSparkle(
                                    center,                             // 初始在武器中心
                                    Vector2.Zero,                       // 构造函数里写 0，实际速度我们自己接管
                                    Color.Gold,                         // 主颜色
                                    Color.Cyan,                         // 光晕
                                    Main.rand.NextFloat(1.8f, 2.5f),    // 缩放
                                    20,                                 // 寿命（短暂，像星爆）
                                    Main.rand.NextFloat(-0.01f, 0.01f), // 自转
                                    1.68f                               // 光晕扩散
                                );
                                GeneralParticleHandler.SpawnParticle(sparker);

                                // 🎯 给它一个随机方向的高速初速度
                                Vector2 randomDir = Main.rand.NextVector2Unit(); // 随机单位向量
                                Vector2 initVel = randomDir * Main.rand.NextFloat(8f, 14f); // 高速向外扩散
                                float homingAccel = 0f; // 不回追

                                // 保存到你的列表，后续在 AI 里更新
                                spinSparkles.Add((sparker, initVel, homingAccel));
                            }
                        }

                        // ---------- 3) 星座环光圈：ConstellationRingVFX ----------
                        // 重、低频：每 10 帧一次，360° 随机方向
                        if (Time % 1 == 0)
                        {
                            ConstellationRingVFX constellationRing = new ConstellationRingVFX(
                                center,
                                Color.GreenYellow * 0.8f,
                                Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
                                1.7f,
                                new Vector2(1f, 1f),
                                0.9f,
                                5,
                                1.5f,
                                0.06f,
                                false
                            );
                            GeneralParticleHandler.SpawnParticle(constellationRing);
                        }
                    }

                    // ================= 后续修改（直接写在这里） =================
                    // A) 更新环形 SparkParticle：锚定武器中心 + 径向速度每帧×0.9 + 轻微涡旋
                    for (int i = spinSparkRing.Count - 1; i >= 0; i--)
                    {
                        var t = spinSparkRing[i];
                        SparkParticle p = t.particle;

                        if (p.Time >= p.Lifetime)
                        {
                            spinSparkRing.RemoveAt(i);
                            continue;
                        }

                        t.radialSpeed *= 0.99f;
                        t.angle += t.angularSpeed;
                        t.radius += t.radialSpeed;

                        Vector2 newPos = Projectile.Center + t.angle.ToRotationVector2() * t.radius;

                        // 更新 Rotation，让粒子朝向速度方向
                        Vector2 vel = newPos - p.Position;
                        if (vel.LengthSquared() > 0.01f)
                            p.Rotation = vel.ToRotation();

                        p.Position = newPos;
                        spinSparkRing[i] = t;
                    }


                    // B) 更新十字星 GenericSparkle：统一右拐 + 衰减
                    for (int i = spinSparkles.Count - 1; i >= 0; i--)
                    {
                        var t = spinSparkles[i];
                        GenericSparkle p = t.particle;

                        if (p.Time >= p.Lifetime)
                        {
                            spinSparkles.RemoveAt(i);
                            continue;
                        }

                        // 每帧固定右拐 1°
                        t.velocity = t.velocity.RotatedBy(MathHelper.ToRadians(1f));

                        // 每帧速度衰减
                        t.velocity *= 0.95f;

                        // 更新位置
                        p.Position += t.velocity;

                        // 可选：旋转与方向保持一致（而不是固定加成）
                        if (t.velocity.LengthSquared() > 0.01f)
                            p.Rotation = t.velocity.ToRotation();

                        // 写回
                        spinSparkles[i] = t;
                    }



                }

            }

            // **如果玩家松开武器，则销毁弹幕**
            if (!Owner.channel)
            {
                Projectile.netUpdate = true;
                Projectile.Kill();
                // **销毁 `Magic`（魔法阵）**
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.type == ModContent.ProjectileType<SunsetCConceptLeftMagic>() && proj.owner == Projectile.owner)
                    {
                        proj.Kill();
                    }
                }
            }
            Projectile.timeLeft = 180;
            Time++;
        }
        // ========================= 类字段区域新增 =========================
        // ① 旋转期：环形 SparkParticle（半径-角度绑定到武器中心，带径向减速）
        private List<(SparkParticle particle, float angle, float radius, float radialSpeed, float angularSpeed)>
            spinSparkRing = new List<(SparkParticle, float, float, float, float)>();

        // ② 旋转期：十字星 GenericSparkle（向外射出→减速→缓慢回追武器）
        private List<(GenericSparkle particle, Vector2 velocity, float homingAccel)>
            spinSparkles = new List<(GenericSparkle, Vector2, float)>();














        // ================== 插入阶段·新增字段（类字段区） ==================
        // A. 向日葵分布（黄金角）喷发的线性粒子：SparkParticle
        private List<(SparkParticle p, float angle, float speed, float drift)> insertSunflower = new();

        // B. 玫瑰线脉冲的 Squishy 光束
        private List<(SquishyLightParticle p, Vector2 vel, float turn, float damp)> insertSquish = new();

        // C. Lissajous 轨道的 GlowOrb
        private List<(GlowOrbParticle p, float t0, float Ax, float Ay, float w1, float w2)> insertOrbs = new();

        // 相位与速率（可微调）
        private float insertPhase;       // 插入期主相位（推进“节奏”）
        private int insertTick;        // 插入期帧计数（用于节流）


        // ================== 重写：概念形态·几何扩散特效 ==================
        private void GenerateParticles()
        {
            insertTick++;
            insertPhase += 0.035f; // 插入期全局相位（越小越“优雅”）

            // 枪头位置（插入期的“发源”）
            Vector2 headDir = (InitialDirection).ToRotationVector2();
            Vector2 headPos = Projectile.Center + headDir * 16f * 3.5f;

            // 银白基色（点少而克制）
            Color[] techMetal =
            {
        Color.Silver,
        new Color(200, 208, 216),
        new Color(150, 164, 180),
        new Color(176, 196, 222)
    };
            Color coreWhite = Color.WhiteSmoke;



            //// =============== A. 向日葵分布（黄金角） ===============
            //// 思路：每 2 帧喷出 6 个点，角度按黄金角累进，半径用速度推进；每粒子有极小角漂移形成“微乱”。
            //if (insertTick % 2 == 0)
            //{
            //    const int sunflowerCount = 6;
            //    float goldenAngle = MathHelper.ToRadians(137.50776f);
            //    // 小技巧：φ ≈ 137.5°；(1+√5)/2 的倒数变体，避免硬编码
            //    // 也可直接用：float goldenAngle = MathHelper.ToRadians(137.50776f);

            //    for (int i = 0; i < sunflowerCount; i++)
            //    {
            //        // 基角：黄金角推进 + 全局相位（让整体缓慢旋转）
            //        float baseAngle = (i * goldenAngle) + insertPhase * 0.6f;
            //        // 初速度（外喷 + 继承少量武器速度，避免玩家动得快时“脱节”）
            //        float speed = Main.rand.NextFloat(8f, 12f);
            //        Vector2 initVel = baseAngle.ToRotationVector2() * speed + Projectile.velocity * 0.25f;

            //        SparkParticle sp = new SparkParticle(
            //            headPos,
            //            Vector2.Zero,   // 我们接管运动
            //            false,
            //            32,             // 寿命短一点更干净
            //            1.05f,
            //            techMetal[Main.rand.Next(techMetal.Length)]
            //        );
            //        GeneralParticleHandler.SpawnParticle(sp);

            //        // 极小角漂移（“无序”的来源之一）
            //        float drift = Main.rand.NextFloat(-0.007f, 0.007f);
            //        insertSunflower.Add((sp, baseAngle, speed, drift));
            //    }
            //}

            //// =============== B. 玫瑰线脉冲（6瓣） ===============
            //// 思路：6根 Squishy 光束，方向按 θ_k = θ0 + k*2π/6；速度/拉伸随 cos(3θ0) 微起伏，轻微右拐+阻尼。
            //if (insertTick % 4 == 0)
            //{
            //    int petals = 6;
            //    float theta0 = insertPhase;             // 主相位
            //    for (int k = 0; k < petals; k++)
            //    {
            //        float theta = theta0 + MathHelper.TwoPi * k / petals;
            //        float pulse = 1f + 0.22f * (float)Math.Cos(3f * theta0 + k * 0.7f); // 玫瑰线 3 倍频造成的“张合”
            //        Vector2 v = theta.ToRotationVector2() * (6.5f * pulse) + Projectile.velocity * 0.15f;

            //        SquishyLightParticle exo = new SquishyLightParticle(
            //            headPos,
            //            Vector2.Zero,     // 接管运动
            //            0.42f * pulse,    // 缩放随脉冲
            //            coreWhite,        // 中核偏白，洁净
            //            26,               // 稍短
            //            opacity: 1f,
            //            squishStrenght: 1.15f + 0.15f * pulse,
            //            maxSquish: 3.4f
            //        );
            //        GeneralParticleHandler.SpawnParticle(exo);

            //        // turn：每帧右拐角速度；damp：速度衰减
            //        insertSquish.Add((exo, v, MathHelper.ToRadians(0.8f), 0.955f));
            //    }
            //}

            //// =============== C. Lissajous 轨道（2:3） ===============
            //// 思路：3 个 GlowOrb 在枪尖周围做 Lissajous，半径做“呼吸”，作为背景秩序支撑。
            //if (insertOrbs.Count < 3 && insertTick % 10 == 0)
            //{
            //    float Ax = 28f, Ay = 20f;                     // 初始轨道半径
            //    float w1 = 2.0f, w2 = 3.0f;                   // 频比 2:3
            //    GlowOrbParticle orb = new GlowOrbParticle(
            //        headPos,
            //        Vector2.Zero,
            //        false,
            //        40,
            //        1.15f,
            //        techMetal[Main.rand.Next(techMetal.Length)],
            //        true, false, true
            //    );
            //    GeneralParticleHandler.SpawnParticle(orb);
            //    insertOrbs.Add((orb, Main.GlobalTimeWrappedHourly, Ax, Ay, w1, w2));
            //}

            //// ================== 统一更新：A/B/C 三类 ==================

            //// A) 向日葵线性粒子：速度衰减 + 微角漂移 + 方向重算 + 朝向对齐
            //for (int i = insertSunflower.Count - 1; i >= 0; i--)
            //{
            //    var t = insertSunflower[i];
            //    SparkParticle p = t.p;
            //    if (p.Time >= p.Lifetime)
            //    {
            //        insertSunflower.RemoveAt(i);
            //        continue;
            //    }

            //    // 衰减 + 轻微角漂移
            //    t.speed *= 0.93f;
            //    t.angle += t.drift;

            //    // 重新计算速度（外扩 + 少量继承武器速度，避免“掉帧感”）
            //    Vector2 newVel = t.angle.ToRotationVector2() * t.speed + Projectile.velocity * 0.12f;

            //    // ✅ 把速度交给粒子系统（内部会据此更新位置/朝向）
            //    p.Velocity = newVel;

            //    // （可选）立刻同步朝向；若贴图需要，可加或减 PiOver2 做修正
            //    if (newVel.LengthSquared() > 0.0001f)
            //        p.Rotation = newVel.ToRotation(); // + MathHelper.PiOver2;

            //    // ❌ 不要再手动位移，否则会“走两次”
            //    // p.Position += newVel;

            //    insertSunflower[i] = t;
            //}


            //// B) 玫瑰线光束：每帧右拐 + 阻尼 + 速度决定拉伸感
            //for (int i = insertSquish.Count - 1; i >= 0; i--)
            //{
            //    var t = insertSquish[i];
            //    SquishyLightParticle p = t.p;
            //    if (p.Time >= p.Lifetime)
            //    {
            //        insertSquish.RemoveAt(i);
            //        continue;
            //    }

            //    // 右拐 & 衰减
            //    t.vel = t.vel.RotatedBy(t.turn);
            //    t.vel *= t.damp;

            //    p.Position += t.vel;

            //    // 根据速度大小微调缩放 / 拉伸（速度越大越“尖锐”）
            //    float vmag = t.vel.Length();
            //    p.Scale = MathHelper.Lerp(p.Scale, 0.36f + 0.06f * vmag, 0.35f);
            //    // （Squishy 内部已有拉伸表现，这里轻调即可）

            //    insertSquish[i] = t;
            //}

            //// C) Lissajous 轨道：以当前枪头为中心做 2:3 轨迹，半径“呼吸式”起伏
            //for (int i = insertOrbs.Count - 1; i >= 0; i--)
            //{
            //    var t = insertOrbs[i];
            //    GlowOrbParticle p = t.p;
            //    if (p.Time >= p.Lifetime)
            //    {
            //        insertOrbs.RemoveAt(i);
            //        continue;
            //    }

            //    float tt = (Main.GlobalTimeWrappedHourly - t.t0);
            //    // 呼吸：半径缓慢涨落（0.85~1.15）
            //    float breath = 1f + 0.15f * (float)Math.Sin(tt * 1.2f);
            //    float Ax = t.Ax * breath;
            //    float Ay = t.Ay * breath;

            //    // Lissajous 参数方程
            //    float x = Ax * (float)Math.Sin(t.w1 * tt + 0.3f);
            //    float y = Ay * (float)Math.Cos(t.w2 * tt);

            //    // 轨道中心跟随当前枪头
            //    p.Position = headPos + new Vector2(x, y);
            //}

            //// （可选极简点睛：每 18 帧来一个轻微脉冲环，避免画面“死寂”）
            //if (insertTick % 18 == 0)
            //{
            //    int pulseCount = 10;
            //    float r0 = 18f;
            //    for (int i = 0; i < pulseCount; i++)
            //    {
            //        float ang = MathHelper.TwoPi * i / pulseCount + insertPhase;
            //        Vector2 v = ang.ToRotationVector2() * 4.2f;
            //        var pulse = new SparkParticle(
            //            headPos + ang.ToRotationVector2() * r0,
            //            v,
            //            false,
            //            24,
            //            0.95f,
            //            Color.WhiteSmoke
            //        );
            //        GeneralParticleHandler.SpawnParticle(pulse);
            //    }
            //}
        }







        private void SpawnTargetProjectiles()
        {
            NPC target = FindClosestTarget();
            if (target == null) return;

            // **检查是否已经存在 `SunsetCConceptLeftMagic`**
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<SunsetCConceptLeftMagic>() && proj.owner == Projectile.owner)
                {
                    return; // **如果已经存在 `Magic`，则不生成新的**
                }
            }

            // **生成 `SunsetCConceptLeftMagic` 造成伤害**
            int damageProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                target.Center, Vector2.Zero,
                ModContent.ProjectileType<SunsetCConceptLeftMagic>(),
                Projectile.damage, Projectile.knockBack, Projectile.owner,
                target.whoAmI);

            subordinateProjectiles.Add(damageProj);


            {
                // **生成枪口魔法阵（SunsetCConceptLeftMagic2）**
                Vector2 gunHeadPosition = Owner.Center + InitialDirection.ToRotationVector2() * 80f;

                // 检查是否已经存在
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.type == ModContent.ProjectileType<SunsetCConceptLeftMagic2>() && proj.owner == Projectile.owner)
                    {
                        return; // 已存在就不生成
                    }
                }

                int vizProj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    gunHeadPosition,
                    Vector2.Zero,
                    ModContent.ProjectileType<SunsetCConceptLeftMagic2>(),
                    0, 0f, Projectile.owner,
                    Projectile.whoAmI // 把本体的 ID 传给 ai[0]
                );

                subordinateProjectiles.Add(vizProj);
            }



        }

        private NPC FindClosestTarget()
        {
            NPC closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (npc.boss && distance < closestDistance)
                    {
                        closestTarget = npc;
                        closestDistance = distance;
                    }
                    else if (!closestTarget?.boss ?? true && distance < closestDistance)
                    {
                        closestTarget = npc;
                        closestDistance = distance;
                    }
                }
            }
            return closestTarget;
        }

        public void ManipulatePlayerArmPositions()
        {
            Vector2 gunHeadPosition = Owner.Center + InitialDirection.ToRotationVector2() * 80f;

            // **让玩家手臂方向始终指向枪头**
            float armRotation = (gunHeadPosition - Owner.Center).ToRotation();

            Owner.ChangeDir((Math.Cos(armRotation) > 0f).ToDirectionInt());
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = Owner.itemAnimation = 2;
            Owner.itemRotation = CalamityUtils.WrapAngle90Degrees(armRotation - MathHelper.PiOver2);

            Projectile.Center = Owner.Center; // 监听弹幕仍然依附于玩家
            if (Owner.CantUseHoldout())
                Projectile.Kill();
        }




        // ================== 可调参数（随时改） ==================
        public static int VizSegments = 64;             // 分段数（建议 64）
        public static float VizBaseRadius = 4f * 16f;   // 基础半径（圆环最内径）
        public static float VizMaxRadius = 8f * 16f;   // 最大外径（不会超过此值）
        public static float VizRotSpeed = 2.6f;       // 整体旋转速度（弧度/秒的量纲）
        public static float VizMainWidth = 2.6f;       // 主环线条基准宽
        public static float VizBarWidth = 1.6f;       // 径向柱（条形）的线宽
        public static float VizSubWidth = 1.2f;       // 副环线条宽
        public static float VizLerp = 0.28f;      // 振幅平滑插值强度（0~1）
        public static float VizNoiseMix = 0.65f;      // 噪声组合占比（越大越“复杂”）
        public static float VizPulse = 1.0f;       // 全局脉冲强度（整体更“躁动”）
        public static float VizOpacity = 0.90f;      // 线条整体不透明度

        // 颜色：银白系（可自行替换为你更喜欢的银灰值）
        public static Color VizColA = Color.WhiteSmoke;
        public static Color VizColB = Color.Silver;

        // ================== 运行时状态（每段各自的“跳动”幅度） ==================
        private float[] vizAmp;      // 当前振幅
        private float[] vizTarget;   // 目标振幅
        private float[] vizPhase;    // 每段的相位偏移（让每段“跳”的不一样）

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制刀盘特效
            if (SpinCompletion >= 0f && SpinCompletion < 1f)
            {
                Texture2D smear = ModContent.Request<Texture2D>("CalamityMod/Particles/SemiCircularSmear").Value;

                float rotation = Projectile.rotation - MathHelper.Pi / 5f;
                if (SpinDirection == -1f)
                    rotation += MathHelper.Pi;

                Color smearColor = Color.GhostWhite * CalamityUtils.Convert01To010(SpinCompletion) * 0.9f;
                Vector2 smearOrigin = smear.Size() * 0.5f;

                Main.EntitySpriteDraw(smear, Owner.Center - Main.screenPosition, null, smearColor with { A = 0 }, rotation, smearOrigin, Projectile.scale * 1.45f, 0, 0);
            }

            // 绘制线条法阵特效
            if (SpinCompletion >= 0f && SpinCompletion < 1f)
            {
                // === 银白“音乐可视化”跳动圆环 ===
                Player owner = Main.player[Projectile.owner];
                Vector2 center = owner.Center - Main.screenPosition;

                // 确保数组初始化
                if (vizAmp == null || vizAmp.Length != VizSegments)
                {
                    vizAmp = new float[VizSegments];
                    vizTarget = new float[VizSegments];
                    vizPhase = new float[VizSegments];
                    for (int i = 0; i < VizSegments; i++)
                        vizPhase[i] = Main.rand.NextFloat(MathHelper.TwoPi); // 每段独立相位
                }

                // 时间基
                float t = Main.GlobalTimeWrappedHourly;      // 连续时间
                float rot = t * VizRotSpeed;                 // 整体旋转（让圆环“转起来”）

                // 振幅上限（外径-内径）
                float maxAmp = Math.Max(0f, VizMaxRadius - VizBaseRadius);

                // 预先计算第一个点，方便连线
                Vector2 prevPos = Vector2.Zero;
                float prevAngle = 0f;
                float prevRadius = 0f;

                // 三层叠加：主环（连接线）+ 径向柱 + 副环（错相位连接）
                for (int layer = 0; layer < 3; layer++)
                {
                    bool isMain = layer == 0;
                    bool isBars = layer == 1;
                    bool isSub = layer == 2;

                    float width = isMain ? VizMainWidth : (isBars ? VizBarWidth : VizSubWidth);
                    float phaseShift = isSub ? (MathHelper.Pi / VizSegments) : 0f; // 副环半段错相

                    // 每段更新振幅（目标更“躁动”，当前做 Lerp 平滑）
                    for (int i = 0; i < VizSegments; i++)
                    {
                        // 复合噪声：两路不同频率/相位的 sin 叠加，控制到 0~1
                        float n1 = (float)Math.Sin((t * 9.5f + vizPhase[i]) + i * 0.11f);
                        float n2 = (float)Math.Sin((t * 16.3f + vizPhase[i] * 1.31f) + i * 0.07f);
                        float noise01 = MathHelper.Clamp(0.5f + 0.5f * (VizNoiseMix * n1 + (1f - VizNoiseMix) * n2), 0f, 1f);

                        // 目标振幅：越外越接近上限，但不超过 maxAmp
                        float target = maxAmp * noise01 * VizPulse;

                        // 每段偶尔“爆点”提升（更像音乐击打）
                        if ((Main.GameUpdateCount + i) % 17 == 0)
                            target *= 1.35f;

                        vizTarget[i] = MathHelper.Clamp(target, 0f, maxAmp);

                        // 平滑靠拢，保证“跳动但不撕裂”
                        vizAmp[i] = MathHelper.Lerp(vizAmp[i], vizTarget[i], VizLerp);
                    }

                    // 绘制
                    for (int i = 0; i < VizSegments; i++)
                    {
                        float angle = (MathHelper.TwoPi * i / VizSegments) + rot + phaseShift;

                        // 当前段的半径（内径 + 振幅），严格限制不超外径
                        float radius = MathHelper.Clamp(VizBaseRadius + vizAmp[i], VizBaseRadius, VizMaxRadius);

                        Vector2 dir = angle.ToRotationVector2();
                        Vector2 curPos = center + dir * radius;

                        // 颜色：银白在段落上做周期变化 + 随时间律动
                        float hue = (float)Math.Sin(angle * 3f + t * 6f) * 0.5f + 0.5f; // 角度周期
                        Color lineColor = Color.Lerp(VizColA, VizColB, hue) * VizOpacity;
                        lineColor.A = 0;

                        if (isBars)
                        {
                            // 径向“柱条”——从内径打到当前半径，像音频棒状可视化
                            Vector2 inner = center + dir * VizBaseRadius;
                            Main.spriteBatch.DrawLineBetter(inner, curPos, lineColor, width);

                            // 顶端再加一条细亮“边线”，增强尖锐科技感
                            Vector2 tip = center + dir * Math.Min(radius + 2f, VizMaxRadius);
                            Main.spriteBatch.DrawLineBetter(curPos, tip, Color.White * 0.6f, Math.Max(1f, width * 0.6f));
                        }
                        else
                        {
                            // 主环 / 副环：连接相邻两点，形成“起伏的波形圆”
                            if (i > 0)
                                Main.spriteBatch.DrawLineBetter(prevPos, curPos, lineColor, width);
                        }

                        prevPos = curPos;
                        prevAngle = angle;
                        prevRadius = radius;
                    }

                    // 闭合环（主环/副环）
                    if (!isBars)
                    {
                        float firstAngle = rot + phaseShift;
                        float firstRadius = MathHelper.Clamp(VizBaseRadius + vizAmp[0], VizBaseRadius, VizMaxRadius);
                        Vector2 firstPos = center + firstAngle.ToRotationVector2() * firstRadius;
                        Main.spriteBatch.DrawLineBetter(prevPos, firstPos, Color.Lerp(VizColA, VizColB, 0.5f) * VizOpacity, width);
                    }
                }

            }


            // **计算监听弹幕的绘制方向**
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            // **修正旋转方向**
            float adjustedRotation = Projectile.rotation;

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), adjustedRotation, origin, Projectile.scale, 0, 0);

            return false;
        }
    }
}
