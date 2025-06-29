using System;
using System.IO;
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

        private const int SegmentCount = 10;
        private Segment[] Segments = new Segment[SegmentCount];

        private const float MaxSpeed = 34f;
        private const float MinSpeed = 6f;
        private static readonly float MaxTurnRate = MathHelper.ToRadians(5f);
        private static readonly float MinTurnRate = MathHelper.ToRadians(0.003f);

        private bool lastTurnLeft = false; // 上一次击中后是左转还是右转，默认 false（右转）
        private float turnRateMultiplier = 1.0f; // 当前转向速度倍率
        private const float MaxTurnRateBase = 8f; // 【可改】基础最大转向度（度）
        private static readonly float MaxTurnRateRad = MathHelper.ToRadians(MaxTurnRateBase); // 转换弧度


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
        }
        private Player lockedPlayer; // 用于B方案时锁定玩家
        private bool useBPlan = false; // 当前弹幕是否使用B方案
        private int startupTimer = 0; // 飞行启动计时器
        private const int StartupDuration = 45; // 直线加速持续时间，可调

        private enum ChasePhase { Startup, TurnLeft, Charge, TurnRight, ChargeBack }
        private ChasePhase chasePhase = ChasePhase.Startup;
        private int phaseTimer = 0;

        public void SetBPlan(bool enable)
        {
            useBPlan = enable;

            // 立即在设置时执行目标锁定
            if (useBPlan)
                lockedPlayer = FindClosestPlayer(1600f);
            else
                lockedTarget = FindClosestNPC(1600f);
        }

        public override void AI()
        {
            // === 特效生成（全程存在） ===
            {
                // 1️⃣ 喷射重型火焰烟雾（更有质感）
                //for (int i = 0; i < 3; i++) // 增加生成量和密度
                //{
                //    // 正前方方向
                //    Vector2 baseDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                //    // 在 -20° ~ 20° 内散射
                //    float scatterAngle = MathHelper.ToRadians(Main.rand.NextFloat(-20f, 20f));
                //    Vector2 smokeVelocity = baseDirection.RotatedBy(scatterAngle) * Main.rand.NextFloat(2f, 5f);

                //    // 烟雾颜色
                //    Color smokeColor = Color.Lerp(Color.OrangeRed, Color.DarkOrange, Main.rand.NextFloat(0.3f, 0.7f));

                //    // 烟雾生成位置（稍微随机偏移模拟火焰口喷射范围）
                //    Vector2 spawnPosition = Projectile.Center + baseDirection * 30f + Main.rand.NextVector2Circular(8f, 8f);

                //    // 生成重型烟雾粒子
                //    Particle smoke = new HeavySmokeParticle(
                //        spawnPosition,
                //        smokeVelocity,
                //        smokeColor,
                //        30, // 生命周期更长
                //        Projectile.scale * Main.rand.NextFloat(0.8f, 1.5f),
                //        1.0f,
                //        MathHelper.ToRadians(Main.rand.NextFloat(-3f, 3f)), // 轻微旋转
                //        required: true // 使用强制渲染，更有质感
                //    );

                //    GeneralParticleHandler.SpawnParticle(smoke);
                //}

                // 2️⃣ 火焰尖刺鳞片粒子
                //if (Main.rand.NextBool(2))
                //{
                //    for (int i = 0; i < 2; i++) // 增加数量
                //    {
                //        Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * 0.8f;
                //        Color sparkColor = Color.Lerp(Color.Orange, Color.Gold, Main.rand.NextFloat(0.3f, 0.7f));
                //        PointParticle spark = new PointParticle(
                //            Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                //            sparkVel,
                //            false,
                //            20,
                //            Main.rand.NextFloat(1f, 1.4f),
                //            sparkColor
                //        );
                //        GeneralParticleHandler.SpawnParticle(spark);
                //    }
                //}

                // 3️⃣ 周期性冲击波（能量波纹）
                if (Main.GameUpdateCount % 12 == 0) // 更高频率
                {
                    for (int i = 0; i < 2; i++) // 每次生成两次不同色冲击波
                    {
                        Color pulseColor = i == 0 ? Color.OrangeRed : Color.Yellow;
                        Particle pulse = new DirectionalPulseRing(
                            Projectile.Center,
                            Projectile.velocity * 0.75f,
                            pulseColor,
                            new Vector2(1f, 2.5f),
                            Projectile.rotation - MathHelper.PiOver4 + Main.rand.NextFloat(-0.1f, 0.1f),
                            0.25f,
                            0.03f,
                            24
                        );
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }
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
                // ===== A方案：启动阶段（直线加速） =====
                if (chasePhase == ChasePhase.Startup)
                {
                    Projectile.velocity += initialDirection * 0.25f;
                    Projectile.velocity = Vector2.Clamp(Projectile.velocity, Vector2.One * MinSpeed, Vector2.One * MaxSpeed * 0.8f);

                    phaseTimer++;
                    if (phaseTimer >= 45) // 启动完成
                    {
                        chasePhase = ChasePhase.TurnLeft;
                        phaseTimer = 0;
                    }
                    return;
                }

                if (lockedTarget == null || !lockedTarget.active)
                {
                    // 无敌人时继续直线飞行
                    Projectile.velocity = Projectile.velocity.SafeNormalize(initialDirection) * MaxSpeed;
                    return;
                }

                Vector2 toTarget = lockedTarget.Center - Projectile.Center;
                float desiredAngle = toTarget.ToRotation();
                float currentAngle = Projectile.velocity.ToRotation();
                float angleDiff = MathHelper.WrapAngle(desiredAngle - currentAngle);

                float turnSpeed = MathHelper.ToRadians(1.5f); // 【可调】转弯速度
                float chargeSpeed = MaxSpeed * 1.1f;          // 【可调】冲刺速度

                switch (chasePhase)
                {
                    case ChasePhase.TurnLeft:
                        // 持续左转
                        Projectile.velocity = Projectile.velocity.RotatedBy(-turnSpeed).SafeNormalize(Vector2.UnitY) * MaxSpeed;
                        phaseTimer++;
                        if (phaseTimer >= 30)
                        {
                            chasePhase = ChasePhase.Charge;
                            phaseTimer = 0;
                        }
                        break;

                    case ChasePhase.Charge:
                        // 朝向目标冲刺
                        Projectile.velocity = Vector2.Lerp(
                            Projectile.velocity,
                            toTarget.SafeNormalize(Vector2.UnitY) * chargeSpeed,
                            0.15f
                        );
                        phaseTimer++;
                        if (phaseTimer >= 20)
                        {
                            chasePhase = ChasePhase.TurnRight;
                            phaseTimer = 0;
                        }
                        break;

                    case ChasePhase.TurnRight:
                        // 持续右转
                        Projectile.velocity = Projectile.velocity.RotatedBy(turnSpeed).SafeNormalize(Vector2.UnitY) * MaxSpeed;
                        phaseTimer++;
                        if (phaseTimer >= 30)
                        {
                            chasePhase = ChasePhase.ChargeBack;
                            phaseTimer = 0;
                        }
                        break;

                    case ChasePhase.ChargeBack:
                        // 再次朝向目标冲刺
                        Projectile.velocity = Vector2.Lerp(
                            Projectile.velocity,
                            toTarget.SafeNormalize(Vector2.UnitY) * chargeSpeed,
                            0.15f
                        );
                        phaseTimer++;
                        if (phaseTimer >= 20)
                        {
                            chasePhase = ChasePhase.TurnLeft;
                            phaseTimer = 0;
                        }
                        break;
                }





            }
            else
            {


                // ===== B 方案：围绕玩家旋转后高速附身 =====
                if (lockedPlayer == null || !lockedPlayer.active)
                {
                    Projectile.Kill();
                    return;
                }

                float elapsed = 300 - Projectile.timeLeft;


                if (elapsed < 50f)
                {
                    // 🚩 最优雅真实高速公转围绕玩家阶段

                    float desiredRadius = 320f; // 公转半径
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


                else if (elapsed < 60f)
                {
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
                    // 阶段 3：极速追踪玩家，速度完全匹配
                    Projectile.velocity = lockedPlayer.velocity;
                }
                else
                {
                    // 超过 120 帧后检测 FinishingTouchDASH 是否存在
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

                    if (!dashExists)
                    {
                        Projectile.Kill();
                        return;
                    }
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
            // 命中敌人后切换转向方向
            lastTurnLeft = !lastTurnLeft; // 切换追踪方向

            // 命中后短暂提升转向速率上限
            turnRateMultiplier = 1.3f; // 撞击后立刻提升到 130%

            {
                // 命中时释放爆炸烟雾、gore
                for (int i = 0; i < 8; i++)
                {
                    int goreIndex = Gore.NewGore(Projectile.GetSource_FromThis(), target.Center, Main.rand.NextVector2Circular(3f, 3f), Main.rand.Next(61, 64));
                    Main.gore[goreIndex].scale = Main.rand.NextFloat(0.8f, 1.5f);
                }

                for (int i = 0; i < 20; i++)
                {
                    int dust = Dust.NewDust(target.position, target.width, target.height, DustID.Smoke);
                    Main.dust[dust].scale = 1.5f;
                    Main.dust[dust].velocity = Main.rand.NextVector2Circular(6f, 6f);
                    Main.dust[dust].noGravity = true;
                }

                // 六芒星魔法阵（Dust + 线性粒子模拟）
                for (int i = 0; i < 36; i++)
                {
                    Vector2 dustPos = target.Center + Vector2.One.RotatedBy(MathHelper.ToRadians(i * 10)) * 40f;
                    Dust d = Dust.NewDustDirect(dustPos, 1, 1, DustID.OrangeTorch, 0f, 0f, 150, Color.Orange, 1.2f);
                    d.velocity = (target.Center - dustPos).SafeNormalize(Vector2.Zero) * 2f;
                    d.noGravity = true;
                }
            }
           
        }


        private void InitializeSegments()
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                Segments[i] = new Segment(255, Projectile.rotation, Projectile.Center - initialDirection * i * 70f);
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

            Main.EntitySpriteDraw(headTex, Projectile.Center - screenPos, null, color, Projectile.rotation, headTex.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
