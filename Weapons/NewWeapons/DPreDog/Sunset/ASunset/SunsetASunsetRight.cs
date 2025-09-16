using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Drawing;
using Terraria.DataStructures;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept;
using CalamityMod.Particles;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    internal class SunsetASunsetRight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
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
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 🚨 遍历所有投射物，检查是否已有 `Aim` 状态的 `SunsetASunsetRight`
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Projectile.owner && proj.whoAmI != Projectile.whoAmI)
                {
                    // 仅检测相同类型的 `Aim` 状态投射物
                    if (proj.type == ModContent.ProjectileType<SunsetASunsetRight>() && proj.ModProjectile is SunsetASunsetRight rightProj && rightProj.CurrentState == SunsetASunsetRight.BehaviorState.Aim)
                    {
                        Projectile.Kill(); // ❌ 删除自己（新的投射物）
                        return;
                    }

                    if (proj.type == ModContent.ProjectileType<SunsetBForgetRight>() && proj.ModProjectile is SunsetBForgetRight forgetProj && forgetProj.CurrentState == SunsetBForgetRight.BehaviorState.Aim)
                    {
                        Projectile.Kill();
                        return;
                    }

                    if (proj.type == ModContent.ProjectileType<SunsetCConceptRight>() && proj.ModProjectile is SunsetCConceptRight conceptProj && conceptProj.CurrentState == SunsetCConceptRight.BehaviorState.Aim)
                    {
                        Projectile.Kill();
                        return;
                    }
                }
            }
        }
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
        // 在类里新建字段（不要用 localAI）
        private int soundTimer = 0;
        private float currentPitch = 0f;

        private int chargeTimer = 0;
        private bool hasReleasedPulse = false;

        private void DoBehavior_Aim()
        {
            chargeTimer++;




            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.friendly = false;


            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = -Vector2.UnitY; // 朝正上方
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            // 对齐到玩家中心
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            Owner.heldProj = Projectile.whoAmI;


            if (chargeTimer <= 300 && !hasReleasedPulse)
            {

                // —— 音效逻辑 —— 
                soundTimer++;
                if (soundTimer >= 5) // 每 5 帧播放一次
                {
                    soundTimer = 0;

                    // 累加音调，但不超过上限
                    currentPitch = Math.Min(currentPitch + 0.05f, 0.8f);

                    // 播放音效
                    SoundEngine.PlaySound(
                        SoundID.Item73 with
                        {
                            Volume = 0.7f,
                            Pitch = currentPitch
                        },
                        Projectile.Center
                    );
                }



                // 枪头位置
                Vector2 HeadPosition = Projectile.Center
                    + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f
                    + Main.rand.NextVector2Circular(5f, 5f);

                // 正前方方向（归一化）
                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                // 随机 + 数学扰动角度（波动感）
                float time = Main.GameUpdateCount * 0.15f;
                float wave = (float)Math.Sin(time + Projectile.whoAmI * 0.5f) * 0.2f; // 统一波动
                float randomOffset = Main.rand.NextFloat(-0.25f, 0.25f);              // 个体随机扰动
                float angleOffset = wave + randomOffset;

                // 最终朝向
                Vector2 forwardDir = forward.RotatedBy(angleOffset);

                // ========== Dust（前向能量雾） ==========
                if (Main.rand.NextBool(2))
                {
                    Vector2 spawnOffset = Main.rand.NextVector2Circular(20f, 30f); // 在枪头附近范围生成
                    Vector2 velocity = forwardDir * Main.rand.NextFloat(2f, 4f);   // 前向速度

                    Dust d = Dust.NewDustPerfect(
                        HeadPosition + spawnOffset,
                        Main.rand.NextBool() ? DustID.YellowTorch : DustID.IchorTorch,
                        velocity,
                        150,
                        Color.Gold,
                        1.1f
                    );
                    d.noGravity = true;
                    d.fadeIn = 0.8f;
                    d.scale *= Main.rand.NextFloat(0.8f, 1.2f);
                }

                // ========== SparkParticle（前向拖尾） ==========
                if (Main.rand.NextBool(3))
                {
                    Vector2 vel = forwardDir.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f))
                                  * Main.rand.NextFloat(2.5f, 4.5f);

                    Particle spark = new SparkParticle(
                        HeadPosition + Main.rand.NextVector2Circular(12f, 12f),
                        vel,
                        false,
                        Main.rand.Next(18, 26),
                        Main.rand.NextFloat(0.9f, 1.2f),
                        Color.Orange
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // ========== PointParticle（尖锐能量碎片） ==========
                if (Main.rand.NextBool(4))
                {
                    // 半规则：以12等分为基准，+ 随机扰动
                    float baseAngle = MathHelper.TwoPi / 12 * Main.rand.Next(12);
                    float angle = baseAngle + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 vel = forwardDir.RotatedBy(angle * 0.05f) * Main.rand.NextFloat(3f, 6f);

                    PointParticle point = new PointParticle(
                        HeadPosition,
                        vel,
                        false,
                        Main.rand.Next(12, 18),
                        1.1f + Main.rand.NextFloat(0.3f),
                        Main.rand.NextBool() ? Color.Yellow : Color.Orange
                    );
                    GeneralParticleHandler.SpawnParticle(point);
                }




            }







            // 检测松手
            Player player = Main.player[Projectile.owner];
            //if (!player.Calamity().rightClickListener)
            //if (!Owner.channel)
            if (!player.Calamity().mouseRight)
            {
                if (chargeTimer <= 300)
                {
                    Projectile.Kill(); // ❌ 蓄力不足，直接消失
                    return;
                }
                Projectile.friendly = true;
                Projectile.netUpdate = true;
                Projectile.timeLeft = 900;
                Projectile.extraUpdates = 3;
                Projectile.penetrate = 1;
                CurrentState = BehaviorState.Dash;
            }


            if (chargeTimer <= 300 && !hasReleasedPulse)
            {
                // ================== 数学化蓄力特效：向日葵 + 对数螺线 + 刻度短弧 ==================
                // 枪头空间锚点（不抖动，稳定）
                Vector2 head = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;

                // 归一化的朝前方向
                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                // 0..1 的蓄力进度（300 帧满）
                float p = MathHelper.Clamp(chargeTimer / 300f, 0f, 1f);
                // 平滑插值（smoothstep），更“顺”
                float ease = p * p * (3f - 2f * p);
                // 随时间自转相位（统一相位，避免乱）
                float phase = Main.GlobalTimeWrappedHourly;

                // ================== 1) 向日葵分布（黄金角）——稳定聚拢的“能量花盘” ==================
                // 公式：θ = n * goldenAngle，r ~ sqrt(n/N)，N 随蓄力逐渐增大
                int seeds = (int)MathHelper.Lerp(5f, 12f, ease);             // 点数从少 → 多
                float golden = MathHelper.ToRadians(137.50776f);              // 黄金角
                float baseR = MathHelper.Lerp(6f, 28f, ease);                 // 花盘半径渐增
                for (int n = 0; n < seeds; n++)
                {
                    float ang = n * golden + phase * (0.8f + 0.4f * ease);    // 随时间慢转
                    float rad = baseR * MathF.Sqrt((n + 1f) / (seeds + 1f));  // sqrt 分布更匀
                    Vector2 pos = head + ang.ToRotationVector2() * rad;

                    // 切向 + 轻微外扩，体现“旋着聚气”
                    Vector2 dir = (pos - head).SafeNormalize(Vector2.UnitX);
                    Vector2 tan = dir.RotatedBy(MathHelper.PiOver2);

                    int dustType = (n % 2 == 0) ? DustID.Electric : DustID.UltraBrightTorch;
                    Color c = Color.Lerp(Color.Orange, Color.Yellow, 0.5f + 0.5f * ease);

                    Dust d = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 150, c, 0.9f + 0.6f * ease);
                    d.noGravity = true;
                    d.fadeIn = 0.8f;
                    d.velocity = tan * (0.6f + 1.0f * ease) + dir * (0.2f + 0.6f * ease);
                }

                // ================== 2) 对数螺线臂（双臂反向）——有“吸附/缠绕”的动态感 ==================
                // 极坐标：r = r0 * e^(b t)，θ = θ0 + a t，t ∈ [0,1]
                for (int arm = 0; arm < 2; arm++)
                {
                    float sign = (arm == 0) ? 1f : -1f;
                    float theta0 = forward.ToRotation() + sign * (MathHelper.PiOver4 * (0.6f + 0.4f * ease));
                    int steps = 8; // 段数适中
                    for (int k = 0; k <= steps; k++)
                    {
                        float t = k / (float)steps;
                        // 角度和半径随 t 增长
                        float theta = theta0 + sign * (2.6f * t + 1.2f * ease * t);
                        float rr = MathF.Exp(0.9f * t) * (8f + 28f * ease);
                        Vector2 pos = head + new Vector2(MathF.Cos(theta), MathF.Sin(theta)) * rr;

                        int type = (k % 2 == 0) ? DustID.GemDiamond : DustID.YellowTorch;
                        Color c = Color.Lerp(Color.Orange, Color.Yellow, 0.3f + 0.7f * t);
                        float sc = (0.8f + 0.9f * t) * (0.8f + 0.4f * ease);

                        Dust d = Dust.NewDustPerfect(pos, type, Vector2.Zero, 140, c, sc);
                        d.noGravity = true;

                        Vector2 dir = (pos - head).SafeNormalize(Vector2.UnitX);
                        d.velocity = dir * (1.0f + 2.0f * t) + dir.RotatedBy(sign * MathHelper.PiOver2) * (0.25f + 0.65f * ease);
                    }
                }

                // ================== 3) 刻度短弧（几何刻度感）——“读数/灌能”的视觉语言 ==================
                int ticks = 6;
                int seg = 4;
                float arcR = MathHelper.Lerp(20f, 50f, ease);
                for (int m = 0; m < ticks; m++)
                {
                    float ang0 = forward.ToRotation() + MathHelper.TwoPi * m / ticks + phase * (0.4f + 0.2f * ease);
                    for (int t = 0; t < seg; t++)
                    {
                        float a = ang0 + (t - seg / 2f) * 0.07f;        // 很短的一小段弧
                        Vector2 dir = a.ToRotationVector2();
                        Vector2 pos = head + dir * (arcR + t);

                        Dust d = Dust.NewDustPerfect(pos, DustID.UltraBrightTorch, Vector2.Zero, 160, Color.Lerp(Color.White, Color.Orange, 0.4f), 0.8f + 0.4f * ease);
                        d.noGravity = true;
                        d.velocity = dir * (0.5f + 0.8f * ease) + dir.RotatedBy(MathHelper.PiOver2) * (0.45f + 0.3f * ease);
                    }
                }



                // ================== 4) 轻量粒子点缀（不喧宾夺主） ==================
                if ((chargeTimer % 3) == 0) // 频率很低
                {
                    // 微型能量核 Bloom
                    GenericBloom bloom = new GenericBloom(
                        head,
                        Vector2.Zero,
                        Color.Lerp(Color.Gold, Color.LightYellow, 0.5f + 0.5f * ease),
                        MathHelper.Lerp(1.2f, 1.9f, ease),
                        6
                    );
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
                if ((chargeTimer % 5) == 0)
                {
                    // 细小亮点沿环轻轻旋
                    float a = forward.ToRotation() + phase * 0.8f;
                    Vector2 pos = head + a.ToRotationVector2() * MathHelper.Lerp(18f, 36f, ease);
                    GlowOrbParticle orb = new GlowOrbParticle(
                        pos, Vector2.Zero, false,
                        8 + (int)(4 * ease),
                        MathHelper.Lerp(0.7f, 1.0f, ease),
                        Color.LightYellow,
                        true, false, true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }





        }


        // 在类里新建两个字段
        private int dashPhaseTimer = 0;
        private bool enteredFinalCharge = false;


        private void DoBehavior_Dash()
        {

            // 重置速度的逻辑
            {
                float initialSpeed = 35f; // 设定初始速度值，可根据需求替换具体值
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;
            }

            // 仅在冲刺阶段添加粒子特效
            if (Main.rand.NextFloat() < 0.6f) // 控制粒子生成的概率
            {
                // 计算枪头位置
                Vector2 gunHeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 5f;

                // 在枪头周围 1×16 半径的矩形区域内均匀分布
                Vector2 particlePos = gunHeadPosition + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1f, 1f);

                // **修正粒子的运动方向，使其与弹幕方向一致**
                Vector2 particleVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;

                // 创建金黄色线性粒子
                Particle trail = new SparkParticle(
                    particlePos, // 粒子初始位置
                    particleVelocity, // **粒子运动方向修正**
                    false, // ❌ 不受重力影响
                    60, // 生命周期 60 帧
                    1.0f, // 缩放大小
                    Color.Gold // 颜色改为金黄色
                );

                // 生成粒子
                GeneralParticleHandler.SpawnParticle(trail);
            }



            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            NPC target = GetClosestChaseableNPC(2400f);
            dashPhaseTimer++;

            if (!enteredFinalCharge)
            {
                // ========== 第一子阶段：软追踪、速度逐渐衰减 ==========
                Projectile.velocity *= 0.95f;

                if (target != null)
                {
                    Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    // 慢慢旋向目标
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * Projectile.velocity.Length(), 0.05f);
                }

                // 持续 30 帧后进入第二子阶段
                if (dashPhaseTimer >= 30)
                {
                    enteredFinalCharge = true;
                    dashPhaseTimer = 0;

                    if (target != null)
                    {
                        // ========== 第二子阶段：一次性高速冲刺 ==========
                        Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        Projectile.velocity = dir * 45f; // 直接锁定敌人高速突刺
                    }
                    else
                    {
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 45f;
                    }
                }
            }
            else
            {
                // 第二子阶段：不再调整角度，保持直线突刺
            }



        }
        // === 工具函数：寻找可追踪的最近 NPC（可穿墙可选）===
        private NPC GetClosestChaseableNPC(float maxDetectDistance, bool requireLineOfSight = false)
        {
            NPC closest = null;
            float sqrBest = maxDetectDistance * maxDetectDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active) continue;
                if (!npc.CanBeChasedBy(Projectile, false)) continue; // 可被追踪（不无敌/不友好）

                float sqrDist = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (sqrDist >= sqrBest) continue;

                // 可选：要求无障碍视线（不想受地形影响就传 false）
                if (requireLineOfSight && !Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    continue;

                sqrBest = sqrDist;
                closest = npc;
            }
            return closest;
        }


        public override void OnKill(int timeLeft)
        {

            
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 explosionPosition = target.Center;

            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/380mmExploded") with { Volume = 0.7f, Pitch = 0.0f }, Projectile.Center);

            //Particle bolt = new CustomPulse(
            //    Projectile.Center, // 粒子生成位置，与弹幕中心重合
            //    Vector2.Zero, // 粒子静止不动
            //    Color.LightYellow, // 设定冲击波颜色
            //    "CalamityThrowingSpear/texture/IonizingRadiation", // 设定粒子使用的贴图【可以随意修改】
            //    Vector2.One * (Projectile.ai[0] == 1 ? 1.5f : 1f), // 冲击波的椭圆变形比例
            //    Main.rand.NextFloat(-10f, 10f), // 设定旋转角度
            //    0.03f, // 初始缩放大小
            //    0.16f, // 最终缩放大小（逐渐变大）【也可以是逐渐变小】
            //    16 // 粒子的存活时间（16 帧）
            //);
            //// 生成自定义冲击波粒子
            //GeneralParticleHandler.SpawnParticle(bolt);


            // 生成爆炸粒子
            Particle explosion = new DetailedExplosion(
                explosionPosition,
                Vector2.Zero,
                Color.OrangeRed * 0.9f,
                Vector2.One,
                Main.rand.NextFloat(-5, 5),
                0.1f * 2.5f, // 修改原始大小
                0.28f * 2.5f, // 修改最终大小
                10
            );
            GeneralParticleHandler.SpawnParticle(explosion);


            // 生成爆炸弹幕
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                explosionPosition,
                Vector2.Zero,
                ModContent.ProjectileType<SunsetASunsetRightEXP>(),
                damageDone,
                Projectile.knockBack,
                Projectile.owner
            );

            // 生成太阳爆炸特效
            for (int i = 0; i < 12; i++) // 太阳的 12 条光线
            {
                float rotation = MathHelper.TwoPi / 12 * i; // 计算每条光线的角度
                Vector2 offset = rotation.ToRotationVector2() * 50f; // 设置光线长度
                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    ParticleOrchestraType.Excalibur,
                    new ParticleOrchestraSettings { PositionInWorld = explosionPosition + offset },
                    Projectile.owner
                );
            }         

            target.AddBuff(ModContent.BuffType<SunsetASunsetEDebuff>(), 300); // 300 帧 = 5 秒

            SoundEngine.PlaySound(SoundID.Item34, Projectile.position);
        }
    }
}