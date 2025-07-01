using System;
using System.IO;
using CalamityMod;
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

        private const float MaxSpeed = 24f;
        private const float MinSpeed = 6f;

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


                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/380mmExploded"));

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
