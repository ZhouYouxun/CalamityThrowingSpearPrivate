using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
{
    public class SunEssenceJavFeather : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取羽毛贴图
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation;
            SpriteEffects effects = SpriteEffects.None;

            // === 🚩 1️⃣ 绘制拖尾（原 Calamity 拖尾）===
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, (int)1.5f);

            // === 🚩 2️⃣ 外层光脉动绘制 ===
            // 计算呼吸脉动缩放（1.6x ~ 2.0x）
            float pulsate = 1.6f + 0.4f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f); // 呼吸周期

            // 设置蓝绿色淡光色
            Color glowColor = Color.Cyan * 0.3f;

            // 多层叠加轻微位移绘制，形成柔和蓝光晕
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 2f;
                Main.spriteBatch.Draw(
                    texture,
                    drawPosition + offset,
                    null,
                    glowColor,
                    rotation,
                    origin,
                    pulsate,
                    effects,
                    0f
                );
            }

            // === 🚩 3️⃣ 绘制本体（1.5x 大小）===
            Main.spriteBatch.Draw(
                texture,
                drawPosition,
                null,
                lightColor,
                rotation,
                origin,
                1.5f,
                effects,
                0f
            );

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 10;
            Projectile.DamageType = DamageClass.Melee;
            //Projectile.aiStyle = ProjAIStyleID.Nail;
            //AIType = ProjectileID.NailFriendly;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.aiStyle = -1;      // 禁止使用 Nail AI，完全跑自定义逻辑
        }
        // 在类里新增一个字段
        private int flyDuration;
        // 在类里新增一个字段来存储锁定的角度
        private float lockedRotation = 0f;

        // 在 OnSpawn 里生成随机值
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            flyDuration = Main.rand.Next(5, 26); // 5~25 随机
        }
        // 在类里增加变量
        private Projectile? parentProj;
        private int orbitTimer = 0;
        private const int orbitTimeMax = 120; // 转两圈，约120帧
        private const float orbitRadius = 96f; // 公转半径
        private const float orbitAngularSpeed = MathHelper.TwoPi / 60f; // 每60帧一圈



        public override void AI()
        {
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);

            Player player = Main.player[Projectile.owner];

            ref float state = ref Projectile.ai[0];  // 状态阶段
            ref float timer = ref Projectile.ai[1];  // 阶段内计时器

            switch ((int)state)
            {
                case 0: // 🌀 阶段 0：出生后先轻微弯曲飞行
                    timer++;
                    if (timer == 1)
                    {
                        // 找最近敌人
                        NPC fakeTarget = Projectile.Center.ClosestNPCAt(900f);
                        Vector2 dir = fakeTarget != null
                            ? (fakeTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY)
                            : Vector2.UnitX;

                        // 故意往反方向 ±40° 偏移（柔和一点，不突兀）
                        float curveDir = Main.rand.NextBool() ? 1f : -1f;
                        float curveAngle = dir.ToRotation() + curveDir * MathHelper.ToRadians(40f);
                        Projectile.velocity = curveAngle.ToRotationVector2() * 6f;
                    }
                    else
                    {
                        // 限制每帧最大转动角度
                        float currentRot = Projectile.velocity.ToRotation();
                        float targetRot = Projectile.velocity.ToRotation(); // 维持当前飞行方向
                        float newRot = currentRot.AngleTowards(targetRot, 0.01f);

                        Projectile.velocity = newRot.ToRotationVector2() * Projectile.velocity.Length();
                    }

                    if (timer > 95f) // 弯曲约 X 帧后进入追踪
                    {
                        state = 1f;
                        timer = 0f;
                    }

                    // rotation 始终绑定速度方向
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * 2;
                    break;

                case 1: // 🎯 阶段 1：有限角速度追踪
                    timer++;
                    NPC target = Projectile.Center.ClosestNPCAt(900f);
                    if (target != null && target.active && !target.friendly && !target.dontTakeDamage)
                    {
                        float maxTurnSpeed = MathHelper.ToRadians(4f); // 每帧最多转 4°
                        Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        float desiredRot = desiredDir.ToRotation();
                        float newRot = Projectile.velocity.ToRotation().AngleTowards(desiredRot, maxTurnSpeed);

                        // 保持流畅的速度，不要过高
                        Projectile.velocity = newRot.ToRotationVector2() * 10f;
                        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * 2;
                    }
                    else
                    {
                        Projectile.Kill();
                    }
                    break;
            }

            // 💡 始终保留视觉效果
            Lighting.AddLight(Projectile.Center, 0.2f, 0.9f, 0.3f); // 绿色光

            // 飞行中的特效（不再因为状态而屏蔽）
            if (Projectile.tileCollide)
            {
                // === 🚩 1️⃣ 高密度 Dust 蓝绿色流动 ===
                int dustCount = 2;
                for (int i = 0; i < dustCount; i++)
                {
                    float spread = MathHelper.ToRadians(30f); // 扩散角度减小一点，更收敛
                    float angle = Main.rand.NextFloat(-spread, spread);
                    Vector2 velocity = forward.RotatedBy(angle) * Main.rand.NextFloat(3f, 7f); // 速度减弱
                    int type = Main.rand.NextFloat() < 0.7f ? DustID.BlueTorch : DustID.GreenTorch;

                    Dust d = Dust.NewDustDirect(
                        Projectile.Center,
                        0, 0,
                        type,
                        velocity.X,
                        velocity.Y,
                        0,
                        default,
                        Main.rand.NextFloat(1.0f, 2.0f)
                    );
                    d.noGravity = true;
                }

                // === 🚩 2️⃣ DirectionalPulseRing 蓝绿色脉冲波 ===
                if (Main.GameUpdateCount % 4 == 0) // 稍微降低频率
                {
                    Particle pulse = new DirectionalPulseRing(
                        Projectile.Center + forward * 8f,
                        forward * 1.5f,
                        Color.Lerp(Color.DeepSkyBlue, Color.Cyan, 0.5f),
                        new Vector2(0.7f, 1.5f),
                        Projectile.rotation,
                        0.12f,
                        0.02f,
                        8
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                }

                // === 🚩 3️⃣ SparkParticle（外侧随机发散）===
                if (Main.rand.NextBool(8)) // 概率调低
                {
                    Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                    Vector2 velocity = forward.RotatedByRandom(MathHelper.ToRadians(12f)) * Main.rand.NextFloat(3f, 6f);

                    Particle spark = new SparkParticle(
                        Projectile.Center + offset,
                        velocity,
                        false,
                        10,
                        Main.rand.NextFloat(0.7f, 0.9f),
                        Color.Lerp(Color.Aqua, Color.Cyan, Main.rand.NextFloat(0.3f, 0.7f))
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // === 🚩 4️⃣ AltSparkParticle（贴近轨迹，柔和光点）===
                if (Main.rand.NextBool(6))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(1.5f, 1.5f);
                    Vector2 velocity = forward * 0.02f + Main.rand.NextVector2Circular(0.08f, 0.08f);

                    AltSparkParticle altSpark = new AltSparkParticle(
                        Projectile.Center + offset,
                        velocity,
                        false,
                        14,
                        1.0f,
                        Color.Cyan * 0.25f
                    );
                    GeneralParticleHandler.SpawnParticle(altSpark);
                }
            }
        }



        // 工具函数：找到最近的 SunEssenceJavPROJ
        private Projectile? FindClosestParent()
        {
            Projectile? result = null;
            float minDist = 2000f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<SunEssenceJavPROJ>())
                {
                    float dist = Vector2.Distance(Projectile.Center, proj.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        result = proj;
                    }
                }
            }
            return result;
        }

        // 工具函数：找到父弹幕最近的敌人
        private NPC? FindParentClosestNPC(Projectile parent)
        {
            NPC? result = null;
            float minDist = 900f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(parent.Center, npc.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        result = npc;
                    }
                }
            }
            return result;
        }

        // 示例：弹性曲线函数（EaseInOutElastic），可调整为你喜欢的速度感
        private float EaseInOutElastic(float t)
        {
            if (t < 0.5f)
                return 0.5f * (float)(Math.Sin(13 * Math.PI / 2 * (2 * t)) * Math.Pow(2, 10 * ((2 * t) - 1)));
            else
                return 0.5f * ((float)(Math.Sin(-13 * Math.PI / 2 * ((2 * t - 1) + 1)) * Math.Pow(2, -10 * (2 * t - 1))) + 2);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            for (int i = 0; i < 15; i++)
            {
                int greenDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1.2f);
                Main.dust[greenDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[greenDust].scale = 0.5f;
                    Main.dust[greenDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 30; j++)
            {
                int greenDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1.7f);
                Main.dust[greenDust2].noGravity = true;
                Main.dust[greenDust2].velocity *= 5f;
                greenDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1f);
                Main.dust[greenDust2].velocity *= 2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 300); // 原版的破晓效果
        }

    }
}
