using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Terraria.ID;
using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00.Laser;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00
{
    internal class TEM00Left : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/TEM00/TEM00";
        public override void SetStaticDefaults()
        {
            // 设置弹幕拖尾长度和模式
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制控制函数，可用于绘制自定义贴图、添加发光效果、叠加特效等
            // 若不需要可返回 true 使用默认绘制【很不推荐】
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1; // 调高这个值可以让弹幕更加顺滑的跟随鼠标
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public enum BehaviorState
        {
            Aim,
            Dash
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public Player Owner => Main.player[Projectile.owner];

        public override void AI()
        {
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
                case BehaviorState.Dash:
                    DoBehavior_Dash();
                    break;
            }
        }
        private int chargeTimer = 0; // 在类里新建字段
        private int chargeCount = 0; // 已经触发几次（最多 8）


                                     // ===== 攻击控制 =====
        private int attackPhase = 0;    // 当前第几轮攻击 (0~4, 共5轮)
        private int shotsThisPhase = 0; // 本轮需要发射多少发
        private int shotsFired = 0;     // 本轮已发射多少发
        private int fireCooldown = 0;   // 单发冷却
        private int phaseCooldown = 0;  // 轮与轮之间的间隔
        private bool specialAttack = false; // 是否进入特殊攻击阶段
        
        // 记录最后生成的超级激光弹幕ID；-1 表示未生成/已失效
        private int superLaserId = -1;
        // 扇形喷发的节流计时（例如每帧都喷，或每2帧喷一次）
        private int backBurstTicker = 0;

        private void DoBehavior_Aim() // 瞄准阶段
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width);
            Owner.heldProj = Projectile.whoAmI;

            // 枪头位置 [这很重要，因为许多特效都需要和他相关]
            Vector2 headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;

            // 如果武器自身会高速旋转 [比如巨龙之怒] ，那么枪头需要改成这个来适配
            float fixedRotation = Projectile.rotation; // 可根据需求加减角度偏移
            headPosition = Projectile.Center + new Vector2(16f * 3f, 0f).RotatedBy(fixedRotation);



            {
                // ====== 攻击流程逻辑 ======
                if (!specialAttack)
                {
                    if (phaseCooldown > 0)
                    {
                        phaseCooldown--;
                    }
                    else
                    {
                        if (shotsThisPhase == 0)
                        {
                            // 初始化本轮射击
                            attackPhase++;
                            if (attackPhase <= 5)
                            {
                                shotsThisPhase = Main.rand.Next(10, 16); // 10~15发
                                shotsFired = 0;
                            }
                            else
                            {
                                // 五轮打完 → 进入特殊攻击阶段（暂时留空）
                                // ===== 当五轮结束，进入特殊攻击（只在此刻生成一次超级激光） =====
                                specialAttack = true;

                                // 生成超级激光（只生成一次）
                                // 把生成位置放在枪口 headPosition，上方发射方向以当前朝向为准
                                int laserProj = Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    headPosition, // 激光出生点：弹幕顶端（你已经算好了 headPosition）
                                    Projectile.velocity.SafeNormalize(Vector2.UnitY), // 方向（单位向量）——激光类会根据 owner 同步方向
                                    ModContent.ProjectileType<TEM00LeftSuperLazer>(),
                                    (int)(Projectile.damage * 5), // 可以按需调整伤害
                                    0f,
                                    Projectile.owner
                                );

                                // 绑定父弹幕索引：把 ai[0] 设为当前父弹幕索引（this.whoAmI）
                                if (laserProj >= 0 && laserProj < Main.maxProjectiles)
                                {
                                    Main.projectile[laserProj].ai[0] = Projectile.whoAmI; // 告诉激光它的“父弹幕”是谁
                                    Main.projectile[laserProj].netUpdate = true; // 多人时同步
                                                                                 // 可选：立即把激光的朝向和速度与父弹幕匹配（便于首帧视觉一致）
                                    Main.projectile[laserProj].rotation = Projectile.rotation - MathHelper.PiOver4;
                                    Main.projectile[laserProj].velocity = (Main.projectile[laserProj].rotation).ToRotationVector2();
                                }

                                superLaserId = laserProj;

                                // 屏幕震动
                                float shakePower = 95f;
                                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                                Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                                    Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);


                                // === 超级魔法阵·一次性后扇形喷发（只在本地拥有者触发，避免多人重复）===
                                if (Main.myPlayer == Projectile.owner)
                                {
                                    // 枪口与方向（与你上方一致）
                                    Vector2 muzzle = headPosition;
                                    Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                                    Vector2 back = -forward;

                                    // 只在后方 180°：以 back 为轴，±90° 的扇形
                                    float coneHalf = MathHelper.PiOver2; // 90°

                                    // 调色板（科技蓝家族）
                                    Color[] techBlue =
                                    {
                                        new Color( 80, 200, 255),
                                        new Color(120, 220, 255),
                                        Color.Cyan,
                                        new Color(180, 220, 255),
                                        Color.WhiteSmoke
                                    };

                                    // ========== “1.5×宏伟度”的一次性强喷（数量、速度全面抬升） ==========
                                    // SquishyLight（EXO高亮喷焰）：粗壮主束感
                                    int exoCount = 32 + Main.rand.Next(10); // 32~41（持续喷发版的 ~1.5×）
                                                                            // Spark（线性火花）：锐利刀锋
                                    int sparkCount = 56 + Main.rand.Next(16); // 56~71
                                                                              // GlowOrb（柔性辉光）：对数螺旋点缀两臂，提供数学骨架
                                    int spiralArms = 2;
                                    int orbPerArm = 18 + Main.rand.Next(6);  // 18~23

                                    // 速度全面提升到“持续背喷”的 ≥1.5×
                                    // （持续背喷示例：EXO 14~26、Spark 18~34、Orb 10~18）
                                    Func<float, float, float> R = (a, b) => Main.rand.NextFloat(a, b);
                                    // 1) EXO：强亮扇形主喷
                                    for (int i = 0; i < exoCount; i++)
                                    {
                                        // 为了“半规则”的美感：分层+分段取角，既均匀又有随机
                                        float u = (i + Main.rand.NextFloat()) / exoCount;            // 0~1
                                        float ang = MathHelper.Lerp(-coneHalf, coneHalf, u);         // 均匀覆盖 180°
                                        Vector2 dir = back.RotatedBy(ang + Main.rand.NextFloat(-0.08f, 0.08f));

                                        var exo = new SquishyLightParticle(
                                            muzzle,
                                            dir * R(21f, 39f),               // ★ 比持续背喷快 ~1.5×
                                            R(0.30f, 0.46f),                 // 体积略大
                                            techBlue[Main.rand.Next(techBlue.Length)],
                                            Main.rand.Next(18, 28),          // 寿命中等
                                            opacity: 1f,
                                            squishStrenght: 1f,
                                            maxSquish: R(2.6f, 3.6f),
                                            hueShift: 0f
                                        );
                                        GeneralParticleHandler.SpawnParticle(exo);
                                    }

                                    // 2) Spark：刀锋形火花（极快、偏直线）
                                    for (int i = 0; i < sparkCount; i++)
                                    {
                                        float u = (i + 0.5f * (i % 2)) / sparkCount;                 // 轻微锯齿排列
                                        float ang = MathHelper.Lerp(-coneHalf, coneHalf, u);
                                        Vector2 baseDir = back.RotatedBy(ang);
                                        Vector2 jitter = baseDir.RotatedBy(Main.rand.NextFloat(-0.17f, 0.17f)); // 细小抖动

                                        var sp = new SparkParticle(
                                            muzzle,
                                            jitter * R(27f, 51f),            // ★ 速度更快
                                            false,
                                            Main.rand.Next(16, 26),
                                            R(0.9f, 1.5f),
                                            Color.Lerp(techBlue[Main.rand.Next(techBlue.Length)], Color.White, 0.35f)
                                        );
                                        GeneralParticleHandler.SpawnParticle(sp);
                                    }

                                    // 3) GlowOrb：两臂对数螺旋（数学美学骨架）
                                    // r = r0 * e^(k * t), theta 从 0 -> ±90°，两臂对称
                                    float r0 = 14f;
                                    float k = 0.035f; // 增长系数（更优雅，别太大）
                                    for (int arm = 0; arm < spiralArms; arm++)
                                    {
                                        float sign = (arm == 0) ? 1f : -1f;
                                        for (int j = 0; j < orbPerArm; j++)
                                        {
                                            float t = j / (float)(orbPerArm - 1);
                                            float theta = sign * MathHelper.Lerp(0f, coneHalf, t) + Main.rand.NextFloat(-0.05f, 0.05f);
                                            float r = r0 * (float)Math.Exp(k * t * 90f); // 半径缓慢外扩
                                            Vector2 dir = back.RotatedBy(theta);
                                            Vector2 pos = muzzle + dir * r;

                                            var orb = new GlowOrbParticle(
                                                pos,
                                                dir * R(15f, 27f),           // ★ 速度抬高到 ≥1.5×
                                                false,
                                                Main.rand.Next(10, 16),
                                                R(0.9f, 1.5f),
                                                techBlue[(arm + j) % techBlue.Length],
                                                true, false, true
                                            );
                                            GeneralParticleHandler.SpawnParticle(orb);
                                        }
                                    }

                                    // （可选）给一次性爆发一个更厚重的音色
                                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74, muzzle);
                                }



                            }
                        }

                        if (shotsThisPhase > 0)
                        {
                            if (fireCooldown > 0)
                                fireCooldown--;
                            else
                            {
                                // 发射一发激光
                                headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 48f;

                                // 基础方向：正前方
                                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                                // 在 ±10° (即 20°范围内) 随机偏移
                                dir = dir.RotatedByRandom(MathHelper.ToRadians(10f));

                                Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    headPosition,
                                    dir,
                                    ModContent.ProjectileType<TEM00LeftLazer>(),
                                    (int)(Projectile.damage * 0.5),
                                    0f,
                                    Projectile.owner
                                );


                                // 屏幕震动
                                float shakePower = 5f;
                                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                                Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                                    Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                                SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);

                                shotsFired++;
                                fireCooldown = 6; // 单发之间的间隔（你可以调大或调小）

                                if (shotsFired >= shotsThisPhase)
                                {
                                    // 本轮结束 → 设置间隔时间
                                    shotsThisPhase = 0;
                                    phaseCooldown = 30; // 两轮之间的间隔（可以调整）
                                }
                            }
                        }
                    }
                }
                else
                {
                    // === 超级激光持续期间，从枪口向左后/右后两个扇形喷发高速粒子 ===


                    //SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/SSL/拉链闪电") with { Volume = 0.95f, Pitch = -0.2f }, Projectile.Center);

                    // ① 确认超级激光仍然存活（存在且类型匹配），否则不喷发
                    bool laserAlive = superLaserId >= 0 && superLaserId < Main.maxProjectiles
                                      && Main.projectile[superLaserId].active
                                      && Main.projectile[superLaserId].type == ModContent.ProjectileType<TEM00LeftSuperLazer>();
                    if (!laserAlive)
                        return;

                    // ② 计算枪口位置（与你上方一致）
                    headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (16f * 3f);
                    //fixedRotation = Projectile.rotation;
                    //headPosition = Projectile.Center + new Vector2(16f * 3f, 0f).RotatedBy(fixedRotation);

                    // ③ 基方向：前=dir，后= -dir；左后/右后锥基向量（±45°）
                    Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                    Vector2 back = -dir;
                    Vector2 leftBackBase = back.RotatedBy(MathHelper.PiOver4);   // 后方 + 左 45°
                    Vector2 rightBackBase = back.RotatedBy(-MathHelper.PiOver4);   // 后方 + 右 45°
                    float coneHalf = MathHelper.ToRadians(24f); // 扇形半角（可调：20°~30°）

                    // ④ 颜色池（科技蓝家族）
                    Color[] techBlue =
                    {
        new Color( 80, 200, 255),
        new Color(120, 220, 255),
        Color.Cyan,
        new Color(180, 220, 255),
        Color.WhiteSmoke
    };

                    // ⑤ 节流（例如每帧都喷；若太炸可改为 %2==0）
                    backBurstTicker++;

                    // 单帧内：每个扇形各喷一组（“疯狂版”数量；若担心性能可把 *Count 降低）
                    void BurstCone(Vector2 baseDir)
                    {
                        // a) EXO（SquishyLightParticle）：强亮、速度极快
                        int exoCount = 4 + Main.rand.Next(3); // 4~6
                        for (int i = 0; i < exoCount; i++)
                        {
                            Vector2 v = baseDir.RotatedByRandom(coneHalf) * Main.rand.NextFloat(14f, 26f); // 非常快
                            var exo = new SquishyLightParticle(
                                headPosition,
                                v,
                                Main.rand.NextFloat(0.28f, 0.42f),
                                techBlue[Main.rand.Next(techBlue.Length)],
                                Main.rand.Next(18, 26),
                                opacity: 1f,
                                squishStrenght: 1f,
                                maxSquish: Main.rand.NextFloat(2.4f, 3.4f),
                                hueShift: 0f
                            );
                            GeneralParticleHandler.SpawnParticle(exo);
                        }

                        // b) SparkParticle：线性火花，刀锋感强
                        int sparkCount = 8 + Main.rand.Next(6); // 8~13
                        for (int i = 0; i < sparkCount; i++)
                        {
                            Vector2 v = baseDir.RotatedByRandom(coneHalf) * Main.rand.NextFloat(18f, 34f);
                            var sp = new SparkParticle(
                                headPosition,
                                v,
                                false,
                                Main.rand.Next(14, 22),
                                Main.rand.NextFloat(0.7f, 1.2f),
                                Color.Lerp(techBlue[Main.rand.Next(techBlue.Length)], Color.White, 0.35f)
                            );
                            GeneralParticleHandler.SpawnParticle(sp);
                        }

                        // c) GlowOrb：柔性辉光，补充层次（数量略少）
                        int orbCount = 4 + Main.rand.Next(3); // 4~6
                        for (int i = 0; i < orbCount; i++)
                        {
                            Vector2 v = baseDir.RotatedByRandom(coneHalf) * Main.rand.NextFloat(10f, 18f);
                            var orb = new GlowOrbParticle(
                                headPosition + v.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(6f, 18f),
                                v * Main.rand.NextFloat(0.85f, 1.15f),
                                false,
                                Main.rand.Next(8, 14),
                                Main.rand.NextFloat(0.9f, 1.5f),
                                techBlue[Main.rand.Next(techBlue.Length)],
                                true, false, true
                            );
                            GeneralParticleHandler.SpawnParticle(orb);
                        }
                    }

                    // 左后扇形 + 右后扇形
                    BurstCone(leftBackBase);
                    BurstCone(rightBackBase);
                }



            }


            // 松手后进入 Dash
            if (!Owner.channel)
            {
                Projectile.netUpdate = true;
                Projectile.timeLeft = 300;
                Projectile.penetrate = -1; // 可调穿透次数

                CurrentState = BehaviorState.Dash;
            }
        }






        private int dashFrameCounter = 0; // 在类里新建计数器字段

        private void DoBehavior_Dash() // 冲刺阶段
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            // 设置冲刺速度
            float initialSpeed = 35f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * initialSpeed;

            // 每帧计数
            dashFrameCounter++;

            if (dashFrameCounter % 3 == 0 && Main.myPlayer == Projectile.owner)
            {
                // ====== 1. 在自己正下方随机位置生成一发激光 ======
                float xOffset = Main.rand.NextFloat(-120f, 120f); // 左右随机
                float yOffset = Main.rand.NextFloat(35 * 16f, 35 * 16f);  // 在正下方一定范围
                Vector2 spawnPos = Projectile.Center + new Vector2(xOffset, yOffset);

                // 方向：指向本体  
                Vector2 dir = (Projectile.Center - spawnPos).SafeNormalize(Vector2.UnitY);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    dir,
                    ModContent.ProjectileType<TEM00LeftLazer>(),
                    (int)(Projectile.damage * 5),
                    0f,
                    Projectile.owner
                );

                SoundEngine.PlaySound(SoundID.Item33, spawnPos);

                // ====== 2. 疯狂的飞行特效 ======
                for (int i = 0; i < 6; i++) // 比平时多，显得更夸张
                {
                    // Square 方块能量片
                    SquareParticle sq = new SquareParticle(
                        spawnPos,
                        dir.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 6f),
                        false,
                        18 + Main.rand.Next(12),
                        1.5f + Main.rand.NextFloat(0.8f),
                        new Color(90, 200, 255) * 1.5f
                    );
                    GeneralParticleHandler.SpawnParticle(sq);

                    // GlowOrb 光点
                    GlowOrbParticle orb = new GlowOrbParticle(
                        spawnPos,
                        dir * Main.rand.NextFloat(1f, 3f),
                        false,
                        6,
                        0.9f + Main.rand.NextFloat(0.5f),
                        new Color(120, 220, 255),
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        public override void OnKill(int timeLeft)
        {

        }



    }
}
