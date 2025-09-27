using System;
using System.IO;
using System.Linq;
using CalamityMod;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch.FTDragon
{
    public class FinishingTouchDragon : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";

        private NPC lockedTarget;
        private Vector2 initialDirection;
        private bool initialized = false;

        private const int SegmentCount = 18;
        private Segment[] Segments = new Segment[SegmentCount];

        private const float MaxSpeed = 27f;
        private const float MinSpeed = 8f;

        private Player lockedPlayer; // 用于B方案
        private bool useBPlan = false; // 用于B方案

        private int startupTimer = 0; // 用于B方案

        private bool hasChased = false;      // 是否已执行过一次追踪
        private bool hasHitTarget = false;   // 是否已击中过目标

        private const int DelayBeforeChase = 60; // 延迟帧数，可调
        private int chaseDelayTimer = 0; // 延迟计时器


        internal class Segment
        {
            internal short Alpha;
            internal float Rotation;
            internal Vector2 Center;

            internal Segment(byte alpha, float rotation, Vector2 center)
            {
                Alpha = alpha;
                Rotation = rotation;
                Center = center;
            }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 108;
            Projectile.height = 108;
            Projectile.alpha = 255;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.light = 1.2f;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 60; // 无敌帧冷却时间为30帧
        }

        public void SetBPlan(bool enable)
        {
            useBPlan = enable;

            // 立即在设置时执行目标锁定
            if (useBPlan)
                lockedPlayer = FindClosestPlayer(1600f);
            else
                lockedTarget = FindClosestNPC(3600f);
        }

        public override void AI()
        {
            // === 飞行期间特效生成（全程存在） ===
            {
                Vector2 baseDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                // 1️⃣ 喷射重型火焰烟雾（质感尾焰）
                if (Main.rand.NextBool(3)) // 减少密度避免遮挡
                {
                    Vector2 spawnPosition = Projectile.Center - baseDirection * 40f + Main.rand.NextVector2Circular(6f, 6f);
                    Vector2 smokeVelocity = -baseDirection.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(1.5f, 3f);
                    Color smokeColor = Color.Lerp(Color.DarkOrange, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.8f));

                    Particle smoke = new HeavySmokeParticle(
                        spawnPosition,
                        smokeVelocity,
                        smokeColor,
                        36, // 生命周期适中
                        Projectile.scale * Main.rand.NextFloat(0.7f, 1.2f),
                        0.8f,
                        MathHelper.ToRadians(Main.rand.NextFloat(-2f, 2f)),
                        required: false
                    );

                    GeneralParticleHandler.SpawnParticle(smoke);
                }

                // 2️⃣ 飞行火花点缀
                if (Main.rand.NextBool(4))
                {
                    Vector2 sparkVelocity = -baseDirection.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(2f, 4f);
                    Color sparkColor = Color.Lerp(Color.Orange, Color.Gold, Main.rand.NextFloat(0.3f, 0.7f));
                    PointParticle spark = new PointParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        sparkVelocity,
                        false,
                        25,
                        Main.rand.NextFloat(0.8f, 1.2f),
                        sparkColor
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 3️⃣ 周期性小型能量脉冲
                if (Main.GameUpdateCount % 30 == 0)
                {
                    Color pulseColor = Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat(0.3f, 0.7f));
                    Particle pulse = new DirectionalPulseRing(
                        Projectile.Center,
                        Projectile.velocity * 0.5f,
                        pulseColor,
                        new Vector2(0.8f, 1.8f),
                        Projectile.rotation - MathHelper.PiOver4 + Main.rand.NextFloat(-0.05f, 0.05f),
                        0.18f,
                        0.02f,
                        20
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }













            if (!initialized)
            {
                initialDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY * -1f);
                InitializeSegments();
                initialized = true;
            }

            // === 透明度渐变 ===
            Projectile.alpha = Utils.Clamp(Projectile.alpha - 10, 0, 255);

            // === 更新段位置（保持蠕动流畅） ===
            for (int i = 0; i < SegmentCount; i++)
                UpdateSegment(i);

            // === 飞行行为控制 ===
            if (!useBPlan)
            {
                if (lockedTarget == null || !lockedTarget.active)
                {
                    lockedTarget = FindClosestNPC(3600f);
                }

                if (lockedTarget == null || !lockedTarget.active)
                {
                    // 无敌人时继续直线上飞
                    Projectile.velocity = Vector2.UnitY * -MaxSpeed * 2f;
                    return;
                }

                if (!hasChased)
                {
                    if (chaseDelayTimer < DelayBeforeChase)
                    {
                        // 启动阶段：正上方高速飞行
                        Projectile.velocity = Vector2.UnitY * -MaxSpeed * 2f;
                        chaseDelayTimer++;
                        Projectile.friendly = false; // 🚩 禁用伤害
                    }
                    else
                    {
                        // 延迟后直接快速朝向敌人冲刺
                        Vector2 toTarget = (lockedTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        Projectile.velocity = toTarget * MaxSpeed * 6f; // 极高速冲锋
                        Projectile.friendly = true; // 🚩 开启伤害
                    }
                }
                else
                {
                    // 命中后什么也不做，保留原有的速度



                }
            }



            else
            {


                // ===== B 方案：围绕玩家旋转后高速附身 =====

                // 检查是否存在 FinishingTouchDASH，如果不存在则自杀
                bool dashExists = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == Projectile.owner && proj.type == ModContent.ProjectileType<FinishingTouchDASH>())
                    {
                        dashExists = true;
                        break;
                    }
                }

                // 🚩 如找不到 FinishingTouchDASH，则立即销毁自身
                if (!dashExists || lockedPlayer == null || !lockedPlayer.active)
                {
                    Projectile.Kill();
                    return;
                }


                float elapsed = 300 - Projectile.timeLeft;


                if (elapsed < 50f)
                {
                    Projectile.friendly = false;

                    // 🚩 最优雅真实高速公转围绕玩家阶段

                    float desiredRadius = 540f; // 公转半径
                    float angularSpeed = MathHelper.TwoPi * 3f; // 每秒3圈（可调：越大转越快）

                    // 玩家位置实时更新
                    Vector2 playerCenter = lockedPlayer.Center;
                    Vector2 toProjectile = Projectile.Center - playerCenter;

                    // 保证有初始偏移避免零向量
                    if (toProjectile == Vector2.Zero)
                        toProjectile = Vector2.UnitY * desiredRadius;

                    // 当前半径和角度
                    float currentRadius = toProjectile.Length();
                    float currentAngle = toProjectile.ToRotation();

                    // 持续快速旋转
                    float newAngle = currentAngle + angularSpeed * (1f / 60f); // 每帧角度增量

                    // 平滑收敛半径
                    float radiusLerpSpeed = 0.1f; // 可调收敛速度
                    float adjustedRadius = MathHelper.Lerp(currentRadius, desiredRadius, radiusLerpSpeed);

                    // 计算目标位置
                    Vector2 targetPos = playerCenter + newAngle.ToRotationVector2() * adjustedRadius;

                    // 计算前往目标位置所需速度（确保持续平滑公转）
                    Vector2 desiredVelocity = (targetPos - Projectile.Center) * 0.5f; // 0.5f 可调收敛速率

                    Projectile.velocity = desiredVelocity;

                    // rotation 实时对齐速度方向
                    if (Projectile.velocity.LengthSquared() > 0.01f)
                        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                }


                else if (elapsed < 75f)
                {
                    Projectile.friendly = true;

                    // 阶段 2：10 帧快速飞向玩家位置
                    Vector2 toPlayer = lockedPlayer.Center - Projectile.Center;
                    Vector2 desiredVelocity = toPlayer.SafeNormalize(Vector2.UnitY) * MaxSpeed * 2f; // 🚩 使用 2x MaxSpeed 提高拉直速度

                    Projectile.velocity = Vector2.Lerp(
                        Projectile.velocity,
                        desiredVelocity,
                        0.5f // 🚩 提升插值速率，加快收敛
                    );
                }
                else if (elapsed < 120f)
                {
                    Projectile.friendly = true;

                    // 阶段 3：极速追踪玩家，速度略快
                    Projectile.velocity = lockedPlayer.velocity;
                }
                else
                {
                    Projectile.friendly = true;

                }


            }



            // === 更新旋转保持朝向速度 ===
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        private Player FindClosestPlayer(float maxDist)
        {
            Player closestPlayer = null;
            float minDist = maxDist;

            foreach (Player player in Main.player)
            {
                if (player.active && !player.dead)
                {
                    float dist = Vector2.Distance(Projectile.Center, player.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestPlayer = player;
                    }
                }
            }
            return closestPlayer;
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中敌人后关闭追踪能力，进入自由飞行
            int slashCount = 1;
            for (int i = 0; i < slashCount; i++)
            {
                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                int slashID = ModContent.ProjectileType<FinishingTouchDASHFuckYou>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, randomDirection, slashID, ((Projectile.damage) * 1), Projectile.knockBack, Projectile.owner);
            }

            if (!hasChased)
            {
                hasChased = true; // 命中后进入停止追踪阶段

                // 🚩 距离衰减型屏幕震动
                float shakePower = 100f; // 设置基础震动强度（可调）
                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);


                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/380mmExploded") with { Volume = 1.2f, Pitch = 0.0f }, Projectile.Center);





                // 大型混合命中特效
                SpawnGrandHitFX_OrangeNova(Projectile.Center, Projectile.velocity);

                {

                    // ======================== 无序部分 1：极限狂野扩散烟雾 ========================
                    for (int i = 0; i < Main.rand.Next(80, 101); i++)
                    {
                        int dust = Dust.NewDust(target.position, target.width, target.height, DustID.Smoke);
                        Main.dust[dust].scale = Main.rand.NextFloat(1.8f, 3.0f);
                        Main.dust[dust].velocity = Main.rand.NextVector2Circular(12f, 12f);
                        Main.dust[dust].noGravity = true;
                    }

                    // ======================== 无序部分 2：极限狂野火花粒子扩散（再翻5倍） ========================
                    for (int i = 0; i < 600; i++) // 🚩 数量提升至600+
                    {
                        int type = Main.rand.NextFloat() < 0.7f ? DustID.OrangeTorch : DustID.FlameBurst; // 🚩 使用浮点概率避免报错

                        Vector2 randPos = target.Center + Main.rand.NextVector2Circular(300f, 300f); // 🚩 扩散范围极大

                        Dust d = Dust.NewDustDirect(
                            randPos,
                            1, 1,
                            type,
                            0f, 0f,
                            50,
                            Color.OrangeRed,
                            Main.rand.NextFloat(2f, 4f) // 🚩 放大粒子大小
                        );

                        d.velocity = Main.rand.NextVector2Circular(25f, 50f); // 🚩 扩散速度极快
                        d.noGravity = true;
                    }


                    // ======================== 无序部分 2：极限狂野火花粒子扩散（再翻5倍） ========================
                    for (int i = 0; i < 600; i++) // 🚩 数量提升至600+
                    {
                        int type = Main.rand.NextFloat() < 0.7f ? DustID.OrangeTorch : DustID.FlameBurst; // 🚩 修复：使用浮点概率
                        Vector2 randPos = target.Center + Main.rand.NextVector2Circular(300f, 300f); // 🚩 扩散范围极大
                        Dust d = Dust.NewDustDirect(randPos, 1, 1, type, 0f, 0f, 50, Color.OrangeRed, Main.rand.NextFloat(2f, 4f)); // 🚩 放大
                        d.velocity = Main.rand.NextVector2Circular(25f, 50f); // 🚩 扩散速度极快
                        d.noGravity = true;
                    }

                    // ======================== 有序部分 1：超大型复杂六芒星魔法阵 Dust（再翻5倍） ========================
                    int layers = Main.rand.Next(7, 10); // 🚩 层数提升至7~9层
                    for (int layer = 0; layer < layers; layer++)
                    {
                        float radius = 300f + layer * 150f; // 🚩 半径提升至300~1500+
                        Color color = Color.Lerp(Color.OrangeRed, Color.Yellow, layer / (float)layers);
                        int points = 144; // 🚩 每层144个点
                        for (int i = 0; i < points; i++)
                        {
                            Vector2 pos = target.Center + (MathHelper.TwoPi * i / points).ToRotationVector2() * radius;
                            Dust d = Dust.NewDustDirect(pos, 1, 1, DustID.OrangeTorch, 0f, 0f, 0, color, 2.5f); // 🚩 放大
                            d.velocity = (target.Center - pos).SafeNormalize(Vector2.Zero) * 15f; // 🚩 回卷速度极快
                            d.noGravity = true;
                        }
                    }


                    // ======================== 有序部分 2：多层火花环形拖尾线性粒子 ========================
                    int sparkLayers = 6;
                    for (int l = 0; l < sparkLayers; l++)
                    {
                        float ringRadius = 40f + l * 30f;
                        int points = 36;
                        for (int p = 0; p < points; p++)
                        {
                            Vector2 pos = target.Center + (MathHelper.TwoPi * p / points).ToRotationVector2() * ringRadius;
                            Vector2 dir = (pos - target.Center).SafeNormalize(Vector2.UnitY);
                            Color color = Color.Lerp(Color.Orange, Color.Red, l / (float)sparkLayers);

                            Particle trail = new SparkParticle(
                                pos,
                                dir * 10f, // 更快速度扩散
                                false,
                                70, // 延长生命周期
                                1.5f,
                                color
                            );
                            GeneralParticleHandler.SpawnParticle(trail);
                        }
                    }


                }


            }

            if (useBPlan)
            {


                SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);


                // ======================== 无序部分 1：小范围喷射烟雾 ========================
                for (int i = 0; i < 15; i++) // 🚩 大幅减少数量
                {
                    Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.ToRadians(20));
                    int dust = Dust.NewDust(target.Center, 1, 1, DustID.Smoke, 0f, 0f, 50, default, Main.rand.NextFloat(1f, 1.5f));
                    Main.dust[dust].velocity = direction * Main.rand.NextFloat(4f, 8f); // 🚩 沿方向喷射
                    Main.dust[dust].noGravity = true;
                }

                // ======================== 无序部分 2：火花粒子沿方向喷射 ========================
                for (int i = 0; i < 25; i++) // 🚩 大幅减少数量
                {
                    int type = Main.rand.NextFloat() < 0.7f ? DustID.OrangeTorch : DustID.FlameBurst;
                    Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.ToRadians(15));
                    Dust d = Dust.NewDustDirect(target.Center, 1, 1, type, 0f, 0f, 50, Color.OrangeRed, Main.rand.NextFloat(1f, 1.8f));
                    d.velocity = direction * Main.rand.NextFloat(5f, 10f);
                    d.noGravity = true;
                }

                // ======================== 有序部分 1：前方能量线性拖尾火花（SparkParticle） ========================
                for (int i = 0; i < 10; i++)
                {
                    Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.ToRadians(10)) * Main.rand.NextFloat(10f, 30f);
                    Particle trail = new SparkParticle(
                        target.Center + offset,
                        offset.SafeNormalize(Vector2.UnitY) * 8f, // 向前喷射
                        false,
                        40, // 生命周期短
                        1.2f,
                        Color.Orange
                    );
                    GeneralParticleHandler.SpawnParticle(trail);
                }

                // ======================== 有序部分 2：前方锥形小型火花喷发环 ========================
                int points = 18;
                float angleSpread = MathHelper.ToRadians(60); // 60度锥形扩散
                for (int p = 0; p < points; p++)
                {
                    float angle = MathHelper.Lerp(-angleSpread / 2, angleSpread / 2, p / (float)(points - 1));
                    Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(angle);
                    Particle trail = new SparkParticle(
                        target.Center,
                        direction * Main.rand.NextFloat(6f, 12f),
                        false,
                        30, // 生命周期短
                        1f,
                        Color.Lerp(Color.Orange, Color.Yellow, p / (float)points)
                    );
                    GeneralParticleHandler.SpawnParticle(trail);
                }
            }









        }

        private void SpawnGrandHitFX_OrangeNova(Vector2 center, Vector2 baseVelocity)
        {
            // =============================== 可调参数（全部集中在此） ===============================
            // —— 覆盖规模（像素）：外圈半径建议 ≥ 20*16=320px；火焰主题更偏“厚重涌动”的体量感
            float tile = 16f;
            float radiusInner = 10f * tile;    // ≈160px，内核（烈焰内核/熔岩核）
            float radiusMiddle = 16f * tile;    // ≈256px，中圈（火舌/焰浪织构）
            float radiusOuter = 20f * tile;    // ≈320px，外圈（冲击/灰烬回流）
            float radiusOvershoot = 22.5f * tile;  // ≈360px，最外缘抖动/定形

            // —— 颜色主题：橙红 → 金黄 → 炽白核心（与龙息火焰一致）
            Color colCore = new Color(255, 96, 48);     // 炽热橙红（主体）
            Color colHot = new Color(255, 140, 64);    // 火舌橙
            Color colGlow = new Color(255, 210, 90);    // 金黄高光
            Color colBloom = new Color(255, 180, 50);    // 柔光打底（中心）

            // —— 中心 GenericBloom（“火核”打底）
            float bloomScaleCore = 2.8f;
            int bloomLifeCore = 54;

            // —— GlowOrb：星芒/多边形/环线（短生命，但高密度“连点成线成纹理”）
            int orbRays = 72;    // 放射光束数量（更饱满）
            int orbLife = 10;    // 瞬闪
            float orbScaleCenter = 1.35f; // 中心点大小
            float orbScaleEnd = 0.85f; // 射线端点大小
            int polySides = 11;    // 不规则正多边形（奇数边更“生物感”）
            float polyRadius = radiusMiddle * 0.9f;

            // —— Dust“焰浪阶梯环”（全部无重力，安全数值 ID）
            int dustFireA = 6;     // Torch 系（橙）
            int dustFireB = 35;    // 火花
            int dustAsh = 31;    // 灰烬（淡/冷灰衬底）
            int ringLayers = 5;     // 环数
            int ringPoints = 96;    // 每环点数
            float ringStart = radiusInner * 0.55f;
            float ringStep = (radiusMiddle - ringStart) / Math.Max(1, ringLayers - 1);
            float ringJitter = 3.2f;  // 每点随机漂移

            // —— SparkParticle：火舌波浪（分层涌动，角度扰动让火舌“呼吸”）
            int sparkLayers = 5;
            int sparkPerLayer = 76;
            float sparkBaseSpeed = 9.6f;   // 火舌外冲速度
            float sparkSpeedStep = 2.4f;   // 层间叠加
            float sparkBaseScale = 1.2f;   // 代表长度/亮度
            float sparkScaleStep = 0.28f;
            float sparkWaveAmp = 0.95f;  // 角度摆动幅度（弧度）
            float sparkWaveFreq = 2.7f;   // 波动频率
            int sparkLife = 62;
            float sparkStartRing = radiusInner * 0.38f; // 起点分布在内核边缘

            // —— 火焰“旋臂”补强：对数螺旋 + 线性连珠（GlowOrb 作为明亮火种）
            int spiralArms = 2;      // 双臂（左/右）
            int spiralPoints = 52;     // 每臂点数
            float spiralK = 0.22f;  // 对数螺旋增长系数
            int spiralOrbLife = 12;
            float spiralOrbScale = 1.05f;

            // —— 放射“燃烧丝”补强（更像火舌分叉，而非电丝）
            int flameRayCount = 18;
            int flameRaySegments = 38;
            float flameStep = 10.5f;
            float flameJitter = 5.5f;
            float flameCurviness = 0.28f;  // 弯曲度更柔（像火焰）
            int flameSparksPerSeg = 2;
            int flameOrbsPerSeg = 1;

            // —— 震动反馈
            float shakePower = 16f;

            // —— 追加特效 · 你点名的两类 —— //
            // 12. 熔岩 Metaball：必须集中在半径 4×16 的圆内
            float lavaCoreRadius = 4f * tile;     // 仅在这个半径内生成
            int lavaBallCount = 10;            // 生成数量
            float lavaBallSizeMin = 60f;           // 粒子大小（半径）
            float lavaBallSizeMax = 100f;

            // 8. 圆形冲击波（收缩环）：可在命中点附近偏移，营造龙息冲击波
            int pulseCount = 3;             // 同时生成几道收缩环
            float pulseOffsetMin = 18f;           // 相对命中点的随机偏移（像喷吐偏心）
            float pulseOffsetMax = 60f;
            Color pulseColor = new Color(255, 120, 60); // 橙红冲击波
            Vector2 pulseShape = new Vector2(1f, 1f);     // 圆形
            float pulseScaleStart = 7.5f;          // 初始缩放
            float pulseScaleEnd = 0.18f;         // 收缩到极小
            float pulseSpread = 3.5f;          // 扩散范围
            int pulseLife = 12;            // 粒子寿命
                                           // ======================================================================

            // =============================== 工具 ===============================
            Vector2 DirOrUp(Vector2 v) => v.LengthSquared() > 0.01f ? v.SafeNormalize(Vector2.UnitY) : Vector2.UnitY;

            Vector2 SoftNoiseDir(Vector2 baseDir, float strength)
            {
                float a = Main.rand.NextFloat(-strength, strength);
                return baseDir.RotatedBy(a).SafeNormalize(baseDir);
            }

            void LineOrbs(Vector2 a, Vector2 b, float step, float scMin, float scMax, Color col, int lifeMin = 10, int lifeMax = 16)
            {
                Vector2 d = b - a;
                float len = d.Length();
                if (len <= 0.5f) return;
                Vector2 dir = d / len;
                for (float t = 0; t <= len; t += step)
                {
                    var orb = new GlowOrbParticle(
                        a + dir * t,
                        Vector2.Zero,
                        false,
                        Main.rand.Next(lifeMin, lifeMax),
                        Main.rand.NextFloat(scMin, scMax),
                        col, true, false, true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }

            void RingOrbs(Vector2 c, float r, int points, float scMin, float scMax, Color col, int lifeMin = 10, int lifeMax = 16)
            {
                for (int i = 0; i < points; i++)
                {
                    float ang = MathHelper.TwoPi * i / points;
                    var orb = new GlowOrbParticle(
                        c + ang.ToRotationVector2() * r,
                        Vector2.Zero,
                        false,
                        Main.rand.Next(lifeMin, lifeMax),
                        Main.rand.NextFloat(scMin, scMax),
                        col, true, false, true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }
            // ===================================================================

            // ======================= 1) 中心火核：Bloom（打底） =======================
            {
                var bloom = new GenericBloom(center, Vector2.Zero, colBloom, bloomScaleCore, bloomLifeCore);
                GeneralParticleHandler.SpawnParticle(bloom);
            }

            // ======================= 2) GlowOrb：火焰星芒 + “生物感”多边形骨架 =======================
            // 星芒（中心+超外圈端点）：撑满体量
            for (int k = 0; k < orbRays; k++)
            {
                float ang = MathHelper.TwoPi * (k / (float)orbRays);
                Vector2 end = center + ang.ToRotationVector2() * radiusOvershoot;

                var o0 = new GlowOrbParticle(center, Vector2.Zero, false, orbLife, orbScaleCenter, colGlow, true, false, true);
                var o1 = new GlowOrbParticle(end, Vector2.Zero, false, orbLife, orbScaleEnd, colGlow, true, false, true);
                GeneralParticleHandler.SpawnParticle(o0);
                GeneralParticleHandler.SpawnParticle(o1);
            }
            // “不规则”正多边形点阵（奇数边：更像生物火焰的张力）
            for (int i = 0; i < polySides; i++)
            {
                float t = i / (float)polySides;
                float ang = MathHelper.TwoPi * t;
                Vector2 p = center + ang.ToRotationVector2() * polyRadius;
                var orb = new GlowOrbParticle(p, Vector2.Zero, false, orbLife, 1.05f, colHot, true, false, true);
                GeneralParticleHandler.SpawnParticle(orb);
            }
            // 多边形边线（连珠成线）
            for (int i = 0; i < polySides; i++)
            {
                float a0 = MathHelper.TwoPi * i / polySides;
                float a1 = MathHelper.TwoPi * (i + 1) / polySides;
                Vector2 p0 = center + a0.ToRotationVector2() * polyRadius;
                Vector2 p1 = center + a1.ToRotationVector2() * polyRadius;
                LineOrbs(p0, p1, 6.0f, 0.9f, 1.2f, colHot);
            }

            // ======================= 3) Dust：焰浪阶梯环 + 灰烬点缀 =======================
            for (int layer = 0; layer < ringLayers; layer++)
            {
                float r = ringStart + ringStep * layer;
                for (int i = 0; i < ringPoints; i++)
                {
                    float t = i / (float)ringPoints;
                    float ang = MathHelper.TwoPi * t;
                    Vector2 pos = center + ang.ToRotationVector2() * r + Main.rand.NextVector2Circular(ringJitter, ringJitter);

                    // 主环（橙红）
                    Dust d1 = Dust.NewDustPerfect(pos, dustFireA, Vector2.Zero);
                    d1.noGravity = true;
                    d1.scale = 1.25f + 0.12f * layer;

                    // 高光（金黄）
                    if ((i + layer) % 4 == 0)
                    {
                        Dust d2 = Dust.NewDustPerfect(pos, dustFireB, Vector2.Zero);
                        d2.noGravity = true;
                        d2.scale = 0.95f + 0.1f * layer;
                        d2.fadeIn = 1.2f;
                    }

                    // 少量灰烬（冷色灰，让暖色火焰更“热”）
                    if ((i + layer) % 9 == 0)
                    {
                        Dust ash = Dust.NewDustPerfect(pos, dustAsh, Main.rand.NextVector2Circular(0.6f, 0.6f));
                        ash.noGravity = true;
                        ash.scale = 0.9f;
                    }
                }
            }

            // ======================= 4) Spark：火舌波浪（分层呼吸） =======================
            Vector2 baseDir = DirOrUp(baseVelocity);
            for (int layer = 0; layer < sparkLayers; layer++)
            {
                float layerSpeed = sparkBaseSpeed + sparkSpeedStep * layer;
                float layerScale = sparkBaseScale + sparkScaleStep * layer;
                Color layerColor = Color.Lerp(colCore, colGlow, layer / Math.Max(1f, (float)(sparkLayers - 1)));

                for (int i = 0; i < sparkPerLayer; i++)
                {
                    float t = i / Math.Max(1f, sparkPerLayer - 1f);
                    float phase = (layer * 0.8f + t * sparkWaveFreq) * MathHelper.TwoPi;
                    float offset = (float)Math.Sin(phase) * sparkWaveAmp;

                    float startRing = sparkStartRing + layer * 7f + Main.rand.NextFloat(-5f, 5f);
                    Vector2 start = center + (t * MathHelper.TwoPi).ToRotationVector2() * startRing;

                    Vector2 v = baseDir.RotatedBy(offset) * layerSpeed;

                    Particle sp = new SparkParticle(
                        start,
                        v,
                        false,
                        sparkLife,
                        layerScale,
                        layerColor
                    );
                    GeneralParticleHandler.SpawnParticle(sp);
                }
            }

            // ======================= 5) 火焰“旋臂”补强（对数螺旋） =======================
            for (int arm = 0; arm < spiralArms; arm++)
            {
                float sign = (arm == 0) ? 1f : -1f;
                Vector2 prev = center;
                for (int k = 0; k < spiralPoints; k++)
                {
                    float t = k / (float)Math.Max(1, spiralPoints - 1);
                    float theta = sign * (t * MathHelper.TwoPi * 1.45f);     // ~1.45 圈
                    float r = 12f * (float)Math.Exp(spiralK * (t * 8f)); // 指数拉伸
                    Vector2 pos = center + new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * r;

                    var orb = new GlowOrbParticle(
                        pos,
                        SoftNoiseDir((pos - center), 0.3f) * Main.rand.NextFloat(0.4f, 1.4f),
                        false,
                        spiralOrbLife,
                        spiralOrbScale * Main.rand.NextFloat(0.92f, 1.15f),
                        Color.Lerp(colHot, colGlow, 0.45f),
                        true, false, true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);

                    // 连线成“火舌丝带”
                    if (k > 0)
                        LineOrbs(prev, pos, 7.0f, 0.88f, 1.10f, Color.Lerp(colCore, colGlow, 0.35f), 8, 12);

                    prev = pos;
                }
            }

            // ======================= 6) 放射“燃烧丝”补强（柔弯火舌） =======================
            for (int r = 0; r < flameRayCount; r++)
            {
                float ang = MathHelper.TwoPi * (r / (float)flameRayCount) + Main.rand.NextFloat(-0.14f, 0.14f);
                Vector2 dir = ang.ToRotationVector2();
                Vector2 p = center;
                Vector2 cur = dir;

                float stopR = radiusOuter + Main.rand.NextFloat(-10f, 14f);

                for (int s = 0; s < flameRaySegments; s++)
                {
                    cur = Vector2.Lerp(cur, SoftNoiseDir(cur, flameCurviness), 0.9f).SafeNormalize(cur);
                    Vector2 jitter = Main.rand.NextVector2Circular(flameJitter, flameJitter);
                    p += cur * flameStep + jitter;

                    if (Vector2.Distance(center, p) > stopR) break;

                    // 火花
                    for (int i = 0; i < flameSparksPerSeg; i++)
                    {
                        var sp = new SparkParticle(
                            p,
                            cur * Main.rand.NextFloat(2.8f, 6.2f),
                            false,
                            Main.rand.Next(18, 26),
                            Main.rand.NextFloat(0.95f, 1.45f),
                            Color.Lerp(colCore, colGlow, Main.rand.NextFloat(0.25f, 0.85f))
                        );
                        GeneralParticleHandler.SpawnParticle(sp);
                    }
                    // 光珠
                    for (int i = 0; i < flameOrbsPerSeg; i++)
                    {
                        var orb = new GlowOrbParticle(
                            p,
                            SoftNoiseDir(cur, 0.35f) * Main.rand.NextFloat(0.15f, 0.9f),
                            false,
                            Main.rand.Next(12, 18),
                            Main.rand.NextFloat(0.9f, 1.25f),
                            Color.Lerp(colHot, colGlow, Main.rand.NextFloat(0.3f, 0.9f)),
                            true, false, true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }
            }

            // ======================= 7) 外环“定形” + 灰烬收束 =======================
            RingOrbs(center, radiusOuter, 160, 0.95f, 1.15f, colGlow, 12, 18);
            RingOrbs(center, radiusOvershoot, 190, 0.92f, 1.08f, Color.Lerp(colHot, colGlow, 0.6f), 10, 16);

            // ======================= 8) 追加：熔岩 Metaball（只在核心 4×16 半径内） =======================
            for (int i = 0; i < lavaBallCount; i++)
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(lavaCoreRadius, lavaCoreRadius);
                float size = Main.rand.NextFloat(lavaBallSizeMin, lavaBallSizeMax);
                // 仅生成在核心：RancorLavaMetaball（橙红熔岩滞留）
                RancorLavaMetaball.SpawnParticle(pos, size);
            }

            // ======================= 9) 追加：圆形收缩冲击波（可偏移） =======================
            for (int i = 0; i < pulseCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(pulseOffsetMax, pulseOffsetMax);
                if (offset.Length() < pulseOffsetMin) offset = offset.SafeNormalize(Vector2.UnitX) * pulseOffsetMin;
                Vector2 at = center + offset;

                Particle shrinking = new DirectionalPulseRing(
                    at,
                    Vector2.Zero,
                    pulseColor,         // 橙红
                    pulseShape,         // 圆形
                    Main.rand.NextFloat(pulseScaleStart * 0.9f, pulseScaleStart * 1.1f),
                    pulseScaleEnd,
                    pulseSpread,
                    pulseLife
                );
                GeneralParticleHandler.SpawnParticle(shrinking);
            }

            // ======================= 10) 屏幕震动（距离衰减） =======================
            float kLerp = Utils.GetLerpValue(1000f, 0f, Vector2.Distance(center, Main.LocalPlayer.Center), true);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * kLerp);
        }









        private const float SegmentSpacing = 70f;         // 身体 & 尾部使用
        private const float HeadToBodySpacing = 152f;     // 头部与身体的专用间距

        private void InitializeSegments()
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                float spacing = i == 0 ? 0f : (i == 1 ? HeadToBodySpacing : SegmentSpacing);
                Segments[i] = new Segment(
                    255,
                    Projectile.rotation,
                    Projectile.Center - initialDirection * spacing
                );
            }
        }


        private void UpdateSegment(int index)
        {
            float prevRotation = index == 0 ? Projectile.rotation : Segments[index - 1].Rotation;
            Vector2 prevCenter = index == 0 ? Projectile.Center : Segments[index - 1].Center;
            Vector2 offset = prevCenter - Segments[index].Center;

            float angleOffset = MathHelper.WrapAngle(prevRotation - Segments[index].Rotation);
            offset = offset.RotatedBy(angleOffset * 0.075f);

            Segments[index].Rotation = offset.ToRotation() + MathHelper.PiOver2;
            if (offset != Vector2.Zero)
                Segments[index].Center = prevCenter - offset.SafeNormalize(Vector2.Zero) * 70f;
        }

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float minDist = maxDist;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
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

        public override bool PreDraw(ref Color lightColor)
        {
            // 🚧 如果 Segments 尚未初始化，跳过绘制，避免报错
            if (Segments == null || Segments.Any(s => s == null))
                return false;

            Texture2D headTex = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/FTDragon/FinishingTouchDragon1Head").Value;
            Texture2D bodyTex = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/FTDragon/FinishingTouchDragon2Body").Value;
            Texture2D tailTex = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/FTDragon/FinishingTouchDragon3Tail").Value;

            Vector2 screenPos = Main.screenPosition;
            Vector2 origin = bodyTex.Size() * 0.5f;
            Color color = Color.White * ((255 - Projectile.alpha) / 255f);

            for (int i = 0; i < SegmentCount; i++)
            {
                Texture2D tex = (i == SegmentCount - 1) ? tailTex : bodyTex;
                Vector2 drawPos = Segments[i].Center - screenPos;
                Main.EntitySpriteDraw(tex, drawPos, null, color, Segments[i].Rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(
                headTex,
                Projectile.Center - screenPos,
                null,
                color,
                Projectile.rotation,
                headTex.Size() * 0.5f,
                Projectile.scale,
                (Projectile.velocity.X < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            );


            return false;
        }
    }
}
