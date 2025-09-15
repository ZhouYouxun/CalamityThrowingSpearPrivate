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
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
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
                Projectile.timeLeft = 300;
                Projectile.penetrate = 2;
                CurrentState = BehaviorState.Dash;
            }


            if (chargeTimer == 300 && !hasReleasedPulse)
            {
                hasReleasedPulse = true;

                Vector2 HeadPosition = Projectile.Center
                    + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;

                // 1️⃣ 爆心闪光
                GenericBloom bloom = new GenericBloom(
                    HeadPosition,
                    Vector2.Zero,
                    Color.Gold,
                    2.0f,
                    25
                );
                GeneralParticleHandler.SpawnParticle(bloom);

                // 2️⃣ 冲击波环（内圈 + 外圈）
                Particle innerRing = new DirectionalPulseRing(
                    HeadPosition,
                    Vector2.Zero,
                    Color.Orange,
                    new Vector2(1f, 1f),
                    0f,
                    0.1f,   // 初始 scale
                    1.2f,   // 最终 scale (约 120px)
                    20
                );
                GeneralParticleHandler.SpawnParticle(innerRing);

                Particle outerRing = new DirectionalPulseRing(
                    HeadPosition,
                    Vector2.Zero,
                    Color.Yellow,
                    new Vector2(1f, 1f),
                    0f,
                    0.2f,
                    2.2f,   // 扩散到更大 (~220px)
                    35
                );
                GeneralParticleHandler.SpawnParticle(outerRing);

                // 3️⃣ 扩散烟雾
                for (int i = 0; i < 40; i++)
                {
                    Vector2 dir = Main.rand.NextVector2Unit();
                    Particle smoke = new HeavySmokeParticle(
                        HeadPosition + dir * 8f,
                        dir * Main.rand.NextFloat(3f, 6f),
                        Color.Lerp(Color.Gray, Color.Orange, 0.4f),
                        Main.rand.Next(35, 50),
                        Main.rand.NextFloat(1.0f, 1.8f),
                        0.8f,
                        Main.rand.NextFloat(-1f, 1f),
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }

                // 4️⃣ 爆裂火花
                for (int i = 0; i < 24; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 8f);
                    PointParticle spark = new PointParticle(
                        HeadPosition,
                        vel,
                        false,
                        20,
                        1.3f,
                        Main.rand.NextBool() ? Color.Orange : Color.Yellow
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 5️⃣ Dust 环绕
                int dustCount = 32;
                float radius = 32f;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi / dustCount * i;
                    Vector2 vel = angle.ToRotationVector2() * 4f;
                    Dust d = Dust.NewDustPerfect(
                        HeadPosition,
                        DustID.YellowTorch,
                        vel,
                        150,
                        Color.Gold,
                        1.3f
                    );
                    d.noGravity = true;
                }

                // 💥 播放强力音效
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.0f, Pitch = -0.2f }, HeadPosition);
            }





        }

        private void DoBehavior_Dash()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            // 重置速度的逻辑
            {
                float initialSpeed = 15f; // 设定初始速度值，可根据需求替换具体值
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

            // 采用 PlagueTaintedDrone 同款 的追踪方式
            NPC target = Projectile.FindTargetWithinRange(1600f); // 1600 像素范围内寻找目标
            if (Projectile.timeLeft < 415 && target != null) // 飞行 65 帧后开始追踪
            {
                float trackingSpeed = 12f;  // 追踪速度
                float maxTurnRate = 15f;    // 最大旋转角度

                // 使用平滑的逐帧调整方式
                Projectile.velocity = Projectile.SuperhomeTowardsTarget(target, trackingSpeed, maxTurnRate);
            }
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