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
using Terraria.Graphics.CameraModifiers;
using Microsoft.Xna.Framework.Graphics; // PunchCameraModifier


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
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            // 计算蓄力比例：0 → 1
            float chargeProgress = 0f;
            if (CurrentState == BehaviorState.Aim)
            {
                chargeProgress = MathHelper.Clamp(chargeTimer / 300f, 0f, 1f);
            }
            else if (CurrentState == BehaviorState.Dash)
            {
                // 冲刺期间固定满值
                chargeProgress = 1f;
            }

            // 🌟 描边宽度，从 0px → 6px
            float outlineWidth = MathHelper.Lerp(0f, 6f, chargeProgress);

            // 🌟 描边颜色，高亮黄（类似太阳/斩击风格）
            Color outlineColor = Color.Lerp(Color.Gold, Color.Yellow, 0.5f) * chargeProgress;
            outlineColor.A = 0;

            // ===== 绘制描边（8 方向循环）=====
            if (outlineWidth > 0f)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * outlineWidth;

                    Main.spriteBatch.Draw(
                        tex,
                        drawPos + offset,
                        null,
                        outlineColor,
                        Projectile.rotation,
                        origin,
                        Projectile.scale,
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            // ===== 绘制本体 =====
            Main.spriteBatch.Draw(
                tex,
                drawPos,
                null,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );

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


                {

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

                    // ========== Dust（前向能量雾：速度×3 + 环带数学结构） ==========
                    if (Main.rand.NextBool(2))
                    {
                        // 在前方构造一条带有正弦起伏的“能量环带”
                        float ringPhase = time * 0.9f;
                        float ringAngle = Main.rand.NextFloat(-(MathHelper.Pi / 3f), (MathHelper.Pi / 3f));
                        float ringRadius = 22f + 8f * (float)Math.Sin(ringPhase + ringAngle * 2f); // 半规则起伏半径

                        Vector2 spawnOffset = forwardDir.RotatedBy(MathHelper.PiOver2 + ringAngle) * ringRadius;

                        // 前向速度至少是原来的三倍（2~4 → 6~12），并叠加一层时间相位扰动
                        Vector2 velocity = forwardDir.RotatedBy((float)Math.Sin(time * 1.3f) * 0.1f)
                                           * Main.rand.NextFloat(6f, 12f);

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

                    // ========== SparkParticle（前向拖尾：速度×3 + 正弦调制） ==========
                    if (Main.rand.NextBool(1))
                    {
                        // 在 forwardDir 附近做更大的角度摆动，同时让速度随正弦变化
                        float sparkPhase = time * 1.4f + Projectile.whoAmI * 0.3f;
                        float sparkAngleOffset = Main.rand.NextFloat(-0.25f, 0.25f)
                                                 + 0.12f * (float)Math.Sin(sparkPhase);

                        float speedLerp = (float)Math.Sin(Main.rand.NextFloat() * MathHelper.Pi); // 0→1→0
                        float speed = MathHelper.Lerp(7.5f, 13.5f, speedLerp); // 原 2.5~4.5 ×3

                        Vector2 vel = forwardDir.RotatedBy(sparkAngleOffset) * speed;

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

                    // ========== PointParticle（尖锐能量碎片：速度×3 + 半规则扇形） ==========
                    if (Main.rand.NextBool(1))
                    {
                        // 以12等分为基准，在前方构造一个“半规则扇形射线阵”
                        float baseAngle = MathHelper.TwoPi / 12f * Main.rand.Next(12); // 12 等分
                        float localT = Main.rand.NextFloat();                           // 0~1，用来做非线性分布
                        float angle = baseAngle + (localT - 0.5f) * 0.6f;               // 每条射线有轻微扩散

                        // 速度从 9~18（原 3~6 ×3），并用正弦做一层权重，让中段更强
                        float speedFactor = (float)Math.Sin(localT * MathHelper.Pi) * 0.5f + 0.5f; // 0.5~1
                        float speed = MathHelper.Lerp(9f, 18f, speedFactor);

                        Vector2 vel = forwardDir.RotatedBy(angle * 0.08f) * speed;

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


















            }




            // === 确保 2号魔法阵存在并绑定自己 ===
            if (Projectile.localAI[0] == 0) // 避免重复生成
            {
                int magic = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<SunsetASunsetRightEXPMagic2>(),
                    0, 0f, Projectile.owner,
                    Projectile.whoAmI // 把自己ID传进去，供绑定用
                );
                if (magic.WithinBounds(Main.maxProjectiles))
                    Projectile.localAI[0] = magic + 1; // 存储ID+1，避免默认0
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

                    int dustType = (n % 2 == 0) ? DustID.Lava : DustID.Lava;
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

                        int type = (k % 2 == 0) ? DustID.Firefly : DustID.YellowTorch;
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

                        Dust d = Dust.NewDustPerfect(pos, DustID.Lava, Vector2.Zero, 160, Color.Lerp(Color.White, Color.Orange, 0.4f), 0.8f + 0.4f * ease);
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

            // === 删除绑定的2号魔法阵 ===
            if (Projectile.localAI[0] > 0)
            {
                int magicID = (int)Projectile.localAI[0] - 1;
                if (magicID.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[magicID].Kill();
                }
                Projectile.localAI[0] = 0;
            }


            // 飞行期间粒子特效：狂野放射
            //if (Main.rand.NextFloat() < 0.8f) // 提高整体出现概率
            for (int i = 0; i < 7; i++)
            {
                Vector2 headPos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 5f;
                Vector2 offset = Projectile.velocity.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(-1f, 1f);
                Vector2 basePos = headPos + offset;
                Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero);

                float choice = Main.rand.NextFloat();

                if (choice < 0.3f)
                {
                    // 🔥 SparkParticle（锐利拖尾）
                    Particle spark = new SparkParticle(
                        basePos,
                        vel * 2f,
                        false,
                        50,
                        1.1f,
                        Color.Lerp(Color.Orange, Color.Gold, 0.5f)
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                else if (choice < 0.6f)
                {
                    // ✨ GlowOrbParticle（放射性光球）
                    GlowOrbParticle orb = new GlowOrbParticle(
                        basePos,
                        vel.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.5f, 2f),
                        false,
                        20,
                        Main.rand.NextFloat(0.8f, 1.2f),
                        Color.Lerp(Color.Gold, Color.White, 0.7f),
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
                else if (choice < 0.8f)
                {
                    // ◼️ SquareParticle（科技碎片）
                    SquareParticle sq = new SquareParticle(
                        basePos,
                        vel.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 5f),
                        false,
                        25,
                        Main.rand.NextFloat(1.3f, 1.8f),
                        Color.Lerp(Color.OrangeRed, Color.Yellow, 0.6f)
                    );
                    GeneralParticleHandler.SpawnParticle(sq);
                }
                else
                {
                    // 🌌 Dust（无序星屑）
                    int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.SolarFlare;
                    Dust d = Dust.NewDustPerfect(
                        basePos,
                        dustType,
                        vel.RotatedByRandom(0.7f) * Main.rand.NextFloat(1f, 3f),
                        150,
                        Color.Lerp(Color.Orange, Color.Gold, 0.8f),
                        1.2f
                    );
                    d.noGravity = true;
                    d.fadeIn = 1.1f;
                }
            }





            // ========== 🔧 参数 ==========
            int teleportDelayFrames = 10;     // 直线飞行多久后传送（按“帧”计算）
            float straightSpeed = 30f;    // 直线飞行速度
            float postTeleportUpSpeed = 30f;    // 传送后向上突刺速度
            float searchRadius = 240000f;  // 寻敌半径
            float teleportOffsetX = 0f;     // 传送到敌人正下方时的横向偏移
            float teleportOffsetY = 20f;    // 传送到敌人正下方时的纵向偏移
            float shakePower = 8f;     // 屏幕震动强度（会按距离衰减）

            // ========== 朝向 & 碰撞 ==========
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            // ========== 计时：只在“真实帧”递增 ==========
            // extraUpdates > 0 时，一个游戏帧会多次调用 AI；
            // 只在 numUpdates == 0 的那次算“1帧”，从而保证 teleportDelayFrames 真正按帧数生效。
            if (Projectile.numUpdates == 0)
                dashPhaseTimer++;

            // ========== 直线飞行，不追踪（直到边界帧） ==========
            Vector2 dir = Projectile.velocity.LengthSquared() > 0.001f
                ? Vector2.Normalize(Projectile.velocity)
                : -Vector2.UnitY; // 兜底向上

            if (dashPhaseTimer < teleportDelayFrames)
            {
                Projectile.velocity = dir * straightSpeed;

                // （可选）你的“狂野放射拖尾”可以继续放在这里
                // EmitFlightTrailFancy(); // 就是你那段 Spark/Orb/Square/Dust 组合

                return; // 直线阶段直接返回（注意：我们已经在上面先递增了计时器）
            }

            // ========== 边界帧：先放特效，再传送 ==========
            if (dashPhaseTimer == teleportDelayFrames)
            {
                // ——1) 记录传送前位置，播放恒星爆炸风格的法阵（保留在原地）——
                Vector2 preTeleportPos = Projectile.Center;
                EmitTeleportStarburst(preTeleportPos, shakePower);

                // ——2) 寻找最近可追踪目标——
                NPC target = GetClosestChaseableNPC(searchRadius);



                if (target != null && target.active && !target.friendly && !target.dontTakeDamage)
                {
                    Rectangle r = target.getRect();

                    // ❌ 不再用固定的 +20px，而是用目标高度来计算偏移
                    float offsetMultiplier = 4f; // 出现在敌人高度的 4 倍下方
                    float spawnDistanceBelow = r.Height * offsetMultiplier;

                    Vector2 teleportPos = new Vector2(r.Center.X, r.Bottom + spawnDistanceBelow);
                    Projectile.Center = teleportPos;
                }

                // 传送后设定高速向上突刺
                Projectile.velocity = -Vector2.UnitY * postTeleportUpSpeed;




                // ——5) 同步一次（联机更稳）——
                Projectile.netUpdate = true;
            }

            // （剩余帧：保持上冲，不再改角度/速度）



        }








        // 封装：传送前“恒星爆炸”法阵（与你切换武器时的特效不同）
        private void EmitTeleportStarburst(Vector2 pos, float shakePower)
        {
            // A. 五角星顶点的收缩脉冲环
            int points = 5;
            float R = 80f;
            for (int i = 0; i < points; i++)
            {
                float ang = MathHelper.TwoPi * i / points;
                Vector2 starPos = pos + ang.ToRotationVector2() * R;
                Particle ring = new DirectionalPulseRing(
                    starPos,
                    Vector2.Zero,
                    Color.Lerp(Color.OrangeRed, Color.Yellow, 0.5f),
                    Vector2.One,
                    12f,
                    0.18f,
                    3.5f,
                    16
                );
                GeneralParticleHandler.SpawnParticle(ring);
            }

            // B. 对数螺旋的高亮光点（“星爆喷涌”）
            for (int arm = 0; arm < 2; arm++)
            {
                float sign = arm == 0 ? 1f : -1f;
                for (int k = 0; k < 18; k++)
                {
                    float t = k / 18f;
                    float theta = sign * (t * 6.28f);
                    float rr = 6f * MathF.Exp(0.25f * t);
                    Vector2 p = pos + theta.ToRotationVector2() * rr * 15f;
                    GlowOrbParticle orb = new GlowOrbParticle(
                        p,
                        Vector2.Zero,
                        false,
                        18,
                        1.2f,
                        Color.Lerp(Color.Yellow, Color.White, 0.5f),
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }

            // C. 星尘喷射（环向爆散）
            for (int i = 0; i < 12; i++)
            {
                float ang = MathHelper.TwoPi * i / 12f;
                Vector2 dir = ang.ToRotationVector2();
                var sp = new SparkParticle(
                    pos,
                    dir * Main.rand.NextFloat(6f, 11f),
                    false,
                    Main.rand.Next(18, 26),
                    Main.rand.NextFloat(1.2f, 1.6f),
                    Color.Lerp(Color.OrangeRed, Color.Orange, 0.5f)
                );
                sp.Rotation = dir.ToRotation();
                GeneralParticleHandler.SpawnParticle(sp);
            }

            // D. 轻量屏幕震动 + 音效
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Vector2.Distance(pos, Main.LocalPlayer.Center), true);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
            SoundEngine.PlaySound(SoundID.Item74, pos);
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

            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/explosion-6801") with { Volume = 1.7f, Pitch = 0.0f }, Projectile.Center);

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
            int totalCrit = (int)Math.Round(Main.player[Projectile.owner].GetTotalCritChance(Projectile.DamageType));

            int exp = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                explosionPosition,
                Vector2.Zero,
                ModContent.ProjectileType<SunsetASunsetRightEXP>(),
                (int)(Projectile.damage * 1.4),
                Projectile.knockBack,
                Projectile.owner
            );
            if (exp.WithinBounds(Main.maxProjectiles))
                Main.projectile[exp].CritChance = totalCrit; // ✅ 让爆炸也能暴击


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

            // 屏幕震动
            float shakePower = 55f;
            float distanceFactor = Utils.GetLerpValue(
                1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            // ========== 调用外包：Sun Inferno 超级爆炸（黄红混合，数学+狂野） ==========
            CalamityThrowingSpear.CTSLightingBoltsSystem.Spawn_SunInfernoSuperExplosion(Projectile.Center, 1.0f);


            // 额外少许“向外伞状”强火花，补一点“爆裂”感
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            int rays = 10;
            float cone = MathHelper.ToRadians(42f);
            for (int i = 0; i < rays; i++)
            {
                float off = MathHelper.Lerp(-cone, cone, i / (float)(rays - 1));
                Vector2 dir = forward.RotatedBy(off);
                float speed = Main.rand.NextFloat(7f, 11.5f);

                var sp = new SparkParticle(
                    Projectile.Center,
                    dir * speed,
                    false,
                    Main.rand.Next(20, 28),
                    Main.rand.NextFloat(1.3f, 1.8f),
                    Color.Lerp(Color.Orange, Color.OrangeRed, 0.5f)
                );
                sp.Rotation = dir.ToRotation();
                GeneralParticleHandler.SpawnParticle(sp);
            }

            SoundEngine.PlaySound(SoundID.Item34, Projectile.position);
        }
    }
}